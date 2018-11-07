// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;
using Xunit;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Microsoft.EntityFrameworkCore.Benchmarks.Query
{
    [BenchmarkJob]
    [MemoryDiagnoser]
    public class NavigationsQueryTests
    {
        private AdventureWorksContext _context;
        private IQueryable<Store> _query;
        private const int _queriesPerIteration = 10;

        [Params(true, false)]
        public bool Async { get; set; }

        [Params(true, false)]
        public bool Filter { get; set; }

        [GlobalSetup]
        public virtual void InitializeContext()
        {
            _context = AdventureWorksFixture.CreateContext();
            _query = Filter
                ? _context.Store.Where(s => s.SalesPerson.Bonus > 3000)
                : _context.Store.Where(s => s.SalesPerson.Bonus >= 0);
        }

        [GlobalCleanup]
        public virtual void CleanupContext()
        {
            Assert.Equal(Filter ? 466 : 701, _query.Count());

            _context.Dispose();
        }

        [Benchmark]
        public virtual async Task PredicateAcrossOptionalNavigation()
        {
            for (var i = 0; i < _queriesPerIteration; i++)
            {
                if (Async)
                {
                    await _query.ToListAsync();
                }
                else
                {
                    _query.ToList();
                }
            }
        }
    }
}
