// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class PropertyMappingValidationConvention : IModelConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                var unmappedProperty = entityType.GetProperties().FirstOrDefault(p => !IsMappedPrimitiveProperty(p.ClrType));
                if (unmappedProperty != null)
                {
                    throw new InvalidOperationException(CoreStrings.PropertyNotMapped(
                        entityType.DisplayName(), unmappedProperty.Name, unmappedProperty.ClrType.ShortDisplayName()));
                }

                if (entityType.HasClrType())
                {
                    var clrProperties = new HashSet<string>();
                    clrProperties.UnionWith(entityType.ClrType.GetRuntimeProperties()
                        .Where(pi => pi.IsCandidateProperty())
                        .Select(pi => pi.Name));

                    clrProperties.ExceptWith(entityType.GetProperties().Select(p => p.Name));

                    clrProperties.ExceptWith(entityType.GetNavigations().Select(p => p.Name));

                    var entityTypeBuilder = modelBuilder.Entity(entityType.ClrType, ConfigurationSource.Convention);

                    clrProperties.RemoveWhere(p => entityTypeBuilder.IsIgnored(p, ConfigurationSource.Convention));

                    if (clrProperties.Count > 0)
                    {
                        foreach (var clrProperty in clrProperties)
                        {
                            var actualProperty = entityType.ClrType.GetRuntimeProperties().First(p => p.Name == clrProperty);
                            var propertyType = actualProperty.PropertyType;
                            var targetSequenceType = propertyType.TryGetSequenceType();
                            var targetType = FindCandidateNavigationPropertyType(actualProperty);
                            if (targetType != null)
                            {
                                if (!modelBuilder.IsIgnored(targetType.DisplayName(), ConfigurationSource.Convention))
                                {
                                    throw new InvalidOperationException(CoreStrings.NavigationNotAdded(
                                        entityType.DisplayName(), actualProperty.Name, propertyType.ShortDisplayName()));
                                }
                            }
                            else if (propertyType.IsPrimitive())
                            {
                                throw new InvalidOperationException(CoreStrings.PropertyNotMapped(
                                    entityType.DisplayName(), actualProperty.Name, propertyType.ShortDisplayName()));
                            }
                            else if (propertyType.GetTypeInfo().IsInterface
                                     || (targetSequenceType != null && targetSequenceType.GetTypeInfo().IsInterface))
                            {
                                throw new InvalidOperationException(CoreStrings.InterfacePropertyNotAdded(
                                    entityType.DisplayName(), actualProperty.Name, propertyType.ShortDisplayName()));
                            }
                            else
                            {
                                throw new InvalidOperationException(CoreStrings.PropertyNotAdded(
                                    entityType.DisplayName(), actualProperty.Name, propertyType.ShortDisplayName()));
                            }
                        }
                    }
                }
            }

            return modelBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsMappedPrimitiveProperty([NotNull] Type clrType)
        {
            Check.NotNull(clrType, nameof(clrType));

            return clrType.IsPrimitive();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Type FindCandidateNavigationPropertyType([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return propertyInfo.FindCandidateNavigationPropertyType(clrType => clrType.IsPrimitive());
        }
    }
}
