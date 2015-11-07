// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class KeyConvention : IKeyConvention, IPrimaryKeyConvention, IForeignKeyConvention, IForeignKeyRemovedConvention, IModelConvention
    {
        public virtual InternalKeyBuilder Apply(InternalKeyBuilder keyBuilder)
        {
            var entityTypeBuilder = keyBuilder.ModelBuilder.Entity(
                keyBuilder.Metadata.DeclaringEntityType.Name, ConfigurationSource.Convention);

            SetValueGeneration(entityTypeBuilder, keyBuilder.Metadata.Properties);

            return keyBuilder;
        }

        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            foreach (var property in relationshipBuilder.Metadata.Properties)
            {
                var propertyBuilder = relationshipBuilder.ModelBuilder
                    .Entity(property.DeclaringEntityType.Name, ConfigurationSource.Convention)
                    .Property(property.Name, ConfigurationSource.Convention);

                propertyBuilder.RequiresValueGenerator(false, ConfigurationSource.Convention);
                propertyBuilder.ValueGenerated(ValueGenerated.Never, ConfigurationSource.Convention);
            }

            return relationshipBuilder;
        }

        public virtual void Apply(InternalEntityTypeBuilder entityTypeBuilder, ForeignKey foreignKey)
        {
            var properties = foreignKey.Properties;
            SetValueGeneration(entityTypeBuilder, properties.Where(property => property.IsKey()));

            var valueGeneratedOnAddProperty = FindValueGeneratedOnAddProperty(properties);
            if (valueGeneratedOnAddProperty != null
                && entityTypeBuilder.Metadata.FindPrimaryKey(properties) != null)
            {
                SetIdentity(entityTypeBuilder, valueGeneratedOnAddProperty);
            }
        }

        public virtual bool Apply(InternalKeyBuilder keyBuilder, Key previousPrimaryKey)
        {
            var entityTypeBuilder = keyBuilder.ModelBuilder.Entity(
                keyBuilder.Metadata.DeclaringEntityType.Name, ConfigurationSource.Convention);

            if (previousPrimaryKey != null)
            {
                foreach (var property in previousPrimaryKey.Properties)
                {
                    if (entityTypeBuilder.Metadata.FindProperty(property.Name) != null)
                    {
                        entityTypeBuilder.Property(property.Name, ConfigurationSource.Convention)
                            ?.ValueGenerated(ValueGenerated.Never, ConfigurationSource.Convention);
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

        private void SetValueGeneration(InternalEntityTypeBuilder entityTypeBuilder, IEnumerable<Property> properties)
        {
            var propertyBuilders = InternalEntityTypeBuilder.GetPropertyBuilders(
                entityTypeBuilder.ModelBuilder,
                properties.Where(property =>
                    !entityTypeBuilder.Metadata.GetForeignKeys().SelectMany(fk => fk.Properties).Contains(property)
                    && ((IProperty)property).ValueGenerated == ValueGenerated.OnAdd),
                ConfigurationSource.Convention);
            foreach (var propertyBuilder in propertyBuilders)
            {
                propertyBuilder?.RequiresValueGenerator(true, ConfigurationSource.Convention);
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
                if (property != null
                    && !property.IsForeignKey(entityType))
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
        {
            var propertyBuilder = entityTypeBuilder.Property(
                property.Name,
                ((IProperty)property).ClrType,
                ConfigurationSource.Convention);

            propertyBuilder?.ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Convention);
            propertyBuilder?.RequiresValueGenerator(true, ConfigurationSource.Convention);
        }

        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                var entityTypeBuilder = modelBuilder.Entity(entityType.Name, ConfigurationSource.Convention);
                foreach (var key in entityType.GetDeclaredKeys())
                {
                    if (key.Properties.Any(p => ((IProperty)p).IsShadowProperty && entityTypeBuilder.CanRemoveProperty(p, ConfigurationSource.Convention)))
                    {
                        string message;
                        var referencingFk = key.FindReferencingForeignKeys().FirstOrDefault();
                        if (referencingFk != null)
                        {
                            message = CoreStrings.ReferencedShadowKey(
                                Property.Format(key.Properties),
                                entityType.Name,
                                Property.Format(key.Properties),
                                Property.Format(referencingFk.Properties),
                                referencingFk.DeclaringEntityType.Name);
                        }
                        else
                        {
                            message = CoreStrings.ShadowKey(
                                Property.Format(key.Properties),
                                entityType.Name,
                                Property.Format(key.Properties));
                        }

                        throw new InvalidOperationException(message);
                    }
                }
            }

            return modelBuilder;
        }
    }
}
