// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosQueryableMethodTranslatingExpressionVisitor : QueryableMethodTranslatingExpressionVisitor
{
    private readonly CosmosQueryCompilationContext _queryCompilationContext;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly ITypeMappingSource _typeMappingSource;
    private readonly IMemberTranslatorProvider _memberTranslatorProvider;
    private readonly IMethodCallTranslatorProvider _methodCallTranslatorProvider;
    private readonly CosmosSqlTranslatingExpressionVisitor _sqlTranslator;
    private readonly CosmosProjectionBindingExpressionVisitor _projectionBindingExpressionVisitor;
    private readonly bool _subquery;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosQueryableMethodTranslatingExpressionVisitor(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        CosmosQueryCompilationContext queryCompilationContext,
        ISqlExpressionFactory sqlExpressionFactory,
        ITypeMappingSource typeMappingSource,
        IMemberTranslatorProvider memberTranslatorProvider,
        IMethodCallTranslatorProvider methodCallTranslatorProvider)
        : base(dependencies, queryCompilationContext, subquery: false)
    {
        _queryCompilationContext = queryCompilationContext;
        _sqlExpressionFactory = sqlExpressionFactory;
        _typeMappingSource = typeMappingSource;
        _memberTranslatorProvider = memberTranslatorProvider;
        _methodCallTranslatorProvider = methodCallTranslatorProvider;
        _sqlTranslator = new CosmosSqlTranslatingExpressionVisitor(
            queryCompilationContext,
            _sqlExpressionFactory,
            _typeMappingSource,
            _memberTranslatorProvider,
            _methodCallTranslatorProvider,
            this);
        _projectionBindingExpressionVisitor =
            new CosmosProjectionBindingExpressionVisitor(_queryCompilationContext.Model, _sqlTranslator);
        _subquery = false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected CosmosQueryableMethodTranslatingExpressionVisitor(
        CosmosQueryableMethodTranslatingExpressionVisitor parentVisitor)
        : base(parentVisitor.Dependencies, parentVisitor.QueryCompilationContext, subquery: true)
    {
        _queryCompilationContext = parentVisitor._queryCompilationContext;
        _sqlExpressionFactory = parentVisitor._sqlExpressionFactory;
        _typeMappingSource = parentVisitor._typeMappingSource;
        _memberTranslatorProvider = parentVisitor._memberTranslatorProvider;
        _methodCallTranslatorProvider = parentVisitor._methodCallTranslatorProvider;
        _sqlTranslator = new CosmosSqlTranslatingExpressionVisitor(
            QueryCompilationContext,
            _sqlExpressionFactory,
            _typeMappingSource,
            _memberTranslatorProvider,
            _methodCallTranslatorProvider,
            parentVisitor);
        _projectionBindingExpressionVisitor =
            new CosmosProjectionBindingExpressionVisitor(_queryCompilationContext.Model, _sqlTranslator);
        _subquery = true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: NotNullIfNotNull(nameof(expression))]
    public override Expression? Visit(Expression? expression)
    {
        if (expression is MethodCallExpression
            {
                Method: { Name: nameof(Queryable.FirstOrDefault), IsGenericMethod: true },
                Arguments:
                [
                    MethodCallExpression
                    {
                        Method: { Name: nameof(Queryable.Where), IsGenericMethod: true },
                        Arguments:
                        [
                            EntityQueryRootExpression { EntityType: var entityType },
                            UnaryExpression { Operand: LambdaExpression lambdaExpression, NodeType: ExpressionType.Quote }
                        ]
                    } whereMethodCall
                ]
            } firstOrDefaultMethodCall
            && firstOrDefaultMethodCall.Method.GetGenericMethodDefinition() == QueryableMethods.FirstOrDefaultWithoutPredicate
            && whereMethodCall.Method.GetGenericMethodDefinition() == QueryableMethods.Where)
        {
            var queryProperties = new List<IProperty>();
            var parameterNames = new List<string>();

            if (ExtractPartitionKeyFromPredicate(entityType, lambdaExpression.Body, queryProperties, parameterNames))
            {
                var entityTypePrimaryKeyProperties = entityType.FindPrimaryKey()!.Properties;
                var idProperty = entityType.GetProperties()
                    .First(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);
                            var partitionKeyProperties = entityType.GetPartitionKeyProperties();

                if (entityTypePrimaryKeyProperties.SequenceEqual(queryProperties)
                                && (!partitionKeyProperties.Any()
                        || partitionKeyProperties.All(p => entityTypePrimaryKeyProperties.Contains(p)))
                    && (idProperty.GetValueGeneratorFactory() != null
                        || entityTypePrimaryKeyProperties.Contains(idProperty)))
                {
                    var propertyParameterList = queryProperties.Zip(
                            parameterNames,
                            (property, parameter) => (property, parameter))
                        .ToDictionary(tuple => tuple.property, tuple => tuple.parameter);

                    var readItemExpression = new ReadItemExpression(entityType, propertyParameterList);

                    return CreateShapedQueryExpression(entityType, readItemExpression)
                        .UpdateResultCardinality(ResultCardinality.SingleOrDefault);
                }
            }
        }

        return base.Visit(expression);

        static bool ExtractPartitionKeyFromPredicate(
            IEntityType entityType,
            Expression joinCondition,
            ICollection<IProperty> properties,
            ICollection<string> parameterNames)
        {
            switch (joinCondition)
            {
                case BinaryExpression joinBinaryExpression:
                    switch (joinBinaryExpression)
                    {
                        case { NodeType: ExpressionType.AndAlso }:
                            return ExtractPartitionKeyFromPredicate(entityType, joinBinaryExpression.Left, properties, parameterNames)
                                && ExtractPartitionKeyFromPredicate(entityType, joinBinaryExpression.Right, properties, parameterNames);

                        case
                        {
                            NodeType: ExpressionType.Equal,
                            Left: MethodCallExpression equalMethodCallExpression,
                            Right: ParameterExpression { Name: string parameterName }
                        } when equalMethodCallExpression.TryGetEFPropertyArguments(out _, out var propertyName):
                            var property = entityType.FindProperty(propertyName);
                            if (property == null)
                            {
                                return false;
                            }

                            properties.Add(property);
                            parameterNames.Add(parameterName);
                            return true;
                    }

                    break;

                case MethodCallExpression
                {
                    Method.Name: "Equals",
                    Object: null,
                    Arguments:
                    [
                        MethodCallExpression equalsMethodCallExpression,
                        ParameterExpression { Name: string parameterName }
                    ]
                } when equalsMethodCallExpression.TryGetEFPropertyArguments(out _, out var propertyName):
                {
                    var property = entityType.FindProperty(propertyName);
                    if (property == null)
                    {
                        return false;
                    }

                    properties.Add(property);
                    parameterNames.Add(parameterName);
                    return true;
                }
            }

            return false;
        }
    }

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        switch (extensionExpression)
        {
            case EntityQueryRootExpression when _subquery:
                AddTranslationErrorDetails(CosmosStrings.NonCorrelatedSubqueriesNotSupported);
                return QueryCompilationContext.NotTranslatedExpression;

            case FromSqlQueryRootExpression fromSqlQueryRootExpression:
                return CreateShapedQueryExpression(
                    fromSqlQueryRootExpression.EntityType,
                    _sqlExpressionFactory.Select(
                        fromSqlQueryRootExpression.EntityType, fromSqlQueryRootExpression.Sql, fromSqlQueryRootExpression.Argument));

            default:
                return base.VisitExtension(extensionExpression);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
        => new CosmosQueryableMethodTranslatingExpressionVisitor(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override ShapedQueryExpression? TranslateSubquery(Expression expression)
    {
        var subqueryVisitor = CreateSubqueryVisitor();
        var translation = subqueryVisitor.Translate(expression) as ShapedQueryExpression;
        if (translation == null && subqueryVisitor.TranslationErrorDetails != null)
        {
            AddTranslationErrorDetails(subqueryVisitor.TranslationErrorDetails);
        }

        return translation;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression CreateShapedQueryExpression(IEntityType entityType)
        => CreateShapedQueryExpression(entityType, _sqlExpressionFactory.Select(entityType));

    private ShapedQueryExpression CreateShapedQueryExpression(IEntityType entityType, Expression queryExpression)
    {
        if (!entityType.IsOwned())
        {
            var cosmosContainer = entityType.GetContainer();
            var existingContainer = _queryCompilationContext.CosmosContainer;
            Check.DebugAssert(cosmosContainer is not null, "Non-owned entity type without a Cosmos container");

            if (existingContainer is not null && existingContainer != cosmosContainer)
            {
                throw new InvalidOperationException(CosmosStrings.MultipleContainersReferencedInQuery(cosmosContainer, existingContainer));
            }

            _queryCompilationContext.CosmosContainer = cosmosContainer;
        }

        return new ShapedQueryExpression(
            queryExpression,
            new StructuralTypeShaperExpression(
                entityType,
                new ProjectionBindingExpression(queryExpression, new ProjectionMember(), typeof(ValueBuffer)),
                false));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateAll(ShapedQueryExpression source, LambdaExpression predicate)
        => null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
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
        subquery.ClearProjection();
        subquery.ApplyProjection();
        if (subquery.Limit == null
            && subquery.Offset == null)
        {
            subquery.ClearOrdering();
        }

        var translation = _sqlExpressionFactory.Exists(subquery);
        var selectExpression = new SelectExpression(translation);

        return source.Update(
            selectExpression,
            Expression.Convert(new ProjectionBindingExpression(selectExpression, new ProjectionMember(), typeof(bool?)), typeof(bool)));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateAverage(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression.IsDistinct
            || selectExpression.Limit != null
            || selectExpression.Offset != null)
        {
            return null;
        }

        if (selector != null)
        {
            source = TranslateSelect(source, selector);
        }

        var projection = (SqlExpression)selectExpression.GetMappedProjection(new ProjectionMember());
        projection = _sqlExpressionFactory.Function("AVG", new[] { projection }, projection.Type, projection.TypeMapping);

        return AggregateResultShaper(source, projection, throwOnNullResult: true, resultType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression TranslateCast(ShapedQueryExpression source, Type resultType)
        => source.ShaperExpression.Type == resultType
            ? source
            : source.UpdateShaperExpression(Expression.Convert(source.ShaperExpression, resultType));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateConcat(ShapedQueryExpression source1, ShapedQueryExpression source2)
        => TranslateSetOperation(source1, source2, "ARRAY_CONCAT");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateContains(ShapedQueryExpression source, Expression item)
    {
        // Simplify x.Array.Contains[1] => ARRAY_CONTAINS(x.Array, 1) insert of IN+subquery
        if (CosmosQueryUtils.TryExtractBareArray(source, out var array, ignoreOrderings: true)
            && TranslateExpression(item) is SqlExpression translatedItem)
        {
            if (array is ArrayConstantExpression arrayConstant)
            {
                var inExpression = _sqlExpressionFactory.In(translatedItem, arrayConstant.Items);
                return source.Update(new SelectExpression(inExpression), source.ShaperExpression);
            }

            (translatedItem, array) = _sqlExpressionFactory.ApplyTypeMappingsOnItemAndArray(translatedItem, array);
            var simplifiedTranslation = _sqlExpressionFactory.Function("ARRAY_CONTAINS", new[] { array, translatedItem }, typeof(bool));
            return source.UpdateQueryExpression(new SelectExpression(simplifiedTranslation));
        }

        // TODO: Translation to IN, with scalars and with subquery
        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateCount(ShapedQueryExpression source, LambdaExpression? predicate)
    {
        // Simplify x.Array.Count() => ARRAY_LENGTH(x.Array) instead of (SELECT COUNT(1) FROM i IN x.Array))
        if (predicate is null
            && CosmosQueryUtils.TryExtractBareArray(source, out var array, ignoreOrderings: true))
        {
            var simplifiedTranslation = _sqlExpressionFactory.Function("ARRAY_LENGTH", new[] { array }, typeof(int));
            return source.UpdateQueryExpression(new SelectExpression(simplifiedTranslation));
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression.IsDistinct
            || selectExpression.Limit != null
            || selectExpression.Offset != null)
        {
            return null;
        }

        if (predicate != null)
        {
            if (TranslateWhere(source, predicate) is not ShapedQueryExpression translatedSource)
            {
                return null;
            }

            source = translatedSource;
        }

        var translation = _sqlExpressionFactory.ApplyDefaultTypeMapping(
            _sqlExpressionFactory.Function("COUNT", new[] { _sqlExpressionFactory.Constant(1) }, typeof(int)));

        var projectionMapping = new Dictionary<ProjectionMember, Expression> { { new ProjectionMember(), translation } };

        selectExpression.ClearOrdering();
        selectExpression.ReplaceProjectionMapping(projectionMapping);
        return source.UpdateShaperExpression(
            Expression.Convert(
                new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), typeof(int?)),
                typeof(int)));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateDefaultIfEmpty(ShapedQueryExpression source, Expression? defaultValue)
        => null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression TranslateDistinct(ShapedQueryExpression source)
    {
        ((SelectExpression)source.QueryExpression).ApplyDistinct();

        return source;
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
        // Simplify x.Array[1] => x.Array[1] (using the Cosmos array subscript operator) instead of a subquery with LIMIT/OFFSET
        if (!returnDefault
            && CosmosQueryUtils.TryExtractBareArray(source, out var array, out var projectedScalarReference)
            && TranslateExpression(index) is { } translatedIndex)
        {
            var arrayIndex = _sqlExpressionFactory.ArrayIndex(
                array, translatedIndex, projectedScalarReference.Type, projectedScalarReference.TypeMapping);
            return source.UpdateQueryExpression(new SelectExpression(arrayIndex));
        }

        // Note that Cosmos doesn't support OFFSET/LIMIT in subqueries, so this translation is for top-level entity querying only.
        // TODO: Translate with OFFSET/LIMIT
        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateExcept(ShapedQueryExpression source1, ShapedQueryExpression source2)
    {
        AddTranslationErrorDetails(CosmosStrings.ExceptNotSupported);
        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateFirstOrDefault(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        Type returnType,
        bool returnDefault)
    {
        if (predicate != null)
        {
            if (TranslateWhere(source, predicate) is not ShapedQueryExpression translatedSource)
            {
                return null;
            }

            source = translatedSource;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression is { Predicate: null, Orderings: [] })
        {
            _queryCompilationContext.Logger.FirstWithoutOrderByAndFilterWarning();
        }

        selectExpression.ApplyLimit(TranslateExpression(Expression.Constant(1))!);

        return source.ShaperExpression.Type != returnType
            ? source.UpdateShaperExpression(Expression.Convert(source.ShaperExpression, returnType))
            : source;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateGroupBy(
        ShapedQueryExpression source,
        LambdaExpression keySelector,
        LambdaExpression? elementSelector,
        LambdaExpression? resultSelector)
        => null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateGroupJoin(
        ShapedQueryExpression outer,
        ShapedQueryExpression inner,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector)
        => null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateIntersect(ShapedQueryExpression source1, ShapedQueryExpression source2)
        => TranslateSetOperation(source1, source2, "SetIntersect", ignoreOrderings: true);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateJoin(
        ShapedQueryExpression outer,
        ShapedQueryExpression inner,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector)
        => null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateLastOrDefault(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        Type returnType,
        bool returnDefault)
    {
        if (predicate != null)
        {
            if (TranslateWhere(source, predicate) is not ShapedQueryExpression translatedSource)
            {
                return null;
            }

            source = translatedSource;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        selectExpression.ReverseOrderings();
        selectExpression.ApplyLimit(TranslateExpression(Expression.Constant(1))!);

        return source.ShaperExpression.Type != returnType
            ? source.UpdateShaperExpression(Expression.Convert(source.ShaperExpression, returnType))
            : source;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateLeftJoin(
        ShapedQueryExpression outer,
        ShapedQueryExpression inner,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector)
        => null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateLongCount(ShapedQueryExpression source, LambdaExpression? predicate)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression.IsDistinct
            || selectExpression.Limit != null
            || selectExpression.Offset != null)
        {
            return null;
        }

        if (predicate != null)
        {
            if (TranslateWhere(source, predicate) is not ShapedQueryExpression translatedSource)
            {
                return null;
            }

            source = translatedSource;
        }

        var translation = _sqlExpressionFactory.ApplyDefaultTypeMapping(
            _sqlExpressionFactory.Function("COUNT", new[] { _sqlExpressionFactory.Constant(1) }, typeof(long)));
        var projectionMapping = new Dictionary<ProjectionMember, Expression> { { new ProjectionMember(), translation } };

        selectExpression.ClearOrdering();
        selectExpression.ReplaceProjectionMapping(projectionMapping);
        return source.UpdateShaperExpression(
            Expression.Convert(
                new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), typeof(long?)),
                typeof(long)));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateMax(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression.IsDistinct
            || selectExpression.Limit != null
            || selectExpression.Offset != null)
        {
            return null;
        }

        if (selector != null)
        {
            source = TranslateSelect(source, selector);
        }

        var projection = (SqlExpression)selectExpression.GetMappedProjection(new ProjectionMember());

        projection = _sqlExpressionFactory.Function("MAX", new[] { projection }, resultType, projection.TypeMapping);

        return AggregateResultShaper(source, projection, throwOnNullResult: true, resultType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateMin(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression.IsDistinct
            || selectExpression.Limit != null
            || selectExpression.Offset != null)
        {
            return null;
        }

        if (selector != null)
        {
            source = TranslateSelect(source, selector);
        }

        var projection = (SqlExpression)selectExpression.GetMappedProjection(new ProjectionMember());

        projection = _sqlExpressionFactory.Function("MIN", new[] { projection }, resultType, projection.TypeMapping);

        return AggregateResultShaper(source, projection, throwOnNullResult: true, resultType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateOfType(ShapedQueryExpression source, Type resultType)
    {
        if (source.ShaperExpression is not StructuralTypeShaperExpression entityShaperExpression)
        {
            return null;
        }

        if (entityShaperExpression.StructuralType is not IEntityType entityType)
        {
            throw new UnreachableException("Complex types not supported in Cosmos");
        }

        if (entityType.ClrType == resultType)
        {
            return source;
        }

        var parameterExpression = Expression.Parameter(entityShaperExpression.Type);
        var predicate = Expression.Lambda(Expression.TypeIs(parameterExpression, resultType), parameterExpression);
        if (TranslateLambdaExpression(source, predicate) is not SqlExpression translation)
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
            return source.UpdateShaperExpression(entityShaperExpression.WithType(baseType));
        }

        var derivedType = entityType.GetDerivedTypes().Single(et => et.ClrType == resultType);
        var projectionBindingExpression = (ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression;

        var projectionMember = projectionBindingExpression.ProjectionMember;
        Check.DebugAssert(new ProjectionMember().Equals(projectionMember), "Invalid ProjectionMember when processing OfType");

        var entityProjectionExpression = (EntityProjectionExpression)selectExpression.GetMappedProjection(projectionMember);
        selectExpression.ReplaceProjectionMapping(
            new Dictionary<ProjectionMember, Expression>
            {
                { projectionMember, entityProjectionExpression.UpdateEntityType(derivedType) }
            });

        return source.UpdateShaperExpression(entityShaperExpression.WithType(derivedType));
    }

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
        if (TranslateLambdaExpression(source, keySelector) is SqlExpression translation)
        {
            ((SelectExpression)source.QueryExpression).ApplyOrdering(new OrderingExpression(translation, ascending));

            return source;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateReverse(ShapedQueryExpression source)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression.Orderings.Count == 0)
        {
            AddTranslationErrorDetails(CosmosStrings.MissingOrderingInSelectExpression);
            return null;
        }

        selectExpression.ReverseOrderings();

        return source;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression TranslateSelect(ShapedQueryExpression source, LambdaExpression selector)
    {
        if (selector.Body == selector.Parameters[0])
        {
            return source;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression.IsDistinct)
        {
            // TODO: The base TranslateSelect does not allow returning null (presumably because client eval should always be possible)
            return null!;
        }

        var newSelectorBody = ReplacingExpressionVisitor.Replace(selector.Parameters.Single(), source.ShaperExpression, selector.Body);

        return source.UpdateShaperExpression(_projectionBindingExpressionVisitor.Translate(selectExpression, newSelectorBody));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateSelectMany(
        ShapedQueryExpression source,
        LambdaExpression collectionSelector,
        LambdaExpression resultSelector)
        => null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateSelectMany(ShapedQueryExpression source, LambdaExpression selector)
        => null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateSingleOrDefault(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        Type returnType,
        bool returnDefault)
    {
        if (predicate != null)
        {
            if (TranslateWhere(source, predicate) is not ShapedQueryExpression translatedSource)
            {
                return null;
            }

            source = translatedSource;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;
        selectExpression.ApplyLimit(TranslateExpression(Expression.Constant(2))!);

        return source.ShaperExpression.Type == returnType
            ? source
            : source.UpdateShaperExpression(Expression.Convert(source.ShaperExpression, returnType));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateSkip(ShapedQueryExpression source, Expression count)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        var translation = TranslateExpression(count);

        if (translation != null)
        {
            // Ordering of documents is not guaranteed in Cosmos, so we warn for Skip without OrderBy.
            // However, when querying on JSON arrays within documents, the order of elements is guaranteed, and Skip without OrderBy is
            // fine. Since subqueries must be correlated (i.e. reference an array in the outer query), we use that to decide whether to
            // warn or not.
            if (selectExpression.Orderings.Count == 0 && !_subquery)
            {
                _queryCompilationContext.Logger.RowLimitingOperationWithoutOrderByWarning();
            }

            selectExpression.ApplyOffset(translation);

            return source;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateSkipWhile(ShapedQueryExpression source, LambdaExpression predicate)
        => null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateSum(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        if (selectExpression.IsDistinct
            || selectExpression.Limit != null
            || selectExpression.Offset != null)
        {
            return null;
        }

        if (selector != null)
        {
            source = TranslateSelect(source, selector);
        }

        var serverOutputType = resultType.UnwrapNullableType();
        var projection = (SqlExpression)selectExpression.GetMappedProjection(new ProjectionMember());

        projection = _sqlExpressionFactory.Function("SUM", new[] { projection }, serverOutputType, projection.TypeMapping);

        return AggregateResultShaper(source, projection, throwOnNullResult: false, resultType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateTake(ShapedQueryExpression source, Expression count)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        var translation = TranslateExpression(count);

        if (translation != null)
        {
            // Ordering of documents is not guaranteed in Cosmos, so we warn for Take without OrderBy.
            // However, when querying on JSON arrays within documents, the order of elements is guaranteed, and Take without OrderBy is
            // fine. Since subqueries must be correlated (i.e. reference an array in the outer query), we use that to decide whether to
            // warn or not.
            if (selectExpression.Orderings.Count == 0 && !_subquery)
            {
                _queryCompilationContext.Logger.RowLimitingOperationWithoutOrderByWarning();
            }

            selectExpression.ApplyLimit(translation);

            return source;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateTakeWhile(ShapedQueryExpression source, LambdaExpression predicate)
        => null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateThenBy(ShapedQueryExpression source, LambdaExpression keySelector, bool ascending)
    {
        if (TranslateLambdaExpression(source, keySelector) is SqlExpression translation)
        {
            ((SelectExpression)source.QueryExpression).AppendOrdering(new OrderingExpression(translation, ascending));

            return source;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateUnion(ShapedQueryExpression source1, ShapedQueryExpression source2)
        => TranslateSetOperation(source1, source2, "SetUnion", ignoreOrderings: true);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateWhere(ShapedQueryExpression source, LambdaExpression predicate)
    {
        if (source.ShaperExpression is StructuralTypeShaperExpression { StructuralType: IEntityType entityType } entityShaperExpression
            && entityType.GetPartitionKeyPropertyNames().FirstOrDefault() != null)
        {
            List<(Expression Expression, IProperty Property)?> partitionKeyValues = new();
            if (TryExtractPartitionKey(predicate.Body, entityType, out var newPredicate, partitionKeyValues))
            {
                foreach (var propertyName in entityType.GetPartitionKeyPropertyNames())
                {
                    var partitionKeyValue = partitionKeyValues.FirstOrDefault(p => p!.Value.Property.Name == propertyName);
                    if (partitionKeyValue == null)
                    {
                        newPredicate = null;
                        break;
                    }

                    ((SelectExpression)source.QueryExpression).AddPartitionKey(
                        partitionKeyValue.Value.Property, partitionKeyValue.Value.Expression);
                }

                if (newPredicate == null)
                {
                    return source;
                }

                predicate = Expression.Lambda(newPredicate, predicate.Parameters);
            }
        }

        if (TranslateLambdaExpression(source, predicate) is SqlExpression translation)
        {
            ((SelectExpression)source.QueryExpression).ApplyPredicate(translation);

            return source;
        }

        return null;

        bool TryExtractPartitionKey(
            Expression expression,
            IEntityType entityType,
            out Expression? updatedPredicate,
            List<(Expression, IProperty)?> partitionKeyValues)
        {
            updatedPredicate = null;
            if (expression is BinaryExpression binaryExpression)
            {
                if (TryGetPartitionKeyValue(binaryExpression, entityType, out var valueExpression, out var property))
                {
                    partitionKeyValues.Add((valueExpression!, property!));
                    return true;
                }

                if (binaryExpression.NodeType == ExpressionType.AndAlso)
                {
                    var foundInRight = TryExtractPartitionKey(binaryExpression.Left, entityType, out var leftPredicate, partitionKeyValues);

                    var foundInLeft = TryExtractPartitionKey(
                        binaryExpression.Right,
                        entityType,
                        out var rightPredicate,
                        partitionKeyValues);

                    if (foundInLeft && foundInRight)
                    {
                        return true;
                    }

                    if (foundInLeft || foundInRight)
                    {
                        updatedPredicate = leftPredicate != null
                            ? rightPredicate != null
                                ? binaryExpression.Update(leftPredicate, binaryExpression.Conversion, rightPredicate)
                                : leftPredicate
                            : rightPredicate;

                        return true;
                    }
                }
            }
            else if (expression.NodeType == ExpressionType.MemberAccess
                     && expression.Type == typeof(bool))
            {
                if (IsPartitionKeyPropertyAccess(expression, entityType, out var property))
                {
                    partitionKeyValues.Add((Expression.Constant(true), property!));
                    return true;
                }
            }
            else if (expression.NodeType == ExpressionType.Not)
            {
                if (IsPartitionKeyPropertyAccess(((UnaryExpression)expression).Operand, entityType, out var property))
                {
                    partitionKeyValues.Add((Expression.Constant(false), property!));
                    return true;
                }
            }

            updatedPredicate = expression;
            return false;
        }

        bool TryGetPartitionKeyValue(
            BinaryExpression binaryExpression,
            IEntityType entityType,
            out Expression? expression,
            out IProperty? property)
        {
            if (binaryExpression.NodeType == ExpressionType.Equal)
            {
                expression = IsPartitionKeyPropertyAccess(binaryExpression.Left, entityType, out property)
                    ? binaryExpression.Right
                    : IsPartitionKeyPropertyAccess(binaryExpression.Right, entityType, out property)
                        ? binaryExpression.Left
                        : null;

                if (expression is ConstantExpression
                    || (expression is ParameterExpression valueParameterExpression
                        && valueParameterExpression.Name?
                            .StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal)
                        == true))
                {
                    return true;
                }
            }

            expression = null;
            property = null;
            return false;
        }

        bool IsPartitionKeyPropertyAccess(Expression expression, IEntityType entityType, out IProperty? property)
        {
            property = expression switch
            {
                MemberExpression memberExpression
                    => entityType.FindProperty(memberExpression.Member.GetSimpleMemberName()),
                MethodCallExpression methodCallExpression when methodCallExpression.TryGetEFPropertyArguments(out _, out var propertyName)
                    => entityType.FindProperty(propertyName),
                MethodCallExpression methodCallExpression
                    when methodCallExpression.TryGetIndexerArguments(_queryCompilationContext.Model, out _, out var propertyName)
                    => entityType.FindProperty(propertyName),
                _ => null
            };

            return property != null && entityType.GetPartitionKeyPropertyNames().Contains(property.Name);
        }
    }

    #region Queryable collection support

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateMemberAccess(Expression source, MemberIdentity member)
    {
        // TODO: the below immediately wraps the JSON array property in a subquery (SELECT VALUE i FROM i IN c.Array).
        // TODO: This isn't strictly necessary, as c.Array can be referenced directly; however, that would mean producing a
        // TODO: ShapedQueryExpression that doesn't wrap a SelectExpression, but rather a KeyAccessExpression directly; this isn't currently
        // TODO: supported.

        // Attempt to translate access into a primitive collection property
        if (_sqlTranslator.TryBindMember(_sqlTranslator.Visit(source), member, out var translatedExpression, out var property)
            && property is IProperty { IsPrimitiveCollection: true }
            && translatedExpression is SqlExpression sqlExpression
            && WrapPrimitiveCollectionAsShapedQuery(
                sqlExpression,
                sqlExpression.Type.GetSequenceType(),
                sqlExpression.TypeMapping!.ElementTypeMapping!) is { } primitiveCollectionTranslation)
        {
            return primitiveCollectionTranslation;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateInlineQueryRoot(InlineQueryRootExpression inlineQueryRootExpression)
    {
        // The below produces an InlineArrayExpression ([1,2,3]), wrapped by a SelectExpression (SELECT VALUE [1,2,3]).
        // This is because a bare inline array can only appear in the projection. For example, the following is wrong:
        // SELECT i FROM i IN [1,2,3] (syntax error)
        var values = inlineQueryRootExpression.Values;
        var translatedItems = new SqlExpression[values.Count];

        for (var i = 0; i < values.Count; i++)
        {
            if (TranslateExpression(values[i]) is not SqlExpression translatedItem)
            {
                return null;
            }

            translatedItems[i] = translatedItem;
        }

        // TODO: Do we need full-on type mapping inference like in relational?
        // TODO: The following currently just gets the type mapping from the CLR type, which ignores e.g. value converters on
        // TODO: properties compared against
        var elementClrType = inlineQueryRootExpression.ElementType;
        var elementTypeMapping = _typeMappingSource.FindMapping(elementClrType)!;
        var arrayTypeMapping = _typeMappingSource.FindMapping(elementClrType.MakeArrayType()); // TODO: IEnumerable?
        var inlineArray = new ArrayConstantExpression(elementClrType, translatedItems, arrayTypeMapping);

        // Unfortunately, Cosmos doesn't support selecting directly from an inline array: SELECT i FROM i IN [1,2,3] (syntax error)
        // We must wrap the inline array in a subquery: SELECT VALUE i FROM (SELECT VALUE [1,2,3])
        var innerSelect = new SelectExpression(
            [new ProjectionExpression(inlineArray, null!)],
            sources: [],
            orderings: [])
        {
            UsesSingleValueProjection = true
        };

        return WrapPrimitiveCollectionAsShapedQuery(innerSelect, elementClrType, elementTypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateParameterQueryRoot(ParameterQueryRootExpression parameterQueryRootExpression)
    {
        if (parameterQueryRootExpression.ParameterExpression.Name?.StartsWith(
                QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal)
            != true)
        {
            return null;
        }

        // TODO: Do we need full-on type mapping inference like in relational?
        // TODO: The following currently just gets the type mapping from the CLR type, which ignores e.g. value converters on
        // TODO: properties compared against
        var elementClrType = parameterQueryRootExpression.ElementType;
        var arrayTypeMapping = _typeMappingSource.FindMapping(elementClrType.MakeArrayType()); // TODO: IEnumerable?
        var elementTypeMapping = _typeMappingSource.FindMapping(elementClrType)!;
        var sqlParameterExpression = new SqlParameterExpression(parameterQueryRootExpression.ParameterExpression, arrayTypeMapping);

        // Unfortunately, Cosmos doesn't support selecting directly from an inline array: SELECT i FROM i IN [1,2,3] (syntax error)
        // We must wrap the inline array in a subquery: SELECT VALUE i FROM (SELECT VALUE [1,2,3])
        var innerSelect = new SelectExpression(
            [new ProjectionExpression(sqlParameterExpression, null!)],
            sources: [],
            orderings: [])
        {
            UsesSingleValueProjection = true
        };

        return WrapPrimitiveCollectionAsShapedQuery(innerSelect, elementClrType, elementTypeMapping);
    }

    private ShapedQueryExpression WrapPrimitiveCollectionAsShapedQuery(
        Expression containerExpression,
        Type elementClrType,
        CoreTypeMapping elementTypeMapping)
    {
        // TODO: Do proper alias management: #33894
        var selectExpression = SelectExpression.CreateForPrimitiveCollection(
            new SourceExpression(containerExpression, "i", withIn: true),
            elementClrType,
            elementTypeMapping);
        var shaperExpression = (Expression)new ProjectionBindingExpression(
            selectExpression, new ProjectionMember(), elementClrType.MakeNullable());
        if (shaperExpression.Type != elementClrType)
        {
            Check.DebugAssert(
                elementClrType.MakeNullable() == shaperExpression.Type,
                "expression.Type must be nullable of targetType");

            shaperExpression = Expression.Convert(shaperExpression, elementClrType);
        }

        return new ShapedQueryExpression(selectExpression, shaperExpression);
    }

    #endregion Queryable collection support

    private ShapedQueryExpression? TranslateSetOperation(
        ShapedQueryExpression source1,
        ShapedQueryExpression source2,
        string functionName,
        bool ignoreOrderings = false)
    {
        if (CosmosQueryUtils.TryConvertToArray(source1, _typeMappingSource, out var array1, out var projection1, ignoreOrderings)
            && CosmosQueryUtils.TryConvertToArray(source2, _typeMappingSource, out var array2, out var projection2, ignoreOrderings)
            && projection1.Type == projection2.Type
            && (projection1.TypeMapping ?? projection2.TypeMapping) is CoreTypeMapping typeMapping)
        {
            var translation = _sqlExpressionFactory.Function(functionName, [array1, array2], projection1.Type, typeMapping);
            var select = SelectExpression.CreateForPrimitiveCollection(
                new SourceExpression(translation, "i", withIn: true),
                projection1.Type,
                typeMapping);
            return source1.UpdateQueryExpression(select);
        }

        // TODO: can also handle subqueries via ARRAY()
        return null;
    }

    private SqlExpression? TranslateExpression(Expression expression)
    {
        var translation = _sqlTranslator.Translate(expression);
        if (translation == null && _sqlTranslator.TranslationErrorDetails != null)
        {
            AddTranslationErrorDetails(_sqlTranslator.TranslationErrorDetails);
        }

        return translation;
    }

    private SqlExpression? TranslateLambdaExpression(
        ShapedQueryExpression shapedQueryExpression,
        LambdaExpression lambdaExpression)
    {
        var lambdaBody = RemapLambdaBody(shapedQueryExpression.ShaperExpression, lambdaExpression);

        return TranslateExpression(lambdaBody);
    }

    private static Expression RemapLambdaBody(Expression shaperBody, LambdaExpression lambdaExpression)
        => ReplacingExpressionVisitor.Replace(lambdaExpression.Parameters.Single(), shaperBody, lambdaExpression.Body);

    private static ShapedQueryExpression AggregateResultShaper(
        ShapedQueryExpression source,
        Expression projection,
        bool throwOnNullResult,
        Type resultType)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        selectExpression.ReplaceProjectionMapping(new Dictionary<ProjectionMember, Expression> { { new ProjectionMember(), projection } });

        selectExpression.ClearOrdering();

        var nullableResultType = resultType.MakeNullable();
        Expression shaper = new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), nullableResultType);

        if (throwOnNullResult)
        {
            var resultVariable = Expression.Variable(nullableResultType, "result");
            var returnValueForNull = resultType.IsNullableType()
                ? (Expression)Expression.Constant(null, resultType)
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
        else if (resultType != shaper.Type)
        {
            shaper = Expression.Convert(shaper, resultType);
        }

        return source.UpdateShaperExpression(shaper);
    }
}
