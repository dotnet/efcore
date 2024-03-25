// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;

// ReSharper disable StaticMemberInGenericType
namespace Microsoft.EntityFrameworkCore;

public abstract class ValueConvertersEndToEndTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : ValueConvertersEndToEndTestBase<TFixture>.ValueConvertersEndToEndFixtureBase, new()
{
    protected ValueConvertersEndToEndTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    private static readonly DateTimeOffset _dateTimeOffset1 = new(1973, 9, 3, 12, 10, 0, new TimeSpan(7, 0, 0));
    private static readonly DateTimeOffset _dateTimeOffset2 = new(1973, 9, 3, 12, 10, 0, new TimeSpan(8, 0, 0));
    private static readonly DateTime _dateTime1 = new(1973, 9, 3, 12, 10, 0);
    private static readonly DateTime _dateTime2 = new(1973, 9, 3, 12, 10, 1);
    private static readonly DateOnly _dateOnly1 = new(1973, 9, 3);
    private static readonly DateOnly _dateOnly2 = new(1973, 9, 4);
    private static readonly IPAddress _ipAddress1 = IPAddress.Parse("127.0.0.1");
    private static readonly IPAddress _ipAddress2 = IPAddress.Parse("127.0.0.2");
    private static readonly PhysicalAddress _physicalAddress1 = PhysicalAddress.Parse("1D4E55D69273");
    private static readonly PhysicalAddress _physicalAddress2 = PhysicalAddress.Parse("1D4E55D69274");
    private static readonly TimeSpan _timeSpan1 = new(7, 0, 0);
    private static readonly TimeSpan _timeSpan2 = new(8, 0, 0);
    private static readonly Uri _uri1 = new("http://localhost/");
    private static readonly Uri _uri2 = new("http://microsoft.com/");
    private static readonly string _dateTimeFormat = @"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFF";
    private static readonly string _dateOnlyFormat = @"yyyy\-MM\-dd";
    private static readonly string _dateTimeOffsetFormat = @"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFFzzz";

    protected static Dictionary<Type, object?[]> TestValues = new()
    {
        { typeof(bool), [true, false, true, false] },
        { typeof(int), [77, 0, 78, 0] },
        { typeof(char), ['A', 'B', 'C', 'D'] },
        { typeof(byte[]), [new byte[] { 1 }, new byte[] { 2 }, new byte[] { 3 }, new byte[] { 4 }] },
        { typeof(DateTimeOffset), [_dateTimeOffset1, _dateTimeOffset2, _dateTimeOffset1, _dateTimeOffset2] },
        { typeof(DateTime), [_dateTime1, _dateTime2, _dateTime1, _dateTime2] },
        { typeof(DateOnly), [_dateOnly1, _dateOnly2, _dateOnly1, _dateOnly2] },
        { typeof(TheExperience), [TheExperience.Jimi, TheExperience.Mitch, TheExperience.Noel, TheExperience.Jimi] },
        { typeof(Guid), [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()] },
        { typeof(IPAddress), [_ipAddress1, _ipAddress2, _ipAddress1, _ipAddress2] },
        { typeof(ulong), [(ulong)77, (ulong)0, (ulong)78, (ulong)0] },
        { typeof(sbyte), [(sbyte)-77, (sbyte)0, (sbyte)78, (sbyte)0] },
        { typeof(PhysicalAddress), [_physicalAddress1, _physicalAddress2, _physicalAddress1, _physicalAddress2] },
        { typeof(TimeSpan), [_timeSpan1, _timeSpan2, _timeSpan1, _timeSpan2] },
        { typeof(Uri), [_uri1, _uri2, _uri1, _uri2] },
        {
            typeof(List<int>), [
                new List<int>
                {
                    47,
                    48,
                    47,
                    46
                },
                new List<int>
                {
                    57,
                    58,
                    57,
                    56
                },
                new List<int>
                {
                    67,
                    68,
                    67,
                    66
                },
                new List<int>
                {
                    77,
                    78,
                    77,
                    76
                }
            ]
        },
        {
            typeof(IEnumerable<int>), [
                new List<int>
                {
                    47,
                    48,
                    47,
                    46
                },
                new List<int>
                {
                    57,
                    58,
                    57,
                    56
                },
                new List<int>
                {
                    67,
                    68,
                    67,
                    66
                },
                new List<int>
                {
                    77,
                    78,
                    77,
                    76
                }
            ]
        },
    };

