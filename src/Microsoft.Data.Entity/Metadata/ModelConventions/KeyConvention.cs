// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.ModelConventions
{
    public class KeyConvention : IModelConvention
    {
        private const string KeySuffix = "Id";

        public virtual void Apply([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            var keyProperties = DiscoverKeyProperties(entityType).ToArray();
            if (keyProperties.Length != 0)
            {
                foreach (var property in keyProperties)
                {
                    ConfigureKeyProperty(property);
                }

                entityType.SetKey(keyProperties);
            }
        }

        protected virtual IEnumerable<Property> DiscoverKeyProperties([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            // TODO: Honor [Key]
            var keyProperties = entityType.Properties
                .Where(p => string.Equals(p.Name, KeySuffix, StringComparison.OrdinalIgnoreCase));

            if (!keyProperties.Any())
            {
                keyProperties = entityType.Properties.Where(
                    p => string.Equals(p.Name, entityType.Name + KeySuffix, StringComparison.OrdinalIgnoreCase));
            }

            if (keyProperties.Count() > 1)
            {
                throw new InvalidOperationException(
                    Strings.FormatMultiplePropertiesMatchedAsKeys(keyProperties.First().Name, entityType.Name));
            }

            return keyProperties;
        }

        protected virtual void ConfigureKeyProperty([NotNull] Property property)
        {
            Check.NotNull(property, "property");

            if (property.PropertyType == typeof(Guid))
            {
                property.ValueGenerationStrategy = ValueGenerationStrategy.Client;
            }

            if (property.PropertyType == typeof(int))
            {
                property.ValueGenerationStrategy = ValueGenerationStrategy.StoreIdentity;
            }

            // TODO: Nullable, Sequence
        }
    }
}
