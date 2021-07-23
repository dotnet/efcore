﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class InvocationExpressionRemovingExpressionVisitor : ExpressionVisitor
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
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
