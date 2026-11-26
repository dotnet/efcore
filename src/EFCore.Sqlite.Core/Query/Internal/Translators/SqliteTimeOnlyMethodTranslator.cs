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
public class SqliteTimeOnlyMethodTranslator(SqliteSqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
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
        if (method.DeclaringType != typeof(TimeOnly))
        {
            return null;
        }

        // Unmapped parameters get their default mapping first, which may carry a converter.
        if (instance is not null)
        {
            instance = sqlExpressionFactory.ApplyDefaultTypeMapping(instance);
        }

        return (instance, method.Name, arguments) switch
        {
            (not null, _, _) when !SqliteSqlExpressionFactory.IsStoredAs(instance, typeof(TimeOnly)) => null,

            (not null, nameof(TimeOnly.AddHours), [var hours]) => AddTicks(instance, hours, TimeSpan.TicksPerHour),
            (not null, nameof(TimeOnly.AddMinutes), [var minutes]) => AddTicks(instance, minutes, TimeSpan.TicksPerMinute),

            // strftime() only keeps milliseconds, so the conversion is done in .NET to preserve the full precision.
            (null, nameof(TimeOnly.FromDateTime), [var dateTime]) => FromDateTime(sqlExpressionFactory.ApplyDefaultTypeMapping(dateTime)),

            _ => null
        };

        SqlExpression? FromDateTime(SqlExpression dateTime)
            => SqliteSqlExpressionFactory.IsStoredAs(dateTime, typeof(DateTime))
                ? sqlExpressionFactory.Function(
                    "ef_time",
                    [dateTime],
                    nullable: true,
                    argumentsPropagateNullability: Statics.TrueArrays[1],
                    typeof(TimeOnly),
                    // The function returns text, whatever the default TimeOnly mapping is.
                    SqliteTimeOnlyTypeMapping.Default)
                : null;

        SqlExpression AddTicks(SqlExpression time, SqlExpression value, long ticksPerUnit)
        {
            // Matches .NET, which truncates the double to whole ticks and wraps around midnight.
            var ticks = value is SqlConstantExpression { Value: double constant }
                ? sqlExpressionFactory.Constant((long)(constant * ticksPerUnit))
                : sqlExpressionFactory.Convert(
                    sqlExpressionFactory.Multiply(value, sqlExpressionFactory.Constant((double)ticksPerUnit)),
                    typeof(long));

            return sqlExpressionFactory.Function(
                "ef_time_add",
                [time, sqlExpressionFactory.ApplyDefaultTypeMapping(ticks)],
                nullable: true,
                argumentsPropagateNullability: Statics.TrueArrays[2],
                time.Type,
                time.TypeMapping);
        }
    }
}
