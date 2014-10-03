// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query
{
    public class AsyncLinqOperatorProvider : ILinqOperatorProvider
    {
        private static readonly MethodInfo _trackEntitiesShim
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("TrackEntitiesShim");

        [UsedImplicitly]
        private static IAsyncEnumerable<TOut> TrackEntitiesShim<TOut, TIn>(
            IAsyncEnumerable<TOut> results,
            QueryContext queryContext,
            ICollection<Func<TIn, object>> entityAccessors)
            where TOut : class
            where TIn : TOut
        {
            return results.Select(result =>
                {
                    if (result != null)
                    {
                        foreach (var entityAccessor in entityAccessors)
                        {
                            var entity = entityAccessor((TIn)result);

                            if (entity != null)
                            {
                                queryContext.QueryBuffer.StartTracking(entity);
                            }
                        }
                    }

                    return result;
                });
        }

        public virtual MethodInfo TrackEntities
        {
            get { return _trackEntitiesShim; }
        }

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

        private static readonly MethodInfo _asQueryableShim
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("AsQueryableShim");

        [UsedImplicitly]
        private static IOrderedQueryable<TSource> AsQueryableShim<TSource>(IAsyncEnumerable<TSource> source)
        {
            return new AsyncQueryableAdapter<TSource>(source);
        }

        private sealed class AsyncQueryableAdapter<T> : IOrderedQueryable<T>, IAsyncEnumerable<T>, IOrderedEnumerable<T>
        {
            private readonly IAsyncEnumerable<T> _source;

            public AsyncQueryableAdapter(IAsyncEnumerable<T> source)
            {
                _source = source;
            }

            public IOrderedEnumerable<T> CreateOrderedEnumerable<TKey>(
                Func<T, TKey> keySelector, IComparer<TKey> comparer, bool descending)
            {
                return
                    !descending
                        ? _source.ToEnumerable().OrderBy(keySelector, comparer)
                        : _source.ToEnumerable().OrderByDescending(keySelector, comparer);
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return _source.ToEnumerable().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<T>)this).GetEnumerator();
            }

            public Type ElementType
            {
                get { return typeof(T); }
            }

            public Expression Expression
            {
                get { throw new NotImplementedException(); }
            }

            public IQueryProvider Provider
            {
                get { throw new NotImplementedException(); }
            }

            public IAsyncEnumerator<T> GetEnumerator()
            {
                return _source.GetEnumerator();
            }
        }

        public virtual MethodInfo AsQueryable
        {
            get { return _asQueryableShim; }
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
            = typeof(AsyncLinqOperatorProvider).GetTypeInfo().GetDeclaredMethod("_GroupBy");

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

            var aggregateMethods
                = typeof(AsyncEnumerable).GetTypeInfo().GetDeclaredMethods(methodName)
                    .Where(mi => mi.GetParameters().Length == 2
                                 && mi.GetParameters()[1].ParameterType == typeof(CancellationToken))
                    .ToList();

            return
                aggregateMethods
                    .SingleOrDefault(mi => mi.GetParameters()[0].ParameterType
                                           == typeof(IAsyncEnumerable<>).MakeGenericType(elementType))
                ?? aggregateMethods.Single(mi => mi.IsGenericMethod)
                    .MakeGenericMethod(elementType);
        }

        public virtual Expression AdjustSequenceType(Expression expression)
        {
            Check.NotNull(expression, "expression");

            if (expression.Type == typeof(string)
                || expression.Type == typeof(byte[]))
            {
                return expression;
            }

            var elementType
                = expression.Type.TryGetElementType(typeof(IEnumerable<>));

            if (elementType != null)
            {
                return
                    Expression.Call(
                        _toAsyncEnumerableShim.MakeGenericMethod(elementType),
                        expression);
            }

            return expression;
        }

        private static readonly MethodInfo _toAsyncEnumerableShim
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("ToAsyncEnumerableShim");

        [UsedImplicitly]
        private static IAsyncEnumerable<T> ToAsyncEnumerableShim<T>(IEnumerable<T> source)
        {
            return source.ToAsyncEnumerable();
        }

        private static MethodInfo GetMethod(string name, int parameterCount = 0)
        {
            var candidateMethods
                = typeof(AsyncEnumerable).GetTypeInfo().GetDeclaredMethods(name)
                    .ToList();

            return candidateMethods
                .SingleOrDefault(mi =>
                    (mi.GetParameters().Length == parameterCount + 2
                     && mi.GetParameters().Last().ParameterType == typeof(CancellationToken)))
                   ?? candidateMethods.Single(mi => mi.GetParameters().Length == parameterCount + 1);
        }
    }
}
