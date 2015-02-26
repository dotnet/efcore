// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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

            modelBuilder
                .Entity<Chassis>(b =>
                    {
                        b.ForRelational().Table("Chassis");
                    });

            modelBuilder
                .Entity<Team>(b =>
                    {

                        b.ForRelational().Table("Teams");
                    });

            modelBuilder
                .Entity<Driver>(b =>
                    {
                        b.ForRelational().Table("Drivers");
                    });

            modelBuilder
                .Entity<Engine>(b =>
                    {
                        b.ForRelational().Table("Engines");
                    });

            modelBuilder
                .Entity<EngineSupplier>(b =>
                    {
                        b.ForRelational().Table("EngineSuppliers");
                    });

            modelBuilder
                .Entity<Gearbox>(b =>
                    {
                        b.ForRelational().Table("Gearboxes");
                    });

            modelBuilder
                .Entity<Sponsor>(b =>
                    {
                        b.ForRelational().Table("Sponsors");
                    });

            modelBuilder
                .Entity<TestDriver>(b =>
                    {
                        b.ForRelational().Table("TestDrivers");
                    });

            modelBuilder
                .Entity<TitleSponsor>()
                .ForRelational()
                .Table("TitleSponsors");
        }
    }
}
