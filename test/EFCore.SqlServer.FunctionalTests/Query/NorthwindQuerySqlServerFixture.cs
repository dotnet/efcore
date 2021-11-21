// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindQuerySqlServerFixture<TModelCustomizer> : NorthwindQueryRelationalFixture<TModelCustomizer>
    where TModelCustomizer : IModelCustomizer, new()
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerNorthwindTestStoreFactory.Instance;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<Customer>()
            .Property(c => c.CustomerID)
            .HasColumnType("nchar(5)");

        modelBuilder.Entity<Employee>(
            b =>
            {
                b.Property(c => c.EmployeeID).HasColumnType("int");
                b.Property(c => c.ReportsTo).HasColumnType("int");
            });

        modelBuilder.Entity<Order>(
            b =>
            {
                b.Property(o => o.EmployeeID).HasColumnType("int");
                b.Property(o => o.OrderDate).HasColumnType("datetime");
            });

        modelBuilder.Entity<OrderDetail>()
            .Property(od => od.UnitPrice)
            .HasColumnType("money");

        modelBuilder.Entity<Product>(
            b =>
            {
                b.Property(p => p.UnitPrice).HasColumnType("money");
                b.Property(p => p.UnitsInStock).HasColumnType("smallint");
            });

        modelBuilder.Entity<MostExpensiveProduct>()
            .Property(p => p.UnitPrice)
            .HasColumnType("money");
    }
}
