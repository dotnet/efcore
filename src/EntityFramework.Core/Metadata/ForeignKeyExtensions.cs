// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public static class ForeignKeyExtensions
    {
        public static bool IsSelfReferencing(
            [NotNull] this IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            return foreignKey.DeclaringEntityType == foreignKey.PrincipalEntityType;
        }

        public static bool IsCompatible(
            [NotNull] this ForeignKey foreignKey,
            [NotNull] EntityType principalType,
            [NotNull] EntityType dependentType,
            bool? isUnique)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(principalType, nameof(principalType));
            Check.NotNull(dependentType, nameof(dependentType));

            return (isUnique == null || ((IForeignKey)foreignKey).IsUnique == isUnique)
                   && foreignKey.PrincipalEntityType == principalType
                   && foreignKey.DeclaringEntityType == dependentType;
        }

        public static bool IsCompatible(
            [NotNull] this ForeignKey foreignKey,
            [NotNull] EntityType principalType,
            [NotNull] EntityType dependentType,
            [CanBeNull] IReadOnlyList<Property> foreignKeyProperties,
            [CanBeNull] IReadOnlyList<Property> principalProperties,
            bool? isUnique)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(principalType, nameof(principalType));
            Check.NotNull(dependentType, nameof(dependentType));

            return foreignKey.IsCompatible(principalType, dependentType, isUnique)
                   && (foreignKeyProperties == null
                       || !foreignKeyProperties.Any()
                       || foreignKey.Properties.SequenceEqual(foreignKeyProperties))
                   && (principalProperties == null
                       || !principalProperties.Any()
                       || foreignKey.PrincipalKey.Properties.SequenceEqual(principalProperties));
        }

        public static bool IsCompatible(
            [NotNull] this ForeignKey foreignKey,
            [NotNull] EntityType principalType,
            [NotNull] EntityType dependentType,
            [CanBeNull] string navigationToPrincipal,
            [CanBeNull] string navigationToDependent,
            [CanBeNull] IReadOnlyList<Property> foreignKeyProperties,
            [CanBeNull] IReadOnlyList<Property> principalProperties,
            bool? isUnique)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(principalType, nameof(principalType));
            Check.NotNull(dependentType, nameof(dependentType));

            var existingNavigationToPrincipal = foreignKey.DependentToPrincipal;
            var existingNavigationToDependent = foreignKey.PrincipalToDependent;
            return foreignKey.IsCompatible(principalType, dependentType, foreignKeyProperties, principalProperties, isUnique)
                   && (existingNavigationToPrincipal == null || existingNavigationToPrincipal.Name == navigationToPrincipal)
                   && (existingNavigationToDependent == null || existingNavigationToDependent.Name == navigationToDependent);
        }

        public static INavigation FindNavigationFrom(
            [NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(entityType, nameof(entityType));

            if (foreignKey.DeclaringEntityType != entityType
                && foreignKey.PrincipalEntityType != entityType)
            {
                throw new ArgumentException(Strings.EntityTypeNotInRelationship(
                    entityType.Name, foreignKey.DeclaringEntityType.Name, foreignKey.PrincipalEntityType.Name), nameof(entityType));
            }

            if (foreignKey.IsSelfReferencing())
            {
                throw new InvalidOperationException(Strings.SelfRefAmbiguousNavigation(entityType.Name, Property.Format(foreignKey.Properties)));
            }

            return foreignKey.DeclaringEntityType == entityType
                ? foreignKey.DependentToPrincipal
                : foreignKey.PrincipalToDependent;
        }

        public static Navigation FindNavigationFrom(
            [NotNull] this ForeignKey foreignKey, [NotNull] EntityType entityType)
        {
            return (Navigation)((IForeignKey)foreignKey).FindNavigationFrom(entityType);
        }

        public static INavigation FindNavigationTo(
            [NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(entityType, nameof(entityType));

            if (foreignKey.DeclaringEntityType != entityType
                && foreignKey.PrincipalEntityType != entityType)
            {
                throw new ArgumentException(Strings.EntityTypeNotInRelationship(
                    entityType.Name, foreignKey.DeclaringEntityType.Name, foreignKey.PrincipalEntityType.Name), nameof(entityType));
            }

            if (foreignKey.IsSelfReferencing())
            {
                throw new InvalidOperationException(Strings.SelfRefAmbiguousNavigation(entityType.Name, Property.Format(foreignKey.Properties)));
            }

            return foreignKey.DeclaringEntityType == entityType
                ? foreignKey.PrincipalToDependent
                : foreignKey.DependentToPrincipal;
        }

        public static Navigation FindNavigationTo(
            [NotNull] this ForeignKey foreignKey, [NotNull] EntityType entityType)
        {
            return (Navigation)((IForeignKey)foreignKey).FindNavigationTo(entityType);
        }

        public static IEntityType GetOtherEntityType(
            [NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(entityType, nameof(entityType));

            if (foreignKey.DeclaringEntityType != entityType
                && foreignKey.PrincipalEntityType != entityType)
            {
                throw new ArgumentException(Strings.EntityTypeNotInRelationship(
                    entityType.Name, foreignKey.DeclaringEntityType.Name, foreignKey.PrincipalEntityType.Name), nameof(entityType));
            }

            return foreignKey.DeclaringEntityType == entityType
                ? foreignKey.PrincipalEntityType
                : foreignKey.DeclaringEntityType;
        }

        public static EntityType GetOtherEntityType(
            [NotNull] this ForeignKey foreignKey, [NotNull] EntityType entityType)
        {
            return (EntityType)((IForeignKey)foreignKey).GetOtherEntityType(entityType);
        }
    }
}
