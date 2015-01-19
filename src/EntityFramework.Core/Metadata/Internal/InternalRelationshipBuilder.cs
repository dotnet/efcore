// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalRelationshipBuilder : InternalMetadataItemBuilder<ForeignKey>
    {
        private ConfigurationSource? _foreignKeyPropertiesConfigurationSource;
        private ConfigurationSource? _referencedKeyConfigurationSource;
        private ConfigurationSource? _isUniqueConfigurationSource;

        public InternalRelationshipBuilder(
            [NotNull] ForeignKey foreignKey,
            [NotNull] InternalModelBuilder modelBuilder,
            ConfigurationSource? initialConfigurationSource)
            : base(foreignKey, modelBuilder)
        {
            Check.NotNull(foreignKey, "foreignKey");
            Check.NotNull(modelBuilder, "modelBuilder");

            _foreignKeyPropertiesConfigurationSource = initialConfigurationSource;
            _referencedKeyConfigurationSource = initialConfigurationSource;
            _isUniqueConfigurationSource = initialConfigurationSource;
        }

        public virtual InternalRelationshipBuilder NavigationToPrincipal(
            [CanBeNull] string navigationToPrincipalName,
            ConfigurationSource configurationSource,
            bool? strictPreferExisting = null)
        {
            var dependentEntityType = ModelBuilder.Entity(Metadata.EntityType.Name, configurationSource);

            if (strictPreferExisting.HasValue)
            {
                var navigationToPrincipal = string.IsNullOrEmpty(navigationToPrincipalName)
                    ? null
                    : Metadata.EntityType.TryGetNavigation(navigationToPrincipalName);

                if (navigationToPrincipal != null
                    && navigationToPrincipal.IsCompatible(
                        Metadata.ReferencedEntityType,
                        Metadata.EntityType,
                        strictPreferExisting.Value ? (bool?)true : null,
                        Metadata.IsUnique))
                {
                    var navigationToDependentName = Metadata.GetNavigationToDependent()?.Name ?? "";

                    if (Metadata == navigationToPrincipal.ForeignKey
                        || dependentEntityType.RemoveRelationship(Metadata, configurationSource).HasValue)
                    {
                        return dependentEntityType.Relationship(
                            navigationToPrincipal,
                            configurationSource,
                            navigationToDependentName);
                    }
                }
            }

            return dependentEntityType
                .Navigation(navigationToPrincipalName, Metadata, pointsToPrincipal: true, configurationSource: configurationSource)
                ? this
                : null;
        }

        public virtual InternalRelationshipBuilder NavigationToDependent(
            [CanBeNull] string navigationToDependentName,
            ConfigurationSource configurationSource,
            bool? strictPreferExisting = null)
        {
            var principalEntityType = ModelBuilder.Entity(Metadata.ReferencedEntityType.Name, configurationSource);

            if (strictPreferExisting.HasValue)
            {
                var navigationToDependent = navigationToDependentName == null
                    ? null
                    : Metadata.ReferencedEntityType.TryGetNavigation(navigationToDependentName);

                if (navigationToDependent != null
                    && navigationToDependent.IsCompatible(
                        Metadata.ReferencedEntityType,
                        Metadata.EntityType,
                        strictPreferExisting.Value ? (bool?)false : null,
                        Metadata.IsUnique))
                {
                    var navigationToPrincipalName = Metadata.GetNavigationToPrincipal()?.Name ?? "";

                    if (Metadata == navigationToDependent.ForeignKey
                        || principalEntityType.RemoveRelationship(Metadata, configurationSource).HasValue)
                    {
                        return principalEntityType.Relationship(
                            navigationToDependent,
                            configurationSource,
                            navigationToPrincipalName);
                    }
                }
            }

            return principalEntityType
                .Navigation(navigationToDependentName, Metadata, pointsToPrincipal: false, configurationSource: configurationSource)
                ? this
                : null;
        }

        public virtual bool Required(bool isRequired, ConfigurationSource configurationSource)
        {
            var entityTypeBuilder = ModelBuilder.Entity(Metadata.EntityType.Name, configurationSource);

            var properties = Metadata.Properties;
            if (!isRequired)
            {
                var nullableTypeProperties = Metadata.Properties.Where(p => p.PropertyType.IsNullableType()).ToList();
                if (nullableTypeProperties.Any())
                {
                    properties = nullableTypeProperties;
                }
            }

            foreach (var property in properties)
            {
                if (!entityTypeBuilder.Property(property.PropertyType, property.Name, configurationSource)
                    .CanSetRequired(isRequired, configurationSource))
                {
                    return false;
                }
            }

            foreach (var property in properties)
            {
                // TODO: Depending on resolution of #723 this may change
                entityTypeBuilder.Property(property.PropertyType, property.Name, configurationSource).Required(isRequired, configurationSource);
            }

            return true;
        }

        public virtual InternalRelationshipBuilder Unique(bool isUnique, ConfigurationSource configurationSource)
        {
            if (((IForeignKey)Metadata).IsUnique == isUnique)
            {
                Metadata.IsUnique = isUnique;
                _isUniqueConfigurationSource = configurationSource.Max(_isUniqueConfigurationSource);
                return this;
            }

            if (Metadata.GetNavigationToDependent() != null)
            {
                // TODO: throw for explicit
                return null;
            }

            if (_isUniqueConfigurationSource == null
                || configurationSource.Overrides(_isUniqueConfigurationSource.Value))
            {
                _isUniqueConfigurationSource = configurationSource.Max(_isUniqueConfigurationSource);
                return ReplaceForeignKey(configurationSource, isUnique: isUnique);
            }

            return null;
        }

        public virtual InternalRelationshipBuilder Invert(ConfigurationSource configurationSource)
        {
            if (!((IForeignKey)Metadata).IsUnique)
            {
                return null;
            }

            if ((_foreignKeyPropertiesConfigurationSource != null && _foreignKeyPropertiesConfigurationSource.Value.Overrides(configurationSource))
                || (_referencedKeyConfigurationSource != null && _referencedKeyConfigurationSource.Value.Overrides(configurationSource)))
            {
                if (configurationSource == ConfigurationSource.Explicit)
                {
                    throw new InvalidOperationException(Strings.RelationshipCannotBeInverted);
                }
                return null;
            }

            _foreignKeyPropertiesConfigurationSource = null;
            _referencedKeyConfigurationSource = null;

            return ReplaceForeignKey(
                Metadata.EntityType,
                Metadata.ReferencedEntityType,
                Metadata.GetNavigationToDependent()?.Name,
                Metadata.GetNavigationToPrincipal()?.Name,
                null,
                null,
                ((IForeignKey)Metadata).IsUnique,
                configurationSource);
        }

        public virtual InternalRelationshipBuilder ForeignKey([NotNull] IReadOnlyList<PropertyInfo> properties,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(properties, "properties");

            return ForeignKey(
                ModelBuilder.Entity(Metadata.EntityType.Name, configurationSource)
                    .GetOrCreateProperties(properties, configurationSource),
                configurationSource);
        }

        public virtual InternalRelationshipBuilder ForeignKey([NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(propertyNames, "propertyNames");

            return ForeignKey(
                ModelBuilder.Entity(Metadata.EntityType.Name, configurationSource)
                    .GetOrCreateProperties(propertyNames, configurationSource),
                configurationSource);
        }

        public virtual InternalRelationshipBuilder ForeignKey([NotNull] IReadOnlyList<Property> properties,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(properties, "properties");

            if (Metadata.Properties.SequenceEqual(properties))
            {
                _foreignKeyPropertiesConfigurationSource = configurationSource.Max(_foreignKeyPropertiesConfigurationSource);
                return this;
            }

            if (_foreignKeyPropertiesConfigurationSource != null
                && !configurationSource.Overrides(_foreignKeyPropertiesConfigurationSource.Value))
            {
                return null;
            }

            _foreignKeyPropertiesConfigurationSource = configurationSource.Max(_foreignKeyPropertiesConfigurationSource);
            return ReplaceForeignKey(configurationSource, dependentProperties: properties);
        }

        public virtual InternalRelationshipBuilder ForeignKey(
            [NotNull] Type specifiedDependentType,
            [NotNull] IReadOnlyList<PropertyInfo> properties,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(specifiedDependentType, "specifiedDependentType");
            Check.NotNull(properties, "properties");

            return ForeignInvertIfNeeded(ResolveType(specifiedDependentType), configurationSource)
                .ForeignKey(properties, configurationSource);
        }

        public virtual InternalRelationshipBuilder ForeignKey(
            [NotNull] Type specifiedDependentType,
            [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(specifiedDependentType, "specifiedDependentType");
            Check.NotNull(propertyNames, "propertyNames");

            return ForeignInvertIfNeeded(ResolveType(specifiedDependentType), configurationSource)
                .ForeignKey(propertyNames, configurationSource);
        }

        public virtual InternalRelationshipBuilder ForeignKey(
            [NotNull] string specifiedDependentTypeName,
            [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(specifiedDependentTypeName, "specifiedDependentTypeName");
            Check.NotNull(propertyNames, "propertyNames");

            return ForeignInvertIfNeeded(ResolveType(specifiedDependentTypeName), configurationSource)
                .ForeignKey(propertyNames, configurationSource);
        }

        private InternalRelationshipBuilder ForeignInvertIfNeeded(EntityType entityType, ConfigurationSource configurationSource)
        {
            return entityType == Metadata.EntityType
                ? this
                : Invert(configurationSource);
        }

        public virtual InternalRelationshipBuilder ReferencedKey([NotNull] IReadOnlyList<PropertyInfo> properties,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(properties, "properties");

            return ReferencedKey(
                ModelBuilder.Entity(Metadata.ReferencedEntityType.Name, configurationSource)
                    .GetOrCreateProperties(properties, configurationSource),
                configurationSource);
        }

        public virtual InternalRelationshipBuilder ReferencedKey([NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(propertyNames, "propertyNames");

            return ReferencedKey(
                ModelBuilder.Entity(Metadata.ReferencedEntityType.Name, configurationSource)
                    .GetOrCreateProperties(propertyNames, configurationSource),
                configurationSource);
        }

        public virtual InternalRelationshipBuilder ReferencedKey([NotNull] IReadOnlyList<Property> properties,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(properties, "properties");

            if (Metadata.ReferencedProperties.SequenceEqual(properties))
            {
                _referencedKeyConfigurationSource = configurationSource.Max(_referencedKeyConfigurationSource);
                return this;
            }

            if (_referencedKeyConfigurationSource != null
                && !configurationSource.Overrides(_referencedKeyConfigurationSource.Value))
            {
                return null;
            }

            _referencedKeyConfigurationSource = configurationSource.Max(_referencedKeyConfigurationSource);
            return ReplaceForeignKey(configurationSource, principalProperties: properties);
        }

        public virtual InternalRelationshipBuilder ReferencedKey(
            [NotNull] Type specifiedPrincipalType,
            [NotNull] IReadOnlyList<PropertyInfo> properties,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(specifiedPrincipalType, "specifiedPrincipalType");
            Check.NotNull(properties, "properties");

            return ReferenceInvertIfNeeded(ResolveType(specifiedPrincipalType), configurationSource)
                .ReferencedKey(properties, configurationSource);
        }

        public virtual InternalRelationshipBuilder ReferencedKey(
            [NotNull] Type specifiedPrincipalType,
            [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(specifiedPrincipalType, "specifiedPrincipalType");
            Check.NotNull(propertyNames, "propertyNames");

            return ReferenceInvertIfNeeded(ResolveType(specifiedPrincipalType), configurationSource)
                .ReferencedKey(propertyNames, configurationSource);
        }

        public virtual InternalRelationshipBuilder ReferencedKey(
            [NotNull] string specifiedPrincipalTypeName,
            [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(specifiedPrincipalTypeName, "specifiedPrincipalTypeName");
            Check.NotNull(propertyNames, "propertyNames");

            return ReferenceInvertIfNeeded(ResolveType(specifiedPrincipalTypeName), configurationSource)
                .ReferencedKey(propertyNames, configurationSource);
        }

        private InternalRelationshipBuilder ReferenceInvertIfNeeded(EntityType entityType, ConfigurationSource configurationSource)
        {
            return entityType == Metadata.ReferencedEntityType
                ? this
                : Invert(configurationSource);
        }

        private InternalRelationshipBuilder ReplaceForeignKey(ConfigurationSource configurationSource,
            IReadOnlyList<Property> dependentProperties = null,
            IReadOnlyList<Property> principalProperties = null,
            bool? isUnique = null)
        {
            dependentProperties = dependentProperties ??
                                  (_foreignKeyPropertiesConfigurationSource.HasValue
                                      ? Metadata.Properties
                                      : null);

            principalProperties = principalProperties ??
                                  (_referencedKeyConfigurationSource.HasValue
                                      ? Metadata.ReferencedProperties
                                      : null);

            return ReplaceForeignKey(
                Metadata.ReferencedEntityType,
                Metadata.EntityType,
                Metadata.GetNavigationToPrincipal()?.Name,
                Metadata.GetNavigationToDependent()?.Name,
                dependentProperties,
                principalProperties,
                isUnique ?? ((IForeignKey)Metadata).IsUnique,
                configurationSource);
        }

        private InternalRelationshipBuilder ReplaceForeignKey(
            [NotNull] EntityType principalType,
            [NotNull] EntityType dependentType,
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            [CanBeNull] IReadOnlyList<Property> foreignKeyProperties,
            [CanBeNull] IReadOnlyList<Property> referencedProperties,
            bool oneToOne,
            ConfigurationSource configurationSource)
        {
            var entityTypeBuilder = ModelBuilder.Entity(Metadata.EntityType.Name, configurationSource);
            var replacedConfigurationSource = entityTypeBuilder.RemoveRelationship(Metadata, ConfigurationSource.Explicit);
            if (!replacedConfigurationSource.HasValue)
            {
                return null;
            }

            var principalEntityTypeBuilder = ModelBuilder.Entity(principalType.Name, configurationSource);
            var dependentEntityTypeBuilder = ModelBuilder.Entity(dependentType.Name, configurationSource);

            return dependentEntityTypeBuilder.Relationship(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                navigationToPrincipalName,
                navigationToDependentName,
                foreignKeyProperties,
                referencedProperties,
                configurationSource,
                oneToOne,
                b => dependentEntityTypeBuilder
                    .Relationship(b.Metadata, existingForeignKey: true, configurationSource: replacedConfigurationSource.Value)
                    .MergeConfigurationSourceWith(this));
        }

        private InternalRelationshipBuilder MergeConfigurationSourceWith(InternalRelationshipBuilder builder)
        {
            var inverted = builder.Metadata.EntityType != Metadata.EntityType;
            Debug.Assert(inverted
                         || (builder.Metadata.EntityType == Metadata.EntityType && builder.Metadata.ReferencedEntityType == Metadata.ReferencedEntityType));
            Debug.Assert(!inverted
                         || (builder.Metadata.EntityType == Metadata.ReferencedEntityType && builder.Metadata.ReferencedEntityType == Metadata.EntityType));

            var targetForeignKeyPropertiesConfigurationSource = inverted
                ? builder._referencedKeyConfigurationSource
                : builder._foreignKeyPropertiesConfigurationSource;
            var targetReferencedKeyConfigurationSource = inverted
                ? builder._foreignKeyPropertiesConfigurationSource
                : builder._referencedKeyConfigurationSource;

            _foreignKeyPropertiesConfigurationSource =
                targetForeignKeyPropertiesConfigurationSource?.Max(_foreignKeyPropertiesConfigurationSource) ?? _foreignKeyPropertiesConfigurationSource;

            _referencedKeyConfigurationSource =
                targetReferencedKeyConfigurationSource?.Max(_referencedKeyConfigurationSource) ?? _referencedKeyConfigurationSource;

            _isUniqueConfigurationSource = builder._isUniqueConfigurationSource?.Max(_isUniqueConfigurationSource) ?? _isUniqueConfigurationSource;

            return this;
        }

        private EntityType ResolveType(Type type)
        {
            if (type == Metadata.EntityType.Type)
            {
                return Metadata.EntityType;
            }

            if (type == Metadata.ReferencedEntityType.Type)
            {
                return Metadata.ReferencedEntityType;
            }

            throw new ArgumentException(Strings.EntityTypeNotInRelationship(type.FullName, Metadata.EntityType.Name, Metadata.ReferencedEntityType.Name));
        }

        private EntityType ResolveType(string name)
        {
            if (name == Metadata.EntityType.Name)
            {
                return Metadata.EntityType;
            }

            if (name == Metadata.ReferencedEntityType.Name)
            {
                return Metadata.ReferencedEntityType;
            }

            throw new ArgumentException(Strings.EntityTypeNotInRelationship(name, Metadata.EntityType.Name, Metadata.ReferencedEntityType.Name));
        }
    }
}
