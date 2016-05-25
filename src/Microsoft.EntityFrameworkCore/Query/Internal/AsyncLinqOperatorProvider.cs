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
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class AsyncLinqOperatorProvider : ILinqOperatorProvider
    {
        public virtual MethodInfo ToEnumerable => _toEnumerable;

        private static readonly MethodInfo _toEnumerable
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_ToEnumerable));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static EnumerableAdapter<TResult> _ToEnumerable<TResult>(IAsyncEnumerable<TResult> results)
            => new EnumerableAdapter<TResult>(results);

        internal class EnumerableAdapter<TResult> : IAsyncEnumerable<TResult>, IEnumerable<TResult>
        {
            protected readonly IAsyncEnumerable<TResult> Results;

            public EnumerableAdapter(IAsyncEnumerable<TResult> results)
            {
                Results = results;
            }

            IAsyncEnumerator<TResult> IAsyncEnumerable<TResult>.GetEnumerator() => Results.GetEnumerator();

            IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator() => Results.ToEnumerable().GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<TResult>)this).GetEnumerator();
        }

        public virtual MethodInfo ToOrdered => _toOrdered;

        private static readonly MethodInfo _toOrdered
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_ToOrdered));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static OrderedEnumerableAdapter<TResult> _ToOrdered<TResult>(IAsyncEnumerable<TResult> results)
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
                => !descending
                    ? Results.OrderBy(keySelector, comparer)
                    : Results.OrderByDescending(keySelector, comparer);

            IOrderedEnumerable<TResult> IOrderedEnumerable<TResult>.CreateOrderedEnumerable<TKey>(
                Func<TResult, TKey> keySelector, IComparer<TKey> comparer, bool descending)
                => !descending
                    ? Results.ToEnumerable().OrderBy(keySelector, comparer)
                    : Results.ToEnumerable().OrderByDescending(keySelector, comparer);
        }

        private static readonly MethodInfo _interceptExceptions
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_InterceptExceptions));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IAsyncEnumerable<T> _InterceptExceptions<T>(
            IAsyncEnumerable<T> source, Type contextType, ILogger logger, QueryContext queryContext)
            => new ExceptionInterceptor<T>(source, contextType, logger, queryContext);

        public virtual MethodInfo InterceptExceptions => _interceptExceptions;

        private sealed class ExceptionInterceptor<T> : IAsyncEnumerable<T>
        {
            private readonly IAsyncEnumerable<T> _innerAsyncEnumerable;
            private readonly Type _contextType;
            private readonly ILogger _logger;
            private readonly QueryContext _queryContext;

            public ExceptionInterceptor(
                IAsyncEnumerable<T> innerAsyncEnumerable, Type contextType, ILogger logger, QueryContext queryContext)
            {
                _innerAsyncEnumerable = innerAsyncEnumerable;
                _contextType = contextType;
                _logger = logger;
                _queryContext = queryContext;
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
                    using (await _exceptionInterceptor._queryContext.ConcurrencyDetector
                        .EnterCriticalSectionAsync(cancellationToken))
                    {
                        try
                        {
                            // TODO remove this when/if bug is resolved in Ix-Async https://github.com/Reactive-Extensions/Rx.NET/issues/166
                            cancellationToken.ThrowIfCancellationRequested();

                            return await _innerEnumerator.MoveNext(cancellationToken);
                        }
                        catch (Exception exception)
                        {
                            _exceptionInterceptor._logger
                                .LogError(
                                    CoreEventId.DatabaseError,
                                    () => new DatabaseErrorLogState(_exceptionInterceptor._contextType),
                                    exception,
                                    e => CoreStrings.LogExceptionDuringQueryIteration(Environment.NewLine, e));

                            throw;
                        }
                    }
                }

                public void Dispose() => _innerEnumerator.Dispose();
            }
        }

        private static readonly MethodInfo _trackEntities
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_TrackEntities));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IAsyncEnumerable<TOut> _TrackEntities<TOut, TIn>(
            IAsyncEnumerable<TOut> results,
            QueryContext queryContext,
            IList<EntityTrackingInfo> entityTrackingInfos,
            IList<Func<TIn, object>> entityAccessors)
            where TIn : class
        {
            queryContext.BeginTrackingQuery();

            return _Select(
                results,
                result =>
                {
                    if (result != null)
                    {
                        for (var i = 0; i < entityTrackingInfos.Count; i++)
                        {
                            var entityOrCollection = entityAccessors[i](result as TIn);

                            if (entityOrCollection != null)
                            {
                                var entityTrackingInfo = entityTrackingInfos[i];

                                if (entityTrackingInfo.IsEnumerableTarget)
                                {
                                    foreach (var entity in (IEnumerable)entityOrCollection)
                                    {
                                        queryContext.StartTracking(entity, entityTrackingInfos[i]);
                                    }
                                }
                                else
                                {
                                    queryContext.StartTracking(entityOrCollection, entityTrackingInfos[i]);
                                }
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
        // ReSharper disable once InconsistentNaming
        private static IAsyncEnumerable<TrackingGrouping<TKey, TOut, TIn>> _TrackGroupedEntities<TKey, TOut, TIn>(
            IAsyncEnumerable<IGrouping<TKey, TOut>> groupings,
            QueryContext queryContext,
            IList<EntityTrackingInfo> entityTrackingInfos,
            IList<Func<TIn, object>> entityAccessors)
            where TIn : class
        {
            return _Select(
                groupings,
                g => new TrackingGrouping<TKey, TOut, TIn>(
                    g,
                    queryContext,
                    entityTrackingInfos,
                    entityAccessors));
        }

        public virtual MethodInfo TrackGroupedEntities => _trackGroupedEntities;

        internal class TrackingGrouping<TKey, TOut, TIn> : IGrouping<TKey, TOut>
            where TIn : class
        {
            private readonly IGrouping<TKey, TOut> _grouping;
            private readonly QueryContext _queryContext;
            private readonly IList<EntityTrackingInfo> _entityTrackingInfos;
            private readonly IList<Func<TIn, object>> _entityAccessors;

            public TrackingGrouping(
                IGrouping<TKey, TOut> grouping,
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

            IEnumerator<TOut> IEnumerable<TOut>.GetEnumerator()
            {
                _queryContext.BeginTrackingQuery();

                foreach (var result in _grouping)
                {
                    if (result != null)
                    {
                        for (var i = 0; i < _entityTrackingInfos.Count; i++)
                        {
                            var entity = _entityAccessors[i](result as TIn);

                            if (entity != null)
                            {
                                _queryContext.StartTracking(entity, _entityTrackingInfos[i]);
                            }
                        }
                    }

                    yield return result;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<TOut>)this).GetEnumerator();
        }

        private static readonly MethodInfo _toSequence
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_ToSequence));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        internal static IAsyncEnumerable<T> _ToSequence<T>(Task<T> task)
            => new TaskResultAsyncEnumerable<T>(task);

        public virtual MethodInfo ToSequence => _toSequence;

        private static readonly MethodInfo _toQueryable
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_ToQueryable));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IOrderedQueryable<TSource> _ToQueryable<TSource>(IAsyncEnumerable<TSource> source)
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
        // ReSharper disable once InconsistentNaming
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
        // ReSharper disable once InconsistentNaming
        private static IAsyncEnumerable<TResult> _Join<TOuter, TInner, TKey, TResult>(
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
        // ReSharper disable once InconsistentNaming
        private static IAsyncEnumerable<TResult> _GroupJoin<TOuter, TInner, TKey, TResult>(
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

        // ReSharper disable once InconsistentNaming
        public static IAsyncEnumerable<TResult> _Select<TSource, TResult>(
            [NotNull] IAsyncEnumerable<TSource> source, [NotNull] Func<TSource, TResult> selector)
            => new SelectAsyncEnumerable<TSource, TResult>(source, selector);

        private sealed class SelectAsyncEnumerable<TSource, TResult> : IAsyncEnumerable<TResult>
        {
            private readonly IAsyncEnumerable<TSource> _source;
            private readonly Func<TSource, TResult> _selector;

            public SelectAsyncEnumerable(
                IAsyncEnumerable<TSource> source, Func<TSource, TResult> selector)
            {
                _source = source;
                _selector = selector;
            }

            public IAsyncEnumerator<TResult> GetEnumerator()
                => new SelectAsyncEnumerator(_source.GetEnumerator(), _selector);

            private sealed class SelectAsyncEnumerator : IAsyncEnumerator<TResult>
            {
                private readonly IAsyncEnumerator<TSource> _source;
                private readonly Func<TSource, TResult> _selector;

                public SelectAsyncEnumerator(
                    IAsyncEnumerator<TSource> asyncEnumerator,
                    Func<TSource, TResult> selector)
                {
                    _source = asyncEnumerator;
                    _selector = selector;
                }

                public void Dispose() => _source.Dispose();

                public Task<bool> MoveNext(CancellationToken cancellationToken)
                    => _source.MoveNext(cancellationToken);

                public TResult Current => _selector(_source.Current);
            }
        }

        public virtual MethodInfo Select => _select;

        private static readonly MethodInfo _orderBy
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_OrderBy));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IOrderedAsyncEnumerable<TSource> _OrderBy<TSource, TKey>(
            IAsyncEnumerable<TSource> source, Func<TSource, TKey> expression, OrderingDirection orderingDirection)
            => orderingDirection == OrderingDirection.Asc
                ? source.OrderBy(expression)
                : source.OrderByDescending(expression);

        public virtual MethodInfo OrderBy => _orderBy;

        private static readonly MethodInfo _thenBy
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_ThenBy));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IOrderedAsyncEnumerable<TSource> _ThenBy<TSource, TKey>(
            IOrderedAsyncEnumerable<TSource> source, Func<TSource, TKey> expression, OrderingDirection orderingDirection)
            => orderingDirection == OrderingDirection.Asc
                ? source.ThenBy(expression)
                : source.ThenByDescending(expression);

        public virtual MethodInfo ThenBy => _thenBy;

        private static readonly MethodInfo _where
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_Where));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IAsyncEnumerable<TSource> _Where<TSource>(
            IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate) => source.Where(predicate);

        public virtual MethodInfo Where => _where;

        // Result operators

        private static readonly MethodInfo _any 
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_Any));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static async Task<bool> _Any<TSource>(
            IAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            using (var asyncEnumerator = source.GetEnumerator())
            {
                return await asyncEnumerator.MoveNext(cancellationToken);
            }
        }

        public virtual MethodInfo Any => _any;
        
        private static readonly MethodInfo _all 
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_All));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static async Task<bool> _All<TSource>(
            IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate, CancellationToken cancellationToken)
        {
            using (var asyncEnumerator = source.GetEnumerator())
            {
                while (await asyncEnumerator.MoveNext(cancellationToken))
                {
                    if (!predicate(asyncEnumerator.Current))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public virtual MethodInfo All => _all;
        
        private static readonly MethodInfo _cast = GetMethod("Cast");
        private static readonly MethodInfo _count = GetMethod("Count");
        private static readonly MethodInfo _contains = GetMethod("Contains", 1);
        private static readonly MethodInfo _defaultIfEmpty = GetMethod("DefaultIfEmpty");
        private static readonly MethodInfo _defaultIfEmptyArg = GetMethod("DefaultIfEmpty", 1);
        private static readonly MethodInfo _distinct = GetMethod("Distinct");

        private static readonly MethodInfo _first 
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_First));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static async Task<TSource> _First<TSource>(
            IAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            using (var asyncEnumerator = source.GetEnumerator())
            {
                if (await asyncEnumerator.MoveNext(cancellationToken))
                {
                    return asyncEnumerator.Current;
                }
            }

            throw new InvalidOperationException(CoreStrings.NoElements);
        }

        public virtual MethodInfo First => _first;

        private static readonly MethodInfo _firstOrDefault
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_FirstOrDefault));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static async Task<TSource> _FirstOrDefault<TSource>(
            IAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            using (var asyncEnumerator = source.GetEnumerator())
            {
                if (await asyncEnumerator.MoveNext(cancellationToken))
                {
                    return asyncEnumerator.Current;
                }
            }

            return default(TSource);
        }

        public virtual MethodInfo FirstOrDefault => _firstOrDefault;
        
        public virtual MethodInfo Cast => _cast;
        public virtual MethodInfo Count => _count;
        public virtual MethodInfo Contains => _contains;
        public virtual MethodInfo DefaultIfEmpty => _defaultIfEmpty;
        public virtual MethodInfo DefaultIfEmptyArg => _defaultIfEmptyArg;
        public virtual MethodInfo Distinct => _distinct;
        
        private static readonly MethodInfo _groupBy
            = typeof(AsyncLinqOperatorProvider).GetTypeInfo().GetDeclaredMethod(nameof(_GroupBy));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IAsyncEnumerable<IGrouping<TKey, TElement>> _GroupBy<TSource, TKey, TElement>(
            IAsyncEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector)
            => new GroupByAsyncEnumerable<TSource, TKey, TElement>(source, keySelector, elementSelector);

        internal sealed class GroupByAsyncEnumerable<TSource, TKey, TElement> : IAsyncEnumerable<IGrouping<TKey, TElement>>
        {
            private readonly IAsyncEnumerable<TSource> _source;
            private readonly Func<TSource, TKey> _keySelector;
            private readonly Func<TSource, TElement> _elementSelector;

            public GroupByAsyncEnumerable(
                IAsyncEnumerable<TSource> source,
                Func<TSource, TKey> keySelector,
                Func<TSource, TElement> elementSelector)
            {
                _source = source;
                _keySelector = keySelector;
                _elementSelector = elementSelector;
            }

            public IAsyncEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
                => new GroupByEnumerator(this);

            private sealed class GroupByEnumerator : IAsyncEnumerator<IGrouping<TKey, TElement>>
            {
                private readonly GroupByAsyncEnumerable<TSource, TKey, TElement> _groupByAsyncEnumerable;

                private IEnumerator<Grouping<TKey, TElement>> _groupsEnumerator;

                public GroupByEnumerator(GroupByAsyncEnumerable<TSource, TKey, TElement> groupByAsyncEnumerable)
                {
                    _groupByAsyncEnumerable = groupByAsyncEnumerable;
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (_groupsEnumerator == null)
                    {
                        var groups = new Dictionary<TKey, Grouping<TKey, TElement>>();

                        using (var sourceEnumerator = _groupByAsyncEnumerable._source.GetEnumerator())
                        {
                            while (await sourceEnumerator.MoveNext(cancellationToken))
                            {
                                var key = _groupByAsyncEnumerable._keySelector(sourceEnumerator.Current);
                                var element = _groupByAsyncEnumerable._elementSelector(sourceEnumerator.Current);

                                Grouping<TKey, TElement> grouping;
                                if (!groups.TryGetValue(key, out grouping))
                                {
                                    groups.Add(key, grouping = new Grouping<TKey, TElement>(key));
                                }

                                grouping.Add(element);
                            }
                        }

                        _groupsEnumerator = groups.Values.GetEnumerator();
                    }

                    return _groupsEnumerator.MoveNext();
                }

                public IGrouping<TKey, TElement> Current => _groupsEnumerator?.Current;

                public void Dispose() => _groupsEnumerator?.Dispose();
            }
        }

        public virtual MethodInfo GroupBy => _groupBy;

        private static readonly MethodInfo _last 
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_Last));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static async Task<TSource> _Last<TSource>(
            IAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            using (var asyncEnumerator = source.GetEnumerator())
            {
                if (await asyncEnumerator.MoveNext(cancellationToken))
                {
                    TSource result;
                    do
                    {
                        result = asyncEnumerator.Current;
                    }
                    while (await asyncEnumerator.MoveNext(cancellationToken));

                    return result;
                }
            }

            throw new InvalidOperationException(CoreStrings.NoElements);
        }

        public virtual MethodInfo Last => _last;

        private static readonly MethodInfo _lastOrDefault
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_LastOrDefault));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static async Task<TSource> _LastOrDefault<TSource>(
            IAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            using (var asyncEnumerator = source.GetEnumerator())
            {
                if (await asyncEnumerator.MoveNext(cancellationToken))
                {
                    TSource result;
                    do
                    {
                        result = asyncEnumerator.Current;
                    }
                    while (await asyncEnumerator.MoveNext(cancellationToken));

                    return result;
                }
            }

            return default(TSource);
        }

        public virtual MethodInfo LastOrDefault => _lastOrDefault;

        private static readonly MethodInfo _longCount = GetMethod("LongCount");
        private static readonly MethodInfo _ofType = GetMethod("OfType");
        private static readonly MethodInfo _skip = GetMethod("Skip", 1);
        private static readonly MethodInfo _take = GetMethod("Take", 1);

        public virtual MethodInfo LongCount => _longCount;
        public virtual MethodInfo OfType => _ofType;

        private static readonly MethodInfo _single 
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_Single));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static async Task<TSource> _Single<TSource>(
            IAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            using (var asyncEnumerator = source.GetEnumerator())
            {
                if (!await asyncEnumerator.MoveNext(cancellationToken))
                {
                    throw new InvalidOperationException(CoreStrings.NoElements);
                }

                var item = asyncEnumerator.Current;

                if (!await asyncEnumerator.MoveNext(cancellationToken))
                {
                    return item;
                }
            }

            throw new InvalidOperationException(CoreStrings.MoreThanOneElement);
        }

        public virtual MethodInfo Single => _single;

        private static readonly MethodInfo _singleOrDefault
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_SingleOrDefault));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static async Task<TSource> _SingleOrDefault<TSource>(
            IAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            using (var asyncEnumerator = source.GetEnumerator())
            {
                if (!await asyncEnumerator.MoveNext(cancellationToken))
                {
                    return default(TSource);
                }

                var item = asyncEnumerator.Current;

                if (!await asyncEnumerator.MoveNext(cancellationToken))
                {
                    return item;
                }
            }

            throw new InvalidOperationException(CoreStrings.MoreThanOneElement);
        }

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

        public virtual Type MakeSequenceType(Type elementType)
            => typeof(IAsyncEnumerable<>)
                .MakeGenericType(Check.NotNull(elementType, nameof(elementType)));

        public static readonly MethodInfo ToAsyncEnumerableMethod
            = typeof(AsyncLinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(ToAsyncEnumerable));

        public static IAsyncEnumerable<T> ToAsyncEnumerable<T>([NotNull] IEnumerable<T> source)
        {
            Check.NotNull(source, nameof(source));

            return new AsyncEnumerableAdapter<T>(source);
        }

        private sealed class AsyncEnumerableAdapter<T> : IAsyncEnumerable<T>
        {
            private readonly IEnumerable<T> _source;

            public AsyncEnumerableAdapter(IEnumerable<T> source)
            {
                _source = source;
            }

            public IAsyncEnumerator<T> GetEnumerator()
                => new AsyncEnumeratorAdapter(_source.GetEnumerator());

            private sealed class AsyncEnumeratorAdapter : IAsyncEnumerator<T>
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

        // Set operations
        private static readonly MethodInfo _concat = GetMethod("Concat", 1);
        private static readonly MethodInfo _except = GetMethod("Except", 1);
        private static readonly MethodInfo _intersect = GetMethod("Intersect", 1);
        private static readonly MethodInfo _union = GetMethod("Union", 1);

        public virtual MethodInfo Concat => _concat;
        public virtual MethodInfo Except => _except;
        public virtual MethodInfo Intersect => _intersect;
        public virtual MethodInfo Union => _union;

        private static MethodInfo GetMethod(string name, int parameterCount = 0)
        {
            var candidateMethods
                = typeof(AsyncEnumerable).GetTypeInfo().GetDeclaredMethods(name)
                    .ToList();

            return candidateMethods
                .SingleOrDefault(mi =>
                    mi.GetParameters().Length == parameterCount + 2
                    && mi.GetParameters().Last().ParameterType == typeof(CancellationToken))
                   ?? candidateMethods.Single(mi => mi.GetParameters().Length == parameterCount + 1);
        }
    }
}
