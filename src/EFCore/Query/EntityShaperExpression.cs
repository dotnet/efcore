// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class EntityShaperExpression : Expression, IPrintable
    {
        public EntityShaperExpression(IEntityType entityType, Expression valueBufferExpression, bool nullable)
        {
            EntityType = entityType;
            ValueBufferExpression = valueBufferExpression;
            IsNullable = nullable;
        }

        public virtual IEntityType EntityType { get; }
        public virtual Expression ValueBufferExpression { get; }
        public virtual bool IsNullable { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var valueBufferExpression = (ProjectionBindingExpression)visitor.Visit(ValueBufferExpression);

            return Update(valueBufferExpression);
        }

        public virtual EntityShaperExpression WithEntityType(IEntityType entityType)
            => entityType != EntityType
                ? new EntityShaperExpression(entityType, ValueBufferExpression, IsNullable)
                : this;

        public virtual EntityShaperExpression MarkAsNullable()
            => !IsNullable
                ? new EntityShaperExpression(EntityType, ValueBufferExpression, true)
                : this;

        public virtual EntityShaperExpression Update(Expression valueBufferExpression)
            => valueBufferExpression != ValueBufferExpression
                ? new EntityShaperExpression(EntityType, valueBufferExpression, IsNullable)
                : this;

        public override Type Type => EntityType.ClrType;
        public override ExpressionType NodeType => ExpressionType.Extension;

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.AppendLine(nameof(EntityShaperExpression) + ": ");
            using (expressionPrinter.StringBuilder.Indent())
            {
                expressionPrinter.StringBuilder.AppendLine(EntityType);
                expressionPrinter.StringBuilder.AppendLine(nameof(ValueBufferExpression) + ": ");
                using (expressionPrinter.StringBuilder.Indent())
                {
                    expressionPrinter.Visit(ValueBufferExpression);
                    expressionPrinter.StringBuilder.AppendLine();
                }

                expressionPrinter.StringBuilder.Append(nameof(IsNullable) + ": ");
                expressionPrinter.StringBuilder.AppendLine(IsNullable);
            }
        }
    }
}
