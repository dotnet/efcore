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

                    b.HasOne(g => g.CityOfBirth).WithMany(c => c.BornGears).HasForeignKey(g => g.CityOrBirthName).IsRequired();
                    b.HasOne(g => g.Tag).WithOne(t => t.Gear).HasForeignKey<CogTag>(t => new { t.GearNickName, t.GearSquadId });
                    b.HasOne(g => g.AssignedCity).WithMany(c => c.StationedGears).IsRequired(false);
                });

            modelBuilder.Entity<Officer>().HasBaseType<Gear>();
            modelBuilder.Entity<Officer>(b =>
                {
                    b.HasMany(o => o.Reports).WithOne().HasForeignKey(o => new { o.LeaderNickname, o.LeaderSquadId });
                });

            modelBuilder.Entity<CogTag>(b =>
                {
                    b.HasKey(t => t.Id);
                });

            modelBuilder.Entity<Squad>(b =>
                {
                    b.HasKey(s => s.Id);
                    b.HasMany(s => s.Members).WithOne(g => g.Squad).HasForeignKey(g => g.SquadId);
                });

            // TODO: See issue #3282
            modelBuilder.Entity<Gear>().Property(g => g.SquadId).Metadata.RequiresValueGenerator = null;

            modelBuilder.Entity<Weapon>(b =>
                {
                    b.HasOne(w => w.SynergyWith).WithOne().HasForeignKey<Weapon>(w => w.SynergyWithId);
                    b.HasOne(w => w.Owner).WithMany(g => g.Weapons).HasForeignKey(w => w.OwnerFullName).HasPrincipalKey(g => g.FullName);
                });
        }
    }
}
