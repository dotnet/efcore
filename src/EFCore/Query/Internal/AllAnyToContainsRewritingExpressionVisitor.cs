// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class AllAnyToContainsRewritingExpressionVisitor : ExpressionVisitor
    {
        private static bool IsExpressionOfFunc(Type type, int funcGenericArgs = 2)
            => type.IsGenericType
                && type.GetGenericArguments().Length == funcGenericArgs;

        private static readonly MethodInfo _allMethodInfo = typeof(Enumerable).GetTypeInfo()
            .GetDeclaredMethods(nameof(Enumerable.All))
            .Single(mi => mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
        private static readonly MethodInfo _anyWithPredicateMethodInfo = typeof(Enumerable).GetTypeInfo()
            .GetDeclaredMethods(nameof(Enumerable.Any))
            .Single(mi => mi.GetParameters().Length == 2 && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
        private static readonly MethodInfo _containsMethodInfo = typeof(Enumerable).GetTypeInfo()
            .GetDeclaredMethods(nameof(Enumerable.Contains))
            .Single(mi => mi.GetParameters().Length == 2);

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() is MethodInfo methodInfo
                && (methodInfo.Equals(_anyWithPredicateMethodInfo) || methodInfo.Equals(_allMethodInfo))
                && methodCallExpression.Arguments[0].NodeType is ExpressionType nodeType
                && (nodeType == ExpressionType.Parameter || nodeType == ExpressionType.Constant)
                && methodCallExpression.Arguments[1] is LambdaExpression lambda
                && TryExtractEqualityOperands(lambda.Body, out var left, out var right, out var negated)
                && (left is ParameterExpression || right is ParameterExpression))
            {
                var nonParameterExpression = left is ParameterExpression ? right : left;

                if (methodInfo.Equals(_anyWithPredicateMethodInfo) && !negated)
                {
                    var containsMethod = _containsMethodInfo.MakeGenericMethod(methodCallExpression.Method.GetGenericArguments()[0]);
                    return Expression.Call(null, containsMethod, methodCallExpression.Arguments[0], nonParameterExpression);
                }

                if (methodInfo.Equals(_allMethodInfo) && negated)
                {
                    var containsMethod = _containsMethodInfo.MakeGenericMethod(methodCallExpression.Method.GetGenericArguments()[0]);
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

                case MethodCallExpression methodCallExpression
                when methodCallExpression.Method.Name == nameof(object.Equals):
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

                case UnaryExpression unaryExpression
                when unaryExpression.NodeType == ExpressionType.Not:
                    var result = TryExtractEqualityOperands(unaryExpression.Operand, out left, out right, out negated);
                    negated = !negated;
                    return result;
            }

            return false;
        }
    }
}
