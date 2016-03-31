// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Microbenchmarks.Core;
using Microsoft.EntityFrameworkCore.Microbenchmarks.EF6.Models.Orders;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Microbenchmarks.EF6.Query
{
    internal class QueryCompilationTests : IClassFixture<QueryCompilationTests.QueryCompilationFixture>
    {
        private readonly QueryCompilationFixture _fixture;

        public QueryCompilationTests(QueryCompilationFixture fixture)
        {
            _fixture = fixture;
        }

        [Benchmark]
        [BenchmarkVariation("Default (10 queries)")]
        public void ToList(IMetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                var query = context.Products
                    .AsNoTracking()
                    .DisableQueryCache();

                using (collector.StartCollection())
                {
                    for (var i = 0; i < 10; i++)
                    {
                        query.ToList();
                    }
                }

                Assert.Equal(0, query.Count());
            }
        }

        [Benchmark]
        [BenchmarkVariation("Default (10 queries)")]
        public void FilterOrderProject(IMetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                var query = context.Products
                    .AsNoTracking()
                    .DisableQueryCache()
                    .Where(p => p.Retail < 1000)
                    .OrderBy(p => p.Name).ThenBy(p => p.Retail)
                    .Select(p => new
                    {
                        p.ProductId,
                        p.Name,
                        p.Description,
                        p.ActualStockLevel,
                        p.SKU,
                        Savings = p.Retail - p.CurrentPrice,
                        Surplus = p.ActualStockLevel - p.TargetStockLevel
                    });

                using (collector.StartCollection())
                {
                    for (var i = 0; i < 10; i++)
                    {
                        query.ToList();
                    }
                }

                Assert.Equal(0, query.Count());
            }
        }

        public class QueryCompilationFixture : OrdersFixture
        {
            public QueryCompilationFixture()
                : base("Perf_Query_Compilation_EF6", 0, 0, 0, 0)
            {
            }
        }
    }
}
