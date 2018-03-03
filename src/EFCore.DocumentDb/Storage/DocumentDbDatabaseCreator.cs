// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class DocumentDbDatabaseCreator : IDatabaseCreator
    {
        private readonly IDocumentDbClientService _documentDbClientService;
        private readonly IModel _model;
        private readonly Uri _databaseUri;

        public DocumentDbDatabaseCreator(IDocumentDbClientService documentDbClientService,
            IModel model)
        {
            _documentDbClientService = documentDbClientService;
            _model = model;
            _databaseUri = UriFactory.CreateDatabaseUri(_documentDbClientService.DatabaseId);
        }

        public bool EnsureDeleted()
        {
            return EnsureDeletedAsync().GetAwaiter().GetResult();
        }

        public async Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default)
        {
            if (await ExistAsync())
            {
                await DeleteAsync();
                return true;
            }

            return false;
        }

        private async Task DeleteAsync()
        {
            await _documentDbClientService.Client.DeleteDatabaseAsync(_databaseUri);
        }

        private async Task<bool> ExistAsync()
        {
            try
            {
                await _documentDbClientService.Client.ReadDatabaseAsync(_databaseUri);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return false;
                }

                throw;
            }

            return true;
        }

        public bool EnsureCreated()
        {
            return EnsureCreatedAsync().GetAwaiter().GetResult();
        }

        public async Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
        {
            if (!await ExistAsync())
            {
                await CreateAsync();
                await CreateCollectionsAsync();
                return true;
            }

            if (!await HasCollectionsAsync())
            {
                await CreateCollectionsAsync();
                return true;
            }

            return false;
        }

        private async Task<bool> HasCollectionsAsync()
        {
            return (await _documentDbClientService.Client.ReadDocumentCollectionFeedAsync(_databaseUri)).Any();
        }

        private async Task CreateCollectionsAsync()
        {
            foreach (var entityType in _model.GetEntityTypes())
            {
                await _documentDbClientService.Client.CreateDocumentCollectionIfNotExistsAsync(
                    _databaseUri,
                    new DocumentCollection { Id = entityType.DocumentDb().CollectionName });
            }
        }

        private async Task CreateAsync()
        {
            await _documentDbClientService.Client.CreateDatabaseAsync(
                new Azure.Documents.Database { Id = _documentDbClientService.DatabaseId });
        }
    }
}
