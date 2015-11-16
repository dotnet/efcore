// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Models.Orders;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EntityFramework.Microbenchmarks.Query
{
    [SqlServerRequired]
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
            using (var context = _fixture.CreateContext(queryCachingEnabled: caching))
            {
                var query = context.Products.ApplyTracking(tracking);

                using (collector.StartCollection())
                {
                    for (var i = 0; i < queriesPerIteration; i++)
                    {
                        query.ToList();
                    }
                }

                Assert.Equal(1000, query.Count());
                Assert.False(tracking && (queriesPerIteration != 1), "Multiple queries per iteration not valid for tracking queries");
            }
        }

        [Benchmark]
        [BenchmarkVariation("Tracking On (1 query)", true, true, 1)]
        [BenchmarkVariation("Tracking Off (10 queries)", false, true, 10)]
        [BenchmarkVariation("Tracking On, Query Cache Off (1 query)", true, false, 1)]
        [BenchmarkVariation("Tracking Off, Query Cache Off (10 queries)", false, false, 10)]
        public void Where(IMetricCollector collector, bool tracking, bool caching, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext(queryCachingEnabled: caching))
            {
                var query = context.Products
                    .ApplyTracking(tracking)
                    .Where(p => p.Retail < 15);

                using (collector.StartCollection())
                {
                    for (var i = 0; i < queriesPerIteration; i++)
                    {
                        query.ToList();
                    }
                }

                Assert.Equal(500, query.Count());
                Assert.False(tracking && (queriesPerIteration != 1), "Multiple queries per iteration not valid for tracking queries");
            }
        }

        [Benchmark]
        [BenchmarkVariation("Tracking On (1 query)", true, true, 1)]
        [BenchmarkVariation("Tracking Off (10 queries)", false, true, 10)]
        [BenchmarkVariation("Tracking On, Query Cache Off  (1 query)", true, false, 1)]
        [BenchmarkVariation("Tracking Off, Query Cache Off (10 queries)", false, false, 10)]
        public void OrderBy(IMetricCollector collector, bool tracking, bool caching, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext(queryCachingEnabled: caching))
            {
                var query = context.Products
                    .ApplyTracking(tracking)
                    .OrderBy(p => p.Retail);

                using (collector.StartCollection())
                {
                    for (var i = 0; i < queriesPerIteration; i++)
                    {
                        query.ToList();
                    }
                }

                Assert.Equal(1000, query.Count());
                Assert.False(tracking && (queriesPerIteration != 1), "Multiple queries per iteration not valid for tracking queries");
            }
        }

        [Benchmark]
        [BenchmarkVariation("Default (100 queries)", true, 100)]
        [BenchmarkVariation("Query Cache Off (100 queries)", false, 100)]
        public void Count(IMetricCollector collector, bool caching, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext(queryCachingEnabled: caching))
            {
                var query = context.Products;

                using (collector.StartCollection())
                {
                    for (var i = 0; i < queriesPerIteration; i++)
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
            using (var context = _fixture.CreateContext(queryCachingEnabled: caching))
            {
                var query = context.Products
                    .ApplyTracking(tracking)
                    .OrderBy(p => p.ProductId)
                    .Skip(500).Take(500);

                using (collector.StartCollection())
                {
                    for (var i = 0; i < queriesPerIteration; i++)
                    {
                        query.ToList();
                    }
                }

                Assert.Equal(500, query.Count());
                Assert.False(tracking && (queriesPerIteration != 1), "Multiple queries per iteration not valid for tracking queries");
            }
        }

        [Benchmark]
        [BenchmarkVariation("Default (10 queries)", true, 10)]
        [BenchmarkVariation("Query Cache Off (10 queries)", false, 10)]
        public void GroupBy(IMetricCollector collector, bool caching, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext(queryCachingEnabled: caching))
            {
                var query = context.Products
                    .GroupBy(p => p.Retail)
                    .Select(g => new
                    {
                        Retail = g.Key,
                        Products = g
                    });

                using (collector.StartCollection())
                {
                    for (var i = 0; i < queriesPerIteration; i++)
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
            using (var context = _fixture.CreateContext(queryCachingEnabled: caching))
            {
                var query = context.Customers
                    .ApplyTracking(tracking)
                    .Include(c => c.Orders);

                using (collector.StartCollection())
                {
                    for (var i = 0; i < queriesPerIteration; i++)
                    {
                        query.ToList();
                    }
                }

                var result = query.ToList();
                Assert.Equal(1000, result.Count);
                Assert.Equal(2000, result.SelectMany(c => c.Orders).Count());
                Assert.False(tracking && (queriesPerIteration != 1), "Multiple queries per iteration not valid for tracking queries");
            }
        }

        [Benchmark]
        [BenchmarkVariation("Default (10 queries)", true, 10)]
        [BenchmarkVariation("Query Cache Off (10 queries)", false, 10)]
        public void Projection(IMetricCollector collector, bool caching, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext(queryCachingEnabled: caching))
            {
                var query = context.Products
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
                    for (var i = 0; i < queriesPerIteration; i++)
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
            using (var context = _fixture.CreateContext(queryCachingEnabled: caching))
            {
                var query = context.Orders
                    .Select(o => new
                    {
                        CustomerTitle = o.Customer.Title,
                        CustomerFirstName = o.Customer.FirstName,
                        CustomerLastName = o.Customer.LastName,
                        OrderDate = o.Date, o.OrderDiscount,
                        OrderDiscountReason = o.DiscountReason,
                        OrderTax = o.Tax,
                        OrderSpecialRequests = o.SpecialRequests
                    });

                using (collector.StartCollection())
                {
                    for (var i = 0; i < queriesPerIteration; i++)
                    {
                        query.ToList();
                    }
                }

                Assert.Equal(2000, query.Count());
            }
        }

        public class SimpleQueryFixture : OrdersFixture
        {
            private readonly IServiceProvider _noQueryCacheServiceProvider;

            public SimpleQueryFixture()
                : base("Perf_Query_Simple", 1000, 1000, 2, 2)
            {
                var collection = new ServiceCollection();
                collection.AddEntityFramework().AddSqlServer();
                collection.AddSingleton<IMemoryCache, NonCachingMemoryCache>();
                _noQueryCacheServiceProvider = collection.BuildServiceProvider();
            }

            public OrdersContext CreateContext(bool disableBatching = false, bool queryCachingEnabled = true)
            {
                if (!queryCachingEnabled)
                {
                    return new OrdersContext(_noQueryCacheServiceProvider, ConnectionString, disableBatching);
                }

                return base.CreateContext(disableBatching);
            }

            private class NonCachingMemoryCache : IMemoryCache
            {
                public bool TryGetValue(object key, out object value)
                {
                    value = null;
                    return false;
                }

                public object Set(object key, object value, MemoryCacheEntryOptions options)
                {
                    return value;
                }

                public void Remove(object key)
                {
                }

                public IEntryLink CreateLinkingScope()
                {
                    throw new NotImplementedException();
                }

                public void Dispose()
                {
                }
            }
        }
    }
}
