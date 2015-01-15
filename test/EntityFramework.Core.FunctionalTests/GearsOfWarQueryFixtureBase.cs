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

            modelBuilder.Entity<Gear>(b =>
            {
                b.Key(g => new { g.Nickname, g.SquadId });
                b.HasOne(g => g.CityOfBirth).WithMany(c => c.BornGears).ForeignKey(g => g.CityOrBirthName).Required();
                b.HasMany(g => g.Reports).WithOne().ForeignKey(g => new { g.LeaderNickname, g.LeaderSquadId });
                b.HasOne(g => g.Tag).WithOne(t => t.Gear).ForeignKey<CogTag>(t => new { t.GearNickName, t.GearSquadId });
                b.HasOne(g => g.AssignedCity).WithMany(c => c.StationedGears).Required(false);
            });

            modelBuilder.Entity<CogTag>(b =>
            {
                b.Key(t => t.Id);
                b.Property(t => t.Id).GenerateValueOnAdd();
            });

            modelBuilder.Entity<Squad>(b =>
            {
                b.Key(s => s.Id);
                b.HasMany(s => s.Members).WithOne(g => g.Squad).ForeignKey(g => g.SquadId);
                b.Property(t => t.Id).GenerateValueOnAdd();
            });

            modelBuilder.Entity<Weapon>(b =>
            {
                b.HasOne(w => w.SynergyWith).WithOne().ForeignKey<Weapon>(w => w.SynergyWithId);
                b.HasOne(w => w.Owner).WithMany(g => g.Weapons).ForeignKey(w => new { w.OwnerNickname, w.OwnerSquadId });
            });
        }
    }
}