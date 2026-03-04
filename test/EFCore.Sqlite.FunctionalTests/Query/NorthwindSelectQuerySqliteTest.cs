// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindSelectQuerySqliteTest : NorthwindSelectQueryRelationalTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
{
    public NorthwindSelectQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Select_datetime_year_component(bool async)
    {
        await base.Select_datetime_year_component(async);

        AssertSql(
            """
SELECT CAST(strftime('%Y', "o"."OrderDate") AS INTEGER)
FROM "Orders" AS "o"
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Select_datetime_year_component_composed(bool async)
    {
        await AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderDate.Value.AddYears(1).Year));

        AssertSql(
            """
SELECT CAST(strftime('%Y', "o"."OrderDate", CAST(1 AS TEXT) || ' years') AS INTEGER)
FROM "Orders" AS "o"
""");
    }

    public override async Task Select_datetime_month_component(bool async)
    {
        await base.Select_datetime_month_component(async);

        AssertSql(
            """
SELECT CAST(strftime('%m', "o"."OrderDate") AS INTEGER)
FROM "Orders" AS "o"
""");
    }

    public override async Task Select_datetime_day_of_year_component(bool async)
    {
        await base.Select_datetime_day_of_year_component(async);

        AssertSql(
            """
SELECT CAST(strftime('%j', "o"."OrderDate") AS INTEGER)
FROM "Orders" AS "o"
""");
    }

    public override async Task Select_datetime_day_component(bool async)
    {
        await base.Select_datetime_day_component(async);

        AssertSql(
            """
SELECT CAST(strftime('%d', "o"."OrderDate") AS INTEGER)
FROM "Orders" AS "o"
""");
    }

    public override async Task Select_datetime_hour_component(bool async)
    {
        await base.Select_datetime_hour_component(async);

        AssertSql(
            """
SELECT CAST(strftime('%H', "o"."OrderDate") AS INTEGER)
FROM "Orders" AS "o"
""");
    }

    public override async Task Select_datetime_minute_component(bool async)
    {
        await base.Select_datetime_minute_component(async);

        AssertSql(
            """
SELECT CAST(strftime('%M', "o"."OrderDate") AS INTEGER)
FROM "Orders" AS "o"
""");
    }

    public override async Task Select_datetime_second_component(bool async)
    {
        await base.Select_datetime_second_component(async);

        AssertSql(
            """
SELECT CAST(strftime('%S', "o"."OrderDate") AS INTEGER)
FROM "Orders" AS "o"
""");
    }

    public override async Task Select_datetime_millisecond_component(bool async)
    {
        await base.Select_datetime_millisecond_component(async);

        AssertSql(
            """
SELECT (CAST(strftime('%f', "o"."OrderDate") AS REAL) * 1000.0) % 1000.0
FROM "Orders" AS "o"
""");
    }

    public override async Task Select_datetime_DayOfWeek_component(bool async)
    {
        await base.Select_datetime_DayOfWeek_component(async);

        AssertSql(
            """
SELECT CAST(CAST(strftime('%w', "o"."OrderDate") AS INTEGER) AS INTEGER)
FROM "Orders" AS "o"
""");
    }

    public override async Task Select_datetime_Ticks_component(bool async)
    {
        await base.Select_datetime_Ticks_component(async);

        AssertSql(
            """
SELECT CAST((julianday("o"."OrderDate") - 1721425.5) * 864000000000.0 AS INTEGER)
FROM "Orders" AS "o"
""");
    }

    public override async Task Select_datetime_TimeOfDay_component(bool async)
    {
        await base.Select_datetime_TimeOfDay_component(async);

        AssertSql(
            """
SELECT rtrim(rtrim(strftime('%H:%M:%f', "o"."OrderDate"), '0'), '.')
FROM "Orders" AS "o"
""");
    }

    public override async Task
        SelectMany_with_collection_being_correlated_subquery_which_references_non_mapped_properties_from_inner_and_outer_entity(
            bool async)
    {
        await AssertUnableToTranslateEFProperty(
            () => base
                .SelectMany_with_collection_being_correlated_subquery_which_references_non_mapped_properties_from_inner_and_outer_entity(
                    async));

        AssertSql();
    }

    public override async Task SelectMany_correlated_with_outer_1(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_correlated_with_outer_1(async))).Message);

    public override async Task SelectMany_correlated_with_outer_2(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_correlated_with_outer_2(async))).Message);

    public override async Task SelectMany_correlated_with_outer_3(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_correlated_with_outer_3(async))).Message);

    public override async Task SelectMany_correlated_with_outer_4(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_correlated_with_outer_4(async))).Message);

    public override async Task SelectMany_correlated_with_outer_5(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_correlated_with_outer_5(async))).Message);

    public override async Task SelectMany_correlated_with_outer_6(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_correlated_with_outer_6(async))).Message);

    public override async Task SelectMany_correlated_with_outer_7(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_correlated_with_outer_7(async))).Message);

    public override async Task SelectMany_whose_selector_references_outer_source(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_whose_selector_references_outer_source(async))).Message);

    public override async Task Projecting_after_navigation_and_distinct(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Projecting_after_navigation_and_distinct(async))).Message);

    public override async Task Select_nested_collection_deep(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Select_nested_collection_deep(async))).Message);

    public override async Task Correlated_collection_after_groupby_with_complex_projection_containing_original_identifier(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collection_after_groupby_with_complex_projection_containing_original_identifier(async))).Message);

    public override async Task Correlated_collection_after_distinct_not_containing_original_identifier(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collection_after_distinct_not_containing_original_identifier(async))).Message);

    public override async Task Correlated_collection_after_distinct_with_complex_projection_not_containing_original_identifier(bool async)
        => Assert.Equal(
            RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collection_after_distinct_with_complex_projection_not_containing_original_identifier(async)))
            .Message);

    public override async Task Correlated_collection_after_distinct_with_complex_projection_containing_original_identifier(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collection_after_distinct_with_complex_projection_containing_original_identifier(async)))
            .Message);

    public override async Task Select_nested_collection_deep_distinct_no_identifiers(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Select_nested_collection_deep_distinct_no_identifiers(async))).Message);

    public override async Task Reverse_in_projection_subquery(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Reverse_in_projection_subquery(async))).Message);

    public override async Task Reverse_in_projection_subquery_single_result(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Reverse_in_projection_subquery_single_result(async))).Message);

    public override async Task Reverse_in_SelectMany_with_Take(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Reverse_in_SelectMany_with_Take(async))).Message);

    public override async Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(async))).Message);

    public override Task Member_binding_after_ctor_arguments_fails_with_client_eval(bool async)
        => AssertTranslationFailed(() => base.Member_binding_after_ctor_arguments_fails_with_client_eval(async));

    public override async Task SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity(async))).Message);

    public override async Task Collection_projection_selecting_outer_element_followed_by_take(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Collection_projection_selecting_outer_element_followed_by_take(async))).Message);

    public override async Task Take_on_top_level_and_on_collection_projection_with_outer_apply(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Take_on_top_level_and_on_collection_projection_with_outer_apply(async))).Message);

    public override async Task Take_on_correlated_collection_in_first(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Take_on_correlated_collection_in_first(async))).Message);

    public override async Task Correlated_collection_after_groupby_with_complex_projection_not_containing_original_identifier(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collection_after_groupby_with_complex_projection_not_containing_original_identifier(async))).Message);

    public override async Task Set_operation_in_pending_collection(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Set_operation_in_pending_collection(async))).Message);

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
