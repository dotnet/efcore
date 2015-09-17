// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Storage;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query
{
    public class QueryMethodProvider : IQueryMethodProvider
    {
        public virtual MethodInfo GroupJoinMethod => _groupJoinMethodInfo;

        private static readonly MethodInfo _groupJoinMethodInfo
            = typeof(QueryMethodProvider)
                .GetTypeInfo().GetDeclaredMethod(nameof(_GroupJoin));

        [UsedImplicitly]
        private static IEnumerable<QueryResultScope> _GroupJoin<TResult>(
            IEnumerable<QueryResultScope> source,
            Func<QueryResultScope, IEnumerable<QueryResultScope<TResult>>, QueryResultScope<IEnumerable<TResult>>> resultSelector)
        {
            using (var sourceEnumerator = source.GetEnumerator())
            {
                var hasRows = sourceEnumerator.MoveNext();

                while (hasRows)
                {
                    var outerQueryResultScope = sourceEnumerator.Current;

                    var innerQueryResultScopes = new List<QueryResultScope<TResult>>();

                    if (sourceEnumerator.Current.UntypedResult == null)
                    {
                        hasRows = sourceEnumerator.MoveNext();
                    }
                    else
                    {
                        var parentScope = sourceEnumerator.Current.ParentScope;

                        while (hasRows)
                        {
                            innerQueryResultScopes.Add((QueryResultScope<TResult>)sourceEnumerator.Current);

                            hasRows = sourceEnumerator.MoveNext();

                            if (hasRows
                                && !ReferenceEquals(
                                    parentScope.UntypedResult,
                                    sourceEnumerator.Current.ParentScope.UntypedResult))
                            {
                                break;
                            }
                        }
                    }

                    yield return resultSelector(outerQueryResultScope, innerQueryResultScopes);
                }
            }
        }

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

        public virtual MethodInfo ShapedQueryMethod => _shapedQueryMethodInfo;

        private static readonly MethodInfo _shapedQueryMethodInfo
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_ShapedQuery));

        [UsedImplicitly]
        private static IEnumerable<T> _ShapedQuery<T>(
            QueryContext queryContext, CommandBuilder commandBuilder, Func<ValueBuffer, T> shaper)
            => new QueryingEnumerable(
                    ((RelationalQueryContext)queryContext),
                    commandBuilder,
                    null)
                    .Select(shaper);

        public virtual MethodInfo QueryMethod => _queryMethodInfo;

        private static readonly MethodInfo _queryMethodInfo
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_Query));

        [UsedImplicitly]
        private static IEnumerable<ValueBuffer> _Query(
            QueryContext queryContext, 
            CommandBuilder commandBuilder,
            int? queryIndex)
        {
            return
                new QueryingEnumerable(
                    ((RelationalQueryContext)queryContext),
                    commandBuilder,
                    queryIndex);
        }

        public virtual MethodInfo IncludeMethod => _includeMethodInfo;

        private static readonly MethodInfo _includeMethodInfo
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_Include));

        [UsedImplicitly]
        private static IEnumerable<T> _Include<T>(
            RelationalQueryContext queryContext,
            IEnumerable<T> innerResults,
            IQuerySource querySource,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<Func<IIncludeRelatedValuesStrategy>> includeRelatedValuesStrategyFactories,
            bool querySourceRequiresTracking)
            where T : QueryResultScope
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
                .Select(qss =>
                    {
                        queryContext.QueryBuffer
                            .Include(
                                qss.GetResult(querySource),
                                navigationPath,
                                relatedEntitiesLoaders,
                                querySourceRequiresTracking);

                        return qss;
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

        public virtual Type IncludeRelatedValuesFactoryType => typeof(Func<IIncludeRelatedValuesStrategy>);

        public virtual MethodInfo CreateReferenceIncludeRelatedValuesStrategyMethod => _createReferenceIncludeStrategyMethodInfo;

        private static readonly MethodInfo _createReferenceIncludeStrategyMethodInfo
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_CreateReferenceIncludeStrategy));

        [UsedImplicitly]
        private static IIncludeRelatedValuesStrategy _CreateReferenceIncludeStrategy(
            RelationalQueryContext relationalQueryContext,
            int valueBufferOffset,
            int queryIndex,
            Func<ValueBuffer, object> materializer)
        {
            return new ReferenceIncludeRelatedValuesStrategy(
                relationalQueryContext, valueBufferOffset, queryIndex, materializer);
        }

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

            public IEnumerable<EntityLoadInfo> GetRelatedValues(EntityKey key, Func<ValueBuffer, EntityKey> keyFactory)
            {
                var valueBuffer = _queryContext.GetIncludeValueBuffer(_queryIndex).WithOffset(_valueBufferOffset);

                yield return new EntityLoadInfo(valueBuffer, _materializer);
            }

            public void Dispose()
            {
                // no-op
            }
        }

        public virtual MethodInfo CreateCollectionIncludeRelatedValuesStrategyMethod => _createCollectionIncludeStrategyMethodInfo;

        private static readonly MethodInfo _createCollectionIncludeStrategyMethodInfo
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod(nameof(_CreateCollectionIncludeStrategy));

        [UsedImplicitly]
        private static IIncludeRelatedValuesStrategy _CreateCollectionIncludeStrategy(
            IEnumerable<ValueBuffer> relatedValueBuffers, Func<ValueBuffer, object> materializer)
        {
            return new CollectionIncludeRelatedValuesStrategy(relatedValueBuffers, materializer);
        }

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

            public IEnumerable<EntityLoadInfo> GetRelatedValues(EntityKey key, Func<ValueBuffer, EntityKey> keyFactory)
            {
                return
                    _includeCollectionIterator
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
