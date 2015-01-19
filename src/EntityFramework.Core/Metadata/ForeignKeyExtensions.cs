// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public static class ForeignKeyExtensions
    {
        public static Navigation GetNavigationToPrincipal([NotNull] this ForeignKey foreignKey)
        {
            return (Navigation)((IForeignKey)foreignKey).GetNavigationToPrincipal();
        }

        public static INavigation GetNavigationToPrincipal([NotNull] this IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return foreignKey.EntityType.Navigations.SingleOrDefault(
                navigation => navigation.ForeignKey == foreignKey && navigation.PointsToPrincipal);
        }

        public static Navigation GetNavigationToDependent([NotNull] this ForeignKey foreignKey)
        {
            return (Navigation)((IForeignKey)foreignKey).GetNavigationToDependent();
        }

        public static INavigation GetNavigationToDependent([NotNull] this IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return foreignKey.ReferencedEntityType.Navigations.SingleOrDefault(
                navigation => navigation.ForeignKey == foreignKey && !navigation.PointsToPrincipal);
        }

        public static bool IsCompatible(
            [NotNull] this ForeignKey foreignKey,
            [NotNull] EntityType principalType,
            [NotNull] EntityType dependentType,
            bool? isUnique)
        {
            Check.NotNull(foreignKey, "foreignKey");
            Check.NotNull(principalType, "principalType");
            Check.NotNull(dependentType, "dependentType");

            return (isUnique == null || ((IForeignKey)foreignKey).IsUnique == isUnique)
                   && foreignKey.ReferencedEntityType == principalType
                   && foreignKey.EntityType == dependentType;
        }

        public static bool IsCompatible(
            [NotNull] this ForeignKey foreignKey,
            [NotNull] EntityType principalType,
            [NotNull] EntityType dependentType,
            [CanBeNull] IReadOnlyList<Property> foreignKeyProperties,
            [CanBeNull] IReadOnlyList<Property> referencedProperties,
            bool? isUnique)
        {
            Check.NotNull(foreignKey, "foreignKey");
            Check.NotNull(principalType, "principalType");
            Check.NotNull(dependentType, "dependentType");

            return foreignKey.IsCompatible(principalType, dependentType, isUnique)
                   && (foreignKeyProperties == null
                       || !foreignKeyProperties.Any()
                       || foreignKey.Properties.SequenceEqual(foreignKeyProperties))
                   && (referencedProperties == null
                       || !referencedProperties.Any()
                       || foreignKey.ReferencedKey.Properties.SequenceEqual(referencedProperties));
        }

        public static bool IsCompatible(
            [NotNull] this ForeignKey foreignKey,
            [NotNull] EntityType principalType,
            [NotNull] EntityType dependentType,
            [CanBeNull] string navigationToPrincipal,
            [CanBeNull] string navigationToDependent,
            [CanBeNull] IReadOnlyList<Property> foreignKeyProperties,
            [CanBeNull] IReadOnlyList<Property> referencedProperties,
            bool? isUnique)
        {
            Check.NotNull(foreignKey, "foreignKey");
            Check.NotNull(principalType, "principalType");
            Check.NotNull(dependentType, "dependentType");

            var existingNavigationToPrincipal = foreignKey.GetNavigationToPrincipal();
            var existingNavigationToDependent = foreignKey.GetNavigationToDependent();
            return foreignKey.IsCompatible(principalType, dependentType, foreignKeyProperties, referencedProperties, isUnique)
                   && (existingNavigationToPrincipal == null || existingNavigationToPrincipal.Name == navigationToPrincipal)
                   && (existingNavigationToDependent == null || existingNavigationToDependent.Name == navigationToDependent);
        }

        public static IEnumerable<IProperty> GetRootPrincipals(
            [NotNull] this IForeignKey foreignKey, int propertyIndex)
        {
            Check.NotNull(foreignKey, "foreignKey");

            var principalProperty = foreignKey.ReferencedProperties[propertyIndex];
            var isForeignKey = false;
            foreach (var nextForeignKey in principalProperty.EntityType.ForeignKeys)
            {
                for (var nextIndex = 0; nextIndex < nextForeignKey.Properties.Count; nextIndex++)
                {
                    if (principalProperty == nextForeignKey.Properties[nextIndex])
                    {
                        isForeignKey = true;

                        foreach (var rootPrincipal in GetRootPrincipals(nextForeignKey, nextIndex))
                        {
                            yield return rootPrincipal;
                        }
                    }
                }
            }

            if (!isForeignKey)
            {
                yield return principalProperty;
            }
        }
    }
}
