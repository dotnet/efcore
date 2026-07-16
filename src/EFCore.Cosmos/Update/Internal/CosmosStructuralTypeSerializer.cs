// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;

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
    private readonly string? _container;

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
                _container = entityType.GetContainer() ?? throw new UnreachableException("Document root entity type does not have container.");
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
    public virtual string Container
        => _container ?? throw new UnreachableException("Can not get json container for non root document type");

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
        using (var writer = new Utf8JsonWriter(stream, CosmosClientWrapper.JsonWriterOptions))
        {
            var context = new EntrySerializationContext(internalEntry);
            WriteStructuralType(writer, context);
        }

        return new ReadOnlyMemory<byte>(stream.GetBuffer(), 0, (int)stream.Length);
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
        using (var writer = new Utf8JsonWriter(stream, CosmosClientWrapper.JsonWriterOptions))
        {
            if (collection)
            {
                writer.WriteStartArray();
                foreach (var item in (IEnumerable)instance!)
                {
                    var context = new InstanceSerializationContext(item);
                    WriteStructuralType(writer, context);
                }
                writer.WriteEndArray();
            }
            else
            {
                var context = new InstanceSerializationContext(instance);
                WriteStructuralType(writer, context);
            }
        }

        return stream.GetBuffer().AsMemory(0, (int)stream.Length);
    }


    private void WriteStructuralType<TContext>(
        Utf8JsonWriter writer,
        TContext context,
        int? ordinal = null)
        where TContext : struct, ISerializationContext<TContext>
    {
        if (context.IsNull)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        if (_discriminatorProperty != null)
        {
            var discriminatorValue = context.GetDiscriminatorValue(_discriminatorProperty, _structuralType);
            WriteProperty(context, writer, _discriminatorProperty, discriminatorValue);
        }

        if (_ordinalKeyProperty != null)

        {
            context.SetOrdinal(_ordinalKeyProperty, ordinal);
        }

        foreach (var property in _scalarProperties)
        {
            var value = context.GetValue(property);
            WriteProperty(context, writer, property, value);
        }

        foreach (var (complexProperty, nestedSerializer) in _complexProperties)
        {
            var value = context.GetValue(complexProperty);
            writer.WritePropertyName(complexProperty.GetJsonPropertyName());

            if (value is null)
            {
                context.ValidateNull(complexProperty, _structuralType);
                writer.WriteNullValue();
                continue;
            }

            if (complexProperty.IsCollection)
            {
                writer.WriteStartArray();

                var i = 0;
                foreach (var item in (IEnumerable)value)
                {
                    var nestedContext = context.CreateNestedContext(complexProperty, item, i);
                    nestedSerializer.WriteStructuralType(writer, nestedContext, i++);
                }

                writer.WriteEndArray();
            }
            else
            {
                var nestedContext = context.CreateNestedContext(complexProperty, value, null);
                nestedSerializer.WriteStructuralType(writer, nestedContext);
            }
        }

        foreach (var (navigation, nestedSerializer) in _navigations)
        {
            var value = context.GetValue(navigation);
            writer.WritePropertyName(navigation.TargetEntityType.GetContainingPropertyName()!);

            if (navigation.IsCollection)
            {
                // @TODO: Owned collections can not be null right? So we always write an array, even if the value is null
                writer.WriteStartArray();

                if (value is not null)
                {
                    context.PrepareNavigationCollection(navigation, nestedSerializer, value);

                    var nestedOrdinal = 0;
                    foreach (var item in (IEnumerable)value)
                    {
                        var nestedContext = context.CreateNestedContext(navigation, item);
                        nestedSerializer.WriteStructuralType(writer, nestedContext, nestedOrdinal++);
                    }
                }

                writer.WriteEndArray();
            }
            else
            {
                if (value is null)
                {
                    context.ValidateNull(navigation, _structuralType);
                    writer.WriteNullValue();
                    continue;
                }

                var nestedContext = context.CreateNestedContext(navigation, value);
                nestedSerializer.WriteStructuralType(writer, nestedContext);
            }
        }

        writer.WriteEndObject();
    }

    private interface ISerializationContext<TContext>
        where TContext : struct, ISerializationContext<TContext>
    {
        public bool IsNull { get; }
        public object? GetValue(IPropertyBase property);
        public object? GetDiscriminatorValue(IProperty discriminatorProperty, ITypeBase structuralType);
        public void SetOrdinal(IProperty ordinalKeyProperty, int? ordinal);
        public void ValidateNull(IProperty property, ITypeBase structuralType);
        public void ValidateNull(IComplexProperty complexProperty, ITypeBase structuralType);
        public void ValidateNull(INavigation navigation, ITypeBase structuralType);
        public void PrepareNavigationCollection(
            INavigation navigation,
            CosmosStructuralTypeSerializer nestedSerializer,
            object value);
        public TContext CreateNestedContext(IComplexProperty complexProperty, object? value, int? ordinal);
        public TContext CreateNestedContext(INavigation navigation, object? value);
    }

    private readonly struct EntrySerializationContext(IInternalEntry entry) : ISerializationContext<EntrySerializationContext>
    {
        public bool IsNull
            => false;

        public object? GetValue(IPropertyBase property)
            => entry.GetCurrentValue(property);

        public object? GetDiscriminatorValue(IProperty discriminatorProperty, ITypeBase structuralType)
            => entry.GetCurrentValue(discriminatorProperty);

        public void SetOrdinal(IProperty ordinalKeyProperty, int? ordinal)
            => entry.SetStoreGeneratedValue(ordinalKeyProperty, ordinal!.Value + 1, setModified: false);

        public void ValidateNull(IProperty property, ITypeBase structuralType)
        {
            // Change tracker has done this
        }

        public void ValidateNull(IComplexProperty complexProperty, ITypeBase structuralType)
        {
            // Change tracker has done this
        }

        public void ValidateNull(INavigation navigation, ITypeBase structuralType)
        {
            // Change tracker has done this
        }

        public void PrepareNavigationCollection(
            INavigation navigation,
            CosmosStructuralTypeSerializer nestedSerializer,
            object value)
        {
            // When items in an owned entity collection are reordered, assigning ordinal key values
            // sequentially can cause identity map conflicts - e.g. assigning ordinal 2 to a new
            // entry while another tracked entry still holds ordinal 2.
            // To avoid this, first assign temporary negative ordinals to move all entries out of
            // the way before assigning the correct final ordinals while serializing.
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
        }

        public EntrySerializationContext CreateNestedContext(
            IComplexProperty complexProperty,
            object? value,
            int? ordinal)
            => ordinal == null
                ? this
                : new(entry.GetComplexCollectionEntry(complexProperty, ordinal.Value));

        public EntrySerializationContext CreateNestedContext(INavigation navigation, object? value)
        {
            Check.DebugAssert(value != null, "Owned collections can not contain null");

            var nestedEntry = ((InternalEntityEntry)entry).StateManager.TryGetEntry(value, navigation.TargetEntityType)
                ?? throw new UnreachableException("Embedded navigation not tracked.");
            return new(nestedEntry);
        }
    }

    private readonly struct InstanceSerializationContext(object? instance) : ISerializationContext<InstanceSerializationContext>
    {
        public bool IsNull
            => instance is null;

        public object? GetValue(IPropertyBase property)
            => property.GetGetter().GetClrValue(instance!);

        public object? GetDiscriminatorValue(IProperty discriminatorProperty, ITypeBase structuralType)
        {
            if (!discriminatorProperty.IsShadowProperty())
            {
                return discriminatorProperty.GetGetter().GetClrValue(instance!);
            }

            var instanceType = instance!.GetType();
            return structuralType.GetDerivedTypesInclusive().First(t => t.ClrType == instanceType).GetDiscriminatorValue();
        }

        public void SetOrdinal(IProperty ordinalKeyProperty, int? ordinal)
        {
        }

        public void ValidateNull(IProperty property, ITypeBase structuralType)
        {
            if (!property.IsNullable)
            {
                throw new InvalidOperationException(CoreStrings.PropertyConceptualNull(property.Name, structuralType.DisplayName()));
            }
        }

        public void ValidateNull(IComplexProperty complexProperty, ITypeBase structuralType)
        {
            if (!complexProperty.IsNullable)
            {
                throw new InvalidOperationException(CoreStrings.PropertyConceptualNull(complexProperty.Name, structuralType.DisplayName()));
            }
        }

        public void ValidateNull(INavigation navigation, ITypeBase structuralType)
        {
            if (navigation.ForeignKey.IsRequired)
            {
                throw new InvalidOperationException(CoreStrings.PropertyConceptualNull(navigation.Name, structuralType.DisplayName()));
            }
        }

        public void PrepareNavigationCollection(
            INavigation navigation,
            CosmosStructuralTypeSerializer nestedSerializer,
            object value)
        {
        }

        public InstanceSerializationContext CreateNestedContext(
            IComplexProperty complexProperty,
            object? value,
            int? ordinal)
            => new(value);

        public InstanceSerializationContext CreateNestedContext(INavigation navigation, object? value)
            => new(value);
    }

    private void WriteProperty<TContext>(TContext context, Utf8JsonWriter writer, IProperty property, object? value)
        where TContext : struct, ISerializationContext<TContext>
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
            context.ValidateNull(property, _structuralType);
            writer.WriteNullValue();
        }
    }
}
