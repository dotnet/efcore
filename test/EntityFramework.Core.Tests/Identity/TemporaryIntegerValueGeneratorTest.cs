// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Identity
{
    public class TemporaryIntegerValueGeneratorTest
    {
        private static readonly Model _model = TestHelpers.BuildModelFor<AnEntity>();

        [Fact]
        public async Task Creates_negative_values()
        {
            var property = _model.GetEntityType(typeof(AnEntity)).GetProperty("Id");
            var generator = new TemporaryIntegerValueGenerator();

            Assert.Equal(-1, await generator.NextAsync(property, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal(-2, await generator.NextAsync(property, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal(-3, await generator.NextAsync(property, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal(-4, generator.Next(property, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal(-5, generator.Next(property, new DbContextService<DataStoreServices>(() => null)));
            Assert.Equal(-6, generator.Next(property, new DbContextService<DataStoreServices>(() => null)));
        }

        [Fact]
        public void Can_create_values_for_all_integer_types()
        {
            var entityType = _model.GetEntityType(typeof(AnEntity));
            var generator = new TemporaryIntegerValueGenerator();

            Assert.Equal(-1, generator.Next(entityType.GetProperty("Id")));
            Assert.Equal(-2L, generator.Next(entityType.GetProperty("Long")));
            Assert.Equal((short)-3, generator.Next(entityType.GetProperty("Short")));
            Assert.Equal(unchecked((byte)-4), generator.Next(entityType.GetProperty("Byte")));
            Assert.Equal((int?)-5, generator.Next(entityType.GetProperty("NullableInt")));
            Assert.Equal((long?)-6, generator.Next(entityType.GetProperty("NullableLong")));
            Assert.Equal((short?)-7, generator.Next(entityType.GetProperty("NullableShort")));
            Assert.Equal(unchecked((byte?)-8), generator.Next(entityType.GetProperty("NullableByte")));
            Assert.Equal(unchecked((uint)-9), generator.Next(entityType.GetProperty("UInt")));
            Assert.Equal(unchecked((ulong)-10), generator.Next(entityType.GetProperty("ULong")));
            Assert.Equal(unchecked((ushort)-11), generator.Next(entityType.GetProperty("UShort")));
            Assert.Equal((sbyte)-12, generator.Next(entityType.GetProperty("SByte")));
            Assert.Equal(unchecked((uint?)-13), generator.Next(entityType.GetProperty("NullableUInt")));
            Assert.Equal(unchecked((ulong?)-14), generator.Next(entityType.GetProperty("NullableULong")));
            Assert.Equal(unchecked((ushort?)-15), generator.Next(entityType.GetProperty("NullableUShort")));
            Assert.Equal((sbyte?)-16, generator.Next(entityType.GetProperty("NullableSByte")));
        }

        [Fact]
        public void Generates_temporary_values()
        {
            Assert.True(new TemporaryIntegerValueGenerator().GeneratesTemporaryValues);
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
        }
    }
}
