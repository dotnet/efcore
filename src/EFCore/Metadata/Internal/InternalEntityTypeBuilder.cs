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
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class InternalEntityTypeBuilder : InternalModelItemBuilder<EntityType>, IConventionEntityTypeBuilder
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalEntityTypeBuilder([NotNull] EntityType metadata, [NotNull] InternalModelBuilder modelBuilder)
            : base(metadata, modelBuilder)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalKeyBuilder PrimaryKey(
            [CanBeNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => PrimaryKey(GetOrCreateProperties(propertyNames, configurationSource, required: true), configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalKeyBuilder PrimaryKey(
            [CanBeNull] IReadOnlyList<MemberInfo> clrMembers, ConfigurationSource configurationSource)
            => PrimaryKey(GetOrCreateProperties(clrMembers, configurationSource), configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalKeyBuilder PrimaryKey(
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
        {
            if (!CanSetPrimaryKey(properties, configurationSource))
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
                var previousPrimaryKey = Metadata.FindPrimaryKey();
                if (previousPrimaryKey != null
                    && PropertyListComparer.Instance.Compare(previousPrimaryKey.Properties, properties) == 0)
                {
                    previousPrimaryKey.UpdateConfigurationSource(configurationSource);
                    return Metadata.SetPrimaryKey(properties, configurationSource).Builder;
                }

                using (ModelBuilder.Metadata.ConventionDispatcher.DelayConventions())
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

                    if (previousPrimaryKey?.Builder != null)
                    {
                        RemoveKeyIfUnused(previousPrimaryKey, configurationSource);
                    }
                }
            }

            // TODO: Use convention batch to get the updated builder, see #214
            if (keyBuilder?.Metadata.Builder == null)
            {
                properties = GetActualProperties(properties, null);
                return properties == null ? null : Metadata.FindPrimaryKey(properties).Builder;
            }

            return keyBuilder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetPrimaryKey(IReadOnlyList<IConventionProperty> properties, ConfigurationSource configurationSource)
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

            return configurationSource.Overrides(Metadata.GetPrimaryKeyConfigurationSource());
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalKeyBuilder HasKey([NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => HasKeyInternal(GetOrCreateProperties(propertyNames, configurationSource, required: true), configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalKeyBuilder HasKey([NotNull] IReadOnlyList<MemberInfo> clrProperties, ConfigurationSource configurationSource)
            => HasKeyInternal(GetOrCreateProperties(clrProperties, configurationSource), configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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

                if (Metadata.IsKeyless
                    && !configurationSource.Overrides(Metadata.GetIsKeylessConfigurationSource()))
                {
                    return null;
                }

                if (Metadata.GetIsKeylessConfigurationSource() != ConfigurationSource.Explicit)
                {
                    Metadata.HasNoKey(false, configurationSource.Value);
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

                using (Metadata.Model.ConventionDispatcher.DelayConventions())
                {
                    foreach (var foreignKey in containingForeignKeys)
                    {
                        if (foreignKey.GetPropertiesConfigurationSource() == ConfigurationSource.Explicit)
                        {
                            // let it throw for explicit
                            continue;
                        }

                        foreignKey.Builder.HasForeignKey((IReadOnlyList<Property>)null, configurationSource.Value);
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
                Metadata.HasNoKey(false, configurationSource.Value);
            }

            return key?.Builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder HasNoKey([NotNull] Key key, ConfigurationSource configurationSource)
        {
            var currentConfigurationSource = key.GetConfigurationSource();
            if (!configurationSource.Overrides(currentConfigurationSource))
            {
                return null;
            }

            using (Metadata.Model.ConventionDispatcher.DelayConventions())
            {
                var detachedRelationships = key.GetReferencingForeignKeys().ToList().Select(DetachRelationship).ToList();

                Metadata.RemoveKey(key);

                foreach (var detachedRelationship in detachedRelationships)
                {
                    detachedRelationship.Attach();
                }

                RemoveUnusedShadowProperties(key.Properties);
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

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        public virtual InternalEntityTypeBuilder HasNoKey(ConfigurationSource configurationSource)
        {
            if (Metadata.IsKeyless)
            {
                Metadata.HasNoKey(true, configurationSource);
                return this;
            }

            if (!configurationSource.Overrides(Metadata.GetIsKeylessConfigurationSource())
                || Metadata.GetReferencingForeignKeys().Any(fk => !configurationSource.Overrides(fk.GetConfigurationSource()))
                || Metadata.GetForeignKeys().Any(fk => !configurationSource.Overrides(fk.GetPrincipalToDependentConfigurationSource())))
            {
                return null;
            }

            using (Metadata.Model.ConventionDispatcher.DelayConventions())
            {
                foreach (var foreignKey in Metadata.GetReferencingForeignKeys().ToList())
                {
                    foreignKey.DeclaringEntityType.Builder.HasNoRelationship(foreignKey, configurationSource);
                }

                foreach (var foreignKey in Metadata.GetForeignKeys())
                {
                    foreignKey.HasPrincipalToDependent((string)null, configurationSource);
                }

                foreach (var key in Metadata.GetKeys().ToList())
                {
                    if (key.GetConfigurationSource() != ConfigurationSource.Explicit)
                    {
                        HasNoKey(key, configurationSource);
                    }
                }

                Metadata.HasNoKey(true, configurationSource);
                return this;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder Property(
            [CanBeNull] Type propertyType,
            [NotNull] string propertyName,
            ConfigurationSource? configurationSource)
            => Property(propertyType, propertyName, typeConfigurationSource: configurationSource, configurationSource: configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder Property(
            [CanBeNull] Type propertyType,
            [NotNull] string propertyName,
            ConfigurationSource? typeConfigurationSource,
            ConfigurationSource? configurationSource)
            => Property(
                propertyType, propertyName, memberInfo: null,
                typeConfigurationSource: typeConfigurationSource,
                configurationSource: configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder Property([NotNull] string propertyName, ConfigurationSource? configurationSource)
            => Property(
                propertyType: null, propertyName: propertyName, memberInfo: null, typeConfigurationSource: null,
                configurationSource: configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder Property([NotNull] MemberInfo memberInfo, ConfigurationSource? configurationSource)
            => Property(memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName(), memberInfo, configurationSource, configurationSource);

        private InternalPropertyBuilder Property(
            [CanBeNull] Type propertyType,
            [NotNull] string propertyName,
            [CanBeNull] MemberInfo memberInfo,
            ConfigurationSource? typeConfigurationSource,
            ConfigurationSource? configurationSource)
        {
            List<Property> propertiesToDetach = null;
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
                            propertiesToDetach = new List<Property> { existingProperty };
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
                            Metadata.RemoveIgnored(propertyName);
                        }

                        return existingProperty.DeclaringEntityType.Builder
                            .Property(
                                existingProperty, propertyName, propertyType, memberInfo, typeConfigurationSource, configurationSource);
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

                if (propertyType == null)
                {
                    var clrMember = Metadata.ClrType?.GetMembersInHierarchy(propertyName).FirstOrDefault();
                    if (clrMember == null)
                    {
                        throw new InvalidOperationException(CoreStrings.NoPropertyType(propertyName, Metadata.DisplayName()));
                    }

                    propertyType = clrMember.GetMemberType();
                    typeConfigurationSource = ConfigurationSource.Explicit;
                }

                Metadata.RemoveIgnored(propertyName);

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

            InternalPropertyBuilder builder;
            using (Metadata.Model.ConventionDispatcher.DelayConventions())
            {
                var detachedProperties = propertiesToDetach == null ? null : DetachProperties(propertiesToDetach);

                builder = Property(
                    existingProperty, propertyName, propertyType, memberInfo, typeConfigurationSource, configurationSource);

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
            ConfigurationSource? typeConfigurationSource,
            ConfigurationSource? configurationSource)
        {
            Property property;
            if (existingProperty == null)
            {
                if (!configurationSource.HasValue)
                {
                    return null;
                }

                using (ModelBuilder.Metadata.ConventionDispatcher.DelayConventions())
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
                            foreignKey.DeclaringEntityType.Builder.HasNoRelationship(foreignKey, ConfigurationSource.Convention);
                        }
                        else if (foreignKey.Builder.HasNavigation(
                                     (string)null,
                                     conflictingNavigation.IsDependentToPrincipal(),
                                     configurationSource.Value) == null)
                        {
                            return null;
                        }
                    }

                    property = clrProperty != null
                        ? Metadata.AddProperty(clrProperty, configurationSource.Value)
                        : Metadata.AddProperty(propertyName, propertyType, typeConfigurationSource, configurationSource.Value);
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

                    using (Metadata.Model.ConventionDispatcher.DelayConventions())
                    {
                        var detachedProperties = DetachProperties(
                            new[] { existingProperty });

                        property = clrProperty != null
                            ? Metadata.AddProperty(clrProperty, configurationSource.Value)
                            : Metadata.AddProperty(propertyName, propertyType, typeConfigurationSource, configurationSource.Value);

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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanAddProperty(Type propertyType, string name,ConfigurationSource? typeConfigurationSource)
        {
            var conflictingMember = Metadata.FindPropertiesInHierarchy(name).FirstOrDefault();
            if (conflictingMember != null
                && conflictingMember.IsShadowProperty()
                && conflictingMember.ClrType != propertyType
                && typeConfigurationSource != null
                && !typeConfigurationSource.Overrides(conflictingMember.GetTypeConfigurationSource()))
            {
                return false;
            }

            if (Metadata.ClrType == null)
            {
                return true;
            }

            var memberInfo = Metadata.ClrType.GetMembersInHierarchy(name).FirstOrDefault();
            if (memberInfo != null
                && propertyType != memberInfo.GetMemberType()
                && (memberInfo as PropertyInfo)?.IsEFIndexerProperty() != true
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
        public virtual InternalServicePropertyBuilder ServiceProperty(
            [NotNull] MemberInfo memberInfo, ConfigurationSource configurationSource)
        {
            var propertyName = memberInfo.GetSimpleMemberName();
            if (IsIgnored(propertyName, configurationSource))
            {
                return null;
            }

            Metadata.RemoveIgnored(propertyName);

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
                    if (existingProperty.GetIdentifyingMemberInfo() == memberInfo)
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

            using (Metadata.Model.ConventionDispatcher.DelayConventions())
            {
                using (ModelBuilder.Metadata.ConventionDispatcher.DelayConventions())
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
                                foreignKey.DeclaringEntityType.Builder.HasNoRelationship(foreignKey, ConfigurationSource.Convention);
                            }
                            else if (foreignKey.Builder.HasNavigation(
                                         (string)null,
                                         conflictingNavigation.IsDependentToPrincipal(),
                                         configurationSource) == null)
                            {
                                return null;
                            }
                        }
                    }

                    return Metadata.AddServiceProperty(memberInfo, configurationSource).Builder;
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanAddNavigation([NotNull] string navigationName, ConfigurationSource configurationSource)
            => !IsIgnored(navigationName, configurationSource)
               && Metadata.FindNavigationsInHierarchy(navigationName).All(
                   n => n.ForeignKey.Builder.CanSetNavigation((string)null, n.IsDependentToPrincipal(), configurationSource));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsIgnored([NotNull] string name, ConfigurationSource? configurationSource)
        {
            Check.NotEmpty(name, nameof(name));

            var ignoredConfigurationSource = Metadata.FindIgnoredConfigurationSource(name);
            return !configurationSource.HasValue
                   || !configurationSource.Value.Overrides(ignoredConfigurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanRemoveForeignKey([NotNull] ForeignKey foreignKey, ConfigurationSource configurationSource)
        {
            Debug.Assert(foreignKey.DeclaringEntityType == Metadata);

            var currentConfigurationSource = foreignKey.GetConfigurationSource();
            return configurationSource.Overrides(currentConfigurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder Ignore([NotNull] string name, ConfigurationSource configurationSource)
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

            using (Metadata.Model.ConventionDispatcher.DelayConventions())
            {
                Metadata.AddIgnored(name, configurationSource);

                var navigation = Metadata.FindNavigation(name);
                if (navigation != null)
                {
                    var foreignKey = navigation.ForeignKey;
                    Debug.Assert(navigation.DeclaringEntityType == Metadata);

                    var isDependent = navigation.IsDependentToPrincipal();
                    var navigationConfigurationSource = isDependent
                        ? foreignKey.GetDependentToPrincipalConfigurationSource()
                        : foreignKey.GetPrincipalToDependentConfigurationSource();
                    if (foreignKey.GetConfigurationSource() != navigationConfigurationSource)
                    {
                        var navigationRemoved = foreignKey.Builder.HasNavigation(
                            (MemberInfo)null, isDependent, configurationSource);
                        Debug.Assert(navigationRemoved != null);
                    }
                    else
                    {
                        var removed = foreignKey.DeclaringEntityType.Builder.HasNoRelationship(
                            foreignKey, configurationSource);
                        Debug.Assert(removed != null);
                    }
                }
                else
                {
                    var property = Metadata.FindProperty(name);
                    if (property != null)
                    {
                        Debug.Assert(property.DeclaringEntityType == Metadata);

                        var removed = property.DeclaringEntityType.Builder.RemoveProperty(
                            property, configurationSource);

                        Debug.Assert(removed != null);
                    }
                    else
                    {
                        var serviceProperty = Metadata.FindServiceProperty(name);
                        if (serviceProperty != null)
                        {
                            Debug.Assert(serviceProperty.DeclaringEntityType == Metadata);

                            serviceProperty.DeclaringEntityType.RemoveServiceProperty(name);
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
                        if (configurationSource != foreignKey.GetConfigurationSource())
                        {
                            foreignKey.DeclaringEntityType.Builder.HasNoRelationship(foreignKey, configurationSource);
                        }
                    }
                    else
                    {
                        var derivedProperty = derivedType.FindDeclaredProperty(name);
                        if (derivedProperty != null)
                        {
                            if (configurationSource != derivedProperty.GetConfigurationSource())
                            {
                                derivedType.Builder.RemoveProperty(derivedProperty, configurationSource);
                            }
                        }
                        else
                        {
                            var derivedServiceProperty = derivedType.FindServiceProperty(name);
                            if (derivedServiceProperty != null
                                && configurationSource.OverridesStrictly(derivedServiceProperty.GetConfigurationSource()))
                            {
                                derivedServiceProperty.DeclaringEntityType.RemoveServiceProperty(name);
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
                var foreignKey = navigation.ForeignKey;
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

                var isDependent = navigation.IsDependentToPrincipal();
                var navigationConfigurationSource = isDependent
                    ? foreignKey.GetDependentToPrincipalConfigurationSource()
                    : foreignKey.GetPrincipalToDependentConfigurationSource();
                if (foreignKey.GetConfigurationSource() != navigationConfigurationSource)
                {
                    if (!configurationSource.Overrides(navigationConfigurationSource))
                    {
                        return false;
                    }
                }
                else if (configurationSource != ConfigurationSource.Explicit
                         && !configurationSource.OverridesStrictly(foreignKey.GetConfigurationSource()))
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
                        property, configurationSource, canOverrideSameSource: configurationSource == ConfigurationSource.Explicit))
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

                        if (configurationSource != ConfigurationSource.Explicit
                            && !configurationSource.OverridesStrictly(serviceProperty.GetConfigurationSource()))
                        {
                            return false;
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
        public virtual InternalEntityTypeBuilder HasQueryFilter(
            [CanBeNull] LambdaExpression filter, ConfigurationSource configurationSource)
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
        public virtual bool CanSetQueryFilter([CanBeNull] LambdaExpression filter, ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetQueryFilterConfigurationSource())
               || Metadata.GetQueryFilter() == filter;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder HasDefiningQuery(
            [CanBeNull] LambdaExpression query, ConfigurationSource configurationSource)
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
        public virtual bool CanSetDefiningQuery(LambdaExpression query, ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetDefiningQueryConfigurationSource())
               || Metadata.GetDefiningQuery() == query;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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

            using (Metadata.Model.ConventionDispatcher.DelayConventions())
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
                    if (Metadata.GetDeclaredKeys().Any(k => !configurationSourceForRemoval.Overrides(k.GetConfigurationSource()))
                        || Metadata.IsKeyless && !configurationSource.Overrides(Metadata.GetIsKeylessConfigurationSource()))
                    {
                        return null;
                    }

                    var relationshipsToBeDetached = FindConflictingRelationships(baseEntityType, configurationSourceForRemoval);
                    if (relationshipsToBeDetached == null)
                    {
                        return null;
                    }

                    var foreignKeysUsingKeyProperties = Metadata.GetDerivedTypesInclusive()
                        .SelectMany(t => t.GetDeclaredForeignKeys())
                        .Where(fk => fk.Properties.Any(p => baseEntityType.FindProperty(p.Name)?.IsKey() == true)).ToList();

                    if (foreignKeysUsingKeyProperties.Any(
                        fk => !configurationSourceForRemoval.Overrides(fk.GetPropertiesConfigurationSource())))
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
                                .Overrides(baseEntityType.FindIgnoredConfigurationSource(p.Name)))
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

                    foreach (var ignoredMember in Metadata.GetIgnoredMembers())
                    {
                        var baseIgnoredSource = baseEntityType.FindIgnoredConfigurationSource(ignoredMember);

                        if (baseIgnoredSource.HasValue
                            && baseIgnoredSource.Value.Overrides(Metadata.FindDeclaredIgnoredConfigurationSource(ignoredMember)))
                        {
                            Metadata.RemoveIgnored(ignoredMember);
                        }
                    }

                    Metadata.HasNoKey(false, configurationSource);
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
                        foreach (var foreignKey in Metadata.GetDerivedTypesInclusive()
                            .SelectMany(t => t.GetDeclaredForeignKeys())
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

                        foreach (var index in Metadata.GetDerivedTypesInclusive()
                            .SelectMany(e => e.GetDeclaredIndexes())
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
                            // ReSharper disable once PossibleInvalidOperationException
                            (ignoredMember, Metadata.BaseType.FindDeclaredIgnoredConfigurationSource(ignoredMember).Value));
                    }
                }

                Metadata.HasBaseType(baseEntityType, configurationSource);

                if (membersToIgnore != null)
                {
                    foreach (var ignoreTuple in membersToIgnore)
                    {
                        Ignore(ignoreTuple.Item1, ignoreTuple.Item2);
                        Metadata.RemoveIgnored(ignoreTuple.Item1);
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetBaseType(EntityType baseEntityType, ConfigurationSource configurationSource)
            => Metadata.BaseType == baseEntityType
               || configurationSource.Overrides(Metadata.GetBaseTypeConfigurationSource());

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
                    baseEntityType.FindIgnoredConfigurationSource(navigation.Name)))
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

            using (Metadata.Model.ConventionDispatcher.DelayConventions())
            {
                var detachedRelationships = property.GetContainingForeignKeys().ToList()
                    .Select(DetachRelationship).ToList();

                foreach (var key in property.GetContainingKeys().ToList())
                {
                    detachedRelationships.AddRange(
                        key.GetReferencingForeignKeys().ToList()
                            .Select(DetachRelationship));
                    var removed = key.DeclaringEntityType.Builder.HasNoKey(key, configurationSource);
                    Debug.Assert(removed != null);
                }

                foreach (var index in property.GetContainingIndexes().ToList())
                {
                    var removed = index.DeclaringEntityType.Builder.HasNoIndex(index, configurationSource);
                    Debug.Assert(removed != null);
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static RelationshipSnapshot DetachRelationship([NotNull] ForeignKey foreignKey)
            => DetachRelationship(foreignKey, false);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static RelationshipSnapshot DetachRelationship([NotNull] ForeignKey foreignKey, bool includeDefinedType)
        {
            var detachedBuilder = foreignKey.Builder;
            var relationshipConfigurationSource = foreignKey.DeclaringEntityType.Builder
                .HasNoRelationship(foreignKey, foreignKey.GetConfigurationSource());
            Debug.Assert(relationshipConfigurationSource != null);

            EntityType.Snapshot definedSnapshot = null;
            if (includeDefinedType)
            {
                var dependentEntityType = foreignKey.DeclaringEntityType;
                if (dependentEntityType.DefiningEntityType == foreignKey.PrincipalEntityType
                    && dependentEntityType.DefiningNavigationName == foreignKey.PrincipalToDependent?.Name)
                {
                    definedSnapshot = DetachAllMembers(dependentEntityType);
                    dependentEntityType.Model.Builder.HasNoEntityType(dependentEntityType, ConfigurationSource.Explicit);
                }
            }

            return new RelationshipSnapshot(detachedBuilder, definedSnapshot);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder HasNoRelationship(
            [NotNull] ForeignKey foreignKey,
            ConfigurationSource configurationSource)
        {
            var currentConfigurationSource = foreignKey.GetConfigurationSource();
            if (!configurationSource.Overrides(currentConfigurationSource))
            {
                return null;
            }

            Metadata.RemoveForeignKey(foreignKey);

            RemoveUnusedShadowProperties(foreignKey.Properties);
            foreignKey.PrincipalKey.DeclaringEntityType.Builder?.RemoveKeyIfUnused(foreignKey.PrincipalKey);

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static EntityType.Snapshot DetachAllMembers([NotNull] EntityType entityType)
        {
            if (entityType.Builder == null)
            {
                return null;
            }

            if (entityType.HasDefiningNavigation())
            {
                entityType.Model.AddDetachedEntityType(
                    entityType.Name, entityType.DefiningNavigationName, entityType.DefiningEntityType.Name);
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

                    var detachedRelationship = DetachRelationship(relationshipToBeDetached, true);
                    if (detachedRelationship.Relationship.Metadata.GetConfigurationSource().Overrides(ConfigurationSource.DataAnnotation)
                        || relationshipToBeDetached.IsOwnership)
                    {
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

        private void RemoveKeyIfUnused(Key key, ConfigurationSource configurationSource = ConfigurationSource.Convention)
        {
            if (Metadata.FindPrimaryKey() == key)
            {
                return;
            }

            if (key.GetReferencingForeignKeys().Any())
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
        public virtual InternalEntityTypeBuilder RemoveUnusedShadowProperties<T>(
            [NotNull] IReadOnlyList<T> properties, ConfigurationSource configurationSource = ConfigurationSource.Convention)
            where T : class, IProperty
        {
            foreach (var property in properties)
            {
                if (property?.IsShadowProperty() == true)
                {
                    RemovePropertyIfUnused((Property)(object)property, configurationSource);
                }
            }

            return this;
        }

        private static void RemovePropertyIfUnused(Property property, ConfigurationSource configurationSource)
        {
            if (property.Builder == null
                || !property.DeclaringEntityType.Builder.CanRemoveProperty(property, configurationSource)
                || property.GetContainingIndexes().Any()
                || property.GetContainingForeignKeys().Any()
                || property.GetContainingKeys().Any())
            {
                return;
            }

            var removedProperty = property.DeclaringEntityType.RemoveProperty(property.Name);
            Debug.Assert(removedProperty == property);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalIndexBuilder HasIndex([NotNull] IReadOnlyList<string> propertyNames, ConfigurationSource configurationSource)
            => HasIndex(GetOrCreateProperties(propertyNames, configurationSource), configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalIndexBuilder HasIndex(
            [NotNull] IReadOnlyList<MemberInfo> clrMembers, ConfigurationSource configurationSource)
            => HasIndex(GetOrCreateProperties(clrMembers, configurationSource), configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalIndexBuilder HasIndex(
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource configurationSource)
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder HasNoIndex([NotNull] Index index, ConfigurationSource configurationSource)
        {
            var currentConfigurationSource = index.GetConfigurationSource();
            if (!configurationSource.Overrides(currentConfigurationSource))
            {
                return null;
            }

            var removedIndex = Metadata.RemoveIndex(index.Properties);
            Debug.Assert(removedIndex == index);

            RemoveUnusedShadowProperties(index.Properties);

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
            var removedConfigurationSource = entityTypeBuilder.HasNoIndex(indexToDetach, indexToDetach.GetConfigurationSource());
            Debug.Assert(removedConfigurationSource != null);
            return indexBuilder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasRelationship(
            [NotNull] string principalEntityTypeName,
            [NotNull] IReadOnlyList<string> propertyNames,
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
        public virtual InternalRelationshipBuilder HasRelationship(
            [NotNull] string principalEntityTypeName,
            [NotNull] IReadOnlyList<string> propertyNames,
            [NotNull] Key principalKey,
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
        public virtual InternalRelationshipBuilder HasRelationship(
            [NotNull] Type principalClrType,
            [NotNull] IReadOnlyList<MemberInfo> clrMembers,
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
        public virtual InternalRelationshipBuilder HasRelationship(
            [NotNull] Type principalClrType,
            [NotNull] IReadOnlyList<MemberInfo> clrMembers,
            [NotNull] Key principalKey,
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
        public virtual InternalRelationshipBuilder HasRelationship(
            [NotNull] EntityType principalEntityType,
            [NotNull] IReadOnlyList<Property> dependentProperties,
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
        public virtual InternalRelationshipBuilder HasRelationship(
            [NotNull] EntityType principalEntityType,
            [NotNull] IReadOnlyList<Property> dependentProperties,
            [CanBeNull] Key principalKey,
            ConfigurationSource configurationSource)
            => HasForeignKey(
                principalEntityType,
                GetActualProperties(dependentProperties, configurationSource),
                principalKey,
                configurationSource);

        private InternalRelationshipBuilder HasForeignKey(
            EntityType principalEntityType,
            IReadOnlyList<Property> dependentProperties,
            Key principalKey,
            ConfigurationSource configurationSource)
        {
            if (dependentProperties == null)
            {
                return null;
            }

            var newRelationship = HasRelationshipInternal(principalEntityType, principalKey, configurationSource);

            var relationship = newRelationship.HasForeignKey(dependentProperties, configurationSource);
            if (relationship == null
                && newRelationship.Metadata.Builder != null)
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
        public virtual InternalRelationshipBuilder HasRelationship(
            [NotNull] EntityType targetEntityType,
            [CanBeNull] string navigationToTargetName,
            [CanBeNull] string inverseNavigationName,
            ConfigurationSource configurationSource,
            bool setTargetAsPrincipal = false)
            => HasRelationship(
                Check.NotNull(targetEntityType, nameof(targetEntityType)),
                MemberIdentity.Create(navigationToTargetName),
                MemberIdentity.Create(inverseNavigationName),
                setTargetAsPrincipal,
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasRelationship(
            [NotNull] EntityType targetEntityType,
            [CanBeNull] MemberInfo navigationToTarget,
            [CanBeNull] MemberInfo inverseNavigation,
            ConfigurationSource configurationSource,
            bool setTargetAsPrincipal = false)
            => HasRelationship(
                Check.NotNull(targetEntityType, nameof(targetEntityType)),
                MemberIdentity.Create(navigationToTarget),
                MemberIdentity.Create(inverseNavigation),
                setTargetAsPrincipal,
                configurationSource);

        private InternalRelationshipBuilder HasRelationship(
            EntityType targetEntityType,
            MemberIdentity? navigationToTarget,
            MemberIdentity? inverseNavigation,
            bool setTargetAsPrincipal,
            ConfigurationSource configurationSource,
            bool? required = null)
        {
            Debug.Assert(
                navigationToTarget != null
                || inverseNavigation != null);

            var navigationProperty = navigationToTarget?.MemberInfo;
            if (inverseNavigation == null
                && navigationProperty?.GetMemberType().GetTypeInfo().IsAssignableFrom(
                    targetEntityType.ClrType.GetTypeInfo()) == false)
            {
                // Only one nav specified and it can't be the nav to principal
                return targetEntityType.Builder.HasRelationship(
                    Metadata, null, navigationToTarget, setTargetAsPrincipal, configurationSource, required);
            }

            var existingRelationship = InternalRelationshipBuilder.FindCurrentRelationshipBuilder(
                targetEntityType,
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
                        Metadata.RemoveIgnored(navigationToTarget.Value.Name);
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
                        targetEntityType.RemoveIgnored(inverseNavigation.Value.Name);
                    }
                }

                existingRelationship.Metadata.UpdateConfigurationSource(configurationSource);

                if (!shouldInvertNavigations)
                {
                    if (setTargetAsPrincipal)
                    {
                        existingRelationship = existingRelationship.HasEntityTypes(
                            existingRelationship.Metadata.PrincipalEntityType,
                            existingRelationship.Metadata.DeclaringEntityType,
                            configurationSource);
                        if (required.HasValue)
                        {
                            existingRelationship = existingRelationship.IsRequired(required.Value, configurationSource);
                        }
                    }

                    return existingRelationship;
                }
            }

            existingRelationship = InternalRelationshipBuilder.FindCurrentRelationshipBuilder(
                Metadata,
                targetEntityType,
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
                        Metadata.RemoveIgnored(navigationToTarget.Value.Name);
                    }
                }

                if (inverseNavigation != null)
                {
                    existingRelationship.Metadata.UpdateDependentToPrincipalConfigurationSource(configurationSource);
                    if (inverseNavigation.Value.Name != null)
                    {
                        targetEntityType.RemoveIgnored(inverseNavigation.Value.Name);
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
            using (var batcher = Metadata.Model.ConventionDispatcher.DelayConventions())
            {
                if (existingRelationship != null)
                {
                    relationship = existingRelationship;
                }
                else
                {
                    if (setTargetAsPrincipal
                        || targetEntityType.DefiningEntityType != Metadata)
                    {
                        newRelationship = CreateForeignKey(
                            targetEntityType.Builder,
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

                        navigationProperty = navigationToTarget?.MemberInfo;

                        newRelationship = targetEntityType.Builder.CreateForeignKey(
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
                        .HasEntityTypes(targetEntityType.Builder.Metadata, Metadata, configurationSource);

                    if (required.HasValue)
                    {
                        relationship = relationship.IsRequired(required.Value, configurationSource);
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
                            navigationToTarget.Value.Name,
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
                if (newRelationship?.Metadata.Builder != null)
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
        public virtual InternalRelationshipBuilder HasRelationship(
            [NotNull] EntityType principalEntityType,
            ConfigurationSource configurationSource)
            => HasRelationshipInternal(principalEntityType, principalKey: null, configurationSource: configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasRelationship(
            [NotNull] EntityType principalEntityType,
            [NotNull] Key principalKey,
            ConfigurationSource configurationSource)
            => HasRelationshipInternal(principalEntityType, principalKey, configurationSource);

        private InternalRelationshipBuilder HasRelationshipInternal(
            EntityType targetEntityType,
            Key principalKey,
            ConfigurationSource configurationSource)
        {
            InternalRelationshipBuilder relationship;
            InternalRelationshipBuilder newRelationship;
            using (var batch = Metadata.Model.ConventionDispatcher.DelayConventions())
            {
                relationship = CreateForeignKey(
                    targetEntityType.Builder,
                    null,
                    principalKey,
                    null,
                    null,
                    configurationSource);

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
                if (relationship?.Metadata.Builder != null)
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
        public virtual InternalRelationshipBuilder HasOwnership(
            [NotNull] string targetEntityTypeName,
            [NotNull] string navigationName,
            ConfigurationSource configurationSource)
            => HasOwnership(
                new TypeIdentity(targetEntityTypeName), MemberIdentity.Create(navigationName),
                inverse: null, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasOwnership(
            [NotNull] Type targetEntityType,
            [NotNull] string navigationName,
            ConfigurationSource configurationSource)
            => HasOwnership(
                new TypeIdentity(targetEntityType, Metadata.Model), MemberIdentity.Create(navigationName),
                inverse: null, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasOwnership(
            [NotNull] Type targetEntityType,
            [NotNull] MemberInfo navigationProperty,
            ConfigurationSource configurationSource)
            => HasOwnership(
                new TypeIdentity(targetEntityType, Metadata.Model), MemberIdentity.Create(navigationProperty),
                inverse: null, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasOwnership(
            [NotNull] Type targetEntityType,
            [NotNull] string navigationPropertyName,
            [CanBeNull] string inversePropertyName,
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
        public virtual InternalRelationshipBuilder HasOwnership(
            [NotNull] Type targetEntityType,
            [NotNull] MemberInfo navigationProperty,
            [CanBeNull] MemberInfo inverseProperty,
            ConfigurationSource configurationSource)
            => HasOwnership(
                new TypeIdentity(targetEntityType, Metadata.Model),
                MemberIdentity.Create(navigationProperty),
                MemberIdentity.Create(inverseProperty),
                configurationSource);

        private InternalRelationshipBuilder HasOwnership(
            in TypeIdentity targetEntityType,
            MemberIdentity navigation,
            MemberIdentity? inverse,
            ConfigurationSource configurationSource)
        {
            InternalEntityTypeBuilder ownedEntityType;
            InternalRelationshipBuilder relationship;
            using (var batch = Metadata.Model.ConventionDispatcher.DelayConventions())
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
                                ModelBuilder.Entity(targetEntityType.Type, configurationSource, shouldBeOwned: true);
                            }
                            else
                            {
                                ModelBuilder.Entity(targetEntityType.Name, configurationSource, shouldBeOwned: true);
                            }
                        }

                        var ownershipBuilder = existingNavigation.ForeignKey.Builder;
                        ownershipBuilder = ownershipBuilder
                            .IsRequired(true, configurationSource)
                            ?.HasEntityTypes(
                                Metadata, ownershipBuilder.Metadata.FindNavigationsFromInHierarchy(Metadata).Single().GetTargetType(),
                                configurationSource)
                            ?.HasNavigations(inverse, navigation, configurationSource)
                            ?.IsOwnership(true, configurationSource);

                        return ownershipBuilder == null ? null : batch.Run(ownershipBuilder);
                    }

                    if (existingNavigation.ForeignKey.DeclaringEntityType.Builder
                            .HasNoRelationship(existingNavigation.ForeignKey, configurationSource) == null)
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

                        ModelBuilder.Metadata.RemoveIgnored(targetTypeName);

                        ownedEntityType = targetType == null
                            ? ModelBuilder.Entity(targetTypeName, configurationSource, shouldBeOwned: true)
                            : ModelBuilder.Entity(targetType, configurationSource, shouldBeOwned: true);
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

                relationship = ownedEntityType.HasRelationship(
                    targetEntityType: principalBuilder.Metadata,
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
                    ModelBuilder.HasNoEntityType(ownedEntityType.Metadata, configurationSource);
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
        public virtual bool RemoveNonOwnershipRelationships(ForeignKey ownership, ConfigurationSource configurationSource)
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
                .Distinct()
                .ToList();

            if (incompatibleRelationships.Any(fk => !configurationSource.Overrides(fk.GetConfigurationSource())))
            {
                return false;
            }

            foreach (var foreignKey in incompatibleRelationships)
            {
                foreignKey.DeclaringEntityType.Builder.HasNoRelationship(foreignKey, configurationSource);
            }

            return true;
        }

        private bool Contains(IForeignKey inheritedFk, IForeignKey derivedFk)
            => inheritedFk != null
               && inheritedFk.PrincipalEntityType.IsAssignableFrom(derivedFk.PrincipalEntityType)
               && PropertyListComparer.Instance.Equals(inheritedFk.Properties, derivedFk.Properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasRelationship(
            [NotNull] EntityType targetEntityType,
            [CanBeNull] string navigationName,
            ConfigurationSource configurationSource,
            bool setTargetAsPrincipal = false)
            => HasRelationship(
                Check.NotNull(targetEntityType, nameof(targetEntityType)),
                MemberIdentity.Create(navigationName),
                null,
                setTargetAsPrincipal,
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder HasRelationship(
            [NotNull] EntityType targetEntityType,
            [CanBeNull] MemberInfo navigationProperty,
            ConfigurationSource configurationSource,
            bool setTargetAsPrincipal = false)
            => HasRelationship(
                Check.NotNull(targetEntityType, nameof(targetEntityType)),
                MemberIdentity.Create(navigationProperty),
                null,
                setTargetAsPrincipal,
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder GetTargetEntityTypeBuilder(
            Type targetClrType,
            MemberInfo navigationInfo,
            ConfigurationSource? configurationSource)
        {
            var ownership = Metadata.FindOwnership();

            // ReSharper disable CheckForReferenceEqualityInstead.1
            // ReSharper disable CheckForReferenceEqualityInstead.3
            if (ownership != null)
            {
                if (targetClrType.Equals(Metadata.ClrType))
                {
                    return null;
                }

                if (targetClrType.IsAssignableFrom(ownership.PrincipalEntityType.ClrType))
                {
                    if (configurationSource != null)
                    {
                        ownership.PrincipalEntityType.UpdateConfigurationSource(configurationSource.Value);
                    }

                    return ownership.PrincipalEntityType.Builder;
                }
            }

            var entityType = Metadata;
            InternalEntityTypeBuilder targetEntityTypeBuilder = null;
            if (!ModelBuilder.Metadata.EntityTypeShouldHaveDefiningNavigation(targetClrType))
            {
                var targetEntityType = ModelBuilder.Metadata.FindEntityType(targetClrType);

                var existingOwnership = targetEntityType?.FindOwnership();
                if (existingOwnership != null
                    && entityType.Model.IsOwned(targetClrType)
                    && (existingOwnership.PrincipalEntityType != entityType
                        || existingOwnership.PrincipalToDependent.Name != navigationInfo.GetSimpleMemberName()))
                {
                    return configurationSource.HasValue
                           && !targetClrType.Equals(Metadata.ClrType)
                        ? ModelBuilder.Entity(
                            targetClrType, navigationInfo.GetSimpleMemberName(), entityType, configurationSource.Value)
                        : null;
                }

                var owned = existingOwnership != null
                            || entityType.Model.IsOwned(targetClrType);
                targetEntityTypeBuilder = configurationSource.HasValue
                    ? ModelBuilder.Entity(targetClrType, configurationSource.Value, owned)
                    : targetEntityType?.Builder;
            }
            else if (!targetClrType.Equals(Metadata.ClrType))
            {
                if (entityType.DefiningEntityType?.ClrType.Equals(targetClrType) == true)
                {
                    if (configurationSource != null)
                    {
                        entityType.DefiningEntityType.UpdateConfigurationSource(configurationSource.Value);
                    }

                    return entityType.DefiningEntityType.Builder;
                }

                targetEntityTypeBuilder =
                    entityType.FindNavigation(navigationInfo.GetSimpleMemberName())?.GetTargetType().Builder
                    ?? entityType.Model.FindEntityType(
                        targetClrType, navigationInfo.GetSimpleMemberName(), entityType)?.Builder;

                if (targetEntityTypeBuilder == null
                    && configurationSource.HasValue
                    && !entityType.IsInDefinitionPath(targetClrType)
                    && !entityType.IsInOwnershipPath(targetClrType))
                {
                    return ModelBuilder.Entity(
                        targetClrType, navigationInfo.GetSimpleMemberName(), entityType, configurationSource.Value);
                }

                if (configurationSource != null)
                {
                    targetEntityTypeBuilder?.Metadata.UpdateConfigurationSource(configurationSource.Value);
                }
            }
            // ReSharper restore CheckForReferenceEqualityInstead.1
            // ReSharper restore CheckForReferenceEqualityInstead.3

            return targetEntityTypeBuilder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder CreateForeignKey(
            [NotNull] InternalEntityTypeBuilder principalEntityTypeBuilder,
            [CanBeNull] IReadOnlyList<Property> dependentProperties,
            [CanBeNull] Key principalKey,
            [CanBeNull] string navigationToPrincipalName,
            bool? isRequired,
            ConfigurationSource configurationSource)
        {
            using (var batch = ModelBuilder.Metadata.ConventionDispatcher.DelayConventions())
            {
                var foreignKey = SetOrAddForeignKey(
                    null, principalEntityTypeBuilder,
                    dependentProperties, principalKey, navigationToPrincipalName, isRequired, configurationSource);
                if (isRequired.HasValue
                    && foreignKey.IsRequired == isRequired.Value)
                {
                    foreignKey = foreignKey.SetIsRequired(isRequired.Value, configurationSource);
                }

                return (InternalRelationshipBuilder)batch.Run(foreignKey)?.Builder;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder UpdateForeignKey(
            [NotNull] ForeignKey foreignKey,
            [CanBeNull] IReadOnlyList<Property> dependentProperties,
            [CanBeNull] Key principalKey,
            [CanBeNull] string navigationToPrincipalName,
            bool? isRequired,
            ConfigurationSource? configurationSource)
        {
            using (var batch = ModelBuilder.Metadata.ConventionDispatcher.DelayConventions())
            {
                foreignKey = SetOrAddForeignKey(
                    foreignKey, foreignKey.PrincipalEntityType.Builder,
                    dependentProperties, principalKey, navigationToPrincipalName, isRequired, configurationSource);

                return (InternalRelationshipBuilder)batch.Run(foreignKey)?.Builder;
            }
        }

        private ForeignKey SetOrAddForeignKey(
            ForeignKey foreignKey,
            InternalEntityTypeBuilder principalEntityTypeBuilder,
            IReadOnlyList<Property> dependentProperties,
            Key principalKey,
            string navigationToPrincipalName,
            bool? isRequired,
            ConfigurationSource? configurationSource)
        {
            var principalType = principalEntityTypeBuilder.Metadata;
            var principalBaseEntityTypeBuilder = principalType.RootType().Builder;
            if (principalKey == null)
            {
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
                dependentProperties = GetActualProperties(dependentProperties, ConfigurationSource.Convention);
                if (principalKey == null)
                {
                    var principalKeyProperties = principalBaseEntityTypeBuilder.TryCreateUniqueProperties(
                        dependentProperties.Count, null, Enumerable.Repeat("", dependentProperties.Count),
                        dependentProperties.Select(p => p.ClrType), isRequired: true, baseName: "TempId").Item2;

                    principalKey = principalBaseEntityTypeBuilder.HasKeyInternal(principalKeyProperties, ConfigurationSource.Convention)
                        .Metadata;
                }
                else
                {
                    Debug.Assert(
                        foreignKey != null
                        || Metadata.FindForeignKey(dependentProperties, principalKey, principalType) == null);
                }
            }
            else
            {
                if (principalKey == null)
                {
                    var principalKeyProperties = principalBaseEntityTypeBuilder.TryCreateUniqueProperties(
                        1, null, new[] { "TempId" }, new[] { typeof(int) }, isRequired: true, baseName: "").Item2;

                    principalKey = principalBaseEntityTypeBuilder.HasKeyInternal(
                        principalKeyProperties, ConfigurationSource.Convention).Metadata;
                }

                if (foreignKey != null)
                {
                    var oldProperties = foreignKey.Properties;
                    var oldKey = foreignKey.PrincipalKey;
                    var temporaryProperties = CreateUniqueProperties(null, principalKey.Properties, isRequired ?? false, "TempFk");
                    foreignKey.SetProperties(temporaryProperties, principalKey, configurationSource);

                    foreignKey.DeclaringEntityType.Builder.RemoveUnusedShadowProperties(oldProperties);
                    if (oldKey != principalKey)
                    {
                        oldKey.DeclaringEntityType.Builder.RemoveKeyIfUnused(oldKey);
                    }
                }

                var baseName = string.IsNullOrEmpty(navigationToPrincipalName)
                    ? principalType.ShortName()
                    : navigationToPrincipalName;
                dependentProperties = CreateUniqueProperties(null, principalKey.Properties, isRequired ?? false, baseName);
            }

            if (foreignKey == null)
            {
                return Metadata.AddForeignKey(
                    dependentProperties, principalKey, principalType, componentConfigurationSource: null, configurationSource.Value);
            }

            var oldFKProperties = foreignKey.Properties;
            var oldPrincipalKey = foreignKey.PrincipalKey;
            foreignKey.SetProperties(dependentProperties, principalKey, configurationSource);

            if (oldFKProperties != dependentProperties)
            {
                foreignKey.DeclaringEntityType.Builder.RemoveUnusedShadowProperties(oldFKProperties);
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
        public virtual bool ShouldReuniquifyTemporaryProperties([NotNull] ForeignKey foreignKey)
            => TryCreateUniqueProperties(
                    foreignKey.PrincipalKey.Properties.Count,
                    foreignKey.Properties,
                    foreignKey.PrincipalKey.Properties.Select(p => p.Name),
                    foreignKey.PrincipalKey.Properties.Select(p => p.ClrType),
                    foreignKey.IsRequired
                    && foreignKey.GetIsRequiredConfigurationSource().Overrides(ConfigurationSource.Convention),
                    foreignKey.DependentToPrincipal?.Name ?? foreignKey.PrincipalEntityType.ShortName())
                .Item1;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
                        var clrType = keyPropertyType.MakeNullable(!isRequired);
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
                                        clrType, propertyName, typeConfigurationSource: null,
                                        configurationSource: ConfigurationSource.Convention);

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
                                if (currentProperty.IsShadowProperty()
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
                    var type = referencedProperties == null
                        ? useDefaultType
                            ? typeof(int)
                            : null
                        : referencedProperties[i].ClrType;

                    if (!configurationSource.HasValue)
                    {
                        return null;
                    }

                    // TODO: Log that a shadow property is created
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
                        && property.IsShadowProperty()
                        && (!property.IsNullable || (required && property.GetIsNullableConfigurationSource() == null))
                        && property.ClrType.IsNullableType())
                    {
                        property = property.DeclaringEntityType.Builder.Property(
                                property.ClrType.MakeNullable(false),
                                property.Name,
                                configurationSource.Value)
                            .Metadata;
                    }
                    else
                    {
                        property = property.DeclaringEntityType.Builder.Property(property.Name, configurationSource.Value).Metadata;
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
        public virtual IReadOnlyList<Property> GetOrCreateProperties(
            [CanBeNull] IEnumerable<MemberInfo> clrMembers, ConfigurationSource? configurationSource)
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
        public virtual IReadOnlyList<Property> GetActualProperties(
            [CanBeNull] IReadOnlyList<Property> properties, ConfigurationSource? configurationSource)
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
        public virtual InternalEntityTypeBuilder HasChangeTrackingStrategy(
            ChangeTrackingStrategy? changeTrackingStrategy, ConfigurationSource configurationSource)
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
            ChangeTrackingStrategy? changeTrackingStrategy, ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetChangeTrackingStrategyConfigurationSource())
               || Metadata.GetChangeTrackingStrategy() == changeTrackingStrategy;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode, ConfigurationSource configurationSource)
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
            => configurationSource.Overrides(Metadata.GetPropertyAccessModeConfigurationSource())
               || Metadata.GetPropertyAccessMode() == propertyAccessMode;

        private static readonly string _defaultDiscriminatorName = "Discriminator";

        private static readonly Type _defaultDiscriminatorType = typeof(string);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder GetOrCreateDiscriminatorProperty(Type type, string name, bool fromDataAnnotation)
        {
            var discriminatorProperty = ((IEntityType)Metadata).GetDiscriminatorProperty();
            if ((name != null && discriminatorProperty?.Name != name)
                || (type != null && discriminatorProperty?.ClrType != type))
            {
                discriminatorProperty = null;
            }

            var configurationSource = fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention;
            return Metadata.RootType().Builder.Property(
                type ?? discriminatorProperty?.ClrType ?? _defaultDiscriminatorType,
                name ?? discriminatorProperty?.Name ?? _defaultDiscriminatorName,
                typeConfigurationSource: type != null ? configurationSource : (ConfigurationSource?)null,
                configurationSource: configurationSource);
        }

        public virtual DiscriminatorBuilder DiscriminatorBuilder(
            InternalPropertyBuilder discriminatorPropertyBuilder,
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
                discriminatorProperty.ClrType, discriminatorProperty.Name, null, ConfigurationSource.Convention);

            RemoveUnusedDiscriminatorProperty(discriminatorProperty, configurationSource);

            rootTypeBuilder.Metadata.SetDiscriminatorProperty(discriminatorProperty, configurationSource);
            discriminatorPropertyBuilder.IsRequired(true, configurationSource);
            discriminatorPropertyBuilder.HasValueGenerator(DiscriminatorValueGenerator.Factory, configurationSource);

            return new DiscriminatorBuilder(Metadata);
        }

        public virtual InternalEntityTypeBuilder HasNoDeclaredDiscriminator(ConfigurationSource configurationSource)
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

            return this;
        }

        private void RemoveUnusedDiscriminatorProperty(Property newDiscriminatorProperty, ConfigurationSource configurationSource)
        {
            var oldDiscriminatorProperty = ((IEntityType)Metadata).GetDiscriminatorProperty() as Property;
            if (oldDiscriminatorProperty?.Builder != null
                && oldDiscriminatorProperty != newDiscriminatorProperty)
            {
                oldDiscriminatorProperty.DeclaringEntityType.Builder.RemoveUnusedShadowProperties(
                    new[] { oldDiscriminatorProperty });

                if (oldDiscriminatorProperty.Builder != null)
                {
                    oldDiscriminatorProperty.Builder.IsRequired(null, configurationSource);
                    oldDiscriminatorProperty.Builder.HasValueGenerator((Type)null, configurationSource);
                }
            }
        }

        private bool CanSetDiscriminator(
            IProperty discriminatorProperty,
            string name,
            Type discriminatorType,
            bool fromDataAnnotation)
            => ((name == null && discriminatorType == null)
               || ((name == null || discriminatorProperty?.Name == name)
                   && (discriminatorType == null || discriminatorProperty?.ClrType == discriminatorType))
               || (fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                    .Overrides(Metadata.GetDiscriminatorPropertyConfigurationSource()))
            && (discriminatorProperty != null
                 || Metadata.RootType().Builder.CanAddProperty(
                        discriminatorType ?? _defaultDiscriminatorType,
                        name ?? _defaultDiscriminatorName,
                        typeConfigurationSource: discriminatorType != null
                            ? fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention
                            : (ConfigurationSource?)null));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityType IConventionEntityTypeBuilder.Metadata
        {
            [DebuggerStepThrough] get => Metadata;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityTypeBuilder IConventionEntityTypeBuilder.HasBaseType(IConventionEntityType baseEntityType, bool fromDataAnnotation)
            => HasBaseType(
                (EntityType)baseEntityType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionEntityTypeBuilder.CanSetBaseType(IConventionEntityType baseEntityType, bool fromDataAnnotation)
            => CanSetBaseType(
                (EntityType)baseEntityType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionPropertyBuilder IConventionEntityTypeBuilder.Property(
            Type propertyType, string propertyName, bool setTypeConfigurationSource, bool fromDataAnnotation)
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
        IConventionPropertyBuilder IConventionEntityTypeBuilder.Property(MemberInfo memberInfo, bool fromDataAnnotation)
            => Property(memberInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IReadOnlyList<IConventionProperty> IConventionEntityTypeBuilder.GetOrCreateProperties(
            IReadOnlyList<string> propertyNames, bool fromDataAnnotation)
            => GetOrCreateProperties(
                propertyNames, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IReadOnlyList<IConventionProperty> IConventionEntityTypeBuilder.GetOrCreateProperties(
            IEnumerable<MemberInfo> memberInfos, bool fromDataAnnotation)
            => GetOrCreateProperties(memberInfos, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityTypeBuilder IConventionEntityTypeBuilder.RemoveUnusedShadowProperties(
            IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation)
            => RemoveUnusedShadowProperties(
                properties, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionServicePropertyBuilder IConventionEntityTypeBuilder.ServiceProperty(MemberInfo memberInfo, bool fromDataAnnotation)
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
        IConventionEntityTypeBuilder IConventionEntityTypeBuilder.Ignore(string name, bool fromDataAnnotation)
            => Ignore(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionEntityTypeBuilder.CanIgnore(string name, bool fromDataAnnotation)
            => CanIgnore(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionKeyBuilder IConventionEntityTypeBuilder.PrimaryKey(
            IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation)
            => PrimaryKey(
                properties as IReadOnlyList<Property> ?? properties?.Cast<Property>().ToList(),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionEntityTypeBuilder.CanSetPrimaryKey(IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation)
            => CanSetPrimaryKey(
                properties as IReadOnlyList<Property> ?? properties?.Cast<Property>().ToList(),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionKeyBuilder IConventionEntityTypeBuilder.HasKey(IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation)
            => HasKey(
                properties as IReadOnlyList<Property> ?? properties.Cast<Property>().ToList(),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityTypeBuilder IConventionEntityTypeBuilder.HasNoKey(bool fromDataAnnotation)
            => HasNoKey(fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityTypeBuilder IConventionEntityTypeBuilder.HasNoKey(
            IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation)
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
        IConventionEntityTypeBuilder IConventionEntityTypeBuilder.HasNoKey(IConventionKey key, bool fromDataAnnotation)
            => HasNoKey((Key)key, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionIndexBuilder IConventionEntityTypeBuilder.HasIndex(
            IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation)
            => HasIndex(
                properties as IReadOnlyList<Property> ?? properties.Cast<Property>().ToList(),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityTypeBuilder IConventionEntityTypeBuilder.HasNoIndex(
            IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation)
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
        IConventionEntityTypeBuilder IConventionEntityTypeBuilder.HasNoIndex(IConventionIndex index, bool fromDataAnnotation)
            => HasNoIndex((Index)index, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionRelationshipBuilder IConventionEntityTypeBuilder.HasRelationship(
            IConventionEntityType targetEntityType, bool fromDataAnnotation)
            => HasRelationship(
                (EntityType)targetEntityType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionRelationshipBuilder IConventionEntityTypeBuilder.HasRelationship(
            IConventionEntityType principalEntityType, IReadOnlyList<IConventionProperty> dependentProperties, bool fromDataAnnotation)
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
        IConventionRelationshipBuilder IConventionEntityTypeBuilder.HasRelationship(
            IConventionEntityType principalEntityType, IConventionKey principalKey, bool fromDataAnnotation)
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
        IConventionRelationshipBuilder IConventionEntityTypeBuilder.HasRelationship(
            IConventionEntityType principalEntityType, IReadOnlyList<IConventionProperty> dependentProperties, IConventionKey principalKey,
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
        IConventionRelationshipBuilder IConventionEntityTypeBuilder.HasRelationship(
            IConventionEntityType targetEntityType, string navigationToTargetName, bool setTargetAsPrincipal, bool fromDataAnnotation)
            => HasRelationship(
                (EntityType)targetEntityType,
                navigationToTargetName,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
                setTargetAsPrincipal);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionRelationshipBuilder IConventionEntityTypeBuilder.HasRelationship(
            IConventionEntityType targetEntityType, MemberInfo navigationToTarget, bool setTargetAsPrincipal, bool fromDataAnnotation)
            => HasRelationship(
                (EntityType)targetEntityType,
                navigationToTarget,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
                setTargetAsPrincipal);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionRelationshipBuilder IConventionEntityTypeBuilder.HasRelationship(
            IConventionEntityType targetEntityType,
            string navigationToTargetName,
            string inverseNavigationName,
            bool setTargetAsPrincipal,
            bool fromDataAnnotation)
            => HasRelationship(
                (EntityType)targetEntityType,
                navigationToTargetName, inverseNavigationName,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
                setTargetAsPrincipal);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionRelationshipBuilder IConventionEntityTypeBuilder.HasRelationship(
            IConventionEntityType targetEntityType,
            MemberInfo navigationToTarget,
            MemberInfo inverseNavigation,
            bool setTargetAsPrincipal,
            bool fromDataAnnotation)
            => HasRelationship(
                (EntityType)targetEntityType,
                navigationToTarget, inverseNavigation,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
                setTargetAsPrincipal);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionRelationshipBuilder IConventionEntityTypeBuilder.HasOwnership(
            Type targetEntityType, string navigationToTargetName, bool fromDataAnnotation)
            => HasOwnership(
                targetEntityType, navigationToTargetName,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionRelationshipBuilder IConventionEntityTypeBuilder.HasOwnership(
            Type targetEntityType, MemberInfo navigationToTarget, bool fromDataAnnotation)
            => HasOwnership(
                targetEntityType, navigationToTarget,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionRelationshipBuilder IConventionEntityTypeBuilder.HasOwnership(
            Type targetEntityType, string navigationToTargetName, string inversePropertyName, bool fromDataAnnotation)
            => HasOwnership(
                targetEntityType, navigationToTargetName, inversePropertyName,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionRelationshipBuilder IConventionEntityTypeBuilder.HasOwnership(
            Type targetEntityType, MemberInfo navigationToTarget, MemberInfo inverseProperty, bool fromDataAnnotation)
            => HasOwnership(
                targetEntityType, navigationToTarget, inverseProperty,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityTypeBuilder IConventionEntityTypeBuilder.HasNoRelationship(
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
        IConventionEntityTypeBuilder IConventionEntityTypeBuilder.HasNoRelationship(
            IConventionForeignKey foreignKey, bool fromDataAnnotation)
            => HasNoRelationship(
                (ForeignKey)foreignKey,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionEntityTypeBuilder.CanAddNavigation(string navigationName, bool fromDataAnnotation)
            => CanAddNavigation(
                navigationName,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityTypeBuilder IConventionEntityTypeBuilder.HasQueryFilter(LambdaExpression filter, bool fromDataAnnotation)
            => HasQueryFilter(filter, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionEntityTypeBuilder.CanSetQueryFilter(LambdaExpression filter, bool fromDataAnnotation)
            => CanSetQueryFilter(filter, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityTypeBuilder IConventionEntityTypeBuilder.HasDefiningQuery(LambdaExpression query, bool fromDataAnnotation)
            => HasDefiningQuery(query, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionEntityTypeBuilder.CanSetDefiningQuery(LambdaExpression query, bool fromDataAnnotation)
            => CanSetDefiningQuery(query, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityTypeBuilder IConventionEntityTypeBuilder.HasChangeTrackingStrategy(
            ChangeTrackingStrategy? changeTrackingStrategy, bool fromDataAnnotation)
            => HasChangeTrackingStrategy(
                changeTrackingStrategy, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionEntityTypeBuilder.CanSetChangeTrackingStrategy(
            ChangeTrackingStrategy? changeTrackingStrategy, bool fromDataAnnotation)
            => CanSetChangeTrackingStrategy(
                changeTrackingStrategy, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityTypeBuilder IConventionEntityTypeBuilder.UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation)
            => UsePropertyAccessMode(
                propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionEntityTypeBuilder.CanSetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation)
            => CanSetPropertyAccessMode(
                propertyAccessMode, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionDiscriminatorBuilder IConventionEntityTypeBuilder.HasDiscriminator(bool fromDataAnnotation)
            => DiscriminatorBuilder(
                GetOrCreateDiscriminatorProperty(type: null, name: null, fromDataAnnotation: false),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionDiscriminatorBuilder IConventionEntityTypeBuilder.HasDiscriminator(Type type, bool fromDataAnnotation)
            => ((IConventionEntityTypeBuilder)this).CanSetDiscriminator(type, fromDataAnnotation)
                ? DiscriminatorBuilder(
                    GetOrCreateDiscriminatorProperty(type, name: null, fromDataAnnotation),
                    fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionDiscriminatorBuilder IConventionEntityTypeBuilder.HasDiscriminator(string name, bool fromDataAnnotation)
            => ((IConventionEntityTypeBuilder)this).CanSetDiscriminator(name, fromDataAnnotation)
                ? DiscriminatorBuilder(
                    GetOrCreateDiscriminatorProperty(type: null, name, fromDataAnnotation),
                    fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionDiscriminatorBuilder IConventionEntityTypeBuilder.HasDiscriminator(string name, Type type, bool fromDataAnnotation)
            => ((IConventionEntityTypeBuilder)this).CanSetDiscriminator(type, name, fromDataAnnotation)
                ? DiscriminatorBuilder(
                    Metadata.RootType().Builder.Property(
                        type, name,
                        fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention),
                    fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionDiscriminatorBuilder IConventionEntityTypeBuilder.HasDiscriminator(MemberInfo memberInfo, bool fromDataAnnotation)
            => ((IConventionEntityTypeBuilder)this).CanSetDiscriminator(
                memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName(), fromDataAnnotation)
                ? DiscriminatorBuilder(
                    Metadata.RootType().Builder.Property(
                        memberInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention),
                    fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityTypeBuilder IConventionEntityTypeBuilder.HasNoDeclaredDiscriminator(bool fromDataAnnotation)
            => HasNoDeclaredDiscriminator(fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionEntityTypeBuilder.CanSetDiscriminator(string name, bool fromDataAnnotation)
            => CanSetDiscriminator(
                ((IEntityType)Metadata).GetDiscriminatorProperty(), name, discriminatorType: null,
                fromDataAnnotation);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionEntityTypeBuilder.CanSetDiscriminator(Type type, bool fromDataAnnotation)
            => CanSetDiscriminator(((IEntityType)Metadata).GetDiscriminatorProperty(), name: null, type, fromDataAnnotation);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionEntityTypeBuilder.CanSetDiscriminator(Type type, string name, bool fromDataAnnotation)
            => CanSetDiscriminator(((IEntityType)Metadata).GetDiscriminatorProperty(), name, type, fromDataAnnotation);
    }
}
