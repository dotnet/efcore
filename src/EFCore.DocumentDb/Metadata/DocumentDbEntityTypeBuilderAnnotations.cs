// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class DocumentDbEntityTypeBuilderAnnotations : DocumentDbEntityTypeAnnotations
    {
        protected readonly string DefaultDiscriminatorName = "Discriminator";

        public DocumentDbEntityTypeBuilderAnnotations(
            InternalEntityTypeBuilder internalBuilder,
            ConfigurationSource configurationSource)
            : base(new DocumentDbAnnotationsBuilder(internalBuilder, configurationSource))
        {
        }

        protected new virtual DocumentDbAnnotationsBuilder Annotations => (DocumentDbAnnotationsBuilder)base.Annotations;
        protected virtual InternalEntityTypeBuilder EntityTypeBuilder => (InternalEntityTypeBuilder)Annotations.MetadataBuilder;

        public virtual bool ToCollection([CanBeNull] string name)
        {
            Check.NullButNotEmpty(name, nameof(name));

            return SetCollectionName(name);
        }

        public virtual bool HasDiscriminatorValue([CanBeNull] object value) => SetDiscriminatorValue(value);
        public virtual DiscriminatorBuilder HasDiscriminator([CanBeNull] PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                return RemoveDiscriminator();
            }

            return DiscriminatorProperty != null
                   && DiscriminatorProperty.Name == propertyInfo.Name
                   && DiscriminatorProperty.ClrType == propertyInfo.PropertyType
                ? DiscriminatorBuilder(null, null)
                : DiscriminatorBuilder(b => b.Property(propertyInfo, Annotations.ConfigurationSource), null);
        }
        private DiscriminatorBuilder RemoveDiscriminator()
        {
            var discriminatorProperty = (Property)GetNonRootDiscriminatorProperty();
            if (discriminatorProperty != null)
            {
                if (!SetDiscriminatorProperty(null))
                {
                    return null;
                }

                if (discriminatorProperty.DeclaringEntityType == EntityTypeBuilder.Metadata)
                {
                    EntityTypeBuilder.RemoveShadowPropertiesIfUnused(new[] { discriminatorProperty });
                }
            }

            return new DiscriminatorBuilder(
                Annotations, entityBuilder
                    => new DocumentDbEntityTypeBuilderAnnotations(entityBuilder, Annotations.ConfigurationSource));
        }

        protected virtual ConfigurationSource? GetDiscriminatorPropertyConfigurationSource()
            => (EntityType as EntityType)
            ?.FindAnnotation(DocumentDbAnnotationNames.DiscriminatorProperty)
            ?.GetConfigurationSource();

        private DiscriminatorBuilder DiscriminatorBuilder(
            [CanBeNull] Func<InternalEntityTypeBuilder, InternalPropertyBuilder> createProperty,
            [CanBeNull] Type propertyType)
        {
            var configurationSource = Annotations.ConfigurationSource;
            var discriminatorProperty = DiscriminatorProperty;
            if (discriminatorProperty != null
                && (createProperty != null || propertyType != null)
                && !configurationSource.Overrides(GetDiscriminatorPropertyConfigurationSource()))
            {
                return null;
            }

            var rootType = EntityTypeBuilder.Metadata.RootType();
            var rootTypeBuilder = EntityTypeBuilder.Metadata == rootType
                ? EntityTypeBuilder
                : rootType.Builder;

            InternalPropertyBuilder propertyBuilder;
            if (createProperty != null)
            {
                propertyBuilder = createProperty(rootTypeBuilder);
            }
            else if (propertyType != null)
            {
                propertyBuilder = rootTypeBuilder.Property(DefaultDiscriminatorName, propertyType, configurationSource);
            }
            else if (discriminatorProperty == null)
            {
                propertyBuilder = rootTypeBuilder.Property(DefaultDiscriminatorName, typeof(string), ConfigurationSource.Convention);
            }
            else
            {
                propertyBuilder = rootTypeBuilder.Property(discriminatorProperty.Name, ConfigurationSource.Convention);
            }

            if (propertyBuilder == null)
            {
                return null;
            }

            var oldDiscriminatorProperty = discriminatorProperty as Property;
            if (oldDiscriminatorProperty?.Builder != null
                && (createProperty != null || propertyType != null)
                && propertyBuilder.Metadata != discriminatorProperty)
            {
                if (discriminatorProperty.DeclaringEntityType == EntityTypeBuilder.Metadata)
                {
                    EntityTypeBuilder.RemoveShadowPropertiesIfUnused(new[] { oldDiscriminatorProperty });
                }
            }

            if (discriminatorProperty == null
                || createProperty != null
                || propertyType != null)
            {
                var discriminatorSet = SetDiscriminatorProperty(propertyBuilder.Metadata, discriminatorProperty?.ClrType);
                Debug.Assert(discriminatorSet);
            }

            propertyBuilder.IsRequired(true, configurationSource);
            propertyBuilder.AfterSave(PropertySaveBehavior.Throw, configurationSource);
            propertyBuilder.HasValueGenerator(
                (property, entityType) => new DiscriminatorValueGenerator(GetAnnotations(entityType).DiscriminatorValue),
                configurationSource);

            return new DiscriminatorBuilder(
                Annotations, entityBuilder
                    => new DocumentDbEntityTypeBuilderAnnotations(entityBuilder, Annotations.ConfigurationSource));
        }

        public virtual DiscriminatorBuilder HasDiscriminator([CanBeNull] Type discriminatorType)
        {
            if (discriminatorType == null)
            {
                return RemoveDiscriminator();
            }

            return DiscriminatorProperty != null
                   && DiscriminatorProperty.ClrType == discriminatorType
                ? DiscriminatorBuilder(null, null)
                : DiscriminatorBuilder(null, discriminatorType);
        }
    }
}
