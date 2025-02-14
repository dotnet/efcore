// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage.Json;
using static System.Linq.Expressions.Expression;

namespace Microsoft.EntityFrameworkCore.Query;

public partial class RelationalShapedQueryCompilingExpressionVisitor
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public sealed partial class ShaperProcessingExpressionVisitor : ExpressionVisitor
    {
        private static readonly bool UseOldBehavior35212 =
            AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue35212", out var enabled35212) && enabled35212;

        /// <summary>
        ///     Reading database values
        /// </summary>
        private static readonly MethodInfo IsDbNullMethod =
            typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.IsDBNull), [typeof(int)])!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static readonly MethodInfo GetFieldValueMethod =
            typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.GetFieldValue), [typeof(int)])!;

        /// <summary>
        ///     Coordinating results
        /// </summary>
        private static readonly MemberInfo ResultContextValuesMemberInfo
            = typeof(ResultContext).GetMember(nameof(ResultContext.Values))[0];

        private static readonly MemberInfo SingleQueryResultCoordinatorResultReadyMemberInfo
            = typeof(SingleQueryResultCoordinator).GetMember(nameof(SingleQueryResultCoordinator.ResultReady))[0];

        private static readonly MethodInfo CollectionAccessorGetOrCreateMethodInfo
            = typeof(IClrCollectionAccessor).GetTypeInfo().GetDeclaredMethod(nameof(IClrCollectionAccessor.GetOrCreate))!;

        private static readonly MethodInfo CollectionAccessorAddMethodInfo
            = typeof(IClrCollectionAccessor).GetTypeInfo().GetDeclaredMethod(nameof(IClrCollectionAccessor.Add))!;

        private static readonly PropertyInfo ObjectArrayIndexerPropertyInfo
            = typeof(object[]).GetProperty("Item")!;

        private static readonly ConstructorInfo JsonReaderDataConstructor
            = typeof(JsonReaderData).GetConstructor([typeof(Stream)])!;

        private static readonly ConstructorInfo JsonReaderManagerConstructor
            = typeof(Utf8JsonReaderManager).GetConstructor(
                [typeof(JsonReaderData), typeof(IDiagnosticsLogger<DbLoggerCategory.Query>)])!;

        private static readonly MethodInfo Utf8JsonReaderManagerMoveNextMethod
            = typeof(Utf8JsonReaderManager).GetMethod(nameof(Utf8JsonReaderManager.MoveNext), [])!;

        private static readonly MethodInfo Utf8JsonReaderManagerCaptureStateMethod
            = typeof(Utf8JsonReaderManager).GetMethod(nameof(Utf8JsonReaderManager.CaptureState), [])!;

        private static readonly FieldInfo Utf8JsonReaderManagerCurrentReaderField
            = typeof(Utf8JsonReaderManager).GetField(nameof(Utf8JsonReaderManager.CurrentReader))!;

        private static readonly MethodInfo Utf8JsonReaderManagerSkipMethod
            = typeof(Utf8JsonReaderManager).GetMethod(nameof(Utf8JsonReaderManager.Skip), [])!;

        private static readonly MethodInfo Utf8JsonReaderValueTextEqualsMethod
            = typeof(Utf8JsonReader).GetMethod(nameof(Utf8JsonReader.ValueTextEquals), [typeof(ReadOnlySpan<byte>)])!;

        private static readonly PropertyInfo EncodingUtf8Property
            = typeof(Encoding).GetProperty(nameof(Encoding.UTF8))!;

        private static readonly MethodInfo Utf8GetBytesMethod
            = typeof(Encoding).GetMethod(nameof(Encoding.GetBytes), [typeof(string)])!;

        private static readonly MethodInfo ByteArrayAsSpanMethod = typeof(MemoryExtensions).GetMethods()
            .Where(x => x.Name == nameof(MemoryExtensions.AsSpan) && x.GetGenericArguments().Count() == 1)
            .Select(x => new { x, prms = x.GetParameters() })
            .Where(x => x.prms.Count() == 1 && x.prms[0].ParameterType.IsArray)
            .Single().x.MakeGenericMethod(typeof(byte));

        private static readonly PropertyInfo Utf8JsonReaderTokenTypeProperty
            = typeof(Utf8JsonReader).GetProperty(nameof(Utf8JsonReader.TokenType))!;

        private static readonly MethodInfo PropertyGetJsonValueReaderWriterMethod =
            typeof(IReadOnlyProperty).GetMethod(nameof(IReadOnlyProperty.GetJsonValueReaderWriter), [])!;

        private static readonly MethodInfo PropertyGetTypeMappingMethod =
            typeof(IReadOnlyProperty).GetMethod(nameof(IReadOnlyProperty.GetTypeMapping), [])!;

        private readonly RelationalShapedQueryCompilingExpressionVisitor _parentVisitor;
        private readonly ISet<string>? _tags;
        private readonly bool _isTracking;
        private readonly bool _queryStateManager;
        private readonly bool _isAsync;
        private readonly bool _splitQuery;
        private readonly bool _detailedErrorsEnabled;
        private readonly bool _generateCommandResolver;
        private readonly ParameterExpression _resultCoordinatorParameter;
        private readonly ParameterExpression? _executionStrategyParameter;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _queryLogger;

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
        private readonly List<ParameterExpression> _variables = [];

        private readonly List<Expression> _expressions = [];

        /// <summary>
        ///     IncludeExpressions are added later in case they are using ValuesArray
        /// </summary>
        private readonly List<Expression> _includeExpressions = [];

        /// <summary>
        ///     Json entities are added after includes so that we can utilize tracking (includes will track all json entities)
        /// </summary>
        private readonly List<Expression> _jsonEntityExpressions = [];

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
            _jsonValueBufferToJsonReaderDataAndKeyValuesParameterMapping = new();

        private readonly Dictionary<ParameterExpression, (ParameterExpression, ParameterExpression)>
            _jsonMaterializationContextToJsonReaderDataAndKeyValuesParameterMapping = new();

        private readonly Dictionary<ParameterExpression, ParameterExpression>
            _jsonReaderDataToJsonReaderManagerParameterMapping = new();

        /// <summary>
        ///     Map between index of the non-constant json array element access
        ///     and the variable we store it's value that we extract from the reader
        /// </summary>
        private readonly Dictionary<int, ParameterExpression> _jsonArrayNonConstantElementAccessMap = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ShaperProcessingExpressionVisitor(
            RelationalShapedQueryCompilingExpressionVisitor parentVisitor,
            SelectExpression selectExpression,
            ISet<string> tags,
            bool splitQuery,
            bool indexMap)
        {
            _parentVisitor = parentVisitor;
            _queryLogger = parentVisitor.QueryCompilationContext.Logger;
            _resultCoordinatorParameter = Parameter(
                splitQuery ? typeof(SplitQueryResultCoordinator) : typeof(SingleQueryResultCoordinator), "resultCoordinator");
            _executionStrategyParameter = splitQuery ? Parameter(typeof(IExecutionStrategy), "executionStrategy") : null;
            _selectExpression = selectExpression;
            _tags = tags;
            _dataReaderParameter = Parameter(typeof(DbDataReader), "dataReader");
            _resultContextParameter = Parameter(typeof(ResultContext), "resultContext");
            _indexMapParameter = indexMap ? Parameter(typeof(int[]), "indexMap") : null;
            if (parentVisitor.QueryCompilationContext.IsBuffering)
            {
                _readerColumns = new ReaderColumn?[_selectExpression.Projection.Count];
            }

            _generateCommandResolver = true;
            _detailedErrorsEnabled = parentVisitor._detailedErrorsEnabled;
            _isTracking = parentVisitor.QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll;
            _queryStateManager = parentVisitor.QueryCompilationContext.QueryTrackingBehavior is QueryTrackingBehavior.TrackAll
                or QueryTrackingBehavior.NoTrackingWithIdentityResolution;
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
            _queryLogger = parentVisitor.QueryCompilationContext.Logger;
            _resultCoordinatorParameter = resultCoordinatorParameter;

            _selectExpression = selectExpression;
            _dataReaderParameter = dataReaderParameter;
            _resultContextParameter = resultContextParameter;
            _readerColumns = readerColumns;
            _generateCommandResolver = false;
            _detailedErrorsEnabled = parentVisitor._detailedErrorsEnabled;
            _isTracking = parentVisitor.QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll;
            _queryStateManager = parentVisitor.QueryCompilationContext.QueryTrackingBehavior is QueryTrackingBehavior.TrackAll
                or QueryTrackingBehavior.NoTrackingWithIdentityResolution;
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
            _queryLogger = parentVisitor.QueryCompilationContext.Logger;
            _resultCoordinatorParameter = resultCoordinatorParameter;
            _executionStrategyParameter = executionStrategyParameter;

            _selectExpression = selectExpression;
            _tags = tags;
            _dataReaderParameter = Parameter(typeof(DbDataReader), "dataReader");
            _resultContextParameter = Parameter(typeof(ResultContext), "resultContext");
            if (parentVisitor.QueryCompilationContext.IsBuffering)
            {
                _readerColumns = new ReaderColumn[_selectExpression.Projection.Count];
            }

            _generateCommandResolver = true;
            _detailedErrorsEnabled = parentVisitor._detailedErrorsEnabled;
            _isTracking = parentVisitor.QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll;
            _queryStateManager = parentVisitor.QueryCompilationContext.QueryTrackingBehavior is QueryTrackingBehavior.TrackAll
                or QueryTrackingBehavior.NoTrackingWithIdentityResolution;
            _isAsync = parentVisitor.QueryCompilationContext.IsAsync;
            _splitQuery = true;

            _selectExpression.ApplyTags(_tags);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public LambdaExpression ProcessRelationalGroupingResult(
            RelationalGroupByResultExpression relationalGroupByResultExpression,
            out Expression relationalCommandResolver,
            out IReadOnlyList<ReaderColumn?>? readerColumns,
            out LambdaExpression keySelector,
            out LambdaExpression keyIdentifier,
            out LambdaExpression? relatedDataLoaders,
            ref int collectionId)
        {
            _inline = true;
            keySelector = Lambda(
                Visit(relationalGroupByResultExpression.KeyShaper),
                QueryCompilationContext.QueryContextParameter,
                _dataReaderParameter);

            keyIdentifier = Lambda(
                Visit(relationalGroupByResultExpression.KeyIdentifier),
                QueryCompilationContext.QueryContextParameter,
                _dataReaderParameter);

            _inline = false;

            return ProcessShaper(
                relationalGroupByResultExpression.ElementShaper,
                out relationalCommandResolver!,
                out readerColumns,
                out relatedDataLoaders,
                ref collectionId);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public LambdaExpression ProcessShaper(
            Expression shaperExpression,
            out Expression relationalCommandResolver,
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
                result = Block(_variables, _expressions);

                relationalCommandResolver = _parentVisitor.CreateRelationalCommandResolverExpression(_selectExpression);
                readerColumns = _readerColumns;

                return Lambda(
                    result,
                    QueryCompilationContext.QueryContextParameter,
                    _dataReaderParameter,
                    _indexMapParameter);
            }

            _containsCollectionMaterialization = new CollectionShaperFindingExpressionVisitor()
                .ContainsCollectionMaterialization(shaperExpression);

            // for NoTrackingWithIdentityResolution we need to make sure we see JSON entities in the correct order
            // specifically, if we project JSON collection, it needs to be projected before any individual element from that collection
            // otherwise we store JSON entities in incorrect order in the Change Tracker, leading to possible data corruption
            // we only need to do this once, on top level
            // see issue #33073 for more context
            if (_queryStateManager && !_isTracking && collectionId == 0)
            {
                var jsonCorrectOrderOfEntitiesForChangeTrackerValidator =
                    new JsonCorrectOrderOfEntitiesForChangeTrackerValidator(_selectExpression);
                jsonCorrectOrderOfEntitiesForChangeTrackerValidator.Validate(shaperExpression);
            }

            if (!_containsCollectionMaterialization)
            {
                var result = Visit(shaperExpression);
                _expressions.AddRange(_includeExpressions);
                _expressions.AddRange(_jsonEntityExpressions);
                _expressions.Add(result);
                result = Block(_variables, _expressions);

                relationalCommandResolver = _generateCommandResolver
                    ? _parentVisitor.CreateRelationalCommandResolverExpression(_selectExpression)
                    : Constant(null, typeof(RelationalCommandResolver));
                readerColumns = _readerColumns;

                return Lambda(
                    result,
                    QueryCompilationContext.QueryContextParameter,
                    _dataReaderParameter,
                    _resultContextParameter,
                    _resultCoordinatorParameter);
            }
            else
            {
                _valuesArrayExpression = MakeMemberAccess(_resultContextParameter, ResultContextValuesMemberInfo);
                _collectionPopulatingExpressions = [];
                _valuesArrayInitializers = [];

                var result = Visit(shaperExpression);

                var valueArrayInitializationExpression = Assign(
                    _valuesArrayExpression, NewArrayInit(typeof(object), _valuesArrayInitializers));

                _expressions.AddRange(_jsonEntityExpressions);
                _expressions.Add(valueArrayInitializationExpression);
                _expressions.AddRange(_includeExpressions);

                if (_splitQuery)
                {
                    _expressions.Add(Default(result.Type));

                    var initializationBlock = Block(_variables, _expressions);
                    result = Condition(
                        Equal(_valuesArrayExpression, Constant(null, typeof(object[]))),
                        initializationBlock,
                        result);

                    if (_isAsync)
                    {
                        var tasks = NewArrayInit(
                            typeof(Func<Task>), _collectionPopulatingExpressions.Select(
                                e => Lambda<Func<Task>>(e)));
                        relatedDataLoaders =
                            Lambda<Func<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator, Task>>(
                                Call(TaskAwaiterMethodInfo, tasks),
                                QueryCompilationContext.QueryContextParameter,
                                _executionStrategyParameter!,
                                _resultCoordinatorParameter);
                    }
                    else
                    {
                        relatedDataLoaders =
                            Lambda<Action<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator>>(
                                Block(_collectionPopulatingExpressions),
                                QueryCompilationContext.QueryContextParameter,
                                _executionStrategyParameter!,
                                _resultCoordinatorParameter);
                    }
                }
                else
                {
                    var initializationBlock = Block(_variables, _expressions);

                    var conditionalMaterializationExpressions = new List<Expression>
                    {
                        IfThen(
                            Equal(_valuesArrayExpression, Constant(null, typeof(object[]))),
                            initializationBlock)
                    };

                    conditionalMaterializationExpressions.AddRange(_collectionPopulatingExpressions);

                    conditionalMaterializationExpressions.Add(
                        Condition(
                            IsTrue(
                                MakeMemberAccess(
                                    _resultCoordinatorParameter, SingleQueryResultCoordinatorResultReadyMemberInfo)),
                            result,
                            Default(result.Type)));

                    result = Block(conditionalMaterializationExpressions);
                }

                relationalCommandResolver = _generateCommandResolver
                    ? _parentVisitor.CreateRelationalCommandResolverExpression(_selectExpression)
                    : Constant(null, typeof(RelationalCommandCache));
                ;
                readerColumns = _readerColumns;

                collectionId = _collectionId;

                return Lambda(
                    result,
                    QueryCompilationContext.QueryContextParameter,
                    _dataReaderParameter,
                    _resultContextParameter,
                    _resultCoordinatorParameter);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            switch (binaryExpression)
            {
                case { NodeType: ExpressionType.Assign, Left: ParameterExpression parameterExpression }
                    when parameterExpression.Type == typeof(MaterializationContext):
                {
                    var newExpression = (NewExpression)binaryExpression.Right;

                    if (newExpression.Arguments[0] is ProjectionBindingExpression projectionBindingExpression)
                    {
                        var projectionIndex = GetProjectionIndex(projectionBindingExpression);
                        var propertyMap = projectionIndex is IDictionary<IProperty, int>
                            ? (IDictionary<IProperty, int>)projectionIndex
                            : ((QueryableJsonProjectionInfo)projectionIndex).PropertyIndexMap;

                        _materializationContextBindings[parameterExpression] = propertyMap;
                        _entityTypeIdentifyingExpressionInfo[parameterExpression] =
                            // If single entity type is being selected in hierarchy then we use the value directly else we store the offset
                            // to read discriminator value.
                            _singleEntityTypeDiscriminatorValues.TryGetValue(projectionBindingExpression, out var value)
                                ? value
                                : propertyMap.Values.Max() + 1;

                        var updatedExpression = newExpression.Update(
                            new[]
                            {
                                _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                    ValueBuffer.Empty,
                                    static _ => ValueBuffer.Empty,
                                    "emptyValueBuffer",
                                    typeof(ValueBuffer)),
                                newExpression.Arguments[1]
                            });

                        return Assign(binaryExpression.Left, updatedExpression);
                    }

                    if (newExpression.Arguments[0] is ParameterExpression valueBufferParameter
                        && _jsonValueBufferToJsonReaderDataAndKeyValuesParameterMapping.TryGetValue(
                            valueBufferParameter, out var mappedParameter))
                    {
                        _jsonMaterializationContextToJsonReaderDataAndKeyValuesParameterMapping[parameterExpression] = mappedParameter;

                        var updatedExpression = newExpression.Update(
                            new[]
                            {
                                _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                    ValueBuffer.Empty,
                                    static _ => ValueBuffer.Empty,
                                    "emptyValueBuffer",
                                    typeof(ValueBuffer)),
                                newExpression.Arguments[1]
                            });

                        return Assign(binaryExpression.Left, updatedExpression);
                    }

                    break;
                }

                case
                {
                    NodeType: ExpressionType.Assign,
                    Left: MemberExpression { Member: FieldInfo { IsInitOnly: true } } memberExpression
                }:
                {
                    return memberExpression.Assign(Visit(binaryExpression.Right));
                }

                // we only have mapping between MaterializationContext and JsonReaderData, but we use JsonReaderManager to extract JSON
                // values so we need to add mapping between JsonReaderData and JsonReaderManager parameter, so we know which parameter to
                // use when generating actual Get* method
                case { NodeType: ExpressionType.Assign, Left: ParameterExpression jsonReaderManagerParameter }
                    when jsonReaderManagerParameter.Type == typeof(Utf8JsonReaderManager):
                {
                    var jsonReaderDataParameter = (ParameterExpression)((NewExpression)binaryExpression.Right).Arguments[0];
                    _jsonReaderDataToJsonReaderManagerParameterMapping[jsonReaderDataParameter] = jsonReaderManagerParameter;
                    break;
                }
            }

            return base.VisitBinary(binaryExpression);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case RelationalStructuralTypeShaperExpression
                    {
                        ValueBufferExpression: ProjectionBindingExpression projectionBindingExpression
                    } shaper
                    when !_inline:
                {
                    // we can't cache ProjectionBindingExpression results for non-tracking queries
                    // JSON entities must be read and re-shaped every time (streaming)
                    // as part of the process we do fixup to the parents, so those JSON entities would be potentially fixed up multiple times
                    // it's ok for references (overwrite) but for collections they would be added multiple times if we were to cache the parent
                    // by creating every entity every time we guarantee this doesn't happen
                    if (!_isTracking || !_variableShaperMapping.TryGetValue(projectionBindingExpression, out var accessor))
                    {
                        if (GetProjectionIndex(projectionBindingExpression) is JsonProjectionInfo jsonProjectionInfo)
                        {
                            Check.DebugAssert(shaper.StructuralType is IEntityType, "JsonProjectionInfo over a complex type");
                            var entityType = (IEntityType)shaper.StructuralType;

                            if (_isTracking)
                            {
                                throw new InvalidOperationException(
                                    RelationalStrings.JsonEntityOrCollectionProjectedAtRootLevelInTrackingQuery(
                                        nameof(EntityFrameworkQueryableExtensions.AsNoTracking)));
                            }

                            // json entity at the root
                            var (jsonReaderDataVariable, keyValuesParameter) = JsonShapingPreProcess(
                                jsonProjectionInfo,
                                entityType,
                                isCollection: false);

                            var shaperResult = CreateJsonShapers(
                                entityType,
                                shaper.IsNullable,
                                jsonReaderDataVariable,
                                keyValuesParameter,
                                parentEntityExpression: null,
                                navigation: null);

                            var visitedShaperResult = Visit(shaperResult);
                            var visitedShaperResultParameter = Parameter(visitedShaperResult.Type);
                            _variables.Add(visitedShaperResultParameter);
                            _jsonEntityExpressions.Add(Assign(visitedShaperResultParameter, visitedShaperResult));

                            accessor = CompensateForCollectionMaterialization(
                                visitedShaperResultParameter,
                                shaper.Type);
                        }
                        else if (GetProjectionIndex(projectionBindingExpression) is QueryableJsonProjectionInfo
                                 queryableJsonEntityProjectionInfo)
                        {
                            if (_isTracking)
                            {
                                throw new InvalidOperationException(
                                    RelationalStrings.JsonEntityOrCollectionProjectedAtRootLevelInTrackingQuery(
                                        nameof(EntityFrameworkQueryableExtensions.AsNoTracking)));
                            }

                            // json entity converted to query root and projected
                            var entityParameter = Parameter(shaper.Type);
                            _variables.Add(entityParameter);
                            var entityMaterializationExpression = (BlockExpression)_parentVisitor.InjectEntityMaterializers(shaper);

                            var mappedProperties = queryableJsonEntityProjectionInfo.PropertyIndexMap.Keys.ToList();
                            var rewrittenEntityMaterializationExpression = new QueryableJsonEntityMaterializerRewriter(mappedProperties)
                                .Rewrite(entityMaterializationExpression);

                            var visitedEntityMaterializationExpression = Visit(rewrittenEntityMaterializationExpression);
                            _expressions.Add(Assign(entityParameter, visitedEntityMaterializationExpression));

                            foreach (var childProjectionInfo in queryableJsonEntityProjectionInfo.ChildrenProjectionInfo)
                            {
                                var (jsonReaderDataVariable, keyValuesParameter) = JsonShapingPreProcess(
                                    childProjectionInfo.JsonProjectionInfo,
                                    childProjectionInfo.Navigation.TargetEntityType,
                                    childProjectionInfo.Navigation.IsCollection);

                                var shaperResult = CreateJsonShapers(
                                    childProjectionInfo.Navigation.TargetEntityType,
                                    nullable: true,
                                    jsonReaderDataVariable,
                                    keyValuesParameter,
                                    parentEntityExpression: entityParameter,
                                    navigation: childProjectionInfo.Navigation);

                                var visitedShaperResult = Visit(shaperResult);

                                _includeExpressions.Add(visitedShaperResult);
                            }

                            accessor = CompensateForCollectionMaterialization(
                                entityParameter,
                                shaper.Type);
                        }
                        else
                        {
                            var entityParameter = Parameter(shaper.Type, "entity");
                            _variables.Add(entityParameter);
                            if (shaper.StructuralType is IEntityType entityType
                                && entityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy)
                            {
                                var concreteTypes = entityType.GetDerivedTypesInclusive().Where(e => !e.IsAbstract()).ToArray();
                                // Single concrete TPC entity type won't have discriminator column.
                                // We store the value here and inject it directly rather than reading from server.
                                if (concreteTypes.Length == 1)
                                {
                                    _singleEntityTypeDiscriminatorValues[
                                            projectionBindingExpression]
                                        = concreteTypes[0].ShortName();
                                }
                            }

                            var entityMaterializationExpression = _parentVisitor.InjectEntityMaterializers(shaper);
                            entityMaterializationExpression = Visit(entityMaterializationExpression);

                            _expressions.Add(Assign(entityParameter, entityMaterializationExpression));

                            accessor = CompensateForCollectionMaterialization(
                                entityParameter,
                                shaper.Type);
                        }

                        if (_isTracking)
                        {
                            _variableShaperMapping[projectionBindingExpression] = accessor;
                        }
                    }

                    return accessor;
                }

                case RelationalStructuralTypeShaperExpression { ValueBufferExpression: ProjectionBindingExpression } shaper
                    when _inline:
                {
                    if (shaper.StructuralType is IEntityType entityType
                        && entityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy)
                    {
                        var concreteTypes = entityType.GetDerivedTypesInclusive().Where(e => !e.IsAbstract()).ToArray();
                        // Single concrete TPC entity type won't have discriminator column.
                        // We store the value here and inject it directly rather than reading from server.
                        if (concreteTypes.Length == 1)
                        {
                            _singleEntityTypeDiscriminatorValues[
                                    (ProjectionBindingExpression)shaper.ValueBufferExpression]
                                = concreteTypes[0].ShortName();
                        }
                    }

                    var entityMaterializationExpression = _parentVisitor.InjectEntityMaterializers(shaper);
                    entityMaterializationExpression = Visit(entityMaterializationExpression);

                    return entityMaterializationExpression;
                }

                case CollectionResultExpression { Navigation: INavigation navigation } collectionResultExpression
                    when GetProjectionIndex(collectionResultExpression.ProjectionBindingExpression)
                        is JsonProjectionInfo jsonProjectionInfo:
                {
                    if (_isTracking)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.JsonEntityOrCollectionProjectedAtRootLevelInTrackingQuery(
                                nameof(EntityFrameworkQueryableExtensions.AsNoTracking)));
                    }

                    // json entity collection at the root
                    var (jsonReaderDataVariable, keyValuesParameter) = JsonShapingPreProcess(
                        jsonProjectionInfo,
                        navigation.TargetEntityType,
                        isCollection: true);

                    var shaperResult = CreateJsonShapers(
                        navigation.TargetEntityType,
                        nullable: true,
                        jsonReaderDataVariable,
                        keyValuesParameter,
                        parentEntityExpression: null,
                        navigation: navigation);

                    var visitedShaperResult = Visit(shaperResult);

                    var jsonCollectionParameter = Parameter(collectionResultExpression.Type);

                    _variables.Add(jsonCollectionParameter);
                    _jsonEntityExpressions.Add(Assign(jsonCollectionParameter, visitedShaperResult));

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

                    var valueParameter = Parameter(projectionBindingExpression.Type, "value" + (_variables.Count + 1));
                    _variables.Add(valueParameter);

                    _expressions.Add(
                        Assign(
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
                            expressionToAdd = Convert(expressionToAdd, typeof(object));
                        }

                        _valuesArrayInitializers!.Add(expressionToAdd);
                        accessor = Convert(
                            ArrayIndex(
                                _valuesArrayExpression!,
                                Constant(_valuesArrayInitializers.Count - 1)),
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
                        var collectionIdConstant = Constant(_collectionId++);
                        var innerShaper = new ShaperProcessingExpressionVisitor(
                                _parentVisitor, _resultCoordinatorParameter, _selectExpression, _dataReaderParameter,
                                _resultContextParameter,
                                _readerColumns)
                            .ProcessShaper(relationalCollectionShaperExpression.InnerShaper, out _, out _, out _, ref _collectionId);

                        var entityClrType = entity.Type;
                        var navigation = includeExpression.Navigation;
                        var includingEntityClrType = navigation.DeclaringEntityType.ClrType;
                        if (includingEntityClrType != entityClrType
                            && includingEntityClrType.IsAssignableFrom(entityClrType))
                        {
                            includingEntityClrType = entityClrType;
                        }

                        _inline = true;

                        var parentIdentifierLambda = Lambda(
                            Visit(relationalCollectionShaperExpression.ParentIdentifier),
                            QueryCompilationContext.QueryContextParameter,
                            _dataReaderParameter);

                        var parentIdentifierExpression = UseOldBehavior35212
                            ? parentIdentifierLambda
                            : _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                parentIdentifierLambda.Compile(),
                                Lambda<Func<MaterializerLiftableConstantContext, object>>(
                                    parentIdentifierLambda,
                                    Parameter(typeof(MaterializerLiftableConstantContext), "_")),
                                "parentIdentifierLambda",
                                typeof(Func<QueryContext, DbDataReader, object[]>));

                        var outerIdentifierLambda = Lambda(
                            Visit(relationalCollectionShaperExpression.OuterIdentifier),
                            QueryCompilationContext.QueryContextParameter,
                            _dataReaderParameter);

                        var outerIdentifierExpression = UseOldBehavior35212
                            ? outerIdentifierLambda
                            : _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                outerIdentifierLambda.Compile(),
                                Lambda<Func<MaterializerLiftableConstantContext, object>>(
                                    outerIdentifierLambda,
                                    Parameter(typeof(MaterializerLiftableConstantContext), "_")),
                                "outerIdentifierLambda",
                                typeof(Func<QueryContext, DbDataReader, object[]>));

                        var selfIdentifierLambda = Lambda(
                            Visit(relationalCollectionShaperExpression.SelfIdentifier),
                            QueryCompilationContext.QueryContextParameter,
                            _dataReaderParameter);

                        var selfIdentifierExpression = UseOldBehavior35212
                            ? selfIdentifierLambda
                            : _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                selfIdentifierLambda.Compile(),
                                Lambda<Func<MaterializerLiftableConstantContext, object>>(
                                    selfIdentifierLambda,
                                    Parameter(typeof(MaterializerLiftableConstantContext), "_")),
                                "selfIdentifierLambda",
                                typeof(Func<QueryContext, DbDataReader, object[]>));

                        _inline = false;

                        _includeExpressions.Add(
                            Call(
                                InitializeIncludeCollectionMethodInfo.MakeGenericMethod(entityClrType, includingEntityClrType),
                                collectionIdConstant,
                                QueryCompilationContext.QueryContextParameter,
                                _dataReaderParameter,
                                _resultCoordinatorParameter,
                                entity,
                                parentIdentifierExpression,
                                outerIdentifierExpression,
                                _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                    navigation,
                                    LiftableConstantExpressionHelpers.BuildNavigationAccessLambda(navigation),
                                    navigation.Name + "Navigation",
                                    typeof(INavigationBase)),
                                navigation.IsShadowProperty()
                                    ? Constant(null, typeof(IClrCollectionAccessor))
                                    : _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                        navigation.GetCollectionAccessor(),
                                        LiftableConstantExpressionHelpers.BuildClrCollectionAccessorLambda(navigation),
                                        navigation.Name + "NavigationCollectionAccessor",
                                        typeof(IClrCollectionAccessor)),
                                Constant(_isTracking),
