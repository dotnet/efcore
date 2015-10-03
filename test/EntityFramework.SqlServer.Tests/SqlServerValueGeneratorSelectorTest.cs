// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Tests;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Data.Entity.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerValueGeneratorSelectorTest
    {
        [Fact]
        public void Returns_built_in_generators_for_types_setup_for_value_generation()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(AnEntity));

            var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.IsType<TemporaryNumberValueGenerator<int>>(selector.Select(entityType.GetProperty("Id"), entityType));
            Assert.IsType<TemporaryNumberValueGenerator<long>>(selector.Select(entityType.GetProperty("Long"), entityType));
            Assert.IsType<TemporaryNumberValueGenerator<short>>(selector.Select(entityType.GetProperty("Short"), entityType));
            Assert.IsType<TemporaryNumberValueGenerator<byte>>(selector.Select(entityType.GetProperty("Byte"), entityType));
            Assert.IsType<TemporaryNumberValueGenerator<char>>(selector.Select(entityType.GetProperty("Char"), entityType));
            Assert.IsType<TemporaryNumberValueGenerator<int>>(selector.Select(entityType.GetProperty("NullableInt"), entityType));
            Assert.IsType<TemporaryNumberValueGenerator<long>>(selector.Select(entityType.GetProperty("NullableLong"), entityType));
            Assert.IsType<TemporaryNumberValueGenerator<short>>(selector.Select(entityType.GetProperty("NullableShort"), entityType));
            Assert.IsType<TemporaryNumberValueGenerator<byte>>(selector.Select(entityType.GetProperty("NullableByte"), entityType));
            Assert.IsType<TemporaryNumberValueGenerator<char>>(selector.Select(entityType.GetProperty("NullableChar"), entityType));
            Assert.IsType<TemporaryNumberValueGenerator<uint>>(selector.Select(entityType.GetProperty("UInt"), entityType));
            Assert.IsType<TemporaryNumberValueGenerator<ulong>>(selector.Select(entityType.GetProperty("ULong"), entityType));
            Assert.IsType<TemporaryNumberValueGenerator<ushort>>(selector.Select(entityType.GetProperty("UShort"), entityType));
            Assert.IsType<TemporaryNumberValueGenerator<sbyte>>(selector.Select(entityType.GetProperty("SByte"), entityType));
            Assert.IsType<TemporaryNumberValueGenerator<uint>>(selector.Select(entityType.GetProperty("NullableUInt"), entityType));
            Assert.IsType<TemporaryNumberValueGenerator<ulong>>(selector.Select(entityType.GetProperty("NullableULong"), entityType));
            Assert.IsType<TemporaryNumberValueGenerator<ushort>>(selector.Select(entityType.GetProperty("NullableUShort"), entityType));
            Assert.IsType<TemporaryNumberValueGenerator<sbyte>>(selector.Select(entityType.GetProperty("NullableSByte"), entityType));
            Assert.IsType<TemporaryStringValueGenerator>(selector.Select(entityType.GetProperty("String"), entityType));
            Assert.IsType<TemporaryGuidValueGenerator>(selector.Select(entityType.GetProperty("Guid"), entityType));
            Assert.IsType<TemporaryBinaryValueGenerator>(selector.Select(entityType.GetProperty("Binary"), entityType));
            Assert.IsType<TemporaryNumberValueGenerator<int>>(selector.Select(entityType.GetProperty("AlwaysIdentity"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<int>>(selector.Select(entityType.GetProperty("AlwaysSequence"), entityType));
        }

        [Fact]
        public void Returns_temp_guid_generator_when_default_sql_set()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(AnEntity));

            entityType.GetProperty("Guid").SqlServer().GeneratedValueSql = "newid()";

            var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.IsType<TemporaryGuidValueGenerator>(selector.Select(entityType.GetProperty("Guid"), entityType));
        }

        [Fact]
        public void Returns_sequence_value_generators_when_configured_for_model()
        {
            var model = BuildModel();
            model.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.SequenceHiLo;
            model.SqlServer().GetOrAddSequence(SqlServerAnnotationNames.DefaultHiLoSequenceName);
            var entityType = model.GetEntityType(typeof(AnEntity));

            foreach (var property in entityType.Properties)
            {
                property.ValueGenerated = ValueGenerated.OnAdd;
            }

            var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.IsType<SqlServerSequenceHiLoValueGenerator<int>>(selector.Select(entityType.GetProperty("Id"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<long>>(selector.Select(entityType.GetProperty("Long"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<short>>(selector.Select(entityType.GetProperty("Short"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<byte>>(selector.Select(entityType.GetProperty("Byte"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<char>>(selector.Select(entityType.GetProperty("Char"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<int>>(selector.Select(entityType.GetProperty("NullableInt"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<long>>(selector.Select(entityType.GetProperty("NullableLong"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<short>>(selector.Select(entityType.GetProperty("NullableShort"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<byte>>(selector.Select(entityType.GetProperty("NullableByte"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<char>>(selector.Select(entityType.GetProperty("NullableChar"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<uint>>(selector.Select(entityType.GetProperty("UInt"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<ulong>>(selector.Select(entityType.GetProperty("ULong"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<ushort>>(selector.Select(entityType.GetProperty("UShort"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<sbyte>>(selector.Select(entityType.GetProperty("SByte"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<uint>>(selector.Select(entityType.GetProperty("NullableUInt"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<ulong>>(selector.Select(entityType.GetProperty("NullableULong"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<ushort>>(selector.Select(entityType.GetProperty("NullableUShort"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<sbyte>>(selector.Select(entityType.GetProperty("NullableSByte"), entityType));
            Assert.IsType<TemporaryStringValueGenerator>(selector.Select(entityType.GetProperty("String"), entityType));
            Assert.IsType<SequentialGuidValueGenerator>(selector.Select(entityType.GetProperty("Guid"), entityType));
            Assert.IsType<TemporaryBinaryValueGenerator>(selector.Select(entityType.GetProperty("Binary"), entityType));
            Assert.IsType<TemporaryNumberValueGenerator<int>>(selector.Select(entityType.GetProperty("AlwaysIdentity"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<int>>(selector.Select(entityType.GetProperty("AlwaysSequence"), entityType));
        }

        [Fact]
        public void Throws_for_unsupported_combinations()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(AnEntity));

            var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.Equal(
                CoreStrings.NoValueGenerator("Random", "AnEntity", typeof(Random).Name),
                Assert.Throws<NotSupportedException>(() => selector.Select(entityType.GetProperty("Random"), entityType)).Message);
        }

        [Fact]
        public void Returns_generator_configured_on_model_when_property_is_Identity()
        {
            var model = SqlServerTestHelpers.Instance.BuildModelFor<AnEntity>();
            model.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.SequenceHiLo;
            model.SqlServer().GetOrAddSequence(SqlServerAnnotationNames.DefaultHiLoSequenceName);
            var entityType = model.GetEntityType(typeof(AnEntity));

            var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.IsType<SqlServerSequenceHiLoValueGenerator<int>>(selector.Select(entityType.GetProperty("Id"), entityType));
        }

        private static Model BuildModel(bool generateValues = true)
        {
            var builder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
            builder.Ignore<Random>();
            builder.Entity<AnEntity>();
            var model = builder.Model;
            model.SqlServer().GetOrAddSequence(SqlServerAnnotationNames.DefaultHiLoSequenceName);
            var entityType = model.GetEntityType(typeof(AnEntity));
            var property1 = entityType.AddProperty("Random", typeof(Random));
            property1.IsShadowProperty = false;

            foreach (var property in entityType.Properties)
            {
                property.RequiresValueGenerator = generateValues;
            }

            entityType.GetProperty("AlwaysIdentity").ValueGenerated = ValueGenerated.OnAdd;
            entityType.GetProperty("AlwaysIdentity").SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.IdentityColumn;

            entityType.GetProperty("AlwaysSequence").ValueGenerated = ValueGenerated.OnAdd;
            entityType.GetProperty("AlwaysSequence").SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.SequenceHiLo;

            return model;
        }

        private class AnEntity
        {
            public int Id { get; set; }
            public long Long { get; set; }
            public short Short { get; set; }
            public byte Byte { get; set; }
            public char Char { get; set; }
            public int? NullableInt { get; set; }
            public long? NullableLong { get; set; }
            public short? NullableShort { get; set; }
            public byte? NullableByte { get; set; }
            public char? NullableChar { get; set; }
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
            public int AlwaysIdentity { get; set; }
            public int AlwaysSequence { get; set; }
            public Random Random { get; set; }
        }
    }
}
