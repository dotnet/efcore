// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query
{
    public class AsyncLinqOperatorProvider : ILinqOperatorProvider
    {
        private static readonly MethodInfo _interceptExceptions
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("_InterceptExceptions");

        [UsedImplicitly]
        private static IAsyncEnumerable<T> _InterceptExceptions<T>(
            Func<IAsyncEnumerable<T>> source, QueryContext queryContext)
        {
            return new ExceptionInterceptor<T>(source, queryContext);
        }

        public virtual MethodInfo InterceptExceptions => _interceptExceptions;

        private sealed class ExceptionInterceptor<T> : IAsyncEnumerable<T>
        {
            private readonly Func<IAsyncEnumerable<T>> _innerFactory;
            private readonly QueryContext _queryContext;

            public ExceptionInterceptor(Func<IAsyncEnumerable<T>> innerFactory, QueryContext queryContext)
            {
                _innerFactory = innerFactory;
                _queryContext = queryContext;
            }

            public IAsyncEnumerator<T> GetEnumerator()
            {
                return new EnumeratorExceptionInterceptor(this);
            }

            [DebuggerStepThrough]
            private sealed class EnumeratorExceptionInterceptor : IAsyncEnumerator<T>
            {
                private readonly ExceptionInterceptor<T> _exceptionInterceptor;

                private IAsyncEnumerator<T> _inner;

                public EnumeratorExceptionInterceptor(ExceptionInterceptor<T> exceptionInterceptor)
                {
                    _exceptionInterceptor = exceptionInterceptor;
                }

                public T Current => _inner.Current;

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    try
                    {
                        if (_inner == null)
                        {
                            _inner = _exceptionInterceptor._innerFactory().GetEnumerator();
                        }

                        return await _inner.MoveNext(cancellationToken).WithCurrentCulture();
                    }
                    catch (Exception e)
                    {
                        _exceptionInterceptor._queryContext.Logger.WriteError(
                            new DataStoreErrorLogState(_exceptionInterceptor._queryContext.ContextType),
                            e,
                            (state, exception) =>
                                Strings.LogExceptionDuringQueryIteration(Environment.NewLine, exception));

                        throw;
                    }
                }

                public void Dispose()
                {
                    _inner?.Dispose();
                }
            }
        }

        private static readonly MethodInfo _trackEntities
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("_TrackEntities");

        [UsedImplicitly]
        private static IAsyncEnumerable<TOut> _TrackEntities<TOut, TIn>(
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
                        foreach (var entity in entityAccessors
                            .Select(entityAccessor => entityAccessor((TIn)result))
                            .Where(entity => entity != null))
                        {
                            queryContext.QueryBuffer.StartTracking(entity);
                        }
                    }

                    return result;
                });
        }

        public virtual MethodInfo TrackEntities => _trackEntities;

        private static readonly MethodInfo _toSequence
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("_ToSequence");

        [UsedImplicitly]
        private static IAsyncEnumerable<T> _ToSequence<T>(T element)
        {
            return AsyncEnumerable.Return(element);
        }

        public virtual MethodInfo ToSequence => _toSequence;

        private static readonly MethodInfo _asQueryable
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("_AsQueryable");

        [UsedImplicitly]
        private static IOrderedQueryable<TSource> _AsQueryable<TSource>(IAsyncEnumerable<TSource> source)
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

            public Type ElementType => typeof(T);

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

        public virtual MethodInfo AsQueryable => _asQueryable;

        private static readonly MethodInfo _selectMany
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("_SelectMany");

        [UsedImplicitly]
        private static IAsyncEnumerable<TResult> _SelectMany<TSource, TResult>(
            IAsyncEnumerable<TSource> source, Func<TSource, IAsyncEnumerable<TResult>> selector)
        {
            return source.SelectMany(selector);
        }

        public virtual MethodInfo SelectMany => _selectMany;

        private static readonly MethodInfo _join
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("_Join");

        [UsedImplicitly]
        private static IAsyncEnumerable<TResult> _Join<TOuter, TInner, TKey, TResult>(
            IAsyncEnumerable<TOuter> outer,
            IAsyncEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector)
        {
            return outer.Join(inner, outerKeySelector, innerKeySelector, resultSelector);
        }

        public virtual MethodInfo Join => _join;

        private static readonly MethodInfo _groupJoin
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("_GroupJoin");

        [UsedImplicitly]
        private static IAsyncEnumerable<TResult> _GroupJoin<TOuter, TInner, TKey, TResult>(
            IAsyncEnumerable<TOuter> outer,
            IAsyncEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IAsyncEnumerable<TInner>, TResult> resultSelector)
        {
            return outer.GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector);
        }

        public virtual MethodInfo GroupJoin => _groupJoin;

        private static readonly MethodInfo _select
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("_Select");

        [UsedImplicitly]
        private static IAsyncEnumerable<TResult> _Select<TSource, TResult>(
            IAsyncEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.Select(selector);
        }

        public virtual MethodInfo Select => _select;

        private static readonly MethodInfo _orderBy
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("_OrderBy");

        [UsedImplicitly]
        private static IOrderedAsyncEnumerable<TSource> _OrderBy<TSource, TKey>(
            IAsyncEnumerable<TSource> source, Func<TSource, TKey> expression, OrderingDirection orderingDirection)
        {
            return orderingDirection == OrderingDirection.Asc
                ? source.OrderBy(expression)
                : source.OrderByDescending(expression);
        }

        public virtual MethodInfo OrderBy => _orderBy;

        private static readonly MethodInfo _thenBy
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("_ThenBy");

        [UsedImplicitly]
        private static IOrderedAsyncEnumerable<TSource> _ThenBy<TSource, TKey>(
            IOrderedAsyncEnumerable<TSource> source, Func<TSource, TKey> expression, OrderingDirection orderingDirection)
        {
            return orderingDirection == OrderingDirection.Asc
                ? source.ThenBy(expression)
                : source.ThenByDescending(expression);
        }

        public virtual MethodInfo ThenBy => _thenBy;

        private static readonly MethodInfo _where
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("_Where");

        [UsedImplicitly]
        private static IAsyncEnumerable<TSource> _Where<TSource>(
            IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return source.Where(predicate);
        }

        public virtual MethodInfo Where => _where;

        // Result operators

        private static readonly MethodInfo _any = GetMethod("Any");
        private static readonly MethodInfo _all = GetMethod("All", 1);
        private static readonly MethodInfo _cast = GetMethod("Cast");
        private static readonly MethodInfo _count = GetMethod("Count");
        private static readonly MethodInfo _contains = GetMethod("Contains", 1);
        private static readonly MethodInfo _defaultIfEmpty = GetMethod("DefaultIfEmpty");
        private static readonly MethodInfo _defaultIfEmptyArg = GetMethod("DefaultIfEmpty", 1);
        private static readonly MethodInfo _distinct = GetMethod("Distinct");
        private static readonly MethodInfo _first = GetMethod("First");
        private static readonly MethodInfo _firstOrDefault = GetMethod("FirstOrDefault");

        public virtual MethodInfo Any => _any;
        public virtual MethodInfo All => _all;
        public virtual MethodInfo Cast => _cast;
        public virtual MethodInfo Count => _count;
        public virtual MethodInfo Contains => _contains;
        public virtual MethodInfo DefaultIfEmpty => _defaultIfEmpty;
        public virtual MethodInfo DefaultIfEmptyArg => _defaultIfEmptyArg;
        public virtual MethodInfo Distinct => _distinct;

        public virtual MethodInfo First => _first;
        public virtual MethodInfo FirstOrDefault => _firstOrDefault;

        private static readonly MethodInfo _groupBy
            = typeof(AsyncLinqOperatorProvider).GetTypeInfo().GetDeclaredMethod("_GroupBy");

        [UsedImplicitly]
        private static IAsyncEnumerable<IAsyncGrouping<TKey, TElement>> _GroupBy<TSource, TKey, TElement>(
            IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return source.GroupBy(keySelector, elementSelector);
        }

        public virtual MethodInfo GroupBy => _groupBy;

        private static readonly MethodInfo _last = GetMethod("Last");
        private static readonly MethodInfo _lastOrDefault = GetMethod("LastOrDefault");
        private static readonly MethodInfo _longCount = GetMethod("LongCount");
        private static readonly MethodInfo _ofType = GetMethod("OfType");
        private static readonly MethodInfo _single = GetMethod("Single");
        private static readonly MethodInfo _singleOrDefault = GetMethod("SingleOrDefault");
        private static readonly MethodInfo _skip = GetMethod("Skip", 1);
        private static readonly MethodInfo _take = GetMethod("Take", 1);

        public virtual MethodInfo Last => _last;
        public virtual MethodInfo LastOrDefault => _lastOrDefault;
        public virtual MethodInfo LongCount => _longCount;
        public virtual MethodInfo OfType => _ofType;
        public virtual MethodInfo Single => _single;
        public virtual MethodInfo SingleOrDefault => _singleOrDefault;
        public virtual MethodInfo Skip => _skip;
        public virtual MethodInfo Take => _take;

        public virtual MethodInfo GetAggregateMethod(string methodName, Type elementType)
        {
            Check.NotEmpty(methodName, nameof(methodName));
            Check.NotNull(elementType, nameof(elementType));

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
            Check.NotNull(expression, nameof(expression));

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
                        _toAsyncEnumerable.MakeGenericMethod(elementType),
                        expression);
            }

            return expression;
        }

        private static readonly MethodInfo _toAsyncEnumerable
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("_ToAsyncEnumerable");

        [UsedImplicitly]
        private static IAsyncEnumerable<T> _ToAsyncEnumerable<T>(IEnumerable<T> source)
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
