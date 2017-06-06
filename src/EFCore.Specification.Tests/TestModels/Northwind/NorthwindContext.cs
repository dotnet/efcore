// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// ReSharper disable StringStartsWithIsCultureSpecific

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind
{
    public class NorthwindContext : DbContext
    {
        public NorthwindContext(
            DbContextOptions options,
            QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll)
            : base(
                queryTrackingBehavior == QueryTrackingBehavior.TrackAll
                    ? options
                    : new DbContextOptionsBuilder(options).UseQueryTrackingBehavior(queryTrackingBehavior).Options)
        {
        }

        internal NorthwindContext()
        {
        }

        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<Product> Products { get; set; }

        public string TenantPrefix { get; set; } = "B";

        private short _quantity = 50;

        public void ConfigureFilters(ModelBuilder modelBuilder)
        {
            // Called explictly from filter test fixtures. Code is here
            // so we can capture TenantPrefix in filter exprs (simulates OnModelCreating).

            modelBuilder.Entity<Customer>().HasQueryFilter(c => c.CompanyName.StartsWith(TenantPrefix));
            modelBuilder.Entity<OrderDetail>().HasQueryFilter(od => EF.Property<short>(od, "Quantity") > _quantity);
            modelBuilder.Entity<Employee>().HasQueryFilter(e => e.Address.StartsWith("A"));
            modelBuilder.Entity<Product>().HasQueryFilter(p => ClientMethod(p));
        }

        private static bool ClientMethod(Product product)
        {
            return !product.Discontinued;
        }
    }
}
