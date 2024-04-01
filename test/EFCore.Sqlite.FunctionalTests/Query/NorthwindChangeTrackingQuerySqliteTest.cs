// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindChangeTrackingQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture) : NorthwindChangeTrackingQueryTestBase<
    NorthwindQuerySqliteFixture<NoopModelCustomizer>>(fixture)
{
    protected override NorthwindContext CreateNoTrackingContext()
        => new NorthwindSqliteContext(
            new DbContextOptionsBuilder(Fixture.CreateOptions())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options);
}
