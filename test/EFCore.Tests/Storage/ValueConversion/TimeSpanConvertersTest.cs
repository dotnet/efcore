// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class TimeSpanConvertersTest
{
    private static readonly TimeSpanToStringConverter _timeSpanToString = new();

    [ConditionalFact]
    public void Can_convert_TimeSpan_to_string()
    {
        var converter = _timeSpanToString.ConvertToProviderExpression.Compile();

        Assert.Equal(
            "10.07:30:18.3330000",
            converter(new TimeSpan(10, 7, 30, 15, 3333)));

        Assert.Equal("00:00:00", converter(new TimeSpan()));
    }

    [ConditionalFact]
    public void Can_convert_string_to_TimeSpan()
    {
        var converter = _timeSpanToString.ConvertFromProviderExpression.Compile();

        Assert.Equal(
            new TimeSpan(10, 7, 30, 15, 3333),
            converter("10.07:30:18.3330000"));

        Assert.Equal(new TimeSpan(), converter("00:00:00"));

        Assert.Throws<ArgumentNullException>(() => converter(null));
        Assert.Throws<FormatException>(() => converter("Not a timespan"));
    }

    [ConditionalFact]
    public void Can_convert_TimeSpan_to_string_object()
    {
        var converter = _timeSpanToString.ConvertToProvider;

        Assert.Equal(
            "10.07:30:18.3330000",
            converter(new TimeSpan(10, 7, 30, 15, 3333)));

        Assert.Equal("00:00:00", converter(new TimeSpan()));

        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_string_to_TimeSpan_object()
    {
        var converter = _timeSpanToString.ConvertFromProvider;

        Assert.Equal(
            new TimeSpan(10, 7, 30, 15, 3333),
            converter("10.07:30:18.3330000"));

        Assert.Equal(new TimeSpan(), converter("00:00:00"));

        Assert.Throws<FormatException>(() => converter("Not a timespan"));
        Assert.Null(converter(null));
    }

    private static readonly TimeSpanToTicksConverter _timeSpanToTicks = new();

    [ConditionalFact]
    public void Can_convert_TimeSpan_to_ticks()
    {
        var converter = _timeSpanToTicks.ConvertToProviderExpression.Compile();

        Assert.Equal(8910183330000, converter(new TimeSpan(10, 7, 30, 15, 3333)));
        Assert.Equal(0, converter(new TimeSpan()));
    }

    [ConditionalFact]
    public void Can_convert_ticks_to_TimeSpan()
    {
        var converter = _timeSpanToTicks.ConvertFromProviderExpression.Compile();

        Assert.Equal(new TimeSpan(10, 7, 30, 15, 3333), converter(8910183330000));
        Assert.Equal(new TimeSpan(), converter(0));
    }

    [ConditionalFact]
    public void Can_convert_TimeSpan_to_ticks_object()
    {
        var converter = _timeSpanToTicks.ConvertToProvider;

        Assert.Equal(8910183330000, converter(new TimeSpan(10, 7, 30, 15, 3333)));
        Assert.Equal(0L, converter(new TimeSpan()));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_ticks_to_TimeSpan_object()
    {
        var converter = _timeSpanToTicks.ConvertFromProvider;

        Assert.Equal(new TimeSpan(10, 7, 30, 15, 3333), converter(8910183330000));
        Assert.Equal(new TimeSpan(), converter(0));
        Assert.Null(converter(null));
    }

    private static readonly CompositeValueConverter<TimeSpan, long, uint> _timeSpanToIntTicks
        = (CompositeValueConverter<TimeSpan, long, uint>)new TimeSpanToTicksConverter().ComposeWith(
            new CastingConverter<long, uint>());

    [ConditionalFact]
    public void Can_convert_TimeSpan_to_int_ticks()
    {
        var converter = _timeSpanToIntTicks.ConvertToProviderExpression.Compile();

        Assert.Equal((uint)183330000, converter(new TimeSpan(0, 0, 0, 15, 3333)));
        Assert.Equal((uint)0, converter(new TimeSpan()));
    }

    [ConditionalFact]
    public void Can_convert_int_ticks_to_TimeSpan()
    {
        var converter = _timeSpanToIntTicks.ConvertFromProviderExpression.Compile();

        Assert.Equal(new TimeSpan(0, 0, 0, 15, 3333), converter(183330000));
        Assert.Equal(new TimeSpan(), converter(0));
    }

    [ConditionalFact]
    public void Can_convert_TimeSpan_to_int_ticks_object()
    {
        var converter = _timeSpanToIntTicks.ConvertToProvider;

        Assert.Equal((uint)183330000, converter(new TimeSpan(0, 0, 0, 15, 3333)));
        Assert.Equal((uint)0, converter(new TimeSpan()));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_int_ticks_to_TimeSpan_object()
    {
        var converter = _timeSpanToIntTicks.ConvertFromProvider;

        Assert.Equal(new TimeSpan(0, 0, 0, 15, 3333), converter(183330000));
        Assert.Equal(new TimeSpan(), converter(0));
        Assert.Null(converter(null));
    }
}
