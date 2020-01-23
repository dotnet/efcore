// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    /// Normalizes certain language-specific aspects of the expression trees produced by languages other
    /// than C#, e.g. Visual Basic.
    /// </summary>
    public class LanguageNormalizingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo _stringCompareWithComparisonMethod
            = typeof(string).GetRuntimeMethod(
                nameof(string.Compare),
                new[] { typeof(string), typeof(string), typeof(StringComparison) });

        private static readonly MethodInfo _stringCompareWithoutComparisonMethod
            = typeof(string).GetRuntimeMethod(
                nameof(string.Compare),
                new[] { typeof(string), typeof(string) });

        private static readonly MethodInfo _stringEqualsMethod
            = typeof(string).GetRuntimeMethod(
                nameof(string.Equals),
                new[] { typeof(string), typeof(string), typeof(StringComparison) });

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var visited = (MethodCallExpression)base.VisitMethodCall(methodCallExpression);

            // In VB.NET, comparison operators between strings (equality, greater-than, less-than) yield
            // calls to a VB-specific CompareString method. Normalize that to string.Compare.
            if (visited.Method.Name == "CompareString"
                && visited.Method.DeclaringType?.Name == "Operators"
                && visited.Method.DeclaringType?.Namespace == "Microsoft.VisualBasic.CompilerServices"
                && visited.Object == null
                && visited.Arguments.Count == 3
                && visited.Arguments[2] is ConstantExpression textCompareConstantExpression)
            {
                return (bool)textCompareConstantExpression.Value
                    ? Expression.Call(
                        _stringCompareWithComparisonMethod,
                        visited.Arguments[0],
                        visited.Arguments[1],
                        Expression.Constant(StringComparison.OrdinalIgnoreCase))
                    : Expression.Call(
                        _stringCompareWithoutComparisonMethod,
                        visited.Arguments[0],
                        visited.Arguments[1]);
            }

            return visited;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            var visitedLeft = Visit(binaryExpression.Left);
            var visitedRight = Visit(binaryExpression.Right);

            // In VB.NET, str1 = str2 yields CompareString(str1, str2, false) == 0.
            // Rewrite this is as a regular equality node.
            if (binaryExpression.NodeType == ExpressionType.Equal
                || binaryExpression.NodeType == ExpressionType.NotEqual)
            {
                var (compareStringExpression, otherExpression) =
                    IsStringCompare(visitedLeft)
                        ? ((MethodCallExpression)visitedLeft, visitedRight)
                        : IsStringCompare(visitedRight)
                            ? ((MethodCallExpression)visitedRight, visitedLeft)
                            : (null, null);

                if (compareStringExpression?.Method == _stringCompareWithoutComparisonMethod)
                {
                    compareStringExpression = Expression.Call(
                        _stringCompareWithComparisonMethod,
                        compareStringExpression.Arguments[0],
                        compareStringExpression.Arguments[1],
                        Expression.Constant(StringComparison.Ordinal));
                }

                if (compareStringExpression != null
                    && (compareStringExpression.Arguments[2] as ConstantExpression)?.Value is StringComparison stringComparison
                    && otherExpression is ConstantExpression otherConstantExpression
                    && (int)otherConstantExpression.Value == 0)
                {
                    switch (stringComparison)
                    {
                        case StringComparison.Ordinal:
                            return Expression.MakeBinary(
                                binaryExpression.NodeType,
                                compareStringExpression.Arguments[0],
                                compareStringExpression.Arguments[1]);

                        case StringComparison.OrdinalIgnoreCase:
                            var stringEqualsExpression = Expression.Call(
                                _stringEqualsMethod,
                                compareStringExpression.Arguments[0],
                                compareStringExpression.Arguments[1],
                                Expression.Constant(StringComparison.OrdinalIgnoreCase)
                            );
                            return binaryExpression.NodeType == ExpressionType.Equal
                                ? (Expression)stringEqualsExpression
                                : Expression.Not(stringEqualsExpression);
                    }
                }
            }

            return binaryExpression.Update(visitedLeft, binaryExpression.Conversion, visitedRight);

            static bool IsStringCompare(Expression expression)
                => expression is MethodCallExpression methodCallExpression
                    && (methodCallExpression.Method == _stringCompareWithComparisonMethod
                        || methodCallExpression.Method == _stringCompareWithoutComparisonMethod);
        }
    }
}
