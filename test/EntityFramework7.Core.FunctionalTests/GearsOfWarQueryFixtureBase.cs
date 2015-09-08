// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests.TestModels.GearsOfWarModel;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class GearsOfWarQueryFixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        public abstract TTestStore CreateTestStore();

        public abstract GearsOfWarContext CreateContext(TTestStore testStore);

        protected virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<City>().Key(c => c.Name);

            modelBuilder.Entity<Gear>(b =>
                {
                    b.Key(g => new { g.Nickname, g.SquadId });

                    b.Reference(g => g.CityOfBirth).InverseCollection(c => c.BornGears).ForeignKey(g => g.CityOrBirthName).Required();
                    b.Collection(g => g.Reports).InverseReference().ForeignKey(g => new { g.LeaderNickname, g.LeaderSquadId });
                    b.Reference(g => g.Tag).InverseReference(t => t.Gear).ForeignKey<CogTag>(t => new { t.GearNickName, t.GearSquadId });
                    b.Reference(g => g.AssignedCity).InverseCollection(c => c.StationedGears).Required(false);
                });

            modelBuilder.Entity<CogTag>(b =>
                {
                    b.Key(t => t.Id);
                });

            modelBuilder.Entity<Squad>(b =>
                {
                    b.Key(s => s.Id);
                    b.Collection(s => s.Members).InverseReference(g => g.Squad).ForeignKey(g => g.SquadId);
                });

            modelBuilder.Entity<Weapon>(b =>
                {
                    b.Reference(w => w.SynergyWith).InverseReference().ForeignKey<Weapon>(w => w.SynergyWithId);
                    b.Reference(w => w.Owner).InverseCollection(g => g.Weapons).ForeignKey(w => new { w.OwnerNickname, w.OwnerSquadId });
                });
        }
    }
}
