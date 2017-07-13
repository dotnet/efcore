// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class F1FixtureBase : SharedStoreFixtureBase<F1Context>
    {
        protected override string StoreName { get; } = "F1Test";
        
        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
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
                .Ignore(s => s.Details);
            // #8973
            //.OwnsOne(s => s.Details);

            // TODO: Sponsor * <-> * Team. Many-to-many relationships are not supported without CLR class for join table. See issue #1368
        }

        protected override void Seed(F1Context context) => F1Context.Seed(context);
    }
}
