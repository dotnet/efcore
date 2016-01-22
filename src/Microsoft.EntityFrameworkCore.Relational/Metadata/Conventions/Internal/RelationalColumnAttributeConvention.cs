// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class RelationalColumnAttributeConvention : PropertyAttributeConvention<ColumnAttribute>
    {
        public override InternalPropertyBuilder Apply(InternalPropertyBuilder propertyBuilder, ColumnAttribute attribute, PropertyInfo clrProperty)
        {
            if (!string.IsNullOrWhiteSpace(attribute.Name))
            {
                propertyBuilder.Relational(ConfigurationSource.DataAnnotation).ColumnName(attribute.Name);
            }

            if (!string.IsNullOrWhiteSpace(attribute.TypeName))
            {
                propertyBuilder.Relational(ConfigurationSource.DataAnnotation).ColumnType(attribute.TypeName);
            }

            return propertyBuilder;
        }
    }
}
