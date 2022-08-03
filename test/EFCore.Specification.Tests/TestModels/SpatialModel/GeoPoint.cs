// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.SpatialModel;

public struct GeoPoint
{
    public GeoPoint(double lat, double lon)
    {
        Lat = lat;
        Lon = lon;
    }

    public double Lat { get; }
    public double Lon { get; }
}
