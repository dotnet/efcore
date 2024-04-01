// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class BuiltInDataTypesTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : BuiltInDataTypesTestBase<TFixture>.BuiltInDataTypesFixtureBase, new()
{
    protected BuiltInDataTypesTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Can_filter_projection_with_captured_enum_variable(bool async)
    {
        using var context = CreateContext();
        var templateType = EmailTemplateTypeDto.PasswordResetRequest;

        var query = context
            .Set<EmailTemplate>()
            .Select(
                t => new EmailTemplateDto { Id = t.Id, TemplateType = (EmailTemplateTypeDto)t.TemplateType })
            .Where(t => t.TemplateType == templateType);

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Single(results);
        Assert.Equal(EmailTemplateTypeDto.PasswordResetRequest, results.Single().TemplateType);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Can_filter_projection_with_inline_enum_variable(bool async)
    {
        using var context = CreateContext();
        var query = context
            .Set<EmailTemplate>()
            .Select(
                t => new EmailTemplateDto { Id = t.Id, TemplateType = (EmailTemplateTypeDto)t.TemplateType })
            .Where(t => t.TemplateType == EmailTemplateTypeDto.PasswordResetRequest);

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Single(results);
        Assert.Equal(EmailTemplateTypeDto.PasswordResetRequest, results.Single().TemplateType);
    }

    [ConditionalFact]
    public virtual async Task Can_perform_query_with_max_length()
    {
        var shortString = "Sky";
        var shortBinary = new byte[] { 8, 8, 7, 8, 7 };
        var longString = new string('X', Fixture.LongStringLength);
        var longBinary = new byte[Fixture.LongStringLength];
        for (var i = 0; i < longBinary.Length; i++)
        {
            longBinary[i] = (byte)i;
        }

        using (var context = CreateContext())
        {
            context.Set<MaxLengthDataTypes>().Add(
                new MaxLengthDataTypes
                {
                    Id = 799,
                    String3 = shortString,
                    ByteArray5 = shortBinary,
                    String9000 = longString,
                    StringUnbounded = longString,
                    ByteArray9000 = longBinary
                });

            Assert.Equal(1, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            Assert.NotNull(
                (await context.Set<MaxLengthDataTypes>().Where(e => e.Id == 799 && e.String3 == shortString).ToListAsync())
                .SingleOrDefault());

            Assert.NotNull(
                (await context.Set<MaxLengthDataTypes>().Where(e => e.Id == 799 && e.String9000 == longString).ToListAsync())
                .SingleOrDefault());

            Assert.NotNull(
                (await context.Set<MaxLengthDataTypes>().Where(e => e.Id == 799 && e.StringUnbounded == longString).ToListAsync())
                .SingleOrDefault());

            Assert.NotNull(
                (await context.Set<MaxLengthDataTypes>().Where(e => e.Id == 799 && e.ByteArray5 == shortBinary).ToListAsync())
                .SingleOrDefault());

            Assert.NotNull(
                (await context.Set<MaxLengthDataTypes>().Where(e => e.Id == 799 && e.ByteArray9000 == longBinary).ToListAsync())
                .SingleOrDefault());
        }
    }

    [ConditionalFact]
    public virtual async Task Can_perform_query_with_ansi_strings_test()
    {
        var shortString = Fixture.SupportsUnicodeToAnsiConversion ? "Ϩky" : "sky";
        var longString = Fixture.SupportsUnicodeToAnsiConversion
            ? new string('Ϩ', Fixture.LongStringLength)
            : new string('s', Fixture.LongStringLength);

        using (var context = CreateContext())
        {
            context.Set<UnicodeDataTypes>().Add(
                new UnicodeDataTypes
                {
                    Id = 799,
                    StringDefault = shortString,
                    StringAnsi = shortString,
                    StringAnsi3 = shortString,
                    StringAnsi9000 = longString,
                    StringUnicode = shortString
                });

            Assert.Equal(1, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            Assert.NotNull(
                (await context.Set<UnicodeDataTypes>().Where(e => e.Id == 799 && e.StringDefault == shortString).ToListAsync())
                .SingleOrDefault());
            Assert.NotNull(
                (await context.Set<UnicodeDataTypes>().Where(e => e.Id == 799 && e.StringAnsi == shortString).ToListAsync())
                .SingleOrDefault());
            Assert.NotNull(
                (await context.Set<UnicodeDataTypes>().Where(e => e.Id == 799 && e.StringAnsi3 == shortString).ToListAsync())
                .SingleOrDefault());

            if (Fixture.SupportsLargeStringComparisons)
            {
                Assert.NotNull(
                    (await context.Set<UnicodeDataTypes>().Where(e => e.Id == 799 && e.StringAnsi9000 == longString).ToListAsync())
                    .SingleOrDefault());
            }

            Assert.NotNull(
                (await context.Set<UnicodeDataTypes>().Where(e => e.Id == 799 && e.StringUnicode == shortString).ToListAsync())
                .SingleOrDefault());

            var entity = (await context.Set<UnicodeDataTypes>().Where(e => e.Id == 799).ToListAsync()).Single();

            Assert.Equal(shortString, entity.StringDefault);
            Assert.Equal(shortString, entity.StringUnicode);

            if (Fixture.SupportsAnsi
                && Fixture.SupportsUnicodeToAnsiConversion)
            {
                Assert.NotEqual(shortString, entity.StringAnsi);
                Assert.NotEqual(shortString, entity.StringAnsi3);
                Assert.NotEqual(longString, entity.StringAnsi9000);
            }
            else
            {
                Assert.Equal(shortString, entity.StringAnsi);
                Assert.Equal(shortString, entity.StringAnsi3);
                Assert.Equal(longString, entity.StringAnsi9000);
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Can_query_using_any_data_type()
    {
        using var context = CreateContext();
        var source = AddTestBuiltInDataTypes(context.Set<BuiltInDataTypes>());

        Assert.Equal(1, await context.SaveChangesAsync());

        await QueryBuiltInDataTypesTest(source);
    }

    [ConditionalFact]
    public virtual async Task Can_query_using_any_data_type_shadow()
    {
        using var context = CreateContext();
        var source = AddTestBuiltInDataTypes(context.Set<BuiltInDataTypesShadow>());

        Assert.Equal(1, await context.SaveChangesAsync());

        await QueryBuiltInDataTypesTest(source);
    }

    private async Task QueryBuiltInDataTypesTest<TEntity>(EntityEntry<TEntity> source)
        where TEntity : BuiltInDataTypesBase
    {
        using var context = CreateContext();
        var set = context.Set<TEntity>();
        var entity = (await set.Where(e => e.Id == 11).ToListAsync()).Single();
        var entityType = context.Model.FindEntityType(typeof(TEntity));

        var param1 = (short)-1234;
        Assert.Same(
            entity,
            (await set.Where(e => e.Id == 11 && EF.Property<short>(e, nameof(BuiltInDataTypes.TestInt16)) == param1).ToListAsync())
            .Single());

        var param2 = -123456789;
        Assert.Same(
            entity,
            (await set.Where(e => e.Id == 11 && EF.Property<int>(e, nameof(BuiltInDataTypes.TestInt32)) == param2).ToListAsync()).Single());

        var param3 = -1234567890123456789L;
        if (Fixture.IntegerPrecision == 64)
        {
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<long>(e, nameof(BuiltInDataTypes.TestInt64)) == param3).ToListAsync())
                .Single());
        }

        double? param4 = -1.23456789;
        if (Fixture.StrictEquality)
        {
            Assert.Same(
                entity, (await set.Where(
                    e => e.Id == 11
                        && EF.Property<double>(e, nameof(BuiltInDataTypes.TestDouble)) == param4).ToListAsync()).Single());
        }
        else if (Fixture.SupportsDecimalComparisons)
        {
            double? param4l = -1.234567891;
            double? param4h = -1.234567889;
            Assert.Same(
                entity, (await set.Where(
                        e => e.Id == 11
                            && (EF.Property<double>(e, nameof(BuiltInDataTypes.TestDouble)) == param4
                                || (EF.Property<double>(e, nameof(BuiltInDataTypes.TestDouble)) > param4l
                                    && EF.Property<double>(e, nameof(BuiltInDataTypes.TestDouble)) < param4h)))
                    .ToListAsync()).Single());
        }

        var param5 = -1234567890.01M;
        Assert.Same(
            entity,
            (await set.Where(e => e.Id == 11 && EF.Property<decimal>(e, nameof(BuiltInDataTypes.TestDecimal)) == param5).ToListAsync())
            .Single());

        var param6 = Fixture.DefaultDateTime;
        Assert.Same(
            entity,
            (await set.Where(e => e.Id == 11 && EF.Property<DateTime>(e, nameof(BuiltInDataTypes.TestDateTime)) == param6).ToListAsync())
            .Single());

        if (entityType.FindProperty(nameof(BuiltInDataTypes.TestDateTimeOffset)) != null)
        {
            var param7 = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0));
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<DateTimeOffset>(e, nameof(BuiltInDataTypes.TestDateTimeOffset)) == param7)
                    .ToListAsync()).Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.TestTimeSpan)) != null)
        {
            var param8 = new TimeSpan(0, 10, 9, 8, 7);
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<TimeSpan>(e, nameof(BuiltInDataTypes.TestTimeSpan)) == param8)
                    .ToListAsync())
                .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.TestDateOnly)) != null)
        {
            var param9 = new DateOnly(2020, 3, 1);
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<DateOnly>(e, nameof(BuiltInDataTypes.TestDateOnly)) == param9)
                    .ToListAsync())
                .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.TestTimeOnly)) != null)
        {
            var param10 = new TimeOnly(12, 30, 45, 123);
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<TimeOnly>(e, nameof(BuiltInDataTypes.TestTimeOnly)) == param10)
                    .ToListAsync())
                .Single());
        }

        var param11 = -1.234F;
        if (Fixture.StrictEquality)
        {
            Assert.Same(
                entity, (await set.Where(
                    e => e.Id == 11
                        && EF.Property<float>(e, nameof(BuiltInDataTypes.TestSingle)) == param11).ToListAsync()).Single());
        }
        else if (Fixture.SupportsDecimalComparisons)
        {
            var param11l = -1.2341F;
            var param11h = -1.2339F;
            Assert.Same(
                entity, (await set.Where(
                    e => e.Id == 11
                        && (EF.Property<float>(e, nameof(BuiltInDataTypes.TestSingle)) == param11
                            || (EF.Property<float>(e, nameof(BuiltInDataTypes.TestSingle)) > param11l
                                && EF.Property<float>(e, nameof(BuiltInDataTypes.TestSingle)) < param11h))).ToListAsync()).Single());
        }

        var param12 = true;
        Assert.Same(
            entity,
            (await set.Where(e => e.Id == 11 && EF.Property<bool>(e, nameof(BuiltInDataTypes.TestBoolean)) == param12).ToListAsync())
            .Single());

        if (entityType.FindProperty(nameof(BuiltInDataTypes.TestByte)) != null)
        {
            var param13 = (byte)255;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<byte>(e, nameof(BuiltInDataTypes.TestByte)) == param13).ToListAsync())
                .Single());
        }

        var param14 = Enum64.SomeValue;
        Assert.Same(
            entity,
            (await set.Where(e => e.Id == 11 && EF.Property<Enum64>(e, nameof(BuiltInDataTypes.Enum64)) == param14).ToListAsync())
            .Single());

        var param15 = Enum32.SomeValue;
        Assert.Same(
            entity,
            (await set.Where(e => e.Id == 11 && EF.Property<Enum32>(e, nameof(BuiltInDataTypes.Enum32)) == param15).ToListAsync())
            .Single());

        var param16 = Enum16.SomeValue;
        Assert.Same(
            entity,
            (await set.Where(e => e.Id == 11 && EF.Property<Enum16>(e, nameof(BuiltInDataTypes.Enum16)) == param16).ToListAsync())
            .Single());

        if (entityType.FindProperty(nameof(BuiltInDataTypes.Enum8)) != null)
        {
            var param17 = Enum8.SomeValue;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<Enum8>(e, nameof(BuiltInDataTypes.Enum8)) == param17).ToListAsync())
                .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.TestUnsignedInt16)) != null)
        {
            var param18 = (ushort)1234;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<ushort>(e, nameof(BuiltInDataTypes.TestUnsignedInt16)) == param18)
                    .ToListAsync())
                .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.TestUnsignedInt32)) != null)
        {
            var param19 = 1234565789U;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<uint>(e, nameof(BuiltInDataTypes.TestUnsignedInt32)) == param19)
                    .ToListAsync())
                .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.TestUnsignedInt64)) != null)
        {
            var param20 = 1234567890123456789UL;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<ulong>(e, nameof(BuiltInDataTypes.TestUnsignedInt64)) == param20)
                    .ToListAsync())
                .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.TestCharacter)) != null)
        {
            var param21 = 'a';
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<char>(e, nameof(BuiltInDataTypes.TestCharacter)) == param21).ToListAsync())
                .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.TestSignedByte)) != null)
        {
            var param22 = (sbyte)-128;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<sbyte>(e, nameof(BuiltInDataTypes.TestSignedByte)) == param22)
                    .ToListAsync())
                .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.EnumU64)) != null)
        {
            var param23 = EnumU64.SomeValue;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<EnumU64>(e, nameof(BuiltInDataTypes.EnumU64)) == param23).ToListAsync())
                .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.EnumU32)) != null)
        {
            var param24 = EnumU32.SomeValue;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<EnumU32>(e, nameof(BuiltInDataTypes.EnumU32)) == param24).ToListAsync())
                .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.EnumU16)) != null)
        {
            var param25 = EnumU16.SomeValue;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<EnumU16>(e, nameof(BuiltInDataTypes.EnumU16)) == param25).ToListAsync())
                .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.EnumS8)) != null)
        {
            var param26 = EnumS8.SomeValue;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<EnumS8>(e, nameof(BuiltInDataTypes.EnumS8)) == param26).ToListAsync())
                .Single());
        }

        if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInDataTypes.Enum64))?.GetProviderClrType()) == typeof(long))
        {
            var param27 = 1;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<Enum64>(e, nameof(BuiltInDataTypes.Enum64)) == (Enum64)param27)
                    .ToListAsync())
                .Single());
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && (int)EF.Property<Enum64>(e, nameof(BuiltInDataTypes.Enum64)) == param27).ToListAsync())
                .Single());
        }

        if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInDataTypes.Enum32))?.GetProviderClrType()) == typeof(int))
        {
            var param28 = 1;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<Enum32>(e, nameof(BuiltInDataTypes.Enum32)) == (Enum32)param28)
                    .ToListAsync())
                .Single());
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && (int)EF.Property<Enum32>(e, nameof(BuiltInDataTypes.Enum32)) == param28).ToListAsync())
                .Single());
        }

        if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInDataTypes.Enum16))?.GetProviderClrType()) == typeof(short))
        {
            var param29 = 1;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<Enum16>(e, nameof(BuiltInDataTypes.Enum16)) == (Enum16)param29)
                    .ToListAsync())
                .Single());
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && (int)EF.Property<Enum16>(e, nameof(BuiltInDataTypes.Enum16)) == param29).ToListAsync())
                .Single());
        }

        if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInDataTypes.Enum8))?.GetProviderClrType()) == typeof(byte))
        {
            var param30 = 1;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<Enum8>(e, nameof(BuiltInDataTypes.Enum8)) == (Enum8)param30).ToListAsync())
                .Single());
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && (int)EF.Property<Enum8>(e, nameof(BuiltInDataTypes.Enum8)) == param30).ToListAsync())
                .Single());
        }

        foreach (var propertyEntry in context.Entry(entity).Properties)
        {
            if (propertyEntry.Metadata.ValueGenerated != ValueGenerated.Never)
            {
                continue;
            }

            Assert.Equal(
                source.Property(propertyEntry.Metadata).CurrentValue,
                propertyEntry.CurrentValue);
        }
    }

    protected EntityEntry<TEntity> AddTestBuiltInDataTypes<TEntity>(DbSet<TEntity> set)
        where TEntity : BuiltInDataTypesBase, new()
    {
        var entityEntry = set.Add(
            new TEntity { Id = 11 });

        entityEntry.CurrentValues.SetValues(
            new BuiltInDataTypes
            {
                Id = 11,
                PartitionId = 1,
                TestInt16 = -1234,
                TestInt32 = -123456789,
                TestInt64 = -1234567890123456789L,
                TestDouble = -1.23456789,
                TestDecimal = -1234567890.01M,
                TestDateTime = Fixture.DefaultDateTime,
                TestDateTimeOffset = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0)),
                TestTimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                TestDateOnly = new DateOnly(2020, 3, 1),
                TestTimeOnly = new TimeOnly(12, 30, 45, 123),
                TestSingle = -1.234F,
                TestBoolean = true,
                TestByte = 255,
                TestUnsignedInt16 = 1234,
                TestUnsignedInt32 = 1234565789U,
                TestUnsignedInt64 = 1234567890123456789UL,
                TestCharacter = 'a',
                TestSignedByte = -128,
                Enum64 = Enum64.SomeValue,
                Enum32 = Enum32.SomeValue,
                Enum16 = Enum16.SomeValue,
                Enum8 = Enum8.SomeValue,
                EnumU64 = EnumU64.SomeValue,
                EnumU32 = EnumU32.SomeValue,
                EnumU16 = EnumU16.SomeValue,
                EnumS8 = EnumS8.SomeValue
            });

        return entityEntry;
    }

    [ConditionalFact]
    public virtual async Task Can_query_using_any_nullable_data_type()
    {
        using var context = CreateContext();
        var source = AddTestBuiltInNullableDataTypes(context.Set<BuiltInNullableDataTypes>());

        Assert.Equal(1, await context.SaveChangesAsync());

        await QueryBuiltInNullableDataTypesTest(source);
    }

    [ConditionalFact]
    public virtual async Task Can_query_using_any_data_type_nullable_shadow()
    {
        using var context = CreateContext();
        var source = AddTestBuiltInNullableDataTypes(context.Set<BuiltInNullableDataTypesShadow>());

        Assert.Equal(1, await context.SaveChangesAsync());

        await QueryBuiltInNullableDataTypesTest(source);
    }

    private async Task QueryBuiltInNullableDataTypesTest<TEntity>(EntityEntry<TEntity> source)
        where TEntity : BuiltInNullableDataTypesBase
    {
        using var context = CreateContext();
        var set = context.Set<TEntity>();
        var entity = (await set.Where(e => e.Id == 11).ToListAsync()).Single();
        var entityType = context.Model.FindEntityType(typeof(TEntity));

        short? param1 = -1234;
        Assert.Same(
            entity,
            (await set.Where(e => e.Id == 11 && EF.Property<short?>(e, nameof(BuiltInNullableDataTypes.TestNullableInt16)) == param1)
                .ToListAsync()).Single());

        int? param2 = -123456789;
        Assert.Same(
            entity,
            (await set.Where(e => e.Id == 11 && EF.Property<int?>(e, nameof(BuiltInNullableDataTypes.TestNullableInt32)) == param2)
                .ToListAsync()).Single());

        long? param3 = -1234567890123456789L;
        Assert.Same(
            entity,
            (await set.Where(e => e.Id == 11 && EF.Property<long?>(e, nameof(BuiltInNullableDataTypes.TestNullableInt64)) == param3)
                .ToListAsync()).Single());

        double? param4 = -1.23456789;
        if (Fixture.StrictEquality)
        {
            Assert.Same(
                entity, (await set.Where(
                    e => e.Id == 11
                        && EF.Property<double?>(e, nameof(BuiltInNullableDataTypes.TestNullableDouble)) == param4).ToListAsync())
                .Single());
        }
        else if (Fixture.SupportsDecimalComparisons)
        {
            double? param4l = -1.234567891;
            double? param4h = -1.234567889;
            Assert.Same(
                entity, (await set.Where(
                        e => e.Id == 11
                            && (EF.Property<double?>(e, nameof(BuiltInNullableDataTypes.TestNullableDouble)) == param4
                                || (EF.Property<double?>(e, nameof(BuiltInNullableDataTypes.TestNullableDouble)) > param4l
                                    && EF.Property<double?>(e, nameof(BuiltInNullableDataTypes.TestNullableDouble)) < param4h)))
                    .ToListAsync()).Single());
        }

        decimal? param5 = -1234567890.01M;
        Assert.Same(
            entity,
            (await set.Where(e => e.Id == 11 && EF.Property<decimal?>(e, nameof(BuiltInNullableDataTypes.TestNullableDecimal)) == param5)
                .ToListAsync()).Single());

        DateTime? param6 = Fixture.DefaultDateTime;
        Assert.Same(
            entity,
            (await set.Where(e => e.Id == 11 && EF.Property<DateTime?>(e, nameof(BuiltInNullableDataTypes.TestNullableDateTime)) == param6)
                .ToListAsync()).Single());

        if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableDateTimeOffset)) != null)
        {
            DateTimeOffset? param7 = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0));
            Assert.Same(
                entity,
                (await set.Where(
                    e => e.Id == 11
                        && EF.Property<DateTimeOffset?>(e, nameof(BuiltInNullableDataTypes.TestNullableDateTimeOffset))
                        == param7).ToListAsync()).Single());
        }

        if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableTimeSpan)) != null)
        {
            TimeSpan? param8 = new TimeSpan(0, 10, 9, 8, 7);
            Assert.Same(
                entity,
                (await set.Where(
                        e => e.Id == 11
                            && EF.Property<TimeSpan?>(e, nameof(BuiltInNullableDataTypes.TestNullableTimeSpan))
                            == param8)
                    .ToListAsync()).Single());
        }

        if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableDateOnly)) != null)
        {
            DateOnly? param9 = new DateOnly(2020, 3, 1);
            Assert.Same(
                entity,
                (await set.Where(
                        e => e.Id == 11
                            && EF.Property<DateOnly?>(e, nameof(BuiltInNullableDataTypes.TestNullableDateOnly))
                            == param9)
                    .ToListAsync()).Single());
        }

        if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableTimeOnly)) != null)
        {
            TimeOnly? param10 = new TimeOnly(12, 30, 45, 123);
            Assert.Same(
                entity,
                (await set.Where(
                        e => e.Id == 11
                            && EF.Property<TimeOnly?>(e, nameof(BuiltInNullableDataTypes.TestNullableTimeOnly))
                            == param10)
                    .ToListAsync()).Single());
        }

        float? param11 = -1.234F;
        if (Fixture.StrictEquality)
        {
            Assert.Same(
                entity, (await set.Where(
                    e => e.Id == 11
                        && EF.Property<float?>(e, nameof(BuiltInNullableDataTypes.TestNullableSingle)) == param11).ToListAsync())
                .Single());
        }
        else if (Fixture.SupportsDecimalComparisons)
        {
            float? param11l = -1.2341F;
            float? param11h = -1.2339F;
            Assert.Same(
                entity, (await set.Where(
                        e => e.Id == 11
                            && (EF.Property<float?>(e, nameof(BuiltInNullableDataTypes.TestNullableSingle)) == param11
                                || (EF.Property<float?>(e, nameof(BuiltInNullableDataTypes.TestNullableSingle)) > param11l
                                    && EF.Property<float?>(e, nameof(BuiltInNullableDataTypes.TestNullableSingle)) < param11h)))
                    .ToListAsync()).Single());
        }

        bool? param12 = true;
        Assert.Same(
            entity,
            (await set.Where(e => e.Id == 11 && EF.Property<bool?>(e, nameof(BuiltInNullableDataTypes.TestNullableBoolean)) == param12)
                .ToListAsync()).Single());

        if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableByte)) != null)
        {
            byte? param13 = 255;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<byte?>(e, nameof(BuiltInNullableDataTypes.TestNullableByte)) == param13)
                    .ToListAsync()).Single());
        }

        Enum64? param14 = Enum64.SomeValue;
        Assert.Same(
            entity,
            (await set.Where(e => e.Id == 11 && EF.Property<Enum64?>(e, nameof(BuiltInNullableDataTypes.Enum64)) == param14).ToListAsync())
            .Single());

        Enum32? param15 = Enum32.SomeValue;
        Assert.Same(
            entity,
            (await set.Where(e => e.Id == 11 && EF.Property<Enum32?>(e, nameof(BuiltInNullableDataTypes.Enum32)) == param15).ToListAsync())
            .Single());

        Enum16? param16 = Enum16.SomeValue;
        Assert.Same(
            entity,
            (await set.Where(e => e.Id == 11 && EF.Property<Enum16?>(e, nameof(BuiltInNullableDataTypes.Enum16)) == param16).ToListAsync())
            .Single());

        if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.Enum8)) != null)
        {
            Enum8? param17 = Enum8.SomeValue;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<Enum8?>(e, nameof(BuiltInNullableDataTypes.Enum8)) == param17)
                    .ToListAsync())
                .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt16)) != null)
        {
            ushort? param18 = 1234;
            Assert.Same(
                entity,
                (await set.Where(
                    e => e.Id == 11
                        && EF.Property<ushort?>(e, nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt16))
                        == param18).ToListAsync()).Single());
        }

        if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt32)) != null)
        {
            uint? param19 = 1234565789U;
            Assert.Same(
                entity,
                (await set.Where(
                        e => e.Id == 11
                            && EF.Property<uint?>(e, nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt32))
                            == param19)
                    .ToListAsync()).Single());
        }

        if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt64)) != null)
        {
            ulong? param20 = 1234567890123456789UL;
            Assert.Same(
                entity,
                (await set.Where(
                    e => e.Id == 11
                        && EF.Property<ulong?>(
                            e, nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt64))
                        == param20).ToListAsync()).Single());
        }

        if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableCharacter)) != null)
        {
            char? param21 = 'a';
            Assert.Same(
                entity,
                (await set.Where(
                        e => e.Id == 11 && EF.Property<char?>(e, nameof(BuiltInNullableDataTypes.TestNullableCharacter)) == param21)
                    .ToListAsync()).Single());
        }

        if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableSignedByte)) != null)
        {
            sbyte? param22 = -128;
            Assert.Same(
                entity,
                (await set.Where(
                        e => e.Id == 11
                            && EF.Property<sbyte?>(e, nameof(BuiltInNullableDataTypes.TestNullableSignedByte))
                            == param22)
                    .ToListAsync()).Single());
        }

        if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU64)) != null)
        {
            var param23 = EnumU64.SomeValue;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<EnumU64?>(e, nameof(BuiltInNullableDataTypes.EnumU64)) == param23)
                    .ToListAsync())
                .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU32)) != null)
        {
            var param24 = EnumU32.SomeValue;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<EnumU32?>(e, nameof(BuiltInNullableDataTypes.EnumU32)) == param24)
                    .ToListAsync())
                .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU16)) != null)
        {
            var param25 = EnumU16.SomeValue;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<EnumU16?>(e, nameof(BuiltInNullableDataTypes.EnumU16)) == param25)
                    .ToListAsync())
                .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumS8)) != null)
        {
            var param26 = EnumS8.SomeValue;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<EnumS8?>(e, nameof(BuiltInNullableDataTypes.EnumS8)) == param26)
                    .ToListAsync())
                .Single());
        }

        if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInNullableDataTypes.Enum64))?.GetProviderClrType())
            == typeof(long))
        {
            int? param27 = 1;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<Enum64?>(e, nameof(BuiltInNullableDataTypes.Enum64)) == (Enum64)param27)
                    .ToListAsync()).Single());
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && (int)EF.Property<Enum64?>(e, nameof(BuiltInNullableDataTypes.Enum64)) == param27)
                    .ToListAsync()).Single());
        }

        if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInNullableDataTypes.Enum32))?.GetProviderClrType())
            == typeof(int))
        {
            int? param28 = 1;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<Enum32?>(e, nameof(BuiltInNullableDataTypes.Enum32)) == (Enum32)param28)
                    .ToListAsync()).Single());
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && (int)EF.Property<Enum32?>(e, nameof(BuiltInNullableDataTypes.Enum32)) == param28)
                    .ToListAsync()).Single());
        }

        if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInNullableDataTypes.Enum16))?.GetProviderClrType())
            == typeof(short))
        {
            int? param29 = 1;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<Enum16?>(e, nameof(BuiltInNullableDataTypes.Enum16)) == (Enum16)param29)
                    .ToListAsync()).Single());
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && (int)EF.Property<Enum16?>(e, nameof(BuiltInNullableDataTypes.Enum16)) == param29)
                    .ToListAsync()).Single());
        }

        if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInNullableDataTypes.Enum8))?.GetProviderClrType())
            == typeof(byte))
        {
            int? param30 = 1;
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && EF.Property<Enum8?>(e, nameof(BuiltInNullableDataTypes.Enum8)) == (Enum8)param30)
                    .ToListAsync()).Single());
            Assert.Same(
                entity,
                (await set.Where(e => e.Id == 11 && (int)EF.Property<Enum8?>(e, nameof(BuiltInNullableDataTypes.Enum8)) == param30)
                    .ToListAsync()).Single());
        }

        foreach (var propertyEntry in context.Entry(entity).Properties)
        {
            if (propertyEntry.Metadata.ValueGenerated != ValueGenerated.Never)
            {
                continue;
            }

            Assert.Equal(
                source.Property(propertyEntry.Metadata).CurrentValue,
                propertyEntry.CurrentValue);
        }
    }

    protected virtual EntityEntry<TEntity> AddTestBuiltInNullableDataTypes<TEntity>(DbSet<TEntity> set)
        where TEntity : BuiltInNullableDataTypesBase, new()
    {
        var entityEntry = set.Add(
            new TEntity { Id = 11 });

        entityEntry.CurrentValues.SetValues(
            new BuiltInNullableDataTypes
            {
                Id = 11,
                PartitionId = 1,
                TestNullableInt16 = -1234,
                TestNullableInt32 = -123456789,
                TestNullableInt64 = -1234567890123456789L,
                TestNullableDouble = -1.23456789,
                TestNullableDecimal = -1234567890.01M,
                TestNullableDateTime = Fixture.DefaultDateTime,
                TestNullableDateTimeOffset = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0)),
                TestNullableTimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                TestNullableDateOnly = new DateOnly(2020, 3, 1),
                TestNullableTimeOnly = new TimeOnly(12, 30, 45, 123),
                TestNullableSingle = -1.234F,
                TestNullableBoolean = true,
                TestNullableByte = 255,
                TestNullableUnsignedInt16 = 1234,
                TestNullableUnsignedInt32 = 1234565789U,
                TestNullableUnsignedInt64 = 1234567890123456789UL,
                TestNullableCharacter = 'a',
                TestNullableSignedByte = -128,
                Enum64 = Enum64.SomeValue,
                Enum32 = Enum32.SomeValue,
                Enum16 = Enum16.SomeValue,
                Enum8 = Enum8.SomeValue,
                EnumU64 = EnumU64.SomeValue,
                EnumU32 = EnumU32.SomeValue,
                EnumU16 = EnumU16.SomeValue,
                EnumS8 = EnumS8.SomeValue
            });

        return entityEntry;
    }

    [ConditionalFact]
    public virtual async Task Can_query_using_any_nullable_data_type_as_literal()
    {
        using (var context = CreateContext())
        {
            context.Set<BuiltInNullableDataTypes>().Add(
                new BuiltInNullableDataTypes
                {
                    Id = 12,
                    PartitionId = 1,
                    TestNullableInt16 = -1234,
                    TestNullableInt32 = -123456789,
                    TestNullableInt64 = -1234567890123456789L,
                    TestNullableDouble = -1.23456789,
                    TestNullableDecimal = -1234567890.01M,
                    TestNullableDateTime = Fixture.DefaultDateTime,
                    TestNullableDateTimeOffset = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0)),
                    TestNullableTimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                    TestNullableDateOnly = new DateOnly(2020, 3, 1),
                    TestNullableTimeOnly = new TimeOnly(12, 30, 45, 123),
                    TestNullableSingle = -1.234F,
                    TestNullableBoolean = true,
                    TestNullableByte = 255,
                    TestNullableUnsignedInt16 = 1234,
                    TestNullableUnsignedInt32 = 1234565789U,
                    TestNullableUnsignedInt64 = 1234567890123456789UL,
                    TestNullableCharacter = 'a',
                    TestNullableSignedByte = -128,
                    Enum64 = Enum64.SomeValue,
                    Enum32 = Enum32.SomeValue,
                    Enum16 = Enum16.SomeValue,
                    Enum8 = Enum8.SomeValue,
                    EnumU64 = EnumU64.SomeValue,
                    EnumU32 = EnumU32.SomeValue,
                    EnumU16 = EnumU16.SomeValue,
                    EnumS8 = EnumS8.SomeValue
                });

            Assert.Equal(1, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            var entity = (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12).ToListAsync()).Single();
            var entityType = context.Model.FindEntityType(typeof(BuiltInNullableDataTypes));

            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableInt16 == -1234).ToListAsync())
                .Single());

            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableInt32 == -123456789).ToListAsync())
                .Single());

            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableInt64 == -1234567890123456789L)
                    .ToListAsync())
                .Single());

            if (Fixture.StrictEquality)
            {
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableDouble == -1.23456789)
                        .ToListAsync())
                    .Single());
            }
            else if (Fixture.SupportsDecimalComparisons)
            {
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(
                        e => e.Id == 12
                            && -e.TestNullableDouble + -1.23456789 < 1E-5
                            && -e.TestNullableDouble + -1.23456789 > -1E-5).ToListAsync()).Single());
            }

            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableDouble != 1E18).ToListAsync())
                .Single());

            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableDecimal == -1234567890.01M)
                    .ToListAsync())
                .Single());

            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableDateTime == Fixture.DefaultDateTime)
                    .ToListAsync()).Single());

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableDateTimeOffset)) != null)
            {
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(
                            e => e.Id == 12
                                && e.TestNullableDateTimeOffset
                                == new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0)))
                        .ToListAsync()).Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableTimeSpan)) != null)
            {
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>()
                        .Where(e => e.Id == 12 && e.TestNullableTimeSpan == new TimeSpan(0, 10, 9, 8, 7)).ToListAsync()).Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableDateOnly)) != null)
            {
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>()
                        .Where(e => e.Id == 12 && e.TestNullableDateOnly == new DateOnly(2020, 3, 1)).ToListAsync()).Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableTimeOnly)) != null)
            {
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>()
                        .Where(e => e.Id == 12 && e.TestNullableTimeOnly == new TimeOnly(12, 30, 45, 123)).ToListAsync()).Single());
            }

            if (Fixture.StrictEquality)
            {
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableSingle == -1.234F).ToListAsync())
                    .Single());
            }
            else if (Fixture.SupportsDecimalComparisons)
            {
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && -e.TestNullableSingle + -1.234F < 1E-5)
                        .ToListAsync())
                    .Single());
            }

            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableSingle != 1E-8).ToListAsync())
                .Single());

            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableBoolean == true).ToListAsync())
                .Single());

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableByte)) != null)
            {
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableByte == 255).ToListAsync())
                    .Single());
            }

            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.Enum64 == Enum64.SomeValue).ToListAsync())
                .Single());

            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.Enum32 == Enum32.SomeValue).ToListAsync())
                .Single());

            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.Enum16 == Enum16.SomeValue).ToListAsync())
                .Single());

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.Enum8)) != null)
            {
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.Enum8 == Enum8.SomeValue).ToListAsync())
                    .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt16)) != null)
            {
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableUnsignedInt16 == 1234)
                        .ToListAsync())
                    .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt32)) != null)
            {
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableUnsignedInt32 == 1234565789U)
                        .ToListAsync()).Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt64)) != null)
            {
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>()
                        .Where(e => e.Id == 12 && e.TestNullableUnsignedInt64 == 1234567890123456789UL).ToListAsync()).Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableCharacter)) != null)
            {
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableCharacter == 'a').ToListAsync())
                    .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableSignedByte)) != null)
            {
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableSignedByte == -128).ToListAsync())
                    .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU64)) != null)
            {
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.EnumU64 == EnumU64.SomeValue).ToListAsync())
                    .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU32)) != null)
            {
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.EnumU32 == EnumU32.SomeValue).ToListAsync())
                    .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU16)) != null)
            {
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.EnumU16 == EnumU16.SomeValue).ToListAsync())
                    .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumS8)) != null)
            {
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.EnumS8 == EnumS8.SomeValue).ToListAsync())
                    .Single());
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Can_query_with_null_parameters_using_any_nullable_data_type()
    {
        using (var context = CreateContext())
        {
            context.Set<BuiltInNullableDataTypes>().Add(
                new BuiltInNullableDataTypes { Id = 711 });

            Assert.Equal(1, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            var entity = (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711).ToListAsync()).Single();

            short? param1 = null;
            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableInt16 == param1).ToListAsync())
                .Single());
            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && (long?)e.TestNullableInt16 == param1)
                    .ToListAsync())
                .Single());

            int? param2 = null;
            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableInt32 == param2).ToListAsync())
                .Single());

            long? param3 = null;
            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableInt64 == param3).ToListAsync())
                .Single());

            double? param4 = null;
            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableDouble == param4).ToListAsync())
                .Single());

            decimal? param5 = null;
            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableDecimal == param5).ToListAsync())
                .Single());

            DateTime? param6 = null;
            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableDateTime == param6).ToListAsync())
                .Single());

            DateTimeOffset? param7 = null;
            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableDateTimeOffset == param7)
                    .ToListAsync())
                .Single());

            TimeSpan? param8 = null;
            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableTimeSpan == param8).ToListAsync())
                .Single());

            DateOnly? param9 = null;
            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableDateOnly == param9).ToListAsync())
                .Single());

            TimeOnly? param10 = null;
            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableTimeOnly == param10).ToListAsync())
                .Single());

            float? param11 = null;
            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableSingle == param11).ToListAsync())
                .Single());

            bool? param12 = null;
            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableBoolean == param12).ToListAsync())
                .Single());

            byte? param13 = null;
            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableByte == param13).ToListAsync())
                .Single());

            Enum64? param14 = null;
            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.Enum64 == param14).ToListAsync()).Single());

            Enum32? param15 = null;
            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.Enum32 == param15).ToListAsync()).Single());

            Enum16? param16 = null;
            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.Enum16 == param16).ToListAsync()).Single());

            Enum8? param17 = null;
            Assert.Same(
                entity,
                (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.Enum8 == param17).ToListAsync()).Single());

            var entityType = context.Model.FindEntityType(typeof(BuiltInNullableDataTypes));
            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt16)) != null)
            {
                ushort? param18 = null;
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableUnsignedInt16 == param18)
                        .ToListAsync())
                    .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt32)) != null)
            {
                uint? param19 = null;
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableUnsignedInt32 == param19)
                        .ToListAsync())
                    .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt64)) != null)
            {
                ulong? param20 = null;
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableUnsignedInt64 == param20)
                        .ToListAsync())
                    .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableCharacter)) != null)
            {
                char? param21 = null;
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableCharacter == param21)
                        .ToListAsync())
                    .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableSignedByte)) != null)
            {
                sbyte? param22 = null;
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableSignedByte == param22)
                        .ToListAsync())
                    .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU64)) != null)
            {
                EnumU64? param23 = null;
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.EnumU64 == param23).ToListAsync()).Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU32)) != null)
            {
                EnumU32? param24 = null;
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.EnumU32 == param24).ToListAsync()).Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU16)) != null)
            {
                EnumU16? param25 = null;
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.EnumU16 == param25).ToListAsync()).Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumS8)) != null)
            {
                EnumS8? param26 = null;
                Assert.Same(
                    entity,
                    (await context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.EnumS8 == param26).ToListAsync()).Single());
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_all_non_nullable_data_types()
    {
        using (var context = CreateContext())
        {
            context.Set<BuiltInDataTypes>().Add(
                new BuiltInDataTypes
                {
                    Id = 1,
                    PartitionId = 1,
                    TestInt16 = -1234,
                    TestInt32 = -123456789,
                    TestInt64 = -1234567890123456789L,
                    TestDouble = -1.23456789,
                    TestDecimal = -1234567890.01M,
                    TestDateTime = DateTime.Parse("01/01/2000 12:34:56", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal),
                    TestDateTimeOffset = new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                    TestTimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                    TestDateOnly = new DateOnly(2020, 3, 1),
                    TestTimeOnly = new TimeOnly(12, 30, 45, 123),
                    TestSingle = -1.234F,
                    TestBoolean = true,
                    TestByte = 255,
                    TestUnsignedInt16 = 1234,
                    TestUnsignedInt32 = 1234565789U,
                    TestUnsignedInt64 = 1234567890123456789UL,
                    TestCharacter = 'a',
                    TestSignedByte = -128,
                    Enum64 = Enum64.SomeValue,
                    Enum32 = Enum32.SomeValue,
                    Enum16 = Enum16.SomeValue,
                    Enum8 = Enum8.SomeValue,
                    EnumU64 = EnumU64.SomeValue,
                    EnumU32 = EnumU32.SomeValue,
                    EnumU16 = EnumU16.SomeValue,
                    EnumS8 = EnumS8.SomeValue
                });

            Assert.Equal(1, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            var dt = (await context.Set<BuiltInDataTypes>().Where(e => e.Id == 1).ToListAsync()).Single();

            var entityType = context.Model.FindEntityType(typeof(BuiltInDataTypes));
            AssertEqualIfMapped(entityType, (short)-1234, () => dt.TestInt16);
            AssertEqualIfMapped(entityType, -123456789, () => dt.TestInt32);
            AssertEqualIfMapped(entityType, -1234567890123456789L, () => dt.TestInt64);
            AssertEqualIfMapped(entityType, -1.23456789, () => dt.TestDouble);
            AssertEqualIfMapped(entityType, -1234567890.01M, () => dt.TestDecimal);
            AssertEqualIfMapped(
                entityType, DateTime.Parse("01/01/2000 12:34:56", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal),
                () => dt.TestDateTime);
            AssertEqualIfMapped(
                entityType, new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                () => dt.TestDateTimeOffset);
            AssertEqualIfMapped(entityType, new TimeSpan(0, 10, 9, 8, 7), () => dt.TestTimeSpan);
            AssertEqualIfMapped(entityType, new DateOnly(2020, 3, 1), () => dt.TestDateOnly);
            AssertEqualIfMapped(entityType, new TimeOnly(12, 30, 45, 123), () => dt.TestTimeOnly);
            AssertEqualIfMapped(entityType, -1.234F, () => dt.TestSingle);
            AssertEqualIfMapped(entityType, true, () => dt.TestBoolean);
            AssertEqualIfMapped(entityType, (byte)255, () => dt.TestByte);
            AssertEqualIfMapped(entityType, Enum64.SomeValue, () => dt.Enum64);
            AssertEqualIfMapped(entityType, Enum32.SomeValue, () => dt.Enum32);
            AssertEqualIfMapped(entityType, Enum16.SomeValue, () => dt.Enum16);
            AssertEqualIfMapped(entityType, Enum8.SomeValue, () => dt.Enum8);
            AssertEqualIfMapped(entityType, (ushort)1234, () => dt.TestUnsignedInt16);
            AssertEqualIfMapped(entityType, 1234565789U, () => dt.TestUnsignedInt32);
            AssertEqualIfMapped(entityType, 1234567890123456789UL, () => dt.TestUnsignedInt64);
            AssertEqualIfMapped(entityType, 'a', () => dt.TestCharacter);
            AssertEqualIfMapped(entityType, (sbyte)-128, () => dt.TestSignedByte);
            AssertEqualIfMapped(entityType, EnumU64.SomeValue, () => dt.EnumU64);
            AssertEqualIfMapped(entityType, EnumU32.SomeValue, () => dt.EnumU32);
            AssertEqualIfMapped(entityType, EnumU16.SomeValue, () => dt.EnumU16);
            AssertEqualIfMapped(entityType, EnumS8.SomeValue, () => dt.EnumS8);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_with_max_length_set()
    {
        const string shortString = "Sky";
        var shortBinary = new byte[] { 8, 8, 7, 8, 7 };

        var longString = new string('X', Fixture.LongStringLength);
        var longBinary = new byte[Fixture.LongStringLength];
        for (var i = 0; i < longBinary.Length; i++)
        {
            longBinary[i] = (byte)i;
        }

        using (var context = CreateContext())
        {
            context.Set<MaxLengthDataTypes>().Add(
                new MaxLengthDataTypes
                {
                    Id = 79,
                    String3 = shortString,
                    ByteArray5 = shortBinary,
                    String9000 = longString,
                    StringUnbounded = longString,
                    ByteArray9000 = longBinary
                });

            Assert.Equal(1, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            var dt = (await context.Set<MaxLengthDataTypes>().Where(e => e.Id == 79).ToListAsync()).Single();

            Assert.Equal(shortString, dt.String3);
            Assert.Equal(shortBinary, dt.ByteArray5);
            Assert.Equal(longString, dt.String9000);
            Assert.Equal(longString, dt.StringUnbounded);
            Assert.Equal(longBinary, dt.ByteArray9000);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_binary_key()
    {
        if (!Fixture.SupportsBinaryKeys)
        {
            return;
        }

        using (var context = CreateContext())
        {
            context.Set<BinaryKeyDataType>().AddRange(
                new BinaryKeyDataType { Id = [1, 2, 3], Ex = "X1" },
                new BinaryKeyDataType { Id = [1, 2, 3, 4], Ex = "X3" },
                new BinaryKeyDataType { Id = [1, 2, 3, 4, 5], Ex = "X2" });

            context.Set<BinaryForeignKeyDataType>().AddRange(
                new BinaryForeignKeyDataType { Id = 77, BinaryKeyDataTypeId = [1, 2, 3, 4] },
                new BinaryForeignKeyDataType { Id = 777, BinaryKeyDataTypeId = [1, 2, 3] },
                new BinaryForeignKeyDataType { Id = 7777, BinaryKeyDataTypeId = [1, 2, 3, 4, 5] });

            Assert.Equal(6, await context.SaveChangesAsync());
        }

        async Task<BinaryKeyDataType> QueryByBinaryKey(DbContext context, byte[] bytes)
            => (await context
                .Set<BinaryKeyDataType>()
                .Include(e => e.Dependents)
                .Where(e => e.Id == bytes)
                .ToListAsync()).Single();

        using (var context = CreateContext())
        {
            var entity1 = await QueryByBinaryKey(context, [1, 2, 3]);
            Assert.Equal(new byte[] { 1, 2, 3 }, entity1.Id);
            Assert.Equal(1, entity1.Dependents.Count);

            var entity2 = await QueryByBinaryKey(context, [1, 2, 3, 4]);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, entity2.Id);
            Assert.Equal(1, entity2.Dependents.Count);

            var entity3 = await QueryByBinaryKey(context, [1, 2, 3, 4, 5]);
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, entity3.Id);
            Assert.Equal(1, entity3.Dependents.Count);

            entity3.Ex = "Xx1";
            entity2.Ex = "Xx3";
            entity1.Ex = "Xx7";

            entity1.Dependents.Single().BinaryKeyDataTypeId = [1, 2, 3, 4, 5];

            entity2.Dependents.Single().BinaryKeyDataTypeId = [1, 2, 3, 4, 5];

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var entity1 = await QueryByBinaryKey(context, [1, 2, 3]);
            Assert.Equal("Xx7", entity1.Ex);
            Assert.Equal(0, entity1.Dependents.Count);

            var entity2 = await QueryByBinaryKey(context, [1, 2, 3, 4]);
            Assert.Equal("Xx3", entity2.Ex);
            Assert.Equal(0, entity2.Dependents.Count);

            var entity3 = await QueryByBinaryKey(context, [1, 2, 3, 4, 5]);
            Assert.Equal("Xx1", entity3.Ex);
            Assert.Equal(3, entity3.Dependents.Count);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_null_binary_foreign_key()
    {
        using (var context = CreateContext())
        {
            context.Set<BinaryForeignKeyDataType>().Add(
                new BinaryForeignKeyDataType { Id = 78 });

            Assert.Equal(1, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            var entity = (await context.Set<BinaryForeignKeyDataType>().Where(e => e.Id == 78).ToListAsync()).Single();

            Assert.Null(entity.BinaryKeyDataTypeId);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_string_key()
    {
        using (var context = CreateContext())
        {
            var principal = context.Set<StringKeyDataType>().Add(
                new StringKeyDataType { Id = "Gumball!" }).Entity;

            var dependent = context.Set<StringForeignKeyDataType>().Add(
                new StringForeignKeyDataType { Id = 77, StringKeyDataTypeId = "Gumball!" }).Entity;

            Assert.Same(principal, dependent.Principal);

            Assert.Equal(2, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            var entity = (await context
                .Set<StringKeyDataType>()
                .Include(e => e.Dependents)
                .Where(e => e.Id == "Gumball!")
                .ToListAsync()).Single();

            Assert.Equal("Gumball!", entity.Id);
            Assert.Equal("Gumball!", entity.Dependents.First().StringKeyDataTypeId);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_null_string_foreign_key()
    {
        using (var context = CreateContext())
        {
            context.Set<StringForeignKeyDataType>().Add(
                new StringForeignKeyDataType { Id = 78 });

            Assert.Equal(1, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            var entity = (await context.Set<StringForeignKeyDataType>().Where(e => e.Id == 78).ToListAsync()).Single();

            Assert.Null(entity.StringKeyDataTypeId);
        }
    }

    private void AssertEqualIfMapped<T>(IEntityType entityType, T expected, Expression<Func<T>> actualExpression)
    {
        if (entityType.FindProperty(((MemberExpression)actualExpression.Body).Member.Name) != null)
        {
            var actual = actualExpression.Compile()();
            var type = UnwrapNullableEnumType(typeof(T));
            if (IsSignedInteger(type))
            {
                Assert.True(Equal(Convert.ToInt64(expected), Convert.ToInt64(actual)), $"Expected:\t{expected}\r\nActual:\t{actual}");
            }
            else if (IsUnsignedInteger(type))
            {
                Assert.True(Equal(Convert.ToUInt64(expected), Convert.ToUInt64(actual)), $"Expected:\t{expected}\r\nActual:\t{actual}");
            }
            else if (type == typeof(DateTime))
            {
                Assert.True(
                    Equal((DateTime)(object)expected, (DateTime)(object)actual), $"Expected:\t{expected:O}\r\nActual:\t{actual:O}");
            }
            else if (type == typeof(DateTimeOffset))
            {
                Assert.True(
                    Equal((DateTimeOffset)(object)expected, (DateTimeOffset)(object)actual),
                    $"Expected:\t{expected:O}\r\nActual:\t{actual:O}");
            }
            else
            {
                Assert.Equal(expected, actual);
            }
        }
    }

    private bool Equal(long left, long right)
    {
        if (left >= 0
            && right >= 0)
        {
            return Equal((ulong)left, (ulong)right);
        }

        if (left < 0
            && right < 0)
        {
            return Equal((ulong)-left, (ulong)-right);
        }

        return false;
    }

    private bool Equal(ulong left, ulong right)
    {
        if (Fixture.IntegerPrecision < 64)
        {
            var largestPrecise = 1ul << Fixture.IntegerPrecision;
            while (left > largestPrecise)
            {
                left >>= 1;
                right >>= 1;
            }
        }

        return left == right;
    }

    private bool Equal(DateTime left, DateTime right)
        => left.Equals(right) && (!Fixture.PreservesDateTimeKind || left.Kind == right.Kind);

    private bool Equal(DateTimeOffset left, DateTimeOffset right)
        => left.EqualsExact(right);

    private static Type UnwrapNullableType(Type type)
        => type == null ? null : Nullable.GetUnderlyingType(type) ?? type;

    public static Type UnwrapNullableEnumType(Type type)
    {
        var underlyingNonNullableType = UnwrapNullableType(type);
        if (!underlyingNonNullableType.IsEnum)
        {
            return underlyingNonNullableType;
        }

        return Enum.GetUnderlyingType(underlyingNonNullableType);
    }

    private static bool IsSignedInteger(Type type)
        => type == typeof(int)
            || type == typeof(long)
            || type == typeof(short)
            || type == typeof(sbyte);

    private static bool IsUnsignedInteger(Type type)
        => type == typeof(byte)
            || type == typeof(uint)
            || type == typeof(ulong)
            || type == typeof(ushort)
            || type == typeof(char);

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_all_nullable_data_types_with_values_set_to_null()
    {
        using (var context = CreateContext())
        {
            context.Set<BuiltInNullableDataTypes>().Add(
                new BuiltInNullableDataTypes { Id = 100, PartitionId = 100 });

            Assert.Equal(1, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            var dt = (await context.Set<BuiltInNullableDataTypes>().Where(ndt => ndt.Id == 100).ToListAsync()).Single();

            Assert.Null(dt.TestString);
            Assert.Null(dt.TestByteArray);
            Assert.Null(dt.TestNullableInt16);
            Assert.Null(dt.TestNullableInt32);
            Assert.Null(dt.TestNullableInt64);
            Assert.Null(dt.TestNullableDouble);
            Assert.Null(dt.TestNullableDecimal);
            Assert.Null(dt.TestNullableDateTime);
            Assert.Null(dt.TestNullableDateTimeOffset);
            Assert.Null(dt.TestNullableTimeSpan);
            Assert.Null(dt.TestNullableDateOnly);
            Assert.Null(dt.TestNullableTimeOnly);
            Assert.Null(dt.TestNullableSingle);
            Assert.Null(dt.TestNullableBoolean);
            Assert.Null(dt.TestNullableByte);
            Assert.Null(dt.TestNullableUnsignedInt16);
            Assert.Null(dt.TestNullableUnsignedInt32);
            Assert.Null(dt.TestNullableUnsignedInt64);
            Assert.Null(dt.TestNullableCharacter);
            Assert.Null(dt.TestNullableSignedByte);
            Assert.Null(dt.Enum64);
            Assert.Null(dt.Enum32);
            Assert.Null(dt.Enum16);
            Assert.Null(dt.Enum8);
            Assert.Null(dt.EnumU64);
            Assert.Null(dt.EnumU32);
            Assert.Null(dt.EnumU16);
            Assert.Null(dt.EnumS8);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_all_nullable_data_types_with_values_set_to_non_null()
    {
        using (var context = CreateContext())
        {
            context.Set<BuiltInNullableDataTypes>().Add(
                new BuiltInNullableDataTypes
                {
                    Id = 101,
                    PartitionId = 101,
                    TestString = "TestString",
                    TestByteArray = [10, 9, 8, 7, 6],
                    TestNullableInt16 = -1234,
                    TestNullableInt32 = -123456789,
                    TestNullableInt64 = -1234567890123456789L,
                    TestNullableDouble = -1.23456789,
                    TestNullableDecimal = -1234567890.01M,
                    TestNullableDateTime = DateTime.Parse("01/01/2000 12:34:56").ToUniversalTime(),
                    TestNullableDateTimeOffset = new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                    TestNullableDateOnly = new DateOnly(2020, 3, 1),
                    TestNullableTimeOnly = new TimeOnly(12, 30, 45, 123),
                    TestNullableTimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                    TestNullableSingle = -1.234F,
                    TestNullableBoolean = false,
                    TestNullableByte = 255,
                    TestNullableUnsignedInt16 = 1234,
                    TestNullableUnsignedInt32 = 1234565789U,
                    TestNullableUnsignedInt64 = 1234567890123456789UL,
                    TestNullableCharacter = 'a',
                    TestNullableSignedByte = -128,
                    Enum64 = Enum64.SomeValue,
                    Enum32 = Enum32.SomeValue,
                    Enum16 = Enum16.SomeValue,
                    Enum8 = Enum8.SomeValue,
                    EnumU64 = EnumU64.SomeValue,
                    EnumU32 = EnumU32.SomeValue,
                    EnumU16 = EnumU16.SomeValue,
                    EnumS8 = EnumS8.SomeValue
                });

            Assert.Equal(1, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            var dt = (await context.Set<BuiltInNullableDataTypes>().Where(ndt => ndt.Id == 101).ToListAsync()).Single();

            var entityType = context.Model.FindEntityType(typeof(BuiltInNullableDataTypes));
            AssertEqualIfMapped(entityType, "TestString", () => dt.TestString);
            AssertEqualIfMapped(entityType, [10, 9, 8, 7, 6], () => dt.TestByteArray);
            AssertEqualIfMapped(entityType, (short)-1234, () => dt.TestNullableInt16);
            AssertEqualIfMapped(entityType, -123456789, () => dt.TestNullableInt32);
            AssertEqualIfMapped(entityType, -1234567890123456789L, () => dt.TestNullableInt64);
            AssertEqualIfMapped(entityType, -1.23456789, () => dt.TestNullableDouble);
            AssertEqualIfMapped(entityType, -1234567890.01M, () => dt.TestNullableDecimal);
            AssertEqualIfMapped(entityType, DateTime.Parse("01/01/2000 12:34:56").ToUniversalTime(), () => dt.TestNullableDateTime);
            AssertEqualIfMapped(
                entityType, new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                () => dt.TestNullableDateTimeOffset);
            AssertEqualIfMapped(entityType, new TimeSpan(0, 10, 9, 8, 7), () => dt.TestNullableTimeSpan);
            AssertEqualIfMapped(entityType, new DateOnly(2020, 3, 1), () => dt.TestNullableDateOnly);
            AssertEqualIfMapped(entityType, new TimeOnly(12, 30, 45, 123), () => dt.TestNullableTimeOnly);
            AssertEqualIfMapped(entityType, -1.234F, () => dt.TestNullableSingle);
            AssertEqualIfMapped(entityType, false, () => dt.TestNullableBoolean);
            AssertEqualIfMapped(entityType, (byte)255, () => dt.TestNullableByte);
            AssertEqualIfMapped(entityType, Enum64.SomeValue, () => dt.Enum64);
            AssertEqualIfMapped(entityType, Enum32.SomeValue, () => dt.Enum32);
            AssertEqualIfMapped(entityType, Enum16.SomeValue, () => dt.Enum16);
            AssertEqualIfMapped(entityType, Enum8.SomeValue, () => dt.Enum8);
            AssertEqualIfMapped(entityType, (ushort)1234, () => dt.TestNullableUnsignedInt16);
            AssertEqualIfMapped(entityType, 1234565789U, () => dt.TestNullableUnsignedInt32);
            AssertEqualIfMapped(entityType, 1234567890123456789UL, () => dt.TestNullableUnsignedInt64);
            AssertEqualIfMapped(entityType, 'a', () => dt.TestNullableCharacter);
            AssertEqualIfMapped(entityType, (sbyte)-128, () => dt.TestNullableSignedByte);
            AssertEqualIfMapped(entityType, EnumU64.SomeValue, () => dt.EnumU64);
            AssertEqualIfMapped(entityType, EnumU32.SomeValue, () => dt.EnumU32);
            AssertEqualIfMapped(entityType, EnumU16.SomeValue, () => dt.EnumU16);
            AssertEqualIfMapped(entityType, EnumS8.SomeValue, () => dt.EnumS8);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_object_backed_data_types()
    {
        using (var context = CreateContext())
        {
            context.Set<ObjectBackedDataTypes>().Add(
                new ObjectBackedDataTypes
                {
                    Id = 101,
                    PartitionId = 101,
                    String = "TestString",
                    Bytes = [10, 9, 8, 7, 6],
                    Int16 = -1234,
                    Int32 = -123456789,
                    Int64 = -1234567890123456789L,
                    Double = -1.23456789,
                    Decimal = -1234567890.01M,
                    DateTime = DateTime.Parse("01/01/2000 12:34:56"),
                    DateTimeOffset = new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                    TimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                    DateOnly = new DateOnly(2020, 3, 1),
                    TimeOnly = new TimeOnly(12, 30, 45, 123),
                    Single = -1.234F,
                    Boolean = false,
                    Byte = 255,
                    UnsignedInt16 = 1234,
                    UnsignedInt32 = 1234565789U,
                    UnsignedInt64 = 1234567890123456789UL,
                    Character = 'a',
                    SignedByte = -128,
                    Enum64 = Enum64.SomeValue,
                    Enum32 = Enum32.SomeValue,
                    Enum16 = Enum16.SomeValue,
                    Enum8 = Enum8.SomeValue,
                    EnumU64 = EnumU64.SomeValue,
                    EnumU32 = EnumU32.SomeValue,
                    EnumU16 = EnumU16.SomeValue,
                    EnumS8 = EnumS8.SomeValue
                });

            Assert.Equal(1, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            var dt = (await context.Set<ObjectBackedDataTypes>().Where(ndt => ndt.Id == 101).ToListAsync()).Single();

            var entityType = context.Model.FindEntityType(typeof(ObjectBackedDataTypes));
            AssertEqualIfMapped(entityType, "TestString", () => dt.String);
            AssertEqualIfMapped(entityType, [10, 9, 8, 7, 6], () => dt.Bytes);
            AssertEqualIfMapped(entityType, (short)-1234, () => dt.Int16);
            AssertEqualIfMapped(entityType, -123456789, () => dt.Int32);
            AssertEqualIfMapped(entityType, -1234567890123456789L, () => dt.Int64);
            AssertEqualIfMapped(entityType, -1.23456789, () => dt.Double);
            AssertEqualIfMapped(entityType, -1234567890.01M, () => dt.Decimal);
            AssertEqualIfMapped(entityType, DateTime.Parse("01/01/2000 12:34:56"), () => dt.DateTime);
            AssertEqualIfMapped(
                entityType, new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                () => dt.DateTimeOffset);
            AssertEqualIfMapped(entityType, new TimeSpan(0, 10, 9, 8, 7), () => dt.TimeSpan);
            AssertEqualIfMapped(entityType, new DateOnly(2020, 3, 1), () => dt.DateOnly);
            AssertEqualIfMapped(entityType, new TimeOnly(12, 30, 45, 123), () => dt.TimeOnly);
            AssertEqualIfMapped(entityType, -1.234F, () => dt.Single);
            AssertEqualIfMapped(entityType, false, () => dt.Boolean);
            AssertEqualIfMapped(entityType, (byte)255, () => dt.Byte);
            AssertEqualIfMapped(entityType, Enum64.SomeValue, () => dt.Enum64);
            AssertEqualIfMapped(entityType, Enum32.SomeValue, () => dt.Enum32);
            AssertEqualIfMapped(entityType, Enum16.SomeValue, () => dt.Enum16);
            AssertEqualIfMapped(entityType, Enum8.SomeValue, () => dt.Enum8);
            AssertEqualIfMapped(entityType, (ushort)1234, () => dt.UnsignedInt16);
            AssertEqualIfMapped(entityType, 1234565789U, () => dt.UnsignedInt32);
            AssertEqualIfMapped(entityType, 1234567890123456789UL, () => dt.UnsignedInt64);
            AssertEqualIfMapped(entityType, 'a', () => dt.Character);
            AssertEqualIfMapped(entityType, (sbyte)-128, () => dt.SignedByte);
            AssertEqualIfMapped(entityType, EnumU64.SomeValue, () => dt.EnumU64);
            AssertEqualIfMapped(entityType, EnumU32.SomeValue, () => dt.EnumU32);
            AssertEqualIfMapped(entityType, EnumU16.SomeValue, () => dt.EnumU16);
            AssertEqualIfMapped(entityType, EnumS8.SomeValue, () => dt.EnumS8);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_nullable_backed_data_types()
    {
        using (var context = CreateContext())
        {
            context.Set<NullableBackedDataTypes>().Add(
                new NullableBackedDataTypes
                {
                    Id = 101,
                    PartitionId = 101,
                    Int16 = -1234,
                    Int32 = -123456789,
                    Int64 = -1234567890123456789L,
                    Double = -1.23456789,
                    Decimal = -1234567890.01M,
                    DateTime = DateTime.Parse("01/01/2000 12:34:56"),
                    DateTimeOffset = new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                    TimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                    DateOnly = new DateOnly(2020, 3, 1),
                    TimeOnly = new TimeOnly(12, 30, 45, 123),
                    Single = -1.234F,
                    Boolean = false,
                    Byte = 255,
                    UnsignedInt16 = 1234,
                    UnsignedInt32 = 1234565789U,
                    UnsignedInt64 = 1234567890123456789UL,
                    Character = 'a',
                    SignedByte = -128,
                    Enum64 = Enum64.SomeValue,
                    Enum32 = Enum32.SomeValue,
                    Enum16 = Enum16.SomeValue,
                    Enum8 = Enum8.SomeValue,
                    EnumU64 = EnumU64.SomeValue,
                    EnumU32 = EnumU32.SomeValue,
                    EnumU16 = EnumU16.SomeValue,
                    EnumS8 = EnumS8.SomeValue
                });

            Assert.Equal(1, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            var dt = (await context.Set<NullableBackedDataTypes>().Where(ndt => ndt.Id == 101).ToListAsync()).Single();

            var entityType = context.Model.FindEntityType(typeof(NullableBackedDataTypes));
            AssertEqualIfMapped(entityType, (short)-1234, () => dt.Int16);
            AssertEqualIfMapped(entityType, -123456789, () => dt.Int32);
            AssertEqualIfMapped(entityType, -1234567890123456789L, () => dt.Int64);
            AssertEqualIfMapped(entityType, -1.23456789, () => dt.Double);
            AssertEqualIfMapped(entityType, -1234567890.01M, () => dt.Decimal);
            AssertEqualIfMapped(entityType, DateTime.Parse("01/01/2000 12:34:56"), () => dt.DateTime);
            AssertEqualIfMapped(
                entityType, new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                () => dt.DateTimeOffset);
            AssertEqualIfMapped(entityType, new TimeSpan(0, 10, 9, 8, 7), () => dt.TimeSpan);
            AssertEqualIfMapped(entityType, new DateOnly(2020, 3, 1), () => dt.DateOnly);
            AssertEqualIfMapped(entityType, new TimeOnly(12, 30, 45, 123), () => dt.TimeOnly);
            AssertEqualIfMapped(entityType, -1.234F, () => dt.Single);
            AssertEqualIfMapped(entityType, false, () => dt.Boolean);
            AssertEqualIfMapped(entityType, (byte)255, () => dt.Byte);
            AssertEqualIfMapped(entityType, Enum64.SomeValue, () => dt.Enum64);
            AssertEqualIfMapped(entityType, Enum32.SomeValue, () => dt.Enum32);
            AssertEqualIfMapped(entityType, Enum16.SomeValue, () => dt.Enum16);
            AssertEqualIfMapped(entityType, Enum8.SomeValue, () => dt.Enum8);
            AssertEqualIfMapped(entityType, (ushort)1234, () => dt.UnsignedInt16);
            AssertEqualIfMapped(entityType, 1234565789U, () => dt.UnsignedInt32);
            AssertEqualIfMapped(entityType, 1234567890123456789UL, () => dt.UnsignedInt64);
            AssertEqualIfMapped(entityType, 'a', () => dt.Character);
            AssertEqualIfMapped(entityType, (sbyte)-128, () => dt.SignedByte);
            AssertEqualIfMapped(entityType, EnumU64.SomeValue, () => dt.EnumU64);
            AssertEqualIfMapped(entityType, EnumU32.SomeValue, () => dt.EnumU32);
            AssertEqualIfMapped(entityType, EnumU16.SomeValue, () => dt.EnumU16);
            AssertEqualIfMapped(entityType, EnumS8.SomeValue, () => dt.EnumS8);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_non_nullable_backed_data_types()
    {
        using (var context = CreateContext())
        {
            context.Set<NonNullableBackedDataTypes>().Add(
                new NonNullableBackedDataTypes
                {
                    Id = 101,
                    PartitionId = 101,
                    Int16 = -1234,
                    Int32 = -123456789,
                    Int64 = -1234567890123456789L,
                    Double = -1.23456789,
                    Decimal = -1234567890.01M,
                    DateTime = DateTime.Parse("01/01/2000 12:34:56"),
                    DateTimeOffset = new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                    TimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                    DateOnly = new DateOnly(2020, 3, 1),
                    TimeOnly = new TimeOnly(12, 30, 45, 123),
                    Single = -1.234F,
                    Boolean = true,
                    Byte = 255,
                    UnsignedInt16 = 1234,
                    UnsignedInt32 = 1234565789U,
                    UnsignedInt64 = 1234567890123456789UL,
                    Character = 'a',
                    SignedByte = -128,
                    Enum64 = Enum64.SomeValue,
                    Enum32 = Enum32.SomeValue,
                    Enum16 = Enum16.SomeValue,
                    Enum8 = Enum8.SomeValue,
                    EnumU64 = EnumU64.SomeValue,
                    EnumU32 = EnumU32.SomeValue,
                    EnumU16 = EnumU16.SomeValue,
                    EnumS8 = EnumS8.SomeValue
                });

            Assert.Equal(1, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            var dt = (await context.Set<NonNullableBackedDataTypes>().Where(ndt => ndt.Id == 101).ToListAsync()).Single();

            var entityType = context.Model.FindEntityType(typeof(NonNullableBackedDataTypes));
            AssertEqualIfMapped(entityType, (short)-1234, () => dt.Int16);
            AssertEqualIfMapped(entityType, -123456789, () => dt.Int32);
            AssertEqualIfMapped(entityType, -1234567890123456789L, () => dt.Int64);
            AssertEqualIfMapped(entityType, -1234567890123456789L, () => dt.Int64);
            AssertEqualIfMapped(entityType, -1.23456789, () => dt.Double);
            AssertEqualIfMapped(entityType, -1234567890.01M, () => dt.Decimal);
            AssertEqualIfMapped(entityType, DateTime.Parse("01/01/2000 12:34:56"), () => dt.DateTime);
            AssertEqualIfMapped(
                entityType, new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                () => dt.DateTimeOffset);
            AssertEqualIfMapped(entityType, new TimeSpan(0, 10, 9, 8, 7), () => dt.TimeSpan);
            AssertEqualIfMapped(entityType, new DateOnly(2020, 3, 1), () => dt.DateOnly);
            AssertEqualIfMapped(entityType, new TimeOnly(12, 30, 45, 123), () => dt.TimeOnly);
            AssertEqualIfMapped(entityType, -1.234F, () => dt.Single);
            AssertEqualIfMapped(entityType, true, () => dt.Boolean);
            AssertEqualIfMapped(entityType, (byte)255, () => dt.Byte);
            AssertEqualIfMapped(entityType, Enum64.SomeValue, () => dt.Enum64);
            AssertEqualIfMapped(entityType, Enum32.SomeValue, () => dt.Enum32);
            AssertEqualIfMapped(entityType, Enum16.SomeValue, () => dt.Enum16);
            AssertEqualIfMapped(entityType, Enum8.SomeValue, () => dt.Enum8);
            AssertEqualIfMapped(entityType, (ushort)1234, () => dt.UnsignedInt16);
            AssertEqualIfMapped(entityType, 1234565789U, () => dt.UnsignedInt32);
            AssertEqualIfMapped(entityType, 1234567890123456789UL, () => dt.UnsignedInt64);
            AssertEqualIfMapped(entityType, 'a', () => dt.Character);
            AssertEqualIfMapped(entityType, (sbyte)-128, () => dt.SignedByte);
            AssertEqualIfMapped(entityType, EnumU64.SomeValue, () => dt.EnumU64);
            AssertEqualIfMapped(entityType, EnumU32.SomeValue, () => dt.EnumU32);
            AssertEqualIfMapped(entityType, EnumU16.SomeValue, () => dt.EnumU16);
            AssertEqualIfMapped(entityType, EnumS8.SomeValue, () => dt.EnumS8);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_read_back_mapped_enum_from_collection_first_or_default()
    {
        using var context = CreateContext();
        var query = from animal in context.Set<Animal>()
                    select new { animal.Id, animal.IdentificationMethods.FirstOrDefault().Method };

        var result = await query.SingleOrDefaultAsync();
        Assert.Equal(IdentificationMethod.EarTag, result.Method);
    }

    [ConditionalFact]
    public virtual async Task Can_read_back_bool_mapped_as_int_through_navigation()
    {
        using var context = CreateContext();
        var query = from animal in context.Set<Animal>()
                    where animal.Details != null
                    select new { animal.Details.BoolField };

        var result = Assert.Single(await query.ToListAsync());
        Assert.True(result.BoolField);
    }

    [ConditionalFact]
    public virtual async Task Can_compare_enum_to_constant()
    {
        using var context = CreateContext();
        var query = await context.Set<AnimalIdentification>()
            .Where(a => a.Method == IdentificationMethod.EarTag)
            .ToListAsync();

        var result = Assert.Single(query);
        Assert.Equal(IdentificationMethod.EarTag, result.Method);
    }

    [ConditionalFact]
    public virtual async Task Can_compare_enum_to_parameter()
    {
        var method = IdentificationMethod.EarTag;
        using var context = CreateContext();
        var query = (await context.Set<AnimalIdentification>()
            .Where(a => a.Method == method)
            .ToListAsync());

        var result = Assert.Single(query);
        Assert.Equal(IdentificationMethod.EarTag, result.Method);
    }

    [ConditionalFact]
    public virtual async Task Object_to_string_conversion()
    {
        using var context = CreateContext();
        var expected = (await context.Set<BuiltInDataTypes>()
                .Where(e => e.Id == 13)
                .ToListAsync())
            .Select(
                b => new
                {
                    Sbyte = b.TestSignedByte.ToString(),
                    Byte = b.TestByte.ToString(),
                    Short = b.TestInt16.ToString(),
                    Ushort = b.TestUnsignedInt16.ToString(),
                    Int = b.TestInt32.ToString(),
                    Uint = b.TestUnsignedInt32.ToString(),
                    Long = b.TestInt64.ToString(),
                    Ulong = b.TestUnsignedInt64.ToString(),
                    Decimal = b.TestDecimal.ToString(),
                    Char = b.TestCharacter.ToString()
                })
            .First();

        Fixture.ListLoggerFactory.Clear();

        var query = await context.Set<BuiltInDataTypes>()
            .Where(e => e.Id == 13)
            .Select(
                b => new
                {
                    Sbyte = b.TestSignedByte.ToString(),
                    Byte = b.TestByte.ToString(),
                    Short = b.TestInt16.ToString(),
                    Ushort = b.TestUnsignedInt16.ToString(),
                    Int = b.TestInt32.ToString(),
                    Uint = b.TestUnsignedInt32.ToString(),
                    Long = b.TestInt64.ToString(),
                    Ulong = b.TestUnsignedInt64.ToString(),
                    Float = b.TestSingle.ToString(),
                    Double = b.TestDouble.ToString(),
                    Decimal = b.TestDecimal.ToString(),
                    Char = b.TestCharacter.ToString(),
                    DateTime = b.TestDateTime.ToString(),
                    DateTimeOffset = b.TestDateTimeOffset.ToString(),
                    TimeSpan = b.TestTimeSpan.ToString(),
                    DateOnly = b.TestDateOnly.ToString(),
                    TimeOnly = b.TestTimeOnly.ToString(),
                })
            .ToListAsync();

        var actual = Assert.Single(query);
        Assert.Equal(expected.Sbyte, actual.Sbyte);
        Assert.Equal(expected.Byte, actual.Byte);
        Assert.Equal(expected.Short, actual.Short);
        Assert.Equal(expected.Ushort, actual.Ushort);
        Assert.Equal(expected.Int, actual.Int);
        Assert.Equal(expected.Uint, actual.Uint);
        Assert.Equal(expected.Long, actual.Long);
        Assert.Equal(expected.Ulong, actual.Ulong);
        Assert.Equal(expected.Decimal, actual.Decimal);
        Assert.Equal(expected.Char, actual.Char);
    }

    [ConditionalFact]
    public virtual async Task Optional_datetime_reading_null_from_database()
    {
        using var context = CreateContext();
        var expected = (await context.Set<DateTimeEnclosure>().ToListAsync())
            .Select(e => new { DT = e.DateTimeOffset == null ? (DateTime?)null : e.DateTimeOffset.Value.DateTime.Date }).ToList();

        var actual = await context.Set<DateTimeEnclosure>()
            .Select(e => new { DT = e.DateTimeOffset == null ? (DateTime?)null : e.DateTimeOffset.Value.DateTime.Date }).ToListAsync();

        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].DT, actual[i].DT);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_query_multiline_string()
    {
        using var context = CreateContext();

        Assert.Equal(Fixture.ReallyLargeString, Assert.Single((await context.Set<StringEnclosure>().ToListAsync())).Value);
    }

    public abstract class BuiltInDataTypesFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => "BuiltInDataTypes";

        public virtual int LongStringLength
            => 9000;

        public virtual string ReallyLargeString
            => string.Join("", Enumerable.Repeat(Environment.NewLine, 1001));

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(
                w => w.Ignore(
                    CoreEventId.MappedEntityTypeIgnoredWarning,
                    CoreEventId.MappedPropertyIgnoredWarning,
                    CoreEventId.MappedNavigationIgnoredWarning));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<BinaryKeyDataType>();
            modelBuilder.Entity<StringKeyDataType>();
            modelBuilder.Entity<BuiltInDataTypes>(
                eb =>
                {
                    eb.HasData(
                        new BuiltInDataTypes
                        {
                            Id = 13,
                            PartitionId = 1,
                            TestInt16 = -1234,
                            TestInt32 = -123456789,
                            TestInt64 = -1234567890123456789L,
                            TestDouble = -1.23456789,
                            TestDecimal = -1234567890.01M,
                            TestDateTime = DateTime.Parse("01/01/2000 12:34:56"),
                            TestDateTimeOffset = new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                            TestTimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                            TestDateOnly = new DateOnly(2020, 3, 1),
                            TestTimeOnly = new TimeOnly(12, 30, 45, 123),
                            TestSingle = -1.234F,
                            TestBoolean = true,
                            TestByte = 255,
                            TestUnsignedInt16 = 1234,
                            TestUnsignedInt32 = 1234565789U,
                            TestUnsignedInt64 = 1234567890123456789UL,
                            TestCharacter = 'a',
                            TestSignedByte = -128,
                            Enum64 = Enum64.SomeValue,
                            Enum32 = Enum32.SomeValue,
                            Enum16 = Enum16.SomeValue,
                            Enum8 = Enum8.SomeValue,
                            EnumU64 = EnumU64.SomeValue,
                            EnumU32 = EnumU32.SomeValue,
                            EnumU16 = EnumU16.SomeValue,
                            EnumS8 = EnumS8.SomeValue
                        });
                    eb.Property(e => e.Id).ValueGeneratedNever();
                });
            modelBuilder.Entity<BuiltInDataTypesShadow>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<BuiltInNullableDataTypes>(
                eb =>
                {
                    eb.HasData(
                        new BuiltInNullableDataTypes
                        {
                            Id = 13,
                            PartitionId = 1,
                            TestNullableInt16 = -1234,
                            TestNullableInt32 = -123456789,
                            TestNullableInt64 = -1234567890123456789L,
                            TestNullableDouble = -1.23456789,
                            TestNullableDecimal = -1234567890.01M,
                            TestNullableDateTimeOffset = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0)),
                            TestNullableTimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                            TestNullableDateOnly = new DateOnly(2020, 3, 1),
                            TestNullableTimeOnly = new TimeOnly(12, 30, 45, 123),
                            TestNullableSingle = -1.234F,
                            TestNullableBoolean = true,
                            TestNullableByte = 255,
                            TestNullableUnsignedInt16 = 1234,
                            TestNullableUnsignedInt32 = 1234565789U,
                            TestNullableUnsignedInt64 = 1234567890123456789UL,
                            TestNullableCharacter = 'a',
                            TestNullableSignedByte = -128,
                            Enum64 = Enum64.SomeValue,
                            Enum32 = Enum32.SomeValue,
                            Enum16 = Enum16.SomeValue,
                            Enum8 = Enum8.SomeValue,
                            EnumU64 = EnumU64.SomeValue,
                            EnumU32 = EnumU32.SomeValue,
                            EnumU16 = EnumU16.SomeValue,
                            EnumS8 = EnumS8.SomeValue
                        });
                    eb.Property(e => e.Id).ValueGeneratedNever();
                });
            modelBuilder.Entity<BuiltInNullableDataTypesShadow>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<BinaryForeignKeyDataType>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<StringForeignKeyDataType>().Property(e => e.Id).ValueGeneratedNever();
            MakeRequired<BuiltInDataTypes>(modelBuilder);
            MakeRequired<BuiltInDataTypesShadow>(modelBuilder);

            modelBuilder.Entity<MaxLengthDataTypes>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.ByteArray5).HasMaxLength(5);
                    b.Property(e => e.String3).HasMaxLength(3);
                    b.Property(e => e.ByteArray9000).HasMaxLength(LongStringLength);
                    b.Property(e => e.String9000).HasMaxLength(LongStringLength);
                    b.Property(e => e.StringUnbounded).HasMaxLength(-1);
                    b.Property(e => e.StringUnbounded).HasMaxLength(LongStringLength);
                });

            modelBuilder.Entity<UnicodeDataTypes>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.StringAnsi).IsUnicode(false);
                    b.Property(e => e.StringAnsi3).HasMaxLength(3).IsUnicode(false);
                    b.Property(e => e.StringAnsi9000).IsUnicode(false).HasMaxLength(LongStringLength);
                    b.Property(e => e.StringUnicode).IsUnicode();
                });

            modelBuilder.Entity<BuiltInDataTypesShadow>(
                b =>
                {
                    foreach (var property in modelBuilder.Entity<BuiltInDataTypes>().Metadata
                                 .GetProperties().Where(p => p.Name != "Id"))
                    {
                        b.Property(property.ClrType, property.Name);
                    }
                });

            modelBuilder.Entity<BuiltInNullableDataTypesShadow>(
                b =>
                {
                    foreach (var property in modelBuilder.Entity<BuiltInNullableDataTypes>().Metadata
                                 .GetProperties().Where(p => p.Name != "Id"))
                    {
                        b.Property(property.ClrType, property.Name);
                    }
                });

            modelBuilder.Entity<EmailTemplate>(
                b =>
                {
                    b.HasData(
                        new EmailTemplate
                        {
                            Id = Guid.Parse("3C56082A-005A-4FFB-A9CF-F5EBD641E07D"),
                            TemplateType = EmailTemplateType.PasswordResetRequest
                        });
                });

            modelBuilder.Entity<ObjectBackedDataTypes>()
                .HasData(
                    new ObjectBackedDataTypes
                    {
                        Id = 13,
                        PartitionId = 1,
                        String = "string",
                        Bytes = [4, 20],
                        Int16 = -1234,
                        Int32 = -123456789,
                        Int64 = -1234567890123456789L,
                        Double = -1.23456789,
                        Decimal = -1234567890.01M,
                        DateTime = new DateTime(1973, 9, 3),
                        DateTimeOffset = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0)),
                        TimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                        DateOnly = new DateOnly(2020, 3, 1),
                        TimeOnly = new TimeOnly(12, 30, 45, 123),
                        Single = -1.234F,
                        Boolean = true,
                        Byte = 255,
                        UnsignedInt16 = 1234,
                        UnsignedInt32 = 1234565789U,
                        UnsignedInt64 = 1234567890123456789UL,
                        Character = 'a',
                        SignedByte = -128,
                        Enum64 = Enum64.SomeValue,
                        Enum32 = Enum32.SomeValue,
                        Enum16 = Enum16.SomeValue,
                        Enum8 = Enum8.SomeValue,
                        EnumU64 = EnumU64.SomeValue,
                        EnumU32 = EnumU32.SomeValue,
                        EnumU16 = EnumU16.SomeValue,
                        EnumS8 = EnumS8.SomeValue
                    });

            modelBuilder.Entity<NullableBackedDataTypes>()
                .HasData(
                    new NullableBackedDataTypes
                    {
                        Id = 13,
                        PartitionId = 1,
                        Int16 = -1234,
                        Int32 = -123456789,
                        Int64 = -1234567890123456789L,
                        Double = -1.23456789,
                        Decimal = -1234567890.01M,
                        DateTime = new DateTime(1973, 9, 3),
                        DateTimeOffset = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0)),
                        TimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                        DateOnly = new DateOnly(2020, 3, 1),
                        TimeOnly = new TimeOnly(12, 30, 45, 123),
                        Single = -1.234F,
                        Boolean = true,
                        Byte = 255,
                        UnsignedInt16 = 1234,
                        UnsignedInt32 = 1234565789U,
                        UnsignedInt64 = 1234567890123456789UL,
                        Character = 'a',
                        SignedByte = -128,
                        Enum64 = Enum64.SomeValue,
                        Enum32 = Enum32.SomeValue,
                        Enum16 = Enum16.SomeValue,
                        Enum8 = Enum8.SomeValue,
                        EnumU64 = EnumU64.SomeValue,
                        EnumU32 = EnumU32.SomeValue,
                        EnumU16 = EnumU16.SomeValue,
                        EnumS8 = EnumS8.SomeValue
                    });

            modelBuilder.Entity<NonNullableBackedDataTypes>()
                .HasData(
                    new NonNullableBackedDataTypes
                    {
                        Id = 13,
                        PartitionId = 1,
                        Int16 = -1234,
                        Int32 = -123456789,
                        Int64 = -1234567890123456789L,
                        Double = -1.23456789,
                        Decimal = -1234567890.01M,
                        DateTime = new DateTime(1973, 9, 3),
                        DateTimeOffset = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0)),
                        TimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                        DateOnly = new DateOnly(2020, 3, 1),
                        TimeOnly = new TimeOnly(12, 30, 45, 123),
                        Single = -1.234F,
                        Boolean = true,
                        Byte = 255,
                        UnsignedInt16 = 1234,
                        UnsignedInt32 = 1234565789U,
                        UnsignedInt64 = 1234567890123456789UL,
                        Character = 'a',
                        SignedByte = -128,
                        Enum64 = Enum64.SomeValue,
                        Enum32 = Enum32.SomeValue,
                        Enum16 = Enum16.SomeValue,
                        Enum8 = Enum8.SomeValue,
                        EnumU64 = EnumU64.SomeValue,
                        EnumU32 = EnumU32.SomeValue,
                        EnumU16 = EnumU16.SomeValue,
                        EnumS8 = EnumS8.SomeValue
                    });

            modelBuilder.Entity<Animal>()
                .HasData(
                    new Animal { Id = 1 });

            modelBuilder.Entity<AnimalDetails>()
                .HasData(
                    new AnimalDetails
                    {
                        Id = 1,
                        AnimalId = 1,
                        BoolField = true
                    });

            modelBuilder.Entity<AnimalIdentification>()
                .HasData(
                    new AnimalIdentification
                    {
                        Id = 1,
                        AnimalId = 1,
                        Method = IdentificationMethod.EarTag
                    });

            modelBuilder.Entity<DateTimeEnclosure>()
                .HasData(
                    new DateTimeEnclosure { Id = 1, DateTimeOffset = new DateTimeOffset(2020, 3, 12, 1, 1, 1, new TimeSpan(3, 0, 0)) },
                    new DateTimeEnclosure { Id = 2 });

            modelBuilder.Entity<StringEnclosure>()
                .HasData(
                    new StringEnclosure { Id = 1, Value = ReallyLargeString });
        }

        protected static void MakeRequired<TEntity>(ModelBuilder modelBuilder)
            where TEntity : class
        {
            foreach (var property in modelBuilder.Entity<TEntity>().Metadata.GetDeclaredProperties())
            {
                property.IsNullable = false;
            }
        }

        public abstract bool StrictEquality { get; }

        public virtual int IntegerPrecision
            => 19;

        public abstract bool SupportsAnsi { get; }

        public abstract bool SupportsUnicodeToAnsiConversion { get; }

        public abstract bool SupportsLargeStringComparisons { get; }

        public abstract bool SupportsBinaryKeys { get; }

        public abstract bool SupportsDecimalComparisons { get; }

        public abstract DateTime DefaultDateTime { get; }

        public abstract bool PreservesDateTimeKind { get; }
    }

    protected class BuiltInDataTypesBase
    {
        public int Id { get; set; }
    }

    protected class BuiltInDataTypes : BuiltInDataTypesBase
    {
        public int PartitionId { get; set; }
        public short TestInt16 { get; set; }
        public int TestInt32 { get; set; }
        public long TestInt64 { get; set; }
        public double TestDouble { get; set; }
        public decimal TestDecimal { get; set; }
        public DateTime TestDateTime { get; set; }
        public DateTimeOffset TestDateTimeOffset { get; set; }
        public TimeSpan TestTimeSpan { get; set; }
        public DateOnly TestDateOnly { get; set; }
        public TimeOnly TestTimeOnly { get; set; }
        public float TestSingle { get; set; }
        public bool TestBoolean { get; set; }
        public byte TestByte { get; set; }
        public ushort TestUnsignedInt16 { get; set; }
        public uint TestUnsignedInt32 { get; set; }
        public ulong TestUnsignedInt64 { get; set; }
        public char TestCharacter { get; set; }
        public sbyte TestSignedByte { get; set; }
        public Enum64 Enum64 { get; set; }
        public Enum32 Enum32 { get; set; }
        public Enum16 Enum16 { get; set; }
        public Enum8 Enum8 { get; set; }
        public EnumU64 EnumU64 { get; set; }
        public EnumU32 EnumU32 { get; set; }
        public EnumU16 EnumU16 { get; set; }
        public EnumS8 EnumS8 { get; set; }
    }

    protected class BuiltInDataTypesShadow : BuiltInDataTypesBase;

    protected enum Enum64 : long
    {
        SomeValue = 1
    }

    protected enum Enum32
    {
        SomeValue = 1
    }

    protected enum Enum16 : short
    {
        SomeValue = 1
    }

    protected enum Enum8 : byte
    {
        SomeValue = 1
    }

    protected enum EnumU64 : ulong
    {
        SomeValue = 1234567890123456789UL
    }

    protected enum EnumU32 : uint
    {
        SomeValue = uint.MaxValue
    }

    protected enum EnumU16 : ushort
    {
        SomeValue = ushort.MaxValue
    }

    protected enum EnumS8 : sbyte
    {
        SomeValue = sbyte.MinValue
    }

    protected class MaxLengthDataTypes
    {
        public int Id { get; set; }
        public string String3 { get; set; }
        public byte[] ByteArray5 { get; set; }
        public string String9000 { get; set; }
        public string StringUnbounded { get; set; }
        public byte[] ByteArray9000 { get; set; }
    }

    protected class UnicodeDataTypes
    {
        public int Id { get; set; }
        public string StringDefault { get; set; }
        public string StringAnsi { get; set; }
        public string StringAnsi3 { get; set; }
        public string StringAnsi9000 { get; set; }
        public string StringUnicode { get; set; }
    }

    protected class BinaryKeyDataType
    {
        public byte[] Id { get; set; }

        public string Ex { get; set; }

        public ICollection<BinaryForeignKeyDataType> Dependents { get; set; }
    }

    protected class BinaryForeignKeyDataType
    {
        public int Id { get; set; }
        public byte[] BinaryKeyDataTypeId { get; set; }

        public BinaryKeyDataType Principal { get; set; }
    }

    protected class StringKeyDataType
    {
        public string Id { get; set; }

        public ICollection<StringForeignKeyDataType> Dependents { get; set; }
    }

    protected class StringForeignKeyDataType
    {
        public int Id { get; set; }
        public string StringKeyDataTypeId { get; set; }

        public StringKeyDataType Principal { get; set; }
    }

    protected class BuiltInNullableDataTypesBase
    {
        public int Id { get; set; }
    }

    protected class BuiltInNullableDataTypes : BuiltInNullableDataTypesBase
    {
        public int PartitionId { get; set; }
        public string TestString { get; set; }
        public byte[] TestByteArray { get; set; }
        public short? TestNullableInt16 { get; set; }
        public int? TestNullableInt32 { get; set; }
        public long? TestNullableInt64 { get; set; }
        public double? TestNullableDouble { get; set; }
        public decimal? TestNullableDecimal { get; set; }
        public DateTime? TestNullableDateTime { get; set; }
        public DateTimeOffset? TestNullableDateTimeOffset { get; set; }
        public TimeSpan? TestNullableTimeSpan { get; set; }
        public DateOnly? TestNullableDateOnly { get; set; }
        public TimeOnly? TestNullableTimeOnly { get; set; }
        public float? TestNullableSingle { get; set; }
        public bool? TestNullableBoolean { get; set; }
        public byte? TestNullableByte { get; set; }
        public ushort? TestNullableUnsignedInt16 { get; set; }
        public uint? TestNullableUnsignedInt32 { get; set; }
        public ulong? TestNullableUnsignedInt64 { get; set; }
        public char? TestNullableCharacter { get; set; }
        public sbyte? TestNullableSignedByte { get; set; }

        // ReSharper disable MemberHidesStaticFromOuterClass
        public Enum64? Enum64 { get; set; }
        public Enum32? Enum32 { get; set; }
        public Enum16? Enum16 { get; set; }
        public Enum8? Enum8 { get; set; }
        public EnumU64? EnumU64 { get; set; }
        public EnumU32? EnumU32 { get; set; }
        public EnumU16? EnumU16 { get; set; }

        public EnumS8? EnumS8 { get; set; }
        // ReSharper restore MemberHidesStaticFromOuterClass
    }

    protected class BuiltInNullableDataTypesShadow : BuiltInNullableDataTypesBase;

    protected class EmailTemplate
    {
        public Guid Id { get; set; }
        public EmailTemplateType TemplateType { get; set; }
    }

    protected enum EmailTemplateType
    {
        PasswordResetRequest = 0,
        EmailConfirmation = 1
    }

    protected class EmailTemplateDto
    {
        public Guid Id { get; set; }
        public EmailTemplateTypeDto TemplateType { get; set; }
    }

    protected enum EmailTemplateTypeDto
    {
        PasswordResetRequest = 0,
        EmailConfirmation = 1
    }

    protected class ObjectBackedDataTypes
    {
        private object _string;
        private object _bytes;
        private object _int16;
        private object _int32;
        private object _int64;
        private object _double;
        private object _decimal;
        private object _dateTime;
        private object _dateTimeOffset;
        private object _timeSpan;
        private object _dateOnly;
        private object _timeOnly;
        private object _single;
        private object _boolean;
        private object _byte;
        private object _unsignedInt16;
        private object _unsignedInt32;
        private object _unsignedInt64;
        private object _character;
        private object _signedByte;
        private object _enum64;
        private object _enum32;
        private object _enum16;
        private object _enum8;
        private object _enumU64;
        private object _enumU32;
        private object _enumU16;
        private object _enumS8;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int PartitionId { get; set; }

        public string String
        {
            get => (string)_string;
            set => _string = value;
        }

        public byte[] Bytes
        {
            get => (byte[])_bytes;
            set => _bytes = value;
        }

        public short Int16
        {
            get => (short)_int16;
            set => _int16 = value;
        }

        public int Int32
        {
            get => (int)_int32;
            set => _int32 = value;
        }

        public long Int64
        {
            get => (long)_int64;
            set => _int64 = value;
        }

        public double Double
        {
            get => (double)_double;
            set => _double = value;
        }

        public decimal Decimal
        {
            get => (decimal)_decimal;
            set => _decimal = value;
        }

        public DateTime DateTime
        {
            get => (DateTime)_dateTime;
            set => _dateTime = value;
        }

        public DateTimeOffset DateTimeOffset
        {
            get => (DateTimeOffset)_dateTimeOffset;
            set => _dateTimeOffset = value;
        }

        public TimeSpan TimeSpan
        {
            get => (TimeSpan)_timeSpan;
            set => _timeSpan = value;
        }

        public DateOnly DateOnly
        {
            get => (DateOnly)_dateOnly;
            set => _dateOnly = value;
        }

        public TimeOnly TimeOnly
        {
            get => (TimeOnly)_timeOnly;
            set => _timeOnly = value;
        }

        public float Single
        {
            get => (float)_single;
            set => _single = value;
        }

        public bool Boolean
        {
            get => (bool)_boolean;
            set => _boolean = value;
        }

        public byte Byte
        {
            get => (byte)_byte;
            set => _byte = value;
        }

        public ushort UnsignedInt16
        {
            get => (ushort)_unsignedInt16;
            set => _unsignedInt16 = value;
        }

        public uint UnsignedInt32
        {
            get => (uint)_unsignedInt32;
            set => _unsignedInt32 = value;
        }

        public ulong UnsignedInt64
        {
            get => (ulong)_unsignedInt64;
            set => _unsignedInt64 = value;
        }

        public char Character
        {
            get => (char)_character;
            set => _character = value;
        }

        public sbyte SignedByte
        {
            get => (sbyte)_signedByte;
            set => _signedByte = value;
        }

        public Enum64 Enum64
        {
            get => (Enum64)_enum64;
            set => _enum64 = value;
        }

        public Enum32 Enum32
        {
            get => (Enum32)_enum32;
            set => _enum32 = value;
        }

        public Enum16 Enum16
        {
            get => (Enum16)_enum16;
            set => _enum16 = value;
        }

        public Enum8 Enum8
        {
            get => (Enum8)_enum8;
            set => _enum8 = value;
        }

        public EnumU64 EnumU64
        {
            get => (EnumU64)_enumU64;
            set => _enumU64 = value;
        }

        public EnumU32 EnumU32
        {
            get => (EnumU32)_enumU32;
            set => _enumU32 = value;
        }

        public EnumU16 EnumU16
        {
            get => (EnumU16)_enumU16;
            set => _enumU16 = value;
        }

        public EnumS8 EnumS8
        {
            get => (EnumS8)_enumS8;
            set => _enumS8 = value;
        }
    }

    protected class NullableBackedDataTypes
    {
        private short? _int16;
        private int? _int32;
        private long? _int64;
        private double? _double;
        private decimal? _decimal;
        private DateTime? _dateTime;
        private DateTimeOffset? _dateTimeOffset;
        private TimeSpan? _timeSpan;
        private DateOnly? _dateOnly;
        private TimeOnly? _timeOnly;
        private float? _single;
        private bool? _boolean;
        private byte? _byte;
        private ushort? _unsignedInt16;
        private uint? _unsignedInt32;
        private ulong? _unsignedInt64;
        private char? _character;
        private sbyte? _signedByte;
        private Enum64? _enum64;
        private Enum32? _enum32;
        private Enum16? _enum16;
        private Enum8? _enum8;
        private EnumU64? _enumU64;
        private EnumU32? _enumU32;
        private EnumU16? _enumU16;
        private EnumS8? _enumS8;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int PartitionId { get; set; }

        public short Int16
        {
            get => (short)_int16;
            set => _int16 = value;
        }

        public int Int32
        {
            get => (int)_int32;
            set => _int32 = value;
        }

        public long Int64
        {
            get => (long)_int64;
            set => _int64 = value;
        }

        public double Double
        {
            get => (double)_double;
            set => _double = value;
        }

        public decimal Decimal
        {
            get => (decimal)_decimal;
            set => _decimal = value;
        }

        public DateTime DateTime
        {
            get => (DateTime)_dateTime;
            set => _dateTime = value;
        }

        public DateTimeOffset DateTimeOffset
        {
            get => (DateTimeOffset)_dateTimeOffset;
            set => _dateTimeOffset = value;
        }

        public TimeSpan TimeSpan
        {
            get => (TimeSpan)_timeSpan;
            set => _timeSpan = value;
        }

        public DateOnly DateOnly
        {
            get => (DateOnly)_dateOnly;
            set => _dateOnly = value;
        }

        public TimeOnly TimeOnly
        {
            get => (TimeOnly)_timeOnly;
            set => _timeOnly = value;
        }

        public float Single
        {
            get => (float)_single;
            set => _single = value;
        }

        public bool Boolean
        {
            get => (bool)_boolean;
            set => _boolean = value;
        }

        public byte Byte
        {
            get => (byte)_byte;
            set => _byte = value;
        }

        public ushort UnsignedInt16
        {
            get => (ushort)_unsignedInt16;
            set => _unsignedInt16 = value;
        }

        public uint UnsignedInt32
        {
            get => (uint)_unsignedInt32;
            set => _unsignedInt32 = value;
        }

        public ulong UnsignedInt64
        {
            get => (ulong)_unsignedInt64;
            set => _unsignedInt64 = value;
        }

        public char Character
        {
            get => (char)_character;
            set => _character = value;
        }

        public sbyte SignedByte
        {
            get => (sbyte)_signedByte;
            set => _signedByte = value;
        }

        public Enum64 Enum64
        {
            get => (Enum64)_enum64;
            set => _enum64 = value;
        }

        public Enum32 Enum32
        {
            get => (Enum32)_enum32;
            set => _enum32 = value;
        }

        public Enum16 Enum16
        {
            get => (Enum16)_enum16;
            set => _enum16 = value;
        }

        public Enum8 Enum8
        {
            get => (Enum8)_enum8;
            set => _enum8 = value;
        }

        public EnumU64 EnumU64
        {
            get => (EnumU64)_enumU64;
            set => _enumU64 = value;
        }

        public EnumU32 EnumU32
        {
            get => (EnumU32)_enumU32;
            set => _enumU32 = value;
        }

        public EnumU16 EnumU16
        {
            get => (EnumU16)_enumU16;
            set => _enumU16 = value;
        }

        public EnumS8 EnumS8
        {
            get => (EnumS8)_enumS8;
            set => _enumS8 = value;
        }
    }

    protected class NonNullableBackedDataTypes
    {
        private short _int16;
        private int _int32;
        private long _int64;
        private double _double;
        private decimal _decimal;
        private DateTime _dateTime;
        private DateTimeOffset _dateTimeOffset;
        private TimeSpan _timeSpan;
        private DateOnly _dateOnly;
        private TimeOnly _timeOnly;
        private float _single;
        private bool _boolean;
        private byte _byte;
        private ushort _unsignedInt16;
        private uint _unsignedInt32;
        private ulong _unsignedInt64;
        private char _character;
        private sbyte _signedByte;
        private Enum64 _enum64;
        private Enum32 _enum32;
        private Enum16 _enum16;
        private Enum8 _enum8;
        private EnumU64 _enumU64;
        private EnumU32 _enumU32;
        private EnumU16 _enumU16;
        private EnumS8 _enumS8;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int PartitionId { get; set; }

        public short? Int16
        {
            get => _int16;
            set => _int16 = (short)value;
        }

        public int? Int32
        {
            get => _int32;
            set => _int32 = (int)value;
        }

        public long? Int64
        {
            get => _int64;
            set => _int64 = (long)value;
        }

        public double? Double
        {
            get => _double;
            set => _double = (double)value;
        }

        public decimal? Decimal
        {
            get => _decimal;
            set => _decimal = (decimal)value;
        }

        public DateTime? DateTime
        {
            get => _dateTime;
            set => _dateTime = (DateTime)value;
        }

        public DateTimeOffset? DateTimeOffset
        {
            get => _dateTimeOffset;
            set => _dateTimeOffset = (DateTimeOffset)value;
        }

        public TimeSpan? TimeSpan
        {
            get => _timeSpan;
            set => _timeSpan = (TimeSpan)value;
        }

        public DateOnly? DateOnly
        {
            get => _dateOnly;
            set => _dateOnly = (DateOnly)value;
        }

        public TimeOnly? TimeOnly
        {
            get => _timeOnly;
            set => _timeOnly = (TimeOnly)value;
        }

        public float? Single
        {
            get => _single;
            set => _single = (float)value;
        }

        public bool? Boolean
        {
            get => _boolean;
            set => _boolean = (bool)value;
        }

        public byte? Byte
        {
            get => _byte;
            set => _byte = (byte)value;
        }

        public ushort? UnsignedInt16
        {
            get => _unsignedInt16;
            set => _unsignedInt16 = (ushort)value;
        }

        public uint? UnsignedInt32
        {
            get => _unsignedInt32;
            set => _unsignedInt32 = (uint)value;
        }

        public ulong? UnsignedInt64
        {
            get => _unsignedInt64;
            set => _unsignedInt64 = (ulong)value;
        }

        public char? Character
        {
            get => _character;
            set => _character = (char)value;
        }

        public sbyte? SignedByte
        {
            get => _signedByte;
            set => _signedByte = (sbyte)value;
        }

        public Enum64? Enum64
        {
            get => _enum64;
            set => _enum64 = (Enum64)value;
        }

        public Enum32? Enum32
        {
            get => _enum32;
            set => _enum32 = (Enum32)value;
        }

        public Enum16? Enum16
        {
            get => _enum16;
            set => _enum16 = (Enum16)value;
        }

        public Enum8? Enum8
        {
            get => _enum8;
            set => _enum8 = (Enum8)value;
        }

        public EnumU64? EnumU64
        {
            get => _enumU64;
            set => _enumU64 = (EnumU64)value;
        }

        public EnumU32? EnumU32
        {
            get => _enumU32;
            set => _enumU32 = (EnumU32)value;
        }

        public EnumU16? EnumU16
        {
            get => _enumU16;
            set => _enumU16 = (EnumU16)value;
        }

        public EnumS8? EnumS8
        {
            get => _enumS8;
            set => _enumS8 = (EnumS8)value;
        }
    }

    protected class Animal
    {
        public int Id { get; set; }
        public ICollection<AnimalIdentification> IdentificationMethods { get; set; }
        public AnimalDetails Details { get; set; }
    }

    protected class AnimalDetails
    {
        public int Id { get; set; }
        public int? AnimalId { get; set; }

        [Column(TypeName = "int")]
        public bool BoolField { get; set; }
    }

    protected class AnimalIdentification
    {
        public int Id { get; set; }
        public int AnimalId { get; set; }
        public IdentificationMethod Method { get; set; }
    }

    protected enum IdentificationMethod
    {
        Notch,
        EarTag,
        Rfid
    }

    protected class DateTimeEnclosure
    {
        public int Id { get; set; }
        public DateTimeOffset? DateTimeOffset { get; set; }
    }

    protected class StringEnclosure
    {
        public int Id { get; set; }

        public string Value { get; set; }
    }
}
