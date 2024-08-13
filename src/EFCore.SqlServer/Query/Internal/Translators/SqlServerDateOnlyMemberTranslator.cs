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
public class SqlServerDateOnlyMemberTranslator : IMemberTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerDateOnlyMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

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
        if (member.DeclaringType != typeof(DateOnly))
        {
            return null;
        }

        return member.Name switch
        {
            nameof(DateOnly.Year) => Function(instance, returnType, "year"),
            nameof(DateOnly.Month) => Function(instance, returnType, "month"),
            nameof(DateOnly.DayOfYear) => Function(instance, returnType, "dayofyear"),
            nameof(DateOnly.Day) => Function(instance, returnType, "day"),
            _ => null
        };
    }

    private SqlExpression Function(SqlExpression? instance, Type returnType, string datePart)
        => _sqlExpressionFactory.Function(
            "DATEPART",
            [_sqlExpressionFactory.Fragment(datePart), instance!],
            nullable: true,
            argumentsPropagateNullability: [false, true],
            returnType);
}
