// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;
using Xunit;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Microsoft.EntityFrameworkCore.Benchmarks.Query;

[DisplayName(nameof(SimpleQueryTests))]
public abstract class SimpleQueryTests
{
    private OrdersContextBase _context;

    protected abstract OrdersFixtureBase CreateFixture();

    [Params(true, false)]
    public virtual bool Async { get; set; }

    [Params(true, false)]
    public virtual bool Tracking { get; set; }

    [GlobalSetup]
    public virtual void CreateContext()
    {
        var fixture = CreateFixture();
        fixture.Initialize(1000, 1000, 2, 2);

        _context = fixture.CreateContext();

        Assert.Equal(1000, _context.Products.Count());
        Assert.Equal(1000, _context.Customers.Count());
        Assert.Equal(2000, _context.Orders.Count());
    }

    [GlobalCleanup]
    public virtual void CleanupContext()
        => _context.Dispose();

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
            .Where(p => p.ActualStockLevel < 5);

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
            .OrderBy(p => p.ActualStockLevel);

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

    // Disabled because of current state of query pipeline
    // [Benchmark]
    public virtual async Task GroupBy()
    {
        var query = _context.Products
            .GroupBy(p => p.ActualStockLevel)
            .Select(
                g => new { ActualStockLevel = g.Key, Products = g });

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
