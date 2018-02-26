// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

#pragma warning disable CA1034 // Nested types should not be visible

namespace Microsoft.EntityFrameworkCore.Benchmarks.ChangeTracker
{
    public class DbSetOperationSqliteTests : DbSetOperationTests
    {
        public class AddDataVariations : AddDataVariationsBase
        {
            public override OrdersFixtureBase CreateFixture()
            {
                return new OrdersFixture("Perf_ChangeTracker_DbSetOperation");
            }
        }

        public class ExistingDataVariations : ExistingDataVariationsBase
        {
            public override OrdersFixtureBase CreateFixture()
            {
                return new OrdersFixture("Perf_ChangeTracker_DbSetOperation");
            }
        }
    }
}
