// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind
{
    public class NorthwindRelationalContext : NorthwindContext
    {
        public NorthwindRelationalContext(DbContextOptions options)
            : base(options)
        {
        }

        public string _empty = string.Empty;

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

            modelBuilder.Entity<CustomerView>().HasNoKey().ToQuery(
                () => CustomerQueries.FromSqlInterpolated(
                    $"SELECT [c].[CustomerID] + {_empty} as [CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]"
                ));

            modelBuilder
                .Entity<OrderQuery>()
                .HasNoKey()
                .ToQuery(
                    () => Orders
                        .FromSqlRaw(@"select * from ""Orders""")
                        .Select(
                            o => new OrderQuery { CustomerID = o.CustomerID }));

            modelBuilder.Entity<ProductQuery>().HasNoKey().ToView("Alphabetical list of products");
        }
    }
}
