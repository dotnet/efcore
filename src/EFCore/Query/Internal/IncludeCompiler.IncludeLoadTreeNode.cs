// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public partial class IncludeCompiler
    {
        private sealed class IncludeLoadTreeNode : IncludeLoadTreeNodeBase
        {
            private static readonly MethodInfo _referenceEqualsMethodInfo
                = typeof(object).GetTypeInfo()
                    .GetDeclaredMethod(nameof(ReferenceEquals));

            private static readonly MethodInfo _collectionAccessorAddMethodInfo
                = typeof(IClrCollectionAccessor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IClrCollectionAccessor.Add));

            private static readonly MethodInfo _queryBufferIncludeCollectionMethodInfo
                = typeof(IQueryBuffer).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IQueryBuffer.IncludeCollection));

            private static readonly MethodInfo _queryBufferIncludeCollectionAsyncMethodInfo
                = typeof(IQueryBuffer).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IQueryBuffer.IncludeCollectionAsync));

            public IncludeLoadTreeNode(INavigation navigation) => Navigation = navigation;

            public INavigation Navigation { get; }

            public Expression Compile(
                QueryCompilationContext queryCompilationContext,
                Expression targetQuerySourceReferenceExpression,
                Expression entityParameter,
                ICollection<Expression> propertyExpressions,
                bool trackingQuery,
                bool asyncQuery,
                ref int includedIndex,
                ref int collectionIncludeId)
                => Navigation.IsCollection()
                    ? CompileCollectionInclude(
                        queryCompilationContext,
                        targetQuerySourceReferenceExpression,
                        entityParameter,
                        trackingQuery,
                        asyncQuery,
                        ref collectionIncludeId)
                    : CompileReferenceInclude(
                        queryCompilationContext,
                        propertyExpressions,
                        entityParameter,
                        trackingQuery,
                        asyncQuery,
                        ref includedIndex,
                        ref collectionIncludeId,
                        targetQuerySourceReferenceExpression);

            private Expression CompileCollectionInclude(
                QueryCompilationContext queryCompilationContext,
                Expression targetExpression,
                Expression entityParameter,
                bool trackingQuery,
                bool asyncQuery,
                ref int collectionIncludeId)
            {
                int collectionId;

                if (targetExpression is QuerySourceReferenceExpression targetQuerySourceReferenceExpression
                    && targetQuerySourceReferenceExpression.ReferencedQuerySource is IFromClause fromClause
                    && fromClause.FromExpression is QuerySourceReferenceExpression fromClauseQuerySourceReferenceExpression
                    && fromClauseQuerySourceReferenceExpression.ReferencedQuerySource is GroupJoinClause)
                {
                    // -1 == unable to optimize (GJ)

                    collectionId = -1;
                }
                else
                {
                    collectionId = collectionIncludeId++;
                }

                var targetType = Navigation.GetTargetType().ClrType;

                var mainFromClause
                    = new MainFromClause(
                        targetType.Name.Substring(0, 1).ToLowerInvariant(),
                        targetType,
                        targetExpression.CreateEFPropertyExpression(Navigation.GetIdentifyingMemberInfo()));

                queryCompilationContext.AddQuerySourceRequiringMaterialization(mainFromClause);

                var querySourceReferenceExpression
                    = new QuerySourceReferenceExpression(mainFromClause);

                var collectionQueryModel
                    = new QueryModel(
                        mainFromClause,
                        new SelectClause(querySourceReferenceExpression));

                Compile(
                    queryCompilationContext,
                    collectionQueryModel,
                    trackingQuery,
                    asyncQuery,
                    ref collectionIncludeId,
                    querySourceReferenceExpression);

                Expression collectionLambdaExpression
                    = Expression.Lambda(new SubQueryExpression(collectionQueryModel));

                var includeCollectionMethodInfo = _queryBufferIncludeCollectionMethodInfo;

                Expression cancellationTokenExpression = null;

                if (asyncQuery)
                {
                    var asyncEnumerableType
                        = typeof(IAsyncEnumerable<>).MakeGenericType(targetType);

                    collectionLambdaExpression
                        = Expression.Convert(
                            collectionLambdaExpression,
                            typeof(Func<>).MakeGenericType(asyncEnumerableType));

                    includeCollectionMethodInfo = _queryBufferIncludeCollectionAsyncMethodInfo;
                    cancellationTokenExpression = _cancellationTokenParameter;
                }

                return
                    BuildCollectionIncludeExpressions(
                        Navigation,
                        entityParameter,
                        trackingQuery,
                        collectionLambdaExpression,
                        includeCollectionMethodInfo,
                        cancellationTokenExpression,
                        collectionId);
            }

            private static Expression BuildCollectionIncludeExpressions(
                INavigation navigation,
                Expression targetEntityExpression,
                bool trackingQuery,
                Expression relatedCollectionFuncExpression,
                MethodInfo includeCollectionMethodInfo,
                Expression cancellationTokenExpression,
                int collectionIncludeId)
            {
                var inverseNavigation = navigation.FindInverse();
                var clrCollectionAccessor = navigation.GetCollectionAccessor();

                var arguments = new List<Expression>
                {
                    Expression.Constant(collectionIncludeId),
                    Expression.Constant(navigation),
                    Expression.Constant(inverseNavigation, typeof(INavigation)),
                    Expression.Constant(navigation.GetTargetType()),
                    Expression.Constant(clrCollectionAccessor),
                    Expression.Constant(inverseNavigation?.GetSetter(), typeof(IClrPropertySetter)),
                    Expression.Constant(trackingQuery),
                    targetEntityExpression,
                    relatedCollectionFuncExpression,
                    TryCreateJoinPredicate(targetEntityExpression.Type, navigation)
                };

                if (cancellationTokenExpression != null)
                {
                    arguments.Add(cancellationTokenExpression);
                }

                var targetClrType = navigation.GetTargetType().ClrType;

                var includeCollectionMethodCall
                    = Expression.Call(
                        Expression.Property(
                            EntityQueryModelVisitor.QueryContextParameter,
                            nameof(QueryContext.QueryBuffer)),
                        includeCollectionMethodInfo
                            .MakeGenericMethod(
                            targetEntityExpression.Type,
                                targetClrType,
                                clrCollectionAccessor.CollectionType.TryGetSequenceType()
                                    ?? targetClrType),
                        arguments);

                return
                    navigation.DeclaringEntityType.BaseType != null
                        ? Expression.Condition(
                            Expression.TypeIs(
                                targetEntityExpression,
                                navigation.DeclaringType.ClrType),
                            includeCollectionMethodCall,
                            includeCollectionMethodInfo.ReturnType == typeof(Task)
                                ? (Expression)Expression.Constant(Task.CompletedTask)
                                : Expression.Default(includeCollectionMethodInfo.ReturnType))
                        : (Expression)includeCollectionMethodCall;
            }

            private static Expression TryCreateJoinPredicate(Type targetType, INavigation navigation)
            {
                var foreignKey = navigation.ForeignKey;
                var primaryKeyProperties = foreignKey.PrincipalKey.Properties;
                var foreignKeyProperties = foreignKey.Properties;
                var relatedType = navigation.GetTargetType().ClrType;

                if (primaryKeyProperties.Any(p => p.IsShadowProperty)
                    || foreignKeyProperties.Any(p => p.IsShadowProperty))
                {
                    return
                        Expression.Default(typeof(Func<,,>)
                            .MakeGenericType(targetType, relatedType, typeof(bool)));
                }

                var targetEntityParameter = Expression.Parameter(targetType, "p");
                var relatedEntityParameter = Expression.Parameter(relatedType, "d");

                return Expression.Lambda(
                    primaryKeyProperties.Zip(foreignKeyProperties,
                            (pk, fk) =>
                            {
                                Expression pkMemberAccess
                                    = Expression.MakeMemberAccess(
                                        targetEntityParameter,
                                        pk.GetMemberInfo(forConstruction: false, forSet: false));

                                Expression fkMemberAccess
                                    = Expression.MakeMemberAccess(
                                        relatedEntityParameter,
                                        fk.GetMemberInfo(forConstruction: false, forSet: false));

                                if (pkMemberAccess.Type != fkMemberAccess.Type)
                                {
                                    if (pkMemberAccess.Type.IsNullableType())
                                    {
                                        fkMemberAccess = Expression.Convert(fkMemberAccess, pkMemberAccess.Type);
                                    }
                                    else
                                    {
                                        pkMemberAccess = Expression.Convert(pkMemberAccess, fkMemberAccess.Type);
                                    }
                                }

                                Expression equalityExpression;

                                var comparer
                                    = pk.GetKeyValueComparer()
                                      ?? pk.FindMapping()?.KeyComparer;

                                if (comparer != null)
                                {
                                    if (comparer.Type != pkMemberAccess.Type
                                        && comparer.Type == pkMemberAccess.Type.UnwrapNullableType())
                                    {
                                        comparer = comparer.ToNonNullNullableComparer();
                                    }

                                    equalityExpression
                                        = comparer.ExtractEqualsBody(
                                            pkMemberAccess,
                                            fkMemberAccess);
                                }
                                else if (typeof(IStructuralEquatable).GetTypeInfo()
                                    .IsAssignableFrom(pkMemberAccess.Type.GetTypeInfo()))
                                {
                                    equalityExpression
                                        = Expression.Call(_structuralEqualsMethod, pkMemberAccess, fkMemberAccess);
                                }
                                else
                                {
                                    equalityExpression = Expression.Equal(pkMemberAccess, fkMemberAccess);
                                }

                                return fk.ClrType.IsNullableType()
                                    ? Expression.Condition(
                                        Expression.Equal(fkMemberAccess, Expression.Default(fk.ClrType)),
                                        Expression.Constant(false),
                                        equalityExpression)
                                    : equalityExpression;
                            })
                        .Aggregate(Expression.AndAlso),
                    targetEntityParameter,
                    relatedEntityParameter);
            }

            private static readonly MethodInfo _structuralEqualsMethod
                = typeof(IncludeLoadTreeNode).GetTypeInfo()
                    .GetDeclaredMethod(nameof(StructuralEquals));

            private static bool StructuralEquals(object x, object y)
            {
                return StructuralComparisons.StructuralEqualityComparer.Equals(x, y);
            }

            private Expression CompileReferenceInclude(
                QueryCompilationContext queryCompilationContext,
                ICollection<Expression> propertyExpressions,
                Expression targetEntityExpression,
                bool trackingQuery,
                bool asyncQuery,
                ref int includedIndex,
                ref int collectionIncludeId,
                Expression lastPropertyExpression)
            {
                propertyExpressions.Add(
                    lastPropertyExpression
                        = lastPropertyExpression.CreateEFPropertyExpression(Navigation));

                var relatedArrayAccessExpression
                    = Expression.ArrayAccess(_includedParameter, Expression.Constant(includedIndex++));

                var relatedEntityExpression
                    = Expression.Convert(relatedArrayAccessExpression, Navigation.ClrType);

                var stateManagerProperty
                    = Expression.Property(
                        EntityQueryModelVisitor.QueryContextParameter,
                        nameof(QueryContext.StateManager));

                var blockExpressions = new List<Expression>();
                var isNullBlockExpressions = new List<Expression>();

                if (trackingQuery)
                {
                    blockExpressions.Add(
                        Expression.Call(
                            Expression.Property(
                                EntityQueryModelVisitor.QueryContextParameter,
                                nameof(QueryContext.QueryBuffer)),
                            _queryBufferStartTrackingMethodInfo,
                            relatedArrayAccessExpression,
                            Expression.Constant(Navigation.GetTargetType())));

                    blockExpressions.Add(
                        Expression.Call(
                            _setRelationshipSnapshotValueMethodInfo,
                            stateManagerProperty,
                            Expression.Constant(Navigation),
                            targetEntityExpression,
                            relatedArrayAccessExpression));

                    isNullBlockExpressions.Add(
                        Expression.Call(
                            _setRelationshipIsLoadedMethodInfo,
                            stateManagerProperty,
                            Expression.Constant(Navigation),
                            targetEntityExpression));
                }
                else
                {
                    blockExpressions.Add(
                        targetEntityExpression
                            .MakeMemberAccess(Navigation.GetMemberInfo(false, true))
                            .CreateAssignExpression(relatedEntityExpression));
                }

                var inverseNavigation = Navigation.FindInverse();

                if (inverseNavigation != null)
                {
                    var collection = inverseNavigation.IsCollection();

                    if (trackingQuery)
                    {
                        blockExpressions.Add(
                            Expression.Call(
                                collection
                                    ? _addToCollectionSnapshotMethodInfo
                                    : _setRelationshipSnapshotValueMethodInfo,
                                stateManagerProperty,
                                Expression.Constant(inverseNavigation),
                                relatedArrayAccessExpression,
                                targetEntityExpression));
                    }
                    else
                    {
                        blockExpressions.Add(
                            collection
                                ? (Expression)Expression.Call(
                                    Expression.Constant(inverseNavigation.GetCollectionAccessor()),
                                    _collectionAccessorAddMethodInfo,
                                    relatedArrayAccessExpression,
                                    targetEntityExpression)
                                : relatedEntityExpression.MakeMemberAccess(
                                        inverseNavigation.GetMemberInfo(forConstruction: false, forSet: true))
                                    .CreateAssignExpression(targetEntityExpression));
                    }
                }

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var includeLoadTreeNode in Children)
                {
                    blockExpressions.Add(
                        includeLoadTreeNode.Compile(
                            queryCompilationContext,
                            lastPropertyExpression,
                            relatedEntityExpression,
                            propertyExpressions,
                            trackingQuery,
                            asyncQuery,
                            ref includedIndex,
                            ref collectionIncludeId));
                }

                AwaitTaskExpressions(asyncQuery, blockExpressions);

                var blockType = blockExpressions.Last().Type;

                isNullBlockExpressions.Add(
                    blockType == typeof(Task)
                        ? Expression.Constant(Task.CompletedTask)
                        : (Expression)Expression.Default(blockType));

                return
                    Expression.Condition(
                        Expression.Not(
                            Expression.Call(
                                _referenceEqualsMethodInfo,
                                relatedArrayAccessExpression,
                                Expression.Constant(null, typeof(object)))),
                        Expression.Block(
                            blockType,
                            blockExpressions),
                        Expression.Block(
                            blockType,
                            isNullBlockExpressions),
                        blockType);
            }

            private static readonly MethodInfo _setRelationshipSnapshotValueMethodInfo
                = typeof(IncludeLoadTreeNode).GetTypeInfo()
                    .GetDeclaredMethod(nameof(SetRelationshipSnapshotValue));

            private static void SetRelationshipSnapshotValue(
                IStateManager stateManager,
                IPropertyBase navigation,
                object entity,
                object value)
            {
                var internalEntityEntry = stateManager.TryGetEntry(entity);

                Debug.Assert(internalEntityEntry != null);

                internalEntityEntry.SetRelationshipSnapshotValue(navigation, value);
            }

            private static readonly MethodInfo _setRelationshipIsLoadedMethodInfo
                = typeof(IncludeLoadTreeNode).GetTypeInfo()
                    .GetDeclaredMethod(nameof(SetRelationshipIsLoaded));

            private static void SetRelationshipIsLoaded(
                IStateManager stateManager,
                IPropertyBase navigation,
                object entity)
            {
                var internalEntityEntry = stateManager.TryGetEntry(entity);

                Debug.Assert(internalEntityEntry != null);

                internalEntityEntry.SetIsLoaded((INavigation)navigation);
            }

            private static readonly MethodInfo _addToCollectionSnapshotMethodInfo
                = typeof(IncludeLoadTreeNode).GetTypeInfo()
                    .GetDeclaredMethod(nameof(AddToCollectionSnapshot));

            private static void AddToCollectionSnapshot(
                IStateManager stateManager,
                IPropertyBase navigation,
                object entity,
                object value)
            {
                var internalEntityEntry = stateManager.TryGetEntry(entity);

                Debug.Assert(internalEntityEntry != null);

                internalEntityEntry.AddToCollectionSnapshot(navigation, value);
            }
        }
    }
}
