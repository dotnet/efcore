// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

public abstract class OrdersFixtureSeedBase
{
    public virtual List<Customer> CreateCustomers(int customerCount, bool setPrimaryKeys)
    {
        var customers = new List<Customer>();
        for (var c = 0; c < customerCount; c++)
        {
            customers.Add(
                new Customer
                {
                    CustomerId = setPrimaryKeys ? c + 1 : 0,
                    Title = c % 2 == 0 ? "Mr" : "Mrs",
                    FirstName = "Customer " + c,
                    LastName = "Customer " + c,
                    DateOfBirth = new DateTime(1980, c % 12 + 1, 1),
                    IsLoyaltyMember = c % 3 == 0,
                    Joined = new DateTime(2000, c % 12 + 1, 1),
                    OptedOutOfMarketing = c % 7 == 0,
                    Phone = "555-555-5555",
                    Email = $"customer{c}@sample.com",
                    AddressLineOne = $"{c} Sample St",
                    City = "Sampleville",
                    StateOrProvince = "SMP",
                    ZipOrPostalCode = "00000",
                    County = "United States"
                });
        }

        return customers;
    }

    public virtual List<Product> CreateProducts(int productCount, bool setPrimaryKeys)
    {
        var products = new List<Product>();
        for (var i = 0; i < productCount; i++)
        {
            products.Add(
                new Product
                {
                    ProductId = setPrimaryKeys ? i + 1 : 0,
                    Name = "Product " + i,
                    Description = "A product for testing purposes.",
                    SKU = "PROD" + i,
                    Retail = i % 10 + 10,
                    CurrentPrice = i % 10 + 10,
                    TargetStockLevel = i % 20,
                    ActualStockLevel = i % 7,
                    QuantityOnOrder = i % 3,
                    NextShipmentExpected = i % 3 == 0 ? null : DateTime.Today,
                    IsDiscontinued = i % 20 == 0
                });
        }

        return products;
    }

    public virtual List<Order> CreateOrders(List<Customer> customers, int ordersPerCustomer, bool setPrimaryKeys)
    {
        var orders = new List<Order>();
        for (var c = 0; c < customers.Count; c++)
        {
            for (var i = 0; i < ordersPerCustomer; i++)
            {
                orders.Add(
                    new Order
                    {
                        OrderId = setPrimaryKeys ? c * ordersPerCustomer + i + 1 : 0,
                        CustomerId = customers[c].CustomerId,
                        Date = new DateTime(2000, 1, 1),
                        OrderDiscount = i % 3,
                        DiscountReason = i % 3 == 0 ? null : "They seemed nice",
                        Tax = i % 10,
                        Addressee = "Person " + i,
                        AddressLineOne = $"{i} Sample St",
                        City = "Sampleville",
                        StateOrProvince = "SMP",
                        ZipOrPostalCode = "00000",
                        County = "United States"
                    });
            }
        }

        return orders;
    }

    public virtual List<OrderLine> CreateOrderLines(List<Product> products, List<Order> orders, int linesPerOrder, bool setPrimaryKeys)
    {
        var lines = new List<OrderLine>();
        for (var o = 0; o < orders.Count; o++)
        {
            for (var l = 0; l < linesPerOrder; l++)
            {
                var product = products[(o + l) % products.Count];
                var quantity = l + 1;
                lines.Add(
                    new OrderLine
                    {
                        OrderLineId = setPrimaryKeys ? l + 1 : 0,
                        OrderId = orders[o].OrderId,
                        ProductId = product.ProductId,
                        Price = product.Retail * quantity,
                        Quantity = quantity,
                        IsSubjectToTax = o % 3 == 0,
                        IsShipped = o % 5 == 0
                    });
            }
        }

        return lines;
    }
}
