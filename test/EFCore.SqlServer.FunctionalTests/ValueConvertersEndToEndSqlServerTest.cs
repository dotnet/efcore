// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore;

public class ValueConvertersEndToEndSqlServerTest
    : ValueConvertersEndToEndTestBase<ValueConvertersEndToEndSqlServerTest.ValueConvertersEndToEndSqlServerFixture>
{
    public ValueConvertersEndToEndSqlServerTest(ValueConvertersEndToEndSqlServerFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [InlineData(nameof(ConvertingEntity.BoolAsChar), "nvarchar(1)", false, false)]
    [InlineData(nameof(ConvertingEntity.BoolAsNullableChar), "nvarchar(1)", false, false)]
    [InlineData(nameof(ConvertingEntity.BoolAsString), "nvarchar(3)", false, false)]
    [InlineData(nameof(ConvertingEntity.BoolAsInt), "int", false, false)]
    [InlineData(nameof(ConvertingEntity.BoolAsNullableString), "nvarchar(3)", false, false)]
    [InlineData(nameof(ConvertingEntity.BoolAsNullableInt), "int", false, false)]
    [InlineData(nameof(ConvertingEntity.IntAsLong), "bigint", false, false)]
    [InlineData(nameof(ConvertingEntity.IntAsNullableLong), "bigint", false, false)]
    [InlineData(nameof(ConvertingEntity.BytesAsString), "nvarchar(max)", false, false)]
    [InlineData(nameof(ConvertingEntity.BytesAsNullableString), "nvarchar(max)", false, false)]
    [InlineData(nameof(ConvertingEntity.CharAsString), "nvarchar(1)", false, false)]
    [InlineData(nameof(ConvertingEntity.CharAsNullableString), "nvarchar(1)", false, false)]
    [InlineData(nameof(ConvertingEntity.DateTimeOffsetToBinary), "bigint", false, false)]
    [InlineData(nameof(ConvertingEntity.DateTimeOffsetToNullableBinary), "bigint", false, false)]
    [InlineData(nameof(ConvertingEntity.DateTimeOffsetToString), "nvarchar(48)", false, false)]
    [InlineData(nameof(ConvertingEntity.DateTimeOffsetToNullableString), "nvarchar(48)", false, false)]
    [InlineData(nameof(ConvertingEntity.DateTimeToBinary), "bigint", false, false)]
    [InlineData(nameof(ConvertingEntity.DateTimeToNullableBinary), "bigint", false, false)]
    [InlineData(nameof(ConvertingEntity.DateTimeToString), "nvarchar(48)", false, false)]
    [InlineData(nameof(ConvertingEntity.DateTimeToNullableString), "nvarchar(48)", false, false)]
    [InlineData(nameof(ConvertingEntity.EnumToString), "nvarchar(max)", false, false)]
    [InlineData(nameof(ConvertingEntity.EnumToNullableString), "nvarchar(max)", false, false)]
    [InlineData(nameof(ConvertingEntity.EnumToNumber), "bigint", false, false)]
    [InlineData(nameof(ConvertingEntity.EnumToNullableNumber), "bigint", false, false)]
    [InlineData(nameof(ConvertingEntity.GuidToString), "nvarchar(36)", false, false)]
    [InlineData(nameof(ConvertingEntity.GuidToNullableString), "nvarchar(36)", false, false)]
    [InlineData(nameof(ConvertingEntity.GuidToBytes), "varbinary(16)", false, false)]
    [InlineData(nameof(ConvertingEntity.GuidToNullableBytes), "varbinary(16)", false, false)]
    [InlineData(nameof(ConvertingEntity.IPAddressToString), "nvarchar(45)", false, false)]
    [InlineData(nameof(ConvertingEntity.IPAddressToNullableString), "nvarchar(45)", false, false)]
    [InlineData(nameof(ConvertingEntity.IPAddressToBytes), "varbinary(16)", false, false)]
    [InlineData(nameof(ConvertingEntity.IPAddressToNullableBytes), "varbinary(16)", false, false)]
    [InlineData(nameof(ConvertingEntity.PhysicalAddressToString), "nvarchar(20)", false, false)]
    [InlineData(nameof(ConvertingEntity.PhysicalAddressToNullableString), "nvarchar(20)", false, false)]
    [InlineData(nameof(ConvertingEntity.PhysicalAddressToBytes), "varbinary(8)", false, false)]
    [InlineData(nameof(ConvertingEntity.PhysicalAddressToNullableBytes), "varbinary(8)", false, false)]
    [InlineData(nameof(ConvertingEntity.NumberToString), "nvarchar(64)", false, false)]
    [InlineData(nameof(ConvertingEntity.NumberToNullableString), "nvarchar(64)", false, false)]
    [InlineData(nameof(ConvertingEntity.NumberToBytes), "varbinary(1)", false, false)]
    [InlineData(nameof(ConvertingEntity.NumberToNullableBytes), "varbinary(1)", false, false)]
    [InlineData(nameof(ConvertingEntity.StringToBool), "bit", false, false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableBool), "bit", false, false)]
    [InlineData(nameof(ConvertingEntity.StringToBytes), "varbinary(max)", false, false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableBytes), "varbinary(max)", false, false)]
    [InlineData(nameof(ConvertingEntity.StringToChar), "nvarchar(1)", false, false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableChar), "nvarchar(1)", false, false)]
    [InlineData(nameof(ConvertingEntity.StringToDateTime), "datetime2", false, false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableDateTime), "datetime2", false, false)]
    [InlineData(nameof(ConvertingEntity.StringToDateTimeOffset), "datetimeoffset", false, false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableDateTimeOffset), "datetimeoffset", false, false)]
    [InlineData(nameof(ConvertingEntity.StringToEnum), "int", false, false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableEnum), "int", false, false)]
    [InlineData(nameof(ConvertingEntity.StringToGuid), "uniqueidentifier", false, false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableGuid), "uniqueidentifier", false, false)]
    [InlineData(nameof(ConvertingEntity.StringToNumber), "tinyint", false, false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableNumber), "tinyint", false, false)]
    [InlineData(nameof(ConvertingEntity.StringToTimeSpan), "time", false, false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableTimeSpan), "time", false, false)]
    [InlineData(nameof(ConvertingEntity.TimeSpanToTicks), "bigint", false, false)]
    [InlineData(nameof(ConvertingEntity.TimeSpanToNullableTicks), "bigint", false, false)]
    [InlineData(nameof(ConvertingEntity.TimeSpanToString), "nvarchar(48)", false, false)]
    [InlineData(nameof(ConvertingEntity.TimeSpanToNullableString), "nvarchar(48)", false, false)]
    [InlineData(nameof(ConvertingEntity.UriToString), "nvarchar(max)", false, false)]
    [InlineData(nameof(ConvertingEntity.UriToNullableString), "nvarchar(max)", false, false)]
    [InlineData(nameof(ConvertingEntity.NullableCharAsString), "nvarchar(1)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableCharAsNullableString), "nvarchar(1)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableBoolAsChar), "nvarchar(1)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableBoolAsNullableChar), "nvarchar(1)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableBoolAsString), "nvarchar(3)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableBoolAsNullableString), "nvarchar(3)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableBoolAsInt), "int", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableBoolAsNullableInt), "int", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableIntAsLong), "bigint", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableIntAsNullableLong), "bigint", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableBytesAsString), "nvarchar(max)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableBytesAsNullableString), "nvarchar(max)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeOffsetToBinary), "bigint", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeOffsetToNullableBinary), "bigint", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeOffsetToString), "nvarchar(48)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeOffsetToNullableString), "nvarchar(48)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeToBinary), "bigint", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeToNullableBinary), "bigint", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeToString), "nvarchar(48)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeToNullableString), "nvarchar(48)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableEnumToString), "nvarchar(max)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableEnumToNullableString), "nvarchar(max)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableEnumToNumber), "bigint", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableEnumToNullableNumber), "bigint", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableGuidToString), "nvarchar(36)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableGuidToNullableString), "nvarchar(36)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableGuidToBytes), "varbinary(16)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableGuidToNullableBytes), "varbinary(16)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableIPAddressToString), "nvarchar(45)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableIPAddressToNullableString), "nvarchar(45)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableIPAddressToBytes), "varbinary(16)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableIPAddressToNullableBytes), "varbinary(16)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullablePhysicalAddressToString), "nvarchar(20)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullablePhysicalAddressToNullableString), "nvarchar(20)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullablePhysicalAddressToBytes), "varbinary(8)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullablePhysicalAddressToNullableBytes), "varbinary(8)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableNumberToString), "nvarchar(64)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableNumberToNullableString), "nvarchar(64)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableNumberToBytes), "varbinary(1)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableNumberToNullableBytes), "varbinary(1)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToBool), "bit", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableBool), "bit", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToBytes), "varbinary(max)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableBytes), "varbinary(max)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToChar), "nvarchar(1)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableChar), "nvarchar(1)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToDateTime), "datetime2", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableDateTime), "datetime2", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToDateTimeOffset), "datetimeoffset", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableDateTimeOffset), "datetimeoffset", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToEnum), "int", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableEnum), "int", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToGuid), "uniqueidentifier", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableGuid), "uniqueidentifier", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNumber), "tinyint", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableNumber), "tinyint", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToTimeSpan), "time", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableTimeSpan), "time", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableTimeSpanToTicks), "bigint", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableTimeSpanToNullableTicks), "bigint", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableTimeSpanToString), "nvarchar(48)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableTimeSpanToNullableString), "nvarchar(48)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableUriToString), "nvarchar(max)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullableUriToNullableString), "nvarchar(max)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullStringToNonNullString), "nvarchar(max)", true, false)]
    [InlineData(nameof(ConvertingEntity.NonNullStringToNullString), "nvarchar(max)", false, true)]
    [InlineData(nameof(ConvertingEntity.NonNullIntToNullString), "nvarchar(max)", false, true)]
    [InlineData(nameof(ConvertingEntity.NonNullIntToNonNullString), "nvarchar(max)", false, false)]
    [InlineData(nameof(ConvertingEntity.NullIntToNullString), "nvarchar(max)", true, true)]
    [InlineData(nameof(ConvertingEntity.NullIntToNonNullString), "nvarchar(max)", true, false)]
    [InlineData(nameof(ConvertingEntity.NullableListOfInt), "nvarchar(max)", true, true)]
    [InlineData(nameof(ConvertingEntity.ListOfInt), "nvarchar(max)", false, false)]
    [InlineData(nameof(ConvertingEntity.NullableEnumerableOfInt), "nvarchar(max)", true, true)]
    [InlineData(nameof(ConvertingEntity.EnumerableOfInt), "nvarchar(max)", false, false)]
    public virtual void Properties_with_conversions_map_to_appropriately_null_columns(
        string propertyName,
        string databaseType,
        bool isNullable,
        bool isColumnNullable)
    {
        using var context = CreateContext();

        var property = context.Model.FindEntityType(typeof(ConvertingEntity))!.FindProperty(propertyName)!;

        Assert.Equal(databaseType, property.GetColumnType());
        Assert.Equal(isNullable, property.IsNullable);
        Assert.Equal(isColumnNullable, property.IsColumnNullable());
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
            @"SELECT [c].[Id], [c].[BoolAsChar], [c].[BoolAsInt], [c].[BoolAsNullableChar], [c].[BoolAsNullableInt], [c].[BoolAsNullableString], [c].[BoolAsString], [c].[BytesAsNullableString], [c].[BytesAsString], [c].[CharAsNullableString], [c].[CharAsString], [c].[DateTimeOffsetToBinary], [c].[DateTimeOffsetToNullableBinary], [c].[DateTimeOffsetToNullableString], [c].[DateTimeOffsetToString], [c].[DateTimeToBinary], [c].[DateTimeToNullableBinary], [c].[DateTimeToNullableString], [c].[DateTimeToString], [c].[EnumToNullableNumber], [c].[EnumToNullableString], [c].[EnumToNumber], [c].[EnumToString], [c].[EnumerableOfInt], [c].[GuidToBytes], [c].[GuidToNullableBytes], [c].[GuidToNullableString], [c].[GuidToString], [c].[IPAddressToBytes], [c].[IPAddressToNullableBytes], [c].[IPAddressToNullableString], [c].[IPAddressToString], [c].[IntAsLong], [c].[IntAsNullableLong], [c].[ListOfInt], [c].[NonNullIntToNonNullString], [c].[NonNullIntToNullString], [c].[NonNullStringToNullString], [c].[NullIntToNonNullString], [c].[NullIntToNullString], [c].[NullStringToNonNullString], [c].[NullableBoolAsChar], [c].[NullableBoolAsInt], [c].[NullableBoolAsNullableChar], [c].[NullableBoolAsNullableInt], [c].[NullableBoolAsNullableString], [c].[NullableBoolAsString], [c].[NullableBytesAsNullableString], [c].[NullableBytesAsString], [c].[NullableCharAsNullableString], [c].[NullableCharAsString], [c].[NullableDateTimeOffsetToBinary], [c].[NullableDateTimeOffsetToNullableBinary], [c].[NullableDateTimeOffsetToNullableString], [c].[NullableDateTimeOffsetToString], [c].[NullableDateTimeToBinary], [c].[NullableDateTimeToNullableBinary], [c].[NullableDateTimeToNullableString], [c].[NullableDateTimeToString], [c].[NullableEnumToNullableNumber], [c].[NullableEnumToNullableString], [c].[NullableEnumToNumber], [c].[NullableEnumToString], [c].[NullableEnumerableOfInt], [c].[NullableGuidToBytes], [c].[NullableGuidToNullableBytes], [c].[NullableGuidToNullableString], [c].[NullableGuidToString], [c].[NullableIPAddressToBytes], [c].[NullableIPAddressToNullableBytes], [c].[NullableIPAddressToNullableString], [c].[NullableIPAddressToString], [c].[NullableIntAsLong], [c].[NullableIntAsNullableLong], [c].[NullableListOfInt], [c].[NullableNumberToBytes], [c].[NullableNumberToNullableBytes], [c].[NullableNumberToNullableString], [c].[NullableNumberToString], [c].[NullablePhysicalAddressToBytes], [c].[NullablePhysicalAddressToNullableBytes], [c].[NullablePhysicalAddressToNullableString], [c].[NullablePhysicalAddressToString], [c].[NullableStringToBool], [c].[NullableStringToBytes], [c].[NullableStringToChar], [c].[NullableStringToDateTime], [c].[NullableStringToDateTimeOffset], [c].[NullableStringToEnum], [c].[NullableStringToGuid], [c].[NullableStringToNullableBool], [c].[NullableStringToNullableBytes], [c].[NullableStringToNullableChar], [c].[NullableStringToNullableDateTime], [c].[NullableStringToNullableDateTimeOffset], [c].[NullableStringToNullableEnum], [c].[NullableStringToNullableGuid], [c].[NullableStringToNullableNumber], [c].[NullableStringToNullableTimeSpan], [c].[NullableStringToNumber], [c].[NullableStringToTimeSpan], [c].[NullableTimeSpanToNullableString], [c].[NullableTimeSpanToNullableTicks], [c].[NullableTimeSpanToString], [c].[NullableTimeSpanToTicks], [c].[NullableUriToNullableString], [c].[NullableUriToString], [c].[NumberToBytes], [c].[NumberToNullableBytes], [c].[NumberToNullableString], [c].[NumberToString], [c].[PhysicalAddressToBytes], [c].[PhysicalAddressToNullableBytes], [c].[PhysicalAddressToNullableString], [c].[PhysicalAddressToString], [c].[StringToBool], [c].[StringToBytes], [c].[StringToChar], [c].[StringToDateTime], [c].[StringToDateTimeOffset], [c].[StringToEnum], [c].[StringToGuid], [c].[StringToNullableBool], [c].[StringToNullableBytes], [c].[StringToNullableChar], [c].[StringToNullableDateTime], [c].[StringToNullableDateTimeOffset], [c].[StringToNullableEnum], [c].[StringToNullableGuid], [c].[StringToNullableNumber], [c].[StringToNullableTimeSpan], [c].[StringToNumber], [c].[StringToTimeSpan], [c].[TimeSpanToNullableString], [c].[TimeSpanToNullableTicks], [c].[TimeSpanToString], [c].[TimeSpanToTicks], [c].[UriToNullableString], [c].[UriToString]
FROM [ConvertingEntity] AS [c]
WHERE CAST(DATALENGTH(CAST(N'' AS nvarchar(max))) AS int) = 1",
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
        }
    }
}

#nullable restore
