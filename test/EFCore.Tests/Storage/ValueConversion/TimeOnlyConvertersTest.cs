// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class TimeOnlyConvertersTest
{
    private static readonly TimeOnlyToStringConverter TimeOnlyToString = new();

    [ConditionalFact]
    public void Can_convert_TimeOnly_to_string()
    {
        var converter = TimeOnlyToString.ConvertToProviderExpression.Compile();

        Assert.Equal("07:30:15", converter(new TimeOnly(7, 30, 15)));
        Assert.Equal("07:30:15.3330000", converter(new TimeOnly(7, 30, 15, 333)));
        Assert.Equal("07:30:15.3334440", converter(new TimeOnly(7, 30, 15, 333, 444)));
        Assert.Equal("07:30:15.3334445", converter(new TimeOnly(7, 30, 15, 333, 444).Add(new TimeSpan(5))));
        Assert.Equal("00:00:00", converter(new TimeOnly()));
    }

    [ConditionalFact]
    public void Can_convert_string_to_TimeOnly()
    {
        var converter = TimeOnlyToString.ConvertFromProviderExpression.Compile();

        Assert.Equal(new TimeOnly(7, 30, 15), converter("07:30:15"));
        Assert.Equal(new TimeOnly(7, 30, 15, 333), converter("07:30:15.3330000"));
        Assert.Equal(new TimeOnly(7, 30, 15, 333, 444), converter("07:30:15.3334440"));
        Assert.Equal(new TimeOnly(7, 30, 15, 333, 444).Add(new TimeSpan(5)), converter("07:30:15.3334445"));
        Assert.Equal(new TimeOnly(), converter("00:00:00"));

        Assert.Throws<ArgumentNullException>(() => converter(null));
        Assert.Throws<FormatException>(() => converter("Not a TimeOnly"));
    }

    [ConditionalFact]
    public void Can_convert_TimeOnly_to_string_object()
    {
        var converter = TimeOnlyToString.ConvertToProvider;

        Assert.Equal("07:30:15", converter(new TimeOnly(7, 30, 15)));
        Assert.Equal("07:30:15.3330000", converter(new TimeOnly(7, 30, 15, 333)));
        Assert.Equal("07:30:15.3334440", converter(new TimeOnly(7, 30, 15, 333, 444)));
        Assert.Equal("07:30:15.3334445", converter(new TimeOnly(7, 30, 15, 333, 444).Add(new TimeSpan(5))));
        Assert.Equal("00:00:00", converter(new TimeOnly()));

        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_string_to_TimeOnly_object()
    {
        var converter = TimeOnlyToString.ConvertFromProvider;

        Assert.Equal(new TimeOnly(7, 30, 15), converter("07:30:15"));
        Assert.Equal(new TimeOnly(7, 30, 15, 333), converter("07:30:15.3330000"));
        Assert.Equal(new TimeOnly(7, 30, 15, 333, 444), converter("07:30:15.3334440"));
        Assert.Equal(new TimeOnly(7, 30, 15, 333, 444).Add(new TimeSpan(5)), converter("07:30:15.3334445"));
        Assert.Equal(new TimeOnly(), converter("00:00:00"));

        Assert.Throws<FormatException>(() => converter("Not a TimeOnly"));
        Assert.Null(converter(null));
    }

    private static readonly TimeOnlyToTicksConverter TimeOnlyToTicks = new();

    [ConditionalFact]
    public void Can_convert_TimeOnly_to_ticks()
    {
        var converter = TimeOnlyToTicks.ConvertToProviderExpression.Compile();

        Assert.Equal(270153330000, converter(new TimeOnly(7, 30, 15, 333)));
        Assert.Equal(0, converter(new TimeOnly()));
    }

    [ConditionalFact]
    public void Can_convert_ticks_to_TimeOnly()
    {
        var converter = TimeOnlyToTicks.ConvertFromProviderExpression.Compile();

        Assert.Equal(new TimeOnly(7, 30, 15, 333), converter(270153330000));
        Assert.Equal(new TimeOnly(), converter(0));
    }

    [ConditionalFact]
    public void Can_convert_TimeOnly_to_ticks_object()
    {
        var converter = TimeOnlyToTicks.ConvertToProvider;

        Assert.Equal(270153330000, converter(new TimeOnly(7, 30, 15, 333)));
        Assert.Equal(0L, converter(new TimeOnly()));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_ticks_to_TimeOnly_object()
    {
        var converter = TimeOnlyToTicks.ConvertFromProvider;

        Assert.Equal(new TimeOnly(7, 30, 15, 333), converter(270153330000));
        Assert.Equal(new TimeOnly(), converter(0));
        Assert.Null(converter(null));
    }
}
