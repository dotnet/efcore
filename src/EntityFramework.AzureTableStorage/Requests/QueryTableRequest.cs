// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;

namespace Microsoft.Data.Entity.AzureTableStorage.Requests
{
    public class QueryTableRequest<TElement> : AtsRequest<IEnumerable<TElement>>
        where TElement : class
    {
        private readonly AtsTableQuery _query;
        private readonly Func<AtsNamedValueBuffer, TElement> _resolver;
        private readonly AtsTable _table;

        public QueryTableRequest(
            [NotNull] AtsTable table, 
            [NotNull] AtsTableQuery query, 
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

        public override IEnumerable<TElement> Execute([NotNull] RequestContext requestContext)
        {
            Check.NotNull(requestContext, "requestContext");
            return requestContext
                .TableClient
                .GetTableReference(_table.Name)
                .ExecuteQuery(_query.ToExecutableQuery(), (key, rowKey, timestamp, properties, etag) =>
                    {
                        var buffer = new AtsNamedValueBuffer(properties);
                        buffer.Add("PartitionKey", key);
                        buffer.Add("RowKey", rowKey);
                        buffer.Add("Timestamp", timestamp);
                        buffer.Add("ETag", etag);
                        return _resolver(buffer);
                    },
                    null,
                    requestContext.OperationContext);
        }
    }
}
