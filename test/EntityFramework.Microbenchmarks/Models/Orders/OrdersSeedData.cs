// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace EntityFramework.Microbenchmarks.Models.Orders
{
    public class OrdersSeedData
    {
        public static void EnsureCreated(
           string connectionString,
           int productCount,
           int customerCount,
           int ordersPerCustomer,
           int linesPerOrder)
        {
            using (var context = new OrdersContext(connectionString))
            {
                if (!context.Database.AsRelational().Exists())
                {
                    context.Database.EnsureCreated();
                    InsertSeedData(connectionString, productCount, customerCount, ordersPerCustomer, linesPerOrder);
                }

                Assert.Equal(productCount, context.Products.Count());
                Assert.Equal(customerCount, context.Customers.Count());
                Assert.Equal(customerCount * ordersPerCustomer, context.Orders.Count());
                Assert.Equal(customerCount * ordersPerCustomer * linesPerOrder, context.OrderLines.Count());
            }
        }

        public static void InsertSeedData(
            string connectionString, 
            int productCount, 
            int customerCount, 
            int ordersPerCustomer, 
            int linesPerOrder)
        {
            List<Product> products = CreateProducts(productCount);
            using (var context = new OrdersContext(connectionString))
            {
                context.Products.Add(products.ToArray());
                context.SaveChanges();
            }

            List<Customer> customers = CreateCustomers(customerCount);
            using (var context = new OrdersContext(connectionString))
            {
                context.Customers.Add(customers.ToArray());
                context.SaveChanges();
            }

            List<Order> orders = CreateOrders(ordersPerCustomer, customers);
            using (var context = new OrdersContext(connectionString))
            {
                context.Orders.Add(orders.ToArray());
                context.SaveChanges();
            }

            List<OrderLine> lines = CreateOrderLines(linesPerOrder, products, orders);

            using (var context = new OrdersContext(connectionString))
            {
                context.OrderLines.Add(lines.ToArray());
                context.SaveChanges();
            }
        }

        private static List<OrderLine> CreateOrderLines(int linesPerOrder, List<Product> products, List<Order> orders)
        {
            var lines = new List<OrderLine>();
            for (int o = 0; o < orders.Count; o++)
            {
                for (int l = 0; l < linesPerOrder; l++)
                {
                    var product = products[(o + l) % products.Count];
                    var quantity = l + 1;
                    lines.Add(new OrderLine
                    {
                        OrderId = orders[o].OrderId,
                        ProductId = product.ProductId,
                        Price = product.Retail * quantity,
                        Quantity = quantity
                    });
                }
            }

            return lines;
        }

        private static List<Order> CreateOrders(int ordersPerCustomer, List<Customer> customers)
        {
            var orders = new List<Order>();
            foreach (var customer in customers)
            {
                for (int i = 0; i < ordersPerCustomer; i++)
                {
                    orders.Add(new Order
                    {
                        CustomerId = customer.CustomerId,
                        Date = new DateTime(2000, 1, 1)
                    });
                }
            }

            return orders;
        }

        private static List<Customer> CreateCustomers(int customerCount)
        {
            var customers = new List<Customer>();
            for (var c = 0; c < customerCount; c++)
            {
                customers.Add(new Customer
                {
                    Name = "Customer " + c
                });
            }

            return customers;
        }

        private static List<Product> CreateProducts(int productCount)
        {
            var products = new List<Product>();
            for (var i = 0; i < productCount; i++)
            {
                products.Add(new Product
                {
                    Name = "Product " + i,
                    Retail = (i % 10) + 10
                });
            }

            return products;
        }
    }
}
