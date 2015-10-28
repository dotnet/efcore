// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public static class NavigationExtensions
    {
        public static IClrCollectionAccessor GetCollectionAccessor([NotNull] this INavigation navigation)
        {
            var accessors = navigation as INavigationAccessors;

            return accessors != null
                ? accessors.CollectionAccessor
                : new ClrCollectionAccessorFactory().Create(navigation);
        }

        public static bool IsNonNotifyingCollection([NotNull] this INavigation navigation, [NotNull] InternalEntityEntry entry)
        {
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
