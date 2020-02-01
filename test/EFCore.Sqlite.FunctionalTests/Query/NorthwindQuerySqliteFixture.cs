// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindQuerySqliteFixture<TModelCustomizer> : NorthwindQueryRelationalFixture<TModelCustomizer>
        where TModelCustomizer : IModelCustomizer, new()
    {
        protected override ITestStoreFactory TestStoreFactory => SqliteNorthwindTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            // NB: SQLite doesn't support decimal very well. Using double instead
            modelBuilder.Entity<OrderDetail>().Property(o => o.UnitPrice).HasConversion<double>();
            modelBuilder.Entity<Product>().Property(o => o.UnitPrice).HasConversion<double?>();
        }
    }
}
