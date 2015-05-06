// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.EF6.Models.Orders;
using Xunit;

namespace EntityFramework.Microbenchmarks.Query
{
    public class FuncletizationTests
    {
        private static readonly string _connectionString 
            = $@"Server={TestConfig.Instance.DataSource};Database=Perf_Query_Funcletization_EF6;Integrated Security=True;MultipleActiveResultSets=true;";

        private static readonly int _funcletizationIterationCount = 100;

        [Fact]
        public void NewQueryInstance()
        {
            new TestDefinition
                {
                    TestName = "Query_Funcletization_NewQueryInstance_EF6",
                    IterationCount = 50,
                    WarmupCount = 5,
                    Setup = EnsureDatabaseSetup,
                    Run = harness =>
                        {
                            using (var context = new OrdersContext(_connectionString))
                            {
                                using (harness.StartCollection())
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
                }.RunTest();
        }

        [Fact]
        public void SameQueryInstance()
        {
            new TestDefinition
                {
                    TestName = "Query_Funcletization_SameQueryInstance_EF6",
                    IterationCount = 50,
                    WarmupCount = 5,
                    Setup = EnsureDatabaseSetup,
                    Run = harness =>
                        {
                            using (var context = new OrdersContext(_connectionString))
                            {
                                using (harness.StartCollection())
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
                }.RunTest();
        }

        [Fact]
        public void ValueFromObject()
        {
            new TestDefinition
                {
                    TestName = "Query_Funcletization_ValueFromObject_EF6",
                    IterationCount = 50,
                    WarmupCount = 5,
                    Setup = EnsureDatabaseSetup,
                    Run = harness =>
                        {
                            using (var context = new OrdersContext(_connectionString))
                            {
                                using (harness.StartCollection())
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
                }.RunTest();
        }

        public class ValueHolder
        {
            public int FirstLevelProperty { get; } = 11;

            public int SecondLevelProperty
            {
                get { return FirstLevelProperty; }
            }
        }

        private static void EnsureDatabaseSetup()
        {
            new OrdersSeedData().EnsureCreated(
                _connectionString,
                productCount: 100,
                customerCount: 0,
                ordersPerCustomer: 0,
                linesPerOrder: 0);
        }
    }
}
