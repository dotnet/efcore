// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Models.Orders;
using Xunit;

namespace EntityFramework.Microbenchmarks.ChangeTracker
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
                        context.Customers.AddWithChildren(customer);
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
                    context.Customers.AddRangeWithChildren(customers);
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
                        context.Customers.AttachWithChildren(customer);
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
                    context.Customers.AttachRangeWithChildren(customers);
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
                context.Customers.AttachRangeWithChildren(customers);

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
                context.Customers.AttachRangeWithChildren(customers);

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
                context.Customers.AttachRangeWithChildren(customers);

                using (collector.StartCollection())
                {
                    foreach (var customer in customers)
                    {
                        context.Customers.UpdateWithChildren(customer);
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
                context.Customers.AttachRangeWithChildren(customers);

                using (collector.StartCollection())
                {
                    context.Customers.UpdateRangeWithChildren(customers);
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
