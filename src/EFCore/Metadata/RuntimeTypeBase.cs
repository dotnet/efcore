// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a structural type in a model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public abstract class RuntimeTypeBase : AnnotatableBase, IRuntimeTypeBase
{
    private RuntimeModel _model;
    private readonly RuntimeTypeBase? _baseType;
    private readonly SortedSet<RuntimeTypeBase> _directlyDerivedTypes = new(TypeBaseNameComparer.Instance);
    private readonly SortedDictionary<string, RuntimeProperty> _properties;

    private readonly SortedDictionary<string, RuntimeComplexProperty> _complexProperties =
        new SortedDictionary<string, RuntimeComplexProperty>(StringComparer.Ordinal);

    private readonly PropertyInfo? _indexerPropertyInfo;
    private readonly bool _isPropertyBag;
    private readonly ChangeTrackingStrategy _changeTrackingStrategy;

    // Warning: Never access these fields directly as access needs to be thread-safe
    private Func<InternalEntityEntry, ISnapshot>? _originalValuesFactory;
    private Func<InternalEntityEntry, ISnapshot>? _temporaryValuesFactory;
    private Func<ISnapshot>? _storeGeneratedValuesFactory;
    private Func<ValueBuffer, ISnapshot>? _shadowValuesFactory;
    private Func<ISnapshot>? _emptyShadowValuesFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public RuntimeTypeBase(
        string name,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        RuntimeModel model,
        RuntimeTypeBase? baseType,
        ChangeTrackingStrategy changeTrackingStrategy,
        PropertyInfo? indexerPropertyInfo,
        bool propertyBag)
    {
        Name = name;
        ClrType = type;
        _model = model;
        if (baseType != null)
        {
            _baseType = baseType;
            baseType._directlyDerivedTypes.Add(this);
        }
        _changeTrackingStrategy = changeTrackingStrategy;
        _indexerPropertyInfo = indexerPropertyInfo;
        _isPropertyBag = propertyBag;
        _properties = new SortedDictionary<string, RuntimeProperty>(new PropertyNameComparer(this));
    }

    /// <summary>
    ///     Gets the name of this type.
    /// </summary>
    public virtual string Name { [DebuggerStepThrough] get; }

    /// <inheritdoc />
    [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)]
    public virtual Type ClrType { get; }

    /// <summary>
    ///     Gets the model that this type belongs to.
    /// </summary>
    public virtual RuntimeModel Model { get => _model; set => _model = value; }

    /// <summary>
    ///     Gets the base type of this type. Returns <see langword="null" /> if this is not a
    ///     derived type in an inheritance hierarchy.
    /// </summary>
    public virtual RuntimeTypeBase? BaseType => _baseType;

    /// <summary>
    ///     Gets all types in the model that directly derive from this type.
    /// </summary>
    /// <returns>The derived types.</returns>
    public virtual SortedSet<RuntimeTypeBase> DirectlyDerivedTypes => _directlyDerivedTypes;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual IEnumerable<RuntimeTypeBase> GetDerivedTypes()
    {
        if (DirectlyDerivedTypes.Count == 0)
        {
            return Enumerable.Empty<RuntimeTypeBase>();
        }

        var derivedTypes = new List<RuntimeTypeBase>();
        var type = this;
        var currentTypeIndex = 0;
        while (type != null)
        {
            derivedTypes.AddRange(type.DirectlyDerivedTypes);
            type = derivedTypes.Count > currentTypeIndex
                ? derivedTypes[currentTypeIndex]
                : null;
            currentTypeIndex++;
        }

        return derivedTypes;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected abstract PropertyCounts Counts { get; }

    /// <summary>
    ///     Adds a property to this entity type.
    /// </summary>
    /// <param name="name">The name of the property to add.</param>
    /// <param name="clrType">The type of value the property will hold.</param>
    /// <param name="sentinel">The property value to use to consider the property not set.</param>
    /// <param name="propertyInfo">The corresponding CLR property or <see langword="null" /> for a shadow property.</param>
    /// <param name="fieldInfo">The corresponding CLR field or <see langword="null" /> for a shadow property.</param>
    /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" /> used for this property.</param>
    /// <param name="nullable">A value indicating whether this property can contain <see langword="null" />.</param>
    /// <param name="concurrencyToken">A value indicating whether this property is used as a concurrency token.</param>
    /// <param name="valueGenerated">A value indicating when a value for this property will be generated by the database.</param>
    /// <param name="beforeSaveBehavior">
    ///     A value indicating whether or not this property can be modified before the entity is saved to the database.
    /// </param>
    /// <param name="afterSaveBehavior">
    ///     A value indicating whether or not this property can be modified after the entity is saved to the database.
    /// </param>
    /// <param name="maxLength">The maximum length of data that is allowed in this property.</param>
    /// <param name="unicode">A value indicating whether or not the property can persist Unicode characters.</param>
    /// <param name="precision">The precision of data that is allowed in this property.</param>
    /// <param name="scale">The scale of data that is allowed in this property.</param>
    /// <param name="providerPropertyType">
    ///     The type that the property value will be converted to before being sent to the database provider.
    /// </param>
    /// <param name="valueGeneratorFactory">The factory that has been set to generate values for this property, if any.</param>
    /// <param name="valueConverter">The custom <see cref="ValueConverter" /> set for this property.</param>
    /// <param name="valueComparer">The <see cref="ValueComparer" /> for this property.</param>
    /// <param name="keyValueComparer">The <see cref="ValueComparer" /> to use with keys for this property.</param>
    /// <param name="providerValueComparer">The <see cref="ValueComparer" /> to use for the provider values for this property.</param>
    /// <param name="jsonValueReaderWriter">The <see cref="JsonValueReaderWriter" /> for this property.</param>
    /// <param name="typeMapping">The <see cref="CoreTypeMapping" /> for this property.</param>
    /// <returns>The newly created property.</returns>
    public virtual RuntimeProperty AddProperty(
        string name,
        Type clrType,
        object? sentinel = null,
        PropertyInfo? propertyInfo = null,
        FieldInfo? fieldInfo = null,
        PropertyAccessMode propertyAccessMode = Internal.Model.DefaultPropertyAccessMode,
        bool nullable = false,
        bool concurrencyToken = false,
        ValueGenerated valueGenerated = ValueGenerated.Never,
        PropertySaveBehavior beforeSaveBehavior = PropertySaveBehavior.Save,
        PropertySaveBehavior afterSaveBehavior = PropertySaveBehavior.Save,
        int? maxLength = null,
        bool? unicode = null,
        int? precision = null,
        int? scale = null,
        Type? providerPropertyType = null,
        Func<IProperty, ITypeBase, ValueGenerator>? valueGeneratorFactory = null,
        ValueConverter? valueConverter = null,
        ValueComparer? valueComparer = null,
        ValueComparer? keyValueComparer = null,
        ValueComparer? providerValueComparer = null,
        JsonValueReaderWriter? jsonValueReaderWriter = null,
        CoreTypeMapping? typeMapping = null)
    {
        var property = new RuntimeProperty(
            name,
            clrType,
            sentinel,
            propertyInfo,
            fieldInfo,
            this,
            propertyAccessMode,
            nullable,
            concurrencyToken,
            valueGenerated,
            beforeSaveBehavior,
            afterSaveBehavior,
            maxLength,
            unicode,
            precision,
            scale,
            providerPropertyType,
            valueGeneratorFactory,
            valueConverter,
            valueComparer,
            keyValueComparer,
            providerValueComparer,
            jsonValueReaderWriter,
            typeMapping);

        _properties.Add(property.Name, property);

        return property;
    }

    /// <summary>
    ///     Gets the property with a given name. Returns <see langword="null" /> if no property with the given name is defined.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation properties.
    /// </remarks>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    public virtual RuntimeProperty? FindProperty(string name)
        => FindDeclaredProperty(name) ?? _baseType?.FindProperty(name);

    private RuntimeProperty? FindDeclaredProperty(string name)
        => _properties.TryGetValue(name, out var property)
            ? property
            : null;

    private IEnumerable<RuntimeProperty> GetDeclaredProperties()
        => _properties.Values;

    private IEnumerable<RuntimeProperty> GetDerivedProperties()
        => _directlyDerivedTypes.Count == 0
            ? Enumerable.Empty<RuntimeProperty>()
            : GetDerivedTypes().SelectMany(et => et.GetDeclaredProperties());

    /// <summary>
    ///     Finds matching properties on the given entity type. Returns <see langword="null" /> if any property is not found.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigations or service properties.
    /// </remarks>
    /// <param name="propertyNames">The property names.</param>
    /// <returns>The properties, or <see langword="null" /> if any property is not found.</returns>
    public virtual IReadOnlyList<RuntimeProperty>? FindProperties(IEnumerable<string> propertyNames)
    {
        var properties = new List<RuntimeProperty>();
        foreach (var propertyName in propertyNames)
        {
            var property = FindProperty(propertyName);
            if (property == null)
            {
                return null;
            }

            properties.Add(property);
        }

        return properties;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual IEnumerable<RuntimeProperty> GetProperties()
        => _baseType != null
            ? _baseType.GetProperties().Concat(_properties.Values)
            : _properties.Values;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual SortedDictionary<string, RuntimeProperty> Properties => _properties;

    /// <inheritdoc />
    [DebuggerStepThrough]
    public virtual PropertyInfo? FindIndexerPropertyInfo()
        => _indexerPropertyInfo;

    /// <summary>
    ///     Returns the default indexer property that takes a <see cref="string" /> value if one exists.
    /// </summary>
    /// <param name="type">The type to look for the indexer on.</param>
    /// <returns>An indexer property or <see langword="null" />.</returns>
    public static PropertyInfo? FindIndexerProperty(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)]
        Type type)
        => type.FindIndexerProperty();

    /// <summary>
    ///     Adds a complex property to this entity type.
    /// </summary>
    /// <param name="name">The name of the property to add.</param>
    /// <param name="clrType">The type of value the property will hold.</param>
    /// <param name="targetTypeName">The name of the complex type to be added.</param>
    /// <param name="targetType">The CLR type that is used to represent instances of this complex type.</param>
    /// <param name="propertyInfo">The corresponding CLR property or <see langword="null" /> for a shadow property.</param>
    /// <param name="fieldInfo">The corresponding CLR field or <see langword="null" /> for a shadow property.</param>
    /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" /> used for this property.</param>
    /// <param name="nullable">A value indicating whether this property can contain <see langword="null" />.</param>
    /// <param name="collection">Indicates whether the property represents a collection.</param>
    /// <param name="changeTrackingStrategy">The change tracking strategy for this complex type.</param>
    /// <param name="indexerPropertyInfo">The <see cref="PropertyInfo" /> for the indexer on the associated CLR type if one exists.</param>
    /// <param name="propertyBag">
    ///     A value indicating whether this entity type has an indexer which is able to contain arbitrary properties
    ///     and a method that can be used to determine whether a given indexer property contains a value.
    /// </param>
    /// <returns>The newly created property.</returns>
    public virtual RuntimeComplexProperty AddComplexProperty(
        string name,
        Type clrType,
        string targetTypeName,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type targetType,
        PropertyInfo? propertyInfo = null,
        FieldInfo? fieldInfo = null,
        PropertyAccessMode propertyAccessMode = Internal.Model.DefaultPropertyAccessMode,
        bool nullable = false,
        bool collection = false,
        ChangeTrackingStrategy changeTrackingStrategy = ChangeTrackingStrategy.Snapshot,
        PropertyInfo? indexerPropertyInfo = null,
        bool propertyBag = false)
    {
        var property = new RuntimeComplexProperty(
            name,
            clrType,
            targetTypeName,
            targetType,
            propertyInfo,
            fieldInfo,
            this,
            propertyAccessMode,
            nullable,
            collection,
            changeTrackingStrategy,
            indexerPropertyInfo,
            propertyBag);

        _complexProperties.Add(property.Name, property);

        return property;
    }

    /// <summary>
    ///     Gets the complex property with a given name. Returns <see langword="null" /> if no property with the given name is defined.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property, or <see langword="null" /> if none is found.</returns>
    public virtual RuntimeComplexProperty? FindComplexProperty(string name)
        => FindDeclaredComplexProperty(name) ?? BaseType?.FindComplexProperty(name);

    private RuntimeComplexProperty? FindDeclaredComplexProperty(string name)
        => _complexProperties.TryGetValue(name, out var property)
            ? property
            : null;

    private IEnumerable<RuntimeComplexProperty> GetDeclaredComplexProperties()
        => _complexProperties.Values;

    private IEnumerable<RuntimeComplexProperty> GetDerivedComplexProperties()
        => DirectlyDerivedTypes.Count == 0
            ? Enumerable.Empty<RuntimeComplexProperty>()
            : GetDerivedTypes().Cast<RuntimeEntityType>().SelectMany(et => et.GetDeclaredComplexProperties());

    private IEnumerable<RuntimeComplexProperty> GetComplexProperties()
        => BaseType != null
            ? BaseType.GetComplexProperties().Concat(_complexProperties.Values)
            : _complexProperties.Values;

    /// <inheritdoc />
    bool IReadOnlyTypeBase.HasSharedClrType
    {
        [DebuggerStepThrough]
        get => true;
    }

    /// <inheritdoc />
    bool IReadOnlyTypeBase.IsPropertyBag
    {
        [DebuggerStepThrough]
        get => _isPropertyBag;
    }

    /// <inheritdoc />
    IReadOnlyModel IReadOnlyTypeBase.Model
    {
        [DebuggerStepThrough]
        get => Model;
    }

    /// <inheritdoc />
    IModel ITypeBase.Model
    {
        [DebuggerStepThrough]
        get => Model;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyProperty? IReadOnlyTypeBase.FindDeclaredProperty(string name)
        => FindDeclaredProperty(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IProperty? ITypeBase.FindDeclaredProperty(string name)
        => FindDeclaredProperty(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyList<IReadOnlyProperty>? IReadOnlyTypeBase.FindProperties(IReadOnlyList<string> propertyNames)
        => FindProperties(propertyNames);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyProperty? IReadOnlyTypeBase.FindProperty(string name)
        => FindProperty(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IProperty? ITypeBase.FindProperty(string name)
        => FindProperty(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyProperty> IReadOnlyTypeBase.GetDeclaredProperties()
        => GetDeclaredProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IProperty> ITypeBase.GetDeclaredProperties()
        => GetDeclaredProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyProperty> IReadOnlyTypeBase.GetDerivedProperties()
        => GetDerivedProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyProperty> IReadOnlyTypeBase.GetProperties()
        => GetProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IProperty> ITypeBase.GetProperties()
        => GetProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IComplexProperty> ITypeBase.GetComplexProperties()
        => GetComplexProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyComplexProperty> IReadOnlyTypeBase.GetComplexProperties()
        => GetComplexProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IComplexProperty> ITypeBase.GetDeclaredComplexProperties()
        => GetDeclaredComplexProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyComplexProperty> IReadOnlyTypeBase.GetDeclaredComplexProperties()
        => GetDeclaredComplexProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyComplexProperty> IReadOnlyTypeBase.GetDerivedComplexProperties()
        => GetDerivedComplexProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IComplexProperty? ITypeBase.FindComplexProperty(string name)
        => FindComplexProperty(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyComplexProperty? IReadOnlyTypeBase.FindComplexProperty(string name)
        => FindComplexProperty(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyComplexProperty? IReadOnlyTypeBase.FindDeclaredComplexProperty(string name)
        => FindDeclaredComplexProperty(name);

    /// <inheritdoc />
    PropertyCounts IRuntimeTypeBase.Counts
    {
        [DebuggerStepThrough]
        get => Counts;
    }

    /// <inheritdoc />
    Func<InternalEntityEntry, ISnapshot> IRuntimeTypeBase.OriginalValuesFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _originalValuesFactory, this,
            static complexType => new OriginalValuesFactoryFactory().Create(complexType));

    /// <inheritdoc />
    Func<ISnapshot> IRuntimeTypeBase.StoreGeneratedValuesFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _storeGeneratedValuesFactory, this,
            static complexType => new StoreGeneratedValuesFactoryFactory().CreateEmpty(complexType));

    /// <inheritdoc />
    Func<InternalEntityEntry, ISnapshot> IRuntimeTypeBase.TemporaryValuesFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _temporaryValuesFactory, this,
            static complexType => new TemporaryValuesFactoryFactory().Create(complexType));

    /// <inheritdoc />
    Func<ValueBuffer, ISnapshot> IRuntimeTypeBase.ShadowValuesFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _shadowValuesFactory, this,
            static complexType => new ShadowValuesFactoryFactory().Create(complexType));

    /// <inheritdoc />
    Func<ISnapshot> IRuntimeTypeBase.EmptyShadowValuesFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _emptyShadowValuesFactory, this,
            static complexType => new EmptyShadowValuesFactoryFactory().CreateEmpty(complexType));

    /// <inheritdoc />
    [DebuggerStepThrough]
    ChangeTrackingStrategy IReadOnlyTypeBase.GetChangeTrackingStrategy()
        => _changeTrackingStrategy;

    /// <inheritdoc />
    PropertyAccessMode IReadOnlyTypeBase.GetPropertyAccessMode()
        => throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);

    /// <inheritdoc />
    ConfigurationSource? IRuntimeTypeBase.GetConstructorBindingConfigurationSource()
        => throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);

    /// <inheritdoc />
    ConfigurationSource? IRuntimeTypeBase.GetServiceOnlyConstructorBindingConfigurationSource()
        => throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
}
