// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.ValueGeneration;

public class InMemoryIntegerValueGeneratorTest
{
    [ConditionalFact]
    public void Creates_values()
    {
        var generator = new InMemoryIntegerValueGenerator<int>(0);

        Assert.Equal(1, generator.Next(null));
        Assert.Equal(2, generator.Next(null));
        Assert.Equal(3, generator.Next(null));
        Assert.Equal(4, generator.Next(null));
        Assert.Equal(5, generator.Next(null));
        Assert.Equal(6, generator.Next(null));

        generator = new InMemoryIntegerValueGenerator<int>(0);

        Assert.Equal(1, generator.Next(null));
        Assert.Equal(2, generator.Next(null));
    }

    [ConditionalFact]
    public void Can_create_values_for_all_integer_types()
    {
        Assert.Equal(1, new InMemoryIntegerValueGenerator<int>(0).Next(null));
        Assert.Equal(1L, new InMemoryIntegerValueGenerator<long>(0).Next(null));
        Assert.Equal((short)1, new InMemoryIntegerValueGenerator<short>(0).Next(null));
        Assert.Equal((byte)1, new InMemoryIntegerValueGenerator<byte>(0).Next(null));
        Assert.Equal((uint)1, new InMemoryIntegerValueGenerator<uint>(0).Next(null));
        Assert.Equal((ulong)1, new InMemoryIntegerValueGenerator<ulong>(0).Next(null));
        Assert.Equal((ushort)1, new InMemoryIntegerValueGenerator<ushort>(0).Next(null));
        Assert.Equal((sbyte)1, new InMemoryIntegerValueGenerator<sbyte>(0).Next(null));
    }

    [ConditionalFact]
    public void Throws_when_type_conversion_would_overflow()
    {
        var generator = new InMemoryIntegerValueGenerator<byte>(0);

        for (var i = 1; i < 256; i++)
        {
            generator.Next(null);
        }

        Assert.Throws<OverflowException>(() => generator.Next(null));
    }

    [ConditionalFact]
    public void Does_not_generate_temp_values()
        => Assert.False(new InMemoryIntegerValueGenerator<int>(0).GeneratesTemporaryValues);
}
