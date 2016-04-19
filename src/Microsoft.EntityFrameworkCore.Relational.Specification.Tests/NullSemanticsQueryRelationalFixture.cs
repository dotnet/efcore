// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.NullSemanticsModel;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class NullSemanticsQueryRelationalFixture<TTestStore>
        where TTestStore : TestStore
    {
        public abstract TTestStore CreateTestStore();

        public abstract NullSemanticsContext CreateContext(TTestStore testStore, bool useRelationalNulls);

        protected virtual void OnModelCreating(ModelBuilder modelBuilder)
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
    }
}
