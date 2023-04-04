// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class NonSharedModelBulkUpdatesSqliteTest : NonSharedModelBulkUpdatesTestBase
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

    public override async Task Update_non_owned_property_on_entity_with_owned(bool async)
    {
        await base.Update_non_owned_property_on_entity_with_owned(async);

        AssertSql(
"""
UPDATE "Owner" AS "o"
SET "Title" = 'SomeValue'
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

    public override Task Delete_entity_with_auto_include(bool async)
        => Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => base.Delete_entity_with_auto_include(async));

    public override async Task Delete_predicate_based_on_optional_navigation(bool async)
    {
        await base.Delete_predicate_based_on_optional_navigation(async);

        AssertSql(
"""
DELETE FROM "Posts" AS "p"
WHERE EXISTS (
    SELECT 1
    FROM "Posts" AS "p0"
    LEFT JOIN "Blogs" AS "b" ON "p0"."BlogId" = "b"."Id"
    WHERE ("b"."Title" IS NOT NULL) AND ("b"."Title" LIKE 'Arthur%') AND "p0"."Id" = "p"."Id")
""");
    }

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(wcb => wcb.Log(SqliteEventId.CompositeKeyWithValueGeneration));

    private void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    private void AssertExecuteUpdateSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}
