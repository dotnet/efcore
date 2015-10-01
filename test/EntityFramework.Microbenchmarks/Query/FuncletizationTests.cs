// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Models.Orders;
using Xunit;

namespace EntityFramework.Microbenchmarks.Query
{
    public class FuncletizationTests : IClassFixture<FuncletizationTests.FuncletizationFixture>
    {
        private readonly FuncletizationFixture _fixture;
        private static readonly int _funcletizationIterationCount = 100;

        public FuncletizationTests(FuncletizationFixture fixture)
        {
            _fixture = fixture;
        }

        [Benchmark]
        public void NewQueryInstance(IMetricCollector collector)
        {

            using (var context = _fixture.CreateContext())
            {
                using (collector.StartCollection())
                {
                    var val = 11;
                    for (var i = 0; i < _funcletizationIterationCount; i++)
                    {
                        var result = context.Products.Where(p => p.ProductId < val).ToList();

                        Assert.Equal(10, result.Count);
                    }
                }
            }
        }

        [Benchmark]
        public void SameQueryInstance(IMetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                using (collector.StartCollection())
                {
                    var val = 11;
                    var query = context.Products.Where(p => p.ProductId < val);

                    for (var i = 0; i < _funcletizationIterationCount; i++)
                    {
                        var result = query.ToList();

                        Assert.Equal(10, result.Count);
                    }
                }
            }
        }

        [Benchmark]
        public void ValueFromObject(IMetricCollector collector)
        {
            using (var context = _fixture.CreateContext())
            {
                using (collector.StartCollection())
                {
                    var valueHolder = new ValueHolder();
                    for (var i = 0; i < _funcletizationIterationCount; i++)
                    {
                        var result = context.Products.Where(p => p.ProductId < valueHolder.SecondLevelProperty).ToList();

                        Assert.Equal(10, result.Count);
                    }
                }
            }
        }

        public class ValueHolder
        {
            public int FirstLevelProperty { get; } = 11;

            public int SecondLevelProperty
            {
                get { return FirstLevelProperty; }
            }
        }

        public class FuncletizationFixture : OrdersFixture
        {
            public FuncletizationFixture()
                : base("Perf_Query_Funcletization", 100, 0, 0, 0)
            { }
        }
    }
}
