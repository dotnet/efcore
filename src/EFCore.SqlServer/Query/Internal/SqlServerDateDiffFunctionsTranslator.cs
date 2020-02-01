// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerDateDiffFunctionsTranslator : IMethodCallTranslator
    {
        private readonly Dictionary<MethodInfo, string> _methodInfoDateDiffMapping
            = new Dictionary<MethodInfo, string>
            {
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffYear),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "YEAR"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffYear),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "YEAR"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffYear),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "YEAR"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffYear),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "YEAR"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMonth),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "MONTH"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMonth),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "MONTH"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMonth),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "MONTH"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMonth),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "MONTH"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffDay),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "DAY"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffDay),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "DAY"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffDay),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "DAY"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffDay),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "DAY"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "HOUR"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "HOUR"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "HOUR"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "HOUR"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan) }),
                    "HOUR"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?) }),
                    "HOUR"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "MINUTE"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "MINUTE"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "MINUTE"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "MINUTE"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan) }),
                    "MINUTE"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?) }),
                    "MINUTE"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "SECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "SECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "SECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "SECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan) }),
                    "SECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?) }),
                    "SECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "MILLISECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "MILLISECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "MILLISECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "MILLISECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan) }),
                    "MILLISECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?) }),
                    "MILLISECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "MICROSECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "MICROSECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "MICROSECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "MICROSECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan) }),
                    "MICROSECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?) }),
                    "MICROSECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "NANOSECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "NANOSECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "NANOSECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "NANOSECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan) }),
                    "NANOSECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?) }),
                    "NANOSECOND"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffWeek),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "WEEK"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffWeek),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "WEEK"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffWeek),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "WEEK"
                },
                {
                    typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                        nameof(SqlServerDbFunctionsExtensions.DateDiffWeek),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "WEEK"
                }
            };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerDateDiffFunctionsTranslator(
            [NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));

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
                    nullResultAllowed: true,
                    argumentsPropagateNullability: new[] { false, true, true },
                    typeof(int));
            }

            return null;
        }
    }
}
