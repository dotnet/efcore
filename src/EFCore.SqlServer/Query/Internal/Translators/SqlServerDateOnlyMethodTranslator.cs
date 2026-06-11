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
public class SqlServerDateOnlyMethodTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
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
        if (method.DeclaringType != typeof(DateOnly))
        {
            return null;
        }

        if (instance is not null)
        {
            if (method.Name == nameof(DateOnly.ToDateTime) && arguments is [var timeOnly])
            {
                // We need to refrain from doing the translation when either the DateOnly or the TimeOnly
                // are a complex SQL expression (anything other than a column/constant/parameter), to avoid evaluating them multiple
                // potentially expensive arbitrary expressions multiple times.
                if (instance is not ColumnExpression and not SqlParameterExpression and not SqlConstantExpression
                    || timeOnly is not ColumnExpression and not SqlParameterExpression and not SqlConstantExpression)
                {
                    return null;
                }

                return sqlExpressionFactory.Function(
                    "DATETIME2FROMPARTS",
                    [
                        MapDatePartExpression("year", instance),
                        MapDatePartExpression("month", instance),
                        MapDatePartExpression("day", instance),
                        MapDatePartExpression("hour", timeOnly),
                        MapDatePartExpression("minute", timeOnly),
                        MapDatePartExpression("second", timeOnly),
                        MapDatePartExpression("fraction", timeOnly),
                        sqlExpressionFactory.Constant(7, typeof(int)),
                    ],
                    nullable: true,
                    argumentsPropagateNullability: [true, true, true, true, true, true, true, false],
                    typeof(DateTime));
            }

            var datePart = method.Name switch
            {
                nameof(DateOnly.AddYears) => "year",
                nameof(DateOnly.AddMonths) => "month",
                nameof(DateOnly.AddDays) => "day",
                _ => (string?)null
            };

            if (datePart is not null)
            {
                instance = sqlExpressionFactory.ApplyDefaultTypeMapping(instance);

                return sqlExpressionFactory.Function(
                    "DATEADD",
                    [sqlExpressionFactory.Fragment(datePart), sqlExpressionFactory.Convert(arguments[0], typeof(int)), instance],
                    nullable: true,
                    argumentsPropagateNullability: [false, true, true],
                    instance.Type,
                    instance.TypeMapping);
            }
        }

        if (method.Name == nameof(DateOnly.FromDateTime) && arguments is [_])
        {
            return sqlExpressionFactory.Convert(arguments[0], typeof(DateOnly));
        }

        return null;
    }

    private SqlExpression MapDatePartExpression(string datepart, SqlExpression argument)
    {
        if (argument is SqlConstantExpression constantArgument)
        {
            var constant = datepart switch
            {
                "year" => ((DateOnly)constantArgument.Value!).Year,
                "month" => ((DateOnly)constantArgument.Value!).Month,
                "day" => ((DateOnly)constantArgument.Value!).Day,
                "hour" => ((TimeOnly)constantArgument.Value!).Hour,
                "minute" => ((TimeOnly)constantArgument.Value!).Minute,
                "second" => ((TimeOnly)constantArgument.Value!).Second,
                "fraction" => ((TimeOnly)constantArgument.Value!).Ticks % 10_000_000,

                _ => throw new UnreachableException()
            };

            return sqlExpressionFactory.Constant(constant, typeof(int));
        }

        if (datepart == "fraction")
        {
            return sqlExpressionFactory.Divide(
                sqlExpressionFactory.Function(
                    "DATEPART",
                    [sqlExpressionFactory.Fragment("nanosecond"), argument],
                    nullable: true,
                    argumentsPropagateNullability: [true, true],
                    typeof(int)
                ),
                sqlExpressionFactory.Constant(100, typeof(int))
            );
        }

        return sqlExpressionFactory.Function(
            "DATEPART",
            [sqlExpressionFactory.Fragment(datepart), argument],
            nullable: true,
            argumentsPropagateNullability: [true, true],
            typeof(int));
    }
}
