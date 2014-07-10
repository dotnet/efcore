// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public static class NavigationExtensions
    {
        public static bool IsCollection([NotNull] this INavigation navigation)
        {
            Check.NotNull(navigation, "navigation");

            return !navigation.PointsToPrincipal && !navigation.ForeignKey.IsUnique;
        }

        public static INavigation TryGetInverse([NotNull] this INavigation navigation)
        {
            Check.NotNull(navigation, "navigation");

            return GetTargetType(navigation).Navigations.FirstOrDefault(
                i => i.ForeignKey == navigation.ForeignKey && i.PointsToPrincipal != navigation.PointsToPrincipal);
        }

        public static IEntityType GetTargetType([NotNull] this INavigation navigation)
        {
            Check.NotNull(navigation, "navigation");

            // TODO: Ensure only one inverse can be created when building metadata
            return navigation.PointsToPrincipal
                ? navigation.ForeignKey.ReferencedEntityType
                : navigation.ForeignKey.EntityType;
        }
    }
}
