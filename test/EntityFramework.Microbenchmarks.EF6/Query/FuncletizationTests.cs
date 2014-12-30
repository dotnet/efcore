// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.EF6.Models.Orders;
using System;
using System.Linq;
using Xunit;

namespace EntityFramework.Microbenchmarks.Query
{
    public class FuncletizationTests
    {
        private static string _connectionString = String.Format(@"Server={0};Database=Perf_Query_Funcletization_EF6;Integrated Security=True;MultipleActiveResultSets=true;", TestConfig.Instance.DataSource);
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
                            for (int i = 0; i < _funcletizationIterationCount; i++)
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

                            for (int i = 0; i < _funcletizationIterationCount; i++)
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
                            for (int i = 0; i < _funcletizationIterationCount; i++)
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
            private int value = 11;

            public int FirstLevelProperty
            {
                get { return value; }
            }

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
