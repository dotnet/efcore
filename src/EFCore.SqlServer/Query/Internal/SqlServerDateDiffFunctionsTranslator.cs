// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

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
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffYear),
                    [typeof(DbFunctions), typeof(DateTime), typeof(DateTime)])!,
                "year"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffYear),
                    [typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?)])!,
                "year"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffYear),
                    [typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset)])!,
                "year"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffYear),
                    [typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?)])!,
                "year"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffYear),
                    [typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly)])!,
                "year"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffYear),
                    [typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?)])!,
                "year"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMonth),
                    [typeof(DbFunctions), typeof(DateTime), typeof(DateTime)])!,
                "month"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMonth),
                    [typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?)])!,
                "month"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMonth),
                    [typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset)])!,
                "month"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMonth),
                    [typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?)])!,
                "month"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMonth),
                    [typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly)])!,
                "month"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMonth),
                    [typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?)])!,
                "month"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffDay), [typeof(DbFunctions), typeof(DateTime), typeof(DateTime)])!,
                "day"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffDay),
                    [typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?)])!,
                "day"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffDay),
                    [typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset)])!,
                "day"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffDay),
                    [typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?)])!,
                "day"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffDay),
                    [typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly)])!,
                "day"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffDay),
                    [typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?)])!,
                "day"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                    [typeof(DbFunctions), typeof(DateTime), typeof(DateTime)])!,
                "hour"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                    [typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?)])!,
                "hour"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                    [typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset)])!,
                "hour"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                    [typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?)])!,
                "hour"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                    [typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan)])!,
                "hour"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                    [typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?)])!,
                "hour"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                    [typeof(DbFunctions), typeof(TimeOnly), typeof(TimeOnly)])!,
                "hour"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                    [typeof(DbFunctions), typeof(TimeOnly?), typeof(TimeOnly?)])!,
                "hour"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                    [typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly)])!,
                "hour"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffHour),
                    [typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?)])!,
                "hour"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                    [typeof(DbFunctions), typeof(DateTime), typeof(DateTime)])!,
                "minute"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                    [typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?)])!,
                "minute"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                    [typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset)])!,
                "minute"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                    [typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?)])!,
                "minute"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                    [typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan)])!,
                "minute"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                    [typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?)])!,
                "minute"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                    [typeof(DbFunctions), typeof(TimeOnly), typeof(TimeOnly)])!,
                "minute"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                    [typeof(DbFunctions), typeof(TimeOnly?), typeof(TimeOnly?)])!,
                "minute"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                    [typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly)])!,
                "minute"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMinute),
                    [typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?)])!,
                "minute"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                    [typeof(DbFunctions), typeof(DateTime), typeof(DateTime)])!,
                "second"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                    [typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?)])!,
                "second"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                    [typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset)])!,
                "second"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                    [typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?)])!,
                "second"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                    [typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan)])!,
                "second"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                    [typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?)])!,
                "second"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                    [typeof(DbFunctions), typeof(TimeOnly), typeof(TimeOnly)])!,
                "second"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                    [typeof(DbFunctions), typeof(TimeOnly?), typeof(TimeOnly?)])!,
                "second"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                    [typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly)])!,
                "second"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffSecond),
                    [typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?)])!,
                "second"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                    [typeof(DbFunctions), typeof(DateTime), typeof(DateTime)])!,
                "millisecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                    [typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?)])!,
                "millisecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                    [typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset)])!,
                "millisecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                    [typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?)])!,
                "millisecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                    [typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan)])!,
                "millisecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                    [typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?)])!,
                "millisecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                    [typeof(DbFunctions), typeof(TimeOnly), typeof(TimeOnly)])!,
                "millisecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                    [typeof(DbFunctions), typeof(TimeOnly?), typeof(TimeOnly?)])!,
                "millisecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                    [typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly)])!,
                "millisecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond),
                    [typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?)])!,
                "millisecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                    [typeof(DbFunctions), typeof(DateTime), typeof(DateTime)])!,
                "microsecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                    [typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?)])!,
                "microsecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                    [typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset)])!,
                "microsecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                    [typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?)])!,
                "microsecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                    [typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan)])!,
                "microsecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                    [typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?)])!,
                "microsecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                    [typeof(DbFunctions), typeof(TimeOnly), typeof(TimeOnly)])!,
                "microsecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                    [typeof(DbFunctions), typeof(TimeOnly?), typeof(TimeOnly?)])!,
                "microsecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                    [typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly)])!,
                "microsecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond),
                    [typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?)])!,
                "microsecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                    [typeof(DbFunctions), typeof(DateTime), typeof(DateTime)])!,
                "nanosecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                    [typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?)])!,
                "nanosecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                    [typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset)])!,
                "nanosecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                    [typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?)])!,
                "nanosecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                    [typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan)])!,
                "nanosecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                    [typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?)])!,
                "nanosecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                    [typeof(DbFunctions), typeof(TimeOnly), typeof(TimeOnly)])!,
                "nanosecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                    [typeof(DbFunctions), typeof(TimeOnly?), typeof(TimeOnly?)])!,
                "nanosecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                    [typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly)])!,
                "nanosecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond),
                    [typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?)])!,
                "nanosecond"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffWeek),
                    [typeof(DbFunctions), typeof(DateTime), typeof(DateTime)])!,
                "week"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffWeek),
                    [typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?)])!,
                "week"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffWeek),
                    [typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset)])!,
                "week"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffWeek),
                    [typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?)])!,
                "week"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffWeek),
                    [typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly)])!,
                "week"
            },
            {
                typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DateDiffWeek),
                    [typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?)])!,
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
