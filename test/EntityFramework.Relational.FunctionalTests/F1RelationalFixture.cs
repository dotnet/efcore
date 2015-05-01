// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.ConcurrencyModel;

namespace Microsoft.Data.Entity.Relational.FunctionalTests
{
    public abstract class F1RelationalFixture<TTestStore> : F1FixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Chassis>().Table("Chassis");
            modelBuilder.Entity<Team>().Table("Teams");
            modelBuilder.Entity<Driver>().Table("Drivers");
            modelBuilder.Entity<Engine>().Table("Engines");
            modelBuilder.Entity<EngineSupplier>().Table("EngineSuppliers");
            modelBuilder.Entity<Gearbox>().Table("Gearboxes");
            modelBuilder.Entity<Sponsor>().Table("Sponsors");
            modelBuilder.Entity<TestDriver>().Table("TestDrivers");
            modelBuilder.Entity<TitleSponsor>().Table("TitleSponsors");
        }
    }
}
