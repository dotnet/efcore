// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Interfaces
{
    public interface ICloudTable
    {
        Task<ITableResult> ExecuteAsync(TableOperation operation, CancellationToken cancellationToken = default(CancellationToken));
        Task<IList<ITableResult>> ExecuteBatchAsync(TableBatchOperation batch, CancellationToken cancellationToken = default(CancellationToken));
        void CreateIfNotExists();
        Task CreateIfNotExistsAsync(CancellationToken cancellationToken = default(CancellationToken));
        IEnumerable<TElement> ExecuteQuery<TElement>(TableQuery<TElement> query) where TElement : ITableEntity, new();
    }
}
