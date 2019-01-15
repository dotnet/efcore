// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public class EntityShaperExpression : Expression
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
    }

    public class CollectionShaperExpression : Expression
    {
        public CollectionShaperExpression(
            Expression parent,
            Expression innerShaper,
            Expression outerKey,
            Expression innerKey)
        {
            Parent = parent;
            InnerShaper = innerShaper;
            OuterKey = outerKey;
            InnerKey = innerKey;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var parent = visitor.Visit(Parent);
            var innerShaper = visitor.Visit(InnerShaper);
            var outerKey = visitor.Visit(OuterKey);
            var innerKey = visitor.Visit(InnerKey);

            return parent != Parent || innerShaper != InnerShaper || outerKey != OuterKey || innerKey != InnerKey
                ? new CollectionShaperExpression(parent, innerShaper, outerKey, innerKey)
                : this;
        }

        public override Expression Reduce()
        {
            var comparer = EqualityComparer<int>.Default;



            return null;
        }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => typeof(IEnumerable<>).MakeGenericType(InnerShaper.Type);

        public Expression Parent { get; }

        public Expression InnerShaper { get; }

        public Expression OuterKey { get; }

        public Expression InnerKey { get; }
    }
}
