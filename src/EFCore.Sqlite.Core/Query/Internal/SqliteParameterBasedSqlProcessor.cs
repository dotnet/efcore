// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteParameterBasedSqlProcessor(
    RelationalParameterBasedSqlProcessorDependencies dependencies,
    RelationalParameterBasedSqlProcessorParameters parameters)
    : RelationalParameterBasedSqlProcessor(dependencies, parameters)
{
    internal const string NonCoveringBooleanFilter = "Sqlite:NonCoveringBooleanFilter";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression ProcessSqlNullability(Expression queryExpression, ParametersCacheDecorator parametersDecorator)
    {
        var processed = new SqliteSqlNullabilityProcessor(Dependencies, Parameters).Process(queryExpression, parametersDecorator);
        return processed is SelectExpression select ? OptimizeBooleanSelect(OptimizeBooleanCount(select)) : processed;
    }

    private SelectExpression OptimizeBooleanSelect(SelectExpression select)
    {
        if (select is not
            {
                IsDistinct: false,
                Tables: [TableExpression table],
                Predicate: ColumnExpression
                {
                    TypeMapping: BoolTypeMapping { Converter: null, StoreType: "INTEGER" },
                    Column: IColumn tableColumn
                } column,
                GroupBy: [], Having: null, Orderings: [], Offset: null, Limit: null
            }
            || column.TableAlias != table.Alias
            || table.Table != tableColumn.Table
            || select.Projection.Count == 0
            || select.Projection.Any(p => p.Expression is not ColumnExpression { Column: IColumn c } projected
                || projected.TableAlias != table.Alias || c.Table != table.Table)
            || !tableColumn.Table.Indexes.Any(i => i.Filter == null && i.Columns[0] == tableColumn)
            || Dependencies.TypeMappingSource.FindMapping(typeof(int)) is not { Converter: null } integerMapping)
        {
            return select;
        }

        if (!tableColumn.Table.Indexes.Any(i => i.Filter == null && i.Columns[0] == tableColumn
                && select.Projection.All(p =>
                    i.Columns.Contains(((ColumnExpression)p.Expression).Column)
                    || tableColumn.Table.PrimaryKey?.Columns is [{ StoreType: "INTEGER" } key]
                    && key == ((ColumnExpression)p.Expression).Column)))
        {
            // A filtered index may already support the original predicate directly.
            return tableColumn.Table.Indexes.Any(i => i.Filter != null)
                ? select
                : select.Update([table.AddAnnotation(NonCoveringBooleanFilter, true)], column, [], null, select.Projection, [], null, null);
        }

        var sqlExpressionFactory = Dependencies.SqlExpressionFactory;
        var numericColumn = new ColumnExpression(
            column.Name, column.TableAlias, tableColumn, typeof(int), integerMapping, column.IsNullable);
        var zero = sqlExpressionFactory.Constant(0, integerMapping);
        var positive = select.Update(
            select.Tables,
            sqlExpressionFactory.AndAlso(sqlExpressionFactory.GreaterThan(numericColumn, zero), column),
            [], null, select.Projection, [], null, null);
        var negative = select.Update(
            select.Tables, sqlExpressionFactory.LessThan(numericColumn, zero),
            [], null, select.Projection, [], null, null);
        var union = new UnionExpression(table.Alias, positive, negative, distinct: false);
        var projections = select.Projection.Select(p =>
        {
            var projectedColumn = (ColumnExpression)p.Expression;
            return p.Update(new ColumnExpression(
                p.Alias.Length == 0 ? projectedColumn.Name : p.Alias,
                table.Alias, projectedColumn.Type, projectedColumn.TypeMapping, projectedColumn.IsNullable));
        }).ToArray();

        return select.Update([union], null, [], null, projections, [], null, null);
    }

    private SelectExpression OptimizeBooleanCount(SelectExpression select)
    {
        if (select is not
            {
                IsDistinct: false,
                Tables: [TableExpression table],
                Predicate: ColumnExpression
                {
                    TypeMapping: BoolTypeMapping { Converter: null, StoreType: "INTEGER" },
                    Column: IColumn tableColumn
                } column,
                Projection:
                [
                    {
                        Expression: SqlFunctionExpression
                        {
                            Name: "COUNT", IsBuiltIn: true, IsNullable: false,
                            Arguments: [SqlFragmentExpression { Sql: "*" }],
                            TypeMapping: { Converter: null }
                        } count
                    }
                ],
                GroupBy: [], Having: null, Orderings: [], Offset: null, Limit: null
            }
            || column.TableAlias != table.Alias
            || table.Table != tableColumn.Table
            || (count.Type != typeof(int) && count.Type != typeof(long))
            || !tableColumn.Table.Indexes.Any(i => i.Filter == null && i.Columns[0] == tableColumn)
            || Dependencies.TypeMappingSource.FindMapping(typeof(int)) is not { Converter: null } integerMapping)
        {
            return select;
        }

        var sqlExpressionFactory = Dependencies.SqlExpressionFactory;
        var numericColumn = new ColumnExpression(
            column.Name, column.TableAlias, tableColumn, typeof(int), integerMapping, column.IsNullable);
        var zero = sqlExpressionFactory.Constant(0, integerMapping);

        // Disjoint counts avoid both row lookups and OR's row-ID deduplication. Only the positive range can contain TEXT/BLOB.
        var positive = select.Update(
            select.Tables,
            sqlExpressionFactory.AndAlso(sqlExpressionFactory.GreaterThan(numericColumn, zero), column),
            [], null, select.Projection, [], null, null);
        var negative = select.Update(
            select.Tables, sqlExpressionFactory.LessThan(numericColumn, zero),
            [], null, select.Projection, [], null, null);
        var sum = sqlExpressionFactory.Add(new ScalarSubqueryExpression(positive), new ScalarSubqueryExpression(negative));

        return select.Update([], null, [], null, [select.Projection[0].Update(sum)], [], null, null);
    }
}
