// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
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

            var fkPropertyOnPrincipal = FindCandidateDependentPropertyThroughEntityType(foreignKey.PrincipalEntityType, foreignKey.PrincipalToDependent?.Name);
            var fkPropertyOnDependent = FindCandidateDependentPropertyThroughEntityType(foreignKey.DeclaringEntityType, foreignKey.DependentToPrincipal?.Name);

            if (!string.IsNullOrEmpty(fkPropertyOnDependent)
                && !string.IsNullOrEmpty(fkPropertyOnPrincipal))
            {
                // TODO: Log Error that unable to determine principal end based on foreign key attributes
                var principalTypeNavigationName = foreignKey.PrincipalToDependent?.Name;
                var dependentTypeNavigationName = foreignKey.DependentToPrincipal?.Name;

                var dependentEntityTypebuilder = relationshipBuilder.ModelBuilder.Entity(foreignKey.DeclaringEntityType.Name, ConfigurationSource.Convention);
                var removedConfigurationSource = dependentEntityTypebuilder.RemoveRelationship(foreignKey, ConfigurationSource.DataAnnotation);

                if (removedConfigurationSource == null)
                {
                    return relationshipBuilder;
                }

                var principalEntityTypeBuilder = relationshipBuilder.ModelBuilder.Entity(foreignKey.PrincipalEntityType.Name, ConfigurationSource.Convention);

                dependentEntityTypebuilder.Relationship(
                    principalEntityTypeBuilder,
                    dependentEntityTypebuilder,
                    navigationToPrincipalName: dependentTypeNavigationName,
                    navigationToDependentName: null,
                    configurationSource: ConfigurationSource.DataAnnotation);

                principalEntityTypeBuilder.Relationship(
                    dependentEntityTypebuilder,
                    principalEntityTypeBuilder,
                    navigationToPrincipalName: principalTypeNavigationName,
                    navigationToDependentName: null,
                    configurationSource: ConfigurationSource.DataAnnotation);

                return null;
            }

            var fkPropertiesOnPrincipalToDependent = FindCandidateDependentPropertiesThroughNavigation(relationshipBuilder, pointsToPrincipal: false);
            var fkPropertiesOnDependentToPrincipal = FindCandidateDependentPropertiesThroughNavigation(relationshipBuilder, pointsToPrincipal: true);

            if (fkPropertiesOnDependentToPrincipal != null
                && fkPropertiesOnPrincipalToDependent != null
                && !fkPropertiesOnDependentToPrincipal.SequenceEqual(fkPropertiesOnPrincipalToDependent))
            {
                // TODO: Log error that mismatch in foreignKey Attribute on both navigations
                return relationshipBuilder;
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
                    newRelationshipBuilder = relationshipBuilder.ForeignKey(new List<string> { fkPropertyOnDependent }, ConfigurationSource.DataAnnotation);
                }
                else
                {
                    newRelationshipBuilder = relationshipBuilder.Invert(ConfigurationSource.DataAnnotation)
                        ?.ForeignKey(new List<string> { fkPropertyOnPrincipal }, ConfigurationSource.DataAnnotation);
                }
            }
            else
            {
                if (fkPropertyOnDependent == null && fkPropertyOnPrincipal == null)
                {
                    if (fkPropertiesOnNavigation.All(p => foreignKey.DeclaringEntityType.FindProperty(p) != null)
                        || fkPropertiesOnNavigation.Any(p => foreignKey.PrincipalEntityType.FindProperty(p) == null))
                    {
                        newRelationshipBuilder = relationshipBuilder.ForeignKey(fkPropertiesOnNavigation, ConfigurationSource.DataAnnotation);
                    }
                    else
                    {
                        newRelationshipBuilder = relationshipBuilder.Invert(ConfigurationSource.DataAnnotation)
                            ?.ForeignKey(fkPropertiesOnNavigation, ConfigurationSource.DataAnnotation);
                    }
                }
                else
                {
                    if (fkPropertiesOnNavigation.Count != 1
                        || !string.Equals(fkPropertiesOnNavigation.First(), fkPropertyOnDependent ?? fkPropertyOnPrincipal))
                    {
                        // TODO: Log error that mismatch in foreignKey Attribute on navigation and property
                        return relationshipBuilder;
                    }
                    if (fkPropertyOnDependent != null)
                    {
                        newRelationshipBuilder = relationshipBuilder.ForeignKey(fkPropertiesOnNavigation, ConfigurationSource.DataAnnotation);
                    }
                    else
                    {
                        newRelationshipBuilder = relationshipBuilder.Invert(ConfigurationSource.DataAnnotation)
                            ?.ForeignKey(fkPropertiesOnNavigation, ConfigurationSource.DataAnnotation);
                    }
                }
            }
            return newRelationshipBuilder ?? relationshipBuilder;
        }

        private ForeignKeyAttribute GetForeignKeyAttribute(EntityType entityType, string propertyName)
        {
            return entityType.ClrType?.GetRuntimeProperties().
                FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase))?.GetCustomAttribute<ForeignKeyAttribute>(true);
        }

        private string FindCandidateDependentPropertyThroughEntityType(EntityType entityType, string navigationName)
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

                var attribute = GetForeignKeyAttribute(entityType, propertyInfo.Name);
                if (attribute != null
                    && attribute.Name == navigationName)
                {
                    candidateProperties.Add(propertyInfo.Name);
                }
            }
            if (candidateProperties.Count > 1)
            {
                // TODO: Log error that multiple ForeignKey Attribute pointing to same Navigation found.
                return null;
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
                    // TODO: Log error stating invalid propertyName in ForeignKeyAttribute
                    return null;
                }

                return properties;
            }
            return null;
        }
    }
}
