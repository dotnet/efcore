// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Redis.Utilities;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Redis
{
    public class RedisDataStoreCreator : DataStoreCreator
    {
        private readonly LazyRef<RedisDatabase> _database;

        public RedisDataStoreCreator([NotNull] DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            _database = new LazyRef<RedisDatabase>(() => (RedisDatabase)configuration.Database);
        }

        public override bool EnsureDeleted(IModel model)
        {
            _database.Value.FlushDatabase();
            return true;
        }

        public override async Task<bool> EnsureDeletedAsync(IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _database.Value.FlushDatabaseAsync(cancellationToken);
            return true;
        }

        public override bool EnsureCreated(IModel model)
        {
            // returns whether anything changed. In Redis the database is always ready. No need to create anything for each table.
            return false;
        }

        public override Task<bool> EnsureCreatedAsync(IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            // returns whether anything changed. In Redis the database is always ready. No need to create anything for each table.
            return Task.FromResult(false);
        }
    }
}
