// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class BoolToZeroOneConverterTest
{
    private static readonly BoolToZeroOneConverter<decimal> _boolToZeroOne = new();

    [ConditionalFact]
    public void Can_convert_bools_to_zero_one()
    {
        var converter = _boolToZeroOne.ConvertToProviderExpression.Compile();

        Assert.Equal(1, converter(true));
        Assert.Equal(0, converter(false));
    }

    [ConditionalFact]
    public void Can_convert_bools_to_zero_one_object()
    {
        var converter = _boolToZeroOne.ConvertToProvider;

        Assert.Equal((decimal)1, converter(true));
        Assert.Equal((decimal)0, converter(false));
        Assert.Equal((decimal)1, converter((bool?)true));
        Assert.Equal((decimal)0, converter((bool?)false));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_zero_one_to_bool()
    {
        var converter = _boolToZeroOne.ConvertFromProviderExpression.Compile();

        Assert.False(converter(0));
        Assert.True(converter(1));
        Assert.False(converter(77));
    }

    [ConditionalFact]
    public void Can_convert_zero_one_to_bool_object()
    {
        var converter = _boolToZeroOne.ConvertFromProvider;

        Assert.Equal(false, converter(0));
        Assert.Equal(true, converter(1));
        Assert.Equal(false, converter(77));
        Assert.Equal(false, converter((int?)0));
        Assert.Equal(true, converter((int?)1));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Ordering_preserved_for_bools_to_zero_one()
        => ValueConverterTest.OrderingTest(_boolToZeroOne, false, true);

    [ConditionalFact]
    public void Can_convert_bools_to_zero_one_for_all_numerics()
    {
        GenericConvertTest(0, 1);
        GenericConvertTest<short>(0, 1);
        GenericConvertTest<long>(0, 1);
        GenericConvertTest<byte>(0, 1);
        GenericConvertTest<uint>(0, 1);
        GenericConvertTest<ushort>(0, 1);
        GenericConvertTest<ulong>(0, 1);
        GenericConvertTest<byte>(0, 1);
        GenericConvertTest<decimal>(0, 1);
        GenericConvertTest<double>(0, 1);
        GenericConvertTest<float>(0, 1);
    }

    private static void GenericConvertTest<TProvider>(TProvider zero, TProvider one)
    {
        var converter = new BoolToZeroOneConverter<TProvider>();

        var toProvider = converter.ConvertToProviderExpression.Compile();
        Assert.Equal(one, toProvider(true));
        Assert.Equal(zero, toProvider(false));

        var fromProvider = converter.ConvertFromProviderExpression.Compile();
        Assert.True(fromProvider(one));
        Assert.False(fromProvider(zero));
    }
}
