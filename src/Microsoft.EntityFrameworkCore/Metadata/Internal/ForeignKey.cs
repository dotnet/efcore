// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class ForeignKey
        : ConventionalAnnotatable, IMutableForeignKey, IDependentKeyValueFactorySource, IDependentsMapFactorySource
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

        public virtual IReadOnlyList<Property> Properties { get; }
        public virtual Key PrincipalKey { get; }
        public virtual EntityType DeclaringEntityType { get; }
        public virtual EntityType PrincipalEntityType { get; }

        public virtual InternalRelationshipBuilder Builder { get; [param: CanBeNull] set; }
        public virtual ConfigurationSource GetConfigurationSource() => _configurationSource;

        public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
        {
            _configurationSource = _configurationSource.Max(configurationSource);

            DeclaringEntityType.UpdateConfigurationSource(configurationSource);
            PrincipalEntityType.UpdateConfigurationSource(configurationSource);
        }

        public virtual ConfigurationSource? GetForeignKeyPropertiesConfigurationSource() => _foreignKeyPropertiesConfigurationSource;

        public virtual void UpdateForeignKeyPropertiesConfigurationSource(ConfigurationSource configurationSource)
        {
            _foreignKeyPropertiesConfigurationSource = configurationSource.Max(_foreignKeyPropertiesConfigurationSource);
            foreach (var property in Properties)
            {
                property.UpdateConfigurationSource(configurationSource);
            }
        }

        public virtual ConfigurationSource? GetPrincipalKeyConfigurationSource() => _principalKeyConfigurationSource;

        public virtual void UpdatePrincipalKeyConfigurationSource(ConfigurationSource configurationSource)
        {
            _principalKeyConfigurationSource = configurationSource.Max(_principalKeyConfigurationSource);
            PrincipalKey.UpdateConfigurationSource(configurationSource);
        }

        public virtual ConfigurationSource? GetPrincipalEndConfigurationSource() => _principalEndConfigurationSource;

        public virtual void SetPrincipalEndConfigurationSource(ConfigurationSource? configurationSource)
            => _principalEndConfigurationSource = configurationSource;

        public virtual void UpdatePrincipalEndConfigurationSource(ConfigurationSource configurationSource)
            => _principalEndConfigurationSource = configurationSource.Max(_principalEndConfigurationSource);

        public virtual Navigation DependentToPrincipal { get; private set; }

        public virtual Navigation HasDependentToPrincipal(
            [CanBeNull] string name,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            var oldNavigation = DependentToPrincipal;
            if (name == oldNavigation?.Name)
            {
                UpdateDependentToPrincipalConfigurationSource(configurationSource);
                return oldNavigation;
            }

            if (oldNavigation != null)
            {
                Debug.Assert(oldNavigation.Name != null);
                DeclaringEntityType.RemoveNavigation(oldNavigation.Name);
            }

            Navigation navigation = null;
            if (name != null)
            {
                navigation = DeclaringEntityType.AddNavigation(name, this, pointsToPrincipal: true);
            }

            DependentToPrincipal = navigation;
            UpdateDependentToPrincipalConfigurationSource(configurationSource);

            if (runConventions)
            {
                if (oldNavigation != null)
                {
                    Debug.Assert(oldNavigation.Name != null);
                    DeclaringEntityType.Model.ConventionDispatcher.OnNavigationRemoved(
                        DeclaringEntityType.Builder,
                        PrincipalEntityType.Builder,
                        oldNavigation.Name);
                }

                if (navigation != null)
                {
                    navigation = DeclaringEntityType.Model.ConventionDispatcher.OnNavigationAdded(Builder, navigation)
                        ?.Metadata.DependentToPrincipal;
                }
            }

            return navigation ?? oldNavigation;
        }

        public virtual ConfigurationSource? GetDependentToPrincipalConfigurationSource() => _dependentToPrincipalConfigurationSource;

        private void UpdateDependentToPrincipalConfigurationSource(ConfigurationSource configurationSource)
            => _dependentToPrincipalConfigurationSource = configurationSource.Max(_dependentToPrincipalConfigurationSource);

        public virtual Navigation PrincipalToDependent { get; private set; }

        public virtual Navigation HasPrincipalToDependent(
            [CanBeNull] string name,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            bool runConventions = true)
        {
            var oldNavigation = PrincipalToDependent;
            if (name == oldNavigation?.Name)
            {
                UpdatePrincipalToDependentConfigurationSource(configurationSource);
                return oldNavigation;
            }

            if (oldNavigation != null)
            {
                Debug.Assert(oldNavigation.Name != null);
                PrincipalEntityType.RemoveNavigation(oldNavigation.Name);
            }

            Navigation navigation = null;
            if (name != null)
            {
                navigation = PrincipalEntityType.AddNavigation(name, this, pointsToPrincipal: false);
            }

            PrincipalToDependent = navigation;
            UpdatePrincipalToDependentConfigurationSource(configurationSource);

            if (runConventions)
            {
                if (oldNavigation != null)
                {
                    Debug.Assert(oldNavigation.Name != null);
                    DeclaringEntityType.Model.ConventionDispatcher.OnNavigationRemoved(
                        PrincipalEntityType.Builder,
                        DeclaringEntityType.Builder,
                        oldNavigation.Name);
                }

                if (navigation != null)
                {
                    navigation = DeclaringEntityType.Model.ConventionDispatcher.OnNavigationAdded(Builder, navigation)
                        ?.Metadata.PrincipalToDependent;
                }
            }

            return navigation ?? oldNavigation;
        }

        public virtual ConfigurationSource? GetPrincipalToDependentConfigurationSource() => _principalToDependentConfigurationSource;

        private void UpdatePrincipalToDependentConfigurationSource(ConfigurationSource configurationSource)
            => _principalToDependentConfigurationSource = configurationSource.Max(_principalToDependentConfigurationSource);

        public virtual bool IsUnique
        {
            get { return _isUnique ?? DefaultIsUnique; }
            set { SetIsUnique(value, ConfigurationSource.Explicit); }
        }

        public virtual void SetIsUnique(bool unique, ConfigurationSource configurationSource)
        {
            _isUnique = unique;
            UpdateIsUniqueConfigurationSource(configurationSource);
        }

        private bool DefaultIsUnique => false;
        public virtual ConfigurationSource? GetIsUniqueConfigurationSource() => _isUniqueConfigurationSource;

        private void UpdateIsUniqueConfigurationSource(ConfigurationSource configurationSource)
            => _isUniqueConfigurationSource = configurationSource.Max(_isUniqueConfigurationSource);

        public virtual bool IsRequired
        {
            get { return !Properties.Any(p => p.IsNullable); }
            set { SetIsRequired(value, ConfigurationSource.Explicit); }
        }

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

        public virtual ConfigurationSource? GetIsRequiredConfigurationSource() => _isRequiredConfigurationSource;

        public virtual void SetIsRequiredConfigurationSource(ConfigurationSource? configurationSource)
            => _isRequiredConfigurationSource = configurationSource;

        public virtual void UpdateIsRequiredConfigurationSource(ConfigurationSource configurationSource)
            => _isRequiredConfigurationSource = configurationSource.Max(_isRequiredConfigurationSource);

        public virtual DeleteBehavior DeleteBehavior
        {
            get { return _deleteBehavior ?? DefaultDeleteBehavior; }
            set { SetDeleteBehavior(value, ConfigurationSource.Explicit); }
        }

        public virtual void SetDeleteBehavior(DeleteBehavior deleteBehavior, ConfigurationSource configurationSource)
        {
            _deleteBehavior = deleteBehavior;
            UpdateDeleteBehaviorConfigurationSource(configurationSource);
        }

        private DeleteBehavior DefaultDeleteBehavior => DeleteBehavior.Restrict;
        public virtual ConfigurationSource? GetDeleteBehaviorConfigurationSource() => _deleteBehaviorConfigurationSource;

        private void UpdateDeleteBehaviorConfigurationSource(ConfigurationSource configurationSource)
            => _deleteBehaviorConfigurationSource = configurationSource.Max(_deleteBehaviorConfigurationSource);

        public virtual IEnumerable<Navigation> FindNavigationsFrom([NotNull] EntityType entityType)
            => ((IForeignKey)this).FindNavigationsFrom(entityType).Cast<Navigation>();

        public virtual IEnumerable<Navigation> FindNavigationsFromInHierarchy([NotNull] EntityType entityType)
            => ((IForeignKey)this).FindNavigationsFromInHierarchy(entityType).Cast<Navigation>();

        public virtual IEnumerable<Navigation> FindNavigationsTo([NotNull] EntityType entityType)
            => ((IForeignKey)this).FindNavigationsTo(entityType).Cast<Navigation>();

        public virtual IEnumerable<Navigation> FindNavigationsToInHierarchy([NotNull] EntityType entityType)
            => ((IForeignKey)this).FindNavigationsToInHierarchy(entityType).Cast<Navigation>();

        public virtual EntityType ResolveOtherEntityTypeInHierarchy([NotNull] EntityType entityType)
            => (EntityType)((IForeignKey)this).ResolveOtherEntityTypeInHierarchy(entityType);

        public virtual EntityType ResolveOtherEntityType([NotNull] EntityType entityType)
            => (EntityType)((IForeignKey)this).ResolveOtherEntityType(entityType);

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

        INavigation IForeignKey.PrincipalToDependent => PrincipalToDependent;
        IMutableNavigation IMutableForeignKey.PrincipalToDependent => PrincipalToDependent;
        IMutableNavigation IMutableForeignKey.HasPrincipalToDependent(string name) => HasPrincipalToDependent(name);

        public override string ToString()
            => $"'{DeclaringEntityType.DisplayName()}' {Property.Format(Properties)} -> '{PrincipalEntityType.DisplayName()}' {Property.Format(PrincipalKey.Properties)}";

        public static bool AreCompatible(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            [CanBeNull] IReadOnlyList<Property> dependentProperties,
            [CanBeNull] IReadOnlyList<Property> principalProperties,
            bool? unique,
            bool? required,
            bool shouldThrow)
        {
            Check.NotNull(principalEntityType, nameof(principalEntityType));
            Check.NotNull(dependentEntityType, nameof(dependentEntityType));

            if (!string.IsNullOrEmpty(navigationToDependentName)
                && !Navigation.IsCompatible(
                    navigationToDependentName,
                    principalEntityType,
                    dependentEntityType,
                    !unique,
                    shouldThrow))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(navigationToPrincipalName)
                && !Navigation.IsCompatible(
                    navigationToPrincipalName,
                    dependentEntityType,
                    principalEntityType,
                    false,
                    shouldThrow))
            {
                return false;
            }

            if ((dependentProperties != null)
                && !CanPropertiesBeRequired(dependentProperties, required, dependentEntityType, true))
            {
                return false;
            }

            if ((principalProperties != null)
                && (dependentProperties != null)
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

        public virtual bool IsCompatible([NotNull] EntityType principalType, [NotNull] EntityType dependentType, bool? unique)
        {
            Check.NotNull(principalType, nameof(principalType));
            Check.NotNull(dependentType, nameof(dependentType));

            return ((unique == null) || (IsUnique == unique))
                   && (PrincipalEntityType == principalType)
                   && (DeclaringEntityType == dependentType);
        }

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
                            dependentEntityType.Name,
                            Property.Format(principalProperties),
                            principalEntityType.Name));
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
                            dependentEntityType.Name,
                            Property.Format(principalProperties),
                            principalEntityType.Name));
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
        public virtual object DependentKeyValueFactory { get; set; }

        // Note: This is set and used only by IdentityMapFactoryFactory, which ensures thread-safety
        public virtual Func<IDependentsMap> DependentsMapFactory { get; set; }
    }
}
