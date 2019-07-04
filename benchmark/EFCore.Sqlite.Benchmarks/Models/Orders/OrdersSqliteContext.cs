// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders
{
    public class OrdersSqliteContext : OrdersContextBase
    {
        private readonly string _connectionString;
        private readonly bool _disableBatching;

        public OrdersSqliteContext(string connectionString, IServiceProvider serviceProvider = null, bool disableBatching = false)
            : base(serviceProvider)
        {
            _connectionString = connectionString;
            _disableBatching = disableBatching;
        }

        protected override void ConfigureProvider(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString, b => { if (_disableBatching) { b.MaxBatchSize(1); } });
        }
    }
}
