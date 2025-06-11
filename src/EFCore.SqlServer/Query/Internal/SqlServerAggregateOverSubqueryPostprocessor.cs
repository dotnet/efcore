// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     SQL Server doesn't support aggregate function invocations over subqueries, or other aggregate function invocations; this
///     postprocessor lifts such subqueries out to an OUTER APPLY/JOIN on the SELECT to work around this limitation.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class SqlServerAggregateOverSubqueryPostprocessor(SqlAliasManager sqlAliasManager) : ExpressionVisitor
{
    private SelectExpression? _currentSelect;
    private bool _inAggregateInvocation;
    private bool _aggregateArgumentContainsSubquery;
    private List<JoinExpressionBase>? _joinsToAdd;
    private bool _isCorrelatedSubquery;
    private HashSet<string>? _tableAliasesInScope;

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
            case SelectExpression select:
            {
                var (parentSelect, parentJoinsToAdd, parentAggregateInvocation) = (_currentSelect, _joinsToAdd, _inAggregateInvocation);
                (_currentSelect, _joinsToAdd, _inAggregateInvocation) = (select, null, false);

                // If _tableAliasesInScope is non-null, we're tracking which table aliases are in scope for the current subquery, to detect
                // correlated vs. uncorrelated subqueries. Add and remove the select's tables to _tableAliasInScope.
                SelectExpression visitedSelect;
                if (_tableAliasesInScope is null)
                {
                    visitedSelect = (SelectExpression)base.VisitExtension(node);
                }
                else
                {
                    List<string> tableAliases = select.Tables.Select(t => t.UnwrapJoin().Alias).Where(a => a is not null).ToList()!;
                    _tableAliasesInScope.UnionWith(tableAliases);
                    visitedSelect = (SelectExpression)base.VisitExtension(node);
                    _tableAliasesInScope.ExceptWith(tableAliases);
                }

                // A subquery is being lifted out somewhere inside this SelectExpression; add the join.
                if (_joinsToAdd is not null)
                {
                    visitedSelect = visitedSelect.Update(
                        [.. visitedSelect.Tables, .. _joinsToAdd],
                        visitedSelect.Predicate,
                        visitedSelect.GroupBy,
                        visitedSelect.Having,
                        visitedSelect.Projection,
                        visitedSelect.Orderings,
                        visitedSelect.Offset,
                        visitedSelect.Limit);
                }

                (_currentSelect, _joinsToAdd, _inAggregateInvocation) = (parentSelect, parentJoinsToAdd, parentAggregateInvocation);
                return visitedSelect;
            }

            // TODO: We currently don't represent the fact that a function is an aggregate or not; so for now we just match a few well-known
            // functions. Improve this in the future.
            case SqlFunctionExpression { IsBuiltIn: true } function
                when function.Name.ToLower(CultureInfo.InvariantCulture) is "sum" or "avg" or "min" or "max" or "count":
            {
                var parentInAggregateInvocation = _inAggregateInvocation;
                var parentIsCorrelatedSubquery = _isCorrelatedSubquery;
                var parentTableAliasesInScope = _tableAliasesInScope;
                var parentAggregateArgumentContainsSubquery = _aggregateArgumentContainsSubquery;
                _inAggregateInvocation = true;
                _isCorrelatedSubquery = false;
                _tableAliasesInScope = new HashSet<string>();
                _aggregateArgumentContainsSubquery = false;

                var result = base.VisitExtension(function);

                if (_aggregateArgumentContainsSubquery)
                {
                    // During our visitation of the aggregate function invocation, a subquery was encountered - this is our trigger to
                    // extract out the argument to be an OUTER APPLY/CROSS JOIN.
                    if (result is not SqlFunctionExpression { Instance: null, Arguments: [var argument] } visitedFunction)
                    {
                        throw new UnreachableException();
                    }

                    // Since the subquery is currently a scalar subquery (or EXISTS), its doesn't have an alias for the subquery, and may
                    // not have an alias on its projection either. As part of lifting it out, we need to assign both aliases, so that the
                    // projection can be referenced.
                    var subqueryAlias = sqlAliasManager.GenerateTableAlias("subquery");

                    SelectExpression liftedSubquery;

                    if (argument is ScalarSubqueryExpression { Subquery: { Projection: [var subqueryProjection] } subquery })
                    {
                        // In the regular, simple case (see else below), we simply extract the entire argument of the aggregate method,
                        // wrap it in a simple subquery, and add that to the containing SelectExpression.
                        // But if the aggregate argument happens to be a scalar subqueries directly, wrapping it in a subquery isn't needed:
                        // we can simply use that scalar subquery directly.

                        // Note that there's an assumption here that the scalar subquery being extracted out will only ever return a single
                        // row (and column); if it didn't, the APPLY/JOIN would cause the principal row to get duplicated, producing
                        // incorrect results. It shouldn't be possible to produce such a state of affairs with LINQ, and in any case,
                        // placing a multiple row/column-returning subquery inside ScalarSubqueryExpression is a bug - that SQL would fail
                        // in any case even if it weren't wrapped inside an aggregate function invocation.
                        if (subqueryProjection.Alias is null or "")
                        {
                            subqueryProjection = new ProjectionExpression(subqueryProjection.Expression, "value");
                        }

                        liftedSubquery = subquery
                            .Update(
                                subquery.Tables,
                                subquery.Predicate,
                                subquery.GroupBy,
                                subquery.Having,
                                [subqueryProjection],
                                subquery.Orderings,
                                subquery.Offset,
                                subquery.Limit)
                            .WithAlias(subqueryAlias);
                    }
                    else
                    {
#pragma warning disable EF1001 // SelectExpression constructor is internal
                        liftedSubquery = new SelectExpression(
                            subqueryAlias,
                            tables: Array.Empty<TableExpressionBase>(),
                            predicate: null,
                            groupBy: Array.Empty<SqlExpression>(),
                            having: null,
                            projections: new[] { new ProjectionExpression(argument, "value") },
                            distinct: false,
                            orderings: Array.Empty<OrderingExpression>(),
                            offset: null,
                            limit: null,
                            sqlAliasManager: sqlAliasManager);
#pragma warning restore EF1001
                    }

                    _joinsToAdd ??= new List<JoinExpressionBase>();
                    _joinsToAdd.Add(
                        _isCorrelatedSubquery ? new OuterApplyExpression(liftedSubquery) : new CrossJoinExpression(liftedSubquery));

                    var projection = liftedSubquery.Projection.Single();

                    return visitedFunction.Update(
                        instance: null,
                        arguments:
                        [
                            new ColumnExpression(
                                projection.Alias, subqueryAlias, projection.Expression.Type, projection.Expression.TypeMapping,
                                nullable: true)
                        ]);
                }

                _inAggregateInvocation = parentInAggregateInvocation;
                _isCorrelatedSubquery = parentIsCorrelatedSubquery;
                _tableAliasesInScope = parentTableAliasesInScope;
                _aggregateArgumentContainsSubquery = parentAggregateArgumentContainsSubquery;

                return result;
            }

            // We have a scalar subquery inside an aggregate function argument; lift it out to an OUTER APPLY/CROSS JOIN that will be added
            // to the containing SELECT, and return a ColumnExpression in its place that references that OUTER APPLY/CROSS JOIN.
            case ScalarSubqueryExpression or ExistsExpression or InExpression { Subquery: not null }
                when _inAggregateInvocation && _currentSelect is not null:
                _aggregateArgumentContainsSubquery = true;
                return base.VisitExtension(node);

            // If _tableAliasesInScope is non-null, we're tracking which table aliases are in scope for the current subquery, to detect
            // correlated vs. uncorrelated subqueries. If we have a column referencing a table that isn't in the current scope, that means
            // we're in a correlated subquery.
            case ColumnExpression column when _tableAliasesInScope?.Contains(column.TableAlias) == false:
                _isCorrelatedSubquery = true;
                return base.VisitExtension(column);

            case ShapedQueryExpression shapedQueryExpression:
                shapedQueryExpression = shapedQueryExpression
                    .UpdateQueryExpression(Visit(shapedQueryExpression.QueryExpression))
                    .UpdateShaperExpression(Visit(shapedQueryExpression.ShaperExpression));
                return shapedQueryExpression.UpdateShaperExpression(Visit(shapedQueryExpression.ShaperExpression));

            default:
                return base.VisitExtension(node);
        }
    }
}
