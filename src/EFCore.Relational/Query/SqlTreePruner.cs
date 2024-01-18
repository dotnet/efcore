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
    // TODO: Make this protected after SelectExpression.Prune is moved into this visitor
    [EntityFrameworkInternal]
    public virtual IReadOnlyDictionary<string, HashSet<string>> ReferencedColumnMap => _referencedColumnMap;

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
    // TODO: Make this protected after SelectExpression.Prune is moved into this visitor
    [EntityFrameworkInternal]
    public virtual string? CurrentTableAlias { get; set; }

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
                    ((SelectExpression)shapedQueryExpression.QueryExpression).PruneToplevel(this),
                    Visit(shapedQueryExpression.ShaperExpression));

            case RelationalSplitCollectionShaperExpression relationalSplitCollectionShaperExpression:
                _referencedColumnMap.Clear();
                return relationalSplitCollectionShaperExpression.Update(
                    relationalSplitCollectionShaperExpression.ParentIdentifier,
                    relationalSplitCollectionShaperExpression.ChildIdentifier,
                    relationalSplitCollectionShaperExpression.SelectExpression.PruneToplevel(this),
                    Visit(relationalSplitCollectionShaperExpression.InnerShaper));

            case DeleteExpression deleteExpression:
                return deleteExpression.Update(deleteExpression.SelectExpression.PruneToplevel(this));

            case UpdateExpression updateExpression:
                // Note that we must visit the setters before we visit the select, since the setters can reference tables inside it.
                var visitedSetters = updateExpression.ColumnValueSetters
                    .Select(e => e with { Value = (SqlExpression)Visit(e.Value) })
                    .ToList();
                return updateExpression.Update(
                    updateExpression.SelectExpression.PruneToplevel(this),
                    visitedSetters);

            // The following remaining cases deal with recursive visitation (i.e. non-top-level things)

            // For any column we encounter, register it in the referenced column map, which records the aliases referenced on each table.
            case ColumnExpression column:
                RegisterTable(column.TableAlias, column);

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

        void RegisterTable(string tableAlias, ColumnExpression column)
            => _referencedColumnMap.GetOrAddNew(tableAlias).Add(column.Name);
    }
}
