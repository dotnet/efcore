// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.FunctionalTests.TestModels.GearsOfWarModel
{
    public class GearsOfWarContext : DbContext
    {
        public GearsOfWarContext(IServiceProvider serviceProvider, DbContextOptions options)
            : base(serviceProvider, options)
        {
        }

        public DbSet<Gear> Gears { get; set; }
        public DbSet<Squad> Squads { get; set; }
        public DbSet<CogTag> Tags { get; set; }
        public DbSet<Weapon> Weapons { get; set; }
        public DbSet<City> Cities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<City>().Key(c => c.Name);

            builder.Entity<Gear>(b =>
                {
                    b.Key(g => new { g.Nickname, g.SquadId });
                    b.ManyToOne(g => g.CityOfBirth).ForeignKey(g => g.CityOrBirthName);
                    b.OneToMany(g => g.Reports).ForeignKey(g => new { g.LeaderNickname, g.LeaderSquadId });
                    b.OneToOne(g => g.Tag, t => t.Gear).ForeignKey<CogTag>(t => new { t.GearNickName, t.GearSquadId });
                });

            builder.Entity<CogTag>(b =>
                {
                    b.Key(t => t.Id);
                    b.Property(t => t.Id).GenerateValueOnAdd();
                });

            builder.Entity<Squad>(b =>
                {
                    b.Key(s => s.Id);
                    b.OneToMany(s => s.Members, g => g.Squad).ForeignKey(g => g.SquadId);
                    b.Property(t => t.Id).GenerateValueOnAdd();
                });

            builder.Entity<Weapon>(b =>
                {
                    b.OneToOne(w => w.SynergyWith).ForeignKey<Weapon>(w => w.SynergyWithId);
                    b.ManyToOne(w => w.Owner, g => g.Weapons).ForeignKey(w => new { w.OwnerNickname, w.OwnerSquadId });
                });
        }
    }
}
