// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.ConcurrencyModel;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class F1RelationalFixture<TTestStore> : F1FixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Chassis>().ToTable("Chassis");
            modelBuilder.Entity<Team>().ToTable("Teams").Property(e => e.Id).StoreGeneratedPattern(StoreGeneratedPattern.None);
            modelBuilder.Entity<Driver>().ToTable("Drivers");
            modelBuilder.Entity<Engine>().ToTable("Engines");
            modelBuilder.Entity<EngineSupplier>().ToTable("EngineSuppliers");
            modelBuilder.Entity<Gearbox>().ToTable("Gearboxes");
            modelBuilder.Entity<Sponsor>().ToTable("Sponsors");
            modelBuilder.Entity<TestDriver>().ToTable("TestDrivers");
            modelBuilder.Entity<TitleSponsor>().ToTable("TitleSponsors");
        }
    }
}
