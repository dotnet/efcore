// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Storage.Internal
{
    public class CosmosSqlDatabaseCreator : IDatabaseCreator
    {
        private readonly CosmosClient _cosmosClient;
        private readonly IModel _model;

        public CosmosSqlDatabaseCreator(CosmosClient cosmosClient, IModel model)
        {
            _cosmosClient = cosmosClient;
            _model = model;
        }

        public bool EnsureCreated()
        {
            var created = _cosmosClient.CreateDatabaseIfNotExists();
            foreach (var collection in _model.GetEntityTypes().Select(et => et.CosmosSql().CollectionName).Distinct())
            {
                created |= _cosmosClient.CreateDocumentCollectionIfNotExists(collection);
            }

            return created;
        }

        public async Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
        {
            var created = await _cosmosClient.CreateDatabaseIfNotExistsAsync(cancellationToken);
            foreach (var collection in _model.GetEntityTypes().Select(et => et.CosmosSql().CollectionName).Distinct())
            {
                created |= await _cosmosClient.CreateDocumentCollectionIfNotExistsAsync(collection, cancellationToken);
            }

            return created;
        }

        public bool EnsureDeleted() => _cosmosClient.DeleteDatabase();

        public Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default)
            => _cosmosClient.DeleteDatabaseAsync(cancellationToken);
    }
}
