// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
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
            if (!CanSetRelatedTypes(Metadata.PrincipalEntityType,
                Metadata.DeclaringEntityType,
                navigationToPrincipalName,
                navigationToDependentName,
                configurationSource,
                configurationSource == ConfigurationSource.Explicit,
                out _,
                out _,
                out _,
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

        // Note: These will not invert relationships, use RelatedEntityTypes for that
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

            var builder = this;
            if (Metadata.DeclaringEntityType.IsAssignableFrom(dependentEntityType))
            {
                Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);
                if (runConventions)
                {
                    builder = ModelBuilder.Metadata.ConventionDispatcher.OnPrincipalEndSet(builder);
                }

                return builder;
            }

            if (dependentEntityType.IsAssignableFrom(Metadata.DeclaringEntityType)
                || configurationSource == ConfigurationSource.Explicit)
            {
                return RelatedEntityTypes(Metadata.PrincipalEntityType, dependentEntityType, configurationSource, runConventions);
            }
            
            return null;
        }

        // Note: These will not invert relationships, use RelatedEntityTypes for that
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

            var builder = this;
            if (Metadata.PrincipalEntityType.IsAssignableFrom(principalEntityType))
            {
                Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);
                if (runConventions)
                {
                    builder = ModelBuilder.Metadata.ConventionDispatcher.OnPrincipalEndSet(builder);
                }

                return builder;
            }

            if (principalEntityType.IsAssignableFrom(Metadata.PrincipalEntityType)
                || configurationSource == ConfigurationSource.Explicit)
            {
                return RelatedEntityTypes(principalEntityType, Metadata.DeclaringEntityType, configurationSource, runConventions);
            }
            
            return null;
        }

        public virtual InternalRelationshipBuilder RelatedEntityTypes(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            ConfigurationSource configurationSource,
            bool runConventions = true)
        {
            bool shouldInvert;
            bool shouldResetToPrincipal;
            bool shouldResetToDependent;
            bool shouldResetPrincipalProperties;
            bool shouldResetDependentProperties;
            if (!CanSetRelatedTypes(
                principalEntityType,
                dependentEntityType,
                !CanInvert(configurationSource),
                null,
                null,
                configurationSource,
                configurationSource == ConfigurationSource.Explicit,
                out shouldInvert,
                out shouldResetToPrincipal,
                out shouldResetToDependent,
                out shouldResetPrincipalProperties,
                out shouldResetDependentProperties)
                && configurationSource != ConfigurationSource.Explicit)
            {
                return null;
            }

            var builder = this;
            if (shouldInvert)
            {
                Debug.Assert(configurationSource.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource()));
                Debug.Assert(configurationSource.Overrides(Metadata.GetPrincipalKeyConfigurationSource()));

                principalEntityType = principalEntityType.LeastDerivedType(Metadata.DeclaringEntityType);
                dependentEntityType = dependentEntityType.LeastDerivedType(Metadata.PrincipalEntityType);
            }
            else
            {
                principalEntityType = principalEntityType.LeastDerivedType(Metadata.PrincipalEntityType);
                dependentEntityType = dependentEntityType.LeastDerivedType(Metadata.DeclaringEntityType);

                if (Metadata.PrincipalEntityType == principalEntityType
                    && Metadata.DeclaringEntityType == dependentEntityType)
                {
                    Debug.Assert(!shouldResetToPrincipal
                                 && !shouldResetToDependent
                                 && !shouldResetPrincipalProperties
                                 && !shouldResetDependentProperties);

                    builder.Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);

                    if (runConventions)
                    {
                        builder = ModelBuilder.Metadata.ConventionDispatcher.OnPrincipalEndSet(builder);
                    }

                    return builder;
                }
            }

            return builder.ReplaceForeignKey(
                configurationSource,
                principalEntityTypeBuilder: principalEntityType.Builder,
                dependentEntityTypeBuilder: dependentEntityType.Builder,
                navigationToPrincipalName: shouldResetToPrincipal ? "" : null,
                navigationToDependentName: shouldResetToDependent ? "" : null,
                dependentProperties: shouldResetDependentProperties ? new Property[0] : null,
                principalProperties: shouldResetPrincipalProperties ? new Property[0] : null,
                isUnique: shouldInvert ? true : (bool?)null,
                strictPrincipal: true,
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
                var builder = this;
                builder.Metadata.UpdateForeignKeyPropertiesConfigurationSource(configurationSource);
                builder.Metadata.UpdateConfigurationSource(configurationSource);

                foreach (var property in properties)
                {
                    property.UpdateConfigurationSource(configurationSource);
                }

                if (!Metadata.IsSelfReferencing())
                {
                    Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);
                    if (runConventions)
                    {
                        builder = ModelBuilder.Metadata.ConventionDispatcher.OnPrincipalEndSet(builder);
                    }
                }

                return builder;
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
                var builder = this;
                builder.Metadata.UpdatePrincipalKeyConfigurationSource(configurationSource);
                builder.Metadata.PrincipalKey.UpdateConfigurationSource(configurationSource);

                if (!Metadata.IsSelfReferencing())
                {
                    Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);
                    if (runConventions)
                    {
                        builder = ModelBuilder.Metadata.ConventionDispatcher.OnPrincipalEndSet(builder);
                    }
                }

                return builder;
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

            var settingNewNavigation = (navigationToPrincipalName ?? navigationToDependentName) != null;

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
                                   // To find matching property when adding navigation. Both sides can have matching propery, one of them being matching with navigation name
                                   && (!settingNewNavigation || !ConfigurationSource.Convention.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource()))
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
                    null,
                    null,
                    null,
                    null,
                    null,
                    strictPrincipal,
                    configurationSource,
                    false,
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

            newRelationshipBuilder = newRelationshipBuilder.RelatedEntityTypes(
                principalEntityTypeBuilder.Metadata,
                dependentEntityTypeBuilder.Metadata,
                strictPrincipal ? configurationSource : ConfigurationSource.Convention, runConventions: false);

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

                newRelationshipBuilder = ModelBuilder.Metadata.ConventionDispatcher.OnPrincipalEndSet(newRelationshipBuilder);

                if (newRelationshipBuilder == null)
                {
                    return null;
                }

                var inverted = newRelationshipBuilder.Metadata.DeclaringEntityType != dependentEntityType;
                if ((dependentToPrincipalIsNew && !inverted)
                    || (principalToDependentIsNew && inverted)
                    // RequiredNavigationAttributeConvention only work on DependentToPrincipal. So when inverting relationship, run the convention regardless of it being new/old
                    || ((newRelationshipBuilder.Metadata.DependentToPrincipal != null) && inverted))
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
            bool shouldThrow,
            out bool shouldInvert)
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

            bool shouldResetToPrincipal;
            bool shouldResetToDependent;
            bool shouldResetPrincipalProperties;
            bool shouldResetDependentProperties;
            if (!CanSetRelatedTypes(
                principalEntityType,
                dependentEntityType,
                strictPrincipal && !CanInvert(configurationSource),
                navigationToPrincipalName,
                navigationToDependentName,
                configurationSource,
                shouldThrow,
                out shouldInvert,
                out shouldResetToPrincipal,
                out shouldResetToDependent,
                out shouldResetPrincipalProperties,
                out shouldResetDependentProperties))
            {
                return false;
            }

            if ((dependentProperties != null)
                && !CanSetForeignKey(
                    dependentProperties,
                    configurationSource: configurationSource,
                    dependentEntityType: shouldInvert ? dependentEntityType : principalEntityType))
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

        private bool CanSetRelatedTypes(
            EntityType principalEntityType,
            EntityType dependentEntityType,
            bool strictPrincipal,
            string navigationToPrincipalName,
            string navigationToDependentName,
            ConfigurationSource configurationSource,
            bool shouldThrow,
            out bool shouldInvert,
            out bool shouldResetToPrincipal,
            out bool shouldResetToDependent,
            out bool shouldResetPrincipalProperties,
            out bool shouldResetDependentProperties)
        {
            shouldInvert = false;
            shouldResetToPrincipal = false;
            shouldResetToDependent = false;
            shouldResetPrincipalProperties = false;
            shouldResetDependentProperties = false;
            var someAspectsFitNonInverted = false;
            var sameHierarchyInvertedNavigations =
                principalEntityType.IsSameHierarchy(dependentEntityType)
                && (((navigationToPrincipalName != null)
                     && (navigationToPrincipalName == Metadata.PrincipalToDependent?.Name))
                    || ((navigationToDependentName != null)
                        && (navigationToDependentName == Metadata.DependentToPrincipal?.Name)));

            if (!sameHierarchyInvertedNavigations
                && CanSetRelatedTypes(
                    principalEntityType,
                    dependentEntityType,
                    navigationToPrincipalName,
                    navigationToDependentName,
                    configurationSource,
                    false,
                    out shouldResetToPrincipal,
                    out shouldResetToDependent,
                    out shouldResetPrincipalProperties,
                    out shouldResetDependentProperties))
            {
                if (!shouldResetToPrincipal
                    && !shouldResetToDependent)
                {
                    return true;
                }
                someAspectsFitNonInverted = true;
            }

            bool invertedShouldResetToPrincipal;
            bool invertedShouldResetToDependent;
            bool _;
            if (!strictPrincipal
                && CanSetRelatedTypes(
                    dependentEntityType,
                    principalEntityType,
                    navigationToDependentName,
                    navigationToPrincipalName,
                    configurationSource,
                    false,
                    out invertedShouldResetToPrincipal,
                    out invertedShouldResetToDependent,
                    out _,
                    out _)
                && (!someAspectsFitNonInverted
                    || (!invertedShouldResetToPrincipal
                        && !invertedShouldResetToDependent)))
            {
                shouldInvert = true;
                shouldResetToPrincipal = invertedShouldResetToDependent;
                shouldResetToDependent = invertedShouldResetToPrincipal;
                return true;
            }

            if (!someAspectsFitNonInverted
                && shouldThrow)
            {
                throw new InvalidOperationException(CoreStrings.EntityTypesNotInRelationship(
                    dependentEntityType.DisplayName(),
                    principalEntityType.DisplayName(),
                    Metadata.DeclaringEntityType.DisplayName(),
                    Metadata.PrincipalEntityType.DisplayName()));
            }

            return someAspectsFitNonInverted;
        }

        private bool CanSetRelatedTypes(
            EntityType principalEntityType,
            EntityType dependentEntityType,
            string navigationToPrincipalName,
            string navigationToDependentName,
            ConfigurationSource configurationSource,
            bool shouldThrow,
            out bool shouldResetToPrincipal,
            out bool shouldResetToDependent,
            out bool shouldResetPrincipalProperties,
            out bool shouldResetDependentProperties)
        {
            shouldResetToPrincipal = false;
            shouldResetToDependent = false;
            shouldResetPrincipalProperties = false;
            shouldResetDependentProperties = false;

            if (!Metadata.DeclaringEntityType.IsSameHierarchy(dependentEntityType))
            {
                return false;
            }

            if (!Metadata.PrincipalEntityType.IsSameHierarchy(principalEntityType))
            {
                return false;
            }

            if (navigationToPrincipalName != null)
            {
                if (!CanSetNavigation(navigationToPrincipalName, true, configurationSource))
                {
                    return false;
                }
            }
            else if ((Metadata.DependentToPrincipal != null)
                     && !Internal.Navigation.IsCompatible(
                         Metadata.DependentToPrincipal.Name,
                         dependentEntityType,
                         principalEntityType,
                         shouldBeCollection: null,
                         shouldThrow: shouldThrow))
            {
                if (!CanSetNavigation(null, true, configurationSource))
                {
                    return false;
                }

                shouldResetToPrincipal = true;
            }

            if (navigationToDependentName != null)
            {
                if (!CanSetNavigation(navigationToDependentName, false, configurationSource))
                {
                    return false;
                }
            }
            else if ((Metadata.PrincipalToDependent != null)
                     && !Internal.Navigation.IsCompatible(
                         Metadata.PrincipalToDependent.Name,
                         principalEntityType,
                         dependentEntityType,
                         shouldBeCollection: null,
                         shouldThrow: shouldThrow))
            {
                if (!CanSetNavigation(null, false, configurationSource))
                {
                    return false;
                }

                shouldResetToDependent = true;
            }

            if (!Property.AreCompatible(Metadata.PrincipalKey.Properties, principalEntityType))
            {
                if (!configurationSource.Overrides(Metadata.GetPrincipalKeyConfigurationSource()))
                {
                    return false;
                }

                shouldResetPrincipalProperties = true;
            }

            if (!Property.AreCompatible(Metadata.Properties, dependentEntityType))
            {
                if (!configurationSource.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource()))
                {
                    return false;
                }

                shouldResetDependentProperties = true;
            }

            return true;
        }
    }
}
