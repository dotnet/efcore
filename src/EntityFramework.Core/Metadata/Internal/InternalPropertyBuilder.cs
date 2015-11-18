// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalPropertyBuilder : InternalMetadataItemBuilder<Property>
    {
        public InternalPropertyBuilder([NotNull] Property property, [NotNull] InternalModelBuilder modelBuilder)
            : base(property, modelBuilder)
        {
        }

        public virtual bool IsRequired(bool isRequired, ConfigurationSource configurationSource)
        {
            if (CanSetRequired(isRequired, configurationSource))
            {
                Metadata.SetIsNullable(!isRequired, configurationSource);
                
                return true;
            }

            return false;
        }

        public virtual bool CanSetRequired(bool isRequired, ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetIsNullableConfigurationSource())
               || (Metadata.IsNullable == !isRequired);

        public virtual bool HasMaxLength(int maxLength, ConfigurationSource configurationSource)
            => HasAnnotation(CoreAnnotationNames.MaxLengthAnnotation, maxLength, configurationSource);

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

        public virtual bool ReadOnlyAfterSave(bool isReadOnlyAfterSave, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetIsReadOnlyAfterSaveConfigurationSource())
                || (Metadata.IsReadOnlyAfterSave == isReadOnlyAfterSave))
            {
                Metadata.SetIsReadOnlyAfterSave(isReadOnlyAfterSave, configurationSource);
                return true;
            }

            return false;
        }

        public virtual bool ReadOnlyBeforeSave(bool isReadOnlyBeforeSave, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetIsReadOnlyBeforeSaveConfigurationSource())
                || (Metadata.IsReadOnlyBeforeSave == isReadOnlyBeforeSave))
            {
                Metadata.SetIsReadOnlyBeforeSave(isReadOnlyBeforeSave, configurationSource);
                return true;
            }

            return false;
        }

        public virtual bool IsShadow(bool isShadowProperty, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetIsShadowPropertyConfigurationSource())
                || (Metadata.IsShadowProperty == isShadowProperty))
            {
                Metadata.SetIsShadowProperty(isShadowProperty, configurationSource);
                return true;
            }

            return false;
        }

        public virtual bool HasClrType([NotNull] Type propertyType, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetClrTypeConfigurationSource())
                || (Metadata.ClrType == propertyType))
            {
                Metadata.HasClrType(propertyType, configurationSource);
                return true;
            }

            return false;
        }

        public virtual bool RequiresValueGenerator(bool generateValue, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetRequiresValueGeneratorConfigurationSource())
                || (Metadata.RequiresValueGenerator == generateValue))
            {
                Metadata.SetRequiresValueGenerator(generateValue, configurationSource);
                return true;
            }

            return false;
        }

        public virtual bool ValueGenerated(ValueGenerated valueGenerated, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetValueGeneratedConfigurationSource())
                || (Metadata.ValueGenerated == valueGenerated))
            {
                Metadata.SetValueGenerated(valueGenerated, configurationSource);

                if (Metadata.IsKey())
                {
                    RequiresValueGenerator(
                        valueGenerated == Entity.Metadata.ValueGenerated.OnAdd,
                        ConfigurationSource.Convention);
                }

                return true;
            }

            return false;
        }

        public virtual InternalPropertyBuilder Attach(
            [NotNull] InternalEntityTypeBuilder entityTypeBuilder, ConfigurationSource configurationSource)
        {
            var newProperty = Metadata.DeclaringEntityType.FindProperty(Metadata.Name);
            Debug.Assert(newProperty != null);
            var newPropertyBuilder = entityTypeBuilder.Property(Metadata.Name, configurationSource);
            if (newProperty == Metadata)
            {
                return newPropertyBuilder;
            }

            newPropertyBuilder.MergeAnnotationsFrom(this);

            if (Metadata.GetClrTypeConfigurationSource().HasValue)
            {
                newPropertyBuilder.HasClrType(Metadata.ClrType, Metadata.GetClrTypeConfigurationSource().Value);
            }
            if (Metadata.GetIsReadOnlyAfterSaveConfigurationSource().HasValue)
            {
                newPropertyBuilder.ReadOnlyAfterSave(Metadata.IsReadOnlyAfterSave,
                    Metadata.GetIsReadOnlyAfterSaveConfigurationSource().Value);
            }
            if (Metadata.GetIsReadOnlyBeforeSaveConfigurationSource().HasValue)
            {
                newPropertyBuilder.ReadOnlyBeforeSave(Metadata.IsReadOnlyBeforeSave,
                    Metadata.GetIsReadOnlyBeforeSaveConfigurationSource().Value);
            }
            if (Metadata.GetIsNullableConfigurationSource().HasValue)
            {
                newPropertyBuilder.IsRequired(!Metadata.IsNullable, Metadata.GetIsNullableConfigurationSource().Value);
            }
            if (Metadata.GetIsConcurrencyTokenConfigurationSource().HasValue)
            {
                newPropertyBuilder.IsConcurrencyToken(Metadata.IsConcurrencyToken,
                    Metadata.GetIsConcurrencyTokenConfigurationSource().Value);
            }
            if (Metadata.GetIsShadowPropertyConfigurationSource().HasValue)
            {
                newPropertyBuilder.IsShadow(Metadata.IsShadowProperty, Metadata.GetIsShadowPropertyConfigurationSource().Value);
            }
            if (Metadata.GetRequiresValueGeneratorConfigurationSource().HasValue)
            {
                newPropertyBuilder.RequiresValueGenerator(Metadata.RequiresValueGenerator,
                    Metadata.GetRequiresValueGeneratorConfigurationSource().Value);
            }
            if (Metadata.GetValueGeneratedConfigurationSource().HasValue)
            {
                newPropertyBuilder.ValueGenerated(Metadata.ValueGenerated, Metadata.GetValueGeneratedConfigurationSource().Value);
            }

            return newPropertyBuilder;
        }
    }
}
