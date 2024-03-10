// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class GearsOfWarQuerySqliteTest : GearsOfWarQueryRelationalTestBase<GearsOfWarQuerySqliteFixture>
{
    public GearsOfWarQuerySqliteTest(GearsOfWarQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Where_datetimeoffset_date_component(bool async)
    {
        await AssertTranslationFailed(() => base.Where_datetimeoffset_date_component(async));

        AssertSql();
    }

    public override async Task Where_datetimeoffset_day_component(bool async)
    {
        await AssertTranslationFailed(() => base.Where_datetimeoffset_date_component(async));

        AssertSql();
    }

    public override async Task Where_datetimeoffset_dayofyear_component(bool async)
    {
        await AssertTranslationFailed(() => base.Where_datetimeoffset_dayofyear_component(async));

        AssertSql();
    }

    public override async Task Where_datetimeoffset_hour_component(bool async)
    {
        await AssertTranslationFailed(() => base.Where_datetimeoffset_hour_component(async));

        AssertSql();
    }

    public override async Task Where_datetimeoffset_millisecond_component(bool async)
    {
        await AssertTranslationFailed(() => base.Where_datetimeoffset_millisecond_component(async));

        AssertSql();
    }

    public override async Task Where_datetimeoffset_minute_component(bool async)
    {
        await AssertTranslationFailed(() => base.Where_datetimeoffset_minute_component(async));

        AssertSql();
    }

    public override async Task Where_datetimeoffset_month_component(bool async)
    {
        await AssertTranslationFailed(() => base.Where_datetimeoffset_month_component(async));

        AssertSql();
    }

    public override async Task Where_datetimeoffset_now(bool async)
    {
        await AssertTranslationFailed(() => base.Where_datetimeoffset_now(async));

        AssertSql();
    }

    public override async Task Where_datetimeoffset_second_component(bool async)
    {
        await AssertTranslationFailed(() => base.Where_datetimeoffset_second_component(async));

        AssertSql();
    }

    public override async Task Where_datetimeoffset_utcnow(bool async)
    {
        await AssertTranslationFailed(() => base.Where_datetimeoffset_utcnow(async));

        AssertSql();
    }

    public override async Task Where_datetimeoffset_year_component(bool async)
    {
        await AssertTranslationFailed(() => base.Where_datetimeoffset_year_component(async));

        AssertSql();
    }

    public override async Task DateTimeOffset_Contains_Less_than_Greater_than(bool async)
    {
        await AssertTranslationFailed(() => base.DateTimeOffset_Contains_Less_than_Greater_than(async));

        AssertSql();
    }

    public override Task DateTimeOffsetNow_minus_timespan(bool async)
        => AssertTranslationFailed(() => base.DateTimeOffsetNow_minus_timespan(async));

    public override async Task DateTimeOffset_Date_returns_datetime(bool async)
    {
        await AssertTranslationFailed(() => base.DateTimeOffset_Date_returns_datetime(async));

        AssertSql();
    }

    public override async Task Correlated_collections_inner_subquery_predicate_references_outer_qsre(bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collections_inner_subquery_predicate_references_outer_qsre(async))).Message);

        AssertSql();
    }

    public override async Task Correlated_collections_inner_subquery_selector_references_outer_qsre(bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collections_inner_subquery_selector_references_outer_qsre(async))).Message);

        AssertSql();
    }

    public override async Task Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(async))).Message);

        AssertSql();
    }

    public override async Task Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(async))).Message);

        AssertSql();
    }

    public override async Task Outer_parameter_in_group_join_with_DefaultIfEmpty(bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Outer_parameter_in_group_join_with_DefaultIfEmpty(async))).Message);

        AssertSql();
    }

    public override async Task Outer_parameter_in_join_key(bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Outer_parameter_in_join_key(async))).Message);

        AssertSql();
    }

    public override async Task Outer_parameter_in_join_key_inner_and_outer(bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Outer_parameter_in_join_key_inner_and_outer(async))).Message);

        AssertSql();
    }

    public override async Task Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion(bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion(async))).Message);

        AssertSql();
    }

    public override async Task Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion_negated(bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Subquery_projecting_nullable_scalar_contains_nullable_value_needs_null_expansion_negated(async))).Message);

        AssertSql();
    }

    public override async Task Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion(
        bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion(async)))
            .Message);

        AssertSql();
    }

    public override async Task Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion_negated(
        bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base
                    .Subquery_projecting_non_nullable_scalar_contains_non_nullable_value_doesnt_need_null_expansion_negated(async)))
            .Message);

        AssertSql();
    }

    public override async Task SelectMany_predicate_with_non_equality_comparison_with_Take_doesnt_convert_to_join(bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_predicate_with_non_equality_comparison_with_Take_doesnt_convert_to_join(async))).Message);

        AssertSql();
    }

    public override async Task Correlated_collection_with_inner_collection_references_element_two_levels_up(bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collection_with_inner_collection_references_element_two_levels_up(async))).Message);

        AssertSql();
    }

    public override async Task
        Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection(bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection(
                    async))).Message);

        AssertSql();
    }

    public override async Task
        Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection_multiple_grouping_keys(
            bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base
                    .Correlated_collection_with_groupby_not_projecting_identifier_column_with_group_aggregate_in_final_projection_multiple_grouping_keys(
                        async))).Message);

        AssertSql();
    }

    public override async Task
        Correlated_collection_with_groupby_not_projecting_identifier_column_but_only_grouping_key_in_final_projection(bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base
                    .Correlated_collection_with_groupby_not_projecting_identifier_column_but_only_grouping_key_in_final_projection(
                        async))).Message);

        AssertSql();
    }

    public override async Task Correlated_collection_with_distinct_projecting_identifier_column(bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collection_with_distinct_projecting_identifier_column(async))).Message);

        AssertSql();
    }

    public override async Task Correlated_collection_with_distinct_not_projecting_identifier_column(bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collection_with_distinct_not_projecting_identifier_column(async))).Message);

        AssertSql();
    }

    public override async Task Correlated_collection_via_SelectMany_with_Distinct_missing_indentifying_columns_in_projection(bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collection_via_SelectMany_with_Distinct_missing_indentifying_columns_in_projection(async)))
            .Message);

        AssertSql();
    }

    public override async Task Correlated_collection_after_distinct_3_levels_without_original_identifiers(bool async)
    {
        await base.Correlated_collection_after_distinct_3_levels_without_original_identifiers(async);

        AssertSql();
    }

    public override async Task Correlated_collection_after_distinct_3_levels(bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collection_after_distinct_3_levels(async))).Message);

        AssertSql();
    }

    public override async Task Correlated_collections_with_Distinct(bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collections_with_Distinct(async))).Message);

        AssertSql();
    }

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

    public override async Task Where_TimeSpan_Hours(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Where_TimeSpan_Hours(async));

        AssertSql();
    }

    public override async Task Where_TimeSpan_Minutes(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Where_TimeSpan_Minutes(async));

        AssertSql();
    }

    public override async Task Where_TimeSpan_Seconds(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Where_TimeSpan_Seconds(async));

        AssertSql();
    }

    public override async Task Where_TimeSpan_Milliseconds(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Where_TimeSpan_Milliseconds(async));

        AssertSql();
    }

    public override async Task First_on_byte_array(bool async)
    {
        // Array access. Issue #16428.
        await AssertTranslationFailed(() => base.First_on_byte_array(async));

        AssertSql();
    }

    public override async Task Array_access_on_byte_array(bool async)
    {
        // Array access. Issue #16428.
        await AssertTranslationFailed(() => base.Array_access_on_byte_array(async));

        AssertSql();
    }

    public override async Task Where_DateOnly_Year(bool async)
    {
        await base.Where_DateOnly_Year(async);

        AssertSql(
            """
SELECT "m"."Id", "m"."CodeName", "m"."Date", "m"."Duration", "m"."Rating", "m"."Time", "m"."Timeline"
FROM "Missions" AS "m"
WHERE CAST(strftime('%Y', "m"."Date") AS INTEGER) = 1990
""");
    }

    public override async Task Where_DateOnly_Month(bool async)
    {
        await base.Where_DateOnly_Month(async);

        AssertSql(
            """
SELECT "m"."Id", "m"."CodeName", "m"."Date", "m"."Duration", "m"."Rating", "m"."Time", "m"."Timeline"
FROM "Missions" AS "m"
WHERE CAST(strftime('%m', "m"."Date") AS INTEGER) = 11
""");
    }

    public override async Task Where_DateOnly_Day(bool async)
    {
        await base.Where_DateOnly_Day(async);

        AssertSql(
            """
SELECT "m"."Id", "m"."CodeName", "m"."Date", "m"."Duration", "m"."Rating", "m"."Time", "m"."Timeline"
FROM "Missions" AS "m"
WHERE CAST(strftime('%d', "m"."Date") AS INTEGER) = 10
""");
    }

    public override async Task Where_DateOnly_DayOfYear(bool async)
    {
        await base.Where_DateOnly_DayOfYear(async);

        AssertSql(
            """
SELECT "m"."Id", "m"."CodeName", "m"."Date", "m"."Duration", "m"."Rating", "m"."Time", "m"."Timeline"
FROM "Missions" AS "m"
WHERE CAST(strftime('%j', "m"."Date") AS INTEGER) = 314
""");
    }

    public override async Task Where_DateOnly_DayOfWeek(bool async)
    {
        await base.Where_DateOnly_DayOfWeek(async);

        AssertSql(
            """
SELECT "m"."Id", "m"."CodeName", "m"."Date", "m"."Duration", "m"."Rating", "m"."Time", "m"."Timeline"
FROM "Missions" AS "m"
WHERE CAST(strftime('%w', "m"."Date") AS INTEGER) = 6
""");
    }

    public override async Task Where_DateOnly_AddYears(bool async)
    {
        await base.Where_DateOnly_AddYears(async);

        AssertSql(
            """
SELECT "m"."Id", "m"."CodeName", "m"."Date", "m"."Duration", "m"."Rating", "m"."Time", "m"."Timeline"
FROM "Missions" AS "m"
WHERE date("m"."Date", CAST(3 AS TEXT) || ' years') = '1993-11-10'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_DateOnly_AddYears_Year(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Date.AddYears(3).Year == 1993).AsTracking());

        AssertSql(
            """
SELECT "m"."Id", "m"."CodeName", "m"."Date", "m"."Duration", "m"."Rating", "m"."Time", "m"."Timeline"
FROM "Missions" AS "m"
WHERE CAST(strftime('%Y', "m"."Date", CAST(3 AS TEXT) || ' years') AS INTEGER) = 1993
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_DateOnly_AddYears_AddMonths(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Mission>().Where(m => m.Date.AddYears(3).AddMonths(3) == new DateOnly(1994, 2, 10)).AsTracking());

        AssertSql(
            """
SELECT "m"."Id", "m"."CodeName", "m"."Date", "m"."Duration", "m"."Rating", "m"."Time", "m"."Timeline"
FROM "Missions" AS "m"
WHERE date("m"."Date", CAST(3 AS TEXT) || ' years', CAST(3 AS TEXT) || ' months') = '1994-02-10'
""");
    }

    public override async Task Where_DateOnly_AddMonths(bool async)
    {
        await base.Where_DateOnly_AddMonths(async);

        AssertSql(
            """
SELECT "m"."Id", "m"."CodeName", "m"."Date", "m"."Duration", "m"."Rating", "m"."Time", "m"."Timeline"
FROM "Missions" AS "m"
WHERE date("m"."Date", CAST(3 AS TEXT) || ' months') = '1991-02-10'
""");
    }

    public override async Task Where_DateOnly_AddDays(bool async)
    {
        await base.Where_DateOnly_AddDays(async);

        AssertSql(
            """
SELECT "m"."Id", "m"."CodeName", "m"."Date", "m"."Duration", "m"."Rating", "m"."Time", "m"."Timeline"
FROM "Missions" AS "m"
WHERE date("m"."Date", CAST(3 AS TEXT) || ' days') = '1990-11-13'
""");
    }

    public override async Task Where_TimeOnly_Hour(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Where_TimeOnly_Hour(async));

        AssertSql();
    }

    public override async Task Where_TimeOnly_Minute(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Where_TimeOnly_Minute(async));

        AssertSql();
    }

    public override async Task Where_TimeOnly_Second(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Where_TimeOnly_Second(async));

        AssertSql();
    }

    public override async Task Where_TimeOnly_Millisecond(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Where_TimeOnly_Millisecond(async));

        AssertSql();
    }

    public override async Task Where_TimeOnly_AddHours(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Where_TimeOnly_AddHours(async));

        AssertSql();
    }

    public override async Task Where_TimeOnly_AddMinutes(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Where_TimeOnly_AddMinutes(async));

        AssertSql();
    }

    public override async Task Where_TimeOnly_Add_TimeSpan(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Where_TimeOnly_Add_TimeSpan(async));

        AssertSql();
    }

    public override async Task Where_TimeOnly_IsBetween(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Where_TimeOnly_IsBetween(async));

        AssertSql();
    }

    public override async Task Where_TimeOnly_subtract_TimeOnly(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Where_TimeOnly_subtract_TimeOnly(async));

        AssertSql();
    }

    public override async Task Optional_navigation_type_compensation_works_with_DTOs(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_DTOs(async);

        AssertSql(
            """
SELECT "g"."SquadId" AS "Id"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE "t"."Note" <> 'K.I.A.' OR "t"."Note" IS NULL
""");
    }

    public override async Task Include_with_join_and_inheritance_with_orderby_before_and_after_include(bool async)
    {
        await base.Include_with_join_and_inheritance_with_orderby_before_and_after_include(async);

        AssertSql(
            """
SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "t"."Id", "g1"."Nickname", "g1"."SquadId", "g1"."AssignedCityName", "g1"."CityOfBirthName", "g1"."Discriminator", "g1"."FullName", "g1"."HasSoulPatch", "g1"."LeaderNickname", "g1"."LeaderSquadId", "g1"."Rank"
FROM "Tags" AS "t"
INNER JOIN (
    SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
    FROM "Gears" AS "g"
    WHERE "g"."Discriminator" = 'Officer'
) AS "g0" ON "t"."GearSquadId" = "g0"."SquadId" AND "t"."GearNickName" = "g0"."Nickname"
LEFT JOIN "Gears" AS "g1" ON "g0"."Nickname" = "g1"."LeaderNickname" AND "g0"."SquadId" = "g1"."LeaderSquadId"
ORDER BY "g0"."HasSoulPatch", "g0"."Nickname" DESC, "t"."Id", "g0"."SquadId", "g1"."Nickname"
""");
    }

    public override async Task DateTimeOffset_DateAdd_AddMonths(bool async)
    {
        await base.DateTimeOffset_DateAdd_AddMonths(async);

        AssertSql(
            """
SELECT "m"."Timeline"
FROM "Missions" AS "m"
""");
    }

    public override async Task Logical_operation_with_non_null_parameter_optimizes_null_checks(bool async)
    {
        await base.Logical_operation_with_non_null_parameter_optimizes_null_checks(async);

        AssertSql(
            """
@__prm_0='True'

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch" <> @__prm_0
""",
            //
            """
@__prm_0='False'

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch" <> @__prm_0
""");
    }

    public override async Task Where_contains_on_navigation_with_composite_keys(bool async)
    {
        await base.Where_contains_on_navigation_with_composite_keys(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE EXISTS (
    SELECT 1
    FROM "Cities" AS "c"
    WHERE EXISTS (
        SELECT 1
        FROM "Gears" AS "g0"
        WHERE "c"."Name" = "g0"."CityOfBirthName" AND "g0"."Nickname" = "g"."Nickname" AND "g0"."SquadId" = "g"."SquadId"))
""");
    }

    public override async Task Correlated_collections_naked_navigation_with_ToArray(bool async)
    {
        await base.Correlated_collections_naked_navigation_with_ToArray(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
WHERE "g"."Nickname" <> 'Marcus'
ORDER BY "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task GetValueOrDefault_with_argument_complex(bool async)
    {
        await base.GetValueOrDefault_with_argument_complex(async);

        AssertSql(
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE COALESCE("w"."SynergyWithId", length("w"."Name") + 42) > 10
""");
    }

    public override async Task Include_where_list_contains_navigation2(bool async)
    {
        await base.Include_where_list_contains_navigation2(async);

        AssertSql(
            """
SELECT "t"."Id"
FROM "Tags" AS "t"
""",
            //
            """
@__tags_0='["34C8D86E-A4AC-4BE5-827F-584DDA348A07","70534E05-782C-4052-8720-C2C54481CE5F","A7BE028A-0CF2-448F-AB55-CE8BC5D8CF69","A8AD98F9-E023-4E2A-9A70-C2728455BD34","B39A6FBA-9026-4D69-828E-FD7068673E57","DF36F493-463F-4123-83F9-6B135DEEB7BA"]' (Size = 235)

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Gears" AS "g"
INNER JOIN "Cities" AS "c" ON "g"."CityOfBirthName" = "c"."Name"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
WHERE "c"."Location" IS NOT NULL AND "t"."Id" IN (
    SELECT "t0"."value"
    FROM json_each(@__tags_0) AS "t0"
)
""");
    }

    public override async Task Projecting_nullable_bool_in_conditional_works(bool async)
    {
        await base.Projecting_nullable_bool_in_conditional_works(async);

        AssertSql(
            """
SELECT CASE
    WHEN "g"."Nickname" IS NOT NULL AND "g"."SquadId" IS NOT NULL THEN "g"."HasSoulPatch"
    ELSE 0
END AS "Prop"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
""");
    }

    public override async Task Include_on_derived_entity_using_OfType(bool async)
    {
        await base.Include_on_derived_entity_using_OfType(async);

        AssertSql(
            """
SELECT "f"."Id", "f"."CapitalName", "f"."Discriminator", "f"."Name", "f"."ServerAddress", "f"."CommanderName", "f"."Eradicated", "l0"."Name", "l0"."Discriminator", "l0"."LocustHordeId", "l0"."ThreatLevel", "l0"."ThreatLevelByte", "l0"."ThreatLevelNullableByte", "l0"."DefeatedByNickname", "l0"."DefeatedBySquadId", "l0"."HighCommandId", "l1"."Name", "l1"."Discriminator", "l1"."LocustHordeId", "l1"."ThreatLevel", "l1"."ThreatLevelByte", "l1"."ThreatLevelNullableByte", "l1"."DefeatedByNickname", "l1"."DefeatedBySquadId", "l1"."HighCommandId"
FROM "Factions" AS "f"
LEFT JOIN (
    SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId"
    FROM "LocustLeaders" AS "l"
    WHERE "l"."Discriminator" = 'LocustCommander'
) AS "l0" ON "f"."CommanderName" = "l0"."Name"
LEFT JOIN "LocustLeaders" AS "l1" ON "f"."Id" = "l1"."LocustHordeId"
ORDER BY "f"."Name", "f"."Id", "l0"."Name"
""");
    }

    public override async Task Correlated_collections_basic_projection_explicit_to_array(bool async)
    {
        await base.Correlated_collections_basic_projection_explicit_to_array(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
    FROM "Weapons" AS "w"
    WHERE "w"."IsAutomatic" OR "w"."Name" <> 'foo' OR "w"."Name" IS NULL
) AS "w0" ON "g"."FullName" = "w0"."OwnerFullName"
WHERE "g"."Nickname" <> 'Marcus'
ORDER BY "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task Correlated_collections_deeply_nested_left_join(bool async)
    {
        await base.Correlated_collections_deeply_nested_left_join(async);

        AssertSql(
            """
SELECT "t"."Id", "g"."Nickname", "g"."SquadId", "s"."Id", "s0"."Nickname", "s0"."SquadId", "s0"."Id", "s0"."AmmunitionType", "s0"."IsAutomatic", "s0"."Name", "s0"."OwnerFullName", "s0"."SynergyWithId"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname"
LEFT JOIN "Squads" AS "s" ON "g"."SquadId" = "s"."Id"
LEFT JOIN (
    SELECT "g0"."Nickname", "g0"."SquadId", "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
    FROM "Gears" AS "g0"
    LEFT JOIN (
        SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
        FROM "Weapons" AS "w"
        WHERE "w"."IsAutomatic"
    ) AS "w0" ON "g0"."FullName" = "w0"."OwnerFullName"
    WHERE "g0"."HasSoulPatch"
) AS "s0" ON "s"."Id" = "s0"."SquadId"
ORDER BY "t"."Note", "g"."Nickname" DESC, "t"."Id", "g"."SquadId", "s"."Id", "s0"."Nickname", "s0"."SquadId"
""");
    }

    public override async Task Collection_navigation_ofType_filter_works(bool async)
    {
        await base.Collection_navigation_ofType_filter_works(async);

        AssertSql(
            """
SELECT "c"."Name", "c"."Location", "c"."Nation"
FROM "Cities" AS "c"
WHERE EXISTS (
    SELECT 1
    FROM "Gears" AS "g"
    WHERE "c"."Name" = "g"."CityOfBirthName" AND "g"."Discriminator" = 'Officer' AND "g"."Nickname" = 'Marcus')
""");
    }

    public override async Task DateTimeOffset_DateAdd_AddMilliseconds(bool async)
    {
        await base.DateTimeOffset_DateAdd_AddMilliseconds(async);

        AssertSql(
            """
SELECT "m"."Timeline"
FROM "Missions" AS "m"
""");
    }

    public override async Task Project_discriminator_columns(bool async)
    {
        await base.Project_discriminator_columns(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."Discriminator"
FROM "Gears" AS "g"
""",
            //
            """
SELECT "g"."Nickname", "g"."Discriminator"
FROM "Gears" AS "g"
WHERE "g"."Discriminator" = 'Officer'
""",
            //
            """
SELECT "f"."Id", "f"."Discriminator"
FROM "Factions" AS "f"
""",
            //
            """
SELECT "f"."Id", "f"."Discriminator"
FROM "Factions" AS "f"
""",
            //
            """
SELECT "l"."Name", "l"."Discriminator"
FROM "LocustLeaders" AS "l"
""",
            //
            """
SELECT "l"."Name", "l"."Discriminator"
FROM "LocustLeaders" AS "l"
WHERE "l"."Discriminator" = 'LocustCommander'
""");
    }

    public override async Task Nullable_bool_comparison_is_translated_to_server(bool async)
    {
        await base.Nullable_bool_comparison_is_translated_to_server(async);

        AssertSql(
            """
SELECT "f"."Eradicated" = 1 AND "f"."Eradicated" IS NOT NULL AS "IsEradicated"
FROM "Factions" AS "f"
""");
    }

    public override async Task Null_propagation_optimization6(bool async)
    {
        await base.Null_propagation_optimization6(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE CASE
    WHEN "g"."LeaderNickname" IS NOT NULL THEN length("g"."LeaderNickname")
    ELSE NULL
END = 5 AND CASE
    WHEN "g"."LeaderNickname" IS NOT NULL THEN length("g"."LeaderNickname")
    ELSE NULL
END IS NOT NULL
""");
    }

    public override async Task GroupBy_Property_Include_Select_Min(bool async)
    {
        await base.GroupBy_Property_Include_Select_Min(async);

        AssertSql(
            """
SELECT MIN("g"."SquadId")
FROM "Gears" AS "g"
GROUP BY "g"."Rank"
""");
    }

    public override async Task Join_with_order_by_on_inner_sequence_navigation_translated_to_subquery_composite_key(bool async)
    {
        await base.Join_with_order_by_on_inner_sequence_navigation_translated_to_subquery_composite_key(async);

        AssertSql(
            """
SELECT "g"."FullName", "s"."Note"
FROM "Gears" AS "g"
INNER JOIN (
    SELECT "t"."Note", "g0"."FullName"
    FROM "Tags" AS "t"
    LEFT JOIN "Gears" AS "g0" ON "t"."GearNickName" = "g0"."Nickname" AND "t"."GearSquadId" = "g0"."SquadId"
) AS "s" ON "g"."FullName" = "s"."FullName"
""");
    }

    public override async Task Navigation_based_on_complex_expression1(bool async)
    {
        await base.Navigation_based_on_complex_expression1(async);

        AssertSql(
            """
SELECT "f"."Id", "f"."CapitalName", "f"."Discriminator", "f"."Name", "f"."ServerAddress", "f"."CommanderName", "f"."Eradicated"
FROM "Factions" AS "f"
LEFT JOIN (
    SELECT "l"."Name"
    FROM "LocustLeaders" AS "l"
    WHERE "l"."Discriminator" = 'LocustCommander'
) AS "l0" ON "f"."CommanderName" = "l0"."Name"
WHERE "l0"."Name" IS NOT NULL
""");
    }

    public override async Task Correlated_collections_basic_projecting_constant_bool(bool async)
    {
        await base.Correlated_collections_basic_projecting_constant_bool(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "w0"."c", "w0"."Id"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT 1 AS "c", "w"."Id", "w"."OwnerFullName"
    FROM "Weapons" AS "w"
    WHERE "w"."IsAutomatic" OR "w"."Name" <> 'foo' OR "w"."Name" IS NULL
) AS "w0" ON "g"."FullName" = "w0"."OwnerFullName"
WHERE "g"."Nickname" <> 'Marcus'
ORDER BY "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task Select_StartsWith_with_null_parameter_as_argument(bool async)
    {
        await base.Select_StartsWith_with_null_parameter_as_argument(async);

        AssertSql(
            """
SELECT 0
FROM "Gears" AS "g"
""");
    }

    public override async Task Comparing_two_collection_navigations_inheritance(bool async)
    {
        await base.Comparing_two_collection_navigations_inheritance(async);

        AssertSql(
            """
SELECT "f"."Name", "g0"."Nickname"
FROM "Factions" AS "f"
CROSS JOIN (
    SELECT "g"."Nickname", "g"."SquadId", "g"."HasSoulPatch"
    FROM "Gears" AS "g"
    WHERE "g"."Discriminator" = 'Officer'
) AS "g0"
LEFT JOIN (
    SELECT "l"."Name", "l"."DefeatedByNickname", "l"."DefeatedBySquadId"
    FROM "LocustLeaders" AS "l"
    WHERE "l"."Discriminator" = 'LocustCommander'
) AS "l0" ON "f"."CommanderName" = "l0"."Name"
LEFT JOIN "Gears" AS "g1" ON "l0"."DefeatedByNickname" = "g1"."Nickname" AND "l0"."DefeatedBySquadId" = "g1"."SquadId"
WHERE "g0"."HasSoulPatch" AND "g1"."Nickname" = "g0"."Nickname" AND "g1"."SquadId" = "g0"."SquadId"
""");
    }

    public override async Task Select_subquery_distinct_singleordefault_boolean1(bool async)
    {
        await base.Select_subquery_distinct_singleordefault_boolean1(async);

        AssertSql(
            """
SELECT COALESCE((
    SELECT "w0"."IsAutomatic"
    FROM (
        SELECT DISTINCT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
        FROM "Weapons" AS "w"
        WHERE "g"."FullName" = "w"."OwnerFullName" AND "w"."Name" IS NOT NULL AND instr("w"."Name", 'Lancer') > 0
    ) AS "w0"
    LIMIT 1), 0)
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch"
""");
    }

    public override async Task Join_entity_with_itself_grouped_by_key_followed_by_include_skip_take(bool async)
    {
        await base.Join_entity_with_itself_grouped_by_key_followed_by_include_skip_take(async);

        AssertSql(
            """
@__p_1='10'
@__p_0='0'

SELECT "s"."Nickname", "s"."SquadId", "s"."AssignedCityName", "s"."CityOfBirthName", "s"."Discriminator", "s"."FullName", "s"."HasSoulPatch", "s"."LeaderNickname", "s"."LeaderSquadId", "s"."Rank", "s"."HasSoulPatch0", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM (
    SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "g1"."HasSoulPatch" AS "HasSoulPatch0"
    FROM "Gears" AS "g"
    INNER JOIN (
        SELECT MIN(length("g0"."Nickname")) AS "c", "g0"."HasSoulPatch"
        FROM "Gears" AS "g0"
        WHERE "g0"."Nickname" <> 'Dom'
        GROUP BY "g0"."HasSoulPatch"
    ) AS "g1" ON length("g"."Nickname") = "g1"."c"
    ORDER BY "g"."Nickname"
    LIMIT @__p_1 OFFSET @__p_0
) AS "s"
LEFT JOIN "Weapons" AS "w" ON "s"."FullName" = "w"."OwnerFullName"
ORDER BY "s"."Nickname", "s"."SquadId", "s"."HasSoulPatch0"
""");
    }

    public override async Task Correlated_collections_nested_mixed_streaming_with_buffer1(bool async)
    {
        await base.Correlated_collections_nested_mixed_streaming_with_buffer1(async);

        AssertSql(
            """
SELECT "s"."Id", "s3"."SquadId", "s3"."MissionId", "s3"."Id", "s3"."SquadId0", "s3"."MissionId0"
FROM "Squads" AS "s"
LEFT JOIN (
    SELECT "s0"."SquadId", "s0"."MissionId", "m"."Id", "s2"."SquadId" AS "SquadId0", "s2"."MissionId" AS "MissionId0"
    FROM "SquadMissions" AS "s0"
    INNER JOIN "Missions" AS "m" ON "s0"."MissionId" = "m"."Id"
    LEFT JOIN (
        SELECT "s1"."SquadId", "s1"."MissionId"
        FROM "SquadMissions" AS "s1"
        WHERE "s1"."SquadId" < 2
    ) AS "s2" ON "m"."Id" = "s2"."MissionId"
    WHERE "s0"."MissionId" < 3
) AS "s3" ON "s"."Id" = "s3"."SquadId"
ORDER BY "s"."Id", "s3"."SquadId", "s3"."MissionId", "s3"."Id", "s3"."SquadId0"
""");
    }

    public override async Task Select_nested_ternary_operations(bool async)
    {
        await base.Select_nested_ternary_operations(async);

        AssertSql(
            """
SELECT "w"."Id", CASE
    WHEN NOT ("w"."IsAutomatic") THEN CASE
        WHEN "w"."AmmunitionType" = 1 THEN 'ManualCartridge'
        ELSE 'Manual'
    END
    ELSE 'Auto'
END AS "IsManualCartridge"
FROM "Weapons" AS "w"
""");
    }

    public override async Task Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column(bool async)
    {
        await base.Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column(async);

        AssertSql(
            """
@__cities_0='["Unknown","Jacinto\u0027s location","Ephyra\u0027s location"]' (Size = 62)

SELECT "c"."Name", "c"."Location", "c"."Nation"
FROM "Cities" AS "c"
WHERE "c"."Location" IN (
    SELECT "c0"."value"
    FROM json_each(@__cities_0) AS "c0"
)
""");
    }

    public override async Task Include_collection_with_Cast_to_base(bool async)
    {
        await base.Include_collection_with_Cast_to_base(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
WHERE "g"."Discriminator" = 'Officer'
ORDER BY "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task Select_required_navigation_on_the_same_type_with_cast(bool async)
    {
        await base.Select_required_navigation_on_the_same_type_with_cast(async);

        AssertSql(
            """
SELECT "c"."Name"
FROM "Gears" AS "g"
INNER JOIN "Cities" AS "c" ON "g"."CityOfBirthName" = "c"."Name"
""");
    }

    public override async Task Anonymous_projection_take_followed_by_projecting_single_element_from_collection_navigation(bool async)
    {
        await base.Anonymous_projection_take_followed_by_projecting_single_element_from_collection_navigation(async);

        AssertSql(
            """
@__p_0='25'

SELECT "w1"."Id", "w1"."AmmunitionType", "w1"."IsAutomatic", "w1"."Name", "w1"."OwnerFullName", "w1"."SynergyWithId"
FROM (
    SELECT "g"."FullName"
    FROM "Gears" AS "g"
    LIMIT @__p_0
) AS "g0"
LEFT JOIN (
    SELECT "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
    FROM (
        SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId", ROW_NUMBER() OVER(PARTITION BY "w"."OwnerFullName" ORDER BY "w"."Id") AS "row"
        FROM "Weapons" AS "w"
    ) AS "w0"
    WHERE "w0"."row" <= 1
) AS "w1" ON "g0"."FullName" = "w1"."OwnerFullName"
""");
    }

    public override async Task Left_join_with_GroupBy_with_composite_group_key(bool async)
    {
        await base.Left_join_with_GroupBy_with_composite_group_key(async);

        AssertSql(
            """
SELECT "g"."CityOfBirthName", "g"."HasSoulPatch"
FROM "Gears" AS "g"
INNER JOIN "Squads" AS "s" ON "g"."SquadId" = "s"."Id"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName"
GROUP BY "g"."CityOfBirthName", "g"."HasSoulPatch"
""");
    }

    public override async Task Where_subquery_concat_firstordefault_boolean(bool async)
    {
        await base.Where_subquery_concat_firstordefault_boolean(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch" AND (
    SELECT "u"."IsAutomatic"
    FROM (
        SELECT "w"."Id", "w"."IsAutomatic"
        FROM "Weapons" AS "w"
        WHERE "g"."FullName" = "w"."OwnerFullName"
        UNION ALL
        SELECT "w0"."Id", "w0"."IsAutomatic"
        FROM "Weapons" AS "w0"
        WHERE "g"."FullName" = "w0"."OwnerFullName"
    ) AS "u"
    ORDER BY "u"."Id"
    LIMIT 1)
""");
    }

    public override async Task Collection_with_inheritance_and_join_include_source(bool async)
    {
        await base.Collection_with_inheritance_and_join_include_source(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "t0"."Id", "t0"."GearNickName", "t0"."GearSquadId", "t0"."IssueDate", "t0"."Note"
FROM "Gears" AS "g"
INNER JOIN "Tags" AS "t" ON "g"."SquadId" = "t"."GearSquadId" AND "g"."Nickname" = "t"."GearNickName"
LEFT JOIN "Tags" AS "t0" ON "g"."Nickname" = "t0"."GearNickName" AND "g"."SquadId" = "t0"."GearSquadId"
WHERE "g"."Discriminator" = 'Officer'
""");
    }

    public override async Task OfTypeNav3(bool async)
    {
        await base.OfTypeNav3(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
INNER JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
LEFT JOIN "Tags" AS "t0" ON "g"."Nickname" = "t0"."GearNickName" AND "g"."SquadId" = "t0"."GearSquadId"
WHERE ("t"."Note" <> 'Foo' OR "t"."Note" IS NULL) AND "g"."Discriminator" = 'Officer' AND ("t0"."Note" <> 'Bar' OR "t0"."Note" IS NULL)
""");
    }

    public override async Task ToString_guid_property_projection(bool async)
    {
        await base.ToString_guid_property_projection(async);

        AssertSql(
            """
SELECT "t"."GearNickName" AS "A", CAST("t"."Id" AS TEXT) AS "B"
FROM "Tags" AS "t"
""");
    }

    public override async Task Join_with_inner_being_a_subquery_projecting_anonymous_type_with_single_property(bool async)
    {
        await base.Join_with_inner_being_a_subquery_projecting_anonymous_type_with_single_property(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
INNER JOIN "Gears" AS "g0" ON "g"."Nickname" = "g0"."Nickname"
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_all(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_all(async);

        AssertSql(
            """
SELECT NOT EXISTS (
    SELECT 1
    FROM "Tags" AS "t"
    LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
    WHERE ("t"."Note" <> 'K.I.A.' OR "t"."Note" IS NULL) AND NOT ("g"."HasSoulPatch"))
""");
    }

    public override async Task Collection_navigation_access_on_derived_entity_using_cast_in_SelectMany(bool async)
    {
        await base.Collection_navigation_access_on_derived_entity_using_cast_in_SelectMany(async);

        AssertSql(
            """
SELECT "f"."Name", "l"."Name" AS "LeaderName"
FROM "Factions" AS "f"
INNER JOIN "LocustLeaders" AS "l" ON "f"."Id" = "l"."LocustHordeId"
ORDER BY "l"."Name"
""");
    }

    public override async Task Where_bitwise_and_nullable_enum_with_nullable_parameter(bool async)
    {
        await base.Where_bitwise_and_nullable_enum_with_nullable_parameter(async);

        AssertSql(
            """
@__ammunitionType_0='1' (Nullable = true)

SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE "w"."AmmunitionType" & @__ammunitionType_0 > 0
""",
            //
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE "w"."AmmunitionType" & NULL > 0
""");
    }

    public override async Task Correlated_collections_on_left_join_with_null_value(bool async)
    {
        await base.Correlated_collections_on_left_join_with_null_value(async);

        AssertSql(
            """
SELECT "t"."Id", "g"."Nickname", "g"."SquadId", "w"."Name", "w"."Id"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
ORDER BY "t"."Note", "t"."Id", "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task Filtered_collection_projection_with_order_comparison_predicate_converted_to_join(bool async)
    {
        await base.Filtered_collection_projection_with_order_comparison_predicate_converted_to_join(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName" AND "g"."SquadId" < "w"."Id"
ORDER BY "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task Byte_array_filter_by_length_literal_does_not_cast_on_varbinary_n(bool async)
    {
        await base.Byte_array_filter_by_length_literal_does_not_cast_on_varbinary_n(async);

        AssertSql(
            """
SELECT "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s"."Name"
FROM "Squads" AS "s"
WHERE length("s"."Banner5") = 5
""");
    }

    public override async Task Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql(bool async)
    {
        await base.Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql(async);

        AssertSql(
            """
SELECT "s"."Name"
FROM "Squads" AS "s"
WHERE NOT EXISTS (
    SELECT 1
    FROM "Gears" AS "g"
    LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
    WHERE "s"."Id" = "g"."SquadId" AND "t"."Note" = 'Dom''s Tag')
""");
    }

    public override async Task Select_null_propagation_works_for_navigations_with_composite_keys(bool async)
    {
        await base.Select_null_propagation_works_for_navigations_with_composite_keys(async);

        AssertSql(
            """
SELECT "g"."Nickname"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
""");
    }

    public override async Task Correlated_collections_on_left_join_with_predicate(bool async)
    {
        await base.Correlated_collections_on_left_join_with_predicate(async);

        AssertSql(
            """
SELECT "g"."Nickname", "t"."Id", "g"."SquadId", "w"."Name", "w"."Id"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
WHERE NOT ("g"."HasSoulPatch")
ORDER BY "t"."Id", "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task Property_access_on_derived_entity_using_cast(bool async)
    {
        await base.Property_access_on_derived_entity_using_cast(async);

        AssertSql(
            """
SELECT "f"."Name", "f"."Eradicated"
FROM "Factions" AS "f"
ORDER BY "f"."Name"
""");
    }

    public override async Task Null_propagation_optimization3(bool async)
    {
        await base.Null_propagation_optimization3(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE CASE
    WHEN "g"."LeaderNickname" IS NOT NULL THEN "g"."LeaderNickname" LIKE '%us'
    ELSE NULL
END = 1
""");
    }

    public override async Task Correlated_collections_different_collections_projected(bool async)
    {
        await base.Correlated_collections_different_collections_projected(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "w0"."Name", "w0"."IsAutomatic", "w0"."Id", "g0"."Nickname", "g0"."Rank", "g0"."SquadId"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "w"."Name", "w"."IsAutomatic", "w"."Id", "w"."OwnerFullName"
    FROM "Weapons" AS "w"
    WHERE "w"."IsAutomatic"
) AS "w0" ON "g"."FullName" = "w0"."OwnerFullName"
LEFT JOIN "Gears" AS "g0" ON "g"."Nickname" = "g0"."LeaderNickname" AND "g"."SquadId" = "g0"."LeaderSquadId"
WHERE "g"."Discriminator" = 'Officer'
ORDER BY "g"."FullName", "g"."Nickname", "g"."SquadId", "w0"."Id", "g0"."FullName", "g0"."Nickname"
""");
    }

    public override async Task Correlated_collections_basic_projection_ordered(bool async)
    {
        await base.Correlated_collections_basic_projection_ordered(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
    FROM "Weapons" AS "w"
    WHERE "w"."IsAutomatic" OR "w"."Name" <> 'foo' OR "w"."Name" IS NULL
) AS "w0" ON "g"."FullName" = "w0"."OwnerFullName"
WHERE "g"."Nickname" <> 'Marcus'
ORDER BY "g"."Nickname", "g"."SquadId", "w0"."Name" DESC
""");
    }

    public override async Task Group_by_nullable_property_and_project_the_grouping_key_HasValue(bool async)
    {
        await base.Group_by_nullable_property_and_project_the_grouping_key_HasValue(async);

        AssertSql(
            """
SELECT "w"."SynergyWithId" IS NOT NULL
FROM "Weapons" AS "w"
GROUP BY "w"."SynergyWithId"
""");
    }

    public override async Task FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql(bool async)
    {
        await base.FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql(async);

        AssertSql(
            """
SELECT "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s"."Name"
FROM "Squads" AS "s"
LEFT JOIN "Gears" AS "g" ON "s"."Id" = "g"."SquadId"
WHERE "s"."Name" = 'Kilo'
LIMIT 1
""");
    }

    public override async Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_duplicated_orderings(
        bool async)
    {
        await base.Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_duplicated_orderings(async);

        AssertSql(
            """
SELECT "g"."FullName", "g"."Nickname", "g"."SquadId", "t"."Id", "g1"."Nickname", "g1"."SquadId", "s"."Id", "s"."AmmunitionType", "s"."IsAutomatic", "s"."Name", "s"."OwnerFullName", "s"."SynergyWithId", "s"."Nickname", "s"."SquadId"
FROM "Gears" AS "g"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
LEFT JOIN "Gears" AS "g1" ON "t"."GearNickName" = "g1"."Nickname" AND "t"."GearSquadId" = "g1"."SquadId"
LEFT JOIN (
    SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId", "g2"."Nickname", "g2"."SquadId"
    FROM "Weapons" AS "w"
    LEFT JOIN "Gears" AS "g2" ON "w"."OwnerFullName" = "g2"."FullName"
) AS "s" ON "g1"."FullName" = "s"."OwnerFullName"
WHERE "g"."Discriminator" = 'Officer' AND EXISTS (
    SELECT 1
    FROM "Gears" AS "g0"
    WHERE "g"."Nickname" = "g0"."LeaderNickname" AND "g"."SquadId" = "g0"."LeaderSquadId")
ORDER BY "g"."HasSoulPatch" DESC, "t"."Note", "g"."Nickname", "g"."SquadId", "t"."Id", "g1"."Nickname", "g1"."SquadId", "s"."IsAutomatic", "s"."Nickname" DESC, "s"."Id"
""");
    }

    public override async Task Select_subquery_int_with_inside_cast_and_coalesce(bool async)
    {
        await base.Select_subquery_int_with_inside_cast_and_coalesce(async);

        AssertSql(
            """
SELECT COALESCE((
    SELECT "w"."Id"
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName"
    ORDER BY "w"."Id"
    LIMIT 1), 42)
FROM "Gears" AS "g"
""");
    }

    public override async Task Cast_OfType_works_correctly(bool async)
    {
        await base.Cast_OfType_works_correctly(async);

        AssertSql(
            """
SELECT "g"."FullName"
FROM "Gears" AS "g"
WHERE "g"."Discriminator" = 'Officer'
""");
    }

    public override async Task DateTimeOffset_DateAdd_AddMinutes(bool async)
    {
        await base.DateTimeOffset_DateAdd_AddMinutes(async);

        AssertSql(
            """
SELECT "m"."Timeline"
FROM "Missions" AS "m"
""");
    }

    public override async Task Select_Singleton_Navigation_With_Member_Access(bool async)
    {
        await base.Select_Singleton_Navigation_With_Member_Access(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE "g"."Nickname" = 'Marcus' AND ("g"."CityOfBirthName" <> 'Ephyra' OR "g"."CityOfBirthName" IS NULL)
""");
    }

    public override async Task String_compare_with_null_conditional_argument2(bool async)
    {
        await base.String_compare_with_null_conditional_argument2(async);

        AssertSql(
            """
SELECT "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
FROM "Weapons" AS "w"
LEFT JOIN "Weapons" AS "w0" ON "w"."SynergyWithId" = "w0"."Id"
ORDER BY 'Marcus'' Lancer' = "w0"."Name" AND "w0"."Name" IS NOT NULL
""");
    }

    public override async Task Entity_equality_empty(bool async)
    {
        await base.Entity_equality_empty(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE 0
""");
    }

    public override async Task Where_with_enum_flags_parameter(bool async)
    {
        await base.Where_with_enum_flags_parameter(async);

        AssertSql(
            """
@__rank_0='1' (Nullable = true)

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."Rank" & @__rank_0 = @__rank_0
""",
            //
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
""",
            //
            """
@__rank_0='2' (Nullable = true)

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."Rank" | @__rank_0 <> @__rank_0
""",
            //
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE 0
""");
    }

    public override async Task Select_null_propagation_negative1(bool async)
    {
        await base.Select_null_propagation_negative1(async);

        AssertSql(
            """
SELECT CASE
    WHEN "g"."LeaderNickname" IS NOT NULL THEN length("g"."Nickname") = 5
    ELSE NULL
END
FROM "Gears" AS "g"
""");
    }

    public override async Task Contains_on_collection_of_nullable_byte_subquery_null_parameter(bool async)
    {
        await base.Contains_on_collection_of_nullable_byte_subquery_null_parameter(async);

        AssertSql(
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId"
FROM "LocustLeaders" AS "l"
WHERE EXISTS (
    SELECT 1
    FROM "LocustLeaders" AS "l0"
    WHERE "l0"."ThreatLevelNullableByte" IS NULL)
""");
    }

    public override async Task OfType_in_subquery_works(bool async)
    {
        await base.OfType_in_subquery_works(async);

        AssertSql(
            """
SELECT "s"."Name", "s"."Location", "s"."Nation"
FROM "Gears" AS "g"
INNER JOIN (
    SELECT "c"."Name", "c"."Location", "c"."Nation", "g0"."LeaderNickname", "g0"."LeaderSquadId"
    FROM "Gears" AS "g0"
    LEFT JOIN "Cities" AS "c" ON "g0"."AssignedCityName" = "c"."Name"
    WHERE "g0"."Discriminator" = 'Officer'
) AS "s" ON "g"."Nickname" = "s"."LeaderNickname" AND "g"."SquadId" = "s"."LeaderSquadId"
WHERE "g"."Discriminator" = 'Officer'
""");
    }

    public override async Task Correlated_collection_with_complex_OrderBy(bool async)
    {
        await base.Correlated_collection_with_complex_OrderBy(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g1"."Nickname", "g1"."SquadId", "g1"."AssignedCityName", "g1"."CityOfBirthName", "g1"."Discriminator", "g1"."FullName", "g1"."HasSoulPatch", "g1"."LeaderNickname", "g1"."LeaderSquadId", "g1"."Rank"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
    FROM "Gears" AS "g0"
    WHERE NOT ("g0"."HasSoulPatch")
) AS "g1" ON "g"."Nickname" = "g1"."LeaderNickname" AND "g"."SquadId" = "g1"."LeaderSquadId"
WHERE "g"."Discriminator" = 'Officer'
ORDER BY (
    SELECT COUNT(*)
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName"), "g"."Nickname", "g"."SquadId", "g1"."Nickname"
""");
    }

    public override async Task Correlated_collections_from_left_join_with_additional_elements_projected_of_that_join(bool async)
    {
        await base.Correlated_collections_from_left_join_with_additional_elements_projected_of_that_join(async);

        AssertSql(
            """
SELECT "w"."Id", "g"."Nickname", "g"."SquadId", "s"."Id", "s0"."Nickname", "s0"."SquadId", "s0"."Id", "s0"."AmmunitionType", "s0"."IsAutomatic", "s0"."Name", "s0"."OwnerFullName", "s0"."SynergyWithId", "s0"."Rank"
FROM "Weapons" AS "w"
LEFT JOIN "Gears" AS "g" ON "w"."OwnerFullName" = "g"."FullName"
LEFT JOIN "Squads" AS "s" ON "g"."SquadId" = "s"."Id"
LEFT JOIN (
    SELECT "g0"."Nickname", "g0"."SquadId", "w1"."Id", "w1"."AmmunitionType", "w1"."IsAutomatic", "w1"."Name", "w1"."OwnerFullName", "w1"."SynergyWithId", "g0"."Rank", "g0"."FullName"
    FROM "Gears" AS "g0"
    LEFT JOIN (
        SELECT "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
        FROM "Weapons" AS "w0"
        WHERE NOT ("w0"."IsAutomatic")
    ) AS "w1" ON "g0"."FullName" = "w1"."OwnerFullName"
) AS "s0" ON "s"."Id" = "s0"."SquadId"
ORDER BY "w"."Name", "w"."Id", "g"."Nickname", "g"."SquadId", "s"."Id", "s0"."FullName" DESC, "s0"."Nickname", "s0"."SquadId", "s0"."Id"
""");
    }

    public override async Task GetValueOrDefault_in_projection(bool async)
    {
        await base.GetValueOrDefault_in_projection(async);

        AssertSql(
            """
SELECT COALESCE("w"."SynergyWithId", 0)
FROM "Weapons" AS "w"
""");
    }

    public override async Task Correlated_collections_basic_projection_composite_key(bool async)
    {
        await base.Correlated_collections_basic_projection_composite_key(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g1"."Nickname", "g1"."FullName", "g1"."SquadId"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "g0"."Nickname", "g0"."FullName", "g0"."SquadId", "g0"."LeaderNickname", "g0"."LeaderSquadId"
    FROM "Gears" AS "g0"
    WHERE NOT ("g0"."HasSoulPatch")
) AS "g1" ON "g"."Nickname" = "g1"."LeaderNickname" AND "g"."SquadId" = "g1"."LeaderSquadId"
WHERE "g"."Discriminator" = 'Officer' AND "g"."Nickname" <> 'Foo'
ORDER BY "g"."Nickname", "g"."SquadId", "g1"."Nickname"
""");
    }

    public override async Task Left_join_projection_using_conditional_tracking(bool async)
    {
        await base.Left_join_projection_using_conditional_tracking(async);

        AssertSql(
            """
SELECT "g0"."Nickname" IS NULL OR "g0"."SquadId" IS NULL, "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."LeaderNickname" = "g0"."Nickname"
""");
    }

    public override async Task OfTypeNav1(bool async)
    {
        await base.OfTypeNav1(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
LEFT JOIN "Tags" AS "t0" ON "g"."Nickname" = "t0"."GearNickName" AND "g"."SquadId" = "t0"."GearSquadId"
WHERE ("t"."Note" <> 'Foo' OR "t"."Note" IS NULL) AND "g"."Discriminator" = 'Officer' AND ("t0"."Note" <> 'Bar' OR "t0"."Note" IS NULL)
""");
    }

    public override async Task Select_subquery_distinct_singleordefault_boolean_empty_with_pushdown(bool async)
    {
        await base.Select_subquery_distinct_singleordefault_boolean_empty_with_pushdown(async);

        AssertSql(
            """
SELECT (
    SELECT "w0"."IsAutomatic"
    FROM (
        SELECT DISTINCT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
        FROM "Weapons" AS "w"
        WHERE "g"."FullName" = "w"."OwnerFullName" AND "w"."Name" = 'BFG'
    ) AS "w0"
    LIMIT 1)
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch"
""");
    }

    public override async Task Correlated_collection_take(bool async)
    {
        await base.Correlated_collection_take(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "c"."Name", "w1"."Id", "w1"."AmmunitionType", "w1"."IsAutomatic", "w1"."Name", "w1"."OwnerFullName", "w1"."SynergyWithId", "c"."Location", "c"."Nation"
FROM "Gears" AS "g"
INNER JOIN "Cities" AS "c" ON "g"."CityOfBirthName" = "c"."Name"
LEFT JOIN (
    SELECT "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
    FROM (
        SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId", ROW_NUMBER() OVER(PARTITION BY "w"."OwnerFullName" ORDER BY "w"."Id") AS "row"
        FROM "Weapons" AS "w"
    ) AS "w0"
    WHERE "w0"."row" <= 10
) AS "w1" ON "g"."FullName" = "w1"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId", "c"."Name"
""");
    }

    public override async Task Where_enum(bool async)
    {
        await base.Where_enum(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."Rank" = 4
""");
    }

    public override async Task Where_has_flag_with_nullable_parameter(bool async)
    {
        await base.Where_has_flag_with_nullable_parameter(async);

        AssertSql(
            """
@__parameter_0='2' (Nullable = true)

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."Rank" & @__parameter_0 = @__parameter_0
""");
    }

    public override async Task Correlated_collection_with_complex_order_by_funcletized_to_constant_bool(bool async)
    {
        await base.Correlated_collection_with_complex_order_by_funcletized_to_constant_bool(async);

        AssertSql(
            """
@__nicknames_0='[]' (Size = 2)

SELECT "g"."Nickname", "g"."SquadId", "w"."Name", "w"."Id"
FROM "Gears" AS "g"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
ORDER BY "g"."Nickname" IN (
    SELECT "n"."value"
    FROM json_each(@__nicknames_0) AS "n"
) DESC, "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task Correlated_collections_basic_projecting_single_property(bool async)
    {
        await base.Correlated_collections_basic_projecting_single_property(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "w0"."Name", "w0"."Id"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "w"."Name", "w"."Id", "w"."OwnerFullName"
    FROM "Weapons" AS "w"
    WHERE "w"."IsAutomatic" OR "w"."Name" <> 'foo' OR "w"."Name" IS NULL
) AS "w0" ON "g"."FullName" = "w0"."OwnerFullName"
WHERE "g"."Nickname" <> 'Marcus'
ORDER BY "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task Unnecessary_include_doesnt_get_added_complex_when_projecting_EF_Property(bool async)
    {
        await base.Unnecessary_include_doesnt_get_added_complex_when_projecting_EF_Property(async);

        AssertSql(
            """
SELECT "g"."FullName"
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch"
ORDER BY "g"."Rank"
""");
    }

    public override async Task Subquery_with_result_operator_is_not_lifted(bool async)
    {
        await base.Subquery_with_result_operator_is_not_lifted(async);

        AssertSql(
            """
@__p_0='2'

SELECT "g0"."FullName"
FROM (
    SELECT "g"."FullName", "g"."Rank"
    FROM "Gears" AS "g"
    WHERE NOT ("g"."HasSoulPatch")
    ORDER BY "g"."FullName"
    LIMIT @__p_0
) AS "g0"
ORDER BY "g0"."Rank"
""");
    }

    public override async Task Accessing_reference_navigation_collection_composition_generates_single_query(bool async)
    {
        await base.Accessing_reference_navigation_collection_composition_generates_single_query(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "s"."Id", "s"."IsAutomatic", "s"."Name", "s"."Id0"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "w"."Id", "w"."IsAutomatic", "w0"."Name", "w0"."Id" AS "Id0", "w"."OwnerFullName"
    FROM "Weapons" AS "w"
    LEFT JOIN "Weapons" AS "w0" ON "w"."SynergyWithId" = "w0"."Id"
) AS "s" ON "g"."FullName" = "s"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId", "s"."Id"
""");
    }

    public override async Task Select_subquery_projecting_single_constant_null_of_non_mapped_type(bool async)
    {
        await base.Select_subquery_projecting_single_constant_null_of_non_mapped_type(async);

        AssertSql(
            """
SELECT "s"."Name", "g1"."c"
FROM "Squads" AS "s"
LEFT JOIN (
    SELECT "g0"."c", "g0"."SquadId"
    FROM (
        SELECT 1 AS "c", "g"."SquadId", ROW_NUMBER() OVER(PARTITION BY "g"."SquadId" ORDER BY "g"."Nickname", "g"."SquadId") AS "row"
        FROM "Gears" AS "g"
        WHERE "g"."HasSoulPatch"
    ) AS "g0"
    WHERE "g0"."row" <= 1
) AS "g1" ON "s"."Id" = "g1"."SquadId"
""");
    }

    public override async Task Subquery_is_lifted_from_main_from_clause_of_SelectMany(bool async)
    {
        await base.Subquery_is_lifted_from_main_from_clause_of_SelectMany(async);

        AssertSql(
            """
SELECT "g"."FullName" AS "Name1", "g0"."FullName" AS "Name2"
FROM "Gears" AS "g"
CROSS JOIN "Gears" AS "g0"
WHERE "g"."HasSoulPatch" AND NOT ("g0"."HasSoulPatch")
ORDER BY "g"."FullName"
""");
    }

    public override async Task Subquery_created_by_include_gets_lifted_nested(bool async)
    {
        await base.Subquery_created_by_include_gets_lifted_nested(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "c"."Name", "c"."Location", "c"."Nation"
FROM "Gears" AS "g"
INNER JOIN "Cities" AS "c" ON "g"."CityOfBirthName" = "c"."Name"
WHERE EXISTS (
    SELECT 1
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName") AND NOT ("g"."HasSoulPatch")
ORDER BY "g"."Nickname"
""");
    }

    public override async Task Where_bitwise_and_integral(bool async)
    {
        await base.Where_bitwise_and_integral(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."Rank" & 1 = 1
""",
            //
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE CAST("g"."Rank" AS INTEGER) & 1 = 1
""",
            //
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE CAST("g"."Rank" AS INTEGER) & 1 = 1
""");
    }

    public override async Task Select_multiple_conditions(bool async)
    {
        await base.Select_multiple_conditions(async);

        AssertSql(
            """
SELECT "w"."Id", NOT ("w"."IsAutomatic") AND "w"."SynergyWithId" = 1 AND "w"."SynergyWithId" IS NOT NULL AS "IsCartridge"
FROM "Weapons" AS "w"
""");
    }

    public override async Task Navigation_inside_interpolated_string_expanded(bool async)
    {
        await base.Navigation_inside_interpolated_string_expanded(async);

        AssertSql(
            """
SELECT "w"."SynergyWithId" IS NOT NULL, "w0"."OwnerFullName"
FROM "Weapons" AS "w"
LEFT JOIN "Weapons" AS "w0" ON "w"."SynergyWithId" = "w0"."Id"
""");
    }

    public override async Task Group_by_with_include_with_entity_in_result_selector(bool async)
    {
        await base.Group_by_with_include_with_entity_in_result_selector(async);

        AssertSql(
            """
SELECT "g1"."Rank", "g1"."c", "s0"."Nickname", "s0"."SquadId", "s0"."AssignedCityName", "s0"."CityOfBirthName", "s0"."Discriminator", "s0"."FullName", "s0"."HasSoulPatch", "s0"."LeaderNickname", "s0"."LeaderSquadId", "s0"."Rank", "s0"."Name", "s0"."Location", "s0"."Nation"
FROM (
    SELECT "g"."Rank", COUNT(*) AS "c"
    FROM "Gears" AS "g"
    GROUP BY "g"."Rank"
) AS "g1"
LEFT JOIN (
    SELECT "s"."Nickname", "s"."SquadId", "s"."AssignedCityName", "s"."CityOfBirthName", "s"."Discriminator", "s"."FullName", "s"."HasSoulPatch", "s"."LeaderNickname", "s"."LeaderSquadId", "s"."Rank", "s"."Name", "s"."Location", "s"."Nation"
    FROM (
        SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "c"."Name", "c"."Location", "c"."Nation", ROW_NUMBER() OVER(PARTITION BY "g0"."Rank" ORDER BY "g0"."Nickname") AS "row"
        FROM "Gears" AS "g0"
        INNER JOIN "Cities" AS "c" ON "g0"."CityOfBirthName" = "c"."Name"
    ) AS "s"
    WHERE "s"."row" <= 1
) AS "s0" ON "g1"."Rank" = "s0"."Rank"
ORDER BY "g1"."Rank"
""");
    }

    public override async Task Include_reference_on_derived_type_using_lambda(bool async)
    {
        await base.Include_reference_on_derived_type_using_lambda(async);

        AssertSql(
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "LocustLeaders" AS "l"
LEFT JOIN "Gears" AS "g" ON "l"."DefeatedByNickname" = "g"."Nickname" AND "l"."DefeatedBySquadId" = "g"."SquadId"
""");
    }

    public override async Task SelectMany_predicate_with_non_equality_comparison_DefaultIfEmpty_converted_to_left_join(bool async)
    {
        await base.SelectMany_predicate_with_non_equality_comparison_DefaultIfEmpty_converted_to_left_join(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" <> "w"."OwnerFullName" OR "w"."OwnerFullName" IS NULL
ORDER BY "g"."Nickname", "w"."Id"
""");
    }

    public override async Task Include_reference_on_derived_type_using_lambda_with_soft_cast(bool async)
    {
        await base.Include_reference_on_derived_type_using_lambda_with_soft_cast(async);

        AssertSql(
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "LocustLeaders" AS "l"
LEFT JOIN "Gears" AS "g" ON "l"."DefeatedByNickname" = "g"."Nickname" AND "l"."DefeatedBySquadId" = "g"."SquadId"
""");
    }

    public override async Task Project_collection_navigation_with_inheritance1(bool async)
    {
        await base.Project_collection_navigation_with_inheritance1(async);

        AssertSql(
            """
SELECT "f"."Id", "l0"."Name", "f0"."Id", "l1"."Name", "l1"."Discriminator", "l1"."LocustHordeId", "l1"."ThreatLevel", "l1"."ThreatLevelByte", "l1"."ThreatLevelNullableByte", "l1"."DefeatedByNickname", "l1"."DefeatedBySquadId", "l1"."HighCommandId"
FROM "Factions" AS "f"
LEFT JOIN (
    SELECT "l"."Name"
    FROM "LocustLeaders" AS "l"
    WHERE "l"."Discriminator" = 'LocustCommander'
) AS "l0" ON "f"."CommanderName" = "l0"."Name"
LEFT JOIN "Factions" AS "f0" ON "l0"."Name" = "f0"."CommanderName"
LEFT JOIN "LocustLeaders" AS "l1" ON "f0"."Id" = "l1"."LocustHordeId"
ORDER BY "f"."Id", "l0"."Name", "f0"."Id"
""");
    }

    public override async Task Where_bool_column_and_Contains(bool async)
    {
        await base.Where_bool_column_and_Contains(async);

        AssertSql(
            """
@__values_0='[false,true]' (Size = 12)

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch" AND "g"."HasSoulPatch" IN (
    SELECT "v"."value"
    FROM json_each(@__values_0) AS "v"
)
""");
    }

    public override async Task Projecting_property_converted_to_nullable_into_unary(bool async)
    {
        await base.Projecting_property_converted_to_nullable_into_unary(async);

        AssertSql(
            """
SELECT "t"."Note"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE CASE
    WHEN "t"."GearNickName" IS NOT NULL THEN "g"."Nickname"
    ELSE NULL
END IS NOT NULL AND NOT (CASE
    WHEN "t"."GearNickName" IS NOT NULL THEN "g"."HasSoulPatch"
    ELSE NULL
END)
ORDER BY "t"."Note"
""");
    }

    public override async Task Where_enum_has_flag_subquery_client_eval(bool async)
    {
        await base.Where_enum_has_flag_subquery_client_eval(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."Rank" & (
    SELECT "g0"."Rank"
    FROM "Gears" AS "g0"
    ORDER BY "g0"."Nickname", "g0"."SquadId"
    LIMIT 1) = (
    SELECT "g0"."Rank"
    FROM "Gears" AS "g0"
    ORDER BY "g0"."Nickname", "g0"."SquadId"
    LIMIT 1) OR (
    SELECT "g0"."Rank"
    FROM "Gears" AS "g0"
    ORDER BY "g0"."Nickname", "g0"."SquadId"
    LIMIT 1) IS NULL
""");
    }

    public override async Task Navigation_access_via_EFProperty_on_derived_entity_using_cast(bool async)
    {
        await base.Navigation_access_via_EFProperty_on_derived_entity_using_cast(async);

        AssertSql(
            """
SELECT "f"."Name", "l0"."ThreatLevel" AS "Threat"
FROM "Factions" AS "f"
LEFT JOIN (
    SELECT "l"."Name", "l"."ThreatLevel"
    FROM "LocustLeaders" AS "l"
    WHERE "l"."Discriminator" = 'LocustCommander'
) AS "l0" ON "f"."CommanderName" = "l0"."Name"
ORDER BY "f"."Name"
""");
    }

    public override async Task Left_join_predicate_condition_equals_condition(bool async)
    {
        await base.Left_join_predicate_condition_equals_condition(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Weapons" AS "w" ON "w"."SynergyWithId" IS NOT NULL
""");
    }

    public override async Task Where_subquery_distinct_singleordefault_boolean2(bool async)
    {
        await base.Where_subquery_distinct_singleordefault_boolean2(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch" AND COALESCE((
    SELECT DISTINCT "w"."IsAutomatic"
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName" AND "w"."Name" IS NOT NULL AND instr("w"."Name", 'Lancer') > 0
    LIMIT 1), 0)
ORDER BY "g"."Nickname"
""");
    }

    public override async Task Join_on_entity_qsre_keys_outer_key_is_navigation(bool async)
    {
        await base.Join_on_entity_qsre_keys_outer_key_is_navigation(async);

        AssertSql(
            """
SELECT "w"."Name" AS "Name1", "w1"."Name" AS "Name2"
FROM "Weapons" AS "w"
LEFT JOIN "Weapons" AS "w0" ON "w"."SynergyWithId" = "w0"."Id"
INNER JOIN "Weapons" AS "w1" ON "w0"."Id" = "w1"."Id"
""");
    }

    public override async Task Enum_ToString_is_client_eval(bool async)
    {
        await base.Enum_ToString_is_client_eval(async);

        AssertSql(
            """
SELECT "g"."Rank"
FROM "Gears" AS "g"
ORDER BY "g"."SquadId", "g"."Nickname"
""");
    }

    public override async Task Include_with_join_collection2(bool async)
    {
        await base.Include_with_join_collection2(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "t"."Id", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Tags" AS "t"
INNER JOIN "Gears" AS "g" ON "t"."GearSquadId" = "g"."SquadId" AND "t"."GearNickName" = "g"."Nickname"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
ORDER BY "t"."Id", "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task GetValueOrDefault_with_argument(bool async)
    {
        await base.GetValueOrDefault_with_argument(async);

        AssertSql(
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE COALESCE("w"."SynergyWithId", "w"."Id") = 1
""");
    }

    public override async Task Projecting_property_converted_to_nullable_with_function_call(bool async)
    {
        await base.Projecting_property_converted_to_nullable_with_function_call(async);

        AssertSql(
            """
SELECT substr(CASE
    WHEN "t"."GearNickName" IS NOT NULL THEN "g"."Nickname"
    ELSE NULL
END, 0 + 1, 3)
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
""");
    }

    public override async Task Order_by_entity_qsre_with_inheritance(bool async)
    {
        await base.Order_by_entity_qsre_with_inheritance(async);

        AssertSql(
            """
SELECT "l"."Name"
FROM "LocustLeaders" AS "l"
INNER JOIN "LocustHighCommands" AS "l0" ON "l"."HighCommandId" = "l0"."Id"
WHERE "l"."Discriminator" = 'LocustCommander'
ORDER BY "l0"."Id", "l"."Name"
""");
    }

    public override async Task Where_is_properly_lifted_from_subquery_created_by_include(bool async)
    {
        await base.Where_is_properly_lifted_from_subquery_created_by_include(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Gears" AS "g"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
WHERE "g"."FullName" <> 'Augustus Cole' AND NOT ("g"."HasSoulPatch")
ORDER BY "g"."FullName"
""");
    }

    public override async Task Group_by_on_StartsWith_with_null_parameter_as_argument(bool async)
    {
        await base.Group_by_on_StartsWith_with_null_parameter_as_argument(async);

        AssertSql(
            """
SELECT "g0"."Key"
FROM (
    SELECT 0 AS "Key"
    FROM "Gears" AS "g"
) AS "g0"
GROUP BY "g0"."Key"
""");
    }

    public override async Task Non_unicode_parameter_is_used_for_non_unicode_column(bool async)
    {
        await base.Non_unicode_parameter_is_used_for_non_unicode_column(async);

        AssertSql(
            """
@__value_0='Unknown' (Size = 7)

SELECT "c"."Name", "c"."Location", "c"."Nation"
FROM "Cities" AS "c"
WHERE "c"."Location" = @__value_0
""");
    }

    public override async Task TimeSpan_Seconds(bool async)
    {
        await base.TimeSpan_Seconds(async);

        AssertSql(
            """
SELECT "m"."Duration"
FROM "Missions" AS "m"
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_contains(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_contains(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE ("t"."Note" <> 'K.I.A.' OR "t"."Note" IS NULL) AND "g"."SquadId" IN (
    SELECT "g0"."SquadId"
    FROM "Gears" AS "g0"
)
""");
    }

    public override async Task Navigation_based_on_complex_expression2(bool async)
    {
        await base.Navigation_based_on_complex_expression2(async);

        AssertSql(
            """
SELECT "f"."Id", "f"."CapitalName", "f"."Discriminator", "f"."Name", "f"."ServerAddress", "f"."CommanderName", "f"."Eradicated"
FROM "Factions" AS "f"
LEFT JOIN (
    SELECT "l"."Name"
    FROM "LocustLeaders" AS "l"
    WHERE "l"."Discriminator" = 'LocustCommander'
) AS "l0" ON "f"."CommanderName" = "l0"."Name"
WHERE "l0"."Name" IS NOT NULL
""");
    }

    public override async Task GroupBy_with_boolean_grouping_key(bool async)
    {
        await base.GroupBy_with_boolean_grouping_key(async);

        AssertSql(
            """
SELECT "g0"."CityOfBirthName", "g0"."HasSoulPatch", "g0"."IsMarcus", COUNT(*) AS "Count"
FROM (
    SELECT "g"."CityOfBirthName", "g"."HasSoulPatch", "g"."Nickname" = 'Marcus' AS "IsMarcus"
    FROM "Gears" AS "g"
) AS "g0"
GROUP BY "g0"."CityOfBirthName", "g0"."HasSoulPatch", "g0"."IsMarcus"
""");
    }

    public override async Task Correlated_collections_on_select_many(bool async)
    {
        await base.Correlated_collections_on_select_many(async);

        AssertSql(
            """
SELECT "g"."Nickname", "s"."Name", "g"."SquadId", "s"."Id", "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId", "g1"."Nickname", "g1"."SquadId", "g1"."AssignedCityName", "g1"."CityOfBirthName", "g1"."Discriminator", "g1"."FullName", "g1"."HasSoulPatch", "g1"."LeaderNickname", "g1"."LeaderSquadId", "g1"."Rank"
FROM "Gears" AS "g"
CROSS JOIN "Squads" AS "s"
LEFT JOIN (
    SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
    FROM "Weapons" AS "w"
    WHERE "w"."IsAutomatic" OR "w"."Name" <> 'foo' OR "w"."Name" IS NULL
) AS "w0" ON "g"."FullName" = "w0"."OwnerFullName"
LEFT JOIN (
    SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
    FROM "Gears" AS "g0"
    WHERE NOT ("g0"."HasSoulPatch")
) AS "g1" ON "s"."Id" = "g1"."SquadId"
WHERE "g"."HasSoulPatch"
ORDER BY "g"."Nickname", "s"."Id" DESC, "g"."SquadId", "w0"."Id", "g1"."Nickname"
""");
    }

    public override async Task Correlated_collection_with_top_level_Last_with_order_by_on_inner(bool async)
    {
        await base.Correlated_collection_with_top_level_Last_with_order_by_on_inner(async);

        AssertSql(
            """
SELECT "g0"."Nickname", "g0"."SquadId", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM (
    SELECT "g"."Nickname", "g"."SquadId", "g"."FullName"
    FROM "Gears" AS "g"
    ORDER BY "g"."FullName" DESC
    LIMIT 1
) AS "g0"
LEFT JOIN "Weapons" AS "w" ON "g0"."FullName" = "w"."OwnerFullName"
ORDER BY "g0"."FullName" DESC, "g0"."Nickname", "g0"."SquadId", "w"."Name"
""");
    }

    public override async Task Correlated_collections_naked_navigation_with_ToList_followed_by_projecting_count(bool async)
    {
        await base.Correlated_collections_naked_navigation_with_ToList_followed_by_projecting_count(async);

        AssertSql(
            """
SELECT (
    SELECT COUNT(*)
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName")
FROM "Gears" AS "g"
WHERE "g"."Nickname" <> 'Marcus'
ORDER BY "g"."Nickname"
""");
    }

    public override async Task Non_unicode_string_literal_is_used_for_non_unicode_column_right(bool async)
    {
        await base.Non_unicode_string_literal_is_used_for_non_unicode_column_right(async);

        AssertSql(
            """
SELECT "c"."Name", "c"."Location", "c"."Nation"
FROM "Cities" AS "c"
WHERE 'Unknown' = "c"."Location"
""");
    }

    public override async Task Cast_to_derived_followed_by_multiple_includes(bool async)
    {
        await base.Cast_to_derived_followed_by_multiple_includes(async);

        AssertSql(
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "LocustLeaders" AS "l"
LEFT JOIN "Gears" AS "g" ON "l"."DefeatedByNickname" = "g"."Nickname" AND "l"."DefeatedBySquadId" = "g"."SquadId"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
WHERE instr("l"."Name", 'Queen') > 0
ORDER BY "l"."Name", "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task Collection_navigation_access_on_derived_entity_using_cast(bool async)
    {
        await base.Collection_navigation_access_on_derived_entity_using_cast(async);

        AssertSql(
            """
SELECT "f"."Name", (
    SELECT COUNT(*)
    FROM "LocustLeaders" AS "l"
    WHERE "f"."Id" = "l"."LocustHordeId") AS "LeadersCount"
FROM "Factions" AS "f"
ORDER BY "f"."Name"
""");
    }

    public override async Task Select_subquery_projecting_single_constant_bool(bool async)
    {
        await base.Select_subquery_projecting_single_constant_bool(async);

        AssertSql(
            """
SELECT "s"."Name", COALESCE((
    SELECT 1
    FROM "Gears" AS "g"
    WHERE "s"."Id" = "g"."SquadId" AND "g"."HasSoulPatch"
    LIMIT 1), 0) AS "Gear"
FROM "Squads" AS "s"
""");
    }

    public override async Task Member_access_on_derived_materialized_entity_using_cast(bool async)
    {
        await base.Member_access_on_derived_materialized_entity_using_cast(async);

        AssertSql(
            """
SELECT "f"."Id", "f"."CapitalName", "f"."Discriminator", "f"."Name", "f"."ServerAddress", "f"."CommanderName", "f"."Eradicated"
FROM "Factions" AS "f"
ORDER BY "f"."Name"
""");
    }

    public override async Task Project_entity_and_collection_element(bool async)
    {
        await base.Project_entity_and_collection_element(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s"."Name", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId", "w2"."Id", "w2"."AmmunitionType", "w2"."IsAutomatic", "w2"."Name", "w2"."OwnerFullName", "w2"."SynergyWithId"
FROM "Gears" AS "g"
INNER JOIN "Squads" AS "s" ON "g"."SquadId" = "s"."Id"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
LEFT JOIN (
    SELECT "w1"."Id", "w1"."AmmunitionType", "w1"."IsAutomatic", "w1"."Name", "w1"."OwnerFullName", "w1"."SynergyWithId"
    FROM (
        SELECT "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId", ROW_NUMBER() OVER(PARTITION BY "w0"."OwnerFullName" ORDER BY "w0"."Id") AS "row"
        FROM "Weapons" AS "w0"
    ) AS "w1"
    WHERE "w1"."row" <= 1
) AS "w2" ON "g"."FullName" = "w2"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId", "s"."Id"
""");
    }

    public override async Task Where_subquery_distinct_first_boolean(bool async)
    {
        await base.Where_subquery_distinct_first_boolean(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch" AND (
    SELECT "w0"."IsAutomatic"
    FROM (
        SELECT DISTINCT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
        FROM "Weapons" AS "w"
        WHERE "g"."FullName" = "w"."OwnerFullName"
    ) AS "w0"
    ORDER BY "w0"."Id"
    LIMIT 1)
ORDER BY "g"."Nickname"
""");
    }

    public override async Task Where_subquery_union_firstordefault_boolean(bool async)
    {
        await base.Where_subquery_union_firstordefault_boolean(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch" AND (
    SELECT "u"."IsAutomatic"
    FROM (
        SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
        FROM "Weapons" AS "w"
        WHERE "g"."FullName" = "w"."OwnerFullName"
        UNION
        SELECT "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
        FROM "Weapons" AS "w0"
        WHERE "g"."FullName" = "w0"."OwnerFullName"
    ) AS "u"
    ORDER BY "u"."Id"
    LIMIT 1)
""");
    }

    public override async Task Subquery_containing_join_gets_lifted_clashing_names(bool async)
    {
        await base.Subquery_containing_join_gets_lifted_clashing_names(async);

        AssertSql(
            """
SELECT "g"."Nickname"
FROM "Gears" AS "g"
INNER JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName"
INNER JOIN "Tags" AS "t0" ON "g"."Nickname" = "t0"."GearNickName"
WHERE "t"."GearNickName" <> 'Cole Train' OR "t"."GearNickName" IS NULL
ORDER BY "g"."Nickname", "t0"."Id"
""");
    }

    public override async Task Projecting_property_converted_to_nullable_with_addition(bool async)
    {
        await base.Projecting_property_converted_to_nullable_with_addition(async);

        AssertSql(
            """
SELECT "t"."Note", "t"."GearNickName" IS NOT NULL, "g"."Nickname", "g"."SquadId", "g"."HasSoulPatch"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE CASE
    WHEN "t"."GearNickName" IS NOT NULL THEN "g"."SquadId"
    ELSE NULL
END + 1 = 2
""");
    }

    public override async Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery(bool async)
    {
        await base.Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery(async);

        AssertSql(
            """
SELECT "g"."FullName", "g"."Nickname", "g"."SquadId", "t"."Id", "g1"."Nickname", "g1"."SquadId", "s"."Id", "s"."AmmunitionType", "s"."IsAutomatic", "s"."Name", "s"."OwnerFullName", "s"."SynergyWithId", "s"."Nickname", "s"."SquadId"
FROM "Gears" AS "g"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
LEFT JOIN "Gears" AS "g1" ON "t"."GearNickName" = "g1"."Nickname" AND "t"."GearSquadId" = "g1"."SquadId"
LEFT JOIN (
    SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId", "g2"."Nickname", "g2"."SquadId"
    FROM "Weapons" AS "w"
    LEFT JOIN "Gears" AS "g2" ON "w"."OwnerFullName" = "g2"."FullName"
) AS "s" ON "g1"."FullName" = "s"."OwnerFullName"
WHERE "g"."Discriminator" = 'Officer' AND EXISTS (
    SELECT 1
    FROM "Gears" AS "g0"
    WHERE "g"."Nickname" = "g0"."LeaderNickname" AND "g"."SquadId" = "g0"."LeaderSquadId")
ORDER BY "g"."HasSoulPatch" DESC, "t"."Note", "g"."Nickname", "g"."SquadId", "t"."Id", "g1"."Nickname", "g1"."SquadId", "s"."IsAutomatic", "s"."Nickname" DESC, "s"."Id"
""");
    }

    public override async Task Select_subquery_projecting_single_constant_of_non_mapped_type(bool async)
    {
        await base.Select_subquery_projecting_single_constant_of_non_mapped_type(async);

        AssertSql(
            """
SELECT "s"."Name", "g1"."c"
FROM "Squads" AS "s"
LEFT JOIN (
    SELECT "g0"."c", "g0"."SquadId"
    FROM (
        SELECT 1 AS "c", "g"."SquadId", ROW_NUMBER() OVER(PARTITION BY "g"."SquadId" ORDER BY "g"."Nickname", "g"."SquadId") AS "row"
        FROM "Gears" AS "g"
        WHERE "g"."HasSoulPatch"
    ) AS "g0"
    WHERE "g0"."row" <= 1
) AS "g1" ON "s"."Id" = "g1"."SquadId"
""");
    }

    public override async Task Correlated_collection_with_very_complex_order_by(bool async)
    {
        await base.Correlated_collection_with_very_complex_order_by(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g2"."Nickname", "g2"."SquadId", "g2"."AssignedCityName", "g2"."CityOfBirthName", "g2"."Discriminator", "g2"."FullName", "g2"."HasSoulPatch", "g2"."LeaderNickname", "g2"."LeaderSquadId", "g2"."Rank"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "g1"."Nickname", "g1"."SquadId", "g1"."AssignedCityName", "g1"."CityOfBirthName", "g1"."Discriminator", "g1"."FullName", "g1"."HasSoulPatch", "g1"."LeaderNickname", "g1"."LeaderSquadId", "g1"."Rank"
    FROM "Gears" AS "g1"
    WHERE NOT ("g1"."HasSoulPatch")
) AS "g2" ON "g"."Nickname" = "g2"."LeaderNickname" AND "g"."SquadId" = "g2"."LeaderSquadId"
WHERE "g"."Discriminator" = 'Officer'
ORDER BY (
    SELECT COUNT(*)
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName" AND "w"."IsAutomatic" = COALESCE((
        SELECT "g0"."HasSoulPatch"
        FROM "Gears" AS "g0"
        WHERE "g0"."Nickname" = 'Marcus'
        LIMIT 1), 0)), "g"."Nickname", "g"."SquadId", "g2"."Nickname"
""");
    }

    public override async Task Contains_on_nullable_array_produces_correct_sql(bool async)
    {
        await base.Contains_on_nullable_array_produces_correct_sql(async);

        AssertSql(
            """
@__cities_0_without_nulls='["Ephyra"]' (Size = 10)

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Cities" AS "c" ON "g"."AssignedCityName" = "c"."Name"
WHERE "g"."SquadId" < 2 AND ("c"."Name" IN (
    SELECT "c0"."value"
    FROM json_each(@__cities_0_without_nulls) AS "c0"
) OR "c"."Name" IS NULL)
""");
    }

    public override async Task Include_after_Select_throws(bool async)
    {
        await base.Include_after_Select_throws(async);

        AssertSql(
            """
SELECT "f"."Id", "f"."CapitalName", "f"."Discriminator", "f"."Name", "f"."ServerAddress", "f"."CommanderName", "f"."Eradicated", "c"."Name", "c"."Location", "c"."Nation"
FROM "Factions" AS "f"
LEFT JOIN "Cities" AS "c" ON "f"."CapitalName" = "c"."Name"
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_predicate_negated_complex1(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_predicate_negated_complex1(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE NOT (CASE
    WHEN "g"."HasSoulPatch" THEN 1
    ELSE "g"."HasSoulPatch"
END)
""");
    }

    public override async Task DateTimeOffset_DateAdd_AddHours(bool async)
    {
        await base.DateTimeOffset_DateAdd_AddHours(async);

        AssertSql(
            """
SELECT "m"."Timeline"
FROM "Missions" AS "m"
""");
    }

    public override async Task Join_inner_source_custom_projection_followed_by_filter(bool async)
    {
        await base.Join_inner_source_custom_projection_followed_by_filter(async);

        AssertSql(
            """
SELECT CASE
    WHEN "f"."Name" = 'Locust' THEN 1
    ELSE NULL
END AS "IsEradicated", "f"."CommanderName", "f"."Name"
FROM "LocustLeaders" AS "l"
INNER JOIN "Factions" AS "f" ON "l"."Name" = "f"."CommanderName"
WHERE CASE
    WHEN "f"."Name" = 'Locust' THEN 1
    ELSE NULL
END <> 1 OR CASE
    WHEN "f"."Name" = 'Locust' THEN 1
    ELSE NULL
END IS NULL
""");
    }

    public override async Task Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery(bool async)
    {
        await base.Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
INNER JOIN "Cities" AS "c" ON "g"."CityOfBirthName" = "c"."Name"
WHERE "g"."Nickname" = 'Marcus' AND "c"."Location" = 'Jacinto''s location'
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_predicate_negated(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_predicate_negated(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE NOT ("g"."HasSoulPatch")
""");
    }

    public override async Task Where_conditional_equality_1(bool async)
    {
        await base.Where_conditional_equality_1(async);

        AssertSql(
            """
SELECT "g"."Nickname"
FROM "Gears" AS "g"
WHERE "g"."LeaderNickname" IS NULL
ORDER BY "g"."Nickname"
""");
    }

    public override async Task Where_bitwise_and_nullable_enum_with_null_constant(bool async)
    {
        await base.Where_bitwise_and_nullable_enum_with_null_constant(async);

        AssertSql(
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE "w"."AmmunitionType" & NULL > 0
""");
    }

    public override async Task Where_datetimeoffset_milliseconds_parameter_and_constant(bool async)
    {
        await base.Where_datetimeoffset_milliseconds_parameter_and_constant(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM "Missions" AS "m"
WHERE "m"."Timeline" = '1902-01-02 10:00:00.1234567+01:30'
""");
    }

    public override async Task Parameter_used_multiple_times_take_appropriate_inferred_type_mapping(bool async)
    {
        await base.Parameter_used_multiple_times_take_appropriate_inferred_type_mapping(async);

        AssertSql(
            """
@__place_0='Ephyra's location' (Size = 17)

SELECT "c"."Name", "c"."Location", "c"."Nation"
FROM "Cities" AS "c"
WHERE "c"."Nation" = @__place_0 OR "c"."Location" = @__place_0 OR "c"."Location" = @__place_0
""");
    }

    public override async Task Correlated_collection_with_top_level_Count(bool async)
    {
        await base.Correlated_collection_with_top_level_Count(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM "Gears" AS "g"
""");
    }

    public override async Task Complex_GroupBy_after_set_operator(bool async)
    {
        await base.Complex_GroupBy_after_set_operator(async);

        AssertSql(
            """
SELECT "u"."Name", "u"."Count", COALESCE(SUM("u"."Count"), 0) AS "Sum"
FROM (
    SELECT "c"."Name", (
        SELECT COUNT(*)
        FROM "Weapons" AS "w"
        WHERE "g"."FullName" = "w"."OwnerFullName") AS "Count"
    FROM "Gears" AS "g"
    LEFT JOIN "Cities" AS "c" ON "g"."AssignedCityName" = "c"."Name"
    UNION ALL
    SELECT "c0"."Name", (
        SELECT COUNT(*)
        FROM "Weapons" AS "w0"
        WHERE "g0"."FullName" = "w0"."OwnerFullName") AS "Count"
    FROM "Gears" AS "g0"
    INNER JOIN "Cities" AS "c0" ON "g0"."CityOfBirthName" = "c0"."Name"
) AS "u"
GROUP BY "u"."Name", "u"."Count"
""");
    }

    public override async Task Projecting_property_converted_to_nullable_into_member_assignment(bool async)
    {
        await base.Projecting_property_converted_to_nullable_into_member_assignment(async);

        AssertSql(
            """
SELECT CASE
    WHEN "t"."GearNickName" IS NOT NULL THEN "g"."SquadId"
    ELSE NULL
END AS "Id"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE CASE
    WHEN "t"."GearNickName" IS NOT NULL THEN "g"."Nickname"
    ELSE NULL
END IS NOT NULL
ORDER BY "t"."Note"
""");
    }

    public override async Task Where_subquery_distinct_singleordefault_boolean_with_pushdown(bool async)
    {
        await base.Where_subquery_distinct_singleordefault_boolean_with_pushdown(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch" AND (
    SELECT "w0"."IsAutomatic"
    FROM (
        SELECT DISTINCT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
        FROM "Weapons" AS "w"
        WHERE "g"."FullName" = "w"."OwnerFullName" AND "w"."Name" IS NOT NULL AND instr("w"."Name", 'Lancer') > 0
    ) AS "w0"
    LIMIT 1)
ORDER BY "g"."Nickname"
""");
    }

    public override async Task Nav_rewrite_with_convert1(bool async)
    {
        await base.Nav_rewrite_with_convert1(async);

        AssertSql(
            """
SELECT "l0"."Name", "l0"."Discriminator", "l0"."LocustHordeId", "l0"."ThreatLevel", "l0"."ThreatLevelByte", "l0"."ThreatLevelNullableByte", "l0"."DefeatedByNickname", "l0"."DefeatedBySquadId", "l0"."HighCommandId"
FROM "Factions" AS "f"
LEFT JOIN "Cities" AS "c" ON "f"."CapitalName" = "c"."Name"
LEFT JOIN (
    SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId"
    FROM "LocustLeaders" AS "l"
    WHERE "l"."Discriminator" = 'LocustCommander'
) AS "l0" ON "f"."CommanderName" = "l0"."Name"
WHERE "c"."Name" <> 'Foo' OR "c"."Name" IS NULL
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_orderby(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_orderby(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE "t"."Note" <> 'K.I.A.' OR "t"."Note" IS NULL
ORDER BY "g"."SquadId"
""");
    }

    public override async Task Comparison_with_value_converted_subclass(bool async)
    {
        await base.Comparison_with_value_converted_subclass(async);

        AssertSql(
            """
SELECT "f"."Id", "f"."CapitalName", "f"."Discriminator", "f"."Name", "f"."ServerAddress", "f"."CommanderName", "f"."Eradicated"
FROM "Factions" AS "f"
WHERE "f"."ServerAddress" = CAST('127.0.0.1' AS TEXT)
""");
    }

    public override async Task GetValueOrDefault_in_filter_non_nullable_column(bool async)
    {
        await base.GetValueOrDefault_in_filter_non_nullable_column(async);

        AssertSql(
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE COALESCE("w"."Id", 0) = 0
""");
    }

    public override async Task Enum_array_contains(bool async)
    {
        await base.Enum_array_contains(async);

        AssertSql(
            """
@__types_0_without_nulls='[1]' (Size = 3)

SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
LEFT JOIN "Weapons" AS "w0" ON "w"."SynergyWithId" = "w0"."Id"
WHERE "w0"."Id" IS NOT NULL AND ("w0"."AmmunitionType" IN (
    SELECT "t"."value"
    FROM json_each(@__types_0_without_nulls) AS "t"
) OR "w0"."AmmunitionType" IS NULL)
""");
    }

    public override async Task Include_multiple_one_to_one_optional_and_one_to_one_required(bool async)
    {
        await base.Include_multiple_one_to_one_optional_and_one_to_one_required(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s"."Name"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
LEFT JOIN "Squads" AS "s" ON "g"."SquadId" = "s"."Id"
""");
    }

    public override async Task Include_with_join_reference1(bool async)
    {
        await base.Include_with_join_reference1(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "c"."Name", "c"."Location", "c"."Nation"
FROM "Gears" AS "g"
INNER JOIN "Tags" AS "t" ON "g"."SquadId" = "t"."GearSquadId" AND "g"."Nickname" = "t"."GearNickName"
INNER JOIN "Cities" AS "c" ON "g"."CityOfBirthName" = "c"."Name"
""");
    }

    public override async Task Sum_with_optional_navigation_is_translated_to_sql(bool async)
    {
        await base.Sum_with_optional_navigation_is_translated_to_sql(async);

        AssertSql(
            """
SELECT COALESCE(SUM("g"."SquadId"), 0)
FROM "Gears" AS "g"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
WHERE "t"."Note" <> 'Foo' OR "t"."Note" IS NULL
""");
    }

    public override async Task ToString_string_property_projection(bool async)
    {
        await base.ToString_string_property_projection(async);

        AssertSql(
            """
SELECT "w"."Name"
FROM "Weapons" AS "w"
""");
    }

    public override async Task ToString_boolean_property_non_nullable(bool async)
    {
        await base.ToString_boolean_property_non_nullable(async);

        AssertSql(
            """
SELECT CASE
    WHEN NOT ("w"."IsAutomatic") THEN 'False'
    ELSE 'True'
END
FROM "Weapons" AS "w"
""");
    }

    public override async Task Select_subquery_projecting_single_constant_int(bool async)
    {
        await base.Select_subquery_projecting_single_constant_int(async);

        AssertSql(
            """
SELECT "s"."Name", COALESCE((
    SELECT 42
    FROM "Gears" AS "g"
    WHERE "s"."Id" = "g"."SquadId" AND "g"."HasSoulPatch"
    LIMIT 1), 0) AS "Gear"
FROM "Squads" AS "s"
""");
    }

    public override async Task Select_null_propagation_works_for_multiple_navigations_with_composite_keys(bool async)
    {
        await base.Select_null_propagation_works_for_multiple_navigations_with_composite_keys(async);

        AssertSql(
            """
SELECT CASE
    WHEN "c"."Name" IS NOT NULL THEN "c"."Name"
    ELSE NULL
END
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
LEFT JOIN "Tags" AS "t0" ON ("g"."Nickname" = "t0"."GearNickName" OR ("g"."Nickname" IS NULL AND "t0"."GearNickName" IS NULL)) AND ("g"."SquadId" = "t0"."GearSquadId" OR ("g"."SquadId" IS NULL AND "t0"."GearSquadId" IS NULL))
LEFT JOIN "Gears" AS "g0" ON "t0"."GearNickName" = "g0"."Nickname" AND "t0"."GearSquadId" = "g0"."SquadId"
LEFT JOIN "Cities" AS "c" ON "g0"."AssignedCityName" = "c"."Name"
""");
    }

    public override async Task Member_access_on_derived_entity_using_cast_and_let(bool async)
    {
        await base.Member_access_on_derived_entity_using_cast_and_let(async);

        AssertSql(
            """
SELECT "f"."Name", "f"."Eradicated"
FROM "Factions" AS "f"
ORDER BY "f"."Name"
""");
    }

    public override async Task Where_subquery_distinct_singleordefault_boolean1(bool async)
    {
        await base.Where_subquery_distinct_singleordefault_boolean1(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch" AND COALESCE((
    SELECT "w0"."IsAutomatic"
    FROM (
        SELECT DISTINCT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
        FROM "Weapons" AS "w"
        WHERE "g"."FullName" = "w"."OwnerFullName" AND "w"."Name" IS NOT NULL AND instr("w"."Name", 'Lancer') > 0
    ) AS "w0"
    LIMIT 1), 0)
ORDER BY "g"."Nickname"
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_negated_predicate(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_negated_predicate(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE ("t"."Note" <> 'K.I.A.' OR "t"."Note" IS NULL) AND NOT ("g"."HasSoulPatch")
""");
    }

    public override async Task Include_collection_on_derived_type_using_string(bool async)
    {
        await base.Include_collection_on_derived_type_using_string(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."Nickname" = "g0"."LeaderNickname" AND "g"."SquadId" = "g0"."LeaderSquadId"
ORDER BY "g"."Nickname", "g"."SquadId", "g0"."Nickname"
""");
    }

    public override async Task Select_subquery_distinct_singleordefault_boolean_empty1(bool async)
    {
        await base.Select_subquery_distinct_singleordefault_boolean_empty1(async);

        AssertSql(
            """
SELECT COALESCE((
    SELECT "w0"."IsAutomatic"
    FROM (
        SELECT DISTINCT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
        FROM "Weapons" AS "w"
        WHERE "g"."FullName" = "w"."OwnerFullName" AND "w"."Name" = 'BFG'
    ) AS "w0"
    LIMIT 1), 0)
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch"
""");
    }

    public override async Task Contains_on_byte_array_property_using_byte_column(bool async)
    {
        await base.Contains_on_byte_array_property_using_byte_column(async);

        AssertSql(
            """
SELECT "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s"."Name", "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId"
FROM "Squads" AS "s"
CROSS JOIN "LocustLeaders" AS "l"
WHERE instr("s"."Banner", char("l"."ThreatLevelByte")) > 0
""");
    }

    public override async Task Select_null_propagation_negative9(bool async)
    {
        await base.Select_null_propagation_negative9(async);

        AssertSql(
            """
SELECT CASE
    WHEN "g"."LeaderNickname" IS NOT NULL THEN COALESCE(length("g"."Nickname") = 5, 0)
    ELSE NULL
END
FROM "Gears" AS "g"
""");
    }

    public override async Task Filtered_collection_projection_with_order_comparison_predicate_converted_to_join2(bool async)
    {
        await base.Filtered_collection_projection_with_order_comparison_predicate_converted_to_join2(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName" AND "g"."SquadId" <= "w"."Id"
ORDER BY "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task Include_reference_on_derived_type_using_lambda_with_tracking(bool async)
    {
        await base.Include_reference_on_derived_type_using_lambda_with_tracking(async);

        AssertSql(
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "LocustLeaders" AS "l"
LEFT JOIN "Gears" AS "g" ON "l"."DefeatedByNickname" = "g"."Nickname" AND "l"."DefeatedBySquadId" = "g"."SquadId"
""");
    }

    public override async Task Null_propagation_optimization2(bool async)
    {
        await base.Null_propagation_optimization2(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE CASE
    WHEN "g"."LeaderNickname" IS NULL THEN NULL
    ELSE "g"."LeaderNickname" LIKE '%us' AND "g"."LeaderNickname" IS NOT NULL
END = 1
""");
    }

    public override async Task Join_on_entity_qsre_keys_inheritance(bool async)
    {
        await base.Join_on_entity_qsre_keys_inheritance(async);

        AssertSql(
            """
SELECT "g"."FullName" AS "GearName", "g1"."FullName" AS "OfficerName"
FROM "Gears" AS "g"
INNER JOIN (
    SELECT "g0"."Nickname", "g0"."SquadId", "g0"."FullName"
    FROM "Gears" AS "g0"
    WHERE "g0"."Discriminator" = 'Officer'
) AS "g1" ON "g"."Nickname" = "g1"."Nickname" AND "g"."SquadId" = "g1"."SquadId"
""");
    }

    public override async Task Project_collection_navigation_nested_with_take_composite_key(bool async)
    {
        await base.Project_collection_navigation_nested_with_take_composite_key(async);

        AssertSql(
            """
SELECT "t"."Id", "g"."Nickname", "g"."SquadId", "g2"."Nickname", "g2"."SquadId", "g2"."AssignedCityName", "g2"."CityOfBirthName", "g2"."Discriminator", "g2"."FullName", "g2"."HasSoulPatch", "g2"."LeaderNickname", "g2"."LeaderSquadId", "g2"."Rank"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
LEFT JOIN (
    SELECT "g1"."Nickname", "g1"."SquadId", "g1"."AssignedCityName", "g1"."CityOfBirthName", "g1"."Discriminator", "g1"."FullName", "g1"."HasSoulPatch", "g1"."LeaderNickname", "g1"."LeaderSquadId", "g1"."Rank"
    FROM (
        SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", ROW_NUMBER() OVER(PARTITION BY "g0"."LeaderNickname", "g0"."LeaderSquadId" ORDER BY "g0"."Nickname", "g0"."SquadId") AS "row"
        FROM "Gears" AS "g0"
    ) AS "g1"
    WHERE "g1"."row" <= 50
) AS "g2" ON ("g"."Nickname" = "g2"."LeaderNickname" OR ("g"."Nickname" IS NULL AND "g2"."LeaderNickname" IS NULL)) AND "g"."SquadId" = "g2"."LeaderSquadId"
WHERE "g"."Discriminator" = 'Officer'
ORDER BY "t"."Id", "g"."Nickname", "g"."SquadId", "g2"."Nickname"
""");
    }

    public override async Task GroupBy_Property_Include_Select_Count(bool async)
    {
        await base.GroupBy_Property_Include_Select_Count(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM "Gears" AS "g"
GROUP BY "g"."Rank"
""");
    }

    public override async Task GroupJoin_Composite_Key(bool async)
    {
        await base.GroupJoin_Composite_Key(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Tags" AS "t"
INNER JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
""");
    }

    public override async Task Correlated_collections_project_anonymous_collection_result(bool async)
    {
        await base.Correlated_collections_project_anonymous_collection_result(async);

        AssertSql(
            """
SELECT "s"."Name", "s"."Id", "g"."FullName", "g"."Rank", "g"."Nickname", "g"."SquadId"
FROM "Squads" AS "s"
LEFT JOIN "Gears" AS "g" ON "s"."Id" = "g"."SquadId"
WHERE "s"."Id" < 20
ORDER BY "s"."Id", "g"."Nickname"
""");
    }

    public override async Task Correlated_collections_naked_navigation_with_ToList(bool async)
    {
        await base.Correlated_collections_naked_navigation_with_ToList(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
WHERE "g"."Nickname" <> 'Marcus'
ORDER BY "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task
        Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation_complex(bool async)
    {
        await base.Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation_complex(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
LEFT JOIN "Squads" AS "s" ON "g"."SquadId" = "s"."Id"
WHERE substr("t"."Note", 0 + 1, length("s"."Name")) = "t"."GearNickName" OR (("t"."Note" IS NULL OR "s"."Name" IS NULL) AND "t"."GearNickName" IS NULL)
""");
    }

    public override async Task OrderBy_bool_coming_from_optional_navigation(bool async)
    {
        await base.OrderBy_bool_coming_from_optional_navigation(async);

        AssertSql(
            """
SELECT "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
FROM "Weapons" AS "w"
LEFT JOIN "Weapons" AS "w0" ON "w"."SynergyWithId" = "w0"."Id"
ORDER BY "w0"."IsAutomatic"
""");
    }

    public override async Task Select_subquery_boolean(bool async)
    {
        await base.Select_subquery_boolean(async);

        AssertSql(
            """
SELECT COALESCE((
    SELECT "w"."IsAutomatic"
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName"
    ORDER BY "w"."Id"
    LIMIT 1), 0)
FROM "Gears" AS "g"
""");
    }

    public override async Task Include_navigation_on_derived_type(bool async)
    {
        await base.Include_navigation_on_derived_type(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."Nickname" = "g0"."LeaderNickname" AND "g"."SquadId" = "g0"."LeaderSquadId"
WHERE "g"."Discriminator" = 'Officer'
ORDER BY "g"."Nickname", "g"."SquadId", "g0"."Nickname"
""");
    }

    public override async Task Correlated_collection_order_by_constant(bool async)
    {
        await base.Correlated_collection_order_by_constant(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "w"."Name", "w"."Id"
FROM "Gears" AS "g"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task Double_order_by_on_nullable_bool_coming_from_optional_navigation(bool async)
    {
        await base.Double_order_by_on_nullable_bool_coming_from_optional_navigation(async);

        AssertSql(
            """
SELECT "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
FROM "Weapons" AS "w"
LEFT JOIN "Weapons" AS "w0" ON "w"."SynergyWithId" = "w0"."Id"
ORDER BY "w0"."IsAutomatic", "w0"."Id"
""");
    }

    public override async Task Where_subquery_distinct_orderby_firstordefault_boolean(bool async)
    {
        await base.Where_subquery_distinct_orderby_firstordefault_boolean(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch" AND COALESCE((
    SELECT "w0"."IsAutomatic"
    FROM (
        SELECT DISTINCT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
        FROM "Weapons" AS "w"
        WHERE "g"."FullName" = "w"."OwnerFullName"
    ) AS "w0"
    ORDER BY "w0"."Id"
    LIMIT 1), 0)
""");
    }

    public override async Task Project_one_value_type_with_client_projection_from_empty_collection(bool async)
    {
        await base.Project_one_value_type_with_client_projection_from_empty_collection(async);

        AssertSql(
            """
SELECT "s"."Name", "g1"."SquadId", "g1"."LeaderSquadId", "g1"."c"
FROM "Squads" AS "s"
LEFT JOIN (
    SELECT "g0"."SquadId", "g0"."LeaderSquadId", "g0"."c"
    FROM (
        SELECT "g"."SquadId", "g"."LeaderSquadId", 1 AS "c", ROW_NUMBER() OVER(PARTITION BY "g"."SquadId" ORDER BY "g"."Nickname", "g"."SquadId") AS "row"
        FROM "Gears" AS "g"
        WHERE "g"."HasSoulPatch"
    ) AS "g0"
    WHERE "g0"."row" <= 1
) AS "g1" ON "s"."Id" = "g1"."SquadId"
WHERE "s"."Name" = 'Kilo'
""");
    }

    public override async Task Projecting_property_converted_to_nullable_into_element_init(bool async)
    {
        await base.Projecting_property_converted_to_nullable_into_element_init(async);

        AssertSql(
            """
SELECT CASE
    WHEN "t"."GearNickName" IS NOT NULL THEN length("g"."Nickname")
    ELSE NULL
END, CASE
    WHEN "t"."GearNickName" IS NOT NULL THEN "g"."SquadId"
    ELSE NULL
END, CASE
    WHEN "t"."GearNickName" IS NOT NULL THEN "g"."SquadId"
    ELSE NULL
END + 1
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE CASE
    WHEN "t"."GearNickName" IS NOT NULL THEN "g"."Nickname"
    ELSE NULL
END IS NOT NULL
ORDER BY "t"."Note"
""");
    }

    public override async Task Group_by_with_having_StartsWith_with_null_parameter_as_argument(bool async)
    {
        await base.Group_by_with_having_StartsWith_with_null_parameter_as_argument(async);

        AssertSql(
            """
SELECT "g"."FullName"
FROM "Gears" AS "g"
GROUP BY "g"."FullName"
HAVING 0
""");
    }

    public override async Task Skip_with_orderby_followed_by_orderBy_is_pushed_down(bool async)
    {
        await base.Skip_with_orderby_followed_by_orderBy_is_pushed_down(async);

        AssertSql(
            """
@__p_0='1'

SELECT "g0"."FullName"
FROM (
    SELECT "g"."FullName", "g"."Rank"
    FROM "Gears" AS "g"
    WHERE NOT ("g"."HasSoulPatch")
    ORDER BY "g"."FullName"
    LIMIT -1 OFFSET @__p_0
) AS "g0"
ORDER BY "g0"."Rank"
""");
    }

    public override async Task Concat_with_count(bool async)
    {
        await base.Concat_with_count(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM (
    SELECT 1
    FROM "Gears" AS "g"
    UNION ALL
    SELECT 1
    FROM "Gears" AS "g0"
) AS "u"
""");
    }

    public override async Task Join_on_entity_qsre_keys_inner_key_is_nested_navigation(bool async)
    {
        await base.Join_on_entity_qsre_keys_inner_key_is_nested_navigation(async);

        AssertSql(
            """
SELECT "s"."Name" AS "SquadName", "s1"."Name" AS "WeaponName"
FROM "Squads" AS "s"
INNER JOIN (
    SELECT "w"."Name", "s0"."Id" AS "Id0"
    FROM "Weapons" AS "w"
    LEFT JOIN "Gears" AS "g" ON "w"."OwnerFullName" = "g"."FullName"
    LEFT JOIN "Squads" AS "s0" ON "g"."SquadId" = "s0"."Id"
    WHERE "w"."IsAutomatic"
) AS "s1" ON "s"."Id" = "s1"."Id0"
""");
    }

    public override async Task Distinct_on_subquery_doesnt_get_lifted(bool async)
    {
        await base.Distinct_on_subquery_doesnt_get_lifted(async);

        AssertSql(
            """
SELECT "g0"."HasSoulPatch"
FROM (
    SELECT DISTINCT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
    FROM "Gears" AS "g"
) AS "g0"
""");
    }

    public override async Task Select_subquery_projecting_single_constant_string(bool async)
    {
        await base.Select_subquery_projecting_single_constant_string(async);

        AssertSql(
            """
SELECT "s"."Name", (
    SELECT 'Foo'
    FROM "Gears" AS "g"
    WHERE "s"."Id" = "g"."SquadId" AND "g"."HasSoulPatch"
    LIMIT 1) AS "Gear"
FROM "Squads" AS "s"
""");
    }

    public override async Task Where_null_parameter_is_not_null(bool async)
    {
        await base.Where_null_parameter_is_not_null(async);

        AssertSql(
            """
@__p_0='False'

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE @__p_0
""");
    }

    public override async Task Include_multiple_one_to_one_and_one_to_many(bool async)
    {
        await base.Include_multiple_one_to_one_and_one_to_many(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
ORDER BY "t"."Id", "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task Navigation_accessed_twice_outside_and_inside_subquery(bool async)
    {
        await base.Navigation_accessed_twice_outside_and_inside_subquery(async);

        AssertSql(
            """
SELECT "t"."Id"
FROM "Tags" AS "t"
""",
            //
            """
@__tags_0='["34C8D86E-A4AC-4BE5-827F-584DDA348A07","70534E05-782C-4052-8720-C2C54481CE5F","A7BE028A-0CF2-448F-AB55-CE8BC5D8CF69","A8AD98F9-E023-4E2A-9A70-C2728455BD34","B39A6FBA-9026-4D69-828E-FD7068673E57","DF36F493-463F-4123-83F9-6B135DEEB7BA"]' (Size = 235)

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
WHERE "t"."Id" IS NOT NULL AND "t"."Id" IN (
    SELECT "t0"."value"
    FROM json_each(@__tags_0) AS "t0"
)
""");
    }

    public override async Task Coalesce_operator_in_projection_with_other_conditions(bool async)
    {
        await base.Coalesce_operator_in_projection_with_other_conditions(async);

        AssertSql(
            """
SELECT "w"."AmmunitionType" = 1 AND "w"."AmmunitionType" IS NOT NULL AND COALESCE("w"."IsAutomatic", 0)
FROM "Weapons" AS "w"
""");
    }

    public override async Task Double_order_by_on_Like(bool async)
    {
        await base.Double_order_by_on_Like(async);

        AssertSql(
            """
SELECT "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
FROM "Weapons" AS "w"
LEFT JOIN "Weapons" AS "w0" ON "w"."SynergyWithId" = "w0"."Id"
ORDER BY "w0"."Name" LIKE '%Lancer' AND "w0"."Name" IS NOT NULL
""");
    }

    public override async Task Select_null_propagation_negative6(bool async)
    {
        await base.Select_null_propagation_negative6(async);

        AssertSql(
            """
SELECT CASE
    WHEN "g"."LeaderNickname" IS NOT NULL THEN length("g"."LeaderNickname") <> length("g"."LeaderNickname")
    ELSE NULL
END
FROM "Gears" AS "g"
""");
    }

    public override async Task Query_reusing_parameter_doesnt_declare_duplicate_parameter(bool async)
    {
        await base.Query_reusing_parameter_doesnt_declare_duplicate_parameter(async);

        AssertSql(
            """
@__prm_Inner_Nickname_0='Marcus' (Size = 6)

SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM (
    SELECT DISTINCT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
    FROM "Gears" AS "g"
    WHERE "g"."Nickname" <> @__prm_Inner_Nickname_0 AND "g"."Nickname" <> @__prm_Inner_Nickname_0
) AS "g0"
ORDER BY "g0"."FullName"
""");
    }

    public override async Task Basic_query_gears(bool async)
    {
        await base.Basic_query_gears(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
""");
    }

    public override async Task Groupby_anonymous_type_with_navigations_followed_up_by_anonymous_projection_and_orderby(bool async)
    {
        await base.Groupby_anonymous_type_with_navigations_followed_up_by_anonymous_projection_and_orderby(async);

        AssertSql(
            """
SELECT "c"."Name", "c"."Location", COUNT(*) AS "Count"
FROM "Weapons" AS "w"
LEFT JOIN "Gears" AS "g" ON "w"."OwnerFullName" = "g"."FullName"
LEFT JOIN "Cities" AS "c" ON "g"."CityOfBirthName" = "c"."Name"
GROUP BY "c"."Name", "c"."Location"
ORDER BY "c"."Location"
""");
    }

    public override async Task Bitwise_projects_values_in_select(bool async)
    {
        await base.Bitwise_projects_values_in_select(async);

        AssertSql(
            """
SELECT "g"."Rank" & 2 = 2 AS "BitwiseTrue", "g"."Rank" & 2 = 4 AS "BitwiseFalse", "g"."Rank" & 2 AS "BitwiseValue"
FROM "Gears" AS "g"
WHERE "g"."Rank" & 2 = 2
LIMIT 1
""");
    }

    public override async Task Cast_to_derived_followed_by_include_and_FirstOrDefault(bool async)
    {
        await base.Cast_to_derived_followed_by_include_and_FirstOrDefault(async);

        AssertSql(
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "LocustLeaders" AS "l"
LEFT JOIN "Gears" AS "g" ON "l"."DefeatedByNickname" = "g"."Nickname" AND "l"."DefeatedBySquadId" = "g"."SquadId"
WHERE instr("l"."Name", 'Queen') > 0
LIMIT 1
""");
    }

    public override async Task SelectMany_without_result_selector_and_non_equality_comparison_converted_to_join(bool async)
    {
        await base.SelectMany_without_result_selector_and_non_equality_comparison_converted_to_join(async);

        AssertSql(
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" <> "w"."OwnerFullName" OR "w"."OwnerFullName" IS NULL
""");
    }

    public override async Task Include_with_join_and_inheritance1(bool async)
    {
        await base.Include_with_join_and_inheritance1(async);

        AssertSql(
            """
SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "c"."Name", "c"."Location", "c"."Nation"
FROM "Tags" AS "t"
INNER JOIN (
    SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
    FROM "Gears" AS "g"
    WHERE "g"."Discriminator" = 'Officer'
) AS "g0" ON "t"."GearSquadId" = "g0"."SquadId" AND "t"."GearNickName" = "g0"."Nickname"
INNER JOIN "Cities" AS "c" ON "g0"."CityOfBirthName" = "c"."Name"
""");
    }

    public override async Task Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains(bool async)
    {
        await base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains(async);

        AssertSql(
            """
SELECT "c"."Name", "c"."Location", "c"."Nation"
FROM "Cities" AS "c"
WHERE "c"."Location" IS NOT NULL AND instr("c"."Location", 'Jacinto') > 0
""");
    }

    public override async Task Select_ternary_operation_multiple_conditions(bool async)
    {
        await base.Select_ternary_operation_multiple_conditions(async);

        AssertSql(
            """
SELECT "w"."Id", CASE
    WHEN "w"."AmmunitionType" = 2 AND "w"."SynergyWithId" = 1 THEN 'Yes'
    ELSE 'No'
END AS "IsCartridge"
FROM "Weapons" AS "w"
""");
    }

    public override async Task Where_compare_anonymous_types_with_uncorrelated_members(bool async)
    {
        await base.Where_compare_anonymous_types_with_uncorrelated_members(async);

        AssertSql(
            """
SELECT "g"."Nickname"
FROM "Gears" AS "g"
WHERE 0
""");
    }

    public override async Task Order_by_is_properly_lifted_from_subquery_with_same_order_by_in_the_outer_query(bool async)
    {
        await base.Order_by_is_properly_lifted_from_subquery_with_same_order_by_in_the_outer_query(async);

        AssertSql(
            """
SELECT "g"."FullName"
FROM "Gears" AS "g"
WHERE NOT ("g"."HasSoulPatch")
ORDER BY "g"."FullName"
""");
    }

    public override async Task Join_predicate_value_equals_condition(bool async)
    {
        await base.Join_predicate_value_equals_condition(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
INNER JOIN "Weapons" AS "w" ON "w"."SynergyWithId" IS NOT NULL
""");
    }

    public override async Task GetValueOrDefault_in_order_by(bool async)
    {
        await base.GetValueOrDefault_in_order_by(async);

        AssertSql(
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
ORDER BY COALESCE("w"."SynergyWithId", 0), "w"."Id"
""");
    }

    public override async Task Member_access_on_derived_entity_using_cast(bool async)
    {
        await base.Member_access_on_derived_entity_using_cast(async);

        AssertSql(
            """
SELECT "f"."Name", "f"."Eradicated"
FROM "Factions" AS "f"
ORDER BY "f"."Name"
""");
    }

    public override async Task Where_subquery_boolean(bool async)
    {
        await base.Where_subquery_boolean(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE COALESCE((
    SELECT "w"."IsAutomatic"
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName"
    ORDER BY "w"."Id"
    LIMIT 1), 0)
""");
    }

    public override async Task Nav_rewrite_with_convert3(bool async)
    {
        await base.Nav_rewrite_with_convert3(async);

        AssertSql(
            """
SELECT "f"."Id", "f"."CapitalName", "f"."Discriminator", "f"."Name", "f"."ServerAddress", "f"."CommanderName", "f"."Eradicated"
FROM "Factions" AS "f"
LEFT JOIN "Cities" AS "c" ON "f"."CapitalName" = "c"."Name"
LEFT JOIN (
    SELECT "l"."Name"
    FROM "LocustLeaders" AS "l"
    WHERE "l"."Discriminator" = 'LocustCommander'
) AS "l0" ON "f"."CommanderName" = "l0"."Name"
WHERE ("c"."Name" <> 'Foo' OR "c"."Name" IS NULL) AND ("l0"."Name" <> 'Bar' OR "l0"."Name" IS NULL)
""");
    }

    public override async Task Correlated_collections_with_funky_orderby_complex_scenario1(bool async)
    {
        await base.Correlated_collections_with_funky_orderby_complex_scenario1(async);

        AssertSql(
            """
SELECT "g"."FullName", "g"."Nickname", "g"."SquadId", "s0"."Id", "s0"."Nickname", "s0"."SquadId", "s0"."Id0", "s0"."Nickname0", "s0"."HasSoulPatch", "s0"."SquadId0"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "w"."Id", "g0"."Nickname", "g0"."SquadId", "s"."Id" AS "Id0", "g1"."Nickname" AS "Nickname0", "g1"."HasSoulPatch", "g1"."SquadId" AS "SquadId0", "w"."OwnerFullName"
    FROM "Weapons" AS "w"
    LEFT JOIN "Gears" AS "g0" ON "w"."OwnerFullName" = "g0"."FullName"
    LEFT JOIN "Squads" AS "s" ON "g0"."SquadId" = "s"."Id"
    LEFT JOIN "Gears" AS "g1" ON "s"."Id" = "g1"."SquadId"
) AS "s0" ON "g"."FullName" = "s0"."OwnerFullName"
ORDER BY "g"."FullName", "g"."Nickname" DESC, "g"."SquadId", "s0"."Id", "s0"."Nickname", "s0"."SquadId", "s0"."Id0", "s0"."Nickname0"
""");
    }

    public override async Task Where_conditional_equality_2(bool async)
    {
        await base.Where_conditional_equality_2(async);

        AssertSql(
            """
SELECT "g"."Nickname"
FROM "Gears" AS "g"
WHERE "g"."LeaderNickname" IS NULL
ORDER BY "g"."Nickname"
""");
    }

    public override async Task Correlated_collections_similar_collection_projected_multiple_times(bool async)
    {
        await base.Correlated_collections_similar_collection_projected_multiple_times(async);

        AssertSql(
            """
SELECT "g"."FullName", "g"."Nickname", "g"."SquadId", "w1"."Id", "w1"."AmmunitionType", "w1"."IsAutomatic", "w1"."Name", "w1"."OwnerFullName", "w1"."SynergyWithId", "w2"."Id", "w2"."AmmunitionType", "w2"."IsAutomatic", "w2"."Name", "w2"."OwnerFullName", "w2"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
    FROM "Weapons" AS "w"
    WHERE "w"."IsAutomatic"
) AS "w1" ON "g"."FullName" = "w1"."OwnerFullName"
LEFT JOIN (
    SELECT "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
    FROM "Weapons" AS "w0"
    WHERE NOT ("w0"."IsAutomatic")
) AS "w2" ON "g"."FullName" = "w2"."OwnerFullName"
ORDER BY "g"."Rank", "g"."Nickname", "g"."SquadId", "w1"."OwnerFullName", "w1"."Id", "w2"."IsAutomatic"
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_array_initializers(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_array_initializers(async);

        AssertSql(
            """
SELECT "g"."SquadId"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE "t"."Note" <> 'K.I.A.' OR "t"."Note" IS NULL
""");
    }

    public override async Task Bool_projection_from_subquery_treated_appropriately_in_where(bool async)
    {
        await base.Bool_projection_from_subquery_treated_appropriately_in_where(async);

        AssertSql(
            """
SELECT "c"."Name", "c"."Location", "c"."Nation"
FROM "Cities" AS "c"
WHERE (
    SELECT "g"."HasSoulPatch"
    FROM "Gears" AS "g"
    ORDER BY "g"."Nickname", "g"."SquadId"
    LIMIT 1)
""");
    }

    public override async Task Include_reference_on_derived_type_using_string(bool async)
    {
        await base.Include_reference_on_derived_type_using_string(async);

        AssertSql(
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "LocustLeaders" AS "l"
LEFT JOIN "Gears" AS "g" ON "l"."DefeatedByNickname" = "g"."Nickname" AND "l"."DefeatedBySquadId" = "g"."SquadId"
""");
    }

    public override async Task Select_subquery_distinct_firstordefault(bool async)
    {
        await base.Select_subquery_distinct_firstordefault(async);

        AssertSql(
            """
SELECT (
    SELECT "w0"."Name"
    FROM (
        SELECT DISTINCT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
        FROM "Weapons" AS "w"
        WHERE "g"."FullName" = "w"."OwnerFullName"
    ) AS "w0"
    ORDER BY "w0"."Id"
    LIMIT 1)
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch"
""");
    }

    public override async Task FirstOrDefault_on_empty_collection_of_DateTime_in_subquery(bool async)
    {
        await base.FirstOrDefault_on_empty_collection_of_DateTime_in_subquery(async);

        AssertSql(
            """
SELECT "g"."Nickname", COALESCE((
    SELECT "t1"."IssueDate"
    FROM "Tags" AS "t1"
    WHERE "t1"."GearNickName" = "g"."FullName"
    ORDER BY "t1"."Id"
    LIMIT 1), '0001-01-01 00:00:00') AS "invalidTagIssueDate"
FROM "Gears" AS "g"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
WHERE "t"."IssueDate" > COALESCE((
    SELECT "t0"."IssueDate"
    FROM "Tags" AS "t0"
    WHERE "t0"."GearNickName" = "g"."FullName"
    ORDER BY "t0"."Id"
    LIMIT 1), '0001-01-01 00:00:00')
""");
    }

    public override async Task Include_collection_on_derived_type_using_lambda_with_soft_cast(bool async)
    {
        await base.Include_collection_on_derived_type_using_lambda_with_soft_cast(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."Nickname" = "g0"."LeaderNickname" AND "g"."SquadId" = "g0"."LeaderSquadId"
ORDER BY "g"."Nickname", "g"."SquadId", "g0"."Nickname"
""");
    }

    public override async Task Where_nullable_enum_with_nullable_parameter(bool async)
    {
        await base.Where_nullable_enum_with_nullable_parameter(async);

        AssertSql(
            """
@__ammunitionType_0='1' (Nullable = true)

SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE "w"."AmmunitionType" = @__ammunitionType_0
""",
            //
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE "w"."AmmunitionType" IS NULL
""");
    }

    public override async Task Include_with_join_reference2(bool async)
    {
        await base.Include_with_join_reference2(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "c"."Name", "c"."Location", "c"."Nation"
FROM "Tags" AS "t"
INNER JOIN "Gears" AS "g" ON "t"."GearSquadId" = "g"."SquadId" AND "t"."GearNickName" = "g"."Nickname"
INNER JOIN "Cities" AS "c" ON "g"."CityOfBirthName" = "c"."Name"
""");
    }

    public override async Task FirstOrDefault_navigation_access_entity_equality_in_where_predicate_apply_peneding_selector(bool async)
    {
        await base.FirstOrDefault_navigation_access_entity_equality_in_where_predicate_apply_peneding_selector(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Cities" AS "c" ON "g"."AssignedCityName" = "c"."Name"
WHERE "c"."Name" = (
    SELECT "c0"."Name"
    FROM "Gears" AS "g0"
    INNER JOIN "Cities" AS "c0" ON "g0"."CityOfBirthName" = "c0"."Name"
    ORDER BY "g0"."Nickname"
    LIMIT 1) OR ("c"."Name" IS NULL AND (
    SELECT "c0"."Name"
    FROM "Gears" AS "g0"
    INNER JOIN "Cities" AS "c0" ON "g0"."CityOfBirthName" = "c0"."Name"
    ORDER BY "g0"."Nickname"
    LIMIT 1) IS NULL)
""");
    }

    public override async Task Nav_rewrite_with_convert2(bool async)
    {
        await base.Nav_rewrite_with_convert2(async);

        AssertSql(
            """
SELECT "f"."Id", "f"."CapitalName", "f"."Discriminator", "f"."Name", "f"."ServerAddress", "f"."CommanderName", "f"."Eradicated"
FROM "Factions" AS "f"
LEFT JOIN "Cities" AS "c" ON "f"."CapitalName" = "c"."Name"
LEFT JOIN (
    SELECT "l"."Name"
    FROM "LocustLeaders" AS "l"
    WHERE "l"."Discriminator" = 'LocustCommander'
) AS "l0" ON "f"."CommanderName" = "l0"."Name"
WHERE ("c"."Name" <> 'Foo' OR "c"."Name" IS NULL) AND ("l0"."Name" <> 'Bar' OR "l0"."Name" IS NULL)
""");
    }

    public override async Task Contains_on_collection_of_byte_subquery(bool async)
    {
        await base.Contains_on_collection_of_byte_subquery(async);

        AssertSql(
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId"
FROM "LocustLeaders" AS "l"
WHERE "l"."ThreatLevelByte" IN (
    SELECT "l0"."ThreatLevelByte"
    FROM "LocustLeaders" AS "l0"
)
""");
    }

    public override async Task Conditional_with_conditions_evaluating_to_true_gets_optimized(bool async)
    {
        await base.Conditional_with_conditions_evaluating_to_true_gets_optimized(async);

        AssertSql(
            """
SELECT "g"."CityOfBirthName"
FROM "Gears" AS "g"
""");
    }

    public override async Task Select_correlated_filtered_collection_works_with_caching(bool async)
    {
        await base.Select_correlated_filtered_collection_works_with_caching(async);

        AssertSql(
            """
SELECT "t"."Id", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname"
ORDER BY "t"."Note", "t"."Id", "g"."Nickname"
""");
    }

    public override async Task Complex_predicate_with_AndAlso_and_nullable_bool_property(bool async)
    {
        await base.Complex_predicate_with_AndAlso_and_nullable_bool_property(async);

        AssertSql(
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
LEFT JOIN "Gears" AS "g" ON "w"."OwnerFullName" = "g"."FullName"
WHERE "w"."Id" <> 50 AND NOT ("g"."HasSoulPatch")
""");
    }

    public override async Task SelectMany_predicate_after_navigation_with_non_equality_comparison_DefaultIfEmpty_converted_to_left_join(
        bool async)
    {
        await base.SelectMany_predicate_after_navigation_with_non_equality_comparison_DefaultIfEmpty_converted_to_left_join(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "s"."Id", "s"."AmmunitionType", "s"."IsAutomatic", "s"."Name", "s"."OwnerFullName", "s"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
    FROM "Weapons" AS "w"
    LEFT JOIN "Weapons" AS "w0" ON "w"."SynergyWithId" = "w0"."Id"
) AS "s" ON "g"."FullName" <> "s"."OwnerFullName" OR "s"."OwnerFullName" IS NULL
ORDER BY "g"."Nickname", "s"."Id"
""");
    }

    public override async Task Select_ternary_operation_with_boolean(bool async)
    {
        await base.Select_ternary_operation_with_boolean(async);

        AssertSql(
            """
SELECT "w"."Id", CASE
    WHEN "w"."IsAutomatic" THEN 1
    ELSE 0
END AS "Num"
FROM "Weapons" AS "w"
""");
    }

    public override async Task GroupBy_Property_Include_Select_Max(bool async)
    {
        await base.GroupBy_Property_Include_Select_Max(async);

        AssertSql(
            """
SELECT MAX("g"."SquadId")
FROM "Gears" AS "g"
GROUP BY "g"."Rank"
""");
    }

    public override async Task CompareTo_used_with_non_unicode_string_column_and_constant(bool async)
    {
        await base.CompareTo_used_with_non_unicode_string_column_and_constant(async);

        AssertSql(
            """
SELECT "c"."Name", "c"."Location", "c"."Nation"
FROM "Cities" AS "c"
WHERE "c"."Location" = 'Unknown'
""");
    }

    public override async Task Query_reusing_parameter_doesnt_declare_duplicate_parameter_complex(bool async)
    {
        await base.Query_reusing_parameter_doesnt_declare_duplicate_parameter_complex(async);

        AssertSql(
            """
@__entity_equality_prm_Inner_Squad_0_Id='1' (Nullable = true)

SELECT "s1"."Nickname", "s1"."SquadId", "s1"."AssignedCityName", "s1"."CityOfBirthName", "s1"."Discriminator", "s1"."FullName", "s1"."HasSoulPatch", "s1"."LeaderNickname", "s1"."LeaderSquadId", "s1"."Rank"
FROM (
    SELECT DISTINCT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
    FROM "Gears" AS "g"
    INNER JOIN "Squads" AS "s" ON "g"."SquadId" = "s"."Id"
    WHERE "s"."Id" = @__entity_equality_prm_Inner_Squad_0_Id
) AS "s1"
INNER JOIN "Squads" AS "s0" ON "s1"."SquadId" = "s0"."Id"
WHERE "s0"."Id" = @__entity_equality_prm_Inner_Squad_0_Id
ORDER BY "s1"."FullName"
""");
    }

    public override async Task Select_Where_Navigation_Null_Reverse(bool async)
    {
        await base.Select_Where_Navigation_Null_Reverse(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE "g"."Nickname" IS NULL OR "g"."SquadId" IS NULL
""");
    }

    public override async Task Collection_with_inheritance_and_join_include_joined(bool async)
    {
        await base.Collection_with_inheritance_and_join_include_joined(async);

        AssertSql(
            """
SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "t0"."Id", "t0"."GearNickName", "t0"."GearSquadId", "t0"."IssueDate", "t0"."Note"
FROM "Tags" AS "t"
INNER JOIN (
    SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
    FROM "Gears" AS "g"
    WHERE "g"."Discriminator" = 'Officer'
) AS "g0" ON "t"."GearSquadId" = "g0"."SquadId" AND "t"."GearNickName" = "g0"."Nickname"
LEFT JOIN "Tags" AS "t0" ON "g0"."Nickname" = "t0"."GearNickName" AND "g0"."SquadId" = "t0"."GearSquadId"
""");
    }

    public override async Task Where_enum_has_flag(bool async)
    {
        await base.Where_enum_has_flag(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."Rank" & 2 = 2
""",
            //
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."Rank" & 18 = 18
""",
            //
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."Rank" & 1 = 1
""",
            //
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."Rank" & 1 = 1
""",
            //
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE 2 & "g"."Rank" = "g"."Rank"
""");
    }

    public override async Task Where_bitwise_and_nullable_enum_with_constant(bool async)
    {
        await base.Where_bitwise_and_nullable_enum_with_constant(async);

        AssertSql(
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE "w"."AmmunitionType" & 1 > 0
""");
    }

    public override async Task Group_by_over_projection_with_multiple_properties_accessed_thru_navigation(bool async)
    {
        await base.Group_by_over_projection_with_multiple_properties_accessed_thru_navigation(async);

        AssertSql(
            """
SELECT "c"."Name"
FROM "Gears" AS "g"
INNER JOIN "Cities" AS "c" ON "g"."CityOfBirthName" = "c"."Name"
GROUP BY "c"."Name"
""");
    }

    public override async Task Correlated_collection_with_distinct_projecting_identifier_column_and_correlation_key(bool async)
    {
        await base.Correlated_collection_with_distinct_projecting_identifier_column_and_correlation_key(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "w0"."Id", "w0"."Name", "w0"."OwnerFullName"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT DISTINCT "w"."Id", "w"."Name", "w"."OwnerFullName"
    FROM "Weapons" AS "w"
) AS "w0" ON "g"."FullName" = "w0"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task GroupBy_Property_Include_Aggregate_with_anonymous_selector(bool async)
    {
        await base.GroupBy_Property_Include_Aggregate_with_anonymous_selector(async);

        AssertSql(
            """
SELECT "g"."Nickname" AS "Key", COUNT(*) AS "c"
FROM "Gears" AS "g"
GROUP BY "g"."Nickname"
ORDER BY "g"."Nickname"
""");
    }

    public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result(bool async)
    {
        await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result(async);

        AssertSql(
            """
SELECT "g0"."Nickname" IS NOT NULL AND "g0"."SquadId" IS NOT NULL, "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "g"."Nickname", "g"."SquadId", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."LeaderNickname" = "g0"."Nickname"
LEFT JOIN "Weapons" AS "w" ON "g0"."FullName" = "w"."OwnerFullName"
LEFT JOIN "Weapons" AS "w0" ON "g"."FullName" = "w0"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId", "g0"."Nickname", "g0"."SquadId", "w"."Id"
""");
    }

    public override async Task Where_subquery_distinct_orderby_firstordefault_boolean_with_pushdown(bool async)
    {
        await base.Where_subquery_distinct_orderby_firstordefault_boolean_with_pushdown(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch" AND (
    SELECT "w0"."IsAutomatic"
    FROM (
        SELECT DISTINCT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
        FROM "Weapons" AS "w"
        WHERE "g"."FullName" = "w"."OwnerFullName"
    ) AS "w0"
    ORDER BY "w0"."Id"
    LIMIT 1)
""");
    }

    public override async Task Select_navigation_with_concat_and_count(bool async)
    {
        await base.Select_navigation_with_concat_and_count(async);

        AssertSql(
            """
SELECT (
    SELECT COUNT(*)
    FROM (
        SELECT 1
        FROM "Weapons" AS "w"
        WHERE "g"."FullName" = "w"."OwnerFullName"
        UNION ALL
        SELECT 1
        FROM "Weapons" AS "w0"
        WHERE "g"."FullName" = "w0"."OwnerFullName"
    ) AS "u")
FROM "Gears" AS "g"
WHERE NOT ("g"."HasSoulPatch")
""");
    }

    public override async Task Projecting_property_converted_to_nullable_with_comparison(bool async)
    {
        await base.Projecting_property_converted_to_nullable_with_comparison(async);

        AssertSql(
            """
SELECT "t"."Note", "t"."GearNickName" IS NOT NULL, "g"."Nickname", "g"."SquadId", "g"."HasSoulPatch"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE CASE
    WHEN "t"."GearNickName" IS NOT NULL THEN "g"."SquadId"
    ELSE NULL
END = 1
""");
    }

    public override async Task TimeSpan_Hours(bool async)
    {
        await base.TimeSpan_Hours(async);

        AssertSql(
            """
SELECT "m"."Duration"
FROM "Missions" AS "m"
""");
    }

    public override async Task Where_bool_column_or_Contains(bool async)
    {
        await base.Where_bool_column_or_Contains(async);

        AssertSql(
            """
@__values_0='[false,true]' (Size = 12)

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch" AND "g"."HasSoulPatch" IN (
    SELECT "v"."value"
    FROM json_each(@__values_0) AS "v"
)
""");
    }

    public override async Task String_concat_with_null_conditional_argument2(bool async)
    {
        await base.String_concat_with_null_conditional_argument2(async);

        AssertSql(
            """
SELECT "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
FROM "Weapons" AS "w"
LEFT JOIN "Weapons" AS "w0" ON "w"."SynergyWithId" = "w0"."Id"
ORDER BY COALESCE("w0"."Name", '') || 'Marcus'' Lancer'
""");
    }

    public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3(bool async)
    {
        await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3(async);

        AssertSql(
            """
SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "g"."Nickname", "g"."SquadId", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."LeaderNickname" = "g0"."Nickname"
LEFT JOIN "Weapons" AS "w" ON "g0"."FullName" = "w"."OwnerFullName"
LEFT JOIN "Weapons" AS "w0" ON "g"."FullName" = "w0"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId", "g0"."Nickname", "g0"."SquadId", "w"."Id"
""");
    }

    public override async Task Coalesce_operator_in_predicate_with_other_conditions(bool async)
    {
        await base.Coalesce_operator_in_predicate_with_other_conditions(async);

        AssertSql(
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE "w"."AmmunitionType" = 1 AND COALESCE("w"."IsAutomatic", 0)
""");
    }

    public override async Task Include_reference_on_derived_type_using_string_nested2(bool async)
    {
        await base.Include_reference_on_derived_type_using_string_nested2(async);

        AssertSql(
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "s"."Nickname", "s"."SquadId", "s"."AssignedCityName", "s"."CityOfBirthName", "s"."Discriminator", "s"."FullName", "s"."HasSoulPatch", "s"."LeaderNickname", "s"."LeaderSquadId", "s"."Rank", "s"."Name", "s"."Location", "s"."Nation"
FROM "LocustLeaders" AS "l"
LEFT JOIN "Gears" AS "g" ON "l"."DefeatedByNickname" = "g"."Nickname" AND "l"."DefeatedBySquadId" = "g"."SquadId"
LEFT JOIN (
    SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "c"."Name", "c"."Location", "c"."Nation"
    FROM "Gears" AS "g0"
    INNER JOIN "Cities" AS "c" ON "g0"."CityOfBirthName" = "c"."Name"
) AS "s" ON ("g"."Nickname" = "s"."LeaderNickname" OR ("g"."Nickname" IS NULL AND "s"."LeaderNickname" IS NULL)) AND "g"."SquadId" = "s"."LeaderSquadId"
ORDER BY "l"."Name", "g"."Nickname", "g"."SquadId", "s"."Nickname", "s"."SquadId"
""");
    }

    public override async Task Select_coalesce_with_anonymous_types(bool async)
    {
        await base.Select_coalesce_with_anonymous_types(async);

        AssertSql(
            """
SELECT "g"."LeaderNickname", "g"."FullName"
FROM "Gears" AS "g"
ORDER BY "g"."Nickname"
""");
    }

    public override async Task Subquery_containing_SelectMany_projecting_main_from_clause_gets_lifted(bool async)
    {
        await base.Subquery_containing_SelectMany_projecting_main_from_clause_gets_lifted(async);

        AssertSql(
            """
SELECT "g"."FullName"
FROM "Gears" AS "g"
CROSS JOIN "Tags" AS "t"
WHERE "g"."HasSoulPatch"
ORDER BY "g"."FullName"
""");
    }

    public override async Task Correlated_collections_complex_scenario1(bool async)
    {
        await base.Correlated_collections_complex_scenario1(async);

        AssertSql(
            """
SELECT "g"."FullName", "g"."Nickname", "g"."SquadId", "s0"."Id", "s0"."Nickname", "s0"."SquadId", "s0"."Id0", "s0"."Nickname0", "s0"."HasSoulPatch", "s0"."SquadId0"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "w"."Id", "g0"."Nickname", "g0"."SquadId", "s"."Id" AS "Id0", "g1"."Nickname" AS "Nickname0", "g1"."HasSoulPatch", "g1"."SquadId" AS "SquadId0", "w"."OwnerFullName"
    FROM "Weapons" AS "w"
    LEFT JOIN "Gears" AS "g0" ON "w"."OwnerFullName" = "g0"."FullName"
    LEFT JOIN "Squads" AS "s" ON "g0"."SquadId" = "s"."Id"
    LEFT JOIN "Gears" AS "g1" ON "s"."Id" = "g1"."SquadId"
) AS "s0" ON "g"."FullName" = "s0"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId", "s0"."Id", "s0"."Nickname", "s0"."SquadId", "s0"."Id0", "s0"."Nickname0"
""");
    }

    public override async Task Order_by_entity_qsre(bool async)
    {
        await base.Order_by_entity_qsre(async);

        AssertSql(
            """
SELECT "g"."FullName"
FROM "Gears" AS "g"
LEFT JOIN "Cities" AS "c" ON "g"."AssignedCityName" = "c"."Name"
ORDER BY "c"."Name", "g"."Nickname" DESC
""");
    }

    public override async Task Reference_include_chain_loads_correctly_when_middle_is_null(bool async)
    {
        await base.Reference_include_chain_loads_correctly_when_middle_is_null(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s"."Name"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
LEFT JOIN "Squads" AS "s" ON "g"."SquadId" = "s"."Id"
ORDER BY "t"."Note"
""");
    }

    public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result4(bool async)
    {
        await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result4(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "g0"."Nickname", "g0"."SquadId", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId", "w1"."Id", "w1"."AmmunitionType", "w1"."IsAutomatic", "w1"."Name", "w1"."OwnerFullName", "w1"."SynergyWithId", "w2"."Id", "w2"."AmmunitionType", "w2"."IsAutomatic", "w2"."Name", "w2"."OwnerFullName", "w2"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."LeaderNickname" = "g0"."Nickname"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
LEFT JOIN "Weapons" AS "w0" ON "g0"."FullName" = "w0"."OwnerFullName"
LEFT JOIN "Weapons" AS "w1" ON "g0"."FullName" = "w1"."OwnerFullName"
LEFT JOIN "Weapons" AS "w2" ON "g"."FullName" = "w2"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId", "g0"."Nickname", "g0"."SquadId", "w"."Id", "w0"."Id", "w1"."Id"
""");
    }

    public override async Task Join_with_order_by_without_skip_or_take(bool async)
    {
        await base.Join_with_order_by_without_skip_or_take(async);

        AssertSql(
            """
SELECT "w"."Name", "g"."FullName"
FROM "Gears" AS "g"
INNER JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
""");
    }

    public override async Task Cast_subquery_to_base_type_using_typed_ToList(bool async)
    {
        await base.Cast_subquery_to_base_type_using_typed_ToList(async);

        AssertSql(
            """
SELECT "c"."Name", "g"."CityOfBirthName", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Nickname", "g"."Rank", "g"."SquadId"
FROM "Cities" AS "c"
LEFT JOIN "Gears" AS "g" ON "c"."Name" = "g"."AssignedCityName"
WHERE "c"."Name" = 'Ephyra'
ORDER BY "c"."Name", "g"."Nickname"
""");
    }

    public override async Task GroupBy_Property_Include_Select_Average(bool async)
    {
        await base.GroupBy_Property_Include_Select_Average(async);

        AssertSql(
            """
SELECT AVG(CAST("g"."SquadId" AS REAL))
FROM "Gears" AS "g"
GROUP BY "g"."Rank"
""");
    }

    public override async Task Select_null_parameter_is_not_null(bool async)
    {
        await base.Select_null_parameter_is_not_null(async);

        AssertSql(
            """
@__p_0='False'

SELECT @__p_0
FROM "Gears" AS "g"
""");
    }

    public override async Task Projecting_property_converted_to_nullable_with_function_call2(bool async)
    {
        await base.Projecting_property_converted_to_nullable_with_function_call2(async);

        AssertSql(
            """
SELECT "t"."Note", substr("t"."Note", 0 + 1, CASE
    WHEN "t"."GearNickName" IS NOT NULL THEN "g"."SquadId"
    ELSE NULL
END) AS "Function"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE CASE
    WHEN "t"."GearNickName" IS NOT NULL THEN "g"."Nickname"
    ELSE NULL
END IS NOT NULL
""");
    }

    public override async Task Select_subquery_distinct_singleordefault_boolean2(bool async)
    {
        await base.Select_subquery_distinct_singleordefault_boolean2(async);

        AssertSql(
            """
SELECT COALESCE((
    SELECT DISTINCT "w"."IsAutomatic"
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName" AND "w"."Name" IS NOT NULL AND instr("w"."Name", 'Lancer') > 0
    LIMIT 1), 0)
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch"
""");
    }

    public override async Task Select_null_propagation_optimization8(bool async)
    {
        await base.Select_null_propagation_optimization8(async);

        AssertSql(
            """
SELECT COALESCE("g"."LeaderNickname", '') || COALESCE("g"."LeaderNickname", '')
FROM "Gears" AS "g"
""");
    }

    public override async Task Where_required_navigation_on_derived_type(bool async)
    {
        await base.Where_required_navigation_on_derived_type(async);

        AssertSql(
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId"
FROM "LocustLeaders" AS "l"
LEFT JOIN "LocustHighCommands" AS "l0" ON "l"."HighCommandId" = "l0"."Id"
WHERE "l0"."IsOperational"
""");
    }

    public override async Task Correlated_collection_with_distinct_projecting_identifier_column_composite_key(bool async)
    {
        await base.Correlated_collection_with_distinct_projecting_identifier_column_composite_key(async);

        AssertSql(
            """
SELECT "s"."Id", "g0"."Nickname", "g0"."SquadId", "g0"."HasSoulPatch"
FROM "Squads" AS "s"
LEFT JOIN (
    SELECT DISTINCT "g"."Nickname", "g"."SquadId", "g"."HasSoulPatch"
    FROM "Gears" AS "g"
) AS "g0" ON "s"."Id" = "g0"."SquadId"
ORDER BY "s"."Id", "g0"."Nickname"
""");
    }

    public override async Task Singleton_Navigation_With_Member_Access(bool async)
    {
        await base.Singleton_Navigation_With_Member_Access(async);

        AssertSql(
            """
SELECT "g"."CityOfBirthName" AS "B"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE "g"."Nickname" = 'Marcus' AND ("g"."CityOfBirthName" <> 'Ephyra' OR "g"."CityOfBirthName" IS NULL)
""");
    }

    public override async Task GroupBy_Property_Include_Select_Sum(bool async)
    {
        await base.GroupBy_Property_Include_Select_Sum(async);

        AssertSql(
            """
SELECT COALESCE(SUM("g"."SquadId"), 0)
FROM "Gears" AS "g"
GROUP BY "g"."Rank"
""");
    }

    public override async Task Project_derivied_entity_with_convert_to_parent(bool async)
    {
        await base.Project_derivied_entity_with_convert_to_parent(async);

        AssertSql(
            """
SELECT "f"."Id", "f"."CapitalName", "f"."Discriminator", "f"."Name", "f"."ServerAddress", "f"."CommanderName", "f"."Eradicated"
FROM "Factions" AS "f"
""");
    }

    public override async Task Include_where_list_contains_navigation(bool async)
    {
        await base.Include_where_list_contains_navigation(async);

        AssertSql(
            """
SELECT "t"."Id"
FROM "Tags" AS "t"
""",
            //
            """
@__tags_0='["34C8D86E-A4AC-4BE5-827F-584DDA348A07","70534E05-782C-4052-8720-C2C54481CE5F","A7BE028A-0CF2-448F-AB55-CE8BC5D8CF69","A8AD98F9-E023-4E2A-9A70-C2728455BD34","B39A6FBA-9026-4D69-828E-FD7068673E57","DF36F493-463F-4123-83F9-6B135DEEB7BA"]' (Size = 235)

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Gears" AS "g"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
WHERE "t"."Id" IS NOT NULL AND "t"."Id" IN (
    SELECT "t0"."value"
    FROM json_each(@__tags_0) AS "t0"
)
""");
    }

    public override async Task Projecting_property_converted_to_nullable_into_new_array(bool async)
    {
        await base.Projecting_property_converted_to_nullable_into_new_array(async);

        AssertSql(
            """
SELECT CASE
    WHEN "t"."GearNickName" IS NOT NULL THEN length("g"."Nickname")
    ELSE NULL
END, CASE
    WHEN "t"."GearNickName" IS NOT NULL THEN "g"."SquadId"
    ELSE NULL
END, CASE
    WHEN "t"."GearNickName" IS NOT NULL THEN "g"."SquadId"
    ELSE NULL
END + 1
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE CASE
    WHEN "t"."GearNickName" IS NOT NULL THEN "g"."Nickname"
    ELSE NULL
END IS NOT NULL
ORDER BY "t"."Note"
""");
    }

    public override async Task Where_nullable_enum_with_constant(bool async)
    {
        await base.Where_nullable_enum_with_constant(async);

        AssertSql(
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE "w"."AmmunitionType" = 1
""");
    }

    public override async Task Project_navigation_defined_on_base_from_entity_with_inheritance_using_soft_cast(bool async)
    {
        await base.Project_navigation_defined_on_base_from_entity_with_inheritance_using_soft_cast(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note", "t"."Id" IS NULL AS "IsNull", "c"."Name", "c"."Location", "c"."Nation", "c"."Name" IS NULL AS "IsNull", "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s"."Name", "s"."Id" IS NULL AS "IsNull"
FROM "Gears" AS "g"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
LEFT JOIN "Cities" AS "c" ON "g"."CityOfBirthName" = "c"."Name"
LEFT JOIN "Squads" AS "s" ON "g"."SquadId" = "s"."Id"
""");
    }

    public override async Task Double_order_by_on_string_compare(bool async)
    {
        await base.Double_order_by_on_string_compare(async);

        AssertSql(
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
ORDER BY "w"."Name" = 'Marcus'' Lancer' AND "w"."Name" IS NOT NULL, "w"."Id"
""");
    }

    public override async Task Take_without_orderby_followed_by_orderBy_is_pushed_down3(bool async)
    {
        await base.Take_without_orderby_followed_by_orderBy_is_pushed_down3(async);

        AssertSql(
            """
@__p_0='999'

SELECT "g0"."FullName"
FROM (
    SELECT "g"."FullName", "g"."Rank"
    FROM "Gears" AS "g"
    WHERE NOT ("g"."HasSoulPatch")
    LIMIT @__p_0
) AS "g0"
ORDER BY "g0"."FullName", "g0"."Rank"
""");
    }

    public override async Task GroupJoin_on_entity_qsre_keys_inner_key_is_nested_navigation(bool async)
    {
        await base.GroupJoin_on_entity_qsre_keys_inner_key_is_nested_navigation(async);

        AssertSql(
            """
SELECT "s"."Name" AS "SquadName", "s1"."Name" AS "WeaponName"
FROM "Squads" AS "s"
LEFT JOIN (
    SELECT "w"."Name", "s0"."Id" AS "Id0"
    FROM "Weapons" AS "w"
    LEFT JOIN "Gears" AS "g" ON "w"."OwnerFullName" = "g"."FullName"
    LEFT JOIN "Squads" AS "s0" ON "g"."SquadId" = "s0"."Id"
) AS "s1" ON "s"."Id" = "s1"."Id0"
""");
    }

    public override async Task Where_enum_has_flag_with_non_nullable_parameter(bool async)
    {
        await base.Where_enum_has_flag_with_non_nullable_parameter(async);

        AssertSql(
            """
@__parameter_0='2'

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."Rank" & @__parameter_0 = @__parameter_0
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_binary_and_expression(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_binary_and_expression(async);

        AssertSql(
            """
SELECT "g"."HasSoulPatch" AND "t"."Note" IS NOT NULL AND instr("t"."Note", 'Cole') > 0
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
""");
    }

    public override async Task Query_reusing_parameter_with_inner_query_expression_doesnt_declare_duplicate_parameter(bool async)
    {
        await base.Query_reusing_parameter_with_inner_query_expression_doesnt_declare_duplicate_parameter(async);

        AssertSql(
            """
@__gearId_0='1'

SELECT "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s"."Name"
FROM "Squads" AS "s"
WHERE EXISTS (
    SELECT 1
    FROM "Gears" AS "g"
    WHERE "s"."Id" = "g"."SquadId" AND "g"."SquadId" = @__gearId_0 AND "g"."SquadId" = @__gearId_0)
""");
    }

    public override async Task Where_enum_has_flag_subquery(bool async)
    {
        await base.Where_enum_has_flag_subquery(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."Rank" & COALESCE((
    SELECT "g0"."Rank"
    FROM "Gears" AS "g0"
    ORDER BY "g0"."Nickname", "g0"."SquadId"
    LIMIT 1), 0) = COALESCE((
    SELECT "g0"."Rank"
    FROM "Gears" AS "g0"
    ORDER BY "g0"."Nickname", "g0"."SquadId"
    LIMIT 1), 0)
""",
            //
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE 2 & COALESCE((
    SELECT "g0"."Rank"
    FROM "Gears" AS "g0"
    ORDER BY "g0"."Nickname", "g0"."SquadId"
    LIMIT 1), 0) = COALESCE((
    SELECT "g0"."Rank"
    FROM "Gears" AS "g0"
    ORDER BY "g0"."Nickname", "g0"."SquadId"
    LIMIT 1), 0)
""");
    }

    public override async Task Select_correlated_filtered_collection_with_composite_key(bool async)
    {
        await base.Select_correlated_filtered_collection_with_composite_key(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g1"."Nickname", "g1"."SquadId", "g1"."AssignedCityName", "g1"."CityOfBirthName", "g1"."Discriminator", "g1"."FullName", "g1"."HasSoulPatch", "g1"."LeaderNickname", "g1"."LeaderSquadId", "g1"."Rank"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
    FROM "Gears" AS "g0"
    WHERE "g0"."Nickname" <> 'Dom'
) AS "g1" ON "g"."Nickname" = "g1"."LeaderNickname" AND "g"."SquadId" = "g1"."LeaderSquadId"
WHERE "g"."Discriminator" = 'Officer'
ORDER BY "g"."Nickname", "g"."SquadId", "g1"."Nickname"
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_projection_into_anonymous_type(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_projection_into_anonymous_type(async);

        AssertSql(
            """
SELECT "g"."SquadId"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE "t"."Note" <> 'K.I.A.' OR "t"."Note" IS NULL
""");
    }

    public override async Task Navigation_access_on_derived_materialized_entity_using_cast(bool async)
    {
        await base.Navigation_access_on_derived_materialized_entity_using_cast(async);

        AssertSql(
            """
SELECT "f"."Id", "f"."CapitalName", "f"."Discriminator", "f"."Name", "f"."ServerAddress", "f"."CommanderName", "f"."Eradicated", "l0"."ThreatLevel" AS "Threat"
FROM "Factions" AS "f"
LEFT JOIN (
    SELECT "l"."Name", "l"."ThreatLevel"
    FROM "LocustLeaders" AS "l"
    WHERE "l"."Discriminator" = 'LocustCommander'
) AS "l0" ON "f"."CommanderName" = "l0"."Name"
ORDER BY "f"."Name"
""");
    }

    public override async Task ThenInclude_collection_on_derived_after_derived_collection(bool async)
    {
        await base.ThenInclude_collection_on_derived_after_derived_collection(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "s"."Nickname", "s"."SquadId", "s"."AssignedCityName", "s"."CityOfBirthName", "s"."Discriminator", "s"."FullName", "s"."HasSoulPatch", "s"."LeaderNickname", "s"."LeaderSquadId", "s"."Rank", "s"."Nickname0", "s"."SquadId0", "s"."AssignedCityName0", "s"."CityOfBirthName0", "s"."Discriminator0", "s"."FullName0", "s"."HasSoulPatch0", "s"."LeaderNickname0", "s"."LeaderSquadId0", "s"."Rank0"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "g1"."Nickname" AS "Nickname0", "g1"."SquadId" AS "SquadId0", "g1"."AssignedCityName" AS "AssignedCityName0", "g1"."CityOfBirthName" AS "CityOfBirthName0", "g1"."Discriminator" AS "Discriminator0", "g1"."FullName" AS "FullName0", "g1"."HasSoulPatch" AS "HasSoulPatch0", "g1"."LeaderNickname" AS "LeaderNickname0", "g1"."LeaderSquadId" AS "LeaderSquadId0", "g1"."Rank" AS "Rank0"
    FROM "Gears" AS "g0"
    LEFT JOIN "Gears" AS "g1" ON "g0"."Nickname" = "g1"."LeaderNickname" AND "g0"."SquadId" = "g1"."LeaderSquadId"
) AS "s" ON "g"."Nickname" = "s"."LeaderNickname" AND "g"."SquadId" = "s"."LeaderSquadId"
ORDER BY "g"."Nickname", "g"."SquadId", "s"."Nickname", "s"."SquadId", "s"."Nickname0"
""");
    }

    public override async Task DateTimeOffset_DateAdd_AddYears(bool async)
    {
        await base.DateTimeOffset_DateAdd_AddYears(async);

        AssertSql(
            """
SELECT "m"."Timeline"
FROM "Missions" AS "m"
""");
    }

    public override async Task Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation(
        bool async)
    {
        await base.Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE substr("t"."Note", 0 + 1, "g"."SquadId") = "t"."GearNickName" OR (("t"."Note" IS NULL OR "g"."SquadId" IS NULL) AND "t"."GearNickName" IS NULL)
""");
    }

    public override async Task Checked_context_with_cast_does_not_fail(bool async)
    {
        await base.Checked_context_with_cast_does_not_fail(async);

        AssertSql(
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId"
FROM "LocustLeaders" AS "l"
WHERE CAST("l"."ThreatLevel" AS INTEGER) >= 5
""");
    }

    public override async Task Concat_anonymous_with_count(bool async)
    {
        await base.Concat_anonymous_with_count(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM (
    SELECT 1
    FROM "Gears" AS "g"
    UNION ALL
    SELECT 1
    FROM "Gears" AS "g0"
) AS "u"
""");
    }

    public override async Task Select_null_propagation_negative3(bool async)
    {
        await base.Select_null_propagation_negative3(async);

        AssertSql(
            """
SELECT "g0"."Nickname", CASE
    WHEN "g0"."Nickname" IS NOT NULL AND "g0"."SquadId" IS NOT NULL THEN "g0"."LeaderNickname" IS NOT NULL
    ELSE NULL
END AS "Condition"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."HasSoulPatch"
ORDER BY "g0"."Nickname"
""");
    }

    public override async Task Correlated_collections_nested_mixed_streaming_with_buffer2(bool async)
    {
        await base.Correlated_collections_nested_mixed_streaming_with_buffer2(async);

        AssertSql(
            """
SELECT "s"."Id", "s3"."SquadId", "s3"."MissionId", "s3"."Id", "s3"."SquadId0", "s3"."MissionId0"
FROM "Squads" AS "s"
LEFT JOIN (
    SELECT "s0"."SquadId", "s0"."MissionId", "m"."Id", "s2"."SquadId" AS "SquadId0", "s2"."MissionId" AS "MissionId0"
    FROM "SquadMissions" AS "s0"
    INNER JOIN "Missions" AS "m" ON "s0"."MissionId" = "m"."Id"
    LEFT JOIN (
        SELECT "s1"."SquadId", "s1"."MissionId"
        FROM "SquadMissions" AS "s1"
        WHERE "s1"."SquadId" < 7
    ) AS "s2" ON "m"."Id" = "s2"."MissionId"
    WHERE "s0"."MissionId" < 42
) AS "s3" ON "s"."Id" = "s3"."SquadId"
ORDER BY "s"."Id", "s3"."SquadId", "s3"."MissionId", "s3"."Id", "s3"."SquadId0"
""");
    }

    public override async Task Select_subquery_boolean_empty(bool async)
    {
        await base.Select_subquery_boolean_empty(async);

        AssertSql(
            """
SELECT COALESCE((
    SELECT "w"."IsAutomatic"
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName" AND "w"."Name" = 'BFG'
    ORDER BY "w"."Id"
    LIMIT 1), 0)
FROM "Gears" AS "g"
""");
    }

    public override async Task Join_with_inner_being_a_subquery_projecting_single_property(bool async)
    {
        await base.Join_with_inner_being_a_subquery_projecting_single_property(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
INNER JOIN "Gears" AS "g0" ON "g"."Nickname" = "g0"."Nickname"
""");
    }

    public override async Task Contains_on_collection_of_nullable_byte_subquery_null_constant(bool async)
    {
        await base.Contains_on_collection_of_nullable_byte_subquery_null_constant(async);

        AssertSql(
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId"
FROM "LocustLeaders" AS "l"
WHERE EXISTS (
    SELECT 1
    FROM "LocustLeaders" AS "l0"
    WHERE "l0"."ThreatLevelNullableByte" IS NULL)
""");
    }

    public override async Task SelectMany_predicate_with_non_equality_comparison_converted_to_inner_join(bool async)
    {
        await base.SelectMany_predicate_with_non_equality_comparison_converted_to_inner_join(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Gears" AS "g"
INNER JOIN "Weapons" AS "w" ON "g"."FullName" <> "w"."OwnerFullName" OR "w"."OwnerFullName" IS NULL
ORDER BY "g"."Nickname", "w"."Id"
""");
    }

    public override async Task String_based_Include_navigation_on_derived_type(bool async)
    {
        await base.String_based_Include_navigation_on_derived_type(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."Nickname" = "g0"."LeaderNickname" AND "g"."SquadId" = "g0"."LeaderSquadId"
WHERE "g"."Discriminator" = 'Officer'
ORDER BY "g"."Nickname", "g"."SquadId", "g0"."Nickname"
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_predicate(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_predicate(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE ("t"."Note" <> 'K.I.A.' OR "t"."Note" IS NULL) AND "g"."HasSoulPatch"
""");
    }

    public override async Task Where_subquery_distinct_last_boolean(bool async)
    {
        await base.Where_subquery_distinct_last_boolean(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE NOT ("g"."HasSoulPatch") AND (
    SELECT "w0"."IsAutomatic"
    FROM (
        SELECT DISTINCT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
        FROM "Weapons" AS "w"
        WHERE "g"."FullName" = "w"."OwnerFullName"
    ) AS "w0"
    ORDER BY "w0"."Id" DESC
    LIMIT 1)
ORDER BY "g"."Nickname"
""");
    }

    public override async Task Correlated_collection_with_top_level_Last_with_orderby_on_outer(bool async)
    {
        await base.Correlated_collection_with_top_level_Last_with_orderby_on_outer(async);

        AssertSql(
            """
SELECT "g0"."Nickname", "g0"."SquadId", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM (
    SELECT "g"."Nickname", "g"."SquadId", "g"."FullName"
    FROM "Gears" AS "g"
    ORDER BY "g"."FullName"
    LIMIT 1
) AS "g0"
LEFT JOIN "Weapons" AS "w" ON "g0"."FullName" = "w"."OwnerFullName"
ORDER BY "g0"."FullName", "g0"."Nickname", "g0"."SquadId"
""");
    }

    public override async Task Where_enum_has_flag_subquery_with_pushdown(bool async)
    {
        await base.Where_enum_has_flag_subquery_with_pushdown(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."Rank" & (
    SELECT "g0"."Rank"
    FROM "Gears" AS "g0"
    ORDER BY "g0"."Nickname", "g0"."SquadId"
    LIMIT 1) = (
    SELECT "g0"."Rank"
    FROM "Gears" AS "g0"
    ORDER BY "g0"."Nickname", "g0"."SquadId"
    LIMIT 1) OR (
    SELECT "g0"."Rank"
    FROM "Gears" AS "g0"
    ORDER BY "g0"."Nickname", "g0"."SquadId"
    LIMIT 1) IS NULL
""",
            //
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE 2 & (
    SELECT "g0"."Rank"
    FROM "Gears" AS "g0"
    ORDER BY "g0"."Nickname", "g0"."SquadId"
    LIMIT 1) = (
    SELECT "g0"."Rank"
    FROM "Gears" AS "g0"
    ORDER BY "g0"."Nickname", "g0"."SquadId"
    LIMIT 1) OR (
    SELECT "g0"."Rank"
    FROM "Gears" AS "g0"
    ORDER BY "g0"."Nickname", "g0"."SquadId"
    LIMIT 1) IS NULL
""");
    }

    public override async Task Null_semantics_on_nullable_bool_from_left_join_subquery_is_fully_applied(bool async)
    {
        await base.Null_semantics_on_nullable_bool_from_left_join_subquery_is_fully_applied(async);

        AssertSql(
            """
SELECT "f0"."Id", "f0"."CapitalName", "f0"."Discriminator", "f0"."Name", "f0"."ServerAddress", "f0"."CommanderName", "f0"."Eradicated"
FROM "LocustLeaders" AS "l"
LEFT JOIN (
    SELECT "f"."Id", "f"."CapitalName", "f"."Discriminator", "f"."Name", "f"."ServerAddress", "f"."CommanderName", "f"."Eradicated"
    FROM "Factions" AS "f"
    WHERE "f"."Name" = 'Swarm'
) AS "f0" ON "l"."Name" = "f0"."CommanderName"
WHERE "f0"."Eradicated" <> 1 OR "f0"."Eradicated" IS NULL
""");
    }

    public override async Task Double_order_by_binary_expression(bool async)
    {
        await base.Double_order_by_binary_expression(async);

        AssertSql(
            """
SELECT "w"."Id" + 2 AS "Binary"
FROM "Weapons" AS "w"
ORDER BY "w"."Id" + 2
""");
    }

    public override async Task Select_length_of_string_property(bool async)
    {
        await base.Select_length_of_string_property(async);

        AssertSql(
            """
SELECT "w"."Name", length("w"."Name") AS "Length"
FROM "Weapons" AS "w"
""");
    }

    public override async Task Include_with_nested_navigation_in_order_by(bool async)
    {
        await base.Include_with_nested_navigation_in_order_by(async);

        AssertSql(
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Weapons" AS "w"
LEFT JOIN "Gears" AS "g" ON "w"."OwnerFullName" = "g"."FullName"
LEFT JOIN "Cities" AS "c" ON "g"."CityOfBirthName" = "c"."Name"
WHERE "g"."Nickname" <> 'Paduk' OR "g"."Nickname" IS NULL
ORDER BY "c"."Name", "w"."Id"
""");
    }

    public override async Task Composite_key_entity_not_equal(bool async)
    {
        await base.Composite_key_entity_not_equal(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM "Gears" AS "g"
CROSS JOIN "Gears" AS "g0"
WHERE "g"."Nickname" <> "g0"."Nickname" OR "g"."SquadId" <> "g0"."SquadId"
""");
    }

    public override async Task Null_semantics_on_nullable_bool_from_inner_join_subquery_is_fully_applied(bool async)
    {
        await base.Null_semantics_on_nullable_bool_from_inner_join_subquery_is_fully_applied(async);

        AssertSql(
            """
SELECT "f0"."Id", "f0"."CapitalName", "f0"."Discriminator", "f0"."Name", "f0"."ServerAddress", "f0"."CommanderName", "f0"."Eradicated"
FROM "LocustLeaders" AS "l"
INNER JOIN (
    SELECT "f"."Id", "f"."CapitalName", "f"."Discriminator", "f"."Name", "f"."ServerAddress", "f"."CommanderName", "f"."Eradicated"
    FROM "Factions" AS "f"
    WHERE "f"."Name" = 'Swarm'
) AS "f0" ON "l"."Name" = "f0"."CommanderName"
WHERE "f0"."Eradicated" <> 1 OR "f0"."Eradicated" IS NULL
""");
    }

    public override async Task Where_subquery_join_firstordefault_boolean(bool async)
    {
        await base.Where_subquery_join_firstordefault_boolean(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch" AND (
    SELECT "w"."IsAutomatic"
    FROM "Weapons" AS "w"
    INNER JOIN (
        SELECT "w0"."Id"
        FROM "Weapons" AS "w0"
        WHERE "g"."FullName" = "w0"."OwnerFullName"
    ) AS "w1" ON "w"."Id" = "w1"."Id"
    WHERE "g"."FullName" = "w"."OwnerFullName"
    ORDER BY "w"."Id"
    LIMIT 1)
""");
    }

    public override async Task Where_any_subquery_without_collision(bool async)
    {
        await base.Where_any_subquery_without_collision(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE EXISTS (
    SELECT 1
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName")
""");
    }

    public override async Task GetValueOrDefault_in_filter(bool async)
    {
        await base.GetValueOrDefault_in_filter(async);

        AssertSql(
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE COALESCE("w"."SynergyWithId", 0) = 0
""");
    }

    public override async Task Select_subquery_int_with_outside_cast_and_coalesce(bool async)
    {
        await base.Select_subquery_int_with_outside_cast_and_coalesce(async);

        AssertSql(
            """
SELECT COALESCE((
    SELECT "w"."Id"
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName"
    ORDER BY "w"."Id"
    LIMIT 1), 0, 42)
FROM "Gears" AS "g"
""");
    }

    public override async Task Select_null_propagation_negative8(bool async)
    {
        await base.Select_null_propagation_negative8(async);

        AssertSql(
            """
SELECT CASE
    WHEN "s"."Id" IS NOT NULL THEN "c"."Name"
    ELSE NULL
END
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
LEFT JOIN "Squads" AS "s" ON "g"."SquadId" = "s"."Id"
LEFT JOIN "Cities" AS "c" ON "g"."AssignedCityName" = "c"."Name"
""");
    }

    public override async Task Left_join_projection_using_coalesce_tracking(bool async)
    {
        await base.Left_join_projection_using_coalesce_tracking(async);

        AssertSql(
            """
SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."LeaderNickname" = "g0"."Nickname"
""");
    }

    public override async Task String_compare_with_null_conditional_argument(bool async)
    {
        await base.String_compare_with_null_conditional_argument(async);

        AssertSql(
            """
SELECT "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
FROM "Weapons" AS "w"
LEFT JOIN "Weapons" AS "w0" ON "w"."SynergyWithId" = "w0"."Id"
ORDER BY "w0"."Name" = 'Marcus'' Lancer' AND "w0"."Name" IS NOT NULL
""");
    }

    public override async Task Project_one_value_type_from_empty_collection(bool async)
    {
        await base.Project_one_value_type_from_empty_collection(async);

        AssertSql(
            """
SELECT "s"."Name", COALESCE((
    SELECT "g"."SquadId"
    FROM "Gears" AS "g"
    WHERE "s"."Id" = "g"."SquadId" AND "g"."HasSoulPatch"
    LIMIT 1), 0) AS "SquadId"
FROM "Squads" AS "s"
WHERE "s"."Name" = 'Kilo'
""");
    }

    public override async Task Correlated_collection_with_top_level_FirstOrDefault(bool async)
    {
        await base.Correlated_collection_with_top_level_FirstOrDefault(async);

        AssertSql(
            """
SELECT "g0"."Nickname", "g0"."SquadId", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM (
    SELECT "g"."Nickname", "g"."SquadId", "g"."FullName"
    FROM "Gears" AS "g"
    ORDER BY "g"."Nickname"
    LIMIT 1
) AS "g0"
LEFT JOIN "Weapons" AS "w" ON "g0"."FullName" = "w"."OwnerFullName"
ORDER BY "g0"."Nickname", "g0"."SquadId"
""");
    }

    public override async Task Project_collection_navigation_nested_composite_key(bool async)
    {
        await base.Project_collection_navigation_nested_composite_key(async);

        AssertSql(
            """
SELECT "t"."Id", "g"."Nickname", "g"."SquadId", "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
LEFT JOIN "Gears" AS "g0" ON ("g"."Nickname" = "g0"."LeaderNickname" OR ("g"."Nickname" IS NULL AND "g0"."LeaderNickname" IS NULL)) AND "g"."SquadId" = "g0"."LeaderSquadId"
WHERE "g"."Discriminator" = 'Officer'
ORDER BY "t"."Id", "g"."Nickname", "g"."SquadId", "g0"."Nickname"
""");
    }

    public override async Task OrderBy_Contains_empty_list(bool async)
    {
        await base.OrderBy_Contains_empty_list(async);

        AssertSql(
            """
@__ids_0='[]' (Size = 2)

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
ORDER BY "g"."SquadId" IN (
    SELECT "i"."value"
    FROM json_each(@__ids_0) AS "i"
)
""");
    }

    public override async Task Null_checks_in_correlated_predicate_are_correctly_translated(bool async)
    {
        await base.Null_checks_in_correlated_predicate_are_correctly_translated(async);

        AssertSql(
            """
SELECT "t"."Id", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId" AND "t"."Note" IS NOT NULL
ORDER BY "t"."Id", "g"."Nickname"
""");
    }

    public override async Task Project_navigation_defined_on_derived_from_entity_with_inheritance_using_soft_cast(bool async)
    {
        await base.Project_navigation_defined_on_derived_from_entity_with_inheritance_using_soft_cast(async);

        AssertSql(
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "g"."Nickname" IS NULL OR "g"."SquadId" IS NULL AS "IsNull", "f"."Id", "f"."CapitalName", "f"."Discriminator", "f"."Name", "f"."ServerAddress", "f"."CommanderName", "f"."Eradicated", "f"."Id" IS NULL AS "IsNull", "l0"."Id", "l0"."IsOperational", "l0"."Name", "l0"."Id" IS NULL AS "IsNull"
FROM "LocustLeaders" AS "l"
LEFT JOIN "Gears" AS "g" ON "l"."DefeatedByNickname" = "g"."Nickname" AND "l"."DefeatedBySquadId" = "g"."SquadId"
LEFT JOIN "Factions" AS "f" ON "l"."Name" = "f"."CommanderName"
LEFT JOIN "LocustHighCommands" AS "l0" ON "l"."HighCommandId" = "l0"."Id"
""");
    }

    public override async Task Correlated_collections_with_FirstOrDefault(bool async)
    {
        await base.Correlated_collections_with_FirstOrDefault(async);

        AssertSql(
            """
SELECT (
    SELECT "g"."FullName"
    FROM "Gears" AS "g"
    WHERE "s"."Id" = "g"."SquadId"
    ORDER BY "g"."Nickname"
    LIMIT 1)
FROM "Squads" AS "s"
ORDER BY "s"."Name"
""");
    }

    public override async Task Distinct_with_optional_navigation_is_translated_to_sql(bool async)
    {
        await base.Distinct_with_optional_navigation_is_translated_to_sql(async);

        AssertSql(
            """
SELECT DISTINCT "g"."HasSoulPatch"
FROM "Gears" AS "g"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
WHERE "t"."Note" <> 'Foo' OR "t"."Note" IS NULL
""");
    }

    public override async Task Composite_key_entity_equal(bool async)
    {
        await base.Composite_key_entity_equal(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM "Gears" AS "g"
CROSS JOIN "Gears" AS "g0"
WHERE "g"."Nickname" = "g0"."Nickname" AND "g"."SquadId" = "g0"."SquadId"
""");
    }

    public override async Task Select_null_propagation_optimization7(bool async)
    {
        await base.Select_null_propagation_optimization7(async);

        AssertSql(
            """
SELECT CASE
    WHEN "g"."LeaderNickname" IS NOT NULL THEN "g"."LeaderNickname" || "g"."LeaderNickname"
    ELSE NULL
END
FROM "Gears" AS "g"
""");
    }

    public override async Task Include_reference_on_derived_type_using_string_nested1(bool async)
    {
        await base.Include_reference_on_derived_type_using_string_nested1(async);

        AssertSql(
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s"."Name"
FROM "LocustLeaders" AS "l"
LEFT JOIN "Gears" AS "g" ON "l"."DefeatedByNickname" = "g"."Nickname" AND "l"."DefeatedBySquadId" = "g"."SquadId"
LEFT JOIN "Squads" AS "s" ON "g"."SquadId" = "s"."Id"
""");
    }

    public override async Task Correlated_collections_same_collection_projected_multiple_times(bool async)
    {
        await base.Correlated_collections_same_collection_projected_multiple_times(async);

        AssertSql(
            """
SELECT "g"."FullName", "g"."Nickname", "g"."SquadId", "w1"."Id", "w1"."AmmunitionType", "w1"."IsAutomatic", "w1"."Name", "w1"."OwnerFullName", "w1"."SynergyWithId", "w2"."Id", "w2"."AmmunitionType", "w2"."IsAutomatic", "w2"."Name", "w2"."OwnerFullName", "w2"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
    FROM "Weapons" AS "w"
    WHERE "w"."IsAutomatic"
) AS "w1" ON "g"."FullName" = "w1"."OwnerFullName"
LEFT JOIN (
    SELECT "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
    FROM "Weapons" AS "w0"
    WHERE "w0"."IsAutomatic"
) AS "w2" ON "g"."FullName" = "w2"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId", "w1"."Id"
""");
    }

    public override async Task Include_multiple_circular_with_filter(bool async)
    {
        await base.Include_multiple_circular_with_filter(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "c"."Name", "c"."Location", "c"."Nation", "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM "Gears" AS "g"
INNER JOIN "Cities" AS "c" ON "g"."CityOfBirthName" = "c"."Name"
LEFT JOIN "Gears" AS "g0" ON "c"."Name" = "g0"."AssignedCityName"
WHERE "g"."Nickname" = 'Marcus'
ORDER BY "g"."Nickname", "g"."SquadId", "c"."Name", "g0"."Nickname"
""");
    }

    public override async Task Where_subquery_distinct_firstordefault_boolean(bool async)
    {
        await base.Where_subquery_distinct_firstordefault_boolean(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch" AND COALESCE((
    SELECT "w0"."IsAutomatic"
    FROM (
        SELECT DISTINCT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
        FROM "Weapons" AS "w"
        WHERE "g"."FullName" = "w"."OwnerFullName"
    ) AS "w0"
    ORDER BY "w0"."Id"
    LIMIT 1), 0)
""");
    }

    public override async Task Union_with_collection_navigations(bool async)
    {
        await base.Union_with_collection_navigations(async);

        AssertSql(
            """
SELECT (
    SELECT COUNT(*)
    FROM (
        SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
        FROM "Gears" AS "g0"
        WHERE "g"."Nickname" = "g0"."LeaderNickname" AND "g"."SquadId" = "g0"."LeaderSquadId"
        UNION
        SELECT "g1"."Nickname", "g1"."SquadId", "g1"."AssignedCityName", "g1"."CityOfBirthName", "g1"."Discriminator", "g1"."FullName", "g1"."HasSoulPatch", "g1"."LeaderNickname", "g1"."LeaderSquadId", "g1"."Rank"
        FROM "Gears" AS "g1"
        WHERE "g"."Nickname" = "g1"."LeaderNickname" AND "g"."SquadId" = "g1"."LeaderSquadId"
    ) AS "u")
FROM "Gears" AS "g"
WHERE "g"."Discriminator" = 'Officer'
""");
    }

    public override async Task Accessing_derived_property_using_hard_and_soft_cast(bool async)
    {
        await base.Accessing_derived_property_using_hard_and_soft_cast(async);

        AssertSql(
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId"
FROM "LocustLeaders" AS "l"
WHERE "l"."Discriminator" = 'LocustCommander' AND ("l"."HighCommandId" <> 0 OR "l"."HighCommandId" IS NULL)
""",
            //
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId"
FROM "LocustLeaders" AS "l"
WHERE "l"."Discriminator" = 'LocustCommander' AND ("l"."HighCommandId" <> 0 OR "l"."HighCommandId" IS NULL)
""");
    }

    public override async Task DateTimeOffset_DateAdd_AddSeconds(bool async)
    {
        await base.DateTimeOffset_DateAdd_AddSeconds(async);

        AssertSql(
            """
SELECT "m"."Timeline"
FROM "Missions" AS "m"
""");
    }

    public override async Task Where_bitwise_and_nullable_enum_with_non_nullable_parameter(bool async)
    {
        await base.Where_bitwise_and_nullable_enum_with_non_nullable_parameter(async);

        AssertSql(
            """
@__ammunitionType_0='1'

SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE "w"."AmmunitionType" & @__ammunitionType_0 > 0
""");
    }

    public override async Task Include_with_join_and_inheritance3(bool async)
    {
        await base.Include_with_join_and_inheritance3(async);

        AssertSql(
            """
SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "t"."Id", "g1"."Nickname", "g1"."SquadId", "g1"."AssignedCityName", "g1"."CityOfBirthName", "g1"."Discriminator", "g1"."FullName", "g1"."HasSoulPatch", "g1"."LeaderNickname", "g1"."LeaderSquadId", "g1"."Rank"
FROM "Tags" AS "t"
INNER JOIN (
    SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
    FROM "Gears" AS "g"
    WHERE "g"."Discriminator" = 'Officer'
) AS "g0" ON "t"."GearSquadId" = "g0"."SquadId" AND "t"."GearNickName" = "g0"."Nickname"
LEFT JOIN "Gears" AS "g1" ON "g0"."Nickname" = "g1"."LeaderNickname" AND "g0"."SquadId" = "g1"."LeaderSquadId"
ORDER BY "t"."Id", "g0"."Nickname", "g0"."SquadId", "g1"."Nickname"
""");
    }

    public override async Task Where_conditional_equality_3(bool async)
    {
        await base.Where_conditional_equality_3(async);

        AssertSql(
            """
SELECT "g"."Nickname"
FROM "Gears" AS "g"
ORDER BY "g"."Nickname"
""");
    }

    public override async Task Where_bitwise_and_enum(bool async)
    {
        await base.Where_bitwise_and_enum(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."Rank" & 2 > 0
""",
            //
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."Rank" & 2 = 2
""");
    }

    public override async Task Select_conditional_with_anonymous_type_and_null_constant(bool async)
    {
        await base.Select_conditional_with_anonymous_type_and_null_constant(async);

        AssertSql(
            """
SELECT "g"."LeaderNickname" IS NOT NULL, "g"."HasSoulPatch"
FROM "Gears" AS "g"
ORDER BY "g"."Nickname"
""");
    }

    public override async Task Where_subquery_left_join_firstordefault_boolean(bool async)
    {
        await base.Where_subquery_left_join_firstordefault_boolean(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch" AND (
    SELECT "w"."IsAutomatic"
    FROM "Weapons" AS "w"
    LEFT JOIN (
        SELECT "w0"."Id"
        FROM "Weapons" AS "w0"
        WHERE "g"."FullName" = "w0"."OwnerFullName"
    ) AS "w1" ON "w"."Id" = "w1"."Id"
    WHERE "g"."FullName" = "w"."OwnerFullName"
    ORDER BY "w"."Id"
    LIMIT 1)
""");
    }

    public override async Task ToString_boolean_property_nullable(bool async)
    {
        await base.ToString_boolean_property_nullable(async);

        AssertSql(
            """
SELECT CASE
    WHEN "f"."Eradicated" = 0 THEN 'False'
    WHEN "f"."Eradicated" = 1 THEN 'True'
    ELSE NULL
END
FROM "Factions" AS "f"
""");
    }

    public override async Task Filtered_collection_projection_with_order_comparison_predicate_converted_to_join3(bool async)
    {
        await base.Filtered_collection_projection_with_order_comparison_predicate_converted_to_join3(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName" AND "g"."SquadId" >= "w"."Id"
ORDER BY "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task Navigation_access_fk_on_derived_entity_using_cast(bool async)
    {
        await base.Navigation_access_fk_on_derived_entity_using_cast(async);

        AssertSql(
            """
SELECT "f"."Name", "l0"."Name" AS "CommanderName"
FROM "Factions" AS "f"
LEFT JOIN (
    SELECT "l"."Name"
    FROM "LocustLeaders" AS "l"
    WHERE "l"."Discriminator" = 'LocustCommander'
) AS "l0" ON "f"."CommanderName" = "l0"."Name"
ORDER BY "f"."Name"
""");
    }

    public override async Task DateTimeOffset_DateAdd_AddDays(bool async)
    {
        await base.DateTimeOffset_DateAdd_AddDays(async);

        AssertSql(
            """
SELECT "m"."Timeline"
FROM "Missions" AS "m"
""");
    }

    public override async Task Select_ternary_operation_with_inverted_boolean(bool async)
    {
        await base.Select_ternary_operation_with_inverted_boolean(async);

        AssertSql(
            """
SELECT "w"."Id", CASE
    WHEN NOT ("w"."IsAutomatic") THEN 1
    ELSE 0
END AS "Num"
FROM "Weapons" AS "w"
""");
    }

    public override async Task Concat_with_scalar_projection(bool async)
    {
        await base.Concat_with_scalar_projection(async);

        AssertSql(
            """
SELECT "g"."Nickname"
FROM "Gears" AS "g"
UNION ALL
SELECT "g0"."Nickname"
FROM "Gears" AS "g0"
""");
    }

    public override async Task Comparing_entities_using_Equals_inheritance(bool async)
    {
        await base.Comparing_entities_using_Equals_inheritance(async);

        AssertSql(
            """
SELECT "g"."Nickname" AS "Nickname1", "g1"."Nickname" AS "Nickname2"
FROM "Gears" AS "g"
CROSS JOIN (
    SELECT "g0"."Nickname", "g0"."SquadId"
    FROM "Gears" AS "g0"
    WHERE "g0"."Discriminator" = 'Officer'
) AS "g1"
WHERE "g"."Nickname" = "g1"."Nickname" AND "g"."SquadId" = "g1"."SquadId"
ORDER BY "g"."Nickname", "g1"."Nickname"
""");
    }

    public override async Task Select_inverted_boolean(bool async)
    {
        await base.Select_inverted_boolean(async);

        AssertSql(
            """
SELECT "w"."Id", NOT ("w"."IsAutomatic") AS "Manual"
FROM "Weapons" AS "w"
WHERE "w"."IsAutomatic"
""");
    }

    public override async Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys(bool async)
    {
        await base.Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys(async);

        AssertSql(
            """
SELECT "g"."FullName"
FROM "Gears" AS "g"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
WHERE "g"."Discriminator" = 'Officer' AND EXISTS (
    SELECT 1
    FROM "Gears" AS "g0"
    WHERE "g"."Nickname" = "g0"."LeaderNickname" AND "g"."SquadId" = "g0"."LeaderSquadId")
ORDER BY "g"."HasSoulPatch" DESC, "t"."Note"
""");
    }

    public override async Task Navigation_based_on_complex_expression3(bool async)
    {
        await base.Navigation_based_on_complex_expression3(async);

        AssertSql(
            """
SELECT "l0"."Name", "l0"."Discriminator", "l0"."LocustHordeId", "l0"."ThreatLevel", "l0"."ThreatLevelByte", "l0"."ThreatLevelNullableByte", "l0"."DefeatedByNickname", "l0"."DefeatedBySquadId", "l0"."HighCommandId"
FROM "Factions" AS "f"
LEFT JOIN (
    SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId"
    FROM "LocustLeaders" AS "l"
    WHERE "l"."Discriminator" = 'LocustCommander'
) AS "l0" ON "f"."CommanderName" = "l0"."Name"
""");
    }

    public override async Task Coalesce_operator_in_predicate(bool async)
    {
        await base.Coalesce_operator_in_predicate(async);

        AssertSql(
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE COALESCE("w"."IsAutomatic", 0)
""");
    }

    public override async Task Select_as_operator(bool async)
    {
        await base.Select_as_operator(async);

        AssertSql(
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId"
FROM "LocustLeaders" AS "l"
""");
    }

    public override async Task Negated_bool_ternary_inside_anonymous_type_in_projection(bool async)
    {
        await base.Negated_bool_ternary_inside_anonymous_type_in_projection(async);

        AssertSql(
            """
SELECT NOT (CASE
    WHEN "g"."HasSoulPatch" THEN 1
    ELSE COALESCE("g"."HasSoulPatch", 1)
END) AS "c"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
""");
    }

    public override async Task Take_without_orderby_followed_by_orderBy_is_pushed_down1(bool async)
    {
        await base.Take_without_orderby_followed_by_orderBy_is_pushed_down1(async);

        AssertSql(
            """
@__p_0='999'

SELECT "g0"."FullName"
FROM (
    SELECT "g"."FullName", "g"."Rank"
    FROM "Gears" AS "g"
    WHERE NOT ("g"."HasSoulPatch")
    LIMIT @__p_0
) AS "g0"
ORDER BY "g0"."Rank"
""");
    }

    public override async Task Include_on_derived_multi_level(bool async)
    {
        await base.Include_on_derived_multi_level(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "s1"."Nickname", "s1"."SquadId", "s1"."AssignedCityName", "s1"."CityOfBirthName", "s1"."Discriminator", "s1"."FullName", "s1"."HasSoulPatch", "s1"."LeaderNickname", "s1"."LeaderSquadId", "s1"."Rank", "s1"."Id", "s1"."Banner", "s1"."Banner5", "s1"."InternalNumber", "s1"."Name", "s1"."SquadId0", "s1"."MissionId"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s"."Name", "s0"."SquadId" AS "SquadId0", "s0"."MissionId"
    FROM "Gears" AS "g0"
    INNER JOIN "Squads" AS "s" ON "g0"."SquadId" = "s"."Id"
    LEFT JOIN "SquadMissions" AS "s0" ON "s"."Id" = "s0"."SquadId"
) AS "s1" ON "g"."Nickname" = "s1"."LeaderNickname" AND "g"."SquadId" = "s1"."LeaderSquadId"
ORDER BY "g"."Nickname", "g"."SquadId", "s1"."Nickname", "s1"."SquadId", "s1"."Id", "s1"."SquadId0"
""");
    }

    public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result(bool async)
    {
        await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "g0"."Nickname", "g0"."SquadId", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId", "w1"."Id", "w1"."AmmunitionType", "w1"."IsAutomatic", "w1"."Name", "w1"."OwnerFullName", "w1"."SynergyWithId", "w2"."Id", "w2"."AmmunitionType", "w2"."IsAutomatic", "w2"."Name", "w2"."OwnerFullName", "w2"."SynergyWithId", "g0"."Nickname" IS NOT NULL AND "g0"."SquadId" IS NOT NULL, "w3"."Id", "w3"."AmmunitionType", "w3"."IsAutomatic", "w3"."Name", "w3"."OwnerFullName", "w3"."SynergyWithId", "w4"."Id", "w4"."AmmunitionType", "w4"."IsAutomatic", "w4"."Name", "w4"."OwnerFullName", "w4"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."LeaderNickname" = "g0"."Nickname"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
LEFT JOIN "Weapons" AS "w0" ON "g0"."FullName" = "w0"."OwnerFullName"
LEFT JOIN "Weapons" AS "w1" ON "g0"."FullName" = "w1"."OwnerFullName"
LEFT JOIN "Weapons" AS "w2" ON "g"."FullName" = "w2"."OwnerFullName"
LEFT JOIN "Weapons" AS "w3" ON "g0"."FullName" = "w3"."OwnerFullName"
LEFT JOIN "Weapons" AS "w4" ON "g"."FullName" = "w4"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId", "g0"."Nickname", "g0"."SquadId", "w"."Id", "w0"."Id", "w1"."Id", "w2"."Id", "w3"."Id"
""");
    }

    public override async Task Correlated_collections_with_Skip(bool async)
    {
        await base.Correlated_collections_with_Skip(async);

        AssertSql(
            """
SELECT "s"."Id", "g1"."Nickname", "g1"."SquadId", "g1"."AssignedCityName", "g1"."CityOfBirthName", "g1"."Discriminator", "g1"."FullName", "g1"."HasSoulPatch", "g1"."LeaderNickname", "g1"."LeaderSquadId", "g1"."Rank"
FROM "Squads" AS "s"
LEFT JOIN (
    SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
    FROM (
        SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", ROW_NUMBER() OVER(PARTITION BY "g"."SquadId" ORDER BY "g"."Nickname") AS "row"
        FROM "Gears" AS "g"
    ) AS "g0"
    WHERE 1 < "g0"."row"
) AS "g1" ON "s"."Id" = "g1"."SquadId"
ORDER BY "s"."Name", "s"."Id", "g1"."SquadId", "g1"."Nickname"
""");
    }

    public override async Task Select_subquery_int_with_pushdown_and_coalesce(bool async)
    {
        await base.Select_subquery_int_with_pushdown_and_coalesce(async);

        AssertSql(
            """
SELECT COALESCE((
    SELECT "w"."Id"
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName"
    ORDER BY "w"."Id"
    LIMIT 1), 42)
FROM "Gears" AS "g"
""");
    }

    public override async Task Bitwise_operation_with_non_null_parameter_optimizes_null_checks(bool async)
    {
        await base.Bitwise_operation_with_non_null_parameter_optimizes_null_checks(async);

        AssertSql(
            """
@__ranks_0='134'

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."Rank" & @__ranks_0 <> 0
""",
            //
            """
@__ranks_0='134'

SELECT "g"."Rank" | @__ranks_0 = @__ranks_0
FROM "Gears" AS "g"
""",
            //
            """
@__ranks_0='134'

SELECT "g"."Rank" | "g"."Rank" | @__ranks_0 | "g"."Rank" | @__ranks_0 = @__ranks_0
FROM "Gears" AS "g"
""");
    }

    public override async Task TimeSpan_Minutes(bool async)
    {
        await base.TimeSpan_Minutes(async);

        AssertSql(
            """
SELECT "m"."Duration"
FROM "Missions" AS "m"
""");
    }

    public override async Task Constant_enum_with_same_underlying_value_as_previously_parameterized_int(bool async)
    {
        await base.Constant_enum_with_same_underlying_value_as_previously_parameterized_int(async);

        AssertSql(
            """
@__p_0='1'

SELECT "g"."Rank" & 1
FROM "Gears" AS "g"
ORDER BY "g"."Nickname"
LIMIT @__p_0
""");
    }

    public override async Task Correlated_collections_with_Take(bool async)
    {
        await base.Correlated_collections_with_Take(async);

        AssertSql(
            """
SELECT "s"."Id", "g1"."Nickname", "g1"."SquadId", "g1"."AssignedCityName", "g1"."CityOfBirthName", "g1"."Discriminator", "g1"."FullName", "g1"."HasSoulPatch", "g1"."LeaderNickname", "g1"."LeaderSquadId", "g1"."Rank"
FROM "Squads" AS "s"
LEFT JOIN (
    SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
    FROM (
        SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", ROW_NUMBER() OVER(PARTITION BY "g"."SquadId" ORDER BY "g"."Nickname") AS "row"
        FROM "Gears" AS "g"
    ) AS "g0"
    WHERE "g0"."row" <= 2
) AS "g1" ON "s"."Id" = "g1"."SquadId"
ORDER BY "s"."Name", "s"."Id", "g1"."SquadId", "g1"."Nickname"
""");
    }

    public override async Task Join_on_entity_qsre_keys(bool async)
    {
        await base.Join_on_entity_qsre_keys(async);

        AssertSql(
            """
SELECT "w"."Name" AS "Name1", "w0"."Name" AS "Name2"
FROM "Weapons" AS "w"
INNER JOIN "Weapons" AS "w0" ON "w"."Id" = "w0"."Id"
""");
    }

    public override async Task Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery(bool async)
    {
        await base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery(async);

        AssertSql(
            """
SELECT "c"."Name", "c"."Location", "c"."Nation"
FROM "Cities" AS "c"
WHERE "c"."Location" = 'Unknown' AND (
    SELECT COUNT(*)
    FROM "Gears" AS "g"
    WHERE "c"."Name" = "g"."CityOfBirthName" AND "g"."Nickname" = 'Paduk') = 1
""");
    }

    public override async Task Filter_with_new_Guid(bool async)
    {
        await base.Filter_with_new_Guid(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Tags" AS "t"
WHERE "t"."Id" = 'DF36F493-463F-4123-83F9-6B135DEEB7BA'
""");
    }

    public override async Task SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector_order_comparison(bool async)
    {
        await base.SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector_order_comparison(async);

        AssertSql(
            """
@__prm_0='1'

SELECT "g"."Nickname", "g"."FullName", "w0"."Id" IS NOT NULL AS "Collection"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "w"."Id", "w"."OwnerFullName"
    FROM "Weapons" AS "w"
    WHERE "w"."Id" > @__prm_0
) AS "w0" ON "g"."FullName" = "w0"."OwnerFullName"
""");
    }

    public override async Task Select_null_conditional_with_inheritance(bool async)
    {
        await base.Select_null_conditional_with_inheritance(async);

        AssertSql(
            """
SELECT CASE
    WHEN "f"."CommanderName" IS NOT NULL THEN "f"."CommanderName"
    ELSE NULL
END
FROM "Factions" AS "f"
""");
    }

    public override async Task String_concat_nullable_expressions_are_coalesced(bool async)
    {
        await base.String_concat_nullable_expressions_are_coalesced(async);

        AssertSql(
            """
SELECT "g"."FullName" || '' || COALESCE("g"."LeaderNickname", '') || ''
FROM "Gears" AS "g"
""");
    }

    public override async Task Left_join_predicate_value(bool async)
    {
        await base.Left_join_predicate_value(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Weapons" AS "w" ON "g"."HasSoulPatch"
""");
    }

    public override async Task Join_navigation_translated_to_subquery_composite_key(bool async)
    {
        await base.Join_navigation_translated_to_subquery_composite_key(async);

        AssertSql(
            """
SELECT "g"."FullName", "s"."Note"
FROM "Gears" AS "g"
INNER JOIN (
    SELECT "t"."Note", "g0"."FullName"
    FROM "Tags" AS "t"
    LEFT JOIN "Gears" AS "g0" ON "t"."GearNickName" = "g0"."Nickname" AND "t"."GearSquadId" = "g0"."SquadId"
) AS "s" ON "g"."FullName" = "s"."FullName"
""");
    }

    public override async Task Correlated_collections_basic_projection_explicit_to_list(bool async)
    {
        await base.Correlated_collections_basic_projection_explicit_to_list(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
    FROM "Weapons" AS "w"
    WHERE "w"."IsAutomatic" OR "w"."Name" <> 'foo' OR "w"."Name" IS NULL
) AS "w0" ON "g"."FullName" = "w0"."OwnerFullName"
WHERE "g"."Nickname" <> 'Marcus'
ORDER BY "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task Unicode_string_literals_is_used_for_non_unicode_column_with_concat(bool async)
    {
        await base.Unicode_string_literals_is_used_for_non_unicode_column_with_concat(async);

        AssertSql(
            """
SELECT "c"."Name", "c"."Location", "c"."Nation"
FROM "Cities" AS "c"
WHERE instr(COALESCE("c"."Location", '') || 'Added', 'Add') > 0
""");
    }

    public override async Task Include_multiple_circular(bool async)
    {
        await base.Include_multiple_circular(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "c"."Name", "c"."Location", "c"."Nation", "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM "Gears" AS "g"
INNER JOIN "Cities" AS "c" ON "g"."CityOfBirthName" = "c"."Name"
LEFT JOIN "Gears" AS "g0" ON "c"."Name" = "g0"."AssignedCityName"
ORDER BY "g"."Nickname", "g"."SquadId", "c"."Name", "g0"."Nickname"
""");
    }

    public override async Task Join_on_entity_qsre_keys_inner_key_is_navigation_composite_key(bool async)
    {
        await base.Join_on_entity_qsre_keys_inner_key_is_navigation_composite_key(async);

        AssertSql(
            """
SELECT "g"."Nickname", "s"."Note"
FROM "Gears" AS "g"
INNER JOIN (
    SELECT "t"."Note", "g0"."Nickname", "g0"."SquadId"
    FROM "Tags" AS "t"
    LEFT JOIN "Gears" AS "g0" ON "t"."GearNickName" = "g0"."Nickname" AND "t"."GearSquadId" = "g0"."SquadId"
    WHERE "t"."Note" IN ('Cole''s Tag', 'Dom''s Tag')
) AS "s" ON "g"."Nickname" = "s"."Nickname" AND "g"."SquadId" = "s"."SquadId"
""");
    }

    public override async Task Contains_on_readonly_enumerable(bool async)
    {
        await base.Contains_on_readonly_enumerable(async);

        AssertSql(
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE "w"."AmmunitionType" = 1
""");
    }

    public override async Task Composite_key_entity_not_equal_null(bool async)
    {
        await base.Composite_key_entity_not_equal_null(async);

        AssertSql(
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId"
FROM "LocustLeaders" AS "l"
LEFT JOIN "Gears" AS "g" ON "l"."DefeatedByNickname" = "g"."Nickname" AND "l"."DefeatedBySquadId" = "g"."SquadId"
WHERE "l"."Discriminator" = 'LocustCommander' AND "g"."Nickname" IS NOT NULL AND "g"."SquadId" IS NOT NULL
""");
    }

    public override async Task Where_nullable_enum_with_non_nullable_parameter(bool async)
    {
        await base.Where_nullable_enum_with_non_nullable_parameter(async);

        AssertSql(
            """
@__ammunitionType_0='1'

SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE "w"."AmmunitionType" = @__ammunitionType_0
""");
    }

    public override async Task Take_without_orderby_followed_by_orderBy_is_pushed_down2(bool async)
    {
        await base.Take_without_orderby_followed_by_orderBy_is_pushed_down2(async);

        AssertSql(
            """
@__p_0='999'

SELECT "g0"."FullName"
FROM (
    SELECT "g"."FullName", "g"."Rank"
    FROM "Gears" AS "g"
    WHERE NOT ("g"."HasSoulPatch")
    LIMIT @__p_0
) AS "g0"
ORDER BY "g0"."Rank"
""");
    }

    public override async Task Where_equals_method_on_nullable_with_object_overload(bool async)
    {
        await base.Where_equals_method_on_nullable_with_object_overload(async);

        AssertSql(
            """
SELECT "m"."Id", "m"."CodeName", "m"."Date", "m"."Duration", "m"."Rating", "m"."Time", "m"."Timeline"
FROM "Missions" AS "m"
WHERE "m"."Rating" IS NULL
""");
    }

    public override async Task Subquery_containing_join_projecting_main_from_clause_gets_lifted(bool async)
    {
        await base.Subquery_containing_join_projecting_main_from_clause_gets_lifted(async);

        AssertSql(
            """
SELECT "g"."Nickname"
FROM "Gears" AS "g"
INNER JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName"
ORDER BY "g"."Nickname"
""");
    }

    public override async Task Include_base_navigation_on_derived_entity(bool async)
    {
        await base.Include_base_navigation_on_derived_entity(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId", "t"."Id"
""");
    }

    public override async Task Select_subquery_boolean_empty_with_pushdown(bool async)
    {
        await base.Select_subquery_boolean_empty_with_pushdown(async);

        AssertSql(
            """
SELECT (
    SELECT "w"."IsAutomatic"
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName" AND "w"."Name" = 'BFG'
    ORDER BY "w"."Id"
    LIMIT 1)
FROM "Gears" AS "g"
""");
    }

    public override async Task OrderBy_StartsWith_with_null_parameter_as_argument(bool async)
    {
        await base.OrderBy_StartsWith_with_null_parameter_as_argument(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
ORDER BY "g"."Nickname"
""");
    }

    public override async Task Include_collection_with_complex_OrderBy3(bool async)
    {
        await base.Include_collection_with_complex_OrderBy3(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."Nickname" = "g0"."LeaderNickname" AND "g"."SquadId" = "g0"."LeaderSquadId"
WHERE "g"."Discriminator" = 'Officer'
ORDER BY COALESCE((
    SELECT "w"."IsAutomatic"
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName"
    ORDER BY "w"."Id"
    LIMIT 1), 0), "g"."Nickname", "g"."SquadId", "g0"."Nickname"
""");
    }

    public override async Task Projecting_property_converted_to_nullable_and_use_it_in_order_by(bool async)
    {
        await base.Projecting_property_converted_to_nullable_and_use_it_in_order_by(async);

        AssertSql(
            """
SELECT "t"."Note", "t"."GearNickName" IS NOT NULL, "g"."Nickname", "g"."SquadId", "g"."HasSoulPatch"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE CASE
    WHEN "t"."GearNickName" IS NOT NULL THEN "g"."Nickname"
    ELSE NULL
END IS NOT NULL
ORDER BY CASE
    WHEN "t"."GearNickName" IS NOT NULL THEN "g"."SquadId"
    ELSE NULL
END, "t"."Note"
""");
    }

    public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1()
    {
        base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1();

        AssertSql(
            """
SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."LeaderNickname" = "g0"."Nickname"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId", "g0"."Nickname", "g0"."SquadId"
""");
    }

    public override async Task Left_join_predicate_value_equals_condition(bool async)
    {
        await base.Left_join_predicate_value_equals_condition(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Weapons" AS "w" ON "w"."SynergyWithId" IS NOT NULL
""");
    }

    public override async Task Conditional_expression_with_test_being_simplified_to_constant_complex(bool async)
    {
        await base.Conditional_expression_with_test_being_simplified_to_constant_complex(async);

        AssertSql(
            """
@__prm_0='True'
@__prm2_1='Marcus' Lancer' (Size = 14)

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE CASE
    WHEN "g"."HasSoulPatch" = @__prm_0 THEN (
        SELECT "w"."Name"
        FROM "Weapons" AS "w"
        WHERE "w"."Id" = "g"."SquadId"
        LIMIT 1) = @__prm2_1 AND (
        SELECT "w"."Name"
        FROM "Weapons" AS "w"
        WHERE "w"."Id" = "g"."SquadId"
        LIMIT 1) IS NOT NULL
    ELSE 0
END
""");
    }

    public override async Task Select_correlated_filtered_collection(bool async)
    {
        await base.Select_correlated_filtered_collection(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "c"."Name", "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
FROM "Gears" AS "g"
INNER JOIN "Cities" AS "c" ON "g"."CityOfBirthName" = "c"."Name"
LEFT JOIN (
    SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
    FROM "Weapons" AS "w"
    WHERE "w"."Name" <> 'Lancer' OR "w"."Name" IS NULL
) AS "w0" ON "g"."FullName" = "w0"."OwnerFullName"
WHERE "c"."Name" IN ('Ephyra', 'Hanover')
ORDER BY "g"."Nickname", "g"."SquadId", "c"."Name"
""");
    }

    public override async Task Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_complex_orderings(
        bool async)
    {
        await base.Multiple_orderby_with_navigation_expansion_on_one_of_the_order_bys_inside_subquery_complex_orderings(async);

        AssertSql(
            """
SELECT "g"."FullName", "g"."Nickname", "g"."SquadId", "t"."Id", "g1"."Nickname", "g1"."SquadId", "s"."Id", "s"."AmmunitionType", "s"."IsAutomatic", "s"."Name", "s"."OwnerFullName", "s"."SynergyWithId", "s"."Nickname", "s"."SquadId"
FROM "Gears" AS "g"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
LEFT JOIN "Gears" AS "g1" ON "t"."GearNickName" = "g1"."Nickname" AND "t"."GearSquadId" = "g1"."SquadId"
LEFT JOIN (
    SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId", "g2"."Nickname", "g2"."SquadId", (
        SELECT COUNT(*)
        FROM "Weapons" AS "w0"
        WHERE "g2"."FullName" IS NOT NULL AND "g2"."FullName" = "w0"."OwnerFullName") AS "c"
    FROM "Weapons" AS "w"
    LEFT JOIN "Gears" AS "g2" ON "w"."OwnerFullName" = "g2"."FullName"
) AS "s" ON "g1"."FullName" = "s"."OwnerFullName"
WHERE "g"."Discriminator" = 'Officer' AND EXISTS (
    SELECT 1
    FROM "Gears" AS "g0"
    WHERE "g"."Nickname" = "g0"."LeaderNickname" AND "g"."SquadId" = "g0"."LeaderSquadId")
ORDER BY "g"."HasSoulPatch" DESC, "t"."Note", "g"."Nickname", "g"."SquadId", "t"."Id", "g1"."Nickname", "g1"."SquadId", "s"."Id" DESC, "s"."c", "s"."Nickname"
""");
    }

    public override async Task Where_member_access_on_anonymous_type(bool async)
    {
        await base.Where_member_access_on_anonymous_type(async);

        AssertSql(
            """
SELECT "g"."Nickname"
FROM "Gears" AS "g"
WHERE "g"."LeaderNickname" = 'Marcus'
""");
    }

    public override async Task Correlated_collections_nested(bool async)
    {
        await base.Correlated_collections_nested(async);

        AssertSql(
            """
SELECT "s"."Id", "s3"."SquadId", "s3"."MissionId", "s3"."Id", "s3"."SquadId0", "s3"."MissionId0"
FROM "Squads" AS "s"
LEFT JOIN (
    SELECT "s0"."SquadId", "s0"."MissionId", "m"."Id", "s2"."SquadId" AS "SquadId0", "s2"."MissionId" AS "MissionId0"
    FROM "SquadMissions" AS "s0"
    INNER JOIN "Missions" AS "m" ON "s0"."MissionId" = "m"."Id"
    LEFT JOIN (
        SELECT "s1"."SquadId", "s1"."MissionId"
        FROM "SquadMissions" AS "s1"
        WHERE "s1"."SquadId" < 7
    ) AS "s2" ON "m"."Id" = "s2"."MissionId"
    WHERE "s0"."MissionId" < 42
) AS "s3" ON "s"."Id" = "s3"."SquadId"
ORDER BY "s"."Id", "s3"."SquadId", "s3"."MissionId", "s3"."Id", "s3"."SquadId0"
""");
    }

    public override async Task Select_subquery_int_with_pushdown_and_coalesce2(bool async)
    {
        await base.Select_subquery_int_with_pushdown_and_coalesce2(async);

        AssertSql(
            """
SELECT COALESCE((
    SELECT "w"."Id"
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName"
    ORDER BY "w"."Id"
    LIMIT 1), (
    SELECT "w0"."Id"
    FROM "Weapons" AS "w0"
    WHERE "g"."FullName" = "w0"."OwnerFullName"
    ORDER BY "w0"."Id"
    LIMIT 1))
FROM "Gears" AS "g"
""");
    }

    public override async Task GroupBy_Property_Include_Select_LongCount(bool async)
    {
        await base.GroupBy_Property_Include_Select_LongCount(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM "Gears" AS "g"
GROUP BY "g"."Rank"
""");
    }

    public override async Task ThenInclude_reference_on_derived_after_derived_collection(bool async)
    {
        await base.ThenInclude_reference_on_derived_after_derived_collection(async);

        AssertSql(
            """
SELECT "f"."Id", "f"."CapitalName", "f"."Discriminator", "f"."Name", "f"."ServerAddress", "f"."CommanderName", "f"."Eradicated", "s"."Name", "s"."Discriminator", "s"."LocustHordeId", "s"."ThreatLevel", "s"."ThreatLevelByte", "s"."ThreatLevelNullableByte", "s"."DefeatedByNickname", "s"."DefeatedBySquadId", "s"."HighCommandId", "s"."Nickname", "s"."SquadId", "s"."AssignedCityName", "s"."CityOfBirthName", "s"."Discriminator0", "s"."FullName", "s"."HasSoulPatch", "s"."LeaderNickname", "s"."LeaderSquadId", "s"."Rank"
FROM "Factions" AS "f"
LEFT JOIN (
    SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator" AS "Discriminator0", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
    FROM "LocustLeaders" AS "l"
    LEFT JOIN "Gears" AS "g" ON "l"."DefeatedByNickname" = "g"."Nickname" AND "l"."DefeatedBySquadId" = "g"."SquadId"
) AS "s" ON "f"."Id" = "s"."LocustHordeId"
ORDER BY "f"."Id", "s"."Name", "s"."Nickname"
""");
    }

    public override async Task Accessing_property_of_optional_navigation_in_child_projection_works(bool async)
    {
        await base.Accessing_property_of_optional_navigation_in_child_projection_works(async);

        AssertSql(
            """
SELECT "g"."Nickname" IS NOT NULL AND "g"."SquadId" IS NOT NULL, "t"."Id", "g"."Nickname", "g"."SquadId", "s"."Nickname", "s"."Id", "s"."SquadId"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
LEFT JOIN (
    SELECT "g0"."Nickname", "w"."Id", "g0"."SquadId", "w"."OwnerFullName"
    FROM "Weapons" AS "w"
    LEFT JOIN "Gears" AS "g0" ON "w"."OwnerFullName" = "g0"."FullName"
) AS "s" ON "g"."FullName" = "s"."OwnerFullName"
ORDER BY "t"."Note", "t"."Id", "g"."Nickname", "g"."SquadId", "s"."Id", "s"."Nickname"
""");
    }

    public override async Task ThenInclude_collection_on_derived_after_derived_reference(bool async)
    {
        await base.ThenInclude_collection_on_derived_after_derived_reference(async);

        AssertSql(
            """
SELECT "f"."Id", "f"."CapitalName", "f"."Discriminator", "f"."Name", "f"."ServerAddress", "f"."CommanderName", "f"."Eradicated", "l0"."Name", "l0"."Discriminator", "l0"."LocustHordeId", "l0"."ThreatLevel", "l0"."ThreatLevelByte", "l0"."ThreatLevelNullableByte", "l0"."DefeatedByNickname", "l0"."DefeatedBySquadId", "l0"."HighCommandId", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM "Factions" AS "f"
LEFT JOIN (
    SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId"
    FROM "LocustLeaders" AS "l"
    WHERE "l"."Discriminator" = 'LocustCommander'
) AS "l0" ON "f"."CommanderName" = "l0"."Name"
LEFT JOIN "Gears" AS "g" ON "l0"."DefeatedByNickname" = "g"."Nickname" AND "l0"."DefeatedBySquadId" = "g"."SquadId"
LEFT JOIN "Gears" AS "g0" ON ("g"."Nickname" = "g0"."LeaderNickname" OR ("g"."Nickname" IS NULL AND "g0"."LeaderNickname" IS NULL)) AND "g"."SquadId" = "g0"."LeaderSquadId"
ORDER BY "f"."Id", "l0"."Name", "g"."Nickname", "g"."SquadId", "g0"."Nickname"
""");
    }

    public override async Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected(bool async)
    {
        await base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected(async);

        AssertSql(
            """
SELECT "t"."Id" AS "Id1", "t0"."Id" AS "Id2"
FROM "Tags" AS "t"
CROSS JOIN "Tags" AS "t0"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
LEFT JOIN "Gears" AS "g0" ON "t0"."GearNickName" = "g0"."Nickname" AND "t0"."GearSquadId" = "g0"."SquadId"
WHERE "g"."Nickname" = "g0"."Nickname" OR ("g"."Nickname" IS NULL AND "g0"."Nickname" IS NULL)
""");
    }

    public override async Task Select_required_navigation_on_derived_type(bool async)
    {
        await base.Select_required_navigation_on_derived_type(async);

        AssertSql(
            """
SELECT "l0"."Name"
FROM "LocustLeaders" AS "l"
LEFT JOIN "LocustHighCommands" AS "l0" ON "l"."HighCommandId" = "l0"."Id"
""");
    }

    public override async Task Concat_with_collection_navigations(bool async)
    {
        await base.Concat_with_collection_navigations(async);

        AssertSql(
            """
SELECT (
    SELECT COUNT(*)
    FROM (
        SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
        FROM "Weapons" AS "w"
        WHERE "g"."FullName" = "w"."OwnerFullName"
        UNION
        SELECT "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
        FROM "Weapons" AS "w0"
        WHERE "g"."FullName" = "w0"."OwnerFullName"
    ) AS "u")
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch"
""");
    }

    public override async Task Correlated_collections_multiple_nested_complex_collections(bool async)
    {
        await base.Correlated_collections_multiple_nested_complex_collections(async);

        AssertSql(
            """
SELECT "g"."FullName", "g"."Nickname", "g"."SquadId", "t"."Id", "g1"."Nickname", "g1"."SquadId", "s1"."FullName", "s1"."Nickname", "s1"."SquadId", "s1"."Id", "s1"."Nickname0", "s1"."SquadId0", "s1"."Id0", "s1"."Name", "s1"."IsAutomatic", "s1"."Id1", "s1"."Nickname00", "s1"."HasSoulPatch", "s1"."SquadId00", "s2"."Id", "s2"."AmmunitionType", "s2"."IsAutomatic", "s2"."Name", "s2"."OwnerFullName", "s2"."SynergyWithId", "s2"."Nickname", "s2"."SquadId"
FROM "Gears" AS "g"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
LEFT JOIN "Gears" AS "g1" ON "t"."GearNickName" = "g1"."Nickname" AND "t"."GearSquadId" = "g1"."SquadId"
LEFT JOIN (
    SELECT "g2"."FullName", "g2"."Nickname", "g2"."SquadId", "s0"."Id", "s0"."Nickname" AS "Nickname0", "s0"."SquadId" AS "SquadId0", "s0"."Id0", "s0"."Name", "s0"."IsAutomatic", "s0"."Id1", "s0"."Nickname0" AS "Nickname00", "s0"."HasSoulPatch", "s0"."SquadId0" AS "SquadId00", "g2"."Rank", "s0"."IsAutomatic0", "g2"."LeaderNickname", "g2"."LeaderSquadId"
    FROM "Gears" AS "g2"
    LEFT JOIN (
        SELECT "w"."Id", "g3"."Nickname", "g3"."SquadId", "s"."Id" AS "Id0", "w0"."Name", "w0"."IsAutomatic", "w0"."Id" AS "Id1", "g4"."Nickname" AS "Nickname0", "g4"."HasSoulPatch", "g4"."SquadId" AS "SquadId0", "w"."IsAutomatic" AS "IsAutomatic0", "w"."OwnerFullName"
        FROM "Weapons" AS "w"
        LEFT JOIN "Gears" AS "g3" ON "w"."OwnerFullName" = "g3"."FullName"
        LEFT JOIN "Squads" AS "s" ON "g3"."SquadId" = "s"."Id"
        LEFT JOIN "Weapons" AS "w0" ON "g3"."FullName" = "w0"."OwnerFullName"
        LEFT JOIN "Gears" AS "g4" ON "s"."Id" = "g4"."SquadId"
        WHERE "w"."Name" <> 'Bar' OR "w"."Name" IS NULL
    ) AS "s0" ON "g2"."FullName" = "s0"."OwnerFullName"
    WHERE "g2"."FullName" <> 'Foo'
) AS "s1" ON "g"."Nickname" = "s1"."LeaderNickname" AND "g"."SquadId" = "s1"."LeaderSquadId"
LEFT JOIN (
    SELECT "w1"."Id", "w1"."AmmunitionType", "w1"."IsAutomatic", "w1"."Name", "w1"."OwnerFullName", "w1"."SynergyWithId", "g5"."Nickname", "g5"."SquadId"
    FROM "Weapons" AS "w1"
    LEFT JOIN "Gears" AS "g5" ON "w1"."OwnerFullName" = "g5"."FullName"
) AS "s2" ON "g1"."FullName" = "s2"."OwnerFullName"
WHERE "g"."Discriminator" = 'Officer' AND EXISTS (
    SELECT 1
    FROM "Gears" AS "g0"
    WHERE "g"."Nickname" = "g0"."LeaderNickname" AND "g"."SquadId" = "g0"."LeaderSquadId")
ORDER BY "g"."HasSoulPatch" DESC, "t"."Note", "g"."Nickname", "g"."SquadId", "t"."Id", "g1"."Nickname", "g1"."SquadId", "s1"."Rank", "s1"."Nickname", "s1"."SquadId", "s1"."IsAutomatic0", "s1"."Id", "s1"."Nickname0", "s1"."SquadId0", "s1"."Id0", "s1"."Id1", "s1"."Nickname00", "s1"."SquadId00", "s2"."IsAutomatic", "s2"."Nickname" DESC, "s2"."Id"
""");
    }

    public override async Task Bitwise_operation_with_null_arguments(bool async)
    {
        await base.Bitwise_operation_with_null_arguments(async);

        AssertSql(
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE "w"."AmmunitionType" IS NULL
""",
            //
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE "w"."AmmunitionType" IS NULL
""",
            //
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE "w"."AmmunitionType" IS NULL
""",
            //
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
""",
            //
            """
@__prm_0='2' (Nullable = true)

SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE "w"."AmmunitionType" & @__prm_0 <> 0 OR "w"."AmmunitionType" IS NULL
""",
            //
            """
@__prm_0='1' (Nullable = true)

SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE "w"."AmmunitionType" & @__prm_0 = @__prm_0
""");
    }

    public override async Task Include_with_client_method_and_member_access_still_applies_includes(bool async)
    {
        await base.Include_with_client_method_and_member_access_still_applies_includes(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Gears" AS "g"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
""");
    }

    public override async Task Optional_navigation_with_collection_composite_key(bool async)
    {
        await base.Optional_navigation_with_collection_composite_key(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE "g"."Discriminator" = 'Officer' AND (
    SELECT COUNT(*)
    FROM "Gears" AS "g0"
    WHERE "g"."Nickname" IS NOT NULL AND "g"."SquadId" IS NOT NULL AND "g"."Nickname" = "g0"."LeaderNickname" AND "g"."SquadId" = "g0"."LeaderSquadId" AND "g0"."Nickname" = 'Dom') > 0
""");
    }

    public override async Task Include_with_order_by_constant(bool async)
    {
        await base.Include_with_order_by_constant(async);

        AssertSql(
            """
SELECT "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s"."Name", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Squads" AS "s"
LEFT JOIN "Gears" AS "g" ON "s"."Id" = "g"."SquadId"
ORDER BY "s"."Id", "g"."Nickname"
""");
    }

    public override async Task Include_on_derived_type_with_order_by_and_paging(bool async)
    {
        await base.Include_on_derived_type_with_order_by_and_paging(async);

        AssertSql(
            """
@__p_0='10'

SELECT "s"."Name", "s"."Discriminator", "s"."LocustHordeId", "s"."ThreatLevel", "s"."ThreatLevelByte", "s"."ThreatLevelNullableByte", "s"."DefeatedByNickname", "s"."DefeatedBySquadId", "s"."HighCommandId", "s"."Nickname", "s"."SquadId", "s"."AssignedCityName", "s"."CityOfBirthName", "s"."Discriminator0", "s"."FullName", "s"."HasSoulPatch", "s"."LeaderNickname", "s"."LeaderSquadId", "s"."Rank", "s"."Id", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM (
    SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator" AS "Discriminator0", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "t"."Id", "t"."Note"
    FROM "LocustLeaders" AS "l"
    LEFT JOIN "Gears" AS "g" ON "l"."DefeatedByNickname" = "g"."Nickname" AND "l"."DefeatedBySquadId" = "g"."SquadId"
    LEFT JOIN "Tags" AS "t" ON ("g"."Nickname" = "t"."GearNickName" OR ("g"."Nickname" IS NULL AND "t"."GearNickName" IS NULL)) AND ("g"."SquadId" = "t"."GearSquadId" OR ("g"."SquadId" IS NULL AND "t"."GearSquadId" IS NULL))
    ORDER BY "t"."Note"
    LIMIT @__p_0
) AS "s"
LEFT JOIN "Weapons" AS "w" ON "s"."FullName" = "w"."OwnerFullName"
ORDER BY "s"."Note", "s"."Name", "s"."Nickname", "s"."SquadId", "s"."Id"
""");
    }

    public override async Task Trying_to_access_unmapped_property_in_projection(bool async)
    {
        await base.Trying_to_access_unmapped_property_in_projection(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
""");
    }

    public override async Task GroupBy_with_boolean_groupin_key_thru_navigation_access(bool async)
    {
        await base.GroupBy_with_boolean_groupin_key_thru_navigation_access(async);

        AssertSql(
            """
SELECT "g"."HasSoulPatch", lower("s"."Name") AS "Name"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
LEFT JOIN "Squads" AS "s" ON "g"."SquadId" = "s"."Id"
GROUP BY "g"."HasSoulPatch", "s"."Name"
""");
    }

    public override async Task Filter_with_complex_predicate_containing_subquery(bool async)
    {
        await base.Filter_with_complex_predicate_containing_subquery(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."FullName" <> 'Dom' AND EXISTS (
    SELECT 1
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName" AND "w"."IsAutomatic")
""");
    }

    public override async Task Project_one_value_type_converted_to_nullable_from_empty_collection(bool async)
    {
        await base.Project_one_value_type_converted_to_nullable_from_empty_collection(async);

        AssertSql(
            """
SELECT "s"."Name", (
    SELECT "g"."SquadId"
    FROM "Gears" AS "g"
    WHERE "s"."Id" = "g"."SquadId" AND "g"."HasSoulPatch"
    LIMIT 1) AS "SquadId"
FROM "Squads" AS "s"
WHERE "s"."Name" = 'Kilo'
""");
    }

    public override async Task Query_reusing_parameter_with_inner_query_doesnt_declare_duplicate_parameter(bool async)
    {
        await base.Query_reusing_parameter_with_inner_query_doesnt_declare_duplicate_parameter(async);

        AssertSql(
            """
@__squadId_0='1'

SELECT "u"."Nickname", "u"."SquadId", "u"."AssignedCityName", "u"."CityOfBirthName", "u"."Discriminator", "u"."FullName", "u"."HasSoulPatch", "u"."LeaderNickname", "u"."LeaderSquadId", "u"."Rank"
FROM (
    SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
    FROM "Gears" AS "g"
    INNER JOIN "Squads" AS "s" ON "g"."SquadId" = "s"."Id"
    WHERE "s"."Id" IN (
        SELECT "s0"."Id"
        FROM "Squads" AS "s0"
        WHERE "s0"."Id" = @__squadId_0
    )
    UNION ALL
    SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
    FROM "Gears" AS "g0"
    INNER JOIN "Squads" AS "s1" ON "g0"."SquadId" = "s1"."Id"
    WHERE "s1"."Id" IN (
        SELECT "s2"."Id"
        FROM "Squads" AS "s2"
        WHERE "s2"."Id" = @__squadId_0
    )
) AS "u"
ORDER BY "u"."FullName"
""");
    }

    public override async Task String_concat_with_null_conditional_argument(bool async)
    {
        await base.String_concat_with_null_conditional_argument(async);

        AssertSql(
            """
SELECT "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
FROM "Weapons" AS "w"
LEFT JOIN "Weapons" AS "w0" ON "w"."SynergyWithId" = "w0"."Id"
ORDER BY COALESCE("w0"."Name", '') || CAST(5 AS TEXT)
""");
    }

    public override async Task Sum_with_no_data_nullable_double(bool async)
    {
        await base.Sum_with_no_data_nullable_double(async);

        AssertSql(
            """
SELECT COALESCE(SUM("m"."Rating"), 0.0)
FROM "Missions" AS "m"
WHERE "m"."CodeName" = 'Operation Foobar'
""");
    }

    public override async Task Select_subquery_boolean_with_pushdown(bool async)
    {
        await base.Select_subquery_boolean_with_pushdown(async);

        AssertSql(
            """
SELECT (
    SELECT "w"."IsAutomatic"
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName"
    ORDER BY "w"."Id"
    LIMIT 1)
FROM "Gears" AS "g"
""");
    }

    public override async Task Cast_to_derived_type_after_OfType_works(bool async)
    {
        await base.Cast_to_derived_type_after_OfType_works(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."Discriminator" = 'Officer'
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_binary_expression(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_binary_expression(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE "g"."HasSoulPatch" OR ("t"."Note" IS NOT NULL AND instr("t"."Note", 'Cole') > 0)
""");
    }

    public override async Task Join_predicate_value(bool async)
    {
        await base.Join_predicate_value(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
INNER JOIN "Weapons" AS "w" ON "g"."HasSoulPatch"
""");
    }

    public override async Task Multiple_order_bys_are_properly_lifted_from_subquery_created_by_include(bool async)
    {
        await base.Multiple_order_bys_are_properly_lifted_from_subquery_created_by_include(async);

        AssertSql(
            """
SELECT "g"."FullName"
FROM "Gears" AS "g"
WHERE NOT ("g"."HasSoulPatch")
ORDER BY "g"."FullName"
""");
    }

    public override async Task Select_subquery_distinct_singleordefault_boolean_empty2(bool async)
    {
        await base.Select_subquery_distinct_singleordefault_boolean_empty2(async);

        AssertSql(
            """
SELECT COALESCE((
    SELECT DISTINCT "w"."IsAutomatic"
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName" AND "w"."Name" = 'BFG'
    LIMIT 1), 0)
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch"
""");
    }

    public override async Task Correlated_collections_left_join_with_self_reference(bool async)
    {
        await base.Correlated_collections_left_join_with_self_reference(async);

        AssertSql(
            """
SELECT "t"."Note", "t"."Id", "g0"."Nickname", "g0"."SquadId", "g1"."FullName", "g1"."Nickname", "g1"."SquadId"
FROM "Tags" AS "t"
LEFT JOIN (
    SELECT "g"."Nickname", "g"."SquadId"
    FROM "Gears" AS "g"
    WHERE "g"."Discriminator" = 'Officer'
) AS "g0" ON "t"."GearNickName" = "g0"."Nickname"
LEFT JOIN "Gears" AS "g1" ON ("g0"."Nickname" = "g1"."LeaderNickname" OR ("g0"."Nickname" IS NULL AND "g1"."LeaderNickname" IS NULL)) AND "g0"."SquadId" = "g1"."LeaderSquadId"
ORDER BY "t"."Id", "g0"."Nickname", "g0"."SquadId", "g1"."Nickname"
""");
    }

    public override async Task Streaming_correlated_collection_issue_11403(bool async)
    {
        await base.Streaming_correlated_collection_issue_11403(async);

        AssertSql(
            """
SELECT "g0"."Nickname", "g0"."SquadId", "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
FROM (
    SELECT "g"."Nickname", "g"."SquadId", "g"."FullName"
    FROM "Gears" AS "g"
    ORDER BY "g"."Nickname"
    LIMIT 1
) AS "g0"
LEFT JOIN (
    SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
    FROM "Weapons" AS "w"
    WHERE NOT ("w"."IsAutomatic")
) AS "w0" ON "g0"."FullName" = "w0"."OwnerFullName"
ORDER BY "g0"."Nickname", "g0"."SquadId", "w0"."Id"
""");
    }

    public override async Task Select_subquery_distinct_singleordefault_boolean_with_pushdown(bool async)
    {
        await base.Select_subquery_distinct_singleordefault_boolean_with_pushdown(async);

        AssertSql(
            """
SELECT (
    SELECT "w0"."IsAutomatic"
    FROM (
        SELECT DISTINCT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
        FROM "Weapons" AS "w"
        WHERE "g"."FullName" = "w"."OwnerFullName" AND "w"."Name" IS NOT NULL AND instr("w"."Name", 'Lancer') > 0
    ) AS "w0"
    LIMIT 1)
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch"
""");
    }

    public override async Task Select_null_propagation_negative7(bool async)
    {
        await base.Select_null_propagation_negative7(async);

        AssertSql(
            """
SELECT CASE
    WHEN "g"."LeaderNickname" IS NOT NULL THEN 1
    ELSE NULL
END
FROM "Gears" AS "g"
""");
    }

    public override async Task Enum_flags_closure_typed_as_different_type_generates_correct_parameter_type(bool async)
    {
        await base.Enum_flags_closure_typed_as_different_type_generates_correct_parameter_type(async);

        AssertSql(
            """
@__prm_0='5'

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE @__prm_0 & CAST("g"."Rank" AS INTEGER) = CAST("g"."Rank" AS INTEGER)
""");
    }

    public override async Task Subquery_containing_left_join_projecting_main_from_clause_gets_lifted(bool async)
    {
        await base.Subquery_containing_left_join_projecting_main_from_clause_gets_lifted(async);

        AssertSql(
            """
SELECT "g"."Nickname"
FROM "Gears" AS "g"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName"
ORDER BY "g"."Nickname"
""");
    }

    public override async Task Enum_closure_typed_as_underlying_type_generates_correct_parameter_type(bool async)
    {
        await base.Enum_closure_typed_as_underlying_type_generates_correct_parameter_type(async);

        AssertSql(
            """
@__prm_0='1' (Nullable = true)

SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE @__prm_0 = "w"."AmmunitionType"
""");
    }

    public override async Task Optional_Navigation_Null_Coalesce_To_Clr_Type(bool async)
    {
        await base.Optional_Navigation_Null_Coalesce_To_Clr_Type(async);

        AssertSql(
            """
SELECT COALESCE("w0"."IsAutomatic", 0) AS "IsAutomatic"
FROM "Weapons" AS "w"
LEFT JOIN "Weapons" AS "w0" ON "w"."SynergyWithId" = "w0"."Id"
ORDER BY "w"."Id"
LIMIT 1
""");
    }

    public override async Task Correlated_collections_complex_scenario2(bool async)
    {
        await base.Correlated_collections_complex_scenario2(async);

        AssertSql(
            """
SELECT "g"."FullName", "g"."Nickname", "g"."SquadId", "s1"."FullName", "s1"."Nickname", "s1"."SquadId", "s1"."Id", "s1"."Nickname0", "s1"."SquadId0", "s1"."Id0", "s1"."Nickname00", "s1"."HasSoulPatch", "s1"."SquadId00"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "g0"."FullName", "g0"."Nickname", "g0"."SquadId", "s0"."Id", "s0"."Nickname" AS "Nickname0", "s0"."SquadId" AS "SquadId0", "s0"."Id0", "s0"."Nickname0" AS "Nickname00", "s0"."HasSoulPatch", "s0"."SquadId0" AS "SquadId00", "g0"."LeaderNickname", "g0"."LeaderSquadId"
    FROM "Gears" AS "g0"
    LEFT JOIN (
        SELECT "w"."Id", "g1"."Nickname", "g1"."SquadId", "s"."Id" AS "Id0", "g2"."Nickname" AS "Nickname0", "g2"."HasSoulPatch", "g2"."SquadId" AS "SquadId0", "w"."OwnerFullName"
        FROM "Weapons" AS "w"
        LEFT JOIN "Gears" AS "g1" ON "w"."OwnerFullName" = "g1"."FullName"
        LEFT JOIN "Squads" AS "s" ON "g1"."SquadId" = "s"."Id"
        LEFT JOIN "Gears" AS "g2" ON "s"."Id" = "g2"."SquadId"
    ) AS "s0" ON "g0"."FullName" = "s0"."OwnerFullName"
) AS "s1" ON "g"."Nickname" = "s1"."LeaderNickname" AND "g"."SquadId" = "s1"."LeaderSquadId"
WHERE "g"."Discriminator" = 'Officer'
ORDER BY "g"."Nickname", "g"."SquadId", "s1"."Nickname", "s1"."SquadId", "s1"."Id", "s1"."Nickname0", "s1"."SquadId0", "s1"."Id0", "s1"."Nickname00"
""");
    }

    public override async Task Cast_result_operator_on_subquery_is_properly_lifted_to_a_convert(bool async)
    {
        await base.Cast_result_operator_on_subquery_is_properly_lifted_to_a_convert(async);

        AssertSql(
            """
SELECT "f"."Eradicated"
FROM "Factions" AS "f"
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_conditional_expression(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_conditional_expression(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE CASE
    WHEN "g"."HasSoulPatch" THEN 1
    ELSE 0
END
""");
    }

    public override async Task Select_null_propagation_negative4(bool async)
    {
        await base.Select_null_propagation_negative4(async);

        AssertSql(
            """
SELECT "g0"."Nickname" IS NOT NULL AND "g0"."SquadId" IS NOT NULL, "g0"."Nickname"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."HasSoulPatch"
ORDER BY "g0"."Nickname"
""");
    }

    public override async Task Null_propagation_optimization5(bool async)
    {
        await base.Null_propagation_optimization5(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE CASE
    WHEN "g"."LeaderNickname" IS NOT NULL THEN length("g"."LeaderNickname")
    ELSE NULL
END = 5 AND CASE
    WHEN "g"."LeaderNickname" IS NOT NULL THEN length("g"."LeaderNickname")
    ELSE NULL
END IS NOT NULL
""");
    }

    public override async Task Non_unicode_string_literal_is_used_for_non_unicode_column(bool async)
    {
        await base.Non_unicode_string_literal_is_used_for_non_unicode_column(async);

        AssertSql(
            """
SELECT "c"."Name", "c"."Location", "c"."Nation"
FROM "Cities" AS "c"
WHERE "c"."Location" = 'Unknown'
""");
    }

    public override async Task Include_with_join_multi_level(bool async)
    {
        await base.Include_with_join_multi_level(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "c"."Name", "c"."Location", "c"."Nation", "t"."Id", "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM "Gears" AS "g"
INNER JOIN "Tags" AS "t" ON "g"."SquadId" = "t"."GearSquadId" AND "g"."Nickname" = "t"."GearNickName"
INNER JOIN "Cities" AS "c" ON "g"."CityOfBirthName" = "c"."Name"
LEFT JOIN "Gears" AS "g0" ON "c"."Name" = "g0"."AssignedCityName"
ORDER BY "g"."Nickname", "g"."SquadId", "t"."Id", "c"."Name", "g0"."Nickname"
""");
    }

    public override async Task Include_with_join_collection1(bool async)
    {
        await base.Include_with_join_collection1(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "t"."Id", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Gears" AS "g"
INNER JOIN "Tags" AS "t" ON "g"."SquadId" = "t"."GearSquadId" AND "g"."Nickname" = "t"."GearNickName"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId", "t"."Id"
""");
    }

    public override async Task Join_on_entity_qsre_keys_inner_key_is_navigation(bool async)
    {
        await base.Join_on_entity_qsre_keys_inner_key_is_navigation(async);

        AssertSql(
            """
SELECT "c"."Name" AS "CityName", "s"."Nickname" AS "GearNickname"
FROM "Cities" AS "c"
INNER JOIN (
    SELECT "g"."Nickname", "c0"."Name"
    FROM "Gears" AS "g"
    LEFT JOIN "Cities" AS "c0" ON "g"."AssignedCityName" = "c0"."Name"
) AS "s" ON "c"."Name" = "s"."Name"
""");
    }

    public override async Task Projecting_property_converted_to_nullable_into_member_access(bool async)
    {
        await base.Projecting_property_converted_to_nullable_into_member_access(async);

        AssertSql(
            """
SELECT "g"."Nickname"
FROM "Gears" AS "g"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
WHERE CAST(strftime('%m', "t"."IssueDate") AS INTEGER) <> 5 OR "t"."IssueDate" IS NULL
ORDER BY "g"."Nickname"
""");
    }

    public override async Task Include_collection_OrderBy_aggregate(bool async)
    {
        await base.Include_collection_OrderBy_aggregate(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."Nickname" = "g0"."LeaderNickname" AND "g"."SquadId" = "g0"."LeaderSquadId"
WHERE "g"."Discriminator" = 'Officer'
ORDER BY (
    SELECT COUNT(*)
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName"), "g"."Nickname", "g"."SquadId", "g0"."Nickname"
""");
    }

    public override async Task Multiple_derived_included_on_one_method(bool async)
    {
        await base.Multiple_derived_included_on_one_method(async);

        AssertSql(
            """
SELECT "f"."Id", "f"."CapitalName", "f"."Discriminator", "f"."Name", "f"."ServerAddress", "f"."CommanderName", "f"."Eradicated", "l0"."Name", "l0"."Discriminator", "l0"."LocustHordeId", "l0"."ThreatLevel", "l0"."ThreatLevelByte", "l0"."ThreatLevelNullableByte", "l0"."DefeatedByNickname", "l0"."DefeatedBySquadId", "l0"."HighCommandId", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM "Factions" AS "f"
LEFT JOIN (
    SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId"
    FROM "LocustLeaders" AS "l"
    WHERE "l"."Discriminator" = 'LocustCommander'
) AS "l0" ON "f"."CommanderName" = "l0"."Name"
LEFT JOIN "Gears" AS "g" ON "l0"."DefeatedByNickname" = "g"."Nickname" AND "l0"."DefeatedBySquadId" = "g"."SquadId"
LEFT JOIN "Gears" AS "g0" ON ("g"."Nickname" = "g0"."LeaderNickname" OR ("g"."Nickname" IS NULL AND "g0"."LeaderNickname" IS NULL)) AND "g"."SquadId" = "g0"."LeaderSquadId"
ORDER BY "f"."Id", "l0"."Name", "g"."Nickname", "g"."SquadId", "g0"."Nickname"
""");
    }

    public override async Task Order_by_entity_qsre_composite_key(bool async)
    {
        await base.Order_by_entity_qsre_composite_key(async);

        AssertSql(
            """
SELECT "w"."Name"
FROM "Weapons" AS "w"
LEFT JOIN "Gears" AS "g" ON "w"."OwnerFullName" = "g"."FullName"
ORDER BY "g"."Nickname", "g"."SquadId", "w"."Id"
""");
    }

    public override async Task Time_of_day_datetimeoffset(bool async)
    {
        await base.Time_of_day_datetimeoffset(async);

        AssertSql(
            """
SELECT "m"."Timeline"
FROM "Missions" AS "m"
""");
    }

    public override async Task Contains_on_collection_of_nullable_byte_subquery(bool async)
    {
        await base.Contains_on_collection_of_nullable_byte_subquery(async);

        AssertSql(
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId"
FROM "LocustLeaders" AS "l"
WHERE EXISTS (
    SELECT 1
    FROM "LocustLeaders" AS "l0"
    WHERE "l0"."ThreatLevelNullableByte" = "l"."ThreatLevelNullableByte" OR ("l0"."ThreatLevelNullableByte" IS NULL AND "l"."ThreatLevelNullableByte" IS NULL))
""");
    }

    public override async Task FirstOrDefault_over_int_compared_to_zero(bool async)
    {
        await base.FirstOrDefault_over_int_compared_to_zero(async);

        AssertSql(
            """
SELECT "s"."Name"
FROM "Squads" AS "s"
WHERE "s"."Name" = 'Delta' AND COALESCE((
    SELECT "g"."SquadId"
    FROM "Gears" AS "g"
    WHERE "s"."Id" = "g"."SquadId" AND "g"."HasSoulPatch"
    ORDER BY "g"."FullName"
    LIMIT 1), 0) <> 0
""");
    }

    public override async Task Select_ternary_operation_with_has_value_not_null(bool async)
    {
        await base.Select_ternary_operation_with_has_value_not_null(async);

        AssertSql(
            """
SELECT "w"."Id", CASE
    WHEN "w"."AmmunitionType" IS NOT NULL AND "w"."AmmunitionType" = 1 THEN 'Yes'
    ELSE 'No'
END AS "IsCartridge"
FROM "Weapons" AS "w"
WHERE "w"."AmmunitionType" IS NOT NULL AND "w"."AmmunitionType" = 1
""");
    }

    public override async Task Filter_on_subquery_projecting_one_value_type_from_empty_collection(bool async)
    {
        await base.Filter_on_subquery_projecting_one_value_type_from_empty_collection(async);

        AssertSql(
            """
SELECT "s"."Name"
FROM "Squads" AS "s"
WHERE "s"."Name" = 'Kilo' AND COALESCE((
    SELECT "g"."SquadId"
    FROM "Gears" AS "g"
    WHERE "s"."Id" = "g"."SquadId" AND "g"."HasSoulPatch"
    LIMIT 1), 0) <> 0
""");
    }

    public override async Task Include_on_derived_entity_with_cast(bool async)
    {
        await base.Include_on_derived_entity_with_cast(async);

        AssertSql(
            """
SELECT "f"."Id", "f"."CapitalName", "f"."Discriminator", "f"."Name", "f"."ServerAddress", "f"."CommanderName", "f"."Eradicated", "c"."Name", "c"."Location", "c"."Nation"
FROM "Factions" AS "f"
LEFT JOIN "Cities" AS "c" ON "f"."CapitalName" = "c"."Name"
ORDER BY "f"."Id"
""");
    }

    public override async Task Select_subquery_projecting_multiple_constants_inside_anonymous(bool async)
    {
        await base.Select_subquery_projecting_multiple_constants_inside_anonymous(async);

        AssertSql(
            """
SELECT "s"."Name", "g1"."True1", "g1"."False1", "g1"."c"
FROM "Squads" AS "s"
LEFT JOIN (
    SELECT "g0"."True1", "g0"."False1", "g0"."c", "g0"."SquadId"
    FROM (
        SELECT 1 AS "True1", 0 AS "False1", 1 AS "c", "g"."SquadId", ROW_NUMBER() OVER(PARTITION BY "g"."SquadId" ORDER BY "g"."Nickname", "g"."SquadId") AS "row"
        FROM "Gears" AS "g"
        WHERE "g"."HasSoulPatch"
    ) AS "g0"
    WHERE "g0"."row" <= 1
) AS "g1" ON "s"."Id" = "g1"."SquadId"
""");
    }

    public override async Task Where_nullable_enum_with_null_constant(bool async)
    {
        await base.Where_nullable_enum_with_null_constant(async);

        AssertSql(
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE "w"."AmmunitionType" IS NULL
""");
    }

    public override async Task Include_with_group_by_and_FirstOrDefault_gets_properly_applied(bool async)
    {
        await base.Include_with_group_by_and_FirstOrDefault_gets_properly_applied(async);

        AssertSql(
            """
SELECT "s0"."Nickname", "s0"."SquadId", "s0"."AssignedCityName", "s0"."CityOfBirthName", "s0"."Discriminator", "s0"."FullName", "s0"."HasSoulPatch", "s0"."LeaderNickname", "s0"."LeaderSquadId", "s0"."Rank", "s0"."Name", "s0"."Location", "s0"."Nation"
FROM (
    SELECT "g"."Rank"
    FROM "Gears" AS "g"
    GROUP BY "g"."Rank"
) AS "g1"
LEFT JOIN (
    SELECT "s"."Nickname", "s"."SquadId", "s"."AssignedCityName", "s"."CityOfBirthName", "s"."Discriminator", "s"."FullName", "s"."HasSoulPatch", "s"."LeaderNickname", "s"."LeaderSquadId", "s"."Rank", "s"."Name", "s"."Location", "s"."Nation"
    FROM (
        SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "c"."Name", "c"."Location", "c"."Nation", ROW_NUMBER() OVER(PARTITION BY "g0"."Rank" ORDER BY "g0"."Nickname", "g0"."SquadId", "c"."Name") AS "row"
        FROM "Gears" AS "g0"
        INNER JOIN "Cities" AS "c" ON "g0"."CityOfBirthName" = "c"."Name"
        WHERE "g0"."HasSoulPatch"
    ) AS "s"
    WHERE "s"."row" <= 1
) AS "s0" ON "g1"."Rank" = "s0"."Rank"
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_predicate_negated_complex2(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_predicate_negated_complex2(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE NOT (CASE
    WHEN NOT ("g"."HasSoulPatch") THEN 0
    ELSE "g"."HasSoulPatch"
END)
""");
    }

    public override async Task Correlated_collections_basic_projecting_constant(bool async)
    {
        await base.Correlated_collections_basic_projecting_constant(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "w0"."c", "w0"."Id"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT 'BFG' AS "c", "w"."Id", "w"."OwnerFullName"
    FROM "Weapons" AS "w"
    WHERE "w"."IsAutomatic" OR "w"."Name" <> 'foo' OR "w"."Name" IS NULL
) AS "w0" ON "g"."FullName" = "w0"."OwnerFullName"
WHERE "g"."Nickname" <> 'Marcus'
ORDER BY "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task Where_subquery_boolean_with_pushdown(bool async)
    {
        await base.Where_subquery_boolean_with_pushdown(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE (
    SELECT "w"."IsAutomatic"
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName"
    ORDER BY "w"."Id"
    LIMIT 1)
""");
    }

    public override async Task Where_bitwise_or_enum(bool async)
    {
        await base.Where_bitwise_or_enum(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."Rank" | 2 > 0
""");
    }

    public override async Task Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result(bool async)
    {
        await base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result(async);

        AssertSql(
            """
SELECT "g1"."Nickname", "g1"."SquadId", "g1"."AssignedCityName", "g1"."CityOfBirthName", "g1"."Discriminator", "g1"."FullName", "g1"."HasSoulPatch", "g1"."LeaderNickname", "g1"."LeaderSquadId", "g1"."Rank", "g"."Nickname", "g"."SquadId", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
    FROM "Gears" AS "g0"
    WHERE "g0"."Discriminator" = 'Officer'
) AS "g1" ON "g"."LeaderNickname" = "g1"."Nickname"
LEFT JOIN "Weapons" AS "w" ON "g1"."FullName" = "w"."OwnerFullName"
LEFT JOIN "Weapons" AS "w0" ON "g"."FullName" = "w0"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId", "g1"."Nickname", "g1"."SquadId", "w"."Id"
""");
    }

    public override async Task Composite_key_entity_equal_null(bool async)
    {
        await base.Composite_key_entity_equal_null(async);

        AssertSql(
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId"
FROM "LocustLeaders" AS "l"
LEFT JOIN "Gears" AS "g" ON "l"."DefeatedByNickname" = "g"."Nickname" AND "l"."DefeatedBySquadId" = "g"."SquadId"
WHERE "l"."Discriminator" = 'LocustCommander' AND ("g"."Nickname" IS NULL OR "g"."SquadId" IS NULL)
""");
    }

    public override async Task Coalesce_used_with_non_unicode_string_column_and_constant(bool async)
    {
        await base.Coalesce_used_with_non_unicode_string_column_and_constant(async);

        AssertSql(
            """
SELECT COALESCE("c"."Location", 'Unknown')
FROM "Cities" AS "c"
""");
    }

    public override async Task SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector(bool async)
    {
        await base.SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector(async);

        AssertSql(
            """
@__isAutomatic_0='True'

SELECT "g"."Nickname", "g"."FullName", "w0"."Id" IS NOT NULL AS "Collection"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "w"."Id", "w"."OwnerFullName"
    FROM "Weapons" AS "w"
    WHERE "w"."IsAutomatic" = @__isAutomatic_0
) AS "w0" ON "g"."FullName" = "w0"."OwnerFullName"
""");
    }

    public override async Task Join_with_order_by_without_skip_or_take_nested(bool async)
    {
        await base.Join_with_order_by_without_skip_or_take_nested(async);

        AssertSql(
            """
SELECT "w"."Name", "g"."FullName"
FROM "Squads" AS "s"
INNER JOIN "Gears" AS "g" ON "s"."Id" = "g"."SquadId"
INNER JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
""");
    }

    public override async Task Select_Where_Navigation(bool async)
    {
        await base.Select_Where_Navigation(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE "g"."Nickname" = 'Marcus'
""");
    }

    public override async Task Navigation_access_on_derived_entity_using_cast(bool async)
    {
        await base.Navigation_access_on_derived_entity_using_cast(async);

        AssertSql(
            """
SELECT "f"."Name", "l0"."ThreatLevel" AS "Threat"
FROM "Factions" AS "f"
LEFT JOIN (
    SELECT "l"."Name", "l"."ThreatLevel"
    FROM "LocustLeaders" AS "l"
    WHERE "l"."Discriminator" = 'LocustCommander'
) AS "l0" ON "f"."CommanderName" = "l0"."Name"
ORDER BY "f"."Name"
""");
    }

    public override async Task Join_predicate_condition_equals_condition(bool async)
    {
        await base.Join_predicate_condition_equals_condition(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
INNER JOIN "Weapons" AS "w" ON "w"."SynergyWithId" IS NOT NULL
""");
    }

    public override async Task SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector_not_equal(bool async)
    {
        await base.SelectMany_Where_DefaultIfEmpty_with_navigation_in_the_collection_selector_not_equal(async);

        AssertSql(
            """
@__isAutomatic_0='True'

SELECT "g"."Nickname", "g"."FullName", "w0"."Id" IS NOT NULL AS "Collection"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "w"."Id", "w"."OwnerFullName"
    FROM "Weapons" AS "w"
    WHERE "w"."IsAutomatic" <> @__isAutomatic_0
) AS "w0" ON "g"."FullName" = "w0"."OwnerFullName"
""");
    }

    public override async Task Correlated_collections_with_funky_orderby_complex_scenario2(bool async)
    {
        await base.Correlated_collections_with_funky_orderby_complex_scenario2(async);

        AssertSql(
            """
SELECT "g"."FullName", "g"."Nickname", "g"."SquadId", "s1"."FullName", "s1"."Nickname", "s1"."SquadId", "s1"."Id", "s1"."Nickname0", "s1"."SquadId0", "s1"."Id0", "s1"."Nickname00", "s1"."HasSoulPatch", "s1"."SquadId00"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "g0"."FullName", "g0"."Nickname", "g0"."SquadId", "s0"."Id", "s0"."Nickname" AS "Nickname0", "s0"."SquadId" AS "SquadId0", "s0"."Id0", "s0"."Nickname0" AS "Nickname00", "s0"."HasSoulPatch", "s0"."SquadId0" AS "SquadId00", "g0"."HasSoulPatch" AS "HasSoulPatch0", "s0"."IsAutomatic", "s0"."Name", "g0"."LeaderNickname", "g0"."LeaderSquadId"
    FROM "Gears" AS "g0"
    LEFT JOIN (
        SELECT "w"."Id", "g1"."Nickname", "g1"."SquadId", "s"."Id" AS "Id0", "g2"."Nickname" AS "Nickname0", "g2"."HasSoulPatch", "g2"."SquadId" AS "SquadId0", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName"
        FROM "Weapons" AS "w"
        LEFT JOIN "Gears" AS "g1" ON "w"."OwnerFullName" = "g1"."FullName"
        LEFT JOIN "Squads" AS "s" ON "g1"."SquadId" = "s"."Id"
        LEFT JOIN "Gears" AS "g2" ON "s"."Id" = "g2"."SquadId"
    ) AS "s0" ON "g0"."FullName" = "s0"."OwnerFullName"
) AS "s1" ON "g"."Nickname" = "s1"."LeaderNickname" AND "g"."SquadId" = "s1"."LeaderSquadId"
WHERE "g"."Discriminator" = 'Officer'
ORDER BY "g"."HasSoulPatch", "g"."LeaderNickname", "g"."FullName", "g"."Nickname", "g"."SquadId", "s1"."FullName", "s1"."HasSoulPatch0" DESC, "s1"."Nickname", "s1"."SquadId", "s1"."IsAutomatic", "s1"."Name" DESC, "s1"."Id", "s1"."Nickname0", "s1"."SquadId0", "s1"."Id0", "s1"."Nickname00"
""");
    }

    public override async Task Where_count_subquery_without_collision(bool async)
    {
        await base.Where_count_subquery_without_collision(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE (
    SELECT COUNT(*)
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName") = 2
""");
    }

    public override async Task Enum_matching_take_value_gets_different_type_mapping(bool async)
    {
        await base.Enum_matching_take_value_gets_different_type_mapping(async);

        AssertSql(
            """
@__value_1='1'
@__p_0='1'

SELECT "g"."Rank" & @__value_1
FROM "Gears" AS "g"
ORDER BY "g"."Nickname"
LIMIT @__p_0
""");
    }

    public override async Task Group_by_nullable_property_HasValue_and_project_the_grouping_key(bool async)
    {
        await base.Group_by_nullable_property_HasValue_and_project_the_grouping_key(async);

        AssertSql(
            """
SELECT "w0"."Key"
FROM (
    SELECT "w"."SynergyWithId" IS NOT NULL AS "Key"
    FROM "Weapons" AS "w"
) AS "w0"
GROUP BY "w0"."Key"
""");
    }

    public override async Task Query_with_complex_let_containing_ordering_and_filter_projecting_firstOrDefault_element_of_let(bool async)
    {
        await base.Query_with_complex_let_containing_ordering_and_filter_projecting_firstOrDefault_element_of_let(async);

        AssertSql(
            """
SELECT "g"."Nickname", (
    SELECT "w"."Name"
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName" AND "w"."IsAutomatic"
    ORDER BY "w"."AmmunitionType" DESC
    LIMIT 1) AS "WeaponName"
FROM "Gears" AS "g"
WHERE "g"."Nickname" <> 'Dom'
""");
    }

    public override async Task Subquery_is_lifted_from_additional_from_clause(bool async)
    {
        await base.Subquery_is_lifted_from_additional_from_clause(async);

        AssertSql(
            """
SELECT "g"."FullName" AS "Name1", "g0"."FullName" AS "Name2"
FROM "Gears" AS "g"
CROSS JOIN "Gears" AS "g0"
WHERE "g"."HasSoulPatch" AND NOT ("g0"."HasSoulPatch")
ORDER BY "g"."FullName"
""");
    }

    public override async Task Contains_with_local_nullable_guid_list_closure(bool async)
    {
        await base.Contains_with_local_nullable_guid_list_closure(async);

        AssertSql(
            """
@__ids_0='["DF36F493-463F-4123-83F9-6B135DEEB7BA","23CBCF9B-CE14-45CF-AAFA-2C2667EBFDD3","AB1B82D7-88DB-42BD-A132-7EEF9AA68AF4"]' (Size = 118)

SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Tags" AS "t"
WHERE "t"."Id" IN (
    SELECT "i"."value"
    FROM json_each(@__ids_0) AS "i"
)
""");
    }

    public override async Task Projecting_property_converted_to_nullable_with_addition_and_final_projection(bool async)
    {
        await base.Projecting_property_converted_to_nullable_with_addition_and_final_projection(async);

        AssertSql(
            """
SELECT "t"."Note", CASE
    WHEN "t"."GearNickName" IS NOT NULL THEN "g"."SquadId"
    ELSE NULL
END + 1 AS "Value"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE CASE
    WHEN "t"."GearNickName" IS NOT NULL THEN "g"."Nickname"
    ELSE NULL
END IS NOT NULL
""");
    }

    public override async Task Correlated_collections_projection_of_collection_thru_navigation(bool async)
    {
        await base.Correlated_collections_projection_of_collection_thru_navigation(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "s"."Id", "s1"."SquadId", "s1"."MissionId"
FROM "Gears" AS "g"
INNER JOIN "Squads" AS "s" ON "g"."SquadId" = "s"."Id"
LEFT JOIN (
    SELECT "s0"."SquadId", "s0"."MissionId"
    FROM "SquadMissions" AS "s0"
    WHERE "s0"."MissionId" <> 17
) AS "s1" ON "s"."Id" = "s1"."SquadId"
WHERE "g"."Nickname" <> 'Marcus'
ORDER BY "g"."FullName", "g"."Nickname", "g"."SquadId", "s"."Id", "s1"."SquadId"
""");
    }

    public override async Task Select_null_conditional_with_inheritance_negative(bool async)
    {
        await base.Select_null_conditional_with_inheritance_negative(async);

        AssertSql(
            """
SELECT CASE
    WHEN "f"."CommanderName" IS NOT NULL THEN "f"."Eradicated"
    ELSE NULL
END
FROM "Factions" AS "f"
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_predicate2(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_predicate2(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE "g"."HasSoulPatch"
""");
    }

    public override async Task Conditional_expression_with_test_being_simplified_to_constant_simple(bool async)
    {
        await base.Conditional_expression_with_test_being_simplified_to_constant_simple(async);

        AssertSql(
            """
@__prm_0='True'

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE CASE
    WHEN "g"."HasSoulPatch" = @__prm_0 THEN 1
    ELSE 0
END
""");
    }

    public override async Task OrderBy_same_expression_containing_IsNull_correctly_deduplicates_the_ordering(bool async)
    {
        await base.OrderBy_same_expression_containing_IsNull_correctly_deduplicates_the_ordering(async);

        AssertSql(
            """
SELECT CASE
    WHEN "g"."LeaderNickname" IS NOT NULL THEN length("g"."Nickname") = 5
    ELSE NULL
END
FROM "Gears" AS "g"
ORDER BY CASE
    WHEN "g"."LeaderNickname" IS NOT NULL THEN length("g"."Nickname") = 5
    ELSE NULL
END IS NOT NULL
""");
    }

    public override async Task Enum_flags_closure_typed_as_underlying_type_generates_correct_parameter_type(bool async)
    {
        await base.Enum_flags_closure_typed_as_underlying_type_generates_correct_parameter_type(async);

        AssertSql(
            """
@__prm_0='133'

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE @__prm_0 & "g"."Rank" = "g"."Rank"
""");
    }

    public override async Task Include_with_join_and_inheritance2(bool async)
    {
        await base.Include_with_join_and_inheritance2(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "t"."Id", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Gears" AS "g"
INNER JOIN "Tags" AS "t" ON "g"."SquadId" = "t"."GearSquadId" AND "g"."Nickname" = "t"."GearNickName"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
WHERE "g"."Discriminator" = 'Officer'
ORDER BY "g"."Nickname", "g"."SquadId", "t"."Id"
""");
    }

    public override async Task Include_on_entity_that_is_not_present_in_final_projection_but_uses_TypeIs_instead(bool async)
    {
        await base.Include_on_entity_that_is_not_present_in_final_projection_but_uses_TypeIs_instead(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."Discriminator" = 'Officer' AS "IsOfficer"
FROM "Gears" AS "g"
""");
    }

    public override async Task Select_comparison_with_null(bool async)
    {
        await base.Select_comparison_with_null(async);

        AssertSql(
            """
@__ammunitionType_0='1' (Nullable = true)

SELECT "w"."Id", "w"."AmmunitionType" = @__ammunitionType_0 AND "w"."AmmunitionType" IS NOT NULL AS "Cartridge"
FROM "Weapons" AS "w"
WHERE "w"."AmmunitionType" = @__ammunitionType_0
""",
            //
            """
SELECT "w"."Id", "w"."AmmunitionType" IS NULL AS "Cartridge"
FROM "Weapons" AS "w"
WHERE "w"."AmmunitionType" IS NULL
""");
    }

    public override async Task OfTypeNav2(bool async)
    {
        await base.OfTypeNav2(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
LEFT JOIN "Cities" AS "c" ON "g"."AssignedCityName" = "c"."Name"
WHERE ("t"."Note" <> 'Foo' OR "t"."Note" IS NULL) AND "g"."Discriminator" = 'Officer' AND ("c"."Location" <> 'Bar' OR "c"."Location" IS NULL)
""");
    }

    public override async Task Correlated_collections_basic_projection(bool async)
    {
        await base.Correlated_collections_basic_projection(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
    FROM "Weapons" AS "w"
    WHERE "w"."IsAutomatic" OR "w"."Name" <> 'foo' OR "w"."Name" IS NULL
) AS "w0" ON "g"."FullName" = "w0"."OwnerFullName"
WHERE "g"."Nickname" <> 'Marcus'
ORDER BY "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task Null_propagation_optimization4(bool async)
    {
        await base.Null_propagation_optimization4(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE CASE
    WHEN "g"."LeaderNickname" IS NULL THEN NULL
    ELSE length("g"."LeaderNickname")
END = 5 AND CASE
    WHEN "g"."LeaderNickname" IS NULL THEN NULL
    ELSE length("g"."LeaderNickname")
END IS NOT NULL
""");
    }

    public override async Task Projecting_required_string_column_compared_to_null_parameter(bool async)
    {
        await base.Projecting_required_string_column_compared_to_null_parameter(async);

        AssertSql(
            """
SELECT 0
FROM "Gears" AS "g"
""");
    }

    public override async Task Select_Where_Navigation_Null(bool async)
    {
        await base.Select_Where_Navigation_Null(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE "g"."Nickname" IS NULL OR "g"."SquadId" IS NULL
""");
    }

    public override async Task Where_subquery_distinct_firstordefault_boolean_with_pushdown(bool async)
    {
        await base.Where_subquery_distinct_firstordefault_boolean_with_pushdown(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."HasSoulPatch" AND (
    SELECT "w0"."IsAutomatic"
    FROM (
        SELECT DISTINCT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
        FROM "Weapons" AS "w"
        WHERE "g"."FullName" = "w"."OwnerFullName"
    ) AS "w0"
    ORDER BY "w0"."Id"
    LIMIT 1)
""");
    }

    public override async Task Select_null_propagation_negative5(bool async)
    {
        await base.Select_null_propagation_negative5(async);

        AssertSql(
            """
SELECT "g0"."Nickname" IS NOT NULL AND "g0"."SquadId" IS NOT NULL, "g0"."Nickname"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."HasSoulPatch"
ORDER BY "g0"."Nickname"
""");
    }

    public override async Task ThenInclude_collection_on_derived_after_base_reference(bool async)
    {
        await base.ThenInclude_collection_on_derived_after_base_reference(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
ORDER BY "t"."Id", "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task Project_collection_navigation_with_inheritance2(bool async)
    {
        await base.Project_collection_navigation_with_inheritance2(async);

        AssertSql(
            """
SELECT "f"."Id", "l0"."Name", "g"."Nickname", "g"."SquadId", "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM "Factions" AS "f"
LEFT JOIN (
    SELECT "l"."Name", "l"."DefeatedByNickname", "l"."DefeatedBySquadId"
    FROM "LocustLeaders" AS "l"
    WHERE "l"."Discriminator" = 'LocustCommander'
) AS "l0" ON "f"."CommanderName" = "l0"."Name"
LEFT JOIN "Gears" AS "g" ON "l0"."DefeatedByNickname" = "g"."Nickname" AND "l0"."DefeatedBySquadId" = "g"."SquadId"
LEFT JOIN "Gears" AS "g0" ON ("g"."Nickname" = "g0"."LeaderNickname" OR ("g"."Nickname" IS NULL AND "g0"."LeaderNickname" IS NULL)) AND "g"."SquadId" = "g0"."LeaderSquadId"
ORDER BY "f"."Id", "l0"."Name", "g"."Nickname", "g"."SquadId", "g0"."Nickname"
""");
    }

    public override async Task Checked_context_with_addition_does_not_fail(bool async)
    {
        await base.Checked_context_with_addition_does_not_fail(async);

        AssertSql(
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId"
FROM "LocustLeaders" AS "l"
WHERE CAST("l"."ThreatLevel" AS INTEGER) <= 5 + CAST("l"."ThreatLevel" AS INTEGER)
""");
    }

    public override async Task Concat_scalars_with_count(bool async)
    {
        await base.Concat_scalars_with_count(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM (
    SELECT 1
    FROM "Gears" AS "g"
    UNION ALL
    SELECT 1
    FROM "Gears" AS "g0"
) AS "u"
""");
    }

    public override async Task Cast_ordered_subquery_to_base_type_using_typed_ToArray(bool async)
    {
        await base.Cast_ordered_subquery_to_base_type_using_typed_ToArray(async);

        AssertSql(
            """
SELECT "c"."Name", "g"."CityOfBirthName", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Nickname", "g"."Rank", "g"."SquadId"
FROM "Cities" AS "c"
LEFT JOIN "Gears" AS "g" ON "c"."Name" = "g"."AssignedCityName"
WHERE "c"."Name" = 'Ephyra'
ORDER BY "c"."Name", "g"."Nickname" DESC
""");
    }

    public override async Task TimeSpan_Milliseconds(bool async)
    {
        await base.TimeSpan_Milliseconds(async);

        AssertSql(
            """
SELECT "m"."Duration"
FROM "Missions" AS "m"
""");
    }

    public override async Task Double_order_by_on_is_null(bool async)
    {
        await base.Double_order_by_on_is_null(async);

        AssertSql(
            """
SELECT "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
FROM "Weapons" AS "w"
LEFT JOIN "Weapons" AS "w0" ON "w"."SynergyWithId" = "w0"."Id"
ORDER BY "w0"."Name" IS NULL
""");
    }

    public override async Task Select_ternary_operation_multiple_conditions_2(bool async)
    {
        await base.Select_ternary_operation_multiple_conditions_2(async);

        AssertSql(
            """
SELECT "w"."Id", CASE
    WHEN NOT ("w"."IsAutomatic") AND "w"."SynergyWithId" = 1 THEN 'Yes'
    ELSE 'No'
END AS "IsCartridge"
FROM "Weapons" AS "w"
""");
    }

    public override async Task Select_subquery_projecting_single_constant_inside_anonymous(bool async)
    {
        await base.Select_subquery_projecting_single_constant_inside_anonymous(async);

        AssertSql(
            """
SELECT "s"."Name", "g1"."One"
FROM "Squads" AS "s"
LEFT JOIN (
    SELECT "g0"."One", "g0"."SquadId"
    FROM (
        SELECT 1 AS "One", "g"."SquadId", ROW_NUMBER() OVER(PARTITION BY "g"."SquadId" ORDER BY "g"."Nickname", "g"."SquadId") AS "row"
        FROM "Gears" AS "g"
        WHERE "g"."HasSoulPatch"
    ) AS "g0"
    WHERE "g0"."row" <= 1
) AS "g1" ON "s"."Id" = "g1"."SquadId"
""");
    }

    public override async Task Null_propagation_optimization1(bool async)
    {
        await base.Null_propagation_optimization1(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE "g"."LeaderNickname" = 'Marcus' AND "g"."LeaderNickname" IS NOT NULL
""");
    }

    public override async Task Project_collection_navigation_with_inheritance3(bool async)
    {
        await base.Project_collection_navigation_with_inheritance3(async);

        AssertSql(
            """
SELECT "f"."Id", "l0"."Name", "g"."Nickname", "g"."SquadId", "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM "Factions" AS "f"
LEFT JOIN (
    SELECT "l"."Name", "l"."DefeatedByNickname", "l"."DefeatedBySquadId"
    FROM "LocustLeaders" AS "l"
    WHERE "l"."Discriminator" = 'LocustCommander'
) AS "l0" ON "f"."CommanderName" = "l0"."Name"
LEFT JOIN "Gears" AS "g" ON "l0"."DefeatedByNickname" = "g"."Nickname" AND "l0"."DefeatedBySquadId" = "g"."SquadId"
LEFT JOIN "Gears" AS "g0" ON ("g"."Nickname" = "g0"."LeaderNickname" OR ("g"."Nickname" IS NULL AND "g0"."LeaderNickname" IS NULL)) AND "g"."SquadId" = "g0"."LeaderSquadId"
ORDER BY "f"."Id", "l0"."Name", "g"."Nickname", "g"."SquadId", "g0"."Nickname"
""");
    }

    public override async Task Include_using_alternate_key(bool async)
    {
        await base.Include_using_alternate_key(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
WHERE "g"."Nickname" = 'Marcus'
ORDER BY "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task Comparing_two_collection_navigations_composite_key(bool async)
    {
        await base.Comparing_two_collection_navigations_composite_key(async);

        AssertSql(
            """
SELECT "g"."Nickname" AS "Nickname1", "g0"."Nickname" AS "Nickname2"
FROM "Gears" AS "g"
CROSS JOIN "Gears" AS "g0"
WHERE "g"."Nickname" = "g0"."Nickname" AND "g"."SquadId" = "g0"."SquadId"
ORDER BY "g"."Nickname"
""");
    }

    public override async Task Include_with_projection_of_unmapped_property_still_gets_applied(bool async)
    {
        await base.Include_with_projection_of_unmapped_property_still_gets_applied(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task Where_subquery_distinct_lastordefault_boolean(bool async)
    {
        await base.Where_subquery_distinct_lastordefault_boolean(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE NOT ((
    SELECT "w0"."IsAutomatic"
    FROM (
        SELECT DISTINCT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
        FROM "Weapons" AS "w"
        WHERE "g"."FullName" = "w"."OwnerFullName"
    ) AS "w0"
    ORDER BY "w0"."Id" DESC
    LIMIT 1))
ORDER BY "g"."Nickname"
""");
    }

    public override async Task Select_null_propagation_negative2(bool async)
    {
        await base.Select_null_propagation_negative2(async);

        AssertSql(
            """
SELECT CASE
    WHEN "g"."LeaderNickname" IS NOT NULL THEN "g0"."LeaderNickname"
    ELSE NULL
END
FROM "Gears" AS "g"
CROSS JOIN "Gears" AS "g0"
""");
    }

    public override async Task Correlated_collections_nested_with_custom_ordering(bool async)
    {
        await base.Correlated_collections_nested_with_custom_ordering(async);

        AssertSql(
            """
SELECT "g"."FullName", "g"."Nickname", "g"."SquadId", "s"."FullName", "s"."Nickname", "s"."SquadId", "s"."Id", "s"."AmmunitionType", "s"."IsAutomatic", "s"."Name", "s"."OwnerFullName", "s"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "g0"."FullName", "g0"."Nickname", "g0"."SquadId", "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId", "g0"."Rank", "g0"."LeaderNickname", "g0"."LeaderSquadId"
    FROM "Gears" AS "g0"
    LEFT JOIN (
        SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
        FROM "Weapons" AS "w"
        WHERE "w"."Name" <> 'Bar' OR "w"."Name" IS NULL
    ) AS "w0" ON "g0"."FullName" = "w0"."OwnerFullName"
    WHERE "g0"."FullName" <> 'Foo'
) AS "s" ON "g"."Nickname" = "s"."LeaderNickname" AND "g"."SquadId" = "s"."LeaderSquadId"
WHERE "g"."Discriminator" = 'Officer'
ORDER BY "g"."HasSoulPatch" DESC, "g"."Nickname", "g"."SquadId", "s"."Rank", "s"."Nickname", "s"."SquadId", "s"."IsAutomatic"
""");
    }

    public override async Task Include_collection_on_derived_type_using_lambda(bool async)
    {
        await base.Include_collection_on_derived_type_using_lambda(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."Nickname" = "g0"."LeaderNickname" AND "g"."SquadId" = "g0"."LeaderSquadId"
ORDER BY "g"."Nickname", "g"."SquadId", "g0"."Nickname"
""");
    }

    public override async Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar(bool async)
    {
        await base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note", "t0"."Id", "t0"."GearNickName", "t0"."GearSquadId", "t0"."IssueDate", "t0"."Note"
FROM "Tags" AS "t"
CROSS JOIN "Tags" AS "t0"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
LEFT JOIN "Gears" AS "g0" ON "t0"."GearNickName" = "g0"."Nickname" AND "t0"."GearSquadId" = "g0"."SquadId"
WHERE "g"."Nickname" = "g0"."Nickname" OR ("g"."Nickname" IS NULL AND "g0"."Nickname" IS NULL)
""");
    }

    public override async Task Conditional_with_conditions_evaluating_to_false_gets_optimized(bool async)
    {
        await base.Conditional_with_conditions_evaluating_to_false_gets_optimized(async);

        AssertSql(
            """
SELECT "g"."FullName"
FROM "Gears" AS "g"
""");
    }

    public override async Task Order_by_entity_qsre_with_other_orderbys(bool async)
    {
        await base.Order_by_entity_qsre_with_other_orderbys(async);

        AssertSql(
            """
SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
LEFT JOIN "Gears" AS "g" ON "w"."OwnerFullName" = "g"."FullName"
LEFT JOIN "Weapons" AS "w0" ON "w"."SynergyWithId" = "w0"."Id"
ORDER BY "w"."IsAutomatic", "g"."Nickname" DESC, "g"."SquadId" DESC, "w0"."Id", "w"."Name"
""");
    }

    public override async Task GroupBy_Select_sum(bool async)
    {
        await base.GroupBy_Select_sum(async);

        AssertSql(
            """
SELECT COALESCE(SUM("m"."Rating"), 0.0)
FROM "Missions" AS "m"
GROUP BY "m"."CodeName"
""");
    }

    public override async Task Join_on_entity_qsre_keys_composite_key(bool async)
    {
        await base.Join_on_entity_qsre_keys_composite_key(async);

        AssertSql(
            """
SELECT "g"."FullName" AS "GearName1", "g0"."FullName" AS "GearName2"
FROM "Gears" AS "g"
INNER JOIN "Gears" AS "g0" ON "g"."Nickname" = "g0"."Nickname" AND "g"."SquadId" = "g0"."SquadId"
""");
    }

    public override async Task Select_null_propagation_optimization9(bool async)
    {
        await base.Select_null_propagation_optimization9(async);

        AssertSql(
            """
SELECT length("g"."FullName")
FROM "Gears" AS "g"
""");
    }

    public override async Task Include_with_complex_order_by(bool async)
    {
        await base.Include_with_complex_order_by(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN "Weapons" AS "w0" ON "g"."FullName" = "w0"."OwnerFullName"
ORDER BY (
    SELECT "w"."Name"
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName" AND "w"."Name" IS NOT NULL AND instr("w"."Name", 'Gnasher') > 0
    LIMIT 1), "g"."Nickname", "g"."SquadId"
""");
    }

    public override async Task Multiple_includes_with_client_method_around_entity_and_also_projecting_included_collection()
    {
        await base.Multiple_includes_with_client_method_around_entity_and_also_projecting_included_collection();

        AssertSql(
            """
SELECT "s"."Name", "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s0"."Nickname", "s0"."SquadId", "s0"."AssignedCityName", "s0"."CityOfBirthName", "s0"."Discriminator", "s0"."FullName", "s0"."HasSoulPatch", "s0"."LeaderNickname", "s0"."LeaderSquadId", "s0"."Rank", "s0"."Id", "s0"."AmmunitionType", "s0"."IsAutomatic", "s0"."Name", "s0"."OwnerFullName", "s0"."SynergyWithId"
FROM "Squads" AS "s"
LEFT JOIN (
    SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
    FROM "Gears" AS "g"
    LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
) AS "s0" ON "s"."Id" = "s0"."SquadId"
WHERE "s"."Name" = 'Delta'
ORDER BY "s"."Id", "s0"."Nickname", "s0"."SquadId"
""");
    }

    public override async Task Select_Where_Navigation_Equals_Navigation(bool async)
    {
        await base.Select_Where_Navigation_Equals_Navigation(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note", "t0"."Id", "t0"."GearNickName", "t0"."GearSquadId", "t0"."IssueDate", "t0"."Note"
FROM "Tags" AS "t"
CROSS JOIN "Tags" AS "t0"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
LEFT JOIN "Gears" AS "g0" ON "t0"."GearNickName" = "g0"."Nickname" AND "t0"."GearSquadId" = "g0"."SquadId"
WHERE ("g"."Nickname" = "g0"."Nickname" OR ("g"."Nickname" IS NULL AND "g0"."Nickname" IS NULL)) AND ("g"."SquadId" = "g0"."SquadId" OR ("g"."SquadId" IS NULL AND "g0"."SquadId" IS NULL))
""");
    }

    public override async Task Complex_GroupBy_after_set_operator_using_result_selector(bool async)
    {
        await base.Complex_GroupBy_after_set_operator_using_result_selector(async);

        AssertSql(
            """
SELECT "u"."Name", "u"."Count", COALESCE(SUM("u"."Count"), 0) AS "Sum"
FROM (
    SELECT "c"."Name", (
        SELECT COUNT(*)
        FROM "Weapons" AS "w"
        WHERE "g"."FullName" = "w"."OwnerFullName") AS "Count"
    FROM "Gears" AS "g"
    LEFT JOIN "Cities" AS "c" ON "g"."AssignedCityName" = "c"."Name"
    UNION ALL
    SELECT "c0"."Name", (
        SELECT COUNT(*)
        FROM "Weapons" AS "w0"
        WHERE "g0"."FullName" = "w0"."OwnerFullName") AS "Count"
    FROM "Gears" AS "g0"
    INNER JOIN "Cities" AS "c0" ON "g0"."CityOfBirthName" = "c0"."Name"
) AS "u"
GROUP BY "u"."Name", "u"."Count"
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_projection(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_projection(async);

        AssertSql(
            """
SELECT "g"."SquadId"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE "t"."Note" <> 'K.I.A.' OR "t"."Note" IS NULL
""");
    }

    public override async Task Cast_to_derived_type_causes_client_eval(bool async)
    {
        await base.Cast_to_derived_type_causes_client_eval(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
""");
    }

    public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2()
    {
        base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2();

        AssertSql(
            """
SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "g"."Nickname", "g"."SquadId", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."LeaderNickname" = "g0"."Nickname"
LEFT JOIN "Weapons" AS "w" ON "g0"."FullName" = "w"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId", "g0"."Nickname", "g0"."SquadId"
""");
    }

    public override async Task Select_conditional_with_anonymous_types(bool async)
    {
        await base.Select_conditional_with_anonymous_types(async);

        AssertSql(
            """
SELECT "g"."LeaderNickname" IS NOT NULL, "g"."Nickname", "g"."FullName"
FROM "Gears" AS "g"
ORDER BY "g"."Nickname"
""");
    }

    public override async Task Include_collection_with_complex_OrderBy2(bool async)
    {
        await base.Include_collection_with_complex_OrderBy2(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."Nickname" = "g0"."LeaderNickname" AND "g"."SquadId" = "g0"."LeaderSquadId"
WHERE "g"."Discriminator" = 'Officer'
ORDER BY (
    SELECT "w"."IsAutomatic"
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName"
    ORDER BY "w"."Id"
    LIMIT 1), "g"."Nickname", "g"."SquadId", "g0"."Nickname"
""");
    }

    public override async Task Select_Where_Navigation_Included(bool async)
    {
        await base.Select_Where_Navigation_Included(async);

        AssertSql(
            """
SELECT "t"."Id", "t"."GearNickName", "t"."GearSquadId", "t"."IssueDate", "t"."Note", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE "g"."Nickname" = 'Marcus'
""");
    }

    public override async Task Optional_navigation_type_compensation_works_with_list_initializers(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_list_initializers(async);

        AssertSql(
            """
SELECT "g"."SquadId", "g"."SquadId" + 1
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
WHERE "t"."Note" <> 'K.I.A.' OR "t"."Note" IS NULL
ORDER BY "t"."Note"
""");
    }

    public override async Task Projecting_property_converted_to_nullable_with_conditional(bool async)
    {
        await base.Projecting_property_converted_to_nullable_with_conditional(async);

        AssertSql(
            """
SELECT CASE
    WHEN "t"."Note" <> 'K.I.A.' OR "t"."Note" IS NULL THEN CASE
        WHEN "t"."GearNickName" IS NOT NULL THEN "g"."SquadId"
        ELSE NULL
    END
    ELSE -1
END
FROM "Tags" AS "t"
LEFT JOIN "Gears" AS "g" ON "t"."GearNickName" = "g"."Nickname" AND "t"."GearSquadId" = "g"."SquadId"
""");
    }

    public override async Task All_with_optional_navigation_is_translated_to_sql(bool async)
    {
        await base.All_with_optional_navigation_is_translated_to_sql(async);

        AssertSql(
            """
SELECT NOT EXISTS (
    SELECT 1
    FROM "Gears" AS "g"
    LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
    WHERE "t"."Note" = 'Foo' AND "t"."Note" IS NOT NULL)
""");
    }

    public override async Task Count_with_optional_navigation_is_translated_to_sql(bool async)
    {
        await base.Count_with_optional_navigation_is_translated_to_sql(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM "Gears" AS "g"
LEFT JOIN "Tags" AS "t" ON "g"."Nickname" = "t"."GearNickName" AND "g"."SquadId" = "t"."GearSquadId"
WHERE "t"."Note" <> 'Foo' OR "t"."Note" IS NULL
""");
    }

    public override async Task Select_null_parameter(bool async)
    {
        await base.Select_null_parameter(async);

        AssertSql(
            """
@__ammunitionType_0='1' (Nullable = true)

SELECT "w"."Id", @__ammunitionType_0 AS "AmmoType"
FROM "Weapons" AS "w"
""",
            //
            """
SELECT "w"."Id", NULL AS "AmmoType"
FROM "Weapons" AS "w"
""",
            //
            """
@__ammunitionType_0='2' (Nullable = true)

SELECT "w"."Id", @__ammunitionType_0 AS "AmmoType"
FROM "Weapons" AS "w"
""",
            //
            """
SELECT "w"."Id", NULL AS "AmmoType"
FROM "Weapons" AS "w"
""");
    }

    public override async Task Project_shadow_properties(bool async)
    {
        await base.Project_shadow_properties(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."AssignedCityName"
FROM "Gears" AS "g"
""");
    }

    public override async Task Select_enum_has_flag(bool async)
    {
        await base.Select_enum_has_flag(async);

        AssertSql(
            """
SELECT "g"."Rank" & 2 = 2 AS "hasFlagTrue", "g"."Rank" & 4 = 4 AS "hasFlagFalse"
FROM "Gears" AS "g"
WHERE "g"."Rank" & 2 = 2
LIMIT 1
""");
    }

    public override async Task
        Correlated_collection_with_groupby_with_complex_grouping_key_not_projecting_identifier_column_with_group_aggregate_in_final_projection(
            bool async)
    {
        Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base
                    .Correlated_collection_with_groupby_with_complex_grouping_key_not_projecting_identifier_column_with_group_aggregate_in_final_projection(
                        async))).Message);

        AssertSql();
    }

    public override async Task Correlated_collection_with_distinct_not_projecting_identifier_column_also_projecting_complex_expressions(
        bool async)
    {
        await base.Correlated_collection_with_distinct_not_projecting_identifier_column_also_projecting_complex_expressions(async);

        AssertSql();
    }

    public override async Task Client_eval_followed_by_aggregate_operation(bool async)
    {
        await base.Client_eval_followed_by_aggregate_operation(async);

        AssertSql();
    }

    public override async Task Client_member_and_unsupported_string_Equals_in_the_same_query(bool async)
    {
        await base.Client_member_and_unsupported_string_Equals_in_the_same_query(async);

        AssertSql();
    }

    public override async Task Client_side_equality_with_parameter_works_with_optional_navigations(bool async)
    {
        await base.Client_side_equality_with_parameter_works_with_optional_navigations(async);

        AssertSql();
    }

    public override async Task Correlated_collection_order_by_constant_null_of_non_mapped_type(bool async)
    {
        await base.Correlated_collection_order_by_constant_null_of_non_mapped_type(async);

        AssertSql();
    }

    public override async Task GetValueOrDefault_on_DateTimeOffset(bool async)
    {
        await base.GetValueOrDefault_on_DateTimeOffset(async);

        AssertSql();
    }

    public override async Task Where_coalesce_with_anonymous_types(bool async)
    {
        await base.Where_coalesce_with_anonymous_types(async);

        AssertSql();
    }

    public override async Task Projecting_correlated_collection_followed_by_Distinct(bool async)
    {
        await base.Projecting_correlated_collection_followed_by_Distinct(async);

        AssertSql();
    }

    public override async Task Projecting_some_properties_as_well_as_correlated_collection_followed_by_Distinct(bool async)
    {
        await base.Projecting_some_properties_as_well_as_correlated_collection_followed_by_Distinct(async);

        AssertSql();
    }

    public override async Task Projecting_entity_as_well_as_correlated_collection_followed_by_Distinct(bool async)
    {
        await base.Projecting_entity_as_well_as_correlated_collection_followed_by_Distinct(async);

        AssertSql();
    }

    public override async Task Projecting_entity_as_well_as_complex_correlated_collection_followed_by_Distinct(bool async)
    {
        await base.Projecting_entity_as_well_as_complex_correlated_collection_followed_by_Distinct(async);

        AssertSql();
    }

    public override async Task Projecting_entity_as_well_as_correlated_collection_of_scalars_followed_by_Distinct(bool async)
    {
        await base.Projecting_entity_as_well_as_correlated_collection_of_scalars_followed_by_Distinct(async);

        AssertSql();
    }

    public override async Task Correlated_collection_with_distinct_3_levels(bool async)
    {
        await base.Correlated_collection_with_distinct_3_levels(async);

        AssertSql();
    }

    public override async Task Include_after_SelectMany_throws(bool async)
    {
        await base.Include_after_SelectMany_throws(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s"."Name"
FROM "Factions" AS "f"
LEFT JOIN "Cities" AS "c" ON "f"."CapitalName" = "c"."Name"
INNER JOIN "Gears" AS "g" ON "c"."Name" = "g"."CityOfBirthName"
INNER JOIN "Squads" AS "s" ON "g"."SquadId" = "s"."Id"
""");
    }

    public override async Task Checked_context_throws_on_client_evaluation(bool async)
    {
        await base.Checked_context_throws_on_client_evaluation(async);

        AssertSql();
    }

    public override async Task Trying_to_access_unmapped_property_throws_informative_error(bool async)
    {
        await base.Trying_to_access_unmapped_property_throws_informative_error(async);

        AssertSql();
    }

    public override async Task Trying_to_access_unmapped_property_inside_aggregate(bool async)
    {
        await base.Trying_to_access_unmapped_property_inside_aggregate(async);

        AssertSql();
    }

    public override async Task Trying_to_access_unmapped_property_inside_subquery(bool async)
    {
        await base.Trying_to_access_unmapped_property_inside_subquery(async);

        AssertSql();
    }

    public override async Task Trying_to_access_unmapped_property_inside_join_key_selector(bool async)
    {
        await base.Trying_to_access_unmapped_property_inside_join_key_selector(async);

        AssertSql();
    }

    public override async Task Client_projection_with_nested_unmapped_property_bubbles_up_translation_failure_info(bool async)
    {
        await base.Client_projection_with_nested_unmapped_property_bubbles_up_translation_failure_info(async);

        AssertSql();
    }

    public override async Task String_concat_on_various_types(bool async)
    {
        await base.String_concat_on_various_types(async);

        AssertSql(
            """
SELECT 'HasSoulPatch ' || CAST("g"."HasSoulPatch" AS TEXT) || ' HasSoulPatch' AS "HasSoulPatch", 'Rank ' || CAST("g"."Rank" AS TEXT) || ' Rank' AS "Rank", 'SquadId ' || CAST("g"."SquadId" AS TEXT) || ' SquadId' AS "SquadId", 'Rating ' || COALESCE(CAST("m"."Rating" AS TEXT), '') || ' Rating' AS "Rating", 'Timeline ' || CAST("m"."Timeline" AS TEXT) || ' Timeline' AS "Timeline"
FROM "Gears" AS "g"
CROSS JOIN "Missions" AS "m"
ORDER BY "g"."Nickname", "m"."Id"
""");
    }

    public override async Task Nav_rewrite_Distinct_with_convert()
    {
        await base.Nav_rewrite_Distinct_with_convert();

        AssertSql();
    }

    public override async Task Nav_rewrite_Distinct_with_convert_anonymous()
    {
        await base.Nav_rewrite_Distinct_with_convert_anonymous();

        AssertSql();
    }

    public override async Task Navigation_based_on_complex_expression4(bool async)
    {
        await base.Navigation_based_on_complex_expression4(async);

        AssertSql(
            """
SELECT 1, "l2"."Name", "l2"."Discriminator", "l2"."LocustHordeId", "l2"."ThreatLevel", "l2"."ThreatLevelByte", "l2"."ThreatLevelNullableByte", "l2"."DefeatedByNickname", "l2"."DefeatedBySquadId", "l2"."HighCommandId", "l0"."Name", "l0"."Discriminator", "l0"."LocustHordeId", "l0"."ThreatLevel", "l0"."ThreatLevelByte", "l0"."ThreatLevelNullableByte", "l0"."DefeatedByNickname", "l0"."DefeatedBySquadId", "l0"."HighCommandId"
FROM "Factions" AS "f"
CROSS JOIN (
    SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId"
    FROM "LocustLeaders" AS "l"
    WHERE "l"."Discriminator" = 'LocustCommander'
) AS "l0"
LEFT JOIN (
    SELECT "l1"."Name", "l1"."Discriminator", "l1"."LocustHordeId", "l1"."ThreatLevel", "l1"."ThreatLevelByte", "l1"."ThreatLevelNullableByte", "l1"."DefeatedByNickname", "l1"."DefeatedBySquadId", "l1"."HighCommandId"
    FROM "LocustLeaders" AS "l1"
    WHERE "l1"."Discriminator" = 'LocustCommander'
) AS "l2" ON "f"."CommanderName" = "l2"."Name"
""");
    }

    public override async Task Navigation_based_on_complex_expression5(bool async)
    {
        await base.Navigation_based_on_complex_expression5(async);

        AssertSql(
            """
SELECT "l2"."Name", "l2"."Discriminator", "l2"."LocustHordeId", "l2"."ThreatLevel", "l2"."ThreatLevelByte", "l2"."ThreatLevelNullableByte", "l2"."DefeatedByNickname", "l2"."DefeatedBySquadId", "l2"."HighCommandId", "l0"."Name", "l0"."Discriminator", "l0"."LocustHordeId", "l0"."ThreatLevel", "l0"."ThreatLevelByte", "l0"."ThreatLevelNullableByte", "l0"."DefeatedByNickname", "l0"."DefeatedBySquadId", "l0"."HighCommandId"
FROM "Factions" AS "f"
CROSS JOIN (
    SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId"
    FROM "LocustLeaders" AS "l"
    WHERE "l"."Discriminator" = 'LocustCommander'
) AS "l0"
LEFT JOIN (
    SELECT "l1"."Name", "l1"."Discriminator", "l1"."LocustHordeId", "l1"."ThreatLevel", "l1"."ThreatLevelByte", "l1"."ThreatLevelNullableByte", "l1"."DefeatedByNickname", "l1"."DefeatedBySquadId", "l1"."HighCommandId"
    FROM "LocustLeaders" AS "l1"
    WHERE "l1"."Discriminator" = 'LocustCommander'
) AS "l2" ON "f"."CommanderName" = "l2"."Name"
""");
    }

    public override async Task Navigation_based_on_complex_expression6(bool async)
    {
        await base.Navigation_based_on_complex_expression6(async);

        AssertSql(
            """
SELECT "l2"."Name" = 'Queen Myrrah' AND "l2"."Name" IS NOT NULL, "l2"."Name", "l2"."Discriminator", "l2"."LocustHordeId", "l2"."ThreatLevel", "l2"."ThreatLevelByte", "l2"."ThreatLevelNullableByte", "l2"."DefeatedByNickname", "l2"."DefeatedBySquadId", "l2"."HighCommandId", "l0"."Name", "l0"."Discriminator", "l0"."LocustHordeId", "l0"."ThreatLevel", "l0"."ThreatLevelByte", "l0"."ThreatLevelNullableByte", "l0"."DefeatedByNickname", "l0"."DefeatedBySquadId", "l0"."HighCommandId"
FROM "Factions" AS "f"
CROSS JOIN (
    SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId"
    FROM "LocustLeaders" AS "l"
    WHERE "l"."Discriminator" = 'LocustCommander'
) AS "l0"
LEFT JOIN (
    SELECT "l1"."Name", "l1"."Discriminator", "l1"."LocustHordeId", "l1"."ThreatLevel", "l1"."ThreatLevelByte", "l1"."ThreatLevelNullableByte", "l1"."DefeatedByNickname", "l1"."DefeatedBySquadId", "l1"."HighCommandId"
    FROM "LocustLeaders" AS "l1"
    WHERE "l1"."Discriminator" = 'LocustCommander'
) AS "l2" ON "f"."CommanderName" = "l2"."Name"
""");
    }

    public override async Task Include_after_select_with_cast_throws(bool async)
    {
        await base.Include_after_select_with_cast_throws(async);

        AssertSql();
    }

    public override async Task Include_after_select_with_entity_projection_throws(bool async)
    {
        await base.Include_after_select_with_entity_projection_throws(async);

        AssertSql();
    }

    public override async Task Include_after_select_anonymous_projection_throws(bool async)
    {
        await base.Include_after_select_anonymous_projection_throws(async);

        AssertSql();
    }

    public override async Task Group_by_with_aggregate_max_on_entity_type(bool async)
    {
        await base.Group_by_with_aggregate_max_on_entity_type(async);

        AssertSql();
    }

    public override async Task Include_collection_and_invalid_navigation_using_string_throws(bool async)
    {
        await base.Include_collection_and_invalid_navigation_using_string_throws(async);

        AssertSql();
    }

    public override async Task Include_with_concat(bool async)
    {
        await base.Include_with_concat(async);

        AssertSql();
    }

    public override async Task Join_with_complex_key_selector(bool async)
    {
        await base.Join_with_complex_key_selector(async);

        AssertSql(
            """
SELECT "s"."Id", "t0"."Id" AS "TagId"
FROM "Squads" AS "s"
CROSS JOIN (
    SELECT "t"."Id"
    FROM "Tags" AS "t"
    WHERE "t"."Note" = 'Marcus'' Tag'
) AS "t0"
""");
    }

    public override async Task Streaming_correlated_collection_issue_11403_returning_ordered_enumerable_throws(bool async)
    {
        await base.Streaming_correlated_collection_issue_11403_returning_ordered_enumerable_throws(async);

        AssertSql();
    }

    public override async Task Optional_navigation_type_compensation_works_with_skip(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_skip(async);

        AssertSql();
    }

    public override async Task Optional_navigation_type_compensation_works_with_take(bool async)
    {
        await base.Optional_navigation_type_compensation_works_with_take(async);

        AssertSql();
    }

    public override async Task Select_correlated_filtered_collection_returning_queryable_throws(bool async)
    {
        await base.Select_correlated_filtered_collection_returning_queryable_throws(async);

        AssertSql();
    }

    public override async Task Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used(
        bool async)
    {
        await base.Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used(async);

        AssertSql();
    }

    public override async Task Client_method_on_collection_navigation_in_predicate(bool async)
    {
        await base.Client_method_on_collection_navigation_in_predicate(async);

        AssertSql();
    }

    public override async Task Client_method_on_collection_navigation_in_predicate_accessed_by_ef_property(bool async)
    {
        await base.Client_method_on_collection_navigation_in_predicate_accessed_by_ef_property(async);

        AssertSql();
    }

    public override async Task Client_method_on_collection_navigation_in_order_by(bool async)
    {
        await base.Client_method_on_collection_navigation_in_order_by(async);

        AssertSql();
    }

    public override async Task Client_method_on_collection_navigation_in_additional_from_clause(bool async)
    {
        await base.Client_method_on_collection_navigation_in_additional_from_clause(async);

        AssertSql();
    }

    public override async Task Client_method_on_collection_navigation_in_outer_join_key(bool async)
    {
        await base.Client_method_on_collection_navigation_in_outer_join_key(async);

        AssertSql();
    }

    public override async Task Include_multiple_one_to_one_and_one_to_many_self_reference(bool async)
    {
        await base.Include_multiple_one_to_one_and_one_to_many_self_reference(async);

        AssertSql();
    }

    public override async Task Include_multiple_one_to_one_and_one_to_one_and_one_to_many(bool async)
    {
        await base.Include_multiple_one_to_one_and_one_to_one_and_one_to_many(async);

        AssertSql();
    }

    public override async Task Include_multiple_include_then_include(bool async)
    {
        await base.Include_multiple_include_then_include(async);

        AssertSql();
    }

    public override async Task Where_compare_anonymous_types(bool async)
    {
        await base.Where_compare_anonymous_types(async);

        AssertSql();
    }

    public override async Task Select_Where_Navigation_Client(bool async)
    {
        await base.Select_Where_Navigation_Client(async);

        AssertSql();
    }

    public override async Task Where_subquery_equality_to_null_with_composite_key(bool async)
    {
        await base.Where_subquery_equality_to_null_with_composite_key(async);

        AssertSql(
            """
SELECT "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s"."Name"
FROM "Squads" AS "s"
WHERE NOT EXISTS (
    SELECT 1
    FROM "Gears" AS "g"
    WHERE "s"."Id" = "g"."SquadId")
""");
    }

    public override async Task Where_subquery_equality_to_null_with_composite_key_should_match_nulls(bool async)
    {
        await base.Where_subquery_equality_to_null_with_composite_key_should_match_nulls(async);

        AssertSql(
            """
SELECT "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s"."Name"
FROM "Squads" AS "s"
WHERE NOT EXISTS (
    SELECT 1
    FROM "Gears" AS "g"
    WHERE "s"."Id" = "g"."SquadId" AND "g"."FullName" = 'Anthony Carmine')
""");
    }

    public override async Task Where_subquery_equality_to_null_without_composite_key(bool async)
    {
        await base.Where_subquery_equality_to_null_without_composite_key(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE NOT EXISTS (
    SELECT 1
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName")
""");
    }

    public override async Task Where_subquery_equality_to_null_without_composite_key_should_match_null(bool async)
    {
        await base.Where_subquery_equality_to_null_without_composite_key_should_match_null(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE NOT EXISTS (
    SELECT 1
    FROM "Weapons" AS "w"
    WHERE "g"."FullName" = "w"."OwnerFullName" AND "w"."Name" = 'Hammer of Dawn')
""");
    }

    public override async Task Include_reference_on_derived_type_using_EF_Property(bool async)
    {
        await base.Include_reference_on_derived_type_using_EF_Property(async);

        AssertSql(
            """
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "LocustLeaders" AS "l"
LEFT JOIN "Gears" AS "g" ON "l"."DefeatedByNickname" = "g"."Nickname" AND "l"."DefeatedBySquadId" = "g"."SquadId"
""");
    }

    public override async Task Include_collection_on_derived_type_using_EF_Property(bool async)
    {
        await base.Include_collection_on_derived_type_using_EF_Property(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."Nickname" = "g0"."LeaderNickname" AND "g"."SquadId" = "g0"."LeaderSquadId"
ORDER BY "g"."Nickname", "g"."SquadId", "g0"."Nickname"
""");
    }

    public override async Task EF_Property_based_Include_navigation_on_derived_type(bool async)
    {
        await base.EF_Property_based_Include_navigation_on_derived_type(async);

        AssertSql(
            """
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."Nickname" = "g0"."LeaderNickname" AND "g"."SquadId" = "g0"."LeaderSquadId"
WHERE "g"."Discriminator" = 'Officer'
ORDER BY "g"."Nickname", "g"."SquadId", "g0"."Nickname"
""");
    }

    public override async Task ElementAt_basic_with_OrderBy(bool async)
    {
        await base.ElementAt_basic_with_OrderBy(async);

        AssertSql(
            """
@__p_0='0'

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
ORDER BY "g"."FullName"
LIMIT 1 OFFSET @__p_0
""");
    }

    public override async Task ElementAtOrDefault_basic_with_OrderBy(bool async)
    {
        await base.ElementAtOrDefault_basic_with_OrderBy(async);

        AssertSql(
            """
@__p_0='1'

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
ORDER BY "g"."FullName"
LIMIT 1 OFFSET @__p_0
""");
    }

    public override async Task ElementAtOrDefault_basic_with_OrderBy_parameter(bool async)
    {
        await base.ElementAtOrDefault_basic_with_OrderBy_parameter(async);

        AssertSql(
            """
@__p_0='2'

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
ORDER BY "g"."FullName"
LIMIT 1 OFFSET @__p_0
""");
    }

    public override async Task Where_subquery_with_ElementAtOrDefault_equality_to_null_with_composite_key(bool async)
    {
        await base.Where_subquery_with_ElementAtOrDefault_equality_to_null_with_composite_key(async);

        AssertSql(
            """
SELECT "s"."Id", "s"."Banner", "s"."Banner5", "s"."InternalNumber", "s"."Name"
FROM "Squads" AS "s"
WHERE NOT EXISTS (
    SELECT 1
    FROM "Gears" AS "g"
    WHERE "s"."Id" = "g"."SquadId"
    ORDER BY "g"."Nickname"
    LIMIT -1 OFFSET 2)
""");
    }

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

    public override async Task Using_indexer_on_byte_array_and_string_in_projection(bool async)
    {
        await base.Using_indexer_on_byte_array_and_string_in_projection(async);

        AssertSql(
            """
SELECT "s"."Id", "s"."Banner", "s"."Name"
FROM "Squads" AS "s"
""");
    }

    public override async Task Set_operator_with_navigation_in_projection_groupby_aggregate(bool async)
    {
        await base.Set_operator_with_navigation_in_projection_groupby_aggregate(async);

        AssertSql(
            """
SELECT "s"."Name", (
    SELECT COALESCE(SUM(length("c"."Location")), 0)
    FROM "Gears" AS "g2"
    INNER JOIN "Squads" AS "s0" ON "g2"."SquadId" = "s0"."Id"
    INNER JOIN "Cities" AS "c" ON "g2"."CityOfBirthName" = "c"."Name"
    WHERE 'Marcus' IN (
        SELECT "g3"."Nickname"
        FROM "Gears" AS "g3"
        UNION ALL
        SELECT "g4"."Nickname"
        FROM "Gears" AS "g4"
    ) AND ("s"."Name" = "s0"."Name" OR ("s"."Name" IS NULL AND "s0"."Name" IS NULL))) AS "SumOfLengths"
FROM "Gears" AS "g"
INNER JOIN "Squads" AS "s" ON "g"."SquadId" = "s"."Id"
WHERE 'Marcus' IN (
    SELECT "g0"."Nickname"
    FROM "Gears" AS "g0"
    UNION ALL
    SELECT "g1"."Nickname"
    FROM "Gears" AS "g1"
)
GROUP BY "s"."Name"
""");
    }

    public override async Task Nav_expansion_inside_Contains_argument(bool async)
    {
        await base.Nav_expansion_inside_Contains_argument(async);

        AssertSql(
            """
@__numbers_0='[1,-1]' (Size = 6)

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE CASE
    WHEN EXISTS (
        SELECT 1
        FROM "Weapons" AS "w"
        WHERE "g"."FullName" = "w"."OwnerFullName") THEN 1
    ELSE 0
END IN (
    SELECT "n"."value"
    FROM json_each(@__numbers_0) AS "n"
)
""");
    }

    public override async Task Nav_expansion_with_member_pushdown_inside_Contains_argument(bool async)
    {
        await base.Nav_expansion_with_member_pushdown_inside_Contains_argument(async);

        AssertSql(
            """
@__weapons_0='["Marcus\u0027 Lancer","Dom\u0027s Gnasher"]' (Size = 44)

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE (
    SELECT "w0"."Name"
    FROM "Weapons" AS "w0"
    WHERE "g"."FullName" = "w0"."OwnerFullName"
    ORDER BY "w0"."Id"
    LIMIT 1) IN (
    SELECT "w"."value"
    FROM json_each(@__weapons_0) AS "w"
)
""");
    }

    public override async Task Subquery_inside_Take_argument(bool async)
    {
        await base.Subquery_inside_Take_argument(async);

        AssertSql(
            """
@__numbers_0='[0,1,2]' (Size = 7)

SELECT "g"."Nickname", "g"."SquadId", "w1"."Id", "w1"."AmmunitionType", "w1"."IsAutomatic", "w1"."Name", "w1"."OwnerFullName", "w1"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
    FROM (
        SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId", ROW_NUMBER() OVER(PARTITION BY "w"."OwnerFullName" ORDER BY "w"."Id") AS "row"
        FROM "Weapons" AS "w"
    ) AS "w0"
    WHERE "w0"."row" <= COALESCE((
        SELECT "n"."value"
        FROM json_each(@__numbers_0) AS "n"
        ORDER BY "n"."value"
        LIMIT 1 OFFSET 1), 0)
) AS "w1" ON "g"."FullName" = "w1"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId", "w1"."OwnerFullName", "w1"."Id"
""");
    }

    public override async Task Nav_expansion_inside_Skip_correlated_to_source(bool async)
    {
        await base.Nav_expansion_inside_Skip_correlated_to_source(async);

        AssertSql();
    }

    public override async Task Nav_expansion_inside_Take_correlated_to_source(bool async)
    {
        await base.Nav_expansion_inside_Take_correlated_to_source(async);

        AssertSql();
    }

    public override async Task Nav_expansion_with_member_pushdown_inside_Take_correlated_to_source(bool async)
    {
        await base.Nav_expansion_with_member_pushdown_inside_Take_correlated_to_source(async);

        AssertSql();
    }

    public override async Task Nav_expansion_inside_ElementAt_correlated_to_source(bool async)
    {
        await base.Nav_expansion_inside_ElementAt_correlated_to_source(async);

        AssertSql();
    }

    public override Task DateTimeOffset_to_unix_time_milliseconds(bool async)
        => AssertTranslationFailed(() => base.DateTimeOffset_to_unix_time_milliseconds(async));

    public override Task DateTimeOffset_to_unix_time_seconds(bool async)
        => AssertTranslationFailed(() => base.DateTimeOffset_to_unix_time_seconds(async));

    public override async Task Include_one_to_many_on_composite_key_then_orderby_key_properties(bool async)
    {
        await base.Include_one_to_many_on_composite_key_then_orderby_key_properties(async);

        AssertSql(
"""
SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
ORDER BY "g"."SquadId", "g"."Nickname"
""");
    }

    public override async Task Find_underlying_property_after_GroupJoin_DefaultIfEmpty(bool async)
    {
        await base.Find_underlying_property_after_GroupJoin_DefaultIfEmpty(async);

        AssertSql(
            """
SELECT "g"."FullName", CAST("l0"."ThreatLevel" AS INTEGER) AS "ThreatLevel"
FROM "Gears" AS "g"
LEFT JOIN (
    SELECT "l"."ThreatLevel", "l"."DefeatedByNickname"
    FROM "LocustLeaders" AS "l"
    WHERE "l"."Discriminator" = 'LocustCommander'
) AS "l0" ON "g"."Nickname" = "l0"."DefeatedByNickname"
""");
    }

    public override async Task Join_include_coalesce_simple(bool async)
    {
        await base.Join_include_coalesce_simple(async);

        AssertSql(
"""
SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId", "g"."Nickname" = 'Marcus'
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."LeaderNickname" = "g0"."Nickname"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId", "g0"."Nickname", "g0"."SquadId"
""",
                //
                """
SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "g"."Nickname", "g"."SquadId", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."LeaderNickname" = "g0"."Nickname"
LEFT JOIN "Weapons" AS "w" ON "g0"."FullName" = "w"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId", "g0"."Nickname", "g0"."SquadId"
""",
                //
                """
SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "g"."Nickname", "g"."SquadId", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."LeaderNickname" = "g0"."Nickname"
LEFT JOIN "Weapons" AS "w" ON "g0"."FullName" = "w"."OwnerFullName"
LEFT JOIN "Weapons" AS "w0" ON "g"."FullName" = "w0"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId", "g0"."Nickname", "g0"."SquadId", "w"."Id"
""");
    }

    public override async Task Join_include_coalesce_nested(bool async)
    {
        await base.Join_include_coalesce_nested(async);

        AssertSql(
"""
SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId", "g"."Nickname" = 'Marcus'
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."LeaderNickname" = "g0"."Nickname"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId", "g0"."Nickname", "g0"."SquadId"
""",
                //
                """
SELECT "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "g"."Nickname", "g"."SquadId", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId", "w0"."Id", "w0"."AmmunitionType", "w0"."IsAutomatic", "w0"."Name", "w0"."OwnerFullName", "w0"."SynergyWithId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "w1"."Id", "w1"."AmmunitionType", "w1"."IsAutomatic", "w1"."Name", "w1"."OwnerFullName", "w1"."SynergyWithId"
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."LeaderNickname" = "g0"."Nickname"
LEFT JOIN "Weapons" AS "w" ON "g0"."FullName" = "w"."OwnerFullName"
LEFT JOIN "Weapons" AS "w0" ON "g0"."FullName" = "w0"."OwnerFullName"
LEFT JOIN "Weapons" AS "w1" ON "g0"."FullName" = "w1"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId", "g0"."Nickname", "g0"."SquadId", "w"."Id", "w0"."Id"
""");
    }

    public override async Task Join_include_conditional(bool async)
    {
        await base.Join_include_conditional(async);

        AssertSql(
"""
SELECT "g0"."Nickname" IS NOT NULL AND "g0"."SquadId" IS NOT NULL, "g0"."Nickname", "g0"."SquadId", "g0"."AssignedCityName", "g0"."CityOfBirthName", "g0"."Discriminator", "g0"."FullName", "g0"."HasSoulPatch", "g0"."LeaderNickname", "g0"."LeaderSquadId", "g0"."Rank", "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank", "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId", "g"."Nickname" = 'Marcus'
FROM "Gears" AS "g"
LEFT JOIN "Gears" AS "g0" ON "g"."LeaderNickname" = "g0"."Nickname"
LEFT JOIN "Weapons" AS "w" ON "g"."FullName" = "w"."OwnerFullName"
ORDER BY "g"."Nickname", "g"."SquadId", "g0"."Nickname", "g0"."SquadId"
""");
    }

    public override async Task Derived_reference_is_skipped_when_base_type(bool async)
    {
        await base.Derived_reference_is_skipped_when_base_type(async);

        AssertSql(
"""
SELECT "l"."Name", "l"."Discriminator", "l"."LocustHordeId", "l"."ThreatLevel", "l"."ThreatLevelByte", "l"."ThreatLevelNullableByte", "l"."DefeatedByNickname", "l"."DefeatedBySquadId", "l"."HighCommandId", "l0"."Id", "l0"."IsOperational", "l0"."Name"
FROM "LocustLeaders" AS "l"
LEFT JOIN "LocustHighCommands" AS "l0" ON "l"."HighCommandId" = "l0"."Id"
""");
    }

    public override async Task Nested_contains_with_enum(bool async)
    {
        await base.Nested_contains_with_enum(async);

        AssertSql(
            """
@__ranks_0='[1]' (Size = 3)
@__key_1='5f221fb9-66f4-442a-92c9-d97ed5989cc7'
@__keys_2='["0A47BCB7-A1CB-4345-8944-C58F82D6AAC7","5F221FB9-66F4-442A-92C9-D97ED5989CC7"]' (Size = 79)

SELECT "g"."Nickname", "g"."SquadId", "g"."AssignedCityName", "g"."CityOfBirthName", "g"."Discriminator", "g"."FullName", "g"."HasSoulPatch", "g"."LeaderNickname", "g"."LeaderSquadId", "g"."Rank"
FROM "Gears" AS "g"
WHERE CASE
    WHEN "g"."Rank" IN (
        SELECT "r"."value"
        FROM json_each(@__ranks_0) AS "r"
    ) THEN @__key_1
    ELSE @__key_1
END IN (
    SELECT "k"."value"
    FROM json_each(@__keys_2) AS "k"
)
""",
            //
            """
@__ammoTypes_0='[1]' (Size = 3)
@__key_1='5f221fb9-66f4-442a-92c9-d97ed5989cc7'
@__keys_2='["0A47BCB7-A1CB-4345-8944-C58F82D6AAC7","5F221FB9-66F4-442A-92C9-D97ED5989CC7"]' (Size = 79)

SELECT "w"."Id", "w"."AmmunitionType", "w"."IsAutomatic", "w"."Name", "w"."OwnerFullName", "w"."SynergyWithId"
FROM "Weapons" AS "w"
WHERE CASE
    WHEN "w"."AmmunitionType" IN (
        SELECT "a"."value"
        FROM json_each(@__ammoTypes_0) AS "a"
    ) THEN @__key_1
    ELSE @__key_1
END IN (
    SELECT "k"."value"
    FROM json_each(@__keys_2) AS "k"
)
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
