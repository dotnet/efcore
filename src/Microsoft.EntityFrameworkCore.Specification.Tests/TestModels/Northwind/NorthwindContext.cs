// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind
{
    public class NorthwindContext : DbContext
    {
        public static readonly string StoreName = "Northwind";

        public NorthwindContext(
            DbContextOptions options,
            QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll)
            : base(queryTrackingBehavior == QueryTrackingBehavior.TrackAll
                ? options
                : new DbContextOptionsBuilder(options).UseQueryTrackingBehavior(queryTrackingBehavior).Options)
        {
        }

        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<Product> Products { get; set; }
    }
}
