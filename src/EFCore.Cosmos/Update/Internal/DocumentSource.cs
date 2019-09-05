// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
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
        public DocumentSource(IEntityType entityType, CosmosDatabaseWrapper database)
        {
            _collectionId = entityType.GetContainer();
            _database = database;
            _idProperty = entityType.FindProperty(StoreKeyConvention.IdPropertyName);
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
        public virtual string GetId(IUpdateEntry entry)
            => entry.GetCurrentValue<string>(_idProperty);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual JObject CreateDocument(IUpdateEntry entry)
        {
            var document = new JObject();
            foreach (var property in entry.EntityType.GetProperties())
            {
                var storeName = property.GetPropertyName();
                if (storeName.Length != 0)
                {
                    document[storeName] = ConvertPropertyValue(property, entry.GetCurrentValue(property));
                }
                else if (entry.HasTemporaryValue(property))
                {
                    ((InternalEntityEntry)entry)[property] = entry.GetCurrentValue(property);
                }
            }

            foreach (var embeddedNavigation in entry.EntityType.GetNavigations())
            {
                var fk = embeddedNavigation.ForeignKey;
                if (!fk.IsOwnership
                    || embeddedNavigation.IsDependentToPrincipal()
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
                    var dependentEntry = ((InternalEntityEntry)entry).StateManager.TryGetEntry(embeddedValue, fk.DeclaringEntityType);
                    document[embeddedPropertyName] = _database.GetDocumentSource(dependentEntry.EntityType).CreateDocument(dependentEntry);
                }
                else
                {
                    var array = new JArray();
                    foreach (var dependent in (IEnumerable)embeddedValue)
                    {
                        var dependentEntry = ((InternalEntityEntry)entry).StateManager.TryGetEntry(dependent, fk.DeclaringEntityType);
                        array.Add(_database.GetDocumentSource(dependentEntry.EntityType).CreateDocument(dependentEntry));
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
        public virtual JObject UpdateDocument(JObject document, IUpdateEntry entry)
        {
            var anyPropertyUpdated = false;
            foreach (var property in entry.EntityType.GetProperties())
            {
                if (entry.EntityState == EntityState.Added
                    || entry.IsModified(property))
                {
                    var storeName = property.GetPropertyName();
                    if (storeName.Length != 0)
                    {
                        document[storeName] = ConvertPropertyValue(property, entry.GetCurrentValue(property));
                        anyPropertyUpdated = true;
                    }
                    else if (entry.HasTemporaryValue(property))
                    {
                        ((InternalEntityEntry)entry)[property] = entry.GetCurrentValue(property);
                    }
                }
            }

            foreach (var ownedNavigation in entry.EntityType.GetNavigations())
            {
                var fk = ownedNavigation.ForeignKey;
                if (!fk.IsOwnership
                    || ownedNavigation.IsDependentToPrincipal()
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
                    var embeddedEntry = ((InternalEntityEntry)entry).StateManager.TryGetEntry(embeddedValue, fk.DeclaringEntityType);
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
                    var array = new JArray();
                    foreach (var dependent in (IEnumerable)embeddedValue)
                    {
                        var embeddedEntry = ((InternalEntityEntry)entry).StateManager.TryGetEntry(dependent, fk.DeclaringEntityType);
                        if (embeddedEntry == null)
                        {
                            continue;
                        }

                        var embeddedDocument = embeddedDocumentSource.GetCurrentDocument(embeddedEntry);
                        embeddedDocument = embeddedDocument != null
                            ? embeddedDocumentSource.UpdateDocument(embeddedDocument, embeddedEntry) ?? embeddedDocument
                            : embeddedDocumentSource.CreateDocument(embeddedEntry);

                        array.Add(embeddedDocument);
                    }

                    document[embeddedPropertyName] = array;
                    anyPropertyUpdated = true;
                }
            }

            return anyPropertyUpdated ? document : null;
        }

        public virtual JObject GetCurrentDocument(IUpdateEntry entry)
            => _jObjectProperty != null
                ? (JObject)(entry.SharedIdentityEntry ?? entry).GetCurrentValue(_jObjectProperty)
                : null;

        private static JToken ConvertPropertyValue(IProperty property, object value)
        {
            if (value == null)
            {
                return null;
            }

            var converter = property.GetTypeMapping().Converter;
            if (converter != null)
            {
                value = converter.ConvertToProvider(value);
            }

            return (value as JToken) ?? JToken.FromObject(value, CosmosClientWrapper.Serializer);
        }
    }
}
