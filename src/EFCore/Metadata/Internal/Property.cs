// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class Property : PropertyBase, IMutableProperty, IConventionProperty, IProperty
{
    private InternalPropertyBuilder? _builder;

    private bool? _isConcurrencyToken;
    private bool? _isNullable;
    private object? _sentinel;
    private ValueGenerated? _valueGenerated;
    private CoreTypeMapping? _typeMapping;

    private ConfigurationSource? _typeConfigurationSource;
    private ConfigurationSource? _isNullableConfigurationSource;
    private ConfigurationSource? _sentinelConfigurationSource;
    private ConfigurationSource? _isConcurrencyTokenConfigurationSource;
    private ConfigurationSource? _valueGeneratedConfigurationSource;
    private ConfigurationSource? _typeMappingConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Property(
        string name,
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type clrType,
        PropertyInfo? propertyInfo,
        FieldInfo? fieldInfo,
        TypeBase declaringType,
        ConfigurationSource configurationSource,
        ConfigurationSource? typeConfigurationSource)
        : base(name, propertyInfo, fieldInfo, configurationSource)
    {
        DeclaringType = declaringType;
        ClrType = clrType;
        _typeConfigurationSource = typeConfigurationSource;
        _builder = new InternalPropertyBuilder(this, declaringType.Model.Builder);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder Builder
    {
        [DebuggerStepThrough]
        get => _builder ?? throw new InvalidOperationException(CoreStrings.ObjectRemovedFromModel(Name));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsInModel
        => _builder is not null
            && DeclaringType.IsInModel;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetRemovedFromModel()
    {
        DeclaringType.Model.RemoveProperty(this);
        _builder = null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override FieldInfo? OnFieldInfoSet(FieldInfo? newFieldInfo, FieldInfo? oldFieldInfo)
        => DeclaringType.Model.ConventionDispatcher.OnPropertyFieldChanged(Builder, newFieldInfo, oldFieldInfo);

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
        => DeclaringType.Model.ConventionDispatcher.OnPropertyAnnotationChanged(Builder, name, annotation, oldAnnotation);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool AreCompatible(IReadOnlyList<Property> properties, EntityType entityType)
        => properties.All(
            property =>
                property.IsShadowProperty()
                || (property.IsIndexerProperty()
                    ? property.PropertyInfo == entityType.FindIndexerPropertyInfo()
                    : ((property.PropertyInfo != null
                            && entityType.GetRuntimeProperties().ContainsKey(property.Name))
                        || (property.FieldInfo != null
                            && entityType.GetRuntimeFields().ContainsKey(property.Name)))));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override TypeBase DeclaringType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)]
    public override Type ClrType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetTypeConfigurationSource()
        => _typeConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void UpdateTypeConfigurationSource(ConfigurationSource configurationSource)
        => _typeConfigurationSource = _typeConfigurationSource.Max(configurationSource);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsNullable
    {
        get => _isNullable ?? DefaultIsNullable;
        set => SetIsNullable(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? SetIsNullable(bool? nullable, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        var isChanging = (nullable ?? DefaultIsNullable) != IsNullable;
        if (nullable == null)
        {
            _isNullable = null;
            _isNullableConfigurationSource = null;
            if (isChanging)
            {
                OnPropertyNullableChanged();
            }

            return nullable;
        }

        if (nullable.Value)
        {
            if (!ClrType.IsNullableType())
            {
                throw new InvalidOperationException(
                    CoreStrings.CannotBeNullable(Name, DeclaringType.DisplayName(), ClrType.ShortDisplayName()));
            }

            if (Keys != null)
            {
                throw new InvalidOperationException(CoreStrings.CannotBeNullablePK(Name, DeclaringType.DisplayName()));
            }
        }

        _isNullableConfigurationSource = configurationSource.Max(_isNullableConfigurationSource);

        _isNullable = nullable;

        return isChanging
            ? OnPropertyNullableChanged()
            : nullable;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual bool? OnPropertyNullableChanged()
        => DeclaringType.Model.ConventionDispatcher.OnPropertyNullabilityChanged(Builder);

    private bool DefaultIsNullable
        => ClrType.IsNullableType();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetIsNullableConfigurationSource()
        => _isNullableConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ValueGenerated ValueGenerated
    {
        get => _valueGenerated ?? DefaultValueGenerated;
        set => SetValueGenerated(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ValueGenerated? SetValueGenerated(ValueGenerated? valueGenerated, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        _valueGenerated = valueGenerated;

        _valueGeneratedConfigurationSource = valueGenerated == null
            ? null
            : configurationSource.Max(_valueGeneratedConfigurationSource);

        return valueGenerated;
    }

    private static ValueGenerated DefaultValueGenerated
        => ValueGenerated.Never;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetValueGeneratedConfigurationSource()
        => _valueGeneratedConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsConcurrencyToken
    {
        get => _isConcurrencyToken ?? DefaultIsConcurrencyToken;
        set => SetIsConcurrencyToken(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? SetIsConcurrencyToken(bool? concurrencyToken, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        if (IsConcurrencyToken != concurrencyToken)
        {
            _isConcurrencyToken = concurrencyToken;
        }

        _isConcurrencyTokenConfigurationSource = concurrencyToken == null
            ? null
            : configurationSource.Max(_isConcurrencyTokenConfigurationSource);

        return concurrencyToken;
    }

    private static bool DefaultIsConcurrencyToken
        => false;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetIsConcurrencyTokenConfigurationSource()
        => _isConcurrencyTokenConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? SetMaxLength(int? maxLength, ConfigurationSource configurationSource)
    {
        if (maxLength is < -1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLength));
        }

        return (int?)SetOrRemoveAnnotation(CoreAnnotationNames.MaxLength, maxLength, configurationSource)?.Value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? GetMaxLength()
        => (int?)this[CoreAnnotationNames.MaxLength];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetMaxLengthConfigurationSource()
        => FindAnnotation(CoreAnnotationNames.MaxLength)?.GetConfigurationSource();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? SetIsUnicode(bool? unicode, ConfigurationSource configurationSource)
        => (bool?)SetOrRemoveAnnotation(CoreAnnotationNames.Unicode, unicode, configurationSource)?.Value;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? IsUnicode()
        => (bool?)this[CoreAnnotationNames.Unicode];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetIsUnicodeConfigurationSource()
        => FindAnnotation(CoreAnnotationNames.Unicode)?.GetConfigurationSource();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? SetPrecision(int? precision, ConfigurationSource configurationSource)
    {
        if (precision != null && precision < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(precision));
        }

        return (int?)SetOrRemoveAnnotation(CoreAnnotationNames.Precision, precision, configurationSource)?.Value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? GetPrecision()
        => (int?)this[CoreAnnotationNames.Precision];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetPrecisionConfigurationSource()
        => FindAnnotation(CoreAnnotationNames.Precision)?.GetConfigurationSource();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? SetScale(int? scale, ConfigurationSource configurationSource)
    {
        if (scale != null && scale < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(scale));
        }

        return (int?)SetOrRemoveAnnotation(CoreAnnotationNames.Scale, scale, configurationSource)?.Value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? GetScale()
        => (int?)this[CoreAnnotationNames.Scale];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetScaleConfigurationSource()
        => FindAnnotation(CoreAnnotationNames.Scale)?.GetConfigurationSource();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PropertySaveBehavior? SetBeforeSaveBehavior(
        PropertySaveBehavior? beforeSaveBehavior,
        ConfigurationSource configurationSource)
        => (PropertySaveBehavior?)SetOrRemoveAnnotation(CoreAnnotationNames.BeforeSaveBehavior, beforeSaveBehavior, configurationSource)
            ?.Value;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PropertySaveBehavior GetBeforeSaveBehavior()
        => (PropertySaveBehavior?)this[CoreAnnotationNames.BeforeSaveBehavior]
            ?? (ValueGenerated == ValueGenerated.OnAddOrUpdate
                ? PropertySaveBehavior.Ignore
                : PropertySaveBehavior.Save);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetBeforeSaveBehaviorConfigurationSource()
        => FindAnnotation(CoreAnnotationNames.BeforeSaveBehavior)?.GetConfigurationSource();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PropertySaveBehavior? SetAfterSaveBehavior(
        PropertySaveBehavior? afterSaveBehavior,
        ConfigurationSource configurationSource)
    {
        if (afterSaveBehavior != null)
        {
            var errorMessage = CheckAfterSaveBehavior(afterSaveBehavior.Value);
            if (errorMessage != null)
            {
                throw new InvalidOperationException(errorMessage);
            }
        }

        return (PropertySaveBehavior?)SetOrRemoveAnnotation(
                CoreAnnotationNames.AfterSaveBehavior, afterSaveBehavior, configurationSource)
            ?.Value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PropertySaveBehavior GetAfterSaveBehavior()
        => (PropertySaveBehavior?)this[CoreAnnotationNames.AfterSaveBehavior]
            ?? (IsKey()
                ? PropertySaveBehavior.Throw
                : ValueGenerated.ForUpdate()
                    ? PropertySaveBehavior.Ignore
                    : PropertySaveBehavior.Save);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetAfterSaveBehaviorConfigurationSource()
        => FindAnnotation(CoreAnnotationNames.AfterSaveBehavior)?.GetConfigurationSource();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? CheckAfterSaveBehavior(PropertySaveBehavior behavior)
        => behavior != PropertySaveBehavior.Throw
            && IsKey()
                ? CoreStrings.KeyPropertyMustBeReadOnly(Name, DeclaringType.DisplayName())
                : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object? Sentinel
    {
        get => _sentinel ?? DefaultSentinel;
        set => SetSentinel(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object? SetSentinel(object? sentinel, ConfigurationSource configurationSource)
    {
        EnsureMutable();

        _sentinel = sentinel;
        if (sentinel == null)
        {
            if (!ClrType.IsNullableType())
            {
                throw new InvalidOperationException(
                    CoreStrings.IncompatibleSentinelValue(
                        "null", DeclaringType.DisplayName(), Name, ClrType.ShortDisplayName()));
            }
        }
        else
        {
            var valueType = sentinel.GetType();
            if (!ClrType.UnwrapNullableType().IsAssignableFrom(valueType))
            {
                try
                {
                    _sentinel = Convert.ChangeType(sentinel, ClrType, CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    throw new InvalidOperationException(
                        CoreStrings.IncompatibleSentinelValue(
                            sentinel, DeclaringType.DisplayName(), Name, ClrType.ShortDisplayName()));
                }
            }
        }

        _sentinelConfigurationSource = configurationSource.Max(_sentinelConfigurationSource);

        return sentinel;
    }

    private object? DefaultSentinel
        => (this is IProperty property
            && property.TryGetMemberInfo(forMaterialization: false, forSet: false, out var member, out _)
                ? member!.GetMemberType()
                : ClrType).GetDefaultValue();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetSentinelConfigurationSource()
        => _sentinelConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<IProperty, ITypeBase, ValueGenerator>? SetValueGeneratorFactory(
        Func<IProperty, ITypeBase, ValueGenerator>? factory,
        ConfigurationSource configurationSource)
    {
        RemoveAnnotation(CoreAnnotationNames.ValueGeneratorFactoryType);
        return (Func<IProperty, ITypeBase, ValueGenerator>?)
            SetAnnotation(CoreAnnotationNames.ValueGeneratorFactory, factory, configurationSource)?.Value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Type? SetValueGeneratorFactory(
        [DynamicallyAccessedMembers(ValueGeneratorFactory.DynamicallyAccessedMemberTypes)]
        Type? factoryType,
        ConfigurationSource configurationSource)
    {
        if (factoryType != null)
        {
            if (!typeof(ValueGeneratorFactory).IsAssignableFrom(factoryType))
            {
                throw new InvalidOperationException(
                    CoreStrings.BadValueGeneratorType(
                        factoryType.ShortDisplayName(), typeof(ValueGeneratorFactory).ShortDisplayName()));
            }

            if (factoryType.IsAbstract
                || !factoryType.GetTypeInfo().DeclaredConstructors.Any(c => c.IsPublic && c.GetParameters().Length == 0))
            {
                throw new InvalidOperationException(
                    CoreStrings.CannotCreateValueGenerator(factoryType.ShortDisplayName(), nameof(SetValueGeneratorFactory)));
            }
        }

        RemoveAnnotation(CoreAnnotationNames.ValueGeneratorFactory);
        return (Type?)SetAnnotation(CoreAnnotationNames.ValueGeneratorFactoryType, factoryType, configurationSource)?.Value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<IProperty, ITypeBase, ValueGenerator>? GetValueGeneratorFactory()
    {
        var factory = (Func<IProperty, ITypeBase, ValueGenerator>?)this[CoreAnnotationNames.ValueGeneratorFactory];
        if (factory == null)
        {
            var factoryType = (Type?)this[CoreAnnotationNames.ValueGeneratorFactoryType];
            if (factoryType != null)
            {
                return ((ValueGeneratorFactory)Activator.CreateInstance(factoryType)!).Create;
            }
        }

        return factory;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetValueGeneratorFactoryConfigurationSource()
        => (FindAnnotation(CoreAnnotationNames.ValueGeneratorFactory)
            ?? FindAnnotation(CoreAnnotationNames.ValueGeneratorFactoryType))?.GetConfigurationSource();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ValueConverter? SetValueConverter(
        ValueConverter? converter,
        ConfigurationSource configurationSource)
    {
        var errorString = CheckValueConverter(converter);
        if (errorString != null)
        {
            throw new InvalidOperationException(errorString);
        }

        RemoveAnnotation(CoreAnnotationNames.ValueConverterType);
        return (ValueConverter?)SetAnnotation(CoreAnnotationNames.ValueConverter, converter, configurationSource)?.Value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Type? SetValueConverter(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? converterType,
        ConfigurationSource configurationSource)
    {
        ValueConverter? converter = null;
        if (converterType != null)
        {
            if (!typeof(ValueConverter).IsAssignableFrom(converterType))
            {
                throw new InvalidOperationException(
                    CoreStrings.BadValueConverterType(converterType.ShortDisplayName(), typeof(ValueConverter).ShortDisplayName()));
            }

            try
            {
                converter = (ValueConverter?)Activator.CreateInstance(converterType);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    CoreStrings.CannotCreateValueConverter(
                        converterType.ShortDisplayName(), nameof(PropertyBuilder.HasConversion)), e);
            }
        }

        SetValueConverter(converter, configurationSource);
        SetAnnotation(CoreAnnotationNames.ValueConverterType, converterType, configurationSource);

        return converterType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ValueConverter? GetValueConverter()
    {
        var annotation = FindAnnotation(CoreAnnotationNames.ValueConverter);
        return annotation != null
            ? (ValueConverter?)annotation.Value
            : GetConversion(throwOnProviderClrTypeConflict: FindAnnotation(CoreAnnotationNames.ProviderClrType) == null)
                .ValueConverter;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetValueConverterConfigurationSource()
        => FindAnnotation(CoreAnnotationNames.ValueConverter)?.GetConfigurationSource();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? CheckValueConverter(ValueConverter? converter)
        => converter != null
            && converter.ModelClrType.UnwrapNullableType() != ClrType.UnwrapNullableType()
                ? CoreStrings.ConverterPropertyMismatch(
                    converter.ModelClrType.ShortDisplayName(),
                    DeclaringType.DisplayName(),
                    Name,
                    ClrType.ShortDisplayName())
                : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Type? SetProviderClrType(Type? providerClrType, ConfigurationSource configurationSource)
        => (Type?)SetAnnotation(CoreAnnotationNames.ProviderClrType, providerClrType, configurationSource)?.Value;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Type? GetProviderClrType()
    {
        var annotation = FindAnnotation(CoreAnnotationNames.ProviderClrType);
        return annotation != null
            ? (Type?)annotation.Value
            : GetConversion(throwOnValueConverterConflict: FindAnnotation(CoreAnnotationNames.ValueConverter) == null)
                .ProviderClrType;
    }
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual (ValueConverter? ValueConverter, Type? ValueConverterType, Type? ProviderClrType) GetConversion(
        bool throwOnValueConverterConflict = true,
        bool throwOnProviderClrTypeConflict = true)
    {
        Queue<(Property CurrentProperty, Property CycleBreakingProperty, int CyclePosition, int MaxCycleLength)>? queue = null;
        (Property CurrentProperty, Property CycleBreakingProperty, int CyclePosition, int MaxCycleLength)? currentNode =
            (this, this, 0, 2);
        HashSet<Property>? visitedProperties = null;

        ValueConverter? valueConverter = null;
        Type? valueConverterType = null;
        Type? providerClrType = null;
        while (currentNode is not null || queue is { Count: > 0 })
        {
            var (property, cycleBreakingProperty, cyclePosition, maxCycleLength) = currentNode ?? queue!.Dequeue();
            currentNode = null;
            if (cyclePosition >= ForeignKey.LongestFkChainAllowedLength
                || (queue is not null
                    && queue.Count >= ForeignKey.LongestFkChainAllowedLength))
            {
                throw new InvalidOperationException(
                    CoreStrings.RelationshipCycle(DeclaringType.DisplayName(), Name, "ValueConverter"));
            }

            visitedProperties?.Add(property);

            foreach (var foreignKey in property.GetContainingForeignKeys())
            {
                for (var propertyIndex = 0; propertyIndex < foreignKey.Properties.Count; propertyIndex++)
                {
                    if (property != foreignKey.Properties[propertyIndex])
                    {
                        continue;
                    }

                    var principalProperty = foreignKey.PrincipalKey.Properties[propertyIndex];
                    if (principalProperty == cycleBreakingProperty)
                    {
                        break;
                    }

                    var annotationFound = GetConversion(
                        principalProperty,
                        throwOnValueConverterConflict,
                        throwOnProviderClrTypeConflict,
                        ref valueConverter,
                        ref valueConverterType,
                        ref providerClrType);
                    if (!annotationFound)
                    {
                        var useQueue = queue != null;
                        if (currentNode != null)
                        {
                            useQueue = true;
                            queue = new();
                            queue.Enqueue(currentNode.Value);
                            visitedProperties = new() { property };
                        }

                        if (visitedProperties?.Contains(principalProperty) == true)
                        {
                            break;
                        }

                        if (cyclePosition == maxCycleLength - 1)
                        {
                            // We need to use different primes to ensure a different cycleBreakingProperty is selected
                            // each time when traversing properties that participate in multiple relationship cycles
                            currentNode = (principalProperty, property, 0, HashHelpers.GetPrime(maxCycleLength << 1));
                        }
                        else
                        {
                            currentNode = (principalProperty, cycleBreakingProperty, cyclePosition + 1, maxCycleLength);
                        }

                        if (useQueue)
                        {
                            queue!.Enqueue(currentNode.Value);
                            currentNode = null;
                        }
                    }
                    break;
                }
            }
        }

        return (valueConverter, valueConverterType, providerClrType);

        bool GetConversion(
        Property principalProperty,
        bool throwOnValueConverterConflict,
        bool throwOnProviderClrTypeConflict,
        ref ValueConverter? valueConverter,
        ref Type? valueConverterType,
        ref Type? providerClrType)
        {
            var annotationFound = false;
            var valueConverterAnnotation = principalProperty.FindAnnotation(CoreAnnotationNames.ValueConverter);
            if (valueConverterAnnotation != null)
            {
                var annotationValue = (ValueConverter?)valueConverterAnnotation.Value;
                if (annotationValue != null)
                {
                    if (valueConverter != null
                        && annotationValue.GetType() != valueConverter.GetType())
                    {
                        throw new InvalidOperationException(
                            CoreStrings.ConflictingRelationshipConversions(
                                DeclaringType.DisplayName(), Name,
                                valueConverter.GetType().ShortDisplayName(), annotationValue.GetType().ShortDisplayName()));
                    }

                    if (valueConverterType != null
                        && annotationValue.GetType() != valueConverterType)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.ConflictingRelationshipConversions(
                                DeclaringType.DisplayName(), Name,
                                valueConverterType.ShortDisplayName(), annotationValue.GetType().ShortDisplayName()));
                    }

                    if (providerClrType != null
                        && throwOnProviderClrTypeConflict)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.ConflictingRelationshipConversions(
                                DeclaringType.DisplayName(), Name,
                                providerClrType.ShortDisplayName(), annotationValue.GetType().ShortDisplayName()));
                    }

                    valueConverter = annotationValue;
                }
                annotationFound = true;
            }

            var valueConverterTypeAnnotation = principalProperty.FindAnnotation(CoreAnnotationNames.ValueConverterType);
            if (valueConverterTypeAnnotation != null)
            {
                var annotationValue = (Type?)valueConverterTypeAnnotation.Value;
                if (annotationValue != null)
                {
                    if (valueConverter != null
                        && valueConverter.GetType() != annotationValue)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.ConflictingRelationshipConversions(
                                DeclaringType.DisplayName(), Name,
                                valueConverter.GetType().ShortDisplayName(), annotationValue.ShortDisplayName()));
                    }

                    if (valueConverterType != null
                        && valueConverterType != annotationValue)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.ConflictingRelationshipConversions(
                                DeclaringType.DisplayName(), Name,
                                valueConverterType.ShortDisplayName(), annotationValue.ShortDisplayName()));
                    }

                    if (providerClrType != null
                        && throwOnProviderClrTypeConflict)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.ConflictingRelationshipConversions(
                                DeclaringType.DisplayName(), Name,
                                providerClrType.ShortDisplayName(), annotationValue.ShortDisplayName()));
                    }

                    valueConverterType = annotationValue;
                }
                annotationFound = true;
            }

            var providerClrTypeAnnotation = principalProperty.FindAnnotation(CoreAnnotationNames.ProviderClrType);
            if (providerClrTypeAnnotation != null)
            {
                var annotationValue = (Type?)providerClrTypeAnnotation.Value;
                if (annotationValue != null)
                {
                    if (providerClrType != null
                        && annotationValue != providerClrType)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.ConflictingRelationshipConversions(
                                DeclaringType.DisplayName(), Name,
                                providerClrType.ShortDisplayName(), annotationValue.ShortDisplayName()));
                    }

                    if (valueConverter != null
                        && throwOnValueConverterConflict)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.ConflictingRelationshipConversions(
                                DeclaringType.DisplayName(), Name,
                                valueConverter.GetType().ShortDisplayName(), annotationValue.ShortDisplayName()));
                    }

                    if (valueConverterType != null
                        && throwOnValueConverterConflict)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.ConflictingRelationshipConversions(
                                DeclaringType.DisplayName(), Name,
                                valueConverterType.ShortDisplayName(), annotationValue.ShortDisplayName()));
                    }

                    providerClrType = annotationValue;
                }
                annotationFound = true;
            }

            return annotationFound;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetProviderClrTypeConfigurationSource()
        => FindAnnotation(CoreAnnotationNames.ProviderClrType)?.GetConfigurationSource();

    private Type GetEffectiveProviderClrType()
        => (TypeMapping?.Converter?.ProviderClrType
            ?? ClrType).UnwrapNullableType();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DisallowNull]
    public virtual CoreTypeMapping? TypeMapping
    {
        get => IsReadOnly
            ? NonCapturingLazyInitializer.EnsureInitialized(
                ref _typeMapping, (IProperty)this, static property =>
                    property.DeclaringType.Model.GetModelDependencies().TypeMappingSource.FindMapping(property)!)
            : _typeMapping;

        set => SetTypeMapping(value, ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CoreTypeMapping? SetTypeMapping(CoreTypeMapping? typeMapping, ConfigurationSource configurationSource)
    {
        _typeMapping = typeMapping;
        _typeMappingConfigurationSource = typeMapping is null
            ? null
            : configurationSource.Max(_typeMappingConfigurationSource);

        return typeMapping;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetTypeMappingConfigurationSource()
        => _typeMappingConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ValueComparer? SetValueComparer(ValueComparer? comparer, ConfigurationSource configurationSource)
    {
        var errorString = CheckValueComparer(comparer);
        if (errorString != null)
        {
            throw new InvalidOperationException(errorString);
        }

        RemoveAnnotation(CoreAnnotationNames.ValueComparerType);
        return (ValueComparer?)SetOrRemoveAnnotation(CoreAnnotationNames.ValueComparer, comparer, configurationSource)?.Value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public virtual Type? SetValueComparer(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? comparerType,
        ConfigurationSource configurationSource)
    {
        ValueComparer? comparer = null;
        if (comparerType != null)
        {
            if (!typeof(ValueComparer).IsAssignableFrom(comparerType))
            {
                throw new InvalidOperationException(
                    CoreStrings.BadValueComparerType(comparerType.ShortDisplayName(), typeof(ValueComparer).ShortDisplayName()));
            }

            try
            {
                comparer = (ValueComparer?)Activator.CreateInstance(comparerType);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    CoreStrings.CannotCreateValueComparer(
                        comparerType.ShortDisplayName(), nameof(PropertyBuilder.HasConversion)), e);
            }
        }

        SetValueComparer(comparer, configurationSource);
        return (Type?)SetOrRemoveAnnotation(CoreAnnotationNames.ValueComparerType, comparerType, configurationSource)?.Value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ValueComparer? GetValueComparer()
        => (GetValueComparer(null) ?? TypeMapping?.Comparer).ToNullableComparer(ClrType);

    private ValueComparer? GetValueComparer(HashSet<Property>? checkedProperties)
    {
        var comparer = (ValueComparer?)this[CoreAnnotationNames.ValueComparer];
        if (comparer != null)
        {
            return comparer;
        }

        var principal = (Property?)this.FindFirstDifferentPrincipal();
        if (principal == null)
        {
            return null;
        }

        if (checkedProperties == null)
        {
            checkedProperties = [];
        }
        else if (checkedProperties.Contains(this))
        {
            return null;
        }

        checkedProperties.Add(this);
        return principal.GetValueComparer(checkedProperties);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetValueComparerConfigurationSource()
        => FindAnnotation(CoreAnnotationNames.ValueComparer)?.GetConfigurationSource();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ValueComparer? GetKeyValueComparer()
        => (GetValueComparer(null) ?? TypeMapping?.KeyComparer).ToNullableComparer(ClrType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ValueComparer? SetProviderValueComparer(ValueComparer? comparer, ConfigurationSource configurationSource)
    {
        RemoveAnnotation(CoreAnnotationNames.ProviderValueComparerType);
        return (ValueComparer?)SetOrRemoveAnnotation(CoreAnnotationNames.ProviderValueComparer, comparer, configurationSource)?.Value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public virtual Type? SetProviderValueComparer(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? comparerType,
        ConfigurationSource configurationSource)
    {
        ValueComparer? comparer = null;
        if (comparerType != null)
        {
            if (!typeof(ValueComparer).IsAssignableFrom(comparerType))
            {
                throw new InvalidOperationException(
                    CoreStrings.BadValueComparerType(comparerType.ShortDisplayName(), typeof(ValueComparer).ShortDisplayName()));
            }

            try
            {
                comparer = (ValueComparer?)Activator.CreateInstance(comparerType);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    CoreStrings.CannotCreateValueComparer(
                        comparerType.ShortDisplayName(), nameof(PropertyBuilder.HasConversion)), e);
            }
        }

        SetProviderValueComparer(comparer, configurationSource);
        return (Type?)SetOrRemoveAnnotation(CoreAnnotationNames.ProviderValueComparerType, comparerType, configurationSource)?.Value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ValueComparer? GetProviderValueComparer()
        => GetProviderValueComparer(null)
            ?? (GetEffectiveProviderClrType() == ClrType.UnwrapNullableType()
                ? GetKeyValueComparer()
                : TypeMapping?.ProviderValueComparer);

    private ValueComparer? GetProviderValueComparer(HashSet<Property>? checkedProperties)
    {
        var comparer = (ValueComparer?)this[CoreAnnotationNames.ProviderValueComparer];
        if (comparer != null)
        {
            return comparer;
        }

        var principal = (Property?)this.FindFirstDifferentPrincipal();
        if (principal == null
            || principal.GetEffectiveProviderClrType() != GetEffectiveProviderClrType())
        {
            return null;
        }

        if (checkedProperties == null)
        {
            checkedProperties = [];
        }
        else if (checkedProperties.Contains(this))
        {
            return null;
        }

        checkedProperties.Add(this);
        return principal.GetProviderValueComparer(checkedProperties);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetProviderValueComparerConfigurationSource()
        => FindAnnotation(CoreAnnotationNames.ProviderValueComparer)?.GetConfigurationSource();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? CheckValueComparer(ValueComparer? comparer)
        => comparer != null
            && !comparer.Type.UnwrapNullableType().IsAssignableFrom(ClrType.UnwrapNullableType())
                ? CoreStrings.ComparerPropertyMismatch(
                    comparer.Type.ShortDisplayName(),
                    DeclaringType.DisplayName(),
                    Name,
                    ClrType.ShortDisplayName())
                : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual JsonValueReaderWriter? GetJsonValueReaderWriter()
        => JsonValueReaderWriter.CreateFromType((Type?)this[CoreAnnotationNames.JsonValueReaderWriterType]);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Type? SetJsonValueReaderWriterType(
        Type? readerWriterType,
        ConfigurationSource configurationSource)
    {
        if (readerWriterType != null)
        {
            var genericType = readerWriterType.GetGenericTypeImplementations(typeof(JsonValueReaderWriter<>)).FirstOrDefault();
            if (genericType == null)
            {
                throw new InvalidOperationException(CoreStrings.BadJsonValueReaderWriterType(readerWriterType.ShortDisplayName()));
            }
        }

        return (Type?)SetOrRemoveAnnotation(CoreAnnotationNames.JsonValueReaderWriterType, readerWriterType, configurationSource)?.Value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetJsonValueReaderWriterTypeConfigurationSource()
        => FindAnnotation(CoreAnnotationNames.JsonValueReaderWriterType)?.GetConfigurationSource();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ElementType? GetElementType()
        => (ElementType?)this[CoreAnnotationNames.ElementType];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsPrimitiveCollection
    {
        get
        {
            var elementType = GetElementType();
            return elementType != null
                && ClrType.TryGetElementType(typeof(IEnumerable<>))?.UnwrapNullableType()
                    .IsAssignableFrom(elementType.ClrType.UnwrapNullableType())
                == true;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ElementType? SetElementType(
        Type? elementType,
        ConfigurationSource configurationSource)
    {
        var existingElementType = GetElementType();
        if (elementType != null
            && elementType != existingElementType?.ClrType)
        {
            var newElementType = new ElementType(elementType, this, configurationSource);
            SetAnnotation(CoreAnnotationNames.ElementType, newElementType, configurationSource);
            OnElementTypeSet(newElementType, null);
            return newElementType;
        }

        if (elementType == null
            && existingElementType != null)
        {
            existingElementType.SetRemovedFromModel();
            RemoveAnnotation(CoreAnnotationNames.ElementType);
            OnElementTypeSet(null, existingElementType);
            return null;
        }

        return existingElementType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IElementType? OnElementTypeSet(IElementType? newElementType, IElementType? oldElementType)
        => DeclaringType.Model.ConventionDispatcher.OnPropertyElementTypeChanged(Builder, newElementType, oldElementType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetElementTypeConfigurationSource()
        => FindAnnotation(CoreAnnotationNames.ElementType)?.GetConfigurationSource();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyKey? PrimaryKey { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual List<Key>? Keys { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsKey()
        => Keys != null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Key> GetContainingKeys()
        => Keys?.OrderBy(k => k.Properties, PropertyListComparer.Instance) ?? Enumerable.Empty<Key>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual List<ForeignKey>? ForeignKeys { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsForeignKey()
        => ForeignKeys != null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ForeignKey> GetContainingForeignKeys()
        => ForeignKeys?.OrderBy(fk => fk, ForeignKeyComparer.Instance) ?? Enumerable.Empty<ForeignKey>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual List<Index>? Indexes { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsIndex()
        => Indexes != null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<Index> GetContainingIndexes()
        => Indexes?.OrderBy(i => i.Properties, PropertyListComparer.Instance) ?? Enumerable.Empty<Index>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string Format(IEnumerable<string?> properties)
        => "{"
            + string.Join(
                ", ",
                properties.Select(p => string.IsNullOrEmpty(p) ? "<null>" : "'" + p + "'"))
            + "}";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DebugView DebugView
        => new(
            () => ((IReadOnlyProperty)this).ToDebugString(),
            () => ((IReadOnlyProperty)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IReadOnlyProperty)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder IConventionProperty.Builder
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
    [DebuggerStepThrough]
    IReadOnlyElementType? IReadOnlyProperty.GetElementType()
        => GetElementType();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    CoreTypeMapping? IReadOnlyProperty.FindTypeMapping()
        => TypeMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableProperty.SetTypeMapping(CoreTypeMapping typeMapping)
        => SetTypeMapping(typeMapping, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    CoreTypeMapping? IConventionProperty.SetTypeMapping(CoreTypeMapping typeMapping, bool fromDataAnnotation)
        => SetTypeMapping(typeMapping, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyForeignKey> IReadOnlyProperty.GetContainingForeignKeys()
        => GetContainingForeignKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IMutableForeignKey> IMutableProperty.GetContainingForeignKeys()
        => GetContainingForeignKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IConventionForeignKey> IConventionProperty.GetContainingForeignKeys()
        => GetContainingForeignKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IForeignKey> IProperty.GetContainingForeignKeys()
        => GetContainingForeignKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyIndex> IReadOnlyProperty.GetContainingIndexes()
        => GetContainingIndexes();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IMutableIndex> IMutableProperty.GetContainingIndexes()
        => GetContainingIndexes();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IConventionIndex> IConventionProperty.GetContainingIndexes()
        => GetContainingIndexes();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IIndex> IProperty.GetContainingIndexes()
        => GetContainingIndexes();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyKey> IReadOnlyProperty.GetContainingKeys()
        => GetContainingKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IMutableKey> IMutableProperty.GetContainingKeys()
        => GetContainingKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IConventionKey> IConventionProperty.GetContainingKeys()
        => GetContainingKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerable<IKey> IProperty.GetContainingKeys()
        => GetContainingKeys();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IReadOnlyKey? IReadOnlyProperty.FindContainingPrimaryKey()
        => PrimaryKey;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool? IConventionProperty.SetIsNullable(bool? nullable, bool fromDataAnnotation)
        => SetIsNullable(
            nullable, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    ValueGenerated? IConventionProperty.SetValueGenerated(ValueGenerated? valueGenerated, bool fromDataAnnotation)
        => SetValueGenerated(
            valueGenerated, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool? IConventionProperty.SetIsConcurrencyToken(bool? concurrencyToken, bool fromDataAnnotation)
        => SetIsConcurrencyToken(
            concurrencyToken, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableProperty.SetMaxLength(int? maxLength)
        => SetMaxLength(maxLength, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    int? IConventionProperty.SetMaxLength(int? maxLength, bool fromDataAnnotation)
        => SetMaxLength(maxLength, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableProperty.SetPrecision(int? precision)
        => SetPrecision(precision, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    int? IConventionProperty.SetPrecision(int? precision, bool fromDataAnnotation)
        => SetPrecision(precision, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableProperty.SetScale(int? scale)
        => SetScale(scale, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    int? IConventionProperty.SetScale(int? scale, bool fromDataAnnotation)
        => SetScale(scale, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableProperty.SetIsUnicode(bool? unicode)
        => SetIsUnicode(unicode, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool? IConventionProperty.SetIsUnicode(bool? unicode, bool fromDataAnnotation)
        => SetIsUnicode(unicode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableProperty.SetBeforeSaveBehavior(PropertySaveBehavior? beforeSaveBehavior)
        => SetBeforeSaveBehavior(beforeSaveBehavior, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    PropertySaveBehavior? IConventionProperty.SetBeforeSaveBehavior(
        PropertySaveBehavior? beforeSaveBehavior,
        bool fromDataAnnotation)
        => SetBeforeSaveBehavior(
            beforeSaveBehavior,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableProperty.SetAfterSaveBehavior(PropertySaveBehavior? afterSaveBehavior)
        => SetAfterSaveBehavior(afterSaveBehavior, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    PropertySaveBehavior? IConventionProperty.SetAfterSaveBehavior(
        PropertySaveBehavior? afterSaveBehavior,
        bool fromDataAnnotation)
        => SetAfterSaveBehavior(
            afterSaveBehavior,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    object? IConventionProperty.SetSentinel(object? sentinel, bool fromDataAnnotation)
        => SetSentinel(
            sentinel, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableProperty.SetValueGeneratorFactory(Func<IProperty, ITypeBase, ValueGenerator>? valueGeneratorFactory)
        => SetValueGeneratorFactory(valueGeneratorFactory, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    Func<IProperty, ITypeBase, ValueGenerator>? IConventionProperty.SetValueGeneratorFactory(
        Func<IProperty, ITypeBase, ValueGenerator>? valueGeneratorFactory,
        bool fromDataAnnotation)
        => SetValueGeneratorFactory(
            valueGeneratorFactory,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableProperty.SetValueGeneratorFactory(
        [DynamicallyAccessedMembers(ValueGeneratorFactory.DynamicallyAccessedMemberTypes)]
        Type? valueGeneratorFactory)
        => SetValueGeneratorFactory(valueGeneratorFactory, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    Type? IConventionProperty.SetValueGeneratorFactory(
        [DynamicallyAccessedMembers(ValueGeneratorFactory.DynamicallyAccessedMemberTypes)]
        Type? valueGeneratorFactory,
        bool fromDataAnnotation)
        => SetValueGeneratorFactory(
            valueGeneratorFactory,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableProperty.SetValueConverter(ValueConverter? converter)
        => SetValueConverter(converter, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    ValueConverter? IConventionProperty.SetValueConverter(ValueConverter? converter, bool fromDataAnnotation)
        => SetValueConverter(
            converter,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableProperty.SetValueConverter(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? converterType)
        => SetValueConverter(converterType, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    Type? IConventionProperty.SetValueConverter(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? converterType,
        bool fromDataAnnotation)
        => SetValueConverter(
            converterType,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableProperty.SetProviderClrType(Type? providerClrType)
        => SetProviderClrType(providerClrType, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    Type? IConventionProperty.SetProviderClrType(Type? providerClrType, bool fromDataAnnotation)
        => SetProviderClrType(
            providerClrType,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableProperty.SetValueComparer(ValueComparer? comparer)
        => SetValueComparer(comparer, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    ValueComparer? IConventionProperty.SetValueComparer(ValueComparer? comparer, bool fromDataAnnotation)
        => SetValueComparer(
            comparer,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableProperty.SetValueComparer(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? comparerType)
        => SetValueComparer(comparerType, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    Type? IConventionProperty.SetValueComparer(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? comparerType,
        bool fromDataAnnotation)
        => SetValueComparer(
            comparerType,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    ValueComparer IProperty.GetValueComparer()
        => GetValueComparer()!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    ValueComparer IProperty.GetKeyValueComparer()
        => GetKeyValueComparer()!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableProperty.SetProviderValueComparer(ValueComparer? comparer)
        => SetProviderValueComparer(comparer, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    ValueComparer? IConventionProperty.SetProviderValueComparer(ValueComparer? comparer, bool fromDataAnnotation)
        => SetProviderValueComparer(
            comparer,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableProperty.SetProviderValueComparer(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? comparerType)
        => SetProviderValueComparer(comparerType, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    Type? IConventionProperty.SetProviderValueComparer(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? comparerType,
        bool fromDataAnnotation)
        => SetProviderValueComparer(
            comparerType,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    ValueComparer IProperty.GetProviderValueComparer()
        => GetProviderValueComparer()!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableProperty.SetJsonValueReaderWriterType(Type? readerWriterType)
        => SetJsonValueReaderWriterType(readerWriterType, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    Type? IConventionProperty.SetJsonValueReaderWriterType(
        Type? readerWriterType,
        bool fromDataAnnotation)
        => SetJsonValueReaderWriterType(
            readerWriterType,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionElementType? IConventionProperty.SetElementType(Type? elementType, bool fromDataAnnotation)
        => SetElementType(
            elementType,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableProperty.SetElementType(Type? elementType)
        => SetElementType(elementType, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IMutableElementType? IMutableProperty.GetElementType()
        => GetElementType();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IElementType? IProperty.GetElementType()
        => GetElementType();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionElementType? IConventionProperty.GetElementType()
        => GetElementType();
}
