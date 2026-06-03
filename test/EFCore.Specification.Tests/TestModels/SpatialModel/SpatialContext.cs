// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.TestModels.SpatialModel;

public class SpatialContext(DbContextOptions options) : PoolableDbContext(options)
{
    public static Task SeedAsync(SpatialContext context, GeometryFactory factory)
    {
        context.AddRange(SpatialData.CreatePointEntities(factory));
        context.AddRange(SpatialData.CreateGeoPointEntities());
        context.AddRange(SpatialData.CreateLineStringEntities(factory));
        context.AddRange(SpatialData.CreatePolygonEntities(factory));
        context.AddRange(SpatialData.CreateMultiLineStringEntities(factory));

        return context.SaveChangesAsync();
    }
}
