// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindMiscellaneousQueryInMemoryTest(NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture)
    : NorthwindMiscellaneousQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>(fixture)
{
    public override Task Where_query_composition_entity_equality_one_element_Single(bool async)
        // Sequence contains no elements.
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Where_query_composition_entity_equality_one_element_Single(async));

    public override Task Where_query_composition_entity_equality_one_element_First(bool async)
        // Sequence contains no elements.
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Where_query_composition_entity_equality_one_element_First(async));

    public override Task Where_query_composition_entity_equality_no_elements_Single(bool async)
        // Sequence contains no elements.
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Where_query_composition_entity_equality_no_elements_Single(async));

    public override Task Where_query_composition_entity_equality_no_elements_First(bool async)
        // Sequence contains no elements.
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Where_query_composition_entity_equality_no_elements_First(async));

    public override Task Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(bool async)
        // Sequence contains more than one element.
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(async));

    public override Task Where_query_composition_entity_equality_multiple_elements_Single(bool async)
        // Sequence contains more than one element.
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Where_query_composition_entity_equality_multiple_elements_Single(async));

    public override Task Max_on_empty_sequence_throws(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Max_on_empty_sequence_throws(async));

    public override async Task Entity_equality_through_subquery_composite_key(bool async)
        => Assert.Equal(
            CoreStrings.EntityEqualityOnCompositeKeyEntitySubqueryNotSupported("==", nameof(OrderDetail)),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Entity_equality_through_subquery_composite_key(async))).Message);

    public override Task Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_constant_zero(bool async)
        => Task.CompletedTask;

    public override Task Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_constant_one(bool async)
        => Task.CompletedTask;

    public override Task Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_parameter(bool async)
        => Task.CompletedTask;
}
