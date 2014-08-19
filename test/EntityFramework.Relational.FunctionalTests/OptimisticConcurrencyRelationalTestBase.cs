// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using ConcurrencyModel;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Relational.FunctionalTests
{
    public abstract class OptimisticConcurrencyRelationalTestBase<TTestStore> : OptimisticConcurrencyTestBase<TTestStore>
        where TTestStore : TestStore
    {
        protected readonly string DatabaseName = "OptimisticConcurrencyTest";

        public virtual Metadata.Model AddStoreMetadata(ModelBuilder modelBuilder)
        {
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

            return model;
        }
    }
}
