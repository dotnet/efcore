// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindInMemoryContext(DbContextOptions options) : NorthwindContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CustomerQuery>().ToInMemoryQuery(
            () => Customers.Select(
                c => new CustomerQuery
                {
                    Address = c.Address,
                    City = c.City,
                    CompanyName = c.CompanyName,
                    ContactName = c.ContactName,
                    ContactTitle = c.ContactTitle
                }));

        modelBuilder.Entity<OrderQuery>().ToInMemoryQuery(
            () => Orders.Select(o => new OrderQuery { CustomerID = o.CustomerID }));

        modelBuilder.Entity<ProductQuery>().ToInMemoryQuery(
            () => Products.Where(p => !p.Discontinued)
                .Select(
                    p => new ProductQuery
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        CategoryName = "Food"
                    }));

        modelBuilder.Entity<CustomerQueryWithQueryFilter>().ToInMemoryQuery(
            () => Customers.Select(
                c => new CustomerQueryWithQueryFilter
                {
                    CompanyName = c.CompanyName,
                    OrderCount = c.Orders.Count(),
                    SearchTerm = SearchTerm
                }));
    }
}
