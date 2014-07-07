// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Framework.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Requests
{
    public class QueryTableRequest<TElement> : AtsRequest<IEnumerable<TElement>>
    {
        private readonly TableQuery _query;
        private readonly Func<AtsNamedValueBuffer, TElement> _resolver;
        private readonly AtsTable _table;

        public QueryTableRequest(
            [NotNull] AtsTable table,
            [NotNull] TableQuery query,
            [NotNull] Func<AtsNamedValueBuffer, TElement> resolver)
        {
            Check.NotNull(table, "table");
            Check.NotNull(query, "query");
            Check.NotNull(resolver, "resolver");
            
            _query = query;
            _resolver = resolver;
            _table = table;
        }

        public override string Name
        {
            get { return "QueryTableRequest"; }
        }

        public virtual TableQuery Query
        {
            get { return _query; }
        }

        public virtual AtsTable Table
        {
            get { return _table; }
        }

        public override IEnumerable<TElement> Execute([NotNull] RequestContext requestContext)
        {
            Check.NotNull(requestContext, "requestContext");

            if (requestContext.Logger != null)
            {
                var queryString = _query.FilterString;
                if (String.IsNullOrEmpty(queryString))
                {
                    requestContext.Logger.WriteWarning(Strings.MissingFilterString);
                }
                else if (!queryString.Contains("PartitionKey")
                         || !queryString.Contains("RowKey"))
                {
                    requestContext.Logger.WriteWarning(Strings.MissingPartitionOrRowKey);
                }
            }

            return requestContext
                .TableClient
                .GetTableReference(_table.Name)
                .ExecuteQuery(_query, (key, rowKey, timestamp, properties, etag) =>
                    {
                        var buffer = new AtsNamedValueBuffer(properties);
                        buffer.Add("PartitionKey", key);
                        buffer.Add("RowKey", rowKey);
                        buffer.Add("Timestamp", timestamp);
                        buffer.Add("ETag", etag);
                        return _resolver(buffer);
                    },
                    requestContext.TableRequestOptions,
                    requestContext.OperationContext);
        }

        protected bool Equals(QueryTableRequest<TElement> other)
        {
            return Equals(_query, other._query) && Equals(_table, other._table);
        }

        public override bool Equals([CanBeNull] object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((QueryTableRequest<TElement>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_query != null ? _query.GetHashCode() : 0) * 397) ^ (_table != null ? _table.GetHashCode() : 0);
            }
        }
    }
}
