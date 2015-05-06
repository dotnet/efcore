// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Models.Orders;
using Microsoft.Data.Entity;
using Xunit;

namespace EntityFramework.Microbenchmarks.Query
{
    public class SimpleQueryTests
    {
        private static readonly string _connectionString 
            = $@"Server={TestConfig.Instance.DataSource};Database=Perf_Query_Simple;Integrated Security=True;MultipleActiveResultSets=true;";

        [Fact]
        public void LoadAll()
        {
            new TestDefinition
                {
                    TestName = "Query_Simple_LoadAll",
                    IterationCount = 100,
                    WarmupCount = 5,
                    Setup = EnsureDatabaseSetup,
                    Run = harness =>
                        {
                            using (var context = new OrdersContext(_connectionString))
                            {
                                harness.StartCollection();
                                var result = context.Products.ToList();
                                harness.StopCollection();
                                Assert.Equal(1000, result.Count);
                            }
                        }
                }.RunTest();
        }

        [Fact]
        public void Where()
        {
            new TestDefinition
                {
                    TestName = "Query_Simple_Where",
                    IterationCount = 100,
                    WarmupCount = 5,
                    Setup = EnsureDatabaseSetup,
                    Run = harness =>
                        {
                            using (var context = new OrdersContext(_connectionString))
                            {
                                harness.StartCollection();
                                var result = context.Products.Where(p => p.Retail < 15).ToList();
                                harness.StopCollection();
                                Assert.Equal(500, result.Count);
                            }
                        }
                }.RunTest();
        }

        [Fact]
        public void OrderBy()
        {
            new TestDefinition
                {
                    TestName = "Query_Simple_OrderBy",
                    IterationCount = 100,
                    WarmupCount = 5,
                    Setup = EnsureDatabaseSetup,
                    Run = harness =>
                        {
                            using (var context = new OrdersContext(_connectionString))
                            {
                                harness.StartCollection();
                                var result = context.Products.OrderBy(p => p.Retail).ToList();
                                harness.StopCollection();
                                Assert.Equal(1000, result.Count);
                            }
                        }
                }.RunTest();
        }

        [Fact]
        public void Count()
        {
            new TestDefinition
                {
                    TestName = "Query_Simple_Count",
                    IterationCount = 100,
                    WarmupCount = 5,
                    Setup = EnsureDatabaseSetup,
                    Run = harness =>
                        {
                            using (var context = new OrdersContext(_connectionString))
                            {
                                harness.StartCollection();
                                var result = context.Products.Count();
                                harness.StopCollection();
                                Assert.Equal(1000, result);
                            }
                        }
                }.RunTest();
        }

        [Fact]
        public void SkipTake()
        {
            new TestDefinition
                {
                    TestName = "Query_Simple_SkipTake",
                    IterationCount = 100,
                    WarmupCount = 5,
                    Setup = EnsureDatabaseSetup,
                    Run = harness =>
                        {
                            using (var context = new OrdersContext(_connectionString))
                            {
                                harness.StartCollection();
                                var result = context.Products
                                    .OrderBy(p => p.ProductId)
                                    .Skip(500).Take(500)
                                    .ToList();

                                harness.StopCollection();
                                Assert.Equal(500, result.Count);
                            }
                        }
                }.RunTest();
        }

        [Fact]
        public void GroupBy()
        {
            new TestDefinition
                {
                    TestName = "Query_Simple_GroupBy",
                    IterationCount = 100,
                    WarmupCount = 5,
                    Setup = EnsureDatabaseSetup,
                    Run = harness =>
                        {
                            using (var context = new OrdersContext(_connectionString))
                            {
                                harness.StartCollection();
                                var result = context.Products
                                    .GroupBy(p => p.Retail)
                                    .Select(g => new
                                        {
                                            Retail = g.Key,
                                            Products = g
                                        })
                                    .ToList();

                                harness.StopCollection();
                                Assert.Equal(10, result.Count);
                                Assert.All(result, g => Assert.Equal(100, g.Products.Count()));
                            }
                        }
                }.RunTest();
        }

        [Fact]
        public void Include()
        {
            new TestDefinition
                {
                    TestName = "Query_Simple_Include",
                    // TODO Increase iteration count once perf issues addressed
                    IterationCount = 2,
                    WarmupCount = 1,
                    Setup = EnsureDatabaseSetup,
                    Run = harness =>
                        {
                            using (var context = new OrdersContext(_connectionString))
                            {
                                harness.StartCollection();
                                var result = context.Customers.Include(c => c.Orders).ToList();
                                harness.StopCollection();
                                Assert.Equal(1000, result.Count);
                                Assert.Equal(2000, result.SelectMany(c => c.Orders).Count());
                            }
                        }
                }.RunTest();
        }

        [Fact]
        public void Projection()
        {
            new TestDefinition
                {
                    TestName = "Query_Simple_Projection",
                    IterationCount = 100,
                    WarmupCount = 5,
                    Setup = EnsureDatabaseSetup,
                    Run = harness =>
                        {
                            using (var context = new OrdersContext(_connectionString))
                            {
                                harness.StartCollection();
                                var result = context.Products.Select(p => new { p.Name, p.Retail }).ToList();
                                harness.StopCollection();
                                Assert.Equal(1000, result.Count);
                            }
                        }
                }.RunTest();
        }

        [Fact]
        public void ProjectionAcrossNavigation()
        {
            new TestDefinition
                {
                    TestName = "Query_Simple_ProjectionAcrossNavigation",
                    IterationCount = 100,
                    WarmupCount = 5,
                    Setup = EnsureDatabaseSetup,
                    Run = harness =>
                        {
                            using (var context = new OrdersContext(_connectionString))
                            {
                                harness.StartCollection();
                                // TODO Use navigation for projection when supported (#325)
                                var result = context.Orders.Join(
                                    context.Customers,
                                    o => o.CustomerId,
                                    c => c.CustomerId,
                                    (o, c) => new { CustomerName = c.Name, OrderDate = o.Date })
                                    .ToList();

                                harness.StopCollection();
                                Assert.Equal(2000, result.Count);
                            }
                        }
                }.RunTest();
        }

        [Fact]
        public void NoTracking()
        {
            new TestDefinition
                {
                    TestName = "Query_Simple_NoTracking",
                    IterationCount = 100,
                    WarmupCount = 5,
                    Setup = EnsureDatabaseSetup,
                    Run = harness =>
                        {
                            using (var context = new OrdersContext(_connectionString))
                            {
                                harness.StartCollection();
                                var result = context.Products.AsNoTracking().ToList();
                                harness.StopCollection();
                                Assert.Equal(1000, result.Count);
                            }
                        }
                }.RunTest();
        }

        private static void EnsureDatabaseSetup()
        {
            new OrdersSeedData().EnsureCreated(
                _connectionString,
                productCount: 1000,
                customerCount: 1000,
                ordersPerCustomer: 2,
                linesPerOrder: 2);
        }
    }
}
