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

            var generatedValue = await generator.NextAsync(property, new DbContextService<DataStoreServices>(() => null));

            Assert.Equal(-1, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);

            generatedValue = await generator.NextAsync(property, new DbContextService<DataStoreServices>(() => null));

            Assert.Equal(-2, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);

            generatedValue = await generator.NextAsync(property, new DbContextService<DataStoreServices>(() => null));

            Assert.Equal(-3, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);

            generatedValue = generator.Next(property, new DbContextService<DataStoreServices>(() => null));

            Assert.Equal(-4, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);

            generatedValue = generator.Next(property, new DbContextService<DataStoreServices>(() => null));

            Assert.Equal(-5, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);

            generatedValue = generator.Next(property, new DbContextService<DataStoreServices>(() => null));

            Assert.Equal(-6, generatedValue.Value);
            Assert.True(generatedValue.IsTemporary);
        }

        [Fact]
        public void Can_create_values_for_all_integer_types()
        {
            var entityType = _model.GetEntityType(typeof(AnEntity));

            var generator = new TemporaryIntegerValueGenerator();

            Assert.Equal(-1, generator.Next(entityType.GetProperty("Id")).Value);
            Assert.Equal(-2L, generator.Next(entityType.GetProperty("Long")).Value);
            Assert.Equal((short)-3, generator.Next(entityType.GetProperty("Short")).Value);
            Assert.Equal(unchecked((byte)-4), generator.Next(entityType.GetProperty("Byte")).Value);
            Assert.Equal((int?)-5, generator.Next(entityType.GetProperty("NullableInt")).Value);
            Assert.Equal((long?)-6, generator.Next(entityType.GetProperty("NullableLong")).Value);
            Assert.Equal((short?)-7, generator.Next(entityType.GetProperty("NullableShort")).Value);
            Assert.Equal(unchecked((byte?)-8), generator.Next(entityType.GetProperty("NullableByte")).Value);
            Assert.Equal(unchecked((uint)-9), generator.Next(entityType.GetProperty("UInt")).Value);
            Assert.Equal(unchecked((ulong)-10), generator.Next(entityType.GetProperty("ULong")).Value);
            Assert.Equal(unchecked((ushort)-11), generator.Next(entityType.GetProperty("UShort")).Value);
            Assert.Equal((sbyte)-12, generator.Next(entityType.GetProperty("SByte")).Value);
            Assert.Equal(unchecked((uint?)-13), generator.Next(entityType.GetProperty("NullableUInt")).Value);
            Assert.Equal(unchecked((ulong?)-14), generator.Next(entityType.GetProperty("NullableULong")).Value);
            Assert.Equal(unchecked((ushort?)-15), generator.Next(entityType.GetProperty("NullableUShort")).Value);
            Assert.Equal((sbyte?)-16, generator.Next(entityType.GetProperty("NullableSByte")).Value);
        }

        [Fact]
        public void Generates_temporary_values()
        {
            var entityType = _model.GetEntityType(typeof(AnEntity));

            var generator = new TemporaryIntegerValueGenerator();

            Assert.True(generator.Next(entityType.GetProperty("Id")).IsTemporary);
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
