// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore;

public abstract class SpatialFixtureBase : SharedStoreFixtureBase<SpatialContext>
{
    private readonly GeometryFactory _geometryFactory
        = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 0);

    protected override string StoreName
        => "SpatialTest";

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<PointEntity>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<LineStringEntity>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<PolygonEntity>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<MultiLineStringEntity>().Property(e => e.Id).ValueGeneratedNever();

        modelBuilder.Entity<GeoPointEntity>(
            b =>
            {
                b.Property(e => e.Id).ValueGeneratedNever();
                b.Property(e => e.Location).HasConversion(new GeoPointConverter(_geometryFactory));
            });
    }

    protected override Task SeedAsync(SpatialContext context)
        => SpatialContext.SeedAsync(context, _geometryFactory);
}
