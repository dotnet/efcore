// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindQuerySqliteFixture<TModelCustomizer> : NorthwindQueryRelationalFixture<TModelCustomizer>
    where TModelCustomizer : ITestModelCustomizer, new()
{
    protected override ITestStoreFactory TestStoreFactory
        => SqliteNorthwindTestStoreFactory.Instance;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        // NB: SQLite doesn't support decimal very well. Using double instead
        modelBuilder.Entity<OrderDetail>().Property(o => o.UnitPrice).HasConversion<double>();
        modelBuilder.Entity<Product>().Property(o => o.UnitPrice).HasConversion<double?>();
    }

    protected override Type ContextType
        => typeof(NorthwindSqliteContext);
}
