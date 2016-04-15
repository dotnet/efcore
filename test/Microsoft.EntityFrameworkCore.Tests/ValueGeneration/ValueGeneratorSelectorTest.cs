// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.ValueGeneration
{
    public class ValueGeneratorSelectorTest
    {
        [Fact]
        public void Returns_built_in_generators_for_types_setup_for_value_generation()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(AnEntity));

            var contextServices = TestHelpers.Instance.CreateContextServices(model);
            var selector = contextServices.GetRequiredService<ValueGeneratorSelector>();

            Assert.IsType<TemporaryIntValueGenerator>(selector.Select(entityType.FindProperty("Id"), entityType));
            Assert.IsType<TemporaryLongValueGenerator>(selector.Select(entityType.FindProperty("Long"), entityType));
            Assert.IsType<TemporaryShortValueGenerator>(selector.Select(entityType.FindProperty("Short"), entityType));
            Assert.IsType<TemporaryByteValueGenerator>(selector.Select(entityType.FindProperty("Byte"), entityType));

            Assert.IsType<TemporaryIntValueGenerator>(selector.Select(entityType.FindProperty("NullableInt"), entityType));
            Assert.IsType<TemporaryLongValueGenerator>(selector.Select(entityType.FindProperty("NullableLong"), entityType));
            Assert.IsType<TemporaryShortValueGenerator>(selector.Select(entityType.FindProperty("NullableShort"), entityType));
            Assert.IsType<TemporaryByteValueGenerator>(selector.Select(entityType.FindProperty("NullableByte"), entityType));

            Assert.IsType<TemporaryUIntValueGenerator>(selector.Select(entityType.FindProperty("UInt"), entityType));
            Assert.IsType<TemporaryULongValueGenerator>(selector.Select(entityType.FindProperty("ULong"), entityType));
            Assert.IsType<TemporaryUShortValueGenerator>(selector.Select(entityType.FindProperty("UShort"), entityType));
            Assert.IsType<TemporarySByteValueGenerator>(selector.Select(entityType.FindProperty("SByte"), entityType));

            Assert.IsType<TemporaryUIntValueGenerator>(selector.Select(entityType.FindProperty("NullableUInt"), entityType));
            Assert.IsType<TemporaryULongValueGenerator>(selector.Select(entityType.FindProperty("NullableULong"), entityType));
            Assert.IsType<TemporaryUShortValueGenerator>(selector.Select(entityType.FindProperty("NullableUShort"), entityType));
            Assert.IsType<TemporarySByteValueGenerator>(selector.Select(entityType.FindProperty("NullableSByte"), entityType));

            Assert.IsType<TemporaryDecimalValueGenerator>(selector.Select(entityType.FindProperty("Decimal"), entityType));
            Assert.IsType<TemporaryDecimalValueGenerator>(selector.Select(entityType.FindProperty("NullableDecimal"), entityType));

            Assert.IsType<TemporaryFloatValueGenerator>(selector.Select(entityType.FindProperty("Float"), entityType));
            Assert.IsType<TemporaryFloatValueGenerator>(selector.Select(entityType.FindProperty("NullableFloat"), entityType));

            Assert.IsType<TemporaryDoubleValueGenerator>(selector.Select(entityType.FindProperty("Double"), entityType));
            Assert.IsType<TemporaryDoubleValueGenerator>(selector.Select(entityType.FindProperty("NullableDouble"), entityType));

            Assert.IsType<TemporaryDateTimeValueGenerator>(selector.Select(entityType.FindProperty("DateTime"), entityType));
            Assert.IsType<TemporaryDateTimeValueGenerator>(selector.Select(entityType.FindProperty("NullableDateTime"), entityType));

            Assert.IsType<TemporaryDateTimeOffsetValueGenerator>(selector.Select(entityType.FindProperty("DateTimeOffset"), entityType));
            Assert.IsType<TemporaryDateTimeOffsetValueGenerator>(selector.Select(entityType.FindProperty("NullableDateTimeOffset"), entityType));

            Assert.IsType<TemporaryStringValueGenerator>(selector.Select(entityType.FindProperty("String"), entityType));

            Assert.IsType<GuidValueGenerator>(selector.Select(entityType.FindProperty("Guid"), entityType));
            Assert.IsType<GuidValueGenerator>(selector.Select(entityType.FindProperty("NullableGuid"), entityType));

            Assert.IsType<TemporaryBinaryValueGenerator>(selector.Select(entityType.FindProperty("Binary"), entityType));
        }

        [Fact]
        public void Throws_for_unsupported_combinations()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(AnEntity));

            var contextServices = TestHelpers.Instance.CreateContextServices(model);

            var selector = contextServices.GetRequiredService<ValueGeneratorSelector>();

            Assert.Equal(
                CoreStrings.NoValueGenerator("Random", "AnEntity", typeof(Random).Name),
                Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("Random"), entityType)).Message);
        }

        private static IMutableModel BuildModel(bool generateValues = true)
        {
            var builder = TestHelpers.Instance.CreateConventionBuilder();
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
            public Guid? NullableGuid { get; set; }
            public byte[] Binary { get; set; }
            public float Float { get; set; }
            public float? NullableFloat { get; set; }
            public double Double { get; set; }
            public double? NullableDouble { get; set; }
            public decimal Decimal { get; set; }
            public decimal? NullableDecimal { get; set; }
            public DateTime DateTime { get; set; }
            public DateTime? NullableDateTime { get; set; }
            public DateTimeOffset DateTimeOffset { get; set; }
            public DateTimeOffset? NullableDateTimeOffset { get; set; }
            public Random Random { get; set; }
        }
    }
}
