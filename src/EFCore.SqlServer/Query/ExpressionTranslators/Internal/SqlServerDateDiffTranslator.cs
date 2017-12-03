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
    public class SqlServerDateDiffTranslator : IMethodCallTranslator
    {
        private readonly Dictionary<MethodInfo, string> _methodInfoDateDiffMapping
            = new Dictionary<MethodInfo, string>
        {
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffYear),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "YEAR"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffYear),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "YEAR"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffYear),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "YEAR"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffYear),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "YEAR"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMonth),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "MONTH"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMonth),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "MONTH"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMonth),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "MONTH"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMonth),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "MONTH"
            },
             {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffDay),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "DAY"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffDay),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "DAY"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffDay),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "DAY"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffDay),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "DAY"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffHour),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "HOUR"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffHour),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "HOUR"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffHour),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "HOUR"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffHour),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "HOUR"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMinute),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "MINUTE"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMinute),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "MINUTE"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMinute),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "MINUTE"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMinute),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "MINUTE"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffSecond),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "SECOND"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffSecond),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "SECOND"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffSecond),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "SECOND"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffSecond),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "SECOND"
            },
             {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMillisecond),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "MILLISECOND"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMillisecond),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "MILLISECOND"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMillisecond),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "MILLISECOND"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMillisecond),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "MILLISECOND"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMicrosecond),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "MICROSECOND"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMicrosecond),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "MICROSECOND"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMicrosecond),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "MICROSECOND"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffMicrosecond),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "MICROSECOND"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffNanosecond),
                    new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "NANOSECOND"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffNanosecond),
                    new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "NANOSECOND"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffNanosecond),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "NANOSECOND"
            },
            {
                typeof(DbFunctionsExtensions).GetRuntimeMethod(
                    nameof(DbFunctionsExtensions.DateDiffNanosecond),
                    new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "NANOSECOND"
            }
        };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (_methodInfoDateDiffMapping.TryGetValue(methodCallExpression.Method, out var datePart))
            {
                return new SqlFunctionExpression(
                    functionName: "DATEDIFF",
                    returnType: methodCallExpression.Type,
                    arguments: new[]
                    {
                        new SqlFragmentExpression(datePart),
                        methodCallExpression.Arguments[1],
                        methodCallExpression.Arguments[2]
                    });
            }

            return null;
        }
    }
}
