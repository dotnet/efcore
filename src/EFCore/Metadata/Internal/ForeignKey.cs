// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ForeignKey : ConventionAnnotatable, IMutableForeignKey, IConventionForeignKey
    {
        private DeleteBehavior? _deleteBehavior;
        private bool? _isUnique;
        private bool _isRequired;
        private bool? _isRequiredDependent;
        private bool? _isOwnership;

        private ConfigurationSource _configurationSource;
        private ConfigurationSource? _propertiesConfigurationSource;
        private ConfigurationSource? _principalKeyConfigurationSource;
        private ConfigurationSource? _isUniqueConfigurationSource;
        private ConfigurationSource? _isRequiredConfigurationSource;
        private ConfigurationSource? _isRequiredDependentConfigurationSource;
        private ConfigurationSource? _deleteBehaviorConfigurationSource;
        private ConfigurationSource? _principalEndConfigurationSource;
        private ConfigurationSource? _isOwnershipConfigurationSource;
        private ConfigurationSource? _dependentToPrincipalConfigurationSource;
        private ConfigurationSource? _principalToDependentConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ForeignKey(
            [NotNull] IReadOnlyList<Property> dependentProperties,
            [NotNull] Key principalKey,
            [NotNull] EntityType dependentEntityType,
            [NotNull] EntityType principalEntityType,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(dependentProperties, nameof(dependentProperties));
            Check.HasNoNulls(dependentProperties, nameof(dependentProperties));
            Check.NotNull(principalKey, nameof(principalKey));
            Check.NotNull(principalEntityType, nameof(principalEntityType));

            Validate(dependentProperties, principalKey, dependentEntityType, principalEntityType);

            Properties = dependentProperties;
            PrincipalKey = principalKey;
            DeclaringEntityType = dependentEntityType;
            PrincipalEntityType = principalEntityType;
            _configurationSource = configurationSource;
            _isRequired = DefaultIsRequired;

            if (principalEntityType.FindKey(principalKey.Properties) != principalKey)
            {
                throw new InvalidOperationException(
                    CoreStrings.ForeignKeyReferencedEntityKeyMismatch(
                        principalKey.Properties.Format(),
                        principalEntityType.DisplayName()));
            }

            Builder = new InternalForeignKeyBuilder(this, dependentEntityType.Model.Builder);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<Property> Properties { get; private set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key PrincipalKey { get; private set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType DeclaringEntityType { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType PrincipalEntityType { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalForeignKeyBuilder Builder
        {
            get;

            [param: CanBeNull]
            set;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedSet<SkipNavigation> ReferencingSkipNavigations { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<SkipNavigation> GetReferencingSkipNavigations()
            => ReferencingSkipNavigations ?? Enumerable.Empty<SkipNavigation>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public virtual ConfigurationSource GetConfigurationSource()
            => _configurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
        {
            _configurationSource = _configurationSource.Max(configurationSource);

            DeclaringEntityType.UpdateConfigurationSource(configurationSource);
            PrincipalEntityType.UpdateConfigurationSource(configurationSource);
        }

        /// <summary>
        ///     Runs the conventions when an annotation was set or removed.
        /// </summary>
        /// <param name="name"> The key of the set annotation. </param>
        /// <param name="annotation"> The annotation set. </param>
        /// <param name="oldAnnotation"> The old annotation. </param>
        /// <returns> The annotation that was set. </returns>
        protected override IConventionAnnotation OnAnnotationSet(
            string name,
            IConventionAnnotation annotation,
            IConventionAnnotation oldAnnotation)
            => Builder.ModelBuilder.Metadata.ConventionDispatcher.OnForeignKeyAnnotationChanged(Builder, name, annotation, oldAnnotation);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<Property> SetProperties(
            [NotNull] IReadOnlyList<Property> properties,
            [NotNull] Key principalKey,
            ConfigurationSource? configurationSource)
        {
            Validate(properties, principalKey, DeclaringEntityType, PrincipalEntityType);

            var oldProperties = Properties;
            var oldPrincipalKey = PrincipalKey;

            DeclaringEntityType.OnForeignKeyUpdating(this);

            Properties = properties;
            PrincipalKey = principalKey;

            DeclaringEntityType.OnForeignKeyUpdated(this);

            if (configurationSource != null)
            {
                UpdatePropertiesConfigurationSource(configurationSource.Value);
                UpdatePrincipalKeyConfigurationSource(configurationSource.Value);
            }

            return (IReadOnlyList<Property>)DeclaringEntityType.Model.ConventionDispatcher
                .OnForeignKeyPropertiesChanged(Builder, oldProperties, oldPrincipalKey);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public virtual ConfigurationSource? GetPropertiesConfigurationSource()
            => _propertiesConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void UpdatePropertiesConfigurationSource(ConfigurationSource configurationSource)
        {
            _propertiesConfigurationSource = configurationSource.Max(_propertiesConfigurationSource);
            foreach (var property in Properties)
            {
                property.UpdateConfigurationSource(configurationSource);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public virtual ConfigurationSource? GetPrincipalKeyConfigurationSource()
            => _principalKeyConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void UpdatePrincipalKeyConfigurationSource(ConfigurationSource configurationSource)
        {
            _principalKeyConfigurationSource = configurationSource.Max(_principalKeyConfigurationSource);
            PrincipalKey.UpdateConfigurationSource(configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public virtual ConfigurationSource? GetPrincipalEndConfigurationSource()
            => _principalEndConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetPrincipalEndConfigurationSource(ConfigurationSource? configurationSource)
            => _principalEndConfigurationSource = configurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void UpdatePrincipalEndConfigurationSource(ConfigurationSource configurationSource)
            => _principalEndConfigurationSource = configurationSource.Max(_principalEndConfigurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Navigation DependentToPrincipal { [DebuggerStepThrough] get; private set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Navigation SetDependentToPrincipal(
            [CanBeNull] string name,
            ConfigurationSource configurationSource)
            => Navigation(MemberIdentity.Create(name), configurationSource, pointsToPrincipal: true);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Navigation SetDependentToPrincipal(
            [CanBeNull] MemberInfo property,
            ConfigurationSource configurationSource)
            => Navigation(MemberIdentity.Create(property), configurationSource, pointsToPrincipal: true);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public virtual ConfigurationSource? GetDependentToPrincipalConfigurationSource()
            => _dependentToPrincipalConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void UpdateDependentToPrincipalConfigurationSource(ConfigurationSource? configurationSource)
            => _dependentToPrincipalConfigurationSource = configurationSource.Max(_dependentToPrincipalConfigurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Navigation PrincipalToDependent { [DebuggerStepThrough] get; private set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Navigation HasPrincipalToDependent(
            [CanBeNull] string name,
            ConfigurationSource configurationSource)
            => Navigation(MemberIdentity.Create(name), configurationSource, pointsToPrincipal: false);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Navigation HasPrincipalToDependent(
            [CanBeNull] MemberInfo property,
            ConfigurationSource configurationSource)
            => Navigation(MemberIdentity.Create(property), configurationSource, pointsToPrincipal: false);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public virtual ConfigurationSource? GetPrincipalToDependentConfigurationSource()
            => _principalToDependentConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void UpdatePrincipalToDependentConfigurationSource(ConfigurationSource? configurationSource)
            => _principalToDependentConfigurationSource = configurationSource.Max(_principalToDependentConfigurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        private Navigation Navigation(
            MemberIdentity? propertyIdentity,
            ConfigurationSource configurationSource,
            bool pointsToPrincipal)
        {
            var name = propertyIdentity?.Name;
            if (pointsToPrincipal
                && PrincipalEntityType.IsKeyless)
            {
                throw new InvalidOperationException(
                    CoreStrings.NavigationToKeylessType(name, PrincipalEntityType.DisplayName()));
            }

            if (!pointsToPrincipal
                && DeclaringEntityType.IsKeyless)
            {
                throw new InvalidOperationException(
                    CoreStrings.NavigationToKeylessType(name, DeclaringEntityType.DisplayName()));
            }

            var oldNavigation = pointsToPrincipal ? DependentToPrincipal : PrincipalToDependent;
            if (name == oldNavigation?.Name)
            {
                if (pointsToPrincipal)
                {
                    UpdateDependentToPrincipalConfigurationSource(configurationSource);
                }
                else
                {
                    UpdatePrincipalToDependentConfigurationSource(configurationSource);
                }

                return oldNavigation;
            }

            if (oldNavigation != null)
            {
                Check.DebugAssert(oldNavigation.Name != null, "oldNavigation.Name is null");
                oldNavigation.Builder = null;
                if (pointsToPrincipal)
                {
                    DeclaringEntityType.RemoveNavigation(oldNavigation.Name);
                }
                else
                {
                    PrincipalEntityType.RemoveNavigation(oldNavigation.Name);
                }
            }

            Navigation navigation = null;
            var property = propertyIdentity?.MemberInfo;
            if (property != null)
            {
                navigation = pointsToPrincipal
                    ? DeclaringEntityType.AddNavigation(property, this, pointsToPrincipal: true)
                    : PrincipalEntityType.AddNavigation(property, this, pointsToPrincipal: false);
            }
            else if (name != null)
            {
                navigation = pointsToPrincipal
                    ? DeclaringEntityType.AddNavigation(name, this, pointsToPrincipal: true)
                    : PrincipalEntityType.AddNavigation(name, this, pointsToPrincipal: false);
            }

            if (pointsToPrincipal)
            {
                DependentToPrincipal = navigation;
                UpdateDependentToPrincipalConfigurationSource(configurationSource);
            }
            else
            {
                PrincipalToDependent = navigation;
                UpdatePrincipalToDependentConfigurationSource(configurationSource);
            }

            if (oldNavigation != null)
            {
                Check.DebugAssert(oldNavigation.Name != null, "oldNavigation.Name is null");

                if (pointsToPrincipal)
                {
                    DeclaringEntityType.Model.ConventionDispatcher.OnNavigationRemoved(
                        DeclaringEntityType.Builder,
                        PrincipalEntityType.Builder,
                        oldNavigation.Name,
                        oldNavigation.GetIdentifyingMemberInfo());
                }
                else
                {
                    DeclaringEntityType.Model.ConventionDispatcher.OnNavigationRemoved(
                        PrincipalEntityType.Builder,
                        DeclaringEntityType.Builder,
                        oldNavigation.Name,
                        oldNavigation.GetIdentifyingMemberInfo());
                }
            }

            if (navigation != null)
            {
                navigation = (Navigation)DeclaringEntityType.Model.ConventionDispatcher.OnNavigationAdded(navigation.Builder)?.Metadata;
            }

            return navigation ?? oldNavigation;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsUnique
        {
            get => _isUnique ?? DefaultIsUnique;
            set => SetIsUnique(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool? SetIsUnique(bool? unique, ConfigurationSource configurationSource)
        {
            var oldUnique = IsUnique;
            _isUnique = unique;

            if (unique == false
                && IsRequiredDependent)
            {
                throw new InvalidOperationException(
                    CoreStrings.NonUniqueRequiredDependentForeignKey(Properties.Format(), DeclaringEntityType.DisplayName()));
            }

            if (unique.HasValue
                && PrincipalEntityType.ClrType != null
                && PrincipalToDependent != null)
            {
                if (!Internal.Navigation.IsCompatible(
                    PrincipalToDependent.GetIdentifyingMemberInfo(),
                    PrincipalEntityType.ClrType,
                    DeclaringEntityType.ClrType,
                    !unique,
                    shouldThrow: false))
                {
                    throw new InvalidOperationException(
                        CoreStrings.UnableToSetIsUnique(
                            unique.Value,
                            PrincipalToDependent.PropertyInfo.Name,
                            PrincipalEntityType.DisplayName()));
                }
            }

            _isUniqueConfigurationSource = unique == null
                ? null
                : (ConfigurationSource?)configurationSource.Max(_isUniqueConfigurationSource);

            return IsUnique != oldUnique
                ? DeclaringEntityType.Model.ConventionDispatcher.OnForeignKeyUniquenessChanged(Builder)
                : oldUnique;
        }

        private static bool DefaultIsUnique
            => false;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public virtual ConfigurationSource? GetIsUniqueConfigurationSource()
            => _isUniqueConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsRequired
        {
            get => _isRequired;
            set => SetIsRequired(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool? SetIsRequired(bool? required, ConfigurationSource configurationSource)
        {
            var oldRequired = IsRequired;
            _isRequired = required ?? DefaultIsRequired;

            _isRequiredConfigurationSource = required == null
                ? null
                : (ConfigurationSource?)configurationSource.Max(_isRequiredConfigurationSource);

            return IsRequired != oldRequired
                ? DeclaringEntityType.Model.ConventionDispatcher.OnForeignKeyRequirednessChanged(Builder)
                : oldRequired;
        }

        private bool DefaultIsRequired
            => !Properties.Any(p => p.IsNullable);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public virtual ConfigurationSource? GetIsRequiredConfigurationSource()
            => _isRequiredConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetIsRequiredConfigurationSource(ConfigurationSource? configurationSource)
            => _isRequiredConfigurationSource = configurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsRequiredDependent
        {
            get => _isRequiredDependent ?? DefaultIsRequiredDependent;
            set => SetIsRequiredDependent(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool? SetIsRequiredDependent(bool? required, ConfigurationSource configurationSource)
        {
            if (!IsUnique
                && required == true)
            {
                throw new InvalidOperationException(
                    CoreStrings.NonUniqueRequiredDependentForeignKey(Properties.Format(), DeclaringEntityType.DisplayName()));
            }

            var oldRequired = IsRequiredDependent;
            _isRequiredDependent = required;

            _isRequiredDependentConfigurationSource = required == null
                ? null
                : (ConfigurationSource?)configurationSource.Max(_isRequiredConfigurationSource);

            return IsRequiredDependent != oldRequired
                ? DeclaringEntityType.Model.ConventionDispatcher.OnForeignKeyDependentRequirednessChanged(Builder)
                : oldRequired;
        }

        private bool DefaultIsRequiredDependent
            => false;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public virtual ConfigurationSource? GetIsRequiredDependentConfigurationSource()
            => _isRequiredDependentConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetIsRequiredDependentConfigurationSource(ConfigurationSource? configurationSource)
            => _isRequiredDependentConfigurationSource = configurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DeleteBehavior DeleteBehavior
        {
            get => _deleteBehavior ?? DefaultDeleteBehavior;
            set => SetDeleteBehavior(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DeleteBehavior? SetDeleteBehavior(DeleteBehavior? deleteBehavior, ConfigurationSource configurationSource)
        {
            _deleteBehavior = deleteBehavior;

            if (deleteBehavior == null)
            {
                _deleteBehaviorConfigurationSource = null;
            }
            else
            {
                UpdateDeleteBehaviorConfigurationSource(configurationSource);
            }

            return DeleteBehavior;
        }

        private static DeleteBehavior DefaultDeleteBehavior
            => DeleteBehavior.ClientSetNull;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public virtual ConfigurationSource? GetDeleteBehaviorConfigurationSource()
            => _deleteBehaviorConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void UpdateDeleteBehaviorConfigurationSource(ConfigurationSource configurationSource)
            => _deleteBehaviorConfigurationSource = configurationSource.Max(_deleteBehaviorConfigurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsOwnership
        {
            get => _isOwnership ?? DefaultIsOwnership;
            set => SetIsOwnership(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool? SetIsOwnership(bool? ownership, ConfigurationSource configurationSource)
        {
            var oldIsOwnership = IsOwnership;
            _isOwnership = ownership;

            if (_isOwnership == null)
            {
                _isOwnershipConfigurationSource = null;
            }
            else
            {
                UpdateIsOwnershipConfigurationSource(configurationSource);
            }

            return IsOwnership != oldIsOwnership
                ? DeclaringEntityType.Model.ConventionDispatcher.OnForeignKeyOwnershipChanged(Builder)
                : oldIsOwnership;
        }

        private static bool DefaultIsOwnership
            => false;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public virtual ConfigurationSource? GetIsOwnershipConfigurationSource()
            => _isOwnershipConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void UpdateIsOwnershipConfigurationSource(ConfigurationSource configurationSource)
            => _isOwnershipConfigurationSource = configurationSource.Max(_isOwnershipConfigurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Navigation> FindNavigationsFromInHierarchy([NotNull] EntityType entityType)
            => ((IForeignKey)this).FindNavigationsFromInHierarchy(entityType).Cast<Navigation>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Navigation> FindNavigationsTo([NotNull] EntityType entityType)
            => ((IForeignKey)this).FindNavigationsTo(entityType).Cast<Navigation>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType ResolveOtherEntityType([NotNull] EntityType entityType)
            => (EntityType)((IForeignKey)this).GetRelatedEntityType(entityType);

        // Note: This is set and used only by IdentityMapFactoryFactory, which ensures thread-safety
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual object DependentKeyValueFactory { get; [param: NotNull] set; }

        // Note: This is set and used only by IdentityMapFactoryFactory, which ensures thread-safety
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<IDependentsMap> DependentsMapFactory { get; [param: NotNull] set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IReadOnlyList<IProperty> IForeignKey.Properties
        {
            [DebuggerStepThrough] get => Properties;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IKey IForeignKey.PrincipalKey
        {
            [DebuggerStepThrough] get => PrincipalKey;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEntityType IForeignKey.DeclaringEntityType
        {
            [DebuggerStepThrough] get => DeclaringEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEntityType IForeignKey.PrincipalEntityType
        {
            [DebuggerStepThrough] get => PrincipalEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        INavigation IForeignKey.DependentToPrincipal
        {
            [DebuggerStepThrough] get => DependentToPrincipal;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        INavigation IForeignKey.PrincipalToDependent
        {
            [DebuggerStepThrough] get => PrincipalToDependent;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IReadOnlyList<IMutableProperty> IMutableForeignKey.Properties
        {
            [DebuggerStepThrough] get => Properties;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableKey IMutableForeignKey.PrincipalKey
        {
            [DebuggerStepThrough] get => PrincipalKey;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableEntityType IMutableForeignKey.DeclaringEntityType
        {
            [DebuggerStepThrough] get => DeclaringEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableEntityType IMutableForeignKey.PrincipalEntityType
        {
            [DebuggerStepThrough] get => PrincipalEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableNavigation IMutableForeignKey.DependentToPrincipal
        {
            [DebuggerStepThrough] get => DependentToPrincipal;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableNavigation IMutableForeignKey.PrincipalToDependent
        {
            [DebuggerStepThrough] get => PrincipalToDependent;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        void IMutableForeignKey.SetProperties(IReadOnlyList<IMutableProperty> properties, IMutableKey principalKey)
            => SetProperties((IReadOnlyList<Property>)properties, (Key)principalKey, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableNavigation IMutableForeignKey.SetDependentToPrincipal(string name)
            => SetDependentToPrincipal(name, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableNavigation IMutableForeignKey.SetDependentToPrincipal(MemberInfo property)
            => SetDependentToPrincipal(property, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableNavigation IMutableForeignKey.SetPrincipalToDependent(string name)
            => HasPrincipalToDependent(name, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableNavigation IMutableForeignKey.SetPrincipalToDependent(MemberInfo property)
            => HasPrincipalToDependent(property, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityType IConventionForeignKey.DeclaringEntityType
        {
            [DebuggerStepThrough] get => DeclaringEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityType IConventionForeignKey.PrincipalEntityType
        {
            [DebuggerStepThrough] get => PrincipalEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionKey IConventionForeignKey.PrincipalKey
        {
            [DebuggerStepThrough] get => PrincipalKey;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IReadOnlyList<IConventionProperty> IConventionForeignKey.Properties
        {
            [DebuggerStepThrough] get => Properties;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionNavigation IConventionForeignKey.DependentToPrincipal
        {
            [DebuggerStepThrough] get => DependentToPrincipal;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionNavigation IConventionForeignKey.PrincipalToDependent
        {
            [DebuggerStepThrough] get => PrincipalToDependent;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionForeignKeyBuilder IConventionForeignKey.Builder
        {
            [DebuggerStepThrough] get => Builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionAnnotatableBuilder IConventionAnnotatable.Builder
        {
            [DebuggerStepThrough] get => Builder;
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        IReadOnlyList<IConventionProperty> IConventionForeignKey.SetProperties(
            IReadOnlyList<IConventionProperty> properties,
            IConventionKey principalKey,
            bool fromDataAnnotation)
            => SetProperties(
                properties.Cast<Property>().ToArray(), (Key)principalKey,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionNavigation IConventionForeignKey.SetDependentToPrincipal(string name, bool fromDataAnnotation)
            => SetDependentToPrincipal(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionNavigation IConventionForeignKey.SetDependentToPrincipal(MemberInfo property, bool fromDataAnnotation)
            => SetDependentToPrincipal(property, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionNavigation IConventionForeignKey.SetPrincipalToDependent(string name, bool fromDataAnnotation)
            => HasPrincipalToDependent(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionNavigation IConventionForeignKey.SetPrincipalToDependent(MemberInfo property, bool fromDataAnnotation)
            => HasPrincipalToDependent(property, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IEnumerable<ISkipNavigation> IForeignKey.GetReferencingSkipNavigations()
            => GetReferencingSkipNavigations();

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool? IConventionForeignKey.SetIsUnique(bool? unique, bool fromDataAnnotation)
            => SetIsUnique(unique, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool? IConventionForeignKey.SetIsRequired(bool? required, bool fromDataAnnotation)
            => SetIsRequired(required, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool? IConventionForeignKey.SetIsRequiredDependent(bool? required, bool fromDataAnnotation)
            => SetIsRequiredDependent(required, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool? IConventionForeignKey.SetIsOwnership(bool? ownership, bool fromDataAnnotation)
            => SetIsOwnership(ownership, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        DeleteBehavior? IConventionForeignKey.SetDeleteBehavior(DeleteBehavior? deleteBehavior, bool fromDataAnnotation)
            => SetDeleteBehavior(deleteBehavior, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DebugView DebugView
            => new DebugView(
                () => this.ToDebugString(MetadataDebugStringOptions.ShortDefault),
                () => this.ToDebugString(MetadataDebugStringOptions.LongDefault));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString()
            => this.ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        private void Validate(
            IReadOnlyList<Property> properties,
            Key principalKey,
            EntityType declaringEntityType,
            EntityType principalEntityType)
        {
            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                for (var j = i + 1; j < properties.Count; j++)
                {
                    if (property == properties[j])
                    {
                        throw new InvalidOperationException(CoreStrings.DuplicatePropertyInForeignKey(properties.Format(), property.Name));
                    }
                }

                var actualProperty = declaringEntityType.FindProperty(property.Name);
                if (actualProperty?.DeclaringEntityType.IsAssignableFrom(property.DeclaringEntityType) != true
                    || property.Builder == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ForeignKeyPropertiesWrongEntity(
                            properties.Format(), declaringEntityType.DisplayName()));
                }
            }

            AreCompatible(
                principalEntityType,
                dependentEntityType: declaringEntityType,
                navigationToPrincipal: null,
                navigationToDependent: null,
                dependentProperties: properties,
                principalProperties: principalKey.Properties,
                unique: null,
                shouldThrow: true);

            var duplicateForeignKey = declaringEntityType.FindForeignKeysInHierarchy(
                properties, principalKey, principalEntityType).FirstOrDefault();
            if (duplicateForeignKey != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.DuplicateForeignKey(
                        properties.Format(),
                        declaringEntityType.DisplayName(),
                        duplicateForeignKey.DeclaringEntityType.DisplayName(),
                        principalKey.Properties.Format(),
                        principalEntityType.DisplayName()));
            }

            if (principalEntityType.Model != declaringEntityType.Model)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeModelMismatch(
                        declaringEntityType.DisplayName(), principalEntityType.DisplayName()));
            }
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
            bool? unique,
            bool shouldThrow)
        {
            Check.NotNull(principalEntityType, nameof(principalEntityType));
            Check.NotNull(dependentEntityType, nameof(dependentEntityType));

            if (principalEntityType.HasDefiningNavigation()
                && principalEntityType == dependentEntityType)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ForeignKeySelfReferencingDependentEntityType(dependentEntityType.DisplayName()));
                }

                return false;
            }

            if (navigationToPrincipal != null
                && !Internal.Navigation.IsCompatible(
                    navigationToPrincipal,
                    dependentEntityType.ClrType,
                    principalEntityType.ClrType,
                    shouldBeCollection: false,
                    shouldThrow: shouldThrow))
            {
                return false;
            }

            if (navigationToDependent != null
                && !Internal.Navigation.IsCompatible(
                    navigationToDependent,
                    principalEntityType.ClrType,
                    dependentEntityType.ClrType,
                    shouldBeCollection: !unique,
                    shouldThrow: shouldThrow))
            {
                return false;
            }

            return principalProperties == null
                || dependentProperties == null
                || AreCompatible(
                    principalProperties,
                    dependentProperties,
                    principalEntityType,
                    dependentEntityType,
                    shouldThrow);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool AreCompatible(
            [NotNull] IReadOnlyList<IProperty> principalProperties,
            [NotNull] IReadOnlyList<IProperty> dependentProperties,
            [NotNull] IEntityType principalEntityType,
            [NotNull] IEntityType dependentEntityType,
            bool shouldThrow)
        {
            Check.NotNull(principalProperties, nameof(principalProperties));
            Check.NotNull(dependentProperties, nameof(dependentProperties));
            Check.NotNull(principalEntityType, nameof(principalEntityType));
            Check.NotNull(dependentEntityType, nameof(dependentEntityType));

            if (!ArePropertyCountsEqual(principalProperties, dependentProperties))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ForeignKeyCountMismatch(
                            dependentProperties.Format(),
                            dependentEntityType.DisplayName(),
                            principalProperties.Format(),
                            principalEntityType.DisplayName()));
                }

                return false;
            }

            if (!ArePropertyTypesCompatible(principalProperties, dependentProperties))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ForeignKeyTypeMismatch(
                            dependentProperties.Format(),
                            dependentEntityType.DisplayName(),
                            principalProperties.Format(),
                            principalEntityType.DisplayName()));
                }

                return false;
            }

            return true;
        }

        private static bool ArePropertyCountsEqual(
            IReadOnlyList<IProperty> principalProperties,
            IReadOnlyList<IProperty> dependentProperties)
            => principalProperties.Count == dependentProperties.Count;

        private static bool ArePropertyTypesCompatible(
            IReadOnlyList<IProperty> principalProperties,
            IReadOnlyList<IProperty> dependentProperties)
            => principalProperties.Select(p => p.ClrType.UnwrapNullableType()).SequenceEqual(
                dependentProperties.Select(p => p.ClrType.UnwrapNullableType()));
    }
}
