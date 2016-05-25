// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Microbenchmarks.Core;
using Microsoft.EntityFrameworkCore.Microbenchmarks.Core.Models.Orders;
using Microsoft.EntityFrameworkCore.Microbenchmarks.EF6.Models.Orders;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Microbenchmarks.EF6.ChangeTracker
{
    [SqlServerRequired]
    [SkipForNonBenchmarkTestRuns("Test takes a long time to execute, only run during benchmark collection runs.")]
    public class FixupTests : IClassFixture<FixupTests.FixupFixture>
    {
        private readonly FixupFixture _fixture;

        public FixupTests(FixupFixture fixture)
        {
            _fixture = fixture;
        }

        [Benchmark]
        [BenchmarkVariation("AutoDetectChanges On", iterations: 1, data: new object[] { true })]
        [BenchmarkVariation("AutoDetectChanges Off", false)]
        public void AddChildren(IMetricCollector collector, bool autoDetectChanges)
        {
            using (var context = _fixture.CreateContext())
            {
                context.Configuration.AutoDetectChangesEnabled = autoDetectChanges;

                var customers = _fixture.CreateCustomers(5000, setPrimaryKeys: true);
                var orders = _fixture.CreateOrders(customers, ordersPerCustomer: 2, setPrimaryKeys: false);
                customers.ForEach(c => context.Customers.Attach(c));

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

        // Note: AddParents() not implemented because fixup to added parents 
        //       only happens during SaveChanges for EF6.x (not during Add)

        [Benchmark]
        [BenchmarkVariation("AutoDetectChanges On", iterations: 1, data: new object[] { true })]
        [BenchmarkVariation("AutoDetectChanges Off", false)]
        public void AttachChildren(IMetricCollector collector, bool autoDetectChanges)
        {
            using (var context = _fixture.CreateContext())
            {
                context.Configuration.AutoDetectChangesEnabled = autoDetectChanges;

                var customers = _fixture.CreateCustomers(5000, setPrimaryKeys: true);
                var orders = _fixture.CreateOrders(customers, ordersPerCustomer: 2, setPrimaryKeys: true);
                customers.ForEach(c => context.Customers.Attach(c));

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
        [BenchmarkVariation("AutoDetectChanges On", iterations: 1, data: new object[] { true })]
        [BenchmarkVariation("AutoDetectChanges Off", false)]
        public void AttachParents(IMetricCollector collector, bool autoDetectChanges)
        {
            using (var context = _fixture.CreateContext())
            {
                context.Configuration.AutoDetectChangesEnabled = autoDetectChanges;

                var customers = _fixture.CreateCustomers(5000, setPrimaryKeys: true);
                var orders = _fixture.CreateOrders(customers, ordersPerCustomer: 2, setPrimaryKeys: true);
                orders.ForEach(o => context.Orders.Attach(o));

                Assert.All(customers, c => Assert.Null(c.Orders));

                using (collector.StartCollection())
                {
                    foreach (var customer in customers)
                    {
                        context.Customers.Attach(customer);
                    }
                }

                Assert.All(customers, c => Assert.Equal(2, c.Orders.Count));
            }
        }

        [Benchmark(Iterations = 1)]
        public void QueryChildren(IMetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                context.Customers.ToList();

                collector.StartCollection();
                var orders = context.Orders.ToList();
                collector.StopCollection();

                Assert.Equal(5000, context.ChangeTracker.Entries<Customer>().Count());
                Assert.Equal(10000, context.ChangeTracker.Entries<Order>().Count());
                Assert.All(orders, o => Assert.NotNull(o.Customer));
            }
        }

        [Benchmark(Iterations = 1)]
        public void QueryParents(IMetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                context.Orders.ToList();

                collector.StartCollection();
                var customers = context.Customers.ToList();
                collector.StopCollection();

                Assert.Equal(5000, context.ChangeTracker.Entries<Customer>().Count());
                Assert.Equal(10000, context.ChangeTracker.Entries<Order>().Count());
                Assert.All(customers, c => Assert.Equal(2, c.Orders.Count));
            }
        }

        public class FixupFixture : OrdersFixture
        {
            public FixupFixture()
                : base("Perf_ChangeTracker_Fixup_EF6", 0, 5000, 2, 0)
            {
            }
        }
    }
}
