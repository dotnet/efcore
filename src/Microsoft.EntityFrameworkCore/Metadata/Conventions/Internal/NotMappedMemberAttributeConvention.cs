// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class NotMappedMemberAttributeConvention : IEntityTypeConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            var properties = entityTypeBuilder.Metadata.ClrType?.GetRuntimeProperties();
            if (properties == null)
            {
                return entityTypeBuilder;
            }

            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes<NotMappedAttribute>(inherit: true);
                if (attributes.Any())
                {
                    entityTypeBuilder.Ignore(property.Name, ConfigurationSource.DataAnnotation);
                }
            }

            return entityTypeBuilder;
        }
    }
}
