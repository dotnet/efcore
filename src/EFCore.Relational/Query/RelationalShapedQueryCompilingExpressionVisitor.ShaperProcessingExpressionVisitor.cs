// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class RelationalShapedQueryCompilingExpressionVisitor
    {
        private sealed class ShaperProcessingExpressionVisitor : ExpressionVisitor
        {
            // Reading database values
            private static readonly MethodInfo _isDbNullMethod =
                typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.IsDBNull), new[] { typeof(int) });

            private static readonly MethodInfo _getFieldValueMethod =
                typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.GetFieldValue), new[] { typeof(int) });

            private static readonly MethodInfo _throwReadValueExceptionMethod =
                typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ThrowReadValueException));

            // Coordinating results
            private static readonly MemberInfo _resultContextValuesMemberInfo
                = typeof(ResultContext).GetMember(nameof(ResultContext.Values))[0];

            private static readonly MemberInfo _singleQueryResultCoordinatorResultReadyMemberInfo
                = typeof(SingleQueryResultCoordinator).GetMember(nameof(SingleQueryResultCoordinator.ResultReady))[0];

            // Performing collection materialization
            private static readonly MethodInfo _includeReferenceMethodInfo
                = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(IncludeReference));

            private static readonly MethodInfo _initializeIncludeCollectionMethodInfo
                = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(InitializeIncludeCollection));

            private static readonly MethodInfo _populateIncludeCollectionMethodInfo
                = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(PopulateIncludeCollection));

            private static readonly MethodInfo _initializeSplitIncludeCollectionMethodInfo
                = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(InitializeSplitIncludeCollection));

            private static readonly MethodInfo _populateSplitIncludeCollectionMethodInfo
                = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(PopulateSplitIncludeCollection));

            private static readonly MethodInfo _populateSplitIncludeCollectionAsyncMethodInfo
                = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(PopulateSplitIncludeCollectionAsync));

            private static readonly MethodInfo _initializeCollectionMethodInfo
                = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(InitializeCollection));

            private static readonly MethodInfo _populateCollectionMethodInfo
                = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(PopulateCollection));

            private static readonly MethodInfo _initializeSplitCollectionMethodInfo
                = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(InitializeSplitCollection));

            private static readonly MethodInfo _populateSplitCollectionMethodInfo
                = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(PopulateSplitCollection));

            private static readonly MethodInfo _populateSplitCollectionAsyncMethodInfo
                = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(PopulateSplitCollectionAsync));

            private static readonly MethodInfo _taskAwaiterMethodInfo
                = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(TaskAwaiter));

            private static readonly MethodInfo _collectionAccessorAddMethodInfo
                = typeof(IClrCollectionAccessor).GetTypeInfo().GetDeclaredMethod(nameof(IClrCollectionAccessor.Add));

            private readonly RelationalShapedQueryCompilingExpressionVisitor _parentVisitor;
            private readonly ISet<string> _tags;
            private readonly bool _isTracking;
            private readonly bool _isAsync;
            private readonly bool _splitQuery;
            private readonly bool _detailedErrorsEnabled;
            private readonly bool _generateCommandCache;
            private readonly ParameterExpression _resultCoordinatorParameter;
            private readonly ParameterExpression _executionStrategyParameter;

            // States scoped to SelectExpression
            private readonly SelectExpression _selectExpression;
            private readonly ParameterExpression _dataReaderParameter;
            private readonly ParameterExpression _resultContextParameter;
            private readonly ParameterExpression _indexMapParameter;
            private readonly ReaderColumn[] _readerColumns;

            // States to materialize only once
            private readonly IDictionary<Expression, Expression> _variableShaperMapping = new Dictionary<Expression, Expression>();

            // There are always entity variables to avoid materializing same entity twice
            private readonly List<ParameterExpression> _variables = new List<ParameterExpression>();

            private readonly List<Expression> _expressions = new List<Expression>();

            // IncludeExpressions are added at the end in case they are using ValuesArray
            private readonly List<Expression> _includeExpressions = new List<Expression>();

            // If there is collection shaper then we need to construct ValuesArray to store values temporarily in ResultContext
            private List<Expression> _collectionPopulatingExpressions;
            private Expression _valuesArrayExpression;
            private List<Expression> _valuesArrayInitializers;

            private bool _containsCollectionMaterialization;

            // Since identifiers for collection are not part of larger lambda they don't cannot use caching to materialize only once.
            private bool _inline;

            // States to convert code to data reader read
            private readonly IDictionary<ParameterExpression, IDictionary<IProperty, int>> _materializationContextBindings
                = new Dictionary<ParameterExpression, IDictionary<IProperty, int>>();

            private readonly IDictionary<ParameterExpression, int> entityTypeIdentifyingExpressionOffsets
                = new Dictionary<ParameterExpression, int>();

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
                    _readerColumns = new ReaderColumn[_selectExpression.Projection.Count];
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
                ReaderColumn[] readerColumns)
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

            public LambdaExpression ProcessShaper(
                Expression shaperExpression,
                out RelationalCommandCache relationalCommandCache,
                out LambdaExpression relatedDataLoaders)
            {
                relatedDataLoaders = null;

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
                        _readerColumns,
                        _parentVisitor._useRelationalNulls);

                    return Expression.Lambda(
                        result,
                        QueryCompilationContext.QueryContextParameter,
                        _dataReaderParameter,
                        _indexMapParameter);
                }

                _containsCollectionMaterialization = new CollectionShaperFindingExpressionVisitor()
                    .ContainsCollectionMaterialization(shaperExpression);
                relatedDataLoaders = null;

                if (!_containsCollectionMaterialization)
                {
                    var result = Visit(shaperExpression);
                    _expressions.AddRange(_includeExpressions);
                    _expressions.Add(result);
                    result = Expression.Block(_variables, _expressions);

                    relationalCommandCache = _generateCommandCache
                        ? new RelationalCommandCache(
                            _parentVisitor.Dependencies.MemoryCache,
                            _parentVisitor.RelationalDependencies.QuerySqlGeneratorFactory,
                            _parentVisitor.RelationalDependencies.RelationalParameterBasedSqlProcessorFactory,
                            _selectExpression,
                            _readerColumns,
                            _parentVisitor._useRelationalNulls)
                        : null;

                    return Expression.Lambda(
                        result,
                        QueryCompilationContext.QueryContextParameter,
                        _dataReaderParameter,
                        _resultContextParameter,
                        _resultCoordinatorParameter);
                }
                else
                {
                    _valuesArrayExpression = Expression.MakeMemberAccess(_resultContextParameter, _resultContextValuesMemberInfo);
                    _collectionPopulatingExpressions = new List<Expression>();
                    _valuesArrayInitializers = new List<Expression>();

                    var result = Visit(shaperExpression);

                    var valueArrayInitializationExpression = Expression.Assign(
                        _valuesArrayExpression, Expression.NewArrayInit(typeof(object), _valuesArrayInitializers));

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
                                    Expression.Call(_taskAwaiterMethodInfo, tasks),
                                    QueryCompilationContext.QueryContextParameter,
                                    _executionStrategyParameter,
                                    _resultCoordinatorParameter);
                        }
                        else
                        {
                            relatedDataLoaders = Expression.Lambda<Action<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator>>(
                                Expression.Block(_collectionPopulatingExpressions),
                                QueryCompilationContext.QueryContextParameter,
                                _executionStrategyParameter,
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
                                        _resultCoordinatorParameter, _singleQueryResultCoordinatorResultReadyMemberInfo)),
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
                            _readerColumns,
                            _parentVisitor._useRelationalNulls)
                        : null;

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
                Check.NotNull(binaryExpression, nameof(binaryExpression));

                if (binaryExpression.NodeType == ExpressionType.Assign
                    && binaryExpression.Left is ParameterExpression parameterExpression
                    && parameterExpression.Type == typeof(MaterializationContext))
                {
                    var newExpression = (NewExpression)binaryExpression.Right;
                    var projectionBindingExpression = (ProjectionBindingExpression)newExpression.Arguments[0];

                    var propertyMap = (IDictionary<IProperty, int>)GetProjectionIndex(projectionBindingExpression);
                    _materializationContextBindings[parameterExpression] = propertyMap;
                    entityTypeIdentifyingExpressionOffsets[parameterExpression] = propertyMap.Values.Max() + 1;

                    var updatedExpression = Expression.New(
                        newExpression.Constructor,
                        Expression.Constant(ValueBuffer.Empty),
                        newExpression.Arguments[1]);

                    return Expression.Assign(binaryExpression.Left, updatedExpression);
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
                Check.NotNull(extensionExpression, nameof(extensionExpression));

                switch (extensionExpression)
                {
                    case RelationalEntityShaperExpression entityShaperExpression:
                    {
                        var key = GenerateKey((ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression);
                        if (!_variableShaperMapping.TryGetValue(key, out var accessor))
                        {
                            var entityParameter = Expression.Parameter(entityShaperExpression.Type);
                            _variables.Add(entityParameter);
                            var entityMaterializationExpression = _parentVisitor.InjectEntityMaterializers(entityShaperExpression);
                            entityMaterializationExpression = Visit(entityMaterializationExpression);

                            _expressions.Add(Expression.Assign(entityParameter, entityMaterializationExpression));

                            if (_containsCollectionMaterialization)
                            {
                                _valuesArrayInitializers.Add(entityParameter);
                                accessor = Expression.Convert(
                                    Expression.ArrayIndex(
                                        _valuesArrayExpression,
                                        Expression.Constant(_valuesArrayInitializers.Count - 1)),
                                    entityShaperExpression.Type);
                            }
                            else
                            {
                                accessor = entityParameter;
                            }

                            _variableShaperMapping[key] = accessor;
                        }

                        return accessor;
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
                            projection.Expression.TypeMapping,
                            projectionBindingExpression.Type);
                    }

                    case ProjectionBindingExpression projectionBindingExpression
                        when !_inline:
                    {
                        var key = GenerateKey(projectionBindingExpression);
                        if (_variableShaperMapping.TryGetValue(key, out var accessor))
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
                                    projection.Expression.TypeMapping,
                                    valueParameter.Type)));

                        if (_containsCollectionMaterialization)
                        {
                            var expressionToAdd = (Expression)valueParameter;
                            if (expressionToAdd.Type.IsValueType)
                            {
                                expressionToAdd = Expression.Convert(expressionToAdd, typeof(object));
                            }

                            _valuesArrayInitializers.Add(expressionToAdd);
                            accessor = Expression.Convert(
                                Expression.ArrayIndex(
                                    _valuesArrayExpression,
                                    Expression.Constant(_valuesArrayInitializers.Count - 1)),
                                projectionBindingExpression.Type);
                        }
                        else
                        {
                            accessor = valueParameter;
                        }

                        _variableShaperMapping[key] = accessor;

                        return accessor;
                    }

                    case IncludeExpression includeExpression:
                    {
                        var entity = Visit(includeExpression.EntityExpression);
                        if (includeExpression.NavigationExpression is RelationalCollectionShaperExpression
                            relationalCollectionShaperExpression)
                        {
                            var innerShaper = new ShaperProcessingExpressionVisitor(
                                    _parentVisitor, _resultCoordinatorParameter, _selectExpression, _dataReaderParameter,
                                    _resultContextParameter,
                                    _readerColumns)
                                .ProcessShaper(relationalCollectionShaperExpression.InnerShaper, out _, out _);

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

                            var collectionIdConstant = Expression.Constant(relationalCollectionShaperExpression.CollectionId);

                            _includeExpressions.Add(
                                Expression.Call(
                                    _initializeIncludeCollectionMethodInfo.MakeGenericMethod(entityType, includingEntityType),
                                    collectionIdConstant,
                                    QueryCompilationContext.QueryContextParameter,
                                    _dataReaderParameter,
                                    _resultCoordinatorParameter,
                                    entity,
                                    Expression.Constant(parentIdentifierLambda.Compile()),
                                    Expression.Constant(outerIdentifierLambda.Compile()),
                                    Expression.Constant(navigation),
                                    Expression.Constant(navigation.GetCollectionAccessor()),
                                    Expression.Constant(_isTracking)));

                            var relatedEntityType = innerShaper.ReturnType;
                            var inverseNavigation = navigation.Inverse;

                            _collectionPopulatingExpressions.Add(
                                Expression.Call(
                                    _populateIncludeCollectionMethodInfo.MakeGenericMethod(includingEntityType, relatedEntityType),
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
                            var innerProcessor = new ShaperProcessingExpressionVisitor(
                                _parentVisitor, _resultCoordinatorParameter,
                                _executionStrategyParameter, relationalSplitCollectionShaperExpression.SelectExpression, _tags);
                            var innerShaper = innerProcessor.ProcessShaper(
                                relationalSplitCollectionShaperExpression.InnerShaper,
                                out var relationalCommandCache,
                                out var relatedDataLoaders);

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

                            var collectionIdConstant = Expression.Constant(relationalSplitCollectionShaperExpression.CollectionId);

                            _includeExpressions.Add(
                                Expression.Call(
                                    _initializeSplitIncludeCollectionMethodInfo.MakeGenericMethod(entityType, includingEntityType),
                                    collectionIdConstant,
                                    QueryCompilationContext.QueryContextParameter,
                                    _dataReaderParameter,
                                    _resultCoordinatorParameter,
                                    entity,
                                    Expression.Constant(parentIdentifierLambda.Compile()),
                                    Expression.Constant(navigation),
                                    Expression.Constant(navigation.GetCollectionAccessor()),
                                    Expression.Constant(_isTracking)));

                            var relatedEntityType = innerShaper.ReturnType;
                            var inverseNavigation = navigation.Inverse;

                            _collectionPopulatingExpressions.Add(
                                Expression.Call(
                                    (_isAsync ? _populateSplitIncludeCollectionAsyncMethodInfo : _populateSplitIncludeCollectionMethodInfo)
                                    .MakeGenericMethod(includingEntityType, relatedEntityType),
                                    collectionIdConstant,
                                    Expression.Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                                    _executionStrategyParameter,
                                    Expression.Constant(_detailedErrorsEnabled),
                                    _resultCoordinatorParameter,
                                    Expression.Constant(relationalCommandCache),
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
                                _includeReferenceMethodInfo.MakeGenericMethod(entityType, includingType, relatedEntityType),
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
                        var key = GenerateKey(relationalCollectionShaperExpression);
                        if (!_variableShaperMapping.TryGetValue(key, out var accessor))
                        {
                            var innerShaper = new ShaperProcessingExpressionVisitor(
                                    _parentVisitor, _resultCoordinatorParameter, _selectExpression, _dataReaderParameter,
                                    _resultContextParameter,
                                    _readerColumns)
                                .ProcessShaper(relationalCollectionShaperExpression.InnerShaper, out _, out _);

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

                            var collectionIdConstant = Expression.Constant(relationalCollectionShaperExpression.CollectionId);

                            var collectionParameter = Expression.Parameter(relationalCollectionShaperExpression.Type);
                            _variables.Add(collectionParameter);
                            _expressions.Add(
                                Expression.Assign(
                                    collectionParameter,
                                    Expression.Call(
                                        _initializeCollectionMethodInfo.MakeGenericMethod(elementType, collectionType),
                                        collectionIdConstant,
                                        QueryCompilationContext.QueryContextParameter,
                                        _dataReaderParameter,
                                        _resultCoordinatorParameter,
                                        Expression.Constant(parentIdentifierLambda.Compile()),
                                        Expression.Constant(outerIdentifierLambda.Compile()),
                                        Expression.Constant(collectionAccessor, typeof(IClrCollectionAccessor)))));

                            _valuesArrayInitializers.Add(collectionParameter);
                            accessor = Expression.Convert(
                                Expression.ArrayIndex(
                                    _valuesArrayExpression,
                                    Expression.Constant(_valuesArrayInitializers.Count - 1)),
                                relationalCollectionShaperExpression.Type);

                            _collectionPopulatingExpressions.Add(
                                Expression.Call(
                                    _populateCollectionMethodInfo.MakeGenericMethod(collectionType, elementType, relatedElementType),
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

                            _variableShaperMapping[key] = accessor;
                        }

                        return accessor;
                    }

                    case RelationalSplitCollectionShaperExpression relationalSplitCollectionShaperExpression:
                    {
                        var key = GenerateKey(relationalSplitCollectionShaperExpression);
                        if (!_variableShaperMapping.TryGetValue(key, out var accessor))
                        {
                            var innerProcessor = new ShaperProcessingExpressionVisitor(
                                _parentVisitor, _resultCoordinatorParameter,
                                _executionStrategyParameter, relationalSplitCollectionShaperExpression.SelectExpression, _tags);
                            var innerShaper = innerProcessor.ProcessShaper(
                                relationalSplitCollectionShaperExpression.InnerShaper,
                                out var relationalCommandCache,
                                out var relatedDataLoaders);

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

                            var collectionIdConstant = Expression.Constant(relationalSplitCollectionShaperExpression.CollectionId);

                            var collectionParameter = Expression.Parameter(collectionType);
                            _variables.Add(collectionParameter);
                            _expressions.Add(
                                Expression.Assign(
                                    collectionParameter,
                                    Expression.Call(
                                        _initializeSplitCollectionMethodInfo.MakeGenericMethod(elementType, collectionType),
                                        collectionIdConstant,
                                        QueryCompilationContext.QueryContextParameter,
                                        _dataReaderParameter,
                                        _resultCoordinatorParameter,
                                        Expression.Constant(parentIdentifierLambda.Compile()),
                                        Expression.Constant(collectionAccessor, typeof(IClrCollectionAccessor)))));

                            _valuesArrayInitializers.Add(collectionParameter);
                            accessor = Expression.Convert(
                                Expression.ArrayIndex(
                                    _valuesArrayExpression,
                                    Expression.Constant(_valuesArrayInitializers.Count - 1)),
                                relationalSplitCollectionShaperExpression.Type);

                            _collectionPopulatingExpressions.Add(
                                Expression.Call(
                                    (_isAsync ? _populateSplitCollectionAsyncMethodInfo : _populateSplitCollectionMethodInfo)
                                    .MakeGenericMethod(collectionType, elementType, relatedElementType),
                                    collectionIdConstant,
                                    Expression.Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                                    _executionStrategyParameter,
                                    Expression.Constant(_detailedErrorsEnabled),
                                    _resultCoordinatorParameter,
                                    Expression.Constant(relationalCommandCache),
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

                            _variableShaperMapping[key] = accessor;
                        }

                        return accessor;
                    }

                    case GroupByShaperExpression _:
                        throw new InvalidOperationException(RelationalStrings.ClientGroupByNotSupported);
                }

                return base.VisitExtension(extensionExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                Check.NotNull(methodCallExpression, nameof(methodCallExpression));

                if (methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition()
                    == Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod)
                {
                    var property = (IProperty)((ConstantExpression)methodCallExpression.Arguments[2]).Value;
                    var mappingParameter = (ParameterExpression)((MethodCallExpression)methodCallExpression.Arguments[0]).Object;
                    var projectionIndex = property == null
                        ? entityTypeIdentifyingExpressionOffsets[mappingParameter]
                        + (int)((ConstantExpression)methodCallExpression.Arguments[1]).Value
                        : _materializationContextBindings[mappingParameter][property];
                    var projection = _selectExpression.Projection[projectionIndex];

                    var nullable = IsNullableProjection(projection);

                    Check.DebugAssert(
                        !nullable || property != null || methodCallExpression.Type.IsNullableType(),
                        "For nullable reads the return type must be null unless property is specified.");

                    return CreateGetValueExpression(
                        _dataReaderParameter,
                        projectionIndex,
                        nullable,
                        projection.Expression.TypeMapping,
                        methodCallExpression.Type,
                        property);
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            private static LambdaExpression GenerateFixup(
                Type entityType,
                Type relatedEntityType,
                INavigationBase navigation,
                INavigationBase inverseNavigation)
            {
                var entityParameter = Expression.Parameter(entityType);
                var relatedEntityParameter = Expression.Parameter(relatedEntityType);
                var expressions = new List<Expression>
                {
                    navigation.IsCollection
                        ? AddToCollectionNavigation(entityParameter, relatedEntityParameter, navigation)
                        : AssignReferenceNavigation(entityParameter, relatedEntityParameter, navigation)
                };

                if (inverseNavigation != null)
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
                    _collectionAccessorAddMethodInfo,
                    entity,
                    relatedEntity,
                    Expression.Constant(true));

            private Expression GenerateKey(Expression expression)
                => expression is ProjectionBindingExpression projectionBindingExpression
                    && projectionBindingExpression.ProjectionMember != null
                        ? _selectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember)
                        : expression;

            private object GetProjectionIndex(ProjectionBindingExpression projectionBindingExpression)
                => projectionBindingExpression.ProjectionMember != null
                    ? ((ConstantExpression)_selectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember)).Value
                    : (projectionBindingExpression.Index != null
                        ? (object)projectionBindingExpression.Index
                        : projectionBindingExpression.IndexMap);

            private static bool IsNullableProjection(ProjectionExpression projection)
                => !(projection.Expression is ColumnExpression column) || column.IsNullable;

            private Expression CreateGetValueExpression(
                ParameterExpression dbDataReader,
                int index,
                bool nullable,
                RelationalTypeMapping typeMapping,
                Type type,
                IPropertyBase property = null)
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
                            ? Expression.Convert(dbDataReader, getMethod.DeclaringType)
                            : (Expression)dbDataReader,
                        getMethod,
                        indexExpression);

                var buffering = false;
                if (_readerColumns != null
                    && _readerColumns[index] == null)
                {
                    buffering = true;
                    var bufferedReaderLambdaExpression = valueExpression;
                    var columnType = bufferedReaderLambdaExpression.Type;
                    if (!columnType.IsValueType
                        || !BufferedDataReader.IsSupportedValueType(columnType))
                    {
                        columnType = typeof(object);
                        bufferedReaderLambdaExpression = Expression.Convert(bufferedReaderLambdaExpression, columnType);
                    }

                    _readerColumns[index] = ReaderColumn.Create(
                        columnType,
                        nullable,
                        _indexMapParameter != null ? ((ColumnExpression)_selectExpression.Projection[index].Expression).Name : null,
                        property,
                        Expression.Lambda(
                            bufferedReaderLambdaExpression,
                            dbDataReader,
                            _indexMapParameter ?? Expression.Parameter(typeof(int[]))).Compile());

                    if (getMethod.DeclaringType != typeof(DbDataReader))
                    {
                        valueExpression = Expression.Call(
                            dbDataReader, RelationalTypeMapping.GetDataReaderMethod(columnType), indexExpression);
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
                    valueExpression = Expression.Condition(
                        Expression.Call(dbDataReader, _isDbNullMethod, indexExpression),
                        Expression.Default(valueExpression.Type),
                        valueExpression);
                }

                if (_detailedErrorsEnabled
                    && !buffering)
                {
                    var exceptionParameter = Expression.Parameter(typeof(Exception), name: "e");

                    var catchBlock = Expression.Catch(
                        exceptionParameter,
                        Expression.Call(
                            _throwReadValueExceptionMethod.MakeGenericMethod(valueExpression.Type),
                            exceptionParameter,
                            Expression.Call(dbDataReader, _getFieldValueMethod.MakeGenericMethod(typeof(object)), indexExpression),
                            Expression.Constant(valueExpression.Type.MakeNullable(nullable), typeof(Type)),
                            Expression.Constant(property, typeof(IPropertyBase))));

                    valueExpression = Expression.TryCatch(valueExpression, catchBlock);
                }

                return valueExpression;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static TValue ThrowReadValueException<TValue>(
                Exception exception,
                object value,
                Type expectedType,
                IPropertyBase property = null)
            {
                var actualType = value?.GetType();

                string message;

                if (property != null)
                {
                    var entityType = property.DeclaringType.DisplayName();
                    var propertyName = property.Name;
                    if (expectedType == typeof(object))
                    {
                        expectedType = property.ClrType;
                    }

                    message = exception is NullReferenceException
                        || Equals(value, DBNull.Value)
                            ? RelationalStrings.ErrorMaterializingPropertyNullReference(entityType, propertyName, expectedType)
                            : exception is InvalidCastException
                                ? CoreStrings.ErrorMaterializingPropertyInvalidCast(entityType, propertyName, expectedType, actualType)
                                : RelationalStrings.ErrorMaterializingProperty(entityType, propertyName);
                }
                else
                {
                    message = exception is NullReferenceException
                        || Equals(value, DBNull.Value)
                            ? RelationalStrings.ErrorMaterializingValueNullReference(expectedType)
                            : exception is InvalidCastException
                                ? RelationalStrings.ErrorMaterializingValueInvalidCast(expectedType, actualType)
                                : RelationalStrings.ErrorMaterializingValue;
                }

                throw new InvalidOperationException(message, exception);
            }

            private static void IncludeReference<TEntity, TIncludingEntity, TIncludedEntity>(
                QueryContext queryContext,
                TEntity entity,
                TIncludedEntity relatedEntity,
                INavigationBase navigation,
                INavigationBase inverseNavigation,
                Action<TIncludingEntity, TIncludedEntity> fixup,
                bool trackingQuery)
                where TEntity : class
                where TIncludingEntity : class, TEntity
                where TIncludedEntity : class
            {
                if (entity is TIncludingEntity includingEntity)
                {
                    if (trackingQuery
                        && navigation.DeclaringEntityType.FindPrimaryKey() != null)
                    {
                        // For non-null relatedEntity StateManager will set the flag
                        if (relatedEntity == null)
                        {
                            queryContext.SetNavigationIsLoaded(includingEntity, navigation);
                        }
                    }
                    else
                    {
                        navigation.SetIsLoadedWhenNoTracking(includingEntity);
                        if (relatedEntity != null)
                        {
                            fixup(includingEntity, relatedEntity);
                            if (inverseNavigation != null
                                && !inverseNavigation.IsCollection)
                            {
                                inverseNavigation.SetIsLoadedWhenNoTracking(relatedEntity);
                            }
                        }
                    }
                }
            }

            private static void InitializeIncludeCollection<TParent, TNavigationEntity>(
                int collectionId,
                QueryContext queryContext,
                DbDataReader dbDataReader,
                SingleQueryResultCoordinator resultCoordinator,
                TParent entity,
                Func<QueryContext, DbDataReader, object[]> parentIdentifier,
                Func<QueryContext, DbDataReader, object[]> outerIdentifier,
                INavigationBase navigation,
                IClrCollectionAccessor clrCollectionAccessor,
                bool trackingQuery)
                where TParent : class
                where TNavigationEntity : class, TParent
            {
                object collection = null;
                if (entity is TNavigationEntity)
                {
                    if (trackingQuery)
                    {
                        queryContext.SetNavigationIsLoaded(entity, navigation);
                    }
                    else
                    {
                        navigation.SetIsLoadedWhenNoTracking(entity);
                    }

                    collection = clrCollectionAccessor.GetOrCreate(entity, forMaterialization: true);
                }

                var parentKey = parentIdentifier(queryContext, dbDataReader);
                var outerKey = outerIdentifier(queryContext, dbDataReader);

                var collectionMaterializationContext = new SingleQueryCollectionContext(entity, collection, parentKey, outerKey);

                resultCoordinator.SetSingleQueryCollectionContext(collectionId, collectionMaterializationContext);
            }

            private static void PopulateIncludeCollection<TIncludingEntity, TIncludedEntity>(
                int collectionId,
                QueryContext queryContext,
                DbDataReader dbDataReader,
                SingleQueryResultCoordinator resultCoordinator,
                Func<QueryContext, DbDataReader, object[]> parentIdentifier,
                Func<QueryContext, DbDataReader, object[]> outerIdentifier,
                Func<QueryContext, DbDataReader, object[]> selfIdentifier,
                IReadOnlyList<ValueComparer> parentIdentifierValueComparers,
                IReadOnlyList<ValueComparer> outerIdentifierValueComparers,
                IReadOnlyList<ValueComparer> selfIdentifierValueComparers,
                Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, TIncludedEntity> innerShaper,
                INavigationBase inverseNavigation,
                Action<TIncludingEntity, TIncludedEntity> fixup,
                bool trackingQuery)
                where TIncludingEntity : class
                where TIncludedEntity : class
            {
                var collectionMaterializationContext = resultCoordinator.Collections[collectionId];
                if (collectionMaterializationContext.Parent is TIncludingEntity entity)
                {
                    if (resultCoordinator.HasNext == false)
                    {
                        // Outer Enumerator has ended
                        GenerateCurrentElementIfPending();
                        return;
                    }

                    if (!CompareIdentifiers(
                        outerIdentifierValueComparers,
                        outerIdentifier(queryContext, dbDataReader), collectionMaterializationContext.OuterIdentifier))
                    {
                        // Outer changed so collection has ended. Materialize last element.
                        GenerateCurrentElementIfPending();
                        // If parent also changed then this row is now pointing to element of next collection
                        if (!CompareIdentifiers(
                            parentIdentifierValueComparers,
                            parentIdentifier(queryContext, dbDataReader), collectionMaterializationContext.ParentIdentifier))
                        {
                            resultCoordinator.HasNext = true;
                        }

                        return;
                    }

                    var innerKey = selfIdentifier(queryContext, dbDataReader);
                    if (innerKey.All(e => e == null))
                    {
                        // No correlated element
                        return;
                    }

                    if (collectionMaterializationContext.SelfIdentifier != null)
                    {
                        if (CompareIdentifiers(selfIdentifierValueComparers, innerKey, collectionMaterializationContext.SelfIdentifier))
                        {
                            // repeated row for current element
                            // If it is pending materialization then it may have nested elements
                            if (collectionMaterializationContext.ResultContext.Values != null)
                            {
                                ProcessCurrentElementRow();
                            }

                            resultCoordinator.ResultReady = false;
                            return;
                        }

                        // Row for new element which is not first element
                        // So materialize the element
                        GenerateCurrentElementIfPending();
                        resultCoordinator.HasNext = null;
                        collectionMaterializationContext.UpdateSelfIdentifier(innerKey);
                    }
                    else
                    {
                        // First row for current element
                        collectionMaterializationContext.UpdateSelfIdentifier(innerKey);
                    }

                    ProcessCurrentElementRow();
                    resultCoordinator.ResultReady = false;
                }

                void ProcessCurrentElementRow()
                {
                    var previousResultReady = resultCoordinator.ResultReady;
                    resultCoordinator.ResultReady = true;
                    var relatedEntity = innerShaper(
                        queryContext, dbDataReader, collectionMaterializationContext.ResultContext, resultCoordinator);
                    if (resultCoordinator.ResultReady)
                    {
                        // related entity is materialized
                        collectionMaterializationContext.ResultContext.Values = null;
                        if (!trackingQuery)
                        {
                            fixup(entity, relatedEntity);
                            if (inverseNavigation != null)
                            {
                                inverseNavigation.SetIsLoadedWhenNoTracking(relatedEntity);
                            }
                        }
                    }

                    resultCoordinator.ResultReady &= previousResultReady;
                }

                void GenerateCurrentElementIfPending()
                {
                    if (collectionMaterializationContext.ResultContext.Values != null)
                    {
                        resultCoordinator.HasNext = false;
                        ProcessCurrentElementRow();
                    }

                    collectionMaterializationContext.UpdateSelfIdentifier(null);
                }
            }

            private static void InitializeSplitIncludeCollection<TParent, TNavigationEntity>(
                int collectionId,
                QueryContext queryContext,
                DbDataReader parentDataReader,
                SplitQueryResultCoordinator resultCoordinator,
                TParent entity,
                Func<QueryContext, DbDataReader, object[]> parentIdentifier,
                INavigationBase navigation,
                IClrCollectionAccessor clrCollectionAccessor,
                bool trackingQuery)
                where TParent : class
                where TNavigationEntity : class, TParent
            {
                object collection = null;
                if (entity is TNavigationEntity)
                {
                    if (trackingQuery)
                    {
                        queryContext.SetNavigationIsLoaded(entity, navigation);
                    }
                    else
                    {
                        navigation.SetIsLoadedWhenNoTracking(entity);
                    }

                    collection = clrCollectionAccessor.GetOrCreate(entity, forMaterialization: true);
                }

                var parentKey = parentIdentifier(queryContext, parentDataReader);

                var splitQueryCollectionContext = new SplitQueryCollectionContext(entity, collection, parentKey);

                resultCoordinator.SetSplitQueryCollectionContext(collectionId, splitQueryCollectionContext);
            }

            private static void PopulateSplitIncludeCollection<TIncludingEntity, TIncludedEntity>(
                int collectionId,
                RelationalQueryContext queryContext,
                IExecutionStrategy executionStrategy,
                bool detailedErrorsEnabled,
                SplitQueryResultCoordinator resultCoordinator,
                RelationalCommandCache relationalCommandCache,
                Func<QueryContext, DbDataReader, object[]> childIdentifier,
                IReadOnlyList<ValueComparer> identifierValueComparers,
                Func<QueryContext, DbDataReader, ResultContext, SplitQueryResultCoordinator, TIncludedEntity> innerShaper,
                Action<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator> relatedDataLoaders,
                INavigationBase inverseNavigation,
                Action<TIncludingEntity, TIncludedEntity> fixup,
                bool trackingQuery)
                where TIncludingEntity : class
                where TIncludedEntity : class
            {
                if (resultCoordinator.DataReaders.Count <= collectionId
                    || resultCoordinator.DataReaders[collectionId] == null)
                {
                    // Execute and fetch data reader
                    RelationalDataReader dataReader = null;
                    executionStrategy.Execute(true, InitializeReader, null);

                    bool InitializeReader(DbContext _, bool result)
                    {
                        var relationalCommand = relationalCommandCache.GetRelationalCommand(queryContext.ParameterValues);

                        dataReader = relationalCommand.ExecuteReader(
                            new RelationalCommandParameterObject(
                                queryContext.Connection,
                                queryContext.ParameterValues,
                                relationalCommandCache.ReaderColumns,
                                queryContext.Context,
                                queryContext.CommandLogger,
                                detailedErrorsEnabled));

                        return result;
                    }

                    resultCoordinator.SetDataReader(collectionId, dataReader);
                }

                var splitQueryCollectionContext = resultCoordinator.Collections[collectionId];
                var dataReaderContext = resultCoordinator.DataReaders[collectionId];
                var dbDataReader = dataReaderContext.DataReader.DbDataReader;
                if (splitQueryCollectionContext.Parent is TIncludingEntity entity)
                {
                    while (dataReaderContext.HasNext ?? dbDataReader.Read())
                    {
                        if (!CompareIdentifiers(
                            identifierValueComparers,
                            splitQueryCollectionContext.ParentIdentifier, childIdentifier(queryContext, dbDataReader)))
                        {
                            dataReaderContext.HasNext = true;

                            return;
                        }

                        dataReaderContext.HasNext = null;
                        splitQueryCollectionContext.ResultContext.Values = null;

                        innerShaper(queryContext, dbDataReader, splitQueryCollectionContext.ResultContext, resultCoordinator);
                        relatedDataLoaders?.Invoke(queryContext, executionStrategy, resultCoordinator);
                        var relatedEntity = innerShaper(
                            queryContext, dbDataReader, splitQueryCollectionContext.ResultContext, resultCoordinator);

                        if (!trackingQuery)
                        {
                            fixup(entity, relatedEntity);
                            if (inverseNavigation != null)
                            {
                                inverseNavigation.SetIsLoadedWhenNoTracking(relatedEntity);
                            }
                        }
                    }

                    dataReaderContext.HasNext = false;
                }
            }

            private static async Task PopulateSplitIncludeCollectionAsync<TIncludingEntity, TIncludedEntity>(
                int collectionId,
                RelationalQueryContext queryContext,
                IExecutionStrategy executionStrategy,
                bool detailedErrorsEnabled,
                SplitQueryResultCoordinator resultCoordinator,
                RelationalCommandCache relationalCommandCache,
                Func<QueryContext, DbDataReader, object[]> childIdentifier,
                IReadOnlyList<ValueComparer> identifierValueComparers,
                Func<QueryContext, DbDataReader, ResultContext, SplitQueryResultCoordinator, TIncludedEntity> innerShaper,
                Func<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator, Task> relatedDataLoaders,
                INavigationBase inverseNavigation,
                Action<TIncludingEntity, TIncludedEntity> fixup,
                bool trackingQuery)
                where TIncludingEntity : class
                where TIncludedEntity : class
            {
                if (resultCoordinator.DataReaders.Count <= collectionId
                    || resultCoordinator.DataReaders[collectionId] == null)
                {
                    // Execute and fetch data reader
                    RelationalDataReader dataReader = null;
                    await executionStrategy.ExecuteAsync(
                        true, InitializeReaderAsync, null, queryContext.CancellationToken).ConfigureAwait(false);

                    async Task<bool> InitializeReaderAsync(DbContext _, bool result, CancellationToken cancellationToken)
                    {
                        var relationalCommand = relationalCommandCache.GetRelationalCommand(queryContext.ParameterValues);

                        dataReader = await relationalCommand.ExecuteReaderAsync(
                            new RelationalCommandParameterObject(
                                queryContext.Connection,
                                queryContext.ParameterValues,
                                relationalCommandCache.ReaderColumns,
                                queryContext.Context,
                                queryContext.CommandLogger,
                                detailedErrorsEnabled),
                            cancellationToken)
                            .ConfigureAwait(false);

                        return result;
                    }

                    resultCoordinator.SetDataReader(collectionId, dataReader);
                }

                var splitQueryCollectionContext = resultCoordinator.Collections[collectionId];
                var dataReaderContext = resultCoordinator.DataReaders[collectionId];
                var dbDataReader = dataReaderContext.DataReader.DbDataReader;
                if (splitQueryCollectionContext.Parent is TIncludingEntity entity)
                {
                    while (dataReaderContext.HasNext ?? await dbDataReader.ReadAsync(queryContext.CancellationToken).ConfigureAwait(false))
                    {
                        if (!CompareIdentifiers(
                            identifierValueComparers,
                            splitQueryCollectionContext.ParentIdentifier, childIdentifier(queryContext, dbDataReader)))
                        {
                            dataReaderContext.HasNext = true;

                            return;
                        }

                        dataReaderContext.HasNext = null;
                        splitQueryCollectionContext.ResultContext.Values = null;

                        innerShaper(queryContext, dbDataReader, splitQueryCollectionContext.ResultContext, resultCoordinator);
                        if (relatedDataLoaders != null)
                        {
                            await relatedDataLoaders(queryContext, executionStrategy, resultCoordinator).ConfigureAwait(false);
                        }

                        var relatedEntity = innerShaper(
                            queryContext, dbDataReader, splitQueryCollectionContext.ResultContext, resultCoordinator);

                        if (!trackingQuery)
                        {
                            fixup(entity, relatedEntity);
                            if (inverseNavigation != null)
                            {
                                inverseNavigation.SetIsLoadedWhenNoTracking(relatedEntity);
                            }
                        }
                    }

                    dataReaderContext.HasNext = false;
                }
            }

            private static TCollection InitializeCollection<TElement, TCollection>(
                int collectionId,
                QueryContext queryContext,
                DbDataReader dbDataReader,
                SingleQueryResultCoordinator resultCoordinator,
                Func<QueryContext, DbDataReader, object[]> parentIdentifier,
                Func<QueryContext, DbDataReader, object[]> outerIdentifier,
                IClrCollectionAccessor clrCollectionAccessor)
                where TCollection : class, ICollection<TElement>
            {
                var collection = clrCollectionAccessor?.Create() ?? new List<TElement>();

                var parentKey = parentIdentifier(queryContext, dbDataReader);
                var outerKey = outerIdentifier(queryContext, dbDataReader);

                var collectionMaterializationContext = new SingleQueryCollectionContext(null, collection, parentKey, outerKey);

                resultCoordinator.SetSingleQueryCollectionContext(collectionId, collectionMaterializationContext);

                return (TCollection)collection;
            }

            private static void PopulateCollection<TCollection, TElement, TRelatedEntity>(
                int collectionId,
                QueryContext queryContext,
                DbDataReader dbDataReader,
                SingleQueryResultCoordinator resultCoordinator,
                Func<QueryContext, DbDataReader, object[]> parentIdentifier,
                Func<QueryContext, DbDataReader, object[]> outerIdentifier,
                Func<QueryContext, DbDataReader, object[]> selfIdentifier,
                IReadOnlyList<ValueComparer> parentIdentifierValueComparers,
                IReadOnlyList<ValueComparer> outerIdentifierValueComparers,
                IReadOnlyList<ValueComparer> selfIdentifierValueComparers,
                Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, TRelatedEntity> innerShaper)
                where TRelatedEntity : TElement
                where TCollection : class, ICollection<TElement>
            {
                var collectionMaterializationContext = resultCoordinator.Collections[collectionId];
                if (collectionMaterializationContext.Collection is null)
                {
                    // nothing to materialize since no collection created
                    return;
                }

                if (resultCoordinator.HasNext == false)
                {
                    // Outer Enumerator has ended
                    GenerateCurrentElementIfPending();
                    return;
                }

                if (!CompareIdentifiers(
                    outerIdentifierValueComparers,
                    outerIdentifier(queryContext, dbDataReader), collectionMaterializationContext.OuterIdentifier))
                {
                    // Outer changed so collection has ended. Materialize last element.
                    GenerateCurrentElementIfPending();
                    // If parent also changed then this row is now pointing to element of next collection
                    if (!CompareIdentifiers(
                        parentIdentifierValueComparers,
                        parentIdentifier(queryContext, dbDataReader), collectionMaterializationContext.ParentIdentifier))
                    {
                        resultCoordinator.HasNext = true;
                    }

                    return;
                }

                var innerKey = selfIdentifier(queryContext, dbDataReader);
                if (innerKey.Length > 0 && innerKey.All(e => e == null))
                {
                    // No correlated element
                    return;
                }

                if (collectionMaterializationContext.SelfIdentifier != null)
                {
                    if (CompareIdentifiers(
                        selfIdentifierValueComparers,
                        innerKey, collectionMaterializationContext.SelfIdentifier))
                    {
                        // repeated row for current element
                        // If it is pending materialization then it may have nested elements
                        if (collectionMaterializationContext.ResultContext.Values != null)
                        {
                            ProcessCurrentElementRow();
                        }

                        resultCoordinator.ResultReady = false;
                        return;
                    }

                    // Row for new element which is not first element
                    // So materialize the element
                    GenerateCurrentElementIfPending();
                    resultCoordinator.HasNext = null;
                    collectionMaterializationContext.UpdateSelfIdentifier(innerKey);
                }
                else
                {
                    // First row for current element
                    collectionMaterializationContext.UpdateSelfIdentifier(innerKey);
                }

                ProcessCurrentElementRow();
                resultCoordinator.ResultReady = false;

                void ProcessCurrentElementRow()
                {
                    var previousResultReady = resultCoordinator.ResultReady;
                    resultCoordinator.ResultReady = true;
                    var element = innerShaper(
                        queryContext, dbDataReader, collectionMaterializationContext.ResultContext, resultCoordinator);
                    if (resultCoordinator.ResultReady)
                    {
                        // related element is materialized
                        collectionMaterializationContext.ResultContext.Values = null;
                        ((TCollection)collectionMaterializationContext.Collection).Add(element);
                    }

                    resultCoordinator.ResultReady &= previousResultReady;
                }

                void GenerateCurrentElementIfPending()
                {
                    if (collectionMaterializationContext.ResultContext.Values != null)
                    {
                        resultCoordinator.HasNext = false;
                        ProcessCurrentElementRow();
                    }

                    collectionMaterializationContext.UpdateSelfIdentifier(null);
                }
            }

            private static TCollection InitializeSplitCollection<TElement, TCollection>(
                int collectionId,
                QueryContext queryContext,
                DbDataReader parentDataReader,
                SplitQueryResultCoordinator resultCoordinator,
                Func<QueryContext, DbDataReader, object[]> parentIdentifier,
                IClrCollectionAccessor clrCollectionAccessor)
                where TCollection : class, ICollection<TElement>
            {
                var collection = clrCollectionAccessor?.Create() ?? new List<TElement>();
                var parentKey = parentIdentifier(queryContext, parentDataReader);
                var splitQueryCollectionContext = new SplitQueryCollectionContext(null, collection, parentKey);

                resultCoordinator.SetSplitQueryCollectionContext(collectionId, splitQueryCollectionContext);

                return (TCollection)collection;
            }

            private static void PopulateSplitCollection<TCollection, TElement, TRelatedEntity>(
                int collectionId,
                RelationalQueryContext queryContext,
                IExecutionStrategy executionStrategy,
                bool detailedErrorsEnabled,
                SplitQueryResultCoordinator resultCoordinator,
                RelationalCommandCache relationalCommandCache,
                Func<QueryContext, DbDataReader, object[]> childIdentifier,
                IReadOnlyList<ValueComparer> identifierValueComparers,
                Func<QueryContext, DbDataReader, ResultContext, SplitQueryResultCoordinator, TRelatedEntity> innerShaper,
                Action<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator> relatedDataLoaders)
                where TRelatedEntity : TElement
                where TCollection : class, ICollection<TElement>
            {
                if (resultCoordinator.DataReaders.Count <= collectionId
                    || resultCoordinator.DataReaders[collectionId] == null)
                {
                    // Execute and fetch data reader
                    RelationalDataReader dataReader = null;
                    executionStrategy.Execute(true, InitializeReader, null);

                    bool InitializeReader(DbContext _, bool result)
                    {
                        var relationalCommand = relationalCommandCache.GetRelationalCommand(queryContext.ParameterValues);

                        dataReader = relationalCommand.ExecuteReader(
                            new RelationalCommandParameterObject(
                                queryContext.Connection,
                                queryContext.ParameterValues,
                                relationalCommandCache.ReaderColumns,
                                queryContext.Context,
                                queryContext.CommandLogger,
                                detailedErrorsEnabled));

                        return result;
                    }

                    resultCoordinator.SetDataReader(collectionId, dataReader);
                }

                var splitQueryCollectionContext = resultCoordinator.Collections[collectionId];
                var dataReaderContext = resultCoordinator.DataReaders[collectionId];
                var dbDataReader = dataReaderContext.DataReader.DbDataReader;
                if (splitQueryCollectionContext.Collection is null)
                {
                    // nothing to materialize since no collection created
                    return;
                }

                while (dataReaderContext.HasNext ?? dbDataReader.Read())
                {
                    if (!CompareIdentifiers(
                        identifierValueComparers,
                        splitQueryCollectionContext.ParentIdentifier, childIdentifier(queryContext, dbDataReader)))
                    {
                        dataReaderContext.HasNext = true;

                        return;
                    }

                    dataReaderContext.HasNext = null;
                    splitQueryCollectionContext.ResultContext.Values = null;

                    innerShaper(queryContext, dbDataReader, splitQueryCollectionContext.ResultContext, resultCoordinator);
                    relatedDataLoaders?.Invoke(queryContext, executionStrategy, resultCoordinator);
                    var relatedElement = innerShaper(
                        queryContext, dbDataReader, splitQueryCollectionContext.ResultContext, resultCoordinator);
                    ((TCollection)splitQueryCollectionContext.Collection).Add(relatedElement);
                }

                dataReaderContext.HasNext = false;
            }

            private static async Task PopulateSplitCollectionAsync<TCollection, TElement, TRelatedEntity>(
                int collectionId,
                RelationalQueryContext queryContext,
                IExecutionStrategy executionStrategy,
                bool detailedErrorsEnabled,
                SplitQueryResultCoordinator resultCoordinator,
                RelationalCommandCache relationalCommandCache,
                Func<QueryContext, DbDataReader, object[]> childIdentifier,
                IReadOnlyList<ValueComparer> identifierValueComparers,
                Func<QueryContext, DbDataReader, ResultContext, SplitQueryResultCoordinator, TRelatedEntity> innerShaper,
                Func<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator, Task> relatedDataLoaders)
                where TRelatedEntity : TElement
                where TCollection : class, ICollection<TElement>
            {
                if (resultCoordinator.DataReaders.Count <= collectionId
                    || resultCoordinator.DataReaders[collectionId] == null)
                {
                    // Execute and fetch data reader
                    RelationalDataReader dataReader = null;
                    await executionStrategy.ExecuteAsync(
                        true, InitializeReaderAsync, null, queryContext.CancellationToken).ConfigureAwait(false);

                    async Task<bool> InitializeReaderAsync(DbContext _, bool result, CancellationToken cancellationToken)
                    {
                        var relationalCommand = relationalCommandCache.GetRelationalCommand(queryContext.ParameterValues);

                        dataReader = await relationalCommand.ExecuteReaderAsync(
                            new RelationalCommandParameterObject(
                                queryContext.Connection,
                                queryContext.ParameterValues,
                                relationalCommandCache.ReaderColumns,
                                queryContext.Context,
                                queryContext.CommandLogger,
                                detailedErrorsEnabled),
                            cancellationToken)
                            .ConfigureAwait(false);

                        return result;
                    }

                    resultCoordinator.SetDataReader(collectionId, dataReader);
                }

                var splitQueryCollectionContext = resultCoordinator.Collections[collectionId];
                var dataReaderContext = resultCoordinator.DataReaders[collectionId];
                var dbDataReader = dataReaderContext.DataReader.DbDataReader;
                if (splitQueryCollectionContext.Collection is null)
                {
                    // nothing to materialize since no collection created
                    return;
                }

                while (dataReaderContext.HasNext ?? await dbDataReader.ReadAsync(queryContext.CancellationToken).ConfigureAwait(false))
                {
                    if (!CompareIdentifiers(
                        identifierValueComparers,
                        splitQueryCollectionContext.ParentIdentifier, childIdentifier(queryContext, dbDataReader)))
                    {
                        dataReaderContext.HasNext = true;

                        return;
                    }

                    dataReaderContext.HasNext = null;
                    splitQueryCollectionContext.ResultContext.Values = null;

                    innerShaper(queryContext, dbDataReader, splitQueryCollectionContext.ResultContext, resultCoordinator);
                    if (relatedDataLoaders != null)
                    {
                        await relatedDataLoaders(queryContext, executionStrategy, resultCoordinator).ConfigureAwait(false);
                    }

                    var relatedElement = innerShaper(
                        queryContext, dbDataReader, splitQueryCollectionContext.ResultContext, resultCoordinator);
                    ((TCollection)splitQueryCollectionContext.Collection).Add(relatedElement);
                }

                dataReaderContext.HasNext = false;
            }

            private static async Task TaskAwaiter(Func<Task>[] taskFactories)
            {
                for (var i = 0; i < taskFactories.Length; i++)
                {
                    await taskFactories[i]().ConfigureAwait(false);
                }
            }

            private static bool CompareIdentifiers(IReadOnlyList<ValueComparer> valueComparers, object[] left, object[] right)
            {
                if (valueComparers != null)
                {
                    // Ignoring size check on all for perf as they should be same unless bug in code.
                    for (var i = 0; i < left.Length; i++)
                    {
                        if (valueComparers[i] != null
                            ? !valueComparers[i].Equals(left[i], right[i])
                            : !Equals(left[i], right[i]))
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return StructuralComparisons.StructuralEqualityComparer.Equals(left, right);
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

                public override Expression Visit(Expression expression)
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
        }
    }
}
