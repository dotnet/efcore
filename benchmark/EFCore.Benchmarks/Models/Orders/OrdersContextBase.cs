// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders
{
    public abstract class OrdersContextBase : DbContext
    {
        private readonly IServiceProvider _serviceProvider;

        protected OrdersContextBase(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ConfigureProvider(optionsBuilder.UseInternalServiceProvider(_serviceProvider));
        }

        protected abstract void ConfigureProvider(DbContextOptionsBuilder optionsBuilder);
    }
}
