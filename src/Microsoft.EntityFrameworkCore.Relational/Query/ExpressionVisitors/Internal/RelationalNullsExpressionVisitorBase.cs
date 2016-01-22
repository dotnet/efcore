// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    public abstract class RelationalNullsExpressionVisitorBase : RelinqExpressionVisitor
    {
        protected virtual Expression BuildIsNullExpression([NotNull] Expression expression)
        {
            var isNullExpressionBuilder = new IsNullExpressionBuildingVisitor();

            isNullExpressionBuilder.Visit(expression);

            return isNullExpressionBuilder.ResultExpression;
        }

        protected override Expression VisitExtension(Expression node)
            => node is NotNullableExpression
                ? node
                : base.VisitExtension(node);
    }
}
