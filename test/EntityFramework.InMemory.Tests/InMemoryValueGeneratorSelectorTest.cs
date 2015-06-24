// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Tests;
using Microsoft.Data.Entity.ValueGeneration.Internal;
using Microsoft.Framework.DependencyInjection;
using Xunit;
using CoreStrings = Microsoft.Data.Entity.Internal.Strings;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryValueGeneratorSelectorTest
    {
        [Fact]
        public void Returns_built_in_generators_for_types_setup_for_value_generation()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(AnEntity));

            var selector = InMemoryTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.IsType<InMemoryIntegerValueGenerator<int>>(selector.Select(entityType.GetProperty("Id"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<long>>(selector.Select(entityType.GetProperty("Long"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<short>>(selector.Select(entityType.GetProperty("Short"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<byte>>(selector.Select(entityType.GetProperty("Byte"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<int>>(selector.Select(entityType.GetProperty("NullableInt"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<long>>(selector.Select(entityType.GetProperty("NullableLong"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<short>>(selector.Select(entityType.GetProperty("NullableShort"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<byte>>(selector.Select(entityType.GetProperty("NullableByte"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<uint>>(selector.Select(entityType.GetProperty("UInt"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<ulong>>(selector.Select(entityType.GetProperty("ULong"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<ushort>>(selector.Select(entityType.GetProperty("UShort"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<sbyte>>(selector.Select(entityType.GetProperty("SByte"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<uint>>(selector.Select(entityType.GetProperty("NullableUInt"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<ulong>>(selector.Select(entityType.GetProperty("NullableULong"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<ushort>>(selector.Select(entityType.GetProperty("NullableUShort"), entityType));
            Assert.IsType<InMemoryIntegerValueGenerator<sbyte>>(selector.Select(entityType.GetProperty("NullableSByte"), entityType));
            Assert.IsType<TemporaryStringValueGenerator>(selector.Select(entityType.GetProperty("String"), entityType));
            Assert.IsType<GuidValueGenerator>(selector.Select(entityType.GetProperty("Guid"), entityType));
            Assert.IsType<TemporaryBinaryValueGenerator>(selector.Select(entityType.GetProperty("Binary"), entityType));
        }

        [Fact]
        public void Throws_for_unsupported_combinations()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(AnEntity));

            var selector = InMemoryTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.Equal(
                CoreStrings.NoValueGenerator("Random", "AnEntity", typeof(Random).Name),
                Assert.Throws<NotSupportedException>(() => selector.Select(entityType.GetProperty("Random"), entityType)).Message);
        }

        private static Model BuildModel(bool generateValues = true)
        {
            var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            builder.Ignore<Random>();
            builder.Entity<AnEntity>();
            var model = builder.Model;
            var entityType = model.GetEntityType(typeof(AnEntity));
            entityType.AddProperty("Random", typeof(Random)).IsShadowProperty = false;

            foreach (var property in entityType.Properties)
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
