// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Query
{
    public class SimpleQuerySqlServerTests : SimpleQueryTests
    {
        protected override OrdersFixtureBase CreateFixture()
        {
            return new OrdersSqlServerFixture("Perf_Query_Simple");
        }
    }
}
