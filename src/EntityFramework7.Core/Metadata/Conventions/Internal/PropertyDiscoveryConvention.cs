// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class PropertyDiscoveryConvention : IEntityTypeConvention
    {
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            var entityType = entityTypeBuilder.Metadata;

            // TODO: Honor [NotMapped]
            // Issue #107
            if (entityType.HasClrType)
            {
                var primitiveProperties = entityType.ClrType.GetRuntimeProperties().Where(IsCandidatePrimitiveProperty);
                foreach (var propertyInfo in primitiveProperties)
                {
                    entityTypeBuilder.Property(propertyInfo, ConfigurationSource.Convention);
                }
            }

            return entityTypeBuilder;
        }

        protected virtual bool IsCandidatePrimitiveProperty([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return propertyInfo.IsCandidateProperty() && propertyInfo.PropertyType.IsPrimitive();
        }
    }
}
