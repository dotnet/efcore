// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Internal
{
    public static class NavigationExpansionHelpers
    {
        public static NavigationExpansionExpression CreateNavigationExpansionRoot(
            Expression operand,
            IEntityType entityType,
            INavigation materializeCollectionNavigation)
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

            return new NavigationExpansionExpression(
                operand,
                new NavigationExpansionExpressionState(
                    pendingSelectorParameter,
                    new List<SourceMapping> { sourceMapping },
                    pendingSelector,
                    applyPendingSelector: false,
                    new List<(MethodInfo method, LambdaExpression keySelector)>(),
                    pendingIncludeChain: null,
                    pendingCardinalityReducingOperator: null,
                    customRootMappings: new List<List<string>>(),
                    materializeCollectionNavigation),
                materializeCollectionNavigation?.ClrType ?? operand.Type);
        }

        private static readonly MethodInfo _leftJoinMethodInfo = typeof(QueryableExtensions).GetTypeInfo()
            .GetDeclaredMethods(nameof(QueryableExtensions.LeftJoin)).Single(mi => mi.GetParameters().Length == 5);

        public static (Expression Source, ParameterExpression Parameter) AddNavigationJoin(
            Expression sourceExpression,
            ParameterExpression parameterExpression,
            SourceMapping sourceMapping,
            NavigationTreeNode navigationTree,
            NavigationExpansionExpressionState state,
            List<INavigation> navigationPath,
            bool include)
        {
            var joinNeeded = include
                ? navigationTree.IncludeState == NavigationState.Pending
                : navigationTree.ExpansionState == NavigationState.Pending;

            if (joinNeeded)
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
                        ? mapping.NavigationTree.Flatten().Where(n => (n.IncludeState == NavigationState.Complete
                            || n.ExpansionState == NavigationState.Complete
                            || n.Navigation.ForeignKey.IsOwnership)
                                && n != navigationTree)
                        : mapping.NavigationTree.Flatten().Where(n => (n.ExpansionState == NavigationState.Complete
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
                    navigationTree.IncludeState = NavigationState.Complete;
                }
                else
                {
                    navigationTree.ExpansionState = NavigationState.Complete;
                }

                navigationPath.Add(navigation);
            }
            else
            {
                navigationPath.Add(navigationTree.Navigation);
            }

            var result = (Source: sourceExpression, Parameter: parameterExpression);
            foreach (var child in navigationTree.Children.Where(n => !n.IsCollection))
            {
                result = AddNavigationJoin(
                    result.Source,
                    result.Parameter,
                    sourceMapping,
                    child,
                    state,
                    navigationPath.ToList(),
                    include);
            }

            return result;
        }

        public static Expression CreateKeyAccessExpression(
            Expression target, IReadOnlyList<IProperty> properties, bool addNullCheck = false)
            => properties.Count == 1
                ? CreatePropertyExpression(target, properties[0], addNullCheck)
                : Expression.New(
                    AnonymousObject.AnonymousObjectCtor,
                    Expression.NewArrayInit(
                        typeof(object),
                        properties.Select(p => Expression.Convert(CreatePropertyExpression(target, p, addNullCheck), typeof(object)))));

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
    }
}
