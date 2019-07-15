// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Visitors
{
    public class NavigationExpansionReducingVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case NavigationBindingExpression navigationBindingExpression:
                    return navigationBindingExpression.RootParameter.BuildPropertyAccess(
                        navigationBindingExpression.NavigationTreeNode.ToMapping);

                case NavigationExpansionRootExpression navigationExpansionRootExpression:
                    return Visit(navigationExpansionRootExpression.Unwrap());

                case NavigationExpansionExpression navigationExpansionExpression:
                    {
                        var (result, state) = ApplyIncludes(navigationExpansionExpression);
                        result = Visit(result);

                        if (!state.ApplyPendingSelector
                            && state.PendingOrderings.Count == 0
                            && state.PendingCardinalityReducingOperator == null
                            && state.MaterializeCollectionNavigation == null)
                        {
                            return result;
                        }

                        var parameterType = result.Type.GetSequenceType();

                        foreach (var pendingOrdering in state.PendingOrderings)
                        {
                            var remappedKeySelectorBody = new ExpressionReplacingVisitor(pendingOrdering.keySelector.Parameters[0], state.CurrentParameter).Visit(pendingOrdering.keySelector.Body);
                            var newSelectorBody = new NavigationPropertyUnbindingVisitor(state.CurrentParameter).Visit(remappedKeySelectorBody);
                            var newSelector = Expression.Lambda(newSelectorBody, state.CurrentParameter);
                            var orderingMethod = pendingOrdering.method.MakeGenericMethod(state.CurrentParameter.Type, newSelectorBody.Type);
                            result = Expression.Call(orderingMethod, result, newSelector);
                        }

                        if (state.ApplyPendingSelector)
                        {
                            var pendingSelector = (LambdaExpression)new NavigationPropertyUnbindingVisitor(state.CurrentParameter).Visit(state.PendingSelector);
                            var pendingSelectorBodyType = pendingSelector.Type.GetGenericArguments()[1];

                            var pendingSelectMethod = result.Type.IsGenericType && (result.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>) || result.Type.GetGenericTypeDefinition() == typeof(IOrderedEnumerable<>))
                                ? LinqMethodHelpers.EnumerableSelectMethodInfo.MakeGenericMethod(parameterType, pendingSelectorBodyType)
                                : LinqMethodHelpers.QueryableSelectMethodInfo.MakeGenericMethod(parameterType, pendingSelectorBodyType);

                            result = Expression.Call(pendingSelectMethod, result, pendingSelector);
                            parameterType = result.Type.GetSequenceType();
                        }

                        if (state.PendingCardinalityReducingOperator != null)
                        {
                            result = Expression.Call(state.PendingCardinalityReducingOperator, result);
                        }

                        if (state.MaterializeCollectionNavigation != null)
                        {
                            result = new MaterializeCollectionNavigationExpression(result, state.MaterializeCollectionNavigation);
                        }

                        if (navigationExpansionExpression.Type != result.Type && navigationExpansionExpression.Type.IsGenericType)
                        {
                            if (navigationExpansionExpression.Type.GetGenericTypeDefinition() == typeof(IOrderedQueryable<>))
                            {
                                var toOrderedQueryableMethodInfo = ToOrderedQueryableMethod.MakeGenericMethod(parameterType);

                                return Expression.Call(toOrderedQueryableMethodInfo, result);
                            }

                            if (navigationExpansionExpression.Type.GetGenericTypeDefinition() == typeof(IOrderedEnumerable<>))
                            {
                                var toOrderedEnumerableMethodInfo = ToOrderedEnumerableMethod.MakeGenericMethod(parameterType);

                                return Expression.Call(toOrderedEnumerableMethodInfo, result);
                            }
                        }

                        return result;
                    }
            }

            return base.VisitExtension(extensionExpression);
        }

        private (Expression Operand, NavigationExpansionExpressionState State) ApplyIncludes(
            NavigationExpansionExpression navigationExpansionExpression)
        {
            var includeVisitor = new PendingSelectorIncludeVisitor();
            var rewrittenBody = includeVisitor.Visit(navigationExpansionExpression.State.PendingSelector.Body);

            if (navigationExpansionExpression.State.PendingSelector.Body != rewrittenBody)
            {
                navigationExpansionExpression.State.PendingSelector = Expression.Lambda(rewrittenBody, navigationExpansionExpression.State.PendingSelector.Parameters[0]);
                navigationExpansionExpression.State.ApplyPendingSelector = true;
            }

            if (includeVisitor.PendingIncludes.Count > 0)
            {
                var result = (Source: navigationExpansionExpression.Operand, Parameter: navigationExpansionExpression.State.CurrentParameter);
                foreach (var pendingIncludeNode in includeVisitor.PendingIncludes)
                {
                    result = NavigationExpansionHelpers.AddNavigationJoin(
                        result.Source,
                        result.Parameter,
                        pendingIncludeNode.SourceMapping,
                        pendingIncludeNode.NavTreeNode,
                        navigationExpansionExpression.State,
                        new List<INavigation>(),
                        include: true);
                }

                var pendingSelector = navigationExpansionExpression.State.PendingSelector;
                if (navigationExpansionExpression.State.CurrentParameter != result.Parameter)
                {
                    var pendingSelectorBody = new ExpressionReplacingVisitor(navigationExpansionExpression.State.CurrentParameter, result.Parameter).Visit(navigationExpansionExpression.State.PendingSelector.Body);
                    pendingSelector = Expression.Lambda(pendingSelectorBody, result.Parameter);
                }

                var newState = new NavigationExpansionExpressionState(
                    result.Parameter,
                    navigationExpansionExpression.State.SourceMappings,
                    pendingSelector,
                    applyPendingSelector: true,
                    navigationExpansionExpression.State.PendingOrderings,
                    navigationExpansionExpression.State.PendingIncludeChain,
                    navigationExpansionExpression.State.PendingCardinalityReducingOperator,
                    navigationExpansionExpression.State.CustomRootMappings,
                    navigationExpansionExpression.State.MaterializeCollectionNavigation);

                return (Operand: result.Source, newState);
            }

            return (navigationExpansionExpression.Operand, navigationExpansionExpression.State);
        }

        public static MethodInfo ToOrderedQueryableMethod = typeof(NavigationExpansionReducingVisitor).GetMethod(nameof(ToOrderedQueryable));

        public static IOrderedQueryable<TElement> ToOrderedQueryable<TElement>(IQueryable<TElement> source)
            => new IOrderedQueryableAdapter<TElement>(source);

        private class IOrderedQueryableAdapter<TElement> : IOrderedQueryable<TElement>
        {
            private readonly IQueryable<TElement> _source;

            public IOrderedQueryableAdapter(IQueryable<TElement> source)
            {
                _source = source;
            }

            public Type ElementType => _source.ElementType;

            public Expression Expression => _source.Expression;

            public IQueryProvider Provider => _source.Provider;

            public IEnumerator<TElement> GetEnumerator()
                => _source.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => ((IEnumerable)_source).GetEnumerator();
        }

        public static MethodInfo ToOrderedEnumerableMethod = typeof(NavigationExpansionReducingVisitor).GetMethod(nameof(ToOrderedEnumerable));

        public static IOrderedEnumerable<TElement> ToOrderedEnumerable<TElement>(IEnumerable<TElement> source)
            => new IOrderedEnumerableAdapter<TElement>(source);

        private class IOrderedEnumerableAdapter<TElement> : IOrderedEnumerable<TElement>
        {
            private readonly IEnumerable<TElement> _source;

            public IOrderedEnumerableAdapter(IEnumerable<TElement> source)
            {
                _source = source;
            }

            public IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
                => descending
                ? _source.OrderByDescending(keySelector, comparer)
                : _source.OrderBy(keySelector, comparer);

            public IEnumerator<TElement> GetEnumerator()
                => _source.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => ((IEnumerable)_source).GetEnumerator();
        }
    }
}
