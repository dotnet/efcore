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
public class SqliteTimeSpanMemberTranslator(SqliteSqlExpressionFactory sqlExpressionFactory) : IMemberTranslator
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (member.DeclaringType != typeof(TimeSpan) || instance is null)
        {
            return null;
        }

        var ticks = sqlExpressionFactory.EfTicks(instance);

        // Integer division and modulo in SQLite truncate toward zero, matching the .NET definitions of these members.
        return member.Name switch
        {
            nameof(TimeSpan.Ticks) => ticks,
            nameof(TimeSpan.Days) => Component(TimeSpan.TicksPerDay, modulo: null),
            nameof(TimeSpan.Hours) => Component(TimeSpan.TicksPerHour, 24),
            nameof(TimeSpan.Minutes) => Component(TimeSpan.TicksPerMinute, 60),
            nameof(TimeSpan.Seconds) => Component(TimeSpan.TicksPerSecond, 60),
            nameof(TimeSpan.Milliseconds) => Component(TimeSpan.TicksPerMillisecond, 1000),
            nameof(TimeSpan.Microseconds) => Component(TimeSpan.TicksPerMicrosecond, 1000),
            nameof(TimeSpan.Nanoseconds)
                => sqlExpressionFactory.Convert(
                    sqlExpressionFactory.Multiply(
                        sqlExpressionFactory.Modulo(ticks, sqlExpressionFactory.Constant(TimeSpan.TicksPerMicrosecond)),
                        sqlExpressionFactory.Constant(100L)),
                    returnType),

            nameof(TimeSpan.TotalDays) => Total(TimeSpan.TicksPerDay),
            nameof(TimeSpan.TotalHours) => Total(TimeSpan.TicksPerHour),
            nameof(TimeSpan.TotalMinutes) => Total(TimeSpan.TicksPerMinute),
            nameof(TimeSpan.TotalSeconds) => Total(TimeSpan.TicksPerSecond),
            nameof(TimeSpan.TotalMilliseconds) => Total(TimeSpan.TicksPerMillisecond),
            nameof(TimeSpan.TotalMicroseconds) => Total(TimeSpan.TicksPerMicrosecond),
            nameof(TimeSpan.TotalNanoseconds)
                => sqlExpressionFactory.Multiply(
                    sqlExpressionFactory.Convert(ticks, typeof(double)),
                    sqlExpressionFactory.Constant(100.0)),

            _ => null
        };

        SqlExpression Component(long ticksPerUnit, int? modulo)
        {
            SqlExpression result = sqlExpressionFactory.Divide(ticks, sqlExpressionFactory.Constant(ticksPerUnit));
            if (modulo is not null)
            {
                result = sqlExpressionFactory.Modulo(result, sqlExpressionFactory.Constant((long)modulo.Value));
            }

            return sqlExpressionFactory.Convert(result, returnType);
        }

        SqlExpression Total(long ticksPerUnit)
            => sqlExpressionFactory.Divide(
                sqlExpressionFactory.Convert(ticks, typeof(double)),
                sqlExpressionFactory.Constant((double)ticksPerUnit));
    }
}