    protected static Dictionary<Type, object?[]> StringTestValues = new()
    {
        { typeof(bool), ["True", "False", "True", "False"] },
        { typeof(char), ["A", "B", "C", "D"] },
        { typeof(byte[]), ["", "", "", ""] },
        {
            typeof(DateTimeOffset), [
                _dateTimeOffset1.ToString(_dateTimeOffsetFormat),
                _dateTimeOffset2.ToString(_dateTimeOffsetFormat),
                _dateTimeOffset1.ToString(_dateTimeOffsetFormat),
                _dateTimeOffset2.ToString(_dateTimeOffsetFormat)
            ]
        },
        {
            typeof(DateTime), [
                _dateTime1.ToString(_dateTimeFormat),
                _dateTime2.ToString(_dateTimeFormat),
                _dateTime1.ToString(_dateTimeFormat),
                _dateTime2.ToString(_dateTimeFormat)
            ]
        },
        {
            typeof(DateOnly), [
                _dateOnly1.ToString(_dateOnlyFormat),
                _dateOnly2.ToString(_dateOnlyFormat),
                _dateOnly1.ToString(_dateOnlyFormat),
                _dateOnly2.ToString(_dateOnlyFormat)
            ]
        },
        { typeof(string), ["A", "<null>", "C", "<null>"] },
        {
            typeof(TheExperience), [
                nameof(TheExperience.Jimi), nameof(TheExperience.Mitch), nameof(TheExperience.Noel), nameof(TheExperience.Jimi)
            ]
        },
        { typeof(Guid), [Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString()] },
        { typeof(ulong), ["77", "0", "78", "0"] },
        { typeof(sbyte), ["-77", "75", "-78", "0"] },
        { typeof(byte), ["77", "75", "78", "0"] },
        { typeof(TimeSpan), [_timeSpan1.ToString(), _timeSpan2.ToString(), _timeSpan1.ToString(), _timeSpan2.ToString()] },
    };

    [ConditionalTheory]
    [InlineData(new[] { 0, 1, 2, 3 })]
    [InlineData(new[] { 3, 2, 1, 0 })]
    [InlineData(new[] { 0, 2, 0, 2 })]
    public virtual async Task Can_insert_and_read_back_with_conversions(int[] valueOrder)
    {
        var id = Guid.Empty;

        using (var context = CreateContext())
        {
            var entity = new ConvertingEntity();

            SetPropertyValues(context, entity, valueOrder[0], -1);

            context.Add(entity);
            await context.SaveChangesAsync();

            id = entity.Id;
        }

        using (var context = CreateContext())
        {
            SetPropertyValues(context, await context.Set<ConvertingEntity>().SingleAsync(e => e.Id == id), valueOrder[1], valueOrder[0]);
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            SetPropertyValues(context, await context.Set<ConvertingEntity>().SingleAsync(e => e.Id == id), valueOrder[2], valueOrder[1]);
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            SetPropertyValues(context, await context.Set<ConvertingEntity>().SingleAsync(e => e.Id == id), valueOrder[3], valueOrder[2]);
            await context.SaveChangesAsync();
        }
    }

    private static void SetPropertyValues(DbContext context, ConvertingEntity entity, int valueIndex, int previousValueIndex)
    {
        var entry = context.Entry(entity);
        foreach (var property in context.Model.FindEntityType(
                     entity.GetType())!.GetProperties().Where(p => !p.IsPrimaryKey() && !p.IsShadowProperty()))
        {
            var testValues = (property.ClrType == typeof(string)
                ? StringTestValues[property.GetValueConverter()!.ProviderClrType.UnwrapNullableType()]
                : TestValues[property.ClrType.UnwrapNullableType()]).ToArray();

            if (property.Name.StartsWith("Null", StringComparison.Ordinal))
            {
                testValues[1] = null;
                testValues[3] = null;
            }

            var propertyEntry = entry.Property(property);

            if (previousValueIndex >= 0
                && property.FindAnnotation("Relational:DefaultValue") == null)
            {
                Assert.Equal(testValues[previousValueIndex], propertyEntry.CurrentValue);
            }

            if (valueIndex < testValues.Length)
            {
                propertyEntry.CurrentValue = testValues[valueIndex];
            }

            Assert.Equal(testValues[valueIndex], propertyEntry.CurrentValue);
        }
    }

