// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Benchmarks.UpdatePipeline;

public class BulkImportSqlServerTests
{
    private OrdersSqlServerFixture _fixture = null!;
    private OrdersContextBase _context = null!;
    private IDbContextTransaction _transaction = null!;
    private List<Customer> _customers = null!;

    [Params(1000, 10000)]
    public int RowCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _fixture = new OrdersSqlServerFixture("Perf_BulkImport");
        _fixture.Initialize(0, 0, 0, 0);
        _context = _fixture.CreateContext();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _transaction = _context.Database.BeginTransaction();
        _customers = _fixture.CreateCustomers(RowCount, setPrimaryKeys: false);
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _transaction.Dispose();
        _context.ChangeTracker.Clear();
    }

    [Benchmark(Baseline = true)]
    public async Task AddRange_SaveChanges()
    {
        await _context.Customers.AddRangeAsync(_customers);
        await _context.SaveChangesAsync();
    }

    [Benchmark]
    public async Task ImportFromAsync()
    {
        await _context.Customers.ImportFromAsync(_customers);
    }
}
