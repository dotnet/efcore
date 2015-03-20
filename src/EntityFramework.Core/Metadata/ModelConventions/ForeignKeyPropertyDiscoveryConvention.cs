// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.Metadata.ModelConventions
{
    public class ForeignKeyPropertyDiscoveryConvention : IRelationshipConvention
    {
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            if (relationshipBuilder.Metadata.Properties.All(fk => fk.IsShadowProperty))
            {
                var foreignKey = relationshipBuilder.Metadata;
                var foreignKeyProperties = GetCandidateForeignKeyProperties(foreignKey, onDependent: true);
                
                if (((IForeignKey)foreignKey).IsUnique
                    && !foreignKey.IsSelfReferencing())
                {
                    if (ShouldFlip(foreignKey, foreignKeyProperties))
                    {
                        var newRelationshipBuilder = relationshipBuilder.Invert(ConfigurationSource.Convention);
                        if (newRelationshipBuilder != null)
                        {
                            return newRelationshipBuilder;
                        }
                    }

                    if (foreignKeyProperties == null
                        && foreignKey.IsRequired != false)
                    {
                        foreignKeyProperties = GetCompatiblePrimaryKeyProperties(foreignKey.EntityType, foreignKey.ReferencedKey.Properties);
                    }
                }

                if (foreignKeyProperties != null
                    && relationshipBuilder.Metadata.EntityType.TryGetForeignKey(foreignKeyProperties) == null)
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

        private bool ShouldFlip(ForeignKey foreignKey, IReadOnlyList<Property> currentDependentCandidateProperties)
        {
            var currentPrincipalCandidateProperties = GetCandidateForeignKeyProperties(foreignKey, onDependent: false);
            
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

            var navigationToPrincipal = foreignKey.GetNavigationToPrincipal();
            var navigationToDependent = foreignKey.GetNavigationToDependent();

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

            var model = foreignKey.EntityType.Model;
            var principalPk = foreignKey.ReferencedEntityType.TryGetPrimaryKey();
            var principalPkReferenceThreshold = foreignKey.ReferencedKey == principalPk? 1 : 0;
            var isPrincipalKeyReferenced = principalPk != null && model.GetReferencingForeignKeys(principalPk).Count > principalPkReferenceThreshold;
            var dependentPk = foreignKey.EntityType.TryGetPrimaryKey();
            var isDependentPrimaryKeyReferenced = dependentPk != null && model.GetReferencingForeignKeys(dependentPk).Count > 0;

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
            
            return StringComparer.Ordinal.Compare(foreignKey.ReferencedEntityType.Name, foreignKey.EntityType.Name) > 0;
        }

        private IReadOnlyList<Property> GetCandidateForeignKeyProperties(ForeignKey foreignKey, bool onDependent)
        {
            var baseNames = new List<string>();
            var navigation = onDependent
                ? foreignKey.GetNavigationToPrincipal()
                : foreignKey.GetNavigationToDependent();
            if (navigation != null)
            {
                baseNames.Add(navigation.Name);
            }

            var entityTypeToReference = onDependent
                ? foreignKey.ReferencedEntityType
                : foreignKey.EntityType;
            baseNames.Add(entityTypeToReference.SimpleName);

            baseNames.Add("");

            foreach (var baseName in baseNames)
            {
                var match = TryFindMatchingClrProperties(foreignKey, baseName, onDependent);
                if (match != null)
                {
                    return match;
                }
            }
            
            return null;
        }

        private IReadOnlyList<Property> GetCompatiblePrimaryKeyProperties(EntityType entityType, IReadOnlyList<Property> propertiesToReference)
        {
            var dependentPkProperties = entityType.TryGetPrimaryKey()?.Properties;
            if (dependentPkProperties != null
                && Property.AreCompatible(propertiesToReference, dependentPkProperties))
            {
                return dependentPkProperties;
            }

            return null;
        }

        private IReadOnlyList<Property> TryFindMatchingClrProperties(ForeignKey foreignKey, string baseName, bool onDependent)
        {
            var entityType = onDependent
                ? foreignKey.EntityType
                : foreignKey.ReferencedEntityType;
            var propertiesToReference = onDependent
                ? foreignKey.ReferencedProperties
                : foreignKey.EntityType.TryGetPrimaryKey()?.Properties;

            if (propertiesToReference == null)
            {
                return null;
            }

            var foreignKeyProperties = new List<Property>();
            foreach (var referencedProperty in propertiesToReference)
            {
                var property = TryGetProperty(entityType, baseName + referencedProperty.Name, referencedProperty.UnderlyingType);

                if (property != null)
                {
                    foreignKeyProperties.Add(property);
                }
            }

            if (propertiesToReference.Count == 1
                && foreignKeyProperties.Count == 0)
            {
                var property = TryGetProperty(entityType, baseName + "Id", propertiesToReference.Single().UnderlyingType);

                if (property != null)
                {
                    foreignKeyProperties.Add(property);
                }
            }

            if (foreignKeyProperties.Count < propertiesToReference.Count)
            {
                return null;
            }

            if (foreignKey.IsRequired == false
                && foreignKeyProperties.All(p => !((IProperty)p).IsNullable))
            {
                return null;
            }

            var primaryKey = entityType.TryGetPrimaryKey();
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
            foreach (var property in entityType.Properties)
            {
                if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                        && !property.IsShadowProperty
                        && property.UnderlyingType == type)
                {
                    return property;
                }
            }
            return null;
        }
    }
}
