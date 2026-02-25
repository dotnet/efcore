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

        var daysExpression = sqlExpressionFactory.EfDays(instance);

        return member.Name switch
        {
            nameof(TimeSpan.Days)
                => sqlExpressionFactory.Convert(daysExpression, typeof(int)),
            nameof(TimeSpan.Hours)
                => sqlExpressionFactory.Convert(
                    sqlExpressionFactory.Modulo(
                        sqlExpressionFactory.Multiply(daysExpression, sqlExpressionFactory.Constant(24.0)),
                        sqlExpressionFactory.Constant(24.0)),
                    returnType),
            nameof(TimeSpan.Minutes)
                => sqlExpressionFactory.Convert(
                    sqlExpressionFactory.Modulo(
                        sqlExpressionFactory.Multiply(daysExpression, sqlExpressionFactory.Constant(1440.0)),
                        sqlExpressionFactory.Constant(60.0)),
                    returnType),
            nameof(TimeSpan.Seconds)
                => sqlExpressionFactory.Convert(
                    sqlExpressionFactory.Modulo(
                        sqlExpressionFactory.Multiply(daysExpression, sqlExpressionFactory.Constant(86400.0)),
                        sqlExpressionFactory.Constant(60.0)),
                    returnType),
            nameof(TimeSpan.Milliseconds)
                => sqlExpressionFactory.Convert(
                    sqlExpressionFactory.Modulo(
                        sqlExpressionFactory.Multiply(daysExpression, sqlExpressionFactory.Constant(86400000.0)),
                        sqlExpressionFactory.Constant(1000.0)),
                    returnType),
            nameof(TimeSpan.Microseconds)
                => sqlExpressionFactory.Convert(
                    sqlExpressionFactory.Modulo(
                        sqlExpressionFactory.Multiply(daysExpression, sqlExpressionFactory.Constant(86400000000.0)),
                        sqlExpressionFactory.Constant(1000.0)),
                    returnType),
            nameof(TimeSpan.Nanoseconds)
                => sqlExpressionFactory.Convert(
                    sqlExpressionFactory.Modulo(
                        sqlExpressionFactory.Multiply(daysExpression, sqlExpressionFactory.Constant(86400000000000.0)),
                        sqlExpressionFactory.Constant(1000.0)),
                    returnType),
            nameof(TimeSpan.Ticks)
                => sqlExpressionFactory.Convert(
                    sqlExpressionFactory.Multiply(
                        daysExpression,
                        sqlExpressionFactory.Constant((double)TimeSpan.TicksPerDay)),
                    typeof(long)),
            nameof(TimeSpan.TotalDays)
                => daysExpression,
            nameof(TimeSpan.TotalHours)
                => sqlExpressionFactory.Multiply(daysExpression, sqlExpressionFactory.Constant(24.0)),
            nameof(TimeSpan.TotalMinutes)
                => sqlExpressionFactory.Multiply(daysExpression, sqlExpressionFactory.Constant(1440.0)),
            nameof(TimeSpan.TotalSeconds)
                => sqlExpressionFactory.Multiply(daysExpression, sqlExpressionFactory.Constant(86400.0)),
            nameof(TimeSpan.TotalMilliseconds)
                => sqlExpressionFactory.Multiply(daysExpression, sqlExpressionFactory.Constant(86400000.0)),
            _ => null
        };
    }
}
