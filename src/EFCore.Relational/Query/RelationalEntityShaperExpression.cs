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
    public class RelationalEntityShaperExpression : EntityShaperExpression
    {
        private static readonly MethodInfo _createUnableToDiscriminateException
               = typeof(RelationalEntityShaperExpression).GetTypeInfo()
                   .GetDeclaredMethod(nameof(CreateUnableToDiscriminateException));

        [UsedImplicitly]
        private static Exception CreateUnableToDiscriminateException(IEntityType entityType, object discriminator)
            => new InvalidOperationException(CoreStrings.UnableToDiscriminate(entityType.DisplayName(), discriminator?.ToString()));
        public RelationalEntityShaperExpression([NotNull] IEntityType entityType, [NotNull] Expression valueBufferExpression, bool nullable)
            : base(entityType, valueBufferExpression, nullable,
                  GenerateMaterializationCondition(Check.NotNull(entityType, nameof(entityType)), nullable))
        {
        }

        public RelationalEntityShaperExpression(
            [NotNull] IEntityType entityType,
            [NotNull] Expression valueBufferExpression,
            bool nullable,
            [NotNull] LambdaExpression discriminatorCondition)
            : base(entityType, valueBufferExpression, nullable,
                  discriminatorCondition ?? GenerateMaterializationCondition(Check.NotNull(entityType, nameof(entityType)), nullable))
        {
        }

        private static LambdaExpression GenerateMaterializationCondition(IEntityType entityType, bool nullable)
        {
            var valueBufferParameter = Parameter(typeof(ValueBuffer));

            var keyless = entityType.FindPrimaryKey() == null;
            var optionalDependent = false;
            if (!keyless)
            {
                var linkingFks = entityType.GetViewOrTableMappings().Single().Table.GetInternalForeignKeys(entityType);
                if (linkingFks != null
                    && linkingFks.Any())
                {
                    optionalDependent = true;
                }
            }

            Expression body;
            var concreteEntityTypes = entityType.GetConcreteDerivedTypesInclusive().ToArray();
            var discriminatorProperty = entityType.GetDiscriminatorProperty();
            if (discriminatorProperty != null)
            {
                var discriminatorValueVariable = Variable(discriminatorProperty.ClrType, "discriminator");
                var expressions = new List<Expression>
                {
                    Assign(
                        discriminatorValueVariable,
                        valueBufferParameter.CreateValueBufferReadValueExpression(
                            discriminatorProperty.ClrType, discriminatorProperty.GetIndex(), discriminatorProperty))
                };

                var switchCases = new SwitchCase[concreteEntityTypes.Length];
                for (var i = 0; i < concreteEntityTypes.Length; i++)
                {
                    var discriminatorValue = Constant(concreteEntityTypes[i].GetDiscriminatorValue(), discriminatorProperty.ClrType);
                    switchCases[i] = SwitchCase(Constant(concreteEntityTypes[i], typeof(IEntityType)), discriminatorValue);
                }

                var defaultBlock = Block(
                    Throw(Call(
                        _createUnableToDiscriminateException, Constant(entityType), Convert(discriminatorValueVariable, typeof(object)))),
                    Default(typeof(IEntityType)));

                expressions.Add(Switch(discriminatorValueVariable, defaultBlock, switchCases));
                body = Block(new[] { discriminatorValueVariable }, expressions);
            }
            else
            {
                body = Constant(concreteEntityTypes.Length == 1 ? concreteEntityTypes[0] : entityType, typeof(IEntityType));
            }

            if (optionalDependent)
            {
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
            }
            else if (keyless
                && nullable)
            {
                body = Condition(
                    entityType.GetProperties()
                        .Select(p => NotEqual(
                            valueBufferParameter.CreateValueBufferReadValueExpression(typeof(object), p.GetIndex(), p),
                            Constant(null)))
                        .Aggregate((a, b) => OrElse(a, b)),
                    body,
                    Default(typeof(IEntityType)));
            }

            return Lambda(body, valueBufferParameter);
        }

        public override EntityShaperExpression WithEntityType(IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType != EntityType
                ? new RelationalEntityShaperExpression(entityType, ValueBufferExpression, IsNullable)
                : this;
        }

        public override EntityShaperExpression MarkAsNullable()
            => !IsNullable
                // Marking nullable requires recomputation of Discriminator condition
                ? new RelationalEntityShaperExpression(EntityType, ValueBufferExpression, true)
                : this;

        public override EntityShaperExpression Update(Expression valueBufferExpression)
        {
            Check.NotNull(valueBufferExpression, nameof(valueBufferExpression));

            return valueBufferExpression != ValueBufferExpression
                ? new RelationalEntityShaperExpression(EntityType, valueBufferExpression, IsNullable, DiscriminatorCondition)
                : this;
        }
    }
}
