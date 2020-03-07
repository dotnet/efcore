// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;
using Xunit;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Microsoft.EntityFrameworkCore.Benchmarks.Query
{
    public class SimpleQueryTests
    {
        private static readonly OrdersFixture _fixture
            = new OrdersFixture("Perf_Query_Simple_EF6", 1000, 1000, 2, 2);

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
        }

        [GlobalCleanup]
        public virtual void CleanupContext()
        {
            _context.Dispose();
        }

        [Benchmark]
        public virtual async Task LoadAll()
        {
            var query = _context.Products
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
        public virtual async Task Where()
        {
            var query = _context.Products
                .ApplyTracking(Tracking)
                .Where(p => p.Retail < 15);

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
        public virtual async Task OrderBy()
        {
            var query = _context.Products
                .ApplyTracking(Tracking)
                .OrderBy(p => p.Retail);

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
        public virtual async Task Count()
        {
            var query = _context.Products;

            if (Async)
            {
                await query.CountAsync();
            }
            else
            {
                query.Count();
            }
        }

        [Benchmark]
        public virtual async Task SkipTake()
        {
            var query = _context.Products
                .ApplyTracking(Tracking)
                .OrderBy(p => p.ProductId)
                .Skip(500)
                .Take(500);

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
        public virtual async Task GroupBy()
        {
            var query = _context.Products
                .GroupBy(p => p.Retail)
                .Select(
                    g => new { Retail = g.Key, Products = g });

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
        public virtual async Task Include()
        {
            var query = _context.Customers
                .ApplyTracking(Tracking)
                .Include(c => c.Orders);

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
        public virtual async Task Projection()
        {
            var query = _context.Products
                .Select(
                    p => new
                    {
                        p.ProductId,
                        p.Name,
                        p.Description,
                        p.SKU,
                        p.Retail,
                        p.CurrentPrice,
                        p.ActualStockLevel
                    });

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
        public virtual async Task ProjectionAcrossNavigation()
        {
            var query = _context.Orders
                .Select(
                    o => new
                    {
                        CustomerTitle = o.Customer.Title,
                        CustomerFirstName = o.Customer.FirstName,
                        CustomerLastName = o.Customer.LastName,
                        OrderDate = o.Date,
                        o.OrderDiscount,
                        OrderDiscountReason = o.DiscountReason,
                        OrderTax = o.Tax,
                        OrderSpecialRequests = o.SpecialRequests
                    });

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
