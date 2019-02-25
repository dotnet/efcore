// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerStartsEndsWithTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _startsWithMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) });

        private static readonly MethodInfo _endsWithMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) });

        private static readonly MethodInfo _concat
            = typeof(string).GetRuntimeMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) });

        private const char EscapeChar = '\\';

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(
            MethodCallExpression methodCallExpression,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            bool startsWith;
            if (Equals(methodCallExpression.Method, _startsWithMethodInfo))
            {
                startsWith = true;
            }
            else if (Equals(methodCallExpression.Method, _endsWithMethodInfo))
            {
                startsWith = false;
            }
            else return null;

            var patternExpression = methodCallExpression.Arguments[0];

            if (patternExpression is ConstantExpression constExpr)
            {
                // The pattern is constant. Aside from null or empty, we escape all special characters (%, _, \)
                // in C# and send a simple LIKE
                if (!(constExpr.Value is string pattern))
                {
                    return new LikeExpression(methodCallExpression.Object, Expression.Constant(null));
                }
                if (pattern.Length == 0)
                {
                    return Expression.Constant(true);
                }
                return pattern.Any(c => IsWildChar(c))
                    ? new LikeExpression(
                        methodCallExpression.Object,
                        Expression.Constant(startsWith ? EscapePattern(pattern) + '%' : '%' + EscapePattern(pattern)),
                        Expression.Constant(EscapeChar.ToString()))  // SQL Server has no char mapping, avoid value conversion warning
                    : new LikeExpression(
                        methodCallExpression.Object,
                        Expression.Constant(startsWith ? pattern + '%' : '%' + pattern));
            }

            // The pattern is non-constant, we use LEFT or RIGHT to extract substring and compare.
            // For StartsWith we also first run a LIKE to quickly filter out most non-matching results (sargable, but imprecise
            // because of wildchars).
            if (startsWith)
            {
                return Expression.OrElse(
                    Expression.AndAlso(
                        new LikeExpression(
                            // ReSharper disable once AssignNullToNotNullAttribute
                            methodCallExpression.Object,
                            Expression.Add(methodCallExpression.Arguments[0], Expression.Constant("%", typeof(string)), _concat)),
                        new NullCompensatedExpression(
                            Expression.Equal(
                                new SqlFunctionExpression(
                                    "LEFT",
                                    // ReSharper disable once PossibleNullReferenceException
                                    methodCallExpression.Object.Type,
                                    new[] { methodCallExpression.Object, new SqlFunctionExpression("LEN", typeof(int), new[] { patternExpression }) }),
                                patternExpression))),
                    Expression.Equal(patternExpression, Expression.Constant(string.Empty)));
            }

            return Expression.OrElse(
                new NullCompensatedExpression(
                    Expression.Equal(
                        new SqlFunctionExpression(
                            "RIGHT",
                            // ReSharper disable once PossibleNullReferenceException
                            methodCallExpression.Object.Type,
                            new[] { methodCallExpression.Object, new SqlFunctionExpression("LEN", typeof(int), new[] { patternExpression }) }),
                        patternExpression)),
                Expression.Equal(patternExpression, Expression.Constant(string.Empty)));
        }

        // See https://docs.microsoft.com/en-us/sql/t-sql/language-elements/like-transact-sql
        private bool IsWildChar(char c) => c == '%' || c == '_' || c == '[';

        private string EscapePattern(string pattern)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < pattern.Length; i++)
            {
                var c = pattern[i];
                if (IsWildChar(c) || c == EscapeChar)
                {
                    builder.Append(EscapeChar);
                }
                builder.Append(c);
            }
            return builder.ToString();
        }
    }
}
