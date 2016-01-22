// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class NotMappedMemberAttributeConvention : IEntityTypeConvention
    {
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
