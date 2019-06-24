// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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

        public EntityShaperExpression(
            IEntityType entityType,
            Expression valueBufferExpression,
            bool nullable,
            INavigation parentNavigation,
            IList<EntityShaperExpression> nestedEntities)
        {
            EntityType = entityType;
            ValueBufferExpression = valueBufferExpression;
            Nullable = nullable;
            ParentNavigation = parentNavigation;
            NestedEntities = nestedEntities;
        }

        public IEntityType EntityType { get; }
        public Expression ValueBufferExpression { get; }
        public bool Nullable { get; }
        public INavigation ParentNavigation { get; }
        public IList<EntityShaperExpression> NestedEntities { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var valueBufferExpression = (ProjectionBindingExpression)visitor.Visit(ValueBufferExpression);
            var nestedEntities = NestedEntities;
            if (nestedEntities != null)
            {
                for (var i = 0; i < nestedEntities.Count; i++)
                {
                    nestedEntities[i] = (EntityShaperExpression)visitor.Visit(nestedEntities[i]);
                }
            }

            return Update(valueBufferExpression, nestedEntities);
        }

        public EntityShaperExpression WithEntityType(IEntityType entityType)
        {
            return entityType != EntityType
                ? new EntityShaperExpression(entityType, ValueBufferExpression, Nullable, null, null)
                : this;
        }

        public EntityShaperExpression MarkAsNullable()
        {
            return !Nullable
                ? new EntityShaperExpression(EntityType, ValueBufferExpression, true, ParentNavigation, NestedEntities)
                : this;
        }

        public EntityShaperExpression Update(Expression valueBufferExpression, IList<EntityShaperExpression> nestedEntities)
        {
            return valueBufferExpression != ValueBufferExpression || nestedEntities != null || NestedEntities != null
                ? new EntityShaperExpression(EntityType, valueBufferExpression, Nullable, ParentNavigation, nestedEntities)
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
                    expressionPrinter.StringBuilder.AppendLine();
                }

                expressionPrinter.StringBuilder.Append(nameof(Nullable) + ": ");
                expressionPrinter.StringBuilder.AppendLine(Nullable);

                if (ParentNavigation != null)
                {
                    expressionPrinter.StringBuilder.Append(nameof(ParentNavigation) + ": ");
                    expressionPrinter.StringBuilder.AppendLine(ParentNavigation);
                }

                if (NestedEntities != null)
                {
                    expressionPrinter.StringBuilder.AppendLine(nameof(NestedEntities) + ": ");
                    using (expressionPrinter.StringBuilder.Indent())
                    {
                        for (var i = 0; i < NestedEntities.Count; i++)
                        {
                            expressionPrinter.Visit(NestedEntities[i]);
                        }
                    }
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
