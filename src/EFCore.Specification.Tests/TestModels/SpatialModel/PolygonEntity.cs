// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using GeoAPI.Geometries;

namespace Microsoft.EntityFrameworkCore.TestModels.SpatialModel
{
    public class PolygonEntity
    {
        public int Id { get; set; }
        public IPolygon Polygon { get; set; }
    }
}
