// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class InternalRelationshipBuilder : InternalModelItemBuilder<ForeignKey>, IConventionRelationshipBuilder
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalRelationshipBuilder(
            [NotNull] ForeignKey foreignKey,
            [NotNull] InternalModelBuilder modelBuilder)
            : base(foreignKey, modelBuilder)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasNavigation(
            [CanBeNull] string name,
            bool pointsToPrincipal,
            ConfigurationSource configurationSource)
            => pointsToPrincipal
                ? HasNavigations(
                    MemberIdentity.Create(name),
                    navigationToDependent: null,
                    configurationSource)
                : HasNavigations(
                    navigationToPrincipal: null,
                    MemberIdentity.Create(name),
                    configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasNavigation(
            [CanBeNull] MemberInfo property,
            bool pointsToPrincipal,
            ConfigurationSource configurationSource)
            => pointsToPrincipal
                ? HasNavigations(
                    MemberIdentity.Create(property),
                    navigationToDependent: null,
                    configurationSource)
                : HasNavigations(
                    navigationToPrincipal: null,
                    MemberIdentity.Create(property),
                    configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasNavigations(
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            ConfigurationSource configurationSource)
            => HasNavigations(
                MemberIdentity.Create(navigationToPrincipalName),
                MemberIdentity.Create(navigationToDependentName),
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasNavigations(
            [CanBeNull] MemberInfo navigationToPrincipal,
            [CanBeNull] MemberInfo navigationToDependent,
            ConfigurationSource configurationSource)
            => HasNavigations(
                MemberIdentity.Create(navigationToPrincipal),
                MemberIdentity.Create(navigationToDependent),
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasNavigations(
            MemberIdentity? navigationToPrincipal,
            MemberIdentity? navigationToDependent,
            ConfigurationSource configurationSource)
            => HasNavigations(
                navigationToPrincipal,
                navigationToDependent,
                Metadata.PrincipalEntityType,
                Metadata.DeclaringEntityType,
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasNavigations(
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            ConfigurationSource configurationSource)
            => HasNavigations(
                MemberIdentity.Create(navigationToPrincipalName),
                MemberIdentity.Create(navigationToDependentName),
                principalEntityType,
                dependentEntityType,
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasNavigations(
            [CanBeNull] MemberInfo navigationToPrincipal,
            [CanBeNull] MemberInfo navigationToDependent,
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            ConfigurationSource configurationSource)
            => HasNavigations(
                MemberIdentity.Create(navigationToPrincipal),
                MemberIdentity.Create(navigationToDependent),
                principalEntityType,
                dependentEntityType,
                configurationSource);

        private InternalRelationshipBuilder HasNavigations(
            MemberIdentity? navigationToPrincipal,
            MemberIdentity? navigationToDependent,
            EntityType principalEntityType,
            EntityType dependentEntityType,
            ConfigurationSource configurationSource)
        {
            var navigationToPrincipalName = navigationToPrincipal?.Name;
            var navigationToDependentName = navigationToDependent?.Name;
            if ((navigationToPrincipal == null
                    || navigationToPrincipal.Value.Name == Metadata.DependentToPrincipal?.Name)
                && (navigationToDependent == null
                    || navigationToDependent.Value.Name == Metadata.PrincipalToDependent?.Name))
            {
                Metadata.UpdateConfigurationSource(configurationSource);
                if (navigationToPrincipal != null)
                {
                    Metadata.UpdateDependentToPrincipalConfigurationSource(configurationSource);
                    if (navigationToPrincipalName != null)
                    {
                        principalEntityType.RemoveIgnored(navigationToPrincipalName);
                    }
                }

                if (navigationToDependent != null)
                {
                    Metadata.UpdatePrincipalToDependentConfigurationSource(configurationSource);
                    if (navigationToDependentName != null)
                    {
                        principalEntityType.RemoveIgnored(navigationToDependentName);
                    }
                }

                return this;
            }

            var shouldThrow = configurationSource == ConfigurationSource.Explicit;

            if (navigationToPrincipalName != null
                && navigationToPrincipal.Value.MemberInfo == null
                && dependentEntityType.HasClrType())
            {
                var navigationProperty = FindCompatibleClrMember(
                    navigationToPrincipalName, dependentEntityType, principalEntityType, shouldThrow);
                if (navigationProperty != null)
                {
                    navigationToPrincipal = MemberIdentity.Create(navigationProperty);
                }
            }

            if (navigationToDependentName != null
                && navigationToDependent.Value.MemberInfo == null
                && principalEntityType.HasClrType())
            {
                var navigationProperty = FindCompatibleClrMember(
                    navigationToDependentName, principalEntityType, dependentEntityType, shouldThrow);
                if (navigationProperty != null)
                {
                    navigationToDependent = MemberIdentity.Create(navigationProperty);
                }
            }

            if (!CanSetNavigations(
                navigationToPrincipal,
                navigationToDependent,
                principalEntityType,
                dependentEntityType,
                configurationSource,
                shouldThrow,
                out var shouldInvert,
                out var shouldBeUnique,
                out var removeOppositeNavigation,
                out var conflictingNavigationsFound,
                out var changeRelatedTypes))
            {
                return null;
            }

            if (removeOppositeNavigation)
            {
                if (navigationToPrincipal == null)
                {
                    navigationToPrincipal = MemberIdentity.None;
                }

                if (navigationToDependent == null)
                {
                    navigationToDependent = MemberIdentity.None;
                }
            }

            IReadOnlyList<Property> dependentProperties = null;
            IReadOnlyList<Property> principalProperties = null;
            if (shouldInvert == true)
            {
                Check.DebugAssert(
                    configurationSource.Overrides(Metadata.GetPropertiesConfigurationSource()),
                    "configurationSource does not override Metadata.GetPropertiesConfigurationSource");

                Check.DebugAssert(
                    configurationSource.Overrides(Metadata.GetPrincipalKeyConfigurationSource()),
                    "configurationSource does not override Metadata.GetPrincipalKeyConfigurationSource");

                var entityType = principalEntityType;
                principalEntityType = dependentEntityType;
                dependentEntityType = entityType;

                var navigation = navigationToPrincipal;
                navigationToPrincipal = navigationToDependent;
                navigationToDependent = navigation;

                navigationToPrincipalName = navigationToPrincipal?.Name;
                navigationToDependentName = navigationToDependent?.Name;

                if (Metadata.GetPropertiesConfigurationSource() == configurationSource)
                {
                    dependentProperties = Array.Empty<Property>();
                }

                if (Metadata.GetPrincipalKeyConfigurationSource() == configurationSource)
                {
                    principalProperties = Array.Empty<Property>();
                }
            }

            if (navigationToPrincipalName != null)
            {
                foreach (var conflictingServiceProperty in dependentEntityType.FindServicePropertiesInHierarchy(navigationToPrincipalName))
                {
                    if (conflictingServiceProperty.GetConfigurationSource() != ConfigurationSource.Explicit)
                    {
                        conflictingServiceProperty.DeclaringEntityType.RemoveServiceProperty(conflictingServiceProperty);
                    }
                }

                foreach (var conflictingProperty in dependentEntityType.FindPropertiesInHierarchy(navigationToPrincipalName))
                {
                    if (conflictingProperty.GetConfigurationSource() != ConfigurationSource.Explicit)
                    {
                        conflictingProperty.DeclaringEntityType.RemoveProperty(conflictingProperty.Name);
                    }
                }

                foreach (var conflictingSkipNavigation in dependentEntityType.FindSkipNavigationsInHierarchy(navigationToPrincipalName))
                {
                    if (conflictingSkipNavigation.GetConfigurationSource() == ConfigurationSource.Explicit)
                    {
                        continue;
                    }

                    var inverse = conflictingSkipNavigation.Inverse;
                    if (inverse?.Builder != null
                        && inverse.DeclaringEntityType.Builder
                            .CanRemoveSkipNavigation(inverse, configurationSource))
                    {
                        inverse.DeclaringEntityType.RemoveSkipNavigation(inverse);
                    }

                    conflictingSkipNavigation.DeclaringEntityType.RemoveSkipNavigation(conflictingSkipNavigation);
                }
            }

            if (navigationToDependentName != null)
            {
                foreach (var conflictingServiceProperty in principalEntityType.FindServicePropertiesInHierarchy(navigationToDependentName))
                {
                    if (conflictingServiceProperty.GetConfigurationSource() != ConfigurationSource.Explicit)
                    {
                        conflictingServiceProperty.DeclaringEntityType.RemoveServiceProperty(conflictingServiceProperty);
                    }
                }

                foreach (var conflictingProperty in principalEntityType.FindPropertiesInHierarchy(navigationToDependentName))
                {
                    if (conflictingProperty.GetConfigurationSource() != ConfigurationSource.Explicit)
                    {
                        conflictingProperty.DeclaringEntityType.RemoveProperty(conflictingProperty.Name);
                    }
                }

                foreach (var conflictingSkipNavigation in principalEntityType.FindSkipNavigationsInHierarchy(navigationToDependentName))
                {
                    if (conflictingSkipNavigation.GetConfigurationSource() == ConfigurationSource.Explicit)
                    {
                        continue;
                    }

                    var inverse = conflictingSkipNavigation.Inverse;
                    if (inverse?.Builder != null
                        && inverse.DeclaringEntityType.Builder
                            .CanRemoveSkipNavigation(inverse, configurationSource))
                    {
                        inverse.DeclaringEntityType.RemoveSkipNavigation(inverse);
                    }

                    conflictingSkipNavigation.DeclaringEntityType.RemoveSkipNavigation(conflictingSkipNavigation);
                }
            }

            InternalRelationshipBuilder builder;
            if (shouldInvert == true
                || conflictingNavigationsFound
                || changeRelatedTypes)
            {
                builder = ReplaceForeignKey(
                    configurationSource,
                    principalEntityType.Builder,
                    dependentEntityType.Builder,
                    navigationToPrincipal,
                    navigationToDependent,
                    dependentProperties,
                    principalProperties: principalProperties,
                    isUnique: shouldBeUnique,
                    removeCurrent: shouldInvert == true || changeRelatedTypes,
                    principalEndConfigurationSource: shouldInvert != null ? configurationSource : (ConfigurationSource?)null,
                    oldRelationshipInverted: shouldInvert == true);

                if (builder == null)
                {
                    return null;
                }

                Check.DebugAssert(builder.Metadata.Builder != null, "builder.Metadata.Builder is null");
            }
            else
            {
                using var batch = Metadata.DeclaringEntityType.Model.ConventionDispatcher.DelayConventions();
                builder = this;
                Metadata.UpdateConfigurationSource(configurationSource);

                if (shouldBeUnique.HasValue)
                {
                    IsUnique(shouldBeUnique.Value, configurationSource);
                }
                else
                {
                    IsUnique(null, ConfigurationSource.Convention);
                }

                if (navigationToPrincipal != null)
                {
                    if (navigationToDependent != null)
                    {
                        Metadata.HasPrincipalToDependent((string)null, configurationSource);
                    }

                    var navigationProperty = navigationToPrincipal.Value.MemberInfo;
                    if (navigationToPrincipalName != null)
                    {
                        Metadata.DeclaringEntityType.RemoveIgnored(navigationToPrincipalName);

                        if (Metadata.DeclaringEntityType.ClrType != null
                            && navigationProperty == null)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.NoClrNavigation(navigationToPrincipalName, Metadata.DeclaringEntityType.DisplayName()));
                        }
                    }

                    if (navigationProperty != null)
                    {
                        Metadata.HasDependentToPrincipal(navigationProperty, configurationSource);
                    }
                    else
                    {
                        Metadata.HasDependentToPrincipal(navigationToPrincipalName, configurationSource);
                    }
                }

                if (navigationToDependent != null)
                {
                    var navigationProperty = navigationToDependent.Value.MemberInfo;
                    if (navigationToDependentName != null)
                    {
                        Metadata.PrincipalEntityType.RemoveIgnored(navigationToDependentName);

                        if (Metadata.DeclaringEntityType.ClrType != null
                            && navigationProperty == null)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.NoClrNavigation(navigationToDependentName, Metadata.PrincipalEntityType.DisplayName()));
                        }
                    }

                    if (navigationProperty != null)
                    {
                        Metadata.HasPrincipalToDependent(navigationProperty, configurationSource);
                    }
                    else
                    {
                        Metadata.HasPrincipalToDependent(navigationToDependentName, configurationSource);
                    }
                }

                builder = batch.Run(builder);
            }

            return builder != null
                && ((navigationToPrincipal != null
                        && builder.Metadata.DependentToPrincipal?.Name != navigationToPrincipal.Value.Name)
                    || (navigationToDependent != null
                        && builder.Metadata.PrincipalToDependent?.Name != navigationToDependent.Value.Name))
                && ((navigationToDependent != null
                        && builder.Metadata.DependentToPrincipal?.Name != navigationToDependent.Value.Name)
                    || (navigationToPrincipal != null
                        && builder.Metadata.PrincipalToDependent?.Name != navigationToPrincipal.Value.Name))
                    ? null
                    : builder;
        }

        private static MemberInfo FindCompatibleClrMember(
            string navigationName,
            EntityType sourceType,
            EntityType targetType,
            bool shouldThrow)
        {
            var navigationProperty = sourceType.FindClrMember(navigationName);
            return !Navigation.IsCompatible(navigationName, navigationProperty, sourceType, targetType, null, shouldThrow)
                ? null
                : navigationProperty;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetNavigation(
            [CanBeNull] MemberInfo property,
            bool pointsToPrincipal,
            ConfigurationSource? configurationSource)
            => CanSetNavigation(
                MemberIdentity.Create(property),
                pointsToPrincipal,
                configurationSource,
                shouldThrow: false,
                out _,
                out _,
                out _);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetNavigation(
            [CanBeNull] string name,
            bool pointsToPrincipal,
            ConfigurationSource? configurationSource)
            => CanSetNavigation(
                MemberIdentity.Create(name),
                pointsToPrincipal,
                configurationSource,
                shouldThrow: false,
                out _,
                out _,
                out _);

        private bool CanSetNavigation(
            MemberIdentity navigation,
            bool pointsToPrincipal,
            ConfigurationSource? configurationSource,
            bool shouldThrow,
            out bool? shouldBeUnique,
            out bool removeOppositeNavigation,
            out bool removeConflictingNavigations)
            => pointsToPrincipal
                ? CanSetNavigations(
                    navigation,
                    navigationToDependent: null,
                    configurationSource,
                    shouldThrow,
                    out _,
                    out shouldBeUnique,
                    out removeOppositeNavigation,
                    out removeConflictingNavigations)
                : CanSetNavigations(
                    navigationToPrincipal: null,
                    navigation,
                    configurationSource,
                    shouldThrow,
                    out _,
                    out shouldBeUnique,
                    out removeOppositeNavigation,
                    out removeConflictingNavigations);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetNavigations(
            [CanBeNull] MemberInfo navigationToPrincipal,
            [CanBeNull] MemberInfo navigationToDependent,
            ConfigurationSource? configurationSource)
            => CanSetNavigations(
                MemberIdentity.Create(navigationToPrincipal),
                MemberIdentity.Create(navigationToDependent),
                configurationSource,
                shouldThrow: false,
                out _,
                out _,
                out _,
                out _);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetNavigations(
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            ConfigurationSource? configurationSource)
            => CanSetNavigations(
                MemberIdentity.Create(navigationToPrincipalName),
                MemberIdentity.Create(navigationToDependentName),
                configurationSource,
                shouldThrow: false,
                out _,
                out _,
                out _,
                out _);

        private bool CanSetNavigations(
            MemberIdentity? navigationToPrincipal,
            MemberIdentity? navigationToDependent,
            ConfigurationSource? configurationSource)
            => CanSetNavigations(
                navigationToPrincipal,
                navigationToDependent,
                configurationSource,
                shouldThrow: false,
                out _,
                out _,
                out _,
                out _);

        private bool CanSetNavigations(
            MemberIdentity? navigationToPrincipal,
            MemberIdentity? navigationToDependent,
            ConfigurationSource? configurationSource,
            bool shouldThrow,
            out bool? shouldInvert,
            out bool? shouldBeUnique,
            out bool removeOppositeNavigation,
            out bool removeConflictingNavigations)
            => CanSetNavigations(
                navigationToPrincipal,
                navigationToDependent,
                Metadata.PrincipalEntityType,
                Metadata.DeclaringEntityType,
                configurationSource,
                shouldThrow,
                out shouldInvert,
                out shouldBeUnique,
                out removeOppositeNavigation,
                out removeConflictingNavigations,
                out _);

        private bool CanSetNavigations(
            MemberIdentity? navigationToPrincipal,
            MemberIdentity? navigationToDependent,
            EntityType principalEntityType,
            EntityType dependentEntityType,
            ConfigurationSource? configurationSource,
            bool shouldThrow,
            out bool? shouldInvert,
            out bool? shouldBeUnique,
            out bool removeOppositeNavigation,
            out bool conflictingNavigationsFound,
            out bool changeRelatedTypes)
        {
            shouldInvert = null;
            shouldBeUnique = null;
            removeOppositeNavigation = false;
            conflictingNavigationsFound = false;
            changeRelatedTypes = false;

            if ((navigationToPrincipal == null
                    || navigationToPrincipal.Value.Name == Metadata.DependentToPrincipal?.Name)
                && (navigationToDependent == null
                    || navigationToDependent.Value.Name == Metadata.PrincipalToDependent?.Name))
            {
                return true;
            }

            if (!configurationSource.HasValue)
            {
                return false;
            }

            var navigationToPrincipalName = navigationToPrincipal?.Name;
            if (navigationToPrincipal != null
                && navigationToPrincipalName != Metadata.DependentToPrincipal?.Name)
            {
                if (!configurationSource.Overrides(Metadata.GetDependentToPrincipalConfigurationSource()))
                {
                    return false;
                }

                if (navigationToPrincipalName != null)
                {
                    if (navigationToDependent == null
                        && navigationToPrincipalName == Metadata.PrincipalToDependent?.Name
                        && Metadata.IsIntraHierarchical())
                    {
                        if (!configurationSource.Value.Overrides(Metadata.GetPrincipalToDependentConfigurationSource()))
                        {
                            return false;
                        }

                        removeOppositeNavigation = true;
                    }
                    else if (!dependentEntityType.Builder.CanHaveNavigation(navigationToPrincipalName, configurationSource))
                    {
                        return false;
                    }
                }
            }

            var navigationToDependentName = navigationToDependent?.Name;
            if (navigationToDependent != null
                && navigationToDependentName != Metadata.PrincipalToDependent?.Name)
            {
                if (!configurationSource.Overrides(Metadata.GetPrincipalToDependentConfigurationSource()))
                {
                    return false;
                }

                if (navigationToDependentName != null)
                {
                    if (navigationToPrincipal == null
                        && navigationToDependentName == Metadata.DependentToPrincipal?.Name
                        && Metadata.IsIntraHierarchical())
                    {
                        if (!configurationSource.Value.Overrides(Metadata.GetDependentToPrincipalConfigurationSource()))
                        {
                            return false;
                        }

                        removeOppositeNavigation = true;
                    }
                    else if (!principalEntityType.Builder.CanHaveNavigation(navigationToDependentName, configurationSource))
                    {
                        return false;
                    }
                }
            }

            var navigationToPrincipalProperty = navigationToPrincipal?.MemberInfo;
            var navigationToDependentProperty = navigationToDependent?.MemberInfo;

            // ReSharper disable once InlineOutVariableDeclaration
            bool? invertedShouldBeUnique = null;
            if (navigationToPrincipalProperty != null
                && !IsCompatible(
                    navigationToPrincipalProperty,
                    pointsToPrincipal: false,
                    principalEntityType.ClrType,
                    dependentEntityType.ClrType,
                    shouldThrow: false,
                    out invertedShouldBeUnique))
            {
                shouldInvert = false;
            }

            if (navigationToDependentProperty != null
                && !IsCompatible(
                    navigationToDependentProperty,
                    pointsToPrincipal: true,
                    principalEntityType.ClrType,
                    dependentEntityType.ClrType,
                    shouldThrow: false,
                    out _))
            {
                shouldInvert = false;
            }

            if (navigationToPrincipalProperty != null
                && !IsCompatible(
                    navigationToPrincipalProperty,
                    pointsToPrincipal: true,
                    dependentEntityType.ClrType,
                    principalEntityType.ClrType,
                    shouldThrow && shouldInvert != null,
                    out _))
            {
                if (shouldInvert != null)
                {
                    return false;
                }

                shouldInvert = true;
            }

            if (navigationToDependentProperty != null
                && !IsCompatible(
                    navigationToDependentProperty,
                    pointsToPrincipal: false,
                    dependentEntityType.ClrType,
                    principalEntityType.ClrType,
                    shouldThrow && shouldInvert != null,
                    out shouldBeUnique))
            {
                if (shouldInvert != null)
                {
                    return false;
                }

                shouldInvert = true;
            }

            if (shouldInvert == true)
            {
                shouldBeUnique = invertedShouldBeUnique;
            }

            if (shouldBeUnique.HasValue
                && Metadata.IsUnique != shouldBeUnique
                && !configurationSource.Overrides(Metadata.GetIsUniqueConfigurationSource()))
            {
                return false;
            }

            var compatibleRelationship = FindCompatibleRelationship(
                principalEntityType,
                dependentEntityType,
                navigationToPrincipal,
                navigationToDependent,
                null,
                null,
                Metadata.GetPrincipalEndConfigurationSource(),
                configurationSource,
                out var _,
                out var conflictingRelationshipsFound,
                out var resolvableRelationships);

            if (conflictingRelationshipsFound)
            {
                return false;
            }

            conflictingNavigationsFound = compatibleRelationship != null
                || resolvableRelationships.Any(r =>
                    (r.Resolution & (Resolution.ResetToDependent | Resolution.ResetToPrincipal)) != 0);

            if (shouldBeUnique == null
                && (Metadata.IsUnique || configurationSource.OverridesStrictly(Metadata.GetIsUniqueConfigurationSource()))
                && ((navigationToDependentProperty != null && shouldInvert != true)
                    || (navigationToPrincipalProperty != null && shouldInvert == true)))
            {
                // if new dependent can be both assume single
                shouldBeUnique = true;
            }

            if (shouldInvert == false
                && !conflictingNavigationsFound
                && (principalEntityType != Metadata.PrincipalEntityType
                    || dependentEntityType != Metadata.DeclaringEntityType))
            {
                if (navigationToPrincipalProperty != null
                    && !IsCompatible(
                        navigationToPrincipalProperty,
                        pointsToPrincipal: true,
                        Metadata.DeclaringEntityType.ClrType,
                        Metadata.PrincipalEntityType.ClrType,
                        shouldThrow: false,
                        out _))
                {
                    changeRelatedTypes = true;
                    return true;
                }

                if (navigationToDependentProperty != null
                    && !IsCompatible(
                        navigationToDependentProperty,
                        pointsToPrincipal: false,
                        Metadata.DeclaringEntityType.ClrType,
                        Metadata.PrincipalEntityType.ClrType,
                        shouldThrow: false,
                        out _))
                {
                    changeRelatedTypes = true;
                    return true;
                }
            }

            return true;
        }

        private bool CanRemoveNavigation(bool pointsToPrincipal, ConfigurationSource? configurationSource, bool overrideSameSource = true)
            => pointsToPrincipal
                ? Metadata.DependentToPrincipal == null
                    || (configurationSource.Overrides(Metadata.GetDependentToPrincipalConfigurationSource())
                        && (overrideSameSource || configurationSource != Metadata.GetDependentToPrincipalConfigurationSource()))
                : Metadata.PrincipalToDependent == null
                    || (configurationSource.Overrides(Metadata.GetPrincipalToDependentConfigurationSource())
                        && (overrideSameSource || configurationSource != Metadata.GetPrincipalToDependentConfigurationSource()));

        private static bool IsCompatible(
            [NotNull] MemberInfo navigationProperty,
            bool pointsToPrincipal,
            [NotNull] Type dependentType,
            [NotNull] Type principalType,
            bool shouldThrow,
            out bool? shouldBeUnique)
        {
            shouldBeUnique = null;
            if (!pointsToPrincipal)
            {
                var canBeUnique = Navigation.IsCompatible(
                    navigationProperty,
                    principalType,
                    dependentType,
                    shouldBeCollection: false,
                    shouldThrow: false);
                var canBeNonUnique = Navigation.IsCompatible(
                    navigationProperty,
                    principalType,
                    dependentType,
                    shouldBeCollection: true,
                    shouldThrow: false);

                if (canBeUnique != canBeNonUnique)
                {
                    shouldBeUnique = canBeUnique;
                }
                else if (!canBeUnique)
                {
                    if (shouldThrow)
                    {
                        Navigation.IsCompatible(
                            navigationProperty,
                            principalType,
                            dependentType,
                            shouldBeCollection: null,
                            shouldThrow: true);
                    }

                    return false;
                }
            }
            else if (!Navigation.IsCompatible(
                navigationProperty,
                dependentType,
                principalType,
                shouldBeCollection: false,
                shouldThrow))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasField(
            [CanBeNull] string fieldName, bool pointsToPrincipal, ConfigurationSource configurationSource)
        {
            var navigation = pointsToPrincipal ? Metadata.DependentToPrincipal : Metadata.PrincipalToDependent;
            if (navigation == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.NoNavigation(
                        pointsToPrincipal ? Metadata.DeclaringEntityType.DisplayName() : Metadata.PrincipalEntityType.DisplayName(),
                        Metadata.Properties.Format()));
            }

            if (navigation.FieldInfo?.GetSimpleMemberName() == fieldName
                || configurationSource.Overrides(navigation.GetFieldInfoConfigurationSource()))
            {
                navigation.SetField(fieldName, configurationSource);

                return this;
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetField([CanBeNull] string fieldName, bool pointsToPrincipal, ConfigurationSource? configurationSource)
        {
            var navigation = pointsToPrincipal ? Metadata.DependentToPrincipal : Metadata.PrincipalToDependent;
            if (navigation == null)
            {
                return false;
            }

            if (configurationSource.Overrides(navigation.GetFieldInfoConfigurationSource()))
            {
                if (fieldName == null)
                {
                    return true;
                }

                var fieldInfo = PropertyBase.GetFieldInfo(
                    fieldName, navigation.DeclaringEntityType, navigation.Name,
                    shouldThrow: false);
                return fieldInfo != null
                    && PropertyBase.IsCompatible(
                        fieldInfo, navigation.ClrType, navigation.DeclaringType.ClrType, navigation.Name,
                        shouldThrow: false);
            }

            return navigation.FieldInfo?.GetSimpleMemberName() == fieldName;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasField(
            [CanBeNull] FieldInfo fieldInfo, bool pointsToPrincipal, ConfigurationSource configurationSource)
        {
            var navigation = pointsToPrincipal ? Metadata.DependentToPrincipal : Metadata.PrincipalToDependent;
            if (navigation == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.NoNavigation(
                        pointsToPrincipal ? Metadata.DeclaringEntityType.DisplayName() : Metadata.PrincipalEntityType.DisplayName(),
                        Metadata.Properties.Format()));
            }

            if (configurationSource.Overrides(navigation.GetFieldInfoConfigurationSource())
                || Equals(navigation.FieldInfo, fieldInfo))
            {
                navigation.SetField(fieldInfo, configurationSource);
                return this;
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetField([CanBeNull] FieldInfo fieldInfo, bool pointsToPrincipal, ConfigurationSource? configurationSource)
        {
            var navigation = pointsToPrincipal ? Metadata.DependentToPrincipal : Metadata.PrincipalToDependent;
            return navigation != null
                && ((configurationSource.Overrides(navigation.GetFieldInfoConfigurationSource())
                        && (fieldInfo == null
                            || PropertyBase.IsCompatible(
                                fieldInfo, navigation.ClrType, Metadata.DeclaringEntityType.ClrType, navigation.Name,
                                shouldThrow: false)))
                    || Equals(navigation.FieldInfo, fieldInfo));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode, bool pointsToPrincipal, ConfigurationSource configurationSource)
        {
            var navigation = pointsToPrincipal ? Metadata.DependentToPrincipal : Metadata.PrincipalToDependent;
            if (navigation == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.NoNavigation(
                        pointsToPrincipal ? Metadata.DeclaringEntityType.DisplayName() : Metadata.PrincipalEntityType.DisplayName(),
                        Metadata.Properties.Format()));
            }

            if (CanSetPropertyAccessMode(propertyAccessMode, pointsToPrincipal, configurationSource))
            {
                navigation.SetPropertyAccessMode(propertyAccessMode, configurationSource);

                return this;
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetPropertyAccessMode(
            PropertyAccessMode? propertyAccessMode, bool pointsToPrincipal, ConfigurationSource? configurationSource)
        {
            IConventionNavigation navigation = pointsToPrincipal ? Metadata.DependentToPrincipal : Metadata.PrincipalToDependent;
            return navigation != null
                && (configurationSource.Overrides(navigation.GetPropertyAccessModeConfigurationSource())
                    || navigation.GetPropertyAccessMode() == propertyAccessMode);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder IsEagerLoaded(
            bool? eagerLoaded,
            bool pointsToPrincipal,
            ConfigurationSource configurationSource)
        {
            var navigation = pointsToPrincipal ? Metadata.DependentToPrincipal : Metadata.PrincipalToDependent;
            if (navigation == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.NoNavigation(
                        pointsToPrincipal ? Metadata.DeclaringEntityType.DisplayName() : Metadata.PrincipalEntityType.DisplayName(),
                        Metadata.Properties.Format()));
            }

            if (CanSetIsEagerLoaded(eagerLoaded, pointsToPrincipal, configurationSource))
            {
                navigation.SetIsEagerLoaded(eagerLoaded, configurationSource);

                return this;
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetIsEagerLoaded(
            bool? eagerLoaded,
            bool pointsToPrincipal,
            ConfigurationSource? configurationSource)
        {
            IConventionNavigation navigation = pointsToPrincipal ? Metadata.DependentToPrincipal : Metadata.PrincipalToDependent;
            return navigation != null
                && (configurationSource.Overrides(navigation.GetIsEagerLoadedConfigurationSource())
                    || navigation.IsEagerLoaded == eagerLoaded);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder IsRequired(bool? required, ConfigurationSource configurationSource)
            => CanSetIsRequired(required, configurationSource)
                ? Metadata.SetIsRequired(required, configurationSource)?.Builder
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetIsRequired(bool? required, ConfigurationSource? configurationSource)
            => Metadata.IsRequired == required
                || configurationSource.Overrides(Metadata.GetIsRequiredConfigurationSource());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder IsOwnership(bool? ownership, ConfigurationSource configurationSource)
        {
            if (Metadata.IsOwnership == ownership)
            {
                return Metadata.SetIsOwnership(ownership, configurationSource).Builder;
            }

            if (ownership == null
                || !configurationSource.Overrides(Metadata.GetIsOwnershipConfigurationSource()))
            {
                return null;
            }

            var declaringType = Metadata.DeclaringEntityType;
            if (ownership.Value)
            {
                InternalRelationshipBuilder newRelationshipBuilder;
                var otherOwnership = declaringType.GetDeclaredForeignKeys().SingleOrDefault(fk => fk.IsOwnership);
                var invertedOwnerships = declaringType.GetDeclaredReferencingForeignKeys()
                    .Where(fk => fk.IsOwnership && fk.DeclaringEntityType.ClrType == Metadata.PrincipalEntityType.ClrType).ToList();

                if (invertedOwnerships.Any(fk => !configurationSource.Overrides(fk.GetConfigurationSource())))
                {
                    return null;
                }

                if (declaringType.HasDefiningNavigation())
                {
                    Check.DebugAssert(
                        Metadata.PrincipalToDependent == null
                        || declaringType.DefiningNavigationName == Metadata.PrincipalToDependent.Name,
                        $"Unexpected navigation");

                    if (otherOwnership != null
                        && !configurationSource.Overrides(otherOwnership.GetConfigurationSource()))
                    {
                        return null;
                    }

                    newRelationshipBuilder = Metadata.SetIsOwnership(ownership: true, configurationSource)?.Builder;
                    newRelationshipBuilder = newRelationshipBuilder?.OnDelete(DeleteBehavior.Cascade, ConfigurationSource.Convention);

                    if (newRelationshipBuilder == null)
                    {
                        return null;
                    }

                    if (otherOwnership?.Builder != null)
                    {
                        otherOwnership.DeclaringEntityType.Builder.HasNoRelationship(otherOwnership, configurationSource);
                    }

                    foreach (var invertedOwnership in invertedOwnerships)
                    {
                        if (invertedOwnership.Builder != null)
                        {
                            invertedOwnership.DeclaringEntityType.Builder.HasNoRelationship(invertedOwnership, configurationSource);
                        }
                    }

                    return newRelationshipBuilder;
                }

                if (otherOwnership != null)
                {
                    if (!Metadata.GetConfigurationSource().Overrides(ConfigurationSource.Explicit)
                        && Metadata.PrincipalEntityType.IsInDefinitionPath(Metadata.DeclaringEntityType.ClrType))
                    {
                        return null;
                    }

                    newRelationshipBuilder = Metadata.SetIsOwnership(ownership: true, configurationSource)?.Builder;
                    newRelationshipBuilder = newRelationshipBuilder?.OnDelete(DeleteBehavior.Cascade, ConfigurationSource.Convention);

                    if (newRelationshipBuilder == null)
                    {
                        return null;
                    }

                    using var batch = ModelBuilder.Metadata.ConventionDispatcher.DelayConventions();
                    foreach (var invertedOwnership in invertedOwnerships)
                    {
                        invertedOwnership.DeclaringEntityType.Builder.HasNoRelationship(invertedOwnership, configurationSource);
                    }

                    var fk = newRelationshipBuilder.Metadata;
                    fk.DeclaringEntityType.Builder.HasNoRelationship(fk, fk.GetConfigurationSource());

                    if (otherOwnership.Builder.IsWeakTypeDefinition(configurationSource) == null)
                    {
                        return null;
                    }

                    var newEntityType = declaringType.ClrType == null
                        ? ModelBuilder.Entity(
                            declaringType.Name,
                            Metadata.PrincipalToDependent.Name,
                            Metadata.PrincipalEntityType,
                            declaringType.GetConfigurationSource()).Metadata
                        : ModelBuilder.Entity(
                            declaringType.ClrType,
                            Metadata.PrincipalToDependent.Name,
                            Metadata.PrincipalEntityType,
                            declaringType.GetConfigurationSource()).Metadata;

                    newRelationshipBuilder = newRelationshipBuilder.Attach(newEntityType.Builder);

                    ModelBuilder.Metadata.ConventionDispatcher.Tracker.Update(
                        Metadata, newRelationshipBuilder.Metadata);

                    return batch.Run(newRelationshipBuilder);
                }

                newRelationshipBuilder = Metadata.SetIsOwnership(ownership: true, configurationSource)?.Builder;
                newRelationshipBuilder = newRelationshipBuilder?.OnDelete(DeleteBehavior.Cascade, ConfigurationSource.Convention);

                if (newRelationshipBuilder == null
                    && Metadata.PrincipalEntityType.Builder != null
                    && Metadata.PrincipalToDependent != null)
                {
                    newRelationshipBuilder = Metadata.PrincipalEntityType.FindNavigation(Metadata.PrincipalToDependent.Name)
                        ?.ForeignKey.Builder;
                }

                using (var batch = ModelBuilder.Metadata.ConventionDispatcher.DelayConventions())
                {
                    foreach (var invertedOwnership in invertedOwnerships)
                    {
                        invertedOwnership.DeclaringEntityType.Builder.HasNoRelationship(invertedOwnership, configurationSource);
                    }

                    if (newRelationshipBuilder != null)
                    {
                        newRelationshipBuilder.Metadata.DeclaringEntityType.Builder.HasBaseType((Type)null, configurationSource);

                        if (!newRelationshipBuilder.Metadata.DeclaringEntityType.Builder
                            .RemoveNonOwnershipRelationships(newRelationshipBuilder.Metadata, configurationSource))
                        {
                            return null;
                        }

                        return batch.Run(newRelationshipBuilder);
                    }
                }

                return null;
            }

            return Metadata.SetIsOwnership(ownership: false, configurationSource)?.Builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetIsOwnership(bool? ownership, ConfigurationSource? configurationSource)
            => Metadata.IsOwnership == ownership || configurationSource.Overrides(Metadata.GetIsOwnershipConfigurationSource());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder IsWeakTypeDefinition(ConfigurationSource configurationSource)
        {
            if (Metadata.DeclaringEntityType.HasDefiningNavigation())
            {
                return this;
            }

            EntityType newEntityType;
            if (Metadata.DeclaringEntityType.ClrType == null)
            {
                newEntityType = ModelBuilder.Entity(
                    Metadata.DeclaringEntityType.Name,
                    Metadata.PrincipalToDependent.Name,
                    Metadata.PrincipalEntityType,
                    Metadata.DeclaringEntityType.GetConfigurationSource()).Metadata;
            }
            else
            {
                newEntityType = ModelBuilder.Entity(
                    Metadata.DeclaringEntityType.ClrType,
                    Metadata.PrincipalToDependent.Name,
                    Metadata.PrincipalEntityType,
                    Metadata.DeclaringEntityType.GetConfigurationSource()).Metadata;
            }

            var newOwnership = newEntityType.GetForeignKeys().SingleOrDefault(fk => fk.IsOwnership);
            if (newOwnership == null)
            {
                Check.DebugAssert(Metadata.Builder != null, "Metadata.Builder is null");
                return Metadata.Builder;
            }

            Check.DebugAssert(Metadata.Builder == null, "Metadata.Builder is not null");
            ModelBuilder.Metadata.ConventionDispatcher.Tracker.Update(Metadata, newOwnership);
            return newOwnership.Builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder OnDelete(DeleteBehavior? deleteBehavior, ConfigurationSource configurationSource)
            => CanSetDeleteBehavior(deleteBehavior, configurationSource)
                ? Metadata.SetDeleteBehavior(deleteBehavior, configurationSource)?.Builder
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetDeleteBehavior(DeleteBehavior? deleteBehavior, ConfigurationSource? configurationSource)
            => Metadata.DeleteBehavior == deleteBehavior || configurationSource.Overrides(Metadata.GetDeleteBehaviorConfigurationSource());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder IsUnique(bool? unique, ConfigurationSource configurationSource)
        {
            if (Metadata.IsUnique == unique)
            {
                return Metadata.SetIsUnique(unique, configurationSource).Builder;
            }

            if (!CanSetIsUnique(unique, configurationSource, out var resetToDependent))
            {
                return null;
            }

            using var batch = Metadata.DeclaringEntityType.Model.ConventionDispatcher.DelayConventions();
            var builder = this;
            if (resetToDependent)
            {
                builder = builder.HasNavigations(navigationToPrincipal: null, MemberIdentity.None, configurationSource);
                if (builder == null)
                {
                    return null;
                }
            }

            builder = builder.Metadata.SetIsUnique(unique, configurationSource)?.Builder;
            Check.DebugAssert(builder != null, "builder is null");

            return batch.Run(builder);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetIsUnique(bool? unique, ConfigurationSource? configurationSource)
            => CanSetIsUnique(unique, configurationSource, out _);

        private bool CanSetIsUnique(bool? unique, ConfigurationSource? configurationSource, out bool resetToDependent)
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

            if (Metadata.PrincipalToDependent?.IsShadowProperty() == false
                && !Navigation.IsCompatible(
                    Metadata.PrincipalToDependent.GetIdentifyingMemberInfo(),
                    Metadata.PrincipalEntityType.ClrType,
                    Metadata.DeclaringEntityType.ClrType,
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
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder DependentEntityType(
            [NotNull] InternalEntityTypeBuilder dependentEntityTypeBuilder, ConfigurationSource configurationSource)
            => DependentEntityType(dependentEntityTypeBuilder.Metadata, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder DependentEntityType(
            [NotNull] Type dependentType, ConfigurationSource configurationSource)
            => DependentEntityType(
                ModelBuilder.Entity(dependentType, configurationSource).Metadata,
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder DependentEntityType(
            [NotNull] string dependentTypeName, ConfigurationSource configurationSource)
            => DependentEntityType(ModelBuilder.Entity(dependentTypeName, configurationSource).Metadata, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder DependentEntityType(
            [NotNull] EntityType dependentEntityType, ConfigurationSource configurationSource)
        {
            Check.NotNull(dependentEntityType, nameof(dependentEntityType));

            var builder = this;
            if (Metadata.DeclaringEntityType.IsAssignableFrom(dependentEntityType))
            {
                if (Metadata.GetPrincipalEndConfigurationSource()?.Overrides(configurationSource) != true)
                {
                    Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);

                    builder =
                        (InternalRelationshipBuilder)ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyPrincipalEndChanged(builder);
                }

                return builder;
            }

            return dependentEntityType.IsAssignableFrom(Metadata.DeclaringEntityType)
                || configurationSource == ConfigurationSource.Explicit
                    ? HasEntityTypes(Metadata.PrincipalEntityType, dependentEntityType, configurationSource)
                    : null;
        }

        // Note: These will not invert relationships, use RelatedEntityTypes for that
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder PrincipalEntityType(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder, ConfigurationSource configurationSource)
            => PrincipalEntityType(principalEntityTypeBuilder.Metadata, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder PrincipalEntityType(
            [NotNull] Type principalType, ConfigurationSource configurationSource)
            => PrincipalEntityType(
                ModelBuilder.Entity(principalType, configurationSource).Metadata,
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder PrincipalEntityType(
            [NotNull] string principalTypeName, ConfigurationSource configurationSource)
            => PrincipalEntityType(
                ModelBuilder.Entity(principalTypeName, configurationSource).Metadata,
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder PrincipalEntityType(
            [NotNull] EntityType principalEntityType, ConfigurationSource configurationSource)
        {
            Check.NotNull(principalEntityType, nameof(principalEntityType));

            var builder = this;
            if (Metadata.PrincipalEntityType.IsAssignableFrom(principalEntityType))
            {
                if (Metadata.GetPrincipalEndConfigurationSource()?.Overrides(configurationSource) != true)
                {
                    Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);

                    builder =
                        (InternalRelationshipBuilder)ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyPrincipalEndChanged(builder);
                }

                return builder;
            }

            return principalEntityType.IsAssignableFrom(Metadata.PrincipalEntityType)
                || configurationSource == ConfigurationSource.Explicit
                    ? HasEntityTypes(principalEntityType, Metadata.DeclaringEntityType, configurationSource)
                    : null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasEntityTypes(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            ConfigurationSource configurationSource)
            => HasEntityTypes(principalEntityType, dependentEntityType, configurationSource, configurationSource);

        private InternalRelationshipBuilder HasEntityTypes(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            ConfigurationSource? principalEndConfigurationSource,
            ConfigurationSource configurationSource)
        {
            if ((Metadata.PrincipalEntityType == principalEntityType
                    && Metadata.DeclaringEntityType == dependentEntityType)
                || (Metadata.PrincipalEntityType == principalEntityType.LeastDerivedType(Metadata.PrincipalEntityType)
                    && Metadata.DeclaringEntityType == dependentEntityType.LeastDerivedType(Metadata.DeclaringEntityType)))
            {
                if (!principalEndConfigurationSource.HasValue
                    || Metadata.GetPrincipalEndConfigurationSource()?.Overrides(principalEndConfigurationSource) == true)
                {
                    return this;
                }

                Metadata.UpdatePrincipalEndConfigurationSource(principalEndConfigurationSource.Value);

                return (InternalRelationshipBuilder)ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyPrincipalEndChanged(this);
            }

            if (!CanSetRelatedTypes(
                    principalEntityType,
                    dependentEntityType,
                    strictPrincipal: true,
                    navigationToPrincipal: null,
                    navigationToDependent: null,
                    configurationSource,
                    shouldThrow: true,
                    out var shouldInvert,
                    out var shouldResetToPrincipal,
                    out var shouldResetToDependent,
                    out var shouldResetPrincipalProperties,
                    out var shouldResetDependentProperties,
                    out var shouldBeUnique)
                && configurationSource != ConfigurationSource.Explicit)
            {
                return null;
            }

            var dependentProperties = (IReadOnlyList<Property>)Array.Empty<Property>();
            var principalProperties = (IReadOnlyList<Property>)Array.Empty<Property>();
            var builder = this;
            if (shouldInvert)
            {
                Check.DebugAssert(
                    configurationSource.Overrides(Metadata.GetPropertiesConfigurationSource()),
                    "configurationSource does not override Metadata.GetPropertiesConfigurationSource");

                Check.DebugAssert(
                    configurationSource.Overrides(Metadata.GetPrincipalKeyConfigurationSource()),
                    "configurationSource does not override Metadata.GetPrincipalKeyConfigurationSource");

                principalEntityType = principalEntityType.LeastDerivedType(Metadata.DeclaringEntityType);
                dependentEntityType = dependentEntityType.LeastDerivedType(Metadata.PrincipalEntityType);

                if (Metadata.GetIsRequiredConfigurationSource() != ConfigurationSource.Explicit)
                {
                    Metadata.SetIsRequiredConfigurationSource(configurationSource: null);
                }
            }
            else
            {
                principalEntityType = principalEntityType.LeastDerivedType(Metadata.PrincipalEntityType);
                dependentEntityType = dependentEntityType.LeastDerivedType(Metadata.DeclaringEntityType);

                dependentProperties = shouldResetDependentProperties
                    ? dependentProperties
                    : ((Metadata.GetPropertiesConfigurationSource()?.Overrides(configurationSource) ?? false)
                        ? dependentEntityType.Builder.GetActualProperties(Metadata.Properties, configurationSource)
                        : null);

                principalProperties = shouldResetPrincipalProperties
                    ? principalProperties
                    : ((Metadata.GetPrincipalKeyConfigurationSource()?.Overrides(configurationSource) ?? false)
                        ? principalEntityType.Builder.GetActualProperties(Metadata.PrincipalKey.Properties, configurationSource)
                        : null);
            }

            return builder.ReplaceForeignKey(
                configurationSource,
                principalEntityType.Builder,
                dependentEntityType.Builder,
                shouldResetToPrincipal ? MemberIdentity.None : (MemberIdentity?)null,
                shouldResetToDependent ? MemberIdentity.None : (MemberIdentity?)null,
                dependentProperties,
                principalProperties: principalProperties,
                isUnique: shouldBeUnique,
                removeCurrent: true,
                principalEndConfigurationSource: principalEndConfigurationSource,
                oldRelationshipInverted: shouldInvert);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetRelatedTypes(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            ConfigurationSource? configurationSource)
            => CanSetRelatedTypes(
                principalEntityType,
                dependentEntityType,
                strictPrincipal: true,
                navigationToPrincipal: null,
                navigationToDependent: null,
                configurationSource,
                shouldThrow: false,
                out _,
                out _,
                out _,
                out _,
                out _,
                out _);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanInvert(
            [CanBeNull] IReadOnlyList<Property> newForeignKeyProperties, ConfigurationSource? configurationSource)
            => configurationSource.Overrides(Metadata.GetPrincipalEndConfigurationSource())
                && ((newForeignKeyProperties == null)
                    || CanSetForeignKey(newForeignKeyProperties, Metadata.PrincipalEntityType, configurationSource));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder ReuniquifyTemporaryProperties(bool force)
        {
            if (!force
                && (Metadata.GetPropertiesConfigurationSource() != null
                    || !Metadata.DeclaringEntityType.Builder
                        .ShouldReuniquifyTemporaryProperties(Metadata)))
            {
                return Metadata.Builder;
            }

            var relationshipBuilder = this;
            using var batch = Metadata.DeclaringEntityType.Model.ConventionDispatcher.DelayConventions();

            var temporaryProperties = Metadata.Properties.Where(
                p => p.IsShadowProperty()
                    && ConfigurationSource.Convention.Overrides(p.GetConfigurationSource())).ToList();

            var keysToDetach = temporaryProperties.SelectMany(
                    p => p.GetContainingKeys()
                        .Where(k => ConfigurationSource.Convention.Overrides(k.GetConfigurationSource())))
                .Distinct().ToList();

            List<RelationshipSnapshot> detachedRelationships = null;
            foreach (var key in keysToDetach)
            {
                foreach (var referencingForeignKey in key.GetReferencingForeignKeys().ToList())
                {
                    if (detachedRelationships == null)
                    {
                        detachedRelationships = new List<RelationshipSnapshot>();
                    }

                    detachedRelationships.Add(InternalEntityTypeBuilder.DetachRelationship(referencingForeignKey));
                }
            }

            var detachedKeys = InternalEntityTypeBuilder.DetachKeys(keysToDetach);

            var detachedIndexes = InternalEntityTypeBuilder.DetachIndexes(
                temporaryProperties.SelectMany(p => p.GetContainingIndexes()).Distinct());

            relationshipBuilder = relationshipBuilder.HasForeignKey((IReadOnlyList<Property>)null, ConfigurationSource.Convention);

            if (detachedIndexes != null)
            {
                foreach (var indexBuilderTuple in detachedIndexes)
                {
                    indexBuilderTuple.Attach(indexBuilderTuple.Metadata.DeclaringEntityType.Builder);
                }
            }

            if (detachedKeys != null)
            {
                foreach (var detachedKeyTuple in detachedKeys)
                {
                    detachedKeyTuple.Item1.Attach(Metadata.DeclaringEntityType.RootType().Builder, detachedKeyTuple.Item2);
                }
            }

            if (detachedRelationships != null)
            {
                foreach (var detachedRelationship in detachedRelationships)
                {
                    detachedRelationship.Attach();
                }
            }

            return batch.Run(relationshipBuilder);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [CanBeNull] IReadOnlyList<MemberInfo> properties, ConfigurationSource configurationSource)
            => HasForeignKey(properties, Metadata.DeclaringEntityType, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [CanBeNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => HasForeignKey(propertyNames, Metadata.DeclaringEntityType, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [CanBeNull] IReadOnlyList<MemberInfo> properties, [NotNull] EntityType dependentEntityType,
            ConfigurationSource configurationSource)
            => HasForeignKey(
                dependentEntityType.Builder.GetOrCreateProperties(properties, configurationSource),
                dependentEntityType,
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [CanBeNull] IReadOnlyList<string> propertyNames, [NotNull] EntityType dependentEntityType,
            ConfigurationSource configurationSource)
            => HasForeignKey(
                dependentEntityType.Builder.GetOrCreateProperties(
                    propertyNames,
                    configurationSource,
                    Metadata.PrincipalKey.Properties,
                    Metadata.GetIsRequiredConfigurationSource() != null && Metadata.IsRequired,
                    Metadata.GetPrincipalKeyConfigurationSource() == null
                    && Metadata.PrincipalEntityType.FindPrimaryKey() == null),
                dependentEntityType,
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
            => HasForeignKey(properties, Metadata.DeclaringEntityType, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [CanBeNull] IReadOnlyList<Property> properties,
            [NotNull] EntityType dependentEntityType,
            ConfigurationSource configurationSource)
        {
            if (properties == null)
            {
                return !configurationSource.Overrides(Metadata.GetPropertiesConfigurationSource())
                    ? null
                    : ReplaceForeignKey(
                        configurationSource,
                        dependentProperties: Array.Empty<Property>());
            }

            properties = dependentEntityType.Builder.GetActualProperties(properties, configurationSource);
            if (Metadata.Properties.SequenceEqual(properties))
            {
                Metadata.UpdatePropertiesConfigurationSource(configurationSource);

                var builder = this;
                if (!Metadata.IsSelfReferencing()
                    && Metadata.GetPrincipalEndConfigurationSource()?.Overrides(configurationSource) != true)
                {
                    Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);

                    builder =
                        (InternalRelationshipBuilder)ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyPrincipalEndChanged(builder);
                }

                return builder;
            }

            if (!CanSetForeignKey(
                properties, dependentEntityType, configurationSource, out var resetPrincipalKey))
            {
                return null;
            }

            return ReplaceForeignKey(
                configurationSource,
                dependentEntityTypeBuilder: dependentEntityType.Builder,
                dependentProperties: properties,
                principalProperties: resetPrincipalKey ? Array.Empty<Property>() : null,
                principalEndConfigurationSource: configurationSource,
                removeCurrent: !Property.AreCompatible(properties, Metadata.DeclaringEntityType));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetForeignKey([CanBeNull] IReadOnlyList<string> propertyNames, ConfigurationSource? configurationSource)
        {
            var properties = Metadata.DeclaringEntityType.FindProperties(propertyNames);
            if (properties != null)
            {
                return CanSetForeignKey(
                    properties,
                    dependentEntityType: null,
                    configurationSource,
                    out _);
            }

            return configurationSource.Overrides(Metadata.GetPropertiesConfigurationSource());
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetForeignKey([CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource? configurationSource)
            => CanSetForeignKey(
                properties,
                dependentEntityType: null,
                configurationSource,
                out _);

        private bool CanSetForeignKey(
            IReadOnlyList<Property> properties,
            EntityType dependentEntityType,
            ConfigurationSource? configurationSource,
            bool overrideSameSource = true)
            => CanSetForeignKey(
                properties,
                dependentEntityType,
                configurationSource,
                out _,
                overrideSameSource);

        private bool CanSetForeignKey(
            IReadOnlyList<Property> properties,
            EntityType dependentEntityType,
            ConfigurationSource? configurationSource,
            out bool resetPrincipalKey,
            bool overrideSameSource = true)
        {
            resetPrincipalKey = false;
            return properties != null
                && Metadata.Properties.SequenceEqual(properties)
                || CanSetForeignKey(
                    properties,
                    dependentEntityType,
                    Metadata.PrincipalKey.Properties,
                    Metadata.PrincipalEntityType,
                    configurationSource,
                    out resetPrincipalKey,
                    overrideSameSource);
        }

        private bool CanSetForeignKey(
            IReadOnlyList<Property> properties,
            EntityType dependentEntityType,
            IReadOnlyList<Property> principalKeyProperties,
            EntityType principalEntityType,
            ConfigurationSource? configurationSource,
            out bool resetPrincipalKey,
            bool overrideSameSource = true)
        {
            resetPrincipalKey = false;

            if (!configurationSource.Overrides(Metadata.GetPropertiesConfigurationSource())
                || (!overrideSameSource && configurationSource == Metadata.GetPropertiesConfigurationSource()))
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

            // FKs are not allowed to use properties from inherited keys since this could result in an ambiguous value space
            if (dependentEntityType.BaseType != null
                && configurationSource != ConfigurationSource.Explicit // let it throw for explicit
                && properties.Any(p => p.GetContainingKeys().Any(k => k.DeclaringEntityType != dependentEntityType)))
            {
                return false;
            }

            if ((dependentEntityType != Metadata.DeclaringEntityType
                    && dependentEntityType == Metadata.PrincipalEntityType) // Check if inverted
                || (properties.Count != 0
                    && !ForeignKey.AreCompatible(
                        principalKeyProperties,
                        properties,
                        principalEntityType,
                        dependentEntityType,
                        shouldThrow: false)))
            {
                if (!configurationSource.HasValue
                    || !configurationSource.Overrides(Metadata.GetPrincipalKeyConfigurationSource()))
                {
                    return false;
                }

                resetPrincipalKey = true;
            }

            return true;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasPrincipalKey(
            [NotNull] IReadOnlyList<MemberInfo> properties,
            ConfigurationSource configurationSource)
            => HasPrincipalKey(
                Metadata.PrincipalEntityType.Builder.GetOrCreateProperties(properties, configurationSource),
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasPrincipalKey(
            [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
            => HasPrincipalKey(
                Metadata.PrincipalEntityType.Builder.GetOrCreateProperties(propertyNames, configurationSource),
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasPrincipalKey(
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            if (properties == null)
            {
                return !configurationSource.Overrides(Metadata.GetPrincipalKeyConfigurationSource())
                    ? null
                    : ReplaceForeignKey(
                        configurationSource,
                        principalProperties: Array.Empty<Property>());
            }

            properties = Metadata.PrincipalEntityType.Builder.GetActualProperties(properties, configurationSource);

            if (Metadata.PrincipalKey.Properties.SequenceEqual(properties))
            {
                Metadata.UpdatePrincipalKeyConfigurationSource(configurationSource);

                var builder = this;
                if (!Metadata.IsSelfReferencing()
                    && Metadata.GetPrincipalEndConfigurationSource()?.Overrides(configurationSource) != true)
                {
                    Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);

                    builder =
                        (InternalRelationshipBuilder)ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyPrincipalEndChanged(builder);
                }

                return builder;
            }

            if (!CanSetPrincipalKey(properties, configurationSource, out var resetDependent, out var oldNameDependentProperties))
            {
                return null;
            }

            return ReplaceForeignKey(
                configurationSource,
                principalProperties: properties,
                dependentProperties: resetDependent ? Array.Empty<Property>() : null,
                principalEndConfigurationSource: configurationSource,
                oldNameDependentProperties: oldNameDependentProperties,
                removeCurrent: oldNameDependentProperties != null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetPrincipalKey([CanBeNull] IReadOnlyList<string> propertyNames, ConfigurationSource? configurationSource)
        {
            var properties = Metadata.PrincipalEntityType.FindProperties(propertyNames);
            if (properties != null)
            {
                return CanSetPrincipalKey(
                    properties,
                    configurationSource,
                    out _,
                    out _);
            }

            return configurationSource.Overrides(Metadata.GetPrincipalKeyConfigurationSource());
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetPrincipalKey([CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource? configurationSource)
            => CanSetPrincipalKey(
                properties,
                configurationSource,
                out _,
                out _);

        private bool CanSetPrincipalKey(
            IReadOnlyList<Property> properties,
            ConfigurationSource? configurationSource,
            out bool resetDependent,
            out IReadOnlyList<Property> oldNameDependentProperties)
        {
            resetDependent = false;
            oldNameDependentProperties = null;

            if (Metadata.PrincipalKey.Properties.SequenceEqual(properties))
            {
                return true;
            }

            if (!configurationSource.Overrides(Metadata.GetPrincipalKeyConfigurationSource()))
            {
                return false;
            }

            if (properties == null)
            {
                return true;
            }

            if (!ForeignKey.AreCompatible(
                properties,
                Metadata.Properties,
                Metadata.PrincipalEntityType,
                Metadata.DeclaringEntityType,
                shouldThrow: false))
            {
                if (!configurationSource.Value.Overrides(Metadata.GetPropertiesConfigurationSource()))
                {
                    return false;
                }

                if (Metadata.GetPropertiesConfigurationSource().Overrides(ConfigurationSource.DataAnnotation)
                    && Metadata.Properties.All(
                        p => ConfigurationSource.Convention.Overrides(p.GetTypeConfigurationSource())
                            && p.IsShadowProperty()))
                {
                    oldNameDependentProperties = Metadata.Properties;
                }

                resetDependent = true;
            }

            return true;
        }

        private InternalRelationshipBuilder ReplaceForeignKey(
            ConfigurationSource configurationSource,
            InternalEntityTypeBuilder principalEntityTypeBuilder = null,
            InternalEntityTypeBuilder dependentEntityTypeBuilder = null,
            MemberIdentity? navigationToPrincipal = null,
            MemberIdentity? navigationToDependent = null,
            IReadOnlyList<Property> dependentProperties = null,
            IReadOnlyList<Property> oldNameDependentProperties = null,
            IReadOnlyList<Property> principalProperties = null,
            bool? isUnique = null,
            bool? isRequired = null,
            bool? isOwnership = null,
            DeleteBehavior? deleteBehavior = null,
            bool removeCurrent = false,
            ConfigurationSource? principalEndConfigurationSource = null,
            bool oldRelationshipInverted = false)
        {
            principalEntityTypeBuilder ??= (oldRelationshipInverted
                ? Metadata.DeclaringEntityType.Builder
                : Metadata.PrincipalEntityType.Builder);
            dependentEntityTypeBuilder ??= (oldRelationshipInverted
                ? Metadata.PrincipalEntityType.Builder
                : Metadata.DeclaringEntityType.Builder);

            if (navigationToPrincipal == null)
            {
                if (oldRelationshipInverted)
                {
                    navigationToPrincipal = Metadata.GetPrincipalToDependentConfigurationSource()?.Overrides(configurationSource)
                        ?? false
                            ? Metadata.PrincipalToDependent.CreateMemberIdentity()
                            : navigationToPrincipal;
                }
                else
                {
                    navigationToPrincipal = Metadata.GetDependentToPrincipalConfigurationSource()?.Overrides(configurationSource)
                        ?? false
                            ? Metadata.DependentToPrincipal.CreateMemberIdentity()
                            : navigationToPrincipal;
                }
            }

            if (navigationToDependent == null)
            {
                if (oldRelationshipInverted)
                {
                    navigationToDependent = Metadata.GetDependentToPrincipalConfigurationSource()?.Overrides(configurationSource)
                        ?? false
                            ? Metadata.DependentToPrincipal.CreateMemberIdentity()
                            : navigationToDependent;
                }
                else
                {
                    navigationToDependent = Metadata.GetPrincipalToDependentConfigurationSource()?.Overrides(configurationSource)
                        ?? false
                            ? Metadata.PrincipalToDependent.CreateMemberIdentity()
                            : navigationToDependent;
                }
            }

            dependentProperties ??= ((Metadata.GetPropertiesConfigurationSource()?.Overrides(configurationSource) ?? false)
                && !oldRelationshipInverted
                    ? Metadata.Properties
                    : null);

            principalProperties ??= ((Metadata.GetPrincipalKeyConfigurationSource()?.Overrides(configurationSource) ?? false)
                && !oldRelationshipInverted
                    ? Metadata.PrincipalKey.Properties
                    : null);

            isUnique ??= ((Metadata.GetIsUniqueConfigurationSource()?.Overrides(configurationSource) ?? false)
                ? Metadata.IsUnique
                : (bool?)null);

            isRequired ??= ((Metadata.GetIsRequiredConfigurationSource()?.Overrides(configurationSource) ?? false)
                ? Metadata.IsRequired
                : (bool?)null);

            isOwnership ??= ((Metadata.GetIsOwnershipConfigurationSource()?.Overrides(configurationSource) ?? false)
                && !oldRelationshipInverted
                    ? Metadata.IsOwnership
                    : (bool?)null);

            deleteBehavior ??= ((Metadata.GetDeleteBehaviorConfigurationSource()?.Overrides(configurationSource) ?? false)
                ? Metadata.DeleteBehavior
                : (DeleteBehavior?)null);

            principalEndConfigurationSource ??= (principalEntityTypeBuilder.Metadata != dependentEntityTypeBuilder.Metadata
                && ((principalProperties?.Count > 0)
                    || (dependentProperties?.Count > 0)
                    || (navigationToDependent != null && isUnique == false)
                    || isOwnership == true)
                    ? configurationSource
                    : (ConfigurationSource?)null);
            principalEndConfigurationSource = principalEndConfigurationSource.Max(Metadata.GetPrincipalEndConfigurationSource());

            return ReplaceForeignKey(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                navigationToPrincipal,
                navigationToDependent,
                dependentProperties,
                oldNameDependentProperties,
                principalProperties,
                isUnique,
                isRequired,
                isOwnership,
                deleteBehavior,
                removeCurrent,
                oldRelationshipInverted,
                principalEndConfigurationSource,
                configurationSource);
        }

        private InternalRelationshipBuilder ReplaceForeignKey(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [NotNull] InternalEntityTypeBuilder dependentEntityTypeBuilder,
            MemberIdentity? navigationToPrincipal,
            MemberIdentity? navigationToDependent,
            [CanBeNull] IReadOnlyList<Property> dependentProperties,
            [CanBeNull] IReadOnlyList<Property> oldNameDependentProperties,
            [CanBeNull] IReadOnlyList<Property> principalProperties,
            bool? isUnique,
            bool? isRequired,
            bool? isOwnership,
            DeleteBehavior? deleteBehavior,
            bool removeCurrent,
            bool oldRelationshipInverted,
            ConfigurationSource? principalEndConfigurationSource,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(principalEntityTypeBuilder, nameof(principalEntityTypeBuilder));
            Check.NotNull(dependentEntityTypeBuilder, nameof(dependentEntityTypeBuilder));

            Check.DebugAssert(
                navigationToPrincipal?.Name == null
                || navigationToPrincipal.Value.MemberInfo != null
                || !dependentEntityTypeBuilder.Metadata.HasClrType(),
                "Principal navigation check failed");

            Check.DebugAssert(
                navigationToDependent?.Name == null
                || navigationToDependent.Value.MemberInfo != null
                || !principalEntityTypeBuilder.Metadata.HasClrType(),
                "Dependent navigation check failed");

            Check.DebugAssert(
                AreCompatible(
                    principalEntityTypeBuilder.Metadata,
                    dependentEntityTypeBuilder.Metadata,
                    navigationToPrincipal?.MemberInfo,
                    navigationToDependent?.MemberInfo,
                    dependentProperties?.Count > 0 ? dependentProperties : null,
                    principalProperties?.Count > 0 ? principalProperties : null,
                    isUnique,
                    configurationSource),
                "Compatibility check failed");

            Check.DebugAssert(
                oldNameDependentProperties == null || (dependentProperties?.Count ?? 0) == 0,
                "Dependent properties check failed");

            Check.DebugAssert(
                removeCurrent
                || Metadata.Builder == null
                || (Metadata.PrincipalEntityType.IsAssignableFrom(principalEntityTypeBuilder.Metadata)
                    && Metadata.DeclaringEntityType.IsAssignableFrom(dependentEntityTypeBuilder.Metadata)),
                "Entity type check failed");

            InternalRelationshipBuilder newRelationshipBuilder;
            using (var batch = Metadata.DeclaringEntityType.Model.ConventionDispatcher.DelayConventions())
            {
                newRelationshipBuilder = GetOrCreateRelationshipBuilder(
                    principalEntityTypeBuilder.Metadata,
                    dependentEntityTypeBuilder.Metadata,
                    navigationToPrincipal,
                    navigationToDependent,
                    dependentProperties?.Count > 0 ? dependentProperties : null,
                    oldNameDependentProperties,
                    principalProperties?.Count > 0 ? principalProperties : null,
                    isRequired,
                    removeCurrent,
                    principalEndConfigurationSource,
                    configurationSource,
                    out var existingRelationshipInverted);

                if (newRelationshipBuilder == null)
                {
                    return null;
                }

                var initialPrincipalEndConfigurationSource = newRelationshipBuilder.Metadata.GetPrincipalEndConfigurationSource();

                var strictPrincipal = principalEndConfigurationSource.HasValue
                    && principalEndConfigurationSource.Value.Overrides(initialPrincipalEndConfigurationSource);
                if (existingRelationshipInverted == true
                    && !strictPrincipal)
                {
                    oldRelationshipInverted = !oldRelationshipInverted;
                    existingRelationshipInverted = false;

                    var entityTypeBuilder = principalEntityTypeBuilder;
                    principalEntityTypeBuilder = dependentEntityTypeBuilder;
                    dependentEntityTypeBuilder = entityTypeBuilder;

                    var navigation = navigationToPrincipal;
                    navigationToPrincipal = navigationToDependent;
                    navigationToDependent = navigation;

                    dependentProperties = null;
                    principalProperties = null;
                }

                var oldNavigationToPrincipal = oldRelationshipInverted
                    ? Metadata.PrincipalToDependent
                    : Metadata.DependentToPrincipal;
                var oldNavigationToDependent = oldRelationshipInverted
                    ? Metadata.DependentToPrincipal
                    : Metadata.PrincipalToDependent;

                var oldToPrincipalConfigurationSource = oldRelationshipInverted
                    ? Metadata.GetPrincipalToDependentConfigurationSource()
                    : Metadata.GetDependentToPrincipalConfigurationSource();
                var oldToDependentConfigurationSource = oldRelationshipInverted
                    ? Metadata.GetDependentToPrincipalConfigurationSource()
                    : Metadata.GetPrincipalToDependentConfigurationSource();

                var newRelationshipConfigurationSource = Metadata.GetConfigurationSource();
                if ((dependentProperties?.Count > 0)
                    || navigationToPrincipal?.Name != null
                    || navigationToDependent?.Name != null)
                {
                    newRelationshipConfigurationSource = newRelationshipConfigurationSource.Max(configurationSource);
                }

                newRelationshipBuilder.Metadata.UpdateConfigurationSource(newRelationshipConfigurationSource);

                var resetToPrincipal = newRelationshipBuilder.Metadata.DependentToPrincipal != null
                    && ((existingRelationshipInverted == false
                            && navigationToPrincipal != null
                            && navigationToPrincipal.Value.Name
                            != newRelationshipBuilder.Metadata.DependentToPrincipal.Name)
                        || (existingRelationshipInverted == true
                            && navigationToDependent != null
                            && navigationToDependent.Value.Name
                            != newRelationshipBuilder.Metadata.DependentToPrincipal.Name));

                var resetToDependent = newRelationshipBuilder.Metadata.PrincipalToDependent != null
                    && ((existingRelationshipInverted == false
                            && navigationToDependent != null
                            && navigationToDependent.Value.Name
                            != newRelationshipBuilder.Metadata.PrincipalToDependent.Name)
                        || (existingRelationshipInverted == true
                            && navigationToPrincipal != null
                            && navigationToPrincipal.Value.Name
                            != newRelationshipBuilder.Metadata.PrincipalToDependent.Name));

                if (resetToPrincipal
                    || resetToDependent)
                {
                    newRelationshipBuilder = newRelationshipBuilder.HasNavigations(
                            resetToPrincipal ? MemberIdentity.None : (MemberIdentity?)null,
                            resetToDependent ? MemberIdentity.None : (MemberIdentity?)null,
                            configurationSource)
                        ?? newRelationshipBuilder;
                }

                newRelationshipBuilder = newRelationshipBuilder.HasEntityTypes(
                        principalEntityTypeBuilder.Metadata,
                        dependentEntityTypeBuilder.Metadata,
                        principalEndConfigurationSource,
                        configurationSource)
                    ?? newRelationshipBuilder;

                dependentProperties = oldNameDependentProperties ?? dependentProperties;
                if (dependentProperties != null
                    || principalProperties != null)
                {
                    var shouldSetProperties = false;
                    ConfigurationSource? foreignKeyPropertiesConfigurationSource = null;
                    if (dependentProperties != null)
                    {
                        dependentProperties = dependentEntityTypeBuilder.GetActualProperties(dependentProperties, configurationSource);

                        foreignKeyPropertiesConfigurationSource = configurationSource;
                        if (PropertyListComparer.Instance.Equals(Metadata.Properties, dependentProperties)
                            && !oldRelationshipInverted)
                        {
                            foreignKeyPropertiesConfigurationSource =
                                foreignKeyPropertiesConfigurationSource.Max(Metadata.GetPropertiesConfigurationSource());
                        }

                        if (foreignKeyPropertiesConfigurationSource.HasValue)
                        {
                            if (newRelationshipBuilder.Metadata.Properties.SequenceEqual(dependentProperties))
                            {
                                var updated = newRelationshipBuilder.HasForeignKey(
                                    dependentProperties, foreignKeyPropertiesConfigurationSource.Value);

                                Check.DebugAssert(updated == newRelationshipBuilder, "updated != newRelationshipBuilder");
                            }
                            else if (dependentProperties.Count > 0
                                || (!removeCurrent
                                    && Metadata == newRelationshipBuilder.Metadata))
                            {
                                shouldSetProperties = true;
                            }
                        }
                    }

                    ConfigurationSource? principalKeyConfigurationSource = null;
                    if (principalProperties != null)
                    {
                        principalProperties = principalEntityTypeBuilder.GetActualProperties(principalProperties, configurationSource);

                        principalKeyConfigurationSource = configurationSource;
                        if (PropertyListComparer.Instance.Equals(
                                principalProperties, newRelationshipBuilder.Metadata.PrincipalKey.Properties)
                            && !oldRelationshipInverted)
                        {
                            principalKeyConfigurationSource =
                                principalKeyConfigurationSource.Max(Metadata.GetPrincipalKeyConfigurationSource());
                        }

                        if (principalKeyConfigurationSource.HasValue)
                        {
                            if (newRelationshipBuilder.Metadata.PrincipalKey.Properties.SequenceEqual(principalProperties))
                            {
                                var updated = newRelationshipBuilder.HasPrincipalKey(
                                    principalProperties, principalKeyConfigurationSource.Value);

                                Check.DebugAssert(updated == newRelationshipBuilder, "updated != newRelationshipBuilder");
                            }
                            else if (principalProperties.Count > 0
                                || (!removeCurrent
                                    && Metadata == newRelationshipBuilder.Metadata))
                            {
                                shouldSetProperties = true;
                            }
                        }
                    }

                    if (shouldSetProperties)
                    {
                        Key principalKey = null;
                        if (principalProperties != null
                            && principalProperties.Count != 0)
                        {
                            principalKey = principalEntityTypeBuilder.Metadata.RootType().Builder
                                .HasKey(principalProperties, configurationSource).Metadata;
                        }

                        var foreignKey = newRelationshipBuilder.Metadata;
                        newRelationshipBuilder = foreignKey.DeclaringEntityType.Builder.UpdateForeignKey(
                            foreignKey,
                            dependentProperties?.Count == 0 ? null : dependentProperties,
                            principalKey,
                            navigationToPrincipal?.Name,
                            isRequired,
                            configurationSource: null);

                        if (foreignKeyPropertiesConfigurationSource != null
                            && dependentProperties?.Count != 0)
                        {
                            newRelationshipBuilder.Metadata.UpdatePropertiesConfigurationSource(
                                foreignKeyPropertiesConfigurationSource.Value);
                        }

                        if (principalKeyConfigurationSource != null
                            && principalProperties.Count != 0)
                        {
                            newRelationshipBuilder.Metadata.UpdatePrincipalKeyConfigurationSource(
                                principalKeyConfigurationSource.Value);
                        }
                    }
                }

                if (isUnique.HasValue)
                {
                    var isUniqueConfigurationSource = configurationSource;
                    if (isUnique.Value == Metadata.IsUnique)
                    {
                        isUniqueConfigurationSource = isUniqueConfigurationSource.Max(Metadata.GetIsUniqueConfigurationSource());
                    }

                    newRelationshipBuilder = newRelationshipBuilder.IsUnique(
                            isUnique.Value,
                            isUniqueConfigurationSource)
                        ?? newRelationshipBuilder;
                }
                else if (!oldRelationshipInverted
                    && Metadata.GetIsUniqueConfigurationSource().HasValue
                    && !newRelationshipBuilder.Metadata.GetIsUniqueConfigurationSource().HasValue)
                {
                    newRelationshipBuilder = newRelationshipBuilder.IsUnique(
                            Metadata.IsUnique,
                            Metadata.GetIsUniqueConfigurationSource().Value)
                        ?? newRelationshipBuilder;
                }

                if (isRequired.HasValue)
                {
                    var isRequiredConfigurationSource = configurationSource;
                    if (isRequired.Value == Metadata.IsRequired)
                    {
                        isRequiredConfigurationSource = isRequiredConfigurationSource.Max(Metadata.GetIsRequiredConfigurationSource());
                    }

                    newRelationshipBuilder = newRelationshipBuilder.IsRequired(
                            isRequired.Value,
                            isRequiredConfigurationSource)
                        ?? newRelationshipBuilder;
                }
                else if (!oldRelationshipInverted
                    && Metadata.GetIsRequiredConfigurationSource().HasValue
                    && !newRelationshipBuilder.Metadata.GetIsRequiredConfigurationSource().HasValue)
                {
                    newRelationshipBuilder = newRelationshipBuilder.IsRequired(
                            Metadata.IsRequired,
                            Metadata.GetIsRequiredConfigurationSource().Value)
                        ?? newRelationshipBuilder;
                }

                if (deleteBehavior.HasValue)
                {
                    var deleteBehaviorConfigurationSource = configurationSource;
                    if (deleteBehavior.Value == Metadata.DeleteBehavior)
                    {
                        deleteBehaviorConfigurationSource =
                            deleteBehaviorConfigurationSource.Max(Metadata.GetDeleteBehaviorConfigurationSource());
                    }

                    newRelationshipBuilder = newRelationshipBuilder.OnDelete(
                            deleteBehavior.Value,
                            deleteBehaviorConfigurationSource)
                        ?? newRelationshipBuilder;
                }
                else if (!oldRelationshipInverted
                    && Metadata.GetDeleteBehaviorConfigurationSource().HasValue
                    && !newRelationshipBuilder.Metadata.GetDeleteBehaviorConfigurationSource().HasValue)
                {
                    newRelationshipBuilder = newRelationshipBuilder.OnDelete(
                            Metadata.DeleteBehavior,
                            Metadata.GetDeleteBehaviorConfigurationSource().Value)
                        ?? newRelationshipBuilder;
                }

                if (navigationToPrincipal != null)
                {
                    var navigationToPrincipalConfigurationSource = configurationSource;
                    if (navigationToPrincipal.Value.Name == oldNavigationToPrincipal?.Name)
                    {
                        navigationToPrincipalConfigurationSource =
                            navigationToPrincipalConfigurationSource.Max(oldToPrincipalConfigurationSource);
                    }

                    newRelationshipBuilder = newRelationshipBuilder.HasNavigations(
                            navigationToPrincipal,
                            navigationToDependent: null,
                            navigationToPrincipalConfigurationSource)
                        ?? newRelationshipBuilder;

                    if (oldNavigationToPrincipal != null
                        && newRelationshipBuilder.Metadata.DependentToPrincipal != null
                        && oldNavigationToPrincipal != newRelationshipBuilder.Metadata.DependentToPrincipal)
                    {
                        newRelationshipBuilder = MergeFacetsFrom(
                            newRelationshipBuilder.Metadata.DependentToPrincipal, oldNavigationToPrincipal);
                    }
                }
                else if (oldNavigationToPrincipal != null
                      && newRelationshipBuilder.Metadata.DependentToPrincipal == null
                      && newRelationshipBuilder.CanSetNavigations(
                         oldNavigationToPrincipal.CreateMemberIdentity(),
                         navigationToDependent: null,
                         oldToPrincipalConfigurationSource))
                {
                    newRelationshipBuilder = newRelationshipBuilder.HasNavigations(
                            oldNavigationToPrincipal.CreateMemberIdentity(),
                            navigationToDependent: null,
                            oldToPrincipalConfigurationSource.Value)
                        ?? newRelationshipBuilder;

                    if (newRelationshipBuilder.Metadata.DependentToPrincipal != null)
                    {
                        newRelationshipBuilder = MergeFacetsFrom(
                            newRelationshipBuilder.Metadata.DependentToPrincipal, oldNavigationToPrincipal);
                    }
                }

                if (navigationToDependent != null)
                {
                    var navigationToDependentConfigurationSource = configurationSource;
                    if (navigationToDependent.Value.Name == oldNavigationToDependent?.Name)
                    {
                        navigationToDependentConfigurationSource =
                            navigationToDependentConfigurationSource.Max(oldToDependentConfigurationSource);
                    }

                    newRelationshipBuilder = newRelationshipBuilder.HasNavigations(
                            navigationToPrincipal: null,
                            navigationToDependent,
                            navigationToDependentConfigurationSource)
                        ?? newRelationshipBuilder;

                    if (oldNavigationToDependent != null
                        && newRelationshipBuilder.Metadata.PrincipalToDependent != null
                        && oldNavigationToDependent != newRelationshipBuilder.Metadata.PrincipalToDependent)
                    {
                        newRelationshipBuilder = MergeFacetsFrom(
                            newRelationshipBuilder.Metadata.PrincipalToDependent, oldNavigationToDependent);
                    }
                }
                else if (oldNavigationToDependent != null
                      && newRelationshipBuilder.Metadata.PrincipalToDependent == null
                      && newRelationshipBuilder.CanSetNavigations(
                        navigationToPrincipal: null,
                        oldNavigationToDependent.CreateMemberIdentity(),
                        oldToDependentConfigurationSource))
                {
                    newRelationshipBuilder = newRelationshipBuilder.HasNavigations(
                            navigationToPrincipal: null,
                            oldNavigationToDependent.CreateMemberIdentity(),
                            oldToDependentConfigurationSource.Value)
                        ?? newRelationshipBuilder;

                    if (newRelationshipBuilder.Metadata.PrincipalToDependent != null)
                    {
                        newRelationshipBuilder = MergeFacetsFrom(
                            newRelationshipBuilder.Metadata.PrincipalToDependent, oldNavigationToDependent);
                    }
                }

                if (isOwnership.HasValue)
                {
                    var isOwnershipConfigurationSource = configurationSource;
                    if (isOwnership.Value == Metadata.IsOwnership)
                    {
                        isOwnershipConfigurationSource = isOwnershipConfigurationSource.Max(Metadata.GetIsOwnershipConfigurationSource());
                    }

                    newRelationshipBuilder = newRelationshipBuilder.IsOwnership(
                            isOwnership.Value,
                            isOwnershipConfigurationSource)
                        ?? newRelationshipBuilder;
                }
                else if (!oldRelationshipInverted
                    && Metadata.GetIsOwnershipConfigurationSource().HasValue)
                {
                    newRelationshipBuilder = newRelationshipBuilder.IsOwnership(
                            Metadata.IsOwnership,
                            Metadata.GetIsOwnershipConfigurationSource().Value)
                        ?? newRelationshipBuilder;
                }

                if (Metadata != newRelationshipBuilder.Metadata)
                {
                    newRelationshipBuilder.MergeAnnotationsFrom(Metadata);
                }

                newRelationshipBuilder = batch.Run(newRelationshipBuilder);
            }

            return newRelationshipBuilder;
        }

        private InternalRelationshipBuilder MergeFacetsFrom(Navigation newNavigation, IConventionNavigation oldNavigation)
        {
            newNavigation?.Builder.MergeAnnotationsFrom(oldNavigation);

            var builder = newNavigation.ForeignKey.Builder;

            var propertyAccessModeConfigurationSource = oldNavigation.GetPropertyAccessModeConfigurationSource();
            if (propertyAccessModeConfigurationSource.HasValue
                && builder.CanSetPropertyAccessMode(
                    oldNavigation.GetPropertyAccessMode(), newNavigation.IsOnDependent, propertyAccessModeConfigurationSource))
            {
                builder = builder.UsePropertyAccessMode(
                    oldNavigation.GetPropertyAccessMode(), newNavigation.IsOnDependent, propertyAccessModeConfigurationSource.Value);
            }

            var oldFieldInfoConfigurationSource = oldNavigation.GetFieldInfoConfigurationSource();
            if (oldFieldInfoConfigurationSource.HasValue
                && builder.CanSetField(oldNavigation.FieldInfo, newNavigation.IsOnDependent, oldFieldInfoConfigurationSource))
            {
                builder = builder.HasField(oldNavigation.FieldInfo, newNavigation.IsOnDependent, oldFieldInfoConfigurationSource.Value);
            }

            return builder;
        }

        private InternalRelationshipBuilder GetOrCreateRelationshipBuilder(
            EntityType principalEntityType,
            EntityType dependentEntityType,
            MemberIdentity? navigationToPrincipal,
            MemberIdentity? navigationToDependent,
            IReadOnlyList<Property> dependentProperties,
            IReadOnlyList<Property> oldNameDependentProperties,
            IReadOnlyList<Property> principalProperties,
            bool? isRequired,
            bool removeCurrent,
            ConfigurationSource? principalEndConfigurationSource,
            ConfigurationSource? configurationSource,
            out bool? existingRelationshipInverted)
        {
            var newRelationshipBuilder = FindCompatibleRelationship(
                principalEntityType,
                dependentEntityType,
                navigationToPrincipal,
                navigationToDependent,
                dependentProperties,
                principalProperties,
                principalEndConfigurationSource,
                configurationSource,
                out existingRelationshipInverted,
                out var conflictingRelationshipsFound,
                out var resolvableRelationships);

            if (conflictingRelationshipsFound)
            {
                return null;
            }
        
            // This workaround prevents the properties to be cleaned away before the new FK is created,
            // this should be replaced with reference counting
            // Issue #15898
            var temporaryProperties = dependentProperties?.Where(
                p => p.GetConfigurationSource() == ConfigurationSource.Convention
                    && p.IsShadowProperty()).ToList();
            var tempIndex = temporaryProperties?.Count > 0
                && dependentEntityType.FindIndex(temporaryProperties) == null
                    ? dependentEntityType.Builder.HasIndex(temporaryProperties, ConfigurationSource.Convention).Metadata
                    : null;

            var temporaryKeyProperties = principalProperties?.Where(
                p => p.GetConfigurationSource() == ConfigurationSource.Convention
                    && p.IsShadowProperty()).ToList();
            var keyTempIndex = temporaryKeyProperties?.Count > 0
                && principalEntityType.FindIndex(temporaryKeyProperties) == null
                    ? principalEntityType.Builder.HasIndex(temporaryKeyProperties, ConfigurationSource.Convention).Metadata
                    : null;

            var removedForeignKeys = new List<ForeignKey>();
            if (Metadata.Builder == null)
            {
                removedForeignKeys.Add(Metadata);
            }
            else
            {
                if (removeCurrent || newRelationshipBuilder != null)
                {
                    if (newRelationshipBuilder == this)
                    {
                        newRelationshipBuilder = null;
                    }

                    removedForeignKeys.Add(Metadata);
                    Metadata.DeclaringEntityType.Builder.HasNoRelationship(Metadata, ConfigurationSource.Explicit);
                }
                else
                {
                    existingRelationshipInverted = false;
                    newRelationshipBuilder = Metadata.Builder;
                }
            }

            foreach (var relationshipWithResolution in resolvableRelationships)
            {
                var resolvableRelationship = relationshipWithResolution.Item1;
                var sameConfigurationSource = relationshipWithResolution.Item2;
                var resolution = relationshipWithResolution.Item3;
                var inverseNavigationRemoved = relationshipWithResolution.Item4;
                if (sameConfigurationSource
                    && configurationSource == ConfigurationSource.Explicit
                    && inverseNavigationRemoved)
                {
                    var foreignKey = resolvableRelationship.Metadata;
                    ThrowForConflictingNavigation(
                        foreignKey,
                        principalEntityType, dependentEntityType,
                        navigationToDependent?.Name, navigationToPrincipal?.Name);
                }

                if (resolvableRelationship == newRelationshipBuilder)
                {
                    continue;
                }

                if (resolution.HasFlag(Resolution.Remove))
                {
                    removedForeignKeys.Add(resolvableRelationship.Metadata);
                    resolvableRelationship.Metadata.DeclaringEntityType.Builder.HasNoRelationship(
                        resolvableRelationship.Metadata, ConfigurationSource.Explicit);
                    continue;
                }

                if (resolution.HasFlag(Resolution.ResetToPrincipal))
                {
                    resolvableRelationship = resolvableRelationship.HasNavigations(
                        MemberIdentity.None, navigationToDependent: null, resolvableRelationship.Metadata.GetConfigurationSource());
                }

                if (resolution.HasFlag(Resolution.ResetToDependent))
                {
                    resolvableRelationship = resolvableRelationship.HasNavigations(
                        navigationToPrincipal: null, MemberIdentity.None, resolvableRelationship.Metadata.GetConfigurationSource());
                }

                if (resolvableRelationship.Metadata.Builder == null)
                {
                    continue;
                }

                var navigationlessForeignKey = resolvableRelationship.Metadata;
                if (navigationlessForeignKey.DependentToPrincipal == null
                    && navigationlessForeignKey.PrincipalToDependent == null
                    && navigationlessForeignKey.DeclaringEntityType.Builder.HasNoRelationship(
                        navigationlessForeignKey, ConfigurationSource.Convention)
                    != null)
                {
                    removedForeignKeys.Add(navigationlessForeignKey);
                }

                if (resolution.HasFlag(Resolution.ResetDependentProperties))
                {
                    var foreignKey = resolvableRelationship.Metadata;
                    resolvableRelationship.HasForeignKey((IReadOnlyList<Property>)null, foreignKey.GetConfigurationSource());
                }
            }

            if (newRelationshipBuilder == null)
            {
                var principalKey = principalProperties != null
                    ? principalEntityType.RootType().Builder.HasKey(principalProperties, configurationSource).Metadata
                    : principalEntityType.FindPrimaryKey();
                if (principalKey != null)
                {
                    if (oldNameDependentProperties != null
                        || (dependentProperties != null
                            && !ForeignKey.AreCompatible(
                                principalKey.Properties,
                                dependentProperties,
                                principalEntityType,
                                dependentEntityType,
                                shouldThrow: false)
                            && dependentProperties.All(
                                p => ConfigurationSource.Convention.Overrides(p.GetTypeConfigurationSource())
                                    && p.IsShadowProperty())))
                    {
                        dependentProperties = oldNameDependentProperties ?? dependentProperties;
                        if (principalKey.Properties.Count == dependentProperties.Count)
                        {
                            var detachedProperties = InternalEntityTypeBuilder.DetachProperties(dependentProperties);
                            dependentProperties = dependentEntityType.Builder.GetOrCreateProperties(
                                dependentProperties.Select(p => p.Name).ToList(),
                                ConfigurationSource.Convention,
                                principalKey.Properties,
                                isRequired ?? false);
                            detachedProperties.Attach(dependentEntityType.Builder);
                        }
                    }

                    if (dependentProperties != null
                        && !ForeignKey.AreCompatible(
                            principalKey.Properties,
                            dependentProperties,
                            principalEntityType,
                            dependentEntityType,
                            shouldThrow: false))
                    {
                        if (principalProperties == null)
                        {
                            principalKey = null;
                        }
                        else
                        {
                            dependentProperties = null;
                        }
                    }
                }

                newRelationshipBuilder = dependentEntityType.Builder.CreateForeignKey(
                    principalEntityType.Builder,
                    dependentProperties,
                    principalKey,
                    navigationToPrincipal?.Name,
                    isRequired,
                    ConfigurationSource.Convention);
            }

            foreach (var removedForeignKey in removedForeignKeys)
            {
                Metadata.DeclaringEntityType.Model.ConventionDispatcher.Tracker.Update(removedForeignKey, newRelationshipBuilder.Metadata);
            }

            if (tempIndex?.Builder != null)
            {
                dependentEntityType.RemoveIndex(tempIndex.Properties);
            }

            if (keyTempIndex?.Builder != null)
            {
                keyTempIndex.DeclaringEntityType.RemoveIndex(keyTempIndex.Properties);
            }

            return newRelationshipBuilder;
        }

        private InternalRelationshipBuilder FindCompatibleRelationship(
            EntityType principalEntityType,
            EntityType dependentEntityType,
            MemberIdentity? navigationToPrincipal,
            MemberIdentity? navigationToDependent,
            IReadOnlyList<Property> dependentProperties,
            IReadOnlyList<Property> principalProperties,
            ConfigurationSource? principalEndConfigurationSource,
            ConfigurationSource? configurationSource,
            out bool? existingRelationshipInverted,
            out bool conflictingRelationshipsFound,
            out List<(
                InternalRelationshipBuilder Builder,
                bool SameConfigurationSource,
                Resolution Resolution,
                bool InverseNavigationShouldBeRemoved)> resolvableRelationships)
        {
            existingRelationshipInverted = null;
            conflictingRelationshipsFound = false;
            resolvableRelationships = new List<(InternalRelationshipBuilder, bool, Resolution, bool)>();

            var matchingRelationships = FindRelationships(
                    principalEntityType,
                    dependentEntityType,
                    navigationToPrincipal,
                    navigationToDependent,
                    dependentProperties,
                    principalProperties ?? principalEntityType.FindPrimaryKey()?.Properties)
                .Where(r => r.Metadata != Metadata)
                .Distinct();

            var unresolvableRelationships = new List<InternalRelationshipBuilder>();
            foreach (var matchingRelationship in matchingRelationships)
            {
                var resolvable = true;
                bool? sameConfigurationSource = null;
                var inverseNavigationRemoved = false;
                var resolution = Resolution.None;
                var navigationToPrincipalName = navigationToPrincipal?.Name;
                var navigationToDependentName = navigationToDependent?.Name;
                if (navigationToPrincipalName != null)
                {
                    if ((navigationToPrincipalName == matchingRelationship.Metadata.DependentToPrincipal?.Name)
                        && dependentEntityType.IsSameHierarchy(matchingRelationship.Metadata.DeclaringEntityType))
                    {
                        if (matchingRelationship.CanRemoveNavigation(
                            pointsToPrincipal: true, configurationSource, overrideSameSource: false))
                        {
                            resolution |= Resolution.ResetToPrincipal;
                            sameConfigurationSource ??= false;
                        }
                        else if (matchingRelationship.CanRemoveNavigation(pointsToPrincipal: true, configurationSource)
                            // Don't remove derived bi-directional navigations
                            && (matchingRelationship.Metadata.GetConfigurationSource() == ConfigurationSource.Explicit
                                || navigationToDependentName != null
                                || matchingRelationship.Metadata.PrincipalToDependent == null
                                || !matchingRelationship.Metadata.DeclaringEntityType.IsStrictlyDerivedFrom(dependentEntityType)))
                        {
                            if (navigationToDependentName != null
                                && matchingRelationship.Metadata.PrincipalToDependent != null
                                && navigationToDependentName != matchingRelationship.Metadata.PrincipalToDependent.Name)
                            {
                                inverseNavigationRemoved = true;
                            }

                            resolution |= Resolution.ResetToPrincipal;
                            sameConfigurationSource = true;
                        }
                        else
                        {
                            resolvable = false;
                        }
                    }
                    else if ((navigationToPrincipalName == matchingRelationship.Metadata.PrincipalToDependent?.Name)
                        && dependentEntityType.IsSameHierarchy(matchingRelationship.Metadata.PrincipalEntityType))
                    {
                        if (matchingRelationship.CanRemoveNavigation(
                            pointsToPrincipal: false, configurationSource, overrideSameSource: false))
                        {
                            resolution |= Resolution.ResetToDependent;
                            sameConfigurationSource ??= false;
                        }
                        else if (matchingRelationship.CanRemoveNavigation(pointsToPrincipal: false, configurationSource)
                            // Don't remove derived bi-directional navigations
                            && (matchingRelationship.Metadata.GetConfigurationSource() == ConfigurationSource.Explicit
                                || navigationToDependentName != null
                                || matchingRelationship.Metadata.DependentToPrincipal == null
                                || !matchingRelationship.Metadata.PrincipalEntityType.IsStrictlyDerivedFrom(dependentEntityType)))
                        {
                            if (navigationToDependentName != null
                                && matchingRelationship.Metadata.DependentToPrincipal != null
                                && navigationToDependentName != matchingRelationship.Metadata.DependentToPrincipal.Name)
                            {
                                inverseNavigationRemoved = true;
                            }

                            resolution |= Resolution.ResetToDependent;
                            sameConfigurationSource = true;
                        }
                        else
                        {
                            resolvable = false;
                        }
                    }
                }

                if (navigationToDependentName != null)
                {
                    if ((navigationToDependentName == matchingRelationship.Metadata.PrincipalToDependent?.Name)
                        && principalEntityType.IsSameHierarchy(matchingRelationship.Metadata.PrincipalEntityType))
                    {
                        if (matchingRelationship.CanRemoveNavigation(
                            pointsToPrincipal: false, configurationSource, overrideSameSource: false))
                        {
                            resolution |= Resolution.ResetToDependent;
                            sameConfigurationSource ??= false;
                        }
                        else if (matchingRelationship.CanRemoveNavigation(pointsToPrincipal: false, configurationSource)
                            // Don't remove derived bi-directional navigations
                            && (matchingRelationship.Metadata.GetConfigurationSource() == ConfigurationSource.Explicit
                                || navigationToPrincipalName != null
                                || matchingRelationship.Metadata.DependentToPrincipal == null
                                || !matchingRelationship.Metadata.PrincipalEntityType.IsStrictlyDerivedFrom(principalEntityType)))
                        {
                            if (navigationToPrincipalName != null
                                && matchingRelationship.Metadata.DependentToPrincipal != null
                                && navigationToPrincipalName != matchingRelationship.Metadata.DependentToPrincipal.Name)
                            {
                                inverseNavigationRemoved = true;
                            }

                            resolution |= Resolution.ResetToDependent;
                            sameConfigurationSource = true;
                        }
                        else
                        {
                            resolvable = false;
                        }
                    }
                    else if ((navigationToDependentName == matchingRelationship.Metadata.DependentToPrincipal?.Name)
                        && principalEntityType.IsSameHierarchy(matchingRelationship.Metadata.DeclaringEntityType))
                    {
                        if (matchingRelationship.CanRemoveNavigation(
                            pointsToPrincipal: true, configurationSource, overrideSameSource: false))
                        {
                            resolution |= Resolution.ResetToPrincipal;
                            sameConfigurationSource ??= false;
                        }
                        else if (matchingRelationship.CanRemoveNavigation(pointsToPrincipal: true, configurationSource)
                            // Don't remove derived bi-directional navigations
                            && (matchingRelationship.Metadata.GetConfigurationSource() == ConfigurationSource.Explicit
                                || navigationToPrincipalName != null
                                || matchingRelationship.Metadata.PrincipalToDependent == null
                                || !matchingRelationship.Metadata.DeclaringEntityType.IsStrictlyDerivedFrom(principalEntityType)))
                        {
                            if (navigationToPrincipalName != null
                                && matchingRelationship.Metadata.PrincipalToDependent != null
                                && navigationToPrincipalName != matchingRelationship.Metadata.PrincipalToDependent.Name)
                            {
                                inverseNavigationRemoved = true;
                            }

                            resolution |= Resolution.ResetToPrincipal;
                            sameConfigurationSource = true;
                        }
                        else
                        {
                            resolvable = false;
                        }
                    }
                }

                if (dependentProperties != null
                    && matchingRelationship.Metadata.Properties.SequenceEqual(dependentProperties))
                {
                    if (matchingRelationship.CanSetForeignKey(
                        properties: null, dependentEntityType: null, configurationSource: configurationSource, overrideSameSource: false))
                    {
                        resolution |= Resolution.ResetDependentProperties;
                        sameConfigurationSource ??= false;
                    }
                    else if (matchingRelationship.CanSetForeignKey(properties: null, configurationSource))
                    {
                        resolution |= Resolution.ResetDependentProperties;
                        sameConfigurationSource = true;
                    }
                    else
                    {
                        resolvable = false;
                    }
                }

                if (resolvable)
                {
                    if ((sameConfigurationSource ?? true)
                        && configurationSource.HasValue
                        && matchingRelationship.Metadata.DeclaringEntityType.Builder
                            .CanRemoveForeignKey(matchingRelationship.Metadata, configurationSource.Value))
                    {
                        resolution |= Resolution.Remove;
                    }

                    resolvableRelationships.Add(
                        (matchingRelationship, sameConfigurationSource ?? true, resolution, inverseNavigationRemoved));
                }
                else
                {
                    unresolvableRelationships.Add(matchingRelationship);
                }
            }

            InternalRelationshipBuilder newRelationshipBuilder = null;

            var candidates = unresolvableRelationships.Concat(
                resolvableRelationships.Where(r => r.SameConfigurationSource).Concat(
                    resolvableRelationships.Where(r => !r.SameConfigurationSource))
                .Select(r => r.Builder));
            foreach (var candidateRelationship in candidates)
            {
                if (!candidateRelationship.CanSetRelatedTypes(
                    principalEntityType,
                    dependentEntityType,
                    strictPrincipal: principalEndConfigurationSource.HasValue
                    && principalEndConfigurationSource.Overrides(Metadata.GetPrincipalEndConfigurationSource()),
                    navigationToPrincipal,
                    navigationToDependent,
                    configurationSource,
                    shouldThrow: false,
                    out var candidateRelationshipInverted,
                    out var shouldResetToPrincipal,
                    out var shouldResetToDependent,
                    out _,
                    out _,
                    out _))
                {
                    continue;
                }

                if (configurationSource != ConfigurationSource.Explicit
                    && (shouldResetToPrincipal || shouldResetToDependent)
                    && (navigationToPrincipal == null
                        || navigationToPrincipal.Value.IsNone()
                        || navigationToDependent?.IsNone() != false)
                    && candidateRelationship.Metadata.DependentToPrincipal != null
                    && candidateRelationship.Metadata.PrincipalToDependent != null
                    && ((!candidateRelationshipInverted
                            && principalEntityType.IsAssignableFrom(candidateRelationship.Metadata.PrincipalEntityType)
                            && dependentEntityType.IsAssignableFrom(candidateRelationship.Metadata.DeclaringEntityType))
                        || (candidateRelationshipInverted
                            && principalEntityType.IsAssignableFrom(candidateRelationship.Metadata.DeclaringEntityType)
                            && dependentEntityType.IsAssignableFrom(candidateRelationship.Metadata.PrincipalEntityType))))
                {
                    // Favor derived bi-directional relationships over one-directional on base
                    continue;
                }

                if (dependentProperties != null
                    && !Property.AreCompatible(dependentProperties, candidateRelationship.Metadata.DeclaringEntityType))
                {
                    continue;
                }

                if (principalProperties != null
                    && !Property.AreCompatible(principalProperties, candidateRelationship.Metadata.PrincipalEntityType))
                {
                    continue;
                }

                existingRelationshipInverted = candidateRelationshipInverted;
                newRelationshipBuilder ??= candidateRelationship;
                break;
            }

            if (unresolvableRelationships.Any(r => r != newRelationshipBuilder))
            {
                conflictingRelationshipsFound = true;
                return null;
            }

            return newRelationshipBuilder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ThrowForConflictingNavigation([NotNull] IForeignKey foreignKey, [NotNull] string newInverseName, bool newToPrincipal)
            => ThrowForConflictingNavigation(
                foreignKey,
                foreignKey.PrincipalEntityType, foreignKey.DeclaringEntityType,
                newToPrincipal ? foreignKey.PrincipalToDependent?.Name : newInverseName,
                newToPrincipal ? newInverseName : foreignKey.DependentToPrincipal?.Name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ThrowForConflictingNavigation(
            [NotNull] IForeignKey foreignKey,
            [NotNull] IEntityType principalEntityType,
            [NotNull] IEntityType dependentEntityType,
            [CanBeNull] string navigationToDependent,
            [CanBeNull] string navigationToPrincipal)
            => throw new InvalidOperationException(
                CoreStrings.ConflictingRelationshipNavigation(
                    principalEntityType.DisplayName() + (navigationToDependent == null
                                                        ? ""
                                                        : "." + navigationToDependent),
                    dependentEntityType.DisplayName() + (navigationToPrincipal == null
                                                        ? ""
                                                        : "." + navigationToPrincipal),
                    foreignKey.PrincipalEntityType.DisplayName() + (foreignKey.PrincipalToDependent == null
                                                        ? ""
                                                        : "." + foreignKey.PrincipalToDependent.Name),
                    foreignKey.DeclaringEntityType.DisplayName() + (foreignKey.DependentToPrincipal == null
                                                        ? ""
                                                        : "." + foreignKey.DependentToPrincipal.Name)));

        private static IReadOnlyList<InternalRelationshipBuilder> FindRelationships(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            MemberIdentity? navigationToPrincipal,
            MemberIdentity? navigationToDependent,
            IReadOnlyList<Property> dependentProperties,
            IReadOnlyList<Property> principalProperties)
        {
            var existingRelationships = new List<InternalRelationshipBuilder>();
            if (navigationToPrincipal?.Name != null)
            {
                existingRelationships.AddRange(
                    dependentEntityType
                        .FindNavigationsInHierarchy(navigationToPrincipal.Value.Name)
                        .Select(n => n.ForeignKey.Builder));
            }

            if (navigationToDependent?.Name != null)
            {
                existingRelationships.AddRange(
                    principalEntityType
                        .FindNavigationsInHierarchy(navigationToDependent.Value.Name)
                        .Select(n => n.ForeignKey.Builder));
            }

            if (dependentProperties != null)
            {
                if (principalProperties != null)
                {
                    var principalKey = principalEntityType.FindKey(principalProperties);
                    if (principalKey != null)
                    {
                        existingRelationships.AddRange(
                            dependentEntityType
                                .FindForeignKeysInHierarchy(dependentProperties, principalKey, principalEntityType)
                                .Select(fk => fk.Builder));
                    }
                }
                else
                {
                    existingRelationships.AddRange(
                        dependentEntityType
                            .FindForeignKeysInHierarchy(dependentProperties)
                            .Where(fk => fk.PrincipalEntityType == principalEntityType)
                            .Select(fk => fk.Builder));
                }
            }

            return existingRelationships;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static InternalRelationshipBuilder FindCurrentRelationshipBuilder(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            MemberIdentity? navigationToPrincipal,
            MemberIdentity? navigationToDependent,
            [CanBeNull] IReadOnlyList<Property> dependentProperties,
            [CanBeNull] IReadOnlyList<Property> principalProperties)
        {
            InternalRelationshipBuilder currentRelationship = null;
            var matchingRelationships = FindRelationships(
                principalEntityType,
                dependentEntityType,
                navigationToPrincipal,
                navigationToDependent,
                dependentProperties,
                principalProperties).Distinct();

            foreach (var matchingRelationship in matchingRelationships)
            {
                if (!matchingRelationship.Metadata.DeclaringEntityType.IsAssignableFrom(dependentEntityType))
                {
                    continue;
                }

                if (!matchingRelationship.Metadata.PrincipalEntityType.IsAssignableFrom(principalEntityType))
                {
                    continue;
                }

                var matchingForeignKey = matchingRelationship.Metadata;
                var sameHierarchyInvertedNavigations =
                    principalEntityType.IsSameHierarchy(dependentEntityType)
                    && (navigationToPrincipal == null
                        || navigationToPrincipal.Value.Name == matchingForeignKey.PrincipalToDependent?.Name)
                    && (navigationToDependent == null
                        || navigationToDependent.Value.Name == matchingForeignKey.DependentToPrincipal?.Name);

                if (!sameHierarchyInvertedNavigations)
                {
                    if (navigationToPrincipal != null
                        && matchingForeignKey.DependentToPrincipal?.Name != navigationToPrincipal.Value.Name)
                    {
                        continue;
                    }

                    if (navigationToDependent != null
                        && matchingForeignKey.PrincipalToDependent?.Name != navigationToDependent.Value.Name)
                    {
                        continue;
                    }
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
                    // More than one match, ambiguity should be dealt with later
                    return null;
                }

                currentRelationship = matchingRelationship;
            }

            return currentRelationship;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder Attach([NotNull] InternalEntityTypeBuilder entityTypeBuilder)
        {
            var configurationSource = Metadata.GetConfigurationSource();
            var model = Metadata.DeclaringEntityType.Model;
            InternalEntityTypeBuilder principalEntityTypeBuilder;
            EntityType principalEntityType;
            if (Metadata.PrincipalEntityType.Builder != null)
            {
                principalEntityTypeBuilder = Metadata.PrincipalEntityType.Builder;
                principalEntityType = Metadata.PrincipalEntityType;
            }
            else
            {
                if (Metadata.PrincipalEntityType.Name == entityTypeBuilder.Metadata.Name)
                {
                    principalEntityTypeBuilder = entityTypeBuilder;
                    principalEntityType = entityTypeBuilder.Metadata;
                }
                else
                {
                    principalEntityType = model.FindEntityType(Metadata.PrincipalEntityType.Name);
                    if (principalEntityType == null)
                    {
                        if (model.EntityTypeShouldHaveDefiningNavigation(Metadata.PrincipalEntityType.Name)
                            && Metadata.PrincipalEntityType.HasDefiningNavigation())
                        {
                            principalEntityType = model.FindEntityType(
                                Metadata.PrincipalEntityType.Name,
                                Metadata.PrincipalEntityType.DefiningNavigationName,
                                Metadata.PrincipalEntityType.DefiningEntityType);
                            if (principalEntityType == null)
                            {
                                return null;
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }

                    principalEntityTypeBuilder = principalEntityType.Builder;
                }
            }

            if (!Metadata.GetConfigurationSource().Overrides(ConfigurationSource.Explicit)
                && principalEntityType.FindOwnership() != null
                && Metadata.DependentToPrincipal != null
                && !Metadata.IsOwnership)
            {
                // Only the owner can have a navigation to an owned type
                return null;
            }

            InternalEntityTypeBuilder dependentEntityTypeBuilder;
            EntityType dependentEntityType;
            if (Metadata.DeclaringEntityType.Builder != null)
            {
                dependentEntityTypeBuilder = Metadata.DeclaringEntityType.Builder;
                dependentEntityType = Metadata.DeclaringEntityType;
            }
            else
            {
                if (Metadata.DeclaringEntityType.Name == entityTypeBuilder.Metadata.Name
                    && (principalEntityType != entityTypeBuilder.Metadata
                        || !principalEntityType.HasDefiningNavigation()))
                {
                    dependentEntityTypeBuilder = entityTypeBuilder;
                    dependentEntityType = entityTypeBuilder.Metadata;
                }
                else
                {
                    dependentEntityType = model.FindEntityType(Metadata.DeclaringEntityType.Name);
                    if (dependentEntityType == null)
                    {
                        using (ModelBuilder.Metadata.ConventionDispatcher.DelayConventions())
                        {
                            if (model.EntityTypeShouldHaveDefiningNavigation(Metadata.DeclaringEntityType.Name))
                            {
                                if (Metadata.DeclaringEntityType.HasDefiningNavigation())
                                {
                                    dependentEntityType = model.FindEntityType(
                                        Metadata.DeclaringEntityType.Name,
                                        Metadata.DeclaringEntityType.DefiningNavigationName,
                                        Metadata.DeclaringEntityType.DefiningEntityType);
                                }

                                if (dependentEntityType == null)
                                {
                                    if (Metadata.IsOwnership
                                        && Metadata.PrincipalToDependent != null)
                                    {
                                        if (model.HasOtherEntityTypesWithDefiningNavigation(Metadata.DeclaringEntityType))
                                        {
                                            dependentEntityType = Metadata.DeclaringEntityType.ClrType == null
                                                ? model.AddEntityType(
                                                    Metadata.DeclaringEntityType.Name,
                                                    Metadata.PrincipalToDependent.Name,
                                                    principalEntityType,
                                                    configurationSource)
                                                : model.AddEntityType(
                                                    Metadata.DeclaringEntityType.ClrType,
                                                    Metadata.PrincipalToDependent.Name,
                                                    principalEntityType,
                                                    configurationSource);
                                        }
                                        else
                                        {
                                            dependentEntityType = Metadata.DeclaringEntityType.ClrType == null
                                                ? model.Builder.Entity(Metadata.DeclaringEntityType.Name, configurationSource).Metadata
                                                : model.Builder.Entity(Metadata.DeclaringEntityType.ClrType, configurationSource).Metadata;
                                        }
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                            }
                            else
                            {
                                dependentEntityType = Metadata.DeclaringEntityType.ClrType == null
                                    ? model.AddEntityType(Metadata.DeclaringEntityType.Name, configurationSource)
                                    : model.AddEntityType(Metadata.DeclaringEntityType.ClrType, configurationSource);
                            }
                        }
                    }

                    dependentEntityTypeBuilder = dependentEntityType.Builder;
                }
            }

            if (!Metadata.GetConfigurationSource().Overrides(ConfigurationSource.Explicit)
                && ((dependentEntityType.HasDefiningNavigation()
                        && (Metadata.PrincipalToDependent?.Name != dependentEntityType.DefiningNavigationName
                            || principalEntityType != dependentEntityType.DefiningEntityType))
                    || (dependentEntityType.FindOwnership() != null
                        && Metadata.PrincipalToDependent != null)))
            {
                return null;
            }

            if (dependentEntityType.GetForeignKeys().Contains(Metadata, ReferenceEqualityComparer.Instance))
            {
                Check.DebugAssert(Metadata.Builder != null, "Metadata.Builder is null");

                return Metadata.Builder;
            }

            IReadOnlyList<Property> dependentProperties;
            IReadOnlyList<Property> principalProperties;
            if (Metadata.GetPrincipalKeyConfigurationSource()?.Overrides(configurationSource) != true)
            {
                principalProperties = new List<Property>();
            }
            else
            {
                principalProperties = principalEntityTypeBuilder.GetActualProperties(Metadata.PrincipalKey.Properties, configurationSource)
                    ?? new List<Property>();
            }

            if ((principalProperties.Count == 0
                    && Metadata.GetPropertiesConfigurationSource()?.Overrides(ConfigurationSource.Explicit) != true)
                || Metadata.GetPropertiesConfigurationSource()?.Overrides(configurationSource) != true)
            {
                dependentProperties = new List<Property>();
            }
            else
            {
                dependentProperties = dependentEntityTypeBuilder.GetActualProperties(Metadata.Properties, configurationSource)
                    ?? new List<Property>();
            }

            if (dependentProperties.Count != 0)
            {
                if (!CanSetForeignKey(
                    dependentProperties,
                    dependentEntityType,
                    principalProperties.Count != 0 ? principalProperties : Metadata.PrincipalKey.Properties,
                    principalEntityType,
                    configurationSource,
                    out var resetPrincipalKey))
                {
                    dependentProperties = new List<Property>();
                }
                else if (resetPrincipalKey)
                {
                    principalProperties = new List<Property>();
                }
            }

            return ReplaceForeignKey(
                configurationSource,
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                dependentProperties: dependentProperties,
                principalProperties: principalProperties);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool AreCompatible(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            [CanBeNull] MemberInfo navigationToPrincipal,
            [CanBeNull] MemberInfo navigationToDependent,
            [CanBeNull] IReadOnlyList<Property> dependentProperties,
            [CanBeNull] IReadOnlyList<Property> principalProperties,
            bool? isUnique,
            ConfigurationSource? configurationSource)
            => ForeignKey.AreCompatible(
                principalEntityType,
                dependentEntityType,
                navigationToPrincipal,
                navigationToDependent,
                dependentProperties,
                principalProperties,
                isUnique,
                configurationSource == ConfigurationSource.Explicit);

        private bool CanSetRelatedTypes(
            EntityType principalEntityType,
            EntityType dependentEntityType,
            bool strictPrincipal,
            MemberIdentity? navigationToPrincipal,
            MemberIdentity? navigationToDependent,
            ConfigurationSource? configurationSource,
            bool shouldThrow,
            out bool shouldInvert,
            out bool shouldResetToPrincipal,
            out bool shouldResetToDependent,
            out bool shouldResetPrincipalProperties,
            out bool shouldResetDependentProperties,
            out bool? shouldBeUnique)
        {
            shouldInvert = false;
            shouldResetToPrincipal = false;
            shouldResetToDependent = false;
            shouldResetPrincipalProperties = false;
            shouldResetDependentProperties = false;
            shouldBeUnique = null;

            var sameHierarchyInvertedNavigations =
                principalEntityType.IsSameHierarchy(dependentEntityType)
                && (((navigationToPrincipal != null)
                        && (navigationToPrincipal.Value.Name == Metadata.PrincipalToDependent?.Name))
                    || ((navigationToDependent != null)
                        && (navigationToDependent.Value.Name == Metadata.DependentToPrincipal?.Name)));

            var someAspectsFitNonInverted = false;
            if (!sameHierarchyInvertedNavigations
                && CanSetRelatedTypes(
                    principalEntityType,
                    dependentEntityType,
                    navigationToPrincipal,
                    navigationToDependent,
                    configurationSource,
                    inverted: false,
                    shouldThrow: false,
                    out shouldResetToPrincipal,
                    out shouldResetToDependent,
                    out shouldResetPrincipalProperties,
                    out shouldResetDependentProperties,
                    out shouldBeUnique))
            {
                if (!shouldResetToPrincipal
                    && !shouldResetToDependent)
                {
                    return true;
                }

                someAspectsFitNonInverted = true;
            }

            var canInvert = configurationSource?.Overrides(Metadata.GetPrincipalEndConfigurationSource()) == true;
            if ((!strictPrincipal
                    || canInvert)
                && CanSetRelatedTypes(
                    dependentEntityType,
                    principalEntityType,
                    navigationToDependent,
                    navigationToPrincipal,
                    configurationSource,
                    strictPrincipal,
                    shouldThrow: false,
                    out var invertedShouldResetToPrincipal,
                    out var invertedShouldResetToDependent,
                    out _,
                    out _,
                    out var invertedShouldBeUnique)
                && (!someAspectsFitNonInverted
                    || (!invertedShouldResetToPrincipal
                        && !invertedShouldResetToDependent)))
            {
                shouldInvert = true;
                shouldResetToPrincipal = invertedShouldResetToDependent;
                shouldResetToDependent = invertedShouldResetToPrincipal;
                shouldBeUnique = invertedShouldBeUnique;
                return true;
            }

            if (!someAspectsFitNonInverted
                && canInvert
                && shouldThrow)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypesNotInRelationship(
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
            MemberIdentity? navigationToPrincipal,
            MemberIdentity? navigationToDependent,
            ConfigurationSource? configurationSource,
            bool inverted,
            bool shouldThrow,
            out bool shouldResetToPrincipal,
            out bool shouldResetToDependent,
            out bool shouldResetPrincipalProperties,
            out bool shouldResetDependentProperties,
            out bool? shouldBeUnique)
        {
            shouldResetToPrincipal = false;
            shouldResetToDependent = false;
            shouldResetPrincipalProperties = false;
            shouldResetDependentProperties = false;
            shouldBeUnique = null;

            if (!Metadata.DeclaringEntityType.IsSameHierarchy(dependentEntityType))
            {
                return false;
            }

            if (!Metadata.PrincipalEntityType.IsSameHierarchy(principalEntityType))
            {
                return false;
            }

            if (navigationToPrincipal != null)
            {
                if (!configurationSource.HasValue
                    || !CanSetNavigation(
                        navigationToPrincipal.Value,
                        pointsToPrincipal: true,
                        configurationSource.Value,
                        shouldThrow,
                        shouldBeUnique: out _,
                        removeOppositeNavigation: out _,
                        removeConflictingNavigations: out _))
                {
                    return false;
                }

                if (Metadata.DependentToPrincipal != null
                    && navigationToPrincipal.Value.Name != Metadata.DependentToPrincipal.Name)
                {
                    shouldResetToPrincipal = true;
                }
            }
            else
            {
                bool? invertedShouldBeUnique = null;
                var navigationToPrincipalProperty = Metadata.DependentToPrincipal?.GetIdentifyingMemberInfo();
                if (navigationToPrincipalProperty != null
                    && !IsCompatible(
                        navigationToPrincipalProperty,
                        !inverted,
                        inverted ? principalEntityType.ClrType : dependentEntityType.ClrType,
                        inverted ? dependentEntityType.ClrType : principalEntityType.ClrType,
                        shouldThrow,
                        out invertedShouldBeUnique))
                {
                    if (!configurationSource.HasValue
                        || !CanSetNavigation((string)null, pointsToPrincipal: true, configurationSource.Value))
                    {
                        return false;
                    }

                    shouldResetToPrincipal = true;
                }

                if (inverted)
                {
                    shouldBeUnique = invertedShouldBeUnique;
                }
            }

            if (navigationToDependent != null)
            {
                if (!configurationSource.HasValue
                    || !CanSetNavigation(
                        navigationToDependent.Value,
                        pointsToPrincipal: false,
                        configurationSource.Value,
                        shouldThrow,
                        out var toDependentShouldBeUnique,
                        removeOppositeNavigation: out _,
                        removeConflictingNavigations: out _))
                {
                    return false;
                }

                if (Metadata.PrincipalToDependent != null
                    && navigationToDependent.Value.Name != Metadata.PrincipalToDependent.Name)
                {
                    shouldResetToDependent = true;
                }

                if (toDependentShouldBeUnique != null)
                {
                    shouldBeUnique = toDependentShouldBeUnique;
                }
            }
            else
            {
                bool? toDependentShouldBeUnique = null;
                var navigationToDependentProperty = Metadata.PrincipalToDependent?.GetIdentifyingMemberInfo();
                if (navigationToDependentProperty != null
                    && !IsCompatible(
                        navigationToDependentProperty,
                        inverted,
                        inverted ? principalEntityType.ClrType : dependentEntityType.ClrType,
                        inverted ? dependentEntityType.ClrType : principalEntityType.ClrType,
                        shouldThrow,
                        out toDependentShouldBeUnique))
                {
                    if (!configurationSource.HasValue
                        || !CanSetNavigation((string)null, pointsToPrincipal: false, configurationSource.Value))
                    {
                        return false;
                    }

                    shouldResetToDependent = true;
                }

                if (!inverted
                    && toDependentShouldBeUnique != null)
                {
                    shouldBeUnique = toDependentShouldBeUnique;
                }
            }

            if (shouldBeUnique.HasValue
                && !CanSetIsUnique(shouldBeUnique.Value, configurationSource, out _))
            {
                return false;
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
                    || !configurationSource.Value.Overrides(Metadata.GetPropertiesConfigurationSource()))
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionForeignKey IConventionRelationshipBuilder.Metadata
        {
            [DebuggerStepThrough] get => Metadata;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionRelationshipBuilder IConventionRelationshipBuilder.HasEntityTypes(
            IConventionEntityType principalEntityType, IConventionEntityType dependentEntityType, bool fromDataAnnotation)
            => HasEntityTypes(
                (EntityType)principalEntityType, (EntityType)dependentEntityType,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionRelationshipBuilder.CanInvert(IReadOnlyList<IConventionProperty> newForeignKeyProperties, bool fromDataAnnotation)
            => CanInvert(
                (IReadOnlyList<Property>)newForeignKeyProperties,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionRelationshipBuilder IConventionRelationshipBuilder.HasForeignKey(
            [CanBeNull] IReadOnlyList<string> properties, bool fromDataAnnotation)
            => HasForeignKey(
                properties,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionRelationshipBuilder IConventionRelationshipBuilder.HasForeignKey(
            IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation)
            => HasForeignKey(
                properties as IReadOnlyList<Property> ?? properties?.Cast<Property>().ToList(),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionRelationshipBuilder.CanSetForeignKey(
            [CanBeNull] IReadOnlyList<string> properties, bool fromDataAnnotation)
            => CanSetForeignKey(
                properties,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionRelationshipBuilder.CanSetForeignKey(IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation)
            => CanSetForeignKey(
                properties as IReadOnlyList<Property> ?? properties?.Cast<Property>().ToList(),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionRelationshipBuilder IConventionRelationshipBuilder.HasPrincipalKey(
            [CanBeNull] IReadOnlyList<string> properties, bool fromDataAnnotation)
            => HasPrincipalKey(
                properties,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionRelationshipBuilder IConventionRelationshipBuilder.HasPrincipalKey(
            IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation)
            => HasPrincipalKey(
                properties as IReadOnlyList<Property> ?? properties?.Cast<Property>().ToList(),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionRelationshipBuilder.CanSetPrincipalKey(
            [CanBeNull] IReadOnlyList<string> properties, bool fromDataAnnotation)
            => CanSetPrincipalKey(
                properties,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionRelationshipBuilder.CanSetPrincipalKey(IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation)
            => CanSetPrincipalKey(
                properties as IReadOnlyList<Property> ?? properties?.Cast<Property>().ToList(),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionRelationshipBuilder IConventionRelationshipBuilder.HasNavigation(
            string name, bool pointsToPrincipal, bool fromDataAnnotation)
            => HasNavigation(
                name, pointsToPrincipal,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionRelationshipBuilder IConventionRelationshipBuilder.HasNavigation(
            MemberInfo property, bool pointsToPrincipal, bool fromDataAnnotation)
            => HasNavigation(
                property, pointsToPrincipal,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionRelationshipBuilder IConventionRelationshipBuilder.HasNavigations(
            string navigationToPrincipalName, string navigationToDependentName, bool fromDataAnnotation)
            => HasNavigations(
                navigationToPrincipalName, navigationToDependentName,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionRelationshipBuilder IConventionRelationshipBuilder.HasNavigations(
            MemberInfo navigationToPrincipal, MemberInfo navigationToDependent, bool fromDataAnnotation)
            => HasNavigations(
                navigationToPrincipal, navigationToDependent,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionRelationshipBuilder.CanSetNavigation(
            MemberInfo property, bool pointsToPrincipal, bool fromDataAnnotation)
            => CanSetNavigation(
                property, pointsToPrincipal,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionRelationshipBuilder.CanSetNavigation(string name, bool pointsToPrincipal, bool fromDataAnnotation)
            => CanSetNavigation(
                name, pointsToPrincipal,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionRelationshipBuilder.CanSetNavigations(
            MemberInfo navigationToPrincipal, MemberInfo navigationToDependent, bool fromDataAnnotation)
            => CanSetNavigations(
                navigationToPrincipal, navigationToDependent,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionRelationshipBuilder IConventionRelationshipBuilder.HasField(
            string fieldName, bool pointsToPrincipal, bool fromDataAnnotation)
            => HasField(
                fieldName, pointsToPrincipal, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionRelationshipBuilder IConventionRelationshipBuilder.HasField(
            FieldInfo fieldInfo, bool pointsToPrincipal, bool fromDataAnnotation)
            => HasField(
                fieldInfo, pointsToPrincipal, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionRelationshipBuilder.CanSetField(string fieldName, bool pointsToPrincipal, bool fromDataAnnotation)
            => CanSetField(
                fieldName, pointsToPrincipal, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionRelationshipBuilder.CanSetField(FieldInfo fieldInfo, bool pointsToPrincipal, bool fromDataAnnotation)
            => CanSetField(
                fieldInfo, pointsToPrincipal, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionRelationshipBuilder IConventionRelationshipBuilder.UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode, bool pointsToPrincipal, bool fromDataAnnotation)
            => UsePropertyAccessMode(
                propertyAccessMode,
                pointsToPrincipal,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionRelationshipBuilder.CanSetPropertyAccessMode(
            PropertyAccessMode? propertyAccessMode, bool pointsToPrincipal, bool fromDataAnnotation)
            => CanSetPropertyAccessMode(
                propertyAccessMode,
                pointsToPrincipal,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionRelationshipBuilder IConventionRelationshipBuilder.IsEagerLoaded(
            bool? eagerLoaded, bool pointsToPrincipal, bool fromDataAnnotation)
            => IsEagerLoaded(
                eagerLoaded, pointsToPrincipal, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionRelationshipBuilder.CanSetIsEagerLoaded(bool? eagerLoaded, bool pointsToPrincipal, bool fromDataAnnotation)
            => CanSetIsEagerLoaded(
                eagerLoaded, pointsToPrincipal, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionRelationshipBuilder.CanSetNavigations(
            string navigationToPrincipalName, string navigationToDependentName, bool fromDataAnnotation)
            => CanSetNavigations(
                navigationToPrincipalName, navigationToDependentName,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionRelationshipBuilder IConventionRelationshipBuilder.IsRequired(bool? required, bool fromDataAnnotation)
            => IsRequired(required, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionRelationshipBuilder.CanSetIsRequired(bool? required, bool fromDataAnnotation)
            => CanSetIsRequired(required, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionRelationshipBuilder IConventionRelationshipBuilder.IsOwnership(bool? ownership, bool fromDataAnnotation)
            => IsOwnership(ownership, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionRelationshipBuilder.CanSetIsOwnership(bool? ownership, bool fromDataAnnotation)
            => CanSetIsOwnership(ownership, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionRelationshipBuilder IConventionRelationshipBuilder.OnDelete(
            DeleteBehavior? deleteBehavior, bool fromDataAnnotation)
            => OnDelete(deleteBehavior, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionRelationshipBuilder.CanSetOnDelete(DeleteBehavior? deleteBehavior, bool fromDataAnnotation)
            => CanSetDeleteBehavior(
                deleteBehavior, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionRelationshipBuilder IConventionRelationshipBuilder.IsUnique(bool? unique, bool fromDataAnnotation)
            => IsUnique(unique, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionRelationshipBuilder.CanSetIsUnique(bool? unique, bool fromDataAnnotation)
            => CanSetIsUnique(unique, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }
}
