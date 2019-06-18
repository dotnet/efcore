// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
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
        private class IncludeCompilingExpressionVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _dbDataReaderParameter;
            private readonly ParameterExpression _resultCoordinatorParameter;
            private readonly bool _tracking;

            public IncludeCompilingExpressionVisitor(
                ParameterExpression dbDataReaderParameter,
                ParameterExpression resultCoordinatorParameter,
                bool tracking)
            {
                _dbDataReaderParameter = dbDataReaderParameter;
                _resultCoordinatorParameter = resultCoordinatorParameter;
                _tracking = tracking;
            }

            private static readonly MethodInfo _includeReferenceMethodInfo
                = typeof(IncludeCompilingExpressionVisitor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IncludeReference));

            private static void IncludeReference<TEntity, TIncludedEntity>(
                QueryContext queryContext,
                DbDataReader dbDataReader,
                TEntity entity,
                Func<QueryContext, DbDataReader, ResultCoordinator, TIncludedEntity> innerShaper,
                INavigation navigation,
                INavigation inverseNavigation,
                Action<TEntity, TIncludedEntity> fixup,
                bool trackingQuery,
                ResultCoordinator resultCoordinator)
            {
                var relatedEntity = innerShaper(queryContext, dbDataReader, resultCoordinator);

                if (trackingQuery)
                {
                    // For non-null relatedEntity StateManager will set the flag
                    if (ReferenceEquals(relatedEntity, null))
                    {
                        queryContext.StateManager.TryGetEntry(entity).SetIsLoaded(navigation);
                    }
                }
                else
                {
                    SetIsLoadedNoTracking(entity, navigation);
                    if (!ReferenceEquals(relatedEntity, null))
                    {
                        fixup(entity, relatedEntity);
                        if (inverseNavigation != null && !inverseNavigation.IsCollection())
                        {
                            SetIsLoadedNoTracking(relatedEntity, inverseNavigation);
                        }
                    }
                }
            }

            private static readonly MethodInfo _includeCollectionMethodInfo
                = typeof(IncludeCompilingExpressionVisitor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IncludeCollection));

            private static void IncludeCollection<TEntity, TIncludedEntity>(
                QueryContext queryContext,
                DbDataReader dbDataReader,
                TEntity entity,
                Func<QueryContext, DbDataReader, object[]> outerKeySelector,
                Func<QueryContext, DbDataReader, object[]> innerKeySelector,
                Func<QueryContext, DbDataReader, ResultCoordinator, TIncludedEntity> innerShaper,
                INavigation navigation,
                INavigation inverseNavigation,
                Action<TEntity, TIncludedEntity> fixup,
                bool trackingQuery,
                ResultCoordinator resultCoordinator)
            {
                if (entity is null)
                {
                    return;
                }

                if (trackingQuery)
                {
                    queryContext.StateManager.TryGetEntry(entity).SetIsLoaded(navigation);
                }
                else
                {
                    SetIsLoadedNoTracking(entity, navigation);
                }

                var innerKey = innerKeySelector(queryContext, dbDataReader);
                var outerKey = outerKeySelector(queryContext, dbDataReader);
                var relatedEntity = innerShaper(queryContext, dbDataReader, resultCoordinator);

                if (ReferenceEquals(relatedEntity, null))
                {
                    navigation.GetCollectionAccessor().GetOrCreate(entity);
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

                var hasNext = resultCoordinator.HasNext ?? dbDataReader.Read();
                while (hasNext)
                {
                    resultCoordinator.HasNext = null;
                    var currentOuterKey = outerKeySelector(queryContext, dbDataReader);
                    if (!StructuralComparisons.StructuralEqualityComparer.Equals(outerKey, currentOuterKey))
                    {
                        resultCoordinator.HasNext = true;
                        break;
                    }

                    var currentInnerKey = innerKeySelector(queryContext, dbDataReader);
                    if (StructuralComparisons.StructuralEqualityComparer.Equals(innerKey, currentInnerKey))
                    {
                        continue;
                    }

                    relatedEntity = innerShaper(queryContext, dbDataReader, resultCoordinator);
                    if (!trackingQuery)
                    {
                        fixup(entity, relatedEntity);
                        if (inverseNavigation != null && !inverseNavigation.IsCollection())
                        {
                            SetIsLoadedNoTracking(relatedEntity, inverseNavigation);
                        }
                    }

                    hasNext = resultCoordinator.HasNext ?? dbDataReader.Read();
                }

                resultCoordinator.HasNext = hasNext;
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
                    Expression result;
                    var entityClrType = includeExpression.EntityExpression.Type;
                    var inverseNavigation = includeExpression.Navigation.FindInverse();
                    if (includeExpression.Navigation.IsCollection())
                    {
                        var relatedEntityClrType = includeExpression.NavigationExpression.Type.TryGetSequenceType();
                        var collectionShaper = (RelationalCollectionShaperExpression)includeExpression.NavigationExpression;
                        var innerShaper = Visit(collectionShaper.InnerShaper);

                        result = Expression.Call(
                            _includeCollectionMethodInfo.MakeGenericMethod(entityClrType, relatedEntityClrType),
                            QueryCompilationContext.QueryContextParameter,
                            _dbDataReaderParameter,
                            // We don't need to visit entityExpression since it is supposed to be a parameterExpression only
                            includeExpression.EntityExpression,
                            Expression.Constant(
                                Expression.Lambda(
                                    collectionShaper.OuterKeySelector,
                                    QueryCompilationContext.QueryContextParameter,
                                    _dbDataReaderParameter).Compile()),
                            Expression.Constant(
                                Expression.Lambda(
                                    collectionShaper.InnerKeySelector,
                                    QueryCompilationContext.QueryContextParameter,
                                    _dbDataReaderParameter).Compile()),
                            Expression.Constant(
                                Expression.Lambda(
                                    innerShaper,
                                    QueryCompilationContext.QueryContextParameter,
                                    _dbDataReaderParameter,
                                    _resultCoordinatorParameter).Compile()),
                            Expression.Constant(includeExpression.Navigation),
                            Expression.Constant(inverseNavigation, typeof(INavigation)),
                            Expression.Constant(
                                GenerateFixup(entityClrType, relatedEntityClrType, includeExpression.Navigation, inverseNavigation).Compile()),
                            Expression.Constant(_tracking),
                            _resultCoordinatorParameter);
                    }
                    else
                    {
                        var relatedEntityClrType = includeExpression.NavigationExpression.Type;
                        result =  Expression.Call(
                            _includeReferenceMethodInfo.MakeGenericMethod(entityClrType, relatedEntityClrType),
                            QueryCompilationContext.QueryContextParameter,
                            _dbDataReaderParameter,
                            // We don't need to visit entityExpression since it is supposed to be a parameterExpression only
                            includeExpression.EntityExpression,
                            Expression.Constant(
                                Expression.Lambda(
                                    Visit(includeExpression.NavigationExpression),
                                    QueryCompilationContext.QueryContextParameter,
                                    _dbDataReaderParameter,
                                    _resultCoordinatorParameter).Compile()),
                            Expression.Constant(includeExpression.Navigation),
                            Expression.Constant(inverseNavigation, typeof(INavigation)),
                            Expression.Constant(
                                GenerateFixup(entityClrType, relatedEntityClrType, includeExpression.Navigation, inverseNavigation).Compile()),
                            Expression.Constant(_tracking),
                            _resultCoordinatorParameter);
                    }

                    return entityClrType != includeExpression.Navigation.DeclaringEntityType.ClrType
                        ? Expression.IfThen(
                            Expression.TypeIs(includeExpression.EntityExpression, includeExpression.Navigation.DeclaringEntityType.ClrType),
                            result)
                        : result;
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
