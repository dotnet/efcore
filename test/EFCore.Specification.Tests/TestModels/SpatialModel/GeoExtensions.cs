// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.TestModels.SpatialModel;

#nullable disable

public static class GeoExtensions
{
    public static double Distance(this GeoPoint x, GeoPoint y)
    {
        var converter = new GeoPointConverter();

        var xPoint = (Point)converter.ConvertToProvider(x);
        var yPoint = (Point)converter.ConvertToProvider(y);

        return yPoint.Distance(xPoint);
    }
}
