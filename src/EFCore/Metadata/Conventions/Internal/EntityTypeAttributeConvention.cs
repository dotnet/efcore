// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract class EntityTypeAttributeConvention<TAttribute> : IEntityTypeAddedConvention
        where TAttribute : Attribute
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            var type = entityTypeBuilder.Metadata.ClrType;
            if (type == null
                || !Attribute.IsDefined(type, typeof(TAttribute), inherit: true))
            {
                return entityTypeBuilder;
            }

            var attributes = type.GetTypeInfo().GetCustomAttributes<TAttribute>(true);
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    entityTypeBuilder = Apply(entityTypeBuilder, attribute);
                    if (entityTypeBuilder == null)
                    {
                        break;
                    }
                }
            }

            return entityTypeBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public abstract InternalEntityTypeBuilder Apply([NotNull] InternalEntityTypeBuilder entityTypeBuilder, [NotNull] TAttribute attribute);
    }
}
