// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Visitors;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion
{
    public static class NavigationExpansionHelpers
    {
        public static NavigationExpansionExpression CreateNavigationExpansionRoot(
            Expression operand,
            IEntityType entityType,
            INavigation materializeCollectionNavigation,
            NavigationExpandingVisitor navigationExpandingVisitor,
            QueryCompilationContext queryCompilationContext)
        {
            var sourceMapping = new SourceMapping
            {
                RootEntityType = entityType,
            };

            var navigationTreeRoot = NavigationTreeNode.CreateRoot(sourceMapping, fromMapping: new List<string>(), optional: false);
            sourceMapping.NavigationTree = navigationTreeRoot;

            var pendingSelectorParameter = Expression.Parameter(entityType.ClrType);
            var pendingSelector = Expression.Lambda(
                new NavigationBindingExpression(
                    pendingSelectorParameter,
                    navigationTreeRoot,
                    entityType,
                    sourceMapping,
                    pendingSelectorParameter.Type),
                pendingSelectorParameter);

            var result = new NavigationExpansionExpression(
                operand,
                new NavigationExpansionExpressionState(
                    pendingSelectorParameter,
                    new List<SourceMapping> { sourceMapping },
                    pendingSelector,
                    applyPendingSelector: false,
                    new List<(MethodInfo method, LambdaExpression keySelector)>(),
                    pendingIncludeChain: null,
                    pendingCardinalityReducingOperator: null,
                    pendingTags: new List<string>(),
                    customRootMappings: new List<List<string>>(),
                    materializeCollectionNavigation),
                materializeCollectionNavigation?.ClrType ?? operand.Type);

            var rootEntityType = entityType.RootType();
            var queryFilterAnnotation = rootEntityType.FindAnnotation("QueryFilter");
            if (queryFilterAnnotation != null
                && !queryCompilationContext.IgnoreQueryFilters
                && !navigationExpandingVisitor.AppliedQueryFilters.Contains(rootEntityType))
            {
                navigationExpandingVisitor.AppliedQueryFilters.Add(rootEntityType);
                var filterPredicate = (LambdaExpression)queryFilterAnnotation.Value;

                var parameterExtractingExpressionVisitor = new ParameterExtractingExpressionVisitor(
                    queryCompilationContext.EvaluatableExpressionFilter,
                    queryCompilationContext.ParameterValues,
                    queryCompilationContext.ContextType,
                    queryCompilationContext.Logger,
                    parameterize: false,
                    generateContextAccessors: true);

                filterPredicate = (LambdaExpression)parameterExtractingExpressionVisitor.ExtractParameters(filterPredicate);

                // in case of query filters we need to strip MaterializeCollectionNavigation from the initial collection and apply it after the filter
                result = (NavigationExpansionExpression)RemoveMaterializeCollection(result);
                var sequenceType = result.Type.GetSequenceType();

                // if we are constructing EntityQueryable of a derived type, we need to re-map filter predicate to the correct derived type
                var filterPredicateParameter = filterPredicate.Parameters[0];
                if (filterPredicateParameter.Type != sequenceType)
                {
                    var newFilterPredicateParameter = Expression.Parameter(sequenceType, filterPredicateParameter.Name);
                    filterPredicate = (LambdaExpression)new ExpressionReplacingVisitor(filterPredicateParameter, newFilterPredicateParameter).Visit(filterPredicate);
                }

                var whereMethod = LinqMethodHelpers.QueryableWhereMethodInfo.MakeGenericMethod(result.Type.GetSequenceType());
                var filteredResult = (Expression)Expression.Call(whereMethod, result, filterPredicate);
                if (materializeCollectionNavigation != null)
                {
                    filteredResult = new MaterializeCollectionNavigationExpression(result, materializeCollectionNavigation);
                }

                result = (NavigationExpansionExpression)navigationExpandingVisitor.Visit(filteredResult);
            }

            return result;
        }

        private static readonly MethodInfo _leftJoinMethodInfo = typeof(QueryableExtensions).GetTypeInfo()
            .GetDeclaredMethods(nameof(QueryableExtensions.LeftJoin)).Single(mi => mi.GetParameters().Length == 5);

        public static (Expression source, ParameterExpression parameter) AddNavigationJoin(
            Expression sourceExpression,
            ParameterExpression parameterExpression,
            SourceMapping sourceMapping,
            NavigationTreeNode navigationTree,
            NavigationExpansionExpressionState state,
            List<INavigation> navigationPath,
            bool include,
            NavigationExpandingVisitor navigationExpandingVisitor,
            QueryCompilationContext queryCompilationContext)
        {
            var joinNeeded = include
                ? navigationTree.Included == NavigationTreeNodeIncludeMode.ReferencePending
                : navigationTree.ExpansionMode == NavigationTreeNodeExpansionMode.ReferencePending;

            if (joinNeeded)
            {
                // TODO: hack/quirk to work-around potential bugs in the new navigation generation
                if (queryCompilationContext.IgnoreQueryFilters)
                {
                    LegacyCodepath(ref sourceExpression, ref parameterExpression, navigationTree, state, navigationPath, include);
                }
                else
                {
                    var navigation = navigationTree.Navigation;
                    var sourceType = sourceExpression.Type.GetSequenceType();
                    var entityQueryable = NullAsyncQueryProvider.Instance.CreateEntityQueryableExpression(navigation.GetTargetType().ClrType);
                    var navigationRoot = CreateNavigationExpansionRoot(entityQueryable, navigation.GetTargetType(), materializeCollectionNavigation: null, navigationExpandingVisitor, queryCompilationContext);

                    var resultType = typeof(TransparentIdentifier<,>).MakeGenericType(sourceType, navigationRoot.State.CurrentParameter.Type);

                    var outerParameter = Expression.Parameter(sourceType, parameterExpression.Name);
                    var outerKeySelectorParameter = outerParameter;
                    var outerTransparentIdentifierAccessorExpression = outerParameter.BuildPropertyAccess(navigationTree.Parent.ToMapping);

                    var outerKeySelectorBody = CreateKeyAccessExpression(
                        outerTransparentIdentifierAccessorExpression,
                        navigation.IsDependentToPrincipal()
                            ? navigation.ForeignKey.Properties
                            : navigation.ForeignKey.PrincipalKey.Properties,
                        addNullCheck: navigationTree.Parent != null && navigationTree.Parent.Optional);

                    var innerKeySelectorParameter = Expression.Parameter(
                        navigationRoot.State.CurrentParameter.Type,
                        navigationRoot.State.CurrentParameter.Name + "." + navigationTree.Navigation.Name);

                    // we are guaranteed to only have one SourceMapping here because it will either be a simple EntityQueryable (normal navigation expansion case)
                    // or EntityQueryable with navigations from query filters - those however can only contain navigations that stem from the original, so it's all part of the same root, hence only one SourceMapping
                    //
                    // if the query filter is complex and contains Joins/GroupJoins etc that would spawn additional SourceMappings,
                    // those mappings would be on the inner NavigationExpansionExpression, so we don't need to worry about them here.
                    var navigationRootSourceMapping = navigationRoot.State.SourceMappings.Single();
                    var innerTransparentIdentifierAccessorExpression = innerKeySelectorParameter.BuildPropertyAccess(navigationRootSourceMapping.NavigationTree.ToMapping);

                    var innerKeySelectorBody = CreateKeyAccessExpression(
                        innerTransparentIdentifierAccessorExpression,
                        navigation.IsDependentToPrincipal()
                            ? navigation.ForeignKey.PrincipalKey.Properties
                            : navigation.ForeignKey.Properties);

                    if (outerKeySelectorBody.Type.IsNullableType()
                        && !innerKeySelectorBody.Type.IsNullableType())
                    {
                        innerKeySelectorBody = Expression.Convert(innerKeySelectorBody, outerKeySelectorBody.Type);
                    }
                    else if (innerKeySelectorBody.Type.IsNullableType()
                        && !outerKeySelectorBody.Type.IsNullableType())
                    {
                        outerKeySelectorBody = Expression.Convert(outerKeySelectorBody, innerKeySelectorBody.Type);
                    }

                    var outerKeySelector = Expression.Lambda(
                        outerKeySelectorBody,
                        outerKeySelectorParameter);

                    var innerKeySelector = Expression.Lambda(
                        innerKeySelectorBody,
                        innerKeySelectorParameter);

                    if (!sourceExpression.Type.IsQueryableType())
                    {
                        var asQueryableMethodInfo = LinqMethodHelpers.AsQueryable.MakeGenericMethod(sourceType);
                        sourceExpression = Expression.Call(asQueryableMethodInfo, sourceExpression);
                    }

                    var joinMethodInfo = navigationTree.Optional
                        ? _leftJoinMethodInfo.MakeGenericMethod(
                            sourceType,
                            navigationRoot.State.CurrentParameter.Type,
                            outerKeySelector.Body.Type,
                            resultType)
                        : LinqMethodHelpers.QueryableJoinMethodInfo.MakeGenericMethod(
                            sourceType,
                            navigationRoot.State.CurrentParameter.Type,
                            outerKeySelector.Body.Type,
                            resultType);

                    var resultSelectorOuterParameterName = outerParameter.Name;
                    var resultSelectorOuterParameter = Expression.Parameter(sourceType, resultSelectorOuterParameterName);

                    var resultSelectorInnerParameterName = innerKeySelectorParameter.Name;
                    var resultSelectorInnerParameter = Expression.Parameter(navigationRoot.State.CurrentParameter.Type, resultSelectorInnerParameterName);

                    var transparentIdentifierCtorInfo
                        = resultType.GetTypeInfo().GetConstructors().Single();

                    var transparentIdentifierOuterMemberInfo = resultType.GetTypeInfo().GetDeclaredField("Outer");
                    var transparentIdentifierInnerMemberInfo = resultType.GetTypeInfo().GetDeclaredField("Inner");

                    var resultSelector = Expression.Lambda(
                        Expression.New(
                            transparentIdentifierCtorInfo,
                            new[] { resultSelectorOuterParameter, resultSelectorInnerParameter },
                            new[] { transparentIdentifierOuterMemberInfo, transparentIdentifierInnerMemberInfo }),
                        resultSelectorOuterParameter,
                        resultSelectorInnerParameter);

                    var joinMethodCall = Expression.Call(
                        joinMethodInfo,
                        sourceExpression,
                        navigationRoot.Operand,
                        outerKeySelector,
                        innerKeySelector,
                        resultSelector);

                    sourceExpression = joinMethodCall;

                    var transparentIdentifierParameterName = resultSelectorInnerParameterName;
                    var transparentIdentifierParameter = Expression.Parameter(resultSelector.ReturnType, transparentIdentifierParameterName);
                    parameterExpression = transparentIdentifierParameter;

                    // remap navigation 'To' paths -> for inner navigations (they should all be expanded by now) prepend "Inner", for every other (already expanded) navigation prepend "Outer"
                    var innerNodes = include
                        ? navigationRootSourceMapping.NavigationTree.Flatten().Where(n => (n.Included == NavigationTreeNodeIncludeMode.ReferenceComplete
                            || n.ExpansionMode == NavigationTreeNodeExpansionMode.ReferenceComplete
                            || n.Navigation.ForeignKey.IsOwnership)
                                && n != navigationTree)
                        : navigationRootSourceMapping.NavigationTree.Flatten().Where(n => (n.ExpansionMode == NavigationTreeNodeExpansionMode.ReferenceComplete
                            || n.Navigation.ForeignKey.IsOwnership)
                                && n != navigationTree);

                    foreach (var innerNode in innerNodes)
                    {
                        innerNode.ToMapping.Insert(0, nameof(TransparentIdentifier<object, object>.Inner));
                    }

                    foreach (var mapping in state.SourceMappings)
                    {
                        var nodes = include
                            ? mapping.NavigationTree.Flatten().Where(n => (n.Included == NavigationTreeNodeIncludeMode.ReferenceComplete
                                || n.ExpansionMode == NavigationTreeNodeExpansionMode.ReferenceComplete
                                || n.Navigation.ForeignKey.IsOwnership)
                                    && n != navigationTree)
                            : mapping.NavigationTree.Flatten().Where(n => (n.ExpansionMode == NavigationTreeNodeExpansionMode.ReferenceComplete
                                || n.Navigation.ForeignKey.IsOwnership)
                                    && n != navigationTree);

                        foreach (var navigationTreeNode in nodes)
                        {
                            navigationTreeNode.ToMapping.Insert(0, nameof(TransparentIdentifier<object, object>.Outer));
                        }
                    }

                    // TODO: there shouldn't be any custom root mappings for inner, but not 100% sure - think & TEST!
                    foreach (var customRootMapping in state.CustomRootMappings)
                    {
                        customRootMapping.Insert(0, nameof(TransparentIdentifier<object, object>.Outer));
                    }

                    if (include)
                    {
                        navigationTree.Included = NavigationTreeNodeIncludeMode.ReferenceComplete;
                    }
                    else
                    {
                        navigationTree.ExpansionMode = NavigationTreeNodeExpansionMode.ReferenceComplete;
                    }

                    // finally, we need to incorporate the newly created navigation tree into the old one
                    navigationRootSourceMapping.NavigationTree.SetNavigation(navigationTree.Navigation);
                    navigationTree.Parent.AddChild(navigationRootSourceMapping.NavigationTree);
                    navigationPath.Add(navigation);
                }
            }
            else
            {
                navigationPath.Add(navigationTree.Navigation);
            }

            var result = (source: sourceExpression, parameter: parameterExpression);
            foreach (var child in navigationTree.Children.Where(n => !n.Navigation.IsCollection()))
            {
                result = AddNavigationJoin(
                    result.source,
                    result.parameter,
                    sourceMapping,
                    child,
                    state,
                    navigationPath.ToList(),
                    include,
                    navigationExpandingVisitor,
                    queryCompilationContext);
            }

            return result;
        }

        private static void LegacyCodepath(
            ref Expression sourceExpression,
            ref ParameterExpression parameterExpression,
            NavigationTreeNode navigationTree,
            NavigationExpansionExpressionState state,
            List<INavigation> navigationPath,
            bool include)
        {
            var navigation = navigationTree.Navigation;
            var sourceType = sourceExpression.Type.GetSequenceType();
            var navigationTargetEntityType = navigation.GetTargetType();

            var entityQueryable = NullAsyncQueryProvider.Instance.CreateEntityQueryableExpression(navigationTargetEntityType.ClrType);
            var resultType = typeof(TransparentIdentifier<,>).MakeGenericType(sourceType, navigationTargetEntityType.ClrType);

            var outerParameter = Expression.Parameter(sourceType, parameterExpression.Name);
            var outerKeySelectorParameter = outerParameter;
            var transparentIdentifierAccessorExpression = outerParameter.BuildPropertyAccess(navigationTree.Parent.ToMapping);

            var outerKeySelectorBody = CreateKeyAccessExpression(
                transparentIdentifierAccessorExpression,
                navigation.IsDependentToPrincipal()
                    ? navigation.ForeignKey.Properties
                    : navigation.ForeignKey.PrincipalKey.Properties,
                addNullCheck: navigationTree.Parent != null && navigationTree.Parent.Optional);

            var innerKeySelectorParameterType = navigationTargetEntityType.ClrType;
            var innerKeySelectorParameter = Expression.Parameter(
                innerKeySelectorParameterType,
                parameterExpression.Name + "." + navigationTree.Navigation.Name);

            var innerKeySelectorBody = CreateKeyAccessExpression(
                innerKeySelectorParameter,
                navigation.IsDependentToPrincipal()
                    ? navigation.ForeignKey.PrincipalKey.Properties
                    : navigation.ForeignKey.Properties);

            if (outerKeySelectorBody.Type.IsNullableType()
                && !innerKeySelectorBody.Type.IsNullableType())
            {
                innerKeySelectorBody = Expression.Convert(innerKeySelectorBody, outerKeySelectorBody.Type);
            }
            else if (innerKeySelectorBody.Type.IsNullableType()
                && !outerKeySelectorBody.Type.IsNullableType())
            {
                outerKeySelectorBody = Expression.Convert(outerKeySelectorBody, innerKeySelectorBody.Type);
            }

            var outerKeySelector = Expression.Lambda(
                outerKeySelectorBody,
                outerKeySelectorParameter);

            var innerKeySelector = Expression.Lambda(
                innerKeySelectorBody,
                innerKeySelectorParameter);

            if (!sourceExpression.Type.IsQueryableType())
            {
                var asQueryableMethodInfo = LinqMethodHelpers.AsQueryable.MakeGenericMethod(sourceType);
                sourceExpression = Expression.Call(asQueryableMethodInfo, sourceExpression);
            }

            var joinMethodInfo = navigationTree.Optional
                ? _leftJoinMethodInfo.MakeGenericMethod(
                    sourceType,
                    navigationTargetEntityType.ClrType,
                    outerKeySelector.Body.Type,
                    resultType)
                : LinqMethodHelpers.QueryableJoinMethodInfo.MakeGenericMethod(
                    sourceType,
                    navigationTargetEntityType.ClrType,
                    outerKeySelector.Body.Type,
                    resultType);

            var resultSelectorOuterParameterName = outerParameter.Name;
            var resultSelectorOuterParameter = Expression.Parameter(sourceType, resultSelectorOuterParameterName);

            var resultSelectorInnerParameterName = innerKeySelectorParameter.Name;
            var resultSelectorInnerParameter = Expression.Parameter(navigationTargetEntityType.ClrType, resultSelectorInnerParameterName);

            var transparentIdentifierCtorInfo
                = resultType.GetTypeInfo().GetConstructors().Single();

            var transparentIdentifierOuterMemberInfo = resultType.GetTypeInfo().GetDeclaredField("Outer");
            var transparentIdentifierInnerMemberInfo = resultType.GetTypeInfo().GetDeclaredField("Inner");

            var resultSelector = Expression.Lambda(
                Expression.New(
                    transparentIdentifierCtorInfo,
                    new[] { resultSelectorOuterParameter, resultSelectorInnerParameter },
                    new[] { transparentIdentifierOuterMemberInfo, transparentIdentifierInnerMemberInfo }),
                resultSelectorOuterParameter,
                resultSelectorInnerParameter);

            var joinMethodCall = Expression.Call(
                joinMethodInfo,
                sourceExpression,
                entityQueryable,
                outerKeySelector,
                innerKeySelector,
                resultSelector);

            sourceExpression = joinMethodCall;

            var transparentIdentifierParameterName = resultSelectorInnerParameterName;
            var transparentIdentifierParameter = Expression.Parameter(resultSelector.ReturnType, transparentIdentifierParameterName);
            parameterExpression = transparentIdentifierParameter;

            // remap navigation 'To' paths -> for this navigation prepend "Inner", for every other (already expanded) navigation prepend "Outer"
            navigationTree.ToMapping.Insert(0, nameof(TransparentIdentifier<object, object>.Inner));
            foreach (var mapping in state.SourceMappings)
            {
                var nodes = include
                    ? mapping.NavigationTree.Flatten().Where(n => (n.Included == NavigationTreeNodeIncludeMode.ReferenceComplete
                        || n.ExpansionMode == NavigationTreeNodeExpansionMode.ReferenceComplete
                        || n.Navigation.ForeignKey.IsOwnership)
                            && n != navigationTree)
                    : mapping.NavigationTree.Flatten().Where(n => (n.ExpansionMode == NavigationTreeNodeExpansionMode.ReferenceComplete
                        || n.Navigation.ForeignKey.IsOwnership)
                            && n != navigationTree);

                foreach (var navigationTreeNode in nodes)
                {
                    navigationTreeNode.ToMapping.Insert(0, nameof(TransparentIdentifier<object, object>.Outer));
                }
            }

            foreach (var customRootMapping in state.CustomRootMappings)
            {
                customRootMapping.Insert(0, nameof(TransparentIdentifier<object, object>.Outer));
            }

            if (include)
            {
                navigationTree.Included = NavigationTreeNodeIncludeMode.ReferenceComplete;
            }
            else
            {
                navigationTree.ExpansionMode = NavigationTreeNodeExpansionMode.ReferenceComplete;

            }
            navigationPath.Add(navigation);
        }

        public static Expression CreateKeyAccessExpression(
            Expression target, IReadOnlyList<IProperty> properties, bool addNullCheck = false)
            => properties.Count == 1
                ? CreatePropertyExpression(target, properties[0], addNullCheck)
                : Expression.New(
                    AnonymousObject.AnonymousObjectCtor,
                    Expression.NewArrayInit(
                        typeof(object),
                        properties
                            .Select(p => Expression.Convert(CreatePropertyExpression(target, p, addNullCheck), typeof(object)))
                            .Cast<Expression>()
                            .ToArray()));

        private static Expression CreatePropertyExpression(Expression target, IProperty property, bool addNullCheck)
        {
            var propertyExpression = target.CreateEFPropertyExpression(property, makeNullable: false);

            var propertyDeclaringType = property.DeclaringType.ClrType;
            if (propertyDeclaringType != target.Type
                && target.Type.GetTypeInfo().IsAssignableFrom(propertyDeclaringType.GetTypeInfo()))
            {
                if (!propertyExpression.Type.IsNullableType())
                {
                    propertyExpression = Expression.Convert(propertyExpression, propertyExpression.Type.MakeNullable());
                }

                return Expression.Condition(
                    Expression.TypeIs(target, propertyDeclaringType),
                    propertyExpression,
                    Expression.Constant(null, propertyExpression.Type));
            }

            return addNullCheck
                ? new NullConditionalExpression(target, propertyExpression)
                : propertyExpression;
        }

        public static Expression CreateNullKeyExpression(Type resultType, int keyCount)
            => resultType == typeof(AnonymousObject)
                ? Expression.New(
                    AnonymousObject.AnonymousObjectCtor,
                    Expression.NewArrayInit(
                        typeof(object),
                        Enumerable.Repeat(
                            Expression.Constant(null),
                            keyCount)))
                : (Expression)Expression.Constant(null);

        public static readonly MethodInfo MaterializeCollectionNavigationMethodInfo
            = typeof(NavigationExpansionHelpers).GetTypeInfo()
                .GetDeclaredMethod(nameof(MaterializeCollectionNavigation));

        public static TResult MaterializeCollectionNavigation<TResult, TEntity>(
            IEnumerable<object> elements,
            INavigation navigation)
            where TResult : IEnumerable<TEntity>
        {
            var collection = navigation.GetCollectionAccessor().Create(elements);

            return (TResult)collection;
        }

        public static Expression RemoveMaterializeCollection(Expression expression)
        {
            if (expression is NavigationExpansionExpression navigationExpansionExpression
                && navigationExpansionExpression.State.MaterializeCollectionNavigation != null)
            {
                navigationExpansionExpression.State.MaterializeCollectionNavigation = null;

                return new NavigationExpansionExpression(
                    navigationExpansionExpression.Operand,
                    navigationExpansionExpression.State,
                    navigationExpansionExpression.Operand.Type);
            }

            if (expression is NavigationExpansionRootExpression navigationExpansionRootExpression
                && navigationExpansionRootExpression.NavigationExpansion.State.MaterializeCollectionNavigation != null)
            {
                navigationExpansionRootExpression.NavigationExpansion.State.MaterializeCollectionNavigation = null;

                var rewritten = new NavigationExpansionExpression(
                    navigationExpansionRootExpression.NavigationExpansion.Operand,
                    navigationExpansionRootExpression.NavigationExpansion.State,
                    navigationExpansionRootExpression.NavigationExpansion.Operand.Type);

                return new NavigationExpansionRootExpression(rewritten, navigationExpansionRootExpression.Mapping);
            }

            return expression;
        }
    }
}
