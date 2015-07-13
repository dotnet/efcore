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
    public class KeyDiscoveryConvention : IEntityTypeConvention
    {
        private const string KeySuffix = "Id";

        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            var entityType = entityTypeBuilder.Metadata;

            var keyProperties = DiscoverKeyProperties(entityType);
            if (keyProperties.Count != 0)
            {
                entityTypeBuilder.PrimaryKey(keyProperties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);
            }

            return entityTypeBuilder;
        }

        public virtual IReadOnlyList<Property> DiscoverKeyProperties([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            // TODO: Honor [Key]
            // Issue #213
            var keyProperties = entityType.Properties
                .Where(p => string.Equals(p.Name, KeySuffix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (keyProperties.Count == 0)
            {
                keyProperties = entityType.Properties.Where(
                    p => string.Equals(p.Name, entityType.DisplayName() + KeySuffix, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (keyProperties.Count > 1)
            {
                //TODO - add in logging using resource Strings.MultiplePropertiesMatchedAsKeys()
                return new Property[0];
            }

            return keyProperties;
        }
    }
}
