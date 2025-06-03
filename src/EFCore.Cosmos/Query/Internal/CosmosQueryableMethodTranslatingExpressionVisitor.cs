// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Internal;

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
    private readonly CosmosAliasManager _aliasManager;
    private bool _subquery;

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
            new CosmosProjectionBindingExpressionVisitor(_queryCompilationContext.Model, this, _sqlTranslator, _typeMappingSource);
        _aliasManager = queryCompilationContext.AliasManager;
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
            new CosmosProjectionBindingExpressionVisitor(_queryCompilationContext.Model, this, _sqlTranslator, _typeMappingSource);
        _aliasManager = parentVisitor._aliasManager;
        _subquery = true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression Translate(Expression expression)
    {
        // Handle ToPageAsync(), which can only ever be the top-level node in the query tree.
        if (expression is MethodCallExpression { Method: var method, Arguments: var arguments }
            && method.DeclaringType == typeof(CosmosQueryableExtensions)
            && method.Name is nameof(CosmosQueryableExtensions.ToPageAsync))
        {
            if (_subquery)
            {
                AddTranslationErrorDetails(CosmosStrings.ToPageAsyncAtTopLevelOnly);
                return QueryCompilationContext.NotTranslatedExpression;
            }

            var source = base.Translate(arguments[0]);

            if (source == QueryCompilationContext.NotTranslatedExpression)
            {
                return source;
            }

            if (source is not ShapedQueryExpression shapedQuery)
            {
                throw new UnreachableException($"Expected a ShapedQueryExpression but found {source.GetType().Name}");
            }

            // The arguments to ToPage/ToPageAsync must have been parameterized by the funcletizer, since they're non-lambda arguments to
            // a top-level function (like Skip/Take). Translate to get these as SqlParameterExpressions.
            if (arguments is not
                [
                    _, // source
                    QueryParameterExpression maxItemCount,
                    QueryParameterExpression continuationToken,
                    QueryParameterExpression responseContinuationTokenLimitInKb,
                    _ // cancellation token
                ]
                || _sqlTranslator.Translate(maxItemCount) is not SqlParameterExpression translatedMaxItemCount
                || _sqlTranslator.Translate(continuationToken) is not SqlParameterExpression translatedContinuationToken
                || _sqlTranslator.Translate(responseContinuationTokenLimitInKb) is not SqlParameterExpression
                    translatedResponseContinuationTokenLimitInKb)
            {
                throw new UnreachableException("ToPageAsync without the appropriate parameterized arguments");
            }

            // Wrap the shaper for the entire query in a PagingExpression which also contains the paging arguments, and update
            // the final cardinality to Single (since we'll be returning a single Page).
            return shapedQuery
                .UpdateShaperExpression(
                    new PagingExpression(
                        shapedQuery.ShaperExpression,
                        translatedMaxItemCount,
                        translatedContinuationToken,
                        translatedResponseContinuationTokenLimitInKb,
                        typeof(CosmosPage<>).MakeGenericType(shapedQuery.ShaperExpression.Type)))
                .UpdateResultCardinality(ResultCardinality.Single);
        }

        return base.Translate(expression);
    }

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        var method = methodCallExpression.Method;

        if (methodCallExpression.Method.DeclaringType == typeof(CosmosQueryableExtensions)
            && methodCallExpression.Method.Name == nameof(CosmosQueryableExtensions.WithPartitionKey))
        {
            if (_queryCompilationContext.PartitionKeyPropertyValues.Count > 0)
            {
                throw new InvalidOperationException(CosmosStrings.WithPartitionKeyAlreadyCalled);
            }

            if (methodCallExpression.Arguments[0] is not EntityQueryRootExpression)
            {
                throw new InvalidOperationException(CosmosStrings.WithPartitionKeyBadNode);
            }

            var innerQueryable = Visit(methodCallExpression.Arguments[0]);

            for (var i = 1; i < methodCallExpression.Arguments.Count; i++)
            {
                var value = _sqlTranslator.Translate(methodCallExpression.Arguments[i], applyDefaultTypeMapping: false);
                if (value is not SqlConstantExpression and not SqlParameterExpression)
                {
                    throw new InvalidOperationException(CosmosStrings.WithPartitionKeyNotConstantOrParameter);
                }

                _queryCompilationContext.PartitionKeyPropertyValues.Add(value);
            }

            return innerQueryable;
        }

        if (method.DeclaringType == typeof(Queryable) && method.IsGenericMethod)
        {
            switch (methodCallExpression.Method.Name)
            {
                // The following is a bad hack to account for https://github.com/dotnet/efcore/issues/32957#issuecomment-2165864086.
                // Basically for the query form Where(b => b.Posts.GetElementAt(0).Id == 1), nav expansion moves the property access
                // forward, generating Where(b => b.Posts.Select(p => p.Id).GetElementAt(0)); unfortunately that means that GetElementAt()
                // over a bare array in Cosmos doesn't get translated to a simple indexer as it should (b["Posts"][0].Id), since the
                // reordering messes things up.
                case nameof(Queryable.ElementAt) or nameof(Queryable.ElementAtOrDefault)
                    when methodCallExpression.Arguments[0] is MethodCallExpression
                    {
                        Method: { Name: "Select", IsGenericMethod: true }
                    } innerMethodCall
                    && method.GetGenericMethodDefinition() is var genericDefinition
                    && (genericDefinition == QueryableMethods.ElementAt || genericDefinition == QueryableMethods.ElementAtOrDefault)
                    && innerMethodCall.Method.GetGenericMethodDefinition() == QueryableMethods.Select:
                {
                    var returnDefault = method.Name == nameof(Queryable.ElementAtOrDefault);
                    if (Visit(innerMethodCall) is ShapedQueryExpression translatedSelect
                        && translatedSelect.TryExtractArray(out _, out _, out _, out var boundMember)
                        && boundMember is IAccessExpression { PropertyName: string boundPropertyName }
                        && Visit(innerMethodCall.Arguments[0]) is ShapedQueryExpression innerSource
                        && TranslateElementAtOrDefault(
                            innerSource, methodCallExpression.Arguments[1], returnDefault) is ShapedQueryExpression elementAtTranslation)
                    {
#pragma warning disable EF1001 // Internal EF Core API usage.
                        var translation = _sqlTranslator.Translate(
                            EntityFrameworkCore.Infrastructure.ExpressionExtensions.CreateEFPropertyExpression(
                                elementAtTranslation.ShaperExpression,
                                elementAtTranslation.ShaperExpression.Type,
                                boundMember.Type,
                                boundPropertyName,
                                makeNullable: true));
#pragma warning restore EF1001 // Internal EF Core API usage.

                        if (translation is not null)
                        {
                            var finalShapedQuery = CreateShapedQueryExpression(new SelectExpression(translation), boundMember.Type);
                            return finalShapedQuery.UpdateResultCardinality(
                                returnDefault ? ResultCardinality.SingleOrDefault : ResultCardinality.Single);
                        }
                    }

                    break;
                }
            }
        }

        return base.VisitMethodCall(methodCallExpression);
    }

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        switch (extensionExpression)
        {
            case EntityQueryRootExpression when _subquery:
                AddTranslationErrorDetails(CosmosStrings.NonCorrelatedSubqueriesNotSupported);
                return QueryCompilationContext.NotTranslatedExpression;

            case FromSqlQueryRootExpression fromSqlQueryRoot:
                var entityType = fromSqlQueryRoot.EntityType;
                var fromSql = new FromSqlExpression(entityType.ClrType, fromSqlQueryRoot.Sql, fromSqlQueryRoot.Argument);
                var alias = _aliasManager.GenerateSourceAlias(fromSql);
                var selectExpression = new SelectExpression(
                    new SourceExpression(fromSql, alias),
                    new EntityProjectionExpression(new ObjectReferenceExpression(entityType, alias), entityType));
                return CreateShapedQueryExpression(entityType, selectExpression) ?? QueryCompilationContext.NotTranslatedExpression;

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
    protected override ShapedQueryExpression? CreateShapedQueryExpression(IEntityType entityType)
    {
        Check.DebugAssert(!entityType.IsOwned(), "Can't create ShapedQueryExpression for owned entity type");

        var alias = _aliasManager.GenerateSourceAlias("c");
        var selectExpression = new SelectExpression(
            new SourceExpression(new ObjectReferenceExpression(entityType, "root"), alias),
            new EntityProjectionExpression(new ObjectReferenceExpression(entityType, alias), entityType));

        // Add discriminator predicate
        var concreteEntityTypes = entityType.GetConcreteDerivedTypesInclusive().ToList();
        if (concreteEntityTypes is [var singleEntityType]
            && singleEntityType.GetIsDiscriminatorMappingComplete()
            && entityType.GetContainer() is var container
            && !entityType.Model.GetEntityTypes().Any(
                // If a read-only/view type is mapped to the same container with the same discriminator, then we still don't need
                // the discriminator, allowing ReadItem in more places.
                e => e.GetContainer() == container && !Equals(e.GetDiscriminatorValue(), singleEntityType.GetDiscriminatorValue())))
        {
            // There's a single entity type mapped to the container and the discriminator mapping is complete; we can skip the
            // discriminator predicate.
        }
        else
        {
            var discriminatorProperty = concreteEntityTypes[0].FindDiscriminatorProperty();
            Check.DebugAssert(
                discriminatorProperty is not null || concreteEntityTypes.Count == 1,
                "Missing discriminator property in hierarchy");
            if (discriminatorProperty is not null)
            {
                var discriminatorColumn = ((EntityProjectionExpression)selectExpression.GetMappedProjection(new ProjectionMember()))
                    .BindProperty(discriminatorProperty, clientEval: false);

                var success = TryApplyPredicate(
                    selectExpression,
                    _sqlExpressionFactory.In(
                        (SqlExpression)discriminatorColumn,
                        concreteEntityTypes.Select(
                                et => _sqlExpressionFactory.Constant(et.GetDiscriminatorValue(), discriminatorColumn.Type))
                            .ToArray()));
                Check.DebugAssert(success, "Couldn't apply predicate when creating a new ShapedQueryExpression");
            }
        }

        return CreateShapedQueryExpression(entityType, selectExpression);
    }

    private ShapedQueryExpression? CreateShapedQueryExpression(IEntityType entityType, SelectExpression queryExpression)
    {
        if (!entityType.IsOwned())
        {
            var existingEntityType = _queryCompilationContext.RootEntityType;
            if (existingEntityType is not null && existingEntityType != entityType)
            {
                AddTranslationErrorDetails(
                    CosmosStrings.MultipleRootEntityTypesReferencedInQuery(entityType.DisplayName(), existingEntityType.DisplayName()));
                return null;
            }

            _queryCompilationContext.RootEntityType = entityType;
        }

        return new ShapedQueryExpression(
            queryExpression,
            new StructuralTypeShaperExpression(
                entityType,
                new ProjectionBindingExpression(queryExpression, new ProjectionMember(), typeof(ValueBuffer)),
                nullable: false));
    }

    private ShapedQueryExpression CreateShapedQueryExpression(SelectExpression select, Type elementClrType)
    {
        var shaperExpression = (Expression)new ProjectionBindingExpression(
            select, new ProjectionMember(), elementClrType.MakeNullable());
        if (shaperExpression.Type != elementClrType)
        {
            Check.DebugAssert(
                elementClrType.MakeNullable() == shaperExpression.Type,
                "expression.Type must be nullable of targetType");

            shaperExpression = Expression.Convert(shaperExpression, elementClrType);
        }

        return new ShapedQueryExpression(select, shaperExpression);
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

        // Simplify x.Array.Any() => ARRAY_LENGTH(x.Array) > 0 instead of (EXISTS(SELECT 1 FROM i IN x.Array))
        if (source.TryExtractArray(out var array, ignoreOrderings: true))
        {
            var simplifiedTranslation = _sqlExpressionFactory.GreaterThan(
                _sqlExpressionFactory.Function(
                    "ARRAY_LENGTH", new[] { array }, typeof(int), _typeMappingSource.FindMapping(typeof(int))),
                _sqlExpressionFactory.Constant(0));
            var select = new SelectExpression(simplifiedTranslation);

            return source.Update(select, new ProjectionBindingExpression(select, new ProjectionMember(), typeof(int)));
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
        => TranslateAggregate(source, selector, resultType, "AVG");

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
        if (source.TryExtractArray(out var array, ignoreOrderings: true)
            && array is SqlExpression scalarArray // TODO: Contains over arrays of structural types, #34027
            && TranslateExpression(item) is SqlExpression translatedItem)
        {
            if (array is ArrayConstantExpression arrayConstant)
            {
                var inExpression = _sqlExpressionFactory.In(translatedItem, arrayConstant.Items);
                return source.Update(new SelectExpression(inExpression), source.ShaperExpression);
            }

            (translatedItem, scalarArray) = _sqlExpressionFactory.ApplyTypeMappingsOnItemAndArray(translatedItem, scalarArray);
            var simplifiedTranslation = _sqlExpressionFactory.Function("ARRAY_CONTAINS", [scalarArray, translatedItem], typeof(bool));
            return source.UpdateQueryExpression(new SelectExpression(simplifiedTranslation));
        }

        // Translate to EXISTS
        var anyLambdaParameter = Expression.Parameter(item.Type, "p");
        var anyLambda = Expression.Lambda(
            EntityFrameworkCore.Infrastructure.ExpressionExtensions.CreateEqualsExpression(anyLambdaParameter, item),
            anyLambdaParameter);

        return TranslateAny(source, anyLambda);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateCount(ShapedQueryExpression source, LambdaExpression? predicate)
        => TranslateCountLongCount(source, predicate, typeof(int));

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
    protected override ShapedQueryExpression? TranslateDistinct(ShapedQueryExpression source)
    {
        var select = (SelectExpression)source.QueryExpression;

        if ((select.Limit is not null || select.Offset is not null)
            && !TryPushdownIntoSubquery(select))
        {
            return null;
        }

        select.ApplyDistinct();

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
        if (TranslateExpression(index) is not SqlExpression translatedIndex)
        {
            return null;
        }

        var select = (SelectExpression)source.QueryExpression;

        // If the source query represents a bare array (e.g. x.Array), simplify x.Array.Skip(2) => ARRAY_SLICE(x.Array, 2) instead of
        // subquery+OFFSET (which isn't supported by Cosmos).
        // Even if the source is a full query (not a bare array), convert it to an array via the Cosmos ARRAY() operator; we do this
        // only in subqueries, because Cosmos supports OFFSET/LIMIT at the top-level but not in subqueries.
        var array = source.TryExtractArray(out var a, out var projection, out var projectedStructuralTypeShaper, out _)
            ? a
            : _subquery && source.TryConvertToArray(_typeMappingSource, out a, out projection)
                ? a
                : null;

        // Simplify x.Array[1] => x.Array[1] (using the Cosmos array subscript operator) instead of a subquery with LIMIT/OFFSET
        switch (array)
        {
            // ElementAtOrDefault over an array of scalars
            case SqlExpression scalarArray when projection is SqlExpression element:
            {
                var translation = _sqlExpressionFactory.ArrayIndex(
                    scalarArray, translatedIndex, element.Type, element.TypeMapping);

                // ElementAt may access indexes beyond the end of the array; Cosmos returns undefined for those cases.
                // If ElementAtOrDefault is used, add the Cosmos undefined-coalescing operator (??) to return a default value instead.
                if (returnDefault)
                {
                    translation = _sqlExpressionFactory.CoalesceUndefined(
                        translation, TranslateExpression(translation.Type.GetDefaultValueConstant())!);
                }

                var translatedSelect = new SelectExpression(translation);
                return source.Update(
                    translatedSelect,
                    new ProjectionBindingExpression(translatedSelect, new ProjectionMember(), element.Type));
            }

            // ElementAtOrDefault over an array of structural types
            case not null when projectedStructuralTypeShaper is not null:
            {
                Expression translation = new ObjectArrayIndexExpression(array, translatedIndex, projectedStructuralTypeShaper.Type);

                // ElementAt may access indexes beyond the end of the array; Cosmos returns undefined for those cases.
                // If ElementAtOrDefault is used, add the Cosmos undefined-coalescing operator (??) to return a default value instead.
                if (returnDefault)
                {
                    // TODO: The following uses SqlConstantExpression as a hack to produce a null for the structural type (#33999)
                    translation = new ObjectBinaryExpression(
                        ExpressionType.Coalesce,
                        translation,
                        new SqlConstantExpression(null, typeof(object), _typeMappingSource.FindMapping(typeof(int))),
                        translation.Type);
                }

                var translatedSelect =
                    new SelectExpression(
                        new EntityProjectionExpression(translation, (IEntityType)projectedStructuralTypeShaper.StructuralType));
                return source.Update(
                    translatedSelect,
                    new StructuralTypeShaperExpression(
                        projectedStructuralTypeShaper.StructuralType,
                        new ProjectionBindingExpression(translatedSelect, new ProjectionMember(), typeof(ValueBuffer)),
                        nullable: true));
            }
        }

        // Simplification to indexing failed, translate using OFFSET/LIMIT, except in subqueries where it isn't supported.
        if (_subquery)
        {
            AddTranslationErrorDetails(CosmosStrings.LimitOffsetNotSupportedInSubqueries);
            return null;
        }

        // Ordering of documents is not guaranteed in Cosmos, so we warn for Take without OrderBy.
        // However, when querying on JSON arrays within documents, the order of elements is guaranteed, and Take without OrderBy is
        // fine. Since subqueries must be correlated (i.e. reference an array in the outer query), we use that to decide whether to
        // warn or not.
        if (select.Orderings.Count == 0 && !_subquery)
        {
            _queryCompilationContext.Logger.RowLimitingOperationWithoutOrderByWarning();
        }

        if (!TryApplyOffset(select, translatedIndex)
            || !TryApplyLimit(select, TranslateExpression(Expression.Constant(1))!))
        {
            return null;
        }

        // TODO: ElementAt on top level
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

        // Cosmos does not support LIMIT in subqueries, so call into TranslateElementAtOrDefault which knows how to either extract an
        // array from the source or wrap it in a Cosmos ARRAY() operator, to turn it into an array. At that point, a regular array index
        // (x.Array[0]) can be used to get the first element.
        if (_subquery)
        {
            return TranslateElementAtOrDefault(source, Expression.Constant(0), returnDefault);
        }

        var select = (SelectExpression)source.QueryExpression;

        if (!TryApplyLimit(select, TranslateExpression(Expression.Constant(1))!))
        {
            return null;
        }

        if (select is { Orderings: [], Predicate: null, ReadItemInfo: null })
        {
            _queryCompilationContext.Logger.FirstWithoutOrderByAndFilterWarning();
        }

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
    {
        AddTranslationErrorDetails(CosmosStrings.CrossDocumentJoinNotSupported);
        return null;
    }

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

        var select = (SelectExpression)source.QueryExpression;
        select.ReverseOrderings();

        if (!TryApplyLimit(select, TranslateExpression(Expression.Constant(1))!))
        {
            return null;
        }

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
    {
        AddTranslationErrorDetails(CosmosStrings.CrossDocumentJoinNotSupported);
        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateRightJoin(
        ShapedQueryExpression outer,
        ShapedQueryExpression inner,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector)
    {
        AddTranslationErrorDetails(CosmosStrings.CrossDocumentJoinNotSupported);
        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateLongCount(ShapedQueryExpression source, LambdaExpression? predicate)
        => TranslateCountLongCount(source, predicate, typeof(long));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateMax(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
        => TranslateAggregate(source, selector, resultType, "MAX");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateMin(ShapedQueryExpression source, LambdaExpression? selector, Type resultType)
        => TranslateAggregate(source, selector, resultType, "MIN");

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

        var select = (SelectExpression)source.QueryExpression;

        var parameterExpression = Expression.Parameter(entityShaperExpression.Type);
        var predicate = Expression.Lambda(Expression.TypeIs(parameterExpression, resultType), parameterExpression);

        if (!TryApplyPredicate(source, predicate))
        {
            return null;
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

        var entityProjectionExpression = (EntityProjectionExpression)select.GetMappedProjection(projectionMember);
        select.ReplaceProjectionMapping(
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
        var select = (SelectExpression)source.QueryExpression;

        if ((select.IsDistinct || select.Limit is not null || select.Offset is not null)
            && !TryPushdownIntoSubquery(select))
        {
            return null;
        }

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

        var newSelectorBody = RemapLambdaBody(source, selector);

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
    {
        var collectionSelectorBody = RemapLambdaBody(source, collectionSelector);

        // The collection selector gets translated in subquery context; specifically, if an uncorrelated SelectMany() is attempted
        // (from b in context.Blogs from p in context.Posts...), we want to detect that and fail translation as an uncorrelated query
        // (see VisitExtension visitation for EntityQueryRootExpression)
        var previousSubquery = _subquery;
        _subquery = true;
        try
        {
            if (Visit(collectionSelectorBody) is ShapedQueryExpression inner)
            {
                var select = (SelectExpression)source.QueryExpression;
                var shaper = select.AddJoin(inner, source.ShaperExpression, _aliasManager);

                return TranslateTwoParameterSelector(source.UpdateShaperExpression(shaper), resultSelector);
            }

            return null;
        }
        finally
        {
            _subquery = previousSubquery;
        }
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSelectMany(ShapedQueryExpression source, LambdaExpression selector)
    {
        // TODO: Note that we currently never actually seem to get SelectMany without a result selector, because nav expansion rewrites
        // that to a more complex variant with a result selector (see https://github.com/dotnet/efcore/issues/32957#issuecomment-2170950767)
        // blogs.SelectMany(c => c.Ints) becomes:
        // blogs
        //     .SelectMany(p => Property(p, "Ints").AsQueryable(), (p, c) => new TransparentIdentifier`2(Outer = p, Inner = c))
        //     .Select(ti => ti.Inner)

        // TODO: In Cosmos, we currently always add a predicate for the discriminator (unless HasNoDiscriminator is explicitly specified),
        // so the source is almost never a bare array.
        // If we stop doing that (see #34005, #20268), and we remove the result selector problem (see just above), we should check if the
        // source is a bare array, and simply return the ShapedQueryExpression returned from visiting the collection selector. This would
        // remove the extra unneeded JOIN we'd currently generate.
        var innerParameter = Expression.Parameter(selector.ReturnType.GetSequenceType(), "i");
        var resultSelector = Expression.Lambda(
            innerParameter, Expression.Parameter(source.Type.GetSequenceType()), innerParameter);

        return TranslateSelectMany(source, selector, resultSelector);
    }

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

        // Cosmos does not support LIMIT in subqueries, so call into TranslateElementAtOrDefault which knows how to either extract an
        // array from the source or wrap it in a Cosmos ARRAY() operator, to turn it into an array. At that point, a regular array index
        // (x.Array[0]) can be used to get the first element.
        if (_subquery)
        {
            return TranslateElementAtOrDefault(source, Expression.Constant(0), returnDefault);
        }

        var select = (SelectExpression)source.QueryExpression;
        if (!TryApplyLimit(select, TranslateExpression(Expression.Constant(2))!))
        {
            return null;
        }

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
        if (TranslateExpression(count) is not SqlExpression translatedCount)
        {
            return null;
        }

        var select = (SelectExpression)source.QueryExpression;

        // If the source query represents a bare array (e.g. x.Array), simplify x.Array.Skip(2) => ARRAY_SLICE(x.Array, 2) instead of
        // subquery+OFFSET (which isn't supported by Cosmos).
        // Even if the source is a full query (not a bare array), convert it to an array via the Cosmos ARRAY() operator; we do this
        // only in subqueries, because Cosmos supports OFFSET/LIMIT at the top-level but not in subqueries.
        var array = source.TryExtractArray(out var a, out var projection, out var projectedStructuralTypeShaper, out _)
            ? a
            : _subquery && source.TryConvertToArray(_typeMappingSource, out a, out projection)
                ? a
                : null;

        switch (array)
        {
            // ElementAtOrDefault over an array of scalars
            case SqlExpression scalarArray when projection is SqlExpression element:
            {
                var arrayType = typeof(IEnumerable<>).MakeGenericType(projection.Type);
                var arrayTypeMapping = _typeMappingSource.FindMapping(arrayType, _queryCompilationContext.Model, element.TypeMapping);

                var slice = _sqlExpressionFactory.Function("ARRAY_SLICE", [scalarArray, translatedCount], arrayType, arrayTypeMapping);

                var alias = _aliasManager.GenerateSourceAlias(slice);
                var translatedSelect = SelectExpression.CreateForCollection(
                    slice,
                    alias,
                    new ScalarReferenceExpression(alias, element.Type, element.TypeMapping));
                return source.UpdateQueryExpression(translatedSelect);
            }

            // ElementAtOrDefault over an array os structural types
            case not null when projectedStructuralTypeShaper is not null:
            {
                var arrayType = typeof(IEnumerable<>).MakeGenericType(projectedStructuralTypeShaper.Type);
                var slice = new ObjectFunctionExpression("ARRAY_SLICE", [array, translatedCount], arrayType);
                var alias = _aliasManager.GenerateSourceAlias(slice);
                var translatedSelect = SelectExpression.CreateForCollection(
                    slice,
                    alias,
                    new EntityProjectionExpression(
                        new ObjectReferenceExpression((IEntityType)projectedStructuralTypeShaper.StructuralType, alias),
                        (IEntityType)projectedStructuralTypeShaper.StructuralType));
                return source.Update(
                    translatedSelect,
                    new StructuralTypeShaperExpression(
                        projectedStructuralTypeShaper.StructuralType,
                        new ProjectionBindingExpression(translatedSelect, new ProjectionMember(), typeof(ValueBuffer)),
                        nullable: true));
            }
        }

        // Translate using OFFSET/LIMIT, except in subqueries where it isn't supported
        if (_subquery)
        {
            AddTranslationErrorDetails(CosmosStrings.LimitOffsetNotSupportedInSubqueries);
            return null;
        }

        if (!TryApplyOffset(select, translatedCount))
        {
            return null;
        }

        // Ordering of documents is not guaranteed in Cosmos, so we warn for Skip without OrderBy.
        // However, when querying on JSON arrays within documents, the order of elements is guaranteed, and Skip without OrderBy is
        // fine. Since subqueries must be correlated (i.e. reference an array in the outer query), we use that to decide whether to
        // warn or not.
        if (select.Orderings.Count == 0 && !_subquery)
        {
            _queryCompilationContext.Logger.RowLimitingOperationWithoutOrderByWarning();
        }

        return source;
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

        return AggregateResultShaper(source, projection, resultType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateTake(ShapedQueryExpression source, Expression count)
    {
        if (TranslateExpression(count) is not SqlExpression translatedCount)
        {
            return null;
        }

        var select = (SelectExpression)source.QueryExpression;

        // If the source query represents a bare array (e.g. x.Array), simplify x.Array.Take(2) => ARRAY_SLICE(x.Array, 0, 2) instead of
        // subquery+LIMIT (which isn't supported by Cosmos).
        // Even if the source is a full query (not a bare array), convert it to an array via the Cosmos ARRAY() operator; we do this
        // only in subqueries, because Cosmos supports OFFSET/LIMIT at the top-level but not in subqueries.
        var array = source.TryExtractArray(out var a, out var projection, out var projectedStructuralTypeShaper, out _)
            ? a
            : _subquery && source.TryConvertToArray(_typeMappingSource, out a, out projection)
                ? a
                : null;

        switch (array)
        {
            // ElementAtOrDefault over an array of scalars
            case SqlExpression scalarArray when projection is SqlExpression element:
            {
                // Take() is composed over Skip(), combine the two together to a single ARRAY_SLICE()
                var slice = array is SqlFunctionExpression
                {
                    Name: "ARRAY_SLICE", Arguments: [var nestedArray, var skipCount]
                } previousSlice
                    ? previousSlice.Update([nestedArray, skipCount, translatedCount])
                    : _sqlExpressionFactory.Function(
                        "ARRAY_SLICE", [scalarArray, TranslateExpression(Expression.Constant(0))!, translatedCount], scalarArray.Type,
                        scalarArray.TypeMapping);

                var alias = _aliasManager.GenerateSourceAlias(slice);
                select = SelectExpression.CreateForCollection(
                    slice,
                    alias,
                    new ScalarReferenceExpression(alias, element.Type, element.TypeMapping));
                return source.UpdateQueryExpression(select);
            }

            // ElementAtOrDefault over an array os structural types
            case not null when projectedStructuralTypeShaper is not null:
            {
                // Take() is composed over Skip(), combine the two together to a single ARRAY_SLICE()
                var slice = array is ObjectFunctionExpression
                {
                    Name: "ARRAY_SLICE", Arguments: [var nestedArray, var skipCount]
                } previousSlice
                    ? previousSlice.Update([nestedArray, skipCount, translatedCount])
                    : new ObjectFunctionExpression(
                        "ARRAY_SLICE", [array, TranslateExpression(Expression.Constant(0))!, translatedCount],
                        projectedStructuralTypeShaper.Type);

                var alias = _aliasManager.GenerateSourceAlias(slice);
                var translatedSelect = SelectExpression.CreateForCollection(
                    slice,
                    alias,
                    new EntityProjectionExpression(
                        new ObjectReferenceExpression((IEntityType)projectedStructuralTypeShaper.StructuralType, alias),
                        (IEntityType)projectedStructuralTypeShaper.StructuralType));
                return source.Update(
                    translatedSelect,
                    new StructuralTypeShaperExpression(
                        projectedStructuralTypeShaper.StructuralType,
                        new ProjectionBindingExpression(translatedSelect, new ProjectionMember(), typeof(ValueBuffer)),
                        nullable: true));
            }
        }

        // Translate using OFFSET/LIMIT, except in subqueries where it isn't supported
        if (_subquery)
        {
            AddTranslationErrorDetails(CosmosStrings.LimitOffsetNotSupportedInSubqueries);
            return null;
        }

        if (!TryApplyLimit(select, translatedCount))
        {
            return null;
        }

        // Ordering of documents is not guaranteed in Cosmos, so we warn for Take without OrderBy.
        // However, when querying on JSON arrays within documents, the order of elements is guaranteed, and Take without OrderBy is
        // fine. Since subqueries must be correlated (i.e. reference an array in the outer query), we use that to decide whether to
        // warn or not.
        if (select.Orderings.Count == 0 && !_subquery)
        {
            _queryCompilationContext.Logger.RowLimitingOperationWithoutOrderByWarning();
        }

        return source;
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
        => TryApplyPredicate(source, predicate) ? source : null;

    #region Queryable collection support

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateMemberAccess(Expression source, MemberIdentity member)
    {
        // Attempt to translate access into a primitive collection property
        if (_sqlTranslator.TryBindMember(
                _sqlTranslator.Visit(source),
                member,
                out var translatedExpression,
                out var property,
                wrapResultExpressionInReferenceExpression: false))
        {
            // TODO: TryBindMember returns EntityReferenceExpression, which is internal to SqlTranslatingEV.
            // Maybe have it return the StructuralTypeShaperExpression instead, and only when binding from within SqlTranslatingEV,
            // wrap with ERE?
            // Check: how is this currently working in relational?

            var sourceAlias = _aliasManager.GenerateSourceAlias(property.Name);

            switch (translatedExpression)
            {
                case StructuralTypeShaperExpression shaper when property is INavigation { IsCollection: true }:
                {
                    var targetEntityType = (IEntityType)shaper.StructuralType;
                    var projection = new EntityProjectionExpression(
                        new ObjectReferenceExpression(targetEntityType, sourceAlias), targetEntityType);
                    var select = SelectExpression.CreateForCollection(
                        shaper.ValueBufferExpression,
                        sourceAlias,
                        projection);
                    return CreateShapedQueryExpression(targetEntityType, select);
                }

                // TODO: Collection of complex type (#31253)

                // Note that non-collection navigations/complex types are handled in CosmosSqlTranslatingExpressionVisitor
                // (no collection -> no queryable operators)

                case SqlExpression sqlExpression when property is IProperty { IsPrimitiveCollection: true }:
                {
                    var elementClrType = sqlExpression.Type.GetSequenceType();
                    var select = SelectExpression.CreateForCollection(
                        sqlExpression,
                        sourceAlias,
                        new ScalarReferenceExpression(sourceAlias, elementClrType, sqlExpression.TypeMapping!.ElementTypeMapping!));
                    return CreateShapedQueryExpression(select, elementClrType);
                }
            }
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
        // The below produces an ArrayConstantExpression ([1,2,3]), wrapped by a SelectExpression (SELECT VALUE [1,2,3]).
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

        // TODO: Temporary hack - need to perform proper derivation of the array type mapping from the element (e.g. for
        // value conversion). #34026.
        var elementClrType = inlineQueryRootExpression.ElementType;
        var elementTypeMapping = _typeMappingSource.FindMapping(elementClrType)!;
        var arrayTypeMapping = _typeMappingSource.FindMapping(typeof(IEnumerable<>).MakeGenericType(elementClrType));
        var inlineArray = new ArrayConstantExpression(elementClrType, translatedItems, arrayTypeMapping);

        var sourceAlias = _aliasManager.GenerateSourceAlias(inlineArray);
        var select = SelectExpression.CreateForCollection(
            inlineArray,
            sourceAlias,
            new ScalarReferenceExpression(sourceAlias, elementClrType, elementTypeMapping));
        return CreateShapedQueryExpression(select, elementClrType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateParameterQueryRoot(ParameterQueryRootExpression parameterQueryRootExpression)
    {
        var queryParameter = parameterQueryRootExpression.QueryParameterExpression;

        // TODO: Temporary hack - need to perform proper derivation of the array type mapping from the element (e.g. for
        // value conversion). #34026.
        var elementClrType = parameterQueryRootExpression.ElementType;
        var arrayTypeMapping = _typeMappingSource.FindMapping(typeof(IEnumerable<>).MakeGenericType(elementClrType));
        var elementTypeMapping = _typeMappingSource.FindMapping(elementClrType)!;
        var sqlParameterExpression = new SqlParameterExpression(queryParameter.Name, queryParameter.Type, arrayTypeMapping);

        var sourceAlias = _aliasManager.GenerateSourceAlias(sqlParameterExpression.Name.TrimStart('_'));
        var select = SelectExpression.CreateForCollection(
            sqlParameterExpression,
            sourceAlias,
            new ScalarReferenceExpression(sourceAlias, elementClrType, elementTypeMapping));
        return CreateShapedQueryExpression(select, elementClrType);
    }

    #endregion Queryable collection support

    private ShapedQueryExpression? TranslateAggregate(ShapedQueryExpression source, LambdaExpression? selector, Type resultType, string functionName)
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

        if (!_subquery && resultType.IsNullableType())
        {
            // For nullable types, we want to return null from Max, Min, and Average, rather than throwing. See Issue #35094.
            // Note that relational databases typically return null, which propagates. Cosmos will instead return no elements,
            // and hence for Cosmos only we need to change no elements into null.
            source = source.UpdateResultCardinality(ResultCardinality.SingleOrDefault);
        }

        var projection = (SqlExpression)selectExpression.GetMappedProjection(new ProjectionMember());
        projection = _sqlExpressionFactory.Function(functionName, [projection], resultType, _typeMappingSource.FindMapping(resultType));

        return AggregateResultShaper(source, projection, resultType);
    }

    private bool TryApplyPredicate(ShapedQueryExpression source, LambdaExpression predicate)
    {
        var select = (SelectExpression)source.QueryExpression;

        if ((select.Limit is not null || select.Offset is not null)
            && !TryPushdownIntoSubquery(select))
        {
            return false;
        }

        if (TranslateLambdaExpression(source, predicate) is SqlExpression translation)
        {
            if (translation is not SqlConstantExpression { Value: true })
            {
                select.ApplyPredicate(translation);
            }

            return true;
        }

        return false;
    }

    private bool TryApplyPredicate(SelectExpression select, SqlExpression predicate)
    {
        if ((select.Limit is not null || select.Offset is not null)
            && !TryPushdownIntoSubquery(select))
        {
            return false;
        }

        select.ApplyPredicate(predicate);
        return true;
    }

    private bool TryApplyOffset(SelectExpression select, SqlExpression offset)
    {
        if ((select.Limit is not null || select.Offset is not null) && !TryPushdownIntoSubquery(select))
        {
            return false;
        }

        select.ApplyOffset(offset);
        return true;
    }

    private bool TryApplyLimit(SelectExpression select, SqlExpression limit)
    {
        if (select.Limit is not null && !TryPushdownIntoSubquery(select))
        {
            return false;
        }

        select.ApplyLimit(limit);
        return true;
    }

    private bool TryPushdownIntoSubquery(SelectExpression select)
    {
        if (select.Offset is not null || select.Limit is not null)
        {
            AddTranslationErrorDetails(CosmosStrings.LimitOffsetNotSupportedInSubqueries);
            return false;
        }

        // TODO: Implement subquery pushdown (#33968); though since Cosmos doesn't support OFFSET/LIMIT in subqueries, this isn't
        // going to unlock many scenarios.
        AddTranslationErrorDetails(CosmosStrings.NoSubqueryPushdown);
        return false;
    }

    private ShapedQueryExpression? TranslateCountLongCount(ShapedQueryExpression source, LambdaExpression? predicate, Type returnType)
    {
        // Simplify x.Array.Count() => ARRAY_LENGTH(x.Array) instead of (SELECT COUNT(1) FROM i IN x.Array))
        if (predicate is null && source.TryExtractArray(out var array, ignoreOrderings: true))
        {
            var simplifiedTranslation = _sqlExpressionFactory.Function(
                "ARRAY_LENGTH", new[] { array }, typeof(int), _typeMappingSource.FindMapping(typeof(int)));
            var select = new SelectExpression(simplifiedTranslation);

            return source.Update(select, new ProjectionBindingExpression(select, new ProjectionMember(), typeof(int)));
        }

        var selectExpression = (SelectExpression)source.QueryExpression;

        // TODO: Subquery pushdown, #33968
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
                new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), returnType.MakeNullable()),
                returnType));
    }

    private ShapedQueryExpression? TranslateSetOperation(
        ShapedQueryExpression source1,
        ShapedQueryExpression source2,
        string functionName,
        bool ignoreOrderings = false)
    {
        if (source1.TryConvertToArray(_typeMappingSource, out var array1, out var projection1, ignoreOrderings)
            && source2.TryConvertToArray(_typeMappingSource, out var array2, out var projection2, ignoreOrderings)
            && projection1.Type == projection2.Type)
        {
            var arrayType = typeof(IEnumerable<>).MakeGenericType(projection1.Type);

            // Set operation over arrays of scalars
            if (projection1 is SqlExpression sqlProjection1
                && projection2 is SqlExpression sqlProjection2
                && (sqlProjection1.TypeMapping ?? sqlProjection2.TypeMapping) is CosmosTypeMapping typeMapping)
            {
                var arrayTypeMapping = _typeMappingSource.FindMapping(arrayType, _queryCompilationContext.Model, typeMapping);
                var translation = _sqlExpressionFactory.Function(functionName, [array1, array2], arrayType, arrayTypeMapping);
                var alias = _aliasManager.GenerateSourceAlias(translation);
                var select = SelectExpression.CreateForCollection(
                    translation, alias, new ScalarReferenceExpression(alias, projection1.Type, typeMapping));
                return source1.UpdateQueryExpression(select);
            }

            // Set operation over arrays of structural types
            if (source1.ShaperExpression is StructuralTypeShaperExpression { StructuralType: var structuralType1 }
                && source2.ShaperExpression is StructuralTypeShaperExpression { StructuralType: var structuralType2 }
                && structuralType1 == structuralType2)
            {
                var translation = new ObjectFunctionExpression(functionName, [array1, array2], arrayType);
                var alias = _aliasManager.GenerateSourceAlias(translation);
                var select = SelectExpression.CreateForCollection(
                    translation, alias, new ObjectReferenceExpression((IEntityType)structuralType1, alias));
                return CreateShapedQueryExpression(select, structuralType1.ClrType);
            }
        }

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
        => TranslateExpression(RemapLambdaBody(shapedQueryExpression, lambdaExpression));

    private Expression RemapLambdaBody(ShapedQueryExpression shapedQueryExpression, LambdaExpression lambdaExpression)
        => ReplacingExpressionVisitor.Replace(
            lambdaExpression.Parameters.Single(), shapedQueryExpression.ShaperExpression, lambdaExpression.Body);

    private static ShapedQueryExpression AggregateResultShaper(
        ShapedQueryExpression source,
        Expression projection,
        Type resultType)
    {
        var selectExpression = (SelectExpression)source.QueryExpression;
        selectExpression.ReplaceProjectionMapping(new Dictionary<ProjectionMember, Expression> { { new ProjectionMember(), projection } });

        selectExpression.ClearOrdering();

        var nullableResultType = resultType.MakeNullable();
        Expression shaper = new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), nullableResultType);

        if (resultType != shaper.Type)
        {
            shaper = Expression.Convert(shaper, resultType);
        }

        return source.UpdateShaperExpression(shaper);
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
}
