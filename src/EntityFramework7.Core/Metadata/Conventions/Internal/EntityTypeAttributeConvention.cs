// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public abstract class EntityTypeAttributeConvention<TAttribute> : IEntityTypeConvention
        where TAttribute : Attribute
    {
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            var attributes = entityTypeBuilder.Metadata.ClrType?.GetTypeInfo().GetCustomAttributes<TAttribute>(true);
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

        public abstract InternalEntityTypeBuilder Apply([NotNull] InternalEntityTypeBuilder entityTypeBuilder, [NotNull] TAttribute attribute);
    }
}
