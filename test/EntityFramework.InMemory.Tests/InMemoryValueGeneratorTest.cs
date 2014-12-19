// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryValueGeneratorTest
    {
        private static readonly Model _model = TestHelpers.BuildModelFor<AnEntity>();
        
        [Fact]
        public async Task Creates_values()
        {
            var property = _model.GetEntityType(typeof(AnEntity)).GetProperty("Id");
            var generator = new InMemoryValueGenerator();

            Assert.Equal(1, await generator.NextAsync(property, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal(2, await generator.NextAsync(property, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal(3, await generator.NextAsync(property, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal(4, generator.Next(property, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal(5, generator.Next(property, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal(6, generator.Next(property, new DbContextService<DataStoreServices>(() => null)));

            generator = new InMemoryValueGenerator();

            Assert.Equal(1, await generator.NextAsync(property, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal(2, await generator.NextAsync(property, new DbContextService<DataStoreServices>(() => null)));
        }

        [Fact]
        public async Task Can_create_values_for_all_integer_types()
        {
            var entityType = _model.GetEntityType(typeof(AnEntity));

            var intProperty = entityType.GetProperty("Id");
            var longProperty = entityType.GetProperty("Long");
            var shortProperty = entityType.GetProperty("Short");
            var byteProperty = entityType.GetProperty("Byte");
            var uintProperty = entityType.GetProperty("UnsignedInt");
            var ulongProperty = entityType.GetProperty("UnsignedLong");
            var ushortProperty = entityType.GetProperty("UnsignedShort");
            var sbyteProperty = entityType.GetProperty("SignedByte");

            var generator = new InMemoryValueGenerator();

            Assert.Equal(1L, await generator.NextAsync(longProperty, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal(2, await generator.NextAsync(intProperty, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal((short)3, await generator.NextAsync(shortProperty, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal((byte)4, await generator.NextAsync(byteProperty, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal((ulong)5, await generator.NextAsync(ulongProperty, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal((uint)6, await generator.NextAsync(uintProperty, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal((ushort)7, await generator.NextAsync(ushortProperty, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal((sbyte)8, await generator.NextAsync(sbyteProperty, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal(9L, generator.Next(longProperty, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal(10, generator.Next(intProperty, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal((short)11, generator.Next(shortProperty, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal((byte)12, generator.Next(byteProperty, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal((ulong)13, generator.Next(ulongProperty, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal((uint)14, generator.Next(uintProperty, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal((ushort)15, generator.Next(ushortProperty, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal((sbyte)16, generator.Next(sbyteProperty, new DbContextService<DataStoreServices>(() => null)));
        }

        [Fact]
        public void Throws_when_type_conversion_would_overflow()
        {
            var generator = new InMemoryValueGenerator();
            var property = CreateProperty(typeof(byte));

            for (var i = 1; i < 256; i++)
            {
                generator.Next(property, new DbContextService<DataStoreServices>(() => null));
            }

            Assert.Throws<OverflowException>(() => generator.Next(property, new DbContextService<DataStoreServices>(() => null)));
        }

        [Fact]
        public void Does_not_generate_temp_values()
        {
            Assert.False(new InMemoryValueGenerator().GeneratesTemporaryValues);
        }

        private static Property CreateProperty(Type propertyType)
        {
            var entityType = new Model().AddEntityType("MyType");
            return entityType.GetOrAddProperty("MyProperty", propertyType, shadowProperty: true);
        }

        private class AnEntity
        {
            public int Id { get; set; }
            public long Long { get; set; }
            public short Short { get; set; }
            public byte Byte { get; set; }
            public uint UnsignedInt { get; set; }
            public ulong UnsignedLong { get; set; }
            public ushort UnsignedShort { get; set; }
            public sbyte SignedByte { get; set; }
        }
    }
}
