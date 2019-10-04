// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class OwnedEntityTypeAttributeConvention : EntityTypeAttributeConvention<OwnedAttribute>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder, OwnedAttribute attribute)
        {
            if (entityTypeBuilder.Metadata.HasClrType())
            {
                entityTypeBuilder.ModelBuilder.Owned(entityTypeBuilder.Metadata.ClrType, ConfigurationSource.DataAnnotation);
            }
            else
            {
                entityTypeBuilder.ModelBuilder.Owned(entityTypeBuilder.Metadata.Name, ConfigurationSource.DataAnnotation);
            }

            return entityTypeBuilder.Metadata.Builder;
        }
    }
}
