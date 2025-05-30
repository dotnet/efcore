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
public class SqlServerDateOnlyMethodTranslator : IMethodCallTranslator
{
    private readonly Dictionary<MethodInfo, string> _methodInfoDatePartMapping = new()
    {
        { typeof(DateOnly).GetRuntimeMethod(nameof(DateOnly.AddYears), [typeof(int)])!, "year" },
        { typeof(DateOnly).GetRuntimeMethod(nameof(DateOnly.AddMonths), [typeof(int)])!, "month" },
        { typeof(DateOnly).GetRuntimeMethod(nameof(DateOnly.AddDays), [typeof(int)])!, "day" }
    };

    private static readonly MethodInfo ToDateTimeMethodInfo
        = typeof(DateOnly).GetRuntimeMethod(nameof(DateOnly.ToDateTime), [typeof(TimeOnly)])!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerDateOnlyMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

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
        if (instance != null)
        {
            if (method == ToDateTimeMethodInfo)
            {
                var timeOnly = arguments[0];

                // We need to refrain from doing the translation when either the DateOnly or the TimeOnly
                // are a complex SQL expression (anything other than a column/constant/parameter), to avoid evaluating them multiple
                // potentially expensive arbitrary expressions multiple times.
                if (instance is not ColumnExpression and not SqlParameterExpression and not SqlConstantExpression
                    || timeOnly is not ColumnExpression and not SqlParameterExpression and not SqlConstantExpression)
                {
                    return null;
                }

                return _sqlExpressionFactory.Function(
                    "DATETIME2FROMPARTS",
                    [
                        MapDatePartExpression("year", instance),
                        MapDatePartExpression("month", instance),
                        MapDatePartExpression("day", instance),
                        MapDatePartExpression("hour", timeOnly),
                        MapDatePartExpression("minute", timeOnly),
                        MapDatePartExpression("second", timeOnly),
                        MapDatePartExpression("fraction", timeOnly),
                        _sqlExpressionFactory.Constant(7, typeof(int)),
                    ],
                    nullable: true,
                    argumentsPropagateNullability: [true, true, true, true, true, true, true, false],
                    typeof(DateTime));
            }

            if (_methodInfoDatePartMapping.TryGetValue(method, out var datePart))
            {
                instance = _sqlExpressionFactory.ApplyDefaultTypeMapping(instance);

                return _sqlExpressionFactory.Function(
                    "DATEADD",
                    [_sqlExpressionFactory.Fragment(datePart), _sqlExpressionFactory.Convert(arguments[0], typeof(int)), instance],
                    nullable: true,
                    argumentsPropagateNullability: [false, true, true],
                    instance.Type,
                    instance.TypeMapping);
            }
        }

        if (method.DeclaringType == typeof(DateOnly)
            && method.Name == nameof(DateOnly.FromDateTime)
            && arguments.Count == 1)
        {
            return _sqlExpressionFactory.Convert(arguments[0], typeof(DateOnly));
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

            return _sqlExpressionFactory.Constant(constant, typeof(int));
        }

        if (datepart == "fraction")
        {
            return _sqlExpressionFactory.Divide(
                _sqlExpressionFactory.Function(
                    "DATEPART",
                    [_sqlExpressionFactory.Fragment("nanosecond"), argument],
                    nullable: true,
                    argumentsPropagateNullability: [true, true],
                    typeof(int)
                ),
                _sqlExpressionFactory.Constant(100, typeof(int))
            );
        }

        return _sqlExpressionFactory.Function(
            "DATEPART",
            [_sqlExpressionFactory.Fragment(datepart), argument],
            nullable: true,
            argumentsPropagateNullability: [true, true],
            typeof(int));
    }
}
