// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Microbenchmarks.Core.Models.Orders;

namespace Microsoft.EntityFrameworkCore.Microbenchmarks.Models.Orders
{
    public class OrdersContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _connectionString;
        private readonly bool _disableBatching;

        public OrdersContext(string connectionString, bool disableBatching = false)
        {
            _connectionString = connectionString;
            _disableBatching = disableBatching;
        }

        public OrdersContext(IServiceProvider serviceProvider, string connectionString, bool disableBatching = false)
        {
            _serviceProvider = serviceProvider;
            _connectionString = connectionString;
            _disableBatching = disableBatching;
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(_serviceProvider)
                .UseSqlServer(
                    _connectionString,
                    b =>
                        {
                            if (_disableBatching)
                            {
                                b.MaxBatchSize(1);
                            }
                        });
    }
}
