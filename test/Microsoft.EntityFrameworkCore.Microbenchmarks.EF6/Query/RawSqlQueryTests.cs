// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Microbenchmarks.Core;
using Microsoft.EntityFrameworkCore.Microbenchmarks.EF6.Models.Orders;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Microbenchmarks.EF6.Query
{
    [SqlServerRequired]
    public class RawSqlQueryTests : IClassFixture<RawSqlQueryTests.RawSqlQueryFixture>
    {
        private readonly RawSqlQueryFixture _fixture;

        public RawSqlQueryTests(RawSqlQueryFixture fixture)
        {
            _fixture = fixture;
        }

        [Benchmark]
        [BenchmarkVariation("Tracking On (1 query)", true, 1)]
        [BenchmarkVariation("Tracking Off (10 queries)", false, 10)]
        public void SelectAll(IMetricCollector collector, bool tracking, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
            {
                var query = context.Products
                    .SqlQuery("SELECT * FROM dbo.Products")
                    .ApplyTracking(tracking);

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
        [BenchmarkVariation("Tracking On (1 query)", true, 1)]
        [BenchmarkVariation("Tracking Off (10 queries)", false, 10)]
        public void SelectParameterized(IMetricCollector collector, bool tracking, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
            {
                var query = context.Products
                    .SqlQuery("SELECT * FROM dbo.Products WHERE CurrentPrice >= @p0 AND CurrentPrice <= @p1", 10, 14)
                    .ApplyTracking(tracking);

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
        [BenchmarkVariation("Tracking On (1 query)", true, 1)]
        [BenchmarkVariation("Tracking Off (10 queries)", false, 10)]
        public void SelectComposed(IMetricCollector collector, bool tracking, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
            {
                var query = context.Products
                    .SqlQuery("SELECT * FROM dbo.Products")
                    .ApplyTracking(tracking)
                    .Where(p => p.CurrentPrice >= 10 && p.CurrentPrice <= 14)
                    .OrderBy(p => p.Name);

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
        [BenchmarkVariation("Tracking On (1 query)", true, 1)]
        [BenchmarkVariation("Tracking Off (10 queries)", false, 10)]
        public void StoredProcedure(IMetricCollector collector, bool tracking, int queriesPerIteration)
        {
            using (var context = _fixture.CreateContext())
            {
                var query = context.Products
                    .SqlQuery("EXECUTE dbo.SearchProducts @p0, @p1", 10, 14)
                    .ApplyTracking(tracking);

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

        public class RawSqlQueryFixture : OrdersFixture
        {
            public RawSqlQueryFixture()
                : base("Perf_Query_RawSql_EF6", 1000, 1000, 2, 2)
            {
            }

            protected override void OnDatabaseCreated(OrdersContext context)
                => context.Database.ExecuteSqlCommand(
                    @"CREATE PROCEDURE dbo.SearchProducts
                        @minPrice decimal(18, 2),
                        @maxPrice decimal(18, 2)
                    AS
                    BEGIN
                        SELECT * FROM dbo.Products WHERE CurrentPrice >= @minPrice AND CurrentPrice <= @maxPrice
                    END");
        }
    }
}
