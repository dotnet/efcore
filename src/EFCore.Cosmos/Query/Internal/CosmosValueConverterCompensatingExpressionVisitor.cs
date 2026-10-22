// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosValueConverterCompensatingExpressionVisitor(ISqlExpressionFactory sqlExpressionFactory)
    : ExpressionVisitor
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
            ShapedQueryExpression shapedQueryExpression => VisitShapedQueryExpression(shapedQueryExpression),
            SelectExpression selectExpression => VisitSelect(selectExpression),
            SqlConditionalExpression sqlConditionalExpression => VisitSqlConditional(sqlConditionalExpression),
            _ => base.VisitExtension(extensionExpression)
        };

    private Expression VisitShapedQueryExpression(ShapedQueryExpression shapedQueryExpression)
        => shapedQueryExpression.UpdateQueryExpression(Visit(shapedQueryExpression.QueryExpression));

    private Expression VisitSelect(SelectExpression selectExpression)
    {
        var changed = false;

        var projections = new List<ProjectionExpression>();
        foreach (var item in selectExpression.Projection)
        {
            var updatedProjection = (ProjectionExpression)Visit(item);
            projections.Add(updatedProjection);
            changed |= updatedProjection != item;
        }

        var sources = new List<SourceExpression>();
        foreach (var item in selectExpression.Sources)
        {
            var updatedSource = (SourceExpression)Visit(item);
            sources.Add(updatedSource);
            changed |= updatedSource != item;
        }

        var predicate = TryCompensateForBoolWithValueConverter((SqlExpression?)Visit(selectExpression.Predicate));
        changed |= predicate != selectExpression.Predicate;

        var orderings = new List<OrderingExpression>();
        foreach (var ordering in selectExpression.Orderings)
        {
            var orderingExpression = (SqlExpression)Visit(ordering.Expression);
            changed |= orderingExpression != ordering.Expression;
            orderings.Add(ordering.Update(orderingExpression));
        }

        var limit = (SqlExpression?)Visit(selectExpression.Limit);
        var offset = (SqlExpression?)Visit(selectExpression.Offset);

        return changed
            ? selectExpression.Update(sources, predicate, projections, orderings, offset, limit)
            : selectExpression;
    }

    private Expression VisitSqlConditional(SqlConditionalExpression sqlConditionalExpression)
    {
        var test = TryCompensateForBoolWithValueConverter((SqlExpression)Visit(sqlConditionalExpression.Test));
        var ifTrue = (SqlExpression)Visit(sqlConditionalExpression.IfTrue);
        var ifFalse = (SqlExpression)Visit(sqlConditionalExpression.IfFalse);

        return sqlConditionalExpression.Update(test, ifTrue, ifFalse);
    }

    [return: NotNullIfNotNull(nameof(sqlExpression))]
    private SqlExpression? TryCompensateForBoolWithValueConverter(SqlExpression? sqlExpression)
        => sqlExpression switch
        {
            ScalarAccessExpression keyAccessExpression
                when keyAccessExpression.TypeMapping!.ClrType == typeof(bool) && keyAccessExpression.TypeMapping!.Converter != null
                => sqlExpressionFactory.Equal(sqlExpression, sqlExpressionFactory.Constant(true, sqlExpression.TypeMapping)),

            SqlUnaryExpression sqlUnaryExpression
                => sqlUnaryExpression.Update(TryCompensateForBoolWithValueConverter(sqlUnaryExpression.Operand)),

            SqlBinaryExpression { OperatorType: ExpressionType.AndAlso or ExpressionType.OrElse } sqlBinaryExpression
                => sqlBinaryExpression.Update(
                    TryCompensateForBoolWithValueConverter(sqlBinaryExpression.Left),
                    TryCompensateForBoolWithValueConverter(sqlBinaryExpression.Right)),

            _ => sqlExpression
        };
}
