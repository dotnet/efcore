// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests.TestModels.ConcurrencyModel;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class F1FixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        public abstract TTestStore CreateTestStore();

        public abstract F1Context CreateContext(TTestStore testStore);

        protected virtual void OnModelCreating(ModelBuilder model)
        {
            // TODO: Uncomment when complex types are supported
            //builder.ComplexType<Location>();
            model.Entity<Chassis>(b =>
                {
                    b.Key(c => c.TeamId);
                    b.Property(e => e.Version)
                        .StoreGeneratedPattern(StoreGeneratedPattern.Computed)
                        .ConcurrencyToken();
                });

            model.Entity<Driver>(b =>
                {
                    b.Property(e => e.Version)
                        .StoreGeneratedPattern(StoreGeneratedPattern.Computed)
                        .ConcurrencyToken();
                });

            model.Entity<Engine>(b =>
                {
                    b.Property(e => e.EngineSupplierId).ConcurrencyToken();
                    b.Property(e => e.Name).ConcurrencyToken();
                });

            // TODO: Complex type
            // .Property(c => c.StorageLocation);
            model.Ignore<Location>();

            model.Entity<EngineSupplier>();

            model.Entity<Gearbox>();

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

            model.Entity<Sponsor>(b =>
                {
                    b.Property(e => e.Version)
                        .StoreGeneratedPattern(StoreGeneratedPattern.Computed)
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
            model.Ignore<SponsorDetails>();

            model.Entity<Team>(b =>
                {
                    b.Property(t => t.Version)
                        .StoreGeneratedPattern(StoreGeneratedPattern.Computed)
                        .ConcurrencyToken();

                    b.Reference(e => e.Gearbox).InverseReference().ForeignKey<Team>(e => e.GearboxId);
                    b.Reference(e => e.Chassis).InverseReference(e => e.Team).ForeignKey<Chassis>(e => e.TeamId);
                });

            model.Entity<TestDriver>();

            model.Entity<TitleSponsor>();
            // TODO: Complex type
            // .Property(t => t.Details);

            // TODO: Sponsor * <-> * Team
        }
    }
}
