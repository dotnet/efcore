// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NullSemanticsQueryRelationalFixture : SharedStoreFixtureBase<NullSemanticsContext>
    {
        public new RelationalTestStore TestStore => (RelationalTestStore)base.TestStore;
        protected override string StoreName { get; } = "NullSemanticsQueryTest";
        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.Id).ValueGeneratedNever();

            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.StringA).IsRequired();
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.StringB).IsRequired();
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.StringC).IsRequired();

            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.Id).ValueGeneratedNever();

            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.StringA).IsRequired();
            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.StringB).IsRequired();
            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.StringC).IsRequired();
        }

        protected override void Seed(NullSemanticsContext context) => NullSemanticsContext.Seed(context);
    }
}
