// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity
{
    public static class MutableNavigationExtensions
    {
        public static IMutableNavigation FindInverse([NotNull] this IMutableNavigation navigation)
            => (IMutableNavigation)((INavigation)navigation).FindInverse();

        public static IMutableEntityType GetTargetType([NotNull] this IMutableNavigation navigation)
            => (IMutableEntityType)((INavigation)navigation).GetTargetType();
    }
}
