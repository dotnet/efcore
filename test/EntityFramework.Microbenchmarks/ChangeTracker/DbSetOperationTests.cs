// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Core.Models.Orders;
using EntityFramework.Microbenchmarks.Models.Orders;
using System;
using System.Linq;
using Xunit;

namespace EntityFramework.Microbenchmarks.ChangeTracker
{
    public class DbSetOperationTests
    {
        private static string _connectionString = String.Format(@"Server={0};Database=Perf_ChangeTracker_DbSetOperation;Integrated Security=True;MultipleActiveResultSets=true;", TestConfig.Instance.DataSource);

        [Fact]
        public void Add()
        {
            new TestDefinition
            {
                TestName = "ChangeTracker_DbSetOperation_Add",
                IterationCount = 100,
                WarmupCount = 5,
                Run = harness =>
                {
                    using (var context = new OrdersContext(_connectionString))
                    {
                        var customers = new Customer[1000];
                        for (int i = 0; i < customers.Length; i++)
                        {
                            customers[i] = new Customer { Name = "Customer " + i };
                        }

                        using (harness.StartCollection())
                        {
                            foreach (var customer in customers)
                            {
                                context.Customers.Add(customer);
                            }
                        }
                    }
                }
            }.RunTest();
        }

        [Fact]
        public void AddCollection()
        {
            new TestDefinition
            {
                TestName = "ChangeTracker_DbSetOperation_AddCollection",
                IterationCount = 100,
                WarmupCount = 5,
                Run = harness =>
                {
                    using (var context = new OrdersContext(_connectionString))
                    {
                        var customers = new Customer[1000];
                        for (int i = 0; i < customers.Length; i++)
                        {
                            customers[i] = new Customer { Name = "Customer " + i };
                        }

                        using (harness.StartCollection())
                        {
                            context.Customers.Add(customers);
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
                TestName = "ChangeTracker_DbSetOperation_Attach",
                IterationCount = 100,
                WarmupCount = 5,
                Setup = EnsureDatabaseSetup,
                Run = harness =>
                {
                    using (var context = new OrdersContext(_connectionString))
                    {
                        var customers = GetAllCustomersFromDatabase();
                        Assert.Equal(1000, customers.Length);

                        using (harness.StartCollection())
                        {
                            foreach (var customer in customers)
                            {
                                context.Customers.Attach(customer);
                            }
                        }
                    }
                }
            }.RunTest();
        }

        [Fact]
        public void AttachCollection()
        {
            new TestDefinition
            {
                TestName = "ChangeTracker_DbSetOperation_AttachCollection",
                IterationCount = 100,
                WarmupCount = 5,
                Setup = EnsureDatabaseSetup,
                Run = harness =>
                {
                    using (var context = new OrdersContext(_connectionString))
                    {
                        var customers = GetAllCustomersFromDatabase();
                        Assert.Equal(1000, customers.Length);

                        using (harness.StartCollection())
                        {
                            context.Customers.Attach(customers);
                        }
                    }
                }
            }.RunTest();
        }

        [Fact]
        public void Remove()
        {
            new TestDefinition
            {
                TestName = "ChangeTracker_DbSetOperation_Remove",
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
                            foreach (var customer in customers)
                            {
                                context.Customers.Remove(customer);
                            }
                        }
                    }
                }
            }.RunTest();
        }

        [Fact]
        public void RemoveCollection()
        {
            new TestDefinition
            {
                TestName = "ChangeTracker_DbSetOperation_RemoveCollection",
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
                            context.Customers.Remove(customers);
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
                TestName = "ChangeTracker_DbSetOperation_Update",
                IterationCount = 100,
                WarmupCount = 5,
                Setup = EnsureDatabaseSetup,
                Run = harness =>
                {
                    using (var context = new OrdersContext(_connectionString))
                    {
                        var customers = GetAllCustomersFromDatabase();
                        Assert.Equal(1000, customers.Length);

                        using (harness.StartCollection())
                        {
                            foreach (var customer in customers)
                            {
                                context.Customers.Update(customer);
                            }
                        }
                    }
                }
            }.RunTest();
        }

        [Fact]
        public void UpdateCollection()
        {
            new TestDefinition
            {
                TestName = "ChangeTracker_DbSetOperation_UpdateCollection",
                IterationCount = 100,
                WarmupCount = 5,
                Setup = EnsureDatabaseSetup,
                Run = harness =>
                {
                    using (var context = new OrdersContext(_connectionString))
                    {
                        var customers = GetAllCustomersFromDatabase();
                        Assert.Equal(1000, customers.Length);

                        using (harness.StartCollection())
                        {
                            context.Customers.Update(customers);
                        }
                    }
                }
            }.RunTest();
        }

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
