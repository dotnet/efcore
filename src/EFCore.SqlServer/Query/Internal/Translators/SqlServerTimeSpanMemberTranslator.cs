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
public class SqlServerTimeSpanMemberTranslator(
    ISqlExpressionFactory sqlExpressionFactory)
    : IMemberTranslator
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
        var declaringType = member.DeclaringType;

        return declaringType != typeof(TimeSpan)
            ? null
            : member.Name switch
            {
                nameof(TimeSpan.Hours) => DatePart("hour"),
                nameof(TimeSpan.Minutes) => DatePart("minute"),
                nameof(TimeSpan.Seconds) => DatePart("second"),
                nameof(TimeSpan.Milliseconds) => DatePart("millisecond"),
                nameof(TimeSpan.Microseconds) => sqlExpressionFactory.Modulo(DatePart("microsecond"), sqlExpressionFactory.Constant(1000)),
                nameof(TimeSpan.Nanoseconds) => sqlExpressionFactory.Modulo(DatePart("nanosecond"), sqlExpressionFactory.Constant(1000)),
                nameof(TimeSpan.TotalDays) => TotalPer(TimeSpan.TicksPerDay),
                nameof(TimeSpan.TotalHours) => TotalPer(TimeSpan.TicksPerHour),
                nameof(TimeSpan.TotalMinutes) => TotalPer(TimeSpan.TicksPerMinute),
                nameof(TimeSpan.TotalSeconds) => TotalPer(TimeSpan.TicksPerSecond),
                nameof(TimeSpan.TotalMilliseconds) => TotalPer(TimeSpan.TicksPerMillisecond),
                nameof(TimeSpan.TotalMicroseconds) => TotalPer(TimeSpan.TicksPerMicrosecond),
                nameof(TimeSpan.TotalNanoseconds) => sqlExpressionFactory.Multiply(
                    sqlExpressionFactory.Convert(Ticks(), typeof(double)),
                    sqlExpressionFactory.Constant((double)TimeSpan.NanosecondsPerTick)),
                _ => null,
            };

        SqlExpression DatePart(string part, Type? type = null)
            => sqlExpressionFactory.Function(
                "DATEPART",
                arguments: [sqlExpressionFactory.Fragment(part), instance!],
                nullable: true,
                argumentsPropagateNullability: Statics.FalseTrue,
                type ?? returnType);

        // Like TimeSpan.TotalHours and the other Total* members, divide the number of ticks once, so that the result is the same
        // double as in .NET. SQL Server time values are below 24 hours, so the ticks are computed exactly from the time parts.
        SqlExpression TotalPer(long ticksPerUnit)
            => sqlExpressionFactory.Divide(
                sqlExpressionFactory.Convert(Ticks(), typeof(double)),
                sqlExpressionFactory.Constant((double)ticksPerUnit));

        SqlExpression Ticks()
            => sqlExpressionFactory.Add(
                sqlExpressionFactory.Multiply(
                    sqlExpressionFactory.Convert(
                        sqlExpressionFactory.Add(
                            sqlExpressionFactory.Add(
                                sqlExpressionFactory.Multiply(DatePart("hour", typeof(int)), sqlExpressionFactory.Constant(3600)),
                                sqlExpressionFactory.Multiply(DatePart("minute", typeof(int)), sqlExpressionFactory.Constant(60))),
                            DatePart("second", typeof(int))),
                        typeof(long)),
                    sqlExpressionFactory.Constant(TimeSpan.TicksPerSecond)),
                sqlExpressionFactory.Divide(DatePart("nanosecond", typeof(int)), sqlExpressionFactory.Constant(100)));
    }
}
