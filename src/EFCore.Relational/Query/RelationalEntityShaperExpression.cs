// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         An expression that represents creation of an entity instance for a relational provider in <see cref="ShapedQueryExpression.ShaperExpression"/>.
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
        /// <param name="materializationCondition"> An expression of <see cref="Func{ValueBuffer, IEntityType}"/> to determine which entity type to materialize. </param>
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

            var baseCondition = base.GenerateMaterializationCondition(entityType, nullable);

            if (entityType.FindPrimaryKey() != null)
            {
                var linkingFks = entityType.GetViewOrTableMappings().SingleOrDefault()?.Table.GetRowInternalForeignKeys(entityType);
                if (linkingFks != null
                    && linkingFks.Any())
                {
                    // Optional dependent
                    var body = baseCondition.Body;
                    var valueBufferParameter = baseCondition.Parameters[0];
                    var requiredNonPkProperties = entityType.GetProperties().Where(p => !p.IsNullable && !p.IsPrimaryKey()).ToList();
                    if (requiredNonPkProperties.Count > 0)
                    {
                        body = Condition(
                            requiredNonPkProperties
                                .Select(p => NotEqual(
                                    valueBufferParameter.CreateValueBufferReadValueExpression(typeof(object), p.GetIndex(), p),
                                    Constant(null)))
                                .Aggregate((a, b) => AndAlso(a, b)),
                            body,
                            Default(typeof(IEntityType)));
                    }
                    else
                    {
                        var allNonPkProperties = entityType.GetProperties().Where(p => !p.IsPrimaryKey()).ToList();
                        if (allNonPkProperties.Count > 0)
                        {
                            body = Condition(
                                allNonPkProperties
                                    .Select(p => NotEqual(
                                        valueBufferParameter.CreateValueBufferReadValueExpression(typeof(object), p.GetIndex(), p),
                                        Constant(null)))
                                    .Aggregate((a, b) => OrElse(a, b)),
                                body,
                                Default(typeof(IEntityType)));
                        }
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
    }
}
