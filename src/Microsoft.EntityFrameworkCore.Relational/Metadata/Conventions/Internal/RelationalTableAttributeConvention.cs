// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class RelationalTableAttributeConvention : EntityTypeAttributeConvention<TableAttribute>
    {
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
