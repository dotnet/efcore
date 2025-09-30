// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

#nullable disable

[SqlServerCondition(SqlServerCondition.IsNotAzureSql)]
public class CustomConvertersSqlServerTest : CustomConvertersTestBase<CustomConvertersSqlServerTest.CustomConvertersSqlServerFixture>
{
    public CustomConvertersSqlServerTest(CustomConvertersSqlServerFixture fixture)
        : base(fixture)
        => Fixture.TestSqlLoggerFactory.Clear();

    [ConditionalFact]
    public override async Task Value_conversion_is_appropriately_used_for_join_condition()
    {
        await base.Value_conversion_is_appropriately_used_for_join_condition();

        AssertSql(
            """
@blogId='1'

SELECT [b].[Url]
FROM [Blog] AS [b]
INNER JOIN [Post] AS [p] ON [b].[BlogId] = [p].[BlogId] AND [b].[IsVisible] = N'Y' AND [b].[BlogId] = @blogId
WHERE [b].[IsVisible] = N'Y'
""");
    }

    [ConditionalFact]
    public override async Task Value_conversion_is_appropriately_used_for_left_join_condition()
    {
        await base.Value_conversion_is_appropriately_used_for_left_join_condition();

        AssertSql(
            """
@blogId='1'

SELECT [b].[Url]
FROM [Blog] AS [b]
LEFT JOIN [Post] AS [p] ON [b].[BlogId] = [p].[BlogId] AND [b].[IsVisible] = N'Y' AND [b].[BlogId] = @blogId
WHERE [b].[IsVisible] = N'Y'
""");
    }

    [ConditionalFact]
    public override async Task Where_bool_gets_converted_to_equality_when_value_conversion_is_used()
    {
        await base.Where_bool_gets_converted_to_equality_when_value_conversion_is_used();

        AssertSql(
            """
SELECT [b].[BlogId], [b].[Discriminator], [b].[IndexerVisible], [b].[IsVisible], [b].[Url], [b].[RssUrl]
FROM [Blog] AS [b]
WHERE [b].[IsVisible] = N'Y'
""");
    }

    [ConditionalFact]
    public override async Task Where_negated_bool_gets_converted_to_equality_when_value_conversion_is_used()
    {
        await base.Where_negated_bool_gets_converted_to_equality_when_value_conversion_is_used();

        AssertSql(
            """
SELECT [b].[BlogId], [b].[Discriminator], [b].[IndexerVisible], [b].[IsVisible], [b].[Url], [b].[RssUrl]
FROM [Blog] AS [b]
WHERE [b].[IsVisible] = N'N'
""");
    }

    public override async Task Where_bool_gets_converted_to_equality_when_value_conversion_is_used_using_EFProperty()
    {
        await base.Where_bool_gets_converted_to_equality_when_value_conversion_is_used_using_EFProperty();

        AssertSql(
            """
SELECT [b].[BlogId], [b].[Discriminator], [b].[IndexerVisible], [b].[IsVisible], [b].[Url], [b].[RssUrl]
FROM [Blog] AS [b]
WHERE [b].[IsVisible] = N'Y'
""");
    }

    public override async Task Where_bool_gets_converted_to_equality_when_value_conversion_is_used_using_indexer()
    {
        await base.Where_bool_gets_converted_to_equality_when_value_conversion_is_used_using_indexer();

        AssertSql(
            """
SELECT [b].[BlogId], [b].[Discriminator], [b].[IndexerVisible], [b].[IsVisible], [b].[Url], [b].[RssUrl]
FROM [Blog] AS [b]
WHERE [b].[IndexerVisible] = N'Nay'
""");
    }

    public override Task Object_to_string_conversion()
        // Return values are not string
        => Task.CompletedTask;

    public override async Task Id_object_as_entity_key()
    {
        await base.Id_object_as_entity_key();

        AssertSql(
            """
SELECT [b].[Id], [b].[Value]
FROM [Book] AS [b]
WHERE [b].[Id] = 1
""");
    }

    public override void Value_conversion_on_enum_collection_contains()
        => Assert.Contains(
            CoreStrings.TranslationFailed("")[47..],
            Assert.Throws<InvalidOperationException>(() => base.Value_conversion_on_enum_collection_contains()).Message);

    [ConditionalTheory(Skip = "Issue #30730: TODO need to find the default type mapping."), InlineData(true), InlineData(false)]
    public virtual async Task SqlQuery_with_converted_type_using_model_configuration_builder_works(bool async)
    {
        using var context = CreateContext();
        var query = context.Database.SqlQueryRaw<HoldingEnum>("SELECT [HoldingEnum] FROM [HolderClass]");

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Equal(HoldingEnum.Value2, result.Single());

        AssertSql(
            """
SELECT [HoldingEnum] FROM [HolderClass]
""");
    }

    public override void Infer_type_mapping_from_in_subquery_to_item()
    {
        base.Infer_type_mapping_from_in_subquery_to_item();

        AssertSql(
            """
SELECT [b].[Id], [b].[Enum16], [b].[Enum32], [b].[Enum64], [b].[Enum8], [b].[EnumS8], [b].[EnumU16], [b].[EnumU32], [b].[EnumU64], [b].[PartitionId], [b].[TestBoolean], [b].[TestByte], [b].[TestCharacter], [b].[TestDateOnly], [b].[TestDateTime], [b].[TestDateTimeOffset], [b].[TestDecimal], [b].[TestDouble], [b].[TestInt16], [b].[TestInt32], [b].[TestInt64], [b].[TestSignedByte], [b].[TestSingle], [b].[TestTimeOnly], [b].[TestTimeSpan], [b].[TestUnsignedInt16], [b].[TestUnsignedInt32], [b].[TestUnsignedInt64]
FROM [BuiltInDataTypes] AS [b]
WHERE N'Yeps' IN (
    SELECT [b0].[TestBoolean]
    FROM [BuiltInDataTypes] AS [b0]
) AND [b].[Id] = 13
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class CustomConvertersSqlServerFixture : CustomConvertersFixtureBase
    {
        public override bool StrictEquality
            => true;

        public override bool SupportsAnsi
            => true;

        public override bool SupportsUnicodeToAnsiConversion
            => true;

        public override bool SupportsLargeStringComparisons
            => true;

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        public override bool SupportsBinaryKeys
            => true;

        public override bool SupportsDecimalComparisons
            => true;

        public override DateTime DefaultDateTime
            => new();

        public override bool PreservesDateTimeKind
            => false;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base
                .AddOptions(builder)
                .ConfigureWarnings(c => c.Log(SqlServerEventId.DecimalTypeDefaultWarning));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<BuiltInDataTypes>().Property(e => e.TestBoolean).IsFixedLength();
        }
    }
}
