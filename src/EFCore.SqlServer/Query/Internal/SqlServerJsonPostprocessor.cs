// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     Converts <see cref="SqlServerOpenJsonExpression" /> expressions with WITH (the default) to OPENJSON without WITH under the following
///     conditions:
///     * When an ordering still exists on the [key] column, i.e. when the ordering of the original JSON array needs to be preserved
///       (e.g. limit/offset).
///     * When the column type in the WITH clause is a SQL Server "CLR type" - these are incompatible with WITH (e.g. hierarchy id).
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class SqlServerJsonPostprocessor : ExpressionVisitor
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    private readonly
        Dictionary<(SqlServerOpenJsonExpression, string), (SelectExpression SelectExpression, SqlServerOpenJsonExpression.ColumnInfo
            ColumnInfo)> _columnsToRewrite = new();

    private RelationalTypeMapping? _nvarcharMaxTypeMapping;

    private static readonly bool UseOldBehavior32976 =
        AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue32976", out var enabled32976) && enabled32976;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerJsonPostprocessor(
        IRelationalTypeMappingSource typeMappingSource,
        ISqlExpressionFactory sqlExpressionFactory)
    {
        (_typeMappingSource, _sqlExpressionFactory) = (typeMappingSource, sqlExpressionFactory);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression Process(Expression expression)
    {
        _columnsToRewrite.Clear();

        return Visit(expression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: NotNullIfNotNull("expression")]
    public override Expression? Visit(Expression? expression)
    {
        switch (expression)
        {
            case ShapedQueryExpression shapedQueryExpression:
                shapedQueryExpression = shapedQueryExpression.UpdateQueryExpression(Visit(shapedQueryExpression.QueryExpression));

                if (!UseOldBehavior32976)
                {
                    shapedQueryExpression = shapedQueryExpression.UpdateShaperExpression(Visit(shapedQueryExpression.ShaperExpression));
                }

                return shapedQueryExpression;

            case SelectExpression selectExpression:
            {
                TableExpressionBase[]? newTables = null;
                Dictionary<(SqlServerOpenJsonExpression, string), SqlServerOpenJsonExpression.ColumnInfo>? columnsToRewrite = null;

                for (var i = 0; i < selectExpression.Tables.Count; i++)
                {
                    var table = selectExpression.Tables[i];

                    if (table.UnwrapJoin() is SqlServerOpenJsonExpression { ColumnInfos: not null } openJsonExpression
                        && (
                            // Condition 1: an ordering still refers to the OPENJSON's [key] column - ordering needs to be preserved.
                            selectExpression.Orderings.Select(o => o.Expression)
                                .Concat(selectExpression.Projection.Select(p => p.Expression))
                                .Any(x => IsKeyColumn(x, table))
                            ||
                            // Condition 2: a column type in the WITH clause is a SQL Server "CLR type" (e.g. hierarchy id).
                            openJsonExpression.ColumnInfos.Any(c => c.TypeMapping.StoreType is "hierarchyid")))
                    {
                        // Remove the WITH clause from the OPENJSON expression
                        var newOpenJsonExpression = openJsonExpression.Update(
                            openJsonExpression.JsonExpression,
                            openJsonExpression.Path,
                            columnInfos: null);

                        table = table switch
                        {
                            JoinExpressionBase j => j.Update(newOpenJsonExpression),
                            SqlServerOpenJsonExpression => newOpenJsonExpression,
                            _ => throw new UnreachableException()
                        };

                        foreach (var columnInfo in openJsonExpression.ColumnInfos!)
                        {
                            columnsToRewrite ??=
                                new Dictionary<(SqlServerOpenJsonExpression, string), SqlServerOpenJsonExpression.ColumnInfo>();
                            columnsToRewrite.Add((newOpenJsonExpression, columnInfo.Name), columnInfo);
                        }

                        if (newTables is null)
                        {
                            newTables = new TableExpressionBase[selectExpression.Tables.Count];
                            for (var j = 0; j < i; j++)
                            {
                                newTables[j] = selectExpression.Tables[j];
                            }
                        }
                    }

                    if (newTables is not null)
                    {
                        newTables[i] = table;
                    }
                }

                // In the common case, we do not have to rewrite any OPENJSON tables.
                if (columnsToRewrite is null)
                {
                    Check.DebugAssert(newTables is null, "newTables must be null if columnsToRewrite is null");
                    return base.Visit(selectExpression);
                }

                var newSelectExpression = newTables is not null
                    ? selectExpression.Update(
                        selectExpression.Projection,
                        newTables,
                        selectExpression.Predicate,
                        selectExpression.GroupBy,
                        selectExpression.Having,
                        selectExpression.Orderings,
                        selectExpression.Limit,
                        selectExpression.Offset)
                    : selectExpression;

                // when we mark columns for rewrite we don't yet have the updated SelectExpression, so we store the info in temporary dictionary
                // and now that we have created new SelectExpression we add it to the proper dictionary that we will use for rewrite
                foreach (var columnToRewrite in columnsToRewrite)
                {
                    _columnsToRewrite.Add(columnToRewrite.Key, (newSelectExpression, columnToRewrite.Value));
                }

                // Record the OPENJSON expression and its projected column(s), along with the store type we just removed from the WITH
                // clause. Then visit the select expression, adding a cast around the matching ColumnExpressions.
                var result = base.Visit(newSelectExpression);

                foreach (var columnsToRewriteKey in columnsToRewrite.Keys)
                {
                    _columnsToRewrite.Remove(columnsToRewriteKey);
                }

                return result;
            }

            case ColumnExpression columnExpression:
            {
                var table = columnExpression.Table;
                if (table is JoinExpressionBase join)
                {
                    table = join.Table;
                }

                return table is SqlServerOpenJsonExpression openJsonTable
                    && _columnsToRewrite.TryGetValue((openJsonTable, columnExpression.Name), out var columnRewriteInfo)
                        ? RewriteOpenJsonColumn(columnExpression, columnRewriteInfo.SelectExpression, columnRewriteInfo.ColumnInfo)
                        : base.Visit(expression);
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

        static bool IsKeyColumn(SqlExpression sqlExpression, TableExpressionBase table)
            => (sqlExpression is ColumnExpression { Name: "key", Table: var keyColumnTable }
                    && keyColumnTable == table)
                || (sqlExpression is SqlUnaryExpression
                    {
                        OperatorType: ExpressionType.Convert,
                        Operand: SqlExpression operand
                    }
                    && IsKeyColumn(operand, table));

        SqlExpression RewriteOpenJsonColumn(
            ColumnExpression columnExpression,
            SelectExpression selectExpression,
            SqlServerOpenJsonExpression.ColumnInfo columnInfo)
        {
            // We found a ColumnExpression that refers to the OPENJSON table, we need to rewrite it.

            // Binary data (varbinary) is stored in JSON as base64, which OPENJSON knows how to decode as long the type is
            // specified in the WITH clause. We're now removing the WITH and applying a relational CAST, but that doesn't work
            // for base64 data.
            if (columnInfo.TypeMapping is SqlServerByteArrayTypeMapping)
            {
                throw new InvalidOperationException(SqlServerStrings.QueryingOrderedBinaryJsonCollectionsNotSupported);
            }

            // The new OPENJSON (without WITH) always projects a `value` column, instead of a properly named column for individual
            // values inside; create a new ColumnExpression with that name.
            SqlExpression rewrittenColumn = selectExpression.CreateColumnExpression(
                columnExpression.Table, "value", columnExpression.Type, _nvarcharMaxTypeMapping, columnExpression.IsNullable);

            Check.DebugAssert(columnInfo.Path is not null, "Path shouldn't be null in OPENJSON WITH");
            //Check.DebugAssert(
            //    !columnInfo.AsJson || columnInfo.TypeMapping.ElementTypeMapping is not null,
            //    "AS JSON signifies an owned sub-entity or array of primitives being projected out of OPENJSON/WITH. "
            //    + "Columns referring to sub-entities must be wrapped in Json{Scalar,Query}Expression and will have been already dealt with above");

            if (columnInfo.Path is [])
            {
                // OPENJSON with WITH specified the store type in the WITH, but the version without just always projects
                // nvarchar(max); add a CAST to convert.
                if (columnInfo.TypeMapping.StoreType != "nvarchar(max)")
                {
                    _nvarcharMaxTypeMapping ??= _typeMappingSource.FindMapping("nvarchar(max)");

                    rewrittenColumn = _sqlExpressionFactory.Convert(
                        rewrittenColumn,
                        columnExpression.Type,
                        columnInfo.TypeMapping);
                }
            }
            else
            {
                // Non-primitive collection case - elements in the JSON collection represent a structural type.
                // We need JSON_VALUE to get the individual properties out of those fragments. Note that the appropriate CASTs are added
                // in SQL generation.
                rewrittenColumn = new JsonScalarExpression(
                    rewrittenColumn,
                    columnInfo.Path,
                    columnExpression.Type,
                    columnExpression.TypeMapping,
                    columnExpression.IsNullable);
            }

            return rewrittenColumn;
        }
    }
}
