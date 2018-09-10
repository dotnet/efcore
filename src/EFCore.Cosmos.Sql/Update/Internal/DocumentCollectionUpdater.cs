// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Update.Internal
{
    public class DocumentCollectionUpdater
    {
        private readonly CosmosClient _cosmosClient;
        private readonly string _collectionId;
        private readonly IEntityType _entityType;

        public DocumentCollectionUpdater(
            CosmosClient cosmosClient,
            IEntityType entityType)
        {
            _cosmosClient = cosmosClient;
            _collectionId = entityType.CosmosSql().CollectionName;
            _entityType = entityType;
        }

        private JObject CreateDocument(IUpdateEntry entry)
        {
            var document = new JObject();
            foreach (var property in _entityType.GetProperties())
            {
                var value = entry.GetCurrentValue(property);
                document[property.Name] = value != null ? JToken.FromObject(value) : null;
            }

            return document;
        }

        public bool Save(IUpdateEntry entry)
        {
            var id = entry.GetCurrentValue<string>(_entityType.FindProperty("id"));

            switch (entry.EntityState)
            {
                case EntityState.Added:
                    return _cosmosClient.CreateDocument(_collectionId, CreateDocument(entry));

                case EntityState.Modified:
                    var document = CreateDocument(entry);

                    // Set Discriminator Property for updates
                    document[_entityType.CosmosSql().DiscriminatorProperty.Name] =
                        JToken.FromObject(_entityType.CosmosSql().DiscriminatorValue);

                    return _cosmosClient.ReplaceDocument(_collectionId, id, document);

                case EntityState.Deleted:
                    return _cosmosClient.DeleteDocument(_collectionId, id);
            }

            return false;
        }

        public Task<bool> SaveAsync(IUpdateEntry entry, CancellationToken cancellationToken = default)
        {
            var id = entry.GetCurrentValue<string>(_entityType.FindProperty("id"));

            switch (entry.EntityState)
            {
                case EntityState.Added:
                    return _cosmosClient.CreateDocumentAsync(_collectionId, CreateDocument(entry), cancellationToken);

                case EntityState.Modified:
                    var document = CreateDocument(entry);

                    // Set Discriminator Property for updates
                    document[_entityType.CosmosSql().DiscriminatorProperty.Name] =
                        JToken.FromObject(_entityType.CosmosSql().DiscriminatorValue);

                    return _cosmosClient.ReplaceDocumentAsync(_collectionId, id, document, cancellationToken);

                case EntityState.Deleted:
                    return _cosmosClient.DeleteDocumentAsync(_collectionId, id, cancellationToken);
            }

            return Task.FromResult(false);
        }
    }
}
