// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerStringIndexOfTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.IndexOf), new[] { typeof(string) });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (Equals(methodCallExpression.Method, _methodInfo))
            {
                var patternExpression = methodCallExpression.Arguments[0];

                var charIndexExpression = Expression.Subtract(
                    new SqlFunctionExpression(
                        "CHARINDEX",
                        typeof(int),
                        new[] { patternExpression, methodCallExpression.Object }),
                    Expression.Constant(1));

                return patternExpression is ConstantExpression constantExpression
                       && !string.IsNullOrEmpty((string)constantExpression.Value)
                    ? (Expression)charIndexExpression
                    : Expression.Condition(
                        Expression.Equal(patternExpression, Expression.Constant(string.Empty)),
                        Expression.Constant(0),
                        charIndexExpression);
            }

            return null;
        }
    }
}
