// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Core.Models.Orders;
using EntityFramework.Microbenchmarks.Models.Orders;
using Xunit;

namespace EntityFramework.Microbenchmarks.ChangeTracker
{
    public class FixupTests
    {
        private static readonly string _connectionString 
            = $@"Server={TestConfig.Instance.DataSource};Database=Perf_ChangeTracker_Fixup;Integrated Security=True;MultipleActiveResultSets=true;";

        [Fact]
        public void AddChildren()
        {
            new TestDefinition
                {
                    TestName = "ChangeTracker_Fixup_AddChildren",
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

        [Fact]
        public void AddParents()
        {
            new TestDefinition
                {
                    TestName = "ChangeTracker_Fixup_AddParents",
                    IterationCount = 10,
                    WarmupCount = 5,
                    Run = harness =>
                        {
                            using (var context = new OrdersContext(_connectionString))
                            {
                                var customers = new List<Customer>();
                                for (var i = 0; i < 1000; i++)
                                {
                                    customers.Add(new Customer { CustomerId = i + 1 });
                                    context.Orders.Add(new Order { CustomerId = i + 1 });
                                }

                                foreach (var customer in customers)
                                {
                                    using (harness.StartCollection())
                                    {
                                        context.Customers.Add(customer);
                                    }

                                    Assert.Same(customer, customer.Orders.Single().Customer);
                                }
                            }
                        }
                }.RunTest();
        }

        [Fact]
        public void AttachChildren()
        {
            new TestDefinition
                {
                    TestName = "ChangeTracker_Fixup_AttachChildren",
                    IterationCount = 10,
                    WarmupCount = 5,
                    Setup = EnsureDatabaseSetup,
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
                    TestName = "ChangeTracker_Fixup_AttachParents",
                    IterationCount = 10,
                    WarmupCount = 5,
                    Setup = EnsureDatabaseSetup,
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
                    TestName = "ChangeTracker_Fixup_QueryChildren",
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
                    TestName = "ChangeTracker_Fixup_QueryParents",
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
            new OrdersSeedData().EnsureCreated(
                _connectionString,
                productCount: 1000,
                customerCount: 1000,
                ordersPerCustomer: 1,
                linesPerOrder: 1);
        }
    }
}
