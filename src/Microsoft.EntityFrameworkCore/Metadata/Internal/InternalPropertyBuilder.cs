// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [DebuggerDisplay("{Metadata,nq}")]
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
                        var removed = key.DeclaringEntityType.Builder.RemoveKey(key, configurationSource);
                        Debug.Assert(removed.HasValue);
                    }
                }
                Metadata.Facets.SetIsNullable(!isRequired, configurationSource);

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
                || (configurationSource.HasValue &&
                    configurationSource.Value.Overrides(Metadata.Facets.IsNullableConfigurationSource)))
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

            return HasValueGenerator((_, __)
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
        {
            if (HasAnnotation(CoreAnnotationNames.ValueGeneratorFactoryAnnotation, factory, configurationSource))
            {
                RequiresValueGenerator(factory != null, ConfigurationSource.Convention);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasField([CanBeNull] string fieldName, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetFieldInfoConfigurationSource()))
            {
                Metadata.SetField(fieldName, configurationSource);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasFieldInfo([CanBeNull] FieldInfo fieldInfo, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetFieldInfoConfigurationSource()))
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
        public virtual bool IsConcurrencyToken(bool concurrencyToken, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.Facets.IsConcurrencyTokenConfigurationSource)
                || (Metadata.IsConcurrencyToken == concurrencyToken))
            {
                Metadata.Facets.SetIsConcurrencyToken(concurrencyToken, configurationSource);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ReadOnlyAfterSave(bool isReadOnlyAfterSave, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.Facets.IsReadOnlyAfterSaveConfigurationSource)
                || (Metadata.IsReadOnlyAfterSave == isReadOnlyAfterSave))
            {
                Metadata.Facets.SetIsReadOnlyAfterSave(isReadOnlyAfterSave, configurationSource);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ReadOnlyBeforeSave(bool isReadOnlyBeforeSave, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.Facets.IsReadOnlyBeforeSaveConfigurationSource)
                || (Metadata.IsReadOnlyBeforeSave == isReadOnlyBeforeSave))
            {
                Metadata.Facets.SetIsReadOnlyBeforeSave(isReadOnlyBeforeSave, configurationSource);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool RequiresValueGenerator(bool generateValue, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.Facets.RequiresValueGeneratorConfigurationSource)
                || (Metadata.RequiresValueGenerator == generateValue))
            {
                Metadata.Facets.SetRequiresValueGenerator(generateValue, configurationSource);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ValueGenerated(ValueGenerated valueGenerated, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.Facets.ValueGeneratedConfigurationSource)
                || (Metadata.ValueGenerated == valueGenerated))
            {
                Metadata.Facets.SetValueGenerated(valueGenerated, configurationSource);

                return true;
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsStoreGeneratedAlways(bool isStoreGeneratedAlways, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.Facets.IsStoreGeneratedAlwaysConfigurationSource)
                || (Metadata.IsStoreGeneratedAlways == isStoreGeneratedAlways))
            {
                Metadata.Facets.SetIsStoreGeneratedAlways(isStoreGeneratedAlways, configurationSource);

                return true;
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalPropertyBuilder Attach(
            [NotNull] InternalEntityTypeBuilder entityTypeBuilder, ConfigurationSource configurationSource)
        {
            var newProperty = Metadata.DeclaringEntityType.FindProperty(Metadata.Name);
            InternalPropertyBuilder newPropertyBuilder = null;
            if (newProperty != null
                && newProperty.GetConfigurationSource().Overrides(configurationSource)
                && ((Metadata.ClrType != null
                     && Metadata.ClrType != newProperty.ClrType)
                    || (Metadata.PropertyInfo != null
                        && newProperty.PropertyInfo == null)))
            {
                newPropertyBuilder = newProperty.Builder;
            }
            else
            {
                newPropertyBuilder = Metadata.PropertyInfo == null
                    ? entityTypeBuilder.Property(Metadata.Name, Metadata.ClrType, configurationSource)
                    : entityTypeBuilder.Property(Metadata.PropertyInfo, configurationSource);
            }

            if (newProperty == Metadata)
            {
                return newPropertyBuilder;
            }

            newPropertyBuilder.MergeAnnotationsFrom(this);

            var oldIsReadOnlyAfterSaveConfigurationSource = Metadata.Facets.IsReadOnlyAfterSaveConfigurationSource;
            if (oldIsReadOnlyAfterSaveConfigurationSource.HasValue)
            {
                newPropertyBuilder.ReadOnlyAfterSave(Metadata.IsReadOnlyAfterSave,
                    oldIsReadOnlyAfterSaveConfigurationSource.Value);
            }
            var oldIsReadOnlyBeforeSaveConfigurationSource = Metadata.Facets.IsReadOnlyBeforeSaveConfigurationSource;
            if (oldIsReadOnlyBeforeSaveConfigurationSource.HasValue)
            {
                newPropertyBuilder.ReadOnlyBeforeSave(Metadata.IsReadOnlyBeforeSave,
                    oldIsReadOnlyBeforeSaveConfigurationSource.Value);
            }
            var oldIsNullableConfigurationSource = Metadata.Facets.IsNullableConfigurationSource;
            if (oldIsNullableConfigurationSource.HasValue)
            {
                newPropertyBuilder.IsRequired(!Metadata.IsNullable, oldIsNullableConfigurationSource.Value);
            }
            var oldIsConcurrencyTokenConfigurationSource = Metadata.Facets.IsConcurrencyTokenConfigurationSource;
            if (oldIsConcurrencyTokenConfigurationSource.HasValue)
            {
                newPropertyBuilder.IsConcurrencyToken(Metadata.IsConcurrencyToken,
                    oldIsConcurrencyTokenConfigurationSource.Value);
            }
            var oldRequiresValueGeneratorConfigurationSource = Metadata.Facets.RequiresValueGeneratorConfigurationSource;
            if (oldRequiresValueGeneratorConfigurationSource.HasValue)
            {
                newPropertyBuilder.RequiresValueGenerator(Metadata.RequiresValueGenerator,
                    oldRequiresValueGeneratorConfigurationSource.Value);
            }
            var oldValueGeneratedConfigurationSource = Metadata.Facets.ValueGeneratedConfigurationSource;
            if (oldValueGeneratedConfigurationSource.HasValue)
            {
                newPropertyBuilder.ValueGenerated(Metadata.ValueGenerated, oldValueGeneratedConfigurationSource.Value);
            }
            var oldIsStoreGeneratedAlwaysConfigurationSource = Metadata.Facets.IsStoreGeneratedAlwaysConfigurationSource;
            if (oldIsStoreGeneratedAlwaysConfigurationSource.HasValue)
            {
                newPropertyBuilder.IsStoreGeneratedAlways(Metadata.IsStoreGeneratedAlways, oldIsStoreGeneratedAlwaysConfigurationSource.Value);
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
