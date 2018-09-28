// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Update.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Storage.Internal
{
    public class CosmosSqlDatabase : Database
    {
        private readonly Dictionary<IEntityType, DocumentSource> _documentCollections
            = new Dictionary<IEntityType, DocumentSource>();
        private readonly CosmosClient _cosmosClient;

        public CosmosSqlDatabase(
            DatabaseDependencies dependencies,
            CosmosClient cosmosClient)
            : base(dependencies)
        {
            _cosmosClient = cosmosClient;
        }

        public override int SaveChanges(IReadOnlyList<IUpdateEntry> entries)
        {
            var rowsAffected = 0;
            var entriesSaved = new HashSet<IUpdateEntry>();
            var rootEntriesToSave = new HashSet<IUpdateEntry>();

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var entityType = entry.EntityType;

                Debug.Assert(!entityType.IsAbstract());

                if (!entityType.IsDocumentRoot())
                {
                    var root = GetRootDocument((InternalEntityEntry)entry);
                    if (!entriesSaved.Contains(root))
                    {
                        rootEntriesToSave.Add(root);
                    }
                    continue;
                }

                entriesSaved.Add(entry);
                if (Save(entry))
                {
                    rowsAffected++;
                }
            }

            foreach(var rootEntry in rootEntriesToSave)
            {
                if (!entriesSaved.Contains(rootEntry)
                    && Save(rootEntry))
                {
                    rowsAffected++;
                }
            }

            return rowsAffected;
        }

        private bool Save(IUpdateEntry entry)
        {
            var entityType = entry.EntityType;
            var documentSource = GetDocumentSource(entityType);
            var collectionId = documentSource.GetCollectionId();

            switch (entry.EntityState)
            {
                case EntityState.Added:
                    return _cosmosClient.CreateDocument(collectionId, documentSource.CreateDocument(entry));
                case EntityState.Modified:
                    var jObjectProperty = entityType.FindProperty(StoreKeyConvention.JObjectPropertyName);
                    var document = jObjectProperty != null ? (JObject)entry.GetCurrentValue(jObjectProperty) : null;
                    if (document != null)
                    {
                        documentSource.UpdateDocument(document, entry);
                    }
                    else
                    {
                        document = documentSource.CreateDocument(entry);

                        // Set Discriminator Property for updates
                        document[entityType.CosmosSql().DiscriminatorProperty.Name] =
                            JToken.FromObject(entityType.CosmosSql().DiscriminatorValue);
                    }

                    return _cosmosClient.ReplaceDocument(collectionId, documentSource.GetId(entry), document);
                case EntityState.Deleted:
                    return _cosmosClient.DeleteDocument(collectionId, documentSource.GetId(entry));
                default:
                    return false;
            }
        }

        public override async Task<int> SaveChangesAsync(
            IReadOnlyList<IUpdateEntry> entries, CancellationToken cancellationToken = default)
        {
            var rowsAffected = 0;
            var entriesSaved = new HashSet<IUpdateEntry>();
            var rootEntriesToSave = new HashSet<IUpdateEntry>();

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var entityType = entry.EntityType;

                Debug.Assert(!entityType.IsAbstract());
                if (!entityType.IsDocumentRoot())
                {
                    var root = GetRootDocument((InternalEntityEntry)entry);
                    if (!entriesSaved.Contains(root))
                    {
                        rootEntriesToSave.Add(root);
                    }
                    continue;
                }

                entriesSaved.Add(entry);
                if (await SaveAsync(entry, cancellationToken))
                {
                    rowsAffected++;
                }
            }

            foreach (var rootEntry in rootEntriesToSave)
            {
                if (!entriesSaved.Contains(rootEntry)
                    && await SaveAsync(rootEntry, cancellationToken))
                {
                    rowsAffected++;
                }
            }

            return rowsAffected;
        }

        private Task<bool> SaveAsync(IUpdateEntry entry, CancellationToken cancellationToken)
        {
            var entityType = entry.EntityType;
            var documentSource = GetDocumentSource(entityType);
            var collectionId = documentSource.GetCollectionId();

            switch (entry.EntityState)
            {
                case EntityState.Added:
                    return _cosmosClient.CreateDocumentAsync(collectionId, documentSource.CreateDocument(entry), cancellationToken);
                case EntityState.Modified:
                    var jObjectProperty = entityType.FindProperty(StoreKeyConvention.JObjectPropertyName);
                    var document = jObjectProperty != null ? (JObject)entry.GetCurrentValue(jObjectProperty) : null;
                    if (document != null)
                    {
                        documentSource.UpdateDocument(document, entry);
                    }
                    else
                    {
                        document = documentSource.CreateDocument(entry);

                        // Set Discriminator Property for updates
                        document[entityType.CosmosSql().DiscriminatorProperty.Name] =
                            JToken.FromObject(entityType.CosmosSql().DiscriminatorValue);
                    }

                    return _cosmosClient.ReplaceDocumentAsync(collectionId, documentSource.GetId(entry), document, cancellationToken);
                case EntityState.Deleted:
                    return _cosmosClient.DeleteDocumentAsync(collectionId, documentSource.GetId(entry), cancellationToken);
                default:
                    return Task.FromResult(false);
            }
        }

        public DocumentSource GetDocumentSource(IEntityType entityType)
        {
            if (!_documentCollections.TryGetValue(entityType, out var documentSource))
            {
                _documentCollections.Add(
                    entityType, documentSource = new DocumentSource(entityType, this));
            }

            return documentSource;
        }

        private IUpdateEntry GetRootDocument(InternalEntityEntry entry)
        {
            var stateManager = entry.StateManager;
            var principal = stateManager.GetPrincipal(entry, entry.EntityType.FindOwnership());
            return principal.EntityType.IsDocumentRoot() ? principal : GetRootDocument(principal);
        }
    }
}
