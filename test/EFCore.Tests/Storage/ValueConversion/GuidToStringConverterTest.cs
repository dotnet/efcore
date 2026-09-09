// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class GuidToStringConverterTest
{
    private static readonly GuidToStringConverter _guidToString = new();

    [ConditionalFact]
    public void Can_convert_GUIDs_to_String()
    {
        var converter = _guidToString.ConvertToProviderExpression.Compile();

        Assert.Equal(
            "96ee27b4-868b-4049-ba67-cbb83ce5b462",
            converter(new Guid("96EE27B4-868B-4049-BA67-CBB83CE5B462")));

        Assert.Equal(
            "00000000-0000-0000-0000-000000000000",
            converter(Guid.Empty));
    }

    [ConditionalFact]
    public void Can_convert_String_to_GUIDs()
    {
        var converter = _guidToString.ConvertFromProviderExpression.Compile();

        Assert.Equal(
            new Guid("96EE27B4-868B-4049-BA67-CBB83CE5B462"),
            converter("96EE27B4-868B-4049-BA67-CBB83CE5B462"));

        Assert.Equal(
            Guid.Empty,
            converter("00000000-0000-0000-0000-000000000000"));

        Assert.Throws<FormatException>(() => converter("Not a GUID"));
        Assert.Throws<ArgumentNullException>(() => converter(null));
    }

    [ConditionalFact]
    public void Can_convert_GUIDs_to_String_object()
    {
        var converter = _guidToString.ConvertToProvider;

        Assert.Equal(
            "96ee27b4-868b-4049-ba67-cbb83ce5b462",
            converter(new Guid("96EE27B4-868B-4049-BA67-CBB83CE5B462")));

        Assert.Equal(
            "00000000-0000-0000-0000-000000000000",
            converter(Guid.Empty));

        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_String_to_GUIDs_object()
    {
        var converter = _guidToString.ConvertFromProvider;

        Assert.Equal(
            new Guid("96EE27B4-868B-4049-BA67-CBB83CE5B462"),
            converter("96EE27B4-868B-4049-BA67-CBB83CE5B462"));

        Assert.Equal(
            Guid.Empty,
            converter("00000000-0000-0000-0000-000000000000"));

        Assert.Throws<FormatException>(() => converter("Not a GUID"));
        Assert.Null(converter(null));
    }
}
