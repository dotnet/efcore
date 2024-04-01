// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore;

public class ValueConvertersEndToEndSqlServerTest(ValueConvertersEndToEndSqlServerTest.ValueConvertersEndToEndSqlServerFixture fixture)
    : ValueConvertersEndToEndTestBase<ValueConvertersEndToEndSqlServerTest.ValueConvertersEndToEndSqlServerFixture>(fixture)
{
    [ConditionalTheory]
    [InlineData(nameof(ConvertingEntity.BoolAsChar), "nvarchar(1)", false)]
    [InlineData(nameof(ConvertingEntity.BoolAsNullableChar), "nvarchar(1)", false)]
    [InlineData(nameof(ConvertingEntity.BoolAsString), "nvarchar(3)", false)]
    [InlineData(nameof(ConvertingEntity.BoolAsInt), "int", false)]
    [InlineData(nameof(ConvertingEntity.BoolAsNullableString), "nvarchar(3)", false)]
    [InlineData(nameof(ConvertingEntity.BoolAsNullableInt), "int", false)]
    [InlineData(nameof(ConvertingEntity.IntAsLong), "bigint", false)]
    [InlineData(nameof(ConvertingEntity.IntAsNullableLong), "bigint", false)]
    [InlineData(nameof(ConvertingEntity.BytesAsString), "nvarchar(max)", false)]
    [InlineData(nameof(ConvertingEntity.BytesAsNullableString), "nvarchar(max)", false)]
    [InlineData(nameof(ConvertingEntity.CharAsString), "nvarchar(1)", false)]
    [InlineData(nameof(ConvertingEntity.CharAsNullableString), "nvarchar(1)", false)]
    [InlineData(nameof(ConvertingEntity.DateTimeOffsetToBinary), "bigint", false)]
    [InlineData(nameof(ConvertingEntity.DateTimeOffsetToNullableBinary), "bigint", false)]
    [InlineData(nameof(ConvertingEntity.DateTimeOffsetToString), "nvarchar(48)", false)]
    [InlineData(nameof(ConvertingEntity.DateTimeOffsetToNullableString), "nvarchar(48)", false)]
    [InlineData(nameof(ConvertingEntity.DateTimeToBinary), "bigint", false)]
    [InlineData(nameof(ConvertingEntity.DateTimeToNullableBinary), "bigint", false)]
    [InlineData(nameof(ConvertingEntity.DateTimeToString), "nvarchar(48)", false)]
    [InlineData(nameof(ConvertingEntity.DateTimeToNullableString), "nvarchar(48)", false)]
    [InlineData(nameof(ConvertingEntity.EnumToString), "nvarchar(max)", false)]
    [InlineData(nameof(ConvertingEntity.EnumToNullableString), "nvarchar(max)", false)]
    [InlineData(nameof(ConvertingEntity.EnumToNumber), "bigint", false)]
    [InlineData(nameof(ConvertingEntity.EnumToNullableNumber), "bigint", false)]
    [InlineData(nameof(ConvertingEntity.GuidToString), "nvarchar(36)", false)]
    [InlineData(nameof(ConvertingEntity.GuidToNullableString), "nvarchar(36)", false)]
    [InlineData(nameof(ConvertingEntity.GuidToBytes), "varbinary(16)", false)]
    [InlineData(nameof(ConvertingEntity.GuidToNullableBytes), "varbinary(16)", false)]
    [InlineData(nameof(ConvertingEntity.IPAddressToString), "nvarchar(45)", false)]
    [InlineData(nameof(ConvertingEntity.IPAddressToNullableString), "nvarchar(45)", false)]
    [InlineData(nameof(ConvertingEntity.IPAddressToBytes), "varbinary(16)", false)]
    [InlineData(nameof(ConvertingEntity.IPAddressToNullableBytes), "varbinary(16)", false)]
    [InlineData(nameof(ConvertingEntity.PhysicalAddressToString), "nvarchar(20)", false)]
    [InlineData(nameof(ConvertingEntity.PhysicalAddressToNullableString), "nvarchar(20)", false)]
    [InlineData(nameof(ConvertingEntity.PhysicalAddressToBytes), "varbinary(8)", false)]
    [InlineData(nameof(ConvertingEntity.PhysicalAddressToNullableBytes), "varbinary(8)", false)]
    [InlineData(nameof(ConvertingEntity.NumberToString), "nvarchar(64)", false)]
    [InlineData(nameof(ConvertingEntity.NumberToNullableString), "nvarchar(64)", false)]
    [InlineData(nameof(ConvertingEntity.NumberToBytes), "varbinary(1)", false)]
    [InlineData(nameof(ConvertingEntity.NumberToNullableBytes), "varbinary(1)", false)]
    [InlineData(nameof(ConvertingEntity.StringToBool), "bit", false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableBool), "bit", false)]
    [InlineData(nameof(ConvertingEntity.StringToBytes), "varbinary(max)", false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableBytes), "varbinary(max)", false)]
    [InlineData(nameof(ConvertingEntity.StringToChar), "nvarchar(1)", false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableChar), "nvarchar(1)", false)]
    [InlineData(nameof(ConvertingEntity.StringToDateTime), "datetime2", false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableDateTime), "datetime2", false)]
    [InlineData(nameof(ConvertingEntity.StringToDateTimeOffset), "datetimeoffset", false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableDateTimeOffset), "datetimeoffset", false)]
    [InlineData(nameof(ConvertingEntity.StringToEnum), "int", false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableEnum), "int", false)]
    [InlineData(nameof(ConvertingEntity.StringToGuid), "uniqueidentifier", false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableGuid), "uniqueidentifier", false)]
    [InlineData(nameof(ConvertingEntity.StringToNumber), "tinyint", false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableNumber), "tinyint", false)]
    [InlineData(nameof(ConvertingEntity.StringToTimeSpan), "time", false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableTimeSpan), "time", false)]
    [InlineData(nameof(ConvertingEntity.TimeSpanToTicks), "bigint", false)]
    [InlineData(nameof(ConvertingEntity.TimeSpanToNullableTicks), "bigint", false)]
    [InlineData(nameof(ConvertingEntity.TimeSpanToString), "nvarchar(48)", false)]
    [InlineData(nameof(ConvertingEntity.TimeSpanToNullableString), "nvarchar(48)", false)]
    [InlineData(nameof(ConvertingEntity.UriToString), "nvarchar(max)", false)]
    [InlineData(nameof(ConvertingEntity.UriToNullableString), "nvarchar(max)", false)]
    [InlineData(nameof(ConvertingEntity.NullableCharAsString), "nvarchar(1)", true)]
    [InlineData(nameof(ConvertingEntity.NullableCharAsNullableString), "nvarchar(1)", true)]
    [InlineData(nameof(ConvertingEntity.NullableBoolAsChar), "nvarchar(1)", true)]
    [InlineData(nameof(ConvertingEntity.NullableBoolAsNullableChar), "nvarchar(1)", true)]
    [InlineData(nameof(ConvertingEntity.NullableBoolAsString), "nvarchar(3)", true)]
    [InlineData(nameof(ConvertingEntity.NullableBoolAsNullableString), "nvarchar(3)", true)]
    [InlineData(nameof(ConvertingEntity.NullableBoolAsInt), "int", true)]
    [InlineData(nameof(ConvertingEntity.NullableBoolAsNullableInt), "int", true)]
    [InlineData(nameof(ConvertingEntity.NullableIntAsLong), "bigint", true)]
    [InlineData(nameof(ConvertingEntity.NullableIntAsNullableLong), "bigint", true)]
    [InlineData(nameof(ConvertingEntity.NullableBytesAsString), "nvarchar(max)", true)]
    [InlineData(nameof(ConvertingEntity.NullableBytesAsNullableString), "nvarchar(max)", true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeOffsetToBinary), "bigint", true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeOffsetToNullableBinary), "bigint", true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeOffsetToString), "nvarchar(48)", true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeOffsetToNullableString), "nvarchar(48)", true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeToBinary), "bigint", true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeToNullableBinary), "bigint", true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeToString), "nvarchar(48)", true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeToNullableString), "nvarchar(48)", true)]
    [InlineData(nameof(ConvertingEntity.NullableEnumToString), "nvarchar(max)", true)]
    [InlineData(nameof(ConvertingEntity.NullableEnumToNullableString), "nvarchar(max)", true)]
    [InlineData(nameof(ConvertingEntity.NullableEnumToNumber), "bigint", true)]
    [InlineData(nameof(ConvertingEntity.NullableEnumToNullableNumber), "bigint", true)]
    [InlineData(nameof(ConvertingEntity.NullableGuidToString), "nvarchar(36)", true)]
    [InlineData(nameof(ConvertingEntity.NullableGuidToNullableString), "nvarchar(36)", true)]
    [InlineData(nameof(ConvertingEntity.NullableGuidToBytes), "varbinary(16)", true)]
    [InlineData(nameof(ConvertingEntity.NullableGuidToNullableBytes), "varbinary(16)", true)]
    [InlineData(nameof(ConvertingEntity.NullableIPAddressToString), "nvarchar(45)", true)]
    [InlineData(nameof(ConvertingEntity.NullableIPAddressToNullableString), "nvarchar(45)", true)]
    [InlineData(nameof(ConvertingEntity.NullableIPAddressToBytes), "varbinary(16)", true)]
    [InlineData(nameof(ConvertingEntity.NullableIPAddressToNullableBytes), "varbinary(16)", true)]
    [InlineData(nameof(ConvertingEntity.NullablePhysicalAddressToString), "nvarchar(20)", true)]
    [InlineData(nameof(ConvertingEntity.NullablePhysicalAddressToNullableString), "nvarchar(20)", true)]
    [InlineData(nameof(ConvertingEntity.NullablePhysicalAddressToBytes), "varbinary(8)", true)]
    [InlineData(nameof(ConvertingEntity.NullablePhysicalAddressToNullableBytes), "varbinary(8)", true)]
    [InlineData(nameof(ConvertingEntity.NullableNumberToString), "nvarchar(64)", true)]
    [InlineData(nameof(ConvertingEntity.NullableNumberToNullableString), "nvarchar(64)", true)]
    [InlineData(nameof(ConvertingEntity.NullableNumberToBytes), "varbinary(1)", true)]
    [InlineData(nameof(ConvertingEntity.NullableNumberToNullableBytes), "varbinary(1)", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToBool), "bit", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableBool), "bit", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToBytes), "varbinary(max)", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableBytes), "varbinary(max)", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToChar), "nvarchar(1)", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableChar), "nvarchar(1)", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToDateTime), "datetime2", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableDateTime), "datetime2", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToDateTimeOffset), "datetimeoffset", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableDateTimeOffset), "datetimeoffset", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToEnum), "int", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableEnum), "int", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToGuid), "uniqueidentifier", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableGuid), "uniqueidentifier", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNumber), "tinyint", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableNumber), "tinyint", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToTimeSpan), "time", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableTimeSpan), "time", true)]
    [InlineData(nameof(ConvertingEntity.NullableTimeSpanToTicks), "bigint", true)]
    [InlineData(nameof(ConvertingEntity.NullableTimeSpanToNullableTicks), "bigint", true)]
    [InlineData(nameof(ConvertingEntity.NullableTimeSpanToString), "nvarchar(48)", true)]
    [InlineData(nameof(ConvertingEntity.NullableTimeSpanToNullableString), "nvarchar(48)", true)]
    [InlineData(nameof(ConvertingEntity.NullableUriToString), "nvarchar(max)", true)]
    [InlineData(nameof(ConvertingEntity.NullableUriToNullableString), "nvarchar(max)", true)]
    [InlineData(nameof(ConvertingEntity.NullStringToNonNullString), "nvarchar(max)", false)]
    [InlineData(nameof(ConvertingEntity.NonNullStringToNullString), "nvarchar(max)", true)]
    [InlineData(nameof(ConvertingEntity.NullableListOfInt), "nvarchar(max)", true)]
    [InlineData(nameof(ConvertingEntity.ListOfInt), "nvarchar(max)", false)]
    public virtual void Properties_with_conversions_map_to_appropriately_null_columns(
        string propertyName,
        string databaseType,
        bool isNullable)
    {
        using var context = CreateContext();

        var property = context.Model.FindEntityType(typeof(ConvertingEntity))!.FindProperty(propertyName);

        Assert.Equal(databaseType, property!.GetColumnType());
        Assert.Equal(isNullable, property!.IsNullable);
    }

    [ConditionalFact]
    public virtual void Can_use_custom_converters_without_property()
    {
        Fixture.TestSqlLoggerFactory.Clear();

        using (var context = CreateContext())
        {
            Assert.Empty(
                context.Set<ConvertingEntity>()
                    .Where(e => EF.Functions.DataLength((string)(object)new WrappedString { Value = "" }) == 1).ToList());
        }

        Assert.Equal(
            """
SELECT [c].[Id], [c].[BoolAsChar], [c].[BoolAsInt], [c].[BoolAsNullableChar], [c].[BoolAsNullableInt], [c].[BoolAsNullableString], [c].[BoolAsString], [c].[BytesAsNullableString], [c].[BytesAsString], [c].[CharAsNullableString], [c].[CharAsString], [c].[DateOnlyToNullableString], [c].[DateOnlyToString], [c].[DateTimeOffsetToBinary], [c].[DateTimeOffsetToNullableBinary], [c].[DateTimeOffsetToNullableString], [c].[DateTimeOffsetToString], [c].[DateTimeToBinary], [c].[DateTimeToNullableBinary], [c].[DateTimeToNullableString], [c].[DateTimeToString], [c].[EnumToNullableNumber], [c].[EnumToNullableString], [c].[EnumToNumber], [c].[EnumToString], [c].[EnumerableOfInt], [c].[GuidToBytes], [c].[GuidToNullableBytes], [c].[GuidToNullableString], [c].[GuidToString], [c].[IPAddressToBytes], [c].[IPAddressToNullableBytes], [c].[IPAddressToNullableString], [c].[IPAddressToString], [c].[IntAsLong], [c].[IntAsNullableLong], [c].[ListOfInt], [c].[NonNullIntToNonNullString], [c].[NonNullIntToNullString], [c].[NonNullStringToNullString], [c].[NullIntToNonNullString], [c].[NullIntToNullString], [c].[NullStringToNonNullString], [c].[NullableBoolAsChar], [c].[NullableBoolAsInt], [c].[NullableBoolAsNullableChar], [c].[NullableBoolAsNullableInt], [c].[NullableBoolAsNullableString], [c].[NullableBoolAsString], [c].[NullableBytesAsNullableString], [c].[NullableBytesAsString], [c].[NullableCharAsNullableString], [c].[NullableCharAsString], [c].[NullableDateOnlyToNullableString], [c].[NullableDateOnlyToString], [c].[NullableDateTimeOffsetToBinary], [c].[NullableDateTimeOffsetToNullableBinary], [c].[NullableDateTimeOffsetToNullableString], [c].[NullableDateTimeOffsetToString], [c].[NullableDateTimeToBinary], [c].[NullableDateTimeToNullableBinary], [c].[NullableDateTimeToNullableString], [c].[NullableDateTimeToString], [c].[NullableEnumToNullableNumber], [c].[NullableEnumToNullableString], [c].[NullableEnumToNumber], [c].[NullableEnumToString], [c].[NullableEnumerableOfInt], [c].[NullableGuidToBytes], [c].[NullableGuidToNullableBytes], [c].[NullableGuidToNullableString], [c].[NullableGuidToString], [c].[NullableIPAddressToBytes], [c].[NullableIPAddressToNullableBytes], [c].[NullableIPAddressToNullableString], [c].[NullableIPAddressToString], [c].[NullableIntAsLong], [c].[NullableIntAsNullableLong], [c].[NullableListOfInt], [c].[NullableNumberToBytes], [c].[NullableNumberToNullableBytes], [c].[NullableNumberToNullableString], [c].[NullableNumberToString], [c].[NullablePhysicalAddressToBytes], [c].[NullablePhysicalAddressToNullableBytes], [c].[NullablePhysicalAddressToNullableString], [c].[NullablePhysicalAddressToString], [c].[NullableStringToBool], [c].[NullableStringToBytes], [c].[NullableStringToChar], [c].[NullableStringToDateTime], [c].[NullableStringToDateTimeOffset], [c].[NullableStringToEnum], [c].[NullableStringToGuid], [c].[NullableStringToNullableBool], [c].[NullableStringToNullableBytes], [c].[NullableStringToNullableChar], [c].[NullableStringToNullableDateTime], [c].[NullableStringToNullableDateTimeOffset], [c].[NullableStringToNullableEnum], [c].[NullableStringToNullableGuid], [c].[NullableStringToNullableNumber], [c].[NullableStringToNullableTimeSpan], [c].[NullableStringToNumber], [c].[NullableStringToTimeSpan], [c].[NullableTimeSpanToNullableString], [c].[NullableTimeSpanToNullableTicks], [c].[NullableTimeSpanToString], [c].[NullableTimeSpanToTicks], [c].[NullableUriToNullableString], [c].[NullableUriToString], [c].[NumberToBytes], [c].[NumberToNullableBytes], [c].[NumberToNullableString], [c].[NumberToString], [c].[PhysicalAddressToBytes], [c].[PhysicalAddressToNullableBytes], [c].[PhysicalAddressToNullableString], [c].[PhysicalAddressToString], [c].[StringToBool], [c].[StringToBytes], [c].[StringToChar], [c].[StringToDateTime], [c].[StringToDateTimeOffset], [c].[StringToEnum], [c].[StringToGuid], [c].[StringToNullableBool], [c].[StringToNullableBytes], [c].[StringToNullableChar], [c].[StringToNullableDateTime], [c].[StringToNullableDateTimeOffset], [c].[StringToNullableEnum], [c].[StringToNullableGuid], [c].[StringToNullableNumber], [c].[StringToNullableTimeSpan], [c].[StringToNumber], [c].[StringToTimeSpan], [c].[TimeSpanToNullableString], [c].[TimeSpanToNullableTicks], [c].[TimeSpanToString], [c].[TimeSpanToTicks], [c].[UriToNullableString], [c].[UriToString]
FROM [ConvertingEntity] AS [c]
WHERE CAST(DATALENGTH(CAST(N'' AS nvarchar(max))) AS int) = 1
""",
            Fixture.TestSqlLoggerFactory.SqlStatements[0],
            ignoreLineEndingDifferences: true);
    }

    private struct WrappedString
    {
        public string Value { get; init; }
    }

    private class WrappedStringToStringConverter : ValueConverter<WrappedString, string>
    {
        public WrappedStringToStringConverter()
            : base(v => v.Value, v => new WrappedString { Value = v })
        {
        }
    }

    [ConditionalFact]
    public virtual void Fixed_length_hints_are_respected()
    {
        Fixture.TestSqlLoggerFactory.Clear();

        using var context = CreateContext();

        var guid = new Guid("d854227f-7076-48c3-997c-4e72c1c713b9");

        var mapping = context.Set<SqlServerConvertingEntity>()
            .EntityType
            .FindProperty(nameof(SqlServerConvertingEntity.GuidToFixedLengthString))!
            .FindRelationalTypeMapping()!;

        Assert.Equal("nchar(40)", mapping.StoreType);
        Assert.Equal(40, mapping.Size);

        Assert.Empty(context.Set<SqlServerConvertingEntity>().Where(e => e.GuidToFixedLengthString != guid));

        Assert.Equal(
            """
@__guid_0='d854227f-7076-48c3-997c-4e72c1c713b9' (Nullable = false) (Size = 40)

SELECT [s].[Id], [s].[GuidToDbTypeString], [s].[GuidToFixedLengthString]
FROM [SqlServerConvertingEntity] AS [s]
WHERE [s].[GuidToFixedLengthString] <> @__guid_0
""",
            Fixture.TestSqlLoggerFactory.SqlStatements[0],
            ignoreLineEndingDifferences: true);

        var parameter = Fixture.TestSqlLoggerFactory.Parameters.Single();
    }

    [ConditionalFact]
    public virtual void DbType_hints_are_respected()
    {
        Fixture.TestSqlLoggerFactory.Clear();

        using var context = CreateContext();

        var mapping = context.Set<SqlServerConvertingEntity>()
            .EntityType
            .FindProperty(nameof(SqlServerConvertingEntity.GuidToDbTypeString))!
            .FindRelationalTypeMapping()!;

        Assert.Equal(DbType.AnsiStringFixedLength, mapping.DbType!);
        Assert.Equal(40, mapping.Size);

        var guid = new Guid("d854227f-7076-48c3-997c-4e72c1c713b9");

        Assert.Empty(context.Set<SqlServerConvertingEntity>().Where(e => e.GuidToDbTypeString != guid));

        Assert.Equal(
            """
@__guid_0='d854227f-7076-48c3-997c-4e72c1c713b9' (Nullable = false) (Size = 40) (DbType = AnsiStringFixedLength)

SELECT [s].[Id], [s].[GuidToDbTypeString], [s].[GuidToFixedLengthString]
FROM [SqlServerConvertingEntity] AS [s]
WHERE [s].[GuidToDbTypeString] <> @__guid_0
""",
            Fixture.TestSqlLoggerFactory.SqlStatements[0],
            ignoreLineEndingDifferences: true);
    }

    protected class SqlServerConvertingEntity
    {
        public Guid Id { get; set; }

        public Guid GuidToFixedLengthString { get; set; }
        public Guid GuidToDbTypeString { get; set; }
    }

    public class ValueConvertersEndToEndSqlServerFixture : ValueConvertersEndToEndFixtureBase
    {
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);

            configurationBuilder.DefaultTypeMapping<WrappedString>().HasConversion<WrappedStringToStringConverter>();
        }

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<ConvertingEntity>(
                b =>
                {
                    b.Property(e => e.NullableListOfInt).HasDefaultValue(new List<int>());
                    b.Property(e => e.ListOfInt).HasDefaultValue(new List<int>());
                    b.Property(e => e.NullableEnumerableOfInt).HasDefaultValue(Enumerable.Empty<int>());
                    b.Property(e => e.EnumerableOfInt).HasDefaultValue(Enumerable.Empty<int>());
                });

            modelBuilder.Entity<SqlServerConvertingEntity>(
                b =>
                {
                    b.Property(e => e.GuidToFixedLengthString).HasConversion(
                        new GuidToStringConverter(
                            new RelationalConverterMappingHints(
                                size: 40, fixedLength: true)));

                    b.Property(e => e.GuidToDbTypeString).HasConversion(
                        new GuidToStringConverter(
                            new RelationalConverterMappingHints(
                                size: 40, unicode: false, dbType: DbType.AnsiStringFixedLength)));
                });
        }
    }
}
