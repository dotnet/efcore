// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

#nullable disable

[SqlServerCondition(SqlServerCondition.IsNotSqlAzure)]
public class CustomConvertersSqlServerTest : CustomConvertersTestBase<CustomConvertersSqlServerTest.CustomConvertersSqlServerFixture>
{
    public CustomConvertersSqlServerTest(CustomConvertersSqlServerFixture fixture)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
    }

    [ConditionalFact]
    public virtual void Columns_have_expected_data_types()
    {
        var actual = BuiltInDataTypesSqlServerTest.QueryForColumnTypes(
            CreateContext(),
            nameof(ObjectBackedDataTypes), nameof(NullableBackedDataTypes), nameof(NonNullableBackedDataTypes));

        const string expected =
            """
Animal.Id ---> [int] [Precision = 10 Scale = 0]
AnimalDetails.AnimalId ---> [nullable int] [Precision = 10 Scale = 0]
AnimalDetails.BoolField ---> [int] [Precision = 10 Scale = 0]
AnimalDetails.Id ---> [int] [Precision = 10 Scale = 0]
AnimalIdentification.AnimalId ---> [int] [Precision = 10 Scale = 0]
AnimalIdentification.Id ---> [int] [Precision = 10 Scale = 0]
AnimalIdentification.Method ---> [int] [Precision = 10 Scale = 0]
BinaryForeignKeyDataType.BinaryKeyDataTypeId ---> [nullable varbinary] [MaxLength = 900]
BinaryForeignKeyDataType.Id ---> [int] [Precision = 10 Scale = 0]
BinaryKeyDataType.Ex ---> [nullable nvarchar] [MaxLength = -1]
BinaryKeyDataType.Id ---> [varbinary] [MaxLength = 900]
Blog.BlogId ---> [int] [Precision = 10 Scale = 0]
Blog.Discriminator ---> [nvarchar] [MaxLength = 8]
Blog.IndexerVisible ---> [nvarchar] [MaxLength = 3]
Blog.IsVisible ---> [nvarchar] [MaxLength = 1]
Blog.RssUrl ---> [nullable nvarchar] [MaxLength = -1]
Blog.Url ---> [nullable nvarchar] [MaxLength = -1]
Book.Id ---> [int] [Precision = 10 Scale = 0]
Book.Value ---> [nullable nvarchar] [MaxLength = -1]
BuiltInDataTypes.Enum16 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.Enum32 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.Enum64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.Enum8 ---> [varchar] [MaxLength = -1]
BuiltInDataTypes.EnumS8 ---> [nvarchar] [MaxLength = 24]
BuiltInDataTypes.EnumU16 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypes.EnumU32 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypes.EnumU64 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.PartitionId ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestBoolean ---> [nchar] [MaxLength = 4]
BuiltInDataTypes.TestByte ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.TestCharacter ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.TestDateOnly ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypes.TestDateTime ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestDateTimeOffset ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestDecimal ---> [varbinary] [MaxLength = 16]
BuiltInDataTypes.TestDouble ---> [decimal] [Precision = 26 Scale = 16]
BuiltInDataTypes.TestInt16 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestInt32 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestInt64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestSignedByte ---> [decimal] [Precision = 18 Scale = 2]
BuiltInDataTypes.TestSingle ---> [float] [Precision = 53]
BuiltInDataTypes.TestTimeOnly ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestTimeSpan ---> [float] [Precision = 53]
BuiltInDataTypes.TestUnsignedInt16 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypes.TestUnsignedInt32 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypes.TestUnsignedInt64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.Enum16 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.Enum32 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.Enum64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.Enum8 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypesShadow.EnumS8 ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypesShadow.EnumU16 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypesShadow.EnumU32 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypesShadow.EnumU64 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypesShadow.Id ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypesShadow.PartitionId ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestBoolean ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypesShadow.TestByte ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypesShadow.TestCharacter ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypesShadow.TestDateOnly ---> [nvarchar] [MaxLength = -1]
BuiltInDataTypesShadow.TestDateTime ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestDateTimeOffset ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestDecimal ---> [varbinary] [MaxLength = 16]
BuiltInDataTypesShadow.TestDouble ---> [decimal] [Precision = 26 Scale = 16]
BuiltInDataTypesShadow.TestInt16 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestInt32 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestInt64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestSignedByte ---> [decimal] [Precision = 18 Scale = 2]
BuiltInDataTypesShadow.TestSingle ---> [float] [Precision = 53]
BuiltInDataTypesShadow.TestTimeOnly ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestTimeSpan ---> [float] [Precision = 53]
BuiltInDataTypesShadow.TestUnsignedInt16 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypesShadow.TestUnsignedInt32 ---> [decimal] [Precision = 20 Scale = 0]
BuiltInDataTypesShadow.TestUnsignedInt64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.Enum16 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.Enum32 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.Enum64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.Enum8 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypes.EnumS8 ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypes.EnumU16 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.EnumU32 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.EnumU64 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.PartitionId ---> [bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestByteArray ---> [nullable varbinary] [MaxLength = -1]
BuiltInNullableDataTypes.TestNullableBoolean ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypes.TestNullableByte ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.TestNullableCharacter ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.TestNullableDateOnly ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypes.TestNullableDateTime ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableDateTimeOffset ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableDecimal ---> [nullable varbinary] [MaxLength = 16]
BuiltInNullableDataTypes.TestNullableDouble ---> [nullable decimal] [Precision = 26 Scale = 16]
BuiltInNullableDataTypes.TestNullableInt16 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableInt32 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableInt64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableSignedByte ---> [nullable decimal] [Precision = 18 Scale = 2]
BuiltInNullableDataTypes.TestNullableSingle ---> [nullable float] [Precision = 53]
BuiltInNullableDataTypes.TestNullableTimeOnly ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableTimeSpan ---> [nullable float] [Precision = 53]
BuiltInNullableDataTypes.TestNullableUnsignedInt16 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.TestNullableUnsignedInt32 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.TestNullableUnsignedInt64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestString ---> [nullable nvarchar] [MaxLength = -1]
BuiltInNullableDataTypesShadow.Enum16 ---> [nullable smallint] [Precision = 5 Scale = 0]
BuiltInNullableDataTypesShadow.Enum32 ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.Enum64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.Enum8 ---> [nullable tinyint] [Precision = 3 Scale = 0]
BuiltInNullableDataTypesShadow.EnumS8 ---> [nullable smallint] [Precision = 5 Scale = 0]
BuiltInNullableDataTypesShadow.EnumU16 ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.EnumU32 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.EnumU64 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypesShadow.Id ---> [int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.PartitionId ---> [int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.TestByteArray ---> [nullable varbinary] [MaxLength = -1]
BuiltInNullableDataTypesShadow.TestNullableBoolean ---> [nullable bit]
BuiltInNullableDataTypesShadow.TestNullableByte ---> [nullable tinyint] [Precision = 3 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableCharacter ---> [nullable nvarchar] [MaxLength = 1]
BuiltInNullableDataTypesShadow.TestNullableDateOnly ---> [nullable date] [Precision = 0]
BuiltInNullableDataTypesShadow.TestNullableDateTime ---> [nullable datetime2] [Precision = 7]
BuiltInNullableDataTypesShadow.TestNullableDateTimeOffset ---> [nullable datetimeoffset] [Precision = 7]
BuiltInNullableDataTypesShadow.TestNullableDecimal ---> [nullable decimal] [Precision = 18 Scale = 2]
BuiltInNullableDataTypesShadow.TestNullableDouble ---> [nullable float] [Precision = 53]
BuiltInNullableDataTypesShadow.TestNullableInt16 ---> [nullable smallint] [Precision = 5 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableInt32 ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableInt64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableSignedByte ---> [nullable smallint] [Precision = 5 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableSingle ---> [nullable real] [Precision = 24]
BuiltInNullableDataTypesShadow.TestNullableTimeOnly ---> [nullable time] [Precision = 7]
BuiltInNullableDataTypesShadow.TestNullableTimeSpan ---> [nullable time] [Precision = 7]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt16 ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt32 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt64 ---> [nullable decimal] [Precision = 20 Scale = 0]
BuiltInNullableDataTypesShadow.TestString ---> [nullable nvarchar] [MaxLength = -1]
CollectionEnum.Id ---> [int] [Precision = 10 Scale = 0]
CollectionEnum.Roles ---> [nullable nvarchar] [MaxLength = -1]
CollectionScalar.Id ---> [int] [Precision = 10 Scale = 0]
CollectionScalar.Tags ---> [nullable nvarchar] [MaxLength = -1]
Dashboard.Id ---> [int] [Precision = 10 Scale = 0]
Dashboard.Layouts ---> [nullable nvarchar] [MaxLength = -1]
Dashboard.Name ---> [nullable nvarchar] [MaxLength = -1]
DateTimeEnclosure.DateTimeOffset ---> [nullable datetimeoffset] [Precision = 7]
DateTimeEnclosure.Id ---> [int] [Precision = 10 Scale = 0]
EmailTemplate.Id ---> [uniqueidentifier]
EmailTemplate.TemplateType ---> [int] [Precision = 10 Scale = 0]
Entity.Id ---> [int] [Precision = 10 Scale = 0]
Entity.SomeEnum ---> [nvarchar] [MaxLength = -1]
EntityWithValueWrapper.Id ---> [int] [Precision = 10 Scale = 0]
EntityWithValueWrapper.Wrapper ---> [nullable nvarchar] [MaxLength = -1]
HolderClass.HoldingEnum ---> [int] [Precision = 10 Scale = 0]
HolderClass.Id ---> [int] [Precision = 10 Scale = 0]
Load.Fuel ---> [float] [Precision = 53]
Load.LoadId ---> [int] [Precision = 10 Scale = 0]
MaxLengthDataTypes.ByteArray5 ---> [nullable varbinary] [MaxLength = 7]
MaxLengthDataTypes.ByteArray9000 ---> [nullable nvarchar] [MaxLength = -1]
MaxLengthDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
MaxLengthDataTypes.String3 ---> [nullable nvarchar] [MaxLength = 12]
MaxLengthDataTypes.String9000 ---> [nullable varbinary] [MaxLength = -1]
MaxLengthDataTypes.StringUnbounded ---> [nullable varbinary] [MaxLength = -1]
NonNullableDependent.Id ---> [int] [Precision = 10 Scale = 0]
NonNullableDependent.PrincipalId ---> [int] [Precision = 10 Scale = 0]
NullablePrincipal.Id ---> [int] [Precision = 10 Scale = 0]
Order.Id ---> [nvarchar] [MaxLength = 450]
Parent.Id ---> [int] [Precision = 10 Scale = 0]
Parent.OwnedWithConverter_Value ---> [nullable nvarchar] [MaxLength = 64]
Person.Id ---> [int] [Precision = 10 Scale = 0]
Person.Name ---> [nullable nvarchar] [MaxLength = -1]
Person.SSN ---> [nullable int] [Precision = 10 Scale = 0]
Post.BlogId ---> [nullable int] [Precision = 10 Scale = 0]
Post.PostId ---> [int] [Precision = 10 Scale = 0]
SimpleCounter.CounterId ---> [int] [Precision = 10 Scale = 0]
SimpleCounter.Discriminator ---> [nullable nvarchar] [MaxLength = -1]
SimpleCounter.IsTest ---> [bit]
SimpleCounter.StyleKey ---> [nullable nvarchar] [MaxLength = -1]
StringEnclosure.Id ---> [int] [Precision = 10 Scale = 0]
StringEnclosure.Value ---> [nullable nvarchar] [MaxLength = -1]
StringForeignKeyDataType.Id ---> [int] [Precision = 10 Scale = 0]
StringForeignKeyDataType.StringKeyDataTypeId ---> [nullable nvarchar] [MaxLength = 450]
StringKeyDataType.Id ---> [nvarchar] [MaxLength = 450]
StringListDataType.Id ---> [int] [Precision = 10 Scale = 0]
StringListDataType.Strings ---> [nullable nvarchar] [MaxLength = -1]
UnicodeDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
UnicodeDataTypes.StringAnsi ---> [nullable varchar] [MaxLength = -1]
UnicodeDataTypes.StringAnsi3 ---> [nullable varchar] [MaxLength = 3]
UnicodeDataTypes.StringAnsi9000 ---> [nullable varchar] [MaxLength = -1]
UnicodeDataTypes.StringDefault ---> [nullable nvarchar] [MaxLength = -1]
UnicodeDataTypes.StringUnicode ---> [nullable nvarchar] [MaxLength = -1]
User.Email ---> [nullable nvarchar] [MaxLength = -1]
User.Id ---> [uniqueidentifier]
User23059.Id ---> [int] [Precision = 10 Scale = 0]
User23059.IsSoftDeleted ---> [bit]
User23059.MessageGroups ---> [nullable nvarchar] [MaxLength = -1]

""";

        Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public override async Task Value_conversion_is_appropriately_used_for_join_condition()
    {
        await base.Value_conversion_is_appropriately_used_for_join_condition();

        AssertSql(
            """
@__blogId_0='1'

SELECT [b].[Url]
FROM [Blog] AS [b]
INNER JOIN [Post] AS [p] ON [b].[BlogId] = [p].[BlogId] AND [b].[IsVisible] = N'Y' AND [b].[BlogId] = @__blogId_0
WHERE [b].[IsVisible] = N'Y'
""");
    }

    [ConditionalFact]
    public override async Task Value_conversion_is_appropriately_used_for_left_join_condition()
    {
        await base.Value_conversion_is_appropriately_used_for_left_join_condition();

        AssertSql(
            """
@__blogId_0='1'

SELECT [b].[Url]
FROM [Blog] AS [b]
LEFT JOIN [Post] AS [p] ON [b].[BlogId] = [p].[BlogId] AND [b].[IsVisible] = N'Y' AND [b].[BlogId] = @__blogId_0
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

    public async override Task Where_bool_gets_converted_to_equality_when_value_conversion_is_used_using_EFProperty()
    {
        await base.Where_bool_gets_converted_to_equality_when_value_conversion_is_used_using_EFProperty();

        AssertSql(
            """
SELECT [b].[BlogId], [b].[Discriminator], [b].[IndexerVisible], [b].[IsVisible], [b].[Url], [b].[RssUrl]
FROM [Blog] AS [b]
WHERE [b].[IsVisible] = N'Y'
""");
    }

    public async override Task Where_bool_gets_converted_to_equality_when_value_conversion_is_used_using_indexer()
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

    public async override Task Id_object_as_entity_key()
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

    [ConditionalTheory(Skip = "Issue #30730: TODO need to find the default type mapping.")]
    [InlineData(true)]
    [InlineData(false)]
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
                .ConfigureWarnings(
                    c => c.Log(SqlServerEventId.DecimalTypeDefaultWarning));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<BuiltInDataTypes>().Property(e => e.TestBoolean).IsFixedLength();
        }
    }
}
