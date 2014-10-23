// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Metadata;
using Northwind;

namespace Microsoft.Data.Entity.Relational.FunctionalTests
{
    public abstract class NorthwindQueryFixtureRelationalBase : NorthwindQueryFixtureBase
    {
        protected override void OnModelCreating(BasicModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().ForRelational().Table("Customers");
            modelBuilder.Entity<Employee>().ForRelational().Table("Employees");
            modelBuilder.Entity<Product>().ForRelational().Table("Products");
            modelBuilder.Entity<Order>().ForRelational().Table("Orders");
            modelBuilder.Entity<OrderDetail>().ForRelational().Table("Order Details");
        }
    }
}
