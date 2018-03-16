// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.InMemory.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class InMemoryIntegerValueGeneratorFactoryTest
    {
        private static readonly IMutableModel _model = InMemoryTestHelpers.Instance.BuildModelFor<AnEntity>();

        [Fact]
        public void Can_create_factories_for_all_integer_types()
        {
            var entityType = _model.FindEntityType(typeof(AnEntity));

            Assert.Equal(1, CreateAndUseFactory(entityType.FindProperty("Id")));
            Assert.Equal(1L, CreateAndUseFactory(entityType.FindProperty("Long")));
            Assert.Equal((short)1, CreateAndUseFactory(entityType.FindProperty("Short")));
            Assert.Equal((byte)1, CreateAndUseFactory(entityType.FindProperty("Byte")));
            Assert.Equal((int?)1, CreateAndUseFactory(entityType.FindProperty("NullableInt")));
            Assert.Equal((long?)1, CreateAndUseFactory(entityType.FindProperty("NullableLong")));
            Assert.Equal((short?)1, CreateAndUseFactory(entityType.FindProperty("NullableShort")));
            Assert.Equal((byte?)1, CreateAndUseFactory(entityType.FindProperty("NullableByte")));
            Assert.Equal((uint)1, CreateAndUseFactory(entityType.FindProperty("UInt")));
            Assert.Equal((ulong)1, CreateAndUseFactory(entityType.FindProperty("ULong")));
            Assert.Equal((ushort)1, CreateAndUseFactory(entityType.FindProperty("UShort")));
            Assert.Equal((sbyte)1, CreateAndUseFactory(entityType.FindProperty("SByte")));
            Assert.Equal((uint?)1, CreateAndUseFactory(entityType.FindProperty("NullableUInt")));
            Assert.Equal((ulong?)1, CreateAndUseFactory(entityType.FindProperty("NullableULong")));
            Assert.Equal((ushort?)1, CreateAndUseFactory(entityType.FindProperty("NullableUShort")));
            Assert.Equal((sbyte?)1, CreateAndUseFactory(entityType.FindProperty("NullableSByte")));
        }

        private static object CreateAndUseFactory(IProperty property)
            => new InMemoryIntegerValueGeneratorFactory().Create(property).Next(null);

        [Fact]
        public void Throws_for_non_integer_property()
        {
            var property = _model.FindEntityType(typeof(AnEntity)).FindProperty("BadCheese");

            Assert.Equal(
                CoreStrings.InvalidValueGeneratorFactoryProperty(nameof(InMemoryIntegerValueGeneratorFactory), "BadCheese", "AnEntity"),
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
