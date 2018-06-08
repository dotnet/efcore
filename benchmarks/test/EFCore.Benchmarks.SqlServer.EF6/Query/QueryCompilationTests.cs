// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Benchmarks.Query
{
    [BenchmarkJob]
    [MemoryDiagnoser]
    public class QueryCompilationTests
    {
        private static readonly OrdersFixture _fixture
            = new OrdersFixture("Perf_Query_Compilation_EF6", 0, 0, 0, 0);
        private OrdersContext _context;
        private IQueryable<Product> _simpleQuery;
        private IQueryable<DTO> _complexQuery;

        [GlobalSetup]
        public virtual void InitializeContext()
        {
            _context = _fixture.CreateContext();
            _simpleQuery = _context.Products
                .AsNoTracking()
                .DisableQueryCache();
            _complexQuery = _context.Products
                .AsNoTracking()
                .DisableQueryCache()
                .Where(p => p.Retail < 1000)
                .OrderBy(p => p.Name).ThenBy(p => p.Retail)
                .Select(
                    p => new DTO
                    {
                        ProductId = p.ProductId,
                        Name = p.Name,
                        Description = p.Description,
                        ActualStockLevel = p.ActualStockLevel,
                        SKU = p.SKU,
                        Savings = p.Retail - p.CurrentPrice,
                        Surplus = p.ActualStockLevel - p.TargetStockLevel
                    });
        }

        [GlobalCleanup]
        public virtual void CleanupContext()
        {
            _context.Dispose();
        }

        [Benchmark]
        public virtual void ToList()
        {
            for (var i = 0; i < 10; i++)
            {
                _simpleQuery.ToList();
            }
        }

        [Benchmark]
        public virtual void FilterOrderProject()
        {
            for (var i = 0; i < 10; i++)
            {
                _complexQuery.ToList();
            }
        }

        private class DTO
        {
            public int ProductId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int ActualStockLevel { get; set; }
            public string SKU { get; set; }
            public decimal Savings { get; set; }
            public int Surplus { get; set; }
        }
    }
}
