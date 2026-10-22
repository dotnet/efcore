// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindSelectQueryInMemoryTest(NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture)
    : NorthwindSelectQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>(fixture)
{
    public override Task
        SelectMany_with_collection_being_correlated_subquery_which_references_non_mapped_properties_from_inner_and_outer_entity(bool async)
        => Assert.ThrowsAsync<NotImplementedException>(
            () => base
                .SelectMany_with_collection_being_correlated_subquery_which_references_non_mapped_properties_from_inner_and_outer_entity(
                    async));

    public override async Task SelectMany_correlated_with_outer_3(bool async)
        // DefaultIfEmpty. Issue #17536.
        => await Assert.ThrowsAsync<EqualException>(() => base.SelectMany_correlated_with_outer_3(async));
}
