// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using EntityFramework.Microbenchmarks.Core.Models.Orders;
using Xunit;

namespace EntityFramework.Microbenchmarks.EF6.Models.Orders
{
    public class OrdersSeedData : OrdersSeedDataBase
    {
        private readonly string _connectionString;
        private readonly int _productCount;
        private readonly int _customerCount;
        private readonly int _ordersPerCustomer;
        private readonly int _linesPerOrder;

        public OrdersSeedData(
            string connectionString,
            int productCount,
            int customerCount,
            int ordersPerCustomer,
            int linesPerOrder)
        {
            _connectionString = connectionString;
            _productCount = productCount;
            _customerCount = customerCount;
            _ordersPerCustomer = ordersPerCustomer;
            _linesPerOrder = linesPerOrder;
        }

        public void EnsureCreated()
        {
            using (var context = new OrdersContext(_connectionString))
            {
                if (!context.Database.Exists())
                {
                    context.Database.Create();
                    InsertSeedData();
                }
                else if (!IsDatabaseCorrect(context))
                {
                    context.Database.Delete();
                    context.Database.Create();
                    InsertSeedData();
                }

                Assert.True(IsDatabaseCorrect(context));
            }
        }

        private bool IsDatabaseCorrect(OrdersContext context)
        {
            return _productCount == context.Products.Count()
                && _customerCount == context.Customers.Count()
                && _customerCount * _ordersPerCustomer == context.Orders.Count()
                && _customerCount * _ordersPerCustomer * _linesPerOrder == context.OrderLines.Count();
        }

        public void InsertSeedData()
        {
            var products = CreateProducts(_productCount);
            using (var context = new OrdersContext(_connectionString))
            {
                context.Products.AddRange(products);
                context.SaveChanges();
            }

            var customers = CreateCustomers(_customerCount);
            using (var context = new OrdersContext(_connectionString))
            {
                context.Customers.AddRange(customers);
                context.SaveChanges();
            }

            var orders = CreateOrders(_ordersPerCustomer, customers);
            using (var context = new OrdersContext(_connectionString))
            {
                context.Orders.AddRange(orders);
                context.SaveChanges();
            }

            var lines = CreateOrderLines(_linesPerOrder, products, orders);

            using (var context = new OrdersContext(_connectionString))
            {
                context.OrderLines.AddRange(lines);
                context.SaveChanges();
            }
        }
    }
}
