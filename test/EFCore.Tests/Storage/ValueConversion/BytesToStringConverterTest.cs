// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class BytesToStringConverterTest
{
    private static readonly BytesToStringConverter _bytesToStringConverter = new();

    [ConditionalFact]
    public void Can_convert_strings_to_bytes()
    {
        var converter = _bytesToStringConverter.ConvertToProviderExpression.Compile();
        Assert.False(_bytesToStringConverter.ConvertsNulls);

        Assert.Equal("U3DEsW7MiGFsIFRhcA==", converter([83, 112, 196, 177, 110, 204, 136, 97, 108, 32, 84, 97, 112]));
        Assert.Equal("", converter([]));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_strings()
    {
        var converter = _bytesToStringConverter.ConvertFromProviderExpression.Compile();

        Assert.Equal(new byte[] { 83, 112, 196, 177, 110, 204, 136, 97, 108, 32, 84, 97, 112 }, converter("U3DEsW7MiGFsIFRhcA=="));
        Assert.Equal([], converter(""));
    }

    [ConditionalFact]
    public void Can_convert_strings_to_long_non_char_bytes()
    {
        var converter = _bytesToStringConverter.ConvertToProviderExpression.Compile();

        Assert.Equal(CreateLongBytesString(), converter(CreateLongBytes()));
    }

    [ConditionalFact]
    public void Can_convert_long_non_char_bytes_to_strings()
    {
        var converter = _bytesToStringConverter.ConvertFromProviderExpression.Compile();

        Assert.Equal(CreateLongBytes(), converter(CreateLongBytesString()));
    }

    private static byte[] CreateLongBytes()
    {
        var longBinary = new byte[1000];
        for (var i = 0; i < longBinary.Length; i++)
        {
            longBinary[i] = (byte)i;
        }

        return longBinary;
    }

    private static string CreateLongBytesString()
    {
        var longBinary = new byte[1000];
        for (var i = 0; i < longBinary.Length; i++)
        {
            longBinary[i] = (byte)i;
        }

        return Convert.ToBase64String(longBinary);
    }
}
