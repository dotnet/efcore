// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.ModelConventions
{
    public class KeyConvention : IKeyConvention, IForeignKeyRemovedConvention
    {
        public virtual InternalKeyBuilder Apply(InternalKeyBuilder keyBuilder)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));

            ConfigureKeyProperties(keyBuilder.ModelBuilder.Entity(keyBuilder.Metadata.EntityType.Name, ConfigurationSource.Convention),
                keyBuilder.Metadata.Properties);

            return keyBuilder;
        }

        protected virtual void ConfigureKeyProperties([NotNull] InternalEntityBuilder entityBuilder, [NotNull] IReadOnlyList<Property> properties)
        {
            Check.NotNull(entityBuilder, nameof(entityBuilder));
            Check.NotNull(properties, nameof(properties));
            foreach (var property in properties.Where(property => !entityBuilder.Metadata.ForeignKeys.SelectMany(fk => fk.Properties).Contains(property)))
            {
                entityBuilder.Property(property.ClrType, property.Name, ConfigurationSource.Convention)
                    ?.GenerateValueOnAdd(true, ConfigurationSource.Convention);
            }
            // TODO: Nullable, Sequence
            // Issue #213
        }

        public virtual void Apply(InternalEntityBuilder entityBuilder, ForeignKey foreignKey)
        {
            Check.NotNull(entityBuilder, nameof(entityBuilder));
            Check.NotNull(foreignKey, nameof(foreignKey));

            ConfigureKeyProperties(entityBuilder, foreignKey.Properties);
        }
    }
}
