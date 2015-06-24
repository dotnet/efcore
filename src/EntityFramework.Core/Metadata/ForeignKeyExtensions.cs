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

            return foreignKey.DeclaringEntityType.IsAssignableFrom(foreignKey.PrincipalEntityType)
                   || foreignKey.PrincipalEntityType.IsAssignableFrom(foreignKey.DeclaringEntityType);
        }

        public static bool IsSelfPrimaryKeyReferencing([NotNull] this IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            return foreignKey.DeclaringEntityType.FindPrimaryKey() == foreignKey.PrincipalKey;
        }

        public static INavigation FindNavigationFrom([NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(entityType, nameof(entityType));

            if (!foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                && !foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
            {
                throw new ArgumentException(Strings.EntityTypeNotInRelationship(
                    entityType.Name, foreignKey.DeclaringEntityType.Name, foreignKey.PrincipalEntityType.Name));
            }

            if (foreignKey.IsIntraHierarchical())
            {
                throw new InvalidOperationException(Strings.IntraHierarchicalAmbiguousNavigation(entityType.Name, Property.Format(foreignKey.Properties), foreignKey.PrincipalEntityType, foreignKey.DeclaringEntityType));
            }

            return foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                ? foreignKey.DependentToPrincipal
                : foreignKey.PrincipalToDependent;
        }

        public static Navigation FindNavigationFrom([NotNull] this ForeignKey foreignKey, [NotNull] EntityType entityType)
            => (Navigation)((IForeignKey)foreignKey).FindNavigationFrom(entityType);

        public static INavigation FindNavigationTo([NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(entityType, nameof(entityType));

            if (!foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                && !foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
            {
                throw new ArgumentException(Strings.EntityTypeNotInRelationship(
                    entityType.Name, foreignKey.DeclaringEntityType.Name, foreignKey.PrincipalEntityType.Name));
            }

            if (foreignKey.IsIntraHierarchical())
            {
                throw new InvalidOperationException(Strings.IntraHierarchicalAmbiguousNavigation(entityType.Name, Property.Format(foreignKey.Properties), foreignKey.PrincipalEntityType, foreignKey.DeclaringEntityType));
            }

            return foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                ? foreignKey.PrincipalToDependent
                : foreignKey.DependentToPrincipal;
        }

        public static Navigation FindNavigationTo([NotNull] this ForeignKey foreignKey, [NotNull] EntityType entityType)
            => (Navigation)((IForeignKey)foreignKey).FindNavigationTo(entityType);

        public static IEntityType ResolveOtherEntityType([NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(entityType, nameof(entityType));

            if (!foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                && !foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
            {
                throw new ArgumentException(Strings.EntityTypeNotInRelationship(
                    entityType.Name, foreignKey.DeclaringEntityType.Name, foreignKey.PrincipalEntityType.Name));
            }

            if (foreignKey.IsIntraHierarchical())
            {
                throw new InvalidOperationException(Strings.IntraHierarchicalAmbiguousTargetEntityType(entityType.Name, Property.Format(foreignKey.Properties)));
            }

            return foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                ? foreignKey.PrincipalEntityType
                : foreignKey.DeclaringEntityType;
        }

        public static EntityType ResolveOtherEntityType([NotNull] this ForeignKey foreignKey, [NotNull] EntityType entityType)
            => (EntityType)((IForeignKey)foreignKey).ResolveOtherEntityType(entityType);

        public static IEntityType ResolveEntityType([NotNull] this IForeignKey foreignKey, [NotNull] IEntityType entityType)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(entityType, nameof(entityType));

            if (!foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                && !foreignKey.PrincipalEntityType.IsAssignableFrom(entityType))
            {
                throw new ArgumentException(Strings.EntityTypeNotInRelationship(
                    entityType.Name, foreignKey.DeclaringEntityType.Name, foreignKey.PrincipalEntityType.Name));
            }

            if (foreignKey.IsIntraHierarchical())
            {
                throw new InvalidOperationException(Strings.IntraHierarchicalAmbiguousTargetEntityType(entityType.Name, Property.Format(foreignKey.Properties)));
            }

            return foreignKey.DeclaringEntityType.IsAssignableFrom(entityType)
                ? foreignKey.DeclaringEntityType
                : foreignKey.PrincipalEntityType;
        }

        public static EntityType ResolveEntityType([NotNull] this ForeignKey foreignKey, [NotNull] EntityType entityType)
            => (EntityType)((IForeignKey)foreignKey).ResolveEntityType(entityType);
    }
}
