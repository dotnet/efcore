// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.EF6.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace EntityFramework.Microbenchmarks.EF6.ChangeTracker
{
    public class FixupTests
    {
        private static string _connectionString = String.Format(@"Server={0};Database=Perf_ChangeTracker_Fixup_EF6;Integrated Security=True;MultipleActiveResultSets=true;", TestConfig.Instance.DataSource);

        [Fact]
        public void AddChildren()
        {
            new TestDefinition
            {
                TestName = "ChangeTracker_Fixup_AddChildren_EF6",
                IterationCount = 10,
                WarmupCount = 5,
                Setup = EnsureDatabaseSetup,
                Run = harness =>
                {
                    using (var context = new OrdersContext(_connectionString))
                    {
                        var customers = context.Customers.ToList();
                        Assert.Equal(1000, customers.Count);

                        foreach (var customer in customers)
                        {
                            var order = new Order { CustomerId = customer.CustomerId };

                            using (harness.StartCollection())
                            {
                                context.Orders.Add(order);
                            }

                            Assert.Same(order, order.Customer.Orders.Single());
                        }
                    }
                }
            }.RunTest();
        }

        // Note: AddParents() not implemented because fixup to added parents 
        //       only happens during SaveChanges for EF6.x (not during Add)

        [Fact]
        public void AttachChildren()
        {
            new TestDefinition
            {
                TestName = "ChangeTracker_Fixup_AttachChildren_EF6",
                IterationCount = 10,
                WarmupCount = 5,
                Run = harness =>
                {
                    List<Order> orders;
                    using (var context = new OrdersContext(_connectionString))
                    {
                        orders = context.Orders.ToList();
                    }

                    using (var context = new OrdersContext(_connectionString))
                    {
                        var customers = context.Customers.ToList();
                        Assert.Equal(1000, orders.Count);
                        Assert.Equal(1000, customers.Count);

                        foreach (var order in orders)
                        {
                            using (harness.StartCollection())
                            {
                                context.Orders.Attach(order);
                            }

                            Assert.Same(order, order.Customer.Orders.Single());
                        }
                    }
                }
            }.RunTest();
        }

        [Fact]
        public void AttachParents()
        {
            new TestDefinition
            {
                TestName = "ChangeTracker_Fixup_AttachParents_EF6",
                IterationCount = 10,
                WarmupCount = 5,
                Run = harness =>
                {
                    List<Customer> customers;
                    using (var context = new OrdersContext(_connectionString))
                    {
                        customers = context.Customers.ToList();
                    }

                    using (var context = new OrdersContext(_connectionString))
                    {
                        var orders = context.Orders.ToList();
                        Assert.Equal(1000, orders.Count);
                        Assert.Equal(1000, customers.Count);

                        foreach (var customer in customers)
                        {
                            using (harness.StartCollection())
                            {
                                context.Customers.Attach(customer);
                            }

                            Assert.Same(customer, customer.Orders.Single().Customer);
                        }
                    }
                }
            }.RunTest();
        }

        [Fact]
        public void QueryChildren()
        {
            new TestDefinition
            {
                TestName = "ChangeTracker_Fixup_QueryChildren_EF6",
                IterationCount = 10,
                WarmupCount = 5,
                Setup = EnsureDatabaseSetup,
                Run = harness =>
                {
                    using (var context = new OrdersContext(_connectionString))
                    {
                        context.Customers.ToList();

                        harness.StartCollection();
                        var orders = context.Orders.ToList();
                        harness.StopCollection();

                        Assert.Equal(1000, context.ChangeTracker.Entries<Customer>().Count());
                        Assert.Equal(1000, context.ChangeTracker.Entries<Order>().Count());
                        Assert.All(orders, o => Assert.NotNull(o.Customer));
                    }
                }
            }.RunTest();
        }

        [Fact]
        public void QueryParents()
        {
            new TestDefinition
            {
                TestName = "ChangeTracker_Fixup_QueryParents_EF6",
                IterationCount = 10,
                WarmupCount = 5,
                Setup = EnsureDatabaseSetup,
                Run = harness =>
                {
                    using (var context = new OrdersContext(_connectionString))
                    {
                        context.Orders.ToList();

                        harness.StartCollection();
                        var customers = context.Customers.ToList();
                        harness.StopCollection();

                        Assert.Equal(1000, context.ChangeTracker.Entries<Customer>().Count());
                        Assert.Equal(1000, context.ChangeTracker.Entries<Order>().Count());
                        Assert.All(customers, c => Assert.Equal(1, c.Orders.Count));
                    }
                }
            }.RunTest();
        }

        private static void EnsureDatabaseSetup()
        {
            OrdersSeedData.EnsureCreated(
                _connectionString,
                productCount: 1000,
                customerCount: 1000,
                ordersPerCustomer: 1,
                linesPerOrder: 1);
        }
    }
}
