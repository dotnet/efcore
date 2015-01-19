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
                var foreignKeyProperties = GetCandidateForeignKeyProperties(relationshipBuilder.Metadata);

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

        private IReadOnlyList<Property> GetCandidateForeignKeyProperties(ForeignKey foreignKey)
        {
            var baseNames = new List<string>();
            var navigationToPrincipal = foreignKey.GetNavigationToPrincipal();
            if (navigationToPrincipal != null)
            {
                baseNames.Add(navigationToPrincipal.Name);
            }

            baseNames.Add(foreignKey.ReferencedEntityType.SimpleName);
            baseNames.Add("");

            foreach (var baseName in baseNames)
            {
                var match = TryFindMatchingClrProperties(foreignKey, baseName);
                if (match != null)
                {
                    return match;
                }
            }

            if (((IForeignKey)foreignKey).IsUnique)
            {
                var dependentPkProperties = foreignKey.EntityType.TryGetPrimaryKey()?.Properties;
                if (dependentPkProperties != null
                    && dependentPkProperties.Select(p => p.UnderlyingType)
                        .SequenceEqual(foreignKey.ReferencedKey.Properties.Select(p => p.UnderlyingType)))
                {
                    return dependentPkProperties;
                }
            }

            return null;
        }

        private IReadOnlyList<Property> TryFindMatchingClrProperties(ForeignKey foreignKey, string baseName)
        {
            var dependentType = foreignKey.EntityType;
            var referencedProperties = foreignKey.ReferencedProperties;
            var fkProperties = new List<Property>();
            foreach (var referencedProperty in referencedProperties)
            {
                var propertyName = baseName + referencedProperty.Name;
                var propertyFound = false;
                foreach (var property in dependentType.Properties)
                {
                    if (property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)
                        && !property.IsShadowProperty
                        && property.UnderlyingType == referencedProperty.UnderlyingType)
                    {
                        propertyFound = true;
                        fkProperties.Add(property);
                        break;
                    }
                }

                if (!propertyFound)
                {
                    return null;
                }
            }

            var dependentPk = dependentType.TryGetPrimaryKey();
            if (dependentPk != null)
            {
                if (fkProperties.All(property => dependentPk.Properties.Contains(property)))
                {
                    return null;
                }
            }

            return fkProperties;
        }
    }
}
