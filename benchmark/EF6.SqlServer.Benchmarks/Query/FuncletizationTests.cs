// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;
using Xunit;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Microsoft.EntityFrameworkCore.Benchmarks.Query
{
    public class FuncletizationTests
    {
        private static readonly OrdersFixture _fixture
            = new OrdersFixture("Perf_Query_Funcletization_EF6", 100, 0, 0, 0);

        private const int _funcletizationIterationCount = 100;

        private OrdersContext _context;

        [GlobalSetup]
        public virtual void InitializeContext()
        {
            _context = _fixture.CreateContext();

            Assert.Equal(100, _context.Products.Count());
        }

        [GlobalCleanup]
        public virtual void CleanupContext()
        {
            _context.Dispose();
        }

        [Benchmark]
        public virtual void NewQueryInstance()
        {
            var val = 11;
            for (var i = 0; i < _funcletizationIterationCount; i++)
            {
                _context.Products.Where(p => p.ProductId < val).ToList();
            }
        }

        [Benchmark]
        public virtual void SameQueryInstance()
        {
            var val = 11;
            var query = _context.Products.Where(p => p.ProductId < val);

            for (var i = 0; i < _funcletizationIterationCount; i++)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                query.ToList();
            }
        }

        [Benchmark]
        public virtual void ValueFromObject()
        {
            var valueHolder = new ValueHolder();
            for (var i = 0; i < _funcletizationIterationCount; i++)
            {
                _context.Products.Where(p => p.ProductId < valueHolder.SecondLevelProperty).ToList();
            }
        }

        private class ValueHolder
        {
            public int FirstLevelProperty { get; } = 11;

            public int SecondLevelProperty => FirstLevelProperty;
        }
    }
}
