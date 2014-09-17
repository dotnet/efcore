// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Tests;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryValueGeneratorTest
    {
        private static readonly Model _model = TestHelpers.BuildModelFor<AnEntity>();

        [Fact]
        public async Task Creates_values()
        {
            var generator = new InMemoryValueGenerator();

            var stateEntry = TestHelpers.CreateStateEntry<AnEntity>(_model, EntityState.Added);
            var property = stateEntry.EntityType.GetProperty("Id");

            await generator.NextAsync(stateEntry, property);

            Assert.Equal(1, stateEntry[property]);
            Assert.False(stateEntry.HasTemporaryValue(property));

            await generator.NextAsync(stateEntry, property);

            Assert.Equal(2, stateEntry[property]);
            Assert.False(stateEntry.HasTemporaryValue(property));

            await generator.NextAsync(stateEntry, property);

            Assert.Equal(3, stateEntry[property]);
            Assert.False(stateEntry.HasTemporaryValue(property));

            generator.Next(stateEntry, property);

            Assert.Equal(4, stateEntry[property]);
            Assert.False(stateEntry.HasTemporaryValue(property));

            generator.Next(stateEntry, property);

            Assert.Equal(5, stateEntry[property]);
            Assert.False(stateEntry.HasTemporaryValue(property));

            generator.Next(stateEntry, property);

            Assert.Equal(6, stateEntry[property]);
            Assert.False(stateEntry.HasTemporaryValue(property));

            generator = new InMemoryValueGenerator();

            await generator.NextAsync(stateEntry, property);

            Assert.Equal(1, stateEntry[property]);
            Assert.False(stateEntry.HasTemporaryValue(property));

            await generator.NextAsync(stateEntry, property);

            Assert.Equal(2, stateEntry[property]);
            Assert.False(stateEntry.HasTemporaryValue(property));
        }

        [Fact]
        public async Task Can_create_values_for_all_integer_types()
        {
            var generator = new InMemoryValueGenerator();

            var stateEntry = TestHelpers.CreateStateEntry<AnEntity>(_model, EntityState.Added);
            var intProperty = stateEntry.EntityType.GetProperty("Id");
            var longProperty = stateEntry.EntityType.GetProperty("Long");
            var shortProperty = stateEntry.EntityType.GetProperty("Short");
            var byteProperty = stateEntry.EntityType.GetProperty("Byte");
            var uintProperty = stateEntry.EntityType.GetProperty("UnsignedInt");
            var ulongProperty = stateEntry.EntityType.GetProperty("UnsignedLong");
            var ushortProperty = stateEntry.EntityType.GetProperty("UnsignedShort");
            var sbyteProperty = stateEntry.EntityType.GetProperty("SignedByte");

            await generator.NextAsync(stateEntry, longProperty);

            Assert.Equal(1L, stateEntry[longProperty]);
            Assert.False(stateEntry.HasTemporaryValue(longProperty));

            await generator.NextAsync(stateEntry, intProperty);

            Assert.Equal(2, stateEntry[intProperty]);
            Assert.False(stateEntry.HasTemporaryValue(intProperty));

            await generator.NextAsync(stateEntry, shortProperty);

            Assert.Equal((short)3, stateEntry[shortProperty]);
            Assert.False(stateEntry.HasTemporaryValue(shortProperty));

            await generator.NextAsync(stateEntry, byteProperty);

            Assert.Equal((byte)4, stateEntry[byteProperty]);
            Assert.False(stateEntry.HasTemporaryValue(byteProperty));

            await generator.NextAsync(stateEntry, ulongProperty);

            Assert.Equal((ulong)5, stateEntry[ulongProperty]);
            Assert.False(stateEntry.HasTemporaryValue(ulongProperty));

            await generator.NextAsync(stateEntry, uintProperty);

            Assert.Equal((uint)6, stateEntry[uintProperty]);
            Assert.False(stateEntry.HasTemporaryValue(uintProperty));

            await generator.NextAsync(stateEntry, ushortProperty);

            Assert.Equal((ushort)7, stateEntry[ushortProperty]);
            Assert.False(stateEntry.HasTemporaryValue(ushortProperty));

            await generator.NextAsync(stateEntry, sbyteProperty);

            Assert.Equal((sbyte)8, stateEntry[sbyteProperty]);
            Assert.False(stateEntry.HasTemporaryValue(sbyteProperty));

            generator.Next(stateEntry, longProperty);

            Assert.Equal(9L, stateEntry[longProperty]);
            Assert.False(stateEntry.HasTemporaryValue(longProperty));

            generator.Next(stateEntry, intProperty);

            Assert.Equal(10, stateEntry[intProperty]);
            Assert.False(stateEntry.HasTemporaryValue(intProperty));

            generator.Next(stateEntry, shortProperty);

            Assert.Equal((short)11, stateEntry[shortProperty]);
            Assert.False(stateEntry.HasTemporaryValue(shortProperty));

            generator.Next(stateEntry, byteProperty);

            Assert.Equal((byte)12, stateEntry[byteProperty]);
            Assert.False(stateEntry.HasTemporaryValue(byteProperty));

            generator.Next(stateEntry, ulongProperty);

            Assert.Equal((ulong)13, stateEntry[ulongProperty]);
            Assert.False(stateEntry.HasTemporaryValue(ulongProperty));

            generator.Next(stateEntry, uintProperty);

            Assert.Equal((uint)14, stateEntry[uintProperty]);
            Assert.False(stateEntry.HasTemporaryValue(uintProperty));

            generator.Next(stateEntry, ushortProperty);

            Assert.Equal((ushort)15, stateEntry[ushortProperty]);
            Assert.False(stateEntry.HasTemporaryValue(ushortProperty));

            generator.Next(stateEntry, sbyteProperty);

            Assert.Equal((sbyte)16, stateEntry[sbyteProperty]);
            Assert.False(stateEntry.HasTemporaryValue(sbyteProperty));
        }

        [Fact]
        public void Throws_when_type_conversion_would_overflow()
        {
            var generator = new InMemoryValueGenerator();
            var property = CreateProperty(typeof(byte));

            for (var i = 1; i < 256; i++)
            {
                generator.Next(Mock.Of<StateEntry>(), property);
            }

            Assert.Throws<OverflowException>(() => generator.Next(Mock.Of<StateEntry>(), property));
        }

        private static Property CreateProperty(Type propertyType)
        {
            var entityType = new EntityType("MyType");
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
