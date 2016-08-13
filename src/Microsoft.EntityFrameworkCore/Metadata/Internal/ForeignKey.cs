// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ForeignKey : ConventionalAnnotatable, IMutableForeignKey
    {
        private DeleteBehavior? _deleteBehavior;
        private bool? _isUnique;

        private ConfigurationSource _configurationSource;
        private ConfigurationSource? _foreignKeyPropertiesConfigurationSource;
        private ConfigurationSource? _principalKeyConfigurationSource;
        private ConfigurationSource? _isUniqueConfigurationSource;
        private ConfigurationSource? _isRequiredConfigurationSource;
        private ConfigurationSource? _deleteBehaviorConfigurationSource;
        private ConfigurationSource? _principalEndConfigurationSource;
        private ConfigurationSource? _dependentToPrincipalConfigurationSource;
        private ConfigurationSource? _principalToDependentConfigurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
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

            Properties = dependentProperties;
            PrincipalKey = principalKey;
            DeclaringEntityType = dependentEntityType;
            PrincipalEntityType = principalEntityType;
            _configurationSource = configurationSource;

            AreCompatible(principalKey.Properties, dependentProperties, principalEntityType, dependentEntityType, shouldThrow: true);

            if (!principalEntityType.GetKeys().Contains(principalKey))
            {
                throw new InvalidOperationException(
                    CoreStrings.ForeignKeyReferencedEntityKeyMismatch(
                        Property.Format(principalKey.Properties),
                        principalEntityType));
            }

            Builder = new InternalRelationshipBuilder(this, dependentEntityType.Model.Builder);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<Property> Properties { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key PrincipalKey { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType DeclaringEntityType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType PrincipalEntityType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Builder { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource GetConfigurationSource() => _configurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
        {
            _configurationSource = _configurationSource.Max(configurationSource);

            DeclaringEntityType.UpdateConfigurationSource(configurationSource);
            PrincipalEntityType.UpdateConfigurationSource(configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetForeignKeyPropertiesConfigurationSource() => _foreignKeyPropertiesConfigurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void UpdateForeignKeyPropertiesConfigurationSource(ConfigurationSource configurationSource)
        {
            _foreignKeyPropertiesConfigurationSource = configurationSource.Max(_foreignKeyPropertiesConfigurationSource);
            foreach (var property in Properties)
            {
                property.UpdateConfigurationSource(configurationSource);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetPrincipalKeyConfigurationSource() => _principalKeyConfigurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void UpdatePrincipalKeyConfigurationSource(ConfigurationSource configurationSource)
        {
            _principalKeyConfigurationSource = configurationSource.Max(_principalKeyConfigurationSource);
            PrincipalKey.UpdateConfigurationSource(configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetPrincipalEndConfigurationSource() => _principalEndConfigurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetPrincipalEndConfigurationSource(ConfigurationSource? configurationSource)
            => _principalEndConfigurationSource = configurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void UpdatePrincipalEndConfigurationSource(ConfigurationSource configurationSource)
            => _principalEndConfigurationSource = configurationSource.Max(_principalEndConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Navigation DependentToPrincipal { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Navigation HasDependentToPrincipal(
            [CanBeNull] string name,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
            => Navigation(PropertyIdentity.Create(name), configurationSource, runConventions, pointsToPrincipal: true);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Navigation HasDependentToPrincipal(
            [CanBeNull] PropertyInfo property,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
            => Navigation(PropertyIdentity.Create(property), configurationSource, runConventions, pointsToPrincipal: true);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetDependentToPrincipalConfigurationSource() => _dependentToPrincipalConfigurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void UpdateDependentToPrincipalConfigurationSource(ConfigurationSource? configurationSource)
            => _dependentToPrincipalConfigurationSource = configurationSource.Max(_dependentToPrincipalConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Navigation PrincipalToDependent { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Navigation HasPrincipalToDependent(
            [CanBeNull] string name,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
            => Navigation(PropertyIdentity.Create(name), configurationSource, runConventions, pointsToPrincipal: false);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Navigation HasPrincipalToDependent(
            [CanBeNull] PropertyInfo property,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
            => Navigation(PropertyIdentity.Create(property), configurationSource, runConventions, pointsToPrincipal: false);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetPrincipalToDependentConfigurationSource() => _principalToDependentConfigurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void UpdatePrincipalToDependentConfigurationSource(ConfigurationSource? configurationSource)
            => _principalToDependentConfigurationSource = configurationSource.Max(_principalToDependentConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        private Navigation Navigation(
            PropertyIdentity? propertyIdentity,
            ConfigurationSource configurationSource,
            bool runConventions,
            bool pointsToPrincipal)
        {
            var name = propertyIdentity?.Name;
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
                Debug.Assert(oldNavigation.Name != null);
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
            var property = propertyIdentity?.Property;
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

            if (runConventions)
            {
                if (oldNavigation != null)
                {
                    Debug.Assert(oldNavigation.Name != null);

                    if (pointsToPrincipal)
                    {
                        DeclaringEntityType.Model.ConventionDispatcher.OnNavigationRemoved(
                            DeclaringEntityType.Builder,
                            PrincipalEntityType.Builder,
                            oldNavigation.Name,
                            oldNavigation.PropertyInfo);
                    }
                    else
                    {
                        DeclaringEntityType.Model.ConventionDispatcher.OnNavigationRemoved(
                            PrincipalEntityType.Builder,
                            DeclaringEntityType.Builder,
                            oldNavigation.Name,
                            oldNavigation.PropertyInfo);
                    }
                }

                if (navigation != null)
                {
                    var builder = DeclaringEntityType.Model.ConventionDispatcher.OnNavigationAdded(Builder, navigation);
                    navigation = pointsToPrincipal ? builder?.Metadata.DependentToPrincipal : builder?.Metadata.PrincipalToDependent;
                }
            }

            return navigation ?? oldNavigation;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsUnique
        {
            get { return _isUnique ?? DefaultIsUnique; }
            set { SetIsUnique(value, ConfigurationSource.Explicit); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetIsUnique(bool unique, ConfigurationSource configurationSource)
        {
            var isChanging = IsUnique != unique;
            _isUnique = unique;
            UpdateIsUniqueConfigurationSource(configurationSource);

            if (isChanging)
            {
                DeclaringEntityType.Model.ConventionDispatcher.OnForeignKeyUniquenessChanged(Builder);
            }
        }

        private bool DefaultIsUnique => false;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetIsUniqueConfigurationSource() => _isUniqueConfigurationSource;

        private void UpdateIsUniqueConfigurationSource(ConfigurationSource configurationSource)
            => _isUniqueConfigurationSource = configurationSource.Max(_isUniqueConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsRequired
        {
            get { return !Properties.Any(p => p.IsNullable); }
            set { SetIsRequired(value, ConfigurationSource.Explicit); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetIsRequired(bool required, ConfigurationSource configurationSource)
        {
            if (required == IsRequired)
            {
                UpdateIsRequiredConfigurationSource(configurationSource);
                return;
            }

            var properties = Properties;
            if (!required)
            {
                var nullableTypeProperties = Properties.Where(p => p.ClrType.IsNullableType()).ToList();
                if (nullableTypeProperties.Any())
                {
                    properties = nullableTypeProperties;
                }
                // If no properties can be made nullable, let it fail
            }

            foreach (var property in properties)
            {
                property.SetIsNullable(!required, configurationSource);
            }

            UpdateIsRequiredConfigurationSource(configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetIsRequiredConfigurationSource() => _isRequiredConfigurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetIsRequiredConfigurationSource(ConfigurationSource? configurationSource)
            => _isRequiredConfigurationSource = configurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void UpdateIsRequiredConfigurationSource(ConfigurationSource configurationSource)
            => _isRequiredConfigurationSource = configurationSource.Max(_isRequiredConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DeleteBehavior DeleteBehavior
        {
            get { return _deleteBehavior ?? DefaultDeleteBehavior; }
            set { SetDeleteBehavior(value, ConfigurationSource.Explicit); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetDeleteBehavior(DeleteBehavior deleteBehavior, ConfigurationSource configurationSource)
        {
            _deleteBehavior = deleteBehavior;
            UpdateDeleteBehaviorConfigurationSource(configurationSource);
        }

        private DeleteBehavior DefaultDeleteBehavior => DeleteBehavior.Restrict;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetDeleteBehaviorConfigurationSource() => _deleteBehaviorConfigurationSource;

        private void UpdateDeleteBehaviorConfigurationSource(ConfigurationSource configurationSource)
            => _deleteBehaviorConfigurationSource = configurationSource.Max(_deleteBehaviorConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Navigation> FindNavigationsFrom([NotNull] EntityType entityType)
            => ((IForeignKey)this).FindNavigationsFrom(entityType).Cast<Navigation>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Navigation> FindNavigationsFromInHierarchy([NotNull] EntityType entityType)
            => ((IForeignKey)this).FindNavigationsFromInHierarchy(entityType).Cast<Navigation>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Navigation> FindNavigationsTo([NotNull] EntityType entityType)
            => ((IForeignKey)this).FindNavigationsTo(entityType).Cast<Navigation>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Navigation> FindNavigationsToInHierarchy([NotNull] EntityType entityType)
            => ((IForeignKey)this).FindNavigationsToInHierarchy(entityType).Cast<Navigation>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType ResolveOtherEntityTypeInHierarchy([NotNull] EntityType entityType)
            => (EntityType)((IForeignKey)this).ResolveOtherEntityTypeInHierarchy(entityType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType ResolveOtherEntityType([NotNull] EntityType entityType)
            => (EntityType)((IForeignKey)this).ResolveOtherEntityType(entityType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType ResolveEntityTypeInHierarchy([NotNull] EntityType entityType)
            => (EntityType)((IForeignKey)this).ResolveEntityTypeInHierarchy(entityType);

        IReadOnlyList<IProperty> IForeignKey.Properties => Properties;
        IReadOnlyList<IMutableProperty> IMutableForeignKey.Properties => Properties;
        IKey IForeignKey.PrincipalKey => PrincipalKey;
        IMutableKey IMutableForeignKey.PrincipalKey => PrincipalKey;
        IEntityType IForeignKey.DeclaringEntityType => DeclaringEntityType;
        IMutableEntityType IMutableForeignKey.DeclaringEntityType => DeclaringEntityType;
        IEntityType IForeignKey.PrincipalEntityType => PrincipalEntityType;
        IMutableEntityType IMutableForeignKey.PrincipalEntityType => PrincipalEntityType;

        INavigation IForeignKey.DependentToPrincipal => DependentToPrincipal;
        IMutableNavigation IMutableForeignKey.DependentToPrincipal => DependentToPrincipal;
        IMutableNavigation IMutableForeignKey.HasDependentToPrincipal(string name) => HasDependentToPrincipal(name);
        IMutableNavigation IMutableForeignKey.HasDependentToPrincipal(PropertyInfo property) => HasDependentToPrincipal(property);

        INavigation IForeignKey.PrincipalToDependent => PrincipalToDependent;
        IMutableNavigation IMutableForeignKey.PrincipalToDependent => PrincipalToDependent;
        IMutableNavigation IMutableForeignKey.HasPrincipalToDependent(string name) => HasPrincipalToDependent(name);
        IMutableNavigation IMutableForeignKey.HasPrincipalToDependent(PropertyInfo property) => HasPrincipalToDependent(property);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string ToString()
            // Interpolation okay; strings/debug output
            => $"'{DeclaringEntityType.DisplayName()}' {Property.Format(Properties)} -> '{PrincipalEntityType.DisplayName()}' {Property.Format(PrincipalKey.Properties)}";

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
            bool? unique,
            bool? required,
            bool shouldThrow)
        {
            Check.NotNull(principalEntityType, nameof(principalEntityType));
            Check.NotNull(dependentEntityType, nameof(dependentEntityType));

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

            if (dependentProperties != null
                && !CanPropertiesBeRequired(dependentProperties, required, dependentEntityType, true))
            {
                return false;
            }

            if (principalProperties != null
                && dependentProperties != null
                && !AreCompatible(
                    principalProperties,
                    dependentProperties,
                    principalEntityType,
                    dependentEntityType,
                    shouldThrow))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool CanPropertiesBeRequired(
            [NotNull] IReadOnlyList<Property> properties,
            bool? required,
            [NotNull] EntityType entityType,
            bool shouldThrow)
        {
            Check.NotNull(properties, nameof(properties));
            Check.NotNull(entityType, nameof(entityType));

            if (!required.HasValue
                || required.Value)
            {
                return true;
            }

            var nullableProperties = properties.Where(p => p.ClrType.IsNullableType()).ToList();
            if (!nullableProperties.Any())
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(CoreStrings.ForeignKeyCannotBeOptional(
                        Property.Format(properties), entityType.DisplayName()));
                }
                return false;
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool AreCompatible(
            [NotNull] IReadOnlyList<Property> principalProperties,
            [NotNull] IReadOnlyList<Property> dependentProperties,
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
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
                            Property.Format(dependentProperties),
                            dependentEntityType.DisplayName(),
                            Property.Format(principalProperties),
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
                            Property.Format(dependentProperties),
                            dependentEntityType.DisplayName(),
                            Property.Format(principalProperties),
                            principalEntityType.DisplayName()));
                }
                return false;
            }

            return true;
        }

        private static bool ArePropertyCountsEqual(IReadOnlyList<IProperty> principalProperties, IReadOnlyList<IProperty> dependentProperties)
            => principalProperties.Count == dependentProperties.Count;

        private static bool ArePropertyTypesCompatible(IReadOnlyList<IProperty> principalProperties, IReadOnlyList<IProperty> dependentProperties)
            => principalProperties.Select(p => p.ClrType.UnwrapNullableType()).SequenceEqual(
                dependentProperties.Select(p => p.ClrType.UnwrapNullableType()));

        // Note: This is set and used only by IdentityMapFactoryFactory, which ensures thread-safety
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object DependentKeyValueFactory { get; [param: NotNull] set; }

        // Note: This is set and used only by IdentityMapFactoryFactory, which ensures thread-safety
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<IDependentsMap> DependentsMapFactory { get; [param: NotNull] set; }
    }
}
