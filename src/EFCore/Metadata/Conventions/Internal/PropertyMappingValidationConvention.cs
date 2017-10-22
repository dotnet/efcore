// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Converters;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class PropertyMappingValidationConvention : IModelBuiltConvention
    {
        private readonly ITypeMapper _typeMapper;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public PropertyMappingValidationConvention([NotNull] ITypeMapper typeMapper)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));

            _typeMapper = typeMapper;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                var unmappedProperty = entityType.GetProperties().FirstOrDefault(p => !IsMappedPrimitiveProperty(p));

                if (unmappedProperty != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.PropertyNotMapped(
                            entityType.DisplayName(), unmappedProperty.Name, unmappedProperty.ClrType.ShortDisplayName()));
                }

                if (entityType.HasClrType())
                {
                    var clrProperties = new HashSet<string>();

                    clrProperties.UnionWith(
                        entityType.ClrType.GetRuntimeProperties()
                            .Where(pi => pi.IsCandidateProperty())
                            .Select(pi => pi.Name));

                    clrProperties.ExceptWith(entityType.GetProperties().Select(p => p.Name));
                    clrProperties.ExceptWith(entityType.GetNavigations().Select(p => p.Name));
                    clrProperties.RemoveWhere(p => entityType.Builder.IsIgnored(p, ConfigurationSource.Convention));

                    if (clrProperties.Count > 0)
                    {
                        foreach (var clrProperty in clrProperties)
                        {
                            var actualProperty = entityType.ClrType.GetRuntimeProperties().First(p => p.Name == clrProperty);
                            var propertyType = actualProperty.PropertyType;
                            var targetSequenceType = propertyType.TryGetSequenceType();

                            if (modelBuilder.IsIgnored(propertyType.DisplayName(), ConfigurationSource.Convention)
                                || targetSequenceType != null
                                && modelBuilder.IsIgnored(targetSequenceType.DisplayName(), ConfigurationSource.Convention))
                            {
                                continue;
                            }

                            var targetType = FindCandidateNavigationPropertyType(actualProperty);

                            var targetEntityType
                                = targetType == null
                                    ? null
                                    : modelBuilder.Metadata.FindEntityType(targetType);

                            var isDependentEntityType
                                = targetType != null
                                  && modelBuilder.Metadata.HasEntityTypeWithDefiningNavigation(targetType);

                            if (targetType != null
                                && (targetEntityType != null
                                    || isDependentEntityType))
                            {
                                if ((!isDependentEntityType
                                     || !targetType.GetTypeInfo().Equals(entityType.ClrType.GetTypeInfo()))
                                    && entityType.GetDerivedTypes().All(dt => dt.FindDeclaredNavigation(actualProperty.Name) == null)
                                    && !entityType.IsInDefinitionPath(targetType))
                                {
                                    throw new InvalidOperationException(
                                        CoreStrings.NavigationNotAdded(
                                            entityType.DisplayName(), actualProperty.Name, propertyType.ShortDisplayName()));
                                }
                            }
                            else if (targetSequenceType == null && propertyType.GetTypeInfo().IsInterface
                                     || targetSequenceType != null && targetSequenceType.GetTypeInfo().IsInterface)
                            {
                                throw new InvalidOperationException(
                                    CoreStrings.InterfacePropertyNotAdded(
                                        entityType.DisplayName(), actualProperty.Name, propertyType.ShortDisplayName()));
                            }
                            else
                            {
                                throw new InvalidOperationException(
                                    CoreStrings.PropertyNotAdded(
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
        public virtual bool IsMappedPrimitiveProperty([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return _typeMapper.IsTypeMapped(
                ((ValueConverter)property[CoreAnnotationNames.ValueConverter])?.StoreType
                ?? property.ClrType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Type FindCandidateNavigationPropertyType([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return propertyInfo.FindCandidateNavigationPropertyType(_typeMapper.IsTypeMapped);
        }
    }
}
