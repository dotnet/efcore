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

// ReSharper disable InconsistentNaming

namespace Microsoft.Data.Entity.Query
{
    public class AsyncLinqOperatorProvider : ILinqOperatorProvider
    {
        public virtual MethodInfo UnwrapQueryResults => _unwrapQueryResults;

        private static readonly MethodInfo _unwrapQueryResults
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("UnwrapResults");

        [UsedImplicitly]
        private static IAsyncEnumerable<TResult> UnwrapResults<TResult>(
            IAsyncEnumerable<QuerySourceScope<TResult>> results)
        {
            // ReSharper disable once MergeConditionalExpression
            return results.Select(qss => qss != null ? qss.Result : default(TResult));
        }

        public virtual MethodInfo ToEnumerable => _toEnumerable;

        private static readonly MethodInfo _toEnumerable
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("_ToEnumerable");

        [UsedImplicitly]
        private static EnumerableAdapter<TResult> _ToEnumerable<TResult>(IAsyncEnumerable<TResult> results)
        {
            return new EnumerableAdapter<TResult>(results);
        }

        private class EnumerableAdapter<TResult> : IAsyncEnumerable<TResult>, IEnumerable<TResult>
        {
            protected readonly IAsyncEnumerable<TResult> _results;

            public EnumerableAdapter(IAsyncEnumerable<TResult> results)
            {
                _results = results;
            }

            IAsyncEnumerator<TResult> IAsyncEnumerable<TResult>.GetEnumerator()
            {
                return _results.GetEnumerator();
            }

            IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
            {
                return _results.ToEnumerable().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<TResult>)this).GetEnumerator();
            }
        }

        public virtual MethodInfo ToOrdered => _toOrdered;

        private static readonly MethodInfo _toOrdered
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("_ToOrdered");

        [UsedImplicitly]
        private static OrderedEnumerableAdapter<TResult> _ToOrdered<TResult>(IAsyncEnumerable<TResult> results)
        {
            return new OrderedEnumerableAdapter<TResult>(results);
        }

        private class OrderedEnumerableAdapter<TResult>
            : EnumerableAdapter<TResult>, IOrderedAsyncEnumerable<TResult>, IOrderedEnumerable<TResult>
        {
            public OrderedEnumerableAdapter(IAsyncEnumerable<TResult> results)
                : base(results)
            {
            }

            IOrderedAsyncEnumerable<TResult> IOrderedAsyncEnumerable<TResult>.CreateOrderedEnumerable<TKey>(
                Func<TResult, TKey> keySelector, IComparer<TKey> comparer, bool @descending)
            {
                return
                    !@descending
                        ? _results.OrderBy(keySelector, comparer)
                        : _results.OrderByDescending(keySelector, comparer);
            }

            IOrderedEnumerable<TResult> IOrderedEnumerable<TResult>.CreateOrderedEnumerable<TKey>(
                Func<TResult, TKey> keySelector, IComparer<TKey> comparer, bool descending)
            {
                return
                    !descending
                        ? _results.ToEnumerable().OrderBy(keySelector, comparer)
                        : _results.ToEnumerable().OrderByDescending(keySelector, comparer);
            }
        }

        public virtual MethodInfo UnwrapGrouping => _unwrapGrouping;

        private static readonly MethodInfo _unwrapGrouping
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("_UnwrapGrouping");

        [UsedImplicitly]
        private static IAsyncGrouping<TKey, TResult> _UnwrapGrouping<TKey, TResult>(
            IAsyncGrouping<TKey, QuerySourceScope<TResult>> grouping)
        {
            return new UnwrappingGrouping<TKey, TResult>(grouping);
        }

        private class UnwrappingGrouping<TKey, TResult> : IAsyncGrouping<TKey, TResult>
        {
            private readonly IAsyncGrouping<TKey, QuerySourceScope<TResult>> _grouping;

            public UnwrappingGrouping(IAsyncGrouping<TKey, QuerySourceScope<TResult>> grouping)
            {
                _grouping = grouping;
            }

            public TKey Key => _grouping.Key;

            public IAsyncEnumerator<TResult> GetEnumerator()
            {
                return _grouping.Select(qss => qss.Result).GetEnumerator();
            }
        }

        public virtual MethodInfo UnwrapGroupedQueryResults => _unwrapGroupedQueryResults;

        private static readonly MethodInfo _unwrapGroupedQueryResults
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("UnwrapGroupedResults");

        [UsedImplicitly]
        private static IAsyncEnumerable<IGrouping<TKey, TResult>> UnwrapGroupedResults<TKey, TResult>(
            IAsyncEnumerable<IAsyncGrouping<TKey, QuerySourceScope<TResult>>> groupings)
        {
            return groupings.Select(g => new AsyncGroupingAdapter<TKey, TResult>(g));
        }

        private class AsyncGroupingAdapter<TKey, TResult> : IGrouping<TKey, TResult>
        {
            private readonly IAsyncGrouping<TKey, QuerySourceScope<TResult>> _grouping;

            public AsyncGroupingAdapter(IAsyncGrouping<TKey, QuerySourceScope<TResult>> grouping)
            {
                _grouping = grouping;
            }

            public TKey Key => _grouping.Key;

            public IEnumerator<TResult> GetEnumerator()
            {
                return _grouping.Select(qss => qss.Result).ToEnumerable().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public virtual MethodInfo RewrapQueryResults => _rewrapQueryResults;

        private static readonly MethodInfo _rewrapQueryResults
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("RewrapResults");

        [UsedImplicitly]
        private static IAsyncEnumerable<QuerySourceScope<TResult>> RewrapResults<TResult>(
            IAsyncEnumerable<QuerySourceScope<TResult>> results,
            IQuerySource querySource)
        {
            return
                results.Select(qss =>
                    new QuerySourceScope<TResult>(
                        querySource,
                        // ReSharper disable once MergeConditionalExpression
                        qss != null ? qss.Result : default(TResult),
                        qss,
                        null));
        }

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
                        _exceptionInterceptor._queryContext.Logger.LogError(
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
        private static IAsyncEnumerable<QuerySourceScope<TOut>> _TrackEntities<TOut, TIn>(
            IAsyncEnumerable<QuerySourceScope<TOut>> results,
            QueryContext queryContext,
            IList<EntityTrackingInfo> entityTrackingInfos,
            IList<Func<TIn, object>> entityAccessors)
            where TIn : class
        {
            return results.Select(qss =>
                {
                    if (qss != null)
                    {
                        for (var i = 0; i < entityTrackingInfos.Count; i++)
                        {
                            var entity = entityAccessors[i](qss.Result as TIn);

                            if (entity != null)
                            {
                                var valueReader = qss.GetValueReader(entity);

                                if (valueReader != null)
                                {
                                    queryContext.StartTracking(
                                        entityTrackingInfos[i].EntityType,
                                        entity,
                                        valueReader);
                                }
                            }
                        }
                    }

                    return qss;
                });
        }

        public virtual MethodInfo TrackEntities => _trackEntities;

        private static readonly MethodInfo _trackGroupedEntities
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("_TrackGroupedEntities");

        [UsedImplicitly]
        private static IAsyncEnumerable<IAsyncGrouping<TKey, QuerySourceScope<TOut>>> _TrackGroupedEntities<TKey, TOut, TIn>(
            IAsyncEnumerable<IAsyncGrouping<TKey, QuerySourceScope<TOut>>> groupings,
            QueryContext queryContext,
            IList<EntityTrackingInfo> entityTrackingInfos,
            IList<Func<TIn, object>> entityAccessors)
            where TIn : class
        {
            return groupings
                .Select(g =>
                    new TrackingGrouping<TKey, TOut, TIn>(
                        g,
                        queryContext,
                        entityTrackingInfos,
                        entityAccessors));
        }

        public virtual MethodInfo TrackGroupedEntities => _trackGroupedEntities;

        private class TrackingGrouping<TKey, TOut, TIn> : IAsyncGrouping<TKey, QuerySourceScope<TOut>>
            where TIn : class
        {
            private readonly IAsyncGrouping<TKey, QuerySourceScope<TOut>> _grouping;
            private readonly QueryContext _queryContext;
            private readonly IList<EntityTrackingInfo> _entityTrackingInfos;
            private readonly IList<Func<TIn, object>> _entityAccessors;

            public TrackingGrouping(
                IAsyncGrouping<TKey, QuerySourceScope<TOut>> grouping,
                QueryContext queryContext,
                IList<EntityTrackingInfo> entityTrackingInfos,
                IList<Func<TIn, object>> entityAccessors)
            {
                _grouping = grouping;
                _queryContext = queryContext;
                _entityTrackingInfos = entityTrackingInfos;
                _entityAccessors = entityAccessors;
            }

            public TKey Key => _grouping.Key;

            public IAsyncEnumerator<QuerySourceScope<TOut>> GetEnumerator()
            {
                return _grouping.Select(qss =>
                    {
                        if (qss != null)
                        {
                            for (var i = 0; i < _entityTrackingInfos.Count; i++)
                            {
                                var entity = _entityAccessors[i](qss.Result as TIn);

                                if (entity != null)
                                {
                                    var valueReader = qss.GetValueReader(entity);

                                    if (valueReader != null)
                                    {
                                        _queryContext.StartTracking(
                                            _entityTrackingInfos[i].EntityType,
                                            entity,
                                            qss.GetValueReader(entity));
                                    }
                                }
                            }
                        }

                        return qss;
                    })
                    .GetEnumerator();
            }
        }

        private static readonly MethodInfo _toSequence
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("_ToSequence");

        [UsedImplicitly]
        private static IAsyncEnumerable<T> _ToSequence<T>(T element)
        {
            return new AsyncEnumerableAdapter<T>(new[] { element });
        }

        public virtual MethodInfo ToSequence => _toSequence;

        private static readonly MethodInfo _toQueryable
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("_ToQueryable");

        [UsedImplicitly]
        private static IOrderedQueryable<TSource> _ToQueryable<TSource>(IAsyncEnumerable<TSource> source)
        {
            return new AsyncQueryableAdapter<TSource>(source);
        }

        private sealed class AsyncQueryableAdapter<T> : IOrderedQueryable<T>
        {
            private readonly IAsyncEnumerable<T> _source;

            public AsyncQueryableAdapter(IAsyncEnumerable<T> source)
            {
                _source = source;
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
        }

        public virtual MethodInfo ToQueryable => _toQueryable;

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

        public virtual MethodInfo CastWrappedResult => _castWrappedResult;

        private static readonly MethodInfo _castWrappedResult
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("_CastWrappedResult");

        public static IAsyncEnumerable<QuerySourceScope<TResult>> _CastWrappedResult<TResult>(
            [NotNull] IAsyncEnumerable<QuerySourceScope> source)
        {
            Check.NotNull(source, nameof(source));

            return source.Select(qss =>
                new QuerySourceScope<TResult>(
                    qss.QuerySource,
                    (TResult)qss.UntypedResult,
                    qss,
                    null));
        }

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

        public virtual MethodInfo OfTypeWrappedResult => _ofTypeWrappedResult;

        private static readonly MethodInfo _ofTypeWrappedResult
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod("_OfTypeWrappedResult");

        public static IAsyncEnumerable<QuerySourceScope<TDerived>> _OfTypeWrappedResult<TBase, TDerived>(
            [NotNull] IAsyncEnumerable<QuerySourceScope<TBase>> source)
            where TDerived : TBase
        {
            Check.NotNull(source, nameof(source));

            return source
                .Where(qss => qss.Result is TDerived)
                .Select(qss =>
                    new QuerySourceScope<TDerived>(
                        qss.QuerySource,
                        (TDerived)qss.Result,
                        qss,
                        null));
        }

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
            return new AsyncEnumerableAdapter<T>(source);
        }

        private class AsyncEnumerableAdapter<T> : IAsyncEnumerable<T>
        {
            private readonly IEnumerable<T> _source;

            public AsyncEnumerableAdapter(IEnumerable<T> source)
            {
                _source = source;
            }

            public IAsyncEnumerator<T> GetEnumerator()
            {
                return new AsyncEnumeratorAdapter(_source.GetEnumerator());
            }

            private class AsyncEnumeratorAdapter : IAsyncEnumerator<T>
            {
                private readonly IEnumerator<T> _enumerator;

                public AsyncEnumeratorAdapter(IEnumerator<T> enumerator)
                {
                    _enumerator = enumerator;
                }

                public Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    return Task.FromResult(_enumerator.MoveNext());
                }

                public T Current => _enumerator.Current;

                public void Dispose()
                {
                    _enumerator.Dispose();
                }
            }
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
