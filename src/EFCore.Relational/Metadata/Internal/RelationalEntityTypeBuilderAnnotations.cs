// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    // Issue#11266 This type is being used by provider code. Do not break.
    public class RelationalEntityTypeBuilderAnnotations : RelationalEntityTypeAnnotations
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected readonly string DefaultDiscriminatorName = "Discriminator";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalEntityTypeBuilderAnnotations(
            [NotNull] InternalEntityTypeBuilder internalBuilder,
            ConfigurationSource configurationSource)
            : base(new RelationalAnnotationsBuilder(internalBuilder, configurationSource))
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected new virtual RelationalAnnotationsBuilder Annotations => (RelationalAnnotationsBuilder)base.Annotations;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalEntityTypeBuilder EntityTypeBuilder => (InternalEntityTypeBuilder)Annotations.MetadataBuilder;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override RelationalModelAnnotations GetAnnotations(IModel model)
            => new RelationalModelBuilderAnnotations(
                ((Model)model).Builder,
                Annotations.ConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override RelationalEntityTypeAnnotations GetAnnotations(IEntityType entityType)
            => new RelationalEntityTypeBuilderAnnotations(
                ((EntityType)entityType).Builder,
                Annotations.ConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ToTable([CanBeNull] string name)
        {
            Check.NullButNotEmpty(name, nameof(name));

            return SetTableName(name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ToSchema([CanBeNull] string name)
        {
            Check.NullButNotEmpty(name, nameof(name));

            return SetSchema(name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ToTable([CanBeNull] string name, [CanBeNull] string schema)
        {
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var originalTable = TableName;
            if (!SetTableName(name))
            {
                return false;
            }

            if (!SetSchema(schema))
            {
                SetTableName(originalTable);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DiscriminatorBuilder HasDiscriminator() => DiscriminatorBuilder(null, null);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DiscriminatorBuilder HasDiscriminator([CanBeNull] Type discriminatorType)
        {
            return discriminatorType == null
                ? RemoveDiscriminator()
                : DiscriminatorProperty != null
                   && DiscriminatorProperty.ClrType == discriminatorType
                ? DiscriminatorBuilder(null, null)
                : DiscriminatorBuilder(null, discriminatorType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DiscriminatorBuilder HasDiscriminator([NotNull] string name, [NotNull] Type discriminatorType)
            => DiscriminatorProperty != null
               && DiscriminatorProperty.Name == name
               && DiscriminatorProperty.ClrType == discriminatorType
                ? DiscriminatorBuilder(null, null)
                : DiscriminatorBuilder(b => b.Property(name, discriminatorType, Annotations.ConfigurationSource), null);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DiscriminatorBuilder HasDiscriminator([CanBeNull] PropertyInfo propertyInfo)
        {
            return propertyInfo == null
                ? RemoveDiscriminator()
                : DiscriminatorProperty != null
                   && DiscriminatorProperty.Name == propertyInfo.GetSimpleMemberName()
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
                    => new RelationalEntityTypeBuilderAnnotations(entityBuilder, Annotations.ConfigurationSource));
        }

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
                    => new RelationalEntityTypeBuilderAnnotations(entityBuilder, Annotations.ConfigurationSource));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool HasDiscriminatorValue([CanBeNull] object value) => SetDiscriminatorValue(value);
    }
}
