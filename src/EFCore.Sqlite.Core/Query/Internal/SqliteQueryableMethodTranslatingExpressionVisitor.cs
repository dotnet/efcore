// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
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
public class SqliteQueryableMethodTranslatingExpressionVisitor : RelationalQueryableMethodTranslatingExpressionVisitor
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteQueryableMethodTranslatingExpressionVisitor(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, relationalDependencies, queryCompilationContext)
    {
        _typeMappingSource = relationalDependencies.TypeMappingSource;
        _sqlExpressionFactory = relationalDependencies.SqlExpressionFactory;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqliteQueryableMethodTranslatingExpressionVisitor(
        SqliteQueryableMethodTranslatingExpressionVisitor parentVisitor)
        : base(parentVisitor)
    {
        _typeMappingSource = parentVisitor._typeMappingSource;
        _sqlExpressionFactory = parentVisitor._sqlExpressionFactory;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
        => new SqliteQueryableMethodTranslatingExpressionVisitor(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateOrderBy(
        ShapedQueryExpression source,
        LambdaExpression keySelector,
        bool ascending)
    {
        var translation = base.TranslateOrderBy(source, keySelector, ascending);
        if (translation == null)
        {
            return null;
        }

        var orderingExpression = ((SelectExpression)translation.QueryExpression).Orderings.Last();
        var orderingExpressionType = GetProviderType(orderingExpression.Expression);
        if (orderingExpressionType == typeof(DateTimeOffset)
            || orderingExpressionType == typeof(decimal)
            || orderingExpressionType == typeof(TimeSpan)
            || orderingExpressionType == typeof(ulong))
        {
            throw new NotSupportedException(
                SqliteStrings.OrderByNotSupported(orderingExpressionType.ShortDisplayName()));
        }

        return translation;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateThenBy(
        ShapedQueryExpression source,
        LambdaExpression keySelector,
        bool ascending)
    {
        var translation = base.TranslateThenBy(source, keySelector, ascending);
        if (translation == null)
        {
            return null;
        }

        var orderingExpression = ((SelectExpression)translation.QueryExpression).Orderings.Last();
        var orderingExpressionType = GetProviderType(orderingExpression.Expression);
        if (orderingExpressionType == typeof(DateTimeOffset)
            || orderingExpressionType == typeof(decimal)
            || orderingExpressionType == typeof(TimeSpan)
            || orderingExpressionType == typeof(ulong))
        {
            throw new NotSupportedException(
                SqliteStrings.OrderByNotSupported(orderingExpressionType.ShortDisplayName()));
        }

        return translation;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateCount(ShapedQueryExpression source, LambdaExpression? predicate)
    {
        // Simplify x.Array.Count() => json_array_length(x.Array) instead of SELECT COUNT(*) FROM json_each(x.Array)
        if (predicate is null && source.QueryExpression is SelectExpression
            {
                Tables: [TableValuedFunctionExpression { Name: "json_each", Schema: null, IsBuiltIn: true, Arguments: [var array] }],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null
            })
        {
            var translation = _sqlExpressionFactory.Function(
                "json_array_length",
                new[] { array },
                nullable: true,
                argumentsPropagateNullability: new[] { true },
                typeof(int));

            return source.UpdateQueryExpression(_sqlExpressionFactory.Select(translation));
        }

        return base.TranslateCount(source, predicate);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression TranslateCollection(
        SqlExpression sqlExpression,
        RelationalTypeMapping? elementTypeMapping,
        string tableAlias)
    {
        var elementClrType = sqlExpression.Type.GetSequenceType();

        var jsonEachExpression = new TableValuedFunctionExpression(tableAlias, "json_each", new[] { sqlExpression });

        // TODO: When we have metadata to determine if the element is nullable, pass that here to SelectExpression
        var selectExpression = new SelectExpression(
            jsonEachExpression, columnName: "value", columnType: elementClrType, columnTypeMapping: elementTypeMapping,
            isColumnNullable: null);

        // TODO: SQLite does have REAL and BLOB types, which JSON does not. Need to possibly cast to that.
        if (elementTypeMapping is not null)
        {
            // TODO: In any case, we still ned to pass through the type mapping API for doing any conversions (e.g. for datetime, from JSON
            // ISO8601 to SQLite's format without the T), see #30677. Do this here.
        }

        // Append an ordering for the json_each 'key' column.
        selectExpression.AppendOrdering(
            new OrderingExpression(
                selectExpression.CreateColumnExpression(
                    jsonEachExpression,
                    "key",
                    typeof(int),
                    typeMapping: _typeMappingSource.FindMapping(typeof(int)),
                    columnNullable: false),
                ascending: true));

        Expression shaperExpression = new ProjectionBindingExpression(
            selectExpression, new ProjectionMember(), elementClrType.MakeNullable());

        if (elementClrType != shaperExpression.Type)
        {
            Check.DebugAssert(
                elementClrType.MakeNullable() == shaperExpression.Type,
                "expression.Type must be nullable of targetType");

            shaperExpression = Expression.Convert(shaperExpression, elementClrType);
        }

        return new ShapedQueryExpression(selectExpression, shaperExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateElementAtOrDefault(
        ShapedQueryExpression source,
        Expression index,
        bool returnDefault)
    {
        if (!returnDefault
            && source.QueryExpression is SelectExpression
            {
                Tables:
                [
                    TableValuedFunctionExpression
                    {
                        Name: "json_each", Schema: null, IsBuiltIn: true, Arguments: [var jsonArrayColumn]
                    } jsonEachExpression
                ],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Orderings: [{ Expression: ColumnExpression { Name: "key" } orderingColumn, IsAscending: true }],
                Limit: null,
                Offset: null
            } selectExpression
            && orderingColumn.Table == jsonEachExpression
            && TranslateExpression(index) is { } translatedIndex)
        {
            // Index on JSON array

            // Extract the column projected out of the source, and simplify the subquery to a simple JsonScalarExpression
            var shaperExpression = source.ShaperExpression;
            if (shaperExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression
                && unaryExpression.Operand.Type.IsNullableType()
                && unaryExpression.Operand.Type.UnwrapNullableType() == unaryExpression.Type)
            {
                shaperExpression = unaryExpression.Operand;
            }

            if (shaperExpression is ProjectionBindingExpression projectionBindingExpression
                && selectExpression.GetProjection(projectionBindingExpression) is ColumnExpression projectionColumn)
            {
                SqlExpression translation = new JsonScalarExpression(
                    jsonArrayColumn,
                    new[] { new PathSegment(translatedIndex) },
                    projectionColumn.Type,
                    projectionColumn.TypeMapping,
                    projectionColumn.IsNullable);

                // If we have a type mapping (i.e. translating over a column rather than a parameter), apply any necessary server-side
                // conversions.
                if (projectionColumn.TypeMapping is not null)
                {
                    translation = ApplyTypeMappingOnColumn(translation, projectionColumn.TypeMapping, projectionColumn.IsNullable);
                }

                return source.UpdateQueryExpression(_sqlExpressionFactory.Select(translation));
            }
        }

        return base.TranslateElementAtOrDefault(source, index, returnDefault);
    }

    private static Type GetProviderType(SqlExpression expression)
        => expression.TypeMapping?.Converter?.ProviderClrType
            ?? expression.TypeMapping?.ClrType
            ?? expression.Type;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression ApplyInferredTypeMappings(
        Expression expression,
        IReadOnlyDictionary<(TableExpressionBase, string), RelationalTypeMapping> inferredTypeMappings)
        => new SqliteInferredTypeMappingApplier(_typeMappingSource, _sqlExpressionFactory, inferredTypeMappings).Visit(expression);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected class SqliteInferredTypeMappingApplier : RelationalInferredTypeMappingApplier
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private Dictionary<TableExpressionBase, RelationalTypeMapping>? _currentSelectInferredTypeMappings;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqliteInferredTypeMappingApplier(
            IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory,
            IReadOnlyDictionary<(TableExpressionBase, string), RelationalTypeMapping> inferredTypeMappings)
            : base(inferredTypeMappings)
            => (_typeMappingSource, _sqlExpressionFactory) = (typeMappingSource, sqlExpressionFactory);

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
                case TableValuedFunctionExpression { Name: "json_each", Schema: null, IsBuiltIn: true } jsonEachExpression
                    when InferredTypeMappings.TryGetValue((jsonEachExpression, "value"), out var typeMapping):
                    return ApplyTypeMappingsOnJsonEachExpression(jsonEachExpression, typeMapping);

                // Above, we applied the type mapping the the parameter that json_each accepts as an argument.
                // But the inferred type mapping also needs to be applied as a SQL conversion on the column projections coming out of the
                // SelectExpression containing the json_each call. So we set state to know about json_each tables and their type mappings
                // in the immediate SelectExpression, and continue visiting down (see ColumnExpression visitation below).
                case SelectExpression selectExpression:
                {
                    Dictionary<TableExpressionBase, RelationalTypeMapping>? previousSelectInferredTypeMappings = null;

                    foreach (var table in selectExpression.Tables)
                    {
                        if (table is TableValuedFunctionExpression { Name: "json_each", Schema: null, IsBuiltIn: true } jsonEachExpression
                            && InferredTypeMappings.TryGetValue((jsonEachExpression, "value"), out var inferredTypeMapping))
                        {
                            if (previousSelectInferredTypeMappings is null)
                            {
                                previousSelectInferredTypeMappings = _currentSelectInferredTypeMappings;
                                _currentSelectInferredTypeMappings = new();
                            }

                            _currentSelectInferredTypeMappings![jsonEachExpression] = inferredTypeMapping;
                        }
                    }

                    var visited = base.VisitExtension(expression);

                    _currentSelectInferredTypeMappings = previousSelectInferredTypeMappings;

                    return visited;
                }

                case ColumnExpression { Name: "value" } columnExpression
                    when _currentSelectInferredTypeMappings is not null
                    && _currentSelectInferredTypeMappings.TryGetValue(columnExpression.Table, out var inferredTypeMapping):
                    return ApplyTypeMappingOnColumn(columnExpression, inferredTypeMapping, columnExpression.IsNullable);

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
        protected virtual TableValuedFunctionExpression ApplyTypeMappingsOnJsonEachExpression(
            TableValuedFunctionExpression jsonEachExpression,
            RelationalTypeMapping inferredTypeMapping)
        {
            // Constant queryables are translated to VALUES, no need for JSON.
            // Column queryables have their type mapping from the model, so we don't ever need to apply an inferred mapping on them.
            if (jsonEachExpression.Arguments[0] is not SqlParameterExpression parameterExpression)
            {
                return jsonEachExpression;
            }

            // TODO: We shouldn't need to manually construct the JSON string type mapping this way; we need to be able to provide the
            // TODO: element's store type mapping as input to _typeMappingSource.FindMapping. #30730
            if (_typeMappingSource.FindMapping(typeof(string)) is not SqliteStringTypeMapping parameterTypeMapping)
            {
                throw new InvalidOperationException("Type mapping for 'string' could not be found or was not a SqliteStringTypeMapping");
            }

            parameterTypeMapping = (SqliteStringTypeMapping)parameterTypeMapping
                .Clone(new CollectionToJsonStringConverter(parameterExpression.Type, inferredTypeMapping));

            parameterTypeMapping = (SqliteStringTypeMapping)parameterTypeMapping.CloneWithElementTypeMapping(inferredTypeMapping);

            return jsonEachExpression.Update(new[] { parameterExpression.ApplyTypeMapping(parameterTypeMapping) });
        }
    }

    private static SqlExpression ApplyTypeMappingOnColumn(SqlExpression expression, RelationalTypeMapping typeMapping, bool isNullable)
        => typeMapping switch
        {
            // TODO: These server-side conversions need to be managed on the type mapping, #30677

            // The "standard" JSON timestamp representation is ISO8601, with a T between date and time; but SQLite's representation has
            // no T. Apply a conversion on the value coming out of json_each.
            SqliteDateTimeTypeMapping => new SqlFunctionExpression(
                "datetime", new[] { expression }, isNullable, new[] { true }, typeof(DateTime), typeMapping),

            SqliteGuidTypeMapping => new SqlFunctionExpression(
                "upper", new[] { expression }, isNullable, new[] { true }, typeof(Guid), typeMapping),

            _ => expression
        };
}
