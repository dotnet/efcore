// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Microbenchmarks.Core.Models.Orders;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;

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

        protected override void OnConfiguring(DbContextOptions builder)
        {
            var sqlBuilder = builder.UseSqlServer(_connectionString);

            if (_disableBatching)
            {
                sqlBuilder.MaxBatchSize(1);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>()
                .OneToMany(c => c.Orders, o => o.Customer)
                .ForeignKey(o => o.CustomerId);

            modelBuilder.Entity<Order>()
                .OneToMany(o => o.OrderLines, ol => ol.Order)
                .ForeignKey(ol => ol.OrderId);

            modelBuilder.Entity<Product>()
                .OneToMany(p => p.OrderLines, ol => ol.Product)
                .ForeignKey(ol => ol.ProductId);
        }
    }
}
