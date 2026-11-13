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
public class SqliteDateTimeMemberTranslator(SqliteSqlExpressionFactory sqlExpressionFactory) : IMemberTranslator
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
        if (member.DeclaringType != typeof(DateTime))
        {
            return null;
        }

        var memberName = member.Name;

        switch (memberName)
        {
            case nameof(DateTime.Year):
                return DatePart("%Y");
            case nameof(DateTime.Month):
                return DatePart("%m");
            case nameof(DateTime.DayOfYear):
                return DatePart("%j");
            case nameof(DateTime.Day):
                return DatePart("%d");
            case nameof(DateTime.Hour):
                return DatePart("%H");
            case nameof(DateTime.Minute):
                return DatePart("%M");
            case nameof(DateTime.Second):
                return DatePart("%S");
            case nameof(DateTime.DayOfWeek):
                return DatePart("%w");

            case nameof(DateTime.Ticks):
                return TranslateTicks(instance!);

            case nameof(DateTime.Millisecond):
                // strftime('%f') returns SS.SSS; take the milliseconds digits directly, since multiplying the REAL value by 1000
                // and truncating loses a millisecond for values that can't be represented exactly (e.g. 01.005).
                return sqlExpressionFactory.Convert(
                    sqlExpressionFactory.Function(
                        "substr",
                        [
                            sqlExpressionFactory.Strftime(
                                typeof(string),
                                "%f",
                                instance!),
                            sqlExpressionFactory.Constant(4)
                        ],
                        nullable: true,
                        argumentsPropagateNullability: Statics.TrueArrays[2],
                        typeof(string)),
                    returnType);

            case nameof(DateTime.Microsecond):
            case nameof(DateTime.Nanosecond):
                return null;
        }

        var format = "%Y-%m-%d %H:%M:%f";
        SqlExpression timestring;
        var modifiers = new List<SqlExpression>();

        switch (memberName)
        {
            case nameof(DateTime.Now):
                timestring = sqlExpressionFactory.Constant("now");
                modifiers.Add(sqlExpressionFactory.Constant("localtime"));
                break;

            case nameof(DateTime.UtcNow):
                timestring = sqlExpressionFactory.Constant("now");
                break;

            case nameof(DateTime.Date):
                timestring = instance!;
                modifiers.Add(sqlExpressionFactory.Constant("start of day"));
                break;

            case nameof(DateTime.Today):
                timestring = sqlExpressionFactory.Constant("now");
                modifiers.Add(sqlExpressionFactory.Constant("localtime"));
                modifiers.Add(sqlExpressionFactory.Constant("start of day"));
                break;

            case nameof(DateTime.TimeOfDay):
                format = "%H:%M:%f";
                timestring = instance!;
                break;

            default:
                return null;
        }

        Check.DebugAssert(timestring != null, "timestring is null");

        return sqlExpressionFactory.Function(
            "rtrim",
            [
                sqlExpressionFactory.Function(
                    "rtrim",
                    [
                        sqlExpressionFactory.Strftime(
                            returnType,
                            format,
                            timestring,
                            modifiers),
                        sqlExpressionFactory.Constant("0")
                    ],
                    nullable: true,
                    argumentsPropagateNullability: Statics.TrueFalse,
                    returnType),
                sqlExpressionFactory.Constant(".")
            ],
            nullable: true,
            argumentsPropagateNullability: Statics.TrueFalse,
            returnType);

        SqlExpression DatePart(string part)
            => sqlExpressionFactory.Convert(
                sqlExpressionFactory.Strftime(typeof(string), part, instance!),
                returnType);
    }

    private SqlExpression TranslateTicks(SqlExpression instance)
    {
        if (instance is ColumnExpression { TypeMapping: SqliteDateTimeTypeMapping { Converter: null } })
        {
            var seconds = sqlExpressionFactory.Add(
                sqlExpressionFactory.Convert(
                    sqlExpressionFactory.Strftime(
                        typeof(string),
                        "%s",
                        sqlExpressionFactory.Function(
                            "substr",
                            [instance, sqlExpressionFactory.Constant(1), sqlExpressionFactory.Constant(DateTimeSecondsLength)],
                            nullable: true,
                            argumentsPropagateNullability: [true, false, false],
                            typeof(string))),
                    typeof(long)),
                sqlExpressionFactory.Constant(UnixEpochSeconds));

            var fraction = sqlExpressionFactory.Convert(
                sqlExpressionFactory.Function(
                    "substr",
                    [
                        sqlExpressionFactory.Add(
                            sqlExpressionFactory.Function(
                                "substr",
                                [instance, sqlExpressionFactory.Constant(DateTimeSecondsLength + 2)],
                                nullable: true,
                                argumentsPropagateNullability: Statics.TrueFalse,
                                typeof(string)),
                            sqlExpressionFactory.Constant("0000000")),
                        sqlExpressionFactory.Constant(1),
                        sqlExpressionFactory.Constant(7)
                    ],
                    nullable: true,
                    argumentsPropagateNullability: [true, false, false],
                    typeof(string)),
                typeof(long));

            return sqlExpressionFactory.Add(
                sqlExpressionFactory.Multiply(seconds, sqlExpressionFactory.Constant(TimeSpan.TicksPerSecond)),
                fraction);
        }

        return sqlExpressionFactory.Multiply(
            sqlExpressionFactory.Subtract(
                sqlExpressionFactory.Convert(
                    sqlExpressionFactory.Function(
                        "round",
                        [
                            sqlExpressionFactory.Multiply(
                                sqlExpressionFactory.Function(
                                    "julianday",
                                    [instance],
                                    nullable: true,
                                    argumentsPropagateNullability: Statics.TrueArrays[1],
                                    typeof(double)),
                                sqlExpressionFactory.Constant((double)MillisecondsPerDay))
                        ],
                        nullable: true,
                        argumentsPropagateNullability: Statics.TrueArrays[1],
                        typeof(double)),
                    typeof(long)),
                sqlExpressionFactory.Constant(JulianDayEpochMilliseconds)),
            sqlExpressionFactory.Constant(TimeSpan.TicksPerMillisecond));
    }

    private const int DateTimeSecondsLength = 19;

    private const long UnixEpochSeconds = 62135596800L;

    private const long MillisecondsPerDay = 86400000L;

    private const long JulianDayEpochMilliseconds = 148731163200000L;
}
