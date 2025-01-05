// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

public class OrdersSqlServerFixture(string databaseName) : OrdersFixtureBase
{
    private readonly string _connectionString = SqlServerBenchmarkEnvironment.CreateConnectionString(databaseName);

    public override OrdersContextBase CreateContext(IServiceProvider serviceProvider = null, bool disableBatching = false)
        => new OrdersSqlServerContext(_connectionString, serviceProvider, disableBatching);
}
