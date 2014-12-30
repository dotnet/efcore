// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using EntityFramework.Microbenchmarks.Core.Models.Orders;

namespace EntityFramework.Microbenchmarks.Models.Orders
{
    public class OrdersContext : DbContext
    {
        private readonly string _connectionString;

        public OrdersContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptions builder)
        {
            builder.UseSqlServer(_connectionString);
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
