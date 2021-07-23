// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders
{
    public class OrdersSqlServerContext : OrdersContextBase
    {
        private readonly string _connectionString;
        private readonly bool _disableBatching;

        public OrdersSqlServerContext(string connectionString, IServiceProvider serviceProvider = null, bool disableBatching = false)
            : base(serviceProvider)
        {
            _connectionString = connectionString;
            _disableBatching = disableBatching;
        }

        protected override void ConfigureProvider(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                _connectionString, b =>
                {
                    if (_disableBatching) { b.MaxBatchSize(1); }
                });
        }
    }
}
