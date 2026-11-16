// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class CustomConvertersSqliteTest : CustomConvertersTestBase<CustomConvertersSqliteTest.CustomConvertersSqliteFixture>
{
    public CustomConvertersSqliteTest(CustomConvertersSqliteFixture fixture)
        : base(fixture)
        => Fixture.TestSqlLoggerFactory.Clear();

    // Disabled: SQLite database is case-sensitive
    public override Task Can_insert_and_read_back_with_case_insensitive_string_key()
        => Task.CompletedTask;

    [Fact]
    public override async Task Value_conversion_is_appropriately_used_for_join_condition()
    {
        await base.Value_conversion_is_appropriately_used_for_join_condition();

        AssertSql(
            """
@blogId='1'

SELECT "b"."Url"
FROM "Blog" AS "b"
INNER JOIN "Post" AS "p" ON "b"."BlogId" = "p"."BlogId" AND "b"."IsVisible" = 'Y' AND "b"."BlogId" = @blogId
WHERE "b"."IsVisible" = 'Y'
""");
    }

    [Fact]
    public override async Task Value_conversion_is_appropriately_used_for_left_join_condition()
    {
        await base.Value_conversion_is_appropriately_used_for_left_join_condition();

        AssertSql(
            """
@blogId='1'

SELECT "b"."Url"
FROM "Blog" AS "b"
LEFT JOIN "Post" AS "p" ON "b"."BlogId" = "p"."BlogId" AND "b"."IsVisible" = 'Y' AND "b"."BlogId" = @blogId
WHERE "b"."IsVisible" = 'Y'
""");
    }

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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
            Assert.Throws<InvalidOperationException>(base.Value_conversion_on_enum_collection_contains).Message);

    public override async Task Value_converter_in_complex_type_on_owned_reference()
    {
        await base.Value_converter_in_complex_type_on_owned_reference();

        AssertSql(
            """
@p0='[{"Code":"A","Id":30,"Label":"Ten","RelatedIds":[]}]' (Nullable = false) (Size = 52)
@p3='1'
@p1='3'
@p2='[5,6,7]' (Nullable = false) (Size = 7)

UPDATE "OwnerWithConvertedTag" SET "Owned_Tags" = @p0, "Owned_Tag_Id" = @p1, "Owned_Tag_RelatedIds" = @p2
WHERE "Id" = @p3
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Owned_Tag_Code", "o"."Owned_Tag_Id", "o"."Owned_Tag_Label", "o"."Owned_Tag_RelatedIds", "o"."Owned_Tags"
FROM "OwnerWithConvertedTag" AS "o"
WHERE "o"."Owned_Tag_Id" = 3
LIMIT 2
""");
    }

    public override async Task Value_comparer_in_complex_type_on_owned_reference()
    {
        await base.Value_comparer_in_complex_type_on_owned_reference();

        AssertSql(
            """
SELECT "o"."Id", "o"."Owned_Tag_Code", "o"."Owned_Tag_Id", "o"."Owned_Tag_Label", "o"."Owned_Tag_RelatedIds", "o"."Owned_Tags"
FROM "OwnerWithComparedTag" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""",
            //
            """
@p1='1'
@p0='5,6,7' (Nullable = false) (Size = 5)

UPDATE "OwnerWithComparedTag" SET "Owned_Tag_RelatedIds" = @p0
WHERE "Id" = @p1
RETURNING 1;
""",
            //
            """
SELECT "o"."Id", "o"."Owned_Tag_Code", "o"."Owned_Tag_Id", "o"."Owned_Tag_Label", "o"."Owned_Tag_RelatedIds", "o"."Owned_Tags"
FROM "OwnerWithComparedTag" AS "o"
WHERE "o"."Id" = 1
LIMIT 2
""");
    }

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

        // In relational, complex collections are only supported as JSON and must be explicitly configured as such
        protected override void ConfigureTagCollection(ComplexCollectionBuilder<Tag> builder)
            => builder.ToJson();
    }
}
