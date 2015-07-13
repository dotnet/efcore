// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Entity;
using System.Linq;
using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.EF6.Models.Orders;
using Xunit;

namespace EntityFramework.Microbenchmarks.Query
{
    public class SimpleQueryTests : IClassFixture<SimpleQueryTests.SimpleQueryFixture>
    {
        private readonly SimpleQueryFixture _fixture;

        public SimpleQueryTests(SimpleQueryFixture fixture)
        {
            _fixture = fixture;
        }

        [Benchmark(Iterations = 2, WarmupIterations = 1)]
        public void LoadAll(MetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                collector.StartCollection();
                var result = context.Products.ToList();
                collector.StopCollection();
                Assert.Equal(1000, result.Count);
            }
        }

        [Benchmark(Iterations = 100, WarmupIterations = 5)]
        public void Where(MetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                collector.StartCollection();
                var result = context.Products.Where(p => p.Retail < 15).ToList();
                collector.StopCollection();
                Assert.Equal(500, result.Count);
            }
        }

        [Benchmark(Iterations = 100, WarmupIterations = 5)]
        public void OrderBy(MetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                collector.StartCollection();
                var result = context.Products.OrderBy(p => p.Retail).ToList();
                collector.StopCollection();
                Assert.Equal(1000, result.Count);
            }
        }

        [Benchmark(Iterations = 100, WarmupIterations = 5)]
        public void Count(MetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                collector.StartCollection();
                var result = context.Products.Count();
                collector.StopCollection();
                Assert.Equal(1000, result);
            }
        }

        [Benchmark(Iterations = 100, WarmupIterations = 5)]
        public void SkipTake(MetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                collector.StartCollection();
                var result = context.Products
                    .OrderBy(p => p.ProductId)
                    .Skip(500).Take(500)
                    .ToList();

                collector.StopCollection();
                Assert.Equal(500, result.Count);
            }
        }

        [Benchmark(Iterations = 100, WarmupIterations = 5)]
        public void GroupBy(MetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                collector.StartCollection();
                var result = context.Products
                    .GroupBy(p => p.Retail)
                    .Select(g => new
                    {
                        Retail = g.Key,
                        Products = g
                    })
                    .ToList();

                collector.StopCollection();
                Assert.Equal(10, result.Count);
                Assert.All(result, g => Assert.Equal(100, g.Products.Count()));
            }
        }

        [Benchmark(Iterations = 100, WarmupIterations = 5)]
        public void Include(MetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                collector.StartCollection();
                var result = context.Customers.Include(c => c.Orders).ToList();
                collector.StopCollection();
                Assert.Equal(1000, result.Count);
                Assert.Equal(2000, result.SelectMany(c => c.Orders).Count());
            }
        }

        [Benchmark(Iterations = 100, WarmupIterations = 5)]
        public void Projection(MetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                collector.StartCollection();
                var result = context.Products.Select(p => new { p.Name, p.Retail }).ToList();
                collector.StopCollection();
                Assert.Equal(1000, result.Count);
            }
        }

        [Benchmark(Iterations = 100, WarmupIterations = 5)]
        public void ProjectionAcrossNavigation(MetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                collector.StartCollection();
                var result = context.Orders.Select(o => new
                {
                    CustomerName = o.Customer.Name,
                    OrderDate = o.Date
                })
                    .ToList();

                collector.StopCollection();
                Assert.Equal(2000, result.Count);
            }
        }

        [Benchmark(Iterations = 100, WarmupIterations = 5)]
        public void NoTracking(MetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                collector.StartCollection();
                var result = context.Products.AsNoTracking().ToList();
                collector.StopCollection();
                Assert.Equal(1000, result.Count);
            }
        }

        public class SimpleQueryFixture : OrdersFixture
        {
            public SimpleQueryFixture()
                : base("Perf_Query_Simple_EF6", 1000, 1000, 2, 2)
            { }
        }
    }
}
