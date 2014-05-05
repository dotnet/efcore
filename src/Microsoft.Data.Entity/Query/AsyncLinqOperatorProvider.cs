// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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
                .GetTypeInfo().GetDeclaredMethod("ToSequenceShim");

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
                .GetTypeInfo().GetDeclaredMethod("SelectManyShim");

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
                .GetTypeInfo().GetDeclaredMethod("JoinShim");

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
                .GetTypeInfo().GetDeclaredMethod("GroupJoinShim");

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
                .GetTypeInfo().GetDeclaredMethod("SelectShim");

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
                .GetTypeInfo().GetDeclaredMethod("OrderByShim");

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
                .GetTypeInfo().GetDeclaredMethod("ThenByShim");

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
                .GetTypeInfo().GetDeclaredMethod("WhereShim");

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
