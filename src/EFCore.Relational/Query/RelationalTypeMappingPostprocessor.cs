// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     A visitor executed after translation, which verifies that all <see cref="SqlExpression" /> nodes have a type mapping,
///     and applies type mappings inferred for queryable constants (VALUES) and parameters (e.g. OPENJSON) back on their root tables.
/// </summary>
public class RelationalTypeMappingPostprocessor : ExpressionVisitor
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private SelectExpression? _currentSelectExpression;

    /// <summary>
    ///     The inferred type mappings to be applied back on their query roots.
    /// </summary>
    private IReadOnlyDictionary<(string TableAlias, string ColumnName), RelationalTypeMapping?> _inferredTypeMappings = null!;

    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalTypeMappingPostprocessor" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this class.</param>
    /// <param name="queryCompilationContext">The query compilation context object to use.</param>
    public RelationalTypeMappingPostprocessor(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
        RelationalQueryCompilationContext queryCompilationContext)
    {
        Model = queryCompilationContext.Model;
        _sqlExpressionFactory = relationalDependencies.SqlExpressionFactory;
    }

    /// <summary>
    ///     The model.
    /// </summary>
    protected virtual IModel Model { get; }

    /// <summary>
    ///     Processes type mappings in the expression tree.
    /// </summary>
    /// <param name="expression">The expression tree.</param>
    public virtual Expression Process(Expression expression)
    {
        // If any constant/parameter query roots exist in the query, their columns don't yet have a type mapping.
        // First, scan the query tree for inferred type mappings (e.g. based on a comparison of those columns to some regular column
        // with a type mapping).
        _inferredTypeMappings = new ColumnTypeMappingScanner().Scan(expression);

        // Then, apply those type mappings back on the constant/parameter tables (e.g. ValuesExpression).
        var visited = Visit(expression);

        return visited;
    }

    /// <summary>
    ///     Attempts to find an inferred type mapping for the given table column.
    /// </summary>
    /// <param name="tableAlias">The alias of the table containing the column for which to find the inferred type mapping.</param>
    /// <param name="columnName">The name of the column for which to find the inferred type mapping.</param>
    /// <param name="inferredTypeMapping">The inferred type mapping, or <see langword="null" /> if none could be found.</param>
    /// <returns>Whether an inferred type mapping could be found.</returns>
    protected virtual bool TryGetInferredTypeMapping(
        string tableAlias,
        string columnName,
        [NotNullWhen(true)] out RelationalTypeMapping? inferredTypeMapping)
    {
        if (_inferredTypeMappings.TryGetValue((tableAlias, columnName), out inferredTypeMapping))
        {
            // The inferred type mapping scanner records a null when two conflicting type mappings were inferred for the same
            // column.
            if (inferredTypeMapping is null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.ConflictingTypeMappingsInferredForColumn(columnName));
            }

            return true;
        }

        inferredTypeMapping = null;
        return false;
    }

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression expression)
    {
        switch (expression)
        {
            case ColumnExpression { TypeMapping: null } columnExpression
                when TryGetInferredTypeMapping(columnExpression.TableAlias, columnExpression.Name, out var typeMapping):
                return columnExpression.ApplyTypeMapping(typeMapping);

            case SelectExpression selectExpression:
                var parentSelectExpression = _currentSelectExpression;
                _currentSelectExpression = selectExpression;
                var visited = base.VisitExtension(expression);
                _currentSelectExpression = parentSelectExpression;
                return visited;

            // For ValueExpression, apply the inferred type mapping on all constants inside.
            case ValuesExpression valuesExpression:
                // By default, the ValuesExpression also contains an ordering by a synthetic increasing _ord. If the containing
                // SelectExpression doesn't project it out or require it (limit/offset), strip that out.
                // TODO: Strictly-speaking, stripping the ordering doesn't belong in this visitor which is about applying type mappings
                return ApplyTypeMappingsOnValuesExpression(
                    valuesExpression,
                    stripOrdering: _currentSelectExpression is { Limit: null, Offset: null }
                    && !_currentSelectExpression.Projection.Any(
                        p => p.Expression is ColumnExpression
                            {
                                Name: RelationalQueryableMethodTranslatingExpressionVisitor.ValuesOrderingColumnName
                            } c
                            && c.TableAlias == valuesExpression.Alias));

            // SqlExpressions without an inferred type mapping indicates a problem in EF - everything should have been inferred.
            // One exception is SqlFragmentExpression, which never has a type mapping.
            case SqlExpression { TypeMapping: null } sqlExpression and not SqlFragmentExpression and not ColumnExpression:
                throw new InvalidOperationException(RelationalStrings.NullTypeMappingInSqlTree(sqlExpression.Print()));

            case ShapedQueryExpression shapedQueryExpression:
                return shapedQueryExpression.UpdateQueryExpression(Visit(shapedQueryExpression.QueryExpression));

            default:
                return base.VisitExtension(expression);
        }
    }

    /// <summary>
    ///     Applies the given type mappings to the values projected out by the given <see cref="ValuesExpression" />.
    ///     As an optimization, it can also strip the first _ord column if it's determined that it isn't needed (most cases).
    /// </summary>
    /// <param name="valuesExpression">The <see cref="ValuesExpression" /> to apply the mappings to.</param>
    /// <param name="stripOrdering">Whether to strip the <c>_ord</c> column.</param>
    protected virtual ValuesExpression ApplyTypeMappingsOnValuesExpression(ValuesExpression valuesExpression, bool stripOrdering)
    {
        var inferredTypeMappings = TryGetInferredTypeMapping(
            valuesExpression.Alias, RelationalQueryableMethodTranslatingExpressionVisitor.ValuesValueColumnName, out var typeMapping)
            ? [null, typeMapping]
            : new RelationalTypeMapping?[] { null, null };

        Check.DebugAssert(
            valuesExpression.ColumnNames[0] == RelationalQueryableMethodTranslatingExpressionVisitor.ValuesOrderingColumnName,
            "First ValuesExpression column isn't the ordering column");
        var newColumnNames = stripOrdering
            ? valuesExpression.ColumnNames.Skip(1).ToArray()
            : valuesExpression.ColumnNames;

        var newRowValues = new RowValueExpression[valuesExpression.RowValues.Count];
        for (var i = 0; i < newRowValues.Length; i++)
        {
            var rowValue = valuesExpression.RowValues[i];
            var newValues = new SqlExpression[newColumnNames.Count];
            for (var j = 0; j < valuesExpression.ColumnNames.Count; j++)
            {
                if (j == 0 && stripOrdering)
                {
                    continue;
                }

                var value = rowValue.Values[j];

                var inferredTypeMapping = inferredTypeMappings[j];
                if (inferredTypeMapping is not null && value.TypeMapping is null)
                {
                    value = _sqlExpressionFactory.ApplyTypeMapping(value, inferredTypeMapping);

                    // We currently add explicit conversions on the first row, to ensure that the inferred types are properly typed.
                    // See #30605 for removing that when not needed.
                    if (i == 0)
                    {
                        value = new SqlUnaryExpression(ExpressionType.Convert, value, value.Type, value.TypeMapping);
                    }
                }

                newValues[j - (stripOrdering ? 1 : 0)] = value;
            }

            newRowValues[i] = new RowValueExpression(newValues);
        }

        return new ValuesExpression(valuesExpression.Alias, newRowValues, newColumnNames);
    }

    /// <summary>
    ///     A visitor which scans an expression tree and attempts to find columns for which we were missing type mappings (projected out
    ///     of queryable constant/parameter), and those type mappings have been inferred.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This handles two cases: (1) an untyped column which type-inferred in the regular way, e.g. through comparison to a typed
    ///         column, and (2) set operations where on side is typed and the other is untyped.
    ///     </para>
    ///     <para>
    ///         Note that this visitor follows type columns across subquery projections. That is, if a root constant/parameter is buried
    ///         within subqueries, and somewhere above the column projected out of a subquery is inferred, this is picked up and propagated
    ///         all the way down.
    ///     </para>
    ///     <para>
    ///         The visitor does not change the query tree in any way - it only populates the inferred type mappings it identified in
    ///         the given dictionary; actual application of the inferred type mappings happens later in
    ///         <see cref="RelationalTypeMappingPostprocessor" />. We can't do this in a single pass since untyped roots
    ///         (e.g. <see cref="ValuesExpression" /> may get visited before the type-inferred column referring to them (e.g. CROSS APPLY,
    ///         correlated subquery).
    ///     </para>
    /// </remarks>
    private sealed class ColumnTypeMappingScanner : ExpressionVisitor
    {
        private readonly Dictionary<(string TableAlias, string ColumnName), RelationalTypeMapping?> _inferredColumns = new();

        /// <summary>
        ///     A mapping of table aliases to the <see cref="TableExpressionBase" /> instances; these are used to check the table type
        ///     when we encounter a typed column pointing to it, and avoid recording inferred type mappings where we know the table
        ///     doesn't need to be inferred from the column.
        /// </summary>
        private readonly Dictionary<string, TableExpressionBase> _tableAliasMap = new();

        private string? _currentSelectTableAlias;
        private ProjectionExpression? _currentProjectionExpression;

        public IReadOnlyDictionary<(string, string), RelationalTypeMapping?> Scan(Expression expression)
        {
            _inferredColumns.Clear();
            _tableAliasMap.Clear();

            Visit(expression);

            return _inferredColumns;
        }

        protected override Expression VisitExtension(Expression node)
        {
            if (node is TableExpressionBase { Alias: string tableAlias } table)
            {
                _tableAliasMap[tableAlias] = table.UnwrapJoin();
            }

            switch (node)
            {
                // A column on a table which was possibly originally untyped (constant/parameter root or a subquery projection of one),
                // which now does have a type mapping - this would mean in got inferred in the usual manner (comparison with typed column).
                // Registered the inferred type mapping so it can be later applied back to its table, if it's untyped.
                case ColumnExpression { TypeMapping: { } typeMapping } c when WasMaybeOriginallyUntyped(c):
                {
                    RegisterInferredTypeMapping(c, typeMapping);

                    return base.VisitExtension(node);
                }

                // Similar to the above, but with ScalarSubqueryExpression the inferred type mapping is on the expression itself, while the
                // ColumnExpression we need is on the subquery's projection.
                case ScalarSubqueryExpression
                    {
                        TypeMapping: { } typeMapping,
                        Subquery.Projection: [{ Expression: ColumnExpression columnExpression }]
                    }:
                {
                    var visitedSubquery = base.VisitExtension(node);

                    if (WasMaybeOriginallyUntyped(columnExpression))
                    {
                        RegisterInferredTypeMapping(columnExpression, typeMapping);
                    }

                    return visitedSubquery;
                }

                // InExpression over a subquery: apply the item's type mapping on the subquery
                case InExpression
                    {
                        Item.TypeMapping: { } typeMapping,
                        Subquery.Projection: [{ Expression: ColumnExpression columnExpression }]
                    }:
                {
                    var visited = base.VisitExtension(node);

                    if (WasMaybeOriginallyUntyped(columnExpression))
                    {
                        RegisterInferredTypeMapping(columnExpression, typeMapping);
                    }

                    return visited;
                }

                // For set operations involving a leg with a type mapping (e.g. some column) and a leg without one (queryable constant or
                // parameter), we infer the missing type mapping from the other side.
                case SetOperationBase
                    {
                        Source1.Projection: [{ Expression: var projection1 }],
                        Source2.Projection: [{ Expression: var projection2 }]
                    }
                    when UnwrapConvert(projection1) is ColumnExpression column1 && UnwrapConvert(projection2) is ColumnExpression column2:
                {
                    // Note that we can't use WasMaybeOriginallyUntyped() here like in the other cases, since that only works after we've
                    // visited the table the column points to (and populated the mapping in _tables). But with set operations specifically,
                    // we must call RegisterInferredTypeMapping *before* visiting, to infer from one side to the other so that that
                    // inference can propagate to subqueries nested within the set operation (chicken and egg problem).
                    // This only results in RegisterInferredTypeMapping being called when it doesn't have it (i.e. _inferredColumns
                    // contains more than it has to).

                    if (projection1.TypeMapping is not null)
                    {
                        RegisterInferredTypeMapping(column2, projection1.TypeMapping);
                    }

                    if (projection2.TypeMapping is not null)
                    {
                        RegisterInferredTypeMapping(column1, projection2.TypeMapping);
                    }

                    return base.VisitExtension(node);
                }

                // Record state on the SelectExpression and ProjectionExpression so that we can associate ColumnExpressions to the
                // projections they're in (see below).
                case SelectExpression selectExpression:
                {
                    var parentSelectTableAlias = _currentSelectTableAlias;
                    _currentSelectTableAlias = selectExpression.Alias;
                    var visited = base.VisitExtension(selectExpression);
                    _currentSelectTableAlias = parentSelectTableAlias;
                    return visited;
                }

                case ProjectionExpression projectionExpression:
                {
                    var parentProjectionExpression = _currentProjectionExpression;
                    _currentProjectionExpression = projectionExpression;
                    var visited = base.VisitExtension(projectionExpression);
                    _currentProjectionExpression = parentProjectionExpression;
                    return visited;
                }

                // When visiting subqueries, we want to propagate the inferred type mappings from above into the subquery, recursively.
                // So we record state above to know which subquery and projection we're visiting; when visiting columns inside a projection
                // which has an inferred type mapping from above, we register the inferred type mapping for that column too.
                case ColumnExpression { TypeMapping: null } columnExpression
                    when _currentSelectTableAlias is not null
                    && _currentProjectionExpression is not null
                    && _inferredColumns.TryGetValue(
                        (_currentSelectTableAlias, _currentProjectionExpression.Alias), out var inferredTypeMapping)
                    && inferredTypeMapping is not null
                    && WasMaybeOriginallyUntyped(columnExpression):
                {
                    RegisterInferredTypeMapping(columnExpression, inferredTypeMapping);
                    return base.VisitExtension(node);
                }

                case ShapedQueryExpression shapedQueryExpression:
                    return shapedQueryExpression.UpdateQueryExpression(Visit(shapedQueryExpression.QueryExpression));

                default:
                    return base.VisitExtension(node);
            }

            bool WasMaybeOriginallyUntyped(ColumnExpression columnExpression)
            {
                var found = _tableAliasMap.TryGetValue(columnExpression.TableAlias, out var table);
                Check.DebugAssert(found, $"Column '{columnExpression}' points to a table that isn't in scope");

                return table switch
                {
                    // TableExpressions are always fully-typed, with type mappings coming from the model
                    TableExpression
                        => false,

                    // FromSqlExpressions always receive the default type mapping for the projected element type - we never need to infer
                    // them.
                    FromSqlExpression
                        => false,

                    SelectExpression subquery
                        => subquery.Projection.FirstOrDefault(p => p.Alias == columnExpression.Name) is { Expression.TypeMapping: null },

                    JoinExpressionBase
                        => throw new UnreachableException("Impossible: nested join"),

                    // Any other table expression is considered a root (TableValuedFunctionExpression, ValuesExpression...) which *may* be
                    // untyped, so we record the possible inference (note that TableValuedFunctionExpression may be typed, or not)
                    _ => true,
                };
            }

            SqlExpression UnwrapConvert(SqlExpression expression)
                => expression is SqlUnaryExpression { OperatorType: ExpressionType.Convert } convert
                    ? UnwrapConvert(convert.Operand)
                    : expression;
        }

        private void RegisterInferredTypeMapping(ColumnExpression columnExpression, RelationalTypeMapping inferredTypeMapping)
        {
            var tableAlias = columnExpression.TableAlias;

            if (_inferredColumns.TryGetValue((tableAlias, columnExpression.Name), out var knownTypeMapping)
                && knownTypeMapping is not null
                && inferredTypeMapping.StoreType != knownTypeMapping.StoreType)
            {
                // A different type mapping was already inferred for this column - we have a conflict.
                // Null out the value for the inferred type mapping as an indication of the conflict. If it turns out that we need the
                // inferred mapping later, during the application phase, we'll throw an exception at that point (not all the inferred type
                // mappings here will actually be needed, so we don't want to needlessly throw here).
                _inferredColumns[(tableAlias, columnExpression.Name)] = null;
                return;
            }

            _inferredColumns[(tableAlias, columnExpression.Name)] = inferredTypeMapping;
        }
    }
}
