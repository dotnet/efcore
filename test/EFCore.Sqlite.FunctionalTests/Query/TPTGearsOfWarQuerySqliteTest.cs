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

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
