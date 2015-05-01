// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind
{
    public class NorthwindContext : DbContext
    {
        public static readonly string StoreName = "Northwind";

        public NorthwindContext(IServiceProvider serviceProvider, DbContextOptions options)
            : base(serviceProvider, options)
        {
        }

        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<Product> Products { get; set; }
    }
}
