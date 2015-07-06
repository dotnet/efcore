// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class RelationalColumnAttributeConvention : PropertyAttributeConvention<ColumnAttribute>
    {
        public override InternalPropertyBuilder Apply(InternalPropertyBuilder propertyBuilder, ColumnAttribute attribute)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NotNull(attribute, nameof(attribute));

            if (!string.IsNullOrWhiteSpace(attribute.Name))
            {
                propertyBuilder.Annotation(RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnName, attribute.Name, ConfigurationSource.DataAnnotation);
            }

            if (attribute.Order != -1)
            {
                propertyBuilder.Annotation(RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnOrder, attribute.Order, ConfigurationSource.DataAnnotation);
            }

            if (!string.IsNullOrWhiteSpace(attribute.TypeName))
            {
                propertyBuilder.Annotation(RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnType, attribute.TypeName, ConfigurationSource.DataAnnotation);
            }

            return propertyBuilder;
        }
    }
}
