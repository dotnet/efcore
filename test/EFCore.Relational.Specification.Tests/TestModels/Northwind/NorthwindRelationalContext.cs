// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind;

#nullable disable

public abstract class NorthwindRelationalContext : NorthwindContext
{
    protected NorthwindRelationalContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>().ToTable("Customers");
        modelBuilder.Entity<Employee>().ToTable("Employees");
        modelBuilder.Entity<Product>().ToTable("Products");
        modelBuilder.Entity<Order>().ToTable("Orders");
        modelBuilder.Entity<OrderDetail>().ToTable("Order Details");

        modelBuilder.Entity<CustomerOrderHistory>().HasKey(coh => coh.ProductName);
        modelBuilder.Entity<MostExpensiveProduct>().HasKey(mep => mep.TenMostExpensiveProducts);

        modelBuilder.Entity<CustomerQuery>().ToSqlQuery(
            "SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]");

        modelBuilder.Entity<OrderQuery>().ToSqlQuery(@"select * from ""Orders""");
        modelBuilder.Entity<ProductView>().ToView("Alphabetical list of products");
    }
}
