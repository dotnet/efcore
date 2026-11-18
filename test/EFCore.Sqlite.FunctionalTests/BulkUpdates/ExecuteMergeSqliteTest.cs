// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#pragma warning disable EF9107 // ExecuteMerge is experimental

public class ExecuteMergeSqliteTest : ExecuteMergeRelationalTestBase<ExecuteMergeSqliteFixture>
{
    public ExecuteMergeSqliteTest(ExecuteMergeSqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Do_nothing_inserts_new_and_ignores_conflict(bool async)
    {
        await base.Do_nothing_inserts_new_and_ignores_conflict(async);

        AssertSql(
            """
@p1='1'
@p2='ignored' (Size = 7)
@p4='3'
@p5='Blog3' (Size = 5)

INSERT INTO "Blogs" ("Id", "Name", "Note")
VALUES (@p1, @p2, NULL),
(@p4, @p5, NULL)
ON CONFLICT ("Id") DO NOTHING;
""",
            //
            """
SELECT "b"."Id", "b"."Name", "b"."Note"
FROM "Blogs" AS "b"
ORDER BY "b"."Id"
""");
    }

    public override async Task Do_nothing_match_on_unique_column(bool async)
    {
        await base.Do_nothing_match_on_unique_column(async);

        AssertSql(
            """
@p1='5'
@p2='Blog1' (Size = 5)
@p4='3'
@p5='Blog3' (Size = 5)

INSERT INTO "Blogs" ("Id", "Name", "Note")
VALUES (@p1, @p2, NULL),
(@p4, @p5, NULL)
ON CONFLICT ("Name") DO NOTHING;
""",
            //
            """
SELECT "b"."Id", "b"."Name", "b"."Note"
FROM "Blogs" AS "b"
ORDER BY "b"."Id"
""");
    }

    public override async Task When_matched_updates_and_inserts(bool async)
    {
        await base.When_matched_updates_and_inserts(async);

        AssertSql(
            """
@p1='1'
@p2='mon' (Size = 3)
@p3='3'
@p4='2'
@p5='tue' (Size = 3)
@p6='7'

INSERT INTO "Visits" ("UserId", "Day", "Visits")
VALUES (@p1, @p2, @p3),
(@p4, @p5, @p6)
ON CONFLICT ("UserId", "Day") DO UPDATE SET
"Visits" = excluded."Visits";
""",
            //
            """
SELECT "v"."UserId", "v"."Day", "v"."Visits"
FROM "Visits" AS "v"
ORDER BY "v"."UserId"
""");
    }

    public override async Task With_multiple_match_calls(bool async)
    {
        await base.With_multiple_match_calls(async);

        AssertSql(
            """
@p1='1'
@p2='mon' (Size = 3)
@p3='3'
@p4='2'
@p5='tue' (Size = 3)
@p6='7'

INSERT INTO "Visits" ("UserId", "Day", "Visits")
VALUES (@p1, @p2, @p3),
(@p4, @p5, @p6)
ON CONFLICT ("UserId", "Day") DO UPDATE SET
"Visits" = excluded."Visits";
""",
            //
            """
SELECT "v"."UserId", "v"."Day", "v"."Visits"
FROM "Visits" AS "v"
ORDER BY "v"."UserId"
""");
    }

    public override async Task Match_uses_source_key_selector(bool async)
    {
        await base.Match_uses_source_key_selector(async);

        AssertSql(
            """
@p1='1'
@p2='50'
@p3='1'
@p4='2'
@p5='70'
@p6='2'

INSERT INTO "Accounts" ("Id", "Balance", "ExternalId")
VALUES (@p1, @p2, @p3),
(@p4, @p5, @p6)
ON CONFLICT ("Id") DO UPDATE SET
"Balance" = excluded."Balance";
""",
            //
            """
SELECT "a"."Id", "a"."Balance", "a"."ExternalId"
FROM "Accounts" AS "a"
ORDER BY "a"."Id"
""");
    }

    public override async Task When_matched_reads_custom_match_source_member(bool async)
    {
        await base.When_matched_reads_custom_match_source_member(async);

        AssertSql(
            """
@p1='1'
@p2='50'
@p3='1'
@p4='2'
@p5='70'
@p6='2'

INSERT INTO "Accounts" ("Id", "Balance", "ExternalId")
VALUES (@p1, @p2, @p3),
(@p4, @p5, @p6)
ON CONFLICT ("Id") DO UPDATE SET
"ExternalId" = excluded."ExternalId";
""",
            //
            """
SELECT "a"."Id", "a"."Balance", "a"."ExternalId"
FROM "Accounts" AS "a"
ORDER BY "a"."Id"
""");
    }

    public override async Task When_not_matched_uses_explicit_insert_setters(bool async)
    {
        await base.When_not_matched_uses_explicit_insert_setters(async);

        AssertSql(
            """
@p1='1'
@p2='ignored' (Size = 7)
@p3='3'
@p4='Blog3' (Size = 5)

INSERT INTO "Blogs" ("Id", "Name")
VALUES (@p1, @p2),
(@p3, @p4)
ON CONFLICT ("Id") DO NOTHING;
""",
            //
            """
SELECT "b"."Id", "b"."Name", "b"."Note"
FROM "Blogs" AS "b"
WHERE "b"."Id" = 3
LIMIT 2
""",
            //
            """
SELECT "b"."Id", "b"."Name", "b"."Note"
FROM "Blogs" AS "b"
WHERE "b"."Id" = 1
LIMIT 2
""");
    }

    public override async Task When_matched_and_when_not_matched_together(bool async)
    {
        await base.When_matched_and_when_not_matched_together(async);

        AssertSql(
            """
@p11='1'
@p12='Updated' (Size = 7)
@p='INS' (Size = 3)
@p13='3'
@p14='Blog3' (Size = 5)

INSERT INTO "Blogs" ("Id", "Name", "Note")
VALUES (@p11, @p12, @p),
(@p13, @p14, @p)
ON CONFLICT ("Id") DO UPDATE SET
"Name" = excluded."Name";
""",
            //
            """
SELECT "b"."Id", "b"."Name", "b"."Note"
FROM "Blogs" AS "b"
WHERE "b"."Id" <> 2
ORDER BY "b"."Id"
""");
    }

    public override async Task When_matched_reads_dto_only_source_member(bool async)
    {
        await base.When_matched_reads_dto_only_source_member(async);

        AssertSql(
            """
@p1='1'
@p2='Updated' (Size = 7)
@p3='3'
@p4='Blog3' (Size = 5)

INSERT INTO "Blogs" ("Id", "Name")
VALUES (@p1, @p2),
(@p3, @p4)
ON CONFLICT ("Id") DO UPDATE SET
"Name" = excluded."Name";
""",
            //
            """
SELECT "b"."Id", "b"."Name", "b"."Note"
FROM "Blogs" AS "b"
ORDER BY "b"."Id"
""");
    }

    public override async Task Source_member_used_for_converted_and_unconverted_properties(bool async)
    {
        await base.Source_member_used_for_converted_and_unconverted_properties(async);

        AssertSql(
            """
@p1='1'
@p2='#5' (Nullable = false) (Size = 2)
@p3='5'
@p4='2'
@p5='#7' (Nullable = false) (Size = 2)
@p6='7'

INSERT INTO "Measurements" ("Id", "ConvertedValue", "Value")
VALUES (@p1, @p2, @p3),
(@p4, @p5, @p6)
ON CONFLICT ("Id") DO UPDATE SET
"Value" = excluded."Value",
"ConvertedValue" = excluded."ConvertedValue";
""",
            //
            """
SELECT "m"."Id", "m"."ConvertedValue", "m"."Value"
FROM "Measurements" AS "m"
ORDER BY "m"."Id"
""");
    }

    public override async Task With_empty_when_matched_leaves_matched_rows_unchanged(bool async)
    {
        await base.With_empty_when_matched_leaves_matched_rows_unchanged(async);

        AssertSql(
            """
@p1='1'
@p2='ignored' (Size = 7)
@p4='3'
@p5='Blog3' (Size = 5)

INSERT INTO "Blogs" ("Id", "Name", "Note")
VALUES (@p1, @p2, NULL),
(@p4, @p5, NULL)
ON CONFLICT ("Id") DO NOTHING;
""",
            //
            """
SELECT "b"."Id", "b"."Name", "b"."Note"
FROM "Blogs" AS "b"
ORDER BY "b"."Id"
""");
    }

    public override async Task When_matched_with_constant_value(bool async)
    {
        await base.When_matched_with_constant_value(async);

        AssertSql(
            """
@p11='1'
@p12='mon' (Size = 3)
@p13='3'
@p='0'

INSERT INTO "Visits" ("UserId", "Day", "Visits")
VALUES (@p11, @p12, @p13)
ON CONFLICT ("UserId", "Day") DO UPDATE SET
"Visits" = @p;
""",
            //
            """
SELECT "v"."UserId", "v"."Day", "v"."Visits"
FROM "Visits" AS "v"
LIMIT 2
""");
    }

    public override async Task When_not_matched_with_constant_value(bool async)
    {
        await base.When_not_matched_with_constant_value(async);

        AssertSql(
            """
@p11='3'
@p='CONST' (Size = 5)

INSERT INTO "Blogs" ("Id", "Name")
VALUES (@p11, @p)
ON CONFLICT ("Id") DO NOTHING;
""",
            //
            """
SELECT "b"."Id", "b"."Name", "b"."Note"
FROM "Blogs" AS "b"
WHERE "b"."Id" = 3
LIMIT 2
""");
    }

    public override async Task Default_insert_reads_same_named_members_from_dto_source(bool async)
    {
        await base.Default_insert_reads_same_named_members_from_dto_source(async);

        AssertSql(
            """
@p1='1'
@p2='ignored' (Size = 7)
@p4='3'
@p5='Blog3' (Size = 5)
@p6='hi' (Size = 2)

INSERT INTO "Blogs" ("Id", "Name", "Note")
VALUES (@p1, @p2, NULL),
(@p4, @p5, @p6)
ON CONFLICT ("Id") DO NOTHING;
""",
            //
            """
SELECT "b"."Id", "b"."Name", "b"."Note"
FROM "Blogs" AS "b"
WHERE "b"."Id" = 3
LIMIT 2
""");
    }

    public override async Task With_empty_source(bool async)
    {
        await base.With_empty_source(async);

        AssertSql(
            """
INSERT INTO "Blogs" ("Id", "Name", "Note")
SELECT NULL, NULL, NULL WHERE 0;
""",
            //
            """
SELECT COUNT(*)
FROM "Blogs" AS "b"
""");
    }

    public override async Task Reused_source_collection_uses_current_values(bool async)
    {
        await base.Reused_source_collection_uses_current_values(async);

        AssertSql(
            """
@p1='3'
@p2='Blog3' (Size = 5)

INSERT INTO "Blogs" ("Id", "Name", "Note")
VALUES (@p1, @p2, NULL)
ON CONFLICT ("Id") DO NOTHING;
""",
            //
            """
@p1='4'
@p2='Blog4' (Size = 5)
@p4='5'
@p5='Blog5' (Size = 5)

INSERT INTO "Blogs" ("Id", "Name", "Note")
VALUES (@p1, @p2, NULL),
(@p4, @p5, NULL)
ON CONFLICT ("Id") DO NOTHING;
""",
            //
            """
SELECT "b"."Id", "b"."Name", "b"."Note"
FROM "Blogs" AS "b"
ORDER BY "b"."Id"
""");
    }

    public override async Task Queryable_source_inserts_rows(bool async)
    {
        await base.Queryable_source_inserts_rows(async);

        AssertSql(
            """
@p1='3'
@p2='Blog3' (Size = 5)

INSERT INTO "Blogs" ("Id", "Name", "Note")
VALUES (@p1, @p2, NULL)
ON CONFLICT ("Id") DO NOTHING;
""",
            //
            """
SELECT "b"."Id", "b"."Name", "b"."Note"
FROM "Blogs" AS "b"
WHERE "b"."Id" = 3
LIMIT 2
""");
    }

    public override async Task Query_source_throws(bool async)
    {
        await base.Query_source_throws(async);

        AssertSql();
    }

    public override async Task Returning_with_queryable_source(bool async)
    {
        await base.Returning_with_queryable_source(async);

        AssertSql(
            """
@p1='3'
@p2='Blog3' (Size = 5)

INSERT INTO "Blogs" ("Id", "Name", "Note")
VALUES (@p1, @p2, NULL)
ON CONFLICT ("Id") DO NOTHING
RETURNING "Id";
""");
    }

    public override async Task Returning_does_not_enumerate_source_until_executed()
    {
        await base.Returning_does_not_enumerate_source_until_executed();

        AssertSql(
            """
@p1='3'
@p2='Blog3' (Size = 5)
@p4='4'
@p5='Blog4' (Size = 5)

INSERT INTO "Blogs" ("Id", "Name", "Note")
VALUES (@p1, @p2, NULL),
(@p4, @p5, NULL)
ON CONFLICT ("Id") DO NOTHING
RETURNING "Id";
""");
    }

    public override async Task Returning_can_be_enumerated_twice()
    {
        await base.Returning_can_be_enumerated_twice();

        AssertSql(
            """
@p1='1'
@p2='Updated' (Size = 7)
@p4='3'
@p5='Blog3' (Size = 5)

INSERT INTO "Blogs" ("Id", "Name", "Note")
VALUES (@p1, @p2, NULL),
(@p4, @p5, NULL)
ON CONFLICT ("Id") DO UPDATE SET
"Name" = excluded."Name"
RETURNING "Id";
""",
            //
            """
@p1='1'
@p2='Updated' (Size = 7)
@p4='3'
@p5='Blog3' (Size = 5)

INSERT INTO "Blogs" ("Id", "Name", "Note")
VALUES (@p1, @p2, NULL),
(@p4, @p5, NULL)
ON CONFLICT ("Id") DO UPDATE SET
"Name" = excluded."Name"
RETURNING "Id";
""");
    }

    public override async Task Returning_async_can_be_enumerated_twice()
    {
        await base.Returning_async_can_be_enumerated_twice();

        AssertSql(
            """
@p1='1'
@p2='Updated' (Size = 7)
@p4='3'
@p5='Blog3' (Size = 5)

INSERT INTO "Blogs" ("Id", "Name", "Note")
VALUES (@p1, @p2, NULL),
(@p4, @p5, NULL)
ON CONFLICT ("Id") DO UPDATE SET
"Name" = excluded."Name"
RETURNING "Id";
""",
            //
            """
@p1='1'
@p2='Updated' (Size = 7)
@p4='3'
@p5='Blog3' (Size = 5)

INSERT INTO "Blogs" ("Id", "Name", "Note")
VALUES (@p1, @p2, NULL),
(@p4, @p5, NULL)
ON CONFLICT ("Id") DO UPDATE SET
"Name" = excluded."Name"
RETURNING "Id";
""");
    }

    public override async Task Returning_projects_affected_rows(bool async)
    {
        await base.Returning_projects_affected_rows(async);

        AssertSql(
            """
@p1='1'
@p2='mon' (Size = 3)
@p3='3'
@p4='2'
@p5='tue' (Size = 3)
@p6='7'

INSERT INTO "Visits" ("UserId", "Day", "Visits")
VALUES (@p1, @p2, @p3),
(@p4, @p5, @p6)
ON CONFLICT ("UserId", "Day") DO UPDATE SET
"Visits" = excluded."Visits"
RETURNING "UserId", "Visits";
""");
    }

    public override async Task Returning_projects_scalars(bool async)
    {
        await base.Returning_projects_scalars(async);

        AssertSql(
            """
@p1='1'
@p2='Updated' (Size = 7)
@p4='3'
@p5='Blog3' (Size = 5)

INSERT INTO "Blogs" ("Id", "Name", "Note")
VALUES (@p1, @p2, NULL),
(@p4, @p5, NULL)
ON CONFLICT ("Id") DO UPDATE SET
"Name" = excluded."Name"
RETURNING "Id";
""",
            //
            """
SELECT "b"."Id", "b"."Name", "b"."Note"
FROM "Blogs" AS "b"
WHERE "b"."Id" = 1
LIMIT 2
""");
    }

    public override async Task Returning_with_do_nothing_returns_only_inserted_rows(bool async)
    {
        await base.Returning_with_do_nothing_returns_only_inserted_rows(async);

        AssertSql(
            """
@p1='1'
@p2='Blog1' (Size = 5)
@p4='3'
@p5='Blog3' (Size = 5)

INSERT INTO "Blogs" ("Id", "Name", "Note")
VALUES (@p1, @p2, NULL),
(@p4, @p5, NULL)
ON CONFLICT ("Id") DO NOTHING
RETURNING "Id";
""");
    }

    public override async Task Returning_reads_null_column_as_default(bool async)
    {
        await base.Returning_reads_null_column_as_default(async);

        AssertSql(
            """
@p1='3'
@p2='Blog3' (Size = 5)

INSERT INTO "Blogs" ("Id", "Name", "Note")
VALUES (@p1, @p2, NULL)
ON CONFLICT ("Id") DO NOTHING
RETURNING "Note";
""");
    }

    public override async Task Returning_with_buffering(bool async)
    {
        await base.Returning_with_buffering(async);

        AssertSql(
            """
@p1='1'
@p2='Updated' (Size = 7)
@p4='3'
@p5='Blog3' (Size = 5)

INSERT INTO "Blogs" ("Id", "Name", "Note")
VALUES (@p1, @p2, NULL),
(@p4, @p5, NULL)
ON CONFLICT ("Id") DO UPDATE SET
"Name" = excluded."Name"
RETURNING "Id", "Name", "Note";
""");
    }

    public override async Task Empty_match_throws(bool async)
    {
        await base.Empty_match_throws(async);

        AssertSql();
    }

    public override async Task Repeated_when_matched_throws(bool async)
    {
        await base.Repeated_when_matched_throws(async);

        AssertSql();
    }

    public override async Task Repeated_when_not_matched_throws(bool async)
    {
        await base.Repeated_when_not_matched_throws(async);

        AssertSql();
    }

    public override async Task Default_insert_skips_computed_columns(bool async)
    {
        await base.Default_insert_skips_computed_columns(async);

        AssertSql(
            """
@p0='1'
@p1='100'

INSERT INTO "Gadgets" ("Id", "Price")
VALUES (@p0, @p1)
RETURNING "Tax";
""",
            //
            """
@p1='1'
@p2='200'
@p3='2'
@p4='500'

INSERT INTO "Gadgets" ("Id", "Price")
VALUES (@p1, @p2),
(@p3, @p4)
ON CONFLICT ("Id") DO UPDATE SET
"Price" = excluded."Price";
""",
            //
            """
SELECT "g"."Id", "g"."Price", "g"."Tax"
FROM "Gadgets" AS "g"
ORDER BY "g"."Id"
""");
    }

    public override async Task Match_on_computed_column_throws(bool async)
    {
        await base.Match_on_computed_column_throws(async);

        AssertSql();
    }

    public override async Task When_not_matched_with_computed_value_throws(bool async)
    {
        await base.When_not_matched_with_computed_value_throws(async);

        AssertSql();
    }

    public override async Task Match_with_computed_source_key_throws(bool async)
    {
        await base.Match_with_computed_source_key_throws(async);

        AssertSql();
    }

    public override async Task When_matched_unsupported_expression_throws(bool async)
    {
        await base.When_matched_unsupported_expression_throws(async);

        AssertSql();
    }

    public override async Task When_matched_with_operator_throws(bool async)
    {
        await base.When_matched_with_operator_throws(async);

        AssertSql();
    }

    public override async Task When_matched_constant_of_incompatible_type_is_not_inlined(bool async)
    {
        await base.When_matched_constant_of_incompatible_type_is_not_inlined(async);

        AssertSql(
            """
SELECT "v"."UserId", "v"."Day", "v"."Visits"
FROM "Visits" AS "v"
LIMIT 2
""");
    }

    public override async Task When_matched_with_narrowing_conversion_throws(bool async)
    {
        await base.When_matched_with_narrowing_conversion_throws(async);

        AssertSql();
    }

    public override async Task When_matched_with_checked_conversion_throws(bool async)
    {
        await base.When_matched_with_checked_conversion_throws(async);

        AssertSql();
    }

    public override async Task When_matched_source_member_inserted_with_other_converter_throws(bool async)
    {
        await base.When_matched_source_member_inserted_with_other_converter_throws(async);

        AssertSql();
    }

    public override async Task When_matched_target_property_with_other_converter_throws(bool async)
    {
        await base.When_matched_target_property_with_other_converter_throws(async);

        AssertSql();
    }

    public override async Task When_not_matched_with_narrowing_conversion_throws(bool async)
    {
        await base.When_not_matched_with_narrowing_conversion_throws(async);

        AssertSql();
    }

    public override async Task Match_with_narrowing_source_conversion_throws(bool async)
    {
        await base.Match_with_narrowing_source_conversion_throws(async);

        AssertSql();
    }

    public override async Task Match_with_narrowing_target_conversion_throws(bool async)
    {
        await base.Match_with_narrowing_target_conversion_throws(async);

        AssertSql();
    }

    public override async Task When_not_matched_without_setters_throws(bool async)
    {
        await base.When_not_matched_without_setters_throws(async);

        AssertSql();
    }

    public override async Task When_not_matched_duplicate_property_throws(bool async)
    {
        await base.When_not_matched_duplicate_property_throws(async);

        AssertSql();
    }

    public override async Task When_matched_duplicate_property_throws(bool async)
    {
        await base.When_matched_duplicate_property_throws(async);

        AssertSql();
    }

    public override async Task When_not_matched_computed_property_throws(bool async)
    {
        await base.When_not_matched_computed_property_throws(async);

        AssertSql();
    }

    public override async Task When_not_matched_setters_omit_match_column_throws(bool async)
    {
        await base.When_not_matched_setters_omit_match_column_throws(async);

        AssertSql();
    }

    public override async Task When_not_matched_match_column_from_other_source_member_throws(bool async)
    {
        await base.When_not_matched_match_column_from_other_source_member_throws(async);

        AssertSql();
    }

    public override async Task When_not_matched_primary_key_from_other_source_member_throws(bool async)
    {
        await base.When_not_matched_primary_key_from_other_source_member_throws(async);

        AssertSql();
    }

    public override async Task When_not_matched_match_column_from_constant_throws(bool async)
    {
        await base.When_not_matched_match_column_from_constant_throws(async);

        AssertSql();
    }

    public override async Task When_matched_source_member_not_inserted_unchanged_throws(bool async)
    {
        await base.When_matched_source_member_not_inserted_unchanged_throws(async);

        AssertSql();
    }

    public override async Task When_matched_nested_member_with_same_name_as_entity_property_throws(bool async)
    {
        await base.When_matched_nested_member_with_same_name_as_entity_property_throws(async);

        AssertSql();
    }

    public override async Task When_not_matched_nested_member_with_same_name_as_entity_property_throws(bool async)
    {
        await base.When_not_matched_nested_member_with_same_name_as_entity_property_throws(async);

        AssertSql();
    }

    public override async Task Duplicate_match_column_throws(bool async)
    {
        await base.Duplicate_match_column_throws(async);

        AssertSql();
    }

    public override async Task Returning_selector_without_target_columns_throws(bool async)
    {
        await base.Returning_selector_without_target_columns_throws(async);

        AssertSql();
    }

    public override async Task Returning_selector_with_captured_variable_throws(bool async)
    {
        await base.Returning_selector_with_captured_variable_throws(async);

        AssertSql();
    }

    public override async Task Null_source_throws(bool async)
    {
        await base.Null_source_throws(async);

        AssertSql();
    }

    public override async Task On_filtered_target_throws(bool async)
    {
        await base.On_filtered_target_throws(async);

        AssertSql();
    }

    public override async Task On_target_with_take_throws(bool async)
    {
        await base.On_target_with_take_throws(async);

        AssertSql();
    }

    public override async Task Without_primary_key_and_no_match_throws(bool async)
    {
        await base.Without_primary_key_and_no_match_throws(async);

        AssertSql();
    }

    public override async Task Property_not_found_throws(bool async)
    {
        await base.Property_not_found_throws(async);

        AssertSql();
    }

    public override async Task Default_insert_missing_source_member_throws(bool async)
    {
        await base.Default_insert_missing_source_member_throws(async);

        AssertSql();
    }

    public override async Task Default_insert_source_member_of_incompatible_type_throws(bool async)
    {
        await base.Default_insert_source_member_of_incompatible_type_throws(async);

        AssertSql();
    }

    public override async Task Default_insert_nullable_source_member_for_non_nullable_property_throws(bool async)
    {
        await base.Default_insert_nullable_source_member_for_non_nullable_property_throws(async);

        AssertSql();
    }

    public override async Task Match_with_nullable_to_non_nullable_source_conversion_throws(bool async)
    {
        await base.Match_with_nullable_to_non_nullable_source_conversion_throws(async);

        AssertSql();
    }

    public override async Task When_not_matched_with_nullable_to_non_nullable_conversion_throws(bool async)
    {
        await base.When_not_matched_with_nullable_to_non_nullable_conversion_throws(async);

        AssertSql();
    }

    public override async Task When_matched_with_nullable_to_non_nullable_conversion_throws(bool async)
    {
        await base.When_matched_with_nullable_to_non_nullable_conversion_throws(async);

        AssertSql();
    }

    public override async Task Default_insert_reads_non_public_property_of_entity_source(bool async)
    {
        await base.Default_insert_reads_non_public_property_of_entity_source(async);

        AssertSql(
            """
@p1='1'
@p2='a' (Size = 1)

INSERT INTO "Token" ("Id", "Secret")
VALUES (@p1, @p2)
ON CONFLICT ("Id") DO NOTHING;
""",
            //
            """
SELECT "t"."Id", "t"."Secret"
FROM "Token" AS "t"
LIMIT 2
""");
    }

    public override async Task When_matched_on_table_named_excluded_reads_source_values(bool async)
    {
        await base.When_matched_on_table_named_excluded_reads_source_values(async);

        AssertSql(
            """
@p1='1'
@p2='new' (Size = 3)

INSERT INTO "excluded" AS "e" ("Id", "Name")
VALUES (@p1, @p2)
ON CONFLICT ("Id") DO UPDATE SET
"Name" = excluded."Name";
""",
            //
            """
SELECT "e"."Id", "e"."Name"
FROM "excluded" AS "e"
LIMIT 2
""");
    }

    public override async Task On_entity_mapped_to_table_and_view_targets_table(bool async)
    {
        await base.On_entity_mapped_to_table_and_view_targets_table(async);

        AssertSql(
            """
@p1='1'
@p2='a' (Size = 1)

INSERT INTO "Widgets" ("Id", "Name")
VALUES (@p1, @p2)
ON CONFLICT ("Id") DO NOTHING;
""",
            //
            """
SELECT "w"."Id", "w"."Name"
FROM "WidgetsView" AS "w"
LIMIT 2
""");
    }

    public override async Task Default_insert_skips_properties_generated_on_add_or_update(bool async)
    {
        await base.Default_insert_skips_properties_generated_on_add_or_update(async);

        AssertSql(
            """
@p1='1'
@p2='a' (Size = 1)

INSERT INTO "Stamped" ("Id", "Name")
VALUES (@p1, @p2)
ON CONFLICT ("Id") DO NOTHING;
""",
            //
            """
SELECT "s"."Id", "s"."Name", "s"."Version"
FROM "Stamped" AS "s"
LIMIT 2
""");
    }

    public override async Task When_not_matched_property_generated_on_add_or_update_throws(bool async)
    {
        await base.When_not_matched_property_generated_on_add_or_update_throws(async);

        AssertSql();
    }

    public override async Task Commands_report_ExecuteMerge_command_source(bool async)
    {
        await base.Commands_report_ExecuteMerge_command_source(async);

        AssertSql(
            """
@p1='1'
@p2='a' (Size = 1)

INSERT INTO "Stamped" ("Id", "Name")
VALUES (@p1, @p2)
ON CONFLICT ("Id") DO NOTHING;
""",
            //
            """
@p1='2'
@p2='b' (Size = 1)

INSERT INTO "Stamped" ("Id", "Name")
VALUES (@p1, @p2)
ON CONFLICT ("Id") DO NOTHING
RETURNING "Id";
""");
    }

    public override async Task Returning_with_nullable_provider_type_converter(bool async)
    {
        await base.Returning_with_nullable_provider_type_converter(async);

        AssertSql(
            """
@p1='1'
@p2='5' (Nullable = true)

INSERT INTO "Rated" ("Id", "Score")
VALUES (@p1, @p2)
ON CONFLICT ("Id") DO NOTHING
RETURNING "Score";
""",
            //
            """
@p1='2'
@p2='7' (Nullable = true)

INSERT INTO "Rated" ("Id", "Score")
VALUES (@p1, @p2)
ON CONFLICT ("Id") DO NOTHING
RETURNING "Score";
""");
    }

    public override async Task Returning_with_converter_that_converts_nulls(bool async)
    {
        await base.Returning_with_converter_that_converts_nulls(async);

        AssertSql(
            """
@p1='1'
@p2=NULL

INSERT INTO "Labeled" ("Id", "Label")
VALUES (@p1, @p2)
ON CONFLICT ("Id") DO NOTHING
RETURNING "Label";
""",
            //
            """
@p1='2'
@p2=NULL

INSERT INTO "Labeled" ("Id", "Label")
VALUES (@p1, @p2)
ON CONFLICT ("Id") DO NOTHING
RETURNING "Label";
""");
    }

    public override async Task On_entity_with_owned_type_throws(bool async)
    {
        await base.On_entity_with_owned_type_throws(async);

        AssertSql();
    }

    public override async Task On_entity_with_complex_property_throws(bool async)
    {
        await base.On_entity_with_complex_property_throws(async);

        AssertSql();
    }

    public override async Task When_matched_source_member_of_incompatible_type_throws(bool async)
    {
        await base.When_matched_source_member_of_incompatible_type_throws(async);

        AssertSql();
    }

    public override async Task When_matched_target_member_of_incompatible_type_throws(bool async)
    {
        await base.When_matched_target_member_of_incompatible_type_throws(async);

        AssertSql();
    }

    public override async Task Returning_selector_capturing_instance_throws(bool async)
    {
        await base.Returning_selector_capturing_instance_throws(async);

        AssertSql();
    }

    public override async Task When_matched_parameter_of_incompatible_type_throws(bool async)
    {
        await base.When_matched_parameter_of_incompatible_type_throws(async);

        AssertSql();
    }

    public override async Task When_matched_captured_value_of_incompatible_type_throws(bool async)
    {
        await base.When_matched_captured_value_of_incompatible_type_throws(async);

        AssertSql();
    }

    public override async Task When_not_matched_parameter_of_incompatible_type_throws(bool async)
    {
        await base.When_not_matched_parameter_of_incompatible_type_throws(async);

        AssertSql();
    }

    public override async Task Boxed_parameters_of_property_type(bool async)
    {
        await base.Boxed_parameters_of_property_type(async);

        AssertSql(
            """
@p21='1'
@p22='mon' (Size = 3)
@p1='20' (Nullable = true)
@p23='2'
@p24='tue' (Size = 3)
@p='10' (Nullable = true)

INSERT INTO "Visits" ("UserId", "Day", "Visits")
VALUES (@p21, @p22, @p1),
(@p23, @p24, @p1)
ON CONFLICT ("UserId", "Day") DO UPDATE SET
"Visits" = @p;
""",
            //
            """
SELECT "v"."UserId", "v"."Day", "v"."Visits"
FROM "Visits" AS "v"
ORDER BY "v"."UserId"
""");
    }

    public override async Task Nullable_value_type_property(bool async)
    {
        await base.Nullable_value_type_property(async);

        AssertSql(
            """
@p1='1'
@p3='2'
@p4='5' (Nullable = true)

INSERT INTO "Starred" ("Id", "Stars")
VALUES (@p1, NULL),
(@p3, @p4)
ON CONFLICT ("Id") DO UPDATE SET
"Stars" = excluded."Stars"
RETURNING "Stars";
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Returning_on_table_without_returning_clause_throws(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<WidgetWithoutReturningContext>();
        await using var context = contextFactory.CreateDbContext();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => MergeReturning(async, context.Set<Widget>(), [new Widget { Id = 1, Name = "a" }], merge => { }, w => w.Id));

        Assert.Equal(SqliteStrings.ExecuteMergeReturningWithoutReturningClause("Widgets"), exception.InnerException!.Message);

        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected class WidgetWithoutReturningContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Widget>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.ToTable("Widgets", tb => tb.UseSqlReturningClause(false));
                });
    }
}
