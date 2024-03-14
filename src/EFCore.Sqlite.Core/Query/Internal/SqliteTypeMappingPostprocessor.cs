// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Sqlite.Query.SqlExpressions.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteTypeMappingPostprocessor : RelationalTypeMappingPostprocessor
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly SqliteSqlExpressionFactory _sqlExpressionFactory;
    private Dictionary<string, RelationalTypeMapping>? _currentSelectInferredTypeMappings;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteTypeMappingPostprocessor(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
        RelationalQueryCompilationContext queryCompilationContext)
        : base(dependencies, relationalDependencies, queryCompilationContext)
    {
        _typeMappingSource = relationalDependencies.TypeMappingSource;
        _sqlExpressionFactory = (SqliteSqlExpressionFactory)relationalDependencies.SqlExpressionFactory;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression expression)
    {
        switch (expression)
        {
            case JsonEachExpression jsonEachExpression
                when TryGetInferredTypeMapping(
                    jsonEachExpression.Alias,
                    SqliteQueryableMethodTranslatingExpressionVisitor.JsonEachValueColumnName,
                    out var typeMapping):
                return ApplyTypeMappingsOnJsonEachExpression(jsonEachExpression, typeMapping);

            // Above, we applied the type mapping to the parameter that json_each accepts as an argument.
            // But the inferred type mapping also needs to be applied as a SQL conversion on the column projections coming out of the
            // SelectExpression containing the json_each call. So we set state to know about json_each tables and their type mappings
            // in the immediate SelectExpression, and continue visiting down (see ColumnExpression visitation below).
            case SelectExpression selectExpression:
            {
                Dictionary<string, RelationalTypeMapping>? previousSelectInferredTypeMappings = null;

                foreach (var table in selectExpression.Tables)
                {
                    if (table is TableValuedFunctionExpression { Name: "json_each", Schema: null, IsBuiltIn: true } jsonEachExpression
                        && TryGetInferredTypeMapping(
                            jsonEachExpression.Alias,
                            SqliteQueryableMethodTranslatingExpressionVisitor.JsonEachValueColumnName,
                            out var inferredTypeMapping))
                    {
                        if (previousSelectInferredTypeMappings is null)
                        {
                            previousSelectInferredTypeMappings = _currentSelectInferredTypeMappings;
                            _currentSelectInferredTypeMappings = new Dictionary<string, RelationalTypeMapping>();
                        }

                        _currentSelectInferredTypeMappings![jsonEachExpression.Alias] = inferredTypeMapping;
                    }
                }

                var visited = base.VisitExtension(expression);

                _currentSelectInferredTypeMappings = previousSelectInferredTypeMappings;

                return visited;
            }

            // Note that we match also ColumnExpressions which already have a type mapping, i.e. coming out of column collections (as
            // opposed to parameter collections, where the type mapping needs to be inferred). This is in order to apply SQL conversion
            // logic later in the process, see note in TranslateCollection.
            case ColumnExpression { Name: SqliteQueryableMethodTranslatingExpressionVisitor.JsonEachValueColumnName } columnExpression
                when _currentSelectInferredTypeMappings?.TryGetValue(columnExpression.TableAlias, out var inferredTypeMapping) is true:
                return SqliteQueryableMethodTranslatingExpressionVisitor.ApplyJsonSqlConversion(
                    columnExpression.ApplyTypeMapping(inferredTypeMapping),
                    _sqlExpressionFactory,
                    inferredTypeMapping,
                    columnExpression.IsNullable);

            default:
                return base.VisitExtension(expression);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual JsonEachExpression ApplyTypeMappingsOnJsonEachExpression(
        JsonEachExpression jsonEachExpression,
        RelationalTypeMapping inferredTypeMapping)
    {
        // Constant queryables are translated to VALUES, no need for JSON.
        // Column queryables have their type mapping from the model, so we don't ever need to apply an inferred mapping on them.
        if (jsonEachExpression.Arguments[0] is not SqlParameterExpression parameterExpression)
        {
            return jsonEachExpression;
        }

        if (_typeMappingSource.FindMapping(parameterExpression.Type, Model, inferredTypeMapping) is not SqliteStringTypeMapping
            parameterTypeMapping)
        {
            throw new InvalidOperationException("Type mapping for 'string' could not be found or was not a SqliteStringTypeMapping");
        }

        Check.DebugAssert(parameterTypeMapping.ElementTypeMapping != null, "Collection type mapping missing element mapping.");

        return jsonEachExpression.Update(
            parameterExpression.ApplyTypeMapping(parameterTypeMapping),
            jsonEachExpression.Path);
    }
}
