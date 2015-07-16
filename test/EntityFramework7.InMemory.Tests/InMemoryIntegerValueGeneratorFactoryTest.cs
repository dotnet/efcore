// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Tests;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryIntegerValueGeneratorFactoryTest
    {
        private static readonly Model _model = TestHelpers.Instance.BuildModelFor<AnEntity>();

        [Fact]
        public void Can_create_factories_for_all_integer_types()
        {
            var entityType = _model.GetEntityType(typeof(AnEntity));

            Assert.Equal(1, CreateAndUseFactory(entityType.GetProperty("Id")));
            Assert.Equal(1L, CreateAndUseFactory(entityType.GetProperty("Long")));
            Assert.Equal((short)1, CreateAndUseFactory(entityType.GetProperty("Short")));
            Assert.Equal(unchecked((byte)1), CreateAndUseFactory(entityType.GetProperty("Byte")));
            Assert.Equal((int?)1, CreateAndUseFactory(entityType.GetProperty("NullableInt")));
            Assert.Equal((long?)1, CreateAndUseFactory(entityType.GetProperty("NullableLong")));
            Assert.Equal((short?)1, CreateAndUseFactory(entityType.GetProperty("NullableShort")));
            Assert.Equal(unchecked((byte?)1), CreateAndUseFactory(entityType.GetProperty("NullableByte")));
            Assert.Equal(unchecked((uint)1), CreateAndUseFactory(entityType.GetProperty("UInt")));
            Assert.Equal(unchecked((ulong)1), CreateAndUseFactory(entityType.GetProperty("ULong")));
            Assert.Equal(unchecked((ushort)1), CreateAndUseFactory(entityType.GetProperty("UShort")));
            Assert.Equal((sbyte)1, CreateAndUseFactory(entityType.GetProperty("SByte")));
            Assert.Equal(unchecked((uint?)1), CreateAndUseFactory(entityType.GetProperty("NullableUInt")));
            Assert.Equal(unchecked((ulong?)1), CreateAndUseFactory(entityType.GetProperty("NullableULong")));
            Assert.Equal(unchecked((ushort?)1), CreateAndUseFactory(entityType.GetProperty("NullableUShort")));
            Assert.Equal((sbyte?)1, CreateAndUseFactory(entityType.GetProperty("NullableSByte")));
        }

        private static object CreateAndUseFactory(IProperty property)
        {
            return new InMemoryIntegerValueGeneratorFactory().Create(property).Next();
        }

        [Fact]
        public void Throws_for_non_integer_property()
        {
            var property = _model.GetEntityType(typeof(AnEntity)).GetProperty("BadCheese");

            Assert.Equal(
                Internal.Strings.InvalidValueGeneratorFactoryProperty(nameof(InMemoryIntegerValueGeneratorFactory), "BadCheese", "AnEntity"),
                Assert.Throws<ArgumentException>(() => new InMemoryIntegerValueGeneratorFactory().Create(property)).Message);
        }

        private class AnEntity
        {
            public int Id { get; set; }
            public long Long { get; set; }
            public short Short { get; set; }
            public byte Byte { get; set; }
            public int? NullableInt { get; set; }
            public long? NullableLong { get; set; }
            public short? NullableShort { get; set; }
            public byte? NullableByte { get; set; }
            public uint UInt { get; set; }
            public ulong ULong { get; set; }
            public ushort UShort { get; set; }
            public sbyte SByte { get; set; }
            public uint? NullableUInt { get; set; }
            public ulong? NullableULong { get; set; }
            public ushort? NullableUShort { get; set; }
            public sbyte? NullableSByte { get; set; }
            public string BadCheese { get; set; }
        }
    }
}
