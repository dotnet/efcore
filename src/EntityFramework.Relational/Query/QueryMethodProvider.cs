// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query
{
    public class QueryMethodProvider : IQueryMethodProvider
    {
        public virtual MethodInfo ShapedQueryMethod => _shapedQueryMethodInfo;

        private static readonly MethodInfo _shapedQueryMethodInfo
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_ShapedQuery));

        [UsedImplicitly]
        private static IEnumerable<T> _ShapedQuery<T>(
            QueryContext queryContext,
            CommandBuilder commandBuilder,
            Func<ValueBuffer, T> shaper)
            => new QueryingEnumerable(
                (RelationalQueryContext)queryContext,
                commandBuilder,
                queryIndex: null)
                .Select(shaper); // TODO: Pass shaper to underlying enumerable

        public virtual MethodInfo QueryMethod => _queryMethodInfo;

        private static readonly MethodInfo _queryMethodInfo
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_Query));

        [UsedImplicitly]
        private static IEnumerable<ValueBuffer> _Query(
            QueryContext queryContext,
            CommandBuilder commandBuilder,
            int? queryIndex)
            => new QueryingEnumerable(
                ((RelationalQueryContext)queryContext),
                commandBuilder,
                queryIndex);

        public virtual MethodInfo GetResultMethod => _getResultMethodInfo;

        private static readonly MethodInfo _getResultMethodInfo
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(GetResult));

        [UsedImplicitly]
        private static TResult GetResult<TResult>(IEnumerable<ValueBuffer> valueBuffers)
        {
            using (var enumerator = valueBuffers.GetEnumerator())
            {
                if (enumerator.MoveNext())
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
            = typeof(QueryMethodProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_GroupJoin));

        [UsedImplicitly]
        private static IEnumerable<TResult> _GroupJoin<TOuter, TInner, TKey, TResult>(
            IEnumerable<ValueBuffer> source,
            Func<ValueBuffer, TOuter> outerFactory,
            Func<ValueBuffer, TInner> innerFactory,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
        {
            using (var sourceEnumerator = source.GetEnumerator())
            {
                var comparer = EqualityComparer<TKey>.Default;
                var hasNext = sourceEnumerator.MoveNext();

                while (hasNext)
                {
                    var outer = outerFactory(sourceEnumerator.Current);
                    var inner = innerFactory(sourceEnumerator.Current);
                    var inners = new List<TInner>();

                    if (inner == null)
                    {
                        yield return resultSelector(outer, inners);

                        hasNext = sourceEnumerator.MoveNext();
                    }
                    else
                    {
                        var currentGroupKey = innerKeySelector(inner);

                        inners.Add(inner);

                        while (true)
                        {
                            hasNext = sourceEnumerator.MoveNext();

                            if (!hasNext)
                            {
                                break;
                            }

                            inner = innerFactory(sourceEnumerator.Current);

                            if (inner == null)
                            {
                                break;
                            }

                            var innerKey = innerKeySelector(inner);

                            if (!comparer.Equals(currentGroupKey, innerKey))
                            {
                                break;
                            }

                            inners.Add(inner);
                        }

                        yield return resultSelector(outer, inners);
                    }
                }
            }
        }

        public virtual MethodInfo IncludeMethod => _includeMethodInfo;

        private static readonly MethodInfo _includeMethodInfo
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_Include));

        [UsedImplicitly]
        private static IEnumerable<T> _Include<T>(
            RelationalQueryContext queryContext,
            IEnumerable<T> innerResults,
            Func<T, object> entityAccessor,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<Func<IIncludeRelatedValuesStrategy>> includeRelatedValuesStrategyFactories,
            bool querySourceRequiresTracking)
        {
            queryContext.BeginIncludeScope();

            var includeRelatedValuesStrategies
                = includeRelatedValuesStrategyFactories
                    .Select(f => f())
                    .ToList();

            var relatedEntitiesLoaders
                = includeRelatedValuesStrategies
                    .Select<IIncludeRelatedValuesStrategy, RelatedEntitiesLoader>(s => s.GetRelatedValues)
                    .ToArray();

            return innerResults
                .Select(r =>
                    {
                        queryContext.QueryBuffer
                            .Include(
                                entityAccessor == null ? r : entityAccessor(r), // TODO: Compile time?
                                navigationPath,
                                relatedEntitiesLoaders,
                                querySourceRequiresTracking);

                        return r;
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
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_CreateReferenceIncludeStrategy));

        [UsedImplicitly]
        private static IIncludeRelatedValuesStrategy _CreateReferenceIncludeStrategy(
            RelationalQueryContext relationalQueryContext,
            int valueBufferOffset,
            int queryIndex,
            Func<ValueBuffer, object> materializer)
            => new ReferenceIncludeRelatedValuesStrategy(
                relationalQueryContext, valueBufferOffset, queryIndex, materializer);

        private class ReferenceIncludeRelatedValuesStrategy : IIncludeRelatedValuesStrategy
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

            public IEnumerable<EntityLoadInfo> GetRelatedValues(IKeyValue keyValue, Func<ValueBuffer, IKeyValue> keyFactory)
            {
                var valueBuffer = _queryContext.GetIncludeValueBuffer(_queryIndex).WithOffset(_valueBufferOffset);

                yield return new EntityLoadInfo(valueBuffer, _materializer);
            }

            public void Dispose()
            {
                // no-op
            }
        }

        public virtual MethodInfo CreateCollectionIncludeRelatedValuesStrategyMethod
            => _createCollectionIncludeStrategyMethodInfo;

        private static readonly MethodInfo _createCollectionIncludeStrategyMethodInfo
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_CreateCollectionIncludeStrategy));

        [UsedImplicitly]
        private static IIncludeRelatedValuesStrategy _CreateCollectionIncludeStrategy(
            IEnumerable<ValueBuffer> relatedValueBuffers, Func<ValueBuffer, object> materializer)
            => new CollectionIncludeRelatedValuesStrategy(relatedValueBuffers, materializer);

        private class CollectionIncludeRelatedValuesStrategy : IIncludeRelatedValuesStrategy
        {
            private readonly IncludeCollectionIterator _includeCollectionIterator;
            private readonly Func<ValueBuffer, object> _materializer;

            public CollectionIncludeRelatedValuesStrategy(
                IEnumerable<ValueBuffer> relatedValueBuffers, Func<ValueBuffer, object> materializer)
            {
                _materializer = materializer;
                _includeCollectionIterator
                    = new IncludeCollectionIterator(relatedValueBuffers.GetEnumerator());
            }

            public IEnumerable<EntityLoadInfo> GetRelatedValues(IKeyValue keyValue, Func<ValueBuffer, IKeyValue> keyFactory)
            {
                return
                    _includeCollectionIterator
                        .GetRelatedValues(keyValue, keyFactory)
                        .Select(vr => new EntityLoadInfo(vr, _materializer));
            }

            public void Dispose() => _includeCollectionIterator.Dispose();
        }

        public virtual Type IncludeRelatedValuesFactoryType => typeof(Func<IIncludeRelatedValuesStrategy>);
    }
}
