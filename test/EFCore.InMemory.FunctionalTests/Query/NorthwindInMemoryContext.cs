// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindInMemoryContext : NorthwindContext
    {
        public NorthwindInMemoryContext(DbContextOptions options)
            : base(options)
        {
        }

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
}
