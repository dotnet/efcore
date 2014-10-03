// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Requests;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    public class AtsQueryContext : QueryContext
    {
        private readonly AtsConnection _connection;

        private readonly ThreadSafeDictionaryCache<QueryKey, IEnumerable> _requestCache
            = new ThreadSafeDictionaryCache<QueryKey, IEnumerable>();

        public AtsQueryContext(
            [NotNull] ILogger logger,
            [NotNull] IQueryBuffer queryBuffer,
            [NotNull] AtsConnection connection,
            [NotNull] AtsValueReaderFactory readerFactory)
            : base(
                Check.NotNull(logger, "logger"),
                Check.NotNull(queryBuffer, "queryBuffer"))
        {
            Check.NotNull(logger, "logger");
            Check.NotNull(readerFactory, "readerFactory");

            _connection = connection;
            ValueReaderFactory = readerFactory;
            TableQueryGenerator = new TableQueryGenerator();
        }

        public virtual AtsConnection Connection
        {
            get { return _connection; }
        }

        public virtual AtsValueReaderFactory ValueReaderFactory { get; private set; }
        public virtual TableQueryGenerator TableQueryGenerator { get; private set; }

        public virtual IEnumerable<TResult> GetOrAddQueryResults<TResult>([NotNull] QueryTableRequest<TResult> request)
        {
            Check.NotNull(request, "request");

            return _requestCache.GetOrAdd(new QueryKey(request.Table, request.Query),
                q => Connection
                    .ExecuteRequest(request, Logger)
                    .ToList() // prevent multiple execution
                ).Cast<TResult>();
        }

        private struct QueryKey
        {
            public readonly AtsTable Table;
            //for now, ignore SelectColumns
            public readonly TableQuery Query;

            public QueryKey(AtsTable table, TableQuery query)
            {
                Table = table;
                Query = query;
            }

            public bool Equals(QueryKey other)
            {
                return Equals(Table, other.Table)
                       && Equals(Query.FilterString, other.Query.FilterString)
                       && Equals(Query.TakeCount, other.Query.TakeCount);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }
                return obj is QueryKey && Equals((QueryKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = Query.TakeCount.GetHashCode();
                    hashCode = (hashCode * 397) ^ (Query.FilterString != null ? Query.FilterString.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Table != null ? Table.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }
    }
}
