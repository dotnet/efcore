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
                (not null, nameof(DateOnly.ToDateTime), [var time])
                    when time.Type == typeof(TimeOnly)
                    && SqliteSqlExpressionFactory.IsStoredAs(instance, typeof(DateOnly))
                    && SqliteSqlExpressionFactory.IsStoredAs(time, typeof(TimeOnly))
                    => sqlExpressionFactory.ApplyDefaultTypeMapping(
                        sqlExpressionFactory.Function(
                            "ef_datetime",
                            [instance, time],
                            nullable: true,
                            argumentsPropagateNullability: Statics.TrueArrays[2],
                            method.ReturnType)),

                _ => null
            };
}
