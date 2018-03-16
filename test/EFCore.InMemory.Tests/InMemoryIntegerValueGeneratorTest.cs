// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.InMemory.ValueGeneration.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class InMemoryIntegerValueGeneratorTest
    {
        [Fact]
        public void Creates_values()
        {
            var generator = new InMemoryIntegerValueGenerator<int>();

            Assert.Equal(1, generator.Next(null));
            Assert.Equal(2, generator.Next(null));
            Assert.Equal(3, generator.Next(null));
            Assert.Equal(4, generator.Next(null));
            Assert.Equal(5, generator.Next(null));
            Assert.Equal(6, generator.Next(null));

            generator = new InMemoryIntegerValueGenerator<int>();

            Assert.Equal(1, generator.Next(null));
            Assert.Equal(2, generator.Next(null));
        }

        [Fact]
        public void Can_create_values_for_all_integer_types()
        {
            Assert.Equal(1, new InMemoryIntegerValueGenerator<int>().Next(null));
            Assert.Equal(1L, new InMemoryIntegerValueGenerator<long>().Next(null));
            Assert.Equal((short)1, new InMemoryIntegerValueGenerator<short>().Next(null));
            Assert.Equal((byte)1, new InMemoryIntegerValueGenerator<byte>().Next(null));
            Assert.Equal((uint)1, new InMemoryIntegerValueGenerator<uint>().Next(null));
            Assert.Equal((ulong)1, new InMemoryIntegerValueGenerator<ulong>().Next(null));
            Assert.Equal((ushort)1, new InMemoryIntegerValueGenerator<ushort>().Next(null));
            Assert.Equal((sbyte)1, new InMemoryIntegerValueGenerator<sbyte>().Next(null));
        }

        [Fact]
        public void Throws_when_type_conversion_would_overflow()
        {
            var generator = new InMemoryIntegerValueGenerator<byte>();

            for (var i = 1; i < 256; i++)
            {
                generator.Next(null);
            }

            Assert.Throws<OverflowException>(() => generator.Next(null));
        }

        [Fact]
        public void Does_not_generate_temp_values()
        {
            Assert.False(new InMemoryIntegerValueGenerator<int>().GeneratesTemporaryValues);
        }
    }
}
