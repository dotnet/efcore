// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class GeometryComparer : IEqualityComparer<Geometry>
    {
        public static GeometryComparer Instance { get; } = new GeometryComparer();

        private GeometryComparer()
        {
        }

        public bool Equals(Geometry x, Geometry y)
            => (x == null && y == null)
                || (x != null
                    && y != null
                    && x.Normalized().EqualsExact(y.Normalized(), tolerance: 0.1));

        public int GetHashCode(Geometry obj)
            => throw new NotImplementedException();
    }
}
