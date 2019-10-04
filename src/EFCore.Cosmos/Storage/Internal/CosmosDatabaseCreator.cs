// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Update.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal
{
    public class CosmosDatabaseCreator : IDatabaseCreator
    {
        private readonly CosmosClient _cosmosClient;
        private readonly StateManagerDependencies _stateManagerDependencies;

        public CosmosDatabaseCreator(
            CosmosClient cosmosClient,
            StateManagerDependencies stateManagerDependencies)
        {
            _cosmosClient = cosmosClient;
            _stateManagerDependencies = stateManagerDependencies;
        }

        public bool EnsureCreated()
        {
            var created = _cosmosClient.CreateDatabaseIfNotExists();
            foreach (var collection in _stateManagerDependencies.Model.GetEntityTypes().Select(et => et.Cosmos().ContainerName).Distinct())
            {
                created |= _cosmosClient.CreateDocumentCollectionIfNotExists(collection);
            }

            if (created)
            {
                var stateManager = new StateManager(_stateManagerDependencies);
                foreach (var entityType in _stateManagerDependencies.Model.GetEntityTypes())
                {
                    foreach (var targetSeed in entityType.GetData())
                    {
                        var entry = stateManager.CreateEntry(targetSeed, entityType);
                        entry.SetEntityState(EntityState.Added);
                    }
                }

                stateManager.SaveChanges(acceptAllChangesOnSuccess: false);
            }

            return created;
        }

        public async Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
        {
            var created = await _cosmosClient.CreateDatabaseIfNotExistsAsync(cancellationToken);
            foreach (var collection in _stateManagerDependencies.Model.GetEntityTypes().Select(et => et.Cosmos().ContainerName).Distinct())
            {
                created |= await _cosmosClient.CreateDocumentCollectionIfNotExistsAsync(collection, cancellationToken);
            }

            if (created)
            {
                var stateManager = new StateManager(_stateManagerDependencies);
                foreach (var entityType in _stateManagerDependencies.Model.GetEntityTypes())
                {
                    foreach (var targetSeed in entityType.GetData())
                    {
                        var entry = stateManager.CreateEntry(targetSeed, entityType);
                        entry.SetEntityState(EntityState.Added);
                    }
                }

                await stateManager.SaveChangesAsync(acceptAllChangesOnSuccess: false);
            }

            return created;
        }

        public bool EnsureDeleted() => _cosmosClient.DeleteDatabase();

        public Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default)
            => _cosmosClient.DeleteDatabaseAsync(cancellationToken);
    }
}
