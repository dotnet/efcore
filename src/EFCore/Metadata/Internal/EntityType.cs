// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class EntityType : TypeBase, IMutableEntityType, IConventionEntityType, IRuntimeEntityType
{
    internal const string DynamicProxyGenAssemblyName = "DynamicProxyGenAssembly2";

    private readonly SortedSet<ForeignKey> _foreignKeys
        = new(ForeignKeyComparer.Instance);

    private readonly SortedDictionary<string, Navigation> _navigations
        = new(StringComparer.Ordinal);

    private readonly SortedDictionary<string, SkipNavigation> _skipNavigations
        = new(StringComparer.Ordinal);

    private readonly SortedDictionary<string, ServiceProperty> _serviceProperties
        = new(StringComparer.Ordinal);

    private readonly SortedDictionary<IReadOnlyList<IReadOnlyProperty>, Index> _unnamedIndexes
        = new(PropertyListComparer.Instance);

    private readonly SortedDictionary<string, Index> _namedIndexes
        = new(StringComparer.Ordinal);

    private readonly SortedDictionary<IReadOnlyList<IReadOnlyProperty>, Key> _keys
        = new(PropertyListComparer.Instance);

    private readonly SortedDictionary<string, Trigger> _triggers
        = new(StringComparer.Ordinal);

    private List<object>? _data;
    private Key? _primaryKey;
    private bool? _isKeyless;
    private bool _isOwned;
    private InternalEntityTypeBuilder? _builder;

    private ConfigurationSource? _primaryKeyConfigurationSource;
    private ConfigurationSource? _isKeylessConfigurationSource;
    private ConfigurationSource? _baseTypeConfigurationSource;
    private ConfigurationSource? _constructorBindingConfigurationSource;
    private ConfigurationSource? _serviceOnlyConstructorBindingConfigurationSource;

    // Warning: Never access these fields directly as access needs to be thread-safe
    private PropertyCounts? _counts;

    // _serviceOnlyConstructorBinding needs to be set as well whenever _constructorBinding is set
    private InstantiationBinding? _constructorBinding;
    private InstantiationBinding? _serviceOnlyConstructorBinding;

    private Func<InternalEntityEntry, ISnapshot>? _relationshipSnapshotFactory;
    private Func<InternalEntityEntry, ISnapshot>? _originalValuesFactory;
    private Func<InternalEntityEntry, ISnapshot>? _temporaryValuesFactory;
    private Func<ISnapshot>? _storeGeneratedValuesFactory;
    private Func<IDictionary<string, object?>, ISnapshot>? _shadowValuesFactory;
    private Func<ISnapshot>? _emptyShadowValuesFactory;
    private IProperty[]? _foreignKeyProperties;
    private IProperty[]? _valueGeneratingProperties;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public EntityType(string name, Model model, bool owned, ConfigurationSource configurationSource)
        : base(name, Model.DefaultPropertyBagType, model, configurationSource)
    {
        _builder = new InternalEntityTypeBuilder(this, model.Builder);
        _isOwned = owned;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public EntityType(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        Model model,
        bool owned,
        ConfigurationSource configurationSource)
        : base(type, model, configurationSource)
    {
        if (!type.IsValidEntityType())
        {
            throw new ArgumentException(CoreStrings.InvalidEntityType(type));
        }

        if (DynamicProxyGenAssemblyName.Equals(
                type.Assembly.GetName().Name, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                CoreStrings.AddingProxyTypeAsEntityType(type.FullName));
        }

        _builder = new InternalEntityTypeBuilder(this, model.Builder);
        _isOwned = owned;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public EntityType(
        string name,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        Model model,
        bool owned,
        ConfigurationSource configurationSource)
        : base(name, type, model, configurationSource)
    {
        if (!type.IsValidEntityType())
        {
            throw new ArgumentException(CoreStrings.InvalidEntityType(type));
        }

        if (DynamicProxyGenAssemblyName.Equals(
                type.Assembly.GetName().Name, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                CoreStrings.AddingProxyTypeAsEntityType(type.FullName));
        }

        _builder = new InternalEntityTypeBuilder(this, model.Builder);
        _isOwned = owned;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public new virtual InternalEntityTypeBuilder Builder
    {
        [DebuggerStepThrough]
        get => _builder ?? throw new InvalidOperationException(CoreStrings.ObjectRemovedFromModel(DisplayName()));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override InternalTypeBaseBuilder BaseBuilder
    {
        [DebuggerStepThrough]
        get => Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool IsInModel
        => _builder is not null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetRemovedFromModel()
    {
        if (_foreignKeys.Count > 0)
        {
            foreach (var foreignKey in GetDeclaredForeignKeys().ToList())
            {
                if (foreignKey.PrincipalEntityType != this)
                {
                    RemoveForeignKey(foreignKey);
                }
            }
        }

        if (_skipNavigations.Count > 0)
        {
            foreach (var skipNavigation in GetDeclaredSkipNavigations().ToList())
            {
                if (skipNavigation.TargetEntityType != this)
                {
                    RemoveSkipNavigation(skipNavigation);
                }
            }
        }

        foreach (var property in Properties.Values)
        {
            Model.RemoveProperty(property);
        }

        _builder = null;
        BaseType?.DirectlyDerivedTypes.Remove(this);

        Model.ConventionDispatcher.OnEntityTypeRemoved(Model.Builder, this);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public new virtual EntityType? BaseType
        => (EntityType?)base.BaseType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsKeyless
    {
        get => GetRootType()._isKeyless ?? false;
        set => SetIsKeyless(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsOwned()
        => _isOwned;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetIsOwned(bool value)
        => _isOwned = value;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual EntityType? Owner
        => FindOwnership()?.PrincipalEntityType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private string DisplayName()
        => ((IReadOnlyEntityType)this).DisplayName();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? SetIsKeyless(bool? keyless, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        if (_isKeyless == keyless)
        {
            UpdateIsKeylessConfigurationSource(configurationSource);
            return keyless;
        }

        if (keyless == true)
        {
            if (BaseType != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.DerivedEntityTypeHasNoKey(DisplayName(), GetRootType().DisplayName()));
            }

            if (_keys.Count != 0)
            {
                throw new InvalidOperationException(
                    CoreStrings.KeylessTypeExistingKey(
                        DisplayName(), _keys.First().Value.Properties.Format()));
            }
        }

        _isKeyless = keyless;

        if (keyless == null)
        {
            _isKeylessConfigurationSource = null;
        }
        else
        {
            UpdateIsKeylessConfigurationSource(configurationSource);
        }

        return keyless;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetIsKeylessConfigurationSource()
        => _isKeylessConfigurationSource;

    private void UpdateIsKeylessConfigurationSource(ConfigurationSource configurationSource)
        => _isKeylessConfigurationSource = configurationSource.Max(_isKeylessConfigurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual EntityType? SetBaseType(EntityType? newBaseType, ConfigurationSource configurationSource)
    {
        EnsureMutable();
        Check.DebugAssert(IsInModel, "The entity type has been removed from the model");

        if (BaseType == newBaseType)
        {
            UpdateBaseTypeConfigurationSource(configurationSource);
            newBaseType?.UpdateConfigurationSource(configurationSource);
            return newBaseType;
        }

        var originalBaseType = BaseType;

        BaseType?.DirectlyDerivedTypes.Remove(this);
        base.BaseType = null;

        if (newBaseType != null)
        {
            if (!newBaseType.ClrType.IsAssignableFrom(ClrType))
            {
                throw new InvalidOperationException(
                    CoreStrings.NotAssignableClrBaseType(
                        DisplayName(), newBaseType.DisplayName(), ClrType.ShortDisplayName(),
                        newBaseType.ClrType.ShortDisplayName()));
            }

            if (newBaseType.InheritsFrom(this))
            {
                throw new InvalidOperationException(CoreStrings.CircularInheritance(DisplayName(), newBaseType.DisplayName()));
            }

            if (_keys.Count > 0)
            {
                throw new InvalidOperationException(CoreStrings.DerivedEntityCannotHaveKeys(DisplayName()));
            }

            if (IsKeyless)
            {
                throw new InvalidOperationException(CoreStrings.DerivedEntityCannotBeKeyless(DisplayName()));
            }

            if (IsOwned() != newBaseType.IsOwned())
            {
                throw new InvalidOperationException(
                    CoreStrings.DerivedEntityOwnershipMismatch(
                        newBaseType.DisplayName(),
                        DisplayName(),
                        IsOwned() ? DisplayName() : newBaseType.DisplayName(),
                        !IsOwned() ? DisplayName() : newBaseType.DisplayName()));
            }

            var conflictingMember = newBaseType.GetMembers()
                .Select(p => p.Name)
                .SelectMany(FindMembersInHierarchy)
                .FirstOrDefault();

            if (conflictingMember != null)
            {
                var baseMember = newBaseType.FindMembersInHierarchy(conflictingMember.Name).Single();
                throw new InvalidOperationException(
                    CoreStrings.DuplicatePropertiesOnBase(
                        DisplayName(),
                        newBaseType.DisplayName(),
                        conflictingMember.DeclaringType.DisplayName(),
                        conflictingMember.Name,
                        baseMember.DeclaringType.DisplayName(),
                        baseMember.Name));
            }

            base.BaseType = newBaseType;
            newBaseType.DirectlyDerivedTypes.Add(this);
        }

        UpdateBaseTypeConfigurationSource(configurationSource);
        newBaseType?.UpdateConfigurationSource(configurationSource);

        return (EntityType?)Model.ConventionDispatcher.OnEntityTypeBaseTypeChanged(Builder, newBaseType, originalBaseType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    public virtual ConfigurationSource? GetBaseTypeConfigurationSource()
        => _baseTypeConfigurationSource;

    [DebuggerStepThrough]
    private void UpdateBaseTypeConfigurationSource(ConfigurationSource configurationSource)
        => _baseTypeConfigurationSource = configurationSource.Max(_baseTypeConfigurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    public virtual IEnumerable<EntityType> GetDirectlyDerivedTypes()
        => DirectlyDerivedTypes.Cast<EntityType>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    public virtual IEnumerable<ForeignKey> GetForeignKeysInHierarchy()
        => DirectlyDerivedTypes.Count == 0
            ? GetForeignKeys()
            : GetForeignKeys().Concat(GetDerivedForeignKeys());

    private bool InheritsFrom(EntityType entityType)
    {
        var et = this;

        do
        {
            if (entityType == et)
            {
                return true;
            }
        }
        while ((et = et.BaseType) != null);

        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    public virtual EntityType GetRootType()
        => (EntityType)((IReadOnlyEntityType)this).GetRootType();

    /// <summary>
    ///     Runs the conventions when an annotation was set or removed.
    /// </summary>
    /// <param name="name">The key of the set annotation.</param>
    /// <param name="annotation">The annotation set.</param>
    /// <param name="oldAnnotation">The old annotation.</param>
    /// <returns>The annotation that was set.</returns>
    protected override IConventionAnnotation? OnAnnotationSet(
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
        => Model.ConventionDispatcher.OnEntityTypeAnnotationChanged(Builder, name, annotation, oldAnnotation);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerable<PropertyBase> GetMembers()
        => GetProperties()
            .Concat<PropertyBase>(GetComplexProperties())
            .Concat(GetServiceProperties())
            .Concat(GetNavigations())
            .Concat(GetSkipNavigations());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerable<PropertyBase> GetDeclaredMembers()
        => GetDeclaredProperties()
            .Concat<PropertyBase>(GetDeclaredComplexProperties())
            .Concat(GetDeclaredServiceProperties())
            .Concat(GetDeclaredNavigations())
            .Concat(GetDeclaredSkipNavigations());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override PropertyBase? FindMember(string name)
        => FindProperty(name)
            ?? FindNavigation(name)
            ?? FindComplexProperty(name)
            ?? FindSkipNavigation(name)
            ?? ((PropertyBase?)FindServiceProperty(name));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerable<PropertyBase> FindMembersInHierarchy(string name)
        => FindPropertiesInHierarchy(name)
            .Concat<PropertyBase>(FindComplexPropertiesInHierarchy(name))
            .Concat(FindServicePropertiesInHierarchy(name))
            .Concat(FindNavigationsInHierarchy(name))
            .Concat(FindSkipNavigationsInHierarchy(name));

    #region Primary and Candidate Keys

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Key? SetPrimaryKey(Property? property, ConfigurationSource configurationSource)
        => SetPrimaryKey(
            property == null
                ? null
                : new[] { property }, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Key? SetPrimaryKey(
        IReadOnlyList<Property>? properties,
        ConfigurationSource configurationSource)
    {
        EnsureMutable();
        Check.DebugAssert(IsInModel, "The entity type has been removed from the model");

        if (BaseType != null)
        {
            throw new InvalidOperationException(CoreStrings.DerivedEntityTypeKey(DisplayName(), GetRootType().DisplayName()));
        }

        var oldPrimaryKey = _primaryKey;
        if (oldPrimaryKey == null && (properties is null || properties.Count == 0))
        {
            return null;
        }

        Key? newKey = null;
        if (properties?.Count > 0)
        {
            newKey = FindKey(properties);
            if (oldPrimaryKey != null
                && oldPrimaryKey == newKey)
            {
                UpdatePrimaryKeyConfigurationSource(configurationSource);
                newKey.UpdateConfigurationSource(configurationSource);
                return newKey;
            }

            newKey ??= AddKey(properties, configurationSource);
        }

        if (oldPrimaryKey != null)
        {
            foreach (var property in oldPrimaryKey.Properties)
            {
                Properties.Remove(property.Name);
                property.PrimaryKey = null;
            }

            _primaryKey = null;

            foreach (var property in oldPrimaryKey.Properties)
            {
                Properties.Add(property.Name, property);
            }
        }

        if (properties?.Count > 0 && newKey != null)
        {
            foreach (var property in newKey.Properties)
            {
                Properties.Remove(property.Name);
                property.PrimaryKey = newKey;
            }

            _primaryKey = newKey;

            foreach (var property in newKey.Properties)
            {
                Properties.Add(property.Name, property);
            }

            UpdatePrimaryKeyConfigurationSource(configurationSource);
        }
        else
        {
            SetPrimaryKeyConfigurationSource(null);
        }

        return (Key?)Model.ConventionDispatcher.OnPrimaryKeyChanged(Builder, newKey, oldPrimaryKey);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Key? FindPrimaryKey()
        => BaseType?.FindPrimaryKey() ?? _primaryKey;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Key? FindPrimaryKey(IReadOnlyList<Property>? properties)
    {
        Check.HasNoNulls(properties, nameof(properties));
        Check.NotEmpty(properties, nameof(properties));

        if (BaseType != null)
        {
            return BaseType.FindPrimaryKey(properties);
        }

        return _primaryKey != null
            && PropertyListComparer.Instance.Compare(_primaryKey.Properties, properties) == 0
                ? _primaryKey
                : null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetPrimaryKeyConfigurationSource()
        => _primaryKeyConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private void SetPrimaryKeyConfigurationSource(ConfigurationSource? configurationSource)
        => _primaryKeyConfigurationSource = configurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private void UpdatePrimaryKeyConfigurationSource(ConfigurationSource configurationSource)
        => _primaryKeyConfigurationSource = configurationSource.Max(_primaryKeyConfigurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Key? AddKey(Property property, ConfigurationSource configurationSource)
        => AddKey(
            new[] { property }, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Key? AddKey(
        IReadOnlyList<Property> properties,
        ConfigurationSource configurationSource)
    {
        Check.NotEmpty(properties, nameof(properties));
        Check.HasNoNulls(properties, nameof(properties));
        EnsureMutable();

        if (BaseType != null)
        {
            throw new InvalidOperationException(CoreStrings.DerivedEntityTypeKey(DisplayName(), BaseType.DisplayName()));
        }

        if (IsKeyless)
        {
            throw new InvalidOperationException(CoreStrings.KeylessTypeWithKey(properties.Format(), DisplayName()));
        }

        for (var i = 0; i < properties.Count; i++)
        {
            var property = properties[i];
            for (var j = i + 1; j < properties.Count; j++)
            {
                if (property == properties[j])
                {
                    throw new InvalidOperationException(CoreStrings.DuplicatePropertyInKey(properties.Format(), property.Name));
                }
            }

            if (FindProperty(property.Name) != property
                || !property.IsInModel)
            {
                throw new InvalidOperationException(CoreStrings.KeyPropertiesWrongEntity(properties.Format(), DisplayName()));
            }

            if (property.IsNullable)
            {
                throw new InvalidOperationException(CoreStrings.NullableKey(DisplayName(), property.Name));
            }
        }

        var key = FindKey(properties);
        if (key != null)
        {
            throw new InvalidOperationException(
                CoreStrings.DuplicateKey(
                    properties.Format(), DisplayName(), key.DeclaringEntityType.DisplayName()));
        }

        key = new Key(properties, configurationSource);
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

        return (Key?)Model.ConventionDispatcher.OnKeyAdded(key.Builder)?.Metadata;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Key? FindKey(IReadOnlyProperty property)
        => FindKey(new[] { property });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Key? FindKey(IReadOnlyList<IReadOnlyProperty> properties)
    {
        Check.HasNoNulls(properties, nameof(properties));
        Check.NotEmpty(properties, nameof(properties));

        return FindDeclaredKey(properties) ?? BaseType?.FindKey(properties);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Key> GetDeclaredKeys()
        => _keys.Values;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Key? FindDeclaredKey(IReadOnlyList<IReadOnlyProperty> properties)
        => _keys.TryGetValue(Check.NotEmpty(properties, nameof(properties)), out var key)
            ? key
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Key? RemoveKey(IReadOnlyList<IReadOnlyProperty> properties)
    {
        Check.NotEmpty(properties, nameof(properties));

        var wrongEntityTypeProperty = properties.FirstOrDefault(p => !((EntityType)p.DeclaringType).IsAssignableFrom(this));
        if (wrongEntityTypeProperty != null)
        {
            throw new InvalidOperationException(
                CoreStrings.KeyWrongType(
                    properties.Format(), DisplayName(), wrongEntityTypeProperty.DeclaringType.DisplayName()));
        }

        var key = FindDeclaredKey(properties);
        return key == null
            ? null
            : RemoveKey(key);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Key? RemoveKey(Key key)
    {
        Check.NotNull(key, nameof(key));
        Check.DebugAssert(IsInModel, "The entity type has been removed from the model");
        EnsureMutable();

        if (key.DeclaringEntityType != this)
        {
            throw new InvalidOperationException(
                CoreStrings.KeyWrongType(key.Properties.Format(), DisplayName(), key.DeclaringEntityType.DisplayName()));
        }

        CheckKeyNotInUse(key);

        if (_primaryKey == key)
        {
            SetPrimaryKey((IReadOnlyList<Property>?)null, ConfigurationSource.Explicit);
            _primaryKeyConfigurationSource = null;
        }

        var removed = _keys.Remove(key.Properties);
        Check.DebugAssert(removed, "removed is false");
        key.SetRemovedFromModel();

        foreach (var property in key.Properties)
        {
            if (property.Keys != null)
            {
                property.Keys.Remove(key);
                if (property.Keys.Count == 0)
                {
                    property.Keys = null;
                }
            }
        }

        return (Key?)Model.ConventionDispatcher.OnKeyRemoved(Builder, key);
    }

    private void CheckKeyNotInUse(Key key)
    {
        var foreignKey = key.GetReferencingForeignKeys().FirstOrDefault();
        if (foreignKey != null)
        {
            throw new InvalidOperationException(
                CoreStrings.KeyInUse(
                    key.Properties.Format(),
                    DisplayName(),
                    foreignKey.Properties.Format(),
                    foreignKey.DeclaringEntityType.DisplayName()));
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Key> GetKeys()
        => BaseType?.GetKeys().Concat(_keys.Values) ?? _keys.Values;

    #endregion

    #region Foreign Keys

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ForeignKey? AddForeignKey(
        Property property,
        Key principalKey,
        EntityType principalEntityType,
        ConfigurationSource? componentConfigurationSource,
        ConfigurationSource configurationSource)
        => AddForeignKey(
            new[] { property }, principalKey, principalEntityType, componentConfigurationSource, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ForeignKey? AddForeignKey(
        IReadOnlyList<Property> properties,
        Key principalKey,
        EntityType principalEntityType,
        ConfigurationSource? componentConfigurationSource,
        ConfigurationSource configurationSource)
    {
        Check.NotEmpty(properties, nameof(properties));
        Check.HasNoNulls(properties, nameof(properties));
        Check.NotNull(principalKey, nameof(principalKey));
        Check.NotNull(principalEntityType, nameof(principalEntityType));
        EnsureMutable();

        var foreignKey = new ForeignKey(
            properties, principalKey, this, principalEntityType, configurationSource);

        principalEntityType.UpdateConfigurationSource(configurationSource);
        if (componentConfigurationSource.HasValue)
        {
            foreignKey.UpdatePropertiesConfigurationSource(componentConfigurationSource.Value);
            foreignKey.UpdatePrincipalKeyConfigurationSource(componentConfigurationSource.Value);
            foreignKey.UpdatePrincipalEndConfigurationSource(componentConfigurationSource.Value);
        }

        OnForeignKeyUpdated(foreignKey);

        return (ForeignKey?)Model.ConventionDispatcher.OnForeignKeyAdded(foreignKey.Builder)?.Metadata;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void OnForeignKeyUpdating(ForeignKey foreignKey)
    {
        var removed = _foreignKeys.Remove(foreignKey);
        Check.DebugAssert(removed, "removed is false");

        foreach (var property in foreignKey.Properties)
        {
            if (property.ForeignKeys != null)
            {
                property.ForeignKeys.Remove(foreignKey);
                if (property.ForeignKeys.Count == 0)
                {
                    property.ForeignKeys = null;
                }
            }
        }

        removed = foreignKey.PrincipalKey.ReferencingForeignKeys!.Remove(foreignKey);
        Check.DebugAssert(removed, "removed is false");
        removed = foreignKey.PrincipalEntityType.DeclaredReferencingForeignKeys!.Remove(foreignKey);
        Check.DebugAssert(removed, "removed is false");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void OnForeignKeyUpdated(ForeignKey foreignKey)
    {
        var added = _foreignKeys.Add(foreignKey);
        Check.DebugAssert(added, "added is false");

        foreach (var property in foreignKey.Properties)
        {
            if (property.ForeignKeys == null)
            {
                property.ForeignKeys = [foreignKey];
            }
            else
            {
                property.ForeignKeys.Add(foreignKey);
            }
        }

        var principalKey = foreignKey.PrincipalKey;
        if (principalKey.ReferencingForeignKeys == null)
        {
            principalKey.ReferencingForeignKeys = new SortedSet<ForeignKey>(ForeignKeyComparer.Instance) { foreignKey };
        }
        else
        {
            added = principalKey.ReferencingForeignKeys.Add(foreignKey);
            Check.DebugAssert(added, "added is false");
        }

        var principalEntityType = foreignKey.PrincipalEntityType;
        if (principalEntityType.DeclaredReferencingForeignKeys == null)
        {
            principalEntityType.DeclaredReferencingForeignKeys = new SortedSet<ForeignKey>(ForeignKeyComparer.Instance) { foreignKey };
        }
        else
        {
            added = principalEntityType.DeclaredReferencingForeignKeys.Add(foreignKey);
            Check.DebugAssert(added, "added is false");
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ForeignKey> FindForeignKeys(IReadOnlyProperty property)
        => FindForeignKeys(new[] { property });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ForeignKey> FindForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
    {
        Check.HasNoNulls(properties, nameof(properties));
        Check.NotEmpty(properties, nameof(properties));

        return BaseType != null
            ? _foreignKeys.Count == 0
                ? BaseType.FindForeignKeys(properties)
                : BaseType.FindForeignKeys(properties).Concat(FindDeclaredForeignKeys(properties))
            : FindDeclaredForeignKeys(properties);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ForeignKey? FindForeignKey(
        IReadOnlyProperty property,
        IReadOnlyKey principalKey,
        IReadOnlyEntityType principalEntityType)
        => FindForeignKey(
            new[] { property }, principalKey, principalEntityType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ForeignKey? FindForeignKey(
        IReadOnlyList<IReadOnlyProperty> properties,
        IReadOnlyKey principalKey,
        IReadOnlyEntityType principalEntityType)
    {
        Check.HasNoNulls(properties, nameof(properties));
        Check.NotEmpty(properties, nameof(properties));
        Check.NotNull(principalKey, nameof(principalKey));
        Check.NotNull(principalEntityType, nameof(principalEntityType));

        return FindDeclaredForeignKey(properties, principalKey, principalEntityType)
            ?? BaseType?.FindForeignKey(properties, principalKey, principalEntityType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ForeignKey? FindOwnership()
    {
        foreach (var foreignKey in GetForeignKeys())
        {
            if (foreignKey.IsOwnership)
            {
                return foreignKey;
            }
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ForeignKey? FindDeclaredOwnership()
    {
        foreach (var foreignKey in _foreignKeys)
        {
            if (foreignKey.IsOwnership)
            {
                return foreignKey;
            }
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ForeignKey> GetDeclaredForeignKeys()
        => _foreignKeys;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ForeignKey> GetDerivedForeignKeys()
        => DirectlyDerivedTypes.Count == 0
            ? Enumerable.Empty<ForeignKey>()
            : GetDerivedTypes<EntityType>().SelectMany(et => et._foreignKeys);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ForeignKey> GetForeignKeys()
        => BaseType != null
            ? _foreignKeys.Count == 0
                ? BaseType.GetForeignKeys()
                : BaseType.GetForeignKeys().Concat(_foreignKeys)
            : _foreignKeys;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ForeignKey> FindDeclaredForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
    {
        Check.NotEmpty(properties, nameof(properties));

        return _foreignKeys.Count == 0
            ? Enumerable.Empty<ForeignKey>()
            : _foreignKeys.Where(fk => PropertyListComparer.Instance.Equals(fk.Properties, properties));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ForeignKey? FindDeclaredForeignKey(
        IReadOnlyList<IReadOnlyProperty> properties,
        IReadOnlyKey principalKey,
        IReadOnlyEntityType principalEntityType)
    {
        Check.NotEmpty(properties, nameof(properties));
        Check.NotNull(principalKey, nameof(principalKey));
        Check.NotNull(principalEntityType, nameof(principalEntityType));

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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ForeignKey> FindDerivedForeignKeys(
        IReadOnlyList<IReadOnlyProperty> properties)
        => DirectlyDerivedTypes.Count == 0
            ? Enumerable.Empty<ForeignKey>()
            : GetDerivedTypes<EntityType>().SelectMany(et => et.FindDeclaredForeignKeys(properties));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ForeignKey> FindDerivedForeignKeys(
        IReadOnlyList<IReadOnlyProperty> properties,
        IReadOnlyKey principalKey,
        IReadOnlyEntityType principalEntityType)
        => DirectlyDerivedTypes.Count == 0
            ? Enumerable.Empty<ForeignKey>()
            : (IEnumerable<ForeignKey>)GetDerivedTypes<EntityType>()
                .Select(et => et.FindDeclaredForeignKey(properties, principalKey, principalEntityType))
                .Where(fk => fk != null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ForeignKey> FindForeignKeysInHierarchy(
        IReadOnlyList<IReadOnlyProperty> properties)
        => DirectlyDerivedTypes.Count == 0
            ? FindForeignKeys(properties)
            : FindForeignKeys(properties).Concat(FindDerivedForeignKeys(properties));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ForeignKey> FindForeignKeysInHierarchy(
        IReadOnlyList<IReadOnlyProperty> properties,
        IReadOnlyKey principalKey,
        IReadOnlyEntityType principalEntityType)
        => DirectlyDerivedTypes.Count == 0
            ? ToEnumerable(FindForeignKey(properties, principalKey, principalEntityType))
            : ToEnumerable(FindForeignKey(properties, principalKey, principalEntityType))
                .Concat(FindDerivedForeignKeys(properties, principalKey, principalEntityType));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ForeignKey? RemoveForeignKey(
        IReadOnlyList<IReadOnlyProperty> properties,
        IReadOnlyKey principalKey,
        IReadOnlyEntityType principalEntityType)
    {
        Check.NotEmpty(properties, nameof(properties));

        var foreignKey = FindDeclaredForeignKey(properties, principalKey, principalEntityType);
        return foreignKey == null
            ? null
            : RemoveForeignKey(foreignKey);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ForeignKey? RemoveForeignKey(ForeignKey foreignKey)
    {
        Check.NotNull(foreignKey, nameof(foreignKey));
        Check.DebugAssert(IsInModel, "The entity type has been removed from the model");
        EnsureMutable();

        if (foreignKey.DeclaringEntityType != this)
        {
            throw new InvalidOperationException(
                CoreStrings.ForeignKeyWrongType(
                    foreignKey.Properties.Format(),
                    foreignKey.PrincipalKey.Properties.Format(),
                    foreignKey.PrincipalEntityType.DisplayName(),
                    DisplayName(),
                    foreignKey.DeclaringEntityType.DisplayName()));
        }

        var referencingSkipNavigation = foreignKey.ReferencingSkipNavigations?.FirstOrDefault();
        if (referencingSkipNavigation != null)
        {
            throw new InvalidOperationException(
                CoreStrings.ForeignKeyInUseSkipNavigation(
                    foreignKey.Properties.Format(),
                    DisplayName(),
                    referencingSkipNavigation.Name,
                    referencingSkipNavigation.DeclaringEntityType.DisplayName()));
        }

        if (foreignKey.DependentToPrincipal != null)
        {
            foreignKey.DeclaringEntityType.RemoveNavigation(foreignKey.DependentToPrincipal.Name);
        }

        if (foreignKey.PrincipalToDependent != null)
        {
            foreignKey.PrincipalEntityType.RemoveNavigation(foreignKey.PrincipalToDependent.Name);
        }

        OnForeignKeyUpdating(foreignKey);

        foreignKey.SetRemovedFromModel();

        if (foreignKey.DependentToPrincipal != null)
        {
            foreignKey.DependentToPrincipal.SetRemovedFromModel();
            Model.ConventionDispatcher.OnNavigationRemoved(
                Builder,
                foreignKey.PrincipalEntityType.Builder,
                foreignKey.DependentToPrincipal.Name,
                foreignKey.DependentToPrincipal.GetIdentifyingMemberInfo());
        }

        if (foreignKey.PrincipalToDependent != null)
        {
            foreignKey.PrincipalToDependent.SetRemovedFromModel();
            Model.ConventionDispatcher.OnNavigationRemoved(
                foreignKey.PrincipalEntityType.Builder,
                Builder,
                foreignKey.PrincipalToDependent.Name,
                foreignKey.PrincipalToDependent.GetIdentifyingMemberInfo());
        }

        return (ForeignKey?)Model.ConventionDispatcher.OnForeignKeyRemoved(Builder, foreignKey);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ForeignKey> GetReferencingForeignKeys()
        => BaseType != null
            ? (DeclaredReferencingForeignKeys?.Count ?? 0) == 0
                ? BaseType.GetReferencingForeignKeys()
                : BaseType.GetReferencingForeignKeys().Concat(GetDeclaredReferencingForeignKeys())
            : GetDeclaredReferencingForeignKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ForeignKey> GetDeclaredReferencingForeignKeys()
        => DeclaredReferencingForeignKeys ?? Enumerable.Empty<ForeignKey>();

    private SortedSet<ForeignKey>? DeclaredReferencingForeignKeys { get; set; }

    #endregion

    #region Navigations

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Navigation AddNavigation(
        string name,
        ForeignKey foreignKey,
        bool pointsToPrincipal)
    {
        Check.NotEmpty(name, nameof(name));
        Check.NotNull(foreignKey, nameof(foreignKey));

        return AddNavigation(new MemberIdentity(name), foreignKey, pointsToPrincipal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Navigation AddNavigation(
        MemberInfo navigationMember,
        ForeignKey foreignKey,
        bool pointsToPrincipal)
    {
        Check.NotNull(navigationMember, nameof(navigationMember));
        Check.NotNull(foreignKey, nameof(foreignKey));

        return AddNavigation(new MemberIdentity(navigationMember), foreignKey, pointsToPrincipal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Navigation AddNavigation(MemberIdentity navigationMember, ForeignKey foreignKey, bool pointsToPrincipal)
    {
        EnsureMutable();

        var name = navigationMember.Name!;
        var duplicateNavigation = FindNavigationsInHierarchy(name).FirstOrDefault();
        if (duplicateNavigation != null)
        {
            if (duplicateNavigation.ForeignKey != foreignKey)
            {
                throw new InvalidOperationException(
                    CoreStrings.NavigationForWrongForeignKey(
                        duplicateNavigation.Name,
                        duplicateNavigation.DeclaringEntityType.DisplayName(),
                        foreignKey.Properties.Format(),
                        duplicateNavigation.ForeignKey.Properties.Format()));
            }

            throw new InvalidOperationException(
                CoreStrings.ConflictingPropertyOrNavigation(
                    name, DisplayName(), duplicateNavigation.DeclaringEntityType.DisplayName()));
        }

        var duplicateProperty = FindMembersInHierarchy(name).FirstOrDefault();
        if (duplicateProperty != null)
        {
            throw new InvalidOperationException(
                CoreStrings.ConflictingPropertyOrNavigation(
                    name, DisplayName(), ((IReadOnlyTypeBase)duplicateProperty.DeclaringType).DisplayName()));
        }

        Check.DebugAssert(
            !GetNavigations().Any(n => n.ForeignKey == foreignKey && n.IsOnDependent == pointsToPrincipal),
            "There is another navigation corresponding to the same foreign key and pointing in the same direction.");

        Check.DebugAssert(
            (pointsToPrincipal ? foreignKey.DeclaringEntityType : foreignKey.PrincipalEntityType) == this,
            "EntityType mismatch");

        var memberInfo = navigationMember.MemberInfo;
        if (memberInfo != null)
        {
            ValidateClrMember(name, memberInfo);
        }
        else if (!IsPropertyBag)
        {
            memberInfo = ClrType.GetMembersInHierarchy(name).FirstOrDefault();
        }

        if (memberInfo != null)
        {
            Navigation.IsCompatible(
                name,
                memberInfo,
                this,
                pointsToPrincipal ? foreignKey.PrincipalEntityType : foreignKey.DeclaringEntityType,
                !pointsToPrincipal && !foreignKey.IsUnique,
                shouldThrow: true);
        }
        else if (IsPropertyBag)
        {
            memberInfo = FindIndexerPropertyInfo()!;
        }

        var navigation = new Navigation(name, memberInfo as PropertyInfo, memberInfo as FieldInfo, foreignKey);

        _navigations.Add(name, navigation);

        return navigation;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Navigation? FindNavigation(string name)
        => (Navigation?)((IReadOnlyEntityType)this).FindNavigation(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Navigation? FindNavigation(MemberInfo memberInfo)
        => (Navigation?)((IReadOnlyEntityType)this).FindNavigation(Check.NotNull(memberInfo, nameof(memberInfo)).GetSimpleMemberName());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Navigation? FindDeclaredNavigation(string name)
        => _navigations.TryGetValue(Check.NotEmpty(name, nameof(name)), out var navigation)
            ? navigation
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Navigation> GetDeclaredNavigations()
        => _navigations.Values;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Navigation> GetDerivedNavigations()
        => DirectlyDerivedTypes.Count == 0
            ? Enumerable.Empty<Navigation>()
            : GetDerivedTypes<EntityType>().SelectMany(et => et.GetDeclaredNavigations());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Navigation> FindDerivedNavigations(string name)
    {
        Check.NotNull(name, nameof(name));

        return DirectlyDerivedTypes.Count == 0
            ? Enumerable.Empty<Navigation>()
            : (IEnumerable<Navigation>)GetDerivedTypes<EntityType>()
                .Select(et => et.FindDeclaredNavigation(name)).Where(n => n != null);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Navigation> FindNavigationsInHierarchy(string name)
        => DirectlyDerivedTypes.Count == 0
            ? ToEnumerable(FindNavigation(name))
            : ToEnumerable(FindNavigation(name)).Concat(FindDerivedNavigations(name));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Navigation? RemoveNavigation(string name)
    {
        Check.NotEmpty(name, nameof(name));
        EnsureMutable();

        var navigation = FindDeclaredNavigation(name);
        if (navigation == null)
        {
            return null;
        }

        _navigations.Remove(name);

        return navigation;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Navigation> GetNavigations()
        => BaseType != null
            ? _navigations.Count == 0 ? BaseType.GetNavigations() : BaseType.GetNavigations().Concat(_navigations.Values)
            : _navigations.Values;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SkipNavigation? AddSkipNavigation(
        string name,
        Type? navigationType,
        MemberInfo? memberInfo,
        EntityType targetEntityType,
        bool collection,
        bool onDependent,
        ConfigurationSource configurationSource)
    {
        Check.NotEmpty(name, nameof(name));
        Check.NotNull(targetEntityType, nameof(targetEntityType));
        EnsureMutable();

        var duplicateProperty = FindMembersInHierarchy(name).FirstOrDefault();
        if (duplicateProperty != null)
        {
            throw new InvalidOperationException(
                CoreStrings.ConflictingPropertyOrNavigation(
                    name, DisplayName(), duplicateProperty.DeclaringType.DisplayName()));
        }

        if (memberInfo != null)
        {
            ValidateClrMember(name, memberInfo);
        }
        else if (!IsPropertyBag)
        {
            memberInfo = ClrType.GetMembersInHierarchy(name).FirstOrDefault();
        }

        if (memberInfo != null)
        {
            Navigation.IsCompatible(
                name,
                memberInfo,
                this,
                targetEntityType,
                collection,
                shouldThrow: true);
        }
        else if (IsPropertyBag)
        {
            memberInfo = FindIndexerPropertyInfo()!;
        }

        var skipNavigation = new SkipNavigation(
            name,
            navigationType,
            memberInfo as PropertyInfo,
            memberInfo as FieldInfo,
            this,
            targetEntityType,
            collection,
            onDependent,
            configurationSource);

        _skipNavigations.Add(name, skipNavigation);

        if (targetEntityType.DeclaredReferencingSkipNavigations == null)
        {
            targetEntityType.DeclaredReferencingSkipNavigations =
                new SortedSet<SkipNavigation>(SkipNavigationComparer.Instance) { skipNavigation };
        }
        else
        {
            var added = targetEntityType.DeclaredReferencingSkipNavigations.Add(skipNavigation);
            Check.DebugAssert(added, "added is false");
        }

        return (SkipNavigation?)Model.ConventionDispatcher.OnSkipNavigationAdded(skipNavigation.Builder)?.Metadata;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SkipNavigation? FindSkipNavigation(string name)
    {
        Check.NotEmpty(name, nameof(name));

        return FindDeclaredSkipNavigation(name) ?? BaseType?.FindSkipNavigation(name);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SkipNavigation? FindSkipNavigation(MemberInfo memberInfo)
        => FindSkipNavigation(Check.NotNull(memberInfo, nameof(memberInfo)).GetSimpleMemberName());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SkipNavigation? FindDeclaredSkipNavigation(string name)
        => _skipNavigations.TryGetValue(Check.NotEmpty(name, nameof(name)), out var navigation)
            ? navigation
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<SkipNavigation> GetDeclaredSkipNavigations()
        => _skipNavigations.Values;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<SkipNavigation> GetDerivedSkipNavigations()
        => DirectlyDerivedTypes.Count == 0
            ? Enumerable.Empty<SkipNavigation>()
            : GetDerivedTypes<EntityType>().SelectMany(et => et.GetDeclaredSkipNavigations());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<SkipNavigation> FindDerivedSkipNavigations(string name)
    {
        Check.NotNull(name, nameof(name));

        return DirectlyDerivedTypes.Count == 0
            ? Enumerable.Empty<SkipNavigation>()
            : (IEnumerable<SkipNavigation>)GetDerivedTypes<EntityType>()
                .Select(et => et.FindDeclaredSkipNavigation(name)).Where(n => n != null);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<SkipNavigation> FindDerivedSkipNavigationsInclusive(string name)
        => DirectlyDerivedTypes.Count == 0
            ? ToEnumerable(FindDeclaredSkipNavigation(name))
            : ToEnumerable(FindDeclaredSkipNavigation(name)).Concat(FindDerivedSkipNavigations(name));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<SkipNavigation> FindSkipNavigationsInHierarchy(string name)
        => DirectlyDerivedTypes.Count == 0
            ? ToEnumerable(FindSkipNavigation(name))
            : ToEnumerable(FindSkipNavigation(name)).Concat(FindDerivedSkipNavigations(name));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SkipNavigation? RemoveSkipNavigation(string name)
    {
        Check.NotEmpty(name, nameof(name));

        var navigation = FindDeclaredSkipNavigation(name);
        return navigation == null ? null : RemoveSkipNavigation(navigation);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SkipNavigation? RemoveSkipNavigation(SkipNavigation navigation)
    {
        Check.NotNull(navigation, nameof(navigation));
        Check.DebugAssert(IsInModel, "The entity type has been removed from the model");
        EnsureMutable();

        if (navigation.DeclaringEntityType != this)
        {
            throw new InvalidOperationException(
                CoreStrings.SkipNavigationWrongType(
                    navigation.Name, DisplayName(), navigation.DeclaringEntityType.DisplayName()));
        }

        if (navigation.Inverse?.Inverse == navigation)
        {
            throw new InvalidOperationException(
                CoreStrings.SkipNavigationInUseBySkipNavigation(
                    navigation.DeclaringEntityType.DisplayName(),
                    navigation.Name,
                    navigation.Inverse.DeclaringEntityType.DisplayName(),
                    navigation.Inverse.Name));
        }

        var removed = _skipNavigations.Remove(navigation.Name);
        Check.DebugAssert(removed, "Expected the navigation to be removed");

        removed = navigation.ForeignKey is not ForeignKey foreignKey
            || foreignKey.ReferencingSkipNavigations!.Remove(navigation);
        Check.DebugAssert(removed, "removed is false");

        removed = navigation.TargetEntityType.DeclaredReferencingSkipNavigations!.Remove(navigation);
        Check.DebugAssert(removed, "removed is false");

        navigation.SetRemovedFromModel();

        return (SkipNavigation?)Model.ConventionDispatcher.OnSkipNavigationRemoved(Builder, navigation);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<SkipNavigation> GetSkipNavigations()
        => BaseType != null
            ? _skipNavigations.Count == 0
                ? BaseType.GetSkipNavigations()
                : BaseType.GetSkipNavigations().Concat(_skipNavigations.Values)
            : _skipNavigations.Values;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<SkipNavigation> GetReferencingSkipNavigations()
        => BaseType != null
            ? (DeclaredReferencingSkipNavigations?.Count ?? 0) == 0
                ? BaseType.GetReferencingSkipNavigations()
                : BaseType.GetReferencingSkipNavigations().Concat(GetDeclaredReferencingSkipNavigations())
            : GetDeclaredReferencingSkipNavigations();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<SkipNavigation> GetDeclaredReferencingSkipNavigations()
        => DeclaredReferencingSkipNavigations ?? Enumerable.Empty<SkipNavigation>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<SkipNavigation> GetDerivedReferencingSkipNavigations()
        => DirectlyDerivedTypes.Count == 0
            ? Enumerable.Empty<SkipNavigation>()
            : GetDerivedTypes<EntityType>().SelectMany(et => et.GetDeclaredReferencingSkipNavigations());

    private SortedSet<SkipNavigation>? DeclaredReferencingSkipNavigations { get; set; }

    #endregion

    #region Indexes

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Index? AddIndex(
        Property property,
        ConfigurationSource configurationSource)
        => AddIndex(new[] { property }, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Index? AddIndex(
        Property property,
        string name,
        ConfigurationSource configurationSource)
        => AddIndex(new[] { property }, name, configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Index? AddIndex(
        IReadOnlyList<Property> properties,
        ConfigurationSource configurationSource)
    {
        Check.NotEmpty(properties, nameof(properties));
        Check.HasNoNulls(properties, nameof(properties));
        EnsureMutable();

        CheckIndexProperties(properties);

        var duplicateIndex = FindIndexesInHierarchy(properties).FirstOrDefault();
        if (duplicateIndex != null)
        {
            throw new InvalidOperationException(
                CoreStrings.DuplicateIndex(properties.Format(), DisplayName(), duplicateIndex.DeclaringEntityType.DisplayName()));
        }

        var index = new Index(properties, this, configurationSource);
        _unnamedIndexes.Add(properties, index);

        UpdatePropertyIndexes(properties, index);

        return (Index?)Model.ConventionDispatcher.OnIndexAdded(index.Builder)?.Metadata;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Index? AddIndex(
        IReadOnlyList<Property> properties,
        string name,
        ConfigurationSource configurationSource)
    {
        Check.NotEmpty(properties, nameof(properties));
        Check.HasNoNulls(properties, nameof(properties));
        Check.NotEmpty(name, nameof(name));
        EnsureMutable();

        CheckIndexProperties(properties);

        var duplicateIndex = FindIndexesInHierarchy(name).FirstOrDefault();
        if (duplicateIndex != null)
        {
            throw new InvalidOperationException(
                CoreStrings.DuplicateNamedIndex(
                    name,
                    properties.Format(),
                    DisplayName(),
                    duplicateIndex.DeclaringEntityType.DisplayName()));
        }

        var index = new Index(properties, name, this, configurationSource);
        _namedIndexes.Add(name, index);

        UpdatePropertyIndexes(properties, index);

        return (Index?)Model.ConventionDispatcher.OnIndexAdded(index.Builder)?.Metadata;
    }

    private void CheckIndexProperties(IReadOnlyList<Property> properties)
    {
        for (var i = 0; i < properties.Count; i++)
        {
            var property = properties[i];
            for (var j = i + 1; j < properties.Count; j++)
            {
                if (property == properties[j])
                {
                    throw new InvalidOperationException(CoreStrings.DuplicatePropertyInIndex(properties.Format(), property.Name));
                }
            }

            if (FindProperty(property.Name) != property
                || !property.IsInModel)
            {
                throw new InvalidOperationException(CoreStrings.IndexPropertiesWrongEntity(properties.Format(), DisplayName()));
            }
        }
    }

    private static void UpdatePropertyIndexes(IReadOnlyList<Property> properties, Index index)
    {
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
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Index? FindIndex(IReadOnlyProperty property)
        => FindIndex(new[] { property });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Index? FindIndex(IReadOnlyList<IReadOnlyProperty> properties)
    {
        Check.HasNoNulls(properties, nameof(properties));
        Check.NotEmpty(properties, nameof(properties));

        return FindDeclaredIndex(properties) ?? BaseType?.FindIndex(properties);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Index? FindIndex(string name)
    {
        Check.NotEmpty(name, nameof(name));

        return FindDeclaredIndex(name) ?? BaseType?.FindIndex(name);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Index> GetDeclaredIndexes()
        => _namedIndexes.Count == 0
            ? _unnamedIndexes.Values
            : _unnamedIndexes.Values.Concat(_namedIndexes.Values);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Index> GetDerivedIndexes()
        => DirectlyDerivedTypes.Count == 0
            ? Enumerable.Empty<Index>()
            : GetDerivedTypes<EntityType>().SelectMany(et => et.GetDeclaredIndexes());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Index? FindDeclaredIndex(IReadOnlyList<IReadOnlyProperty> properties)
        => _unnamedIndexes.TryGetValue(Check.NotEmpty(properties, nameof(properties)), out var index)
            ? index
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Index? FindDeclaredIndex(string name)
        => _namedIndexes.TryGetValue(Check.NotEmpty(name, nameof(name)), out var index)
            ? index
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Index> FindDerivedIndexes(IReadOnlyList<IReadOnlyProperty> properties)
        => DirectlyDerivedTypes.Count == 0
            ? Enumerable.Empty<Index>()
            : (IEnumerable<Index>)GetDerivedTypes<EntityType>()
                .Select(et => et.FindDeclaredIndex(properties)).Where(i => i != null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Index> FindDerivedIndexes(string name)
        => DirectlyDerivedTypes.Count == 0
            ? Enumerable.Empty<Index>()
            : (IEnumerable<Index>)GetDerivedTypes<EntityType>()
                .Select(et => et.FindDeclaredIndex(Check.NotEmpty(name, nameof(name))))
                .Where(i => i != null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Index> FindIndexesInHierarchy(IReadOnlyList<IReadOnlyProperty> properties)
        => DirectlyDerivedTypes.Count == 0
            ? ToEnumerable(FindIndex(properties))
            : ToEnumerable(FindIndex(properties)).Concat(FindDerivedIndexes(properties));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Index> FindIndexesInHierarchy(string name)
        => DirectlyDerivedTypes.Count == 0
            ? ToEnumerable(FindIndex(Check.NotEmpty(name, nameof(name))))
            : ToEnumerable(FindIndex(Check.NotEmpty(name, nameof(name)))).Concat(FindDerivedIndexes(name));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Index? RemoveIndex(IReadOnlyList<IReadOnlyProperty> properties)
    {
        Check.NotEmpty(properties, nameof(properties));

        var index = FindDeclaredIndex(properties);
        return index == null
            ? null
            : RemoveIndex(index);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Index? RemoveIndex(string name)
    {
        Check.NotEmpty(name, nameof(name));

        var index = FindDeclaredIndex(name);
        return index == null
            ? null
            : RemoveIndex(index);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Index? RemoveIndex(Index index)
    {
        Check.NotNull(index, nameof(index));
        Check.DebugAssert(IsInModel, "The entity type has been removed from the model");
        EnsureMutable();

        if (index.Name == null)
        {
            if (!_unnamedIndexes.Remove(index.Properties))
            {
                throw new InvalidOperationException(
                    CoreStrings.IndexWrongType(index.DisplayName(), DisplayName(), index.DeclaringEntityType.DisplayName()));
            }
        }
        else
        {
            if (!_namedIndexes.Remove(index.Name))
            {
                throw new InvalidOperationException(
                    CoreStrings.NamedIndexWrongType(index.Name, DisplayName()));
            }
        }

        index.SetRemovedFromModel();

        foreach (var property in index.Properties)
        {
            if (property.Indexes != null)
            {
                property.Indexes.Remove(index);
                if (property.Indexes.Count == 0)
                {
                    property.Indexes = null;
                }
            }
        }

        return (Index?)Model.ConventionDispatcher.OnIndexRemoved(Builder, index);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Index> GetIndexes()
        => BaseType != null
            ? _namedIndexes.Count == 0 && _unnamedIndexes.Count == 0
                ? BaseType.GetIndexes()
                : BaseType.GetIndexes().Concat(GetDeclaredIndexes())
            : GetDeclaredIndexes();

    #endregion

    #region Lazy runtime logic

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PropertyCounts Counts
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _counts, this, static entityType =>
            {
                entityType.EnsureReadOnly();
                return entityType.CalculateCounts();
            });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerable<PropertyBase> GetSnapshottableMembers()
        => base.GetSnapshottableMembers().Concat(GetNavigations());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<InternalEntityEntry, ISnapshot> RelationshipSnapshotFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _relationshipSnapshotFactory, this,
            static entityType =>
            {
                entityType.EnsureReadOnly();
                return RelationshipSnapshotFactoryFactory.Instance.Create(entityType);
            });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<InternalEntityEntry, ISnapshot> OriginalValuesFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _originalValuesFactory, this,
            static entityType =>
            {
                entityType.EnsureReadOnly();
                return OriginalValuesFactoryFactory.Instance.Create(entityType);
            });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<ISnapshot> StoreGeneratedValuesFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _storeGeneratedValuesFactory, this,
            static entityType =>
            {
                entityType.EnsureReadOnly();
                return StoreGeneratedValuesFactoryFactory.Instance.CreateEmpty(entityType);
            });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<InternalEntityEntry, ISnapshot> TemporaryValuesFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _temporaryValuesFactory, this,
            static entityType =>
            {
                entityType.EnsureReadOnly();
                return TemporaryValuesFactoryFactory.Instance.Create(entityType);
            });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<IDictionary<string, object?>, ISnapshot> ShadowValuesFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _shadowValuesFactory, this,
            static entityType =>
            {
                entityType.EnsureReadOnly();
                return ShadowValuesFactoryFactory.Instance.Create(entityType);
            });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<ISnapshot> EmptyShadowValuesFactory
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _emptyShadowValuesFactory, this,
            static entityType =>
            {
                entityType.EnsureReadOnly();
                return EmptyShadowValuesFactoryFactory.Instance.CreateEmpty(entityType);
            });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<IProperty> ForeignKeyProperties
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _foreignKeyProperties, this,
            static entityType =>
            {
                entityType.EnsureReadOnly();

                return entityType.GetProperties().Where(p => p.IsForeignKey()).ToArray();
            });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<IProperty> ValueGeneratingProperties
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _valueGeneratingProperties, this,
            static entityType =>
            {
                entityType.EnsureReadOnly();

                return entityType.GetProperties().Where(p => p.RequiresValueGenerator()).ToArray();
            });

    #endregion

    #region Service properties

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ServiceProperty AddServiceProperty(
        MemberInfo memberInfo,
        Type serviceType,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        ConfigurationSource configurationSource)
    {
        Check.NotNull(memberInfo, nameof(memberInfo));
        EnsureMutable();

        var name = memberInfo.GetSimpleMemberName();
        var duplicateMember = FindMembersInHierarchy(name).FirstOrDefault();
        if (duplicateMember != null)
        {
            throw new InvalidOperationException(
                CoreStrings.ConflictingPropertyOrNavigation(
                    name, DisplayName(),
                    ((IReadOnlyTypeBase)duplicateMember.DeclaringType).DisplayName()));
        }

        ValidateClrMember(name, memberInfo, false);

        var serviceProperty = new ServiceProperty(
            name,
            memberInfo as PropertyInfo,
            memberInfo as FieldInfo,
            serviceType,
            this,
            configurationSource);

        _serviceProperties[serviceProperty.Name] = serviceProperty;

        return serviceProperty;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ServiceProperty? FindServiceProperty(string name)
        => FindDeclaredServiceProperty(Check.NotEmpty(name, nameof(name))) ?? BaseType?.FindServiceProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Property? FindServiceProperty(MemberInfo memberInfo)
        => FindProperty(memberInfo.GetSimpleMemberName());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ServiceProperty? FindDeclaredServiceProperty(string name)
        => _serviceProperties.TryGetValue(Check.NotEmpty(name, nameof(name)), out var property)
            ? property
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ServiceProperty> FindDerivedServiceProperties(string propertyName)
    {
        Check.NotNull(propertyName, nameof(propertyName));

        return DirectlyDerivedTypes.Count == 0
            ? Enumerable.Empty<ServiceProperty>()
            : (IEnumerable<ServiceProperty>)GetDerivedTypes<EntityType>()
                .Select(et => et.FindDeclaredServiceProperty(propertyName))
                .Where(p => p != null);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ServiceProperty> FindDerivedServicePropertiesInclusive(string propertyName)
        => DirectlyDerivedTypes.Count == 0
            ? ToEnumerable(FindDeclaredServiceProperty(propertyName))
            : ToEnumerable(FindDeclaredServiceProperty(propertyName)).Concat(FindDerivedServiceProperties(propertyName));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ServiceProperty> FindServicePropertiesInHierarchy(string propertyName)
        => DirectlyDerivedTypes.Count == 0
            ? ToEnumerable(FindServiceProperty(propertyName))
            : ToEnumerable(FindServiceProperty(propertyName)).Concat(FindDerivedServiceProperties(propertyName));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ServiceProperty? RemoveServiceProperty(string name)
    {
        Check.NotEmpty(name, nameof(name));

        var property = FindServiceProperty(name);
        return property == null
            ? null
            : RemoveServiceProperty(property);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ServiceProperty RemoveServiceProperty(ServiceProperty property)
    {
        Check.NotNull(property, nameof(property));
        Check.DebugAssert(IsInModel, "The entity type has been removed from the model");
        EnsureMutable();

        if (property.DeclaringEntityType != this)
        {
            throw new InvalidOperationException(
                CoreStrings.PropertyWrongType(
                    property.Name,
                    DisplayName(),
                    property.DeclaringEntityType.DisplayName()));
        }

        var removed = _serviceProperties.Remove(property.Name);
        Check.DebugAssert(removed, "removed is false");

        property.SetRemovedFromModel();

        return property;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool HasServiceProperties()
        => _serviceProperties.Count != 0 || BaseType != null && BaseType.HasServiceProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ServiceProperty> GetServiceProperties()
        => BaseType != null
            ? _serviceProperties.Count == 0
                ? BaseType.GetServiceProperties()
                : BaseType.GetServiceProperties().Concat(_serviceProperties.Values)
            : _serviceProperties.Values;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ServiceProperty> GetDeclaredServiceProperties()
        => _serviceProperties.Values;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ServiceProperty> GetDerivedServiceProperties()
        => DirectlyDerivedTypes.Count == 0
            ? Enumerable.Empty<ServiceProperty>()
            : GetDerivedTypes<EntityType>().SelectMany(et => et.GetDeclaredServiceProperties());

    #endregion

    #region Triggers

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Trigger? AddTrigger(
        string modelName,
        ConfigurationSource configurationSource)
    {
        Check.NotEmpty(modelName, nameof(modelName));
        Check.DebugAssert(IsInModel, "The entity type has been removed from the model");
        EnsureMutable();

        if (_triggers.ContainsKey(modelName))
        {
            throw new InvalidOperationException(
                CoreStrings.DuplicateTrigger(
                    modelName, DisplayName(), DisplayName()));
        }

        var trigger = new Trigger(modelName, this, configurationSource);

        _triggers.Add(modelName, trigger);

        return (Trigger?)Model.ConventionDispatcher.OnTriggerAdded(trigger.Builder)?.Metadata;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Trigger? FindDeclaredTrigger(string modelName)
    {
        Check.NotEmpty(modelName, nameof(modelName));

        return _triggers.TryGetValue(modelName, out var trigger)
            ? trigger
            : null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Trigger> GetDeclaredTriggers()
        => _triggers.Values;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Trigger? RemoveTrigger(string modelName)
    {
        Check.NotEmpty(modelName, nameof(modelName));
        Check.DebugAssert(IsInModel, "The entity type has been removed from the model");
        EnsureMutable();

        if (!_triggers.TryGetValue(modelName, out var trigger))
        {
            return null;
        }

        _triggers.Remove(modelName);

        trigger.SetRemovedFromModel();

        return (Trigger?)Model.ConventionDispatcher.OnTriggerRemoved(Builder, trigger);
    }

    #endregion

    #region Ignore

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string? OnTypeMemberIgnored(string name)
        => Model.ConventionDispatcher.OnEntityTypeMemberIgnored(Builder, name);

    #endregion

    #region Data

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<IDictionary<string, object?>> GetSeedData(bool providerValues = false)
    {
        if (_data == null
            || _data.Count == 0)
        {
            return Enumerable.Empty<IDictionary<string, object?>>();
        }

        List<IPropertyBase>? propertiesList = null;
        Dictionary<string, IPropertyBase>? propertiesMap = null;
        var data = new List<Dictionary<string, object?>>();
        var valueConverters = new Dictionary<string, ValueConverter?>(StringComparer.Ordinal);
        foreach (var rawSeed in _data)
        {
            var seed = new Dictionary<string, object?>(StringComparer.Ordinal);
            data.Add(seed);
            var type = rawSeed.GetType();

            propertiesList ??= GetProperties()
                .Concat<IPropertyBase>(GetNavigations())
                .Concat(GetSkipNavigations())
                .Concat(GetComplexProperties())
                .ToList();
            if (ClrType.IsAssignableFrom(type))
            {
                // non-anonymous type
                foreach (var propertyBase in propertiesList)
                {
                    if (propertyBase.IsShadowProperty())
                    {
                        continue;
                    }

                    ValueConverter? valueConverter = null;
                    if (providerValues
                        && propertyBase is IProperty property
                        && !valueConverters.TryGetValue(propertyBase.Name, out valueConverter))
                    {
                        valueConverter = property.GetTypeMapping().Converter;
                        valueConverters[propertyBase.Name] = valueConverter;
                    }

                    var memberInfo = propertyBase.GetMemberInfo(forMaterialization: false, forSet: false);

                    object? value = null;
                    switch (memberInfo)
                    {
                        case PropertyInfo propertyInfo:
                            if (propertyBase.IsIndexerProperty())
                            {
                                try
                                {
                                    value = propertyInfo.GetValue(rawSeed, [propertyBase.Name]);
                                }
                                catch (Exception)
                                {
                                    // Swallow if the property value is not set on the seed data
                                }
                            }
                            else
                            {
                                value = propertyInfo.GetValue(rawSeed);
                            }

                            break;
                        case FieldInfo fieldInfo:
                            value = fieldInfo.GetValue(rawSeed);
                            break;
                        case null:
                            continue;
                    }

                    seed[propertyBase.Name] = valueConverter == null
                        ? value
                        : valueConverter.ConvertToProvider(value);
                }
            }
            else
            {
                // anonymous type
                propertiesMap ??= GetProperties()
                    .Concat<IPropertyBase>(GetNavigations())
                    .Concat(GetSkipNavigations())
                    .ToDictionary(p => p.Name);
                foreach (var memberInfo in type.GetMembersInHierarchy())
                {
                    if (!propertiesMap.TryGetValue(memberInfo.GetSimpleMemberName(), out var propertyBase))
                    {
                        continue;
                    }

                    ValueConverter? valueConverter = null;
                    if (providerValues
                        && !valueConverters.TryGetValue(propertyBase.Name, out valueConverter))
                    {
                        if (propertyBase is IReadOnlyProperty property)
                        {
                            valueConverter = property.GetTypeMapping().Converter;
                        }

                        valueConverters[propertyBase.Name] = valueConverter;
                    }

                    // All memberInfos are PropertyInfo in anonymous type
                    var value = ((PropertyInfo)memberInfo).GetValue(rawSeed);

                    seed[propertyBase.Name] = valueConverter == null
                        ? value
                        : valueConverter.ConvertToProvider(value);
                }
            }
        }

        return data;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<object> GetRawSeedData()
        => _data ?? Enumerable.Empty<object>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AddData(IEnumerable<object> data)
    {
        EnsureMutable();

        _data ??= [];

        foreach (var entity in data)
        {
            if (ClrType != entity.GetType()
                && ClrType.IsInstanceOfType(entity))
            {
                throw new InvalidOperationException(
                    CoreStrings.SeedDatumDerivedType(
                        DisplayName(), entity.GetType().ShortDisplayName()));
            }

            _data.Add(entity);
        }
    }

    #endregion

    #region Other

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PropertyAccessMode GetNavigationAccessMode()
        => (PropertyAccessMode?)this[CoreAnnotationNames.NavigationAccessMode]
            ?? GetPropertyAccessMode();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PropertyAccessMode? SetNavigationAccessMode(
        PropertyAccessMode? propertyAccessMode,
        ConfigurationSource configurationSource)
        => (PropertyAccessMode?)SetOrRemoveAnnotation(
            CoreAnnotationNames.NavigationAccessMode, propertyAccessMode, configurationSource)?.Value;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual LambdaExpression? SetQueryFilter(LambdaExpression? queryFilter, ConfigurationSource configurationSource)
    {
        var errorMessage = CheckQueryFilter(queryFilter);
        if (errorMessage != null)
        {
            throw new InvalidOperationException(errorMessage);
        }

        return (LambdaExpression?)SetOrRemoveAnnotation(CoreAnnotationNames.QueryFilter, queryFilter, configurationSource)?.Value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? CheckQueryFilter(LambdaExpression? queryFilter)
    {
        if (queryFilter != null
            && (queryFilter.Parameters.Count != 1
                || queryFilter.Parameters[0].Type != ClrType
                || queryFilter.ReturnType != typeof(bool)))
        {
            return CoreStrings.BadFilterExpression(queryFilter, DisplayName(), ClrType);
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual LambdaExpression? GetQueryFilter()
        => (LambdaExpression?)this[CoreAnnotationNames.QueryFilter];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetQueryFilterConfigurationSource()
        => FindAnnotation(CoreAnnotationNames.QueryFilter)?.GetConfigurationSource();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [Obsolete]
    public virtual LambdaExpression? SetDefiningQuery(LambdaExpression? definingQuery, ConfigurationSource configurationSource)
        => (LambdaExpression?)SetOrRemoveAnnotation(CoreAnnotationNames.DefiningQuery, definingQuery, configurationSource)?.Value;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Property? SetDiscriminatorProperty(Property? property, ConfigurationSource configurationSource)
    {
        if ((string?)this[CoreAnnotationNames.DiscriminatorProperty] == property?.Name)
        {
            return property;
        }

        CheckDiscriminatorProperty(property);

        SetAnnotation(CoreAnnotationNames.DiscriminatorProperty, property?.Name, configurationSource);

        return Model.ConventionDispatcher.OnDiscriminatorPropertySet(Builder, property?.Name) == property?.Name
                ? property
                : (Property?)((IReadOnlyEntityType)this).FindDiscriminatorProperty();
    }

    private void CheckDiscriminatorProperty(Property? property)
    {
        if (property != null)
        {
            if (BaseType != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.DiscriminatorPropertyMustBeOnRoot(DisplayName()));
            }

            if (property.DeclaringType != this)
            {
                throw new InvalidOperationException(
                    CoreStrings.DiscriminatorPropertyNotFound(property.Name, DisplayName()));
            }
        }
    }

    /// <summary>
    ///     Returns the name of the property that will be used for storing a discriminator value.
    /// </summary>
    /// <returns>The name of the property that will be used for storing a discriminator value.</returns>
    public virtual string? GetDiscriminatorPropertyName()
        => BaseType is null
            ? (string?)this[CoreAnnotationNames.DiscriminatorProperty]
            : ((IReadOnlyEntityType)this).GetRootType().GetDiscriminatorPropertyName();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    public virtual ConfigurationSource? GetDiscriminatorPropertyConfigurationSource()
        => FindAnnotation(CoreAnnotationNames.DiscriminatorProperty)?.GetConfigurationSource();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsImplicitlyCreatedJoinEntityType
        => GetConfigurationSource() == ConfigurationSource.Convention
            && ClrType == Model.DefaultPropertyBagType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override InstantiationBinding? ConstructorBinding
    {
        get => IsReadOnly && !ClrType.IsAbstract
            ? NonCapturingLazyInitializer.EnsureInitialized(
                ref _constructorBinding, this, static entityType =>
                {
                    ((IModel)entityType.Model).GetModelDependencies().ConstructorBindingFactory.GetBindings(
                        (IReadOnlyEntityType)entityType,
                        out entityType._constructorBinding,
                        out entityType._serviceOnlyConstructorBinding);
                })
            : _constructorBinding;

        set => SetConstructorBinding(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InstantiationBinding? SetConstructorBinding(
        InstantiationBinding? constructorBinding,
        ConfigurationSource configurationSource)
    {
        EnsureMutable();

        _constructorBinding = constructorBinding;

        if (_constructorBinding == null)
        {
            _constructorBindingConfigurationSource = null;
        }
        else
        {
            UpdateConstructorBindingConfigurationSource(configurationSource);
        }

        return constructorBinding;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetConstructorBindingConfigurationSource()
        => _constructorBindingConfigurationSource;

    private void UpdateConstructorBindingConfigurationSource(ConfigurationSource configurationSource)
        => _constructorBindingConfigurationSource = configurationSource.Max(_constructorBindingConfigurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InstantiationBinding? ServiceOnlyConstructorBinding
    {
        get => _serviceOnlyConstructorBinding;
        set => SetServiceOnlyConstructorBinding(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InstantiationBinding? SetServiceOnlyConstructorBinding(
        InstantiationBinding? constructorBinding,
        ConfigurationSource configurationSource)
    {
        EnsureMutable();

        _serviceOnlyConstructorBinding = constructorBinding;

        if (_serviceOnlyConstructorBinding == null)
        {
            _serviceOnlyConstructorBindingConfigurationSource = null;
        }
        else
        {
            UpdateServiceOnlyConstructorBindingConfigurationSource(configurationSource);
        }

        return constructorBinding;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetServiceOnlyConstructorBindingConfigurationSource()
        => _serviceOnlyConstructorBindingConfigurationSource;

    private void UpdateServiceOnlyConstructorBindingConfigurationSource(ConfigurationSource configurationSource)
        => _serviceOnlyConstructorBindingConfigurationSource =
            configurationSource.Max(_serviceOnlyConstructorBindingConfigurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual Func<MaterializationContext, object> GetOrCreateMaterializer(IEntityMaterializerSource source)
        => source.GetMaterializer(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual Func<MaterializationContext, object> GetOrCreateEmptyMaterializer(IEntityMaterializerSource source)
        => source.GetEmptyMaterializer(this);

    #endregion

    #region Explicit interface implementations

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionEntityTypeBuilder IConventionEntityType.Builder
    {
        [DebuggerStepThrough]
        get => Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionAnnotatableBuilder IConventionAnnotatable.Builder
    {
        [DebuggerStepThrough]
        get => Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IReadOnlyModel IReadOnlyTypeBase.Model
    {
        [DebuggerStepThrough]
        get => Model;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IMutableModel IMutableTypeBase.Model
    {
        [DebuggerStepThrough]
        get => Model;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IModel ITypeBase.Model
    {
        [DebuggerStepThrough]
        get => Model;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IReadOnlyEntityType? IReadOnlyEntityType.BaseType
    {
        [DebuggerStepThrough]
        get => BaseType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IMutableEntityType? IMutableEntityType.BaseType
    {
        get => BaseType;
        set => SetBaseType((EntityType?)value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionEntityType? IConventionEntityType.BaseType
    {
        [DebuggerStepThrough]
        get => BaseType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IEntityType? IEntityType.BaseType
    {
        [DebuggerStepThrough]
        get => BaseType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableEntityType.SetDiscriminatorProperty(IReadOnlyProperty? property)
        => SetDiscriminatorProperty((Property?)property, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionProperty? IConventionEntityType.SetDiscriminatorProperty(
        IReadOnlyProperty? property,
        bool fromDataAnnotation)
        => SetDiscriminatorProperty(
            (Property?)property,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableEntityType.SetQueryFilter(LambdaExpression? queryFilter)
        => SetQueryFilter(queryFilter, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    LambdaExpression? IConventionEntityType.SetQueryFilter(LambdaExpression? queryFilter, bool fromDataAnnotation)
        => SetQueryFilter(queryFilter, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyEntityType> IReadOnlyEntityType.GetDerivedTypes()
        => GetDerivedTypes<EntityType>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyEntityType> IReadOnlyEntityType.GetDirectlyDerivedTypes()
        => GetDirectlyDerivedTypes();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IEntityType> IEntityType.GetDirectlyDerivedTypes()
        => GetDirectlyDerivedTypes();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionEntityType? IConventionEntityType.SetBaseType(IConventionEntityType? entityType, bool fromDataAnnotation)
        => SetBaseType(
            (EntityType?)entityType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool? IConventionEntityType.SetIsKeyless(bool? keyless, bool fromDataAnnotation)
        => SetIsKeyless(keyless, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableKey? IMutableEntityType.SetPrimaryKey(IReadOnlyList<IMutableProperty>? properties)
        => SetPrimaryKey(properties?.Cast<Property>().ToList(), ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionKey? IConventionEntityType.SetPrimaryKey(IReadOnlyList<IConventionProperty>? properties, bool fromDataAnnotation)
        => SetPrimaryKey(
            properties?.Cast<Property>().ToList(),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IReadOnlyKey? IReadOnlyEntityType.FindPrimaryKey()
        => FindPrimaryKey();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableKey? IMutableEntityType.FindPrimaryKey()
        => FindPrimaryKey();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionKey? IConventionEntityType.FindPrimaryKey()
        => FindPrimaryKey();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IKey? IEntityType.FindPrimaryKey()
        => FindPrimaryKey();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableKey IMutableEntityType.AddKey(IReadOnlyList<IMutableProperty> properties)
        => AddKey(properties.Cast<Property>().ToList(), ConfigurationSource.Explicit)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionKey? IConventionEntityType.AddKey(IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation)
        => AddKey(
            properties.Cast<Property>().ToList(),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IReadOnlyKey? IReadOnlyEntityType.FindKey(IReadOnlyList<IReadOnlyProperty> properties)
        => FindKey(properties);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableKey? IMutableEntityType.FindKey(IReadOnlyList<IReadOnlyProperty> properties)
        => FindKey(properties);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionKey? IConventionEntityType.FindKey(IReadOnlyList<IReadOnlyProperty> properties)
        => FindKey(properties);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IKey? IEntityType.FindKey(IReadOnlyList<IReadOnlyProperty> properties)
        => FindKey(properties);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyKey> IReadOnlyEntityType.GetDeclaredKeys()
        => GetDeclaredKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IKey> IEntityType.GetDeclaredKeys()
        => GetDeclaredKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyKey> IReadOnlyEntityType.GetKeys()
        => GetKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IMutableKey> IMutableEntityType.GetKeys()
        => GetKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IConventionKey> IConventionEntityType.GetKeys()
        => GetKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IKey> IEntityType.GetKeys()
        => GetKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableKey? IMutableEntityType.RemoveKey(IReadOnlyList<IReadOnlyProperty> properties)
        => RemoveKey(properties);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionKey? IConventionEntityType.RemoveKey(IReadOnlyList<IReadOnlyProperty> properties)
        => RemoveKey(properties);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableKey? IMutableEntityType.RemoveKey(IReadOnlyKey key)
        => RemoveKey((Key)key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionKey? IConventionEntityType.RemoveKey(IReadOnlyKey key)
        => RemoveKey((Key)key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableForeignKey IMutableEntityType.AddForeignKey(
        IReadOnlyList<IMutableProperty> properties,
        IMutableKey principalKey,
        IMutableEntityType principalEntityType)
        => AddForeignKey(
            properties.Cast<Property>().ToList(),
            (Key)principalKey,
            (EntityType)principalEntityType,
            ConfigurationSource.Explicit,
            ConfigurationSource.Explicit)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionForeignKey? IConventionEntityType.AddForeignKey(
        IReadOnlyList<IConventionProperty> properties,
        IConventionKey principalKey,
        IConventionEntityType principalEntityType,
        bool setComponentConfigurationSource,
        bool fromDataAnnotation)
        => AddForeignKey(
            properties.Cast<Property>().ToList(),
            (Key)principalKey,
            (EntityType)principalEntityType,
            setComponentConfigurationSource
                ? fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention
                : null,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IReadOnlyForeignKey? IReadOnlyEntityType.FindForeignKey(
        IReadOnlyList<IReadOnlyProperty> properties,
        IReadOnlyKey principalKey,
        IReadOnlyEntityType principalEntityType)
        => FindForeignKey(properties, principalKey, principalEntityType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableForeignKey? IMutableEntityType.FindForeignKey(
        IReadOnlyList<IReadOnlyProperty> properties,
        IReadOnlyKey principalKey,
        IReadOnlyEntityType principalEntityType)
        => FindForeignKey(properties, principalKey, principalEntityType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionForeignKey? IConventionEntityType.FindForeignKey(
        IReadOnlyList<IReadOnlyProperty> properties,
        IReadOnlyKey principalKey,
        IReadOnlyEntityType principalEntityType)
        => FindForeignKey(properties, principalKey, principalEntityType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IForeignKey? IEntityType.FindForeignKey(
        IReadOnlyList<IReadOnlyProperty> properties,
        IReadOnlyKey principalKey,
        IReadOnlyEntityType principalEntityType)
        => FindForeignKey(properties, principalKey, principalEntityType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.FindForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
        => FindForeignKeys(properties);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IForeignKey> IEntityType.FindForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
        => FindForeignKeys(properties);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.FindDeclaredForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
        => FindDeclaredForeignKeys(properties);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IForeignKey> IEntityType.FindDeclaredForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
        => FindDeclaredForeignKeys(properties);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetForeignKeys()
        => GetForeignKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IMutableForeignKey> IMutableEntityType.GetForeignKeys()
        => GetForeignKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IConventionForeignKey> IConventionEntityType.GetForeignKeys()
        => GetForeignKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IForeignKey> IEntityType.GetForeignKeys()
        => GetForeignKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetDeclaredForeignKeys()
        => GetDeclaredForeignKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IForeignKey> IEntityType.GetDeclaredForeignKeys()
        => GetDeclaredForeignKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetDerivedForeignKeys()
        => GetDerivedForeignKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IForeignKey> IEntityType.GetDerivedForeignKeys()
        => GetDerivedForeignKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetDeclaredReferencingForeignKeys()
        => GetDeclaredReferencingForeignKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IForeignKey> IEntityType.GetDeclaredReferencingForeignKeys()
        => GetDeclaredReferencingForeignKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetReferencingForeignKeys()
        => GetReferencingForeignKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IForeignKey> IEntityType.GetReferencingForeignKeys()
        => GetReferencingForeignKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionForeignKey? IConventionEntityType.RemoveForeignKey(
        IReadOnlyList<IReadOnlyProperty> properties,
        IConventionKey principalKey,
        IConventionEntityType principalEntityType)
        => RemoveForeignKey(properties, principalKey, principalEntityType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableForeignKey? IMutableEntityType.RemoveForeignKey(
        IReadOnlyList<IReadOnlyProperty> properties,
        IMutableKey principalKey,
        IMutableEntityType principalEntityType)
        => RemoveForeignKey(properties, principalKey, principalEntityType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableForeignKey? IMutableEntityType.RemoveForeignKey(IReadOnlyForeignKey foreignKey)
        => RemoveForeignKey((ForeignKey)foreignKey);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionForeignKey? IConventionEntityType.RemoveForeignKey(IReadOnlyForeignKey foreignKey)
        => RemoveForeignKey((ForeignKey)foreignKey);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyNavigation> IReadOnlyEntityType.GetDeclaredNavigations()
        => GetDeclaredNavigations();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<INavigation> IEntityType.GetDeclaredNavigations()
        => GetDeclaredNavigations();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IReadOnlyNavigation? IReadOnlyEntityType.FindDeclaredNavigation(string name)
        => FindDeclaredNavigation(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    INavigation? IEntityType.FindDeclaredNavigation(string name)
        => FindDeclaredNavigation(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyNavigation> IReadOnlyEntityType.GetDerivedNavigations()
        => GetDerivedNavigations();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyNavigation> IReadOnlyEntityType.GetNavigations()
        => GetNavigations();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<INavigation> IEntityType.GetNavigations()
        => GetNavigations();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableSkipNavigation IMutableEntityType.AddSkipNavigation(
        string name,
        Type? navigationType,
        MemberInfo? memberInfo,
        IMutableEntityType targetEntityType,
        bool collection,
        bool onDependent)
        => AddSkipNavigation(
            name, navigationType, memberInfo, (EntityType)targetEntityType, collection, onDependent,
            ConfigurationSource.Explicit)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionSkipNavigation? IConventionEntityType.AddSkipNavigation(
        string name,
        Type? navigationType,
        MemberInfo? memberInfo,
        IConventionEntityType targetEntityType,
        bool collection,
        bool onDependent,
        bool fromDataAnnotation)
        => AddSkipNavigation(
            name, navigationType, memberInfo, (EntityType)targetEntityType, collection, onDependent,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IReadOnlySkipNavigation? IReadOnlyEntityType.FindSkipNavigation(MemberInfo memberInfo)
        => FindSkipNavigation(memberInfo);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IReadOnlySkipNavigation? IReadOnlyEntityType.FindSkipNavigation(string name)
        => FindSkipNavigation(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableSkipNavigation? IMutableEntityType.FindSkipNavigation(string name)
        => FindSkipNavigation(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionSkipNavigation? IConventionEntityType.FindSkipNavigation(string name)
        => FindSkipNavigation(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    ISkipNavigation? IEntityType.FindSkipNavigation(string name)
        => FindSkipNavigation(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IReadOnlySkipNavigation? IReadOnlyEntityType.FindDeclaredSkipNavigation(string name)
        => FindDeclaredSkipNavigation(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlySkipNavigation> IReadOnlyEntityType.GetDeclaredSkipNavigations()
        => GetDeclaredSkipNavigations();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlySkipNavigation> IReadOnlyEntityType.GetDerivedSkipNavigations()
        => GetDerivedSkipNavigations();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlySkipNavigation> IReadOnlyEntityType.GetSkipNavigations()
        => GetSkipNavigations();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IMutableSkipNavigation> IMutableEntityType.GetSkipNavigations()
        => GetSkipNavigations();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IConventionSkipNavigation> IConventionEntityType.GetSkipNavigations()
        => GetSkipNavigations();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<ISkipNavigation> IEntityType.GetSkipNavigations()
        => GetSkipNavigations();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableSkipNavigation? IMutableEntityType.RemoveSkipNavigation(IReadOnlySkipNavigation navigation)
        => RemoveSkipNavigation((SkipNavigation)navigation);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionSkipNavigation? IConventionEntityType.RemoveSkipNavigation(IReadOnlySkipNavigation navigation)
        => RemoveSkipNavigation((SkipNavigation)navigation);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IProperty? IEntityType.FindProperty(string name) => FindProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IProperty? IEntityType.FindDeclaredProperty(string name) => FindDeclaredProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IProperty> IEntityType.GetDeclaredProperties() => GetDeclaredProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IProperty> IEntityType.GetProperties() => GetProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableIndex IMutableEntityType.AddIndex(IReadOnlyList<IMutableProperty> properties)
        => AddIndex(properties as IReadOnlyList<Property> ?? properties.Cast<Property>().ToList(), ConfigurationSource.Explicit)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableIndex IMutableEntityType.AddIndex(IReadOnlyList<IMutableProperty> properties, string name)
        => AddIndex(properties as IReadOnlyList<Property> ?? properties.Cast<Property>().ToList(), name, ConfigurationSource.Explicit)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionIndex? IConventionEntityType.AddIndex(IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation)
        => AddIndex(
            properties as IReadOnlyList<Property> ?? properties.Cast<Property>().ToList(),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionIndex? IConventionEntityType.AddIndex(
        IReadOnlyList<IConventionProperty> properties,
        string name,
        bool fromDataAnnotation)
        => AddIndex(
            properties as IReadOnlyList<Property> ?? properties.Cast<Property>().ToList(),
            name,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IReadOnlyIndex? IReadOnlyEntityType.FindIndex(IReadOnlyList<IReadOnlyProperty> properties)
        => FindIndex(properties);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableIndex? IMutableEntityType.FindIndex(IReadOnlyList<IReadOnlyProperty> properties)
        => FindIndex(properties);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionIndex? IConventionEntityType.FindIndex(IReadOnlyList<IReadOnlyProperty> properties)
        => FindIndex(properties);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IIndex? IEntityType.FindIndex(IReadOnlyList<IReadOnlyProperty> properties)
        => FindIndex(properties);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IReadOnlyIndex? IReadOnlyEntityType.FindIndex(string name)
        => FindIndex(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableIndex? IMutableEntityType.FindIndex(string name)
        => FindIndex(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionIndex? IConventionEntityType.FindIndex(string name)
        => FindIndex(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IIndex? IEntityType.FindIndex(string name)
        => FindIndex(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyIndex> IReadOnlyEntityType.GetDeclaredIndexes()
        => GetDeclaredIndexes();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IIndex> IEntityType.GetDeclaredIndexes()
        => GetDeclaredIndexes();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyIndex> IReadOnlyEntityType.GetDerivedIndexes()
        => GetDerivedIndexes();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IIndex> IEntityType.GetDerivedIndexes()
        => GetDerivedIndexes();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyIndex> IReadOnlyEntityType.GetIndexes()
        => GetIndexes();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IMutableIndex> IMutableEntityType.GetIndexes()
        => GetIndexes();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IConventionIndex> IConventionEntityType.GetIndexes()
        => GetIndexes();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IIndex> IEntityType.GetIndexes()
        => GetIndexes();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionIndex? IConventionEntityType.RemoveIndex(IReadOnlyList<IReadOnlyProperty> properties)
        => RemoveIndex(properties);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableIndex? IMutableEntityType.RemoveIndex(IReadOnlyList<IReadOnlyProperty> properties)
        => RemoveIndex(properties);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableIndex? IMutableEntityType.RemoveIndex(IReadOnlyIndex index)
        => RemoveIndex((Index)index);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionIndex? IConventionEntityType.RemoveIndex(IReadOnlyIndex index)
        => RemoveIndex((Index)index);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableServiceProperty IMutableEntityType.AddServiceProperty(MemberInfo memberInfo, Type? serviceType)
        => AddServiceProperty(memberInfo, serviceType ?? memberInfo.GetMemberType(), ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionServiceProperty IConventionEntityType.AddServiceProperty(MemberInfo memberInfo, Type? serviceType, bool fromDataAnnotation)
        => AddServiceProperty(
            memberInfo,
            serviceType ?? memberInfo.GetMemberType(),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IReadOnlyServiceProperty? IReadOnlyEntityType.FindServiceProperty(string name)
        => FindServiceProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableServiceProperty? IMutableEntityType.FindServiceProperty(string name)
        => FindServiceProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionServiceProperty? IConventionEntityType.FindServiceProperty(string name)
        => FindServiceProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IServiceProperty? IEntityType.FindServiceProperty(string name)
        => FindServiceProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyServiceProperty> IReadOnlyEntityType.GetDeclaredServiceProperties()
        => GetDeclaredServiceProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IServiceProperty> IEntityType.GetDeclaredServiceProperties()
        => GetDeclaredServiceProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyServiceProperty> IReadOnlyEntityType.GetDerivedServiceProperties()
        => GetDerivedServiceProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool IReadOnlyEntityType.HasServiceProperties()
        => HasServiceProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyServiceProperty> IReadOnlyEntityType.GetServiceProperties()
        => GetServiceProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IMutableServiceProperty> IMutableEntityType.GetServiceProperties()
        => GetServiceProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IConventionServiceProperty> IConventionEntityType.GetServiceProperties()
        => GetServiceProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IServiceProperty> IEntityType.GetServiceProperties()
        => GetServiceProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableServiceProperty? IMutableEntityType.RemoveServiceProperty(IReadOnlyServiceProperty property)
        => RemoveServiceProperty((ServiceProperty)property);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionServiceProperty? IConventionEntityType.RemoveServiceProperty(IReadOnlyServiceProperty property)
        => RemoveServiceProperty((ServiceProperty)property);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableServiceProperty? IMutableEntityType.RemoveServiceProperty(string name)
        => RemoveServiceProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionServiceProperty? IConventionEntityType.RemoveServiceProperty(string name)
        => RemoveServiceProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IReadOnlyTrigger? IReadOnlyEntityType.FindDeclaredTrigger(string name)
        => FindDeclaredTrigger(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionTrigger? IConventionEntityType.FindDeclaredTrigger(string name)
        => FindDeclaredTrigger(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableTrigger? IMutableEntityType.FindDeclaredTrigger(string name)
        => FindDeclaredTrigger(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    ITrigger? IEntityType.FindDeclaredTrigger(string name)
        => FindDeclaredTrigger(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyTrigger> IReadOnlyEntityType.GetDeclaredTriggers()
        => GetDeclaredTriggers();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IConventionTrigger> IConventionEntityType.GetDeclaredTriggers()
        => GetDeclaredTriggers();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IMutableTrigger> IMutableEntityType.GetDeclaredTriggers()
        => GetDeclaredTriggers();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<ITrigger> IEntityType.GetDeclaredTriggers()
        => GetDeclaredTriggers();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableTrigger IMutableEntityType.AddTrigger(string name)
        => AddTrigger(name, ConfigurationSource.Explicit)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionTrigger? IConventionEntityType.AddTrigger(string name, bool fromDataAnnotation)
        => AddTrigger(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableTrigger? IMutableEntityType.RemoveTrigger(string name)
        => RemoveTrigger(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionTrigger? IConventionEntityType.RemoveTrigger(string name)
        => RemoveTrigger(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IProperty> IEntityType.GetForeignKeyProperties()
        => ForeignKeyProperties;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IProperty> IEntityType.GetValueGeneratingProperties()
        => ValueGeneratingProperties;

    #endregion

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class Snapshot
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            EntityType entityType,
            PropertiesSnapshot? properties,
            List<InternalIndexBuilder>? indexes,
            List<(InternalKeyBuilder, ConfigurationSource?)>? keys,
            List<RelationshipSnapshot>? relationships,
            List<InternalSkipNavigationBuilder>? skipNavigations,
            List<InternalServicePropertyBuilder>? serviceProperties)
        {
            EntityType = entityType;
            Properties = properties ?? new PropertiesSnapshot(null, null, null, null);
            if (indexes != null)
            {
                Properties.Add(indexes);
            }

            if (keys != null)
            {
                Properties.Add(keys);
            }

            if (relationships != null)
            {
                Properties.Add(relationships);
            }

            SkipNavigations = skipNavigations;
            ServiceProperties = serviceProperties;
        }

        private EntityType EntityType { [DebuggerStepThrough] get; }
        private PropertiesSnapshot Properties { [DebuggerStepThrough] get; }
        private List<InternalSkipNavigationBuilder>? SkipNavigations { [DebuggerStepThrough] get; }
        private List<InternalServicePropertyBuilder>? ServiceProperties { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Attach(InternalEntityTypeBuilder entityTypeBuilder)
        {
            entityTypeBuilder.MergeAnnotationsFrom(EntityType);

            foreach (var ignoredMember in EntityType.GetIgnoredMembers())
            {
                entityTypeBuilder.Ignore(ignoredMember, EntityType.FindDeclaredIgnoredConfigurationSource(ignoredMember)!.Value);
            }

            if (EntityType._baseTypeConfigurationSource != null)
            {
                var baseType = EntityType.BaseType;
                if (baseType?.IsInModel == false)
                {
                    baseType = EntityType.Model.FindActualEntityType(baseType);
                }

                entityTypeBuilder.Metadata.SetBaseType(baseType, EntityType._baseTypeConfigurationSource.Value);
            }

            if (EntityType._isKeylessConfigurationSource != null)
            {
                entityTypeBuilder.Metadata.SetIsKeyless(EntityType.IsKeyless, EntityType._isKeylessConfigurationSource.Value);
            }

            if (EntityType.GetChangeTrackingStrategyConfigurationSource() != null)
            {
                entityTypeBuilder.Metadata.SetChangeTrackingStrategy(
                    EntityType.GetChangeTrackingStrategy(), EntityType.GetChangeTrackingStrategyConfigurationSource()!.Value);
            }

            foreach (var trigger in EntityType.GetDeclaredTriggers())
            {
                trigger.Builder.Attach(entityTypeBuilder);
            }

            if (ServiceProperties != null)
            {
                foreach (var detachedServiceProperty in ServiceProperties)
                {
                    detachedServiceProperty.Attach(entityTypeBuilder);
                }
            }

            Properties.Attach(entityTypeBuilder);

            if (SkipNavigations != null)
            {
                foreach (var detachedSkipNavigation in SkipNavigations)
                {
                    detachedSkipNavigation.Attach();
                }
            }

            if (EntityType._constructorBindingConfigurationSource != null)
            {
                entityTypeBuilder.Metadata.SetConstructorBinding(
                    Create(EntityType.ConstructorBinding, entityTypeBuilder.Metadata),
                    EntityType._constructorBindingConfigurationSource.Value);
            }

            if (EntityType._serviceOnlyConstructorBindingConfigurationSource != null)
            {
                entityTypeBuilder.Metadata.SetServiceOnlyConstructorBinding(
                    Create(EntityType.ServiceOnlyConstructorBinding, entityTypeBuilder.Metadata),
                    EntityType._serviceOnlyConstructorBindingConfigurationSource.Value);
            }

            var rawData = EntityType._data;
            if (rawData != null)
            {
                entityTypeBuilder.Metadata.AddData(rawData);
            }
        }

        private static InstantiationBinding? Create(InstantiationBinding? instantiationBinding, EntityType entityType)
            => instantiationBinding?.With(
                instantiationBinding.ParameterBindings.Select(binding => Create(binding, entityType)).ToList());

        private static ParameterBinding Create(ParameterBinding parameterBinding, EntityType entityType)
            => parameterBinding.With(
                parameterBinding.ConsumedProperties.Select(
                    property =>
                        (entityType.FindProperty(property.Name)
                            ?? entityType.FindServiceProperty(property.Name)
                            ?? entityType.FindComplexProperty(property.Name)
                            ?? entityType.FindNavigation(property.Name)
                            ?? (IPropertyBase?)entityType.FindSkipNavigation(property.Name))!).ToArray());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DebugView DebugView
        => new(
            () => ((IReadOnlyEntityType)this).ToDebugString(),
            () => ((IReadOnlyEntityType)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IReadOnlyEntityType)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);
}
