// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Models.Orders;
using Xunit;

namespace EntityFramework.Microbenchmarks.Query
{
    public class FuncletizationTests
    {
        private static readonly string _connectionString
            = $@"Server={BenchmarkConfig.Instance.BenchmarkDatabaseInstance};Database=Perf_Query_Funcletization;Integrated Security=True;MultipleActiveResultSets=true;";

        private static readonly int _funcletizationIterationCount = 100;

        public FuncletizationTests()
        {
            new OrdersSeedData().EnsureCreated(
                _connectionString,
                productCount: 100,
                customerCount: 0,
                ordersPerCustomer: 0,
                linesPerOrder: 0);
        }

        [Benchmark(Iterations = 50, WarmupIterations = 5)]
        public void NewQueryInstance(MetricCollector collector)
        {

            using (var context = new OrdersContext(_connectionString))
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

        [Benchmark(Iterations = 50, WarmupIterations = 5)]
        public void SameQueryInstance(MetricCollector collector)
        {
            using (var context = new OrdersContext(_connectionString))
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

        [Benchmark(Iterations = 50, WarmupIterations = 5)]
        public void ValueFromObject(MetricCollector collector)
        {
            using (var context = new OrdersContext(_connectionString))
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
    }
}
