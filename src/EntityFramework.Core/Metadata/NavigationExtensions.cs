// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
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

        public static bool IsCompatible(
            [NotNull] this Navigation navigation,
            [NotNull] EntityType principalType,
            [NotNull] EntityType dependentType,
            bool? shouldPointToPrincipal,
            bool? oneToOne)
        {
            Check.NotNull(navigation, "navigation");
            Check.NotNull(principalType, "principalType");
            Check.NotNull(dependentType, "dependentType");

            if ((!shouldPointToPrincipal.HasValue || navigation.PointsToPrincipal == shouldPointToPrincipal.Value)
                && navigation.ForeignKey.IsCompatible(principalType, dependentType, oneToOne))
            {
                return true;
            }

            if (!shouldPointToPrincipal.HasValue
                && navigation.ForeignKey.IsCompatible(dependentType, principalType, oneToOne))
            {
                return true;
            }

            return false;
        }

        public static bool IsNonNotifyingCollection([NotNull] this INavigation navigation, [NotNull] InternalEntityEntry entry)
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
