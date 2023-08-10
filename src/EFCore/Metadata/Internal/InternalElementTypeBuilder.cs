// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InternalElementTypeBuilder : AnnotatableBuilder<ElementType, InternalModelBuilder>, IConventionElementTypeBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalElementTypeBuilder(ElementType element, InternalModelBuilder modelBuilder)
        : base(element, modelBuilder)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IConventionElementTypeBuilder This
        => this;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalElementTypeBuilder? IsRequired(bool? required, ConfigurationSource configurationSource)
    {
        if (configurationSource != ConfigurationSource.Explicit
            && !CanSetIsRequired(required, configurationSource))
        {
            return null;
        }

        Metadata.SetIsNullable(!required, configurationSource);

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetIsRequired(bool? required, ConfigurationSource? configurationSource)
        => ((configurationSource.HasValue
                    && configurationSource.Value.Overrides(Metadata.GetIsNullableConfigurationSource()))
                || (Metadata.IsNullable == !required))
            && (required != false
                || Metadata.ClrType.IsNullableType());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalElementTypeBuilder? HasMaxLength(int? maxLength, ConfigurationSource configurationSource)
    {
        if (CanSetMaxLength(maxLength, configurationSource))
        {
            Metadata.SetMaxLength(maxLength, configurationSource);

            return this;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetMaxLength(int? maxLength, ConfigurationSource? configurationSource)
        => configurationSource.Overrides(Metadata.GetMaxLengthConfigurationSource())
            || Metadata.GetMaxLength() == maxLength;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalElementTypeBuilder? HasPrecision(int? precision, ConfigurationSource configurationSource)
    {
        if (CanSetPrecision(precision, configurationSource))
        {
            Metadata.SetPrecision(precision, configurationSource);

            return this;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetPrecision(int? precision, ConfigurationSource? configurationSource)
        => configurationSource.Overrides(Metadata.GetPrecisionConfigurationSource())
            || Metadata.GetPrecision() == precision;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalElementTypeBuilder? HasScale(int? scale, ConfigurationSource configurationSource)
    {
        if (CanSetScale(scale, configurationSource))
        {
            Metadata.SetScale(scale, configurationSource);

            return this;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetScale(int? scale, ConfigurationSource? configurationSource)
        => configurationSource.Overrides(Metadata.GetScaleConfigurationSource())
            || Metadata.GetScale() == scale;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalElementTypeBuilder? IsUnicode(bool? unicode, ConfigurationSource configurationSource)
    {
        if (CanSetIsUnicode(unicode, configurationSource))
        {
            Metadata.SetIsUnicode(unicode, configurationSource);

            return this;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetIsUnicode(bool? unicode, ConfigurationSource? configurationSource)
        => configurationSource.Overrides(Metadata.GetIsUnicodeConfigurationSource())
            || Metadata.IsUnicode() == unicode;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalElementTypeBuilder? HasConversion(ValueConverter? converter, ConfigurationSource configurationSource)
    {
        if (CanSetConversion(converter, configurationSource))
        {
            Metadata.SetProviderClrType(null, configurationSource);
            Metadata.SetValueConverter(converter, configurationSource);

            return this;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetConversion(
        ValueConverter? converter,
        ConfigurationSource? configurationSource)
        => (configurationSource == ConfigurationSource.Explicit
                || (configurationSource.Overrides(Metadata.GetValueConverterConfigurationSource())
                    && Metadata.CheckValueConverter(converter) == null)
                || (Metadata[CoreAnnotationNames.ValueConverterType] == null
                    && (ValueConverter?)Metadata[CoreAnnotationNames.ValueConverter] == converter))
            && configurationSource.Overrides(Metadata.GetProviderClrTypeConfigurationSource());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalElementTypeBuilder? HasConversion(Type? providerClrType, ConfigurationSource configurationSource)
    {
        if (CanSetConversion(providerClrType, configurationSource))
        {
            Metadata.SetValueConverter((ValueConverter?)null, configurationSource);
            Metadata.SetProviderClrType(providerClrType, configurationSource);

            return this;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetConversion(Type? providerClrType, ConfigurationSource? configurationSource)
        => (configurationSource.Overrides(Metadata.GetProviderClrTypeConfigurationSource())
                || Metadata.GetProviderClrType() == providerClrType)
            && configurationSource.Overrides(Metadata.GetValueConverterConfigurationSource());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalElementTypeBuilder? HasConverter(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? converterType,
        ConfigurationSource configurationSource)
    {
        if (CanSetConverter(converterType, configurationSource))
        {
            Metadata.SetProviderClrType(null, configurationSource);
            Metadata.SetValueConverter(converterType, configurationSource);

            return this;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetConverter(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? converterType,
        ConfigurationSource? configurationSource)
        => configurationSource.Overrides(Metadata.GetValueConverterConfigurationSource())
            || (Metadata[CoreAnnotationNames.ValueConverter] == null
                && (Type?)Metadata[CoreAnnotationNames.ValueConverterType] == converterType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalElementTypeBuilder? HasTypeMapping(
        CoreTypeMapping? typeMapping,
        ConfigurationSource configurationSource)
    {
        if (CanSetTypeMapping(typeMapping, configurationSource))
        {
            Metadata.SetTypeMapping(typeMapping, configurationSource);

            return this;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetTypeMapping(CoreTypeMapping? typeMapping, ConfigurationSource? configurationSource)
        => configurationSource.Overrides(Metadata.GetTypeMappingConfigurationSource())
            || Metadata.TypeMapping == typeMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalElementTypeBuilder? HasValueComparer(
        ValueComparer? comparer,
        ConfigurationSource configurationSource)
    {
        if (CanSetValueComparer(comparer, configurationSource))
        {
            Metadata.SetValueComparer(comparer, configurationSource);

            return this;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetValueComparer(ValueComparer? comparer, ConfigurationSource? configurationSource)
    {
        if (configurationSource.Overrides(Metadata.GetValueComparerConfigurationSource()))
        {
            var errorString = Metadata.CheckValueComparer(comparer);
            if (errorString != null)
            {
                if (configurationSource == ConfigurationSource.Explicit)
                {
                    throw new InvalidOperationException(errorString);
                }

                return false;
            }

            return true;
        }

        return Metadata[CoreAnnotationNames.ValueComparerType] == null
            && Metadata[CoreAnnotationNames.ValueComparer] == comparer;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalElementTypeBuilder? HasValueComparer(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? comparerType,
        ConfigurationSource configurationSource)
    {
        if (CanSetValueComparer(comparerType, configurationSource))
        {
            Metadata.SetValueComparer(comparerType, configurationSource);

            return this;
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetValueComparer(Type? comparerType, ConfigurationSource? configurationSource)
        => configurationSource.Overrides(Metadata.GetValueComparerConfigurationSource())
            || (Metadata[CoreAnnotationNames.ValueComparer] == null
                && (Type?)Metadata[CoreAnnotationNames.ValueComparerType] == comparerType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionElementType IConventionElementTypeBuilder.Metadata
    {
        [DebuggerStepThrough]
        get => Metadata;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionElementTypeBuilder? IConventionElementTypeBuilder.HasAnnotation(string name, object? value, bool fromDataAnnotation)
        => base.HasAnnotation(
                name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
            == null
                ? null
                : this;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionElementTypeBuilder? IConventionElementTypeBuilder.HasNonNullAnnotation(string name, object? value, bool fromDataAnnotation)
        => base.HasNonNullAnnotation(
                name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
            == null
                ? null
                : this;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    IConventionElementTypeBuilder? IConventionElementTypeBuilder.HasNoAnnotation(string name, bool fromDataAnnotation)
        => base.HasNoAnnotation(
                name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
            == null
                ? null
                : this;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionElementTypeBuilder? IConventionElementTypeBuilder.IsRequired(bool? required, bool fromDataAnnotation)
        => IsRequired(required, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionElementTypeBuilder.CanSetIsRequired(bool? required, bool fromDataAnnotation)
        => CanSetIsRequired(required, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionElementTypeBuilder? IConventionElementTypeBuilder.HasMaxLength(int? maxLength, bool fromDataAnnotation)
        => HasMaxLength(maxLength, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionElementTypeBuilder.CanSetMaxLength(int? maxLength, bool fromDataAnnotation)
        => CanSetMaxLength(maxLength, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionElementTypeBuilder? IConventionElementTypeBuilder.IsUnicode(bool? unicode, bool fromDataAnnotation)
        => IsUnicode(unicode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionElementTypeBuilder.CanSetIsUnicode(bool? unicode, bool fromDataAnnotation)
        => CanSetIsUnicode(unicode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionElementTypeBuilder? IConventionElementTypeBuilder.HasPrecision(int? precision, bool fromDataAnnotation)
        => HasPrecision(precision, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionElementTypeBuilder.CanSetPrecision(int? precision, bool fromDataAnnotation)
        => CanSetPrecision(precision, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionElementTypeBuilder? IConventionElementTypeBuilder.HasScale(int? scale, bool fromDataAnnotation)
        => HasScale(scale, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionElementTypeBuilder.CanSetScale(int? scale, bool fromDataAnnotation)
        => CanSetScale(scale, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionElementTypeBuilder? IConventionElementTypeBuilder.HasConversion(ValueConverter? converter, bool fromDataAnnotation)
        => HasConversion(converter, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionElementTypeBuilder.CanSetConversion(ValueConverter? converter, bool fromDataAnnotation)
        => CanSetConversion(converter, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionElementTypeBuilder? IConventionElementTypeBuilder.HasConverter(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? converterType,
        bool fromDataAnnotation)
        => HasConverter(converterType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionElementTypeBuilder.CanSetConverter(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? converterType,
        bool fromDataAnnotation)
        => CanSetConverter(converterType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionElementTypeBuilder? IConventionElementTypeBuilder.HasConversion(Type? providerClrType, bool fromDataAnnotation)
        => HasConversion(
            providerClrType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionElementTypeBuilder.CanSetConversion(Type? providerClrType, bool fromDataAnnotation)
        => CanSetConversion(providerClrType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    IConventionElementTypeBuilder? IConventionElementTypeBuilder.HasTypeMapping(
        CoreTypeMapping? typeMapping,
        bool fromDataAnnotation)
        => HasTypeMapping(typeMapping, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    bool IConventionElementTypeBuilder.CanSetTypeMapping(CoreTypeMapping typeMapping, bool fromDataAnnotation)
        => CanSetTypeMapping(typeMapping, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionElementTypeBuilder? IConventionElementTypeBuilder.HasValueComparer(ValueComparer? comparer, bool fromDataAnnotation)
        => HasValueComparer(comparer, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionElementTypeBuilder.CanSetValueComparer(ValueComparer? comparer, bool fromDataAnnotation)
        => CanSetValueComparer(comparer, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionElementTypeBuilder? IConventionElementTypeBuilder.HasValueComparer(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? comparerType,
        bool fromDataAnnotation)
        => HasValueComparer(
            comparerType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionElementTypeBuilder.CanSetValueComparer(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? comparerType,
        bool fromDataAnnotation)
        => CanSetValueComparer(comparerType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
