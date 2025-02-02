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
public class SqlServerDateTimeMemberTranslator(
    ISqlExpressionFactory sqlExpressionFactory,
    IRelationalTypeMappingSource typeMappingSource)
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

        if (declaringType != typeof(DateTime) && declaringType != typeof(DateTimeOffset))
        {
            return null;
        }

        return member.Name switch
        {
            nameof(DateTime.Year) => DatePart("year"),
            nameof(DateTime.Month) => DatePart("month"),
            nameof(DateTime.DayOfYear) => DatePart("dayofyear"),
            nameof(DateTime.Day) => DatePart("day"),
            nameof(DateTime.Hour) => DatePart("hour"),
            nameof(DateTime.Minute) => DatePart("minute"),
            nameof(DateTime.Second) => DatePart("second"),
            nameof(DateTime.Millisecond) => DatePart("millisecond"),
            nameof(DateTime.Microsecond) => sqlExpressionFactory.Modulo(DatePart("microsecond"), sqlExpressionFactory.Constant(1000)),
            nameof(DateTime.Nanosecond) => sqlExpressionFactory.Modulo(DatePart("nanosecond"), sqlExpressionFactory.Constant(1000)),
            //Use datediff to get the minute difference of 0001-01-01 00:00:00.0, then multiply it by the scale of each minute,
            //and then get the scale of seconds, milliseconds, microseconds, and nanoseconds respectively, and finally add them together.
            //The nanoseconds obtained by DATEPART include milliseconds and microseconds.
            nameof(DateTime.Ticks)
                => sqlExpressionFactory.Add(
                    sqlExpressionFactory.Multiply(
                        sqlExpressionFactory.Function(
                            "DATEDIFF",
                            arguments: new[]
                            {
                                sqlExpressionFactory.Fragment("minute"),
                                sqlExpressionFactory.Constant("0001-01-01T00:00:00.0"),
                                instance!
                            },
                            nullable: true,
                            argumentsPropagateNullability: new[] { false, false, true },
                            typeof(long)),
                        sqlExpressionFactory.Constant(600000000)),
                    sqlExpressionFactory.Add(
                        sqlExpressionFactory.Multiply(
                            sqlExpressionFactory.Function(
                                "DATEPART",
                                arguments: new[] { sqlExpressionFactory.Fragment("second"), instance! },
                                nullable: true,
                                argumentsPropagateNullability: Statics.FalseTrue,
                                typeof(int)),
                            sqlExpressionFactory.Constant(10000000)),
                        sqlExpressionFactory.Divide(
                            sqlExpressionFactory.Function(
                                "DATEPART",
                                arguments: new[] { sqlExpressionFactory.Fragment("nanosecond"), instance! },
                                nullable: true,
                                argumentsPropagateNullability: Statics.FalseTrue,
                                typeof(int)),
                            sqlExpressionFactory.Constant(100)))),

            nameof(DateTime.Date)
                => sqlExpressionFactory.Function(
                    "CONVERT",
                    new[] { sqlExpressionFactory.Fragment("date"), instance! },
                    nullable: true,
                    argumentsPropagateNullability: Statics.FalseTrue,
                    returnType,
                    declaringType == typeof(DateTime)
                        ? instance!.TypeMapping
                        : typeMappingSource.FindMapping(typeof(DateTime))),

            nameof(DateTime.TimeOfDay)
                => sqlExpressionFactory.Function(
                    "CONVERT",
                    new[] { sqlExpressionFactory.Fragment("time"), instance! },
                    nullable: true,
                    argumentsPropagateNullability: Statics.FalseTrue,
                    returnType),

            nameof(DateTime.Now)
                => sqlExpressionFactory.Function(
                    declaringType == typeof(DateTime) ? "GETDATE" : "SYSDATETIMEOFFSET",
                    arguments: [],
                    nullable: false,
                    argumentsPropagateNullability: [],
                    returnType),

            nameof(DateTime.UtcNow)
                when declaringType == typeof(DateTime)
                => sqlExpressionFactory.Function(
                    "GETUTCDATE",
                    arguments: [],
                    nullable: false,
                    argumentsPropagateNullability: [],
                    returnType),

            nameof(DateTime.UtcNow)
                when declaringType == typeof(DateTimeOffset)
                => sqlExpressionFactory.Convert(
                    sqlExpressionFactory.Function(
                        "SYSUTCDATETIME",
                        arguments: [],
                        nullable: false,
                        argumentsPropagateNullability: [],
                        returnType), returnType),

            nameof(DateTime.Today)
                => sqlExpressionFactory.Function(
                    "CONVERT",
                    new[]
                    {
                        sqlExpressionFactory.Fragment("date"),
                        sqlExpressionFactory.Function(
                            "GETDATE",
                            arguments: [],
                            nullable: false,
                            argumentsPropagateNullability: [],
                            typeof(DateTime))
                    },
                    nullable: true,
                    argumentsPropagateNullability: Statics.FalseTrue,
                    returnType),

            _ => null
        };

        SqlExpression DatePart(string part)
            => sqlExpressionFactory.Function(
                "DATEPART",
                arguments: [sqlExpressionFactory.Fragment(part), instance!],
                nullable: true,
                argumentsPropagateNullability: Statics.FalseTrue,
                returnType);
    }
}
