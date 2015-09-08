// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace EntityFramework.Microbenchmarks.Core.Models.Orders
{
    public abstract class OrdersSeedDataBase
    {
        protected virtual List<OrderLine> CreateOrderLines(int linesPerOrder, List<Product> products, List<Order> orders)
        {
            var lines = new List<OrderLine>();
            for (var o = 0; o < orders.Count; o++)
            {
                for (var l = 0; l < linesPerOrder; l++)
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

        protected virtual List<Order> CreateOrders(int ordersPerCustomer, List<Customer> customers)
        {
            var orders = new List<Order>();
            foreach (var customer in customers)
            {
                for (var i = 0; i < ordersPerCustomer; i++)
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

        protected virtual List<Customer> CreateCustomers(int customerCount)
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

        protected virtual List<Product> CreateProducts(int productCount)
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
