// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
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
public class RuntimeEntityType : RuntimeTypeBase, IRuntimeEntityType
{
    private readonly List<RuntimeForeignKey> _foreignKeys;
    private readonly OrderedDictionary<string, RuntimeNavigation> _navigations;
    private OrderedDictionary<string, RuntimeSkipNavigation>? _skipNavigations;
    private OrderedDictionary<string, RuntimeServiceProperty>? _serviceProperties;
    private readonly OrderedDictionary<IReadOnlyList<IReadOnlyProperty>, RuntimeIndex> _unnamedIndexes;
    private OrderedDictionary<string, RuntimeIndex>? _namedIndexes;
    private readonly OrderedDictionary<IReadOnlyList<IReadOnlyProperty>, RuntimeKey> _keys;
    private OrderedDictionary<string, RuntimeTrigger>? _triggers;
    private readonly object? _discriminatorValue;
    private readonly bool _hasSharedClrType;
    private RuntimeKey? _primaryKey;
    private InstantiationBinding? _constructorBinding;
    private InstantiationBinding? _serviceOnlyConstructorBinding;

    // Warning: Never access these fields directly as access needs to be thread-safe
    private PropertyCounts? _counts;
    private Func<InternalEntityEntry, ISnapshot>? _relationshipSnapshotFactory;
    private IProperty[]? _foreignKeyProperties;
    private IProperty[]? _valueGeneratingProperties;
    private Func<InternalEntityEntry, ISnapshot>? _originalValuesFactory;
    private Func<InternalEntityEntry, ISnapshot>? _temporaryValuesFactory;
    private Func<ISnapshot>? _storeGeneratedValuesFactory;
    private Func<IDictionary<string, object?>, ISnapshot>? _shadowValuesFactory;
    private Func<ISnapshot>? _emptyShadowValuesFactory;
    private RuntimePropertyBase[]? _snapshottableProperties;
    private Func<MaterializationContext, object>? _materializer;
    private Func<MaterializationContext, object>? _emptyMaterializer;

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
        object? discriminatorValue,
        int derivedTypesCount,
        int propertyCount,
        int complexPropertyCount,
        int foreignKeyCount,
        int navigationCount,
        int skipNavigationCount,
        int servicePropertyCount,
        int unnamedIndexCount,
        int namedIndexCount,
        int keyCount,
        int triggerCount)
        : base(name, type, model, baseType, changeTrackingStrategy, indexerPropertyInfo, propertyBag,
            derivedTypesCount: derivedTypesCount,
            propertyCount: propertyCount,
            complexPropertyCount: complexPropertyCount)
    {
        _hasSharedClrType = sharedClrType;

        SetAnnotation(CoreAnnotationNames.DiscriminatorProperty, discriminatorProperty);
        _discriminatorValue = discriminatorValue;
        _foreignKeys = new(foreignKeyCount);
        _navigations = new(navigationCount, StringComparer.Ordinal);
        if (skipNavigationCount > 0)
        {
            _skipNavigations = new(skipNavigationCount, StringComparer.Ordinal);
        }
        if (servicePropertyCount > 0)
        {
            _serviceProperties = new(servicePropertyCount, StringComparer.Ordinal);
        }
        _unnamedIndexes = new(unnamedIndexCount, PropertyListComparer.Instance);
        if (namedIndexCount > 0)
        {
            _namedIndexes = new(namedIndexCount, StringComparer.Ordinal);
        }
        _keys = new(keyCount, PropertyListComparer.Instance);
        if (triggerCount > 0)
        {
            _triggers = new(triggerCount, StringComparer.Ordinal);
        }
    }

    private new RuntimeEntityType? BaseType
        => (RuntimeEntityType?)base.BaseType;

    /// <summary>
    ///     Re-parents this entity type to the given model.
    /// </summary>
    /// <param name="model">The new parent model.</param>
    public virtual void Reparent(RuntimeModel model)
        => Model = model;

    private RuntimeKey? FindPrimaryKey()
        => BaseType?.FindPrimaryKey() ?? _primaryKey;

    /// <summary>
    ///     Sets the primary key for this entity type.
    /// </summary>
    /// <param name="key">The new primary key.</param>
    public virtual void SetPrimaryKey(RuntimeKey key)
    {
        foreach (var property in key.Properties)
        {
            property.PrimaryKey = key;
        }

        _primaryKey = key;
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
                property.Keys = [key];
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
            : BaseType?.FindKey(properties);

    /// <summary>
    ///     Gets all keys declared on this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return keys declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same key more than once.
    ///     Use <see cref="GetKeys" /> to also return keys declared on base types.
    /// </remarks>
    /// <returns>Declared keys.</returns>
    public virtual IEnumerable<RuntimeKey> GetDeclaredKeys()
        => _keys.Values;

    /// <summary>
    ///     Gets the primary and alternate keys for this entity type.
    /// </summary>
    /// <returns>The primary and alternate keys.</returns>
    public virtual IEnumerable<RuntimeKey> GetKeys()
        => BaseType?.GetKeys().Concat(_keys.Values) ?? _keys.Values;

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
                property.ForeignKeys = new SortedSet<RuntimeForeignKey>(ForeignKeyComparer.Instance) { foreignKey };
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
        => BaseType != null
            ? _foreignKeys.Count == 0
                ? BaseType.FindForeignKeys(properties)
                : BaseType.FindForeignKeys(properties).Concat(FindDeclaredForeignKeys(properties))
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
            ?? BaseType?.FindForeignKey(properties, principalKey, principalEntityType);

    /// <summary>
    ///     Gets all foreign keys declared on this entity type..
    /// </summary>
    /// <remarks>
    ///     This method does not return foreign keys declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same foreign key more than once.
    ///     Use <see cref="GetForeignKeys" /> to also return foreign keys declared on base types.
    /// </remarks>
    /// <returns>Declared foreign keys.</returns>
    public virtual List<RuntimeForeignKey> GetDeclaredForeignKeys() => _foreignKeys;

    private IEnumerable<RuntimeForeignKey> GetDerivedForeignKeys()
        => !HasDirectlyDerivedTypes
            ? Enumerable.Empty<RuntimeForeignKey>()
            : GetDerivedTypes().Cast<RuntimeEntityType>().SelectMany(et => et._foreignKeys);

    /// <summary>
    ///     Gets the foreign keys defined on this entity type.
    /// </summary>
    /// <returns>The foreign keys defined on this entity type.</returns>
    public virtual IEnumerable<RuntimeForeignKey> GetForeignKeys()
        => BaseType != null
            ? _foreignKeys.Count == 0
                ? BaseType.GetForeignKeys()
                : BaseType.GetForeignKeys().Concat(_foreignKeys)
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
        => BaseType != null
            ? (DeclaredReferencingForeignKeys?.Count ?? 0) == 0
                ? BaseType.GetReferencingForeignKeys()
                : BaseType.GetReferencingForeignKeys().Concat(GetDeclaredReferencingForeignKeys())
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

        _navigations.Insert(name, navigation);

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
        => BaseType != null
            ? _navigations.Count == 0 ? BaseType.GetNavigations() : BaseType.GetNavigations().Concat(_navigations.Values)
            : _navigations.Values;

    private IEnumerable<RuntimeNavigation> FindDerivedNavigations(string name)
    {
        Check.NotNull(name, nameof(name));

        return !HasDirectlyDerivedTypes
            ? Enumerable.Empty<RuntimeNavigation>()
            : (IEnumerable<RuntimeNavigation>)GetDerivedTypes<RuntimeEntityType>()
                .Select(et => et.FindDeclaredNavigation(name)).Where(n => n != null);
    }

    /// <summary>
    ///     Gets the navigations with the given name on this type, base types or derived types.
    /// </summary>
    /// <returns>Type navigations.</returns>
    public virtual IEnumerable<RuntimeNavigation> FindNavigationsInHierarchy(string name)
        => !HasDirectlyDerivedTypes
            ? ToEnumerable(FindNavigation(name))
            : ToEnumerable(FindNavigation(name)).Concat(FindDerivedNavigations(name));

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

        _skipNavigations ??= new(StringComparer.Ordinal);
        _skipNavigations.Add(name, skipNavigation);

        return skipNavigation;
    }

    /// <summary>
    ///     Gets a skip navigation property on this entity type. Returns <see langword="null" /> if no skip navigation property is found.
    /// </summary>
    /// <param name="name">The name of the navigation property on the entity class.</param>
    /// <returns>The navigation property, or <see langword="null" /> if none is found.</returns>
    public virtual RuntimeSkipNavigation? FindSkipNavigation(string name)
        => FindDeclaredSkipNavigation(name) ?? BaseType?.FindSkipNavigation(name);

    private RuntimeSkipNavigation? FindSkipNavigation(MemberInfo memberInfo)
        => FindSkipNavigation(memberInfo.GetSimpleMemberName());

    private RuntimeSkipNavigation? FindDeclaredSkipNavigation(string name)
        => _skipNavigations != null && _skipNavigations.TryGetValue(name, out var navigation)
            ? navigation
            : null;

    private IEnumerable<RuntimeSkipNavigation> GetDeclaredSkipNavigations()
        => _skipNavigations?.Values ?? Enumerable.Empty<RuntimeSkipNavigation>();

    private IEnumerable<RuntimeSkipNavigation> GetDerivedSkipNavigations()
        => !HasDirectlyDerivedTypes
            ? Enumerable.Empty<RuntimeSkipNavigation>()
            : GetDerivedTypes().Cast<RuntimeEntityType>().SelectMany(et => et.GetDeclaredSkipNavigations());

    private IEnumerable<RuntimeSkipNavigation> GetSkipNavigations()
        => BaseType != null
            ? _skipNavigations == null
                ? BaseType.GetSkipNavigations()
                : BaseType.GetSkipNavigations().Concat(_skipNavigations.Values)
            : GetDeclaredSkipNavigations();

    private IEnumerable<RuntimeSkipNavigation> FindDerivedSkipNavigations(string name)
    {
        Check.NotNull(name, nameof(name));

        return !HasDirectlyDerivedTypes
            ? Enumerable.Empty<RuntimeSkipNavigation>()
            : (IEnumerable<RuntimeSkipNavigation>)GetDerivedTypes<RuntimeEntityType>()
                .Select(et => et.FindDeclaredSkipNavigation(name)).Where(n => n != null);
    }

    /// <summary>
    ///     Gets the skip navigations with the given name on this type, base types or derived types.
    /// </summary>
    /// <returns>Type skip navigations.</returns>
    public virtual IEnumerable<RuntimeSkipNavigation> FindSkipNavigationsInHierarchy(string name)
        => !HasDirectlyDerivedTypes
            ? ToEnumerable(FindSkipNavigation(name))
            : ToEnumerable(FindSkipNavigation(name)).Concat(FindDerivedSkipNavigations(name));

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
            (_namedIndexes ??= new(StringComparer.Ordinal)).Add(name, index);
        }
        else
        {
            _unnamedIndexes.Add(properties, index);
        }

        foreach (var property in properties)
        {
            if (property.Indexes == null)
            {
                property.Indexes = [index];
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
            : BaseType?.FindIndex(properties);

    /// <summary>
    ///     Gets the index with the given name. Returns <see langword="null" /> if no such index exists.
    /// </summary>
    /// <param name="name">The name of the index.</param>
    /// <returns>The index, or <see langword="null" /> if none is found.</returns>
    public virtual RuntimeIndex? FindIndex(string name)
        => _namedIndexes != null && _namedIndexes.TryGetValue(name, out var index)
            ? index
            : BaseType?.FindIndex(name);

    /// <summary>
    ///     Gets all indexes declared on this entity type.
    /// </summary>
    /// <remarks>
    ///     This method does not return indexes declared on base types.
    ///     It is useful when iterating over all entity types to avoid processing the same index more than once.
    ///     Use <see cref="GetForeignKeys" /> to also return indexes declared on base types.
    /// </remarks>
    /// <returns>Declared indexes.</returns>
    public virtual IEnumerable<RuntimeIndex> GetDeclaredIndexes()
        => _namedIndexes == null
            ? _unnamedIndexes.Values
            : _unnamedIndexes.Values.Concat(_namedIndexes.Values);

    private IEnumerable<RuntimeIndex> GetDerivedIndexes()
        => !HasDirectlyDerivedTypes
            ? Enumerable.Empty<RuntimeIndex>()
            : GetDerivedTypes().Cast<RuntimeEntityType>().SelectMany(et => et.GetDeclaredIndexes());

    /// <summary>
    ///     Gets the indexes defined on this entity type.
    /// </summary>
    /// <returns>The indexes defined on this entity type.</returns>
    public virtual IEnumerable<RuntimeIndex> GetIndexes()
        => BaseType != null
            ? _namedIndexes == null
                ? BaseType.GetIndexes()
                : BaseType.GetIndexes().Concat(GetDeclaredIndexes())
            : GetDeclaredIndexes();

    /// <summary>
    ///     Adds a service property to this entity type.
    /// </summary>
    /// <param name="name">The name of the property to add.</param>
    /// <param name="propertyInfo">The corresponding CLR property or <see langword="null" /> for a shadow property.</param>
    /// <param name="fieldInfo">The corresponding CLR field or <see langword="null" /> for a shadow property.</param>
    /// <param name="serviceType">The type of the service, or <see langword="null" /> to use the type of the member.</param>
    /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" /> used for this property.</param>
    /// <returns>The newly created service property.</returns>
    public virtual RuntimeServiceProperty AddServiceProperty(
        string name,
        PropertyInfo? propertyInfo = null,
        FieldInfo? fieldInfo = null,
        Type? serviceType = null,
        PropertyAccessMode propertyAccessMode = Internal.Model.DefaultPropertyAccessMode)
    {
        var serviceProperty = new RuntimeServiceProperty(
            name,
            propertyInfo,
            fieldInfo,
            serviceType ?? (propertyInfo?.PropertyType ?? fieldInfo?.FieldType)!,
            this,
            propertyAccessMode);

        (_serviceProperties ??= new(StringComparer.Ordinal))[serviceProperty.Name] = serviceProperty;

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
        => FindDeclaredServiceProperty(name) ?? BaseType?.FindServiceProperty(name);

    private RuntimeServiceProperty? FindDeclaredServiceProperty(string name)
        => _serviceProperties != null && _serviceProperties.TryGetValue(name, out var property)
            ? property
            : null;

    private bool HasServiceProperties()
        => _serviceProperties != null || BaseType != null && BaseType.HasServiceProperties();

    private IEnumerable<RuntimeServiceProperty> GetServiceProperties()
        => BaseType != null
            ? _serviceProperties != null
                ? BaseType.GetServiceProperties().Concat(_serviceProperties.Values)
                : BaseType.GetServiceProperties()
            : GetDeclaredServiceProperties();

    private IEnumerable<RuntimeServiceProperty> GetDeclaredServiceProperties()
        => _serviceProperties?.Values ?? Enumerable.Empty<RuntimeServiceProperty>();

    private IEnumerable<RuntimeServiceProperty> GetDerivedServiceProperties()
        => !HasDirectlyDerivedTypes
            ? Enumerable.Empty<RuntimeServiceProperty>()
            : GetDerivedTypes().Cast<RuntimeEntityType>().SelectMany(et => et.GetDeclaredServiceProperties());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private IEnumerable<RuntimeServiceProperty> FindDerivedServiceProperties(string propertyName)
    {
        Check.NotNull(propertyName, nameof(propertyName));

        return !HasDirectlyDerivedTypes
            ? Enumerable.Empty<RuntimeServiceProperty>()
            : (IEnumerable<RuntimeServiceProperty>)GetDerivedTypes<RuntimeEntityType>()
                .Select(et => et.FindDeclaredServiceProperty(propertyName))
                .Where(p => p != null);
    }

    /// <summary>
    ///     Gets the service properties with the given name on this type, base types or derived types.
    /// </summary>
    /// <returns>Type service properties.</returns>
    public virtual IEnumerable<RuntimeServiceProperty> FindServicePropertiesInHierarchy(string propertyName)
        => !HasDirectlyDerivedTypes
            ? ToEnumerable(FindServiceProperty(propertyName))
            : ToEnumerable(FindServiceProperty(propertyName)).Concat(FindDerivedServiceProperties(propertyName));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerable<RuntimePropertyBase> GetMembers()
        => GetProperties()
            .Concat<RuntimePropertyBase>(GetComplexProperties())
            .Concat(GetServiceProperties())
            .Concat(GetNavigations())
            .Concat(GetSkipNavigations());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerable<RuntimePropertyBase> GetDeclaredMembers()
        => GetDeclaredProperties()
            .Concat<RuntimePropertyBase>(GetDeclaredComplexProperties())
            .Concat(GetDeclaredServiceProperties())
            .Concat(GetDeclaredNavigations())
            .Concat(GetDeclaredSkipNavigations());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override RuntimePropertyBase? FindMember(string name)
        => FindProperty(name)
            ?? FindNavigation(name)
            ?? FindComplexProperty(name)
            ?? FindSkipNavigation(name)
            ?? ((RuntimePropertyBase?)FindServiceProperty(name));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerable<RuntimePropertyBase> FindMembersInHierarchy(string name)
        => FindPropertiesInHierarchy(name)
            .Concat<RuntimePropertyBase>(FindComplexPropertiesInHierarchy(name))
            .Concat(FindServicePropertiesInHierarchy(name))
            .Concat(FindNavigationsInHierarchy(name))
            .Concat(FindSkipNavigationsInHierarchy(name));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual RuntimeTrigger AddTrigger(string modelName)
    {
        var trigger = new RuntimeTrigger(this, modelName);

        (_triggers ??= new(StringComparer.Ordinal)).Add(modelName, trigger);

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

        return _triggers != null && _triggers.TryGetValue(modelName, out var trigger)
            ? trigger
            : null;
    }

    private IEnumerable<RuntimeTrigger> GetDeclaredTriggers()
        => _triggers?.Values ?? Enumerable.Empty<RuntimeTrigger>();

    private IEnumerable<RuntimeTrigger> GetTriggers()
        => BaseType != null
            ? BaseType.GetTriggers().Concat(GetDeclaredTriggers())
            : GetDeclaredTriggers();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetRelationshipSnapshotFactory(Func<InternalEntityEntry, ISnapshot> factory)
        => _relationshipSnapshotFactory = factory;

    /// <summary>
    ///     Gets or sets the <see cref="InstantiationBinding" /> for the preferred constructor.
    /// </summary>
    public override InstantiationBinding? ConstructorBinding
    {
        get => !base.ClrType.IsAbstract
            ? NonCapturingLazyInitializer.EnsureInitialized(
                ref _constructorBinding, this, entityType =>
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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual PropertyCounts Counts
    {
        get => NonCapturingLazyInitializer.EnsureInitialized(ref _counts, this, static entityType =>
            entityType.CalculateCounts());

        [DebuggerStepThrough]
        set => _counts = value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerable<RuntimePropertyBase> GetSnapshottableMembers()
    {
        return NonCapturingLazyInitializer.EnsureInitialized(
            ref _snapshottableProperties, this,
            static type => Create(type).ToArray());

        static IEnumerable<RuntimePropertyBase> Create(RuntimeEntityType type)
        {
            foreach (var property in type.GetProperties())
            {
                yield return property;
            }

            foreach (var complexProperty in type.GetComplexProperties())
            {
                yield return complexProperty;

                foreach (var property in complexProperty.ComplexType.GetSnapshottableMembers())
                {
                    yield return property;
                }
            }

            foreach (var navigation in type.GetNavigations())
            {
                yield return navigation;
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual Func<MaterializationContext, object> GetOrCreateMaterializer(IEntityMaterializerSource source)
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _materializer, this, source,
            static (e, s) => s.GetMaterializer(e));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual Func<MaterializationContext, object> GetOrCreateEmptyMaterializer(IEntityMaterializerSource source)
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _emptyMaterializer, this, source,
            static (e, s) => s.GetEmptyMaterializer(e));

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
    [DebuggerStepThrough]
    LambdaExpression? IReadOnlyEntityType.GetQueryFilter()
        => (LambdaExpression?)this[CoreAnnotationNames.QueryFilter];

    /// <inheritdoc />
    [DebuggerStepThrough]
    string? IReadOnlyEntityType.GetDiscriminatorPropertyName()
    {
        if (BaseType != null)
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
        get => BaseType;
    }

    /// <inheritdoc />
    IEntityType? IEntityType.BaseType
    {
        [DebuggerStepThrough]
        get => BaseType;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyEntityType> IReadOnlyEntityType.GetDerivedTypes()
        => GetDerivedTypes<RuntimeEntityType>();

    /// <inheritdoc />
    IEnumerable<IReadOnlyEntityType> IReadOnlyEntityType.GetDerivedTypesInclusive()
        => !HasDirectlyDerivedTypes
            ? new[] { this }
            : new[] { this }.Concat(GetDerivedTypes<RuntimeEntityType>());

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyEntityType> IReadOnlyEntityType.GetDirectlyDerivedTypes()
        => DirectlyDerivedTypes.Cast<RuntimeEntityType>();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IEntityType> IEntityType.GetDirectlyDerivedTypes()
        => DirectlyDerivedTypes.Cast<RuntimeEntityType>();

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
        => GetDeclaredForeignKeys();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IForeignKey> IEntityType.GetDeclaredForeignKeys()
        => GetDeclaredForeignKeys();

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
        => !HasDirectlyDerivedTypes
            ? Enumerable.Empty<RuntimeNavigation>()
            : GetDerivedTypes().Cast<RuntimeEntityType>().SelectMany(et => et.GetDeclaredNavigations());

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
    [DebuggerStepThrough]
    IProperty? IEntityType.FindProperty(string name) => FindProperty(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyList<IProperty>? IEntityType.FindProperties(IReadOnlyList<string> propertyNames)
        => FindProperties(propertyNames);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IProperty? IEntityType.FindDeclaredProperty(string name) => FindDeclaredProperty(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IProperty> IEntityType.GetDeclaredProperties() => GetDeclaredProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IProperty> IEntityType.GetProperties() => GetProperties();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IProperty> IEntityType.GetForeignKeyProperties()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _foreignKeyProperties, this,
            static entityType => entityType.GetProperties().Where(p => ((IReadOnlyProperty)p).IsForeignKey()).ToArray());

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IProperty> IEntityType.GetValueGeneratingProperties()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _valueGeneratingProperties, this,
            static entityType => entityType.GetProperties().Where(p => p.RequiresValueGenerator()).ToArray());

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

    PropertyAccessMode IReadOnlyEntityType.GetNavigationAccessMode()
        => throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetOriginalValuesFactory(Func<InternalEntityEntry, ISnapshot> factory)
        => _originalValuesFactory = factory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetStoreGeneratedValuesFactory(Func<ISnapshot> factory)
        => _storeGeneratedValuesFactory = factory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetTemporaryValuesFactory(Func<InternalEntityEntry, ISnapshot> factory)
        => _temporaryValuesFactory = factory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetEmptyShadowValuesFactory(Func<ISnapshot> factory)
        => _emptyShadowValuesFactory = factory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetShadowValuesFactory(Func<IDictionary<string, object?>, ISnapshot> factory)
        => _shadowValuesFactory = factory;

    /// <inheritdoc />
    Func<InternalEntityEntry, ISnapshot> IRuntimeEntityType.OriginalValuesFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _originalValuesFactory, this,
            static entityType => RuntimeFeature.IsDynamicCodeSupported
                ? OriginalValuesFactoryFactory.Instance.Create(entityType)
                : throw new InvalidOperationException(CoreStrings.NativeAotNoCompiledModel));

    /// <inheritdoc />
    Func<ISnapshot> IRuntimeEntityType.StoreGeneratedValuesFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _storeGeneratedValuesFactory, this,
            static entityType => RuntimeFeature.IsDynamicCodeSupported
                ? StoreGeneratedValuesFactoryFactory.Instance.CreateEmpty(entityType)
                : throw new InvalidOperationException(CoreStrings.NativeAotNoCompiledModel));

    /// <inheritdoc />
    Func<InternalEntityEntry, ISnapshot> IRuntimeEntityType.TemporaryValuesFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _temporaryValuesFactory, this,
            static entityType => RuntimeFeature.IsDynamicCodeSupported
                ? TemporaryValuesFactoryFactory.Instance.Create(entityType)
                : throw new InvalidOperationException(CoreStrings.NativeAotNoCompiledModel));

    /// <inheritdoc />
    Func<IDictionary<string, object?>, ISnapshot> IRuntimeEntityType.ShadowValuesFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _shadowValuesFactory, this,
            static entityType => RuntimeFeature.IsDynamicCodeSupported
                ? ShadowValuesFactoryFactory.Instance.Create(entityType)
                : throw new InvalidOperationException(CoreStrings.NativeAotNoCompiledModel));

    /// <inheritdoc />
    Func<ISnapshot> IRuntimeEntityType.EmptyShadowValuesFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _emptyShadowValuesFactory, this,
            static entityType => RuntimeFeature.IsDynamicCodeSupported
                ? EmptyShadowValuesFactoryFactory.Instance.CreateEmpty(entityType)
                : throw new InvalidOperationException(CoreStrings.NativeAotNoCompiledModel));

    /// <inheritdoc />
    Func<InternalEntityEntry, ISnapshot> IRuntimeEntityType.RelationshipSnapshotFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _relationshipSnapshotFactory, this,
            static entityType => RuntimeFeature.IsDynamicCodeSupported
                ? RelationshipSnapshotFactoryFactory.Instance.Create(entityType)
                : throw new InvalidOperationException(CoreStrings.NativeAotNoCompiledModel));
}
