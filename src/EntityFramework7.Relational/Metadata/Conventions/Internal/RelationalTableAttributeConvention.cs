// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class RelationalTableAttributeConvention : EntityTypeAttributeConvention<TableAttribute>
    {
        public override InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder, TableAttribute attribute)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(attribute, nameof(attribute));

            if (!string.IsNullOrWhiteSpace(attribute.Schema))
            {
                entityTypeBuilder.Annotation(RelationalAnnotationNames.Prefix + RelationalAnnotationNames.Schema, attribute.Schema, ConfigurationSource.DataAnnotation);
            }

            if (!string.IsNullOrWhiteSpace(attribute.Name))
            {
                entityTypeBuilder.Annotation(RelationalAnnotationNames.Prefix + RelationalAnnotationNames.TableName, attribute.Name, ConfigurationSource.DataAnnotation);
            }

            return entityTypeBuilder;
        }
    }
}
