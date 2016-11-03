// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable InconsistentNaming
// ReSharper disable ImplicitlyCapturedClosure
namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     Provides reflection objects for late-binding to asynchronous relational query operations.
    /// </summary>
    public class AsyncQueryMethodProvider : IQueryMethodProvider
    {
        /// <summary>
        ///     The shaped query method.
        /// </summary>
        public virtual MethodInfo ShapedQueryMethod => _shapedQueryMethodInfo;

        private static readonly MethodInfo _shapedQueryMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_ShapedQuery));

        [UsedImplicitly]
        private static IAsyncEnumerable<T> _ShapedQuery<T>(
            QueryContext queryContext,
            ShaperCommandContext shaperCommandContext,
            IShaper<T> shaper)
            => AsyncLinqOperatorProvider
                ._Select(new AsyncQueryingEnumerable(
                    (RelationalQueryContext)queryContext,
                    shaperCommandContext,
                    queryIndex: null),
                    vb => shaper.Shape(queryContext, vb)); // TODO: Pass shaper to underlying enumerable

        /// <summary>
        ///     The default if empty shaped query method.
        /// </summary>
        public virtual MethodInfo DefaultIfEmptyShapedQueryMethod => _defaultIfEmptyShapedQueryMethodInfo;

        private static readonly MethodInfo _defaultIfEmptyShapedQueryMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_DefaultIfEmptyShapedQuery));

        [UsedImplicitly]
        private static IAsyncEnumerable<T> _DefaultIfEmptyShapedQuery<T>(
            QueryContext queryContext,
            ShaperCommandContext shaperCommandContext,
            IShaper<T> shaper)
            => AsyncLinqOperatorProvider
                ._Select(
                    new DefaultIfEmptyAsyncEnumerable(
                        new AsyncQueryingEnumerable(
                            (RelationalQueryContext)queryContext,
                            shaperCommandContext,
                            queryIndex: null)),
                    vb => shaper.Shape(queryContext, vb));

        private sealed class DefaultIfEmptyAsyncEnumerable : IAsyncEnumerable<ValueBuffer>
        {
            private readonly IAsyncEnumerable<ValueBuffer> _source;

            public DefaultIfEmptyAsyncEnumerable(IAsyncEnumerable<ValueBuffer> source)
            {
                _source = source;
            }

            public IAsyncEnumerator<ValueBuffer> GetEnumerator()
                => new DefaultIfEmptyAsyncEnumerator(_source.GetEnumerator());

            private sealed class DefaultIfEmptyAsyncEnumerator : IAsyncEnumerator<ValueBuffer>
            {
                private readonly IAsyncEnumerator<ValueBuffer> _enumerator;

                private bool _checkedEmpty;

                public DefaultIfEmptyAsyncEnumerator(IAsyncEnumerator<ValueBuffer> enumerator)
                {
                    _enumerator = enumerator;
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    if (!await _enumerator.MoveNext(cancellationToken))
                    {
                        return false;
                    }

                    if (!_checkedEmpty)
                    {
                        var empty = true;

                        for (var i = 0; i < _enumerator.Current.Count; i++)
                        {
                            empty &= _enumerator.Current[i] == null;
                        }

                        if (empty)
                        {
                            return false;
                        }

                        _checkedEmpty = true;
                    }

                    return true;
                }

                public ValueBuffer Current => _enumerator.Current;

                public void Dispose() => _enumerator.Dispose();
            }
        }

        // TODO: Pass shaper to underlying enumerable

        /// <summary>
        ///     The query method.
        /// </summary>
        public virtual MethodInfo QueryMethod => _queryMethodInfo;

        private static readonly MethodInfo _queryMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_Query));

        [UsedImplicitly]
        private static IAsyncEnumerable<ValueBuffer> _Query(
            QueryContext queryContext,
            ShaperCommandContext shaperCommandContext,
            int? queryIndex)
            => new AsyncQueryingEnumerable(
                (RelationalQueryContext)queryContext,
                shaperCommandContext,
                queryIndex);

        /// <summary>
        ///     The get result method.
        /// </summary>
        public virtual MethodInfo GetResultMethod => _getResultMethodInfo;

        private static readonly MethodInfo _getResultMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(GetResult));

        [UsedImplicitly]
        private static async Task<TResult> GetResult<TResult>(
            IAsyncEnumerable<ValueBuffer> valueBuffers, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var enumerator = valueBuffers.GetEnumerator())
            {
                if (await enumerator.MoveNext(cancellationToken))
                {
                    return enumerator.Current[0] == null
                        ? default(TResult)
                        : (TResult)enumerator.Current[0];
                }
            }

            return default(TResult);
        }

        /// <summary>
        ///     The group by method.
        /// </summary>
        public virtual MethodInfo GroupByMethod => _groupByMethodInfo;

        private static readonly MethodInfo _groupByMethodInfo
            = typeof(AsyncQueryMethodProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_GroupBy));

        [UsedImplicitly]
        private static IAsyncEnumerable<IGrouping<TKey, TElement>> _GroupBy<TSource, TKey, TElement>(
            IAsyncEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector)
            => new GroupByAsyncEnumerable<TSource, TKey, TElement>(source, keySelector, elementSelector);

        private sealed class GroupByAsyncEnumerable<TSource, TKey, TElement> : IAsyncEnumerable<IGrouping<TKey, TElement>>
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

            public IAsyncEnumerator<IGrouping<TKey, TElement>> GetEnumerator() => new GroupByAsyncEnumerator(this);

            private sealed class GroupByAsyncEnumerator : IAsyncEnumerator<IGrouping<TKey, TElement>>
            {
                private readonly GroupByAsyncEnumerable<TSource, TKey, TElement> _groupByAsyncEnumerable;
                private readonly IEqualityComparer<TKey> _comparer;

                private IAsyncEnumerator<TSource> _sourceEnumerator;
                private bool _hasNext;

                public GroupByAsyncEnumerator(GroupByAsyncEnumerable<TSource, TKey, TElement> groupByAsyncEnumerable)
                {
                    _groupByAsyncEnumerable = groupByAsyncEnumerable;
                    _comparer = EqualityComparer<TKey>.Default;
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (_sourceEnumerator == null)
                    {
                        _sourceEnumerator = _groupByAsyncEnumerable._source.GetEnumerator();
                        _hasNext = await _sourceEnumerator.MoveNext(cancellationToken);
                    }

                    if (_hasNext)
                    {
                        var currentKey = _groupByAsyncEnumerable._keySelector(_sourceEnumerator.Current);
                        var element = _groupByAsyncEnumerable._elementSelector(_sourceEnumerator.Current);
                        var grouping = new Grouping<TKey, TElement>(currentKey) { element };

                        while (true)
                        {
                            _hasNext = await _sourceEnumerator.MoveNext(cancellationToken);

                            if (!_hasNext)
                            {
                                break;
                            }

                            if (!_comparer.Equals(
                                currentKey,
                                _groupByAsyncEnumerable._keySelector(_sourceEnumerator.Current)))
                            {
                                break;
                            }

                            grouping.Add(_groupByAsyncEnumerable._elementSelector(_sourceEnumerator.Current));
                        }

                        Current = grouping;

                        return true;
                    }

                    return false;
                }

                public IGrouping<TKey, TElement> Current { get; private set; }

                public void Dispose() => _sourceEnumerator?.Dispose();
            }
        }

        /// <summary>
        ///     Type of the group join include.
        /// </summary>
        public virtual Type GroupJoinIncludeType => typeof(AsyncGroupJoinInclude);

        /// <summary>
        ///     Creates a group join include used to describe an Include operation that should
        ///     be performed as part of a GroupJoin.
        /// </summary>
        /// <param name="navigationPath"> The included navigation path. </param>
        /// <param name="querySourceRequiresTracking"> true if this query source requires tracking. </param>
        /// <param name="existingGroupJoinInclude"> A possibly null existing group join include. </param>
        /// <param name="relatedEntitiesLoaders"> The related entities loaders. </param>
        /// <returns>
        ///     A new group join include.
        /// </returns>
        public virtual object CreateGroupJoinInclude(
            IReadOnlyList<INavigation> navigationPath,
            bool querySourceRequiresTracking,
            object existingGroupJoinInclude,
            object relatedEntitiesLoaders)
        {
            Check.NotNull(navigationPath, nameof(navigationPath));
            Check.NotNull(relatedEntitiesLoaders, nameof(relatedEntitiesLoaders));

            var previousGroupJoinInclude
                = new AsyncGroupJoinInclude(
                    navigationPath,
                    (IReadOnlyList<Func<QueryContext, IAsyncRelatedEntitiesLoader>>)relatedEntitiesLoaders,
                    querySourceRequiresTracking);

            var groupJoinInclude = existingGroupJoinInclude as AsyncGroupJoinInclude;

            if (groupJoinInclude != null)
            {
                groupJoinInclude.SetPrevious(previousGroupJoinInclude);

                return null;
            }

            return previousGroupJoinInclude;
        }

        /// <summary>
        ///     The group join method.
        /// </summary>
        public virtual MethodInfo GroupJoinMethod => _groupJoinMethodInfo;

        private static readonly MethodInfo _groupJoinMethodInfo
            = typeof(AsyncQueryMethodProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_GroupJoin));

        [UsedImplicitly]
        private static IAsyncEnumerable<TResult> _GroupJoin<TOuter, TInner, TKey, TResult>(
            RelationalQueryContext queryContext,
            IAsyncEnumerable<ValueBuffer> source,
            IShaper<TOuter> outerShaper,
            IShaper<TInner> innerShaper,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IAsyncEnumerable<TInner>, TResult> resultSelector,
            AsyncGroupJoinInclude outerGroupJoinInclude,
            AsyncGroupJoinInclude innerGroupJoinInclude)
            => new GroupJoinAsyncEnumerable<TOuter, TInner, TKey, TResult>(
                queryContext,
                source,
                outerShaper,
                innerShaper,
                innerKeySelector,
                resultSelector,
                outerGroupJoinInclude,
                innerGroupJoinInclude);

        private sealed class GroupJoinAsyncEnumerable<TOuter, TInner, TKey, TResult> : IAsyncEnumerable<TResult>
        {
            private readonly RelationalQueryContext _queryContext;
            private readonly IAsyncEnumerable<ValueBuffer> _source;
            private readonly IShaper<TOuter> _outerShaper;
            private readonly IShaper<TInner> _innerShaper;
            private readonly Func<TInner, TKey> _innerKeySelector;
            private readonly Func<TOuter, IAsyncEnumerable<TInner>, TResult> _resultSelector;
            private readonly AsyncGroupJoinInclude _outerGroupJoinInclude;
            private readonly AsyncGroupJoinInclude _innerGroupJoinInclude;
            private readonly bool _hasOuters;

            public GroupJoinAsyncEnumerable(
                RelationalQueryContext queryContext,
                IAsyncEnumerable<ValueBuffer> source,
                IShaper<TOuter> outerShaper,
                IShaper<TInner> innerShaper,
                Func<TInner, TKey> innerKeySelector,
                Func<TOuter, IAsyncEnumerable<TInner>, TResult> resultSelector,
                AsyncGroupJoinInclude outerGroupJoinInclude,
                AsyncGroupJoinInclude innerGroupJoinInclude)
            {
                _queryContext = queryContext;
                _source = source;
                _outerShaper = outerShaper;
                _innerShaper = innerShaper;
                _innerKeySelector = innerKeySelector;
                _resultSelector = resultSelector;
                _outerGroupJoinInclude = outerGroupJoinInclude;
                _innerGroupJoinInclude = innerGroupJoinInclude;
                _hasOuters = (_innerShaper as EntityShaper)?.ValueBufferOffset > 0;
            }

            public IAsyncEnumerator<TResult> GetEnumerator() => new GroupJoinAsyncEnumerator(this);

            private sealed class GroupJoinAsyncEnumerator : IAsyncEnumerator<TResult>
            {
                private readonly GroupJoinAsyncEnumerable<TOuter, TInner, TKey, TResult> _groupJoinAsyncEnumerable;
                private readonly IEqualityComparer<TKey> _comparer;

                private IAsyncEnumerator<ValueBuffer> _sourceEnumerator;
                private bool _hasNext;
                private TOuter _nextOuter;
                private AsyncGroupJoinInclude.AsyncGroupJoinIncludeContext _outerGroupJoinIncludeContext;
                private AsyncGroupJoinInclude.AsyncGroupJoinIncludeContext _innerGroupJoinIncludeContext;
                private Func<TOuter, object> _outerEntityAccessor;
                private Func<TInner, object> _innerEntityAccessor;

                public GroupJoinAsyncEnumerator(
                    GroupJoinAsyncEnumerable<TOuter, TInner, TKey, TResult> groupJoinAsyncEnumerable)
                {
                    _groupJoinAsyncEnumerable = groupJoinAsyncEnumerable;
                    _comparer = EqualityComparer<TKey>.Default;
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (_sourceEnumerator == null)
                    {
                        _outerGroupJoinIncludeContext = _groupJoinAsyncEnumerable._outerGroupJoinInclude?.CreateIncludeContext(_groupJoinAsyncEnumerable._queryContext);
                        _innerGroupJoinIncludeContext = _groupJoinAsyncEnumerable._innerGroupJoinInclude?.CreateIncludeContext(_groupJoinAsyncEnumerable._queryContext);
                        _outerEntityAccessor = _groupJoinAsyncEnumerable._outerGroupJoinInclude?.EntityAccessor as Func<TOuter, object>;
                        _innerEntityAccessor = _groupJoinAsyncEnumerable._innerGroupJoinInclude?.EntityAccessor as Func<TInner, object>;
                        _sourceEnumerator = _groupJoinAsyncEnumerable._source.GetEnumerator();
                        _hasNext = await _sourceEnumerator.MoveNext(cancellationToken);
                        _nextOuter = default(TOuter);
                    }

                    if (_hasNext)
                    {
                        var outer
                            = Equals(_nextOuter, default(TOuter))
                                ? _groupJoinAsyncEnumerable._outerShaper
                                    .Shape(_groupJoinAsyncEnumerable._queryContext, _sourceEnumerator.Current)
                                : _nextOuter;

                        _nextOuter = default(TOuter);

                        if (_outerGroupJoinIncludeContext != null)
                        {
                            if (_outerEntityAccessor != null)
                            {
                                await _outerGroupJoinIncludeContext.IncludeAsync(_outerEntityAccessor(outer), cancellationToken);
                            }
                            else
                            {
                                await _outerGroupJoinIncludeContext.IncludeAsync(outer, cancellationToken);
                            }
                        }

                        var inner
                            = _groupJoinAsyncEnumerable._innerShaper
                                .Shape(_groupJoinAsyncEnumerable._queryContext, _sourceEnumerator.Current);

                        var inners = new List<TInner>();

                        if (inner == null)
                        {
                            Current
                                = _groupJoinAsyncEnumerable._resultSelector(
                                    outer, inners.ToAsyncEnumerable());

                            _hasNext = await _sourceEnumerator.MoveNext(cancellationToken);

                            return true;
                        }

                        var currentGroupKey = _groupJoinAsyncEnumerable._innerKeySelector(inner);

                        if (_innerGroupJoinIncludeContext != null)
                        {
                            if (_innerEntityAccessor != null)
                            {
                                await _innerGroupJoinIncludeContext.IncludeAsync(_innerEntityAccessor(inner), cancellationToken);
                            }
                            else
                            {
                                await _innerGroupJoinIncludeContext.IncludeAsync(inner, cancellationToken);
                            }
                        }

                        inners.Add(inner);

                        while (true)
                        {
                            _hasNext = await _sourceEnumerator.MoveNext(cancellationToken);

                            if (!_hasNext)
                            {
                                break;
                            }

                            if (_groupJoinAsyncEnumerable._hasOuters)
                            {
                                _nextOuter
                                    = _groupJoinAsyncEnumerable._outerShaper
                                        .Shape(_groupJoinAsyncEnumerable._queryContext, _sourceEnumerator.Current);

                                if (!Equals(outer, _nextOuter))
                                {
                                    break;
                                }

                                _nextOuter = default(TOuter);
                            }

                            inner
                                = _groupJoinAsyncEnumerable._innerShaper
                                    .Shape(_groupJoinAsyncEnumerable._queryContext, _sourceEnumerator.Current);

                            if (inner == null)
                            {
                                break;
                            }

                            var innerKey = _groupJoinAsyncEnumerable._innerKeySelector(inner);

                            if (!_comparer.Equals(currentGroupKey, innerKey))
                            {
                                break;
                            }

                            if (_innerGroupJoinIncludeContext != null)
                            {
                                await _innerGroupJoinIncludeContext.IncludeAsync(inner, cancellationToken);
                            }

                            inners.Add(inner);
                        }

                        Current
                            = _groupJoinAsyncEnumerable._resultSelector(
                                outer, inners.ToAsyncEnumerable());

                        return true;
                    }

                    return false;
                }

                public TResult Current { get; private set; }

                public void Dispose()
                {
                    _sourceEnumerator?.Dispose();
                    _innerGroupJoinIncludeContext?.Dispose();
                    _outerGroupJoinIncludeContext?.Dispose();
                }
            }
        }

        /// <summary>
        ///     The include method.
        /// </summary>
        public virtual MethodInfo IncludeMethod => _includeMethodInfo;

        private static readonly MethodInfo _includeMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_Include));

        [UsedImplicitly]
        private static IAsyncEnumerable<T> _Include<T>(
            RelationalQueryContext queryContext,
            IAsyncEnumerable<T> innerResults,
            Func<T, object> entityAccessor,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<Func<QueryContext, IAsyncRelatedEntitiesLoader>> relatedEntitiesLoaderFactories,
            bool querySourceRequiresTracking)
        {
            queryContext.BeginIncludeScope();

            var relatedEntitiesLoaders
                = relatedEntitiesLoaderFactories.Select(f => f(queryContext))
                    .ToArray();

            return new IncludeAsyncEnumerable<T>(
                queryContext,
                innerResults,
                entityAccessor,
                navigationPath,
                relatedEntitiesLoaders,
                querySourceRequiresTracking);
        }

        private sealed class IncludeAsyncEnumerable<T> : IAsyncEnumerable<T>
        {
            private readonly RelationalQueryContext _queryContext;
            private readonly IAsyncEnumerable<T> _innerResults;
            private readonly Func<T, object> _entityAccessor;
            private readonly IReadOnlyList<INavigation> _navigationPath;
            private readonly IAsyncRelatedEntitiesLoader[] _relatedEntitiesLoaders;
            private readonly bool _querySourceRequiresTracking;

            public IncludeAsyncEnumerable(
                RelationalQueryContext queryContext,
                IAsyncEnumerable<T> innerResults,
                Func<T, object> entityAccessor,
                IReadOnlyList<INavigation> navigationPath,
                IAsyncRelatedEntitiesLoader[] relatedEntitiesLoaders,
                bool querySourceRequiresTracking)
            {
                _queryContext = queryContext;
                _innerResults = innerResults;
                _entityAccessor = entityAccessor;
                _navigationPath = navigationPath;
                _relatedEntitiesLoaders = relatedEntitiesLoaders;
                _querySourceRequiresTracking = querySourceRequiresTracking;
            }

            public IAsyncEnumerator<T> GetEnumerator()
                => new IncludeAsyncEnumerator<T>(
                    _queryContext,
                    _innerResults.GetEnumerator(),
                    _entityAccessor,
                    _navigationPath,
                    _relatedEntitiesLoaders,
                    _querySourceRequiresTracking);

            private class IncludeAsyncEnumerator<T1> : IAsyncEnumerator<T>
            {
                private readonly RelationalQueryContext _queryContext;
                private readonly IAsyncEnumerator<T> _innerResults;
                private readonly Func<T, object> _entityAccessor;
                private readonly IReadOnlyList<INavigation> _navigationPath;
                private readonly IAsyncRelatedEntitiesLoader[] _relatedEntitiesLoaders;
                private readonly bool _querySourceRequiresTracking;

                public IncludeAsyncEnumerator(
                    RelationalQueryContext queryContext,
                    IAsyncEnumerator<T> innerResults,
                    Func<T, object> entityAccessor,
                    IReadOnlyList<INavigation> navigationPath,
                    IAsyncRelatedEntitiesLoader[] relatedEntitiesLoaders,
                    bool querySourceRequiresTracking)
                {
                    _queryContext = queryContext;
                    _innerResults = innerResults;
                    _entityAccessor = entityAccessor;
                    _navigationPath = navigationPath;
                    _relatedEntitiesLoaders = relatedEntitiesLoaders;
                    _querySourceRequiresTracking = querySourceRequiresTracking;
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    if (await _innerResults.MoveNext(cancellationToken))
                    {
                        Current = _innerResults.Current;

                        await _queryContext.QueryBuffer
                            .IncludeAsync(
                                _queryContext,
                                _entityAccessor == null
                                    ? Current
                                    : _entityAccessor(Current), // TODO: Compile time?
                                _navigationPath,
                                _relatedEntitiesLoaders,
                                _querySourceRequiresTracking,
                                cancellationToken);

                        return true;
                    }

                    return false;
                }

                public T Current { get; private set; }

                public void Dispose()
                {
                    _innerResults.Dispose();

                    foreach (var relatedEntitiesLoader in _relatedEntitiesLoaders)
                    {
                        relatedEntitiesLoader.Dispose();
                    }

                    _queryContext.EndIncludeScope();
                }
            }
        }

        /// <summary>
        ///     Type of the related entities loader.
        /// </summary>
        public virtual Type RelatedEntitiesLoaderType => typeof(IAsyncRelatedEntitiesLoader);

        /// <summary>
        ///     The create reference related entities loader method.
        /// </summary>
        public virtual MethodInfo CreateReferenceRelatedEntitiesLoaderMethod
            => _createReferenceRelatedEntitiesLoaderMethod;

        private static readonly MethodInfo _createReferenceRelatedEntitiesLoaderMethod
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_CreateReferenceRelatedEntitiesLoader));

        [UsedImplicitly]
        private static IAsyncRelatedEntitiesLoader _CreateReferenceRelatedEntitiesLoader(
            int valueBufferOffset,
            int queryIndex,
            Func<ValueBuffer, object> materializer)
            => new ReferenceRelatedEntitiesLoader(valueBufferOffset, queryIndex, materializer);

        private sealed class ReferenceRelatedEntitiesLoader : IAsyncRelatedEntitiesLoader
        {
            private readonly int _valueBufferOffset;
            private readonly int _queryIndex;
            private readonly Func<ValueBuffer, object> _materializer;

            public ReferenceRelatedEntitiesLoader(
                int valueBufferOffset,
                int queryIndex,
                Func<ValueBuffer, object> materializer)
            {
                _valueBufferOffset = valueBufferOffset;
                _queryIndex = queryIndex;
                _materializer = materializer;
            }

            public IAsyncEnumerable<EntityLoadInfo> Load(QueryContext queryContext, IIncludeKeyComparer keyComparer)
            {
                var valueBuffer
                    = ((RelationalQueryContext)queryContext)
                        .GetIncludeValueBuffer(_queryIndex).WithOffset(_valueBufferOffset);

                return new AsyncEnumerableAdapter<EntityLoadInfo>(
                    new EntityLoadInfo(valueBuffer, _materializer));
            }

            private sealed class AsyncEnumerableAdapter<T> : IAsyncEnumerable<T>
            {
                private readonly T _value;

                public AsyncEnumerableAdapter(T value)
                {
                    _value = value;
                }

                public IAsyncEnumerator<T> GetEnumerator() => new AsyncEnumeratorAdapter(_value);

                private sealed class AsyncEnumeratorAdapter : IAsyncEnumerator<T>
                {
                    private readonly T _value;
                    private bool _hasNext = true;

                    public AsyncEnumeratorAdapter(T value)
                    {
                        _value = value;
                    }

                    public Task<bool> MoveNext(CancellationToken cancellationToken)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var hasNext = _hasNext;

                        if (hasNext)
                        {
                            _hasNext = false;
                        }

                        return Task.FromResult(hasNext);
                    }

                    public T Current => !_hasNext ? _value : default(T);

                    public void Dispose()
                    {
                    }
                }
            }

            public void Dispose()
            {
                // no-op
            }
        }

        /// <summary>
        ///     The create collection related entities loader method.
        /// </summary>
        public virtual MethodInfo CreateCollectionRelatedEntitiesLoaderMethod
            => _createCollectionRelatedEntitiesLoaderMethod;

        private static readonly MethodInfo _createCollectionRelatedEntitiesLoaderMethod
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_CreateCollectionRelatedEntitiesLoader));

        [UsedImplicitly]
        private static IAsyncRelatedEntitiesLoader _CreateCollectionRelatedEntitiesLoader(
            QueryContext queryContext,
            ShaperCommandContext shaperCommandContext,
            int queryIndex,
            Func<ValueBuffer, object> materializer)
            => new CollectionRelatedEntitiesLoader(
                queryContext, shaperCommandContext, queryIndex, materializer);

        private class CollectionRelatedEntitiesLoader : IAsyncRelatedEntitiesLoader
        {
            private readonly AsyncIncludeCollectionIterator _includeCollectionIterator;
            private readonly Func<ValueBuffer, object> _materializer;

            public CollectionRelatedEntitiesLoader(
                QueryContext queryContext,
                ShaperCommandContext shaperCommandContext,
                int queryIndex,
                Func<ValueBuffer, object> materializer)
            {
                _includeCollectionIterator
                    = new AsyncIncludeCollectionIterator(
                        _Query(queryContext, shaperCommandContext, queryIndex)
                            .GetEnumerator());

                _materializer = materializer;
            }

            public IAsyncEnumerable<EntityLoadInfo> Load(QueryContext queryContext, IIncludeKeyComparer keyComparer)
            {
                return
                    AsyncLinqOperatorProvider
                        ._Select(
                            _includeCollectionIterator.GetRelatedValues(keyComparer),
                            vr => new EntityLoadInfo(vr, _materializer));
            }

            public void Dispose() => _includeCollectionIterator?.Dispose();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo InjectParametersMethod => _injectParametersMethodInfo;

        private static readonly MethodInfo _injectParametersMethodInfo
            = typeof(AsyncQueryMethodProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_InjectParameters));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IAsyncEnumerable<TElement> _InjectParameters<TElement>(
            QueryContext queryContext,
            IAsyncEnumerable<TElement> source,
            string[] parameterNames,
            object[] parameterValues)
            => new ParameterInjector<TElement>(queryContext, source, parameterNames, parameterValues);

        private sealed class ParameterInjector<TElement> : IAsyncEnumerable<TElement>
        {
            private readonly QueryContext _queryContext;
            private readonly IAsyncEnumerable<TElement> _innerEnumerable;
            private readonly string[] _parameterNames;
            private readonly object[] _parameterValues;

            public ParameterInjector(
                QueryContext queryContext,
                IAsyncEnumerable<TElement> innerEnumerable,
                string[] parameterNames,
                object[] parameterValues)
            {
                _queryContext = queryContext;
                _innerEnumerable = innerEnumerable;
                _parameterNames = parameterNames;
                _parameterValues = parameterValues;
            }

            IAsyncEnumerator<TElement> IAsyncEnumerable<TElement>.GetEnumerator() => new InjectParametersEnumerator(this);

            private sealed class InjectParametersEnumerator : IAsyncEnumerator<TElement>
            {
                private readonly ParameterInjector<TElement> _parameterInjector;
                private readonly IAsyncEnumerator<TElement> _innerEnumerator;
                private bool _disposed;

                public InjectParametersEnumerator(ParameterInjector<TElement> parameterInjector)
                {
                    _parameterInjector = parameterInjector;

                    for (var i = 0; i < _parameterInjector._parameterNames.Length; i++)
                    {
                        _parameterInjector._queryContext.AddParameter(
                            _parameterInjector._parameterNames[i],
                            _parameterInjector._parameterValues[i]);
                    }

                    _innerEnumerator = _parameterInjector._innerEnumerable.GetEnumerator();
                }

                public TElement Current => _innerEnumerator.Current;

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                    => await _innerEnumerator.MoveNext(cancellationToken);

                public void Dispose()
                {
                    if (!_disposed)
                    {
                        _innerEnumerator.Dispose();

                        foreach (var parameterName in _parameterInjector._parameterNames)
                        {
                            _parameterInjector._queryContext.RemoveParameter(parameterName);
                        }

                        _disposed = true;
                    }
                }
            }
        }
    }
}
