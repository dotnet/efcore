// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryIntegerValueGeneratorTest
    {
        [Fact]
        public void Creates_values()
        {
            var generator = new InMemoryIntegerValueGenerator<int>();

            Assert.Equal(1, generator.Next());
            Assert.Equal(2, generator.Next());
            Assert.Equal(3, generator.Next());
            Assert.Equal(4, generator.Next());
            Assert.Equal(5, generator.Next());
            Assert.Equal(6, generator.Next());

            generator = new InMemoryIntegerValueGenerator<int>();

            Assert.Equal(1, generator.Next());
            Assert.Equal(2, generator.Next());
        }

        [Fact]
        public void Can_create_values_for_all_integer_types()
        {
            Assert.Equal(1, new InMemoryIntegerValueGenerator<int>().Next());
            Assert.Equal(1L, new InMemoryIntegerValueGenerator<long>().Next());
            Assert.Equal((short)1, new InMemoryIntegerValueGenerator<short>().Next());
            Assert.Equal(unchecked((byte)1), new InMemoryIntegerValueGenerator<byte>().Next());
            Assert.Equal(unchecked((uint)1), new InMemoryIntegerValueGenerator<uint>().Next());
            Assert.Equal(unchecked((ulong)1), new InMemoryIntegerValueGenerator<ulong>().Next());
            Assert.Equal(unchecked((ushort)1), new InMemoryIntegerValueGenerator<ushort>().Next());
            Assert.Equal((sbyte)1, new InMemoryIntegerValueGenerator<sbyte>().Next());
        }

        [Fact]
        public void Throws_when_type_conversion_would_overflow()
        {
            var generator = new InMemoryIntegerValueGenerator<byte>();

            for (var i = 1; i < 256; i++)
            {
                generator.Next();
            }

            Assert.Throws<OverflowException>(() => generator.Next());
        }

        [Fact]
        public void Does_not_generate_temp_values()
        {
            Assert.False(new InMemoryIntegerValueGenerator<int>().GeneratesTemporaryValues);
        }
    }
}
