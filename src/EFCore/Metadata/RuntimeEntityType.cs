// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents an entity type in a model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class RuntimeEntityType : AnnotatableBase, IRuntimeEntityType
{
    private readonly List<RuntimeForeignKey> _foreignKeys = new();

    private readonly SortedDictionary<string, RuntimeNavigation> _navigations
        = new(StringComparer.Ordinal);

    private readonly SortedDictionary<string, RuntimeSkipNavigation> _skipNavigations
        = new(StringComparer.Ordinal);

    private readonly SortedDictionary<string, RuntimeServiceProperty> _serviceProperties
        = new(StringComparer.Ordinal);

    private readonly SortedDictionary<string, RuntimeProperty> _properties;

    private readonly SortedDictionary<IReadOnlyList<IReadOnlyProperty>, RuntimeIndex> _unnamedIndexes
        = new(PropertyListComparer.Instance);

    private readonly SortedDictionary<string, RuntimeIndex> _namedIndexes
        = new(StringComparer.Ordinal);

    private readonly SortedDictionary<IReadOnlyList<IReadOnlyProperty>, RuntimeKey> _keys
        = new(PropertyListComparer.Instance);

    private readonly SortedDictionary<string, RuntimeTrigger> _triggers
        = new(StringComparer.Ordinal);

    private RuntimeKey? _primaryKey;
    private readonly bool _hasSharedClrType;

    [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)]
    private readonly Type _clrType;

    private readonly RuntimeEntityType? _baseType;
    private readonly SortedSet<RuntimeEntityType> _directlyDerivedTypes = new(EntityTypeFullNameComparer.Instance);
    private readonly ChangeTrackingStrategy _changeTrackingStrategy;
    private InstantiationBinding? _constructorBinding;
    private InstantiationBinding? _serviceOnlyConstructorBinding;
    private readonly PropertyInfo? _indexerPropertyInfo;
    private readonly bool _isPropertyBag;
    private readonly object? _discriminatorValue;
    private bool _hasServiceProperties;

    // Warning: Never access these fields directly as access needs to be thread-safe
    private PropertyCounts? _counts;

    private Func<InternalEntityEntry, ISnapshot>? _relationshipSnapshotFactory;
    private Func<InternalEntityEntry, ISnapshot>? _originalValuesFactory;
    private Func<InternalEntityEntry, ISnapshot>? _temporaryValuesFactory;
    private Func<ISnapshot>? _storeGeneratedValuesFactory;
    private Func<ValueBuffer, ISnapshot>? _shadowValuesFactory;
    private Func<ISnapshot>? _emptyShadowValuesFactory;
    private IProperty[]? _foreignKeyProperties;
    private IProperty[]? _valueGeneratingProperties;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public RuntimeEntityType(
        string name,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        bool sharedClrType,
        RuntimeModel model,
        RuntimeEntityType? baseType,
        string? discriminatorProperty,
        ChangeTrackingStrategy changeTrackingStrategy,
        PropertyInfo? indexerPropertyInfo,
        bool propertyBag,
        object? discriminatorValue)
    {
        Name = name;
        _clrType = type;
        _hasSharedClrType = sharedClrType;
        Model = model;
        if (baseType != null)
        {
            _baseType = baseType;
            baseType._directlyDerivedTypes.Add(this);
        }

        _changeTrackingStrategy = changeTrackingStrategy;
        _indexerPropertyInfo = indexerPropertyInfo;
        _isPropertyBag = propertyBag;
        SetAnnotation(CoreAnnotationNames.DiscriminatorProperty, discriminatorProperty);
        _discriminatorValue = discriminatorValue;

        _properties = new SortedDictionary<string, RuntimeProperty>(new PropertyNameComparer(this));
    }

    /// <summary>
    ///     Gets the name of this type.
    /// </summary>
    public virtual string Name { [DebuggerStepThrough] get; }

    /// <summary>
    ///     Gets the model that this type belongs to.
    /// </summary>
    public virtual RuntimeModel Model { [DebuggerStepThrough] get; private set; }

    /// <summary>
    ///     Re-parents this entity type to the given model.
    /// </summary>
    /// <param name="model">The new parent model.</param>
    public virtual void Reparent(RuntimeModel model)
        => Model = model;

    private IEnumerable<RuntimeEntityType> GetDerivedTypes()
    {
        if (_directlyDerivedTypes.Count == 0)
        {
            return Enumerable.Empty<RuntimeEntityType>();
        }

        var derivedTypes = new List<RuntimeEntityType>();
        var type = this;
        var currentTypeIndex = 0;
        while (type != null)
        {
            derivedTypes.AddRange(type._directlyDerivedTypes);
            type = derivedTypes.Count > currentTypeIndex
                ? derivedTypes[currentTypeIndex]
                : null;
            currentTypeIndex++;
        }

        return derivedTypes;
    }

    private RuntimeKey? FindPrimaryKey()
        => _baseType?.FindPrimaryKey() ?? _primaryKey;

    /// <summary>
    ///     Sets the primary key for this entity type.
    /// </summary>
    /// <param name="key">The new primary key.</param>
    public virtual void SetPrimaryKey(RuntimeKey key)
    {
        foreach (var property in key.Properties)
        {
            _properties.Remove(property.Name);
            property.PrimaryKey = key;
        }

        _primaryKey = key;

        foreach (var property in key.Properties)
        {
            _properties.Add(property.Name, property);
        }
    }

    /// <summary>
    ///     Adds a new alternate key to this entity type.
    /// </summary>
    /// <param name="properties">The properties that make up the alternate key.</param>
    /// <returns>The newly created key.</returns>
    public virtual RuntimeKey AddKey(IReadOnlyList<RuntimeProperty> properties)
    {
        var key = new RuntimeKey(properties);
        _keys.Add(properties, key);

        foreach (var property in properties)
        {
            if (property.Keys == null)
            {
                property.Keys = new List<RuntimeKey> { key };
            }
            else
            {
                property.Keys.Add(key);
            }
        }

        return key;
    }

    /// <summary>
    ///     Gets the primary or alternate key that is defined on the given properties.
    ///     Returns <see langword="null" /> if no key is defined for the given properties.
    /// </summary>
    /// <param name="properties">The properties that make up the key.</param>
    /// <returns>The key, or <see langword="null" /> if none is defined.</returns>
    public virtual RuntimeKey? FindKey(IReadOnlyList<IReadOnlyProperty> properties)
        => _keys.TryGetValue(properties, out var key)
            ? key
            : _baseType?.FindKey(properties);

    private IEnumerable<RuntimeKey> GetDeclaredKeys()
        => _keys.Values;

    private IEnumerable<RuntimeKey> GetKeys()
        => _baseType?.GetKeys().Concat(_keys.Values) ?? _keys.Values;

    /// <summary>
    ///     Adds a new relationship to this entity type.
    /// </summary>
    /// <param name="properties">The properties that the foreign key is defined on.</param>
    /// <param name="principalKey">The primary or alternate key that is referenced.</param>
    /// <param name="principalEntityType">
    ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
    ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
    ///     base type of the hierarchy).
    /// </param>
    /// <param name="deleteBehavior">
    ///     A value indicating how a delete operation is applied to dependent entities in the relationship when the
    ///     principal is deleted or the relationship is severed.
    /// </param>
    /// <param name="unique">A value indicating whether the values assigned to the foreign key properties are unique.</param>
    /// <param name="required">A value indicating whether the principal entity is required.</param>
    /// <param name="requiredDependent">A value indicating whether the dependent entity is required.</param>
    /// <param name="ownership">A value indicating whether this relationship defines an ownership.</param>
    /// <returns>The newly created foreign key.</returns>
    public virtual RuntimeForeignKey AddForeignKey(
        IReadOnlyList<RuntimeProperty> properties,
        RuntimeKey principalKey,
        RuntimeEntityType principalEntityType,
        DeleteBehavior deleteBehavior = ForeignKey.DefaultDeleteBehavior,
        bool unique = false,
        bool required = false,
        bool requiredDependent = false,
        bool ownership = false)
    {
        var foreignKey = new RuntimeForeignKey(
            properties, principalKey, this, principalEntityType, deleteBehavior, unique, required, requiredDependent, ownership);

        _foreignKeys.Add(foreignKey);

        foreach (var property in foreignKey.Properties)
        {
            if (property.ForeignKeys == null)
            {
                property.ForeignKeys = new List<RuntimeForeignKey> { foreignKey };
            }
            else
            {
                property.ForeignKeys.Add(foreignKey);
            }
        }

        if (principalKey.ReferencingForeignKeys == null)
        {
            principalKey.ReferencingForeignKeys = new SortedSet<RuntimeForeignKey>(ForeignKeyComparer.Instance) { foreignKey };
        }
        else
        {
            principalKey.ReferencingForeignKeys.Add(foreignKey);
        }

        if (principalEntityType.DeclaredReferencingForeignKeys == null)
        {
            principalEntityType.DeclaredReferencingForeignKeys =
                new SortedSet<RuntimeForeignKey>(ForeignKeyComparer.Instance) { foreignKey };
        }
        else
        {
            principalEntityType.DeclaredReferencingForeignKeys.Add(foreignKey);
        }

        return foreignKey;
    }

    private IEnumerable<RuntimeForeignKey> FindForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
        => _baseType != null
            ? _foreignKeys.Count == 0
                ? _baseType.FindForeignKeys(properties)
                : _baseType.FindForeignKeys(properties).Concat(FindDeclaredForeignKeys(properties))
            : FindDeclaredForeignKeys(properties);

    /// <summary>
    ///     Gets the foreign key for the given properties that points to a given primary or alternate key.
    ///     Returns <see langword="null" /> if no foreign key is found.
    /// </summary>
    /// <param name="properties">The properties that the foreign key is defined on.</param>
    /// <param name="principalKey">The primary or alternate key that is referenced.</param>
    /// <param name="principalEntityType">
    ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
    ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
    ///     base type of the hierarchy).
    /// </param>
    /// <returns>The foreign key, or <see langword="null" /> if none is defined.</returns>
    public virtual RuntimeForeignKey? FindForeignKey(
        IReadOnlyList<IReadOnlyProperty> properties,
        IReadOnlyKey principalKey,
        IReadOnlyEntityType principalEntityType)
        => FindDeclaredForeignKey(properties, principalKey, principalEntityType)
            ?? _baseType?.FindForeignKey(properties, principalKey, principalEntityType);

    private IEnumerable<RuntimeForeignKey> GetDerivedForeignKeys()
        => _directlyDerivedTypes.Count == 0
            ? Enumerable.Empty<RuntimeForeignKey>()
            : GetDerivedTypes().SelectMany(et => et._foreignKeys);

    private IEnumerable<RuntimeForeignKey> GetForeignKeys()
        => _baseType != null
            ? _foreignKeys.Count == 0
                ? _baseType.GetForeignKeys()
                : _baseType.GetForeignKeys().Concat(_foreignKeys)
            : _foreignKeys;

    /// <summary>
    ///     Gets the foreign keys declared on this entity type using the given properties.
    /// </summary>
    /// <param name="properties">The properties to find the foreign keys on.</param>
    /// <returns>Declared foreign keys.</returns>
    public virtual IEnumerable<RuntimeForeignKey> FindDeclaredForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
        => _foreignKeys.Count == 0
            ? Enumerable.Empty<RuntimeForeignKey>()
            : _foreignKeys.Where(fk => PropertyListComparer.Instance.Equals(fk.Properties, properties));

    private RuntimeForeignKey? FindDeclaredForeignKey(
        IReadOnlyList<IReadOnlyProperty> properties,
        IReadOnlyKey principalKey,
        IReadOnlyEntityType principalEntityType)
    {
        if (_foreignKeys.Count == 0)
        {
            return null;
        }

        foreach (var fk in FindDeclaredForeignKeys(properties))
        {
            if (PropertyListComparer.Instance.Equals(fk.PrincipalKey.Properties, principalKey.Properties)
                && fk.PrincipalEntityType == principalEntityType)
            {
                return fk;
            }
        }

        return null;
    }

    private IEnumerable<RuntimeForeignKey> GetReferencingForeignKeys()
        => _baseType != null
            ? (DeclaredReferencingForeignKeys?.Count ?? 0) == 0
                ? _baseType.GetReferencingForeignKeys()
                : _baseType.GetReferencingForeignKeys().Concat(GetDeclaredReferencingForeignKeys())
            : GetDeclaredReferencingForeignKeys();

    private IEnumerable<RuntimeForeignKey> GetDeclaredReferencingForeignKeys()
        => DeclaredReferencingForeignKeys ?? Enumerable.Empty<RuntimeForeignKey>();

    private SortedSet<RuntimeForeignKey>? DeclaredReferencingForeignKeys { get; set; }

    /// <summary>
    ///     Adds a new navigation property to this entity type.
    /// </summary>
    /// <param name="name">The name of the navigation property to add.</param>
    /// <param name="foreignKey">The foreign key that defines the relationship this navigation property will navigate.</param>
    /// <param name="onDependent">
    ///     A value indicating whether the navigation property is defined on the dependent side of the underlying foreign key.
    /// </param>
    /// <param name="clrType">The type of value that this navigation holds.</param>
    /// <param name="propertyInfo">The corresponding CLR property or <see langword="null" /> for a shadow navigation.</param>
    /// <param name="fieldInfo">The corresponding CLR field or <see langword="null" /> for a shadow navigation.</param>
    /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" /> used for this navigation.</param>
    /// <param name="eagerLoaded">A value indicating whether this navigation should be eager loaded by default.</param>
    /// <param name="lazyLoadingEnabled">A value indicating whether this navigation should be enabled for lazy-loading.</param>
    /// <returns>The newly created navigation property.</returns>
    public virtual RuntimeNavigation AddNavigation(
        string name,
        RuntimeForeignKey foreignKey,
        bool onDependent,
        Type clrType,
        PropertyInfo? propertyInfo = null,
        FieldInfo? fieldInfo = null,
        PropertyAccessMode propertyAccessMode = Internal.Model.DefaultPropertyAccessMode,
        bool eagerLoaded = false,
        bool lazyLoadingEnabled = true)
    {
        var navigation = new RuntimeNavigation(
            name, clrType, propertyInfo, fieldInfo, foreignKey, propertyAccessMode, eagerLoaded, lazyLoadingEnabled);

        _navigations.Add(name, navigation);

        foreignKey.AddNavigation(navigation, onDependent);

        return navigation;
    }

    /// <summary>
    ///     Gets a navigation property on the given entity type. Returns <see langword="null" /> if no navigation property is found.
    /// </summary>
    /// <param name="name">The name of the navigation property on the entity class.</param>
    /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
    public virtual RuntimeNavigation? FindNavigation(string name)
        => (RuntimeNavigation?)((IReadOnlyEntityType)this).FindNavigation(name);

    private RuntimeNavigation? FindDeclaredNavigation(string name)
        => _navigations.TryGetValue(name, out var navigation)
            ? navigation
            : null;

    private IEnumerable<RuntimeNavigation> GetDeclaredNavigations()
        => _navigations.Values;

    private IEnumerable<RuntimeNavigation> GetNavigations()
        => _baseType != null
            ? _navigations.Count == 0 ? _baseType.GetNavigations() : _baseType.GetNavigations().Concat(_navigations.Values)
            : _navigations.Values;

    /// <summary>
    ///     Adds a new skip navigation property to this entity type.
    /// </summary>
    /// <param name="name">The name of the skip navigation property to add.</param>
    /// <param name="targetEntityType">The entity type that the skip navigation property will hold an instance(s) of.</param>
    /// <param name="foreignKey">The foreign key to the join type.</param>
    /// <param name="collection">Whether the navigation property is a collection property.</param>
    /// <param name="onDependent">
    ///     Whether the navigation property is defined on the dependent side of the underlying foreign key.
    /// </param>
    /// <param name="clrType">The type of value that this navigation holds.</param>
    /// <param name="propertyInfo">The corresponding CLR property or <see langword="null" /> for a shadow navigation.</param>
    /// <param name="fieldInfo">The corresponding CLR field or <see langword="null" /> for a shadow navigation.</param>
    /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" /> used for this navigation.</param>
    /// <param name="eagerLoaded">A value indicating whether this navigation should be eager loaded by default.</param>
    /// <param name="lazyLoadingEnabled">A value indicating whether this navigation should be enabled for lazy-loading.</param>
    /// <returns>The newly created skip navigation property.</returns>
    public virtual RuntimeSkipNavigation AddSkipNavigation(
        string name,
        RuntimeEntityType targetEntityType,
        RuntimeForeignKey foreignKey,
        bool collection,
        bool onDependent,
        Type clrType,
        PropertyInfo? propertyInfo = null,
        FieldInfo? fieldInfo = null,
        PropertyAccessMode propertyAccessMode = Internal.Model.DefaultPropertyAccessMode,
        bool eagerLoaded = false,
        bool lazyLoadingEnabled = true)
    {
        var skipNavigation = new RuntimeSkipNavigation(
            name,
            clrType,
            propertyInfo,
            fieldInfo,
            this,
            targetEntityType,
            foreignKey,
            collection,
            onDependent,
            propertyAccessMode,
            eagerLoaded,
            lazyLoadingEnabled);

        _skipNavigations.Add(name, skipNavigation);

        return skipNavigation;
    }

    /// <summary>
    ///     Gets a skip navigation property on this entity type. Returns <see langword="null" /> if no skip navigation property is found.
    /// </summary>
    /// <param name="name">The name of the navigation property on the entity class.</param>
    /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
    public virtual RuntimeSkipNavigation? FindSkipNavigation(string name)
        => FindDeclaredSkipNavigation(name) ?? _baseType?.FindSkipNavigation(name);

    private RuntimeSkipNavigation? FindSkipNavigation(MemberInfo memberInfo)
        => FindSkipNavigation(memberInfo.GetSimpleMemberName());

    private RuntimeSkipNavigation? FindDeclaredSkipNavigation(string name)
        => _skipNavigations.TryGetValue(name, out var navigation)
            ? navigation
            : null;

    private IEnumerable<RuntimeSkipNavigation> GetDeclaredSkipNavigations()
        => _skipNavigations.Values;

    private IEnumerable<RuntimeSkipNavigation> GetDerivedSkipNavigations()
        => _directlyDerivedTypes.Count == 0
            ? Enumerable.Empty<RuntimeSkipNavigation>()
            : GetDerivedTypes().SelectMany(et => et.GetDeclaredSkipNavigations());

    private IEnumerable<RuntimeSkipNavigation> GetSkipNavigations()
        => _baseType != null
            ? _skipNavigations.Count == 0
                ? _baseType.GetSkipNavigations()
                : _baseType.GetSkipNavigations().Concat(_skipNavigations.Values)
            : _skipNavigations.Values;

    /// <summary>
    ///     Adds an index to this entity type.
    /// </summary>
    /// <param name="properties">The properties that are to be indexed.</param>
    /// <param name="name">The name of the index.</param>
    /// <param name="unique">A value indicating whether the values assigned to the indexed properties are unique.</param>
    /// <returns>The newly created index.</returns>
    public virtual RuntimeIndex AddIndex(
        IReadOnlyList<RuntimeProperty> properties,
        string? name = null,
        bool unique = false)
    {
        var index = new RuntimeIndex(properties, this, name, unique);
        if (name != null)
        {
            _namedIndexes.Add(name, index);
        }
        else
        {
            _unnamedIndexes.Add(properties, index);
        }

        foreach (var property in properties)
        {
            if (property.Indexes == null)
            {
                property.Indexes = new List<RuntimeIndex> { index };
            }
            else
            {
                property.Indexes.Add(index);
            }
        }

        return index;
    }

    /// <summary>
    ///     Gets the unnamed index defined on the given properties. Returns <see langword="null" /> if no such index is defined.
    /// </summary>
    /// <remarks>
    ///     Named indexes will not be returned even if the list of properties matches.
    /// </remarks>
    /// <param name="properties">The properties to find the index on.</param>
    /// <returns>The index, or <see langword="null" /> if none is found.</returns>
    public virtual RuntimeIndex? FindIndex(IReadOnlyList<IReadOnlyProperty> properties)
        => _unnamedIndexes.TryGetValue(properties, out var index)
            ? index
            : _baseType?.FindIndex(properties);

    /// <summary>
    ///     Gets the index with the given name. Returns <see langword="null" /> if no such index exists.
    /// </summary>
    /// <param name="name">The name of the index.</param>
    /// <returns>The index, or <see langword="null" /> if none is found.</returns>
    public virtual RuntimeIndex? FindIndex(string name)
        => _namedIndexes.TryGetValue(name, out var index)
            ? index
            : _baseType?.FindIndex(name);

    private IEnumerable<RuntimeIndex> GetDeclaredIndexes()
        => _namedIndexes.Count == 0
            ? _unnamedIndexes.Values
            : _unnamedIndexes.Values.Concat(_namedIndexes.Values);

    private IEnumerable<RuntimeIndex> GetDerivedIndexes()
        => _directlyDerivedTypes.Count == 0
            ? Enumerable.Empty<RuntimeIndex>()
            : GetDerivedTypes().SelectMany(et => et.GetDeclaredIndexes());

    private IEnumerable<RuntimeIndex> GetIndexes()
        => _baseType != null
            ? _namedIndexes.Count == 0 && _unnamedIndexes.Count == 0
                ? _baseType.GetIndexes()
                : _baseType.GetIndexes().Concat(GetDeclaredIndexes())
            : GetDeclaredIndexes();

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
    /// <param name="typeMapping">The <see cref="CoreTypeMapping" /> for this property.</param>
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
        Func<IProperty, IEntityType, ValueGenerator>? valueGeneratorFactory = null,
        ValueConverter? valueConverter = null,
        ValueComparer? valueComparer = null,
        ValueComparer? keyValueComparer = null,
        ValueComparer? providerValueComparer = null,
        CoreTypeMapping? typeMapping = null)
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
            typeMapping);

        _properties.Add(property.Name, property);

        return property;
    }

    /// <summary>
    ///     Gets the property with a given name. Returns <see langword="null" /> if no property with the given name is defined.
    /// </summary>
    /// <remarks>
    ///     This API only finds scalar properties and does not find navigation properties. Use
    ///     <see cref="FindNavigation(string)" /> to find a navigation property.
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

    private IEnumerable<RuntimeProperty> GetProperties()
        => _baseType != null
            ? _baseType.GetProperties().Concat(_properties.Values)
            : _properties.Values;

    /// <inheritdoc />
    [DebuggerStepThrough]
    public virtual PropertyInfo? FindIndexerPropertyInfo()
        => _indexerPropertyInfo;

    /// <summary>
    ///     Adds a service property to this entity type.
    /// </summary>
    /// <param name="name">The name of the property to add.</param>
    /// <param name="propertyInfo">The corresponding CLR property or <see langword="null" /> for a shadow property.</param>
    /// <param name="fieldInfo">The corresponding CLR field or <see langword="null" /> for a shadow property.</param>
    /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" /> used for this property.</param>
    /// <returns>The newly created service property.</returns>
    public virtual RuntimeServiceProperty AddServiceProperty(
        string name,
        PropertyInfo? propertyInfo = null,
        FieldInfo? fieldInfo = null,
        PropertyAccessMode propertyAccessMode = Internal.Model.DefaultPropertyAccessMode)
        => AddServiceProperty(name, (propertyInfo?.PropertyType ?? fieldInfo?.FieldType)!, propertyInfo, fieldInfo, propertyAccessMode);

    /// <summary>
    ///     Adds a service property to this entity type.
    /// </summary>
    /// <param name="name">The name of the property to add.</param>
    /// <param name="serviceType">The type of the service.</param>
    /// <param name="propertyInfo">The corresponding CLR property or <see langword="null" /> for a shadow property.</param>
    /// <param name="fieldInfo">The corresponding CLR field or <see langword="null" /> for a shadow property.</param>
    /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" /> used for this property.</param>
    /// <returns>The newly created service property.</returns>
    public virtual RuntimeServiceProperty AddServiceProperty(
        string name,
        Type serviceType,
        PropertyInfo? propertyInfo = null,
        FieldInfo? fieldInfo = null,
        PropertyAccessMode propertyAccessMode = Internal.Model.DefaultPropertyAccessMode)
    {
        var serviceProperty = new RuntimeServiceProperty(
            name,
            propertyInfo,
            fieldInfo,
            serviceType,
            this,
            propertyAccessMode);

        _serviceProperties[serviceProperty.Name] = serviceProperty;
        _hasServiceProperties = true;

        return serviceProperty;
    }

    /// <summary>
    ///     Gets the service property with a given name.
    ///     Returns <see langword="null" /> if no property with the given name is defined.
    /// </summary>
    /// <remarks>
    ///     This API only finds service properties and does not find scalar or navigation properties.
    /// </remarks>
    /// <param name="name">The name of the service property.</param>
    /// <returns>The service property, or <see langword="null" /> if none is found.</returns>
    public virtual RuntimeServiceProperty? FindServiceProperty(string name)
        => FindDeclaredServiceProperty(name) ?? _baseType?.FindServiceProperty(name);

    private RuntimeServiceProperty? FindDeclaredServiceProperty(string name)
        => _serviceProperties.TryGetValue(name, out var property)
            ? property
            : null;

    private bool HasServiceProperties()
        => _hasServiceProperties || _baseType != null && _baseType.HasServiceProperties();

    private IEnumerable<RuntimeServiceProperty> GetServiceProperties()
        => _baseType != null
            ? _hasServiceProperties
                ? _baseType.GetServiceProperties().Concat(_serviceProperties.Values)
                : _baseType.GetServiceProperties()
            : _serviceProperties.Values;

    private IEnumerable<RuntimeServiceProperty> GetDeclaredServiceProperties()
        => _serviceProperties.Values;

    private IEnumerable<RuntimeServiceProperty> GetDerivedServiceProperties()
        => _directlyDerivedTypes.Count == 0
            ? Enumerable.Empty<RuntimeServiceProperty>()
            : GetDerivedTypes().SelectMany(et => et.GetDeclaredServiceProperties());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual RuntimeTrigger AddTrigger(string modelName)
    {
        var trigger = new RuntimeTrigger(this, modelName);

        _triggers.Add(modelName, trigger);

        return trigger;
    }

    /// <summary>
    ///     Finds a trigger with the given name.
    /// </summary>
    /// <param name="modelName">The trigger name.</param>
    /// <returns>The trigger or <see langword="null" /> if no trigger with the given name was found.</returns>
    public virtual RuntimeTrigger? FindDeclaredTrigger(string modelName)
    {
        Check.NotEmpty(modelName, nameof(modelName));

        return _triggers.TryGetValue(modelName, out var trigger)
            ? trigger
            : null;
    }

    private IEnumerable<RuntimeTrigger> GetDeclaredTriggers()
        => _triggers.Values;

    private IEnumerable<RuntimeTrigger> GetTriggers()
        => _baseType != null
            ? _baseType.GetTriggers().Concat(GetDeclaredTriggers())
            : GetDeclaredTriggers();

    /// <summary>
    ///     Gets or sets the <see cref="InstantiationBinding" /> for the preferred constructor.
    /// </summary>
    public virtual InstantiationBinding? ConstructorBinding
    {
        get => !_clrType.IsAbstract
            ? NonCapturingLazyInitializer.EnsureInitialized(
                ref _constructorBinding, this, static entityType =>
                {
                    ((IModel)entityType.Model).GetModelDependencies().ConstructorBindingFactory.GetBindings(
                        entityType,
                        out entityType._constructorBinding,
                        out entityType._serviceOnlyConstructorBinding);
                })
            : _constructorBinding;

        [DebuggerStepThrough]
        set => _constructorBinding = value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual InstantiationBinding? ServiceOnlyConstructorBinding
    {
        [DebuggerStepThrough]
        get => _serviceOnlyConstructorBinding;

        [DebuggerStepThrough]
        set => _serviceOnlyConstructorBinding = value;
    }

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
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
        => ((IReadOnlyEntityType)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((IReadOnlyEntityType)this).ToDebugString(),
            () => ((IReadOnlyEntityType)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)]
    Type IReadOnlyTypeBase.ClrType
    {
        [DebuggerStepThrough]
        get => _clrType;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    ChangeTrackingStrategy IReadOnlyEntityType.GetChangeTrackingStrategy()
        => _changeTrackingStrategy;

    /// <inheritdoc />
    [DebuggerStepThrough]
    LambdaExpression? IReadOnlyEntityType.GetQueryFilter()
        => (LambdaExpression?)this[CoreAnnotationNames.QueryFilter];

    /// <inheritdoc />
    [DebuggerStepThrough]
    string? IReadOnlyEntityType.GetDiscriminatorPropertyName()
    {
        if (_baseType != null)
        {
            return ((IReadOnlyEntityType)this).GetRootType().GetDiscriminatorPropertyName();
        }

        return (string?)this[CoreAnnotationNames.DiscriminatorProperty];
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    object? IReadOnlyEntityType.GetDiscriminatorValue()
        => _discriminatorValue;

    /// <inheritdoc />
    bool IReadOnlyTypeBase.HasSharedClrType
    {
        [DebuggerStepThrough]
        get => _hasSharedClrType;
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
    IReadOnlyEntityType? IReadOnlyEntityType.BaseType
    {
        [DebuggerStepThrough]
        get => _baseType;
    }

    /// <inheritdoc />
    IEntityType? IEntityType.BaseType
    {
        [DebuggerStepThrough]
        get => _baseType;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyEntityType> IReadOnlyEntityType.GetDerivedTypes()
        => GetDerivedTypes();

    /// <inheritdoc />
    IEnumerable<IReadOnlyEntityType> IReadOnlyEntityType.GetDerivedTypesInclusive()
        => _directlyDerivedTypes.Count == 0
            ? new[] { this }
            : new[] { this }.Concat(GetDerivedTypes());

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyEntityType> IReadOnlyEntityType.GetDirectlyDerivedTypes()
        => _directlyDerivedTypes;

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IEntityType> IEntityType.GetDirectlyDerivedTypes()
        => _directlyDerivedTypes;

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyKey? IReadOnlyEntityType.FindPrimaryKey()
        => FindPrimaryKey();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IKey? IEntityType.FindPrimaryKey()
        => FindPrimaryKey();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyKey? IReadOnlyEntityType.FindKey(IReadOnlyList<IReadOnlyProperty> properties)
        => FindKey(properties);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IKey? IEntityType.FindKey(IReadOnlyList<IReadOnlyProperty> properties)
        => FindKey(properties);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyKey> IReadOnlyEntityType.GetDeclaredKeys()
        => GetDeclaredKeys();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IKey> IEntityType.GetDeclaredKeys()
        => GetDeclaredKeys();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyKey> IReadOnlyEntityType.GetKeys()
        => GetKeys();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IKey> IEntityType.GetKeys()
        => GetKeys();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyForeignKey? IReadOnlyEntityType.FindForeignKey(
        IReadOnlyList<IReadOnlyProperty> properties,
        IReadOnlyKey principalKey,
        IReadOnlyEntityType principalEntityType)
        => FindForeignKey(properties, principalKey, principalEntityType);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IForeignKey? IEntityType.FindForeignKey(
        IReadOnlyList<IReadOnlyProperty> properties,
        IReadOnlyKey principalKey,
        IReadOnlyEntityType principalEntityType)
        => FindForeignKey(properties, principalKey, principalEntityType);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.FindForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
        => FindForeignKeys(properties);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IForeignKey> IEntityType.FindForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
        => FindForeignKeys(properties);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.FindDeclaredForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
        => FindDeclaredForeignKeys(properties);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IForeignKey> IEntityType.FindDeclaredForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
        => FindDeclaredForeignKeys(properties);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetForeignKeys()
        => GetForeignKeys();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IForeignKey> IEntityType.GetForeignKeys()
        => GetForeignKeys();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetDeclaredForeignKeys()
        => _foreignKeys;

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IForeignKey> IEntityType.GetDeclaredForeignKeys()
        => _foreignKeys;

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetDerivedForeignKeys()
        => GetDerivedForeignKeys();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IForeignKey> IEntityType.GetDerivedForeignKeys()
        => GetDerivedForeignKeys();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetDeclaredReferencingForeignKeys()
        => GetDeclaredReferencingForeignKeys();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IForeignKey> IEntityType.GetDeclaredReferencingForeignKeys()
        => GetDeclaredReferencingForeignKeys();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetReferencingForeignKeys()
        => GetReferencingForeignKeys();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IForeignKey> IEntityType.GetReferencingForeignKeys()
        => GetReferencingForeignKeys();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyNavigation> IReadOnlyEntityType.GetDeclaredNavigations()
        => GetDeclaredNavigations();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<INavigation> IEntityType.GetDeclaredNavigations()
        => GetDeclaredNavigations();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyNavigation? IReadOnlyEntityType.FindDeclaredNavigation(string name)
        => FindDeclaredNavigation(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    INavigation? IEntityType.FindDeclaredNavigation(string name)
        => FindDeclaredNavigation(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyNavigation> IReadOnlyEntityType.GetDerivedNavigations()
        => _directlyDerivedTypes.Count == 0
            ? Enumerable.Empty<RuntimeNavigation>()
            : GetDerivedTypes().SelectMany(et => et.GetDeclaredNavigations());

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyNavigation> IReadOnlyEntityType.GetNavigations()
        => GetNavigations();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<INavigation> IEntityType.GetNavigations()
        => GetNavigations();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlySkipNavigation? IReadOnlyEntityType.FindSkipNavigation(MemberInfo memberInfo)
        => FindSkipNavigation(memberInfo);

    /// <inheritdoc />
    [DebuggerStepThrough]
    ISkipNavigation? IEntityType.FindSkipNavigation(MemberInfo memberInfo)
        => FindSkipNavigation(memberInfo);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlySkipNavigation? IReadOnlyEntityType.FindSkipNavigation(string name)
        => FindSkipNavigation(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    ISkipNavigation? IEntityType.FindSkipNavigation(string name)
        => FindSkipNavigation(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlySkipNavigation? IReadOnlyEntityType.FindDeclaredSkipNavigation(string name)
        => FindDeclaredSkipNavigation(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlySkipNavigation> IReadOnlyEntityType.GetDeclaredSkipNavigations()
        => GetDeclaredSkipNavigations();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<ISkipNavigation> IEntityType.GetDeclaredSkipNavigations()
        => GetDeclaredSkipNavigations();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlySkipNavigation> IReadOnlyEntityType.GetDerivedSkipNavigations()
        => GetDerivedSkipNavigations();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<ISkipNavigation> IEntityType.GetDerivedSkipNavigations()
        => GetDerivedSkipNavigations();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlySkipNavigation> IReadOnlyEntityType.GetSkipNavigations()
        => GetSkipNavigations();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<ISkipNavigation> IEntityType.GetSkipNavigations()
        => GetSkipNavigations();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyIndex? IReadOnlyEntityType.FindIndex(IReadOnlyList<IReadOnlyProperty> properties)
        => FindIndex(properties);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IIndex? IEntityType.FindIndex(IReadOnlyList<IReadOnlyProperty> properties)
        => FindIndex(properties);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyIndex? IReadOnlyEntityType.FindIndex(string name)
        => FindIndex(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IIndex? IEntityType.FindIndex(string name)
        => FindIndex(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyIndex> IReadOnlyEntityType.GetDeclaredIndexes()
        => GetDeclaredIndexes();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IIndex> IEntityType.GetDeclaredIndexes()
        => GetDeclaredIndexes();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyIndex> IReadOnlyEntityType.GetDerivedIndexes()
        => GetDerivedIndexes();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IIndex> IEntityType.GetDerivedIndexes()
        => GetDerivedIndexes();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyIndex> IReadOnlyEntityType.GetIndexes()
        => GetIndexes();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IIndex> IEntityType.GetIndexes()
        => GetIndexes();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyProperty? IReadOnlyEntityType.FindDeclaredProperty(string name)
        => FindDeclaredProperty(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IProperty? IEntityType.FindDeclaredProperty(string name)
        => FindDeclaredProperty(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyList<IReadOnlyProperty>? IReadOnlyEntityType.FindProperties(IReadOnlyList<string> propertyNames)
        => FindProperties(propertyNames);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyProperty? IReadOnlyEntityType.FindProperty(string name)
        => FindProperty(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IProperty? IEntityType.FindProperty(string name)
        => FindProperty(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyProperty> IReadOnlyEntityType.GetDeclaredProperties()
        => GetDeclaredProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IProperty> IEntityType.GetDeclaredProperties()
        => GetDeclaredProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyProperty> IReadOnlyEntityType.GetDerivedProperties()
        => GetDerivedProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyProperty> IReadOnlyEntityType.GetProperties()
        => GetProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IProperty> IEntityType.GetProperties()
        => GetProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyTrigger? IReadOnlyEntityType.FindDeclaredTrigger(string name)
        => FindDeclaredTrigger(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    ITrigger? IEntityType.FindDeclaredTrigger(string name)
        => FindDeclaredTrigger(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyTrigger> IReadOnlyEntityType.GetDeclaredTriggers()
        => GetDeclaredTriggers();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<ITrigger> IEntityType.GetDeclaredTriggers()
        => GetDeclaredTriggers();

    /// <inheritdoc />
    PropertyCounts IRuntimeEntityType.Counts
        => NonCapturingLazyInitializer.EnsureInitialized(ref _counts, this, static entityType => entityType.CalculateCounts());

    /// <inheritdoc />
    Func<InternalEntityEntry, ISnapshot> IRuntimeEntityType.RelationshipSnapshotFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _relationshipSnapshotFactory, this,
            static entityType => new RelationshipSnapshotFactoryFactory().Create(entityType));

    /// <inheritdoc />
    Func<InternalEntityEntry, ISnapshot> IRuntimeEntityType.OriginalValuesFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _originalValuesFactory, this,
            static entityType => new OriginalValuesFactoryFactory().Create(entityType));

    /// <inheritdoc />
    Func<ISnapshot> IRuntimeEntityType.StoreGeneratedValuesFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _storeGeneratedValuesFactory, this,
            static entityType => new StoreGeneratedValuesFactoryFactory().CreateEmpty(entityType));

    /// <inheritdoc />
    Func<InternalEntityEntry, ISnapshot> IRuntimeEntityType.TemporaryValuesFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _temporaryValuesFactory, this,
            static entityType => new TemporaryValuesFactoryFactory().Create(entityType));

    /// <inheritdoc />
    Func<ValueBuffer, ISnapshot> IRuntimeEntityType.ShadowValuesFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _shadowValuesFactory, this,
            static entityType => new ShadowValuesFactoryFactory().Create(entityType));

    /// <inheritdoc />
    Func<ISnapshot> IRuntimeEntityType.EmptyShadowValuesFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _emptyShadowValuesFactory, this,
            static entityType => new EmptyShadowValuesFactoryFactory().CreateEmpty(entityType));

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IProperty> IEntityType.GetForeignKeyProperties()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _foreignKeyProperties, this,
            static entityType => { return entityType.GetProperties().Where(p => ((IReadOnlyProperty)p).IsForeignKey()).ToArray(); });

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IProperty> IEntityType.GetValueGeneratingProperties()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _valueGeneratingProperties, this,
            static entityType => { return entityType.GetProperties().Where(p => p.RequiresValueGenerator()).ToArray(); });

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyServiceProperty? IReadOnlyEntityType.FindServiceProperty(string name)
        => FindServiceProperty(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IServiceProperty? IEntityType.FindServiceProperty(string name)
        => FindServiceProperty(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyServiceProperty> IReadOnlyEntityType.GetDeclaredServiceProperties()
        => GetDeclaredServiceProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IServiceProperty> IEntityType.GetDeclaredServiceProperties()
        => GetDeclaredServiceProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyServiceProperty> IReadOnlyEntityType.GetDerivedServiceProperties()
        => GetDerivedServiceProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IReadOnlyEntityType.HasServiceProperties()
        => HasServiceProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyServiceProperty> IReadOnlyEntityType.GetServiceProperties()
        => GetServiceProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IServiceProperty> IEntityType.GetServiceProperties()
        => GetServiceProperties();

    IEnumerable<IDictionary<string, object?>> IReadOnlyEntityType.GetSeedData(bool providerValues)
        => throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);

    PropertyAccessMode IReadOnlyTypeBase.GetPropertyAccessMode()
        => throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);

    PropertyAccessMode IReadOnlyTypeBase.GetNavigationAccessMode()
        => throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);

    ConfigurationSource? IRuntimeEntityType.GetConstructorBindingConfigurationSource()
        => throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);

    ConfigurationSource? IRuntimeEntityType.GetServiceOnlyConstructorBindingConfigurationSource()
        => throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
}
