// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
public partial class RelationalQueryableMethodTranslatingExpressionVisitor : QueryableMethodTranslatingExpressionVisitor
{
    private const string SqlQuerySingleColumnAlias = "Value";

    private readonly RelationalSqlTranslatingExpressionVisitor _sqlTranslator;
    private readonly SharedTypeEntityExpandingExpressionVisitor _sharedTypeEntityExpandingExpressionVisitor;
    private readonly RelationalProjectionBindingExpressionVisitor _projectionBindingExpressionVisitor;
    private readonly RelationalQueryCompilationContext _queryCompilationContext;
    private readonly SqlAliasManager _sqlAliasManager;
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly bool _subquery;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public const string ValuesOrderingColumnName = "_ord";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public const string ValuesValueColumnName = "Value";

    /// <summary>
    ///     Creates a new instance of the <see cref="QueryableMethodTranslatingExpressionVisitor" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this class.</param>
    /// <param name="queryCompilationContext">The query compilation context object to use.</param>
    public RelationalQueryableMethodTranslatingExpressionVisitor(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
        RelationalQueryCompilationContext queryCompilationContext)
        : base(dependencies, queryCompilationContext, subquery: false)
    {
        RelationalDependencies = relationalDependencies;

        var sqlExpressionFactory = relationalDependencies.SqlExpressionFactory;
        _queryCompilationContext = queryCompilationContext;
        _sqlAliasManager = queryCompilationContext.SqlAliasManager;
        _sqlTranslator = relationalDependencies.RelationalSqlTranslatingExpressionVisitorFactory.Create(queryCompilationContext, this);
        _sharedTypeEntityExpandingExpressionVisitor = new SharedTypeEntityExpandingExpressionVisitor(this);
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
        _sqlAliasManager = _queryCompilationContext.SqlAliasManager;
        _sqlTranslator = RelationalDependencies.RelationalSqlTranslatingExpressionVisitorFactory.Create(
            parentVisitor._queryCompilationContext, parentVisitor);
        _sharedTypeEntityExpandingExpressionVisitor = new SharedTypeEntityExpandingExpressionVisitor(this);
        _projectionBindingExpressionVisitor = new RelationalProjectionBindingExpressionVisitor(this, _sqlTranslator);
        _typeMappingSource = parentVisitor._typeMappingSource;
        _sqlExpressionFactory = parentVisitor._sqlExpressionFactory;
        _subquery = true;
    }

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        switch (extensionExpression)
        {
            case FromSqlQueryRootExpression fromSqlQueryRootExpression:
            {
                var table = fromSqlQueryRootExpression.EntityType.GetDefaultMappings().Single().Table;
                var alias = _sqlAliasManager.GenerateTableAlias(table);

                return CreateShapedQueryExpression(
                    fromSqlQueryRootExpression.EntityType,
                    CreateSelect(
                        fromSqlQueryRootExpression.EntityType,
                        new FromSqlExpression(alias, table, fromSqlQueryRootExpression.Sql, fromSqlQueryRootExpression.Argument)));
            }

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
                                Expression.Default(methodInfo.DeclaringType!),
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
                var alias = _sqlAliasManager.GenerateTableAlias(function);
                var translation = new TableValuedFunctionExpression(alias, function, arguments);
                var queryExpression = CreateSelect(entityType, translation);

                return CreateShapedQueryExpression(entityType, queryExpression);
            }

            case EntityQueryRootExpression entityQueryRootExpression
                when entityQueryRootExpression.GetType() == typeof(EntityQueryRootExpression)
                && entityQueryRootExpression.EntityType.GetSqlQueryMappings().FirstOrDefault(m => m.IsDefaultSqlQueryMapping)?.SqlQuery is
                    ISqlQuery sqlQuery:
            {
                var table = entityQueryRootExpression.EntityType.GetDefaultMappings().Single().Table;
                var alias = _sqlAliasManager.GenerateTableAlias(table);

                return CreateShapedQueryExpression(
                    entityQueryRootExpression.EntityType,
                    CreateSelect(
                        entityQueryRootExpression.EntityType,
                        new FromSqlExpression(alias, table, sqlQuery.Sql, Expression.Constant(Array.Empty<object>(), typeof(object[])))));
            }

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

