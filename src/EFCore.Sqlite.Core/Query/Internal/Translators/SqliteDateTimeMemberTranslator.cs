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
                return sqlExpressionFactory.Convert(
                    sqlExpressionFactory.Multiply(
                        sqlExpressionFactory.Subtract(
                            sqlExpressionFactory.Function(
                                "julianday",
                                new[] { instance! },
                                nullable: true,
                                argumentsPropagateNullability: new[] { true },
                                typeof(double)),
                            sqlExpressionFactory.Constant(1721425.5)), // NB: Result of julianday('0001-01-01 00:00:00')
                        sqlExpressionFactory.Constant(TimeSpan.TicksPerDay)),
                    typeof(long));

            case nameof(DateTime.Millisecond):
                return sqlExpressionFactory.Modulo(
                    sqlExpressionFactory.Multiply(
                        sqlExpressionFactory.Convert(
                            sqlExpressionFactory.Strftime(
                                typeof(string),
                                "%f",
                                instance!),
                            typeof(double)),
                        sqlExpressionFactory.Constant(1000)),
                    sqlExpressionFactory.Constant(1000));
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
            new[]
            {
                sqlExpressionFactory.Function(
                    "rtrim",
                    new[]
                    {
                        sqlExpressionFactory.Strftime(
                            returnType,
                            format,
                            timestring,
                            modifiers),
                        sqlExpressionFactory.Constant("0")
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true, false },
                    returnType),
                sqlExpressionFactory.Constant(".")
            },
            nullable: true,
            argumentsPropagateNullability: new[] { true, false },
            returnType);

        SqlExpression DatePart(string part)
            => sqlExpressionFactory.Convert(
                sqlExpressionFactory.Strftime(typeof(string), part, instance!),
                returnType);
    }
}
