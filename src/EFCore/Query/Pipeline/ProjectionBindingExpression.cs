// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public class ProjectionBindingExpression : Expression, IPrintable
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

        public Expression QueryExpression { get; }
        public ProjectionMember ProjectionMember { get; }
        public int? Index { get; }
        public IDictionary<IProperty, int> IndexMap { get; }
        public override Type Type { get; }
        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.Append(nameof(ProjectionBindingExpression) + ": ");
            if (ProjectionMember != null)
            {
                expressionPrinter.StringBuilder.Append(ProjectionMember);
            }
            else if (Index != null)
            {
                expressionPrinter.StringBuilder.Append(Index);
            }
            else
            {
                using (expressionPrinter.StringBuilder.Indent())
                {
                    foreach (var kvp in IndexMap)
                    {
                        expressionPrinter.StringBuilder.AppendLine($"{kvp.Key.Name}:{kvp.Value},");
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
