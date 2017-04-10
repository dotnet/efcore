// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.GearsOfWarModel;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
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

                    b.HasOne(g => g.CityOfBirth).WithMany(c => c.BornGears).HasForeignKey(g => g.CityOrBirthName).IsRequired();
                    b.HasOne(g => g.Tag).WithOne(t => t.Gear).HasForeignKey<CogTag>(t => new { t.GearNickName, t.GearSquadId });
                    b.HasOne(g => g.AssignedCity).WithMany(c => c.StationedGears).IsRequired(false);
                });

            modelBuilder.Entity<Officer>().HasMany(o => o.Reports).WithOne().HasForeignKey(o => new { o.LeaderNickname, o.LeaderSquadId });

            modelBuilder.Entity<Squad>(b =>
                {
                    b.HasKey(s => s.Id);
                    b.HasMany(s => s.Members).WithOne(g => g.Squad).HasForeignKey(g => g.SquadId);
                });

            modelBuilder.Entity<Weapon>(b =>
                {
                    b.Property(w => w.Id).ValueGeneratedNever();
                    b.HasOne(w => w.SynergyWith).WithOne().HasForeignKey<Weapon>(w => w.SynergyWithId);
                    b.HasOne(w => w.Owner).WithMany(g => g.Weapons).HasForeignKey(w => w.OwnerFullName).HasPrincipalKey(g => g.FullName);
                });

            modelBuilder.Entity<SquadMission>(b =>
                {
                    b.HasKey(sm => new { sm.SquadId, sm.MissionId });
                    b.HasOne(sm => sm.Mission).WithMany(m => m.ParticipatingSquads).HasForeignKey(sm => sm.MissionId);
                    b.HasOne(sm => sm.Squad).WithMany(s => s.Missions).HasForeignKey(sm => sm.SquadId);
                });
        }
    }
}
