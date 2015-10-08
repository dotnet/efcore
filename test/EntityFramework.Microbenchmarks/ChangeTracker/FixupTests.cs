// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Core.Models.Orders;
using EntityFramework.Microbenchmarks.Models.Orders;
using System.Linq;
using Xunit;

namespace EntityFramework.Microbenchmarks.ChangeTracker
{
    public class FixupTests : IClassFixture<FixupTests.FixupFixture>
    {
        private readonly FixupFixture _fixture;

        public FixupTests(FixupFixture fixture)
        {
            _fixture = fixture;
        }

        [Benchmark]
        public void AddChildren(IMetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                var customers = _fixture.CreateCustomers(1000, setPrimaryKeys: true);
                var orders = _fixture.CreateOrders(customers, ordersPerCustomer: 1, setPrimaryKeys: false);
                context.Customers.AttachRange(customers);

                Assert.All(orders, o => Assert.Null(o.Customer));

                using (collector.StartCollection())
                {
                    foreach (var order in orders)
                    {
                        context.Orders.Add(order);
                    }
                }

                Assert.All(orders, o => Assert.NotNull(o.Customer));
            }
        }

        [Benchmark]
        public void AddParents(IMetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                var customers = _fixture.CreateCustomers(1000, setPrimaryKeys: true);
                var orders = _fixture.CreateOrders(customers, ordersPerCustomer: 1, setPrimaryKeys: false);
                context.Orders.AddRange(orders);

                Assert.All(customers, c => Assert.Null(c.Orders));

                using (collector.StartCollection())
                {
                    foreach (var customer in customers)
                    {
                        context.Customers.Add(customer);
                    }
                }

                Assert.All(customers, c => Assert.Equal(1, c.Orders.Count));
            }
        }

        [Benchmark]
        public void AttachChildren(IMetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                var customers = _fixture.CreateCustomers(1000, setPrimaryKeys: true);
                var orders = _fixture.CreateOrders(customers, ordersPerCustomer: 1, setPrimaryKeys: true);
                context.Customers.AttachRange(customers);

                Assert.All(orders, o => Assert.Null(o.Customer));

                using (collector.StartCollection())
                {
                    foreach (var order in orders)
                    {
                        context.Orders.Attach(order);
                    }
                }

                Assert.All(orders, o => Assert.NotNull(o.Customer));
            }
        }

        [Benchmark]
        public void AttachParents(IMetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                var customers = _fixture.CreateCustomers(1000, setPrimaryKeys: true);
                var orders = _fixture.CreateOrders(customers, ordersPerCustomer: 1, setPrimaryKeys: true);
                context.Orders.AttachRange(orders);

                Assert.All(customers, c => Assert.Null(c.Orders));

                using (collector.StartCollection())
                {
                    foreach (var customer in customers)
                    {
                        context.Customers.Attach(customer);
                    }
                }

                Assert.All(customers, c => Assert.Equal(1, c.Orders.Count));
            }
        }

        [Benchmark]
        public void QueryChildren(IMetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                context.Customers.ToList();

                collector.StartCollection();
                var orders = context.Orders.ToList();
                collector.StopCollection();

                Assert.Equal(1000, context.ChangeTracker.Entries<Customer>().Count());
                Assert.Equal(1000, context.ChangeTracker.Entries<Order>().Count());
                Assert.All(orders, o => Assert.NotNull(o.Customer));
            }
        }

        [Benchmark]
        public void QueryParents(IMetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                context.Orders.ToList();

                collector.StartCollection();
                var customers = context.Customers.ToList();
                collector.StopCollection();

                Assert.Equal(1000, context.ChangeTracker.Entries<Customer>().Count());
                Assert.Equal(1000, context.ChangeTracker.Entries<Order>().Count());
                Assert.All(customers, c => Assert.Equal(1, c.Orders.Count));
            }
        }

        public class FixupFixture : OrdersFixture
        {
            public FixupFixture()
                : base("Perf_ChangeTracker_Fixup", 0, 1000, 1, 0)
            { }
        }
    }
}
