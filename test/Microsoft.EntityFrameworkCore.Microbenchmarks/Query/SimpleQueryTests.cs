// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Microbenchmarks.Core;
using Microsoft.EntityFrameworkCore.Microbenchmarks.Models.Orders;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Microbenchmarks.Query
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
        [BenchmarkVariation("Tracking On - Sync (1 query)", true, false, 1)]
        [BenchmarkVariation("Tracking Off - Sync (10 queries)", false, false, 10)]
        [BenchmarkVariation("Tracking On - Async (1 query)", true, true, 1)]
        [BenchmarkVariation("Tracking Off - Async (10 queries)", false, true, 10)]
        public async Task LoadAll(IMetricCollector collector, bool tracking, bool async, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
            {
                var query = context.Products.ApplyTracking(tracking);

                using (collector.StartCollection())
                {
                    for (var i = 0; i < queriesPerIteration; i++)
                    {
                        if(async)
                        {
                            await query.ToListAsync();
                        }
                        else
                        {
                            query.ToList();
                        }
                    }
                }

                Assert.Equal(1000, query.Count());
                Assert.False(tracking && (queriesPerIteration != 1), "Multiple queries per iteration not valid for tracking queries");
            }
        }

        [Benchmark]
        [BenchmarkVariation("Tracking On - Sync (1 query)", true, false, 1)]
        [BenchmarkVariation("Tracking Off - Sync (10 queries)", false, false, 10)]
        [BenchmarkVariation("Tracking On - Async (1 query)", true, true, 1)]
        [BenchmarkVariation("Tracking Off - Async (10 queries)", false, true, 10)]
        public async Task Where(IMetricCollector collector, bool tracking, bool async, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
            {
                var query = context.Products
                    .ApplyTracking(tracking)
                    .Where(p => p.Retail < 15);

                using (collector.StartCollection())
                {
                    for (var i = 0; i < queriesPerIteration; i++)
                    {
                        if (async)
                        {
                            await query.ToListAsync();
                        }
                        else
                        {
                            query.ToList();
                        }
                    }
                }

                Assert.Equal(500, query.Count());
                Assert.False(tracking && (queriesPerIteration != 1), "Multiple queries per iteration not valid for tracking queries");
            }
        }

        [Benchmark]
        [BenchmarkVariation("Tracking On - Sync (1 query)", true, false, 1)]
        [BenchmarkVariation("Tracking Off - Sync (10 queries)", false, false, 10)]
        [BenchmarkVariation("Tracking On - Async (1 query)", true, true, 1)]
        [BenchmarkVariation("Tracking Off - Async (10 queries)", false, true, 10)]
        public async Task OrderBy(IMetricCollector collector, bool tracking, bool async, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
            {
                var query = context.Products
                    .ApplyTracking(tracking)
                    .OrderBy(p => p.Retail);

                using (collector.StartCollection())
                {
                    for (var i = 0; i < queriesPerIteration; i++)
                    {
                        if (async)
                        {
                            await query.ToListAsync();
                        }
                        else
                        {
                            query.ToList();
                        }
                    }
                }

                Assert.Equal(1000, query.Count());
                Assert.False(tracking && (queriesPerIteration != 1), "Multiple queries per iteration not valid for tracking queries");
            }
        }

        [Benchmark]
        [BenchmarkVariation("Sync (100 queries)", false, 100)]
        [BenchmarkVariation("Async (100 queries)", true, 100)]
        public async Task Count(IMetricCollector collector, bool async, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
            {
                var query = context.Products;

                using (collector.StartCollection())
                {
                    for (var i = 0; i < queriesPerIteration; i++)
                    {
                        if (async)
                        {
                            await query.CountAsync();
                        }
                        else
                        {
                            query.Count();
                        }
                    }
                }

                Assert.Equal(1000, query.Count());
            }
        }

        [Benchmark]
        [BenchmarkVariation("Tracking On - Sync (1 query)", true, false, 1)]
        [BenchmarkVariation("Tracking Off - Sync (10 queries)", false, false, 10)]
        [BenchmarkVariation("Tracking On - Async (1 query)", true, true, 1)]
        [BenchmarkVariation("Tracking Off - Async (10 queries)", false, true, 10)]
        public async Task SkipTake(IMetricCollector collector, bool tracking, bool async, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
            {
                var query = context.Products
                    .ApplyTracking(tracking)
                    .OrderBy(p => p.ProductId)
                    .Skip(500).Take(500);

                using (collector.StartCollection())
                {
                    for (var i = 0; i < queriesPerIteration; i++)
                    {
                        if (async)
                        {
                            await query.ToListAsync();
                        }
                        else
                        {
                            query.ToList();
                        }
                    }
                }

                Assert.Equal(500, query.Count());
                Assert.False(tracking && (queriesPerIteration != 1), "Multiple queries per iteration not valid for tracking queries");
            }
        }

        [Benchmark]
        [BenchmarkVariation("Sync (10 queries)", false, 10)]
        [BenchmarkVariation("Async (10 queries)", true, 10)]
        public async Task GroupBy(IMetricCollector collector, bool async, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
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
                        if (async)
                        {
                            await query.ToListAsync();
                        }
                        else
                        {
                            query.ToList();
                        }
                    }
                }

                var result = query.ToList();
                Assert.Equal(10, result.Count);
                Assert.All(result, g => Assert.Equal(100, g.Products.Count()));
            }
        }

        [Benchmark]
        [BenchmarkVariation("Tracking On - Sync (1 query)", true, false, 1)]
        [BenchmarkVariation("Tracking Off - Sync (1 query)", false, false, 1)]
        [BenchmarkVariation("Tracking On - Async (1 query)", true, true, 1)]
        [BenchmarkVariation("Tracking Off - Async (1 query)", false, true, 1)]
        public async Task Include(IMetricCollector collector, bool tracking, bool async, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
            {
                var query = context.Customers
                    .ApplyTracking(tracking)
                    .Include(c => c.Orders);

                using (collector.StartCollection())
                {
                    for (var i = 0; i < queriesPerIteration; i++)
                    {
                        if (async)
                        {
                            await query.ToListAsync();
                        }
                        else
                        {
                            query.ToList();
                        }
                    }
                }

                var result = query.ToList();
                Assert.Equal(1000, result.Count);
                Assert.Equal(2000, result.SelectMany(c => c.Orders).Count());
                Assert.False(tracking && (queriesPerIteration != 1), "Multiple queries per iteration not valid for tracking queries");
            }
        }

        [Benchmark]
        [BenchmarkVariation("Sync (10 queries)", false, 10)]
        [BenchmarkVariation("Async (10 queries)", true, 10)]
        public async Task Projection(IMetricCollector collector, bool async, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
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
                        if (async)
                        {
                            await query.ToListAsync();
                        }
                        else
                        {
                            query.ToList();
                        }
                    }
                }

                Assert.Equal(1000, query.Count());
            }
        }

        [Benchmark]
        [BenchmarkVariation("Sync (10 queries)", false, 10)]
        [BenchmarkVariation("Async (10 queries)", true, 10)]
        public async Task ProjectionAcrossNavigation(IMetricCollector collector, bool async, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
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
                        if (async)
                        {
                            await query.ToListAsync();
                        }
                        else
                        {
                            query.ToList();
                        }
                    }
                }

                Assert.Equal(2000, query.Count());
            }
        }

        public class SimpleQueryFixture : OrdersFixture
        {
            public SimpleQueryFixture()
                : base("Perf_Query_Simple", 1000, 1000, 2, 2)
            {
            }
        }
    }
}
