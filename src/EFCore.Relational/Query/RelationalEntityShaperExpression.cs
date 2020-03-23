// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class RelationalEntityShaperExpression : EntityShaperExpression
    {
        public RelationalEntityShaperExpression([NotNull] IEntityType entityType, [NotNull] Expression valueBufferExpression, bool nullable)
            : base(entityType, valueBufferExpression, nullable, null)
        {
        }

        public RelationalEntityShaperExpression(
            [NotNull] IEntityType entityType,
            [NotNull] Expression valueBufferExpression,
            bool nullable,
            [CanBeNull] LambdaExpression materializationCondition)
            : base(entityType, valueBufferExpression, nullable, materializationCondition)
        {
        }

        protected override LambdaExpression GenerateMaterializationCondition(IEntityType entityType, bool nullable)
        {
            Check.NotNull(entityType, nameof(EntityType));

            var baseCondition = base.GenerateMaterializationCondition(entityType, nullable);

            if (entityType.FindPrimaryKey() != null)
            {
                var linkingFks = entityType.GetViewOrTableMappings().SingleOrDefault()?.Table.GetInternalForeignKeys(entityType);
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
                ? new RelationalEntityShaperExpression(EntityType, valueBufferExpression, IsNullable, MaterializationCondition)
                : this;
        }
    }
}
