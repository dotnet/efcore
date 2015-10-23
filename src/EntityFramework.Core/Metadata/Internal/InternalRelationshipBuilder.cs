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
            ConfigurationSource configurationSource)
            => Navigation(
                navigationToPrincipalName,
                pointsToPrincipal: true,
                configurationSource: configurationSource,
                runConventions: true);

        public virtual InternalRelationshipBuilder PrincipalToDependent(
            [CanBeNull] string navigationToDependentName,
            ConfigurationSource configurationSource)
            => Navigation(
                navigationToDependentName,
                pointsToPrincipal: false,
                configurationSource: configurationSource,
                runConventions: true);

        public virtual InternalRelationshipBuilder Navigations(
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            ConfigurationSource configurationSource)
        {
            bool _;
            if (!CanSet(Metadata.PrincipalEntityType,
                Metadata.DeclaringEntityType,
                navigationToPrincipalName,
                navigationToDependentName,
                null,
                null,
                null,
                null,
                null,
                false,
                configurationSource,
                out _))
            {
                return null;
            }

            return ReplaceForeignKey(configurationSource,
                navigationToPrincipalName: navigationToPrincipalName ?? "",
                navigationToDependentName: navigationToDependentName ?? "");
        }

        private InternalRelationshipBuilder Navigation(
            [CanBeNull] string navigationName,
            bool pointsToPrincipal,
            ConfigurationSource configurationSource,
            bool runConventions)
        {
            var oldNavigation = pointsToPrincipal ? Metadata.DependentToPrincipal : Metadata.PrincipalToDependent;
            if (navigationName == oldNavigation?.Name)
            {
                if (pointsToPrincipal)
                {
                    ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, configurationSource);
                    _dependentToPrincipalConfigurationSource = configurationSource.Max(
                        _dependentToPrincipalConfigurationSource);
                }
                else
                {
                    ModelBuilder.Entity(Metadata.PrincipalEntityType.Name, configurationSource);
                    _principalToDependentConfigurationSource = configurationSource.Max(
                        _principalToDependentConfigurationSource);
                }
                return this;
            }

            if (pointsToPrincipal
                && _dependentToPrincipalConfigurationSource.HasValue
                && !configurationSource.Overrides(_dependentToPrincipalConfigurationSource.Value))
            {
                return null;
            }
            if (!pointsToPrincipal
                && _principalToDependentConfigurationSource.HasValue
                && !configurationSource.Overrides(_principalToDependentConfigurationSource.Value))
            {
                return null;
            }

            var entityType = pointsToPrincipal ? Metadata.DeclaringEntityType : Metadata.PrincipalEntityType;
            var entityTypeBuilder = ModelBuilder.Entity(entityType.Name, ConfigurationSource.Convention);

            var strictPrincipal = false;
            bool? shouldBeUnique = null;
            var uniquenessConfigurationSource = ConfigurationSource.Convention;
            if (navigationName != null)
            {
                if (entityTypeBuilder.IsIgnored(navigationName, configurationSource))
                {
                    return null;
                }

                if (!pointsToPrincipal)
                {
                    var canBeUnique = Entity.Metadata.Navigation.IsCompatible(
                        navigationName,
                        Metadata.PrincipalEntityType,
                        Metadata.DeclaringEntityType,
                        shouldBeCollection: false,
                        shouldThrow: false);
                    var canBeNonUnique = Entity.Metadata.Navigation.IsCompatible(
                        navigationName,
                        Metadata.PrincipalEntityType,
                        Metadata.DeclaringEntityType,
                        shouldBeCollection: true,
                        shouldThrow: false);

                    if (canBeUnique != canBeNonUnique)
                    {
                        if (_isUniqueConfigurationSource.HasValue
                            && !configurationSource.Overrides(_isUniqueConfigurationSource.Value)
                            && ((IForeignKey)Metadata).IsUnique != canBeUnique)
                        {
                            return null;
                        }

                        shouldBeUnique = canBeUnique;
                        if (canBeUnique)
                        {
                            uniquenessConfigurationSource = (_principalEndConfigurationSource ?? ConfigurationSource.Convention)
                                .Max(_isUniqueConfigurationSource);
                        }
                        else
                        {
                            strictPrincipal = true;
                            uniquenessConfigurationSource = configurationSource;
                        }
                    }
                    else if (!canBeUnique)
                    {
                        Entity.Metadata.Navigation.IsCompatible(
                            navigationName,
                            Metadata.PrincipalEntityType,
                            Metadata.DeclaringEntityType,
                            shouldBeCollection: false,
                            shouldThrow: configurationSource == ConfigurationSource.Explicit);

                        return null;
                    }
                }
                else if (!Entity.Metadata.Navigation.IsCompatible(
                    navigationName,
                    Metadata.DeclaringEntityType,
                    Metadata.PrincipalEntityType,
                    shouldBeCollection: false,
                    shouldThrow: configurationSource == ConfigurationSource.Explicit))
                {
                    return null;
                }
            }

            var conflictingNavigation = navigationName == null
                ? null
                : entityTypeBuilder.Metadata.FindNavigationsInHierarchy(navigationName).FirstOrDefault();

            var builder = this;
            if (conflictingNavigation?.ForeignKey == Metadata)
            {
                Debug.Assert(conflictingNavigation.PointsToPrincipal() != pointsToPrincipal);

                builder = builder.Navigation(null, conflictingNavigation.PointsToPrincipal(), configurationSource, runConventions: runConventions);
            }
            else
            {
                Debug.Assert(conflictingNavigation == null || runConventions);
            }

            if (shouldBeUnique.HasValue)
            {
                builder = builder.IsUnique(shouldBeUnique.Value, uniquenessConfigurationSource, runConventions: runConventions);
            }

            if (runConventions)
            {
                navigationName = navigationName ?? "";
                return builder.ReplaceForeignKey(
                    configurationSource,
                    navigationToPrincipalName: pointsToPrincipal ? navigationName : null,
                    navigationToDependentName: pointsToPrincipal ? null : navigationName,
                    strictPrincipal: strictPrincipal);
            }

            if (oldNavigation != null)
            {
                var removedNavigation = entityType.RemoveNavigation(oldNavigation.Name);
                Debug.Assert(removedNavigation == oldNavigation);
            }

            if (navigationName != null)
            {
                entityTypeBuilder.Unignore(navigationName);
                entityType.AddNavigation(navigationName, builder.Metadata, pointsToPrincipal);
            }

            if (pointsToPrincipal)
            {
                builder._dependentToPrincipalConfigurationSource = configurationSource.Max(
                    builder._dependentToPrincipalConfigurationSource);
            }
            else
            {
                builder._principalToDependentConfigurationSource = configurationSource.Max(
                    builder._principalToDependentConfigurationSource);
            }

            return builder;
        }

        public virtual bool CanSetNavigation(
            [CanBeNull] string navigationName,
            bool pointsToPrincipal,
            ConfigurationSource configurationSource)
        {
            var existingNavigation = pointsToPrincipal ? Metadata.DependentToPrincipal : Metadata.PrincipalToDependent;
            if (navigationName == existingNavigation?.Name)
            {
                return true;
            }

            if (pointsToPrincipal
                && _dependentToPrincipalConfigurationSource.HasValue
                && !configurationSource.Overrides(_dependentToPrincipalConfigurationSource.Value))
            {
                return false;
            }
            if (!pointsToPrincipal
                && _principalToDependentConfigurationSource.HasValue
                && !configurationSource.Overrides(_principalToDependentConfigurationSource.Value))
            {
                return false;
            }

            var entityTypeBuilder = pointsToPrincipal
                ? ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, ConfigurationSource.Convention)
                : ModelBuilder.Entity(Metadata.PrincipalEntityType.Name, ConfigurationSource.Convention);

            if (navigationName != null)
            {
                if (entityTypeBuilder.IsIgnored(navigationName, configurationSource))
                {
                    return false;
                }

                if (!pointsToPrincipal)
                {
                    var canBeUnique = Entity.Metadata.Navigation.IsCompatible(
                        navigationName,
                        Metadata.PrincipalEntityType,
                        Metadata.DeclaringEntityType,
                        shouldBeCollection: false,
                        shouldThrow: false);
                    var canBeNonUnique = Entity.Metadata.Navigation.IsCompatible(
                        navigationName,
                        Metadata.PrincipalEntityType,
                        Metadata.DeclaringEntityType,
                        shouldBeCollection: true,
                        shouldThrow: false);

                    if (canBeUnique != canBeNonUnique)
                    {
                        if (_isUniqueConfigurationSource.HasValue
                            && !configurationSource.Overrides(_isUniqueConfigurationSource.Value)
                            && ((IForeignKey)Metadata).IsUnique != canBeUnique)
                        {
                            return false;
                        }
                    }
                    else if (!canBeUnique)
                    {
                        return false;
                    }
                }
                else if (!Entity.Metadata.Navigation.IsCompatible(
                    navigationName,
                    Metadata.DeclaringEntityType,
                    Metadata.PrincipalEntityType,
                    shouldBeCollection: false,
                    shouldThrow: false))
                {
                    return false;
                }
            }

            var conflictingNavigation = navigationName == null
                ? null
                : entityTypeBuilder.Metadata.FindNavigation(navigationName);
            if (conflictingNavigation != null)
            {
                if (conflictingNavigation.ForeignKey == Metadata)
                {
                    if (!CanSetNavigation(null, conflictingNavigation.PointsToPrincipal(), configurationSource))
                    {
                        return false;
                    }
                }
                else if (!ModelBuilder.Entity(conflictingNavigation.ForeignKey.DeclaringEntityType.Name, ConfigurationSource.Convention)
                    .CanRemove(conflictingNavigation.ForeignKey, configurationSource))
                {
                    return false;
                }
            }

            return true;
        }

        public virtual InternalRelationshipBuilder IsRequired(bool isRequired, ConfigurationSource configurationSource)
            => IsRequired(isRequired, configurationSource, runConventions: true);

        private InternalRelationshipBuilder IsRequired(bool isRequired, ConfigurationSource configurationSource, bool runConventions)
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

            if (!CanSetRequiredOnProperties(Metadata.Properties, isRequired, configurationSource, shouldThrow: false))
            {
                if (_foreignKeyPropertiesConfigurationSource.HasValue
                    && !configurationSource.Overrides(_foreignKeyPropertiesConfigurationSource.Value))
                {
                    return null;
                }
                _foreignKeyPropertiesConfigurationSource = null;

                return ReplaceForeignKey(configurationSource, isRequired: isRequired, runConventions: runConventions);
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

            if (!ForeignKey.CanPropertiesBeRequired(
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

        public virtual InternalRelationshipBuilder DeleteBehavior(DeleteBehavior deleteBehavior, ConfigurationSource configurationSource)
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

        public virtual InternalRelationshipBuilder IsUnique(bool unique, ConfigurationSource configurationSource)
            => IsUnique(unique, configurationSource, runConventions: true);

        private InternalRelationshipBuilder IsUnique(bool unique, ConfigurationSource configurationSource, bool runConventions)
        {
            if (((IForeignKey)Metadata).IsUnique == unique)
            {
                Metadata.IsUnique = unique;
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
                && !Entity.Metadata.Navigation.IsCompatible(
                    Metadata.PrincipalToDependent.Name,
                    Metadata.PrincipalEntityType,
                    Metadata.DeclaringEntityType,
                    !unique,
                    shouldThrow: false))
            {
                builder = builder.Navigation(
                    null,
                    pointsToPrincipal: false,
                    configurationSource: configurationSource,
                    runConventions: runConventions);

                if (builder == null)
                {
                    return null;
                }
            }

            builder._isUniqueConfigurationSource = configurationSource.Max(builder._isUniqueConfigurationSource);
            builder.Metadata.IsUnique = unique;
            return builder;
        }

        private bool CanSetUnique(bool isUnique, ConfigurationSource configurationSource)
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
                && _principalToDependentConfigurationSource.HasValue
                && !configurationSource.Overrides(_principalToDependentConfigurationSource.Value)
                && !Entity.Metadata.Navigation.IsCompatible(
                    Metadata.PrincipalToDependent.Name,
                    Metadata.PrincipalEntityType,
                    Metadata.DeclaringEntityType,
                    !isUnique,
                    shouldThrow: false))
            {
                return false;
            }

            return true;
        }

        public virtual InternalRelationshipBuilder DependentEntityType(
            [NotNull] InternalEntityTypeBuilder dependentEntityTypeBuilder, ConfigurationSource configurationSource)
            => DependentEntityType(dependentEntityTypeBuilder.Metadata, configurationSource, runConventions: true);

        public virtual InternalRelationshipBuilder DependentEntityType(
            [NotNull] Type dependentType, ConfigurationSource configurationSource)
            => DependentEntityType(ModelBuilder.Entity(dependentType, configurationSource).Metadata,
                configurationSource, runConventions: true);

        public virtual InternalRelationshipBuilder DependentEntityType(
            [NotNull] string dependentTypeName, ConfigurationSource configurationSource)
            => DependentEntityType(ModelBuilder.Entity(dependentTypeName, configurationSource).Metadata, configurationSource, runConventions: true);

        public virtual InternalRelationshipBuilder DependentEntityType(
            [NotNull] EntityType dependentEntityType, ConfigurationSource configurationSource)
            => DependentEntityType(dependentEntityType, configurationSource, runConventions: true);

        private InternalRelationshipBuilder DependentEntityType(
            EntityType dependentEntityType, ConfigurationSource configurationSource, bool runConventions)
        {
            Check.NotNull(dependentEntityType, nameof(dependentEntityType));

            if (Metadata.DeclaringEntityType.IsAssignableFrom(dependentEntityType))
            {
                _principalEndConfigurationSource = configurationSource.Max(_principalEndConfigurationSource);
                return this;
            }

            var shouldInvert = Metadata.PrincipalEntityType.IsAssignableFrom(dependentEntityType)
                               || dependentEntityType.IsAssignableFrom(Metadata.PrincipalEntityType);
            if (!shouldInvert
                && !dependentEntityType.IsAssignableFrom(Metadata.DeclaringEntityType))
            {
                if (configurationSource == ConfigurationSource.Explicit)
                {
                    throw new InvalidOperationException(CoreStrings.EntityTypeNotInRelationship(
                        dependentEntityType.DisplayName(),
                        Metadata.DeclaringEntityType.DisplayName(),
                        Metadata.PrincipalEntityType.DisplayName()));
                }
            }

            if (shouldInvert)
            {
                if (!CanSetUnique(true, configurationSource))
                {
                    return null;
                }

                if (_principalEndConfigurationSource.HasValue
                    && !configurationSource.Overrides(_principalEndConfigurationSource.Value))
                {
                    return null;
                }

                if (_foreignKeyPropertiesConfigurationSource.HasValue)
                {
                    Debug.Assert(configurationSource.Overrides(_foreignKeyPropertiesConfigurationSource.Value));
                    _foreignKeyPropertiesConfigurationSource = null;
                }

                if (_principalKeyConfigurationSource.HasValue)
                {
                    Debug.Assert(configurationSource.Overrides(_principalKeyConfigurationSource.Value));
                    _principalKeyConfigurationSource = null;
                }

                dependentEntityType = Metadata.PrincipalEntityType.IsAssignableFrom(dependentEntityType)
                    ? Metadata.PrincipalEntityType
                    : dependentEntityType;
            }

            var principalEntityType = shouldInvert ? Metadata.DeclaringEntityType : Metadata.PrincipalEntityType;

            var resetToDependent = false;
            var resetToPrincipal = false;
            if (!shouldInvert)
            {
                if (Metadata.DependentToPrincipal != null
                    && !Entity.Metadata.Navigation.IsCompatible(
                        Metadata.DependentToPrincipal.Name,
                        dependentEntityType,
                        principalEntityType,
                        null,
                        shouldThrow: false))
                {
                    if (_dependentToPrincipalConfigurationSource.HasValue
                        && !configurationSource.Overrides(_dependentToPrincipalConfigurationSource.Value))
                    {
                        return null;
                    }
                    resetToPrincipal = true;
                }

                if (Metadata.PrincipalToDependent != null
                    && !Entity.Metadata.Navigation.IsCompatible(
                        Metadata.PrincipalToDependent.Name,
                        principalEntityType,
                        dependentEntityType,
                        null,
                        shouldThrow: false))
                {
                    if (_principalToDependentConfigurationSource.HasValue
                        && !configurationSource.Overrides(_principalToDependentConfigurationSource.Value))
                    {
                        return null;
                    }
                    resetToDependent = true;
                }

                if (!Property.AreCompatible(Metadata.Properties, dependentEntityType))
                {
                    if (_foreignKeyPropertiesConfigurationSource.HasValue
                        && !configurationSource.Overrides(_foreignKeyPropertiesConfigurationSource.Value))
                    {
                        return null;
                    }

                    _foreignKeyPropertiesConfigurationSource = null;
                }
            }

            var builder = this;
            if (resetToPrincipal)
            {
                builder = builder.Navigation(
                    null,
                    pointsToPrincipal: true,
                    configurationSource: configurationSource,
                    runConventions: runConventions);
            }

            if (resetToDependent)
            {
                builder = builder.Navigation(
                    null,
                    pointsToPrincipal: false,
                    configurationSource: configurationSource,
                    runConventions: runConventions);
            }

            return builder.ReplaceForeignKey(
                configurationSource,
                principalEntityTypeBuilder: ModelBuilder.Entity(principalEntityType.Name, ConfigurationSource.Convention),
                dependentEntityTypeBuilder: ModelBuilder.Entity(dependentEntityType.Name, ConfigurationSource.Convention),
                isUnique: shouldInvert ? true : (bool?)null,
                strictPrincipal: shouldInvert,
                oldRelationshipInverted: shouldInvert,
                runConventions: runConventions);
        }

        public virtual InternalRelationshipBuilder PrincipalEntityType(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder, ConfigurationSource configurationSource)
            => PrincipalEntityType(principalEntityTypeBuilder.Metadata, configurationSource, runConventions: true);

        public virtual InternalRelationshipBuilder PrincipalEntityType(
            [NotNull] Type principalType, ConfigurationSource configurationSource)
            => PrincipalEntityType(ModelBuilder.Entity(principalType, configurationSource).Metadata,
                configurationSource, runConventions: true);

        public virtual InternalRelationshipBuilder PrincipalEntityType(
            [NotNull] string principalTypeName, ConfigurationSource configurationSource)
            => PrincipalEntityType(ModelBuilder.Entity(principalTypeName, configurationSource).Metadata,
                configurationSource, runConventions: true);

        public virtual InternalRelationshipBuilder PrincipalEntityType(
            [NotNull] EntityType principalEntityType, ConfigurationSource configurationSource)
            => PrincipalEntityType(principalEntityType, configurationSource, runConventions: true);

        private InternalRelationshipBuilder PrincipalEntityType(
            EntityType principalEntityType, ConfigurationSource configurationSource, bool runConventions)
        {
            Check.NotNull(principalEntityType, nameof(principalEntityType));

            if (Metadata.PrincipalEntityType.IsAssignableFrom(principalEntityType))
            {
                _principalEndConfigurationSource = configurationSource.Max(_principalEndConfigurationSource);
                return this;
            }

            var shouldInvert = Metadata.DeclaringEntityType.IsAssignableFrom(principalEntityType)
                               || principalEntityType.IsAssignableFrom(Metadata.DeclaringEntityType);
            if (!shouldInvert
                && !principalEntityType.IsAssignableFrom(Metadata.PrincipalEntityType))
            {
                throw new InvalidOperationException(CoreStrings.EntityTypeNotInRelationship(
                    principalEntityType.DisplayName(),
                    Metadata.DeclaringEntityType.DisplayName(),
                    Metadata.PrincipalEntityType.DisplayName()));
            }

            if (shouldInvert)
            {
                if (!CanSetUnique(true, configurationSource))
                {
                    return null;
                }

                if (_principalEndConfigurationSource.HasValue
                    && !configurationSource.Overrides(_principalEndConfigurationSource.Value))
                {
                    return null;
                }

                if (_foreignKeyPropertiesConfigurationSource.HasValue)
                {
                    Debug.Assert(configurationSource.Overrides(_foreignKeyPropertiesConfigurationSource.Value));
                    _foreignKeyPropertiesConfigurationSource = null;
                }

                if (_principalKeyConfigurationSource.HasValue)
                {
                    Debug.Assert(configurationSource.Overrides(_principalKeyConfigurationSource.Value));
                    _principalKeyConfigurationSource = null;
                }

                principalEntityType = Metadata.DeclaringEntityType.IsAssignableFrom(principalEntityType)
                    ? Metadata.DeclaringEntityType
                    : principalEntityType;
            }

            var dependentEntityType = shouldInvert ? Metadata.PrincipalEntityType : Metadata.DeclaringEntityType;

            var resetToDependent = false;
            var resetToPrincipal = false;
            if (!shouldInvert)
            {
                if (Metadata.DependentToPrincipal != null
                    && !Entity.Metadata.Navigation.IsCompatible(
                        Metadata.DependentToPrincipal.Name,
                        dependentEntityType,
                        principalEntityType,
                        null,
                        shouldThrow: false))
                {
                    if (_dependentToPrincipalConfigurationSource.HasValue
                        && !configurationSource.Overrides(_dependentToPrincipalConfigurationSource.Value))
                    {
                        return null;
                    }
                    resetToPrincipal = true;
                }

                if (Metadata.PrincipalToDependent != null
                    && !Entity.Metadata.Navigation.IsCompatible(
                        Metadata.PrincipalToDependent.Name,
                        principalEntityType,
                        dependentEntityType,
                        null,
                        shouldThrow: false))
                {
                    if (_principalToDependentConfigurationSource.HasValue
                        && !configurationSource.Overrides(_principalToDependentConfigurationSource.Value))
                    {
                        return null;
                    }
                    resetToDependent = true;
                }

                if (!Property.AreCompatible(Metadata.PrincipalKey.Properties, principalEntityType))
                {
                    if (_principalKeyConfigurationSource.HasValue
                        && !configurationSource.Overrides(_principalKeyConfigurationSource.Value))
                    {
                        return null;
                    }

                    _principalKeyConfigurationSource = null;
                }
            }

            var builder = this;
            if (resetToPrincipal)
            {
                builder = builder.Navigation(
                    null,
                    pointsToPrincipal: true,
                    configurationSource: configurationSource,
                    runConventions: runConventions);
            }

            if (resetToDependent)
            {
                builder = builder.Navigation(
                    null,
                    pointsToPrincipal: false,
                    configurationSource: configurationSource,
                    runConventions: runConventions);
            }

            return builder.ReplaceForeignKey(
                configurationSource,
                principalEntityTypeBuilder: ModelBuilder.Entity(principalEntityType.Name, ConfigurationSource.Convention),
                dependentEntityTypeBuilder: ModelBuilder.Entity(dependentEntityType.Name, ConfigurationSource.Convention),
                isUnique: shouldInvert ? true : (bool?)null,
                strictPrincipal: shouldInvert,
                oldRelationshipInverted: shouldInvert,
                runConventions: runConventions);
        }

        public virtual bool CanInvert(
            [CanBeNull] IReadOnlyList<Property> newForeignKeyProperties, ConfigurationSource configurationSource)
            => CanInvert(configurationSource)
               && (newForeignKeyProperties == null
                   || CanSetForeignKey(newForeignKeyProperties, Metadata.PrincipalEntityType, configurationSource));

        private bool CanInvert(ConfigurationSource configurationSource)
        {
            if (!CanSetUnique(true, configurationSource))
            {
                return false;
            }

            if (_principalEndConfigurationSource.HasValue
                && !configurationSource.Overrides(_principalEndConfigurationSource.Value))
            {
                return false;
            }

            return true;
        }

        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] IReadOnlyList<PropertyInfo> properties, ConfigurationSource configurationSource)
            => HasForeignKey(
                ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, configurationSource)
                    .GetOrCreateProperties(properties, configurationSource),
                configurationSource,
                runConventions: true);

        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => HasForeignKey(
                ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, configurationSource)
                    .GetOrCreateProperties(propertyNames, configurationSource),
                configurationSource,
                runConventions: true);

        public virtual InternalRelationshipBuilder HasForeignKey(
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
            => HasForeignKey(
                GetExistingProperties(properties, Metadata.DeclaringEntityType), configurationSource, runConventions: true);

        private InternalRelationshipBuilder HasForeignKey(
            IReadOnlyList<Property> properties, ConfigurationSource configurationSource, bool runConventions)
        {
            if (properties == null)
            {
                return null;
            }

            if (Metadata.Properties.SequenceEqual(properties))
            {
                if (!Metadata.IsSelfReferencing())
                {
                    _principalEndConfigurationSource = configurationSource.Max(_principalEndConfigurationSource);
                }
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
            if (Metadata.IsRequired.HasValue
                && !CanSetRequiredOnProperties(properties, Metadata.IsRequired, configurationSource, shouldThrow: false))
            {
                if (_isRequiredConfigurationSource.HasValue
                    && !configurationSource.Overrides(_isRequiredConfigurationSource.Value))
                {
                    return null;
                }
                resetIsRequired = true;
            }

            if (_principalKeyConfigurationSource.HasValue
                && !ForeignKey.AreCompatible(
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
                Metadata.IsRequired = null;
                _isRequiredConfigurationSource = null;
            }

            return ReplaceForeignKey(
                configurationSource,
                dependentProperties: properties,
                runConventions: runConventions);
        }

        private bool CanSetForeignKey(
            IReadOnlyList<Property> properties, EntityType dependentEntityType, ConfigurationSource configurationSource)
        {
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
                && dependentEntityType == Metadata.DeclaringEntityType
                && !ForeignKey.AreCompatible(
                    Metadata.PrincipalKey.Properties,
                    properties,
                    Metadata.PrincipalEntityType,
                    Metadata.DeclaringEntityType,
                    shouldThrow: false))
            {
                return false;
            }

            var conflictingForeignKey = dependentEntityType.FindForeignKey(properties);
            if (conflictingForeignKey != null
                && !ModelBuilder.Entity(dependentEntityType.Name, ConfigurationSource.Convention)
                    .CanRemove(conflictingForeignKey, configurationSource))
            {
                return false;
            }

            return true;
        }

        public virtual InternalRelationshipBuilder HasPrincipalKey([NotNull] IReadOnlyList<PropertyInfo> properties,
            ConfigurationSource configurationSource)
            => HasPrincipalKey(
                ModelBuilder.Entity(Metadata.PrincipalEntityType.Name, configurationSource)
                    .GetOrCreateProperties(properties, configurationSource),
                configurationSource,
                runConventions: true);

        public virtual InternalRelationshipBuilder HasPrincipalKey([NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
            => HasPrincipalKey(
                ModelBuilder.Entity(Metadata.PrincipalEntityType.Name, configurationSource)
                    .GetOrCreateProperties(propertyNames, configurationSource),
                configurationSource,
                runConventions: true);

        public virtual InternalRelationshipBuilder HasPrincipalKey(
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
            => HasPrincipalKey(GetExistingProperties(properties, Metadata.PrincipalEntityType), configurationSource, runConventions: true);

        private InternalRelationshipBuilder HasPrincipalKey(
            IReadOnlyList<Property> properties, ConfigurationSource configurationSource, bool runConventions)
        {
            if (properties == null)
            {
                return null;
            }

            if (Metadata.PrincipalKey.Properties.SequenceEqual(properties))
            {
                if (!Metadata.IsSelfReferencing())
                {
                    _principalEndConfigurationSource = configurationSource.Max(_principalEndConfigurationSource);
                }
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
                && !ForeignKey.AreCompatible(
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
                && !ForeignKey.AreCompatible(
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

        private InternalRelationshipBuilder ReplaceForeignKey(
            ConfigurationSource configurationSource,
            InternalEntityTypeBuilder principalEntityTypeBuilder = null,
            InternalEntityTypeBuilder dependentEntityTypeBuilder = null,
            string navigationToPrincipalName = null,
            string navigationToDependentName = null,
            IReadOnlyList<Property> dependentProperties = null,
            IReadOnlyList<Property> principalProperties = null,
            bool? isUnique = null,
            bool? isRequired = null,
            DeleteBehavior? deleteBehavior = null,
            bool strictPrincipal = false,
            bool oldRelationshipInverted = false,
            bool runConventions = true)
        {
            principalEntityTypeBuilder = principalEntityTypeBuilder ??
                                         ModelBuilder.Entity(oldRelationshipInverted ? Metadata.DeclaringEntityType.Name : Metadata.PrincipalEntityType.Name,
                                             ConfigurationSource.Convention);
            dependentEntityTypeBuilder = dependentEntityTypeBuilder ??
                                         ModelBuilder.Entity(oldRelationshipInverted ? Metadata.PrincipalEntityType.Name : Metadata.DeclaringEntityType.Name,
                                             ConfigurationSource.Convention);

            if (navigationToPrincipalName == null)
            {
                if (oldRelationshipInverted)
                {
                    navigationToPrincipalName = _principalToDependentConfigurationSource.HasValue
                                                && _principalToDependentConfigurationSource.Value.Overrides(configurationSource)
                        ? Metadata.PrincipalToDependent?.Name ?? ""
                        : null;
                }
                else
                {
                    navigationToPrincipalName = _dependentToPrincipalConfigurationSource.HasValue
                                                && _dependentToPrincipalConfigurationSource.Value.Overrides(configurationSource)
                        ? Metadata.DependentToPrincipal?.Name ?? ""
                        : null;
                }
            }

            if (navigationToDependentName == null)
            {
                if (oldRelationshipInverted)
                {
                    navigationToDependentName = _dependentToPrincipalConfigurationSource.HasValue
                                                && _dependentToPrincipalConfigurationSource.Value.Overrides(configurationSource)
                        ? Metadata.DependentToPrincipal?.Name ?? ""
                        : null;
                }
                else
                {
                    navigationToDependentName = _principalToDependentConfigurationSource.HasValue
                                                && _principalToDependentConfigurationSource.Value.Overrides(configurationSource)
                        ? Metadata.PrincipalToDependent?.Name ?? ""
                        : null;
                }
            }

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

            strictPrincipal = strictPrincipal
                              || (_principalEndConfigurationSource.HasValue
                                  && _principalEndConfigurationSource.Value.Overrides(configurationSource))
                              || (principalEntityTypeBuilder.Metadata != dependentEntityTypeBuilder.Metadata
                                  && (principalProperties != null
                                      || dependentProperties != null));

            return ReplaceForeignKey(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                navigationToPrincipalName,
                navigationToDependentName,
                dependentProperties,
                principalProperties,
                isUnique,
                isRequired,
                deleteBehavior,
                strictPrincipal,
                oldRelationshipInverted,
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
            bool strictPrincipal,
            bool oldRelationshipInverted,
            ConfigurationSource configurationSource,
            bool runConventions)
            => Relationship(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                navigationToPrincipalName,
                navigationToDependentName,
                dependentProperties,
                principalProperties,
                isUnique,
                isRequired,
                deleteBehavior,
                strictPrincipal,
                oldRelationshipInverted,
                null,
                null,
                configurationSource,
                runConventions);

        private InternalRelationshipBuilder Relationship(
            InternalEntityTypeBuilder principalEntityTypeBuilder,
            InternalEntityTypeBuilder dependentEntityTypeBuilder,
            string navigationToPrincipalName,
            string navigationToDependentName,
            IReadOnlyList<Property> dependentProperties,
            IReadOnlyList<Property> principalProperties,
            bool? isUnique,
            bool? isRequired,
            DeleteBehavior? deleteBehavior,
            bool strictPrincipal,
            bool oldRelationshipInverted,
            string oldNavigationToPrincipalName,
            string oldNavigationToDependentName,
            ConfigurationSource configurationSource,
            bool runConventions)
        {
            Check.NotNull(principalEntityTypeBuilder, nameof(principalEntityTypeBuilder));
            Check.NotNull(dependentEntityTypeBuilder, nameof(dependentEntityTypeBuilder));
            Debug.Assert(AreCompatible(
                principalEntityTypeBuilder.Metadata,
                dependentEntityTypeBuilder.Metadata,
                navigationToPrincipalName == "" ? null : navigationToPrincipalName,
                navigationToDependentName == "" ? null : navigationToDependentName,
                dependentProperties,
                principalProperties,
                isUnique,
                isRequired,
                ModelBuilder,
                configurationSource));

            var dependentEntityType = dependentEntityTypeBuilder.Metadata;
            var principalEntityType = principalEntityTypeBuilder.Metadata;
            var matchingRelationships = dependentEntityTypeBuilder.GetRelationshipBuilders(
                principalEntityType,
                navigationToPrincipalName == "" ? null : navigationToPrincipalName,
                navigationToDependentName == "" ? null : navigationToDependentName,
                dependentProperties);

            var existingRelationshipInverted = false;
            matchingRelationships = matchingRelationships.Distinct().Where(r => r.Metadata != Metadata).ToList();
            var newRelationshipBuilder = matchingRelationships.FirstOrDefault(r =>
                r.CanSet(principalEntityType,
                    dependentEntityType,
                    navigationToPrincipalName == "" ? null : navigationToPrincipalName,
                    navigationToDependentName == "" ? null : navigationToDependentName,
                    strictPrincipal,
                    configurationSource,
                    out existingRelationshipInverted));

            var conflictingRelationships = matchingRelationships.Where(r => r != newRelationshipBuilder).ToList();
            if (conflictingRelationships.Any(relationshipBuilder =>
                !ModelBuilder.Entity(relationshipBuilder.Metadata.DeclaringEntityType.Name, ConfigurationSource.Convention)
                    .CanRemove(relationshipBuilder.Metadata, configurationSource)))
            {
                return null;
            }

            var shouldUpgradeSource = dependentProperties != null
                                      || !string.IsNullOrEmpty(navigationToPrincipalName)
                                      || !string.IsNullOrEmpty(navigationToDependentName);

            var newRelationshipConfigurationSource = shouldUpgradeSource
                ? configurationSource
                : ConfigurationSource.Convention;

            var removedNavigations = new Dictionary<string, Tuple<InternalEntityTypeBuilder, InternalEntityTypeBuilder, string>>();
            var removedForeignKeys = new List<Tuple<InternalEntityTypeBuilder, ForeignKey>>();
            if (Metadata.DeclaringEntityType.GetDeclaredForeignKeys().Contains(Metadata))
            {
                Debug.Assert(oldNavigationToPrincipalName == null
                             || Metadata.DependentToPrincipal?.Name == null);
                Debug.Assert(oldNavigationToDependentName == null
                             || Metadata.PrincipalToDependent?.Name == null);

                var oldDependentEntityTypeBuilder = ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, ConfigurationSource.Convention);
                var oldPrincipalEntityTypeBuilder = ModelBuilder.Entity(Metadata.PrincipalEntityType.Name, ConfigurationSource.Convention);
                oldNavigationToPrincipalName = Metadata.DependentToPrincipal?.Name;
                if (oldNavigationToPrincipalName != null)
                {
                    removedNavigations[Metadata.DeclaringEntityType.Name + oldNavigationToPrincipalName] = Tuple.Create(
                        oldDependentEntityTypeBuilder, oldPrincipalEntityTypeBuilder, oldNavigationToPrincipalName);
                }
                oldNavigationToDependentName = Metadata.PrincipalToDependent?.Name;
                if (oldNavigationToDependentName != null)
                {
                    removedNavigations[Metadata.PrincipalEntityType.Name + oldNavigationToDependentName] = Tuple.Create(
                        oldPrincipalEntityTypeBuilder, oldDependentEntityTypeBuilder, oldNavigationToDependentName);
                }

                var fkOwner = ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, ConfigurationSource.Convention);
                var replacedConfigurationSource = fkOwner.RemoveForeignKey(Metadata, ConfigurationSource.Explicit, runConventions: false);
                Debug.Assert(replacedConfigurationSource.HasValue);

                removedForeignKeys.Add(Tuple.Create(fkOwner, Metadata));
                newRelationshipConfigurationSource = newRelationshipConfigurationSource.Max(replacedConfigurationSource.Value);
            }

            foreach (var relationshipBuilder in conflictingRelationships)
            {
                var foreignKey = relationshipBuilder.Metadata;
                var oldDependentEntityTypeBuilder = ModelBuilder.Entity(foreignKey.DeclaringEntityType.Name, ConfigurationSource.Convention);
                var oldPrincipalEntityTypeBuilder = ModelBuilder.Entity(foreignKey.PrincipalEntityType.Name, ConfigurationSource.Convention);
                if (foreignKey.DependentToPrincipal != null)
                {
                    removedNavigations[foreignKey.DeclaringEntityType.Name + foreignKey.DependentToPrincipal.Name] = Tuple.Create(
                        oldDependentEntityTypeBuilder, oldPrincipalEntityTypeBuilder, foreignKey.DependentToPrincipal.Name);
                }
                if (foreignKey.PrincipalToDependent != null)
                {
                    removedNavigations[foreignKey.PrincipalEntityType.Name + foreignKey.PrincipalToDependent.Name] = Tuple.Create(
                        oldPrincipalEntityTypeBuilder, oldDependentEntityTypeBuilder, foreignKey.PrincipalToDependent.Name);
                }
                var fkOwner = ModelBuilder.Entity(foreignKey.DeclaringEntityType.Name, ConfigurationSource.Convention);
                var removed = fkOwner.RemoveForeignKey(foreignKey, configurationSource, runConventions: false);
                Debug.Assert(removed.HasValue);

                removedForeignKeys.Add(Tuple.Create(fkOwner, Metadata));
            }

            if (newRelationshipBuilder == null)
            {
                newRelationshipBuilder = dependentEntityTypeBuilder.CreateForeignKey(
                    principalEntityTypeBuilder,
                    dependentProperties,
                    principalProperties,
                    navigationToPrincipalName == "" ? null : navigationToPrincipalName,
                    isRequired,
                    newRelationshipConfigurationSource,
                    runConventions: false);
            }
            else if (existingRelationshipInverted
                     && !strictPrincipal)
            {
                oldRelationshipInverted = !oldRelationshipInverted;

                var entityTypeBuilder = principalEntityTypeBuilder;
                principalEntityTypeBuilder = dependentEntityTypeBuilder;
                dependentEntityTypeBuilder = entityTypeBuilder;

                var navigationName = navigationToPrincipalName;
                navigationToPrincipalName = navigationToDependentName;
                navigationToDependentName = navigationName;
            }

            var newForeignKey = newRelationshipBuilder.Metadata;
            if (newForeignKey.DependentToPrincipal != null)
            {
                removedNavigations[newForeignKey.DeclaringEntityType.Name + newForeignKey.DependentToPrincipal.Name] = Tuple.Create(
                    dependentEntityTypeBuilder, principalEntityTypeBuilder, newForeignKey.DependentToPrincipal.Name);
            }
            if (newForeignKey.PrincipalToDependent != null)
            {
                removedNavigations[newForeignKey.PrincipalEntityType.Name + newForeignKey.PrincipalToDependent.Name] = Tuple.Create(
                    principalEntityTypeBuilder, dependentEntityTypeBuilder, newForeignKey.PrincipalToDependent.Name);
            }

            newRelationshipBuilder = ModelBuilder.Entity(newRelationshipBuilder.Metadata.DeclaringEntityType.Name, ConfigurationSource.Convention)
                .Relationship(newRelationshipBuilder.Metadata, newRelationshipConfigurationSource);

            newRelationshipBuilder = newRelationshipBuilder.DependentEntityType(
                dependentEntityTypeBuilder.Metadata,
                strictPrincipal ? configurationSource : ConfigurationSource.Convention,
                runConventions: false);
            newRelationshipBuilder = newRelationshipBuilder.PrincipalEntityType(
                principalEntityTypeBuilder.Metadata,
                strictPrincipal ? configurationSource : ConfigurationSource.Convention,
                runConventions: false);

            if (oldRelationshipInverted)
            {
                newRelationshipBuilder._principalEndConfigurationSource = configurationSource
                    .Max(newRelationshipBuilder._principalEndConfigurationSource);
            }
            if (dependentProperties != null)
            {
                newRelationshipBuilder = newRelationshipBuilder.HasForeignKey(
                    dependentProperties,
                    configurationSource.Max(oldRelationshipInverted ? null : _foreignKeyPropertiesConfigurationSource),
                    runConventions: false)
                                         ?? newRelationshipBuilder;
            }
            if (principalProperties != null)
            {
                newRelationshipBuilder = newRelationshipBuilder.HasPrincipalKey(
                    principalProperties,
                    configurationSource.Max(oldRelationshipInverted ? null : _principalKeyConfigurationSource),
                    runConventions: false)
                                         ?? newRelationshipBuilder;
            }
            if (isUnique.HasValue)
            {
                newRelationshipBuilder = newRelationshipBuilder.IsUnique(
                    isUnique.Value,
                    configurationSource.Max(_isUniqueConfigurationSource),
                    runConventions: false)
                                         ?? newRelationshipBuilder;
            }
            if (isRequired.HasValue)
            {
                newRelationshipBuilder = newRelationshipBuilder.IsRequired(
                    isRequired.Value,
                    configurationSource.Max(_isRequiredConfigurationSource),
                    runConventions: false)
                                         ?? newRelationshipBuilder;
            }
            if (deleteBehavior.HasValue)
            {
                newRelationshipBuilder = newRelationshipBuilder.DeleteBehavior(
                    deleteBehavior.Value,
                    configurationSource.Max(_deleteBehaviorConfigurationSource))
                                         ?? newRelationshipBuilder;
            }
            if (navigationToPrincipalName != null)
            {
                newRelationshipBuilder = newRelationshipBuilder.Navigation(
                    navigationToPrincipalName == "" ? null : navigationToPrincipalName,
                    pointsToPrincipal: true,
                    configurationSource: configurationSource.Max(oldRelationshipInverted
                        ? _principalToDependentConfigurationSource
                        : _dependentToPrincipalConfigurationSource),
                    runConventions: false)
                                         ?? newRelationshipBuilder;
            }
            if (navigationToDependentName != null)
            {
                newRelationshipBuilder = newRelationshipBuilder.Navigation(
                    navigationToDependentName == "" ? null : navigationToDependentName,
                    pointsToPrincipal: false,
                    configurationSource: configurationSource.Max(oldRelationshipInverted
                        ? _dependentToPrincipalConfigurationSource
                        : _principalToDependentConfigurationSource),
                    runConventions: false)
                                         ?? newRelationshipBuilder;
            }

            if (!oldRelationshipInverted)
            {
                newRelationshipBuilder._principalEndConfigurationSource = _principalEndConfigurationSource?
                    .Max(newRelationshipBuilder._principalEndConfigurationSource)
                                                                          ?? newRelationshipBuilder._principalEndConfigurationSource;
            }
            if (dependentProperties == null
                && !oldRelationshipInverted
                && _foreignKeyPropertiesConfigurationSource.HasValue)
            {
                var oldDependentProperties = GetExistingProperties(Metadata.Properties, newRelationshipBuilder.Metadata.DeclaringEntityType);
                newRelationshipBuilder = newRelationshipBuilder.HasForeignKey(
                    oldDependentProperties, _foreignKeyPropertiesConfigurationSource.Value, runConventions: false)
                                         ?? newRelationshipBuilder;
            }
            if (principalProperties == null
                && !oldRelationshipInverted
                && _principalKeyConfigurationSource.HasValue)
            {
                var oldPrincipalKey = newRelationshipBuilder.Metadata.PrincipalEntityType.FindKey(Metadata.PrincipalKey.Properties);
                if (oldPrincipalKey != null)
                {
                    newRelationshipBuilder = newRelationshipBuilder.HasPrincipalKey(
                        oldPrincipalKey.Properties, _principalKeyConfigurationSource.Value, runConventions: false)
                                             ?? newRelationshipBuilder;
                }
            }
            if (!isUnique.HasValue
                && _isUniqueConfigurationSource.HasValue
                && Metadata.IsUnique.HasValue)
            {
                newRelationshipBuilder = newRelationshipBuilder.IsUnique(
                    Metadata.IsUnique.Value, _isUniqueConfigurationSource.Value, runConventions: false)
                                         ?? newRelationshipBuilder;
            }
            if (!isRequired.HasValue
                && _isRequiredConfigurationSource.HasValue
                && Metadata.IsRequired.HasValue)
            {
                newRelationshipBuilder = newRelationshipBuilder.IsRequired(Metadata.IsRequired.Value, _isRequiredConfigurationSource.Value, runConventions: false)
                                         ?? newRelationshipBuilder;
            }
            if (!deleteBehavior.HasValue
                && _deleteBehaviorConfigurationSource.HasValue
                && Metadata.DeleteBehavior.HasValue)
            {
                newRelationshipBuilder = newRelationshipBuilder.DeleteBehavior(Metadata.DeleteBehavior.Value, _deleteBehaviorConfigurationSource.Value)
                                         ?? newRelationshipBuilder;
            }

            if (_dependentToPrincipalConfigurationSource.HasValue)
            {
                if (oldRelationshipInverted)
                {
                    if (navigationToDependentName == null)
                    {
                        newRelationshipBuilder = newRelationshipBuilder.Navigation(
                            oldNavigationToPrincipalName,
                            pointsToPrincipal: false,
                            configurationSource: _dependentToPrincipalConfigurationSource.Value,
                            runConventions: false)
                                                 ?? newRelationshipBuilder;
                    }
                }
                else
                {
                    if (navigationToPrincipalName == null)
                    {
                        newRelationshipBuilder = newRelationshipBuilder.Navigation(
                            oldNavigationToPrincipalName,
                            pointsToPrincipal: true,
                            configurationSource: _dependentToPrincipalConfigurationSource.Value,
                            runConventions: false)
                                                 ?? newRelationshipBuilder;
                    }
                }
            }

            if (_principalToDependentConfigurationSource.HasValue)
            {
                if (oldRelationshipInverted)
                {
                    if (navigationToPrincipalName == null)
                    {
                        newRelationshipBuilder = newRelationshipBuilder.Navigation(
                            oldNavigationToDependentName,
                            pointsToPrincipal: true,
                            configurationSource: _principalToDependentConfigurationSource.Value,
                            runConventions: false)
                                                 ?? newRelationshipBuilder;
                    }
                }
                else
                {
                    if (navigationToDependentName == null)
                    {
                        newRelationshipBuilder = newRelationshipBuilder.Navigation(
                            oldNavigationToDependentName,
                            pointsToPrincipal: false,
                            configurationSource: _principalToDependentConfigurationSource.Value,
                            runConventions: false)
                                                 ?? newRelationshipBuilder;
                    }
                }
            }

            if (runConventions)
            {
                var dependentToPrincipalIsNew = false;
                if (newRelationshipBuilder.Metadata.DependentToPrincipal != null)
                {
                    dependentToPrincipalIsNew = !removedNavigations.Remove(
                        newRelationshipBuilder.Metadata.DeclaringEntityType.Name + newRelationshipBuilder.Metadata.DependentToPrincipal.Name);
                }

                var principalToDependentIsNew = false;
                if (newRelationshipBuilder.Metadata.PrincipalToDependent != null)
                {
                    principalToDependentIsNew = !removedNavigations.Remove(
                        newRelationshipBuilder.Metadata.PrincipalEntityType.Name + newRelationshipBuilder.Metadata.PrincipalToDependent.Name);
                }

                foreach (var removedNavigation in removedNavigations)
                {
                    ModelBuilder.ConventionDispatcher.OnNavigationRemoved(
                        removedNavigation.Value.Item1, removedNavigation.Value.Item2, removedNavigation.Value.Item3);
                }

                foreach (var removedForeignKey in removedForeignKeys)
                {
                    ModelBuilder.ConventionDispatcher.OnForeignKeyRemoved(removedForeignKey.Item1, removedForeignKey.Item2);
                }

                newRelationshipBuilder = ModelBuilder.ConventionDispatcher.OnForeignKeyAdded(newRelationshipBuilder);
                if (newRelationshipBuilder == null)
                {
                    return null;
                }

                var inverted = newRelationshipBuilder.Metadata.DeclaringEntityType != dependentEntityType;
                if ((dependentToPrincipalIsNew && !inverted)
                    || (principalToDependentIsNew && inverted))
                {
                    newRelationshipBuilder = ModelBuilder.ConventionDispatcher.OnNavigationAdded(
                        newRelationshipBuilder, newRelationshipBuilder.Metadata.DependentToPrincipal);
                }
                if (newRelationshipBuilder == null)
                {
                    return null;
                }

                if ((principalToDependentIsNew && !inverted)
                    || (dependentToPrincipalIsNew && inverted))
                {
                    newRelationshipBuilder = ModelBuilder.ConventionDispatcher.OnNavigationAdded(
                        newRelationshipBuilder, newRelationshipBuilder.Metadata.PrincipalToDependent);
                }
            }

            return newRelationshipBuilder;
        }

        public virtual InternalRelationshipBuilder Attach(
            [CanBeNull] string dependentToPrincipalName,
            [CanBeNull] string principalToDependentName,
            ConfigurationSource configurationSource)
        {
            Debug.Assert(!Metadata.DeclaringEntityType.GetForeignKeys().Contains(Metadata));

            var dependentProperties = GetExistingProperties(Metadata.Properties, Metadata.DeclaringEntityType);
            if (dependentProperties == null)
            {
                _foreignKeyPropertiesConfigurationSource = null;
            }
            else if (!_foreignKeyPropertiesConfigurationSource.HasValue
                     || !_foreignKeyPropertiesConfigurationSource.Value.Overrides(configurationSource))
            {
                dependentProperties = null;
            }

            var principalKey = Metadata.PrincipalEntityType.FindKey(Metadata.PrincipalKey.Properties);
            if (principalKey == null)
            {
                _principalKeyConfigurationSource = null;
                if (!_foreignKeyPropertiesConfigurationSource.HasValue
                    || !_foreignKeyPropertiesConfigurationSource.Value.Overrides(ConfigurationSource.Explicit))
                {
                    _foreignKeyPropertiesConfigurationSource = null;
                    dependentProperties = null;
                }
            }
            else if (!_principalKeyConfigurationSource.HasValue
                     || !_principalKeyConfigurationSource.Value.Overrides(configurationSource))
            {
                principalKey = null;
            }

            bool? isUnique = null;
            if (_isUniqueConfigurationSource.HasValue
                && _isUniqueConfigurationSource.Value.Overrides(configurationSource))
            {
                isUnique = Metadata.IsUnique;
            }

            bool? isRequired = null;
            if (_isRequiredConfigurationSource.HasValue
                && _isRequiredConfigurationSource.Value.Overrides(configurationSource))
            {
                isRequired = Metadata.IsRequired;
            }

            DeleteBehavior? deleteBehavior = null;
            if (_deleteBehaviorConfigurationSource.HasValue
                && _deleteBehaviorConfigurationSource.Value.Overrides(configurationSource))
            {
                deleteBehavior = Metadata.DeleteBehavior;
            }

            var strictPrincipal = _principalEndConfigurationSource.HasValue
                                  && _principalEndConfigurationSource.Value.Overrides(configurationSource);

            var principalEntityTypeBuilder = ModelBuilder.Entity(Metadata.PrincipalEntityType.Name, configurationSource);
            var dependentEntityTypeBuilder = ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, configurationSource);
            var principalProperties = principalKey?.Properties;

            return Relationship(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                dependentToPrincipalName,
                principalToDependentName,
                dependentProperties,
                principalProperties,
                isUnique,
                isRequired,
                deleteBehavior,
                strictPrincipal,
                /*oldRelationshipInverted:*/ false,
                dependentToPrincipalName,
                principalToDependentName,
                configurationSource,
                runConventions: true);
        }

        private static List<Property> GetExistingProperties(IReadOnlyList<Property> properties, EntityType entityType)
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

            return ForeignKey.AreCompatible(
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

        private bool CanSet(
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

            if (!CanSet(
                principalEntityType,
                dependentEntityType,
                navigationToPrincipalName,
                navigationToDependentName,
                strictPrincipal,
                configurationSource,
                out inverted))
            {
                return false;
            }

            if (inverted)
            {
                var navigationName = navigationToPrincipalName;
                navigationToPrincipalName = navigationToDependentName;
                navigationToDependentName = navigationName;
            }

            if (navigationToPrincipalName != null
                && !CanSetNavigation(navigationToPrincipalName, true, configurationSource))
            {
                return false;
            }

            if (navigationToDependentName != null
                && !CanSetNavigation(navigationToDependentName, false, configurationSource))
            {
                return false;
            }

            if (dependentProperties != null
                && !CanSetForeignKey(dependentProperties, inverted ? dependentEntityType : principalEntityType, configurationSource))
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

        private bool CanSet(
            EntityType principalEntityType,
            EntityType dependentEntityType,
            string navigationToPrincipalName,
            string navigationToDependentName,
            bool strictPrincipal,
            ConfigurationSource configurationSource,
            out bool inverted)
        {
            inverted = false;

            if (CanSet(
                principalEntityType,
                dependentEntityType,
                navigationToPrincipalName,
                navigationToDependentName))
            {
                return true;
            }

            if (!CanInvert(configurationSource)
                && strictPrincipal)
            {
                return false;
            }

            if (CanSet(
                dependentEntityType,
                principalEntityType,
                navigationToDependentName,
                navigationToPrincipalName))
            {
                inverted = true;
                return true;
            }

            return false;
        }

        private bool CanSet(
            EntityType principalEntityType,
            EntityType dependentEntityType,
            string navigationToPrincipalName,
            string navigationToDependentName)
        {
            if (principalEntityType == dependentEntityType)
            {
                // The dependent end cannot be determined based on entity types, so use navigations
                if ((navigationToPrincipalName != null
                     && Metadata.DependentToPrincipal != null
                     && navigationToPrincipalName != Metadata.DependentToPrincipal.Name)
                    || (navigationToDependentName != null
                        && Metadata.PrincipalToDependent != null
                        && navigationToDependentName != Metadata.PrincipalToDependent.Name))
                {
                    return false;
                }
            }

            if (Metadata.DeclaringEntityType != dependentEntityType)
            {
                return false;
            }

            if (Metadata.PrincipalEntityType != principalEntityType)
            {
                return false;
            }

            return true;
        }
    }
}
