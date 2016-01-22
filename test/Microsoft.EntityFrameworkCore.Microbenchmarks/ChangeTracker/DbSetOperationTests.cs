// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Microbenchmarks.Core;
using Microsoft.EntityFrameworkCore.Microbenchmarks.Models.Orders;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Microbenchmarks.ChangeTracker
{
    [SqlServerRequired]
    public class DbSetOperationTests : IClassFixture<DbSetOperationTests.DbSetOperationFixture>
    {
        private readonly DbSetOperationFixture _fixture;

        public DbSetOperationTests(DbSetOperationFixture fixture)
        {
            _fixture = fixture;
        }

        [Benchmark]
        [BenchmarkVariation("AutoDetectChanges On", true)]
        [BenchmarkVariation("AutoDetectChanges Off", false)]
        public void Add(IMetricCollector collector, bool autoDetectChanges)
        {
            using (var context = _fixture.CreateContext())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = autoDetectChanges;

                var customers = _fixture.CreateCustomers(1000, setPrimaryKeys: false);

                using (collector.StartCollection())
                {
                    foreach (var customer in customers)
                    {
                        context.Customers.Add(customer);
                    }
                }
            }
        }

        [Benchmark]
        [BenchmarkVariation("AutoDetectChanges On", true)]
        [BenchmarkVariation("AutoDetectChanges Off", false)]
        public void AddRange(IMetricCollector collector, bool autoDetectChanges)
        {
            using (var context = _fixture.CreateContext())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = autoDetectChanges;

                var customers = _fixture.CreateCustomers(1000, setPrimaryKeys: false);

                using (collector.StartCollection())
                {
                    context.Customers.AddRange(customers);
                }
            }
        }

        [Benchmark]
        [BenchmarkVariation("AutoDetectChanges On", true)]
        [BenchmarkVariation("AutoDetectChanges Off", false)]
        public void Attach(IMetricCollector collector, bool autoDetectChanges)
        {
            using (var context = _fixture.CreateContext())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = autoDetectChanges;

                var customers = _fixture.CreateCustomers(1000, setPrimaryKeys: true);

                using (collector.StartCollection())
                {
                    foreach (var customer in customers)
                    {
                        context.Customers.Attach(customer);
                    }
                }
            }
        }

        [Benchmark]
        [BenchmarkVariation("AutoDetectChanges On", true)]
        [BenchmarkVariation("AutoDetectChanges Off", false)]
        public void AttachRange(IMetricCollector collector, bool autoDetectChanges)
        {
            using (var context = _fixture.CreateContext())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = autoDetectChanges;

                var customers = _fixture.CreateCustomers(1000, setPrimaryKeys: true);

                using (collector.StartCollection())
                {
                    context.Customers.AttachRange(customers);
                }
            }
        }

        [Benchmark]
        [BenchmarkVariation("AutoDetectChanges On", true)]
        [BenchmarkVariation("AutoDetectChanges Off", false)]
        public void Remove(IMetricCollector collector, bool autoDetectChanges)
        {
            using (var context = _fixture.CreateContext())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = autoDetectChanges;

                var customers = _fixture.CreateCustomers(1000, setPrimaryKeys: true);
                context.Customers.AttachRange(customers);

                using (collector.StartCollection())
                {
                    foreach (var customer in customers)
                    {
                        context.Customers.Remove(customer);
                    }
                }
            }
        }

        [Benchmark]
        [BenchmarkVariation("AutoDetectChanges On", true)]
        [BenchmarkVariation("AutoDetectChanges Off", false)]
        public void RemoveRange(IMetricCollector collector, bool autoDetectChanges)
        {
            using (var context = _fixture.CreateContext())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = autoDetectChanges;

                var customers = _fixture.CreateCustomers(1000, setPrimaryKeys: true);
                context.Customers.AttachRange(customers);

                using (collector.StartCollection())
                {
                    context.Customers.RemoveRange(customers);
                }
            }
        }

        [Benchmark]
        [BenchmarkVariation("AutoDetectChanges On", true)]
        [BenchmarkVariation("AutoDetectChanges Off", false)]
        public void Update(IMetricCollector collector, bool autoDetectChanges)
        {
            using (var context = _fixture.CreateContext())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = autoDetectChanges;

                var customers = _fixture.CreateCustomers(1000, setPrimaryKeys: true);
                context.Customers.AttachRange(customers);

                using (collector.StartCollection())
                {
                    foreach (var customer in customers)
                    {
                        context.Customers.Update(customer);
                    }
                }
            }
        }

        [Benchmark]
        [BenchmarkVariation("AutoDetectChanges On", true)]
        [BenchmarkVariation("AutoDetectChanges Off", false)]
        public void UpdateRange(IMetricCollector collector, bool autoDetectChanges)
        {
            using (var context = _fixture.CreateContext())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = autoDetectChanges;

                var customers = _fixture.CreateCustomers(1000, setPrimaryKeys: true);
                context.Customers.AttachRange(customers);

                using (collector.StartCollection())
                {
                    context.Customers.UpdateRange(customers);
                }
            }
        }

        public class DbSetOperationFixture : OrdersFixture
        {
            public DbSetOperationFixture()
                : base("Perf_ChangeTracker_DbSetOperation", 0, 0, 0, 0)
            {
            }
        }
    }
}
