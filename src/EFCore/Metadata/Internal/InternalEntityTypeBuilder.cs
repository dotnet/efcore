// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
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
    public class InternalEntityTypeBuilder : InternalMetadataItemBuilder<EntityType>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalEntityTypeBuilder([NotNull] EntityType metadata, [NotNull] InternalModelBuilder modelBuilder)
            : base(metadata, modelBuilder)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalKeyBuilder PrimaryKey(
            [CanBeNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => PrimaryKey(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalKeyBuilder PrimaryKey(
            [CanBeNull] IReadOnlyList<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
            => PrimaryKey(GetOrCreateProperties(clrProperties, configurationSource), configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalKeyBuilder PrimaryKey(
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            var previousPrimaryKey = Metadata.FindPrimaryKey();
            if (properties == null)
            {
                if (previousPrimaryKey == null)
                {
                    return null;
                }
            }
            else if (previousPrimaryKey != null
                     && PropertyListComparer.Instance.Compare(previousPrimaryKey.Properties, properties) == 0)
            {
                previousPrimaryKey.UpdateConfigurationSource(configurationSource);
                return Metadata.SetPrimaryKey(properties, configurationSource).Builder;
            }

            var primaryKeyConfigurationSource = Metadata.GetPrimaryKeyConfigurationSource();
            if (primaryKeyConfigurationSource.HasValue
                && !configurationSource.Overrides(primaryKeyConfigurationSource.Value))
            {
                return null;
            }

            InternalKeyBuilder keyBuilder = null;
            if (properties == null)
            {
                Metadata.SetPrimaryKey(properties, configurationSource);
            }
            else
            {
                using (ModelBuilder.Metadata.ConventionDispatcher.StartBatch())
                {
                    keyBuilder = HasKeyInternal(properties, configurationSource);
                    if (keyBuilder == null)
                    {
                        return null;
                    }

                    Metadata.SetPrimaryKey(keyBuilder.Metadata.Properties, configurationSource);
                    foreach (var key in Metadata.GetDeclaredKeys().ToList())
                    {
                        if (key == keyBuilder.Metadata)
                        {
                            continue;
                        }

                        var referencingForeignKeys = key
                            .GetReferencingForeignKeys()
                            .Where(fk => fk.GetPrincipalKeyConfigurationSource() == null)
                            .ToList();
                        foreach (var referencingForeignKey in referencingForeignKeys)
                        {
                            DetachRelationship(referencingForeignKey).Attach();
                        }
                    }
                }
            }

            if (previousPrimaryKey?.Builder != null)
            {
                RemoveKeyIfUnused(previousPrimaryKey);
            }

            if (keyBuilder?.Metadata.Builder == null)
            {
                properties = GetActualProperties(properties, null);
                if (properties == null)
                {
                    return null;
                }

                // TODO: Use convention batch to get the updated builder, see #214
                return Metadata.FindPrimaryKey(properties).Builder;
            }

            return keyBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalKeyBuilder HasKey([NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => HasKeyInternal(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalKeyBuilder HasKey([NotNull] IReadOnlyList<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
            => HasKeyInternal(GetOrCreateProperties(clrProperties, configurationSource), configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalKeyBuilder HasKey([NotNull] IReadOnlyList<Property> properties, ConfigurationSource? configurationSource)
            => HasKeyInternal(properties, configurationSource);

        private InternalKeyBuilder HasKeyInternal(IReadOnlyList<Property> properties, ConfigurationSource? configurationSource)
        {
            if (properties == null)
            {
                return null;
            }

            var actualProperties = GetActualProperties(properties, configurationSource);
            var key = Metadata.FindDeclaredKey(actualProperties);
            if (key == null)
            {
                if (configurationSource == null)
                {
                    return null;
                }

                var containingForeignKeys = actualProperties
                    .SelectMany(p => p.GetContainingForeignKeys().Where(k => k.DeclaringEntityType != Metadata))
                    .ToList();

                if (containingForeignKeys.Any(fk => !configurationSource.Overrides(fk.GetForeignKeyPropertiesConfigurationSource())))
                {
                    return null;
                }

                if (configurationSource != ConfigurationSource.Explicit // let it throw for explicit
                    && actualProperties.Any(p => !p.Builder.CanSetRequired(true, configurationSource)))
                {
                    return null;
                }

                using (Metadata.Model.ConventionDispatcher.StartBatch())
                {
                    foreach (var foreignKey in containingForeignKeys
                        // let it throw for explicit
                        .Where(fk => fk.GetForeignKeyPropertiesConfigurationSource() != ConfigurationSource.Explicit)
                        .ToList())
                    {
                        foreignKey.Builder.HasForeignKey(null, configurationSource);
                    }

                    foreach (var actualProperty in actualProperties)
                    {
                        actualProperty.Builder.IsRequired(true, configurationSource.Value);
                    }

                    key = Metadata.AddKey(actualProperties, configurationSource.Value);
                }

                if (key.Builder == null)
                {
                    key = Metadata.FindDeclaredKey(actualProperties);
                }
            }
            else if (configurationSource.HasValue)
            {
                key.UpdateConfigurationSource(configurationSource.Value);
            }

            return key?.Builder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? RemoveKey([NotNull] Key key, ConfigurationSource configurationSource)
        {
            var currentConfigurationSource = key.GetConfigurationSource();
            if (!configurationSource.Overrides(currentConfigurationSource))
            {
                return null;
            }

            using (Metadata.Model.ConventionDispatcher.StartBatch())
            {
                var detachedRelationships = key.GetReferencingForeignKeys().ToList().Select(DetachRelationship).ToList();

                var removedKey = Metadata.RemoveKey(key.Properties);
                Debug.Assert(removedKey == key);

                foreach (var detachedRelationship in detachedRelationships)
                {
                    detachedRelationship.Attach();
                }

                RemoveShadowPropertiesIfUnused(key.Properties);
                foreach (var property in key.Properties)
                {
                    if (!property.IsKey()
                        && property.ClrType.IsNullableType()
                        && !property.GetContainingForeignKeys().Any(fk => fk.IsRequired))
                    {
                        // TODO: This should be handled by reference tracking, see #214
                        property.Builder?.IsRequired(null, configurationSource);
                    }
                }
            }

            return currentConfigurationSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static List<(InternalKeyBuilder, ConfigurationSource?)> DetachKeys(IEnumerable<Key> keysToDetach)
        {
            var keysToDetachList = (keysToDetach as List<Key>) ?? keysToDetach.ToList();
            if (keysToDetachList.Count == 0)
            {
                return null;
            }

            var detachedKeys = new List<(InternalKeyBuilder, ConfigurationSource?)>();
            foreach (var keyToDetach in keysToDetachList)
            {
                var detachedKey = DetachKey(keyToDetach);
                detachedKeys.Add(detachedKey);
            }

            return detachedKeys;
        }

        private static (InternalKeyBuilder, ConfigurationSource?) DetachKey(Key keyToDetach)
        {
            var entityTypeBuilder = keyToDetach.DeclaringEntityType.Builder;
            var keyBuilder = keyToDetach.Builder;

            var primaryKeyConfigurationSource = keyToDetach.IsPrimaryKey()
                ? keyToDetach.DeclaringEntityType.GetPrimaryKeyConfigurationSource()
                : null;

            if (entityTypeBuilder == null)
            {
                keyToDetach.DeclaringEntityType.RemoveKey(keyToDetach.Properties);
            }
            else
            {
                entityTypeBuilder.RemoveKey(keyToDetach, keyToDetach.GetConfigurationSource());
            }

            return (keyBuilder, primaryKeyConfigurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalPropertyBuilder Property(
            [NotNull] string propertyName,
            [NotNull] Type propertyType,
            ConfigurationSource configurationSource)
            => Property(propertyName, propertyType, configurationSource, typeConfigurationSource: configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalPropertyBuilder Property(
            [NotNull] string propertyName,
            [NotNull] Type propertyType,
            ConfigurationSource configurationSource,
            ConfigurationSource? typeConfigurationSource)
            => Property(
                propertyName, propertyType, memberInfo: null,
                configurationSource: configurationSource, typeConfigurationSource: typeConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalPropertyBuilder Property([NotNull] string propertyName, ConfigurationSource configurationSource)
            => Property(propertyName, propertyType: null, memberInfo: null, configurationSource, typeConfigurationSource: null);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalPropertyBuilder Property([NotNull] MemberInfo clrProperty, ConfigurationSource configurationSource)
            => Property(clrProperty.GetSimpleMemberName(), clrProperty.GetMemberType(), clrProperty, configurationSource, configurationSource);

        private InternalPropertyBuilder Property(
            [NotNull] string propertyName,
            [CanBeNull] Type propertyType,
            [CanBeNull] MemberInfo memberInfo,
            ConfigurationSource? configurationSource,
            ConfigurationSource? typeConfigurationSource)
        {
            IEnumerable<Property> propertiesToDetach = null;
            var existingProperty = Metadata.FindProperty(propertyName);
            if (existingProperty != null)
            {
                if (existingProperty.DeclaringEntityType != Metadata)
                {
                    if (memberInfo != null
                        && existingProperty.GetIdentifyingMemberInfo() == null)
                    {
                        if (configurationSource.Overrides(existingProperty.GetConfigurationSource()))
                        {
                            propertiesToDetach = new[] { existingProperty };
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        if (!IsIgnored(propertyName, configurationSource))
                        {
                            Metadata.Unignore(propertyName);
                        }

                        return existingProperty.DeclaringEntityType.Builder
                            .Property(existingProperty, propertyName, propertyType, memberInfo, configurationSource, typeConfigurationSource);
                    }
                }
            }
            else
            {
                if (IsIgnored(propertyName, configurationSource))
                {
                    return null;
                }

                foreach (var conflictingServiceProperty in Metadata.FindServicePropertiesInHierarchy(propertyName))
                {
                    if (!configurationSource.Overrides(conflictingServiceProperty.GetConfigurationSource()))
                    {
                        return null;
                    }
                }

                foreach (var conflictingNavigation in Metadata.FindNavigationsInHierarchy(propertyName))
                {
                    var foreignKey = conflictingNavigation.ForeignKey;

                    var navigationConfigurationSource = conflictingNavigation.IsDependentToPrincipal()
                        ? foreignKey.GetDependentToPrincipalConfigurationSource()
                        : foreignKey.GetPrincipalToDependentConfigurationSource();
                    if (!configurationSource.Overrides(navigationConfigurationSource))
                    {
                        return null;
                    }

                    if (navigationConfigurationSource == ConfigurationSource.Explicit)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.PropertyCalledOnNavigation(propertyName, Metadata.DisplayName()));
                    }
                }

                Metadata.Unignore(propertyName);

                propertiesToDetach = Metadata.FindDerivedProperties(propertyName);
            }

            InternalPropertyBuilder builder;
            using (Metadata.Model.ConventionDispatcher.StartBatch())
            {
                var detachedProperties = propertiesToDetach == null ? null : DetachProperties(propertiesToDetach);

                builder = Property(
                    existingProperty, propertyName, propertyType, memberInfo, configurationSource, typeConfigurationSource);

                detachedProperties?.Attach(this);
            }

            return builder != null
                   && builder.Metadata.Builder == null
                ? Metadata.FindProperty(propertyName)?.Builder
                : builder;
        }

        private InternalPropertyBuilder Property(
            [CanBeNull] Property existingProperty,
            [NotNull] string propertyName,
            [CanBeNull] Type propertyType,
            [CanBeNull] MemberInfo clrProperty,
            ConfigurationSource? configurationSource,
            ConfigurationSource? typeConfigurationSource)
        {
            Property property;
            if (existingProperty == null)
            {
                if (!configurationSource.HasValue)
                {
                    return null;
                }

                using (ModelBuilder.Metadata.ConventionDispatcher.StartBatch())
                {
                    foreach (var conflictingServiceProperty in Metadata.FindServicePropertiesInHierarchy(propertyName))
                    {
                        Metadata.RemoveServiceProperty(conflictingServiceProperty.Name);
                    }

                    foreach (var conflictingNavigation in Metadata.FindNavigationsInHierarchy(propertyName))
                    {
                        var foreignKey = conflictingNavigation.ForeignKey;
                        if (foreignKey.GetConfigurationSource() == ConfigurationSource.Convention)
                        {
                            foreignKey.DeclaringEntityType.Builder.RemoveForeignKey(foreignKey, ConfigurationSource.Convention);
                        }
                        else
                        {
                            if (conflictingNavigation.IsDependentToPrincipal())
                            {
                                if (foreignKey.Builder.DependentToPrincipal((string)null, configurationSource.Value) == null)
                                {
                                    return null;
                                }
                            }
                            else
                            {
                                if (foreignKey.Builder.PrincipalToDependent((string)null, configurationSource.Value) == null)
                                {
                                    return null;
                                }
                            }
                        }
                    }

                    property = clrProperty != null
                        ? Metadata.AddProperty(clrProperty, configurationSource.Value)
                        : Metadata.AddProperty(propertyName, propertyType, configurationSource.Value, typeConfigurationSource);
                }
            }
            else
            {
                if ((propertyType != null
                     && propertyType != existingProperty.ClrType)
                    || (clrProperty?.IsSameAs(existingProperty.GetIdentifyingMemberInfo()) == false))
                {
                    if (!configurationSource.HasValue
                        || !configurationSource.Value.Overrides(existingProperty.GetConfigurationSource()))
                    {
                        return null;
                    }

                    using (Metadata.Model.ConventionDispatcher.StartBatch())
                    {
                        var detachedProperties = DetachProperties(new[] { existingProperty });

                        property = clrProperty != null
                            ? Metadata.AddProperty(clrProperty, configurationSource.Value)
                            : Metadata.AddProperty(propertyName, propertyType, configurationSource.Value, typeConfigurationSource);

                        detachedProperties.Attach(this);
                    }
                }
                else
                {
                    if (configurationSource.HasValue)
                    {
                        existingProperty.UpdateConfigurationSource(configurationSource.Value);
                    }

                    if (propertyType != null
                        && typeConfigurationSource.HasValue)
                    {
                        existingProperty.UpdateTypeConfigurationSource(typeConfigurationSource.Value);
                    }

                    return existingProperty.Builder;
                }
            }

            return property.Builder;
        }

        private bool CanRemoveProperty(
            [NotNull] Property property, ConfigurationSource configurationSource, bool canOverrideSameSource = true)
        {
            Check.NotNull(property, nameof(property));
            Debug.Assert(property.DeclaringEntityType == Metadata);

            var currentConfigurationSource = property.GetConfigurationSource();
            return configurationSource.Overrides(currentConfigurationSource)
                   && (canOverrideSameSource || (configurationSource != currentConfigurationSource));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalServicePropertyBuilder ServiceProperty(
            [NotNull] MemberInfo memberInfo, ConfigurationSource configurationSource)
        {
            var propertyName = memberInfo.GetSimpleMemberName();
            if (IsIgnored(propertyName, configurationSource))
            {
                return null;
            }

            Metadata.Unignore(propertyName);

            var existingProperty = Metadata.FindServiceProperty(propertyName);
            if (existingProperty != null)
            {
                if (existingProperty.DeclaringEntityType != Metadata)
                {
                    if (!configurationSource.Overrides(existingProperty.GetConfigurationSource()))
                    {
                        return null;
                    }
                }
                else
                {
                    if (existingProperty.PropertyInfo == memberInfo)
                    {
                        existingProperty.UpdateConfigurationSource(configurationSource);
                        return existingProperty.Builder;
                    }

                    if (!configurationSource.Overrides(existingProperty.GetConfigurationSource()))
                    {
                        return null;
                    }
                }
            }
            else
            {
                foreach (var conflictingServiceProperty in Metadata.FindDerivedServiceProperties(propertyName))
                {
                    if (!configurationSource.Overrides(conflictingServiceProperty.GetConfigurationSource()))
                    {
                        return null;
                    }
                }

                foreach (var conflictingProperty in Metadata.FindPropertiesInHierarchy(propertyName))
                {
                    if (!configurationSource.Overrides(conflictingProperty.GetConfigurationSource()))
                    {
                        return null;
                    }
                }

                foreach (var conflictingNavigation in Metadata.FindNavigationsInHierarchy(propertyName))
                {
                    var foreignKey = conflictingNavigation.ForeignKey;

                    if (!configurationSource.Overrides(
                        conflictingNavigation.IsDependentToPrincipal()
                            ? foreignKey.GetDependentToPrincipalConfigurationSource()
                            : foreignKey.GetPrincipalToDependentConfigurationSource()))
                    {
                        return null;
                    }
                }
            }

            using (Metadata.Model.ConventionDispatcher.StartBatch())
            {
                using (ModelBuilder.Metadata.ConventionDispatcher.StartBatch())
                {
                    foreach (var conflictingServiceProperty in Metadata.FindServicePropertiesInHierarchy(propertyName))
                    {
                        Metadata.RemoveServiceProperty(conflictingServiceProperty.Name);
                    }

                    if (existingProperty == null)
                    {
                        foreach (var conflictingProperty in Metadata.FindPropertiesInHierarchy(propertyName))
                        {
                            Metadata.RemoveProperty(conflictingProperty.Name);
                        }

                        foreach (var conflictingNavigation in Metadata.FindNavigationsInHierarchy(propertyName))
                        {
                            var foreignKey = conflictingNavigation.ForeignKey;
                            if (foreignKey.GetConfigurationSource() == ConfigurationSource.Convention)
                            {
                                foreignKey.DeclaringEntityType.Builder.RemoveForeignKey(foreignKey, ConfigurationSource.Convention);
                            }
                            else
                            {
                                if (conflictingNavigation.IsDependentToPrincipal())
                                {
                                    if (foreignKey.Builder.DependentToPrincipal((string)null, configurationSource) == null)
                                    {
                                        return null;
                                    }
                                }
                                else
                                {
                                    if (foreignKey.Builder.PrincipalToDependent((string)null, configurationSource) == null)
                                    {
                                        return null;
                                    }
                                }
                            }
                        }
                    }

                    return Metadata.AddServiceProperty(memberInfo, configurationSource).Builder;
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool CanAddNavigation([NotNull] string navigationName, ConfigurationSource configurationSource)
            => !IsIgnored(navigationName, configurationSource)
               && !Metadata.FindNavigationsInHierarchy(navigationName).Any();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool CanAddOrReplaceNavigation([NotNull] string navigationName, ConfigurationSource configurationSource)
            => !IsIgnored(navigationName, configurationSource)
               && Metadata.FindNavigationsInHierarchy(navigationName).All(
                   n => n.ForeignKey.Builder.CanSetNavigation((string)null, n.IsDependentToPrincipal(), configurationSource));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsIgnored([NotNull] string name, ConfigurationSource? configurationSource)
        {
            Check.NotEmpty(name, nameof(name));

            var ignoredConfigurationSource = Metadata.FindIgnoredMemberConfigurationSource(name);
            return !configurationSource.HasValue
                   || !configurationSource.Value.Overrides(ignoredConfigurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool CanRemoveForeignKey([NotNull] ForeignKey foreignKey, ConfigurationSource configurationSource)
        {
            Debug.Assert(foreignKey.DeclaringEntityType == Metadata);

            var currentConfigurationSource = foreignKey.GetConfigurationSource();
            return configurationSource.Overrides(currentConfigurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Ignore([NotNull] string name, ConfigurationSource configurationSource)
        {
            var ignoredConfigurationSource = Metadata.FindIgnoredMemberConfigurationSource(name);
            if (ignoredConfigurationSource.HasValue
                && ignoredConfigurationSource.Value.Overrides(configurationSource))
            {
                return true;
            }

            using (Metadata.Model.ConventionDispatcher.StartBatch())
            {
                Metadata.Ignore(name, configurationSource);

                var navigation = Metadata.FindNavigation(name);
                if (navigation != null)
                {
                    var foreignKey = navigation.ForeignKey;
                    if (navigation.DeclaringEntityType != Metadata)
                    {
                        if (configurationSource == ConfigurationSource.Explicit)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.InheritedPropertyCannotBeIgnored(
                                    name, Metadata.DisplayName(), navigation.DeclaringEntityType.DisplayName()));
                        }

                        return false;
                    }

                    var isDependent = navigation.IsDependentToPrincipal();
                    var navigationConfigurationSource = isDependent
                        ? foreignKey.GetDependentToPrincipalConfigurationSource()
                        : foreignKey.GetPrincipalToDependentConfigurationSource();
                    if (foreignKey.GetConfigurationSource() != navigationConfigurationSource)
                    {
                        if (foreignKey.Builder.Navigations(
                                isDependent ? PropertyIdentity.None : (PropertyIdentity?)null,
                                isDependent ? (PropertyIdentity?)null : PropertyIdentity.None,
                                configurationSource) == null)
                        {
                            Metadata.Unignore(name);
                            return false;
                        }
                    }
                    else if (foreignKey.DeclaringEntityType.Builder.RemoveForeignKey(
                                 foreignKey, configurationSource, canOverrideSameSource: configurationSource == ConfigurationSource.Explicit) == null)
                    {
                        Metadata.Unignore(name);
                        return false;
                    }
                }
                else
                {
                    var property = Metadata.FindProperty(name);
                    if (property != null)
                    {
                        if (property.DeclaringEntityType != Metadata)
                        {
                            if (configurationSource == ConfigurationSource.Explicit)
                            {
                                throw new InvalidOperationException(
                                    CoreStrings.InheritedPropertyCannotBeIgnored(
                                        name, Metadata.DisplayName(), property.DeclaringEntityType.DisplayName()));
                            }

                            return false;
                        }

                        if (property.DeclaringEntityType.Builder.RemoveProperty(
                                property, configurationSource, canOverrideSameSource: configurationSource == ConfigurationSource.Explicit) == null)
                        {
                            Metadata.Unignore(name);
                            return false;
                        }
                    }
                    else
                    {
                        var serviceProperty = Metadata.FindServiceProperty(name);
                        if (serviceProperty != null)
                        {
                            if (serviceProperty.DeclaringEntityType != Metadata)
                            {
                                if (configurationSource == ConfigurationSource.Explicit)
                                {
                                    throw new InvalidOperationException(
                                        CoreStrings.InheritedPropertyCannotBeIgnored(
                                            name, Metadata.DisplayName(), serviceProperty.DeclaringEntityType.DisplayName()));
                                }

                                return false;
                            }

                            if (!configurationSource.Overrides(serviceProperty.GetConfigurationSource()))
                            {
                                Metadata.Unignore(name);
                                return false;
                            }

                            serviceProperty.DeclaringEntityType.RemoveServiceProperty(name);
                        }
                    }
                }

                foreach (var derivedType in Metadata.GetDerivedTypes())
                {
                    var derivedNavigation = derivedType.FindDeclaredNavigation(name);
                    if (derivedNavigation != null)
                    {
                        var foreignKey = derivedNavigation.ForeignKey;
                        foreignKey.DeclaringEntityType.Builder.RemoveForeignKey(foreignKey, configurationSource, canOverrideSameSource: false);
                    }
                    else
                    {
                        var derivedProperty = derivedType.FindDeclaredProperty(name);
                        if (derivedProperty != null)
                        {
                            derivedType.Builder.RemoveProperty(derivedProperty, configurationSource, canOverrideSameSource: false);
                        }
                        else
                        {
                            var derivedServiceProperty = derivedType.FindServiceProperty(name);
                            if (derivedServiceProperty != null)
                            {
                                if (configurationSource.Overrides(derivedServiceProperty.GetConfigurationSource()))
                                {
                                    derivedServiceProperty.DeclaringEntityType.RemoveServiceProperty(name);
                                }
                            }
                        }
                    }

                    var derivedIgnoredSource = derivedType.FindDeclaredIgnoredMemberConfigurationSource(name);
                    if (derivedIgnoredSource.HasValue
                        && configurationSource.Overrides(derivedIgnoredSource))
                    {
                        derivedType.Unignore(name);
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void HasQueryFilter([CanBeNull] LambdaExpression filter)
        {
            Metadata.QueryFilter = filter;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void HasDefiningQuery([CanBeNull] LambdaExpression query)
        {
            Metadata.DefiningQuery = query;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder HasBaseType([CanBeNull] Type baseEntityType, ConfigurationSource configurationSource)
        {
            if (baseEntityType == null)
            {
                return HasBaseType((EntityType)null, configurationSource);
            }

            var baseType = ModelBuilder.Entity(baseEntityType, configurationSource);
            return baseType == null
                ? null
                : HasBaseType(baseType.Metadata, configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder HasBaseType([CanBeNull] string baseEntityTypeName, ConfigurationSource configurationSource)
        {
            if (baseEntityTypeName == null)
            {
                return HasBaseType((EntityType)null, configurationSource);
            }

            var baseType = ModelBuilder.Entity(baseEntityTypeName, configurationSource);
            return baseType == null
                ? null
                : HasBaseType(baseType.Metadata, configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder HasBaseType(
            [CanBeNull] EntityType baseEntityType, ConfigurationSource configurationSource)
        {
            if (Metadata.BaseType == baseEntityType)
            {
                Metadata.HasBaseType(baseEntityType, configurationSource);
                return this;
            }

            if (!configurationSource.Overrides(Metadata.GetBaseTypeConfigurationSource()))
            {
                return null;
            }

            using (Metadata.Model.ConventionDispatcher.StartBatch())
            {
                List<RelationshipSnapshot> detachedRelationships = null;
                PropertiesSnapshot detachedProperties = null;
                IReadOnlyList<(InternalKeyBuilder, ConfigurationSource?)> detachedKeys = null;
                // We use at least DataAnnotation as ConfigurationSource while removing to allow us
                // to remove metadata object which were defined in derived type
                // while corresponding annotations were present on properties in base type.
                var configurationSourceForRemoval = ConfigurationSource.DataAnnotation.Max(configurationSource);
                if (baseEntityType != null)
                {
                    if (Metadata.GetDeclaredKeys().Any(k => !configurationSourceForRemoval.Overrides(k.GetConfigurationSource())))
                    {
                        return null;
                    }

                    var relationshipsToBeDetached = FindConflictingRelationships(baseEntityType, configurationSourceForRemoval);
                    if (relationshipsToBeDetached == null)
                    {
                        return null;
                    }

                    var foreignKeysUsingKeyProperties = Metadata.GetDerivedForeignKeysInclusive()
                        .Where(fk => fk.Properties.Any(p => baseEntityType.FindProperty(p.Name)?.IsKey() == true)).ToList();

                    if (foreignKeysUsingKeyProperties.Any(
                        fk => !configurationSourceForRemoval.Overrides(fk.GetForeignKeyPropertiesConfigurationSource())))
                    {
                        return null;
                    }

                    foreach (var foreignKeyUsingKeyProperties in foreignKeysUsingKeyProperties)
                    {
                        foreignKeyUsingKeyProperties.Builder.HasForeignKey((IReadOnlyList<Property>)null, configurationSourceForRemoval);
                    }

                    foreach (var relationshipToBeRemoved in relationshipsToBeDetached)
                    {
                        if (detachedRelationships == null)
                        {
                            detachedRelationships = new List<RelationshipSnapshot>();
                        }

                        detachedRelationships.Add(DetachRelationship(relationshipToBeRemoved));
                    }

                    foreach (var key in Metadata.GetDeclaredKeys().ToList())
                    {
                        foreach (var referencingForeignKey in key.GetReferencingForeignKeys().ToList())
                        {
                            if (detachedRelationships == null)
                            {
                                detachedRelationships = new List<RelationshipSnapshot>();
                            }

                            detachedRelationships.Add(DetachRelationship(referencingForeignKey));
                        }
                    }

                    detachedKeys = DetachKeys(Metadata.GetDeclaredKeys());

                    var duplicatedProperties = baseEntityType.GetProperties()
                        .SelectMany(p => Metadata.FindDerivedPropertiesInclusive(p.Name))
                        .Where(p => p != null);

                    detachedProperties = DetachProperties(duplicatedProperties);

                    var propertiesToRemove = Metadata.GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredProperties())
                        .Where(
                            p => !p.GetConfigurationSource()
                                .Overrides(baseEntityType.FindIgnoredMemberConfigurationSource(p.Name)))
                        .ToList();
                    foreach (var property in propertiesToRemove)
                    {
                        property.DeclaringEntityType.Builder.RemoveProperty(property, ConfigurationSource.Explicit);
                    }

                    var duplicatedServiceProperties = baseEntityType.GetServiceProperties()
                        .SelectMany(p => Metadata.FindDerivedServicePropertiesInclusive(p.Name))
                        .Where(p => p != null)
                        .ToList();

                    foreach (var duplicatedServiceProperty in duplicatedServiceProperties)
                    {
                        duplicatedServiceProperty.DeclaringEntityType.RemoveServiceProperty(duplicatedServiceProperty.Name);
                    }

                    foreach (var ignoredMember in Metadata.GetIgnoredMembers().ToList())
                    {
                        var ignoredSource = Metadata.FindDeclaredIgnoredMemberConfigurationSource(ignoredMember);
                        var baseIgnoredSource = baseEntityType.FindIgnoredMemberConfigurationSource(ignoredMember);

                        if (baseIgnoredSource.HasValue
                            && baseIgnoredSource.Value.Overrides(ignoredSource))
                        {
                            Metadata.Unignore(ignoredMember);
                        }
                    }

                    baseEntityType.UpdateConfigurationSource(configurationSource);
                }

                List<InternalIndexBuilder> detachedIndexes = null;
                HashSet<Property> removedInheritedPropertiesToDuplicate = null;
                List<(string, ConfigurationSource)> membersToIgnore = null;
                if (Metadata.BaseType != null)
                {
                    var removedInheritedProperties = new HashSet<Property>(
                        Metadata.BaseType.GetProperties()
                            .Where(p => baseEntityType == null || baseEntityType.FindProperty(p.Name) != p));
                    if (removedInheritedProperties.Count != 0)
                    {
                        removedInheritedPropertiesToDuplicate = new HashSet<Property>();
                        foreach (var foreignKey in Metadata.GetDerivedForeignKeysInclusive()
                            .Where(fk => fk.Properties.Any(p => removedInheritedProperties.Contains(p))).ToList())
                        {
                            foreach (var property in foreignKey.Properties)
                            {
                                if (removedInheritedProperties.Contains(property))
                                {
                                    removedInheritedPropertiesToDuplicate.Add(property);
                                }
                            }

                            if (detachedRelationships == null)
                            {
                                detachedRelationships = new List<RelationshipSnapshot>();
                            }

                            detachedRelationships.Add(DetachRelationship(foreignKey));
                        }

                        foreach (var index in Metadata.GetDerivedIndexesInclusive()
                            .Where(i => i.Properties.Any(p => removedInheritedProperties.Contains(p))).ToList())
                        {
                            foreach (var property in index.Properties)
                            {
                                if (removedInheritedProperties.Contains(property))
                                {
                                    removedInheritedPropertiesToDuplicate.Add(property);
                                }
                            }

                            if (detachedIndexes == null)
                            {
                                detachedIndexes = new List<InternalIndexBuilder>();
                            }

                            detachedIndexes.Add(DetachIndex(index));
                        }

                        foreach (var key in Metadata.GetKeys()
                            .Where(
                                k => k.ReferencingForeignKeys != null
                                     && k.Properties.Any(p => removedInheritedProperties.Contains(p))).ToList())
                        {
                            foreach (var referencingForeignKey in key.ReferencingForeignKeys.ToList())
                            {
                                if (Metadata.IsAssignableFrom(referencingForeignKey.PrincipalEntityType))
                                {
                                    if (detachedRelationships == null)
                                    {
                                        detachedRelationships = new List<RelationshipSnapshot>();
                                    }

                                    detachedRelationships.Add(DetachRelationship(referencingForeignKey));
                                }
                            }
                        }
                    }

                    foreach (var ignoredMember in Metadata.BaseType.GetIgnoredMembers())
                    {
                        if (baseEntityType != null
                            && (baseEntityType.FindProperty(ignoredMember) != null
                                || baseEntityType.FindNavigation(ignoredMember) != null))
                        {
                            continue;
                        }

                        if (membersToIgnore == null)
                        {
                            membersToIgnore = new List<(string, ConfigurationSource)>();
                        }

                        membersToIgnore.Add(
                            (ignoredMember, Metadata.BaseType.FindDeclaredIgnoredMemberConfigurationSource(ignoredMember).Value));
                    }
                }

                Metadata.HasBaseType(baseEntityType, configurationSource);

                if (membersToIgnore != null)
                {
                    foreach (var ignoreTuple in membersToIgnore)
                    {
                        Ignore(ignoreTuple.Item1, ignoreTuple.Item2);
                        Metadata.Unignore(ignoreTuple.Item1);
                    }
                }

                if (removedInheritedPropertiesToDuplicate != null)
                {
                    foreach (var property in removedInheritedPropertiesToDuplicate)
                    {
                        property.Builder?.Attach(this);
                    }
                }

                detachedProperties?.Attach(this);

                if (detachedKeys != null)
                {
                    foreach (var detachedKeyTuple in detachedKeys)
                    {
                        detachedKeyTuple.Item1.Attach(Metadata.RootType().Builder, detachedKeyTuple.Item2);
                    }
                }

                if (detachedIndexes != null)
                {
                    foreach (var indexBuilderTuple in detachedIndexes)
                    {
                        indexBuilderTuple.Attach(indexBuilderTuple.Metadata.DeclaringEntityType.Builder);
                    }
                }

                if (detachedRelationships != null)
                {
                    foreach (var detachedRelationship in detachedRelationships)
                    {
                        detachedRelationship.Attach();
                    }
                }
            }

            return this;
        }

        internal static PropertiesSnapshot DetachProperties(IEnumerable<Property> propertiesToDetach)
        {
            var propertiesToDetachList = propertiesToDetach.ToList();
            if (propertiesToDetachList.Count == 0)
            {
                return null;
            }

            List<RelationshipSnapshot> detachedRelationships = null;
            foreach (var propertyToDetach in propertiesToDetachList)
            {
                foreach (var relationship in propertyToDetach.GetContainingForeignKeys().ToList())
                {
                    if (detachedRelationships == null)
                    {
                        detachedRelationships = new List<RelationshipSnapshot>();
                    }

                    detachedRelationships.Add(DetachRelationship(relationship));
                }
            }

            var detachedIndexes = DetachIndexes(propertiesToDetachList.SelectMany(p => p.GetContainingIndexes()).Distinct());

            var keysToDetach = propertiesToDetachList.SelectMany(p => p.GetContainingKeys()).Distinct().ToList();
            foreach (var key in keysToDetach)
            {
                foreach (var referencingForeignKey in key.GetReferencingForeignKeys().ToList())
                {
                    if (detachedRelationships == null)
                    {
                        detachedRelationships = new List<RelationshipSnapshot>();
                    }

                    detachedRelationships.Add(DetachRelationship(referencingForeignKey));
                }
            }

            var detachedKeys = DetachKeys(keysToDetach);

            var detachedProperties = new List<InternalPropertyBuilder>();
            foreach (var propertyToDetach in propertiesToDetachList)
            {
                var property = propertyToDetach.DeclaringEntityType.FindDeclaredProperty(propertyToDetach.Name);
                if (property != null)
                {
                    var entityTypeBuilder = property.DeclaringEntityType.Builder;
                    var propertyBuilder = property.Builder;
                    // Reset convention configuration
                    propertyBuilder.ValueGenerated(null, ConfigurationSource.Convention);
                    propertyBuilder.AfterSave(null, ConfigurationSource.Convention);
                    propertyBuilder.BeforeSave(null, ConfigurationSource.Convention);
                    ConfigurationSource? removedConfigurationSource;
                    if (entityTypeBuilder != null)
                    {
                        removedConfigurationSource = entityTypeBuilder
                            .RemoveProperty(property, property.GetConfigurationSource());
                    }
                    else
                    {
                        removedConfigurationSource = property.GetConfigurationSource();
                        property.DeclaringEntityType.RemoveProperty(property.Name);
                    }

                    Debug.Assert(removedConfigurationSource.HasValue);
                    detachedProperties.Add(propertyBuilder);
                }
            }

            return new PropertiesSnapshot(detachedProperties, detachedIndexes, detachedKeys, detachedRelationships);
        }

        private IEnumerable<ForeignKey> FindConflictingRelationships(
            EntityType baseEntityType,
            ConfigurationSource configurationSource)
        {
            var relationshipsToBeDetached = new HashSet<ForeignKey>();
            foreach (var navigation in Metadata.GetDerivedNavigationsInclusive())
            {
                if (!navigation.ForeignKey.GetConfigurationSource().Overrides(
                    baseEntityType.FindIgnoredMemberConfigurationSource(navigation.Name)))
                {
                    relationshipsToBeDetached.Add(navigation.ForeignKey);
                    continue;
                }

                var baseNavigation = baseEntityType.FindNavigation(navigation.Name);
                if (baseNavigation == null)
                {
                    continue;
                }

                // When reattached the FK will override the other one if not compatible
                if (navigation.ForeignKey.DeclaringEntityType.Builder
                    .CanRemoveForeignKey(navigation.ForeignKey, configurationSource))
                {
                    relationshipsToBeDetached.Add(baseNavigation.ForeignKey);
                }
                else if (baseNavigation.ForeignKey.DeclaringEntityType.Builder
                    .CanRemoveForeignKey(baseNavigation.ForeignKey, configurationSource))
                {
                    relationshipsToBeDetached.Add(navigation.ForeignKey);
                }
                else
                {
                    return null;
                }
            }

            return relationshipsToBeDetached;
        }

        private ConfigurationSource? RemoveProperty(
            Property property, ConfigurationSource configurationSource, bool canOverrideSameSource = true)
        {
            var currentConfigurationSource = property.GetConfigurationSource();
            if (!configurationSource.Overrides(currentConfigurationSource)
                || !(canOverrideSameSource || (configurationSource != currentConfigurationSource)))
            {
                return null;
            }

            using (Metadata.Model.ConventionDispatcher.StartBatch())
            {
                var detachedRelationships = property.GetContainingForeignKeys().ToList()
                    .Select(DetachRelationship).ToList();

                foreach (var key in property.GetContainingKeys().ToList())
                {
                    detachedRelationships.AddRange(
                        key.GetReferencingForeignKeys().ToList()
                            .Select(DetachRelationship));
                    var removed = RemoveKey(key, configurationSource);
                    Debug.Assert(removed.HasValue);
                }

                foreach (var index in property.GetContainingIndexes().ToList())
                {
                    var removed = RemoveIndex(index, configurationSource);
                    Debug.Assert(removed.HasValue);
                }

                if (property.Builder != null)
                {
                    var removedProperty = Metadata.RemoveProperty(property.Name);
                    Debug.Assert(removedProperty == property);
                }

                foreach (var relationshipSnapshot in detachedRelationships)
                {
                    relationshipSnapshot.Attach();
                }
            }

            return currentConfigurationSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static RelationshipSnapshot DetachRelationship([NotNull] ForeignKey foreignKey)
        {
            var snapshot = new RelationshipSnapshot(foreignKey.Builder, null);

            var relationshipConfigurationSource = foreignKey.DeclaringEntityType.Builder
                .RemoveForeignKey(foreignKey, foreignKey.GetConfigurationSource());
            Debug.Assert(relationshipConfigurationSource != null);

            return snapshot;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? RemoveForeignKey(
            [NotNull] ForeignKey foreignKey,
            ConfigurationSource configurationSource,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            bool canOverrideSameSource = true)
        {
            Debug.Assert(foreignKey.DeclaringEntityType == Metadata);

            var currentConfigurationSource = foreignKey.GetConfigurationSource();
            if (!configurationSource.Overrides(currentConfigurationSource)
                || !(canOverrideSameSource || (configurationSource != currentConfigurationSource)))
            {
                return null;
            }

            var removedForeignKey = Metadata.RemoveForeignKey(
                foreignKey.Properties, foreignKey.PrincipalKey, foreignKey.PrincipalEntityType);

            if (removedForeignKey == null)
            {
                return null;
            }

            Debug.Assert(removedForeignKey == foreignKey);

            RemoveShadowPropertiesIfUnused(foreignKey.Properties.Where(p => p.DeclaringEntityType.FindDeclaredProperty(p.Name) != null));
            foreignKey.PrincipalKey.DeclaringEntityType.Builder?.RemoveKeyIfUnused(foreignKey.PrincipalKey);

            return currentConfigurationSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static EntityType.Snapshot DetachAllMembers([NotNull] EntityType entityType)
        {
            if (entityType.Builder == null)
            {
                return null;
            }

            if (entityType.HasDefiningNavigation())
            {
                entityType.Model.AddDetachedEntityType(entityType.Name, entityType.DefiningNavigationName, entityType.DefiningEntityType.Name);
            }

            List<RelationshipSnapshot> detachedRelationships = null;
            foreach (var relationshipToBeDetached in entityType.GetDeclaredForeignKeys().ToList())
            {
                if (detachedRelationships == null)
                {
                    detachedRelationships = new List<RelationshipSnapshot>();
                }

                var detachedRelationship = DetachRelationship(relationshipToBeDetached);
                if (detachedRelationship.Relationship.Metadata.GetConfigurationSource().Overrides(ConfigurationSource.DataAnnotation)
                    || relationshipToBeDetached.IsOwnership)
                {
                    detachedRelationships.Add(detachedRelationship);
                }
            }

            List<(InternalKeyBuilder, ConfigurationSource?)> detachedKeys = null;
            foreach (var keyToDetach in entityType.GetDeclaredKeys().ToList())
            {
                foreach (var relationshipToBeDetached in keyToDetach.GetReferencingForeignKeys().ToList())
                {
                    if (detachedRelationships == null)
                    {
                        detachedRelationships = new List<RelationshipSnapshot>();
                    }

                    var detachedRelationship = DetachRelationship(relationshipToBeDetached);
                    if (detachedRelationship.Relationship.Metadata.GetConfigurationSource().Overrides(ConfigurationSource.DataAnnotation)
                        || relationshipToBeDetached.IsOwnership)
                    {
                        EntityType.Snapshot weakSnapshot = null;
                        var dependentEntityType = relationshipToBeDetached.DeclaringEntityType;
                        if (dependentEntityType.DefiningEntityType == entityType
                            && dependentEntityType.DefiningNavigationName == relationshipToBeDetached.PrincipalToDependent?.Name)
                        {
                            weakSnapshot = DetachAllMembers(dependentEntityType);
                            entityType.Model.Builder.RemoveEntityType(dependentEntityType, ConfigurationSource.Explicit);
                        }

                        detachedRelationship.WeakEntityTypeSnapshot = weakSnapshot;
                        detachedRelationships.Add(detachedRelationship);
                    }
                }

                if (keyToDetach.Builder == null)
                {
                    continue;
                }

                if (detachedKeys == null)
                {
                    detachedKeys = new List<(InternalKeyBuilder, ConfigurationSource?)>();
                }

                var detachedKey = DetachKey(keyToDetach);
                if (detachedKey.Item1.Metadata.GetConfigurationSource().Overrides(ConfigurationSource.Explicit))
                {
                    detachedKeys.Add(detachedKey);
                }
            }

            List<InternalIndexBuilder> detachedIndexes = null;
            foreach (var index in entityType.GetDeclaredIndexes().ToList())
            {
                if (detachedIndexes == null)
                {
                    detachedIndexes = new List<InternalIndexBuilder>();
                }

                var detachedIndex = DetachIndex(index);
                if (detachedIndex.Metadata.GetConfigurationSource().Overrides(ConfigurationSource.Explicit))
                {
                    detachedIndexes.Add(detachedIndex);
                }
            }

            var detachedProperties = DetachProperties(entityType.GetDeclaredProperties());

            return new EntityType.Snapshot(entityType, detachedProperties, detachedIndexes, detachedKeys, detachedRelationships);
        }

        private void RemoveKeyIfUnused(Key key)
        {
            if (Metadata.FindPrimaryKey() == key)
            {
                return;
            }

            if (key.GetReferencingForeignKeys().Any())
            {
                return;
            }

            RemoveKey(key, ConfigurationSource.Convention);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void RemoveShadowPropertiesIfUnused([NotNull] IEnumerable<Property> properties)
        {
            foreach (var property in properties.ToList())
            {
                if (property?.IsShadowProperty == true)
                {
                    RemovePropertyIfUnused(property);
                }
            }
        }

        private static void RemovePropertyIfUnused(Property property)
        {
            if (!property.DeclaringEntityType.Builder.CanRemoveProperty(property, ConfigurationSource.Convention))
            {
                return;
            }

            if (property.GetContainingIndexes().Any())
            {
                return;
            }

            if (property.GetContainingForeignKeys().Any())
            {
                return;
            }

            if (property.GetContainingKeys().Any())
            {
                return;
            }

            var removedProperty = property.DeclaringEntityType.RemoveProperty(property.Name);
            Debug.Assert(removedProperty == property);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalIndexBuilder HasIndex([NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => HasIndex(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalIndexBuilder HasIndex([NotNull] IReadOnlyList<PropertyInfo> clrProperties, ConfigurationSource configurationSource)
            => HasIndex(GetOrCreateProperties(clrProperties, configurationSource), configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalIndexBuilder HasIndex([CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            if (properties == null)
            {
                return null;
            }

            List<InternalIndexBuilder> detachedIndexes = null;
            var existingIndex = Metadata.FindIndex(properties);
            if (existingIndex == null)
            {
                detachedIndexes = Metadata.FindDerivedIndexes(properties).ToList().Select(DetachIndex).ToList();
            }
            else if (existingIndex.DeclaringEntityType != Metadata)
            {
                return existingIndex.DeclaringEntityType.Builder.HasIndex(existingIndex, properties, configurationSource);
            }

            var indexBuilder = HasIndex(existingIndex, properties, configurationSource);

            if (detachedIndexes != null)
            {
                foreach (var indexBuilderTuple in detachedIndexes)
                {
                    indexBuilderTuple.Attach(indexBuilderTuple.Metadata.DeclaringEntityType.Builder);
                }
            }

            return indexBuilder;
        }

        private InternalIndexBuilder HasIndex(
            Index index, IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            if (index == null)
            {
                index = Metadata.AddIndex(properties, configurationSource);
            }
            else
            {
                index.UpdateConfigurationSource(configurationSource);
            }

            return index?.Builder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? RemoveIndex([NotNull] Index index, ConfigurationSource configurationSource)
        {
            var currentConfigurationSource = index.GetConfigurationSource();
            if (!configurationSource.Overrides(currentConfigurationSource))
            {
                return null;
            }

            var removedIndex = Metadata.RemoveIndex(index.Properties);
            Debug.Assert(removedIndex == index);

            RemoveShadowPropertiesIfUnused(index.Properties);

            return currentConfigurationSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static List<InternalIndexBuilder> DetachIndexes(IEnumerable<Index> indexesToDetach)
        {
            var indexesToDetachList = (indexesToDetach as List<Index>) ?? indexesToDetach.ToList();
            if (indexesToDetachList.Count == 0)
            {
                return null;
            }

            var detachedIndexes = new List<InternalIndexBuilder>();
            foreach (var indexToDetach in indexesToDetachList)
            {
                var detachedIndex = DetachIndex(indexToDetach);
                detachedIndexes.Add(detachedIndex);
            }

            return detachedIndexes;
        }

        private static InternalIndexBuilder DetachIndex(Index indexToDetach)
        {
            var entityTypeBuilder = indexToDetach.DeclaringEntityType.Builder;
            var indexBuilder = indexToDetach.Builder;
            var removedConfigurationSource = entityTypeBuilder.RemoveIndex(indexToDetach, indexToDetach.GetConfigurationSource());
            Debug.Assert(removedConfigurationSource != null);
            return indexBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] string principalEntityTypeName,
            [NotNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(principalEntityTypeName, nameof(principalEntityTypeName));
            Check.NotEmpty(propertyNames, nameof(propertyNames));

            var principalType = ModelBuilder.Entity(principalEntityTypeName, configurationSource);
            return principalType == null
                ? null
                : HasForeignKeyInternal(
                    principalType,
                    GetOrCreateProperties(
                        propertyNames, configurationSource, principalType.Metadata.FindPrimaryKey()?.Properties, useDefaultType: true),
                    null,
                    configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] string principalEntityTypeName,
            [NotNull] IReadOnlyList<string> propertyNames,
            [NotNull] Key principalKey,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(principalEntityTypeName, nameof(principalEntityTypeName));
            Check.NotEmpty(propertyNames, nameof(propertyNames));

            var principalType = ModelBuilder.Entity(principalEntityTypeName, configurationSource);
            return principalType == null
                ? null
                : HasForeignKeyInternal(
                    principalType,
                    GetOrCreateProperties(propertyNames, configurationSource, principalKey.Properties, useDefaultType: true),
                    principalKey,
                    configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] Type principalClrType,
            [NotNull] IReadOnlyList<PropertyInfo> clrProperties,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(principalClrType, nameof(principalClrType));
            Check.NotEmpty(clrProperties, nameof(clrProperties));

            var principalType = ModelBuilder.Entity(principalClrType, configurationSource);
            return principalType == null
                ? null
                : HasForeignKeyInternal(
                    principalType,
                    GetOrCreateProperties(clrProperties, configurationSource),
                    null,
                    configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] Type principalClrType,
            [NotNull] IReadOnlyList<PropertyInfo> clrProperties,
            [NotNull] Key principalKey,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(principalClrType, nameof(principalClrType));
            Check.NotEmpty(clrProperties, nameof(clrProperties));

            var principalType = ModelBuilder.Entity(principalClrType, configurationSource);
            return principalType == null
                ? null
                : HasForeignKeyInternal(
                    principalType,
                    GetOrCreateProperties(clrProperties, configurationSource),
                    principalKey,
                    configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [NotNull] IReadOnlyList<Property> dependentProperties,
            ConfigurationSource configurationSource)
            => HasForeignKeyInternal(
                principalEntityTypeBuilder,
                GetActualProperties(dependentProperties, configurationSource),
                null,
                configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder HasForeignKey(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [NotNull] IReadOnlyList<Property> dependentProperties,
            [NotNull] Key principalKey,
            ConfigurationSource configurationSource)
            => HasForeignKeyInternal(
                principalEntityTypeBuilder,
                GetActualProperties(dependentProperties, configurationSource),
                principalKey,
                configurationSource);

        private InternalRelationshipBuilder HasForeignKeyInternal(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [CanBeNull] IReadOnlyList<Property> dependentProperties,
            [CanBeNull] Key principalKey,
            ConfigurationSource configurationSource)
        {
            if (dependentProperties == null)
            {
                return null;
            }

            var newRelationship = RelationshipInternal(principalEntityTypeBuilder, principalKey, configurationSource);
            var relationship = newRelationship.HasForeignKey(dependentProperties, configurationSource);
            if (relationship == null
                && newRelationship.Metadata.Builder != null)
            {
                RemoveForeignKey(newRelationship.Metadata, configurationSource);
            }

            return relationship;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] InternalEntityTypeBuilder targetEntityTypeBuilder,
            [CanBeNull] string navigationToTargetName,
            [CanBeNull] string inverseNavigationName,
            ConfigurationSource configurationSource,
            bool setTargetAsPrincipal = false)
            => Relationship(
                Check.NotNull(targetEntityTypeBuilder, nameof(targetEntityTypeBuilder)),
                PropertyIdentity.Create(navigationToTargetName),
                PropertyIdentity.Create(inverseNavigationName),
                setTargetAsPrincipal,
                configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] InternalEntityTypeBuilder targetEntityTypeBuilder,
            [CanBeNull] MemberInfo navigationToTarget,
            [CanBeNull] MemberInfo inverseNavigation,
            ConfigurationSource configurationSource,
            bool setTargetAsPrincipal = false)
            => Relationship(
                Check.NotNull(targetEntityTypeBuilder, nameof(targetEntityTypeBuilder)),
                PropertyIdentity.Create(navigationToTarget),
                PropertyIdentity.Create(inverseNavigation),
                setTargetAsPrincipal,
                configurationSource);

        private InternalRelationshipBuilder Relationship(
            InternalEntityTypeBuilder targetEntityTypeBuilder,
            PropertyIdentity? navigationToTarget,
            PropertyIdentity? inverseNavigation,
            bool setTargetAsPrincipal,
            ConfigurationSource configurationSource,
            bool? required = null)
        {
            Check.NotNull(targetEntityTypeBuilder, nameof(targetEntityTypeBuilder));

            Debug.Assert(
                navigationToTarget != null
                || inverseNavigation != null);

            var navigationProperty = navigationToTarget?.Property;
            if (inverseNavigation == null
                && navigationProperty?.GetMemberType().GetTypeInfo().IsAssignableFrom(
                    targetEntityTypeBuilder.Metadata.ClrType.GetTypeInfo()) == false)
            {
                // Only one nav specified and it can't be the nav to principal
                return targetEntityTypeBuilder.Relationship(
                    this, null, navigationToTarget, setTargetAsPrincipal, configurationSource, required);
            }

            var existingRelationship = InternalRelationshipBuilder.FindCurrentRelationshipBuilder(
                targetEntityTypeBuilder.Metadata,
                Metadata,
                navigationToTarget,
                inverseNavigation,
                dependentProperties: null,
                principalProperties: null);
            if (existingRelationship != null)
            {
                var shouldInvertNavigations = false;
                if (navigationToTarget != null)
                {
                    if (navigationToTarget.Value.Name == existingRelationship.Metadata.DependentToPrincipal?.Name)
                    {
                        existingRelationship.Metadata.UpdateDependentToPrincipalConfigurationSource(configurationSource);
                    }
                    else if (setTargetAsPrincipal)
                    {
                        shouldInvertNavigations = true;
                    }
                    else
                    {
                        existingRelationship.Metadata.UpdatePrincipalToDependentConfigurationSource(configurationSource);
                    }

                    if (navigationToTarget.Value.Name != null)
                    {
                        Metadata.Unignore(navigationToTarget.Value.Name);
                    }
                }

                if (inverseNavigation != null)
                {
                    if (inverseNavigation.Value.Name == existingRelationship.Metadata.PrincipalToDependent?.Name)
                    {
                        existingRelationship.Metadata.UpdatePrincipalToDependentConfigurationSource(configurationSource);
                    }
                    else if (setTargetAsPrincipal)
                    {
                        shouldInvertNavigations = true;
                    }
                    else
                    {
                        existingRelationship.Metadata.UpdateDependentToPrincipalConfigurationSource(configurationSource);
                    }

                    if (inverseNavigation.Value.Name != null)
                    {
                        targetEntityTypeBuilder.Metadata.Unignore(inverseNavigation.Value.Name);
                    }
                }

                existingRelationship.Metadata.UpdateConfigurationSource(configurationSource);

                if (!shouldInvertNavigations)
                {
                    if (setTargetAsPrincipal)
                    {
                        existingRelationship.Metadata.UpdatePrincipalEndConfigurationSource(configurationSource);
                        if (required.HasValue)
                        {
                            existingRelationship.IsRequired(required.Value, configurationSource);
                        }
                    }

                    return existingRelationship;
                }
            }

            existingRelationship = InternalRelationshipBuilder.FindCurrentRelationshipBuilder(
                Metadata,
                targetEntityTypeBuilder.Metadata,
                inverseNavigation,
                navigationToTarget,
                dependentProperties: null,
                principalProperties: null);
            if (existingRelationship != null)
            {
                if (navigationToTarget != null)
                {
                    existingRelationship.Metadata.UpdatePrincipalToDependentConfigurationSource(configurationSource);
                    if (navigationToTarget.Value.Name != null)
                    {
                        Metadata.Unignore(navigationToTarget.Value.Name);
                    }
                }

                if (inverseNavigation != null)
                {
                    existingRelationship.Metadata.UpdateDependentToPrincipalConfigurationSource(configurationSource);
                    if (inverseNavigation.Value.Name != null)
                    {
                        targetEntityTypeBuilder.Metadata.Unignore(inverseNavigation.Value.Name);
                    }
                }

                existingRelationship.Metadata.UpdateConfigurationSource(configurationSource);
                if (!setTargetAsPrincipal)
                {
                    return existingRelationship;
                }
            }

            InternalRelationshipBuilder relationship;
            InternalRelationshipBuilder newRelationship = null;
            using (var batcher = Metadata.Model.ConventionDispatcher.StartBatch())
            {
                if (existingRelationship != null)
                {
                    relationship = existingRelationship;
                }
                else
                {
                    if (setTargetAsPrincipal
                        || targetEntityTypeBuilder.Metadata.DefiningEntityType != Metadata)
                    {
                        newRelationship = CreateForeignKey(
                            targetEntityTypeBuilder,
                            dependentProperties: null,
                            principalKey: null,
                            navigationToPrincipalName: navigationProperty?.GetSimpleMemberName(),
                            isRequired: required,
                            configurationSource: configurationSource);
                    }
                    else
                    {
                        var navigation = navigationToTarget;
                        navigationToTarget = inverseNavigation;
                        inverseNavigation = navigation;

                        navigationProperty = navigationToTarget?.Property;

                        newRelationship = targetEntityTypeBuilder.CreateForeignKey(
                            this,
                            dependentProperties: null,
                            principalKey: null,
                            navigationToPrincipalName: navigationProperty?.GetSimpleMemberName(),
                            isRequired: null,
                            configurationSource: configurationSource);
                    }

                    relationship = newRelationship;
                }

                if (setTargetAsPrincipal)
                {
                    relationship = relationship
                        .RelatedEntityTypes(targetEntityTypeBuilder.Metadata, Metadata, configurationSource);
                }

                var inverseProperty = inverseNavigation?.Property;
                if (inverseNavigation == null)
                {
                    relationship = navigationProperty != null
                        ? relationship.DependentToPrincipal(navigationProperty, configurationSource)
                        : relationship.DependentToPrincipal(navigationToTarget.Value.Name, configurationSource);
                }
                else if (navigationToTarget == null)
                {
                    relationship = inverseProperty != null
                        ? relationship.PrincipalToDependent(inverseProperty, configurationSource)
                        : relationship.PrincipalToDependent(inverseNavigation.Value.Name, configurationSource);
                }
                else
                {
                    relationship = navigationProperty != null || inverseProperty != null
                        ? relationship.Navigations(navigationProperty, inverseProperty, configurationSource)
                        : relationship.Navigations(navigationToTarget.Value.Name, inverseNavigation.Value.Name, configurationSource);
                }

                if (relationship != null)
                {
                    relationship = batcher.Run(relationship);
                }
            }

            if (relationship != null
                && ((navigationToTarget != null
                     && relationship.Metadata.DependentToPrincipal?.Name != navigationToTarget.Value.Name)
                    || (inverseNavigation != null
                        && relationship.Metadata.PrincipalToDependent?.Name != inverseNavigation.Value.Name))
                && ((inverseNavigation != null
                     && relationship.Metadata.DependentToPrincipal?.Name != inverseNavigation.Value.Name)
                    || (navigationToTarget != null
                        && relationship.Metadata.PrincipalToDependent?.Name != navigationToTarget.Value.Name)))
            {
                relationship = null;
            }

            if (relationship == null)
            {
                if (newRelationship?.Metadata.Builder != null)
                {
                    newRelationship.Metadata.DeclaringEntityType.Builder.RemoveForeignKey(newRelationship.Metadata, configurationSource);
                }

                return null;
            }

            return relationship;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] EntityType principalEntityType,
            ConfigurationSource configurationSource)
            => RelationshipInternal(principalEntityType.Builder, principalKey: null, configurationSource: configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] EntityType principalEntityType,
            [NotNull] Key principalKey,
            ConfigurationSource configurationSource)
            => RelationshipInternal(principalEntityType.Builder, principalKey, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            ConfigurationSource configurationSource)
            => RelationshipInternal(principalEntityTypeBuilder, principalKey: null, configurationSource: configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Relationship(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [NotNull] Key principalKey,
            ConfigurationSource configurationSource)
            => RelationshipInternal(principalEntityTypeBuilder, principalKey, configurationSource);

        private InternalRelationshipBuilder RelationshipInternal(
            InternalEntityTypeBuilder targetEntityTypeBuilder,
            Key principalKey,
            ConfigurationSource configurationSource)
        {
            InternalRelationshipBuilder relationship;
            InternalRelationshipBuilder newRelationship;
            using (var batch = Metadata.Model.ConventionDispatcher.StartBatch())
            {
                relationship = CreateForeignKey(
                    targetEntityTypeBuilder,
                    null,
                    principalKey,
                    null,
                    null,
                    configurationSource);

                newRelationship = relationship;
                if (principalKey != null)
                {
                    newRelationship = newRelationship.RelatedEntityTypes(targetEntityTypeBuilder.Metadata, Metadata, configurationSource)
                        ?.HasPrincipalKey(principalKey.Properties, configurationSource);
                }

                newRelationship = newRelationship == null ? null : batch.Run(newRelationship);
            }

            if (newRelationship == null)
            {
                if (relationship?.Metadata.Builder != null)
                {
                    relationship.Metadata.DeclaringEntityType.Builder.RemoveForeignKey(relationship.Metadata, configurationSource);
                }

                return null;
            }

            return newRelationship;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Owns(
            [NotNull] string targetEntityTypeName,
            [NotNull] string navigationName,
            ConfigurationSource configurationSource)
            => Owns(
                new TypeIdentity(targetEntityTypeName), PropertyIdentity.Create(navigationName),
                inverse: null, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Owns(
            [NotNull] string targetEntityTypeName,
            [NotNull] PropertyInfo navigationProperty,
            ConfigurationSource configurationSource)
            => Owns(
                new TypeIdentity(targetEntityTypeName), PropertyIdentity.Create(navigationProperty),
                inverse: null, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Owns(
            [NotNull] Type targetEntityType,
            [NotNull] string navigationName,
            ConfigurationSource configurationSource)
            => Owns(
                new TypeIdentity(targetEntityType, Metadata.Model), PropertyIdentity.Create(navigationName),
                inverse: null, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Owns(
            [NotNull] Type targetEntityType,
            [NotNull] PropertyInfo navigationProperty,
            ConfigurationSource configurationSource)
            => Owns(
                new TypeIdentity(targetEntityType, Metadata.Model), PropertyIdentity.Create(navigationProperty),
                inverse: null, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Owns(
            [NotNull] Type targetEntityType,
            [NotNull] MemberInfo navigationProperty,
            [CanBeNull] MemberInfo inverseProperty,
            ConfigurationSource configurationSource)
            => Owns(
                new TypeIdentity(targetEntityType, Metadata.Model),
                PropertyIdentity.Create(navigationProperty),
                PropertyIdentity.Create(inverseProperty),
                configurationSource);

        private InternalRelationshipBuilder Owns(
            in TypeIdentity targetEntityType,
            PropertyIdentity navigation,
            PropertyIdentity? inverse,
            ConfigurationSource configurationSource)
        {
            InternalEntityTypeBuilder ownedEntityType;
            InternalRelationshipBuilder relationship;
            using (var batch = Metadata.Model.ConventionDispatcher.StartBatch())
            {
                var existingNavigation = Metadata.FindNavigation(navigation.Name);
                if (existingNavigation != null)
                {
                    if (existingNavigation.GetTargetType().Name == targetEntityType.Name)
                    {
                        var existingOwnedEntityType = existingNavigation.ForeignKey.DeclaringEntityType;
                        if (existingOwnedEntityType.HasDefiningNavigation())
                        {
                            if (targetEntityType.Type != null)
                            {
                                ModelBuilder.Entity(
                                    targetEntityType.Type,
                                    existingOwnedEntityType.DefiningNavigationName,
                                    existingOwnedEntityType.DefiningEntityType,
                                    configurationSource);
                            }
                            else
                            {
                                ModelBuilder.Entity(
                                    targetEntityType.Name,
                                    existingOwnedEntityType.DefiningNavigationName,
                                    existingOwnedEntityType.DefiningEntityType,
                                    configurationSource);
                            }
                        }
                        else
                        { 
                            if (targetEntityType.Type != null)
                            {
                                ModelBuilder.Entity(targetEntityType.Type, configurationSource, owned: true);
                            }
                            else
                            {
                                ModelBuilder.Entity(targetEntityType.Name, configurationSource, owned: true);
                            }
                        }

                        var ownershipBuilder = existingNavigation.ForeignKey.Builder;
                        ownershipBuilder = ownershipBuilder
                            .IsRequired(true, configurationSource)
                            ?.RelatedEntityTypes(
                                Metadata, ownershipBuilder.Metadata.FindNavigationsFromInHierarchy(Metadata).Single().GetTargetType(),
                                configurationSource)
                            ?.Navigations(inverse, navigation, configurationSource)
                            ?.IsOwnership(true, configurationSource);

                        return ownershipBuilder == null ? null : batch.Run(ownershipBuilder);
                    }

                    if (existingNavigation.ForeignKey.DeclaringEntityType.Builder
                            .RemoveForeignKey(existingNavigation.ForeignKey, configurationSource) == null)
                    {
                        return null;
                    }
                }

                var principalBuilder = this;
                var targetTypeName = targetEntityType.Name;
                var targetType = targetEntityType.Type;
                if (targetType == null)
                {
                    var memberType = existingNavigation?.GetIdentifyingMemberInfo()?.GetMemberType();
                    if (memberType != null)
                    {
                        targetType = memberType.TryGetSequenceType() ?? memberType;
                    }
                }

                ownedEntityType = targetType == null
                    ? ModelBuilder.Metadata.FindEntityType(targetTypeName)?.Builder
                    : ModelBuilder.Metadata.FindEntityType(targetType)?.Builder;
                if (ownedEntityType == null)
                {
                    if (Metadata.Model.EntityTypeShouldHaveDefiningNavigation(targetTypeName))
                    {
                        if (!configurationSource.Overrides(ConfigurationSource.Explicit)
                            && (targetType == null
                                ? Metadata.IsInDefinitionPath(targetTypeName)
                                : Metadata.IsInDefinitionPath(targetType)))
                        {
                            return null;
                        }

                        ownedEntityType = targetType == null
                            ? ModelBuilder.Entity(targetTypeName, navigation.Name, Metadata, configurationSource)
                            : ModelBuilder.Entity(targetType, navigation.Name, Metadata, configurationSource);
                    }
                    else
                    {
                        if (ModelBuilder.IsIgnored(targetTypeName, configurationSource))
                        {
                            return null;
                        }

                        ModelBuilder.Metadata.Unignore(targetTypeName);

                        ownedEntityType = targetType == null
                            ? ModelBuilder.Entity(targetTypeName, configurationSource, owned: true)
                            : ModelBuilder.Entity(targetType, configurationSource, owned: true);
                    }

                    if (ownedEntityType == null)
                    {
                        return null;
                    }
                }
                else
                {
                    var otherOwnership = ownedEntityType.Metadata.FindDeclaredOwnership();
                    if (otherOwnership != null)
                    {
                        if (!configurationSource.Overrides(ConfigurationSource.Explicit)
                            && (targetType == null
                                ? Metadata.IsInDefinitionPath(targetTypeName)
                                : Metadata.IsInDefinitionPath(targetType)))
                        {
                            return null;
                        }

                        var newOtherOwnership = otherOwnership.Builder.IsWeakTypeDefinition(configurationSource);
                        if (newOtherOwnership == null)
                        {
                            return null;
                        }

                        if (otherOwnership.DeclaringEntityType == Metadata)
                        {
                            principalBuilder = newOtherOwnership.Metadata.DeclaringEntityType.Builder;
                        }

                        ownedEntityType = targetType == null
                            ? ModelBuilder.Entity(targetTypeName, navigation.Name, principalBuilder.Metadata, configurationSource)
                            : ModelBuilder.Entity(targetType, navigation.Name, principalBuilder.Metadata, configurationSource);
                    }
                }

                relationship = ownedEntityType.Relationship(
                    targetEntityTypeBuilder: principalBuilder,
                    navigationToTarget: inverse,
                    inverseNavigation: navigation,
                    setTargetAsPrincipal: true,
                    configurationSource: configurationSource,
                    required: true);
                relationship = batch.Run(relationship.IsOwnership(true, configurationSource));
            }

            if (relationship?.Metadata.Builder == null)
            {
                if (ownedEntityType.Metadata.Builder != null
                    && ownedEntityType.Metadata.HasDefiningNavigation())
                {
                    ModelBuilder.RemoveEntityType(ownedEntityType.Metadata, configurationSource);
                }

                return null;
            }

            return relationship;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool RemoveNonOwnershipRelationships(ForeignKey ownership, ConfigurationSource configurationSource)
        {
            var incompatibleRelationships = Metadata.GetDerivedForeignKeysInclusive()
                .Where(
                    fk => !fk.IsOwnership && fk.PrincipalToDependent != null
                                          && !Contains(ownership, fk))
                .Concat(
                    Metadata.GetDerivedReferencingForeignKeysInclusive()
                        .Where(
                            fk => !fk.IsOwnership
                                  && !Contains(fk.DeclaringEntityType.FindOwnership(), fk)))
                .Distinct()
                .ToList();

            if (incompatibleRelationships.Any(fk => !configurationSource.Overrides(fk.GetConfigurationSource())))
            {
                return false;
            }

            foreach (var foreignKey in incompatibleRelationships)
            {
                foreignKey.DeclaringEntityType.Builder.RemoveForeignKey(foreignKey, configurationSource);
            }

            return true;
        }

        private bool Contains(IForeignKey inheritedFk, IForeignKey derivedFk)
            => inheritedFk != null
               && inheritedFk.PrincipalEntityType.IsAssignableFrom(derivedFk.PrincipalEntityType)
               && PropertyListComparer.Instance.Equals(inheritedFk.Properties, derivedFk.Properties);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Navigation(
            [NotNull] InternalEntityTypeBuilder targetEntityTypeBuilder,
            [CanBeNull] string navigationName,
            ConfigurationSource configurationSource,
            bool setTargetAsPrincipal = false)
            => Relationship(
                Check.NotNull(targetEntityTypeBuilder, nameof(targetEntityTypeBuilder)),
                PropertyIdentity.Create(navigationName),
                null,
                setTargetAsPrincipal,
                configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Navigation(
            [NotNull] InternalEntityTypeBuilder targetEntityTypeBuilder,
            [CanBeNull] PropertyInfo navigationProperty,
            ConfigurationSource configurationSource,
            bool setTargetAsPrincipal = false)
            => Relationship(
                Check.NotNull(targetEntityTypeBuilder, nameof(targetEntityTypeBuilder)),
                PropertyIdentity.Create(navigationProperty),
                null,
                setTargetAsPrincipal,
                configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder CreateForeignKey(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [CanBeNull] IReadOnlyList<Property> dependentProperties,
            [CanBeNull] Key principalKey,
            [CanBeNull] string navigationToPrincipalName,
            bool? isRequired,
            ConfigurationSource configurationSource)
        {
            using (var batch = ModelBuilder.Metadata.ConventionDispatcher.StartBatch())
            {
                var principalType = principalEntityTypeBuilder.Metadata;
                var principalBaseEntityTypeBuilder = principalType.RootType().Builder;
                if (principalKey == null)
                {
                    principalKey = principalType.FindPrimaryKey();
                    if (principalKey != null
                        && dependentProperties != null)
                    {
                        if (!ForeignKey.AreCompatible(
                            principalKey.Properties,
                            dependentProperties,
                            principalType,
                            Metadata,
                            shouldThrow: false))
                        {
                            if (dependentProperties.All(p => p.GetTypeConfigurationSource() == null))
                            {
                                var detachedProperties = DetachProperties(dependentProperties);
                                GetOrCreateProperties(dependentProperties.Select(p => p.Name).ToList(), configurationSource, principalKey.Properties, isRequired ?? false);
                                detachedProperties.Attach(this);
                            }
                            else
                            {
                                principalKey = null;
                            }
                        }
                        else if (Metadata.FindForeignKeysInHierarchy(dependentProperties, principalKey, principalType).Any())
                        {
                            principalKey = null;
                        }
                    }
                }

                if (dependentProperties != null)
                {
                    dependentProperties = GetActualProperties(dependentProperties, ConfigurationSource.Convention);
                    if (principalKey == null)
                    {
                        var principalKeyProperties = principalBaseEntityTypeBuilder.TryCreateUniqueProperties(
                            dependentProperties.Count, null, Enumerable.Repeat("", dependentProperties.Count), dependentProperties.Select(p => p.ClrType), isRequired: true, baseName: "TempId").Item2;

                        var keyBuilder = principalBaseEntityTypeBuilder.HasKeyInternal(principalKeyProperties, ConfigurationSource.Convention);

                        principalKey = keyBuilder.Metadata;
                    }
                    else
                    {
                        Debug.Assert(Metadata.FindForeignKey(dependentProperties, principalKey, principalType) == null);
                    }
                }
                else
                {
                    if (principalKey == null)
                    {
                        var principalKeyProperties = principalBaseEntityTypeBuilder.TryCreateUniqueProperties(
                            1, null, new[] { "TempId" }, new[] { typeof(int) }, isRequired: true, baseName: "").Item2;

                        principalKey = principalBaseEntityTypeBuilder.HasKeyInternal(principalKeyProperties, ConfigurationSource.Convention).Metadata;
                    }

                    var baseName = string.IsNullOrEmpty(navigationToPrincipalName)
                        ? principalType.ShortName()
                        : navigationToPrincipalName;
                    dependentProperties = CreateUniqueProperties(null, principalKey.Properties, isRequired ?? false, baseName);
                }

                var foreignKey = Metadata.AddForeignKey(dependentProperties, principalKey, principalType, configurationSource: null);
                foreignKey.UpdateConfigurationSource(configurationSource);
                if (isRequired.HasValue
                    && foreignKey.IsRequired == isRequired.Value)
                {
                    foreignKey.SetIsRequired(isRequired.Value, configurationSource);
                }

                principalType.UpdateConfigurationSource(configurationSource);

                return batch.Run(foreignKey)?.Builder;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ShouldReuniquifyTemporaryProperties(
            [NotNull] IReadOnlyList<Property> currentProperties,
            [NotNull] IReadOnlyList<Property> principalProperties,
            bool isRequired,
            [NotNull] string baseName)
            => TryCreateUniqueProperties(
                principalProperties.Count,
                currentProperties,
                principalProperties.Select(p => p.Name),
                principalProperties.Select(p => p.ClrType),
                isRequired,
                baseName).Item1;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Property CreateUniqueProperty(
            string propertyName,
            Type propertyType,
            bool isRequired)
            => CreateUniqueProperties(
                new[] { propertyName },
                new[] { propertyType },
                isRequired).First();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<Property> CreateUniqueProperties(
            IReadOnlyList<string> propertyNames,
            IReadOnlyList<Type> propertyTypes,
            bool isRequired)
            => TryCreateUniqueProperties(
                propertyNames.Count,
                null,
                propertyNames,
                propertyTypes,
                isRequired,
                "").Item2;

        private IReadOnlyList<Property> CreateUniqueProperties(
            IReadOnlyList<Property> currentProperties,
            IReadOnlyList<Property> principalProperties,
            bool isRequired,
            string baseName)
            => TryCreateUniqueProperties(
                principalProperties.Count,
                currentProperties,
                principalProperties.Select(p => p.Name),
                principalProperties.Select(p => p.ClrType),
                isRequired,
                baseName).Item2;

        private (bool, IReadOnlyList<Property>) TryCreateUniqueProperties(
            int propertyCount,
            IReadOnlyList<Property> currentProperties,
            IEnumerable<string> principalPropertyNames,
            IEnumerable<Type> principalPropertyTypes,
            bool isRequired,
            string baseName)
        {
            var newProperties = currentProperties == null ? new Property[propertyCount] : null;
            var clrProperties = Metadata.GetRuntimeProperties();
            var clrFields = Metadata.GetRuntimeFields();
            var canReuniquify = false;
            using (var principalPropertyNamesEnumerator = principalPropertyNames.GetEnumerator())
            {
                using (var principalPropertyTypesEnumerator = principalPropertyTypes.GetEnumerator())
                {
                    for (var i = 0;
                         i < propertyCount
                         && principalPropertyNamesEnumerator.MoveNext()
                         && principalPropertyTypesEnumerator.MoveNext();
                         i++)
                    {
                        var keyPropertyName = principalPropertyNamesEnumerator.Current;
                        var keyPropertyType = principalPropertyTypesEnumerator.Current;
                        var keyModifiedBaseName = keyPropertyName.StartsWith(baseName, StringComparison.OrdinalIgnoreCase)
                            ? keyPropertyName
                            : baseName + keyPropertyName;
                        string propertyName;
                        var clrType = isRequired ? keyPropertyType : keyPropertyType.MakeNullable();
                        var index = -1;
                        while (true)
                        {
                            propertyName = keyModifiedBaseName + (++index > 0 ? index.ToString(CultureInfo.InvariantCulture) : "");
                            if (!Metadata.FindPropertiesInHierarchy(propertyName).Any()
                                && clrProperties?.ContainsKey(propertyName) != true
                                && clrFields?.ContainsKey(propertyName) != true
                                && !IsIgnored(propertyName, ConfigurationSource.Convention))
                            {
                                if (currentProperties == null)
                                {
                                    var propertyBuilder = Property(
                                        propertyName, clrType, ConfigurationSource.Convention, typeConfigurationSource: null);

                                    if (clrType.IsNullableType())
                                    {
                                        propertyBuilder.IsRequired(isRequired, ConfigurationSource.Convention);
                                    }

                                    newProperties[i] = propertyBuilder.Metadata;
                                }
                                else
                                {
                                    canReuniquify = true;
                                }

                                break;
                            }

                            var currentProperty = currentProperties?.SingleOrDefault(p => p.Name == propertyName);
                            if (currentProperty != null)
                            {
                                if (currentProperty.IsShadowProperty
                                    && currentProperty.ClrType != clrType
                                    && isRequired)
                                {
                                    canReuniquify = true;
                                }

                                break;
                            }
                        }
                    }
                }
            }

            return (canReuniquify, newProperties);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<Property> GetOrCreateProperties(
            [CanBeNull] IReadOnlyList<string> propertyNames,
            ConfigurationSource? configurationSource,
            [CanBeNull] IReadOnlyList<Property> referencedProperties = null,
            bool required = false,
            bool useDefaultType = false)
        {
            if (propertyNames == null)
            {
                return null;
            }

            if (referencedProperties != null
                && referencedProperties.Count != propertyNames.Count)
            {
                referencedProperties = null;
            }

            var propertyList = new List<Property>();
            for (var i = 0; i < propertyNames.Count; i++)
            {
                var propertyName = propertyNames[i];
                var property = Metadata.FindProperty(propertyName);
                if (property == null)
                {
                    var clrProperty = Metadata.ClrType?.GetMembersInHierarchy(propertyName).FirstOrDefault();
                    var type = referencedProperties == null
                        ? useDefaultType
                            ? typeof(int)
                            : null
                        : referencedProperties[i].ClrType;

                    InternalPropertyBuilder propertyBuilder;
                    if (!configurationSource.HasValue)
                    {
                        return null;
                    }

                    if (clrProperty != null)
                    {
                        propertyBuilder = Property(clrProperty, configurationSource.Value);
                    }
                    else if (type != null)
                    {
                        // TODO: Log that a shadow property is created
                        propertyBuilder = Property(
                            propertyName, required ? type : type.MakeNullable(), configurationSource.Value, typeConfigurationSource: null);
                    }
                    else
                    {
                        throw new InvalidOperationException(CoreStrings.NoPropertyType(propertyName, Metadata.DisplayName()));
                    }

                    if (propertyBuilder == null)
                    {
                        return null;
                    }

                    property = propertyBuilder.Metadata;
                }
                else if (configurationSource.HasValue)
                {
                    property.DeclaringEntityType.UpdateConfigurationSource(configurationSource.Value);
                    property = property.DeclaringEntityType.Builder.Property(property.Name, configurationSource.Value).Metadata;
                }

                propertyList.Add(property);
            }

            return propertyList;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<Property> GetOrCreateProperties([CanBeNull] IEnumerable<MemberInfo> clrProperties, ConfigurationSource configurationSource)
        {
            if (clrProperties == null)
            {
                return null;
            }

            var list = new List<Property>();
            foreach (var propertyInfo in clrProperties)
            {
                var propertyBuilder = Property(propertyInfo, configurationSource);
                if (propertyBuilder == null)
                {
                    return null;
                }

                list.Add(propertyBuilder.Metadata);
            }

            return list;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<Property> GetActualProperties([CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource? configurationSource)
        {
            if (properties == null)
            {
                return null;
            }

            var actualProperties = new Property[properties.Count];
            for (var i = 0; i < actualProperties.Length; i++)
            {
                var property = properties[i];
                var typeConfigurationSource = property.GetTypeConfigurationSource();
                var builder = property.Builder != null && property.DeclaringEntityType.IsAssignableFrom(Metadata)
                    ? property.Builder
                    : Property(
                        property.Name,
                        typeConfigurationSource.HasValue ? property.ClrType : null,
                        property.GetIdentifyingMemberInfo(),
                        configurationSource,
                        typeConfigurationSource);
                if (builder == null)
                {
                    return null;
                }

                actualProperties[i] = builder.Metadata;
            }

            return actualProperties;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool UsePropertyAccessMode(PropertyAccessMode propertyAccessMode, ConfigurationSource configurationSource)
            => HasAnnotation(CoreAnnotationNames.PropertyAccessModeAnnotation, propertyAccessMode, configurationSource);
    }
}
