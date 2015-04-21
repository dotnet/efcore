// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Utilities;
using Microsoft.Data.Entity.Storage;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class QueryMethodProvider : IQueryMethodProvider
    {
        public virtual MethodInfo GetResultMethod => _getResultMethodInfo;

        private static readonly MethodInfo _getResultMethodInfo
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod("GetResult");

        [UsedImplicitly]
        private static TResult GetResult<TResult>(IEnumerable<DbDataReader> dataReaders)
        {
            using (var enumerator = dataReaders.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return enumerator.Current.IsDBNull(0)
                        ? default(TResult)
                        : (TResult)enumerator.Current.GetValue(0);
                }
            }

            return default(TResult);
        }

        public virtual MethodInfo QueryMethod => _queryMethodInfo;

        private static readonly MethodInfo _queryMethodInfo
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod("_Query");

        [UsedImplicitly]
        private static IEnumerable<T> _Query<T>(
            QueryContext queryContext, CommandBuilder commandBuilder, Func<DbDataReader, T> shaper)
        {
            return new QueryingEnumerable<T>(
                ((RelationalQueryContext)queryContext),
                commandBuilder,
                shaper,
                queryContext.Logger);
        }

        public virtual MethodInfo IncludeMethod => _includeMethodInfo;

        private static readonly MethodInfo _includeMethodInfo
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod("_Include");

        [UsedImplicitly]
        private static IEnumerable<T> _Include<T>(
            RelationalQueryContext queryContext,
            IEnumerable<T> innerResults,
            IQuerySource querySource,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<Func<IIncludeRelatedValuesStrategy>> includeRelatedValuesStrategyFactories,
            bool querySourceRequiresTracking)
            where T : QuerySourceScope
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
                .GetDeclaredMethod("_CreateReferenceIncludeStrategy");

        [UsedImplicitly]
        private static IIncludeRelatedValuesStrategy _CreateReferenceIncludeStrategy(
            RelationalQueryContext relationalQueryContext,
            IRelationalValueReaderFactory valueReaderFactory,
            int readerIndex,
            int readerOffset,
            Func<IValueReader, object> materializer)
        {
            return new ReferenceIncludeRelatedValuesStrategy(
                relationalQueryContext, valueReaderFactory, readerIndex, readerOffset, materializer);
        }

        private class ReferenceIncludeRelatedValuesStrategy : IIncludeRelatedValuesStrategy
        {
            private readonly RelationalQueryContext _queryContext;
            private readonly IRelationalValueReaderFactory _valueReaderFactory;
            private readonly int _readerIndex;
            private readonly int _readerOffset;
            private readonly Func<IValueReader, object> _materializer;

            public ReferenceIncludeRelatedValuesStrategy(
                RelationalQueryContext queryContext,
                IRelationalValueReaderFactory valueReaderFactory,
                int readerIndex,
                int readerOffset,
                Func<IValueReader, object> materializer)
            {
                _queryContext = queryContext;
                _valueReaderFactory = valueReaderFactory;
                _readerIndex = readerIndex;
                _readerOffset = readerOffset;
                _materializer = materializer;
            }

            public IEnumerable<EntityLoadInfo> GetRelatedValues(EntityKey key, Func<IValueReader, EntityKey> keyFactory)
            {
                yield return
                    new EntityLoadInfo(
                        new OffsetValueReaderDecorator(
                            _valueReaderFactory.CreateValueReader(_queryContext.GetDataReader(_readerIndex)),
                            _readerOffset),
                        _materializer);
            }

            public void Dispose()
            {
                // no-op
            }
        }

        public virtual MethodInfo CreateCollectionIncludeRelatedValuesStrategyMethod => _createCollectionIncludeStrategyMethodInfo;

        private static readonly MethodInfo _createCollectionIncludeStrategyMethodInfo
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod("_CreateCollectionIncludeStrategy");

        [UsedImplicitly]
        private static IIncludeRelatedValuesStrategy _CreateCollectionIncludeStrategy(
            IEnumerable<IValueReader> relatedValueReaders, Func<IValueReader, object> materializer)
        {
            return new CollectionIncludeRelatedValuesStrategy(relatedValueReaders, materializer);
        }

        private class CollectionIncludeRelatedValuesStrategy : IIncludeRelatedValuesStrategy
        {
            private readonly IncludeCollectionIterator _includeCollectionIterator;
            private readonly Func<IValueReader, object> _materializer;

            public CollectionIncludeRelatedValuesStrategy(
                IEnumerable<IValueReader> relatedValueReaders, Func<IValueReader, object> materializer)
            {
                _materializer = materializer;
                _includeCollectionIterator
                    = new IncludeCollectionIterator(relatedValueReaders.GetEnumerator());
            }

            public IEnumerable<EntityLoadInfo> GetRelatedValues(EntityKey key, Func<IValueReader, EntityKey> keyFactory)
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
