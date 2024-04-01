// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class TPTGearsOfWarQuerySqliteTest : TPTGearsOfWarQueryRelationalTestBase<TPTGearsOfWarQuerySqliteFixture>
{
    public TPTGearsOfWarQuerySqliteTest(TPTGearsOfWarQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Where_datetimeoffset_date_component(bool async)
        => AssertTranslationFailed(() => base.Where_datetimeoffset_date_component(async));

    public override Task Where_datetimeoffset_day_component(bool async)
        => AssertTranslationFailed(() => base.Where_datetimeoffset_date_component(async));

    public override Task Where_datetimeoffset_dayofyear_component(bool async)
        => AssertTranslationFailed(() => base.Where_datetimeoffset_dayofyear_component(async));

    public override Task Where_datetimeoffset_hour_component(bool async)
        => AssertTranslationFailed(() => base.Where_datetimeoffset_hour_component(async));

    public override Task Where_datetimeoffset_millisecond_component(bool async)
        => AssertTranslationFailed(() => base.Where_datetimeoffset_millisecond_component(async));

    public override Task Where_datetimeoffset_minute_component(bool async)
        => AssertTranslationFailed(() => base.Where_datetimeoffset_minute_component(async));

    public override Task Where_datetimeoffset_month_component(bool async)
        => AssertTranslationFailed(() => base.Where_datetimeoffset_month_component(async));

    public override Task Where_datetimeoffset_now(bool async)
        => AssertTranslationFailed(() => base.Where_datetimeoffset_now(async));

    public override Task Where_datetimeoffset_second_component(bool async)
        => AssertTranslationFailed(() => base.Where_datetimeoffset_second_component(async));

    public override Task Where_datetimeoffset_utcnow(bool async)
        => AssertTranslationFailed(() => base.Where_datetimeoffset_utcnow(async));

    public override Task Where_datetimeoffset_year_component(bool async)
        => AssertTranslationFailed(() => base.Where_datetimeoffset_year_component(async));

    public override Task DateTimeOffset_Contains_Less_than_Greater_than(bool async)
        => AssertTranslationFailed(() => base.DateTimeOffset_Contains_Less_than_Greater_than(async));

    public override Task DateTimeOffsetNow_minus_timespan(bool async)
        => AssertTranslationFailed(() => base.DateTimeOffsetNow_minus_timespan(async));

    public override Task DateTimeOffset_Date_returns_datetime(bool async)
        => AssertTranslationFailed(() => base.DateTimeOffset_Date_returns_datetime(async));

    public override async Task Correlated_collections_inner_subquery_predicate_references_outer_qsre(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collections_inner_subquery_predicate_references_outer_qsre(async))).Message);

    public override async Task Correlated_collections_inner_subquery_selector_references_outer_qsre(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collections_inner_subquery_selector_references_outer_qsre(async))).Message);

    public override async Task Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(async))).Message);

    public override async Task Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(async))).Message);

    public override async Task Outer_parameter_in_group_join_with_DefaultIfEmpty(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Outer_parameter_in_group_join_with_DefaultIfEmpty(async))).Message);

    public override async Task Outer_parameter_in_join_key(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Outer_parameter_in_join_key(async))).Message);

    public override async Task Outer_parameter_in_join_key_inner_and_outer(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Outer_parameter_in_join_key_inner_and_outer(async))).Message);

    public override async Task Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion(async))).Message);

    public override async Task Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion_negated(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion_negated(async))).Message);

    public override async Task Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion(
        bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion(async)))
            .Message);

    public override async Task Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion_negated(
        bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base
                    .Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion_negated(async)))
            .Message);

    public override async Task SelectMany_predicate_with_non_equality_comparison_with_Take_doesnt_convert_to_join(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_predicate_with_non_equality_comparison_with_Take_doesnt_convert_to_join(async))).Message);

    public override async Task Correlated_collection_with_inner_collection_references_element_two_levels_up(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collection_with_inner_collection_references_element_two_levels_up(async))).Message);

    public override async Task
        Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection(
                    async))).Message);

    public override async Task
        Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection_multiple_grouping_keys(
            bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base
                    .Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection_multiple_grouping_keys(
                        async))).Message);

    public override async Task
        Correlated_collection_with_groupby_not_projecting_identifier_column_but_only_grouping_key_in_final_projection(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base
                    .Correlated_collection_with_groupby_not_projecting_identifier_column_but_only_grouping_key_in_final_projection(
                        async))).Message);

    public override async Task Correlated_collection_with_distinct_projecting_identifier_column(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collection_with_distinct_projecting_identifier_column(async))).Message);

    public override async Task Correlated_collection_with_distinct_not_projecting_identifier_column(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collection_with_distinct_not_projecting_identifier_column(async))).Message);

    public override async Task Correlated_collection_via_SelectMany_with_Distinct_missing_indentifying_columns_in_projection(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collection_via_SelectMany_with_Distinct_missing_indentifying_columns_in_projection(async)))
            .Message);

    public override async Task Correlated_collection_after_distinct_3_levels(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collection_after_distinct_3_levels(async))).Message);

    public override async Task Correlated_collections_with_Distinct(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collections_with_Distinct(async))).Message);

    public override async Task
        Correlated_collection_with_groupby_with_complex_grouping_key_not_projecting_identifier_column_with_group_aggregate_in_final_projection(
            bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base
                    .Correlated_collection_with_groupby_with_complex_grouping_key_not_projecting_identifier_column_with_group_aggregate_in_final_projection(
                        async))).Message);

    public override async Task Select_datetimeoffset_comparison_in_projection(bool async)
    {
        await base.Select_datetimeoffset_comparison_in_projection(async);

        AssertSql(
            """
SELECT "m"."Timeline"
FROM "Missions" AS "m"
""");
    }

    public override async Task Byte_array_contains_literal(bool async)
    {
        await base.Byte_array_contains_literal(async);

        AssertSql(
            """
SELECT "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s"."Name"
FROM "Squads" AS "s"
WHERE instr("s"."Banner", X'01') > 0
""");
    }

    public override async Task Byte_array_contains_parameter(bool async)
    {
        await base.Byte_array_contains_parameter(async);

        AssertSql(
            """
@__someByte_0='1'

SELECT "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s"."Name"
FROM "Squads" AS "s"
WHERE instr("s"."Banner", char(@__someByte_0)) > 0
""");
    }

    public override async Task Byte_array_filter_by_length_literal(bool async)
    {
        await base.Byte_array_filter_by_length_literal(async);

        AssertSql(
            """
SELECT "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s"."Name"
FROM "Squads" AS "s"
WHERE length("s"."Banner") = 2
""");
    }

    public override async Task Byte_array_filter_by_length_parameter(bool async)
    {
        await base.Byte_array_filter_by_length_parameter(async);

        AssertSql(
            """
@__p_0='2'

SELECT "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s"."Name"
FROM "Squads" AS "s"
WHERE length("s"."Banner") = @__p_0
""");
    }

    public override void Byte_array_filter_by_length_parameter_compiled()
    {
        base.Byte_array_filter_by_length_parameter_compiled();

        AssertSql(
            """
@__byteArrayParam='0x2A80' (Size = 2)

SELECT COUNT(*)
FROM "Squads" AS "s"
WHERE length("s"."Banner") = length(@__byteArrayParam)
""");
    }

    public override async Task Byte_array_filter_by_SequenceEqual(bool async)
    {
        await base.Byte_array_filter_by_SequenceEqual(async);

        AssertSql(
            """
@__byteArrayParam_0='0x0405060708' (Size = 5)

SELECT "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s"."Name"
FROM "Squads" AS "s"
WHERE "s"."Banner5" = @__byteArrayParam_0
""");
    }

    public override Task Where_TimeSpan_Hours(bool async)
        // TimeSpan. Issue #18844.
        => AssertTranslationFailed(() => base.Where_TimeSpan_Hours(async));

    public override Task Where_TimeSpan_Minutes(bool async)
        // TimeSpan. Issue #18844.
        => AssertTranslationFailed(() => base.Where_TimeSpan_Minutes(async));

    public override Task Where_TimeSpan_Seconds(bool async)
        // TimeSpan. Issue #18844.
        => AssertTranslationFailed(() => base.Where_TimeSpan_Seconds(async));

    public override Task Where_TimeSpan_Milliseconds(bool async)
        // TimeSpan. Issue #18844.
        => AssertTranslationFailed(() => base.Where_TimeSpan_Milliseconds(async));

    public override Task First_on_byte_array(bool async)
        // Array access. Issue #16428.
        => AssertTranslationFailed(() => base.First_on_byte_array(async));

    public override Task Array_access_on_byte_array(bool async)
        // Array access. Issue #16428.
        => AssertTranslationFailed(() => base.Array_access_on_byte_array(async));

    public override Task Where_TimeOnly_Hour(bool async)
        // TimeSpan. Issue #18844.
        => AssertTranslationFailed(() => base.Where_TimeOnly_Hour(async));

    public override Task Where_TimeOnly_Minute(bool async)
        // TimeSpan. Issue #18844.
        => AssertTranslationFailed(() => base.Where_TimeOnly_Minute(async));

    public override Task Where_TimeOnly_Second(bool async)
        // TimeSpan. Issue #18844.
        => AssertTranslationFailed(() => base.Where_TimeOnly_Second(async));

    public override Task Where_TimeOnly_Millisecond(bool async)
        // TimeSpan. Issue #18844.
        => AssertTranslationFailed(() => base.Where_TimeOnly_Millisecond(async));

    public override Task Where_TimeOnly_AddHours(bool async)
        // TimeSpan. Issue #18844.
        => AssertTranslationFailed(() => base.Where_TimeOnly_AddHours(async));

    public override Task Where_TimeOnly_AddMinutes(bool async)
        // TimeSpan. Issue #18844.
        => AssertTranslationFailed(() => base.Where_TimeOnly_AddMinutes(async));

    public override Task Where_TimeOnly_Add_TimeSpan(bool async)
        // TimeSpan. Issue #18844.
        => AssertTranslationFailed(() => base.Where_TimeOnly_Add_TimeSpan(async));

    public override Task Where_TimeOnly_IsBetween(bool async)
        // TimeSpan. Issue #18844.
        => AssertTranslationFailed(() => base.Where_TimeOnly_IsBetween(async));

    public override Task Where_TimeOnly_subtract_TimeOnly(bool async)
        // TimeSpan. Issue #18844.
        => AssertTranslationFailed(() => base.Where_TimeOnly_subtract_TimeOnly(async));

    public override async Task Where_subquery_with_ElementAt_using_column_as_index(bool async)
    {
        var message = (await Assert.ThrowsAsync<SqliteException>(
            () => base.Where_subquery_with_ElementAt_using_column_as_index(async))).Message;

        Assert.Equal("SQLite Error 1: 'no such column: s.Id'.", message);

        AssertSql(
            """
SELECT "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s"."Name"
FROM "Squads" AS "s"
WHERE (
    SELECT "g"."Nickname"
    FROM "Gears" AS "g"
    WHERE "s"."Id" = "g"."SquadId"
    ORDER BY "g"."Nickname"
    LIMIT 1 OFFSET "s"."Id") = 'Cole Train'
""");
    }

    public override Task DateTimeOffset_to_unix_time_milliseconds(bool async)
        => AssertTranslationFailed(() => base.DateTimeOffset_to_unix_time_milliseconds(async));

    public override Task DateTimeOffset_to_unix_time_seconds(bool async)
        => AssertTranslationFailed(() => base.DateTimeOffset_to_unix_time_seconds(async));

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
