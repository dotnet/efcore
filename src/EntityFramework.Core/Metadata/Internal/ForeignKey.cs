// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class ForeignKey : ConventionalAnnotatable, IMutableForeignKey
    {
        private DeleteBehavior? _deleteBehavior;
        private ConfigurationSource _configurationSource;

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
                throw new ArgumentException(
                    CoreStrings.ForeignKeyReferencedEntityKeyMismatch(
                        Property.Format(principalKey.Properties),
                        principalEntityType));
            }
        }

        public virtual InternalRelationshipBuilder Builder { get; [param: CanBeNull] set; }

        public virtual ConfigurationSource GetConfigurationSource() => _configurationSource;

        public virtual ConfigurationSource UpdateConfigurationSource(ConfigurationSource configurationSource)
            => _configurationSource = _configurationSource.Max(configurationSource);

        public virtual Navigation DependentToPrincipal { get; private set; }

        public virtual Navigation HasDependentToPrincipal([CanBeNull] string name)
        {
            var oldNavigation = DependentToPrincipal;
            if (oldNavigation != null)
            {
                DeclaringEntityType.RemoveNavigation(oldNavigation.Name);
            }

            Navigation newNavigation = null;
            if (name != null)
            {
                newNavigation = DeclaringEntityType.AddNavigation(name, this, pointsToPrincipal: true);
            }

            DependentToPrincipal = newNavigation;
            return newNavigation ?? oldNavigation;
        }

        public virtual Navigation PrincipalToDependent { get; private set; }

        public virtual Navigation HasPrincipalToDependent([CanBeNull] string name)
        {
            var oldNavigation = PrincipalToDependent;
            if (oldNavigation != null)
            {
                PrincipalEntityType.RemoveNavigation(oldNavigation.Name);
            }

            Navigation navigation = null;
            if (name != null)
            {
                navigation = PrincipalEntityType.AddNavigation(name, this, pointsToPrincipal: false);
            }

            PrincipalToDependent = navigation;
            return navigation ?? oldNavigation;
        }

        public virtual IReadOnlyList<Property> Properties { get; }
        public virtual Key PrincipalKey { get; }
        public virtual EntityType DeclaringEntityType { get; }
        public virtual EntityType PrincipalEntityType { get; }

        public virtual bool? IsUnique { get; set; }
        protected virtual bool DefaultIsUnique => false;

        public virtual bool? IsRequired
        {
            get
            {
                return Properties.Any(p => p.GetIsNullableConfigurationSource().HasValue)
                    ? !Properties.Any(p => p.IsNullable) as bool?
                    : null;
            }
            set
            {
                if (value == IsRequired)
                {
                    return;
                }

                var properties = Properties;
                if (value.HasValue
                    && !value.Value)
                {
                    var nullableTypeProperties = Properties.Where(p => ((IProperty)p).ClrType.IsNullableType()).ToList();
                    if (nullableTypeProperties.Any())
                    {
                        properties = nullableTypeProperties;
                    }
                    // If no properties can be made nullable, let it fail
                }

                if (value.HasValue)
                {
                    foreach (var property in properties)
                    {
                        property.IsNullable = !value.Value;
                    }
                }
            }
        }

        protected virtual bool DefaultIsRequired => !((IForeignKey)this).Properties.Any(p => p.IsNullable);

        public virtual DeleteBehavior? DeleteBehavior
        {
            get { return _deleteBehavior; }
            set
            {
                if (value != null)
                {
                    Check.IsDefined(value.Value, nameof(value));
                }
                _deleteBehavior = value;
            }
        }

        protected virtual DeleteBehavior DefaultDeleteBehavior => Metadata.DeleteBehavior.Restrict;

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

        bool IForeignKey.IsUnique => IsUnique ?? DefaultIsUnique;
        bool IForeignKey.IsRequired => IsRequired ?? DefaultIsRequired;
        DeleteBehavior IForeignKey.DeleteBehavior => DeleteBehavior ?? DefaultDeleteBehavior;

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
            [NotNull] IEnumerable<Property> properties,
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
    }
}
