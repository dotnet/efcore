// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class GeometryComparer : IEqualityComparer<IGeometry>
    {
        public static GeometryComparer Instance { get; } = new GeometryComparer();

        private GeometryComparer()
        {
        }

        public bool Equals(IGeometry x, IGeometry y)
            => x == null ? y == null : x.Normalized().EqualsExact(y.Normalized(), tolerance: 0.1);

        public int GetHashCode(IGeometry obj)
            => throw new NotImplementedException();
    }
}
