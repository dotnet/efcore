// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using EntityFramework.Microbenchmarks.Core.Models.Orders;
using Xunit;

namespace EntityFramework.Microbenchmarks.Models.Orders
{
    public class OrdersSeedData : OrdersSeedDataBase
    {
        public void EnsureCreated(
            string connectionString,
            int productCount,
            int customerCount,
            int ordersPerCustomer,
            int linesPerOrder)
        {
            using (var context = new OrdersContext(connectionString))
            {
                if (context.Database.EnsureCreated())
                {
                    InsertSeedData(connectionString, productCount, customerCount, ordersPerCustomer, linesPerOrder);
                }

                Assert.Equal(productCount, context.Products.Count());
                Assert.Equal(customerCount, context.Customers.Count());
                Assert.Equal(customerCount * ordersPerCustomer, context.Orders.Count());
                Assert.Equal(customerCount * ordersPerCustomer * linesPerOrder, context.OrderLines.Count());
            }
        }

        public void InsertSeedData(
            string connectionString,
            int productCount,
            int customerCount,
            int ordersPerCustomer,
            int linesPerOrder)
        {
            var products = CreateProducts(productCount);
            using (var context = new OrdersContext(connectionString))
            {
                context.Products.AddRange(products);
                context.SaveChanges();
            }

            var customers = CreateCustomers(customerCount);
            using (var context = new OrdersContext(connectionString))
            {
                context.Customers.AddRange(customers);
                context.SaveChanges();
            }

            var orders = CreateOrders(ordersPerCustomer, customers);
            using (var context = new OrdersContext(connectionString))
            {
                context.Orders.AddRange(orders);
                context.SaveChanges();
            }

            var lines = CreateOrderLines(linesPerOrder, products, orders);

            using (var context = new OrdersContext(connectionString))
            {
                context.OrderLines.AddRange(lines);
                context.SaveChanges();
            }
        }
    }
}
