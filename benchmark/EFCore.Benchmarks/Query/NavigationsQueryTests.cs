// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Query;

[DisplayName(nameof(NavigationsQueryTests))]
public abstract class NavigationsQueryTests
{
    private AdventureWorksContextBase _context;
    private IQueryable<Store> _query;

    protected virtual int QueriesPerIteration
        => 10;

    protected virtual int UnfilteredCount
        => 466;

    [Params(true, false)]
    public bool Async { get; set; }

    [Params(true, false)]
    public bool Filter { get; set; }

    protected abstract AdventureWorksContextBase CreateContext();

    [GlobalSetup]
    public virtual void InitializeContext()
    {
        _context = CreateContext();
        _query = Filter
            ? _context.Store.Where(s => s.SalesPerson.Bonus > 3000)
            : _context.Store.Where(s => s.SalesPerson.Bonus >= 0);
    }

    [GlobalCleanup]
    public virtual void CleanupContext()
    {
        Assert.Equal(Filter ? UnfilteredCount : 701, _query.Count());

        _context.Dispose();
    }

    [Benchmark]
    public virtual async Task PredicateAcrossOptionalNavigation()
    {
        for (var i = 0; i < QueriesPerIteration; i++)
        {
            if (Async)
            {
                await _query.ToListAsync();
            }
            else
            {
                _query.ToList();
            }
        }
    }
}
