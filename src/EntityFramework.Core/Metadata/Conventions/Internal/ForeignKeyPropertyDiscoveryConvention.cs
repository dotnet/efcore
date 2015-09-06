// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class ForeignKeyPropertyDiscoveryConvention : IForeignKeyConvention
    {
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            var foreignKey = (IForeignKey)relationshipBuilder.Metadata;
            if (foreignKey.Properties.All(fk => fk.IsShadowProperty))
            {
                var foreignKeyProperties = FindCandidateForeignKeyProperties(relationshipBuilder, onDependent: true);

                if (foreignKey.IsUnique
                    && !foreignKey.IsSelfPrimaryKeyReferencing())
                {
                    if (ShouldFlip(relationshipBuilder, foreignKeyProperties))
                    {
                        var newRelationshipBuilder = relationshipBuilder.Invert(ConfigurationSource.Convention);
                        if (newRelationshipBuilder != null)
                        {
                            return newRelationshipBuilder;
                        }
                    }

                    if (foreignKeyProperties == null)
                    {
                        foreignKeyProperties = GetCompatiblePrimaryKeyProperties(
                            relationshipBuilder.Metadata.DeclaringEntityType, relationshipBuilder.Metadata.PrincipalKey.Properties);
                    }
                }

                if (foreignKeyProperties != null
                    && relationshipBuilder.Metadata.DeclaringEntityType.FindForeignKey(foreignKeyProperties) == null)
                {
                    var newRelationshipBuilder = relationshipBuilder.ForeignKey(foreignKeyProperties, ConfigurationSource.Convention);
                    if (newRelationshipBuilder != null)
                    {
                        return newRelationshipBuilder;
                    }
                }
            }

            return relationshipBuilder;
        }

        private bool ShouldFlip(InternalRelationshipBuilder relationshipBuilder, IReadOnlyList<Property> currentDependentCandidateProperties)
        {
            var foreignKey = relationshipBuilder.Metadata;
            var currentPrincipalCandidateProperties = FindCandidateForeignKeyProperties(relationshipBuilder, onDependent: false);

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

        private IReadOnlyList<Property> FindCandidateForeignKeyProperties(InternalRelationshipBuilder relationshipBuilder, bool onDependent)
        {
            var foreignKey = relationshipBuilder.Metadata;
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
                var match = FindMatchingNonShadowProperties(relationshipBuilder, baseName, onDependent);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private IReadOnlyList<Property> GetCompatiblePrimaryKeyProperties(EntityType entityType, IReadOnlyList<Property> propertiesToReference)
        {
            var dependentPkProperties = entityType.FindPrimaryKey()?.Properties;
            if (dependentPkProperties != null
                && Property.AreCompatible(propertiesToReference, dependentPkProperties))
            {
                return dependentPkProperties;
            }

            return null;
        }

        private IReadOnlyList<Property> FindMatchingNonShadowProperties(InternalRelationshipBuilder relationshipBuilder, string baseName, bool onDependent)
        {
            var foreignKey = relationshipBuilder.Metadata;
            var entityType = onDependent
                ? foreignKey.DeclaringEntityType
                : foreignKey.PrincipalEntityType;
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
                var property = TryGetProperty(entityType,
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
                var property = TryGetProperty(entityType,
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
            
            if (!relationshipBuilder.CanSetForeignKey(foreignKeyProperties, ConfigurationSource.Convention))
            {
                return null;
            }

            var primaryKey = entityType.FindPrimaryKey();
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
    }
}
