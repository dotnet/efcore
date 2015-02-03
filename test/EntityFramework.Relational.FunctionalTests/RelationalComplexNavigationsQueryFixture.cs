// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Relational.FunctionalTests
{
    public abstract class RelationalComplexNavigationsQueryFixture<TTestStore> : ComplexNavigationsQueryFixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
