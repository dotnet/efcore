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
        if (instance is null || arguments is not [var arg])
        {
            return null;
        }

        var modifier = method.Name switch
        {
            nameof(DateTime.AddMilliseconds) => sqlExpressionFactory.Add(
                sqlExpressionFactory.Convert(
                    sqlExpressionFactory.Divide(arg, sqlExpressionFactory.Constant(1000.0)),
                    typeof(string)),
                sqlExpressionFactory.Constant(" seconds")),

            nameof(DateTime.AddTicks) => sqlExpressionFactory.Add(
                sqlExpressionFactory.Convert(
                    sqlExpressionFactory.Divide(arg, sqlExpressionFactory.Constant((double)TimeSpan.TicksPerSecond)),
                    typeof(string)),
                sqlExpressionFactory.Constant(" seconds")),

            nameof(DateTime.AddYears) => MakeModifier(arg, " years"),
            nameof(DateTime.AddMonths) => MakeModifier(arg, " months"),
            nameof(DateTime.AddDays) => MakeModifier(arg, " days"),
            nameof(DateTime.AddHours) => MakeModifier(arg, " hours"),
            nameof(DateTime.AddMinutes) => MakeModifier(arg, " minutes"),
            nameof(DateTime.AddSeconds) => MakeModifier(arg, " seconds"),

            _ => (SqlExpression?)null
        };

        if (modifier is null)
        {
            return null;
        }

        return sqlExpressionFactory.Function(
            "rtrim",
            [
                sqlExpressionFactory.Function(
                    "rtrim",
                    [
                        sqlExpressionFactory.Strftime(
                            method.ReturnType,
                            "%Y-%m-%d %H:%M:%f",
                            instance,
                            modifiers: [modifier]),
                        sqlExpressionFactory.Constant("0")
                    ],
                    nullable: true,
                    argumentsPropagateNullability: Statics.TrueFalse,
                    method.ReturnType),
                sqlExpressionFactory.Constant(".")
            ],
            nullable: true,
            argumentsPropagateNullability: Statics.TrueFalse,
            method.ReturnType);
    }

    private SqlExpression? TranslateDateOnly(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments)
    {
        if (instance is null || arguments is not [var arg])
        {
            return null;
        }

        var unitSuffix = method.Name switch
        {
            nameof(DateOnly.AddYears) => " years",
            nameof(DateOnly.AddMonths) => " months",
            nameof(DateOnly.AddDays) => " days",
            _ => (string?)null
        };

        return unitSuffix is not null
            ? sqlExpressionFactory.Date(
                method.ReturnType,
                instance,
                modifiers: [MakeModifier(arg, unitSuffix)])
            : null;
    }

    private SqlExpression MakeModifier(SqlExpression argument, string unitSuffix)
        => sqlExpressionFactory.Add(
            sqlExpressionFactory.Convert(argument, typeof(string)),
            sqlExpressionFactory.Constant(unitSuffix));
}
