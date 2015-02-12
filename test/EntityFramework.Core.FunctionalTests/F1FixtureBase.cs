// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests.TestModels.ConcurrencyModel;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class F1FixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        public abstract TTestStore CreateTestStore();

        public abstract F1Context CreateContext(TTestStore testStore);

        protected virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
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
                    b.HasMany(e => e.Teams).WithOne(e => e.Engine);
                    b.HasMany(e => e.Gearboxes).WithOne();
                });

            // TODO: Complex type
            // .Property(c => c.StorageLocation);
            modelBuilder.Ignore<Location>();

            modelBuilder.Entity<EngineSupplier>(b =>
                {
                    b.Property(e => e.Name);
                    b.HasMany(e => e.Engines).WithOne(e => e.EngineSupplier);
                });

            modelBuilder.Entity<Gearbox>(b => { b.Property(g => g.Name); });

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
            modelBuilder.Ignore<SponsorDetails>();

            modelBuilder.Entity<Team>(b =>
                {
                    b.Property(t => t.Version)
                        .StoreComputed()
                        .ConcurrencyToken();

                    b.HasMany(e => e.Drivers).WithOne(e => e.Team);
                    b.HasOne(e => e.Gearbox).WithOne().ForeignKey<Team>(e => e.GearboxId);
                });

            modelBuilder.Entity<TestDriver>();

            modelBuilder.Entity<TitleSponsor>();
            // TODO: Complex type
            // .Property(t => t.Details);

            // TODO: Sponsor * <-> * Team
        }
    }
}
