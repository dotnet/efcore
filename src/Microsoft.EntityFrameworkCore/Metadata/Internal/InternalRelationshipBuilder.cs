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
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [DebuggerDisplay("{Metadata,nq}")]
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
            [CanBeNull] PropertyInfo property,
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
            [CanBeNull] PropertyInfo property,
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
            [CanBeNull] PropertyInfo navigationToPrincipalProperty,
            [CanBeNull] PropertyInfo navigationToDependentProperty,
            ConfigurationSource configurationSource)
            => Navigations(
                PropertyIdentity.Create(navigationToPrincipalProperty),
                PropertyIdentity.Create(navigationToDependentProperty),
                configurationSource);

        private InternalRelationshipBuilder Navigations(
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
            [CanBeNull] PropertyInfo navigationToPrincipalProperty,
            [CanBeNull] PropertyInfo navigationToDependentProperty,
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
                var navigationProperty = Navigation.GetClrProperty(navigationToPrincipalName, dependentEntityType, principalEntityType, shouldThrow);
                if (navigationProperty == null)
                {
                    return null;
                }
                navigationToPrincipal = PropertyIdentity.Create(navigationProperty);
            }

            var navigationToDependentName = navigationToDependent?.Name;
            if (navigationToDependentName != null
                && navigationToDependent.Value.Property == null
                && principalEntityType.HasClrType())
            {
                var navigationProperty = Navigation.GetClrProperty(navigationToDependentName, principalEntityType, dependentEntityType, shouldThrow);
                if (navigationProperty == null)
                {
                    return null;
                }
                navigationToDependent = PropertyIdentity.Create(navigationProperty);
            }

            return Navigations(
                navigationToPrincipal,
                navigationToDependent,
                principalEntityType,
                dependentEntityType,
                configurationSource,
                runConventions: true);
        }

        private InternalRelationshipBuilder Navigations(
            PropertyIdentity? navigationToPrincipal,
            PropertyIdentity? navigationToDependent,
            ConfigurationSource? configurationSource,
            bool runConventions)
            => Navigations(
                navigationToPrincipal,
                navigationToDependent,
                Metadata.PrincipalEntityType,
                Metadata.DeclaringEntityType,
                configurationSource,
                runConventions);

        private InternalRelationshipBuilder Navigations(
            PropertyIdentity? navigationToPrincipal,
            PropertyIdentity? navigationToDependent,
            EntityType principalEntityType,
            EntityType dependentEntityType,
            ConfigurationSource? configurationSource,
            bool runConventions)
        {
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
                        if (navigationToPrincipal.Value.Name != null)
                        {
                            principalEntityType.Unignore(navigationToPrincipal.Value.Name);
                        }
                    }

                    if (navigationToDependent != null)
                    {
                        Metadata.UpdatePrincipalToDependentConfigurationSource(configurationSource);
                        if (navigationToDependent.Value.Name != null)
                        {
                            principalEntityType.Unignore(navigationToDependent.Value.Name);
                        }
                    }
                }
                return this;
            }

            var shouldThrow = configurationSource == ConfigurationSource.Explicit;
            bool? shouldInvert;
            bool? shouldBeUnique;
            bool removeOppositeNavigation;
            if (!CanSetNavigations(
                navigationToPrincipal,
                navigationToDependent,
                principalEntityType,
                dependentEntityType,
                configurationSource,
                shouldThrow,
                true,
                out shouldInvert,
                out shouldBeUnique,
                out removeOppositeNavigation))
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
                    dependentProperties = new Property[0];
                }

                if (Metadata.GetPrincipalKeyConfigurationSource() == configurationSource.Value)
                {
                    principalProperties = new Property[0];
                }
            }

            var builder = this;
            if (runConventions
                || shouldInvert == true)
            {
                builder = builder.ReplaceForeignKey(configurationSource,
                    principalEntityTypeBuilder: principalEntityType.Builder,
                    dependentEntityTypeBuilder: dependentEntityType.Builder,
                    navigationToPrincipal: navigationToPrincipal,
                    navigationToDependent: navigationToDependent,
                    dependentProperties: dependentProperties,
                    principalProperties: principalProperties,
                    isUnique: shouldBeUnique,
                    removeCurrent: shouldInvert ?? false,
                    principalEndConfigurationSource: shouldInvert != null ? configurationSource : null,
                    oldRelationshipInverted: shouldInvert == true,
                    runConventions: runConventions);

                Debug.Assert(builder == null
                             || builder.Metadata.Builder != null);
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

            if (shouldBeUnique.HasValue)
            {
                builder = builder.IsUnique(shouldBeUnique.Value, configurationSource.Value, runConventions: false);
            }

            if (navigationToPrincipal != null)
            {
                if (navigationToDependent != null)
                {
                    // Remove the other navigation in case it's conflicting
                    builder.Metadata.HasPrincipalToDependent((string)null, configurationSource.Value, runConventions: false);
                }

                var navigationToPrincipalName = navigationToPrincipal.Value.Name;
                if (navigationToPrincipalName != null)
                {
                    Metadata.DeclaringEntityType.Unignore(navigationToPrincipalName);
                }

                var navigationProperty = navigationToPrincipal.Value.Property;
                if (navigationProperty != null)
                {
                    builder.Metadata.HasDependentToPrincipal(navigationProperty, configurationSource.Value, runConventions: false);
                }
                else
                {
                    builder.Metadata.HasDependentToPrincipal(navigationToPrincipalName, configurationSource.Value, runConventions: false);
                }
            }

            if (navigationToDependent != null)
            {
                var navigationToDependentName = navigationToDependent.Value.Name;
                if (navigationToDependentName != null)
                {
                    Metadata.PrincipalEntityType.Unignore(navigationToDependentName);
                }

                var navigationProperty = navigationToDependent.Value.Property;
                if (navigationProperty != null)
                {
                    builder.Metadata.HasPrincipalToDependent(navigationProperty, configurationSource.Value, runConventions: false);
                }
                else
                {
                    builder.Metadata.HasPrincipalToDependent(navigationToDependentName, configurationSource.Value, runConventions: false);
                }
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
                var navigationProperty = Navigation.GetClrProperty(
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

            bool? _;
            bool __;
            return CanSetNavigation(
                navigation,
                pointsToPrincipal,
                configurationSource,
                false,
                overrideSameSource,
                out _,
                out __);
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
        {
            bool? _;
            bool __;
            return CanSetNavigation(
                PropertyIdentity.Create(navigationProperty),
                pointsToPrincipal,
                configurationSource,
                false,
                overrideSameSource,
                out _,
                out __);
        }

        private bool CanSetNavigation(
            PropertyIdentity navigation,
            bool pointsToPrincipal,
            ConfigurationSource? configurationSource,
            bool shouldThrow,
            bool overrideSameSource,
            out bool? shouldBeUnique,
            out bool removeOppositeNavigation)
        {
            bool? _;
            return pointsToPrincipal
                ? CanSetNavigations(
                    navigation,
                    null,
                    configurationSource,
                    shouldThrow,
                    overrideSameSource,
                    out _,
                    out shouldBeUnique,
                    out removeOppositeNavigation)
                : CanSetNavigations(
                    null,
                    navigation,
                    configurationSource,
                    shouldThrow,
                    overrideSameSource,
                    out _,
                    out shouldBeUnique,
                    out removeOppositeNavigation);
        }

        private bool CanSetNavigations(
            PropertyIdentity? navigationToPrincipal,
            PropertyIdentity? navigationToDependent,
            ConfigurationSource? configurationSource,
            bool shouldThrow,
            bool overrideSameSource,
            out bool? shouldInvert,
            out bool? shouldBeUnique,
            out bool removeOppositeNavigation)
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
                out removeOppositeNavigation);

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
            out bool removeOppositeNavigation)
        {
            shouldInvert = null;
            shouldBeUnique = null;
            removeOppositeNavigation = false;

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

            bool? _;
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

            return true;
        }

        private static bool IsCompatible(
            [NotNull] PropertyInfo navigationProperty,
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
            => IsUnique(unique, configurationSource, runConventions: true);

        private InternalRelationshipBuilder IsUnique(bool unique, ConfigurationSource configurationSource, bool runConventions)
        {
            if (Metadata.IsUnique == unique)
            {
                Metadata.SetIsUnique(unique, configurationSource, runConventions);

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
                builder = builder.Navigations(
                    null,
                    PropertyIdentity.None,
                    configurationSource: configurationSource,
                    runConventions: runConventions);

                if (builder == null)
                {
                    return null;
                }
            }

            builder = builder.Metadata.SetIsUnique(unique, configurationSource, runConventions)?.Builder;
            builder?.Metadata.DeclaringEntityType.FindIndex(builder.Metadata.Properties)?.SetIsUnique(unique, configurationSource);
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
                && !Navigation.IsCompatible(
                    Metadata.PrincipalToDependent.PropertyInfo,
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
            => DependentEntityType(dependentEntityTypeBuilder.Metadata, configurationSource, runConventions: true);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder DependentEntityType(
            [NotNull] Type dependentType, ConfigurationSource configurationSource)
            => DependentEntityType(ModelBuilder.Entity(dependentType, configurationSource).Metadata,
                configurationSource, runConventions: true);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder DependentEntityType(
            [NotNull] string dependentTypeName, ConfigurationSource configurationSource)
            => DependentEntityType(ModelBuilder.Entity(dependentTypeName, configurationSource).Metadata, configurationSource, runConventions: true);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder PrincipalEntityType(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder, ConfigurationSource configurationSource)
            => PrincipalEntityType(principalEntityTypeBuilder.Metadata, configurationSource, runConventions: true);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder PrincipalEntityType(
            [NotNull] Type principalType, ConfigurationSource configurationSource)
            => PrincipalEntityType(ModelBuilder.Entity(principalType, configurationSource).Metadata,
                configurationSource, runConventions: true);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder PrincipalEntityType(
            [NotNull] string principalTypeName, ConfigurationSource configurationSource)
            => PrincipalEntityType(ModelBuilder.Entity(principalTypeName, configurationSource).Metadata,
                configurationSource, runConventions: true);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder RelatedEntityTypes(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            ConfigurationSource? configurationSource,
            bool runConventions = true)
            => RelatedEntityTypes(principalEntityType, dependentEntityType, configurationSource, configurationSource, runConventions);

        private InternalRelationshipBuilder RelatedEntityTypes(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            ConfigurationSource? principalEndConfigurationSource,
            ConfigurationSource? configurationSource,
            bool runConventions)
        {
            bool shouldInvert;
            bool shouldResetToPrincipal;
            bool shouldResetToDependent;
            bool shouldResetPrincipalProperties;
            bool shouldResetDependentProperties;
            bool? shouldBeUnique;
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
                    out shouldResetDependentProperties,
                    out shouldBeUnique)
                && configurationSource != ConfigurationSource.Explicit)
            {
                return null;
            }

            var dependentProperties = (IReadOnlyList<Property>)new Property[0];
            var principalProperties = (IReadOnlyList<Property>)new Property[0];
            var builder = this;
            if (shouldInvert)
            {
                Debug.Assert(configurationSource.HasValue
                             && configurationSource.Value.Overrides(Metadata.GetForeignKeyPropertiesConfigurationSource()));
                Debug.Assert(configurationSource.HasValue
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

                if (Metadata.PrincipalEntityType == principalEntityType
                    && Metadata.DeclaringEntityType == dependentEntityType)
                {
                    Debug.Assert(!shouldResetToPrincipal
                                 && !shouldResetToDependent
                                 && !shouldResetPrincipalProperties
                                 && !shouldResetDependentProperties);

                    if (principalEndConfigurationSource.HasValue
                        && Metadata.GetPrincipalEndConfigurationSource()?.Overrides(principalEndConfigurationSource) != true)
                    {
                        builder.Metadata.UpdatePrincipalEndConfigurationSource(principalEndConfigurationSource.Value);
                        if (runConventions)
                        {
                            builder = ModelBuilder.Metadata.ConventionDispatcher.OnPrincipalEndSet(builder);
                        }
                    }

                    return builder;
                }

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
                oldRelationshipInverted: shouldInvert,
                runConventions: runConventions);
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
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
            => HasForeignKey(properties, Metadata.DeclaringEntityType, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] IReadOnlyList<MemberInfo> properties, [NotNull] EntityType dependentEntityType, ConfigurationSource configurationSource)
            => HasForeignKey(
                dependentEntityType.Builder.GetOrCreateProperties(properties, configurationSource),
                dependentEntityType,
                configurationSource,
                runConventions: true);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] IReadOnlyList<string> propertyNames, [NotNull] EntityType dependentEntityType, ConfigurationSource configurationSource)
            => HasForeignKey(
                dependentEntityType.Builder.GetOrCreateProperties(propertyNames, configurationSource, Metadata.PrincipalKey.Properties, useDefaultType: true),
                dependentEntityType,
                configurationSource,
                runConventions: true);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [CanBeNull] IReadOnlyList<Property> properties, [NotNull] EntityType dependentEntityType, ConfigurationSource configurationSource)
            => HasForeignKey(
                dependentEntityType.Builder.GetActualProperties(properties, configurationSource),
                dependentEntityType,
                configurationSource,
                runConventions: true);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource? configurationSource, bool runConventions)
            => HasForeignKey(properties, Metadata.DeclaringEntityType, configurationSource, runConventions);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [CanBeNull] IReadOnlyList<Property> properties,
            [NotNull] EntityType dependentEntityType,
            ConfigurationSource? configurationSource,
            bool runConventions)
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
            if (!CanSetForeignKey(properties, dependentEntityType, configurationSource, out resetIsRequired, out resetPrincipalKey))
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasPrincipalKey([NotNull] IReadOnlyList<PropertyInfo> properties,
            ConfigurationSource configurationSource)
            => HasPrincipalKey(
                Metadata.PrincipalEntityType.Builder.GetOrCreateProperties(properties, configurationSource),
                configurationSource,
                runConventions: true);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasPrincipalKey([NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
            => HasPrincipalKey(
                Metadata.PrincipalEntityType.Builder.GetOrCreateProperties(propertyNames, configurationSource),
                configurationSource,
                runConventions: true);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasPrincipalKey(
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
            => HasPrincipalKey(
                Metadata.PrincipalEntityType.Builder.GetActualProperties(properties, configurationSource),
                configurationSource,
                runConventions: true);

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
            PropertyIdentity? navigationToPrincipal = null,
            PropertyIdentity? navigationToDependent = null,
            IReadOnlyList<Property> dependentProperties = null,
            IReadOnlyList<Property> principalProperties = null,
            bool? isUnique = null,
            bool? isRequired = null,
            DeleteBehavior? deleteBehavior = null,
            bool removeCurrent = true,
            ConfigurationSource? principalEndConfigurationSource = null,
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

            deleteBehavior = deleteBehavior ??
                             ((Metadata.GetDeleteBehaviorConfigurationSource()?.Overrides(configurationSource) ?? false)
                                 ? Metadata.DeleteBehavior
                                 : (DeleteBehavior?)null);

            principalEndConfigurationSource = principalEndConfigurationSource ??
                                              (principalEntityTypeBuilder.Metadata != dependentEntityTypeBuilder.Metadata
                                               && ((principalProperties != null && principalProperties.Any())
                                                   || (dependentProperties != null && dependentProperties.Any())
                                                   || (navigationToDependent != null && isUnique == false))
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
                deleteBehavior,
                removeCurrent,
                oldRelationshipInverted,
                principalEndConfigurationSource,
                configurationSource,
                runConventions);
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
            DeleteBehavior? deleteBehavior,
            bool removeCurrent,
            bool oldRelationshipInverted,
            ConfigurationSource? principalEndConfigurationSource,
            ConfigurationSource? configurationSource,
            bool runConventions,
            List<Tuple<InternalEntityTypeBuilder, InternalEntityTypeBuilder, string, PropertyInfo>> removedNavigations = null,
            List<Tuple<InternalEntityTypeBuilder, ForeignKey>> removedForeignKeys = null)
        {
            Check.NotNull(principalEntityTypeBuilder, nameof(principalEntityTypeBuilder));
            Check.NotNull(dependentEntityTypeBuilder, nameof(dependentEntityTypeBuilder));
            Debug.Assert(navigationToPrincipal?.Name == null
                         || navigationToPrincipal.Value.Property != null
                         || !dependentEntityTypeBuilder.Metadata.HasClrType());
            Debug.Assert(navigationToDependent?.Name == null
                         || navigationToDependent.Value.Property != null
                         || !principalEntityTypeBuilder.Metadata.HasClrType());
            Debug.Assert(AreCompatible(
                principalEntityTypeBuilder.Metadata,
                dependentEntityTypeBuilder.Metadata,
                navigationToPrincipal?.Property,
                navigationToDependent?.Property,
                dependentProperties != null && dependentProperties.Any() ? dependentProperties : null,
                principalProperties != null && principalProperties.Any() ? principalProperties : null,
                isUnique,
                isRequired,
                configurationSource));
            Debug.Assert(removeCurrent
                         || ((dependentProperties == null
                              || PropertyListComparer.Instance.Equals(dependentProperties, Metadata.Properties))
                             && (principalProperties == null
                                 || PropertyListComparer.Instance.Equals(principalProperties, Metadata.PrincipalKey.Properties))
                             && Metadata.PrincipalEntityType.IsAssignableFrom(principalEntityTypeBuilder.Metadata)
                             && Metadata.DeclaringEntityType.IsAssignableFrom(dependentEntityTypeBuilder.Metadata)));

            var dependentEntityType = dependentEntityTypeBuilder.Metadata;
            var principalEntityType = principalEntityTypeBuilder.Metadata;
            removedNavigations = removedNavigations
                                 ?? new List<Tuple<InternalEntityTypeBuilder, InternalEntityTypeBuilder, string, PropertyInfo>>();
            removedForeignKeys = removedForeignKeys
                                 ?? new List<Tuple<InternalEntityTypeBuilder, ForeignKey>>();
            var addedForeignKeys = new List<InternalRelationshipBuilder>();
            bool? existingRelationshipInverted;
            var newRelationshipBuilder = GetOrCreateRelationshipBuilder(
                principalEntityType,
                dependentEntityType,
                navigationToPrincipal,
                navigationToDependent,
                dependentProperties != null && dependentProperties.Any() ? dependentProperties : null,
                principalProperties != null && principalProperties.Any() ? principalProperties : null,
                isRequired,
                removeCurrent,
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

            string initialDependentToPrincipalName = null;
            string initialPrincipalToDependentName = null;
            if (existingRelationshipInverted.HasValue)
            {
                var foreignKey = newRelationshipBuilder.Metadata;
                if (newRelationshipBuilder.Metadata.DependentToPrincipal != null)
                {
                    initialDependentToPrincipalName = foreignKey.DependentToPrincipal.Name;
                }
                if (newRelationshipBuilder.Metadata.PrincipalToDependent != null)
                {
                    initialPrincipalToDependentName = foreignKey.PrincipalToDependent.Name;
                }
            }

            var initialRelationship = newRelationshipBuilder.Metadata;
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

                dependentEntityType = dependentEntityTypeBuilder.Metadata;
                principalEntityType = principalEntityTypeBuilder.Metadata;

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

            newRelationshipBuilder = newRelationshipBuilder.RelatedEntityTypes(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                navigationToPrincipal,
                navigationToDependent,
                dependentProperties,
                principalEndConfigurationSource,
                configurationSource,
                existingRelationshipInverted ?? false);

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
                                                 foreignKeyPropertiesConfigurationSource.Value,
                                                 runConventions: false)
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
                                                 principalKeyConfigurationSource.Value,
                                                 runConventions: false)
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
                                                 isUniqueConfigurationSource.Value,
                                                 runConventions: false)
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
                                                 isRequiredConfigurationSource.Value,
                                                 runConventions: false)
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
                                                 navigationToPrincipalConfigurationSource.Value,
                                                 runConventions: false)
                                             ?? newRelationshipBuilder;
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
                                                 navigationToDependentConfigurationSource.Value,
                                                 runConventions: false)
                                             ?? newRelationshipBuilder;
                }
            }

            if (runConventions)
            {
                for (var i = 0; i < removedNavigations.Count; i++)
                {
                    var removedNavigation = removedNavigations[i];
                    if (newRelationshipBuilder.Metadata.DependentToPrincipal != null
                        && removedNavigation.Item3 == newRelationshipBuilder.Metadata.DependentToPrincipal.Name
                        && newRelationshipBuilder.Metadata.DeclaringEntityType.IsAssignableFrom(removedNavigation.Item1.Metadata))
                    {
                        removedNavigations.RemoveAt(i);
                        i--;
                        continue;
                    }

                    if (newRelationshipBuilder.Metadata.PrincipalToDependent != null
                        && removedNavigation.Item3 == newRelationshipBuilder.Metadata.PrincipalToDependent.Name
                        && newRelationshipBuilder.Metadata.PrincipalEntityType.IsAssignableFrom(removedNavigation.Item1.Metadata))
                    {
                        removedNavigations.RemoveAt(i);
                        i--;
                    }
                }

                foreach (var removedNavigation in removedNavigations)
                {
                    ModelBuilder.Metadata.ConventionDispatcher.OnNavigationRemoved(
                        removedNavigation.Item1, removedNavigation.Item2, removedNavigation.Item3, removedNavigation.Item4);
                }

                if (newRelationshipBuilder.Metadata != initialRelationship)
                {
                    removedForeignKeys.Add(Tuple.Create(initialRelationship.DeclaringEntityType.Builder, initialRelationship));
                }

                foreach (var removedForeignKey in removedForeignKeys)
                {
                    ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyRemoved(removedForeignKey.Item1, removedForeignKey.Item2);
                }

                dependentProperties = dependentProperties != null && dependentProperties.Any()
                    ? newRelationshipBuilder.Metadata.Properties : null;
                principalProperties = principalProperties != null && principalProperties.Any()
                    ? newRelationshipBuilder.Metadata.PrincipalKey.Properties : null;
                if (newRelationshipBuilder.Metadata.Builder == null)
                {
                    newRelationshipBuilder = FindCurrentRelationshipBuilder(
                        principalEntityType,
                        dependentEntityType,
                        navigationToPrincipal,
                        navigationToDependent,
                        dependentProperties,
                        principalProperties);
                }

                if (newRelationshipBuilder != null
                    && (newRelationshipBuilder.Metadata != initialRelationship
                        || existingRelationshipInverted == null))
                {
                    newRelationshipBuilder = ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyAdded(newRelationshipBuilder);
                }

                foreach (var addedForeignKey in addedForeignKeys)
                {
                    ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyAdded(addedForeignKey);
                }

                if (newRelationshipBuilder?.Metadata.Builder == null)
                {
                    newRelationshipBuilder = FindCurrentRelationshipBuilder(
                        principalEntityType,
                        dependentEntityType,
                        navigationToPrincipal,
                        navigationToDependent,
                        dependentProperties,
                        principalProperties);
                    if (newRelationshipBuilder == null)
                    {
                        return null;
                    }
                }

                if (strictPrincipal
                    && initialPrincipalEndConfigurationSource != principalEndConfigurationSource)
                {
                    newRelationshipBuilder = ModelBuilder.Metadata.ConventionDispatcher.OnPrincipalEndSet(newRelationshipBuilder);
                    if (newRelationshipBuilder == null)
                    {
                        return null;
                    }

                    if (newRelationshipBuilder.Metadata.Builder == null)
                    {
                        newRelationshipBuilder = FindCurrentRelationshipBuilder(
                            principalEntityType,
                            dependentEntityType,
                            navigationToPrincipal,
                            navigationToDependent,
                            dependentProperties,
                            principalProperties);
                        if (newRelationshipBuilder == null)
                        {
                            return null;
                        }
                    }
                }

                if (newRelationshipBuilder.Metadata.DependentToPrincipal != null
                    && ((existingRelationshipInverted != true
                         && (initialDependentToPrincipalName != newRelationshipBuilder.Metadata.DependentToPrincipal.Name))
                        || (existingRelationshipInverted.Value
                            && (initialPrincipalToDependentName != newRelationshipBuilder.Metadata.DependentToPrincipal.Name))))
                {
                    newRelationshipBuilder = ModelBuilder.Metadata.ConventionDispatcher.OnNavigationAdded(
                        newRelationshipBuilder, newRelationshipBuilder.Metadata.DependentToPrincipal);
                    if (newRelationshipBuilder == null)
                    {
                        return null;
                    }

                    if (newRelationshipBuilder.Metadata.Builder == null)
                    {
                        newRelationshipBuilder = FindCurrentRelationshipBuilder(
                            principalEntityType,
                            dependentEntityType,
                            navigationToPrincipal,
                            navigationToDependent,
                            dependentProperties,
                            principalProperties);
                        if (newRelationshipBuilder == null)
                        {
                            return null;
                        }
                    }
                }

                if (newRelationshipBuilder.Metadata.PrincipalToDependent != null
                    && ((existingRelationshipInverted != true
                         && (initialPrincipalToDependentName != newRelationshipBuilder.Metadata.PrincipalToDependent.Name))
                        || (existingRelationshipInverted.Value
                            && (initialDependentToPrincipalName != newRelationshipBuilder.Metadata.PrincipalToDependent.Name))))
                {
                    newRelationshipBuilder = ModelBuilder.Metadata.ConventionDispatcher.OnNavigationAdded(
                        newRelationshipBuilder, newRelationshipBuilder.Metadata.PrincipalToDependent);
                    if (newRelationshipBuilder == null)
                    {
                        return null;
                    }

                    if (newRelationshipBuilder.Metadata.Builder == null)
                    {
                        newRelationshipBuilder = FindCurrentRelationshipBuilder(
                            principalEntityType,
                            dependentEntityType,
                            navigationToPrincipal,
                            navigationToDependent,
                            dependentProperties,
                            principalProperties);
                        if (newRelationshipBuilder == null)
                        {
                            return null;
                        }
                    }
                }
            }

            return newRelationshipBuilder;
        }

        private InternalRelationshipBuilder RelatedEntityTypes(
            InternalEntityTypeBuilder principalEntityTypeBuilder,
            InternalEntityTypeBuilder dependentEntityTypeBuilder,
            PropertyIdentity? navigationToPrincipal,
            PropertyIdentity? navigationToDependent,
            IReadOnlyList<Property> dependentProperties,
            ConfigurationSource? principalEndConfigurationSource,
            ConfigurationSource? configurationSource,
            bool existingRelationshipInverted)
        {
            var newRelationshipBuilder = this;
            var resetToPrincipal = newRelationshipBuilder.Metadata.DependentToPrincipal != null
                                   && ((!existingRelationshipInverted
                                        && navigationToPrincipal != null
                                        && navigationToPrincipal.Value.Name != newRelationshipBuilder.Metadata.DependentToPrincipal.Name)
                                       || (existingRelationshipInverted
                                           && navigationToDependent != null
                                           && navigationToDependent.Value.Name != newRelationshipBuilder.Metadata.DependentToPrincipal.Name));

            var resetToDependent = newRelationshipBuilder.Metadata.PrincipalToDependent != null
                                   && ((!existingRelationshipInverted
                                        && navigationToDependent != null
                                        && navigationToDependent.Value.Name != newRelationshipBuilder.Metadata.PrincipalToDependent.Name)
                                       || (existingRelationshipInverted
                                           && navigationToPrincipal != null
                                           && navigationToPrincipal.Value.Name != newRelationshipBuilder.Metadata.PrincipalToDependent.Name));

            if (resetToPrincipal
                || resetToDependent)
            {
                newRelationshipBuilder = newRelationshipBuilder.Navigations(
                    resetToPrincipal ? PropertyIdentity.None : (PropertyIdentity?)null,
                    resetToDependent ? PropertyIdentity.None : (PropertyIdentity?)null,
                    configurationSource,
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
                configurationSource,
                runConventions: false);
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
            List<Tuple<InternalEntityTypeBuilder, InternalEntityTypeBuilder, string, PropertyInfo>> removedNavigations,
            List<Tuple<InternalEntityTypeBuilder, ForeignKey>> removedForeignKeys,
            List<InternalRelationshipBuilder> addedForeignKeys,
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
                var sameConfigurationSource = true;
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
                            sameConfigurationSource = false;
                        }
                        else if (matchingRelationship.CanSetNavigation((string)null, true, configurationSource))
                        {
                            if (navigationToDependentName != null
                                && matchingRelationship.Metadata.PrincipalToDependent != null
                                && navigationToDependentName != matchingRelationship.Metadata.PrincipalToDependent.Name)
                            {
                                inverseNavigationRemoved = true;
                            }
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
                        if (matchingRelationship.CanSetNavigation((string)null, false, configurationSource, overrideSameSource: false))
                        {
                            resolution |= Resolution.ResetToDependent;
                            sameConfigurationSource = false;
                        }
                        else if (matchingRelationship.CanSetNavigation((string)null, false, configurationSource))
                        {
                            if (navigationToDependentName != null
                                && matchingRelationship.Metadata.DependentToPrincipal != null
                                && navigationToDependentName != matchingRelationship.Metadata.DependentToPrincipal.Name)
                            {
                                inverseNavigationRemoved = true;
                            }
                            resolution |= Resolution.ResetToDependent;
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
                            sameConfigurationSource = false;
                        }
                        else if (matchingRelationship.CanSetNavigation((string)null, false, configurationSource))
                        {
                            if (navigationToPrincipalName != null
                                && matchingRelationship.Metadata.DependentToPrincipal != null
                                && navigationToPrincipalName != matchingRelationship.Metadata.DependentToPrincipal.Name)
                            {
                                inverseNavigationRemoved = true;
                            }
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
                        if (matchingRelationship.CanSetNavigation((string)null, true, configurationSource, overrideSameSource: false))
                        {
                            resolution |= Resolution.ResetToPrincipal;
                            sameConfigurationSource = false;
                        }
                        else if (matchingRelationship.CanSetNavigation((string)null, true, configurationSource))
                        {
                            if (navigationToPrincipalName != null
                                && matchingRelationship.Metadata.PrincipalToDependent != null
                                && navigationToPrincipalName != matchingRelationship.Metadata.PrincipalToDependent.Name)
                            {
                                inverseNavigationRemoved = true;
                            }
                            resolution |= Resolution.ResetToPrincipal;
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
                            sameConfigurationSource = false;
                        }
                        else if (matchingRelationship.CanSetForeignKey(null, configurationSource, overrideSameSource: false))
                        {
                            resolution |= Resolution.ResetDependentProperties;
                            sameConfigurationSource = false;
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
                            sameConfigurationSource = false;
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
                    if (sameConfigurationSource
                        && configurationSource.HasValue
                        && matchingRelationship.Metadata.DeclaringEntityType.Builder
                            .CanRemoveForeignKey(matchingRelationship.Metadata, configurationSource.Value))
                    {
                        resolution |= Resolution.Remove;
                    }

                    resolvableRelationships.Add(Tuple.Create(matchingRelationship, sameConfigurationSource, resolution, inverseNavigationRemoved));
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
                bool candidateRelationshipInverted;
                bool _;
                bool? __;
                if (!candidateRelationship.CanSetRelatedTypes(
                    principalEntityType,
                    dependentEntityType,
                    principalEndConfigurationSource,
                    navigationToPrincipal,
                    navigationToDependent,
                    configurationSource,
                    false,
                    out candidateRelationshipInverted,
                    out _,
                    out _,
                    out _,
                    out _,
                    out __))
                {
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
            var temporaryProperties = dependentProperties?.Where(p => p.GetConfigurationSource() == ConfigurationSource.Convention
                                                                      && p.IsShadowProperty).ToList();
            var tempIndex = temporaryProperties != null
                            && temporaryProperties.Any()
                            && dependentEntityType.FindIndex(temporaryProperties) == null
                ? dependentEntityType.Builder.HasIndex(temporaryProperties, ConfigurationSource.Convention).Metadata
                : null;

            var temporaryKeyProperties = principalProperties?.Where(p => p.GetConfigurationSource() == ConfigurationSource.Convention
                                                                         && p.IsShadowProperty).ToList();
            var keyTempIndex = temporaryKeyProperties != null
                               && temporaryKeyProperties.Any()
                               && principalEntityType.FindIndex(temporaryKeyProperties) == null
                ? principalEntityType.Builder.HasIndex(temporaryKeyProperties, ConfigurationSource.Convention).Metadata
                : null;

            if (Metadata.Builder != null)
            {
                if (removeCurrent || newRelationshipBuilder != null)
                {
                    RemoveForeignKey(Metadata, removedNavigations, removedForeignKeys);
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
                    var foreingKey = resolvableRelationship.Metadata;
                    throw new InvalidOperationException(CoreStrings.ConflictingRelationshipNavigation(
                        principalEntityType.DisplayName(),
                        navigationToDependent?.Name,
                        dependentEntityType.DisplayName(),
                        navigationToPrincipal?.Name,
                        foreingKey.PrincipalEntityType.DisplayName(),
                        foreingKey.PrincipalToDependent.Name,
                        foreingKey.DeclaringEntityType.DisplayName(),
                        foreingKey.DependentToPrincipal.Name));
                }

                if (resolvableRelationship == newRelationshipBuilder)
                {
                    continue;
                }

                if (resolution.HasFlag(Resolution.Remove))
                {
                    RemoveForeignKey(resolvableRelationship.Metadata, removedNavigations, removedForeignKeys);
                    continue;
                }

                if (resolution.HasFlag(Resolution.ResetToPrincipal))
                {
                    var foreignKey = resolvableRelationship.Metadata;
                    removedNavigations.Add(Tuple.Create(
                        foreignKey.DeclaringEntityType.Builder,
                        foreignKey.PrincipalEntityType.Builder,
                        foreignKey.DependentToPrincipal.Name,
                        foreignKey.DependentToPrincipal.PropertyInfo));
                    resolvableRelationship = resolvableRelationship.Navigations(
                        PropertyIdentity.None, null, foreignKey.GetConfigurationSource(), runConventions: false);
                }

                if (resolution.HasFlag(Resolution.ResetToDependent))
                {
                    var foreignKey = resolvableRelationship.Metadata;
                    removedNavigations.Add(Tuple.Create(
                        foreignKey.PrincipalEntityType.Builder,
                        foreignKey.DeclaringEntityType.Builder,
                        foreignKey.PrincipalToDependent.Name,
                        foreignKey.PrincipalToDependent.PropertyInfo));
                    resolvableRelationship = resolvableRelationship.Navigations(
                        null, PropertyIdentity.None, foreignKey.GetConfigurationSource(), runConventions: false);
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
                    ConfigurationSource.Convention,
                    runConventions: false);
            }
            else
            {
                if (newRelationshipBuilder.Metadata.DependentToPrincipal != null)
                {
                    var newForeignKey = newRelationshipBuilder.Metadata;
                    removedNavigations.Add(Tuple.Create(
                        newForeignKey.DeclaringEntityType.Builder,
                        newForeignKey.PrincipalEntityType.Builder,
                        newForeignKey.DependentToPrincipal.Name,
                        newForeignKey.DependentToPrincipal.PropertyInfo));
                }
                if (newRelationshipBuilder.Metadata.PrincipalToDependent != null)
                {
                    var newForeignKey = newRelationshipBuilder.Metadata;
                    removedNavigations.Add(Tuple.Create(
                        newForeignKey.PrincipalEntityType.Builder,
                        newForeignKey.DeclaringEntityType.Builder,
                        newForeignKey.PrincipalToDependent.Name,
                        newForeignKey.PrincipalToDependent.PropertyInfo));
                }
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

        private void RemoveForeignKey(
            ForeignKey foreignKey,
            List<Tuple<InternalEntityTypeBuilder, InternalEntityTypeBuilder, string, PropertyInfo>> removedNavigations,
            List<Tuple<InternalEntityTypeBuilder, ForeignKey>> removedForeignKeys)
        {
            var dependentEntityTypeBuilder = foreignKey.DeclaringEntityType.Builder;
            var principalEntityTypeBuilder = foreignKey.PrincipalEntityType.Builder;
            var navigationToPrincipal = foreignKey.DependentToPrincipal;
            if (navigationToPrincipal != null)
            {
                removedNavigations.Add(Tuple.Create(
                    dependentEntityTypeBuilder, principalEntityTypeBuilder, navigationToPrincipal.Name, navigationToPrincipal.PropertyInfo));
            }

            var navigationToDependent = foreignKey.PrincipalToDependent;
            if (navigationToDependent != null)
            {
                removedNavigations.Add(Tuple.Create(
                    principalEntityTypeBuilder, dependentEntityTypeBuilder, navigationToDependent.Name, navigationToDependent.PropertyInfo));
            }

            var foreignKeyOwner = foreignKey.DeclaringEntityType.Builder;
            var replacedConfigurationSource = foreignKeyOwner.RemoveForeignKey(foreignKey, ConfigurationSource.Explicit, runConventions: false);
            Debug.Assert(replacedConfigurationSource.HasValue);

            removedForeignKeys.Add(Tuple.Create(foreignKeyOwner, foreignKey));
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
                existingRelationships.AddRange(dependentEntityType
                    .FindNavigationsInHierarchy(navigationToPrincipal.Value.Name)
                    .Select(n => n.ForeignKey.Builder));
            }

            if (navigationToDependent?.Name != null)
            {
                existingRelationships.AddRange(principalEntityType
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
                        existingRelationships.AddRange(dependentEntityType
                            .FindForeignKeysInHierarchy(dependentProperties, principalKey, principalEntityType)
                            .Select(fk => fk.Builder));
                    }
                }
                else
                {
                    existingRelationships.AddRange(dependentEntityType
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
        public virtual InternalRelationshipBuilder Attach(ConfigurationSource configurationSource)
        {
            if (Metadata.DeclaringEntityType.GetForeignKeys().Contains(Metadata, ReferenceEqualityComparer.Instance))
            {
                Debug.Assert(Metadata.Builder != null);
                return Metadata.Builder;
            }

            IReadOnlyList<Property> dependentProperties = null;
            if (Metadata.GetForeignKeyPropertiesConfigurationSource()?.Overrides(configurationSource) == true)
            {
                dependentProperties = Metadata.DeclaringEntityType.Builder.GetActualProperties(Metadata.Properties, configurationSource: null)
                                      ?? new List<Property>();
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

            if (dependentProperties != null
                && dependentProperties.Count != 0)
            {
                bool _;
                bool resetPrincipalKey;
                if (!CanSetForeignKey(dependentProperties, Metadata.DeclaringEntityType, configurationSource, out _, out resetPrincipalKey))
                {
                    dependentProperties = new List<Property>();
                }
                else if (resetPrincipalKey)
                {
                    principalProperties = new List<Property>();
                }
            }

            Metadata.DeclaringEntityType.Model.ConventionDispatcher.OnForeignKeyRemoved(Metadata.DeclaringEntityType.Builder, Metadata);

            return ReplaceForeignKey(configurationSource,
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
            [CanBeNull] PropertyInfo navigationToPrincipal,
            [CanBeNull] PropertyInfo navigationToDependent,
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
            bool invertedShouldResetToPrincipal;
            bool invertedShouldResetToDependent;
            bool? invertedShouldBeUnique;
            bool _;
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
                    out invertedShouldResetToPrincipal,
                    out invertedShouldResetToDependent,
                    out _,
                    out _,
                    out invertedShouldBeUnique)
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

            bool? _;
            bool __;
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
                        removeOppositeNavigation: out __))
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
                var navigationToPrincipalProperty = Metadata.DependentToPrincipal?.PropertyInfo;
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
                bool? toDependentShouldBeUnique = null;
                if (!configurationSource.HasValue
                    || !CanSetNavigation(
                        navigationToDependent.Value,
                        false,
                        configurationSource.Value,
                        shouldThrow,
                        overrideSameSource: true,
                        shouldBeUnique: out toDependentShouldBeUnique,
                        removeOppositeNavigation: out __))
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
                var navigationToDependentProperty = Metadata.PrincipalToDependent?.PropertyInfo;
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
                && !CanSetUnique(shouldBeUnique.Value, configurationSource, out __))
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
