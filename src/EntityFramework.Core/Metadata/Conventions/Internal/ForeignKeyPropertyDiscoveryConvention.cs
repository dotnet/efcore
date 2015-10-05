// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class ForeignKeyPropertyDiscoveryConvention : IForeignKeyConvention, INavigationConvention, IPropertyConvention
    {
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            var foreignKey = (IForeignKey)relationshipBuilder.Metadata;
            if (!foreignKey.Properties.All(fk => fk.IsShadowProperty))
            {
                return relationshipBuilder;
            }

            var foreignKeyProperties = FindCandidateForeignKeyProperties(
                relationshipBuilder.Metadata, onDependent: true);
            if (foreignKey.IsUnique
                && !foreignKey.IsSelfPrimaryKeyReferencing())
            {
                var candidatePropertiesOnPrincipal = FindCandidateForeignKeyProperties(
                    relationshipBuilder.Metadata, onDependent: false);

                bool shouldInvert;
                if (ShouldFlip(relationshipBuilder.Metadata, foreignKeyProperties, candidatePropertiesOnPrincipal)
                    && relationshipBuilder.CanSet(relationshipBuilder.Metadata.DeclaringEntityType,
                        relationshipBuilder.Metadata.PrincipalEntityType,
                        null,
                        null,
                        /*dependentProperties:*/ candidatePropertiesOnPrincipal,
                        null,
                        null,
                        null,
                        null,
                        true,
                        ConfigurationSource.Convention,
                        out shouldInvert))
                {
                    Debug.Assert(shouldInvert);
                    var invertedBuilder = relationshipBuilder.Invert(ConfigurationSource.Convention);
                    Debug.Assert(invertedBuilder != null);

                    if (candidatePropertiesOnPrincipal == null)
                    {
                        return invertedBuilder;
                    }

                    // TODO: Remove, as this is redundant
                    invertedBuilder = invertedBuilder.HasForeignKey(
                        candidatePropertiesOnPrincipal, ConfigurationSource.Convention);
                    Debug.Assert(invertedBuilder != null);
                    return invertedBuilder;
                }

                if (foreignKeyProperties == null)
                {
                    foreignKeyProperties = GetCompatiblePrimaryKeyProperties(
                        relationshipBuilder.Metadata.DeclaringEntityType,
                        relationshipBuilder.Metadata.PrincipalEntityType,
                        relationshipBuilder.Metadata.PrincipalKey.Properties);
                }
            }

            if (foreignKeyProperties != null
                && relationshipBuilder.Metadata.DeclaringEntityType.FindForeignKey(foreignKeyProperties) == null)
            {
                var newRelationshipBuilder = relationshipBuilder.HasForeignKey(foreignKeyProperties, ConfigurationSource.Convention);
                if (newRelationshipBuilder != null)
                {
                    return newRelationshipBuilder;
                }
            }

            return relationshipBuilder;
        }

        private bool ShouldFlip(
            ForeignKey foreignKey,
            IReadOnlyList<Property> currentDependentCandidateProperties,
            IReadOnlyList<Property> currentPrincipalCandidateProperties)
        {
            if (currentDependentCandidateProperties != null
                && currentPrincipalCandidateProperties == null)
            {
                return false;
            }

            if (currentDependentCandidateProperties == null
                && currentPrincipalCandidateProperties != null)
            {
                return true;
            }

            var navigationToPrincipal = foreignKey.DependentToPrincipal;
            var navigationToDependent = foreignKey.PrincipalToDependent;

            if (navigationToPrincipal == null
                && navigationToDependent != null)
            {
                return false;
            }

            if (navigationToPrincipal != null
                && navigationToDependent == null)
            {
                return true;
            }

            var model = foreignKey.DeclaringEntityType.Model;
            var principalPk = foreignKey.PrincipalEntityType.FindPrimaryKey();
            var principalPkReferenceThreshold = foreignKey.PrincipalKey == principalPk ? 1 : 0;
            var isPrincipalKeyReferenced = principalPk != null && model.FindReferencingForeignKeys(principalPk).Count() > principalPkReferenceThreshold;
            var dependentPk = foreignKey.DeclaringEntityType.FindPrimaryKey();
            var isDependentPrimaryKeyReferenced = dependentPk != null && model.FindReferencingForeignKeys(dependentPk).Any();

            if (isPrincipalKeyReferenced
                && !isDependentPrimaryKeyReferenced)
            {
                return false;
            }

            if (!isPrincipalKeyReferenced
                && isDependentPrimaryKeyReferenced)
            {
                return true;
            }

            return StringComparer.Ordinal.Compare(foreignKey.PrincipalEntityType.Name, foreignKey.DeclaringEntityType.Name) > 0;
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

        private IReadOnlyList<Property> GetCompatiblePrimaryKeyProperties(EntityType dependentEntityType, EntityType principalEntityType, IReadOnlyList<Property> propertiesToReference)
        {
            var dependentPkProperties = dependentEntityType.FindPrimaryKey()?.Properties;
            if (dependentPkProperties != null
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

            if (propertiesToReference.Count == 1
                && foreignKeyProperties.Count == 0)
            {
                var property = TryGetProperty(dependentEntityType,
                    baseName + "Id",
                    ((IProperty)propertiesToReference.Single()).ClrType.UnwrapNullableType());

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

        private Property TryGetProperty(EntityType entityType, string name, Type type)
        {
            foreach (var mutableProperty in entityType.Properties)
            {
                var property = (IProperty)mutableProperty;
                if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                    && !property.IsShadowProperty
                    && property.ClrType.UnwrapNullableType() == type)
                {
                    return mutableProperty;
                }
            }
            return null;
        }

        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder, Navigation navigation)
            => Apply(relationshipBuilder);

        public virtual InternalPropertyBuilder Apply(InternalPropertyBuilder propertyBuilder)
        {
            if (!((IProperty)(propertyBuilder.Metadata)).IsShadowProperty)
            {
                var entityType = propertyBuilder.Metadata.DeclaringEntityType;
                var entityTypeBuilder = propertyBuilder.ModelBuilder.Entity(entityType.Name, ConfigurationSource.Convention);

                foreach (var foreignKey in entityType.GetDeclaredForeignKeys().ToList())
                {
                    var relationshipBuilder = entityTypeBuilder.Relationship(foreignKey, existingForeignKey: true, configurationSource: ConfigurationSource.Convention);
                    Apply(relationshipBuilder);
                }
                foreach (var foreignKey in entityType.FindReferencingForeignKeys().ToList())
                {
                    var relationshipBuilder = entityTypeBuilder.Relationship(foreignKey, existingForeignKey: true, configurationSource: ConfigurationSource.Convention);
                    Apply(relationshipBuilder);
                }
            }
            return propertyBuilder;
        }
    }
}
