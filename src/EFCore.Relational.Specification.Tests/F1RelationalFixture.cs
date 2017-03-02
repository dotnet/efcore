// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.ConcurrencyModel;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class F1RelationalFixture<TTestStore> : F1FixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
