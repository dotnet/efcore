// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Update.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class CosmosDatabaseWrapper : Database
    {
        private readonly Dictionary<IEntityType, DocumentSource> _documentCollections
            = new Dictionary<IEntityType, DocumentSource>();

        private readonly CosmosClientWrapper _cosmosClient;
        private readonly bool _sensitiveLoggingEnabled;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CosmosDatabaseWrapper(
            DatabaseDependencies dependencies,
            CosmosClientWrapper cosmosClient,
            ILoggingOptions loggingOptions)
            : base(dependencies)
        {
            _cosmosClient = cosmosClient;

            if (loggingOptions.IsSensitiveDataLoggingEnabled)
            {
                _sensitiveLoggingEnabled = true;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override int SaveChanges(IList<IUpdateEntry> entries)
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
                    if (!entriesSaved.Contains(root)
                        && rootEntriesToSave.Add(root)
                        && root.EntityState == EntityState.Unchanged)
                    {
                        ((InternalEntityEntry)root).SetEntityState(EntityState.Modified);
                    }

                    continue;
                }

                entriesSaved.Add(entry);
                if (Save(entry))
                {
                    rowsAffected++;
                }
            }

            foreach (var rootEntry in rootEntriesToSave)
            {
                if (!entriesSaved.Contains(rootEntry)
                    && Save(rootEntry))
                {
                    rowsAffected++;
                }
            }

            return rowsAffected;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override async Task<int> SaveChangesAsync(
            IList<IUpdateEntry> entries, CancellationToken cancellationToken = default)
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
                    if (!entriesSaved.Contains(root)
                        && rootEntriesToSave.Add(root)
                        && root.EntityState == EntityState.Unchanged)
                    {
                        ((InternalEntityEntry)root).SetEntityState(EntityState.Modified);
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

        private bool Save(IUpdateEntry entry)
        {
            var entityType = entry.EntityType;
            var documentSource = GetDocumentSource(entityType);
            var collectionId = documentSource.GetCollectionId();
            var state = entry.EntityState;

            if (entry.SharedIdentityEntry != null)
            {
                if (entry.EntityState == EntityState.Deleted)
                {
                    return false;
                }

                if (state == EntityState.Added)
                {
                    state = EntityState.Modified;
                }
            }

            switch (state)
            {
                case EntityState.Added:
                    var newDocument = documentSource.CreateDocument(entry);

                    return _cosmosClient.CreateItem(collectionId, newDocument, GetPartitionKey(entry));
                case EntityState.Modified:
                    var document = documentSource.GetCurrentDocument(entry);
                    if (document != null)
                    {
                        if (documentSource.UpdateDocument(document, entry) == null)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        document = documentSource.CreateDocument(entry);

                        var propertyName = entityType.GetDiscriminatorProperty()?.GetPropertyName();
                        if (propertyName != null)
                        {
                            document[propertyName] =
                                JToken.FromObject(entityType.GetDiscriminatorValue(), CosmosClientWrapper.Serializer);
                        }
                    }

                    return _cosmosClient.ReplaceItem(
                        collectionId, documentSource.GetId(entry.SharedIdentityEntry ?? entry), document, GetPartitionKey(entry));
                case EntityState.Deleted:
                    return _cosmosClient.DeleteItem(collectionId, documentSource.GetId(entry), GetPartitionKey(entry));
                default:
                    return false;
            }
        }

        private Task<bool> SaveAsync(IUpdateEntry entry, CancellationToken cancellationToken)
        {
            var entityType = entry.EntityType;
            var documentSource = GetDocumentSource(entityType);
            var collectionId = documentSource.GetCollectionId();
            var state = entry.EntityState;

            if (entry.SharedIdentityEntry != null)
            {
                if (entry.EntityState == EntityState.Deleted)
                {
                    return Task.FromResult(false);
                }

                if (state == EntityState.Added)
                {
                    state = EntityState.Modified;
                }
            }

            switch (state)
            {
                case EntityState.Added:
                    var newDocument = documentSource.CreateDocument(entry);
                    return _cosmosClient.CreateItemAsync(collectionId, newDocument, GetPartitionKey(entry), cancellationToken);
                case EntityState.Modified:
                    var document = documentSource.GetCurrentDocument(entry);
                    if (document != null)
                    {
                        if (documentSource.UpdateDocument(document, entry) == null)
                        {
                            return Task.FromResult(false);
                        }
                    }
                    else
                    {
                        document = documentSource.CreateDocument(entry);

                        var propertyName = entityType.GetDiscriminatorProperty()?.GetPropertyName();
                        if (propertyName != null)
                        {
                            document[propertyName] =
                                JToken.FromObject(entityType.GetDiscriminatorValue(), CosmosClientWrapper.Serializer);
                        }
                    }

                    return _cosmosClient.ReplaceItemAsync(
                        collectionId, documentSource.GetId(entry.SharedIdentityEntry ?? entry), document, GetPartitionKey(entry),
                        cancellationToken);
                case EntityState.Deleted:
                    return _cosmosClient.DeleteItemAsync(
                        collectionId, documentSource.GetId(entry), GetPartitionKey(entry), cancellationToken);
                default:
                    return Task.FromResult(false);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DocumentSource GetDocumentSource(IEntityType entityType)
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
            var ownership = entry.EntityType.FindOwnership();
            var principal = stateManager.FindPrincipal(entry, ownership);
            if (principal == null)
            {
                if (_sensitiveLoggingEnabled)
                {
                    throw new InvalidOperationException(
                        CosmosStrings.OrphanedNestedDocumentSensitive(
                            entry.EntityType.DisplayName(),
                            ownership.PrincipalEntityType.DisplayName(),
                            entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey().Properties)));
                }

                throw new InvalidOperationException(
                    CosmosStrings.OrphanedNestedDocument(
                        entry.EntityType.DisplayName(),
                        ownership.PrincipalEntityType.DisplayName()));
            }

            return principal.EntityType.IsDocumentRoot() ? principal : GetRootDocument(principal);
        }

        private static string GetPartitionKey(IUpdateEntry entry)
        {
            object partitionKey = null;
            var partitionKeyPropertyName = entry.EntityType.GetPartitionKeyPropertyName();
            if (partitionKeyPropertyName != null)
            {
                var partitionKeyProperty = entry.EntityType.FindProperty(partitionKeyPropertyName);
                partitionKey = entry.GetCurrentValue(partitionKeyProperty);

                var converter = partitionKeyProperty.GetTypeMapping().Converter;
                if (converter != null)
                {
                    partitionKey = converter.ConvertToProvider(partitionKey);
                }
            }

            return (string)partitionKey;
        }
    }
}
