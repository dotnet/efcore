// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage.Json;

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
            _jsonValueBufferToJsonReaderDataAndKeyValuesParameterMapping = new();

        private readonly Dictionary<ParameterExpression, ParameterExpression>
            _jsonValueBufferParameterMapping2 = new();

        private readonly Dictionary<ParameterExpression, (ParameterExpression, ParameterExpression)>
            _jsonMaterializationContextParameterMapping = new();

        private readonly Dictionary<ParameterExpression, ParameterExpression>
            _jsonMaterializationContextParameterMapping2 = new();

        private readonly Dictionary<ParameterExpression, (ParameterExpression, ParameterExpression)>
            _jsonMaterializationContextToJsonReaderDataAndKeyValuesParameterMapping = new();

        private readonly Dictionary<ParameterExpression, ParameterExpression>
            _jsonReaderDataToJsonReaderManagerParameterMapping = new();

        /// <summary>
        ///     Cache for the JsonElement values we have generated - storing variables that the JsonElements are assigned to
        /// </summary>
        private readonly Dictionary<(int JsonColumnIndex, (string? JsonPropertyName, int? ConstantArrayIndex, int? NonConstantArrayIndex)[] AdditionalPath), ParameterExpression> _existingJsonElementMap
            = new(new ExistingJsonElementMapKeyComparer());

        /// <summary>
        ///     Cache for the key values we have generated - storing variables that the keys are assigned to
        /// </summary>
        private readonly Dictionary<(int JsonColumnIndex, (int? ConstantArrayIndex, int? NonConstantArrayIndex)[] AdditionalPath), ParameterExpression> _existingKeyValuesMap
            = new(new ExistingJsonKeyValuesMapKeyComparer());

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
            if (binaryExpression is { NodeType: ExpressionType.Assign, Left: ParameterExpression parameterExpression }
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
                    && _jsonValueBufferToJsonReaderDataAndKeyValuesParameterMapping.ContainsKey(valueBufferParameter))
                {
                    _jsonMaterializationContextToJsonReaderDataAndKeyValuesParameterMapping[parameterExpression] =
                        _jsonValueBufferToJsonReaderDataAndKeyValuesParameterMapping[valueBufferParameter];

                    var updatedExpression = newExpression.Update(
                        new[] { Expression.Constant(ValueBuffer.Empty), newExpression.Arguments[1] });

                    return Expression.Assign(binaryExpression.Left, updatedExpression);
                }

                //if (newExpression.Arguments[0] is ParameterExpression valueBufferParameter
                //    && _jsonValueBufferParameterMapping2.ContainsKey(valueBufferParameter))
                //{
                //    _jsonMaterializationContextParameterMapping2[parameterExpression] =
                //        _jsonValueBufferParameterMapping2[valueBufferParameter];

                //    var updatedExpression = newExpression.Update(
                //        new[] { Expression.Constant(ValueBuffer.Empty), newExpression.Arguments[1] });

                //    return Expression.Assign(binaryExpression.Left, updatedExpression);
                //}

                ////if (newExpression.Arguments[0] is ParameterExpression valueBufferParameter
                ////    && _jsonValueBufferParameterMapping.ContainsKey(valueBufferParameter))
                ////{
                ////    _jsonMaterializationContextParameterMapping[parameterExpression] =
                ////        _jsonValueBufferParameterMapping[valueBufferParameter];

                ////    var updatedExpression = newExpression.Update(
                ////        new[] { Expression.Constant(ValueBuffer.Empty), newExpression.Arguments[1] });

                ////    return Expression.Assign(binaryExpression.Left, updatedExpression);
                ////}
            }

            if (binaryExpression is
                {
                    NodeType: ExpressionType.Assign,
                    Left: MemberExpression { Member: FieldInfo { IsInitOnly: true } } memberExpression
                })
            {
                return memberExpression.Assign(Visit(binaryExpression.Right));
            }

            // we only have mapping between MaterializationContext and JsonReaderData, but we use JsonReaderManager to extract JSON values
            // so we need to add mapping between JsonReaderData and JsonReaderManager parameter, so we know which parameter to use
            // when generating actual Get* method
            if (binaryExpression.NodeType == ExpressionType.Assign
                && binaryExpression.Left is ParameterExpression jsonReaderManagerParameter
                && jsonReaderManagerParameter.Type == typeof(Utf8JsonReaderManager))
            {
                if (binaryExpression.Right is NewExpression { Arguments: [ParameterExpression jsonReaderDataParameter] })
                {
                    if (!_jsonMaterializationContextToJsonReaderDataAndKeyValuesParameterMapping.Any(x => x.Value.Item1 == jsonReaderDataParameter))
                    {
                        throw new InvalidOperationException("unknown data reader parameter we should have the mapping from materializer context by now - remove this check later tho, just keep for now to make sure we dind't mess up");
                    }

                    if (_jsonReaderDataToJsonReaderManagerParameterMapping.ContainsKey(jsonReaderDataParameter)
                        && _jsonReaderDataToJsonReaderManagerParameterMapping[jsonReaderDataParameter] != jsonReaderManagerParameter)
                    {
                        throw new InvalidOperationException("already in dictionary - remove this check later tho, just keep for now to make sure we dind't mess up");
                    }

                    _jsonReaderDataToJsonReaderManagerParameterMapping[jsonReaderDataParameter] = jsonReaderManagerParameter;
                }
                else
                {
                    throw new InvalidOperationException("something rong");
                }
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
                            var (jsonReaderDataVariable, keyValuesParameter) = JsonShapingPreProcess2(
                                jsonProjectionInfo,
                                entityShaperExpression.EntityType);

                            var shaperResult2 = CreateJsonShapers2(
                                entityShaperExpression.EntityType,
                                entityShaperExpression.IsNullable,
                                collection: false,
                                jsonReaderDataVariable,
                                keyValuesParameter,
                                parentEntityExpression: null,
                                navigation: null);

                            var visitedShaperResult = Visit(shaperResult2);
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

                case CollectionResultExpression { Navigation: INavigation navigation } collectionResultExpression
                    when GetProjectionIndex(collectionResultExpression.ProjectionBindingExpression)
                        is JsonProjectionInfo jsonProjectionInfo:
                {
                    // json entity collection at the root
                    var (jsonReaderDataVariable, keyValuesParameter) = JsonShapingPreProcess2(
                        jsonProjectionInfo,
                        navigation.TargetEntityType);

                    var shaperResult2 = CreateJsonShapers2(
                        navigation.TargetEntityType,
                        nullable: true,
                        collection: true,
                        jsonReaderDataVariable,
                        keyValuesParameter,
                        parentEntityExpression: null,
                        navigation: navigation);

                    var visitedShaperResult = Visit(shaperResult2);

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
                            var (jsonReaderDataVariable, keyValuesParameter) = JsonShapingPreProcess2(
                                jsonProjectionInfo,
                                includeExpression.Navigation.TargetEntityType);

                            var shaperResult2 = CreateJsonShapers2(
                                includeExpression.Navigation.TargetEntityType,
                                nullable: true,
                                collection: includeExpression.NavigationExpression is CollectionResultExpression,
                                jsonReaderDataVariable,
                                keyValuesParameter,
                                parentEntityExpression: entity,
                                navigation: (INavigation)includeExpression.Navigation);

                            var visitedShaperResult = Visit(shaperResult2);

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

                if (_jsonMaterializationContextToJsonReaderDataAndKeyValuesParameterMapping.ContainsKey(mappingParameter))
                {
                    var (jsonReaderDataParameter, keyPropertyValuesParameter) = _jsonMaterializationContextToJsonReaderDataAndKeyValuesParameterMapping[mappingParameter];

                    if (property!.IsPrimaryKey())
                    {
                        return Expression.MakeIndex(
                            keyPropertyValuesParameter,
                            ObjectArrayIndexerPropertyInfo,
                            new[] { Expression.Constant(index) });
                    }
                    else
                    {
                        var jsonReaderManagerParameter = _jsonReaderDataToJsonReaderManagerParameterMapping[jsonReaderDataParameter];

                        return CreateReadJsonPropertyValueExpression(jsonReaderManagerParameter, property);
                    }
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

        private static readonly MethodInfo InverseCollectionFixupMethod
            = typeof(ShaperProcessingExpressionVisitor).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Single(x => x.Name == nameof(InverseCollectionFixup))!;

        private Expression CreateJsonShapers2(
            IEntityType entityType,
            bool nullable,
            bool collection,
            ParameterExpression jsonReaderDataParameter,
            ParameterExpression keyValuesParameter,
            Expression? parentEntityExpression,
            INavigation? navigation)
        {
            var jsonReaderDataShaperLambdaParameter = Expression.Parameter(typeof(JsonReaderData));
            var keyValuesShaperLambdaParameter = Expression.Parameter(typeof(object[]));
            var shaperBlockVariables = new List<ParameterExpression>();
            var shaperBlockExpressions = new List<Expression>();

            var valueBufferParameter = Expression.Parameter(typeof(ValueBuffer));

            _jsonValueBufferParameterMapping2[valueBufferParameter] = keyValuesShaperLambdaParameter;

            _jsonValueBufferToJsonReaderDataAndKeyValuesParameterMapping[valueBufferParameter] = (jsonReaderDataShaperLambdaParameter, keyValuesShaperLambdaParameter);

            var entityShaperExpression = new RelationalEntityShaperExpression(
                entityType,
                valueBufferParameter,
                nullable);

            var entityShaperMaterializer = (BlockExpression)_parentVisitor.InjectEntityMaterializers(entityShaperExpression);
            var instanceVariable = entityShaperMaterializer.Variables[^1];

            // TODO: need to find instance 

            var innerShapersMap = new Dictionary<string, Expression>();
            var innerFixupMap = new Dictionary<string, LambdaExpression>();
            foreach (var ownedNavigation in entityType.GetNavigations().Where(
                n => n.TargetEntityType.IsMappedToJson() && n.ForeignKey.IsOwnership && n == n.ForeignKey.PrincipalToDependent))
            {
                // we need to build entity shapers and fixup separately
                // we don't know the order in which data comes, so we need to read through everything
                // before we can do fixup safely
                var innerShaper = CreateJsonShapers2(
                    ownedNavigation.TargetEntityType,
                    nullable || !ownedNavigation.ForeignKey.IsRequired,
                    ownedNavigation.IsCollection,
                    jsonReaderDataShaperLambdaParameter,
                    keyValuesShaperLambdaParameter,
                    parentEntityExpression: null,
                    navigation: ownedNavigation);

                // TODO: do we already have validation that all those names are unique?
                var navigationJsonPropertyName = ownedNavigation.TargetEntityType.GetJsonPropertyName()!;

                innerShapersMap[navigationJsonPropertyName] = innerShaper;

                if (ownedNavigation.IsCollection)
                {
                    var shaperEntityParameter = Expression.Parameter(ownedNavigation.DeclaringEntityType.ClrType);
                    var shaperCollectionParameter = Expression.Parameter(ownedNavigation.ClrType);
                    var expressions = new List<Expression>();

                    if (!ownedNavigation.IsShadowProperty())
                    {
                        expressions.Add(
                            shaperEntityParameter.MakeMemberAccess(ownedNavigation.GetMemberInfo(forMaterialization: true, forSet: true)).Assign(shaperCollectionParameter));
                    }

                    if (ownedNavigation.Inverse is INavigation inverseNavigation
                        && !inverseNavigation.IsShadowProperty())
                    {
                        //for (var i = 0; i < prm.Count; i++)
                        //{
                        //    prm[i].Parent = instance
                        //}
                        var innerFixupCollectionElementParameter = Expression.Parameter(inverseNavigation.DeclaringEntityType.ClrType);
                        var innerFixupParentParameter = Expression.Parameter(inverseNavigation.TargetEntityType.ClrType);

                        var elementFixup = Expression.Lambda(
                            Expression.Block(
                                typeof(void), 
                                AssignReferenceNavigation(
                                    innerFixupCollectionElementParameter,
                                    innerFixupParentParameter,
                                    inverseNavigation)),
                                    innerFixupCollectionElementParameter,
                                    innerFixupParentParameter);

                        expressions.Add(
                            Expression.Call(
                                InverseCollectionFixupMethod.MakeGenericMethod(
                                    inverseNavigation.DeclaringEntityType.ClrType,
                                    inverseNavigation.TargetEntityType.ClrType),
                                shaperCollectionParameter,
                                shaperEntityParameter,
                                elementFixup));
                    }

                    var fixup = Expression.Lambda(
                        Expression.Block(typeof(void), expressions),
                        shaperEntityParameter,
                        shaperCollectionParameter);

                    innerFixupMap[navigationJsonPropertyName] = fixup;
                }
                else
                {
                    var fixup = GenerateReferenceFixupForJson(
                        ownedNavigation.DeclaringEntityType.ClrType,
                        ownedNavigation.TargetEntityType.ClrType,
                        ownedNavigation,
                        ownedNavigation.Inverse);

                    innerFixupMap[navigationJsonPropertyName] = fixup;
                }
            }

            var rewrittenEntityShaperMaterializer = new JsonEntityMaterializerRewriter2(
                entityShaperExpression.EntityType,
                jsonReaderDataShaperLambdaParameter,
                innerShapersMap, innerFixupMap).Rewrite(entityShaperMaterializer);

            var entityShaperMaterializerVariable = Expression.Variable(entityShaperMaterializer.Type);
            shaperBlockVariables.Add(entityShaperMaterializerVariable);
            shaperBlockExpressions.Add(Expression.Assign(entityShaperMaterializerVariable, rewrittenEntityShaperMaterializer));

            var shaperBlock = Expression.Block(
                shaperBlockVariables,
                shaperBlockExpressions);

            var shaperLambda = Expression.Lambda(
                shaperBlock,
                QueryCompilationContext.QueryContextParameter,
                keyValuesShaperLambdaParameter,
                jsonReaderDataShaperLambdaParameter);

            if (parentEntityExpression != null)
            {
                // this happens only on top level when we project owner entity in this case we can do fixup as part of generating materializer
                // (since we are guaranteed that the parent already exists) - for nested JSON materialization we need to do fixup at the end
                // because we are streaming the data and don't know if we get the parent json object before the child
                // (in case parent ctor takes some parameters and they are read as last thing in the JSON)
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
                            IncludeJsonEntityCollection2MethodInfo.MakeGenericMethod(
                                navigation.DeclaringEntityType.ClrType,
                                navigation.TargetEntityType.ClrType),
                            QueryCompilationContext.QueryContextParameter,
                            keyValuesParameter,
                            jsonReaderDataParameter,
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

                var includeJsonEntityReference2MethodCall =
                    Expression.Call(
                        IncludeJsonEntityReference2MethodInfo.MakeGenericMethod(
                            navigation.DeclaringEntityType.ClrType,
                            navigation.TargetEntityType.ClrType),
                        QueryCompilationContext.QueryContextParameter,
                        keyValuesParameter,
                        jsonReaderDataParameter,
                        includingEntityExpression,
                        shaperLambda,
                fixup);

                return navigation.DeclaringEntityType.ClrType.IsAssignableFrom(parentEntityExpression.Type)
                    ? includeJsonEntityReference2MethodCall
                    : Expression.IfThen(
                        Expression.TypeIs(
                            parentEntityExpression,
                            navigation.DeclaringEntityType.ClrType),
                        includeJsonEntityReference2MethodCall);
            }

            if (navigation != null && navigation.IsCollection)
            {
                var materializeJsonEntityCollectionMethodCall =
                    Expression.Call(
                        MaterializeJsonEntityCollection2MethodInfo.MakeGenericMethod(
                            navigation.TargetEntityType.ClrType,
                            navigation.ClrType),
                        QueryCompilationContext.QueryContextParameter,
                        keyValuesParameter,
                        jsonReaderDataParameter,
                        Expression.Constant(navigation),
                        shaperLambda);

                return materializeJsonEntityCollectionMethodCall;
            }

            var materializedRootJsonEntity = Expression.Call(
                MaterializeJsonEntity2MethodInfo.MakeGenericMethod(entityType.ClrType),
                QueryCompilationContext.QueryContextParameter,
                keyValuesParameter,
                jsonReaderDataParameter,
                Expression.Constant(nullable),
                shaperLambda);

            return materializedRootJsonEntity;
        }

        private class JsonEntityMaterializerRewriter2 : ExpressionVisitor
        {
            //private static readonly Dictionary<Type, MethodInfo> GetXMethods = new()
            //{
            //    { typeof(bool), typeof(Utf8JsonReader).GetMethod(nameof(Utf8JsonReader.GetBoolean))! },
            //    { typeof(byte), typeof(Utf8JsonReader).GetMethod(nameof(Utf8JsonReader.GetByte))! },
            //    { typeof(DateTime), typeof(Utf8JsonReader).GetMethod(nameof(Utf8JsonReader.GetDateTime))! },
            //    { typeof(DateTimeOffset), typeof(Utf8JsonReader).GetMethod(nameof(Utf8JsonReader.GetDateTimeOffset))! },
            //    { typeof(decimal), typeof(Utf8JsonReader).GetMethod(nameof(Utf8JsonReader.GetDecimal))! },
            //    { typeof(double), typeof(Utf8JsonReader).GetMethod(nameof(Utf8JsonReader.GetDouble))! },
            //    { typeof(float), typeof(Utf8JsonReader).GetMethod(nameof(Utf8JsonReader.GetSingle))! },
            //    { typeof(Guid), typeof(Utf8JsonReader).GetMethod(nameof(Utf8JsonReader.GetGuid))! },
            //    { typeof(short), typeof(Utf8JsonReader).GetMethod(nameof(Utf8JsonReader.GetInt16))! },
            //    { typeof(int), typeof(Utf8JsonReader).GetMethod(nameof(Utf8JsonReader.GetInt32))! },
            //    { typeof(long), typeof(Utf8JsonReader).GetMethod(nameof(Utf8JsonReader.GetInt64))! },
            //    { typeof(string), typeof(Utf8JsonReader).GetMethod(nameof(Utf8JsonReader.GetString))! },
            //};

            public static bool ValueTextEquals(ref Utf8JsonReaderManager manager, JsonEncodedText json)
                => manager.CurrentReader.ValueTextEquals(json.EncodedUtf8Bytes);

            private readonly IEntityType? _entityType;
            private readonly ParameterExpression _jsonReaderDataParameter;
            private readonly IDictionary<string, Expression> _innerShapersMap;
            private readonly IDictionary<string, LambdaExpression> _innerFixupMap;
            private bool _found = false;

            // TODO: move it to common place somewhere
            private static readonly ConstructorInfo JsonReaderManagerConstructor
                = typeof(Utf8JsonReaderManager).GetConstructor(new Type[] { typeof(JsonReaderData) })!;

            private static readonly MethodInfo Utf8JsonReaderManagerMoveNextMethod
                = typeof(Utf8JsonReaderManager).GetMethod(nameof(Utf8JsonReaderManager.MoveNext), new Type[] { })!;

            private static readonly MethodInfo Utf8JsonReaderManagerCaptureStateMethod
                = typeof(Utf8JsonReaderManager).GetMethod(nameof(Utf8JsonReaderManager.CaptureState), new Type[] { })!;

            //private static readonly PropertyInfo Utf8JsonReaderManagerCurrentReaderProperty
            //    = typeof(Utf8JsonReaderManager).GetProperty(nameof(Utf8JsonReaderManager.CurrentReader))!;

            private static readonly FieldInfo Utf8JsonReaderManagerCurrentReaderField
                = typeof(Utf8JsonReaderManager).GetField(nameof(Utf8JsonReaderManager.CurrentReader))!;

            private static readonly MethodInfo Utf8JsonReaderValueTextEqualsMethod
                = typeof(Utf8JsonReader).GetMethod(nameof(Utf8JsonReader.ValueTextEquals), new Type[] { typeof(ReadOnlySpan<byte>) })!;

            private static readonly MethodInfo Utf8JsonReaderManagerValueTextEqualsMethod
                = typeof(Utf8JsonReaderManager).GetMethod(nameof(Utf8JsonReaderManager.ValueTextEquals), new Type[] { typeof(ReadOnlySpan<byte>) })!;

            private static readonly MethodInfo Utf8JsonReaderManagerSkipMethod
                = typeof(Utf8JsonReaderManager).GetMethod(nameof(Utf8JsonReaderManager.Skip), new Type[] { })!;

            private static readonly MethodInfo Utf8JsonReaderManagerTokenTypeMethod
                = typeof(Utf8JsonReaderManager).GetMethod(nameof(Utf8JsonReaderManager.TokenType), new Type[] { })!;

            // keep track which variable corresponds to which navigation - we need that info for fixup
            // which happens at the end (after we read everything to guarantee that we can instantiate the entity 
            private readonly Dictionary<string, ParameterExpression> _navigationVariableMap = new();

            public JsonEntityMaterializerRewriter2(
                IEntityType entityType,
                ParameterExpression jsonReaderDataParameter,
                IDictionary<string, Expression> innerShapersMap,
                IDictionary<string, LambdaExpression> innerFixupMap)
            {
                _entityType = entityType;
                _jsonReaderDataParameter = jsonReaderDataParameter;
                _innerShapersMap = innerShapersMap;
                _innerFixupMap = innerFixupMap;
            }

            public BlockExpression Rewrite(BlockExpression jsonEntityShaperMaterializer)
            {
                _found = false;

                var result = (BlockExpression)VisitBlock(jsonEntityShaperMaterializer);

                if (!_found)
                {
                    throw new InvalidOperationException("Didn't find the materializer to rewrite - pattern matching's busted!");
                }

                return result;
            }

            protected override Expression VisitSwitch(SwitchExpression switchExpression)
            {
                // TODO: make a nice pattern match so that Shay is happy
                if (switchExpression.SwitchValue.Type == typeof(IEntityType)
                    && switchExpression.Cases.Count == 1
                    && switchExpression.Cases[0] is SwitchCase onlyCase
                    && onlyCase.TestValues.Count == 1
                    && onlyCase.TestValues[0] is ConstantExpression onlyValue
                    && onlyValue.Value == _entityType
                    && onlyCase.Body is BlockExpression body)
                {
                    if (body.Expressions.Count > 0
                        //sometimes we have shadow value buffer, sometimes not
                        && body.Expressions[^1] is BlockExpression jsonEntityTypeInitializerBlock)
                    {
                        if (jsonEntityTypeInitializerBlock.Variables.Count == 1
                            && jsonEntityTypeInitializerBlock.Variables[0] is ParameterExpression jsonEntityTypeVariable
                            && jsonEntityTypeInitializerBlock.Expressions[0] is BinaryExpression jsonEntityTypeConstructionAssignment
                            && jsonEntityTypeConstructionAssignment.NodeType == ExpressionType.Assign
                            && jsonEntityTypeConstructionAssignment.Left == jsonEntityTypeVariable
                            && jsonEntityTypeConstructionAssignment.Right is NewExpression jsonEntityTypeConstruction)
                        {
                            var propertyAssignments = jsonEntityTypeInitializerBlock.Expressions.Skip(1).Where(x => x.NodeType == ExpressionType.Assign).Cast<BinaryExpression>().ToList();
                            var managerVariable = Expression.Variable(typeof(Utf8JsonReaderManager));
                            var tokenTypeVariable = Expression.Variable(typeof(JsonTokenType), "tokenType");

                            var finalBlockVariables = new List<ParameterExpression>
                            {
                                jsonEntityTypeVariable,
                                managerVariable,
                                tokenTypeVariable,
                            };

                            var finalBlockExpressions = new List<Expression>
                            {
                                Expression.Assign(
                                    managerVariable,
                                    Expression.New(
                                        JsonReaderManagerConstructor,
                                        _jsonReaderDataParameter)),
                                Expression.Assign(
                                    tokenTypeVariable,
                                    Expression.Call(managerVariable, Utf8JsonReaderManagerTokenTypeMethod)),
                            };

                            // TODO: do the proper check - make sure this thing always can only have 1 or 2 statements (with and without value buffer)
                            if (body.Expressions.Count > 1)
                            {
                                finalBlockExpressions.Insert(0, body.Expressions[0]);
                            }

                            if (jsonEntityTypeConstruction.Arguments.Any())
                            {
                                //propertyAssignments.AddRange(jsonEntityTypeConstruction.Arguments);

                                // ctor has arguments - need to cache all the values 
                            }
                            else
                            {

                                var breakLabel = Expression.Label("done");

                                var testExpressions = new List<Expression>();
                                var readExpressions = new List<Expression>();

                                // look into ctor - if it takes any arguments we need to include them in the loop
                                var ctorAssignmentMap = new Dictionary<Expression, ParameterExpression>();
                                var propertyAssignmentMap = new Dictionary<Expression, ParameterExpression>();
                                var navigationAssignmentMap = new Dictionary<Expression, ParameterExpression>();

                                foreach (var propertyAssignment in propertyAssignments)
                                {
                                    if (propertyAssignment.Right is MethodCallExpression valueBufferTryReadValueCall)
                                    {
                                        var property = (IProperty)((ConstantExpression)valueBufferTryReadValueCall.Arguments[2]).Value!;

                                        // right should be read the proper value of token instead
                                        //var newCase = Expression.SwitchCase(
                                        //    Expression.Block(
                                        //        Expression.Assign(propertyAssignment.Left, propertyAssignment.Right),
                                        //        Expression.Empty()),
                                        //   Expression.Constant(property.GetJsonPropertyName()));

                                        testExpressions.Add(
                                            Expression.Call(
                                                managerVariable,
                                                Utf8JsonReaderManagerValueTextEqualsMethod,
                                                Expression.Property(
                                                    Expression.Constant(JsonEncodedText.Encode(property.GetJsonPropertyName()!)),
                                                    "EncodedUtf8Bytes")));

                                        var propertyVariable = Expression.Variable(property.ClrType);
                                        finalBlockVariables.Add(propertyVariable);

                                        var moveNext = Expression.Call(
                                            managerVariable,
                                            Utf8JsonReaderManagerMoveNextMethod);

                                        // do the conversion to appropriate json reader method in the visit later (like we do for non-json property access)
                                        var assignment = Expression.Assign(
                                            propertyVariable,
                                            propertyAssignment.Right);

                                        readExpressions.Add(
                                            Expression.Block(
                                                moveNext,
                                                assignment,
                                                Expression.Empty()));

                                        propertyAssignmentMap[propertyAssignment.Left] = propertyVariable;
                                    }
                                }

                                foreach (var innerShaperMapElement in _innerShapersMap)
                                {
                                    testExpressions.Add(
                                        Expression.Call(
                                            managerVariable,
                                            Utf8JsonReaderManagerValueTextEqualsMethod,
                                            Expression.Property(
                                                Expression.Constant(JsonEncodedText.Encode(innerShaperMapElement.Key)),
                                                "EncodedUtf8Bytes")));

                                    var propertyVariable = Expression.Variable(innerShaperMapElement.Value.Type);
                                    finalBlockVariables.Add(propertyVariable);

                                    _navigationVariableMap[innerShaperMapElement.Key] = propertyVariable;

                                    var moveNext = Expression.Call(
                                        managerVariable,
                                        Utf8JsonReaderManagerMoveNextMethod);

                                    var captureState = Expression.Call(
                                        managerVariable,
                                        Utf8JsonReaderManagerCaptureStateMethod);

                                    var assignment = Expression.Assign(
                                        propertyVariable,
                                        innerShaperMapElement.Value);

                                    var managerRecreation = Expression.Assign(
                                        managerVariable,
                                        Expression.New(JsonReaderManagerConstructor, _jsonReaderDataParameter));

                                    readExpressions.Add(
                                        Expression.Block(
                                            moveNext,
                                            captureState,
                                            assignment,
                                            managerRecreation,
                                            Expression.Empty()));
                                }

                                var testsCount = testExpressions.Count;
                                var testExpression = Expression.IfThen(
                                    testExpressions[testsCount - 1],
                                    readExpressions[testsCount - 1]);

                                for (var i = testsCount - 2; i >= 0; i--)
                                {
                                    testExpression = Expression.IfThenElse(
                                        testExpressions[i],
                                        readExpressions[i],
                                        testExpression);
                                }

                                var cases = new List<SwitchCase>();

                                var propertySwitchCase = Expression.SwitchCase(
                                    testExpression,
                                    Expression.Constant(JsonTokenType.PropertyName));

                                cases.Add(propertySwitchCase);

                                var loopTest = Expression.NotEqual(
                                    tokenTypeVariable,
                                    Expression.Constant(JsonTokenType.EndObject));

                                var loopBody = Expression.Block(
                                    Expression.Assign(
                                        tokenTypeVariable,
                                        Expression.Call(
                                            managerVariable,
                                            Utf8JsonReaderManagerMoveNextMethod)),
                                    Expression.IfThenElse(
                                        loopTest,
                                        Expression.Switch(
                                            tokenTypeVariable,
                                            Expression.Call(managerVariable, Utf8JsonReaderManagerSkipMethod),
                                            cases.ToArray()),
                                        Expression.Break(breakLabel)));

                                var loop = Expression.Loop(loopBody, breakLabel);
                                finalBlockExpressions.Add(loop);

                                var finalCaptureState = Expression.Call(managerVariable, Utf8JsonReaderManagerCaptureStateMethod);
                                finalBlockExpressions.Add(finalCaptureState);

                                var entityCtor = jsonEntityTypeInitializerBlock.Expressions[0];
                                finalBlockExpressions.Add(entityCtor);

                                // also for ctor
                                foreach (var propertyAssignmentMapElement in propertyAssignmentMap)
                                {
                                    finalBlockExpressions.Add(
                                        Expression.Assign(propertyAssignmentMapElement.Key, propertyAssignmentMapElement.Value));
                                }

                                foreach (var fixup in _innerFixupMap)
                                {
                                    finalBlockExpressions.Add(
                                        Expression.Invoke(
                                            fixup.Value,
                                            jsonEntityTypeVariable,
                                            _navigationVariableMap[fixup.Key]));
                                }

                                _found = true;

                                finalBlockExpressions.Add(jsonEntityTypeVariable);


                                var kups = Expression.Block(
                                    finalBlockVariables,
                                    finalBlockExpressions);

                                return Expression.Block(
                                    finalBlockVariables,
                                    finalBlockExpressions);
                            }
                        }
                    }
                }

                return base.VisitSwitch(switchExpression);
            }
        }

        private static readonly PropertyInfo UTF8Property
            = typeof(Encoding).GetProperty(nameof(Encoding.UTF8))!;

        private static readonly MethodInfo EncodingGetBytesMethod
            = typeof(Encoding).GetMethod(nameof(Encoding.GetBytes), new[] { typeof(string) })!;

        private static readonly ConstructorInfo MemoryStreamConstructor
            = typeof(MemoryStream).GetConstructor(new[] { typeof(byte[]) })!;

        private static readonly ConstructorInfo JsonReaderDataConstructor
            = typeof(JsonReaderData).GetConstructor(new Type[] { typeof(Stream) })!;

        private static readonly ConstructorInfo JsonReaderManagerConstructor
            = typeof(Utf8JsonReaderManager).GetConstructor(new Type[] { typeof(JsonReaderData) })!;

        private static readonly MethodInfo Utf8JsonReaderManagerMoveNextMethod
            = typeof(Utf8JsonReaderManager).GetMethod(nameof(Utf8JsonReaderManager.MoveNext), new Type[] { })!;

        private static readonly MethodInfo Utf8JsonReaderManagerCaptureStateMethod
            = typeof(Utf8JsonReaderManager).GetMethod(nameof(Utf8JsonReaderManager.CaptureState), new Type[] { })!;

        private (ParameterExpression, ParameterExpression) JsonShapingPreProcess2(
            JsonProjectionInfo jsonProjectionInfo,
            IEntityType entityType)
        {
            var jsonColumnName = entityType.GetContainerColumnName()!;
            var jsonColumnTypeMapping = (entityType.GetViewOrTableMappings().SingleOrDefault()?.Table
                    ?? entityType.GetDefaultMappings().Single().Table)
                .FindColumn(jsonColumnName)!.StoreTypeMapping;

            var jsonStreamVariable = Expression.Variable(typeof(Stream));
            var jsonReaderDataVariable = Expression.Variable(typeof(JsonReaderData));
            var jsonReaderManagerVariable = Expression.Variable(typeof(Utf8JsonReaderManager));

            var jsonStreamAssignment = Expression.Assign(
                jsonStreamVariable,
                CreateGetValueExpression(
                    _dataReaderParameter,
                    jsonProjectionInfo.JsonColumnIndex,
                    nullable: true,
                    jsonColumnTypeMapping,
                    typeof(MemoryStream),
                    property: null));

            var jsonReaderDataAssignment = Expression.Assign(
                jsonReaderDataVariable,
                Expression.New(JsonReaderDataConstructor, jsonStreamVariable));

            var jsonReaderManagerAssignment = Expression.Assign(
                jsonReaderManagerVariable,
                Expression.New(JsonReaderManagerConstructor, jsonReaderDataVariable));

            var jumpStartMoveNext = Expression.Call(
                jsonReaderManagerVariable,
                Utf8JsonReaderManagerMoveNextMethod);

            var captureState = Expression.Call(
                jsonReaderManagerVariable,
                Utf8JsonReaderManagerCaptureStateMethod);

            _variables.Add(jsonStreamVariable);
            _variables.Add(jsonReaderDataVariable);
            _variables.Add(jsonReaderManagerVariable);
            _expressions.Add(jsonStreamAssignment);
            _expressions.Add(jsonReaderDataAssignment);
            _expressions.Add(jsonReaderManagerAssignment);
            _expressions.Add(jumpStartMoveNext);
            _expressions.Add(captureState);

            //var currentKeyValuesVariable = Expression.Variable(typeof(object[]));

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
            var currentKeyValuesVariable = Expression.Parameter(typeof(object[]));
            var keyValuesAssignment = Expression.Assign(
                currentKeyValuesVariable,
                Expression.NewArrayInit(typeof(object), keyValues));

            _variables.Add(currentKeyValuesVariable);
            _expressions.Add(keyValuesAssignment);

            return (jsonReaderDataVariable, currentKeyValuesVariable);

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

        private static LambdaExpression GenerateReferenceFixupForJson(
            Type entityType,
            Type relatedEntityType,
            INavigationBase navigation,
            INavigationBase? inverseNavigation)
        {
            var entityParameter = Expression.Parameter(entityType);
            var relatedEntityParameter = Expression.Parameter(relatedEntityType);
            var expressions = new List<Expression>();

            if (navigation.IsCollection)
            {
                throw new InvalidOperationException("cleaup this!");
                //if (!navigation.IsShadowProperty())
                //{
                //    expressions.Add(
                //        entityParameter.MakeMemberAccess(
                //            navigation.GetMemberInfo(
                //                forMaterialization: true,
                //                forSet: true))
                //        .Assign(relatedEntityParameter));
                //}

                //if (inverseNavigation != null
                //    && !inverseNavigation.IsShadowProperty())
                //{
                //    // TODO: foreach, add everything
                //}
            }
            else
            {
                if (!navigation.IsShadowProperty())
                {
                    expressions.Add(
                        AssignReferenceNavigation(
                            entityParameter,
                            relatedEntityParameter,
                            navigation));
                }

                if (inverseNavigation != null
                    && !inverseNavigation.IsShadowProperty())
                {
                    expressions.Add(
                        AssignReferenceNavigation(
                            relatedEntityParameter,
                            entityParameter,
                            inverseNavigation));
                }
            }

            return Expression.Lambda(Expression.Block(typeof(void), expressions), entityParameter, relatedEntityParameter);
        }

        private static Expression AddToCollectionNavigationForJson(
            ParameterExpression entity,
            ParameterExpression relatedEntity,
            INavigationBase navigation)
            => Expression.Call(
                Expression.Constant(navigation.GetCollectionAccessor()),
                CollectionAccessorAddMethodInfo,
                entity,
                relatedEntity,
                Expression.Constant(true));

        private static void InverseCollectionFixup<TCollectionElement, TEntity>(
            ICollection<TCollectionElement> collection,
            TEntity entity,
            Action<TCollectionElement, TEntity> elementFixup)
        {
            foreach (var collectionElement in collection)
            {
                elementFixup(collectionElement, entity);
            }
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

        private static readonly MethodInfo Utf8JsonReaderManagerTokenTypeMethod
            = typeof(Utf8JsonReaderManager).GetMethod(nameof(Utf8JsonReaderManager.TokenType), new Type[] { })!;

        private Expression CreateReadJsonPropertyValueExpression(
            ParameterExpression jsonReaderManagerParameter,
            IProperty property)
        {
            var nullable = property.IsNullable;
            var typeMapping = property.GetTypeMapping();
            var providerClrType = (typeMapping.Converter?.ProviderClrType ?? typeMapping.ClrType).UnwrapNullableType();

            var jsonReaderWriterExpression = Expression.Constant(property.GetJsonValueReaderWriter()!);

            var fromJsonMethod = jsonReaderWriterExpression.Type.GetMethod(
                nameof(JsonValueReaderWriter.FromJson), new[] { typeof(Utf8JsonReaderManager).MakeByRefType() })!;

            Expression resultExpression = Expression.Convert(
                Expression.Call(jsonReaderWriterExpression, fromJsonMethod, jsonReaderManagerParameter),
                providerClrType);

            if (property.GetTypeMapping().Converter is ValueConverter converter)
            {
                if (!property.IsNullable || converter.ConvertsNulls)
                {
                    // in case of null value we can't just use the JsonReader method, but rather check the current token type
                    // if it's JsonTokenType.Null means value is null, only if it's not we are safe to read the value
                    if (nullable)
                    {
                        resultExpression = Expression.Condition(
                            Expression.Equal(
                                Expression.Call(jsonReaderManagerParameter, Utf8JsonReaderManagerTokenTypeMethod),
                                Expression.Constant(JsonTokenType.Null)),
                            Expression.Default(providerClrType),
                            resultExpression);
                    }

                    resultExpression = Expression.Convert(
                        Expression.Call(jsonReaderWriterExpression, fromJsonMethod, jsonReaderManagerParameter),
                        providerClrType);

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
                    resultExpression = Expression.Convert(
                        Expression.Call(jsonReaderWriterExpression, fromJsonMethod, jsonReaderManagerParameter),
                        providerClrType);

                    resultExpression = ReplacingExpressionVisitor.Replace(
                        converter.ConvertFromProviderExpression.Parameters.Single(),
                        resultExpression,
                        converter.ConvertFromProviderExpression.Body);

                    if (resultExpression.Type != property.ClrType)
                    {
                        resultExpression = Expression.Convert(resultExpression, property.ClrType);
                    }

                    resultExpression = Expression.Condition(
                        Expression.Equal(
                            Expression.Call(jsonReaderManagerParameter, Utf8JsonReaderManagerTokenTypeMethod),
                            Expression.Constant(JsonTokenType.Null)),
                        Expression.Default(property.ClrType),
                        resultExpression);
                }
            }
            else
            {
                if (nullable)
                {
                    // in case of null value we can't just use the JsonReader method, but rather check the current token type
                    // if it's JsonTokenType.Null means value is null, only if it's not we are safe to read the value
                    resultExpression = Expression.Condition(
                        Expression.Equal(
                            Expression.Call(jsonReaderManagerParameter, Utf8JsonReaderManagerTokenTypeMethod),
                            Expression.Constant(JsonTokenType.Null)),
                        Expression.Default(providerClrType),
                        resultExpression);
                }
            }

            if (resultExpression.Type != property.ClrType)
            {
                resultExpression = Expression.Convert(resultExpression, property.ClrType);
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

        private sealed class ExistingJsonElementMapKeyComparer
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

        private sealed class ExistingJsonKeyValuesMapKeyComparer
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
