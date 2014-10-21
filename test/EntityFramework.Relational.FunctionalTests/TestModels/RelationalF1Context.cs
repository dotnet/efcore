// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests.TestModels.ConcurrencyModel;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Relational.FunctionalTests.TestModels
{
    public class RelationalF1Context : F1Context
    {
        public static readonly string DatabaseName = "OptimisticConcurrencyTest";

        public RelationalF1Context(DbContextOptions options)
            : base(options)
        {
        }

        public RelationalF1Context(IServiceProvider serviceProvider, DbContextOptions options)
            : base(serviceProvider, options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<Chassis>(b =>
                    {
                        b.Key(c => c.TeamId);
                        b.ForRelational().Table("Chassis");
                    });

            modelBuilder
                .Entity<Team>(b =>
                    {
                        b.Key(c => c.Id);
                        b.ForRelational().Table("Teams");
                    });

            modelBuilder
                .Entity<Driver>(b =>
                    {
                        b.Key(c => c.Id);
                        b.ForRelational().Table("Drivers");
                    });

            modelBuilder
                .Entity<Engine>(b =>
                    {
                        b.Key(c => c.Id);
                        b.ForRelational().Table("Engines");
                    });

            modelBuilder
                .Entity<EngineSupplier>(b =>
                    {
                        b.Key(c => c.Id);
                        b.ForRelational().Table("EngineSuppliers");
                    });

            modelBuilder
                .Entity<Gearbox>(b =>
                    {
                        b.Key(c => c.Id);
                        b.ForRelational().Table("Gearboxes");
                    });

            modelBuilder
                .Entity<Sponsor>(b =>
                    {
                        b.Key(c => c.Id);
                        b.ForRelational().Table("Sponsors");
                    });

            modelBuilder
                .Entity<TestDriver>(b =>
                    {
                        b.Key(c => c.Id);
                        b.ForRelational().Table("TestDrivers");
                    });

            modelBuilder
                .Entity<TitleSponsor>()
                .ForRelational()
                .Table("TitleSponsors");

            modelBuilder
                .Entity<Team>()
                .OneToOne(e => e.Chassis, e => e.Team);
        }
    }
}
