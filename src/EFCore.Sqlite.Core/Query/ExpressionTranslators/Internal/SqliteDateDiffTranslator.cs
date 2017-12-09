// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteDateDiffTranslator : IMethodCallTranslator
    {
        private const string _sqliteFunctionDateFormat = "strftime";
        private static readonly string _sqliteCalcMonth = "'%m'";
        private static readonly string _sqliteCalcDay = "86400";
        private static readonly string _sqliteCalcHour = "3600";
        private static readonly string _sqliteCalcMinute = "60";
        private static readonly string _sqliteCalcSecond = "'%S'";
        private static readonly string _sqliteCalcYear = "'%Y'";

        private readonly Dictionary<MethodInfo, string> _methodInfoDateDiffMapping
            = new Dictionary<MethodInfo, string>
        {
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffYear),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    _sqliteCalcYear
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffYear),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    _sqliteCalcYear
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffYear),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    _sqliteCalcYear
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffYear),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    _sqliteCalcYear
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMonth),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    _sqliteCalcMonth
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMonth),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    _sqliteCalcMonth
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMonth),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    _sqliteCalcMonth
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMonth),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    _sqliteCalcMonth
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffDay),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    _sqliteCalcDay
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffDay),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    _sqliteCalcDay
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffDay),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    _sqliteCalcDay
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffDay),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    _sqliteCalcDay
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffHour),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    _sqliteCalcHour
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffHour),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    _sqliteCalcHour
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffHour),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    _sqliteCalcHour
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffHour),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    _sqliteCalcHour
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMinute),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    _sqliteCalcMinute
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMinute),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    _sqliteCalcMinute
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMinute),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    _sqliteCalcMinute
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMinute),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    _sqliteCalcMinute
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffSecond),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    _sqliteCalcSecond
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffSecond),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    _sqliteCalcSecond
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffSecond),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    _sqliteCalcSecond
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffSecond),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    _sqliteCalcSecond
            }
        };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (_methodInfoDateDiffMapping.TryGetValue(methodCallExpression.Method, out var datePartCalculate))
            {
                switch (methodCallExpression.Method.Name)
                {
                    case nameof(DbFunctionsExtensions.DateDiffMonth):
                        return
                            Expression.Multiply(
                                Expression.Constant(12, typeof(int?)), 
                                Expression.Add(
                                    Expression.Subtract(
                                        new ExplicitCastExpression(
                                            new SqlFunctionExpression(
                                                _sqliteFunctionDateFormat,
                                                methodCallExpression.Type,
                                                new[]
                                                {
                                                    new SqlFragmentExpression(_sqliteCalcYear),
                                                    methodCallExpression.Arguments[1]
                                                }), typeof(int?)),
                                        new ExplicitCastExpression(
                                           new SqlFunctionExpression(
                                                _sqliteFunctionDateFormat,
                                                methodCallExpression.Type,
                                                new[]
                                                {
                                                    new SqlFragmentExpression(_sqliteCalcYear),
                                                    methodCallExpression.Arguments[2]
                                                }), typeof(int?))),
                                    Expression.Subtract(
                                        new ExplicitCastExpression(
                                            new SqlFunctionExpression(
                                                _sqliteFunctionDateFormat,
                                                methodCallExpression.Type,
                                                new[]
                                                {
                                                    new SqlFragmentExpression(_sqliteCalcMonth),
                                                    methodCallExpression.Arguments[1]
                                                }), typeof(int?)),
                                        new ExplicitCastExpression(
                                           new SqlFunctionExpression(
                                                _sqliteFunctionDateFormat,
                                                methodCallExpression.Type,
                                                new[]
                                                {
                                                    new SqlFragmentExpression(_sqliteCalcMonth),
                                                    methodCallExpression.Arguments[2]
                                                }), typeof(int?))
                                              )));
                    case nameof(DbFunctionsExtensions.DateDiffDay):
                    case nameof(DbFunctionsExtensions.DateDiffHour):
                    case nameof(DbFunctionsExtensions.DateDiffMinute):
                        return new ExplicitCastExpression(
                            Expression.Divide(
                                Expression.Subtract(
                                  new SqlFunctionExpression(
                                      _sqliteFunctionDateFormat,
                                      methodCallExpression.Type,
                                      new[]
                                      {
                                            new SqlFragmentExpression("'%s'"),
                                            methodCallExpression.Arguments[1]
                                      }),
                                  new SqlFunctionExpression(
                                      _sqliteFunctionDateFormat,
                                      methodCallExpression.Type,
                                      new[]
                                      {
                                            new SqlFragmentExpression("'%s'"),
                                            methodCallExpression.Arguments[2]
                                      })
                                  ),
                                Expression.Constant(int.Parse(datePartCalculate), typeof(int?)))
                            ,
                            typeof(int?));
                    default:
                        return Expression.Subtract(
                             new ExplicitCastExpression(
                                 new SqlFunctionExpression(
                                     _sqliteFunctionDateFormat,
                                     typeof(double),
                                     new[]
                                     {
                                        new SqlFragmentExpression(datePartCalculate),
                                        methodCallExpression.Arguments[1]
                                     }),
                             typeof(int?)),
                             new ExplicitCastExpression(
                                 new SqlFunctionExpression(
                                     _sqliteFunctionDateFormat,
                                     typeof(double),
                                     new[]
                                     {
                                        new SqlFragmentExpression(datePartCalculate),
                                        methodCallExpression.Arguments[2]
                                     }),
                             typeof(int?)));
                }
            }

            return null;
        }
    }
}
