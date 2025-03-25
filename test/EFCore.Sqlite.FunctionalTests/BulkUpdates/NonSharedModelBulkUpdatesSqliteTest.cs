// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public class NonSharedModelBulkUpdatesSqliteTest(NonSharedFixture fixture) : NonSharedModelBulkUpdatesRelationalTestBase(fixture)
{
    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Delete_aggregate_root_when_eager_loaded_owned_collection(bool async)
    {
        await base.Delete_aggregate_root_when_eager_loaded_owned_collection(async);

        AssertSql(
            """
DELETE FROM "Owner" AS "o"
""");
    }

    public override async Task Delete_with_owned_collection_and_non_natively_translatable_query(bool async)
    {
        await base.Delete_with_owned_collection_and_non_natively_translatable_query(async);

        AssertSql(
            """
@p='1'

DELETE FROM "Owner" AS "o"
WHERE "o"."Id" IN (
    SELECT "o0"."Id"
    FROM "Owner" AS "o0"
    ORDER BY "o0"."Title"
    LIMIT -1 OFFSET @p
)
""");
    }

    public override async Task Delete_aggregate_root_when_table_sharing_with_owned(bool async)
    {
        await base.Delete_aggregate_root_when_table_sharing_with_owned(async);

        AssertSql(
            """
DELETE FROM "Owner" AS "o"
""");
    }

    public override async Task Delete_aggregate_root_when_table_sharing_with_non_owned_throws(bool async)
    {
        await base.Delete_aggregate_root_when_table_sharing_with_non_owned_throws(async);

        AssertSql();
    }

    public override async Task Replace_ColumnExpression_in_column_setter(bool async)
    {
        // #33947
        await Assert.ThrowsAsync<SqliteException>(() => base.Replace_ColumnExpression_in_column_setter(async));

        AssertSql(
            """
@p='SomeValue' (Size = 9)

UPDATE "OwnedCollection" AS "o0"
SET "Value" = @p
FROM "Owner" AS "o"
INNER JOIN "OwnedCollection" AS "o0" ON "o"."Id" = "o0"."OwnerId"
""");
    }

    public override async Task Update_non_owned_property_on_entity_with_owned(bool async)
    {
        await base.Update_non_owned_property_on_entity_with_owned(async);

        AssertSql(
            """
@p='SomeValue' (Size = 9)

UPDATE "Owner" AS "o"
SET "Title" = @p
""");
    }

    public override async Task Update_non_owned_property_on_entity_with_owned2(bool async)
    {
        await base.Update_non_owned_property_on_entity_with_owned2(async);

        AssertSql(
            """
UPDATE "Owner" AS "o"
SET "Title" = COALESCE("o"."Title", '') || '_Suffix'
""");
    }

    public override async Task Update_non_owned_property_on_entity_with_owned_in_join(bool async)
    {
        await base.Update_non_owned_property_on_entity_with_owned_in_join(async);

        AssertSql(
            """
@p='NewValue' (Size = 8)

UPDATE "Owner" AS "o"
SET "Title" = @p
FROM "Owner" AS "o0"
WHERE "o"."Id" = "o0"."Id"
""");
    }

    public override async Task Update_owned_and_non_owned_properties_with_table_sharing(bool async)
    {
        await base.Update_owned_and_non_owned_properties_with_table_sharing(async);

        AssertSql(
            """
UPDATE "Owner" AS "o"
SET "Title" = COALESCE(CAST("o"."OwnedReference_Number" AS TEXT), ''),
    "OwnedReference_Number" = length("o"."Title")
""");
    }

    public override async Task Update_main_table_in_entity_with_entity_splitting(bool async)
    {
        await base.Update_main_table_in_entity_with_entity_splitting(async);

        AssertSql(
            """
UPDATE "Blogs" AS "b"
SET "CreationTimestamp" = '2020-01-01 00:00:00'
""");
    }

    public override async Task Update_non_main_table_in_entity_with_entity_splitting(bool async)
    {
        await base.Update_non_main_table_in_entity_with_entity_splitting(async);

        AssertSql(
            """
UPDATE "BlogsPart1" AS "b0"
SET "Title" = CAST("b0"."Rating" AS TEXT),
    "Rating" = length("b0"."Title")
FROM "Blogs" AS "b"
WHERE "b"."Id" = "b0"."Id"
""");
    }

    public override async Task Delete_entity_with_auto_include(bool async)
    {
        await base.Delete_entity_with_auto_include(async);

        AssertSql(
            """
DELETE FROM "Context30572_Principal" AS "c"
WHERE "c"."Id" IN (
    SELECT "c0"."Id"
    FROM "Context30572_Principal" AS "c0"
    LEFT JOIN "Context30572_Dependent" AS "c1" ON "c0"."DependentId" = "c1"."Id"
)
""");
    }

    public override async Task Delete_predicate_based_on_optional_navigation(bool async)
    {
        await base.Delete_predicate_based_on_optional_navigation(async);

        AssertSql(
            """
DELETE FROM "Posts" AS "p"
WHERE "p"."Id" IN (
    SELECT "p0"."Id"
    FROM "Posts" AS "p0"
    LEFT JOIN "Blogs" AS "b" ON "p0"."BlogId" = "b"."Id"
    WHERE "b"."Title" LIKE 'Arthur%'
)
""");
    }

    public override async Task Update_with_alias_uniquification_in_setter_subquery(bool async)
    {
        await base.Update_with_alias_uniquification_in_setter_subquery(async);

        AssertSql(
            """
UPDATE "Orders" AS "o"
SET "Total" = (
    SELECT COALESCE(SUM("o0"."Amount"), 0)
    FROM "OrderProduct" AS "o0"
    WHERE "o"."Id" = "o0"."OrderId")
WHERE "o"."Id" = 1
""");
    }

    public override async Task Delete_with_view_mapping(bool async)
    {
        await base.Delete_with_view_mapping(async);

        AssertSql(
            """
DELETE FROM "Blogs" AS "b"
""");
    }

    public override async Task Update_with_view_mapping(bool async)
    {
        await base.Update_with_view_mapping(async);

        AssertSql(
            """
@p='Updated' (Size = 7)

UPDATE "Blogs" AS "b"
SET "Data" = @p
""");
    }

    public override async Task Update_complex_type_with_view_mapping(bool async)
    {
        await base.Update_complex_type_with_view_mapping(async);

        // #34706
        AssertSql();
    }

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(wcb => wcb.Log(SqliteEventId.CompositeKeyWithValueGeneration));

    private void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    private void AssertExecuteUpdateSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}
