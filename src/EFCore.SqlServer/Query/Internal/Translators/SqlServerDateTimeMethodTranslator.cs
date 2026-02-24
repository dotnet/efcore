// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerDateTimeMethodTranslator(
    ISqlExpressionFactory sqlExpressionFactory,
    IRelationalTypeMappingSource typeMappingSource)
    : IMethodCallTranslator
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
        var declaringType = method.DeclaringType;

        if (declaringType == typeof(DateTime) || declaringType == typeof(DateTimeOffset))
        {
            var datePart = method.Name switch
            {
                nameof(DateTime.AddYears) => "year",
                nameof(DateTime.AddMonths) => "month",
                nameof(DateTime.AddDays) => "day",
                nameof(DateTime.AddHours) => "hour",
                nameof(DateTime.AddMinutes) => "minute",
                nameof(DateTime.AddSeconds) => "second",
                nameof(DateTime.AddMilliseconds) => "millisecond",
                _ => (string?)null
            };

            if (datePart is not null && instance is not null)
            {
                // Some Add methods accept a double, and SQL Server DateAdd does not accept number argument outside of int range
                if (arguments[0] is SqlConstantExpression { Value: double and (<= int.MinValue or >= int.MaxValue) })
                {
                    return null;
                }

                // DATEADD defaults to interpreting its last argument as datetime, not datetime2.
                // Our default mapping for DateTime is datetime2, so we force constants to be datetime instead here.
                if (instance is SqlConstantExpression instanceConstant)
                {
                    instance = instanceConstant.ApplyTypeMapping(typeMappingSource.FindMapping(typeof(DateTime), "datetime"));
                }

                return sqlExpressionFactory.Function(
                    "DATEADD",
                    arguments:
                    [
                        sqlExpressionFactory.Fragment(datePart),
                        sqlExpressionFactory.Convert(arguments[0], typeof(int)),
                        instance
                    ],
                    nullable: true,
                    argumentsPropagateNullability: [false, true, true],
                    instance.Type,
                    instance.TypeMapping);
            }

            if (declaringType == typeof(DateTimeOffset))
            {
                var timePart = method.Name switch
                {
                    nameof(DateTimeOffset.ToUnixTimeSeconds) => "second",
                    nameof(DateTimeOffset.ToUnixTimeMilliseconds) => "millisecond",
                    _ => null
                };

                if (timePart is not null)
                {
                    return sqlExpressionFactory.Function(
                        "DATEDIFF_BIG",
                        arguments:
                        [
                            sqlExpressionFactory.Fragment(timePart),
                            sqlExpressionFactory.Constant(DateTimeOffset.UnixEpoch, instance!.TypeMapping),
                            instance
                        ],
                        nullable: true,
                        argumentsPropagateNullability: [false, true, true],
                        typeof(long));
                }
            }

            return null;
        }

        if (declaringType == typeof(SqlServerDbFunctionsExtensions)
            && method.Name == nameof(SqlServerDbFunctionsExtensions.AtTimeZone)
            && arguments is [_, var operand, var timeZone])
        {
            RelationalTypeMapping? resultTypeMapping = null;

            // The AT TIME ZONE construct bubbles up the precision of its operand, so when invoked over datetime2(2) it returns a
            // datetimeoffset(2). So if the operand has a type mapping, bubble it up accordingly, otherwise allow the result type mapping
            // to be inferred.
            if (operand.TypeMapping is { } operandTypeMapping)
            {
                resultTypeMapping = operandTypeMapping.StoreTypeNameBase switch
                {
                    "datetimeoffset"
                        => operandTypeMapping,
                    "datetime" or "datetime2" or "smalldatetime"
                        => typeMappingSource.FindMapping(
                            typeof(DateTimeOffset), "datetimeoffset", precision: operandTypeMapping.Precision),
                    _ => null
                };

                Check.DebugAssert(
                    resultTypeMapping is not null,
                    $"Unknown operand type mapping '{operandTypeMapping.StoreTypeNameBase}' when translating EF.Functions.AtTimeZone");
            }

            if (operand is SqlConstantExpression)
            {
                // Our constant representation for datetime/datetimeoffset is an untyped string literal, which the AT TIME ZONE expression
                // does not accept. Type it explicitly.
                operand = sqlExpressionFactory.Convert(operand, operand.Type);
            }

            return new AtTimeZoneExpression(
                operand,
                sqlExpressionFactory.ApplyTypeMapping(timeZone, typeMappingSource.FindMapping("varchar")),
                typeof(DateTimeOffset),
                resultTypeMapping);
        }

        return null;
    }
}
