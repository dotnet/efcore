// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.EF6.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace EntityFramework.Microbenchmarks.EF6.UpdatePipeline
{
    public class SimpleUpdatePipelineTests
    {
        private static string _connectionString = String.Format(@"Server={0};Database=Perf_UpdatePipeline_Simple_EF6;Integrated Security=True;MultipleActiveResultSets=true;", TestConfig.Instance.DataSource);

        [Fact]
        public void Insert()
        {
            new TestDefinition
            {
                TestName = "UpdatePipeline_Simple_Insert_EF6",
                IterationCount = 100,
                WarmupCount = 5,
                RunWithCollector = Insert,
                Setup = EnsureDatabaseSetup
            }.RunTest();
        }

        private static void Insert(MetricCollector collector)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                using (context.Database.BeginTransaction())
                {
                    var customers = new List<Customer>();
                    for (int i = 0; i < 1000; i++)
                    {
                        customers.Add(new Customer { Name = "Test Customer" });
                    }

                    int records;
                    using (collector.Start())
                    {
                        foreach (var customer in customers)
                        {
                            context.Customers.Add(customer);
                        }

                        records = context.SaveChanges();
                    }

                    Assert.Equal(1000, records);
                }
            }
        }

        [Fact]
        public void Update()
        {
            new TestDefinition
            {
                TestName = "UpdatePipeline_Simple_Update_EF6",
                IterationCount = 100,
                WarmupCount = 5,
                RunWithCollector = Update,
                Setup = EnsureDatabaseSetup
            }.RunTest();
        }

        private static void Update(MetricCollector collector)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                using (context.Database.BeginTransaction())
                {
                    int records;
                    var customers = context.Customers.ToList();
                    using (collector.Start())
                    {

                        foreach (var customer in customers)
                        {
                            customer.Name += " Modified";
                        }

                        records = context.SaveChanges();
                    }

                    Assert.Equal(1000, records);
                }
            }
        }

        [Fact]
        public void Delete()
        {
            new TestDefinition
            {
                TestName = "UpdatePipeline_Simple_Delete_EF6",
                IterationCount = 100,
                WarmupCount = 5,
                RunWithCollector = Delete,
                Setup = EnsureDatabaseSetup
            }.RunTest();
        }

        private static void Delete(MetricCollector collector)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                using (context.Database.BeginTransaction())
                {
                    int records;
                    var customers = context.Customers.ToList();
                    using (collector.Start())
                    {
                        foreach (var customer in customers)
                        {
                            context.Customers.Remove(customer);
                        }

                        records = context.SaveChanges();
                    }

                    Assert.Equal(1000, records);
                }
            }
        }

        [Fact]
        public void Mixed()
        {
            new TestDefinition
            {
                TestName = "UpdatePipeline_Simple_Mixed_EF6",
                IterationCount = 100,
                WarmupCount = 5,
                RunWithCollector = Mixed,
                Setup = EnsureDatabaseSetup
            }.RunTest();
        }

        private static void Mixed(MetricCollector collector)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                using (context.Database.BeginTransaction())
                {
                    int records;
                    var customers = context.Customers.ToArray();
                    var newCustomers = new List<Customer>();

                    for (int i = 0; i < 333; i++)
                    {
                        newCustomers.Add(new Customer { Name = "Test Customer" });
                    }

                    using (collector.Start())
                    {
                        for (int i = 0; i < 1000; i += 3)
                        {
                            context.Customers.Remove(customers[i]);
                        }

                        for (int i = 1; i < 1000; i += 3)
                        {
                            customers[i].Name += " Modified";
                        }

                        foreach (var customer in newCustomers)
                        {
                            context.Customers.Add(customer);
                        }

                        records = context.SaveChanges();
                    }

                    Assert.Equal(1000, records);
                }
            }
        }

        private static void EnsureDatabaseSetup()
        {
            OrdersSeedData.EnsureCreated(
                _connectionString,
                productCount: 0,
                customerCount: 1000,
                ordersPerCustomer: 0,
                linesPerOrder: 0);
        }
    }
}
