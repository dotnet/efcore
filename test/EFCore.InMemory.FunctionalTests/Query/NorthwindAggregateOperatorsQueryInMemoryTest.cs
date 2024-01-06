// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindAggregateOperatorsQueryInMemoryTest(NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture)
    : NorthwindAggregateOperatorsQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>(fixture)
{

    // InMemory can throw server side exception
    public override async Task Average_no_data_subquery(bool async)
        => Assert.Equal(
            "Sequence contains no elements",
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Average_no_data_subquery(async))).Message);

    public override async Task Max_no_data_subquery(bool async)
        => Assert.Equal(
            "Sequence contains no elements",
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Max_no_data_subquery(async))).Message);

    public override async Task Min_no_data_subquery(bool async)
        => Assert.Equal(
            "Sequence contains no elements",
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Min_no_data_subquery(async))).Message);

    public override async Task Average_on_nav_subquery_in_projection(bool async)
        => Assert.Equal(
            "Sequence contains no elements",
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Average_on_nav_subquery_in_projection(async))).Message);

    public override Task Collection_Last_member_access_in_projection_translated(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Collection_Last_member_access_in_projection_translated(async));

    // Issue #31776
    public override async Task Contains_with_local_enumerable_inline(bool async)
        => await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await base.Contains_with_local_enumerable_inline(async));

    // Issue #31776
    public override async Task Contains_with_local_enumerable_inline_closure_mix(bool async)
        => await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await base.Contains_with_local_enumerable_inline_closure_mix(async));
}
