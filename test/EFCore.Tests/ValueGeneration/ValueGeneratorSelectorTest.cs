// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    public class ValueGeneratorSelectorTest
    {
        [ConditionalFact]
        public void Returns_built_in_generators_for_types_setup_for_value_generation()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(AnEntity));

            var selector = new ValueGeneratorSelector(
                new ValueGeneratorSelectorDependencies(new ValueGeneratorCache(new ValueGeneratorCacheDependencies())));

            Assert.IsType<CustomValueGenerator>(selector.Select(entityType.FindProperty("Custom"), entityType));

            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("Id"), entityType));
            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("Long"), entityType));
            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("Short"), entityType));
            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("Byte"), entityType));

            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("NullableInt"), entityType));
            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("NullableLong"), entityType));
            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("NullableShort"), entityType));
            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("NullableByte"), entityType));

            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("UInt"), entityType));
            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("ULong"), entityType));
            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("UShort"), entityType));
            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("SByte"), entityType));

            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("NullableUInt"), entityType));
            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("NullableULong"), entityType));
            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("NullableUShort"), entityType));
            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("NullableSByte"), entityType));

            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("Decimal"), entityType));
            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("NullableDecimal"), entityType));

            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("Float"), entityType));
            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("NullableFloat"), entityType));

            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("Double"), entityType));
            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("NullableDouble"), entityType));

            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("DateTime"), entityType));
            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("NullableDateTime"), entityType));

            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("DateTimeOffset"), entityType));
            Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("NullableDateTimeOffset"), entityType));

            Assert.IsType<StringValueGenerator>(selector.Select(entityType.FindProperty("String"), entityType));

            Assert.IsType<GuidValueGenerator>(selector.Select(entityType.FindProperty("Guid"), entityType));
            Assert.IsType<GuidValueGenerator>(selector.Select(entityType.FindProperty("NullableGuid"), entityType));

            Assert.IsType<BinaryValueGenerator>(selector.Select(entityType.FindProperty("Binary"), entityType));
        }

        [ConditionalFact]
        public void Throws_for_unsupported_combinations()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(AnEntity));

            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var selector = contextServices.GetRequiredService<IValueGeneratorSelector>();

            Assert.Equal(
                CoreStrings.NoValueGenerator("Random", "AnEntity", "char"),
                Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("Random"), entityType)).Message);
        }

        private static IModel BuildModel(bool generateValues = true)
        {
            var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            builder.Entity<AnEntity>().Property(e => e.Custom).HasValueGenerator<CustomValueGenerator>();
            var entityType = builder.Model.FindEntityType(typeof(AnEntity));

            foreach (var property in entityType.GetProperties())
            {
                property.ValueGenerated = generateValues ? ValueGenerated.OnAdd : ValueGenerated.Never;
            }

            return builder.FinalizeModel();
        }

        private class AnEntity
        {
            public int Id { get; set; }
            public int Custom { get; set; }
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
            public char Random { get; set; }
        }

        private class CustomValueGenerator : ValueGenerator<int>
        {
            public override int Next(EntityEntry entry)
            {
                throw new NotImplementedException();
            }

            public override bool GeneratesTemporaryValues => false;
        }
    }
}
