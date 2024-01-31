// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         A visitor that processes a SQL tree and prunes out parts which aren't needed.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class SqlTreePruner : ExpressionVisitor
{
    private readonly Dictionary<string, HashSet<string>> _referencedColumnMap = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Maps table aliases to the list of column aliases found referenced on them.
    /// </summary>
    protected virtual IReadOnlyDictionary<string, HashSet<string>> ReferencedColumnMap => _referencedColumnMap;

    /// <summary>
    ///     When visiting a nested <see cref="TableExpressionBase" /> (e.g. a select within a set operation), this holds the table alias
    ///     of the top-most table (the one which has the alias referenced by columns). This is needed in order to properly prune the
    ///     projection of such nested selects, which don't themselves have an alias.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    protected virtual string? CurrentTableAlias { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression Prune(Expression expression)
        => Visit(expression);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression node)
    {
        switch (node)
        {
            case ShapedQueryExpression shapedQueryExpression:
                _referencedColumnMap.Clear();
                return shapedQueryExpression.Update(
                    PruneToplevelSelect((SelectExpression)shapedQueryExpression.QueryExpression),
                    Visit(shapedQueryExpression.ShaperExpression));

            case RelationalSplitCollectionShaperExpression relationalSplitCollectionShaperExpression:
                _referencedColumnMap.Clear();
                return relationalSplitCollectionShaperExpression.Update(
                    relationalSplitCollectionShaperExpression.ParentIdentifier,
                    relationalSplitCollectionShaperExpression.ChildIdentifier,
                    PruneToplevelSelect(relationalSplitCollectionShaperExpression.SelectExpression),
                    Visit(relationalSplitCollectionShaperExpression.InnerShaper));

            case DeleteExpression deleteExpression:
                return deleteExpression.Update(deleteExpression.Table, PruneToplevelSelect(deleteExpression.SelectExpression));

            case UpdateExpression updateExpression:
                // Note that we must visit the setters before we visit the select, since the setters can reference tables inside it.
                var visitedSetters = updateExpression.ColumnValueSetters
                    .Select(e => e with { Value = (SqlExpression)Visit(e.Value) })
                    .ToList();
                return updateExpression.Update(PruneToplevelSelect(updateExpression.SelectExpression), visitedSetters);

            // The following remaining cases deal with recursive visitation (i.e. non-top-level things)

            // For any column we encounter, register it in the referenced column map, which records the aliases referenced on each table.
            case ColumnExpression column:
                RegisterTable(column.TableAlias, column);

                return column;

            // Note that this only handles nested selects, and *not* the top-level select - that was already handled above in the first
            // cases.
            case SelectExpression select:
                return PruneSelect(select, preserveProjection: false);

            // PredicateJoinExpressionBase.VisitChildren visits the table before the predicate, but we must visit the predicate first
            // since it can contain columns referencing the table's projection (which we shouldn't prune).
            case PredicateJoinExpressionBase join:
                var joinPredicate = (SqlExpression)Visit(join.JoinPredicate);
                var table = (TableExpressionBase)Visit(join.Table);
                return join.Update(table, joinPredicate);

            // Never prune the projection of a scalar subquery. Note that there are never columns referencing scalar subqueries, since
            // they're not tables.
            case ScalarSubqueryExpression scalarSubquery:
                return scalarSubquery.Update(PruneSelect(scalarSubquery.Subquery, preserveProjection: true));

            // Same for subqueries inside InExpression
            case InExpression { Subquery: SelectExpression subquery } inExpression:
                var visitedItem = (SqlExpression)Visit(inExpression.Item);
                var visitedSubquery = PruneSelect(subquery, preserveProjection: true);
                return inExpression.Update(visitedItem, visitedSubquery);

            // If the set operation is distinct (union/intersect/except, but not concat), we cannot prune the projection since that would
            // affect which rows come out.
            // Also, even if the set operation is non-distinct but one side is distinct, we avoid pruning the projection since that could
            // make the two sides have different and incompatible projections (see #30273).
            // Note that we still visit to prune within the set operation, and to make sure the referenced column map gets updated.
            case SetOperationBase { Source1: var source1, Source2: var source2 } setOperation
                when setOperation.IsDistinct || source1.IsDistinct || source2.IsDistinct:
            {
                return setOperation.Update(
                    PruneSelect(source1, preserveProjection: true),
                    PruneSelect(source2, preserveProjection: true));
            }

            default:
                return base.VisitExtension(node);
        }

        void RegisterTable(string tableAlias, ColumnExpression column)
            => _referencedColumnMap.GetOrAddNew(tableAlias).Add(column.Name);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual SelectExpression PruneToplevelSelect(SelectExpression select)
    {
        // TODO: This doesn't belong in pruning, take a deeper look at how we manage TPC, #32873
        select = select.RemoveTpcTableExpression();
        return PruneSelect(select, preserveProjection: true);
    }

    /// <summary>
    ///     Prunes a <see cref="SelectExpression" />, removes tables inside it which aren't referenced, and optionally also projections
    ///     which aren't referenced from outside it.
    /// </summary>
    /// <param name="select">The <see cref="SelectExpression" /> to prune.</param>
    /// <param name="preserveProjection">Whether to prune projections if they aren't referenced from the outside.</param>
    /// <returns>A pruned copy of <paramref name="select" />, or the same instance of nothing was pruned.</returns>
    protected virtual SelectExpression PruneSelect(SelectExpression select, bool preserveProjection)
    {
        Check.DebugAssert(!select.IsMutable, "Mutable SelectExpression found when pruning");

        var referencedColumnMap = ReferencedColumnMap;

        // When visiting the select's tables, we track the alias so that we know it when processing nested table expressions (e.g. within
        // set operations); make sure that when visiting other clauses (e.g. predicate), the tracked table alias is null.
        var currentSelectAlias = select.Alias ?? CurrentTableAlias;
        var parentTableAlias = CurrentTableAlias;
        CurrentTableAlias = null;

        // First visit all the non-table clauses of the SelectExpression - this will populate referencedColumnMap with all columns
        // referenced on all tables.

        // Go over the projections, prune any that isn't referenced from the outside and visiting those that are.
        // We avoid pruning projections when:
        // 1. The caller requests we don't (top-level select, scalar subquery, select within a set operation where the other is distinct -
        //     projection must be preserved as-is)
        // 2. The select has distinct (removing a projection changes which rows get projected out)
        preserveProjection |= select.IsDistinct || currentSelectAlias is null;
        List<ProjectionExpression>? projections = null;

        var referencedProjectionAliases = currentSelectAlias is not null ? referencedColumnMap.GetValueOrDefault(currentSelectAlias) : null;

        for (var i = 0; i < select.Projection.Count; i++)
        {
            var projection = select.Projection[i];

            var visitedProjection = preserveProjection || referencedProjectionAliases?.Contains(projection.Alias) == true
                ? (ProjectionExpression)Visit(projection)
                : null; // "visited" in the sense of pruned

            if (visitedProjection != projection && projections is null)
            {
                projections = new(select.Projection.Count);
                for (var j = 0; j < i; j++)
                {
                    projections.Add(select.Projection[j]);
                }
            }

            if (projections is not null && visitedProjection is not null)
            {
                projections.Add(visitedProjection);
            }
        }

        var predicate = (SqlExpression?)Visit(select.Predicate);
        var groupBy = this.VisitAndConvert(select.GroupBy);
        var having = (SqlExpression?)Visit(select.Having);
        var orderings = this.VisitAndConvert(select.Orderings);
        var offset = (SqlExpression?)Visit(select.Offset);
        var limit = (SqlExpression?)Visit(select.Limit);

        // Note that we don't visit/copy _identifier, _childIdentifier and _tpcDiscriminatorValues; these have already been applied
        // and are no longer needed.

        // We've visited the entire select expression except for the table, and now have referencedColumnMap fully populated with column
        // references to all its tables.
        // Go over the tables, removing any which aren't referenced anywhere (and are prunable).
        // We do this in backwards order, so that later joins referencing earlier tables in the predicate don't cause the earlier tables
        // to be preserved.
        List<TableExpressionBase>? tables = null;
        for (var i = select.Tables.Count - 1; i >= 0; i--)
        {
            var table = select.Tables[i];
            var alias = table.GetRequiredAlias();
            TableExpressionBase? visitedTable;

            if (!referencedColumnMap.ContainsKey(alias)
                && table is JoinExpressionBase { IsPrunable: true })
            {
                // If no column references the table, prune it.
                // Note that we only prune joins; pruning the main is more complex because other tables need to unwrap joins to be main.
                // We also only prune joins explicitly marked as prunable; otherwise e.g. an inner join may be needed to filter out rows
                // even if no column references it.
                visitedTable = null;
            }
            else
            {
                // The table wasn't pruned - visit it. This may add references to a previous table, causing it to be preserved (e.g. if it's
                // referenced from the join predicate), or just prune something inside (e.g. a subquery table).
                // Note that we track the table's alias in CurrentTableAlias, in case it contains nested selects (i.e. within set
                // operations), which don't have their own alias.
                CurrentTableAlias = alias;
                visitedTable = (TableExpressionBase)Visit(table);
            }

            if (visitedTable != table && tables is null)
            {
                tables = new List<TableExpressionBase>(select.Tables.Count);
                for (var j = i + 1; j < select.Tables.Count; j++)
                {
                    tables.Add(select.Tables[j]);
                }
            }

            if (tables is not null && visitedTable is not null)
            {
                tables.Insert(0, visitedTable);
            }
        }

        CurrentTableAlias = parentTableAlias;

        return select.Update(
            projections ?? select.Projection, tables ?? select.Tables, predicate, groupBy, having, orderings, limit, offset);
    }
}
