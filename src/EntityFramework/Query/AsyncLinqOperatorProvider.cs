// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
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

        public virtual MethodInfo ToSequence
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

        public virtual MethodInfo SelectMany
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

        public virtual MethodInfo Join
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

        public virtual MethodInfo GroupJoin
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

        public virtual MethodInfo Select
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

        public virtual MethodInfo OrderBy
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

        public virtual MethodInfo ThenBy
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

        public virtual MethodInfo Where
        {
            get { return _whereShim; }
        }
        
        // TODO: Replace with First when IX-Async dispose bug is fixed.
        public virtual MethodInfo _First
        {
            get { return _single; }
        }

        // Result operators

        private static readonly MethodInfo _any = GetMethod("Any");
        private static readonly MethodInfo _all = GetMethod("All", 1);
        private static readonly MethodInfo _cast = GetMethod("Cast");
        private static readonly MethodInfo _count = GetMethod("Count");
        private static readonly MethodInfo _defaultIfEmpty = GetMethod("DefaultIfEmpty");
        private static readonly MethodInfo _defaultIfEmptyArg = GetMethod("DefaultIfEmpty", 1);
        private static readonly MethodInfo _distinct = GetMethod("Distinct");
        private static readonly MethodInfo _first = GetMethod("First");
        private static readonly MethodInfo _firstOrDefault = GetMethod("FirstOrDefault");

        public virtual MethodInfo Any
        {
            get { return _any; }
        }

        public virtual MethodInfo All
        {
            get { return _all; }
        }

        public virtual MethodInfo Cast
        {
            get { return _cast; }
        }

        public virtual MethodInfo Count
        {
            get { return _count; }
        }

        public virtual MethodInfo DefaultIfEmpty
        {
            get { return _defaultIfEmpty; }
        }

        public virtual MethodInfo DefaultIfEmptyArg
        {
            get { return _defaultIfEmptyArg; }
        }

        public virtual MethodInfo Distinct
        {
            get { return _distinct; }
        }

        public virtual MethodInfo First
        {
            get { return _first; }
        }

        public virtual MethodInfo FirstOrDefault
        {
            get { return _firstOrDefault; }
        }

        private static readonly MethodInfo _groupBy
            = typeof(LinqOperatorProvider).GetTypeInfo().GetDeclaredMethod("_GroupBy");

        [UsedImplicitly]
        private static IAsyncEnumerable<IAsyncGrouping<TKey, TElement>> _GroupBy<TSource, TKey, TElement>(
            IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return source.GroupBy(keySelector, elementSelector);
        }

        public virtual MethodInfo GroupBy
        {
            get { return _groupBy; }
        }

        private static readonly MethodInfo _last = GetMethod("Last");
        private static readonly MethodInfo _lastOrDefault = GetMethod("LastOrDefault");
        private static readonly MethodInfo _longCount = GetMethod("LongCount");
        private static readonly MethodInfo _single = GetMethod("Single");
        private static readonly MethodInfo _singleOrDefault = GetMethod("SingleOrDefault");
        private static readonly MethodInfo _skip = GetMethod("Skip", 1);
        private static readonly MethodInfo _take = GetMethod("Take", 1);

        public virtual MethodInfo Last
        {
            get { return _last; }
        }

        public virtual MethodInfo LastOrDefault
        {
            get { return _lastOrDefault; }
        }

        public virtual MethodInfo LongCount
        {
            get { return _longCount; }
        }

        public virtual MethodInfo Single
        {
            get { return _single; }
        }

        public virtual MethodInfo SingleOrDefault
        {
            get { return _singleOrDefault; }
        }

        public virtual MethodInfo Skip
        {
            get { return _skip; }
        }

        public virtual MethodInfo Take
        {
            get { return _take; }
        }

        public virtual MethodInfo GetAggregateMethod(string methodName, Type elementType)
        {
            Check.NotEmpty(methodName, "methodName");
            Check.NotNull(elementType, "elementType");

            var aggregateMethods = GetMethods(methodName).ToList();

            return
                aggregateMethods
                    .FirstOrDefault(mi => mi.GetParameters()[0].ParameterType
                                          == typeof(IEnumerable<>).MakeGenericType(elementType))
                ?? aggregateMethods.Single(mi => mi.IsGenericMethod)
                    .MakeGenericMethod(elementType);
        }

        private static MethodInfo GetMethod(string name, int parameterCount = 0)
        {
            return GetMethods(name, parameterCount).Single();
        }

        private static IEnumerable<MethodInfo> GetMethods(string name, int parameterCount = 0)
        {
            return typeof(AsyncEnumerable).GetTypeInfo().GetDeclaredMethods(name)
                .Where(mi => mi.GetParameters().Length == parameterCount + 1);
        }
    }
}