                var alias = _sqlAliasManager.GenerateTableAlias("sql");
                var selectExpression = new SelectExpression(
                    [new FromSqlExpression(alias, sqlQueryRootExpression.Sql, sqlQueryRootExpression.Argument)],
                    new ColumnExpression(
                        SqlQuerySingleColumnAlias,
                        alias,
                        sqlQueryRootExpression.Type.UnwrapNullableType(),
                        typeMapping,
                        sqlQueryRootExpression.Type.IsNullableType()),
                    identifier: [],
                    _sqlAliasManager);

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

        // For Contains over a collection parameter, if the provider hasn't implemented TranslateCollection (e.g. OPENJSON on SQL
        // Server), we need to fall back to the previous IN translation.
        if (translated == QueryCompilationContext.NotTranslatedExpression
            && method.IsGenericMethod
            && method.GetGenericMethodDefinition() == QueryableMethods.Contains
            && methodCallExpression.Arguments[0] is ParameterQueryRootExpression parameterSource
            && TranslateExpression(methodCallExpression.Arguments[1]) is SqlExpression item
            && _sqlTranslator.Visit(parameterSource.ParameterExpression) is SqlParameterExpression sqlParameterExpression)
        {
            var inExpression = _sqlExpressionFactory.In(item, sqlParameterExpression);
            var selectExpression = new SelectExpression(inExpression, _sqlAliasManager);
            var shaperExpression = Expression.Convert(
                new ProjectionBindingExpression(selectExpression, new ProjectionMember(), typeof(bool?)), typeof(bool));
            var shapedQueryExpression = new ShapedQueryExpression(selectExpression, shaperExpression)
                .UpdateResultCardinality(ResultCardinality.Single);
            return shapedQueryExpression;
        }

