// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
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
}
