// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class ForeignKeyAttributeConvention : IForeignKeyConvention
    {
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            Check.NotNull(relationshipBuilder, nameof(relationshipBuilder));

            var foreignKey = relationshipBuilder.Metadata;

            var fkPropertyOnPrincipal = FindForeignKeyAttributeOnProperty(foreignKey.PrincipalEntityType, foreignKey.PrincipalToDependent?.Name);
            var fkPropertyOnDependent = FindForeignKeyAttributeOnProperty(foreignKey.DeclaringEntityType, foreignKey.DependentToPrincipal?.Name);

            if (!string.IsNullOrEmpty(fkPropertyOnDependent)
                && !string.IsNullOrEmpty(fkPropertyOnPrincipal))
            {
                // TODO: Log Error that unable to determine principal end based on foreign key attributes on properties
                SplitNavigationsInSeparateRelationships(relationshipBuilder);

                return null;
            }

            var fkPropertiesOnPrincipalToDependent = FindCandidateDependentPropertiesThroughNavigation(relationshipBuilder, pointsToPrincipal: false);
            var fkPropertiesOnDependentToPrincipal = FindCandidateDependentPropertiesThroughNavigation(relationshipBuilder, pointsToPrincipal: true);

            if ((fkPropertiesOnDependentToPrincipal != null)
                && (fkPropertiesOnPrincipalToDependent != null)
                && !fkPropertiesOnDependentToPrincipal.SequenceEqual(fkPropertiesOnPrincipalToDependent))
            {
                // TODO: Log error that foreign key properties on both navigations do not match
                SplitNavigationsInSeparateRelationships(relationshipBuilder);

                return null;
            }

            var fkPropertiesOnNavigation = fkPropertiesOnDependentToPrincipal ?? fkPropertiesOnPrincipalToDependent;
            var upgradePrincipalToDependentNavigationSource = fkPropertiesOnPrincipalToDependent != null;
            var upgradeDependentToPrincipalNavigationSource = fkPropertiesOnDependentToPrincipal != null;
            ConfigurationSource? invertConfigurationSource = null;
            IReadOnlyList<string> fkPropertiesToSet;

            if ((fkPropertiesOnNavigation == null)
                || (fkPropertiesOnNavigation.Count == 0))
            {
                if ((fkPropertyOnDependent == null)
                    && (fkPropertyOnPrincipal == null))
                {
                    return relationshipBuilder;
                }
                if (fkPropertyOnDependent != null)
                {
                    fkPropertiesToSet = new List<string> { fkPropertyOnDependent };
                    upgradeDependentToPrincipalNavigationSource = true;
                }
                else
                {
                    invertConfigurationSource = ConfigurationSource.DataAnnotation;
                    fkPropertiesToSet = new List<string> { fkPropertyOnPrincipal };
                    upgradeDependentToPrincipalNavigationSource = true;
                }
            }
            else
            {
                fkPropertiesToSet = fkPropertiesOnNavigation;
                if ((fkPropertyOnDependent == null)
                    && (fkPropertyOnPrincipal == null))
                {
                    if (fkPropertiesOnPrincipalToDependent != null
                        && foreignKey.IsUnique)
                    {
                        invertConfigurationSource = ConfigurationSource.DataAnnotation;
                    }
                }
                else
                {
                    if ((fkPropertiesOnNavigation.Count != 1)
                        || !string.Equals(fkPropertiesOnNavigation.First(), fkPropertyOnDependent ?? fkPropertyOnPrincipal))
                    {
                        // TODO: Log error that mismatch in foreignKey Attribute on navigation and property
                        SplitNavigationsInSeparateRelationships(relationshipBuilder);

                        return null;
                    }

                    if (fkPropertyOnDependent != null)
                    {
                        upgradeDependentToPrincipalNavigationSource = true;
                    }
                    else
                    {
                        invertConfigurationSource = ConfigurationSource.DataAnnotation;
                    }
                }
            }

            var newRelationshipBuilder = relationshipBuilder;
            if (invertConfigurationSource != null)
            {
                newRelationshipBuilder = newRelationshipBuilder.RelatedEntityTypes(
                    foreignKey.DeclaringEntityType, foreignKey.PrincipalEntityType, invertConfigurationSource.Value);
                if (newRelationshipBuilder != null)
                {
                    var temp = upgradeDependentToPrincipalNavigationSource;
                    upgradeDependentToPrincipalNavigationSource = upgradePrincipalToDependentNavigationSource;
                    upgradePrincipalToDependentNavigationSource = temp;
                }
            }
            if (newRelationshipBuilder != null
                && upgradeDependentToPrincipalNavigationSource)
            {
                newRelationshipBuilder = newRelationshipBuilder.DependentToPrincipal(
                    newRelationshipBuilder.Metadata.DependentToPrincipal.Name, ConfigurationSource.DataAnnotation);
            }
            if (newRelationshipBuilder != null
                && upgradePrincipalToDependentNavigationSource)
            {
                newRelationshipBuilder = newRelationshipBuilder.PrincipalToDependent(
                    newRelationshipBuilder.Metadata.PrincipalToDependent.Name, ConfigurationSource.DataAnnotation);
            }

            return newRelationshipBuilder?.HasForeignKey(fkPropertiesToSet, ConfigurationSource.DataAnnotation) ?? relationshipBuilder;
        }

        private void SplitNavigationsInSeparateRelationships(InternalRelationshipBuilder relationshipBuilder)
        {
            var foreignKey = relationshipBuilder.Metadata;
            var dependentToPrincipalNavigationName = foreignKey.DependentToPrincipal?.Name;
            var principalToDepedentNavigationName = foreignKey.PrincipalToDependent?.Name;

            if ((GetInversePropertyAttributeOnNavigation(foreignKey.PrincipalToDependent) != null)
                || (GetInversePropertyAttributeOnNavigation(foreignKey.DependentToPrincipal) != null))
            {
                // Relationship is joined by InversePropertyAttribute
                throw new InvalidOperationException(CoreStrings.InvalidRelationshipUsingDataAnnotations(
                    dependentToPrincipalNavigationName,
                    foreignKey.DeclaringEntityType.Name,
                    principalToDepedentNavigationName,
                    foreignKey.PrincipalEntityType.Name));
            }

            var dependentEntityTypebuilder = foreignKey.DeclaringEntityType.Builder;
            var principalEntityTypeBuilder = foreignKey.PrincipalEntityType.Builder;

            dependentEntityTypebuilder.Relationship(
                principalEntityTypeBuilder,
                dependentToPrincipalNavigationName,
                null,
                ConfigurationSource.DataAnnotation);

            principalEntityTypeBuilder.Relationship(
                dependentEntityTypebuilder,
                principalToDepedentNavigationName,
                null,
                ConfigurationSource.DataAnnotation);
        }

        private static InversePropertyAttribute GetInversePropertyAttributeOnNavigation(Navigation navigation)
        {
            return navigation.DeclaringEntityType.ClrType?.GetRuntimeProperties()
                .FirstOrDefault(p => string.Equals(p.Name, navigation.Name, StringComparison.OrdinalIgnoreCase))
                ?.GetCustomAttribute<InversePropertyAttribute>(true);
        }

        private static ForeignKeyAttribute GetForeignKeyAttribute(EntityType entityType, string propertyName)
        {
            return entityType.ClrType?.GetRuntimeProperties()
                .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                ?.GetCustomAttribute<ForeignKeyAttribute>(true);
        }

        private string FindForeignKeyAttributeOnProperty(EntityType entityType, string navigationName)
        {
            if (string.IsNullOrWhiteSpace(navigationName)
                || !entityType.HasClrType())
            {
                return null;
            }

            var candidateProperties = new List<string>();
            foreach (var propertyInfo in entityType.ClrType.GetRuntimeProperties().OrderBy(p => p.Name))
            {
                var targetType = FindCandidateNavigationPropertyType(propertyInfo);
                if (targetType != null)
                {
                    continue;
                }

                var attribute = propertyInfo.GetCustomAttribute<ForeignKeyAttribute>(true);
                if ((attribute != null)
                    && (attribute.Name == navigationName))
                {
                    candidateProperties.Add(propertyInfo.Name);
                }
            }

            if (candidateProperties.Count > 1)
            {
                throw new InvalidOperationException(CoreStrings.CompositeFkOnProperty(navigationName, entityType.Name));
            }

            if (candidateProperties.Count == 1)
            {
                var fkAttributeOnNavigation = GetForeignKeyAttribute(entityType, navigationName);
                if ((fkAttributeOnNavigation != null)
                    && (fkAttributeOnNavigation.Name != candidateProperties.First()))
                {
                    throw new InvalidOperationException(CoreStrings.FkAttributeOnPropertyNavigationMismatch(candidateProperties.First(), navigationName, entityType.Name));
                }
            }

            return candidateProperties.FirstOrDefault();
        }

        public virtual Type FindCandidateNavigationPropertyType([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return propertyInfo.FindCandidateNavigationPropertyType(clrType => clrType.IsPrimitive());
        }

        private IReadOnlyList<string> FindCandidateDependentPropertiesThroughNavigation(InternalRelationshipBuilder relationshipBuilder, bool pointsToPrincipal)
        {
            var navigation = pointsToPrincipal
                ? relationshipBuilder.Metadata.DependentToPrincipal
                : relationshipBuilder.Metadata.PrincipalToDependent;

            var navigationFkAttribute = navigation != null
                ? GetForeignKeyAttribute(navigation.DeclaringEntityType, navigation.Name)
                : null;

            if (navigationFkAttribute != null)
            {
                var properties = navigationFkAttribute.Name.Split(',').Select(p => p.Trim()).ToList();

                if (properties.Any(string.IsNullOrWhiteSpace))
                {
                    throw new InvalidOperationException(CoreStrings.InvalidPropertyListOnNavigation(navigation.Name, navigation.DeclaringEntityType.Name));
                }

                var navigationPropertyTargetType = navigation.DeclaringEntityType.ClrType.GetRuntimeProperties()
                    .Single(p => p.Name == navigation.Name).PropertyType;

                var otherNavigations = navigation.DeclaringEntityType.ClrType.GetRuntimeProperties()
                    .Where(p => p.PropertyType == navigationPropertyTargetType && p.Name != navigation.Name)
                    .OrderBy(p => p.Name);
                foreach (var propertyInfo in otherNavigations)
                {
                    var attribute = propertyInfo.GetCustomAttribute<ForeignKeyAttribute>(true);
                    if ((attribute != null)
                        && (attribute.Name == navigationFkAttribute.Name))
                    {
                        throw new InvalidOperationException(CoreStrings.MultipleNavigationsSameFk(navigation.DeclaringEntityType.DisplayName(), attribute.Name));
                    }
                }

                return properties;
            }
            return null;
        }
    }
}
