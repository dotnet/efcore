// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Models.Orders;
using Xunit;

namespace EntityFramework.Microbenchmarks.UpdatePipeline
{
    public class SimpleUpdatePipelineTests : IClassFixture<SimpleUpdatePipelineTests.SimpleUpdatePipelineFixture>
    {
        private readonly SimpleUpdatePipelineFixture _fixture;

        public SimpleUpdatePipelineTests(SimpleUpdatePipelineFixture fixture)
        {
            _fixture = fixture;
        }

        [Benchmark]
        [BenchmarkVariation("Batching Off", true)]
        [BenchmarkVariation("Default", false)]
        public void Insert(IMetricCollector collector, bool disableBatching)
        {
            using (var context = _fixture.CreateContext(disableBatching))
            {
                using (context.Database.BeginTransaction())
                {
                    var customers = _fixture.CreateCustomers(1000, setPrimaryKeys: false);
                    context.Customers.AddRange(customers);

                    collector.StartCollection();
                    var records = context.SaveChanges();
                    collector.StopCollection();

                    Assert.Equal(1000, records);
                }
            }
        }

        [Benchmark]
        [BenchmarkVariation("Batching Off", true)]
        [BenchmarkVariation("Default", false)]
        public void Update(IMetricCollector collector, bool disableBatching)
        {
            using (var context = _fixture.CreateContext(disableBatching))
            {
                using (context.Database.BeginTransaction())
                {
                    foreach (var customer in context.Customers)
                    {
                        customer.FirstName += " Modified";
                    }

                    collector.StartCollection();
                    var records = context.SaveChanges();
                    collector.StopCollection();

                    Assert.Equal(1000, records);
                }
            }
        }

        [Benchmark]
        [BenchmarkVariation("Batching Off", true)]
        [BenchmarkVariation("Default", false)]
        public void Delete(IMetricCollector collector, bool disableBatching)
        {
            using (var context = _fixture.CreateContext(disableBatching))
            {
                using (context.Database.BeginTransaction())
                {
                    context.Customers.RemoveRange(context.Customers.ToList());

                    collector.StartCollection();
                    var records = context.SaveChanges();
                    collector.StopCollection();

                    Assert.Equal(1000, records);
                }
            }
        }

        [Benchmark]
        [BenchmarkVariation("Batching Off", true)]
        [BenchmarkVariation("Default", false)]
        public void Mixed(IMetricCollector collector, bool disableBatching)
        {
            using (var context = _fixture.CreateContext(disableBatching))
            {
                using (context.Database.BeginTransaction())
                {
                    var existingCustomers = context.Customers.ToArray();

                    var newCustomers = _fixture.CreateCustomers(333, setPrimaryKeys: false);
                    context.Customers.AddRange(newCustomers);

                    for (var i = 0; i < 1000; i += 3)
                    {
                        context.Customers.Remove(existingCustomers[i]);
                    }

                    for (var i = 1; i < 1000; i += 3)
                    {
                        existingCustomers[i].FirstName += " Modified";
                    }

                    collector.StartCollection();
                    var records = context.SaveChanges();
                    collector.StopCollection();

                    Assert.Equal(1000, records);
                }
            }
        }

        public class SimpleUpdatePipelineFixture : OrdersFixture
        {
            public SimpleUpdatePipelineFixture()
                : base("Perf_UpdatePipeline_Simple", 0, 1000, 0, 0)
            {
            }
        }
    }
}
