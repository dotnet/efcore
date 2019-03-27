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
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class PropertyMappingValidationConvention : IModelBuiltConvention
    {
        private readonly ITypeMappingSource _typeMappingSource;
        private readonly IMemberClassifier _memberClassifier;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public PropertyMappingValidationConvention(
            [NotNull] ITypeMappingSource typeMappingSource,
            [NotNull] IMemberClassifier memberClassifier)
        {
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));
            Check.NotNull(memberClassifier, nameof(memberClassifier));

            _typeMappingSource = typeMappingSource;
            _memberClassifier = memberClassifier;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                var unmappedProperty = entityType.GetProperties().FirstOrDefault(
                    p => (!ConfigurationSource.Convention.Overrides(p.GetConfigurationSource()) || !p.IsShadowProperty)
                         && !IsMappedPrimitiveProperty(p));

                if (unmappedProperty != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.PropertyNotMapped(
                            entityType.DisplayName(), unmappedProperty.Name, unmappedProperty.ClrType.ShortDisplayName()));
                }

                if (!entityType.HasClrType())
                {
                    continue;
                }

                var clrProperties = new HashSet<string>(StringComparer.Ordinal);

                clrProperties.UnionWith(
                    entityType.GetRuntimeProperties().Values
                        .Where(pi => pi.IsCandidateProperty())
                        .Select(pi => pi.GetSimpleMemberName()));

                clrProperties.ExceptWith(entityType.GetProperties().Select(p => p.Name));
                clrProperties.ExceptWith(entityType.GetNavigations().Select(p => p.Name));
                clrProperties.ExceptWith(entityType.GetServiceProperties().Select(p => p.Name));
                clrProperties.RemoveWhere(p => entityType.Builder.IsIgnored(p, ConfigurationSource.Convention));

                if (clrProperties.Count <= 0)
                {
                    continue;
                }

                foreach (var clrProperty in clrProperties)
                {
                    var actualProperty = entityType.GetRuntimeProperties()[clrProperty];
                    var propertyType = actualProperty.PropertyType;
                    var targetSequenceType = propertyType.TryGetSequenceType();

                    if (modelBuilder.IsIgnored(
                            modelBuilder.Metadata.GetDisplayName(propertyType),
                            ConfigurationSource.Convention)
                        || (targetSequenceType != null
                            && modelBuilder.IsIgnored(
                                modelBuilder.Metadata.GetDisplayName(targetSequenceType),
                                ConfigurationSource.Convention)))
                    {
                        continue;
                    }

                    var targetType = FindCandidateNavigationPropertyType(actualProperty);

                    var isTargetWeakOrOwned
                        = targetType != null
                          && (modelBuilder.Metadata.HasEntityTypeWithDefiningNavigation(targetType)
                              || modelBuilder.Metadata.ShouldBeOwnedType(targetType));

                    if (targetType?.IsValidEntityType() == true
                        && (isTargetWeakOrOwned
                            || modelBuilder.Metadata.FindEntityType(targetType) != null
                            || targetType.GetRuntimeProperties().Any(p => p.IsCandidateProperty())))
                    {
                        // ReSharper disable CheckForReferenceEqualityInstead.1
                        // ReSharper disable CheckForReferenceEqualityInstead.3
                        if ((!entityType.IsQueryType
                                || targetSequenceType == null)
                            && entityType.GetDerivedTypes().All(
                                dt => dt.FindDeclaredNavigation(actualProperty.GetSimpleMemberName()) == null)
                            && (!isTargetWeakOrOwned
                                || (!targetType.Equals(entityType.ClrType)
                                    && (!entityType.IsInOwnershipPath(targetType)
                                        || (entityType.FindOwnership().PrincipalEntityType.ClrType.Equals(targetType)
                                            && targetSequenceType == null))
                                    && (!entityType.IsInDefinitionPath(targetType)
                                        || (entityType.DefiningEntityType.ClrType.Equals(targetType)
                                            && targetSequenceType == null)))))
                        {
                            if (modelBuilder.Metadata.ShouldBeOwnedType(entityType.ClrType)
                                && modelBuilder.Metadata.ShouldBeOwnedType(targetType))
                            {
                                throw new InvalidOperationException(
                                    CoreStrings.AmbiguousOwnedNavigation(entityType.DisplayName(), targetType.ShortDisplayName()));
                            }

                            throw new InvalidOperationException(
                                CoreStrings.NavigationNotAdded(
                                    entityType.DisplayName(), actualProperty.Name, propertyType.ShortDisplayName()));
                        }
                        // ReSharper restore CheckForReferenceEqualityInstead.3
                        // ReSharper restore CheckForReferenceEqualityInstead.1
                    }
                    else if (targetSequenceType == null && propertyType.GetTypeInfo().IsInterface
                             || targetSequenceType?.GetTypeInfo().IsInterface == true)
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

            return modelBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual bool IsMappedPrimitiveProperty([NotNull] IProperty property)
            => _typeMappingSource.FindMapping(property) != null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual Type FindCandidateNavigationPropertyType([NotNull] PropertyInfo propertyInfo)
            => _memberClassifier.FindCandidateNavigationPropertyType(propertyInfo);
    }
}
