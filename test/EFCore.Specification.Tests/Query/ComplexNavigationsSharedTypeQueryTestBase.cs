// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class ComplexNavigationsSharedTypeQueryTestBase<TFixture> : ComplexNavigationsQueryTestBase<TFixture>
    where TFixture : ComplexNavigationsSharedTypeQueryFixtureBase, new()
{
    protected ComplexNavigationsSharedTypeQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    public override Task Join_navigation_self_ref(bool async)
        => AssertTranslationFailed(() => base.Join_navigation_self_ref(async));

    public override Task Join_condition_optimizations_applied_correctly_when_anonymous_type_with_multiple_properties(bool async)
        => AssertUnableToTranslateEFProperty(
            () => base.Join_condition_optimizations_applied_correctly_when_anonymous_type_with_multiple_properties(async));

    public override Task Join_condition_optimizations_applied_correctly_when_anonymous_type_with_single_property(bool async)
        => AssertUnableToTranslateEFProperty(
            () => base.Join_condition_optimizations_applied_correctly_when_anonymous_type_with_single_property(async));

    public override Task Multiple_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_joined_together(bool async)
        => AssertTranslationFailed(
            () => base.Multiple_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_joined_together(async));

    public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany(
        bool async)
        => AssertTranslationFailed(
            () => base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany(async));

    public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany2(
        bool async)
        => AssertTranslationFailed(
            () => base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany2(async));

    public override Task SelectMany_with_nested_navigations_and_additional_joins_outside_of_SelectMany(bool async)
        => AssertTranslationFailed(() => base.SelectMany_with_nested_navigations_and_additional_joins_outside_of_SelectMany(async));

    public override Task Include8(bool async)
        => AssertIncludeOnNonEntity(() => base.Include8(async));

    public override Task Include9(bool async)
        => AssertIncludeOnNonEntity(() => base.Include9(async));

    public override Task Join_with_navigations_in_the_result_selector2(bool async)
        => AssertInvalidSetSharedType(() => base.Join_with_navigations_in_the_result_selector2(async), "Level2");

    public override Task Member_pushdown_chain_3_levels_deep(bool async)
        => AssertInvalidSetSharedType(() => base.Member_pushdown_chain_3_levels_deep(async), "Level2");

    public override Task Member_pushdown_chain_3_levels_deep_entity(bool async)
        => AssertInvalidSetSharedType(() => base.Member_pushdown_chain_3_levels_deep_entity(async), "Level2");

    public override Task Member_pushdown_with_collection_navigation_in_the_middle(bool async)
        => AssertInvalidSetSharedType(() => base.Member_pushdown_with_collection_navigation_in_the_middle(async), "Level2");

    public override Task Project_shadow_properties1(bool async)
        => AssertUnableToTranslateEFProperty(() => base.Project_shadow_properties1(async));

    public override Task Project_shadow_properties2(bool async)
        => AssertUnableToTranslateEFProperty(() => base.Project_shadow_properties2(async));

    public override Task Project_shadow_properties3(bool async)
        => AssertUnableToTranslateEFProperty(() => base.Project_shadow_properties3(async));

    public override Task Project_shadow_properties4(bool async)
        => AssertUnableToTranslateEFProperty(() => base.Project_shadow_properties4(async));

    public override Task Project_shadow_properties9(bool async)
        => AssertUnableToTranslateEFProperty(() => base.Project_shadow_properties9(async));

    public override async Task Null_check_removal_applied_recursively_complex(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Null_check_removal_applied_recursively_complex(async))).Message;

        Assert.Equal(CoreStrings.IncludeOnNonEntity("x => x.OneToMany_Required_Inverse3"), message);
    }
}
