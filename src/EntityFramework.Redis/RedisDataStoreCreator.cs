// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Redis
{
    public class RedisDataStoreCreator : DataStoreCreator
    {
        private readonly RedisDataStore _datastore;

        public RedisDataStoreCreator([NotNull] RedisDataStore dataStore)
        {
            Check.NotNull(dataStore, "dataStore");

            _datastore = dataStore;
        }

        public override bool EnsureDeleted(IModel model)
        {
            _datastore.FlushDatabase();
            return true;
        }

        public override async Task<bool> EnsureDeletedAsync(IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _datastore.FlushDatabaseAsync(cancellationToken).WithCurrentCulture();

            return true;
        }

        public override bool EnsureCreated(IModel model)
        {
            // returns whether anything changed. In Redis the database is always ready. No need to create anything for each table.
            return true;
        }

        public override Task<bool> EnsureCreatedAsync(IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            // returns whether anything changed. In Redis the database is always ready. No need to create anything for each table.
            return Task.FromResult(true);
        }
    }
}
