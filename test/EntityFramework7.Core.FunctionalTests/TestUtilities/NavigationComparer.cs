// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.FunctionalTests.TestUtilities
{
    public class NavigationComparer : IEqualityComparer<INavigation>, IComparer<INavigation>
    {
        public static readonly NavigationComparer Instance = new NavigationComparer();

        private NavigationComparer()
        {
        }

        public int Compare(INavigation x, INavigation y)
        {
            return StringComparer.Ordinal.Compare(x.Name, y.Name);
        }

        public bool Equals(INavigation x, INavigation y)
        {
            if (x == null)
            {
                return y == null;
            }

            if (y == null)
            {
                return false;
            }

            return x.Name == y.Name
                   && x.PointsToPrincipal() == y.PointsToPrincipal();
        }

        public int GetHashCode(INavigation obj) => obj.Name.GetHashCode();
    }
}
