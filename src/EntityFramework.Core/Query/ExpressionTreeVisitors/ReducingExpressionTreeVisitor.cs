// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Query.ExpressionTreeVisitors
{
    public class ReducingExpressionVisitor : ExpressionTreeVisitor
    {
        public override Expression VisitExpression([NotNull] Expression node)
            => node != null
               && node.CanReduce
                ? VisitExpression(node.Reduce())
                : base.VisitExpression(node);

        protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
        {
            var clonedModel = expression.QueryModel.Clone();
            clonedModel.TransformExpressions(VisitExpression);

            return new SubQueryExpression(clonedModel);
        }
    }
}
