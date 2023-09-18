// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public abstract class TypeBase : ConventionAnnotatable, IMutableTypeBase, IConventionTypeBase, ITypeBase
{
    private readonly SortedDictionary<string, Property> _properties;
    private readonly SortedDictionary<string, ComplexProperty> _complexProperties = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ConfigurationSource> _ignoredMembers = new(StringComparer.Ordinal);

    private TypeBase? _baseType;
    private readonly SortedSet<TypeBase> _directlyDerivedTypes = new(TypeBaseNameComparer.Instance);
    private ChangeTrackingStrategy? _changeTrackingStrategy;

    private ConfigurationSource _configurationSource;
    private ConfigurationSource? _changeTrackingStrategyConfigurationSource;

    private bool _indexerPropertyInitialized;
    private PropertyInfo? _indexerPropertyInfo;
    private SortedDictionary<string, PropertyInfo>? _runtimeProperties;
    private SortedDictionary<string, FieldInfo>? _runtimeFields;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected TypeBase(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        Model model,
        ConfigurationSource configurationSource)
    {
        Check.NotNull(model, nameof(model));

        ClrType = type;
        Model = model;
        _configurationSource = configurationSource;
        Name = model.GetDisplayName(type);
        HasSharedClrType = false;
        IsPropertyBag = type.IsPropertyBagType();
        _properties = new SortedDictionary<string, Property>(new PropertyNameComparer(this));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected TypeBase(
        string name,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        Model model,
        ConfigurationSource configurationSource)
    {
        Name = name;
        ClrType = type;
        Model = model;
        _configurationSource = configurationSource;
        HasSharedClrType = true;
        IsPropertyBag = type.IsPropertyBagType();
        _properties = new SortedDictionary<string, Property>(new PropertyNameComparer(this));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)]
    public virtual Type ClrType { [DebuggerStepThrough] get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Model Model { [DebuggerStepThrough] get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool IsReadOnly
        => Model.IsReadOnly;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract bool IsInModel { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Name { [DebuggerStepThrough] get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool HasSharedClrType { [DebuggerStepThrough] get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsPropertyBag { [DebuggerStepThrough] get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalTypeBaseBuilder Builder
        => BaseBuilder;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected abstract InternalTypeBaseBuilder BaseBuilder { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual TypeBase? BaseType
    {
        get => _baseType;
        set => _baseType = value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SortedSet<TypeBase> DirectlyDerivedTypes
        => _directlyDerivedTypes;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<TypeBase> GetDerivedTypes()
        => GetDerivedTypes<TypeBase>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IEnumerable<T> GetDerivedTypes<T>()
        where T : TypeBase
    {
        if (DirectlyDerivedTypes.Count == 0)
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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    public virtual IEnumerable<TypeBase> GetDerivedTypesInclusive()
        => DirectlyDerivedTypes.Count == 0
            ? new[] { this }
            : new[] { this }.Concat(GetDerivedTypes());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsAssignableFrom(TypeBase derivedType)
    {
        Check.NotNull(derivedType, nameof(derivedType));

        if (derivedType == this)
        {
            return true;
        }

        if (DirectlyDerivedTypes.Count == 0)
        {
            return false;
        }

        var baseType = derivedType.BaseType;
        while (baseType != null)
        {
            if (baseType == this)
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    public virtual ConfigurationSource GetConfigurationSource()
        => _configurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
        => _configurationSource = configurationSource.Max(_configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract IEnumerable<PropertyBase> GetMembers();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract IEnumerable<PropertyBase> GetDeclaredMembers();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract PropertyBase? FindMember(string name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract IEnumerable<PropertyBase> FindMembersInHierarchy(string name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Type? ValidateClrMember(string name, MemberInfo memberInfo, bool throwOnNameMismatch = true)
    {
        if (name != memberInfo.GetSimpleMemberName())
        {
            if (memberInfo != FindIndexerPropertyInfo())
            {
                if (throwOnNameMismatch)
                {
                    throw new InvalidOperationException(
                        CoreStrings.PropertyWrongName(
                            name,
                            DisplayName(),
                            memberInfo.GetSimpleMemberName()));
                }

                return memberInfo.GetMemberType();
            }

            var clashingMemberInfo = IsPropertyBag
                ? null
                : ClrType.GetMembersInHierarchy(name).FirstOrDefault();
            if (clashingMemberInfo != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.PropertyClashingNonIndexer(
                        name,
                        DisplayName()));
            }
        }

        return null;
    }

    #region Properties

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Property? AddProperty(
        string name,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType,
        ConfigurationSource? typeConfigurationSource,
        ConfigurationSource configurationSource)
    {
        Check.NotNull(name, nameof(name));
        Check.NotNull(propertyType, nameof(propertyType));

        return AddProperty(
            name,
            propertyType,
            null,
            typeConfigurationSource,
            configurationSource);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [RequiresUnreferencedCode("Use an overload that accepts a type")]
    public virtual Property? AddProperty(
        MemberInfo memberInfo,
        ConfigurationSource configurationSource)
        => AddProperty(
            memberInfo.GetSimpleMemberName(),
            memberInfo.GetMemberType(),
            memberInfo,
            configurationSource,
            configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [RequiresUnreferencedCode("Use an overload that accepts a type")]
    public virtual Property? AddProperty(
        string name,
        ConfigurationSource configurationSource)
    {
        MemberInfo? clrMember;
        if (IsPropertyBag)
        {
            clrMember = FindIndexerPropertyInfo()!;
        }
        else
        {
            clrMember = ClrType.GetMembersInHierarchy(name).FirstOrDefault();
            if (clrMember == null)
            {
                throw new InvalidOperationException(CoreStrings.NoPropertyType(name, DisplayName()));
            }
        }

        return AddProperty(clrMember, configurationSource);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Property? AddProperty(
        string name,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType,
        MemberInfo? memberInfo,
        ConfigurationSource? typeConfigurationSource,
        ConfigurationSource configurationSource)
    {
        Check.NotNull(name, nameof(name));
        Check.NotNull(propertyType, nameof(propertyType));
        Check.DebugAssert(IsInModel, "The entity type has been removed from the model");
        EnsureMutable();

        var conflictingMember = FindMembersInHierarchy(name).FirstOrDefault();
        if (conflictingMember != null)
        {
            throw new InvalidOperationException(
                CoreStrings.ConflictingPropertyOrNavigation(
                    name, DisplayName(),
                    ((IReadOnlyTypeBase)conflictingMember.DeclaringType).DisplayName()));
        }

        if (memberInfo != null)
        {
            propertyType = ValidateClrMember(name, memberInfo, typeConfigurationSource != null)
                ?? propertyType;

            if (memberInfo.DeclaringType?.IsAssignableFrom(ClrType) != true)
            {
                throw new InvalidOperationException(
                    CoreStrings.PropertyWrongEntityClrType(
                        memberInfo.Name, DisplayName(), memberInfo.DeclaringType?.ShortDisplayName()));
            }
        }
        else if (IsPropertyBag)
        {
            memberInfo = FindIndexerPropertyInfo();
        }
        else
        {
            memberInfo = ClrType.GetMembersInHierarchy(name).FirstOrDefault();
        }

        if (memberInfo != null
            && propertyType != memberInfo.GetMemberType()
            && memberInfo != FindIndexerPropertyInfo())
        {
            if (typeConfigurationSource != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.PropertyWrongClrType(
                        name,
                        DisplayName(),
                        memberInfo.GetMemberType().ShortDisplayName(),
                        propertyType.ShortDisplayName()));
            }

            propertyType = memberInfo.GetMemberType();
        }

        var property = new Property(
            name, propertyType, memberInfo as PropertyInfo, memberInfo as FieldInfo, this,
            configurationSource, typeConfigurationSource);

        _properties.Add(property.Name, property);

        Model.AddProperty(property);

        if (Model.Configuration != null)
        {
            using (Model.ConventionDispatcher.DelayConventions())
            {
                Model.ConventionDispatcher.OnPropertyAdded(property.Builder);
                Model.Configuration.ConfigureProperty(property);
                return property;
            }
        }

        return (Property?)Model.ConventionDispatcher.OnPropertyAdded(property.Builder)?.Metadata;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Property? FindProperty(string name)
        => FindDeclaredProperty(Check.NotEmpty(name, nameof(name))) ?? _baseType?.FindProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Property? FindDeclaredProperty(string name)
        => _properties.TryGetValue(Check.NotEmpty(name, nameof(name)), out var property)
            ? property
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Property> GetDeclaredProperties()
        => _properties.Values;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Property> GetDerivedProperties()
        => _directlyDerivedTypes.Count == 0
            ? Enumerable.Empty<Property>()
            : GetDerivedTypes().SelectMany(et => et.GetDeclaredProperties());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Property> FindDerivedProperties(string propertyName)
    {
        Check.NotNull(propertyName, nameof(propertyName));

        return _directlyDerivedTypes.Count == 0
            ? Enumerable.Empty<Property>()
            : (IEnumerable<Property>)GetDerivedTypes().Select(et => et.FindDeclaredProperty(propertyName)).Where(p => p != null);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Property> FindDerivedPropertiesInclusive(string propertyName)
        => _directlyDerivedTypes.Count == 0
            ? ToEnumerable(FindDeclaredProperty(propertyName))
            : ToEnumerable(FindDeclaredProperty(propertyName)).Concat(FindDerivedProperties(propertyName));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Property> FindPropertiesInHierarchy(string propertyName)
        => _directlyDerivedTypes.Count == 0
            ? ToEnumerable(FindProperty(propertyName))
            : ToEnumerable(FindProperty(propertyName)).Concat(FindDerivedProperties(propertyName));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<Property>? FindProperties(IReadOnlyList<string> propertyNames)
    {
        Check.NotNull(propertyNames, nameof(propertyNames));

        var properties = new List<Property>(propertyNames.Count);
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
    public virtual Property? RemoveProperty(string name)
    {
        Check.NotEmpty(name, nameof(name));

        var property = FindDeclaredProperty(name);
        return property == null
            ? null
            : RemoveProperty(property);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Property? RemoveProperty(Property property)
    {
        Check.NotNull(property, nameof(property));
        Check.DebugAssert(IsInModel, "The entity type has been removed from the model");
        EnsureMutable();

        if (property.DeclaringType != this)
        {
            throw new InvalidOperationException(
                CoreStrings.PropertyWrongType(
                    property.Name,
                    DisplayName(),
                    property.DeclaringType.DisplayName()));
        }

        CheckPropertyNotInUse(property);

        var removed = _properties.Remove(property.Name);
        Check.DebugAssert(removed, "removed is false");

        property.SetRemovedFromModel();

        return (Property?)Model.ConventionDispatcher.OnPropertyRemoved(BaseBuilder, property);
    }

    private void CheckPropertyNotInUse(Property property)
    {
        var containingKey = property.Keys?.FirstOrDefault();
        if (containingKey != null)
        {
            throw new InvalidOperationException(
                CoreStrings.PropertyInUseKey(property.Name, DisplayName(), containingKey.Properties.Format()));
        }

        var containingForeignKey = property.ForeignKeys?.FirstOrDefault();
        if (containingForeignKey != null)
        {
            throw new InvalidOperationException(
                CoreStrings.PropertyInUseForeignKey(
                    property.Name, DisplayName(),
                    containingForeignKey.Properties.Format(), containingForeignKey.DeclaringEntityType.DisplayName()));
        }

        var containingIndex = property.Indexes?.FirstOrDefault();
        if (containingIndex != null)
        {
            throw new InvalidOperationException(
                CoreStrings.PropertyInUseIndex(
                    property.Name, DisplayName(),
                    containingIndex.DisplayName(), containingIndex.DeclaringEntityType.DisplayName()));
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Property> GetProperties()
        => _baseType != null
            ? _baseType.GetProperties().Concat(_properties.Values)
            : _properties.Values;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SortedDictionary<string, Property> Properties
        => _properties;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyDictionary<string, PropertyInfo> GetRuntimeProperties()
    {
        if (_runtimeProperties == null)
        {
            var runtimeProperties = new SortedDictionary<string, PropertyInfo>(StringComparer.Ordinal);
            foreach (var property in ClrType.GetRuntimeProperties())
            {
                if (!property.IsStatic()
                    && !runtimeProperties.ContainsKey(property.Name))
                {
                    runtimeProperties[property.Name] = property;
                }
            }

            Interlocked.CompareExchange(ref _runtimeProperties, runtimeProperties, null);
        }

        return _runtimeProperties;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyDictionary<string, FieldInfo> GetRuntimeFields()
    {
        if (_runtimeFields == null)
        {
            var runtimeFields = new SortedDictionary<string, FieldInfo>(StringComparer.Ordinal);
            foreach (var field in ClrType.GetRuntimeFields())
            {
                if (!field.IsStatic
                    && !runtimeFields.ContainsKey(field.Name))
                {
                    runtimeFields[field.Name] = field;
                }
            }

            Interlocked.CompareExchange(ref _runtimeFields, runtimeFields, null);
        }

        return _runtimeFields;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PropertyInfo? FindIndexerPropertyInfo()
    {
        if (!_indexerPropertyInitialized)
        {
            var indexerPropertyInfo = ClrType.FindIndexerProperty();

            Interlocked.CompareExchange(ref _indexerPropertyInfo, indexerPropertyInfo, null);
            _indexerPropertyInitialized = true;
        }

        return _indexerPropertyInfo;
    }

    #endregion

    #region Complex properties

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ComplexProperty? AddComplexProperty(
        string name,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type targetType,
        bool collection,
        ConfigurationSource configurationSource)
    {
        Check.NotNull(name, nameof(name));
        Check.NotNull(propertyType, nameof(propertyType));
        Check.NotNull(targetType, nameof(targetType));

        return AddComplexProperty(
            name,
            propertyType,
            memberInfo: null,
            complexTypeName: null,
            targetType,
            collection,
            configurationSource);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [RequiresUnreferencedCode("Use an overload that accepts a type")]
    public virtual ComplexProperty? AddComplexProperty(
        MemberInfo memberInfo,
        bool collection,
        ConfigurationSource configurationSource)
        => AddComplexProperty(
            memberInfo.GetSimpleMemberName(),
            memberInfo.GetMemberType(),
            memberInfo,
            complexTypeName: null,
            collection ? memberInfo.GetMemberType().TryGetSequenceType()! : memberInfo.GetMemberType(),
            collection,
            configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [RequiresUnreferencedCode("Use an overload that accepts a type")]
    public virtual ComplexProperty? AddComplexProperty(
        string name,
        bool collection,
        ConfigurationSource configurationSource)
    {
        MemberInfo? clrMember;
        if (IsPropertyBag)
        {
            clrMember = FindIndexerPropertyInfo()!;
        }
        else
        {
            clrMember = ClrType.GetMembersInHierarchy(name).FirstOrDefault();
            if (clrMember == null)
            {
                throw new InvalidOperationException(CoreStrings.NoPropertyType(name, DisplayName()));
            }
        }

        return AddComplexProperty(clrMember, collection, configurationSource);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ComplexProperty? AddComplexProperty(
        string name,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType,
        MemberInfo? memberInfo,
        string? complexTypeName,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type targetType,
        bool collection,
        ConfigurationSource configurationSource)
    {
        Check.NotNull(name, nameof(name));
        Check.NotNull(propertyType, nameof(propertyType));
        Check.NotNull(targetType, nameof(targetType));
        Check.DebugAssert(IsInModel, "The entity type has been removed from the model");
        EnsureMutable();

        var conflictingMember = FindMembersInHierarchy(name).FirstOrDefault();
        if (conflictingMember != null)
        {
            throw new InvalidOperationException(
                CoreStrings.ConflictingPropertyOrNavigation(
                    name, DisplayName(),
                    conflictingMember.DeclaringType.DisplayName()));
        }

        if (memberInfo != null)
        {
            propertyType = ValidateClrMember(name, memberInfo)
                ?? propertyType;

            if (memberInfo.DeclaringType?.IsAssignableFrom(ClrType) != true)
            {
                throw new InvalidOperationException(
                    CoreStrings.PropertyWrongEntityClrType(
                        memberInfo.Name, DisplayName(), memberInfo.DeclaringType?.ShortDisplayName()));
            }
        }
        else if (IsPropertyBag)
        {
            memberInfo = FindIndexerPropertyInfo();
        }
        else
        {
            memberInfo = ClrType.GetMembersInHierarchy(name).FirstOrDefault();
        }

        if (memberInfo != null)
        {
            if (propertyType != memberInfo.GetMemberType()
                && memberInfo != FindIndexerPropertyInfo())
            {
                throw new InvalidOperationException(
                    CoreStrings.PropertyWrongClrType(
                        name,
                        DisplayName(),
                        memberInfo.GetMemberType().ShortDisplayName(),
                        propertyType.ShortDisplayName()));
            }

            ComplexProperty.IsCompatible(
                name,
                memberInfo,
                this,
                targetType,
                collection,
                shouldThrow: true);
        }

        var property = new ComplexProperty(
            name, propertyType, memberInfo as PropertyInfo, memberInfo as FieldInfo, this,
            complexTypeName, targetType, collection, configurationSource);

        _complexProperties.Add(property.Name, property);

        if (Model.Configuration != null)
        {
            using (Model.ConventionDispatcher.DelayConventions())
            {
                property = (ComplexProperty)Model.ConventionDispatcher.OnComplexPropertyAdded(property.Builder)!.Metadata;
                Model.Configuration.ConfigureComplexProperty(property);
                return property;
            }
        }

        return (ComplexProperty?)Model.ConventionDispatcher.OnComplexPropertyAdded(property.Builder)?.Metadata;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ComplexProperty? FindComplexProperty(string name)
        => FindDeclaredComplexProperty(Check.NotEmpty(name, nameof(name))) ?? BaseType?.FindComplexProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ComplexProperty? FindDeclaredComplexProperty(string name)
        => _complexProperties.TryGetValue(Check.NotEmpty(name, nameof(name)), out var property)
            ? property
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ComplexProperty> GetDeclaredComplexProperties()
        => _complexProperties.Values;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ComplexProperty> GetDerivedComplexProperties()
        => _directlyDerivedTypes.Count == 0
            ? Enumerable.Empty<ComplexProperty>()
            : GetDerivedTypes().SelectMany(et => et.GetDeclaredComplexProperties());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ComplexProperty> FindDerivedComplexProperties(string propertyName)
    {
        Check.NotNull(propertyName, nameof(propertyName));

        return _directlyDerivedTypes.Count == 0
            ? Enumerable.Empty<ComplexProperty>()
            : (IEnumerable<ComplexProperty>)GetDerivedTypes()
                .Select(et => et.FindDeclaredComplexProperty(propertyName)).Where(p => p != null);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ComplexProperty> FindDerivedComplexPropertiesInclusive(string propertyName)
        => _directlyDerivedTypes.Count == 0
            ? ToEnumerable(FindDeclaredComplexProperty(propertyName))
            : ToEnumerable(FindDeclaredComplexProperty(propertyName)).Concat(FindDerivedComplexProperties(propertyName));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ComplexProperty> FindComplexPropertiesInHierarchy(string propertyName)
        => _directlyDerivedTypes.Count == 0
            ? ToEnumerable(FindComplexProperty(propertyName))
            : ToEnumerable(FindComplexProperty(propertyName)).Concat(FindDerivedComplexProperties(propertyName));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ComplexProperty? RemoveComplexProperty(string name)
    {
        Check.NotEmpty(name, nameof(name));

        var property = FindDeclaredComplexProperty(name);
        return property == null
            ? null
            : RemoveComplexProperty(property);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ComplexProperty? RemoveComplexProperty(ComplexProperty property)
    {
        Check.NotNull(property, nameof(property));
        Check.DebugAssert(IsInModel, "The entity type has been removed from the model");
        EnsureMutable();

        if (property.DeclaringType != this)
        {
            throw new InvalidOperationException(
                CoreStrings.PropertyWrongType(
                    property.Name,
                    DisplayName(),
                    property.DeclaringType.DisplayName()));
        }

        CheckPropertyNotInUse(property);
        var removed = _complexProperties.Remove(property.Name);
        Check.DebugAssert(removed, "removed is false");

        property.SetRemovedFromModel();

        return (ComplexProperty?)Model.ConventionDispatcher.OnComplexPropertyRemoved(BaseBuilder, property);
    }

    private void CheckPropertyNotInUse(ComplexProperty property)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ComplexProperty> GetComplexProperties()
        => BaseType != null
            ? BaseType.GetComplexProperties().Concat(_complexProperties.Values)
            : _complexProperties.Values;

    #endregion

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PropertyAccessMode GetPropertyAccessMode()
        => (PropertyAccessMode?)this[CoreAnnotationNames.PropertyAccessMode]
            ?? Model.GetPropertyAccessMode();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PropertyAccessMode? SetPropertyAccessMode(
        PropertyAccessMode? propertyAccessMode,
        ConfigurationSource configurationSource)
        => (PropertyAccessMode?)SetOrRemoveAnnotation(
            CoreAnnotationNames.PropertyAccessMode, propertyAccessMode, configurationSource)?.Value;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    public virtual ChangeTrackingStrategy GetChangeTrackingStrategy()
        => _changeTrackingStrategy ?? Model.GetChangeTrackingStrategy();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ChangeTrackingStrategy? SetChangeTrackingStrategy(
        ChangeTrackingStrategy? changeTrackingStrategy,
        ConfigurationSource configurationSource)
    {
        EnsureMutable();

        if (changeTrackingStrategy != null)
        {
            var requireFullNotifications =
                (bool?)Model[CoreAnnotationNames.FullChangeTrackingNotificationsRequired] == true;
            var errorMessage = CheckChangeTrackingStrategy(this, changeTrackingStrategy.Value, requireFullNotifications);
            if (errorMessage != null)
            {
                throw new InvalidOperationException(errorMessage);
            }
        }

        _changeTrackingStrategy = changeTrackingStrategy;

        _changeTrackingStrategyConfigurationSource = _changeTrackingStrategy == null
            ? null
            : configurationSource.Max(_changeTrackingStrategyConfigurationSource);

        return changeTrackingStrategy;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string? CheckChangeTrackingStrategy(
        IReadOnlyTypeBase typeBase,
        ChangeTrackingStrategy value,
        bool requireFullNotifications)
    {
        if (requireFullNotifications)
        {
            if (value != ChangeTrackingStrategy.ChangingAndChangedNotifications
                && value != ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)
            {
                return CoreStrings.FullChangeTrackingRequired(
                    typeBase.DisplayName(), value, nameof(ChangeTrackingStrategy.ChangingAndChangedNotifications),
                    nameof(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues));
            }
        }
        else
        {
            if (value != ChangeTrackingStrategy.Snapshot
                && !typeof(INotifyPropertyChanged).IsAssignableFrom(typeBase.ClrType))
            {
                return CoreStrings.ChangeTrackingInterfaceMissing(typeBase.DisplayName(), value, nameof(INotifyPropertyChanged));
            }

            if ((value == ChangeTrackingStrategy.ChangingAndChangedNotifications
                    || value == ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)
                && !typeof(INotifyPropertyChanging).IsAssignableFrom(typeBase.ClrType))
            {
                return CoreStrings.ChangeTrackingInterfaceMissing(typeBase.DisplayName(), value, nameof(INotifyPropertyChanging));
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
    public virtual ConfigurationSource? GetChangeTrackingStrategyConfigurationSource()
        => _changeTrackingStrategyConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? AddIgnored(string name, ConfigurationSource configurationSource)
    {
        Check.NotNull(name, nameof(name));
        EnsureMutable();

        if (_ignoredMembers.TryGetValue(name, out var existingIgnoredConfigurationSource))
        {
            _ignoredMembers[name] = configurationSource.Max(existingIgnoredConfigurationSource);
            return name;
        }

        _ignoredMembers[name] = configurationSource;

        return OnTypeMemberIgnored(name);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract string? OnTypeMemberIgnored(string name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<string> GetIgnoredMembers()
        => _ignoredMembers.Keys;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? FindDeclaredIgnoredConfigurationSource(string name)
        => _ignoredMembers.TryGetValue(Check.NotEmpty(name, nameof(name)), out var ignoredConfigurationSource)
            ? ignoredConfigurationSource
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? FindIgnoredConfigurationSource(string name)
    {
        var ignoredSource = FindDeclaredIgnoredConfigurationSource(name);

        return BaseType == null ? ignoredSource : BaseType.FindIgnoredConfigurationSource(name).Max(ignoredSource);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsIgnored(string name)
        => FindIgnoredConfigurationSource(name) != null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? RemoveIgnored(string name)
    {
        Check.NotNull(name, nameof(name));
        EnsureMutable();

        return _ignoredMembers.Remove(name) ? name : null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private string DisplayName()
        => ((IReadOnlyTypeBase)this).DisplayName();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract InstantiationBinding? ConstructorBinding { get; set; }

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

    /// <summary>
    ///     Returns all <see cref="IProperty" /> members from this type and all nested complex types, if any.
    /// </summary>
    /// <returns>The properties.</returns>
    public virtual IEnumerable<Property> GetFlattenedProperties()
    {
        foreach (var property in GetProperties())
        {
            yield return property;
        }

        foreach (var complexProperty in GetComplexProperties())
        {
            foreach (var property in complexProperty.ComplexType.GetFlattenedProperties())
            {
                yield return property;
            }
        }
    }

    /// <summary>
    ///     Returns all <see cref="ComplexProperty" /> members from this type and all nested complex types, if any.
    /// </summary>
    /// <returns>The properties.</returns>
    public virtual IEnumerable<ComplexProperty> GetFlattenedComplexProperties()
    {
        foreach (var complexProperty in GetComplexProperties())
        {
            yield return complexProperty;

            foreach (var nestedComplexProperty in complexProperty.ComplexType.GetFlattenedComplexProperties())
            {
                yield return nestedComplexProperty;
            }
        }
    }

    /// <summary>
    ///     Returns all <see cref="IProperty" /> members from this type and all nested complex types, if any.
    /// </summary>
    /// <returns>The properties.</returns>
    public virtual IEnumerable<Property> GetFlattenedDeclaredProperties()
    {
        foreach (var property in GetDeclaredProperties())
        {
            yield return property;
        }

        foreach (var complexProperty in GetDeclaredComplexProperties())
        {
            foreach (var property in complexProperty.ComplexType.GetFlattenedDeclaredProperties())
            {
                yield return property;
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<PropertyBase> GetSnapshottableMembers()
    {
        foreach (var property in GetProperties())
        {
            yield return property;
        }

        foreach (var complexProperty in GetComplexProperties())
        {
            yield return complexProperty;

            foreach (var propertyBase in complexProperty.ComplexType.GetSnapshottableMembers())
            {
                yield return propertyBase;
            }
        }
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
    IConventionModel IConventionTypeBase.Model
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
    [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)]
    Type IReadOnlyTypeBase.ClrType
    {
        [DebuggerStepThrough]
        get => ClrType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionTypeBaseBuilder IConventionTypeBase.Builder
    {
        [DebuggerStepThrough]
        get => BaseBuilder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    string? IMutableTypeBase.AddIgnored(string name)
        => AddIgnored(name, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    string? IConventionTypeBase.AddIgnored(string name, bool fromDataAnnotation)
        => AddIgnored(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableProperty IMutableTypeBase.AddProperty(string name)
        => AddProperty(name, ConfigurationSource.Explicit)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionProperty? IConventionTypeBase.AddProperty(string name, bool fromDataAnnotation)
        => AddProperty(
            name,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableProperty IMutableTypeBase.AddProperty(
        string name,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType)
        => AddProperty(
            name,
            propertyType,
            ConfigurationSource.Explicit,
            ConfigurationSource.Explicit)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionProperty? IConventionTypeBase.AddProperty(
        string name,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType,
        bool setTypeConfigurationSource,
        bool fromDataAnnotation)
        => AddProperty(
            name,
            propertyType,
            setTypeConfigurationSource
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
    IMutableProperty IMutableTypeBase.AddProperty(
        string name,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType,
        MemberInfo memberInfo)
        => AddProperty(
            name, propertyType, memberInfo,
            ConfigurationSource.Explicit, ConfigurationSource.Explicit)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionProperty? IConventionTypeBase.AddProperty(
        string name,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType,
        MemberInfo? memberInfo,
        bool setTypeConfigurationSource,
        bool fromDataAnnotation)
        => AddProperty(
            name,
            propertyType,
            memberInfo,
            setTypeConfigurationSource
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
    IReadOnlyProperty? IReadOnlyTypeBase.FindDeclaredProperty(string name)
        => FindDeclaredProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IProperty? ITypeBase.FindDeclaredProperty(string name)
        => FindDeclaredProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IReadOnlyList<IReadOnlyProperty>? IReadOnlyTypeBase.FindProperties(IReadOnlyList<string> propertyNames)
        => FindProperties(propertyNames);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IReadOnlyProperty? IReadOnlyTypeBase.FindProperty(string name)
        => FindProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableProperty? IMutableTypeBase.FindProperty(string name)
        => FindProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionProperty? IConventionTypeBase.FindProperty(string name)
        => FindProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IProperty? ITypeBase.FindProperty(string name)
        => FindProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyProperty> IReadOnlyTypeBase.GetDeclaredProperties()
        => GetDeclaredProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IMutableProperty> IMutableTypeBase.GetDeclaredProperties()
        => GetDeclaredProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IConventionProperty> IConventionTypeBase.GetDeclaredProperties()
        => GetDeclaredProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IProperty> ITypeBase.GetDeclaredProperties()
        => GetDeclaredProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyProperty> IReadOnlyTypeBase.GetDerivedProperties()
        => GetDerivedProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyProperty> IReadOnlyTypeBase.GetProperties()
        => GetProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IMutableProperty> IMutableTypeBase.GetProperties()
        => GetProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IConventionProperty> IConventionTypeBase.GetProperties()
        => GetProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IProperty> ITypeBase.GetProperties()
        => GetProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableProperty? IMutableTypeBase.RemoveProperty(string name)
        => RemoveProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionProperty? IConventionTypeBase.RemoveProperty(string name)
        => RemoveProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableProperty? IMutableTypeBase.RemoveProperty(IReadOnlyProperty property)
        => RemoveProperty((Property)property);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionProperty? IConventionTypeBase.RemoveProperty(IReadOnlyProperty property)
        => RemoveProperty((Property)property);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableComplexProperty IMutableTypeBase.AddComplexProperty(string name, bool collection)
        => AddComplexProperty(name, collection, ConfigurationSource.Explicit)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionComplexProperty? IConventionTypeBase.AddComplexProperty(string name, bool collection, bool fromDataAnnotation)
        => AddComplexProperty(
            name,
            collection,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableComplexProperty IMutableTypeBase.AddComplexProperty(
        string name,
        Type propertyType,
        Type targetType,
        string? complexTypeName,
        bool collection)
        => AddComplexProperty(
            name, propertyType, memberInfo: null, complexTypeName, targetType, collection,
            ConfigurationSource.Explicit)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionComplexProperty? IConventionTypeBase.AddComplexProperty(
        string name,
        Type propertyType,
        Type targetType,
        string? complexTypeName,
        bool collection,
        bool fromDataAnnotation)
        => AddComplexProperty(
            name, propertyType, memberInfo: null, complexTypeName, targetType, collection,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableComplexProperty IMutableTypeBase.AddComplexProperty(
        string name,
        Type propertyType,
        MemberInfo memberInfo,
        Type targetType,
        string? complexTypeName,
        bool collection)
        => AddComplexProperty(name, propertyType, memberInfo, complexTypeName, targetType, collection, ConfigurationSource.Explicit)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionComplexProperty? IConventionTypeBase.AddComplexProperty(
        string name,
        Type propertyType,
        MemberInfo memberInfo,
        Type targetType,
        string? complexTypeName,
        bool collection,
        bool fromDataAnnotation)
        => AddComplexProperty(
            name, propertyType, memberInfo, complexTypeName, targetType, collection,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IReadOnlyComplexProperty? IReadOnlyTypeBase.FindComplexProperty(string name)
        => FindComplexProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableComplexProperty? IMutableTypeBase.FindComplexProperty(string name)
        => FindComplexProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionComplexProperty? IConventionTypeBase.FindComplexProperty(string name)
        => FindComplexProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IComplexProperty? ITypeBase.FindComplexProperty(string name)
        => FindComplexProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IReadOnlyComplexProperty? IReadOnlyTypeBase.FindDeclaredComplexProperty(string name)
        => FindDeclaredComplexProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyComplexProperty> IReadOnlyTypeBase.GetComplexProperties()
        => GetComplexProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IMutableComplexProperty> IMutableTypeBase.GetComplexProperties()
        => GetComplexProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IConventionComplexProperty> IConventionTypeBase.GetComplexProperties()
        => GetComplexProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IComplexProperty> ITypeBase.GetComplexProperties()
        => GetComplexProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyComplexProperty> IReadOnlyTypeBase.GetDeclaredComplexProperties()
        => GetDeclaredComplexProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IMutableComplexProperty> IMutableTypeBase.GetDeclaredComplexProperties()
        => GetDeclaredComplexProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IConventionComplexProperty> IConventionTypeBase.GetDeclaredComplexProperties()
        => GetDeclaredComplexProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IComplexProperty> ITypeBase.GetDeclaredComplexProperties()
        => GetDeclaredComplexProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyComplexProperty> IReadOnlyTypeBase.GetDerivedComplexProperties()
        => GetDerivedComplexProperties();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableComplexProperty? IMutableTypeBase.RemoveComplexProperty(string name)
        => RemoveComplexProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionComplexProperty? IConventionTypeBase.RemoveComplexProperty(string name)
        => RemoveComplexProperty(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableComplexProperty? IMutableTypeBase.RemoveComplexProperty(IReadOnlyProperty property)
        => RemoveComplexProperty((ComplexProperty)property);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionComplexProperty? IConventionTypeBase.RemoveComplexProperty(IConventionComplexProperty property)
        => RemoveComplexProperty((ComplexProperty)property);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableTypeBase.SetChangeTrackingStrategy(ChangeTrackingStrategy? changeTrackingStrategy)
        => SetChangeTrackingStrategy(changeTrackingStrategy, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    ChangeTrackingStrategy? IConventionTypeBase.SetChangeTrackingStrategy(
        ChangeTrackingStrategy? changeTrackingStrategy,
        bool fromDataAnnotation)
        => SetChangeTrackingStrategy(
            changeTrackingStrategy, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IPropertyBase> ITypeBase.GetMembers()
        => GetMembers();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IMutablePropertyBase> IMutableTypeBase.GetMembers()
        => GetMembers();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IConventionPropertyBase> IConventionTypeBase.GetMembers()
        => GetMembers();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyPropertyBase> IReadOnlyTypeBase.GetMembers()
        => GetMembers();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyPropertyBase> IReadOnlyTypeBase.GetDeclaredMembers()
        => GetDeclaredMembers();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IMutablePropertyBase> IMutableTypeBase.GetDeclaredMembers()
        => GetDeclaredMembers();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IConventionPropertyBase> IConventionTypeBase.GetDeclaredMembers()
        => GetDeclaredMembers();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IPropertyBase> ITypeBase.GetDeclaredMembers()
        => GetDeclaredMembers();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IReadOnlyPropertyBase? IReadOnlyTypeBase.FindMember(string name)
        => FindMember(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutablePropertyBase? IMutableTypeBase.FindMember(string name)
        => FindMember(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionPropertyBase? IConventionTypeBase.FindMember(string name)
        => FindMember(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IPropertyBase? ITypeBase.FindMember(string name)
        => FindMember(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyPropertyBase> IReadOnlyTypeBase.FindMembersInHierarchy(string name)
        => FindMembersInHierarchy(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IMutablePropertyBase> IMutableTypeBase.FindMembersInHierarchy(string name)
        => FindMembersInHierarchy(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IConventionPropertyBase> IConventionTypeBase.FindMembersInHierarchy(string name)
        => FindMembersInHierarchy(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IPropertyBase> ITypeBase.FindMembersInHierarchy(string name)
        => FindMembersInHierarchy(name);

    /// <summary>
    ///     Returns all properties that implement <see cref="IProperty" />, including those on complex types.
    /// </summary>
    /// <returns>The properties.</returns>
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
