// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class SpatialSqlServerFixture : SpatialFixtureBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => base.AddServices(serviceCollection)
            .AddEntityFrameworkSqlServerNetTopologySuite();

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    {
        var optionsBuilder = base.AddOptions(builder);
        new SqlServerDbContextOptionsBuilder(optionsBuilder).UseNetTopologySuite();

        return optionsBuilder;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<LineStringEntity>().Property(e => e.LineString).HasColumnType("geometry");
        modelBuilder.Entity<MultiLineStringEntity>().Property(e => e.MultiLineString).HasColumnType("geometry");
        modelBuilder.Entity<PointEntity>(
            x =>
            {
                x.Property(e => e.Geometry).HasColumnType("geometry");
                x.Property(e => e.Point).HasColumnType("geometry");
                x.Property(e => e.PointZ).HasColumnType("geometry");
                x.Property(e => e.PointM).HasColumnType("geometry");
                x.Property(e => e.PointZM).HasColumnType("geometry");
            });
        modelBuilder.Entity<PolygonEntity>().Property(e => e.Polygon).HasColumnType("geometry");
        modelBuilder.Entity<GeoPointEntity>().Property(e => e.Location).HasColumnType("geometry");
    }
}
