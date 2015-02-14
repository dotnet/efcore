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
                        : enumerator.Current.GetFieldValue<TResult>(0);
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

            var relatedValueReaders
                = includeRelatedValuesStrategies
                    .Select<
                        IIncludeRelatedValuesStrategy,
                        Func<EntityKey, Func<IValueReader, EntityKey>, IEnumerable<IValueReader>>>(
                            s => s.GetRelatedValues)
                    .ToArray();

            return innerResults
                .Select(qss =>
                    {
                        queryContext.QueryBuffer
                            .Include(
                                qss.GetResult(querySource),
                                navigationPath,
                                relatedValueReaders,
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
            RelationalQueryContext relationalQueryContext, int readerIndex, int readerOffset)
        {
            return new ReferenceIncludeRelatedValuesStrategy(relationalQueryContext, readerIndex, readerOffset);
        }

        private class ReferenceIncludeRelatedValuesStrategy : IIncludeRelatedValuesStrategy
        {
            private readonly RelationalQueryContext _queryContext;
            private readonly int _readerIndex;
            private readonly int _readerOffset;

            public ReferenceIncludeRelatedValuesStrategy(
                RelationalQueryContext queryContext, int readerIndex, int readerOffset)
            {
                _queryContext = queryContext;
                _readerIndex = readerIndex;
                _readerOffset = readerOffset;
            }

            public IEnumerable<IValueReader> GetRelatedValues(EntityKey key, Func<IValueReader, EntityKey> keyFactory)
            {
                yield return new OffsetValueReaderDecorator(
                    _queryContext.CreateValueReader(_readerIndex),
                    _readerOffset);
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
        private static IIncludeRelatedValuesStrategy _CreateCollectionIncludeStrategy(IEnumerable<IValueReader> relatedValueReaders)
        {
            return new CollectionIncludeRelatedValuesStrategy(relatedValueReaders);
        }

        private class CollectionIncludeRelatedValuesStrategy : IIncludeRelatedValuesStrategy
        {
            private readonly IncludeCollectionIterator _includeCollectionIterator;

            public CollectionIncludeRelatedValuesStrategy(IEnumerable<IValueReader> relatedValueReaders)
            {
                _includeCollectionIterator
                    = new IncludeCollectionIterator(relatedValueReaders.GetEnumerator());
            }

            public IEnumerable<IValueReader> GetRelatedValues(EntityKey key, Func<IValueReader, EntityKey> keyFactory)
            {
                return _includeCollectionIterator.GetRelatedValues(key, keyFactory);
            }

            public void Dispose()
            {
                _includeCollectionIterator.Dispose();
            }
        }
    }
}
