// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.EF6.Models.Orders;
using System.Data.Entity;
using System.Linq;
using Xunit;

namespace EntityFramework.Microbenchmarks.EF6.Query
{
    public class SimpleQueryTests : IClassFixture<SimpleQueryTests.SimpleQueryFixture>
    {
        private readonly SimpleQueryFixture _fixture;

        public SimpleQueryTests(SimpleQueryFixture fixture)
        {
            _fixture = fixture;
        }

        [Benchmark]
        [BenchmarkVariation("Tracking On (1 query)", true, true, 1)]
        [BenchmarkVariation("Tracking Off (10 queries)", false, true, 10)]
        [BenchmarkVariation("Tracking On, Query Cache Off (1 query)", true, false, 1)]
        [BenchmarkVariation("Tracking Off, Query Cache Off (10 queries)", false, false, 10)]
        public void LoadAll(IMetricCollector collector, bool tracking, bool caching, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
            {
                var query = context.Products
                    .ApplyCaching(caching)
                    .ApplyTracking(tracking);

                using (collector.StartCollection())
                {
                    for (int i = 0; i < queriesPerIteration; i++)
                    {
                        query.ToList();
                    }
                }

                Assert.Equal(1000, query.Count());
                Assert.False(tracking && queriesPerIteration != 1, "Multiple queries per iteration not valid for tracking queries");
            }
        }

        [Benchmark]
        [BenchmarkVariation("Tracking On (1 query)", true, true, 1)]
        [BenchmarkVariation("Tracking Off (10 queries)", false, true, 10)]
        [BenchmarkVariation("Tracking On, Query Cache Off (1 query)", true, false, 1)]
        [BenchmarkVariation("Tracking Off, Query Cache Off (10 queries)", false, false, 10)]
        public void Where(IMetricCollector collector, bool tracking, bool caching, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
            {
                var query = context.Products
                    .ApplyCaching(caching)
                    .ApplyTracking(tracking)
                    .Where(p => p.Retail < 15);

                using (collector.StartCollection())
                {
                    for (int i = 0; i < queriesPerIteration; i++)
                    {
                        query.ToList();
                    }
                }

                Assert.Equal(500, query.Count());
                Assert.False(tracking && queriesPerIteration != 1, "Multiple queries per iteration not valid for tracking queries");
            }
        }

        [Benchmark]
        [BenchmarkVariation("Tracking On (1 query)", true, true, 1)]
        [BenchmarkVariation("Tracking Off (10 queries)", false, true, 10)]
        [BenchmarkVariation("Tracking On, Query Cache Off  (1 query)", true, false, 1)]
        [BenchmarkVariation("Tracking Off, Query Cache Off (10 queries)", false, false, 10)]
        public void OrderBy(IMetricCollector collector, bool tracking, bool caching, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
            {
                var query = context.Products
                    .ApplyCaching(caching)
                    .ApplyTracking(tracking)
                    .OrderBy(p => p.Retail);

                using (collector.StartCollection())
                {
                    for (int i = 0; i < queriesPerIteration; i++)
                    {
                        query.ToList();
                    }
                }

                Assert.Equal(1000, query.Count());
                Assert.False(tracking && queriesPerIteration != 1, "Multiple queries per iteration not valid for tracking queries");
            }
        }

        [Benchmark]
        [BenchmarkVariation("Default (100 queries)", true, 100)]
        [BenchmarkVariation("Query Cache Off (100 queries)", false, 100)]
        public void Count(IMetricCollector collector, bool caching, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
            {
                var query = context.Products
                    .ApplyCaching(caching);

                using (collector.StartCollection())
                {
                    for (int i = 0; i < queriesPerIteration; i++)
                    {
                        query.Count();
                    }
                }

                Assert.Equal(1000, query.Count());
            }
        }

