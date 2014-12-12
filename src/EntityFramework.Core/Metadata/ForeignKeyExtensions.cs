// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public static class ForeignKeyExtensions
    {
        public static INavigation GetNavigationToPrincipal([NotNull] this IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return foreignKey.EntityType.Navigations.SingleOrDefault(
                navigation => navigation.ForeignKey == foreignKey && navigation.PointsToPrincipal);
        }

        public static INavigation GetNavigationToDependent([NotNull] this IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return foreignKey.ReferencedEntityType.Navigations.SingleOrDefault(
                navigation => navigation.ForeignKey == foreignKey && !navigation.PointsToPrincipal);
        }

        public static IProperty FindRootValueGenerationProperty([NotNull] this IForeignKey foreignKey, int propertyIndex)
        {
            Check.NotNull(foreignKey, "foreignKey");

            var principalProperty = foreignKey.ReferencedProperties[propertyIndex];
            foreach (var nextForeignKey in principalProperty.EntityType.ForeignKeys)
            {
                for (var nextIndex = 0; nextIndex < nextForeignKey.Properties.Count; nextIndex++)
                {
                    if (principalProperty == nextForeignKey.Properties[nextIndex])
                    {
                        var rootPrincipal = FindRootValueGenerationProperty(nextForeignKey, nextIndex);
                        if (rootPrincipal.GenerateValueOnAdd)
                        {
                            return rootPrincipal;
                        }
                    }
                }
            }

            return principalProperty;
        }
    }
}
