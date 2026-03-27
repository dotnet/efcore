// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

public abstract class OrdersContextBase(IServiceProvider serviceProvider) : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderLine> OrderLines { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => ConfigureProvider(optionsBuilder.UseInternalServiceProvider(serviceProvider));

    protected abstract void ConfigureProvider(DbContextOptionsBuilder optionsBuilder);
}
