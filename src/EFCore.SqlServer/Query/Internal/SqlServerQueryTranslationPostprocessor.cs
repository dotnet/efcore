// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerQueryTranslationPostprocessor : RelationalQueryTranslationPostprocessor
{
    private readonly JsonPostprocessor _openJsonPostprocessor;
    private readonly SkipWithoutOrderByInSplitQueryVerifier _skipWithoutOrderByInSplitQueryVerifier = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerQueryTranslationPostprocessor(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext,
        IRelationalTypeMappingSource typeMappingSource)
        : base(dependencies, relationalDependencies, queryCompilationContext)
    {
        _openJsonPostprocessor = new(typeMappingSource, relationalDependencies.SqlExpressionFactory);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression Process(Expression query)
    {
        query = base.Process(query);

        query = _openJsonPostprocessor.Process(query);
        _skipWithoutOrderByInSplitQueryVerifier.Visit(query);

        return query;
    }

    private sealed class SkipWithoutOrderByInSplitQueryVerifier : ExpressionVisitor
    {
        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            switch (expression)
            {
                case ShapedQueryExpression shapedQueryExpression:
                    Visit(shapedQueryExpression.ShaperExpression);
                    return shapedQueryExpression;

                case RelationalSplitCollectionShaperExpression relationalSplitCollectionShaperExpression:
                    foreach (var table in relationalSplitCollectionShaperExpression.SelectExpression.Tables)
                    {
                        Visit(table);
                    }

                    Visit(relationalSplitCollectionShaperExpression.InnerShaper);

                    return relationalSplitCollectionShaperExpression;

                case SelectExpression { Offset: not null, Orderings.Count: 0 }:
                    throw new InvalidOperationException(SqlServerStrings.SplitQueryOffsetWithoutOrderBy);

                case NonQueryExpression nonQueryExpression:
                    return nonQueryExpression;

                default:
                    return base.Visit(expression);
            }
        }
    }

    /// <summary>
    ///     Converts <see cref="SqlServerOpenJsonExpression" /> expressions with WITH (the default) to OPENJSON without WITH when an
    ///     ordering still exists on the [key] column, i.e. when the ordering of the original JSON array needs to be preserved
    ///     (e.g. limit/offset).
    /// </summary>
    private sealed class JsonPostprocessor : ExpressionVisitor
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly Dictionary<(SqlServerOpenJsonExpression, string), (SelectExpression, SqlServerOpenJsonExpression.ColumnInfo)> _columnsToRewrite = new();

        private RelationalTypeMapping? _nvarcharMaxTypeMapping, _nvarchar4000TypeMapping;

        public JsonPostprocessor(
            IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory)
            => (_typeMappingSource, _sqlExpressionFactory) = (typeMappingSource, sqlExpressionFactory);

        public Expression Process(Expression expression)
        {
            _columnsToRewrite.Clear();
            return Visit(expression);
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            switch (expression)
            {
                case ShapedQueryExpression shapedQueryExpression:
                    return shapedQueryExpression.UpdateQueryExpression(Visit(shapedQueryExpression.QueryExpression));

                case SelectExpression
                {
                    Tables: [SqlServerOpenJsonExpression { ColumnInfos: not null } openJsonExpression, ..],
                    Orderings:
                    [
                        {
                            Expression: SqlUnaryExpression
                            {
                                OperatorType: ExpressionType.Convert,
                                Operand: ColumnExpression { Name: "key", Table: var keyColumnTable }
                            }
                        }
                    ]
                } selectExpression
                    when keyColumnTable == openJsonExpression:
                {
                    // Remove the WITH clause from the OPENJSON expression
                    var newOpenJsonExpression = openJsonExpression.Update(
                        openJsonExpression.JsonExpression,
                        openJsonExpression.Path,
                        columnInfos: null);

                    var newTables = selectExpression.Tables.ToArray();
                    newTables[0] = newOpenJsonExpression;

                    var newSelectExpression = selectExpression.Update(
                        selectExpression.Projection,
                        newTables,
                        selectExpression.Predicate,
                        selectExpression.GroupBy,
                        selectExpression.Having,
                        selectExpression.Orderings,
                        selectExpression.Limit,
                        selectExpression.Offset);

                    // Record the OPENJSON expression and its projected column(s), along with the store type we just removed from the WITH
                    // clause. Then visit the select expression, replacing all matching ColumnExpressions - see below for the details.
                    // TODO: Need to pass through the type mapping API for converting the JSON value (nvarchar) to the relational store type
                    // (e.g. datetime2), see #30677
                    foreach (var columnInfo in openJsonExpression.ColumnInfos)
                    {
                        _columnsToRewrite.Add((newOpenJsonExpression, columnInfo.Name), new(newSelectExpression, columnInfo));
                    }

                    var result = base.Visit(newSelectExpression);

                    foreach (var column in openJsonExpression.ColumnInfos)
                    {
                        _columnsToRewrite.Remove((newOpenJsonExpression, column.Name));
                    }

                    return result;
                }

                case ColumnExpression { Table: SqlServerOpenJsonExpression openJsonTable, Name: var name } columnExpression
                    when _columnsToRewrite.TryGetValue((openJsonTable, name), out var columnRewriteInfo):
                {
                    // We found a ColumnExpression that refers to the OPENJSON table, we need to rewrite it.

                    var (selectExpression, columnInfo) = columnRewriteInfo;

                    // The new OPENJSON (without WITH) always projects a `value` column, instead of a properly named column for individual
                    // values inside; create a new ColumnExpression with that name.
                    SqlExpression rewrittenColumn = selectExpression.CreateColumnExpression(
                        columnExpression.Table, "value", columnExpression.Type, _nvarcharMaxTypeMapping, columnExpression.IsNullable);

                    // If the WITH column info contained a path, we need to wrap the new column expression with a JSON_VALUE for that path.
                    if (columnInfo.Path is not (null or []))
                    {
                        if (columnInfo.AsJson)
                        {
                            throw new InvalidOperationException(
                                "IMPOSSIBLE. AS JSON signifies an owned sub-entity being projected out of OPENJSON/WITH. "
                                + "Columns referring to that must be wrapped be Json{Scalar,Query}Expression and will have been already " +
                                "dealt with below");
                        }

                        _nvarchar4000TypeMapping ??= _typeMappingSource.FindMapping("nvarchar(4000)");

                        rewrittenColumn = new JsonScalarExpression(
                            rewrittenColumn, columnInfo.Path, rewrittenColumn.Type, _nvarchar4000TypeMapping, columnExpression.IsNullable);
                    }

                    // OPENJSON with WITH specified the store type in the WITH, but the version without just always projects
                    // nvarchar(max); add a CAST to convert. Note that for AS JSON the type mapping is always nvarchar(max), and we don't
                    // need to add a CAST over the JSON_QUERY returned above.
                    if (columnInfo.TypeMapping.StoreType != "nvarchar(max)")
                    {
                        _nvarcharMaxTypeMapping ??= _typeMappingSource.FindMapping("nvarchar(max)");

                        rewrittenColumn = _sqlExpressionFactory.Convert(
                            rewrittenColumn,
                            columnExpression.Type,
                            columnInfo.TypeMapping);
                    }

                    return rewrittenColumn;
                }

                // JsonScalarExpression over a column coming out of OPENJSON/WITH; this means that the column represents an owned sub-
                // entity, and therefore must have AS JSON. Rewrite the column and simply collapse the paths together.
                case JsonScalarExpression
                    {
                        Json: ColumnExpression { Table: SqlServerOpenJsonExpression openJsonTable } columnExpression
                    } jsonScalarExpression
                    when _columnsToRewrite.TryGetValue((openJsonTable, columnExpression.Name), out var columnRewriteInfo):
                {
                    var (selectExpression, columnInfo) = columnRewriteInfo;

                    Check.DebugAssert(
                        columnInfo.AsJson,
                        "JsonScalarExpression over a column coming out of OPENJSON is only valid when that column represents an owned "
                        + "sub-entity, which means it must have AS JSON");

                    // The new OPENJSON (without WITH) always projects a `value` column, instead of a properly named column for individual
                    // values inside; create a new ColumnExpression with that name.
                    SqlExpression rewrittenColumn = selectExpression.CreateColumnExpression(
                        columnExpression.Table, "value", columnExpression.Type, _nvarcharMaxTypeMapping, columnExpression.IsNullable);

                    // Prepend the path from the OPENJSON/WITH to the path in the JsonScalarExpression
                    var path = columnInfo.Path is null
                        ? jsonScalarExpression.Path
                        : columnInfo.Path.Concat(jsonScalarExpression.Path).ToList();

                    return new JsonScalarExpression(
                        rewrittenColumn, path, jsonScalarExpression.Type, jsonScalarExpression.TypeMapping,
                        jsonScalarExpression.IsNullable);
                }

                default:
                    return base.Visit(expression);
            }
        }
    }
}
