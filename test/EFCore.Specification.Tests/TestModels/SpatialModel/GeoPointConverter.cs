// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.TestModels.SpatialModel
{
    public class GeoPointConverter : ValueConverter<GeoPoint, Point>
    {
        private static readonly GeometryFactory _geometryFactory
            = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 0);

        public GeoPointConverter()
            : this(_geometryFactory)
        {
        }

        public GeoPointConverter(GeometryFactory geoFactory)
            : base(
                v => geoFactory.CreatePoint(new Coordinate(v.Lon, v.Lat)),
                v => new GeoPoint(v.Y, v.X))
        {
        }
    }
}
