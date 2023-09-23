// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Numerics;

namespace Microsoft.EntityFrameworkCore.Storage;

public class StringNumberConverterTestBase
{
    public void Can_convert_uint_to_natural_strings<T>(Func<T, string> converter)
        where T : IMinMaxValue<T>, INumberBase<T>
    {
        Assert.Equal(T.Zero, T.MinValue);
        Assert.Equal(T.MaxValue.ToString("R", CultureInfo.InvariantCulture), converter(T.MaxValue));
        Assert.Equal("77", converter(T.Parse("77", CultureInfo.InvariantCulture)));
        Assert.Equal("0", converter(T.Zero));
    }

    public void Can_convert_natural_strings_to_uint<T>(Func<string, T> converter)
            where T : IMinMaxValue<T>, INumberBase<T>
    {
        string maxValue = T.MaxValue.ToString("R", CultureInfo.InvariantCulture);

        Assert.Equal(T.Zero, T.MinValue);

        Assert.Equal(T.MaxValue, converter(maxValue));
        Assert.Equal(T.Parse("77", CultureInfo.InvariantCulture), converter("77"));
        Assert.Equal(T.Zero, converter("0"));

        Assert.Throws<OverflowException>(() => converter(maxValue + "0"));
        Assert.Throws<OverflowException>(() => converter("-1"));
        Assert.Throws<FormatException>(() => converter("Not a number"));
    }

    public void Can_convert_int_to_natural_strings<T>(Func<T, string> converter)
        where T : IMinMaxValue<T>, INumberBase<T>
    {
        Assert.True(T.IsNegative(T.MinValue));
        Assert.Equal(T.MaxValue.ToString("R", CultureInfo.InvariantCulture), converter(T.MaxValue));
        Assert.Equal(T.MinValue.ToString("R", CultureInfo.InvariantCulture), converter(T.MinValue));
        Assert.Equal("77", converter(T.Parse("77", CultureInfo.InvariantCulture)));
        Assert.Equal("-77", converter(T.Parse("-77", CultureInfo.InvariantCulture)));
        Assert.Equal("0", converter(T.Zero));
    }

    public void Can_convert_natural_strings_to_int<T>(Func<string, T> converter)
        where T : IMinMaxValue<T>, INumberBase<T>
    {
        string maxValue = T.MaxValue.ToString("R", CultureInfo.InvariantCulture);
        string minValue = T.MinValue.ToString("R", CultureInfo.InvariantCulture);

        Assert.Equal(T.MaxValue, converter(maxValue));
        Assert.Equal(T.MinValue, converter(minValue));
        Assert.Equal(T.Parse("77", CultureInfo.InvariantCulture), converter("77"));
        Assert.Equal(T.Parse("-77", CultureInfo.InvariantCulture), converter("-77"));
        Assert.Equal(T.Zero, converter("0"));

        Assert.Throws<OverflowException>(() => converter(maxValue + "0"));
        Assert.Throws<OverflowException>(() => converter(minValue + "0"));
        Assert.Throws<FormatException>(() => converter("Not a number"));
    }

    public void Can_convert_BigInteger_to_natural_strings(Func<BigInteger, string> converter)
    {
        Assert.Equal("170141183460469231731687303715884105728", converter(BigInteger.One + Int128.MaxValue));
        Assert.Equal("-170141183460469231731687303715884105729", converter(BigInteger.MinusOne + Int128.MinValue));
        Assert.Equal("1", converter(BigInteger.One));
        Assert.Equal("-1", converter(BigInteger.MinusOne));
        Assert.Equal("0", converter(BigInteger.Zero));
    }

    public void Can_convert_natural_strings_to_BigInteger(Func<string, BigInteger> converter)
    {
        Assert.Equal(BigInteger.One + Int128.MaxValue, converter("170141183460469231731687303715884105728"));
        Assert.Equal(BigInteger.MinusOne + Int128.MinValue, converter("-170141183460469231731687303715884105729"));
        Assert.Equal(BigInteger.One, converter("1"));
        Assert.Equal(BigInteger.MinusOne, converter("-1"));
        Assert.Equal(BigInteger.Zero, converter("0"));

        Assert.Throws<FormatException>(() => converter("Not a number"));
    }
}
