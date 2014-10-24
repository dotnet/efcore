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

            builder.Entity<Gear>().Key(g => new { g.Nickname, g.SquadId });
            builder.Entity<Gear>().ManyToOne(g => g.CityOfBirth).ForeignKey(g => g.CityOrBirthName);
            builder.Entity<Gear>().OneToMany(g => g.Reports).ForeignKey(g => new { g.LeaderNickname, g.LeaderSquadId });
            builder.Entity<Gear>().OneToOne(g => g.Tag, t => t.Gear).ForeignKey<CogTag>(t => new { t.GearNickName, t.GearSquadId });

            builder.Entity<CogTag>().Key(t => t.Id);
            builder.Model.GetEntityType(typeof(CogTag)).GetProperty("Id").ValueGeneration = ValueGeneration.OnAdd;

            builder.Entity<Squad>().Key(s => s.Id);
            builder.Entity<Squad>().OneToMany(s => s.Members, g => g.Squad).ForeignKey(g => g.SquadId);
            builder.Model.GetEntityType(typeof(Squad)).GetProperty("Id").ValueGeneration = ValueGeneration.OnAdd;

            builder.Entity<Weapon>().OneToOne(w => w.SynergyWith).ForeignKey<Weapon>(w => w.SynergyWithId);
            builder.Entity<Weapon>().ManyToOne(w => w.Owner, g => g.Weapons).ForeignKey(w => new { w.OwnerNickname, w.OwnerSquadId });
        }
    }
}
