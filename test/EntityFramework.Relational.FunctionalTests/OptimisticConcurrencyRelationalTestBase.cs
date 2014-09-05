// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.ConcurrencyModel;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Relational.FunctionalTests
{
    public abstract class OptimisticConcurrencyRelationalTestBase<TTestStore> : OptimisticConcurrencyTestBase<TTestStore>
        where TTestStore : TestStore
    {
        protected readonly string DatabaseName = "OptimisticConcurrencyTest";

        public virtual void AddStoreMetadata(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chassis>().Key(c => c.TeamId);
            modelBuilder.Entity<Driver>().Key(d => d.Id);
            modelBuilder.Entity<Engine>().Key(e => e.Id);
            modelBuilder.Entity<EngineSupplier>().Key(e => e.Id);
            modelBuilder.Entity<Gearbox>().Key(g => g.Id);
            modelBuilder.Entity<Sponsor>().Key(s => s.Id);
            modelBuilder.Entity<Team>().Key(t => t.Id);
            modelBuilder.Entity<TestDriver>(b => b.Key(t => t.Id));
            modelBuilder.Entity<TitleSponsor>();

            var model = modelBuilder.Model;

            model.GetEntityType(typeof(Chassis)).SetTableName("Chassis");
            model.GetEntityType(typeof(Team)).SetTableName("Team");
            model.GetEntityType(typeof(Driver)).SetTableName("Drivers");
            model.GetEntityType(typeof(Engine)).SetTableName("Engines");
            model.GetEntityType(typeof(EngineSupplier)).SetTableName("EngineSuppliers");
            model.GetEntityType(typeof(Gearbox)).SetTableName("Gearboxes");
            model.GetEntityType(typeof(Sponsor)).SetTableName("Sponsors");
            model.GetEntityType(typeof(TestDriver)).SetTableName("TestDrivers");
            model.GetEntityType(typeof(TitleSponsor)).SetTableName("TitleSponsors");

            modelBuilder.Entity<Team>().OneToOne(e => e.Chassis, e => e.Team);
        }
    }
}
