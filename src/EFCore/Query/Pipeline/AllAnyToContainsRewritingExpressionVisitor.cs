// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.NavigationExpansion;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public class AllAnyToContainsRewritingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo _anyMethodInfo =
            LinqMethodHelpers.EnumerableAnyPredicateMethodInfo.GetGenericMethodDefinition();

        private static readonly MethodInfo _allMethodInfo =
            LinqMethodHelpers.EnumerableAllMethodInfo.GetGenericMethodDefinition();

        private static readonly MethodInfo _containsMethod =
            LinqMethodHelpers.EnumerableContainsMethodInfo.GetGenericMethodDefinition();

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() is MethodInfo methodInfo
                && (methodInfo.Equals(_anyMethodInfo) || methodInfo.Equals(_allMethodInfo))
                && methodCallExpression.Arguments[0].NodeType is ExpressionType nodeType
                && (nodeType == ExpressionType.Parameter || nodeType == ExpressionType.Constant)
                && methodCallExpression.Arguments[1] is LambdaExpression lambda
                && TryExtractEqualityOperands(lambda.Body, out var left, out var right, out var negated)
                && (left is ParameterExpression || right is ParameterExpression))
            {
                var nonParameterExpression = left is ParameterExpression ? right : left;

                if (methodInfo.Equals(_anyMethodInfo) && !negated)
                {
                    var containsMethod = _containsMethod.MakeGenericMethod(methodCallExpression.Method.GetGenericArguments()[0]);
                    return Expression.Call(null, containsMethod, methodCallExpression.Arguments[0], nonParameterExpression);
                }

                if (methodInfo.Equals(_allMethodInfo) && negated)
                {
                    var containsMethod = _containsMethod.MakeGenericMethod(methodCallExpression.Method.GetGenericArguments()[0]);
                    return Expression.Not(Expression.Call(null, containsMethod, methodCallExpression.Arguments[0], nonParameterExpression));
                }
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        private bool TryExtractEqualityOperands(Expression expression, out Expression left, out Expression right, out bool negated)
        {
            (left, right, negated) = (default, default, default);

            switch (expression)
            {
                case BinaryExpression binaryExpression:
                {
                    switch (binaryExpression.NodeType)
                    {
                        case ExpressionType.Equal:
                            negated = false;
                            break;
                        case ExpressionType.NotEqual:
                            negated = true;
                            break;
                        default:
                            return false;
                    }

                    (left, right) = (binaryExpression.Left, binaryExpression.Right);
                    return true;
                }

                case MethodCallExpression methodCallExpression when methodCallExpression.Method.Name == nameof(object.Equals):
                {
                    negated = false;
                    if (methodCallExpression.Arguments.Count == 1 && methodCallExpression.Object.Type == methodCallExpression.Arguments[0].Type)
                    {
                        (left, right) = (methodCallExpression.Object, methodCallExpression.Arguments[0]);
                    }
                    else if (methodCallExpression.Arguments.Count == 2 && methodCallExpression.Arguments[0].Type == methodCallExpression.Arguments[1].Type)
                    {
                        (left, right) = (methodCallExpression.Arguments[0], methodCallExpression.Arguments[1]);
                    }

                    return true;
                }

                case UnaryExpression unaryExpression when unaryExpression.NodeType == ExpressionType.Not:
                {
                    var result = TryExtractEqualityOperands(unaryExpression.Operand, out left, out right, out negated);
                    negated = !negated;
                    return result;
                }
            }

            return false;
        }
    }
}
