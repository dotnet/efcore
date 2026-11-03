// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteQueryTranslationPostprocessor : RelationalQueryTranslationPostprocessor
{
    private readonly ApplyValidatingVisitor _applyValidator = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteQueryTranslationPostprocessor(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
        RelationalQueryCompilationContext queryCompilationContext)
        : base(dependencies, relationalDependencies, queryCompilationContext)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression Process(Expression query)
    {
        var result = base.Process(query);

        // SQLite has no APPLY, but its table-valued functions can reference preceding tables in FROM; rewrite what we can as joins
        // before rejecting the rest.
        result = new SqliteApplyFlatteningExpressionVisitor(RelationalDependencies.SqlExpressionFactory).Visit(result);

        // Fold single-table filter subqueries in LEFT/INNER joins back into the join condition, removing needless subqueries.
        result = new SqliteSubqueryToJoinRewriter(RelationalDependencies.SqlExpressionFactory).Visit(result);

        _applyValidator.Visit(result);

        return new JsonProjectionRewritingVisitor().Visit(result);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression ProcessTypeMappings(Expression expression)
        => new SqliteTypeMappingPostprocessor(Dependencies, RelationalDependencies, RelationalQueryCompilationContext).Process(expression);

    private sealed class JsonProjectionRewritingVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression expression)
        {
            if (expression is ShapedQueryExpression shapedQuery)
            {
                return shapedQuery.Update(
                    Visit(shapedQuery.QueryExpression), Visit(shapedQuery.ShaperExpression));
            }

            if (expression is SelectExpression select)
            {
                // Only change values read by the shaper; subquery projections must retain their SQL representation.
                var tables = select.Tables.ToList();
                var projections = select.Projection
                    .Select(p => p.Update(RewriteProjection(p.Expression, tables))).ToArray();
                return select.Update(
                    tables, select.Predicate, select.GroupBy, select.Having, projections,
                    select.Orderings, select.Offset, select.Limit);
            }

            return base.VisitExtension(expression);
        }

        private static SqlExpression RewriteProjection(SqlExpression expression, List<TableExpressionBase> tables)
        {
            if (expression is JsonScalarExpression
                {
                    Path.Count: > 0, TypeMapping: SqliteJsonScalarTypeMapping mapping
                } jsonScalar)
            {
                return new JsonScalarExpression(
                    jsonScalar.Json, jsonScalar.Path, jsonScalar.Type, mapping.ForJsonProjection(), jsonScalar.IsNullable);
            }

            if (expression is ColumnExpression { TypeMapping: SqliteJsonScalarTypeMapping } column)
            {
                for (var i = 0; i < tables.Count; i++)
                {
                    if (tables[i].UnwrapJoin() is not SelectExpression subquery
                        || subquery.Alias != column.TableAlias)
                    {
                        continue;
                    }

                    var source = subquery.Projection.Single(p => p.Alias == column.Name);
                    var innerTables = subquery.Tables.ToList();
                    var jsonProjection = RewriteProjection(source.Expression, innerTables);
                    if (jsonProjection.TypeMapping is not SqliteJsonScalarTypeMapping { IsJsonProjection: true } projectionMapping)
                    {
                        return expression;
                    }

                    var groupBy = subquery.GroupBy;
                    if (subquery.IsDistinct || groupBy.Count > 0)
                    {
                        if (groupBy.Count == 0)
                        {
                            groupBy = subquery.Projection.Select(p => p.Expression).ToArray();
                        }

                        // Keep one raw token per original result row without changing DISTINCT/grouping membership.
                        jsonProjection = new SqlFunctionExpression(
                            "MIN", [jsonProjection], nullable: true, argumentsPropagateNullability: [false],
                            jsonProjection.Type, projectionMapping);
                    }

                    var alias = column.Name + "Json";
                    var suffix = 0;
                    while (subquery.Projection.Any(p => p.Alias == alias))
                    {
                        alias = column.Name + "Json" + suffix++;
                    }

                    subquery = subquery.Update(
                        innerTables, subquery.Predicate, groupBy, subquery.Having,
                        [.. subquery.Projection, new ProjectionExpression(jsonProjection, alias)],
                        subquery.Orderings, subquery.Offset, subquery.Limit);
                    tables[i] = tables[i] is JoinExpressionBase join ? join.Update(subquery) : subquery;
                    return new ColumnExpression(alias, column.TableAlias, column.Type, projectionMapping, column.IsNullable);
                }
            }

            return expression;
        }
    }

    private sealed class ApplyValidatingVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is ShapedQueryExpression shapedQueryExpression)
            {
                Visit(shapedQueryExpression.QueryExpression);
                Visit(shapedQueryExpression.ShaperExpression);

                return extensionExpression;
            }

            return extensionExpression is SelectExpression selectExpression
                && selectExpression.Tables.Any(t => t is CrossApplyExpression or OuterApplyExpression)
                    ? throw new InvalidOperationException(SqliteStrings.ApplyNotSupported)
                    : base.VisitExtension(extensionExpression);
        }
    }
}
