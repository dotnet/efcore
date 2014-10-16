// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Cud.Model;
using Microsoft.Data.Entity;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Advanced;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;
#if K10
using Cud.Utilities;
#endif

namespace Cud
{
    public sealed class PocoCudTests
    {
        private string _connectionString;
        private string _connectionStringWithTcpIp;
        private static IServiceProvider _serviceProvider;

        public static IServiceProvider CreateServiceProvider(Configuration configuration)
        {
            var services = new ServiceCollection();
            services.AddEntityFramework().AddSqlServer().UseLoggerFactory<LoggerFactory>();
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

            try
            {
                configuration.AddJsonFile(@"LocalConfig.json");

                _connectionString = configuration.Get("Data:DefaultConnection:Connectionstring");
                if (_connectionString == null)
                {
                    Console.WriteLine("The required configuration value 'Data:DefaultConnection:Connectionstring' is not present in 'LocalConfig.json'");
                    return;
                }

                _connectionStringWithTcpIp = configuration.Get("Data:TcpIpConnection:Connectionstring");
                if (_connectionStringWithTcpIp == null)
                {
                    Console.WriteLine("The required configuration value 'Data:TcpIpConnection:Connectionstring' is not present in 'LocalConfig.json'");
                    return;
                }

                _serviceProvider = CreateServiceProvider(configuration);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading config: " + e.Message);
            }

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
                        orderLineEntry.State = EntityState.Deleted;
                    }

                    foreach (var orderEntry in context.ChangeTracker.Entries<Order>())
                    {
                        orderEntry.State = EntityState.Deleted;
                    }

                    foreach (var customerEntry in context.ChangeTracker.Entries<Customer>())
                    {
                        customerEntry.State = EntityState.Deleted;
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
                        orderLineEntry.State = EntityState.Deleted;
                    }

                    CudContext.InsertTestingData(context);

                    context.SaveChanges();
                }
            }
        }
    }
}