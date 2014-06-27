// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Interfaces;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Wrappers
{
    public class CloudTableWrapper : ICloudTable
    {
        private readonly CloudTable _table;

        public CloudTableWrapper([NotNull] CloudTable table)
        {
            Check.NotNull(table, "table");
            _table = table;
        }

        public virtual Task<ITableResult> ExecuteAsync([NotNull] TableOperation operation, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(operation, "operation");
            return Task.Run<ITableResult>(
                () => new TableResultWrapper(_table.ExecuteAsync(operation, cancellationToken).Result)
                , cancellationToken);
        }

        public virtual Task<IList<ITableResult>> ExecuteBatchAsync([NotNull] TableBatchOperation batch, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(batch, "batch");
            return Task.Run<IList<ITableResult>>(
                () => _table.ExecuteBatchAsync(batch, cancellationToken)
                    .Result
                    .Select(s => new TableResultWrapper(s))
                    .ToList<ITableResult>(),
                cancellationToken);
        }

        public virtual bool CreateIfNotExists()
        {
            return _table.CreateIfNotExists();
        }

        public virtual Task<bool> CreateIfNotExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _table.CreateIfNotExistsAsync(cancellationToken);
        }

        public virtual bool DeleteIfExists()
        {
            return _table.DeleteIfExists();
        }

        public virtual Task<bool> DeleteIfExistsAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _table.DeleteIfExistsAsync(cancellationToken);
        }

        public virtual IEnumerable<TElement> ExecuteQuery<TElement>([NotNull] AtsTableQuery query, [NotNull] Func<AtsNamedValueBuffer, TElement> resolver) where TElement : class
        {
            Check.NotNull(query, "query");
            Check.NotNull(resolver, "resolver");
            return _table.ExecuteQuery(query.ToExecutableQuery(), (key, rowKey, timestamp, properties, etag) =>
                {
                    var buffer = new AtsNamedValueBuffer(properties);
                    buffer.Add("PartitionKey", key);
                    buffer.Add("RowKey", rowKey);
                    buffer.Add("Timestamp", timestamp);
                    buffer.Add("ETag", etag);
                    return resolver(buffer);
                });
        }
    }
}
