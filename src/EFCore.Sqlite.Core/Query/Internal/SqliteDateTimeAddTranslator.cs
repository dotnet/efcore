// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteDateTimeAddTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo _addMilliseconds
        = typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddMilliseconds), typeof(double));

    private static readonly MethodInfo _addTicks
        = typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddTicks), typeof(long));

    private readonly Dictionary<MethodInfo, string> _methodInfoToUnitSuffix = new()
    {
        { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddYears), typeof(int)), " years" },
        { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddMonths), typeof(int)), " months" },
        { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddDays), typeof(double)), " days" },
        { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddHours), typeof(double)), " hours" },
        { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddMinutes), typeof(double)), " minutes" },
        { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddSeconds), typeof(double)), " seconds" },
        { typeof(DateOnly).GetRequiredRuntimeMethod(nameof(DateOnly.AddYears), typeof(int)), " years" },
        { typeof(DateOnly).GetRequiredRuntimeMethod(nameof(DateOnly.AddMonths), typeof(int)), " months" },
        { typeof(DateOnly).GetRequiredRuntimeMethod(nameof(DateOnly.AddDays), typeof(int)), " days" },
    };

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteDateTimeAddTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        => method.DeclaringType == typeof(DateTime)
            ? TranslateDateTime(instance, method, arguments)
            : method.DeclaringType == typeof(DateOnly)
                ? TranslateDateOnly(instance, method, arguments)
                : null;

    private SqlExpression? TranslateDateTime(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments)
    {
        SqlExpression? modifier = null;
        if (_addMilliseconds.Equals(method))
        {
            modifier = _sqlExpressionFactory.Add(
                _sqlExpressionFactory.Convert(
                    _sqlExpressionFactory.Divide(
                        arguments[0],
                        _sqlExpressionFactory.Constant(1000.0)),
                    typeof(string)),
                _sqlExpressionFactory.Constant(" seconds"));
        }
        else if (_addTicks.Equals(method))
        {
            modifier = _sqlExpressionFactory.Add(
                _sqlExpressionFactory.Convert(
                    _sqlExpressionFactory.Divide(
                        arguments[0],
                        _sqlExpressionFactory.Constant((double)TimeSpan.TicksPerDay)),
                    typeof(string)),
                _sqlExpressionFactory.Constant(" seconds"));
        }
        else if (_methodInfoToUnitSuffix.TryGetValue(method, out var unitSuffix))
        {
            modifier = _sqlExpressionFactory.Add(
                _sqlExpressionFactory.Convert(arguments[0], typeof(string)),
                _sqlExpressionFactory.Constant(unitSuffix));
        }

        if (modifier != null)
        {
            return _sqlExpressionFactory.Function(
                "rtrim",
                new SqlExpression[]
                {
                    _sqlExpressionFactory.Function(
                        "rtrim",
                        new SqlExpression[]
                        {
                            SqliteExpression.Strftime(
                                _sqlExpressionFactory,
                                method.ReturnType,
                                "%Y-%m-%d %H:%M:%f",
                                instance!,
                                new[] { modifier }),
                            _sqlExpressionFactory.Constant("0")
                        },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true, false },
                        method.ReturnType),
                    _sqlExpressionFactory.Constant(".")
                },
                nullable: true,
                argumentsPropagateNullability: new[] { true, false },
                method.ReturnType);
        }

        return null;
    }

    private SqlExpression? TranslateDateOnly(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments)
    {
        if (instance is not null && _methodInfoToUnitSuffix.TryGetValue(method, out var unitSuffix))
        {
            return _sqlExpressionFactory.Function(
                "date",
                new[]
                {
                    instance,
                    _sqlExpressionFactory.Add(
                        _sqlExpressionFactory.Convert(arguments[0], typeof(string)),
                        _sqlExpressionFactory.Constant(unitSuffix))
                },
                argumentsPropagateNullability: new[] { true, true },
                nullable: true,
                returnType: method.ReturnType);
        }

        return null;
    }
}
