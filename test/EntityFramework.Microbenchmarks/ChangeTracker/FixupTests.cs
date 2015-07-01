// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
            = $@"Server={BenchmarkConfig.Instance.BenchmarkDatabaseInstance};Database=Perf_ChangeTracker_Fixup;Integrated Security=True;MultipleActiveResultSets=true;";

        public FixupTests()
        {
            new OrdersSeedData().EnsureCreated(
                _connectionString,
                productCount: 100,
                customerCount: 100,
                ordersPerCustomer: 1,
                linesPerOrder: 1);
        }

        [Benchmark(Iterations = 10, WarmupIterations = 5)]
        public void AddChildren(MetricCollector collector)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                var customers = context.Customers.ToList();
                Assert.Equal(100, customers.Count);

                foreach (var customer in customers)
                {
                    var order = new Order { CustomerId = customer.CustomerId };

                    using (collector.StartCollection())
                    {
                        context.Orders.Add(order);
                    }

                    Assert.Same(order, order.Customer.Orders.Single());
                }
            }
        }

        [Benchmark(Iterations = 10, WarmupIterations = 5)]
        public void AddParents(MetricCollector collector)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                var customers = new List<Customer>();
                for (var i = 0; i < 100; i++)
                {
                    customers.Add(new Customer { CustomerId = i + 1 });
                    context.Orders.Add(new Order { CustomerId = i + 1 });
                }

                foreach (var customer in customers)
                {
                    using (collector.StartCollection())
                    {
                        context.Customers.Add(customer);
                    }

                    Assert.Same(customer, customer.Orders.Single().Customer);
                }
            }
        }

        [Benchmark(Iterations = 10, WarmupIterations = 5)]
        public void AttachChildren(MetricCollector collector)
        {
            List<Order> orders;
            using (var context = new OrdersContext(_connectionString))
            {
                orders = context.Orders.ToList();
            }

            using (var context = new OrdersContext(_connectionString))
            {
                var customers = context.Customers.ToList();
                Assert.Equal(100, orders.Count);
                Assert.Equal(100, customers.Count);

                foreach (var order in orders)
                {
                    using (collector.StartCollection())
                    {
                        context.Orders.Attach(order);
                    }

                    Assert.Same(order, order.Customer.Orders.Single());
                }
            }
        }

        [Benchmark(Iterations = 10, WarmupIterations = 5)]
        public void AttachParents(MetricCollector collector)
        {
            List<Customer> customers;
            using (var context = new OrdersContext(_connectionString))
            {
                customers = context.Customers.ToList();
            }

            using (var context = new OrdersContext(_connectionString))
            {
                var orders = context.Orders.ToList();
                Assert.Equal(100, orders.Count);
                Assert.Equal(100, customers.Count);

                foreach (var customer in customers)
                {
                    using (collector.StartCollection())
                    {
                        context.Customers.Attach(customer);
                    }

                    Assert.Same(customer, customer.Orders.Single().Customer);
                }
            }
        }

        [Benchmark(Iterations = 10, WarmupIterations = 5)]
        public void QueryChildren(MetricCollector collector)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                context.Customers.ToList();

                collector.StartCollection();
                var orders = context.Orders.ToList();
                collector.StopCollection();

                Assert.Equal(100, context.ChangeTracker.Entries<Customer>().Count());
                Assert.Equal(100, context.ChangeTracker.Entries<Order>().Count());
                Assert.All(orders, o => Assert.NotNull(o.Customer));
            }
        }

        [Benchmark(Iterations = 10, WarmupIterations = 5)]
        public void QueryParents(MetricCollector collector)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                context.Orders.ToList();

                collector.StartCollection();
                var customers = context.Customers.ToList();
                collector.StopCollection();

                Assert.Equal(100, context.ChangeTracker.Entries<Customer>().Count());
                Assert.Equal(100, context.ChangeTracker.Entries<Order>().Count());
                Assert.All(customers, c => Assert.Equal(1, c.Orders.Count));
            }
        }
    }
}
