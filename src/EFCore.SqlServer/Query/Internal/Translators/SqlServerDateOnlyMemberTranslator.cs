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
public class SqlServerDateOnlyMemberTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMemberTranslator
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
        if (member.DeclaringType != typeof(DateOnly) || instance is null)
        {
            return null;
        }

        return member.Name switch
        {
            nameof(DateOnly.Year) => DatePart("year"),
            nameof(DateOnly.Month) => DatePart("month"),
            nameof(DateOnly.DayOfYear) => DatePart("dayofyear"),
            nameof(DateOnly.Day) => DatePart("day"),

            nameof(DateOnly.DayNumber) => sqlExpressionFactory.Function(
                "DATEDIFF",
                [
                    sqlExpressionFactory.Fragment("day"),
                    sqlExpressionFactory.Constant(new DateOnly(1, 1, 1)),
                    instance
                ],
                nullable: true,
                argumentsPropagateNullability: [false, true, true],
                returnType),

            _ => null
        };

        SqlExpression DatePart(string datePart)
            => sqlExpressionFactory.Function(
                "DATEPART",
                [sqlExpressionFactory.Fragment(datePart), instance],
                nullable: true,
                argumentsPropagateNullability: Statics.FalseTrue,
                returnType);
    }
}
