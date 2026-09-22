// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

public class OrdersSqliteFixture(string databaseName) : OrdersFixtureBase
{
    private static readonly string _baseDirectory
        = Path.GetDirectoryName(typeof(OrdersSqliteFixture).Assembly.Location);

    private readonly string _connectionString = $"Data Source={Path.Combine(_baseDirectory, databaseName + ".db")}";

    public override OrdersContextBase CreateContext(IServiceProvider serviceProvider = null, bool disableBatching = false)
        => new OrdersSqliteContext(_connectionString, serviceProvider, disableBatching);
}
