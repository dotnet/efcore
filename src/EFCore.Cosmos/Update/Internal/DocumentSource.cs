// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Update.Internal;

// #16707
#pragma warning disable EF1001 // Internal EF Core API usage.

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class DocumentSource
{
    private readonly CosmosDatabaseWrapper _database;
    private readonly ITypeBase _structuralType;
    private readonly IProperty? _idProperty;
    private readonly IProperty? _jObjectProperty;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public DocumentSource(ITypeBase structuralType, CosmosDatabaseWrapper database)
    {
        _database = database;
        _structuralType = structuralType;
        _idProperty = structuralType.GetProperties().FirstOrDefault(p => p.GetJsonPropertyName() == CosmosJsonIdConvention.IdPropertyJsonName);
        _jObjectProperty = structuralType.FindProperty(CosmosPartitionKeyInPrimaryKeyConvention.JObjectPropertyName);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string GetId(IUpdateEntry entry)
        => _idProperty is null
            ? throw new InvalidOperationException(CosmosStrings.NoIdProperty(_structuralType.DisplayName()))
            : (string)entry.GetCurrentProviderValue(_idProperty)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual JObject CreateDocument(IUpdateEntry entry)
        => CreateDocument((IInternalEntry)entry, entry.EntityType, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual JObject CreateDocument(IInternalEntry entry, ITypeBase structuralType, int? ordinal)
    {
        var document = new JObject();
        foreach (var property in structuralType.GetProperties())
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

        if (structuralType is IEntityType entityType)
        {
            foreach (var embeddedNavigation in entityType.GetNavigations())
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
                    var dependentEntry = ((InternalEntityEntry)entry).StateManager.TryGetEntry(embeddedValue, fk.DeclaringEntityType)!;
                    document[embeddedPropertyName] = _database.GetDocumentSource(dependentEntry.EntityType).CreateDocument(dependentEntry);
                }
                else
                {
                    SetTemporaryOrdinals(entry, fk, embeddedValue);

                    var stateManager = ((InternalEntityEntry)entry).StateManager;

                    var embeddedOrdinal = 1;
                    var array = new JArray();
                    foreach (var dependent in (IEnumerable)embeddedValue)
                    {
                        var dependentEntry = stateManager.TryGetEntry(dependent, fk.DeclaringEntityType)!;
                        array.Add(_database.GetDocumentSource(dependentEntry.EntityType).CreateDocument(dependentEntry, dependentEntry.EntityType, embeddedOrdinal));
                        embeddedOrdinal++;
                    }

                    document[embeddedPropertyName] = array;
                }
            }
        }

        foreach (var complexProperty in structuralType.GetComplexProperties())
        {
            var embeddedValue = entry.GetCurrentValue(complexProperty);
            var embeddedPropertyName = complexProperty.Name;
            if (embeddedValue == null)
            {
                document[embeddedPropertyName] = null;
            }
            else if (!complexProperty.IsCollection)
            {
                document[embeddedPropertyName] = _database.GetDocumentSource(complexProperty.ComplexType).CreateDocument(entry, complexProperty.ComplexType, null);
            }
            else
            {
                var internalEntry = (InternalEntryBase)entry;

                var embeddedOrdinal = 0;
                var array = new JArray();
                foreach (var dependent in (IEnumerable)embeddedValue)
                {
                    var dependentEntry = internalEntry.GetComplexCollectionEntry(complexProperty, embeddedOrdinal);
                    array.Add(_database.GetDocumentSource(dependentEntry.ComplexType).CreateDocument(dependentEntry, complexProperty.ComplexType, null));
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
        => UpdateDocument(document, (IInternalEntry)entry, entry.EntityType, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual JObject? UpdateDocument(JObject document, IInternalEntry entry, ITypeBase structuralType, int? ordinal)
    {
        var anyPropertyUpdated = false;
        foreach (var property in structuralType.GetProperties())
        {
            if (ordinal != null
                && entry.HasTemporaryValue(property)
                && property.IsOrdinalKeyProperty())
            {
                entry.SetStoreGeneratedValue(property, ordinal.Value);
            }

            if (entry.EntityState == EntityState.Added
                || (entry is IUpdateEntry updateEntry && updateEntry.SharedIdentityEntry != null)
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

        if (structuralType is IEntityType entityType)
        {
            foreach (var ownedNavigation in entityType.GetNavigations())
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
                    var embeddedEntry = ((InternalEntityEntry)entry).StateManager.TryGetEntry(embeddedValue, fk.DeclaringEntityType)!;

                    var embeddedDocument = embeddedDocumentSource.GetCurrentDocument((IInternalEntry)embeddedEntry);
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

                    var stateManager = ((InternalEntityEntry)entry).StateManager;

                    var embeddedOrdinal = 1;
                    var array = new JArray();
                    foreach (var dependent in (IEnumerable)embeddedValue)
                    {
                        var embeddedEntry = stateManager.TryGetEntry(dependent, fk.DeclaringEntityType)!;

                        var embeddedDocument = embeddedDocumentSource.GetCurrentDocument((IInternalEntry)embeddedEntry);
                        embeddedDocument = embeddedDocument != null
                            ? embeddedDocumentSource.UpdateDocument(embeddedDocument, embeddedEntry, embeddedEntry.EntityType, embeddedOrdinal) ?? embeddedDocument
                            : embeddedDocumentSource.CreateDocument(embeddedEntry, embeddedEntry.EntityType, embeddedOrdinal);

                        array.Add(embeddedDocument);
                        embeddedOrdinal++;
                    }

                    document[embeddedPropertyName] = array;
                    anyPropertyUpdated = true;
                }
            }
        }

        foreach (var complexProperty in structuralType.GetComplexProperties())
        {
            anyPropertyUpdated = true;

            var embeddedValue = entry.GetCurrentValue(complexProperty);
            var embeddedPropertyName = complexProperty.Name;
            if (embeddedValue == null)
            {
                document[embeddedPropertyName] = null;
            }
            else if (!complexProperty.IsCollection)
            {
                document[embeddedPropertyName] = _database.GetDocumentSource(complexProperty.ComplexType).CreateDocument(entry, complexProperty.ComplexType, null);
            }
            else
            {
                var internalEntry = (InternalEntryBase)entry;

                var embeddedOrdinal = 0;
                var array = new JArray();
                foreach (var dependent in (IEnumerable)embeddedValue)
                {
                    var dependentEntry = internalEntry.GetComplexCollectionEntry(complexProperty, embeddedOrdinal);
                    array.Add(_database.GetDocumentSource(dependentEntry.ComplexType).CreateDocument(dependentEntry, complexProperty.ComplexType, null));
                    embeddedOrdinal++;
                }

                document[embeddedPropertyName] = array;
            }
        }

        return anyPropertyUpdated ? document : null;
    }

    private static void SetTemporaryOrdinals(
        IInternalEntry entry,
        IForeignKey fk,
        object embeddedValue)
    {
        var embeddedOrdinal = 1;
        var ordinalKeyProperty = FindOrdinalKeyProperty(fk.DeclaringEntityType);
        if (ordinalKeyProperty != null)
        {
            var stateManager = ((InternalEntityEntry)entry).StateManager;
            var shouldSetTemporaryKeys = false;
            foreach (var dependent in (IEnumerable)embeddedValue)
            {
                var embeddedEntry = stateManager.TryGetEntry(dependent, fk.DeclaringEntityType)!;

                if ((int)embeddedEntry.GetCurrentValue(ordinalKeyProperty)! != embeddedOrdinal
                    && !embeddedEntry.HasTemporaryValue(ordinalKeyProperty))
                {
                    shouldSetTemporaryKeys = true;
                    break;
                }

                embeddedOrdinal++;
            }

            if (shouldSetTemporaryKeys)
            {
                var temporaryOrdinal = -1;
                foreach (var dependent in (IEnumerable)embeddedValue)
                {
                    var embeddedEntry = stateManager.TryGetEntry(dependent, fk.DeclaringEntityType)!;

                    embeddedEntry.SetTemporaryValue(ordinalKeyProperty, temporaryOrdinal, setModified: false);

                    temporaryOrdinal--;
                }
            }
        }
    }

    private static IProperty? FindOrdinalKeyProperty(IEntityType entityType)
        => entityType.FindPrimaryKey()!.Properties.FirstOrDefault(p => p.GetJsonPropertyName().Length == 0 && p.IsOrdinalKeyProperty());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual JObject? GetCurrentDocument(IUpdateEntry entry)
        => GetCurrentDocument((IInternalEntry)entry);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual JObject? GetCurrentDocument(IInternalEntry entry)
        => _jObjectProperty != null
            ? (JObject?)(((IInternalEntry?)(entry as IUpdateEntry)?.SharedIdentityEntry) ?? entry).GetCurrentValue(_jObjectProperty)
            : null;

    private static JToken? ConvertPropertyValue(IProperty property, IInternalEntry entry)
    {
        var value = entry.GetCurrentProviderValue(property);
        return value == null
            ? null
            : (value as JToken) ?? JToken.FromObject(value, CosmosClientWrapper.Serializer);
    }
}
#pragma warning restore EF1001 // Internal EF Core API usage.
