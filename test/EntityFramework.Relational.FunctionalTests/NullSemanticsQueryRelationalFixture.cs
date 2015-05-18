// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.NullSemantics;

namespace Microsoft.Data.Entity.Relational.FunctionalTests
{
    public abstract class NullSemanticsQueryRelationalFixture<TTestStore> 
        where TTestStore : TestStore
    {
        public abstract TTestStore CreateTestStore();

        public abstract NullSemanticsContext CreateContext(TTestStore testStore);

        protected virtual void OnModelCreating(ModelBuilder model)
        {
            model.Entity<NullSemanticsEntity1>().Key(e => e.Id);
            model.Entity<NullSemanticsEntity1>().Property(e => e.Id).GenerateValueOnAdd(false);

            model.Entity<NullSemanticsEntity1>().Property(e => e.StringA).Required(true);
            model.Entity<NullSemanticsEntity1>().Property(e => e.StringB).Required(true);
            model.Entity<NullSemanticsEntity1>().Property(e => e.StringC).Required(true);

            model.Entity<NullSemanticsEntity2>().Key(e => e.Id);
            model.Entity<NullSemanticsEntity2>().Property(e => e.Id).GenerateValueOnAdd(false);

            model.Entity<NullSemanticsEntity2>().Property(e => e.StringA).Required(true);
            model.Entity<NullSemanticsEntity2>().Property(e => e.StringB).Required(true);
            model.Entity<NullSemanticsEntity2>().Property(e => e.StringC).Required(true);
        }
    }
}