#pragma warning disable EF1001 // Internal EF Core API usage.
                                Constant(includeExpression.SetLoaded)));
#pragma warning restore EF1001 // Internal EF Core API usage.

                        var relatedEntityClrType = innerShaper.ReturnType;
                        var inverseNavigation = navigation.Inverse;

                        _collectionPopulatingExpressions!.Add(
                            Call(
                                PopulateIncludeCollectionMethodInfo.MakeGenericMethod(includingEntityClrType, relatedEntityClrType),
                                collectionIdConstant,
                                QueryCompilationContext.QueryContextParameter,
                                _dataReaderParameter,
                                _resultCoordinatorParameter,
                                parentIdentifierExpression,
                                outerIdentifierExpression,
                                selfIdentifierExpression,
                                _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                    relationalCollectionShaperExpression.ParentIdentifierValueComparers
                                        .Select(x => (Func<object, object, bool>)x.Equals).ToArray(),
                                    Lambda<Func<MaterializerLiftableConstantContext, object>>(
                                        NewArrayInit(
                                            typeof(Func<object, object, bool>),
                                            relationalCollectionShaperExpression.ParentIdentifierValueComparers.Select(
                                                vc => vc.ObjectEqualsExpression)),
                                        Parameter(typeof(MaterializerLiftableConstantContext), "_")),
                                    "parentIdentifierValueComparers",
                                    typeof(Func<object, object, bool>[])),
                                _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                    relationalCollectionShaperExpression.OuterIdentifierValueComparers
                                        .Select(x => (Func<object, object, bool>)x.Equals).ToArray(),
                                    Lambda<Func<MaterializerLiftableConstantContext, object>>(
                                        NewArrayInit(
                                            typeof(Func<object, object, bool>),
                                            relationalCollectionShaperExpression.OuterIdentifierValueComparers.Select(
                                                vc => vc.ObjectEqualsExpression)),
                                        Parameter(typeof(MaterializerLiftableConstantContext), "_")),
                                    "outerIdentifierValueComparers",
                                    typeof(Func<object, object, bool>[])),
                                _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                    relationalCollectionShaperExpression.SelfIdentifierValueComparers
                                        .Select(x => (Func<object, object, bool>)x.Equals).ToArray(),
                                    Lambda<Func<MaterializerLiftableConstantContext, object>>(
                                        NewArrayInit(
                                            typeof(Func<object, object, bool>),
                                            relationalCollectionShaperExpression.SelfIdentifierValueComparers.Select(
                                                vc => vc.ObjectEqualsExpression)),
                                        Parameter(typeof(MaterializerLiftableConstantContext), "_")),
                                    "selfIdentifierValueComparers",
                                    typeof(Func<object, object, bool>[])),
                                innerShaper,
                                _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                    inverseNavigation,
                                    LiftableConstantExpressionHelpers.BuildNavigationAccessLambda(inverseNavigation),
                                    (inverseNavigation?.Name ?? "null") + "InverseNavigation",
                                    typeof(INavigationBase)),
                                GenerateFixup(includingEntityClrType, relatedEntityClrType, navigation, inverseNavigation),
                                Constant(_isTracking)));
                    }
                    else if (includeExpression.NavigationExpression is RelationalSplitCollectionShaperExpression
                             relationalSplitCollectionShaperExpression)
                    {
                        var collectionIdConstant = Constant(_collectionId++);
                        var innerProcessor = new ShaperProcessingExpressionVisitor(
                            _parentVisitor, _resultCoordinatorParameter,
                            _executionStrategyParameter!, relationalSplitCollectionShaperExpression.SelectExpression, _tags!);
                        var innerShaper = innerProcessor.ProcessShaper(
                            relationalSplitCollectionShaperExpression.InnerShaper,
                            out var relationalCommandResolver,
                            out var readerColumns,
                            out var relatedDataLoaders,
                            ref _collectionId);

                        var entityType = entity.Type;
                        var navigation = includeExpression.Navigation;
                        var includingEntityClrType = navigation.DeclaringEntityType.ClrType;
                        if (includingEntityClrType != entityType
                            && includingEntityClrType.IsAssignableFrom(entityType))
                        {
                            includingEntityClrType = entityType;
                        }

                        _inline = true;

                        var parentIdentifierLambda = Lambda(
                            Visit(relationalSplitCollectionShaperExpression.ParentIdentifier),
                            QueryCompilationContext.QueryContextParameter,
                            _dataReaderParameter);

                        var parentIdentifierExpression = UseOldBehavior35212
                            ? parentIdentifierLambda
                            : _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                parentIdentifierLambda.Compile(),
                                Lambda<Func<MaterializerLiftableConstantContext, object>>(
                                    parentIdentifierLambda,
                                    Parameter(typeof(MaterializerLiftableConstantContext), "_")),
                                "parentIdentifierLambda",
                                typeof(Func<QueryContext, DbDataReader, object[]>));

                        _inline = false;

                        innerProcessor._inline = true;

                        var childIdentifierLambda = Lambda(
                            innerProcessor.Visit(relationalSplitCollectionShaperExpression.ChildIdentifier),
                            QueryCompilationContext.QueryContextParameter,
                            innerProcessor._dataReaderParameter);

                        var childIdentifierExpression = UseOldBehavior35212
                            ? childIdentifierLambda
                            : _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                childIdentifierLambda.Compile(),
                                Lambda<Func<MaterializerLiftableConstantContext, object>>(
                                    childIdentifierLambda,
                                    Parameter(typeof(MaterializerLiftableConstantContext), "_")),
                                "childIdentifierLambda",
                                typeof(Func<QueryContext, DbDataReader, object[]>));

                        innerProcessor._inline = false;

                        _includeExpressions.Add(
                            Call(
                                InitializeSplitIncludeCollectionMethodInfo.MakeGenericMethod(entityType, includingEntityClrType),
                                collectionIdConstant,
                                QueryCompilationContext.QueryContextParameter,
                                _dataReaderParameter,
                                _resultCoordinatorParameter,
                                entity,
                                parentIdentifierExpression,
                                _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                    navigation,
                                    LiftableConstantExpressionHelpers.BuildNavigationAccessLambda(navigation),
                                    navigation.Name + "Navigation",
                                    typeof(INavigationBase)),
                                _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                    navigation.GetCollectionAccessor(),
                                    LiftableConstantExpressionHelpers.BuildClrCollectionAccessorLambda(navigation),
                                    navigation.Name + "NavigationCollectionAccessor",
                                    typeof(IClrCollectionAccessor)),
                                Constant(_isTracking),
