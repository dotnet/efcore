// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindKeylessEntitiesQueryInMemoryTest : NorthwindKeylessEntitiesQueryTestBase<
    NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
{
    public NorthwindKeylessEntitiesQueryInMemoryTest(
        NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture,
#pragma warning disable IDE0060 // Remove unused parameter
        ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
        : base(fixture)
    {
        //TestLoggerFactory.TestOutputHelper = testOutputHelper;
    }

    // mapping to view not supported on InMemory
    public override void KeylessEntity_by_database_view()
    {
    }

    public override void Entity_mapped_to_view_on_right_side_of_join()
    {
    }

    public override async Task KeylessEntity_with_included_nav(bool async)
        => await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.KeylessEntity_with_included_nav(async));

    public override async Task KeylessEntity_with_included_navs_multi_level(bool async)
        => await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.KeylessEntity_with_included_navs_multi_level(async));
}
