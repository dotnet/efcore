// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public static class NavigationExtensions
    {
        public static IClrCollectionAccessor GetCollectionAccessor([NotNull] this INavigation navigation)
            => navigation.AsNavigation().CollectionAccessor;

        public static Navigation AsNavigation([NotNull] this INavigation navigation, [NotNull] [CallerMemberName] string methodName = "")
            => navigation.AsConcreteMetadataType<INavigation, Navigation>(methodName);
    }
}
