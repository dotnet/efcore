// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.TestModels.SpatialModel
{
    public class SpatialContext : PoolableDbContext
    {
        public SpatialContext(DbContextOptions options)
            : base(options)
        {
        }

        public static void Seed(SpatialContext context, IGeometryFactory factory)
        {
            context.AddRange(SpatialData.CreatePointEntities(factory));
            context.AddRange(SpatialData.CreateGeoPointEntities());
            context.AddRange(SpatialData.CreateLineStringEntities(factory));
            context.AddRange(SpatialData.CreatePolygonEntities(factory));
            context.AddRange(SpatialData.CreateMultiLineStringEntities(factory));
            context.SaveChanges();
        }
    }
}
