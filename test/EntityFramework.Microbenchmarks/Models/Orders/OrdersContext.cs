// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Microbenchmarks.Core.Models.Orders;
using Microsoft.Data.Entity;

namespace EntityFramework.Microbenchmarks.Models.Orders
{
    public class OrdersContext : DbContext
    {
        private readonly string _connectionString;
        private readonly bool _disableBatching;

        public OrdersContext(string connectionString, bool disableBatching = false)
        {
            _connectionString = connectionString;
            _disableBatching = disableBatching;
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var sqlBuilder = optionsBuilder.UseSqlServer(_connectionString);

            if (_disableBatching)
            {
                sqlBuilder.MaxBatchSize(1);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().Collection(c => c.Orders).InverseReference(o => o.Customer)
                .ForeignKey(o => o.CustomerId);

            modelBuilder.Entity<Order>().Collection(o => o.OrderLines).InverseReference(ol => ol.Order)
                .ForeignKey(ol => ol.OrderId);

            modelBuilder.Entity<Product>().Collection(p => p.OrderLines).InverseReference(ol => ol.Product)
                .ForeignKey(ol => ol.ProductId);
        }
    }
}
