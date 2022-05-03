// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class TPCManyToManyNoTrackingQuerySqlServerTest : TPCManyToManyNoTrackingQueryRelationalTestBase<TPCManyToManyQuerySqlServerFixture>
{
    public TPCManyToManyNoTrackingQuerySqlServerTest(TPCManyToManyQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override bool CanExecuteQueryString
        => true;

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_all(bool async)
    {
        await base.Skip_navigation_all(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_any_without_predicate(bool async)
    {
        await base.Skip_navigation_any_without_predicate(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_any_with_predicate(bool async)
    {
        await base.Skip_navigation_any_with_predicate(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_contains(bool async)
    {
        await base.Skip_navigation_contains(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_count_without_predicate(bool async)
    {
        await base.Skip_navigation_count_without_predicate(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_count_with_predicate(bool async)
    {
        await base.Skip_navigation_count_with_predicate(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_long_count_without_predicate(bool async)
    {
        await base.Skip_navigation_long_count_without_predicate(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_long_count_with_predicate(bool async)
    {
        await base.Skip_navigation_long_count_with_predicate(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_select_many_average(bool async)
    {
        await base.Skip_navigation_select_many_average(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_select_many_max(bool async)
    {
        await base.Skip_navigation_select_many_max(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_select_many_min(bool async)
    {
        await base.Skip_navigation_select_many_min(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_select_many_sum(bool async)
    {
        await base.Skip_navigation_select_many_sum(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_select_subquery_average(bool async)
    {
        await base.Skip_navigation_select_subquery_average(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_select_subquery_max(bool async)
    {
        await base.Skip_navigation_select_subquery_max(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_select_subquery_min(bool async)
    {
        await base.Skip_navigation_select_subquery_min(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_select_subquery_sum(bool async)
    {
        await base.Skip_navigation_select_subquery_sum(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_order_by_first_or_default(bool async)
    {
        await base.Skip_navigation_order_by_first_or_default(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_order_by_single_or_default(bool async)
    {
        await base.Skip_navigation_order_by_single_or_default(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_order_by_last_or_default(bool async)
    {
        await base.Skip_navigation_order_by_last_or_default(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_order_by_reverse_first_or_default(bool async)
    {
        await base.Skip_navigation_order_by_reverse_first_or_default(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_cast(bool async)
    {
        await base.Skip_navigation_cast(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Skip_navigation_of_type(bool async)
    {
        await base.Skip_navigation_of_type(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Join_with_skip_navigation(bool async)
    {
        await base.Join_with_skip_navigation(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Left_join_with_skip_navigation(bool async)
    {
        await base.Left_join_with_skip_navigation(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Select_many_over_skip_navigation(bool async)
    {
        await base.Select_many_over_skip_navigation(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Select_many_over_skip_navigation_where(bool async)
    {
        await base.Select_many_over_skip_navigation_where(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Select_many_over_skip_navigation_order_by_skip(bool async)
    {
        await base.Select_many_over_skip_navigation_order_by_skip(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Select_many_over_skip_navigation_order_by_take(bool async)
    {
        await base.Select_many_over_skip_navigation_order_by_take(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Select_many_over_skip_navigation_order_by_skip_take(bool async)
    {
        await base.Select_many_over_skip_navigation_order_by_skip_take(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Select_many_over_skip_navigation_of_type(bool async)
    {
        await base.Select_many_over_skip_navigation_of_type(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Select_many_over_skip_navigation_cast(bool async)
    {
        await base.Select_many_over_skip_navigation_cast(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Select_skip_navigation(bool async)
    {
        await base.Select_skip_navigation(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Select_skip_navigation_multiple(bool async)
    {
        await base.Select_skip_navigation_multiple(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Select_skip_navigation_first_or_default(bool async)
    {
        await base.Select_skip_navigation_first_or_default(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_skip_navigation(bool async)
    {
        await base.Include_skip_navigation(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_skip_navigation_then_reference(bool async)
    {
        await base.Include_skip_navigation_then_reference(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_skip_navigation_then_include_skip_navigation(bool async)
    {
        await base.Include_skip_navigation_then_include_skip_navigation(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_skip_navigation_then_include_reference_and_skip_navigation(bool async)
    {
        await base.Include_skip_navigation_then_include_reference_and_skip_navigation(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_skip_navigation_and_reference(bool async)
    {
        await base.Include_skip_navigation_and_reference(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_include_skip_navigation_where(bool async)
    {
        await base.Filtered_include_skip_navigation_where(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_include_skip_navigation_order_by(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_include_skip_navigation_order_by_skip(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_include_skip_navigation_order_by_take(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_take(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_include_skip_navigation_order_by_skip_take(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_take(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_then_include_skip_navigation_where(bool async)
    {
        await base.Filtered_then_include_skip_navigation_where(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_then_include_skip_navigation_order_by_skip_take(bool async)
    {
        await base.Filtered_then_include_skip_navigation_order_by_skip_take(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_include_skip_navigation_where_then_include_skip_navigation(bool async)
    {
        await base.Filtered_include_skip_navigation_where_then_include_skip_navigation(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take(bool async)
    {
        await base.Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filter_include_on_skip_navigation_combined(bool async)
    {
        await base.Filter_include_on_skip_navigation_combined(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filter_include_on_skip_navigation_combined_with_filtered_then_includes(bool async)
    {
        await base.Filter_include_on_skip_navigation_combined_with_filtered_then_includes(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_include_on_skip_navigation_then_filtered_include_on_navigation(bool async)
    {
        await base.Filtered_include_on_skip_navigation_then_filtered_include_on_navigation(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_include_on_navigation_then_filtered_include_on_skip_navigation(bool async)
    {
        await base.Filtered_include_on_navigation_then_filtered_include_on_skip_navigation(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Includes_accessed_via_different_path_are_merged(bool async)
    {
        await base.Includes_accessed_via_different_path_are_merged(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filered_includes_accessed_via_different_path_are_merged(bool async)
    {
        await base.Filered_includes_accessed_via_different_path_are_merged(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_skip_navigation_split(bool async)
    {
        await base.Include_skip_navigation_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_skip_navigation_then_reference_split(bool async)
    {
        await base.Include_skip_navigation_then_reference_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_skip_navigation_then_include_skip_navigation_split(bool async)
    {
        await base.Include_skip_navigation_then_include_skip_navigation_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_skip_navigation_then_include_reference_and_skip_navigation_split(bool async)
    {
        await base.Include_skip_navigation_then_include_reference_and_skip_navigation_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_skip_navigation_and_reference_split(bool async)
    {
        await base.Include_skip_navigation_and_reference_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_include_skip_navigation_where_split(bool async)
    {
        await base.Filtered_include_skip_navigation_where_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_include_skip_navigation_order_by_split(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_include_skip_navigation_order_by_skip_split(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_include_skip_navigation_order_by_take_split(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_take_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_include_skip_navigation_order_by_skip_take_split(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_take_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_then_include_skip_navigation_where_split(bool async)
    {
        await base.Filtered_then_include_skip_navigation_where_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_then_include_skip_navigation_order_by_skip_take_split(bool async)
    {
        await base.Filtered_then_include_skip_navigation_order_by_skip_take_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_include_skip_navigation_where_then_include_skip_navigation_split(bool async)
    {
        await base.Filtered_include_skip_navigation_where_then_include_skip_navigation_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_split(bool async)
    {
        await base.Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take_split(bool async)
    {
        await base.Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filter_include_on_skip_navigation_combined_split(bool async)
    {
        await base.Filter_include_on_skip_navigation_combined_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filter_include_on_skip_navigation_combined_with_filtered_then_includes_split(bool async)
    {
        await base.Filter_include_on_skip_navigation_combined_with_filtered_then_includes_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_include_on_skip_navigation_then_filtered_include_on_navigation_split(bool async)
    {
        await base.Filtered_include_on_skip_navigation_then_filtered_include_on_navigation_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Filtered_include_on_navigation_then_filtered_include_on_skip_navigation_split(bool async)
    {
        await base.Filtered_include_on_navigation_then_filtered_include_on_skip_navigation_split(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_skip_navigation_then_include_inverse_throws_in_no_tracking(bool async)
    {
        await base.Include_skip_navigation_then_include_inverse_throws_in_no_tracking(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Include_skip_navigation_then_include_inverse_works_for_tracking_query(bool async)
    {
        await base.Include_skip_navigation_then_include_inverse_works_for_tracking_query(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Throws_when_different_filtered_include(bool async)
    {
        await base.Throws_when_different_filtered_include(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Throws_when_different_filtered_then_include(bool async)
    {
        await base.Throws_when_different_filtered_then_include(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Throws_when_different_filtered_then_include_via_different_paths(bool async)
    {
        await base.Throws_when_different_filtered_then_include_via_different_paths(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Select_many_over_skip_navigation_where_non_equality(bool async)
    {
        await base.Select_many_over_skip_navigation_where_non_equality(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Contains_on_skip_collection_navigation(bool async)
    {
        await base.Contains_on_skip_collection_navigation(async);

        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}

