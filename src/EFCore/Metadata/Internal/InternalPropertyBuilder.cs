// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InternalPropertyBuilder
    : InternalPropertyBaseBuilder<IConventionPropertyBuilder, Property>, IConventionPropertyBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InternalPropertyBuilder(Property property, InternalModelBuilder modelBuilder)
        : base(property, modelBuilder)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override IConventionPropertyBuilder This
        => this;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? IsRequired(bool? required, ConfigurationSource configurationSource)
    {
        if (configurationSource != ConfigurationSource.Explicit
            && !CanSetIsRequired(required, configurationSource))
        {
            return null;
        }

        if (required == false)
        {
            using (Metadata.DeclaringType.Model.DelayConventions())
            {
                foreach (var key in Metadata.GetContainingKeys().ToList())
                {
                    if (configurationSource == ConfigurationSource.Explicit
                        && key.GetConfigurationSource() == ConfigurationSource.Explicit)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.KeyPropertyCannotBeNullable(
                                Metadata.Name, Metadata.DeclaringType.DisplayName(), key.Properties.Format()));
                    }

                    var removed = key.DeclaringEntityType.Builder.HasNoKey(key, configurationSource);
                    Check.DebugAssert(removed != null, "removed is null");
                }

                Metadata.SetIsNullable(true, configurationSource);
            }
        }
        else
        {
            Metadata.SetIsNullable(!required, configurationSource);
        }

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
                || (Metadata.ClrType.IsNullableType()
                    && Metadata.GetContainingKeys().All(k => configurationSource.Overrides(k.GetConfigurationSource()))));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? ValueGenerated(ValueGenerated? valueGenerated, ConfigurationSource configurationSource)
    {
        if (CanSetValueGenerated(valueGenerated, configurationSource))
        {
            Metadata.SetValueGenerated(valueGenerated, configurationSource);

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
    public virtual bool CanSetValueGenerated(ValueGenerated? valueGenerated, ConfigurationSource? configurationSource)
        => configurationSource.Overrides(Metadata.GetValueGeneratedConfigurationSource())
            || Metadata.ValueGenerated == valueGenerated;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? IsConcurrencyToken(bool? concurrencyToken, ConfigurationSource configurationSource)
    {
        if (CanSetIsConcurrencyToken(concurrencyToken, configurationSource))
        {
            Metadata.SetIsConcurrencyToken(concurrencyToken, configurationSource);
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
    public virtual bool CanSetIsConcurrencyToken(bool? concurrencyToken, ConfigurationSource? configurationSource)
        => configurationSource.Overrides(Metadata.GetIsConcurrencyTokenConfigurationSource())
            || Metadata.IsConcurrencyToken == concurrencyToken;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? HasSentinel(object? sentinel, ConfigurationSource configurationSource)
    {
        if (CanSetSentinel(sentinel, configurationSource))
        {
            Metadata.SetSentinel(sentinel, configurationSource);

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
    public virtual bool CanSetSentinel(object? sentinel, ConfigurationSource? configurationSource)
    {
        if (configurationSource.Overrides(Metadata.GetSentinelConfigurationSource()))
        {
            return true;
        }

        if (sentinel == null
            || Metadata.ClrType.UnwrapNullableType().IsAssignableFrom(sentinel.GetType()))
        {
            return Equals(Metadata.Sentinel, sentinel);
        }
        else
        {
            try
            {
                return Equals(Metadata.Sentinel, Convert.ChangeType(sentinel, Metadata.ClrType, CultureInfo.InvariantCulture));
            }
            catch (Exception)
            {
                throw new InvalidOperationException(
                    CoreStrings.IncompatibleSentinelValue(
                        sentinel, Metadata.DeclaringType.DisplayName(), Metadata.Name, Metadata.ClrType.ShortDisplayName()));
            }
        }
    }


    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public new virtual InternalPropertyBuilder? HasField(string? fieldName, ConfigurationSource configurationSource)
        => base.HasField(fieldName, configurationSource) == null
            ? null
            : this;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public new virtual InternalPropertyBuilder? HasField(FieldInfo? fieldInfo, ConfigurationSource configurationSource)
        => base.HasField(fieldInfo, configurationSource) == null
            ? null
            : this;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public new virtual InternalPropertyBuilder? UsePropertyAccessMode(
        PropertyAccessMode? propertyAccessMode,
        ConfigurationSource configurationSource)
        => base.UsePropertyAccessMode(propertyAccessMode, configurationSource) == null
            ? null
            : this;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? HasMaxLength(int? maxLength, ConfigurationSource configurationSource)
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
    public virtual InternalPropertyBuilder? HasPrecision(int? precision, ConfigurationSource configurationSource)
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
    public virtual InternalPropertyBuilder? HasScale(int? scale, ConfigurationSource configurationSource)
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
    public virtual InternalPropertyBuilder? IsUnicode(bool? unicode, ConfigurationSource configurationSource)
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
    public virtual InternalPropertyBuilder? BeforeSave(PropertySaveBehavior? behavior, ConfigurationSource configurationSource)
    {
        if (CanSetBeforeSave(behavior, configurationSource))
        {
            Metadata.SetBeforeSaveBehavior(behavior, configurationSource);

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
    public virtual bool CanSetBeforeSave(PropertySaveBehavior? behavior, ConfigurationSource? configurationSource)
        => configurationSource.Overrides(Metadata.GetBeforeSaveBehaviorConfigurationSource())
            || Metadata.GetBeforeSaveBehavior() == behavior;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? AfterSave(PropertySaveBehavior? behavior, ConfigurationSource configurationSource)
    {
        if (CanSetAfterSave(behavior, configurationSource))
        {
            Metadata.SetAfterSaveBehavior(behavior, configurationSource);

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
    public virtual bool CanSetAfterSave(PropertySaveBehavior? behavior, ConfigurationSource? configurationSource)
        => (configurationSource.Overrides(Metadata.GetAfterSaveBehaviorConfigurationSource())
                && (behavior == null
                    || Metadata.CheckAfterSaveBehavior(behavior.Value) == null))
            || Metadata.GetAfterSaveBehavior() == behavior;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? HasValueGenerator(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? valueGeneratorType,
        ConfigurationSource configurationSource)
    {
        if (valueGeneratorType == null)
        {
            return HasValueGenerator((Func<IProperty, ITypeBase, ValueGenerator>?)null, configurationSource);
        }

        if (!typeof(ValueGenerator).IsAssignableFrom(valueGeneratorType))
        {
            throw new ArgumentException(
                CoreStrings.BadValueGeneratorType(valueGeneratorType.ShortDisplayName(), typeof(ValueGenerator).ShortDisplayName()));
        }

        return HasValueGenerator(
            (_, _)
                =>
            {
                try
                {
                    return (ValueGenerator)Activator.CreateInstance(valueGeneratorType)!;
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(
                        CoreStrings.CannotCreateValueGenerator(
                            valueGeneratorType.ShortDisplayName(), nameof(PropertyBuilder.HasValueGenerator)), e);
                }
            }, configurationSource);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? HasValueGenerator(
        Func<IProperty, ITypeBase, ValueGenerator>? factory,
        ConfigurationSource configurationSource)
    {
        if (CanSetValueGenerator(factory, configurationSource))
        {
            Metadata.SetValueGeneratorFactory(factory, configurationSource);

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
    public virtual InternalPropertyBuilder? HasValueGeneratorFactory(
        [DynamicallyAccessedMembers(ValueGeneratorFactory.DynamicallyAccessedMemberTypes)]
        Type? factory,
        ConfigurationSource configurationSource)
    {
        if (CanSetValueGeneratorFactory(factory, configurationSource))
        {
            Metadata.SetValueGeneratorFactory(factory, configurationSource);

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
    public virtual bool CanSetValueGenerator(
        Func<IProperty, IEntityType, ValueGenerator>? factory,
        ConfigurationSource? configurationSource)
        => configurationSource.Overrides(Metadata.GetValueGeneratorFactoryConfigurationSource())
            || (Metadata[CoreAnnotationNames.ValueGeneratorFactoryType] == null
                && (Func<IProperty, IEntityType, ValueGenerator>?)Metadata[CoreAnnotationNames.ValueGeneratorFactory] == factory);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetValueGeneratorFactory(
        [DynamicallyAccessedMembers(ValueGeneratorFactory.DynamicallyAccessedMemberTypes)]
        Type? factory,
        ConfigurationSource? configurationSource)
        => configurationSource.Overrides(Metadata.GetValueGeneratorFactoryConfigurationSource())
            || (Metadata[CoreAnnotationNames.ValueGeneratorFactory] == null
                && (Type?)Metadata[CoreAnnotationNames.ValueGeneratorFactoryType] == factory);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? HasConversion(ValueConverter? converter, ConfigurationSource configurationSource)
    {
        if (CanSetConversion(converter, configurationSource))
        {
            if (converter != null)
            {
                Metadata.SetElementType(null, configurationSource);
            }
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
            && configurationSource.Overrides(Metadata.GetProviderClrTypeConfigurationSource())
            && (converter == null || CanSetElementType(null, configurationSource));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? HasConversion(Type? providerClrType, ConfigurationSource configurationSource)
    {
        if (CanSetConversion(providerClrType, configurationSource))
        {
            if (providerClrType != null)
            {
                Metadata.SetElementType(null, configurationSource);
            }
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
            && configurationSource.Overrides(Metadata.GetValueConverterConfigurationSource())
            && (providerClrType == null || CanSetElementType(null, configurationSource));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? HasConverter(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? converterType,
        ConfigurationSource configurationSource)
    {
        if (CanSetConverter(converterType, configurationSource))
        {
            if (converterType != null)
            {
                Metadata.SetElementType(null, configurationSource);
            }
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
        => (configurationSource.Overrides(Metadata.GetValueConverterConfigurationSource())
            || (Metadata[CoreAnnotationNames.ValueConverter] == null
                && (Type?)Metadata[CoreAnnotationNames.ValueConverterType] == converterType))
            && (converterType == null || CanSetElementType(null, configurationSource));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? HasTypeMapping(
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
    public virtual InternalPropertyBuilder? HasValueComparer(
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
    public virtual InternalPropertyBuilder? HasValueComparer(
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
    public virtual InternalPropertyBuilder? HasProviderValueComparer(
        ValueComparer? comparer,
        ConfigurationSource configurationSource)
    {
        if (CanSetProviderValueComparer(comparer, configurationSource))
        {
            Metadata.SetProviderValueComparer(comparer, configurationSource);

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
    public virtual bool CanSetProviderValueComparer(ValueComparer? comparer, ConfigurationSource? configurationSource)
    {
        if (configurationSource.Overrides(Metadata.GetProviderValueComparerConfigurationSource()))
        {
            return true;
        }

        return Metadata[CoreAnnotationNames.ProviderValueComparerType] == null
            && Metadata[CoreAnnotationNames.ProviderValueComparer] == comparer;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? HasProviderValueComparer(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? comparerType,
        ConfigurationSource configurationSource)
    {
        if (CanSetProviderValueComparer(comparerType, configurationSource))
        {
            Metadata.SetProviderValueComparer(comparerType, configurationSource);

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
    public virtual bool CanSetProviderValueComparer(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? comparerType,
        ConfigurationSource? configurationSource)
        => configurationSource.Overrides(Metadata.GetProviderValueComparerConfigurationSource())
            || (Metadata[CoreAnnotationNames.ProviderValueComparer] == null
                && (Type?)Metadata[CoreAnnotationNames.ProviderValueComparerType] == comparerType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalElementTypeBuilder? SetElementType(Type? elementType, ConfigurationSource configurationSource)
    {
        if (CanSetElementType(elementType, configurationSource))
        {
            Metadata.SetElementType(elementType, configurationSource);
            if (elementType != null)
            {
                Metadata.SetValueConverter((Type?)null, configurationSource);
            }
            return new InternalElementTypeBuilder(Metadata.GetElementType()!, ModelBuilder);
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanSetElementType(Type? elementType, ConfigurationSource? configurationSource)
        => (configurationSource.Overrides(Metadata.GetElementTypeConfigurationSource())
            && (elementType == null || CanSetConversion((Type?)null, configurationSource)))
            || elementType == Metadata.GetElementType()?.ClrType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalPropertyBuilder? Attach(InternalTypeBaseBuilder typeBaseBuilder)
    {
        var newProperty = typeBaseBuilder.Metadata.FindProperty(Metadata.Name);
        InternalPropertyBuilder? newPropertyBuilder;
        var configurationSource = Metadata.GetConfigurationSource();
        var typeConfigurationSource = Metadata.GetTypeConfigurationSource();
        if (newProperty != null
            && (newProperty.GetConfigurationSource().Overrides(configurationSource)
                || newProperty.GetTypeConfigurationSource().Overrides(typeConfigurationSource)
                || (Metadata.ClrType == newProperty.ClrType
                    && Metadata.Name == newProperty.Name
                    && Metadata.GetIdentifyingMemberInfo() == newProperty.GetIdentifyingMemberInfo())))
        {
            newPropertyBuilder = newProperty.Builder;
            newProperty.UpdateConfigurationSource(configurationSource);
            if (typeConfigurationSource.HasValue)
            {
                newProperty.UpdateTypeConfigurationSource(typeConfigurationSource.Value);
            }
        }
        else
        {
            var identifyingMemberInfo = Metadata.GetIdentifyingMemberInfo();

            newPropertyBuilder = Metadata.IsIndexerProperty()
                ? typeBaseBuilder.IndexerProperty(Metadata.ClrType, Metadata.Name, configurationSource)
                : identifyingMemberInfo == null
                    ? typeBaseBuilder.Property(
                        Metadata.ClrType, Metadata.Name, Metadata.GetTypeConfigurationSource(), configurationSource)
                    : typeBaseBuilder.Property(identifyingMemberInfo, configurationSource);

            if (newPropertyBuilder is null)
            {
                return null;
            }
        }

        if (newProperty == Metadata)
        {
            return newPropertyBuilder;
        }

        newPropertyBuilder.MergeAnnotationsFrom(Metadata);

        var oldBeforeSaveBehaviorConfigurationSource = Metadata.GetBeforeSaveBehaviorConfigurationSource();
        if (oldBeforeSaveBehaviorConfigurationSource.HasValue)
        {
            newPropertyBuilder.BeforeSave(
                Metadata.GetBeforeSaveBehavior(),
                oldBeforeSaveBehaviorConfigurationSource.Value);
        }

        var oldAfterSaveBehaviorConfigurationSource = Metadata.GetAfterSaveBehaviorConfigurationSource();
        if (oldAfterSaveBehaviorConfigurationSource.HasValue)
        {
            newPropertyBuilder.AfterSave(
                Metadata.GetAfterSaveBehavior(),
                oldAfterSaveBehaviorConfigurationSource.Value);
        }

        var oldIsNullableConfigurationSource = Metadata.GetIsNullableConfigurationSource();
        if (oldIsNullableConfigurationSource.HasValue)
        {
            newPropertyBuilder.IsRequired(!Metadata.IsNullable, oldIsNullableConfigurationSource.Value);
        }

        var oldIsConcurrencyTokenConfigurationSource = Metadata.GetIsConcurrencyTokenConfigurationSource();
        if (oldIsConcurrencyTokenConfigurationSource.HasValue)
        {
            newPropertyBuilder.IsConcurrencyToken(
                Metadata.IsConcurrencyToken,
                oldIsConcurrencyTokenConfigurationSource.Value);
        }

        var oldValueGeneratedConfigurationSource = Metadata.GetValueGeneratedConfigurationSource();
        if (oldValueGeneratedConfigurationSource.HasValue)
        {
            newPropertyBuilder.ValueGenerated(Metadata.ValueGenerated, oldValueGeneratedConfigurationSource.Value);
        }

        var oldPropertyAccessModeConfigurationSource = Metadata.GetPropertyAccessModeConfigurationSource();
        if (oldPropertyAccessModeConfigurationSource.HasValue)
        {
            newPropertyBuilder.UsePropertyAccessMode(
                ((IReadOnlyProperty)Metadata).GetPropertyAccessMode(), oldPropertyAccessModeConfigurationSource.Value);
        }

        var oldFieldInfoConfigurationSource = Metadata.GetFieldInfoConfigurationSource();
        if (oldFieldInfoConfigurationSource.HasValue
            && newPropertyBuilder.CanSetField(Metadata.FieldInfo, oldFieldInfoConfigurationSource))
        {
            newPropertyBuilder.HasField(Metadata.FieldInfo, oldFieldInfoConfigurationSource.Value);
        }

        var oldTypeMappingConfigurationSource = Metadata.GetTypeMappingConfigurationSource();
        if (oldTypeMappingConfigurationSource.HasValue
            && newPropertyBuilder.CanSetTypeMapping(Metadata.TypeMapping, oldTypeMappingConfigurationSource))
        {
            newPropertyBuilder.HasTypeMapping(Metadata.TypeMapping, oldTypeMappingConfigurationSource.Value);
        }

        var oldElementType = Metadata.GetElementType();
        if (oldElementType != null)
        {
            var newElementType = newPropertyBuilder.Metadata.GetElementType();
            if (newElementType != null)
            {
                var newElementTypeBuilder = new InternalElementTypeBuilder(newElementType, ModelBuilder);
                newElementTypeBuilder.MergeAnnotationsFrom(oldElementType);

                var oldElementNullableConfigurationSource = oldElementType.GetIsNullableConfigurationSource();
                if (oldElementNullableConfigurationSource.HasValue
                    && newElementTypeBuilder.CanSetIsRequired(!oldElementType.IsNullable, oldElementNullableConfigurationSource))
                {
                    newElementTypeBuilder.IsRequired(!oldElementType.IsNullable, oldElementNullableConfigurationSource.Value);
                }

                var oldElementUnicodeConfigurationSource = oldElementType.GetIsUnicodeConfigurationSource();
                if (oldElementUnicodeConfigurationSource.HasValue
                    && newElementTypeBuilder.CanSetIsUnicode(oldElementType.IsNullable, oldElementUnicodeConfigurationSource))
                {
                    newElementTypeBuilder.IsUnicode(oldElementType.IsNullable, oldElementUnicodeConfigurationSource.Value);
                }

                var oldElementProviderClrTypeConfigurationSource = oldElementType.GetProviderClrTypeConfigurationSource();
                if (oldElementProviderClrTypeConfigurationSource.HasValue
                    && newElementTypeBuilder.CanSetConversion(
                        oldElementType.GetProviderClrType(), oldElementProviderClrTypeConfigurationSource))
                {
                    newElementTypeBuilder.HasConversion(
                        oldElementType.GetProviderClrType(), oldElementProviderClrTypeConfigurationSource.Value);
                }

                var oldElementConverterConfigurationSource = oldElementType.GetValueConverterConfigurationSource();
                if (oldElementConverterConfigurationSource.HasValue
                    && newElementTypeBuilder.CanSetConverter(
                        oldElementType.GetValueConverter()?.GetType(), oldElementConverterConfigurationSource))
                {
                    newElementTypeBuilder.HasConverter(
                        oldElementType.GetValueConverter()?.GetType(), oldElementConverterConfigurationSource.Value);
                }

                var oldElementPrecisionConfigurationSource = oldElementType.GetPrecisionConfigurationSource();
                if (oldElementPrecisionConfigurationSource.HasValue
                    && newElementTypeBuilder.CanSetPrecision(oldElementType.GetPrecision(), oldElementPrecisionConfigurationSource))
                {
                    newElementTypeBuilder.HasPrecision(oldElementType.GetPrecision(), oldElementPrecisionConfigurationSource.Value);
                }

                var oldElementScaleConfigurationSource = oldElementType.GetScaleConfigurationSource();
                if (oldElementScaleConfigurationSource.HasValue
                    && newElementTypeBuilder.CanSetScale(oldElementType.GetScale(), oldElementScaleConfigurationSource))
                {
                    newElementTypeBuilder.HasScale(oldElementType.GetScale(), oldElementScaleConfigurationSource.Value);
                }

                var oldElementMaxLengthConfigurationSource = oldElementType.GetMaxLengthConfigurationSource();
                if (oldElementMaxLengthConfigurationSource.HasValue
                    && newElementTypeBuilder.CanSetMaxLength(oldElementType.GetMaxLength(), oldElementMaxLengthConfigurationSource))
                {
                    newElementTypeBuilder.HasMaxLength(oldElementType.GetMaxLength(), oldElementMaxLengthConfigurationSource.Value);
                }

                var oldElementTypeMappingConfigurationSource = oldElementType.GetTypeMappingConfigurationSource();
                if (oldElementTypeMappingConfigurationSource.HasValue
                    && newElementTypeBuilder.CanSetTypeMapping(oldElementType.TypeMapping, oldElementTypeMappingConfigurationSource))
                {
                    newPropertyBuilder.HasTypeMapping(oldElementType.TypeMapping, oldElementTypeMappingConfigurationSource.Value);
                }

                var oldElementComparerConfigurationSource = oldElementType.GetValueComparerConfigurationSource();
                if (oldElementComparerConfigurationSource.HasValue
                    && newElementTypeBuilder.CanSetValueComparer(oldElementType.GetValueComparer(), oldElementComparerConfigurationSource))
                {
                    newElementTypeBuilder.HasValueComparer(oldElementType.GetValueComparer(), oldElementComparerConfigurationSource.Value);
                }
            }
        }

        return newPropertyBuilder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBase IConventionPropertyBaseBuilder<IConventionPropertyBuilder>.Metadata
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
    IConventionProperty IConventionPropertyBuilder.Metadata
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
    IConventionPropertyBuilder? IConventionPropertyBaseBuilder<IConventionPropertyBuilder>.HasAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation)
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
    IConventionPropertyBuilder? IConventionPropertyBaseBuilder<IConventionPropertyBuilder>.HasNonNullAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation)
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
    IConventionPropertyBuilder? IConventionPropertyBaseBuilder<IConventionPropertyBuilder>.HasNoAnnotation(
        string name,
        bool fromDataAnnotation)
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
    IConventionPropertyBuilder? IConventionPropertyBuilder.IsRequired(bool? required, bool fromDataAnnotation)
        => IsRequired(required, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBuilder.CanSetIsRequired(bool? required, bool fromDataAnnotation)
        => CanSetIsRequired(required, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder? IConventionPropertyBuilder.ValueGenerated(ValueGenerated? valueGenerated, bool fromDataAnnotation)
        => ValueGenerated(valueGenerated, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBuilder.CanSetValueGenerated(ValueGenerated? valueGenerated, bool fromDataAnnotation)
        => CanSetValueGenerated(
            valueGenerated, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder? IConventionPropertyBuilder.IsConcurrencyToken(bool? concurrencyToken, bool fromDataAnnotation)
        => IsConcurrencyToken(
            concurrencyToken, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBuilder.CanSetIsConcurrencyToken(bool? concurrencyToken, bool fromDataAnnotation)
        => CanSetIsConcurrencyToken(
            concurrencyToken, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder? IConventionPropertyBuilder.HasSentinel(object? sentinel, bool fromDataAnnotation)
        => HasSentinel(sentinel, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBuilder.CanSetSentinel(object? sentinel, bool fromDataAnnotation)
        => CanSetSentinel(sentinel, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder? IConventionPropertyBaseBuilder<IConventionPropertyBuilder>.HasField(
        string? fieldName,
        bool fromDataAnnotation)
        => HasField(fieldName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder? IConventionPropertyBaseBuilder<IConventionPropertyBuilder>.HasField(
        FieldInfo? fieldInfo,
        bool fromDataAnnotation)
        => HasField(fieldInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBaseBuilder<IConventionPropertyBuilder>.CanSetField(string? fieldName, bool fromDataAnnotation)
        => CanSetField(fieldName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBaseBuilder<IConventionPropertyBuilder>.CanSetField(FieldInfo? fieldInfo, bool fromDataAnnotation)
        => CanSetField(fieldInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder? IConventionPropertyBaseBuilder<IConventionPropertyBuilder>.UsePropertyAccessMode(
        PropertyAccessMode? propertyAccessMode,
        bool fromDataAnnotation)
        => UsePropertyAccessMode(
            propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBaseBuilder<IConventionPropertyBuilder>.CanSetPropertyAccessMode(
        PropertyAccessMode? propertyAccessMode,
        bool fromDataAnnotation)
        => CanSetPropertyAccessMode(
            propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder? IConventionPropertyBuilder.HasMaxLength(int? maxLength, bool fromDataAnnotation)
        => HasMaxLength(maxLength, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBuilder.CanSetMaxLength(int? maxLength, bool fromDataAnnotation)
        => CanSetMaxLength(maxLength, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder? IConventionPropertyBuilder.IsUnicode(bool? unicode, bool fromDataAnnotation)
        => IsUnicode(unicode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBuilder.CanSetIsUnicode(bool? unicode, bool fromDataAnnotation)
        => CanSetIsUnicode(unicode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder? IConventionPropertyBuilder.HasPrecision(int? precision, bool fromDataAnnotation)
        => HasPrecision(precision, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBuilder.CanSetPrecision(int? precision, bool fromDataAnnotation)
        => CanSetPrecision(precision, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder? IConventionPropertyBuilder.HasScale(int? scale, bool fromDataAnnotation)
        => HasScale(scale, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBuilder.CanSetScale(int? scale, bool fromDataAnnotation)
        => CanSetScale(scale, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder? IConventionPropertyBuilder.BeforeSave(PropertySaveBehavior? behavior, bool fromDataAnnotation)
        => BeforeSave(behavior, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBuilder.CanSetBeforeSave(PropertySaveBehavior? behavior, bool fromDataAnnotation)
        => CanSetBeforeSave(behavior, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder? IConventionPropertyBuilder.AfterSave(PropertySaveBehavior? behavior, bool fromDataAnnotation)
        => AfterSave(behavior, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBuilder.CanSetAfterSave(PropertySaveBehavior? behavior, bool fromDataAnnotation)
        => CanSetAfterSave(behavior, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder? IConventionPropertyBuilder.HasValueGenerator(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? valueGeneratorType,
        bool fromDataAnnotation)
        => HasValueGenerator(
            valueGeneratorType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder? IConventionPropertyBuilder.HasValueGenerator(
        Func<IProperty, ITypeBase, ValueGenerator>? factory,
        bool fromDataAnnotation)
        => HasValueGenerator(factory, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBuilder.CanSetValueGenerator(Func<IProperty, ITypeBase, ValueGenerator>? factory, bool fromDataAnnotation)
        => CanSetValueGenerator(factory, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder? IConventionPropertyBuilder.HasValueGeneratorFactory(
        [DynamicallyAccessedMembers(ValueGeneratorFactory.DynamicallyAccessedMemberTypes)]
        Type? valueGeneratorFactoryType,
        bool fromDataAnnotation)
        => HasValueGeneratorFactory(
            valueGeneratorFactoryType,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBuilder.CanSetValueGeneratorFactory(
        [DynamicallyAccessedMembers(ValueGeneratorFactory.DynamicallyAccessedMemberTypes)]
        Type? valueGeneratorFactoryType,
        bool fromDataAnnotation)
        => CanSetValueGeneratorFactory(
            valueGeneratorFactoryType,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder? IConventionPropertyBuilder.HasConversion(ValueConverter? converter, bool fromDataAnnotation)
        => HasConversion(converter, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBuilder.CanSetConversion(ValueConverter? converter, bool fromDataAnnotation)
        => CanSetConversion(converter, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder? IConventionPropertyBuilder.HasConverter(
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
    bool IConventionPropertyBuilder.CanSetConverter(
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
    IConventionPropertyBuilder? IConventionPropertyBuilder.HasConversion(Type? providerClrType, bool fromDataAnnotation)
        => HasConversion(providerClrType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBuilder.CanSetConversion(Type? providerClrType, bool fromDataAnnotation)
        => CanSetConversion(providerClrType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    IConventionPropertyBuilder? IConventionPropertyBuilder.HasTypeMapping(CoreTypeMapping? typeMapping, bool fromDataAnnotation)
        => HasTypeMapping(typeMapping, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    bool IConventionPropertyBuilder.CanSetTypeMapping(CoreTypeMapping typeMapping, bool fromDataAnnotation)
        => CanSetTypeMapping(typeMapping, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder? IConventionPropertyBuilder.HasValueComparer(ValueComparer? comparer, bool fromDataAnnotation)
        => HasValueComparer(comparer, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBuilder.CanSetValueComparer(ValueComparer? comparer, bool fromDataAnnotation)
        => CanSetValueComparer(comparer, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder? IConventionPropertyBuilder.HasValueComparer(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? comparerType,
        bool fromDataAnnotation)
        => HasValueComparer(comparerType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBuilder.CanSetValueComparer(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? comparerType,
        bool fromDataAnnotation)
        => CanSetValueComparer(comparerType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder? IConventionPropertyBuilder.HasProviderValueComparer(ValueComparer? comparer, bool fromDataAnnotation)
        => HasProviderValueComparer(comparer, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBuilder.CanSetProviderValueComparer(ValueComparer? comparer, bool fromDataAnnotation)
        => CanSetProviderValueComparer(comparer, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionPropertyBuilder? IConventionPropertyBuilder.HasProviderValueComparer(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? comparerType,
        bool fromDataAnnotation)
        => HasProviderValueComparer(comparerType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBuilder.CanSetProviderValueComparer(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type? comparerType,
        bool fromDataAnnotation)
        => CanSetProviderValueComparer(
            comparerType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IConventionElementTypeBuilder? IConventionPropertyBuilder.SetElementType(Type? elementType, bool fromDataAnnotation)
        => SetElementType(elementType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    bool IConventionPropertyBuilder.CanSetElementType(Type? elementType, bool fromDataAnnotation)
        => CanSetElementType(elementType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
}
