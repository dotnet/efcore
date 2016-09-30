// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RelationalColumnAttributeConvention : PropertyAttributeConvention<ColumnAttribute>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override InternalPropertyBuilder Apply(
            InternalPropertyBuilder propertyBuilder, ColumnAttribute attribute, MemberInfo clrMember)
        {
            if (!string.IsNullOrWhiteSpace(attribute.Name))
            {
                propertyBuilder.Relational(ConfigurationSource.DataAnnotation).HasColumnName(attribute.Name);
            }

            if (!string.IsNullOrWhiteSpace(attribute.TypeName))
            {
                propertyBuilder.Relational(ConfigurationSource.DataAnnotation).HasColumnType(attribute.TypeName);
            }

            return propertyBuilder;
        }
    }
}
