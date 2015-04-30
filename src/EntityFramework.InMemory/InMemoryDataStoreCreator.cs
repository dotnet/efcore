// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryDataStoreCreator : IInMemoryDataStoreCreator
    {
        private readonly IInMemoryDataStore _dataStore;

        public InMemoryDataStoreCreator([NotNull] IInMemoryDataStore dataStore)
        {
            Check.NotNull(dataStore, nameof(dataStore));

            _dataStore = dataStore;
        }

        public virtual bool EnsureDeleted(IModel model)
        {
            if (_dataStore.Database.Any())
            {
                _dataStore.Database.Clear();
                return true;
            }
            return false;
        }

        public virtual Task<bool> EnsureDeletedAsync(IModel model, CancellationToken cancellationToken = default(CancellationToken))
            => Task.FromResult(EnsureDeleted(model));

        public virtual bool EnsureCreated(IModel model) => _dataStore.EnsureDatabaseCreated(model);

        public virtual Task<bool> EnsureCreatedAsync(IModel model, CancellationToken cancellationToken = default(CancellationToken))
            => Task.FromResult(_dataStore.EnsureDatabaseCreated(model));
    }
}
