// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     SQLite doesn't support APPLY, but its table-valued functions (e.g. json_each) may reference columns of preceding tables in the
///     FROM clause, which is what APPLY is generally needed for. This visitor rewrites CROSS/OUTER APPLY over a trivial subquery whose
///     only table is such a function into a regular CROSS/LEFT JOIN over that function, inlining the subquery's projection into the
///     containing SELECT.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class SqliteApplyFlatteningExpressionVisitor(ISqlExpressionFactory sqlExpressionFactory) : ExpressionVisitor
{
    // OUTER APPLY keeps rows whose function yields nothing, so it becomes a LEFT JOIN that always matches.
    private readonly SqlExpression _alwaysTrue =
        sqlExpressionFactory.ApplyDefaultTypeMapping(sqlExpressionFactory.Constant(true))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression node)
    {
        // ShapedQueryExpression doesn't support VisitChildren, its parts have to be visited explicitly.
        if (node is ShapedQueryExpression shapedQuery)
        {
            return shapedQuery
                .UpdateQueryExpression(Visit(shapedQuery.QueryExpression))
                .UpdateShaperExpression(Visit(shapedQuery.ShaperExpression));
        }

        var visited = base.VisitExtension(node);

        return visited is SelectExpression select ? TryFlattenApplies(select) : visited;
    }

    private Expression TryFlattenApplies(SelectExpression select)
    {
        List<TableExpressionBase>? newTables = null;
        Dictionary<string, IReadOnlyDictionary<string, SqlExpression>>? inlinedProjections = null;

        for (var i = 0; i < select.Tables.Count; i++)
        {
            if (select.Tables[i] is not JoinExpressionBase { Table: SelectExpression applied } join
                || join is not (CrossApplyExpression or OuterApplyExpression)
                || !IsFlattenable(applied))
            {
                continue;
            }

            // The subquery is a trivial wrapper over a table-valued function; SQLite can reference the outer tables from the function's
            // arguments directly, so the wrapper can be removed and the join rewritten.
            var projectionMap = applied.Projection.ToDictionary(p => p.Alias, p => p.Expression);

            newTables ??= [.. select.Tables];
            newTables[i] = join is CrossApplyExpression
                ? new CrossJoinExpression(applied.Tables[0])
                : new LeftJoinExpression(applied.Tables[0], _alwaysTrue);

            inlinedProjections ??= [];
            inlinedProjections[applied.Alias!] = projectionMap;
        }

        if (newTables is null)
        {
            return select;
        }

        var inliner = new ProjectionInliningVisitor(inlinedProjections!);

        return select.Update(
            newTables,
            (SqlExpression?)inliner.Visit(select.Predicate),
            select.GroupBy.Select(g => (SqlExpression)inliner.Visit(g)).ToList(),
            (SqlExpression?)inliner.Visit(select.Having),
            select.Projection.Select(p => (ProjectionExpression)inliner.Visit(p)).ToList(),
            select.Orderings.Select(o => (OrderingExpression)inliner.Visit(o)).ToList(),
            (SqlExpression?)inliner.Visit(select.Offset),
            (SqlExpression?)inliner.Visit(select.Limit));
    }

    /// <summary>
    ///     A subquery can be unwrapped only if it does nothing beyond projecting out of a single table-valued function; anything else
    ///     (filtering, ordering, paging, grouping) would change the meaning of the query once lifted into the containing SELECT.
    /// </summary>
    private static bool IsFlattenable(SelectExpression select)
        => select is
        {
            Tables: [JsonEachExpression],
            Predicate: null,
            GroupBy: [],
            Having: null,
            Orderings: [],
            Limit: null,
            Offset: null,
            IsDistinct: false
        };

    private sealed class ProjectionInliningVisitor(
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, SqlExpression>> inlinedProjections) : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
            => node is ColumnExpression column
                && inlinedProjections.TryGetValue(column.TableAlias, out var projections)
                && projections.TryGetValue(column.Name, out var inlined)
                    ? inlined
                    : base.VisitExtension(node);
    }
}
