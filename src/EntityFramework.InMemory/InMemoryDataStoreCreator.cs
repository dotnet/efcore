// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
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

        public override bool EnsureDeleted(IModel model)
        {
            if (_dataStore.Database.Any())
            {
                _dataStore.Database.Clear();
                return true;
            }
            return false;
        }

        public override Task<bool> EnsureDeletedAsync(IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(EnsureDeleted(model));
        }

        public override bool EnsureCreated(IModel model)
        {
            return _dataStore.EnsurePersistentDatabaseCreated();
        }

        public override Task<bool> EnsureCreatedAsync(IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(_dataStore.EnsurePersistentDatabaseCreated());
        }
    }
}
