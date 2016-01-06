// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalRelationshipBuilder : InternalMetadataItemBuilder<ForeignKey>
    {
        public InternalRelationshipBuilder(
            [NotNull] ForeignKey foreignKey,
            [NotNull] InternalModelBuilder modelBuilder)
            : base(foreignKey, modelBuilder)
        {
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

            var newRelationshipBuilder = ReplaceForeignKey(configurationSource,
                navigationToPrincipalName: navigationToPrincipalName ?? "",
                navigationToDependentName: navigationToDependentName ?? "");

            if (newRelationshipBuilder != null
                && newRelationshipBuilder.Metadata.Builder == null)
            {
                return FindForeignKey(
                    navigationToPrincipalName,
                    navigationToDependentName);
            }

            return newRelationshipBuilder;
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
                Metadata.UpdateConfigurationSource(configurationSource);

                if (pointsToPrincipal)
                {
                    Metadata.HasDependentToPrincipal(navigationName, configurationSource, runConventions);
                }
                else
                {
                    Metadata.HasPrincipalToDependent(navigationName, configurationSource, runConventions);
                }
                return this;
            }

            if (pointsToPrincipal
                && !configurationSource.Overrides(Metadata.GetDependentToPrincipalConfigurationSource()))
            {
                return null;
            }

            if (!pointsToPrincipal
                && !configurationSource.Overrides(Metadata.GetPrincipalToDependentConfigurationSource()))
            {
                return null;
            }

            var entityType = pointsToPrincipal ? Metadata.DeclaringEntityType : Metadata.PrincipalEntityType;
            var entityTypeBuilder = entityType.Builder;

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
                    var canBeUnique = Internal.Navigation.IsCompatible(
                        navigationName,
                        Metadata.PrincipalEntityType,
                        Metadata.DeclaringEntityType,
                        shouldBeCollection: false,
                        shouldThrow: false);
                    var canBeNonUnique = Internal.Navigation.IsCompatible(
                        navigationName,
                        Metadata.PrincipalEntityType,
                        Metadata.DeclaringEntityType,
                        shouldBeCollection: true,
                        shouldThrow: false);

                    if (canBeUnique != canBeNonUnique)
                    {
                        if (!configurationSource.Overrides(Metadata.GetIsUniqueConfigurationSource())
                            && (Metadata.IsUnique != canBeUnique))
                        {
                            return null;
                        }

                        shouldBeUnique = canBeUnique;
                        if (canBeUnique)
                        {
                            uniquenessConfigurationSource = (Metadata.GetPrincipalEndConfigurationSource() ?? ConfigurationSource.Convention)
                                .Max(Metadata.GetIsUniqueConfigurationSource());
                        }
                        else
                        {
                            strictPrincipal = true;
                            uniquenessConfigurationSource = configurationSource;
                        }
                    }
                    else if (!canBeUnique)
                    {
                        Internal.Navigation.IsCompatible(
                            navigationName,
                            Metadata.PrincipalEntityType,
                            Metadata.DeclaringEntityType,
                            shouldBeCollection: false,
                            shouldThrow: configurationSource == ConfigurationSource.Explicit);

                        return null;
                    }
                }
                else if (!Internal.Navigation.IsCompatible(
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
                Debug.Assert(conflictingNavigation.IsDependentToPrincipal() != pointsToPrincipal);

                builder = builder.Navigation(null, conflictingNavigation.IsDependentToPrincipal(), configurationSource, runConventions);
            }
            else
            {
                Debug.Assert((conflictingNavigation == null) || runConventions);
            }

            if (shouldBeUnique.HasValue)
            {
                builder = builder.IsUnique(shouldBeUnique.Value, uniquenessConfigurationSource, runConventions);
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

            if (navigationName != null)
            {
                entityTypeBuilder.Unignore(navigationName);
            }

            if (pointsToPrincipal)
            {
                builder.Metadata.HasDependentToPrincipal(navigationName, configurationSource, runConventions);
            }
            else
            {
                builder.Metadata.HasPrincipalToDependent(navigationName, configurationSource, runConventions);
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
                && !configurationSource.Overrides(Metadata.GetDependentToPrincipalConfigurationSource()))
            {
                return false;
            }

            if (!pointsToPrincipal
                && !configurationSource.Overrides(Metadata.GetPrincipalToDependentConfigurationSource()))
            {
                return false;
            }

            var entityTypeBuilder = pointsToPrincipal
                ? Metadata.DeclaringEntityType.Builder
                : Metadata.PrincipalEntityType.Builder;

            if (navigationName != null)
            {
                if (entityTypeBuilder.IsIgnored(navigationName, configurationSource))
                {
                    return false;
                }

                if (!pointsToPrincipal)
                {
                    var canBeUnique = Internal.Navigation.IsCompatible(
                        navigationName,
                        Metadata.PrincipalEntityType,
                        Metadata.DeclaringEntityType,
                        shouldBeCollection: false,
                        shouldThrow: false);
                    var canBeNonUnique = Internal.Navigation.IsCompatible(
                        navigationName,
                        Metadata.PrincipalEntityType,
                        Metadata.DeclaringEntityType,
                        shouldBeCollection: true,
                        shouldThrow: false);

                    if (canBeUnique != canBeNonUnique)
                    {
                        if (!configurationSource.Overrides(Metadata.GetIsUniqueConfigurationSource())
                            && (Metadata.IsUnique != canBeUnique))
                        {
                            return false;
                        }
                    }
                    else if (!canBeUnique)
                    {
                        return false;
                    }
                }
                else if (!Internal.Navigation.IsCompatible(
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
                    if (!CanSetNavigation(null, conflictingNavigation.IsDependentToPrincipal(), configurationSource))
                    {
                        return false;
                    }
                }
                else if (!conflictingNavigation.ForeignKey.DeclaringEntityType.Builder
                    .CanRemoveForeignKey(conflictingNavigation.ForeignKey, configurationSource))
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
            if (Metadata.IsRequired == isRequired)
            {
                Metadata.SetIsRequired(isRequired, configurationSource);
                return this;
            }

            if (!configurationSource.Overrides(Metadata.GetIsRequiredConfigurationSource()))
            {
                return null;
            }

            if (!CanSetRequiredOnProperties(Metadata.Properties, isRequired, configurationSource, shouldThrow: false))
            {
                if (!configurationSource.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource()))
                {
                    return null;
                }

                return ReplaceForeignKey(configurationSource, dependentProperties: new Property[0], isRequired: isRequired, runConventions: runConventions);
            }

            foreach (var property in Metadata.Properties.Where(p => p.ClrType.IsNullableType()))
            {
                var requiredSet = property.Builder.IsRequired(isRequired, configurationSource);
                if (requiredSet
                    && (isRequired != true))
                {
                    break;
                }
                Debug.Assert(requiredSet || (isRequired != true));
            }

            Metadata.SetIsRequired(isRequired, configurationSource);
            return this;
        }

        private bool CanSetRequired(bool isRequired, ConfigurationSource configurationSource)
        {
            if (Metadata.IsRequired == isRequired)
            {
                return true;
            }

            if (!configurationSource.Overrides(Metadata.GetIsRequiredConfigurationSource()))
            {
                return false;
            }

            if (!configurationSource.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource())
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
                configurationSource,
                shouldThrow);

        private static bool CanSetRequiredOnProperties(
            IEnumerable<Property> properties,
            bool? isRequired,
            EntityType entityType,
            ConfigurationSource configurationSource,
            bool shouldThrow)
        {
            if ((isRequired == null)
                || (properties == null))
            {
                return true;
            }

            if (!ForeignKey.CanPropertiesBeRequired(properties, isRequired, entityType, shouldThrow))
            {
                return false;
            }

            var nullableProperties = properties.Where(p => p.ClrType.IsNullableType());

            return isRequired.Value
                ? nullableProperties.All(property => property.Builder.CanSetRequired(true, configurationSource))
                : nullableProperties.Any(property => property.Builder.CanSetRequired(false, configurationSource));
        }

        public virtual InternalRelationshipBuilder DeleteBehavior(DeleteBehavior deleteBehavior, ConfigurationSource configurationSource)
        {
            if (Metadata.DeleteBehavior == deleteBehavior)
            {
                Metadata.SetDeleteBehavior(deleteBehavior, configurationSource);
                return this;
            }

            if (!configurationSource.Overrides(Metadata.GetDeleteBehaviorConfigurationSource()))
            {
                return null;
            }

            Metadata.SetDeleteBehavior(deleteBehavior, configurationSource);
            return this;
        }

        public virtual bool CanSetDeleteBehavior(DeleteBehavior deleteBehavior, ConfigurationSource configurationSource)
        {
            if (Metadata.DeleteBehavior == deleteBehavior)
            {
                return true;
            }

            if (!configurationSource.Overrides(Metadata.GetDeleteBehaviorConfigurationSource()))
            {
                return false;
            }

            return true;
        }

        public virtual InternalRelationshipBuilder IsUnique(bool unique, ConfigurationSource configurationSource)
            => IsUnique(unique, configurationSource, runConventions: true);

        private InternalRelationshipBuilder IsUnique(bool unique, ConfigurationSource configurationSource, bool runConventions)
        {
            if (Metadata.IsUnique == unique)
            {
                Metadata.SetIsUnique(unique, configurationSource);
                return this;
            }

            if (!configurationSource.Overrides(Metadata.GetIsUniqueConfigurationSource()))
            {
                return null;
            }

            var builder = this;
            if ((Metadata.PrincipalToDependent != null)
                && !Internal.Navigation.IsCompatible(
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

            builder.Metadata.SetIsUnique(unique, configurationSource);
            return builder;
        }

        private bool CanSetUnique(bool isUnique, ConfigurationSource configurationSource)
        {
            if (Metadata.IsUnique == isUnique)
            {
                return true;
            }

            if (!configurationSource.Overrides(Metadata.GetIsUniqueConfigurationSource()))
            {
                return false;
            }

            if ((Metadata.PrincipalToDependent != null)
                && !configurationSource.Overrides(Metadata.GetPrincipalToDependentConfigurationSource())
                && !Internal.Navigation.IsCompatible(
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
                Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);
                return this;
            }

            var shouldInvert = Metadata.PrincipalEntityType.IsAssignableFrom(dependentEntityType)
                               || dependentEntityType.IsAssignableFrom(Metadata.PrincipalEntityType);
            if (!shouldInvert
                && !dependentEntityType.IsAssignableFrom(Metadata.DeclaringEntityType))
            {
                return null;
            }

            if (shouldInvert)
            {
                if (!CanSetUnique(true, configurationSource))
                {
                    return null;
                }

                if (!configurationSource.Overrides(Metadata.GetPrincipalEndConfigurationSource()))
                {
                    return null;
                }

                Debug.Assert(configurationSource.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource()));
                Debug.Assert(configurationSource.Overrides(Metadata.GetPrincipalKeyConfigurationSource()));

                dependentEntityType = Metadata.PrincipalEntityType.IsAssignableFrom(dependentEntityType)
                    ? Metadata.PrincipalEntityType
                    : dependentEntityType;
            }

            var principalEntityType = shouldInvert ? Metadata.DeclaringEntityType : Metadata.PrincipalEntityType;

            var resetToDependent = false;
            var resetToPrincipal = false;
            Property[] dependentProperties = null;
            if (!shouldInvert)
            {
                if ((Metadata.DependentToPrincipal != null)
                    && !Internal.Navigation.IsCompatible(
                        Metadata.DependentToPrincipal.Name,
                        dependentEntityType,
                        principalEntityType,
                        null,
                        shouldThrow: false))
                {
                    if (!configurationSource.Overrides(Metadata.GetDependentToPrincipalConfigurationSource()))
                    {
                        return null;
                    }
                    resetToPrincipal = true;
                }

                if ((Metadata.PrincipalToDependent != null)
                    && !Internal.Navigation.IsCompatible(
                        Metadata.PrincipalToDependent.Name,
                        principalEntityType,
                        dependentEntityType,
                        null,
                        shouldThrow: false))
                {
                    if (!configurationSource.Overrides(Metadata.GetPrincipalToDependentConfigurationSource()))
                    {
                        return null;
                    }
                    resetToDependent = true;
                }

                if (!Property.AreCompatible(Metadata.Properties, dependentEntityType))
                {
                    if (!configurationSource.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource()))
                    {
                        return null;
                    }

                    dependentProperties = new Property[0];
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
                principalEntityTypeBuilder: principalEntityType.Builder,
                dependentEntityTypeBuilder: dependentEntityType.Builder,
                dependentProperties: dependentProperties,
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
                Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);
                return this;
            }

            var shouldInvert = Metadata.DeclaringEntityType.IsAssignableFrom(principalEntityType)
                               || principalEntityType.IsAssignableFrom(Metadata.DeclaringEntityType);
            if (!shouldInvert
                && !principalEntityType.IsAssignableFrom(Metadata.PrincipalEntityType))
            {
                return null;
            }

            if (shouldInvert)
            {
                if (!CanSetUnique(true, configurationSource))
                {
                    return null;
                }

                if (!configurationSource.Overrides(Metadata.GetPrincipalEndConfigurationSource()))
                {
                    return null;
                }

                Debug.Assert(configurationSource.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource()));
                Debug.Assert(configurationSource.Overrides(Metadata.GetPrincipalKeyConfigurationSource()));

                principalEntityType = Metadata.DeclaringEntityType.IsAssignableFrom(principalEntityType)
                    ? Metadata.DeclaringEntityType
                    : principalEntityType;
            }

            var dependentEntityType = shouldInvert ? Metadata.PrincipalEntityType : Metadata.DeclaringEntityType;

            var resetToDependent = false;
            var resetToPrincipal = false;
            Property[] principalProperties = null;
            if (!shouldInvert)
            {
                if ((Metadata.DependentToPrincipal != null)
                    && !Internal.Navigation.IsCompatible(
                        Metadata.DependentToPrincipal.Name,
                        dependentEntityType,
                        principalEntityType,
                        null,
                        shouldThrow: false))
                {
                    if (!configurationSource.Overrides(Metadata.GetDependentToPrincipalConfigurationSource()))
                    {
                        return null;
                    }
                    resetToPrincipal = true;
                }

                if ((Metadata.PrincipalToDependent != null)
                    && !Internal.Navigation.IsCompatible(
                        Metadata.PrincipalToDependent.Name,
                        principalEntityType,
                        dependentEntityType,
                        null,
                        shouldThrow: false))
                {
                    if (!configurationSource.Overrides(Metadata.GetPrincipalToDependentConfigurationSource()))
                    {
                        return null;
                    }
                    resetToDependent = true;
                }

                if (!Property.AreCompatible(Metadata.PrincipalKey.Properties, principalEntityType))
                {
                    if (!configurationSource.Overrides(Metadata.GetPrincipalKeyConfigurationSource()))
                    {
                        return null;
                    }

                    principalProperties = new Property[0];
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
                principalEntityTypeBuilder: principalEntityType.Builder,
                dependentEntityTypeBuilder: dependentEntityType.Builder,
                principalProperties: principalProperties,
                isUnique: shouldInvert ? true : (bool?)null,
                strictPrincipal: shouldInvert,
                oldRelationshipInverted: shouldInvert,
                runConventions: runConventions);
        }

        public virtual bool CanInvert(
            [CanBeNull] IReadOnlyList<Property> newForeignKeyProperties, ConfigurationSource configurationSource)
            => CanInvert(configurationSource)
               && ((newForeignKeyProperties == null)
                   || CanSetForeignKey(newForeignKeyProperties, Metadata.PrincipalEntityType, configurationSource));

        private bool CanInvert(ConfigurationSource configurationSource)
        {
            if (!CanSetUnique(true, configurationSource))
            {
                return false;
            }

            if (!configurationSource.Overrides(Metadata.GetPrincipalEndConfigurationSource()))
            {
                return false;
            }

            return true;
        }

        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] IReadOnlyList<PropertyInfo> properties, ConfigurationSource configurationSource)
            => HasForeignKey(
                Metadata.DeclaringEntityType.Builder.GetOrCreateProperties(properties, configurationSource),
                configurationSource,
                runConventions: true);

        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => HasForeignKey(
                Metadata.DeclaringEntityType.Builder.GetOrCreateProperties(propertyNames, configurationSource, Metadata.PrincipalKey.Properties),
                configurationSource,
                runConventions: true);

        public virtual InternalRelationshipBuilder HasForeignKey(
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
            => HasForeignKey(
                GetExistingProperties(properties, Metadata.DeclaringEntityType), configurationSource, runConventions: true);

        public virtual InternalRelationshipBuilder HasForeignKey(
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource, bool runConventions)
        {
            if (properties == null)
            {
                return !configurationSource.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource())
                    ? null
                    : ReplaceForeignKey(configurationSource,
                        dependentProperties: new Property[0],
                        runConventions: runConventions);
            }

            if (Metadata.Properties.SequenceEqual(properties))
            {
                if (!Metadata.IsSelfReferencing())
                {
                    Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);
                }

                Metadata.UpdateForeignKeyPropertiesConfigurationSource(configurationSource);
                Metadata.UpdateConfigurationSource(configurationSource);
                foreach (var property in properties)
                {
                    property.UpdateConfigurationSource(configurationSource);
                }
                return this;
            }

            if (!configurationSource.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource()))
            {
                return null;
            }

            if ((Metadata.DeclaringEntityType.BaseType != null)
                && (configurationSource != ConfigurationSource.Explicit) // let it throw for explicit
                && properties.Any(p => p.FindContainingKeys().Any(k => k.DeclaringEntityType != Metadata.DeclaringEntityType)))
            {
                return null;
            }

            var resetIsRequired = false;
            if (!CanSetRequiredOnProperties(properties, Metadata.IsRequired, configurationSource, shouldThrow: false))
            {
                if (!configurationSource.Overrides(Metadata.GetIsRequiredConfigurationSource()))
                {
                    return null;
                }
                resetIsRequired = true;
            }

            Property[] principalProperties = null;
            if (Metadata.GetForeignKeyPropertiesConfigurationSource().HasValue
                && !ForeignKey.AreCompatible(
                    Metadata.PrincipalKey.Properties,
                    properties,
                    Metadata.PrincipalEntityType,
                    Metadata.DeclaringEntityType,
                    shouldThrow: false))
            {
                if (!configurationSource.Overrides(Metadata.GetPrincipalKeyConfigurationSource()))
                {
                    return null;
                }

                principalProperties = new Property[0];
            }

            if (resetIsRequired
                && Metadata.GetIsRequiredConfigurationSource().HasValue)
            {
                // ReSharper disable once PossibleInvalidOperationException
                Metadata.SetIsRequired(!Metadata.IsRequired, Metadata.GetIsRequiredConfigurationSource().Value);
            }

            return ReplaceForeignKey(
                configurationSource,
                dependentProperties: properties,
                principalProperties: principalProperties,
                runConventions: runConventions);
        }

        private bool CanSetForeignKey(
            IReadOnlyList<Property> properties, EntityType dependentEntityType, ConfigurationSource configurationSource)
        {
            if (properties != null
                && Metadata.Properties.SequenceEqual(properties))
            {
                return true;
            }

            if (!configurationSource.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource()))
            {
                return false;
            }

            if (properties == null)
            {
                return true;
            }

            if (!configurationSource.Overrides(Metadata.GetIsRequiredConfigurationSource())
                && !CanSetRequiredOnProperties(properties, Metadata.IsRequired, configurationSource, shouldThrow: false))
            {
                return false;
            }

            if (!configurationSource.Overrides(Metadata.GetPrincipalKeyConfigurationSource())
                && (dependentEntityType == Metadata.DeclaringEntityType)
                && !ForeignKey.AreCompatible(
                    Metadata.PrincipalKey.Properties,
                    properties,
                    Metadata.PrincipalEntityType,
                    Metadata.DeclaringEntityType,
                    shouldThrow: false))
            {
                return false;
            }

            var conflictingForeignKey = dependentEntityType.FindForeignKeys(properties).SingleOrDefault();
            if ((conflictingForeignKey != null)
                && !conflictingForeignKey.DeclaringEntityType.Builder.CanRemoveForeignKey(conflictingForeignKey, configurationSource))
            {
                return false;
            }

            return true;
        }

        public virtual InternalRelationshipBuilder HasPrincipalKey([NotNull] IReadOnlyList<PropertyInfo> properties,
            ConfigurationSource configurationSource)
            => HasPrincipalKey(
                Metadata.PrincipalEntityType.Builder.GetOrCreateProperties(properties, configurationSource),
                configurationSource,
                runConventions: true);

        public virtual InternalRelationshipBuilder HasPrincipalKey([NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
            => HasPrincipalKey(
                Metadata.PrincipalEntityType.Builder.GetOrCreateProperties(propertyNames, configurationSource),
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
                    Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);
                }

                Metadata.UpdatePrincipalKeyConfigurationSource(configurationSource);
                Metadata.PrincipalKey.UpdateConfigurationSource(configurationSource);
                return this;
            }

            if (!configurationSource.Overrides(Metadata.GetPrincipalKeyConfigurationSource()))
            {
                return null;
            }

            Property[] dependentProperties = null;
            if (Metadata.GetForeignKeyPropertiesConfigurationSource().HasValue
                && !ForeignKey.AreCompatible(
                    properties,
                    Metadata.Properties,
                    Metadata.PrincipalEntityType,
                    Metadata.DeclaringEntityType,
                    shouldThrow: false))
            {
                if (!configurationSource.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource()))
                {
                    return null;
                }

                dependentProperties = new Property[0];
            }

            return ReplaceForeignKey(
                configurationSource,
                principalProperties: properties,
                dependentProperties: dependentProperties,
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

            if (!configurationSource.Overrides(Metadata.GetPrincipalKeyConfigurationSource()))
            {
                return false;
            }

            if (!configurationSource.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource())
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
                                         (oldRelationshipInverted
                                             ? Metadata.DeclaringEntityType.Builder
                                             : Metadata.PrincipalEntityType.Builder);
            dependentEntityTypeBuilder = dependentEntityTypeBuilder ??
                                         (oldRelationshipInverted
                                             ? Metadata.PrincipalEntityType.Builder
                                             : Metadata.DeclaringEntityType.Builder);

            if (navigationToPrincipalName == null)
            {
                if (oldRelationshipInverted)
                {
                    navigationToPrincipalName = Metadata.GetPrincipalToDependentConfigurationSource()?.Overrides(configurationSource)
                                                ?? false
                        ? Metadata.PrincipalToDependent?.Name ?? ""
                        : null;
                }
                else
                {
                    navigationToPrincipalName = Metadata.GetDependentToPrincipalConfigurationSource()?.Overrides(configurationSource)
                                                ?? false
                        ? Metadata.DependentToPrincipal?.Name ?? ""
                        : null;
                }
            }

            if (navigationToDependentName == null)
            {
                if (oldRelationshipInverted)
                {
                    navigationToDependentName = Metadata.GetDependentToPrincipalConfigurationSource()?.Overrides(configurationSource)
                                                ?? false
                        ? Metadata.DependentToPrincipal?.Name ?? ""
                        : null;
                }
                else
                {
                    navigationToDependentName = Metadata.GetPrincipalToDependentConfigurationSource()?.Overrides(configurationSource)
                                                ?? false
                        ? Metadata.PrincipalToDependent?.Name ?? ""
                        : null;
                }
            }

            dependentProperties = dependentProperties ??
                                  ((Metadata.GetForeignKeyPropertiesConfigurationSource()?.Overrides(configurationSource) ?? false)
                                   && !oldRelationshipInverted
                                      ? Metadata.Properties
                                      : null);
            dependentProperties = dependentProperties?.Count == 0
                ? null
                : dependentProperties;

            principalProperties = principalProperties ??
                                  ((Metadata.GetPrincipalKeyConfigurationSource()?.Overrides(configurationSource) ?? false)
                                   && !oldRelationshipInverted
                                      ? Metadata.PrincipalKey.Properties
                                      : null);
            principalProperties = principalProperties?.Count == 0
                ? null
                : principalProperties;

            isUnique = isUnique ??
                       ((Metadata.GetIsUniqueConfigurationSource()?.Overrides(configurationSource) ?? false)
                           ? Metadata.IsUnique
                           : (bool?)null);

            isRequired = isRequired ??
                         ((Metadata.GetIsRequiredConfigurationSource()?.Overrides(configurationSource) ?? false)
                             ? Metadata.IsRequired
                             : (bool?)null);

            deleteBehavior = deleteBehavior ??
                             ((Metadata.GetDeleteBehaviorConfigurationSource()?.Overrides(configurationSource) ?? false)
                                 ? Metadata.DeleteBehavior
                                 : (DeleteBehavior?)null);

            strictPrincipal = strictPrincipal
                              || (Metadata.GetPrincipalEndConfigurationSource()?.Overrides(configurationSource) ?? false)
                              || ((principalEntityTypeBuilder.Metadata != dependentEntityTypeBuilder.Metadata)
                                  && ((principalProperties != null)
                                      || (dependentProperties != null)));

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
            var addedForeignKeys = new List<InternalRelationshipBuilder>();
            if (conflictingRelationships.Any(relationshipBuilder =>
                !relationshipBuilder.Metadata.DeclaringEntityType.Builder
                    .CanRemoveForeignKey(relationshipBuilder.Metadata, configurationSource)))
            {
                if (dependentProperties == null)
                {
                    return null;
                }

                var conflictingForeignKeyWithSameProperties = conflictingRelationships.FirstOrDefault(r => r.Metadata.Properties.SequenceEqual(dependentProperties));
                if (conflictingForeignKeyWithSameProperties == null
                    || !conflictingForeignKeyWithSameProperties.CanSetForeignKey(null, conflictingForeignKeyWithSameProperties.Metadata.DeclaringEntityType, configurationSource))
                {
                    return null;
                }

                conflictingRelationships.Remove(conflictingForeignKeyWithSameProperties);
                if (conflictingRelationships.Count > 0)
                {
                    return null;
                }
                conflictingForeignKeyWithSameProperties = conflictingForeignKeyWithSameProperties.HasForeignKey(null, configurationSource, runConventions: false);
                addedForeignKeys.Add(conflictingForeignKeyWithSameProperties);
            }

            var shouldUpgradeSource = (dependentProperties != null)
                                      || !string.IsNullOrEmpty(navigationToPrincipalName)
                                      || !string.IsNullOrEmpty(navigationToDependentName);

            var newRelationshipConfigurationSource = shouldUpgradeSource
                ? configurationSource
                : ConfigurationSource.Convention;

            var removedNavigations = new Dictionary<string, Tuple<InternalEntityTypeBuilder, InternalEntityTypeBuilder, string>>();
            var removedForeignKeys = new List<Tuple<InternalEntityTypeBuilder, ForeignKey>>();
            if (Metadata.DeclaringEntityType.GetDeclaredForeignKeys().Contains(Metadata))
            {
                var oldDependentEntityTypeBuilder = Metadata.DeclaringEntityType.Builder;
                var oldPrincipalEntityTypeBuilder = Metadata.PrincipalEntityType.Builder;
                var oldNavigationToPrincipalName = Metadata.DependentToPrincipal?.Name;
                if (oldNavigationToPrincipalName != null)
                {
                    removedNavigations[Metadata.DeclaringEntityType.Name + oldNavigationToPrincipalName] = Tuple.Create(
                        oldDependentEntityTypeBuilder, oldPrincipalEntityTypeBuilder, oldNavigationToPrincipalName);
                }
                var oldNavigationToDependentName = Metadata.PrincipalToDependent?.Name;
                if (oldNavigationToDependentName != null)
                {
                    removedNavigations[Metadata.PrincipalEntityType.Name + oldNavigationToDependentName] = Tuple.Create(
                        oldPrincipalEntityTypeBuilder, oldDependentEntityTypeBuilder, oldNavigationToDependentName);
                }

                var fkOwner = Metadata.DeclaringEntityType.Builder;
                var replacedConfigurationSource = fkOwner.RemoveForeignKey(Metadata, ConfigurationSource.Explicit, runConventions: false);
                Debug.Assert(replacedConfigurationSource.HasValue);

                removedForeignKeys.Add(Tuple.Create(fkOwner, Metadata));
                newRelationshipConfigurationSource = newRelationshipConfigurationSource.Max(replacedConfigurationSource.Value);
            }

            foreach (var relationshipBuilder in conflictingRelationships)
            {
                var foreignKey = relationshipBuilder.Metadata;
                var oldDependentEntityTypeBuilder = foreignKey.DeclaringEntityType.Builder;
                var oldPrincipalEntityTypeBuilder = foreignKey.PrincipalEntityType.Builder;
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
                var fkOwner = foreignKey.DeclaringEntityType.Builder;
                var removed = fkOwner.RemoveForeignKey(foreignKey, configurationSource, runConventions: false);
                Debug.Assert(removed.HasValue);

                removedForeignKeys.Add(Tuple.Create(fkOwner, foreignKey));
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

            newRelationshipBuilder.Metadata.UpdateConfigurationSource(newRelationshipConfigurationSource);

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
                newRelationshipBuilder.Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);
            }
            if (dependentProperties != null)
            {
                newRelationshipBuilder = newRelationshipBuilder.HasForeignKey(
                    dependentProperties,
                    configurationSource.Max(oldRelationshipInverted ? null : Metadata.GetForeignKeyPropertiesConfigurationSource()),
                    runConventions: false)
                                         ?? newRelationshipBuilder;
            }
            if (principalProperties != null)
            {
                newRelationshipBuilder = newRelationshipBuilder.HasPrincipalKey(
                    principalProperties,
                    configurationSource.Max(oldRelationshipInverted ? null : Metadata.GetPrincipalKeyConfigurationSource()),
                    runConventions: false)
                                         ?? newRelationshipBuilder;
            }
            if (isUnique.HasValue)
            {
                newRelationshipBuilder = newRelationshipBuilder.IsUnique(
                    isUnique.Value,
                    configurationSource.Max(Metadata.GetIsUniqueConfigurationSource()),
                    runConventions: false)
                                         ?? newRelationshipBuilder;
            }
            if (isRequired.HasValue)
            {
                newRelationshipBuilder = newRelationshipBuilder.IsRequired(
                    isRequired.Value,
                    configurationSource.Max(Metadata.GetIsRequiredConfigurationSource()),
                    runConventions: false)
                                         ?? newRelationshipBuilder;
            }
            if (deleteBehavior.HasValue)
            {
                newRelationshipBuilder = newRelationshipBuilder.DeleteBehavior(
                    deleteBehavior.Value,
                    configurationSource.Max(Metadata.GetDeleteBehaviorConfigurationSource()))
                                         ?? newRelationshipBuilder;
            }
            if (navigationToPrincipalName != null)
            {
                newRelationshipBuilder = newRelationshipBuilder.Navigation(
                    navigationToPrincipalName == "" ? null : navigationToPrincipalName,
                    pointsToPrincipal: true,
                    configurationSource: configurationSource.Max(oldRelationshipInverted
                        ? Metadata.GetPrincipalToDependentConfigurationSource()
                        : Metadata.GetDependentToPrincipalConfigurationSource()),
                    runConventions: false)
                                         ?? newRelationshipBuilder;
            }
            if (navigationToDependentName != null)
            {
                newRelationshipBuilder = newRelationshipBuilder.Navigation(
                    navigationToDependentName == "" ? null : navigationToDependentName,
                    pointsToPrincipal: false,
                    configurationSource: configurationSource.Max(oldRelationshipInverted
                        ? Metadata.GetDependentToPrincipalConfigurationSource()
                        : Metadata.GetPrincipalToDependentConfigurationSource()),
                    runConventions: false)
                                         ?? newRelationshipBuilder;
            }

            if (!oldRelationshipInverted
                && Metadata.GetPrincipalEndConfigurationSource().HasValue)
            {
                newRelationshipBuilder.Metadata.UpdatePrincipalEndConfigurationSource(
                    Metadata.GetPrincipalEndConfigurationSource().Value);
            }
            if ((dependentProperties == null)
                && !oldRelationshipInverted
                && Metadata.GetForeignKeyPropertiesConfigurationSource().HasValue)
            {
                var oldDependentProperties = GetExistingProperties(
                    Metadata.Properties, newRelationshipBuilder.Metadata.DeclaringEntityType);
                if (oldDependentProperties != null
                    && CanSetRequiredOnProperties(oldDependentProperties, newRelationshipBuilder.Metadata.IsRequired, Metadata.GetForeignKeyPropertiesConfigurationSource().Value, shouldThrow: false)
                    && ForeignKey.AreCompatible(
                        newRelationshipBuilder.Metadata.PrincipalKey.Properties,
                        oldDependentProperties,
                        newRelationshipBuilder.Metadata.PrincipalEntityType,
                        newRelationshipBuilder.Metadata.DeclaringEntityType,
                        shouldThrow: false))
                {
                    newRelationshipBuilder = newRelationshipBuilder.HasForeignKey(
                        oldDependentProperties, Metadata.GetForeignKeyPropertiesConfigurationSource().Value, runConventions: false)
                                             ?? newRelationshipBuilder;
                }
            }
            if ((principalProperties == null)
                && !oldRelationshipInverted
                && Metadata.GetPrincipalKeyConfigurationSource().HasValue)
            {
                var oldPrincipalKey = newRelationshipBuilder.Metadata.PrincipalEntityType.FindKey(Metadata.PrincipalKey.Properties);
                if (oldPrincipalKey != null
                    && ForeignKey.AreCompatible(
                        oldPrincipalKey.Properties,
                        newRelationshipBuilder.Metadata.Properties,
                        newRelationshipBuilder.Metadata.PrincipalEntityType,
                        newRelationshipBuilder.Metadata.DeclaringEntityType,
                        shouldThrow: false))
                {
                    newRelationshipBuilder = newRelationshipBuilder.HasPrincipalKey(
                        oldPrincipalKey.Properties, Metadata.GetPrincipalKeyConfigurationSource().Value, runConventions: false)
                                             ?? newRelationshipBuilder;
                }
            }
            if (!isUnique.HasValue
                && Metadata.GetIsUniqueConfigurationSource().HasValue)
            {
                newRelationshipBuilder = newRelationshipBuilder.IsUnique(
                    Metadata.IsUnique, Metadata.GetIsUniqueConfigurationSource().Value, runConventions: false)
                                         ?? newRelationshipBuilder;
            }
            if (!isRequired.HasValue
                && Metadata.GetIsRequiredConfigurationSource().HasValue
                && CanSetRequiredOnProperties(
                    newRelationshipBuilder.Metadata.Properties, Metadata.IsRequired, configurationSource, shouldThrow: false))
            {
                newRelationshipBuilder = newRelationshipBuilder.IsRequired(
                    Metadata.IsRequired, Metadata.GetIsRequiredConfigurationSource().Value, runConventions: false)
                                         ?? newRelationshipBuilder;
            }
            if (!deleteBehavior.HasValue
                && Metadata.GetDeleteBehaviorConfigurationSource().HasValue)
            {
                newRelationshipBuilder = newRelationshipBuilder.DeleteBehavior(Metadata.DeleteBehavior, Metadata.GetDeleteBehaviorConfigurationSource().Value)
                                         ?? newRelationshipBuilder;
            }

            if (Metadata.DependentToPrincipal != null)
            {
                if (oldRelationshipInverted)
                {
                    if (navigationToDependentName == null)
                    {
                        newRelationshipBuilder = newRelationshipBuilder.Navigation(
                            Metadata.DependentToPrincipal.Name,
                            pointsToPrincipal: false,
                            configurationSource: Metadata.GetDependentToPrincipalConfigurationSource().Value,
                            runConventions: false)
                                                 ?? newRelationshipBuilder;
                    }
                }
                else
                {
                    if (navigationToPrincipalName == null)
                    {
                        newRelationshipBuilder = newRelationshipBuilder.Navigation(
                            Metadata.DependentToPrincipal.Name,
                            pointsToPrincipal: true,
                            configurationSource: Metadata.GetDependentToPrincipalConfigurationSource().Value,
                            runConventions: false)
                                                 ?? newRelationshipBuilder;
                    }
                }
            }

            if (Metadata.PrincipalToDependent != null)
            {
                if (oldRelationshipInverted)
                {
                    if (navigationToPrincipalName == null)
                    {
                        newRelationshipBuilder = newRelationshipBuilder.Navigation(
                            Metadata.PrincipalToDependent.Name,
                            pointsToPrincipal: true,
                            configurationSource: Metadata.GetPrincipalToDependentConfigurationSource().Value,
                            runConventions: false)
                                                 ?? newRelationshipBuilder;
                    }
                }
                else
                {
                    if (navigationToDependentName == null)
                    {
                        newRelationshipBuilder = newRelationshipBuilder.Navigation(
                            Metadata.PrincipalToDependent.Name,
                            pointsToPrincipal: false,
                            configurationSource: Metadata.GetPrincipalToDependentConfigurationSource().Value,
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
                    ModelBuilder.Metadata.ConventionDispatcher.OnNavigationRemoved(
                        removedNavigation.Value.Item1, removedNavigation.Value.Item2, removedNavigation.Value.Item3);
                }

                foreach (var removedForeignKey in removedForeignKeys)
                {
                    ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyRemoved(removedForeignKey.Item1, removedForeignKey.Item2);
                }

                if (newRelationshipBuilder.Metadata.Builder == null)
                {
                    newRelationshipBuilder = FindForeignKey(navigationToPrincipalName,
                        navigationToDependentName,
                        dependentProperties);
                }

                if (newRelationshipBuilder == null)
                {
                    return null;
                }

                newRelationshipBuilder = ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyAdded(newRelationshipBuilder);

                foreach (var addedForeignKey in addedForeignKeys)
                {
                    ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyAdded(addedForeignKey);
                }

                if (newRelationshipBuilder == null)
                {
                    return null;
                }

                var inverted = newRelationshipBuilder.Metadata.DeclaringEntityType != dependentEntityType;
                if ((dependentToPrincipalIsNew && !inverted)
                    || (principalToDependentIsNew && inverted))
                {
                    newRelationshipBuilder = ModelBuilder.Metadata.ConventionDispatcher.OnNavigationAdded(
                        newRelationshipBuilder, newRelationshipBuilder.Metadata.DependentToPrincipal);
                }
                if (newRelationshipBuilder == null)
                {
                    return null;
                }

                if ((principalToDependentIsNew && !inverted)
                    || (dependentToPrincipalIsNew && inverted))
                {
                    newRelationshipBuilder = ModelBuilder.Metadata.ConventionDispatcher.OnNavigationAdded(
                        newRelationshipBuilder, newRelationshipBuilder.Metadata.PrincipalToDependent);
                }
            }

            return newRelationshipBuilder;
        }

        private InternalRelationshipBuilder FindForeignKey(
            string navigationToPrincipalName = null,
            string navigationToDependentName = null,
            IReadOnlyList<Property> dependentProperties = null)
        {
            var matchingRelationships = Metadata.DeclaringEntityType.Builder.GetRelationshipBuilders(
                Metadata.PrincipalEntityType,
                navigationToPrincipalName == "" ? null : navigationToPrincipalName,
                navigationToDependentName == "" ? null : navigationToDependentName,
                dependentProperties);

            matchingRelationships = matchingRelationships.Distinct().ToList();

            return matchingRelationships.Count == 1 ? matchingRelationships.First() : null;
        }

        public virtual InternalRelationshipBuilder Attach(ConfigurationSource configurationSource)
        {
            Debug.Assert(!Metadata.DeclaringEntityType.GetForeignKeys().Contains(Metadata));

            List<Property> dependentProperties = null;
            if (Metadata.GetForeignKeyPropertiesConfigurationSource()?.Overrides(configurationSource) == true)
            {
                dependentProperties = GetExistingProperties(Metadata.Properties, Metadata.DeclaringEntityType) ?? new List<Property>();
            }

            IReadOnlyList<Property> principalProperties = null;
            var principalKey = Metadata.PrincipalEntityType.FindKey(Metadata.PrincipalKey.Properties);
            if (principalKey == null)
            {
                principalProperties = new List<Property>();
                if (Metadata.GetForeignKeyPropertiesConfigurationSource()?.Overrides(ConfigurationSource.Explicit) != true)
                {
                    dependentProperties = new List<Property>();
                }
            }
            else if (Metadata.GetPrincipalKeyConfigurationSource()?.Overrides(configurationSource) == true)
            {
                principalProperties = principalKey.Properties;
            }

            return ReplaceForeignKey(configurationSource,
                dependentProperties: dependentProperties,
                principalProperties: principalProperties);
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
            ConfigurationSource configurationSource)
        {
            var shouldThrow = configurationSource == ConfigurationSource.Explicit;
            if ((dependentProperties != null)
                && !CanSetRequiredOnProperties(
                    dependentProperties,
                    isRequired,
                    dependentEntityType,
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

            if ((dependentProperties != null)
                && !CanSetForeignKey(dependentProperties, inverted ? dependentEntityType : principalEntityType, configurationSource))
            {
                return false;
            }

            if ((principalProperties != null)
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
                navigationToDependentName,
                configurationSource))
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
                navigationToPrincipalName,
                configurationSource))
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
            string navigationToDependentName,
            ConfigurationSource configurationSource)
        {
            if (principalEntityType.IsSameHierarchy(dependentEntityType))
            {
                // The dependent end cannot be determined based on entity types, so use navigations
                if (((navigationToPrincipalName != null)
                     && (Metadata.DependentToPrincipal != null)
                     && (navigationToPrincipalName != Metadata.DependentToPrincipal.Name))
                    || ((navigationToDependentName != null)
                        && (Metadata.PrincipalToDependent != null)
                        && (navigationToDependentName != Metadata.PrincipalToDependent.Name)))
                {
                    return false;
                }
            }

            if (!Metadata.DeclaringEntityType.IsAssignableFrom(dependentEntityType))
            {
                return false;
            }

            if (!Metadata.PrincipalEntityType.IsAssignableFrom(principalEntityType))
            {
                return false;
            }

            if ((navigationToPrincipalName != null)
                && !CanSetNavigation(navigationToPrincipalName, true, configurationSource))
            {
                return false;
            }

            if ((navigationToDependentName != null)
                && !CanSetNavigation(navigationToDependentName, false, configurationSource))
            {
                return false;
            }

            return true;
        }
    }
}
