// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class
    ComplexNavigationsCollectionsSharedTypeQueryTestBase<TFixture> : ComplexNavigationsCollectionsQueryTestBase<TFixture>
    where TFixture : ComplexNavigationsSharedTypeQueryFixtureBase, new()
{
    protected ComplexNavigationsCollectionsSharedTypeQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    public override async Task Multiple_complex_includes_self_ref(bool async)
        => Assert.Equal(
            CoreStrings.InvalidIncludeExpression("e.OneToOne_Optional_Self1"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Multiple_complex_includes_self_ref(async))).Message);

    public override async Task Multiple_complex_includes_self_ref_EF_Property(bool async)
        => Assert.Equal(
            CoreStrings.InvalidIncludeExpression("Property(e, \"OneToOne_Optional_Self1\")"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Multiple_complex_includes_self_ref_EF_Property(async))).Message);

    public override Task
        Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(bool async)
        => AssertTranslationFailed(
            () => base
                .Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(
                    async));

    public override Task Include_collection_with_multiple_orderbys_complex(bool async)
        => AssertIncludeOnNonEntity(() => base.Include_collection_with_multiple_orderbys_complex(async));

    public override Task Include_collection_with_multiple_orderbys_complex_repeated(bool async)
        => AssertIncludeOnNonEntity(() => base.Include_collection_with_multiple_orderbys_complex_repeated(async));

    public override Task Include_collection_with_multiple_orderbys_complex_repeated_checked(bool async)
        => AssertIncludeOnNonEntity(() => base.Include_collection_with_multiple_orderbys_complex_repeated_checked(async));

    public override Task Include_collection_with_multiple_orderbys_member(bool async)
        => AssertIncludeOnNonEntity(() => base.Include_collection_with_multiple_orderbys_member(async));

    public override Task Include_collection_with_multiple_orderbys_methodcall(bool async)
        => AssertIncludeOnNonEntity(() => base.Include_collection_with_multiple_orderbys_methodcall(async));

    public override Task Include_collection_with_multiple_orderbys_property(bool async)
        => AssertIncludeOnNonEntity(() => base.Include_collection_with_multiple_orderbys_property(async));

    public override Task Include_inside_subquery(bool async)
        => AssertIncludeOnNonEntity(() => base.Include_inside_subquery(async));

    public override Task Filtered_include_outer_parameter_used_inside_filter(bool async)
        => AssertIncludeOnNonEntity(() => base.Filtered_include_outer_parameter_used_inside_filter(async));

    public override Task Include_after_multiple_SelectMany_and_reference_navigation(bool async)
        => AssertInvalidIncludeExpression(() => base.Include_after_multiple_SelectMany_and_reference_navigation(async));

    public override Task Include_after_SelectMany_and_multiple_reference_navigations(bool async)
        => AssertInvalidIncludeExpression(() => base.Include_after_SelectMany_and_multiple_reference_navigations(async));

    public override Task Required_navigation_with_Include(bool async)
        => AssertIncludeOnNonEntity(() => base.Required_navigation_with_Include(async));

    public override Task Required_navigation_with_Include_ThenInclude(bool async)
        => AssertIncludeOnNonEntity(() => base.Required_navigation_with_Include_ThenInclude(async));

    public override Task SelectMany_DefaultIfEmpty_multiple_times_with_joins_projecting_a_collection(bool async)
        => AssertTranslationFailed(() => base.SelectMany_DefaultIfEmpty_multiple_times_with_joins_projecting_a_collection(async));

    public override Task Complex_query_issue_21665(bool async)
        => AssertTranslationFailed(() => base.Complex_query_issue_21665(async));
}
