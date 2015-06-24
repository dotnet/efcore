// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ForeignKey : Annotatable, IForeignKey
    {
        private Navigation _dependentToPrincipal;
        private Navigation _principalToDependent;
        private DeleteBehavior? _deleteBehavior;

        public ForeignKey(
            [NotNull] IReadOnlyList<Property> dependentProperties,
            [NotNull] Key principalKey,
            [NotNull] EntityType dependentEntityType,
            [NotNull] EntityType principalEntityType)
        {
            Check.NotEmpty(dependentProperties, nameof(dependentProperties));
            Check.HasNoNulls(dependentProperties, nameof(dependentProperties));
            MetadataHelper.CheckSameEntityType(dependentProperties, nameof(dependentProperties));
            Check.NotNull(principalKey, nameof(principalKey));
            Check.NotNull(principalEntityType, nameof(principalEntityType));

            Properties = dependentProperties;

            PrincipalKey = principalKey;

            DeclaringEntityType = dependentEntityType;
            PrincipalEntityType = principalEntityType;

            AreCompatible(principalKey.Properties, dependentProperties, principalEntityType, dependentEntityType, shouldThrow: true);

            if (!principalEntityType.GetKeys().Contains(principalKey))
            {
                throw new ArgumentException(
                    Strings.ForeignKeyReferencedEntityKeyMismatch(
                        Property.Format(principalKey.Properties),
                        principalEntityType));
            }
        }

        public virtual Navigation DependentToPrincipal
        {
            get { return _dependentToPrincipal; }
            [param: CanBeNull]
            set
            {
                CheckNavigation(value, DeclaringEntityType);

                if (value == null
                    && _dependentToPrincipal != null
                    && DeclaringEntityType.Navigations.Contains(_dependentToPrincipal))
                {
                    throw new InvalidOperationException(
                        Strings.NavigationStillOnEntityType(_dependentToPrincipal.Name, DeclaringEntityType.Name));
                }

                _dependentToPrincipal = value;
            }
        }

        public virtual Navigation PrincipalToDependent
        {
            get { return _principalToDependent; }
            [param: CanBeNull]
            set
            {
                CheckNavigation(value, PrincipalEntityType);

                if (value == null
                    && _principalToDependent != null
                    && PrincipalEntityType.Navigations.Contains(_principalToDependent))
                {
                    throw new InvalidOperationException(
                        Strings.NavigationStillOnEntityType(_principalToDependent.Name, PrincipalEntityType.Name));
                }

                _principalToDependent = value;
            }
        }

        private void CheckNavigation(Navigation value, EntityType entityType)
        {
            if (value != null)
            {
                if (value.ForeignKey != this)
                {
                    throw new InvalidOperationException(
                        Strings.NavigationForWrongForeignKey(value.Name, value.DeclaringEntityType.DisplayName(), Property.Format(Properties), Property.Format(value.ForeignKey.Properties)));
                }

                if (!entityType.Navigations.Contains(value))
                {
                    throw new InvalidOperationException(Strings.NavigationNotFound(value.Name, entityType.Name));
                }
            }
        }

        public virtual IReadOnlyList<Property> Properties { get; }

        public virtual EntityType DeclaringEntityType { get; }

        public virtual Key PrincipalKey { get; }

        public virtual EntityType PrincipalEntityType { get; }

        public virtual bool? IsUnique { get; set; }

        protected virtual bool DefaultIsUnique => false;

        public virtual bool? IsRequired
        {
            get
            {
                return Properties.Any(p => p.IsNullable.HasValue)
                    ? !Properties.Any(p => ((IProperty)p).IsNullable) as bool?
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

                foreach (var property in properties)
                {
                    property.IsNullable = !value;
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

        protected virtual DeleteBehavior DefaultDeleteBehavior => Metadata.DeleteBehavior.None;

        IReadOnlyList<IProperty> IForeignKey.Properties => Properties;

        IEntityType IForeignKey.DeclaringEntityType => DeclaringEntityType;

        IEntityType IForeignKey.PrincipalEntityType => PrincipalEntityType;

        IKey IForeignKey.PrincipalKey => PrincipalKey;

        INavigation IForeignKey.DependentToPrincipal => DependentToPrincipal;

        INavigation IForeignKey.PrincipalToDependent => PrincipalToDependent;

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

            var nullableProperties = properties.Where(p => ((IProperty)p).ClrType.IsNullableType()).ToList();
            if (!nullableProperties.Any())
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(Strings.ForeignKeyCannotBeOptional(
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
                        Strings.ForeignKeyCountMismatch(
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
                        Strings.ForeignKeyTypeMismatch(
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
