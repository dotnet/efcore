// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
/// <remarks>
///     Some providers render literals of these types as plain strings (e.g. '0001-01-01T00:00:00.0000000'), which the database
///     then returns as text when the literal is projected. Wrapping projected literals in an explicit CAST makes the database
///     return a value of the mapped store type, which can be read back as the expected CLR type.
/// </remarks>
public class ProjectedConstantConvertingExpressionVisitor : ExpressionVisitor
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression extensionExpression)
        => extensionExpression switch
        {
            ShapedQueryExpression shapedQuery
                => shapedQuery.Update(Visit(shapedQuery.QueryExpression), Visit(shapedQuery.ShaperExpression)),

            ProjectionExpression projection
                => projection.Update(ConvertProjectedConstant((SqlExpression)Visit(projection.Expression))),

            _ => base.VisitExtension(extensionExpression)
        };

    private static SqlExpression ConvertProjectedConstant(SqlExpression expression)
        => expression switch
        {
            SqlConstantExpression { Value: not null } constant when RequiresExplicitConvert(constant.Type)
                => new SqlUnaryExpression(ExpressionType.Convert, constant, constant.Type, constant.TypeMapping),

            // The type of a CASE is derived from its results, so constant results must carry their type too.
            CaseExpression caseExpression
                => caseExpression.Update(
                    caseExpression.Operand,
                    caseExpression.WhenClauses
                        .Select(c => new CaseWhenClause(c.Test, ConvertProjectedConstant(c.Result)))
                        .ToList(),
                    caseExpression.ElseResult is null ? null : ConvertProjectedConstant(caseExpression.ElseResult)),

            _ => expression
        };

    private static bool RequiresExplicitConvert(Type type)
        => type == typeof(Guid)
            || type == typeof(DateTime)
            || type == typeof(DateOnly)
            || type == typeof(TimeOnly)
            || type == typeof(TimeSpan)
            || type == typeof(DateTimeOffset);
}
