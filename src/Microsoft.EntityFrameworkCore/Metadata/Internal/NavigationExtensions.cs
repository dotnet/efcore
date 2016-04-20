// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public static class NavigationExtensions
    {
        public static IClrCollectionAccessor GetCollectionAccessor([NotNull] this INavigation navigation)
            => navigation.AsNavigation().CollectionAccessor;

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

        public static Navigation AsNavigation([NotNull] this INavigation navigation, [NotNull] [CallerMemberName] string methodName = "")
            => navigation.AsConcreteMetadataType<INavigation, Navigation>(methodName);
    }
}
