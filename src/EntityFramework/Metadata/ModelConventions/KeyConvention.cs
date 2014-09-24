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

        public virtual void Apply(EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            var keyProperties = DiscoverKeyProperties(entityType).ToArray();
            if (keyProperties.Length != 0)
            {
                foreach (var property in keyProperties)
                {
                    ConfigureKeyProperty(property);
                }

                entityType.GetOrSetPrimaryKey(keyProperties);
            }
        }

        protected virtual IEnumerable<Property> DiscoverKeyProperties([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            // TODO: Honor [Key]
            // Issue #213
            var keyProperties = entityType.Properties
                .Where(p => string.Equals(p.Name, KeySuffix, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (keyProperties.Length == 0)
            {
                keyProperties = entityType.Properties.Where(
                    p => string.Equals(p.Name, entityType.SimpleName + KeySuffix, StringComparison.OrdinalIgnoreCase)).ToArray();
            }

            if (keyProperties.Length > 1)
            {
                throw new InvalidOperationException(
                    Strings.FormatMultiplePropertiesMatchedAsKeys(keyProperties.First().Name, entityType.Name));
            }

            return keyProperties;
        }

        protected virtual void ConfigureKeyProperty([NotNull] Property property)
        {
            Check.NotNull(property, "property");

            if (property.PropertyType == typeof(Guid)
                || property.PropertyType.IsInteger())
            {
                property.ValueGeneration = ValueGeneration.OnAdd;
            }

            // TODO: Nullable, Sequence
            // Issue #213
        }
    }
}
