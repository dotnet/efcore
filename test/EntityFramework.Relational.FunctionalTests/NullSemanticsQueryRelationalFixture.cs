// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.NullSemantics;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class NullSemanticsQueryRelationalFixture<TTestStore> 
        where TTestStore : TestStore
    {
        public abstract TTestStore CreateTestStore();

        public abstract NullSemanticsContext CreateContext(TTestStore testStore);

        protected virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.Id).StoreGeneratedPattern(StoreGeneratedPattern.None);

            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.StringA).Required();
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.StringB).Required();
            modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.StringC).Required();

            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.Id).StoreGeneratedPattern(StoreGeneratedPattern.None);

            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.StringA).Required();
            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.StringB).Required();
            modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.StringC).Required();
        }
    }
}
