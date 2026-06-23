// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerDateDiffFunctionsTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
{
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
        if (method.DeclaringType != typeof(SqlServerDbFunctionsExtensions))
        {
            return null;
        }

        var datePart = method.Name switch
        {
            nameof(SqlServerDbFunctionsExtensions.DateDiffYear) => "year",
            nameof(SqlServerDbFunctionsExtensions.DateDiffMonth) => "month",
            nameof(SqlServerDbFunctionsExtensions.DateDiffDay) => "day",
            nameof(SqlServerDbFunctionsExtensions.DateDiffHour) => "hour",
            nameof(SqlServerDbFunctionsExtensions.DateDiffMinute) => "minute",
            nameof(SqlServerDbFunctionsExtensions.DateDiffSecond) => "second",
            nameof(SqlServerDbFunctionsExtensions.DateDiffMillisecond) => "millisecond",
            nameof(SqlServerDbFunctionsExtensions.DateDiffMicrosecond) => "microsecond",
            nameof(SqlServerDbFunctionsExtensions.DateDiffNanosecond) => "nanosecond",
            nameof(SqlServerDbFunctionsExtensions.DateDiffWeek) => "week",
            _ => null
        };

        if (datePart is null)
        {
            return null;
        }

        var startDate = arguments[1];
        var endDate = arguments[2];
        var typeMapping = ExpressionExtensions.InferTypeMapping(startDate, endDate);

        startDate = sqlExpressionFactory.ApplyTypeMapping(startDate, typeMapping);
        endDate = sqlExpressionFactory.ApplyTypeMapping(endDate, typeMapping);

        return sqlExpressionFactory.Function(
            "DATEDIFF",
            [sqlExpressionFactory.Fragment(datePart), startDate, endDate],
            nullable: true,
            argumentsPropagateNullability: [false, true, true],
            typeof(int));
    }
}
