// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.SqlClient;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Microbenchmarks.Core;
using Microsoft.EntityFrameworkCore.Microbenchmarks.Core.Models.Orders;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Microbenchmarks.Models.Orders
{
    public class OrdersFixture : OrdersFixtureBase
    {
        private readonly int _productCount;
        private readonly int _customerCount;
        private readonly int _ordersPerCustomer;
        private readonly int _linesPerOrder;

        public OrdersFixture(string databaseName, int productCount, int customerCount, int ordersPerCustomer, int linesPerOrder)
        {
            ConnectionString = $@"Server={BenchmarkConfig.Instance.BenchmarkDatabaseInstance};Database={databaseName};Integrated Security=True;MultipleActiveResultSets=true;";
            _productCount = productCount;
            _customerCount = customerCount;
            _ordersPerCustomer = ordersPerCustomer;
            _linesPerOrder = linesPerOrder;

            EnsureDatabaseCreated();
        }

        public string ConnectionString { get; }

        public virtual OrdersContext CreateContext() => new OrdersContext(ConnectionString);

        protected virtual void OnDatabaseCreated(OrdersContext context)
        {
        }

        private void EnsureDatabaseCreated()
        {
            using (var context = new OrdersContext(ConnectionString))
            {
                var database = context.GetService<IRelationalDatabaseCreator>();
                if (!database.Exists())
                {
                    context.Database.EnsureCreated();
                    InsertSeedData();
                    OnDatabaseCreated(context);
                }
                else if (!IsDatabaseCorrect(context))
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                    InsertSeedData();
                    OnDatabaseCreated(context);
                }

                Assert.True(IsDatabaseCorrect(context));
            }
        }

        private bool IsDatabaseCorrect(OrdersContext context)
        {
            try
            {
                context.Customers.FirstOrDefault();
                context.Products.FirstOrDefault();
                context.Orders.FirstOrDefault();
                context.OrderLines.FirstOrDefault();
            }
            catch (SqlException)
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
            using (var context = new OrdersContext(ConnectionString))
            {
                context.Products.AddRange(products);
                context.SaveChanges();
            }

            var customers = CreateCustomers(_customerCount, setPrimaryKeys: false);
            using (var context = new OrdersContext(ConnectionString))
            {
                context.Customers.AddRange(customers);
                context.SaveChanges();
            }

            var orders = CreateOrders(customers, _ordersPerCustomer, setPrimaryKeys: false);
            using (var context = new OrdersContext(ConnectionString))
            {
                context.Orders.AddRange(orders);
                context.SaveChanges();
            }

            var lines = CreateOrderLines(products, orders, _linesPerOrder, setPrimaryKeys: false);

            using (var context = new OrdersContext(ConnectionString))
            {
                context.OrderLines.AddRange(lines);
                context.SaveChanges();
            }
        }
    }
}
