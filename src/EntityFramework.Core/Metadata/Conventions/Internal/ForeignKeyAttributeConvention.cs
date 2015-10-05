// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
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

            if (fkPropertiesOnDependentToPrincipal != null
                && fkPropertiesOnPrincipalToDependent != null
                && !fkPropertiesOnDependentToPrincipal.SequenceEqual(fkPropertiesOnPrincipalToDependent))
            {
                // TODO: Log error that foreign key properties on both navigations do not match
                SplitNavigationsInSeparateRelationships(relationshipBuilder);

                return null;
            }

            var fkPropertiesOnNavigation = fkPropertiesOnDependentToPrincipal ?? fkPropertiesOnPrincipalToDependent;

            InternalRelationshipBuilder newRelationshipBuilder = null;
            if (fkPropertiesOnNavigation == null || fkPropertiesOnNavigation.Count == 0)
            {
                if (fkPropertyOnDependent == null && fkPropertyOnPrincipal == null)
                {
                    return relationshipBuilder;
                }
                if (fkPropertyOnDependent != null)
                {
                    newRelationshipBuilder = relationshipBuilder.HasForeignKey(new List<string> { fkPropertyOnDependent }, ConfigurationSource.DataAnnotation);
                }
                else
                {
                    newRelationshipBuilder = relationshipBuilder.Invert(ConfigurationSource.DataAnnotation)
                        ?.HasForeignKey(new List<string> { fkPropertyOnPrincipal }, ConfigurationSource.DataAnnotation);
                }
            }
            else
            {
                if (fkPropertyOnDependent == null && fkPropertyOnPrincipal == null)
                {
                    if (fkPropertiesOnNavigation.All(p => foreignKey.DeclaringEntityType.FindProperty(p) != null)
                        || fkPropertiesOnNavigation.Any(p => foreignKey.PrincipalEntityType.FindProperty(p) == null))
                    {
                        newRelationshipBuilder = relationshipBuilder.HasForeignKey(fkPropertiesOnNavigation, ConfigurationSource.DataAnnotation);
                    }
                    else
                    {
                        newRelationshipBuilder = relationshipBuilder.Invert(ConfigurationSource.DataAnnotation)
                            ?.HasForeignKey(fkPropertiesOnNavigation, ConfigurationSource.DataAnnotation);
                    }
                }
                else
                {
                    if (fkPropertiesOnNavigation.Count != 1
                       || !string.Equals(fkPropertiesOnNavigation.First(), fkPropertyOnDependent ?? fkPropertyOnPrincipal))
                    {
                        // TODO: Log error that mismatch in foreignKey Attribute on navigation and property
                        SplitNavigationsInSeparateRelationships(relationshipBuilder);

                        return null;
                    }

                    if (fkPropertyOnDependent != null)
                    {
                        newRelationshipBuilder = relationshipBuilder.HasForeignKey(fkPropertiesOnNavigation, ConfigurationSource.DataAnnotation);
                    }
                    else
                    {
                        newRelationshipBuilder = relationshipBuilder.Invert(ConfigurationSource.DataAnnotation)
                            ?.HasForeignKey(fkPropertiesOnNavigation, ConfigurationSource.DataAnnotation);
                    }
                }
            }
            return newRelationshipBuilder ?? relationshipBuilder;
        }

        private void SplitNavigationsInSeparateRelationships(InternalRelationshipBuilder relationshipBuilder)
        {
            var foreignKey = relationshipBuilder.Metadata;
            var dependentToPrincipalNavigationName = foreignKey.DependentToPrincipal?.Name;
            var principalToDepedentNavigationName = foreignKey.PrincipalToDependent?.Name;


            if (GetInversePropertyAttributeOnNavigation(foreignKey.PrincipalToDependent) != null
                || GetInversePropertyAttributeOnNavigation(foreignKey.DependentToPrincipal) != null)
            {
                // Relationship is joined by InversePropertyAttribute
                throw new InvalidOperationException(CoreStrings.InvalidRelationshipUsingDataAnnotations(
                    dependentToPrincipalNavigationName,
                    foreignKey.DeclaringEntityType.Name,
                    principalToDepedentNavigationName,
                    foreignKey.PrincipalEntityType.Name));
            }

            var dependentEntityTypebuilder = relationshipBuilder.ModelBuilder.Entity(foreignKey.DeclaringEntityType.Name, ConfigurationSource.Convention);
            var removedConfigurationSource = dependentEntityTypebuilder.RemoveForeignKey(foreignKey, ConfigurationSource.DataAnnotation);

            if (removedConfigurationSource == null)
            {
                // Relationship not removed
                return;
            }

            var principalEntityTypeBuilder = relationshipBuilder.ModelBuilder.Entity(foreignKey.PrincipalEntityType.Name, ConfigurationSource.Convention);

            dependentEntityTypebuilder.Relationship(
                principalEntityTypeBuilder,
                dependentEntityTypebuilder,
                navigationToPrincipalName: dependentToPrincipalNavigationName,
                navigationToDependentName: null,
                configurationSource: ConfigurationSource.DataAnnotation);

            principalEntityTypeBuilder.Relationship(
                dependentEntityTypebuilder,
                principalEntityTypeBuilder,
                navigationToPrincipalName: principalToDepedentNavigationName,
                navigationToDependentName: null,
                configurationSource: ConfigurationSource.DataAnnotation);
        }

        private InversePropertyAttribute GetInversePropertyAttributeOnNavigation(Navigation navigation)
        {
            return navigation.DeclaringEntityType.ClrType?.GetRuntimeProperties()
                .FirstOrDefault(p => string.Equals(p.Name, navigation.Name, StringComparison.OrdinalIgnoreCase))
                ?.GetCustomAttribute<InversePropertyAttribute>(true);
        }

        private ForeignKeyAttribute GetForeignKeyAttribute(EntityType entityType, string propertyName)
        {
            return entityType.ClrType?.GetRuntimeProperties()
                .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                ?.GetCustomAttribute<ForeignKeyAttribute>(true);
        }

        private string FindForeignKeyAttributeOnProperty(EntityType entityType, string navigationName)
        {
            if (string.IsNullOrWhiteSpace(navigationName)
                || !entityType.HasClrType)
            {
                return null;
            }

            var candidateProperties = new List<string>();
            foreach (var propertyInfo in entityType.ClrType.GetRuntimeProperties().OrderBy(p => p.Name))
            {
                var targetType = propertyInfo.FindCandidateNavigationPropertyType();
                if (targetType != null)
                {
                    continue;
                }

                var attribute = propertyInfo.GetCustomAttribute<ForeignKeyAttribute>(true);
                if (attribute != null
                    && attribute.Name == navigationName)
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
                if (fkAttributeOnNavigation != null && fkAttributeOnNavigation.Name != candidateProperties.First())
                {
                    throw new InvalidOperationException(CoreStrings.FkAttributeOnPropertyNavigationMismatch(candidateProperties.First(), navigationName, entityType.Name));
                }
            }

            return candidateProperties.FirstOrDefault();
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

                return properties;
            }
            return null;
        }
    }
}
