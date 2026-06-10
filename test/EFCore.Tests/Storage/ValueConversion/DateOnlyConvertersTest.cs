// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class DateOnlyConvertersTest
{
    private static readonly DateOnlyToStringConverter DateOnlyToString = new();

    [ConditionalFact]
    public void Can_convert_DateOnly_to_string()
    {
        var converter = DateOnlyToString.ConvertToProviderExpression.Compile();

        Assert.Equal("1973-09-03", converter(new DateOnly(1973, 9, 3)));
        Assert.Equal("0001-01-01", converter(new DateOnly()));
    }

    [ConditionalFact]
    public void Can_convert_string_to_DateOnly()
    {
        var converter = DateOnlyToString.ConvertFromProviderExpression.Compile();

        Assert.Equal(new DateOnly(1973, 9, 3), converter("1973-09-03"));
        Assert.Equal(new DateOnly(), converter("0001-01-01"));

        Assert.Throws<ArgumentNullException>(() => converter(null));
        Assert.Throws<FormatException>(() => converter("Not a DateOnly"));
    }

    [ConditionalFact]
    public void Can_convert_DateOnly_to_string_object()
    {
        var converter = DateOnlyToString.ConvertToProvider;

        Assert.Equal("1973-09-03", converter(new DateOnly(1973, 9, 3)));
        Assert.Equal("0001-01-01", converter(new DateOnly()));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_string_to_DateOnly_object()
    {
        var converter = DateOnlyToString.ConvertFromProvider;

        Assert.Equal(new DateOnly(1973, 9, 3), converter("1973-09-03"));
        Assert.Throws<FormatException>(() => converter("Not a DateOnly"));
        Assert.Null(converter(null));
    }
}
