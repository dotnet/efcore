// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SingleRowSubqueryMergingExpressionVisitor(SqlAliasManager sqlAliasManager, IReadOnlySet<string> volatileFunctions)
    : ExpressionVisitor
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression node)
        => node switch
        {
            ShapedQueryExpression shapedQuery
                => shapedQuery.Update(Visit(shapedQuery.QueryExpression), Visit(shapedQuery.ShaperExpression)),
            SelectExpression select => Merge((SelectExpression)base.VisitExtension(select)),
            _ => base.VisitExtension(node)
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression MergeTopLevelSelect(Expression query)
        => query is ShapedQueryExpression { QueryExpression: SelectExpression select } shapedQuery
            ? shapedQuery.UpdateQueryExpression(Merge(select))
            : query;

    private SelectExpression Merge(SelectExpression select)
    {
        // With grouping, a subquery in the projection is evaluated per group, which a join can't express.
        if (select.GroupBy.Count > 0
            || select.Having is not null)
        {
            return select;
        }

        var reads = ReadCollector.Collect([.. select.Projection, .. select.Orderings]);
        if (reads.Count < 2)
        {
            return select;
        }

        var outerAliases = select.Tables.Select(t => t.UnwrapJoin().Alias).OfType<string>().ToHashSet();
        var rows = new List<Row>();
        foreach (var read in reads.Where(r => IsMergeable(r.Subquery, outerAliases)))
        {
            AddRead(rows, read);
        }

        var merged = rows.Where(r => r.Reads.Count > 1).Select(MergeRow).ToList();
        if (merged.Count == 0)
        {
            return select;
        }

        var replacer = new ReplacingExpressionVisitor(
            [.. merged.SelectMany(m => m.Columns.Select(c => c.Read))],
            [.. merged.SelectMany(m => m.Columns.Select(c => c.Column))]);

        return select.Update(
            [.. select.Tables, .. merged.Select(m => m.Apply)], select.Predicate, select.GroupBy, select.Having,
            [.. select.Projection.Select(p => (ProjectionExpression)replacer.Visit(p))],
            [.. select.Orderings.Select(o => (OrderingExpression)replacer.Visit(o)).DistinctBy(o => o.Expression)],
            select.Offset, select.Limit);
    }

    private (OuterApplyExpression Apply, List<(Expression Read, Expression Column)> Columns) MergeRow(Row row)
    {
        var subquery = row.Subquery;
        var tableAlias = sqlAliasManager.GenerateTableAlias(subquery.Tables is [{ Alias: { } singleAlias }] ? singleAlias : "subquery");
        var projections = new List<ProjectionExpression>();
        var columns = new List<(Expression, Expression)>();
        foreach (var (read, member) in row.Reads)
        {
            var projection = projections.Find(p => p.Expression.Equals(member));
            if (projection is null)
            {
                projection = new ProjectionExpression(member, UniqueAlias(member, projections));
                projections.Add(projection);
            }

            columns.Add((read, new ColumnExpression(projection.Alias, tableAlias, read.Type, read.TypeMapping, nullable: true)));
        }

        var apply = Merge(
            subquery.Update(
                subquery.Tables, subquery.Predicate, subquery.GroupBy, subquery.Having, projections, subquery.Orderings,
                subquery.Offset, subquery.Limit));

        return (new OuterApplyExpression(apply.WithAlias(tableAlias)), columns);
    }

    private static string UniqueAlias(SqlExpression member, List<ProjectionExpression> projections)
    {
        var name = member is ColumnExpression column ? column.Name : "c";
        var alias = name;
        var counter = 0;
        while (projections.Exists(p => string.Equals(p.Alias, alias, StringComparison.OrdinalIgnoreCase)))
        {
            alias = $"{name}{counter++}";
        }

        return alias;
    }

    private bool IsMergeable(SelectExpression subquery, HashSet<string> outerAliases)
    {
        if (subquery is not { Limit: SqlConstantExpression { Value: 1 }, IsDistinct: false, GroupBy.Count: 0, Having: null })
        {
            return false;
        }

        var nodes = NodeCollector.Collect([subquery]);

        // An uncorrelated subquery can sit next to aggregates in a select without GROUP BY, where a joined column is invalid.
        return nodes.Exists(e => e is ColumnExpression column && outerAliases.Contains(column.TableAlias))
            && !nodes.Exists(IsVolatile);
    }

    private bool IsVolatile(Expression expression)
        => expression switch
        {
            SqlFunctionExpression function => !function.IsBuiltIn || volatileFunctions.Contains(function.Name),
            TableValuedFunctionExpression { IsBuiltIn: false } => true,
            FromSqlExpression => true,

            // SQL Server only persists computed columns that are deterministic.
            ColumnExpression { Column: IColumn { ComputedColumnSql: not null, IsStored: not true } } => true,
            _ => false
        };

    private static void AddRead(List<Row> rows, ScalarSubqueryExpression read)
    {
        var aliases = GetDeclaredAliases(read.Subquery);
        foreach (var row in rows)
        {
            var rebased = Rebase(read.Subquery, aliases, row.DeclaredAliases);
            if (GetRowParts(rebased).SequenceEqual(GetRowParts(row.Subquery)))
            {
                row.Reads.Add((read, rebased.Projection[0].Expression));
                return;
            }
        }

        rows.Add(new Row(read.Subquery, aliases, [(read, read.Subquery.Projection[0].Expression)]));
    }

    // Table aliases are unique across the query, so equal subqueries differ by the aliases they declare; a reused instance repeats them.
    private static SelectExpression Rebase(SelectExpression subquery, List<string> aliases, List<string> targetAliases)
        => (SelectExpression)SqlAliasManager.TableAliasRewriter.Rewrite(
            subquery, aliases.Zip(targetAliases).DistinctBy(p => p.First).ToDictionary(p => p.First, p => p.Second));

    private static Expression?[] GetRowParts(SelectExpression subquery)
        => [.. subquery.Tables, subquery.Predicate, .. subquery.Orderings, subquery.Offset];

    private static List<string> GetDeclaredAliases(SelectExpression subquery)
        => [.. NodeCollector.Collect(GetRowParts(subquery)).OfType<TableExpressionBase>().Select(t => t.Alias).OfType<string>()];

    private sealed record Row(
        SelectExpression Subquery,
        List<string> DeclaredAliases,
        List<(ScalarSubqueryExpression Read, SqlExpression Member)> Reads);

    private sealed class NodeCollector : ExpressionVisitor
    {
        private readonly List<Expression> _nodes = [];

        public static List<Expression> Collect(IEnumerable<Expression?> expressions)
        {
            var collector = new NodeCollector();
            foreach (var expression in expressions)
            {
                collector.Visit(expression);
            }

            return collector._nodes;
        }

        [return: NotNullIfNotNull(nameof(node))]
        public override Expression? Visit(Expression? node)
        {
            if (node is not null)
            {
                _nodes.Add(node);
            }

            return base.Visit(node);
        }
    }

    private sealed class ReadCollector : ExpressionVisitor
    {
        private readonly List<ScalarSubqueryExpression> _reads = [];

        public static List<ScalarSubqueryExpression> Collect(IEnumerable<Expression> expressions)
        {
            var collector = new ReadCollector();
            foreach (var expression in expressions)
            {
                collector.Visit(expression);
            }

            return collector._reads;
        }

        [return: NotNullIfNotNull(nameof(node))]
        public override Expression? Visit(Expression? node)
        {
            switch (node)
            {
                case ScalarSubqueryExpression read:
                    _reads.Add(read);
                    return node;

                // Subqueries of nested selects are merged into their own select.
                case TableExpressionBase:
                    return node;

                case CaseExpression { WhenClauses: [var first, ..] } caseExpression:
                    Visit(caseExpression.Operand);
                    Visit(first.Test);
                    return node;

                case SqlFunctionExpression { IsBuiltIn: true, Arguments: [var first, ..] } function
                    when string.Equals(function.Name, "COALESCE", StringComparison.OrdinalIgnoreCase):
                    Visit(first);
                    return node;

                case InExpression inExpression:
                    Visit(inExpression.Item);
                    return node;

                case SqlBinaryExpression { OperatorType: ExpressionType.AndAlso or ExpressionType.OrElse }:
                    return node;

                default:
                    return base.Visit(node);
            }
        }
    }
}
