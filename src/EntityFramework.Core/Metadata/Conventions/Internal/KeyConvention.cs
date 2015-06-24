// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class KeyConvention : IKeyConvention, IForeignKeyRemovedConvention, IPrimaryKeyConvention
    {
        public virtual InternalKeyBuilder Apply(InternalKeyBuilder keyBuilder)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));

            var entityTypeBuilder = keyBuilder.ModelBuilder.Entity(
                keyBuilder.Metadata.DeclaringEntityType.Name, ConfigurationSource.Convention);

            SetValueGeneration(entityTypeBuilder, keyBuilder.Metadata.Properties);

            return keyBuilder;
        }

        public virtual void Apply(InternalEntityTypeBuilder entityTypeBuilder, ForeignKey foreignKey)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(foreignKey, nameof(foreignKey));

            var properties = foreignKey.Properties;
            SetValueGeneration(entityTypeBuilder, properties);

            var valueGeneratedOnAddProperty = FindValueGeneratedOnAddProperty(properties);
            if (valueGeneratedOnAddProperty != null
                && entityTypeBuilder.Metadata.FindPrimaryKey(properties) != null)
            {
                SetIdentity(entityTypeBuilder, valueGeneratedOnAddProperty);
            }
        }

        public virtual bool Apply(InternalKeyBuilder keyBuilder, Key previousPrimaryKey)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));

            var entityTypeBuilder = keyBuilder.ModelBuilder.Entity(
                keyBuilder.Metadata.DeclaringEntityType.Name, ConfigurationSource.Convention);

            if (previousPrimaryKey != null)
            {
                foreach (var property in previousPrimaryKey.Properties)
                {
                    if (entityTypeBuilder.Metadata.FindProperty(property.Name) != null)
                    {
                        entityTypeBuilder.Property(property.Name, ConfigurationSource.Convention)
                            ?.ValueGenerated(null, ConfigurationSource.Convention);
                    }
                }
            }

            var valueGeneratedOnAddProperty = FindValueGeneratedOnAddProperty(keyBuilder.Metadata.Properties);
            if (valueGeneratedOnAddProperty != null
                && !valueGeneratedOnAddProperty.IsForeignKey(entityTypeBuilder.Metadata))
            {
                SetIdentity(entityTypeBuilder, valueGeneratedOnAddProperty);
            }

            return true;
        }

        protected virtual void SetValueGeneration(
            [NotNull] InternalEntityTypeBuilder entityTypeBuilder,
            [NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(properties, nameof(properties));

            var propertyBuilders = InternalEntityTypeBuilder.GetPropertyBuilders(
                entityTypeBuilder.ModelBuilder,
                properties.Where(property =>
                    !entityTypeBuilder.Metadata.GetForeignKeys().SelectMany(fk => fk.Properties).Contains(property)),
                ConfigurationSource.Convention);
            foreach (var propertyBuilder in propertyBuilders)
            {
                propertyBuilder.UseValueGenerator(true, ConfigurationSource.Convention);
            }
        }

        public virtual Property FindValueGeneratedOnAddProperty(
            [NotNull] IReadOnlyList<Property> properties, [NotNull] EntityType entityType)
        {
            Check.NotNull(properties, nameof(properties));
            Check.NotNull(entityType, nameof(entityType));

            if (entityType.FindPrimaryKey(properties) != null)
            {
                var property = FindValueGeneratedOnAddProperty(properties);
                if (!property.IsForeignKey(entityType))
                {
                    return property;
                }
            }

            return null;
        }

        private Property FindValueGeneratedOnAddProperty(IReadOnlyList<Property> properties)
        {
            if (properties.Count == 1)
            {
                var property = properties.First();
                var propertyType = ((IProperty)property).ClrType.UnwrapNullableType();
                if (propertyType.IsInteger()
                    || propertyType == typeof(Guid))
                {
                    return property;
                }
            }

            return null;
        }

        private void SetIdentity(InternalEntityTypeBuilder entityTypeBuilder, Property property)
            => entityTypeBuilder.Property(
                property.Name,
                ((IProperty)property).ClrType,
                ConfigurationSource.Convention)
                ?.ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Convention);
    }
}
