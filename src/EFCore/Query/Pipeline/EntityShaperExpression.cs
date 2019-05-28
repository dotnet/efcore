// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public class EntityShaperExpression : Expression, IPrintable
    {
        public EntityShaperExpression(IEntityType entityType, ProjectionBindingExpression valueBufferExpression, bool nullable)
        {
            EntityType = entityType;
            ValueBufferExpression = valueBufferExpression;
            Nullable = nullable;
        }

        public IEntityType EntityType { get; }
        public ProjectionBindingExpression ValueBufferExpression { get; }
        public bool Nullable { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var valueBufferExpression = (ProjectionBindingExpression)visitor.Visit(ValueBufferExpression);

            return Update(valueBufferExpression);
        }

        public EntityShaperExpression Update(ProjectionBindingExpression valueBufferExpression)
        {
            return valueBufferExpression != ValueBufferExpression
                ? new EntityShaperExpression(EntityType, valueBufferExpression, Nullable)
                : this;
        }

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
                }
            }
        }
    }

    public class EntityValuesExpression : Expression
    {
        public EntityValuesExpression(IEntityType entityType, ProjectionBindingExpression valueBufferExpression)
        {
            EntityType = entityType;
            ValueBufferExpression = valueBufferExpression;
        }

        public IEntityType EntityType { get; }
        public ProjectionBindingExpression ValueBufferExpression { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var valueBufferExpression = (ProjectionBindingExpression)visitor.Visit(ValueBufferExpression);

            return Update(valueBufferExpression);
        }

        public EntityValuesExpression Update(ProjectionBindingExpression valueBufferExpression)
        {
            return valueBufferExpression != ValueBufferExpression
                ? new EntityValuesExpression(EntityType, valueBufferExpression)
                : this;
        }

        public override Type Type => typeof(object[]);
        public override ExpressionType NodeType => ExpressionType.Extension;
    }

    public class CollectionShaperExpression : Expression, IPrintable
    {
        public CollectionShaperExpression(
            ProjectionBindingExpression projection,
            Expression innerShaper,
            INavigation navigation)
        {
            Projection = projection;
            InnerShaper = innerShaper;
            Navigation = navigation;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var projection = (ProjectionBindingExpression)visitor.Visit(Projection);
            var innerShaper = visitor.Visit(InnerShaper);

            return Update(projection, innerShaper);
        }

        public CollectionShaperExpression Update(ProjectionBindingExpression projection, Expression innerShaper)
        {
            return projection != Projection || innerShaper != InnerShaper
                ? new CollectionShaperExpression(projection, innerShaper, Navigation)
                : this;
        }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => typeof(IEnumerable<>).MakeGenericType(InnerShaper.Type);

        public ProjectionBindingExpression Projection { get; }
        public Expression InnerShaper { get; }
        public INavigation Navigation { get; }

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.AppendLine("CollectionShaper:");
            expressionPrinter.StringBuilder.IncrementIndent();
            expressionPrinter.StringBuilder.Append("(");
            expressionPrinter.Visit(Projection);
            expressionPrinter.StringBuilder.Append(", ");
            expressionPrinter.Visit(InnerShaper);
            expressionPrinter.StringBuilder.AppendLine($", {Navigation.Name})");
            expressionPrinter.StringBuilder.DecrementIndent();
        }
    }
}
