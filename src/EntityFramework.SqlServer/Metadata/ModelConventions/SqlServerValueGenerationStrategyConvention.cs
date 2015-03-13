// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata.ModelConventions
{
    public class SqlServerValueGenerationStrategyConvention : IKeyConvention, IForeignKeyRemovedConvention, IRelationshipConvention
    {
        public virtual InternalKeyBuilder Apply(InternalKeyBuilder keyBuilder)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));

            var key = keyBuilder.Metadata;

            ConfigureValueGenerationStrategy(
                keyBuilder.ModelBuilder.Entity(key.EntityType.Name, ConfigurationSource.Convention),
                key.Properties,
                true);

            return keyBuilder;
        }

        protected virtual void ConfigureValueGenerationStrategy([NotNull] InternalEntityBuilder entityBuilder, [NotNull] IReadOnlyList<Property> properties, bool generateValue)
        {
            Check.NotNull(entityBuilder, nameof(entityBuilder));
            Check.NotNull(properties, nameof(properties));

            if (entityBuilder.Metadata.TryGetPrimaryKey(properties) != null
                && properties.Count == 1
                && properties.First().PropertyType.IsInteger()
                && properties.First().GenerateValueOnAdd == generateValue)
            {
                entityBuilder.Property(properties.First().PropertyType, properties.First().Name, ConfigurationSource.Convention)
                    .Annotation(SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration,
                        generateValue ? SqlServerValueGenerationStrategy.Default.ToString() : null,
                        ConfigurationSource.Convention);
            }
        }

        public virtual void Apply(InternalEntityBuilder entityBuilder, ForeignKey foreignKey)
        {
            Check.NotNull(entityBuilder, nameof(entityBuilder));
            Check.NotNull(foreignKey, nameof(foreignKey));

            var properties = foreignKey.Properties;

            if (properties.Any(e => e.GenerateValueOnAdd == true))
            {
                ConfigureValueGenerationStrategy(entityBuilder, properties, true);
            }
        }

        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            ConfigureValueGenerationStrategy(
                relationshipBuilder.ModelBuilder.Entity(relationshipBuilder.Metadata.EntityType.Name, ConfigurationSource.Convention),
                relationshipBuilder.Metadata.Properties,
                false);

            return relationshipBuilder;
        }
    }
}
