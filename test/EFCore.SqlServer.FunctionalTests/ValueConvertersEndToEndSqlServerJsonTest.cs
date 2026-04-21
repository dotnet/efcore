// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class ValueConvertersEndToEndSqlServerJsonTest(ValueConvertersEndToEndSqlServerJsonTest.ValueConvertersEndToEndSqlServerJsonFixture fixture)
    : ValueConvertersEndToEndTestBase<ValueConvertersEndToEndSqlServerJsonTest.ValueConvertersEndToEndSqlServerJsonFixture>(fixture)
{
    protected override void Add(DbContext context, ConvertingEntity entity)
    {
        var root = new RootEntity
        {
            Id = Guid.NewGuid(),
            ConvertingEntity = entity
        };
        entity.Id = root.Id;
        context.Add(root);
    }

    protected override async Task<ConvertingEntity> GetAsync(DbContext context, Guid id)
        => (await context.Set<RootEntity>()
            .Where(e => e.Id == id)
            .SingleAsync()).ConvertingEntity;

    protected override PropertyEntry Property(DbContext context, ConvertingEntity entity, IProperty property)
        => context.ChangeTracker.Entries<RootEntity>().Single(x => x.Entity.ConvertingEntity == entity).Property(property);

    protected override ITypeBase FindType(DbContext context)
        => context.Model.GetEntityTypes().Single(x => x.ClrType == typeof(RootEntity)).GetComplexProperties().Single().ComplexType;

    public class ValueConvertersEndToEndSqlServerJsonFixture : ValueConvertersEndToEndFixtureBase
    {
        protected override string StoreName => nameof(ValueConvertersEndToEndSqlServerJsonFixture);

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<RootEntity>().ComplexProperty(
                e => e.ConvertingEntity, b =>
                {
                    b.ToJson();
                    b.Ignore(x => x.Id);
                    b.Property(e => e.BoolAsChar).HasConversion(new BoolToTwoValuesConverter<char>('O', 'N'));
                    b.Property(e => e.BoolAsNullableChar).HasConversion(new BoolToTwoValuesConverter<char?>('O', 'N'));
                    b.Property(e => e.NullableBoolAsChar).HasConversion(new BoolToTwoValuesConverter<char>('O', 'N'));
                    b.Property(e => e.NullableBoolAsNullableChar).HasConversion(new BoolToTwoValuesConverter<char?>('O', 'N'));

                    b.Property(e => e.BoolAsString).HasConversion(new BoolToStringConverter("Non", "Oui"));
                    b.Property(e => e.BoolAsNullableString).HasConversion(
                        new BoolToTwoValuesConverter<string?>("Non", "Oui", mappingHints: new ConverterMappingHints(size: 3)));
                    b.Property(e => e.NullableBoolAsString).HasConversion(new BoolToStringConverter("Non", "Oui"));
                    b.Property(e => e.NullableBoolAsNullableString).HasConversion(
                        new BoolToTwoValuesConverter<string?>("Non", "Oui", mappingHints: new ConverterMappingHints(size: 3)));

                    b.Property(e => e.BoolAsInt).HasConversion(new BoolToZeroOneConverter<int>());
                    b.Property(e => e.BoolAsNullableInt).HasConversion(new BoolToZeroOneConverter<int?>());
                    b.Property(e => e.NullableBoolAsInt).HasConversion(new BoolToZeroOneConverter<int>());
                    b.Property(e => e.NullableBoolAsNullableInt).HasConversion(new BoolToZeroOneConverter<int?>());

                    b.Property(e => e.IntAsLong).HasConversion(new CastingConverter<int, long>());
                    b.Property(e => e.IntAsNullableLong).HasConversion(new CastingConverter<int, long?>());
                    b.Property(e => e.NullableIntAsLong).HasConversion(new CastingConverter<int?, long>());
                    b.Property(e => e.NullableIntAsNullableLong).HasConversion(new CastingConverter<int?, long?>());

                    b.Property(e => e.BytesAsString).HasConversion(
                        (ValueConverter)new BytesToStringConverter(), new ArrayStructuralComparer<byte>());
                    b.Property(e => e.BytesAsNullableString).HasConversion(
                        (ValueConverter)new BytesToStringConverter(), new ArrayStructuralComparer<byte>());
                    b.Property(e => e.NullableBytesAsString).HasConversion(
                        new BytesToStringConverter(), new ArrayStructuralComparer<byte>());
                    b.Property(e => e.NullableBytesAsNullableString).HasConversion(
                        new BytesToStringConverter(), new ArrayStructuralComparer<byte>());

                    b.Property(e => e.CharAsString).HasConversion(new CharToStringConverter());
                    b.Property(e => e.NullableCharAsString).HasConversion(new CharToStringConverter());
                    b.Property(e => e.CharAsNullableString).HasConversion(new CharToStringConverter());
                    b.Property(e => e.NullableCharAsNullableString).HasConversion(new CharToStringConverter());

                    b.Property(e => e.DateTimeOffsetToBinary).HasConversion(new DateTimeOffsetToBinaryConverter());
                    b.Property(e => e.DateTimeOffsetToNullableBinary).HasConversion(new DateTimeOffsetToBinaryConverter());
                    b.Property(e => e.NullableDateTimeOffsetToBinary).HasConversion(new DateTimeOffsetToBinaryConverter());
                    b.Property(e => e.NullableDateTimeOffsetToNullableBinary).HasConversion(new DateTimeOffsetToBinaryConverter());

                    b.Property(e => e.DateTimeOffsetToString).HasConversion(new DateTimeOffsetToStringConverter());
                    b.Property(e => e.DateTimeOffsetToNullableString).HasConversion(new DateTimeOffsetToStringConverter());
                    b.Property(e => e.NullableDateTimeOffsetToString).HasConversion(new DateTimeOffsetToStringConverter());
                    b.Property(e => e.NullableDateTimeOffsetToNullableString).HasConversion(new DateTimeOffsetToStringConverter());

                    b.Property(e => e.DateTimeToBinary).HasConversion(new DateTimeToBinaryConverter());
                    b.Property(e => e.DateTimeToNullableBinary).HasConversion(new DateTimeToBinaryConverter());
                    b.Property(e => e.NullableDateTimeToBinary).HasConversion(new DateTimeToBinaryConverter());
                    b.Property(e => e.NullableDateTimeToNullableBinary).HasConversion(new DateTimeToBinaryConverter());

                    b.Property(e => e.DateTimeToString).HasConversion(new DateTimeToStringConverter());
                    b.Property(e => e.DateTimeToNullableString).HasConversion(new DateTimeToStringConverter());
                    b.Property(e => e.NullableDateTimeToString).HasConversion(new DateTimeToStringConverter());
                    b.Property(e => e.NullableDateTimeToNullableString).HasConversion(new DateTimeToStringConverter());

                    b.Property(e => e.DateOnlyToString).HasConversion(new DateOnlyToStringConverter());
                    b.Property(e => e.DateOnlyToNullableString).HasConversion(new DateOnlyToStringConverter());
                    b.Property(e => e.NullableDateOnlyToString).HasConversion(new DateOnlyToStringConverter());
                    b.Property(e => e.NullableDateOnlyToNullableString).HasConversion(new DateOnlyToStringConverter());

                    b.Property(e => e.EnumToString).HasConversion(new EnumToStringConverter<TheExperience>());
                    b.Property(e => e.EnumToNullableString).HasConversion(new EnumToStringConverter<TheExperience>());
                    b.Property(e => e.NullableEnumToString).HasConversion(new EnumToStringConverter<TheExperience>());
                    b.Property(e => e.NullableEnumToNullableString).HasConversion(new EnumToStringConverter<TheExperience>());

                    b.Property(e => e.EnumToNumber).HasConversion(new EnumToNumberConverter<TheExperience, long>());
                    b.Property(e => e.EnumToNullableNumber).HasConversion(new EnumToNumberConverter<TheExperience, long>());
                    b.Property(e => e.NullableEnumToNumber).HasConversion(new EnumToNumberConverter<TheExperience, long>());
                    b.Property(e => e.NullableEnumToNullableNumber).HasConversion(new EnumToNumberConverter<TheExperience, long>());

                    b.Property(e => e.GuidToString).HasConversion(new GuidToStringConverter());
                    b.Property(e => e.GuidToNullableString).HasConversion(new GuidToStringConverter());
                    b.Property(e => e.NullableGuidToString).HasConversion(new GuidToStringConverter());
                    b.Property(e => e.NullableGuidToNullableString).HasConversion(new GuidToStringConverter());

                    b.Property(e => e.GuidToBytes).HasConversion(new GuidToBytesConverter());
                    b.Property(e => e.GuidToNullableBytes).HasConversion(new GuidToBytesConverter());
                    b.Property(e => e.NullableGuidToBytes).HasConversion(new GuidToBytesConverter());
                    b.Property(e => e.NullableGuidToNullableBytes).HasConversion(new GuidToBytesConverter());

                    b.Property(e => e.IPAddressToString).HasConversion((ValueConverter)new IPAddressToStringConverter());
                    b.Property(e => e.IPAddressToNullableString).HasConversion((ValueConverter)new IPAddressToStringConverter());
                    b.Property(e => e.NullableIPAddressToString).HasConversion(new IPAddressToStringConverter());
                    b.Property(e => e.NullableIPAddressToNullableString).HasConversion(new IPAddressToStringConverter());

                    b.Property(e => e.IPAddressToBytes).HasConversion((ValueConverter)new IPAddressToBytesConverter());
                    b.Property(e => e.IPAddressToNullableBytes).HasConversion((ValueConverter)new IPAddressToBytesConverter());
                    b.Property(e => e.NullableIPAddressToBytes).HasConversion(new IPAddressToBytesConverter());
                    b.Property(e => e.NullableIPAddressToNullableBytes).HasConversion(new IPAddressToBytesConverter());

                    b.Property(e => e.PhysicalAddressToString).HasConversion((ValueConverter)new PhysicalAddressToStringConverter());
                    b.Property(e => e.PhysicalAddressToNullableString)
                        .HasConversion((ValueConverter)new PhysicalAddressToStringConverter());
                    b.Property(e => e.NullablePhysicalAddressToString).HasConversion(new PhysicalAddressToStringConverter());
                    b.Property(e => e.NullablePhysicalAddressToNullableString).HasConversion(new PhysicalAddressToStringConverter());

                    b.Property(e => e.PhysicalAddressToBytes).HasConversion((ValueConverter)new PhysicalAddressToBytesConverter());
                    b.Property(e => e.PhysicalAddressToNullableBytes)
                        .HasConversion((ValueConverter)new PhysicalAddressToBytesConverter());
                    b.Property(e => e.NullablePhysicalAddressToBytes).HasConversion(new PhysicalAddressToBytesConverter());
                    b.Property(e => e.NullablePhysicalAddressToNullableBytes).HasConversion(new PhysicalAddressToBytesConverter());

                    b.Property(e => e.NumberToString).HasConversion(new NumberToStringConverter<ulong>());
                    b.Property(e => e.NumberToNullableString).HasConversion(new NumberToStringConverter<ulong>());
                    b.Property(e => e.NullableNumberToString).HasConversion(new NumberToStringConverter<ulong>());
                    b.Property(e => e.NullableNumberToNullableString).HasConversion(new NumberToStringConverter<ulong>());

                    b.Property(e => e.NumberToBytes).HasConversion(new NumberToBytesConverter<sbyte>());
                    b.Property(e => e.NumberToNullableBytes).HasConversion(new NumberToBytesConverter<sbyte>());
                    b.Property(e => e.NullableNumberToBytes).HasConversion(new NumberToBytesConverter<sbyte>());
                    b.Property(e => e.NullableNumberToNullableBytes).HasConversion(new NumberToBytesConverter<sbyte>());

                    b.Property(e => e.StringToBool).HasConversion(new StringToBoolConverter());
                    b.Property(e => e.StringToNullableBool).HasConversion(new StringToBoolConverter());
                    b.Property(e => e.NullableStringToBool).HasConversion((ValueConverter)new StringToBoolConverter());
                    b.Property(e => e.NullableStringToNullableBool).HasConversion((ValueConverter)new StringToBoolConverter());

                    b.Property(e => e.StringToBytes).HasConversion((ValueConverter)new StringToBytesConverter(Encoding.UTF32));
                    b.Property(e => e.StringToNullableBytes).HasConversion((ValueConverter)new StringToBytesConverter(Encoding.UTF32));
                    b.Property(e => e.NullableStringToBytes).HasConversion(new StringToBytesConverter(Encoding.UTF32));
                    b.Property(e => e.NullableStringToNullableBytes).HasConversion(new StringToBytesConverter(Encoding.UTF32));

                    b.Property(e => e.StringToChar).HasConversion(new StringToCharConverter());
                    b.Property(e => e.StringToNullableChar).HasConversion(new StringToCharConverter());
                    b.Property(e => e.NullableStringToChar).HasConversion((ValueConverter)new StringToCharConverter());
                    b.Property(e => e.NullableStringToNullableChar).HasConversion((ValueConverter)new StringToCharConverter());

                    b.Property(e => e.StringToDateTime).HasConversion(new StringToDateTimeConverter());
                    b.Property(e => e.StringToNullableDateTime).HasConversion(new StringToDateTimeConverter());
                    b.Property(e => e.NullableStringToDateTime).HasConversion((ValueConverter)new StringToDateTimeConverter());
                    b.Property(e => e.NullableStringToNullableDateTime).HasConversion((ValueConverter)new StringToDateTimeConverter());

                    b.Property(e => e.StringToDateTimeOffset).HasConversion(new StringToDateTimeOffsetConverter());
                    b.Property(e => e.StringToNullableDateTimeOffset).HasConversion(new StringToDateTimeOffsetConverter());
                    b.Property(e => e.NullableStringToDateTimeOffset)
                        .HasConversion((ValueConverter)new StringToDateTimeOffsetConverter());
                    b.Property(e => e.NullableStringToNullableDateTimeOffset)
                        .HasConversion((ValueConverter)new StringToDateTimeOffsetConverter());

                    b.Property(e => e.StringToEnum).HasConversion(new StringToEnumConverter<TheExperience>());
                    b.Property(e => e.StringToNullableEnum).HasConversion(new StringToEnumConverter<TheExperience>());
                    b.Property(e => e.NullableStringToEnum).HasConversion((ValueConverter)new StringToEnumConverter<TheExperience>());
                    b.Property(e => e.NullableStringToNullableEnum)
                        .HasConversion((ValueConverter)new StringToEnumConverter<TheExperience>());

                    b.Property(e => e.StringToGuid).HasConversion(new StringToGuidConverter());
                    b.Property(e => e.StringToNullableGuid).HasConversion(new StringToGuidConverter());
                    b.Property(e => e.NullableStringToGuid).HasConversion((ValueConverter)new StringToGuidConverter());
                    b.Property(e => e.NullableStringToNullableGuid).HasConversion((ValueConverter)new StringToGuidConverter());

                    b.Property(e => e.StringToNumber).HasConversion(new StringToNumberConverter<byte>());
                    b.Property(e => e.StringToNullableNumber).HasConversion(new StringToNumberConverter<byte>());
                    b.Property(e => e.NullableStringToNumber).HasConversion((ValueConverter)new StringToNumberConverter<byte>());
                    b.Property(e => e.NullableStringToNullableNumber)
                        .HasConversion((ValueConverter)new StringToNumberConverter<byte>());

                    b.Property(e => e.StringToTimeSpan).HasConversion(new StringToTimeSpanConverter());
                    b.Property(e => e.StringToNullableTimeSpan).HasConversion(new StringToTimeSpanConverter());
                    b.Property(e => e.NullableStringToTimeSpan).HasConversion((ValueConverter)new StringToTimeSpanConverter());
                    b.Property(e => e.NullableStringToNullableTimeSpan).HasConversion((ValueConverter)new StringToTimeSpanConverter());

                    b.Property(e => e.TimeSpanToTicks).HasConversion(new TimeSpanToTicksConverter());
                    b.Property(e => e.TimeSpanToNullableTicks).HasConversion(new TimeSpanToTicksConverter());
                    b.Property(e => e.NullableTimeSpanToTicks).HasConversion(new TimeSpanToTicksConverter());
                    b.Property(e => e.NullableTimeSpanToNullableTicks).HasConversion(new TimeSpanToTicksConverter());

                    b.Property(e => e.TimeSpanToString).HasConversion(new TimeSpanToStringConverter());
                    b.Property(e => e.TimeSpanToNullableString).HasConversion(new TimeSpanToStringConverter());
                    b.Property(e => e.NullableTimeSpanToString).HasConversion(new TimeSpanToStringConverter());
                    b.Property(e => e.NullableTimeSpanToNullableString).HasConversion(new TimeSpanToStringConverter());

                    b.Property(e => e.UriToString).HasConversion((ValueConverter)new UriToStringConverter());
                    b.Property(e => e.UriToNullableString).HasConversion((ValueConverter)new UriToStringConverter());
                    b.Property(e => e.NullableUriToString).HasConversion(new UriToStringConverter());
                    b.Property(e => e.NullableUriToNullableString).HasConversion(new UriToStringConverter());

                    b.Property(e => e.NonNullIntToNullString).HasConversion(new NonNullIntToNullStringConverter());
                    b.Property(e => e.NonNullIntToNonNullString).HasConversion(new NonNullIntToNonNullStringConverter());
                    b.Property(e => e.NullIntToNullString).HasConversion(new NullIntToNullStringConverter()).IsRequired(false);
                    b.Property(e => e.NullIntToNonNullString).HasConversion(new NullIntToNonNullStringConverter()).IsRequired(false);

                    b.Property(e => e.NullStringToNonNullString).HasConversion(new NullStringToNonNullStringConverter()).IsRequired();
                    b.Property(e => e.NonNullStringToNullString).HasConversion(new NonNullStringToNullStringConverter())
                        .IsRequired(false);

                    b.Property(e => e.NullableListOfInt).HasConversion(
                        (ValueConverter?)new ListOfIntToJsonConverter(), new ListOfIntComparer());

                    b.Property(e => e.ListOfInt).HasConversion(
                        new ListOfIntToJsonConverter(), new ListOfIntComparer());

                    b.Property(e => e.NullableEnumerableOfInt).HasConversion(
                        (ValueConverter?)new EnumerableOfIntToJsonConverter(), new EnumerableOfIntComparer());

                    b.Property(e => e.EnumerableOfInt).HasConversion(
                        new EnumerableOfIntToJsonConverter(), new EnumerableOfIntComparer());
                });

            var complexType = modelBuilder.Model.GetEntityTypes().Single(x => x.ClrType == typeof(RootEntity)).GetComplexProperties().Single().ComplexType;
            foreach (var property in complexType.GetProperties())
            {
                if (property.GetValueConverter() is null)
                {
                    Assert.Fail("All properties should have a value converter configured");
                }
            }
        }
    }

    protected class RootEntity
    {
        public Guid Id { get; set; }

        public ConvertingEntity ConvertingEntity { get; set; } = null!;
    }
}
