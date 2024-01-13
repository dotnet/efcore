// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
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
public abstract class RuntimeTypeBase : RuntimeAnnotatableBase, IRuntimeTypeBase
{
    private RuntimeModel _model;
    private readonly RuntimeTypeBase? _baseType;
    private SortedSet<RuntimeTypeBase>? _directlyDerivedTypes;
    private readonly OrderedDictionary<string, RuntimeProperty> _properties;
    private OrderedDictionary<string, RuntimeComplexProperty>? _complexProperties;
    private readonly PropertyInfo? _indexerPropertyInfo;
    private readonly bool _isPropertyBag;
    private readonly ChangeTrackingStrategy _changeTrackingStrategy;

    // Warning: Never access these fields directly as access needs to be thread-safe
    private RuntimeProperty[]? _flattenedProperties;
    private RuntimeProperty[]? _flattenedDeclaredProperties;
    private RuntimeComplexProperty[]? _flattenedComplexProperties;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected RuntimeTypeBase(
        string name,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        RuntimeModel model,
        RuntimeTypeBase? baseType,
        ChangeTrackingStrategy changeTrackingStrategy,
        PropertyInfo? indexerPropertyInfo,
        bool propertyBag,
        int derivedTypesCount,
        int propertyCount,
        int complexPropertyCount)
    {
        Name = name;
        ClrType = type;
        _model = model;
        if (baseType != null)
        {
            _baseType = baseType;
            (baseType._directlyDerivedTypes ??= new(TypeBaseNameComparer.Instance)).Add(this);
        }

        _changeTrackingStrategy = changeTrackingStrategy;
        _indexerPropertyInfo = indexerPropertyInfo;
        _isPropertyBag = propertyBag;
        _properties = new OrderedDictionary<string, RuntimeProperty>(propertyCount, new PropertyNameComparer(this));
        if (complexPropertyCount > 0)
        {
            _complexProperties = new(complexPropertyCount, StringComparer.Ordinal);
        }
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
    public virtual RuntimeTypeBase? BaseType
        => _baseType;

    /// <summary>
    ///     Gets all types in the model that directly derive from this type.
    /// </summary>
    /// <returns>The derived types.</returns>
    [EntityFrameworkInternal]
    protected virtual IEnumerable<RuntimeTypeBase> DirectlyDerivedTypes
        => _directlyDerivedTypes ?? Enumerable.Empty<RuntimeTypeBase>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual bool HasDirectlyDerivedTypes
        => _directlyDerivedTypes != null
        && _directlyDerivedTypes.Count > 0;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual IEnumerable<RuntimeTypeBase> GetDerivedTypes()
        => GetDerivedTypes<RuntimeTypeBase>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<T> GetDerivedTypes<T>()
        where T : RuntimeTypeBase
    {
        if (!HasDirectlyDerivedTypes)
        {
            return Enumerable.Empty<T>();
        }

        var derivedTypes = new List<T>();
        var type = (T)this;
        var currentTypeIndex = 0;
        while (type != null)
        {
            derivedTypes.AddRange(type.DirectlyDerivedTypes.Cast<T>());
            type = derivedTypes.Count > currentTypeIndex
                ? derivedTypes[currentTypeIndex]
                : null;
            currentTypeIndex++;
        }

        return derivedTypes;
    }

    /// <summary>
    ///     Adds a property to this entity type.
    /// </summary>
    /// <param name="name">The name of the property to add.</param>
    /// <param name="clrType">The type of value the property will hold.</param>
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
    /// <param name="sentinel">The property value to use to consider the property not set.</param>
    /// <returns>The newly created property.</returns>
    public virtual RuntimeProperty AddProperty(
        string name,
        Type clrType,
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
        CoreTypeMapping? typeMapping = null,
        object? sentinel = null)
    {
        var property = new RuntimeProperty(
            name,
            clrType,
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
            typeMapping,
            sentinel);

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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual RuntimeProperty? FindDeclaredProperty(string name)
        => _properties.TryGetValue(name, out var property)
            ? property
            : null;

    /// <summary>
    ///     Gets all scalar properties declared on this type.
    /// </summary>
    /// <remarks>
    ///     This method does not return properties declared on base types.
    ///     It is useful when iterating over all types to avoid processing the same property more than once.
    ///     Use <see cref="GetProperties" /> to also return properties declared on base types.
    /// </remarks>
    /// <returns>Declared scalar properties.</returns>
    public virtual IEnumerable<RuntimeProperty> GetDeclaredProperties()
        => _properties.Values;

    private IEnumerable<RuntimeProperty> GetDerivedProperties()
        => !HasDirectlyDerivedTypes
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
    ///     Gets the properties with the given name on this type, base types or derived types.
    /// </summary>
    /// <returns>Type properties.</returns>
    public virtual IEnumerable<RuntimeProperty> FindPropertiesInHierarchy(string propertyName)
        => !HasDirectlyDerivedTypes
            ? ToEnumerable(FindProperty(propertyName))
            : ToEnumerable(FindProperty(propertyName)).Concat(FindDerivedProperties(propertyName));

    private IEnumerable<RuntimeProperty> FindDerivedProperties(string propertyName)
    {
        Check.NotNull(propertyName, nameof(propertyName));

        return !HasDirectlyDerivedTypes
            ? Enumerable.Empty<RuntimeProperty>()
            : (IEnumerable<RuntimeProperty>)GetDerivedTypes()
                .Select(et => et.FindDeclaredProperty(propertyName)).Where(p => p != null);
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
    /// <param name="propertyCount">The expected number of declared properties for this complex type.</param>
    /// <param name="complexPropertyCount">The expected number of declared complex properties for this complex type.</param>
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
        bool propertyBag = false,
        int propertyCount = 0,
        int complexPropertyCount = 0)
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
            propertyBag,
            propertyCount: propertyCount,
            complexPropertyCount: complexPropertyCount);

        _complexProperties ??= new(StringComparer.Ordinal);
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
        => _complexProperties != null
            && _complexProperties.TryGetValue(name, out var property)
            ? property
            : null;

    /// <summary>
    ///     Gets the complex properties declared on this type.
    /// </summary>
    /// <returns>Declared complex properties.</returns>
    public virtual IEnumerable<RuntimeComplexProperty> GetDeclaredComplexProperties()
        => _complexProperties?.Values ?? Enumerable.Empty<RuntimeComplexProperty>();

    private IEnumerable<RuntimeComplexProperty> GetDerivedComplexProperties()
        => !HasDirectlyDerivedTypes
            ? Enumerable.Empty<RuntimeComplexProperty>()
            : GetDerivedTypes().Cast<RuntimeEntityType>().SelectMany(et => et.GetDeclaredComplexProperties());

    /// <summary>
    ///     Gets the complex properties defined on this type.
    /// </summary>
    /// <remarks>
    ///     This API only returns complex properties and does not find navigation, scalar or service properties.
    /// </remarks>
    /// <returns>The complex properties defined on this type.</returns>
    public virtual IEnumerable<RuntimeComplexProperty> GetComplexProperties()
        => BaseType != null
            ? _complexProperties != null
                ? BaseType.GetComplexProperties().Concat(_complexProperties.Values)
                : BaseType.GetComplexProperties()
            : GetDeclaredComplexProperties();

    /// <summary>
    ///     Gets the complex properties with the given name on this type, base types or derived types.
    /// </summary>
    /// <returns>Type complex properties.</returns>
    public virtual IEnumerable<RuntimeComplexProperty> FindComplexPropertiesInHierarchy(string propertyName)
        => !HasDirectlyDerivedTypes
            ? ToEnumerable(FindComplexProperty(propertyName))
            : ToEnumerable(FindComplexProperty(propertyName)).Concat(FindDerivedComplexProperties(propertyName));

    private IEnumerable<RuntimeComplexProperty> FindDerivedComplexProperties(string propertyName)
    {
        Check.NotNull(propertyName, nameof(propertyName));

        return !HasDirectlyDerivedTypes
            ? Enumerable.Empty<RuntimeComplexProperty>()
            : (IEnumerable<RuntimeComplexProperty>)GetDerivedTypes()
                .Select(et => et.FindDeclaredComplexProperty(propertyName)).Where(p => p != null);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract IEnumerable<RuntimePropertyBase> GetMembers();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract IEnumerable<RuntimePropertyBase> GetDeclaredMembers();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract RuntimePropertyBase? FindMember(string name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract IEnumerable<RuntimePropertyBase> FindMembersInHierarchy(string name);

    /// <summary>
    ///     Gets or sets the <see cref="InstantiationBinding" /> for the preferred constructor.
    /// </summary>
    public abstract InstantiationBinding? ConstructorBinding { get; set; }

    /// <summary>
    ///     Returns all <see cref="IProperty" /> members from this type and all nested complex types, if any.
    /// </summary>
    /// <returns>The properties.</returns>
    public virtual IEnumerable<RuntimeProperty> GetFlattenedProperties()
    {
        return NonCapturingLazyInitializer.EnsureInitialized(
            ref _flattenedProperties, this,
            static type => Create(type).ToArray());

        static IEnumerable<RuntimeProperty> Create(RuntimeTypeBase type)
        {
            foreach (var property in type.GetProperties())
            {
                yield return property;
            }

            foreach (var complexProperty in type.GetComplexProperties())
            {
                foreach (var property in complexProperty.ComplexType.GetFlattenedProperties())
                {
                    yield return property;
                }
            }
        }
    }

    /// <summary>
    ///     Returns all <see cref="RuntimeComplexProperty" /> members from this type and all nested complex types, if any.
    /// </summary>
    /// <returns>The properties.</returns>
    public virtual IEnumerable<RuntimeComplexProperty> GetFlattenedComplexProperties()
    {
        return NonCapturingLazyInitializer.EnsureInitialized(
            ref _flattenedComplexProperties, this,
            static type => Create(type).ToArray());

        static IEnumerable<RuntimeComplexProperty> Create(RuntimeTypeBase type)
        {
            foreach (var complexProperty in type.GetComplexProperties())
            {
                yield return complexProperty;

                foreach (var nestedComplexProperty in complexProperty.ComplexType.GetFlattenedComplexProperties())
                {
                    yield return nestedComplexProperty;
                }
            }
        }
    }

    /// <summary>
    ///     Returns all <see cref="IProperty" /> members from this type and all nested complex types, if any.
    /// </summary>
    /// <returns>The properties.</returns>
    public virtual IEnumerable<RuntimeProperty> GetFlattenedDeclaredProperties()
    {
        return NonCapturingLazyInitializer.EnsureInitialized(
            ref _flattenedDeclaredProperties, this,
            static type => Create(type).ToArray());

        static IEnumerable<RuntimeProperty> Create(RuntimeTypeBase type)
        {
            foreach (var property in type.GetDeclaredProperties())
            {
                yield return property;
            }

            foreach (var complexProperty in type.GetDeclaredComplexProperties())
            {
                foreach (var property in complexProperty.ComplexType.GetFlattenedDeclaredProperties())
                {
                    yield return property;
                }
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract IEnumerable<RuntimePropertyBase> GetSnapshottableMembers();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void FinalizeType()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected static IEnumerable<T> ToEnumerable<T>(T? element)
        where T : class
        => element == null
            ? Enumerable.Empty<T>()
            : new[] { element };

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

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IPropertyBase> ITypeBase.GetMembers()
        => GetMembers();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyPropertyBase> IReadOnlyTypeBase.GetMembers()
        => GetMembers();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyPropertyBase> IReadOnlyTypeBase.GetDeclaredMembers()
        => GetDeclaredMembers();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IPropertyBase> ITypeBase.GetDeclaredMembers()
        => GetDeclaredMembers();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyPropertyBase? IReadOnlyTypeBase.FindMember(string name)
        => FindMember(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IPropertyBase? ITypeBase.FindMember(string name)
        => FindMember(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyPropertyBase> IReadOnlyTypeBase.FindMembersInHierarchy(string name)
        => FindMembersInHierarchy(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IPropertyBase> ITypeBase.FindMembersInHierarchy(string name)
        => FindMembersInHierarchy(name);

    /// <summary>
    ///     Returns all members that may need a snapshot value when change tracking.
    /// </summary>
    /// <returns>The members.</returns>
    IEnumerable<IPropertyBase> ITypeBase.GetSnapshottableMembers()
        => GetSnapshottableMembers();

    /// <summary>
    ///     Returns all properties that implement <see cref="IProperty" />, including those on complex types.
    /// </summary>
    /// <returns>The properties.</returns>
    IEnumerable<IProperty> ITypeBase.GetFlattenedProperties()
        => GetFlattenedProperties();

    /// <summary>
    ///     Returns all properties that implement <see cref="IComplexProperty" />, including those on complex types.
    /// </summary>
    /// <returns>The properties.</returns>
    IEnumerable<IComplexProperty> ITypeBase.GetFlattenedComplexProperties()
        => GetFlattenedComplexProperties();

    /// <summary>
    ///     Returns all properties declared properties that implement <see cref="IProperty" />, including those on complex types.
    /// </summary>
    /// <returns>The properties.</returns>
    IEnumerable<IProperty> ITypeBase.GetFlattenedDeclaredProperties()
        => GetFlattenedDeclaredProperties();
}
