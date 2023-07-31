// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
public class RelationalQueryableMethodTranslatingExpressionVisitor : QueryableMethodTranslatingExpressionVisitor
{
    private const string SqlQuerySingleColumnAlias = "Value";
    private const string ValuesOrderingColumnName = "_ord", ValuesValueColumnName = "Value";

    private readonly RelationalSqlTranslatingExpressionVisitor _sqlTranslator;
    private readonly SharedTypeEntityExpandingExpressionVisitor _sharedTypeEntityExpandingExpressionVisitor;
    private readonly RelationalProjectionBindingExpressionVisitor _projectionBindingExpressionVisitor;
    private readonly QueryCompilationContext _queryCompilationContext;
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly bool _subquery;

    /// <summary>
    ///     Creates a new instance of the <see cref="QueryableMethodTranslatingExpressionVisitor" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this class.</param>
    /// <param name="queryCompilationContext">The query compilation context object to use.</param>
    public RelationalQueryableMethodTranslatingExpressionVisitor(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, queryCompilationContext, subquery: false)
    {
        RelationalDependencies = relationalDependencies;

        var sqlExpressionFactory = relationalDependencies.SqlExpressionFactory;
        _queryCompilationContext = queryCompilationContext;
        _sqlTranslator = relationalDependencies.RelationalSqlTranslatingExpressionVisitorFactory.Create(queryCompilationContext, this);
        _sharedTypeEntityExpandingExpressionVisitor =
            new SharedTypeEntityExpandingExpressionVisitor(_sqlTranslator, sqlExpressionFactory);
        _projectionBindingExpressionVisitor = new RelationalProjectionBindingExpressionVisitor(this, _sqlTranslator);
        _typeMappingSource = relationalDependencies.TypeMappingSource;
        _sqlExpressionFactory = sqlExpressionFactory;
        _subquery = false;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalQueryableMethodTranslatingExpressionVisitorDependencies RelationalDependencies { get; }

    /// <summary>
    ///     Creates a new instance of the <see cref="QueryableMethodTranslatingExpressionVisitor" /> class.
    /// </summary>
    /// <param name="parentVisitor">A parent visitor to create subquery visitor for.</param>
    protected RelationalQueryableMethodTranslatingExpressionVisitor(
        RelationalQueryableMethodTranslatingExpressionVisitor parentVisitor)
        : base(parentVisitor.Dependencies, parentVisitor.QueryCompilationContext, subquery: true)
    {
        RelationalDependencies = parentVisitor.RelationalDependencies;
        _queryCompilationContext = parentVisitor._queryCompilationContext;
        _sqlTranslator = RelationalDependencies.RelationalSqlTranslatingExpressionVisitorFactory.Create(
            parentVisitor._queryCompilationContext, parentVisitor);
        _sharedTypeEntityExpandingExpressionVisitor =
            new SharedTypeEntityExpandingExpressionVisitor(_sqlTranslator, parentVisitor._sqlExpressionFactory);
        _projectionBindingExpressionVisitor = new RelationalProjectionBindingExpressionVisitor(this, _sqlTranslator);
        _typeMappingSource = parentVisitor._typeMappingSource;
        _sqlExpressionFactory = parentVisitor._sqlExpressionFactory;
        _subquery = true;
    }

    /// <inheritdoc />
    public override Expression Translate(Expression expression)
    {
        var visited = base.Translate(expression);

        if (!_subquery)
        {
            // We've finished translating the entire query.

            // If any constant/parameter query roots exist in the query, their columns don't yet have a type mapping.
            // First, scan the query tree for inferred type mappings (e.g. based on a comparison of those columns to some regular column
            // with a type mapping).
            var inferredColumns = new ColumnTypeMappingScanner().Scan(visited);

            // Then, apply those type mappings back on the constant/parameter tables (e.g. ValuesExpression).
            visited = ApplyInferredTypeMappings(visited, inferredColumns);
        }

        return visited;
    }

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        switch (extensionExpression)
        {
            case FromSqlQueryRootExpression fromSqlQueryRootExpression:
                return CreateShapedQueryExpression(
                    fromSqlQueryRootExpression.EntityType,
                    _sqlExpressionFactory.Select(
                        fromSqlQueryRootExpression.EntityType,
                        new FromSqlExpression(
                            fromSqlQueryRootExpression.EntityType.GetDefaultMappings().Single().Table,
                            fromSqlQueryRootExpression.Sql,
                            fromSqlQueryRootExpression.Argument)));

            case TableValuedFunctionQueryRootExpression tableValuedFunctionQueryRootExpression:
            {
                var function = tableValuedFunctionQueryRootExpression.Function;
                var arguments = new List<SqlExpression>();
                foreach (var arg in tableValuedFunctionQueryRootExpression.Arguments)
                {
                    var sqlArgument = TranslateExpression(arg);
                    if (sqlArgument == null)
                    {
                        string call;
                        var methodInfo = function.DbFunctions.Last().MethodInfo;
                        if (methodInfo != null)
                        {
                            var methodCall = Expression.Call(
                                // Declaring types would be derived db context.
                                Expression.Constant(null, methodInfo.DeclaringType!),
                                methodInfo,
                                tableValuedFunctionQueryRootExpression.Arguments);

                            call = methodCall.Print();
                        }
                        else
                        {
                            call = $"{function.DbFunctions.Last().Name}()";
                        }

                        throw new InvalidOperationException(
                            TranslationErrorDetails == null
                                ? CoreStrings.TranslationFailed(call)
                                : CoreStrings.TranslationFailedWithDetails(call, TranslationErrorDetails));
                    }

                    arguments.Add(sqlArgument);
                }

                var entityType = tableValuedFunctionQueryRootExpression.EntityType;

                var translation = new TableValuedFunctionExpression(function, arguments);
                var queryExpression = _sqlExpressionFactory.Select(entityType, translation);

                return CreateShapedQueryExpression(entityType, queryExpression);
            }

            case EntityQueryRootExpression entityQueryRootExpression
                when entityQueryRootExpression.GetType() == typeof(EntityQueryRootExpression)
                && entityQueryRootExpression.EntityType.GetSqlQueryMappings().FirstOrDefault(m => m.IsDefaultSqlQueryMapping)?.SqlQuery is
                    ISqlQuery sqlQuery:
                return CreateShapedQueryExpression(
                    entityQueryRootExpression.EntityType,
                    _sqlExpressionFactory.Select(
                        entityQueryRootExpression.EntityType,
                        new FromSqlExpression(
                            entityQueryRootExpression.EntityType.GetDefaultMappings().Single().Table,
                            sqlQuery.Sql,
                            Expression.Constant(Array.Empty<object>(), typeof(object[])))));

            case GroupByShaperExpression groupByShaperExpression:
                var groupShapedQueryExpression = groupByShaperExpression.GroupingEnumerable;
                var groupClonedSelectExpression = ((SelectExpression)groupShapedQueryExpression.QueryExpression).Clone();
                return new ShapedQueryExpression(
                    groupClonedSelectExpression,
                    new QueryExpressionReplacingExpressionVisitor(
                            groupShapedQueryExpression.QueryExpression, groupClonedSelectExpression)
                        .Visit(groupShapedQueryExpression.ShaperExpression));

            case ShapedQueryExpression shapedQueryExpression:
                var clonedSelectExpression = ((SelectExpression)shapedQueryExpression.QueryExpression).Clone();
                return new ShapedQueryExpression(
                    clonedSelectExpression,
                    new QueryExpressionReplacingExpressionVisitor(shapedQueryExpression.QueryExpression, clonedSelectExpression)
                        .Visit(shapedQueryExpression.ShaperExpression));

            case SqlQueryRootExpression sqlQueryRootExpression:
            {
                var typeMapping = RelationalDependencies.TypeMappingSource.FindMapping(
                    sqlQueryRootExpression.ElementType, RelationalDependencies.Model);

                if (typeMapping == null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.SqlQueryUnmappedType(sqlQueryRootExpression.ElementType.DisplayName()));
                }

                var selectExpression = new SelectExpression(
                    new FromSqlExpression("t", sqlQueryRootExpression.Sql, sqlQueryRootExpression.Argument), SqlQuerySingleColumnAlias,
                    sqlQueryRootExpression.Type, typeMapping);

                Expression shaperExpression = new ProjectionBindingExpression(
                    selectExpression, new ProjectionMember(), sqlQueryRootExpression.ElementType.MakeNullable());

                if (sqlQueryRootExpression.ElementType != shaperExpression.Type)
                {
                    Check.DebugAssert(
                        sqlQueryRootExpression.ElementType.MakeNullable() == shaperExpression.Type,
                        "expression.Type must be nullable of targetType");

                    shaperExpression = Expression.Convert(shaperExpression, sqlQueryRootExpression.ElementType);
                }

                return new ShapedQueryExpression(selectExpression, shaperExpression);
            }

            case InlineQueryRootExpression inlineQueryRootExpression:
                return VisitInlineQueryRoot(inlineQueryRootExpression) ?? base.VisitExtension(extensionExpression);

            case ParameterQueryRootExpression parameterQueryRootExpression:
                var sqlParameterExpression =
                    _sqlTranslator.Visit(parameterQueryRootExpression.ParameterExpression) as SqlParameterExpression;
                Check.DebugAssert(sqlParameterExpression is not null, "sqlParameterExpression is not null");
                return TranslateCollection(
                        sqlParameterExpression,
                        elementTypeMapping: null,
                        char.ToLowerInvariant(sqlParameterExpression.Name.First(c => c != '_')).ToString())
                    ?? base.VisitExtension(extensionExpression);

            case JsonQueryExpression jsonQueryExpression:
                return TransformJsonQueryToTable(jsonQueryExpression) ?? base.VisitExtension(extensionExpression);

            default:
                return base.VisitExtension(extensionExpression);
        }
    }

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        var method = methodCallExpression.Method;
        if (method.DeclaringType == typeof(RelationalQueryableExtensions))
        {
            var source = Visit(methodCallExpression.Arguments[0]);
            if (source is ShapedQueryExpression shapedQueryExpression)
            {
                var genericMethod = method.IsGenericMethod ? method.GetGenericMethodDefinition() : null;
                switch (method.Name)
                {
                    case nameof(RelationalQueryableExtensions.ExecuteDelete)
                        when genericMethod == RelationalQueryableExtensions.ExecuteDeleteMethodInfo:
                        return TranslateExecuteDelete(shapedQueryExpression)
                            ?? throw new InvalidOperationException(
                                RelationalStrings.NonQueryTranslationFailedWithDetails(
                                    methodCallExpression.Print(), TranslationErrorDetails));

                    case nameof(RelationalQueryableExtensions.ExecuteUpdate)
                        when genericMethod == RelationalQueryableExtensions.ExecuteUpdateMethodInfo:
                        return TranslateExecuteUpdate(shapedQueryExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote())
                            ?? throw new InvalidOperationException(
                                RelationalStrings.NonQueryTranslationFailedWithDetails(
                                    methodCallExpression.Print(), TranslationErrorDetails));
                }
            }
        }

        var translated = base.VisitMethodCall(methodCallExpression);

        if (translated == QueryCompilationContext.NotTranslatedExpression)
        {
            // Attempt to translate access into a primitive collection property (i.e. array column)
            if (_sqlTranslator.TryTranslatePropertyAccess(methodCallExpression, out var propertyAccessExpression)
                && propertyAccessExpression is
                {
                    TypeMapping.ElementTypeMapping: RelationalTypeMapping elementTypeMapping
                } collectionPropertyAccessExpression)
            {
                var tableAlias = collectionPropertyAccessExpression switch
                {
                    ColumnExpression c => c.Name[..1].ToLowerInvariant(),
                    JsonScalarExpression { Path: [.., { PropertyName: string propertyName }] } => propertyName[..1].ToLowerInvariant(),
                    _ => "j"
                };

                if (TranslateCollection(collectionPropertyAccessExpression, elementTypeMapping, tableAlias) is
                    { } primitiveCollectionTranslation)
                {
                    return primitiveCollectionTranslation;
                }
            }

            // For Contains over a collection parameter, if the provider hasn't implemented TranslateCollection (e.g. OPENJSON on SQL
            // Server), we need to fall back to the previous IN translation.
            if (method.IsGenericMethod
                && method.GetGenericMethodDefinition() == QueryableMethods.Contains
                && methodCallExpression.Arguments[0] is ParameterQueryRootExpression parameterSource
                && TranslateExpression(methodCallExpression.Arguments[1]) is SqlExpression item
                && _sqlTranslator.Visit(parameterSource.ParameterExpression) is SqlParameterExpression sqlParameterExpression)
            {
                var inExpression = _sqlExpressionFactory.In(item, sqlParameterExpression);
                var selectExpression = new SelectExpression(inExpression);
                var shaperExpression = Expression.Convert(
                    new ProjectionBindingExpression(selectExpression, new ProjectionMember(), typeof(bool?)), typeof(bool));
                var shapedQueryExpression = new ShapedQueryExpression(selectExpression, shaperExpression)
                    .UpdateResultCardinality(ResultCardinality.Single);
                return shapedQueryExpression;
            }
        }

        return translated;
    }

    /// <summary>
    ///     Translates a parameter or column collection. Providers can override this to translate e.g. int[] columns/parameters/constants to
    ///     a queryable table (OPENJSON on SQL Server, unnest on PostgreSQL...). The default implementation always returns
    ///     <see langword="null" /> (no translation).
    /// </summary>
    /// <remarks>
    ///     Inline collections aren't passed to this method; see <see cref="VisitInlineQueryRoot" /> for the translation of inline
    ///     collections.
    /// </remarks>
    /// <param name="sqlExpression">The expression to try to translate as a primitive collection expression.</param>
    /// <param name="elementTypeMapping">
    ///     The type mapping of the collection's element, or <see langword="null" /> when it's not known (i.e. for parameters).
    /// </param>
    /// <param name="tableAlias">
    ///     Provides an alias to be used for the table returned from translation, which will represent the collection.
    /// </param>
    /// <returns>A <see cref="ShapedQueryExpression" /> if the translation was successful, otherwise <see langword="null" />.</returns>
    protected virtual ShapedQueryExpression? TranslateCollection(
        SqlExpression sqlExpression,
        RelationalTypeMapping? elementTypeMapping,
        string tableAlias)
        => null;

    /// <summary>
    ///     Invoked when LINQ operators are composed over a collection within a JSON document.
    ///     Transforms the provided <see cref="JsonQueryExpression" /> - representing access to the collection - into a provider-specific
    ///     means to expand the JSON array into a relational table/rowset (e.g. SQL Server OPENJSON).
    /// </summary>
    /// <param name="jsonQueryExpression">The <see cref="JsonQueryExpression" /> referencing the JSON array.</param>
    /// <returns>A <see cref="ShapedQueryExpression" /> if the translation was successful, otherwise <see langword="null" />.</returns>
    protected virtual ShapedQueryExpression? TransformJsonQueryToTable(JsonQueryExpression jsonQueryExpression)
    {
        AddTranslationErrorDetails(RelationalStrings.JsonQueryLinqOperatorsNotSupported);
        return null;
    }

    /// <summary>
    ///     Translates an inline collection into a queryable SQL VALUES expression.
    /// </summary>
    /// <param name="inlineQueryRootExpression">The inline collection to be translated.</param>
    /// <returns>A queryable SQL VALUES expression.</returns>
    protected virtual ShapedQueryExpression? VisitInlineQueryRoot(InlineQueryRootExpression inlineQueryRootExpression)
    {
        var elementType = inlineQueryRootExpression.ElementType;

        var rowExpressions = new List<RowValueExpression>();
        var encounteredNull = false;
        var intTypeMapping = _typeMappingSource.FindMapping(typeof(int), RelationalDependencies.Model);

        for (var i = 0; i < inlineQueryRootExpression.Values.Count; i++)
        {
            // Note that we specifically don't apply the default type mapping to the translation, to allow it to get inferred later based
            // on usage.
            if (TranslateExpression(inlineQueryRootExpression.Values[i], applyDefaultTypeMapping: false)
                is not SqlExpression translatedValue)
            {
                return null;
            }

            // TODO: Poor man's null semantics: in SqlNullabilityProcessor we don't fully handle the nullability of SelectExpression
            // projections. Whether the SelectExpression's projection is nullable or not is determined here in translation, but at this
            // point we don't know how to properly calculate nullability (and can't look at parameters).
            // So for now, we assume the projected column is nullable if we see anything but non-null constants and non-nullable columns.
            encounteredNull |=
                translatedValue is not SqlConstantExpression { Value: not null } and not ColumnExpression { IsNullable: false };

            rowExpressions.Add(
                new RowValueExpression(
                    new[]
                    {
                        // Since VALUES may not guarantee row ordering, we add an _ord value by which we'll order.
                        _sqlExpressionFactory.Constant(i, intTypeMapping),
                        // Note that for the actual value, we must leave the type mapping null to allow it to get inferred later based on usage
                        translatedValue
                    }));
        }

        if (rowExpressions.Count == 0)
        {
            AddTranslationErrorDetails(RelationalStrings.EmptyCollectionNotSupportedAsInlineQueryRoot);
            return null;
        }

        var valuesExpression = new ValuesExpression("v", rowExpressions, new[] { ValuesOrderingColumnName, ValuesValueColumnName });

        // Note: we leave the element type mapping null, to allow it to get inferred based on queryable operators composed on top.
        var selectExpression = new SelectExpression(
            valuesExpression,
            ValuesValueColumnName,
            columnType: elementType.UnwrapNullableType(),
            columnTypeMapping: null,
            isColumnNullable: encounteredNull);

        selectExpression.AppendOrdering(
            new OrderingExpression(
                selectExpression.CreateColumnExpression(
                    valuesExpression,
                    ValuesOrderingColumnName,
                    typeof(int),
                    intTypeMapping,
                    columnNullable: false),
                ascending: true));

        Expression shaperExpression = new ProjectionBindingExpression(
            selectExpression, new ProjectionMember(), encounteredNull ? elementType.MakeNullable() : elementType);

        if (elementType != shaperExpression.Type)
        {
            Check.DebugAssert(
                elementType.MakeNullable() == shaperExpression.Type,
                "expression.Type must be nullable of targetType");

            shaperExpression = Expression.Convert(shaperExpression, elementType);
        }

        return new ShapedQueryExpression(selectExpression, shaperExpression);
    }

    /// <inheritdoc />
    protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
        => new RelationalQueryableMethodTranslatingExpressionVisitor(this);

    /// <inheritdoc />
    protected override ShapedQueryExpression CreateShapedQueryExpression(IEntityType entityType)
        => CreateShapedQueryExpression(entityType, _sqlExpressionFactory.Select(entityType));

    private static ShapedQueryExpression CreateShapedQueryExpression(IEntityType entityType, SelectExpression selectExpression)
        => new(
            selectExpression,
            new RelationalEntityShaperExpression(
                entityType,
                new ProjectionBindingExpression(
                    selectExpression,
                    new ProjectionMember(),
                    typeof(ValueBuffer)),
                false));

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateAll(ShapedQueryExpression source, LambdaExpression predicate)
    {
        var translation = TranslateLambdaExpression(source, predicate);
        if (translation == null)
        {
            return null;
        }

        var subquery = (SelectExpression)source.QueryExpression;

        // Negate the predicate, unless it's already negated, in which case remove that.
        subquery.ApplyPredicate(
            translation is SqlUnaryExpression { OperatorType: ExpressionType.Not, Operand: var nestedOperand }
                ? nestedOperand
                : _sqlExpressionFactory.Not(translation));

        subquery.ReplaceProjection(new List<Expression>());
        subquery.ApplyProjection();
        if (subquery.Limit == null
            && subquery.Offset == null)
        {
            subquery.ClearOrdering();
        }

        translation = _sqlExpressionFactory.Not(_sqlExpressionFactory.Exists(subquery));
        subquery = _sqlExpressionFactory.Select(translation);

        return source.Update(
            subquery,
            Expression.Convert(new ProjectionBindingExpression(subquery, new ProjectionMember(), typeof(bool?)), typeof(bool)));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateAny(ShapedQueryExpression source, LambdaExpression? predicate)
    {
        if (predicate != null)
        {
            var translatedSource = TranslateWhere(source, predicate);
            if (translatedSource == null)
            {
                return null;
            }

            source = translatedSource;
        }

        var subquery = (SelectExpression)source.QueryExpression;
        subquery.ReplaceProjection(new List<Expression>());
        subquery.ApplyProjection();
        if (subquery.Limit == null
            && subquery.Offset == null)
        {
            subquery.ClearOrdering();
        }

        var translation = _sqlExpressionFactory.Exists(subquery);
        var selectExpression = _sqlExpressionFactory.Select(translation);

        return source.Update(
            selectExpression,
            Expression.Convert(new ProjectionBindingExpression(selectExpression, new ProjectionMember(), typeof(bool?)), typeof(bool)));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateAverage(
        ShapedQueryExpression source,
        LambdaExpression? selector,
        Type resultType)
        => TranslateAggregateWithSelector(source, selector, QueryableMethods.GetAverageWithoutSelector, throwWhenEmpty: true, resultType);

    /// <inheritdoc />
    protected override ShapedQueryExpression TranslateCast(ShapedQueryExpression source, Type resultType)
        => source.ShaperExpression.Type != resultType
            ? source.UpdateShaperExpression(Expression.Convert(source.ShaperExpression, resultType))
            : source;

    /// <inheritdoc />
    protected override ShapedQueryExpression TranslateConcat(ShapedQueryExpression source1, ShapedQueryExpression source2)
    {
        ((SelectExpression)source1.QueryExpression).ApplyUnion((SelectExpression)source2.QueryExpression, distinct: false);

        return source1.UpdateShaperExpression(
            MatchShaperNullabilityForSetOperation(source1.ShaperExpression, source2.ShaperExpression, makeNullable: true));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateContains(ShapedQueryExpression source, Expression item)
    {
        // Note that we don't apply the default type mapping to the item in order to allow it to be inferred from e.g. the subquery
        // projection on the other side.
        if (TranslateExpression(item, applyDefaultTypeMapping: false) is not SqlExpression translatedItem
            || !TryGetProjection(source, out var projection))
        {
            // If the item can't be translated, we can't translate to an IN expression.
            // However, attempt to translate as Any since that passes through Where predicate translation, which e.g. does entity equality.
            var anyLambdaParameter = Expression.Parameter(item.Type, "p");
            var anyLambda = Expression.Lambda(
                Infrastructure.ExpressionExtensions.CreateEqualsExpression(anyLambdaParameter, item),
                anyLambdaParameter);

            return TranslateAny(source, anyLambda);
        }

        // Pattern-match Contains over ValuesExpression, translating to simplified 'item IN (1, 2, 3)' with constant elements
        if (source.QueryExpression is SelectExpression
            {
                Tables:
                [
                    ValuesExpression
                    {
                        RowValues: [{ Values.Count: 2 }, ..],
                        ColumnNames: [ValuesOrderingColumnName, ValuesValueColumnName]
                    } valuesExpression
                ],
                Predicate: null,
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null,
                // Note that in the context of Contains we don't care about orderings
            }
            // Make sure that the source projects the column from the ValuesExpression directly, i.e. no projection out with some expression
            && projection is ColumnExpression projectedColumn
            && projectedColumn.Table == valuesExpression)
        {
            var values = new SqlExpression[valuesExpression.RowValues.Count];
            for (var i = 0; i < values.Length; i++)
            {
                // Skip the first value (_ord), which is irrelevant for Contains
                values[i] = valuesExpression.RowValues[i].Values[1];
            }

            var inExpression = _sqlExpressionFactory.In(translatedItem, values);
            return source.Update(_sqlExpressionFactory.Select(inExpression), source.ShaperExpression);
        }

        // Translate to IN with a subquery.
        // Note that because of null semantics, this may get transformed to an EXISTS subquery in SqlNullabilityProcessor.
        var subquery = (SelectExpression)source.QueryExpression;
        if (subquery.Limit == null
            && subquery.Offset == null)
        {
            subquery.ClearOrdering();
        }

        subquery.ReplaceProjection(new List<Expression> { projection });
        subquery.ApplyProjection();

        var translation = _sqlExpressionFactory.In(translatedItem, subquery);
        subquery = _sqlExpressionFactory.Select(translation);

        return source.Update(
            subquery,
            Expression.Convert(
                new ProjectionBindingExpression(subquery, new ProjectionMember(), typeof(bool?)), typeof(bool)));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateCount(ShapedQueryExpression source, LambdaExpression? predicate)
        => TranslateAggregateWithPredicate(source, predicate, QueryableMethods.CountWithoutPredicate, liftOrderings: false);

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateDefaultIfEmpty(ShapedQueryExpression source, Expression? defaultValue)
    {
        if (defaultValue == null)
        {
            ((SelectExpression)source.QueryExpression).ApplyDefaultIfEmpty(_sqlExpressionFactory);
            return source.UpdateShaperExpression(MarkShaperNullable(source.ShaperExpression));
        }

        return null;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression TranslateDistinct(ShapedQueryExpression source)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;

        if (selectExpression is { Orderings.Count: > 0, Limit: null, Offset: null }
            && !IsNaturallyOrdered(selectExpression))
        {
            _queryCompilationContext.Logger.DistinctAfterOrderByWithoutRowLimitingOperatorWarning();
        }

        selectExpression.ApplyDistinct();
        return source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateElementAtOrDefault(
        ShapedQueryExpression source,
        Expression index,
        bool returnDefault)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        var translation = TranslateExpression(index);
        if (translation == null)
        {
            return null;
        }

        if (!IsOrdered(selectExpression))
        {
            _queryCompilationContext.Logger.RowLimitingOperationWithoutOrderByWarning();
        }

        selectExpression.ApplyOffset(translation);
        selectExpression.ApplyLimit(TranslateExpression(Expression.Constant(1))!);

        return source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression TranslateExcept(ShapedQueryExpression source1, ShapedQueryExpression source2)
    {
        ((SelectExpression)source1.QueryExpression).ApplyExcept((SelectExpression)source2.QueryExpression, distinct: true);

        // Since except has result from source1, we don't need to change shaper
        return source1;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateFirstOrDefault(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        Type returnType,
        bool returnDefault)
    {
        if (predicate != null)
        {
            var translatedSource = TranslateWhere(source, predicate);
            if (translatedSource == null)
            {
                return null;
            }

            source = translatedSource;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression.Predicate == null
            && selectExpression.Orderings.Count == 0)
        {
            _queryCompilationContext.Logger.FirstWithoutOrderByAndFilterWarning();
        }

        selectExpression.ApplyLimit(TranslateExpression(Expression.Constant(1))!);

        return source.ShaperExpression.Type != returnType
            ? source.UpdateShaperExpression(Expression.Convert(source.ShaperExpression, returnType))
            : source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateGroupBy(
        ShapedQueryExpression source,
        LambdaExpression keySelector,
        LambdaExpression? elementSelector,
        LambdaExpression? resultSelector)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        selectExpression.PrepareForAggregate();

        var remappedKeySelector = RemapLambdaBody(source, keySelector);
        var translatedKey = TranslateGroupingKey(remappedKeySelector);
        if (translatedKey == null)
        {
            // This could be group by entity type
            if (remappedKeySelector is not EntityShaperExpression
                {
                    ValueBufferExpression: ProjectionBindingExpression pbe
                } ese)
            {
                // ValueBufferExpression can be JsonQuery, ProjectionBindingExpression, EntityProjection
                // We only allow ProjectionBindingExpression which represents a regular entity
                return null;
            }

            translatedKey = ese.Update(((SelectExpression)pbe.QueryExpression).GetProjection(pbe));
        }

        if (elementSelector != null)
        {
            source = TranslateSelect(source, elementSelector);
        }

        var groupByShaper = selectExpression.ApplyGrouping(translatedKey, source.ShaperExpression, _sqlExpressionFactory);
        if (resultSelector == null)
        {
            return source.UpdateShaperExpression(groupByShaper);
        }

        var original1 = resultSelector.Parameters[0];
        var original2 = resultSelector.Parameters[1];

        var newResultSelectorBody = new ReplacingExpressionVisitor(
                new Expression[] { original1, original2 },
                new[] { groupByShaper.KeySelector, groupByShaper })
            .Visit(resultSelector.Body);

        newResultSelectorBody = ExpandSharedTypeEntities(selectExpression, newResultSelectorBody);

        return source.UpdateShaperExpression(
            _projectionBindingExpressionVisitor.Translate(selectExpression, newResultSelectorBody));
    }

    private Expression? TranslateGroupingKey(Expression expression)
    {
        switch (expression)
        {
            case NewExpression newExpression:
                if (newExpression.Arguments.Count == 0)
                {
                    return newExpression;
                }

                var newArguments = new Expression[newExpression.Arguments.Count];
                for (var i = 0; i < newArguments.Length; i++)
                {
                    var key = TranslateGroupingKey(newExpression.Arguments[i]);
                    if (key == null)
                    {
                        return null;
                    }

                    newArguments[i] = key;
                }

                return newExpression.Update(newArguments);

            case MemberInitExpression memberInitExpression:
                var updatedNewExpression = (NewExpression?)TranslateGroupingKey(memberInitExpression.NewExpression);
                if (updatedNewExpression == null)
                {
                    return null;
                }

                var newBindings = new MemberAssignment[memberInitExpression.Bindings.Count];
                for (var i = 0; i < newBindings.Length; i++)
                {
                    var memberAssignment = (MemberAssignment)memberInitExpression.Bindings[i];
                    var visitedExpression = TranslateGroupingKey(memberAssignment.Expression);
                    if (visitedExpression == null)
                    {
                        return null;
                    }

                    newBindings[i] = memberAssignment.Update(visitedExpression);
                }

                return memberInitExpression.Update(updatedNewExpression, newBindings);

            default:
                var translation = TranslateExpression(expression);
                if (translation == null)
                {
                    return null;
                }

                return translation.Type == expression.Type
                    ? translation
                    : Expression.Convert(translation, expression.Type);
        }
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateGroupJoin(
        ShapedQueryExpression outer,
        ShapedQueryExpression inner,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector)
        => null;

    /// <inheritdoc />
    protected override ShapedQueryExpression TranslateIntersect(ShapedQueryExpression source1, ShapedQueryExpression source2)
    {
        ((SelectExpression)source1.QueryExpression).ApplyIntersect((SelectExpression)source2.QueryExpression, distinct: true);

        // For intersect since result comes from both sides, if one of them is non-nullable then both are non-nullable
        return source1.UpdateShaperExpression(
            MatchShaperNullabilityForSetOperation(source1.ShaperExpression, source2.ShaperExpression, makeNullable: false));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateJoin(
        ShapedQueryExpression outer,
        ShapedQueryExpression inner,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector)
    {
        var joinPredicate = CreateJoinPredicate(outer, outerKeySelector, inner, innerKeySelector);
        if (joinPredicate != null)
        {
            var outerSelectExpression = (SelectExpression)outer.QueryExpression;
            var outerShaperExpression = outerSelectExpression.AddInnerJoin(inner, joinPredicate, outer.ShaperExpression);
            outer = outer.UpdateShaperExpression(outerShaperExpression);

            return TranslateTwoParameterSelector(outer, resultSelector);
        }

        return null;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateLeftJoin(
        ShapedQueryExpression outer,
        ShapedQueryExpression inner,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector)
    {
        var joinPredicate = CreateJoinPredicate(outer, outerKeySelector, inner, innerKeySelector);
        if (joinPredicate != null)
        {
            var outerSelectExpression = (SelectExpression)outer.QueryExpression;
            var outerShaperExpression = outerSelectExpression.AddLeftJoin(inner, joinPredicate, outer.ShaperExpression);
            outer = outer.UpdateShaperExpression(outerShaperExpression);

            return TranslateTwoParameterSelector(outer, resultSelector);
        }

        return null;
    }

    private SqlExpression CreateJoinPredicate(
        ShapedQueryExpression outer,
        LambdaExpression outerKeySelector,
        ShapedQueryExpression inner,
        LambdaExpression innerKeySelector)
    {
        var outerKey = RemapLambdaBody(outer, outerKeySelector);
        var innerKey = RemapLambdaBody(inner, innerKeySelector);

        if (outerKey is NewExpression { Arguments.Count: > 0 } outerNew)
        {
            var innerNew = (NewExpression)innerKey;

            SqlExpression? result = null;
            for (var i = 0; i < outerNew.Arguments.Count; i++)
            {
                var joinPredicate = CreateJoinPredicate(outerNew.Arguments[i], innerNew.Arguments[i]);
                result = result == null
                    ? joinPredicate
                    : _sqlExpressionFactory.AndAlso(result, joinPredicate);
            }

            if (outerNew.Arguments.Count == 1)
            {
                result = _sqlExpressionFactory.AndAlso(
                    result!,
                    CreateJoinPredicate(Expression.Constant(true), Expression.Constant(true)));
            }

            return result!;
        }

        return CreateJoinPredicate(outerKey, innerKey);
    }

    private SqlExpression CreateJoinPredicate(Expression outerKey, Expression innerKey)
        => TranslateExpression(Infrastructure.ExpressionExtensions.CreateEqualsExpression(outerKey, innerKey))!;

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateLastOrDefault(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        Type returnType,
        bool returnDefault)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression.Orderings.Count == 0)
        {
            throw new InvalidOperationException(
                RelationalStrings.LastUsedWithoutOrderBy(returnDefault ? nameof(Queryable.LastOrDefault) : nameof(Queryable.Last)));
        }

        if (predicate != null)
        {
            var translatedSource = TranslateWhere(source, predicate);
            if (translatedSource == null)
            {
                return null;
            }

            source = translatedSource;
        }

        selectExpression.ReverseOrderings();
        selectExpression.ApplyLimit(TranslateExpression(Expression.Constant(1))!);

        return source.ShaperExpression.Type != returnType
            ? source.UpdateShaperExpression(Expression.Convert(source.ShaperExpression, returnType))
            : source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateLongCount(ShapedQueryExpression source, LambdaExpression? predicate)
        => TranslateAggregateWithPredicate(source, predicate, QueryableMethods.LongCountWithoutPredicate, liftOrderings: false);

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateMax(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
        => TranslateAggregateWithSelector(
            source, selector, t => QueryableMethods.MaxWithoutSelector.MakeGenericMethod(t), throwWhenEmpty: true, resultType);

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateMin(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
        => TranslateAggregateWithSelector(
            source, selector, t => QueryableMethods.MinWithoutSelector.MakeGenericMethod(t), throwWhenEmpty: true, resultType);

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateOfType(ShapedQueryExpression source, Type resultType)
    {
        if (source.ShaperExpression is EntityShaperExpression entityShaperExpression)
        {
            var entityType = entityShaperExpression.EntityType;
            if (entityType.ClrType == resultType)
            {
                return source;
            }

            var parameterExpression = Expression.Parameter(entityShaperExpression.Type);
            var predicate = Expression.Lambda(Expression.TypeIs(parameterExpression, resultType), parameterExpression);
            var translation = TranslateLambdaExpression(source, predicate);
            if (translation == null)
            {
                // EntityType is not part of hierarchy
                return null;
            }

            var selectExpression = (SelectExpression)source.QueryExpression;
            if (translation is not SqlConstantExpression { Value: true })
            {
                selectExpression.ApplyPredicate(translation);
            }

            var baseType = entityType.GetAllBaseTypes().SingleOrDefault(et => et.ClrType == resultType);
            if (baseType != null)
            {
                return source.UpdateShaperExpression(entityShaperExpression.WithEntityType(baseType));
            }

            var derivedType = entityType.GetDerivedTypes().Single(et => et.ClrType == resultType);
            var projectionBindingExpression = (ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression;

            var projectionMember = projectionBindingExpression.ProjectionMember;
            Check.DebugAssert(new ProjectionMember().Equals(projectionMember), "Invalid ProjectionMember when processing OfType");

            var entityProjectionExpression = (EntityProjectionExpression)selectExpression.GetProjection(projectionBindingExpression);
            selectExpression.ReplaceProjection(
                new Dictionary<ProjectionMember, Expression>
                {
                    { projectionMember, entityProjectionExpression.UpdateEntityType(derivedType) }
                });

            return source.UpdateShaperExpression(entityShaperExpression.WithEntityType(derivedType));
        }

        return null;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateOrderBy(
        ShapedQueryExpression source,
        LambdaExpression keySelector,
        bool ascending)
    {
        var translation = TranslateLambdaExpression(source, keySelector);
        if (translation == null)
        {
            return null;
        }

        ((SelectExpression)source.QueryExpression).ApplyOrdering(new OrderingExpression(translation, ascending));

        return source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateReverse(ShapedQueryExpression source)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression.Orderings.Count == 0)
        {
            AddTranslationErrorDetails(RelationalStrings.MissingOrderingInSelectExpression);
            return null;
        }

        selectExpression.ReverseOrderings();

        return source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression TranslateSelect(ShapedQueryExpression source, LambdaExpression selector)
    {
        if (selector.Body == selector.Parameters[0])
        {
            return source;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression.IsDistinct)
        {
            selectExpression.PushdownIntoSubquery();
        }

        var newSelectorBody = RemapLambdaBody(source, selector);

        return source.UpdateShaperExpression(_projectionBindingExpressionVisitor.Translate(selectExpression, newSelectorBody));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSelectMany(
        ShapedQueryExpression source,
        LambdaExpression collectionSelector,
        LambdaExpression resultSelector)
    {
        var (newCollectionSelector, correlated, defaultIfEmpty)
            = new CorrelationFindingExpressionVisitor().IsCorrelated(collectionSelector);
        if (correlated)
        {
            var collectionSelectorBody = RemapLambdaBody(source, newCollectionSelector);
            if (Visit(collectionSelectorBody) is ShapedQueryExpression inner)
            {
                var innerSelectExpression = (SelectExpression)source.QueryExpression;
                var shaper = defaultIfEmpty
                    ? innerSelectExpression.AddOuterApply(inner, source.ShaperExpression)
                    : innerSelectExpression.AddCrossApply(inner, source.ShaperExpression);

                return TranslateTwoParameterSelector(source.UpdateShaperExpression(shaper), resultSelector);
            }
        }
        else
        {
            if (Visit(newCollectionSelector.Body) is ShapedQueryExpression inner)
            {
                if (defaultIfEmpty)
                {
                    var translatedInner = TranslateDefaultIfEmpty(inner, null);
                    if (translatedInner == null)
                    {
                        return null;
                    }

                    inner = translatedInner;
                }

                var innerSelectExpression = (SelectExpression)source.QueryExpression;
                var shaper = innerSelectExpression.AddCrossJoin(inner, source.ShaperExpression);

                return TranslateTwoParameterSelector(source.UpdateShaperExpression(shaper), resultSelector);
            }
        }

        return null;
    }

    private sealed class CorrelationFindingExpressionVisitor : ExpressionVisitor
    {
        private ParameterExpression? _outerParameter;
        private bool _correlated;
        private bool _defaultIfEmpty;

        public (LambdaExpression, bool, bool) IsCorrelated(LambdaExpression lambdaExpression)
        {
            Check.DebugAssert(
                lambdaExpression.Parameters.Count == 1, "Multi-parameter lambda passed to CorrelationFindingExpressionVisitor");

            _correlated = false;
            _defaultIfEmpty = false;
            _outerParameter = lambdaExpression.Parameters[0];

            var result = Visit(lambdaExpression.Body);

            return (Expression.Lambda(result, _outerParameter), _correlated, _defaultIfEmpty);
        }

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            if (parameterExpression == _outerParameter)
            {
                _correlated = true;
            }

            return base.VisitParameter(parameterExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.DefaultIfEmptyWithoutArgument)
            {
                _defaultIfEmpty = true;
                return Visit(methodCallExpression.Arguments[0]);
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSelectMany(ShapedQueryExpression source, LambdaExpression selector)
    {
        var innerParameter = Expression.Parameter(selector.ReturnType.GetSequenceType(), "i");
        var resultSelector = Expression.Lambda(
            innerParameter, Expression.Parameter(source.Type.GetSequenceType()), innerParameter);

        return TranslateSelectMany(source, selector, resultSelector);
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSingleOrDefault(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        Type returnType,
        bool returnDefault)
    {
        if (predicate != null)
        {
            var translatedSource = TranslateWhere(source, predicate);
            if (translatedSource == null)
            {
                return null;
            }

            source = translatedSource;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        selectExpression.ApplyLimit(TranslateExpression(Expression.Constant(_subquery ? 1 : 2))!);

        return source.ShaperExpression.Type != returnType
            ? source.UpdateShaperExpression(Expression.Convert(source.ShaperExpression, returnType))
            : source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSkip(ShapedQueryExpression source, Expression count)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        var translation = TranslateExpression(count);
        if (translation == null)
        {
            return null;
        }

        if (!IsOrdered(selectExpression))
        {
            _queryCompilationContext.Logger.RowLimitingOperationWithoutOrderByWarning();
        }

        selectExpression.ApplyOffset(translation);

        return source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSkipWhile(ShapedQueryExpression source, LambdaExpression predicate)
        => null;

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSum(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
        => TranslateAggregateWithSelector(source, selector, QueryableMethods.GetSumWithoutSelector, throwWhenEmpty: false, resultType);

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateTake(ShapedQueryExpression source, Expression count)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        var translation = TranslateExpression(count);
        if (translation == null)
        {
            return null;
        }

        if (!IsOrdered(selectExpression))
        {
            _queryCompilationContext.Logger.RowLimitingOperationWithoutOrderByWarning();
        }

        selectExpression.ApplyLimit(translation);

        return source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateTakeWhile(ShapedQueryExpression source, LambdaExpression predicate)
        => null;

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateThenBy(
        ShapedQueryExpression source,
        LambdaExpression keySelector,
        bool ascending)
    {
        var translation = TranslateLambdaExpression(source, keySelector);
        if (translation == null)
        {
            return null;
        }

        ((SelectExpression)source.QueryExpression).AppendOrdering(new OrderingExpression(translation, ascending));

        return source;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression TranslateUnion(ShapedQueryExpression source1, ShapedQueryExpression source2)
    {
        ((SelectExpression)source1.QueryExpression).ApplyUnion((SelectExpression)source2.QueryExpression, distinct: true);

        return source1.UpdateShaperExpression(
            MatchShaperNullabilityForSetOperation(source1.ShaperExpression, source2.ShaperExpression, makeNullable: true));
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateWhere(ShapedQueryExpression source, LambdaExpression predicate)
    {
        var translation = TranslateLambdaExpression(source, predicate);
        if (translation == null)
        {
            return null;
        }

        ((SelectExpression)source.QueryExpression).ApplyPredicate(translation);

        return source;
    }

    /// <summary>
    ///     Translates <see cref="RelationalQueryableExtensions.ExecuteDelete{TSource}(IQueryable{TSource})" /> method
    ///     over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <returns>The non query after translation.</returns>
    protected virtual NonQueryExpression? TranslateExecuteDelete(ShapedQueryExpression source)
    {
        if (source.ShaperExpression is IncludeExpression includeExpression)
        {
            source = source.UpdateShaperExpression(PruneIncludes(includeExpression));
        }

        if (source.ShaperExpression is not EntityShaperExpression entityShaperExpression)
        {
            AddTranslationErrorDetails(RelationalStrings.ExecuteDeleteOnNonEntityType);
            return null;
        }

        var entityType = entityShaperExpression.EntityType;
        var mappingStrategy = entityType.GetMappingStrategy();
        if (mappingStrategy == RelationalAnnotationNames.TptMappingStrategy)
        {
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnTPT(nameof(RelationalQueryableExtensions.ExecuteDelete), entityType.DisplayName()));
            return null;
        }

        if (mappingStrategy == RelationalAnnotationNames.TpcMappingStrategy
            && entityType.GetDirectlyDerivedTypes().Any())
        {
            // We allow TPC is it is leaf type
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnTPC(nameof(RelationalQueryableExtensions.ExecuteDelete), entityType.DisplayName()));
            return null;
        }

        if (entityType.GetViewOrTableMappings().Count() != 1)
        {
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnEntitySplitting(
                    nameof(RelationalQueryableExtensions.ExecuteDelete), entityType.DisplayName()));
            return null;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        if (IsValidSelectExpressionForExecuteDelete(selectExpression, entityShaperExpression, out var tableExpression))
        {
            if (AreOtherNonOwnedEntityTypesInTheTable(entityType.GetRootType(), tableExpression.Table))
            {
                AddTranslationErrorDetails(
                    RelationalStrings.ExecuteDeleteOnTableSplitting(tableExpression.Table.SchemaQualifiedName));

                return null;
            }

            selectExpression.ReplaceProjection(new List<Expression>());
            selectExpression.ApplyProjection();

            return new NonQueryExpression(new DeleteExpression(tableExpression, selectExpression));

            static bool AreOtherNonOwnedEntityTypesInTheTable(IEntityType rootType, ITableBase table)
            {
                foreach (var entityTypeMapping in table.EntityTypeMappings)
                {
                    var typeBase = entityTypeMapping.TypeBase;
                    if ((entityTypeMapping.IsSharedTablePrincipal == true
                            && typeBase != rootType)
                        || (entityTypeMapping.IsSharedTablePrincipal == false
                            && typeBase is IEntityType entityType
                            && entityType.GetRootType() != rootType
                            && !entityType.IsOwned()))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        // We need to convert to PK predicate
        var pk = entityType.FindPrimaryKey();
        if (pk == null)
        {
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnKeylessEntityTypeWithUnsupportedOperator(
                    nameof(RelationalQueryableExtensions.ExecuteDelete),
                    entityType.DisplayName()));
            return null;
        }

        var clrType = entityType.ClrType;
        var entityParameter = Expression.Parameter(clrType);
        var innerParameter = Expression.Parameter(clrType);
        var predicateBody = Expression.Call(
            QueryableMethods.AnyWithPredicate.MakeGenericMethod(clrType),
            source,
            Expression.Quote(
                Expression.Lambda(
                    Infrastructure.ExpressionExtensions.CreateEqualsExpression(innerParameter, entityParameter),
                    innerParameter)));

        var newSource = Expression.Call(
            QueryableMethods.Where.MakeGenericMethod(clrType),
            new EntityQueryRootExpression(entityType),
            Expression.Quote(Expression.Lambda(predicateBody, entityParameter)));

        return TranslateExecuteDelete((ShapedQueryExpression)Visit(newSource));
    }

    /// <summary>
    ///     Translates
    ///     <see
    ///         cref="RelationalQueryableExtensions.ExecuteUpdate{TSource}(IQueryable{TSource}, Expression{Func{SetPropertyCalls{TSource}, SetPropertyCalls{TSource}}})" />
    ///     method
    ///     over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="setPropertyCalls">
    ///     The lambda expression containing
    ///     <see
    ///         cref="SetPropertyCalls{TSource}.SetProperty{TProperty}(Func{TSource, TProperty}, Func{TSource, TProperty})" />
    ///     statements.
    /// </param>
    /// <returns>The non query after translation.</returns>
    protected virtual NonQueryExpression? TranslateExecuteUpdate(
        ShapedQueryExpression source,
        LambdaExpression setPropertyCalls)
    {
        // Our source may have IncludeExpressions because of owned entities or auto-include; unwrap these, as they're meaningless for
        // ExecuteUpdate's lambdas. Note that we don't currently support updates across tables.
        if (source.ShaperExpression is IncludeExpression includeExpression)
        {
            source = source.UpdateShaperExpression(PruneIncludes(includeExpression));
        }

        var propertyValueLambdaExpressions = new List<(LambdaExpression, Expression)>();
        PopulateSetPropertyCalls(setPropertyCalls.Body, propertyValueLambdaExpressions, setPropertyCalls.Parameters[0]);
        if (TranslationErrorDetails != null)
        {
            return null;
        }

        if (propertyValueLambdaExpressions.Count == 0)
        {
            AddTranslationErrorDetails(RelationalStrings.NoSetPropertyInvocation);
            return null;
        }

        EntityShaperExpression? entityShaperExpression = null;
        var remappedUnwrappedLeftExpressions = new List<Expression>();
        foreach (var (propertyExpression, _) in propertyValueLambdaExpressions)
        {
            var left = RemapLambdaBody(source, propertyExpression);

            if (!TryProcessPropertyAccess(RelationalDependencies.Model, ref left, out var ese))
            {
                AddTranslationErrorDetails(RelationalStrings.InvalidPropertyInSetProperty(propertyExpression.Print()));
                return null;
            }

            if (entityShaperExpression is null)
            {
                entityShaperExpression = ese;
            }
            else if (!ReferenceEquals(ese, entityShaperExpression))
            {
                AddTranslationErrorDetails(
                    RelationalStrings.MultipleEntityPropertiesInSetProperty(
                        entityShaperExpression.EntityType.DisplayName(), ese.EntityType.DisplayName()));
                return null;
            }

            remappedUnwrappedLeftExpressions.Add(left);
        }

        Check.DebugAssert(entityShaperExpression != null, "EntityShaperExpression should have a value.");

        var entityType = entityShaperExpression.EntityType;
        var mappingStrategy = entityType.GetMappingStrategy();
        if (mappingStrategy == RelationalAnnotationNames.TptMappingStrategy)
        {
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnTPT(nameof(RelationalQueryableExtensions.ExecuteUpdate), entityType.DisplayName()));
            return null;
        }

        if (mappingStrategy == RelationalAnnotationNames.TpcMappingStrategy
            && entityType.GetDirectlyDerivedTypes().Any())
        {
            // We allow TPC is it is leaf type
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnTPC(nameof(RelationalQueryableExtensions.ExecuteUpdate), entityType.DisplayName()));
            return null;
        }

        if (entityType.GetViewOrTableMappings().Count() != 1)
        {
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnEntitySplitting(
                    nameof(RelationalQueryableExtensions.ExecuteUpdate), entityType.DisplayName()));
            return null;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        if (IsValidSelectExpressionForExecuteUpdate(selectExpression, entityShaperExpression, out var tableExpression))
        {
            return TranslateSetPropertyExpressions(
                this, source, selectExpression, tableExpression,
                propertyValueLambdaExpressions, remappedUnwrappedLeftExpressions);
        }

        // We need to convert to join with original query using PK
        var pk = entityType.FindPrimaryKey();
        if (pk == null)
        {
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnKeylessEntityTypeWithUnsupportedOperator(
                    nameof(RelationalQueryableExtensions.ExecuteUpdate),
                    entityType.DisplayName()));
            return null;
        }

        var outer = (ShapedQueryExpression)Visit(new EntityQueryRootExpression(entityType));
        var inner = source;
        var outerParameter = Expression.Parameter(entityType.ClrType);
        var outerKeySelector = Expression.Lambda(outerParameter.CreateKeyValuesExpression(pk.Properties), outerParameter);
        var firstPropertyLambdaExpression = propertyValueLambdaExpressions[0].Item1;
        var entitySource = GetEntitySource(RelationalDependencies.Model, firstPropertyLambdaExpression.Body);
        var innerKeySelector = Expression.Lambda(
            entitySource.CreateKeyValuesExpression(pk.Properties), firstPropertyLambdaExpression.Parameters);

        var joinPredicate = CreateJoinPredicate(outer, outerKeySelector, inner, innerKeySelector);

        Check.DebugAssert(joinPredicate != null, "Join predicate shouldn't be null");

        var outerSelectExpression = (SelectExpression)outer.QueryExpression;
        var outerShaperExpression = outerSelectExpression.AddInnerJoin(inner, joinPredicate, outer.ShaperExpression);
        outer = outer.UpdateShaperExpression(outerShaperExpression);
        var transparentIdentifierType = outer.ShaperExpression.Type;
        var transparentIdentifierParameter = Expression.Parameter(transparentIdentifierType);

        var propertyReplacement = AccessField(transparentIdentifierType, transparentIdentifierParameter, "Outer");
        var valueReplacement = AccessField(transparentIdentifierType, transparentIdentifierParameter, "Inner");
        for (var i = 0; i < propertyValueLambdaExpressions.Count; i++)
        {
            var (propertyExpression, valueExpression) = propertyValueLambdaExpressions[i];
            propertyExpression = Expression.Lambda(
                ReplacingExpressionVisitor.Replace(
                    ReplacingExpressionVisitor.Replace(
                        firstPropertyLambdaExpression.Parameters[0],
                        propertyExpression.Parameters[0],
                        entitySource),
                    propertyReplacement, propertyExpression.Body),
                transparentIdentifierParameter);

            valueExpression = valueExpression is LambdaExpression lambdaExpression
                ? Expression.Lambda(
                    ReplacingExpressionVisitor.Replace(lambdaExpression.Parameters[0], valueReplacement, lambdaExpression.Body),
                    transparentIdentifierParameter)
                : valueExpression;

            propertyValueLambdaExpressions[i] = (propertyExpression, valueExpression);
        }

        tableExpression = (TableExpression)outerSelectExpression.Tables[0];

        return TranslateSetPropertyExpressions(this, outer, outerSelectExpression, tableExpression, propertyValueLambdaExpressions, null);

        static NonQueryExpression? TranslateSetPropertyExpressions(
            RelationalQueryableMethodTranslatingExpressionVisitor visitor,
            ShapedQueryExpression source,
            SelectExpression selectExpression,
            TableExpression tableExpression,
            List<(LambdaExpression, Expression)> propertyValueLambdaExpressions,
            List<Expression>? leftExpressions)
        {
            var columnValueSetters = new List<ColumnValueSetter>();
            for (var i = 0; i < propertyValueLambdaExpressions.Count; i++)
            {
                var (propertyExpression, valueExpression) = propertyValueLambdaExpressions[i];
                Expression left;
                if (leftExpressions != null)
                {
                    left = leftExpressions[i];
                }
                else
                {
                    left = visitor.RemapLambdaBody(source, propertyExpression);
                    left = left.UnwrapTypeConversion(out _);
                }

                var right = valueExpression is LambdaExpression lambdaExpression
                    ? visitor.RemapLambdaBody(source, lambdaExpression)
                    : valueExpression;

                if (right.Type != left.Type)
                {
                    right = Expression.Convert(right, left.Type);
                }

                // We generate equality between property = value while translating so that we infer the type mapping from property correctly.
                // Later we decompose it back into left/right components so that the equality is not in the tree which can get affected by
                // null semantics or other visitor.
                var setter = Infrastructure.ExpressionExtensions.CreateEqualsExpression(left, right);
                var translation = visitor._sqlTranslator.Translate(setter);
                if (translation is SqlBinaryExpression
                    {
                        OperatorType: ExpressionType.Equal, Left: ColumnExpression column
                    } sqlBinaryExpression)
                {
                    columnValueSetters.Add(
                        new ColumnValueSetter(
                            column,
                            selectExpression.AssignUniqueAliases(sqlBinaryExpression.Right)));
                }
                else
                {
                    // We would reach here only if the property is unmapped or value fails to translate.
                    visitor.AddTranslationErrorDetails(
                        RelationalStrings.UnableToTranslateSetProperty(
                            propertyExpression.Print(), valueExpression.Print(), visitor._sqlTranslator.TranslationErrorDetails));
                    return null;
                }
            }

            selectExpression.ReplaceProjection(new List<Expression>());
            selectExpression.ApplyProjection();

            return new NonQueryExpression(new UpdateExpression(tableExpression, selectExpression, columnValueSetters));
        }

        void PopulateSetPropertyCalls(
            Expression expression,
            List<(LambdaExpression, Expression)> list,
            ParameterExpression parameter)
        {
            switch (expression)
            {
                case ParameterExpression p
                    when parameter == p:
                    break;

                case MethodCallExpression
                    {
                        Method:
                        {
                            IsGenericMethod: true,
                            Name: nameof(SetPropertyCalls<int>.SetProperty),
                            DeclaringType.IsGenericType: true
                        }
                    } methodCallExpression
                    when methodCallExpression.Method.DeclaringType.GetGenericTypeDefinition() == typeof(SetPropertyCalls<>):
                    list.Add(((LambdaExpression)methodCallExpression.Arguments[0], methodCallExpression.Arguments[1]));

                    PopulateSetPropertyCalls(methodCallExpression.Object!, list, parameter);

                    break;

                default:
                    AddTranslationErrorDetails(RelationalStrings.InvalidArgumentToExecuteUpdate);
                    break;
            }
        }

        // For property setter selectors in ExecuteUpdate, we support only simple member access, EF.Function, etc.
        // We also unwrap casts to interface/base class (#29618). Note that owned IncludeExpressions have already been pruned from the
        // source before remapping the lambda (#28727).
        static bool TryProcessPropertyAccess(
            IModel model,
            ref Expression expression,
            [NotNullWhen(true)] out EntityShaperExpression? entityShaperExpression)
        {
            expression = expression.UnwrapTypeConversion(out _);

            if (expression is MemberExpression { Expression : not null } memberExpression
                && memberExpression.Expression.UnwrapTypeConversion(out _) is EntityShaperExpression ese)
            {
                expression = memberExpression.Update(ese);
                entityShaperExpression = ese;
                return true;
            }

            if (expression is MethodCallExpression mce)
            {
                if (mce.TryGetEFPropertyArguments(out var source, out _)
                    && source.UnwrapTypeConversion(out _) is EntityShaperExpression ese1)
                {
                    if (source != ese1)
                    {
                        var rewrittenArguments = mce.Arguments.ToArray();
                        rewrittenArguments[0] = ese1;
                        expression = mce.Update(mce.Object, rewrittenArguments);
                    }

                    entityShaperExpression = ese1;
                    return true;
                }

                if (mce.TryGetIndexerArguments(model, out var source2, out _)
                    && source2.UnwrapTypeConversion(out _) is EntityShaperExpression ese2)
                {
                    expression = mce.Update(ese2, mce.Arguments);
                    entityShaperExpression = ese2;
                    return true;
                }
            }

            entityShaperExpression = null;
            return false;
        }

        static Expression GetEntitySource(IModel model, Expression propertyAccessExpression)
        {
            propertyAccessExpression = propertyAccessExpression.UnwrapTypeConversion(out _);
            if (propertyAccessExpression is MethodCallExpression mce)
            {
                if (mce.TryGetEFPropertyArguments(out var source, out _))
                {
                    return source;
                }

                if (mce.TryGetIndexerArguments(model, out var source2, out _))
                {
                    return source2;
                }
            }

            return ((MemberExpression)propertyAccessExpression).Expression!;
        }
    }

    /// <summary>
    ///     Checks weather the current select expression can be used as-is for execute a delete operation,
    ///     or whether it must be pushed down into a subquery.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, only single-table select expressions are supported, and optionally with a predicate.
    ///     </para>
    ///     <para>
    ///         Providers can override this to allow more select expression features to be supported without pushing down into a subquery.
    ///         When doing this, VisitDelete must also be overridden in the provider's QuerySqlGenerator to add SQL generation support for
    ///         the feature.
    ///     </para>
    /// </remarks>
    /// <param name="selectExpression">The select expression to validate.</param>
    /// <param name="entityShaperExpression">The entity shaper expression on which the delete operation is being applied.</param>
    /// <param name="tableExpression">The table expression from which rows are being deleted.</param>
    /// <returns>Returns <see langword="true" /> if the current select expression can be used for delete as-is, <see langword="false" /> otherwise.</returns>
    protected virtual bool IsValidSelectExpressionForExecuteDelete(
        SelectExpression selectExpression,
        EntityShaperExpression entityShaperExpression,
        [NotNullWhen(true)] out TableExpression? tableExpression)
    {
        if (selectExpression is
            {
                Tables: [TableExpression expression],
                Orderings: [],
                Offset: null,
                Limit: null,
                GroupBy: [],
                Having: null
            }
            // If entity type has primary key then Distinct is no-op
            && (!selectExpression.IsDistinct || entityShaperExpression.EntityType.FindPrimaryKey() != null))
        {
            tableExpression = expression;

            return true;
        }

        tableExpression = null;
        return false;
    }

    /// <summary>
    ///     Validates if the current select expression can be used for execute update operation or it requires to be joined as a subquery.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, only multi-table select expressions are supported, and optionally with a predicate.
    ///     </para>
    ///     <para>
    ///         Providers can override this to allow more select expression features to be supported without pushing down into a subquery.
    ///         When doing this, VisitUpdate must also be overridden in the provider's QuerySqlGenerator to add SQL generation support for
    ///         the feature.
    ///     </para>
    /// </remarks>
    /// <param name="selectExpression">The select expression to validate.</param>
    /// <param name="entityShaperExpression">The entity shaper expression on which the update operation is being applied.</param>
    /// <param name="tableExpression">The table expression from which rows are being deleted.</param>
    /// <returns>Returns <see langword="true" /> if the current select expression can be used for update as-is, <see langword="false" /> otherwise.</returns>
    protected virtual bool IsValidSelectExpressionForExecuteUpdate(
        SelectExpression selectExpression,
        EntityShaperExpression entityShaperExpression,
        [NotNullWhen(true)] out TableExpression? tableExpression)
    {
        tableExpression = null;
        if (selectExpression.Offset == null
            && selectExpression.Limit == null
            // If entity type has primary key then Distinct is no-op
            && (!selectExpression.IsDistinct || entityShaperExpression.EntityType.FindPrimaryKey() != null)
            && selectExpression.GroupBy.Count == 0
            && selectExpression.Having == null
            && selectExpression.Orderings.Count == 0
            && selectExpression.Tables.Count > 0)
        {
            TableExpressionBase table;
            if (selectExpression.Tables.Count == 1)
            {
                table = selectExpression.Tables[0];
            }
            else
            {
                var projectionBindingExpression = (ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression;
                var entityProjectionExpression = (EntityProjectionExpression)selectExpression.GetProjection(projectionBindingExpression);
                var column = entityProjectionExpression.BindProperty(entityShaperExpression.EntityType.GetProperties().First());
                table = column.Table;
                if (ReferenceEquals(selectExpression.Tables[0], table))
                {
                    // If the table we are looking for it first table, then we need to verify if we can lift the next table in FROM clause
                    var secondTable = selectExpression.Tables[1];
                    if (secondTable is not InnerJoinExpression and not CrossJoinExpression)
                    {
                        return false;
                    }
                }

                if (table is JoinExpressionBase joinExpressionBase)
                {
                    table = joinExpressionBase.Table;
                }
            }

            if (table is TableExpression te)
            {
                tableExpression = te;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Translates the given expression into equivalent SQL representation.
    /// </summary>
    /// <param name="expression">An expression to translate.</param>
    /// <param name="applyDefaultTypeMapping">
    ///     Whether to apply the default type mapping on the top-most element if it has none. Defaults to <see langword="true" />.
    /// </param>
    /// <returns>A <see cref="SqlExpression" /> which is translation of given expression or <see langword="null" />.</returns>
    protected virtual SqlExpression? TranslateExpression(Expression expression, bool applyDefaultTypeMapping = true)
    {
        var translation = _sqlTranslator.Translate(expression, applyDefaultTypeMapping);

        if (translation is null)
        {
            if (_sqlTranslator.TranslationErrorDetails != null)
            {
                AddTranslationErrorDetails(_sqlTranslator.TranslationErrorDetails);
            }
        }

        return translation;
    }

    /// <summary>
    ///     Translates the given lambda expression for the <see cref="ShapedQueryExpression" /> source into equivalent SQL representation.
    /// </summary>
    /// <param name="shapedQueryExpression">A <see cref="ShapedQueryExpression" /> on which the lambda expression is being applied.</param>
    /// <param name="lambdaExpression">A <see cref="LambdaExpression" /> to translate into SQL.</param>
    /// <returns>A <see cref="SqlExpression" /> which is translation of given lambda expression or <see langword="null" />.</returns>
    protected virtual SqlExpression? TranslateLambdaExpression(
        ShapedQueryExpression shapedQueryExpression,
        LambdaExpression lambdaExpression)
        => TranslateExpression(RemapLambdaBody(shapedQueryExpression, lambdaExpression));

    /// <summary>
    ///     Invoked at the end of top-level translation, applies inferred type mappings for queryable constants/parameters and verifies that
    ///     all <see cref="SqlExpression" /> have a type mapping.
    /// </summary>
    /// <param name="expression">The query expression to process.</param>
    /// <param name="inferredTypeMappings">
    ///     Inferred type mappings for queryable constants/parameters collected during translation. These will be applied to the appropriate
    ///     nodes in the tree.
    /// </param>
    protected virtual Expression ApplyInferredTypeMappings(
        Expression expression,
        IReadOnlyDictionary<(TableExpressionBase, string), RelationalTypeMapping?> inferredTypeMappings)
        => new RelationalInferredTypeMappingApplier(
            RelationalDependencies.Model, _sqlExpressionFactory, inferredTypeMappings).Visit(expression);

    /// <summary>
    ///     Determines whether the given <see cref="SelectExpression" /> is ordered, typically because orderings have been added to it.
    /// </summary>
    /// <param name="selectExpression">The <see cref="SelectExpression" /> to check for ordering.</param>
    /// <returns>Whether <paramref name="selectExpression" /> is ordered.</returns>
    protected virtual bool IsOrdered(SelectExpression selectExpression)
        => selectExpression.Orderings.Count > 0;

    /// <summary>
    ///     Determines whether the given <see cref="SelectExpression" /> is naturally ordered, meaning that any ordering has been added
    ///     automatically by EF to preserve e.g. the natural ordering of a JSON array, and not because the original LINQ query contained
    ///     an explicit ordering.
    /// </summary>
    /// <param name="selectExpression">The <see cref="SelectExpression" /> to check for ordering.</param>
    /// <returns>Whether <paramref name="selectExpression"/> is ordered.</returns>
    protected virtual bool IsNaturallyOrdered(SelectExpression selectExpression)
        => false;

    private Expression RemapLambdaBody(ShapedQueryExpression shapedQueryExpression, LambdaExpression lambdaExpression)
    {
        var lambdaBody = ReplacingExpressionVisitor.Replace(
            lambdaExpression.Parameters.Single(), shapedQueryExpression.ShaperExpression, lambdaExpression.Body);

        return ExpandSharedTypeEntities((SelectExpression)shapedQueryExpression.QueryExpression, lambdaBody);
    }

    private Expression ExpandSharedTypeEntities(SelectExpression selectExpression, Expression lambdaBody)
        => _sharedTypeEntityExpandingExpressionVisitor.Expand(selectExpression, lambdaBody);

    private static Expression PruneIncludes(IncludeExpression includeExpression)
    {
        if (includeExpression.Navigation is ISkipNavigation or not INavigation)
        {
            return includeExpression;
        }

        return includeExpression.EntityExpression is IncludeExpression innerIncludeExpression
            ? PruneIncludes(innerIncludeExpression)
            : includeExpression.EntityExpression;
    }

    private sealed class SharedTypeEntityExpandingExpressionVisitor : ExpressionVisitor
    {
        private readonly RelationalSqlTranslatingExpressionVisitor _sqlTranslator;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        private SelectExpression _selectExpression;

        public SharedTypeEntityExpandingExpressionVisitor(
            RelationalSqlTranslatingExpressionVisitor sqlTranslator,
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlTranslator = sqlTranslator;
            _sqlExpressionFactory = sqlExpressionFactory;
            _selectExpression = null!;
        }

        public Expression Expand(SelectExpression selectExpression, Expression lambdaBody)
        {
            _selectExpression = selectExpression;

            return Visit(lambdaBody);
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var innerExpression = Visit(memberExpression.Expression);

            return TryExpand(innerExpression, MemberIdentity.Create(memberExpression.Member))
                ?? memberExpression.Update(innerExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var navigationName))
            {
                source = Visit(source);

                return TryExpand(source, MemberIdentity.Create(navigationName))
                    ?? TryBindPrimitiveCollection(source, navigationName)
                    ?? methodCallExpression.Update(null!, new[] { source, methodCallExpression.Arguments[1] });
            }

            if (methodCallExpression.Method.IsGenericMethod
                && (methodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.ElementAt
                    || methodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.ElementAtOrDefault))
            {
                source = methodCallExpression.Arguments[0];
                var selectMethodCallExpression = default(MethodCallExpression);

                if (source is MethodCallExpression { Method.IsGenericMethod: true } sourceMethodCall
                    && sourceMethodCall.Method.GetGenericMethodDefinition() == QueryableMethods.Select)
                {
                    selectMethodCallExpression = sourceMethodCall;
                    source = sourceMethodCall.Arguments[0];
                }

                var asQueryableMethodCallExpression = default(MethodCallExpression);
                if (source is MethodCallExpression { Method.IsGenericMethod: true } maybeAsQueryableMethodCall
                    && maybeAsQueryableMethodCall.Method.GetGenericMethodDefinition() == QueryableMethods.AsQueryable)
                {
                    asQueryableMethodCallExpression = maybeAsQueryableMethodCall;
                    source = maybeAsQueryableMethodCall.Arguments[0];
                }

                source = Visit(source);

                if (source is JsonQueryExpression jsonQueryExpression)
                {
                    var collectionIndexExpression = _sqlTranslator.Translate(methodCallExpression.Arguments[1]);
                    if (collectionIndexExpression == null)
                    {
                        // before we return from failed translation
                        // we need to bring back methods we may have trimmed above (AsQueryable/Select)
                        // we translate what we can (source) and rest is the original tree
                        // so that sql translation can fail later (as the tree will be in unexpected shape)
                        return PrepareFailedTranslationResult(
                            source,
                            asQueryableMethodCallExpression,
                            selectMethodCallExpression,
                            methodCallExpression);
                    }

                    var newJsonQuery = jsonQueryExpression.BindCollectionElement(collectionIndexExpression);

                    var entityShaper = new RelationalEntityShaperExpression(
                        jsonQueryExpression.EntityType,
                        newJsonQuery,
                        nullable: true);

                    if (selectMethodCallExpression == null)
                    {
                        return entityShaper;
                    }

                    var selectorLambda = selectMethodCallExpression.Arguments[1].UnwrapLambdaFromQuote();

                    // short circuit what we know is wrong without a closer look
                    if (selectorLambda.Body is NewExpression or MemberInitExpression)
                    {
                        return PrepareFailedTranslationResult(
                            source,
                            asQueryableMethodCallExpression,
                            selectMethodCallExpression,
                            methodCallExpression);
                    }

                    var replaced = ReplacingExpressionVisitor.Replace(selectorLambda.Parameters[0], entityShaper, selectorLambda.Body);
                    var result = Visit(replaced);

                    return IsValidSelectorForJsonArrayElementAccess(result, newJsonQuery)
                        ? result
                        : PrepareFailedTranslationResult(
                            source,
                            asQueryableMethodCallExpression,
                            selectMethodCallExpression,
                            methodCallExpression);
                }
            }

            return base.VisitMethodCall(methodCallExpression);

            static Expression PrepareFailedTranslationResult(
                Expression source,
                MethodCallExpression? asQueryable,
                MethodCallExpression? select,
                MethodCallExpression elementAt)
            {
                var result = source;
                if (asQueryable != null)
                {
                    result = asQueryable.Update(null, new[] { result });
                }

                if (select != null)
                {
                    result = select.Update(null, new[] { result, select.Arguments[1] });
                }

                return elementAt.Update(null, new[] { result, elementAt.Arguments[1] });
            }

            static bool IsValidSelectorForJsonArrayElementAccess(Expression expression, JsonQueryExpression baselineJsonQuery)
            {
                switch (expression)
                {
                    // JSON_QUERY($[0]).Property
                    case MemberExpression
                        {
                            Expression: RelationalEntityShaperExpression { ValueBufferExpression: JsonQueryExpression memberJqe }
                        }
                        when JsonQueryExpressionIsRootedIn(memberJqe, baselineJsonQuery):
                    {
                        return true;
                    }

                    // MCNE(JSON_QUERY($[0].Collection))
                    // MCNE(JSON_QUERY($[0].Collection).AsQueryable())
                    // MCNE(JSON_QUERY($[0].Collection).Select(xx => xx.Includes())
                    // MCNE(JSON_QUERY($[0].Collection).AsQueryable().Select(xx => xx.Includes())
                    case MaterializeCollectionNavigationExpression { Subquery: var subquery }:
                    {
                        if (subquery is MethodCallExpression { Method.IsGenericMethod: true } selectMethodCall
                            && selectMethodCall.Method.GetGenericMethodDefinition() == QueryableMethods.Select
                            && selectMethodCall.Arguments[1].UnwrapLambdaFromQuote() is LambdaExpression selectorLambda
                            && StripIncludes(selectorLambda.Body) == selectorLambda.Parameters[0])
                        {
                            subquery = selectMethodCall.Arguments[0];
                        }

                        if (subquery is MethodCallExpression { Method.IsGenericMethod: true } asQueryableMethodCall
                            && asQueryableMethodCall.Method.GetGenericMethodDefinition() == QueryableMethods.AsQueryable)
                        {
                            subquery = asQueryableMethodCall.Arguments[0];
                        }

                        if (subquery is JsonQueryExpression subqueryJqe
                            && JsonQueryExpressionIsRootedIn(subqueryJqe, baselineJsonQuery))
                        {
                            return true;
                        }

                        goto default;
                    }

                    default:
                        // JSON_QUERY($[0]).Includes()
                        // JSON_QUERY($[0].Reference).Includes()
                        // JSON_QUERY($[0])
                        // JSON_QUERY($[0].Reference)
                        expression = StripIncludes(expression);
                        return expression is RelationalEntityShaperExpression { ValueBufferExpression: JsonQueryExpression reseJqe }
                            && JsonQueryExpressionIsRootedIn(reseJqe, baselineJsonQuery);
                }
            }

            static bool JsonQueryExpressionIsRootedIn(JsonQueryExpression expressionToTest, JsonQueryExpression root)
                => expressionToTest.JsonColumn == root.JsonColumn
                    && expressionToTest.Path.Count >= root.Path.Count
                    && expressionToTest.Path.Take(root.Path.Count).SequenceEqual(root.Path);

            static Expression StripIncludes(Expression expression)
            {
                var current = expression;
                while (current is IncludeExpression includeExpression)
                {
                    current = includeExpression.EntityExpression;
                }

                return current;
            }
        }

        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression is EntityShaperExpression or ShapedQueryExpression or GroupByShaperExpression
                ? extensionExpression
                : base.VisitExtension(extensionExpression);

        private Expression? TryExpand(Expression? source, MemberIdentity member)
        {
            source = source.UnwrapTypeConversion(out var convertedType);
            if (source is not EntityShaperExpression entityShaperExpression)
            {
                return null;
            }

            var entityType = entityShaperExpression.EntityType;
            if (convertedType != null)
            {
                entityType = entityType.GetRootType().GetDerivedTypesInclusive()
                    .FirstOrDefault(et => et.ClrType == convertedType);

                if (entityType == null)
                {
                    return null;
                }
            }

            var navigation = member.MemberInfo != null
                ? entityType.FindNavigation(member.MemberInfo)
                : entityType.FindNavigation(member.Name!);

            if (navigation == null)
            {
                return null;
            }

            var targetEntityType = navigation.TargetEntityType;
            if (targetEntityType == null
                || !targetEntityType.IsOwned())
            {
                return null;
            }

            if (TryGetJsonQueryExpression(entityShaperExpression, out var jsonQueryExpression))
            {
                var newJsonQueryExpression = jsonQueryExpression.BindNavigation(navigation);

                return navigation.IsCollection
                    ? newJsonQueryExpression
                    : new RelationalEntityShaperExpression(
                        navigation.TargetEntityType,
                        newJsonQueryExpression,
                        nullable: entityShaperExpression.IsNullable || !navigation.ForeignKey.IsRequired);
            }

            var entityProjectionExpression = GetEntityProjectionExpression(entityShaperExpression);
            var foreignKey = navigation.ForeignKey;

            if (targetEntityType.IsMappedToJson())
            {
                var innerShaper = entityProjectionExpression.BindNavigation(navigation)!;

                return navigation.IsCollection
                    ? (JsonQueryExpression)innerShaper.ValueBufferExpression
                    : innerShaper;
            }

            if (navigation.IsCollection)
            {
                // just need any column - we use it only to extract the table it originated from
                var sourceColumn = entityProjectionExpression
                    .BindProperty(
                        navigation.IsOnDependent
                            ? foreignKey.Properties[0]
                            : foreignKey.PrincipalKey.Properties[0]);

                var sourceTable = FindRootTableExpressionForColumn(sourceColumn);
                var innerSelectExpression = _sqlExpressionFactory.Select(targetEntityType);
                innerSelectExpression = (SelectExpression)new AnnotationApplyingExpressionVisitor(sourceTable.GetAnnotations().ToList())
                    .Visit(innerSelectExpression);

                var innerShapedQuery = CreateShapedQueryExpression(targetEntityType, innerSelectExpression);

                var makeNullable = foreignKey.PrincipalKey.Properties
                    .Concat(foreignKey.Properties)
                    .Select(p => p.ClrType)
                    .Any(t => t.IsNullableType());

                var innerSequenceType = innerShapedQuery.Type.GetSequenceType();
                var correlationPredicateParameter = Expression.Parameter(innerSequenceType);

                var outerKey = entityShaperExpression.CreateKeyValuesExpression(
                    navigation.IsOnDependent
                        ? foreignKey.Properties
                        : foreignKey.PrincipalKey.Properties,
                    makeNullable);
                var innerKey = correlationPredicateParameter.CreateKeyValuesExpression(
                    navigation.IsOnDependent
                        ? foreignKey.PrincipalKey.Properties
                        : foreignKey.Properties,
                    makeNullable);

                var keyComparison = Infrastructure.ExpressionExtensions.CreateEqualsExpression(outerKey, innerKey);

                var predicate = makeNullable
                    ? Expression.AndAlso(
                        outerKey is NewArrayExpression newArrayExpression
                            ? newArrayExpression.Expressions
                                .Select(
                                    e =>
                                    {
                                        var left = (e as UnaryExpression)?.Operand ?? e;

                                        return Expression.NotEqual(left, Expression.Constant(null, left.Type));
                                    })
                                .Aggregate(Expression.AndAlso)
                            : Expression.NotEqual(outerKey, Expression.Constant(null, outerKey.Type)),
                        keyComparison)
                    : keyComparison;

                var correlationPredicate = Expression.Lambda(predicate, correlationPredicateParameter);

                return Expression.Call(
                    QueryableMethods.Where.MakeGenericMethod(innerSequenceType),
                    innerShapedQuery,
                    Expression.Quote(correlationPredicate));
            }

            return entityProjectionExpression.BindNavigation(navigation)
                ?? _selectExpression.GenerateOwnedReferenceEntityProjectionExpression(
                    entityProjectionExpression, navigation, _sqlExpressionFactory);

            static TableExpressionBase FindRootTableExpressionForColumn(ColumnExpression column)
            {
                var table = column.Table;
                if (table is JoinExpressionBase joinExpressionBase)
                {
                    table = joinExpressionBase.Table;
                }
                else if (table is SetOperationBase setOperationBase)
                {
                    table = setOperationBase.Source1;
                }

                if (table is SelectExpression selectExpression)
                {
                    var matchingProjection = (ColumnExpression)selectExpression.Projection.Single(p => p.Alias == column.Name).Expression;

                    return FindRootTableExpressionForColumn(matchingProjection);
                }

                return table;
            }
        }

        private Expression? TryBindPrimitiveCollection(Expression? source, string memberName)
        {
            while (source is IncludeExpression includeExpression)
            {
                source = includeExpression.EntityExpression;
            }

            source = source.UnwrapTypeConversion(out var convertedType);
            if (source is not EntityShaperExpression entityShaperExpression)
            {
                return null;
            }

            var entityType = entityShaperExpression.EntityType;
            if (convertedType != null)
            {
                entityType = entityType.GetRootType().GetDerivedTypesInclusive()
                    .FirstOrDefault(et => et.ClrType == convertedType);

                if (entityType == null)
                {
                    return null;
                }
            }

            // TODO: Check that the property is a primitive collection property directly once we have that in metadata, rather than
            // looking at the type mapping.
            var property = entityType.FindProperty(memberName);
            if (property?.GetRelationalTypeMapping().ElementTypeMapping is null)
            {
                return null;
            }

            return source.CreateEFPropertyExpression(property);
        }

        private sealed class AnnotationApplyingExpressionVisitor : ExpressionVisitor
        {
            private readonly IReadOnlyList<IAnnotation> _annotations;

            public AnnotationApplyingExpressionVisitor(IReadOnlyList<IAnnotation> annotations)
            {
                _annotations = annotations;
            }

            [return: NotNullIfNotNull("expression")]
            public override Expression? Visit(Expression? expression)
            {
                if (expression is TableExpression te)
                {
                    TableExpressionBase ownedTable = te;
                    foreach (var annotation in _annotations)
                    {
                        ownedTable = ownedTable.AddAnnotation(annotation.Name, annotation.Value);
                    }

                    return ownedTable;
                }

                return base.Visit(expression);
            }
        }

        private bool TryGetJsonQueryExpression(
            EntityShaperExpression entityShaperExpression,
            [NotNullWhen(true)] out JsonQueryExpression? jsonQueryExpression)
        {
            switch (entityShaperExpression.ValueBufferExpression)
            {
                case ProjectionBindingExpression projectionBindingExpression:
                    jsonQueryExpression = _selectExpression.GetProjection(projectionBindingExpression) as JsonQueryExpression;
                    return jsonQueryExpression != null;

                case JsonQueryExpression jqe:
                    jsonQueryExpression = jqe;
                    return true;

                default:
                    jsonQueryExpression = null;
                    return false;
            }
        }

        private EntityProjectionExpression GetEntityProjectionExpression(EntityShaperExpression entityShaperExpression)
            => entityShaperExpression.ValueBufferExpression switch
            {
                ProjectionBindingExpression projectionBindingExpression
                    => (EntityProjectionExpression)_selectExpression.GetProjection(projectionBindingExpression),
                EntityProjectionExpression entityProjectionExpression => entityProjectionExpression,
                _ => throw new InvalidOperationException()
            };
    }

    private ShapedQueryExpression TranslateTwoParameterSelector(ShapedQueryExpression source, LambdaExpression resultSelector)
    {
        var transparentIdentifierType = source.ShaperExpression.Type;
        var transparentIdentifierParameter = Expression.Parameter(transparentIdentifierType);

        Expression original1 = resultSelector.Parameters[0];
        var replacement1 = AccessField(transparentIdentifierType, transparentIdentifierParameter, "Outer");
        Expression original2 = resultSelector.Parameters[1];
        var replacement2 = AccessField(transparentIdentifierType, transparentIdentifierParameter, "Inner");
        var newResultSelector = Expression.Lambda(
            new ReplacingExpressionVisitor(
                    new[] { original1, original2 }, new[] { replacement1, replacement2 })
                .Visit(resultSelector.Body),
            transparentIdentifierParameter);

        return TranslateSelect(source, newResultSelector);
    }

    private static Expression AccessField(
        Type transparentIdentifierType,
        Expression targetExpression,
        string fieldName)
        => Expression.Field(targetExpression, transparentIdentifierType.GetTypeInfo().GetDeclaredField(fieldName)!);

    private static Expression MatchShaperNullabilityForSetOperation(Expression shaper1, Expression shaper2, bool makeNullable)
    {
        switch (shaper1)
        {
            case EntityShaperExpression entityShaperExpression1
                when shaper2 is EntityShaperExpression entityShaperExpression2:
                return entityShaperExpression1.IsNullable != entityShaperExpression2.IsNullable
                    ? entityShaperExpression1.MakeNullable(makeNullable)
                    : entityShaperExpression1;

            case NewExpression newExpression1
                when shaper2 is NewExpression newExpression2:
                var newArguments = new Expression[newExpression1.Arguments.Count];
                for (var i = 0; i < newArguments.Length; i++)
                {
                    newArguments[i] = MatchShaperNullabilityForSetOperation(
                        newExpression1.Arguments[i], newExpression2.Arguments[i], makeNullable);
                }

                return newExpression1.Update(newArguments);

            case MemberInitExpression memberInitExpression1
                when shaper2 is MemberInitExpression memberInitExpression2:
                var newExpression = (NewExpression)MatchShaperNullabilityForSetOperation(
                    memberInitExpression1.NewExpression, memberInitExpression2.NewExpression, makeNullable);

                var memberBindings = new MemberBinding[memberInitExpression1.Bindings.Count];
                for (var i = 0; i < memberBindings.Length; i++)
                {
                    var memberAssignment = memberInitExpression1.Bindings[i] as MemberAssignment;
                    Check.DebugAssert(memberAssignment != null, "Only member assignment bindings are supported");

                    memberBindings[i] = memberAssignment.Update(
                        MatchShaperNullabilityForSetOperation(
                            memberAssignment.Expression, ((MemberAssignment)memberInitExpression2.Bindings[i]).Expression,
                            makeNullable));
                }

                return memberInitExpression1.Update(newExpression, memberBindings);

            default:
                return shaper1;
        }
    }

    private ShapedQueryExpression? TranslateAggregateWithPredicate(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        MethodInfo predicateLessMethodInfo,
        bool liftOrderings)
    {
        if (predicate != null)
        {
            var translatedSource = TranslateWhere(source, predicate);
            if (translatedSource == null)
            {
                return null;
            }

            source = translatedSource;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        if (!selectExpression.IsDistinct)
        {
            selectExpression.ReplaceProjection(new List<Expression>());
        }

        selectExpression.PrepareForAggregate(liftOrderings);
        var selector = _sqlExpressionFactory.Fragment("*");
        var methodCall = Expression.Call(
            predicateLessMethodInfo.MakeGenericMethod(selector.Type),
            Expression.Call(
                QueryableMethods.AsQueryable.MakeGenericMethod(selector.Type), new EnumerableExpression(selector)));
        var translation = TranslateExpression(methodCall);
        if (translation == null)
        {
            return null;
        }

        var projectionMapping = new Dictionary<ProjectionMember, Expression> { { new ProjectionMember(), translation } };

        selectExpression.ClearOrdering();
        selectExpression.ReplaceProjection(projectionMapping);
        var resultType = predicateLessMethodInfo.ReturnType;

        return source.UpdateShaperExpression(
            Expression.Convert(
                new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), resultType.MakeNullable()),
                resultType));
    }

    private ShapedQueryExpression? TranslateAggregateWithSelector(
        ShapedQueryExpression source,
        LambdaExpression? selectorLambda,
        Func<Type, MethodInfo> methodGenerator,
        bool throwWhenEmpty,
        Type resultType)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        selectExpression.PrepareForAggregate();

        Expression? selector = null;
        if (selectorLambda == null
            || selectorLambda.Body == selectorLambda.Parameters[0])
        {
            var shaperExpression = source.ShaperExpression;
            if (shaperExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
            {
                shaperExpression = unaryExpression.Operand;
            }

            if (shaperExpression is ProjectionBindingExpression projectionBindingExpression)
            {
                selector = selectExpression.GetProjection(projectionBindingExpression);
            }
        }
        else
        {
            selector = RemapLambdaBody(source, selectorLambda);
        }

        if (selector == null
            || TranslateExpression(selector) is not SqlExpression translatedSelector)
        {
            return null;
        }

        var methodCall = Expression.Call(
            methodGenerator(translatedSelector.Type),
            Expression.Call(
                QueryableMethods.AsQueryable.MakeGenericMethod(translatedSelector.Type), new EnumerableExpression(translatedSelector)));
        var translation = _sqlTranslator.Translate(methodCall);
        if (translation == null)
        {
            return null;
        }

        selectExpression.ReplaceProjection(
            new Dictionary<ProjectionMember, Expression> { { new ProjectionMember(), translation } });

        selectExpression.ClearOrdering();
        Expression shaper;

        if (throwWhenEmpty)
        {
            // Avg/Max/Min case.
            // We always read nullable value
            // If resultType is nullable then we always return null. Only non-null result shows throwing behavior.
            // otherwise, if projection.Type is nullable then server result is passed through DefaultIfEmpty, hence we return default
            // otherwise, server would return null only if it is empty, and we throw
            var nullableResultType = resultType.MakeNullable();
            shaper = new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), nullableResultType);
            var resultVariable = Expression.Variable(nullableResultType, "result");
            var returnValueForNull = resultType.IsNullableType()
                ? (Expression)Expression.Constant(null, resultType)
                : translation.Type.IsNullableType()
                    ? Expression.Default(resultType)
                    : Expression.Throw(
                        Expression.New(
                            typeof(InvalidOperationException).GetConstructors()
                                .Single(ci => ci.GetParameters().Length == 1),
                            Expression.Constant(CoreStrings.SequenceContainsNoElements)),
                        resultType);

            shaper = Expression.Block(
                new[] { resultVariable },
                Expression.Assign(resultVariable, shaper),
                Expression.Condition(
                    Expression.Equal(resultVariable, Expression.Default(nullableResultType)),
                    returnValueForNull,
                    resultType != resultVariable.Type
                        ? Expression.Convert(resultVariable, resultType)
                        : resultVariable));
        }
        else
        {
            // Sum case. Projection is always non-null. We read nullable value.
            shaper = new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), translation.Type.MakeNullable());

            if (resultType != shaper.Type)
            {
                shaper = Expression.Convert(shaper, resultType);
            }
        }

        return source.UpdateShaperExpression(shaper);
    }

    private bool TryGetProjection(ShapedQueryExpression shapedQueryExpression, [NotNullWhen(true)] out SqlExpression? projection)
    {
        var shaperExpression = shapedQueryExpression.ShaperExpression;
        // No need to check ConvertChecked since this is convert node which we may have added during projection
        if (shaperExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression
            && unaryExpression.Operand.Type.IsNullableType()
            && unaryExpression.Operand.Type.UnwrapNullableType() == unaryExpression.Type)
        {
            shaperExpression = unaryExpression.Operand;
        }

        if (shapedQueryExpression.QueryExpression is SelectExpression selectExpression
            && shaperExpression is ProjectionBindingExpression projectionBindingExpression
            && selectExpression.GetProjection(projectionBindingExpression) is SqlExpression sqlExpression)
        {
            projection = sqlExpression;
            return true;
        }

        projection = null;
        return false;
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
    ///         The visitor dose not change the query tree in any way - it only populates the inferred type mappings it identified in
    ///         the given dictionary; actual application of the inferred type mappings happens later in
    ///         <see cref="RelationalInferredTypeMappingApplier" />. We can't do this in a single pass since untyped roots
    ///         (e.g. <see cref="ValuesExpression" /> may get visited before the type-inferred column referring to them (e.g. CROSS APPLY,
    ///         correlated subquery).
    ///     </para>
    /// </remarks>
    private sealed class ColumnTypeMappingScanner : ExpressionVisitor
    {
        private readonly Dictionary<(TableExpressionBase, string), RelationalTypeMapping?> _inferredColumns = new();

        private SelectExpression? _currentSelectExpression;
        private ProjectionExpression? _currentProjectionExpression;

        public IReadOnlyDictionary<(TableExpressionBase, string), RelationalTypeMapping?> Scan(Expression expression)
        {
            _inferredColumns.Clear();

            Visit(expression);

            return _inferredColumns;
        }

        protected override Expression VisitExtension(Expression node)
        {
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
                    }
                    when WasMaybeOriginallyUntyped(columnExpression):
                {
                    RegisterInferredTypeMapping(columnExpression, typeMapping);

                    return base.VisitExtension(node);
                }

                // InExpression over a subquery: apply the item's type mapping on the subquery
                case InExpression
                    {
                        Item.TypeMapping: { } typeMapping,
                        Subquery.Projection: [{ Expression: ColumnExpression columnExpression }]
                    }
                    when WasMaybeOriginallyUntyped(columnExpression):
                {
                    RegisterInferredTypeMapping(columnExpression, typeMapping);

                    return base.VisitExtension(node);
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
                    if (projection1.TypeMapping is not null && WasMaybeOriginallyUntyped(column2))
                    {
                        RegisterInferredTypeMapping(column2, projection1.TypeMapping);
                    }

                    if (projection2.TypeMapping is not null && WasMaybeOriginallyUntyped(column1))
                    {
                        RegisterInferredTypeMapping(column1, projection2.TypeMapping);
                    }

                    return base.VisitExtension(node);
                }

                // Record state on the SelectExpression and ProjectionExpression so that we can associate ColumnExpressions to the
                // projections they're in (see below).
                case SelectExpression selectExpression:
                {
                    var parentSelectExpression = _currentSelectExpression;
                    _currentSelectExpression = selectExpression;
                    var visited = base.VisitExtension(selectExpression);
                    _currentSelectExpression = parentSelectExpression;
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
                    when _currentSelectExpression is not null
                    && _currentProjectionExpression is not null
                    && _inferredColumns.TryGetValue(
                        (_currentSelectExpression, _currentProjectionExpression.Alias), out var inferredTypeMapping)
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

            static bool WasMaybeOriginallyUntyped(ColumnExpression columnExpression)
            {
                var underlyingTable = columnExpression.Table is JoinExpressionBase joinExpression
                    ? joinExpression.Table
                    : columnExpression.Table;

                return underlyingTable switch
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
            var underlyingTable = columnExpression.Table is JoinExpressionBase joinExpression
                ? joinExpression.Table
                : columnExpression.Table;

            if (_inferredColumns.TryGetValue((underlyingTable, columnExpression.Name), out var knownTypeMapping)
                && knownTypeMapping is not null
                && inferredTypeMapping.StoreType != knownTypeMapping.StoreType)
            {
                // A different type mapping was already inferred for this column - we have a conflict.
                // Null out the value for the inferred type mapping as an indication of the conflict. If it turns out that we need the
                // inferred mapping later, during the application phase, we'll throw an exception at that point (not all the inferred type
                // mappings here will actually be needed, so we don't want to needlessly throw here).
                _inferredColumns[(underlyingTable, columnExpression.Name)] = null;
                return;
            }

            _inferredColumns[(underlyingTable, columnExpression.Name)] = inferredTypeMapping;
        }
    }

    /// <summary>
    ///     A visitor executed at the end of translation, which verifies that all <see cref="SqlExpression" /> nodes have a type mapping,
    ///     and applies type mappings inferred for queryable constants (VALUES) and parameters (e.g. OPENJSON) back on their root tables.
    /// </summary>
    protected class RelationalInferredTypeMappingApplier : ExpressionVisitor
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private SelectExpression? _currentSelectExpression;

        /// <summary>
        ///     The inferred type mappings to be applied back on their query roots.
        /// </summary>
        private readonly IReadOnlyDictionary<(TableExpressionBase Table, string ColumnName), RelationalTypeMapping?> _inferredTypeMappings;

        /// <summary>
        ///     Creates a new instance of the <see cref="RelationalInferredTypeMappingApplier" /> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="sqlExpressionFactory">The SQL expression factory.</param>
        /// <param name="inferredTypeMappings">The inferred type mappings to be applied back on their query roots.</param>
        public RelationalInferredTypeMappingApplier(
            IModel model,
            ISqlExpressionFactory sqlExpressionFactory,
            IReadOnlyDictionary<(TableExpressionBase, string), RelationalTypeMapping?> inferredTypeMappings)
        {
            Model = model;
            _sqlExpressionFactory = sqlExpressionFactory;
            _inferredTypeMappings = inferredTypeMappings;
        }

        /// <summary>
        ///     The model.
        /// </summary>
        protected virtual IModel Model { get; }

        /// <summary>
        ///     Attempts to find an inferred type mapping for the given table column.
        /// </summary>
        /// <param name="table">The table containing the column for which to find the inferred type mapping.</param>
        /// <param name="columnName">The name of the column for which to find the inferred type mapping.</param>
        /// <param name="inferredTypeMapping">The inferred type mapping, or <see langword="null" /> if none could be found.</param>
        /// <returns>Whether an inferred type mapping could be found.</returns>
        protected virtual bool TryGetInferredTypeMapping(
            TableExpressionBase table,
            string columnName,
            [NotNullWhen(true)] out RelationalTypeMapping? inferredTypeMapping)
        {
            if (_inferredTypeMappings.TryGetValue((table, columnName), out inferredTypeMapping))
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
                    when TryGetInferredTypeMapping(columnExpression.Table, columnExpression.Name, out var typeMapping):
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
                            p => p.Expression is ColumnExpression { Name: ValuesOrderingColumnName } c && c.Table == valuesExpression));

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
            var inferredTypeMappings = TryGetInferredTypeMapping(valuesExpression, ValuesValueColumnName, out var typeMapping)
                ? new[] { null, typeMapping }
                : new RelationalTypeMapping?[] { null, null };

            Check.DebugAssert(
                valuesExpression.ColumnNames[0] == ValuesOrderingColumnName, "First ValuesExpression column isn't the ordering column");
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

            return new ValuesExpression(valuesExpression.Alias, newRowValues, newColumnNames, valuesExpression.GetAnnotations());
        }
    }
}
