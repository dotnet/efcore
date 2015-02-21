// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.ModelConventions
{
    public class PropertiesConvention : IEntityTypeConvention
    {
        public virtual InternalEntityBuilder Apply(InternalEntityBuilder entityBuilder)
        {
            Check.NotNull(entityBuilder, nameof(entityBuilder));
            var entityType = entityBuilder.Metadata;

            // TODO: Honor [NotMapped]
            // Issue #107
            if (entityType.HasClrType)
            {
                var primitiveProperties = entityType.Type.GetRuntimeProperties().Where(ConventionsPropertyInfoExtensions.IsCandidatePrimitiveProperty);
                foreach (var propertyInfo in primitiveProperties)
                {
                    entityBuilder.Property(propertyInfo, ConfigurationSource.Convention);
                }
            }

            return entityBuilder;
        }
    }
}
