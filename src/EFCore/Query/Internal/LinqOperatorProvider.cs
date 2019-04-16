// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    // Issue#11266 This type is being used by provider code. Do not break.
    public class LinqOperatorProvider : ILinqOperatorProvider
    {
        #region EnumerableAdapters

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo ToEnumerable => _toEnumerable;

        private static readonly MethodInfo _toEnumerable
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_ToEnumerable));

        [UsedImplicitly]
        // ReSharper disable once PossibleMultipleEnumeration
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<TResult> _ToEnumerable<TResult>(IEnumerable<TResult> results) => results;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo ToOrdered => _toOrdered;

        private static readonly MethodInfo _toOrdered
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_ToOrdered));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static OrderedEnumerableAdapter<TResult> _ToOrdered<TResult>(IEnumerable<TResult> results)
            => new OrderedEnumerableAdapter<TResult>(results);

        private sealed class OrderedEnumerableAdapter<TResult> : IOrderedEnumerable<TResult>
        {
            private readonly IEnumerable<TResult> _results;

            public OrderedEnumerableAdapter(IEnumerable<TResult> results) => _results = results;

            public IOrderedEnumerable<TResult> CreateOrderedEnumerable<TKey>(
                Func<TResult, TKey> keySelector, IComparer<TKey> comparer, bool descending) => !descending
                ? _results.OrderBy(keySelector, comparer)
                : _results.OrderByDescending(keySelector, comparer);

            public IEnumerator<TResult> GetEnumerator() => _results.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo InterceptExceptions => _interceptExceptions;

        private static readonly MethodInfo _interceptExceptions
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_InterceptExceptions));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<T> _InterceptExceptions<T>(
            IEnumerable<T> source, Type contextType, IDiagnosticsLogger<DbLoggerCategory.Query> logger, QueryContext queryContext)
            => new ExceptionInterceptor<T>(source, contextType, logger, queryContext);

        private sealed class ExceptionInterceptor<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> _innerEnumerable;
            private readonly Type _contextType;
            private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
            private readonly QueryContext _queryContext;

            public ExceptionInterceptor(
                IEnumerable<T> innerEnumerable, Type contextType, IDiagnosticsLogger<DbLoggerCategory.Query> logger, QueryContext queryContext)
            {
                _innerEnumerable = innerEnumerable;
                _contextType = contextType;
                _logger = logger;
                _queryContext = queryContext;
            }

            public IEnumerator<T> GetEnumerator() => new EnumeratorExceptionInterceptor(this);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            [DebuggerStepThrough]
            private sealed class EnumeratorExceptionInterceptor : IEnumerator<T>
            {
                private readonly ExceptionInterceptor<T> _exceptionInterceptor;
                private IEnumerator<T> _innerEnumerator;

                public EnumeratorExceptionInterceptor(ExceptionInterceptor<T> exceptionInterceptor)
                {
                    _exceptionInterceptor = exceptionInterceptor;
                }

                public T Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _innerEnumerator.Current;
                }

                object IEnumerator.Current => _innerEnumerator.Current;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    using (_exceptionInterceptor._queryContext.ConcurrencyDetector.EnterCriticalSection())
                    {
                        try
                        {
                            if (_innerEnumerator == null)
                            {
                                _innerEnumerator = _exceptionInterceptor._innerEnumerable.GetEnumerator();
                            }

                            return _innerEnumerator.MoveNext();
                        }
                        catch (Exception exception)
                        {
                            _exceptionInterceptor._logger.QueryIterationFailed(_exceptionInterceptor._contextType, exception);

                            throw;
                        }
                    }
                }

                public void Reset() => _innerEnumerator?.Reset();

                public void Dispose()
                {
                    _innerEnumerator?.Dispose();
                    _exceptionInterceptor._queryContext.Dispose();
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo TrackEntities => _trackEntities;

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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo TrackGroupedEntities => _trackGroupedEntities;

        private static readonly MethodInfo _trackGroupedEntities
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_TrackGroupedEntities));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<IGrouping<TKey, TElement>> _TrackGroupedEntities<TKey, TElement>(
            IEnumerable<IGrouping<TKey, TElement>> groupings,
            QueryContext queryContext,
            IList<EntityTrackingInfo> entityTrackingInfos,
            IList<Func<TElement, object>> entityAccessors)
        {
            queryContext.BeginTrackingQuery();

            foreach (var grouping in groupings)
            {
                foreach (var result in grouping)
                {
                    if (result != null)
                    {
                        for (var i = 0; i < entityTrackingInfos.Count; i++)
                        {
                            var entity = entityAccessors[i](result);

                            if (entity != null)
                            {
                                queryContext.StartTracking(entity, entityTrackingInfos[i]);
                            }
                        }
                    }
                }

                yield return grouping;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo ToSequence => _toSequence;

        private static readonly MethodInfo _toSequence
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_ToSequence));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<T> _ToSequence<T>(Func<T> getElement)
            => new ResultEnumerable<T>(getElement);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        private sealed class ResultEnumerable<T> : IEnumerable<T>
        {
            private readonly Func<T> _getElement;

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public ResultEnumerable(Func<T> getElement)
            {
                _getElement = getElement;
            }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public IEnumerator<T> GetEnumerator() => new ResultEnumerator(_getElement());

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private sealed class ResultEnumerator : IEnumerator<T>
            {
                private readonly T _value;
                private bool _moved;

                public ResultEnumerator(T value) => _value = value;

                public bool MoveNext()
                {
                    if (!_moved)
                    {
                        _moved = true;

                        return _moved;
                    }

                    return false;
                }

                public void Reset()
                {
                    _moved = false;
                }

                object IEnumerator.Current => Current;

                public T Current => !_moved ? default : _value;

                void IDisposable.Dispose()
                {
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo ToQueryable => _toQueryable;

        private static readonly MethodInfo _toQueryable
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_ToQueryable));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IOrderedQueryable<TSource> _ToQueryable<TSource>(
            IEnumerable<TSource> source, QueryContext queryContext)
            => new QueryableAdapter<TSource>(source, queryContext);

        private sealed class QueryableAdapter<T> : IOrderedQueryable<T>
        {
            private readonly IEnumerable<T> _source;
            private readonly QueryContext _queryContext;
            private readonly ConstantExpression _constantExpression;

            public QueryableAdapter(IEnumerable<T> source, QueryContext queryContext)
            {
                _source = source;
                _queryContext = queryContext;

                _constantExpression = Expression.Constant(this);
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator() => _source.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

            public Type ElementType => typeof(T);

            public Expression Expression => _constantExpression;

            public IQueryProvider Provider => _queryContext.QueryProvider;
        }

        #endregion

        #region BaseComplexMethods

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo SelectMany => _selectMany;

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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo Join => _join;

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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo GroupJoin => _groupJoin;

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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo GroupBy => _groupBy;

        private static readonly MethodInfo _groupBy
            = typeof(LinqOperatorProvider).GetTypeInfo().GetDeclaredMethod(nameof(_GroupBy));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<IGrouping<TKey, TElement>> _GroupBy<TSource, TKey, TElement>(
            IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
            => source.GroupBy(keySelector, elementSelector);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo Select => _select;

        private static readonly MethodInfo _select
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_Select));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<TResult> _Select<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, TResult> selector)
            => source.Select(selector);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo OrderBy => _orderBy;

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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo ThenBy => _thenBy;

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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo Where => _where;

        private static readonly MethodInfo _where
            = typeof(LinqOperatorProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_Where));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<TSource> _Where<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate) => source.Where(predicate);

        #endregion

        #region SimpleOperators

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo All => _all;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo Any => _any;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo Cast => _cast;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo Contains => _contains;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo Count => _count;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo DefaultIfEmpty => _defaultIfEmpty;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo DefaultIfEmptyArg => _defaultIfEmptyArg;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo Distinct => _distinct;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo First => _first;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo FirstOrDefault => _firstOrDefault;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo Last => _last;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo LastOrDefault => _lastOrDefault;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo LongCount => _longCount;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo OfType => _ofType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo Single => _single;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo SingleOrDefault => _singleOrDefault;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo Skip => _skip;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo Take => _take;

        // Set operations

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo Concat => _concat;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo Except => _except;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo Intersect => _intersect;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo Union => _union;

        private static readonly MethodInfo _all = GetMethod("All", 1);
        private static readonly MethodInfo _any = GetMethod("Any");
        private static readonly MethodInfo _cast = GetMethod("Cast");
        private static readonly MethodInfo _contains = GetMethod("Contains", 1);
        private static readonly MethodInfo _count = GetMethod("Count");
        private static readonly MethodInfo _defaultIfEmpty = GetMethod("DefaultIfEmpty");
        private static readonly MethodInfo _defaultIfEmptyArg = GetMethod("DefaultIfEmpty", 1);
        private static readonly MethodInfo _distinct = GetMethod("Distinct");
        private static readonly MethodInfo _first = GetMethod("First");
        private static readonly MethodInfo _firstOrDefault = GetMethod("FirstOrDefault");
        private static readonly MethodInfo _last = GetMethod("Last");
        private static readonly MethodInfo _lastOrDefault = GetMethod("LastOrDefault");
        private static readonly MethodInfo _longCount = GetMethod("LongCount");
        private static readonly MethodInfo _ofType = GetMethod("OfType");
        private static readonly MethodInfo _single = GetMethod("Single");
        private static readonly MethodInfo _singleOrDefault = GetMethod("SingleOrDefault");
        private static readonly MethodInfo _skip = GetMethod("Skip", 1);
        private static readonly MethodInfo _take = GetMethod("Take", 1);

        // Set Operations
        private static readonly MethodInfo _concat = GetMethod("Concat", 1);

        private static readonly MethodInfo _except = GetMethod("Except", 1);
        private static readonly MethodInfo _intersect = GetMethod("Intersect", 1);
        private static readonly MethodInfo _union = GetMethod("Union", 1);

        #endregion

        #region HelperMethods

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo GetAggregateMethod(string methodName, Type elementType)
        {
            Check.NotEmpty(methodName, nameof(methodName));
            Check.NotNull(elementType, nameof(elementType));

            var aggregateMethods = GetMethods(methodName).ToList();

            return
                aggregateMethods
                    .FirstOrDefault(
                        mi => mi.GetParameters()[0].ParameterType
                              == typeof(IEnumerable<>).MakeGenericType(elementType))
                ?? aggregateMethods.Single(mi => mi.IsGenericMethod)
                    .MakeGenericMethod(elementType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Type MakeSequenceType(Type elementType)
            => typeof(IEnumerable<>)
                .MakeGenericType(Check.NotNull(elementType, nameof(elementType)));

        private static MethodInfo GetMethod(string name, int parameterCount = 0)
            => GetMethods(name, parameterCount).Single();

        private static IEnumerable<MethodInfo> GetMethods(string name, int parameterCount = 0)
            => typeof(Enumerable).GetTypeInfo().GetDeclaredMethods(name)
                .Where(mi => mi.GetParameters().Length == parameterCount + 1);

        #endregion
    }
}
