// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;

namespace ConcurrencyModel
{
    public class F1Context : DbContext
    {
        public F1Context(IServiceProvider serviceProvider, DbContextOptions options)
            : base(serviceProvider, options)
        {
        }

        public DbSet<Team> Teams { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Sponsor> Sponsors { get; set; }
        public DbSet<Engine> Engines { get; set; }
        public DbSet<EngineSupplier> EngineSuppliers { get; set; }

        // TODO: convert to OnModelCreated
        public static BasicModelBuilder CreateModel()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Chassis>();
            modelBuilder.Entity<Driver>();
            modelBuilder.Entity<Engine>();
            modelBuilder.Entity<Team>();
            modelBuilder.Entity<Sponsor>();

            var chassisType = model.GetEntityType(typeof(Chassis));
            var driverType = model.GetEntityType(typeof(Driver));
            var engineType = model.GetEntityType(typeof(Engine));
            var teamType = model.GetEntityType(typeof(Team));
            var sponsorType = model.GetEntityType(typeof(Sponsor));

            // TODO: Remove when FAPI supports this
            engineType.AddProperty("EngineSupplierId", typeof(int), shadowProperty: false, concurrencyToken: true);
            engineType.AddProperty("Name", typeof(string), shadowProperty: false, concurrencyToken: true);
            chassisType.AddProperty("Version", typeof(byte[]), shadowProperty: false, concurrencyToken: true)
                .ValueGenerationOnSave = ValueGenerationOnSave.WhenInsertingAndUpdating;
            driverType.AddProperty("Version", typeof(byte[]), shadowProperty: false, concurrencyToken: true)
                .ValueGenerationOnSave = ValueGenerationOnSave.WhenInsertingAndUpdating;
            teamType.AddProperty("Version", typeof(byte[]), shadowProperty: false, concurrencyToken: true)
                .ValueGenerationOnSave = ValueGenerationOnSave.WhenInsertingAndUpdating;
            sponsorType.AddProperty("Version", typeof(byte[]), shadowProperty: false, concurrencyToken: true)
                .ValueGenerationOnSave = ValueGenerationOnSave.WhenInsertingAndUpdating;

            // TODO: Uncomment when complex types are supported
            //builder.ComplexType<Location>();
            modelBuilder.Entity<Chassis>(b => b.Key(c => c.TeamId));

            modelBuilder.Entity<Driver>(b =>
                {
                    b.Key(d => d.Id);
                    b.Property(d => d.CarNumber);
                    b.Property(d => d.Championships);
                    b.Property(d => d.FastestLaps);
                    b.Property(d => d.Name);
                    b.Property(d => d.Podiums);
                    b.Property(d => d.Poles);
                    b.Property(d => d.Races);
                    b.Property(d => d.TeamId);
                    b.Property(d => d.Wins);
                });

            modelBuilder.Entity<Engine>(b =>
                {
                    b.Key(e => e.Id);
                    b.OneToMany(e => e.Teams, e => e.Engine);
                    b.OneToMany(e => e.Gearboxes);
                });

            // TODO: Complex type
            // .Property(c => c.StorageLocation);

            modelBuilder.Entity<EngineSupplier>(b =>
                {
                    b.Key(e => e.Id);
                    b.Property(e => e.Name);
                    b.OneToMany(e => e.Engines, e => e.EngineSupplier);
                });

            modelBuilder.Entity<Gearbox>(b =>
                {
                    b.Key(g => g.Id);
                    b.Property(g => g.Name);
                    b.Property<int>("EngineId", shadowProperty: true);
                });

            // TODO: Complex type
            //builder
            //    .ComplexType<Location>()
            //    .Properties(ps =>
            //        {
            //            // TODO: Use lambda expression
            //            ps.Property<double>("Latitude", concurrencyToken: true);
            //            // TODO: Use lambda expression
            //            ps.Property<double>("Longitude", concurrencyToken: true);
            //        });

            modelBuilder.Entity<Sponsor>(b =>
                {
                    b.Key(s => s.Id);
                    b.Property(s => s.Name);
                });

            // TODO: Complex type
            //builder
            //    .ComplexType<SponsorDetails>()
            //    .Properties(ps =>
            //        {
            //            ps.Property(s => s.Days);
            //            ps.Property(s => s.Space);
            //        });

            modelBuilder.Entity<Team>(b =>
                {
                    b.Key(t => t.Id);
                    b.Property(t => t.Constructor);
                    b.Property(t => t.ConstructorsChampionships);
                    b.Property(t => t.DriversChampionships);
                    b.Property<int>("EngineId", shadowProperty: true);
                    b.Property(t => t.FastestLaps);
                    b.Property(t => t.GearboxId);
                    b.Property(t => t.Name);
                    b.Property(t => t.Poles);
                    b.Property(t => t.Principal);
                    b.Property(t => t.Races);
                    b.Property(t => t.Tire);
                    b.Property(t => t.Victories);
                    b.OneToMany(e => e.Drivers, e => e.Team);
                    b.OneToOne(e => e.Gearbox).ForeignKey<Team>(e => e.GearboxId);
                });

            modelBuilder.Entity<TestDriver>(b => b.Key(t => t.Id));

            modelBuilder.Entity<TitleSponsor>();
            // TODO: Complex type
            // .Property(t => t.Details);

            // TODO: Sponsor * <-> * Team

            // TODO: Remove once temporary keys can be overridden
            teamType.GetProperty("Id").ValueGenerationOnAdd = ValueGenerationOnAdd.None;
            teamType.GetProperty("Id").ValueGenerationOnSave = ValueGenerationOnSave.None;

            // TODO: Remove when FAPI supports this
            teamType.GetProperty("EngineId").IsNullable = true;

            return modelBuilder;
        }
    }
}
