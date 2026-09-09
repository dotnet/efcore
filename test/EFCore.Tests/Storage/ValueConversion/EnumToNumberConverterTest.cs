// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class EnumToNumberConverterTest
{
    private static readonly ValueConverter<Beatles, int> _enumToNumber
        = new EnumToNumberConverter<Beatles, int>();

    [ConditionalFact]
    public void Can_convert_enums_to_numbers()
    {
        var converter = _enumToNumber.ConvertToProviderExpression.Compile();

        Assert.Equal(7, converter(Beatles.John));
        Assert.Equal(4, converter(Beatles.Paul));
        Assert.Equal(1, converter(Beatles.George));
        Assert.Equal(-1, converter(Beatles.Ringo));
        Assert.Equal(77, converter((Beatles)77));
        Assert.Equal(0, converter(default));
    }

    [ConditionalFact]
    public void Can_convert_enums_to_numbers_object()
    {
        var converter = _enumToNumber.ConvertToProvider;

        Assert.Equal(7, converter(Beatles.John));
        Assert.Equal(4, converter(Beatles.Paul));
        Assert.Equal(1, converter(Beatles.George));
        Assert.Equal(-1, converter(Beatles.Ringo));
        Assert.Equal(77, converter((Beatles)77));
        Assert.Equal(0, converter(default(Beatles)));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_numbers_to_enums()
    {
        var converter = _enumToNumber.ConvertFromProviderExpression.Compile();

        Assert.Equal(Beatles.John, converter(7));
        Assert.Equal(Beatles.Paul, converter(4));
        Assert.Equal(Beatles.George, converter(1));
        Assert.Equal(Beatles.Ringo, converter(-1));
        Assert.Equal((Beatles)77, converter(77));
        Assert.Equal(default, converter(0));
    }

    [ConditionalFact]
    public void Can_convert_numbers_to_enums_object()
    {
        var converter = _enumToNumber.ConvertFromProvider;

        Assert.Equal(Beatles.John, converter(7));
        Assert.Equal(Beatles.Paul, converter(4));
        Assert.Equal(Beatles.George, converter(1));
        Assert.Equal(Beatles.Ringo, converter(-1));
        Assert.Equal((Beatles)77, converter(77));
        Assert.Equal(default(Beatles), converter(0));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Ordering_preserved_for_enums_to_numbers()
        => ValueConverterTest.OrderingTest(_enumToNumber, Beatles.Ringo, Beatles.George, Beatles.Paul, Beatles.John);

    private static readonly ValueConverter<Beatles, double> _enumToDouble
        = new EnumToNumberConverter<Beatles, double>();

    [ConditionalFact]
    public void Can_convert_enums_to_doubles()
    {
        var converter = _enumToDouble.ConvertToProviderExpression.Compile();

        Assert.Equal(7, converter(Beatles.John));
        Assert.Equal(4, converter(Beatles.Paul));
        Assert.Equal(1, converter(Beatles.George));
        Assert.Equal(-1, converter(Beatles.Ringo));
        Assert.Equal(77, converter((Beatles)77));
        Assert.Equal(0, converter(default));
    }

    [ConditionalFact]
    public void Can_convert_doubles_to_enums()
    {
        var converter = _enumToDouble.ConvertFromProviderExpression.Compile();

        Assert.Equal(Beatles.John, converter(7));
        Assert.Equal(Beatles.Paul, converter(4));
        Assert.Equal(Beatles.George, converter(1));
        Assert.Equal(Beatles.Ringo, converter(-1));
        Assert.Equal((Beatles)77, converter(77));
        Assert.Equal(default, converter(0));
    }

    private static readonly ValueConverter<Beatles, decimal> _enumToDecimal
        = new EnumToNumberConverter<Beatles, decimal>();

    [ConditionalFact]
    public void Can_convert_enums_to_decimals()
    {
        var converter = _enumToDecimal.ConvertToProviderExpression.Compile();

        Assert.Equal(7, converter(Beatles.John));
        Assert.Equal(4, converter(Beatles.Paul));
        Assert.Equal(1, converter(Beatles.George));
        Assert.Equal(-1, converter(Beatles.Ringo));
        Assert.Equal(77, converter((Beatles)77));
        Assert.Equal(0, converter(default));
    }

    [ConditionalFact]
    public void Can_convert_decimals_to_enums()
    {
        var converter = _enumToDecimal.ConvertFromProviderExpression.Compile();

        Assert.Equal(Beatles.John, converter(7));
        Assert.Equal(Beatles.Paul, converter(4));
        Assert.Equal(Beatles.George, converter(1));
        Assert.Equal(Beatles.Ringo, converter(-1));
        Assert.Equal((Beatles)77, converter(77));
        Assert.Equal(default, converter(0));
    }

    private static readonly ValueConverter<Beatles, float> _enumToFloat
        = new EnumToNumberConverter<Beatles, float>();

    [ConditionalFact]
    public void Can_convert_enums_to_floats()
    {
        var converter = _enumToFloat.ConvertToProviderExpression.Compile();

        Assert.Equal(7, converter(Beatles.John));
        Assert.Equal(4, converter(Beatles.Paul));
        Assert.Equal(1, converter(Beatles.George));
        Assert.Equal(-1, converter(Beatles.Ringo));
        Assert.Equal(77, converter((Beatles)77));
        Assert.Equal(0, converter(default));
    }

    [ConditionalFact]
    public void Can_convert_floats_to_enums()
    {
        var converter = _enumToFloat.ConvertFromProviderExpression.Compile();

        Assert.Equal(Beatles.John, converter(7));
        Assert.Equal(Beatles.Paul, converter(4));
        Assert.Equal(Beatles.George, converter(1));
        Assert.Equal(Beatles.Ringo, converter(-1));
        Assert.Equal((Beatles)77, converter(77));
        Assert.Equal(default, converter(0));
    }

    private static readonly ValueConverter<Beatles, byte> _enumToByte
        = new EnumToNumberConverter<Beatles, byte>();

    [ConditionalFact]
    public void Can_convert_enums_to_bytes()
    {
        var converter = _enumToByte.ConvertToProviderExpression.Compile();

        Assert.Equal(7, converter(Beatles.John));
        Assert.Equal(4, converter(Beatles.Paul));
        Assert.Equal(1, converter(Beatles.George));
        Assert.Equal(77, converter((Beatles)77));
        Assert.Equal(0, converter(default));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_enums()
    {
        var converter = _enumToByte.ConvertFromProviderExpression.Compile();

        Assert.Equal(Beatles.John, converter(7));
        Assert.Equal(Beatles.Paul, converter(4));
        Assert.Equal(Beatles.George, converter(1));
        Assert.Equal((Beatles)77, converter(77));
        Assert.Equal(default, converter(0));
    }

    private static readonly ValueConverter<Beatles, sbyte> _enumToSByte
        = new EnumToNumberConverter<Beatles, sbyte>();

    [ConditionalFact]
    public void Can_convert_enums_to_sbytes()
    {
        var converter = _enumToSByte.ConvertToProviderExpression.Compile();

        Assert.Equal(7, converter(Beatles.John));
        Assert.Equal(4, converter(Beatles.Paul));
        Assert.Equal(1, converter(Beatles.George));
        Assert.Equal(-1, converter(Beatles.Ringo));
        Assert.Equal(77, converter((Beatles)77));
        Assert.Equal(0, converter(default));
    }

    [ConditionalFact]
    public void Can_convert_sbytes_to_enums()
    {
        var converter = _enumToSByte.ConvertFromProviderExpression.Compile();

        Assert.Equal(Beatles.John, converter(7));
        Assert.Equal(Beatles.Paul, converter(4));
        Assert.Equal(Beatles.George, converter(1));
        Assert.Equal(Beatles.Ringo, converter(-1));
        Assert.Equal((Beatles)77, converter(77));
        Assert.Equal(default, converter(0));
    }

    private static readonly ValueConverter<Beatles, long> _enumToLong
        = new EnumToNumberConverter<Beatles, long>();

    [ConditionalFact]
    public void Can_convert_enums_to_longs()
    {
        var converter = _enumToLong.ConvertToProviderExpression.Compile();

        Assert.Equal(7, converter(Beatles.John));
        Assert.Equal(4, converter(Beatles.Paul));
        Assert.Equal(1, converter(Beatles.George));
        Assert.Equal(-1, converter(Beatles.Ringo));
        Assert.Equal(77, converter((Beatles)77));
        Assert.Equal(0, converter(default));
    }

    [ConditionalFact]
    public void Can_convert_longs_to_enums()
    {
        var converter = _enumToLong.ConvertFromProviderExpression.Compile();

        Assert.Equal(Beatles.John, converter(7));
        Assert.Equal(Beatles.Paul, converter(4));
        Assert.Equal(Beatles.George, converter(1));
        Assert.Equal(Beatles.Ringo, converter(-1));
        Assert.Equal((Beatles)77, converter(77));
        Assert.Equal(default, converter(0));
    }

    private static readonly ValueConverter<Beatles, ulong> _enumToULong
        = new EnumToNumberConverter<Beatles, ulong>();

    [ConditionalFact]
    public void Can_convert_enums_to_ulongs()
    {
        var converter = _enumToULong.ConvertToProviderExpression.Compile();

        Assert.Equal((ulong)7, converter(Beatles.John));
        Assert.Equal((ulong)4, converter(Beatles.Paul));
        Assert.Equal((ulong)1, converter(Beatles.George));
        Assert.Equal((ulong)77, converter((Beatles)77));
        Assert.Equal((ulong)0, converter(default));
    }

    [ConditionalFact]
    public void Can_convert_ulongs_to_enums()
    {
        var converter = _enumToULong.ConvertFromProviderExpression.Compile();

        Assert.Equal(Beatles.John, converter(7));
        Assert.Equal(Beatles.Paul, converter(4));
        Assert.Equal(Beatles.George, converter(1));
        Assert.Equal((Beatles)77, converter(77));
        Assert.Equal(default, converter(0));
    }

    private enum Beatles
    {
        John = 7,
        Paul = 4,
        George = 1,
        Ringo = -1
    }
}
