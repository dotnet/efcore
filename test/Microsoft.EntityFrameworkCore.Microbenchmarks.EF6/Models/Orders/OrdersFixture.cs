// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Core.Models.Orders;
using Xunit;

namespace EntityFramework.Microbenchmarks.EF6.Models.Orders
{
    public class OrdersFixture : OrdersFixtureBase
    {
        private readonly string _connectionString;
        private readonly int _productCount;
        private readonly int _customerCount;
        private readonly int _ordersPerCustomer;
        private readonly int _linesPerOrder;

        public OrdersFixture(string databaseName, int productCount, int customerCount, int ordersPerCustomer, int linesPerOrder)
        {
            _connectionString = $@"Server={BenchmarkConfig.Instance.BenchmarkDatabaseInstance};Database={databaseName};Integrated Security=True;MultipleActiveResultSets=true;";
            _productCount = productCount;
            _customerCount = customerCount;
            _ordersPerCustomer = ordersPerCustomer;
            _linesPerOrder = linesPerOrder;

            EnsureDatabaseCreated();
        }

        public virtual OrdersContext CreateContext()
        {
            return new OrdersContext(_connectionString);
        }

        protected virtual void OnDatabaseCreated(OrdersContext context)
        { }

        private void EnsureDatabaseCreated()
        {
            using (var context = new OrdersContext(_connectionString))
            {
                if (!context.Database.Exists())
                {
                    context.Database.Create();
                    InsertSeedData();
                    OnDatabaseCreated(context);
                }
                else if (!IsDatabaseCorrect(context))
                {
                    context.Database.Delete();
                    context.Database.Create();
                    InsertSeedData();
                    OnDatabaseCreated(context);
                }

                Assert.True(IsDatabaseCorrect(context));
            }
        }

        private bool IsDatabaseCorrect(OrdersContext context)
        {
            return context.Database.CompatibleWithModel(throwIfNoMetadata: true)
                   && (_productCount == context.Products.Count())
                   && (_customerCount == context.Customers.Count())
                   && (_customerCount * _ordersPerCustomer == context.Orders.Count())
                   && (_customerCount * _ordersPerCustomer * _linesPerOrder == context.OrderLines.Count());
        }

        private void InsertSeedData()
        {
            var products = CreateProducts(_productCount, setPrimaryKeys: false);
            using (var context = new OrdersContext(_connectionString))
            {
                context.Products.AddRange(products);
                context.SaveChanges();
            }

            var customers = CreateCustomers(_customerCount, setPrimaryKeys: false);
            using (var context = new OrdersContext(_connectionString))
            {
                context.Customers.AddRange(customers);
                context.SaveChanges();
            }

            var orders = CreateOrders(customers, _ordersPerCustomer, setPrimaryKeys: false);
            using (var context = new OrdersContext(_connectionString))
            {
                context.Orders.AddRange(orders);
                context.SaveChanges();
            }

            var lines = CreateOrderLines(products, orders, _linesPerOrder, setPrimaryKeys: false);

            using (var context = new OrdersContext(_connectionString))
            {
                context.OrderLines.AddRange(lines);
                context.SaveChanges();
            }
        }
    }
}
