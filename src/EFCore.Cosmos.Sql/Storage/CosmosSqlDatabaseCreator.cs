// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Metadata;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Storage
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
            throw new NotImplementedException();
        }

        public async Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
        {
            var created = false;

            var dbResponse = await _cosmosClient.DocumentClient.CreateDatabaseIfNotExistsAsync(
                new Azure.Documents.Database { Id = _cosmosClient.DatabaseId });

            created |= dbResponse.StatusCode == System.Net.HttpStatusCode.Created;

            var _databaseUri = UriFactory.CreateDatabaseUri(_cosmosClient.DatabaseId);
            foreach (var collection in _model.GetEntityTypes().Select(et => et.CosmosSql().CollectionName).Distinct())
            {
                var collectionResponse = await _cosmosClient.DocumentClient.CreateDocumentCollectionIfNotExistsAsync(
                    _databaseUri,
                    new DocumentCollection { Id = collection });

                created |= collectionResponse.StatusCode == System.Net.HttpStatusCode.Created;
            }

            return created;
        }

        public bool EnsureDeleted()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _cosmosClient.DocumentClient.DeleteDatabaseAsync(
                    UriFactory.CreateDatabaseUri(_cosmosClient.DatabaseId));
            }
            catch (DocumentClientException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            return true;
        }
    }
}
