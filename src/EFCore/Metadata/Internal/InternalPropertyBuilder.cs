// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class InternalPropertyBuilder : InternalPropertyBaseBuilder<Property>, IConventionPropertyBuilder
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
        public virtual InternalPropertyBuilder? IsRequired(bool? required, ConfigurationSource configurationSource)
        {
            if (configurationSource != ConfigurationSource.Explicit
                && !CanSetIsRequired(required, configurationSource))
            {
                return null;
            }

            if (required == false)
            {
                using (Metadata.DeclaringEntityType.Model.DelayConventions())
                {
                    foreach (var key in Metadata.GetContainingKeys().ToList())
                    {
                        if (configurationSource == ConfigurationSource.Explicit
                            && key.GetConfigurationSource() == ConfigurationSource.Explicit)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.KeyPropertyCannotBeNullable(
                                    Metadata.Name, Metadata.DeclaringEntityType.DisplayName(), key.Properties.Format()));
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
                    || Metadata.ClrType.IsNullableType());

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
        public new virtual InternalPropertyBuilder? HasField(string? fieldName, ConfigurationSource configurationSource)
            => (InternalPropertyBuilder?)base.HasField(fieldName, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public new virtual InternalPropertyBuilder? HasField(FieldInfo? fieldInfo, ConfigurationSource configurationSource)
            => (InternalPropertyBuilder?)base.HasField(fieldInfo, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public new virtual InternalPropertyBuilder? UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode,
            ConfigurationSource configurationSource)
            => (InternalPropertyBuilder?)base.UsePropertyAccessMode(propertyAccessMode, configurationSource);

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
            Type? valueGeneratorType,
            ConfigurationSource configurationSource)
        {
            if (valueGeneratorType == null)
            {
                return HasValueGenerator((Func<IProperty, IEntityType, ValueGenerator>?)null, configurationSource);
            }

            if (!typeof(ValueGenerator).IsAssignableFrom(valueGeneratorType))
            {
                throw new ArgumentException(
                    CoreStrings.BadValueGeneratorType(valueGeneratorType.ShortDisplayName(), typeof(ValueGenerator).ShortDisplayName()));
            }

            return HasValueGenerator(
                (_, __)
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
            Func<IProperty, IEntityType, ValueGenerator>? factory,
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
            => configurationSource == ConfigurationSource.Explicit
                || (configurationSource.Overrides(Metadata.GetValueConverterConfigurationSource())
                    && Metadata.CheckValueConverter(converter) == null)
                || (Metadata[CoreAnnotationNames.ValueConverterType] == null
                    && (ValueConverter?)Metadata[CoreAnnotationNames.ValueConverter] == converter);

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
            => configurationSource.Overrides(Metadata.GetProviderClrTypeConfigurationSource())
                || Metadata.GetProviderClrType() == providerClrType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder? HasConverter(Type? converterType, ConfigurationSource configurationSource)
        {
            if (CanSetConverter(converterType, configurationSource))
            {
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
        public virtual bool CanSetConverter(Type? converterType, ConfigurationSource? configurationSource)
            => configurationSource.Overrides(Metadata.GetValueConverterConfigurationSource())
                || (Metadata[CoreAnnotationNames.ValueConverter] == null
                    && (Type?)Metadata[CoreAnnotationNames.ValueConverterType] == converterType);

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
        public virtual InternalPropertyBuilder? HasKeyValueComparer(
            ValueComparer? comparer,
            ConfigurationSource configurationSource)
            => HasValueComparer(comparer, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetKeyValueComparer(ValueComparer? comparer, ConfigurationSource? configurationSource)
            => CanSetValueComparer(comparer, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder? Attach(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var newProperty = entityTypeBuilder.Metadata.FindProperty(Metadata.Name);
            InternalPropertyBuilder? newPropertyBuilder;
            var configurationSource = Metadata.GetConfigurationSource();
            var typeConfigurationSource = Metadata.GetTypeConfigurationSource();
            if (newProperty != null
                && (newProperty.GetConfigurationSource().Overrides(configurationSource)
                    || newProperty.GetTypeConfigurationSource().Overrides(typeConfigurationSource)
                    || (Metadata.ClrType == newProperty.ClrType
                        && Metadata.GetIdentifyingMemberInfo()?.Name == newProperty.GetIdentifyingMemberInfo()?.Name)))
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

                newPropertyBuilder = identifyingMemberInfo == null
                    ? entityTypeBuilder.Property(
                        Metadata.ClrType, Metadata.Name, Metadata.GetTypeConfigurationSource(), configurationSource)
                    : (identifyingMemberInfo as PropertyInfo)?.IsIndexerProperty() == true
                        ? entityTypeBuilder.IndexerProperty(Metadata.ClrType, Metadata.Name, configurationSource)
                        : entityTypeBuilder.Property(identifyingMemberInfo, configurationSource!);

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

            return newPropertyBuilder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionPropertyBase IConventionPropertyBaseBuilder.Metadata
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
        IConventionPropertyBaseBuilder? IConventionPropertyBaseBuilder.HasField(string? fieldName, bool fromDataAnnotation)
            => HasField(fieldName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionPropertyBuilder? IConventionPropertyBuilder.HasField(string? fieldName, bool fromDataAnnotation)
            => HasField(fieldName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionPropertyBaseBuilder? IConventionPropertyBaseBuilder.HasField(FieldInfo? fieldInfo, bool fromDataAnnotation)
            => HasField(fieldInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionPropertyBuilder? IConventionPropertyBuilder.HasField(FieldInfo? fieldInfo, bool fromDataAnnotation)
            => HasField(fieldInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionPropertyBaseBuilder.CanSetField(string? fieldName, bool fromDataAnnotation)
            => CanSetField(fieldName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionPropertyBaseBuilder.CanSetField(FieldInfo? fieldInfo, bool fromDataAnnotation)
            => CanSetField(fieldInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionPropertyBaseBuilder? IConventionPropertyBaseBuilder.UsePropertyAccessMode(
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
        IConventionPropertyBuilder? IConventionPropertyBuilder.UsePropertyAccessMode(
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
        bool IConventionPropertyBaseBuilder.CanSetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation)
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
        IConventionPropertyBuilder? IConventionPropertyBuilder.HasValueGenerator(Type? valueGeneratorType, bool fromDataAnnotation)
            => HasValueGenerator(
                valueGeneratorType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionPropertyBuilder? IConventionPropertyBuilder.HasValueGenerator(
            Func<IProperty, IEntityType, ValueGenerator>? factory,
            bool fromDataAnnotation)
            => HasValueGenerator(factory, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionPropertyBuilder.CanSetValueGenerator(Func<IProperty, IEntityType, ValueGenerator>? factory, bool fromDataAnnotation)
            => CanSetValueGenerator(factory, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionPropertyBuilder? IConventionPropertyBuilder.HasValueGeneratorFactory(Type? valueGeneratorFactoryType, bool fromDataAnnotation)
            => HasValueGeneratorFactory(valueGeneratorFactoryType,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionPropertyBuilder.CanSetValueGeneratorFactory(Type? valueGeneratorFactoryType, bool fromDataAnnotation)
            => CanSetValueGeneratorFactory(valueGeneratorFactoryType,
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
        IConventionPropertyBuilder? IConventionPropertyBuilder.HasConverter(Type? converterType, bool fromDataAnnotation)
            => HasConverter(converterType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionPropertyBuilder.CanSetConverter(Type? converterType, bool fromDataAnnotation)
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
        IConventionPropertyBuilder? IConventionPropertyBuilder.HasValueComparer(Type? comparerType, bool fromDataAnnotation)
            => HasValueComparer(comparerType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionPropertyBuilder.CanSetValueComparer(Type? comparerType, bool fromDataAnnotation)
            => CanSetValueComparer(comparerType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionPropertyBuilder? IConventionPropertyBuilder.HasKeyValueComparer(ValueComparer? comparer, bool fromDataAnnotation)
            => HasKeyValueComparer(comparer, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionPropertyBuilder.CanSetKeyValueComparer(ValueComparer? comparer, bool fromDataAnnotation)
            => CanSetKeyValueComparer(comparer, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionPropertyBuilder? IConventionPropertyBuilder.HasStructuralValueComparer(ValueComparer? comparer, bool fromDataAnnotation)
            => HasKeyValueComparer(
                comparer, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionPropertyBuilder.CanSetStructuralValueComparer(ValueComparer? comparer, bool fromDataAnnotation)
            => CanSetKeyValueComparer(
                comparer, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }
}
