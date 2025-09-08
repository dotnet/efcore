// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.XuGu.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal;

/// <summary>
///     MySQL &amp; MariaDB currently do not support complex expressions in HAVING clauses (e.g. function calls).
///     Instead, they want you to reference SELECT aliases for those expressions in the HAVING clause.
///     See https://bugs.mysql.com/bug.php?id=103961
///     This is only an issue for HAVING expressions that do not contain any aggregate functions.
/// </summary>
public class XGHavingExpressionVisitor : ExpressionVisitor
{
    private readonly XGSqlExpressionFactory _sqlExpressionFactory;
    private readonly XGContainsAggregateFunctionExpressionVisitor _containsAggregateFunctionExpressionVisitor;
    private bool _usePrePostprocessorMode;

    public XGHavingExpressionVisitor(XGSqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _containsAggregateFunctionExpressionVisitor = new XGContainsAggregateFunctionExpressionVisitor();
    }

    public virtual Expression Process(Expression expression, bool usePrePostprocessorMode)
    {
        _usePrePostprocessorMode = usePrePostprocessorMode;
        return Visit(expression);
    }

    protected override Expression VisitExtension(Expression extensionExpression)
        => extensionExpression switch
        {
            SelectExpression selectExpression => VisitSelect(selectExpression),
            ShapedQueryExpression shapedQueryExpression => VisitShapedQuery(shapedQueryExpression),
            RelationalGroupByShaperExpression relationalGroupByShaperExpression => VisitRelationalGroupByShaper(
                relationalGroupByShaperExpression),
            _ => base.VisitExtension(extensionExpression)
        };

    private Expression VisitRelationalGroupByShaper(RelationalGroupByShaperExpression relationalGroupByShaperExpression)
    {
        if (_usePrePostprocessorMode)
        {
            Visit(relationalGroupByShaperExpression.KeySelector);
            Visit(relationalGroupByShaperExpression.ElementSelector);
            Visit(relationalGroupByShaperExpression.GroupingEnumerable);

            return relationalGroupByShaperExpression;
        }

        return base.VisitExtension(relationalGroupByShaperExpression);
    }

    private ShapedQueryExpression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression)
    {
        if (_usePrePostprocessorMode)
        {
            Visit(shapedQueryExpression.QueryExpression);
            Visit(shapedQueryExpression.ShaperExpression);

            return shapedQueryExpression;
        }

        return shapedQueryExpression.Update(
            Visit(shapedQueryExpression.QueryExpression),
            Visit(shapedQueryExpression.ShaperExpression));
    }

    protected virtual Expression VisitSelect(SelectExpression selectExpression)
    {
        selectExpression = (SelectExpression)base.VisitExtension(selectExpression);

        var havingExpression = selectExpression.Having;

        if (HasHavingExpressionWithoutAggregateFunction(havingExpression))
        {
            if (_usePrePostprocessorMode)
            {
                // This part needs to run before `RelationalQueryTranslationPostprocessor.Process()` is called, so that the
                // `SelectExpression` is still mutable, and we can call `SelectExpression.PushdownIntoSubquery()`.

                selectExpression.PushdownIntoSubquery();

                // Paradoxically, it seems quite complicated to change the subquery, as long as the outer query is still mutable.
                // We postpone that work for later, when the outer query is immutable, and we simply use the normal expression visitor
                // update process.
            }
            else
            {
                // This part needs to run after `RelationalQueryTranslationPostprocessor.Process()` is called, so that the
                // `SelectExpression` is already immutable, and we can simply update the select subquery.

                var projectionIndex = selectExpression.AddToProjection(havingExpression!);
                var alias = selectExpression.Projection[projectionIndex].Alias;

                var columnAliasReferenceExpression = _sqlExpressionFactory.ColumnAliasReference(
                    alias,
                    havingExpression,
                    havingExpression.Type,
                    havingExpression.TypeMapping);

                // Having expressions, not containing an aggregate function, need to be part of the GROUP BY clause, because they now
                // also appear as part of the SELECT clause.
                selectExpression = selectExpression.Update(
                    selectExpression.Tables,
                    selectExpression.Predicate,
                    selectExpression.GroupBy.Append(columnAliasReferenceExpression).ToList(),
                    having: columnAliasReferenceExpression,
                    selectExpression.Projection,
                    selectExpression.Orderings,
                    selectExpression.Offset,
                    selectExpression.Limit);
            }
        }

        return selectExpression;
    }

    /// <summary>
    /// Backed by `EFCore.XG.Tests/Behaviors/HavingBehavior.cs`.
    /// </summary>
    private bool HasHavingExpressionWithoutAggregateFunction(SqlExpression havingExpression)
        => havingExpression is not null
                            and not SqlConstantExpression
                            and not XGColumnAliasReferenceExpression &&
           !_containsAggregateFunctionExpressionVisitor.ProcessUntilSelect(havingExpression);
}
