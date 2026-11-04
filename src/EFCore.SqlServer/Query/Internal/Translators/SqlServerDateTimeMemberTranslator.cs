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

        return declaringType != typeof(DateTime) && declaringType != typeof(DateTimeOffset)
            ? null
            : member.Name switch
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

                nameof(DateTime.Date)
                    => sqlExpressionFactory.Function(
                        "CONVERT",
                        [sqlExpressionFactory.Fragment("date"), instance!],
                        nullable: true,
                        argumentsPropagateNullability: Statics.FalseTrue,
                        returnType,
                        declaringType == typeof(DateTime)
                            ? instance!.TypeMapping
                            : typeMappingSource.FindMapping(typeof(DateTime))),

                nameof(DateTime.TimeOfDay)
                    => sqlExpressionFactory.Function(
                        "CONVERT",
                        [sqlExpressionFactory.Fragment("time"), instance!],
                        nullable: true,
                        argumentsPropagateNullability: Statics.FalseTrue,
                        returnType),

                nameof(DateTimeOffset.DateTime)
                    when declaringType == typeof(DateTimeOffset)
                    && instance!.TypeMapping is { StoreTypeNameBase: "datetimeoffset" }
                    => sqlExpressionFactory.Function(
                        "CONVERT",
                        [sqlExpressionFactory.Fragment("datetime2"), instance!],
                        nullable: true,
                        argumentsPropagateNullability: Statics.FalseTrue,
                        returnType,
                        typeMappingSource.FindMapping(typeof(DateTime))),

                nameof(DateTimeOffset.UtcDateTime)
                    when declaringType == typeof(DateTimeOffset)
                    && instance!.TypeMapping is { StoreTypeNameBase: "datetimeoffset" }
                    => sqlExpressionFactory.Function(
                        "CONVERT",
                        [
                            sqlExpressionFactory.Fragment("datetime2"),
                            new AtTimeZoneExpression(
                                instance!,
                                sqlExpressionFactory.ApplyTypeMapping(
                                    sqlExpressionFactory.Constant("UTC"),
                                    typeMappingSource.FindMapping("varchar")),
                                typeof(DateTimeOffset),
                                instance!.TypeMapping)
                        ],
                        nullable: true,
                        argumentsPropagateNullability: Statics.FalseTrue,
                        returnType,
                        typeMappingSource.FindMapping(typeof(DateTime))),

                nameof(DateTimeOffset.LocalDateTime)
                    when declaringType == typeof(DateTimeOffset)
                    && instance!.TypeMapping is { StoreTypeNameBase: "datetimeoffset" }
                    => sqlExpressionFactory.Function(
                        "CONVERT",
                        [
                            sqlExpressionFactory.Fragment("datetime2"),
                            new AtTimeZoneExpression(
                                instance!,
                                sqlExpressionFactory.Function(
                                    "CURRENT_TIMEZONE_ID",
                                    arguments: [],
                                    nullable: false,
                                    argumentsPropagateNullability: [],
                                    typeof(string),
                                    typeMappingSource.FindMapping("varchar")),
                                typeof(DateTimeOffset),
                                instance!.TypeMapping)
                        ],
                        nullable: true,
                        argumentsPropagateNullability: Statics.FalseTrue,
                        returnType,
                        typeMappingSource.FindMapping(typeof(DateTime))),

                nameof(DateTime.Now)
                    when declaringType == typeof(DateTime)
                    => sqlExpressionFactory.Function(
                        "GETDATE",
                        arguments: [],
                        nullable: false,
                        argumentsPropagateNullability: [],
                        typeof(DateTime),
                        typeMappingSource.FindMapping("datetime")),

                nameof(DateTimeOffset.Now)
                    when declaringType == typeof(DateTimeOffset)
                    => sqlExpressionFactory.Function(
                        "SYSDATETIMEOFFSET",
                        arguments: [],
                        nullable: false,
                        argumentsPropagateNullability: [],
                        typeof(DateTimeOffset),
                        typeMappingSource.FindMapping("datetimeoffset")),

                nameof(DateTime.UtcNow)
                    when declaringType == typeof(DateTime)
                    => sqlExpressionFactory.Function(
                        "GETUTCDATE",
                        arguments: [],
                        nullable: false,
                        argumentsPropagateNullability: [],
                        typeof(DateTime),
                        typeMappingSource.FindMapping("datetime")),

                nameof(DateTimeOffset.UtcNow)
                    when declaringType == typeof(DateTimeOffset)
                    => sqlExpressionFactory.Convert(
                        sqlExpressionFactory.Function(
                            "SYSUTCDATETIME",
                            arguments: [],
                            nullable: false,
                            argumentsPropagateNullability: [],
                            typeof(DateTime),
                            typeMappingSource.FindMapping("datetime2")),
                        typeof(DateTimeOffset),
                        typeMappingSource.FindMapping("datetimeoffset")),

                nameof(DateTime.Today)
                    => sqlExpressionFactory.Function(
                        "CONVERT",
                        [
                            sqlExpressionFactory.Fragment("date"),
                            sqlExpressionFactory.Function(
                                "GETDATE",
                                arguments: [],
                                nullable: false,
                                argumentsPropagateNullability: [],
                                typeof(DateTime))
                        ],
                        nullable: true,
                        argumentsPropagateNullability: Statics.FalseTrue,
                        returnType),

                nameof(DateTime.Ticks)
                    when declaringType == typeof(DateTime)
                    && instance is ColumnExpression or SqlParameterExpression
                    => Ticks(instance),

                _ => null
            };

        // DATEDIFF_BIG isn't available before SQL Server 2016, so the ticks are built from days, seconds within the day and
        // the fraction of the second, each of which fits in an int.
        SqlExpression? Ticks(SqlExpression dateTime)
        {
            // Parameters reach member translators without a type mapping.
            dateTime = sqlExpressionFactory.ApplyDefaultTypeMapping(dateTime);
            // The type mapping keeps the store type as configured, which SQL Server accepts in any case and in brackets.
            var storeType = dateTime.TypeMapping?.StoreTypeNameBase.Trim('[', ']').ToLowerInvariant();
            if (storeType is not ("datetime2" or "datetime" or "smalldatetime" or "date"))
            {
                return null;
            }

            var days = DateDiff("day", sqlExpressionFactory.Constant(DateTime.MinValue, typeMappingSource.FindMapping("datetime2")));
            var ticks = Multiply(days, TimeSpan.TicksPerDay);

            if (storeType == "date")
            {
                return ticks;
            }

            var seconds = DateDiff(
                "second",
                sqlExpressionFactory.Function(
                    "CONVERT",
                    [sqlExpressionFactory.Fragment("date"), dateTime],
                    nullable: true,
                    argumentsPropagateNullability: Statics.FalseTrue,
                    typeof(DateTime)));
            ticks = sqlExpressionFactory.Add(ticks, Multiply(seconds, TimeSpan.TicksPerSecond));

            if (storeType == "smalldatetime")
            {
                return ticks;
            }

            var nanoseconds = sqlExpressionFactory.Function(
                "DATEPART",
                [sqlExpressionFactory.Fragment("nanosecond"), dateTime],
                nullable: true,
                argumentsPropagateNullability: Statics.FalseTrue,
                typeof(int));

            // datetime is stored in 1/300 s units, which SqlClient rounds to the nearest millisecond when reading a DateTime.
            var fraction = storeType == "datetime"
                ? sqlExpressionFactory.Multiply(
                    sqlExpressionFactory.Divide(
                        sqlExpressionFactory.Add(nanoseconds, sqlExpressionFactory.Constant(500_000)),
                        sqlExpressionFactory.Constant(1_000_000)),
                    sqlExpressionFactory.Constant((int)TimeSpan.TicksPerMillisecond))
                : sqlExpressionFactory.Divide(nanoseconds, sqlExpressionFactory.Constant(100));

            return sqlExpressionFactory.Add(ticks, sqlExpressionFactory.Convert(fraction, typeof(long)));

            SqlExpression DateDiff(string part, SqlExpression start)
                => sqlExpressionFactory.Function(
                    "DATEDIFF",
                    [sqlExpressionFactory.Fragment(part), start, dateTime],
                    nullable: true,
                    argumentsPropagateNullability: [false, true, true],
                    typeof(int));

            SqlExpression Multiply(SqlExpression value, long factor)
                => sqlExpressionFactory.Multiply(
                    sqlExpressionFactory.Convert(value, typeof(long)),
                    sqlExpressionFactory.Constant(factor));
        }

        SqlExpression DatePart(string part)
            => sqlExpressionFactory.Function(
                "DATEPART",
                arguments: [sqlExpressionFactory.Fragment(part), instance!],
                nullable: true,
                argumentsPropagateNullability: Statics.FalseTrue,
                returnType);
    }
}
