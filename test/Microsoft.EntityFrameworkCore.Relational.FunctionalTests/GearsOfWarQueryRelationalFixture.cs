// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.FunctionalTests.TestModels.GearsOfWarModel;

namespace Microsoft.EntityFrameworkCore.FunctionalTests
{
    public abstract class GearsOfWarQueryRelationalFixture<TTestStore> : GearsOfWarQueryFixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<City>().Property(g => g.Location).HasColumnType("varchar(100)");
        }
    }
}
