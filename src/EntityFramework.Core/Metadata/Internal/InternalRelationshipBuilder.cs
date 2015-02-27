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
        private ConfigurationSource? _isRequiredConfigurationSource;

        public InternalRelationshipBuilder(
            [NotNull] ForeignKey foreignKey,
            [NotNull] InternalModelBuilder modelBuilder,
            ConfigurationSource? initialConfigurationSource)
            : base(foreignKey, modelBuilder)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            _foreignKeyPropertiesConfigurationSource = initialConfigurationSource;
            _referencedKeyConfigurationSource = initialConfigurationSource;
            _isUniqueConfigurationSource = initialConfigurationSource;
            _isRequiredConfigurationSource = initialConfigurationSource;
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

            var hasChanged = navigationToPrincipalName != null &&
                             Metadata.GetNavigationToPrincipal()?.Name != navigationToPrincipalName;

            if (Metadata.EntityType == Metadata.ReferencedEntityType
                && navigationToPrincipalName != null
                && navigationToPrincipalName == Metadata.GetNavigationToDependent()?.Name)
            {
                throw new InvalidOperationException(Strings.NavigationToSelfDuplicate(navigationToPrincipalName));
            }

            return dependentEntityType
                .Navigation(navigationToPrincipalName, Metadata, pointsToPrincipal: true, configurationSource: configurationSource)
                ? hasChanged ? ReplaceForeignKey(configurationSource) : this
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

            var hasChanged = navigationToDependentName != null &&
                             Metadata.GetNavigationToDependent()?.Name != navigationToDependentName;

            if (Metadata.EntityType == Metadata.ReferencedEntityType
                && navigationToDependentName != null
                && navigationToDependentName == Metadata.GetNavigationToPrincipal()?.Name)
            {
                throw new InvalidOperationException(Strings.NavigationToSelfDuplicate(navigationToDependentName));
            }

            return principalEntityType
                .Navigation(navigationToDependentName, Metadata, pointsToPrincipal: false, configurationSource: configurationSource)
                ? hasChanged ? ReplaceForeignKey(configurationSource) : this
                : null;
        }

        public virtual InternalRelationshipBuilder Required(bool isRequired, ConfigurationSource configurationSource)
        {
            if (((IForeignKey)Metadata).IsRequired == isRequired)
            {
                Metadata.IsRequired = isRequired;
                _isRequiredConfigurationSource = configurationSource.Max(_isRequiredConfigurationSource);
                return this;
            }

            if (_isRequiredConfigurationSource != null
                && !configurationSource.Overrides(_isRequiredConfigurationSource.Value))
            {
                return null;
            }

            _isRequiredConfigurationSource = configurationSource.Max(_isRequiredConfigurationSource);
            return ReplaceForeignKey(configurationSource, isRequired: isRequired);
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

            if (_isUniqueConfigurationSource != null
                && !configurationSource.Overrides(_isUniqueConfigurationSource.Value))
            {
                return null;
            }

            _isUniqueConfigurationSource = configurationSource.Max(_isUniqueConfigurationSource);
            return ReplaceForeignKey(configurationSource, isUnique: isUnique);
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
                _isRequiredConfigurationSource.HasValue ? ((IForeignKey)Metadata).IsRequired : (bool?)null,
                configurationSource);
        }

        public virtual InternalRelationshipBuilder ForeignKey([NotNull] IReadOnlyList<PropertyInfo> properties,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(properties, nameof(properties));

            return ForeignKey(
                ModelBuilder.Entity(Metadata.EntityType.Name, configurationSource)
                    .GetOrCreateProperties(properties, configurationSource),
                configurationSource);
        }

        public virtual InternalRelationshipBuilder ForeignKey([NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(propertyNames, nameof(propertyNames));

            return ForeignKey(
                ModelBuilder.Entity(Metadata.EntityType.Name, configurationSource)
                    .GetOrCreateProperties(propertyNames, configurationSource),
                configurationSource);
        }

        public virtual InternalRelationshipBuilder ForeignKey([NotNull] IReadOnlyList<Property> properties,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(properties, nameof(properties));

            if (Metadata.Properties.SequenceEqual(properties))
            {
                _foreignKeyPropertiesConfigurationSource = configurationSource.Max(_foreignKeyPropertiesConfigurationSource);

                ModelBuilder.Entity(Metadata.EntityType.Name, configurationSource)
                    .GetOrCreateProperties(properties.Select(p => p.Name), configurationSource);
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
            Check.NotNull(specifiedDependentType, nameof(specifiedDependentType));
            Check.NotNull(properties, nameof(properties));

            return ForeignInvertIfNeeded(ResolveType(specifiedDependentType), configurationSource)
                .ForeignKey(properties, configurationSource);
        }

        public virtual InternalRelationshipBuilder ForeignKey(
            [NotNull] Type specifiedDependentType,
            [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(specifiedDependentType, nameof(specifiedDependentType));
            Check.NotNull(propertyNames, nameof(propertyNames));

            return ForeignInvertIfNeeded(ResolveType(specifiedDependentType), configurationSource)
                .ForeignKey(propertyNames, configurationSource);
        }

        public virtual InternalRelationshipBuilder ForeignKey(
            [NotNull] string specifiedDependentTypeName,
            [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(specifiedDependentTypeName, nameof(specifiedDependentTypeName));
            Check.NotNull(propertyNames, nameof(propertyNames));

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
            Check.NotNull(properties, nameof(properties));

            return ReferencedKey(
                ModelBuilder.Entity(Metadata.ReferencedEntityType.Name, configurationSource)
                    .GetOrCreateProperties(properties, configurationSource),
                configurationSource);
        }

        public virtual InternalRelationshipBuilder ReferencedKey([NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(propertyNames, nameof(propertyNames));

            return ReferencedKey(
                ModelBuilder.Entity(Metadata.ReferencedEntityType.Name, configurationSource)
                    .GetOrCreateProperties(propertyNames, configurationSource),
                configurationSource);
        }

        public virtual InternalRelationshipBuilder ReferencedKey([NotNull] IReadOnlyList<Property> properties,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(properties, nameof(properties));

            if (Metadata.ReferencedProperties.SequenceEqual(properties))
            {
                var principalEntityTypeBuilder = ModelBuilder.Entity(Metadata.ReferencedEntityType.Name, configurationSource);
                principalEntityTypeBuilder.Key(properties, configurationSource);
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
            Check.NotNull(specifiedPrincipalType, nameof(specifiedPrincipalType));
            Check.NotNull(properties, nameof(properties));

            return ReferenceInvertIfNeeded(ResolveType(specifiedPrincipalType), configurationSource)
                .ReferencedKey(properties, configurationSource);
        }

        public virtual InternalRelationshipBuilder ReferencedKey(
            [NotNull] Type specifiedPrincipalType,
            [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(specifiedPrincipalType, nameof(specifiedPrincipalType));
            Check.NotNull(propertyNames, nameof(propertyNames));

            return ReferenceInvertIfNeeded(ResolveType(specifiedPrincipalType), configurationSource)
                .ReferencedKey(propertyNames, configurationSource);
        }

        public virtual InternalRelationshipBuilder ReferencedKey(
            [NotNull] string specifiedPrincipalTypeName,
            [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(specifiedPrincipalTypeName, nameof(specifiedPrincipalTypeName));
            Check.NotNull(propertyNames, nameof(propertyNames));

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
            bool? isUnique = null,
            bool? isRequired = null)
        {
            dependentProperties = dependentProperties ??
                                  (_foreignKeyPropertiesConfigurationSource.HasValue
                                   && _foreignKeyPropertiesConfigurationSource.Value.Overrides(configurationSource)
                                      ? Metadata.Properties
                                      : null);

            principalProperties = principalProperties ??
                                  (_referencedKeyConfigurationSource.HasValue
                                   && _referencedKeyConfigurationSource.Value.Overrides(configurationSource)
                                      ? Metadata.ReferencedProperties
                                      : null);

            var navigationToDependentName = Metadata.GetNavigationToDependent()?.Name;

            isUnique = isUnique ??
                       (navigationToDependentName != null
                           ? ((IForeignKey)Metadata).IsUnique
                           : (bool?)null);

            isUnique = isUnique ??
                       (_isUniqueConfigurationSource.HasValue
                        && _isUniqueConfigurationSource.Value.Overrides(configurationSource)
                           ? Metadata.IsUnique
                           : null);

            isRequired = isRequired ??
                         (_isRequiredConfigurationSource.HasValue
                          && _isRequiredConfigurationSource.Value.Overrides(configurationSource)
                             ? Metadata.IsRequired
                             : null);

            return ReplaceForeignKey(
                Metadata.ReferencedEntityType,
                Metadata.EntityType,
                Metadata.GetNavigationToPrincipal()?.Name,
                navigationToDependentName,
                dependentProperties,
                principalProperties,
                isUnique,
                isRequired,
                configurationSource);
        }

        private InternalRelationshipBuilder ReplaceForeignKey(
            [NotNull] EntityType principalType,
            [NotNull] EntityType dependentType,
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            [CanBeNull] IReadOnlyList<Property> foreignKeyProperties,
            [CanBeNull] IReadOnlyList<Property> referencedProperties,
            bool? isUnique,
            bool? isRequired,
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
                isUnique,
                isRequired,
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
            _isRequiredConfigurationSource = builder._isRequiredConfigurationSource?.Max(_isRequiredConfigurationSource) ?? _isRequiredConfigurationSource;

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