    [ConditionalFact]
    protected void Convert_string_nulls_to_string_non_nulls_in_provider()
    {
        var converter = new NullStringToNonNullStringConverter().ConvertToProviderExpression.Compile();

        Assert.Equal("A", converter("A"));
        Assert.Equal("<null>", converter(null));
        Assert.Equal("<null>", converter("<null>"));
        Assert.Equal("", converter(""));
    }

    [ConditionalFact]
    protected void Convert_string_nulls_to_string_non_nulls_in_provider_object()
    {
        var converter = new NullStringToNonNullStringConverter().ConvertToProvider;

        Assert.Equal("A", converter("A"));
        Assert.Equal("<null>", converter(null));
        Assert.Equal("<null>", converter("<null>"));
        Assert.Equal("", converter(""));
    }

    [ConditionalFact]
    protected void Convert_string_nulls_to_string_non_nulls_in_app()
    {
        var converter = new NonNullStringToNullStringConverter().ConvertFromProviderExpression.Compile();

        Assert.Equal("A", converter("A"));
        Assert.Equal("<null>", converter(null));
        Assert.Equal("<null>", converter("<null>"));
        Assert.Equal("", converter(""));
    }

    [ConditionalFact]
    protected void Convert_string_nulls_to_string_non_nulls_in_app_object()
    {
        var converter = new NonNullStringToNullStringConverter().ConvertFromProvider;

        Assert.Equal("A", converter("A"));
        Assert.Equal("<null>", converter(null));
        Assert.Equal("<null>", converter("<null>"));
        Assert.Equal("", converter(""));
    }

    [ConditionalFact]
    protected void Convert_string_non_nulls_to_string_nulls_in_provider()
    {
        var converter = new NonNullStringToNullStringConverter().ConvertToProviderExpression.Compile();

        Assert.Equal("A", converter("A"));
        Assert.Null(converter("<null>"));
        Assert.Equal("", converter(""));
    }

    [ConditionalFact]
    protected void Convert_string_non_nulls_to_string_nulls_in_provider_object()
    {
        var converter = new NonNullStringToNullStringConverter().ConvertToProvider;

        Assert.Equal("A", converter("A"));
        Assert.Null(converter("<null>"));
        Assert.Equal("", converter(""));
    }

    [ConditionalFact]
    protected void Convert_string_non_nulls_to_string_nulls_in_app()
    {
        var converter = new NullStringToNonNullStringConverter().ConvertFromProviderExpression.Compile();

        Assert.Equal("A", converter("A"));
        Assert.Null(converter("<null>"));
        Assert.Equal("", converter(""));
    }

    [ConditionalFact]
    protected void Convert_string_non_nulls_to_string_nulls_in_app_object()
    {
        var converter = new NullStringToNonNullStringConverter().ConvertFromProvider;

        Assert.Equal("A", converter("A"));
        Assert.Null(converter("<null>"));
        Assert.Equal("", converter(""));
    }

    [ConditionalFact]
    protected void Convert_int_nulls_to_string_non_nulls_in_provider()
    {
        var converter = new NullIntToNonNullStringConverter().ConvertToProviderExpression.Compile();

        Assert.Equal("0", converter(0));
        Assert.Equal("1", converter(1));
        Assert.Equal("<null>", converter(null));
    }