#pragma warning disable EF1001 // Internal EF Core API usage.
                                Constant(includeExpression.SetLoaded)));
#pragma warning restore EF1001 // Internal EF Core API usage.

                        var relatedEntityClrType = innerShaper.ReturnType;
                        var inverseNavigation = navigation.Inverse;

                        _collectionPopulatingExpressions!.Add(
                            Call(
                                (_isAsync ? PopulateSplitIncludeCollectionAsyncMethodInfo : PopulateSplitIncludeCollectionMethodInfo)
                                .MakeGenericMethod(includingEntityClrType, relatedEntityClrType),
                                collectionIdConstant,
                                Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                                _executionStrategyParameter!,
                                relationalCommandResolver,
                                CreateReaderColumnsExpression(readerColumns, _parentVisitor.Dependencies.LiftableConstantFactory),
                                Constant(_detailedErrorsEnabled),
                                _resultCoordinatorParameter,
                                childIdentifierExpression,
                                _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                    relationalSplitCollectionShaperExpression.IdentifierValueComparers
                                        .Select(x => (Func<object, object, bool>)x.Equals).ToArray(),
                                    Lambda<Func<MaterializerLiftableConstantContext, object>>(
                                        NewArrayInit(
                                            typeof(Func<object, object, bool>),
                                            relationalSplitCollectionShaperExpression.IdentifierValueComparers.Select(
                                                vc => vc.ObjectEqualsExpression)),
                                        Parameter(typeof(MaterializerLiftableConstantContext), "_")),
                                    "identifierValueComparers",
                                    typeof(Func<object, object, bool>[])),
                                innerShaper,
                                relatedDataLoaders
                                ?? (Expression)Constant(
                                    null,
                                    _isAsync
                                        ? typeof(Func<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator, Task>)
                                        : typeof(Action<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator>)),
                                _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                    inverseNavigation,
                                    LiftableConstantExpressionHelpers.BuildNavigationAccessLambda(inverseNavigation),
                                    (inverseNavigation?.Name ?? "null") + "InverseNavigation",
                                    typeof(INavigationBase)),
                                GenerateFixup(includingEntityClrType, relatedEntityClrType, navigation, inverseNavigation),
                                Constant(_isTracking)));
                    }
                    else
                    {
                        var projectionBindingExpression = (includeExpression.NavigationExpression as CollectionResultExpression)
                            ?.ProjectionBindingExpression
                            ?? (includeExpression.NavigationExpression as RelationalStructuralTypeShaperExpression)
                            ?.ValueBufferExpression as
                            ProjectionBindingExpression;

                        // json include case
                        if (projectionBindingExpression != null
                            && GetProjectionIndex(projectionBindingExpression) is JsonProjectionInfo jsonProjectionInfo)
                        {
                            var (jsonReaderDataVariable, keyValuesParameter) = JsonShapingPreProcess(
                                jsonProjectionInfo,
                                includeExpression.Navigation.TargetEntityType,
                                includeExpression.Navigation.IsCollection);

                            var shaperResult = CreateJsonShapers(
                                includeExpression.Navigation.TargetEntityType,
                                nullable: true,
                                jsonReaderDataVariable,
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

                        var updatedExpression = Call(
                            IncludeReferenceMethodInfo.MakeGenericMethod(entityType, includingType, relatedEntityType),
                            QueryCompilationContext.QueryContextParameter,
                            entity,
                            navigationExpression,
                            _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                navigation,
                                LiftableConstantExpressionHelpers.BuildNavigationAccessLambda(navigation),
                                navigation.Name + "Navigation",
                                typeof(INavigation)),
                            _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                inverseNavigation,
                                LiftableConstantExpressionHelpers.BuildNavigationAccessLambda(inverseNavigation),
                                (inverseNavigation?.Name ?? "null") + "InverseNavigation",
                                typeof(INavigation)),
                            GenerateFixup(includingType, relatedEntityType, navigation, inverseNavigation),
                            Constant(_isTracking));

                        _includeExpressions.Add(updatedExpression);
                    }

                    return entity;
                }

                case RelationalCollectionShaperExpression relationalCollectionShaperExpression:
                {
                    if (!_variableShaperMapping.TryGetValue(relationalCollectionShaperExpression, out var accessor))
                    {
                        var collectionIdConstant = Constant(_collectionId++);
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

                        var parentIdentifierLambda = Lambda(
                            Visit(relationalCollectionShaperExpression.ParentIdentifier),
                            QueryCompilationContext.QueryContextParameter,
                            _dataReaderParameter);

                        var parentIdentifierExpression = UseOldBehavior35212
                            ? parentIdentifierLambda
                            : _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                parentIdentifierLambda.Compile(),
                                Lambda<Func<MaterializerLiftableConstantContext, object>>(
                                    parentIdentifierLambda,
                                    Parameter(typeof(MaterializerLiftableConstantContext), "_")),
                                "parentIdentifierLambda",
                                typeof(Func<QueryContext, DbDataReader, object[]>));

                        var outerIdentifierLambda = Lambda(
                            Visit(relationalCollectionShaperExpression.OuterIdentifier),
                            QueryCompilationContext.QueryContextParameter,
                            _dataReaderParameter);

                        var outerIdentifierExpression = UseOldBehavior35212
                            ? outerIdentifierLambda
                            : _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                outerIdentifierLambda.Compile(),
                                Lambda<Func<MaterializerLiftableConstantContext, object>>(
                                    outerIdentifierLambda,
                                    Parameter(typeof(MaterializerLiftableConstantContext), "_")),
                                "outerIdentifierLambda",
                                typeof(Func<QueryContext, DbDataReader, object[]>));

                        var selfIdentifierLambda = Lambda(
                            Visit(relationalCollectionShaperExpression.SelfIdentifier),
                            QueryCompilationContext.QueryContextParameter,
                            _dataReaderParameter);

                        var selfIdentifierExpression = UseOldBehavior35212
                            ? selfIdentifierLambda
                            : _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                selfIdentifierLambda.Compile(),
                                Lambda<Func<MaterializerLiftableConstantContext, object>>(
                                    selfIdentifierLambda,
                                    Parameter(typeof(MaterializerLiftableConstantContext), "_")),
                                "selfIdentifierLambda",
                                typeof(Func<QueryContext, DbDataReader, object[]>));

                        _inline = false;

                        var collectionParameter = Parameter(relationalCollectionShaperExpression.Type);
                        _variables.Add(collectionParameter);
                        _expressions.Add(
                            Assign(
                                collectionParameter,
                                Call(
                                    InitializeCollectionMethodInfo.MakeGenericMethod(elementType, collectionType),
                                    collectionIdConstant,
                                    QueryCompilationContext.QueryContextParameter,
                                    _dataReaderParameter,
                                    _resultCoordinatorParameter,
                                    parentIdentifierExpression,
                                    outerIdentifierExpression,
                                    _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                        collectionAccessor,
                                        LiftableConstantExpressionHelpers.BuildClrCollectionAccessorLambda(navigation),
                                        (navigation?.Name ?? "null") + "ClrCollectionAccessor",
                                        typeof(IClrCollectionAccessor)))));

                        _valuesArrayInitializers!.Add(collectionParameter);
                        accessor = Convert(
                            ArrayIndex(
                                _valuesArrayExpression!,
                                Constant(_valuesArrayInitializers.Count - 1)),
                            relationalCollectionShaperExpression.Type);

                        _collectionPopulatingExpressions!.Add(
                            Call(
                                PopulateCollectionMethodInfo.MakeGenericMethod(collectionType, elementType, relatedElementType),
                                collectionIdConstant,
                                QueryCompilationContext.QueryContextParameter,
                                _dataReaderParameter,
                                _resultCoordinatorParameter,
                                parentIdentifierExpression,
                                outerIdentifierExpression,
                                selfIdentifierExpression,
                                _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                    relationalCollectionShaperExpression.ParentIdentifierValueComparers
                                        .Select(x => (Func<object, object, bool>)x.Equals).ToArray(),
                                    Lambda<Func<MaterializerLiftableConstantContext, object>>(
                                        NewArrayInit(
                                            typeof(Func<object, object, bool>),
                                            relationalCollectionShaperExpression.ParentIdentifierValueComparers.Select(
                                                vc => vc.ObjectEqualsExpression)),
                                        Parameter(typeof(MaterializerLiftableConstantContext), "_")),
                                    "parentIdentifierValueComparers",
                                    typeof(Func<object, object, bool>[])),
                                _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                    relationalCollectionShaperExpression.OuterIdentifierValueComparers
                                        .Select(x => (Func<object, object, bool>)x.Equals).ToArray(),
                                    Lambda<Func<MaterializerLiftableConstantContext, object>>(
                                        NewArrayInit(
                                            typeof(Func<object, object, bool>),
                                            relationalCollectionShaperExpression.OuterIdentifierValueComparers.Select(
                                                vc => vc.ObjectEqualsExpression)),
                                        Parameter(typeof(MaterializerLiftableConstantContext), "_")),
                                    "parentIdentifierValueComparers",
                                    typeof(Func<object, object, bool>[])),
                                _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                    relationalCollectionShaperExpression.SelfIdentifierValueComparers
                                        .Select(x => (Func<object, object, bool>)x.Equals).ToArray(),
                                    Lambda<Func<MaterializerLiftableConstantContext, object>>(
                                        NewArrayInit(
                                            typeof(Func<object, object, bool>),
                                            relationalCollectionShaperExpression.SelfIdentifierValueComparers.Select(
                                                vc => vc.ObjectEqualsExpression)),
                                        Parameter(typeof(MaterializerLiftableConstantContext), "_")),
                                    "parentIdentifierValueComparers",
                                    typeof(Func<object, object, bool>[])),
                                innerShaper));

                        _variableShaperMapping[relationalCollectionShaperExpression] = accessor;
                    }

                    return accessor;
                }

                case RelationalSplitCollectionShaperExpression relationalSplitCollectionShaperExpression:
                {
                    if (!_variableShaperMapping.TryGetValue(relationalSplitCollectionShaperExpression, out var accessor))
                    {
                        var collectionIdConstant = Constant(_collectionId++);
                        var innerProcessor = new ShaperProcessingExpressionVisitor(
                            _parentVisitor, _resultCoordinatorParameter,
                            _executionStrategyParameter!, relationalSplitCollectionShaperExpression.SelectExpression, _tags!);
                        var innerShaper = innerProcessor.ProcessShaper(
                            relationalSplitCollectionShaperExpression.InnerShaper,
                            out var relationalCommandResolver,
                            out var readerColumns,
                            out var relatedDataLoaders,
                            ref _collectionId);

                        var navigation = relationalSplitCollectionShaperExpression.Navigation;
                        var collectionAccessor = navigation?.GetCollectionAccessor();
                        var collectionType = collectionAccessor?.CollectionType ?? relationalSplitCollectionShaperExpression.Type;
                        var elementType = relationalSplitCollectionShaperExpression.ElementType;
                        var relatedElementType = innerShaper.ReturnType;

                        _inline = true;

                        var parentIdentifierLambda = Lambda(
                            Visit(relationalSplitCollectionShaperExpression.ParentIdentifier),
                            QueryCompilationContext.QueryContextParameter,
                            _dataReaderParameter);

                        var parentIdentifierExpression = UseOldBehavior35212
                            ? parentIdentifierLambda
                            : _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                parentIdentifierLambda.Compile(),
                                Lambda<Func<MaterializerLiftableConstantContext, object>>(
                                    parentIdentifierLambda,
                                    Parameter(typeof(MaterializerLiftableConstantContext), "_")),
                                "parentIdentifierLambda",
                                typeof(Func<QueryContext, DbDataReader, object[]>));

                        _inline = false;

                        innerProcessor._inline = true;

                        var childIdentifierLambda = Lambda(
                            innerProcessor.Visit(relationalSplitCollectionShaperExpression.ChildIdentifier),
                            QueryCompilationContext.QueryContextParameter,
                            innerProcessor._dataReaderParameter);

                        var childIdentifierExpression = UseOldBehavior35212
                            ? childIdentifierLambda
                            : _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                childIdentifierLambda.Compile(),
                                Lambda<Func<MaterializerLiftableConstantContext, object>>(
                                    childIdentifierLambda,
                                    Parameter(typeof(MaterializerLiftableConstantContext), "_")),
                                "childIdentifierLambda",
                                typeof(Func<QueryContext, DbDataReader, object[]>));

                        innerProcessor._inline = false;

                        var collectionParameter = Parameter(collectionType);
                        _variables.Add(collectionParameter);

                        _expressions.Add(
                            Assign(
                                collectionParameter,
                                Call(
                                    InitializeSplitCollectionMethodInfo.MakeGenericMethod(elementType, collectionType),
                                    collectionIdConstant,
                                    QueryCompilationContext.QueryContextParameter,
                                    _dataReaderParameter,
                                    _resultCoordinatorParameter,
                                    parentIdentifierExpression,
                                    _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                        collectionAccessor,
                                        LiftableConstantExpressionHelpers.BuildClrCollectionAccessorLambda(navigation),
                                        (navigation?.Name ?? "null") + "CollectionAccessor",
                                        typeof(IClrCollectionAccessor)))));

                        _valuesArrayInitializers!.Add(collectionParameter);
                        accessor = Convert(
                            ArrayIndex(
                                _valuesArrayExpression!,
                                Constant(_valuesArrayInitializers.Count - 1)),
                            relationalSplitCollectionShaperExpression.Type);

                        _collectionPopulatingExpressions!.Add(
                            Call(
                                (_isAsync ? PopulateSplitCollectionAsyncMethodInfo : PopulateSplitCollectionMethodInfo)
                                .MakeGenericMethod(collectionType, elementType, relatedElementType),
                                collectionIdConstant,
                                Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                                _executionStrategyParameter!,
                                relationalCommandResolver,
                                CreateReaderColumnsExpression(readerColumns, _parentVisitor.Dependencies.LiftableConstantFactory),
                                Constant(_detailedErrorsEnabled),
                                _resultCoordinatorParameter,
                                childIdentifierExpression,
                                _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                    relationalSplitCollectionShaperExpression.IdentifierValueComparers
                                        .Select(x => (Func<object, object, bool>)x.Equals).ToArray(),
                                    Lambda<Func<MaterializerLiftableConstantContext, object>>(
                                        NewArrayInit(
                                            typeof(Func<object, object, bool>),
                                            relationalSplitCollectionShaperExpression.IdentifierValueComparers.Select(
                                                vc => vc.ObjectEqualsExpression)),
                                        Parameter(typeof(MaterializerLiftableConstantContext), "_")),
                                    "identifierValueComparers",
                                    typeof(Func<object, object, bool>[])),
                                innerShaper,
                                relatedDataLoaders == null
                                    ? Constant(
                                        null, _isAsync
                                            ? typeof(Func<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator, Task>)
                                            : typeof(Action<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator>))
                                    : relatedDataLoaders));

                        _variableShaperMapping[relationalSplitCollectionShaperExpression] = accessor;
                    }

                    return accessor;
                }

                case GroupByShaperExpression:
                    throw new InvalidOperationException(RelationalStrings.ClientGroupByNotSupported);

                case LiftableConstantExpression:
                    return extensionExpression;
            }

            return base.VisitExtension(extensionExpression);

            Expression CompensateForCollectionMaterialization(ParameterExpression parameter, Type resultType)
            {
                if (_containsCollectionMaterialization)
                {
                    _valuesArrayInitializers!.Add(parameter);
                    return Convert(
                        ArrayIndex(
                            _valuesArrayExpression!,
                            Constant(_valuesArrayInitializers.Count - 1)),
                        resultType);
                }

                return parameter;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition()
                == Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod)
            {
                var index = methodCallExpression.Arguments[1].GetConstantValue<int>();
                var property = methodCallExpression.Arguments[2].GetConstantValue<IProperty?>();
                var mappingParameter = (ParameterExpression)((MethodCallExpression)methodCallExpression.Arguments[0]).Object!;

                if (_jsonMaterializationContextToJsonReaderDataAndKeyValuesParameterMapping.TryGetValue(
                        mappingParameter, out var mappedParameter))
                {
                    var (jsonReaderDataParameter, keyPropertyValuesParameter) = mappedParameter;

                    if (property!.IsPrimaryKey())
                    {
                        var valueExpression = MakeIndex(
                            keyPropertyValuesParameter,
                            ObjectArrayIndexerPropertyInfo,
                            new[] { Constant(index) });
                        return methodCallExpression.Type != valueExpression.Type
                            ? Convert(valueExpression, methodCallExpression.Type)
                            : valueExpression;
                    }

                    var jsonReaderManagerParameter = _jsonReaderDataToJsonReaderManagerParameterMapping[jsonReaderDataParameter];

                    var jsonReadPropertyValueExpression = CreateReadJsonPropertyValueExpression(jsonReaderManagerParameter, property);

                    return methodCallExpression.Type != jsonReadPropertyValueExpression.Type
                        ? Convert(jsonReadPropertyValueExpression, methodCallExpression.Type)
                        : jsonReadPropertyValueExpression;
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
                        return Constant(s);
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
            ParameterExpression jsonReaderDataParameter,
            ParameterExpression keyValuesParameter,
            Expression? parentEntityExpression,
            INavigation? navigation)
        {
            var jsonReaderDataShaperLambdaParameter = Parameter(typeof(JsonReaderData));
            // TODO: Use ISnapshot instead #26544
            var keyValuesShaperLambdaParameter = Parameter(typeof(object[]));
            var shaperBlockVariables = new List<ParameterExpression>();
            var shaperBlockExpressions = new List<Expression>();

            var valueBufferParameter = Parameter(typeof(ValueBuffer));

            _jsonValueBufferToJsonReaderDataAndKeyValuesParameterMapping[valueBufferParameter] =
                (jsonReaderDataShaperLambdaParameter, keyValuesShaperLambdaParameter);

            var entityShaperExpression = new RelationalStructuralTypeShaperExpression(
                entityType,
                valueBufferParameter,
                nullable);

            var entityShaperMaterializer = (BlockExpression)_parentVisitor.InjectEntityMaterializers(entityShaperExpression);

            var innerShapersMap = new Dictionary<string, Expression>();
            var innerFixupMap = new Dictionary<string, LambdaExpression>();
            var trackingInnerFixupMap = new Dictionary<string, LambdaExpression>();
            foreach (var ownedNavigation in entityType.GetNavigations().Where(
                         n => n.TargetEntityType.IsMappedToJson() && n.ForeignKey.IsOwnership && n == n.ForeignKey.PrincipalToDependent))
            {
                // we need to build entity shapers and fixup separately
                // we don't know the order in which data comes, so we need to read through everything
                // before we can do fixup safely
                var innerShaper = CreateJsonShapers(
                    ownedNavigation.TargetEntityType,
                    nullable || !ownedNavigation.ForeignKey.IsRequired,
                    jsonReaderDataShaperLambdaParameter,
                    keyValuesShaperLambdaParameter,
                    parentEntityExpression: null,
                    navigation: ownedNavigation);

                var navigationJsonPropertyName = ownedNavigation.TargetEntityType.GetJsonPropertyName()!;
                innerShapersMap[navigationJsonPropertyName] = innerShaper;

                if (ownedNavigation.IsCollection)
                {
                    var shaperEntityParameter = Parameter(ownedNavigation.DeclaringEntityType.ClrType);
                    var ownedNavigationType = ownedNavigation.GetMemberInfo(forMaterialization: true, forSet: true).GetMemberType();
                    var shaperCollectionParameter = Parameter(ownedNavigationType);
                    var expressions = new List<Expression>();
                    var expressionsForTracking = new List<Expression>();

                    if (!ownedNavigation.IsShadowProperty())
                    {
                        expressions.Add(
                            shaperEntityParameter.MakeMemberAccess(ownedNavigation.GetMemberInfo(forMaterialization: true, forSet: true))
                                .Assign(shaperCollectionParameter));

                        expressionsForTracking.Add(
                            IfThen(
                                OrElse(
                                    ReferenceEqual(Constant(null), shaperCollectionParameter),
                                    IsFalse(
                                        Call(
                                            typeof(ShaperProcessingExpressionVisitor).GetMethod(nameof(Any))!,
                                            shaperCollectionParameter))),
                                shaperEntityParameter
                                    .MakeMemberAccess(ownedNavigation.GetMemberInfo(forMaterialization: true, forSet: true))
                                    .Assign(shaperCollectionParameter)));
                    }

                    if (ownedNavigation.Inverse is INavigation inverseNavigation
                        && !inverseNavigation.IsShadowProperty())
                    {
                        var innerFixupCollectionElementParameter = Parameter(inverseNavigation.DeclaringEntityType.ClrType);
                        var innerFixupParentParameter = Parameter(inverseNavigation.TargetEntityType.ClrType);

                        var elementFixup = Lambda(
                            Block(
                                typeof(void),
                                AssignReferenceNavigation(
                                    innerFixupCollectionElementParameter,
                                    innerFixupParentParameter,
                                    inverseNavigation)),
                            innerFixupCollectionElementParameter,
                            innerFixupParentParameter);

                        expressions.Add(
                            Call(
                                InverseCollectionFixupMethod.MakeGenericMethod(
                                    inverseNavigation.DeclaringEntityType.ClrType,
                                    inverseNavigation.TargetEntityType.ClrType),
                                shaperCollectionParameter,
                                shaperEntityParameter,
                                elementFixup));
                    }

                    var fixup = Lambda(
                        Block(typeof(void), expressions),
                        shaperEntityParameter,
                        shaperCollectionParameter);

                    innerFixupMap[navigationJsonPropertyName] = fixup;

                    var trackedFixup = Lambda(
                        Block(typeof(void), expressionsForTracking),
                        shaperEntityParameter,
                        shaperCollectionParameter);

                    innerFixupMap[navigationJsonPropertyName] = fixup;
                    trackingInnerFixupMap[navigationJsonPropertyName] = trackedFixup;
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

            var rewrittenEntityShaperMaterializer = new JsonEntityMaterializerRewriter(
                entityType,
                _queryStateManager,
                jsonReaderDataShaperLambdaParameter,
                innerShapersMap,
                innerFixupMap,
                trackingInnerFixupMap,
                _queryLogger,
                _parentVisitor.Dependencies.LiftableConstantFactory).Rewrite(entityShaperMaterializer);

            var entityShaperMaterializerVariable = Variable(
                entityShaperMaterializer.Type,
                "entityShaperMaterializer");

            shaperBlockVariables.Add(entityShaperMaterializerVariable);
            shaperBlockExpressions.Add(Assign(entityShaperMaterializerVariable, rewrittenEntityShaperMaterializer));

            var shaperBlock = Block(
                shaperBlockVariables,
                shaperBlockExpressions);

            var shaperLambda = Lambda(
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
                    ? Convert(parentEntityExpression, navigation.DeclaringEntityType.ClrType)
                    : parentEntityExpression;

                if (navigation.IsCollection)
                {
                    var includeJsonEntityCollectionMethodCall =
                        Call(
                            IncludeJsonEntityCollectionMethodInfo.MakeGenericMethod(
                                navigation.DeclaringEntityType.ClrType,
                                navigation.TargetEntityType.ClrType),
                            QueryCompilationContext.QueryContextParameter,
                            keyValuesParameter,
                            jsonReaderDataParameter,
                            includingEntityExpression,
                            shaperLambda,
                            GetOrCreateCollectionObjectLambda(
                                navigation.DeclaringEntityType.ClrType,
                                navigation),
                            fixup,
                            Constant(_isTracking));

                    return navigation.DeclaringEntityType.ClrType.IsAssignableFrom(parentEntityExpression.Type)
                        ? includeJsonEntityCollectionMethodCall
                        : IfThen(
                            TypeIs(
                                parentEntityExpression,
                                navigation.DeclaringEntityType.ClrType),
                            includeJsonEntityCollectionMethodCall);
                }

                var includeJsonEntityReferenceMethodCall =
                    Call(
                        IncludeJsonEntityReferenceMethodInfo.MakeGenericMethod(
                            navigation.DeclaringEntityType.ClrType,
                            navigation.TargetEntityType.ClrType),
                        QueryCompilationContext.QueryContextParameter,
                        keyValuesParameter,
                        jsonReaderDataParameter,
                        includingEntityExpression,
                        shaperLambda,
                        fixup,
                        Constant(_isTracking));

                return navigation.DeclaringEntityType.ClrType.IsAssignableFrom(parentEntityExpression.Type)
                    ? includeJsonEntityReferenceMethodCall
                    : IfThen(
                        TypeIs(
                            parentEntityExpression,
                            navigation.DeclaringEntityType.ClrType),
                        includeJsonEntityReferenceMethodCall);
            }

            if (navigation is { IsCollection: true })
            {
                var collectionClrType = navigation.GetMemberInfo(forMaterialization: true, forSet: true).GetMemberType();
                var materializeJsonEntityCollectionMethodCall =
                    Call(
                        MaterializeJsonEntityCollectionMethodInfo.MakeGenericMethod(
                            navigation.TargetEntityType.ClrType,
                            collectionClrType),
                        QueryCompilationContext.QueryContextParameter,
                        keyValuesParameter,
                        jsonReaderDataParameter,
                        _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                            navigation,
                            LiftableConstantExpressionHelpers.BuildNavigationAccessLambda(navigation),
                            navigation.Name + "Navigation",
                            typeof(INavigation)),
                        shaperLambda);

                return materializeJsonEntityCollectionMethodCall;
            }

            var materializedRootJsonEntity = Call(
                MaterializeJsonEntityMethodInfo.MakeGenericMethod(entityType.ClrType),
                QueryCompilationContext.QueryContextParameter,
                keyValuesParameter,
                jsonReaderDataParameter,
                Constant(nullable),
                shaperLambda);

            return materializedRootJsonEntity;
        }

        private sealed class JsonEntityMaterializerRewriter(
            IEntityType entityType,
            bool queryStateManager,
            ParameterExpression jsonReaderDataParameter,
            IDictionary<string, Expression> innerShapersMap,
            IDictionary<string, LambdaExpression> innerFixupMap,
            IDictionary<string, LambdaExpression> trackingInnerFixupMap,
            IDiagnosticsLogger<DbLoggerCategory.Query> queryLogger,
            ILiftableConstantFactory liftableConstantFactory)
            : ExpressionVisitor
        {
            private static readonly PropertyInfo JsonEncodedTextEncodedUtf8BytesProperty
                = typeof(JsonEncodedText).GetProperty(nameof(JsonEncodedText.EncodedUtf8Bytes))!;

            private static readonly MethodInfo JsonEncodedTextEncodeMethod
                = typeof(JsonEncodedText).GetMethod(nameof(JsonEncodedText.Encode), [typeof(string), typeof(JavaScriptEncoder)])!;

            // keep track which variable corresponds to which navigation - we need that info for fixup
            // which happens at the end (after we read everything to guarantee that we can instantiate the entity
            private readonly Dictionary<string, ParameterExpression> _navigationVariableMap = new();

            public BlockExpression Rewrite(BlockExpression jsonEntityShaperMaterializer)
                => (BlockExpression)VisitBlock(jsonEntityShaperMaterializer);

            protected override Expression VisitSwitch(SwitchExpression switchExpression)
            {
                if (switchExpression.SwitchValue.Type == typeof(IEntityType)
                    && switchExpression is
                    {
                        Cases:
                        [
                            {
                                Body: BlockExpression { Expressions.Count: > 0 } body,
                                TestValues: [Expression onlyValueExpression]
                            }
                        ]
                    }
                    && onlyValueExpression.GetConstantValue<object>() == entityType)
                {
                    var valueBufferTryReadValueMethodsToProcess =
                        new ValueBufferTryReadValueMethodsFinder(entityType).FindValueBufferTryReadValueMethods(body);

                    BlockExpression jsonEntityTypeInitializerBlock;
                    //sometimes we have shadow snapshot and sometimes not, but type initializer always comes last
                    switch (body.Expressions[^1])
                    {
                        case UnaryExpression { Operand: BlockExpression innerBlock } jsonEntityTypeInitializerUnary
                            when jsonEntityTypeInitializerUnary.NodeType is ExpressionType.Convert or ExpressionType.ConvertChecked:
                        {
                            // in case of proxies, the entity initializer block is wrapped around Convert node
                            // that converts from the proxy type to the actual entity type.
                            // We normalize that into a block by pushing the convert inside the inner block. Rather than:
                            //
                            // return (MyEntity)
                            // {
                            //     ProxyEntity instance;
                            //     (...)
                            //     return instance;
                            // }
                            //
                            // we produce:
                            // return
                            // {
                            //     ProxyEntity instance;
                            //     MyEntity actualInstance;
                            //     (...)
                            //     actualInstance = (MyEntity)instance;
                            //     return actualInstance;
                            // }
                            var newVariables = innerBlock.Variables.ToList();
                            var proxyConversionVariable = Variable(jsonEntityTypeInitializerUnary.Type);
                            newVariables.Add(proxyConversionVariable);
                            var newExpressions = innerBlock.Expressions.ToList()[..^1];
                            newExpressions.Add(
                                Assign(proxyConversionVariable, jsonEntityTypeInitializerUnary.Update(innerBlock.Expressions[^1])));
                            newExpressions.Add(proxyConversionVariable);
                            jsonEntityTypeInitializerBlock = Block(newVariables, newExpressions);
                            break;
                        }

                        case BlockExpression b:
                            jsonEntityTypeInitializerBlock = b;
                            break;
                        // case where we don't use block but rather return construction directly, as in:
                        // return new MyEntity(...)
                        //
                        // rather than:
                        // return
                        // {
                        //    MyEntity instance;
                        //    instance = new MyEntity(...)
                        //    (...)
                        // }
                        // we normalize this into block, since we are going to be adding extra statements (i.e. loop extracting JSON
                        // property values) there anyway
                        case NewExpression jsonEntityTypeInitializerCtor:
                            var newInstanceVariable = Variable(jsonEntityTypeInitializerCtor.Type, "instance");
                            jsonEntityTypeInitializerBlock = Block(
                                new[] { newInstanceVariable },
                                Assign(newInstanceVariable, jsonEntityTypeInitializerCtor),
                                newInstanceVariable);
                            break;
                        default:
                            throw new UnreachableException();
                    }

                    var managerVariable = Variable(typeof(Utf8JsonReaderManager), "jsonReaderManager");
                    var tokenTypeVariable = Variable(typeof(JsonTokenType), "tokenType");
                    var jsonEntityTypeVariable = (ParameterExpression)jsonEntityTypeInitializerBlock.Expressions[^1];

                    Debug.Assert(jsonEntityTypeVariable.Type == entityType.ClrType);

                    var finalBlockVariables = new List<ParameterExpression>
                    {
                        managerVariable, tokenTypeVariable,
                    };

                    finalBlockVariables.AddRange(jsonEntityTypeInitializerBlock.Variables);

                    var finalBlockExpressions = new List<Expression>
                    {
                        // jsonReaderManager = new Utf8JsonReaderManager(jsonReaderData))
                        Assign(
                            managerVariable,
                            New(
                                JsonReaderManagerConstructor,
                                jsonReaderDataParameter,
                                liftableConstantFactory.CreateLiftableConstant(
                                    queryLogger,
                                    static c => c.Dependencies.QueryLogger,
                                    "queryLogger",
                                    typeof(IDiagnosticsLogger<DbLoggerCategory.Query>)))),
                        // tokenType = jsonReaderManager.CurrentReader.TokenType
                        Assign(
                            tokenTypeVariable,
                            Property(
                                Field(
                                    managerVariable,
                                    Utf8JsonReaderManagerCurrentReaderField),
                                Utf8JsonReaderTokenTypeProperty)),
                    };

                    var (loop, propertyAssignmentMap) = GenerateJsonPropertyReadLoop(
                        managerVariable,
                        tokenTypeVariable,
                        finalBlockVariables,
                        valueBufferTryReadValueMethodsToProcess);

                    finalBlockExpressions.Add(loop);

                    var finalCaptureState = Call(managerVariable, Utf8JsonReaderManagerCaptureStateMethod);
                    finalBlockExpressions.Add(finalCaptureState);

                    // we have the loop, now we can add code that generate the entity instance
                    // will have to replace ValueBufferTryReadValue method calls with the parameters that store the value
                    // we can't use simple ExpressionReplacingVisitor, because there could be multiple instances of MethodCallExpression for given property
                    // using dedicated mini-visitor that looks for MCEs with a given shape and compare the IProperty inside
                    // order is:
                    // - shadow snapshot (if there was one)
                    // - entity construction / property assignments
                    // - navigation fixups
                    // - entity instance variable that is returned as end result
                    var propertyAssignmentReplacer = new ValueBufferTryReadValueMethodsReplacer(
                        jsonEntityTypeVariable, propertyAssignmentMap);

                    if (body.Expressions[0] is BinaryExpression
                        {
                            NodeType: ExpressionType.Assign,
                            Right: UnaryExpression
                            {
                                NodeType: ExpressionType.Convert,
                                Operand: NewExpression
                            }
                        } shadowSnapshotAssignment
#pragma warning disable EF1001 // Internal EF Core API usage.
                        && shadowSnapshotAssignment.Type == typeof(ISnapshot))
#pragma warning restore EF1001 // Internal EF Core API usage.
                    {
                        finalBlockExpressions.Add(propertyAssignmentReplacer.Visit(shadowSnapshotAssignment));
                    }

                    foreach (var jsonEntityTypeInitializerBlockExpression in jsonEntityTypeInitializerBlock.Expressions.ToArray()[..^1])
                    {
                        finalBlockExpressions.Add(propertyAssignmentReplacer.Visit(jsonEntityTypeInitializerBlockExpression));
                    }

                    // Fixup is only needed for non-tracking queries, in case of tracking (or NoTrackingWithIdentityResolution) - ChangeTracker does the job
                    // or for empty/null collections of a tracking queries.
                    if (queryStateManager)
                    {
                        ProcessFixup(trackingInnerFixupMap);
                    }
                    else
                    {
                        ProcessFixup(innerFixupMap);
                    }

                    finalBlockExpressions.Add(jsonEntityTypeVariable);

                    return Block(
                        finalBlockVariables,
                        finalBlockExpressions);

                    void ProcessFixup(IDictionary<string, LambdaExpression> fixupMap)
                    {
                        foreach (var fixup in fixupMap)
                        {
                            var navigationEntityParameter = _navigationVariableMap[fixup.Key];

                            // we need to add null checks before we run fixup logic. For regular entities, whose fixup is done as part of the "Materialize*" method
                            // the checks are done there (same will be done for the "optimized" scenario, where we populate properties directly rather than store in variables)
                            // but in this case fixups are standalone, so the null safety must be added by us directly
                            finalBlockExpressions.Add(
                                IfThen(
                                    NotEqual(
                                        jsonEntityTypeVariable,
                                        Constant(null, jsonEntityTypeVariable.Type)),
                                    Invoke(
                                        fixup.Value,
                                        jsonEntityTypeVariable,
                                        _navigationVariableMap[fixup.Key])));
                        }
                    }
                }

                return base.VisitSwitch(switchExpression);

                // builds a loop that extracts values of JSON properties and assigns them into variables
                // also injects entity shapers (generated earlier) for child navigations
                // returns the loop expression and mappings for properties (so we know which calls to replace with variables)
                (LoopExpression, Dictionary<IProperty, ParameterExpression>) GenerateJsonPropertyReadLoop(
                    ParameterExpression managerVariable,
                    ParameterExpression tokenTypeVariable,
                    List<ParameterExpression> finalBlockVariables,
                    List<MethodCallExpression> valueBufferTryReadValueMethodsToProcess)
                {
                    var breakLabel = Label("done");
                    var testExpressions = new List<Expression>();
                    var readExpressions = new List<Expression>();
                    var propertyAssignmentMap = new Dictionary<IProperty, ParameterExpression>();

                    foreach (var valueBufferTryReadValueMethodToProcess in valueBufferTryReadValueMethodsToProcess)
                    {
                        var property = valueBufferTryReadValueMethodToProcess.Arguments[2].GetConstantValue<IProperty>();
                        var jsonPropertyName = property.GetJsonPropertyName()!;
                        testExpressions.Add(
                            Call(
                                Field(
                                    managerVariable,
                                    Utf8JsonReaderManagerCurrentReaderField),
                                Utf8JsonReaderValueTextEqualsMethod,
                                Convert(
                                    Call(
                                        ByteArrayAsSpanMethod,
                                        Call(
                                            Property(null, EncodingUtf8Property),
                                            Utf8GetBytesMethod,
                                            Constant(jsonPropertyName))),
                                    typeof(ReadOnlySpan<>).MakeGenericType(typeof(byte)))));

                        var propertyVariable = Variable(valueBufferTryReadValueMethodToProcess.Type);

                        finalBlockVariables.Add(propertyVariable);

                        var moveNext = Call(
                            managerVariable,
                            Utf8JsonReaderManagerMoveNextMethod);

                        var assignment = Assign(
                            propertyVariable,
                            valueBufferTryReadValueMethodToProcess);

                        readExpressions.Add(
                            Block(
                                moveNext,
                                assignment,
                                Empty()));

                        propertyAssignmentMap[property] = propertyVariable;
                    }

                    foreach (var innerShaperMapElement in innerShapersMap)
                    {
                        var innerShaperMapElementKey = innerShaperMapElement.Key;
                        testExpressions.Add(
                            Call(
                                Field(
                                    managerVariable,
                                    Utf8JsonReaderManagerCurrentReaderField),
                                Utf8JsonReaderValueTextEqualsMethod,
                                Convert(
                                    Call(
                                        ByteArrayAsSpanMethod,
                                        Call(
                                            Property(null, EncodingUtf8Property),
                                            Utf8GetBytesMethod,
                                            Constant(innerShaperMapElementKey))),
                                    typeof(ReadOnlySpan<>).MakeGenericType(typeof(byte)))));

                        var propertyVariable = Variable(innerShaperMapElement.Value.Type);
                        finalBlockVariables.Add(propertyVariable);

                        _navigationVariableMap[innerShaperMapElement.Key] = propertyVariable;

                        var moveNext = Call(managerVariable, Utf8JsonReaderManagerMoveNextMethod);
                        var captureState = Call(managerVariable, Utf8JsonReaderManagerCaptureStateMethod);
                        var assignment = Assign(propertyVariable, innerShaperMapElement.Value);
                        var managerRecreation = Assign(
                            managerVariable,
                            New(
                                JsonReaderManagerConstructor,
                                jsonReaderDataParameter,
                                liftableConstantFactory.CreateLiftableConstant(
                                    queryLogger,
                                    static c => c.Dependencies.QueryLogger,
                                    "queryLogger",
                                    typeof(IDiagnosticsLogger<DbLoggerCategory.Query>))));

                        readExpressions.Add(
                            Block(
                                moveNext,
                                captureState,
                                assignment,
                                managerRecreation,
                                Empty()));
                    }

                    var switchCases = new List<SwitchCase>();
                    var testsCount = testExpressions.Count;

                    // generate PropertyName switch-case code
                    if (testsCount > 0)
                    {
                        var testExpression = IfThen(
                            testExpressions[testsCount - 1],
                            readExpressions[testsCount - 1]);

                        for (var i = testsCount - 2; i >= 0; i--)
                        {
                            testExpression = IfThenElse(
                                testExpressions[i],
                                readExpressions[i],
                                testExpression);
                        }

                        switchCases.Add(
                            SwitchCase(
                                testExpression,
                                Constant(JsonTokenType.PropertyName)));
                    }

                    switchCases.Add(
                        SwitchCase(
                            Break(breakLabel),
                            Constant(JsonTokenType.EndObject)));

                    var loopBody = Block(
                        Assign(tokenTypeVariable, Call(managerVariable, Utf8JsonReaderManagerMoveNextMethod)),
                        Switch(
                            tokenTypeVariable,
                            Block(
                                Call(managerVariable, Utf8JsonReaderManagerSkipMethod),
                                Default(typeof(void))),
                            switchCases.ToArray()));

                    return (Loop(loopBody, breakLabel), propertyAssignmentMap);
                }
            }

            protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
            {
                var visited = base.VisitConditional(conditionalExpression);

                // this code compensates for differences between regular entities and JSON entities for tracking queries
                // for regular entities we preserve all the includes, so shaper for each entity is visited regardless
                // because of that, the original entity materializer code short-circuits if we find entity in change tracker
                //
                // for JSON entities that is incorrect, because all includes are part of the parent's shaper
                // so if we short circuit the parent, we never process the children
                // this is a problem when someone modifies child entity in the database directly - we would never pick up those changes
                // if we are tracking the parent
                // the code here re-arranges the existing materializer so that even if we find parent in the change tracker
                // we still process all the child navigations, it's just that we use the parent instance from change tracker, rather than create new one
#pragma warning disable EF1001 // Internal EF Core API usage.
                if (queryStateManager
                    && visited is ConditionalExpression
                    {
                        Test: BinaryExpression
                        {
                            NodeType: ExpressionType.NotEqual,
                            Left: ParameterExpression,
                            Right: DefaultExpression rightDefault
                        } testBinaryExpression,
                        IfTrue: BlockExpression ifTrueBlock,
                        IfFalse: BlockExpression ifFalseBlock
                    }
                    && rightDefault.Type == typeof(InternalEntityEntry))
                {
                    var entityAlreadyTrackedVariable = Variable(typeof(bool), "entityAlreadyTracked");

                    var resultBlockVariables = new List<ParameterExpression> { entityAlreadyTrackedVariable };
                    var resultBlockExpressions = new List<Expression>
                    {
                        Assign(entityAlreadyTrackedVariable, Constant(false)),

                        // shadowSnapshot = Snapshot.Empty;
                        ifFalseBlock.Expressions[0],

                        // entityType = EntityType;
                        ifFalseBlock.Expressions[1],
                        IfThen(
                            testBinaryExpression,
                            Block(
                                ifTrueBlock.Variables,
                                ifTrueBlock.Expressions.Concat(
                                    new Expression[] { Assign(entityAlreadyTrackedVariable, Constant(true)), Default(typeof(void)) })))
                    };

                    resultBlockVariables.AddRange(ifFalseBlock.Variables.ToList());

                    var instanceAssignment = ifFalseBlock.Expressions.OfType<BinaryExpression>().Single(
                        e => e is { NodeType: ExpressionType.Assign, Left: ParameterExpression instance, Right: BlockExpression }
                            && instance.Type == entityType.ClrType);
                    var instanceAssignmentBody = (BlockExpression)instanceAssignment.Right;

                    var newInstanceAssignmentVariables = instanceAssignmentBody.Variables.ToList();
                    var newInstanceAssignmentExpressions = new List<Expression>();

                    // we only need to generate shadowSnapshot if the entity isn't already tracked
                    // shadow snapshot can be generated early in the block (default)
                    // or after we read all the values from JSON (case when the entity has some shadow properties)
                    // so we loop through the existing expressions and add the condition to snapshot assignment when we find it
                    // expressions processed here:
                    // shadowSnapshot = new Snapshot(...)
                    // jsonManagerPrm = new Utf8JsonReaderManager(jsonReaderDataPrm);
                    // tokenType = jsonManagerPrm.TokenType;
                    // property_reading_loop(...)
                    // jsonManagerPrm.CaptureState();
                    for (var i = 0; i < 5; i++)
                    {
                        newInstanceAssignmentExpressions.Add(
                            instanceAssignmentBody.Expressions[i].Type == typeof(ISnapshot)
                                ? IfThen(
                                    Not(entityAlreadyTrackedVariable),
                                    instanceAssignmentBody.Expressions[i])
                                : instanceAssignmentBody.Expressions[i]);
                    }

                    // from now on we have entity construction and property assignments
                    // then navigation fixup and then returning the final product
                    // entity construction could vary in length (e.g. when we have custom materializer)
                    // but we know how many navigation fixups there are and that instance is returned as last statement
                    var innerInstanceVariable = instanceAssignmentBody.Expressions[^1];

                    var createAndPopulateInstanceIfTrueBlock = Block(
                        Assign(innerInstanceVariable, instanceAssignment.Left),
                        Default(typeof(void)));

                    // all expressions except first 5 (that we already added)
                    // final variable being returned is also omitted but we generate Express.Default(typeof(void)) instead
                    var createAndPopulateInstanceIfFalseBlockExpressionsCount = instanceAssignmentBody.Expressions.Count - 5;
                    var createAndPopulateInstanceIfFalseBlockExpressions =
                        new Expression[createAndPopulateInstanceIfFalseBlockExpressionsCount];

                    Array.Copy(
                        instanceAssignmentBody.Expressions.ToArray()[5..^1],
                        createAndPopulateInstanceIfFalseBlockExpressions,
                        createAndPopulateInstanceIfFalseBlockExpressionsCount - 1);

                    createAndPopulateInstanceIfFalseBlockExpressions[^1] = Default(typeof(void));

                    var createAndPopulateInstanceExpression = IfThenElse(
                        entityAlreadyTrackedVariable,
                        createAndPopulateInstanceIfTrueBlock,
                        Block(createAndPopulateInstanceIfFalseBlockExpressions));

                    newInstanceAssignmentExpressions.Add(createAndPopulateInstanceExpression);
                    newInstanceAssignmentExpressions.Add(innerInstanceVariable);

                    var newInstanceAssignmentBlock = Block(newInstanceAssignmentVariables, newInstanceAssignmentExpressions);

                    resultBlockExpressions.Add(
                        Assign(instanceAssignment.Left, newInstanceAssignmentBlock));

                    var startTrackingAssignment = ifFalseBlock.Expressions
                        .OfType<BinaryExpression>()
                        .Single(
                            e => e is { NodeType: ExpressionType.Assign, Left: ParameterExpression instance, Right: ConditionalExpression }
                                && instance.Type == typeof(InternalEntityEntry));

                    var startTrackingExpression =
                        IfThen(
                            Not(
                                OrElse(
                                    entityAlreadyTrackedVariable,
                                    ((ConditionalExpression)startTrackingAssignment.Right).Test)),
                            Block(
                                ((ConditionalExpression)startTrackingAssignment.Right).IfFalse,
                                Default(typeof(void))));

                    resultBlockExpressions.Add(startTrackingExpression);
                    resultBlockExpressions.Add(Default(typeof(void)));
                    var resultBlock = Block(resultBlockVariables, resultBlockExpressions);

                    return resultBlock;
                }
#pragma warning restore EF1001 // Internal EF Core API usage.

                return visited;
            }

            private sealed class ValueBufferTryReadValueMethodsFinder : ExpressionVisitor
            {
                private readonly List<IProperty> _nonKeyProperties;
                private readonly List<MethodCallExpression> _valueBufferTryReadValueMethods = [];

                public ValueBufferTryReadValueMethodsFinder(IEntityType entityType)
                    => _nonKeyProperties = entityType.GetProperties().Where(p => !p.IsPrimaryKey()).ToList();

                public List<MethodCallExpression> FindValueBufferTryReadValueMethods(Expression expression)
                {
                    _valueBufferTryReadValueMethods.Clear();

                    Visit(expression);

                    return _valueBufferTryReadValueMethods;
                }

                protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
                {
                    if (methodCallExpression.Method.IsGenericMethod
                        && methodCallExpression.Method.GetGenericMethodDefinition()
                        == Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod
                        && methodCallExpression.Arguments[2].GetConstantValue<object>() is IProperty property
                        && _nonKeyProperties.Contains(property))
                    {
                        _valueBufferTryReadValueMethods.Add(methodCallExpression);
                        _nonKeyProperties.Remove(property);

                        return methodCallExpression;
                    }

                    return base.VisitMethodCall(methodCallExpression);
                }
            }

            private sealed class ValueBufferTryReadValueMethodsReplacer(
                Expression instance,
                Dictionary<IProperty, ParameterExpression> propertyAssignmentMap)
                : ExpressionVisitor
            {
                protected override Expression VisitBinary(BinaryExpression node)
                {
                    if (node.Right is MethodCallExpression methodCallExpression
                        && IsPropertyAssignment(methodCallExpression, out var property, out var parameter))
                    {
                        if (property!.IsPrimitiveCollection
                            && !property.ClrType.IsArray)
                        {
#pragma warning disable EF1001 // Internal EF Core API usage.
                            var genericMethod = EntityMaterializerSource.PopulateListMethod.MakeGenericMethod(
                                property.ClrType.TryGetElementType(typeof(IEnumerable<>))!);
#pragma warning restore EF1001 // Internal EF Core API usage.
                            var currentVariable = Variable(parameter!.Type);
                            var convertedVariable = genericMethod.GetParameters()[1].ParameterType.IsAssignableFrom(currentVariable.Type)
                                ? (Expression)currentVariable
                                : Convert(currentVariable, genericMethod.GetParameters()[1].ParameterType);
                            return Block(
                                new[] { currentVariable },
                                MakeMemberAccess(instance, property.GetMemberInfo(forMaterialization: true, forSet: false))
                                    .Assign(currentVariable),
                                IfThenElse(
                                    OrElse(
                                        ReferenceEqual(currentVariable, Constant(null)),
                                        ReferenceEqual(parameter, Constant(null))),
                                    node is { NodeType: ExpressionType.Assign, Left: MemberExpression leftMemberExpression }
                                        ? leftMemberExpression.Assign(parameter)
                                        : MakeBinary(node.NodeType, node.Left, parameter),
                                    Call(
                                        genericMethod,
                                        parameter,
                                        convertedVariable)
                                ));
                        }

                        var visitedLeft = Visit(node.Left);
                        return node.NodeType == ExpressionType.Assign
                            && visitedLeft is MemberExpression memberExpression
                                ? memberExpression.Assign(parameter!)
                                : MakeBinary(node.NodeType, visitedLeft, parameter!);
                    }

                    return base.VisitBinary(node);
                }

                protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
                    => IsPropertyAssignment(methodCallExpression, out _, out var parameter)
                        ? parameter!
                        : base.VisitMethodCall(methodCallExpression);

                private bool IsPropertyAssignment(
                    MethodCallExpression methodCallExpression,
                    out IProperty? property,
                    out Expression? parameter)
                {
                    if (methodCallExpression.Method.IsGenericMethod
                        && methodCallExpression.Method.GetGenericMethodDefinition()
                        == Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod
                        && methodCallExpression.Arguments[2].GetConstantValue<object>() is IProperty prop
                        && propertyAssignmentMap.TryGetValue(prop, out var param))
                    {
                        property = prop;
                        parameter = param;
                        return true;
                    }

                    property = null;
                    parameter = null;
                    return false;
                }
            }
        }

        private (ParameterExpression, ParameterExpression) JsonShapingPreProcess(
            JsonProjectionInfo jsonProjectionInfo,
            IEntityType entityType,
            bool isCollection)
        {
            var jsonColumnName = entityType.GetContainerColumnName()!;
            var jsonColumnTypeMapping = (entityType.GetViewOrTableMappings().SingleOrDefault()?.Table
                    ?? entityType.GetDefaultMappings().Single().Table)
                .FindColumn(jsonColumnName)!.StoreTypeMapping;

            var jsonStreamVariable = Variable(typeof(Stream), "jsonStream");
            var jsonReaderDataVariable = Variable(typeof(JsonReaderData), "jsonReader");
            var jsonReaderManagerVariable = Variable(typeof(Utf8JsonReaderManager), "jsonReaderManager");

            var jsonStreamAssignment = Assign(
                jsonStreamVariable,
                CreateGetValueExpression(
                    _dataReaderParameter,
                    jsonProjectionInfo.JsonColumnIndex,
                    nullable: true,
                    jsonColumnTypeMapping,
                    typeof(MemoryStream),
                    property: null));

            var jsonReaderDataAssignment = Assign(
                jsonReaderDataVariable,
                Condition(
                    Equal(jsonStreamVariable, Default(typeof(MemoryStream))),
                    Default(typeof(JsonReaderData)),
                    New(JsonReaderDataConstructor, jsonStreamVariable)));

            // if (jsonReaderData) != default
            // {
            //     var jsonReaderManager = new Utf8JsonReaderManager(jsonReaderData);
            //     jsonReaderManager.MoveNext();
            //     jsonReaderManager.CaptureState();
            // }
            var jsonReaderManagerBlock =
                IfThen(
                    NotEqual(
                        jsonReaderDataVariable,
                        Default(typeof(JsonReaderData))),
                    Block(
                        Assign(
                            jsonReaderManagerVariable,
                            New(
                                JsonReaderManagerConstructor,
                                jsonReaderDataVariable,
                                _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                    _queryLogger,
                                    static c => c.Dependencies.QueryLogger,
                                    "queryLogger",
                                    typeof(IDiagnosticsLogger<DbLoggerCategory.Query>)))),
                        Call(jsonReaderManagerVariable, Utf8JsonReaderManagerMoveNextMethod),
                        Call(jsonReaderManagerVariable, Utf8JsonReaderManagerCaptureStateMethod)));

            _variables.Add(jsonStreamVariable);
            _variables.Add(jsonReaderDataVariable);
            _variables.Add(jsonReaderManagerVariable);
            _expressions.Add(jsonStreamAssignment);
            _expressions.Add(jsonReaderDataAssignment);
            _expressions.Add(jsonReaderManagerBlock);

            // we should have keyAccessInfo for every PK property of the entity, unless we are generating shaper for the collection
            // in that case the final key property will be synthesized in the shaper code
            var expectedKeyValuesCount = entityType.FindPrimaryKey()!.Properties.Count - (isCollection ? 1 : 0);
            var keyValues = new Expression[expectedKeyValuesCount];

            if (keyValues.Length != expectedKeyValuesCount && !_isTracking)
            {
                throw new InvalidOperationException(RelationalStrings.JsonEntityMissingKeyInformation(entityType.ShortName()));
            }

            //var keyValues = new Expression[jsonProjectionInfo.KeyAccessInfo.Count];
            for (var i = 0; i < jsonProjectionInfo.KeyAccessInfo.Count; i++)
            {
                var keyAccessInfo = jsonProjectionInfo.KeyAccessInfo[i];
                switch (keyAccessInfo)
                {
                    case { ConstantKeyValue: int constant }:
                        // if key access was a constant (and we have the actual value) add it directly to key values array
                        // adding 1 to the value as we start keys from 1 and the array starts at 0
                        keyValues[i] = Convert(
                            Constant(constant + 1),
                            typeof(object));
                        break;

                    case { KeyProperty: IProperty keyProperty }:
                        // if key value has IProperty, it must be a PK of the owner
                        var projection = _selectExpression.Projection[keyAccessInfo.KeyProjectionIndex!.Value];
                        keyValues[i] = Convert(
                            CreateGetValueExpression(
                                _dataReaderParameter,
                                keyAccessInfo.KeyProjectionIndex!.Value,
                                IsNullableProjection(projection),
                                projection.Expression.TypeMapping!,
                                keyProperty.ClrType,
                                keyProperty),
                            typeof(object));
                        break;

                    default:
                        // otherwise it must be non-constant array access and we stored its projection index
                        // extract the value from the projection (or the cache if we used it before)
                        var collectionElementAccessParameter = ExtractAndCacheNonConstantJsonArrayElementAccessValue(
                            keyAccessInfo.KeyProjectionIndex!.Value);
                        keyValues[i] = Convert(
                            Add(collectionElementAccessParameter, Constant(1, typeof(int?))),
                            typeof(object));
                        break;
                }
            }

            // fill missing keys (with arbitrary values) - this *should* only be missing synthesized keys (CHECK!)
            // and those are only used to build identity for purpose of identity resolution in Tracking queries
            // missing keys can happen when we do advanced querying of JSON entities (e.g. filters, paging)
            for (var i = jsonProjectionInfo.KeyAccessInfo.Count; i < expectedKeyValuesCount; i++)
            {
                keyValues[i] = Constant(1, typeof(object));
            }

            // create key values for initial entity
            var currentKeyValuesVariable = Variable(typeof(object[]), "currentKeyValues");
            var keyValuesAssignment = Assign(
                currentKeyValuesVariable,
                NewArrayInit(typeof(object), keyValues));

            _variables.Add(currentKeyValuesVariable);
            _expressions.Add(keyValuesAssignment);

            return (jsonReaderDataVariable, currentKeyValuesVariable);

            ParameterExpression ExtractAndCacheNonConstantJsonArrayElementAccessValue(int index)
            {
                if (!_jsonArrayNonConstantElementAccessMap.TryGetValue(index, out var arrayElementAccessParameter))
                {
                    arrayElementAccessParameter = Parameter(typeof(int?));
                    var projection = _selectExpression.Projection[index];

                    var arrayElementAccessValue = CreateGetValueExpression(
                        _dataReaderParameter,
                        index,
                        IsNullableProjection(projection),
                        projection.Expression.TypeMapping!,
                        type: typeof(int?),
                        property: null);

                    var arrayElementAccessAssignment = Assign(
                        arrayElementAccessParameter,
                        arrayElementAccessValue);

                    _variables.Add(arrayElementAccessParameter);
                    _expressions.Add(arrayElementAccessAssignment);

                    _jsonArrayNonConstantElementAccessMap.Add(index, arrayElementAccessParameter);
                }

                return arrayElementAccessParameter;
            }
        }

        private sealed class QueryableJsonEntityMaterializerRewriter(List<IProperty> mappedProperties) : ExpressionVisitor
        {
            public BlockExpression Rewrite(BlockExpression jsonEntityShaperMaterializer)
                => (BlockExpression)VisitBlock(jsonEntityShaperMaterializer);

            protected override Expression VisitBinary(BinaryExpression binaryExpression)
            {
                // here we try to pattern match part of the shaper code that checks if key values are null
                // if they are all non-null then we generate the entity
                // problem for JSON entities is that some of the keys are synthesized and should be omitted
                // if the key is one of the mapped ones, we leave the expression as is, otherwise replace with Constant(true)
                // i.e. removing it
                if (binaryExpression is
                    {
                        NodeType: ExpressionType.NotEqual,
                        Left: MethodCallExpression
                        {
                            Method: { IsGenericMethod: true } method,
                            Arguments: [_, _, Expression leftExpression]
                        },
                        Right: Expression rightExpression
                    }
                    && leftExpression.TryGetNonNullConstantValue<IProperty>(out var property)
                    && rightExpression is ConstantExpression or LiftableConstantExpression
                    && rightExpression.GetConstantValue<object>() == null
                    && method.GetGenericMethodDefinition() == Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod)
                {
                    return mappedProperties.Contains(property)
                        ? binaryExpression
                        : Constant(true);
                }

                return base.VisitBinary(binaryExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression is
                    {
                        Method: { IsGenericMethod: true } method,
                        Arguments: [_, _, Expression argumentExpression]
                    }
                    && argumentExpression.TryGetNonNullConstantValue<IProperty>(out var property)
                    && method.GetGenericMethodDefinition() == Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod
                    && !mappedProperties.Contains(property))
                {
                    return Default(methodCallExpression.Type);
                }

                return base.VisitMethodCall(methodCallExpression);
            }
        }

        private LambdaExpression GenerateFixup(
            Type entityType,
            Type relatedEntityType,
            INavigationBase navigation,
            INavigationBase? inverseNavigation)
        {
            var entityParameter = Parameter(entityType);
            var relatedEntityParameter = Parameter(relatedEntityType);
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

            return Lambda(Block(typeof(void), expressions), entityParameter, relatedEntityParameter);
        }

        private static LambdaExpression GenerateReferenceFixupForJson(
            Type entityType,
            Type relatedEntityType,
            INavigationBase navigation,
            INavigationBase? inverseNavigation)
        {
            var entityParameter = Parameter(entityType);
            var relatedEntityParameter = Parameter(relatedEntityType);
            var expressions = new List<Expression>();

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

            return Lambda(Block(typeof(void), expressions), entityParameter, relatedEntityParameter);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public static void InverseCollectionFixup<TCollectionElement, TEntity>(
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

        private Expression GetOrCreateCollectionObjectLambda(
            Type entityType,
            INavigationBase navigation)
        {
            var prm = Parameter(entityType);

            return Lambda(
                Block(
                    typeof(void),
                    Call(
                        _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                            navigation.GetCollectionAccessor(),
                            LiftableConstantExpressionHelpers.BuildClrCollectionAccessorLambda(navigation),
                            navigation.Name + "NavigationCollectionAccessor",
                            typeof(IClrCollectionAccessor)),
                        CollectionAccessorGetOrCreateMethodInfo,
                        prm,
                        Constant(true))),
                prm);
        }

        private Expression AddToCollectionNavigation(
            ParameterExpression entity,
            ParameterExpression relatedEntity,
            INavigationBase navigation)
            => Call(
                _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                    navigation.GetCollectionAccessor(),
                    LiftableConstantExpressionHelpers.BuildClrCollectionAccessorLambda(navigation),
                    navigation.Name + "NavigationCollectionAccessor",
                    typeof(IClrCollectionAccessor)),
                CollectionAccessorAddMethodInfo,
                entity,
                relatedEntity,
                Constant(true));

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

            Expression indexExpression = Constant(index);
            if (_indexMapParameter != null)
            {
                indexExpression = ArrayIndex(_indexMapParameter, indexExpression);
            }

            Expression valueExpression
                = Call(
                    getMethod.DeclaringType != typeof(DbDataReader)
                        ? Convert(dbDataReader, getMethod.DeclaringType!)
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
                        bufferedReaderLambdaExpression = Convert(bufferedReaderLambdaExpression, bufferedColumnType);
                    }

                    _readerColumns[index] = ReaderColumn.Create(
                        bufferedColumnType,
                        nullable,
                        _indexMapParameter != null ? ((ColumnExpression)_selectExpression.Projection[index].Expression).Name : null,
                        property,
                        Lambda(
                            bufferedReaderLambdaExpression,
                            dbDataReader,
                            _indexMapParameter ?? Parameter(typeof(int[]), "indexMap")));
                }

                valueExpression = Call(
                    dbDataReader, RelationalTypeMapping.GetDataReaderMethod(bufferedColumnType), indexExpression);
                if (valueExpression.Type != columnType)
                {
                    valueExpression = Convert(valueExpression, columnType);
                }
            }

            valueExpression = typeMapping.CustomizeDataReaderExpression(valueExpression);

            var converter = typeMapping.Converter;

            var converterExpression = default(Expression);
            if (converter != null)
            {
                // if IProperty is available, we can reliably get the converter from the model and then incorporate FromProvider(Typed) delegate
                // into the expression. This way we have consistent behavior between precompiled and normal queries (same code path)
                // however, if IProperty is not available, we could try to get TypeMapping from TypeMappingSource based on ClrType, but that may
                // return incorrect mapping. So for that case we would prefer to incorporate the FromProvider lambda, like we used to do before AOT
                // and only resort to unreliable TypeMappingSource lookup, if the converter expression captures "forbidden" constant
                // see issue #33517 for more details
                // UPDATE: instead of guessing the type mapping in case where we don't have IProperty and converter uses non-literal constant,
                // we just revert to the pre-AOT behavior, i.e. we still use converter.ConvertFromProviderExpression
                // this will not work for precompiled query (which realistically was already broken for this scenario - type mapping we "guess"
                // is pretty much always wrong), but regular case (not pre-compiled) will continue to work. 
                if (property != null)
                {
                    var typeMappingExpression = Call(
                        Convert(
                            _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                                property,
                                LiftableConstantExpressionHelpers.BuildMemberAccessLambdaForProperty(property),
                                property.Name + "Property",
                                typeof(IPropertyBase)),
                            typeof(IReadOnlyProperty)),
                        PropertyGetTypeMappingMethod);

                    converterExpression = Property(typeMappingExpression, nameof(CoreTypeMapping.Converter));

                    var converterType = converter.GetType();
                    var typedConverterType = converterType.GetGenericTypeImplementations(typeof(ValueConverter<,>)).FirstOrDefault();
                    Expression invocationExpression;

                    // TODO: do we even need to do this check? can we ever have a custom ValueConverter that is not generic?
                    if (typedConverterType != null)
                    {
                        if (converterExpression.Type != converter.GetType())
                        {
                            converterExpression = Convert(converterExpression, converter.GetType());
                        }

                        if (valueExpression.Type != converter.ProviderClrType)
                        {
                            valueExpression = Convert(valueExpression, converter.ProviderClrType);
                        }

                        invocationExpression = Invoke(
                            Property(
                                converterExpression,
                                nameof(ValueConverter<object, object>.ConvertFromProviderTyped)),
                            valueExpression);
                    }
                    else
                    {
                        invocationExpression = Invoke(
                            Property(
                                converterExpression,
                                nameof(ValueConverter.ConvertFromProvider)),
                            Convert(valueExpression, typeof(object)));
                    }

                    valueExpression = invocationExpression;
                }
                else
                {
                    if (valueExpression.Type != converter.ProviderClrType)
                    {
                        valueExpression = Convert(valueExpression, converter.ProviderClrType);
                    }

                    valueExpression = ReplacingExpressionVisitor.Replace(
                        converter.ConvertFromProviderExpression.Parameters.Single(),
                        valueExpression,
                        converter.ConvertFromProviderExpression.Body);
                }
            }

            if (valueExpression.Type != type)
            {
                valueExpression = Convert(valueExpression, type);
            }

            if (nullable)
            {
                Expression replaceExpression;
                if (converter?.ConvertsNulls == true)
                {
                    // we potentially have to repeat logic from above here. We can check if we computed converterExpression before
                    // if so, it means there are liftable constants in the ConvertFromProvider expression
                    // we can also reuse converter expression, just switch argument to the Invoke for default(provier type) or object
                    if (converterExpression != null)
                    {
                        var converterType = converter.GetType();
                        var typedConverterType = converterType.GetGenericTypeImplementations(typeof(ValueConverter<,>)).FirstOrDefault();
                        Expression invocationExpression;
                        if (typedConverterType != null)
                        {
                            if (converterExpression.Type != converter.GetType())
                            {
                                converterExpression = Convert(converterExpression, converter.GetType());
                            }

                            invocationExpression = Invoke(
                                Property(
                                    converterExpression,
                                    nameof(ValueConverter<object, object>.ConvertFromProviderTyped)),
                                Default(converter.ProviderClrType));
                        }
                        else
                        {
                            invocationExpression = Invoke(
                                Property(
                                    converterExpression,
                                    nameof(ValueConverter.ConvertFromProvider)),
                                Default(typeof(object)));
                        }

                        replaceExpression = invocationExpression;
                    }
                    else
                    {
                        replaceExpression = ReplacingExpressionVisitor.Replace(
                            converter.ConvertFromProviderExpression.Parameters.Single(),
                            Default(converter.ProviderClrType),
                            converter.ConvertFromProviderExpression.Body);
                    }

                    if (replaceExpression.Type != type)
                    {
                        replaceExpression = Convert(replaceExpression, type);
                    }
                }
                else
                {
                    replaceExpression = Default(valueExpression.Type);
                }

                valueExpression = Condition(
                    Call(dbDataReader, IsDbNullMethod, indexExpression),
                    replaceExpression,
                    valueExpression);
            }

            if (_detailedErrorsEnabled
                && !buffering)
            {
                var exceptionParameter = Parameter(typeof(Exception), name: "e");

                var catchBlock = Catch(
                    exceptionParameter,
                    Call(
                        ThrowReadValueExceptionMethod.MakeGenericMethod(valueExpression.Type),
                        exceptionParameter,
                        Call(dbDataReader, GetFieldValueMethod.MakeGenericMethod(typeof(object)), indexExpression),
                        Constant(valueExpression.Type.MakeNullable(nullable), typeof(Type)),
                        _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                            property,
                            LiftableConstantExpressionHelpers.BuildMemberAccessLambdaForProperty(property),
                            property + "Property",
                            typeof(IPropertyBase))));

                valueExpression = TryCatch(valueExpression, catchBlock);
            }

            return valueExpression;
        }

        private Expression CreateReadJsonPropertyValueExpression(
            ParameterExpression jsonReaderManagerParameter,
            IProperty property)
        {
            var jsonReaderWriter = property.GetJsonValueReaderWriter() ?? property.GetTypeMapping().JsonValueReaderWriter!;
            var prm = Parameter(typeof(MaterializerLiftableConstantContext), "c");
            var jsonReaderWriterExpression = _parentVisitor.Dependencies.LiftableConstantFactory.CreateLiftableConstant(
                property.GetJsonValueReaderWriter() ?? property.GetTypeMapping().JsonValueReaderWriter!,
                Lambda<Func<MaterializerLiftableConstantContext, object>>(
                    Coalesce(
                        Call(
                            LiftableConstantExpressionHelpers.BuildMemberAccessForProperty(property, prm),
                            PropertyGetJsonValueReaderWriterMethod),
                        Property(
                            Call(
                                LiftableConstantExpressionHelpers.BuildMemberAccessForProperty(property, prm),
                                PropertyGetTypeMappingMethod),
                            nameof(CoreTypeMapping.JsonValueReaderWriter))),
                    prm),
                property.Name + "PropertyName",
                jsonReaderWriter.GetType());

            var fromJsonMethod = jsonReaderWriterExpression.Type.GetMethod(
                nameof(JsonValueReaderWriter<object>.FromJsonTyped),
                [typeof(Utf8JsonReaderManager).MakeByRefType(), typeof(object)])!;

            Expression resultExpression = Convert(
                Call(jsonReaderWriterExpression, fromJsonMethod, jsonReaderManagerParameter, Default(typeof(object))),
                property.GetTypeMapping().ClrType);

            if (property.IsNullable)
            {
                // in case of null value we can't just use the JsonReader method, but rather check the current token type
                // if it's JsonTokenType.Null means value is null, only if it's not we are safe to read the value
                if (resultExpression.Type != property.ClrType)
                {
                    resultExpression = Convert(resultExpression, property.ClrType);
                }

                resultExpression = Condition(
                    Equal(
                        Property(
                            Field(
                                jsonReaderManagerParameter,
                                Utf8JsonReaderManagerCurrentReaderField),
                            Utf8JsonReaderTokenTypeProperty),
                        Constant(JsonTokenType.Null)),
                    Default(property.ClrType),
                    resultExpression);
            }

            if (_detailedErrorsEnabled)
            {
                var exceptionParameter = Parameter(typeof(Exception), name: "e");
                var catchBlock = Catch(
                    exceptionParameter,
                    Call(
                        ThrowExtractJsonPropertyExceptionMethod.MakeGenericMethod(resultExpression.Type),
                        exceptionParameter,
                        Constant(property, typeof(IProperty))));

                resultExpression = TryCatch(resultExpression, catchBlock);
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

            [return: NotNullIfNotNull(nameof(expression))]
            public override Expression? Visit(Expression? expression)
            {
                if (_containsCollection)
                {
                    return expression;
                }

                if (expression is RelationalCollectionShaperExpression or RelationalSplitCollectionShaperExpression)
                {
                    _containsCollection = true;

                    return expression;
                }

                return base.Visit(expression);
            }
        }

        private sealed class JsonCorrectOrderOfEntitiesForChangeTrackerValidator(SelectExpression selectExpression) : ExpressionVisitor
        {
            private bool _insideCollection;
            private bool _insideInclude;
            private SelectExpression _selectExpression = selectExpression;
            private readonly
                List<(IEntityType JsonEntityType, List<(IProperty? KeyProperty, int? ConstantKeyValue, int? KeyProjectionIndex)>
                    KeyAccessInfo)> _projectedKeyAccessInfos = [];

            private readonly List<IEntityType> _includedJsonEntityTypes = [];

            public void Validate(Expression expression)
            {
                // this visitor makes sure that we don't end up with data corruption in NoTrackingWithIdentityResolution mode
                // In order to avoid it, we need to make sure entities land in Change Tracker in a correct order
                // This is because until we have ordered collections, when we populate collection from Change Tracker, we do it in the same
                // order that the entities landed there. So if, say, 3rd element of the collection is read first, if subsequently the entire
                // collection is projected, that third element will appear at the start, and only after elements 1 and 2 will show up
                _insideCollection = false;
                _insideInclude = false;
                Visit(expression);

                // all projections that are contained in any of the included are safe - we process all the includes first
                // so the entries are guaranteed to land in Change Tracker in the right order
                // for all the remaining - entities deeper in the structure must appear after entities/collections that are "shallower"
                // i.e. entity.MyJsonCollection must appear before entity.MyJsonCollection[2]
                // we can verify that by comparing key access infos
                // we process from the end to the beginning, so we can safely remove elements as we iterate over the list
                if (_projectedKeyAccessInfos.Count > 0 && _includedJsonEntityTypes.Count > 0)
                {
                    for (var i = _projectedKeyAccessInfos.Count - 1; i >= 0; i--)
                    {
                        if (_includedJsonEntityTypes.Any(
                                t => t == _projectedKeyAccessInfos[i].JsonEntityType
                                    || _projectedKeyAccessInfos[i].JsonEntityType.IsInOwnershipPath(t)))
                        {
                            _projectedKeyAccessInfos.RemoveAt(i);
                        }
                    }
                }

                // if there is only one thing projected, we are good no matter what
                // if we project one thing only, the result will always land in correct order in ChangeTracker
                if (_projectedKeyAccessInfos.Count > 1)
                {
                    var i = 0;

                    do
                    {
                        var outerKeyAccessInfo = _projectedKeyAccessInfos[i].KeyAccessInfo;
                        var outerJsonEntityType = _projectedKeyAccessInfos[i].JsonEntityType;

                        // accessing collection element using parameter is not supported for NoTrackingWithIdentityResolution
                        // we can't always tell if the path is the same or not - e.g. when we use two different parameters with the same value
                        // or a constant and a parameter with the same value as the constant we would think it's different, but they are the same
                        // so we can't correctly flag this scenario as invalid and it could cause data corruption
                        // so we just disable it altogether
                        // consider this query:
                        // var prm1 = 0;
                        // var prm2 = 0
                        // entities.Select(x => new
                        // {
                        //     One = x.JsonCollection[prm1].NestedCollection[1],
                        //     Two = x.JsonCollection[prm2].NestedCollection
                        // })
                        if (outerKeyAccessInfo.Any(x => x.KeyProperty == null && x.KeyProjectionIndex != null))
                        {
                            throw new InvalidOperationException(
                                RelationalStrings.JsonProjectingCollectionElementAccessedUsingParmeterNoTrackingWithIdentityResolution(
                                    outerJsonEntityType.DisplayName(), nameof(QueryTrackingBehavior.NoTrackingWithIdentityResolution)));
                        }

                        for (var j = _projectedKeyAccessInfos.Count - 1; j > i; j--)
                        {
                            var innerKeyAccessInfo = _projectedKeyAccessInfos[j].KeyAccessInfo;
                            var innerJsonEntityType = _projectedKeyAccessInfos[j].JsonEntityType;

                            var different = false;
                            for (var k = 0; k < Math.Min(outerKeyAccessInfo.Count, innerKeyAccessInfo.Count); k++)
                            {
                                if (!KeyAccessInfoElementEqual(outerKeyAccessInfo[k], innerKeyAccessInfo[k]))
                                {
                                    different = true;
                                    break;
                                }
                            }

                            // if shared path is the same, we are ok if full paths are the same or if the outer path is shorter than inner
                            // in that case the inner entry can be removed from the list - it will land in ChangeTracker in the correct order
                            // if outer path is longer however, there is risk of data corruption so we throw
                            // if common paths are different, we don't do anything - outer and inner are different they won't clash in ChangeTracker
                            // just continue processing, inner will eventually become outer and we will validate if all is correct with it
                            if (!different)
                            {
                                if (outerJsonEntityType != innerJsonEntityType
                                    && !innerJsonEntityType.IsInOwnershipPath(outerJsonEntityType)
                                    && !outerJsonEntityType.IsInOwnershipPath(innerJsonEntityType))
                                {
                                    // inner and outer are on different ownership paths - they are not related so they won't clash
                                    continue;
                                }

                                if ((outerJsonEntityType == innerJsonEntityType
                                        || innerJsonEntityType.IsInOwnershipPath(outerJsonEntityType))
                                    && outerKeyAccessInfo.Count <= innerKeyAccessInfo.Count)
                                {
                                    // outer and inner are on same ownership paths and outer is the owner of inner
                                    // this is good - we can remove inner from the list now, because it will be materialized correctly
                                    _projectedKeyAccessInfos.RemoveAt(j);
                                    continue;
                                }

                                throw new InvalidOperationException(
                                    RelationalStrings.JsonProjectingEntitiesIncorrectOrderNoTrackingWithIdentityResolution(
                                        outerJsonEntityType.DisplayName(),
                                        nameof(QueryTrackingBehavior.NoTrackingWithIdentityResolution)));
                            }
                        }

                        i++;
                    }
                    while (i < _projectedKeyAccessInfos.Count);
                }

                static bool KeyAccessInfoElementEqual(
                    (IProperty? KeyProperty, int? ConstantKeyValue, int? KeyProjectionIndex) first,
                    (IProperty? KeyProperty, int? ConstantKeyValue, int? KeyProjectionIndex) second)
                {
                    if (first.KeyProperty != null != (second.KeyProperty != null))
                    {
                        return false;
                    }

                    if (first.ConstantKeyValue != second.ConstantKeyValue)
                    {
                        return false;
                    }

                    // key property itself could be different, as long as they map to the same index in data reader, it's the same key
                    // this could be a problem when index is a parameter (two different parameters but same value),
                    // but we disabled that scenario already
                    return first.KeyProjectionIndex == second.KeyProjectionIndex;
                }
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                if (extensionExpression is RelationalCollectionShaperExpression collectionShaperExpression)
                {
                    var insideCollection = _insideCollection;
                    _insideCollection = true;
                    Visit(collectionShaperExpression.InnerShaper);
                    _insideCollection = insideCollection;

                    return collectionShaperExpression;
                }

                if (extensionExpression is RelationalSplitCollectionShaperExpression splitCollectionShaperExpression)
                {
                    var insideCollection = _insideCollection;
                    _insideCollection = true;
                    var oldSelectExpression = _selectExpression;
                    _selectExpression = splitCollectionShaperExpression.SelectExpression;
                    Visit(splitCollectionShaperExpression.InnerShaper);
                    _insideCollection = insideCollection;
                    _selectExpression = oldSelectExpression;

                    return splitCollectionShaperExpression;
                }

                if (extensionExpression is IncludeExpression includeExpression)
                {
                    var insideInclude = _insideInclude;
                    _insideInclude = true;
                    Visit(includeExpression.NavigationExpression);
                    _insideInclude = insideInclude;
                }

                if (extensionExpression is StructuralTypeShaperExpression
                    {
                        ValueBufferExpression: ProjectionBindingExpression entityProjectionBindingExpression
                    } entityShaperExpression)
                {
                    var entityProjection = _selectExpression.GetProjection(entityProjectionBindingExpression).GetConstantValue<object>();

                    switch (entityProjection)
                    {
                        case QueryableJsonProjectionInfo:
                        case JsonProjectionInfo when _insideCollection:
                            throw new InvalidOperationException(
                                RelationalStrings.JsonProjectingQueryableOperationNoTrackingWithIdentityResolution(
                                    nameof(QueryTrackingBehavior.NoTrackingWithIdentityResolution)));

                        case JsonProjectionInfo jsonEntityProjectionInfo:
                        {
                            var jsonEntityType = (IEntityType)entityShaperExpression.StructuralType;
                            if (_insideInclude)
                            {
                                if (!_includedJsonEntityTypes.Contains(jsonEntityType))
                                {
                                    _includedJsonEntityTypes.Add(jsonEntityType);
                                }
                            }
                            else
                            {
                                _projectedKeyAccessInfos.Add((jsonEntityType, jsonEntityProjectionInfo.KeyAccessInfo));
                            }

                            break;
                        }

                        default:
                            return extensionExpression;
                    }
                }

                if (extensionExpression is CollectionResultExpression
                    {
                        ProjectionBindingExpression: ProjectionBindingExpression collectionProjectionBindingExpression
                    } collectionResultExpression)
                {
                    var collectionProjection =
                        _selectExpression.GetProjection(collectionProjectionBindingExpression).GetConstantValue<object>();

                    switch (collectionProjection)
                    {
                        case QueryableJsonProjectionInfo:
                        case JsonProjectionInfo when _insideCollection:
                            throw new InvalidOperationException(
                                RelationalStrings.JsonProjectingQueryableOperationNoTrackingWithIdentityResolution(
                                    nameof(QueryTrackingBehavior.NoTrackingWithIdentityResolution)));

                        case JsonProjectionInfo jsonCollectionProjectionInfo:
                        {
                            var jsonEntityType = collectionResultExpression.Navigation!.TargetEntityType;
                            if (_insideInclude)
                            {
                                if (!_includedJsonEntityTypes.Contains(jsonEntityType))
                                {
                                    _includedJsonEntityTypes.Add(jsonEntityType);
                                }
                            }
                            else
                            {
                                _projectedKeyAccessInfos.Add((jsonEntityType, jsonCollectionProjectionInfo.KeyAccessInfo));
                            }

                            break;
                        }

                        default:
                            return extensionExpression;
                    }
                }

                return base.VisitExtension(extensionExpression);
            }
        }
    }
}
