// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class EntityShaperExpression : Expression, IPrintableExpression
    {
        public EntityShaperExpression(
            [NotNull] IEntityType entityType,
            [NotNull] Expression valueBufferExpression,
            bool nullable)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(valueBufferExpression, nameof(valueBufferExpression));

            EntityType = entityType;
            ValueBufferExpression = valueBufferExpression;
            IsNullable = nullable;
        }

        public virtual IEntityType EntityType { get; }
        public virtual Expression ValueBufferExpression { get; }
        public virtual bool IsNullable { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var valueBufferExpression = visitor.Visit(ValueBufferExpression);

            return Update(valueBufferExpression);
        }

        public virtual EntityShaperExpression WithEntityType([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType != EntityType
                ? new EntityShaperExpression(entityType, ValueBufferExpression, IsNullable)
                : this;
        }

        public virtual EntityShaperExpression MarkAsNullable()
            => !IsNullable
                ? new EntityShaperExpression(EntityType, ValueBufferExpression, true)
                : this;

        public virtual EntityShaperExpression Update([NotNull] Expression valueBufferExpression)
        {
            Check.NotNull(valueBufferExpression, nameof(valueBufferExpression));

            return valueBufferExpression != ValueBufferExpression
                ? new EntityShaperExpression(EntityType, valueBufferExpression, IsNullable)
                : this;
        }

        public override Type Type => EntityType.ClrType;

        public sealed override ExpressionType NodeType => ExpressionType.Extension;

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.AppendLine(nameof(EntityShaperExpression) + ": ");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.AppendLine(EntityType);
                expressionPrinter.AppendLine(nameof(ValueBufferExpression) + ": ");
                using (expressionPrinter.Indent())
                {
                    expressionPrinter.Visit(ValueBufferExpression);
                    expressionPrinter.AppendLine();
                }

                expressionPrinter.Append(nameof(IsNullable) + ": ");
                expressionPrinter.AppendLine(IsNullable);
            }
        }
    }
}
