// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ValueGeneration;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ValueGeneration
{
    public class TemporaryNumberValueGeneratorTest
    {
        [Fact]
        public void Creates_negative_values()
        {
            var generator = new TemporaryNumberValueGenerator<int>();

            Assert.Equal(-1, generator.Next());
            Assert.Equal(-2, generator.Next());
            Assert.Equal(-3, generator.Next());
            Assert.Equal(-4, generator.Next());
            Assert.Equal(-5, generator.Next());
            Assert.Equal(-6, generator.Next());
        }

        [Fact]
        public void Can_create_values_for_all_integer_types()
        {
            Assert.Equal(-1, new TemporaryNumberValueGenerator<int>().Next());
            Assert.Equal(-1L, new TemporaryNumberValueGenerator<long>().Next());
            Assert.Equal((short)-1, new TemporaryNumberValueGenerator<short>().Next());
            Assert.Equal(unchecked((byte)-1), new TemporaryNumberValueGenerator<byte>().Next());
            Assert.Equal(unchecked((uint)-1), new TemporaryNumberValueGenerator<uint>().Next());
            Assert.Equal(unchecked((ulong)-1), new TemporaryNumberValueGenerator<ulong>().Next());
            Assert.Equal(unchecked((ushort)-1), new TemporaryNumberValueGenerator<ushort>().Next());
            Assert.Equal((sbyte)-1, new TemporaryNumberValueGenerator<sbyte>().Next());
        }

        [Fact]
        public void Can_create_values_for_decimal_types()
        {
            var generator = new TemporaryNumberValueGenerator<decimal>();

            Assert.Equal(-1m, generator.Next());
            Assert.Equal(-2m, generator.Next());
        }

        [Fact]
        public void Can_create_values_for_float_types()
        {
            var generator = new TemporaryNumberValueGenerator<float>();

            Assert.Equal(-1.0f, generator.Next());
            Assert.Equal(-2.0f, generator.Next());
        }

        [Fact]
        public void Can_create_values_for_double_types()
        {
            var generator = new TemporaryNumberValueGenerator<double>();

            Assert.Equal(-1.0, generator.Next());
            Assert.Equal(-2.0, generator.Next());
        }

        [Fact]
        public void Generates_temporary_values()
        {
            Assert.True(new TemporaryNumberValueGenerator<int>().GeneratesTemporaryValues);
        }
    }
}
