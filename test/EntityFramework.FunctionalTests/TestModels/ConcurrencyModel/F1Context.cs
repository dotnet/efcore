// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.FunctionalTests.TestModels.ConcurrencyModel
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
        public static ModelBuilder CreateModel(ModelBuilder modelBuilder)
        {
            var model = modelBuilder.Model;

            // TODO: Uncomment when complex types are supported
            //builder.ComplexType<Location>();
            modelBuilder.Entity<Chassis>(b =>
                {
                    b.Property(e => e.Version)
                        .StoreComputed()
                        .ConcurrencyToken();
                });

            modelBuilder.Entity<Driver>(b =>
                {
                    b.Property(d => d.CarNumber);
                    b.Property(d => d.Championships);
                    b.Property(d => d.FastestLaps);
                    b.Property(d => d.Name);
                    b.Property(d => d.Podiums);
                    b.Property(d => d.Poles);
                    b.Property(d => d.Races);
                    b.Property(d => d.TeamId);
                    b.Property(d => d.Wins);

                    b.Property(e => e.Version)
                        .StoreComputed()
                        .ConcurrencyToken();
                });

            modelBuilder.Entity<Engine>(b =>
                {
                    b.Property(e => e.EngineSupplierId).ConcurrencyToken();
                    b.Property(e => e.Name).ConcurrencyToken();
                    b.OneToMany(e => e.Teams, e => e.Engine);
                    b.OneToMany(e => e.Gearboxes);
                });

            // TODO: Complex type
            // .Property(c => c.StorageLocation);

            modelBuilder.Entity<EngineSupplier>(b =>
                {
                    b.Property(e => e.Name);
                    b.OneToMany(e => e.Engines, e => e.EngineSupplier);
                });

            modelBuilder.Entity<Gearbox>(b =>
                {
                    b.Property(g => g.Name);
                    b.Property<int>("EngineId");
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
                    b.Property(s => s.Name);

                    b.Property(e => e.Version)
                        .StoreComputed()
                        .ConcurrencyToken();
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
                    // TODO: Remove once temporary keys can be overridden
                    b.Property(t => t.Id).GenerateValuesOnAdd(false);
                    b.Property(t => t.Constructor);
                    b.Property(t => t.ConstructorsChampionships);
                    b.Property(t => t.DriversChampionships);
                    b.Property<int>("EngineId").Required(false);
                    b.Property(t => t.FastestLaps);
                    b.Property(t => t.GearboxId);
                    b.Property(t => t.Name);
                    b.Property(t => t.Poles);
                    b.Property(t => t.Principal);
                    b.Property(t => t.Races);
                    b.Property(t => t.Tire);
                    
                    b.Property(t => t.Version)
                        .StoreComputed()
                        .ConcurrencyToken();

                    b.Property(t => t.Victories);
                    b.OneToMany(e => e.Drivers, e => e.Team);
                    b.OneToOne(e => e.Gearbox).ForeignKey<Team>(e => e.GearboxId);
                });

            modelBuilder.Entity<TestDriver>();

            modelBuilder.Entity<TitleSponsor>();
            // TODO: Complex type
            // .Property(t => t.Details);

            // TODO: Sponsor * <-> * Team

            return modelBuilder;
        }
    }
}
