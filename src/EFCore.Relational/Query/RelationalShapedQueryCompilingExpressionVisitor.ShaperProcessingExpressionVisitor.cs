// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

public partial class RelationalShapedQueryCompilingExpressionVisitor
{
    private sealed partial class ShaperProcessingExpressionVisitor : ExpressionVisitor
    {
        /// <summary>
        ///     Reading database values
        /// </summary>
        private static readonly MethodInfo IsDbNullMethod =
            typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.IsDBNull), new[] { typeof(int) })!;

        public static readonly MethodInfo GetFieldValueMethod =
            typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.GetFieldValue), new[] { typeof(int) })!;

        /// <summary>
        ///     Coordinating results
        /// </summary>
        private static readonly MemberInfo ResultContextValuesMemberInfo
            = typeof(ResultContext).GetMember(nameof(ResultContext.Values))[0];

        private static readonly MemberInfo SingleQueryResultCoordinatorResultReadyMemberInfo
            = typeof(SingleQueryResultCoordinator).GetMember(nameof(SingleQueryResultCoordinator.ResultReady))[0];

        private static readonly MethodInfo CollectionAccessorAddMethodInfo
            = typeof(IClrCollectionAccessor).GetTypeInfo().GetDeclaredMethod(nameof(IClrCollectionAccessor.Add))!;

        private static readonly MethodInfo JsonElementTryGetPropertyMethod
            = typeof(JsonElement).GetMethod(nameof(JsonElement.TryGetProperty), new[] { typeof(string), typeof(JsonElement).MakeByRefType() })!;

        private static readonly MethodInfo JsonElementGetItemMethodInfo
            = typeof(JsonElement).GetMethod("get_Item", new[] { typeof(int) })!;

        private static readonly PropertyInfo ObjectArrayIndexerPropertyInfo
            = typeof(object[]).GetProperty("Item")!;

        private static readonly PropertyInfo NullableJsonElementHasValuePropertyInfo
            = typeof(JsonElement?).GetProperty(nameof(Nullable<JsonElement>.HasValue))!;

        private static readonly PropertyInfo NullableJsonElementValuePropertyInfo
            = typeof(JsonElement?).GetProperty(nameof(Nullable<JsonElement>.Value))!;

        private static readonly MethodInfo ArrayCopyMethodInfo
            = typeof(Array).GetMethod(nameof(Array.Copy), new[] { typeof(Array), typeof(Array), typeof(int) })!;

        private readonly RelationalShapedQueryCompilingExpressionVisitor _parentVisitor;
        private readonly ISet<string>? _tags;
        private readonly bool _isTracking;
        private readonly bool _isAsync;
        private readonly bool _splitQuery;
        private readonly bool _detailedErrorsEnabled;
        private readonly bool _generateCommandCache;
        private readonly ParameterExpression _resultCoordinatorParameter;
        private readonly ParameterExpression? _executionStrategyParameter;

        /// <summary>
        ///     States scoped to SelectExpression
        /// </summary>
        private readonly SelectExpression _selectExpression;
        private readonly ParameterExpression _dataReaderParameter;
        private readonly ParameterExpression _resultContextParameter;
        private readonly ParameterExpression? _indexMapParameter;
        private readonly ReaderColumn?[]? _readerColumns;

        /// <summary>
        ///     States to materialize only once
        /// </summary>
        private readonly Dictionary<Expression, Expression> _variableShaperMapping = new(ReferenceEqualityComparer.Instance);

        /// <summary>
        ///     There are always entity variables to avoid materializing same entity twice
        /// </summary>
        private readonly List<ParameterExpression> _variables = new();

        private readonly List<Expression> _expressions = new();

        /// <summary>
        ///     IncludeExpressions are added later in case they are using ValuesArray
        /// </summary>
        private readonly List<Expression> _includeExpressions = new();

        /// <summary>
        ///     Json entities are added after includes so that we can utilize tracking (includes will track all json entities)
        /// </summary>
        private readonly List<Expression> _jsonEntityExpressions = new();

        /// <summary>
        ///     If there is collection shaper then we need to construct ValuesArray to store values temporarily in ResultContext
        /// </summary>
        private List<Expression>? _collectionPopulatingExpressions;
        private Expression? _valuesArrayExpression;
        private List<Expression>? _valuesArrayInitializers;

        private bool _containsCollectionMaterialization;

        /// <summary>
        ///     Since identifiers for collection are not part of larger lambda they don't cannot use caching to materialize only once.
        /// </summary>
        private bool _inline;
        private int _collectionId;

        /// <summary>
        ///     States to convert code to data reader read
        /// </summary>
        private readonly Dictionary<ParameterExpression, IDictionary<IProperty, int>> _materializationContextBindings = new();
        private readonly Dictionary<ParameterExpression, object> _entityTypeIdentifyingExpressionInfo = new();
        private readonly Dictionary<ProjectionBindingExpression, string> _singleEntityTypeDiscriminatorValues = new();

        private readonly Dictionary<ParameterExpression, (ParameterExpression, ParameterExpression)>
            _jsonValueBufferParameterMapping = new();

        private readonly Dictionary<ParameterExpression, (ParameterExpression, ParameterExpression)>
            _jsonMaterializationContextParameterMapping = new();

        /// <summary>
        ///     Cache for the JsonElement values we have generated - storing variables that the JsonElements are assigned to
        /// </summary>
        private readonly Dictionary<(int JsonColumnIndex, (string? JsonPropertyName, int? ConstantArrayIndex, int? NonConstantArrayIndex)[] AdditionalPath), ParameterExpression> _existingJsonElementMap
            = new(new ExisitingJsonElementMapKeyComparer());

        /// <summary>
        ///     Cache for the key values we have generated - storing variables that the keys are assigned to
        /// </summary>
        private readonly Dictionary<(int JsonColumnIndex, (int? ConstantArrayIndex, int? NonConstantArrayIndex)[] AdditionalPath), ParameterExpression> _existingKeyValuesMap
            = new(new ExisitingJsonKeyValuesMapKeyComparer());

        /// <summary>
        ///     Map between index of the non-constant json array element access
        ///     and the variable we store it's value that we extract from the reader
        /// </summary>
        private readonly Dictionary<int, ParameterExpression> _jsonArrayNonConstantElementAccessMap = new();

        public ShaperProcessingExpressionVisitor(
            RelationalShapedQueryCompilingExpressionVisitor parentVisitor,
            SelectExpression selectExpression,
            ISet<string> tags,
            bool splitQuery,
            bool indexMap)
        {
            _parentVisitor = parentVisitor;
            _resultCoordinatorParameter = Expression.Parameter(
                splitQuery ? typeof(SplitQueryResultCoordinator) : typeof(SingleQueryResultCoordinator), "resultCoordinator");
            _executionStrategyParameter = splitQuery ? Expression.Parameter(typeof(IExecutionStrategy), "executionStrategy") : null;

            _selectExpression = selectExpression;
            _tags = tags;
            _dataReaderParameter = Expression.Parameter(typeof(DbDataReader), "dataReader");
            _resultContextParameter = Expression.Parameter(typeof(ResultContext), "resultContext");
            _indexMapParameter = indexMap ? Expression.Parameter(typeof(int[]), "indexMap") : null;
            if (parentVisitor.QueryCompilationContext.IsBuffering)
            {
                _readerColumns = new ReaderColumn?[_selectExpression.Projection.Count];
            }

            _generateCommandCache = true;
            _detailedErrorsEnabled = parentVisitor._detailedErrorsEnabled;
            _isTracking = parentVisitor.QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll;
            _isAsync = parentVisitor.QueryCompilationContext.IsAsync;
            _splitQuery = splitQuery;

            _selectExpression.ApplyTags(_tags);
        }

        // For single query scenario
        private ShaperProcessingExpressionVisitor(
            RelationalShapedQueryCompilingExpressionVisitor parentVisitor,
            ParameterExpression resultCoordinatorParameter,
            SelectExpression selectExpression,
            ParameterExpression dataReaderParameter,
            ParameterExpression resultContextParameter,
            ReaderColumn?[]? readerColumns)
        {
            _parentVisitor = parentVisitor;
            _resultCoordinatorParameter = resultCoordinatorParameter;

            _selectExpression = selectExpression;
            _dataReaderParameter = dataReaderParameter;
            _resultContextParameter = resultContextParameter;
            _readerColumns = readerColumns;
            _generateCommandCache = false;
            _detailedErrorsEnabled = parentVisitor._detailedErrorsEnabled;
            _isTracking = parentVisitor.QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll;
            _isAsync = parentVisitor.QueryCompilationContext.IsAsync;
            _splitQuery = false;
        }

        // For split query scenario
        private ShaperProcessingExpressionVisitor(
            RelationalShapedQueryCompilingExpressionVisitor parentVisitor,
            ParameterExpression resultCoordinatorParameter,
            ParameterExpression executionStrategyParameter,
            SelectExpression selectExpression,
            ISet<string> tags)
        {
            _parentVisitor = parentVisitor;
            _resultCoordinatorParameter = resultCoordinatorParameter;
            _executionStrategyParameter = executionStrategyParameter;

            _selectExpression = selectExpression;
            _tags = tags;
            _dataReaderParameter = Expression.Parameter(typeof(DbDataReader), "dataReader");
            _resultContextParameter = Expression.Parameter(typeof(ResultContext), "resultContext");
            if (parentVisitor.QueryCompilationContext.IsBuffering)
            {
                _readerColumns = new ReaderColumn[_selectExpression.Projection.Count];
            }

            _generateCommandCache = true;
            _detailedErrorsEnabled = parentVisitor._detailedErrorsEnabled;
            _isTracking = parentVisitor.QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll;
            _isAsync = parentVisitor.QueryCompilationContext.IsAsync;
            _splitQuery = true;

            _selectExpression.ApplyTags(_tags);
        }

        public LambdaExpression ProcessRelationalGroupingResult(
            RelationalGroupByResultExpression relationalGroupByResultExpression,
            out RelationalCommandCache relationalCommandCache,
            out IReadOnlyList<ReaderColumn?>? readerColumns,
            out LambdaExpression keySelector,
            out LambdaExpression keyIdentifier,
            out LambdaExpression? relatedDataLoaders,
            ref int collectionId)
        {
            _inline = true;
            keySelector = Expression.Lambda(
                Visit(relationalGroupByResultExpression.KeyShaper),
                QueryCompilationContext.QueryContextParameter,
                _dataReaderParameter);

            keyIdentifier = Expression.Lambda(
                    Visit(relationalGroupByResultExpression.KeyIdentifier),
                    QueryCompilationContext.QueryContextParameter,
                    _dataReaderParameter);

            _inline = false;

            return ProcessShaper(relationalGroupByResultExpression.ElementShaper,
                out relationalCommandCache!,
                out readerColumns,
                out relatedDataLoaders,
                ref collectionId);
        }

        public LambdaExpression ProcessShaper(
            Expression shaperExpression,
            out RelationalCommandCache? relationalCommandCache,
            out IReadOnlyList<ReaderColumn?>? readerColumns,
            out LambdaExpression? relatedDataLoaders,
            ref int collectionId)
        {
            relatedDataLoaders = null;
            _collectionId = collectionId;

            if (_indexMapParameter != null)
            {
                var result = Visit(shaperExpression);
                _expressions.Add(result);
                result = Expression.Block(_variables, _expressions);

                relationalCommandCache = new RelationalCommandCache(
                    _parentVisitor.Dependencies.MemoryCache,
                    _parentVisitor.RelationalDependencies.QuerySqlGeneratorFactory,
                    _parentVisitor.RelationalDependencies.RelationalParameterBasedSqlProcessorFactory,
                    _selectExpression,
                    _parentVisitor._useRelationalNulls);
                readerColumns = _readerColumns;

                return Expression.Lambda(
                    result,
                    QueryCompilationContext.QueryContextParameter,
                    _dataReaderParameter,
                    _indexMapParameter);
            }

            _containsCollectionMaterialization = new CollectionShaperFindingExpressionVisitor()
                .ContainsCollectionMaterialization(shaperExpression);

            if (!_containsCollectionMaterialization)
            {
                var result = Visit(shaperExpression);
                _expressions.AddRange(_includeExpressions);
                _expressions.AddRange(_jsonEntityExpressions);
                _expressions.Add(result);
                result = Expression.Block(_variables, _expressions);

                relationalCommandCache = _generateCommandCache
                    ? new RelationalCommandCache(
                        _parentVisitor.Dependencies.MemoryCache,
                        _parentVisitor.RelationalDependencies.QuerySqlGeneratorFactory,
                        _parentVisitor.RelationalDependencies.RelationalParameterBasedSqlProcessorFactory,
                        _selectExpression,
                        _parentVisitor._useRelationalNulls)
                    : null;
                readerColumns = _readerColumns;

                return Expression.Lambda(
                    result,
                    QueryCompilationContext.QueryContextParameter,
                    _dataReaderParameter,
                    _resultContextParameter,
                    _resultCoordinatorParameter);
            }
            else
            {
                _valuesArrayExpression = Expression.MakeMemberAccess(_resultContextParameter, ResultContextValuesMemberInfo);
                _collectionPopulatingExpressions = new List<Expression>();
                _valuesArrayInitializers = new List<Expression>();

                var result = Visit(shaperExpression);

                var valueArrayInitializationExpression = Expression.Assign(
                    _valuesArrayExpression, Expression.NewArrayInit(typeof(object), _valuesArrayInitializers));

                _expressions.AddRange(_jsonEntityExpressions);
                _expressions.Add(valueArrayInitializationExpression);
                _expressions.AddRange(_includeExpressions);

                if (_splitQuery)
                {
                    _expressions.Add(Expression.Default(result.Type));

                    var initializationBlock = Expression.Block(_variables, _expressions);
                    result = Expression.Condition(
                        Expression.Equal(_valuesArrayExpression, Expression.Constant(null, typeof(object[]))),
                        initializationBlock,
                        result);

                    if (_isAsync)
                    {
                        var tasks = Expression.NewArrayInit(
                            typeof(Func<Task>), _collectionPopulatingExpressions.Select(
                                e => Expression.Lambda<Func<Task>>(e)));
                        relatedDataLoaders =
                            Expression.Lambda<Func<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator, Task>>(
                                Expression.Call(TaskAwaiterMethodInfo, tasks),
                                QueryCompilationContext.QueryContextParameter,
                                _executionStrategyParameter!,
                                _resultCoordinatorParameter);
                    }
                    else
                    {
                        relatedDataLoaders =
                            Expression.Lambda<Action<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator>>(
                                Expression.Block(_collectionPopulatingExpressions),
                                QueryCompilationContext.QueryContextParameter,
                                _executionStrategyParameter!,
                                _resultCoordinatorParameter);
                    }
                }
                else
                {
                    var initializationBlock = Expression.Block(_variables, _expressions);

                    var conditionalMaterializationExpressions = new List<Expression>
                    {
                        Expression.IfThen(
                            Expression.Equal(_valuesArrayExpression, Expression.Constant(null, typeof(object[]))),
                            initializationBlock)
                    };

                    conditionalMaterializationExpressions.AddRange(_collectionPopulatingExpressions);

                    conditionalMaterializationExpressions.Add(
                        Expression.Condition(
                            Expression.IsTrue(
                                Expression.MakeMemberAccess(
                                    _resultCoordinatorParameter, SingleQueryResultCoordinatorResultReadyMemberInfo)),
                            result,
                            Expression.Default(result.Type)));

                    result = Expression.Block(conditionalMaterializationExpressions);
                }

                relationalCommandCache = _generateCommandCache
                    ? new RelationalCommandCache(
                        _parentVisitor.Dependencies.MemoryCache,
                        _parentVisitor.RelationalDependencies.QuerySqlGeneratorFactory,
                        _parentVisitor.RelationalDependencies.RelationalParameterBasedSqlProcessorFactory,
                        _selectExpression,
                        _parentVisitor._useRelationalNulls)
                    : null;
                readerColumns = _readerColumns;

                collectionId = _collectionId;

                return Expression.Lambda(
                    result,
                    QueryCompilationContext.QueryContextParameter,
                    _dataReaderParameter,
                    _resultContextParameter,
                    _resultCoordinatorParameter);
            }
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (binaryExpression.NodeType == ExpressionType.Assign
                && binaryExpression.Left is ParameterExpression parameterExpression
                && parameterExpression.Type == typeof(MaterializationContext))
            {
                var newExpression = (NewExpression)binaryExpression.Right;

                if (newExpression.Arguments[0] is ProjectionBindingExpression projectionBindingExpression)
                {
                    var propertyMap = (IDictionary<IProperty, int>)GetProjectionIndex(projectionBindingExpression);
                    _materializationContextBindings[parameterExpression] = propertyMap;
                    _entityTypeIdentifyingExpressionInfo[parameterExpression] =
                        // If single entity type is being selected in hierarchy then we use the value directly else we store the offset to
                        // read discriminator value.
                        _singleEntityTypeDiscriminatorValues.TryGetValue(projectionBindingExpression, out var value)
                            ? value
                            : propertyMap.Values.Max() + 1;

                    var updatedExpression = newExpression.Update(
                        new[] { Expression.Constant(ValueBuffer.Empty), newExpression.Arguments[1] });

                    return Expression.Assign(binaryExpression.Left, updatedExpression);
                }

                if (newExpression.Arguments[0] is ParameterExpression valueBufferParameter
                    && _jsonValueBufferParameterMapping.ContainsKey(valueBufferParameter))
                {
                    _jsonMaterializationContextParameterMapping[parameterExpression] =
                        _jsonValueBufferParameterMapping[valueBufferParameter];

                    var updatedExpression = newExpression.Update(
                        new[] { Expression.Constant(ValueBuffer.Empty), newExpression.Arguments[1] });

                    return Expression.Assign(binaryExpression.Left, updatedExpression);
                }
            }

            if (binaryExpression.NodeType == ExpressionType.Assign
                && binaryExpression.Left is MemberExpression memberExpression
                && memberExpression.Member is FieldInfo fieldInfo
                && fieldInfo.IsInitOnly)
            {
                return memberExpression.Assign(Visit(binaryExpression.Right));
            }

            return base.VisitBinary(binaryExpression);
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case RelationalEntityShaperExpression entityShaperExpression
                    when !_inline && entityShaperExpression.ValueBufferExpression is ProjectionBindingExpression projectionBindingExpression:
                {
                    if (!_variableShaperMapping.TryGetValue(entityShaperExpression.ValueBufferExpression, out var accessor))
                    {
                        if (GetProjectionIndex(projectionBindingExpression) is JsonProjectionInfo jsonProjectionInfo)
                        {
                            // json entity at the root
                            var (jsonElementParameter, keyValuesParameter) = JsonShapingPreProcess(
                                jsonProjectionInfo,
                                entityShaperExpression.EntityType);

                            var shaperResult = CreateJsonShapers(
                                entityShaperExpression.EntityType,
                                entityShaperExpression.IsNullable,
                                collection: false,
                                jsonElementParameter,
                                keyValuesParameter,
                                parentEntityExpression: null,
                                navigation: null);

                            var visitedShaperResult = Visit(shaperResult);
                            var visitedShaperResultParameter = Expression.Parameter(visitedShaperResult.Type);
                            _variables.Add(visitedShaperResultParameter);
                            _jsonEntityExpressions.Add(Expression.Assign(visitedShaperResultParameter, visitedShaperResult));


                            accessor = CompensateForCollectionMaterialization(
                                visitedShaperResultParameter,
                                entityShaperExpression.Type);
                        }
                        else
                        {
                            var entityParameter = Expression.Parameter(entityShaperExpression.Type);
                            _variables.Add(entityParameter);
                            if (entityShaperExpression.EntityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy)
                            {
                                var concreteTypes = entityShaperExpression.EntityType.GetDerivedTypesInclusive().Where(e => !e.IsAbstract())
                                    .ToArray();
                                // Single concrete TPC entity type won't have discriminator column.
                                // We store the value here and inject it directly rather than reading from server.
                                if (concreteTypes.Length == 1)
                                {
                                    _singleEntityTypeDiscriminatorValues[
                                            (ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression]
                                        = concreteTypes[0].ShortName();
                                }
                            }

                            var entityMaterializationExpression = _parentVisitor.InjectEntityMaterializers(entityShaperExpression);
                            entityMaterializationExpression = Visit(entityMaterializationExpression);

                            _expressions.Add(Expression.Assign(entityParameter, entityMaterializationExpression));

                            accessor = CompensateForCollectionMaterialization(
                                entityParameter,
                                entityShaperExpression.Type);
                        }

                        _variableShaperMapping[entityShaperExpression.ValueBufferExpression] = accessor;
                    }

                    return accessor;
                }

                case RelationalEntityShaperExpression entityShaperExpression
                    when _inline && entityShaperExpression.ValueBufferExpression is ProjectionBindingExpression projectionBindingExpression:
                {
                    if (entityShaperExpression.EntityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy)
                    {
                        var concreteTypes = entityShaperExpression.EntityType.GetDerivedTypesInclusive().Where(e => !e.IsAbstract())
                            .ToArray();
                        // Single concrete TPC entity type won't have discriminator column.
                        // We store the value here and inject it directly rather than reading from server.
                        if (concreteTypes.Length == 1)
                        {
                            _singleEntityTypeDiscriminatorValues[
                                    (ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression]
                                = concreteTypes[0].ShortName();
                        }
                    }

                    var entityMaterializationExpression = _parentVisitor.InjectEntityMaterializers(entityShaperExpression);
                    entityMaterializationExpression = Visit(entityMaterializationExpression);

                    return entityMaterializationExpression;
                }

                case CollectionResultExpression collectionResultExpression
                    when collectionResultExpression.Navigation is INavigation navigation
                    && GetProjectionIndex(collectionResultExpression.ProjectionBindingExpression)
                        is JsonProjectionInfo jsonProjectionInfo:
                {
                    // json entity collection at the root
                    var (jsonElementParameter, keyValuesParameter) = JsonShapingPreProcess(
                        jsonProjectionInfo,
                        navigation.TargetEntityType);

                    var shaperResult = CreateJsonShapers(
                        navigation.TargetEntityType,
                        nullable: true,
                        collection: true,
                        jsonElementParameter,
                        keyValuesParameter,
                        parentEntityExpression: null,
                        navigation);

                    var visitedShaperResult = Visit(shaperResult);

                    var jsonCollectionParameter = Expression.Parameter(collectionResultExpression.Type);

                    _variables.Add(jsonCollectionParameter);
                    _jsonEntityExpressions.Add(Expression.Assign(jsonCollectionParameter, visitedShaperResult));

                    return CompensateForCollectionMaterialization(
                        jsonCollectionParameter,
                        collectionResultExpression.Type);
                }

                case ProjectionBindingExpression projectionBindingExpression
                    when _inline:
                {
                    var projectionIndex = (int)GetProjectionIndex(projectionBindingExpression);
                    var projection = _selectExpression.Projection[projectionIndex];

                    return CreateGetValueExpression(
                        _dataReaderParameter,
                        projectionIndex,
                        IsNullableProjection(projection),
                        projection.Expression.TypeMapping!,
                        projectionBindingExpression.Type);
                }

                case ProjectionBindingExpression projectionBindingExpression
                    when !_inline:
                {
                    if (_variableShaperMapping.TryGetValue(projectionBindingExpression, out var accessor))
                    {
                        return accessor;
                    }

                    var projectionIndex = (int)GetProjectionIndex(projectionBindingExpression);
                    var projection = _selectExpression.Projection[projectionIndex];
                    var nullable = IsNullableProjection(projection);

                    var valueParameter = Expression.Parameter(projectionBindingExpression.Type);
                    _variables.Add(valueParameter);

                    _expressions.Add(
                        Expression.Assign(
                            valueParameter,
                            CreateGetValueExpression(
                                _dataReaderParameter,
                                projectionIndex,
                                nullable,
                                projection.Expression.TypeMapping!,
                                valueParameter.Type)));

                    if (_containsCollectionMaterialization)
                    {
                        var expressionToAdd = (Expression)valueParameter;
                        if (expressionToAdd.Type.IsValueType)
                        {
                            expressionToAdd = Expression.Convert(expressionToAdd, typeof(object));
                        }

                        _valuesArrayInitializers!.Add(expressionToAdd);
                        accessor = Expression.Convert(
                            Expression.ArrayIndex(
                                _valuesArrayExpression!,
                                Expression.Constant(_valuesArrayInitializers.Count - 1)),
                            projectionBindingExpression.Type);
                    }
                    else
                    {
                        accessor = valueParameter;
                    }

                    _variableShaperMapping[projectionBindingExpression] = accessor;

                    return accessor;
                }

                case IncludeExpression includeExpression:
                {
                    var entity = Visit(includeExpression.EntityExpression);
                    if (includeExpression.NavigationExpression is RelationalCollectionShaperExpression
                        relationalCollectionShaperExpression)
                    {
                        var collectionIdConstant = Expression.Constant(_collectionId++);
                        var innerShaper = new ShaperProcessingExpressionVisitor(
                                _parentVisitor, _resultCoordinatorParameter, _selectExpression, _dataReaderParameter,
                                _resultContextParameter,
                                _readerColumns)
                            .ProcessShaper(relationalCollectionShaperExpression.InnerShaper, out _, out _, out _, ref _collectionId);

                        var entityType = entity.Type;
                        var navigation = includeExpression.Navigation;
                        var includingEntityType = navigation.DeclaringEntityType.ClrType;
                        if (includingEntityType != entityType
                            && includingEntityType.IsAssignableFrom(entityType))
                        {
                            includingEntityType = entityType;
                        }

                        _inline = true;

                        var parentIdentifierLambda = Expression.Lambda(
                            Visit(relationalCollectionShaperExpression.ParentIdentifier),
                            QueryCompilationContext.QueryContextParameter,
                            _dataReaderParameter);

                        var outerIdentifierLambda = Expression.Lambda(
                            Visit(relationalCollectionShaperExpression.OuterIdentifier),
                            QueryCompilationContext.QueryContextParameter,
                            _dataReaderParameter);

                        var selfIdentifierLambda = Expression.Lambda(
                            Visit(relationalCollectionShaperExpression.SelfIdentifier),
                            QueryCompilationContext.QueryContextParameter,
                            _dataReaderParameter);

                        _inline = false;

                        _includeExpressions.Add(
                            Expression.Call(
                                InitializeIncludeCollectionMethodInfo.MakeGenericMethod(entityType, includingEntityType),
                                collectionIdConstant,
                                QueryCompilationContext.QueryContextParameter,
                                _dataReaderParameter,
                                _resultCoordinatorParameter,
                                entity,
                                Expression.Constant(parentIdentifierLambda.Compile()),
                                Expression.Constant(outerIdentifierLambda.Compile()),
                                Expression.Constant(navigation),
                                Expression.Constant(navigation.IsShadowProperty()
                                    ? null
                                    : navigation.GetCollectionAccessor(), typeof(IClrCollectionAccessor)),
                                Expression.Constant(_isTracking),
#pragma warning disable EF1001 // Internal EF Core API usage.
                                Expression.Constant(includeExpression.SetLoaded)));
#pragma warning restore EF1001 // Internal EF Core API usage.

                        var relatedEntityType = innerShaper.ReturnType;
                        var inverseNavigation = navigation.Inverse;

                        _collectionPopulatingExpressions!.Add(
                            Expression.Call(
                                PopulateIncludeCollectionMethodInfo.MakeGenericMethod(includingEntityType, relatedEntityType),
                                collectionIdConstant,
                                QueryCompilationContext.QueryContextParameter,
                                _dataReaderParameter,
                                _resultCoordinatorParameter,
                                Expression.Constant(parentIdentifierLambda.Compile()),
                                Expression.Constant(outerIdentifierLambda.Compile()),
                                Expression.Constant(selfIdentifierLambda.Compile()),
                                Expression.Constant(
                                    relationalCollectionShaperExpression.ParentIdentifierValueComparers,
                                    typeof(IReadOnlyList<ValueComparer>)),
                                Expression.Constant(
                                    relationalCollectionShaperExpression.OuterIdentifierValueComparers,
                                    typeof(IReadOnlyList<ValueComparer>)),
                                Expression.Constant(
                                    relationalCollectionShaperExpression.SelfIdentifierValueComparers,
                                    typeof(IReadOnlyList<ValueComparer>)),
                                Expression.Constant(innerShaper.Compile()),
                                Expression.Constant(inverseNavigation, typeof(INavigationBase)),
                                Expression.Constant(
                                    GenerateFixup(
                                        includingEntityType, relatedEntityType, navigation, inverseNavigation).Compile()),
                                Expression.Constant(_isTracking)));
                    }
                    else if (includeExpression.NavigationExpression is RelationalSplitCollectionShaperExpression
                             relationalSplitCollectionShaperExpression)
                    {
                        var collectionIdConstant = Expression.Constant(_collectionId++);
                        var innerProcessor = new ShaperProcessingExpressionVisitor(
                            _parentVisitor, _resultCoordinatorParameter,
                            _executionStrategyParameter!, relationalSplitCollectionShaperExpression.SelectExpression, _tags!);
                        var innerShaper = innerProcessor.ProcessShaper(
                            relationalSplitCollectionShaperExpression.InnerShaper,
                            out var relationalCommandCache,
                            out var readerColumns,
                            out var relatedDataLoaders,
                            ref _collectionId);

                        var entityType = entity.Type;
                        var navigation = includeExpression.Navigation;
                        var includingEntityType = navigation.DeclaringEntityType.ClrType;
                        if (includingEntityType != entityType
                            && includingEntityType.IsAssignableFrom(entityType))
                        {
                            includingEntityType = entityType;
                        }

                        _inline = true;

                        var parentIdentifierLambda = Expression.Lambda(
                            Visit(relationalSplitCollectionShaperExpression.ParentIdentifier),
                            QueryCompilationContext.QueryContextParameter,
                            _dataReaderParameter);

                        _inline = false;

                        innerProcessor._inline = true;

                        var childIdentifierLambda = Expression.Lambda(
                            innerProcessor.Visit(relationalSplitCollectionShaperExpression.ChildIdentifier),
                            QueryCompilationContext.QueryContextParameter,
                            innerProcessor._dataReaderParameter);

                        innerProcessor._inline = false;

                        _includeExpressions.Add(
                            Expression.Call(
                                InitializeSplitIncludeCollectionMethodInfo.MakeGenericMethod(entityType, includingEntityType),
                                collectionIdConstant,
                                QueryCompilationContext.QueryContextParameter,
                                _dataReaderParameter,
                                _resultCoordinatorParameter,
                                entity,
                                Expression.Constant(parentIdentifierLambda.Compile()),
                                Expression.Constant(navigation),
                                Expression.Constant(navigation.GetCollectionAccessor()),
                                Expression.Constant(_isTracking),
#pragma warning disable EF1001 // Internal EF Core API usage.
                                Expression.Constant(includeExpression.SetLoaded)));
#pragma warning restore EF1001 // Internal EF Core API usage.

                        var relatedEntityType = innerShaper.ReturnType;
                        var inverseNavigation = navigation.Inverse;

                        _collectionPopulatingExpressions!.Add(
                            Expression.Call(
                                (_isAsync ? PopulateSplitIncludeCollectionAsyncMethodInfo : PopulateSplitIncludeCollectionMethodInfo)
                                .MakeGenericMethod(includingEntityType, relatedEntityType),
                                collectionIdConstant,
                                Expression.Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                                _executionStrategyParameter!,
                                Expression.Constant(relationalCommandCache),
                                Expression.Constant(readerColumns, typeof(IReadOnlyList<ReaderColumn?>)),
                                Expression.Constant(_detailedErrorsEnabled),
                                _resultCoordinatorParameter,
                                Expression.Constant(childIdentifierLambda.Compile()),
                                Expression.Constant(
                                    relationalSplitCollectionShaperExpression.IdentifierValueComparers,
                                    typeof(IReadOnlyList<ValueComparer>)),
                                Expression.Constant(innerShaper.Compile()),
                                Expression.Constant(
                                    relatedDataLoaders?.Compile(),
                                    _isAsync
                                        ? typeof(Func<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator, Task>)
                                        : typeof(Action<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator>)),
                                Expression.Constant(inverseNavigation, typeof(INavigationBase)),
                                Expression.Constant(
                                    GenerateFixup(
                                        includingEntityType, relatedEntityType, navigation, inverseNavigation).Compile()),
                                Expression.Constant(_isTracking)));
                    }
                    else
                    {
                        var projectionBindingExpression = (includeExpression.NavigationExpression as CollectionResultExpression)
                            ?.ProjectionBindingExpression
                            ?? (includeExpression.NavigationExpression as RelationalEntityShaperExpression)?.ValueBufferExpression as
                            ProjectionBindingExpression;

                        // json include case
                        if (projectionBindingExpression != null
                            && GetProjectionIndex(projectionBindingExpression) is JsonProjectionInfo jsonProjectionInfo)
                        {
                            var (jsonElementParameter, keyValuesParameter) = JsonShapingPreProcess(
                                jsonProjectionInfo,
                                includeExpression.Navigation.TargetEntityType);

                            var shaperResult = CreateJsonShapers(
                                includeExpression.Navigation.TargetEntityType,
                                nullable: true,
                                collection: includeExpression.NavigationExpression is CollectionResultExpression,
                                jsonElementParameter,
                                keyValuesParameter,
                                parentEntityExpression: entity,
                                navigation: (INavigation)includeExpression.Navigation);

                            var visitedShaperResult = Visit(shaperResult);

                            _includeExpressions.Add(visitedShaperResult);

                            return entity;
                        }

                        var navigationExpression = Visit(includeExpression.NavigationExpression);
                        var entityType = entity.Type;
                        var navigation = includeExpression.Navigation;
                        var includingType = navigation.DeclaringEntityType.ClrType;
                        var inverseNavigation = navigation.Inverse;
                        var relatedEntityType = navigation.TargetEntityType.ClrType;
                        if (includingType != entityType
                            && includingType.IsAssignableFrom(entityType))
                        {
                            includingType = entityType;
                        }

                        var updatedExpression = Expression.Call(
                            IncludeReferenceMethodInfo.MakeGenericMethod(entityType, includingType, relatedEntityType),
                            QueryCompilationContext.QueryContextParameter,
                            entity,
                            navigationExpression,
                            Expression.Constant(navigation),
                            Expression.Constant(inverseNavigation, typeof(INavigationBase)),
                            Expression.Constant(
                                GenerateFixup(
                                    includingType, relatedEntityType, navigation, inverseNavigation).Compile()),
                            Expression.Constant(_isTracking));

                        _includeExpressions.Add(updatedExpression);
                    }

                    return entity;
                }

                case RelationalCollectionShaperExpression relationalCollectionShaperExpression:
                {
                    if (!_variableShaperMapping.TryGetValue(relationalCollectionShaperExpression, out var accessor))
                    {
                        var collectionIdConstant = Expression.Constant(_collectionId++);
                        var innerShaper = new ShaperProcessingExpressionVisitor(
                                _parentVisitor, _resultCoordinatorParameter, _selectExpression, _dataReaderParameter,
                                _resultContextParameter,
                                _readerColumns)
                            .ProcessShaper(relationalCollectionShaperExpression.InnerShaper, out _, out _, out _, ref _collectionId);

                        var navigation = relationalCollectionShaperExpression.Navigation;
                        var collectionAccessor = navigation?.GetCollectionAccessor();
                        var collectionType = collectionAccessor?.CollectionType ?? relationalCollectionShaperExpression.Type;
                        var elementType = relationalCollectionShaperExpression.ElementType;
                        var relatedElementType = innerShaper.ReturnType;

                        _inline = true;

                        var parentIdentifierLambda = Expression.Lambda(
                            Visit(relationalCollectionShaperExpression.ParentIdentifier),
                            QueryCompilationContext.QueryContextParameter,
                            _dataReaderParameter);

                        var outerIdentifierLambda = Expression.Lambda(
                            Visit(relationalCollectionShaperExpression.OuterIdentifier),
                            QueryCompilationContext.QueryContextParameter,
                            _dataReaderParameter);

                        var selfIdentifierLambda = Expression.Lambda(
                            Visit(relationalCollectionShaperExpression.SelfIdentifier),
                            QueryCompilationContext.QueryContextParameter,
                            _dataReaderParameter);

                        _inline = false;

                        var collectionParameter = Expression.Parameter(relationalCollectionShaperExpression.Type);
                        _variables.Add(collectionParameter);
                        _expressions.Add(
                            Expression.Assign(
                                collectionParameter,
                                Expression.Call(
                                    InitializeCollectionMethodInfo.MakeGenericMethod(elementType, collectionType),
                                    collectionIdConstant,
                                    QueryCompilationContext.QueryContextParameter,
                                    _dataReaderParameter,
                                    _resultCoordinatorParameter,
                                    Expression.Constant(parentIdentifierLambda.Compile()),
                                    Expression.Constant(outerIdentifierLambda.Compile()),
                                    Expression.Constant(collectionAccessor, typeof(IClrCollectionAccessor)))));

                        _valuesArrayInitializers!.Add(collectionParameter);
                        accessor = Expression.Convert(
                            Expression.ArrayIndex(
                                _valuesArrayExpression!,
                                Expression.Constant(_valuesArrayInitializers.Count - 1)),
                            relationalCollectionShaperExpression.Type);

                        _collectionPopulatingExpressions!.Add(
                            Expression.Call(
                                PopulateCollectionMethodInfo.MakeGenericMethod(collectionType, elementType, relatedElementType),
                                collectionIdConstant,
                                QueryCompilationContext.QueryContextParameter,
                                _dataReaderParameter,
                                _resultCoordinatorParameter,
                                Expression.Constant(parentIdentifierLambda.Compile()),
                                Expression.Constant(outerIdentifierLambda.Compile()),
                                Expression.Constant(selfIdentifierLambda.Compile()),
                                Expression.Constant(
                                    relationalCollectionShaperExpression.ParentIdentifierValueComparers,
                                    typeof(IReadOnlyList<ValueComparer>)),
                                Expression.Constant(
                                    relationalCollectionShaperExpression.OuterIdentifierValueComparers,
                                    typeof(IReadOnlyList<ValueComparer>)),
                                Expression.Constant(
                                    relationalCollectionShaperExpression.SelfIdentifierValueComparers,
                                    typeof(IReadOnlyList<ValueComparer>)),
                                Expression.Constant(innerShaper.Compile())));

                        _variableShaperMapping[relationalCollectionShaperExpression] = accessor;
                    }

                    return accessor;
                }

                case RelationalSplitCollectionShaperExpression relationalSplitCollectionShaperExpression:
                {
                    if (!_variableShaperMapping.TryGetValue(relationalSplitCollectionShaperExpression, out var accessor))
                    {
                        var collectionIdConstant = Expression.Constant(_collectionId++);
                        var innerProcessor = new ShaperProcessingExpressionVisitor(
                            _parentVisitor, _resultCoordinatorParameter,
                            _executionStrategyParameter!, relationalSplitCollectionShaperExpression.SelectExpression, _tags!);
                        var innerShaper = innerProcessor.ProcessShaper(
                            relationalSplitCollectionShaperExpression.InnerShaper,
                            out var relationalCommandCache,
                            out var readerColumns,
                            out var relatedDataLoaders,
                            ref _collectionId);

                        var navigation = relationalSplitCollectionShaperExpression.Navigation;
                        var collectionAccessor = navigation?.GetCollectionAccessor();
                        var collectionType = collectionAccessor?.CollectionType ?? relationalSplitCollectionShaperExpression.Type;
                        var elementType = relationalSplitCollectionShaperExpression.ElementType;
                        var relatedElementType = innerShaper.ReturnType;

                        _inline = true;

                        var parentIdentifierLambda = Expression.Lambda(
                            Visit(relationalSplitCollectionShaperExpression.ParentIdentifier),
                            QueryCompilationContext.QueryContextParameter,
                            _dataReaderParameter);

                        _inline = false;

                        innerProcessor._inline = true;

                        var childIdentifierLambda = Expression.Lambda(
                            innerProcessor.Visit(relationalSplitCollectionShaperExpression.ChildIdentifier),
                            QueryCompilationContext.QueryContextParameter,
                            innerProcessor._dataReaderParameter);

                        innerProcessor._inline = false;

                        var collectionParameter = Expression.Parameter(collectionType);
                        _variables.Add(collectionParameter);
                        _expressions.Add(
                            Expression.Assign(
                                collectionParameter,
                                Expression.Call(
                                    InitializeSplitCollectionMethodInfo.MakeGenericMethod(elementType, collectionType),
                                    collectionIdConstant,
                                    QueryCompilationContext.QueryContextParameter,
                                    _dataReaderParameter,
                                    _resultCoordinatorParameter,
                                    Expression.Constant(parentIdentifierLambda.Compile()),
                                    Expression.Constant(collectionAccessor, typeof(IClrCollectionAccessor)))));

                        _valuesArrayInitializers!.Add(collectionParameter);
                        accessor = Expression.Convert(
                            Expression.ArrayIndex(
                                _valuesArrayExpression!,
                                Expression.Constant(_valuesArrayInitializers.Count - 1)),
                            relationalSplitCollectionShaperExpression.Type);

                        _collectionPopulatingExpressions!.Add(
                            Expression.Call(
                                (_isAsync ? PopulateSplitCollectionAsyncMethodInfo : PopulateSplitCollectionMethodInfo)
                                .MakeGenericMethod(collectionType, elementType, relatedElementType),
                                collectionIdConstant,
                                Expression.Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                                _executionStrategyParameter!,
                                Expression.Constant(relationalCommandCache),
                                Expression.Constant(readerColumns, typeof(IReadOnlyList<ReaderColumn?>)),
                                Expression.Constant(_detailedErrorsEnabled),
                                _resultCoordinatorParameter,
                                Expression.Constant(childIdentifierLambda.Compile()),
                                Expression.Constant(
                                    relationalSplitCollectionShaperExpression.IdentifierValueComparers,
                                    typeof(IReadOnlyList<ValueComparer>)),
                                Expression.Constant(innerShaper.Compile()),
                                Expression.Constant(
                                    relatedDataLoaders?.Compile(),
                                    _isAsync
                                        ? typeof(Func<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator, Task>)
                                        : typeof(Action<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator>))));

                        _variableShaperMapping[relationalSplitCollectionShaperExpression] = accessor;
                    }

                    return accessor;
                }

                case GroupByShaperExpression:
                    throw new InvalidOperationException(RelationalStrings.ClientGroupByNotSupported);
            }

            return base.VisitExtension(extensionExpression);

            Expression CompensateForCollectionMaterialization(ParameterExpression parameter, Type resultType)
            {
                if (_containsCollectionMaterialization)
                {
                    _valuesArrayInitializers!.Add(parameter);
                    return Expression.Convert(
                        Expression.ArrayIndex(
                            _valuesArrayExpression!,
                            Expression.Constant(_valuesArrayInitializers.Count - 1)),
                        resultType);
                }
                else
                {
                    return parameter;
                }
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition()
                == Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod)
            {
                var index = methodCallExpression.Arguments[1].GetConstantValue<int>();
                var property = methodCallExpression.Arguments[2].GetConstantValue<IProperty?>();
                var mappingParameter = (ParameterExpression)((MethodCallExpression)methodCallExpression.Arguments[0]).Object!;

                if (_jsonMaterializationContextParameterMapping.ContainsKey(mappingParameter))
                {
                    var (jsonElementParameter, keyPropertyValuesParameter) = _jsonMaterializationContextParameterMapping[mappingParameter];

                    return property!.IsPrimaryKey()
                        ? Expression.MakeIndex(
                            keyPropertyValuesParameter,
                            ObjectArrayIndexerPropertyInfo,
                            new[] { Expression.Constant(index) })
                        : CreateExtractJsonPropertyExpression(jsonElementParameter, property);
                }

                int projectionIndex;
                if (property == null)
                {
                    // This is trying to read the computed discriminator value
                    var storedInfo = _entityTypeIdentifyingExpressionInfo[mappingParameter];
                    if (storedInfo is string s)
                    {
                        // If the value is fixed then there is single entity type and discriminator is not present in query
                        // We just return the value as-is.
                        return Expression.Constant(s);
                    }

                    projectionIndex = (int)_entityTypeIdentifyingExpressionInfo[mappingParameter] + index;
                }
                else
                {
                    projectionIndex = _materializationContextBindings[mappingParameter][property];
                }

                var projection = _selectExpression.Projection[projectionIndex];
                var nullable = IsNullableProjection(projection);

                Check.DebugAssert(
                    !nullable || property != null || methodCallExpression.Type.IsNullableType(),
                    "For nullable reads the return type must be null unless property is specified.");

                return CreateGetValueExpression(
                    _dataReaderParameter,
                    projectionIndex,
                    nullable,
                    projection.Expression.TypeMapping!,
                    methodCallExpression.Type,
                    property);
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        private Expression CreateJsonShapers(
            IEntityType entityType,
            bool nullable,
            bool collection,
            ParameterExpression jsonElementParameter,
            ParameterExpression keyValuesParameter,
            Expression? parentEntityExpression,
            INavigation? navigation)
        {
            var jsonElementShaperLambdaParameter = Expression.Parameter(typeof(JsonElement));
            var keyValuesShaperLambdaParameter = Expression.Parameter(typeof(object[]));
            var shaperBlockVariables = new List<ParameterExpression>();
            var shaperBlockExpressions = new List<Expression>();

            var valueBufferParameter = Expression.Parameter(typeof(ValueBuffer));

            _jsonValueBufferParameterMapping[valueBufferParameter] = (jsonElementShaperLambdaParameter, keyValuesShaperLambdaParameter);

            var entityShaperExpression = new RelationalEntityShaperExpression(
                entityType,
                valueBufferParameter,
                nullable);

            var entityShaperMaterializer = (BlockExpression)_parentVisitor.InjectEntityMaterializers(entityShaperExpression);
            var entityShaperMaterializerVariable = Expression.Variable(entityShaperMaterializer.Type);
            shaperBlockVariables.Add(entityShaperMaterializerVariable);
            shaperBlockExpressions.Add(Expression.Assign(entityShaperMaterializerVariable, entityShaperMaterializer));

            foreach (var ownedNavigation in entityType.GetNavigations().Where(
                         n => n.TargetEntityType.IsMappedToJson() && n.ForeignKey.IsOwnership && n == n.ForeignKey.PrincipalToDependent))
            {
                // TODO: use caching like we do in pre-process, there's chance we already have this json element
                var innerJsonElementParameter = Expression.Variable(
                    typeof(JsonElement?));

                shaperBlockVariables.Add(innerJsonElementParameter);

                // JsonElement temp;
                // JsonElement? innerJsonElement = jsonElement.TryGetProperty("PropertyName", temp)
                //  ? (JsonElement?)temp
                //  : null;
                var tempParameter = Expression.Variable(typeof(JsonElement));
                shaperBlockVariables.Add(tempParameter);

                var innerJsonElementAssignment = Expression.Assign(
                    innerJsonElementParameter,
                    Expression.Condition(
                        Expression.Call(
                            jsonElementShaperLambdaParameter,
                            JsonElementTryGetPropertyMethod,
                            Expression.Constant(ownedNavigation.TargetEntityType.GetJsonPropertyName()),
                            tempParameter),
                        Expression.Convert(
                            tempParameter,
                            typeof(JsonElement?)),
                        Expression.Constant(null, typeof(JsonElement?))));

                shaperBlockExpressions.Add(innerJsonElementAssignment);

                var innerShaperResult = CreateJsonShapers(
                    ownedNavigation.TargetEntityType,
                    nullable || !ownedNavigation.ForeignKey.IsRequired,
                    ownedNavigation.IsCollection,
                    innerJsonElementParameter,
                    keyValuesShaperLambdaParameter,
                    entityShaperMaterializerVariable,
                    ownedNavigation);

                shaperBlockExpressions.Add(innerShaperResult);
            }

            shaperBlockExpressions.Add(entityShaperMaterializerVariable);

            var shaperBlock = Expression.Block(
                shaperBlockVariables,
                shaperBlockExpressions);

            var shaperLambda = Expression.Lambda(
                shaperBlock,
                QueryCompilationContext.QueryContextParameter,
                keyValuesShaperLambdaParameter,
                jsonElementShaperLambdaParameter);

            if (parentEntityExpression != null)
            {
                Check.DebugAssert(navigation != null, "Navigation shouldn't be null when including.");

                var fixup = GenerateFixup(
                    navigation.DeclaringEntityType.ClrType,
                    navigation.TargetEntityType.ClrType,
                    navigation,
                    navigation.Inverse);

                // inheritance scenario - navigation defined on derived
                var includingEntityExpression = parentEntityExpression.Type != navigation.DeclaringEntityType.ClrType
                    ? Expression.Convert(parentEntityExpression, navigation.DeclaringEntityType.ClrType)
                    : parentEntityExpression;

                if (navigation.IsCollection)
                {
                    var includeJsonEntityCollectionMethodCall =
                        Expression.Call(
                            IncludeJsonEntityCollectionMethodInfo.MakeGenericMethod(
                                navigation.DeclaringEntityType.ClrType,
                                navigation.TargetEntityType.ClrType),
                            QueryCompilationContext.QueryContextParameter,
                            jsonElementParameter,
                            keyValuesParameter,
                            includingEntityExpression,
                            shaperLambda,
                            fixup);

                    return navigation.DeclaringEntityType.ClrType.IsAssignableFrom(parentEntityExpression.Type)
                        ? includeJsonEntityCollectionMethodCall
                        : Expression.IfThen(
                            Expression.TypeIs(
                                parentEntityExpression,
                                navigation.DeclaringEntityType.ClrType),
                            includeJsonEntityCollectionMethodCall);
                }

                var includeJsonEntityReferenceMethodCall =
                    Expression.Call(
                        IncludeJsonEntityReferenceMethodInfo.MakeGenericMethod(
                            navigation.DeclaringEntityType.ClrType,
                            navigation.TargetEntityType.ClrType),
                        QueryCompilationContext.QueryContextParameter,
                        jsonElementParameter,
                        keyValuesParameter,
                        includingEntityExpression,
                        shaperLambda,
                        fixup);

                return navigation.DeclaringEntityType.ClrType.IsAssignableFrom(parentEntityExpression.Type)
                    ? includeJsonEntityReferenceMethodCall
                    : Expression.IfThen(
                        Expression.TypeIs(
                            parentEntityExpression,
                            navigation.DeclaringEntityType.ClrType),
                        includeJsonEntityReferenceMethodCall);
            }

            if (collection)
            {
                Check.DebugAssert(navigation != null, "navigation shouldn't be null when materializing collection.");

                var materializeJsonEntityCollection = Expression.Call(
                    MaterializeJsonEntityCollectionMethodInfo.MakeGenericMethod(
                        entityType.ClrType,
                        navigation.ClrType),
                    QueryCompilationContext.QueryContextParameter,
                    jsonElementParameter,
                    keyValuesParameter,
                    Expression.Constant(navigation),
                    shaperLambda);

                return materializeJsonEntityCollection;
            }

            var materializedRootJsonEntity = Expression.Call(
                MaterializeJsonEntityMethodInfo.MakeGenericMethod(entityType.ClrType),
                QueryCompilationContext.QueryContextParameter,
                jsonElementParameter,
                keyValuesParameter,
                Expression.Constant(nullable),
                shaperLambda);

            return materializedRootJsonEntity;
        }

        private (ParameterExpression, ParameterExpression) JsonShapingPreProcess(
            JsonProjectionInfo jsonProjectionInfo,
            IEntityType entityType)
        {
            if (_existingJsonElementMap.TryGetValue(
                (jsonProjectionInfo.JsonColumnIndex, jsonProjectionInfo.AdditionalPath),
                out var finalJsonElementVariable))
            {
                // if we already cached JsonElement then key values are guaranteed to have been cached also, as they go in tandem
                var fullPathCacheKey = jsonProjectionInfo.AdditionalPath.Select(x => (x.ConstantArrayIndex, x.NonConstantArrayIndex)).ToArray();
                var finalKeyValuesVariable = _existingKeyValuesMap[(jsonProjectionInfo.JsonColumnIndex, fullPathCacheKey)];

                // if the JsonElement variable for the full path is present in the cache,
                // it means we already went through this process before
                // and have already generated all the steps leading to the result
                // i.e. we can safely return from the pre process
                return (finalJsonElementVariable, finalKeyValuesVariable);
            }

            var currentJsonElementVariable = default(ParameterExpression);
            var currentKeyValuesVariable = default(ParameterExpression);
            var additionalKeyGeneratedCount = 0;

            // go through each segment in the additional path and generate JsonElement and key values
            // store them in variables and cache them, so we can re-use it later if needed
            // JsonElement needs to be generated for every path segment, as they are always different
            // key values only changes if we access element of the array (as opposed to JSON property access)
            for (var index = 0; index <= jsonProjectionInfo.AdditionalPath.Length; index++)
            {
                var jsonElementCacheKey = jsonProjectionInfo.AdditionalPath[..index];
                var keyValuesCacheKey = jsonProjectionInfo.AdditionalPath[..index].Select(x => (x.ConstantArrayIndex, x.NonConstantArrayIndex)).ToArray();

                if (_existingJsonElementMap.TryGetValue(
                    (jsonProjectionInfo.JsonColumnIndex, jsonElementCacheKey),
                    out var existingJsonElementVariable))
                {
                    currentJsonElementVariable = existingJsonElementVariable;
                    currentKeyValuesVariable = _existingKeyValuesMap[(jsonProjectionInfo.JsonColumnIndex, keyValuesCacheKey)];

                    continue;
                }

                if (index == 0)
                {
                    var jsonColumnName = entityType.GetContainerColumnName()!;
                    var jsonColumnTypeMapping = (entityType.GetViewOrTableMappings().SingleOrDefault()?.Table
                            ?? entityType.GetDefaultMappings().Single().Table)
                        .FindColumn(jsonColumnName)!.StoreTypeMapping;

                    // create the JsonElement for the initial entity
                    var jsonElementValueExpression = CreateGetValueExpression(
                        _dataReaderParameter,
                        jsonProjectionInfo.JsonColumnIndex,
                        nullable: true,
                        jsonColumnTypeMapping,
                        typeof(JsonElement?),
                        property: null);

                    currentJsonElementVariable = Expression.Variable(
                        typeof(JsonElement?));

                    var jsonElementAssignment = Expression.Assign(
                        currentJsonElementVariable,
                        jsonElementValueExpression);

                    _variables.Add(currentJsonElementVariable);
                    _expressions.Add(jsonElementAssignment);

                    var keyValues = new Expression[jsonProjectionInfo.KeyAccessInfo.Count];
                    for (var i = 0; i < jsonProjectionInfo.KeyAccessInfo.Count; i++)
                    {
                        if (jsonProjectionInfo.KeyAccessInfo[i].ConstantKeyValue is int constant)
                        {
                            // if key access was a constant (and we have the actual value) add it directly to key values array
                            // adding 1 to the value as we start keys from 1 and the array starts at 0
                            keyValues[i] = Expression.Convert(
                                Expression.Constant(constant + 1),
                                typeof(object));
                        }
                        else if (jsonProjectionInfo.KeyAccessInfo[i].KeyProperty is IProperty keyProperty)
                        {
                            // if key value has IProperty, it must be a PK of the owner
                            var projection = _selectExpression.Projection[jsonProjectionInfo.KeyAccessInfo[i].KeyProjectionIndex!.Value];
                            keyValues[i] = Expression.Convert(
                                CreateGetValueExpression(
                                    _dataReaderParameter,
                                    jsonProjectionInfo.KeyAccessInfo[i].KeyProjectionIndex!.Value,
                                    IsNullableProjection(projection),
                                    projection.Expression.TypeMapping!,
                                    keyProperty.ClrType,
                                    keyProperty),
                                typeof(object));
                        }
                        else
                        {
                            // otherwise it must be non-constant array access and we stored it's projection index
                            // extract the value from the projection (or the cache if we used it before)
                            var collectionElementAccessParameter = ExtractAndCacheNonConstantJsonArrayElementAccessValue(
                                jsonProjectionInfo.KeyAccessInfo[i].KeyProjectionIndex!.Value);

                            keyValues[i] = Expression.Convert(
                                Expression.Add(collectionElementAccessParameter, Expression.Constant(1, typeof(int?))),
                                typeof(object));
                        }
                    }

                    // create key values for initial entity
                    currentKeyValuesVariable = Expression.Parameter(typeof(object[]));
                    var keyValuesAssignment = Expression.Assign(
                        currentKeyValuesVariable,
                        Expression.NewArrayInit(typeof(object), keyValues));

                    _variables.Add(currentKeyValuesVariable);
                    _expressions.Add(keyValuesAssignment);

                    _existingJsonElementMap[(jsonProjectionInfo.JsonColumnIndex, jsonElementCacheKey)] = currentJsonElementVariable;
                    _existingKeyValuesMap[(jsonProjectionInfo.JsonColumnIndex, keyValuesCacheKey)] = currentKeyValuesVariable;
                }
                else
                {
                    // create JsonElement for the additional path segment
                    var currentPath = jsonProjectionInfo.AdditionalPath[index - 1];

                    Expression jsonElementAccessExpressionFragment;
                    if (currentPath.JsonPropertyName is string stringPath)
                    {
                        // JsonElement? jsonElement = (...) <- this is the previous one
                        // JsonElement temp;
                        // JsonElement? newJsonElement = jsonElement.HasValue && jsonElement.Value.TryGetProperty("PropertyName", temp)
                        //   ? (JsonElement?)temp
                        //   : null;
                        var tempParameter = Expression.Variable(typeof(JsonElement));
                        _variables.Add(tempParameter);

                        var tryGetPropertyCall = Expression.Call(
                            Expression.MakeMemberAccess(
                                currentJsonElementVariable!,
                                NullableJsonElementValuePropertyInfo),
                            JsonElementTryGetPropertyMethod,
                            Expression.Constant(stringPath),
                            tempParameter);

                        var newJsonElementVariable = Expression.Variable(
                            typeof(JsonElement?));

                        var newJsonElementAssignment = Expression.Assign(
                            newJsonElementVariable,
                            Expression.Condition(
                                Expression.AndAlso(
                                    Expression.MakeMemberAccess(
                                        currentJsonElementVariable!,
                                        NullableJsonElementHasValuePropertyInfo),
                                    tryGetPropertyCall),
                                Expression.Convert(tempParameter, typeof(JsonElement?)),
                                Expression.Constant(null, typeof(JsonElement?))));

                        _variables.Add(newJsonElementVariable);
                        _expressions.Add(newJsonElementAssignment);

                        currentJsonElementVariable = newJsonElementVariable;
                    }
                    else
                    {
                        var elementAccessExpression = currentPath.ConstantArrayIndex is int constantElementAccess
                            ? (Expression)Expression.Constant(constantElementAccess)
                            : Expression.Convert(
                                ExtractAndCacheNonConstantJsonArrayElementAccessValue(currentPath.NonConstantArrayIndex!.Value),
                                typeof(int));

                        jsonElementAccessExpressionFragment = Expression.Call(
                            Expression.MakeMemberAccess(
                                currentJsonElementVariable!,
                                NullableJsonElementValuePropertyInfo),
                            JsonElementGetItemMethodInfo,
                            elementAccessExpression);

                        additionalKeyGeneratedCount++;
                        if (_existingKeyValuesMap.TryGetValue((jsonProjectionInfo.JsonColumnIndex, keyValuesCacheKey), out var existingKeyValuesVariable))
                        {
                            currentKeyValuesVariable = existingKeyValuesVariable;
                        }
                        else
                        {
                            // create new array of size 1 more than current array (as we will be adding the extra key value)
                            // copy values from current array and set the last remaining value
                            var previousKeyValuesVariable = currentKeyValuesVariable;
                            currentKeyValuesVariable = Expression.Parameter(typeof(object[]));

                            var currentKeyValuesCount = jsonProjectionInfo.KeyAccessInfo.Count
                                + additionalKeyGeneratedCount;

                            var currentKeyValuesArrayInitAssignment = Expression.Assign(
                                currentKeyValuesVariable,
                                Expression.NewArrayBounds(
                                    typeof(object),
                                    Expression.Constant(currentKeyValuesCount)));

                            var keyValuesArrayCopyFromPrevious = Expression.Call(
                                ArrayCopyMethodInfo,
                                previousKeyValuesVariable!,
                                currentKeyValuesVariable,
                                Expression.Constant(currentKeyValuesCount - 1));

                            var missingKeyValueAssignment = Expression.Assign(
                                Expression.MakeIndex(
                                    currentKeyValuesVariable,
                                    ObjectArrayIndexerPropertyInfo,
                                    new[] { Expression.Constant(currentKeyValuesCount - 1) }),
                                Expression.Convert(
                                    Expression.Add(elementAccessExpression, Expression.Constant(1)),
                                    typeof(object)));

                            _variables.Add(currentKeyValuesVariable);
                            _expressions.Add(currentKeyValuesArrayInitAssignment);
                            _expressions.Add(keyValuesArrayCopyFromPrevious);
                            _expressions.Add(missingKeyValueAssignment);
                        }

                        var jsonElementValueExpression = Expression.Condition(
                            Expression.MakeMemberAccess(
                                currentJsonElementVariable,
                                NullableJsonElementHasValuePropertyInfo),
                            Expression.Convert(
                                jsonElementAccessExpressionFragment,
                                currentJsonElementVariable!.Type),
                            Expression.Default(currentJsonElementVariable.Type));

                        currentJsonElementVariable = Expression.Variable(
                            typeof(JsonElement?));

                        var jsonElementAssignment = Expression.Assign(
                            currentJsonElementVariable,
                            jsonElementValueExpression);

                        _variables.Add(currentJsonElementVariable);
                        _expressions.Add(jsonElementAssignment);
                    }
                }
            }

            return (currentJsonElementVariable!, currentKeyValuesVariable!);

            ParameterExpression ExtractAndCacheNonConstantJsonArrayElementAccessValue(int index)
            {
                if (!_jsonArrayNonConstantElementAccessMap.TryGetValue(index, out var arrayElementAccessParameter))
                {
                    arrayElementAccessParameter = Expression.Parameter(typeof(int?));
                    var projection = _selectExpression.Projection[index];

                    var arrayElementAccessValue = CreateGetValueExpression(
                        _dataReaderParameter,
                        index,
                        IsNullableProjection(projection),
                        projection.Expression.TypeMapping!,
                        type: typeof(int?),
                        property: null);

                    var arrayElementAccessAssignment = Expression.Assign(
                        arrayElementAccessParameter,
                        arrayElementAccessValue);

                    _variables.Add(arrayElementAccessParameter);
                    _expressions.Add(arrayElementAccessAssignment);

                    _jsonArrayNonConstantElementAccessMap.Add(index, arrayElementAccessParameter);
                }

                return arrayElementAccessParameter;
            }
        }

        private static LambdaExpression GenerateFixup(
            Type entityType,
            Type relatedEntityType,
            INavigationBase navigation,
            INavigationBase? inverseNavigation)
        {
            var entityParameter = Expression.Parameter(entityType);
            var relatedEntityParameter = Expression.Parameter(relatedEntityType);
            var expressions = new List<Expression>();

            if (!navigation.IsShadowProperty())
            {
                expressions.Add(
                    navigation.IsCollection
                        ? AddToCollectionNavigation(entityParameter, relatedEntityParameter, navigation)
                        : AssignReferenceNavigation(entityParameter, relatedEntityParameter, navigation));
            }

            if (inverseNavigation != null
                && !inverseNavigation.IsShadowProperty())
            {
                expressions.Add(
                    inverseNavigation.IsCollection
                        ? AddToCollectionNavigation(relatedEntityParameter, entityParameter, inverseNavigation)
                        : AssignReferenceNavigation(relatedEntityParameter, entityParameter, inverseNavigation));
            }

            return Expression.Lambda(Expression.Block(typeof(void), expressions), entityParameter, relatedEntityParameter);
        }

        private static Expression AssignReferenceNavigation(
            ParameterExpression entity,
            ParameterExpression relatedEntity,
            INavigationBase navigation)
            => entity.MakeMemberAccess(navigation.GetMemberInfo(forMaterialization: true, forSet: true)).Assign(relatedEntity);

        private static Expression AddToCollectionNavigation(
            ParameterExpression entity,
            ParameterExpression relatedEntity,
            INavigationBase navigation)
            => Expression.Call(
                Expression.Constant(navigation.GetCollectionAccessor()),
                CollectionAccessorAddMethodInfo,
                entity,
                relatedEntity,
                Expression.Constant(true));

        private object GetProjectionIndex(ProjectionBindingExpression projectionBindingExpression)
            => _selectExpression.GetProjection(projectionBindingExpression).GetConstantValue<object>();

        private static bool IsNullableProjection(ProjectionExpression projection)
            => projection.Expression is not ColumnExpression column || column.IsNullable;

        private Expression CreateGetValueExpression(
            ParameterExpression dbDataReader,
            int index,
            bool nullable,
            RelationalTypeMapping typeMapping,
            Type type,
            IPropertyBase? property = null)
        {
            Check.DebugAssert(
                property != null || type.IsNullableType(), "Must read nullable value from database if property is not specified.");

            var getMethod = typeMapping.GetDataReaderMethod();

            Expression indexExpression = Expression.Constant(index);
            if (_indexMapParameter != null)
            {
                indexExpression = Expression.ArrayIndex(_indexMapParameter, indexExpression);
            }

            Expression valueExpression
                = Expression.Call(
                    getMethod.DeclaringType != typeof(DbDataReader)
                        ? Expression.Convert(dbDataReader, getMethod.DeclaringType!)
                        : dbDataReader,
                    getMethod,
                    indexExpression);

            var buffering = false;

            if (_readerColumns != null)
            {
                buffering = true;
                var columnType = valueExpression.Type;
                var bufferedColumnType = columnType;
                if (!bufferedColumnType.IsValueType
                    || !BufferedDataReader.IsSupportedValueType(bufferedColumnType))
                {
                    bufferedColumnType = typeof(object);
                }

                if (_readerColumns[index] == null)
                {
                    var bufferedReaderLambdaExpression = valueExpression;
                    if (columnType != bufferedColumnType)
                    {
                        bufferedReaderLambdaExpression = Expression.Convert(bufferedReaderLambdaExpression, bufferedColumnType);
                    }

                    _readerColumns[index] = ReaderColumn.Create(
                        bufferedColumnType,
                        nullable,
                        _indexMapParameter != null ? ((ColumnExpression)_selectExpression.Projection[index].Expression).Name : null,
                        property,
                        Expression.Lambda(
                            bufferedReaderLambdaExpression,
                            dbDataReader,
                            _indexMapParameter ?? Expression.Parameter(typeof(int[]))).Compile());
                }

                valueExpression = Expression.Call(
                    dbDataReader, RelationalTypeMapping.GetDataReaderMethod(bufferedColumnType), indexExpression);
                if (valueExpression.Type != columnType)
                {
                    valueExpression = Expression.Convert(valueExpression, columnType);
                }
            }

            valueExpression = typeMapping.CustomizeDataReaderExpression(valueExpression);

            var converter = typeMapping.Converter;

            if (converter != null)
            {
                if (valueExpression.Type != converter.ProviderClrType)
                {
                    valueExpression = Expression.Convert(valueExpression, converter.ProviderClrType);
                }

                valueExpression = ReplacingExpressionVisitor.Replace(
                    converter.ConvertFromProviderExpression.Parameters.Single(),
                    valueExpression,
                    converter.ConvertFromProviderExpression.Body);
            }

            if (valueExpression.Type != type)
            {
                valueExpression = Expression.Convert(valueExpression, type);
            }

            if (nullable)
            {
                Expression replaceExpression;
                if (converter?.ConvertsNulls == true)
                {
                    replaceExpression = ReplacingExpressionVisitor.Replace(
                        converter.ConvertFromProviderExpression.Parameters.Single(),
                        Expression.Default(converter.ProviderClrType),
                        converter.ConvertFromProviderExpression.Body);

                    if (replaceExpression.Type != type)
                    {
                        replaceExpression = Expression.Convert(replaceExpression, type);
                    }
                }
                else
                {
                    replaceExpression = Expression.Default(valueExpression.Type);
                }

                valueExpression = Expression.Condition(
                    Expression.Call(dbDataReader, IsDbNullMethod, indexExpression),
                    replaceExpression,
                    valueExpression);
            }

            if (_detailedErrorsEnabled
                && !buffering)
            {
                var exceptionParameter = Expression.Parameter(typeof(Exception), name: "e");

                var catchBlock = Expression.Catch(
                    exceptionParameter,
                    Expression.Call(
                        ThrowReadValueExceptionMethod.MakeGenericMethod(valueExpression.Type),
                        exceptionParameter,
                        Expression.Call(dbDataReader, GetFieldValueMethod.MakeGenericMethod(typeof(object)), indexExpression),
                        Expression.Constant(valueExpression.Type.MakeNullable(nullable), typeof(Type)),
                        Expression.Constant(property, typeof(IPropertyBase))));

                valueExpression = Expression.TryCatch(valueExpression, catchBlock);
            }

            return valueExpression;
        }

        private Expression CreateExtractJsonPropertyExpression(
            ParameterExpression jsonElementParameter,
            IProperty property)
        {
            var nullable = property.IsNullable;
            Expression resultExpression;
            if (property.GetTypeMapping().Converter is ValueConverter converter)
            {
                var providerClrType = converter.ProviderClrType.MakeNullable(nullable);
                if (!property.IsNullable || converter.ConvertsNulls)
                {
                    resultExpression = Expression.Call(
                        ExtractJsonPropertyMethodInfo.MakeGenericMethod(providerClrType),
                        jsonElementParameter,
                        Expression.Constant(property.GetJsonPropertyName()),
                        Expression.Constant(nullable));

                    resultExpression = ReplacingExpressionVisitor.Replace(
                        converter.ConvertFromProviderExpression.Parameters.Single(),
                        resultExpression,
                        converter.ConvertFromProviderExpression.Body);

                    if (resultExpression.Type != property.ClrType)
                    {
                        resultExpression = Expression.Convert(resultExpression, property.ClrType);
                    }
                }
                else
                {
                    // property is nullable and the converter can't handle nulls
                    // we need to peek into the JSON value and only pass it thru converter if it's not null
                    var jsonPropertyCall = Expression.Call(
                        ExtractJsonPropertyMethodInfo.MakeGenericMethod(providerClrType),
                        jsonElementParameter,
                        Expression.Constant(property.GetJsonPropertyName()),
                        Expression.Constant(nullable));

                    var jsonPropertyVariable = Expression.Variable(providerClrType);
                    var jsonPropertyAssignment = Expression.Assign(jsonPropertyVariable, jsonPropertyCall);

                    var testExpression = Expression.NotEqual(
                        jsonPropertyVariable,
                        Expression.Default(providerClrType));

                    var ifTrueExpression = (Expression)jsonPropertyVariable;
                    if (ifTrueExpression.Type != converter.ProviderClrType)
                    {
                        ifTrueExpression = Expression.Convert(ifTrueExpression, converter.ProviderClrType);
                    }

                    ifTrueExpression = ReplacingExpressionVisitor.Replace(
                        converter.ConvertFromProviderExpression.Parameters.Single(),
                        ifTrueExpression,
                        converter.ConvertFromProviderExpression.Body);

                    if (ifTrueExpression.Type != property.ClrType)
                    {
                        ifTrueExpression = Expression.Convert(ifTrueExpression, property.ClrType);
                    }

                    var condition = Expression.Condition(
                        testExpression,
                        ifTrueExpression,
                        Expression.Default(property.ClrType));

                    resultExpression = Expression.Block(
                        new ParameterExpression[] { jsonPropertyVariable },
                        new Expression[] { jsonPropertyAssignment, condition });
                }
            }
            else
            {
                resultExpression = Expression.Call(
                    ExtractJsonPropertyMethodInfo.MakeGenericMethod(property.ClrType),
                    jsonElementParameter,
                    Expression.Constant(property.GetJsonPropertyName()),
                    Expression.Constant(nullable));
            }

            if (_detailedErrorsEnabled)
            {
                var exceptionParameter = Expression.Parameter(typeof(Exception), name: "e");
                var catchBlock = Expression.Catch(
                    exceptionParameter,
                    Expression.Call(
                        ThrowExtractJsonPropertyExceptionMethod.MakeGenericMethod(resultExpression.Type),
                        exceptionParameter,
                        Expression.Constant(property, typeof(IProperty))));

                resultExpression = Expression.TryCatch(resultExpression, catchBlock);
            }

            return resultExpression;
        }

        private sealed class CollectionShaperFindingExpressionVisitor : ExpressionVisitor
        {
            private bool _containsCollection;

            public bool ContainsCollectionMaterialization(Expression expression)
            {
                _containsCollection = false;

                Visit(expression);

                return _containsCollection;
            }

            [return: NotNullIfNotNull("expression")]
            public override Expression? Visit(Expression? expression)
            {
                if (_containsCollection)
                {
                    return expression;
                }

                if (expression is RelationalCollectionShaperExpression
                    || expression is RelationalSplitCollectionShaperExpression)
                {
                    _containsCollection = true;

                    return expression;
                }

                return base.Visit(expression);
            }
        }

        private sealed class ExisitingJsonElementMapKeyComparer
            : IEqualityComparer<(int JsonColumnIndex, (string? JsonPropertyName, int? ConstantArrayIndex, int? NonConstantArrayIndex)[] AdditionalPath)>
        {
            public bool Equals(
                (int JsonColumnIndex, (string? JsonPropertyName, int? ConstantArrayIndex, int? NonConstantArrayIndex)[] AdditionalPath) x,
                (int JsonColumnIndex, (string? JsonPropertyName, int? ConstantArrayIndex, int? NonConstantArrayIndex)[] AdditionalPath) y)
                => x.JsonColumnIndex == y.JsonColumnIndex
                    && x.AdditionalPath.Length == y.AdditionalPath.Length
                    && x.AdditionalPath.SequenceEqual(y.AdditionalPath);

            public int GetHashCode([DisallowNull] (int JsonColumnIndex, (string? JsonPropertyName, int? ConstantArrayIndex, int? NonConstantArrayIndex)[] AdditionalPath) obj)
                => HashCode.Combine(obj.JsonColumnIndex, obj.AdditionalPath?.Length);
        }

        private sealed class ExisitingJsonKeyValuesMapKeyComparer
            : IEqualityComparer<(int JsonColumnIndex, (int? ConstantArrayIndex, int? NonConstantArrayIndex)[] AdditionalPath)>
        {
            public bool Equals(
                (int JsonColumnIndex, (int? ConstantArrayIndex, int? NonConstantArrayIndex)[] AdditionalPath) x,
                (int JsonColumnIndex, (int? ConstantArrayIndex, int? NonConstantArrayIndex)[] AdditionalPath) y)
                => x.JsonColumnIndex == y.JsonColumnIndex
                    && x.AdditionalPath.Length == y.AdditionalPath.Length
                    && x.AdditionalPath.SequenceEqual(y.AdditionalPath);

            public int GetHashCode([DisallowNull] (int JsonColumnIndex, (int? ConstantArrayIndex, int? NonConstantArrayIndex)[] AdditionalPath) obj)
                => HashCode.Combine(obj.JsonColumnIndex, obj.AdditionalPath?.Length);
        }
    }
}
