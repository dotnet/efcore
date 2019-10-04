// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class BaseTypeDiscoveryConvention : InheritanceDiscoveryConventionBase, IEntityTypeAddedConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var entityType = entityTypeBuilder.Metadata;
            var clrType = entityType.ClrType;
            if (clrType == null
                || entityType.HasDefiningNavigation()
                || entityType.FindDeclaredOwnership() != null
                || entityType.Model.ShouldBeOwnedType(entityType.Model.GetDisplayName(clrType)))
            {
                return entityTypeBuilder;
            }

            var baseEntityType = FindClosestBaseType(entityType);
            return baseEntityType?.HasDefiningNavigation() != false
                ? entityTypeBuilder
                : entityTypeBuilder.HasBaseType(baseEntityType, ConfigurationSource.Convention);
        }
    }
}
