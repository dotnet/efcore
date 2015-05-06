// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Entity;
using System.Linq;
using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Core.Models.Orders;
using EntityFramework.Microbenchmarks.EF6.Models.Orders;
using Xunit;

namespace EntityFramework.Microbenchmarks.EF6.ChangeTracker
{
    public class DbSetOperationTests
    {
        private static readonly string _connectionString 
            = $@"Server={TestConfig.Instance.DataSource};Database=Perf_ChangeTracker_DbSetOperation_EF6;Integrated Security=True;MultipleActiveResultSets=true;";

        [Fact]
        public void Add()
        {
            new TestDefinition
                {
                    TestName = "ChangeTracker_DbSetOperation_Add_EF6",
                    IterationCount = 100,
                    WarmupCount = 5,
                    Run = harness => Add(harness, true)
                }.RunTest();
        }

        [Fact]
        public void Add_AutoDetectChangesDisabled()
        {
            new TestDefinition
                {
                    TestName = "ChangeTracker_DbSetOperation_Add_AutoDetectChangesDisabled_EF6",
                    IterationCount = 100,
                    WarmupCount = 5,
                    Run = harness => Add(harness, false)
                }.RunTest();
        }

        public void Add(TestHarness harness, bool autoDetectChanges)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                var customers = new Customer[1000];
                for (var i = 0; i < customers.Length; i++)
                {
                    customers[i] = new Customer { Name = "Customer " + i };
                }

                context.Configuration.AutoDetectChangesEnabled = autoDetectChanges;
                using (harness.StartCollection())
                {
                    foreach (var customer in customers)
                    {
                        context.Customers.Add(customer);
                    }
                }
            }
        }

        [Fact]
        public void AddCollection()
        {
            new TestDefinition
                {
                    TestName = "ChangeTracker_DbSetOperation_AddCollection_EF6",
                    IterationCount = 100,
                    WarmupCount = 5,
                    Run = harness =>
                        {
                            using (var context = new OrdersContext(_connectionString))
                            {
                                var customers = new Customer[1000];
                                for (var i = 0; i < customers.Length; i++)
                                {
                                    customers[i] = new Customer { Name = "Customer " + i };
                                }

                                using (harness.StartCollection())
                                {
                                    context.Customers.AddRange(customers);
                                }
                            }
                        }
                }.RunTest();
        }

        [Fact]
        public void Attach()
        {
            new TestDefinition
                {
                    TestName = "ChangeTracker_DbSetOperation_Attach_EF6",
                    IterationCount = 100,
                    WarmupCount = 5,
                    Setup = EnsureDatabaseSetup,
                    Run = harness => Attach(harness, true)
                }.RunTest();
        }

        [Fact]
        public void Attach_AutoDetectChangesDisabled()
        {
            new TestDefinition
                {
                    TestName = "ChangeTracker_DbSetOperation_Attach_AutoDetectChangesDisabled_EF6",
                    IterationCount = 100,
                    WarmupCount = 5,
                    Setup = EnsureDatabaseSetup,
                    Run = harness => Attach(harness, false)
                }.RunTest();
        }

        public void Attach(TestHarness harness, bool autoDetectChanges)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                var customers = GetAllCustomersFromDatabase();
                Assert.Equal(1000, customers.Length);

                context.Configuration.AutoDetectChangesEnabled = autoDetectChanges;
                using (harness.StartCollection())
                {
                    foreach (var customer in customers)
                    {
                        context.Customers.Attach(customer);
                    }
                }
            }
        }

        // Note: AttachCollection() not implemented because there is no
        //       API for bulk attach in EF6.x

        [Fact]
        public void Remove()
        {
            new TestDefinition
                {
                    TestName = "ChangeTracker_DbSetOperation_Remove_EF6",
                    IterationCount = 100,
                    WarmupCount = 5,
                    Setup = EnsureDatabaseSetup,
                    Run = harness => Remove(harness, true)
                }.RunTest();
        }

        [Fact]
        public void Remove_AutoDetectChangesDisabled()
        {
            new TestDefinition
                {
                    TestName = "ChangeTracker_DbSetOperation_Remove_AutoDetectChangesDisabled_EF6",
                    IterationCount = 100,
                    WarmupCount = 5,
                    Setup = EnsureDatabaseSetup,
                    Run = harness => Remove(harness, false)
                }.RunTest();
        }

        public void Remove(TestHarness harness, bool autoDetectChanges)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                var customers = context.Customers.ToArray();
                Assert.Equal(1000, customers.Length);

                context.Configuration.AutoDetectChangesEnabled = autoDetectChanges;
                using (harness.StartCollection())
                {
                    foreach (var customer in customers)
                    {
                        context.Customers.Remove(customer);
                    }
                }
            }
        }

        [Fact]
        public void RemoveCollection()
        {
            new TestDefinition
                {
                    TestName = "ChangeTracker_DbSetOperation_RemoveCollection_EF6",
                    IterationCount = 100,
                    WarmupCount = 5,
                    Setup = EnsureDatabaseSetup,
                    Run = harness =>
                        {
                            using (var context = new OrdersContext(_connectionString))
                            {
                                var customers = context.Customers.ToArray();
                                Assert.Equal(1000, customers.Length);

                                using (harness.StartCollection())
                                {
                                    context.Customers.RemoveRange(customers);
                                }
                            }
                        }
                }.RunTest();
        }

        [Fact]
        public void Update()
        {
            new TestDefinition
                {
                    TestName = "ChangeTracker_DbSetOperation_Update_EF6",
                    IterationCount = 100,
                    WarmupCount = 5,
                    Setup = EnsureDatabaseSetup,
                    Run = harness => Update(harness, true)
                }.RunTest();
        }

        [Fact]
        public void Update_AutoDetectChangesDisabled()
        {
            new TestDefinition
                {
                    TestName = "ChangeTracker_DbSetOperation_Update_AutoDetectChangesDisabled_EF6",
                    IterationCount = 100,
                    WarmupCount = 5,
                    Setup = EnsureDatabaseSetup,
                    Run = harness => Update(harness, false)
                }.RunTest();
        }

        public void Update(TestHarness harness, bool autoDetectChanges)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                var customers = GetAllCustomersFromDatabase();
                Assert.Equal(1000, customers.Length);

                context.Configuration.AutoDetectChangesEnabled = autoDetectChanges;
                using (harness.StartCollection())
                {
                    foreach (var customer in customers)
                    {
                        context.Entry(customer).State = EntityState.Modified;
                    }
                }
            }
        }

        // Note: UpdateCollection() not implemented because there is no
        //       API for bulk update in EF6.x

        private static Customer[] GetAllCustomersFromDatabase()
        {
            using (var context = new OrdersContext(_connectionString))
            {
                return context.Customers.ToArray();
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
