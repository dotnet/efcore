// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class ForeignKeyExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsSelfReferencing([NotNull] this IForeignKey foreignKey)
            => foreignKey.DeclaringEntityType == foreignKey.PrincipalEntityType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsIntraHierarchical([NotNull] this IForeignKey foreignKey)
            => foreignKey.DeclaringEntityType.IsSameHierarchy(foreignKey.PrincipalEntityType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsSelfPrimaryKeyReferencing([NotNull] this IForeignKey foreignKey)
            => foreignKey.DeclaringEntityType.FindPrimaryKey() == foreignKey.PrincipalKey;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<INavigation> FindNavigationsToInHierarchy(
            [NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            if (!foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                && !foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityTypeNotInRelationship(
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEntityType ResolveOtherEntityType([NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
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

            return foreignKey.DeclaringEntityType == entityType
                ? foreignKey.PrincipalEntityType
                : foreignKey.DeclaringEntityType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
                        Property.Format(foreignKey.Properties),
                        foreignKey.PrincipalEntityType.DisplayName(),
                        foreignKey.DeclaringEntityType.DisplayName()));
            }

            return foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                ? foreignKey.PrincipalEntityType
                : foreignKey.DeclaringEntityType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
                        entityType.DisplayName(), Property.Format(foreignKey.Properties),
                        foreignKey.PrincipalEntityType.DisplayName(),
                        foreignKey.DeclaringEntityType.DisplayName()));
            }

            return foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                ? foreignKey.DeclaringEntityType
                : foreignKey.PrincipalEntityType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IDependentKeyValueFactory<TKey> GetDependentKeyValueFactory<TKey>(
            [NotNull] this IForeignKey foreignKey)
            => (IDependentKeyValueFactory<TKey>)foreignKey.AsForeignKey().DependentKeyValueFactory;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IDependentsMap CreateDependentsMapFactory([NotNull] this IForeignKey foreignKey)
            => foreignKey.AsForeignKey().DependentsMapFactory();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
                .Append(Property.Format(foreignKey.Properties))
                .Append(" -> ")
                .Append(foreignKey.PrincipalEntityType.DisplayName())
                .Append(" ")
                .Append(Property.Format(foreignKey.PrincipalKey.Properties));

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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static ForeignKey AsForeignKey([NotNull] this IForeignKey foreignKey, [NotNull] [CallerMemberName] string methodName = "")
            => MetadataExtensions.AsConcreteMetadataType<IForeignKey, ForeignKey>(foreignKey, methodName);
    }
}
