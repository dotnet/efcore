// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class DerivedTypeDiscoveryConvention : InheritanceDiscoveryConventionBase, IEntityTypeConvention
    {
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var entityType = entityTypeBuilder.Metadata;
            var clrType = entityType.ClrType;
            if (clrType == null)
            {
                return entityTypeBuilder;
            }

            var directlyDerivedTypes = entityType.Model.GetEntityTypes().Where(t =>
                t.BaseType == entityType.BaseType
                && t.HasClrType()
                && FindClosestBaseType(t) == entityType);

            foreach (var directlyDerivedType in directlyDerivedTypes)
            {
                entityTypeBuilder.ModelBuilder.Entity(directlyDerivedType.ClrType, ConfigurationSource.Convention)
                    .HasBaseType(entityType, ConfigurationSource.Convention);
            }

            return entityTypeBuilder;
        }
    }
}
