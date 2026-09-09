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
public abstract class PropertyBase : ConventionAnnotatable, IMutablePropertyBase, IConventionPropertyBase, IRuntimePropertyBase
{
    private FieldInfo? _fieldInfo;
    private ConfigurationSource _configurationSource;
    private ConfigurationSource? _fieldInfoConfigurationSource;

    // Warning: Never access these fields directly as access needs to be thread-safe
    private IClrPropertyGetter? _getter;
    private IClrPropertySetter? _setter;
    private IClrPropertySetter? _materializationSetter;
    private PropertyAccessors? _accessors;
    private PropertyIndexes? _indexes;
    private IComparer<IUpdateEntry>? _currentValueComparer;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected PropertyBase(
        string name,
        PropertyInfo? propertyInfo,
        FieldInfo? fieldInfo,
        ConfigurationSource configurationSource)
    {
        Name = name;
        PropertyInfo = propertyInfo;
        _fieldInfo = fieldInfo;
        _configurationSource = configurationSource;
    }

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
    [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)]
    public abstract Type ClrType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool IsReadOnly
        => DeclaringType.Model.IsReadOnly;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract TypeBase DeclaringType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PropertyInfo? PropertyInfo { [DebuggerStepThrough] get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual FieldInfo? FieldInfo
    {
        [DebuggerStepThrough]
        get => _fieldInfo;
        [DebuggerStepThrough]
        set => SetFieldInfo(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
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

    // Needed for a workaround before reference counting is implemented
    // Issue #15898
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetConfigurationSource(ConfigurationSource configurationSource)
        => _configurationSource = configurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual FieldInfo? SetField(string? fieldName, ConfigurationSource configurationSource)
    {
        if (fieldName == null)
        {
            return SetFieldInfo(null, configurationSource);
        }

        if (FieldInfo?.GetSimpleMemberName() == fieldName)
        {
            return SetFieldInfo(FieldInfo, configurationSource);
        }

        var fieldInfo = GetFieldInfo(fieldName, DeclaringType, Name, shouldThrow: true);
        return SetFieldInfo(fieldInfo, configurationSource);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static FieldInfo? GetFieldInfo(
        string fieldName,
        TypeBase type,
        string propertyName,
        bool shouldThrow)
    {
        if (!type.GetRuntimeFields().TryGetValue(fieldName, out var fieldInfo)
            && shouldThrow)
        {
            throw new InvalidOperationException(
                CoreStrings.MissingBackingField(fieldName, propertyName, type.DisplayName()));
        }

        return fieldInfo;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual FieldInfo? SetFieldInfo(FieldInfo? fieldInfo, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        if (Equals(FieldInfo, fieldInfo))
        {
            if (fieldInfo != null)
            {
                UpdateFieldInfoConfigurationSource(configurationSource);
            }

            return fieldInfo;
        }

        if (fieldInfo != null)
        {
            IsCompatible(fieldInfo, ClrType, DeclaringType.ClrType, Name, shouldThrow: true);

            if (PropertyInfo != null
                && PropertyInfo.IsIndexerProperty())
            {
                throw new InvalidOperationException(
                    CoreStrings.BackingFieldOnIndexer(fieldInfo.GetSimpleMemberName(), DeclaringType.DisplayName(), Name));
            }

            UpdateFieldInfoConfigurationSource(configurationSource);
        }
        else
        {
            _fieldInfoConfigurationSource = null;
        }

        if (PropertyInfo == null
            && fieldInfo?.GetSimpleMemberName() != Name)
        {
            throw new InvalidOperationException(
                CoreStrings.FieldNameMismatch(fieldInfo?.GetSimpleMemberName(), DeclaringType.DisplayName(), Name));
        }

        var oldFieldInfo = FieldInfo;
        _fieldInfo = fieldInfo;

        return OnFieldInfoSet(fieldInfo, oldFieldInfo);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual FieldInfo? OnFieldInfoSet(FieldInfo? newFieldInfo, FieldInfo? oldFieldInfo)
        => newFieldInfo;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetFieldInfoConfigurationSource()
        => _fieldInfoConfigurationSource;

    private void UpdateFieldInfoConfigurationSource(ConfigurationSource configurationSource)
        => _fieldInfoConfigurationSource = configurationSource.Max(_fieldInfoConfigurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PropertyAccessMode? SetPropertyAccessMode(
        PropertyAccessMode? propertyAccessMode,
        ConfigurationSource configurationSource)
        => (PropertyAccessMode?)SetOrRemoveAnnotation(CoreAnnotationNames.PropertyAccessMode, propertyAccessMode, configurationSource)
            ?.Value;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PropertyAccessMode GetPropertyAccessMode()
        => (PropertyAccessMode)(this[CoreAnnotationNames.PropertyAccessMode]
            ?? ((IReadOnlyTypeBase)DeclaringType).GetPropertyAccessMode());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetPropertyAccessModeConfigurationSource()
        => FindAnnotation(CoreAnnotationNames.PropertyAccessMode)?.GetConfigurationSource();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsCompatible(
        FieldInfo fieldInfo,
        Type? propertyType,
        Type? entityType,
        string? propertyName,
        bool shouldThrow)
    {
        Check.DebugAssert(propertyName != null || !shouldThrow, "propertyName is null");

        if (entityType == null
            || !fieldInfo.DeclaringType!.IsAssignableFrom(entityType))
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    CoreStrings.MissingBackingField(fieldInfo.Name, propertyName, entityType?.ShortDisplayName()));
            }

            return false;
        }

        var fieldType = fieldInfo.FieldType;
        if (propertyType != null
            && !propertyType.IsCompatibleWith(fieldType))
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException(
                    CoreStrings.BadBackingFieldType(
                        fieldInfo.Name,
                        fieldInfo.FieldType.ShortDisplayName(),
                        entityType.ShortDisplayName(),
                        propertyName,
                        propertyType.ShortDisplayName()));
            }

            return false;
        }

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PropertyIndexes PropertyIndexes
    {
        get => NonCapturingLazyInitializer.EnsureInitialized(
            ref _indexes, this,
            static property =>
            {
                property.EnsureReadOnly();
                _ = ((IRuntimeEntityType)(((IRuntimeTypeBase)property.DeclaringType).ContainingEntityType)).Counts;
            });

        set => NonCapturingLazyInitializer.EnsureInitialized(ref _indexes, value);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IClrPropertyGetter Getter
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _getter, this, static property =>
            {
                property.EnsureReadOnly();
                return ClrPropertyGetterFactory.Instance.Create(property);
            });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IClrPropertySetter GetSetter()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _setter, this, static property =>
            {
                property.EnsureReadOnly();
                return ClrPropertySetterFactory.Instance.Create(property);
            });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IClrPropertySetter MaterializationSetter
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _materializationSetter, this, static property =>
            {
                property.EnsureReadOnly();
                return ClrPropertyMaterializationSetterFactory.Instance.Create(property);
            });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PropertyAccessors Accessors
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _accessors, this, static property =>
            {
                property.EnsureReadOnly();
                return PropertyAccessorsFactory.Instance.Create(property);
            });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IComparer<IUpdateEntry> GetCurrentValueComparer()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _currentValueComparer, this, static property =>
            {
                property.EnsureReadOnly();
                return CurrentValueComparerFactory.Instance.Create(property);
            });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetCurrentValueComparer(IComparer<IUpdateEntry> comparer)
        => _currentValueComparer = comparer;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IReadOnlyTypeBase IReadOnlyPropertyBase.DeclaringType
    {
        [DebuggerStepThrough]
        get => DeclaringType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IMutableTypeBase IMutablePropertyBase.DeclaringType
    {
        [DebuggerStepThrough]
        get => DeclaringType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionTypeBase IConventionPropertyBase.DeclaringType
    {
        [DebuggerStepThrough]
        get => DeclaringType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutablePropertyBase.SetField(string? fieldName)
        => SetField(fieldName, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    FieldInfo? IConventionPropertyBase.SetField(string? fieldName, bool fromDataAnnotation)
        => SetField(fieldName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    FieldInfo? IConventionPropertyBase.SetFieldInfo(FieldInfo? fieldInfo, bool fromDataAnnotation)
        => SetFieldInfo(fieldInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutablePropertyBase.SetPropertyAccessMode(PropertyAccessMode? propertyAccessMode)
        => SetPropertyAccessMode(propertyAccessMode, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    PropertyAccessMode? IConventionPropertyBase.SetPropertyAccessMode(
        PropertyAccessMode? propertyAccessMode,
        bool fromDataAnnotation)
        => SetPropertyAccessMode(
            propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IClrPropertyGetter IPropertyBase.GetGetter()
        => Getter;

    /// <summary>
    ///     Gets the sentinel value that indicates that this property is not set.
    /// </summary>
    object? IReadOnlyPropertyBase.Sentinel
        => null;
}
