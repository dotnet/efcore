// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

// Replaces every GroupingAggregateFallbackExpression with its folded aggregate where the final SQL tree shows SQL would evaluate that
// aggregate over the owning query's GROUP BY, and with its subquery fallback everywhere else (#27130).
//
// The folded aggregate is used only when it sits in the projection, HAVING or ORDER BY of the query owning the grouping: not inside a
// nested query, where an aggregate naming none of the owner's columns would be evaluated by the nested query instead; not in a clause
// where SQL doesn't allow aggregates; and not inside another aggregate or a window function, which can't contain one. Everything else
// in between - operators, scalar functions, and provider expressions this visitor doesn't know - leaves its operands evaluated where
// they are, so a folded aggregate under it stays foldable.
//
// Runs once projections have been applied, so that every SelectExpression is immutable and in its final place, and before pruning and
// alias postprocessing, which the tree must still be in step with the grouping keys for.
internal sealed class GroupingAggregateFoldingExpressionVisitor : ExpressionVisitor
{
    // The grouped query that a folded aggregate visited now would be evaluated over, or null where one can't be folded.
    private SelectExpression? _foldTarget;

    protected override Expression VisitExtension(Expression node)
    {
        switch (node)
        {
            case ShapedQueryExpression shapedQueryExpression:
                return shapedQueryExpression.Update(
                    Visit(shapedQueryExpression.QueryExpression), Visit(shapedQueryExpression.ShaperExpression));

            case GroupingAggregateFallbackExpression fallback:
                return CanFold(fallback)
                    ? VisitBoundary(fallback.FoldedAggregate)
                    : VisitBoundary(
                        fallback.Fallback
                        ?? throw new InvalidOperationException(CoreStrings.TranslationFailed(fallback.AggregateCall.Print())));

            case SelectExpression { IsMutable: false } select:
            {
                var parentFoldTarget = _foldTarget;

                _foldTarget = null;
                var tables = VisitList(select.Tables);
                var predicate = (SqlExpression?)Visit(select.Predicate);
                var groupBy = VisitList(select.GroupBy);
                var offset = (SqlExpression?)Visit(select.Offset);
                var limit = (SqlExpression?)Visit(select.Limit);

                _foldTarget = select.GroupBy.Count > 0 ? select : null;
                var projections = VisitList(select.Projection);
                var having = (SqlExpression?)Visit(select.Having);
                var orderings = VisitList(select.Orderings);

                _foldTarget = parentFoldTarget;

                return select.Update(tables, predicate, groupBy, having, projections, orderings, offset, limit);
            }

            // An aggregate or a window function evaluates its arguments over a set of rows, and SQL doesn't allow an aggregate there.
#pragma warning disable EF9001 // Window function support is experimental
            case AggregateFunctionExpression or WindowFunctionExpression or RowNumberExpression or DistinctExpression:
#pragma warning restore EF9001
            case SqlFunctionExpression { IsBuiltIn: true } function when IsKnownAggregateFunction(function):
                return VisitBoundary(node);

            default:
                return base.VisitExtension(node);
        }
    }

    private bool CanFold(GroupingAggregateFallbackExpression fallback)
        => _foldTarget is { } target
            && target.GroupBy.SequenceEqual(fallback.GroupingKeys)
            // A GROUP BY made of an outer query's columns - an inner query grouped by g.Key - equals that query's own, so the target
            // is the owner only if the grouping keys are over its tables.
            && ReferencesOnlyTablesOf(fallback.GroupingKeys, target, allowNone: false)
            && fallback.FoldedAggregate.Type == fallback.Type
            && fallback.FoldedAggregate.TypeMapping == fallback.TypeMapping
            // An aggregate naming columns of no table of the target - an outer query's - would be evaluated by that query instead.
            && ReferencesOnlyTablesOf([fallback.FoldedAggregate], target, allowNone: true);

    private static bool ReferencesOnlyTablesOf(IReadOnlyList<SqlExpression> expressions, SelectExpression target, bool allowNone)
    {
        var aliases = new HashSet<string>();
        foreach (var expression in expressions)
        {
            aliases.UnionWith(RelationalSqlTranslatingExpressionVisitor.ReferencedTableAliasCollector.Collect(expression));
        }

        return (allowNone || aliases.Count > 0) && aliases.All(alias => target.Tables.Any(t => t.UnwrapJoin().Alias == alias));
    }

    private Expression VisitBoundary(Expression node)
    {
        var parentFoldTarget = _foldTarget;
        _foldTarget = null;
        var visited = node is SelectExpression { IsMutable: false } ? VisitExtension(node) : base.VisitExtension(node);
        _foldTarget = parentFoldTarget;

        return visited;
    }

    private IReadOnlyList<T> VisitList<T>(IReadOnlyList<T> list)
        where T : Expression
    {
        List<T>? visitedList = null;
        for (var i = 0; i < list.Count; i++)
        {
            var visited = (T)Visit(list[i]);
            if (visited != list[i] && visitedList is null)
            {
                visitedList = [.. list.Take(i)];
            }

            visitedList?.Add(visited);
        }

        return visitedList ?? list;
    }

    // There's no representation of whether a function is an aggregate, and providers emit their own as plain functions (SQLite's
    // ef_sum, for one), so the aggregates are recognized by name, as SqlServerAggregateArgumentPostprocessor does. An aggregate not
    // known here only gets the folded aggregate nested in it, which SQL rejects - as it did before #27130, when every aggregate over a
    // grouping was folded - while treating every unknown function as an aggregate would turn away the scalar functions of providers
    // and users, failing to translate the aggregates which have no subquery to fall back to.
    private static bool IsKnownAggregateFunction(SqlFunctionExpression function)
    {
        // SQL Server's spatial aggregates are called on the type, as geometry::UnionAggregate.
        var name = function.Name;
        var typeSeparator = name.LastIndexOf("::", StringComparison.Ordinal);
        if (typeSeparator >= 0)
        {
            name = name[(typeSeparator + 2)..];
        }

        return KnownAggregateFunctions.Contains(name)
            // SQLite's max, min and SpatiaLite's GUnion are aggregates with one argument, but scalar with several (Math.Max, Math.Min,
            // Geometry.Union).
            && !(name.ToLower(CultureInfo.InvariantCulture) is "max" or "min" or "gunion" && function.Arguments?.Count > 1);
    }

    private static readonly HashSet<string> KnownAggregateFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        // The built-in providers'
        "count", "count_big", "sum", "avg", "min", "max", "stdev", "stdevp", "var", "varp", "string_agg", "group_concat", "ef_sum",
        "ef_avg", "ef_min", "ef_max", "CollectionAggregate", "UnionAggregate", "ConvexHullAggregate", "EnvelopeAggregate", "Collect",
        "Extent", "GUnion",

        // Other standard SQL aggregates
        "array_agg", "listagg", "stddev_pop", "stddev_samp", "var_pop", "var_samp", "every", "bool_and", "bool_or"
    };
}
