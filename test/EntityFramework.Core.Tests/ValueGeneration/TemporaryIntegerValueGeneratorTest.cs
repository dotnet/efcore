// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.ValueGeneration;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ValueGeneration
{
    public class TemporaryIntegerValueGeneratorTest
    {
        [Fact]
        public void Creates_negative_values()
        {
            var generator = new TemporaryIntegerValueGenerator<int>();

            Assert.Equal(-1, generator.Next(new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal(-2, generator.Next(new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal(-3, generator.Next(new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal(-4, generator.Next(new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal(-5, generator.Next(new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal(-6, generator.Next(new DbContextService<DataStoreServices>(() => null)));
        }

        [Fact]
        public void Can_create_values_for_all_integer_types()
        {
            Assert.Equal(-1, new TemporaryIntegerValueGenerator<int>().Next());
            Assert.Equal(-1L, new TemporaryIntegerValueGenerator<long>().Next());
            Assert.Equal((short)-1, new TemporaryIntegerValueGenerator<short>().Next());
            Assert.Equal(unchecked((byte)-1), new TemporaryIntegerValueGenerator<byte>().Next());
            Assert.Equal(unchecked((uint)-1), new TemporaryIntegerValueGenerator<uint>().Next());
            Assert.Equal(unchecked((ulong)-1), new TemporaryIntegerValueGenerator<ulong>().Next());
            Assert.Equal(unchecked((ushort)-1), new TemporaryIntegerValueGenerator<ushort>().Next());
            Assert.Equal((sbyte)-1, new TemporaryIntegerValueGenerator<sbyte>().Next());
        }

        [Fact]
        public void Generates_temporary_values()
        {
            Assert.True(new TemporaryIntegerValueGenerator<int>().GeneratesTemporaryValues);
        }
    }
}
