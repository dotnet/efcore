// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class ForeignKeyPropertyDiscoveryConvention : IForeignKeyConvention, INavigationConvention, IPropertyConvention
    {
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            var foreignKey = relationshipBuilder.Metadata;
            if (!foreignKey.Properties.All(fk => fk.IsShadowProperty))
            {
                return relationshipBuilder;
            }

            var foreignKeyProperties = FindCandidateForeignKeyProperties(
                foreignKey, onDependent: true);
            if (foreignKey.IsUnique
                && !foreignKey.IsSelfPrimaryKeyReferencing())
            {
                var candidatePropertiesOnPrincipal = FindCandidateForeignKeyProperties(
                    foreignKey, onDependent: false);

                if (candidatePropertiesOnPrincipal != null)
                {
                    if ((foreignKeyProperties == null)
                        && relationshipBuilder.CanInvert(candidatePropertiesOnPrincipal, ConfigurationSource.Convention))
                    {
                        // Invert only if principal side has matching property & dependent does not have
                        relationshipBuilder = relationshipBuilder
                            .DependentEntityType(foreignKey.PrincipalEntityType, ConfigurationSource.Convention)
                            .HasForeignKey(candidatePropertiesOnPrincipal, ConfigurationSource.Convention);

                        Debug.Assert(relationshipBuilder != null);
                        return relationshipBuilder;
                    }

                    // Return if both sides have matching property
                    return relationshipBuilder;
                }

                // Only match with PK if the principal end is set
                if ((!ConfigurationSource.Convention.Overrides(foreignKey.GetPrincipalEndConfigurationSource())
                     || !ConfigurationSource.Convention.Overrides(foreignKey.GetPrincipalKeyConfigurationSource()))
                    && (foreignKeyProperties == null))
                {
                    foreignKeyProperties = GetCompatiblePrimaryKeyProperties(
                        foreignKey.DeclaringEntityType,
                        foreignKey.PrincipalEntityType,
                        foreignKey.PrincipalKey.Properties);
                }
            }

            if ((foreignKeyProperties == null)
                || (foreignKey.DeclaringEntityType.FindForeignKey(foreignKeyProperties, foreignKey.PrincipalKey, foreignKey.PrincipalEntityType) != null))
            {
                return relationshipBuilder;
            }

            var newRelationshipBuilder = relationshipBuilder.HasForeignKey(foreignKeyProperties, ConfigurationSource.Convention);
            if (newRelationshipBuilder != null)
            {
                return newRelationshipBuilder;
            }

            return relationshipBuilder;
        }

        private IReadOnlyList<Property> FindCandidateForeignKeyProperties(ForeignKey foreignKey, bool onDependent)
        {
            var baseNames = new List<string>();
            var navigation = onDependent
                ? foreignKey.DependentToPrincipal
                : foreignKey.PrincipalToDependent;
            if (navigation != null)
            {
                baseNames.Add(navigation.Name);
            }

            var entityTypeToReference = onDependent
                ? foreignKey.PrincipalEntityType
                : foreignKey.DeclaringEntityType;
            baseNames.Add(entityTypeToReference.DisplayName());

            baseNames.Add("");

            foreach (var baseName in baseNames)
            {
                var match = FindMatchingNonShadowProperties(foreignKey, baseName, onDependent);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static IReadOnlyList<Property> GetCompatiblePrimaryKeyProperties(EntityType dependentEntityType, EntityType principalEntityType, IReadOnlyList<Property> propertiesToReference)
        {
            var dependentPkProperties = dependentEntityType.FindPrimaryKey()?.Properties;
            if ((dependentPkProperties != null)
                && ForeignKey.AreCompatible(
                    propertiesToReference,
                    dependentPkProperties,
                    principalEntityType,
                    dependentEntityType,
                    shouldThrow: false))
            {
                return dependentPkProperties;
            }

            return null;
        }

        private IReadOnlyList<Property> FindMatchingNonShadowProperties(
            ForeignKey foreignKey, string baseName, bool onDependent)
        {
            var dependentEntityType = onDependent
                ? foreignKey.DeclaringEntityType
                : foreignKey.PrincipalEntityType;
            var principalEntityType = onDependent
                ? foreignKey.PrincipalEntityType
                : foreignKey.DeclaringEntityType;
            var propertiesToReference = onDependent
                ? foreignKey.PrincipalKey.Properties
                : foreignKey.DeclaringEntityType.FindPrimaryKey()?.Properties;

            if (propertiesToReference == null)
            {
                return null;
            }

            var foreignKeyProperties = new List<Property>();
            foreach (IProperty referencedProperty in propertiesToReference)
            {
                var property = TryGetProperty(dependentEntityType,
                    baseName + referencedProperty.Name,
                    referencedProperty.ClrType.UnwrapNullableType());

                if (property != null)
                {
                    foreignKeyProperties.Add(property);
                }
            }

            if ((propertiesToReference.Count == 1)
                && (foreignKeyProperties.Count == 0))
            {
                var property = TryGetProperty(dependentEntityType,
                    baseName + "Id",
                    propertiesToReference.Single().ClrType.UnwrapNullableType());

                if (property != null)
                {
                    foreignKeyProperties.Add(property);
                }
            }

            if (foreignKeyProperties.Count < propertiesToReference.Count)
            {
                return null;
            }

            if (!ForeignKey.AreCompatible(
                propertiesToReference,
                foreignKeyProperties,
                principalEntityType,
                dependentEntityType,
                shouldThrow: false))
            {
                return null;
            }

            var primaryKey = dependentEntityType.FindPrimaryKey();
            if (primaryKey != null)
            {
                if (foreignKeyProperties.All(property => primaryKey.Properties.Contains(property)))
                {
                    return null;
                }
            }

            return foreignKeyProperties;
        }

        private static Property TryGetProperty(EntityType entityType, string name, Type type)
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                    && !property.IsShadowProperty
                    && (property.ClrType.UnwrapNullableType() == type))
                {
                    return property;
                }
            }
            return null;
        }

        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder, Navigation navigation)
            => Apply(relationshipBuilder);

        public virtual InternalPropertyBuilder Apply(InternalPropertyBuilder propertyBuilder)
        {
            if (!((IProperty)propertyBuilder.Metadata).IsShadowProperty)
            {
                var entityType = propertyBuilder.Metadata.DeclaringEntityType;

                foreach (var foreignKey in entityType.GetDeclaredForeignKeys().Concat(entityType.GetDerivedForeignKeys()).ToList())
                {
                    Apply(foreignKey.Builder);
                }

                foreach (var foreignKey in entityType.GetReferencingForeignKeys().ToList())
                {
                    Apply(foreignKey.Builder);
                }
            }
            return propertyBuilder;
        }
    }
}
