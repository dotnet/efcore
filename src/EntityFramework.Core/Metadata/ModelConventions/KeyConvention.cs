// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.ModelConventions
{
    public class KeyConvention : IKeyConvention, IForeignKeyRemovedConvention
    {
        public virtual InternalKeyBuilder Apply(InternalKeyBuilder keyBuilder)
        {
            Check.NotNull(keyBuilder, "keyBuilder");

            var entityBuilder = keyBuilder.ModelBuilder.Entity(keyBuilder.Metadata.EntityType.Name, ConfigurationSource.Convention);
            var properties = keyBuilder.Metadata.Properties;

            if (entityBuilder.Metadata.TryGetForeignKey(properties) == null)
            {
                foreach (var property in properties)
                {
                    ConfigureKeyProperty(entityBuilder.Property(property.PropertyType, property.Name, ConfigurationSource.Convention));
                }
            }
            return keyBuilder;
        }

        protected virtual void ConfigureKeyProperty([NotNull] InternalPropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            propertyBuilder.GenerateValueOnAdd(true, ConfigurationSource.Convention);

            // TODO: Nullable, Sequence
            // Issue #213
        }

        public virtual void Apply(InternalEntityBuilder entityBuilder, ForeignKey foreignKey)
        {
            Check.NotNull(entityBuilder, "entityBuilder");
            Check.NotNull(foreignKey, "foreignKey");

            var properties = foreignKey.Properties;

            if (entityBuilder.Metadata.TryGetForeignKey(properties) == null)
            {
                foreach (var property in properties)
                {
                    ConfigureKeyProperty(entityBuilder.Property(property.PropertyType, property.Name, ConfigurationSource.Convention));
                }
            }
        }
    }
}
