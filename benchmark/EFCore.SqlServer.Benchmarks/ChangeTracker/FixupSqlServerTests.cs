// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

#pragma warning disable CA1034 // Nested types should not be visible

namespace Microsoft.EntityFrameworkCore.Benchmarks.ChangeTracker;

public class FixupSqlServerTests : FixupTests
{
    public class ChildVariations : ChildVariationsBase
    {
        public override OrdersFixtureBase CreateFixture()
            => new OrdersSqlServerFixture("Perf_ChangeTracker_Fixup");
    }

    public class ParentVariations : ParentVariationsBase
    {
        public override OrdersFixtureBase CreateFixture()
            => new OrdersSqlServerFixture("Perf_ChangeTracker_Fixup");
    }
}
