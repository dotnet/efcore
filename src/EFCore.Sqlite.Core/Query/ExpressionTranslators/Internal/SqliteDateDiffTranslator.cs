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
        private readonly Dictionary<MethodInfo, string> _methodInfoDateDiffMapping
            = new Dictionary<MethodInfo, string>
        {
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffYear),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    SqliteDateTimeHelper.SqliteCalcYear
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffYear),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    SqliteDateTimeHelper.SqliteCalcYear
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffYear),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    SqliteDateTimeHelper.SqliteCalcYear
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffYear),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    SqliteDateTimeHelper.SqliteCalcYear
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMonth),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    SqliteDateTimeHelper.SqliteCalcMonth
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMonth),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    SqliteDateTimeHelper.SqliteCalcMonth
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMonth),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    SqliteDateTimeHelper.SqliteCalcMonth
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMonth),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    SqliteDateTimeHelper.SqliteCalcMonth
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffDay),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    SqliteDateTimeHelper.SqliteCalcDay
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffDay),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    SqliteDateTimeHelper.SqliteCalcDay
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffDay),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    SqliteDateTimeHelper.SqliteCalcDay
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffDay),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    SqliteDateTimeHelper.SqliteCalcDay
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffHour),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    SqliteDateTimeHelper.SqliteCalcHour
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffHour),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    SqliteDateTimeHelper.SqliteCalcHour
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffHour),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    SqliteDateTimeHelper.SqliteCalcHour
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffHour),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    SqliteDateTimeHelper.SqliteCalcHour
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMinute),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    SqliteDateTimeHelper.SqliteCalcMinute
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMinute),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    SqliteDateTimeHelper.SqliteCalcMinute
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMinute),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    SqliteDateTimeHelper.SqliteCalcMinute
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMinute),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    SqliteDateTimeHelper.SqliteCalcMinute
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffSecond),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    SqliteDateTimeHelper.SqliteCalcSecond
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffSecond),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    SqliteDateTimeHelper.SqliteCalcSecond
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffSecond),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    SqliteDateTimeHelper.SqliteCalcSecond
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffSecond),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    SqliteDateTimeHelper.SqliteCalcSecond
            },
             {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMillisecond),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    SqliteDateTimeHelper.SqliteFractionalSeconds
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMillisecond),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    SqliteDateTimeHelper.SqliteFractionalSeconds
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMillisecond),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    SqliteDateTimeHelper.SqliteFractionalSeconds
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMillisecond),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    SqliteDateTimeHelper.SqliteFractionalSeconds
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
                              SqliteDateTimeHelper.SqliteFunctionDateFormat,
                              methodCallExpression.Type,
                              new[]
                              {
                                    new SqlFragmentExpression("'%s'"),
                                    methodCallExpression.Arguments[1]
                              }),
                          new SqlFunctionExpression(
                              SqliteDateTimeHelper.SqliteFunctionDateFormat,
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