        return translated;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateMemberAccess(Expression source, MemberIdentity member)
    {
        // Attempt to translate access into a primitive collection property (i.e. array column)
        if (_sqlTranslator.TryBindMember(_sqlTranslator.Visit(source), member, out var translatedExpression, out var property)
            && property is IProperty { IsPrimitiveCollection: true } regularProperty
            && translatedExpression is SqlExpression sqlExpression
            && TranslatePrimitiveCollection(
                    sqlExpression, regularProperty, _sqlAliasManager.GenerateTableAlias(GenerateTableAlias(sqlExpression))) is
                { } primitiveCollectionTranslation)
        {
            return primitiveCollectionTranslation;
        }

        return null;

        string GenerateTableAlias(SqlExpression sqlExpression)
            => sqlExpression switch
            {
                ColumnExpression c => c.Name,
                JsonScalarExpression jsonScalar
                    => jsonScalar.Path.LastOrDefault(s => s.PropertyName is not null) is PathSegment lastPropertyNameSegment
                        ? lastPropertyNameSegment.PropertyName!
                        : GenerateTableAlias(jsonScalar.Json),
                ScalarSubqueryExpression scalarSubquery => scalarSubquery.Subquery.Projection[0].Alias,

                _ => "collection"
            };
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateParameterQueryRoot(ParameterQueryRootExpression parameterQueryRootExpression)
    {
        var sqlParameterExpression =
            _sqlTranslator.Visit(parameterQueryRootExpression.ParameterExpression) as SqlParameterExpression;

        Check.DebugAssert(sqlParameterExpression is not null, "sqlParameterExpression is not null");

        var tableAlias = _sqlAliasManager.GenerateTableAlias(sqlParameterExpression.Name.TrimStart('_'));
        return TranslatePrimitiveCollection(sqlParameterExpression, property: null, tableAlias);
    }

    /// <summary>
    ///     Translates a parameter or column collection of primitive values. Providers can override this to translate e.g. int[] columns or
    ///     parameters to a queryable table (OPENJSON on SQL Server, unnest on PostgreSQL...). The default implementation always returns
    ///     <see langword="null" /> (no translation).
    /// </summary>
    /// <remarks>
    ///     Inline collections aren't passed to this method; see <see cref="TranslateInlineQueryRoot" /> for the translation of inline
    ///     collections.
    /// </remarks>
    /// <param name="sqlExpression">The expression to try to translate as a primitive collection expression.</param>
    /// <param name="property">
    ///     If the primitive collection is a property, contains the <see cref="IProperty" /> for that property. Otherwise, the collection
    ///     represents a parameter, and this contains <see langword="null" />.
    /// </param>
    /// <param name="tableAlias">
    ///     Provides an alias to be used for the table returned from translation, which will represent the collection.
    /// </param>
    /// <returns>A <see cref="ShapedQueryExpression" /> if the translation was successful, otherwise <see langword="null" />.</returns>
    protected virtual ShapedQueryExpression? TranslatePrimitiveCollection(
        SqlExpression sqlExpression,
        IProperty? property,
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
    protected override ShapedQueryExpression? TranslateInlineQueryRoot(InlineQueryRootExpression inlineQueryRootExpression)
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

        var alias = _sqlAliasManager.GenerateTableAlias("values");
        var valuesExpression = new ValuesExpression(alias, rowExpressions, new[] { ValuesOrderingColumnName, ValuesValueColumnName });

        // Note: we leave the element type mapping null, to allow it to get inferred based on queryable operators composed on top.
        var selectExpression = new SelectExpression(
            [valuesExpression],
            new ColumnExpression(
                ValuesValueColumnName,
                alias,
                elementType.UnwrapNullableType(),
                typeMapping: null,
                nullable: encounteredNull),
            identifier: [],
            _sqlAliasManager);

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
        => CreateShapedQueryExpression(entityType, CreateSelect(entityType));

    private static ShapedQueryExpression CreateShapedQueryExpression(IEntityType entityType, SelectExpression selectExpression)
        => new(
            selectExpression,
            new RelationalStructuralTypeShaperExpression(
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
        subquery = new SelectExpression(translation, _sqlAliasManager);

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
        var selectExpression = new SelectExpression(translation, _sqlAliasManager);

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

            // We do attempt one thing: if this is a contains over an entity type which has a single key property (non-composite key),
            // we can project its key property (entity equality/containment) and translate to InExpression over that.
            if (item is StructuralTypeShaperExpression { StructuralType: IEntityType entityType }
                && entityType.FindPrimaryKey()?.Properties is [var singleKeyProperty])
            {
                var keySelectorParam = Expression.Parameter(source.Type);

                return TranslateContains(
                    TranslateSelect(
                        source,
                        Expression.Lambda(keySelectorParam.CreateEFPropertyExpression(singleKeyProperty), keySelectorParam)),
                    item.CreateEFPropertyExpression(singleKeyProperty));
            }

            // Otherwise, attempt to translate as Any since that passes through Where predicate translation. This will e.g. take care of
            // entity , which e.g. does entity equality/containment for entities with composite keys.
            var anyLambdaParameter = Expression.Parameter(item.Type, "p");
            var anyLambda = Expression.Lambda(
                Infrastructure.ExpressionExtensions.CreateEqualsExpression(anyLambdaParameter, item),
                anyLambdaParameter);

            return TranslateAny(source, anyLambda);
        }

        // Pattern-match Contains over ValuesExpression, translating to simplified 'item IN (1, 2, 3)' with constant elements
        if (TryExtractBareInlineCollectionValues(source, out var values))
        {
            var inExpression = _sqlExpressionFactory.In(translatedItem, values);
            return source.Update(new SelectExpression(inExpression, _sqlAliasManager), source.ShaperExpression);
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
        subquery = new SelectExpression(translation, _sqlAliasManager);

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
            if (remappedKeySelector is not StructuralTypeShaperExpression
                {
                    ValueBufferExpression: ProjectionBindingExpression pbe
                } shaper)
            {
                // ValueBufferExpression can be JsonQuery, ProjectionBindingExpression, EntityProjection
                // We only allow ProjectionBindingExpression which represents a regular entity
                return null;
            }

            translatedKey = shaper.Update(((SelectExpression)pbe.QueryExpression).GetProjection(pbe));
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
        => TryExtractBareInlineCollectionValues(source, out var values)
            && _sqlExpressionFactory.TryCreateGreatest(values, resultType, out var greatestExpression)
                ? source.Update(new SelectExpression(greatestExpression, _sqlAliasManager), source.ShaperExpression)
                : TranslateAggregateWithSelector(
                    source, selector, t => QueryableMethods.MaxWithoutSelector.MakeGenericMethod(t), throwWhenEmpty: true, resultType);

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateMin(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
        => TryExtractBareInlineCollectionValues(source, out var values)
            && _sqlExpressionFactory.TryCreateLeast(values, resultType, out var leastExpression)
                ? source.Update(new SelectExpression(leastExpression, _sqlAliasManager), source.ShaperExpression)
                : TranslateAggregateWithSelector(
                    source, selector, t => QueryableMethods.MinWithoutSelector.MakeGenericMethod(t), throwWhenEmpty: true, resultType);

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateOfType(ShapedQueryExpression source, Type resultType)
    {
        if (source.ShaperExpression is StructuralTypeShaperExpression { StructuralType: IEntityType entityType } shaper)
        {
            if (entityType.ClrType == resultType)
            {
                return source;
            }

            var parameterExpression = Expression.Parameter(shaper.Type);
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
                return source.UpdateShaperExpression(shaper.WithType(baseType));
            }

            var derivedType = entityType.GetDerivedTypes().Single(et => et.ClrType == resultType);
            var projectionBindingExpression = (ProjectionBindingExpression)shaper.ValueBufferExpression;

            var projectionMember = projectionBindingExpression.ProjectionMember;
            Check.DebugAssert(new ProjectionMember().Equals(projectionMember), "Invalid ProjectionMember when processing OfType");

            var projection = (StructuralTypeProjectionExpression)selectExpression.GetProjection(projectionBindingExpression);
            selectExpression.ReplaceProjection(
                new Dictionary<ProjectionMember, Expression> { { projectionMember, projection.UpdateEntityType(derivedType) } });

            return source.UpdateShaperExpression(shaper.WithType(derivedType));
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
    /// <returns>Whether <paramref name="selectExpression" /> is ordered.</returns>
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

    private sealed class IncludePruner : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
            => node switch
            {
                IncludeExpression { Navigation: ISkipNavigation or not INavigation } i => i,
                IncludeExpression i => Visit(i.EntityExpression),
                _ => base.VisitExtension(node)
            };
    }

    private sealed class SharedTypeEntityExpandingExpressionVisitor(
        RelationalQueryableMethodTranslatingExpressionVisitor queryableTranslator)
        : ExpressionVisitor
    {
        private readonly SqlAliasManager _sqlAliasManager = queryableTranslator._sqlAliasManager;
        private SelectExpression _selectExpression = null!;

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
                    var collectionIndexExpression = queryableTranslator._sqlTranslator.Translate(methodCallExpression.Arguments[1]);
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

                    var entityShaper = new RelationalStructuralTypeShaperExpression(
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
                            Expression: RelationalStructuralTypeShaperExpression { ValueBufferExpression: JsonQueryExpression memberJqe }
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
                        return expression is RelationalStructuralTypeShaperExpression { ValueBufferExpression: JsonQueryExpression reseJqe }
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
            => extensionExpression is StructuralTypeShaperExpression or ShapedQueryExpression or GroupByShaperExpression
                ? extensionExpression
                : base.VisitExtension(extensionExpression);

        private Expression? TryExpand(Expression? source, MemberIdentity member)
        {
            source = source.UnwrapTypeConversion(out var convertedType);
            if (source is not StructuralTypeShaperExpression shaper)
            {
                return null;
            }

            if (shaper.StructuralType is not IEntityType entityType)
            {
                return null;
            }

            if (convertedType != null)
            {
                var convertedEntityType = entityType.GetRootType().GetDerivedTypesInclusive()
                    .FirstOrDefault(et => et.ClrType == convertedType);

                if (convertedEntityType == null)
                {
                    return null;
                }

                entityType = convertedEntityType;
            }

            var navigation = member.MemberInfo != null
                ? entityType.FindNavigation(member.MemberInfo)
                : entityType.FindNavigation(member.Name!);

            if (navigation is { TargetEntityType: IEntityType targetEntityType }
                && targetEntityType.IsOwned())
            {
                return ExpandOwnedNavigation(navigation);
            }

            return null;

            Expression ExpandOwnedNavigation(INavigation navigation)
            {
                var targetEntityType = navigation.TargetEntityType;

                if (TryGetJsonQueryExpression(shaper, out var jsonQueryExpression))
                {
                    var newJsonQueryExpression = jsonQueryExpression.BindNavigation(navigation);

                    return navigation.IsCollection
                        ? newJsonQueryExpression
                        : new RelationalStructuralTypeShaperExpression(
                            navigation.TargetEntityType,
                            newJsonQueryExpression,
                            nullable: shaper.IsNullable || !navigation.ForeignKey.IsRequired);
                }

                var entityProjectionExpression = GetEntityProjectionExpression(shaper);
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

                    var sourceTable = FindRootTableExpressionForColumn(_selectExpression, sourceColumn);
                    var innerSelectExpression = queryableTranslator.CreateSelect(targetEntityType);
                    innerSelectExpression = (SelectExpression)new AnnotationApplyingExpressionVisitor(sourceTable.GetAnnotations().ToList())
                        .Visit(innerSelectExpression);

                    var innerShapedQuery = CreateShapedQueryExpression(targetEntityType, innerSelectExpression);

                    var makeNullable = foreignKey.PrincipalKey.Properties
                        .Concat(foreignKey.Properties)
                        .Select(p => p.ClrType)
                        .Any(t => t.IsNullableType());

                    var innerSequenceType = innerShapedQuery.Type.GetSequenceType();
                    var correlationPredicateParameter = Expression.Parameter(innerSequenceType);

                    var outerKey = shaper.CreateKeyValuesExpression(
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
                        entityProjectionExpression, navigation, queryableTranslator._sqlExpressionFactory, _sqlAliasManager);
            }

            static TableExpressionBase FindRootTableExpressionForColumn(SelectExpression select, ColumnExpression column)
            {
                var table = select.GetTable(column).UnwrapJoin();

                if (table is SetOperationBase setOperationBase)
                {
                    table = setOperationBase.Source1;
                }

                if (table is SelectExpression innerSelect)
                {
                    var matchingProjection = (ColumnExpression)innerSelect.Projection.Single(p => p.Alias == column.Name).Expression;

                    return FindRootTableExpressionForColumn(innerSelect, matchingProjection);
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
            if (source is not StructuralTypeShaperExpression shaper)
            {
                return null;
            }

            var type = shaper.StructuralType;
            if (convertedType != null)
            {
                Check.DebugAssert(
                    type is IEntityType,
                    "A type conversion was unwrapped over a complex type, which does not (yet) support inheritance");

                type = ((IEntityType)type).GetRootType().GetDerivedTypesInclusive()
                    .FirstOrDefault(et => et.ClrType == convertedType);

                if (type == null)
                {
                    return null;
                }
            }

            var property = type.FindProperty(memberName);
            if (property?.IsPrimitiveCollection != true)
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
            StructuralTypeShaperExpression shaper,
            [NotNullWhen(true)] out JsonQueryExpression? jsonQueryExpression)
        {
            switch (shaper.ValueBufferExpression)
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

        private StructuralTypeProjectionExpression GetEntityProjectionExpression(StructuralTypeShaperExpression shaper)
            => shaper.ValueBufferExpression switch
            {
                ProjectionBindingExpression projectionBindingExpression
                    => (StructuralTypeProjectionExpression)_selectExpression.GetProjection(projectionBindingExpression),
                StructuralTypeProjectionExpression typeProjection => typeProjection,
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
            case StructuralTypeShaperExpression entityShaperExpression1
                when shaper2 is StructuralTypeShaperExpression entityShaperExpression2:
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
                ? (Expression)Expression.Default(resultType)
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

    private bool TryExtractBareInlineCollectionValues(ShapedQueryExpression shapedQuery, [NotNullWhen(true)] out SqlExpression[]? values)
    {
        if (TryGetProjection(shapedQuery, out var projection)
            && shapedQuery.QueryExpression is SelectExpression
            {
                Tables:
                [
                    ValuesExpression { ColumnNames: [ValuesOrderingColumnName, ValuesValueColumnName] } valuesExpression
                ],
                Predicate: null,
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null,
                // Note that we assume ordering doesn't matter (Contains/Min/Max)
            }
            // Make sure that the source projects the column from the ValuesExpression directly, i.e. no projection out with some expression
            && projection is ColumnExpression { TableAlias: var tableAlias }
            && tableAlias == valuesExpression.Alias)
        {
            values = new SqlExpression[valuesExpression.RowValues.Count];

            for (var i = 0; i < values.Length; i++)
            {
                // Skip the first value (_ord) - this function assumes ordering doesn't matter
                values[i] = valuesExpression.RowValues[i].Values[1];
            }

            return true;
        }

        values = null;
        return false;
    }

    /// <summary>
    ///     This visitor has been obsoleted; Extend RelationalTypeMappingPostprocessor instead, and invoke it from
    ///     <see cref="RelationalQueryTranslationPostprocessor.ProcessTypeMappings" />.
    /// </summary>
    [Obsolete("Extend RelationalTypeMappingPostprocessor instead, and invoke it from  RelationalQueryTranslationPostprocessor.ProcessTypeMappings().")]
    protected class RelationalInferredTypeMappingApplier;
}
