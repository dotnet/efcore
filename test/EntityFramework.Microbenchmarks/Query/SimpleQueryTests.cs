// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Models.Orders;
using Microsoft.Data.Entity;
using Xunit;

namespace EntityFramework.Microbenchmarks.Query
{
    public class SimpleQueryTests
    {
        private static readonly string _connectionString
            = $@"Server={BenchmarkConfig.Instance.BenchmarkDatabaseInstance};Database=Perf_Query_Simple;Integrated Security=True;MultipleActiveResultSets=true;";

        public SimpleQueryTests()
        {
            new OrdersSeedData().EnsureCreated(
                _connectionString,
                productCount: 1000,
                customerCount: 1000,
                ordersPerCustomer: 2,
                linesPerOrder: 2);
        }

        [Benchmark(Iterations = 100, WarmupIterations = 5)]
        public void LoadAll(MetricCollector collector)
        {
            using (var context = new OrdersContext(_connectionString))
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
            using (var context = new OrdersContext(_connectionString))
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
            using (var context = new OrdersContext(_connectionString))
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
            using (var context = new OrdersContext(_connectionString))
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
            using (var context = new OrdersContext(_connectionString))
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
            using (var context = new OrdersContext(_connectionString))
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

        [Benchmark(Iterations = 2, WarmupIterations = 1)]
        public void Include(MetricCollector collector)
        {
            using (var context = new OrdersContext(_connectionString))
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
            using (var context = new OrdersContext(_connectionString))
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
            using (var context = new OrdersContext(_connectionString))
            {
                collector.StartCollection();
                // TODO Use navigation for projection when supported (#325)
                var result = context.Orders.Join(
                context.Customers,
                o => o.CustomerId,
                c => c.CustomerId,
                (o, c) => new { CustomerName = c.Name, OrderDate = o.Date })
                .ToList();

                collector.StopCollection();
                Assert.Equal(2000, result.Count);
            }
        }

        [Benchmark(Iterations = 100, WarmupIterations = 5)]
        public void NoTracking(MetricCollector collector)
        {
            using (var context = new OrdersContext(_connectionString))
            {
                collector.StartCollection();
                var result = context.Products.AsNoTracking().ToList();
                collector.StopCollection();
                Assert.Equal(1000, result.Count);
            }
        }
    }
}
