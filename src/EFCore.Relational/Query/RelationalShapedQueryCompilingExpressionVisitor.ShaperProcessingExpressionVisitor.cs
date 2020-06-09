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

            private static readonly MemberInfo _resultCoordinatorResultReadyMemberInfo
                = typeof(ResultCoordinator).GetMember(nameof(ResultCoordinator.ResultReady))[0];

            // Performing collection materialization
            private static readonly MethodInfo _includeReferenceMethodInfo
               = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(IncludeReference));
            private static readonly MethodInfo _initializeIncludeCollectionMethodInfo
                = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(InitializeIncludeCollection));
            private static readonly MethodInfo _populateIncludeCollectionMethodInfo
                = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(PopulateIncludeCollection));
            private static readonly MethodInfo _initializeCollectionMethodInfo
                = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(InitializeCollection));
            private static readonly MethodInfo _populateCollectionMethodInfo
                = typeof(ShaperProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(PopulateCollection));
            private static readonly MethodInfo _collectionAccessorAddMethodInfo
                = typeof(IClrCollectionAccessor).GetTypeInfo().GetDeclaredMethod(nameof(IClrCollectionAccessor.Add));

            private readonly RelationalShapedQueryCompilingExpressionVisitor _parentVisitor;
            private readonly bool _detailedErrorsEnabled;
            private readonly bool _nested;

            // States scoped to SelectExpression
            private readonly SelectExpression _selectExpression;
            private readonly ParameterExpression _dataReaderParameter;
            private readonly ParameterExpression _resultContextParameter;
            private readonly ParameterExpression _resultCoordinatorParameter;
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

            public ShaperProcessingExpressionVisitor(
                RelationalShapedQueryCompilingExpressionVisitor parentVisitor,
                SelectExpression selectExpression,
                bool indexMap)
                : this(parentVisitor, selectExpression,
                      Expression.Parameter(typeof(DbDataReader), "dataReader"),
                      Expression.Parameter(typeof(ResultContext), "resultContext"),
                      Expression.Parameter(typeof(ResultCoordinator), "resultCoordinator"),
                      indexMap ? Expression.Parameter(typeof(int[]), "indexMap") : null,
                      parentVisitor.QueryCompilationContext.IsBuffering ? new ReaderColumn[selectExpression.Projection.Count] : null,
                      nested: false)
            {
            }

            // Private ctor to preserve states when needed for nested visitor
            private ShaperProcessingExpressionVisitor(
                RelationalShapedQueryCompilingExpressionVisitor parentVisitor,
                SelectExpression selectExpression,
                ParameterExpression dataReaderParameter,
                ParameterExpression resultContextParameter,
                ParameterExpression resultCoordinatorParameter,
                ParameterExpression indexMapParameter,
                ReaderColumn[] readerColumns,
                bool nested)
            {
                _parentVisitor = parentVisitor;
                _selectExpression = selectExpression;
                _dataReaderParameter = dataReaderParameter;
                _resultContextParameter = resultContextParameter;
                _resultCoordinatorParameter = resultCoordinatorParameter;
                _indexMapParameter = indexMapParameter;
                _readerColumns = readerColumns;
                _nested = nested;
                _detailedErrorsEnabled = parentVisitor._detailedErrorsEnabled;
            }

            public LambdaExpression ProcessShaper(Expression shaperExpression, out RelationalCommandCache relationalCommandCache)
            {
                if (_indexMapParameter == null)
                {
                    _containsCollectionMaterialization = new CollectionShaperFindingExpressionVisitor()
                       .ContainsCollectionMaterialization(shaperExpression);
                }

                if (_containsCollectionMaterialization)
                {
                    _valuesArrayExpression = Expression.MakeMemberAccess(_resultContextParameter, _resultContextValuesMemberInfo);
                    _collectionPopulatingExpressions = new List<Expression>();
                    _valuesArrayInitializers = new List<Expression>();
                }

                var result = Visit(shaperExpression);

                if (_containsCollectionMaterialization)
                {
                    var valueArrayInitializationExpression = Expression.Assign(
                        _valuesArrayExpression,
                        Expression.NewArrayInit(
                            typeof(object),
                            _valuesArrayInitializers));

                    _expressions.Add(valueArrayInitializationExpression);
                    _expressions.AddRange(_includeExpressions);

                    var initializationBlock = Expression.Block(
                        _variables,
                        _expressions);

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
                                    _resultCoordinatorParameter, _resultCoordinatorResultReadyMemberInfo)),
                            result,
                            Expression.Default(result.Type)));

                    result = Expression.Block(conditionalMaterializationExpressions);
                }
                else
                {
                    _expressions.AddRange(_includeExpressions);
                    _expressions.Add(result);
                    result = Expression.Block(_variables, _expressions);
                }

                relationalCommandCache = _nested
                    ? null
                    : new RelationalCommandCache(
                        _parentVisitor.Dependencies.MemoryCache,
                        _parentVisitor.RelationalDependencies.QuerySqlGeneratorFactory,
                        _parentVisitor.RelationalDependencies.RelationalParameterBasedSqlProcessorFactory,
                        _selectExpression,
                        _readerColumns,
                        _parentVisitor._useRelationalNulls);

                return _indexMapParameter != null
                    ? Expression.Lambda(
                        result,
                        QueryCompilationContext.QueryContextParameter,
                        _dataReaderParameter,
                        _indexMapParameter)
                    : Expression.Lambda(
                        result,
                        QueryCompilationContext.QueryContextParameter,
                        _dataReaderParameter,
                        _resultContextParameter,
                        _resultCoordinatorParameter);
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

                    _materializationContextBindings[parameterExpression]
                        = (IDictionary<IProperty, int>)GetProjectionIndex(projectionBindingExpression);

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
                    case EntityShaperExpression entityShaperExpression:
                    {
                        var key = GenerateKey((ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression);
                        if (!_variableShaperMapping.TryGetValue(key, out var accessor))
                        {
                            var entityParameter = Expression.Parameter(entityShaperExpression.Type);
                            _variables.Add(entityParameter);
                            var entityMaterializationExpression = Visit(_parentVisitor.InjectEntityMaterializers(entityShaperExpression));

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

                        var valueParameter = Expression.Parameter(projectionBindingExpression.Type);
                        _variables.Add(valueParameter);

                        var projectionIndex = (int)GetProjectionIndex(projectionBindingExpression);
                        var projection = _selectExpression.Projection[projectionIndex];

                        _expressions.Add(Expression.Assign(valueParameter,
                            CreateGetValueExpression(
                                _dataReaderParameter,
                                projectionIndex,
                                IsNullableProjection(projection),
                                projection.Expression.TypeMapping,
                                projectionBindingExpression.Type)));

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
                            var innerShaper = new ShaperProcessingExpressionVisitor(_parentVisitor, _selectExpression, _dataReaderParameter,
                                _resultContextParameter, _resultCoordinatorParameter, null, _readerColumns, true)
                                .ProcessShaper(relationalCollectionShaperExpression.InnerShaper, out _);

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

                            _includeExpressions.Add(Expression.Call(
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
                                Expression.Constant(_parentVisitor.QueryCompilationContext.IsTracking)));

                            var relatedEntityType = innerShaper.ReturnType;
                            var inverseNavigation = navigation.Inverse;

                            _collectionPopulatingExpressions.Add(Expression.Call(
                                _populateIncludeCollectionMethodInfo.MakeGenericMethod(includingEntityType, relatedEntityType),
                                collectionIdConstant,
                                QueryCompilationContext.QueryContextParameter,
                                _dataReaderParameter,
                                _resultCoordinatorParameter,
                                Expression.Constant(parentIdentifierLambda.Compile()),
                                Expression.Constant(outerIdentifierLambda.Compile()),
                                Expression.Constant(selfIdentifierLambda.Compile()),
                                Expression.Constant(relationalCollectionShaperExpression.ParentIdentifierValueComparers, typeof(IReadOnlyList<ValueComparer>)),
                                Expression.Constant(relationalCollectionShaperExpression.OuterIdentifierValueComparers, typeof(IReadOnlyList<ValueComparer>)),
                                Expression.Constant(relationalCollectionShaperExpression.SelfIdentifierValueComparers, typeof(IReadOnlyList<ValueComparer>)),
                                Expression.Constant(innerShaper.Compile()),
                                Expression.Constant(inverseNavigation, typeof(INavigation)),
                                Expression.Constant(
                                    GenerateFixup(
                                        includingEntityType, relatedEntityType, navigation, inverseNavigation).Compile()),
                                Expression.Constant(_parentVisitor.QueryCompilationContext.IsTracking)));
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
                                Expression.Constant(inverseNavigation, typeof(INavigation)),
                                Expression.Constant(
                                    GenerateFixup(
                                        includingType, relatedEntityType, navigation, inverseNavigation).Compile()),
                                Expression.Constant(_parentVisitor.QueryCompilationContext.IsTracking));

                            _includeExpressions.Add(updatedExpression);
                        }

                        return entity;
                    }

                    case RelationalCollectionShaperExpression relationalCollectionShaperExpression:
                    {
                        var key = GenerateKey(relationalCollectionShaperExpression);
                        if (!_variableShaperMapping.TryGetValue(key, out var accessor))
                        {
                            var innerShaper = new ShaperProcessingExpressionVisitor(_parentVisitor, _selectExpression, _dataReaderParameter,
                                _resultContextParameter, _resultCoordinatorParameter, null, _readerColumns, true)
                                .ProcessShaper(relationalCollectionShaperExpression.InnerShaper, out _);

                            var collectionType = relationalCollectionShaperExpression.Type;
                            var elementType = collectionType.TryGetSequenceType();
                            var relatedElementType = innerShaper.ReturnType;
                            var navigation = relationalCollectionShaperExpression.Navigation;

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
                                        Expression.Constant(navigation?.GetCollectionAccessor(), typeof(IClrCollectionAccessor)))));

                            _valuesArrayInitializers.Add(collectionParameter);
                            accessor = Expression.Convert(
                                Expression.ArrayIndex(
                                    _valuesArrayExpression,
                                    Expression.Constant(_valuesArrayInitializers.Count - 1)),
                                relationalCollectionShaperExpression.Type);

                            _collectionPopulatingExpressions.Add(Expression.Call(
                                _populateCollectionMethodInfo.MakeGenericMethod(collectionType, elementType, relatedElementType),
                                collectionIdConstant,
                                QueryCompilationContext.QueryContextParameter,
                                _dataReaderParameter,
                                _resultCoordinatorParameter,
                                Expression.Constant(parentIdentifierLambda.Compile()),
                                Expression.Constant(outerIdentifierLambda.Compile()),
                                Expression.Constant(selfIdentifierLambda.Compile()),
                                Expression.Constant(relationalCollectionShaperExpression.ParentIdentifierValueComparers, typeof(IReadOnlyList<ValueComparer>)),
                                Expression.Constant(relationalCollectionShaperExpression.OuterIdentifierValueComparers, typeof(IReadOnlyList<ValueComparer>)),
                                Expression.Constant(relationalCollectionShaperExpression.SelfIdentifierValueComparers, typeof(IReadOnlyList<ValueComparer>)),
                                Expression.Constant(innerShaper.Compile())));

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
                    && methodCallExpression.Method.GetGenericMethodDefinition() == Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod)
                {
                    var property = (IProperty)((ConstantExpression)methodCallExpression.Arguments[2]).Value;
                    var propertyProjectionMap = methodCallExpression.Arguments[0] is ProjectionBindingExpression projectionBindingExpression
                        ? (IDictionary<IProperty, int>)GetProjectionIndex(projectionBindingExpression)
                        : _materializationContextBindings[
                            (ParameterExpression)((MethodCallExpression)methodCallExpression.Arguments[0]).Object];

                    var projectionIndex = propertyProjectionMap[property];
                    var projection = _selectExpression.Projection[projectionIndex];

                    return CreateGetValueExpression(
                        _dataReaderParameter,
                        projectionIndex,
                        IsNullableProjection(projection),
                        property.GetRelationalTypeMapping(),
                        methodCallExpression.Type,
                        property);
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            private static LambdaExpression GenerateFixup(
                Type entityType,
                Type relatedEntityType,
                INavigation navigation,
                INavigation inverseNavigation)
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
                INavigation navigation)
                => entity.MakeMemberAccess(navigation.GetMemberInfo(forMaterialization: true, forSet: true)).Assign(relatedEntity);

            private static Expression AddToCollectionNavigation(
                ParameterExpression entity,
                ParameterExpression relatedEntity,
                INavigation navigation)
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
                Type clrType,
                IPropertyBase property = null)
            {
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

                if (_readerColumns != null)
                {
                    var columnType = valueExpression.Type;
                    if (!columnType.IsValueType
                        || !BufferedDataReader.IsSupportedValueType(columnType))
                    {
                        columnType = typeof(object);
                        valueExpression = Expression.Convert(valueExpression, typeof(object));
                    }

                    if (_readerColumns[index] == null)
                    {
                        _readerColumns[index] = ReaderColumn.Create(
                            columnType,
                            nullable,
                            _indexMapParameter != null ? ((ColumnExpression)_selectExpression.Projection[index].Expression).Name : null,
                            Expression.Lambda(
                                valueExpression,
                                dbDataReader,
                                _indexMapParameter ?? Expression.Parameter(typeof(int[]))).Compile());
                    }

                    if (getMethod.DeclaringType != typeof(DbDataReader))
                    {
                        valueExpression
                            = Expression.Call(
                                dbDataReader,
                                RelationalTypeMapping.GetDataReaderMethod(columnType),
                                indexExpression);
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

                if (valueExpression.Type != clrType)
                {
                    valueExpression = Expression.Convert(valueExpression, clrType);
                }

                var exceptionParameter
                    = Expression.Parameter(typeof(Exception), name: "e");

                if (_detailedErrorsEnabled)
                {
                    var catchBlock
                        = Expression
                            .Catch(
                                exceptionParameter,
                                Expression.Call(
                                    _throwReadValueExceptionMethod
                                        .MakeGenericMethod(valueExpression.Type),
                                    exceptionParameter,
                                    Expression.Call(
                                        dbDataReader,
                                        _getFieldValueMethod.MakeGenericMethod(typeof(object)),
                                        indexExpression),
                                    Expression.Constant(property, typeof(IPropertyBase))));

                    valueExpression = Expression.TryCatch(valueExpression, catchBlock);
                }

                if (nullable)
                {
                    valueExpression
                        = Expression.Condition(
                            Expression.Call(dbDataReader, _isDbNullMethod, indexExpression),
                            Expression.Default(valueExpression.Type),
                            valueExpression);
                }

                return valueExpression;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static TValue ThrowReadValueException<TValue>(
                Exception exception, object value, IPropertyBase property = null)
            {
                var expectedType = typeof(TValue);
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
                        ? CoreStrings.ErrorMaterializingPropertyNullReference(entityType, propertyName, expectedType)
                        : exception is InvalidCastException
                            ? CoreStrings.ErrorMaterializingPropertyInvalidCast(entityType, propertyName, expectedType, actualType)
                            : CoreStrings.ErrorMaterializingProperty(entityType, propertyName);
                }
                else
                {
                    message = exception is NullReferenceException
                        || Equals(value, DBNull.Value)
                        ? CoreStrings.ErrorMaterializingValueNullReference(expectedType)
                        : exception is InvalidCastException
                            ? CoreStrings.ErrorMaterializingValueInvalidCast(expectedType, actualType)
                            : CoreStrings.ErrorMaterializingValue;
                }

                throw new InvalidOperationException(message, exception);
            }

            private static void IncludeReference<TEntity, TIncludingEntity, TIncludedEntity>(
                QueryContext queryContext,
                TEntity entity,
                TIncludedEntity relatedEntity,
                INavigation navigation,
                INavigation inverseNavigation,
                Action<TIncludingEntity, TIncludedEntity> fixup,
                bool trackingQuery)
                where TIncludingEntity : TEntity
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
                        SetIsLoadedNoTracking(includingEntity, navigation);
                        if (relatedEntity != null)
                        {
                            fixup(includingEntity, relatedEntity);
                            if (inverseNavigation != null
                                && !inverseNavigation.IsCollection)
                            {
                                SetIsLoadedNoTracking(relatedEntity, inverseNavigation);
                            }
                        }
                    }
                }
            }

            private static void InitializeIncludeCollection<TParent, TNavigationEntity>(
                int collectionId,
                QueryContext queryContext,
                DbDataReader dbDataReader,
                ResultCoordinator resultCoordinator,
                TParent entity,
                Func<QueryContext, DbDataReader, object[]> parentIdentifier,
                Func<QueryContext, DbDataReader, object[]> outerIdentifier,
                INavigation navigation,
                IClrCollectionAccessor clrCollectionAccessor,
                bool trackingQuery)
                where TNavigationEntity : TParent
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
                        SetIsLoadedNoTracking(entity, navigation);
                    }

                    collection = clrCollectionAccessor.GetOrCreate(entity, forMaterialization: true);
                }

                var parentKey = parentIdentifier(queryContext, dbDataReader);
                var outerKey = outerIdentifier(queryContext, dbDataReader);

                var collectionMaterializationContext = new CollectionMaterializationContext(entity, collection, parentKey, outerKey);

                resultCoordinator.SetCollectionMaterializationContext(collectionId, collectionMaterializationContext);
            }

            private static void PopulateIncludeCollection<TIncludingEntity, TIncludedEntity>(
                int collectionId,
                QueryContext queryContext,
                DbDataReader dbDataReader,
                ResultCoordinator resultCoordinator,
                Func<QueryContext, DbDataReader, object[]> parentIdentifier,
                Func<QueryContext, DbDataReader, object[]> outerIdentifier,
                Func<QueryContext, DbDataReader, object[]> selfIdentifier,
                IReadOnlyList<ValueComparer> parentIdentifierValueComparers,
                IReadOnlyList<ValueComparer> outerIdentifierValueComparers,
                IReadOnlyList<ValueComparer> selfIdentifierValueComparers,
                Func<QueryContext, DbDataReader, ResultContext, ResultCoordinator, TIncludedEntity> innerShaper,
                INavigation inverseNavigation,
                Action<TIncludingEntity, TIncludedEntity> fixup,
                bool trackingQuery)
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

                    if (!CompareIdentifiers(outerIdentifierValueComparers,
                        outerIdentifier(queryContext, dbDataReader), collectionMaterializationContext.OuterIdentifier))
                    {
                        // Outer changed so collection has ended. Materialize last element.
                        GenerateCurrentElementIfPending();
                        // If parent also changed then this row is now pointing to element of next collection
                        if (!CompareIdentifiers(parentIdentifierValueComparers,
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
                                SetIsLoadedNoTracking(relatedEntity, inverseNavigation);
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

            private static TCollection InitializeCollection<TElement, TCollection>(
                int collectionId,
                QueryContext queryContext,
                DbDataReader dbDataReader,
                ResultCoordinator resultCoordinator,
                Func<QueryContext, DbDataReader, object[]> parentIdentifier,
                Func<QueryContext, DbDataReader, object[]> outerIdentifier,
                IClrCollectionAccessor clrCollectionAccessor)
                where TCollection : class, IEnumerable<TElement>
            {
                var collection = clrCollectionAccessor?.Create() ?? new List<TElement>();

                var parentKey = parentIdentifier(queryContext, dbDataReader);
                var outerKey = outerIdentifier(queryContext, dbDataReader);

                var collectionMaterializationContext = new CollectionMaterializationContext(null, collection, parentKey, outerKey);

                resultCoordinator.SetCollectionMaterializationContext(collectionId, collectionMaterializationContext);

                return (TCollection)collection;
            }

            private static void PopulateCollection<TCollection, TElement, TRelatedEntity>(
                int collectionId,
                QueryContext queryContext,
                DbDataReader dbDataReader,
                ResultCoordinator resultCoordinator,
                Func<QueryContext, DbDataReader, object[]> parentIdentifier,
                Func<QueryContext, DbDataReader, object[]> outerIdentifier,
                Func<QueryContext, DbDataReader, object[]> selfIdentifier,
                IReadOnlyList<ValueComparer> parentIdentifierValueComparers,
                IReadOnlyList<ValueComparer> outerIdentifierValueComparers,
                IReadOnlyList<ValueComparer> selfIdentifierValueComparers,
                Func<QueryContext, DbDataReader, ResultContext, ResultCoordinator, TRelatedEntity> innerShaper)
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

                if (!CompareIdentifiers(outerIdentifierValueComparers,
                    outerIdentifier(queryContext, dbDataReader), collectionMaterializationContext.OuterIdentifier))
                {
                    // Outer changed so collection has ended. Materialize last element.
                    GenerateCurrentElementIfPending();
                    // If parent also changed then this row is now pointing to element of next collection
                    if (!CompareIdentifiers(parentIdentifierValueComparers,
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
                    if (CompareIdentifiers(selfIdentifierValueComparers,
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

            private static void SetIsLoadedNoTracking(object entity, INavigation navigation)
                => ((ILazyLoader)navigation
                            .DeclaringEntityType
                            .GetServiceProperties()
                            .FirstOrDefault(p => p.ClrType == typeof(ILazyLoader))
                        ?.GetGetter().GetClrValue(entity))
                    ?.SetLoaded(entity, navigation.Name);

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

                    if (expression is RelationalCollectionShaperExpression)
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
