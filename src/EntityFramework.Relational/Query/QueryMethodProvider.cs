// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;

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

        public virtual MethodInfo IncludeCollectionMethod => _includeCollectionMethodInfo;

        private static readonly MethodInfo _includeCollectionMethodInfo
            = typeof(QueryMethodProvider).GetTypeInfo()
                .GetDeclaredMethod("_IncludeCollection");

        [UsedImplicitly]
        private static IEnumerable<TResult> _IncludeCollection<TResult>(
            QueryContext queryContext,
            IEnumerable<TResult> source,
            INavigation navigation,
            IEnumerable<IValueReader> relatedValueReaders,
            Func<TResult, object> accessorLambda)
        {
            using (var relatedValuesIterator
                = new IncludeCollectionIterator(relatedValueReaders.GetEnumerator()))
            {
                foreach (var result in source)
                {
                    queryContext.QueryBuffer
                        .Include(
                            accessorLambda.Invoke(result),
                            new[] { navigation },
                            new Func<EntityKey, Func<IValueReader, EntityKey>, IEnumerable<IValueReader>>[]
                                {
                                    relatedValuesIterator.GetRelatedValues
                                });

                    yield return result;
                }
            }
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
                ((RelationalQueryContext)queryContext).Connection,
                commandBuilder,
                shaper,
                queryContext.Logger);
        }
    }
}
