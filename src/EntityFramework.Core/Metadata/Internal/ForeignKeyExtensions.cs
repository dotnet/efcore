// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public static class ForeignKeyExtensions
    {
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

        public static IEnumerable<INavigation> GetNavigations([NotNull] this IForeignKey foreignKey)
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

        public static IEnumerable<INavigation> FindNavigationsFrom(
            [NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(entityType, nameof(entityType));

            if ((foreignKey.DeclaringEntityType != entityType)
                && (foreignKey.PrincipalEntityType != entityType))
            {
                throw new ArgumentException(CoreStrings.EntityTypeNotInRelationshipStrict(
                    entityType.DisplayName(),
                    foreignKey.DeclaringEntityType.DisplayName(),
                    foreignKey.PrincipalEntityType.DisplayName()));
            }

            return foreignKey.IsSelfReferencing()
                ? foreignKey.GetNavigations()
                : foreignKey.FindNavigations(foreignKey.DeclaringEntityType == entityType);
        }

        public static IEnumerable<INavigation> FindNavigationsFromInHierarchy(
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

            return foreignKey.IsIntraHierarchical()
                ? foreignKey.GetNavigations()
                : foreignKey.FindNavigations(foreignKey.DeclaringEntityType.IsAssignableFrom(entityType));
        }

        public static IEnumerable<INavigation> FindNavigationsTo([NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(entityType, nameof(entityType));

            if ((foreignKey.DeclaringEntityType != entityType)
                && (foreignKey.PrincipalEntityType != entityType))
            {
                throw new ArgumentException(CoreStrings.EntityTypeNotInRelationshipStrict(
                    entityType.DisplayName(),
                    foreignKey.DeclaringEntityType.DisplayName(),
                    foreignKey.PrincipalEntityType.DisplayName()));
            }

            return foreignKey.IsSelfReferencing()
                ? foreignKey.GetNavigations()
                : foreignKey.FindNavigations(foreignKey.PrincipalEntityType == entityType);
        }

        public static IEnumerable<INavigation> FindNavigationsToInHierarchy(
            [NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(entityType, nameof(entityType));

            if (!foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                && !foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
            {
                throw new ArgumentException(CoreStrings.EntityTypeNotInRelationship(
                    entityType.DisplayName(), foreignKey.DeclaringEntityType.DisplayName(), foreignKey.PrincipalEntityType.DisplayName()));
            }

            return foreignKey.IsIntraHierarchical()
                ? foreignKey.GetNavigations()
                : foreignKey.FindNavigations(foreignKey.PrincipalEntityType.IsAssignableFrom(entityType));
        }

        private static IEnumerable<INavigation> FindNavigations(
            this IForeignKey foreignKey, bool toPrincipal)
        {
            if (toPrincipal)
            {
                if (foreignKey.DependentToPrincipal != null)
                {
                    yield return foreignKey.DependentToPrincipal;
                }
            }
            else
            {
                if (foreignKey.PrincipalToDependent != null)
                {
                    yield return foreignKey.PrincipalToDependent;
                }
            }
        }

        public static IEntityType ResolveOtherEntityType([NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(entityType, nameof(entityType));

            if ((foreignKey.DeclaringEntityType != entityType)
                && (foreignKey.PrincipalEntityType != entityType))
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
    }
}
