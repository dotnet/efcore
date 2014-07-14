// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using ConcurrencyModel;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Relational.FunctionalTests
{
    public abstract class OptimisticConcurrencyRelationalTestBase<TTestStore> : OptimisticConcurrencyTestBase<TTestStore>
        where TTestStore : TestStore
    {
        public virtual Metadata.Model AddStoreMetadata(ModelBuilder modelBuilder)
        {
            var model = modelBuilder.Model;

            var chassisType = model.GetEntityType(typeof(Chassis));
            var teamType = model.GetEntityType(typeof(Team));

            chassisType.SetTableName("Chassis");
            teamType.SetTableName("Team");
            model.GetEntityType(typeof(Driver)).SetTableName("Drivers");
            model.GetEntityType(typeof(Engine)).SetTableName("Engines");
            model.GetEntityType(typeof(EngineSupplier)).SetTableName("EngineSuppliers");
            model.GetEntityType(typeof(Gearbox)).SetTableName("Gearboxes");
            model.GetEntityType(typeof(Sponsor)).SetTableName("Sponsors");
            model.GetEntityType(typeof(TestDriver)).SetTableName("TestDrivers");
            model.GetEntityType(typeof(TitleSponsor)).SetTableName("TitleSponsors");

            {
                // Chasis 1 <-> 1 Team
                modelBuilder
                    .Entity<Chassis>()
                    .ForeignKeys(fks => fks.ForeignKey<Team>(c => c.TeamId, isUnique: true).CascadeDelete(cascadeDelete: true));

                var chassisTeamIdFk = chassisType.ForeignKeys.Single(fk => fk.Properties.Single().Name == "TeamId");
                chassisType.AddNavigation(new Navigation(chassisTeamIdFk, "Team", pointsToPrincipal: true));
                teamType.AddNavigation(new Navigation(chassisTeamIdFk, "Chassis", pointsToPrincipal: false));
            }

            return model;
        }
    }
}
