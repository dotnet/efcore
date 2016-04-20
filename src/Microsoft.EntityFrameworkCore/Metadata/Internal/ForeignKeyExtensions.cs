// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public static class ForeignKeyExtensions
    {
        public static bool IsSelfReferencing([NotNull] this IForeignKey foreignKey)
            => foreignKey.DeclaringEntityType == foreignKey.PrincipalEntityType;

        public static bool IsIntraHierarchical([NotNull] this IForeignKey foreignKey)
            => foreignKey.DeclaringEntityType.IsSameHierarchy(foreignKey.PrincipalEntityType);

        public static bool IsSelfPrimaryKeyReferencing([NotNull] this IForeignKey foreignKey)
            => foreignKey.DeclaringEntityType.FindPrimaryKey() == foreignKey.PrincipalKey;

        public static IEnumerable<INavigation> GetNavigations([NotNull] this IForeignKey foreignKey)
        {
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
            if (foreignKey.DeclaringEntityType != entityType
                && foreignKey.PrincipalEntityType != entityType)
            {
                throw new InvalidOperationException(CoreStrings.EntityTypeNotInRelationshipStrict(
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
            if (!foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                && !foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
            {
                throw new InvalidOperationException(CoreStrings.EntityTypeNotInRelationship(
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
            if (foreignKey.DeclaringEntityType != entityType
                && foreignKey.PrincipalEntityType != entityType)
            {
                throw new InvalidOperationException(CoreStrings.EntityTypeNotInRelationshipStrict(
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
            if (!foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                && !foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
            {
                throw new InvalidOperationException(CoreStrings.EntityTypeNotInRelationship(
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
            if (foreignKey.DeclaringEntityType != entityType
                && foreignKey.PrincipalEntityType != entityType)
            {
                throw new InvalidOperationException(CoreStrings.EntityTypeNotInRelationshipStrict(
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
            if (!foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                && !foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
            {
                throw new InvalidOperationException(CoreStrings.EntityTypeNotInRelationship(
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
            if (!foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                && !foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
            {
                throw new InvalidOperationException(CoreStrings.EntityTypeNotInRelationship(
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

        public static IDependentKeyValueFactory<TKey> GetDependentKeyValueFactory<TKey>(
            [NotNull] this IForeignKey foreignKey)
            => (IDependentKeyValueFactory<TKey>)foreignKey.AsForeignKey().DependentKeyValueFactory;

        public static IDependentsMap CreateDependentsMapFactory([NotNull] this IForeignKey foreignKey)
            => foreignKey.AsForeignKey().DependentsMapFactory();

        public static ForeignKey AsForeignKey([NotNull] this IForeignKey foreignKey, [NotNull] [CallerMemberName] string methodName = "")
            => foreignKey.AsConcreteMetadataType<IForeignKey, ForeignKey>(methodName);
    }
}
