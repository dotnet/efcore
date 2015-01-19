// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.ModelConventions
{
    public class KeyConvention : IEntityTypeConvention
    {
        private const string KeySuffix = "Id";

        public virtual InternalEntityBuilder Apply(InternalEntityBuilder entityBuilder)
        {
            Check.NotNull(entityBuilder, "entityBuilder");
            var entityType = entityBuilder.Metadata;

            var keyProperties = DiscoverKeyProperties(entityType);
            if (keyProperties.Count != 0
                && entityBuilder.PrimaryKey(keyProperties.Select(p => p.Name).ToList(), ConfigurationSource.Convention) != null)
            {
                foreach (var property in keyProperties)
                {
                    ConfigureKeyProperty(entityBuilder.Property(property.PropertyType, property.Name, ConfigurationSource.Convention));
                }
            }

            return entityBuilder;
        }

        protected virtual IReadOnlyList<Property> DiscoverKeyProperties([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            // TODO: Honor [Key]
            // Issue #213
            var keyProperties = entityType.Properties
                .Where(p => string.Equals(p.Name, KeySuffix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (keyProperties.Count == 0)
            {
                keyProperties = entityType.Properties.Where(
                    p => string.Equals(p.Name, entityType.SimpleName + KeySuffix, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (keyProperties.Count > 1)
            {
                throw new InvalidOperationException(
                    Strings.MultiplePropertiesMatchedAsKeys(keyProperties.First().Name, entityType.Name));
            }

            return keyProperties;
        }

        protected virtual void ConfigureKeyProperty([NotNull] InternalPropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");

            propertyBuilder.GenerateValueOnAdd(true, ConfigurationSource.Convention);

            // TODO: Nullable, Sequence
            // Issue #213
        }

        private static bool IsCommonInteger(Type type)
        {
            type = type.UnwrapNullableType();

            return type == typeof(int)
                   || type == typeof(long)
                   || type == typeof(short)
                   || type == typeof(byte);
        }
    }
}
