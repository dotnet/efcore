// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.SpatialModel;

public struct GeoPoint(double lat, double lon)
{
    public double Lat { get; } = lat;
    public double Lon { get; } = lon;
}
