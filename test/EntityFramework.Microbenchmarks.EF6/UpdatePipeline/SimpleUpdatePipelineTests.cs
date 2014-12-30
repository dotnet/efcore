// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Core.Models.Orders;
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
                Run = Insert,
                Setup = EnsureDatabaseSetup
            }.RunTest();
        }

        private static void Insert(TestHarness harness)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                using (context.Database.BeginTransaction())
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        context.Customers.Add(new Customer { Name = "New Customer " + i });
                    }

                    harness.StartCollection();
                    var records = context.SaveChanges();
                    harness.StopCollection();

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
                Run = Update,
                Setup = EnsureDatabaseSetup
            }.RunTest();
        }

        private static void Update(TestHarness harness)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                using (context.Database.BeginTransaction())
                {
                    foreach (var customer in context.Customers)
                    {
                        customer.Name += " Modified";
                    }

                    harness.StartCollection();
                    var records = context.SaveChanges();
                    harness.StopCollection();

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
                Run = Delete,
                Setup = EnsureDatabaseSetup
            }.RunTest();
        }

        private static void Delete(TestHarness harness)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                using (context.Database.BeginTransaction())
                {
                    foreach (var customer in context.Customers)
                    {
                        context.Customers.Remove(customer);
                    }

                    harness.StartCollection();
                    var records = context.SaveChanges();
                    harness.StopCollection();

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
                Run = Mixed,
                Setup = EnsureDatabaseSetup
            }.RunTest();
        }

        private static void Mixed(TestHarness harness)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                using (context.Database.BeginTransaction())
                {
                    var customers = context.Customers.ToArray();

                    for (int i = 0; i < 333; i++)
                    {
                        context.Customers.Add(new Customer { Name = "New Customer " + i });
                    }

                    for (int i = 0; i < 1000; i += 3)
                    {
                        context.Customers.Remove(customers[i]);
                    }

                    for (int i = 1; i < 1000; i += 3)
                    {
                        customers[i].Name += " Modified";
                    }

                    harness.StartCollection();
                    var records = context.SaveChanges();
                    harness.StopCollection();

                    Assert.Equal(1000, records);
                }
            }
        }

        private static void EnsureDatabaseSetup()
        {
            new OrdersSeedData().EnsureCreated(
                _connectionString,
                productCount: 0,
                customerCount: 1000,
                ordersPerCustomer: 0,
                linesPerOrder: 0);
        }
    }
}
