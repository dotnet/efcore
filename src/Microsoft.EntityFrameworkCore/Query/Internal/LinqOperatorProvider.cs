// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class LinqOperatorProvider : ILinqOperatorProvider
    {
        public virtual MethodInfo ToEnumerable => _toEnumerable;

        private static readonly MethodInfo _toEnumerable
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_ToEnumerable));

        [UsedImplicitly]
        // ReSharper disable once PossibleMultipleEnumeration
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<TResult> _ToEnumerable<TResult>(IEnumerable<TResult> results) => results;

        public virtual MethodInfo ToOrdered => _toOrdered;

        private static readonly MethodInfo _toOrdered
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_ToOrdered));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static OrderedEnumerableAdapter<TResult> _ToOrdered<TResult>(IEnumerable<TResult> results)
            => new OrderedEnumerableAdapter<TResult>(results);

        internal class OrderedEnumerableAdapter<TResult> : IOrderedEnumerable<TResult>
        {
            private readonly IEnumerable<TResult> _results;

            public OrderedEnumerableAdapter(IEnumerable<TResult> results)
            {
                _results = results;
            }

            public IOrderedEnumerable<TResult> CreateOrderedEnumerable<TKey>(
                Func<TResult, TKey> keySelector, IComparer<TKey> comparer, bool descending) => !@descending
                    ? _results.OrderBy(keySelector, comparer)
                    : _results.OrderByDescending(keySelector, comparer);

            public IEnumerator<TResult> GetEnumerator() => _results.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private static readonly MethodInfo _interceptExceptions
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_InterceptExceptions));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<T> _InterceptExceptions<T>(
            IEnumerable<T> source, Type contextType, ILogger logger, QueryContext queryContext)
            => new ExceptionInterceptor<T>(source, contextType, logger, queryContext);

        public virtual MethodInfo InterceptExceptions => _interceptExceptions;

        private sealed class ExceptionInterceptor<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> _innerEnumerable;
            private readonly Type _contextType;
            private readonly ILogger _logger;
            private readonly QueryContext _queryContext;

            public ExceptionInterceptor(
                IEnumerable<T> innerEnumerable, Type contextType, ILogger logger, QueryContext queryContext)
            {
                _innerEnumerable = innerEnumerable;
                _contextType = contextType;
                _logger = logger;
                _queryContext = queryContext;
            }

            public IEnumerator<T> GetEnumerator() => new EnumeratorExceptionInterceptor(this);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            [DebuggerStepThrough]
            private sealed class EnumeratorExceptionInterceptor : IEnumerator<T>
            {
                private readonly ExceptionInterceptor<T> _exceptionInterceptor;
                private readonly IEnumerator<T> _innerEnumerator;

                public EnumeratorExceptionInterceptor(ExceptionInterceptor<T> exceptionInterceptor)
                {
                    _exceptionInterceptor = exceptionInterceptor;
                    _innerEnumerator = _exceptionInterceptor._innerEnumerable.GetEnumerator();
                }

                public T Current => _innerEnumerator.Current;

                object IEnumerator.Current => _innerEnumerator.Current;

                public bool MoveNext()
                {
                    using (_exceptionInterceptor._queryContext.ConcurrencyDetector.EnterCriticalSection())
                    {
                        try
                        {
                            return _innerEnumerator.MoveNext();
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

                public void Reset() => _innerEnumerator?.Reset();
                public void Dispose() => _innerEnumerator?.Dispose();
            }
        }

        private static readonly MethodInfo _trackEntities
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_TrackEntities));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<TOut> _TrackEntities<TOut, TIn>(
            IEnumerable<TOut> results,
            QueryContext queryContext,
            IList<EntityTrackingInfo> entityTrackingInfos,
            IList<Func<TIn, object>> entityAccessors)
            where TIn : class
        {
            queryContext.BeginTrackingQuery();

            foreach (var result in results)
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

                yield return result;
            }
        }

        public virtual MethodInfo TrackEntities => _trackEntities;

        private static readonly MethodInfo _trackGroupedEntities
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_TrackGroupedEntities));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<IGrouping<TKey, TOut>> _TrackGroupedEntities<TKey, TOut, TIn>(
            IEnumerable<IGrouping<TKey, TOut>> groupings,
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

        private class TrackingGrouping<TKey, TOut, TIn> : IGrouping<TKey, TOut>
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

            public IEnumerator<TOut> GetEnumerator()
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

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private static readonly MethodInfo _toSequence
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_ToSequence));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<T> _ToSequence<T>(T element) => new[] { element };

        public virtual MethodInfo ToSequence => _toSequence;

        private static readonly MethodInfo _toQueryable
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_ToQueryable));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IOrderedQueryable<TSource> _ToQueryable<TSource>(IEnumerable<TSource> source)
            => new EnumerableQuery<TSource>(source);

        public virtual MethodInfo ToQueryable => _toQueryable;

        private static readonly MethodInfo _selectMany
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_SelectMany));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<TResult> _SelectMany<TSource, TCollection, TResult>(
            IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
            => source.SelectMany(collectionSelector, resultSelector);

        public virtual MethodInfo SelectMany => _selectMany;

        private static readonly MethodInfo _join
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_Join));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<TResult> _Join<TOuter, TInner, TKey, TResult>(
            IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector)
            => outer.Join(inner, outerKeySelector, innerKeySelector, resultSelector);

        public virtual MethodInfo Join => _join;

        private static readonly MethodInfo _groupJoin
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_GroupJoin));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<TResult> _GroupJoin<TOuter, TInner, TKey, TResult>(
            IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
            => outer.GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector);

        public virtual MethodInfo GroupJoin => _groupJoin;

        private static readonly MethodInfo _select
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_Select));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<TResult> _Select<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, TResult> selector)
            => source.Select(selector);

        public virtual MethodInfo Select => _select;

        private static readonly MethodInfo _orderBy
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_OrderBy));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IOrderedEnumerable<TSource> _OrderBy<TSource, TKey>(
            IEnumerable<TSource> source, Func<TSource, TKey> expression, OrderingDirection orderingDirection)
            => orderingDirection == OrderingDirection.Asc
                ? source.OrderBy(expression)
                : source.OrderByDescending(expression);

        public virtual MethodInfo OrderBy => _orderBy;

        private static readonly MethodInfo _thenBy
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_ThenBy));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IOrderedEnumerable<TSource> _ThenBy<TSource, TKey>(
            IOrderedEnumerable<TSource> source, Func<TSource, TKey> expression, OrderingDirection orderingDirection)
            => orderingDirection == OrderingDirection.Asc
                ? source.ThenBy(expression)
                : source.ThenByDescending(expression);

        public virtual MethodInfo ThenBy => _thenBy;

        private static readonly MethodInfo _where
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_Where));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<TSource> _Where<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate) => source.Where(predicate);

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
            = typeof(LinqOperatorProvider).GetTypeInfo().GetDeclaredMethod(nameof(_GroupBy));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<IGrouping<TKey, TElement>> _GroupBy<TSource, TKey, TElement>(
            IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
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

        // Set operations
        private static readonly MethodInfo _concat = GetMethod("Concat", 1);
        private static readonly MethodInfo _except = GetMethod("Except", 1);
        private static readonly MethodInfo _intersect = GetMethod("Intersect", 1);
        private static readonly MethodInfo _union = GetMethod("Union", 1);

        public virtual MethodInfo Concat => _concat;
        public virtual MethodInfo Except => _except;
        public virtual MethodInfo Intersect => _intersect;
        public virtual MethodInfo Union => _union;

        public virtual MethodInfo GetAggregateMethod(string methodName, Type elementType)
        {
            Check.NotEmpty(methodName, nameof(methodName));
            Check.NotNull(elementType, nameof(elementType));

            var aggregateMethods = GetMethods(methodName).ToList();

            return
                aggregateMethods
                    .FirstOrDefault(mi => mi.GetParameters()[0].ParameterType
                                          == typeof(IEnumerable<>).MakeGenericType(elementType))
                ?? aggregateMethods.Single(mi => mi.IsGenericMethod)
                    .MakeGenericMethod(elementType);
        }

        public virtual Type MakeSequenceType(Type elementType)
            => typeof(IEnumerable<>)
                .MakeGenericType(Check.NotNull(elementType, nameof(elementType)));

        private static MethodInfo GetMethod(string name, int parameterCount = 0)
            => GetMethods(name, parameterCount).Single();

        private static IEnumerable<MethodInfo> GetMethods(string name, int parameterCount = 0)
            => typeof(Enumerable).GetTypeInfo().GetDeclaredMethods(name)
                .Where(mi => mi.GetParameters().Length == parameterCount + 1);
    }
}
