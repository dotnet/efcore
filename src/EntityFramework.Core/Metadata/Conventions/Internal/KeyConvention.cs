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
    public class KeyConvention : IKeyConvention, IForeignKeyRemovedConvention
    {
        public virtual InternalKeyBuilder Apply(InternalKeyBuilder keyBuilder)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));

            var entityTypeBuilder = keyBuilder.ModelBuilder.Entity(keyBuilder.Metadata.EntityType.Name, ConfigurationSource.Convention);
            var properties = keyBuilder.Metadata.Properties;

            SetValueGeneration(entityTypeBuilder, properties);
            SetIdentity(entityTypeBuilder, properties);

            return keyBuilder;
        }

        public virtual void Apply(InternalEntityTypeBuilder entityTypeBuilder, ForeignKey foreignKey)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(foreignKey, nameof(foreignKey));

            SetValueGeneration(entityTypeBuilder, foreignKey.Properties);
            SetIdentity(entityTypeBuilder, foreignKey.Properties);
        }

        protected virtual void SetValueGeneration(
            [NotNull] InternalEntityTypeBuilder entityTypeBuilder,
            [NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(properties, nameof(properties));

            foreach (var property in properties.Where(
                property => !entityTypeBuilder.Metadata.GetForeignKeys().SelectMany(fk => fk.Properties).Contains(property)))
            {
                entityTypeBuilder.Property(property.ClrType, property.Name, ConfigurationSource.Convention)
                    ?.GenerateValueOnAdd(true, ConfigurationSource.Convention);
            }
        }

        protected virtual void SetIdentity(
            [NotNull] InternalEntityTypeBuilder entityTypeBuilder,
            [NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(properties, nameof(properties));

            if (entityTypeBuilder.Metadata.FindPrimaryKey(properties) != null)
            {
                foreach (var property in entityTypeBuilder.Metadata.Properties)
                {
                    entityTypeBuilder.Property(property.ClrType, property.Name, ConfigurationSource.Convention)
                        ?.StoreGeneratedPattern(null, ConfigurationSource.Convention);
                }

                if (properties.Count == 1)
                {
                    var property = properties.First();

                    if (property.ClrType.IsInteger()
                        && entityTypeBuilder.Metadata.FindPrimaryKey(properties) != null
                        && !property.IsForeignKey())
                    {
                        entityTypeBuilder.Property(property.ClrType, property.Name, ConfigurationSource.Convention)
                            ?.StoreGeneratedPattern(StoreGeneratedPattern.Identity, ConfigurationSource.Convention);
                    }
                }
            }
        }
    }
}
