// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalRelationshipBuilder : InternalMetadataItemBuilder<ForeignKey>
    {
        private EntityType _principalType;
        private EntityType _dependentType;
        private Navigation _navigationToPrincipal;
        private Navigation _navigationToDependent;
        private bool _areDependentPropertiesByConvention = true;
        private bool _arePrincipalPropertiesByConvention = true;

        public InternalRelationshipBuilder(
            [NotNull] ForeignKey foreignKey, [NotNull] InternalModelBuilder modelBuilder,
            [NotNull] EntityType principalType, [NotNull] EntityType dependentType,
            [CanBeNull] Navigation navigationToPrincipal, [CanBeNull] Navigation navigationToDependent)
            : base(foreignKey, modelBuilder)
        {
            Check.NotNull(principalType, "principalType");
            Check.NotNull(dependentType, "dependentType");

            _principalType = principalType;
            _dependentType = dependentType;
            _navigationToPrincipal = navigationToPrincipal;
            _navigationToDependent = navigationToDependent;
        }

        public InternalRelationshipBuilder([NotNull] InternalRelationshipBuilder currentBuilder,
            [NotNull] ForeignKey foreignKey,
            [CanBeNull] Navigation navigationToPrincipal,
            [CanBeNull] Navigation navigationToDependent)
            : this(foreignKey, currentBuilder.ModelBuilder, currentBuilder._principalType, currentBuilder._dependentType,
                navigationToPrincipal, navigationToDependent)
        {
            _areDependentPropertiesByConvention = currentBuilder._areDependentPropertiesByConvention;
            _arePrincipalPropertiesByConvention = currentBuilder._arePrincipalPropertiesByConvention;
        }

        public virtual EntityType PrincipalType
        {
            get { return _principalType; }
        }

        public virtual EntityType DependentType
        {
            get { return _dependentType; }
        }

        public Navigation NavigationToPrincipal
        {
            get { return _navigationToPrincipal; }
        }

        public Navigation NavigationToDependent
        {
            get { return _navigationToDependent; }
        }

        public virtual void Required(bool required)
        {
            Metadata.IsRequired = required;
        }

        public virtual InternalRelationshipBuilder Invert()
        {
            var navigationToDependent = NavigationToDependent;
            _navigationToDependent = NavigationToPrincipal;
            _navigationToPrincipal = navigationToDependent;

            var dependentType = _dependentType;
            _dependentType = _principalType;
            _principalType = dependentType;
            
            var arePrincipalPropertiesByConvention = _areDependentPropertiesByConvention;
            _areDependentPropertiesByConvention = _arePrincipalPropertiesByConvention;
            _arePrincipalPropertiesByConvention = arePrincipalPropertiesByConvention;

            if (NavigationToDependent != null)
            {
                NavigationToDependent.PointsToPrincipal = false;
            }

            if (NavigationToPrincipal != null)
            {
                NavigationToPrincipal.PointsToPrincipal = true;
            }

            return this;
        }

        public virtual InternalRelationshipBuilder ForeignKey([NotNull] IList<PropertyInfo> propertyAccessList)
        {
            Check.NotNull(propertyAccessList, "propertyAccessList");

            return ForeignKey(propertyAccessList.Select(p => _dependentType.GetOrAddProperty(p)));
        }

        public virtual InternalRelationshipBuilder ForeignKey([NotNull] IReadOnlyList<string> propertyNames)
        {
            Check.NotNull(propertyNames, "propertyNames");

            return ForeignKey(propertyNames.Select(p => _dependentType.GetProperty(p)));
        }

        public virtual InternalRelationshipBuilder ForeignKey([NotNull] IEnumerable<Property> properties)
        {
            Check.NotNull(properties, "properties");

            var dependentProperties = properties.ToList();
            _areDependentPropertiesByConvention = false;
            if (Metadata.Properties.SequenceEqual(dependentProperties))
            {
                return this;
            }

            return ReplaceForeignKey(dependentProperties: dependentProperties);
        }

        public virtual InternalRelationshipBuilder ReferencedKey([NotNull] IList<PropertyInfo> propertyAccessList)
        {
            Check.NotNull(propertyAccessList, "propertyAccessList");

            return ReferencedKey(propertyAccessList.Select(p => _principalType.GetOrAddProperty(p)));
        }

        public virtual InternalRelationshipBuilder ReferencedKey([NotNull] IReadOnlyList<string> propertyNames)
        {
            Check.NotNull(propertyNames, "propertyNames");

            return ReferencedKey(propertyNames.Select(p => _principalType.GetProperty(p)));
        }

        public virtual InternalRelationshipBuilder ReferencedKey([NotNull] IEnumerable<Property> properties)
        {
            Check.NotNull(properties, "properties");

            var principalProperties = properties.ToList();
            _arePrincipalPropertiesByConvention = false;
            if (Metadata.ReferencedProperties.SequenceEqual(principalProperties))
            {
                return this;
            }

            return ReplaceForeignKey(principalProperties: principalProperties);
        }

        public virtual InternalRelationshipBuilder OneToOneForeignKey(
            [NotNull] Type specifiedDependentType,
            [NotNull] IList<PropertyInfo> propertyAccessList)
        {
            Check.NotNull(specifiedDependentType, "specifiedDependentType");
            Check.NotNull(propertyAccessList, "propertyAccessList");

            return ForeignInvertIfNeeded(specifiedDependentType).ForeignKey(propertyAccessList);
        }

        public virtual InternalRelationshipBuilder OneToOneForeignKey(
            [NotNull] Type specifiedDependentType,
            [NotNull] IReadOnlyList<string> propertyNames)
        {
            Check.NotNull(specifiedDependentType, "specifiedDependentType");
            Check.NotNull(propertyNames, "propertyNames");

            return ForeignInvertIfNeeded(specifiedDependentType).ForeignKey(propertyNames);
        }

        public virtual InternalRelationshipBuilder OneToOneForeignKey(
            [NotNull] string specifiedDependentTypeName,
            [NotNull] IReadOnlyList<string> propertyNames)
        {
            Check.NotNull(specifiedDependentTypeName, "specifiedDependentTypeName");
            Check.NotNull(propertyNames, "propertyNames");

            return ForeignInvertIfNeeded(ModelBuilder.Metadata.GetEntityType(specifiedDependentTypeName)).ForeignKey(propertyNames);
        }

        private InternalRelationshipBuilder ForeignInvertIfNeeded(Type specifiedDependentType)
        {
            return ForeignInvertIfNeeded(ModelBuilder.Entity(specifiedDependentType).Metadata);
        }

        private InternalRelationshipBuilder ForeignInvertIfNeeded(EntityType entityType)
        {
            if (entityType != DependentType)
            {
                return Invert();
            }
            return this;
        }

        public virtual InternalRelationshipBuilder OneToOneReferencedKey(
            [NotNull] Type specifiedPrincipalType,
            [NotNull] IReadOnlyList<string> propertyNames)
        {
            Check.NotNull(specifiedPrincipalType, "specifiedPrincipalType");
            Check.NotNull(propertyNames, "propertyNames");

            return ReferenceInvertIfNeeded(specifiedPrincipalType).ReferencedKey(propertyNames);
        }

        public virtual InternalRelationshipBuilder OneToOneReferencedKey(
            [NotNull] string specifiedPrincipalTypeName,
            [NotNull] IReadOnlyList<string> propertyNames)
        {
            Check.NotNull(specifiedPrincipalTypeName, "specifiedPrincipalTypeName");
            Check.NotNull(propertyNames, "propertyNames");

            return ReferenceInvertIfNeeded(ModelBuilder.Metadata.GetEntityType(specifiedPrincipalTypeName)).ReferencedKey(propertyNames);
        }
        
        public virtual InternalRelationshipBuilder OneToOneReferencedKey(
            [NotNull] Type specifiedPrincipalType,
            [NotNull] IList<PropertyInfo> propertyAccessList)
        {
            Check.NotNull(specifiedPrincipalType, "specifiedPrincipalType");
            Check.NotNull(propertyAccessList, "propertyAccessList");

            return ReferenceInvertIfNeeded(specifiedPrincipalType).ReferencedKey(propertyAccessList);
        }

        private InternalRelationshipBuilder ReferenceInvertIfNeeded(Type specifiedPrincipalType)
        {
            return ReferenceInvertIfNeeded(ModelBuilder.Entity(specifiedPrincipalType).Metadata);
        }

        private InternalRelationshipBuilder ReferenceInvertIfNeeded(EntityType entityType)
        {
            return entityType == PrincipalType
                ? this
                : Invert().ReplaceForeignKey();
        }

        private InternalRelationshipBuilder ReplaceForeignKey(IReadOnlyList<Property> dependentProperties = null, IReadOnlyList<Property> principalProperties = null)
        {
            var inverted = Metadata.EntityType != DependentType;
            dependentProperties = dependentProperties ??
                                  (_areDependentPropertiesByConvention
                                      ? new Property[0]
                                      : inverted
                                          ? Metadata.ReferencedProperties
                                          : Metadata.Properties);

            principalProperties = principalProperties ??
                                  (_arePrincipalPropertiesByConvention
                                      ? new Property[0]
                                      : inverted
                                          ? Metadata.Properties
                                          : Metadata.ReferencedProperties);

            return ModelBuilder.Entity(Metadata.EntityType.Name)
                .ReplaceForeignKey(this, dependentProperties, principalProperties);
        }
    }
}
