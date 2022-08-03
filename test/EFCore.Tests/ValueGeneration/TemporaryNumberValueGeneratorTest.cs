// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.ValueGeneration;

public class TemporaryNumberValueGeneratorTest
{
    [ConditionalFact]
    public void Can_create_values_for_int_types()
    {
        var generator = new TemporaryIntValueGenerator();

        Assert.Equal(int.MinValue + 1001, generator.Next(null));
        Assert.Equal(int.MinValue + 1002, generator.Next(null));
        Assert.Equal(int.MinValue + 1003, generator.Next(null));
    }

    [ConditionalFact]
    public void Can_create_values_for_long_types()
    {
        var generator = new TemporaryLongValueGenerator();

        Assert.Equal(long.MinValue + 1001, generator.Next(null));
        Assert.Equal(long.MinValue + 1002, generator.Next(null));
        Assert.Equal(long.MinValue + 1003, generator.Next(null));
    }

    [ConditionalFact]
    public void Can_create_values_for_short_types()
    {
        var generator = new TemporaryShortValueGenerator();

        Assert.Equal(short.MinValue + 101, generator.Next(null));
        Assert.Equal(short.MinValue + 102, generator.Next(null));
        Assert.Equal(short.MinValue + 103, generator.Next(null));
    }

    [ConditionalFact]
    public void Can_create_values_for_byte_types()
    {
        var generator = new TemporaryByteValueGenerator();

        Assert.Equal(255, generator.Next(null));
        Assert.Equal(254, generator.Next(null));
        Assert.Equal(253, generator.Next(null));
    }

    [ConditionalFact]
    public void Can_create_values_for_uint_types()
    {
        var generator = new TemporaryUIntValueGenerator();

        Assert.Equal(unchecked((uint)int.MinValue + 1001), generator.Next(null));
        Assert.Equal(unchecked((uint)int.MinValue + 1002), generator.Next(null));
        Assert.Equal(unchecked((uint)int.MinValue + 1003), generator.Next(null));
    }

    [ConditionalFact]
    public void Can_create_values_for_ulong_types()
    {
        var generator = new TemporaryULongValueGenerator();

        Assert.Equal(unchecked((ulong)long.MinValue + 1001), generator.Next(null));
        Assert.Equal(unchecked((ulong)long.MinValue + 1002), generator.Next(null));
        Assert.Equal(unchecked((ulong)long.MinValue + 1003), generator.Next(null));
    }

    [ConditionalFact]
    public void Can_create_values_for_ushort_types()
    {
        var generator = new TemporaryUShortValueGenerator();

        Assert.Equal(unchecked((ushort)short.MinValue + 101), generator.Next(null));
        Assert.Equal(unchecked((ushort)short.MinValue + 102), generator.Next(null));
        Assert.Equal(unchecked((ushort)short.MinValue + 103), generator.Next(null));
    }

    [ConditionalFact]
    public void Can_create_values_for_sbyte_types()
    {
        var generator = new TemporarySByteValueGenerator();

        Assert.Equal(-127, generator.Next(null));
        Assert.Equal(-126, generator.Next(null));
        Assert.Equal(-125, generator.Next(null));
    }

    [ConditionalFact]
    public void Can_create_values_for_char_types()
    {
        var generator = new TemporaryCharValueGenerator();

        Assert.Equal(char.MaxValue - 101, generator.Next(null));
        Assert.Equal(char.MaxValue - 102, generator.Next(null));
        Assert.Equal(char.MaxValue - 103, generator.Next(null));
    }

    [ConditionalFact]
    public void Can_create_values_for_decimal_types()
    {
        var generator = new TemporaryDecimalValueGenerator();

        Assert.Equal(-2147482647m, generator.Next(null));
        Assert.Equal(-2147482646m, generator.Next(null));
    }

    [ConditionalFact]
    public void Can_create_values_for_float_types()
    {
        var generator = new TemporaryFloatValueGenerator();

        Assert.Equal(-2147482647.0f, generator.Next(null));
        Assert.Equal(-2147482646.0f, generator.Next(null));
    }

    [ConditionalFact]
    public void Can_create_values_for_double_types()
    {
        var generator = new TemporaryDoubleValueGenerator();

        Assert.Equal(-2147482647.0, generator.Next(null));
        Assert.Equal(-2147482646.0, generator.Next(null));
    }

    [ConditionalFact]
    public void Generates_temporary_values()
    {
        Assert.True(new TemporaryIntValueGenerator().GeneratesTemporaryValues);
        Assert.True(new TemporaryLongValueGenerator().GeneratesTemporaryValues);
        Assert.True(new TemporaryShortValueGenerator().GeneratesTemporaryValues);
        Assert.True(new TemporaryByteValueGenerator().GeneratesTemporaryValues);
        Assert.True(new TemporaryUIntValueGenerator().GeneratesTemporaryValues);
        Assert.True(new TemporaryULongValueGenerator().GeneratesTemporaryValues);
        Assert.True(new TemporaryUShortValueGenerator().GeneratesTemporaryValues);
        Assert.True(new TemporarySByteValueGenerator().GeneratesTemporaryValues);
        Assert.True(new TemporaryDecimalValueGenerator().GeneratesTemporaryValues);
        Assert.True(new TemporaryDoubleValueGenerator().GeneratesTemporaryValues);
        Assert.True(new TemporaryFloatValueGenerator().GeneratesTemporaryValues);
        Assert.True(new TemporaryCharValueGenerator().GeneratesTemporaryValues);
    }
}
