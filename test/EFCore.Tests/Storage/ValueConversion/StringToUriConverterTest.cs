// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class StringToUriConverterTest
{
    private static readonly StringToUriConverter _stringToUri = new();

    [ConditionalFact]
    public void Can_convert_strings_to_uris()
    {
        var converter = _stringToUri.ConvertToProviderExpression.Compile();

        Assert.Equal(new Uri("https://www.github.com"), converter("https://www.github.com"));
        Assert.Equal(new Uri("/relative/path", UriKind.Relative), converter("/relative/path"));
        Assert.Equal(new Uri("ftp://www.github.com", UriKind.Absolute), converter("ftp://www.github.com/"));
        Assert.Equal(new Uri(".", UriKind.Relative), converter("."));

        Assert.Throws<UriFormatException>(() => converter("http:///"));
    }

    [ConditionalFact]
    public void Can_convert_strings_to_uris_object()
    {
        var converter = _stringToUri.ConvertToProvider;

        Assert.Equal(new Uri("https://www.github.com"), converter("https://www.github.com"));
        Assert.Equal(new Uri("/relative/path", UriKind.Relative), converter("/relative/path"));
        Assert.Equal(new Uri("ftp://www.github.com", UriKind.Absolute), converter("ftp://www.github.com/"));
        Assert.Equal(new Uri(".", UriKind.Relative), converter("."));

        Assert.Null(converter(null));
        Assert.Throws<UriFormatException>(() => converter("http:///"));
    }

    [ConditionalFact]
    public void Can_convert_uris_to_strings()
    {
        var converter = _stringToUri.ConvertFromProviderExpression.Compile();

        Assert.Equal("https://www.github.com/", converter(new Uri("https://www.github.com")));
        Assert.Equal("/relative/path", converter(new Uri("/relative/path", UriKind.Relative)));
        Assert.Equal("ftp://www.github.com/", converter(new Uri("ftp://www.github.com/", UriKind.Absolute)));
        Assert.Equal(".", converter(new Uri(".", UriKind.Relative)));
    }

    [ConditionalFact]
    public void Can_convert_uris_to_strings_object()
    {
        var converter = _stringToUri.ConvertFromProvider;

        Assert.Equal("https://www.github.com/", converter(new Uri("https://www.github.com")));
        Assert.Equal("/relative/path", converter(new Uri("/relative/path", UriKind.Relative)));
        Assert.Equal("ftp://www.github.com/", converter(new Uri("ftp://www.github.com/", UriKind.Absolute)));
        Assert.Equal(".", converter(new Uri(".", UriKind.Relative)));
        Assert.Null(converter(null));
    }
}
