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
    public class KeyDiscoveryConvention : IEntityTypeConvention, IPropertyConvention, IBaseTypeConvention
    {
        private const string KeySuffix = "Id";

        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            var entityType = entityTypeBuilder.Metadata;

            if (entityType.BaseType == null)
            {
                var candidateProperties = entityType.Properties.Where(p => !((IProperty)p).IsShadowProperty || !entityTypeBuilder.CanRemoveProperty(p, ConfigurationSource.Convention)).ToList();
                var keyProperties = DiscoverKeyProperties(entityType, candidateProperties);
                if (keyProperties.Count != 0)
                {
                    entityTypeBuilder.PrimaryKey(keyProperties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);
                }
            }

            return entityTypeBuilder;
        }

        public virtual IReadOnlyList<Property> DiscoverKeyProperties([NotNull] EntityType entityType, [NotNull] IReadOnlyList<Property> candidateProperties)
        {
            Check.NotNull(entityType, nameof(entityType));

            var keyProperties = candidateProperties.Where(p => string.Equals(p.Name, KeySuffix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (keyProperties.Count == 0)
            {
                keyProperties = candidateProperties.Where(
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

        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
            => Apply(entityTypeBuilder) != null;

        public virtual InternalPropertyBuilder Apply(InternalPropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            var entityTypeBuilder = propertyBuilder.ModelBuilder.Entity(propertyBuilder.Metadata.DeclaringEntityType.Name, ConfigurationSource.Convention);
            Apply(entityTypeBuilder);

            return propertyBuilder;
        }
    }
}
