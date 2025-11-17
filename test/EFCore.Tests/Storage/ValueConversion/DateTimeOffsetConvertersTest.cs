// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class DateTimeOffsetConvertersTest
{
    private static readonly DateTimeOffsetToStringConverter _dateTimeOffsetToString = new();

    [ConditionalFact]
    public void Can_convert_DateTimeOffset_to_string()
    {
        var converter = _dateTimeOffsetToString.ConvertToProviderExpression.Compile();

        Assert.Equal(
            "1973-09-03 00:10:15+07:30",
            converter(new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0))));

        Assert.Equal(
            "0001-01-01 00:00:00+00:00",
            converter(new DateTimeOffset()));
    }

    [ConditionalFact]
    public void Can_convert_string_to_DateTimeOffset()
    {
        var converter = _dateTimeOffsetToString.ConvertFromProviderExpression.Compile();

        Assert.Equal(
            new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0)),
            converter("1973-09-03 00:10:15+07:30"));

        Assert.Equal(
            new DateTimeOffset(),
            converter("0001-01-01 00:00:00+00:00"));

        Assert.Throws<ArgumentNullException>(() => converter(null));
        Assert.Throws<FormatException>(() => converter("Not a DateTimeOffset"));
    }

    [ConditionalFact]
    public void Can_convert_DateTimeOffset_to_string_object()
    {
        var converter = _dateTimeOffsetToString.ConvertToProvider;

        Assert.Equal(
            "1973-09-03 00:10:15+07:30",
            converter(new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0))));

        Assert.Equal(
            "0001-01-01 00:00:00+00:00",
            converter(new DateTimeOffset()));

        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_string_to_DateTimeOffset_object()
    {
        var converter = _dateTimeOffsetToString.ConvertFromProvider;

        Assert.Equal(
            new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0)),
            converter("1973-09-03 00:10:15+07:30"));

        Assert.Equal(
            new DateTimeOffset(),
            converter("0001-01-01 00:00:00+00:00"));

        Assert.Null(converter(null));
        Assert.Throws<FormatException>(() => converter("Not a DateTimeOffset"));
    }

    private static readonly DateTimeOffsetToBytesConverter _dateTimeOffsetToBytes = new();

    [ConditionalFact]
    public void Can_convert_DateTimeOffset_to_bytes()
    {
        var converter = _dateTimeOffsetToBytes.ConvertToProviderExpression.Compile();

        Assert.Equal(
            new byte[] { 8, 163, 157, 186, 146, 57, 205, 128, 1, 194 },
            converter(new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0))));

        Assert.Equal(
            new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            converter(new DateTimeOffset()));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_DateTimeOffset()
    {
        var converter = _dateTimeOffsetToBytes.ConvertFromProviderExpression.Compile();

        Assert.Equal(
            new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0)),
            converter([8, 163, 157, 186, 146, 57, 205, 128, 1, 194]));

        Assert.Equal(
            new DateTimeOffset(),
            converter([0, 0, 0, 0, 0, 0, 0, 0, 0, 0]));

        Assert.Equal(new DateTimeOffset(), converter([]));
        Assert.Throws<NullReferenceException>(() => converter(null));
        Assert.Throws<IndexOutOfRangeException>(() => converter([1, 2]));
    }

    [ConditionalFact]
    public void Can_convert_DateTimeOffset_to_bytes_object()
    {
        var converter = _dateTimeOffsetToBytes.ConvertToProvider;

        Assert.Equal(
            new byte[] { 8, 163, 157, 186, 146, 57, 205, 128, 1, 194 },
            converter(new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0))));

        Assert.Equal(
            new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            converter(new DateTimeOffset()));

        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_DateTimeOffset_object()
    {
        var converter = _dateTimeOffsetToBytes.ConvertFromProvider;

        Assert.Equal(
            new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0)),
            converter(new byte[] { 8, 163, 157, 186, 146, 57, 205, 128, 1, 194 }));

        Assert.Equal(
            new DateTimeOffset(),
            converter(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));

        Assert.Equal(new DateTimeOffset(), converter(Array.Empty<byte>()));
        Assert.Throws<IndexOutOfRangeException>(() => converter(new byte[] { 1, 2 }));
        Assert.Null(converter(null));
    }

    private static readonly DateTimeOffsetToBinaryConverter _dateTimeOffsetToBinary = new();

    [ConditionalFact]
    public void Can_convert_DateTimeOffset_to_binary()
    {
        var converter = _dateTimeOffsetToBinary.ConvertToProviderExpression.Compile();

        Assert.Equal(
            1274909897011200450,
            converter(new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0))));

        Assert.Equal(
            1274909897018021048,
            converter(new DateTimeOffset(new DateTime(1973, 9, 3, 0, 10, 15, 333), new TimeSpan(-14, 0, 0))));

        Assert.Equal(
            1274909897018020680,
            converter(new DateTimeOffset(new DateTime(1973, 9, 3, 0, 10, 15, 333), new TimeSpan(14, 0, 0))));

        Assert.Equal(0, converter(new DateTimeOffset()));
    }

    [ConditionalFact]
    public void Can_convert_binary_to_DateTimeOffset()
    {
        var converter = _dateTimeOffsetToBinary.ConvertFromProviderExpression.Compile();

        Assert.Equal(
            new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0)),
            converter(1274909897011200450));

        Assert.Equal(
            new DateTimeOffset(new DateTime(1973, 9, 3, 0, 10, 15, 333), new TimeSpan(-14, 0, 0)),
            converter(1274909897018021048));

        Assert.Equal(
            new DateTimeOffset(new DateTime(1973, 9, 3, 0, 10, 15, 333), new TimeSpan(14, 0, 0)),
            converter(1274909897018020680));

        Assert.Equal(new DateTimeOffset(), converter(0));
    }

    [ConditionalFact]
    public void Can_convert_DateTimeOffset_to_binary_object()
    {
        var converter = _dateTimeOffsetToBinary.ConvertToProvider;

        Assert.Equal(
            1274909897011200450,
            converter(new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0))));

        Assert.Equal(
            1274909897018021048,
            converter(new DateTimeOffset(new DateTime(1973, 9, 3, 0, 10, 15, 333), new TimeSpan(-14, 0, 0))));

        Assert.Equal(
            1274909897018020680,
            converter(new DateTimeOffset(new DateTime(1973, 9, 3, 0, 10, 15, 333), new TimeSpan(14, 0, 0))));

        Assert.Equal(0L, converter(new DateTimeOffset()));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_binary_to_DateTimeOffset_object()
    {
        var converter = _dateTimeOffsetToBinary.ConvertFromProvider;

        Assert.Equal(
            new DateTimeOffset(1973, 9, 3, 0, 10, 15, new TimeSpan(7, 30, 0)),
            converter(1274909897011200450));

        Assert.Equal(
            new DateTimeOffset(new DateTime(1973, 9, 3, 0, 10, 15, 333), new TimeSpan(-14, 0, 0)),
            converter(1274909897018021048));

        Assert.Equal(
            new DateTimeOffset(new DateTime(1973, 9, 3, 0, 10, 15, 333), new TimeSpan(14, 0, 0)),
            converter(1274909897018020680));

        Assert.Equal(new DateTimeOffset(), converter(0));
        Assert.Null(converter(null));
    }
}
