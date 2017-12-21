// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DiscriminatorConvention : IBaseTypeChangedConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
        {
            if (oldBaseType != null
                && oldBaseType.BaseType == null
                && !oldBaseType.GetDerivedTypes().Any())
            {
                var oldBaseTypeBuilder = oldBaseType.Builder;
                oldBaseTypeBuilder?.Relational(ConfigurationSource.Convention).HasDiscriminator(propertyInfo: null);
            }

            var entityType = entityTypeBuilder.Metadata;
            var derivedEntityTypes = entityType.GetDerivedTypes().ToList();

            DiscriminatorBuilder discriminator;
            if (entityType.BaseType == null)
            {
                if (!derivedEntityTypes.Any())
                {
                    entityTypeBuilder.Relational(ConfigurationSource.Convention).HasDiscriminator(propertyInfo: null);
                    return true;
                }

                discriminator = entityTypeBuilder.Relational(ConfigurationSource.Convention)
                    .HasDiscriminator(typeof(string));
            }
            else
            {
                if (entityTypeBuilder.Relational(ConfigurationSource.Convention).HasDiscriminator(propertyInfo: null) == null)
                {
                    return true;
                }

                var rootTypeBuilder = entityType.RootType().Builder;
                discriminator = rootTypeBuilder?.Relational(ConfigurationSource.Convention).HasDiscriminator(typeof(string));

                if (entityType.BaseType.BaseType == null)
                {
                    discriminator?.HasValue(entityType.BaseType.Name, entityType.BaseType.ShortName());
                }
            }

            if (discriminator != null)
            {
                discriminator.HasValue(entityTypeBuilder.Metadata.Name, entityTypeBuilder.Metadata.ShortName());
                SetDefaultDiscriminatorValues(derivedEntityTypes, discriminator);
            }

            return true;
        }

        private void SetDefaultDiscriminatorValues(IReadOnlyList<EntityType> entityTypes, DiscriminatorBuilder discriminator)
        {
            foreach (var entityType in entityTypes)
            {
                discriminator.HasValue(entityType.Name, entityType.ShortName());
            }
        }
    }
}
