// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;
using Xunit;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Microsoft.EntityFrameworkCore.Benchmarks.Query
{
    public abstract class FuncletizationBase
    {
        private OrdersContextBase _context;

        public const int OperationsPerInvoke = 100;

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
        {
            _context.Dispose();
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public virtual void NewQueryInstance()
        {
            var val = 11;
            for (var i = 0; i < OperationsPerInvoke; i++)
            {
                _context.Products.Where(p => p.ProductId < val).ToList();
            }
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public virtual void SameQueryInstance()
        {
            var val = 11;
            var query = _context.Products.Where(p => p.ProductId < val);

            for (var i = 0; i < OperationsPerInvoke; i++)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                query.ToList();
            }
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public virtual void ValueFromObject()
        {
            var valueHolder = new ValueHolder();
            for (var i = 0; i < OperationsPerInvoke; i++)
            {
                _context.Products.Where(p => p.ProductId < valueHolder.SecondLevelProperty).ToList();
            }
        }

        protected class ValueHolder
        {
            public int FirstLevelProperty { get; } = 11;

            public int SecondLevelProperty => FirstLevelProperty;
        }
    }
}
