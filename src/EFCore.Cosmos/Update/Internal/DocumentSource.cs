// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Update;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Update.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class DocumentSource
    {
        private readonly string _collectionId;
        private readonly CosmosDatabaseWrapper _database;
        private readonly IProperty _idProperty;
        private readonly IProperty _jObjectProperty;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public DocumentSource([NotNull] IEntityType entityType, [NotNull] CosmosDatabaseWrapper database)
        {
            _collectionId = entityType.GetContainer();
            _database = database;
            _idProperty = entityType.GetProperties().FirstOrDefault(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);
            _jObjectProperty = entityType.FindProperty(StoreKeyConvention.JObjectPropertyName);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string GetCollectionId()
            => _collectionId;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string GetId([NotNull] IUpdateEntry entry)
            => (string)entry.GetCurrentProviderValue(_idProperty);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual JObject CreateDocument([NotNull] IUpdateEntry entry)
            => CreateDocument(entry, null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual JObject CreateDocument([NotNull] IUpdateEntry entry, int? ordinal)
        {
            var document = new JObject();
            foreach (var property in entry.EntityType.GetProperties())
            {
                var storeName = property.GetJsonPropertyName();
                if (storeName.Length != 0)
                {
                    document[storeName] = ConvertPropertyValue(property, entry);
                }
                else if (entry.HasTemporaryValue(property))
                {
                    if (ordinal != null
                        && property.IsOrdinalKeyProperty())
                    {
                        entry.SetStoreGeneratedValue(property, ordinal.Value);
                    }
                }
            }

            foreach (var embeddedNavigation in entry.EntityType.GetNavigations())
            {
                var fk = embeddedNavigation.ForeignKey;
                if (!fk.IsOwnership
                    || embeddedNavigation.IsOnDependent
                    || fk.DeclaringEntityType.IsDocumentRoot())
                {
                    continue;
                }

                var embeddedValue = entry.GetCurrentValue(embeddedNavigation);
                var embeddedPropertyName = fk.DeclaringEntityType.GetContainingPropertyName();
                if (embeddedValue == null)
                {
                    document[embeddedPropertyName] = null;
                }
                else if (fk.IsUnique)
                {
#pragma warning disable EF1001 // Internal EF Core API usage.
                    // #16707
                    var dependentEntry = ((InternalEntityEntry)entry).StateManager.TryGetEntry(embeddedValue, fk.DeclaringEntityType);
                    document[embeddedPropertyName] = _database.GetDocumentSource(dependentEntry.EntityType).CreateDocument(dependentEntry);
#pragma warning restore EF1001 // Internal EF Core API usage.
                }
                else
                {
                    var embeddedOrdinal = 1;
                    var array = new JArray();
                    foreach (var dependent in (IEnumerable)embeddedValue)
                    {
#pragma warning disable EF1001 // Internal EF Core API usage.
                        // #16707
                        var dependentEntry = ((InternalEntityEntry)entry).StateManager.TryGetEntry(dependent, fk.DeclaringEntityType);
                        array.Add(_database.GetDocumentSource(dependentEntry.EntityType).CreateDocument(dependentEntry, embeddedOrdinal));
#pragma warning restore EF1001 // Internal EF Core API usage.
                        embeddedOrdinal++;
                    }

                    document[embeddedPropertyName] = array;
                }
            }

            return document;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual JObject UpdateDocument([NotNull] JObject document, [NotNull] IUpdateEntry entry)
            => UpdateDocument(document, entry, null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual JObject UpdateDocument([NotNull] JObject document, [NotNull] IUpdateEntry entry, int? ordinal)
        {
            var anyPropertyUpdated = false;
#pragma warning disable EF1001 // Internal EF Core API usage.
            // #16707
            var stateManager = ((InternalEntityEntry)entry).StateManager;
#pragma warning restore EF1001 // Internal EF Core API usage.
            foreach (var property in entry.EntityType.GetProperties())
            {
                if (entry.EntityState == EntityState.Added
                    || entry.IsModified(property))
                {
                    var storeName = property.GetJsonPropertyName();
                    if (storeName.Length != 0)
                    {
                        document[storeName] = ConvertPropertyValue(property, entry);
                        anyPropertyUpdated = true;
                    }
                }

                if (ordinal != null
                    && entry.HasTemporaryValue(property)
                    && property.IsOrdinalKeyProperty())
                {
                    entry.SetStoreGeneratedValue(property, ordinal.Value);
                }
            }

            foreach (var ownedNavigation in entry.EntityType.GetNavigations())
            {
                var fk = ownedNavigation.ForeignKey;
                if (!fk.IsOwnership
                    || ownedNavigation.IsOnDependent
                    || fk.DeclaringEntityType.IsDocumentRoot())
                {
                    continue;
                }

                var embeddedDocumentSource = _database.GetDocumentSource(fk.DeclaringEntityType);
                var embeddedValue = entry.GetCurrentValue(ownedNavigation);
                var embeddedPropertyName = fk.DeclaringEntityType.GetContainingPropertyName();
                if (embeddedValue == null)
                {
                    if (document[embeddedPropertyName] != null)
                    {
                        document[embeddedPropertyName] = null;
                        anyPropertyUpdated = true;
                    }
                }
                else if (fk.IsUnique)
                {
#pragma warning disable EF1001 // Internal EF Core API usage.
                    // #16707
                    var embeddedEntry = ((InternalEntityEntry)entry).StateManager.TryGetEntry(embeddedValue, fk.DeclaringEntityType);
#pragma warning restore EF1001 // Internal EF Core API usage.
                    if (embeddedEntry == null)
                    {
                        continue;
                    }

                    var embeddedDocument = embeddedDocumentSource.GetCurrentDocument(embeddedEntry);
                    embeddedDocument = embeddedDocument != null
                        ? embeddedDocumentSource.UpdateDocument(embeddedDocument, embeddedEntry)
                        : embeddedDocumentSource.CreateDocument(embeddedEntry);

                    if (embeddedDocument != null)
                    {
                        document[embeddedPropertyName] = embeddedDocument;
                        anyPropertyUpdated = true;
                    }
                }
                else
                {
                    var embeddedOrdinal = 1;
                    var ordinalKeyProperty = GetOrdinalKeyProperty(fk.DeclaringEntityType);
                    if (ordinalKeyProperty != null)
                    {
                        var shouldSetTemporaryKeys = false;
                        foreach (var dependent in (IEnumerable)embeddedValue)
                        {
#pragma warning disable EF1001 // Internal EF Core API usage.
                            // #16707
                            var embeddedEntry = stateManager.TryGetEntry(dependent, fk.DeclaringEntityType);
                            if (embeddedEntry == null)
                            {
                                continue;
                            }

                            if ((int)embeddedEntry.GetCurrentValue(ordinalKeyProperty) != embeddedOrdinal)
                            {
                                shouldSetTemporaryKeys = true;
                                break;
                            }
#pragma warning restore EF1001 // Internal EF Core API usage.

                            embeddedOrdinal++;
                        }

                        if (shouldSetTemporaryKeys)
                        {
                            var temporaryOrdinal = -1;
                            foreach (var dependent in (IEnumerable)embeddedValue)
                            {
#pragma warning disable EF1001 // Internal EF Core API usage.
                                // #16707
                                var embeddedEntry = stateManager.TryGetEntry(dependent, fk.DeclaringEntityType);
                                if (embeddedEntry == null)
                                {
                                    continue;
                                }

                                embeddedEntry.SetTemporaryValue(ordinalKeyProperty, temporaryOrdinal, setModified: false);
#pragma warning restore EF1001 // Internal EF Core API usage.

                                temporaryOrdinal--;
                            }
                        }
                    }

                    embeddedOrdinal = 1;
                    var array = new JArray();
                    foreach (var dependent in (IEnumerable)embeddedValue)
                    {
#pragma warning disable EF1001 // Internal EF Core API usage.
                        // #16707
                        var embeddedEntry = ((InternalEntityEntry)entry).StateManager.TryGetEntry(dependent, fk.DeclaringEntityType);
#pragma warning restore EF1001 // Internal EF Core API usage.
                        if (embeddedEntry == null)
                        {
                            continue;
                        }

                        var embeddedDocument = embeddedDocumentSource.GetCurrentDocument(embeddedEntry);
                        embeddedDocument = embeddedDocument != null
                            ? embeddedDocumentSource.UpdateDocument(embeddedDocument, embeddedEntry, embeddedOrdinal) ?? embeddedDocument
                            : embeddedDocumentSource.CreateDocument(embeddedEntry, embeddedOrdinal);

                        array.Add(embeddedDocument);
                        embeddedOrdinal++;
                    }

                    document[embeddedPropertyName] = array;
                    anyPropertyUpdated = true;
                }
            }

            return anyPropertyUpdated ? document : null;
        }

        private IProperty GetOrdinalKeyProperty(IEntityType entityType)
            => entityType.FindPrimaryKey().Properties.FirstOrDefault(
                p =>
                    p.GetJsonPropertyName().Length == 0 && p.IsOrdinalKeyProperty());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual JObject GetCurrentDocument([NotNull] IUpdateEntry entry)
            => _jObjectProperty != null
                ? (JObject)(entry.SharedIdentityEntry ?? entry).GetCurrentValue(_jObjectProperty)
                : null;

        private static JToken ConvertPropertyValue(IProperty property, IUpdateEntry entry)
        {
            var value = entry.GetCurrentProviderValue(property);
            return value == null
                ? null
                : (value as JToken) ?? JToken.FromObject(value, CosmosClientWrapper.Serializer);
        }
    }
}
