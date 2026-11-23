// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     Rewrites a LEFT/INNER JOIN over a single-table subquery whose only content is a WHERE filter into a join over the table
///     itself, folding the filter into the join condition. This removes an unnecessary subquery for queries such as
///     <c>from b in Bars.Where(x =&gt; x.FooId == foo.Id &amp;&amp; x.State == "Blah").DefaultIfEmpty()</c>, which EF otherwise
///     translates to a <c>LEFT JOIN (SELECT ... WHERE State = 'Blah')</c>.
/// </summary>
/// <remarks>
///     A subquery is only unwrapped when no other join in the same SELECT references its alias in a join condition. Such a
///     dependency (e.g. a nested collection include whose join keys off the previous subquery) relies on the subquery having
///     already filtered its rows, so folding the filter out would change the query's results.
///
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class SqliteSubqueryToJoinRewriter(ISqlExpressionFactory sqlExpressionFactory) : ExpressionVisitor
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression node)
    {
        if (node is ShapedQueryExpression shapedQuery)
        {
            return shapedQuery
                .UpdateQueryExpression(Visit(shapedQuery.QueryExpression))
                .UpdateShaperExpression(Visit(shapedQuery.ShaperExpression));
        }

        var visited = base.VisitExtension(node);

        return visited is SelectExpression select ? TryFlattenJoins(select) : visited;
    }

    private Expression TryFlattenJoins(SelectExpression select)
    {
        List<TableExpressionBase>? newTables = null;
        Dictionary<string, (IReadOnlyDictionary<string, SqlExpression> Map, bool IsLeftJoin)>? inlinedAliases = null;

        for (var i = 0; i < select.Tables.Count; i++)
        {
            if (select.Tables[i] is not PredicateJoinExpressionBase { Table: SelectExpression inner } join
                || join is not (InnerJoinExpression or LeftJoinExpression)
                || !IsFilterOnlySubquery(inner)
                || IsAliasReferencedElsewhere(select, inner.Alias!, i)
                || JoinKeyReferencesSibling(select, join, i)
                // Under a LEFT JOIN the projected columns must be made nullable; we can only do that for bare column projections.
                || (join is LeftJoinExpression && inner.Projection.Any(p => p.Expression is not ColumnExpression))
                // A predicate made purely of IS NOT NULL checks is an optional-entity existence test (table splitting), not a real
                // filter; the containing query uses it to decide whether the entity is present, so keep it inside the subquery.
                || (join is LeftJoinExpression && IsExistenceCheckOnly(inner.Predicate!)))
            {
                continue;
            }

            var projectionMap = inner.Projection.ToDictionary(p => p.Alias, p => p.Expression);
            var inliner = new AliasInliningVisitor(inner.Alias!, projectionMap);
            var joinPredicate = (SqlExpression)inliner.Visit(join.JoinPredicate);

            // SqlNullabilityProcessor leaves a join predicate that is a single top-level equality uncompensated, because in LINQ
            // equijoins null doesn't match null. Once the filter is AND-ed onto it, the equality is no longer top-level and gets
            // the regular null compensation, so NULL keys would start matching each other; keep such subqueries as they are.
            if (IsEqualityBetweenNullableExpressions(joinPredicate))
            {
                continue;
            }

            // The subquery's WHERE moves onto the join condition; both the existing join key and the filter are remapped from the
            // subquery alias onto the underlying table's columns.
            var newJoinPredicate = sqlExpressionFactory.AndAlso(joinPredicate, (SqlExpression)inliner.Visit(inner.Predicate!));

            newTables ??= [.. select.Tables];
            newTables[i] = join is InnerJoinExpression
                ? new InnerJoinExpression(inner.Tables[0], newJoinPredicate)
                : new LeftJoinExpression(inner.Tables[0], newJoinPredicate);

            inlinedAliases ??= [];
            inlinedAliases[inner.Alias!] = (projectionMap, join is LeftJoinExpression);
        }

        if (newTables is null)
        {
            return select;
        }

        // Anything in the containing SELECT that referenced the removed subquery alias now points at the underlying table's columns.
        var outerInliner = new MultiAliasInliningVisitor(inlinedAliases!);

        return select.Update(
            newTables,
            (SqlExpression?)outerInliner.Visit(select.Predicate),
            select.GroupBy.Select(g => (SqlExpression)outerInliner.Visit(g)).ToList(),
            (SqlExpression?)outerInliner.Visit(select.Having),
            select.Projection.Select(p => (ProjectionExpression)outerInliner.Visit(p)).ToList(),
            select.Orderings.Select(o => (OrderingExpression)outerInliner.Visit(o)).ToList(),
            (SqlExpression?)outerInliner.Visit(select.Offset),
            (SqlExpression?)outerInliner.Visit(select.Limit));
    }

    private static bool IsEqualityBetweenNullableExpressions(SqlExpression predicate)
        => predicate is SqlBinaryExpression { OperatorType: ExpressionType.Equal, Left: var left, Right: var right }
            && MayBeNull(left)
            && MayBeNull(right);

    // A compile-time approximation of the nullability SqlNullabilityProcessor computes later (parameter values aren't known yet,
    // so parameters count as nullable). It must never report a nullable expression as non-nullable: that would fold the subquery and
    // let the compensated key match NULL with NULL, while the opposite only keeps a subquery that could have been flattened.
    private static bool MayBeNull(SqlExpression expression)
        => expression switch
        {
            ColumnExpression column => column.IsNullable,
            SqlConstantExpression constant => constant.Value is null,
            // IS NULL / IS NOT NULL never yield NULL
            SqlUnaryExpression { OperatorType: ExpressionType.Equal or ExpressionType.NotEqual } => false,
            SqlUnaryExpression unary => MayBeNull(unary.Operand),
            SqlBinaryExpression binary => MayBeNull(binary.Left) || MayBeNull(binary.Right),
            SqlFunctionExpression function => FunctionMayBeNull(function),
            CaseExpression caseExpression
                => caseExpression.ElseResult is null
                || MayBeNull(caseExpression.ElseResult)
                || caseExpression.WhenClauses.Any(clause => MayBeNull(clause.Result)),
            ExistsExpression => false,
            _ => true
        };

    private static bool FunctionMayBeNull(SqlFunctionExpression function)
    {
        if (!function.IsNullable)
        {
            return false;
        }

        if (function is { IsBuiltIn: true, Arguments: not null }
            && string.Equals(function.Name, "COALESCE", StringComparison.OrdinalIgnoreCase))
        {
            return function.Arguments.All(MayBeNull);
        }

        // Same rule as SqlNullabilityProcessor.VisitSqlFunction: when nullability propagation is declared for the instance or any
        // argument, the result is nullable only if one of the propagating operands is; otherwise fall back to IsNullable.
        var usesNullabilityPropagation = function.InstancePropagatesNullability == true;
        var hasNullableOperand = function is { InstancePropagatesNullability: true, Instance: { } instance } && MayBeNull(instance);

        if (function is { Arguments: not null, ArgumentsPropagateNullability: not null })
        {
            for (var i = 0; i < function.Arguments.Count; i++)
            {
                usesNullabilityPropagation |= function.ArgumentsPropagateNullability[i];
                hasNullableOperand |= function.ArgumentsPropagateNullability[i] && MayBeNull(function.Arguments[i]);
            }
        }

        return !usesNullabilityPropagation || hasNullableOperand;
    }

    // An "IS NOT NULL" check is a SqlUnaryExpression with a NotEqual operator; a predicate built only from these (AND-ed together)
    // is an entity-existence test rather than a value filter.
    private static bool IsExistenceCheckOnly(SqlExpression predicate)
        => predicate switch
        {
            SqlUnaryExpression { OperatorType: ExpressionType.NotEqual } => true,
            SqlBinaryExpression { OperatorType: ExpressionType.AndAlso } binary
                => IsExistenceCheckOnly(binary.Left) && IsExistenceCheckOnly(binary.Right),
            _ => false
        };

    private static bool IsFilterOnlySubquery(SelectExpression select)
        => select is
        {
            Tables: [TableExpression],
            Predicate: not null,
            GroupBy: [],
            Having: null,
            Orderings: [],
            Limit: null,
            Offset: null,
            IsDistinct: false
        };

    // Any reference to the subquery alias from another table in the FROM clause (a sibling join's condition, or a correlated
    // subquery nested inside one) means that table depends on the subquery having filtered its rows first, so it can't be unwrapped.
    // References from the projection, predicate or orderings are fine, since those are remapped onto the underlying columns.
    private static bool IsAliasReferencedElsewhere(SelectExpression select, string alias, int currentIndex)
    {
        for (var i = 0; i < select.Tables.Count; i++)
        {
            if (i != currentIndex
                && new AliasReferenceFindingVisitor(alias).ContainsReference(select.Tables[i]))
            {
                return true;
            }
        }

        return false;
    }

    // If this join's own condition references another sibling join's table (a correlated join chain), unwrapping it would change
    // the join order and nullability semantics that chain relies on (e.g. keying off a preceding optional/LEFT JOIN), so leave it.
    private static bool JoinKeyReferencesSibling(SelectExpression select, PredicateJoinExpressionBase join, int currentIndex)
    {
        for (var i = 0; i < select.Tables.Count; i++)
        {
            if (i != currentIndex
                && select.Tables[i] is PredicateJoinExpressionBase otherJoin
                && otherJoin.Table.Alias is { } siblingAlias
                && new AliasReferenceFindingVisitor(siblingAlias).ContainsReference(join.JoinPredicate))
            {
                return true;
            }
        }

        return false;
    }

    private sealed class AliasReferenceFindingVisitor(string alias) : ExpressionVisitor
    {
        private bool _found;

        public bool ContainsReference(Expression expression)
        {
            _found = false;
            Visit(expression);
            return _found;
        }

        public override Expression? Visit(Expression? node)
        {
            if (_found)
            {
                return node;
            }

            if (node is ColumnExpression column && column.TableAlias == alias)
            {
                _found = true;
                return node;
            }

            return base.Visit(node);
        }
    }

    private sealed class AliasInliningVisitor(string alias, IReadOnlyDictionary<string, SqlExpression> projectionMap)
        : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
            => node is ColumnExpression column && column.TableAlias == alias && projectionMap.TryGetValue(column.Name, out var expr)
                ? expr
                : base.VisitExtension(node);
    }

    private sealed class MultiAliasInliningVisitor(
        IReadOnlyDictionary<string, (IReadOnlyDictionary<string, SqlExpression> Map, bool IsLeftJoin)> inlinedAliases)
        : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
        {
            if (node is ColumnExpression column
                && inlinedAliases.TryGetValue(column.TableAlias, out var entry)
                && entry.Map.TryGetValue(column.Name, out var expr))
            {
                // Under a LEFT JOIN the unwrapped table's columns become nullable (no match yields NULL), so the reference that used
                // to point at the subquery's already-nullable projection must be made nullable too.
                return entry.IsLeftJoin && expr is ColumnExpression inlinedColumn ? inlinedColumn.MakeNullable() : expr;
            }

            return base.VisitExtension(node);
        }
    }
}
