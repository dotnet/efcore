// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteDateTimeAddTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _concat
            = typeof(string).GetRuntimeMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) });

        private static readonly MethodInfo _addMilliseconds
            = typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMilliseconds), new[] { typeof(double) });

        private static readonly MethodInfo _addTicks
            = typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddTicks), new[] { typeof(long) });

        private readonly Dictionary<MethodInfo, string> _methodInfoToUnitSuffix = new Dictionary<MethodInfo, string>
        {
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddYears), new[] { typeof(int) }), " years" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMonths), new[] { typeof(int) }), " months" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddDays), new[] { typeof(double) }), " days" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddHours), new[] { typeof(double) }), " hours" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMinutes), new[] { typeof(double) }), " minutes" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddSeconds), new[] { typeof(double) }), " seconds" }
        };

        /// <summary>
        ///     Translates the given method call expression.
        /// </summary>
        /// <param name="methodCallExpression">The method call expression.</param>
        /// <param name="logger"> The logger. </param>
        /// <returns>
        ///     A SQL expression representing the translated MethodCallExpression.
        /// </returns>
        public virtual Expression Translate(
            MethodCallExpression methodCallExpression,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            var method = methodCallExpression.Method;

            Expression modifier = null;
            if (Equals(method, _addMilliseconds))
            {
                modifier = Expression.Add(
                    new ExplicitCastExpression(
                        Expression.Divide(
                            methodCallExpression.Arguments[0],
                            Expression.Convert(
                                Expression.Constant(1000),
                                typeof(double))),
                        typeof(string)),
                    Expression.Constant(" seconds"),
                    _concat);
            }
            else if (Equals(method, _addTicks))
            {
                modifier = Expression.Add(
                    new ExplicitCastExpression(
                        Expression.Divide(
                            Expression.Convert(
                                methodCallExpression.Arguments[0],
                                typeof(double)),
                            Expression.Constant((double)TimeSpan.TicksPerSecond)),
                        typeof(string)),
                    Expression.Constant(" seconds"),
                    _concat);
            }
            else if (_methodInfoToUnitSuffix.TryGetValue(method, out var unitSuffix))
            {
                modifier = Expression.Add(
                    new ExplicitCastExpression(
                        methodCallExpression.Arguments[0],
                        typeof(string)),
                    Expression.Constant(unitSuffix),
                    _concat);
            }
            else
            {
                return null;
            }

            Debug.Assert(modifier != null);

            return new SqlFunctionExpression(
                "rtrim",
                typeof(DateTime),
                new Expression[]
                {
                    new SqlFunctionExpression(
                        "rtrim",
                        typeof(DateTime),
                        new Expression[]
                        {
                            SqliteExpression.Strftime(
                                typeof(DateTime),
                                "%Y-%m-%d %H:%M:%f",
                                methodCallExpression.Object,
                                new[] { modifier }),
                            Expression.Constant("0")
                        }),
                    Expression.Constant(".")
                });
        }
    }
}
