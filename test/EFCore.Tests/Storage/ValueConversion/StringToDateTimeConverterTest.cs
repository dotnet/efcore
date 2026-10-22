// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class StringToDateTimeConverterTest
{
    private static readonly StringToDateTimeConverter _stringToDateTime = new();

    [ConditionalFact]
    public void Can_convert_string_to_DateTime()
    {
        var converter = _stringToDateTime.ConvertToProviderExpression.Compile();

        Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 15), converter("1973-09-03 00:10:15"));
        Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 15), converter("1973-09-03 00:10:15"));
        // Kind is not preserved
        Assert.NotEqual(DateTimeKind.Utc, converter("1973-09-03 00:10:15").Kind);
        Assert.Equal(new DateTime(), converter("0001-01-01 00:00:00"));

        Assert.Throws<FormatException>(() => converter("Not a DateTime"));
        Assert.Throws<ArgumentNullException>(() => converter(null));
    }

    [ConditionalFact]
    public void Can_convert_DateTime_to_string()
    {
        var converter = _stringToDateTime.ConvertFromProviderExpression.Compile();

        Assert.Equal("1973-09-03 00:10:15", converter(new DateTime(1973, 9, 3, 0, 10, 15, DateTimeKind.Utc)));
        Assert.Equal("1973-09-03 00:10:15", converter(new DateTime(1973, 9, 3, 0, 10, 15)));
        Assert.Equal("0001-01-01 00:00:00", converter(new DateTime()));
    }

    [ConditionalFact]
    public void Can_convert_string_to_DateTime_object()
    {
        var converter = _stringToDateTime.ConvertToProvider;

        Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 15), converter("1973-09-03 00:10:15"));
        Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 15), converter("1973-09-03 00:10:15"));
        // Kind is not preserved
        Assert.NotEqual(DateTimeKind.Utc, ((DateTime)converter("1973-09-03 00:10:15")!).Kind);
        Assert.Equal(new DateTime(), converter("0001-01-01 00:00:00"));

        Assert.Throws<FormatException>(() => converter("Not a DateTime"));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_DateTime_to_string_object()
    {
        var converter = _stringToDateTime.ConvertFromProvider;

        Assert.Equal("1973-09-03 00:10:15", converter(new DateTime(1973, 9, 3, 0, 10, 15, DateTimeKind.Utc)));
        Assert.Equal("1973-09-03 00:10:15", converter(new DateTime(1973, 9, 3, 0, 10, 15)));
        Assert.Equal("0001-01-01 00:00:00", converter(new DateTime()));
    }
}
