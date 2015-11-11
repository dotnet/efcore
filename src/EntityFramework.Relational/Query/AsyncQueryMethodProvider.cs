// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query.ExpressionVisitors.Internal;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query
{
    public class AsyncQueryMethodProvider : IQueryMethodProvider
    {
        public virtual MethodInfo ShapedQueryMethod => _shapedQueryMethodInfo;

        private static readonly MethodInfo _shapedQueryMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_ShapedQuery));

        [UsedImplicitly]
        internal static IAsyncEnumerable<T> _ShapedQuery<T>(
            QueryContext queryContext,
            ShaperCommandContext shaperCommandContext,
            IShaper<T> shaper)
            => new AsyncQueryingEnumerable(
                (RelationalQueryContext)queryContext,
                shaperCommandContext,
                queryIndex: null)
                .Select(vb => shaper.Shape(queryContext, vb)); // TODO: Pass shaper to underlying enumerable

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
        internal static async Task<TResult> GetResult<TResult>(
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

        public virtual MethodInfo GroupJoinMethod => _groupJoinMethodInfo;

        private static readonly MethodInfo _groupJoinMethodInfo
            = typeof(AsyncQueryMethodProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_GroupJoin));

        [UsedImplicitly]
        private static IAsyncEnumerable<TResult> _GroupJoin<TOuter, TInner, TKey, TResult>(
            QueryContext queryContext,
            IAsyncEnumerable<ValueBuffer> source,
            IShaper<TOuter> outerShaper,
            IShaper<TInner> innerShaper,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IAsyncEnumerable<TInner>, TResult> resultSelector)
            => new GroupJoinAsyncEnumerable<TOuter, TInner, TKey, TResult>(
                queryContext,
                source,
                outerShaper,
                innerShaper,
                innerKeySelector,
                resultSelector);

        private class GroupJoinAsyncEnumerable<TOuter, TInner, TKey, TResult> : IAsyncEnumerable<TResult>
        {
            private readonly QueryContext _queryContext;
            private readonly IAsyncEnumerable<ValueBuffer> _source;
            private readonly IShaper<TOuter> _outerShaper;
            private readonly IShaper<TInner> _innerShaper;
            private readonly Func<TInner, TKey> _innerKeySelector;
            private readonly Func<TOuter, IAsyncEnumerable<TInner>, TResult> _resultSelector;

            public GroupJoinAsyncEnumerable(
                QueryContext queryContext,
                IAsyncEnumerable<ValueBuffer> source,
                IShaper<TOuter> outerShaper,
                IShaper<TInner> innerShaper,
                Func<TInner, TKey> innerKeySelector,
                Func<TOuter, IAsyncEnumerable<TInner>, TResult> resultSelector)
            {
                _queryContext = queryContext;
                _source = source;
                _outerShaper = outerShaper;
                _innerShaper = innerShaper;
                _innerKeySelector = innerKeySelector;
                _resultSelector = resultSelector;
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
                        _sourceEnumerator = _groupJoinAsyncEnumerable._source.GetEnumerator();
                        _hasNext = await _sourceEnumerator.MoveNext();
                    }

                    if (_hasNext)
                    {
                        var outer
                            = _groupJoinAsyncEnumerable._outerShaper
                                .Shape(_groupJoinAsyncEnumerable._queryContext, _sourceEnumerator.Current);

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

                public void Dispose() => _sourceEnumerator?.Dispose();
            }
        }

        public virtual MethodInfo IncludeMethod => _includeMethodInfo;

        private static readonly MethodInfo _includeMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_Include));

        [UsedImplicitly]
        internal static IAsyncEnumerable<T> _Include<T>(
            RelationalQueryContext queryContext,
            IAsyncEnumerable<T> innerResults,
            Func<T, object> entityAccessor,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<Func<IAsyncIncludeRelatedValuesStrategy>> includeRelatedValuesStrategyFactories,
            bool querySourceRequiresTracking)
        {
            queryContext.BeginIncludeScope();

            var includeRelatedValuesStrategies
                = includeRelatedValuesStrategyFactories
                    .Select(f => f())
                    .ToList();

            var relatedValueBuffers
                = includeRelatedValuesStrategies
                    .Select<IAsyncIncludeRelatedValuesStrategy, AsyncRelatedEntitiesLoader>(s => s.GetRelatedValues)
                    .ToArray();

            return innerResults
                .Select(
                    async (result, cancellationToken) =>
                        {
                            await queryContext.QueryBuffer
                                .IncludeAsync(
                                    entityAccessor == null ? result : entityAccessor(result), // TODO: Compile time?
                                    navigationPath,
                                    relatedValueBuffers,
                                    cancellationToken,
                                    querySourceRequiresTracking);

                            return result;
                        })
                .Finally(() =>
                    {
                        foreach (var includeRelatedValuesStrategy in includeRelatedValuesStrategies)
                        {
                            includeRelatedValuesStrategy.Dispose();
                        }

                        queryContext.EndIncludeScope();
                    });
        }

        public virtual MethodInfo CreateReferenceIncludeRelatedValuesStrategyMethod
            => _createReferenceIncludeStrategyMethodInfo;

        private static readonly MethodInfo _createReferenceIncludeStrategyMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_CreateReferenceIncludeStrategy));

        [UsedImplicitly]
        private static IAsyncIncludeRelatedValuesStrategy _CreateReferenceIncludeStrategy(
            RelationalQueryContext relationalQueryContext,
            int valueBufferOffset,
            int queryIndex,
            Func<ValueBuffer, object> materializer)
            => new ReferenceIncludeRelatedValuesStrategy(
                relationalQueryContext, valueBufferOffset, queryIndex, materializer);

        private class ReferenceIncludeRelatedValuesStrategy : IAsyncIncludeRelatedValuesStrategy
        {
            private readonly RelationalQueryContext _queryContext;
            private readonly int _valueBufferOffset;
            private readonly int _queryIndex;
            private readonly Func<ValueBuffer, object> _materializer;

            public ReferenceIncludeRelatedValuesStrategy(
                RelationalQueryContext queryContext,
                int valueBufferOffset,
                int queryIndex,
                Func<ValueBuffer, object> materializer)
            {
                _queryContext = queryContext;
                _valueBufferOffset = valueBufferOffset;
                _queryIndex = queryIndex;
                _materializer = materializer;
            }

            public IAsyncEnumerable<EntityLoadInfo> GetRelatedValues(IKeyValue keyValue, Func<ValueBuffer, IKeyValue> keyFactory)
            {
                var valueBuffer = _queryContext.GetIncludeValueBuffer(_queryIndex).WithOffset(_valueBufferOffset);

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

        public virtual MethodInfo CreateCollectionIncludeRelatedValuesStrategyMethod
            => _createCollectionIncludeStrategyMethodInfo;

        private static readonly MethodInfo _createCollectionIncludeStrategyMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_CreateCollectionIncludeStrategy));

        [UsedImplicitly]
        private static IAsyncIncludeRelatedValuesStrategy _CreateCollectionIncludeStrategy(
            IAsyncEnumerable<ValueBuffer> relatedValueBuffers, Func<ValueBuffer, object> materializer)
            => new CollectionIncludeRelatedValuesStrategy(relatedValueBuffers, materializer);

        private class CollectionIncludeRelatedValuesStrategy : IAsyncIncludeRelatedValuesStrategy
        {
            private readonly AsyncIncludeCollectionIterator _includeCollectionIterator;
            private readonly Func<ValueBuffer, object> _materializer;

            public CollectionIncludeRelatedValuesStrategy(
                IAsyncEnumerable<ValueBuffer> relatedValueBuffers, Func<ValueBuffer, object> materializer)
            {
                _materializer = materializer;
                _includeCollectionIterator
                    = new AsyncIncludeCollectionIterator(relatedValueBuffers.GetEnumerator());
            }

            public IAsyncEnumerable<EntityLoadInfo> GetRelatedValues(IKeyValue keyValue, Func<ValueBuffer, IKeyValue> keyFactory)
            {
                return _includeCollectionIterator
                    .GetRelatedValues(keyValue, keyFactory)
                    .Select(vr => new EntityLoadInfo(vr, _materializer));
            }

            public void Dispose() => _includeCollectionIterator.Dispose();
        }

        public virtual Type IncludeRelatedValuesFactoryType => typeof(Func<IAsyncIncludeRelatedValuesStrategy>);
    }
}
