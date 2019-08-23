// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public partial class InMemoryShapedQueryCompilingExpressionVisitor
    {
        private class CustomShaperCompilingExpressionVisitor : ExpressionVisitor
        {
            private readonly bool _tracking;

            public CustomShaperCompilingExpressionVisitor(bool tracking)
            {
                _tracking = tracking;
            }

            private static readonly MethodInfo _includeReferenceMethodInfo
                = typeof(CustomShaperCompilingExpressionVisitor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IncludeReference));

            private static readonly MethodInfo _includeCollectionMethodInfo
                = typeof(CustomShaperCompilingExpressionVisitor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IncludeCollection));

            private static readonly MethodInfo _materializeCollectionMethodInfo
                = typeof(CustomShaperCompilingExpressionVisitor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(MaterializeCollection));

            private static readonly MethodInfo _materializeSingleResultMethodInfo
                = typeof(CustomShaperCompilingExpressionVisitor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(MaterializeSingleResult));

            private static void SetIsLoadedNoTracking(object entity, INavigation navigation)
                => ((ILazyLoader)(navigation
                            .DeclaringEntityType
                            .GetServiceProperties()
                            .FirstOrDefault(p => p.ClrType == typeof(ILazyLoader)))
                        ?.GetGetter().GetClrValue(entity))
                    ?.SetLoaded(entity, navigation.Name);

            private static void IncludeReference<TEntity, TIncludingEntity, TIncludedEntity>(
                QueryContext queryContext,
                TEntity entity,
                TIncludedEntity relatedEntity,
                INavigation navigation,
                INavigation inverseNavigation,
                Action<TIncludingEntity, TIncludedEntity> fixup,
                bool trackingQuery)
                where TIncludingEntity : class, TEntity
                where TEntity : class
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

            private static void IncludeCollection<TEntity, TIncludingEntity, TIncludedEntity>(
                QueryContext queryContext,
                IEnumerable<ValueBuffer> innerValueBuffers,
                Func<QueryContext, ValueBuffer, TIncludedEntity> innerShaper,
                TEntity entity,
                INavigation navigation,
                INavigation inverseNavigation,
                Action<TIncludingEntity, TIncludedEntity> fixup,
                bool trackingQuery)
                where TIncludingEntity : class, TEntity
                where TEntity : class
                where TIncludedEntity : class
            {
                if (entity is TIncludingEntity includingEntity)
                {
                    var collectionAccessor = navigation.GetCollectionAccessor();
                    collectionAccessor.GetOrCreate(includingEntity, forMaterialization: true);

                    if (trackingQuery)
                    {
                        queryContext.SetNavigationIsLoaded(entity, navigation);
                    }
                    else
                    {
                        SetIsLoadedNoTracking(entity, navigation);
                    }

                    foreach (var valueBuffer in innerValueBuffers)
                    {
                        var relatedEntity = innerShaper(queryContext, valueBuffer);

                        if (!trackingQuery)
                        {
                            fixup(includingEntity, relatedEntity);
                            if (inverseNavigation != null)
                            {
                                SetIsLoadedNoTracking(relatedEntity, inverseNavigation);
                            }
                        }
                    }
                }
            }

            private static TCollection MaterializeCollection<TElement, TCollection>(
                QueryContext queryContext,
                IEnumerable<ValueBuffer> innerValueBuffers,
                Func<QueryContext, ValueBuffer, TElement> innerShaper,
                IClrCollectionAccessor clrCollectionAccessor)
                where TCollection : class, ICollection<TElement>
            {
                var collection = (TCollection)(clrCollectionAccessor?.Create() ?? new List<TElement>());

                foreach (var valueBuffer in innerValueBuffers)
                {
                    var element = innerShaper(queryContext, valueBuffer);
                    collection.Add(element);
                }

                return collection;
            }

            private static TResult MaterializeSingleResult<TResult>(
                QueryContext queryContext,
                ValueBuffer valueBuffer,
                Func<QueryContext, ValueBuffer, TResult> innerShaper)
                => valueBuffer.IsEmpty
                    ? default
                    : innerShaper(queryContext, valueBuffer);

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                if (extensionExpression is IncludeExpression includeExpression)
                {
                    var entityClrType = includeExpression.EntityExpression.Type;
                    var includingClrType = includeExpression.Navigation.DeclaringEntityType.ClrType;
                    var inverseNavigation = includeExpression.Navigation.FindInverse();
                    var relatedEntityClrType = includeExpression.Navigation.GetTargetType().ClrType;
                    if (includingClrType != entityClrType
                        && includingClrType.IsAssignableFrom(entityClrType))
                    {
                        includingClrType = entityClrType;
                    }

                    if (includeExpression.Navigation.IsCollection())
                    {
                        var collectionShaper = (CollectionShaperExpression)includeExpression.NavigationExpression;
                        return Expression.Call(
                            _includeCollectionMethodInfo.MakeGenericMethod(entityClrType, includingClrType, relatedEntityClrType),
                            QueryCompilationContext.QueryContextParameter,
                            collectionShaper.Projection,
                            Expression.Constant(((LambdaExpression)Visit(collectionShaper.InnerShaper)).Compile()),
                            includeExpression.EntityExpression,
                            Expression.Constant(includeExpression.Navigation),
                            Expression.Constant(inverseNavigation, typeof(INavigation)),
                            Expression.Constant(
                                GenerateFixup(
                                    includingClrType, relatedEntityClrType, includeExpression.Navigation, inverseNavigation).Compile()),
                            Expression.Constant(_tracking));
                    }

                    return Expression.Call(
                        _includeReferenceMethodInfo.MakeGenericMethod(entityClrType, includingClrType, relatedEntityClrType),
                        QueryCompilationContext.QueryContextParameter,
                        includeExpression.EntityExpression,
                        includeExpression.NavigationExpression,
                        Expression.Constant(includeExpression.Navigation),
                        Expression.Constant(inverseNavigation, typeof(INavigation)),
                        Expression.Constant(
                            GenerateFixup(
                                includingClrType, relatedEntityClrType, includeExpression.Navigation, inverseNavigation).Compile()),
                        Expression.Constant(_tracking));
                }

                if (extensionExpression is CollectionShaperExpression collectionShaperExpression)
                {
                    var elementType = collectionShaperExpression.ElementType;
                    var collectionType = collectionShaperExpression.Type;

                    return Expression.Call(
                        _materializeCollectionMethodInfo.MakeGenericMethod(elementType, collectionType),
                        QueryCompilationContext.QueryContextParameter,
                        collectionShaperExpression.Projection,
                        Expression.Constant(((LambdaExpression)Visit(collectionShaperExpression.InnerShaper)).Compile()),
                        Expression.Constant(
                            collectionShaperExpression.Navigation?.GetCollectionAccessor(),
                            typeof(IClrCollectionAccessor)));
                }

                if (extensionExpression is SingleResultShaperExpression singleResultShaperExpression)
                {
                    return Expression.Call(
                        _materializeSingleResultMethodInfo.MakeGenericMethod(singleResultShaperExpression.Type),
                        QueryCompilationContext.QueryContextParameter,
                        singleResultShaperExpression.Projection,
                        Expression.Constant(((LambdaExpression)Visit(singleResultShaperExpression.InnerShaper)).Compile()));
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
