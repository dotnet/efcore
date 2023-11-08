// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
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
public class ElementType : ConventionAnnotatable, IMutableElementType, IConventionElementType, IElementType
{
    private InternalElementTypeBuilder? _builder;

    private bool? _isNullable;
    private CoreTypeMapping? _typeMapping;

    private ConfigurationSource _configurationSource;
    private ConfigurationSource? _isNullableConfigurationSource;
    private ConfigurationSource? _typeMappingConfigurationSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ElementType(
        Type clrType,
        Property collectionProperty,
        ConfigurationSource configurationSource)
    {
        ClrType = clrType;
        CollectionProperty = collectionProperty;
        _configurationSource = configurationSource;
        _builder = new InternalElementTypeBuilder(this, collectionProperty.DeclaringType.Model.Builder);
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
    public virtual InternalElementTypeBuilder Builder
    {
        [DebuggerStepThrough]
        get => _builder ?? throw new InvalidOperationException(CoreStrings.ObjectRemovedFromModel(ClrType.ShortDisplayName()));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsInModel
        => _builder is not null
            && CollectionProperty.DeclaringType.IsInModel;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetRemovedFromModel()
        => _builder = null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override IConventionAnnotation? OnAnnotationSet(
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
        => CollectionProperty.DeclaringType.Model.ConventionDispatcher.OnElementTypeAnnotationChanged(
            Builder, name, annotation, oldAnnotation);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Property CollectionProperty { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)]
    public virtual Type ClrType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool IsReadOnly
        => CollectionProperty.IsReadOnly;

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
                OnElementTypeNullableChanged();
            }

            return nullable;
        }

        if (nullable.Value && !ClrType.IsNullableType())
        {
            throw new InvalidOperationException(
                CoreStrings.CannotBeNullableElement(
                    CollectionProperty.DeclaringType.DisplayName(), CollectionProperty.Name, ClrType.ShortDisplayName()));
        }

        _isNullableConfigurationSource = configurationSource.Max(_isNullableConfigurationSource);

        _isNullable = nullable;

        return isChanging
            ? OnElementTypeNullableChanged()
            : nullable;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual bool? OnElementTypeNullableChanged()
        => CollectionProperty.DeclaringType.Model.ConventionDispatcher.OnElementTypeNullabilityChanged(Builder);

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
        => (ValueConverter?)FindAnnotation(CoreAnnotationNames.ValueConverter)?.Value;

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
                ? CoreStrings.ConverterPropertyMismatchElement(
                    converter.ModelClrType.ShortDisplayName(),
                    CollectionProperty.DeclaringType.DisplayName(),
                    CollectionProperty.Name,
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
        => (Type?)FindAnnotation(CoreAnnotationNames.ProviderClrType)?.Value;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConfigurationSource? GetProviderClrTypeConfigurationSource()
        => FindAnnotation(CoreAnnotationNames.ProviderClrType)?.GetConfigurationSource();

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
                ref _typeMapping, (IElementType)this, static elementType =>
                    elementType.CollectionProperty.DeclaringType.Model.GetModelDependencies().TypeMappingSource.FindMapping(elementType)!)
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
        => ((ValueComparer?)this[CoreAnnotationNames.ValueComparer]
            ?? TypeMapping?.Comparer)?.ToNullableComparer(ClrType);

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
    public virtual string? CheckValueComparer(ValueComparer? comparer)
        => comparer != null
            && comparer.Type.UnwrapNullableType() != ClrType.UnwrapNullableType()
                ? CoreStrings.ComparerPropertyMismatchElement(
                    comparer.Type.ShortDisplayName(),
                    CollectionProperty.DeclaringType.DisplayName(),
                    CollectionProperty.Name,
                    ClrType.ShortDisplayName())
                : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual JsonValueReaderWriter? GetJsonValueReaderWriter()
        => JsonValueReaderWriter.CreateFromType((Type?)this[CoreAnnotationNames.JsonValueReaderWriterType])
            ?? TypeMapping?.JsonValueReaderWriter;

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
    public virtual DebugView DebugView
        => new(
            () => ((IReadOnlyElementType)this).ToDebugString(),
            () => ((IReadOnlyElementType)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IReadOnlyElementType)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IReadOnlyProperty IReadOnlyElementType.CollectionProperty
    {
        [DebuggerStepThrough]
        get => CollectionProperty;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IMutableProperty IMutableElementType.CollectionProperty
    {
        [DebuggerStepThrough]
        get => CollectionProperty;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionProperty IConventionElementType.CollectionProperty
    {
        [DebuggerStepThrough]
        get => CollectionProperty;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionElementTypeBuilder IConventionElementType.Builder
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
    CoreTypeMapping? IReadOnlyElementType.FindTypeMapping()
        => TypeMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableElementType.SetTypeMapping(CoreTypeMapping typeMapping)
        => SetTypeMapping(typeMapping, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    CoreTypeMapping? IConventionElementType.SetTypeMapping(CoreTypeMapping typeMapping, bool fromDataAnnotation)
        => SetTypeMapping(typeMapping, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool? IConventionElementType.SetIsNullable(bool? nullable, bool fromDataAnnotation)
        => SetIsNullable(
            nullable, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableElementType.SetMaxLength(int? maxLength)
        => SetMaxLength(maxLength, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    int? IConventionElementType.SetMaxLength(int? maxLength, bool fromDataAnnotation)
        => SetMaxLength(maxLength, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableElementType.SetPrecision(int? precision)
        => SetPrecision(precision, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    int? IConventionElementType.SetPrecision(int? precision, bool fromDataAnnotation)
        => SetPrecision(precision, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableElementType.SetScale(int? scale)
        => SetScale(scale, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    int? IConventionElementType.SetScale(int? scale, bool fromDataAnnotation)
        => SetScale(scale, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableElementType.SetIsUnicode(bool? unicode)
        => SetIsUnicode(unicode, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    bool? IConventionElementType.SetIsUnicode(bool? unicode, bool fromDataAnnotation)
        => SetIsUnicode(unicode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    void IMutableElementType.SetValueConverter(ValueConverter? converter)
        => SetValueConverter(converter, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    ValueConverter? IConventionElementType.SetValueConverter(ValueConverter? converter, bool fromDataAnnotation)
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
    void IMutableElementType.SetValueConverter(
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
    Type? IConventionElementType.SetValueConverter(
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
    void IMutableElementType.SetProviderClrType(Type? providerClrType)
        => SetProviderClrType(providerClrType, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    Type? IConventionElementType.SetProviderClrType(Type? providerClrType, bool fromDataAnnotation)
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
    void IMutableElementType.SetValueComparer(ValueComparer? comparer)
        => SetValueComparer(comparer, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    ValueComparer? IConventionElementType.SetValueComparer(ValueComparer? comparer, bool fromDataAnnotation)
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
    void IMutableElementType.SetValueComparer(
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
    Type? IConventionElementType.SetValueComparer(
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
    void IMutableElementType.SetJsonValueReaderWriterType(Type? readerWriterType)
        => SetJsonValueReaderWriterType(readerWriterType, ConfigurationSource.Explicit);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    Type? IConventionElementType.SetJsonValueReaderWriterType(
        Type? readerWriterType,
        bool fromDataAnnotation)
        => SetJsonValueReaderWriterType(
            readerWriterType,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
