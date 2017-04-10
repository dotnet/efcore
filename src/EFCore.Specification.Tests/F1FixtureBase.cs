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
            modelBuilder.Entity<Chassis>(b => { b.HasKey(c => c.TeamId); });

            modelBuilder.Entity<Engine>(b =>
                {
                    b.Property(e => e.EngineSupplierId).IsConcurrencyToken();
                    b.Property(e => e.Name).IsConcurrencyToken();
                    b.OwnsOne(e => e.StorageLocation, lb =>
                        {
                            lb.Property(l => l.Latitude).IsConcurrencyToken();
                            lb.Property(l => l.Longitude).IsConcurrencyToken();
                        });
                });

            modelBuilder.Ignore<Location>();

            modelBuilder.Entity<EngineSupplier>();

            modelBuilder.Entity<Gearbox>();

            modelBuilder.Entity<Sponsor>(b =>
                {
                    b.Property<int?>(Sponsor.ClientTokenPropertyName)
                        .IsConcurrencyToken();
                });

            modelBuilder.Entity<Team>(b =>
                {
                    b.HasOne(e => e.Gearbox).WithOne().HasForeignKey<Team>(e => e.GearboxId);
                    b.HasOne(e => e.Chassis).WithOne(e => e.Team).HasForeignKey<Chassis>(e => e.TeamId);
                });

            modelBuilder.Entity<TestDriver>();
            modelBuilder.Entity<TitleSponsor>()
                .OwnsOne(s => s.Details);

            // TODO: Sponsor * <-> * Team. Many-to-many relationships are not supported without CLR class for join table. See issue#1368
        }
    }
}
