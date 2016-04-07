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
            navigationToPrincipalName = navigationToPrincipalName ?? "";
            navigationToDependentName = navigationToDependentName ?? "";
            bool _;
            if (!CanSetRelatedTypes(Metadata.PrincipalEntityType,
                Metadata.DeclaringEntityType,
                navigationToPrincipalName,
                navigationToDependentName,
                configurationSource,
                false,
                configurationSource == ConfigurationSource.Explicit,
                out _,
                out _,
                out _,
                out _))
            {
                return null;
            }

            var newRelationshipBuilder = ReplaceForeignKey(configurationSource,
                navigationToPrincipalName: navigationToPrincipalName,
                navigationToDependentName: navigationToDependentName);

            if (newRelationshipBuilder != null
                && newRelationshipBuilder.Metadata.Builder == null)
            {
                newRelationshipBuilder = FindCurrentRelationshipBuilder(
                    Metadata.PrincipalEntityType,
                    Metadata.DeclaringEntityType,
                    navigationToPrincipalName,
                    navigationToDependentName)
                                         ?? FindCurrentRelationshipBuilder(
                                             Metadata.DeclaringEntityType,
                                             Metadata.PrincipalEntityType,
                                             navigationToDependentName,
                                             navigationToPrincipalName);
            }

            if (newRelationshipBuilder == null
                || ((navigationToPrincipalName != (newRelationshipBuilder.Metadata.DependentToPrincipal?.Name ?? "")
                     || navigationToDependentName != (newRelationshipBuilder.Metadata.PrincipalToDependent?.Name ?? ""))
                    && (navigationToDependentName != (newRelationshipBuilder.Metadata.DependentToPrincipal?.Name ?? "")
                        || navigationToPrincipalName != (newRelationshipBuilder.Metadata.PrincipalToDependent?.Name ?? ""))))
            {
                return null;
            }

            return newRelationshipBuilder;
        }

        private InternalRelationshipBuilder Navigation(
            [CanBeNull] string navigationName,
            bool pointsToPrincipal,
            ConfigurationSource? configurationSource,
            bool runConventions)
        {
            var oldNavigation = pointsToPrincipal ? Metadata.DependentToPrincipal : Metadata.PrincipalToDependent;
            if (navigationName == oldNavigation?.Name)
            {
                if (configurationSource.HasValue)
                {
                    Metadata.UpdateConfigurationSource(configurationSource.Value);

                    if (pointsToPrincipal)
                    {
                        Metadata.HasDependentToPrincipal(navigationName, configurationSource.Value, runConventions);
                    }
                    else
                    {
                        Metadata.HasPrincipalToDependent(navigationName, configurationSource.Value, runConventions);
                    }
                }
                return this;
            }

            bool removeOppositeNavigation;
            bool? shouldBeUnique;
            if (!CanSetNavigation(
                navigationName,
                pointsToPrincipal,
                configurationSource,
                shouldThrow: configurationSource == ConfigurationSource.Explicit,
                overrideSameSource: true,
                shouldBeUnique: out shouldBeUnique,
                removeOppositeNavigation: out removeOppositeNavigation))
            {
                return null;
            }

            Debug.Assert(configurationSource.HasValue);

            var builder = this;
            if (removeOppositeNavigation)
            {
                builder = builder.Navigation(null, !pointsToPrincipal, configurationSource, runConventions);
            }

            if (runConventions)
            {
                navigationName = navigationName ?? "";
                return builder.ReplaceForeignKey(
                    configurationSource,
                    navigationToPrincipalName: pointsToPrincipal ? navigationName : null,
                    navigationToDependentName: pointsToPrincipal ? null : navigationName,
                    isUnique: shouldBeUnique);
            }

            if (shouldBeUnique.HasValue)
            {
                builder = builder.IsUnique(shouldBeUnique.Value, configurationSource.Value, false);
            }

            if (navigationName != null)
            {
                var entityTypeBuilder = pointsToPrincipal
                    ? Metadata.DeclaringEntityType.Builder
                    : Metadata.PrincipalEntityType.Builder;
                entityTypeBuilder.Unignore(navigationName);
            }

            if (pointsToPrincipal)
            {
                builder.Metadata.HasDependentToPrincipal(navigationName, configurationSource.Value, runConventions: false);
            }
            else
            {
                builder.Metadata.HasPrincipalToDependent(navigationName, configurationSource.Value, runConventions: false);
            }

            return builder;
        }

        public virtual bool CanSetNavigation(
            [CanBeNull] string navigationName,
            bool pointsToPrincipal,
            ConfigurationSource? configurationSource,
            bool overrideSameSource = true)
        {
            bool? _;
            bool __;
            return CanSetNavigation(
                navigationName,
                pointsToPrincipal,
                configurationSource,
                false,
                overrideSameSource,
                out _,
                out __);
        }

        private bool CanSetNavigation(
            string navigationName,
            bool pointsToPrincipal,
            ConfigurationSource? configurationSource,
            bool shouldThrow,
            bool overrideSameSource,
            out bool? shouldBeUnique,
            out bool removeOppositeNavigation)
        {
            shouldBeUnique = null;
            removeOppositeNavigation = false;

            var existingNavigation = pointsToPrincipal ? Metadata.DependentToPrincipal : Metadata.PrincipalToDependent;
            if (navigationName == existingNavigation?.Name)
            {
                return true;
            }

            if (!configurationSource.HasValue)
            {
                return false;
            }

            if (pointsToPrincipal
                && (!configurationSource.Value.Overrides(Metadata.GetDependentToPrincipalConfigurationSource())
                    || (!overrideSameSource && configurationSource.Value == Metadata.GetDependentToPrincipalConfigurationSource())))
            {
                return false;
            }

            if (!pointsToPrincipal
                && (!configurationSource.Value.Overrides(Metadata.GetPrincipalToDependentConfigurationSource())
                    || (!overrideSameSource && configurationSource.Value == Metadata.GetPrincipalToDependentConfigurationSource())))
            {
                return false;
            }

            var entityTypeBuilder = pointsToPrincipal
                ? Metadata.DeclaringEntityType.Builder
                : Metadata.PrincipalEntityType.Builder;

            if (navigationName == null)
            {
                return true;
            }

            if (entityTypeBuilder.IsIgnored(navigationName, configurationSource.Value))
            {
                return false;
            }

            if (!Internal.Navigation.IsCompatible(
                navigationName,
                pointsToPrincipal,
                Metadata.DeclaringEntityType,
                Metadata.PrincipalEntityType,
                shouldThrow,
                out shouldBeUnique))
            {
                return false;
            }

            if (shouldBeUnique.HasValue
                && (Metadata.IsUnique != shouldBeUnique)
                && !configurationSource.Value.Overrides(Metadata.GetIsUniqueConfigurationSource()))
            {
                return false;
            }

            foreach (var conflictingNavigation in entityTypeBuilder.Metadata.FindNavigationsInHierarchy(navigationName))
            {
                if (conflictingNavigation.ForeignKey == Metadata)
                {
                    Debug.Assert(conflictingNavigation.IsDependentToPrincipal() != pointsToPrincipal);

                    if (!pointsToPrincipal
                        && !configurationSource.Value.Overrides(Metadata.GetDependentToPrincipalConfigurationSource())
                        || (!overrideSameSource && configurationSource.Value == Metadata.GetDependentToPrincipalConfigurationSource()))
                    {
                        return false;
                    }

                    if (pointsToPrincipal
                        && !configurationSource.Value.Overrides(Metadata.GetPrincipalToDependentConfigurationSource())
                        || (!overrideSameSource && configurationSource.Value == Metadata.GetPrincipalToDependentConfigurationSource()))
                    {
                        return false;
                    }

                    removeOppositeNavigation = true;
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

            if (!CanSetRequiredOnProperties(
                Metadata.Properties,
                isRequired,
                Metadata.DeclaringEntityType,
                configurationSource,
                shouldThrow: false))
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

        public virtual bool CanSetRequired(bool isRequired, ConfigurationSource? configurationSource)
        {
            if (Metadata.IsRequired == isRequired)
            {
                return true;
            }

            if (!configurationSource.HasValue
                || !configurationSource.Value.Overrides(Metadata.GetIsRequiredConfigurationSource()))
            {
                return false;
            }

            if (!configurationSource.Value.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource())
                && !CanSetRequiredOnProperties(
                    Metadata.Properties,
                    isRequired,
                    Metadata.DeclaringEntityType,
                    configurationSource,
                    shouldThrow: false))
            {
                return false;
            }

            return true;
        }

        private static bool CanSetRequiredOnProperties(
            IReadOnlyList<Property> properties,
            bool? isRequired,
            EntityType entityType,
            ConfigurationSource? configurationSource,
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

        public virtual bool CanSetDeleteBehavior(DeleteBehavior deleteBehavior, ConfigurationSource? configurationSource)
        {
            if (Metadata.DeleteBehavior == deleteBehavior)
            {
                return true;
            }

            if (!configurationSource.HasValue
                || !configurationSource.Value.Overrides(Metadata.GetDeleteBehaviorConfigurationSource()))
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

            bool resetToDependent;
            if (!CanSetUnique(unique, configurationSource, out resetToDependent))
            {
                return null;
            }

            var builder = this;
            if (resetToDependent)
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

        private bool CanSetUnique(bool unique, ConfigurationSource? configurationSource, out bool resetToDependent)
        {
            resetToDependent = false;
            if (Metadata.IsUnique == unique)
            {
                return true;
            }

            if (!configurationSource.HasValue
                || !configurationSource.Value.Overrides(Metadata.GetIsUniqueConfigurationSource()))
            {
                return false;
            }

            if ((Metadata.PrincipalToDependent != null)
                && !Internal.Navigation.IsCompatible(
                    Metadata.PrincipalToDependent.Name,
                    Metadata.PrincipalEntityType,
                    Metadata.DeclaringEntityType,
                    !unique,
                    shouldThrow: false))
            {
                if (!configurationSource.Value.Overrides(Metadata.GetPrincipalToDependentConfigurationSource()))
                {
                    return false;
                }

                resetToDependent = true;
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
                if (Metadata.GetPrincipalEndConfigurationSource()?.Overrides(configurationSource) != true)
                {
                    Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);
                    if (runConventions)
                    {
                        builder = ModelBuilder.Metadata.ConventionDispatcher.OnPrincipalEndSet(builder);
                    }
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
                if (Metadata.GetPrincipalEndConfigurationSource()?.Overrides(configurationSource) != true)
                {
                    Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);
                    if (runConventions)
                    {
                        builder = ModelBuilder.Metadata.ConventionDispatcher.OnPrincipalEndSet(builder);
                    }
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
            ConfigurationSource? configurationSource,
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
                ConfigurationSource.Explicit,
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
                Debug.Assert(!configurationSource.HasValue
                             || configurationSource.Value.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource()));
                Debug.Assert(!configurationSource.HasValue
                             || configurationSource.Value.Overrides(Metadata.GetPrincipalKeyConfigurationSource()));

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

                    if (configurationSource.HasValue
                        && Metadata.GetPrincipalEndConfigurationSource()?.Overrides(configurationSource) != true)
                    {
                        builder.Metadata.UpdatePrincipalEndConfigurationSource(configurationSource.Value);
                        if (runConventions)
                        {
                            builder = ModelBuilder.Metadata.ConventionDispatcher.OnPrincipalEndSet(builder);
                        }
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
                strictPrincipal: true,
                oldRelationshipInverted: shouldInvert,
                runConventions: runConventions);
        }

        public virtual bool CanInvert(
            [CanBeNull] IReadOnlyList<Property> newForeignKeyProperties, ConfigurationSource configurationSource)
            => CanInvert(configurationSource)
               && ((newForeignKeyProperties == null)
                   || CanSetForeignKey(newForeignKeyProperties, configurationSource, Metadata.PrincipalEntityType));

        private bool CanInvert(ConfigurationSource? configurationSource)
        {
            if (configurationSource == null
                || !configurationSource.Value.Overrides(Metadata.GetPrincipalEndConfigurationSource()))
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
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource? configurationSource, bool runConventions)
        {
            if (properties == null)
            {
                return !configurationSource.HasValue
                       || !configurationSource.Value.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource())
                    ? null
                    : ReplaceForeignKey(configurationSource,
                        dependentProperties: new Property[0],
                        runConventions: runConventions);
            }

            var builder = this;
            if (Metadata.Properties.SequenceEqual(properties))
            {
                if (!configurationSource.HasValue)
                {
                    return this;
                }

                builder.Metadata.UpdateForeignKeyPropertiesConfigurationSource(configurationSource.Value);
                builder.Metadata.UpdateConfigurationSource(configurationSource.Value);

                foreach (var property in properties)
                {
                    property.UpdateConfigurationSource(configurationSource.Value);
                }

                if (!Metadata.IsSelfReferencing()
                    && Metadata.GetPrincipalEndConfigurationSource()?.Overrides(configurationSource) != true)
                {
                    Metadata.UpdatePrincipalEndConfigurationSource(configurationSource.Value);
                    if (runConventions)
                    {
                        builder = ModelBuilder.Metadata.ConventionDispatcher.OnPrincipalEndSet(builder);
                    }
                }

                return builder;
            }

            bool resetIsRequired;
            bool resetPrincipalKey;
            if (!CanSetForeignKey(properties, Metadata.DeclaringEntityType, configurationSource, out resetIsRequired, out resetPrincipalKey))
            {
                return null;
            }

            // FKs are not allowed to use properties from inherited keys since this could result in an ambiguous value space
            Debug.Assert(configurationSource.HasValue);

            if ((Metadata.DeclaringEntityType.BaseType != null)
                && (configurationSource != ConfigurationSource.Explicit) // let it throw for explicit
                && properties.Any(p => p.FindContainingKeys().Any(k => k.DeclaringEntityType != Metadata.DeclaringEntityType)))
            {
                return null;
            }

            if (resetIsRequired)
            {
                Metadata.SetIsRequiredConfigurationSource(null);
            }

            return builder.ReplaceForeignKey(
                configurationSource,
                dependentProperties: properties,
                principalProperties: resetPrincipalKey ? new Property[0] : null,
                runConventions: runConventions);
        }

        private bool CanSetForeignKey(
            IReadOnlyList<Property> properties,
            ConfigurationSource? configurationSource,
            EntityType dependentEntityType = null,
            bool overrideSameSource = true)
        {
            bool _;
            return CanSetForeignKey(
                properties,
                dependentEntityType,
                configurationSource,
                out _,
                out _,
                overrideSameSource);
        }

        private bool CanSetForeignKey(
            IReadOnlyList<Property> properties,
            EntityType dependentEntityType,
            ConfigurationSource? configurationSource,
            out bool resetIsRequired,
            out bool resetPrincipalKey,
            bool overrideSameSource = true)
        {
            resetIsRequired = false;
            resetPrincipalKey = false;
            if (properties != null
                && Metadata.Properties.SequenceEqual(properties))
            {
                return true;
            }

            if (!configurationSource.HasValue
                || !configurationSource.Value.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource())
                || (!overrideSameSource && configurationSource.Value == Metadata.GetForeignKeyPropertiesConfigurationSource()))
            {
                return false;
            }

            if (properties == null)
            {
                return true;
            }

            if (dependentEntityType == null)
            {
                dependentEntityType = Metadata.DeclaringEntityType;
            }

            if (!CanSetRequiredOnProperties(
                properties,
                Metadata.IsRequired,
                dependentEntityType,
                configurationSource,
                shouldThrow: false))
            {
                if (!configurationSource.Value.Overrides(Metadata.GetIsRequiredConfigurationSource()))
                {
                    return false;
                }

                resetIsRequired = true;
            }

            if ((dependentEntityType != Metadata.DeclaringEntityType)
                || !ForeignKey.AreCompatible(
                    Metadata.PrincipalKey.Properties,
                    properties,
                    Metadata.PrincipalEntityType,
                    Metadata.DeclaringEntityType,
                    shouldThrow: false))
            {
                if (!configurationSource.Value.Overrides(Metadata.GetPrincipalKeyConfigurationSource()))
                {
                    return false;
                }

                resetPrincipalKey = true;
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
            bool resetDependent;
            if (!CanSetPrincipalKey(properties, configurationSource, out resetDependent))
            {
                return null;
            }

            if (Metadata.PrincipalKey.Properties.SequenceEqual(properties))
            {
                var builder = this;
                builder.Metadata.UpdatePrincipalKeyConfigurationSource(configurationSource);
                builder.Metadata.PrincipalKey.UpdateConfigurationSource(configurationSource);

                if (!Metadata.IsSelfReferencing()
                    && Metadata.GetPrincipalEndConfigurationSource()?.Overrides(configurationSource) != true)
                {
                    Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);
                    if (runConventions)
                    {
                        builder = ModelBuilder.Metadata.ConventionDispatcher.OnPrincipalEndSet(builder);
                    }
                }

                return builder;
            }

            return ReplaceForeignKey(
                configurationSource,
                principalProperties: properties,
                dependentProperties: resetDependent ? new Property[0] : null,
                runConventions: runConventions);
        }

        private bool CanSetPrincipalKey(
            IReadOnlyList<Property> properties,
            ConfigurationSource? configurationSource,
            out bool resetDependent)
        {
            resetDependent = false;
            if (properties == null)
            {
                return false;
            }

            if (Metadata.PrincipalKey.Properties.SequenceEqual(properties))
            {
                return true;
            }

            if (!configurationSource.HasValue
                || !configurationSource.Value.Overrides(Metadata.GetPrincipalKeyConfigurationSource()))
            {
                return false;
            }

            if (!ForeignKey.AreCompatible(
                    properties,
                    Metadata.Properties,
                    Metadata.PrincipalEntityType,
                    Metadata.DeclaringEntityType,
                    shouldThrow: false))
            {
                if (!configurationSource.Value.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource()))
                {
                    return false;
                }

                resetDependent = true;
            }

            return true;
        }

        private InternalRelationshipBuilder ReplaceForeignKey(
            ConfigurationSource? configurationSource,
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

            principalProperties = principalProperties ??
                                  ((Metadata.GetPrincipalKeyConfigurationSource()?.Overrides(configurationSource) ?? false)
                                   && !oldRelationshipInverted
                                      ? Metadata.PrincipalKey.Properties
                                      : null);

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

            var principalEndConfigurationSource =
                strictPrincipal
                || (principalEntityTypeBuilder.Metadata != dependentEntityTypeBuilder.Metadata
                    && (principalProperties != null
                        || dependentProperties != null
                        || (navigationToDependentName != null && isUnique == false)))
                    ? configurationSource
                    : null;
            principalEndConfigurationSource = principalEndConfigurationSource.Max(Metadata.GetPrincipalEndConfigurationSource());

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
                oldRelationshipInverted,
                principalEndConfigurationSource,
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
            bool oldRelationshipInverted,
            ConfigurationSource? principalEndConfigurationSource,
            ConfigurationSource? configurationSource,
            bool runConventions)
        {
            Check.NotNull(principalEntityTypeBuilder, nameof(principalEntityTypeBuilder));
            Check.NotNull(dependentEntityTypeBuilder, nameof(dependentEntityTypeBuilder));
            Debug.Assert(AreCompatible(
                principalEntityTypeBuilder.Metadata,
                dependentEntityTypeBuilder.Metadata,
                navigationToPrincipalName == "" ? null : navigationToPrincipalName,
                navigationToDependentName == "" ? null : navigationToDependentName,
                dependentProperties != null && dependentProperties.Any() ? dependentProperties : null,
                principalProperties != null && principalProperties.Any() ? principalProperties : null,
                isUnique,
                isRequired,
                configurationSource));

            var newRelationshipConfigurationSource = Metadata.GetConfigurationSource();
            if ((dependentProperties != null && dependentProperties.Any())
                || !string.IsNullOrEmpty(navigationToPrincipalName)
                || !string.IsNullOrEmpty(navigationToDependentName))
            {
                newRelationshipConfigurationSource = newRelationshipConfigurationSource.Max(configurationSource);
            }

            var dependentEntityType = dependentEntityTypeBuilder.Metadata;
            var principalEntityType = principalEntityTypeBuilder.Metadata;
            var removedNavigations = new Dictionary<string, Tuple<InternalEntityTypeBuilder, InternalEntityTypeBuilder, string>>();
            var removedForeignKeys = new List<Tuple<InternalEntityTypeBuilder, ForeignKey>>();
            var addedForeignKeys = new List<InternalRelationshipBuilder>();
            bool existingRelationshipInverted;
            var newRelationshipBuilder = GetOrCreateRelationshipBuilder(
                principalEntityType,
                dependentEntityType,
                navigationToPrincipalName,
                navigationToDependentName,
                dependentProperties != null && dependentProperties.Any() ? dependentProperties : null,
                principalProperties != null && principalProperties.Any() ? principalProperties : null,
                isRequired,
                principalEndConfigurationSource,
                configurationSource,
                removedNavigations,
                removedForeignKeys,
                addedForeignKeys,
                out existingRelationshipInverted);

            if (newRelationshipBuilder == null)
            {
                return null;
            }

            var existingPrincipalEndConfigurationSource = newRelationshipBuilder.Metadata.GetPrincipalEndConfigurationSource();
            var strictPrincipal = principalEndConfigurationSource.HasValue
                                  && principalEndConfigurationSource.Value.Overrides(existingPrincipalEndConfigurationSource);
            if (existingRelationshipInverted
                && !strictPrincipal)
            {
                oldRelationshipInverted = !oldRelationshipInverted;
                existingRelationshipInverted = false;

                var entityTypeBuilder = principalEntityTypeBuilder;
                principalEntityTypeBuilder = dependentEntityTypeBuilder;
                dependentEntityTypeBuilder = entityTypeBuilder;

                dependentEntityType = dependentEntityTypeBuilder.Metadata;
                principalEntityType = principalEntityTypeBuilder.Metadata;

                var navigationName = navigationToPrincipalName;
                navigationToPrincipalName = navigationToDependentName;
                navigationToDependentName = navigationName;
            }

            newRelationshipBuilder.Metadata.UpdateConfigurationSource(newRelationshipConfigurationSource);

            newRelationshipBuilder = newRelationshipBuilder.RelatedEntityTypes(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                navigationToPrincipalName,
                navigationToDependentName,
                dependentProperties,
                principalEndConfigurationSource,
                configurationSource,
                existingRelationshipInverted);

            if (dependentProperties != null
                && dependentProperties.Any())
            {
                newRelationshipBuilder = newRelationshipBuilder.HasForeignKey(
                    dependentProperties,
                    configurationSource.Max(oldRelationshipInverted ? null : Metadata.GetForeignKeyPropertiesConfigurationSource()).Value,
                    runConventions: false)
                                         ?? newRelationshipBuilder;
            }
            if (principalProperties != null
                && principalProperties.Any())
            {
                newRelationshipBuilder = newRelationshipBuilder.HasPrincipalKey(
                    principalProperties,
                    configurationSource.Max(oldRelationshipInverted ? null : Metadata.GetPrincipalKeyConfigurationSource()).Value,
                    runConventions: false)
                                         ?? newRelationshipBuilder;
            }
            if (isUnique.HasValue)
            {
                newRelationshipBuilder = newRelationshipBuilder.IsUnique(
                    isUnique.Value,
                    configurationSource.Max(Metadata.GetIsUniqueConfigurationSource()).Value,
                    runConventions: false)
                                         ?? newRelationshipBuilder;
            }
            if (isRequired.HasValue)
            {
                newRelationshipBuilder = newRelationshipBuilder.IsRequired(
                    isRequired.Value,
                    configurationSource.Max(Metadata.GetIsRequiredConfigurationSource()).Value,
                    runConventions: false)
                                         ?? newRelationshipBuilder;
            }
            if (deleteBehavior.HasValue)
            {
                newRelationshipBuilder = newRelationshipBuilder.DeleteBehavior(
                    deleteBehavior.Value,
                    configurationSource.Max(Metadata.GetDeleteBehaviorConfigurationSource()).Value)
                                         ?? newRelationshipBuilder;
            }
            if (navigationToPrincipalName != null)
            {
                newRelationshipBuilder = newRelationshipBuilder.Navigation(
                    navigationToPrincipalName == "" ? null : navigationToPrincipalName,
                    pointsToPrincipal: true,
                    configurationSource: configurationSource.Max(oldRelationshipInverted
                        ? Metadata.GetPrincipalToDependentConfigurationSource()
                        : Metadata.GetDependentToPrincipalConfigurationSource()).Value,
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
                        : Metadata.GetPrincipalToDependentConfigurationSource()).Value,
                    runConventions: false)
                                         ?? newRelationshipBuilder;
            }

            if ((dependentProperties == null)
                && !oldRelationshipInverted
                && Metadata.GetForeignKeyPropertiesConfigurationSource().HasValue)
            {
                var oldDependentProperties = GetExistingProperties(
                    Metadata.Properties, newRelationshipBuilder.Metadata.DeclaringEntityType);
                if (oldDependentProperties != null
                    && CanSetRequiredOnProperties(
                        oldDependentProperties,
                        newRelationshipBuilder.Metadata.IsRequired,
                        Metadata.DeclaringEntityType,
                        Metadata.GetForeignKeyPropertiesConfigurationSource().Value,
                        shouldThrow: false)
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
                    newRelationshipBuilder.Metadata.Properties,
                    Metadata.IsRequired,
                    Metadata.DeclaringEntityType,
                    configurationSource,
                    shouldThrow: false))
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
                        newRelationshipBuilder.Metadata.DeclaringEntityType.Name + "." +
                        newRelationshipBuilder.Metadata.DependentToPrincipal.Name);
                }

                var principalToDependentIsNew = false;
                if (newRelationshipBuilder.Metadata.PrincipalToDependent != null)
                {
                    principalToDependentIsNew = !removedNavigations.Remove(
                        newRelationshipBuilder.Metadata.PrincipalEntityType.Name + "." +
                        newRelationshipBuilder.Metadata.PrincipalToDependent.Name);
                }

                foreach (var removedNavigation in removedNavigations.Values)
                {
                    ModelBuilder.Metadata.ConventionDispatcher.OnNavigationRemoved(
                        removedNavigation.Item1, removedNavigation.Item2, removedNavigation.Item3);
                }

                foreach (var removedForeignKey in removedForeignKeys)
                {
                    ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyRemoved(removedForeignKey.Item1, removedForeignKey.Item2);
                }

                if (newRelationshipBuilder == null)
                {
                    return null;
                }

                if (newRelationshipBuilder.Metadata.Builder == null)
                {
                    newRelationshipBuilder = FindCurrentRelationshipBuilder(
                        principalEntityType,
                        dependentEntityType,
                        navigationToPrincipalName,
                        navigationToDependentName,
                        dependentProperties,
                        principalProperties);
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

                if (newRelationshipBuilder.Metadata.Builder == null)
                {
                    newRelationshipBuilder = FindCurrentRelationshipBuilder(
                        principalEntityType,
                        dependentEntityType,
                        navigationToPrincipalName,
                        navigationToDependentName,
                        dependentProperties,
                        principalProperties);
                }

                if (strictPrincipal
                    && existingPrincipalEndConfigurationSource != principalEndConfigurationSource)
                {
                    newRelationshipBuilder = ModelBuilder.Metadata.ConventionDispatcher.OnPrincipalEndSet(newRelationshipBuilder);
                }

                if (newRelationshipBuilder == null)
                {
                    return null;
                }

                if (newRelationshipBuilder.Metadata.Builder == null)
                {
                    newRelationshipBuilder = FindCurrentRelationshipBuilder(
                        principalEntityType,
                        dependentEntityType,
                        navigationToPrincipalName,
                        navigationToDependentName,
                        dependentProperties,
                        principalProperties);
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

                if (newRelationshipBuilder.Metadata.Builder == null)
                {
                    newRelationshipBuilder = FindCurrentRelationshipBuilder(
                        principalEntityType,
                        dependentEntityType,
                        navigationToPrincipalName,
                        navigationToDependentName,
                        dependentProperties,
                        principalProperties);
                }

                if ((principalToDependentIsNew && !inverted)
                    || (dependentToPrincipalIsNew && inverted))
                {
                    newRelationshipBuilder = ModelBuilder.Metadata.ConventionDispatcher.OnNavigationAdded(
                        newRelationshipBuilder, newRelationshipBuilder.Metadata.PrincipalToDependent);
                }

                if (newRelationshipBuilder == null)
                {
                    return null;
                }

                if (newRelationshipBuilder.Metadata.Builder == null)
                {
                    newRelationshipBuilder = FindCurrentRelationshipBuilder(
                        principalEntityType,
                        dependentEntityType,
                        navigationToPrincipalName,
                        navigationToDependentName,
                        dependentProperties,
                        principalProperties);
                }
            }

            return newRelationshipBuilder;
        }

        private InternalRelationshipBuilder RelatedEntityTypes(
            InternalEntityTypeBuilder principalEntityTypeBuilder,
            InternalEntityTypeBuilder dependentEntityTypeBuilder,
            string navigationToPrincipalName,
            string navigationToDependentName,
            IReadOnlyList<Property> dependentProperties,
            ConfigurationSource? principalEndConfigurationSource,
            ConfigurationSource? configurationSource,
            bool existingRelationshipInverted)
        {
            var newRelationshipBuilder = this;
            if (newRelationshipBuilder.Metadata.DependentToPrincipal != null
                && ((!existingRelationshipInverted
                     && navigationToPrincipalName != null
                     && navigationToPrincipalName != newRelationshipBuilder.Metadata.DependentToPrincipal.Name)
                    || (existingRelationshipInverted
                        && navigationToDependentName != null
                        && navigationToDependentName != newRelationshipBuilder.Metadata.DependentToPrincipal.Name)))
            {
                newRelationshipBuilder = newRelationshipBuilder.Navigation(
                    null,
                    pointsToPrincipal: true,
                    configurationSource: configurationSource,
                    runConventions: false)
                                         ?? newRelationshipBuilder;
            }
            if (newRelationshipBuilder.Metadata.PrincipalToDependent != null
                && ((!existingRelationshipInverted
                     && navigationToDependentName != null
                     && navigationToDependentName != newRelationshipBuilder.Metadata.PrincipalToDependent.Name)
                    || (existingRelationshipInverted
                        && navigationToPrincipalName != null
                        && navigationToPrincipalName != newRelationshipBuilder.Metadata.PrincipalToDependent.Name)))
            {
                newRelationshipBuilder = newRelationshipBuilder.Navigation(
                    null,
                    pointsToPrincipal: false,
                    configurationSource: configurationSource,
                    runConventions: false)
                                         ?? newRelationshipBuilder;
            }
            if (newRelationshipBuilder.Metadata.GetForeignKeyPropertiesConfigurationSource() != null
                && dependentProperties != null
                && !dependentProperties.SequenceEqual(newRelationshipBuilder.Metadata.Properties))
            {
                newRelationshipBuilder = newRelationshipBuilder.HasForeignKey(null, configurationSource, runConventions: false)
                                         ?? newRelationshipBuilder;
            }

            return newRelationshipBuilder.RelatedEntityTypes(
                principalEntityTypeBuilder.Metadata,
                dependentEntityTypeBuilder.Metadata,
                principalEndConfigurationSource,
                runConventions: false);
        }

        private InternalRelationshipBuilder GetOrCreateRelationshipBuilder(
            EntityType principalEntityType,
            EntityType dependentEntityType,
            string navigationToPrincipalName,
            string navigationToDependentName,
            IReadOnlyList<Property> dependentProperties,
            IReadOnlyList<Property> principalProperties,
            bool? isRequired,
            ConfigurationSource? principalEndConfigurationSource,
            ConfigurationSource? configurationSource,
            Dictionary<string, Tuple<InternalEntityTypeBuilder, InternalEntityTypeBuilder, string>> removedNavigations,
            List<Tuple<InternalEntityTypeBuilder, ForeignKey>> removedForeignKeys,
            List<InternalRelationshipBuilder> addedForeignKeys,
            out bool existingRelationshipInverted)
        {
            existingRelationshipInverted = false;
            var matchingRelationships = FindRelationships(
                principalEntityType,
                dependentEntityType,
                navigationToPrincipalName,
                navigationToDependentName,
                dependentProperties,
                principalProperties);
            matchingRelationships = matchingRelationships.Distinct().Where(r => r.Metadata != Metadata).ToList();

            var unresolvableRelationships = new List<InternalRelationshipBuilder>();
            var resolvableRelationships = new List<Tuple<InternalRelationshipBuilder, bool, Resolution>>();
            foreach (var matchingRelationship in matchingRelationships)
            {
                var resolvable = true;
                var goodMatch = true;
                var resolution = Resolution.None;
                if (!string.IsNullOrEmpty(navigationToPrincipalName))
                {
                    if ((navigationToPrincipalName == matchingRelationship.Metadata.DependentToPrincipal?.Name)
                        && dependentEntityType.IsSameHierarchy(matchingRelationship.Metadata.DependentToPrincipal.DeclaringEntityType))
                    {
                        if (matchingRelationship.CanSetNavigation(null, true, configurationSource, overrideSameSource: false))
                        {
                            resolution |= Resolution.ResetToPrincipal;
                            goodMatch = false;
                        }
                        else if (matchingRelationship.CanSetNavigation(null, true, configurationSource))
                        {
                            resolution |= Resolution.ResetToPrincipal;
                        }
                        else
                        {
                            resolvable = false;
                        }
                    }
                    else if ((navigationToPrincipalName == matchingRelationship.Metadata.PrincipalToDependent?.Name)
                             && dependentEntityType.IsSameHierarchy(matchingRelationship.Metadata.PrincipalToDependent.DeclaringEntityType))
                    {
                        if (matchingRelationship.CanSetNavigation(null, false, configurationSource, overrideSameSource: false))
                        {
                            resolution |= Resolution.ResetToDependent;
                            goodMatch = false;
                        }
                        else if (matchingRelationship.CanSetNavigation(null, false, configurationSource))
                        {
                            resolution |= Resolution.ResetToDependent;
                        }
                        else
                        {
                            resolvable = false;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(navigationToDependentName))
                {
                    if ((navigationToDependentName == matchingRelationship.Metadata.PrincipalToDependent?.Name)
                        && principalEntityType.IsSameHierarchy(matchingRelationship.Metadata.PrincipalToDependent.DeclaringEntityType))
                    {
                        if (matchingRelationship.CanSetNavigation(null, false, configurationSource, overrideSameSource: false))
                        {
                            resolution |= Resolution.ResetToDependent;
                            goodMatch = false;
                        }
                        else if (matchingRelationship.CanSetNavigation(null, false, configurationSource))
                        {
                            resolution |= Resolution.ResetToDependent;
                        }
                        else
                        {
                            resolvable = false;
                        }
                    }
                    else if ((navigationToDependentName == matchingRelationship.Metadata.DependentToPrincipal?.Name)
                             && principalEntityType.IsSameHierarchy(matchingRelationship.Metadata.DependentToPrincipal.DeclaringEntityType))
                    {
                        if (matchingRelationship.CanSetNavigation(null, true, configurationSource, overrideSameSource: false))
                        {
                            resolution |= Resolution.ResetToPrincipal;
                            goodMatch = false;
                        }
                        else if (matchingRelationship.CanSetNavigation(null, true, configurationSource))
                        {
                            resolution |= Resolution.ResetToPrincipal;
                        }
                        else
                        {
                            resolvable = false;
                        }
                    }
                }

                if ((dependentProperties != null)
                    && matchingRelationship.Metadata.Properties.SequenceEqual(dependentProperties))
                {
                    if (principalProperties == null)
                    {
                        // If principal key wasn't specified on both we treat them as if it was configured to be the PK on the principal type
                        if (matchingRelationship.Metadata.GetPrincipalKeyConfigurationSource().HasValue
                            && matchingRelationship.Metadata.GetPrincipalKeyConfigurationSource().Value.Overrides(configurationSource))
                        {
                            goodMatch = false;
                        }
                        else if (matchingRelationship.CanSetForeignKey(null, configurationSource, overrideSameSource: false))
                        {
                            resolution |= Resolution.ResetDependentProperties;
                            goodMatch = false;
                        }
                        else if (matchingRelationship.CanSetForeignKey(null, configurationSource))
                        {
                            resolution |= Resolution.ResetDependentProperties;
                        }
                    }
                    else
                    {
                        if (matchingRelationship.CanSetForeignKey(null, configurationSource, overrideSameSource: false))
                        {
                            resolution |= Resolution.ResetDependentProperties;
                            goodMatch = false;
                        }
                        else if (matchingRelationship.CanSetForeignKey(null, configurationSource))
                        {
                            resolution |= Resolution.ResetDependentProperties;
                        }
                        else
                        {
                            resolvable = false;
                        }
                    }
                }

                if (resolvable)
                {
                    if (goodMatch
                        && configurationSource.HasValue
                        && matchingRelationship.Metadata.DeclaringEntityType.Builder
                            .CanRemoveForeignKey(matchingRelationship.Metadata, configurationSource.Value))
                    {
                        resolution |= Resolution.Remove;
                    }

                    resolvableRelationships.Add(Tuple.Create(matchingRelationship, goodMatch, resolution));
                }
                else
                {
                    unresolvableRelationships.Add(matchingRelationship);
                }
            }

            var candidates = unresolvableRelationships.Concat(
                resolvableRelationships.Where(r => r.Item2).Concat(
                    resolvableRelationships.Where(r => !r.Item2 && !r.Item1.Metadata.GetConfigurationSource().Overrides(configurationSource)))
                    .Select(r => r.Item1))
                .ToList();
            InternalRelationshipBuilder newRelationshipBuilder = null;
            foreach (var candidateRelationship in candidates)
            {
                bool _;
                if (candidateRelationship.CanSetRelatedTypes(
                    principalEntityType,
                    dependentEntityType,
                    principalEndConfigurationSource,
                    navigationToPrincipalName,
                    navigationToDependentName,
                    configurationSource,
                    false,
                    out existingRelationshipInverted,
                    out _,
                    out _,
                    out _,
                    out _))
                {
                    newRelationshipBuilder = candidateRelationship;
                    break;
                }
            }

            if (unresolvableRelationships.Any(r => r != newRelationshipBuilder))
            {
                return null;
            }

            // This workaround prevents the properties to be cleaned away before the new FK is created,
            // this should be replaced with reference counting
            // Issue #214
            var temporaryProperties = dependentProperties?.Where(p => p.GetConfigurationSource() == ConfigurationSource.Convention
                                                                      && p.IsShadowProperty).ToList() ?? new List<Property>();
            foreach (var temporaryProperty in temporaryProperties)
            {
                temporaryProperty.UpdateConfigurationSource(ConfigurationSource.DataAnnotation);
            }

            if (Metadata.Builder != null)
            {
                RemoveForeignKey(Metadata, removedNavigations, removedForeignKeys);
            }

            foreach (var relationshipWithResolution in resolvableRelationships)
            {
                var resolvableRelationship = relationshipWithResolution.Item1;
                if (resolvableRelationship == newRelationshipBuilder)
                {
                    continue;
                }

                var resolution = relationshipWithResolution.Item3;
                if (resolution.HasFlag(Resolution.Remove))
                {
                    // TODO: Merge non-conflicting aspects from the removed relationships
                    RemoveForeignKey(resolvableRelationship.Metadata, removedNavigations, removedForeignKeys);
                    continue;
                }

                if (resolution.HasFlag(Resolution.ResetToPrincipal))
                {
                    var foreignKey = resolvableRelationship.Metadata;
                    removedNavigations[foreignKey.DeclaringEntityType.Name + "." + foreignKey.DependentToPrincipal.Name] =
                        Tuple.Create(
                            foreignKey.DeclaringEntityType.Builder,
                            foreignKey.PrincipalEntityType.Builder,
                            foreignKey.DependentToPrincipal.Name);
                    resolvableRelationship = resolvableRelationship.Navigation(null, true, foreignKey.GetConfigurationSource(), runConventions: false);
                }

                if (resolution.HasFlag(Resolution.ResetToDependent))
                {
                    var foreignKey = resolvableRelationship.Metadata;
                    removedNavigations[foreignKey.PrincipalEntityType.Name + "." + foreignKey.PrincipalToDependent.Name] =
                        Tuple.Create(
                            foreignKey.PrincipalEntityType.Builder,
                            foreignKey.DeclaringEntityType.Builder,
                            foreignKey.PrincipalToDependent.Name);
                    resolvableRelationship = resolvableRelationship.Navigation(null, false, foreignKey.GetConfigurationSource(), runConventions: false);
                }

                if (resolvableRelationship.Metadata.Builder == null)
                {
                    continue;
                }

                var navigationLessForeignKey = resolvableRelationship.Metadata;
                if (navigationLessForeignKey.DependentToPrincipal == null
                    && navigationLessForeignKey.PrincipalToDependent == null)
                {
                    var extraForeignKeyOwner = navigationLessForeignKey.DeclaringEntityType.Builder;
                    if (extraForeignKeyOwner.RemoveForeignKey(navigationLessForeignKey, ConfigurationSource.Convention, runConventions: false).HasValue)
                    {
                        removedForeignKeys.Add(Tuple.Create(extraForeignKeyOwner, navigationLessForeignKey));
                        continue;
                    }
                }

                if (resolution.HasFlag(Resolution.ResetDependentProperties))
                {
                    var foreignKey = resolvableRelationship.Metadata;
                    removedForeignKeys.Add(Tuple.Create(foreignKey.DeclaringEntityType.Builder, foreignKey));
                    resolvableRelationship = resolvableRelationship.HasForeignKey(null, foreignKey.GetConfigurationSource(), runConventions: false);
                    addedForeignKeys.Add(resolvableRelationship);
                }
            }

            if (configurationSource == ConfigurationSource.Convention)
            {
                foreach (var temporaryProperty in temporaryProperties)
                {
                    temporaryProperty.SetConfigurationSource(ConfigurationSource.Convention);
                }
            }

            if (newRelationshipBuilder == null)
            {
                var principalKey = principalProperties == null
                    ? null
                    : principalEntityType.RootType().Builder.HasKey(principalProperties, configurationSource).Metadata;
                newRelationshipBuilder = dependentEntityType.Builder.CreateForeignKey(
                    principalEntityType.Builder,
                    dependentProperties,
                    principalKey,
                    navigationToPrincipalName,
                    isRequired,
                    ConfigurationSource.Convention,
                    runConventions: false);
                existingRelationshipInverted = false;
            }
            else
            {
                if (newRelationshipBuilder.Metadata.DependentToPrincipal != null)
                {
                    var newForeignKey = newRelationshipBuilder.Metadata;
                    removedNavigations[newForeignKey.DeclaringEntityType.Name + "." + newForeignKey.DependentToPrincipal.Name] =
                        Tuple.Create(
                            newForeignKey.DeclaringEntityType.Builder,
                            newForeignKey.PrincipalEntityType.Builder,
                            newForeignKey.DependentToPrincipal.Name);
                }
                if (newRelationshipBuilder.Metadata.PrincipalToDependent != null)
                {
                    var newForeignKey = newRelationshipBuilder.Metadata;
                    removedNavigations[newForeignKey.PrincipalEntityType.Name + "." + newForeignKey.PrincipalToDependent.Name] =
                        Tuple.Create(
                            newForeignKey.PrincipalEntityType.Builder,
                            newForeignKey.DeclaringEntityType.Builder,
                            newForeignKey.PrincipalToDependent.Name);
                }

                if (navigationToPrincipalName != null
                    && (navigationToPrincipalName == "" ? null : navigationToPrincipalName) != newRelationshipBuilder.Metadata.DependentToPrincipal?.Name
                    && dependentProperties == null
                    && newRelationshipBuilder.Metadata.GetForeignKeyPropertiesConfigurationSource() == null
                    && (!existingRelationshipInverted
                        || (existingRelationshipInverted
                            && newRelationshipBuilder.Metadata.IsSelfReferencing())))
                {
                    // TODO: Also handle the case where existing relationship cannot be inverted,
                    // so the new nav to principal will be specified nav to dependent
                    newRelationshipBuilder = newRelationshipBuilder.ReplaceForeignKey(
                        principalEntityType.Builder,
                        dependentEntityType.Builder,
                        navigationToPrincipalName,
                        null,
                        dependentProperties,
                        principalProperties,
                        null,
                        isRequired,
                        null,
                        false,
                        null,
                        configurationSource,
                        runConventions: false);
                }
            }

            return newRelationshipBuilder;
        }

        private void RemoveForeignKey(
            ForeignKey foreignKey,
            Dictionary<string, Tuple<InternalEntityTypeBuilder, InternalEntityTypeBuilder, string>> removedNavigations,
            List<Tuple<InternalEntityTypeBuilder, ForeignKey>> removedForeignKeys)
        {
            var dependentEntityTypeBuilder = foreignKey.DeclaringEntityType.Builder;
            var principalEntityTypeBuilder = foreignKey.PrincipalEntityType.Builder;
            var navigationToPrincipalName = foreignKey.DependentToPrincipal?.Name;
            if (navigationToPrincipalName != null)
            {
                removedNavigations[foreignKey.DeclaringEntityType.Name + "." + navigationToPrincipalName] =
                    Tuple.Create(dependentEntityTypeBuilder, principalEntityTypeBuilder, navigationToPrincipalName);
            }

            var navigationToDependentName = foreignKey.PrincipalToDependent?.Name;
            if (navigationToDependentName != null)
            {
                removedNavigations[foreignKey.PrincipalEntityType.Name + "." + navigationToDependentName] =
                    Tuple.Create(principalEntityTypeBuilder, dependentEntityTypeBuilder, navigationToDependentName);
            }

            var foreignKeyOwner = foreignKey.DeclaringEntityType.Builder;
            var replacedConfigurationSource = foreignKeyOwner.RemoveForeignKey(foreignKey, ConfigurationSource.Explicit, runConventions: false);
            Debug.Assert(replacedConfigurationSource.HasValue);

            removedForeignKeys.Add(Tuple.Create(foreignKeyOwner, foreignKey));
        }

        private static IReadOnlyList<InternalRelationshipBuilder> FindRelationships(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            string navigationToPrincipalName,
            string navigationToDependentName,
            IReadOnlyList<Property> dependentProperties,
            IReadOnlyList<Property> principalProperties)
        {
            var existingRelationships = new List<InternalRelationshipBuilder>();
            if (!string.IsNullOrEmpty(navigationToPrincipalName))
            {
                existingRelationships.AddRange(dependentEntityType
                    .FindNavigationsInHierarchy(navigationToPrincipalName)
                    .Select(n => n.ForeignKey.Builder));
            }

            if (!string.IsNullOrEmpty(navigationToDependentName))
            {
                existingRelationships.AddRange(principalEntityType
                    .FindNavigationsInHierarchy(navigationToDependentName)
                    .Select(n => n.ForeignKey.Builder));
            }

            if (dependentProperties != null)
            {
                if (principalProperties != null)
                {
                    var principalKey = principalEntityType.FindKey(principalProperties);
                    if (principalKey != null)
                    {
                        existingRelationships.AddRange(dependentEntityType
                            .FindForeignKeysInHierarchy(dependentProperties, principalKey, principalEntityType)
                            .Select(fk => fk.Builder));
                    }
                }
                else
                {
                    existingRelationships.AddRange(dependentEntityType
                        .FindForeignKeysInHierarchy(dependentProperties)
                        .Select(fk => fk.Builder));
                }
            }

            return existingRelationships;
        }

        private InternalRelationshipBuilder FindCurrentRelationshipBuilder(
            EntityType principalEntityType,
            EntityType dependentEntityType,
            string navigationToPrincipalName = null,
            string navigationToDependentName = null,
            IReadOnlyList<Property> dependentProperties = null,
            IReadOnlyList<Property> principalProperties = null)
        {
            InternalRelationshipBuilder currentRelationship = null;
            var matchingRelationships = FindRelationships(
                principalEntityType,
                dependentEntityType,
                navigationToPrincipalName,
                navigationToDependentName,
                dependentProperties,
                principalProperties).Distinct();

            foreach (var matchingRelationship in matchingRelationships)
            {
                var matchingForeignKey = matchingRelationship.Metadata;
                if (navigationToPrincipalName != null
                    && (matchingForeignKey.DependentToPrincipal?.Name ?? "") != navigationToPrincipalName)
                {
                    continue;
                }

                if (navigationToDependentName != null
                    && (matchingForeignKey.PrincipalToDependent?.Name ?? "") != navigationToDependentName)
                {
                    continue;
                }

                if (dependentProperties != null
                    && !matchingForeignKey.Properties.SequenceEqual(dependentProperties))
                {
                    continue;
                }

                if (principalProperties != null
                    && !matchingForeignKey.PrincipalKey.Properties.SequenceEqual(principalProperties))
                {
                    continue;
                }

                if (currentRelationship != null)
                {
                    return null;
                }

                currentRelationship = matchingRelationship;
            }

            return currentRelationship;
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
            ConfigurationSource? configurationSource)
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

        private bool CanSetRelatedTypes(
            EntityType principalEntityType,
            EntityType dependentEntityType,
            ConfigurationSource? principalEndConfigurationSource,
            string navigationToPrincipalName,
            string navigationToDependentName,
            ConfigurationSource? configurationSource,
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

            var strictPrincipal = principalEndConfigurationSource.HasValue
                                  && principalEndConfigurationSource.Value.Overrides(Metadata.GetPrincipalEndConfigurationSource());
            var canInvert = CanInvert(configurationSource);
            bool invertedShouldResetToPrincipal;
            bool invertedShouldResetToDependent;
            bool _;
            if ((!strictPrincipal
                 || canInvert)
                && CanSetRelatedTypes(
                    dependentEntityType,
                    principalEntityType,
                    navigationToDependentName,
                    navigationToPrincipalName,
                    configurationSource,
                    strictPrincipal,
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
            ConfigurationSource? configurationSource,
            bool inverted,
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

            bool? _;
            bool __;
            if (navigationToPrincipalName != null)
            {
                if (!configurationSource.HasValue
                    || !CanSetNavigation(
                        navigationToPrincipalName == "" ? null : navigationToPrincipalName,
                        true,
                        configurationSource.Value,
                        shouldThrow,
                        true,
                        out _,
                        out __))
                {
                    return false;
                }

                shouldResetToPrincipal = true;
            }
            else
            {
                navigationToPrincipalName = Metadata.DependentToPrincipal?.Name;
                if ((navigationToPrincipalName != null)
                    && !Internal.Navigation.IsCompatible(
                        navigationToPrincipalName,
                        !inverted,
                        inverted ? principalEntityType : dependentEntityType,
                        inverted ? dependentEntityType : principalEntityType,
                        shouldThrow,
                        out _))
                {
                    if (!configurationSource.HasValue
                        || !CanSetNavigation(null, true, configurationSource.Value))
                    {
                        return false;
                    }

                    shouldResetToPrincipal = true;
                }
            }

            if (navigationToDependentName != null)
            {
                if (!configurationSource.HasValue
                    || !CanSetNavigation(
                        navigationToDependentName == "" ? null : navigationToDependentName,
                        false,
                        configurationSource.Value,
                        shouldThrow,
                        true,
                        out _,
                        out __))
                {
                    return false;
                }
            }
            else
            {
                navigationToDependentName = Metadata.PrincipalToDependent?.Name;
                if ((navigationToDependentName != null)
                    && !Internal.Navigation.IsCompatible(
                        navigationToDependentName,
                        inverted,
                        inverted ? principalEntityType : dependentEntityType,
                        inverted ? dependentEntityType : principalEntityType,
                        shouldThrow,
                        out _))
                {
                    if (!configurationSource.HasValue
                        || !CanSetNavigation(null, false, configurationSource.Value))
                    {
                        return false;
                    }

                    shouldResetToDependent = true;
                }
            }

            if (!Property.AreCompatible(Metadata.PrincipalKey.Properties, principalEntityType))
            {
                if (!configurationSource.HasValue
                    || !configurationSource.Value.Overrides(Metadata.GetPrincipalKeyConfigurationSource()))
                {
                    return false;
                }

                shouldResetPrincipalProperties = true;
            }

            if (!Property.AreCompatible(Metadata.Properties, dependentEntityType))
            {
                if (!configurationSource.HasValue
                    || !configurationSource.Value.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource()))
                {
                    return false;
                }

                shouldResetDependentProperties = true;
            }

            return true;
        }

        [Flags]
        private enum Resolution
        {
            None = 0,
            Remove = 1 << 0,
            ResetToPrincipal = 1 << 1,
            ResetToDependent = 1 << 2,
            ResetDependentProperties = 1 << 3
        }
    }
}
