// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class DocumentSource
{
    private readonly string _containerId;
    private readonly CosmosDatabaseWrapper _database;
    private readonly IEntityType _entityType;
    private readonly IProperty? _idProperty;
    private readonly IProperty? _jObjectProperty;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public DocumentSource(IEntityType entityType, CosmosDatabaseWrapper database)
    {
        _containerId = entityType.GetContainer()!;
        _database = database;
        _entityType = entityType;
        _idProperty = entityType.GetProperties().FirstOrDefault(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);
        _jObjectProperty = entityType.FindProperty(StoreKeyConvention.JObjectPropertyName);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string GetContainerId()
        => _containerId;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string GetId(IUpdateEntry entry)
        => _idProperty is null
            ? throw new InvalidOperationException(CosmosStrings.NoIdProperty(_entityType.DisplayName()))
            : (string)entry.GetCurrentProviderValue(_idProperty)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual JObject CreateDocument(IUpdateEntry entry)
        => CreateDocument(entry, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual JObject CreateDocument(IUpdateEntry entry, int? ordinal)
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
            var embeddedPropertyName = fk.DeclaringEntityType.GetContainingPropertyName()!;
            if (embeddedValue == null)
            {
                document[embeddedPropertyName] = null;
            }
            else if (fk.IsUnique)
            {
#pragma warning disable EF1001 // Internal EF Core API usage.
                // #16707
                var dependentEntry = ((InternalEntityEntry)entry).StateManager.TryGetEntry(embeddedValue, fk.DeclaringEntityType)!;
                document[embeddedPropertyName] = _database.GetDocumentSource(dependentEntry.EntityType).CreateDocument(dependentEntry);
#pragma warning restore EF1001 // Internal EF Core API usage.
            }
            else
            {
                SetTemporaryOrdinals(entry, fk, embeddedValue);

#pragma warning disable EF1001 // Internal EF Core API usage.
                // #16707
                var stateManager = ((InternalEntityEntry)entry).StateManager;
#pragma warning restore EF1001 // Internal EF Core API usage.

                var embeddedOrdinal = 1;
                var array = new JArray();
                foreach (var dependent in (IEnumerable)embeddedValue)
                {
#pragma warning disable EF1001 // Internal EF Core API usage.
                    // #16707
                    var dependentEntry = stateManager.TryGetEntry(dependent, fk.DeclaringEntityType)!;
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
    public virtual JObject? UpdateDocument(JObject document, IUpdateEntry entry)
        => UpdateDocument(document, entry, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual JObject? UpdateDocument(JObject document, IUpdateEntry entry, int? ordinal)
    {
        var anyPropertyUpdated = false;
        foreach (var property in entry.EntityType.GetProperties())
        {
            if (ordinal != null
                && entry.HasTemporaryValue(property)
                && property.IsOrdinalKeyProperty())
            {
                entry.SetStoreGeneratedValue(property, ordinal.Value);
            }

            if (entry.EntityState == EntityState.Added
                || entry.SharedIdentityEntry != null
                || entry.IsModified(property))
            {
                var storeName = property.GetJsonPropertyName();
                if (storeName.Length != 0)
                {
                    document[storeName] = ConvertPropertyValue(property, entry);
                    anyPropertyUpdated = true;
                }
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
            var embeddedPropertyName = fk.DeclaringEntityType.GetContainingPropertyName()!;
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
                var embeddedEntry = ((InternalEntityEntry)entry).StateManager.TryGetEntry(embeddedValue, fk.DeclaringEntityType)!;
#pragma warning restore EF1001 // Internal EF Core API usage.

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
                SetTemporaryOrdinals(entry, fk, embeddedValue);

#pragma warning disable EF1001 // Internal EF Core API usage.
                // #16707
                var stateManager = ((InternalEntityEntry)entry).StateManager;
#pragma warning restore EF1001 // Internal EF Core API usage.

                var embeddedOrdinal = 1;
                var array = new JArray();
                foreach (var dependent in (IEnumerable)embeddedValue)
                {
#pragma warning disable EF1001 // Internal EF Core API usage.
                    // #16707
                    var embeddedEntry = stateManager.TryGetEntry(dependent, fk.DeclaringEntityType)!;
#pragma warning restore EF1001 // Internal EF Core API usage.

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

    private static void SetTemporaryOrdinals(
        IUpdateEntry entry,
        IForeignKey fk,
        object embeddedValue)
    {
        var embeddedOrdinal = 1;
        var ordinalKeyProperty = FindOrdinalKeyProperty(fk.DeclaringEntityType);
        if (ordinalKeyProperty != null)
        {
#pragma warning disable EF1001 // Internal EF Core API usage.
            // #16707
            var stateManager = ((InternalEntityEntry)entry).StateManager;
#pragma warning restore EF1001 // Internal EF Core API usage.
            var shouldSetTemporaryKeys = false;
            foreach (var dependent in (IEnumerable)embeddedValue)
            {
#pragma warning disable EF1001 // Internal EF Core API usage.
                // #16707
                var embeddedEntry = stateManager.TryGetEntry(dependent, fk.DeclaringEntityType)!;

                if ((int)embeddedEntry.GetCurrentValue(ordinalKeyProperty)! != embeddedOrdinal
                    && !embeddedEntry.HasTemporaryValue(ordinalKeyProperty))
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
                    var embeddedEntry = stateManager.TryGetEntry(dependent, fk.DeclaringEntityType)!;

                    embeddedEntry.SetTemporaryValue(ordinalKeyProperty, temporaryOrdinal, setModified: false);
#pragma warning restore EF1001 // Internal EF Core API usage.

                    temporaryOrdinal--;
                }
            }
        }
    }

    private static IProperty? FindOrdinalKeyProperty(IEntityType entityType)
        => entityType.FindPrimaryKey()!.Properties.FirstOrDefault(
            p => p.GetJsonPropertyName().Length == 0 && p.IsOrdinalKeyProperty());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual JObject? GetCurrentDocument(IUpdateEntry entry)
        => _jObjectProperty != null
            ? (JObject?)(entry.SharedIdentityEntry ?? entry).GetCurrentValue(_jObjectProperty)
            : null;

    private static JToken? ConvertPropertyValue(IProperty property, IUpdateEntry entry)
    {
        var value = entry.GetCurrentProviderValue(property);
        return value == null
            ? null
            : (value as JToken) ?? JToken.FromObject(value, CosmosClientWrapper.Serializer);
    }
}
