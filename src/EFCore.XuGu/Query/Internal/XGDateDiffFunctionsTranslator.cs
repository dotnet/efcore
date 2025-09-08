// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XGDateDiffFunctionsTranslator : IMethodCallTranslator
    {
        private readonly Dictionary<MethodInfo, string> _methodInfoDateDiffMapping
            = new Dictionary<MethodInfo, string>
            {
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffYear), new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }), "YEAR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffYear), new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }), "YEAR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffYear), new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }), "YEAR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffYear), new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }), "YEAR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffYear), new[] { typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly) }), "YEAR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffYear), new[] { typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?) }), "YEAR" },

                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffQuarter), new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }), "QUARTER" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffQuarter), new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }), "QUARTER" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffQuarter), new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }), "QUARTER" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffQuarter), new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }), "QUARTER" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffQuarter), new[] { typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly) }), "QUARTER" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffQuarter), new[] { typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?) }), "QUARTER" },

                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMonth), new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }), "MONTH" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMonth), new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }), "MONTH" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMonth), new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }), "MONTH" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMonth), new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }), "MONTH" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMonth), new[] { typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly) }), "MONTH" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMonth), new[] { typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?) }), "MONTH" },

                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffWeek), new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }), "WEEK" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffWeek), new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }), "WEEK" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffWeek), new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }), "WEEK" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffWeek), new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }), "WEEK" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffWeek), new[] { typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly) }), "WEEK" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffWeek), new[] { typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?) }), "WEEK" },

                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffDay), new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }), "DAY" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffDay), new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }), "DAY" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffDay), new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }), "DAY" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffDay), new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }), "DAY" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffDay), new[] { typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly) }), "DAY" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffDay), new[] { typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?) }), "DAY" },

                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffHour), new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }), "HOUR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffHour), new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }), "HOUR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffHour), new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }), "HOUR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffHour), new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }), "HOUR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffHour), new[] { typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly) }), "HOUR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffHour), new[] { typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?) }), "HOUR" },

                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMinute), new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }), "MINUTE" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMinute), new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }), "MINUTE" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMinute), new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }), "MINUTE" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMinute), new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }), "MINUTE" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMinute), new[] { typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly) }), "MINUTE" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMinute), new[] { typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?) }), "MINUTE" },

                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffSecond), new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }), "SECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffSecond), new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }), "SECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffSecond), new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }), "SECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffSecond), new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }), "SECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffSecond), new[] { typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly) }), "SECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffSecond), new[] { typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?) }), "SECOND" },

                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMillisecond), new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }), "MILLISECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMillisecond), new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }), "MILLISECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMillisecond), new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }), "MILLISECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMillisecond), new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }), "MILLISECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMillisecond), new[] { typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly) }), "MILLISECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMillisecond), new[] { typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?) }), "MILLISECOND" },

                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMicrosecond), new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }), "MICROSECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMicrosecond), new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }), "MICROSECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMicrosecond), new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }), "MICROSECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMicrosecond), new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }), "MICROSECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMicrosecond), new[] { typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly) }), "MICROSECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMicrosecond), new[] { typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?) }), "MICROSECOND" },

                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffTick), new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }), "TICK" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffTick), new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }), "TICK" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffTick), new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }), "TICK" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffTick), new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }), "TICK" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffTick), new[] { typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly) }), "TICK" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffTick), new[] { typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?) }), "TICK" },

                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffNanosecond), new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }), "NANOSECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffNanosecond), new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }), "NANOSECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffNanosecond), new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }), "NANOSECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffNanosecond), new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }), "NANOSECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffNanosecond), new[] { typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly) }), "NANOSECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffNanosecond), new[] { typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?) }), "NANOSECOND" },
            };

        private readonly XGSqlExpressionFactory _sqlExpressionFactory;

        public XGDateDiffFunctionsTranslator(XGSqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (_methodInfoDateDiffMapping.TryGetValue(method, out var datePart))
            {
                var startDate = arguments[1];
                var endDate = arguments[2];
                var typeMapping = ExpressionExtensions.InferTypeMapping(startDate, endDate);

                startDate = _sqlExpressionFactory.ApplyTypeMapping(startDate, typeMapping);
                endDate = _sqlExpressionFactory.ApplyTypeMapping(endDate, typeMapping);

                var actualDatePart = datePart is "MILLISECOND"
                                              or "TICK"
                                              or "NANOSECOND"
                    ? "MICROSECOND"
                    : datePart;

                var timeStampDiffExpression = _sqlExpressionFactory.NullableFunction(
                    "TIMESTAMPDIFF",
                    new[]
                    {
                        _sqlExpressionFactory.Fragment(actualDatePart),
                        startDate,
                        endDate
                    },
                    typeof(int),
                    typeMapping: null,
                    onlyNullWhenAnyNullPropagatingArgumentIsNull: true,
                    argumentsPropagateNullability: new[] { false, true, true });

                return datePart switch
                {
                    "MILLISECOND" => _sqlExpressionFactory.XGIntegerDivide(timeStampDiffExpression, _sqlExpressionFactory.Constant(1_000)),
                    "TICK" => _sqlExpressionFactory.Multiply(timeStampDiffExpression, _sqlExpressionFactory.Constant(10)),
                    "NANOSECOND" => _sqlExpressionFactory.Multiply(timeStampDiffExpression, _sqlExpressionFactory.Constant(1_000)),
                    _ => timeStampDiffExpression
                };
            }

            return null;
        }
    }
}
