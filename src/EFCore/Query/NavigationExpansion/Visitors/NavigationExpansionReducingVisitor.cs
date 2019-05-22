// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Visitors
{
    public class NavigationExpansionReducingVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is NavigationBindingExpression navigationBindingExpression)
            {
                var result = navigationBindingExpression.RootParameter.BuildPropertyAccess(navigationBindingExpression.NavigationTreeNode.ToMapping);

                return result;
            }

            // TODO: temporary hack
            if (extensionExpression is IncludeExpression includeExpression)
            {
                var methodInfo = typeof(IncludeHelpers).GetMethod(nameof(IncludeHelpers.IncludeMethod))
                    .MakeGenericMethod(includeExpression.EntityExpression.Type);

                var newEntityExpression = Visit(includeExpression.EntityExpression);
                var newNavigationExpression = Visit(includeExpression.NavigationExpression);

                return Expression.Call(
                    methodInfo,
                    newEntityExpression,
                    newNavigationExpression,
                    Expression.Constant(includeExpression.Navigation));
            }

            if (extensionExpression is NavigationExpansionRootExpression navigationExpansionRootExpression)
            {
                return Visit(navigationExpansionRootExpression.Unwrap());
            }

            if (extensionExpression is NavigationExpansionExpression navigationExpansionExpression)
            {
                var includeResult = ApplyIncludes(navigationExpansionExpression);
                var state = includeResult.state;
                var result = Visit(includeResult.operand);

                if (!state.ApplyPendingSelector
                    && state.PendingOrderings.Count == 0
                    && state.PendingTags.Count == 0
                    && state.PendingCardinalityReducingOperator == null
                    && state.MaterializeCollectionNavigation == null)
                {
                    return result;
                }

                var parameter = Expression.Parameter(result.Type.GetSequenceType());

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

                    var pendingSelectMathod = result.Type.IsGenericType && (result.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>) || result.Type.GetGenericTypeDefinition() == typeof(IOrderedEnumerable<>))
                        ? LinqMethodHelpers.EnumerableSelectMethodInfo.MakeGenericMethod(parameter.Type, pendingSelectorBodyType)
                        : LinqMethodHelpers.QueryableSelectMethodInfo.MakeGenericMethod(parameter.Type, pendingSelectorBodyType);

                    result = Expression.Call(pendingSelectMathod, result, pendingSelector);
                    parameter = Expression.Parameter(result.Type.GetSequenceType());
                }

                if (state.PendingTags.Count > 0)
                {
                    var withTagMethodInfo = EntityFrameworkQueryableExtensions.TagWithMethodInfo.MakeGenericMethod(parameter.Type);
                    foreach (var pendingTag in state.PendingTags)
                    {
                        result = Expression.Call(withTagMethodInfo, result, Expression.Constant(pendingTag));
                    }
                }

                if (state.PendingCardinalityReducingOperator != null)
                {
                    var terminatingOperatorMethodInfo = state.PendingCardinalityReducingOperator.MakeGenericMethod(parameter.Type);

                    result = Expression.Call(terminatingOperatorMethodInfo, result);
                }

                if (state.MaterializeCollectionNavigation != null)
                {
                    var entityType = state.MaterializeCollectionNavigation.ClrType.IsGenericType
                        ? state.MaterializeCollectionNavigation.ClrType.GetGenericArguments()[0]
                        : state.MaterializeCollectionNavigation.GetTargetType().ClrType;

                    result = Expression.Call(
                        NavigationExpansionHelpers.MaterializeCollectionNavigationMethodInfo.MakeGenericMethod(
                            state.MaterializeCollectionNavigation.ClrType,
                            entityType),
                        result,
                        Expression.Constant(state.MaterializeCollectionNavigation));
                }

                if (navigationExpansionExpression.Type != result.Type && navigationExpansionExpression.Type.IsGenericType)
                {
                    if (navigationExpansionExpression.Type.GetGenericTypeDefinition() == typeof(IOrderedQueryable<>))
                    {
                        var toOrderedQueryableMethodInfo = ToOrderedQueryableMethod.MakeGenericMethod(parameter.Type);

                        return Expression.Call(toOrderedQueryableMethodInfo, result);
                    }
                    else if (navigationExpansionExpression.Type.GetGenericTypeDefinition() == typeof(IOrderedEnumerable<>))
                    {
                        var toOrderedEnumerableMethodInfo = ToOrderedEnumerableMethod.MakeGenericMethod(parameter.Type);

                        return Expression.Call(toOrderedEnumerableMethodInfo, result);
                    }
                    else if (navigationExpansionExpression.Type.GetGenericTypeDefinition() == typeof(IIncludableQueryable<,>))
                    {
                        // TODO: handle this using adapter, just like we do for order by?
                        return Expression.Convert(result, navigationExpansionExpression.Type);
                    }
                }

                return result;
            }

            return base.VisitExtension(extensionExpression);
        }

        private (Expression operand, NavigationExpansionExpressionState state) ApplyIncludes(NavigationExpansionExpression navigationExpansionExpression)
        {
            var includeFinder = new PendingIncludeFindingVisitor();
            includeFinder.Visit(navigationExpansionExpression.State.PendingSelector.Body);

            var includeRewriter = new PendingSelectorIncludeRewriter();
            var rewrittenBody = includeRewriter.Visit(navigationExpansionExpression.State.PendingSelector.Body);

            if (navigationExpansionExpression.State.PendingSelector.Body != rewrittenBody)
            {
                navigationExpansionExpression.State.PendingSelector = Expression.Lambda(rewrittenBody, navigationExpansionExpression.State.PendingSelector.Parameters[0]);
                navigationExpansionExpression.State.ApplyPendingSelector = true;
            }

            if (includeFinder.PendingIncludes.Count > 0)
            {
                var result = (source: navigationExpansionExpression.Operand, parameter: navigationExpansionExpression.State.CurrentParameter);
                foreach (var pendingIncludeNode in includeFinder.PendingIncludes)
                {
                    result = NavigationExpansionHelpers.AddNavigationJoin(
                        result.source,
                        result.parameter,
                        pendingIncludeNode.Value,
                        pendingIncludeNode.Key,
                        navigationExpansionExpression.State,
                        new List<INavigation>(),
                        include: true);
                }

                var pendingSelector = navigationExpansionExpression.State.PendingSelector;
                if (navigationExpansionExpression.State.CurrentParameter != result.parameter)
                {
                    var pendingSelectorBody = new ExpressionReplacingVisitor(navigationExpansionExpression.State.CurrentParameter, result.parameter).Visit(navigationExpansionExpression.State.PendingSelector.Body);
                    pendingSelector = Expression.Lambda(pendingSelectorBody, result.parameter);
                }

                var newState = new NavigationExpansionExpressionState(
                    result.parameter,
                    navigationExpansionExpression.State.SourceMappings,
                    pendingSelector,
                    applyPendingSelector: true,
                    navigationExpansionExpression.State.PendingOrderings,
                    navigationExpansionExpression.State.PendingIncludeChain,
                    navigationExpansionExpression.State.PendingCardinalityReducingOperator,
                    navigationExpansionExpression.State.PendingTags,
                    navigationExpansionExpression.State.CustomRootMappings,
                    navigationExpansionExpression.State.MaterializeCollectionNavigation);

                return (operand: result.source, state: newState);
            }

            return (operand: navigationExpansionExpression.Operand, state: navigationExpansionExpression.State);
        }

        public static MethodInfo ToOrderedQueryableMethod = typeof(NavigationExpansionReducingVisitor).GetMethod(nameof(ToOrderedQueryable));

        public static IOrderedQueryable<TElement> ToOrderedQueryable<TElement>(IQueryable<TElement> source)
            => new IOrderedQueryableAdapter<TElement>(source);

        private class IOrderedQueryableAdapter<TElement> : IOrderedQueryable<TElement>
        {
            IQueryable<TElement> _source;

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
            IEnumerable<TElement> _source;

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
