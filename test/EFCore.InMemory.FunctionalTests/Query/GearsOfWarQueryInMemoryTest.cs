// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query;

public class GearsOfWarQueryInMemoryTest(GearsOfWarQueryInMemoryFixture fixture)
    : GearsOfWarQueryTestBase<GearsOfWarQueryInMemoryFixture>(fixture)
{
    public override Task Client_member_and_unsupported_string_Equals_in_the_same_query(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Client_member_and_unsupported_string_Equals_in_the_same_query(async),
            CoreStrings.QueryUnableToTranslateMember(nameof(Gear.IsMarcus), nameof(Gear)));

    public override async Task
        Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation_complex(bool async)
        => Assert.Equal(
            "Nullable object must have a value.",
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base
                    .Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation_complex(
                        async))).Message);

    public override async Task Group_by_on_StartsWith_with_null_parameter_as_argument(bool async)
        => Assert.Equal(
            "Value cannot be null. (Parameter 'value')",
            (await Assert.ThrowsAsync<ArgumentNullException>(
                () => base.Group_by_on_StartsWith_with_null_parameter_as_argument(async))).Message);

    public override async Task Group_by_with_having_StartsWith_with_null_parameter_as_argument(bool async)
        => Assert.Equal(
            "Value cannot be null. (Parameter 'value')",
            (await Assert.ThrowsAsync<ArgumentNullException>(
                () => base.Group_by_with_having_StartsWith_with_null_parameter_as_argument(async))).Message);

    public override async Task OrderBy_StartsWith_with_null_parameter_as_argument(bool async)
        => Assert.Equal(
            "Value cannot be null. (Parameter 'value')",
            (await Assert.ThrowsAsync<ArgumentNullException>(
                () => base.OrderBy_StartsWith_with_null_parameter_as_argument(async))).Message);

    public override async Task Select_StartsWith_with_null_parameter_as_argument(bool async)
        => Assert.Equal(
            "Value cannot be null. (Parameter 'value')",
            (await Assert.ThrowsAsync<ArgumentNullException>(
                () => base.Select_StartsWith_with_null_parameter_as_argument(async))).Message);

    public override async Task Projecting_entity_as_well_as_correlated_collection_followed_by_Distinct(bool async)
        // Distinct. Issue #24325.
        => Assert.Equal(
            InMemoryStrings.DistinctOnSubqueryNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Projecting_entity_as_well_as_correlated_collection_followed_by_Distinct(async))).Message);

    public override async Task Projecting_entity_as_well_as_complex_correlated_collection_followed_by_Distinct(bool async)
        // Distinct. Issue #24325.
        => Assert.Equal(
            InMemoryStrings.DistinctOnSubqueryNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Projecting_entity_as_well_as_complex_correlated_collection_followed_by_Distinct(async))).Message);

    public override async Task Projecting_entity_as_well_as_correlated_collection_of_scalars_followed_by_Distinct(bool async)
        // Distinct. Issue #24325.
        => Assert.Equal(
            InMemoryStrings.DistinctOnSubqueryNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Projecting_entity_as_well_as_correlated_collection_of_scalars_followed_by_Distinct(async))).Message);

    public override async Task Correlated_collection_with_distinct_3_levels(bool async)
        // Distinct. Issue #24325.
        => Assert.Equal(
            InMemoryStrings.DistinctOnSubqueryNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collection_with_distinct_3_levels(async))).Message);

    public override async Task Projecting_correlated_collection_followed_by_Distinct(bool async)
        // Distinct. Issue #24325.
        => Assert.Equal(
            InMemoryStrings.DistinctOnSubqueryNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Projecting_correlated_collection_followed_by_Distinct(async))).Message);

    public override async Task Projecting_some_properties_as_well_as_correlated_collection_followed_by_Distinct(bool async)
        // Distinct. Issue #24325.
        => Assert.Equal(
            InMemoryStrings.DistinctOnSubqueryNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Projecting_some_properties_as_well_as_correlated_collection_followed_by_Distinct(async))).Message);

    public override Task Include_after_SelectMany_throws(bool async)
        => Assert.ThrowsAsync<NullReferenceException>(() => base.Include_after_SelectMany_throws(async));

    public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result4(bool async)
        => await Assert.ThrowsAsync<TargetInvocationException>(
            () => base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result4(async));

    public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result(bool async)
        => await Assert.ThrowsAsync<TargetInvocationException>(
            () => base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result(async));

    public override Task Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation(
            bool async)
        // Null protection. Issue #13721.
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation(async));

    public override Task ElementAt_basic_with_OrderBy(bool async)
        => Task.CompletedTask;

    public override Task ElementAtOrDefault_basic_with_OrderBy(bool async)
        => Task.CompletedTask;

    public override Task ElementAtOrDefault_basic_with_OrderBy_parameter(bool async)
        => Task.CompletedTask;

    public override Task Where_subquery_with_ElementAtOrDefault_equality_to_null_with_composite_key(bool async)
        => Task.CompletedTask;

    public override Task Where_subquery_with_ElementAt_using_column_as_index(bool async)
        => Task.CompletedTask;

    public override Task Where_compare_anonymous_types(bool async)
        => Task.CompletedTask;

    public override Task Subquery_inside_Take_argument(bool async)
        => Task.CompletedTask;

    public override async Task Find_underlying_property_after_GroupJoin_DefaultIfEmpty(bool async)
        => Assert.Equal(
            "Nullable object must have a value.",
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base
                    .Find_underlying_property_after_GroupJoin_DefaultIfEmpty(
                        async))).Message);

    public override Task Join_include_coalesce_simple(bool async)
        => Task.CompletedTask;

    public override Task Join_include_coalesce_nested(bool async)
        => Task.CompletedTask;

    public override Task Join_include_conditional(bool async)
        => Task.CompletedTask;
}
