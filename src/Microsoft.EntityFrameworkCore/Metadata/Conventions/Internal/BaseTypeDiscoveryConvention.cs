// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class BaseTypeDiscoveryConvention : InheritanceDiscoveryConventionBase, IEntityTypeConvention
    {
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var entityType = entityTypeBuilder.Metadata;
            var clrType = entityType.ClrType;
            if (clrType == null)
            {
                return entityTypeBuilder;
            }

            var baseEntityType = FindClosestBaseType(entityType);
            return entityTypeBuilder.HasBaseType(baseEntityType, ConfigurationSource.Convention);
        }
    }
}
