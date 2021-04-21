// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerDateDiffFunctionsTranslator : IMethodCallTranslator
    {
        private readonly Dictionary<MethodInfo, string> _methodInfoDateDiffMapping
            = new()
            {
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffYear),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "year"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffYear),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "year"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffYear),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "year"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffYear),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "year"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMonth),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "month"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMonth),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "month"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMonth),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "month"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMonth),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "month"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffDay),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "day"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffDay),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "day"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffDay),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "day"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffDay),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "day"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "hour"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "hour"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "hour"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "hour"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan) }),
                    "hour"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?) }),
                    "hour"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "minute"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "minute"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "minute"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "minute"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan) }),
                    "minute"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?) }),
                    "minute"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "second"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "second"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "second"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "second"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan) }),
                    "second"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?) }),
                    "second"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "millisecond"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "millisecond"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "millisecond"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "millisecond"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan) }),
                    "millisecond"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?) }),
                    "millisecond"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "microsecond"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "microsecond"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "microsecond"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "microsecond"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan) }),
                    "microsecond"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?) }),
                    "microsecond"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "nanosecond"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "nanosecond"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "nanosecond"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "nanosecond"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan) }),
                    "nanosecond"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?) }),
                    "nanosecond"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffWeek),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "week"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffWeek),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "week"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffWeek),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "week"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffWeek),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "week"
                }
            };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerDateDiffFunctionsTranslator(
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression? Translate(
            SqlExpression? instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(logger, nameof(logger));

            if (_methodInfoDateDiffMapping.TryGetValue(method, out var datePart))
            {
                var startDate = arguments[1];
                var endDate = arguments[2];
                var typeMapping = ExpressionExtensions.InferTypeMapping(startDate, endDate);

                startDate = _sqlExpressionFactory.ApplyTypeMapping(startDate, typeMapping);
                endDate = _sqlExpressionFactory.ApplyTypeMapping(endDate, typeMapping);

                return _sqlExpressionFactory.Function(
                    "DATEDIFF",
                    new[] { _sqlExpressionFactory.Fragment(datePart), startDate, endDate },
                    nullable: true,
                    argumentsPropagateNullability: new[] { false, true, true },
                    typeof(int));
            }

            return null;
        }
    }
}
