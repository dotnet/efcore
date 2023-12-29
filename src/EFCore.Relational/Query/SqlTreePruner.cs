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
    private readonly Dictionary<TableExpressionBase, HashSet<string>> _referencedColumnMap = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Maps tables to the list of column aliases found referenced on them.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual IReadOnlyDictionary<TableExpressionBase, HashSet<string>> ReferencedColumnMap => _referencedColumnMap;

    /// <summary>
    /// Used for extra verification for DEBUG only
    /// </summary>
    [EntityFrameworkInternal]
    public virtual List<string> RemovedAliases { get; private set; } = null!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression Prune(Expression expression)
    {
        _referencedColumnMap.Clear();

        return Visit(expression);
    }

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
                return shapedQueryExpression.Update(
                    PruneTopLevelSelect((SelectExpression)shapedQueryExpression.QueryExpression),
                    Visit(shapedQueryExpression.ShaperExpression));

            case RelationalSplitCollectionShaperExpression relationalSplitCollectionShaperExpression:
                return relationalSplitCollectionShaperExpression.Update(
                    relationalSplitCollectionShaperExpression.ParentIdentifier,
                    relationalSplitCollectionShaperExpression.ChildIdentifier,
                    PruneTopLevelSelect(relationalSplitCollectionShaperExpression.SelectExpression),
                    Visit(relationalSplitCollectionShaperExpression.InnerShaper));

            case DeleteExpression deleteExpression:
                return deleteExpression.Update(PruneTopLevelSelect(deleteExpression.SelectExpression));

            case UpdateExpression updateExpression:
                // Note that we must visit the setters before we visit the select, since the setters can reference tables inside it.
                var visitedSetters = updateExpression.ColumnValueSetters
                    .Select(e => e with { Value = (SqlExpression)Visit(e.Value) })
                    .ToList();
                return updateExpression.Update(
                    PruneTopLevelSelect(updateExpression.SelectExpression),
                    visitedSetters);

            // The following remaining cases deal with recursive visitation (i.e. non-top-level things)

            // For any column we encounter, register it in the referenced column map, which records the aliases referenced on each table.
            case ColumnExpression column:
                RegisterTable(column.Table.UnwrapJoin());

                void RegisterTable(TableExpressionBase table)
                {
                    _referencedColumnMap.GetOrAddNew(table).Add(column.Name);

                    // If the table is a set operation, we need to recurse and register the contained tables as well.
                    // This is because when we visit a select inside a set operation, we need to be able to know who's referencing our
                    // projection from the outside (in order to prune unreferenced projections).
                    if (table is SetOperationBase setOperation)
                    {
                        RegisterTable(setOperation.Source1);
                        RegisterTable(setOperation.Source2);
                    }
                }

                return column;

            // Note that this only handles nested selects, and *not* the top-level select - that was already handled above in the first
            // cases.
            case SelectExpression select:
                select.Prune(this, pruneProjection: true);
                return select;

            // PredicateJoinExpressionBase.VisitChildren visits the table before the predicate, but we must visit the predicate first
            // since it can contain columns referencing the table's projection (which we shouldn't prune).
            case PredicateJoinExpressionBase join:
                var joinPredicate = (SqlExpression)Visit(join.JoinPredicate);
                var table = (TableExpressionBase)Visit(join.Table);
                return join.Update(table, joinPredicate);

            // Never prune the projection of a scalar subquery. Note that there are never columns referencing scalar subqueries, since
            // they're not tables.
            case ScalarSubqueryExpression scalarSubquery:
                scalarSubquery.Subquery.Prune(this, pruneProjection: false);
                return scalarSubquery;

            // Same for subqueries inside InExpression
            case InExpression { Subquery: SelectExpression subquery } inExpression:
                var item = (SqlExpression)Visit(inExpression.Item);
                subquery.Prune(this, pruneProjection: false);
                return inExpression.Update(item, subquery);

            // If the set operation is distinct (union/intersect/except, but not concat), we cannot prune the projection since that would
            // affect which rows come out.
            // Also, even if the set operation is non-distinct but one side is distinct, we avoid pruning the projection since that could
            // make the two sides have different and incompatible projections (see #30273).
            // Note that we still visit to prune within the set operation, and to make sure the referenced column map gets updated.
            case SetOperationBase { Source1: var source1, Source2: var source2 } setOperation
                when setOperation.IsDistinct || source1.IsDistinct || source2.IsDistinct:
            {
                source1.Prune(this, pruneProjection: false);
                source2.Prune(this, pruneProjection: false);
                return setOperation;
            }

            default:
                return base.VisitExtension(node);
        }
    }

    private SelectExpression PruneTopLevelSelect(SelectExpression select)
    {
#if DEBUG
        RemovedAliases = new();
#endif
        select = select.PruneToplevel(this);
#if DEBUG
        select.RemovedAliases = RemovedAliases;
#endif

        return select;
    }
}
