// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RelationalTableAttributeConvention : EntityTypeAttributeConvention<TableAttribute>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder, TableAttribute attribute)
        {
            if (!string.IsNullOrWhiteSpace(attribute.Schema))
            {
                entityTypeBuilder.Relational(ConfigurationSource.DataAnnotation).ToTable(attribute.Name, attribute.Schema);
            }

            if (!string.IsNullOrWhiteSpace(attribute.Name))
            {
                entityTypeBuilder.Relational(ConfigurationSource.DataAnnotation).ToTable(attribute.Name);
            }

            return entityTypeBuilder;
        }
    }
}
