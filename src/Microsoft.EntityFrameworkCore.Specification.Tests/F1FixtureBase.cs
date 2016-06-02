// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.ConcurrencyModel;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
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
                    b.HasKey(c => c.TeamId);
                    b.Property<byte[]>("Version")
                        .ValueGeneratedOnAddOrUpdate()
                        .IsConcurrencyToken();
                });

            modelBuilder.Entity<Driver>(b =>
                {
                    b.Property<byte[]>("Version")
                        .ValueGeneratedOnAddOrUpdate()
                        .IsConcurrencyToken();
                });

            modelBuilder.Entity<Engine>(b =>
                {
                    b.Property(e => e.EngineSupplierId).IsConcurrencyToken();
                    b.Property(e => e.Name).IsConcurrencyToken();
                });

            // TODO: Complex type
            // .Property(c => c.StorageLocation);

            modelBuilder.Ignore<Location>();

            modelBuilder.Entity<EngineSupplier>();

            modelBuilder.Entity<Gearbox>();

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
                    b.Property<int?>(Sponsor.ClientTokenPropertyName)
                        .IsConcurrencyToken();
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
                    b.Property<byte[]>("Version")
                        .ValueGeneratedOnAddOrUpdate()
                        .IsConcurrencyToken();

                    b.HasOne(e => e.Gearbox).WithOne().HasForeignKey<Team>(e => e.GearboxId);
                    b.HasOne(e => e.Chassis).WithOne(e => e.Team).HasForeignKey<Chassis>(e => e.TeamId);
                });

            modelBuilder.Entity<TestDriver>();
            modelBuilder.Entity<TitleSponsor>();

            // TODO: Complex type
            // .Property(t => t.Details);

            // TODO: Sponsor * <-> * Team
        }
    }
}
