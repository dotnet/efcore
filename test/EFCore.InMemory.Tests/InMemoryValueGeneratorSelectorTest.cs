// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Local
namespace Microsoft.EntityFrameworkCore
{
    public class InMemoryValueGeneratorSelectorTest
    {
        [ConditionalFact]
        public void Returns_built_in_generators_for_types_setup_for_value_generation()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(AnEntity));

            var selector = InMemoryTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.IsType<CustomValueGenerator>(selector.Select(entityType.FindProperty("Custom"), entityType));
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
            Assert.IsType<StringValueGenerator>(selector.Select(entityType.FindProperty("String"), entityType));
            Assert.IsType<GuidValueGenerator>(selector.Select(entityType.FindProperty("Guid"), entityType));
            Assert.IsType<BinaryValueGenerator>(selector.Select(entityType.FindProperty("Binary"), entityType));
        }

        [ConditionalFact]
        public void Can_create_factories_for_all_integer_types()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(AnEntity));

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
        {
            var model = BuildModel();

            var selector = InMemoryTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            return selector.Select(property, property.DeclaringEntityType).Next(null);
        }

        [ConditionalFact]
        public void Throws_for_unsupported_combinations()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(AnEntity));

            var selector = InMemoryTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.Equal(
                CoreStrings.NoValueGenerator("Float", "AnEntity", "float"),
                Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("Float"), entityType)).Message);
        }

        private static IModel BuildModel(bool generateValues = true)
        {
            var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
            builder.Entity<AnEntity>().Property(e => e.Custom).HasValueGenerator<CustomValueGenerator>();
            var model = builder.Model;
            var entityType = model.FindEntityType(typeof(AnEntity));

            foreach (var property in entityType.GetProperties())
            {
                property.ValueGenerated = generateValues ? ValueGenerated.OnAdd : ValueGenerated.Never;
            }

            return model.FinalizeModel();
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
            public byte[] Binary { get; set; }
            public float Float { get; set; }
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
