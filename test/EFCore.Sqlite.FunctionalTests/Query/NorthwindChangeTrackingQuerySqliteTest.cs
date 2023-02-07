// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindChangeTrackingQuerySqliteTest : NorthwindChangeTrackingQueryTestBase<
    NorthwindQuerySqliteFixture<NoopModelCustomizer>>
{
    public NorthwindChangeTrackingQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture)
        : base(fixture)
    {
    }

    protected override NorthwindContext CreateNoTrackingContext()
        => new NorthwindSqliteContext(
            new DbContextOptionsBuilder(Fixture.CreateOptions())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options);
}
