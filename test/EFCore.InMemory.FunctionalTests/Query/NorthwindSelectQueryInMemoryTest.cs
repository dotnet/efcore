// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindSelectQueryInMemoryTest(NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture)
    : NorthwindSelectQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>(fixture)
{
    public override Task
        SelectMany_with_collection_being_correlated_subquery_which_references_non_mapped_properties_from_inner_and_outer_entity(bool async)
        => Assert.ThrowsAsync<NotImplementedException>(() => base
            .SelectMany_with_collection_being_correlated_subquery_which_references_non_mapped_properties_from_inner_and_outer_entity(
                async));

    public override Task SelectMany_over_inline_array_projecting_range_variable_and_outer(bool async)
        => AssertTranslationFailed(() => base.SelectMany_over_inline_array_projecting_range_variable_and_outer(async));

    // InMemory doesn't translate ElementAt in a subquery, and doesn't opt in to lifting it into a join.
    public override Task Multiple_members_of_correlated_single_result_subquery_lift_to_single_join(bool async, string method)
        => method is nameof(Queryable.ElementAt) or nameof(Queryable.ElementAtOrDefault)
            ? AssertTranslationFailed(() => base.Multiple_members_of_correlated_single_result_subquery_lift_to_single_join(async, method))
            : base.Multiple_members_of_correlated_single_result_subquery_lift_to_single_join(async, method);
}
