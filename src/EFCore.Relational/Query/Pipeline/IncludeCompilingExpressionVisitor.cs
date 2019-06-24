// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            private static void PopulateCollection<TIncludingEntity, TIncludedEntity>(
                int collectionId,
                QueryContext queryContext,
                DbDataReader dbDataReader,
                Func<QueryContext, DbDataReader, object[]> outerIdentifier,
                Func<QueryContext, DbDataReader, object[]> selfIdentifier,
                Func<QueryContext, DbDataReader, TIncludedEntity, ResultCoordinator, TIncludedEntity> innerShaper,
                INavigation inverseNavigation,
                Action<TIncludingEntity, TIncludedEntity> fixup,
                bool trackingQuery,
                ResultCoordinator resultCoordinator)
            {
                var collectionMaterializationContext = resultCoordinator.Collections[collectionId];

                var parent = collectionMaterializationContext.Parent;
                if (collectionMaterializationContext.Collection is null)
                {
                    // Nothing to include since parent was not materialized
                    return;
                }

                var entity = (TIncludingEntity)parent;

                var outerKey = outerIdentifier(queryContext, dbDataReader);
                if (!StructuralComparisons.StructuralEqualityComparer.Equals(
                        outerKey, collectionMaterializationContext.OuterIdentifier))
                {
                    resultCoordinator.HasNext = true;

                    return;
                }

                var innerKey = selfIdentifier(queryContext, dbDataReader);
                TIncludedEntity current;
                if (StructuralComparisons.StructuralEqualityComparer.Equals(
                        innerKey, collectionMaterializationContext.SelfIdentifier))
                {
                    current = (TIncludedEntity)collectionMaterializationContext.Current;
                }
                else
                {
                    current = default;
                    collectionMaterializationContext.SelfIdentifier = innerKey;
                }

                var relatedEntity = innerShaper(queryContext, dbDataReader, current, resultCoordinator);
                collectionMaterializationContext.UpdateCurrent(relatedEntity);
                if (relatedEntity is null)
                {
                    return;
                }

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

            private static readonly MethodInfo _initializeCollectionIncludeMethodInfo
                = typeof(CustomShaperCompilingExpressionVisitor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(InitializeCollectionInclude));

            private static void InitializeCollectionInclude<TEntity, TIncludingEntity>(
                int collectionId,
                QueryContext queryContext,
                DbDataReader dbDataReader,
                ResultCoordinator resultCoordinator,
                TEntity entity,
                Func<QueryContext, DbDataReader, object[]> outerIdentifier,
                INavigation navigation,
                IClrCollectionAccessor clrCollectionAccessor,
                bool trackingQuery)
                where TIncludingEntity : TEntity
            {
                object collection = null;
                if (entity is TIncludingEntity)
                {
                    // Include case
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

                var outerKey = outerIdentifier(queryContext, dbDataReader);

                var collectionMaterializationContext = new CollectionMaterializationContext(entity, collection, outerKey);
                if (resultCoordinator.Collections.Count == collectionId)
                {
                    resultCoordinator.Collections.Add(collectionMaterializationContext);
                }
                else
                {
                    resultCoordinator.Collections[collectionId] = collectionMaterializationContext;
                }
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
                    if (includingClrType.IsAssignableFrom(entityClrType))
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

                if (extensionExpression is CollectionInitializingExperssion collectionInitializingExperssion)
                {
                    var entityClrType = collectionInitializingExperssion.Parent.Type;
                    var includingClrType = collectionInitializingExperssion.Navigation.DeclaringEntityType.ClrType;
                    if (includingClrType.IsAssignableFrom(entityClrType))
                    {
                        includingClrType = entityClrType;
                    }

                    return Expression.Call(
                        _initializeCollectionIncludeMethodInfo.MakeGenericMethod(entityClrType, includingClrType),
                        Expression.Constant(collectionInitializingExperssion.CollectionId),
                        QueryCompilationContext.QueryContextParameter,
                        _dbDataReaderParameter,
                        _resultCoordinatorParameter,
                        collectionInitializingExperssion.Parent,
                        Expression.Constant(
                            Expression.Lambda(
                                collectionInitializingExperssion.OuterIdentifier,
                                QueryCompilationContext.QueryContextParameter,
                                _dbDataReaderParameter).Compile()),
                        Expression.Constant(collectionInitializingExperssion.Navigation),
                        Expression.Constant(collectionInitializingExperssion.Navigation.GetCollectionAccessor()),
                        Expression.Constant(_tracking));
                }

                if (extensionExpression is CollectionPopulatingExpression collectionPopulatingExpression)
                {
                    var collectionShaper = collectionPopulatingExpression.Parent;
                    var entityClrType = collectionShaper.Navigation.DeclaringEntityType.ClrType;
                    var relatedEntityClrType = collectionShaper.Navigation.GetTargetType().ClrType;
                    var inverseNavigation = collectionShaper.Navigation.FindInverse();
                    var innerShaper = Visit(collectionShaper.InnerShaper);
                    innerShaper = Expression.Constant(((LambdaExpression)innerShaper).Compile());

                    return Expression.Call(
                        _populateCollectionMethodInfo.MakeGenericMethod(entityClrType, relatedEntityClrType),
                        Expression.Constant(collectionShaper.CollectionId),
                        QueryCompilationContext.QueryContextParameter,
                        _dbDataReaderParameter,
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
                        innerShaper,
                        Expression.Constant(inverseNavigation, typeof(INavigation)),
                        Expression.Constant(
                            GenerateFixup(entityClrType, relatedEntityClrType, collectionShaper.Navigation, inverseNavigation).Compile()),
                        Expression.Constant(_tracking),
                        _resultCoordinatorParameter);
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
