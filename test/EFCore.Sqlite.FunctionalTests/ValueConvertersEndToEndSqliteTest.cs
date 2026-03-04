// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class ValueConvertersEndToEndSqliteTest(ValueConvertersEndToEndSqliteTest.ValueConvertersEndToEndSqliteFixture fixture)
    : ValueConvertersEndToEndTestBase<ValueConvertersEndToEndSqliteTest.ValueConvertersEndToEndSqliteFixture>(fixture)
{
    [ConditionalTheory]
    [InlineData(nameof(ConvertingEntity.BoolAsChar), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.BoolAsNullableChar), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.BoolAsString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.BoolAsInt), "INTEGER", false)]
    [InlineData(nameof(ConvertingEntity.BoolAsNullableString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.BoolAsNullableInt), "INTEGER", false)]
    [InlineData(nameof(ConvertingEntity.IntAsLong), "INTEGER", false)]
    [InlineData(nameof(ConvertingEntity.IntAsNullableLong), "INTEGER", false)]
    [InlineData(nameof(ConvertingEntity.BytesAsString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.BytesAsNullableString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.CharAsString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.CharAsNullableString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.DateTimeOffsetToBinary), "INTEGER", false)]
    [InlineData(nameof(ConvertingEntity.DateTimeOffsetToNullableBinary), "INTEGER", false)]
    [InlineData(nameof(ConvertingEntity.DateTimeOffsetToString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.DateTimeOffsetToNullableString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.DateTimeToBinary), "INTEGER", false)]
    [InlineData(nameof(ConvertingEntity.DateTimeToNullableBinary), "INTEGER", false)]
    [InlineData(nameof(ConvertingEntity.DateTimeToString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.DateTimeToNullableString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.EnumToString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.EnumToNullableString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.EnumToNumber), "INTEGER", false)]
    [InlineData(nameof(ConvertingEntity.EnumToNullableNumber), "INTEGER", false)]
    [InlineData(nameof(ConvertingEntity.GuidToString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.GuidToNullableString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.GuidToBytes), "BLOB", false)]
    [InlineData(nameof(ConvertingEntity.GuidToNullableBytes), "BLOB", false)]
    [InlineData(nameof(ConvertingEntity.IPAddressToString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.IPAddressToNullableString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.IPAddressToBytes), "BLOB", false)]
    [InlineData(nameof(ConvertingEntity.IPAddressToNullableBytes), "BLOB", false)]
    [InlineData(nameof(ConvertingEntity.PhysicalAddressToString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.PhysicalAddressToNullableString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.PhysicalAddressToBytes), "BLOB", false)]
    [InlineData(nameof(ConvertingEntity.PhysicalAddressToNullableBytes), "BLOB", false)]
    [InlineData(nameof(ConvertingEntity.NumberToString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.NumberToNullableString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.NumberToBytes), "BLOB", false)]
    [InlineData(nameof(ConvertingEntity.NumberToNullableBytes), "BLOB", false)]
    [InlineData(nameof(ConvertingEntity.StringToBool), "INTEGER", false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableBool), "INTEGER", false)]
    [InlineData(nameof(ConvertingEntity.StringToBytes), "BLOB", false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableBytes), "BLOB", false)]
    [InlineData(nameof(ConvertingEntity.StringToChar), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableChar), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.StringToDateTime), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableDateTime), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.StringToDateTimeOffset), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableDateTimeOffset), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.StringToEnum), "INTEGER", false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableEnum), "INTEGER", false)]
    [InlineData(nameof(ConvertingEntity.StringToGuid), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableGuid), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.StringToNumber), "INTEGER", false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableNumber), "INTEGER", false)]
    [InlineData(nameof(ConvertingEntity.StringToTimeSpan), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.StringToNullableTimeSpan), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.TimeSpanToTicks), "INTEGER", false)]
    [InlineData(nameof(ConvertingEntity.TimeSpanToNullableTicks), "INTEGER", false)]
    [InlineData(nameof(ConvertingEntity.TimeSpanToString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.TimeSpanToNullableString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.UriToString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.UriToNullableString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.NullableCharAsString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableCharAsNullableString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableBoolAsChar), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableBoolAsNullableChar), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableBoolAsString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableBoolAsNullableString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableBoolAsInt), "INTEGER", true)]
    [InlineData(nameof(ConvertingEntity.NullableBoolAsNullableInt), "INTEGER", true)]
    [InlineData(nameof(ConvertingEntity.NullableIntAsLong), "INTEGER", true)]
    [InlineData(nameof(ConvertingEntity.NullableIntAsNullableLong), "INTEGER", true)]
    [InlineData(nameof(ConvertingEntity.NullableBytesAsString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableBytesAsNullableString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeOffsetToBinary), "INTEGER", true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeOffsetToNullableBinary), "INTEGER", true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeOffsetToString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeOffsetToNullableString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeToBinary), "INTEGER", true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeToNullableBinary), "INTEGER", true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeToString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableDateTimeToNullableString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableEnumToString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableEnumToNullableString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableEnumToNumber), "INTEGER", true)]
    [InlineData(nameof(ConvertingEntity.NullableEnumToNullableNumber), "INTEGER", true)]
    [InlineData(nameof(ConvertingEntity.NullableGuidToString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableGuidToNullableString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableGuidToBytes), "BLOB", true)]
    [InlineData(nameof(ConvertingEntity.NullableGuidToNullableBytes), "BLOB", true)]
    [InlineData(nameof(ConvertingEntity.NullableIPAddressToString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableIPAddressToNullableString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableIPAddressToBytes), "BLOB", true)]
    [InlineData(nameof(ConvertingEntity.NullableIPAddressToNullableBytes), "BLOB", true)]
    [InlineData(nameof(ConvertingEntity.NullablePhysicalAddressToString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullablePhysicalAddressToNullableString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullablePhysicalAddressToBytes), "BLOB", true)]
    [InlineData(nameof(ConvertingEntity.NullablePhysicalAddressToNullableBytes), "BLOB", true)]
    [InlineData(nameof(ConvertingEntity.NullableNumberToString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableNumberToNullableString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableNumberToBytes), "BLOB", true)]
    [InlineData(nameof(ConvertingEntity.NullableNumberToNullableBytes), "BLOB", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToBool), "INTEGER", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableBool), "INTEGER", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToBytes), "BLOB", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableBytes), "BLOB", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToChar), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableChar), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToDateTime), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableDateTime), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToDateTimeOffset), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableDateTimeOffset), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToEnum), "INTEGER", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableEnum), "INTEGER", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToGuid), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableGuid), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNumber), "INTEGER", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableNumber), "INTEGER", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToTimeSpan), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableStringToNullableTimeSpan), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableTimeSpanToTicks), "INTEGER", true)]
    [InlineData(nameof(ConvertingEntity.NullableTimeSpanToNullableTicks), "INTEGER", true)]
    [InlineData(nameof(ConvertingEntity.NullableTimeSpanToString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableTimeSpanToNullableString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableUriToString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullableUriToNullableString), "TEXT", true)]
    [InlineData(nameof(ConvertingEntity.NullStringToNonNullString), "TEXT", false)]
    [InlineData(nameof(ConvertingEntity.NonNullStringToNullString), "TEXT", true)]
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

    public class ValueConvertersEndToEndSqliteFixture : ValueConvertersEndToEndFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

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
