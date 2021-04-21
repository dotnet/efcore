// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
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

        protected override void Clean(DbContext context)
        {
            context.Database.ExecuteSqlRaw("DROP VIEW IF EXISTS vector_layers");
            context.Database.ExecuteSqlRaw("DROP VIEW IF EXISTS vector_layers_auth");
            context.Database.ExecuteSqlRaw("DROP VIEW IF EXISTS vector_layers_statistics");
            context.Database.ExecuteSqlRaw("DROP VIEW IF EXISTS vector_layers_field_infos");
        }
    }
}
