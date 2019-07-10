// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public class EntityShaperExpression : Expression, IPrintable
    {
        public EntityShaperExpression(IEntityType entityType, Expression valueBufferExpression, bool nullable)
        {
            EntityType = entityType;
            ValueBufferExpression = valueBufferExpression;
            Nullable = nullable;
        }

        public IEntityType EntityType { get; }
        public Expression ValueBufferExpression { get; }
        public bool Nullable { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var valueBufferExpression = (ProjectionBindingExpression)visitor.Visit(ValueBufferExpression);

            return Update(valueBufferExpression);
        }

        public EntityShaperExpression WithEntityType(IEntityType entityType)
            => entityType != EntityType
                ? new EntityShaperExpression(entityType, ValueBufferExpression, Nullable)
                : this;

        public EntityShaperExpression MarkAsNullable()
            => !Nullable
                ? new EntityShaperExpression(EntityType, ValueBufferExpression, true)
                : this;

        public EntityShaperExpression Update(Expression valueBufferExpression)
            => valueBufferExpression != ValueBufferExpression
                ? new EntityShaperExpression(EntityType, valueBufferExpression, Nullable)
                : this;

        public override Type Type => EntityType.ClrType;
        public override ExpressionType NodeType => ExpressionType.Extension;

        public void Print(ExpressionPrinter expressionPrinter)
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

                expressionPrinter.StringBuilder.Append(nameof(Nullable) + ": ");
                expressionPrinter.StringBuilder.AppendLine(Nullable);
            }
        }
    }
}
