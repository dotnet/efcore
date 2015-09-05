// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors.Internal
{
    public class ReducingExpressionVisitor : ExpressionVisitorBase
    {
        public override Expression Visit(Expression node)
            => node != null
               && node.CanReduce
                ? Visit(node.Reduce())
                : base.Visit(node);

        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            var clonedModel = expression.QueryModel.Clone();

            clonedModel.TransformExpressions(Visit);

            return new SubQueryExpression(clonedModel);
        }
    }
}
