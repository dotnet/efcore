// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class SpatialSqliteFixture : SpatialFixtureBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => base.AddServices(serviceCollection)
            .AddEntityFrameworkSqliteNetTopologySuite();

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    {
        var optionsBuilder = base.AddOptions(builder);
        new SqliteDbContextOptionsBuilder(optionsBuilder).UseNetTopologySuite();

        return optionsBuilder;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<PointEntity>().Property(e => e.PointZ).HasColumnType("POINTZ");
        modelBuilder.Entity<PointEntity>().Property(e => e.PointM).HasColumnType("POINTM");
        modelBuilder.Entity<PointEntity>().Property(e => e.PointZM).HasColumnType("POINTZM");
    }
}
