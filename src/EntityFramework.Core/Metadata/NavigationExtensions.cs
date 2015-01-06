// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
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

        public static Navigation TryGetInverse([NotNull] this Navigation navigation)
        {
            Check.NotNull(navigation, "navigation");

            return navigation.PointsToPrincipal
                ? navigation.ForeignKey.GetNavigationToDependent()
                : navigation.ForeignKey.GetNavigationToPrincipal();
        }

        public static INavigation TryGetInverse([NotNull] this INavigation navigation)
        {
            Check.NotNull(navigation, "navigation");

            return navigation.PointsToPrincipal
                ? navigation.ForeignKey.GetNavigationToDependent()
                : navigation.ForeignKey.GetNavigationToPrincipal();
        }

        public static EntityType GetTargetType([NotNull] this Navigation navigation)
        {
            Check.NotNull(navigation, "navigation");

            return navigation.PointsToPrincipal
                ? navigation.ForeignKey.ReferencedEntityType
                : navigation.ForeignKey.EntityType;
        }

        public static IEntityType GetTargetType([NotNull] this INavigation navigation)
        {
            Check.NotNull(navigation, "navigation");

            return navigation.PointsToPrincipal
                ? navigation.ForeignKey.ReferencedEntityType
                : navigation.ForeignKey.EntityType;
        }

        public static bool IsNonNotifyingCollection([NotNull] this INavigation navigation, [NotNull] StateEntry entry)
        {
            Check.NotNull(navigation, "navigation");
            Check.NotNull(entry, "entry");

            if (!navigation.IsCollection())
            {
                return false;
            }

            // TODO: Returning true until INotifyCollectionChanged (Issue #445) is supported.
            return true;

            //if (typeof(INotifyCollectionChanged).GetTypeInfo().IsAssignableFrom(navigation.GetType().GetTypeInfo()))
            //{
            //    return false;
            //}

            //var collectionInstance = entry[navigation];

            //return collectionInstance != null && !(collectionInstance is INotifyCollectionChanged);
        }
    }
}
