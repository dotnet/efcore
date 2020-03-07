// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class InvocationExpressionRemovingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitInvocation(InvocationExpression invocationExpression)
        {
            Check.NotNull(invocationExpression, nameof(invocationExpression));

            var invokedExpression = StripTrivialConversions(invocationExpression.Expression);

            return invokedExpression is LambdaExpression lambdaExpression
                ? Visit(InlineLambdaExpression(lambdaExpression, invocationExpression.Arguments))
                : base.VisitInvocation(invocationExpression);
        }

        private Expression StripTrivialConversions(Expression expression)
        {
            while (expression is UnaryExpression unaryExpression
                && unaryExpression.NodeType == ExpressionType.Convert
                && expression.Type == unaryExpression.Operand.Type
                && unaryExpression.Method == null)
            {
                expression = unaryExpression.Operand;
            }

            return expression;
        }

        private Expression InlineLambdaExpression(LambdaExpression lambdaExpression, ReadOnlyCollection<Expression> arguments)
            => new ReplacingExpressionVisitor(
                lambdaExpression.Parameters.ToArray<Expression>(), arguments.ToArray())
                .Visit(lambdaExpression.Body);
    }
}
