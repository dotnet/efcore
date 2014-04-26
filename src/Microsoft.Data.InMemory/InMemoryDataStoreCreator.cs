// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.InMemory.Utilities;

namespace Microsoft.Data.InMemory
{
    public class InMemoryDataStoreCreator : DataStoreCreator
    {
        private readonly InMemoryDataStore _dataStore;

        public InMemoryDataStoreCreator([NotNull] InMemoryDataStore dataStore)
        {
            Check.NotNull(dataStore, "dataStore");

            _dataStore = dataStore;
        }

        public override Task CreateAsync(IModel model, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(0);
        }

        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(true);
        }

        public override Task DeleteAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            _dataStore.Database.Clear();

            return Task.FromResult(0);
        }

        public override void Create(IModel model)
        {
        }

        public override bool Exists()
        {
            return true;
        }

        public override void Delete()
        {
            _dataStore.Database.Clear();
        }
    }
}
