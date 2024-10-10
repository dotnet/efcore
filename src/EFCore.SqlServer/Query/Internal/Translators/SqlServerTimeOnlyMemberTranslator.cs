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
public class SqlServerTimeOnlyMemberTranslator : IMemberTranslator
{
    private const string MillisecondPart = "millisecond";
    private const string MicrosecondPart = "microsecond";
    private const string NanosecondPart = "nanosecond";
    private static readonly Dictionary<string, string> DatePartMappings = new()
    {
        { nameof(TimeOnly.Hour), "hour" },
        { nameof(TimeOnly.Minute), "minute" },
        { nameof(TimeOnly.Second), "second" },
        { nameof(TimeOnly.Millisecond), MillisecondPart },
        { nameof(TimeOnly.Microsecond), MicrosecondPart },
        { nameof(TimeOnly.Nanosecond), NanosecondPart }
    };

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerTimeOnlyMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

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
        return member.DeclaringType == typeof(TimeOnly) && DatePartMappings.TryGetValue(member.Name, out var value)
             ? value switch
             {
                 MicrosecondPart => DatePartMicrosecond(),
                 NanosecondPart => DatePartNanosecond(),
                 _ => DatePart(value),
             }
            : null;

        SqlExpression DatePartMicrosecond()
            => _sqlExpressionFactory.MakeBinary(
                    ExpressionType.Subtract,
                    DatePart(MicrosecondPart),
                    _sqlExpressionFactory.MakeBinary(
                        ExpressionType.Multiply,
                        DatePart(MillisecondPart),
                        _sqlExpressionFactory.Constant(1000),
                        null)!,
                    null)!;

        SqlExpression DatePartNanosecond()
            => _sqlExpressionFactory.MakeBinary(
                    ExpressionType.Subtract,
                    DatePart(NanosecondPart),
                    _sqlExpressionFactory.MakeBinary(
                        ExpressionType.Multiply,
                        DatePart(MicrosecondPart),
                        _sqlExpressionFactory.Constant(1000),
                        null)!,
                    null)!;

        SqlExpression DatePart(string part)
            => _sqlExpressionFactory.Function(
                "DATEPART",
                arguments: [_sqlExpressionFactory.Fragment(part), instance!],
                nullable: true,
                argumentsPropagateNullability: Statics.FalseTrue,
                returnType);
    }
}
