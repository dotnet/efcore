// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryDataStoreCreator : IDataStoreCreator
    {
        private readonly IModel _model;
        private readonly IInMemoryDataStore _dataStore;

        public InMemoryDataStoreCreator([NotNull] IInMemoryDataStore dataStore, [NotNull] IModel model)
        {
            Check.NotNull(dataStore, nameof(dataStore));
            Check.NotNull(model, nameof(model));

            _dataStore = dataStore;
            _model = model;
        }

        public virtual bool EnsureDeleted()
        {
            if (_dataStore.Database.Any())
            {
                _dataStore.Database.Clear();
                return true;
            }
            return false;
        }

        public virtual Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default(CancellationToken))
            => Task.FromResult(EnsureDeleted());

        public virtual bool EnsureCreated() => _dataStore.EnsureDatabaseCreated(_model);

        public virtual Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default(CancellationToken))
            => Task.FromResult(_dataStore.EnsureDatabaseCreated(_model));
    }
}
