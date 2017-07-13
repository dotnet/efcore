// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class F1RelationalFixture : F1FixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Chassis>().ToTable("Chassis");
            modelBuilder.Entity<Team>().ToTable("Teams").Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<Driver>().ToTable("Drivers");
            modelBuilder.Entity<Engine>().ToTable("Engines");
            modelBuilder.Entity<EngineSupplier>().ToTable("EngineSuppliers");
            modelBuilder.Entity<Gearbox>().ToTable("Gearboxes");
            modelBuilder.Entity<Sponsor>().ToTable("Sponsors");
        }
    }
}
