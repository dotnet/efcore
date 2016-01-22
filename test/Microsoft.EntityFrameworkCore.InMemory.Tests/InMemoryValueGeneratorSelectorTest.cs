// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Tests;
using Microsoft.Data.Entity.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryValueGeneratorSelectorTest
    {
        [Fact]
        public void Returns_built_in_generators_for_types_setup_for_value_generation()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(AnEntity));

            var selector = InMemoryTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.IsType<InMemoryIntegerValueGenerator<int>>(selector.Select(entityType.FindProperty("Id"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<long>>(selector.Select(entityType.FindProperty("Long"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<short>>(selector.Select(entityType.FindProperty("Short"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<byte>>(selector.Select(entityType.FindProperty("Byte"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<int>>(selector.Select(entityType.FindProperty("NullableInt"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<long>>(selector.Select(entityType.FindProperty("NullableLong"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<short>>(selector.Select(entityType.FindProperty("NullableShort"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<byte>>(selector.Select(entityType.FindProperty("NullableByte"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<uint>>(selector.Select(entityType.FindProperty("UInt"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<ulong>>(selector.Select(entityType.FindProperty("ULong"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<ushort>>(selector.Select(entityType.FindProperty("UShort"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<sbyte>>(selector.Select(entityType.FindProperty("SByte"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<uint>>(selector.Select(entityType.FindProperty("NullableUInt"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<ulong>>(selector.Select(entityType.FindProperty("NullableULong"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<ushort>>(selector.Select(entityType.FindProperty("NullableUShort"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<sbyte>>(selector.Select(entityType.FindProperty("NullableSByte"), entityType));
            Assert.IsType<TemporaryStringValueGenerator>(selector.Select(entityType.FindProperty("String"), entityType));
            Assert.IsType<GuidValueGenerator>(selector.Select(entityType.FindProperty("Guid"), entityType));
            Assert.IsType<TemporaryBinaryValueGenerator>(selector.Select(entityType.FindProperty("Binary"), entityType));
        }

        [Fact]
        public void Throws_for_unsupported_combinations()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(AnEntity));

            var selector = InMemoryTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.Equal(
                CoreStrings.NoValueGenerator("Random", "AnEntity", typeof(Random).Name),
                Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("Random"), entityType)).Message);
        }

        private static IMutableModel BuildModel(bool generateValues = true)
        {
            var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            builder.Ignore<Random>();
            builder.Entity<AnEntity>();
            var model = builder.Model;
            var entityType = model.FindEntityType(typeof(AnEntity));
            entityType.AddProperty("Random", typeof(Random)).IsShadowProperty = false;

            foreach (var property in entityType.GetProperties())
            {
                property.RequiresValueGenerator = generateValues;
            }

            return model;
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
            public string String { get; set; }
            public Guid Guid { get; set; }
            public byte[] Binary { get; set; }
            public float Float { get; set; }
            public Random Random { get; set; }
        }
    }
}
