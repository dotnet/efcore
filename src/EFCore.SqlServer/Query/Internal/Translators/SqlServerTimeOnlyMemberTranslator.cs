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
public class SqlServerTimeOnlyMemberTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMemberTranslator
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

        if (declaringType != typeof(TimeOnly))
        {
            return null;
        }

        return member.Name switch
        {
            nameof(TimeOnly.Hour) => DatePart("hour"),
            nameof(TimeOnly.Minute) => DatePart("minute"),
            nameof(TimeOnly.Second) => DatePart("second"),
            nameof(TimeOnly.Millisecond) => DatePart("millisecond"),
            nameof(TimeOnly.Microsecond) => sqlExpressionFactory.Modulo(DatePart("microsecond"), sqlExpressionFactory.Constant(1000)),
            nameof(TimeOnly.Nanosecond) => sqlExpressionFactory.Modulo(DatePart("nanosecond"), sqlExpressionFactory.Constant(1000)),
            _ => null,
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
