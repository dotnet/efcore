// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public static class ForeignKeyExtensions
    {
        public static bool IsCompatible(
            [NotNull] this ForeignKey foreignKey,
            [NotNull] EntityType principalType,
            [NotNull] EntityType dependentType,
            bool? unique)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(principalType, nameof(principalType));
            Check.NotNull(dependentType, nameof(dependentType));

            return (unique == null || ((IForeignKey)foreignKey).IsUnique == unique)
                   && foreignKey.PrincipalEntityType == principalType
                   && foreignKey.DeclaringEntityType == dependentType;
        }

        public static bool IsSelfReferencing([NotNull] this IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            return foreignKey.DeclaringEntityType == foreignKey.PrincipalEntityType;
        }

        public static bool IsIntraHierarchical([NotNull] this IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            return foreignKey.DeclaringEntityType.IsSameHierarchy(foreignKey.PrincipalEntityType);
        }

        public static bool IsSelfPrimaryKeyReferencing([NotNull] this IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            return foreignKey.DeclaringEntityType.FindPrimaryKey() == foreignKey.PrincipalKey;
        }

        public static IEnumerable<INavigation> GetNavigations(
            [NotNull] this IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            if (foreignKey.PrincipalToDependent != null)
            {
                yield return foreignKey.PrincipalToDependent;
            }

            if (foreignKey.DependentToPrincipal != null)
            {
                yield return foreignKey.DependentToPrincipal;
            }
        }

        public static INavigation FindNavigationFrom([NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(entityType, nameof(entityType));

            if (foreignKey.DeclaringEntityType != entityType
                && foreignKey.PrincipalEntityType != entityType)
            {
                throw new ArgumentException(CoreStrings.EntityTypeNotInRelationshipStrict(
                    entityType.DisplayName(), foreignKey.DeclaringEntityType.DisplayName(), foreignKey.PrincipalEntityType.DisplayName()));
            }

            if (foreignKey.IsSelfReferencing())
            {
                throw new InvalidOperationException(CoreStrings.SelfReferencingAmbiguousNavigation(
                    entityType.DisplayName(), Property.Format(foreignKey.Properties)));
            }

            return foreignKey.DeclaringEntityType == entityType
                ? foreignKey.DependentToPrincipal
                : foreignKey.PrincipalToDependent;
        }

        public static Navigation FindNavigationFrom([NotNull] this ForeignKey foreignKey, [NotNull] EntityType entityType)
            => (Navigation)((IForeignKey)foreignKey).FindNavigationFrom(entityType);

        public static INavigation FindNavigationFromInHierarchy([NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(entityType, nameof(entityType));

            if (!foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                && !foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
            {
                throw new ArgumentException(CoreStrings.EntityTypeNotInRelationship(
                    entityType.DisplayName(), foreignKey.DeclaringEntityType.DisplayName(), foreignKey.PrincipalEntityType.DisplayName()));
            }

            if (foreignKey.IsIntraHierarchical())
            {
                throw new InvalidOperationException(CoreStrings.IntraHierarchicalAmbiguousNavigation(
                    entityType.DisplayName(),
                    Property.Format(foreignKey.Properties),
                    foreignKey.PrincipalEntityType.DisplayName(),
                    foreignKey.DeclaringEntityType.DisplayName()));
            }

            return foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                ? foreignKey.DependentToPrincipal
                : foreignKey.PrincipalToDependent;
        }

        public static Navigation FindNavigationFromInHierarchy([NotNull] this ForeignKey foreignKey, [NotNull] EntityType entityType)
            => (Navigation)((IForeignKey)foreignKey).FindNavigationFromInHierarchy(entityType);

        public static INavigation FindNavigationTo([NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(entityType, nameof(entityType));

            if (foreignKey.DeclaringEntityType != entityType
                && foreignKey.PrincipalEntityType != entityType)
            {
                throw new ArgumentException(CoreStrings.EntityTypeNotInRelationshipStrict(
                    entityType.DisplayName(),
                    foreignKey.DeclaringEntityType.DisplayName(),
                    foreignKey.PrincipalEntityType.DisplayName()));
            }

            if (foreignKey.IsSelfReferencing())
            {
                throw new InvalidOperationException(CoreStrings.SelfReferencingAmbiguousNavigation(
                    entityType.DisplayName(), Property.Format(foreignKey.Properties)));
            }

            return foreignKey.DeclaringEntityType == entityType
                ? foreignKey.PrincipalToDependent
                : foreignKey.DependentToPrincipal;
        }

        public static Navigation FindNavigationTo([NotNull] this ForeignKey foreignKey, [NotNull] EntityType entityType)
            => (Navigation)((IForeignKey)foreignKey).FindNavigationTo(entityType);

        public static INavigation FindNavigationToInHierarchy([NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(entityType, nameof(entityType));

            if (!foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                && !foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
            {
                throw new ArgumentException(CoreStrings.EntityTypeNotInRelationship(
                    entityType.DisplayName(), foreignKey.DeclaringEntityType.DisplayName(), foreignKey.PrincipalEntityType.DisplayName()));
            }

            if (foreignKey.IsIntraHierarchical())
            {
                throw new InvalidOperationException(CoreStrings.IntraHierarchicalAmbiguousNavigation(
                    entityType.DisplayName(),
                    Property.Format(foreignKey.Properties),
                    foreignKey.PrincipalEntityType.DisplayName(),
                    foreignKey.DeclaringEntityType.DisplayName()));
            }

            return foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                ? foreignKey.PrincipalToDependent
                : foreignKey.DependentToPrincipal;
        }

        public static Navigation FindNavigationToInHierarchy([NotNull] this ForeignKey foreignKey, [NotNull] EntityType entityType)
            => (Navigation)((IForeignKey)foreignKey).FindNavigationToInHierarchy(entityType);

        public static IEntityType ResolveOtherEntityType([NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(entityType, nameof(entityType));

            if (foreignKey.DeclaringEntityType != entityType
                && foreignKey.PrincipalEntityType != entityType)
            {
                throw new ArgumentException(CoreStrings.EntityTypeNotInRelationshipStrict(
                    entityType.DisplayName(),
                    foreignKey.DeclaringEntityType.DisplayName(),
                    foreignKey.PrincipalEntityType.DisplayName()));
            }

            return foreignKey.DeclaringEntityType == entityType
                ? foreignKey.PrincipalEntityType
                : foreignKey.DeclaringEntityType;
        }

        public static EntityType ResolveOtherEntityType([NotNull] this ForeignKey foreignKey, [NotNull] EntityType entityType)
            => (EntityType)((IForeignKey)foreignKey).ResolveOtherEntityType(entityType);

        public static IEntityType ResolveOtherEntityTypeInHierarchy(
            [NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(entityType, nameof(entityType));

            if (!foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                && !foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
            {
                throw new ArgumentException(CoreStrings.EntityTypeNotInRelationship(
                    entityType.DisplayName(),
                    foreignKey.DeclaringEntityType.DisplayName(),
                    foreignKey.PrincipalEntityType.DisplayName()));
            }

            if (foreignKey.IsIntraHierarchical())
            {
                throw new InvalidOperationException(CoreStrings.IntraHierarchicalAmbiguousTargetEntityType(
                    entityType.DisplayName(),
                    Property.Format(foreignKey.Properties),
                    foreignKey.PrincipalEntityType.DisplayName(),
                    foreignKey.DeclaringEntityType.DisplayName()));
            }

            return foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                ? foreignKey.PrincipalEntityType
                : foreignKey.DeclaringEntityType;
        }

        public static EntityType ResolveOtherEntityTypeInHierarchy([NotNull] this ForeignKey foreignKey, [NotNull] EntityType entityType)
            => (EntityType)((IForeignKey)foreignKey).ResolveOtherEntityTypeInHierarchy(entityType);

        public static IEntityType ResolveEntityTypeInHierarchy([NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(entityType, nameof(entityType));

            if (!foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                && !foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
            {
                throw new ArgumentException(CoreStrings.EntityTypeNotInRelationship(
                    entityType.DisplayName(),
                    foreignKey.DeclaringEntityType.DisplayName(),
                    foreignKey.PrincipalEntityType.DisplayName()));
            }

            if (foreignKey.IsIntraHierarchical())
            {
                throw new InvalidOperationException(CoreStrings.IntraHierarchicalAmbiguousTargetEntityType(
                    entityType.DisplayName(), Property.Format(foreignKey.Properties),
                    foreignKey.PrincipalEntityType.DisplayName(),
                    foreignKey.DeclaringEntityType.DisplayName()));
            }

            return foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                ? foreignKey.DeclaringEntityType
                : foreignKey.PrincipalEntityType;
        }

        public static EntityType ResolveEntityTypeInHierarchy([NotNull] this ForeignKey foreignKey, [NotNull] EntityType entityType)
            => (EntityType)((IForeignKey)foreignKey).ResolveEntityTypeInHierarchy(entityType);
    }
}
