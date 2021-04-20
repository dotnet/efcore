// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class InternalEntityTypeBuilder : AnnotatableBuilder<EntityType, InternalModelBuilder>, IConventionEntityTypeBuilder
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalEntityTypeBuilder(EntityType metadata, InternalModelBuilder modelBuilder)
            : base(metadata, modelBuilder)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalKeyBuilder? PrimaryKey(
            IReadOnlyList<string>? propertyNames,
            ConfigurationSource configurationSource)
            => PrimaryKey(GetOrCreateProperties(propertyNames, configurationSource, required: true), configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalKeyBuilder? PrimaryKey(
            IReadOnlyList<MemberInfo>? clrMembers,
            ConfigurationSource configurationSource)
            => PrimaryKey(GetOrCreateProperties(clrMembers, configurationSource), configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalKeyBuilder? PrimaryKey(
            IReadOnlyList<Property>? properties,
            ConfigurationSource configurationSource)
        {
            if (!CanSetPrimaryKey(properties, configurationSource))
            {
                return null;
            }

            InternalKeyBuilder? keyBuilder = null;
            if (properties == null)
            {
                Metadata.SetPrimaryKey(properties, configurationSource);
            }
            else
            {
                var previousPrimaryKey = Metadata.FindPrimaryKey();
                if (previousPrimaryKey != null
                    && PropertyListComparer.Instance.Compare(previousPrimaryKey.Properties, properties) == 0)
                {
                    previousPrimaryKey.UpdateConfigurationSource(configurationSource);
                    return Metadata.SetPrimaryKey(properties, configurationSource)!.Builder;
                }

                using (ModelBuilder.Metadata.DelayConventions())
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

                    if (previousPrimaryKey?.IsInModel == true)
                    {
                        RemoveKeyIfUnused(previousPrimaryKey, configurationSource);
                    }
                }
            }

            // TODO: Use convention batch to get the updated builder, see #15898
            if (keyBuilder is null || !keyBuilder.Metadata.IsInModel)
            {
                properties = GetActualProperties(properties, null);
                return properties == null ? null : Metadata.FindPrimaryKey(properties)!.Builder;
            }

            return keyBuilder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetPrimaryKey(
            IReadOnlyList<IConventionProperty>? properties,
            ConfigurationSource configurationSource)
        {
            var previousPrimaryKey = Metadata.FindPrimaryKey();
            if (properties == null)
            {
                if (previousPrimaryKey == null)
                {
                    return true;
                }
            }
            else if (previousPrimaryKey != null
                && PropertyListComparer.Instance.Compare(previousPrimaryKey.Properties, properties) == 0)
            {
                return true;
            }

            return configurationSource.Overrides(Metadata.GetPrimaryKeyConfigurationSource())
                && (properties == null
                    || !Metadata.IsKeyless
                    || configurationSource.Overrides(Metadata.GetIsKeylessConfigurationSource()));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalKeyBuilder? HasKey(IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => HasKeyInternal(GetOrCreateProperties(propertyNames, configurationSource, required: true), configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalKeyBuilder? HasKey(IReadOnlyList<MemberInfo> clrMembers, ConfigurationSource configurationSource)
            => HasKeyInternal(GetOrCreateProperties(clrMembers, configurationSource), configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalKeyBuilder? HasKey(IReadOnlyList<Property> properties, ConfigurationSource? configurationSource)
            => HasKeyInternal(properties, configurationSource);

        private InternalKeyBuilder? HasKeyInternal(IReadOnlyList<Property>? properties, ConfigurationSource? configurationSource)
        {
            if (properties == null)
            {
                return null;
            }

            var actualProperties = GetActualProperties(properties, configurationSource)!;
            var key = Metadata.FindDeclaredKey(actualProperties);
            if (key == null)
            {
                if (configurationSource == null)
                {
                    return null;
                }

                if (Metadata.IsKeyless
                    && !configurationSource.Overrides(Metadata.GetIsKeylessConfigurationSource()))
                {
                    return null;
                }

                if (Metadata.GetIsKeylessConfigurationSource() != ConfigurationSource.Explicit)
                {
                    Metadata.SetIsKeyless(false, configurationSource.Value);
                }

                var containingForeignKeys = actualProperties
                    .SelectMany(p => p.GetContainingForeignKeys().Where(k => k.DeclaringEntityType != Metadata))
                    .ToList();

                if (containingForeignKeys.Any(fk => !configurationSource.Overrides(fk.GetPropertiesConfigurationSource())))
                {
                    return null;
                }

                if (configurationSource != ConfigurationSource.Explicit // let it throw for explicit
                    && actualProperties.Any(p => !p.Builder.CanSetIsRequired(true, configurationSource)))
                {
                    return null;
                }

                using (Metadata.Model.DelayConventions())
                {
                    foreach (var foreignKey in containingForeignKeys)
                    {
                        if (foreignKey.GetPropertiesConfigurationSource() == ConfigurationSource.Explicit)
                        {
                            // let it throw for explicit
                            continue;
                        }

                        foreignKey.Builder.HasForeignKey((IReadOnlyList<Property>?)null, configurationSource.Value);
                    }

                    foreach (var actualProperty in actualProperties)
                    {
                        actualProperty.Builder.IsRequired(true, configurationSource.Value);
                    }

                    key = Metadata.AddKey(actualProperties, configurationSource.Value)!;
                }

                if (!key.IsInModel)
                {
                    key = Metadata.FindDeclaredKey(actualProperties);
                }
            }
            else if (configurationSource.HasValue)
            {
                key.UpdateConfigurationSource(configurationSource.Value);
                Metadata.SetIsKeyless(false, configurationSource.Value);
            }

            return key?.IsInModel == true ? key.Builder : null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder? HasNoKey(Key key, ConfigurationSource configurationSource)
        {
            var currentConfigurationSource = key.GetConfigurationSource();
            if (!configurationSource.Overrides(currentConfigurationSource))
            {
                return null;
            }

            using (Metadata.Model.DelayConventions())
            {
                var detachedRelationships = key.GetReferencingForeignKeys().ToList().Select(DetachRelationship).ToList();

                Metadata.RemoveKey(key);

                foreach (var detachedRelationship in detachedRelationships)
                {
                    detachedRelationship.Attach();
                }

                RemoveUnusedImplicitProperties(key.Properties);
                foreach (var property in key.Properties)
                {
                    if (!property.IsKey()
                        && property.ClrType.IsNullableType()
                        && !property.GetContainingForeignKeys().Any(fk => fk.IsRequired))
                    {
                        // TODO: This should be handled by reference tracking, see #15898
                        if (property.IsInModel)
                        {
                            property.Builder.IsRequired(null, configurationSource);
                        }
                    }
                }
            }

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanRemoveKey(Key key, ConfigurationSource configurationSource)
            => configurationSource.Overrides(key.GetConfigurationSource());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static List<(InternalKeyBuilder, ConfigurationSource?)>? DetachKeys(IEnumerable<Key> keysToDetach)
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

            var primaryKeyConfigurationSource = ((IReadOnlyKey)keyToDetach).IsPrimaryKey()
                ? keyToDetach.DeclaringEntityType.GetPrimaryKeyConfigurationSource()
                : null;

            if (entityTypeBuilder == null)
            {
                keyToDetach.DeclaringEntityType.RemoveKey(keyToDetach.Properties);
            }
            else
            {
                entityTypeBuilder.HasNoKey(keyToDetach, keyToDetach.GetConfigurationSource());
            }

            return (keyBuilder, primaryKeyConfigurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder? HasNoKey(ConfigurationSource configurationSource)
        {
            if (Metadata.IsKeyless)
            {
                Metadata.SetIsKeyless(true, configurationSource);
                return this;
            }

            if (!CanRemoveKey(configurationSource))
            {
                return null;
            }

            using (Metadata.Model.DelayConventions())
            {
                foreach (var foreignKey in Metadata.GetReferencingForeignKeys().ToList())
                {
                    foreignKey.DeclaringEntityType.Builder.HasNoRelationship(foreignKey, configurationSource);
                }

                foreach (var foreignKey in Metadata.GetForeignKeys())
                {
                    foreignKey.SetPrincipalToDependent((string?)null, configurationSource);
                }

                foreach (var key in Metadata.GetKeys().ToList())
                {
                    if (key.GetConfigurationSource() != ConfigurationSource.Explicit)
                    {
                        HasNoKey(key, configurationSource);
                    }
                }

                Metadata.SetIsKeyless(true, configurationSource);
                return this;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanRemoveKey(ConfigurationSource configurationSource)
            => Metadata.IsKeyless
                || (configurationSource.Overrides(Metadata.GetIsKeylessConfigurationSource())
                    && !Metadata.GetKeys().Any(key => !configurationSource.Overrides(key.GetConfigurationSource()))
                    && !Metadata.GetReferencingForeignKeys().Any(fk => !configurationSource.Overrides(fk.GetConfigurationSource()))
                    && !Metadata.GetForeignKeys()
                        .Any(fk => !configurationSource.Overrides(fk.GetPrincipalToDependentConfigurationSource())));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder? Property(
            Type? propertyType,
            string propertyName,
            ConfigurationSource? configurationSource)
            => Property(propertyType, propertyName, typeConfigurationSource: configurationSource, configurationSource: configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder? Property(
            Type? propertyType,
            string propertyName,
            ConfigurationSource? typeConfigurationSource,
            ConfigurationSource? configurationSource)
            => Property(
                propertyType, propertyName, memberInfo: null,
                typeConfigurationSource,
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder? Property(string propertyName, ConfigurationSource? configurationSource)
            => Property(propertyType: null, propertyName, memberInfo: null, typeConfigurationSource: null, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder? Property(MemberInfo memberInfo, ConfigurationSource? configurationSource)
            => Property(memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName(), memberInfo, configurationSource, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder? IndexerProperty(
            Type? propertyType,
            string propertyName,
            ConfigurationSource? configurationSource)
        {
            var indexerPropertyInfo = Metadata.FindIndexerPropertyInfo();
            if (indexerPropertyInfo == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.NonIndexerEntityType(propertyName, Metadata.DisplayName(), typeof(string).ShortDisplayName()));
            }

            return Property(propertyType, propertyName, indexerPropertyInfo, configurationSource, configurationSource);
        }

        private InternalPropertyBuilder? Property(
            Type? propertyType,
            string propertyName,
            MemberInfo? memberInfo,
            ConfigurationSource? typeConfigurationSource,
            ConfigurationSource? configurationSource)
        {
            var entityType = Metadata;
            List<Property>? propertiesToDetach = null;
            var existingProperty = entityType.FindProperty(propertyName);
            if (existingProperty != null)
            {
                if (existingProperty.DeclaringEntityType != Metadata)
                {
                    if (!IsIgnored(propertyName, configurationSource))
                    {
                        Metadata.RemoveIgnored(propertyName);
                    }

                    entityType = existingProperty.DeclaringEntityType;
                }

                if (IsCompatible(memberInfo, existingProperty)
                    && (propertyType == null || propertyType == existingProperty.ClrType))
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

                if (memberInfo == null
                    || (memberInfo is PropertyInfo propertyInfo && propertyInfo.IsIndexerProperty()))
                {
                    if (existingProperty.GetTypeConfigurationSource() is ConfigurationSource existingTypeConfigurationSource
                        && !typeConfigurationSource.Overrides(existingTypeConfigurationSource))
                    {
                        return null;
                    }

                    memberInfo ??= existingProperty.GetIdentifyingMemberInfo();
                }
                else if (!configurationSource.Overrides(existingProperty.GetConfigurationSource()))
                {
                    return null;
                }

                if (propertyType == null)
                {
                    propertyType = existingProperty.ClrType;
                }

                propertiesToDetach = new List<Property> { existingProperty };
            }
            else
            {
                if (!configurationSource.HasValue
                    || IsIgnored(propertyName, configurationSource))
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

                    var navigationConfigurationSource = conflictingNavigation.GetConfigurationSource();
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

                foreach (var conflictingSkipNavigation in Metadata.FindSkipNavigationsInHierarchy(propertyName))
                {
                    if (!configurationSource.Overrides(conflictingSkipNavigation.GetConfigurationSource()))
                    {
                        return null;
                    }
                }

                if (memberInfo == null)
                {
                    memberInfo = Metadata.ClrType.GetMembersInHierarchy(propertyName).FirstOrDefault();
                }

                if (propertyType == null)
                {
                    if (memberInfo == null)
                    {
                        throw new InvalidOperationException(CoreStrings.NoPropertyType(propertyName, Metadata.DisplayName()));
                    }

                    propertyType = memberInfo.GetMemberType();
                    typeConfigurationSource = ConfigurationSource.Explicit;
                }
                else if (memberInfo != null
                    && propertyType != memberInfo.GetMemberType()
                    && memberInfo != Metadata.FindIndexerPropertyInfo()
                    && typeConfigurationSource != null)
                {
                    return null;
                }

                foreach (var derivedType in Metadata.GetDerivedTypes())
                {
                    var derivedProperty = derivedType.FindDeclaredProperty(propertyName);
                    if (derivedProperty != null)
                    {
                        if (propertiesToDetach == null)
                        {
                            propertiesToDetach = new List<Property>();
                        }

                        propertiesToDetach.Add(derivedProperty);
                    }
                }
            }

            Check.DebugAssert(configurationSource is not null, "configurationSource is null");

            InternalPropertyBuilder builder;
            using (Metadata.Model.DelayConventions())
            {
                var detachedProperties = propertiesToDetach == null ? null : DetachProperties(propertiesToDetach);

                if (existingProperty == null)
                {
                    Metadata.RemoveIgnored(propertyName);

                    foreach (var conflictingServiceProperty in Metadata.FindServicePropertiesInHierarchy(propertyName))
                    {
                        if (conflictingServiceProperty.GetConfigurationSource() != ConfigurationSource.Explicit)
                        {
                            conflictingServiceProperty.DeclaringEntityType.RemoveServiceProperty(conflictingServiceProperty);
                        }
                    }

                    foreach (var conflictingNavigation in Metadata.FindNavigationsInHierarchy(propertyName))
                    {
                        if (conflictingNavigation.GetConfigurationSource() == ConfigurationSource.Explicit)
                        {
                            continue;
                        }

                        var foreignKey = conflictingNavigation.ForeignKey;
                        if (foreignKey.GetConfigurationSource() == ConfigurationSource.Convention)
                        {
                            foreignKey.DeclaringEntityType.Builder.HasNoRelationship(foreignKey, ConfigurationSource.Convention);
                        }
                        else if (foreignKey.Builder.HasNavigation(
                                (string?)null,
                                conflictingNavigation.IsOnDependent,
                                configurationSource.Value)
                            == null)
                        {
                            return null;
                        }
                    }

                    foreach (var conflictingSkipNavigation in Metadata.FindSkipNavigationsInHierarchy(propertyName))
                    {
                        if (conflictingSkipNavigation.GetConfigurationSource() == ConfigurationSource.Explicit)
                        {
                            continue;
                        }

                        var inverse = conflictingSkipNavigation.Inverse;
                        if (inverse?.IsInModel == true
                            && inverse.GetConfigurationSource() != ConfigurationSource.Explicit)
                        {
                            inverse.DeclaringEntityType.Builder.HasNoSkipNavigation(inverse, configurationSource.Value);
                        }

                        conflictingSkipNavigation.DeclaringEntityType.Builder.HasNoSkipNavigation(
                            conflictingSkipNavigation, configurationSource.Value);
                    }
                }

                builder = entityType.AddProperty(
                    propertyName, propertyType, memberInfo, typeConfigurationSource, configurationSource.Value)!.Builder;

                detachedProperties?.Attach(this);
            }

            return builder.Metadata.IsInModel
                ? builder
                : Metadata.FindProperty(propertyName)?.Builder;
        }

        private bool IsCompatible(MemberInfo? newMemberInfo, Property existingProperty)
        {
            if (newMemberInfo == null)
            {
                return true;
            }

            var existingMemberInfo = existingProperty.GetIdentifyingMemberInfo();
            if (existingMemberInfo == null)
            {
                return false;
            }

            if (newMemberInfo == existingMemberInfo)
            {
                return true;
            }

            var declaringType = (IMutableEntityType)existingProperty.DeclaringType;
            if (!newMemberInfo.DeclaringType!.IsAssignableFrom(declaringType.ClrType))
            {
                return existingMemberInfo.IsOverridenBy(newMemberInfo);
            }

            IMutableEntityType? existingMemberDeclaringEntityType = null;
            foreach (var baseType in declaringType.GetAllBaseTypes())
            {
                if (newMemberInfo.DeclaringType == baseType.ClrType)
                {
                    return existingMemberDeclaringEntityType != null
                        && existingMemberInfo.IsOverridenBy(newMemberInfo);
                }

                if (existingMemberDeclaringEntityType == null
                    && existingMemberInfo.DeclaringType == baseType.ClrType)
                {
                    existingMemberDeclaringEntityType = baseType;
                }
            }

            // newMemberInfo is declared on an unmapped base type, existingMemberInfo should be kept
            return newMemberInfo.IsOverridenBy(existingMemberInfo);
        }

        private bool CanRemoveProperty(
            Property property,
            ConfigurationSource configurationSource,
            bool canOverrideSameSource = true)
        {
            Check.NotNull(property, nameof(property));
            Check.DebugAssert(property.DeclaringEntityType == Metadata, "property.DeclaringEntityType != Metadata");

            var currentConfigurationSource = property.GetConfigurationSource();
            return configurationSource.Overrides(currentConfigurationSource)
                && (canOverrideSameSource || (configurationSource != currentConfigurationSource));
        }

        private ConfigurationSource? RemoveProperty(
            Property property,
            ConfigurationSource configurationSource,
            bool canOverrideSameSource = true)
        {
            var currentConfigurationSource = property.GetConfigurationSource();
            if (!configurationSource.Overrides(currentConfigurationSource)
                || !(canOverrideSameSource || (configurationSource != currentConfigurationSource)))
            {
                return null;
            }

            using (Metadata.Model.DelayConventions())
            {
                var detachedRelationships = property.GetContainingForeignKeys().ToList()
                    .Select(DetachRelationship).ToList();

                foreach (var key in property.GetContainingKeys().ToList())
                {
                    detachedRelationships.AddRange(
                        key.GetReferencingForeignKeys().ToList()
                            .Select(DetachRelationship));
                    var removed = key.DeclaringEntityType.Builder.HasNoKey(key, configurationSource);
                    Check.DebugAssert(removed != null, "removed is null");
                }

                foreach (var index in property.GetContainingIndexes().ToList())
                {
                    var removed = index.DeclaringEntityType.Builder.HasNoIndex(index, configurationSource);
                    Check.DebugAssert(removed != null, "removed is null");
                }

                if (property.IsInModel)
                {
                    var removedProperty = Metadata.RemoveProperty(property.Name);
                    Check.DebugAssert(removedProperty == property, "removedProperty != property");
                }

                foreach (var relationshipSnapshot in detachedRelationships)
                {
                    relationshipSnapshot.Attach();
                }
            }

            return currentConfigurationSource;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IMutableNavigationBase Navigation(MemberInfo memberInfo)
            => Navigation(memberInfo.GetSimpleMemberName());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IMutableNavigationBase Navigation(string navigationName)
            => (IMutableNavigationBase?)Metadata.FindNavigation(navigationName)
                ?? Metadata.FindSkipNavigation(navigationName)
                ?? throw new InvalidOperationException(
                    CoreStrings.CanOnlyConfigureExistingNavigations(navigationName, Metadata.DisplayName()));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalServicePropertyBuilder? ServiceProperty(
            MemberInfo memberInfo,
            ConfigurationSource? configurationSource)
        {
            var propertyName = memberInfo.GetSimpleMemberName();
            List<ServiceProperty>? propertiesToDetach = null;
            InternalServicePropertyBuilder? builder;
            var existingProperty = Metadata.FindServiceProperty(propertyName);
            if (existingProperty != null)
            {
                if (existingProperty.DeclaringEntityType != Metadata)
                {
                    if (!IsIgnored(propertyName, configurationSource))
                    {
                        Metadata.RemoveIgnored(propertyName);
                    }
                }

                if (existingProperty.GetIdentifyingMemberInfo()?.IsOverridenBy(memberInfo) == true)
                {
                    if (configurationSource.HasValue)
                    {
                        existingProperty.UpdateConfigurationSource(configurationSource.Value);
                    }

                    return existingProperty.Builder;
                }

                if (!configurationSource.Overrides(existingProperty.GetConfigurationSource()))
                {
                    return null;
                }

                propertiesToDetach = new List<ServiceProperty> { existingProperty };
            }
            else if (!CanAddServiceProperty(memberInfo, configurationSource))
            {
                return null;
            }
            else
            {
                foreach (var derivedType in Metadata.GetDerivedTypes())
                {
                    var derivedProperty = derivedType.FindDeclaredServiceProperty(propertyName);
                    if (derivedProperty != null)
                    {
                        if (propertiesToDetach == null)
                        {
                            propertiesToDetach = new List<ServiceProperty>();
                        }

                        propertiesToDetach.Add(derivedProperty);
                    }
                }
            }

            Check.DebugAssert(configurationSource is not null, "configurationSource is not null");

            using (ModelBuilder.Metadata.DelayConventions())
            {
                List<InternalServicePropertyBuilder>? detachedProperties = null;
                if (propertiesToDetach != null)
                {
                    detachedProperties = new List<InternalServicePropertyBuilder>();
                    foreach (var propertyToDetach in propertiesToDetach)
                    {
                        detachedProperties.Add(DetachServiceProperty(propertyToDetach)!);
                    }
                }

                if (existingProperty == null)
                {
                    Metadata.RemoveIgnored(propertyName);

                    foreach (var conflictingProperty in Metadata.FindPropertiesInHierarchy(propertyName).ToList())
                    {
                        if (conflictingProperty.GetConfigurationSource() != ConfigurationSource.Explicit)
                        {
                            conflictingProperty.DeclaringEntityType.Builder.RemoveProperty(conflictingProperty, configurationSource.Value);
                        }
                    }

                    foreach (var conflictingNavigation in Metadata.FindNavigationsInHierarchy(propertyName).ToList())
                    {
                        if (conflictingNavigation.GetConfigurationSource() == ConfigurationSource.Explicit)
                        {
                            continue;
                        }

                        var foreignKey = conflictingNavigation.ForeignKey;
                        if (foreignKey.GetConfigurationSource() == ConfigurationSource.Convention)
                        {
                            foreignKey.DeclaringEntityType.Builder.HasNoRelationship(foreignKey, ConfigurationSource.Convention);
                        }
                        else if (foreignKey.Builder.HasNavigation(
                                (string?)null,
                                conflictingNavigation.IsOnDependent,
                                configurationSource.Value)
                            == null)
                        {
                            return null;
                        }
                    }

                    foreach (var conflictingSkipNavigation in Metadata.FindSkipNavigationsInHierarchy(propertyName).ToList())
                    {
                        if (conflictingSkipNavigation.GetConfigurationSource() == ConfigurationSource.Explicit)
                        {
                            continue;
                        }

                        var inverse = conflictingSkipNavigation.Inverse;
                        if (inverse?.IsInModel == true
                            && inverse.GetConfigurationSource() != ConfigurationSource.Explicit)
                        {
                            inverse.DeclaringEntityType.Builder.HasNoSkipNavigation(inverse, configurationSource.Value);
                        }

                        conflictingSkipNavigation.DeclaringEntityType.Builder.HasNoSkipNavigation(
                            conflictingSkipNavigation, configurationSource.Value);
                    }
                }

                builder = Metadata.AddServiceProperty(memberInfo, configurationSource.Value).Builder;

                if (detachedProperties != null)
                {
                    foreach (var detachedProperty in detachedProperties)
                    {
                        detachedProperty.Attach(this);
                    }
                }
            }

            return builder.Metadata.IsInModel
                ? builder
                : Metadata.FindServiceProperty(propertyName)?.Builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanHaveServiceProperty(MemberInfo memberInfo, ConfigurationSource? configurationSource)
        {
            var existingProperty = Metadata.FindServiceProperty(memberInfo);
            return existingProperty != null
                ? existingProperty.DeclaringEntityType == Metadata
                || (configurationSource.HasValue
                    && configurationSource.Value.Overrides(existingProperty.GetConfigurationSource()))
                : CanAddServiceProperty(memberInfo, configurationSource);
        }

        private bool CanAddServiceProperty(MemberInfo memberInfo, ConfigurationSource? configurationSource)
        {
            var propertyName = memberInfo.GetSimpleMemberName();
            if (!configurationSource.HasValue
                || IsIgnored(propertyName, configurationSource))
            {
                return false;
            }

            foreach (var conflictingProperty in Metadata.FindMembersInHierarchy(propertyName))
            {
                if (!configurationSource.Overrides(conflictingProperty.GetConfigurationSource())
                    && (conflictingProperty is not ServiceProperty derivedServiceProperty
                        || !memberInfo.IsOverridenBy(derivedServiceProperty.GetIdentifyingMemberInfo())))
                {
                    return false;
                }
            }

            return true;
        }

        private static InternalServicePropertyBuilder? DetachServiceProperty(ServiceProperty? serviceProperty)
        {
            if (serviceProperty is null || !serviceProperty.IsInModel)
            {
                return null;
            }

            var builder = serviceProperty.Builder;
            serviceProperty.DeclaringEntityType.RemoveServiceProperty(serviceProperty);
            return builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanHaveNavigation(string navigationName, ConfigurationSource? configurationSource)
            => !IsIgnored(navigationName, configurationSource)
                && Metadata.FindPropertiesInHierarchy(navigationName).Cast<IConventionPropertyBase>()
                    .Concat(Metadata.FindServicePropertiesInHierarchy(navigationName))
                    .Concat(Metadata.FindSkipNavigationsInHierarchy(navigationName))
                    .All(m => configurationSource.Overrides(m.GetConfigurationSource()));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanHaveSkipNavigation(string skipNavigationName, ConfigurationSource? configurationSource)
            => !IsIgnored(skipNavigationName, configurationSource)
                && Metadata.FindPropertiesInHierarchy(skipNavigationName).Cast<IConventionPropertyBase>()
                    .Concat(Metadata.FindServicePropertiesInHierarchy(skipNavigationName))
                    .Concat(Metadata.FindNavigationsInHierarchy(skipNavigationName))
                    .All(m => configurationSource.Overrides(m.GetConfigurationSource()));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsIgnored(string name, ConfigurationSource? configurationSource)
        {
            Check.NotEmpty(name, nameof(name));

            return configurationSource != ConfigurationSource.Explicit
                && !configurationSource.OverridesStrictly(Metadata.FindIgnoredConfigurationSource(name));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder? Ignore(string name, ConfigurationSource configurationSource)
        {
            var ignoredConfigurationSource = Metadata.FindIgnoredConfigurationSource(name);
            if (ignoredConfigurationSource.HasValue)
            {
                if (ignoredConfigurationSource.Value.Overrides(configurationSource))
                {
                    return this;
                }
            }
            else if (!CanIgnore(name, configurationSource, shouldThrow: true))
            {
                return null;
            }

            using (Metadata.Model.DelayConventions())
            {
                Metadata.AddIgnored(name, configurationSource);

                var navigation = Metadata.FindNavigation(name);
                if (navigation != null)
                {
                    var foreignKey = navigation.ForeignKey;
                    Check.DebugAssert(navigation.DeclaringEntityType == Metadata, "navigation.DeclaringEntityType != Metadata");

                    var navigationConfigurationSource = navigation.GetConfigurationSource();
                    if (foreignKey.GetConfigurationSource() != navigationConfigurationSource)
                    {
                        var removedNavigation = foreignKey.Builder.HasNavigation(
                            (MemberInfo?)null, navigation.IsOnDependent, configurationSource);
                        Check.DebugAssert(removedNavigation != null, "removedNavigation is null");
                    }
                    else
                    {
                        var removedForeignKey = foreignKey.DeclaringEntityType.Builder.HasNoRelationship(
                            foreignKey, configurationSource);
                        Check.DebugAssert(removedForeignKey != null, "removedForeignKey is null");
                    }
                }
                else
                {
                    var property = Metadata.FindProperty(name);
                    if (property != null)
                    {
                        Check.DebugAssert(property.DeclaringEntityType == Metadata, "property.DeclaringEntityType != Metadata");

                        var removedProperty = RemoveProperty(property, configurationSource);

                        Check.DebugAssert(removedProperty != null, "removedProperty is null");
                    }
                    else
                    {
                        var skipNavigation = Metadata.FindSkipNavigation(name);
                        if (skipNavigation != null)
                        {
                            var inverse = skipNavigation.Inverse;
                            if (inverse?.IsInModel == true
                                && inverse.GetConfigurationSource() != ConfigurationSource.Explicit)
                            {
                                inverse.DeclaringEntityType.Builder.HasNoSkipNavigation(inverse, configurationSource);
                            }

                            Check.DebugAssert(
                                skipNavigation.DeclaringEntityType == Metadata, "skipNavigation.DeclaringEntityType != Metadata");

                            Metadata.Builder.HasNoSkipNavigation(skipNavigation, configurationSource);
                        }
                        else
                        {
                            var serviceProperty = Metadata.FindServiceProperty(name);
                            if (serviceProperty != null)
                            {
                                Check.DebugAssert(
                                    serviceProperty.DeclaringEntityType == Metadata, "serviceProperty.DeclaringEntityType != Metadata");

                                Metadata.RemoveServiceProperty(serviceProperty);
                            }
                        }
                    }
                }

                foreach (var derivedType in Metadata.GetDerivedTypes())
                {
                    var derivedIgnoredSource = derivedType.FindDeclaredIgnoredConfigurationSource(name);
                    if (derivedIgnoredSource.HasValue)
                    {
                        if (configurationSource.Overrides(derivedIgnoredSource))
                        {
                            derivedType.RemoveIgnored(name);
                        }

                        continue;
                    }

                    var derivedNavigation = derivedType.FindDeclaredNavigation(name);
                    if (derivedNavigation != null)
                    {
                        var foreignKey = derivedNavigation.ForeignKey;
                        if (foreignKey.GetConfigurationSource() != derivedNavigation.GetConfigurationSource())
                        {
                            if (derivedNavigation.GetConfigurationSource() != ConfigurationSource.Explicit)
                            {
                                foreignKey.Builder.HasNavigation(
                                    (MemberInfo?)null, derivedNavigation.IsOnDependent, configurationSource);
                            }
                        }
                        else if (foreignKey.GetConfigurationSource() != ConfigurationSource.Explicit)
                        {
                            foreignKey.DeclaringEntityType.Builder.HasNoRelationship(
                                foreignKey, configurationSource);
                        }
                    }
                    else
                    {
                        var derivedProperty = derivedType.FindDeclaredProperty(name);
                        if (derivedProperty != null)
                        {
                            derivedType.Builder.RemoveProperty(
                                derivedProperty, configurationSource,
                                canOverrideSameSource: configurationSource != ConfigurationSource.Explicit);
                        }
                        else
                        {
                            var skipNavigation = derivedType.FindDeclaredSkipNavigation(name);
                            if (skipNavigation != null)
                            {
                                var inverse = skipNavigation.Inverse;
                                if (inverse?.IsInModel == true
                                    && inverse.GetConfigurationSource() != ConfigurationSource.Explicit)
                                {
                                    inverse.DeclaringEntityType.Builder.HasNoSkipNavigation(inverse, configurationSource);
                                }

                                if (skipNavigation.GetConfigurationSource() != ConfigurationSource.Explicit)
                                {
                                    derivedType.Builder.HasNoSkipNavigation(skipNavigation, configurationSource);
                                }
                            }
                            else
                            {
                                var derivedServiceProperty = derivedType.FindDeclaredServiceProperty(name);
                                if (derivedServiceProperty != null
                                    && configurationSource.Overrides(derivedServiceProperty.GetConfigurationSource())
                                    && derivedServiceProperty.GetConfigurationSource() != ConfigurationSource.Explicit)
                                {
                                    derivedType.RemoveServiceProperty(name);
                                }
                            }
                        }
                    }
                }
            }

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanIgnore(string name, ConfigurationSource configurationSource)
            => CanIgnore(name, configurationSource, shouldThrow: false);

        private bool CanIgnore(string name, ConfigurationSource configurationSource, bool shouldThrow)
        {
            var ignoredConfigurationSource = Metadata.FindIgnoredConfigurationSource(name);
            if (ignoredConfigurationSource.HasValue)
            {
                return true;
            }

            var navigation = Metadata.FindNavigation(name);
            if (navigation != null)
            {
                if (navigation.DeclaringEntityType != Metadata)
                {
                    if (shouldThrow)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.InheritedPropertyCannotBeIgnored(
                                name, Metadata.DisplayName(), navigation.DeclaringEntityType.DisplayName()));
                    }

                    return false;
                }

                if (!configurationSource.Overrides(navigation.GetConfigurationSource()))
                {
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
                        if (shouldThrow)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.InheritedPropertyCannotBeIgnored(
                                    name, Metadata.DisplayName(), property.DeclaringEntityType.DisplayName()));
                        }

                        return false;
                    }

                    if (!property.DeclaringEntityType.Builder.CanRemoveProperty(
                        property, configurationSource, canOverrideSameSource: true))
                    {
                        return false;
                    }
                }
                else
                {
                    var skipNavigation = Metadata.FindSkipNavigation(name);
                    if (skipNavigation != null)
                    {
                        if (skipNavigation.DeclaringEntityType != Metadata)
                        {
                            if (shouldThrow)
                            {
                                throw new InvalidOperationException(
                                    CoreStrings.InheritedPropertyCannotBeIgnored(
                                        name, Metadata.DisplayName(), skipNavigation.DeclaringEntityType.DisplayName()));
                            }

                            return false;
                        }

                        if (!configurationSource.Overrides(skipNavigation.GetConfigurationSource()))
                        {
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
                                if (shouldThrow)
                                {
                                    throw new InvalidOperationException(
                                        CoreStrings.InheritedPropertyCannotBeIgnored(
                                            name, Metadata.DisplayName(), serviceProperty.DeclaringEntityType.DisplayName()));
                                }

                                return false;
                            }

                            if (!configurationSource.Overrides(serviceProperty.GetConfigurationSource()))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder? HasQueryFilter(
            LambdaExpression? filter,
            ConfigurationSource configurationSource)
        {
            if (CanSetQueryFilter(filter, configurationSource))
            {
                Metadata.SetQueryFilter(filter, configurationSource);

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
        public virtual bool CanSetQueryFilter(LambdaExpression? filter, ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetQueryFilterConfigurationSource())
                || Metadata.GetQueryFilter() == filter;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [Obsolete]
        public virtual InternalEntityTypeBuilder? HasDefiningQuery(
            LambdaExpression? query,
            ConfigurationSource configurationSource)
        {
            if (CanSetDefiningQuery(query, configurationSource))
            {
                Metadata.SetDefiningQuery(query, configurationSource);

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
        [Obsolete]
        public virtual bool CanSetDefiningQuery(LambdaExpression? query, ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetDefiningQueryConfigurationSource())
                || Metadata.GetDefiningQuery() == query;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder? HasBaseType(Type? baseEntityType, ConfigurationSource configurationSource)
        {
            if (baseEntityType == null)
            {
                return HasBaseType((EntityType?)null, configurationSource);
            }

            var baseType = ModelBuilder.Entity(baseEntityType, configurationSource);
            return baseType == null
                ? null
                : HasBaseType(baseType.Metadata, configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder? HasBaseType(string? baseEntityTypeName, ConfigurationSource configurationSource)
        {
            if (baseEntityTypeName == null)
            {
                return HasBaseType((EntityType?)null, configurationSource);
            }

            var baseType = ModelBuilder.Entity(baseEntityTypeName, configurationSource);
            return baseType == null
                ? null
                : HasBaseType(baseType.Metadata, configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder? HasBaseType(
            EntityType? baseEntityType,
            ConfigurationSource configurationSource)
        {
            if (Metadata.BaseType == baseEntityType)
            {
                Metadata.SetBaseType(baseEntityType, configurationSource);
                return this;
            }

            if (!CanSetBaseType(baseEntityType, configurationSource))
            {
                return null;
            }

            using (Metadata.Model.DelayConventions())
            {
                List<RelationshipSnapshot>? detachedRelationships = null;
                List<InternalSkipNavigationBuilder>? detachedSkipNavigations = null;
                PropertiesSnapshot? detachedProperties = null;
                List<InternalServicePropertyBuilder>? detachedServiceProperties = null;
                IReadOnlyList<(InternalKeyBuilder, ConfigurationSource?)>? detachedKeys = null;
                // We use at least DataAnnotation as ConfigurationSource while removing to allow us
                // to remove metadata object which were defined in derived type
                // while corresponding annotations were present on properties in base type.
                var configurationSourceForRemoval = ConfigurationSource.DataAnnotation.Max(configurationSource);
                if (baseEntityType != null)
                {
                    var baseMemberNames = baseEntityType.GetMembers()
                        .ToDictionary(m => m.Name, m => (ConfigurationSource?)m.GetConfigurationSource());

                    var relationshipsToBeDetached =
                        FindConflictingMembers(
                                Metadata.GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredNavigations()),
                                baseMemberNames,
                                n =>
                                {
                                    var baseNavigation = baseEntityType.FindNavigation(n.Name);
                                    return baseNavigation != null
                                        && n.TargetEntityType == baseNavigation.TargetEntityType;
                                },
                                n => n.ForeignKey.DeclaringEntityType.Builder.HasNoRelationship(n.ForeignKey, ConfigurationSource.Explicit))
                            ?.Select(n => n.ForeignKey).ToHashSet();

                    foreach (var key in Metadata.GetDeclaredKeys().ToList())
                    {
                        foreach (var referencingForeignKey in key.GetReferencingForeignKeys().ToList())
                        {
                            var navigationToDependent = referencingForeignKey.PrincipalToDependent;
                            if (navigationToDependent != null
                                && baseMemberNames.TryGetValue(navigationToDependent.Name, out var baseConfigurationSource)
                                && baseConfigurationSource == ConfigurationSource.Explicit
                                && configurationSource == ConfigurationSource.Explicit
                                && navigationToDependent.GetConfigurationSource() == ConfigurationSource.Explicit)
                            {
                                IReadOnlyPropertyBase baseProperty = baseEntityType.FindMembersInHierarchy(navigationToDependent.Name).Single();
                                if (baseProperty is not IReadOnlyNavigation)
                                {
                                    throw new InvalidOperationException(
                                        CoreStrings.DuplicatePropertiesOnBase(
                                            Metadata.DisplayName(),
                                            baseEntityType.DisplayName(),
                                            navigationToDependent.DeclaringEntityType.DisplayName(),
                                            navigationToDependent.Name,
                                            baseProperty.DeclaringType.DisplayName(),
                                            baseProperty.Name));
                                }
                            }

                            if (relationshipsToBeDetached == null)
                            {
                                relationshipsToBeDetached = new HashSet<ForeignKey>();
                            }

                            relationshipsToBeDetached.Add(referencingForeignKey);
                        }
                    }

                    if (relationshipsToBeDetached != null)
                    {
                        detachedRelationships = new List<RelationshipSnapshot>();
                        foreach (var relationshipToBeDetached in relationshipsToBeDetached)
                        {
                            detachedRelationships.Add(DetachRelationship(relationshipToBeDetached));
                        }
                    }

                    var foreignKeysUsingKeyProperties = Metadata.GetDerivedTypesInclusive()
                        .SelectMany(t => t.GetDeclaredForeignKeys())
                        .Where(fk => fk.Properties.Any(p => baseEntityType.FindProperty(p.Name)?.IsKey() == true));

                    foreach (var foreignKeyUsingKeyProperties in foreignKeysUsingKeyProperties.ToList())
                    {
                        foreignKeyUsingKeyProperties.Builder.HasForeignKey((IReadOnlyList<Property>?)null, configurationSourceForRemoval);
                    }

                    var skipNavigationsToDetach =
                        FindConflictingMembers(
                            Metadata.GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredSkipNavigations()),
                            baseMemberNames,
                            n =>
                            {
                                var baseNavigation = baseEntityType.FindSkipNavigation(n.Name);
                                return baseNavigation != null
                                    && n.TargetEntityType == baseNavigation.TargetEntityType;
                            },
                            n => n.DeclaringEntityType.Builder.HasNoSkipNavigation(n, ConfigurationSource.Explicit));

                    if (skipNavigationsToDetach != null)
                    {
                        detachedSkipNavigations = new List<InternalSkipNavigationBuilder>();
                        foreach (var skipNavigation in skipNavigationsToDetach)
                        {
                            detachedSkipNavigations.Add(DetachSkipNavigation(skipNavigation)!);
                        }
                    }

                    detachedKeys = DetachKeys(Metadata.GetDeclaredKeys());

                    Metadata.SetIsKeyless(false, configurationSource);

                    var propertiesToDetach =
                        FindConflictingMembers(
                            Metadata.GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredProperties()),
                            baseMemberNames,
                            n => baseEntityType.FindProperty(n.Name) != null,
                            p => p.DeclaringEntityType.Builder.RemoveProperty(p, ConfigurationSource.Explicit));

                    if (propertiesToDetach != null)
                    {
                        detachedProperties = DetachProperties(propertiesToDetach);
                    }

                    var servicePropertiesToDetach =
                        FindConflictingMembers(
                            Metadata.GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredServiceProperties()),
                            baseMemberNames,
                            n => baseEntityType.FindServiceProperty(n.Name) != null,
                            p => p.DeclaringEntityType.RemoveServiceProperty(p));

                    if (servicePropertiesToDetach != null)
                    {
                        detachedServiceProperties = new List<InternalServicePropertyBuilder>();
                        foreach (var serviceProperty in servicePropertiesToDetach)
                        {
                            detachedServiceProperties.Add(DetachServiceProperty(serviceProperty)!);
                        }
                    }

                    foreach (var ignoredMember in Metadata.GetIgnoredMembers().ToList())
                    {
                        if (baseEntityType.FindIgnoredConfigurationSource(ignoredMember)
                            .Overrides(Metadata.FindDeclaredIgnoredConfigurationSource(ignoredMember)))
                        {
                            Metadata.RemoveIgnored(ignoredMember);
                        }
                    }

                    baseEntityType.UpdateConfigurationSource(configurationSource);
                }

                List<InternalIndexBuilder>? detachedIndexes = null;
                HashSet<Property>? removedInheritedPropertiesToDuplicate = null;
                if (Metadata.BaseType != null)
                {
                    var removedInheritedProperties = new HashSet<Property>(
                        Metadata.BaseType.GetProperties()
                            .Where(p => baseEntityType == null || baseEntityType.FindProperty(p.Name) != p));
                    if (removedInheritedProperties.Count != 0)
                    {
                        removedInheritedPropertiesToDuplicate = new HashSet<Property>();
                        List<ForeignKey>? relationshipsToBeDetached = null;
                        foreach (var foreignKey in Metadata.GetDerivedTypesInclusive()
                            .SelectMany(t => t.GetDeclaredForeignKeys()))
                        {
                            var shouldBeDetached = false;
                            foreach (var property in foreignKey.Properties)
                            {
                                if (removedInheritedProperties.Contains(property))
                                {
                                    removedInheritedPropertiesToDuplicate.Add(property);
                                    shouldBeDetached = true;
                                }
                            }

                            if (!shouldBeDetached)
                            {
                                continue;
                            }

                            if (relationshipsToBeDetached == null)
                            {
                                relationshipsToBeDetached = new List<ForeignKey>();
                            }

                            relationshipsToBeDetached.Add(foreignKey);
                        }

                        foreach (var key in Metadata.GetKeys())
                        {
                            if (key.ReferencingForeignKeys == null
                                || !key.ReferencingForeignKeys.Any()
                                || !key.Properties.Any(p => removedInheritedProperties.Contains(p)))
                            {
                                continue;
                            }

                            foreach (var referencingForeignKey in key.ReferencingForeignKeys.ToList())
                            {
                                if (Metadata.IsAssignableFrom(referencingForeignKey.PrincipalEntityType))
                                {
                                    if (relationshipsToBeDetached == null)
                                    {
                                        relationshipsToBeDetached = new List<ForeignKey>();
                                    }

                                    relationshipsToBeDetached.Add(referencingForeignKey);
                                }
                            }
                        }

                        if (relationshipsToBeDetached != null)
                        {
                            detachedRelationships = new List<RelationshipSnapshot>();
                            foreach (var relationshipToBeDetached in relationshipsToBeDetached)
                            {
                                detachedRelationships.Add(DetachRelationship(relationshipToBeDetached));
                            }
                        }

                        List<Index>? indexesToBeDetached = null;
                        foreach (var index in Metadata.GetDerivedTypesInclusive().SelectMany(e => e.GetDeclaredIndexes()))
                        {
                            var shouldBeDetached = false;
                            foreach (var property in index.Properties)
                            {
                                if (removedInheritedProperties.Contains(property))
                                {
                                    removedInheritedPropertiesToDuplicate.Add(property);
                                    shouldBeDetached = true;
                                }
                            }

                            if (!shouldBeDetached)
                            {
                                continue;
                            }

                            if (indexesToBeDetached == null)
                            {
                                indexesToBeDetached = new List<Index>();
                            }

                            indexesToBeDetached.Add(index);
                        }

                        if (indexesToBeDetached != null)
                        {
                            detachedIndexes = new List<InternalIndexBuilder>();
                            foreach (var indexToBeDetached in indexesToBeDetached)
                            {
                                detachedIndexes.Add(DetachIndex(indexToBeDetached));
                            }
                        }
                    }
                }

                Metadata.SetBaseType(baseEntityType, configurationSource);

                if (removedInheritedPropertiesToDuplicate != null)
                {
                    foreach (var property in removedInheritedPropertiesToDuplicate)
                    {
                        if (property.IsInModel)
                        {
                            property.Builder.Attach(this);
                        }
                    }
                }

                if (detachedServiceProperties != null)
                {
                    foreach (var detachedServiceProperty in detachedServiceProperties)
                    {
                        detachedServiceProperty.Attach(detachedServiceProperty.Metadata.DeclaringEntityType.Builder);
                    }
                }

                detachedProperties?.Attach(this);

                if (detachedKeys != null)
                {
                    foreach (var detachedKeyTuple in detachedKeys)
                    {
                        var newKeyBuilder = detachedKeyTuple.Item1.Attach(Metadata.RootType().Builder, detachedKeyTuple.Item2);
                        if (newKeyBuilder == null
                            && detachedKeyTuple.Item1.Metadata.GetConfigurationSource() == ConfigurationSource.Explicit)
                        {
                            throw new InvalidOperationException(CoreStrings.DerivedEntityCannotHaveKeys(Metadata.DisplayName()));
                        }
                    }
                }

                if (detachedIndexes != null)
                {
                    foreach (var detachedIndex in detachedIndexes)
                    {
                        detachedIndex.Attach(detachedIndex.Metadata.DeclaringEntityType.Builder);
                    }
                }

                if (detachedSkipNavigations != null)
                {
                    foreach (var detachedSkipNavigation in detachedSkipNavigations)
                    {
                        detachedSkipNavigation.Attach();
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

            List<T>? FindConflictingMembers<T>(
                IEnumerable<T> derivedMembers,
                Dictionary<string, ConfigurationSource?> baseMemberNames,
                Func<T, bool> compatibleWithBaseMember,
                Action<T> removeMember)
                where T : PropertyBase
            {
                List<T>? membersToBeDetached = null;
                List<T>? membersToBeRemoved = null;
                foreach (var member in derivedMembers)
                {
                    ConfigurationSource? baseConfigurationSource = null;
                    if ((!member.GetConfigurationSource().OverridesStrictly(
                                baseEntityType.FindIgnoredConfigurationSource(member.Name))
                            && member.GetConfigurationSource() != ConfigurationSource.Explicit)
                        || (baseMemberNames.TryGetValue(member.Name, out baseConfigurationSource)
                            && baseConfigurationSource.Overrides(member.GetConfigurationSource())
                            && !compatibleWithBaseMember(member)))
                    {
                        if (baseConfigurationSource == ConfigurationSource.Explicit
                            && configurationSource == ConfigurationSource.Explicit
                            && member.GetConfigurationSource() == ConfigurationSource.Explicit)
                        {
                            continue;
                        }

                        if (membersToBeRemoved == null)
                        {
                            membersToBeRemoved = new List<T>();
                        }

                        membersToBeRemoved.Add(member);
                        continue;
                    }

                    if (baseConfigurationSource != null)
                    {
                        if (membersToBeDetached == null)
                        {
                            membersToBeDetached = new List<T>();
                        }

                        membersToBeDetached.Add(member);
                    }
                }

                if (membersToBeRemoved != null)
                {
                    foreach (var memberToBeRemoved in membersToBeRemoved)
                    {
                        removeMember(memberToBeRemoved);
                    }
                }

                return membersToBeDetached;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetBaseType(EntityType? baseEntityType, ConfigurationSource configurationSource)
        {
            if (Metadata.BaseType == baseEntityType
                || configurationSource == ConfigurationSource.Explicit)
            {
                return true;
            }

            if (!configurationSource.Overrides(Metadata.GetBaseTypeConfigurationSource()))
            {
                return false;
            }

            if (baseEntityType == null)
            {
                return true;
            }

            var configurationSourceForRemoval = ConfigurationSource.DataAnnotation.Max(configurationSource);
            if (Metadata.GetDeclaredKeys().Any(k => !configurationSourceForRemoval.Overrides(k.GetConfigurationSource())
                && k.Properties.Any(p => baseEntityType.FindProperty(p.Name) == null))
                || (Metadata.IsKeyless && !configurationSource.Overrides(Metadata.GetIsKeylessConfigurationSource())))
            {
                return false;
            }

            if (Metadata.GetDerivedTypesInclusive()
                .SelectMany(t => t.GetDeclaredForeignKeys())
                .Where(fk => fk.Properties.Any(p => baseEntityType.FindProperty(p.Name)?.IsKey() == true))
                .Any(fk => !configurationSourceForRemoval.Overrides(fk.GetPropertiesConfigurationSource())))
            {
                return false;
            }

            var baseMembers = baseEntityType.GetMembers()
                .Where(m => m.GetConfigurationSource() == ConfigurationSource.Explicit)
                .ToDictionary(m => m.Name);

            foreach (var derivedMember in Metadata.GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredMembers()))
            {
                if (derivedMember.GetConfigurationSource() == ConfigurationSource.Explicit
                    && baseMembers.TryGetValue(derivedMember.Name, out var baseMember))
                {
                    if (derivedMember is IReadOnlyProperty)
                    {
                        return baseMember is IReadOnlyProperty;
                    }

                    if (derivedMember is IReadOnlyNavigation derivedNavigation)
                    {
                        return baseMember is IReadOnlyNavigation baseNavigation
                            && derivedNavigation.TargetEntityType == baseNavigation.TargetEntityType;
                    }

                    if (derivedMember is IReadOnlyServiceProperty)
                    {
                        return baseMember is IReadOnlyServiceProperty;
                    }

                    if (derivedMember is IReadOnlySkipNavigation derivedSkipNavigation)
                    {
                        return baseMember is IReadOnlySkipNavigation baseSkipNavigation
                            && derivedSkipNavigation.TargetEntityType == baseSkipNavigation.TargetEntityType;
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static PropertiesSnapshot? DetachProperties(IReadOnlyList<Property> propertiesToDetach)
        {
            if (propertiesToDetach.Count == 0)
            {
                return null;
            }

            List<RelationshipSnapshot>? detachedRelationships = null;
            foreach (var propertyToDetach in propertiesToDetach)
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

            var detachedIndexes = DetachIndexes(propertiesToDetach.SelectMany(p => p.GetContainingIndexes()).Distinct());

            var keysToDetach = propertiesToDetach.SelectMany(p => p.GetContainingKeys()).Distinct().ToList();
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
            foreach (var propertyToDetach in propertiesToDetach)
            {
                var property = propertyToDetach.DeclaringEntityType.FindDeclaredProperty(propertyToDetach.Name);
                if (property != null)
                {
                    var propertyBuilder = property.Builder;
                    // Reset convention configuration
                    propertyBuilder.ValueGenerated(null, ConfigurationSource.Convention);
                    propertyBuilder.AfterSave(null, ConfigurationSource.Convention);
                    propertyBuilder.BeforeSave(null, ConfigurationSource.Convention);
                    ConfigurationSource? removedConfigurationSource;
                    if (property.DeclaringEntityType.IsInModel)
                    {
                        removedConfigurationSource = property.DeclaringEntityType.Builder
                            .RemoveProperty(property, property.GetConfigurationSource());
                    }
                    else
                    {
                        removedConfigurationSource = property.GetConfigurationSource();
                        property.DeclaringEntityType.RemoveProperty(property.Name);
                    }

                    Check.DebugAssert(removedConfigurationSource.HasValue, "removedConfigurationSource.HasValue is false");
                    detachedProperties.Add(propertyBuilder);
                }
            }

            return new PropertiesSnapshot(detachedProperties, detachedIndexes, detachedKeys, detachedRelationships);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanRemoveForeignKey(ForeignKey foreignKey, ConfigurationSource configurationSource)
        {
            Check.DebugAssert(foreignKey.DeclaringEntityType == Metadata, "foreignKey.DeclaringEntityType != Metadata");

            return configurationSource.Overrides(foreignKey.GetConfigurationSource());
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanRemoveSkipNavigation(SkipNavigation skipNavigation, ConfigurationSource? configurationSource)
        {
            Check.DebugAssert(skipNavigation.DeclaringEntityType == Metadata, "skipNavigation.DeclaringEntityType != Metadata");

            return configurationSource.Overrides(skipNavigation.GetConfigurationSource());
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static RelationshipSnapshot DetachRelationship(ForeignKey foreignKey)
            => DetachRelationship(foreignKey, false);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static RelationshipSnapshot DetachRelationship(ForeignKey foreignKey, bool includeOwnedSharedType)
        {
            var detachedBuilder = foreignKey.Builder;
            var referencingSkipNavigations = foreignKey.ReferencingSkipNavigations?
                .Select(s => (s, s.GetForeignKeyConfigurationSource()!.Value)).ToList();
            var relationshipConfigurationSource = foreignKey.DeclaringEntityType.Builder
                .HasNoRelationship(foreignKey, foreignKey.GetConfigurationSource());
            Check.DebugAssert(relationshipConfigurationSource != null, "relationshipConfigurationSource is null");

            EntityType.Snapshot? ownedSnapshot = null;
            var dependentEntityType = foreignKey.DeclaringEntityType;
            if (includeOwnedSharedType
                && foreignKey.IsOwnership
                && dependentEntityType.HasSharedClrType)
            {
                ownedSnapshot = DetachAllMembers(dependentEntityType);
                dependentEntityType.Model.Builder.HasNoEntityType(dependentEntityType, ConfigurationSource.Explicit);
            }

            return new RelationshipSnapshot(detachedBuilder, ownedSnapshot, referencingSkipNavigations);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder? HasNoRelationship(
            ForeignKey foreignKey,
            ConfigurationSource configurationSource)
        {
            if (!foreignKey.IsInModel)
            {
                return this;
            }

            var currentConfigurationSource = foreignKey.GetConfigurationSource();
            if (!configurationSource.Overrides(currentConfigurationSource))
            {
                return null;
            }

            if (foreignKey.ReferencingSkipNavigations != null)
            {
                foreach (var referencingSkipNavigation in foreignKey.ReferencingSkipNavigations.ToList())
                {
                    Check.DebugAssert(
                        currentConfigurationSource.Overrides(referencingSkipNavigation.GetForeignKeyConfigurationSource()),
                        "Setting the FK on the skip navigation should upgrade the configuration source");

                    referencingSkipNavigation.Builder.HasForeignKey(null, configurationSource);
                }
            }

            if (!foreignKey.IsInModel)
            {
                return this;
            }

            Metadata.RemoveForeignKey(foreignKey);

            RemoveUnusedImplicitProperties(foreignKey.Properties);
            if (foreignKey.PrincipalKey.DeclaringEntityType.IsInModel)
            {
                foreignKey.PrincipalKey.DeclaringEntityType.Builder.RemoveKeyIfUnused(foreignKey.PrincipalKey);
            }

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static EntityType.Snapshot? DetachAllMembers(EntityType entityType)
        {
            if (!entityType.IsInModel)
            {
                return null;
            }

            List<RelationshipSnapshot>? detachedRelationships = null;
            foreach (var relationshipToBeDetached in entityType.GetDeclaredForeignKeys().ToList())
            {
                if (detachedRelationships == null)
                {
                    detachedRelationships = new List<RelationshipSnapshot>();
                }

                var detachedRelationship = DetachRelationship(relationshipToBeDetached, false);
                if (detachedRelationship.Relationship.Metadata.GetConfigurationSource().Overrides(ConfigurationSource.DataAnnotation)
                    || relationshipToBeDetached.IsOwnership)
                {
                    detachedRelationships.Add(detachedRelationship);
                }
            }

            List<InternalSkipNavigationBuilder>? detachedSkipNavigations = null;
            foreach (var skipNavigationsToBeDetached in entityType.GetDeclaredSkipNavigations().ToList())
            {
                if (detachedSkipNavigations == null)
                {
                    detachedSkipNavigations = new List<InternalSkipNavigationBuilder>();
                }

                detachedSkipNavigations.Add(DetachSkipNavigation(skipNavigationsToBeDetached)!);
            }

            List<(InternalKeyBuilder, ConfigurationSource?)>? detachedKeys = null;
            foreach (var keyToDetach in entityType.GetDeclaredKeys().ToList())
            {
                foreach (var relationshipToBeDetached in keyToDetach.GetReferencingForeignKeys().ToList())
                {
                    if (detachedRelationships == null)
                    {
                        detachedRelationships = new List<RelationshipSnapshot>();
                    }

                    var detachedRelationship = DetachRelationship(relationshipToBeDetached, true);
                    if (detachedRelationship.Relationship.Metadata.GetConfigurationSource().Overrides(ConfigurationSource.DataAnnotation)
                        || relationshipToBeDetached.IsOwnership)
                    {
                        detachedRelationships.Add(detachedRelationship);
                    }
                }

                if (!keyToDetach.IsInModel)
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

            List<InternalIndexBuilder>? detachedIndexes = null;
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

            var detachedProperties = DetachProperties(entityType.GetDeclaredProperties().ToList());

            List<InternalServicePropertyBuilder>? detachedServiceProperties = null;
            foreach (var servicePropertiesToBeDetached in entityType.GetDeclaredServiceProperties().ToList())
            {
                if (detachedServiceProperties == null)
                {
                    detachedServiceProperties = new List<InternalServicePropertyBuilder>();
                }

                detachedServiceProperties.Add(DetachServiceProperty(servicePropertiesToBeDetached)!);
            }

            return new EntityType.Snapshot(
                entityType,
                detachedProperties,
                detachedIndexes,
                detachedKeys,
                detachedRelationships,
                detachedSkipNavigations,
                detachedServiceProperties);
        }

        private void RemoveKeyIfUnused(Key key, ConfigurationSource configurationSource = ConfigurationSource.Convention)
        {
            if (Metadata.FindPrimaryKey() == key
                || key.ReferencingForeignKeys?.Any() == true)
            {
                return;
            }

            HasNoKey(key, configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder RemoveUnusedImplicitProperties<T>(IReadOnlyList<T> properties)
            where T : class, IConventionProperty
        {
            foreach (var property in properties)
            {
                if (property?.IsInModel == true && property.IsImplicitlyCreated())
                {
                    RemovePropertyIfUnused((Property)(object)property, ConfigurationSource.Convention);
                }
            }

            return this;
        }

        private static void RemovePropertyIfUnused(Property property, ConfigurationSource configurationSource)
        {
            if (!property.IsInModel
                || !property.DeclaringEntityType.Builder.CanRemoveProperty(property, configurationSource)
                || property.GetContainingIndexes().Any()
                || property.GetContainingForeignKeys().Any()
                || property.GetContainingKeys().Any())
            {
                return;
            }

            var removedProperty = property.DeclaringEntityType.RemoveProperty(property.Name);
            Check.DebugAssert(removedProperty == property, "removedProperty != property");
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalIndexBuilder? HasIndex(IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => HasIndex(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalIndexBuilder? HasIndex(
            IReadOnlyList<string> propertyNames,
            string name,
            ConfigurationSource configurationSource)
            => HasIndex(GetOrCreateProperties(propertyNames, configurationSource), name, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalIndexBuilder? HasIndex(
            IReadOnlyList<MemberInfo> clrMembers,
            ConfigurationSource configurationSource)
            => HasIndex(GetOrCreateProperties(clrMembers, configurationSource), configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalIndexBuilder? HasIndex(
            IReadOnlyList<MemberInfo> clrMembers,
            string name,
            ConfigurationSource configurationSource)
            => HasIndex(GetOrCreateProperties(clrMembers, configurationSource), name, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalIndexBuilder? HasIndex(
            IReadOnlyList<Property>? properties,
            ConfigurationSource configurationSource)
        {
            if (properties == null)
            {
                return null;
            }

            List<InternalIndexBuilder>? detachedIndexes = null;
            var existingIndex = Metadata.FindIndex(properties);
            if (existingIndex == null)
            {
                detachedIndexes = Metadata.FindDerivedIndexes(properties).ToList().Select(DetachIndex).ToList();
            }
            else if (existingIndex.DeclaringEntityType != Metadata)
            {
                return existingIndex.DeclaringEntityType.Builder.HasIndex(existingIndex, properties, null, configurationSource);
            }

            var indexBuilder = HasIndex(existingIndex, properties, null, configurationSource);

            if (detachedIndexes != null)
            {
                foreach (var detachedIndex in detachedIndexes)
                {
                    detachedIndex.Attach(detachedIndex.Metadata.DeclaringEntityType.Builder);
                }
            }

            return indexBuilder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalIndexBuilder? HasIndex(
            IReadOnlyList<Property>? properties,
            string name,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(name, nameof(name));

            if (properties == null)
            {
                return null;
            }

            List<InternalIndexBuilder>? detachedIndexes = null;

            var existingIndex = Metadata.FindIndex(name);
            if (existingIndex != null
                && !existingIndex.Properties.SequenceEqual(properties))
            {
                // use existing index only if properties match
                existingIndex = null;
            }

            if (existingIndex == null)
            {
                detachedIndexes = Metadata.FindDerivedIndexes(name)
                    .Where(i => i.Properties.SequenceEqual(properties))
                    .ToList().Select(DetachIndex).ToList();
            }
            else if (existingIndex.DeclaringEntityType != Metadata)
            {
                return existingIndex.DeclaringEntityType.Builder.HasIndex(existingIndex, properties, name, configurationSource);
            }

            var indexBuilder = HasIndex(existingIndex, properties, name, configurationSource);

            if (detachedIndexes != null)
            {
                foreach (var detachedIndex in detachedIndexes)
                {
                    detachedIndex.Attach(detachedIndex.Metadata.DeclaringEntityType.Builder);
                }
            }

            return indexBuilder;
        }

        private InternalIndexBuilder? HasIndex(
            Index? index,
            IReadOnlyList<Property> properties,
            string? name,
            ConfigurationSource configurationSource)
        {
            if (index == null)
            {
                if (name == null)
                {
                    index = Metadata.AddIndex(properties, configurationSource);
                }
                else
                {
                    index = Metadata.AddIndex(properties, name, configurationSource);
                }
            }
            else
            {
                index.UpdateConfigurationSource(configurationSource);
            }

            return index?.Builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder? HasNoIndex(Index index, ConfigurationSource configurationSource)
        {
            var currentConfigurationSource = index.GetConfigurationSource();
            if (!configurationSource.Overrides(currentConfigurationSource))
            {
                return null;
            }

            var removedIndex = index.Name == null
                ? Metadata.RemoveIndex(index.Properties)
                : Metadata.RemoveIndex(index.Name);
            Check.DebugAssert(removedIndex == index, "removedIndex != index");

            RemoveUnusedImplicitProperties(index.Properties);

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanRemoveIndex(Index index, ConfigurationSource configurationSource)
            => configurationSource.Overrides(index.GetConfigurationSource());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static List<InternalIndexBuilder>? DetachIndexes(IEnumerable<Index> indexesToDetach)
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
            var removedConfigurationSource = entityTypeBuilder.HasNoIndex(indexToDetach, indexToDetach.GetConfigurationSource());
            Check.DebugAssert(removedConfigurationSource != null, "removedConfigurationSource is null");
            return indexBuilder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder? HasRelationship(
            string principalEntityTypeName,
            IReadOnlyList<string> propertyNames,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(principalEntityTypeName, nameof(principalEntityTypeName));
            Check.NotEmpty(propertyNames, nameof(propertyNames));

            var principalTypeBuilder = ModelBuilder.Entity(principalEntityTypeName, configurationSource);
            var principalKey = principalTypeBuilder?.Metadata.FindPrimaryKey();
            return principalTypeBuilder == null
                ? null
                : HasForeignKey(
                    principalTypeBuilder.Metadata,
                    GetOrCreateProperties(
                        propertyNames, configurationSource, principalKey?.Properties, useDefaultType: principalKey == null),
                    null,
                    configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder? HasRelationship(
            string principalEntityTypeName,
            IReadOnlyList<string> propertyNames,
            Key principalKey,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(principalEntityTypeName, nameof(principalEntityTypeName));
            Check.NotEmpty(propertyNames, nameof(propertyNames));

            var principalTypeBuilder = ModelBuilder.Entity(principalEntityTypeName, configurationSource);
            return principalTypeBuilder == null
                ? null
                : HasForeignKey(
                    principalTypeBuilder.Metadata,
                    GetOrCreateProperties(propertyNames, configurationSource, principalKey.Properties),
                    principalKey,
                    configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder? HasRelationship(
            Type principalClrType,
            IReadOnlyList<MemberInfo> clrMembers,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(principalClrType, nameof(principalClrType));
            Check.NotEmpty(clrMembers, nameof(clrMembers));

            var principalTypeBuilder = ModelBuilder.Entity(principalClrType, configurationSource);
            return principalTypeBuilder == null
                ? null
                : HasForeignKey(
                    principalTypeBuilder.Metadata,
                    GetOrCreateProperties(clrMembers, configurationSource),
                    null,
                    configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder? HasRelationship(
            Type principalClrType,
            IReadOnlyList<MemberInfo> clrMembers,
            Key principalKey,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(principalClrType, nameof(principalClrType));
            Check.NotEmpty(clrMembers, nameof(clrMembers));

            var principalTypeBuilder = ModelBuilder.Entity(principalClrType, configurationSource);
            return principalTypeBuilder == null
                ? null
                : HasForeignKey(
                    principalTypeBuilder.Metadata,
                    GetOrCreateProperties(clrMembers, configurationSource),
                    principalKey,
                    configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder? HasRelationship(
            EntityType principalEntityType,
            IReadOnlyList<Property> dependentProperties,
            ConfigurationSource configurationSource)
            => HasForeignKey(
                principalEntityType,
                GetActualProperties(dependentProperties, configurationSource),
                null,
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder? HasRelationship(
            EntityType principalEntityType,
            IReadOnlyList<Property> dependentProperties,
            Key? principalKey,
            ConfigurationSource configurationSource)
            => HasForeignKey(
                principalEntityType,
                GetActualProperties(dependentProperties, configurationSource),
                principalKey,
                configurationSource);

        private InternalForeignKeyBuilder? HasForeignKey(
            EntityType principalEntityType,
            IReadOnlyList<Property>? dependentProperties,
            Key? principalKey,
            ConfigurationSource configurationSource)
        {
            if (dependentProperties == null)
            {
                return null;
            }

            var newRelationship = HasRelationshipInternal(principalEntityType, principalKey, configurationSource)!;

            var relationship = newRelationship.HasForeignKey(dependentProperties, configurationSource);
            if (relationship == null
                && newRelationship.Metadata.IsInModel)
            {
                HasNoRelationship(newRelationship.Metadata, configurationSource);
            }

            newRelationship = relationship;

            return newRelationship;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder? HasRelationship(
            EntityType targetEntityType,
            string? navigationName,
            ConfigurationSource configurationSource,
            bool? targetIsPrincipal = null)
            => HasRelationship(
                Check.NotNull(targetEntityType, nameof(targetEntityType)),
                MemberIdentity.Create(navigationName),
                null,
                targetIsPrincipal,
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder? HasRelationship(
            EntityType targetEntityType,
            MemberInfo? navigationMember,
            ConfigurationSource configurationSource,
            bool? targetIsPrincipal = null)
            => HasRelationship(
                Check.NotNull(targetEntityType, nameof(targetEntityType)),
                MemberIdentity.Create(navigationMember),
                null,
                targetIsPrincipal,
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder? HasRelationship(
            EntityType targetEntityType,
            string? navigationName,
            string? inverseNavigationName,
            ConfigurationSource configurationSource,
            bool setTargetAsPrincipal = false)
            => HasRelationship(
                Check.NotNull(targetEntityType, nameof(targetEntityType)),
                MemberIdentity.Create(navigationName),
                MemberIdentity.Create(inverseNavigationName),
                setTargetAsPrincipal ? true : (bool?)null,
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder? HasRelationship(
            EntityType targetEntityType,
            MemberInfo? navigation,
            MemberInfo? inverseNavigation,
            ConfigurationSource configurationSource,
            bool setTargetAsPrincipal = false)
            => HasRelationship(
                Check.NotNull(targetEntityType, nameof(targetEntityType)),
                MemberIdentity.Create(navigation),
                MemberIdentity.Create(inverseNavigation),
                setTargetAsPrincipal ? true : (bool?)null,
                configurationSource);

        private InternalForeignKeyBuilder? HasRelationship(
            EntityType targetEntityType,
            MemberIdentity? navigationToTarget,
            MemberIdentity? inverseNavigation,
            bool? setTargetAsPrincipal,
            ConfigurationSource configurationSource,
            bool? required = null)
        {
            Check.DebugAssert(
                navigationToTarget != null || inverseNavigation != null,
                "navigationToTarget == null and inverseNavigation == null");

            Check.DebugAssert(
                setTargetAsPrincipal != null || required == null,
                "required should only be set if principal end is known");

            var navigationProperty = navigationToTarget?.MemberInfo;
            if (setTargetAsPrincipal == false
                || (inverseNavigation == null
                    && navigationProperty?.GetMemberType().IsAssignableFrom(
                        targetEntityType.ClrType)
                    == false))
            {
                // Target is expected to be dependent or only one nav specified and it can't be the nav to principal
                return targetEntityType.Builder.HasRelationship(
                    Metadata, null, navigationToTarget, !setTargetAsPrincipal, configurationSource, required);
            }

            if (configurationSource == ConfigurationSource.Explicit
                && setTargetAsPrincipal.HasValue)
            {
                if (setTargetAsPrincipal.Value)
                {
                    if (targetEntityType.IsKeyless
                        && targetEntityType.GetIsKeylessConfigurationSource() == ConfigurationSource.Explicit)
                    {
                        throw new InvalidOperationException(CoreStrings.PrincipalKeylessType(
                            targetEntityType.DisplayName(),
                            targetEntityType.DisplayName()
                            + (inverseNavigation == null
                                ? ""
                                : "." + inverseNavigation.Value.Name),
                            Metadata.DisplayName()
                            + (navigationToTarget == null
                                ? ""
                                : "." + navigationToTarget.Value.Name)));
                    }
                }
                else
                {
                    if (Metadata.IsKeyless
                        && Metadata.GetIsKeylessConfigurationSource() == ConfigurationSource.Explicit)
                    {
                        throw new InvalidOperationException(CoreStrings.PrincipalKeylessType(
                            Metadata.DisplayName(),
                            Metadata.DisplayName()
                            + (navigationToTarget == null
                                ? ""
                                : "." + navigationToTarget.Value.Name),
                            targetEntityType.DisplayName()
                            + (inverseNavigation == null
                                ? ""
                                : "." + inverseNavigation.Value.Name)));
                    }
                }
            }

            var existingRelationship = InternalForeignKeyBuilder.FindCurrentForeignKeyBuilder(
                targetEntityType,
                Metadata,
                navigationToTarget,
                inverseNavigation,
                dependentProperties: null,
                principalProperties: null);
            if (existingRelationship != null)
            {
                var shouldInvert = false;
                // The dependent and principal sides could be in the same hierarchy so we need to use the navigations to determine
                // the expected principal side.
                // And since both sides are in the same hierarchy different navigations must have different names.
                if (navigationToTarget != null)
                {
                    if (navigationToTarget.Value.Name == existingRelationship.Metadata.DependentToPrincipal?.Name)
                    {
                        existingRelationship.Metadata.UpdateDependentToPrincipalConfigurationSource(configurationSource);
                    }
                    else if (setTargetAsPrincipal == true)
                    {
                        shouldInvert = true;
                    }
                    else
                    {
                        existingRelationship.Metadata.UpdatePrincipalToDependentConfigurationSource(configurationSource);
                    }

                    if (navigationToTarget.Value.Name != null)
                    {
                        Metadata.RemoveIgnored(navigationToTarget.Value.Name);
                    }
                }

                if (inverseNavigation != null)
                {
                    if (inverseNavigation.Value.Name == existingRelationship.Metadata.PrincipalToDependent?.Name)
                    {
                        existingRelationship.Metadata.UpdatePrincipalToDependentConfigurationSource(configurationSource);
                    }
                    else if (setTargetAsPrincipal == true)
                    {
                        shouldInvert = true;
                    }
                    else
                    {
                        existingRelationship.Metadata.UpdateDependentToPrincipalConfigurationSource(configurationSource);
                    }

                    if (inverseNavigation.Value.Name != null)
                    {
                        targetEntityType.RemoveIgnored(inverseNavigation.Value.Name);
                    }
                }

                existingRelationship.Metadata.UpdateConfigurationSource(configurationSource);

                if (!shouldInvert)
                {
                    if (setTargetAsPrincipal == true)
                    {
                        existingRelationship = existingRelationship.HasEntityTypes(
                            existingRelationship.Metadata.PrincipalEntityType,
                            existingRelationship.Metadata.DeclaringEntityType,
                            configurationSource)!;

                        if (required.HasValue)
                        {
                            existingRelationship = existingRelationship.IsRequired(required.Value, configurationSource);
                        }
                    }

                    return existingRelationship;
                }

                // If relationship should be inverted it will be handled below
            }
            else
            {
                existingRelationship = InternalForeignKeyBuilder.FindCurrentForeignKeyBuilder(
                    Metadata,
                    targetEntityType,
                    inverseNavigation,
                    navigationToTarget,
                    dependentProperties: null,
                    principalProperties: null);
                if (existingRelationship != null)
                {
                    // Since the existing relationship didn't match the first case then the dependent and principal sides
                    // are not in the same hierarchy therefore we don't need to check existing navigations
                    if (navigationToTarget != null)
                    {
                        Check.DebugAssert(
                            navigationToTarget.Value.Name == existingRelationship.Metadata.PrincipalToDependent?.Name,
                            $"Expected {navigationToTarget.Value.Name}, found {existingRelationship.Metadata.PrincipalToDependent?.Name}");

                        existingRelationship.Metadata.UpdatePrincipalToDependentConfigurationSource(configurationSource);
                        if (navigationToTarget.Value.Name != null)
                        {
                            Metadata.RemoveIgnored(navigationToTarget.Value.Name);
                        }
                    }

                    if (inverseNavigation != null)
                    {
                        Check.DebugAssert(
                            inverseNavigation.Value.Name == existingRelationship.Metadata.DependentToPrincipal?.Name,
                            $"Expected {inverseNavigation.Value.Name}, found {existingRelationship.Metadata.DependentToPrincipal?.Name}");

                        existingRelationship.Metadata.UpdateDependentToPrincipalConfigurationSource(configurationSource);
                        if (inverseNavigation.Value.Name != null)
                        {
                            targetEntityType.RemoveIgnored(inverseNavigation.Value.Name);
                        }
                    }

                    existingRelationship.Metadata.UpdateConfigurationSource(configurationSource);

                    if (setTargetAsPrincipal == null)
                    {
                        return existingRelationship;
                    }
                }
            }

            InternalForeignKeyBuilder? relationship;
            InternalForeignKeyBuilder? newRelationship = null;
            using (var batcher = Metadata.Model.DelayConventions())
            {
                if (existingRelationship != null)
                {
                    relationship = existingRelationship;
                }
                else
                {
                    if (setTargetAsPrincipal == true
                        || (setTargetAsPrincipal == null
                            && !((IReadOnlyEntityType)targetEntityType).IsInOwnershipPath(Metadata)))
                    {
                        newRelationship = CreateForeignKey(
                            targetEntityType.Builder,
                            dependentProperties: null,
                            principalKey: null,
                            propertyBaseName: navigationProperty?.GetSimpleMemberName(),
                            required,
                            configurationSource);
                    }
                    else
                    {
                        var navigation = navigationToTarget;
                        navigationToTarget = inverseNavigation;
                        inverseNavigation = navigation;

                        navigationProperty = navigationToTarget?.MemberInfo;

                        newRelationship = targetEntityType.Builder.CreateForeignKey(
                            this,
                            dependentProperties: null,
                            principalKey: null,
                            propertyBaseName: navigationProperty?.GetSimpleMemberName(),
                            required: null,
                            configurationSource);
                    }

                    relationship = newRelationship;

                    if (relationship == null)
                    {
                        return null;
                    }
                }

                if (setTargetAsPrincipal == true)
                {
                    relationship = relationship
                        .HasEntityTypes(targetEntityType.Builder.Metadata, Metadata, configurationSource)!;

                    if (required.HasValue)
                    {
                        relationship = relationship.IsRequired(required.Value, configurationSource)!;
                    }
                }

                var inverseProperty = inverseNavigation?.MemberInfo;
                if (inverseNavigation == null)
                {
                    relationship = navigationProperty != null
                        ? relationship.HasNavigation(
                            navigationProperty,
                            pointsToPrincipal: true,
                            configurationSource)
                        : relationship.HasNavigation(
                            navigationToTarget!.Value.Name,
                            pointsToPrincipal: true,
                            configurationSource);
                }
                else if (navigationToTarget == null)
                {
                    relationship = inverseProperty != null
                        ? relationship.HasNavigation(
                            inverseProperty,
                            pointsToPrincipal: false,
                            configurationSource)
                        : relationship.HasNavigation(
                            inverseNavigation.Value.Name,
                            pointsToPrincipal: false,
                            configurationSource);
                }
                else
                {
                    relationship = navigationProperty != null || inverseProperty != null
                        ? relationship.HasNavigations(navigationProperty, inverseProperty, configurationSource)
                        : relationship.HasNavigations(navigationToTarget.Value.Name, inverseNavigation.Value.Name, configurationSource);
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
                if (newRelationship?.Metadata.IsInModel == true)
                {
                    newRelationship.Metadata.DeclaringEntityType.Builder.HasNoRelationship(newRelationship.Metadata, configurationSource);
                }

                return null;
            }

            return relationship;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder? HasRelationship(
            EntityType principalEntityType,
            ConfigurationSource configurationSource,
            bool? required = null,
            string? propertyBaseName = null)
            => HasRelationshipInternal(principalEntityType, principalKey: null, configurationSource, required, propertyBaseName);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder? HasRelationship(
            EntityType principalEntityType,
            Key principalKey,
            ConfigurationSource configurationSource,
            bool? required = null,
            string? propertyBaseName = null)
            => HasRelationshipInternal(principalEntityType, principalKey, configurationSource, required, propertyBaseName);

        private InternalForeignKeyBuilder? HasRelationshipInternal(
            EntityType targetEntityType,
            Key? principalKey,
            ConfigurationSource configurationSource,
            bool? required = null,
            string? propertyBaseName = null)
        {
            InternalForeignKeyBuilder? relationship;
            InternalForeignKeyBuilder? newRelationship;
            using (var batch = Metadata.Model.DelayConventions())
            {
                relationship = CreateForeignKey(
                    targetEntityType.Builder,
                    null,
                    principalKey,
                    propertyBaseName,
                    required,
                    configurationSource)!;

                newRelationship = relationship;
                if (principalKey != null)
                {
                    newRelationship = newRelationship.HasEntityTypes(targetEntityType, Metadata, configurationSource)
                        ?.HasPrincipalKey(principalKey.Properties, configurationSource);
                }

                newRelationship = newRelationship == null ? null : batch.Run(newRelationship);
            }

            if (newRelationship == null)
            {
                if (relationship?.Metadata.IsInModel == true)
                {
                    relationship.Metadata.DeclaringEntityType.Builder.HasNoRelationship(relationship.Metadata, configurationSource);
                }

                return null;
            }

            return newRelationship;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder? HasOwnership(
            string targetEntityTypeName,
            string navigationName,
            ConfigurationSource configurationSource)
            => HasOwnership(
                new TypeIdentity(targetEntityTypeName),
                MemberIdentity.Create(navigationName), inverse: null, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder? HasOwnership(
            Type targetEntityType,
            string navigationName,
            ConfigurationSource configurationSource)
            => HasOwnership(
                new TypeIdentity(targetEntityType, Metadata.Model),
                MemberIdentity.Create(navigationName), inverse: null, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder? HasOwnership(
            Type targetEntityType,
            MemberInfo navigationMember,
            ConfigurationSource configurationSource)
            => HasOwnership(
                new TypeIdentity(targetEntityType, Metadata.Model),
                MemberIdentity.Create(navigationMember), inverse: null, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder? HasOwnership(
            Type targetEntityType,
            MemberIdentity navigation,
            ConfigurationSource configurationSource)
            => HasOwnership(
                new TypeIdentity(targetEntityType, Metadata.Model), navigation, inverse: null, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder? HasOwnership(
            in TypeIdentity typeIdentity,
            MemberIdentity navigation,
            ConfigurationSource configurationSource)
            => HasOwnership(typeIdentity, navigation, inverse: null, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder? HasOwnership(
            Type targetEntityType,
            string navigationPropertyName,
            string? inversePropertyName,
            ConfigurationSource configurationSource)
            => HasOwnership(
                new TypeIdentity(targetEntityType, Metadata.Model),
                MemberIdentity.Create(navigationPropertyName),
                MemberIdentity.Create(inversePropertyName),
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder? HasOwnership(
            Type targetEntityType,
            MemberInfo navigationMember,
            MemberInfo? inverseMember,
            ConfigurationSource configurationSource)
            => HasOwnership(
                new TypeIdentity(targetEntityType, Metadata.Model),
                MemberIdentity.Create(navigationMember),
                MemberIdentity.Create(inverseMember),
                configurationSource);

        private InternalForeignKeyBuilder? HasOwnership(
            in TypeIdentity targetEntityType,
            in MemberIdentity navigation,
            in MemberIdentity? inverse,
            ConfigurationSource configurationSource)
        {
            InternalEntityTypeBuilder? ownedEntityTypeBuilder;
            InternalForeignKeyBuilder? relationship;
            using (var batch = Metadata.Model.DelayConventions())
            {
                var ownership = Metadata.FindOwnership();
                ownedEntityTypeBuilder = GetTargetEntityTypeBuilder(targetEntityType, navigation, configurationSource, targetShouldBeOwned: true);

                var principalBuilder = Metadata.IsInModel
                    ? Metadata.Builder
                    : ownership?.PrincipalEntityType.FindNavigation(ownership.PrincipalToDependent!.Name)?.TargetEntityType is EntityType target
                    && target.IsInModel
                        ? target.Builder
                        : null;

                if (ownedEntityTypeBuilder == null
                    || principalBuilder == null)
                {
                    Check.DebugAssert(configurationSource != ConfigurationSource.Explicit,
                        $"Adding {Metadata.ShortName()}.{navigation.Name} ownership failed because one of the related types doesn't exist.");
                    return null;
                }

                relationship = ownedEntityTypeBuilder.HasRelationship(
                    targetEntityType: principalBuilder.Metadata,
                    navigationToTarget: inverse,
                    inverseNavigation: navigation,
                    setTargetAsPrincipal: true,
                    configurationSource,
                    required: true)!;
                relationship = batch.Run(relationship.IsOwnership(true, configurationSource)!);
            }

            if (relationship is null || !relationship.Metadata.IsInModel)
            {
                if (ownedEntityTypeBuilder.Metadata.IsInModel
                    && ownedEntityTypeBuilder.Metadata.HasSharedClrType)
                {
                    ModelBuilder.HasNoEntityType(ownedEntityTypeBuilder.Metadata, configurationSource);
                }

                return null;
            }

            return relationship;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool RemoveNonOwnershipRelationships(ForeignKey? ownership, ConfigurationSource configurationSource)
        {
            var incompatibleRelationships = Metadata.GetDerivedTypesInclusive()
                .SelectMany(t => t.GetDeclaredForeignKeys())
                .Where(
                    fk => !fk.IsOwnership
                        && fk.PrincipalToDependent != null
                        && !Contains(ownership, fk))
                .Concat(
                    Metadata.GetDerivedTypesInclusive()
                        .SelectMany(t => t.GetDeclaredReferencingForeignKeys())
                        .Where(
                            fk => !fk.IsOwnership
                                && !Contains(fk.DeclaringEntityType.FindOwnership(), fk)))
                .ToList();

            if (incompatibleRelationships.Any(fk => !configurationSource.Overrides(fk.GetConfigurationSource())))
            {
                return false;
            }

            foreach (var foreignKey in incompatibleRelationships)
            {
                // foreignKey.Builder can be null below if calling HasNoRelationship() below
                // affects the other foreign key(s) in incompatibleRelationships
                if (foreignKey.IsInModel)
                {
                    foreignKey.DeclaringEntityType.Builder.HasNoRelationship(foreignKey, configurationSource);
                }
            }

            return true;
        }

        private bool Contains(IReadOnlyForeignKey? inheritedFk, IReadOnlyForeignKey derivedFk)
            => inheritedFk != null
                && inheritedFk.PrincipalEntityType.IsAssignableFrom(derivedFk.PrincipalEntityType)
                && PropertyListComparer.Instance.Equals(inheritedFk.Properties, derivedFk.Properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder? GetTargetEntityTypeBuilder(
            Type targetClrType,
            MemberInfo navigationInfo,
            ConfigurationSource? configurationSource)
            => GetTargetEntityTypeBuilder(
                new TypeIdentity(targetClrType, Metadata.Model), MemberIdentity.Create(navigationInfo), configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder? GetTargetEntityTypeBuilder(
            TypeIdentity targetEntityType,
            MemberIdentity navigation,
            ConfigurationSource? configurationSource,
            bool? targetShouldBeOwned = null)
        {
            var existingNavigation = Metadata.FindNavigation(navigation.Name!);
            if (existingNavigation != null)
            {
                var existingTargetType = existingNavigation.TargetEntityType;
                if ((!targetEntityType.IsNamed
                        || existingTargetType.Name == targetEntityType.Name)
                    && (targetEntityType.Type == null
                        || existingTargetType.ClrType == targetEntityType.Type))
                {
                    Check.DebugAssert(existingNavigation.ForeignKey.IsOwnership
                        || !((IReadOnlyNavigation)existingNavigation).TargetEntityType.IsOwned(),
                        $"Found '{existingNavigation.DeclaringEntityType.ShortName()}.{existingNavigation.Name}'. " +
                        "Owned types should only have ownership navigations point at it");

                    return existingTargetType.HasSharedClrType
                        ? ModelBuilder.SharedTypeEntity(
                            existingTargetType.Name, existingTargetType.ClrType, configurationSource!.Value, targetShouldBeOwned)
                        : ModelBuilder.Entity(existingTargetType.ClrType, configurationSource!.Value, targetShouldBeOwned);
                }

                if (configurationSource == null
                    || existingNavigation.ForeignKey.DeclaringEntityType.Builder
                        .HasNoRelationship(existingNavigation.ForeignKey, configurationSource.Value) == null)
                {
                    return null;
                }
            }

            if (navigation.MemberInfo == null)
            {
                if (Metadata.GetRuntimeProperties().TryGetValue(navigation.Name!, out var propertyInfo))
                {
                    navigation = new MemberIdentity(propertyInfo);
                }
                else if (Metadata.GetRuntimeFields().TryGetValue(navigation.Name!, out var fieldInfo))
                {
                    navigation = new MemberIdentity(fieldInfo);
                }
            }

            var targetType = targetEntityType.Type;
            if (targetType == null)
            {
                var memberType = navigation.MemberInfo?.GetMemberType();
                if (memberType != null)
                {
                    targetType = memberType.TryGetSequenceType() ?? memberType;

                    if (targetType != null
                        && targetEntityType.Name == Metadata.Model.GetDisplayName(targetType))
                    {
                        targetEntityType = new TypeIdentity(targetType, Metadata.Model);
                    }
                }
            }

            if (targetType == null)
            {
                targetType = Model.DefaultPropertyBagType;
            }

            if (targetShouldBeOwned != true)
            {
                var ownership = Metadata.FindOwnership();
                if (ownership != null)
                {
                    if (targetType.Equals(Metadata.ClrType))
                    {
                        // Avoid infinite recursion on self reference
                        return null;
                    }

                    if (targetType.IsAssignableFrom(ownership.PrincipalEntityType.ClrType))
                    {
                        if (configurationSource.HasValue)
                        {
                            ownership.PrincipalEntityType.UpdateConfigurationSource(configurationSource.Value);
                        }

                        return ownership.PrincipalEntityType.Builder;
                    }
                }
            }

            var targetTypeName = targetEntityType.IsNamed && (targetEntityType.Type != null || targetShouldBeOwned != true)
                ? targetEntityType.Name
                : Metadata.Model.IsShared(targetType)
                    ? Metadata.GetOwnedName(targetEntityType.IsNamed ? targetEntityType.Name : targetType.ShortDisplayName(), navigation.Name!)
                    : Metadata.Model.GetDisplayName(targetType);

            var shouldBeOwned = targetShouldBeOwned ?? Metadata.Model.IsOwned(targetType);
            var targetEntityTypeBuilder = ModelBuilder.Metadata.FindEntityType(targetTypeName)?.Builder;
            if (targetEntityTypeBuilder != null
                && shouldBeOwned)
            {
                var existingOwnership = targetEntityTypeBuilder.Metadata.FindDeclaredOwnership();
                if (existingOwnership != null)
                {
                    if (!configurationSource.Overrides(ConfigurationSource.Explicit)
                        && navigation.MemberInfo != null
                        && Metadata.IsInOwnershipPath(targetType))
                    {
                        return null;
                    }

                    if (targetEntityType.IsNamed
                        && targetEntityType.Type != null)
                    {
                        if (configurationSource == ConfigurationSource.Explicit)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.ClashingNamedOwnedType(
                                    targetTypeName, Metadata.DisplayName(), navigation.Name));
                        }

                        return null;
                    }

                    if (existingOwnership.Builder.MakeDeclaringTypeShared(configurationSource) == null)
                    {
                        return null;
                    }

                    targetEntityTypeBuilder = null;
                    if (!targetEntityType.IsNamed)
                    {
                        targetTypeName = Metadata.GetOwnedName(targetType.ShortDisplayName(), navigation.Name!);
                    }
                }
            }

            if (targetEntityTypeBuilder == null)
            {
                if (configurationSource == null)
                {
                    return null;
                }

                if (Metadata.Model.IsShared(targetType)
                    || targetEntityType.IsNamed)
                {
                    if (shouldBeOwned != true
                        || (!configurationSource.Overrides(ConfigurationSource.Explicit)
                            && navigation.MemberInfo != null
                            && Metadata.IsInOwnershipPath(targetType)))
                    {
                        return null;
                    }

                    targetEntityTypeBuilder = ModelBuilder.SharedTypeEntity(
                        targetTypeName, targetType, configurationSource.Value, shouldBeOwned);
                }
                else
                {
                    targetEntityTypeBuilder = targetEntityType.IsNamed
                        ? targetType == null
                            ? ModelBuilder.Entity(targetTypeName, configurationSource.Value, shouldBeOwned)
                            : ModelBuilder.SharedTypeEntity(targetTypeName, targetType, configurationSource.Value, shouldBeOwned)
                        : ModelBuilder.Entity(targetType, configurationSource.Value, shouldBeOwned);
                }

                if (targetEntityTypeBuilder == null)
                {
                    return null;
                }
            }

            return targetEntityTypeBuilder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder? CreateForeignKey(
            InternalEntityTypeBuilder principalEntityTypeBuilder,
            IReadOnlyList<Property>? dependentProperties,
            Key? principalKey,
            string? propertyBaseName,
            bool? required,
            ConfigurationSource configurationSource)
        {
            using var batch = ModelBuilder.Metadata.DelayConventions();
            var foreignKey = SetOrAddForeignKey(
                foreignKey: null, principalEntityTypeBuilder, dependentProperties, principalKey,
                propertyBaseName, required, configurationSource)!;

            if (required.HasValue
                && foreignKey.IsRequired == required.Value)
            {
                foreignKey.SetIsRequired(required.Value, configurationSource);
            }

            return (InternalForeignKeyBuilder?)batch.Run(foreignKey)?.Builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder? UpdateForeignKey(
            ForeignKey foreignKey,
            IReadOnlyList<Property>? dependentProperties,
            Key? principalKey,
            string? propertyBaseName,
            bool? isRequired,
            ConfigurationSource? configurationSource)
        {
            using var batch = ModelBuilder.Metadata.DelayConventions();
            var updatedForeignKey = SetOrAddForeignKey(
                foreignKey, foreignKey.PrincipalEntityType.Builder, dependentProperties, principalKey,
                propertyBaseName, isRequired, configurationSource)!;

            return (InternalForeignKeyBuilder?)batch.Run(updatedForeignKey)?.Builder;
        }

        private ForeignKey? SetOrAddForeignKey(
            ForeignKey? foreignKey,
            InternalEntityTypeBuilder principalEntityTypeBuilder,
            IReadOnlyList<Property>? dependentProperties,
            Key? principalKey,
            string? propertyBaseName,
            bool? isRequired,
            ConfigurationSource? configurationSource)
        {
            var principalType = principalEntityTypeBuilder.Metadata;
            var principalBaseEntityTypeBuilder = principalType.RootType().Builder;
            if (principalKey == null)
            {
                if (principalType.IsKeyless
                    && !configurationSource.Overrides(principalType.GetIsKeylessConfigurationSource()))
                {
                    return null;
                }

                principalKey = principalType.FindPrimaryKey();
                if (principalKey != null
                    && dependentProperties != null
                    && (!ForeignKey.AreCompatible(
                            principalKey.Properties,
                            dependentProperties,
                            principalType,
                            Metadata,
                            shouldThrow: false)
                        || (foreignKey == null
                            && Metadata.FindForeignKeysInHierarchy(dependentProperties, principalKey, principalType).Any())))
                {
                    principalKey = null;
                }

                if (principalKey == null
                    && foreignKey != null
                    && (dependentProperties == null
                        || ForeignKey.AreCompatible(
                            foreignKey.PrincipalKey.Properties,
                            dependentProperties,
                            principalType,
                            Metadata,
                            shouldThrow: false)))
                {
                    principalKey = foreignKey.PrincipalKey;
                }
            }

            if (dependentProperties != null)
            {
                dependentProperties = GetActualProperties(dependentProperties, ConfigurationSource.Convention)!;
                if (principalKey == null)
                {
                    var principalKeyProperties = principalBaseEntityTypeBuilder.TryCreateUniqueProperties(
                        dependentProperties.Count, null, dependentProperties.Select(p => p.ClrType),
                        Enumerable.Repeat("", dependentProperties.Count), isRequired: true, baseName: "TempId").Item2;

                    principalKey = principalBaseEntityTypeBuilder.HasKeyInternal(principalKeyProperties, ConfigurationSource.Convention)!
                        .Metadata;
                }
                else
                {
                    Check.DebugAssert(
                        foreignKey != null
                        || Metadata.FindForeignKey(dependentProperties, principalKey, principalType) == null,
                        "FK not found");
                }
            }
            else
            {
                if (principalKey == null)
                {
                    var principalKeyProperties = principalBaseEntityTypeBuilder.TryCreateUniqueProperties(
                        1, null, new[] { typeof(int) }, new[] { "TempId" }, isRequired: true, baseName: "").Item2;

                    principalKey = principalBaseEntityTypeBuilder.HasKeyInternal(
                        principalKeyProperties, ConfigurationSource.Convention)?.Metadata;

                    if (principalKey == null)
                    {
                        return null;
                    }
                }

                if (foreignKey != null)
                {
                    var oldProperties = foreignKey.Properties;
                    var oldKey = foreignKey.PrincipalKey;
                    var temporaryProperties = CreateUniqueProperties(null, principalKey.Properties, isRequired ?? false, "TempFk")!;
                    foreignKey.SetProperties(temporaryProperties, principalKey, configurationSource);

                    foreignKey.DeclaringEntityType.Builder.RemoveUnusedImplicitProperties(oldProperties);
                    if (oldKey != principalKey)
                    {
                        oldKey.DeclaringEntityType.Builder.RemoveKeyIfUnused(oldKey);
                    }

                    propertyBaseName ??= ForeignKeyPropertyDiscoveryConvention.GetPropertyBaseName(foreignKey);
                }

                var baseName = string.IsNullOrEmpty(propertyBaseName)
                    ? principalType.ShortName()
                    : propertyBaseName;
                dependentProperties = CreateUniqueProperties(null, principalKey.Properties, isRequired ?? false, baseName)!;
            }

            if (foreignKey == null)
            {
                return Metadata.AddForeignKey(
                    dependentProperties, principalKey, principalType, componentConfigurationSource: null, configurationSource!.Value);
            }

            var oldFKProperties = foreignKey.Properties;
            var oldPrincipalKey = foreignKey.PrincipalKey;
            foreignKey.SetProperties(dependentProperties, principalKey, configurationSource);

            if (oldFKProperties != dependentProperties)
            {
                foreignKey.DeclaringEntityType.Builder.RemoveUnusedImplicitProperties(oldFKProperties);
            }

            if (oldPrincipalKey != principalKey)
            {
                oldPrincipalKey.DeclaringEntityType.Builder.RemoveKeyIfUnused(oldPrincipalKey);
            }

            return foreignKey;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalSkipNavigationBuilder? HasSkipNavigation(
            MemberIdentity navigation,
            EntityType targetEntityType,
            MemberIdentity inverseNavigation,
            ConfigurationSource configurationSource,
            bool? collections = null,
            bool? onDependent = null)
        {
            var skipNavigationBuilder = HasSkipNavigation(
                navigation, targetEntityType, configurationSource, collections, onDependent);
            if (skipNavigationBuilder == null)
            {
                return null;
            }

            var inverseSkipNavigationBuilder = targetEntityType.Builder.HasSkipNavigation(
                inverseNavigation, Metadata, configurationSource, collections, onDependent);
            if (inverseSkipNavigationBuilder == null)
            {
                HasNoSkipNavigation(skipNavigationBuilder.Metadata, configurationSource);
                return null;
            }

            return skipNavigationBuilder.HasInverse(inverseSkipNavigationBuilder.Metadata, configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalSkipNavigationBuilder? HasSkipNavigation(
            MemberIdentity navigationProperty,
            EntityType targetEntityType,
            ConfigurationSource? configurationSource,
            bool? collection = null,
            bool? onDependent = null)
        {
            List<SkipNavigation>? navigationsToDetach = null;
            List<(InternalSkipNavigationBuilder Navigation, InternalSkipNavigationBuilder Inverse)>? detachedNavigations = null;
            var navigationName = navigationProperty.Name!;
            var memberInfo = navigationProperty.MemberInfo;
            var existingNavigation = Metadata.FindSkipNavigation(navigationName);
            if (existingNavigation != null)
            {
                Check.DebugAssert(
                    memberInfo == null || memberInfo.IsSameAs(existingNavigation.GetIdentifyingMemberInfo()),
                    "Expected memberInfo to be the same on the existing navigation");

                Check.DebugAssert(
                    collection == null || collection == existingNavigation.IsCollection,
                    "Expected existing navigation to have the same cardinality");

                Check.DebugAssert(
                    onDependent == null || onDependent == existingNavigation.IsOnDependent,
                    "Expected existing navigation to be on the same side");

                if (existingNavigation.DeclaringEntityType != Metadata)
                {
                    if (!IsIgnored(navigationName, configurationSource))
                    {
                        Metadata.RemoveIgnored(navigationName);
                    }
                }

                if (configurationSource.HasValue)
                {
                    existingNavigation.UpdateConfigurationSource(configurationSource.Value);
                }

                return existingNavigation.Builder;
            }

            if (!configurationSource.HasValue
                || IsIgnored(navigationName, configurationSource))
            {
                return null;
            }

            foreach (var conflictingMember in Metadata.FindPropertiesInHierarchy(navigationName).Cast<PropertyBase>()
                .Concat(Metadata.FindNavigationsInHierarchy(navigationName))
                .Concat(Metadata.FindServicePropertiesInHierarchy(navigationName)))
            {
                if (!configurationSource.Overrides(conflictingMember.GetConfigurationSource()))
                {
                    return null;
                }
            }

            foreach (var derivedType in Metadata.GetDerivedTypes())
            {
                var conflictingNavigation = derivedType.FindDeclaredSkipNavigation(navigationName);
                if (conflictingNavigation != null)
                {
                    if (navigationsToDetach == null)
                    {
                        navigationsToDetach = new List<SkipNavigation>();
                    }

                    navigationsToDetach.Add(conflictingNavigation);
                }
            }

            if (collection == null
                && navigationProperty.MemberInfo != null)
            {
                var navigationType = navigationProperty.MemberInfo.GetMemberType();
                var navigationTargetClrType = navigationType.TryGetSequenceType();
                collection = navigationTargetClrType != null
                    && navigationType != targetEntityType.ClrType
                    && navigationTargetClrType.IsAssignableFrom(targetEntityType.ClrType);
            }

            InternalSkipNavigationBuilder builder;
            using (ModelBuilder.Metadata.DelayConventions())
            {
                Metadata.RemoveIgnored(navigationName);

                foreach (var conflictingProperty in Metadata.FindPropertiesInHierarchy(navigationName))
                {
                    if (conflictingProperty.GetConfigurationSource() != ConfigurationSource.Explicit)
                    {
                        conflictingProperty.DeclaringEntityType.RemoveProperty(conflictingProperty);
                    }
                }

                foreach (var conflictingServiceProperty in Metadata.FindServicePropertiesInHierarchy(navigationName))
                {
                    if (conflictingServiceProperty.GetConfigurationSource() != ConfigurationSource.Explicit)
                    {
                        conflictingServiceProperty.DeclaringEntityType.RemoveServiceProperty(conflictingServiceProperty);
                    }
                }

                foreach (var conflictingNavigation in Metadata.FindNavigationsInHierarchy(navigationName))
                {
                    if (conflictingNavigation.GetConfigurationSource() == ConfigurationSource.Explicit)
                    {
                        continue;
                    }

                    var conflictingForeignKey = conflictingNavigation.ForeignKey;
                    if (conflictingForeignKey.GetConfigurationSource() == ConfigurationSource.Convention)
                    {
                        conflictingForeignKey.DeclaringEntityType.Builder.HasNoRelationship(
                            conflictingForeignKey, ConfigurationSource.Convention);
                    }
                    else if (conflictingForeignKey.Builder.HasNavigation(
                            (string?)null,
                            conflictingNavigation.IsOnDependent,
                            configurationSource.Value)
                        == null)
                    {
                        return null;
                    }
                }

                if (navigationsToDetach != null)
                {
                    detachedNavigations = new List<(InternalSkipNavigationBuilder, InternalSkipNavigationBuilder)>();
                    foreach (var navigationToDetach in navigationsToDetach)
                    {
                        var inverse = navigationToDetach.Inverse;
                        detachedNavigations.Add((DetachSkipNavigation(navigationToDetach)!, DetachSkipNavigation(inverse)!));
                    }
                }

                builder = Metadata.AddSkipNavigation(
                    navigationName, navigationProperty.MemberInfo,
                    targetEntityType, collection ?? true, onDependent ?? false, configurationSource.Value)!.Builder;

                if (detachedNavigations != null)
                {
                    foreach (var detachedSkipNavigationTuple in detachedNavigations)
                    {
                        detachedSkipNavigationTuple.Navigation.Attach(this, inverseBuilder: detachedSkipNavigationTuple.Inverse);
                    }
                }
            }

            return builder.Metadata.IsInModel
                ? builder
                : Metadata.FindSkipNavigation(navigationName)?.Builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder? HasNoSkipNavigation(
            SkipNavigation skipNavigation,
            ConfigurationSource configurationSource)
        {
            if (!CanRemoveSkipNavigation(skipNavigation, configurationSource))
            {
                return null;
            }

            if (skipNavigation.Inverse != null)
            {
                var removed = skipNavigation.Inverse.Builder.HasInverse(null, configurationSource);
                Check.DebugAssert(removed != null, "Expected inverse to be removed");
            }

            Metadata.RemoveSkipNavigation(skipNavigation);

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanRemoveSkipNavigation(SkipNavigation skipNavigation, ConfigurationSource configurationSource)
            => configurationSource.Overrides(skipNavigation.GetConfigurationSource());

        private static InternalSkipNavigationBuilder? DetachSkipNavigation(SkipNavigation? skipNavigationToDetach)
        {
            if (skipNavigationToDetach is null || !skipNavigationToDetach.IsInModel)
            {
                return null;
            }

            var builder = skipNavigationToDetach.Builder;
            skipNavigationToDetach.DeclaringEntityType.Builder.HasNoSkipNavigation(skipNavigationToDetach, ConfigurationSource.Explicit);
            return builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool ShouldReuniquifyTemporaryProperties(ForeignKey foreignKey)
            => TryCreateUniqueProperties(
                    foreignKey.PrincipalKey.Properties.Count,
                    foreignKey.Properties,
                    foreignKey.PrincipalKey.Properties.Select(p => p.ClrType),
                    foreignKey.PrincipalKey.Properties.Select(p => p.Name),
                    foreignKey.IsRequired
                    && foreignKey.GetIsRequiredConfigurationSource().Overrides(ConfigurationSource.Convention),
                    foreignKey.DependentToPrincipal?.Name
                    ?? foreignKey.ReferencingSkipNavigations?.FirstOrDefault()?.Inverse?.Name
                    ?? foreignKey.PrincipalEntityType.ShortName())
                .Item1;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder? CreateUniqueProperty(
            Type propertyType,
            string propertyName,
            bool required)
            => CreateUniqueProperties(
                new[] { propertyType },
                new[] { propertyName },
                required).First().Builder;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<Property> CreateUniqueProperties(
            IReadOnlyList<Type> propertyTypes,
            IReadOnlyList<string> propertyNames,
            bool isRequired)
            => TryCreateUniqueProperties(
                propertyNames.Count,
                null,
                propertyTypes,
                propertyNames,
                isRequired,
                "").Item2!;

        private IReadOnlyList<Property>? CreateUniqueProperties(
            IReadOnlyList<Property>? currentProperties,
            IReadOnlyList<Property> principalProperties,
            bool isRequired,
            string baseName)
            => TryCreateUniqueProperties(
                principalProperties.Count,
                currentProperties,
                principalProperties.Select(p => p.ClrType),
                principalProperties.Select(p => p.Name),
                isRequired,
                baseName).Item2;

        private (bool, IReadOnlyList<Property>?) TryCreateUniqueProperties(
            int propertyCount,
            IReadOnlyList<Property>? currentProperties,
            IEnumerable<Type> principalPropertyTypes,
            IEnumerable<string> principalPropertyNames,
            bool isRequired,
            string baseName)
        {
            var newProperties = currentProperties == null ? new Property[propertyCount] : null;
            var clrProperties = Metadata.GetRuntimeProperties();
            var clrFields = Metadata.GetRuntimeFields();
            var canReuniquify = false;
            using (var principalPropertyNamesEnumerator = principalPropertyNames.GetEnumerator())
            {
                using var principalPropertyTypesEnumerator = principalPropertyTypes.GetEnumerator();
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
                    var clrType = keyPropertyType.MakeNullable(!isRequired);
                    var index = -1;
                    while (true)
                    {
                        propertyName = keyModifiedBaseName + (++index > 0 ? index.ToString(CultureInfo.InvariantCulture) : "");
                        if (!Metadata.FindPropertiesInHierarchy(propertyName).Any()
                            && !clrProperties.ContainsKey(propertyName)
                            && !clrFields.ContainsKey(propertyName)
                            && !IsIgnored(propertyName, ConfigurationSource.Convention))
                        {
                            if (currentProperties == null)
                            {
                                var propertyBuilder = Property(
                                    clrType, propertyName, typeConfigurationSource: null,
                                    configurationSource: ConfigurationSource.Convention)!;

                                if (clrType.IsNullableType())
                                {
                                    propertyBuilder.IsRequired(isRequired, ConfigurationSource.Convention);
                                }

                                newProperties![i] = propertyBuilder.Metadata;
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
                            if (((IConventionProperty)currentProperty).IsImplicitlyCreated()
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

            return (canReuniquify, newProperties);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<Property>? GetOrCreateProperties(
            IReadOnlyList<string>? propertyNames,
            ConfigurationSource? configurationSource,
            IReadOnlyList<Property>? referencedProperties = null,
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
                    var type = referencedProperties == null
                        ? useDefaultType
                            ? typeof(int)
                            : null
                        : referencedProperties[i].ClrType;

                    if (!configurationSource.HasValue)
                    {
                        return null;
                    }

                    var propertyBuilder = Property(
                        required
                            ? type
                            : type?.MakeNullable(),
                        propertyName,
                        typeConfigurationSource: null,
                        configurationSource.Value);

                    if (propertyBuilder == null)
                    {
                        return null;
                    }

                    property = propertyBuilder.Metadata;
                }
                else if (configurationSource.HasValue)
                {
                    if (ConfigurationSource.Convention.Overrides(property.GetTypeConfigurationSource())
                        && (property.IsShadowProperty() || property.IsIndexerProperty())
                        && (!property.IsNullable || (required && property.GetIsNullableConfigurationSource() == null))
                        && property.ClrType.IsNullableType())
                    {
                        property = property.DeclaringEntityType.Builder.Property(
                                property.ClrType.MakeNullable(false),
                                property.Name,
                                configurationSource.Value)!
                            .Metadata;
                    }
                    else
                    {
                        property = property.DeclaringEntityType.Builder.Property(property.Name, configurationSource.Value)!.Metadata;
                    }
                }

                propertyList.Add(property);
            }

            return propertyList;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<Property>? GetOrCreateProperties(
            IEnumerable<MemberInfo>? clrMembers,
            ConfigurationSource? configurationSource)
        {
            if (clrMembers == null)
            {
                return null;
            }

            var list = new List<Property>();
            foreach (var propertyInfo in clrMembers)
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<Property>? GetActualProperties(
            IReadOnlyList<Property>? properties,
            ConfigurationSource? configurationSource)
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
                var builder = property.IsInModel && property.DeclaringEntityType.IsAssignableFrom(Metadata)
                    ? property.Builder
                    : Property(
                        typeConfigurationSource.Overrides(ConfigurationSource.DataAnnotation) ? property.ClrType : null,
                        property.Name,
                        property.GetIdentifyingMemberInfo(),
                        typeConfigurationSource.Overrides(ConfigurationSource.DataAnnotation) ? typeConfigurationSource : null,
                        configurationSource);
                if (builder == null)
                {
                    return null;
                }

                actualProperties[i] = builder.Metadata;
            }

            return actualProperties;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder? HasChangeTrackingStrategy(
            ChangeTrackingStrategy? changeTrackingStrategy,
            ConfigurationSource configurationSource)
        {
            if (CanSetChangeTrackingStrategy(changeTrackingStrategy, configurationSource))
            {
                Metadata.SetChangeTrackingStrategy(changeTrackingStrategy, configurationSource);

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
        public virtual bool CanSetChangeTrackingStrategy(
            ChangeTrackingStrategy? changeTrackingStrategy,
            ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetChangeTrackingStrategyConfigurationSource())
                || Metadata.GetChangeTrackingStrategy() == changeTrackingStrategy;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder? UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode,
            ConfigurationSource configurationSource)
        {
            if (CanSetPropertyAccessMode(propertyAccessMode, configurationSource))
            {
                Metadata.SetPropertyAccessMode(propertyAccessMode, configurationSource);

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
        public virtual bool CanSetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, ConfigurationSource configurationSource)
            => configurationSource.Overrides(((IConventionEntityType)Metadata).GetPropertyAccessModeConfigurationSource())
                || ((IConventionEntityType)Metadata).GetPropertyAccessMode() == propertyAccessMode;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder? HasData(IEnumerable<object> data, ConfigurationSource configurationSource)
        {
            Metadata.AddData(data);

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionEntityTypeBuilder? HasConstructorBinding(
            InstantiationBinding? constructorBinding, ConfigurationSource configurationSource)
        {
            if (CanSetConstructorBinding(constructorBinding, configurationSource))
            {
                Metadata.SetConstructorBinding(constructorBinding, configurationSource);

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
        public virtual bool CanSetConstructorBinding(InstantiationBinding? constructorBinding, ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetConstructorBindingConfigurationSource())
                || Metadata.ConstructorBinding == constructorBinding;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionEntityTypeBuilder? HasServiceOnlyConstructorBinding(
            InstantiationBinding? constructorBinding, ConfigurationSource configurationSource)
        {
            if (CanSetServiceOnlyConstructorBinding(constructorBinding, configurationSource))
            {
                Metadata.SetServiceOnlyConstructorBinding(constructorBinding, configurationSource);

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
        public virtual bool CanSetServiceOnlyConstructorBinding(InstantiationBinding? constructorBinding, ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetServiceOnlyConstructorBindingConfigurationSource())
                || Metadata.ServiceOnlyConstructorBinding == constructorBinding;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DiscriminatorBuilder? HasDiscriminator(ConfigurationSource configurationSource)
            => DiscriminatorBuilder(
                GetOrCreateDiscriminatorProperty(type: null, name: null, ConfigurationSource.Convention),
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DiscriminatorBuilder? HasDiscriminator(
            string? name,
            Type? type,
            ConfigurationSource configurationSource)
        {
            Check.DebugAssert(name != null || type != null, $"Either {nameof(name)} or {nameof(type)} should be non-null");

            return CanSetDiscriminator(name, type, configurationSource)
                ? DiscriminatorBuilder(
                    GetOrCreateDiscriminatorProperty(type, name, configurationSource),
                    configurationSource)
                : null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DiscriminatorBuilder? HasDiscriminator(MemberInfo memberInfo, ConfigurationSource configurationSource)
            => CanSetDiscriminator(
                Check.NotNull(memberInfo, nameof(memberInfo)).GetSimpleMemberName(), memberInfo.GetMemberType(), configurationSource)
                ? DiscriminatorBuilder(
                    Metadata.RootType().Builder.Property(
                        memberInfo, configurationSource),
                    configurationSource)
                : null;

        private static readonly string _defaultDiscriminatorName = "Discriminator";

        private static readonly Type _defaultDiscriminatorType = typeof(string);

        private InternalPropertyBuilder? GetOrCreateDiscriminatorProperty(Type? type, string? name, ConfigurationSource configurationSource)
        {
            var discriminatorProperty = ((IReadOnlyEntityType)Metadata).FindDiscriminatorProperty();
            if ((name != null && discriminatorProperty?.Name != name)
                || (type != null && discriminatorProperty?.ClrType != type))
            {
                discriminatorProperty = null;
            }

            return Metadata.RootType().Builder.Property(
                    type ?? discriminatorProperty?.ClrType ?? _defaultDiscriminatorType,
                    name ?? discriminatorProperty?.Name ?? _defaultDiscriminatorName,
                    typeConfigurationSource: type != null ? configurationSource : (ConfigurationSource?)null,
                    configurationSource)
                ?.AfterSave(PropertySaveBehavior.Throw, ConfigurationSource.Convention);
        }

        private DiscriminatorBuilder? DiscriminatorBuilder(
            InternalPropertyBuilder? discriminatorPropertyBuilder,
            ConfigurationSource configurationSource)
        {
            if (discriminatorPropertyBuilder == null)
            {
                return null;
            }

            var rootTypeBuilder = Metadata.RootType().Builder;
            var discriminatorProperty = discriminatorPropertyBuilder.Metadata;
            // Make sure the property is on the root type
            discriminatorPropertyBuilder = rootTypeBuilder.Property(
                discriminatorProperty.ClrType, discriminatorProperty.Name, null, ConfigurationSource.Convention)!;

            RemoveUnusedDiscriminatorProperty(discriminatorProperty, configurationSource);

            rootTypeBuilder.Metadata.SetDiscriminatorProperty(discriminatorProperty, configurationSource);
            discriminatorPropertyBuilder.IsRequired(true, ConfigurationSource.Convention);
            discriminatorPropertyBuilder.HasValueGeneratorFactory(typeof(DiscriminatorValueGeneratorFactory), ConfigurationSource.Convention);

            return new DiscriminatorBuilder(Metadata);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder? HasNoDiscriminator(ConfigurationSource configurationSource)
        {
            if (Metadata[CoreAnnotationNames.DiscriminatorProperty] != null
                && !configurationSource.Overrides(Metadata.GetDiscriminatorPropertyConfigurationSource()))
            {
                return null;
            }

            if (Metadata.BaseType == null)
            {
                RemoveUnusedDiscriminatorProperty(null, configurationSource);
            }

            Metadata.SetDiscriminatorProperty(null, configurationSource);

            if (configurationSource == ConfigurationSource.Explicit)
            {
                ((IMutableEntityType)Metadata).SetDiscriminatorMappingComplete(null);
            }
            else if (CanSetAnnotation(CoreAnnotationNames.DiscriminatorMappingComplete, null, configurationSource))
            {
                ((IConventionEntityType)Metadata).SetDiscriminatorMappingComplete(null,
                    configurationSource == ConfigurationSource.DataAnnotation);
            }

            return this;
        }

        private void RemoveUnusedDiscriminatorProperty(Property? newDiscriminatorProperty, ConfigurationSource configurationSource)
        {
            var oldDiscriminatorProperty = ((IReadOnlyEntityType)Metadata).FindDiscriminatorProperty() as Property;
            if (oldDiscriminatorProperty?.IsInModel == true
                && oldDiscriminatorProperty != newDiscriminatorProperty)
            {
                oldDiscriminatorProperty.DeclaringEntityType.Builder.RemoveUnusedImplicitProperties(
                    new[] { oldDiscriminatorProperty });

                if (oldDiscriminatorProperty.IsInModel)
                {
                    oldDiscriminatorProperty.Builder.IsRequired(null, configurationSource);
                    oldDiscriminatorProperty.Builder.HasValueGenerator((Type?)null, configurationSource);
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetDiscriminator(string? name, Type? type, ConfigurationSource configurationSource)
            => name == null && type == null
                ? CanRemoveDiscriminator(configurationSource)
                : CanSetDiscriminator(((IReadOnlyEntityType)Metadata).FindDiscriminatorProperty(), name, type, configurationSource);

        private bool CanSetDiscriminator(
            IReadOnlyProperty? discriminatorProperty,
            string? name,
            Type? discriminatorType,
            ConfigurationSource configurationSource)
            => ((name == null && discriminatorType == null)
                    || ((name == null || discriminatorProperty?.Name == name)
                        && (discriminatorType == null || discriminatorProperty?.ClrType == discriminatorType))
                    || configurationSource.Overrides(Metadata.GetDiscriminatorPropertyConfigurationSource()))
                && (discriminatorProperty != null
                    || Metadata.RootType().Builder.CanAddDiscriminatorProperty(
                        discriminatorType ?? _defaultDiscriminatorType,
                        name ?? _defaultDiscriminatorName,
                        typeConfigurationSource: discriminatorType != null
                            ? configurationSource
                            : (ConfigurationSource?)null));

        private bool CanRemoveDiscriminator(ConfigurationSource configurationSource)
            => CanSetAnnotation(CoreAnnotationNames.DiscriminatorProperty, null, configurationSource);

        private bool CanAddDiscriminatorProperty(
            Type propertyType,
            string name,
            ConfigurationSource? typeConfigurationSource)
        {
            var conflictingProperty = Metadata.FindPropertiesInHierarchy(name).FirstOrDefault();
            if (conflictingProperty != null
                && (conflictingProperty.IsShadowProperty() || conflictingProperty.IsIndexerProperty())
                && conflictingProperty.ClrType != propertyType
                && typeConfigurationSource != null
                && !typeConfigurationSource.Overrides(conflictingProperty.GetTypeConfigurationSource()))
            {
                return false;
            }

            var memberInfo = Metadata.ClrType.GetMembersInHierarchy(name).FirstOrDefault();
            if (memberInfo != null
                && propertyType != memberInfo.GetMemberType()
                && typeConfigurationSource != null)
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
        IConventionEntityType IConventionEntityTypeBuilder.Metadata
        {
            [DebuggerStepThrough]
            get => Metadata;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasBaseType(IConventionEntityType? baseEntityType, bool fromDataAnnotation)
            => HasBaseType(
                (EntityType?)baseEntityType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionEntityTypeBuilder.CanSetBaseType(IConventionEntityType? baseEntityType, bool fromDataAnnotation)
            => CanSetBaseType(
                (EntityType?)baseEntityType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionPropertyBuilder? IConventionEntityTypeBuilder.Property(
            Type propertyType,
            string propertyName,
            bool setTypeConfigurationSource,
            bool fromDataAnnotation)
            => Property(
                propertyType,
                propertyName, setTypeConfigurationSource
                    ? fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention
                    : (ConfigurationSource?)null, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionPropertyBuilder? IConventionEntityTypeBuilder.Property(MemberInfo memberInfo, bool fromDataAnnotation)
            => Property(memberInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IReadOnlyList<IConventionProperty>? IConventionEntityTypeBuilder.GetOrCreateProperties(
            IReadOnlyList<string>? propertyNames,
            bool fromDataAnnotation)
            => GetOrCreateProperties(
                propertyNames, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IReadOnlyList<IConventionProperty>? IConventionEntityTypeBuilder.GetOrCreateProperties(
            IEnumerable<MemberInfo>? memberInfos,
            bool fromDataAnnotation)
            => GetOrCreateProperties(memberInfos, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityTypeBuilder IConventionEntityTypeBuilder.RemoveUnusedImplicitProperties(
            IReadOnlyList<IConventionProperty> properties)
            => RemoveUnusedImplicitProperties(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionServicePropertyBuilder? IConventionEntityTypeBuilder.ServiceProperty(MemberInfo memberInfo, bool fromDataAnnotation)
            => ServiceProperty(memberInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionEntityTypeBuilder.IsIgnored(string name, bool fromDataAnnotation)
            => IsIgnored(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.Ignore(string name, bool fromDataAnnotation)
            => Ignore(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionEntityTypeBuilder.CanIgnore(string name, bool fromDataAnnotation)
            => CanIgnore(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionKeyBuilder? IConventionEntityTypeBuilder.PrimaryKey(
            IReadOnlyList<IConventionProperty>? properties,
            bool fromDataAnnotation)
            => PrimaryKey(
                properties as IReadOnlyList<Property> ?? properties?.Cast<Property>().ToList(),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionEntityTypeBuilder.CanSetPrimaryKey(IReadOnlyList<IConventionProperty>? properties, bool fromDataAnnotation)
            => CanSetPrimaryKey(
                properties as IReadOnlyList<Property> ?? properties?.Cast<Property>().ToList(),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionKeyBuilder? IConventionEntityTypeBuilder.HasKey(IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation)
            => HasKey(
                properties as IReadOnlyList<Property> ?? properties.Cast<Property>().ToList(),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoKey(bool fromDataAnnotation)
            => HasNoKey(fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoKey(
            IReadOnlyList<IConventionProperty> properties,
            bool fromDataAnnotation)
        {
            Check.NotEmpty(properties, nameof(properties));

            var key = Metadata.FindDeclaredKey(properties);
            return key != null
                ? HasNoKey(key, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                : this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionEntityTypeBuilder.CanRemoveKey(IConventionKey key, bool fromDataAnnotation)
            => CanRemoveKey((Key)key, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoKey(IConventionKey key, bool fromDataAnnotation)
            => HasNoKey((Key)key, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionEntityTypeBuilder.CanRemoveKey(bool fromDataAnnotation)
            => CanRemoveKey(fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionIndexBuilder? IConventionEntityTypeBuilder.HasIndex(
            IReadOnlyList<string> propertyNames,
            bool fromDataAnnotation)
            => HasIndex(
                propertyNames,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionIndexBuilder? IConventionEntityTypeBuilder.HasIndex(
            IReadOnlyList<string> propertyNames,
            string name,
            bool fromDataAnnotation)
            => HasIndex(
                propertyNames,
                name,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionIndexBuilder? IConventionEntityTypeBuilder.HasIndex(
            IReadOnlyList<IConventionProperty> properties,
            bool fromDataAnnotation)
            => HasIndex(
                properties as IReadOnlyList<Property> ?? properties.Cast<Property>().ToList(),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionIndexBuilder? IConventionEntityTypeBuilder.HasIndex(
            IReadOnlyList<IConventionProperty> properties,
            string name,
            bool fromDataAnnotation)
            => HasIndex(
                properties as IReadOnlyList<Property> ?? properties.Cast<Property>().ToList(),
                name,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoIndex(
            IReadOnlyList<IConventionProperty> properties,
            bool fromDataAnnotation)
        {
            Check.NotEmpty(properties, nameof(properties));

            var index = Metadata.FindDeclaredIndex(properties);
            return index != null
                ? HasNoIndex(index, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                : this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoIndex(IConventionIndex index, bool fromDataAnnotation)
            => HasNoIndex((Index)index, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionEntityTypeBuilder.CanRemoveIndex(IConventionIndex index, bool fromDataAnnotation)
            => CanRemoveIndex((Index)index, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasRelationship(
            IConventionEntityType targetEntityType,
            bool fromDataAnnotation)
            => HasRelationship(
                (EntityType)targetEntityType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasRelationship(
            IConventionEntityType principalEntityType,
            IReadOnlyList<IConventionProperty> dependentProperties,
            bool fromDataAnnotation)
            => HasRelationship(
                (EntityType)principalEntityType,
                dependentProperties as IReadOnlyList<Property> ?? dependentProperties.Cast<Property>().ToList(),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasRelationship(
            IConventionEntityType principalEntityType,
            IConventionKey principalKey,
            bool fromDataAnnotation)
            => HasRelationship(
                (EntityType)principalEntityType,
                (Key)principalKey,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasRelationship(
            IConventionEntityType principalEntityType,
            IReadOnlyList<IConventionProperty> dependentProperties,
            IConventionKey principalKey,
            bool fromDataAnnotation)
            => HasRelationship(
                (EntityType)principalEntityType,
                dependentProperties as IReadOnlyList<Property> ?? dependentProperties.Cast<Property>().ToList(),
                (Key)principalKey,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasRelationship(
            IConventionEntityType targetEntityType,
            string navigationName,
            bool setTargetAsPrincipal,
            bool fromDataAnnotation)
            => HasRelationship(
                (EntityType)targetEntityType,
                navigationName,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
                setTargetAsPrincipal ? true : (bool?)null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasRelationship(
            IConventionEntityType targetEntityType,
            MemberInfo navigation,
            bool setTargetAsPrincipal,
            bool fromDataAnnotation)
            => HasRelationship(
                (EntityType)targetEntityType,
                navigation,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
                setTargetAsPrincipal ? true : (bool?)null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasRelationship(
            IConventionEntityType targetEntityType,
            string navigationName,
            string? inverseNavigationName,
            bool setTargetAsPrincipal,
            bool fromDataAnnotation)
            => HasRelationship(
                (EntityType)targetEntityType,
                navigationName, inverseNavigationName,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
                setTargetAsPrincipal);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasRelationship(
            IConventionEntityType targetEntityType,
            MemberInfo navigation,
            MemberInfo? inverseNavigation,
            bool setTargetAsPrincipal,
            bool fromDataAnnotation)
            => HasRelationship(
                (EntityType)targetEntityType,
                navigation, inverseNavigation,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
                setTargetAsPrincipal);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionSkipNavigationBuilder? IConventionEntityTypeBuilder.HasSkipNavigation(
            MemberInfo navigation,
            IConventionEntityType targetEntityType,
            MemberInfo inverseNavigation,
            bool? collections,
            bool? onDependent,
            bool fromDataAnnotation)
            => HasSkipNavigation(
                MemberIdentity.Create(navigation),
                (EntityType)targetEntityType,
                MemberIdentity.Create(inverseNavigation),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
                collections,
                onDependent);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasOwnership(
            Type targetEntityType,
            string navigationName,
            bool fromDataAnnotation)
            => HasOwnership(
                targetEntityType, navigationName,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasOwnership(
            Type targetEntityType,
            MemberInfo navigation,
            bool fromDataAnnotation)
            => HasOwnership(
                targetEntityType, navigation,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasOwnership(
            Type targetEntityType,
            string navigationName,
            string? inversePropertyName,
            bool fromDataAnnotation)
            => HasOwnership(
                targetEntityType, navigationName, inversePropertyName,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionForeignKeyBuilder? IConventionEntityTypeBuilder.HasOwnership(
            Type targetEntityType,
            MemberInfo navigation,
            MemberInfo? inverseProperty,
            bool fromDataAnnotation)
            => HasOwnership(
                targetEntityType, navigation, inverseProperty,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoRelationship(
            IReadOnlyList<IConventionProperty> properties,
            IConventionKey principalKey,
            IConventionEntityType principalEntityType,
            bool fromDataAnnotation)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.NotNull(principalKey, nameof(principalKey));
            Check.NotNull(principalEntityType, nameof(principalEntityType));

            var foreignKey = Metadata.FindDeclaredForeignKey(properties, principalKey, principalEntityType);
            return foreignKey != null
                ? HasNoRelationship(foreignKey, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                : this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoRelationship(
            IConventionForeignKey foreignKey,
            bool fromDataAnnotation)
            => HasNoRelationship(
                (ForeignKey)foreignKey,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionEntityTypeBuilder.CanRemoveRelationship(IConventionForeignKey foreignKey, bool fromDataAnnotation)
            => CanRemoveForeignKey(
                (ForeignKey)foreignKey,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionEntityTypeBuilder.CanHaveNavigation(string navigationName, bool fromDataAnnotation)
            => CanHaveNavigation(
                navigationName,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionEntityTypeBuilder.CanHaveSkipNavigation(string skipNavigationName, bool fromDataAnnotation)
            => CanHaveSkipNavigation(
                skipNavigationName,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionSkipNavigationBuilder? IConventionEntityTypeBuilder.HasSkipNavigation(
            MemberInfo navigation,
            IConventionEntityType targetEntityType,
            bool? collection,
            bool? onDependent,
            bool fromDataAnnotation)
            => HasSkipNavigation(
                MemberIdentity.Create(navigation),
                (EntityType)targetEntityType,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
                collection,
                onDependent);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionSkipNavigationBuilder? IConventionEntityTypeBuilder.HasSkipNavigation(
            string navigationName,
            IConventionEntityType targetEntityType,
            bool? collection,
            bool? onDependent,
            bool fromDataAnnotation)
            => HasSkipNavigation(
                MemberIdentity.Create(navigationName),
                (EntityType)targetEntityType,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
                collection,
                onDependent);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoSkipNavigation(
            IReadOnlySkipNavigation skipNavigation,
            bool fromDataAnnotation)
            => HasNoSkipNavigation(
                (SkipNavigation)skipNavigation,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionEntityTypeBuilder.CanRemoveSkipNavigation(IReadOnlySkipNavigation skipNavigation, bool fromDataAnnotation)
            => CanRemoveSkipNavigation(
                (SkipNavigation)skipNavigation,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasQueryFilter(LambdaExpression? filter, bool fromDataAnnotation)
            => HasQueryFilter(filter, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionEntityTypeBuilder.CanSetQueryFilter(LambdaExpression? filter, bool fromDataAnnotation)
            => CanSetQueryFilter(filter, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        [Obsolete]
        IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasDefiningQuery(LambdaExpression? query, bool fromDataAnnotation)
            => HasDefiningQuery(query, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        [Obsolete]
        bool IConventionEntityTypeBuilder.CanSetDefiningQuery(LambdaExpression? query, bool fromDataAnnotation)
            => CanSetDefiningQuery(query, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasChangeTrackingStrategy(
            ChangeTrackingStrategy? changeTrackingStrategy,
            bool fromDataAnnotation)
            => HasChangeTrackingStrategy(
                changeTrackingStrategy, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionEntityTypeBuilder.CanSetChangeTrackingStrategy(
            ChangeTrackingStrategy? changeTrackingStrategy,
            bool fromDataAnnotation)
            => CanSetChangeTrackingStrategy(
                changeTrackingStrategy, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode,
            bool fromDataAnnotation)
            => UsePropertyAccessMode(
                propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionEntityTypeBuilder.CanSetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation)
            => CanSetPropertyAccessMode(
                propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionDiscriminatorBuilder? IConventionEntityTypeBuilder.HasDiscriminator(bool fromDataAnnotation)
            => HasDiscriminator(
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionDiscriminatorBuilder? IConventionEntityTypeBuilder.HasDiscriminator(Type type, bool fromDataAnnotation)
            => HasDiscriminator(
                name: null, Check.NotNull(type, nameof(type)),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionDiscriminatorBuilder? IConventionEntityTypeBuilder.HasDiscriminator(string name, bool fromDataAnnotation)
            => HasDiscriminator(
                Check.NotEmpty(name, nameof(name)), type: null,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionDiscriminatorBuilder? IConventionEntityTypeBuilder.HasDiscriminator(string name, Type type, bool fromDataAnnotation)
            => HasDiscriminator(
                Check.NotEmpty(name, nameof(name)), Check.NotNull(type, nameof(type)),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionDiscriminatorBuilder? IConventionEntityTypeBuilder.HasDiscriminator(MemberInfo memberInfo, bool fromDataAnnotation)
            => HasDiscriminator(
                memberInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionEntityTypeBuilder? IConventionEntityTypeBuilder.HasNoDiscriminator(bool fromDataAnnotation)
            => HasNoDiscriminator(fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionEntityTypeBuilder.CanSetDiscriminator(string name, bool fromDataAnnotation)
            => CanSetDiscriminator(
                name, type: null,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionEntityTypeBuilder.CanSetDiscriminator(Type type, bool fromDataAnnotation)
            => CanSetDiscriminator(
                name: null, type,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionEntityTypeBuilder.CanSetDiscriminator(string name, Type type, bool fromDataAnnotation)
            => CanSetDiscriminator(
                name, type,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionEntityTypeBuilder.CanSetDiscriminator(MemberInfo memberInfo, bool fromDataAnnotation)
            => CanSetDiscriminator(
                Check.NotNull(memberInfo, nameof(memberInfo)).GetSimpleMemberName(), memberInfo.GetMemberType(),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool IConventionEntityTypeBuilder.CanRemoveDiscriminator(bool fromDataAnnotation)
            => CanRemoveDiscriminator(fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionPropertyBuilder? IConventionEntityTypeBuilder.CreateUniqueProperty(
            Type propertyType,
            string basePropertyName,
            bool required)
            => CreateUniqueProperty(propertyType, basePropertyName, required);
    }
}
