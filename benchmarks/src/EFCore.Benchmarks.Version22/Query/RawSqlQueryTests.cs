// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;
using Xunit;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable FormatStringProblem

namespace Microsoft.EntityFrameworkCore.Benchmarks.Query
{
    [DisplayName(nameof(RawSqlQueryTests))]
    public abstract class RawSqlQueryTests
    {
        private OrdersContextBase _context;

        protected abstract OrdersFixtureBase CreateFixture();
        protected abstract string StoredProcedureCreationScript { get; }

        [Params(true, false)]
        public virtual bool Async { get; set; }

        [Params(true, false)]
        public virtual bool Tracking { get; set; }

        [GlobalSetup]
        public virtual void CreateContext()
        {
            var fixture = CreateFixture();
            fixture.Initialize(
                1000, 1000, 2, 2,
                ctx =>
                {
                    if (!string.IsNullOrEmpty(StoredProcedureCreationScript))
                    {
                        ctx.Database.ExecuteSqlCommand(StoredProcedureCreationScript);
                    }
                });

            _context = fixture.CreateContext();

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
                .FromSql(@"SELECT * FROM ""Products""")
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
                .FromSql(@"SELECT * FROM ""Products"" WHERE ""CurrentPrice"" >= @p0 AND ""CurrentPrice"" <= @p1", 10, 14)
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
        public virtual async Task SelectComposed()
        {
            var query = _context.Products
                .FromSql(@"SELECT * FROM ""Products""")
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
        public virtual async Task StoredProcedure()
        {
            var query = _context.Products
                .FromSql(@"EXECUTE dbo.SearchProducts @p0, @p1", 10, 14)
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
