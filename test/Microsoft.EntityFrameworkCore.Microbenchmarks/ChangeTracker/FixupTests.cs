// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Microbenchmarks.Core;
using Microsoft.EntityFrameworkCore.Microbenchmarks.Core.Models.Orders;
using Microsoft.EntityFrameworkCore.Microbenchmarks.Models.Orders;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Microbenchmarks.ChangeTracker
{
    [SqlServerRequired]
    public class FixupTests : IClassFixture<FixupTests.FixupFixture>
    {
        private readonly FixupFixture _fixture;

        public FixupTests(FixupFixture fixture)
        {
            _fixture = fixture;
        }

        [Benchmark]
        [BenchmarkVariation("AutoDetectChanges On", true)]
        [BenchmarkVariation("AutoDetectChanges Off", false)]
        public void AddChildren(IMetricCollector collector, bool autoDetectChanges)
        {
            using (var context = _fixture.CreateContext())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = autoDetectChanges;

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
        [BenchmarkVariation("AutoDetectChanges On", true)]
        [BenchmarkVariation("AutoDetectChanges Off", false)]
        public void AddParents(IMetricCollector collector, bool autoDetectChanges)
        {
            using (var context = _fixture.CreateContext())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = autoDetectChanges;

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
        [BenchmarkVariation("AutoDetectChanges On", true)]
        [BenchmarkVariation("AutoDetectChanges Off", false)]
        public void AttachChildren(IMetricCollector collector, bool autoDetectChanges)
        {
            using (var context = _fixture.CreateContext())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = autoDetectChanges;

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
        [BenchmarkVariation("AutoDetectChanges On", true)]
        [BenchmarkVariation("AutoDetectChanges Off", false)]
        public void AttachParents(IMetricCollector collector, bool autoDetectChanges)
        {
            using (var context = _fixture.CreateContext())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = autoDetectChanges;

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
            {
            }
        }
    }
}
