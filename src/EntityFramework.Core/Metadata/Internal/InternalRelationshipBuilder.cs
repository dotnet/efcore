// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalRelationshipBuilder : InternalMetadataItemBuilder<ForeignKey>
    {
        private ConfigurationSource? _foreignKeyPropertiesConfigurationSource;
        private ConfigurationSource? _referencedKeyConfigurationSource;

        public InternalRelationshipBuilder(
            [NotNull] ForeignKey foreignKey,
            [NotNull] InternalModelBuilder modelBuilder,
            ConfigurationSource? foreignKeyConfigurationSource)
            : base(foreignKey, modelBuilder)
        {
            _foreignKeyPropertiesConfigurationSource = foreignKeyConfigurationSource;
            _referencedKeyConfigurationSource = foreignKeyConfigurationSource;
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
                return this;
            }

            if (Metadata.GetNavigationToDependent() != null)
            {
                return null;
            }

            return ReplaceForeignKey(
                Metadata.ReferencedEntityType,
                Metadata.EntityType,
                Metadata.Properties,
                Metadata.ReferencedProperties,
                Metadata.GetNavigationToPrincipal()?.Name,
                null,
                isUnique,
                configurationSource);
        }

        private InternalRelationshipBuilder Invert(ConfigurationSource configurationSource)
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
                null,
                null,
                Metadata.GetNavigationToDependent()?.Name,
                Metadata.GetNavigationToPrincipal()?.Name,
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
            IReadOnlyList<Property> dependentProperties = null, IReadOnlyList<Property> principalProperties = null)
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
                dependentProperties,
                principalProperties,
                Metadata.GetNavigationToPrincipal()?.Name,
                Metadata.GetNavigationToDependent()?.Name,
                ((IForeignKey)Metadata).IsUnique,
                configurationSource);
        }

        private InternalRelationshipBuilder ReplaceForeignKey(
            [NotNull] EntityType principalType,
            [NotNull] EntityType dependentType,
            [CanBeNull] IReadOnlyList<Property> foreignKeyProperties,
            [CanBeNull] IReadOnlyList<Property> referencedProperties,
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            bool oneToOne,
            ConfigurationSource configurationSource)
        {
            var entityTypeBuilder = ModelBuilder.Entity(Metadata.EntityType.Name, configurationSource);
            if (!entityTypeBuilder.RemoveRelationship(Metadata, configurationSource))
            {
                return null;
            }

            var dependentEntityTypeBuilder = ModelBuilder.Entity(dependentType.Name, configurationSource);
            var foreignKey = foreignKeyProperties == null
                ? null
                : new ForeignKeyConvention().TryGetForeignKey(
                    principalType,
                    dependentType,
                    null,
                    null,
                    foreignKeyProperties,
                    referencedProperties,
                    oneToOne);

            var existingForeignKey = foreignKey != null;
            if (!existingForeignKey)
            {
                if (foreignKeyProperties != null)
                {
                    foreignKey = dependentType.TryGetForeignKey(foreignKeyProperties);
                    if (foreignKey != null
                        && !dependentEntityTypeBuilder.RemoveRelationship(foreignKey, configurationSource))
                    {
                        return null;
                    }
                }

                foreignKey =
                    new ForeignKeyConvention().CreateForeignKeyByConvention(
                        principalType,
                        dependentType,
                        navigationToPrincipalName,
                        foreignKeyProperties,
                        referencedProperties,
                        oneToOne);

                if (foreignKey == null)
                {
                    if (configurationSource == ConfigurationSource.Explicit
                        && foreignKeyProperties != null
                        && foreignKeyProperties.Any())
                    {
                        if (referencedProperties == null
                            || !referencedProperties.Any())
                        {
                            referencedProperties = principalType.GetPrimaryKey().Properties;
                        }

                        if (referencedProperties.Count != foreignKeyProperties.Count)
                        {
                            throw new InvalidOperationException(
                                Strings.ForeignKeyCountMismatch(
                                    Property.Format(foreignKeyProperties),
                                    foreignKeyProperties[0].EntityType.Name,
                                    Property.Format(referencedProperties),
                                    principalType.Name));
                        }

                        if (!referencedProperties.Select(p => p.UnderlyingType).SequenceEqual(foreignKeyProperties.Select(p => p.UnderlyingType)))
                        {
                            throw new InvalidOperationException(
                                Strings.ForeignKeyTypeMismatch(
                                    Property.Format(foreignKeyProperties),
                                    foreignKeyProperties[0].EntityType.Name, principalType.Name));
                        }
                    }

                    return null;
                }
            }

            var navigationToPrincipalSet = dependentEntityTypeBuilder
                .Navigation(navigationToPrincipalName, foreignKey, pointsToPrincipal: true, configurationSource: configurationSource);
            Debug.Assert(navigationToPrincipalSet);

            var principalEntityTypeBuilder = ModelBuilder.Entity(foreignKey.ReferencedEntityType.Name, configurationSource);
            var navigationToDependentSet = principalEntityTypeBuilder
                .Navigation(navigationToDependentName, foreignKey, pointsToPrincipal: false, configurationSource: configurationSource);
            Debug.Assert(navigationToDependentSet);
            
            var builder = entityTypeBuilder.Relationship(foreignKey, existingForeignKey, configurationSource);
            Debug.Assert(builder != null);

            builder.MergeConfigurationSourceWith(this, foreignKeyProperties != null, referencedProperties != null, configurationSource);

            return builder;
        }

        private void MergeConfigurationSourceWith(
            InternalRelationshipBuilder builder,
            bool foreignKeyPropertiesConfigured,
            bool referencedPropertiesConfigured,
            ConfigurationSource configurationSource)
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

            targetForeignKeyPropertiesConfigurationSource = configurationSource.Max(targetForeignKeyPropertiesConfigurationSource);
            targetReferencedKeyConfigurationSource = configurationSource.Max(targetReferencedKeyConfigurationSource);

            if (foreignKeyPropertiesConfigured)
            {
                _foreignKeyPropertiesConfigurationSource = targetForeignKeyPropertiesConfigurationSource.Value.Max(_foreignKeyPropertiesConfigurationSource);
            }

            if (referencedPropertiesConfigured)
            {
                _referencedKeyConfigurationSource = targetReferencedKeyConfigurationSource.Value.Max(_referencedKeyConfigurationSource);
            }
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