    [ConditionalFact]
    protected void Convert_int_nulls_to_string_nulls_in_provider()
    {
        var converter = new NullIntToNullStringConverter().ConvertToProviderExpression.Compile();

        Assert.Equal("0", converter(0));
        Assert.Equal("1", converter(1));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    protected void Convert_int_non_nulls_to_string_non_nulls_in_provider()
    {
        var converter = new NonNullIntToNonNullStringConverter().ConvertToProviderExpression.Compile();

        Assert.Equal("0", converter(0));
        Assert.Equal("1", converter(1));
    }

    [ConditionalFact]
    protected void Convert_int_non_nulls_to_string_nulls_in_provider()
    {
        var converter = new NonNullIntToNullStringConverter().ConvertToProviderExpression.Compile();

        Assert.Equal("0", converter(0));
        Assert.Equal("1", converter(1));
    }

    [ConditionalFact]
    protected void Convert_string_nulls_to_int_non_nulls_in_app()
    {
        var converter = new NonNullIntToNullStringConverter().ConvertFromProviderExpression.Compile();

        Assert.Equal(0, converter("0"));
        Assert.Equal(1, converter("1"));
        Assert.Equal(0, converter(null));
    }

    [ConditionalFact]
    protected void Convert_string_nulls_to_int_nulls_in_app()
    {
        var converter = new NullIntToNullStringConverter().ConvertFromProviderExpression.Compile();

        Assert.Equal(0, converter("0"));
        Assert.Equal(1, converter("1"));
        Assert.Null(converter("<null>"));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    protected void Convert_string_non_nulls_to_int_non_nulls_in_app()
    {
        var converter = new NonNullIntToNonNullStringConverter().ConvertFromProviderExpression.Compile();

        Assert.Equal(0, converter("0"));
        Assert.Equal(1, converter("1"));
        Assert.Equal(0, converter("<null>"));
    }

    [ConditionalFact]
    protected void Convert_string_non_nulls_to_int_nulls_in_app()
    {
        var converter = new NullIntToNonNullStringConverter().ConvertFromProviderExpression.Compile();

        Assert.Equal(0, converter("0"));
        Assert.Equal(1, converter("1"));
        Assert.Null(converter("<null>"));
    }

    [ConditionalFact]
    protected void Convert_int_nulls_to_string_non_nulls_in_provider_object()
    {
        var converter = new NullIntToNonNullStringConverter().ConvertToProvider;

        Assert.Equal("0", converter(0));
        Assert.Equal("1", converter(1));
        Assert.Equal("<null>", converter(null));
    }

    [ConditionalFact]
    protected void Convert_int_nulls_to_string_nulls_in_provider_object()
    {
        var converter = new NullIntToNullStringConverter().ConvertToProvider;

        Assert.Equal("0", converter(0));
        Assert.Equal("1", converter(1));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    protected void Convert_int_non_nulls_to_string_non_nulls_in_provider_object()
    {
        var converter = new NonNullIntToNonNullStringConverter().ConvertToProvider;

        Assert.Equal("0", converter(0));
        Assert.Equal("1", converter(1));
        Assert.Throws<NullReferenceException>(() => converter(null));
    }

    [ConditionalFact]
    protected void Convert_int_non_nulls_to_string_nulls_in_provider_object()
    {
        var converter = new NonNullIntToNullStringConverter().ConvertToProvider;

        Assert.Equal("0", converter(0));
        Assert.Equal("1", converter(1));
        Assert.Throws<NullReferenceException>(() => converter(null));
    }

    [ConditionalFact]
    protected void Convert_string_nulls_to_int_non_nulls_in_app_object()
    {
        var converter = new NonNullIntToNullStringConverter().ConvertFromProvider;

        Assert.Equal(0, converter("0"));
        Assert.Equal(1, converter("1"));
        Assert.Equal(0, converter(null));
        Assert.Equal(0, converter(null));
    }

    [ConditionalFact]
    protected void Convert_string_nulls_to_int_nulls_in_app_object()
    {
        var converter = new NullIntToNullStringConverter().ConvertFromProvider;

        Assert.Equal(0, converter("0"));
        Assert.Equal(1, converter("1"));
        Assert.Null(converter("<null>"));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    protected void Convert_string_non_nulls_to_int_non_nulls_in_app_object()
    {
        var converter = new NonNullIntToNonNullStringConverter().ConvertFromProvider;

        Assert.Equal(0, converter("0"));
        Assert.Equal(1, converter("1"));
        Assert.Equal(0, converter("<null>"));
        Assert.Throws<ArgumentNullException>(() => converter(null));
    }

    [ConditionalFact]
    protected void Convert_string_non_nulls_to_int_nulls_in_app_object()
    {
        var converter = new NullIntToNonNullStringConverter().ConvertFromProvider;

        Assert.Equal(0, converter("0"));
        Assert.Equal(1, converter("1"));
        Assert.Null(converter("<null>"));
        Assert.Throws<ArgumentNullException>(() => converter(null));
    }

    protected class ConvertingEntity
    {
        public Guid Id { get; set; }

        public bool BoolAsString { get; set; }
        public bool BoolAsNullableString { get; set; }
        public bool? NullableBoolAsString { get; set; }
        public bool? NullableBoolAsNullableString { get; set; }

        public bool BoolAsChar { get; set; }
        public bool BoolAsNullableChar { get; set; }
        public bool? NullableBoolAsChar { get; set; }
        public bool? NullableBoolAsNullableChar { get; set; }

        public bool BoolAsInt { get; set; }
        public bool BoolAsNullableInt { get; set; }
        public bool? NullableBoolAsInt { get; set; }
        public bool? NullableBoolAsNullableInt { get; set; }

        public int IntAsLong { get; set; }
        public int IntAsNullableLong { get; set; }
        public int? NullableIntAsLong { get; set; }
        public int? NullableIntAsNullableLong { get; set; }

        public byte[] BytesAsString { get; set; } = null!;
        public byte[] BytesAsNullableString { get; set; } = null!;
        public byte[]? NullableBytesAsString { get; set; }
        public byte[]? NullableBytesAsNullableString { get; set; }

        public char CharAsString { get; set; }
        public char CharAsNullableString { get; set; }
        public char? NullableCharAsString { get; set; }
        public char? NullableCharAsNullableString { get; set; }

        public DateTimeOffset DateTimeOffsetToBinary { get; set; }
        public DateTimeOffset DateTimeOffsetToNullableBinary { get; set; }
        public DateTimeOffset? NullableDateTimeOffsetToBinary { get; set; }
        public DateTimeOffset? NullableDateTimeOffsetToNullableBinary { get; set; }

        public DateTimeOffset DateTimeOffsetToString { get; set; }
        public DateTimeOffset DateTimeOffsetToNullableString { get; set; }
        public DateTimeOffset? NullableDateTimeOffsetToString { get; set; }
        public DateTimeOffset? NullableDateTimeOffsetToNullableString { get; set; }

        public DateTime DateTimeToBinary { get; set; }
        public DateTime DateTimeToNullableBinary { get; set; }
        public DateTime? NullableDateTimeToBinary { get; set; }
        public DateTime? NullableDateTimeToNullableBinary { get; set; }

        public DateTime DateTimeToString { get; set; }
        public DateTime DateTimeToNullableString { get; set; }
        public DateTime? NullableDateTimeToString { get; set; }
        public DateTime? NullableDateTimeToNullableString { get; set; }

        public DateOnly DateOnlyToString { get; set; }
        public DateOnly DateOnlyToNullableString { get; set; }
        public DateOnly? NullableDateOnlyToString { get; set; }
        public DateOnly? NullableDateOnlyToNullableString { get; set; }

        public TheExperience EnumToString { get; set; }
        public TheExperience EnumToNullableString { get; set; }
        public TheExperience? NullableEnumToString { get; set; }
        public TheExperience? NullableEnumToNullableString { get; set; }

        public TheExperience EnumToNumber { get; set; }
        public TheExperience EnumToNullableNumber { get; set; }
        public TheExperience? NullableEnumToNumber { get; set; }
        public TheExperience? NullableEnumToNullableNumber { get; set; }

        public Guid GuidToString { get; set; }
        public Guid GuidToNullableString { get; set; }
        public Guid? NullableGuidToString { get; set; }
        public Guid? NullableGuidToNullableString { get; set; }

        public Guid GuidToBytes { get; set; }
        public Guid GuidToNullableBytes { get; set; }
        public Guid? NullableGuidToBytes { get; set; }
        public Guid? NullableGuidToNullableBytes { get; set; }

        public IPAddress IPAddressToString { get; set; } = null!;
        public IPAddress IPAddressToNullableString { get; set; } = null!;
        public IPAddress? NullableIPAddressToString { get; set; }
        public IPAddress? NullableIPAddressToNullableString { get; set; }

        public IPAddress IPAddressToBytes { get; set; } = null!;
        public IPAddress IPAddressToNullableBytes { get; set; } = null!;
        public IPAddress? NullableIPAddressToBytes { get; set; }
        public IPAddress? NullableIPAddressToNullableBytes { get; set; }

        public PhysicalAddress PhysicalAddressToString { get; set; } = null!;
        public PhysicalAddress PhysicalAddressToNullableString { get; set; } = null!;
        public PhysicalAddress? NullablePhysicalAddressToString { get; set; }
        public PhysicalAddress? NullablePhysicalAddressToNullableString { get; set; }

        public PhysicalAddress PhysicalAddressToBytes { get; set; } = null!;
        public PhysicalAddress PhysicalAddressToNullableBytes { get; set; } = null!;
        public PhysicalAddress? NullablePhysicalAddressToBytes { get; set; }
        public PhysicalAddress? NullablePhysicalAddressToNullableBytes { get; set; }

        public ulong NumberToString { get; set; }
        public ulong NumberToNullableString { get; set; }
        public ulong? NullableNumberToString { get; set; }
        public ulong? NullableNumberToNullableString { get; set; }

        public sbyte NumberToBytes { get; set; }
        public sbyte NumberToNullableBytes { get; set; }
        public sbyte? NullableNumberToBytes { get; set; }
        public sbyte? NullableNumberToNullableBytes { get; set; }

        public string StringToBool { get; set; } = null!;
        public string StringToNullableBool { get; set; } = null!;
        public string? NullableStringToBool { get; set; }
        public string? NullableStringToNullableBool { get; set; }

        public string StringToBytes { get; set; } = null!;
        public string StringToNullableBytes { get; set; } = null!;
        public string? NullableStringToBytes { get; set; }
        public string? NullableStringToNullableBytes { get; set; }

        public string StringToChar { get; set; } = null!;
        public string StringToNullableChar { get; set; } = null!;
        public string? NullableStringToChar { get; set; }
        public string? NullableStringToNullableChar { get; set; }

        public string StringToDateTime { get; set; } = null!;
        public string StringToNullableDateTime { get; set; } = null!;
        public string? NullableStringToDateTime { get; set; }
        public string? NullableStringToNullableDateTime { get; set; }

        public string StringToDateTimeOffset { get; set; } = null!;
        public string StringToNullableDateTimeOffset { get; set; } = null!;
        public string? NullableStringToDateTimeOffset { get; set; }
        public string? NullableStringToNullableDateTimeOffset { get; set; }

        public string StringToEnum { get; set; } = null!;
        public string StringToNullableEnum { get; set; } = null!;
        public string? NullableStringToEnum { get; set; }
        public string? NullableStringToNullableEnum { get; set; }

        public string StringToGuid { get; set; } = null!;
        public string StringToNullableGuid { get; set; } = null!;
        public string? NullableStringToGuid { get; set; }
        public string? NullableStringToNullableGuid { get; set; }

        public string StringToNumber { get; set; } = null!;
        public string StringToNullableNumber { get; set; } = null!;
        public string? NullableStringToNumber { get; set; }
        public string? NullableStringToNullableNumber { get; set; }

        public string StringToTimeSpan { get; set; } = null!;
        public string StringToNullableTimeSpan { get; set; } = null!;
        public string? NullableStringToTimeSpan { get; set; }
        public string? NullableStringToNullableTimeSpan { get; set; }

        public TimeSpan TimeSpanToTicks { get; set; }
        public TimeSpan TimeSpanToNullableTicks { get; set; }
        public TimeSpan? NullableTimeSpanToTicks { get; set; }
        public TimeSpan? NullableTimeSpanToNullableTicks { get; set; }

        public TimeSpan TimeSpanToString { get; set; }
        public TimeSpan TimeSpanToNullableString { get; set; }
        public TimeSpan? NullableTimeSpanToString { get; set; }
        public TimeSpan? NullableTimeSpanToNullableString { get; set; }

        public Uri UriToString { get; set; } = null!;
        public Uri UriToNullableString { get; set; } = null!;
        public Uri? NullableUriToString { get; set; }
        public Uri? NullableUriToNullableString { get; set; }

        public string? NullStringToNonNullString { get; set; }
        public string NonNullStringToNullString { get; set; } = null!;

        public int NonNullIntToNullString { get; set; }
        public int NonNullIntToNonNullString { get; set; }
        public int? NullIntToNullString { get; set; }
        public int? NullIntToNonNullString { get; set; }

        public List<int>? NullableListOfInt { get; set; }
        public List<int> ListOfInt { get; set; } = null!;

        public IEnumerable<int>? NullableEnumerableOfInt { get; set; }
        public IEnumerable<int> EnumerableOfInt { get; set; } = null!;
    }

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    public abstract class ValueConvertersEndToEndFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => "ValueConvertersEndToEnd";

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(
                w => w.Ignore(
                    CoreEventId.MappedEntityTypeIgnoredWarning,
                    CoreEventId.MappedPropertyIgnoredWarning,
                    CoreEventId.MappedNavigationIgnoredWarning));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            => modelBuilder.Entity<ConvertingEntity>(
                b =>
                {
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
    }

    protected class NullStringToNonNullStringConverter : ValueConverter<string?, string>
    {
        public NullStringToNonNullStringConverter()
            : base(v => v ?? "<null>", v => v == "<null>" ? null : v, convertsNulls: true)
        {
        }
    }

    protected class NonNullStringToNullStringConverter : ValueConverter<string, string?>
    {
        public NonNullStringToNullStringConverter()
            : base(v => v == "<null>" ? null : v, v => v ?? "<null>", convertsNulls: true)
        {
        }
    }

    protected class NullIntToNonNullStringConverter : ValueConverter<int?, string>
    {
        public NullIntToNonNullStringConverter()
            : base(v => v == null ? "<null>" : v.ToString()!, v => v == "<null>" ? null : int.Parse(v), convertsNulls: true)
        {
        }
    }

    protected class NullIntToNullStringConverter : ValueConverter<int?, string?>
    {
        public NullIntToNullStringConverter()
            : base(v => v == null ? null : v.ToString()!, v => v == null || v == "<null>" ? null : int.Parse(v), convertsNulls: true)
        {
        }
    }

    protected class NonNullIntToNonNullStringConverter : ValueConverter<int, string>
    {
        public NonNullIntToNonNullStringConverter()
            : base(v => v.ToString()!, v => v == "<null>" ? 0 : int.Parse(v), convertsNulls: true)
        {
        }
    }

    protected class NonNullIntToNullStringConverter : ValueConverter<int, string?>
    {
        public NonNullIntToNullStringConverter()
            : base(v => v.ToString()!, v => v == null ? 0 : int.Parse(v), convertsNulls: true)
        {
        }
    }

    protected enum TheExperience : ushort
    {
        Jimi,
        Noel,
        Mitch
    }

    protected class ListOfIntToJsonConverter : ValueConverter<List<int>, string>
    {
        public ListOfIntToJsonConverter()
            : base(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null)!)
        {
        }
    }

    protected class ListOfIntComparer : ValueComparer<List<int>?>
    {
        public ListOfIntComparer()
            : base(
                (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c == null ? null : c.ToList())
        {
        }
    }

    protected class EnumerableOfIntToJsonConverter : ValueConverter<IEnumerable<int>, string>
    {
        public EnumerableOfIntToJsonConverter()
            : base(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null)!)
        {
        }
    }

    protected class EnumerableOfIntComparer : ValueComparer<IEnumerable<int>?>
    {
        public EnumerableOfIntComparer()
            : base(
                (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c == null ? null : c.ToList())
        {
        }
    }
}
