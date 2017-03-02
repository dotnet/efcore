// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class NorthwindQueryRelationalFixture : NorthwindQueryFixtureBase
    {
        public override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Employee>().ToTable("Employees");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Product>().Ignore(p => p.SupplierID);
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OrderDetail>().ToTable("Order Details");
        }

        public abstract CancellationToken CancelQuery();
    }
}
