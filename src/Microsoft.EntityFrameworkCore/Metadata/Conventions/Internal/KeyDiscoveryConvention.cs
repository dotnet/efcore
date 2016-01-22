// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class KeyDiscoveryConvention : IEntityTypeConvention, IPropertyConvention, IBaseTypeConvention
    {
        private const string KeySuffix = "Id";

        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            var entityType = entityTypeBuilder.Metadata;

            if (entityType.BaseType == null
                && entityType.FindPrimaryKey() == null)
            {
                var candidateProperties = entityType.GetProperties().Where(p =>
                    !p.IsShadowProperty
                    || !ConfigurationSource.Convention.Overrides(p.GetConfigurationSource())).ToList();
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

            Apply(propertyBuilder.Metadata.DeclaringEntityType.Builder);

            return propertyBuilder;
        }
    }
}
