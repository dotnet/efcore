// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.EF6.Models.Orders;
using System;
using System.Data.Entity;
using System.Linq;
using Xunit;

namespace EntityFramework.Microbenchmarks.EF6.ChangeTracker
{
    public class DbSetOperationTests
    {
        private static string _connectionString = String.Format(@"Server={0};Database=Perf_ChangeTracker_DbSetOperation;Integrated Security=True;MultipleActiveResultSets=true;", TestConfig.Instance.DataSource);

        [Fact]
        public void Add()
        {
            new TestDefinition
            {
                TestName = "ChangeTracker_DbSetOperation_Add_EF6",
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
                TestName = "ChangeTracker_DbSetOperation_AddCollection_EF6",
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
                                context.Entry(customer).State = EntityState.Modified;
                            }
                        }
                    }
                }
            }.RunTest();
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
            OrdersSeedData.EnsureCreated(
                _connectionString,
                productCount: 0,
                customerCount: 1000,
                ordersPerCustomer: 0,
                linesPerOrder: 0);
        }
    }
}
