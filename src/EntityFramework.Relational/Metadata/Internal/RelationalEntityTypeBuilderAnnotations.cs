// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class RelationalEntityTypeBuilderAnnotations : RelationalEntityTypeAnnotations
    {
        public RelationalEntityTypeBuilderAnnotations(
            [NotNull] InternalEntityTypeBuilder internalBuilder,
            ConfigurationSource configurationSource,
            [CanBeNull] string providerPrefix)
            : base(new RelationalAnnotationsBuilder(internalBuilder, configurationSource, providerPrefix))
        {
        }

        public new virtual RelationalAnnotationsBuilder Annotations => (RelationalAnnotationsBuilder)base.Annotations;

        public virtual InternalEntityTypeBuilder EntityTypeBuilder => (InternalEntityTypeBuilder)Annotations.EntityTypeBuilder;

        public virtual bool ToTable([CanBeNull] string name)
        {
            Check.NullButNotEmpty(name, nameof(name));

            return SetTableName(name);
        }

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

        [CanBeNull]
        public virtual DiscriminatorBuilder HasDiscriminator() => DiscriminatorBuilder(null, null);

        [CanBeNull]
        public virtual DiscriminatorBuilder HasDiscriminator([CanBeNull] Type discriminatorType)
        {
            if (discriminatorType == null)
            {
                return RemoveDiscriminator();
            }

            return DiscriminatorBuilder(null, discriminatorType);
        }

        [CanBeNull]
        public virtual DiscriminatorBuilder HasDiscriminator([NotNull] string name, [NotNull] Type discriminatorType)
            => DiscriminatorBuilder(b => b.Property(name, discriminatorType, Annotations.ConfigurationSource), null);

        [CanBeNull]
        public virtual DiscriminatorBuilder HasDiscriminator([CanBeNull] PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                return RemoveDiscriminator();
            }

            return DiscriminatorBuilder(b => b.Property(propertyInfo, Annotations.ConfigurationSource), null);
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

            return new DiscriminatorBuilder(this);
        }

        private DiscriminatorBuilder DiscriminatorBuilder(
            [CanBeNull] Func<InternalEntityTypeBuilder, InternalPropertyBuilder> createProperty,
            [CanBeNull] Type propertyType)
        {
            var discriminatorProperty = DiscriminatorProperty;
            if (discriminatorProperty != null
                && createProperty != null)
            {
                if (!SetDiscriminatorProperty(null))
                {
                    return null;
                }
            }
            var rootType = EntityTypeBuilder.Metadata.RootType();
            var rootTypeBuilder = EntityTypeBuilder.Metadata == rootType
                ? EntityTypeBuilder
                : EntityTypeBuilder.ModelBuilder.Entity(rootType.Name, ConfigurationSource.Convention);

            InternalPropertyBuilder propertyBuilder;
            if (createProperty != null)
            {
                propertyBuilder = createProperty(rootTypeBuilder);
            }
            else if (discriminatorProperty == null)
            {
                propertyBuilder = rootTypeBuilder.Property(GetDefaultDiscriminatorName(), ConfigurationSource.Convention);
            }
            else
            {
                propertyBuilder = rootTypeBuilder.Property(discriminatorProperty.Name, ConfigurationSource.Convention);
            }

            if (propertyBuilder == null)
            {
                if (discriminatorProperty != null
                    && createProperty != null)
                {
                    SetDiscriminatorProperty(discriminatorProperty);
                }
                return null;
            }

            if (discriminatorProperty != null
                && createProperty != null
                && propertyBuilder.Metadata != discriminatorProperty)
            {
                if (discriminatorProperty.DeclaringEntityType == EntityTypeBuilder.Metadata)
                {
                    EntityTypeBuilder.RemoveShadowPropertiesIfUnused(new[] { (Property)discriminatorProperty });
                }
            }

            var configurationSource = Annotations.ConfigurationSource;
            if (propertyType != null)
            {
                if (!propertyBuilder.ClrType(propertyType, configurationSource))
                {
                    return null;
                }
            }

            var discriminatorSet = SetDiscriminatorProperty(propertyBuilder.Metadata);
            Debug.Assert(discriminatorSet);

            propertyBuilder.IsRequired(true, configurationSource);
            //propertyBuilder.ReadOnlyBeforeSave(true, configurationSource);// #2132
            propertyBuilder.ReadOnlyAfterSave(true, configurationSource);
            propertyBuilder.UseValueGenerator(true, configurationSource);

            return new DiscriminatorBuilder(this);
        }

        public new virtual bool DiscriminatorValue([CanBeNull] object value) => SetDiscriminatorValue(value);

        protected virtual string GetDefaultDiscriminatorName() => "Discriminator";
    }
}
