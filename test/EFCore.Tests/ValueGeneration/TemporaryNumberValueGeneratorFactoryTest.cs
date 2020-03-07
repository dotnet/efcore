// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    public class TemporaryNumberValueGeneratorFactoryTest
    {
        private static readonly IMutableModel _model = InMemoryTestHelpers.Instance.BuildModelFor<AnEntity>();

        [ConditionalFact]
        public void Can_create_factories_for_all_integer_types()
        {
            var entityType = _model.FindEntityType(typeof(AnEntity));

            Assert.Equal(int.MinValue + 1001, CreateAndUseFactory(entityType.FindProperty("Id")));
            Assert.Equal(long.MinValue + 1001, CreateAndUseFactory(entityType.FindProperty("Long")));
            Assert.Equal((short)(short.MinValue + 101), CreateAndUseFactory(entityType.FindProperty("Short")));
            Assert.Equal((byte)255, CreateAndUseFactory(entityType.FindProperty("Byte")));
            Assert.Equal(int.MinValue + 1001, CreateAndUseFactory(entityType.FindProperty("NullableInt")));
            Assert.Equal(long.MinValue + 1001, CreateAndUseFactory(entityType.FindProperty("NullableLong")));
            Assert.Equal((short)(short.MinValue + 101), CreateAndUseFactory(entityType.FindProperty("NullableShort")));
            Assert.Equal((byte)255, CreateAndUseFactory(entityType.FindProperty("NullableByte")));
            Assert.Equal(unchecked((uint)(int.MinValue + 1001)), CreateAndUseFactory(entityType.FindProperty("UInt")));
            Assert.Equal(unchecked((ulong)(long.MinValue + 1001)), CreateAndUseFactory(entityType.FindProperty("ULong")));
            Assert.Equal(unchecked((ushort)(short.MinValue + 101)), CreateAndUseFactory(entityType.FindProperty("UShort")));
            Assert.Equal((sbyte)-127, CreateAndUseFactory(entityType.FindProperty("SByte")));
            Assert.Equal(unchecked((uint)(int.MinValue + 1001)), CreateAndUseFactory(entityType.FindProperty("NullableUInt")));
            Assert.Equal(unchecked((ulong)(long.MinValue + 1001)), CreateAndUseFactory(entityType.FindProperty("NullableULong")));
            Assert.Equal(unchecked((ushort)(short.MinValue + 101)), CreateAndUseFactory(entityType.FindProperty("NullableUShort")));
            Assert.Equal((sbyte)-127, CreateAndUseFactory(entityType.FindProperty("NullableSByte")));
        }

        private static object CreateAndUseFactory(IProperty property)
            => new TemporaryNumberValueGeneratorFactory().Create(property).Next(null);

        [ConditionalFact]
        public void Throws_for_non_integer_property()
        {
            var property = _model.FindEntityType(typeof(AnEntity)).FindProperty("BadCheese");

            Assert.Equal(
                CoreStrings.InvalidValueGeneratorFactoryProperty(nameof(TemporaryNumberValueGeneratorFactory), "BadCheese", "AnEntity"),
                Assert.Throws<ArgumentException>(() => new TemporaryNumberValueGeneratorFactory().Create(property)).Message);
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
