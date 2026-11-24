// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteDateOnlyMethodTranslator(SqliteSqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => method.DeclaringType != typeof(DateOnly)
            ? null
            : (instance, method.Name, arguments) switch
            {
                (null, nameof(DateOnly.FromDateTime), [var dateTime]) => sqlExpressionFactory.Date(method.ReturnType, dateTime),

                // Done in .NET, since datetime() would drop the sub-millisecond part of the time.
                // Unmapped parameters get their default mapping first, which may carry a converter.
                (not null, nameof(DateOnly.ToDateTime), [var time]) when time.Type == typeof(TimeOnly)
                    => ToDateTime(sqlExpressionFactory.ApplyDefaultTypeMapping(instance), sqlExpressionFactory.ApplyDefaultTypeMapping(time)),

                _ => null
            };

    private SqlExpression? ToDateTime(SqlExpression date, SqlExpression time)
        => SqliteSqlExpressionFactory.IsStoredAs(date, typeof(DateOnly)) && SqliteSqlExpressionFactory.IsStoredAs(time, typeof(TimeOnly))
            ? sqlExpressionFactory.Function(
                "ef_datetime",
                [date, time],
                nullable: true,
                argumentsPropagateNullability: Statics.TrueArrays[2],
                typeof(DateTime),
                // The function returns text, whatever the default DateTime mapping is.
                SqliteDateTimeTypeMapping.Default)
            : null;
}
