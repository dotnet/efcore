// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders
{
    public class OrdersSqliteFixture : OrdersFixtureBase
    {
        private static readonly string _baseDirectory
            = Path.GetDirectoryName(new Uri(typeof(OrdersSqliteFixture).Assembly.CodeBase).LocalPath);

        private readonly string _connectionString;

        public OrdersSqliteFixture(string databaseName)
        {
            _connectionString = $"Data Source={Path.Combine(_baseDirectory, databaseName + ".db")}";
        }

        public override OrdersContextBase CreateContext(IServiceProvider serviceProvider = null, bool disableBatching = false)
        {
            return new OrdersSqliteContext(_connectionString, serviceProvider, disableBatching);
        }
    }
}
