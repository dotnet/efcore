// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.Logging;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query
{
    public class AsyncQueryMethodProvider : IQueryMethodProvider
    {
        public virtual MethodInfo GroupJoinMethod => _groupJoinMethodInfo;

        private static readonly MethodInfo _groupJoinMethodInfo
            = typeof(AsyncQueryMethodProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_GroupJoin));

        [UsedImplicitly]
        private static IAsyncEnumerable<QueryResultScope> _GroupJoin<TResult>(
            IAsyncEnumerable<QueryResultScope> source,
            Func<QueryResultScope, IAsyncEnumerable<QueryResultScope<TResult>>, QueryResultScope<IAsyncEnumerable<TResult>>> resultSelector)
            => new GroupJoinAsyncEnumerable<TResult>(source, resultSelector);

        private class GroupJoinAsyncEnumerable<TResult> : IAsyncEnumerable<QueryResultScope>
        {
            private readonly IAsyncEnumerable<QueryResultScope> _source;
            private readonly Func<QueryResultScope, IAsyncEnumerable<QueryResultScope<TResult>>, QueryResultScope<IAsyncEnumerable<TResult>>> _resultSelector;

            public GroupJoinAsyncEnumerable(
                IAsyncEnumerable<QueryResultScope> source,
                Func<QueryResultScope, IAsyncEnumerable<QueryResultScope<TResult>>, QueryResultScope<IAsyncEnumerable<TResult>>> resultSelector)
            {
                _source = source;
                _resultSelector = resultSelector;
            }

            public IAsyncEnumerator<QueryResultScope> GetEnumerator() => new GroupJoinAsyncEnumerator(this);

            private class GroupJoinAsyncEnumerator : IAsyncEnumerator<QueryResultScope>
            {
                private readonly GroupJoinAsyncEnumerable<TResult> _groupJoinAsyncEnumerable;

                private IAsyncEnumerator<QueryResultScope> _sourceEnumerator;
                private bool _hasRows;

                public GroupJoinAsyncEnumerator(GroupJoinAsyncEnumerable<TResult> groupJoinAsyncEnumerable)
                {
                    _groupJoinAsyncEnumerable = groupJoinAsyncEnumerable;
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    if (_sourceEnumerator == null)
                    {
                        _sourceEnumerator = _groupJoinAsyncEnumerable._source.GetEnumerator();
                        _hasRows = await _sourceEnumerator.MoveNext();
                    }

                    if (_hasRows)
                    {
                        var outerQueryResultScope = _sourceEnumerator.Current;

                        var innerQueryResultScopes = new List<QueryResultScope<TResult>>();

                        if (_sourceEnumerator.Current.UntypedResult == null)
                        {
                            _hasRows = await _sourceEnumerator.MoveNext();
                        }
                        else
                        {
                            var parentScope = _sourceEnumerator.Current.ParentScope;

                            while (_hasRows)
                            {
                                innerQueryResultScopes.Add((QueryResultScope<TResult>)_sourceEnumerator.Current);

                                _hasRows = await _sourceEnumerator.MoveNext();

                                if (_hasRows
                                    && !ReferenceEquals(
                                        parentScope.UntypedResult,
                                        _sourceEnumerator.Current.ParentScope.UntypedResult))
                                {
                                    break;
                                }
                            }
                        }

                        Current
                            = _groupJoinAsyncEnumerable._resultSelector(
                                outerQueryResultScope, AsyncLinqOperatorProvider.ToAsyncEnumerable(innerQueryResultScopes));

                        return true;
                    }

                    return false;
                }

                public QueryResultScope Current { get; private set; }

                public void Dispose() => _sourceEnumerator?.Dispose();
            }
        }

        public virtual MethodInfo GetResultMethod => _getResultMethodInfo;

        private static readonly MethodInfo _getResultMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(GetResult));

        [UsedImplicitly]
        private static async Task<TResult> GetResult<TResult>(
            IAsyncEnumerable<ValueBuffer> valueBuffers, CancellationToken cancellationToken)
        {
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

        public virtual MethodInfo ShapedQueryMethod => _shapedQueryMethodInfo;

        private static readonly MethodInfo _shapedQueryMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_ShapedQuery));

        [UsedImplicitly]
        private static IAsyncEnumerable<T> _ShapedQuery<T>(
            QueryContext queryContext,
            CommandBuilder commandBuilder,
            ISensitiveDataLogger logger,
            Func<ValueBuffer, T> shaper)
            => new AsyncQueryingEnumerable(
                ((RelationalQueryContext)queryContext),
                commandBuilder,
                logger,
                queryIndex: null)
                .Select(shaper);

        public virtual MethodInfo QueryMethod => _queryMethodInfo;

        private static readonly MethodInfo _queryMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_Query));

        [UsedImplicitly]
        private static IAsyncEnumerable<ValueBuffer> _Query(
            QueryContext queryContext,
            CommandBuilder commandBuilder,
            ISensitiveDataLogger logger,
            int? queryIndex)
            => new AsyncQueryingEnumerable(
                ((RelationalQueryContext)queryContext),
                commandBuilder,
                logger,
                queryIndex);

        public virtual MethodInfo IncludeMethod => _includeMethodInfo;

        private static readonly MethodInfo _includeMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_Include));

        [UsedImplicitly]
        private static IAsyncEnumerable<T> _Include<T>(
            RelationalQueryContext queryContext,
            IAsyncEnumerable<T> innerResults,
            IQuerySource querySource,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<Func<IAsyncIncludeRelatedValuesStrategy>> includeRelatedValuesStrategyFactories,
            bool querySourceRequiresTracking)
            where T : QueryResultScope
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

            return
                innerResults.Select(
                    async (queryResultScope, cancellationToken) =>
                        {
                            await queryContext.QueryBuffer
                                .IncludeAsync(
                                    queryResultScope.GetResult(querySource),
                                    navigationPath,
                                    relatedValueBuffers,
                                    cancellationToken,
                                    querySourceRequiresTracking);

                            return queryResultScope;
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

        public virtual Type IncludeRelatedValuesFactoryType => typeof(Func<IAsyncIncludeRelatedValuesStrategy>);

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

            public IAsyncEnumerable<EntityLoadInfo> GetRelatedValues(EntityKey key, Func<ValueBuffer, EntityKey> keyFactory)
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

            public IAsyncEnumerable<EntityLoadInfo> GetRelatedValues(EntityKey key, Func<ValueBuffer, EntityKey> keyFactory)
            {
                return _includeCollectionIterator
                    .GetRelatedValues(key, keyFactory)
                    .Select(vr => new EntityLoadInfo(vr, _materializer));
            }

            public void Dispose() => _includeCollectionIterator.Dispose();
        }
    }
}
