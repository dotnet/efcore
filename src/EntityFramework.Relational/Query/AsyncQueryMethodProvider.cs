// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class AsyncQueryMethodProvider : IQueryMethodProvider
    {
        public virtual MethodInfo GetResultMethod => _getResultMethodInfo;

        private static readonly MethodInfo _getResultMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(GetResult));

        [UsedImplicitly]
        private static async Task<TResult> GetResult<TResult>(
            IAsyncEnumerable<DbDataReader> dataReaders, CancellationToken cancellationToken)
        {
            using (var enumerator = dataReaders.GetEnumerator())
            {
                if (await enumerator.MoveNext(cancellationToken))
                {
                    var result
                        = await enumerator.Current.IsDBNullAsync(0, cancellationToken)
                            .WithCurrentCulture()
                            ? default(TResult)
                            : await enumerator.Current.GetFieldValueAsync<TResult>(0, cancellationToken)
                                .WithCurrentCulture();

                    return result;
                }
            }

            return default(TResult);
        }

        public virtual MethodInfo QueryMethod => _queryMethodInfo;

        private static readonly MethodInfo _queryMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_Query));

        [UsedImplicitly]
        private static IAsyncEnumerable<T> _Query<T>(
            QueryContext queryContext, CommandBuilder commandBuilder, Func<DbDataReader, T> shaper)
        {
            return new AsyncQueryingEnumerable<T>(
                ((RelationalQueryContext)queryContext),
                commandBuilder,
                shaper,
                queryContext.Logger);
        }

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
            where T : QuerySourceScope
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
                    async (querySourceScope, cancellationToken) =>
                        {
                            await queryContext.QueryBuffer
                                .IncludeAsync(
                                    querySourceScope.GetResult(querySource),
                                    navigationPath,
                                    relatedValueBuffers,
                                    cancellationToken,
                                    querySourceRequiresTracking);

                            return querySourceScope;
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

        public virtual MethodInfo CreateReferenceIncludeRelatedValuesStrategyMethod => _createReferenceIncludeStrategyMethodInfo;

        private static readonly MethodInfo _createReferenceIncludeStrategyMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_CreateReferenceIncludeStrategy));

        [UsedImplicitly]
        private static IAsyncIncludeRelatedValuesStrategy _CreateReferenceIncludeStrategy(
            RelationalQueryContext relationalQueryContext,
            IRelationalValueBufferFactory valueBufferFactory,
            int readerIndex,
            Func<ValueBuffer, object> materializer)
        {
            return new ReferenceIncludeRelatedValuesStrategy(
                relationalQueryContext, valueBufferFactory, readerIndex, materializer);
        }

        private class ReferenceIncludeRelatedValuesStrategy : IAsyncIncludeRelatedValuesStrategy
        {
            private readonly RelationalQueryContext _queryContext;
            private readonly IRelationalValueBufferFactory _valueBufferFactory;
            private readonly int _readerIndex;
            private readonly Func<ValueBuffer, object> _materializer;

            public ReferenceIncludeRelatedValuesStrategy(
                RelationalQueryContext queryContext,
                IRelationalValueBufferFactory valueBufferFactory,
                int readerIndex,
                Func<ValueBuffer, object> materializer)
            {
                _queryContext = queryContext;
                _valueBufferFactory = valueBufferFactory;
                _readerIndex = readerIndex;
                _materializer = materializer;
            }

            public IAsyncEnumerable<EntityLoadInfo> GetRelatedValues(EntityKey key, Func<ValueBuffer, EntityKey> keyFactory)
            {
                return new AsyncEnumerableAdapter<EntityLoadInfo>(
                    new EntityLoadInfo(
                        _valueBufferFactory.CreateValueBuffer(_queryContext.GetDataReader(_readerIndex)),
                        _materializer));
            }

            private class AsyncEnumerableAdapter<T> : IAsyncEnumerable<T>
            {
                private readonly T _value;

                public AsyncEnumerableAdapter(T value)
                {
                    _value = value;
                }

                public IAsyncEnumerator<T> GetEnumerator()
                {
                    return new AsyncEnumeratorAdapter(_value);
                }

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

        public virtual MethodInfo CreateCollectionIncludeRelatedValuesStrategyMethod => _createCollectionIncludeStrategyMethodInfo;

        private static readonly MethodInfo _createCollectionIncludeStrategyMethodInfo
            = typeof(AsyncQueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_CreateCollectionIncludeStrategy));

        [UsedImplicitly]
        private static IAsyncIncludeRelatedValuesStrategy _CreateCollectionIncludeStrategy(
            IAsyncEnumerable<ValueBuffer> relatedValueBuffers, Func<ValueBuffer, object> materializer)
        {
            return new CollectionIncludeRelatedValuesStrategy(relatedValueBuffers, materializer);
        }

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

            public void Dispose()
            {
                _includeCollectionIterator.Dispose();
            }
        }
    }
}
