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
public class SqliteTimeOnlyMemberTranslator(SqliteSqlExpressionFactory sqlExpressionFactory) : IMemberTranslator
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
        return member.DeclaringType != typeof(TimeOnly)
            || instance is null
            || !SqliteSqlExpressionFactory.IsStoredAs(instance, typeof(TimeOnly))
            ? null
            : member.Name switch
            {
                nameof(TimeOnly.Hour) => DatePart("%H"),
                nameof(TimeOnly.Minute) => DatePart("%M"),
                nameof(TimeOnly.Second) => DatePart("%S"),

                // strftime('%f') rounds to milliseconds, so the digits are read from the text instead. Padding covers values
                // without a fraction or with fewer than seven digits.
                nameof(TimeOnly.Millisecond) => FractionPart(10, 3),
                nameof(TimeOnly.Microsecond) => FractionPart(13, 3),
                nameof(TimeOnly.Nanosecond) => sqlExpressionFactory.Multiply(FractionPart(16, 1), sqlExpressionFactory.Constant(100)),

                _ => null
            };

        SqlExpression DatePart(string datePart)
            => sqlExpressionFactory.Convert(
                sqlExpressionFactory.Strftime(
                    typeof(string),
                    datePart,
                    instance),
                returnType);

        SqlExpression FractionPart(int start, int length)
        {
            var padding = sqlExpressionFactory.ApplyDefaultTypeMapping(sqlExpressionFactory.Constant("0000000"));

            var fraction = sqlExpressionFactory.Convert(
                sqlExpressionFactory.Function(
                    "substr",
                    [
                        new SqlBinaryExpression(ExpressionType.Add, instance, padding, typeof(string), padding.TypeMapping),
                        sqlExpressionFactory.Constant(start),
                        sqlExpressionFactory.Constant(length)
                    ],
                    nullable: true,
                    argumentsPropagateNullability: Statics.TrueArrays[3],
                    typeof(string)),
                returnType);

            // Null protection on the padding concatenation would turn a NULL time into '', so NULL is propagated explicitly.
            return sqlExpressionFactory.Case(
                [new CaseWhenClause(sqlExpressionFactory.IsNotNull(instance), fraction)],
                elseResult: null);
        }
    }
}
