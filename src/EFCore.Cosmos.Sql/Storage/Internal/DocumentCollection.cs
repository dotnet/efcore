// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Storage.Internal
{
    public class DocumentCollection<TKey> : IDocumentCollection
    {
        private DocumentClient _documentClient;
        private string _databaseId;
        private string _collectionName;
        private readonly IEntityType _entityType;
        private IPrincipalKeyValueFactory<TKey> _principalKeyValueFactory;

        public DocumentCollection(
            CosmosClient cosmosClient,
            IEntityType entityType,
            IPrincipalKeyValueFactory<TKey> principalKeyValueFactory)
        {
            _documentClient = cosmosClient.DocumentClient;
            _databaseId = cosmosClient.DatabaseId;
            _collectionName = entityType.CosmosSql().CollectionName;
            _entityType = entityType;
            _principalKeyValueFactory = principalKeyValueFactory;
        }

        private string GetId(IUpdateEntry entry)
        {
            var key = _principalKeyValueFactory.CreateFromCurrentValues((InternalEntityEntry)entry);

            // TODO: Escape | Bar|string.Empty
            return key is object[] array ? string.Join("|", array) : key.ToString();
        }

        private Document CreateDocument(string id, IUpdateEntry entry)
        {
            var document = new Document
            {
                Id = id
            };

            foreach (var property in _entityType.GetProperties())
            {
                document.SetPropertyValue(property.Name, entry.GetCurrentValue(property));
            }

            return document;
        }

        public Task SaveAsync(IUpdateEntry entry, CancellationToken cancellationToken = default)
        {
            var id = GetId(entry);

            switch (entry.EntityState)
            {
                case EntityState.Added:
                    return _documentClient.CreateDocumentAsync(
                        UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionName),
                        CreateDocument(id, entry));

                case EntityState.Modified:
                    var document = CreateDocument(id, entry);

                    // Set Discriminator Property for updates
                    document.SetPropertyValue(
                        _entityType.CosmosSql().DiscriminatorProperty.Name,
                        _entityType.CosmosSql().DiscriminatorValue);

                    return _documentClient.ReplaceDocumentAsync(
                        UriFactory.CreateDocumentUri(_databaseId, _collectionName, id),
                        document);

                case EntityState.Deleted:
                    return _documentClient.DeleteDocumentAsync(
                        UriFactory.CreateDocumentUri(_databaseId, _collectionName, id));
            }

            return Task.CompletedTask;
        }
    }
}
