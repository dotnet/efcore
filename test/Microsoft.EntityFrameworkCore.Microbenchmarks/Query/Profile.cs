// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Microbenchmarks.Core;
using Microsoft.EntityFrameworkCore.Microbenchmarks.Models.Orders;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Microbenchmarks.Query
{
    // Test class for manual profiling work.
    public class Profile : IDisposable
    {
        private readonly OrdersContext _context;
        //private readonly IQueryable<object> _query;

        public Profile()
        {
            var connectionString
                = $@"Server={BenchmarkConfig.Instance.BenchmarkDatabaseInstance};Database=Perf_Query_Simple;Integrated Security=True;MultipleActiveResultSets=true;";

            _context = new OrdersContext(connectionString);

            //_query = _context.Products.AsNoTracking().Where(p => p.Retail < 15);

            var product
                = (from p in _context.Products
                   from p2 in _context.Products
                   select new { p, p2 })
                    .Select(a => a.p.Name)
                    .OrderBy(n => n)
                    .AsNoTracking()
                    .First();

            //_query.Load();
        }

        //[Fact]
        public void Run()
        {
            for (var i = 0; i < 1; i++)
            {
                var product
                    = (from p in _context.Products
                       from p2 in _context.Products
                       select new { p, p2 })
                        .Select(a => a.p.Name)
                        .OrderBy(n => n)
                        .AsNoTracking()
                        .First();

                Assert.NotNull(product);
            }
        }

        public void Dispose() => _context.Dispose();
    }
}
