// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
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
    private bool _subquery;
    private ReadItemInfo? _readItemExpression;

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
                Arguments: [MethodCallExpression innerMethodCall]
            })
        {
            var clrType = innerMethodCall.Type.TryGetSequenceType() ?? typeof(object);
            if (innerMethodCall is
                {
                    Method: { Name: nameof(Queryable.Select), IsGenericMethod: true },
                    Arguments:
                    [
                        MethodCallExpression innerInnerMethodCall,
                        UnaryExpression { NodeType: ExpressionType.Quote } unaryExpression
                    ]
                })
            {
                // Strip out Include and Convert expressions until we get to the parameter, or not.
                var processing = unaryExpression.Operand;
                while (true)
                {
                    switch (processing)
                    {
                        case UnaryExpression { NodeType: ExpressionType.Quote or ExpressionType.Convert } q:
                            processing = q.Operand;
                            continue;
                        case LambdaExpression l:
                            processing = l.Body;
                            continue;
                        case IncludeExpression i:
                            processing = i.EntityExpression;
                            continue;
                    }
                    break;
                }

                // If we are left with the ParameterExpression, then it's safe to use ReadItem.
                if (processing is ParameterExpression)
                {
                    innerMethodCall = innerInnerMethodCall;
                }
            }

            if (innerMethodCall is
                {
                    Method: { Name: nameof(Queryable.Where), IsGenericMethod: true },
                    Arguments:
                    [
                        EntityQueryRootExpression { EntityType: var entityType },
                        UnaryExpression { Operand: LambdaExpression lambdaExpression, NodeType: ExpressionType.Quote }
                    ]
                })
            {
                var queryProperties = new List<IProperty>();
                var parameterNames = new List<string>();

                if (ExtractPartitionKeyFromPredicate(entityType, lambdaExpression.Body, queryProperties, parameterNames))
                {
                    var entityTypePrimaryKeyProperties = entityType.FindPrimaryKey()!.Properties;
                    var partitionKeyProperties = entityType.GetPartitionKeyProperties();

                    if (entityTypePrimaryKeyProperties.SequenceEqual(queryProperties)
                        && (!partitionKeyProperties.Any()
                            || partitionKeyProperties.All(p => entityTypePrimaryKeyProperties.Contains(p)))
                        && entityType.GetJsonIdDefinition() != null)
                    {
                        var propertyParameterList = queryProperties.Zip(
                                parameterNames,
                                (property, parameter) => (property, parameter))
                            .ToDictionary(tuple => tuple.property, tuple => tuple.parameter);

                        _readItemExpression = new ReadItemInfo(entityType, propertyParameterList, clrType);
                    }
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
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        var method = methodCallExpression.Method;
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
                            Microsoft.EntityFrameworkCore.Infrastructure.ExpressionExtensions.CreateEFPropertyExpression(
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
        => CreateShapedQueryExpression(
            entityType,
            _readItemExpression == null
                ? _sqlExpressionFactory.Select(entityType)
                : _sqlExpressionFactory.ReadItem(entityType, _readItemExpression));

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
            Microsoft.EntityFrameworkCore.Infrastructure.ExpressionExtensions.CreateEqualsExpression(anyLambdaParameter, item),
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
                SqlExpression translation = _sqlExpressionFactory.ArrayIndex(
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
                        new SqlConstantExpression(Expression.Constant(null, typeof(object)), _typeMappingSource.FindMapping(typeof(int))),
                        translation.Type);
                }

                var translatedSelect =
                    new SelectExpression(new EntityProjectionExpression(translation, (IEntityType)projectedStructuralTypeShaper.StructuralType));
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

        select.ApplyOffset(translatedIndex);
        select.ApplyLimit(TranslateExpression(Expression.Constant(1))!);

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
        => TranslateCountLongCount(source, predicate, typeof(long));

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
                var shaper = select.AddJoin(inner, source.ShaperExpression);

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
                var slice = _sqlExpressionFactory.Function(
                    "ARRAY_SLICE", [scalarArray, translatedCount], scalarArray.Type, scalarArray.TypeMapping);

                // TODO: Proper alias management (#33894). Ideally reach into the source of the original SelectExpression and use that alias.
                var translatedSelect = SelectExpression.CreateForCollection(
                    slice,
                    "i",
                    new ScalarReferenceExpression("i", element.Type, element.TypeMapping));
                return source.UpdateQueryExpression(translatedSelect);
            }

            // ElementAtOrDefault over an array os structural types
            case not null when projectedStructuralTypeShaper is not null:
            {
                // TODO: Proper alias management (#33894).
                var slice = new ObjectFunctionExpression("ARRAY_SLICE", [array, translatedCount], projectedStructuralTypeShaper.Type);
                var translatedSelect = SelectExpression.CreateForCollection(
                    slice,
                    "i",
                    new EntityProjectionExpression(
                        new ObjectReferenceExpression((IEntityType)projectedStructuralTypeShaper.StructuralType, "i"),
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

        // Ordering of documents is not guaranteed in Cosmos, so we warn for Skip without OrderBy.
        // However, when querying on JSON arrays within documents, the order of elements is guaranteed, and Skip without OrderBy is
        // fine. Since subqueries must be correlated (i.e. reference an array in the outer query), we use that to decide whether to
        // warn or not.
        if (select.Orderings.Count == 0 && !_subquery)
        {
            _queryCompilationContext.Logger.RowLimitingOperationWithoutOrderByWarning();
        }

        select.ApplyOffset(translatedCount);

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
                var slice = array is SqlFunctionExpression { Name: "ARRAY_SLICE", Arguments: [var nestedArray, var skipCount] } previousSlice
                    ? previousSlice.Update([nestedArray, skipCount, translatedCount])
                    : _sqlExpressionFactory.Function(
                        "ARRAY_SLICE", [scalarArray, TranslateExpression(Expression.Constant(0))!, translatedCount], scalarArray.Type,
                        scalarArray.TypeMapping);

                // TODO: Proper alias management (#33894). Ideally reach into the source of the original SelectExpression and use that alias.
                select = SelectExpression.CreateForCollection(
                    slice,
                    "i",
                    new ScalarReferenceExpression("i", element.Type, element.TypeMapping));
                return source.UpdateQueryExpression(select);
            }

            // ElementAtOrDefault over an array os structural types
            case not null when projectedStructuralTypeShaper is not null:
            {
                // TODO: Proper alias management (#33894).
                // Take() is composed over Skip(), combine the two together to a single ARRAY_SLICE()
                var slice = array is ObjectFunctionExpression { Name: "ARRAY_SLICE", Arguments: [var nestedArray, var skipCount] } previousSlice
                    ? previousSlice.Update([nestedArray, skipCount, translatedCount])
                    : new ObjectFunctionExpression(
                        "ARRAY_SLICE", [array, TranslateExpression(Expression.Constant(0))!, translatedCount], projectedStructuralTypeShaper.Type);

                var translatedSelect = SelectExpression.CreateForCollection(
                    slice,
                    "i",
                    new EntityProjectionExpression(
                        new ObjectReferenceExpression((IEntityType)projectedStructuralTypeShaper.StructuralType, "i"),
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

        // Ordering of documents is not guaranteed in Cosmos, so we warn for Take without OrderBy.
        // However, when querying on JSON arrays within documents, the order of elements is guaranteed, and Take without OrderBy is
        // fine. Since subqueries must be correlated (i.e. reference an array in the outer query), we use that to decide whether to
        // warn or not.
        if (select.Orderings.Count == 0 && !_subquery)
        {
            _queryCompilationContext.Logger.RowLimitingOperationWithoutOrderByWarning();
        }

        select.ApplyLimit(translatedCount);

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
            switch (translatedExpression)
            {
                case StructuralTypeShaperExpression shaper when property is INavigation { IsCollection: true }:
                {
                    // TODO: Alias management #33894
                    var targetEntityType = (IEntityType)shaper.StructuralType;
                    var sourceAlias = "t";
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
                    // TODO: Do proper alias management: #33894
                    var select = SelectExpression.CreateForCollection(
                        sqlExpression,
                        "i",
                        new ScalarReferenceExpression("i", elementClrType, sqlExpression.TypeMapping!.ElementTypeMapping!));
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

        // TODO: Temporary hack - need to perform proper derivation of the array type mapping from the element (e.g. for
        // value conversion). #34026.
        var elementClrType = inlineQueryRootExpression.ElementType;
        var elementTypeMapping = _typeMappingSource.FindMapping(elementClrType)!;
        var arrayTypeMapping = _typeMappingSource.FindMapping(elementClrType.MakeArrayType()); // TODO: IEnumerable?
        var inlineArray = new ArrayConstantExpression(elementClrType, translatedItems, arrayTypeMapping);

        // TODO: Do proper alias management: #33894
        var select = SelectExpression.CreateForCollection(
            inlineArray,
            "i",
            new ScalarReferenceExpression("i", elementClrType, elementTypeMapping));
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
        if (parameterQueryRootExpression.ParameterExpression.Name?.StartsWith(
                QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal)
            != true)
        {
            return null;
        }

        // TODO: Temporary hack - need to perform proper derivation of the array type mapping from the element (e.g. for
        // value conversion). #34026.
        var elementClrType = parameterQueryRootExpression.ElementType;
        var arrayTypeMapping = _typeMappingSource.FindMapping(elementClrType.MakeArrayType()); // TODO: IEnumerable?
        var elementTypeMapping = _typeMappingSource.FindMapping(elementClrType)!;
        var sqlParameterExpression = new SqlParameterExpression(parameterQueryRootExpression.ParameterExpression, arrayTypeMapping);

        // TODO: Do proper alias management: #33894
        var select = SelectExpression.CreateForCollection(
            sqlParameterExpression,
            "i",
            new ScalarReferenceExpression("i", elementClrType, elementTypeMapping));
        return CreateShapedQueryExpression(select, elementClrType);
    }

    #endregion Queryable collection support

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
            // Set operation over arrays of scalars
            if (projection1 is SqlExpression sqlProjection1
                && projection2 is SqlExpression sqlProjection2
                && (sqlProjection1.TypeMapping ?? sqlProjection2.TypeMapping) is CoreTypeMapping typeMapping)
            {
                // TODO: Proper alias management (#33894).
                var translation = _sqlExpressionFactory.Function(functionName, [array1, array2], projection1.Type, typeMapping);
                var select = SelectExpression.CreateForCollection(
                    translation, "i", new ScalarReferenceExpression("i", projection1.Type, typeMapping));
                return source1.UpdateQueryExpression(select);
            }

            // Set operation over arrays of structural types
            if (source1.ShaperExpression is StructuralTypeShaperExpression { StructuralType: var structuralType1 }
                && source2.ShaperExpression is StructuralTypeShaperExpression { StructuralType: var structuralType2 }
                && structuralType1 == structuralType2)
            {
                // TODO: Proper alias management (#33894).
                var translation = new ObjectFunctionExpression(functionName, [array1, array2], projection1.Type);
                var select = SelectExpression.CreateForCollection(
                    translation, "i", new ObjectReferenceExpression((IEntityType)structuralType1, "i"));
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
