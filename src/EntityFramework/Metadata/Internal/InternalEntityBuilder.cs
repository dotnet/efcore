// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalEntityBuilder : InternalMetadataItemBuilder<EntityType>
    {
        public InternalEntityBuilder([NotNull] EntityType metadata, [NotNull] InternalModelBuilder modelBuilder)
            : base(metadata, modelBuilder)
        {
        }

        public virtual InternalKeyBuilder Key([NotNull] IReadOnlyList<string> propertyNames)
        {
            Check.NotNull(propertyNames, "propertyNames");

            return Key(GetExistingProperties(propertyNames));
        }

        public virtual InternalKeyBuilder Key([NotNull] IList<PropertyInfo> clrProperties)
        {
            Check.NotNull(clrProperties, "clrProperties");

            return Key(GetOrCreateProperties(clrProperties));
        }

        private InternalKeyBuilder Key(IEnumerable<Property> properties)
        {
            return new InternalKeyBuilder(Metadata.SetKey(properties.ToArray()), ModelBuilder);
        }

        public virtual InternalPropertyBuilder Property(
            [NotNull] Type propertyType, [NotNull] string name)
        {
            Check.NotNull(propertyType, "propertyType");
            Check.NotEmpty(name, "name");

            return new InternalPropertyBuilder(GetOrCreateProperty(propertyType, name, createShadowProperty: true), ModelBuilder);
        }

        public virtual InternalPropertyBuilder Property([NotNull] PropertyInfo clrProperty)
        {
            Check.NotNull(clrProperty, "clrProperty");

            return new InternalPropertyBuilder(GetOrCreateProperty(clrProperty.PropertyType, clrProperty.Name, createShadowProperty: false), ModelBuilder);
        }

        public virtual InternalForeignKeyBuilder ForeignKey(
            [NotNull] string referencedEntityTypeName, [NotNull] IReadOnlyList<string> propertyNames)
        {
            Check.NotEmpty(referencedEntityTypeName, "referencedEntityTypeName");
            Check.NotNull(propertyNames, "propertyNames");

            var principalType = ModelBuilder.Metadata.GetEntityType(referencedEntityTypeName);

            return ForeignKey(principalType, GetExistingProperties(propertyNames));
        }

        public virtual InternalForeignKeyBuilder ForeignKey([NotNull] Type referencedType, [NotNull] IList<PropertyInfo> clrProperties)
        {
            Check.NotNull(referencedType, "referencedType");
            Check.NotNull(clrProperties, "clrProperties");

            var principalType = ModelBuilder.GetOrAddEntity(referencedType).Metadata;

            return ForeignKey(principalType, GetOrCreateProperties(clrProperties));
        }

        private InternalForeignKeyBuilder ForeignKey(EntityType principalType, IEnumerable<Property> dependentProperties)
        {
            // TODO: This code currently assumes that the FK maps to a PK on the principal end
            var foreignKey = Metadata.AddForeignKey(principalType.GetKey(), dependentProperties.ToArray());

            return new InternalForeignKeyBuilder(foreignKey, ModelBuilder);
        }

        public virtual InternalIndexBuilder Index([NotNull] IReadOnlyList<string> propertyNames)
        {
            Check.NotNull(propertyNames, "propertyNames");

            return Index(GetExistingProperties(propertyNames));
        }

        public virtual InternalIndexBuilder Index([NotNull] IList<PropertyInfo> clrProperties)
        {
            Check.NotNull(clrProperties, "clrProperties");

            return Index(GetOrCreateProperties(clrProperties));
        }

        private InternalIndexBuilder Index(IEnumerable<Property> properties)
        {
            return new InternalIndexBuilder(Metadata.AddIndex(properties.ToArray()), ModelBuilder);
        }

        public virtual InternalRelationshipBuilder BuildRelationship(
            [NotNull] Type principalType, [NotNull] Type dependentType,
            [CanBeNull] string navNameToPrincipal, [CanBeNull] string navNameToDependent, bool oneToOne)
        {
            Check.NotNull(principalType, "principalType");
            Check.NotNull(dependentType, "dependentType");

            var dependentEntityType = ModelBuilder.GetOrAddEntity(dependentType).Metadata;
            var principalEntityType = ModelBuilder.GetOrAddEntity(principalType).Metadata;

            var navToDependent = principalEntityType.Navigations.FirstOrDefault(e => e.Name == navNameToDependent);
            var navToPrincipal = dependentEntityType.Navigations.FirstOrDefault(e => e.Name == navNameToPrincipal);

            // Find the associated FK on an already existing navigation, or create one by convention
            // TODO: If FK isn't already specified, then creating the navigation should cause it to be found/created
            // by convention, but this part of conventions is not done yet, so we do it here instead--kind of h.acky

            var foreignKey = navToDependent != null
                ? navToDependent.ForeignKey
                : navToPrincipal != null
                    ? navToPrincipal.ForeignKey
                    : new ForeignKeyConvention()
                        .FindOrCreateForeignKey(principalEntityType, dependentEntityType, navNameToPrincipal, navNameToDependent, oneToOne);

            if (navNameToDependent != null
                && navToDependent == null)
            {
                navToDependent = principalEntityType.AddNavigation(new Navigation(foreignKey, navNameToDependent, false));
            }

            if (navNameToPrincipal != null
                && navToPrincipal == null)
            {
                navToPrincipal = dependentEntityType.AddNavigation(new Navigation(foreignKey, navNameToPrincipal, true));
            }

            return new InternalRelationshipBuilder(
                foreignKey, ModelBuilder, principalEntityType, dependentEntityType, navToPrincipal, navToDependent);
        }

        private IEnumerable<Property> GetExistingProperties(IEnumerable<string> propertyNames)
        {
            return propertyNames.Select(n => Metadata.GetProperty(n));
        }

        private IEnumerable<Property> GetOrCreateProperties(IEnumerable<PropertyInfo> clrProperties)
        {
            return clrProperties.Select(p => GetOrCreateProperty(p.PropertyType, p.Name, false));
        }

        private Property GetOrCreateProperty(Type propertyType, string name, bool createShadowProperty)
        {
            return Metadata.TryGetProperty(name)
                   ?? Metadata.AddProperty(name, propertyType, createShadowProperty, concurrencyToken: false);
        }
    }
}
