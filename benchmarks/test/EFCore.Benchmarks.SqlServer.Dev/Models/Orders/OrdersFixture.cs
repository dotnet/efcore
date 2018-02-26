// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders
{
    public class OrdersFixture : OrdersFixtureBase
    {
        private readonly string _connectionString;

        public OrdersFixture(string databaseName)
        {
            _connectionString = $@"{BenchmarkConfig.Instance.BenchmarkDatabase}Database={databaseName};";
        }

        public override OrdersContextBase CreateContext(IServiceProvider serviceProvider = null, bool disableBatching = false)
        {
            return new OrdersContext(_connectionString, serviceProvider, disableBatching);
        }
    }
}
