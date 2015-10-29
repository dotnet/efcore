// Copyright (c) .NET Foundation. All rights reserved.
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
using Microsoft.Data.Entity.Extensions.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.Logging;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class AsyncLinqOperatorProvider : ILinqOperatorProvider
    {
        public virtual MethodInfo ToEnumerable => _toEnumerable;

        private static readonly MethodInfo _toEnumerable
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_ToEnumerable));

        [UsedImplicitly]
        internal static EnumerableAdapter<TResult> _ToEnumerable<TResult>(IAsyncEnumerable<TResult> results)
            => new EnumerableAdapter<TResult>(results);

        internal class EnumerableAdapter<TResult> : IAsyncEnumerable<TResult>, IEnumerable<TResult>
        {
            protected readonly IAsyncEnumerable<TResult> _results;

            public EnumerableAdapter(IAsyncEnumerable<TResult> results)
            {
                _results = results;
            }

            IAsyncEnumerator<TResult> IAsyncEnumerable<TResult>.GetEnumerator() => _results.GetEnumerator();

            IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator() => _results.ToEnumerable().GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<TResult>)this).GetEnumerator();
        }

        public virtual MethodInfo ToOrdered => _toOrdered;

        private static readonly MethodInfo _toOrdered
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_ToOrdered));

        [UsedImplicitly]
        internal static OrderedEnumerableAdapter<TResult> _ToOrdered<TResult>(IAsyncEnumerable<TResult> results)
            => new OrderedEnumerableAdapter<TResult>(results);

        internal class OrderedEnumerableAdapter<TResult>
            : EnumerableAdapter<TResult>, IOrderedAsyncEnumerable<TResult>, IOrderedEnumerable<TResult>
        {
            public OrderedEnumerableAdapter(IAsyncEnumerable<TResult> results)
                : base(results)
            {
            }

            IOrderedAsyncEnumerable<TResult> IOrderedAsyncEnumerable<TResult>.CreateOrderedEnumerable<TKey>(
                Func<TResult, TKey> keySelector, IComparer<TKey> comparer, bool descending)
                => !@descending
                    ? _results.OrderBy(keySelector, comparer)
                    : _results.OrderByDescending(keySelector, comparer);

            IOrderedEnumerable<TResult> IOrderedEnumerable<TResult>.CreateOrderedEnumerable<TKey>(
                Func<TResult, TKey> keySelector, IComparer<TKey> comparer, bool descending)
                => !@descending
                    ? _results.ToEnumerable().OrderBy(keySelector, comparer)
                    : _results.ToEnumerable().OrderByDescending(keySelector, comparer);
        }

        private static readonly MethodInfo _interceptExceptions
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_InterceptExceptions));

        [UsedImplicitly]
        internal static IAsyncEnumerable<T> _InterceptExceptions<T>(
            IAsyncEnumerable<T> source, Type contextType, ILogger logger)
            => new ExceptionInterceptor<T>(source, contextType, logger);

        public virtual MethodInfo InterceptExceptions => _interceptExceptions;

        private sealed class ExceptionInterceptor<T> : IAsyncEnumerable<T>
        {
            private readonly IAsyncEnumerable<T> _innerAsyncEnumerable;
            private readonly Type _contextType;
            private readonly ILogger _logger;

            public ExceptionInterceptor(
                IAsyncEnumerable<T> innerAsyncEnumerable, Type contextType, ILogger logger)
            {
                _innerAsyncEnumerable = innerAsyncEnumerable;
                _contextType = contextType;
                _logger = logger;
            }

            public IAsyncEnumerator<T> GetEnumerator() => new EnumeratorExceptionInterceptor(this);

            [DebuggerStepThrough]
            private sealed class EnumeratorExceptionInterceptor : IAsyncEnumerator<T>
            {
                private readonly ExceptionInterceptor<T> _exceptionInterceptor;
                private readonly IAsyncEnumerator<T> _innerEnumerator;

                public EnumeratorExceptionInterceptor(ExceptionInterceptor<T> exceptionInterceptor)
                {
                    _exceptionInterceptor = exceptionInterceptor;
                    _innerEnumerator = _exceptionInterceptor._innerAsyncEnumerable.GetEnumerator();
                }

                public T Current => _innerEnumerator.Current;

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    try
                    {
                        return await _innerEnumerator.MoveNext(cancellationToken);
                    }
                    catch (Exception exception)
                    {
                        _exceptionInterceptor._logger
                            .LogError(
                                CoreLoggingEventId.DatabaseError,
                                () => new DatabaseErrorLogState(_exceptionInterceptor._contextType),
                                exception,
                                e => CoreStrings.LogExceptionDuringQueryIteration(Environment.NewLine, e));

                        throw;
                    }
                }

                public void Dispose() => _innerEnumerator.Dispose();
            }
        }

        private static readonly MethodInfo _trackEntities
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_TrackEntities));

        [UsedImplicitly]
        internal static IAsyncEnumerable<TOut> _TrackEntities<TOut, TIn>(
            IAsyncEnumerable<TOut> results,
            QueryContext queryContext,
            IList<EntityTrackingInfo> entityTrackingInfos,
            IList<Func<TIn, object>> entityAccessors)
            where TIn : class
        {
            return results.Select(result =>
                {
                    if (result != null)
                    {
                        for (var i = 0; i < entityTrackingInfos.Count; i++)
                        {
                            var entity = entityAccessors[i](result as TIn);

                            if (entity != null)
                            {
                                queryContext.QueryBuffer
                                    .StartTracking(entity, entityTrackingInfos[i]);
                            }
                        }
                    }

                    return result;
                });
        }

        public virtual MethodInfo TrackEntities => _trackEntities;

        private static readonly MethodInfo _trackGroupedEntities
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_TrackGroupedEntities));

        [UsedImplicitly]
        internal static IAsyncEnumerable<TrackingGrouping<TKey, TOut, TIn>> _TrackGroupedEntities<TKey, TOut, TIn>(
            IAsyncEnumerable<IAsyncGrouping<TKey, TOut>> groupings,
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

        internal class TrackingGrouping<TKey, TOut, TIn> : IAsyncGrouping<TKey, TOut>, IGrouping<TKey, TOut>
            where TIn : class
        {
            private readonly IAsyncGrouping<TKey, TOut> _grouping;
            private readonly QueryContext _queryContext;
            private readonly IList<EntityTrackingInfo> _entityTrackingInfos;
            private readonly IList<Func<TIn, object>> _entityAccessors;

            public TrackingGrouping(
                IAsyncGrouping<TKey, TOut> grouping,
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

            public IAsyncEnumerator<TOut> GetEnumerator() => CreateTrackingEnumerable().GetEnumerator();

            private IAsyncEnumerable<TOut> CreateTrackingEnumerable()
            {
                return _grouping.Select(result =>
                    {
                        if (result != null)
                        {
                            for (var i = 0; i < _entityTrackingInfos.Count; i++)
                            {
                                var entity = _entityAccessors[i](result as TIn);

                                if (entity != null)
                                {
                                    _queryContext.QueryBuffer
                                        .StartTracking(entity, _entityTrackingInfos[i]);
                                }
                            }
                        }

                        return result;
                    });
            }

            IEnumerator<TOut> IEnumerable<TOut>.GetEnumerator()
                => CreateTrackingEnumerable().ToEnumerable().GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<TOut>)this).GetEnumerator();
        }

        private static readonly MethodInfo _toSequence
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_ToSequence));

        [UsedImplicitly]
        internal static IAsyncEnumerable<T> _ToSequence<T>(T element)
            => new AsyncEnumerableAdapter<T>(new[] { element });

        public virtual MethodInfo ToSequence => _toSequence;

        private static readonly MethodInfo _toQueryable
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_ToQueryable));

        [UsedImplicitly]
        internal static IOrderedQueryable<TSource> _ToQueryable<TSource>(IAsyncEnumerable<TSource> source)
            => new AsyncQueryableAdapter<TSource>(source);

        private sealed class AsyncQueryableAdapter<T> : IOrderedQueryable<T>
        {
            private readonly IAsyncEnumerable<T> _source;

            public AsyncQueryableAdapter(IAsyncEnumerable<T> source)
            {
                _source = source;
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator() => _source.ToEnumerable().GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

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
                .GetTypeInfo().GetDeclaredMethod(nameof(_SelectMany));

        [UsedImplicitly]
        private static IAsyncEnumerable<TResult> _SelectMany<TSource, TCollection, TResult>(
            IAsyncEnumerable<TSource> source,
            Func<TSource, IAsyncEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
            => source.SelectMany(collectionSelector, resultSelector);

        public virtual MethodInfo SelectMany => _selectMany;

        private static readonly MethodInfo _join
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_Join));

        [UsedImplicitly]
        internal static IAsyncEnumerable<TResult> _Join<TOuter, TInner, TKey, TResult>(
            IAsyncEnumerable<TOuter> outer,
            IAsyncEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector)
            => outer.Join(inner, outerKeySelector, innerKeySelector, resultSelector);

        public virtual MethodInfo Join => _join;

        private static readonly MethodInfo _groupJoin
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_GroupJoin));

        [UsedImplicitly]
        internal static IAsyncEnumerable<TResult> _GroupJoin<TOuter, TInner, TKey, TResult>(
            IAsyncEnumerable<TOuter> outer,
            IAsyncEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IAsyncEnumerable<TInner>, TResult> resultSelector)
            => outer.GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector);

        public virtual MethodInfo GroupJoin => _groupJoin;

        private static readonly MethodInfo _select
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_Select));

        [UsedImplicitly]
        internal static IAsyncEnumerable<TResult> _Select<TSource, TResult>(
            IAsyncEnumerable<TSource> source, Func<TSource, TResult> selector)
            => source.Select(selector);

        public virtual MethodInfo Select => _select;

        private static readonly MethodInfo _orderBy
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_OrderBy));

        [UsedImplicitly]
        internal static IOrderedAsyncEnumerable<TSource> _OrderBy<TSource, TKey>(
            IAsyncEnumerable<TSource> source, Func<TSource, TKey> expression, OrderingDirection orderingDirection)
            => orderingDirection == OrderingDirection.Asc
                ? source.OrderBy(expression)
                : source.OrderByDescending(expression);

        public virtual MethodInfo OrderBy => _orderBy;

        private static readonly MethodInfo _thenBy
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_ThenBy));

        [UsedImplicitly]
        internal static IOrderedAsyncEnumerable<TSource> _ThenBy<TSource, TKey>(
            IOrderedAsyncEnumerable<TSource> source, Func<TSource, TKey> expression, OrderingDirection orderingDirection)
            => orderingDirection == OrderingDirection.Asc
                ? source.ThenBy(expression)
                : source.ThenByDescending(expression);

        public virtual MethodInfo ThenBy => _thenBy;

        private static readonly MethodInfo _where
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_Where));

        [UsedImplicitly]
        internal static IAsyncEnumerable<TSource> _Where<TSource>(
            IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate) => source.Where(predicate);

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
            = typeof(AsyncLinqOperatorProvider).GetTypeInfo().GetDeclaredMethod(nameof(_GroupBy));

        [UsedImplicitly]
        private static IAsyncEnumerable<IAsyncGrouping<TKey, TElement>> _GroupBy<TSource, TKey, TElement>(
            IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
            => source.GroupBy(keySelector, elementSelector);

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

        public virtual Type MakeSequenceType(Type elementType)
            => typeof(IAsyncEnumerable<>)
                .MakeGenericType(Check.NotNull(elementType, nameof(elementType)));

        private static readonly MethodInfo _toAsyncEnumerable
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(ToAsyncEnumerable));

        public static IAsyncEnumerable<T> ToAsyncEnumerable<T>([NotNull] IEnumerable<T> source)
        {
            Check.NotNull(source, nameof(source));

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
                => new AsyncEnumeratorAdapter(_source.GetEnumerator());

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

                public void Dispose() => _enumerator.Dispose();
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
