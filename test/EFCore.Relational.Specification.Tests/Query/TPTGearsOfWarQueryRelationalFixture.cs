// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class TPTGearsOfWarQueryRelationalFixture : GearsOfWarQueryFixtureBase
    {
        protected override string StoreName { get; } = "TPTGearsOfWarQueryTest";

        public new RelationalTestStore TestStore
            => (RelationalTestStore)base.TestStore;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override bool ShouldLogCategory(string logCategory)
            => logCategory == DbLoggerCategory.Query.Name;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Gear>().ToTable("Gears");
            modelBuilder.Entity<Officer>().ToTable("Officers");

            modelBuilder.Entity<Faction>().ToTable("Factions");
            modelBuilder.Entity<LocustHorde>().ToTable("LocustHordes");

            modelBuilder.Entity<LocustLeader>().ToTable("LocustLeaders");
            modelBuilder.Entity<LocustCommander>().ToTable("LocustCommanders");
        }
    }
}
