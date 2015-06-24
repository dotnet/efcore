// Copyright (c) .NET Foundation. All rights reserved.
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
        private ConfigurationSource? _principalKeyConfigurationSource;
        private ConfigurationSource? _isUniqueConfigurationSource;
        private ConfigurationSource? _isRequiredConfigurationSource;
        private ConfigurationSource? _deletebehaviorConfigurationSource;
        private ConfigurationSource? _principalEndConfigurationSource;

        public InternalRelationshipBuilder(
            [NotNull] ForeignKey foreignKey,
            [NotNull] InternalModelBuilder modelBuilder,
            ConfigurationSource? initialConfigurationSource)
            : base(foreignKey, modelBuilder)
        {
            _foreignKeyPropertiesConfigurationSource = initialConfigurationSource;
            _principalKeyConfigurationSource = initialConfigurationSource;
            _isUniqueConfigurationSource = initialConfigurationSource;
            _isRequiredConfigurationSource = initialConfigurationSource;
            _deletebehaviorConfigurationSource = initialConfigurationSource;
            _principalEndConfigurationSource = initialConfigurationSource;
        }

        public virtual InternalRelationshipBuilder DependentToPrincipal(
            [CanBeNull] string navigationToPrincipalName,
            ConfigurationSource configurationSource,
            bool? strictPreferExisting = null)
        {
            var dependentEntityType = ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, configurationSource);

            if (strictPreferExisting.HasValue)
            {
                var navigationToPrincipal = string.IsNullOrEmpty(navigationToPrincipalName)
                    ? null
                    : Metadata.DeclaringEntityType.FindDeclaredNavigation(navigationToPrincipalName);

                if (navigationToPrincipal != null
                    && navigationToPrincipal.IsCompatible(
                        Metadata.PrincipalEntityType,
                        Metadata.DeclaringEntityType,
                        strictPreferExisting.Value ? (bool?)true : null,
                        Metadata.IsUnique))
                {
                    var navigationToDependentName = Metadata.PrincipalToDependent?.Name ?? "";

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
                             Metadata.DependentToPrincipal?.Name != navigationToPrincipalName;

            var builder = dependentEntityType.Navigation(navigationToPrincipalName, Metadata, pointsToPrincipal: true, configurationSource: configurationSource);
            return builder != null
                ? hasChanged ? builder.ReplaceForeignKey(ForeignKeyAspect.None, configurationSource) : builder
                : null;
        }

        public virtual InternalRelationshipBuilder PrincipalToDependent(
            [CanBeNull] string navigationToDependentName,
            ConfigurationSource configurationSource,
            bool? strictPreferExisting = null)
        {
            var principalEntityType = ModelBuilder.Entity(Metadata.PrincipalEntityType.Name, configurationSource);

            if (strictPreferExisting.HasValue)
            {
                var navigationToDependent = navigationToDependentName == null
                    ? null
                    : Metadata.PrincipalEntityType.FindDeclaredNavigation(navigationToDependentName);

                if (navigationToDependent != null
                    && navigationToDependent.IsCompatible(
                        Metadata.PrincipalEntityType,
                        Metadata.DeclaringEntityType,
                        strictPreferExisting.Value ? (bool?)false : null,
                        Metadata.IsUnique))
                {
                    var navigationToPrincipalName = Metadata.DependentToPrincipal?.Name ?? "";

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
                             Metadata.PrincipalToDependent?.Name != navigationToDependentName;

            var builder = principalEntityType.Navigation(navigationToDependentName, Metadata, pointsToPrincipal: false, configurationSource: configurationSource);
            return builder != null
                ? hasChanged ? builder.ReplaceForeignKey(ForeignKeyAspect.None, configurationSource) : builder
                : null;
        }

        public virtual InternalRelationshipBuilder Required(bool isRequired, ConfigurationSource configurationSource)
            => Required(isRequired, configurationSource, runConvention: true);

        public virtual InternalRelationshipBuilder SetRequiredIfCompatible(bool isRequired, ConfigurationSource configurationSource)
            => Required(isRequired, configurationSource, runConvention: false);

        private InternalRelationshipBuilder Required(bool isRequired, ConfigurationSource configurationSource, bool runConvention)
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

            if (_foreignKeyPropertiesConfigurationSource.HasValue
                && (!configurationSource.Overrides(_foreignKeyPropertiesConfigurationSource.Value)
                    || !runConvention)
                && !CanSetRequiredOnProperties(Metadata.Properties, isRequired, configurationSource, shouldThrow: false))
            {
                return null;
            }

            if (runConvention)
            {
                return ReplaceForeignKey(ForeignKeyAspect.IsRequired, configurationSource, isRequired: isRequired);
            }
            var propertyBuilders = InternalEntityTypeBuilder.GetPropertyBuilders(
                ModelBuilder,
                Metadata.Properties.Where(p => ((IProperty)p).ClrType.IsNullableType()),
                ConfigurationSource.Convention);

            foreach (var property in propertyBuilders)
            {
                var requiredSet = property.Required(isRequired, configurationSource);
                if (requiredSet
                    && isRequired != true)
                {
                    break;
                }
                Debug.Assert(requiredSet || isRequired != true);
            }

            _isRequiredConfigurationSource = configurationSource.Max(_isRequiredConfigurationSource);
            return this;
        }

        private bool CanSetRequiredOnProperties(IEnumerable<Property> properties, bool? isRequired, ConfigurationSource configurationSource, bool shouldThrow)
            => CanSetRequiredOnProperties(
                properties,
                isRequired,
                ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, ConfigurationSource.Convention),
                configurationSource,
                shouldThrow);

        private static bool CanSetRequiredOnProperties(
            IEnumerable<Property> properties,
            bool? isRequired,
            InternalEntityTypeBuilder entityTypeBuilder,
            ConfigurationSource configurationSource,
            bool shouldThrow)
        {
            if (isRequired == null
                || properties == null)
            {
                return true;
            }

            if (!Entity.Metadata.ForeignKey.CanPropertiesBeRequired(
                properties,
                isRequired,
                entityTypeBuilder.Metadata, shouldThrow))
            {
                return false;
            }

            var nullableProperties = properties.Where(p => ((IProperty)p).ClrType.IsNullableType())
                .Select(property => entityTypeBuilder.Property(property.Name, ConfigurationSource.Convention));

            return isRequired.Value
                ? nullableProperties.All(property => property.CanSetRequired(true, configurationSource))
                : nullableProperties.Any(property => property.CanSetRequired(false, configurationSource));
        }

        public virtual InternalRelationshipBuilder DeleteBehavior(DeleteBehavior deleteBehavior, ConfigurationSource configurationSource)
            => DeleteBehavior(deleteBehavior, configurationSource, runConventions: true);

        public virtual InternalRelationshipBuilder SetDeleteBehaviorIfCompatible(DeleteBehavior deleteBehavior, ConfigurationSource configurationSource)
            => DeleteBehavior(deleteBehavior, configurationSource, runConventions: false);

        private InternalRelationshipBuilder DeleteBehavior(DeleteBehavior deleteBehavior, ConfigurationSource configurationSource, bool runConventions)
        {
            if (((IForeignKey)Metadata).DeleteBehavior == deleteBehavior)
            {
                Metadata.DeleteBehavior = deleteBehavior;
                _deletebehaviorConfigurationSource = configurationSource.Max(_deletebehaviorConfigurationSource);
                return this;
            }

            if (_deletebehaviorConfigurationSource != null
                && !configurationSource.Overrides(_deletebehaviorConfigurationSource.Value))
            {
                return null;
            }

            if (runConventions)
            {
                return ReplaceForeignKey(ForeignKeyAspect.DeleteBehavior, configurationSource, deleteBehavior: deleteBehavior);
            }
            _deletebehaviorConfigurationSource = configurationSource.Max(_deletebehaviorConfigurationSource);
            Metadata.DeleteBehavior = deleteBehavior;
            return this;
        }

        public virtual InternalRelationshipBuilder Unique(bool isUnique, ConfigurationSource configurationSource)
            => Unique(isUnique, configurationSource, runConventions: true);

        public virtual InternalRelationshipBuilder SetUniqueIfCompatible(bool isUnique, ConfigurationSource configurationSource)
            => Unique(isUnique, configurationSource, runConventions: false);

        private InternalRelationshipBuilder Unique(bool isUnique, ConfigurationSource configurationSource, bool runConventions)
        {
            if (((IForeignKey)Metadata).IsUnique == isUnique)
            {
                Metadata.IsUnique = isUnique;
                _isUniqueConfigurationSource = configurationSource.Max(_isUniqueConfigurationSource);
                return this;
            }

            if (_isUniqueConfigurationSource != null
                && !configurationSource.Overrides(_isUniqueConfigurationSource.Value))
            {
                return null;
            }

            var builder = this;
            if (Metadata.PrincipalToDependent != null)
            {
                if (!Navigation.IsCompatible(
                    Metadata.PrincipalToDependent.Name,
                    Metadata.PrincipalEntityType,
                    Metadata.DeclaringEntityType,
                    !isUnique,
                    shouldThrow: false))
                {
                    if (runConventions)
                    {
                        builder = ModelBuilder.Entity(Metadata.PrincipalEntityType.Name, ConfigurationSource.Convention)
                            .Navigation(null, Metadata, false, configurationSource);
                        if (builder == null)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            if (runConventions)
            {
                return builder.ReplaceForeignKey(ForeignKeyAspect.IsUnique, configurationSource, isUnique: isUnique);
            }
            _isUniqueConfigurationSource = configurationSource.Max(_isUniqueConfigurationSource);
            Metadata.IsUnique = isUnique;
            return this;
        }

        public virtual bool CanSetUnique(bool isUnique, ConfigurationSource configurationSource)
        {
            if (((IForeignKey)Metadata).IsUnique == isUnique)
            {
                return true;
            }

            if (_isUniqueConfigurationSource != null
                && !configurationSource.Overrides(_isUniqueConfigurationSource.Value))
            {
                return false;
            }

            if (Metadata.PrincipalToDependent != null
                && !Navigation.IsCompatible(
                    Metadata.PrincipalToDependent.Name,
                    Metadata.PrincipalEntityType,
                    Metadata.DeclaringEntityType,
                    !isUnique,
                    shouldThrow: false))
            {
                return ModelBuilder.Entity(Metadata.PrincipalEntityType.Name, ConfigurationSource.Convention)
                    .CanSetNavigation(null, this, pointsToPrincipal: false, configurationSource: configurationSource);
            }

            return true;
        }

        public virtual InternalRelationshipBuilder Invert(ConfigurationSource configurationSource, bool runConventions = true)
        {
            if (!CanSetUnique(true, configurationSource))
            {
                return null;
            }

            if ((_foreignKeyPropertiesConfigurationSource != null && _foreignKeyPropertiesConfigurationSource.Value.Overrides(configurationSource))
                || (_principalKeyConfigurationSource != null && _principalKeyConfigurationSource.Value.Overrides(configurationSource))
                || (_principalEndConfigurationSource != null && !configurationSource.Overrides(_principalEndConfigurationSource.Value)))
            {
                if (configurationSource == ConfigurationSource.Explicit)
                {
                    throw new InvalidOperationException(Strings.RelationshipCannotBeInverted);
                }
                return null;
            }

            return ReplaceForeignKey(
                ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, ConfigurationSource.Convention),
                ModelBuilder.Entity(Metadata.PrincipalEntityType.Name, ConfigurationSource.Convention),
                Metadata.PrincipalToDependent?.Name,
                Metadata.DependentToPrincipal?.Name,
                null,
                null,
                true,
                null,
                null,
                ForeignKeyAspect.PrincipalEnd | ForeignKeyAspect.IsUnique,
                configurationSource,
                runConventions);
        }

        public virtual InternalRelationshipBuilder SetPrincipalEndIfCompatible(
            [NotNull] EntityType principalEntityType, ConfigurationSource configurationSource)
        {
            Check.NotNull(principalEntityType, nameof(principalEntityType));

            if (Metadata.PrincipalEntityType != principalEntityType)
            {
                return null;
            }

            _principalEndConfigurationSource = configurationSource.Max(_principalEndConfigurationSource);
            return this;
        }

        public virtual InternalRelationshipBuilder ForeignKey(
            [NotNull] IReadOnlyList<PropertyInfo> properties, ConfigurationSource configurationSource)
            => ForeignKey(
                ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, configurationSource)
                    .GetOrCreateProperties(properties, configurationSource),
                configurationSource);

        public virtual InternalRelationshipBuilder ForeignKey(
            [NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => ForeignKey(
                ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, configurationSource)
                    .GetOrCreateProperties(propertyNames, configurationSource),
                configurationSource);

        public virtual InternalRelationshipBuilder ForeignKey(
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
            => ForeignKey(properties, configurationSource, runConventions: true);

        public virtual InternalRelationshipBuilder SetForeignKeyIfCompatible(
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
            => ForeignKey(properties, configurationSource, runConventions: false);

        private InternalRelationshipBuilder ForeignKey(
            IReadOnlyList<Property> properties, ConfigurationSource configurationSource, bool runConventions)
        {
            properties = GetExistingProperties(properties, Metadata.DeclaringEntityType);
            if (properties == null)
            {
                return null;
            }

            if (Metadata.Properties.SequenceEqual(properties))
            {
                _foreignKeyPropertiesConfigurationSource = configurationSource.Max(_foreignKeyPropertiesConfigurationSource);
                InternalEntityTypeBuilder.GetPropertyBuilders(ModelBuilder, properties, configurationSource).ToList();
                return this;
            }

            if (_foreignKeyPropertiesConfigurationSource != null
                && !configurationSource.Overrides(_foreignKeyPropertiesConfigurationSource.Value))
            {
                return null;
            }

            if (_isRequiredConfigurationSource.HasValue
                && (!configurationSource.Overrides(_isRequiredConfigurationSource.Value)
                    || !runConventions)
                && !CanSetRequiredOnProperties(properties, ((IForeignKey)Metadata).IsRequired, configurationSource, shouldThrow: false))
            {
                return null;
            }

            if (_principalKeyConfigurationSource.HasValue
                && (!configurationSource.Overrides(_principalKeyConfigurationSource.Value)
                    || !runConventions)
                && !Entity.Metadata.ForeignKey.AreCompatible(
                    Metadata.PrincipalKey.Properties,
                    properties,
                    Metadata.PrincipalEntityType,
                    Metadata.DeclaringEntityType,
                    shouldThrow: false))
            {
                return null;
            }

            return ReplaceForeignKey(
                ForeignKeyAspect.DependentProperties,
                configurationSource,
                dependentProperties: properties,
                principalProperties: runConventions ? null : Metadata.PrincipalKey.Properties,
                runConventions: runConventions);
        }

        public virtual InternalRelationshipBuilder ForeignKey(
            [NotNull] Type specifiedDependentType,
            [NotNull] IReadOnlyList<PropertyInfo> properties,
            ConfigurationSource configurationSource)
            => EnsureDependent(ResolveType(specifiedDependentType), configurationSource)
                .ForeignKey(properties, configurationSource);

        public virtual InternalRelationshipBuilder ForeignKey(
            [NotNull] Type specifiedDependentType,
            [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
            => EnsureDependent(ResolveType(specifiedDependentType), configurationSource)
                .ForeignKey(propertyNames, configurationSource);

        public virtual InternalRelationshipBuilder ForeignKey(
            [NotNull] string specifiedDependentTypeName,
            [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
            => EnsureDependent(ResolveType(specifiedDependentTypeName), configurationSource)
                .ForeignKey(propertyNames, configurationSource);

        private InternalRelationshipBuilder EnsureDependent(EntityType entityType, ConfigurationSource configurationSource)
            => entityType == Metadata.DeclaringEntityType
                ? this
                : Invert(configurationSource);

        public virtual InternalRelationshipBuilder PrincipalKey([NotNull] IReadOnlyList<PropertyInfo> properties,
            ConfigurationSource configurationSource)
            => PrincipalKey(
                ModelBuilder.Entity(Metadata.PrincipalEntityType.Name, configurationSource)
                    .GetOrCreateProperties(properties, configurationSource),
                configurationSource);

        public virtual InternalRelationshipBuilder PrincipalKey([NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
            => PrincipalKey(
                ModelBuilder.Entity(Metadata.PrincipalEntityType.Name, configurationSource)
                    .GetOrCreateProperties(propertyNames, configurationSource),
                configurationSource);

        public virtual InternalRelationshipBuilder PrincipalKey(
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
            => PrincipalKey(properties, configurationSource, runConventions: true);

        public virtual InternalRelationshipBuilder SetPrincipalKeyIfCompatible(
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
            => PrincipalKey(properties, configurationSource, runConventions: false);

        private InternalRelationshipBuilder PrincipalKey(
            IReadOnlyList<Property> properties, ConfigurationSource configurationSource, bool runConventions)
        {
            properties = GetExistingProperties(properties, Metadata.PrincipalEntityType);
            if (properties == null)
            {
                return null;
            }

            if (Metadata.PrincipalKey.Properties.SequenceEqual(properties))
            {
                _principalKeyConfigurationSource = configurationSource.Max(_principalKeyConfigurationSource);
                var principalEntityTypeBuilder = ModelBuilder.Entity(Metadata.PrincipalKey.DeclaringEntityType.Name, configurationSource);
                principalEntityTypeBuilder.Key(properties.Select(p => p.Name).ToList(), configurationSource);
                return this;
            }

            if (_principalKeyConfigurationSource != null
                && !configurationSource.Overrides(_principalKeyConfigurationSource.Value))
            {
                return null;
            }

            if (_foreignKeyPropertiesConfigurationSource.HasValue
                && (!configurationSource.Overrides(_foreignKeyPropertiesConfigurationSource.Value)
                    || !runConventions)
                && !Entity.Metadata.ForeignKey.AreCompatible(
                    properties,
                    Metadata.Properties,
                    Metadata.PrincipalEntityType,
                    Metadata.DeclaringEntityType,
                    shouldThrow: false))
            {
                return null;
            }

            return ReplaceForeignKey(
                ForeignKeyAspect.PrincipalKey,
                configurationSource,
                principalProperties: properties,
                dependentProperties: runConventions ? null : Metadata.Properties,
                runConventions: runConventions);
        }

        public virtual InternalRelationshipBuilder PrincipalKey(
            [NotNull] Type specifiedPrincipalType,
            [NotNull] IReadOnlyList<PropertyInfo> properties,
            ConfigurationSource configurationSource)
            => EnsurePrincipal(ResolveType(specifiedPrincipalType), configurationSource)
                .PrincipalKey(properties, configurationSource);

        public virtual InternalRelationshipBuilder PrincipalKey(
            [NotNull] Type specifiedPrincipalType,
            [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
            => EnsurePrincipal(ResolveType(specifiedPrincipalType), configurationSource)
                .PrincipalKey(propertyNames, configurationSource);

        public virtual InternalRelationshipBuilder PrincipalKey(
            [NotNull] string specifiedPrincipalTypeName,
            [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
            => EnsurePrincipal(ResolveType(specifiedPrincipalTypeName), configurationSource)
                .PrincipalKey(propertyNames, configurationSource);

        private InternalRelationshipBuilder EnsurePrincipal(EntityType entityType, ConfigurationSource configurationSource)
            => entityType == Metadata.PrincipalEntityType
                ? this
                : Invert(configurationSource);

        private InternalRelationshipBuilder ReplaceForeignKey(
            ForeignKeyAspect aspectsConfigured,
            ConfigurationSource configurationSource,
            IReadOnlyList<Property> dependentProperties = null,
            IReadOnlyList<Property> principalProperties = null,
            bool? isUnique = null,
            bool? isRequired = null,
            DeleteBehavior? deleteBehavior = null,
            bool runConventions = true)
        {
            dependentProperties = dependentProperties ??
                                  (_foreignKeyPropertiesConfigurationSource.HasValue
                                   && (!configurationSource.Overrides(_foreignKeyPropertiesConfigurationSource.Value)
                                       || _foreignKeyPropertiesConfigurationSource == ConfigurationSource.Explicit)
                                      ? Metadata.Properties
                                      : null);

            principalProperties = principalProperties ??
                                  (_principalKeyConfigurationSource.HasValue
                                   && (!configurationSource.Overrides(_principalKeyConfigurationSource.Value)
                                       || _principalKeyConfigurationSource == ConfigurationSource.Explicit)
                                      ? Metadata.PrincipalKey.Properties
                                      : null);

            isUnique = isUnique ??
                       (_isUniqueConfigurationSource.HasValue
                        && (!configurationSource.Overrides(_isUniqueConfigurationSource.Value)
                            || _isUniqueConfigurationSource == ConfigurationSource.Explicit)
                           ? ((IForeignKey)Metadata).IsUnique
                           : (bool?)null);

            isRequired = isRequired ??
                         (_isRequiredConfigurationSource.HasValue
                          && (!configurationSource.Overrides(_isRequiredConfigurationSource.Value)
                              || _isRequiredConfigurationSource == ConfigurationSource.Explicit)
                             ? ((IForeignKey)Metadata).IsRequired
                             : (bool?)null);

            deleteBehavior = deleteBehavior ??
                             (_deletebehaviorConfigurationSource.HasValue
                              && (!configurationSource.Overrides(_deletebehaviorConfigurationSource.Value)
                                  || _deletebehaviorConfigurationSource == ConfigurationSource.Explicit)
                                 ? ((IForeignKey)Metadata).DeleteBehavior
                                 : (DeleteBehavior?)null);

            var principalEntityTypeBuilder = ModelBuilder.Entity(Metadata.PrincipalEntityType.Name, ConfigurationSource.Convention);
            var dependentEntityTypeBuilder = ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, ConfigurationSource.Convention);
            return ReplaceForeignKey(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                Metadata.DependentToPrincipal?.Name,
                Metadata.PrincipalToDependent?.Name,
                dependentProperties,
                principalProperties,
                isUnique,
                isRequired,
                deleteBehavior,
                aspectsConfigured,
                configurationSource,
                runConventions);
        }

        private InternalRelationshipBuilder ReplaceForeignKey(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [NotNull] InternalEntityTypeBuilder dependentEntityTypeBuilder,
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            [CanBeNull] IReadOnlyList<Property> dependentProperties,
            [CanBeNull] IReadOnlyList<Property> principalProperties,
            bool? isUnique,
            bool? isRequired,
            DeleteBehavior? deleteBehavior,
            ForeignKeyAspect aspectsConfigured,
            ConfigurationSource configurationSource,
            bool runConventions = true)
        {
            if (!AreCompatible(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                navigationToPrincipalName,
                navigationToDependentName,
                dependentProperties,
                principalProperties,
                isUnique,
                isRequired,
                configurationSource))
            {
                return null;
            }

            var inverted = Metadata.DeclaringEntityType != dependentEntityTypeBuilder.Metadata;
            Debug.Assert(inverted
                         || (Metadata.DeclaringEntityType == dependentEntityTypeBuilder.Metadata
                             && Metadata.PrincipalEntityType == principalEntityTypeBuilder.Metadata));
            Debug.Assert(!inverted
                         || (Metadata.DeclaringEntityType == principalEntityTypeBuilder.Metadata
                             && Metadata.PrincipalEntityType == dependentEntityTypeBuilder.Metadata));
            Debug.Assert(!inverted
                         || aspectsConfigured.HasFlag(ForeignKeyAspect.PrincipalEnd));

            var replacedConfigurationSource = inverted
                ? principalEntityTypeBuilder.RemoveRelationship(Metadata, ConfigurationSource.Explicit)
                : dependentEntityTypeBuilder.RemoveRelationship(Metadata, ConfigurationSource.Explicit);

            var shouldUpgradeSource = aspectsConfigured.HasFlag(ForeignKeyAspect.DependentProperties)
                                      || aspectsConfigured.HasFlag(ForeignKeyAspect.PrincipalEnd);

            return !replacedConfigurationSource.HasValue
                ? null
                : AddRelationship(
                    principalEntityTypeBuilder,
                    dependentEntityTypeBuilder,
                    navigationToPrincipalName,
                    navigationToDependentName,
                    GetExistingProperties(dependentProperties, dependentEntityTypeBuilder.Metadata),
                    GetExistingProperties(principalProperties, principalEntityTypeBuilder.Metadata),
                    isUnique,
                    isRequired,
                    deleteBehavior,
                    aspectsConfigured,
                    configurationSource,
                    shouldUpgradeSource ? configurationSource.Max(replacedConfigurationSource.Value) : replacedConfigurationSource.Value,
                    runConventions);
        }

        private InternalRelationshipBuilder AddRelationship(
            InternalEntityTypeBuilder principalEntityTypeBuilder,
            InternalEntityTypeBuilder dependentEntityTypeBuilder,
            string navigationToPrincipalName,
            string navigationToDependentName,
            IReadOnlyList<Property> foreignKeyProperties,
            IReadOnlyList<Property> principalProperties,
            bool? isUnique,
            bool? isRequired,
            DeleteBehavior? deleteBehavior,
            ForeignKeyAspect aspectsConfigured,
            ConfigurationSource aspectConfigurationSource,
            ConfigurationSource configurationSource,
            bool runConventions = true)
        {
            return dependentEntityTypeBuilder.Relationship(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                navigationToPrincipalName,
                navigationToDependentName,
                foreignKeyProperties,
                principalProperties,
                configurationSource,
                isUnique,
                isRequired,
                deleteBehavior,
                strictPrincipal: true,
                onRelationshipAdding: b => MergeConfigurationSourceWith(b, this, aspectsConfigured, aspectConfigurationSource),
                runConventions: runConventions);
        }

        public virtual InternalRelationshipBuilder Attach(ConfigurationSource configurationSource)
        {
            if (Metadata.DeclaringEntityType.GetForeignKeys().Contains(Metadata))
            {
                return ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, configurationSource)
                    .Relationship(Metadata, existingForeignKey: true, configurationSource: configurationSource);
            }

            // Only restore explicit configuration. The rest will be handled by the conventions.
            var aspectsConfigured = ForeignKeyAspect.None;
            IReadOnlyList<Property> foreignKeyProperties = null;
            if (_foreignKeyPropertiesConfigurationSource.HasValue
                && _foreignKeyPropertiesConfigurationSource.Value.Overrides(ConfigurationSource.Explicit))
            {
                foreignKeyProperties = GetExistingProperties(Metadata.Properties, Metadata.DeclaringEntityType);
                if (foreignKeyProperties == null)
                {
                    _foreignKeyPropertiesConfigurationSource = null;
                }
                else
                {
                    aspectsConfigured |= ForeignKeyAspect.DependentProperties;
                    aspectsConfigured |= ForeignKeyAspect.PrincipalEnd;
                }
            }

            Key principalKey = null;
            if (_principalKeyConfigurationSource.HasValue
                && _principalKeyConfigurationSource.Value.Overrides(ConfigurationSource.Explicit))
            {
                principalKey = Metadata.PrincipalEntityType.FindKey(Metadata.PrincipalKey.Properties);
                if (principalKey == null)
                {
                    _principalKeyConfigurationSource = null;
                }
                else
                {
                    aspectsConfigured |= ForeignKeyAspect.PrincipalKey;
                    aspectsConfigured |= ForeignKeyAspect.PrincipalEnd;
                }
            }

            bool? isUnique = null;
            if (_isUniqueConfigurationSource.HasValue
                && _isUniqueConfigurationSource.Value.Overrides(ConfigurationSource.Explicit))
            {
                isUnique = Metadata.IsUnique;
            }

            bool? isRequired = null;
            if (_isRequiredConfigurationSource.HasValue
                && _isRequiredConfigurationSource.Value.Overrides(ConfigurationSource.Explicit))
            {
                isRequired = Metadata.IsRequired;
            }

            DeleteBehavior? deletebehavior = null;
            if (_deletebehaviorConfigurationSource.HasValue
                && _deletebehaviorConfigurationSource.Value.Overrides(ConfigurationSource.Explicit))
            {
                deletebehavior = Metadata.DeleteBehavior;
            }

            return AddRelationship(
                ModelBuilder.Entity(Metadata.PrincipalEntityType.Name, configurationSource),
                ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, configurationSource),
                null,
                null,
                foreignKeyProperties,
                principalKey?.Properties,
                isUnique,
                isRequired,
                deletebehavior,
                aspectsConfigured,
                ConfigurationSource.Explicit,
                configurationSource);
        }

        private List<Property> GetExistingProperties(IReadOnlyList<Property> properties, EntityType entityType)
        {
            if (properties == null)
            {
                return null;
            }

            var foundProperties = new List<Property>();
            foreach (var property in properties)
            {
                var foundProperty = entityType.FindProperty(property.Name);
                if (foundProperty == null)
                {
                    return null;
                }
                foundProperties.Add(foundProperty);
            }
            return foundProperties;
        }

        private static InternalRelationshipBuilder MergeConfigurationSourceWith(
            InternalRelationshipBuilder newBuilder,
            InternalRelationshipBuilder oldBuilder,
            ForeignKeyAspect aspectsConfigured,
            ConfigurationSource configurationSource)
        {
            var newFk = newBuilder.Metadata;
            if (aspectsConfigured.HasFlag(ForeignKeyAspect.PrincipalEnd)
                || aspectsConfigured.HasFlag(ForeignKeyAspect.DependentProperties)
                || aspectsConfigured.HasFlag(ForeignKeyAspect.PrincipalKey))
            {
                newBuilder._principalEndConfigurationSource = configurationSource
                    .Max(newBuilder._principalEndConfigurationSource);
            }
            else
            {
                newBuilder._principalEndConfigurationSource = oldBuilder._principalEndConfigurationSource?
                    .Max(newBuilder._principalEndConfigurationSource)
                                                              ?? newBuilder._principalEndConfigurationSource;
            }

            var inverted = oldBuilder.Metadata.DeclaringEntityType != newBuilder.Metadata.DeclaringEntityType;
            Debug.Assert(inverted
                         || (oldBuilder.Metadata.DeclaringEntityType == newBuilder.Metadata.DeclaringEntityType
                             && oldBuilder.Metadata.PrincipalEntityType == newBuilder.Metadata.PrincipalEntityType));
            Debug.Assert(!inverted
                         || (oldBuilder.Metadata.DeclaringEntityType == newBuilder.Metadata.PrincipalEntityType
                             && oldBuilder.Metadata.PrincipalEntityType == newBuilder.Metadata.DeclaringEntityType));

            if (aspectsConfigured.HasFlag(ForeignKeyAspect.DependentProperties))
            {
                newBuilder = newBuilder.SetForeignKeyIfCompatible(
                    newFk.Properties, configurationSource);
            }
            if (aspectsConfigured.HasFlag(ForeignKeyAspect.PrincipalKey))
            {
                newBuilder = newBuilder.SetPrincipalKeyIfCompatible(
                    newFk.PrincipalKey.Properties, configurationSource);
            }
            if (aspectsConfigured.HasFlag(ForeignKeyAspect.IsUnique))
            {
                newBuilder = newBuilder.SetUniqueIfCompatible(
                    newFk.IsUnique.Value, configurationSource);
            }
            if (aspectsConfigured.HasFlag(ForeignKeyAspect.IsRequired))
            {
                newBuilder = newBuilder.SetRequiredIfCompatible(
                    newFk.IsRequired.Value, configurationSource);
            }
            if (aspectsConfigured.HasFlag(ForeignKeyAspect.DeleteBehavior))
            {
                newBuilder = newBuilder.SetDeleteBehaviorIfCompatible(
                    newFk.DeleteBehavior.Value, configurationSource);
            }

            if (!aspectsConfigured.HasFlag(ForeignKeyAspect.PrincipalKey)
                && !inverted
                && oldBuilder._principalKeyConfigurationSource.HasValue
                && (!newBuilder._principalKeyConfigurationSource.HasValue
                    || oldBuilder._principalKeyConfigurationSource.Value.Overrides(
                        newBuilder._principalKeyConfigurationSource.Value)))
            {
                newBuilder = newBuilder.SetPrincipalKeyIfCompatible(
                    oldBuilder.Metadata.PrincipalKey.Properties, oldBuilder._principalKeyConfigurationSource.Value)
                             ?? newBuilder;
            }

            if (!aspectsConfigured.HasFlag(ForeignKeyAspect.DependentProperties)
                && !inverted
                && oldBuilder._foreignKeyPropertiesConfigurationSource.HasValue
                && (!newBuilder._foreignKeyPropertiesConfigurationSource.HasValue
                    || oldBuilder._foreignKeyPropertiesConfigurationSource.Value.Overrides(newBuilder._foreignKeyPropertiesConfigurationSource.Value)))
            {
                newBuilder = newBuilder.SetForeignKeyIfCompatible(
                    oldBuilder.Metadata.Properties, oldBuilder._foreignKeyPropertiesConfigurationSource.Value)
                             ?? newBuilder;
            }

            if (!aspectsConfigured.HasFlag(ForeignKeyAspect.IsUnique)
                && oldBuilder._isUniqueConfigurationSource.HasValue
                && oldBuilder.Metadata.IsUnique.HasValue
                && (!newBuilder._isUniqueConfigurationSource.HasValue
                    || oldBuilder._isUniqueConfigurationSource.Value.Overrides(
                        newBuilder._isUniqueConfigurationSource.Value)))
            {
                newBuilder = newBuilder.SetUniqueIfCompatible(
                    oldBuilder.Metadata.IsUnique.Value, oldBuilder._isUniqueConfigurationSource.Value)
                             ?? newBuilder;
            }

            if (!aspectsConfigured.HasFlag(ForeignKeyAspect.IsRequired)
                && oldBuilder._isRequiredConfigurationSource.HasValue
                && oldBuilder.Metadata.IsRequired.HasValue
                && (!newBuilder._isRequiredConfigurationSource.HasValue
                    || oldBuilder._isRequiredConfigurationSource.Value.Overrides(
                        newBuilder._isRequiredConfigurationSource.Value)))
            {
                newBuilder = newBuilder.SetRequiredIfCompatible(
                    oldBuilder.Metadata.IsRequired.Value, oldBuilder._isRequiredConfigurationSource.Value)
                             ?? newBuilder;
            }

            if (!aspectsConfigured.HasFlag(ForeignKeyAspect.DeleteBehavior)
                && oldBuilder._deletebehaviorConfigurationSource.HasValue
                && oldBuilder.Metadata.DeleteBehavior.HasValue
                && (!newBuilder._deletebehaviorConfigurationSource.HasValue
                    || oldBuilder._deletebehaviorConfigurationSource.Value.Overrides(
                        newBuilder._deletebehaviorConfigurationSource.Value)))
            {
                newBuilder = newBuilder.SetDeleteBehaviorIfCompatible(
                    oldBuilder.Metadata.DeleteBehavior.Value, oldBuilder._deletebehaviorConfigurationSource.Value)
                             ?? newBuilder;
            }

            return newBuilder;
        }

        private EntityType ResolveType(Type type)
        {
            if (type == Metadata.DeclaringEntityType.ClrType)
            {
                return Metadata.DeclaringEntityType;
            }

            if (type == Metadata.PrincipalEntityType.ClrType)
            {
                return Metadata.PrincipalEntityType;
            }

            throw new ArgumentException(Strings.EntityTypeNotInRelationship(type.FullName, Metadata.DeclaringEntityType.Name, Metadata.PrincipalEntityType.Name));
        }

        private EntityType ResolveType(string name)
        {
            if (name == Metadata.DeclaringEntityType.Name)
            {
                return Metadata.DeclaringEntityType;
            }

            if (name == Metadata.PrincipalEntityType.Name)
            {
                return Metadata.PrincipalEntityType;
            }

            throw new ArgumentException(Strings.EntityTypeNotInRelationship(name, Metadata.DeclaringEntityType.Name, Metadata.PrincipalEntityType.Name));
        }

        public static bool AreCompatible(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [NotNull] InternalEntityTypeBuilder dependentEntityTypeBuilder,
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            [CanBeNull] IReadOnlyList<Property> dependentProperties,
            [CanBeNull] IReadOnlyList<Property> principalProperties,
            bool? isUnique,
            bool? isRequired,
            ConfigurationSource configurationSource)
        {
            var shouldThrow = configurationSource == ConfigurationSource.Explicit;
            if (dependentProperties != null
                && !CanSetRequiredOnProperties(
                    dependentProperties,
                    isRequired,
                    dependentEntityTypeBuilder,
                    configurationSource,
                    shouldThrow))
            {
                return false;
            }

            return Entity.Metadata.ForeignKey.AreCompatible(
                principalEntityTypeBuilder.Metadata,
                dependentEntityTypeBuilder.Metadata,
                navigationToPrincipalName,
                navigationToDependentName,
                dependentProperties,
                principalProperties,
                isUnique,
                isRequired,
                shouldThrow);
        }

        [Flags]
        private enum ForeignKeyAspect
        {
            None = 0,
            PrincipalEnd = 1 << 0,
            DependentProperties = 1 << 1,
            PrincipalKey = 1 << 2,
            IsUnique = 1 << 3,
            IsRequired = 1 << 4,
            DeleteBehavior = 1 << 5
        }
    }
}
