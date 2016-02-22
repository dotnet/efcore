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

// ReSharper disable ImplicitlyCapturedClosure

namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsyncQueryMethodProvider : IQueryMethodProvider
    {
        public virtual MethodInfo ShapedQueryMethod => _shapedQueryMethodInfo;

        private static readonly MethodInfo _shapedQueryMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_ShapedQuery));

        [UsedImplicitly]
        private static IAsyncEnumerable<T> _ShapedQuery<T>(
            QueryContext queryContext,
            ShaperCommandContext shaperCommandContext,
            IShaper<T> shaper)
            => new AsyncQueryingEnumerable(
                (RelationalQueryContext)queryContext,
                shaperCommandContext,
                queryIndex: null)
                .Select(vb => shaper.Shape(queryContext, vb)); // TODO: Pass shaper to underlying enumerable

        public virtual MethodInfo DefaultIfEmptyShapedQueryMethod => _defaultIfEmptyShapedQueryMethodInfo;

        private static readonly MethodInfo _defaultIfEmptyShapedQueryMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_DefaultIfEmptyShapedQuery));

        [UsedImplicitly]
        private static IAsyncEnumerable<T> _DefaultIfEmptyShapedQuery<T>(
            QueryContext queryContext,
            ShaperCommandContext shaperCommandContext,
            IShaper<T> shaper)
            => new DefaultIfEmptyAsyncEnumerable(
                new AsyncQueryingEnumerable(
                    (RelationalQueryContext)queryContext,
                    shaperCommandContext,
                    queryIndex: null))
                .Select(vb => shaper.Shape(queryContext, vb));

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
                    if (!await _enumerator.MoveNext())
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

        private class GroupByAsyncEnumerable<TSource, TKey, TElement> : IAsyncEnumerable<IGrouping<TKey, TElement>>
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

            private class GroupByAsyncEnumerator : IAsyncEnumerator<IGrouping<TKey, TElement>>
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
                        _hasNext = await _sourceEnumerator.MoveNext();
                    }

                    if (_hasNext)
                    {
                        var currentKey = _groupByAsyncEnumerable._keySelector(_sourceEnumerator.Current);
                        var element = _groupByAsyncEnumerable._elementSelector(_sourceEnumerator.Current);
                        var grouping = new Grouping<TKey, TElement>(currentKey) { element };

                        while (true)
                        {
                            _hasNext = await _sourceEnumerator.MoveNext();

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

        public virtual Type GroupJoinIncludeType => typeof(AsyncGroupJoinInclude);

        public virtual object CreateGroupJoinInclude(
            IReadOnlyList<INavigation> navigationPath,
            bool querySourceRequiresTracking,
            object existingGroupJoinInclude,
            object relatedEntitiesLoaders)
        {
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

        private class GroupJoinAsyncEnumerable<TOuter, TInner, TKey, TResult> : IAsyncEnumerable<TResult>
        {
            private readonly RelationalQueryContext _queryContext;
            private readonly IAsyncEnumerable<ValueBuffer> _source;
            private readonly IShaper<TOuter> _outerShaper;
            private readonly IShaper<TInner> _innerShaper;
            private readonly Func<TInner, TKey> _innerKeySelector;
            private readonly Func<TOuter, IAsyncEnumerable<TInner>, TResult> _resultSelector;
            private readonly AsyncGroupJoinInclude _outerGroupJoinInclude;
            private readonly AsyncGroupJoinInclude _innerGroupJoinInclude;

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
            }

            public IAsyncEnumerator<TResult> GetEnumerator() => new GroupJoinAsyncEnumerator(this);

            private class GroupJoinAsyncEnumerator : IAsyncEnumerator<TResult>
            {
                private readonly GroupJoinAsyncEnumerable<TOuter, TInner, TKey, TResult> _groupJoinAsyncEnumerable;
                private readonly IEqualityComparer<TKey> _comparer;

                private IAsyncEnumerator<ValueBuffer> _sourceEnumerator;
                private bool _hasNext;

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
                        _groupJoinAsyncEnumerable._outerGroupJoinInclude?.Initialize(_groupJoinAsyncEnumerable._queryContext);
                        _groupJoinAsyncEnumerable._innerGroupJoinInclude?.Initialize(_groupJoinAsyncEnumerable._queryContext);
                        _sourceEnumerator = _groupJoinAsyncEnumerable._source.GetEnumerator();
                        _hasNext = await _sourceEnumerator.MoveNext();
                    }

                    if (_hasNext)
                    {
                        var outer
                            = _groupJoinAsyncEnumerable._outerShaper
                                .Shape(_groupJoinAsyncEnumerable._queryContext, _sourceEnumerator.Current);

                        if (_groupJoinAsyncEnumerable._outerGroupJoinInclude != null)
                        {
                            await _groupJoinAsyncEnumerable._outerGroupJoinInclude.IncludeAsync(outer, cancellationToken);
                        }

                        var inner
                            = _groupJoinAsyncEnumerable._innerShaper
                                .Shape(_groupJoinAsyncEnumerable._queryContext, _sourceEnumerator.Current);

                        var inners = new List<TInner>();

                        if (inner == null)
                        {
                            Current
                                = _groupJoinAsyncEnumerable._resultSelector(
                                    outer, AsyncLinqOperatorProvider.ToAsyncEnumerable(inners));

                            _hasNext = await _sourceEnumerator.MoveNext();

                            return true;
                        }

                        var currentGroupKey = _groupJoinAsyncEnumerable._innerKeySelector(inner);

                        if (_groupJoinAsyncEnumerable._innerGroupJoinInclude != null)
                        {
                            await _groupJoinAsyncEnumerable._innerGroupJoinInclude.IncludeAsync(inner, cancellationToken);
                        }

                        inners.Add(inner);

                        while (true)
                        {
                            _hasNext = await _sourceEnumerator.MoveNext();

                            if (!_hasNext)
                            {
                                break;
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

                            if (_groupJoinAsyncEnumerable._innerGroupJoinInclude != null)
                            {
                                await _groupJoinAsyncEnumerable._innerGroupJoinInclude.IncludeAsync(inner, cancellationToken);
                            }

                            inners.Add(inner);
                        }

                        Current
                            = _groupJoinAsyncEnumerable._resultSelector(
                                outer, AsyncLinqOperatorProvider.ToAsyncEnumerable(inners));

                        return true;
                    }

                    return false;
                }

                public TResult Current { get; private set; }

                public void Dispose()
                {
                    _sourceEnumerator?.Dispose();
                    _groupJoinAsyncEnumerable._innerGroupJoinInclude?.Dispose();
                    _groupJoinAsyncEnumerable._outerGroupJoinInclude?.Dispose();
                }
            }
        }

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

            return innerResults
                .Select(
                    async (result, cancellationToken) =>
                        {
                            await queryContext.QueryBuffer
                                .IncludeAsync(
                                    queryContext,
                                    entityAccessor == null
                                        ? result
                                        : entityAccessor(result), // TODO: Compile time?
                                    navigationPath,
                                    relatedEntitiesLoaders,
                                    querySourceRequiresTracking,
                                    cancellationToken);

                            return result;
                        })
                .Finally(() =>
                    {
                        foreach (var relatedEntitiesLoader in relatedEntitiesLoaders)
                        {
                            relatedEntitiesLoader.Dispose();
                        }

                        queryContext.EndIncludeScope();
                    });
        }

        public virtual Type RelatedEntitiesLoaderType => typeof(IAsyncRelatedEntitiesLoader);

        public virtual MethodInfo CreateReferenceRelatedEntitiesLoaderMethod => _createReferenceRelatedEntitiesLoaderMethod;

        private static readonly MethodInfo _createReferenceRelatedEntitiesLoaderMethod
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_CreateReferenceRelatedEntitiesLoader));

        [UsedImplicitly]
        private static IAsyncRelatedEntitiesLoader _CreateReferenceRelatedEntitiesLoader(
            int valueBufferOffset,
            int queryIndex,
            Func<ValueBuffer, object> materializer)
            => new ReferenceRelatedEntitiesLoader(valueBufferOffset, queryIndex, materializer);

        private class ReferenceRelatedEntitiesLoader : IAsyncRelatedEntitiesLoader
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

            private class AsyncEnumerableAdapter<T> : IAsyncEnumerable<T>
            {
                private readonly T _value;

                public AsyncEnumerableAdapter(T value)
                {
                    _value = value;
                }

                public IAsyncEnumerator<T> GetEnumerator() => new AsyncEnumeratorAdapter(_value);

                private class AsyncEnumeratorAdapter : IAsyncEnumerator<T>
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

        public virtual MethodInfo CreateCollectionRelatedEntitiesLoaderMethod => _createCollectionRelatedEntitiesLoaderMethod;

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
                    _includeCollectionIterator
                        .GetRelatedValues(keyComparer)
                        .Select(vr => new EntityLoadInfo(vr, _materializer));
            }

            public void Dispose() => _includeCollectionIterator?.Dispose();
        }
    }
}
