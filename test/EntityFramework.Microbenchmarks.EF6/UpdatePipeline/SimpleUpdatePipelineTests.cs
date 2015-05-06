// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Core.Models.Orders;
using EntityFramework.Microbenchmarks.EF6.Models.Orders;
using Xunit;

namespace EntityFramework.Microbenchmarks.EF6.UpdatePipeline
{
    public class SimpleUpdatePipelineTests
    {
        private static readonly string _connectionString 
            = $@"Server={TestConfig.Instance.DataSource};Database=Perf_UpdatePipeline_Simple_EF6;Integrated Security=True;MultipleActiveResultSets=true;";

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
                    for (var i = 0; i < 1000; i++)
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

                    for (var i = 0; i < 333; i++)
                    {
                        context.Customers.Add(new Customer { Name = "New Customer " + i });
                    }

                    for (var i = 0; i < 1000; i += 3)
                    {
                        context.Customers.Remove(customers[i]);
                    }

                    for (var i = 1; i < 1000; i += 3)
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
