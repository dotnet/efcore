// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class StringToTimeSpanConverterTest
{
    private static readonly StringToTimeSpanConverter _stringToTimeSpan = new();

    [ConditionalFact]
    public void Can_convert_string_to_TimeSpan()
    {
        var converter = _stringToTimeSpan.ConvertToProviderExpression.Compile();

        Assert.Equal(
            new TimeSpan(10, 7, 30, 15, 3333),
            converter("10.07:30:18.3330000"));

        Assert.Equal(new TimeSpan(), converter("00:00:00"));

        Assert.Throws<ArgumentNullException>(() => converter(null));
        Assert.Throws<FormatException>(() => converter("Not a TimeSpan"));
    }

    [ConditionalFact]
    public void Can_convert_TimeSpan_to_string()
    {
        var converter = _stringToTimeSpan.ConvertFromProviderExpression.Compile();

        Assert.Equal(
            "10.07:30:18.3330000",
            converter(new TimeSpan(10, 7, 30, 15, 3333)));

        Assert.Equal("00:00:00", converter(new TimeSpan()));
    }

    [ConditionalFact]
    public void Can_convert_string_to_TimeSpan_object()
    {
        var converter = _stringToTimeSpan.ConvertToProvider;

        Assert.Equal(
            new TimeSpan(10, 7, 30, 15, 3333),
            converter("10.07:30:18.3330000"));

        Assert.Equal(new TimeSpan(), converter("00:00:00"));

        Assert.Null(converter(null));
        Assert.Throws<FormatException>(() => converter("Not a TimeSpan"));
    }

    [ConditionalFact]
    public void Can_convert_TimeSpan_to_string_object()
    {
        var converter = _stringToTimeSpan.ConvertFromProvider;

        Assert.Equal(
            "10.07:30:18.3330000",
            converter(new TimeSpan(10, 7, 30, 15, 3333)));

        Assert.Equal("00:00:00", converter(new TimeSpan()));

        Assert.Null(converter(null));
    }
}
