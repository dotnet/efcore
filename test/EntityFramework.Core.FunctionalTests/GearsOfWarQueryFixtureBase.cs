// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
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
            modelBuilder.Entity<City>().OneToMany(c => c.StationedGears, g => g.AssignedCity).Required(false);

            modelBuilder.Entity<Gear>(b =>
            {
                b.Key(g => new { g.Nickname, g.SquadId });
                b.ManyToOne(g => g.CityOfBirth, c => c.BornGears).ForeignKey(g => g.CityOrBirthName).Required(true);
                b.OneToMany(g => g.Reports).ForeignKey(g => new { g.LeaderNickname, g.LeaderSquadId });
                b.OneToOne(g => g.Tag, t => t.Gear).ForeignKey<CogTag>(t => new { t.GearNickName, t.GearSquadId });
            });

            modelBuilder.Entity<CogTag>(b =>
            {
                b.Key(t => t.Id);
                b.Property(t => t.Id).GenerateValueOnAdd();
            });

            modelBuilder.Entity<Squad>(b =>
            {
                b.Key(s => s.Id);
                b.OneToMany(s => s.Members, g => g.Squad).ForeignKey(g => g.SquadId);
                b.Property(t => t.Id).GenerateValueOnAdd();
            });

            modelBuilder.Entity<Weapon>(b =>
            {
                b.OneToOne(w => w.SynergyWith).ForeignKey<Weapon>(w => w.SynergyWithId);
                b.ManyToOne(w => w.Owner, g => g.Weapons).ForeignKey(w => new { w.OwnerNickname, w.OwnerSquadId });
            });
        }
    }
}