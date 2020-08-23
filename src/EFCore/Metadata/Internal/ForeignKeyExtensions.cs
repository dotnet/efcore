// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class ForeignKeyExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsSelfReferencing([NotNull] this IForeignKey foreignKey)
            => foreignKey.DeclaringEntityType == foreignKey.PrincipalEntityType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<INavigation> FindNavigationsFrom(
            [NotNull] this IForeignKey foreignKey,
            [NotNull] IEntityType entityType)
        {
            if (foreignKey.DeclaringEntityType != entityType
                && foreignKey.PrincipalEntityType != entityType)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeNotInRelationshipStrict(
                        entityType.DisplayName(),
                        foreignKey.DeclaringEntityType.DisplayName(),
                        foreignKey.PrincipalEntityType.DisplayName()));
            }

            return foreignKey.IsSelfReferencing()
                ? foreignKey.GetNavigations()
                : foreignKey.FindNavigations(foreignKey.DeclaringEntityType == entityType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<INavigation> FindNavigationsFromInHierarchy(
            [NotNull] this IForeignKey foreignKey,
            [NotNull] IEntityType entityType)
        {
            if (!foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                && !foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeNotInRelationship(
                        entityType.DisplayName(),
                        foreignKey.DeclaringEntityType.DisplayName(),
                        foreignKey.PrincipalEntityType.DisplayName()));
            }

            return foreignKey.DeclaringEntityType.IsAssignableFrom(foreignKey.PrincipalEntityType)
                || foreignKey.PrincipalEntityType.IsAssignableFrom(foreignKey.DeclaringEntityType)
                    ? foreignKey.GetNavigations()
                    : foreignKey.FindNavigations(foreignKey.DeclaringEntityType.IsAssignableFrom(entityType));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<INavigation> FindNavigationsTo([NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            if (foreignKey.DeclaringEntityType != entityType
                && foreignKey.PrincipalEntityType != entityType)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeNotInRelationshipStrict(
                        entityType.DisplayName(),
                        foreignKey.DeclaringEntityType.DisplayName(),
                        foreignKey.PrincipalEntityType.DisplayName()));
            }

            return foreignKey.IsSelfReferencing()
                ? foreignKey.GetNavigations()
                : foreignKey.FindNavigations(foreignKey.PrincipalEntityType == entityType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<INavigation> FindNavigationsToInHierarchy(
            [NotNull] this IForeignKey foreignKey,
            [NotNull] IEntityType entityType)
        {
            if (!foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                && !foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeNotInRelationship(
                        entityType.DisplayName(), foreignKey.DeclaringEntityType.DisplayName(),
                        foreignKey.PrincipalEntityType.DisplayName()));
            }

            return foreignKey.DeclaringEntityType.IsAssignableFrom(foreignKey.PrincipalEntityType)
                || foreignKey.PrincipalEntityType.IsAssignableFrom(foreignKey.DeclaringEntityType)
                    ? foreignKey.GetNavigations()
                    : foreignKey.FindNavigations(foreignKey.PrincipalEntityType.IsAssignableFrom(entityType));
        }

        private static IEnumerable<INavigation> FindNavigations(
            this IForeignKey foreignKey,
            bool toPrincipal)
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEntityType ResolveOtherEntityTypeInHierarchy(
            [NotNull] this IForeignKey foreignKey,
            [NotNull] IEntityType entityType)
        {
            if (!foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                && !foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeNotInRelationship(
                        entityType.DisplayName(),
                        foreignKey.DeclaringEntityType.DisplayName(),
                        foreignKey.PrincipalEntityType.DisplayName()));
            }

            if (foreignKey.DeclaringEntityType.IsAssignableFrom(foreignKey.PrincipalEntityType)
                || foreignKey.PrincipalEntityType.IsAssignableFrom(foreignKey.DeclaringEntityType))
            {
                throw new InvalidOperationException(
                    CoreStrings.IntraHierarchicalAmbiguousTargetEntityType(
                        entityType.DisplayName(),
                        foreignKey.Properties.Format(),
                        foreignKey.PrincipalEntityType.DisplayName(),
                        foreignKey.DeclaringEntityType.DisplayName()));
            }

            return foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                ? foreignKey.PrincipalEntityType
                : foreignKey.DeclaringEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEntityType ResolveEntityTypeInHierarchy([NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            if (!foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                && !foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeNotInRelationship(
                        entityType.DisplayName(),
                        foreignKey.DeclaringEntityType.DisplayName(),
                        foreignKey.PrincipalEntityType.DisplayName()));
            }

            if (foreignKey.DeclaringEntityType.IsAssignableFrom(foreignKey.PrincipalEntityType)
                || foreignKey.PrincipalEntityType.IsAssignableFrom(foreignKey.DeclaringEntityType))
            {
                throw new InvalidOperationException(
                    CoreStrings.IntraHierarchicalAmbiguousTargetEntityType(
                        entityType.DisplayName(), foreignKey.Properties.Format(),
                        foreignKey.PrincipalEntityType.DisplayName(),
                        foreignKey.DeclaringEntityType.DisplayName()));
            }

            return foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                ? foreignKey.DeclaringEntityType
                : foreignKey.PrincipalEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IDependentsMap CreateDependentsMapFactory([NotNull] this IForeignKey foreignKey)
            => foreignKey.AsForeignKey().DependentsMapFactory();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static ForeignKey AsForeignKey([NotNull] this IForeignKey foreignKey, [NotNull] [CallerMemberName] string methodName = "")
            => MetadataExtensions.AsConcreteMetadataType<IForeignKey, ForeignKey>(foreignKey, methodName);
    }
}
