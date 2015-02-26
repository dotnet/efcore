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
    public class SqlServerKeyConvention : IKeyConvention
    {
        private const string IdentityKeySuffix = "Id";
        public virtual InternalKeyBuilder Apply(InternalKeyBuilder keyBuilder)
        {
            Check.NotNull(keyBuilder, "keyBuilder");

            var key = keyBuilder.Metadata;
            var properties = key.Properties;

            if (key == key.EntityType.TryGetPrimaryKey())
            {
                var identityProperty = DiscoverIdentityProperty(properties);
                if (identityProperty != null)
                {
                    var entityBuilder = keyBuilder.ModelBuilder.Entity(identityProperty.EntityType.Name, ConfigurationSource.Convention);
                    ConfigureIdentityProperty(entityBuilder.Property(identityProperty.PropertyType, identityProperty.Name, ConfigurationSource.Convention));
                }
            }
            return keyBuilder;
        }

        protected virtual IProperty DiscoverIdentityProperty(IReadOnlyList<Property> properties)
        {
            var identityProperty = properties
                .Where(p => p.PropertyType.IsInteger())
                .FirstOrDefault(p => string.Equals(p.Name, IdentityKeySuffix, StringComparison.OrdinalIgnoreCase));

            if (identityProperty == null)
            {
                var entityType = properties.First().EntityType;

                identityProperty = properties
                    .Where(p => p.PropertyType.IsInteger())
                    .FirstOrDefault(p => string.Equals(p.Name, entityType.SimpleName + IdentityKeySuffix, StringComparison.OrdinalIgnoreCase));
            }

            return identityProperty ?? properties.FirstOrDefault(p => p.PropertyType.IsInteger());
        }

        protected virtual void ConfigureIdentityProperty([NotNull] InternalPropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");

            propertyBuilder.Annotation(SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration, SqlServerValueGenerationStrategy.Identity.ToString(), ConfigurationSource.Convention);
        }
    }
}
