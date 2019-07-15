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
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.NavigationExpansion;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
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
                    if (trackingQuery)
                    {
                        // For non-null relatedEntity StateManager will set the flag
                        if (relatedEntity == null)
                        {
                            queryContext.StateManager.TryGetEntry(includingEntity).SetIsLoaded(navigation);
                        }
                    }
                    else
                    {
                        SetIsLoadedNoTracking(includingEntity, navigation);
                        if (relatedEntity is object)
                        {
                            fixup(includingEntity, relatedEntity);
                            if (inverseNavigation != null && !inverseNavigation.IsCollection())
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
                Func<QueryContext, DbDataReader, TRelatedEntity, ResultCoordinator, TRelatedEntity> innerShaper)
                where TRelatedEntity : TElement
                where TCollection : class, ICollection<TElement>
            {
                var collectionMaterializationContext = resultCoordinator.Collections[collectionId];

                if (collectionMaterializationContext.Collection is null)
                {
                    // nothing to include since no collection created
                    return;
                }

                if (!StructuralComparisons.StructuralEqualityComparer.Equals(
                    outerIdentifier(queryContext, dbDataReader), collectionMaterializationContext.OuterIdentifier))
                {
                    if (StructuralComparisons.StructuralEqualityComparer.Equals(
                        parentIdentifier(queryContext, dbDataReader), collectionMaterializationContext.ParentIdentifier))
                    {
                        resultCoordinator.ResultReady = false;
                    }
                    else
                    {
                        resultCoordinator.HasNext = true;
                    }

                    return;
                }

                var innerKey = selfIdentifier(queryContext, dbDataReader);
                if (innerKey.Any(e => e == null))
                {
                    // If innerKey was null then return since no related data
                    return;
                }

                if (StructuralComparisons.StructuralEqualityComparer.Equals(
                        innerKey, collectionMaterializationContext.SelfIdentifier))
                {
                    // We don't need to materialize this entity but we may need to populate inner collections if any.
                    innerShaper(queryContext, dbDataReader, (TRelatedEntity)collectionMaterializationContext.Current, resultCoordinator);

                    return;
                }

                var relatedEntity = innerShaper(queryContext, dbDataReader, default, resultCoordinator);
                collectionMaterializationContext.UpdateCurrent(relatedEntity, innerKey);

                ((TCollection)collectionMaterializationContext.Collection).Add(relatedEntity);

                resultCoordinator.ResultReady = false;
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
                Func<QueryContext, DbDataReader, TIncludedEntity, ResultCoordinator, TIncludedEntity> innerShaper,
                INavigation inverseNavigation,
                Action<TIncludingEntity, TIncludedEntity> fixup,
                bool trackingQuery)
            {
                var collectionMaterializationContext = resultCoordinator.Collections[collectionId];
                var parentEntity = collectionMaterializationContext.Parent;

                if (parentEntity is TIncludingEntity entity)
                {
                    if (!StructuralComparisons.StructuralEqualityComparer.Equals(
                        outerIdentifier(queryContext, dbDataReader), collectionMaterializationContext.OuterIdentifier))
                    {
                        if (StructuralComparisons.StructuralEqualityComparer.Equals(
                            parentIdentifier(queryContext, dbDataReader), collectionMaterializationContext.ParentIdentifier))
                        {
                            resultCoordinator.ResultReady = false;
                        }
                        else
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

                    if (StructuralComparisons.StructuralEqualityComparer.Equals(
                            innerKey, collectionMaterializationContext.SelfIdentifier))
                    {
                        // We don't need to materialize this entity but we may need to populate inner collections if any.
                        innerShaper(queryContext, dbDataReader, (TIncludedEntity)collectionMaterializationContext.Current, resultCoordinator);

                        return;
                    }

                    var relatedEntity = innerShaper(queryContext, dbDataReader, default, resultCoordinator);
                    collectionMaterializationContext.UpdateCurrent(relatedEntity, innerKey);

                    if (!trackingQuery)
                    {
                        fixup(entity, relatedEntity);
                        if (inverseNavigation != null && !inverseNavigation.IsCollection())
                        {
                            SetIsLoadedNoTracking(relatedEntity, inverseNavigation);
                        }
                    }

                    resultCoordinator.ResultReady = false;
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
                        queryContext.StateManager.TryGetEntry(entity).SetIsLoaded(navigation);
                    }
                    else
                    {
                        SetIsLoadedNoTracking(entity, navigation);
                    }

                    collection = clrCollectionAccessor.GetOrCreate(entity);
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
                where TCollection :class, IEnumerable<TElement>
            {
                var collection = clrCollectionAccessor?.Create() ?? new List<TElement>();

                var parentKey = parentIdentifier(queryContext, dbDataReader);
                var outerKey = outerIdentifier(queryContext, dbDataReader);

                var collectionMaterializationContext = new CollectionMaterializationContext(null, collection, parentKey, outerKey);

                resultCoordinator.SetCollectionMaterializationContext(collectionId, collectionMaterializationContext);

                return (TCollection)collection;
            }

            private static void SetIsLoadedNoTracking(object entity, INavigation navigation)
                => ((ILazyLoader)((PropertyBase)navigation
                            .DeclaringEntityType
                            .GetServiceProperties()
                            .FirstOrDefault(p => p.ClrType == typeof(ILazyLoader)))
                        ?.Getter.GetClrValue(entity))
                    ?.SetLoaded(entity, navigation.Name);

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                if (extensionExpression is IncludeExpression includeExpression)
                {
                    Debug.Assert(!includeExpression.Navigation.IsCollection(),
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
                            GenerateFixup(includingClrType, relatedEntityClrType, includeExpression.Navigation, inverseNavigation).Compile()),
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
                        Expression.Constant(collectionInitializingExpression.Navigation?.GetCollectionAccessor(),
                            typeof(IClrCollectionAccessor)));
                }

                if (extensionExpression is CollectionPopulatingExpression collectionPopulatingExpression)
                {
                    var collectionShaper = collectionPopulatingExpression.Parent;
                    var relatedEntityClrType = ((LambdaExpression)collectionShaper.InnerShaper).ReturnType;

                    if (collectionPopulatingExpression.Include)
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
                                GenerateFixup(entityClrType, relatedEntityClrType, collectionShaper.Navigation, inverseNavigation).Compile()),
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
                return entity.MakeMemberAccess(navigation.GetMemberInfo(forConstruction: false, forSet: true))
                    .CreateAssignExpression(relatedEntity);
            }

            private static Expression AddToCollectionNavigation(
                ParameterExpression entity,
                ParameterExpression relatedEntity,
                INavigation navigation)
            {
                return Expression.Call(
                    Expression.Constant(navigation.GetCollectionAccessor()),
                    _collectionAccessorAddMethodInfo,
                    entity,
                    relatedEntity);
            }

            private static readonly MethodInfo _collectionAccessorAddMethodInfo
                = typeof(IClrCollectionAccessor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IClrCollectionAccessor.Add));
        }
    }
}
