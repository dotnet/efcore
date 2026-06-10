// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindChangeTrackingQueryInMemoryTest(NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture)
    : NorthwindChangeTrackingQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>(fixture)
{
    protected override NorthwindContext CreateNoTrackingContext()
        => new NorthwindInMemoryContext(
            new DbContextOptionsBuilder(Fixture.CreateOptions())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options);
}
