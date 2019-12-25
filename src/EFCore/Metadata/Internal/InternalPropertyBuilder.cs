// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
    public class InternalPropertyBuilder : InternalModelItemBuilder<Property>, IConventionPropertyBuilder
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalPropertyBuilder([NotNull] Property property, [NotNull] InternalModelBuilder modelBuilder)
            : base(property, modelBuilder)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder IsRequired(bool? required, ConfigurationSource configurationSource)
        {
            if (configurationSource != ConfigurationSource.Explicit
                && !CanSetIsRequired(required, configurationSource))
            {
                return null;
            }

            if (required == false)
            {
                using (Metadata.DeclaringEntityType.Model.ConventionDispatcher.DelayConventions())
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
        public virtual InternalPropertyBuilder ValueGenerated(ValueGenerated? valueGenerated, ConfigurationSource configurationSource)
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
        public virtual InternalPropertyBuilder IsConcurrencyToken(bool? concurrencyToken, ConfigurationSource configurationSource)
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
        public virtual InternalPropertyBuilder HasField([CanBeNull] string fieldName, ConfigurationSource configurationSource)
        {
            if (Metadata.FieldInfo?.GetSimpleMemberName() == fieldName
                || configurationSource.Overrides(Metadata.GetFieldInfoConfigurationSource()))
            {
                Metadata.SetField(fieldName, configurationSource);

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
        public virtual bool CanSetField([CanBeNull] string fieldName, ConfigurationSource? configurationSource)
        {
            if (fieldName != null
                && configurationSource.Overrides(Metadata.GetFieldInfoConfigurationSource()))
            {
                var fieldInfo = PropertyBase.GetFieldInfo(
                    fieldName, Metadata.DeclaringType, Metadata.Name,
                    shouldThrow: false);
                return fieldInfo != null
                    && PropertyBase.IsCompatible(
                        fieldInfo, Metadata.ClrType, Metadata.DeclaringType.ClrType, Metadata.Name,
                        shouldThrow: false);
            }

            return Metadata.FieldInfo?.GetSimpleMemberName() == fieldName;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder HasField([CanBeNull] FieldInfo fieldInfo, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetFieldInfoConfigurationSource())
                || Equals(Metadata.FieldInfo, fieldInfo))
            {
                Metadata.SetField(fieldInfo, configurationSource);
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
        public virtual bool CanSetField([CanBeNull] FieldInfo fieldInfo, ConfigurationSource? configurationSource)
            => (configurationSource.Overrides(Metadata.GetFieldInfoConfigurationSource())
                    && (fieldInfo == null
                        || PropertyBase.IsCompatible(
                            fieldInfo, Metadata.ClrType, Metadata.DeclaringType.ClrType, Metadata.Name,
                            shouldThrow: false)))
                || Equals(Metadata.FieldInfo, fieldInfo);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode, ConfigurationSource configurationSource)
        {
            if (CanSetPropertyAccessMode(propertyAccessMode, configurationSource))
            {
                Metadata.SetPropertyAccessMode(propertyAccessMode, configurationSource);

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
        public virtual bool CanSetPropertyAccessMode(
            PropertyAccessMode? propertyAccessMode, ConfigurationSource? configurationSource)
            => configurationSource.Overrides(Metadata.GetPropertyAccessModeConfigurationSource())
                || Metadata.GetPropertyAccessMode() == propertyAccessMode;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder HasMaxLength(int? maxLength, ConfigurationSource configurationSource)
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
        public virtual InternalPropertyBuilder IsUnicode(bool? unicode, ConfigurationSource configurationSource)
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
        public virtual InternalPropertyBuilder BeforeSave(PropertySaveBehavior? behavior, ConfigurationSource configurationSource)
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
        public virtual InternalPropertyBuilder AfterSave(PropertySaveBehavior? behavior, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetAfterSaveBehaviorConfigurationSource())
                || Metadata.GetAfterSaveBehavior() == behavior)
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
        public virtual InternalPropertyBuilder HasValueGenerator(
            [CanBeNull] Type valueGeneratorType, ConfigurationSource configurationSource)
        {
            if (valueGeneratorType == null)
            {
                return HasValueGenerator((Func<IProperty, IEntityType, ValueGenerator>)null, configurationSource);
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
                        return (ValueGenerator)Activator.CreateInstance(valueGeneratorType);
                    }
                    catch (Exception e)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.CannotCreateValueGenerator(valueGeneratorType.ShortDisplayName()), e);
                    }
                }, configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder HasValueGenerator(
            [CanBeNull] Func<IProperty, IEntityType, ValueGenerator> factory,
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
        public virtual bool CanSetValueGenerator(
            [CanBeNull] Func<IProperty, IEntityType, ValueGenerator> factory, ConfigurationSource? configurationSource)
            => configurationSource.Overrides(Metadata.GetValueGeneratorFactoryConfigurationSource())
                || Metadata.GetValueGeneratorFactory() == factory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder HasConversion([CanBeNull] ValueConverter converter, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetValueConverterConfigurationSource())
                || Metadata.GetValueConverter() == converter)
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
            [CanBeNull] ValueConverter converter, ConfigurationSource? configurationSource)
            => (configurationSource.Overrides(Metadata.GetValueConverterConfigurationSource())
                    && Metadata.CheckValueConverter(converter) == null)
                || Metadata.GetValueConverter() == converter;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder HasConversion([CanBeNull] Type providerClrType, ConfigurationSource configurationSource)
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
        public virtual bool CanSetConversion([CanBeNull] Type providerClrType, ConfigurationSource? configurationSource)
            => configurationSource.Overrides(Metadata.GetProviderClrTypeConfigurationSource())
                || Metadata.GetProviderClrType() == providerClrType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder HasValueComparer(
            [CanBeNull] ValueComparer comparer, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetValueComparerConfigurationSource())
                || Metadata.GetValueComparer(fallback: false) == comparer)
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
        public virtual bool CanSetValueComparer([CanBeNull] ValueComparer comparer, ConfigurationSource? configurationSource)
            => (configurationSource.Overrides(Metadata.GetValueComparerConfigurationSource())
                    && Metadata.CheckValueComparer(comparer) == null)
                || Metadata.GetValueComparer(fallback: false) == comparer;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder HasKeyValueComparer(
            [CanBeNull] ValueComparer comparer, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetKeyValueComparerConfigurationSource())
                || Metadata.GetKeyValueComparer(fallback: false) == comparer)
            {
                Metadata.SetKeyValueComparer(comparer, configurationSource);

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
        public virtual bool CanSetKeyValueComparer([CanBeNull] ValueComparer comparer, ConfigurationSource? configurationSource)
            => (configurationSource.Overrides(Metadata.GetKeyValueComparerConfigurationSource())
                    && Metadata.CheckValueComparer(comparer) == null)
                || Metadata.GetKeyValueComparer(fallback: false) == comparer;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder HasStructuralValueComparer(
            [CanBeNull] ValueComparer comparer, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetStructuralValueComparerConfigurationSource())
                || Metadata.GetStructuralValueComparer(fallback: false) == comparer)
            {
                Metadata.SetStructuralValueComparer(comparer, configurationSource);

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
        public virtual bool CanSetStructuralValueComparer([CanBeNull] ValueComparer comparer, ConfigurationSource? configurationSource)
            => (configurationSource.Overrides(Metadata.GetStructuralValueComparerConfigurationSource())
                    && Metadata.CheckValueComparer(comparer) == null)
                || Metadata.GetStructuralValueComparer(fallback: false) == comparer;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder Attach([NotNull] InternalEntityTypeBuilder entityTypeBuilder)
        {
            var newProperty = entityTypeBuilder.Metadata.FindProperty(Metadata.Name);
            InternalPropertyBuilder newPropertyBuilder;
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
                newPropertyBuilder = Metadata.GetIdentifyingMemberInfo() == null
                    ? entityTypeBuilder.Property(
                        Metadata.ClrType, Metadata.Name, Metadata.GetTypeConfigurationSource(), configurationSource)
                    : entityTypeBuilder.Property(Metadata.GetIdentifyingMemberInfo(), configurationSource);
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
        IConventionProperty IConventionPropertyBuilder.Metadata
        {
            [DebuggerStepThrough] get => Metadata;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionPropertyBuilder IConventionPropertyBuilder.IsRequired(bool? required, bool fromDataAnnotation)
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
        IConventionPropertyBuilder IConventionPropertyBuilder.ValueGenerated(ValueGenerated? valueGenerated, bool fromDataAnnotation)
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
        IConventionPropertyBuilder IConventionPropertyBuilder.IsConcurrencyToken(bool? concurrencyToken, bool fromDataAnnotation)
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
        IConventionPropertyBuilder IConventionPropertyBuilder.HasField(string fieldName, bool fromDataAnnotation)
            => HasField(fieldName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionPropertyBuilder IConventionPropertyBuilder.HasField(FieldInfo fieldInfo, bool fromDataAnnotation)
            => HasField(fieldInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionPropertyBuilder.CanSetField(string fieldName, bool fromDataAnnotation)
            => CanSetField(fieldName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionPropertyBuilder.CanSetField(FieldInfo fieldInfo, bool fromDataAnnotation)
            => CanSetField(fieldInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionPropertyBuilder IConventionPropertyBuilder.UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation)
            => UsePropertyAccessMode(
                propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionPropertyBuilder.CanSetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation)
            => CanSetPropertyAccessMode(
                propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionPropertyBuilder IConventionPropertyBuilder.HasMaxLength(int? maxLength, bool fromDataAnnotation)
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
        IConventionPropertyBuilder IConventionPropertyBuilder.IsUnicode(bool? unicode, bool fromDataAnnotation)
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
        IConventionPropertyBuilder IConventionPropertyBuilder.BeforeSave(PropertySaveBehavior? behavior, bool fromDataAnnotation)
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
        IConventionPropertyBuilder IConventionPropertyBuilder.AfterSave(PropertySaveBehavior? behavior, bool fromDataAnnotation)
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
        IConventionPropertyBuilder IConventionPropertyBuilder.HasValueGenerator(Type valueGeneratorType, bool fromDataAnnotation)
            => HasValueGenerator(
                valueGeneratorType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionPropertyBuilder IConventionPropertyBuilder.HasValueGenerator(
            Func<IProperty, IEntityType, ValueGenerator> factory, bool fromDataAnnotation)
            => HasValueGenerator(factory, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionPropertyBuilder.CanSetValueGenerator(Func<IProperty, IEntityType, ValueGenerator> factory, bool fromDataAnnotation)
            => CanSetValueGenerator(factory, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionPropertyBuilder IConventionPropertyBuilder.HasConversion(ValueConverter converter, bool fromDataAnnotation)
            => HasConversion(converter, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionPropertyBuilder.CanSetConversion(ValueConverter converter, bool fromDataAnnotation)
            => CanSetConversion(converter, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionPropertyBuilder IConventionPropertyBuilder.HasConversion(Type providerClrType, bool fromDataAnnotation)
            => HasConversion(providerClrType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionPropertyBuilder.CanSetConversion(Type providerClrType, bool fromDataAnnotation)
            => CanSetConversion(providerClrType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionPropertyBuilder IConventionPropertyBuilder.HasValueComparer(ValueComparer comparer, bool fromDataAnnotation)
            => HasValueComparer(comparer, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionPropertyBuilder.CanSetValueComparer(ValueComparer comparer, bool fromDataAnnotation)
            => CanSetValueComparer(comparer, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionPropertyBuilder IConventionPropertyBuilder.HasKeyValueComparer(ValueComparer comparer, bool fromDataAnnotation)
            => HasKeyValueComparer(comparer, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionPropertyBuilder.CanSetKeyValueComparer(ValueComparer comparer, bool fromDataAnnotation)
            => CanSetKeyValueComparer(comparer, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionPropertyBuilder IConventionPropertyBuilder.HasStructuralValueComparer(ValueComparer comparer, bool fromDataAnnotation)
            => HasStructuralValueComparer(
                comparer, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionPropertyBuilder.CanSetStructuralValueComparer(ValueComparer comparer, bool fromDataAnnotation)
            => CanSetStructuralValueComparer(
                comparer, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }
}
