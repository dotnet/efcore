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
    public class VBToCSharpConvertingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo _stringCompareWithComparisonMethod
            = typeof(string).GetRuntimeMethod(
                nameof(string.Compare),
                new[] { typeof(string), typeof(string), typeof(StringComparison) });

        private static readonly MethodInfo _stringCompareWithoutComparisonMethod
            = typeof(string).GetRuntimeMethod(
                nameof(string.Compare),
                new[] { typeof(string), typeof(string) });

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
    }
}
