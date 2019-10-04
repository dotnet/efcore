// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Entity;
using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders
{
    public class OrdersFixture : OrdersFixtureSeedBase
    {
        private readonly string _connectionString;
        private readonly int _productCount;
        private readonly int _customerCount;
        private readonly int _ordersPerCustomer;
        private readonly int _linesPerOrder;

        public OrdersFixture(string databaseName, int productCount, int customerCount,
            int ordersPerCustomer, int linesPerOrder, Action<DbContext> seedAction = null)
        {
            _connectionString = $"{BenchmarkConfig.Instance.BenchmarkDatabase}Database={databaseName};";
            _productCount = productCount;
            _customerCount = customerCount;
            _ordersPerCustomer = ordersPerCustomer;
            _linesPerOrder = linesPerOrder;

            EnsureDatabaseCreated(seedAction);
        }

        public virtual OrdersContext CreateContext()
        {
            return new OrdersContext(_connectionString);
        }

        private void EnsureDatabaseCreated(Action<DbContext> seedAction)
        {
            using (var context = CreateContext())
            {
                if (!context.Database.Exists())
                {
                    context.Database.Create();
                    InsertSeedData();
                    seedAction?.Invoke(context);
                }
                else if (!IsDatabaseCorrect(context))
                {
                    context.Database.Delete();
                    context.Database.Create();
                    InsertSeedData();
                    seedAction?.Invoke(context);
                }

                Assert.True(IsDatabaseCorrect(context));
            }
        }

        private bool IsDatabaseCorrect(OrdersContext context)
        {
            return context.Database.CompatibleWithModel(throwIfNoMetadata: true)
                && _productCount == context.Products.Count()
                && _customerCount == context.Customers.Count()
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
}
