// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;
using Xunit;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable FormatStringProblem

namespace Microsoft.EntityFrameworkCore.Benchmarks.Query;

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
#if OLD_FROM_SQL
                        ctx.Database.ExecuteSqlCommand(StoredProcedureCreationScript);
#else
                    ctx.Database.ExecuteSqlRaw(StoredProcedureCreationScript);
#endif
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
        => _context.Dispose();

    [Benchmark]
    public virtual async Task SelectAll()
    {
        var sql = @"SELECT * FROM ""Products""";
        var query = _context.Products
#if OLD_FROM_SQL
                .FromSql(sql)
#else
            .FromSqlRaw(sql)
#endif
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
        var sql = @"SELECT * FROM ""Products"" WHERE ""CurrentPrice"" >= @p0 AND ""CurrentPrice"" <= @p1";
        var query = _context.Products
#if OLD_FROM_SQL
                .FromSql(sql, 10, 14)
#else
            .FromSqlRaw(sql, 10, 14)
#endif
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
        var sql = @"SELECT * FROM ""Products""";
        var query = _context.Products
#if OLD_FROM_SQL
                .FromSql(sql)
#else
            .FromSqlRaw(sql)
#endif
            .ApplyTracking(Tracking)
            .Where(p => p.ActualStockLevel >= 2 && p.ActualStockLevel <= 6)
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
        var sql = @"EXECUTE dbo.SearchProducts @p0, @p1";
        var query = _context.Products
#if OLD_FROM_SQL
                .FromSql(sql, 10, 14)
#else
            .FromSqlRaw(sql, 10, 14)
#endif
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
