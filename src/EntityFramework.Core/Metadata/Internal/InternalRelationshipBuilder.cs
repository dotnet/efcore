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
        private ConfigurationSource? _deleteBehaviorConfigurationSource;
        private ConfigurationSource? _principalEndConfigurationSource;
        private ConfigurationSource? _dependentToPrincipalConfigurationSource;
        private ConfigurationSource? _principalToDependentConfigurationSource;

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
            _deleteBehaviorConfigurationSource = initialConfigurationSource;
            _principalEndConfigurationSource = initialConfigurationSource;
            _dependentToPrincipalConfigurationSource = initialConfigurationSource;
            _principalToDependentConfigurationSource = initialConfigurationSource;
        }

        public virtual InternalRelationshipBuilder DependentToPrincipal(
            [CanBeNull] string navigationToPrincipalName,
            ConfigurationSource configurationSource,
            bool? strictPrincipal = null,
            bool runConventions = true)
        {
            if (navigationToPrincipalName == Metadata.DependentToPrincipal?.Name)
            {
                ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, configurationSource);
                _dependentToPrincipalConfigurationSource = configurationSource.Max(
                    _dependentToPrincipalConfigurationSource);
                return this;
            }

            if (_dependentToPrincipalConfigurationSource.HasValue
                && !configurationSource.Overrides(_dependentToPrincipalConfigurationSource.Value))
            {
                return null;
            }

            var dependentEntityType = ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, configurationSource);

            // TODO: Remove this
            if (strictPrincipal.HasValue)
            {
                var navigationToPrincipal = string.IsNullOrEmpty(navigationToPrincipalName)
                    ? null
                    : Metadata.DeclaringEntityType.FindDeclaredNavigation(navigationToPrincipalName);

                if (navigationToPrincipal != null
                    && navigationToPrincipal.IsCompatible(
                        Metadata.PrincipalEntityType,
                        Metadata.DeclaringEntityType,
                        strictPrincipal.Value ? (bool?)true : null,
                        Metadata.IsUnique))
                {
                    var navigationToDependentName = Metadata.PrincipalToDependent?.Name ?? "";

                    if (Metadata == navigationToPrincipal.ForeignKey
                        || dependentEntityType.RemoveForeignKey(Metadata, configurationSource).HasValue)
                    {
                        return dependentEntityType.Relationship(
                            navigationToPrincipal,
                            configurationSource,
                            navigationToDependentName);
                    }
                }
            }

            var hasChanged = Metadata.DependentToPrincipal?.Name != navigationToPrincipalName;

            var builder = dependentEntityType.Navigation(
                navigationToPrincipalName,
                Metadata,
                pointsToPrincipal: true,
                configurationSource: configurationSource,
                runConventions: runConventions);

            if (builder == null
                || !hasChanged
                || !runConventions)
            {
                return builder;
            }

            return builder.ReplaceForeignKey(ForeignKeyAspect.None, configurationSource);
        }

        public virtual InternalRelationshipBuilder PrincipalToDependent(
            [CanBeNull] string navigationToDependentName,
            ConfigurationSource configurationSource,
            bool? strictPrincipal = null,
            bool runConventions = true)
        {
            if (navigationToDependentName == Metadata.PrincipalToDependent?.Name)
            {
                ModelBuilder.Entity(Metadata.PrincipalEntityType.Name, configurationSource);
                _principalToDependentConfigurationSource = configurationSource.Max(
                    _principalToDependentConfigurationSource);
                return this;
            }

            if (_principalToDependentConfigurationSource.HasValue
                && !configurationSource.Overrides(_principalToDependentConfigurationSource.Value))
            {
                return null;
            }

            var principalEntityType = ModelBuilder.Entity(Metadata.PrincipalEntityType.Name, configurationSource);

            // TODO: Remove this
            if (strictPrincipal.HasValue)
            {
                var navigationToDependent = navigationToDependentName == null
                    ? null
                    : Metadata.PrincipalEntityType.FindDeclaredNavigation(navigationToDependentName);

                if (navigationToDependent != null
                    && navigationToDependent.IsCompatible(
                        Metadata.PrincipalEntityType,
                        Metadata.DeclaringEntityType,
                        strictPrincipal.Value ? (bool?)false : null,
                        Metadata.IsUnique))
                {
                    var navigationToPrincipalName = Metadata.DependentToPrincipal?.Name ?? "";

                    if (Metadata == navigationToDependent.ForeignKey
                        || principalEntityType.RemoveForeignKey(Metadata, configurationSource).HasValue)
                    {
                        return principalEntityType.Relationship(
                            navigationToDependent,
                            configurationSource,
                            navigationToPrincipalName);
                    }
                }
            }

            var hasChanged = Metadata.PrincipalToDependent?.Name != navigationToDependentName;

            var builder = principalEntityType.Navigation(
                navigationToDependentName,
                Metadata,
                pointsToPrincipal: false,
                configurationSource: configurationSource,
                runConventions: runConventions);

            if (builder == null
                || !hasChanged
                || !runConventions)
            {
                return builder;
            }

            return builder.ReplaceForeignKey(ForeignKeyAspect.None, configurationSource);
        }

        public virtual bool CanSetNavigation(
            [CanBeNull] string navigationName,
            bool pointsToPrincipal,
            ConfigurationSource configurationSource)
        {
            if (pointsToPrincipal)
            {
                if (navigationName == Metadata.DependentToPrincipal?.Name)
                {
                    return true;
                }

                return !_dependentToPrincipalConfigurationSource.HasValue
                       || configurationSource.Overrides(_dependentToPrincipalConfigurationSource.Value);
            }
            if (navigationName == Metadata.PrincipalToDependent?.Name)
            {
                return true;
            }

            return !_principalToDependentConfigurationSource.HasValue
                   || configurationSource.Overrides(_principalToDependentConfigurationSource.Value);
        }

        public virtual InternalRelationshipBuilder IsRequired(bool isRequired, ConfigurationSource configurationSource, bool runConventions = true)
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
                && !CanSetRequiredOnProperties(Metadata.Properties, isRequired, configurationSource, shouldThrow: false))
            {
                if (!configurationSource.Overrides(_foreignKeyPropertiesConfigurationSource.Value))
                {
                    return null;
                }
                _foreignKeyPropertiesConfigurationSource = null;
            }

            if (runConventions)
            {
                return ReplaceForeignKey(ForeignKeyAspect.IsRequired, configurationSource, isRequired: isRequired);
            }
            var propertyBuilders = InternalEntityTypeBuilder.GetPropertyBuilders(
                ModelBuilder,
                Metadata.Properties.Where(p => ((IProperty)p).ClrType.IsNullableType()),
                ConfigurationSource.Convention);

            foreach (var property in propertyBuilders)
            {
                var requiredSet = property.IsRequired(isRequired, configurationSource);
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

        private bool CanSetRequired(bool isRequired, ConfigurationSource configurationSource)
        {
            if (((IForeignKey)Metadata).IsRequired == isRequired)
            {
                return true;
            }

            if (_isRequiredConfigurationSource != null
                && !configurationSource.Overrides(_isRequiredConfigurationSource.Value))
            {
                return false;
            }

            if (_foreignKeyPropertiesConfigurationSource.HasValue
                && !configurationSource.Overrides(_foreignKeyPropertiesConfigurationSource.Value)
                && !CanSetRequiredOnProperties(Metadata.Properties, isRequired, configurationSource, shouldThrow: false))
            {
                return false;
            }

            return true;
        }

        private bool CanSetRequiredOnProperties(IEnumerable<Property> properties, bool? isRequired, ConfigurationSource configurationSource, bool shouldThrow)
            => CanSetRequiredOnProperties(
                properties,
                isRequired,
                Metadata.DeclaringEntityType,
                ModelBuilder,
                configurationSource,
                shouldThrow);

        private static bool CanSetRequiredOnProperties(
            IEnumerable<Property> properties,
            bool? isRequired,
            EntityType entityType,
            InternalModelBuilder modelBuilder,
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
                entityType, shouldThrow))
            {
                return false;
            }

            var nullableProperties = properties.Where(p => ((IProperty)p).ClrType.IsNullableType());
            var propertyBuilders = InternalEntityTypeBuilder.GetPropertyBuilders(modelBuilder, nullableProperties, ConfigurationSource.Convention);

            return isRequired.Value
                ? propertyBuilders.All(property => property.CanSetRequired(true, configurationSource))
                : propertyBuilders.Any(property => property.CanSetRequired(false, configurationSource));
        }

        public virtual InternalRelationshipBuilder DeleteBehavior(DeleteBehavior deleteBehavior, ConfigurationSource configurationSource, bool runConventions = true)
        {
            if (((IForeignKey)Metadata).DeleteBehavior == deleteBehavior)
            {
                Metadata.DeleteBehavior = deleteBehavior;
                _deleteBehaviorConfigurationSource = configurationSource.Max(_deleteBehaviorConfigurationSource);
                return this;
            }

            if (_deleteBehaviorConfigurationSource != null
                && !configurationSource.Overrides(_deleteBehaviorConfigurationSource.Value))
            {
                return null;
            }

            if (runConventions)
            {
                return ReplaceForeignKey(ForeignKeyAspect.DeleteBehavior, configurationSource, deleteBehavior: deleteBehavior);
            }

            _deleteBehaviorConfigurationSource = configurationSource.Max(_deleteBehaviorConfigurationSource);
            Metadata.DeleteBehavior = deleteBehavior;
            return this;
        }

        public virtual bool CanSetDeleteBehavior(DeleteBehavior deleteBehavior, ConfigurationSource configurationSource)
        {
            if (((IForeignKey)Metadata).DeleteBehavior == deleteBehavior)
            {
                return true;
            }

            if (_deleteBehaviorConfigurationSource != null
                && !configurationSource.Overrides(_deleteBehaviorConfigurationSource.Value))
            {
                return false;
            }

            return true;
        }

        public virtual InternalRelationshipBuilder IsUnique(bool isUnique, ConfigurationSource configurationSource, bool runConventions = true)
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
            if (Metadata.PrincipalToDependent != null
                && !Navigation.IsCompatible(
                    Metadata.PrincipalToDependent.Name,
                    Metadata.PrincipalEntityType,
                    Metadata.DeclaringEntityType,
                    !isUnique,
                    shouldThrow: false))
            {
                builder = ModelBuilder.Entity(Metadata.PrincipalEntityType.Name, ConfigurationSource.Convention)
                    .Navigation(null, Metadata, false, configurationSource);
                if (builder == null)
                {
                    return null;
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

        public virtual InternalRelationshipBuilder Invert(ConfigurationSource configurationSource)
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
                    throw new InvalidOperationException(CoreStrings.RelationshipCannotBeInverted);
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
                runConventions: true);
        }

        private bool CanInvert(ConfigurationSource configurationSource)
        {
            if (!CanSetUnique(true, configurationSource))
            {
                return false;
            }

            if ((_foreignKeyPropertiesConfigurationSource != null && _foreignKeyPropertiesConfigurationSource.Value.Overrides(configurationSource))
                || (_principalKeyConfigurationSource != null && _principalKeyConfigurationSource.Value.Overrides(configurationSource))
                || (_principalEndConfigurationSource != null && !configurationSource.Overrides(_principalEndConfigurationSource.Value)))
            {
                return false;
            }

            return true;
        }

        public virtual InternalRelationshipBuilder DependentEnd(
            [NotNull] EntityType dependentEntityType, ConfigurationSource configurationSource, bool runConventions = true)
        {
            Check.NotNull(dependentEntityType, nameof(dependentEntityType));

            if (Metadata.DeclaringEntityType == dependentEntityType)
            {
                _principalEndConfigurationSource = configurationSource.Max(_principalEndConfigurationSource);
                return this;
            }

            if (!runConventions)
            {
                return null;
            }

            if (Metadata.PrincipalEntityType != dependentEntityType)
            {
                if (configurationSource == ConfigurationSource.Explicit)
                {
                    throw new ArgumentException(CoreStrings.EntityTypeNotInRelationship(
                        dependentEntityType.DisplayName(),
                        Metadata.DeclaringEntityType.DisplayName(),
                        Metadata.PrincipalEntityType.DisplayName()));
                }

                return null;
            }

            return Invert(configurationSource);
        }

        public virtual InternalRelationshipBuilder PrincipalEnd(
            [NotNull] EntityType principalEntityType, ConfigurationSource configurationSource, bool runConventions = true)
        {
            Check.NotNull(principalEntityType, nameof(principalEntityType));

            if (Metadata.PrincipalEntityType == principalEntityType)
            {
                _principalEndConfigurationSource = configurationSource.Max(_principalEndConfigurationSource);
                return this;
            }

            if (!runConventions)
            {
                return null;
            }

            if (Metadata.DeclaringEntityType != principalEntityType)
            {
                if (configurationSource == ConfigurationSource.Explicit)
                {
                    throw new ArgumentException(CoreStrings.EntityTypeNotInRelationship(
                        principalEntityType.DisplayName(),
                        Metadata.DeclaringEntityType.DisplayName(),
                        Metadata.PrincipalEntityType.DisplayName()));
                }

                return null;
            }

            return Invert(configurationSource);
        }

        public virtual InternalRelationshipBuilder DependentType(
            [NotNull] EntityType dependentEntityType, ConfigurationSource configurationSource)
        {
            if (Metadata.DeclaringEntityType.IsAssignableFrom(dependentEntityType))
            {
                return this;
            }

            if (!dependentEntityType.IsAssignableFrom(Metadata.DeclaringEntityType))
            {
                if (configurationSource == ConfigurationSource.Explicit)
                {
                    throw new InvalidOperationException();
                }
            }

            return ReplaceForeignKey(ForeignKeyAspect.None,
                configurationSource,
                dependentEntityTypeBuilder: ModelBuilder.Entity(dependentEntityType.Name, ConfigurationSource.Convention));
        }

        public virtual InternalRelationshipBuilder PrincipalType(
            [NotNull] EntityType principalEntityType, ConfigurationSource configurationSource)
        {
            if (Metadata.PrincipalEntityType.IsAssignableFrom(principalEntityType))
            {
                return this;
            }

            if (!principalEntityType.IsAssignableFrom(Metadata.PrincipalEntityType))
            {
                if (configurationSource == ConfigurationSource.Explicit)
                {
                    throw new InvalidOperationException();
                }
            }

            return ReplaceForeignKey(ForeignKeyAspect.None,
                configurationSource,
                principalEntityTypeBuilder: ModelBuilder.Entity(principalEntityType.Name, ConfigurationSource.Convention));
        }

        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] IReadOnlyList<PropertyInfo> properties, ConfigurationSource configurationSource)
            => HasForeignKey(
                ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, configurationSource)
                    .GetOrCreateProperties(properties, configurationSource),
                configurationSource);

        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => HasForeignKey(
                ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, configurationSource)
                    .GetOrCreateProperties(propertyNames, configurationSource),
                configurationSource);

        public virtual InternalRelationshipBuilder HasForeignKey(
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource, bool runConventions = true)
        {
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

            var resetIsRequired = false;
            if (_isRequiredConfigurationSource.HasValue
                && !CanSetRequiredOnProperties(properties, ((IForeignKey)Metadata).IsRequired, configurationSource, shouldThrow: false))
            {
                if (!configurationSource.Overrides(_isRequiredConfigurationSource.Value))
                {
                    return null;
                }
                resetIsRequired = true;
            }

            if (_principalKeyConfigurationSource.HasValue
                && !Entity.Metadata.ForeignKey.AreCompatible(
                    Metadata.PrincipalKey.Properties,
                    properties,
                    Metadata.PrincipalEntityType,
                    Metadata.DeclaringEntityType,
                    shouldThrow: false))
            {
                if (!configurationSource.Overrides(_principalKeyConfigurationSource.Value))
                {
                    return null;
                }
                _principalKeyConfigurationSource = null;
            }

            if (resetIsRequired)
            {
                _isRequiredConfigurationSource = null;
            }

            return ReplaceForeignKey(
                ForeignKeyAspect.DependentProperties,
                configurationSource,
                dependentProperties: properties,
                principalProperties: runConventions ? null : Metadata.PrincipalKey.Properties,
                runConventions: runConventions);
        }

        private bool CanSetForeignKey(IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            if (properties == null)
            {
                return false;
            }

            if (Metadata.Properties.SequenceEqual(properties))
            {
                return true;
            }

            if (_foreignKeyPropertiesConfigurationSource != null
                && !configurationSource.Overrides(_foreignKeyPropertiesConfigurationSource.Value))
            {
                return false;
            }

            if (_isRequiredConfigurationSource.HasValue
                && !configurationSource.Overrides(_isRequiredConfigurationSource.Value)
                && !CanSetRequiredOnProperties(properties, ((IForeignKey)Metadata).IsRequired, configurationSource, shouldThrow: false))
            {
                return false;
            }

            if (_principalKeyConfigurationSource.HasValue
                && !configurationSource.Overrides(_principalKeyConfigurationSource.Value)
                && !Entity.Metadata.ForeignKey.AreCompatible(
                    Metadata.PrincipalKey.Properties,
                    properties,
                    Metadata.PrincipalEntityType,
                    Metadata.DeclaringEntityType,
                    shouldThrow: false))
            {
                return false;
            }

            return true;
        }

        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] Type dependentType,
            [NotNull] IReadOnlyList<PropertyInfo> properties,
            ConfigurationSource configurationSource)
            => DependentEnd(ModelBuilder.Metadata.GetEntityType(dependentType), configurationSource)
                ?.HasForeignKey(properties, configurationSource);

        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] Type dependentType,
            [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
            => DependentEnd(ModelBuilder.Metadata.GetEntityType(dependentType), configurationSource)
                ?.HasForeignKey(propertyNames, configurationSource);

        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] string dependentTypeName,
            [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
            => DependentEnd(ModelBuilder.Metadata.GetEntityType(dependentTypeName), configurationSource)
                ?.HasForeignKey(propertyNames, configurationSource);

        public virtual InternalRelationshipBuilder HasPrincipalKey([NotNull] IReadOnlyList<PropertyInfo> properties,
            ConfigurationSource configurationSource)
            => HasPrincipalKey(
                ModelBuilder.Entity(Metadata.PrincipalEntityType.Name, configurationSource)
                    .GetOrCreateProperties(properties, configurationSource),
                configurationSource);

        public virtual InternalRelationshipBuilder HasPrincipalKey([NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
            => HasPrincipalKey(
                ModelBuilder.Entity(Metadata.PrincipalEntityType.Name, configurationSource)
                    .GetOrCreateProperties(propertyNames, configurationSource),
                configurationSource);

        public virtual InternalRelationshipBuilder HasPrincipalKey(
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource, bool runConventions = true)
        {
            if (properties == null)
            {
                return null;
            }

            if (Metadata.PrincipalKey.Properties.SequenceEqual(properties))
            {
                _principalKeyConfigurationSource = configurationSource.Max(_principalKeyConfigurationSource);
                var principalEntityTypeBuilder = ModelBuilder.Entity(Metadata.PrincipalKey.DeclaringEntityType.Name, configurationSource);
                principalEntityTypeBuilder.HasKey(properties.Select(p => p.Name).ToList(), configurationSource);
                return this;
            }

            if (_principalKeyConfigurationSource != null
                && !configurationSource.Overrides(_principalKeyConfigurationSource.Value))
            {
                return null;
            }

            if (_foreignKeyPropertiesConfigurationSource.HasValue
                && !Entity.Metadata.ForeignKey.AreCompatible(
                    properties,
                    Metadata.Properties,
                    Metadata.PrincipalEntityType,
                    Metadata.DeclaringEntityType,
                    shouldThrow: false))
            {
                if (!configurationSource.Overrides(_foreignKeyPropertiesConfigurationSource.Value))
                {
                    return null;
                }
                _foreignKeyPropertiesConfigurationSource = null;
            }

            return ReplaceForeignKey(
                ForeignKeyAspect.PrincipalKey,
                configurationSource,
                principalProperties: properties,
                runConventions: runConventions);
        }

        private bool CanSetPrincipalKey(IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            if (properties == null)
            {
                return false;
            }

            if (Metadata.PrincipalKey.Properties.SequenceEqual(properties))
            {
                return true;
            }

            if (_principalKeyConfigurationSource != null
                && !configurationSource.Overrides(_principalKeyConfigurationSource.Value))
            {
                return false;
            }

            if (_foreignKeyPropertiesConfigurationSource.HasValue
                && !configurationSource.Overrides(_foreignKeyPropertiesConfigurationSource.Value)
                && !Entity.Metadata.ForeignKey.AreCompatible(
                    properties,
                    Metadata.Properties,
                    Metadata.PrincipalEntityType,
                    Metadata.DeclaringEntityType,
                    shouldThrow: false))
            {
                return false;
            }

            return true;
        }

        public virtual InternalRelationshipBuilder HasPrincipalKey(
            [NotNull] Type principalType,
            [NotNull] IReadOnlyList<PropertyInfo> properties,
            ConfigurationSource configurationSource)
            => PrincipalEnd(ModelBuilder.Metadata.GetEntityType(principalType), configurationSource)
                ?.HasPrincipalKey(properties, configurationSource);

        public virtual InternalRelationshipBuilder HasPrincipalKey(
            [NotNull] Type principalType,
            [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
            => PrincipalEnd(ModelBuilder.Metadata.GetEntityType(principalType), configurationSource)
                ?.HasPrincipalKey(propertyNames, configurationSource);

        public virtual InternalRelationshipBuilder HasPrincipalKey(
            [NotNull] string principalTypeName,
            [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
            => PrincipalEnd(ModelBuilder.Metadata.GetEntityType(principalTypeName), configurationSource)
                ?.HasPrincipalKey(propertyNames, configurationSource);

        private InternalRelationshipBuilder ReplaceForeignKey(
            ForeignKeyAspect aspectsConfigured,
            ConfigurationSource configurationSource,
            InternalEntityTypeBuilder principalEntityTypeBuilder = null,
            InternalEntityTypeBuilder dependentEntityTypeBuilder = null,
            IReadOnlyList<Property> dependentProperties = null,
            IReadOnlyList<Property> principalProperties = null,
            bool? isUnique = null,
            bool? isRequired = null,
            DeleteBehavior? deleteBehavior = null,
            bool runConventions = true)
        {
            principalEntityTypeBuilder = principalEntityTypeBuilder ?? ModelBuilder.Entity(Metadata.PrincipalEntityType.Name, ConfigurationSource.Convention);
            dependentEntityTypeBuilder = dependentEntityTypeBuilder ?? ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, ConfigurationSource.Convention);

            dependentProperties = dependentProperties ??
                                  (_foreignKeyPropertiesConfigurationSource.HasValue
                                   && _foreignKeyPropertiesConfigurationSource.Value.Overrides(configurationSource)
                                      ? Metadata.Properties
                                      : null);

            principalProperties = principalProperties ??
                                  (_principalKeyConfigurationSource.HasValue
                                   && _principalKeyConfigurationSource.Value.Overrides(configurationSource)
                                      ? Metadata.PrincipalKey.Properties
                                      : null);

            var dependentToPrincipal = (_dependentToPrincipalConfigurationSource.HasValue
                                        && _dependentToPrincipalConfigurationSource.Value.Overrides(configurationSource))
                ? Metadata.DependentToPrincipal?.Name ?? ""
                : null;

            var principalToDependent = (_principalToDependentConfigurationSource.HasValue
                                        && _principalToDependentConfigurationSource.Value.Overrides(configurationSource))
                ? Metadata.PrincipalToDependent?.Name ?? ""
                : null;

            isUnique = isUnique ??
                       (_isUniqueConfigurationSource.HasValue
                        && _isUniqueConfigurationSource.Value.Overrides(configurationSource)
                           ? ((IForeignKey)Metadata).IsUnique
                           : (bool?)null);

            isRequired = isRequired ??
                         (_isRequiredConfigurationSource.HasValue
                          && _isRequiredConfigurationSource.Value.Overrides(configurationSource)
                             ? ((IForeignKey)Metadata).IsRequired
                             : (bool?)null);

            deleteBehavior = deleteBehavior ??
                             (_deleteBehaviorConfigurationSource.HasValue
                              && _deleteBehaviorConfigurationSource.Value.Overrides(configurationSource)
                                 ? ((IForeignKey)Metadata).DeleteBehavior
                                 : (DeleteBehavior?)null);

            return ReplaceForeignKey(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                dependentToPrincipal,
                principalToDependent,
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
                principalEntityTypeBuilder.Metadata,
                dependentEntityTypeBuilder.Metadata,
                navigationToPrincipalName,
                navigationToDependentName,
                dependentProperties,
                principalProperties,
                isUnique,
                isRequired,
                ModelBuilder,
                configurationSource))
            {
                return null;
            }

            var oldNavigationToPrincipalName = Metadata.DependentToPrincipal?.Name;
            var oldNavigationToDependentName = Metadata.PrincipalToDependent?.Name;

            var replacedConfigurationSource = ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, ConfigurationSource.Convention)
                .RemoveForeignKey(Metadata, ConfigurationSource.Explicit);

            var shouldUpgradeSource = aspectsConfigured.HasFlag(ForeignKeyAspect.DependentProperties)
                                      || navigationToPrincipalName != null
                                      || navigationToDependentName != null;

            return !replacedConfigurationSource.HasValue
                ? null
                : AddRelationship(
                    principalEntityTypeBuilder,
                    dependentEntityTypeBuilder,
                    navigationToPrincipalName,
                    navigationToDependentName,
                    dependentProperties,
                    principalProperties,
                    isUnique,
                    isRequired,
                    deleteBehavior,
                    oldNavigationToPrincipalName,
                    oldNavigationToDependentName,
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
            string oldDependentToPrincipal,
            string oldPrincipalToDependent,
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
                strictPrincipal: true, // TODO: false if principal end not being configured
                onRelationshipAdding: b =>
                    MergeConfigurationSourceWith(b, this, oldDependentToPrincipal, oldPrincipalToDependent, aspectsConfigured, aspectConfigurationSource),
                runConventions: runConventions);
        }

        public virtual InternalRelationshipBuilder Attach(
            [CanBeNull] string dependentToPrincipalName,
            [CanBeNull] string principalToDependentName,
            ConfigurationSource configurationSource)
        {
            if (Metadata.DeclaringEntityType.GetForeignKeys().Contains(Metadata))
            {
                return ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, configurationSource)
                    .Relationship(Metadata, existingForeignKey: true, configurationSource: configurationSource);
            }

            // Only restore explicit configuration. The rest will be handled by the conventions.
            var aspectsConfigured = ForeignKeyAspect.None;
            IReadOnlyList<Property> foreignKeyProperties = null;
            if (_foreignKeyPropertiesConfigurationSource.HasValue)
            {
                foreignKeyProperties = GetExistingProperties(Metadata.Properties, Metadata.DeclaringEntityType);
                if (foreignKeyProperties == null)
                {
                    _foreignKeyPropertiesConfigurationSource = null;
                }
                else if (!_foreignKeyPropertiesConfigurationSource.Value.Overrides(ConfigurationSource.Explicit))
                {
                    foreignKeyProperties = null;
                }
                else
                {
                    aspectsConfigured |= ForeignKeyAspect.DependentProperties;
                }
            }

            //TODO: Remove aspects
            Key principalKey = null;
            if (_principalKeyConfigurationSource.HasValue)
            {
                principalKey = Metadata.PrincipalEntityType.FindKey(Metadata.PrincipalKey.Properties);
                if (principalKey == null)
                {
                    _principalKeyConfigurationSource = null;
                }
                else if (!_principalKeyConfigurationSource.Value.Overrides(ConfigurationSource.Explicit))
                {
                    principalKey = null;
                }
                else
                {
                    aspectsConfigured |= ForeignKeyAspect.PrincipalKey;
                }
            }

            bool? isUnique = null;
            if (_isUniqueConfigurationSource.HasValue
                && _isUniqueConfigurationSource.Value.Overrides(ConfigurationSource.Explicit))
            {
                isUnique = Metadata.IsUnique;
                aspectsConfigured |= ForeignKeyAspect.IsUnique;
            }

            bool? isRequired = null;
            if (_isRequiredConfigurationSource.HasValue
                && _isRequiredConfigurationSource.Value.Overrides(ConfigurationSource.Explicit))
            {
                isRequired = Metadata.IsRequired;
                aspectsConfigured |= ForeignKeyAspect.IsRequired;
            }

            DeleteBehavior? deleteBehavior = null;
            if (_deleteBehaviorConfigurationSource.HasValue
                && _deleteBehaviorConfigurationSource.Value.Overrides(ConfigurationSource.Explicit))
            {
                deleteBehavior = Metadata.DeleteBehavior;
                aspectsConfigured |= ForeignKeyAspect.DeleteBehavior;
            }

            return AddRelationship(
                ModelBuilder.Entity(Metadata.PrincipalEntityType.Name, configurationSource),
                ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, configurationSource),
                dependentToPrincipalName,
                principalToDependentName,
                foreignKeyProperties,
                principalKey?.Properties,
                isUnique,
                isRequired,
                deleteBehavior,
                dependentToPrincipalName,
                principalToDependentName,
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
            string oldDependentToPrincipal,
            string oldPrincipalToDependent,
            ForeignKeyAspect aspectsConfigured,
            ConfigurationSource configurationSource)
        {
            var newFk = newBuilder.Metadata;
            if (aspectsConfigured.HasFlag(ForeignKeyAspect.PrincipalEnd)
                || aspectsConfigured.HasFlag(ForeignKeyAspect.DependentProperties)
                || aspectsConfigured.HasFlag(ForeignKeyAspect.PrincipalKey))
            {
                newBuilder._principalEndConfigurationSource = configurationSource
                    .Max(newBuilder._principalEndConfigurationSource)
                    .Max(oldBuilder._principalEndConfigurationSource);
            }
            else
            {
                newBuilder._principalEndConfigurationSource = oldBuilder._principalEndConfigurationSource?
                    .Max(newBuilder._principalEndConfigurationSource)
                                                              ?? newBuilder._principalEndConfigurationSource;
            }

            var inverted = oldBuilder.Metadata.DeclaringEntityType != newBuilder.Metadata.DeclaringEntityType
                           || (newBuilder.Metadata.IsSelfReferencing()
                               && aspectsConfigured.HasFlag(ForeignKeyAspect.PrincipalEnd));

            if (aspectsConfigured.HasFlag(ForeignKeyAspect.DependentProperties))
            {
                newBuilder = newBuilder.HasForeignKey(
                    newFk.Properties, configurationSource, runConventions: false);
            }
            if (aspectsConfigured.HasFlag(ForeignKeyAspect.PrincipalKey))
            {
                newBuilder = newBuilder.HasPrincipalKey(
                    newFk.PrincipalKey.Properties, configurationSource, runConventions: false);
            }
            if (aspectsConfigured.HasFlag(ForeignKeyAspect.IsUnique))
            {
                newBuilder = newBuilder.IsUnique(
                    newFk.IsUnique.Value, configurationSource, runConventions: false);
            }
            if (aspectsConfigured.HasFlag(ForeignKeyAspect.IsRequired))
            {
                newBuilder = newBuilder.IsRequired(
                    newFk.IsRequired.Value, configurationSource, runConventions: false);
            }
            if (aspectsConfigured.HasFlag(ForeignKeyAspect.DeleteBehavior))
            {
                newBuilder = newBuilder.DeleteBehavior(
                    newFk.DeleteBehavior.Value, configurationSource, runConventions: false);
            }

            if (!aspectsConfigured.HasFlag(ForeignKeyAspect.PrincipalKey)
                && !inverted
                && oldBuilder._principalKeyConfigurationSource.HasValue
                && (!newBuilder._principalKeyConfigurationSource.HasValue // TODO: Remove these checks
                    || oldBuilder._principalKeyConfigurationSource.Value.Overrides(
                        newBuilder._principalKeyConfigurationSource.Value)))
            {
                newBuilder = newBuilder.HasPrincipalKey(
                    oldBuilder.Metadata.PrincipalKey.Properties, oldBuilder._principalKeyConfigurationSource.Value, runConventions: false)
                             ?? newBuilder;
            }

            if (!aspectsConfigured.HasFlag(ForeignKeyAspect.DependentProperties)
                && !inverted
                && oldBuilder._foreignKeyPropertiesConfigurationSource.HasValue
                && (!newBuilder._foreignKeyPropertiesConfigurationSource.HasValue
                    || oldBuilder._foreignKeyPropertiesConfigurationSource.Value.Overrides(newBuilder._foreignKeyPropertiesConfigurationSource.Value)))
            {
                newBuilder = newBuilder.HasForeignKey(
                    oldBuilder.Metadata.Properties, oldBuilder._foreignKeyPropertiesConfigurationSource.Value, runConventions: false)
                             ?? newBuilder;
            }

            if (!aspectsConfigured.HasFlag(ForeignKeyAspect.IsUnique)
                && oldBuilder._isUniqueConfigurationSource.HasValue
                && oldBuilder.Metadata.IsUnique.HasValue
                && (!newBuilder._isUniqueConfigurationSource.HasValue
                    || oldBuilder._isUniqueConfigurationSource.Value.Overrides(
                        newBuilder._isUniqueConfigurationSource.Value)))
            {
                newBuilder = newBuilder.IsUnique(
                    oldBuilder.Metadata.IsUnique.Value, oldBuilder._isUniqueConfigurationSource.Value, runConventions: false)
                             ?? newBuilder;
            }

            if (!aspectsConfigured.HasFlag(ForeignKeyAspect.IsRequired)
                && oldBuilder._isRequiredConfigurationSource.HasValue
                && oldBuilder.Metadata.IsRequired.HasValue
                && (!newBuilder._isRequiredConfigurationSource.HasValue
                    || oldBuilder._isRequiredConfigurationSource.Value.Overrides(
                        newBuilder._isRequiredConfigurationSource.Value)))
            {
                newBuilder = newBuilder.IsRequired(oldBuilder.Metadata.IsRequired.Value, oldBuilder._isRequiredConfigurationSource.Value, runConventions: false)
                             ?? newBuilder;
            }

            if (!aspectsConfigured.HasFlag(ForeignKeyAspect.DeleteBehavior)
                && oldBuilder._deleteBehaviorConfigurationSource.HasValue
                && oldBuilder.Metadata.DeleteBehavior.HasValue
                && (!newBuilder._deleteBehaviorConfigurationSource.HasValue
                    || oldBuilder._deleteBehaviorConfigurationSource.Value.Overrides(
                        newBuilder._deleteBehaviorConfigurationSource.Value)))
            {
                newBuilder = newBuilder.DeleteBehavior(oldBuilder.Metadata.DeleteBehavior.Value, oldBuilder._deleteBehaviorConfigurationSource.Value, runConventions: false)
                             ?? newBuilder;
            }

            if (oldBuilder._dependentToPrincipalConfigurationSource.HasValue)
            {
                if (inverted)
                {
                    if (!newBuilder._principalToDependentConfigurationSource.HasValue
                        || oldBuilder._dependentToPrincipalConfigurationSource.Value.Overrides(
                            newBuilder._principalToDependentConfigurationSource.Value))
                    {
                        newBuilder = newBuilder.PrincipalToDependent(
                            oldDependentToPrincipal, oldBuilder._dependentToPrincipalConfigurationSource.Value, runConventions: false)
                                     ?? newBuilder;
                    }
                }
                else
                {
                    if (!newBuilder._dependentToPrincipalConfigurationSource.HasValue
                        || oldBuilder._dependentToPrincipalConfigurationSource.Value.Overrides(
                            newBuilder._dependentToPrincipalConfigurationSource.Value))
                    {
                        newBuilder = newBuilder.DependentToPrincipal(
                            oldDependentToPrincipal, oldBuilder._dependentToPrincipalConfigurationSource.Value, runConventions: false)
                                     ?? newBuilder;
                    }
                }
            }

            if (oldBuilder._principalToDependentConfigurationSource.HasValue)
            {
                if (inverted)
                {
                    if (!newBuilder._dependentToPrincipalConfigurationSource.HasValue
                        || oldBuilder._principalToDependentConfigurationSource.Value.Overrides(
                            newBuilder._dependentToPrincipalConfigurationSource.Value))
                    {
                        newBuilder = newBuilder.DependentToPrincipal(
                            oldPrincipalToDependent, oldBuilder._principalToDependentConfigurationSource.Value, runConventions: false)
                                     ?? newBuilder;
                    }
                }
                else
                {
                    if (!newBuilder._principalToDependentConfigurationSource.HasValue
                        || oldBuilder._principalToDependentConfigurationSource.Value.Overrides(
                            newBuilder._principalToDependentConfigurationSource.Value))
                    {
                        newBuilder = newBuilder.PrincipalToDependent(
                            oldPrincipalToDependent, oldBuilder._principalToDependentConfigurationSource.Value, runConventions: false)
                                     ?? newBuilder;
                    }
                }
            }

            return newBuilder;
        }

        public static bool AreCompatible(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            [CanBeNull] IReadOnlyList<Property> dependentProperties,
            [CanBeNull] IReadOnlyList<Property> principalProperties,
            bool? isUnique,
            bool? isRequired,
            [NotNull] InternalModelBuilder modelBuilder,
            ConfigurationSource configurationSource)
        {
            var shouldThrow = configurationSource == ConfigurationSource.Explicit;
            if (dependentProperties != null
                && !CanSetRequiredOnProperties(
                    dependentProperties,
                    isRequired,
                    dependentEntityType,
                    modelBuilder,
                    configurationSource,
                    shouldThrow))
            {
                return false;
            }

            return Entity.Metadata.ForeignKey.AreCompatible(
                principalEntityType,
                dependentEntityType,
                navigationToPrincipalName,
                navigationToDependentName,
                dependentProperties,
                principalProperties,
                isUnique,
                isRequired,
                shouldThrow);
        }

        public virtual bool CanSet(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            [CanBeNull] IReadOnlyList<Property> dependentProperties,
            [CanBeNull] IReadOnlyList<Property> principalProperties,
            bool? isUnique,
            bool? isRequired,
            DeleteBehavior? deleteBehavior,
            bool strictPrincipal,
            ConfigurationSource configurationSource,
            out bool inverted)
        {
            Debug.Assert(AreCompatible(
                principalEntityType,
                dependentEntityType,
                navigationToPrincipalName,
                navigationToDependentName,
                dependentProperties,
                principalProperties,
                isUnique,
                isRequired,
                ModelBuilder,
                configurationSource));

            inverted = false;
            if ((!dependentEntityType.IsAssignableFrom(Metadata.DeclaringEntityType)
                 && !Metadata.DeclaringEntityType.IsAssignableFrom(dependentEntityType))
                || (!principalEntityType.IsAssignableFrom(Metadata.PrincipalEntityType)
                    && !Metadata.PrincipalEntityType.IsAssignableFrom(principalEntityType)))
            {
                if ((dependentEntityType.IsAssignableFrom(Metadata.PrincipalEntityType)
                     || Metadata.PrincipalEntityType.IsAssignableFrom(dependentEntityType))
                    && (principalEntityType.IsAssignableFrom(Metadata.DeclaringEntityType)
                        || Metadata.DeclaringEntityType.IsAssignableFrom(principalEntityType))
                    && (CanInvert(configurationSource) || !strictPrincipal))
                {
                    inverted = true;
                }
                else
                {
                    return false;
                }
            }

            if (principalEntityType == dependentEntityType
                && ((navigationToPrincipalName != null
                     && navigationToPrincipalName == Metadata.PrincipalToDependent?.Name)
                    || (navigationToDependentName != null
                        && navigationToDependentName == Metadata.DependentToPrincipal?.Name))
                && (CanInvert(configurationSource) || !strictPrincipal))
            {
                inverted = true;
            }

            if (inverted)
            {
                var navigationName = navigationToPrincipalName;
                navigationToPrincipalName = navigationToDependentName;
                navigationToDependentName = navigationName;

                var properties = dependentProperties;
                dependentProperties = principalProperties;
                principalProperties = properties;
            }

            var currentOwner = ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, ConfigurationSource.Convention);

            if (navigationToPrincipalName != null
                && navigationToPrincipalName != Metadata.DependentToPrincipal?.Name
                && !currentOwner.CanRemove(Metadata, configurationSource))
            {
                return false;
            }

            if (navigationToDependentName != null
                && navigationToDependentName != Metadata.PrincipalToDependent?.Name
                && !currentOwner.CanRemove(Metadata, configurationSource))
            {
                return false;
            }

            if (dependentProperties != null
                && !CanSetForeignKey(dependentProperties, configurationSource))
            {
                return false;
            }

            if (principalProperties != null
                && !CanSetPrincipalKey(principalProperties, configurationSource))
            {
                return false;
            }

            if (isUnique.HasValue
                && !CanSetUnique(isUnique.Value, configurationSource))
            {
                return false;
            }

            if (isRequired.HasValue
                && !CanSetRequired(isRequired.Value, configurationSource))
            {
                return false;
            }

            if (deleteBehavior.HasValue
                && !CanSetDeleteBehavior(deleteBehavior.Value, configurationSource))
            {
                return false;
            }

            return true;
        }

        [Flags]
        private enum ForeignKeyAspect
        {
            None = 0,
            DependentType = 1 << 0,
            PrincipalType = 1 << 1,
            PrincipalEnd = 1 << 2,
            DependentProperties = 1 << 3,
            PrincipalKey = 1 << 4,
            IsUnique = 1 << 5,
            IsRequired = 1 << 6,
            DeleteBehavior = 1 << 7
        }
    }
}
