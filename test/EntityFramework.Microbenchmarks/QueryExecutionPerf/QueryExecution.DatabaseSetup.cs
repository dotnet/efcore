// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using EntityFramework.Microbenchmarks.QueryExecutionPerf.Model;
using Microsoft.Data.Entity;

namespace EntityFramework.Microbenchmarks.QueryExecutionPerf
{
    public partial class QueryExecutionBase
    {
        public void SetupDatabase(Func<DbContext> contextFactory)
        {
            using (var context = contextFactory())
            {
                context.Database.EnsureCreated();
                if (!context.Set<Customer>().Any())
                {
                    InsertTestingData(context);
                }
                //clear the SQL Server plan cache
                //TODO: context.ExecuteStoreCommand("DBCC FREEPROCCACHE WITH NO_INFOMSGS;");
            }
        }

        private void InsertTestingData(DbContext context)
        {
            const int customerCount = 1000;
            const int customerNameCount = 20;

            const int productPerCustomerCount = 2;
            const int photosPerProduct = 1;

            const int ordersPerCustomer = 1;

            for (var i = 0; i < customerCount; ++i)
            {
                var customer = new Customer
                {
                    Name = (i % customerNameCount).ToString(),
                    ContactInfo_Email = "email@domain.com",
                    ContactInfo_HomePhone_PhoneNumber = "425-999-9999",
                    ContactInfo_WorkPhone_PhoneNumber = "425-888-8888",
                    ContactInfo_MobilePhone_PhoneNumber = "425-777-7777",
                    Auditing_ModifiedBy = i.ToString(),
                    Auditing_Concurrency_Token = i.ToString(),
                    Auditing_ModifiedDate = System.DateTime.Now,
                };
                context.Set<Customer>().Add(customer);
                var login = new Login
                {
                    Customer = customer,
                    CustomerId = customer.CustomerId,
                    Username = customer.Name + customer.CustomerId,
                };
                customer.Logins.Add(login);
                for (var j = 0; j < productPerCustomerCount; ++j)
                {
                    var product = new Product
                        {
                            ProductId = (productPerCustomerCount * customer.CustomerId) + j,
                            Description = (i + "_" + j),
                            BaseConcurrency = i.ToString(),
                            ComplexConcurrency_Token = i.ToString(),
                            NestedComplexConcurrency_ModifiedBy = i.ToString(),
                            NestedComplexConcurrency_Concurrency_Token = i.ToString(),
                            NestedComplexConcurrency_ModifiedDate = DateTime.Now,
                        };
                    context.Set<Product>().Add(product);
                    for (var k = 0; k < photosPerProduct; ++k)
                    {
                        var photo = new ProductPhoto
                            {
                                PhotoId = (photosPerProduct * product.ProductId) + k,
                                Photo = new byte[] { 0, 1, 0, 1, 0, 1, 0, 1 },
                            };
                        product.Photos.Add(photo);
                    }
                }
                for (var l = 0; l < ordersPerCustomer; ++l)
                {
                    var order = new Order
                        {
                            OrderId = ((2 * ordersPerCustomer) * customer.CustomerId) + l,
                            CustomerId = customer.CustomerId,
                            Concurrency_Token = i.ToString(),
                        };
                    context.Set<Order>().Add(order);
                    for (var m = 0; m < productPerCustomerCount; ++m)
                    {
                        var line = new OrderLine
                            {
                                OrderId = order.OrderId,
                                ProductId = (productPerCustomerCount * customer.CustomerId) + m,
                                ConcurrencyToken = i.ToString(),
                            };
                        order.OrderLines.Add(line);
                    }
                }
                for (var l = 0; l < ordersPerCustomer; ++l)
                {
                    var order = new Order
                        {
                            OrderId = ((2 * ordersPerCustomer) * customer.CustomerId) + l + ordersPerCustomer,
                            CustomerId = customer.CustomerId,
                            Concurrency_Token = i.ToString(),
                        };
                    context.Set<Order>().Add(order);
                    for (var m = 0; m < productPerCustomerCount; ++m)
                    {
                        var backOrderLine = new BackOrderLine
                            {
                                OrderId = order.OrderId,
                                ProductId = (productPerCustomerCount * customer.CustomerId) + m,
                                ConcurrencyToken = i.ToString()
                            };
                        order.OrderLines.Add(backOrderLine);
                    }
                }
            }

            context.SaveChanges();
        }
    }
}
