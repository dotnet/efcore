// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerValueGeneratorSelectorTest
    {
        [Fact]
        public void Returns_built_in_generators_for_types_setup_for_value_generation()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(AnEntity));

            var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.IsType<TemporaryIntValueGenerator>(selector.Select(entityType.FindProperty("Id"), entityType));
            Assert.IsType<CustomValueGenerator>(selector.Select(entityType.FindProperty("Custom"), entityType));
            Assert.IsType<TemporaryLongValueGenerator>(selector.Select(entityType.FindProperty("Long"), entityType));
            Assert.IsType<TemporaryShortValueGenerator>(selector.Select(entityType.FindProperty("Short"), entityType));
            Assert.IsType<TemporaryByteValueGenerator>(selector.Select(entityType.FindProperty("Byte"), entityType));
            Assert.IsType<TemporaryIntValueGenerator>(selector.Select(entityType.FindProperty("NullableInt"), entityType));
            Assert.IsType<TemporaryLongValueGenerator>(selector.Select(entityType.FindProperty("NullableLong"), entityType));
            Assert.IsType<TemporaryShortValueGenerator>(selector.Select(entityType.FindProperty("NullableShort"), entityType));
            Assert.IsType<TemporaryByteValueGenerator>(selector.Select(entityType.FindProperty("NullableByte"), entityType));
            Assert.IsType<TemporaryDecimalValueGenerator>(selector.Select(entityType.FindProperty("Decimal"), entityType));
            Assert.IsType<StringValueGenerator>(selector.Select(entityType.FindProperty("String"), entityType));
            Assert.IsType<SequentialGuidValueGenerator>(selector.Select(entityType.FindProperty("Guid"), entityType));
            Assert.IsType<BinaryValueGenerator>(selector.Select(entityType.FindProperty("Binary"), entityType));
            Assert.IsType<TemporaryIntValueGenerator>(selector.Select(entityType.FindProperty("AlwaysIdentity"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<int>>(selector.Select(entityType.FindProperty("AlwaysSequence"), entityType));
        }

        [Fact]
        public void Returns_temp_guid_generator_when_default_sql_set()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(AnEntity));

            entityType.FindProperty("Guid").SqlServer().DefaultValueSql = "newid()";

            var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.IsType<TemporaryGuidValueGenerator>(selector.Select(entityType.FindProperty("Guid"), entityType));
        }

        [Fact]
        public void Returns_temp_string_generator_when_default_sql_set()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(AnEntity));

            entityType.FindProperty("String").SqlServer().DefaultValueSql = "Foo";

            var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            var generator = selector.Select(entityType.FindProperty("String"), entityType);
            Assert.IsType<StringValueGenerator>(generator);
            Assert.True(generator.GeneratesTemporaryValues);
        }

        [Fact]
        public void Returns_temp_binary_generator_when_default_sql_set()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(AnEntity));

            entityType.FindProperty("Binary").SqlServer().DefaultValueSql = "Foo";

            var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            var generator = selector.Select(entityType.FindProperty("Binary"), entityType);
            Assert.IsType<BinaryValueGenerator>(generator);
            Assert.True(generator.GeneratesTemporaryValues);
        }

        [Fact]
        public void Returns_sequence_value_generators_when_configured_for_model()
        {
            var model = BuildModel();
            model.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.SequenceHiLo;
            model.SqlServer().GetOrAddSequence(SqlServerModelAnnotations.DefaultHiLoSequenceName);
            var entityType = model.FindEntityType(typeof(AnEntity));

            var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.IsType<SqlServerSequenceHiLoValueGenerator<int>>(selector.Select(entityType.FindProperty("Id"), entityType));
            Assert.IsType<CustomValueGenerator>(selector.Select(entityType.FindProperty("Custom"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<long>>(selector.Select(entityType.FindProperty("Long"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<short>>(selector.Select(entityType.FindProperty("Short"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<byte>>(selector.Select(entityType.FindProperty("Byte"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<int>>(selector.Select(entityType.FindProperty("NullableInt"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<long>>(selector.Select(entityType.FindProperty("NullableLong"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<short>>(selector.Select(entityType.FindProperty("NullableShort"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<byte>>(selector.Select(entityType.FindProperty("NullableByte"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<decimal>>(selector.Select(entityType.FindProperty("Decimal"), entityType));
            Assert.IsType<StringValueGenerator>(selector.Select(entityType.FindProperty("String"), entityType));
            Assert.IsType<SequentialGuidValueGenerator>(selector.Select(entityType.FindProperty("Guid"), entityType));
            Assert.IsType<BinaryValueGenerator>(selector.Select(entityType.FindProperty("Binary"), entityType));
            Assert.IsType<TemporaryIntValueGenerator>(selector.Select(entityType.FindProperty("AlwaysIdentity"), entityType));
            Assert.IsType<SqlServerSequenceHiLoValueGenerator<int>>(selector.Select(entityType.FindProperty("AlwaysSequence"), entityType));
        }

        [Fact]
        public void Throws_for_unsupported_combinations()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(AnEntity));

            var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.Equal(
                CoreStrings.NoValueGenerator("Random", "AnEntity", typeof(Random).Name),
                Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("Random"), entityType)).Message);
        }

        [Fact]
        public void Returns_generator_configured_on_model_when_property_is_Identity()
        {
            var model = SqlServerTestHelpers.Instance.BuildModelFor<AnEntity>();
            model.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.SequenceHiLo;
            model.SqlServer().GetOrAddSequence(SqlServerModelAnnotations.DefaultHiLoSequenceName);
            var entityType = model.FindEntityType(typeof(AnEntity));

            var selector = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.IsType<SqlServerSequenceHiLoValueGenerator<int>>(selector.Select(entityType.FindProperty("Id"), entityType));
        }

        private static IMutableModel BuildModel(bool generateValues = true)
        {
            var builder = SqlServerTestHelpers.Instance.CreateConventionBuilder();

            builder.Ignore<Random>();
            builder.Entity<AnEntity>().Property(e => e.Custom).HasValueGenerator<CustomValueGenerator>();

            var model = builder.Model;
            model.SqlServer().GetOrAddSequence(SqlServerModelAnnotations.DefaultHiLoSequenceName);

            var entityType = model.FindEntityType(typeof(AnEntity));
            entityType.AddProperty("Random", typeof(Random));

            foreach (var property in entityType.GetProperties())
            {
                property.ValueGenerated = generateValues ? ValueGenerated.OnAdd : ValueGenerated.Never;
            }

            entityType.FindProperty("AlwaysIdentity").ValueGenerated = ValueGenerated.OnAdd;
            entityType.FindProperty("AlwaysIdentity").SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.IdentityColumn;

            entityType.FindProperty("AlwaysSequence").ValueGenerated = ValueGenerated.OnAdd;
            entityType.FindProperty("AlwaysSequence").SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.SequenceHiLo;

            return model;
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
            public string String { get; set; }
            public Guid Guid { get; set; }
            public byte[] Binary { get; set; }
            public float Float { get; set; }
            public decimal Decimal { get; set; }
            public int AlwaysIdentity { get; set; }
            public int AlwaysSequence { get; set; }
            public Random Random { get; set; }
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
