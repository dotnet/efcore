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
        public static ModelBuilder CreateModel()
        {
            var model = new Model();
            var modelBuilder = new ConventionModelBuilder(model);

            // TODO: Uncomment when complex types are supported
            //builder.ComplexType<Location>();
            modelBuilder
                .Entity<Chassis>()
                .Key(c => c.TeamId);

            modelBuilder
                .Entity<Driver>(ps =>
                    {
                        ps.Key(d => d.Id);
                        ps.Property(d => d.CarNumber);
                        ps.Property(d => d.Championships);
                        ps.Property(d => d.FastestLaps);
                        ps.Property(d => d.Name);
                        ps.Property(d => d.Podiums);
                        ps.Property(d => d.Poles);
                        ps.Property(d => d.Races);
                        ps.Property(d => d.TeamId);
                        ps.Property(d => d.Wins);
                    });

            modelBuilder
                .Entity<Engine>()
                .Key(e => e.Id);

            // TODO: Complex type
            // .Property(c => c.StorageLocation);

            modelBuilder.Entity<EngineSupplier>(b =>
                {
                    b.Key(e => e.Id);
                    b.Property(e => e.Name);
                });

            modelBuilder
                .Entity<Gearbox>(ps =>
                    {
                        ps.Key(g => g.Id);
                        ps.Property(g => g.Name);
                        ps.Property<int>("EngineId", shadowProperty: true);
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

            modelBuilder
                .Entity<Team>(ps =>
                    {
                        ps.Key(t => t.Id);
                        ps.Property(t => t.Constructor);
                        ps.Property(t => t.ConstructorsChampionships);
                        ps.Property(t => t.DriversChampionships);
                        ps.Property<int>("EngineId", shadowProperty: true);
                        ps.Property(t => t.FastestLaps);
                        ps.Property(t => t.GearboxId);
                        ps.Property(t => t.Name);
                        ps.Property(t => t.Poles);
                        ps.Property(t => t.Principal);
                        ps.Property(t => t.Races);
                        ps.Property(t => t.Tire);
                        ps.Property(t => t.Victories);
                    });

            modelBuilder
                .Entity<TestDriver>()
                .Key(t => t.Id);

            modelBuilder
                .Entity<TitleSponsor>();
            // TODO: Complex type
            // .Property(t => t.Details);

            var chassisType = model.GetEntityType(typeof(Chassis));
            var driverType = model.GetEntityType(typeof(Driver));
            var engineType = model.GetEntityType(typeof(Engine));
            var engineSupplierType = model.GetEntityType(typeof(EngineSupplier));
            var gearboxType = model.GetEntityType(typeof(Gearbox));
            var teamType = model.GetEntityType(typeof(Team));
            var sponsorType = model.GetEntityType(typeof(Sponsor));

            // TODO: Use FAPIS when available
            // TODO: Sponsor * <-> * Team

            {
                // Team * <-> 1 Engine
                var teamEngineIdFk = teamType.AddForeignKey(engineType.GetKey(), teamType.GetProperty("EngineId"));
                teamType.AddNavigation(new Navigation(teamEngineIdFk, "Engine", pointsToPrincipal: true));
                engineType.AddNavigation(new Navigation(teamEngineIdFk, "Teams", pointsToPrincipal: false));
            }

            {
                // Team -> 1? Gearbox
                var teamGearboxIdFk = teamType.AddForeignKey(gearboxType.GetKey(), teamType.GetProperty("GearboxId"));
                teamType.AddNavigation(new Navigation(teamGearboxIdFk, "Gearbox", pointsToPrincipal: true));
            }

            {
                // Driver * <-> 1 Team
                var driverTeamIdFk = driverType.AddForeignKey(teamType.GetKey(), driverType.GetProperty("TeamId"));
                driverType.AddNavigation(new Navigation(driverTeamIdFk, "Team", pointsToPrincipal: true));
                teamType.AddNavigation(new Navigation(driverTeamIdFk, "Drivers", pointsToPrincipal: false));
            }

            {
                // Engine * <-> 1 EngineSupplier
                var engineEngineSupplierIdFk = engineType.AddForeignKey(engineSupplierType.GetKey(), engineType.GetProperty("EngineSupplierId"));
                engineType.AddNavigation(new Navigation(engineEngineSupplierIdFk, "EngineSupplier", pointsToPrincipal: true));
                engineSupplierType.AddNavigation(new Navigation(engineEngineSupplierIdFk, "Engines", pointsToPrincipal: false));
            }

            {
                // Engine -> * Gearbox
                var gearboxEngineIdFk = gearboxType.AddForeignKey(engineType.GetKey(), gearboxType.GetProperty("EngineId"));
                engineType.AddNavigation(new Navigation(gearboxEngineIdFk, "Gearboxes", pointsToPrincipal: false));
            }

            // TODO: Remove once temporary keys can be overridden
            teamType.GetProperty("Id").ValueGenerationOnAdd = ValueGenerationOnAdd.None;
            teamType.GetProperty("Id").ValueGenerationOnSave = ValueGenerationOnSave.None;

            // TODO: Remove when FAPI supports this
            teamType.GetProperty("EngineId").IsNullable = true;
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

            return modelBuilder;
        }
    }
}
