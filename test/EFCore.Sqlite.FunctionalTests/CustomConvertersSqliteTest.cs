// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class CustomConvertersSqliteTest : CustomConvertersTestBase<CustomConvertersSqliteTest.CustomConvertersSqliteFixture>
{
    public CustomConvertersSqliteTest(CustomConvertersSqliteFixture fixture)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
    }

    // Disabled: SQLite database is case-sensitive
    public override Task Can_insert_and_read_back_with_case_insensitive_string_key()
        => Task.CompletedTask;

    [ConditionalFact]
    public override async Task Value_conversion_is_appropriately_used_for_join_condition()
    {
        await base.Value_conversion_is_appropriately_used_for_join_condition();

        AssertSql(
            """
@__blogId_0='1'

SELECT "b"."Url"
FROM "Blog" AS "b"
INNER JOIN "Post" AS "p" ON "b"."BlogId" = "p"."BlogId" AND "b"."IsVisible" = 'Y' AND "b"."BlogId" = @__blogId_0
WHERE "b"."IsVisible" = 'Y'
""");
    }

    [ConditionalFact]
    public override async Task Value_conversion_is_appropriately_used_for_left_join_condition()
    {
        await base.Value_conversion_is_appropriately_used_for_left_join_condition();

        AssertSql(
            """
@__blogId_0='1'

SELECT "b"."Url"
FROM "Blog" AS "b"
LEFT JOIN "Post" AS "p" ON "b"."BlogId" = "p"."BlogId" AND "b"."IsVisible" = 'Y' AND "b"."BlogId" = @__blogId_0
WHERE "b"."IsVisible" = 'Y'
""");
    }

    [ConditionalFact]
    public override async Task Where_bool_gets_converted_to_equality_when_value_conversion_is_used()
    {
        await base.Where_bool_gets_converted_to_equality_when_value_conversion_is_used();

        AssertSql(
            """
SELECT "b"."BlogId", "b"."Discriminator", "b"."IndexerVisible", "b"."IsVisible", "b"."Url", "b"."RssUrl"
FROM "Blog" AS "b"
WHERE "b"."IsVisible" = 'Y'
""");
    }

    [ConditionalFact]
    public override async Task Where_negated_bool_gets_converted_to_equality_when_value_conversion_is_used()
    {
        await base.Where_negated_bool_gets_converted_to_equality_when_value_conversion_is_used();

        AssertSql(
            """
SELECT "b"."BlogId", "b"."Discriminator", "b"."IndexerVisible", "b"."IsVisible", "b"."Url", "b"."RssUrl"
FROM "Blog" AS "b"
WHERE "b"."IsVisible" = 'N'
""");
    }

    public override async Task Where_bool_with_value_conversion_inside_comparison_doesnt_get_converted_twice()
    {
        await base.Where_bool_with_value_conversion_inside_comparison_doesnt_get_converted_twice();

        AssertSql(
            """
SELECT "b"."BlogId", "b"."Discriminator", "b"."IndexerVisible", "b"."IsVisible", "b"."Url", "b"."RssUrl"
FROM "Blog" AS "b"
WHERE "b"."IsVisible" = 'Y'
""",
            //
            """
SELECT "b"."BlogId", "b"."Discriminator", "b"."IndexerVisible", "b"."IsVisible", "b"."Url", "b"."RssUrl"
FROM "Blog" AS "b"
WHERE "b"."IsVisible" <> 'Y'
""");
    }

    public override async Task Select_bool_with_value_conversion_is_used()
    {
        await base.Select_bool_with_value_conversion_is_used();

        AssertSql(
            """
SELECT "b"."IsVisible"
FROM "Blog" AS "b"
""");
    }

    [ConditionalFact]
    public override async Task Where_bool_gets_converted_to_equality_when_value_conversion_is_used_using_EFProperty()
    {
        await base.Where_bool_gets_converted_to_equality_when_value_conversion_is_used_using_EFProperty();

        AssertSql(
            """
SELECT "b"."BlogId", "b"."Discriminator", "b"."IndexerVisible", "b"."IsVisible", "b"."Url", "b"."RssUrl"
FROM "Blog" AS "b"
WHERE "b"."IsVisible" = 'Y'
""");
    }

    [ConditionalFact]
    public override async Task Where_bool_gets_converted_to_equality_when_value_conversion_is_used_using_indexer()
    {
        await base.Where_bool_gets_converted_to_equality_when_value_conversion_is_used_using_indexer();

        AssertSql(
            """
SELECT "b"."BlogId", "b"."Discriminator", "b"."IndexerVisible", "b"."IsVisible", "b"."Url", "b"."RssUrl"
FROM "Blog" AS "b"
WHERE "b"."IndexerVisible" = 'Nay'
""");
    }

    public override void Value_conversion_on_enum_collection_contains()
        => Assert.Contains(
            CoreStrings.TranslationFailed("")[47..],
            Assert.Throws<InvalidOperationException>(() => base.Value_conversion_on_enum_collection_contains()).Message);

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class CustomConvertersSqliteFixture : CustomConvertersFixtureBase, ITestSqlLoggerFactory
    {
        public override bool StrictEquality
            => false;

        public override bool SupportsAnsi
            => false;

        public override bool SupportsUnicodeToAnsiConversion
            => true;

        public override bool SupportsLargeStringComparisons
            => true;

        public override bool SupportsDecimalComparisons
            => false;

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        public override bool SupportsBinaryKeys
            => true;

        public override DateTime DefaultDateTime
            => new();

        public override bool PreservesDateTimeKind
            => true;
    }
}
