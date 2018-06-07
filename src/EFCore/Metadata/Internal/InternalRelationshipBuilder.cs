// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [DebuggerDisplay("{Metadata,nq}")]
    // Issue#11266 This type is being used by provider code. Do not break.
    public class InternalRelationshipBuilder : InternalMetadataItemBuilder<ForeignKey>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalRelationshipBuilder(
            [NotNull] ForeignKey foreignKey,
            [NotNull] InternalModelBuilder modelBuilder)
            : base(foreignKey, modelBuilder)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder DependentToPrincipal(
            [CanBeNull] string name,
            ConfigurationSource configurationSource)
            => Navigations(
                PropertyIdentity.Create(name),
                null,
                configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder DependentToPrincipal(
            [CanBeNull] MemberInfo property,
            ConfigurationSource configurationSource)
            => Navigations(
                PropertyIdentity.Create(property),
                null,
                configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder PrincipalToDependent(
            [CanBeNull] string name,
            ConfigurationSource configurationSource)
            => Navigations(
                null,
                PropertyIdentity.Create(name),
                configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder PrincipalToDependent(
            [CanBeNull] MemberInfo property,
            ConfigurationSource configurationSource)
            => Navigations(
                null,
                PropertyIdentity.Create(property),
                configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Navigations(
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            ConfigurationSource configurationSource)
            => Navigations(
                PropertyIdentity.Create(navigationToPrincipalName),
                PropertyIdentity.Create(navigationToDependentName),
                configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Navigations(
            [CanBeNull] MemberInfo navigationToPrincipalProperty,
            [CanBeNull] MemberInfo navigationToDependentProperty,
            ConfigurationSource configurationSource)
            => Navigations(
                PropertyIdentity.Create(navigationToPrincipalProperty),
                PropertyIdentity.Create(navigationToDependentProperty),
                configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Navigations(
            PropertyIdentity? navigationToPrincipal,
            PropertyIdentity? navigationToDependent,
            ConfigurationSource? configurationSource)
            => Navigations(
                navigationToPrincipal,
                navigationToDependent,
                Metadata.PrincipalEntityType,
                Metadata.DeclaringEntityType,
                configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Navigations(
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            ConfigurationSource configurationSource)
            => Navigations(
                PropertyIdentity.Create(navigationToPrincipalName),
                PropertyIdentity.Create(navigationToDependentName),
                principalEntityType,
                dependentEntityType,
                configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Navigations(
            [CanBeNull] MemberInfo navigationToPrincipalProperty,
            [CanBeNull] MemberInfo navigationToDependentProperty,
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            ConfigurationSource configurationSource)
            => Navigations(
                PropertyIdentity.Create(navigationToPrincipalProperty),
                PropertyIdentity.Create(navigationToDependentProperty),
                principalEntityType,
                dependentEntityType,
                configurationSource);

        private InternalRelationshipBuilder Navigations(
            PropertyIdentity? navigationToPrincipal,
            PropertyIdentity? navigationToDependent,
            EntityType principalEntityType,
            EntityType dependentEntityType,
            ConfigurationSource? configurationSource)
        {
            var shouldThrow = configurationSource == ConfigurationSource.Explicit;

            var navigationToPrincipalName = navigationToPrincipal?.Name;
            if (navigationToPrincipalName != null
                && navigationToPrincipal.Value.Property == null
                && dependentEntityType.HasClrType())
            {
                var navigationProperty = Navigation.GetClrMember(navigationToPrincipalName, dependentEntityType, principalEntityType, shouldThrow);
                if (navigationProperty != null)
                {
                    navigationToPrincipal = PropertyIdentity.Create(navigationProperty);
                }
            }

            var navigationToDependentName = navigationToDependent?.Name;
            if (navigationToDependentName != null
                && navigationToDependent.Value.Property == null
                && principalEntityType.HasClrType())
            {
                var navigationProperty = Navigation.GetClrMember(navigationToDependentName, principalEntityType, dependentEntityType, shouldThrow);
                if (navigationProperty != null)
                {
                    navigationToDependent = PropertyIdentity.Create(navigationProperty);
                }
            }

            if (!CanSetNavigations(
                navigationToPrincipal,
                navigationToDependent,
                principalEntityType,
                dependentEntityType,
                configurationSource,
                shouldThrow,
                true,
                out var shouldInvert,
                out var shouldBeUnique,
                out var removeOppositeNavigation,
                out var removeConflictingNavigations))
            {
                return null;
            }

            if (removeOppositeNavigation)
            {
                if (navigationToPrincipal == null)
                {
                    navigationToPrincipal = PropertyIdentity.None;
                }

                if (navigationToDependent == null)
                {
                    navigationToDependent = PropertyIdentity.None;
                }
            }

            if ((navigationToPrincipal == null
                 || navigationToPrincipal.Value.Name == Metadata.DependentToPrincipal?.Name)
                && (navigationToDependent == null
                    || navigationToDependent.Value.Name == Metadata.PrincipalToDependent?.Name))
            {
                if (configurationSource.HasValue)
                {
                    Metadata.UpdateConfigurationSource(configurationSource.Value);
                    if (navigationToPrincipal != null)
                    {
                        Metadata.UpdateDependentToPrincipalConfigurationSource(configurationSource);
                        if (navigationToPrincipalName != null)
                        {
                            principalEntityType.Unignore(navigationToPrincipalName);
                        }
                    }

                    if (navigationToDependent != null)
                    {
                        Metadata.UpdatePrincipalToDependentConfigurationSource(configurationSource);
                        if (navigationToDependentName != null)
                        {
                            principalEntityType.Unignore(navigationToDependentName);
                        }
                    }
                }
                return this;
            }

            Debug.Assert(configurationSource.HasValue);

            IReadOnlyList<Property> dependentProperties = null;
            IReadOnlyList<Property> principalProperties = null;
            if (shouldInvert == true)
            {
                Debug.Assert(configurationSource.Value.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource()));
                Debug.Assert(configurationSource.Value.Overrides(Metadata.GetPrincipalKeyConfigurationSource()));

                var entityType = principalEntityType;
                principalEntityType = dependentEntityType;
                dependentEntityType = entityType;

                var navigation = navigationToPrincipal;
                navigationToPrincipal = navigationToDependent;
                navigationToDependent = navigation;

                if (Metadata.GetForeignKeyPropertiesConfigurationSource() == configurationSource.Value)
                {
                    dependentProperties = Array.Empty<Property>();
                }

                if (Metadata.GetPrincipalKeyConfigurationSource() == configurationSource.Value)
                {
                    principalProperties = Array.Empty<Property>();
                }
            }

            InternalRelationshipBuilder builder;
            if (shouldInvert == true
                || removeConflictingNavigations)
            {
                builder = ReplaceForeignKey(
                    configurationSource,
                    principalEntityTypeBuilder: principalEntityType.Builder,
                    dependentEntityTypeBuilder: dependentEntityType.Builder,
                    navigationToPrincipal: navigationToPrincipal,
                    navigationToDependent: navigationToDependent,
                    dependentProperties: dependentProperties,
                    principalProperties: principalProperties,
                    isUnique: shouldBeUnique,
                    removeCurrent: shouldInvert ?? false,
                    principalEndConfigurationSource: shouldInvert != null ? configurationSource : null,
                    oldRelationshipInverted: shouldInvert == true);

                if (builder == null)
                {
                    return null;
                }

                Debug.Assert(builder.Metadata.Builder != null);
            }
            else
            {
                using (var batch = Metadata.DeclaringEntityType.Model.ConventionDispatcher.StartBatch())
                {
                    builder = this;
                    Metadata.UpdateConfigurationSource(configurationSource.Value);
                    if (shouldBeUnique.HasValue)
                    {
                        IsUnique(shouldBeUnique.Value, configurationSource.Value);
                    }

                    if (navigationToPrincipal != null)
                    {
                        if (navigationToDependent != null)
                        {
                            Metadata.HasPrincipalToDependent((string)null, configurationSource.Value);
                        }

                        var navigationProperty = navigationToPrincipal.Value.Property;
                        if (navigationToPrincipalName != null)
                        {
                            Metadata.DeclaringEntityType.Unignore(navigationToPrincipalName);

                            if (Metadata.DeclaringEntityType.ClrType != null
                                && navigationProperty == null)
                            {
                                throw new InvalidOperationException(
                                    CoreStrings.NoClrNavigation(navigationToPrincipalName, Metadata.DeclaringEntityType.DisplayName()));
                            }
                        }

                        if (navigationProperty != null)
                        {
                            Metadata.HasDependentToPrincipal(navigationProperty, configurationSource.Value);
                        }
                        else
                        {
                            Metadata.HasDependentToPrincipal(navigationToPrincipalName, configurationSource.Value);
                        }
                    }

                    if (navigationToDependent != null)
                    {
                        var navigationProperty = navigationToDependent.Value.Property;
                        if (navigationToDependentName != null)
                        {
                            Metadata.PrincipalEntityType.Unignore(navigationToDependentName);

                            if (Metadata.DeclaringEntityType.ClrType != null
                                && navigationProperty == null)
                            {
                                throw new InvalidOperationException(
                                    CoreStrings.NoClrNavigation(navigationToDependentName, Metadata.PrincipalEntityType.DisplayName()));
                            }
                        }

                        if (navigationProperty != null)
                        {
                            Metadata.HasPrincipalToDependent(navigationProperty, configurationSource.Value);
                        }
                        else
                        {
                            Metadata.HasPrincipalToDependent(navigationToDependentName, configurationSource.Value);
                        }
                    }

                    builder = batch.Run(builder);
                }
            }

            if (builder != null
                && ((navigationToPrincipal != null
                     && builder.Metadata.DependentToPrincipal?.Name != navigationToPrincipal.Value.Name)
                    || (navigationToDependent != null
                        && builder.Metadata.PrincipalToDependent?.Name != navigationToDependent.Value.Name))
                && ((navigationToDependent != null
                     && builder.Metadata.DependentToPrincipal?.Name != navigationToDependent.Value.Name)
                    || (navigationToPrincipal != null
                        && builder.Metadata.PrincipalToDependent?.Name != navigationToPrincipal.Value.Name)))
            {
                return null;
            }

            return builder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool CanSetNavigation(
            [CanBeNull] string navigationName,
            bool pointsToPrincipal,
            ConfigurationSource? configurationSource,
            bool overrideSameSource = true)
        {
            PropertyIdentity navigation;
            var sourceType = pointsToPrincipal ? Metadata.DeclaringEntityType : Metadata.PrincipalEntityType;
            if (navigationName == null
                || !sourceType.HasClrType())
            {
                navigation = PropertyIdentity.Create(navigationName);
            }
            else
            {
                var navigationProperty = Navigation.GetClrMember(
                    navigationName,
                    sourceType,
                    pointsToPrincipal ? Metadata.PrincipalEntityType : Metadata.DeclaringEntityType,
                    shouldThrow: configurationSource == ConfigurationSource.Explicit);
                if (navigationProperty == null)
                {
                    return false;
                }
                navigation = PropertyIdentity.Create(navigationProperty);
            }

            return CanSetNavigation(
                navigation,
                pointsToPrincipal,
                configurationSource,
                false,
                overrideSameSource,
                out _,
                out _,
                out _);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool CanSetNavigation(
            [CanBeNull] PropertyInfo navigationProperty,
            bool pointsToPrincipal,
            ConfigurationSource? configurationSource,
            bool overrideSameSource = true)
            => CanSetNavigation(
                PropertyIdentity.Create(navigationProperty),
                pointsToPrincipal,
                configurationSource,
                false,
                overrideSameSource,
                out _,
                out _,
                out _);

        private bool CanSetNavigation(
            PropertyIdentity navigation,
            bool pointsToPrincipal,
            ConfigurationSource? configurationSource,
            bool shouldThrow,
            bool overrideSameSource,
            out bool? shouldBeUnique,
            out bool removeOppositeNavigation,
            out bool removeConflictingNavigations)
            => pointsToPrincipal
                ? CanSetNavigations(
                    navigation,
                    null,
                    configurationSource,
                    shouldThrow,
                    overrideSameSource,
                    out _,
                    out shouldBeUnique,
                    out removeOppositeNavigation,
                    out removeConflictingNavigations)
                : CanSetNavigations(
                    null,
                    navigation,
                    configurationSource,
                    shouldThrow,
                    overrideSameSource,
                    out _,
                    out shouldBeUnique,
                    out removeOppositeNavigation,
                    out removeConflictingNavigations);

        private bool CanSetNavigations(
            PropertyIdentity? navigationToPrincipal,
            PropertyIdentity? navigationToDependent,
            ConfigurationSource? configurationSource,
            bool shouldThrow,
            bool overrideSameSource,
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
                overrideSameSource,
                out shouldInvert,
                out shouldBeUnique,
                out removeOppositeNavigation,
                out removeConflictingNavigations);

        private bool CanSetNavigations(
            PropertyIdentity? navigationToPrincipal,
            PropertyIdentity? navigationToDependent,
            EntityType principalEntityType,
            EntityType dependentEntityType,
            ConfigurationSource? configurationSource,
            bool shouldThrow,
            bool overrideSameSource,
            out bool? shouldInvert,
            out bool? shouldBeUnique,
            out bool removeOppositeNavigation,
            out bool removeConflictingNavigations)
        {
            shouldInvert = null;
            shouldBeUnique = null;
            removeOppositeNavigation = false;
            removeConflictingNavigations = false;

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
                if (!configurationSource.Value.Overrides(Metadata.GetDependentToPrincipalConfigurationSource())
                    || (!overrideSameSource && configurationSource == Metadata.GetDependentToPrincipalConfigurationSource()))
                {
                    return false;
                }

                if (navigationToPrincipalName != null)
                {
                    if (dependentEntityType.Builder.IsIgnored(navigationToPrincipalName, configurationSource))
                    {
                        return false;
                    }

                    if (navigationToDependent == null
                        && navigationToPrincipalName == Metadata.PrincipalToDependent?.Name
                        && Metadata.IsIntraHierarchical())
                    {
                        if (!configurationSource.Value.Overrides(Metadata.GetPrincipalToDependentConfigurationSource())
                            || (!overrideSameSource && configurationSource == Metadata.GetPrincipalToDependentConfigurationSource()))
                        {
                            return false;
                        }

                        removeOppositeNavigation = true;
                    }
                }
            }

            var navigationToDependentName = navigationToDependent?.Name;
            if (navigationToDependent != null
                && navigationToDependentName != Metadata.PrincipalToDependent?.Name)
            {
                if (!configurationSource.Value.Overrides(Metadata.GetPrincipalToDependentConfigurationSource())
                    || (!overrideSameSource && configurationSource == Metadata.GetPrincipalToDependentConfigurationSource()))
                {
                    return false;
                }

                if (navigationToDependentName != null)
                {
                    if (principalEntityType.Builder.IsIgnored(navigationToDependentName, configurationSource))
                    {
                        return false;
                    }

                    if (navigationToPrincipal == null
                        && navigationToDependentName == Metadata.DependentToPrincipal?.Name
                        && Metadata.IsIntraHierarchical())
                    {
                        if (!configurationSource.Value.Overrides(Metadata.GetDependentToPrincipalConfigurationSource())
                            || (!overrideSameSource && configurationSource == Metadata.GetDependentToPrincipalConfigurationSource()))
                        {
                            return false;
                        }

                        removeOppositeNavigation = true;
                    }
                }
            }

            var navigationToPrincipalProperty = navigationToPrincipal?.Property;
            var navigationToDependentProperty = navigationToDependent?.Property;

            // ReSharper disable once InlineOutVariableDeclaration
            bool? invertedShouldBeUnique = null;
            if (navigationToPrincipalProperty != null
                && !IsCompatible(
                    navigationToPrincipalProperty,
                    false,
                    principalEntityType.ClrType,
                    dependentEntityType.ClrType,
                    false,
                    out invertedShouldBeUnique))
            {
                shouldInvert = false;
            }

            if (navigationToDependentProperty != null
                && !IsCompatible(
                    navigationToDependentProperty,
                    true,
                    principalEntityType.ClrType,
                    dependentEntityType.ClrType,
                    false,
                    out _))
            {
                shouldInvert = false;
            }

            if (navigationToPrincipalProperty != null
                && !IsCompatible(
                    navigationToPrincipalProperty,
                    true,
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
                    false,
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
                && !configurationSource.Value.Overrides(Metadata.GetIsUniqueConfigurationSource()))
            {
                return false;
            }

            if (shouldBeUnique == null
                && (Metadata.IsUnique || configurationSource.Value.OverridesStrictly(Metadata.GetIsUniqueConfigurationSource()))
                && ((navigationToDependentProperty != null && shouldInvert != true)
                    || (navigationToPrincipalProperty != null && shouldInvert == true)))
            {
                // if new dependent can be both assume single
                shouldBeUnique = true;
            }

            // TODO: check whether the conflicting navigations can be removed
            removeConflictingNavigations = FindRelationships(
                principalEntityType,
                dependentEntityType,
                navigationToPrincipal,
                navigationToDependent,
                null,
                null).Where(r => r.Metadata != Metadata).Distinct().Any();

            return true;
        }

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
                            shouldBeCollection: false,
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
                shouldThrow: shouldThrow))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder IsRequired(bool isRequired, ConfigurationSource configurationSource)
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

                return ReplaceForeignKey(configurationSource, dependentProperties: Array.Empty<Property>(), isRequired: isRequired);
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
            if (isRequired == null
                || properties == null)
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder IsOwnership(bool ownership, ConfigurationSource configurationSource)
        {
            if (Metadata.IsOwnership == ownership)
            {
                Metadata.SetIsOwnership(ownership, configurationSource);
                return this;
            }

            if (!configurationSource.Overrides(Metadata.GetIsOwnershipConfigurationSource()))
            {
                return null;
            }

            using (var batch = ModelBuilder.Metadata.ConventionDispatcher.StartBatch())
            {
                var declaringType = Metadata.DeclaringEntityType;
                var otherOwnerships = declaringType.GetDeclaredForeignKeys().Where(fk => fk.IsOwnership).ToList();
                var newRelationshipBuilder = this;
                if (ownership)
                {
                    if (declaringType.HasDefiningNavigation())
                    {
                        Debug.Assert(Metadata.PrincipalToDependent == null
                                     || declaringType.DefiningNavigationName == Metadata.PrincipalToDependent.Name);

                        if (otherOwnerships.Any(fk => !configurationSource.Overrides(fk.GetIsOwnershipConfigurationSource())))
                        {
                            return null;
                        }

                        foreach (var otherOwnership in otherOwnerships)
                        {
                            otherOwnership.Builder.IsOwnership(false, configurationSource);
                        }

                        Metadata.SetIsOwnership(true, configurationSource);
                    }
                    else if (otherOwnerships.Count > 0)
                    {
                        if (!Metadata.GetConfigurationSource().Overrides(ConfigurationSource.Explicit)
                            && Metadata.PrincipalEntityType.IsInDefinitionPath(Metadata.DeclaringEntityType.ClrType))
                        {
                            return null;
                        }

                        var otherOwnership = otherOwnerships.Single();
                        Metadata.SetIsOwnership(true, configurationSource);
                        Metadata.DeclaringEntityType.Builder.RemoveForeignKey(Metadata, Metadata.GetConfigurationSource());

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

                        newRelationshipBuilder = Attach(newEntityType.Builder);

                        ModelBuilder.Metadata.ConventionDispatcher.Tracker.Update(
                            Metadata, newRelationshipBuilder.Metadata);
                    }
                    else
                    {
                        Metadata.SetIsOwnership(true, configurationSource);
                        newRelationshipBuilder.Metadata.DeclaringEntityType.Builder.HasBaseType((Type)null, configurationSource);
                    }

                    if (newRelationshipBuilder.Metadata.IsUnique)
                    {
                        newRelationshipBuilder.Metadata.DeclaringEntityType.Builder.PrimaryKey(
                            newRelationshipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);
                    }

                    newRelationshipBuilder.Metadata.DeclaringEntityType.Builder.RemoveNonOwnershipRelationships(configurationSource);
                }
                else
                {
                    newRelationshipBuilder.Metadata.SetIsOwnership(false, configurationSource);
                }

                return batch.Run(newRelationshipBuilder);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
                Debug.Assert(Metadata.Builder != null);
                return Metadata.Builder;
            }

            Debug.Assert(Metadata.Builder == null);
            ModelBuilder.Metadata.ConventionDispatcher.Tracker.Update(Metadata, newOwnership);
            return newOwnership.Builder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder IsUnique(bool unique, ConfigurationSource configurationSource)
        {
            if (Metadata.IsUnique == unique)
            {
                Metadata.SetIsUnique(unique, configurationSource);

                return this;
            }

            if (!CanSetUnique(unique, configurationSource, out var resetToDependent))
            {
                return null;
            }

            using (var batch = Metadata.DeclaringEntityType.Model.ConventionDispatcher.StartBatch())
            {
                var builder = this;
                if (resetToDependent)
                {
                    builder = builder.Navigations(null, PropertyIdentity.None, configurationSource);
                    if (builder == null)
                    {
                        return null;
                    }
                }
                builder = builder.Metadata.SetIsUnique(unique, configurationSource)?.Builder;
                if (builder == null)
                {
                    return null;
                }

                return batch.Run(builder);
            }
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

            if (Metadata.PrincipalToDependent != null
                && !Metadata.PrincipalToDependent.IsShadowProperty
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder DependentEntityType(
            [NotNull] InternalEntityTypeBuilder dependentEntityTypeBuilder, ConfigurationSource configurationSource)
            => DependentEntityType(dependentEntityTypeBuilder.Metadata, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder DependentEntityType(
            [NotNull] Type dependentType, ConfigurationSource configurationSource)
            => DependentEntityType(
                ModelBuilder.Entity(dependentType, configurationSource).Metadata,
                configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder DependentEntityType(
            [NotNull] string dependentTypeName, ConfigurationSource configurationSource)
            => DependentEntityType(ModelBuilder.Entity(dependentTypeName, configurationSource).Metadata, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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

                    builder = ModelBuilder.Metadata.ConventionDispatcher.OnPrincipalEndChanged(builder);
                }

                return builder;
            }

            if (dependentEntityType.IsAssignableFrom(Metadata.DeclaringEntityType)
                || configurationSource == ConfigurationSource.Explicit)
            {
                return RelatedEntityTypes(Metadata.PrincipalEntityType, dependentEntityType, configurationSource);
            }

            return null;
        }

        // Note: These will not invert relationships, use RelatedEntityTypes for that
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder PrincipalEntityType(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder, ConfigurationSource configurationSource)
            => PrincipalEntityType(principalEntityTypeBuilder.Metadata, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder PrincipalEntityType(
            [NotNull] Type principalType, ConfigurationSource configurationSource)
            => PrincipalEntityType(
                ModelBuilder.Entity(principalType, configurationSource).Metadata,
                configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder PrincipalEntityType(
            [NotNull] string principalTypeName, ConfigurationSource configurationSource)
            => PrincipalEntityType(
                ModelBuilder.Entity(principalTypeName, configurationSource).Metadata,
                configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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

                    builder = ModelBuilder.Metadata.ConventionDispatcher.OnPrincipalEndChanged(builder);
                }

                return builder;
            }

            if (principalEntityType.IsAssignableFrom(Metadata.PrincipalEntityType)
                || configurationSource == ConfigurationSource.Explicit)
            {
                return RelatedEntityTypes(principalEntityType, Metadata.DeclaringEntityType, configurationSource);
            }

            return null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder RelatedEntityTypes(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            ConfigurationSource? configurationSource)
            => RelatedEntityTypes(principalEntityType, dependentEntityType, configurationSource, configurationSource);

        private InternalRelationshipBuilder RelatedEntityTypes(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            ConfigurationSource? principalEndConfigurationSource,
            ConfigurationSource? configurationSource)
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

                return ModelBuilder.Metadata.ConventionDispatcher.OnPrincipalEndChanged(this);
            }

            if (!CanSetRelatedTypes(
                    principalEntityType,
                    dependentEntityType,
                    ConfigurationSource.Explicit,
                    null,
                    null,
                    configurationSource,
                    configurationSource == ConfigurationSource.Explicit,
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
                Debug.Assert(
                    configurationSource.HasValue
                    && configurationSource.Value.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource()));
                Debug.Assert(
                    configurationSource.HasValue
                    && configurationSource.Value.Overrides(Metadata.GetPrincipalKeyConfigurationSource()));

                principalEntityType = principalEntityType.LeastDerivedType(Metadata.DeclaringEntityType);
                dependentEntityType = dependentEntityType.LeastDerivedType(Metadata.PrincipalEntityType);

                if (Metadata.GetIsRequiredConfigurationSource() != ConfigurationSource.Explicit)
                {
                    Metadata.SetIsRequiredConfigurationSource(null);
                }
            }
            else
            {
                principalEntityType = principalEntityType.LeastDerivedType(Metadata.PrincipalEntityType);
                dependentEntityType = dependentEntityType.LeastDerivedType(Metadata.DeclaringEntityType);

                dependentProperties = shouldResetDependentProperties
                    ? dependentProperties
                    : ((Metadata.GetForeignKeyPropertiesConfigurationSource()?.Overrides(configurationSource) ?? false)
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
                principalEntityTypeBuilder: principalEntityType.Builder,
                dependentEntityTypeBuilder: dependentEntityType.Builder,
                navigationToPrincipal: shouldResetToPrincipal ? PropertyIdentity.None : (PropertyIdentity?)null,
                navigationToDependent: shouldResetToDependent ? PropertyIdentity.None : (PropertyIdentity?)null,
                dependentProperties: dependentProperties,
                principalProperties: principalProperties,
                isUnique: shouldBeUnique,
                principalEndConfigurationSource: principalEndConfigurationSource,
                oldRelationshipInverted: shouldInvert);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] IReadOnlyList<MemberInfo> properties, ConfigurationSource configurationSource)
            => HasForeignKey(properties, Metadata.DeclaringEntityType, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => HasForeignKey(propertyNames, Metadata.DeclaringEntityType, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] IReadOnlyList<MemberInfo> properties, [NotNull] EntityType dependentEntityType, ConfigurationSource configurationSource)
            => HasForeignKey(
                dependentEntityType.Builder.GetOrCreateProperties(properties, configurationSource),
                dependentEntityType,
                configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] IReadOnlyList<string> propertyNames, [NotNull] EntityType dependentEntityType, ConfigurationSource configurationSource)
            => HasForeignKey(
                dependentEntityType.Builder.GetOrCreateProperties(
                    propertyNames, configurationSource, Metadata.PrincipalKey.Properties, Metadata.IsRequired, useDefaultType: true),
                dependentEntityType,
                configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource? configurationSource)
            => HasForeignKey(properties, Metadata.DeclaringEntityType, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [CanBeNull] IReadOnlyList<Property> properties,
            [NotNull] EntityType dependentEntityType,
            ConfigurationSource? configurationSource)
        {
            if (properties == null)
            {
                return !configurationSource.HasValue
                       || !configurationSource.Value.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource())
                    ? null
                    : ReplaceForeignKey(
                        configurationSource,
                        dependentProperties: Array.Empty<Property>());
            }

            properties = dependentEntityType.Builder.GetActualProperties(properties, configurationSource);
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

                    builder = ModelBuilder.Metadata.ConventionDispatcher.OnPrincipalEndChanged(builder);
                }

                return builder;
            }

            if (!CanSetForeignKey(
                properties, dependentEntityType, configurationSource, out var resetIsRequired, out var resetPrincipalKey))
            {
                return null;
            }

            if (resetIsRequired)
            {
                Metadata.SetIsRequiredConfigurationSource(null);
            }

            return builder.ReplaceForeignKey(
                configurationSource,
                dependentEntityTypeBuilder: dependentEntityType.Builder,
                dependentProperties: properties,
                principalProperties: resetPrincipalKey ? Array.Empty<Property>() : null,
                principalEndConfigurationSource: properties == null ? null : configurationSource);
        }

        private bool CanSetForeignKey(
            IReadOnlyList<Property> properties,
            ConfigurationSource? configurationSource,
            EntityType dependentEntityType = null,
            bool overrideSameSource = true)
            => CanSetForeignKey(
                properties,
                dependentEntityType,
                configurationSource,
                out _,
                out _,
                overrideSameSource);

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

            if (!configurationSource.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource())
                || (!overrideSameSource && configurationSource == Metadata.GetForeignKeyPropertiesConfigurationSource()))
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

            if (!CanSetRequiredOnProperties(
                properties,
                Metadata.IsRequired,
                dependentEntityType,
                configurationSource,
                shouldThrow: false))
            {
                if (!configurationSource.HasValue
                    || !configurationSource.Value.Overrides(Metadata.GetIsRequiredConfigurationSource()))
                {
                    return false;
                }

                resetIsRequired = true;
            }

            if (dependentEntityType != Metadata.DeclaringEntityType
                || (properties.Count != 0
                    && !ForeignKey.AreCompatible(
                        Metadata.PrincipalKey.Properties,
                        properties,
                        Metadata.PrincipalEntityType,
                        Metadata.DeclaringEntityType,
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasPrincipalKey(
            [NotNull] IReadOnlyList<PropertyInfo> properties,
            ConfigurationSource configurationSource)
            => HasPrincipalKey(
                Metadata.PrincipalEntityType.Builder.GetOrCreateProperties(properties, configurationSource),
                configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasPrincipalKey(
            [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
            => HasPrincipalKey(
                Metadata.PrincipalEntityType.Builder.GetOrCreateProperties(propertyNames, configurationSource),
                configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasPrincipalKey(
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            properties = Metadata.PrincipalEntityType.Builder.GetActualProperties(properties, configurationSource);
            if (!CanSetPrincipalKey(properties, configurationSource, out var resetDependent))
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

                    builder = ModelBuilder.Metadata.ConventionDispatcher.OnPrincipalEndChanged(builder);
                }

                return builder;
            }

            return ReplaceForeignKey(
                configurationSource,
                principalProperties: properties,
                dependentProperties: resetDependent ? Array.Empty<Property>() : null,
                principalEndConfigurationSource: properties == null ? (ConfigurationSource?)null : configurationSource);
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
            PropertyIdentity? navigationToPrincipal = null,
            PropertyIdentity? navigationToDependent = null,
            IReadOnlyList<Property> dependentProperties = null,
            IReadOnlyList<Property> principalProperties = null,
            bool? isUnique = null,
            bool? isRequired = null,
            bool? isOwnership = null,
            DeleteBehavior? deleteBehavior = null,
            bool removeCurrent = true,
            ConfigurationSource? principalEndConfigurationSource = null,
            bool oldRelationshipInverted = false)
        {
            principalEntityTypeBuilder = principalEntityTypeBuilder ??
                                         (oldRelationshipInverted
                                             ? Metadata.DeclaringEntityType.Builder
                                             : Metadata.PrincipalEntityType.Builder);
            dependentEntityTypeBuilder = dependentEntityTypeBuilder ??
                                         (oldRelationshipInverted
                                             ? Metadata.PrincipalEntityType.Builder
                                             : Metadata.DeclaringEntityType.Builder);

            if (navigationToPrincipal == null)
            {
                if (oldRelationshipInverted)
                {
                    navigationToPrincipal = Metadata.GetPrincipalToDependentConfigurationSource()?.Overrides(configurationSource)
                                            ?? false
                        ? PropertyIdentity.Create(Metadata.PrincipalToDependent)
                        : navigationToPrincipal;
                }
                else
                {
                    navigationToPrincipal = Metadata.GetDependentToPrincipalConfigurationSource()?.Overrides(configurationSource)
                                            ?? false
                        ? PropertyIdentity.Create(Metadata.DependentToPrincipal)
                        : navigationToPrincipal;
                }
            }

            if (navigationToDependent == null)
            {
                if (oldRelationshipInverted)
                {
                    navigationToDependent = Metadata.GetDependentToPrincipalConfigurationSource()?.Overrides(configurationSource)
                                            ?? false
                        ? PropertyIdentity.Create(Metadata.DependentToPrincipal)
                        : navigationToDependent;
                }
                else
                {
                    navigationToDependent = Metadata.GetPrincipalToDependentConfigurationSource()?.Overrides(configurationSource)
                                            ?? false
                        ? PropertyIdentity.Create(Metadata.PrincipalToDependent)
                        : navigationToDependent;
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

            isOwnership = isOwnership ??
                          ((Metadata.GetIsOwnershipConfigurationSource()?.Overrides(configurationSource) ?? false)
                           && !oldRelationshipInverted
                              ? Metadata.IsOwnership
                              : (bool?)null);

            deleteBehavior = deleteBehavior ??
                             ((Metadata.GetDeleteBehaviorConfigurationSource()?.Overrides(configurationSource) ?? false)
                                 ? Metadata.DeleteBehavior
                                 : (DeleteBehavior?)null);

            principalEndConfigurationSource = principalEndConfigurationSource ??
                                              (principalEntityTypeBuilder.Metadata != dependentEntityTypeBuilder.Metadata
                                               && ((principalProperties != null && principalProperties.Count > 0)
                                                   || (dependentProperties != null && dependentProperties.Count > 0)
                                                   || (navigationToDependent != null && isUnique == false)
                                                   || isOwnership == true)
                                                  ? configurationSource
                                                  : null);
            principalEndConfigurationSource = principalEndConfigurationSource.Max(Metadata.GetPrincipalEndConfigurationSource());

            return ReplaceForeignKey(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                navigationToPrincipal,
                navigationToDependent,
                dependentProperties,
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
            PropertyIdentity? navigationToPrincipal,
            PropertyIdentity? navigationToDependent,
            [CanBeNull] IReadOnlyList<Property> dependentProperties,
            [CanBeNull] IReadOnlyList<Property> principalProperties,
            bool? isUnique,
            bool? isRequired,
            bool? isOwnership,
            DeleteBehavior? deleteBehavior,
            bool removeCurrent,
            bool oldRelationshipInverted,
            ConfigurationSource? principalEndConfigurationSource,
            ConfigurationSource? configurationSource)
        {
            Check.NotNull(principalEntityTypeBuilder, nameof(principalEntityTypeBuilder));
            Check.NotNull(dependentEntityTypeBuilder, nameof(dependentEntityTypeBuilder));
            Debug.Assert(
                navigationToPrincipal?.Name == null
                || navigationToPrincipal.Value.Property != null
                || !dependentEntityTypeBuilder.Metadata.HasClrType());
            Debug.Assert(
                navigationToDependent?.Name == null
                || navigationToDependent.Value.Property != null
                || !principalEntityTypeBuilder.Metadata.HasClrType());
            Debug.Assert(
                AreCompatible(
                    principalEntityTypeBuilder.Metadata,
                    dependentEntityTypeBuilder.Metadata,
                    navigationToPrincipal?.Property,
                    navigationToDependent?.Property,
                    dependentProperties != null && dependentProperties.Any() ? dependentProperties : null,
                    principalProperties != null && principalProperties.Any() ? principalProperties : null,
                    isUnique,
                    isRequired,
                    configurationSource));
            Debug.Assert(
                removeCurrent
                || ((dependentProperties == null
                     || PropertyListComparer.Instance.Equals(dependentProperties, Metadata.Properties))
                    && (principalProperties == null
                        || PropertyListComparer.Instance.Equals(principalProperties, Metadata.PrincipalKey.Properties))
                    && Metadata.PrincipalEntityType.IsAssignableFrom(principalEntityTypeBuilder.Metadata)
                    && Metadata.DeclaringEntityType.IsAssignableFrom(dependentEntityTypeBuilder.Metadata)));

            InternalRelationshipBuilder newRelationshipBuilder;
            using (var batch = Metadata.DeclaringEntityType.Model.ConventionDispatcher.StartBatch())
            {
                newRelationshipBuilder = GetOrCreateRelationshipBuilder(
                    principalEntityTypeBuilder.Metadata,
                    dependentEntityTypeBuilder.Metadata,
                    navigationToPrincipal,
                    navigationToDependent,
                    dependentProperties != null && dependentProperties.Any() ? dependentProperties : null,
                    principalProperties != null && principalProperties.Any() ? principalProperties : null,
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

                var oldNavigationToPrincipalName = oldRelationshipInverted
                    ? Metadata.PrincipalToDependent?.Name
                    : Metadata.DependentToPrincipal?.Name;
                var oldNavigationToDependentName = oldRelationshipInverted
                    ? Metadata.DependentToPrincipal?.Name
                    : Metadata.PrincipalToDependent?.Name;

                var newRelationshipConfigurationSource = Metadata.GetConfigurationSource();
                if ((dependentProperties != null && dependentProperties.Any())
                    || navigationToPrincipal?.Name != null
                    || navigationToDependent?.Name != null)
                {
                    newRelationshipConfigurationSource = newRelationshipConfigurationSource.Max(configurationSource);
                }
                newRelationshipBuilder.Metadata.UpdateConfigurationSource(newRelationshipConfigurationSource);

                var resetToPrincipal = newRelationshipBuilder.Metadata.DependentToPrincipal != null
                                       && ((existingRelationshipInverted == false
                                            && navigationToPrincipal != null
                                            && navigationToPrincipal.Value.Name != newRelationshipBuilder.Metadata.DependentToPrincipal.Name)
                                           || (existingRelationshipInverted == true
                                               && navigationToDependent != null
                                               && navigationToDependent.Value.Name != newRelationshipBuilder.Metadata.DependentToPrincipal.Name));

                var resetToDependent = newRelationshipBuilder.Metadata.PrincipalToDependent != null
                                       && ((existingRelationshipInverted == false
                                            && navigationToDependent != null
                                            && navigationToDependent.Value.Name != newRelationshipBuilder.Metadata.PrincipalToDependent.Name)
                                           || (existingRelationshipInverted == true
                                               && navigationToPrincipal != null
                                               && navigationToPrincipal.Value.Name != newRelationshipBuilder.Metadata.PrincipalToDependent.Name));

                if (resetToPrincipal
                    || resetToDependent)
                {
                    newRelationshipBuilder = newRelationshipBuilder.Navigations(
                                                 resetToPrincipal ? PropertyIdentity.None : (PropertyIdentity?)null,
                                                 resetToDependent ? PropertyIdentity.None : (PropertyIdentity?)null,
                                                 configurationSource)
                                             ?? newRelationshipBuilder;
                }

                newRelationshipBuilder = newRelationshipBuilder.RelatedEntityTypes(
                                             principalEntityTypeBuilder.Metadata,
                                             dependentEntityTypeBuilder.Metadata,
                                             principalEndConfigurationSource,
                                             configurationSource)
                                         ?? newRelationshipBuilder;

                if (dependentProperties != null
                    && dependentProperties.Any())
                {
                    dependentProperties = dependentEntityTypeBuilder.GetActualProperties(dependentProperties, configurationSource);
                    var foreignKeyPropertiesConfigurationSource = configurationSource;
                    if (PropertyListComparer.Instance.Equals(Metadata.Properties, dependentProperties)
                        && !oldRelationshipInverted)
                    {
                        foreignKeyPropertiesConfigurationSource =
                            foreignKeyPropertiesConfigurationSource.Max(Metadata.GetForeignKeyPropertiesConfigurationSource());
                    }

                    if (foreignKeyPropertiesConfigurationSource.HasValue)
                    {
                        newRelationshipBuilder = newRelationshipBuilder.HasForeignKey(
                                                     dependentProperties,
                                                     foreignKeyPropertiesConfigurationSource.Value)
                                                 ?? newRelationshipBuilder;
                    }
                }
                if (principalProperties != null
                    && principalProperties.Any())
                {
                    principalProperties = principalEntityTypeBuilder.GetActualProperties(principalProperties, configurationSource);
                    var principalKeyConfigurationSource = configurationSource;
                    if (PropertyListComparer.Instance.Equals(principalProperties, newRelationshipBuilder.Metadata.PrincipalKey.Properties)
                        && !oldRelationshipInverted)
                    {
                        principalKeyConfigurationSource = principalKeyConfigurationSource.Max(Metadata.GetPrincipalKeyConfigurationSource());
                    }

                    if (principalKeyConfigurationSource.HasValue)
                    {
                        newRelationshipBuilder = newRelationshipBuilder.HasPrincipalKey(
                                                     principalProperties,
                                                     principalKeyConfigurationSource.Value)
                                                 ?? newRelationshipBuilder;
                    }
                }
                if (isUnique.HasValue)
                {
                    var isUniqueConfigurationSource = configurationSource;
                    if (isUnique.Value == Metadata.IsUnique)
                    {
                        isUniqueConfigurationSource = isUniqueConfigurationSource.Max(Metadata.GetIsUniqueConfigurationSource());
                    }

                    if (isUniqueConfigurationSource.HasValue)
                    {
                        newRelationshipBuilder = newRelationshipBuilder.IsUnique(
                                                     isUnique.Value,
                                                     isUniqueConfigurationSource.Value)
                                                 ?? newRelationshipBuilder;
                    }
                }
                if (isRequired.HasValue)
                {
                    var isRequiredConfigurationSource = configurationSource;
                    if (isRequired.Value == Metadata.IsRequired)
                    {
                        isRequiredConfigurationSource = isRequiredConfigurationSource.Max(Metadata.GetIsRequiredConfigurationSource());
                    }

                    if (isRequiredConfigurationSource.HasValue)
                    {
                        newRelationshipBuilder = newRelationshipBuilder.IsRequired(
                                                     isRequired.Value,
                                                     isRequiredConfigurationSource.Value)
                                                 ?? newRelationshipBuilder;
                    }
                }
                if (deleteBehavior.HasValue)
                {
                    var deleteBehaviorConfigurationSource = configurationSource;
                    if (deleteBehavior.Value == Metadata.DeleteBehavior)
                    {
                        deleteBehaviorConfigurationSource = deleteBehaviorConfigurationSource.Max(Metadata.GetDeleteBehaviorConfigurationSource());
                    }

                    if (deleteBehaviorConfigurationSource.HasValue)
                    {
                        newRelationshipBuilder = newRelationshipBuilder.DeleteBehavior(
                                                     deleteBehavior.Value,
                                                     deleteBehaviorConfigurationSource.Value)
                                                 ?? newRelationshipBuilder;
                    }
                }
                if (navigationToPrincipal != null)
                {
                    var navigationToPrincipalConfigurationSource = configurationSource;
                    if (navigationToPrincipal.Value.Name == oldNavigationToPrincipalName)
                    {
                        var oldToPrincipalConfigurationSource = oldRelationshipInverted
                            ? Metadata.GetPrincipalToDependentConfigurationSource()
                            : Metadata.GetDependentToPrincipalConfigurationSource();
                        navigationToPrincipalConfigurationSource = navigationToPrincipalConfigurationSource.Max(oldToPrincipalConfigurationSource);
                    }

                    if (navigationToPrincipalConfigurationSource.HasValue)
                    {
                        newRelationshipBuilder = newRelationshipBuilder.Navigations(
                                                     navigationToPrincipal,
                                                     null,
                                                     navigationToPrincipalConfigurationSource.Value)
                                                 ?? newRelationshipBuilder;
                        var oldNavigation = oldRelationshipInverted
                            ? Metadata.PrincipalToDependent
                            : Metadata.DependentToPrincipal;
                        if (oldNavigation != null
                            && oldNavigation != newRelationshipBuilder.Metadata.DependentToPrincipal)
                        {
                            newRelationshipBuilder.Metadata.DependentToPrincipal?.Builder.MergeAnnotationsFrom(oldNavigation);
                        }
                    }
                }
                if (navigationToDependent != null)
                {
                    var navigationToDependentConfigurationSource = configurationSource;
                    if (navigationToDependent.Value.Name == oldNavigationToDependentName)
                    {
                        var oldToDependentConfigurationSource = oldRelationshipInverted
                            ? Metadata.GetDependentToPrincipalConfigurationSource()
                            : Metadata.GetPrincipalToDependentConfigurationSource();
                        navigationToDependentConfigurationSource = navigationToDependentConfigurationSource.Max(oldToDependentConfigurationSource);
                    }

                    if (navigationToDependentConfigurationSource.HasValue)
                    {
                        newRelationshipBuilder = newRelationshipBuilder.Navigations(
                                                     null,
                                                     navigationToDependent,
                                                     navigationToDependentConfigurationSource.Value)
                                                 ?? newRelationshipBuilder;
                        var oldNavigation = oldRelationshipInverted
                            ? Metadata.DependentToPrincipal
                            : Metadata.PrincipalToDependent;
                        if (oldNavigation != null
                            && oldNavigation != newRelationshipBuilder.Metadata.PrincipalToDependent)
                        {
                            newRelationshipBuilder.Metadata.PrincipalToDependent?.Builder.MergeAnnotationsFrom(oldNavigation);
                        }
                    }
                }
                if (isOwnership.HasValue)
                {
                    var isOwnershipConfigurationSource = configurationSource;
                    if (isOwnership.Value == Metadata.IsOwnership)
                    {
                        isOwnershipConfigurationSource = isOwnershipConfigurationSource.Max(Metadata.GetIsOwnershipConfigurationSource());
                    }

                    if (isOwnershipConfigurationSource.HasValue)
                    {
                        newRelationshipBuilder = newRelationshipBuilder.IsOwnership(
                            isOwnership.Value,
                            isOwnershipConfigurationSource.Value)
                                                 ?? newRelationshipBuilder;
                    }
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

        private InternalRelationshipBuilder GetOrCreateRelationshipBuilder(
            EntityType principalEntityType,
            EntityType dependentEntityType,
            PropertyIdentity? navigationToPrincipal,
            PropertyIdentity? navigationToDependent,
            IReadOnlyList<Property> dependentProperties,
            IReadOnlyList<Property> principalProperties,
            bool? isRequired,
            bool removeCurrent,
            ConfigurationSource? principalEndConfigurationSource,
            ConfigurationSource? configurationSource,
            out bool? existingRelationshipInverted)
        {
            existingRelationshipInverted = null;
            var matchingRelationships = FindRelationships(
                principalEntityType,
                dependentEntityType,
                navigationToPrincipal,
                navigationToDependent,
                dependentProperties,
                principalProperties);
            matchingRelationships = matchingRelationships.Distinct().Where(r => r.Metadata != Metadata).ToList();

            var unresolvableRelationships = new List<InternalRelationshipBuilder>();
            var resolvableRelationships = new List<Tuple<InternalRelationshipBuilder, bool, Resolution, bool>>();
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
                        && dependentEntityType.IsSameHierarchy(matchingRelationship.Metadata.DependentToPrincipal.DeclaringEntityType))
                    {
                        if (matchingRelationship.CanSetNavigation((string)null, true, configurationSource, overrideSameSource: false))
                        {
                            resolution |= Resolution.ResetToPrincipal;
                            sameConfigurationSource = sameConfigurationSource ?? false;
                        }
                        else if (matchingRelationship.CanSetNavigation((string)null, true, configurationSource)
                                 && (matchingRelationship.Metadata.GetConfigurationSource() == ConfigurationSource.Explicit
                                     || navigationToDependentName != null
                                     || matchingRelationship.Metadata.PrincipalToDependent == null))
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
                             && dependentEntityType.IsSameHierarchy(matchingRelationship.Metadata.PrincipalToDependent.DeclaringEntityType))
                    {
                        if (matchingRelationship.CanSetNavigation((string)null, false, configurationSource, overrideSameSource: false))
                        {
                            resolution |= Resolution.ResetToDependent;
                            sameConfigurationSource = sameConfigurationSource ?? false;
                        }
                        else if (matchingRelationship.CanSetNavigation((string)null, false, configurationSource)
                                 && (matchingRelationship.Metadata.GetConfigurationSource() == ConfigurationSource.Explicit
                                     || navigationToDependentName != null
                                     || matchingRelationship.Metadata.DependentToPrincipal == null))
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
                        && principalEntityType.IsSameHierarchy(matchingRelationship.Metadata.PrincipalToDependent.DeclaringEntityType))
                    {
                        if (matchingRelationship.CanSetNavigation((string)null, false, configurationSource, overrideSameSource: false))
                        {
                            resolution |= Resolution.ResetToDependent;
                            sameConfigurationSource = sameConfigurationSource ?? false;
                        }
                        else if (matchingRelationship.CanSetNavigation((string)null, false, configurationSource)
                                 && (matchingRelationship.Metadata.GetConfigurationSource() == ConfigurationSource.Explicit
                                     || navigationToPrincipalName != null
                                     || matchingRelationship.Metadata.DependentToPrincipal == null))
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
                             && principalEntityType.IsSameHierarchy(matchingRelationship.Metadata.DependentToPrincipal.DeclaringEntityType))
                    {
                        if (matchingRelationship.CanSetNavigation((string)null, true, configurationSource, overrideSameSource: false))
                        {
                            resolution |= Resolution.ResetToPrincipal;
                            sameConfigurationSource = sameConfigurationSource ?? false;
                        }
                        else if (matchingRelationship.CanSetNavigation((string)null, true, configurationSource)
                                 && (matchingRelationship.Metadata.GetConfigurationSource() == ConfigurationSource.Explicit
                                     || navigationToPrincipalName != null
                                     || matchingRelationship.Metadata.PrincipalToDependent == null))
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
                    if (principalProperties == null)
                    {
                        // If principal key wasn't specified on both we treat them as if it was configured to be the PK on the principal type
                        if (matchingRelationship.Metadata.GetPrincipalKeyConfigurationSource().HasValue
                            && matchingRelationship.Metadata.GetPrincipalKeyConfigurationSource().Value.Overrides(configurationSource))
                        {
                            sameConfigurationSource = sameConfigurationSource ?? false;
                        }
                        else if (matchingRelationship.CanSetForeignKey(null, configurationSource, overrideSameSource: false))
                        {
                            resolution |= Resolution.ResetDependentProperties;
                            sameConfigurationSource = sameConfigurationSource ?? false;
                        }
                        else if (matchingRelationship.CanSetForeignKey(null, configurationSource))
                        {
                            resolution |= Resolution.ResetDependentProperties;
                            sameConfigurationSource = true;
                        }
                    }
                    else
                    {
                        if (matchingRelationship.CanSetForeignKey(null, configurationSource, overrideSameSource: false))
                        {
                            resolution |= Resolution.ResetDependentProperties;
                            sameConfigurationSource = sameConfigurationSource ?? false;
                        }
                        else if (matchingRelationship.CanSetForeignKey(null, configurationSource))
                        {
                            resolution |= Resolution.ResetDependentProperties;
                            sameConfigurationSource = true;
                        }
                        else
                        {
                            resolvable = false;
                        }
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

                    resolvableRelationships.Add(Tuple.Create(matchingRelationship, sameConfigurationSource ?? true, resolution, inverseNavigationRemoved));
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
                if (!candidateRelationship.CanSetRelatedTypes(
                    principalEntityType,
                    dependentEntityType,
                    principalEndConfigurationSource,
                    navigationToPrincipal,
                    navigationToDependent,
                    configurationSource,
                    false,
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
                    && (navigationToPrincipal == null || navigationToDependent == null)
                    && candidateRelationship.Metadata.DependentToPrincipal != null
                    && candidateRelationship.Metadata.PrincipalToDependent != null)
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
                newRelationshipBuilder = candidateRelationship;
                break;
            }

            if (unresolvableRelationships.Any(r => r != newRelationshipBuilder))
            {
                return null;
            }

            // This workaround prevents the properties to be cleaned away before the new FK is created,
            // this should be replaced with reference counting
            // Issue #214
            var temporaryProperties = dependentProperties?.Where(
                p => p.GetConfigurationSource() == ConfigurationSource.Convention
                     && p.IsShadowProperty).ToList();
            var tempIndex = temporaryProperties != null
                            && temporaryProperties.Any()
                            && dependentEntityType.FindIndex(temporaryProperties) == null
                ? dependentEntityType.Builder.HasIndex(temporaryProperties, ConfigurationSource.Convention).Metadata
                : null;

            var temporaryKeyProperties = principalProperties?.Where(
                p => p.GetConfigurationSource() == ConfigurationSource.Convention
                     && p.IsShadowProperty).ToList();
            var keyTempIndex = temporaryKeyProperties != null
                               && temporaryKeyProperties.Any()
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
                    removedForeignKeys.Add(Metadata);
                    Metadata.DeclaringEntityType.Builder.RemoveForeignKey(Metadata, ConfigurationSource.Explicit);
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
                    throw new InvalidOperationException(
                        CoreStrings.ConflictingRelationshipNavigation(
                            principalEntityType.DisplayName(),
                            navigationToDependent?.Name,
                            dependentEntityType.DisplayName(),
                            navigationToPrincipal?.Name,
                            foreignKey.PrincipalEntityType.DisplayName(),
                            foreignKey.PrincipalToDependent.Name,
                            foreignKey.DeclaringEntityType.DisplayName(),
                            foreignKey.DependentToPrincipal.Name));
                }

                if (resolvableRelationship == newRelationshipBuilder)
                {
                    continue;
                }

                if (resolution.HasFlag(Resolution.Remove))
                {
                    removedForeignKeys.Add(resolvableRelationship.Metadata);
                    resolvableRelationship.Metadata.DeclaringEntityType.Builder.RemoveForeignKey(
                        resolvableRelationship.Metadata, ConfigurationSource.Explicit);
                    continue;
                }

                if (resolution.HasFlag(Resolution.ResetToPrincipal))
                {
                    resolvableRelationship = resolvableRelationship.Navigations(
                        PropertyIdentity.None, null, resolvableRelationship.Metadata.GetConfigurationSource());
                }

                if (resolution.HasFlag(Resolution.ResetToDependent))
                {
                    resolvableRelationship = resolvableRelationship.Navigations(
                        null, PropertyIdentity.None, resolvableRelationship.Metadata.GetConfigurationSource());
                }

                if (resolvableRelationship.Metadata.Builder == null)
                {
                    continue;
                }

                var navigationLessForeignKey = resolvableRelationship.Metadata;
                if (navigationLessForeignKey.DependentToPrincipal == null
                    && navigationLessForeignKey.PrincipalToDependent == null)
                {
                    navigationLessForeignKey.DeclaringEntityType.Builder.RemoveForeignKey(
                        navigationLessForeignKey, ConfigurationSource.Convention);
                }

                if (resolution.HasFlag(Resolution.ResetDependentProperties))
                {
                    var foreignKey = resolvableRelationship.Metadata;
                    resolvableRelationship.HasForeignKey((IReadOnlyList<Property>)null, foreignKey.GetConfigurationSource());
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

        private static IReadOnlyList<InternalRelationshipBuilder> FindRelationships(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            PropertyIdentity? navigationToPrincipal,
            PropertyIdentity? navigationToDependent,
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static InternalRelationshipBuilder FindCurrentRelationshipBuilder(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            PropertyIdentity? navigationToPrincipal,
            PropertyIdentity? navigationToDependent,
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
                    return null;
                }

                currentRelationship = matchingRelationship;
            }

            return currentRelationship;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
                        if (model.HasEntityTypeWithDefiningNavigation(Metadata.PrincipalEntityType.Name))
                        {
                            if (Metadata.PrincipalEntityType.HasDefiningNavigation())
                            {
                                principalEntityType = model.FindEntityType(
                                    Metadata.PrincipalEntityType.Name,
                                    Metadata.PrincipalEntityType.DefiningNavigationName,
                                    Metadata.PrincipalEntityType.DefiningEntityType.Name);
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
                        else
                        {
                            using (ModelBuilder.Metadata.ConventionDispatcher.StartBatch())
                            {
                                var entityTypeSnapshot = InternalEntityTypeBuilder.DetachAllMembers(Metadata.PrincipalEntityType);
                                principalEntityType = Metadata.PrincipalEntityType.ClrType == null
                                    ? model.AddEntityType(Metadata.PrincipalEntityType.Name, configurationSource)
                                    : model.AddEntityType(Metadata.PrincipalEntityType.ClrType, configurationSource);
                                entityTypeSnapshot.Attach(principalEntityType.Builder);
                            }
                        }
                    }

                    principalEntityTypeBuilder = principalEntityType.Builder;
                }
            }

            if (!Metadata.GetConfigurationSource().Overrides(ConfigurationSource.Explicit)
                && (principalEntityType.HasDefiningNavigation()
                    || principalEntityType.FindOwnership() != null)
                && Metadata.DependentToPrincipal != null
                && !Metadata.IsOwnership)
            {
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
                        using (ModelBuilder.Metadata.ConventionDispatcher.StartBatch())
                        {
                            var entityTypeSnapshot = InternalEntityTypeBuilder.DetachAllMembers(Metadata.DeclaringEntityType);

                            if (model.HasEntityTypeWithDefiningNavigation(Metadata.DeclaringEntityType.Name))
                            {
                                if (Metadata.DeclaringEntityType.HasDefiningNavigation())
                                {
                                    dependentEntityType = model.FindEntityType(
                                        Metadata.DeclaringEntityType.Name,
                                        Metadata.DeclaringEntityType.DefiningNavigationName,
                                        Metadata.DeclaringEntityType.DefiningEntityType.Name);
                                }

                                if (dependentEntityType == null)
                                {
                                    if (Metadata.IsOwnership
                                        && Metadata.PrincipalToDependent != null)
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

                            entityTypeSnapshot.Attach(dependentEntityType.Builder);
                        }
                    }

                    dependentEntityTypeBuilder = dependentEntityType.Builder;
                }
            }

            if (!Metadata.GetConfigurationSource().Overrides(ConfigurationSource.Explicit)
                && ((dependentEntityType.HasDefiningNavigation()
                     && (Metadata.PrincipalToDependent?.Name != dependentEntityType.DefiningNavigationName
                         || Metadata.PrincipalEntityType != dependentEntityType.DefiningEntityType))
                    || (dependentEntityType.FindOwnership() != null
                        && Metadata.PrincipalToDependent != null)))
            {
                return null;
            }

            if (dependentEntityType.GetForeignKeys().Contains(Metadata, ReferenceEqualityComparer.Instance))
            {
                Debug.Assert(Metadata.Builder != null);
                return Metadata.Builder;
            }

            IReadOnlyList<Property> dependentProperties = null;
            if (Metadata.GetForeignKeyPropertiesConfigurationSource()?.Overrides(configurationSource) == true)
            {
                dependentProperties = dependentEntityTypeBuilder.GetActualProperties(
                    Metadata.Properties,
                    AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue12107", out var isEnabled) && isEnabled
                        ? (ConfigurationSource?)null
                        : configurationSource)
                                      ?? new List<Property>();
            }

            IReadOnlyList<Property> principalProperties;
            var principalKey = principalEntityType.FindKey(Metadata.PrincipalKey.Properties);
            if (principalKey == null
                || Metadata.GetPrincipalKeyConfigurationSource()?.Overrides(configurationSource) != true)
            {
                principalProperties = new List<Property>();
                if (Metadata.GetForeignKeyPropertiesConfigurationSource()?.Overrides(ConfigurationSource.Explicit) != true)
                {
                    dependentProperties = new List<Property>();
                }
            }
            else
            {
                principalProperties = principalKey.Properties;
            }

            if (dependentProperties != null
                && dependentProperties.Count != 0)
            {
                if (!CanSetForeignKey(dependentProperties, dependentEntityType, configurationSource, out _, out var resetPrincipalKey))
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
                principalEntityTypeBuilder: principalEntityTypeBuilder,
                dependentEntityTypeBuilder: dependentEntityTypeBuilder,
                dependentProperties: dependentProperties,
                principalProperties: principalProperties);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool AreCompatible(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            [CanBeNull] MemberInfo navigationToPrincipal,
            [CanBeNull] MemberInfo navigationToDependent,
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
                navigationToPrincipal,
                navigationToDependent,
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
            PropertyIdentity? navigationToPrincipal,
            PropertyIdentity? navigationToDependent,
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
                    false,
                    false,
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

            var strictPrincipal = principalEndConfigurationSource.HasValue
                                  && principalEndConfigurationSource.Value.Overrides(Metadata.GetPrincipalEndConfigurationSource());
            var canInvert = CanInvert(configurationSource);
            if ((!strictPrincipal
                 || canInvert)
                && CanSetRelatedTypes(
                    dependentEntityType,
                    principalEntityType,
                    navigationToDependent,
                    navigationToPrincipal,
                    configurationSource,
                    strictPrincipal,
                    false,
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
            PropertyIdentity? navigationToPrincipal,
            PropertyIdentity? navigationToDependent,
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
                        true,
                        configurationSource.Value,
                        shouldThrow,
                        overrideSameSource: true,
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
                        || !CanSetNavigation((string)null, true, configurationSource.Value))
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
                        false,
                        configurationSource.Value,
                        shouldThrow,
                        overrideSameSource: true,
                        shouldBeUnique: out var toDependentShouldBeUnique,
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
                        || !CanSetNavigation((string)null, false, configurationSource.Value))
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
                && !CanSetUnique(shouldBeUnique.Value, configurationSource, out _))
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
