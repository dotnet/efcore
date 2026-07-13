// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Update.Internal;

#pragma warning disable EF1001 // Internal EF Core API usage.

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosStructuralTypeSerializer
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly MethodInfo SerializeInstanceMethod
        = typeof(CosmosStructuralTypeSerializer).GetMethod(nameof(Serialize), [typeof(object), typeof(bool)]) ?? throw new UnreachableException();

    private readonly ITypeBase _structuralType;
    private readonly IProperty? _jsonIdProperty;
    private readonly IProperty? _discriminatorProperty;
    private readonly IProperty? _ordinalKeyProperty;

    /// <summary>
    /// Any properties that have to be written to the document (excluding the discriminator property)
    /// </summary>
    private readonly IProperty[] _scalarProperties;

    private readonly (IComplexProperty ComplexProperty, CosmosStructuralTypeSerializer Serializer)[] _complexProperties;
    private readonly (INavigation Navigation, CosmosStructuralTypeSerializer Serializer)[] _navigations = [];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosStructuralTypeSerializer(CosmosStructuralTypeSerializerProvider provider, ITypeBase structuralType)
    {
        _structuralType = structuralType;

        _discriminatorProperty = structuralType.FindDiscriminatorProperty();
        _ordinalKeyProperty = structuralType.GetProperties().SingleOrDefault(p => p.IsOrdinalKeyProperty());
        _scalarProperties = [.. structuralType.GetProperties().Where(p => p.IsPersisted() && p != _discriminatorProperty)];
        _complexProperties = [.. structuralType.GetComplexProperties().Select(cp => (cp, provider.Get(cp.ComplexType)))];

        if (structuralType is IEntityType entityType)
        {
            if (entityType.IsDocumentRoot())
            {
                _jsonIdProperty = structuralType.GetProperties().FirstOrDefault(p => p.GetJsonPropertyName() == CosmosJsonIdConvention.IdPropertyJsonName)
                    ?? throw new InvalidOperationException(CosmosStrings.NoIdProperty(structuralType.DisplayName()));
            }

            _navigations = [.. entityType.GetNavigations().Where(n => n.ForeignKey.IsOwnership && !n.IsOnDependent).Select(n => (n, provider.Get(n.TargetEntityType)))];
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string GetJsonId(IUpdateEntry entry)
        => (string)entry.GetCurrentProviderValue(_jsonIdProperty ?? throw new UnreachableException("Can not get json id for non root document type"))!;

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
            WriteEntry(writer, internalEntry);
        }

        return new ReadOnlyMemory<byte>(stream.GetBuffer(), 0, (int)stream.Length);
    }

    private void WriteEntry(
        Utf8JsonWriter writer,
        IInternalEntry entry,
        int? ordinal = null)
    {
        writer.WriteStartObject();

        if (_discriminatorProperty != null)
        {
            var discriminatorValue = entry.GetCurrentValue(_discriminatorProperty);
            WriteProperty(writer, _discriminatorProperty, discriminatorValue);
        }

        if (_ordinalKeyProperty != null)
        {
            entry.SetStoreGeneratedValue(_ordinalKeyProperty, ordinal!.Value + 1, setModified: false);
        }

        foreach (var property in _scalarProperties)
        {
            var value = entry.GetCurrentValue(property);
            WriteProperty(writer, property, value);
        }

        foreach (var (complexProperty, nestedSerializer) in _complexProperties)
        {
            var value = entry.GetCurrentValue(complexProperty);
            writer.WritePropertyName(complexProperty.GetJsonPropertyName());

            if (value is null)
            {
                writer.WriteNullValue();
                continue;
            }

            if (complexProperty.IsCollection)
            {
                writer.WriteStartArray();

                var i = 0;
                foreach (var item in (IEnumerable)value)
                {
                    var nestedEntry = (IInternalEntry)entry.GetComplexCollectionEntry(complexProperty, i);
                    nestedSerializer.WriteEntry(writer, nestedEntry, i++);
                }

                writer.WriteEndArray();
            }
            else
            {
                nestedSerializer.WriteEntry(writer, entry, null);
            }
        }

        foreach (var (navigation, nestedSerializer) in _navigations)
        {
            var value = entry.GetCurrentValue(navigation);
            writer.WritePropertyName(navigation.TargetEntityType.GetContainingPropertyName()!);

            if (navigation.IsCollection)
            {
                // @TODO: Owned collections can not be null right? So we always write an array, even if the value is null
                writer.WriteStartArray();

                if (value is not null)
                {
                    // When items in an owned entity collection are reordered, assigning ordinal key values
                    // sequentially can cause identity map conflicts - e.g. assigning ordinal 2 to a new
                    // entry while another tracked entry still holds ordinal 2.
                    // To avoid this, first assign temporary negative ordinals to move all entries out of
                    // the way, then let WriteJsonObject assign the correct final ordinals.
                    if (nestedSerializer._ordinalKeyProperty != null)
                    {
                        var stateManager = ((InternalEntityEntry)entry).StateManager;
                        var tempOrdinal = -1;
                        foreach (var collectionElement in (IEnumerable)value)
                        {
                            var tempEntry = stateManager.TryGetEntry(collectionElement, navigation.TargetEntityType);
                            tempEntry?.SetTemporaryValue(nestedSerializer._ordinalKeyProperty, tempOrdinal--, setModified: false);
                        }
                    }

                    var nestedOrdinal = 0;
                    foreach (var item in (IEnumerable)value)
                    {
                        Check.DebugAssert(item != null, "Owned collections can not contain null");

                        var nestedEntry = ((InternalEntityEntry)entry).StateManager.TryGetEntry(item, navigation.TargetEntityType) ?? throw new UnreachableException("Embedded navigation not tracked.");
                        nestedSerializer.WriteEntry(writer, nestedEntry, nestedOrdinal++);
                    }
                }

                writer.WriteEndArray();
            }
            else
            {
                if (value is null)
                {
                    writer.WriteNullValue();
                    continue;
                }

                var nestedEntry = ((InternalEntityEntry)entry).StateManager.TryGetEntry(value, navigation.TargetEntityType) ?? throw new UnreachableException("Embedded navigation not tracked.");
                nestedSerializer.WriteEntry(writer, nestedEntry);
            }
        }

        writer.WriteEndObject();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ReadOnlyMemory<byte> Serialize(object? instance, bool collection = false)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
        {
            if (collection)
            {
                writer.WriteStartArray();
                foreach (var item in (IEnumerable)instance!)
                {
                    WriteInstace(writer, item);
                }
                writer.WriteEndArray();
            }
            else
            {
                WriteInstace(writer, instance);
            }
        }

        return stream.GetBuffer().AsMemory(0, (int)stream.Length);
    }

    private void WriteInstace(Utf8JsonWriter writer, object? instance)
    {
        if (instance is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        if (_discriminatorProperty != null)
        {
            var value = _discriminatorProperty.IsShadowProperty()
                            ? _structuralType.GetDerivedTypesInclusive().First(t => t.ClrType == instance.GetType()).GetDiscriminatorValue()
                            : _discriminatorProperty.GetGetter().GetClrValue(instance);
            WriteProperty(writer, _discriminatorProperty, value);
        }

        foreach (var property in _scalarProperties)
        {
            var value = property.GetGetter().GetClrValue(instance);
            WriteProperty(writer, property, value);
        }

        foreach (var (complexProperty, nestedSerializer) in _complexProperties)
        {
            var value = complexProperty.GetGetter().GetClrValue(instance);
            WriteComplexProperty(writer, complexProperty, nestedSerializer, value);
        }

        foreach (var (navigation, nestedSerializer) in _navigations)
        {
            var value = navigation.GetGetter().GetClrValue(instance);
            WriteNavigation(writer, navigation, nestedSerializer, value);
        }

        writer.WriteEndObject();
    }

    private void WriteComplexProperty(Utf8JsonWriter writer, IComplexProperty complexProperty, CosmosStructuralTypeSerializer nestedSerializer, object? value)
    {
        writer.WritePropertyName(complexProperty.GetJsonPropertyName());

        if (value is null && !complexProperty.IsNullable)
        {
            throw new InvalidOperationException(CoreStrings.PropertyConceptualNull(complexProperty.Name, _structuralType.DisplayName()));
        }

        if (complexProperty.IsCollection)
        {
            if (value is null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStartArray();
                foreach (var item in (IEnumerable)value)
                {
                    nestedSerializer.WriteInstace(writer, item);
                }
                writer.WriteEndArray();
            }
        }
        else
        {
            nestedSerializer.WriteInstace(writer, value);
        }
    }

    private void WriteNavigation(Utf8JsonWriter writer, INavigation navigation, CosmosStructuralTypeSerializer nestedSerializer, object? value)
    {
        writer.WritePropertyName(navigation.TargetEntityType.GetContainingPropertyName()!);

        if (navigation.IsCollection)
        {
            // @TODO: Owned collections can not be null right? So we always write an array, even if the value is null
            writer.WriteStartArray();

            if (value is not null)
            {
                foreach (var item in (IEnumerable)value)
                {
                    nestedSerializer.WriteInstace(writer, item);
                }
            }

            writer.WriteEndArray();
        }
        else
        {
            if (value is null && navigation.ForeignKey.IsRequired)
            {
                throw new InvalidOperationException(CoreStrings.PropertyConceptualNull(navigation.Name, _structuralType.DisplayName()));
            }

            nestedSerializer.WriteInstace(writer, value);
        }
    }

    private void WriteProperty(Utf8JsonWriter writer, IProperty property, object? value)
    {
        writer.WritePropertyName(property.GetJsonPropertyName());

        var jsonValueReaderWriter = property.GetJsonValueReaderWriter() ?? property.GetTypeMapping().JsonValueReaderWriter;
        if (value is not null || jsonValueReaderWriter?.HandlesNullWrites == true)
        {
            Check.DebugAssert(jsonValueReaderWriter is not null, $"Missing JsonValueReaderWriter for property: {property}");
            jsonValueReaderWriter.ToJson(writer, value!);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
