// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosDateTimeMemberTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMemberTranslator
{
    private const string MillisecondPart = "ms";
    private const string MicrosecondPart = "mcs";
    private const string NanosecondPart = "ns";

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
            nameof(DateTime.Year) => DateTimePart("yyyy"),
            nameof(DateTime.Month) => DateTimePart("mm"),
            nameof(DateTime.Day) => DateTimePart("dd"),
            nameof(DateTime.Hour) => DateTimePart("hh"),
            nameof(DateTime.Minute) => DateTimePart("mi"),
            nameof(DateTime.Second) => DateTimePart("ss"),
            nameof(DateTime.Millisecond) => DateTimePart(MillisecondPart),
            nameof(DateTime.Microsecond) => DateTimePartMicrosecond(),
            nameof(DateTime.Nanosecond) => DateTimePartNanosecond(),

            nameof(DateTime.UtcNow)
                => sqlExpressionFactory.Function(
                    "GetCurrentDateTime",
                    [],
                    returnType),

            _ => null
        };

        SqlExpression DateTimePartMicrosecond()
            => sqlExpressionFactory.MakeBinary(
                    ExpressionType.Subtract,
                    DateTimePart(MicrosecondPart),
                    sqlExpressionFactory.MakeBinary(
                        ExpressionType.Multiply,
                        DateTimePart(MillisecondPart),
                        sqlExpressionFactory.Constant(1000),
                        null)!,
                    null)!;

        SqlExpression DateTimePartNanosecond()
            => sqlExpressionFactory.MakeBinary(
                    ExpressionType.Subtract,
                    DateTimePart(NanosecondPart),
                    sqlExpressionFactory.MakeBinary(
                        ExpressionType.Multiply,
                        DateTimePart(MicrosecondPart),
                        sqlExpressionFactory.Constant(1000),
                        null)!,
                    null)!;

        SqlExpression DateTimePart(string part)
            => sqlExpressionFactory.Function(
                "DateTimePart",
                arguments: [sqlExpressionFactory.Constant(part), instance!],
                returnType);
    }
}
