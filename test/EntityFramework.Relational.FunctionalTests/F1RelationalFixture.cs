// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.ConcurrencyModel;

namespace Microsoft.Data.Entity.Relational.FunctionalTests
{
    public abstract class F1RelationalFixture<TTestStore> : F1FixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);

            model.Entity<Chassis>().Table("Chassis");
            model.Entity<Team>().Table("Teams");
            model.Entity<Driver>().Table("Drivers");
            model.Entity<Engine>().Table("Engines");
            model.Entity<EngineSupplier>().Table("EngineSuppliers");
            model.Entity<Gearbox>().Table("Gearboxes");
            model.Entity<Sponsor>().Table("Sponsors");
            model.Entity<TestDriver>().Table("TestDrivers");
            model.Entity<TitleSponsor>().Table("TitleSponsors");
        }
    }
}
