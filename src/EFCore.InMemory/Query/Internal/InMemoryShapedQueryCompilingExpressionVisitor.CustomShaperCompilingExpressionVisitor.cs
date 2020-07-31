// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public partial class InMemoryShapedQueryCompilingExpressionVisitor
    {
        private sealed class CustomShaperCompilingExpressionVisitor : ExpressionVisitor
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

            private static void IncludeReference<TEntity, TIncludingEntity, TIncludedEntity>(
                QueryContext queryContext,
                TEntity entity,
                TIncludedEntity relatedEntity,
                INavigationBase navigation,
                INavigationBase inverseNavigation,
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

            private static void IncludeCollection<TEntity, TIncludingEntity, TIncludedEntity>(
                QueryContext queryContext,
                IEnumerable<ValueBuffer> innerValueBuffers,
                Func<QueryContext, ValueBuffer, TIncludedEntity> innerShaper,
                TEntity entity,
                INavigationBase navigation,
                INavigationBase inverseNavigation,
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
                        navigation.SetIsLoadedWhenNoTracking(entity);
                    }

                    foreach (var valueBuffer in innerValueBuffers)
                    {
                        var relatedEntity = innerShaper(queryContext, valueBuffer);

                        if (!trackingQuery)
                        {
                            fixup(includingEntity, relatedEntity);
                            if (inverseNavigation != null)
                            {
                                inverseNavigation.SetIsLoadedWhenNoTracking(relatedEntity);
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
                Check.NotNull(extensionExpression, nameof(extensionExpression));

                if (extensionExpression is IncludeExpression includeExpression)
                {
                    var entityClrType = includeExpression.EntityExpression.Type;
                    var includingClrType = includeExpression.Navigation.DeclaringEntityType.ClrType;
                    var inverseNavigation = includeExpression.Navigation.Inverse;
                    var relatedEntityClrType = includeExpression.Navigation.TargetEntityType.ClrType;
                    if (includingClrType != entityClrType
                        && includingClrType.IsAssignableFrom(entityClrType))
                    {
                        includingClrType = entityClrType;
                    }

                    if (includeExpression.Navigation.IsCollection)
                    {
                        var collectionShaper = (CollectionShaperExpression)includeExpression.NavigationExpression;
                        return Expression.Call(
                            _includeCollectionMethodInfo.MakeGenericMethod(entityClrType, includingClrType, relatedEntityClrType),
                            QueryCompilationContext.QueryContextParameter,
                            collectionShaper.Projection,
                            Expression.Constant(((LambdaExpression)Visit(collectionShaper.InnerShaper)).Compile()),
                            includeExpression.EntityExpression,
                            Expression.Constant(includeExpression.Navigation),
                            Expression.Constant(inverseNavigation, typeof(INavigationBase)),
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
                        Expression.Constant(inverseNavigation, typeof(INavigationBase)),
                        Expression.Constant(
                            GenerateFixup(
                                includingClrType, relatedEntityClrType, includeExpression.Navigation, inverseNavigation).Compile()),
                        Expression.Constant(_tracking));
                }

                if (extensionExpression is CollectionShaperExpression collectionShaperExpression)
                {
                    var navigation = collectionShaperExpression.Navigation;
                    var collectionAccessor = navigation?.GetCollectionAccessor();
                    var collectionType = collectionAccessor?.CollectionType ?? collectionShaperExpression.Type;
                    var elementType = collectionShaperExpression.ElementType;

                    return Expression.Call(
                        _materializeCollectionMethodInfo.MakeGenericMethod(elementType, collectionType),
                        QueryCompilationContext.QueryContextParameter,
                        collectionShaperExpression.Projection,
                        Expression.Constant(((LambdaExpression)Visit(collectionShaperExpression.InnerShaper)).Compile()),
                        Expression.Constant(collectionAccessor, typeof(IClrCollectionAccessor)));
                }

                if (extensionExpression is SingleResultShaperExpression singleResultShaperExpression)
                {
                    var innerShaper = (LambdaExpression)Visit(singleResultShaperExpression.InnerShaper);

                    return Expression.Call(
                        _materializeSingleResultMethodInfo.MakeGenericMethod(singleResultShaperExpression.Type),
                        QueryCompilationContext.QueryContextParameter,
                        singleResultShaperExpression.Projection,
                        Expression.Constant(innerShaper.Compile()));
                }

                return base.VisitExtension(extensionExpression);
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
            {
                return entity.MakeMemberAccess(navigation.GetMemberInfo(forMaterialization: true, forSet: true)).Assign(relatedEntity);
            }

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

            private static readonly MethodInfo _collectionAccessorAddMethodInfo
                = typeof(IClrCollectionAccessor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IClrCollectionAccessor.Add));
        }
    }
}
