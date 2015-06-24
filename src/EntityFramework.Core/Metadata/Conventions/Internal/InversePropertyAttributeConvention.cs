// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class InversePropertyAttributeConvention : NavigationAttributeEntityTypeConvention<InversePropertyAttribute>
    {
        public override InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder, PropertyInfo navigationPropertyInfo, InversePropertyAttribute attribute)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(navigationPropertyInfo, nameof(navigationPropertyInfo));
            Check.NotNull(attribute, nameof(attribute));

            if (!entityTypeBuilder.CanAddOrReplaceNavigation(navigationPropertyInfo.Name, ConfigurationSource.DataAnnotation))
            {
                return entityTypeBuilder;
            }

            var targetType = navigationPropertyInfo.FindCandidateNavigationPropertyType();
            var targetEntityTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(targetType, ConfigurationSource.DataAnnotation);
            if (targetEntityTypeBuilder == null)
            {
                return entityTypeBuilder;
            }

            // The navigation could have been added when the target entity type was added
            if (!entityTypeBuilder.CanAddOrReplaceNavigation(navigationPropertyInfo.Name, ConfigurationSource.DataAnnotation))
            {
                return entityTypeBuilder;
            }

            var inverseNavigationPropertyInfo = targetType.GetRuntimeProperties().FirstOrDefault(p => string.Equals(p.Name, attribute.Property, StringComparison.OrdinalIgnoreCase));

            if (inverseNavigationPropertyInfo == null
                || inverseNavigationPropertyInfo.FindCandidateNavigationPropertyType() != entityTypeBuilder.Metadata.ClrType)
            {
                throw new InvalidOperationException(
                    CoreStrings.InvalidNavigationWithInverseProperty(navigationPropertyInfo.Name, entityTypeBuilder.Metadata.Name, attribute.Property, targetType.FullName));
            }

            if (inverseNavigationPropertyInfo == navigationPropertyInfo)
            {
                throw new InvalidOperationException(
                    CoreStrings.SelfReferencingNavigationWithInverseProperty(
                        navigationPropertyInfo.Name,
                        entityTypeBuilder.Metadata.Name,
                        navigationPropertyInfo.Name,
                        entityTypeBuilder.Metadata.Name));
            }

            // Check for InversePropertyAttribute on the inverseNavigation to verify that it matches.
            var inverseAttribute = inverseNavigationPropertyInfo.GetCustomAttribute<InversePropertyAttribute>(true);
            if (inverseAttribute != null
                && inverseAttribute.Property != navigationPropertyInfo.Name)
            {
                throw new InvalidOperationException(
                    CoreStrings.InversePropertyMismatch(
                        navigationPropertyInfo.Name,
                        entityTypeBuilder.Metadata.Name,
                        inverseNavigationPropertyInfo.Name,
                        targetEntityTypeBuilder.Metadata.Name));
            }

            targetEntityTypeBuilder.Relationship(
                entityTypeBuilder,
                inverseNavigationPropertyInfo,
                navigationPropertyInfo,
                ConfigurationSource.DataAnnotation);

            return entityTypeBuilder;
        }
    }
}
