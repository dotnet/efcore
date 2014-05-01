// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query
{
    public class AsyncLinqOperatorProvider : ILinqOperatorProvider
    {
        private static readonly MethodInfo _toSequenceShim
            = typeof(AsyncLinqOperatorProvider)
                .GetMethod("ToSequenceShim", BindingFlags.NonPublic | BindingFlags.Static);

        [UsedImplicitly]
        private static IAsyncEnumerable<T> ToSequenceShim<T>(T element)
        {
            return AsyncEnumerable.Return(element);
        }

        public MethodInfo ToSequence
        {
            get { return _toSequenceShim; }
        }

        private static readonly MethodInfo _selectManyShim
            = typeof(AsyncLinqOperatorProvider)
                .GetMethod("SelectManyShim", BindingFlags.NonPublic | BindingFlags.Static);

        [UsedImplicitly]
        private static IAsyncEnumerable<TResult> SelectManyShim<TSource, TResult>(
            IAsyncEnumerable<TSource> source, Func<TSource, IAsyncEnumerable<TResult>> selector)
        {
            return source.SelectMany(selector);
        }

        public MethodInfo SelectMany
        {
            get { return _selectManyShim; }
        }

        private static readonly MethodInfo _joinShim
            = typeof(AsyncLinqOperatorProvider)
                .GetMethod("JoinShim", BindingFlags.NonPublic | BindingFlags.Static);

        [UsedImplicitly]
        private static IAsyncEnumerable<TResult> JoinShim<TOuter, TInner, TKey, TResult>(
            IAsyncEnumerable<TOuter> outer,
            IAsyncEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector)
        {
            return outer.Join(inner, outerKeySelector, innerKeySelector, resultSelector);
        }

        public MethodInfo Join
        {
            get { return _joinShim; }
        }

        private static readonly MethodInfo _groupJoinShim
            = typeof(AsyncLinqOperatorProvider)
                .GetMethod("GroupJoinShim", BindingFlags.NonPublic | BindingFlags.Static);

        [UsedImplicitly]
        private static IAsyncEnumerable<TResult> GroupJoinShim<TOuter, TInner, TKey, TResult>(
            IAsyncEnumerable<TOuter> outer,
            IAsyncEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IAsyncEnumerable<TInner>, TResult> resultSelector)
        {
            return outer.GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector);
        }

        public MethodInfo GroupJoin
        {
            get { return _groupJoinShim; }
        }

        private static readonly MethodInfo _selectShim
            = typeof(AsyncLinqOperatorProvider)
                .GetMethod("SelectShim", BindingFlags.NonPublic | BindingFlags.Static);

        [UsedImplicitly]
        private static IAsyncEnumerable<TResult> SelectShim<TSource, TResult>(
            IAsyncEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.Select(selector);
        }

        public MethodInfo Select
        {
            get { return _selectShim; }
        }

        private static readonly MethodInfo _orderByShim
            = typeof(AsyncLinqOperatorProvider)
                .GetMethod("OrderByShim", BindingFlags.NonPublic | BindingFlags.Static);

        [UsedImplicitly]
        private static IOrderedAsyncEnumerable<TSource> OrderByShim<TSource, TKey>(
            IAsyncEnumerable<TSource> source, Func<TSource, TKey> expression, OrderingDirection orderingDirection)
        {
            return orderingDirection == OrderingDirection.Asc
                ? source.OrderBy(expression)
                : source.OrderByDescending(expression);
        }

        public MethodInfo OrderBy
        {
            get { return _orderByShim; }
        }

        private static readonly MethodInfo _thenByShim
            = typeof(AsyncLinqOperatorProvider)
                .GetMethod("ThenByShim", BindingFlags.NonPublic | BindingFlags.Static);

        [UsedImplicitly]
        private static IOrderedAsyncEnumerable<TSource> ThenByShim<TSource, TKey>(
            IOrderedAsyncEnumerable<TSource> source, Func<TSource, TKey> expression, OrderingDirection orderingDirection)
        {
            return orderingDirection == OrderingDirection.Asc
                ? source.ThenBy(expression)
                : source.ThenByDescending(expression);
        }

        public MethodInfo ThenBy
        {
            get { return _thenByShim; }
        }

        private static readonly MethodInfo _whereShim
            = typeof(AsyncLinqOperatorProvider)
                .GetMethod("WhereShim", BindingFlags.NonPublic | BindingFlags.Static);

        [UsedImplicitly]
        private static IAsyncEnumerable<TSource> WhereShim<TSource>(
            IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return source.Where(predicate);
        }

        public MethodInfo Where
        {
            get { return _whereShim; }
        }
    }
}
