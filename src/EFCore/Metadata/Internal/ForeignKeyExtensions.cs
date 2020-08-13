// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

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
        public static bool IsIntraHierarchical([NotNull] this IForeignKey foreignKey)
            => foreignKey.DeclaringEntityType.IsSameHierarchy(foreignKey.PrincipalEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsSelfPrimaryKeyReferencing([NotNull] this IForeignKey foreignKey)
            => foreignKey.DeclaringEntityType.FindPrimaryKey() == foreignKey.PrincipalKey;

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
            [NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
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
            [NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
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

            return foreignKey.IsIntraHierarchical()
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
            [NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            if (!foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                && !foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeNotInRelationship(
                        entityType.DisplayName(), foreignKey.DeclaringEntityType.DisplayName(),
                        foreignKey.PrincipalEntityType.DisplayName()));
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEntityType ResolveOtherEntityTypeInHierarchy(
            [NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
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

            if (foreignKey.IsIntraHierarchical())
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

            if (foreignKey.IsIntraHierarchical())
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
        public static IDependentKeyValueFactory<TKey> GetDependentKeyValueFactory<TKey>(
            [NotNull] this IForeignKey foreignKey)
            => (IDependentKeyValueFactory<TKey>)foreignKey.AsForeignKey().DependentKeyValueFactory;

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
        public static string ToDebugString([NotNull] this IForeignKey foreignKey, bool singleLine = true, [NotNull] string indent = "")
        {
            var builder = new StringBuilder();

            builder.Append(indent);

            if (singleLine)
            {
                builder.Append("ForeignKey: ");
            }

            builder
                .Append(foreignKey.DeclaringEntityType.DisplayName())
                .Append(" ")
                .Append(foreignKey.Properties.Format())
                .Append(" -> ")
                .Append(foreignKey.PrincipalEntityType.DisplayName())
                .Append(" ")
                .Append(foreignKey.PrincipalKey.Properties.Format());

            if (foreignKey.IsUnique)
            {
                builder.Append(" Unique");
            }

            if (foreignKey.IsOwnership)
            {
                builder.Append(" Ownership");
            }

            if (foreignKey.PrincipalToDependent != null)
            {
                builder.Append(" ToDependent: ").Append(foreignKey.PrincipalToDependent.Name);
            }

            if (foreignKey.DependentToPrincipal != null)
            {
                builder.Append(" ToPrincipal: ").Append(foreignKey.DependentToPrincipal.Name);
            }

            if (!singleLine)
            {
                builder.Append(foreignKey.AnnotationsToDebugString(indent + "  "));
            }

            return builder.ToString();
        }

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
