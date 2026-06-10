// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class StringToCharConverterTest
{
    private static readonly StringToCharConverter _stringToChar = new();

    [Fact]
    public void Can_convert_strings_to_chars()
    {
        var converter = _stringToChar.ConvertToProviderExpression.Compile();

        Assert.Equal('A', converter("A"));
        Assert.Equal('z', converter("z"));
        Assert.Equal('F', converter("Funkadelic"));
        Assert.Equal('\0', converter(""));

        Assert.Throws<NullReferenceException>(() => converter(null));
    }

    [Fact]
    public void Can_convert_strings_to_chars_object()
    {
        var converter = _stringToChar.ConvertToProvider;

        Assert.Equal('A', converter("A"));
        Assert.Equal('z', converter("z"));
        Assert.Equal('F', converter("Funkadelic"));
        Assert.Null(converter(null));
    }

    [Fact]
    public void Can_convert_chars_to_strings()
    {
        var converter = _stringToChar.ConvertFromProviderExpression.Compile();

        Assert.Equal("A", converter('A'));
        Assert.Equal("!", converter('!'));
    }

    [Fact]
    public void Can_convert_chars_to_strings_object()
    {
        var converter = _stringToChar.ConvertFromProvider;

        Assert.Equal("A", converter('A'));
        Assert.Equal("!", converter('!'));
        Assert.Equal("A", converter((char?)'A'));
        Assert.Equal("!", converter((char?)'!'));
        Assert.Null(converter(null));
    }
}
