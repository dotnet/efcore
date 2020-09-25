// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         An expression that represents creation of an entity instance for a relational provider in
    ///         <see cref="ShapedQueryExpression.ShaperExpression" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class RelationalEntityShaperExpression : EntityShaperExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="RelationalEntityShaperExpression" /> class.
        /// </summary>
        /// <param name="entityType"> The entity type to shape. </param>
        /// <param name="valueBufferExpression"> An expression of ValueBuffer to get values for properties of the entity. </param>
        /// <param name="nullable"> A bool value indicating whether this entity instance can be null. </param>
        public RelationalEntityShaperExpression([NotNull] IEntityType entityType, [NotNull] Expression valueBufferExpression, bool nullable)
            : base(entityType, valueBufferExpression, nullable, null)
        {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="RelationalEntityShaperExpression" /> class.
        /// </summary>
        /// <param name="entityType"> The entity type to shape. </param>
        /// <param name="valueBufferExpression"> An expression of ValueBuffer to get values for properties of the entity. </param>
        /// <param name="nullable"> Whether this entity instance can be null. </param>
        /// <param name="materializationCondition">
        ///     An expression of <see cref="Func{ValueBuffer, IEntityType}" /> to determine which entity type to
        ///     materialize.
        /// </param>
        protected RelationalEntityShaperExpression(
            [NotNull] IEntityType entityType,
            [NotNull] Expression valueBufferExpression,
            bool nullable,
            [CanBeNull] LambdaExpression materializationCondition)
            : base(entityType, valueBufferExpression, nullable, materializationCondition)
        {
        }

        /// <inheritdoc />
        protected override LambdaExpression GenerateMaterializationCondition(IEntityType entityType, bool nullable)
        {
            Check.NotNull(entityType, nameof(EntityType));

            LambdaExpression baseCondition;
            if (entityType.GetDiscriminatorProperty() == null
                && entityType.GetDirectlyDerivedTypes().Any())
            {
                // TPT
                var valueBufferParameter = Parameter(typeof(ValueBuffer));
                var discriminatorValueVariable = Variable(typeof(string), "discriminator");
                var expressions = new List<Expression>
                {
                    Assign(
                        discriminatorValueVariable,
                        valueBufferParameter.CreateValueBufferReadValueExpression(typeof(string), 0, null))
                };

                var derivedConcreteEntityTypes = entityType.GetDerivedTypes().Where(dt => !dt.IsAbstract()).ToArray();
                var switchCases = new SwitchCase[derivedConcreteEntityTypes.Length];
                for (var i = 0; i < derivedConcreteEntityTypes.Length; i++)
                {
                    var discriminatorValue = Constant(derivedConcreteEntityTypes[i].ShortName(), typeof(string));
                    switchCases[i] = SwitchCase(Constant(derivedConcreteEntityTypes[i], typeof(IEntityType)), discriminatorValue);
                }

                var defaultBlock = entityType.IsAbstract()
                    ? CreateUnableToDiscriminateExceptionExpression(entityType, discriminatorValueVariable)
                    : Constant(entityType, typeof(IEntityType));

                expressions.Add(Switch(discriminatorValueVariable, defaultBlock, switchCases));
                baseCondition = Lambda(Block(new[] { discriminatorValueVariable }, expressions), valueBufferParameter);
            }
            else
            {
                baseCondition = base.GenerateMaterializationCondition(entityType, nullable);
            }

            if (entityType.FindPrimaryKey() != null)
            {
                var table = entityType.GetViewOrTableMappings().FirstOrDefault()?.Table;
                if (table != null
                    && table.IsOptional(entityType))
                {
                    // Optional dependent
                    var body = baseCondition.Body;
                    var valueBufferParameter = baseCondition.Parameters[0];
                    Expression condition = null;
                    var requiredNonPkProperties = entityType.GetProperties().Where(p => !p.IsNullable && !p.IsPrimaryKey()).ToList();
                    if (requiredNonPkProperties.Count > 0)
                    {
                        condition = requiredNonPkProperties
                            .Select(
                                p => NotEqual(
                                    valueBufferParameter.CreateValueBufferReadValueExpression(typeof(object), p.GetIndex(), p),
                                    Constant(null)))
                            .Aggregate((a, b) => AndAlso(a, b));
                    }

                    var allNonSharedProperties = GetNonSharedProperties(table, entityType);
                    if (allNonSharedProperties.Count != 0
                        && allNonSharedProperties.All(p => p.IsNullable))
                    {
                        var allNonSharedNullableProperties = allNonSharedProperties.Where(p => p.IsNullable).ToList();
                        var atLeastOneNonNullValueInNullablePropertyCondition = allNonSharedNullableProperties
                            .Select(
                                p => NotEqual(
                                    valueBufferParameter.CreateValueBufferReadValueExpression(typeof(object), p.GetIndex(), p),
                                    Constant(null)))
                            .Aggregate((a, b) => OrElse(a, b));

                        condition = condition == null
                            ? atLeastOneNonNullValueInNullablePropertyCondition
                            : AndAlso(condition, atLeastOneNonNullValueInNullablePropertyCondition);
                    }

                    if (condition != null)
                    {
                        body = Condition(condition, body, Default(typeof(IEntityType)));
                    }

                    return Lambda(body, valueBufferParameter);
                }
            }

            return baseCondition;
        }

        /// <inheritdoc />
        public override EntityShaperExpression WithEntityType(IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType != EntityType
                ? new RelationalEntityShaperExpression(entityType, ValueBufferExpression, IsNullable)
                : this;
        }

        /// <inheritdoc />
        public override EntityShaperExpression MarkAsNullable()
            => !IsNullable
                // Marking nullable requires recomputation of Discriminator condition
                ? new RelationalEntityShaperExpression(EntityType, ValueBufferExpression, true)
                : this;

        /// <inheritdoc />
        public override EntityShaperExpression Update(Expression valueBufferExpression)
        {
            Check.NotNull(valueBufferExpression, nameof(valueBufferExpression));

            return valueBufferExpression != ValueBufferExpression
                ? new RelationalEntityShaperExpression(EntityType, valueBufferExpression, IsNullable, MaterializationCondition)
                : this;
        }

        private IReadOnlyList<IProperty> GetNonSharedProperties(ITableBase table, IEntityType entityType)
        {
            var nonSharedProperties = new List<IProperty>();
            var principalEntityTypes = new HashSet<IEntityType>();
            GetPrincipalEntityTypes(table, entityType, principalEntityTypes);
            foreach (var property in entityType.GetProperties())
            {
                if (property.IsPrimaryKey())
                {
                    continue;
                }

                var propertyMappings = table.FindColumn(property).PropertyMappings;
                if (propertyMappings.Count() > 1
                    && propertyMappings.Any(pm => principalEntityTypes.Contains(pm.TableMapping.EntityType)))
                {
                    continue;
                }

                nonSharedProperties.Add(property);
            }

            return nonSharedProperties;
        }

        private void GetPrincipalEntityTypes(ITableBase table, IEntityType entityType, HashSet<IEntityType> entityTypes)
        {
            foreach (var linkingFk in table.GetRowInternalForeignKeys(entityType))
            {
                entityTypes.Add(linkingFk.PrincipalEntityType);
                GetPrincipalEntityTypes(table, linkingFk.PrincipalEntityType, entityTypes);
            }
        }
    }
}
