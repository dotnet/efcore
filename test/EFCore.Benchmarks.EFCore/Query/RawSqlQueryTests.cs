// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.EFCore.Models.Orders;
using Xunit;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable FormatStringProblem

namespace Microsoft.EntityFrameworkCore.Benchmarks.EFCore.Query
{
    public class RawSqlQueryTests
    {
        private static readonly RawSqlQueryFixture _fixture = new RawSqlQueryFixture();
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
        public async Task SelectAll()
        {
            var query = _context.Products
                .FromSql("SELECT * FROM dbo.Products")
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
        public async Task SelectParameterized()
        {
            var query = _context.Products
                .FromSql("SELECT * FROM dbo.Products WHERE CurrentPrice >= @p0 AND CurrentPrice <= @p1", 10, 14)
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
        public async Task SelectComposed()
        {
            var query = _context.Products
                .FromSql("SELECT * FROM dbo.Products")
                .ApplyTracking(Tracking)
                .Where(p => p.CurrentPrice >= 10 && p.CurrentPrice <= 14)
                .OrderBy(p => p.Name);

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
        public async Task StoredProcedure()
        {
            var query = _context.Products
                .FromSql("EXECUTE dbo.SearchProducts @p0, @p1", 10, 14)
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

        public class RawSqlQueryFixture : OrdersFixture
        {
            public RawSqlQueryFixture()
                : base("Perf_Query_RawSql", 1000, 1000, 2, 2)
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
