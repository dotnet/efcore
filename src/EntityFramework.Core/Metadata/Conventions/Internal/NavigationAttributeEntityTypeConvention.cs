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
    public abstract class NavigationAttributeEntityTypeConvention<TAttribute> : IEntityTypeConvention
        where TAttribute : Attribute
    {
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            var entityType = entityTypeBuilder.Metadata;

            if (entityType.HasClrType)
            {
                foreach (var navigationPropertyInfo in entityType.ClrType.GetRuntimeProperties().OrderBy(p => p.Name))
                {
                    var entityClrType = navigationPropertyInfo.FindCandidateNavigationPropertyType();
                    if (entityClrType == null)
                    {
                        continue;
                    }

                    var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(true);
                    if (attributes != null)
                    {
                        foreach (var attribute in attributes)
                        {
                            var returnValue = Apply(entityTypeBuilder, navigationPropertyInfo, attribute);
                            if (returnValue == null)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return entityTypeBuilder;
        }

        public abstract InternalEntityTypeBuilder Apply([NotNull] InternalEntityTypeBuilder entityTypeBuilder, [NotNull] PropertyInfo navigationPropertyInfo, [NotNull] TAttribute attribute);
    }
}
