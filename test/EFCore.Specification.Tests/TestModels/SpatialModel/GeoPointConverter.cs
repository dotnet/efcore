// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.TestModels.SpatialModel;

public class GeoPointConverter(GeometryFactory geoFactory) : ValueConverter<GeoPoint, Point>(
    v => geoFactory.CreatePoint(new Coordinate(v.Lon, v.Lat)),
    v => new GeoPoint(v.Y, v.X))
{
    private static readonly GeometryFactory _geometryFactory
        = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 0);

    public GeoPointConverter()
        : this(_geometryFactory)
    {
    }
}
