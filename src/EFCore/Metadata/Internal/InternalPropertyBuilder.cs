// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [DebuggerDisplay("{Metadata,nq}")]
    // Issue#11266 This type is being used by provider code. Do not break.
    public class InternalPropertyBuilder : InternalMetadataItemBuilder<Property>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalPropertyBuilder([NotNull] Property property, [NotNull] InternalModelBuilder modelBuilder)
            : base(property, modelBuilder)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsRequired(bool isRequired, ConfigurationSource configurationSource)
        {
            if (CanSetRequired(isRequired, configurationSource))
            {
                if (!isRequired)
                {
                    foreach (var key in Metadata.GetContainingKeys().ToList())
                    {
                        if (configurationSource == ConfigurationSource.Explicit
                            && key.GetConfigurationSource() == ConfigurationSource.Explicit)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.KeyPropertyCannotBeNullable(Metadata.Name, Metadata.DeclaringEntityType.DisplayName(), Property.Format(key.Properties)));
                        }

                        var removed = key.DeclaringEntityType.Builder.RemoveKey(key, configurationSource);
                        Debug.Assert(removed.HasValue);
                    }
                }

                Metadata.SetIsNullable(!isRequired, configurationSource);

                return true;
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool CanSetRequired(bool isRequired, ConfigurationSource? configurationSource)
            => ((Metadata.IsNullable == !isRequired)
                || (configurationSource.HasValue
                    && configurationSource.Value.Overrides(Metadata.GetIsNullableConfigurationSource())))
               && (isRequired
                   || Metadata.ClrType.IsNullableType()
                   || (configurationSource == ConfigurationSource.Explicit)); // let it throw for Explicit

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasMaxLength(int maxLength, ConfigurationSource configurationSource)
            => HasAnnotation(CoreAnnotationNames.MaxLengthAnnotation, maxLength, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsUnicode(bool unicode, ConfigurationSource configurationSource)
            => HasAnnotation(CoreAnnotationNames.UnicodeAnnotation, unicode, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasValueGenerator([CanBeNull] Type valueGeneratorType, ConfigurationSource configurationSource)
        {
            if (valueGeneratorType == null)
            {
                return HasValueGenerator((Func<IProperty, IEntityType, ValueGenerator>)null, configurationSource);
            }

            if (!typeof(ValueGenerator).GetTypeInfo().IsAssignableFrom(valueGeneratorType.GetTypeInfo()))
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasValueGenerator(
            [CanBeNull] Func<IProperty, IEntityType, ValueGenerator> factory,
            ConfigurationSource configurationSource)
            => HasAnnotation(CoreAnnotationNames.ValueGeneratorFactoryAnnotation, factory, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasField([CanBeNull] string fieldName, ConfigurationSource configurationSource)
        {
            if (Metadata.FieldInfo?.GetSimpleMemberName() == fieldName)
            {
                Metadata.SetField(fieldName, configurationSource);
                return true;
            }

            if (!configurationSource.Overrides(Metadata.GetFieldInfoConfigurationSource()))
            {
                return false;
            }

            if (fieldName != null)
            {
                var fieldInfo = PropertyBase.GetFieldInfo(
                    fieldName, Metadata.DeclaringType, Metadata.Name,
                    shouldThrow: configurationSource == ConfigurationSource.Explicit);
                Metadata.SetFieldInfo(fieldInfo, configurationSource);
                return true;
            }

            Metadata.SetField(fieldName, configurationSource);
            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasFieldInfo([CanBeNull] FieldInfo fieldInfo, ConfigurationSource configurationSource)
        {
            if ((configurationSource.Overrides(Metadata.GetFieldInfoConfigurationSource())
                 && (fieldInfo == null
                     || PropertyBase.IsCompatible(
                         fieldInfo, Metadata.ClrType, Metadata.DeclaringType.ClrType, Metadata.Name,
                         shouldThrow: configurationSource == ConfigurationSource.Explicit)))
                || Equals(Metadata.FieldInfo, fieldInfo))
            {
                Metadata.SetFieldInfo(fieldInfo, configurationSource);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool UsePropertyAccessMode(PropertyAccessMode propertyAccessMode, ConfigurationSource configurationSource)
            => HasAnnotation(CoreAnnotationNames.PropertyAccessModeAnnotation, propertyAccessMode, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasConversion([CanBeNull] ValueConverter valueConverter, ConfigurationSource configurationSource)
        {
            if (valueConverter != null
                && valueConverter.ModelClrType.UnwrapNullableType() != Metadata.ClrType.UnwrapNullableType())
            {
                throw new ArgumentException(
                    CoreStrings.ConverterPropertyMismatch(
                        valueConverter.ModelClrType.ShortDisplayName(),
                        Metadata.DeclaringEntityType.DisplayName(),
                        Metadata.Name,
                        Metadata.ClrType.ShortDisplayName()));
            }

            return HasAnnotation(CoreAnnotationNames.ValueConverter, valueConverter, configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasConversion([CanBeNull] Type providerClrType, ConfigurationSource configurationSource)
            => HasAnnotation(CoreAnnotationNames.ProviderClrType, providerClrType, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsConcurrencyToken(bool concurrencyToken, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetIsConcurrencyTokenConfigurationSource())
                || (Metadata.IsConcurrencyToken == concurrencyToken))
            {
                Metadata.SetIsConcurrencyToken(concurrencyToken, configurationSource);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool BeforeSave(PropertySaveBehavior? behavior, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetBeforeSaveBehaviorConfigurationSource())
                || Metadata.BeforeSaveBehavior == behavior)
            {
                Metadata.SetBeforeSaveBehavior(behavior, configurationSource);

                return true;
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool AfterSave(PropertySaveBehavior? behavior, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetAfterSaveBehaviorConfigurationSource())
                || Metadata.AfterSaveBehavior == behavior)
            {
                Metadata.SetAfterSaveBehavior(behavior, configurationSource);

                return true;
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ValueGenerated(ValueGenerated? valueGenerated, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetValueGeneratedConfigurationSource())
                || Metadata.ValueGenerated == valueGenerated)
            {
                Metadata.SetValueGenerated(valueGenerated, configurationSource);

                return true;
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
                    ? entityTypeBuilder.Property(Metadata.Name, Metadata.ClrType, configurationSource, Metadata.GetTypeConfigurationSource())
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
                    Metadata.BeforeSaveBehavior,
                    oldBeforeSaveBehaviorConfigurationSource.Value);
            }

            var oldAfterSaveBehaviorConfigurationSource = Metadata.GetAfterSaveBehaviorConfigurationSource();
            if (oldAfterSaveBehaviorConfigurationSource.HasValue)
            {
                newPropertyBuilder.AfterSave(
                    Metadata.AfterSaveBehavior,
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
            if (oldFieldInfoConfigurationSource.HasValue)
            {
                newPropertyBuilder.HasFieldInfo(Metadata.FieldInfo, oldFieldInfoConfigurationSource.Value);
            }

            return newPropertyBuilder;
        }
    }
}
