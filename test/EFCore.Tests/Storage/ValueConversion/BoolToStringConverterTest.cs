// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class BoolToStringConverterTest
{
    private static readonly BoolToStringConverter _boolToTrueFalse
        = new("False", "True");

    [ConditionalFact]
    public void Can_convert_bools_to_true_false_strings()
    {
        var converter = _boolToTrueFalse.ConvertToProviderExpression.Compile();

        Assert.Equal("True", converter(true));
        Assert.Equal("False", converter(false));
    }

    [ConditionalFact]
    public void Can_convert_true_false_strings_to_bool()
    {
        var converter = _boolToTrueFalse.ConvertFromProviderExpression.Compile();

        Assert.False(converter("False"));
        Assert.True(converter("True"));
        Assert.False(converter("false"));
        Assert.True(converter("true"));
        Assert.False(converter("F"));
        Assert.True(converter("T"));
        Assert.False(converter("Yes"));
        Assert.False(converter(""));
        Assert.False(converter(null));
    }

    private static readonly BoolToStringConverter _boolToYn
        = new("N", "Y");

    [ConditionalFact]
    public void Can_convert_bools_to_Y_N_strings()
    {
        var converter = _boolToYn.ConvertToProviderExpression.Compile();

        Assert.Equal("Y", converter(true));
        Assert.Equal("N", converter(false));
    }

    [ConditionalFact]
    public void Can_convert_Y_N_strings_to_bool()
    {
        var converter = _boolToYn.ConvertFromProviderExpression.Compile();

        Assert.False(converter("N"));
        Assert.True(converter("Y"));
        Assert.False(converter("no"));
        Assert.True(converter("yes"));
        Assert.False(converter("True"));
        Assert.False(converter(""));
        Assert.False(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_bools_to_empty_strings_or_whitespace()
    {
        var converter = new BoolToStringConverter("", " ").ConvertToProviderExpression.Compile();

        Assert.Equal(" ", converter(true));
        Assert.Equal("", converter(false));
    }

    [ConditionalFact]
    public void Can_convert_empty_strings_or_whitespace_to_bool()
    {
        var converter = new BoolToStringConverter("", " ").ConvertFromProviderExpression.Compile();

        Assert.False(converter(""));
        Assert.True(converter(" "));
        Assert.False(converter("\t"));
        Assert.False(converter(null));
    }
}
