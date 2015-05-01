// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public abstract class NullSemanticsExpressionVisitorBase : ExpressionTreeVisitor
    {
        protected Expression BuildIsNullExpression(Expression expression)
        {
            var isNullExpressionBuilder = new IsNullExpressionBuildingVisitor();

            isNullExpressionBuilder.VisitExpression(expression);

            return isNullExpressionBuilder.ResultExpression;
        }

        protected override Expression VisitExtensionExpression(ExtensionExpression expression)
        {
            return expression is NotNullableExpression 
                ? expression
                : base.VisitExtensionExpression(expression);
        }
    }
}
