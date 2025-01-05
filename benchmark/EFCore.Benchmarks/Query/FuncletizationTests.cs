// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;
using Xunit;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Microsoft.EntityFrameworkCore.Benchmarks.Query;

[DisplayName(nameof(FuncletizationTests))]
public abstract class FuncletizationTests
{
    private OrdersContextBase _context;

    protected virtual int FuncletizationIterationCount
        => 100;

    protected abstract OrdersFixtureBase CreateFixture();

    [GlobalSetup]
    public virtual void InitializeContext()
    {
        var fixture = CreateFixture();
        fixture.Initialize(100, 0, 0, 0);

        _context = fixture.CreateContext();

        Assert.Equal(100, _context.Products.Count());
    }

    [GlobalCleanup]
    public virtual void CleanupContext()
        => _context.Dispose();

    [Benchmark]
    public virtual void NewQueryInstance()
    {
        var val = 11;
        for (var i = 0; i < FuncletizationIterationCount; i++)
        {
            _context.Products.Where(p => p.ProductId < val).ToList();
        }
    }

    [Benchmark]
    public virtual void SameQueryInstance()
    {
        var val = 11;
        var query = _context.Products.Where(p => p.ProductId < val);

        for (var i = 0; i < FuncletizationIterationCount; i++)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            query.ToList();
        }
    }

    [Benchmark]
    public virtual void ValueFromObject()
    {
        var valueHolder = new ValueHolder();
        for (var i = 0; i < FuncletizationIterationCount; i++)
        {
            _context.Products.Where(p => p.ProductId < valueHolder.SecondLevelProperty).ToList();
        }
    }

    protected class ValueHolder
    {
        public int FirstLevelProperty { get; } = 11;

        public int SecondLevelProperty
            => FirstLevelProperty;
    }
}
