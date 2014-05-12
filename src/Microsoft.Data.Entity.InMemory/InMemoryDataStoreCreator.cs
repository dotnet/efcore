// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.InMemory.Utilities;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryDataStoreCreator : DataStoreCreator
    {
        private readonly InMemoryDataStore _dataStore;

        public InMemoryDataStoreCreator([NotNull] InMemoryDataStore dataStore)
        {
            Check.NotNull(dataStore, "dataStore");

            _dataStore = dataStore;
        }

        public override void Create()
        {
        }

        public override Task CreateAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(0);
        }

        public override void CreateTables(IModel model)
        {
        }

        public override Task CreateTablesAsync(IModel model, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(0);
        }

        public override bool Exists()
        {
            return true;
        }

        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(true);
        }

        public override void Delete()
        {
            _dataStore.Database.Clear();
        }

        public override Task DeleteAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            _dataStore.Database.Clear();

            return Task.FromResult(0);
        }

        public override bool HasTables()
        {
            return true;
        }

        public override Task<bool> HasTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(true);
        }
    }
}
