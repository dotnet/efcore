// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class DiscriminatorConvention : IBaseTypeConvention
    {
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
        {
            if (oldBaseType != null
                && oldBaseType.BaseType == null
                && !oldBaseType.GetDerivedTypes().Any())
            {
                var oldBaseTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(oldBaseType.Name, ConfigurationSource.Convention);
                oldBaseTypeBuilder?.Relational(ConfigurationSource.Convention)
                    .Discriminator(propertyInfo: null);
            }

            var entityType = entityTypeBuilder.Metadata;
            var derivedEntityTypes = entityType.GetDerivedTypes().ToList();

            DiscriminatorBuilder discriminator;
            if (entityType.BaseType == null)
            {
                if (!derivedEntityTypes.Any())
                {
                    entityTypeBuilder.Relational(ConfigurationSource.Convention)
                        .Discriminator(propertyInfo: null);
                    return true;
                }

                discriminator = entityTypeBuilder.Relational(ConfigurationSource.Convention)
                    .Discriminator(typeof(string));
            }
            else
            {
                if (entityTypeBuilder.Relational(ConfigurationSource.Convention).Discriminator(propertyInfo: null) == null)
                {
                    // TODO: log warning that the current discriminator couldn't be removed
                    return true;
                }

                var rootTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(entityType.RootType().Name, ConfigurationSource.Convention);
                discriminator = rootTypeBuilder?.Relational(ConfigurationSource.Convention)
                    .Discriminator(typeof(string));

                if (entityType.BaseType.BaseType == null)
                {
                    discriminator?.HasValue(entityType.BaseType.Name, entityType.BaseType.DisplayName());
                }
            }

            discriminator?.HasValue(entityTypeBuilder.Metadata.Name, entityTypeBuilder.Metadata.DisplayName());

            return true;
        }
    }
}
