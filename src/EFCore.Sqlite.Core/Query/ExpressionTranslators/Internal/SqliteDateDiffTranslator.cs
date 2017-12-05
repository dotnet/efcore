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
        private static string _sqliteCalcDay = "60 * 60 * 24";
        private static string _sqliteCalcHour = "60 * 60";
        private static string _sqliteCalcMonth = "60 * 60 * 24 * 366/12";
        private static string _sqliteCalcMinute = "60";
        private static string _sqliteCalcSecond = "1";
        private static string _sqliteCalcYear = "60 * 60 * 24 * 366";
        private static string _sqliteFunctionDateFormat = "strftime";
        private static string _sqliteFractionalSeconds = "'%f'";

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
            },
             {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMillisecond),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    _sqliteFractionalSeconds
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMillisecond),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    _sqliteFractionalSeconds
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMillisecond),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    _sqliteFractionalSeconds
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMillisecond),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    _sqliteFractionalSeconds
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
                        new SqlFunctionExpression(
                              string.Empty,
                              returnType: methodCallExpression.Type,
                              arguments: new[]
                              {
                                    new SqlFragmentExpression(datePartCalculate)
                              }))
                    ,
                    typeof(int?));
            }

            return null;
        }
    }
}
