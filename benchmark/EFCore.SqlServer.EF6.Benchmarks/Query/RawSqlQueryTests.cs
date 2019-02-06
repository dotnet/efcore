// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;
using Xunit;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Microsoft.EntityFrameworkCore.Benchmarks.Query
{
    public class RawSqlQueryTests
    {
        private static readonly OrdersFixture _fixture
            = new OrdersFixture("Perf_Query_RawSql_EF6", 1000, 1000, 2, 2,
                ctx => ctx.Database.ExecuteSqlCommand(
                    @"CREATE PROCEDURE dbo.SearchProducts
                        @minPrice decimal(18, 2),
                        @maxPrice decimal(18, 2)
                    AS
                    BEGIN
                        SELECT * FROM dbo.Products WHERE CurrentPrice >= @minPrice AND CurrentPrice <= @maxPrice
                    END"));
        private OrdersContext _context;

        [Params(true, false)]
        public bool Async;

        [Params(true, false)]
        public bool Tracking;

        [GlobalSetup]
        public virtual void CreateContext()
        {

            _context = _fixture.CreateContext();

            Assert.Equal(1000, _context.Products.Count());
            Assert.Equal(1000, _context.Customers.Count());
            Assert.Equal(2000, _context.Orders.Count());
            Assert.Equal(4000, _context.OrderLines.Count());
        }

        [GlobalCleanup]
        public virtual void CleanupContext()
        {
            _context.Dispose();
        }

        [Benchmark]
        public virtual async Task SelectAll()
        {
            var query = _context.Products
                .SqlQuery("SELECT * FROM dbo.Products")
                .ApplyTracking(Tracking);

            if (Async)
            {
                await query.ToListAsync();
            }
            else
            {
                query.ToList();
            }
        }

        [Benchmark]
        public virtual async Task SelectParameterized()
        {
            var query = _context.Products
                .SqlQuery("SELECT * FROM dbo.Products WHERE CurrentPrice >= @p0 AND CurrentPrice <= @p1", 10, 14)
                .ApplyTracking(Tracking);

            if (Async)
            {
                await query.ToListAsync();
            }
            else
            {
                query.ToList();
            }
        }

        [Benchmark]
        // NOTE: Composition is done on client, so there is no async version
        public virtual void SelectComposed()
        {
            var query = _context.Products
                .SqlQuery("SELECT * FROM dbo.Products")
                .ApplyTracking(Tracking)
                .Where(p => p.CurrentPrice >= 10 && p.CurrentPrice <= 14)
                .OrderBy(p => p.Name);

            query.ToList();
        }

        [Benchmark]
        public virtual async Task StoredProcedure()
        {
            var query = _context.Products
                .SqlQuery("EXECUTE dbo.SearchProducts @p0, @p1", 10, 14)
                .ApplyTracking(Tracking);

            if (Async)
            {
                await query.ToListAsync();
            }
            else
            {
                query.ToList();
            }
        }
    }
}
