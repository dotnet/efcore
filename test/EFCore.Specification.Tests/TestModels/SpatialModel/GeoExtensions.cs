// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.TestModels.SpatialModel
{
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
}
