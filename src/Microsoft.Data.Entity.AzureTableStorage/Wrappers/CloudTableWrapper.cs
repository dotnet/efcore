// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.AzureTableStorage.Interfaces;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Wrappers
{
    public class CloudTableWrapper : ICloudTable
    {
        private readonly CloudTable _table;

        public CloudTableWrapper(CloudTable table)
        {
            _table = table;
        }

        public Task<ITableResult> ExecuteAsync(TableOperation operation, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run<ITableResult>(
                () => new TableResultWrapper(_table.ExecuteAsync(operation, cancellationToken).Result)
                , cancellationToken);
        }

        public Task<IList<ITableResult>> ExecuteBatchAsync(TableBatchOperation batch, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run<IList<ITableResult>>(
                () => _table.ExecuteBatchAsync(batch, cancellationToken)
                    .Result
                    .Select(s => new TableResultWrapper(s))
                    .ToList<ITableResult>(),
                cancellationToken);
        }

        public void CreateIfNotExists()
        {
            CreateIfNotExistsAsync().Wait();
        }

        public Task CreateIfNotExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _table.CreateIfNotExistsAsync(cancellationToken);
        }

        public IEnumerable<TElement> ExecuteQuery<TElement>(TableQuery<TElement> query) where TElement : ITableEntity, new()
        {
            return _table.ExecuteQuery(query);
        }
    }
}
