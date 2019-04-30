// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class SqlServerMetadataExtensionsTest
    {
        [Fact]
        public void Can_get_and_set_column_name()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Equal("Name", property.GetColumnName());

            property.SetColumnName("Eman");

            Assert.Equal("Name", property.Name);
            Assert.Equal("Eman", property.GetColumnName());

            property.SetColumnName("MyNameIs");

            Assert.Equal("Name", property.Name);
            Assert.Equal("MyNameIs", property.GetColumnName());

            property.SetColumnName(null);

            Assert.Equal("Name", property.Name);
            Assert.Equal("Name", property.GetColumnName());
        }


        [Fact]
        public void Can_get_and_set_column_key_name()
        {
            var modelBuilder = GetModelBuilder();

            var key = modelBuilder
                .Entity<Customer>()
                .HasKey(e => e.Id)
                .Metadata;

            Assert.Equal("PK_Customer", key.GetName());

            key.SetName("PrimaryKey");

            Assert.Equal("PrimaryKey", key.GetName());

            key.SetName("PrimarySchool");

            Assert.Equal("PrimarySchool", key.GetName());

            key.SetName(null);

            Assert.Equal("PK_Customer", key.GetName());
        }

        [Fact]
        public void Can_get_and_set_index_clustering()
        {
            var modelBuilder = GetModelBuilder();

            var index = modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Id)
                .Metadata;

            Assert.Null(index.GetSqlServerIsClustered());

            index.SetSqlServerIsClustered(true);

            Assert.True(index.GetSqlServerIsClustered().Value);

            index.SetSqlServerIsClustered(null);

            Assert.Null(index.GetSqlServerIsClustered());
        }

        [Fact]
        public void Can_get_and_set_key_clustering()
        {
            var modelBuilder = GetModelBuilder();

            var key = modelBuilder
                .Entity<Customer>()
                .HasKey(e => e.Id)
                .Metadata;

            Assert.Null(key.GetSqlServerIsClustered());

            key.SetSqlServerIsClustered(true);

            Assert.True(key.GetSqlServerIsClustered().Value);

            key.SetSqlServerIsClustered(null);

            Assert.Null(key.GetSqlServerIsClustered());
        }

        [Fact]
        public void Can_get_and_set_sequence()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            Assert.Null(model.FindSequence("Foo"));
            Assert.Null(model.FindSequence("Foo"));
            Assert.Null(((IModel)model).FindSequence("Foo"));

            var sequence = model.AddSequence("Foo");

            Assert.Equal("Foo", model.FindSequence("Foo").Name);
            Assert.Equal("Foo", ((IModel)model).FindSequence("Foo").Name);
            Assert.Equal("Foo", model.FindSequence("Foo").Name);
            Assert.Equal("Foo", ((IModel)model).FindSequence("Foo").Name);

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(1, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);

            Assert.NotNull(model.FindSequence("Foo"));

            var sequence2 = model.FindSequence("Foo");

            sequence.StartValue = 1729;
            sequence.IncrementBy = 11;
            sequence.MinValue = 2001;
            sequence.MaxValue = 2010;
            sequence.ClrType = typeof(int);

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(2001, sequence.MinValue);
            Assert.Equal(2010, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.ClrType);

            Assert.Equal(sequence2.Name, sequence.Name);
            Assert.Equal(sequence2.Schema, sequence.Schema);
            Assert.Equal(sequence2.IncrementBy, sequence.IncrementBy);
            Assert.Equal(sequence2.StartValue, sequence.StartValue);
            Assert.Equal(sequence2.MinValue, sequence.MinValue);
            Assert.Equal(sequence2.MaxValue, sequence.MaxValue);
            Assert.Same(sequence2.ClrType, sequence.ClrType);
        }

        [Fact]
        public void Can_get_and_set_sequence_with_schema_name()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            Assert.Null(model.FindSequence("Foo", "Smoo"));
            Assert.Null(model.FindSequence("Foo", "Smoo"));
            Assert.Null(((IModel)model).FindSequence("Foo", "Smoo"));

            var sequence = model.AddSequence("Foo", "Smoo");

            Assert.Equal("Foo", model.FindSequence("Foo", "Smoo").Name);
            Assert.Equal("Foo", ((IModel)model).FindSequence("Foo", "Smoo").Name);
            Assert.Equal("Foo", model.FindSequence("Foo", "Smoo").Name);
            Assert.Equal("Foo", ((IModel)model).FindSequence("Foo", "Smoo").Name);

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
            Assert.Equal(1, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);

            Assert.NotNull(model.FindSequence("Foo", "Smoo"));

            var sequence2 = model.FindSequence("Foo", "Smoo");

            sequence.StartValue = 1729;
            sequence.IncrementBy = 11;
            sequence.MinValue = 2001;
            sequence.MaxValue = 2010;
            sequence.ClrType = typeof(int);

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(2001, sequence.MinValue);
            Assert.Equal(2010, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.ClrType);

            Assert.Equal(sequence2.Name, sequence.Name);
            Assert.Equal(sequence2.Schema, sequence.Schema);
            Assert.Equal(sequence2.IncrementBy, sequence.IncrementBy);
            Assert.Equal(sequence2.StartValue, sequence.StartValue);
            Assert.Equal(sequence2.MinValue, sequence.MinValue);
            Assert.Equal(sequence2.MaxValue, sequence.MaxValue);
            Assert.Same(sequence2.ClrType, sequence.ClrType);
        }

        [Fact]
        public void Can_get_multiple_sequences()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            model.AddSequence("Fibonacci");
            model.AddSequence("Golomb");

            var sequences = model.GetSequences();

            Assert.Equal(2, sequences.Count);
            Assert.Contains(sequences, s => s.Name == "Fibonacci");
            Assert.Contains(sequences, s => s.Name == "Golomb");
        }

        [Fact]
        public void Can_get_multiple_sequences_when_overridden()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            model.AddSequence("Fibonacci").StartValue = 1;
            model.FindSequence("Fibonacci").StartValue = 3;
            model.AddSequence("Golomb");

            var sequences = model.GetSequences();

            Assert.Equal(2, sequences.Count);
            Assert.Contains(sequences, s => s.Name == "Golomb");

            var sequence = sequences.FirstOrDefault(s => s.Name == "Fibonacci");
            Assert.NotNull(sequence);
            Assert.Equal(3, sequence.StartValue);
        }

        [Fact]
        public void Can_get_and_set_value_generation_on_model()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, model.GetSqlServerValueGenerationStrategy());

            model.SetSqlServerValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, model.GetSqlServerValueGenerationStrategy());

            model.SetSqlServerValueGenerationStrategy(null);

            Assert.Null(model.GetSqlServerValueGenerationStrategy());
        }

        [Fact]
        public void Can_get_and_set_default_sequence_name_on_model()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            model.SetSqlServerHiLoSequenceName("Tasty.Snook");

            Assert.Equal("Tasty.Snook", model.GetSqlServerHiLoSequenceName());

            model.SetSqlServerHiLoSequenceName(null);

            Assert.Equal(SqlServerModelExtensions.DefaultHiLoSequenceName, model.GetSqlServerHiLoSequenceName());
        }

        [Fact]
        public void Can_get_and_set_default_sequence_schema_on_model()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            Assert.Null(model.GetSqlServerHiLoSequenceSchema());

            model.SetSqlServerHiLoSequenceSchema("Tasty.Snook");

            Assert.Equal("Tasty.Snook", model.GetSqlServerHiLoSequenceSchema());

            model.SetSqlServerHiLoSequenceSchema(null);

            Assert.Null(model.GetSqlServerHiLoSequenceSchema());
        }

        [Fact]
        public void Can_get_and_set_value_generation_on_property()
        {
            var modelBuilder = GetModelBuilder();
            modelBuilder.Model.SetSqlServerValueGenerationStrategy(null);

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            Assert.Null(property.GetSqlServerValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);

            property.SetSqlServerValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, property.GetSqlServerValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);

            property.SetSqlServerValueGenerationStrategy(null);

            Assert.Null(property.GetSqlServerValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [Fact]
        public void Can_get_and_set_value_generation_on_nullable_property()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.NullableInt)
                .Metadata;

            Assert.Null(property.GetSqlServerValueGenerationStrategy());

            property.SetSqlServerValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, property.GetSqlServerValueGenerationStrategy());

            property.SetSqlServerValueGenerationStrategy(null);

            Assert.Null(property.GetSqlServerValueGenerationStrategy());
        }

        [Fact]
        public void Throws_setting_sequence_generation_for_invalid_type()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Equal(
                SqlServerStrings.SequenceBadType("Name", nameof(Customer), "string"),
                Assert.Throws<ArgumentException>(
                    () => property.SetSqlServerValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo)).Message);
        }

        [Fact]
        public void Throws_setting_identity_generation_for_invalid_type()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Equal(
                SqlServerStrings.IdentityBadType("Name", nameof(Customer), "string"),
                Assert.Throws<ArgumentException>(
                    () => property.SetSqlServerValueGenerationStrategy(SqlServerValueGenerationStrategy.IdentityColumn)).Message);
        }

        [Fact]
        public void Can_get_and_set_sequence_name_on_property()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            Assert.Null(property.GetSqlServerHiLoSequenceName());
            Assert.Null(((IProperty)property).GetSqlServerHiLoSequenceName());

            property.SetSqlServerHiLoSequenceName("Snook");

            Assert.Equal("Snook", property.GetSqlServerHiLoSequenceName());

            property.SetSqlServerHiLoSequenceName(null);

            Assert.Null(property.GetSqlServerHiLoSequenceName());
        }

        [Fact]
        public void Can_get_and_set_sequence_schema_on_property()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            Assert.Null(property.GetSqlServerHiLoSequenceSchema());

            property.SetSqlServerHiLoSequenceSchema("Tasty");

            Assert.Equal("Tasty", property.GetSqlServerHiLoSequenceSchema());

            property.SetSqlServerHiLoSequenceSchema(null);

            Assert.Null(property.GetSqlServerHiLoSequenceSchema());
        }

        [Fact]
        public void TryGetSequence_returns_null_if_property_is_not_configured_for_sequence_value_generation()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            modelBuilder.Model.AddSequence("DaneelOlivaw");

            Assert.Null(property.FindSqlServerHiLoSequence());

            property.SetSqlServerHiLoSequenceName("DaneelOlivaw");

            Assert.Null(property.FindSqlServerHiLoSequence());

            modelBuilder.Model.SetSqlServerValueGenerationStrategy(SqlServerValueGenerationStrategy.IdentityColumn);

            Assert.Null(property.FindSqlServerHiLoSequence());

            modelBuilder.Model.SetSqlServerValueGenerationStrategy(null);
            property.SetSqlServerValueGenerationStrategy(SqlServerValueGenerationStrategy.IdentityColumn);

            Assert.Null(property.FindSqlServerHiLoSequence());
        }

        [Fact]
        public void TryGetSequence_returns_sequence_property_is_marked_for_sequence_generation()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.AddSequence("DaneelOlivaw");
            property.SetSqlServerHiLoSequenceName("DaneelOlivaw");
            property.SetSqlServerValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);

            Assert.Equal("DaneelOlivaw", property.FindSqlServerHiLoSequence().Name);
        }

        [Fact]
        public void TryGetSequence_returns_sequence_property_is_marked_for_default_generation_and_model_is_marked_for_sequence_generation()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.AddSequence("DaneelOlivaw");
            modelBuilder.Model.SetSqlServerValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);
            property.SetSqlServerHiLoSequenceName("DaneelOlivaw");

            Assert.Equal("DaneelOlivaw", property.FindSqlServerHiLoSequence().Name);
        }

        [Fact]
        public void TryGetSequence_returns_sequence_property_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.AddSequence("DaneelOlivaw");
            modelBuilder.Model.SetSqlServerHiLoSequenceName("DaneelOlivaw");
            property.SetSqlServerValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);

            Assert.Equal("DaneelOlivaw", property.FindSqlServerHiLoSequence().Name);
        }

        [Fact]
        public void
            TryGetSequence_returns_sequence_property_is_marked_for_default_generation_and_model_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.AddSequence("DaneelOlivaw");
            modelBuilder.Model.SetSqlServerValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);
            modelBuilder.Model.SetSqlServerHiLoSequenceName("DaneelOlivaw");

            Assert.Equal("DaneelOlivaw", property.FindSqlServerHiLoSequence().Name);
        }

        [Fact]
        public void TryGetSequence_with_schema_returns_sequence_property_is_marked_for_sequence_generation()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.AddSequence("DaneelOlivaw", "R");
            property.SetSqlServerHiLoSequenceName("DaneelOlivaw");
            property.SetSqlServerHiLoSequenceSchema("R");
            property.SetSqlServerValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);

            Assert.Equal("DaneelOlivaw", property.FindSqlServerHiLoSequence().Name);
            Assert.Equal("R", property.FindSqlServerHiLoSequence().Schema);
        }

        [Fact]
        public void TryGetSequence_with_schema_returns_sequence_model_is_marked_for_sequence_generation()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.AddSequence("DaneelOlivaw", "R");
            modelBuilder.Model.SetSqlServerValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);
            property.SetSqlServerHiLoSequenceName("DaneelOlivaw");
            property.SetSqlServerHiLoSequenceSchema("R");

            Assert.Equal("DaneelOlivaw", property.FindSqlServerHiLoSequence().Name);
            Assert.Equal("R", property.FindSqlServerHiLoSequence().Schema);
        }

        [Fact]
        public void TryGetSequence_with_schema_returns_sequence_property_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.AddSequence("DaneelOlivaw", "R");
            modelBuilder.Model.SetSqlServerHiLoSequenceName("DaneelOlivaw");
            modelBuilder.Model.SetSqlServerHiLoSequenceSchema("R");
            property.SetSqlServerValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);

            Assert.Equal("DaneelOlivaw", property.FindSqlServerHiLoSequence().Name);
            Assert.Equal("R", property.FindSqlServerHiLoSequence().Schema);
        }

        [Fact]
        public void TryGetSequence_with_schema_returns_sequence_model_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.AddSequence("DaneelOlivaw", "R");
            modelBuilder.Model.SetSqlServerValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);
            modelBuilder.Model.SetSqlServerHiLoSequenceName("DaneelOlivaw");
            modelBuilder.Model.SetSqlServerHiLoSequenceSchema("R");

            Assert.Equal("DaneelOlivaw", property.FindSqlServerHiLoSequence().Name);
            Assert.Equal("R", property.FindSqlServerHiLoSequence().Schema);
        }

        private static ModelBuilder GetModelBuilder() => SqlServerTestHelpers.Instance.CreateConventionBuilder();

        // ReSharper disable once ClassNeverInstantiated.Local
        private class Customer
        {
            public int Id { get; set; }
            public int? NullableInt { get; set; }
            public string Name { get; set; }
            public byte Byte { get; set; }
            public byte? NullableByte { get; set; }
            public byte[] ByteArray { get; set; }
        }

        private class Order
        {
            public int OrderId { get; set; }
            public int CustomerId { get; set; }
        }
    }
}
