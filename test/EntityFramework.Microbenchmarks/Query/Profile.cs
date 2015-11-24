// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Models.Orders;
using Microsoft.Data.Entity;

namespace EntityFramework.Microbenchmarks.Query
{
    public class Profile : IDisposable
    {
        private readonly OrdersContext _context;
        private readonly IQueryable<object> _query;

        public Profile()
        {
            var connectionString
                = $@"Server={BenchmarkConfig.Instance.BenchmarkDatabaseInstance};Database=Perf_Query_Simple;Integrated Security=True;MultipleActiveResultSets=true;";

            _context = new OrdersContext(connectionString);

            //_query = _context.Products.AsNoTracking().Where(p => p.Retail < 15);

            _query = _context.Products
                .AsNoTracking()
                .Select(p => new
                {
                    p.ProductId,
                    p.Name,
                    p.Description,
                    p.SKU,
                    p.Retail,
                    p.CurrentPrice,
                    p.ActualStockLevel
                });

            _query.Load();
        }

        public void Run()
        {
            for (var i = 0; i < 2000; i++)
            {
                _query.Load();
            }
        }

        public void Dispose() => _context.Dispose();
    }
}
