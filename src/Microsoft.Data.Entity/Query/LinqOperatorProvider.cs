// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query
{
    public class LinqOperatorProvider : ILinqOperatorProvider
    {
        private static readonly MethodInfo _toSequenceShim
            = typeof(LinqOperatorProvider)
                .GetMethod("ToSequenceShim", BindingFlags.NonPublic | BindingFlags.Static);

        [UsedImplicitly]
        private static IEnumerable<T> ToSequenceShim<T>(T element)
        {
            return new[] { element };
        }

        public MethodInfo ToSequence
        {
            get { return _toSequenceShim; }
        }

        private static readonly MethodInfo _selectManyShim
            = typeof(LinqOperatorProvider)
                .GetMethod("SelectManyShim", BindingFlags.NonPublic | BindingFlags.Static);

        [UsedImplicitly]
        private static IEnumerable<TResult> SelectManyShim<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            return source.SelectMany(selector);
        }

        public MethodInfo SelectMany
        {
            get { return _selectManyShim; }
        }

        private static readonly MethodInfo _joinShim
            = typeof(LinqOperatorProvider)
                .GetMethod("JoinShim", BindingFlags.NonPublic | BindingFlags.Static);

        [UsedImplicitly]
        private static IEnumerable<TResult> JoinShim<TOuter, TInner, TKey, TResult>(
            IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
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
            = typeof(LinqOperatorProvider)
                .GetMethod("GroupJoinShim", BindingFlags.NonPublic | BindingFlags.Static);

        [UsedImplicitly]
        private static IEnumerable<TResult> GroupJoinShim<TOuter, TInner, TKey, TResult>(
            IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
        {
            return outer.GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector);
        }

        public MethodInfo GroupJoin
        {
            get { return _groupJoinShim; }
        }

        private static readonly MethodInfo _selectShim
            = typeof(LinqOperatorProvider)
                .GetMethod("SelectShim", BindingFlags.NonPublic | BindingFlags.Static);

        [UsedImplicitly]
        private static IEnumerable<TResult> SelectShim<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.Select(selector);
        }

        public MethodInfo Select
        {
            get { return _selectShim; }
        }

        private static readonly MethodInfo _orderByShim
            = typeof(LinqOperatorProvider)
                .GetMethod("OrderByShim", BindingFlags.NonPublic | BindingFlags.Static);

        [UsedImplicitly]
        private static IOrderedEnumerable<TSource> OrderByShim<TSource, TKey>(
            IEnumerable<TSource> source, Func<TSource, TKey> expression, OrderingDirection orderingDirection)
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
            = typeof(LinqOperatorProvider)
                .GetMethod("ThenByShim", BindingFlags.NonPublic | BindingFlags.Static);

        [UsedImplicitly]
        private static IOrderedEnumerable<TSource> ThenByShim<TSource, TKey>(
            IOrderedEnumerable<TSource> source, Func<TSource, TKey> expression, OrderingDirection orderingDirection)
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
            = typeof(LinqOperatorProvider)
                .GetMethod("WhereShim", BindingFlags.NonPublic | BindingFlags.Static);

        [UsedImplicitly]
        private static IEnumerable<TSource> WhereShim<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return source.Where(predicate);
        }

        public MethodInfo Where
        {
            get { return _whereShim; }
        }
    }
}
