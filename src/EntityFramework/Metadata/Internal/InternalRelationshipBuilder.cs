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
    public class InternalRelationshipBuilder : InternalMetadataItemBuilder<ForeignKey>
    {
        private EntityType _principalType;
        private EntityType _dependentType;
        private Navigation _navigationToPrincipal;
        private Navigation _navigationToDependent;

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

        public virtual EntityType PrincipalType
        {
            get { return _principalType; }
        }

        public virtual EntityType DependentType
        {
            get { return _dependentType; }
        }

        public virtual InternalRelationshipBuilder Invert()
        {
            var navigationToDependent = _navigationToDependent;
            _navigationToDependent = _navigationToPrincipal;
            _navigationToPrincipal = navigationToDependent;

            var dependentType = _dependentType;
            _dependentType = _principalType;
            _principalType = dependentType;

            if (_navigationToDependent != null)
            {
                _navigationToDependent.PointsToPrincipal = false;
            }

            if (_navigationToPrincipal != null)
            {
                _navigationToPrincipal.PointsToPrincipal = true;
            }

            return this;
        }

        public virtual InternalRelationshipBuilder ForeignKey([NotNull] IList<PropertyInfo> propertyAccessList)
        {
            Check.NotNull(propertyAccessList, "propertyAccessList");

            var dependentProperties = propertyAccessList
                .Select(pi => _dependentType.GetOrAddProperty(pi))
                .ToArray();

            if (Metadata.Properties.SequenceEqual(dependentProperties))
            {
                return this;
            }

            var newForeignKey = new ForeignKeyConvention().FindOrCreateForeignKey(
                _principalType,
                _dependentType,
                _navigationToPrincipal != null ? _navigationToPrincipal.Name : null,
                _navigationToDependent != null ? _navigationToDependent.Name : null,
                dependentProperties.Any() ? new[] { dependentProperties } : new Property[0][],
                Metadata.EntityType == _dependentType ? Metadata.ReferencedProperties : new Property[0],
                Metadata.IsUnique);

            ReplaceForeignKey(newForeignKey, Metadata.Properties.Except(dependentProperties));

            return new InternalRelationshipBuilder(
                newForeignKey, ModelBuilder, _principalType, _dependentType, _navigationToPrincipal, _navigationToDependent);
        }

        public virtual InternalRelationshipBuilder ReferencedKey([NotNull] IList<PropertyInfo> propertyAccessList)
        {
            Check.NotNull(propertyAccessList, "propertyAccessList");

            var principalProperties = propertyAccessList
                .Select(pi => _principalType.GetOrAddProperty(pi))
                .ToArray();

            if (Metadata.ReferencedProperties.SequenceEqual(principalProperties))
            {
                return this;
            }

            var newForeignKey = _dependentType.AddForeignKey(new ForeignKey(new Key(principalProperties), Metadata.Properties.ToArray()));
            newForeignKey.IsUnique = Metadata.IsUnique;

            ReplaceForeignKey(newForeignKey, Enumerable.Empty<Property>());

            return new InternalRelationshipBuilder(
                newForeignKey, ModelBuilder, _principalType, _dependentType, _navigationToPrincipal, _navigationToDependent);
        }

        public virtual InternalRelationshipBuilder OneToOneForeignKey(
            [NotNull] Type specifiedDepdnentType,
            [NotNull] IList<PropertyInfo> propertyAccessList)
        {
            Check.NotNull(specifiedDepdnentType, "specifiedDepdnentType");
            Check.NotNull(propertyAccessList, "propertyAccessList");

            if (ModelBuilder.GetOrAddEntity(specifiedDepdnentType).Metadata != DependentType)
            {
                Invert();
            }

            return ForeignKey(propertyAccessList);
        }

        public virtual InternalRelationshipBuilder OneToOneReferencedKey(
            [NotNull] Type specifiedPrincipalType,
            [NotNull] IList<PropertyInfo> propertyAccessList)
        {
            Check.NotNull(specifiedPrincipalType, "specifiedPrincipalType");
            Check.NotNull(propertyAccessList, "propertyAccessList");

            var builder = ModelBuilder.GetOrAddEntity(specifiedPrincipalType).Metadata == PrincipalType
                ? this
                : Invert().ForeignKey(new PropertyInfo[0]);

            return builder.ReferencedKey(propertyAccessList);
        }

        private void ReplaceForeignKey(ForeignKey newForeignKey, IEnumerable<Property> propertiesToRemove)
        {
            if (_navigationToPrincipal != null)
            {
                _dependentType.RemoveNavigation(_navigationToPrincipal);
            }

            if (_navigationToDependent != null)
            {
                _principalType.RemoveNavigation(_navigationToDependent);
            }

            // TODO: Remove FK only if it was added by convention
            Metadata.EntityType.RemoveForeignKey(Metadata);

            // TODO: Remove property only if it was added by convention
            foreach (var property in propertiesToRemove)
            {
                // TODO: This check not needed once only removing properties added by convention
                var dependentPk = Metadata.EntityType.TryGetPrimaryKey();
                if (dependentPk == null
                    || !dependentPk.Properties.Contains(property))
                {
                    Metadata.EntityType.RemoveProperty(property);
                }
            }

            if (_navigationToPrincipal != null)
            {
                _navigationToPrincipal.ForeignKey = newForeignKey;
                _dependentType.AddNavigation(_navigationToPrincipal);
            }

            if (_navigationToDependent != null)
            {
                _navigationToDependent.ForeignKey = newForeignKey;
                _principalType.AddNavigation(_navigationToDependent);
            }
        }
    }
}