        [Benchmark]
        [BenchmarkVariation("Tracking On (1 query)", true, true, 1)]
        [BenchmarkVariation("Tracking Off (10 queries)", false, true, 10)]
        [BenchmarkVariation("Tracking On, Query Cache Off  (1 query)", true, false, 1)]
        [BenchmarkVariation("Tracking Off, Query Cache Off (10 queries)", false, false, 10)]
        public void SkipTake(IMetricCollector collector, bool tracking, bool caching, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
            {
                var query = context.Products
                    .ApplyCaching(caching)
                    .ApplyTracking(tracking)
                    .OrderBy(p => p.ProductId)
                    .Skip(500).Take(500);

                using (collector.StartCollection())
                {
                    for (int i = 0; i < queriesPerIteration; i++)
                    {
                        query.ToList();
                    }
                }

                Assert.Equal(500, query.Count());
                Assert.False(tracking && queriesPerIteration != 1, "Multiple queries per iteration not valid for tracking queries");
            }
        }

        [Benchmark]
        [BenchmarkVariation("Default (10 queries)", true, 10)]
        [BenchmarkVariation("Query Cache Off (10 queries)", false, 10)]
        public void GroupBy(IMetricCollector collector, bool caching, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
            {
                var query = context.Products
                    .ApplyCaching(caching)
                    .GroupBy(p => p.Retail)
                    .Select(g => new
                    {
                        Retail = g.Key,
                        Products = g
                    });

                using (collector.StartCollection())
                {
                    for (int i = 0; i < queriesPerIteration; i++)
                    {
                        query.ToList();
                    }
                }

                var result = query.ToList();
                Assert.Equal(10, result.Count);
                Assert.All(result, g => Assert.Equal(100, g.Products.Count()));
            }
        }

        [Benchmark]
        [BenchmarkVariation("Tracking On (1 query)", true, true, 1)]
        [BenchmarkVariation("Tracking Off (1 query)", false, true, 1)]
        [BenchmarkVariation("Tracking On, Query Cache Off  (1 query)", true, false, 1)]
        [BenchmarkVariation("Tracking Off, Query Cache Off  (1 query)", false, false, 1)]
        public void Include(IMetricCollector collector, bool tracking, bool caching, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
            {
                var query = context.Customers
                    .ApplyCaching(caching)
                    .ApplyTracking(tracking)
                    .Include(c => c.Orders);

                using (collector.StartCollection())
                {
                    for (int i = 0; i < queriesPerIteration; i++)
                    {
                        query.ToList();
                    }
                }

                var result = query.ToList();
                Assert.Equal(1000, result.Count);
                Assert.Equal(2000, result.SelectMany(c => c.Orders).Count());
                Assert.False(tracking && queriesPerIteration != 1, "Multiple queries per iteration not valid for tracking queries");
            }
        }

        [Benchmark]
        [BenchmarkVariation("Default (10 queries)", true, 10)]
        [BenchmarkVariation("Query Cache Off (10 queries)", false, 10)]
        public void Projection(IMetricCollector collector, bool caching, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
            {
                var query = context.Products
                    .ApplyCaching(caching)
                    .Select(p => new
                    {
                        p.ProductId,
                        p.Name,
                        p.Description,
                        p.SKU,
                        p.Retail,
                        p.CurrentPrice,
                        p.ActualStockLevel
                    });

                using (collector.StartCollection())
                {
                    for (int i = 0; i < queriesPerIteration; i++)
                    {
                        query.ToList();
                    }
                }

                Assert.Equal(1000, query.Count());
            }
        }

        [Benchmark]
        [BenchmarkVariation("Default (10 queries)", true, 10)]
        [BenchmarkVariation("Query Cache Off (10 queries)", false, 10)]
        public void ProjectionAcrossNavigation(IMetricCollector collector, bool caching, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
            {
                var query = context.Orders
                    .ApplyCaching(caching)
                    .Select(o => new
                    {
                        CustomerTitle = o.Customer.Title,
                        CustomerFirstName = o.Customer.FirstName,
                        CustomerLastName = o.Customer.LastName,
                        OrderDate = o.Date,
                        OrderDiscount = o.OrderDiscount,
                        OrderDiscountReason = o.DiscountReason,
                        OrderTax = o.Tax,
                        OrderSpecialRequests = o.SpecialRequests,
                    });

                using (collector.StartCollection())
                {
                    for (int i = 0; i < queriesPerIteration; i++)
                    {
                        query.ToList();
                    }
                }

                Assert.Equal(2000, query.Count());
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
