// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class RelationalShapedQueryCompilingExpressionVisitor
    {
        private class CustomShaperCompilingExpressionVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _dbDataReaderParameter;
            private readonly ParameterExpression _resultCoordinatorParameter;
            private readonly bool _tracking;

            public CustomShaperCompilingExpressionVisitor(
                ParameterExpression dbDataReaderParameter,
                ParameterExpression resultCoordinatorParameter,
                bool tracking)
            {
                _dbDataReaderParameter = dbDataReaderParameter;
                _resultCoordinatorParameter = resultCoordinatorParameter;
                _tracking = tracking;
            }

            private static readonly MethodInfo _includeReferenceMethodInfo
                = typeof(CustomShaperCompilingExpressionVisitor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IncludeReference));

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
                                && !inverseNavigation.IsCollection())
                            {
                                SetIsLoadedNoTracking(relatedEntity, inverseNavigation);
                            }
                        }
                    }
                }
            }

            private static readonly MethodInfo _populateCollectionMethodInfo
                = typeof(CustomShaperCompilingExpressionVisitor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(PopulateCollection));

            private static void PopulateCollection<TCollection, TElement, TRelatedEntity>(
                int collectionId,
                QueryContext queryContext,
                DbDataReader dbDataReader,
                ResultCoordinator resultCoordinator,
                Func<QueryContext, DbDataReader, object[]> parentIdentifier,
                Func<QueryContext, DbDataReader, object[]> outerIdentifier,
                Func<QueryContext, DbDataReader, object[]> selfIdentifier,
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

                if (!StructuralComparisons.StructuralEqualityComparer.Equals(
                    outerIdentifier(queryContext, dbDataReader), collectionMaterializationContext.OuterIdentifier))
                {
                    // Outer changed so collection has ended. Materialize last element.
                    GenerateCurrentElementIfPending();
                    // If parent also changed then this row is now pointing to element of next collection
                    if (!StructuralComparisons.StructuralEqualityComparer.Equals(
                        parentIdentifier(queryContext, dbDataReader), collectionMaterializationContext.ParentIdentifier))
                    {
                        resultCoordinator.HasNext = true;
                    }

                    return;
                }

                var innerKey = selfIdentifier(queryContext, dbDataReader);
                if (innerKey.Any(e => e == null))
                {
                    // No correlated element
                    return;
                }

                if (collectionMaterializationContext.SelfIdentifier != null)
                {
                    if (StructuralComparisons.StructuralEqualityComparer.Equals(
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

            private static readonly MethodInfo _populateIncludeCollectionMethodInfo
                = typeof(CustomShaperCompilingExpressionVisitor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(PopulateIncludeCollection));

            private static void PopulateIncludeCollection<TIncludingEntity, TIncludedEntity>(
                int collectionId,
                QueryContext queryContext,
                DbDataReader dbDataReader,
                ResultCoordinator resultCoordinator,
                Func<QueryContext, DbDataReader, object[]> parentIdentifier,
                Func<QueryContext, DbDataReader, object[]> outerIdentifier,
                Func<QueryContext, DbDataReader, object[]> selfIdentifier,
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

                    if (!StructuralComparisons.StructuralEqualityComparer.Equals(
                        outerIdentifier(queryContext, dbDataReader), collectionMaterializationContext.OuterIdentifier))
                    {
                        // Outer changed so collection has ended. Materialize last element.
                        GenerateCurrentElementIfPending();
                        // If parent also changed then this row is now pointing to element of next collection
                        if (!StructuralComparisons.StructuralEqualityComparer.Equals(
                            parentIdentifier(queryContext, dbDataReader), collectionMaterializationContext.ParentIdentifier))
                        {
                            resultCoordinator.HasNext = true;
                        }

                        return;
                    }

                    var innerKey = selfIdentifier(queryContext, dbDataReader);
                    if (innerKey.Any(e => e == null))
                    {
                        // No correlated element
                        return;
                    }

                    if (collectionMaterializationContext.SelfIdentifier != null)
                    {
                        if (StructuralComparisons.StructuralEqualityComparer.Equals(
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

            private static readonly MethodInfo _initializeIncludeCollectionMethodInfo
                = typeof(CustomShaperCompilingExpressionVisitor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(InitializeIncludeCollection));

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

            private static readonly MethodInfo _initializeCollectionMethodInfo
                = typeof(CustomShaperCompilingExpressionVisitor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(InitializeCollection));

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

            private static void SetIsLoadedNoTracking(object entity, INavigation navigation)
                => ((ILazyLoader)(navigation
                            .DeclaringEntityType
                            .GetServiceProperties()
                            .FirstOrDefault(p => p.ClrType == typeof(ILazyLoader)))
                        ?.GetGetter().GetClrValue(entity))
                    ?.SetLoaded(entity, navigation.Name);

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                if (extensionExpression is IncludeExpression includeExpression)
                {
                    Debug.Assert(
                        !includeExpression.Navigation.IsCollection(),
                        "Only reference include should be present in tree");
                    var entityClrType = includeExpression.EntityExpression.Type;
                    var includingClrType = includeExpression.Navigation.DeclaringEntityType.ClrType;
                    var inverseNavigation = includeExpression.Navigation.FindInverse();
                    var relatedEntityClrType = includeExpression.Navigation.GetTargetType().ClrType;
                    if (includingClrType != entityClrType
                        && includingClrType.IsAssignableFrom(entityClrType))
                    {
                        includingClrType = entityClrType;
                    }

                    return Expression.Call(
                        _includeReferenceMethodInfo.MakeGenericMethod(entityClrType, includingClrType, relatedEntityClrType),
                        QueryCompilationContext.QueryContextParameter,
                        // We don't need to visit entityExpression since it is supposed to be a parameterExpression only
                        includeExpression.EntityExpression,
                        includeExpression.NavigationExpression,
                        Expression.Constant(includeExpression.Navigation),
                        Expression.Constant(inverseNavigation, typeof(INavigation)),
                        Expression.Constant(
                            GenerateFixup(
                                includingClrType, relatedEntityClrType, includeExpression.Navigation, inverseNavigation).Compile()),
                        Expression.Constant(_tracking));
                }

                if (extensionExpression is CollectionInitializingExpression collectionInitializingExpression)
                {
                    if (collectionInitializingExpression.Parent != null)
                    {
                        // Include case
                        var entityClrType = collectionInitializingExpression.Parent.Type;
                        var includingClrType = collectionInitializingExpression.Navigation.DeclaringEntityType.ClrType;
                        if (includingClrType != entityClrType
                            && includingClrType.IsAssignableFrom(entityClrType))
                        {
                            includingClrType = entityClrType;
                        }

                        return Expression.Call(
                            _initializeIncludeCollectionMethodInfo.MakeGenericMethod(entityClrType, includingClrType),
                            Expression.Constant(collectionInitializingExpression.CollectionId),
                            QueryCompilationContext.QueryContextParameter,
                            _dbDataReaderParameter,
                            _resultCoordinatorParameter,
                            collectionInitializingExpression.Parent,
                            Expression.Constant(
                                Expression.Lambda(
                                    collectionInitializingExpression.ParentIdentifier,
                                    QueryCompilationContext.QueryContextParameter,
                                    _dbDataReaderParameter).Compile()),
                            Expression.Constant(
                                Expression.Lambda(
                                    collectionInitializingExpression.OuterIdentifier,
                                    QueryCompilationContext.QueryContextParameter,
                                    _dbDataReaderParameter).Compile()),
                            Expression.Constant(collectionInitializingExpression.Navigation),
                            Expression.Constant(collectionInitializingExpression.Navigation.GetCollectionAccessor()),
                            Expression.Constant(_tracking));
                    }

                    var collectionClrType = collectionInitializingExpression.Type;
                    var elementType = collectionClrType.TryGetSequenceType();

                    return Expression.Call(
                        _initializeCollectionMethodInfo.MakeGenericMethod(elementType, collectionClrType),
                        Expression.Constant(collectionInitializingExpression.CollectionId),
                        QueryCompilationContext.QueryContextParameter,
                        _dbDataReaderParameter,
                        _resultCoordinatorParameter,
                        Expression.Constant(
                            Expression.Lambda(
                                collectionInitializingExpression.ParentIdentifier,
                                QueryCompilationContext.QueryContextParameter,
                                _dbDataReaderParameter).Compile()),
                        Expression.Constant(
                            Expression.Lambda(
                                collectionInitializingExpression.OuterIdentifier,
                                QueryCompilationContext.QueryContextParameter,
                                _dbDataReaderParameter).Compile()),
                        Expression.Constant(
                            collectionInitializingExpression.Navigation?.GetCollectionAccessor(),
                            typeof(IClrCollectionAccessor)));
                }

                if (extensionExpression is CollectionPopulatingExpression collectionPopulatingExpression)
                {
                    var collectionShaper = collectionPopulatingExpression.Parent;
                    var relatedEntityClrType = ((LambdaExpression)collectionShaper.InnerShaper).ReturnType;

                    if (collectionPopulatingExpression.IsInclude)
                    {
                        var entityClrType = collectionShaper.Navigation.DeclaringEntityType.ClrType;
                        var inverseNavigation = collectionShaper.Navigation.FindInverse();

                        return Expression.Call(
                            _populateIncludeCollectionMethodInfo.MakeGenericMethod(entityClrType, relatedEntityClrType),
                            Expression.Constant(collectionShaper.CollectionId),
                            QueryCompilationContext.QueryContextParameter,
                            _dbDataReaderParameter,
                            _resultCoordinatorParameter,
                            Expression.Constant(
                                Expression.Lambda(
                                    collectionShaper.ParentIdentifier,
                                    QueryCompilationContext.QueryContextParameter,
                                    _dbDataReaderParameter).Compile()),
                            Expression.Constant(
                                Expression.Lambda(
                                    collectionShaper.OuterIdentifier,
                                    QueryCompilationContext.QueryContextParameter,
                                    _dbDataReaderParameter).Compile()),
                            Expression.Constant(
                                Expression.Lambda(
                                    collectionShaper.SelfIdentifier,
                                    QueryCompilationContext.QueryContextParameter,
                                    _dbDataReaderParameter).Compile()),
                            Expression.Constant(((LambdaExpression)Visit(collectionShaper.InnerShaper)).Compile()),
                            Expression.Constant(inverseNavigation, typeof(INavigation)),
                            Expression.Constant(
                                GenerateFixup(
                                    entityClrType, relatedEntityClrType, collectionShaper.Navigation, inverseNavigation).Compile()),
                            Expression.Constant(_tracking));
                    }

                    var collectionType = collectionPopulatingExpression.Type;
                    var elementType = collectionType.TryGetSequenceType();

                    return Expression.Call(
                        _populateCollectionMethodInfo.MakeGenericMethod(collectionType, elementType, relatedEntityClrType),
                        Expression.Constant(collectionShaper.CollectionId),
                        QueryCompilationContext.QueryContextParameter,
                        _dbDataReaderParameter,
                        _resultCoordinatorParameter,
                        Expression.Constant(
                            Expression.Lambda(
                                collectionShaper.ParentIdentifier,
                                QueryCompilationContext.QueryContextParameter,
                                _dbDataReaderParameter).Compile()),
                        Expression.Constant(
                            Expression.Lambda(
                                collectionShaper.OuterIdentifier,
                                QueryCompilationContext.QueryContextParameter,
                                _dbDataReaderParameter).Compile()),
                        Expression.Constant(
                            Expression.Lambda(
                                collectionShaper.SelfIdentifier,
                                QueryCompilationContext.QueryContextParameter,
                                _dbDataReaderParameter).Compile()),
                        Expression.Constant(((LambdaExpression)Visit(collectionShaper.InnerShaper)).Compile()));
                }

                if (extensionExpression is GroupByShaperExpression)
                {
                    throw new InvalidOperationException("Client side GroupBy is not supported.");
                }

                return base.VisitExtension(extensionExpression);
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
                    navigation.IsCollection()
                        ? AddToCollectionNavigation(entityParameter, relatedEntityParameter, navigation)
                        : AssignReferenceNavigation(entityParameter, relatedEntityParameter, navigation)
                };

                if (inverseNavigation != null)
                {
                    expressions.Add(
                        inverseNavigation.IsCollection()
                            ? AddToCollectionNavigation(relatedEntityParameter, entityParameter, inverseNavigation)
                            : AssignReferenceNavigation(relatedEntityParameter, entityParameter, inverseNavigation));
                }

                return Expression.Lambda(Expression.Block(typeof(void), expressions), entityParameter, relatedEntityParameter);
            }

            private static Expression AssignReferenceNavigation(
                ParameterExpression entity,
                ParameterExpression relatedEntity,
                INavigation navigation)
            {
                return entity.MakeMemberAccess(navigation.GetMemberInfo(forMaterialization: true, forSet: true)).Assign(relatedEntity);
            }

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

            private static readonly MethodInfo _collectionAccessorAddMethodInfo
                = typeof(IClrCollectionAccessor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IClrCollectionAccessor.Add));
        }
    }
}
