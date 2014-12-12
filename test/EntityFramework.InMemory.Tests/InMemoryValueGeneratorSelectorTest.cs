// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Tests;
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

            var selector = TestHelpers.CreateContextServices(model).GetRequiredService<InMemoryValueGeneratorSelector>();

            Assert.IsType<SimpleValueGeneratorFactory<InMemoryValueGenerator>>(selector.Select(entityType.GetProperty("Id")));
            Assert.IsType<SimpleValueGeneratorFactory<InMemoryValueGenerator>>(selector.Select(entityType.GetProperty("Long")));
            Assert.IsType<SimpleValueGeneratorFactory<InMemoryValueGenerator>>(selector.Select(entityType.GetProperty("Short")));
            Assert.IsType<SimpleValueGeneratorFactory<InMemoryValueGenerator>>(selector.Select(entityType.GetProperty("Byte")));
            Assert.IsType<SimpleValueGeneratorFactory<InMemoryValueGenerator>>(selector.Select(entityType.GetProperty("NullableInt")));
            Assert.IsType<SimpleValueGeneratorFactory<InMemoryValueGenerator>>(selector.Select(entityType.GetProperty("NullableLong")));
            Assert.IsType<SimpleValueGeneratorFactory<InMemoryValueGenerator>>(selector.Select(entityType.GetProperty("NullableShort")));
            Assert.IsType<SimpleValueGeneratorFactory<InMemoryValueGenerator>>(selector.Select(entityType.GetProperty("NullableByte")));
            Assert.IsType<SimpleValueGeneratorFactory<InMemoryValueGenerator>>(selector.Select(entityType.GetProperty("UInt")));
            Assert.IsType<SimpleValueGeneratorFactory<InMemoryValueGenerator>>(selector.Select(entityType.GetProperty("ULong")));
            Assert.IsType<SimpleValueGeneratorFactory<InMemoryValueGenerator>>(selector.Select(entityType.GetProperty("UShort")));
            Assert.IsType<SimpleValueGeneratorFactory<InMemoryValueGenerator>>(selector.Select(entityType.GetProperty("SByte")));
            Assert.IsType<SimpleValueGeneratorFactory<InMemoryValueGenerator>>(selector.Select(entityType.GetProperty("NullableUInt")));
            Assert.IsType<SimpleValueGeneratorFactory<InMemoryValueGenerator>>(selector.Select(entityType.GetProperty("NullableULong")));
            Assert.IsType<SimpleValueGeneratorFactory<InMemoryValueGenerator>>(selector.Select(entityType.GetProperty("NullableUShort")));
            Assert.IsType<SimpleValueGeneratorFactory<InMemoryValueGenerator>>(selector.Select(entityType.GetProperty("NullableSByte")));
            Assert.IsType<SimpleValueGeneratorFactory<TemporaryStringValueGenerator>>(selector.Select(entityType.GetProperty("String")));
            Assert.IsType<SimpleValueGeneratorFactory<GuidValueGenerator>>(selector.Select(entityType.GetProperty("Guid")));
            Assert.IsType<SimpleValueGeneratorFactory<TemporaryBinaryValueGenerator>>(selector.Select(entityType.GetProperty("Binary")));
        }

        [Fact]
        public void Throws_for_unsupported_combinations()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(AnEntity));

            var selector = TestHelpers.CreateContextServices(model).GetRequiredService<InMemoryValueGeneratorSelector>();

            Assert.Equal(
                CoreStrings.NoValueGenerator("Float", "AnEntity", typeof(float).Name),
                Assert.Throws<NotSupportedException>(() => selector.Select(entityType.GetProperty("Float"))).Message);
        }

        private static Model BuildModel(bool generateValues = true)
        {
            var model = TestHelpers.BuildModelFor<AnEntity>();
            var entityType = model.GetEntityType(typeof(AnEntity));

            foreach (var property in entityType.Properties)
            {
                property.GenerateValueOnAdd = generateValues;
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
        }
    }
}
