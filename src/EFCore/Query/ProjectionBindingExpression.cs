// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ProjectionBindingExpression : Expression, IPrintableExpression
    {
        public ProjectionBindingExpression(Expression queryExpression, ProjectionMember projectionMember, Type type)
        {
            QueryExpression = queryExpression;
            ProjectionMember = projectionMember;
            Type = type;
        }

        public ProjectionBindingExpression(Expression queryExpression, int index, Type type)
        {
            QueryExpression = queryExpression;
            Index = index;
            Type = type;
        }

        public ProjectionBindingExpression(Expression queryExpression, IDictionary<IProperty, int> indexMap)
        {
            QueryExpression = queryExpression;
            IndexMap = indexMap;
            Type = typeof(ValueBuffer);
        }

        public virtual Expression QueryExpression { get; }
        public virtual ProjectionMember ProjectionMember { get; }
        public virtual int? Index { get; }
        public virtual IDictionary<IProperty, int> IndexMap { get; }
        public override Type Type { get; }
        public sealed override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append(nameof(ProjectionBindingExpression) + ": ");
            if (ProjectionMember != null)
            {
                expressionPrinter.Append(ProjectionMember);
            }
            else if (Index != null)
            {
                expressionPrinter.Append(Index);
            }
            else
            {
                using (expressionPrinter.Indent())
                {
                    foreach (var kvp in IndexMap)
                    {
                        expressionPrinter.AppendLine($"{kvp.Key.Name}:{kvp.Value},");
                    }
                }
            }
        }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is ProjectionBindingExpression projectionBindingExpression
                    && Equals(projectionBindingExpression));

        private bool Equals(ProjectionBindingExpression projectionBindingExpression)
            => QueryExpression.Equals(projectionBindingExpression.QueryExpression)
               && Type == projectionBindingExpression.Type
               && (ProjectionMember?.Equals(projectionBindingExpression.ProjectionMember)
                   ?? projectionBindingExpression.ProjectionMember == null)
               && Index == projectionBindingExpression.Index
               // Using reference equality here since if we are this far, we don't need to compare this.
               && IndexMap == projectionBindingExpression.IndexMap;

        public override int GetHashCode() => HashCode.Combine(QueryExpression, ProjectionMember, Index, IndexMap);
    }
}
