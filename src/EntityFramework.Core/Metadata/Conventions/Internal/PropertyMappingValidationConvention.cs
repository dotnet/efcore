// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class PropertyMappingValidationConvention : IModelConvention
    {
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            foreach (var entityType in modelBuilder.Metadata.EntityTypes)
            {
                var unmappedProperty = entityType.Properties.FirstOrDefault(p => !IsMappedPrimitiveProperty(p));
                if (unmappedProperty != null)
                {
                    throw new InvalidOperationException(CoreStrings.PropertyNotMapped(unmappedProperty.Name, entityType.Name));
                }

                if (entityType.HasClrType)
                {
                    var clrProperties = new HashSet<string>();
                    clrProperties.UnionWith(entityType.ClrType.GetRuntimeProperties()
                        .Where(pi => pi.IsCandidateProperty())
                        .Select(pi => pi.Name));

                    clrProperties.ExceptWith(entityType.Properties.Select(p => p.Name));

                    clrProperties.ExceptWith(entityType.Navigations.Select(p => p.Name));

                    var entityTypeBuilder = modelBuilder.Entity(entityType.ClrType, ConfigurationSource.Convention);

                    clrProperties.RemoveWhere(p => entityTypeBuilder.IsIgnored(p, ConfigurationSource.Convention));

                    if (clrProperties.Count > 0)
                    {
                        foreach (var clrProperty in clrProperties)
                        {
                            var actualProperty = entityType.ClrType.GetRuntimeProperty(clrProperty);
                            var targetType = FindCandidateNavigationPropertyType(actualProperty);
                            if (targetType != null)
                            {
                                if (!modelBuilder.IsIgnored(targetType.FullName, ConfigurationSource.Convention))
                                {
                                    throw new InvalidOperationException(CoreStrings.NavigationNotAdded(actualProperty.Name, entityType.Name));
                                }
                            }
                            else
                            {
                                throw new InvalidOperationException(CoreStrings.PropertyNotAdded(actualProperty.Name, entityType.Name));
                            }
                        }
                    }
                }
            }

            return modelBuilder;
        }

        public virtual bool IsMappedPrimitiveProperty([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return property.ClrType.IsPrimitive();
        }

        public virtual Type FindCandidateNavigationPropertyType([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return propertyInfo.FindCandidateNavigationPropertyType(clrType => clrType.IsPrimitive());
        }
    }
}
