// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

#pragma warning disable CA1034 // Nested types should not be visible

namespace Microsoft.EntityFrameworkCore.Benchmarks.ChangeTracker;

public class DbSetOperationSqlServerTests : DbSetOperationTests
{
    public class AddDataVariations : AddDataVariationsBase
    {
        public override OrdersFixtureBase CreateFixture()
            => new OrdersSqlServerFixture("Perf_ChangeTracker_DbSetOperation");
    }

    public class ExistingDataVariations : ExistingDataVariationsBase
    {
        public override OrdersFixtureBase CreateFixture()
            => new OrdersSqlServerFixture("Perf_ChangeTracker_DbSetOperation");
    }
}
