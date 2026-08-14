// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteDateTimeMethodTranslator(SqliteSqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
{
    private static readonly MethodInfo AddMilliseconds
        = typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMilliseconds), [typeof(double)])!;

    private static readonly MethodInfo AddTicks
        = typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddTicks), [typeof(long)])!;

    private static readonly Dictionary<MethodInfo, string> MethodInfoToUnitSuffix = new()
    {
        { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddYears), [typeof(int)])!, " years" },
        { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMonths), [typeof(int)])!, " months" },
        { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddDays), [typeof(double)])!, " days" },
        { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddHours), [typeof(double)])!, " hours" },
        { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMinutes), [typeof(double)])!, " minutes" },
        { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddSeconds), [typeof(double)])!, " seconds" },
        { typeof(DateOnly).GetRuntimeMethod(nameof(DateOnly.AddYears), [typeof(int)])!, " years" },
        { typeof(DateOnly).GetRuntimeMethod(nameof(DateOnly.AddMonths), [typeof(int)])!, " months" },
        { typeof(DateOnly).GetRuntimeMethod(nameof(DateOnly.AddDays), [typeof(int)])!, " days" }
    };

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
        if (AddMilliseconds.Equals(method))
        {
            modifier = sqlExpressionFactory.Add(
                sqlExpressionFactory.Convert(
                    sqlExpressionFactory.Divide(
                        arguments[0],
                        sqlExpressionFactory.Constant(1000.0)),
                    typeof(string)),
                sqlExpressionFactory.Constant(" seconds"));
        }
        else if (AddTicks.Equals(method))
        {
            modifier = sqlExpressionFactory.Add(
                sqlExpressionFactory.Convert(
                    sqlExpressionFactory.Divide(
                        arguments[0],
                        sqlExpressionFactory.Constant((double)TimeSpan.TicksPerSecond)),
                    typeof(string)),
                sqlExpressionFactory.Constant(" seconds"));
        }
        else if (MethodInfoToUnitSuffix.TryGetValue(method, out var unitSuffix))
        {
            modifier = sqlExpressionFactory.Add(
                sqlExpressionFactory.Convert(arguments[0], typeof(string)),
                sqlExpressionFactory.Constant(unitSuffix));
        }

        if (modifier != null)
        {
            return sqlExpressionFactory.Function(
                "rtrim",
                new[]
                {
                    sqlExpressionFactory.Function(
                        "rtrim",
                        new[]
                        {
                            sqlExpressionFactory.Strftime(
                                method.ReturnType,
                                "%Y-%m-%d %H:%M:%f",
                                instance!,
                                modifiers: [modifier]),
                            sqlExpressionFactory.Constant("0")
                        },
                        nullable: true,
                        argumentsPropagateNullability: [true, false],
                        method.ReturnType),
                    sqlExpressionFactory.Constant(".")
                },
                nullable: true,
                argumentsPropagateNullability: [true, false],
                method.ReturnType);
        }

        return null;
    }

    private SqlExpression? TranslateDateOnly(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments)
    {
        if (instance is not null && MethodInfoToUnitSuffix.TryGetValue(method, out var unitSuffix))
        {
            return sqlExpressionFactory.Date(
                method.ReturnType,
                instance,
                modifiers:
                [
                    sqlExpressionFactory.Add(
                        sqlExpressionFactory.Convert(arguments[0], typeof(string)),
                        sqlExpressionFactory.Constant(unitSuffix))
                ]);
        }

        return null;
    }
}
