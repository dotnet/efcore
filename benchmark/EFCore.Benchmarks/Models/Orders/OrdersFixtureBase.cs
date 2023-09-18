// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

public abstract class OrdersFixtureBase : OrdersFixtureSeedBase
{
    private IServiceProvider _serviceProvider;
    private int _productCount;
    private int _customerCount;
    private int _ordersPerCustomer;
    private int _linesPerOrder;

    public void Initialize(
        int productCount,
        int customerCount,
        int ordersPerCustomer,
        int linesPerOrder,
        Action<DbContext> seedAction = null)
    {
        _productCount = productCount;
        _customerCount = customerCount;
        _ordersPerCustomer = ordersPerCustomer;
        _linesPerOrder = linesPerOrder;

        EnsureDatabaseCreated(seedAction);
    }

    public void SetServiceProvider(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public abstract OrdersContextBase CreateContext(IServiceProvider serviceProvider = null, bool disableBatching = false);

    private void EnsureDatabaseCreated(Action<DbContext> seedAction)
    {
        using (var context = CreateContext())
        {
            var database = context.GetService<IRelationalDatabaseCreator>();
            if (!database.Exists())
            {
                context.Database.EnsureCreated();
                InsertSeedData();
                seedAction?.Invoke(context);
            }
            else if (!IsDatabaseCorrect(context))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                InsertSeedData();
                seedAction?.Invoke(context);
            }

            Assert.True(IsDatabaseCorrect(context));
        }
    }

    private bool IsDatabaseCorrect(OrdersContextBase context)
    {
        try
        {
            context.Customers.FirstOrDefault();
            context.Products.FirstOrDefault();
            context.Orders.FirstOrDefault();
            context.OrderLines.FirstOrDefault();
        }
        catch (DbException)
        {
            // Assume an exception means the schema is out of date
            return false;
        }

        return (_productCount == context.Products.Count())
            && (_customerCount == context.Customers.Count())
            && (_customerCount * _ordersPerCustomer == context.Orders.Count())
            && (_customerCount * _ordersPerCustomer * _linesPerOrder == context.OrderLines.Count());
    }

    private void InsertSeedData()
    {
        var products = CreateProducts(_productCount, setPrimaryKeys: false);
        using (var context = CreateContext())
        {
            context.Products.AddRange(products);
            context.SaveChanges();
        }

        var customers = CreateCustomers(_customerCount, setPrimaryKeys: false);
        using (var context = CreateContext())
        {
            context.Customers.AddRange(customers);
            context.SaveChanges();
        }

        var orders = CreateOrders(customers, _ordersPerCustomer, setPrimaryKeys: false);
        using (var context = CreateContext())
        {
            context.Orders.AddRange(orders);
            context.SaveChanges();
        }

        var lines = CreateOrderLines(products, orders, _linesPerOrder, setPrimaryKeys: false);

        using (var context = CreateContext())
        {
            context.OrderLines.AddRange(lines);
            context.SaveChanges();
        }
    }
}
