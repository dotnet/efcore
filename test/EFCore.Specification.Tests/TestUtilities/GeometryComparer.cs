// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class GeometryComparer : IEqualityComparer<Geometry>
{
    public static GeometryComparer Instance { get; } = new();

    private GeometryComparer()
    {
    }

    public bool Equals(Geometry? x, Geometry? y)
        => (x == null && y == null)
            || (x != null
                && y != null
                && x.Normalized().EqualsExact(y.Normalized(), tolerance: 0.1));

    public int GetHashCode(Geometry obj)
        => throw new NotImplementedException();
}
