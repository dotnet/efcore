// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using EntityFramework.Microbenchmarks.CudPerf.Model;
using Microsoft.Data.Entity;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

#if K10
using Cud.Utilities;
#endif

namespace EntityFramework.Microbenchmarks.CudPerf
{
    public sealed class PocoCudTests
    {
        private string _connectionString =          @"Data Source={0};Integrated Security=True;MultipleActiveResultSets=true;Initial Catalog=EF7_Cud;";
        private string _connectionStringWithTcpIp = @"Data Source={0};Integrated Security=True;MultipleActiveResultSets=true;Network Library=dbmssocn;Initial Catalog=EF7_Cud;";
        private static IServiceProvider _serviceProvider;

        public static IServiceProvider CreateServiceProvider(Configuration configuration)
        {
            var services = new ServiceCollection();
            services.AddEntityFramework().AddSqlServer();
            services.AddInstance<IConfiguration>(configuration);
            return services.BuildServiceProvider();
        }

        public static CudContext CreateContext(string connectionString)
        {
            var options = new DbContextOptions();
            return new CudContext(connectionString, _serviceProvider, options);
        }

        public void Setup()
        {
            var configuration = new Configuration();

            configuration.AddJsonFile(@"LocalConfig.json");
            _connectionString = string.Format(_connectionString, TestConfig.Instance.DataSource);
            _connectionStringWithTcpIp = string.Format(_connectionStringWithTcpIp, TestConfig.Instance.DataSource);

            _serviceProvider = CreateServiceProvider(configuration);
            CudContext.SetupDatabase(CreateContext(_connectionString));
        }

        //[Test("PocoCUD_Create",
        //    Description = "Poco CUD Test Create",
        //    WarmupIterations = 5,
        //    TestIterations = 5000,
        //    Priority = TestPriority.BVT)]
        public void PocoCUD_Create()
        {
            PocoCUD_Create(_connectionString);
        }

        //[Test("PocoCUD_Create_TCPIP",
        //    Description = "Poco CUD Test Create With TCP/IP Connection String",
        //    WarmupIterations = 5,
        //    TestIterations = 5000,
        //    Priority = TestPriority.Low)]
        public void PocoCUD_Create_TCPIP()
        {
            PocoCUD_Create(_connectionStringWithTcpIp);
        }

        private void PocoCUD_Create(string connectionString)
        {
            using (var context = CreateContext(connectionString))
            {
                using (context.Database.AsRelational().Connection.BeginTransaction())
                {
                    var customer = new Customer { Name = "New Customer" };
                    var order = new Order { Date = DateTime.Today };
                    var orderLine = new OrderLine { Price = 123.45m, Quantity = 42 };
                    var product = new Product { Name = "New Product" };

                    customer.Orders.Add(order);
                    order.OrderLines.Add(orderLine);
                    orderLine.Product = product;

                    context.Customers.Add(customer);

                    context.SaveChanges();
                }
            }
        }

        //[Test("PocoCUD_Update",
        //    Description = "Poco CUD Test Update",
        //    WarmupIterations = 5,
        //    TestIterations = 5000,
        //    Priority = TestPriority.BVT)]
        public void PocoCUD_Update()
        {
            PocoCUD_Update(_connectionString);
        }

        //[Test("PocoCUD_Update_TCPIP",
        //    Description = "Poco CUD Test Update With TCP/IP Connection String",
        //    WarmupIterations = 5,
        //    TestIterations = 5000,
        //    Priority = TestPriority.Low)]
        public void PocoCUD_Update_TCPIP()
        {
            PocoCUD_Update(_connectionStringWithTcpIp);
        }

        private void PocoCUD_Update(string connectionString)
        {
            using (var context = CreateContext(connectionString))
            {
                using (context.Database.AsRelational().Connection.BeginTransaction())
                {
                    var customers = context.Customers.ToList();

                    foreach (var customer in customers)
                    {
                        customer.Name = "Updated Name";
                        var orders = customer.Orders.ToList();
                        foreach (var order in orders)
                        {
                            order.Date = DateTime.Now;
                            var orderLines = order.OrderLines.ToList();
                            foreach (var orderLine in orderLines)
                            {
                                orderLine.Price = orderLine.Price * 2;
                                orderLine.Quantity = orderLine.Quantity * 2;
                            }
                        }
                    }
                    context.SaveChanges();
                }
            }
        }

        //[Test("PocoCUD_Delete",
        //    Description = "Poco CUD Test Delete",
        //    WarmupIterations = 5,
        //    TestIterations = 500,
        //    Priority = TestPriority.BVT)]
        public void PocoCUD_Delete()
        {
            PocoCUD_Delete(_connectionString);
        }

        //[Test("PocoCUD_Delete_TCPIP",
        //    Description = "Poco CUD Test Delete With TCP/IP Connection String",
        //    WarmupIterations = 5,
        //    TestIterations = 500,
        //    Priority = TestPriority.Low)]
        public void PocoCUD_Delete_TCPIP()
        {
            PocoCUD_Delete(_connectionStringWithTcpIp);
        }

        private void PocoCUD_Delete(string connectionString)
        {
            using (var context = CreateContext(connectionString))
            {
                using (context.Database.AsRelational().Connection.BeginTransaction())
                {
                    foreach (var orderLineEntry in context.ChangeTracker.Entries<OrderLine>())
                    {
                        orderLineEntry.SetState(EntityState.Deleted);
                    }

                    foreach (var orderEntry in context.ChangeTracker.Entries<Order>())
                    {
                        orderEntry.SetState(EntityState.Deleted);
                    }

                    foreach (var customerEntry in context.ChangeTracker.Entries<Customer>())
                    {
                        customerEntry.SetState(EntityState.Deleted);
                    }

                    context.SaveChanges();
                }
            }
        }

        //[Test("PocoCUD_Batch",
        //    Description = "Poco CUD Test Create/Delete/Update",
        //    WarmupIterations = 5,
        //    TestIterations = 200,
        //    Priority = TestPriority.BVT)]
        public void PocoCUD_Batch()
        {
            PocoCUD_Batch(_connectionString);
        }

        //[Test("PocoCUD_Batch_TCPIP",
        //    Description = "Poco CUD Test Create/Delete/Update With TCP/IP Connection String",
        //    WarmupIterations = 5,
        //    TestIterations = 200,
        //    Priority = TestPriority.Low)]
        public void PocoCUD_Batch_TCPIP()
        {
            PocoCUD_Batch(_connectionStringWithTcpIp);
        }

        private void PocoCUD_Batch(string connectionString)
        {
            using (var context = CreateContext(connectionString))
            {
                using (context.Database.AsRelational().Connection.BeginTransaction())
                {
                    var customers = context.Customers.ToList();
                    foreach (var customer in customers)
                    {
                        customer.Name = "Updated Name";
                    }

                    foreach (var orderLineEntry in context.ChangeTracker.Entries<OrderLine>())
                    {
                        orderLineEntry.SetState(EntityState.Deleted);
                    }

                    CudContext.InsertTestingData(context);

                    context.SaveChanges();
                }
            }
        }
    }
}
