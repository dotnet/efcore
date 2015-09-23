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
            modelBuilder.Entity<City>().HasKey(c => c.Name);

            modelBuilder.Entity<Gear>(b =>
                {
                    b.HasKey(g => new { g.Nickname, g.SquadId });

                    b.HasOne(g => g.CityOfBirth).WithMany(c => c.BornGears).ForeignKey(g => g.CityOrBirthName).Required();
                    b.HasOne(g => g.Tag).WithMany(t => t.Gear).ForeignKey<CogTag>(t => new { t.GearNickName, t.GearSquadId });
                    b.HasOne(g => g.AssignedCity).WithMany(c => c.StationedGears).Required(false);
                });

            modelBuilder.Entity<Officer>().BaseType<Gear>();
            modelBuilder.Entity<Officer>(b =>
                {
                    b.Collection(o => o.Reports).InverseReference().ForeignKey(o => new { o.LeaderNickname, o.LeaderSquadId });
                });

            modelBuilder.Entity<CogTag>(b =>
                {
                    b.HasKey(t => t.Id);
                });

            modelBuilder.Entity<Squad>(b =>
                {
                    b.HasKey(s => s.Id);
                    b.HasMany(s => s.Members).WithOne(g => g.Squad).ForeignKey(g => g.SquadId);
                });

            modelBuilder.Entity<Weapon>(b =>
                {
                    b.HasOne(w => w.SynergyWith).WithOne().ForeignKey<Weapon>(w => w.SynergyWithId);
                    b.HasOne(w => w.Owner).WithMany(g => g.Weapons).ForeignKey(w => w.OwnerFullName).PrincipalKey(g => g.FullName);
                });
        }
    }
}
