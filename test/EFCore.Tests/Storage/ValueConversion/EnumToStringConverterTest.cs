// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class EnumToStringConverterTest
{
    private static readonly ValueConverter<Beatles, string> _enumToString
        = new EnumToStringConverter<Beatles>();

    [ConditionalFact]
    public void Can_convert_enums_to_strings()
    {
        var converter = _enumToString.ConvertToProviderExpression.Compile();

        Assert.Equal("John", converter(Beatles.John));
        Assert.Equal("Paul", converter(Beatles.Paul));
        Assert.Equal("George", converter(Beatles.George));
        Assert.Equal("Ringo", converter(Beatles.Ringo));
        Assert.Equal("77", converter((Beatles)77));
        Assert.Equal("0", converter(default));
    }

    [ConditionalFact]
    public void Can_convert_enums_to_strings_object()
    {
        var converter = _enumToString.ConvertToProvider;

        Assert.Equal("John", converter(Beatles.John));
        Assert.Equal("Paul", converter(Beatles.Paul));
        Assert.Equal("George", converter(Beatles.George));
        Assert.Equal("Ringo", converter(Beatles.Ringo));
        Assert.Equal("77", converter((Beatles)77));
        Assert.Equal("0", converter(default(Beatles)));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_strings_to_enums()
    {
        var converter = _enumToString.ConvertFromProviderExpression.Compile();

        Assert.Equal(Beatles.John, converter("John"));
        Assert.Equal(Beatles.Paul, converter("Paul"));
        Assert.Equal(Beatles.George, converter("George"));
        Assert.Equal(Beatles.Ringo, converter("Ringo"));
        Assert.Equal(Beatles.Ringo, converter("RINGO"));
        Assert.Equal(Beatles.John, converter("7"));
        Assert.Equal(Beatles.Ringo, converter("-1"));
        Assert.Equal((Beatles)77, converter("77"));
        Assert.Equal(default, converter("0"));
        Assert.Equal(default, converter(""));

        Assert.Throws<ArgumentNullException>(() => converter(null));

        Assert.Equal(
            CoreStrings.CannotConvertEnumValue("Jon", "Beatles"),
            Assert.Throws<InvalidOperationException>(() => converter("Jon")).Message);
    }

    [ConditionalFact]
    public void Can_convert_strings_to_enums_object()
    {
        var converter = _enumToString.ConvertFromProvider;

        Assert.Equal(Beatles.John, converter("John"));
        Assert.Equal(Beatles.Paul, converter("Paul"));
        Assert.Equal(Beatles.George, converter("George"));
        Assert.Equal(Beatles.Ringo, converter("Ringo"));
        Assert.Equal(Beatles.Ringo, converter("rINGO"));
        Assert.Equal(Beatles.John, converter("7"));
        Assert.Equal(Beatles.Ringo, converter("-1"));
        Assert.Equal((Beatles)77, converter("77"));
        Assert.Equal(default(Beatles), converter("0"));
        Assert.Null(converter(null));
    }

    private enum Beatles
    {
        John = 7,
        Paul = 4,
        George = 1,
        Ringo = -1
    }
}
