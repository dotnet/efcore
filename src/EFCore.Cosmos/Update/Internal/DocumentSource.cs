// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Update.Internal;

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
    private readonly string _containerId;
    private readonly IEntityType _entityType;
    private readonly IProperty? _idProperty;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public DocumentSource(IEntityType entityType)
    {
        _containerId = entityType.GetContainer()!;
        _entityType = entityType;
        _idProperty = entityType.GetProperties().FirstOrDefault(p => p.GetJsonPropertyName() == CosmosJsonIdConvention.IdPropertyJsonName);
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
    public virtual ReadOnlyMemory<byte> Serialize(IUpdateEntry entry)
    {
        var internalEntry = (IInternalEntry)entry;
        var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
        {
            WriteJsonObject(writer, internalEntry, internalEntry.StructuralType, null);
        }

        return new ReadOnlyMemory<byte>(stream.GetBuffer(), 0, (int)stream.Length);
    }

    private void WriteJsonObject(
        Utf8JsonWriter writer,
        IInternalEntry entry,
        ITypeBase structuralType,
        int? ordinal)
    {
        writer.WriteStartObject();

        foreach (var property in structuralType.GetProperties())
        {
            var jsonPropertyName = property.GetJsonPropertyName();

            if (jsonPropertyName == "")
            {
                if (property.IsKey() && property.IsOrdinalKeyProperty())
                {
                    entry.SetStoreGeneratedValue(property, ordinal!.Value + 1, setModified: false);
                }
                continue;
            }

            var propertyValue = entry.GetCurrentValue(property);
            writer.WritePropertyName(jsonPropertyName);

            var jsonValueReaderWriter = property.GetJsonValueReaderWriter() ?? property.GetTypeMapping().JsonValueReaderWriter;
            if (propertyValue is not null || jsonValueReaderWriter?.HandlesNullWrites == true)
            {
                Check.DebugAssert(jsonValueReaderWriter is not null, $"Missing JsonValueReaderWriter for property: {property}");
                jsonValueReaderWriter.ToJson(writer, propertyValue!);
            }
            else
            {
                writer.WriteNullValue();
            }
        }

        foreach (var complexProperty in structuralType.GetComplexProperties())
        {
            var jsonPropertyName = complexProperty.GetJsonPropertyName()!;
            writer.WritePropertyName(jsonPropertyName);

            WriteJsonStructuralPropertyValue(writer, entry, complexProperty, complexProperty.ComplexType, complexProperty.IsCollection);
        }

        if (structuralType is IEntityType entityType)
        {
            foreach (var navigation in entityType.GetNavigations())
            {
                // skip back-references to the parent
                var fk = navigation.ForeignKey;
                if (!fk.IsOwnership || navigation.IsOnDependent)
                {
                    continue;
                }

                var jsonPropertyName = navigation.TargetEntityType.GetContainingPropertyName();

                Debug.Assert(jsonPropertyName != null, "Containing property name should not be null on owned navigation.");

                writer.WritePropertyName(jsonPropertyName);

                WriteJsonStructuralPropertyValue(writer, entry, navigation, navigation.TargetEntityType, navigation.IsCollection);
            }
        }

        writer.WriteEndObject();
    }

    private void WriteJsonStructuralPropertyValue(
        Utf8JsonWriter writer,
        IInternalEntry parentEntry,
        IPropertyBase property,
        ITypeBase structuralType,
        bool isCollection)
    {
        var value = parentEntry.GetCurrentValue(property);
        if (isCollection)
        {
            WriteJsonArray(writer, parentEntry, property, structuralType, (IEnumerable?)value);
        }
        else
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            var entry = structuralType is IComplexType
                ? parentEntry
                : ((InternalEntityEntry)parentEntry).StateManager.TryGetEntry(value!, (IEntityType)structuralType)!;

            WriteJsonObject(writer, entry, structuralType, null);
        }
    }

    private void WriteJsonArray(
        Utf8JsonWriter writer,
        IInternalEntry parentEntry,
        IPropertyBase property,
        ITypeBase structuralType,
        IEnumerable? value)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        // When items in an owned entity collection are reordered, assigning ordinal key values
        // sequentially can cause identity map conflicts - e.g. assigning ordinal 2 to a new
        // entry while another tracked entry still holds ordinal 2.
        // To avoid this, first assign temporary negative ordinals to move all entries out of
        // the way, then let WriteJsonObject assign the correct final ordinals.
        if (property is INavigation { TargetEntityType: var entityType })
        {
            var ordinalKeyProperty = entityType.FindPrimaryKey()?.Properties
                .FirstOrDefault(p => p.IsOrdinalKeyProperty());
            if (ordinalKeyProperty != null)
            {
                var stateManager = ((InternalEntityEntry)parentEntry).StateManager;
                var tempOrdinal = -1;
                foreach (var collectionElement in value)
                {
                    var tempEntry = stateManager.TryGetEntry(collectionElement, entityType);
                    tempEntry?.SetTemporaryValue(ordinalKeyProperty, tempOrdinal--, setModified: false);
                }
            }
        }

        var i = 0;
        writer.WriteStartArray();
        foreach (var collectionElement in value)
        {
            var entry = structuralType is IComplexType complexType
                ? (IInternalEntry)parentEntry.GetComplexCollectionEntry(complexType.ComplexProperty, i)
                : ((InternalEntityEntry)parentEntry).StateManager.TryGetEntry(collectionElement, (IEntityType)structuralType)!;

            WriteJsonObject(
                writer,
                entry,
                structuralType,
                ordinal: i++);
        }

        writer.WriteEndArray();
        return;
    }
}

#pragma warning restore EF1001 // Internal EF Core API usage.
