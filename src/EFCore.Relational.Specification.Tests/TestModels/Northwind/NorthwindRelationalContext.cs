// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind
{
    public class NorthwindRelationalContext : NorthwindContext
    {
        public NorthwindRelationalContext(DbContextOptions options)
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
        }
    }
}
