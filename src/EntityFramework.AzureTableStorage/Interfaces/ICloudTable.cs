// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Interfaces
{
    public interface ICloudTable
    {
        Task<ITableResult> ExecuteAsync([NotNull] TableOperation operation, CancellationToken cancellationToken = default(CancellationToken));
        Task<IList<ITableResult>> ExecuteBatchAsync([NotNull] TableBatchOperation batch, CancellationToken cancellationToken = default(CancellationToken));
        bool CreateIfNotExists();
        Task<bool> CreateIfNotExistsAsync(CancellationToken cancellationToken = default(CancellationToken));
        bool DeleteIfExists();
        Task<bool> DeleteIfExistsAsync(CancellationToken cancellationToken = default(CancellationToken));
        IEnumerable<TElement> ExecuteQuery<TElement>([NotNull] AtsTableQuery query, [NotNull] Func<AtsNamedValueBuffer, TElement> resolver) where TElement : class;
    }
}
