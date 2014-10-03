// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerMetadataExtensionsTest
    {
        [Fact]
        public void Can_set_sequence_generation_on_property_with_basic_builder()
        {
            var builder = new BasicModelBuilder();

            builder.Entity<AnEntity>()
                .Property(e => e.Id)
                .GenerateValuesUsingSequence();

            var property = GetProperty(builder.Model, "Id");
            ValidateSequence(property);
            Assert.Equal(ValueGeneration.OnAdd, property.ValueGeneration);
        }

        [Fact]
        public void Can_set_sequence_generation_on_non_key_property_with_basic_builder()
        {
            var builder = new BasicModelBuilder();

            builder.Entity<AnEntity>()
                .Property(e => e.NotAnId)
                .GenerateValuesUsingSequence();

            var property = GetProperty(builder.Model, "NotAnId");
            ValidateSequence(property);
            Assert.Equal(ValueGeneration.OnAdd, property.ValueGeneration);
        }

        [Fact]
        public void Can_set_sequence_generation_on_property_with_convention_builder()
        {
            var builder = new ModelBuilder();

            builder.Entity<AnEntity>()
                .Property(e => e.Id)
                .GenerateValuesUsingSequence();

            var property = GetProperty(builder.Model, "Id");
            ValidateSequence(property);
            Assert.Equal(ValueGeneration.OnAdd, property.ValueGeneration);
        }

        [Fact]
        public void Can_set_sequence_generation_on_non_key_property_with_convention_builder()
        {
            var builder = new ModelBuilder();

            builder.Entity<AnEntity>()
                .Property(e => e.NotAnId)
                .GenerateValuesUsingSequence();

            var property = GetProperty(builder.Model, "NotAnId");
            ValidateSequence(property);
            Assert.Equal(ValueGeneration.OnAdd, property.ValueGeneration);
        }

        [Fact]
        public void Can_set_sequence_generation_on_entity_type_with_basic_builder()
        {
            var builder = new BasicModelBuilder();

            builder.Entity<AnEntity>()
                .GenerateValuesUsingSequence();

            ValidateSequence(GetEntityType(builder.Model));
        }

        [Fact]
        public void Can_set_sequence_generation_on_entity_type_with_convention_builder()
        {
            var builder = new ModelBuilder();

            builder.Entity<AnEntity>()
                .GenerateValuesUsingSequence();

            ValidateSequence(GetEntityType(builder.Model));
        }

        [Fact]
        public void Can_set_sequence_generation_on_entity_type_with_non_generic_basic_builder()
        {
            var builder = new BasicModelBuilder();

            builder.Entity(typeof(AnEntity))
                .GenerateValuesUsingSequence();

            ValidateSequence(GetEntityType(builder.Model));
        }

        [Fact]
        public void Can_set_sequence_generation_on_entity_type_with_non_generic_convention_builder()
        {
            var builder = new ModelBuilder();

            builder.Entity(typeof(AnEntity))
                .GenerateValuesUsingSequence();

            ValidateSequence(GetEntityType(builder.Model));
        }

        [Fact]
        public void Can_set_sequence_generation_on_model_with_basic_builder()
        {
            var builder = new BasicModelBuilder();
            builder.Entity<AnEntity>();

            builder.GenerateValuesUsingSequence();

            ValidateSequence(builder.Model);
        }

        [Fact]
        public void Can_set_sequence_generation_on_model_with_convention_builder()
        {
            var builder = new ModelBuilder();
            builder.Entity<AnEntity>();

            builder.GenerateValuesUsingSequence();

            ValidateSequence(builder.Model);
        }

        [Fact]
        public void Can_set_sequence_generation_with_options_on_property_with_basic_builder()
        {
            var builder = new BasicModelBuilder();

            builder.Entity<AnEntity>()
                .Property(e => e.Id)
                .GenerateValuesUsingSequence("SlimShady", 8);

            var property = GetProperty(builder.Model, "Id");
            ValidateSequenceWithOptions(property);
            Assert.Equal(ValueGeneration.OnAdd, property.ValueGeneration);
        }

        [Fact]
        public void Can_set_sequence_generation_with_options_on_non_key_property_with_basic_builder()
        {
            var builder = new BasicModelBuilder();

            builder.Entity<AnEntity>()
                .Property(e => e.NotAnId)
                .GenerateValuesUsingSequence("SlimShady", 8);

            var property = GetProperty(builder.Model, "NotAnId");
            ValidateSequenceWithOptions(property);
            Assert.Equal(ValueGeneration.OnAdd, property.ValueGeneration);
        }

        [Fact]
        public void Can_set_sequence_generation_with_options_on_property_with_convention_builder()
        {
            var builder = new ModelBuilder();

            builder.Entity<AnEntity>()
                .Property(e => e.Id)
                .GenerateValuesUsingSequence("SlimShady", 8);

            var property = GetProperty(builder.Model, "Id");
            ValidateSequenceWithOptions(property);
            Assert.Equal(ValueGeneration.OnAdd, property.ValueGeneration);
        }

        [Fact]
        public void Can_set_sequence_generation_with_options_on_non_key_property_with_convention_builder()
        {
            var builder = new ModelBuilder();

            builder.Entity<AnEntity>()
                .Property(e => e.NotAnId)
                .GenerateValuesUsingSequence("SlimShady", 8);

            var property = GetProperty(builder.Model, "NotAnId");
            ValidateSequenceWithOptions(property);
            Assert.Equal(ValueGeneration.OnAdd, property.ValueGeneration);
        }

        [Fact]
        public void Can_set_sequence_generation_with_options_on_entity_type_with_basic_builder()
        {
            var builder = new BasicModelBuilder();

            builder.Entity<AnEntity>()
                .GenerateValuesUsingSequence("SlimShady", 8);

            ValidateSequenceWithOptions(GetEntityType(builder.Model));
        }

        [Fact]
        public void Can_set_sequence_generation_with_options_on_entity_type_with_convention_builder()
        {
            var builder = new ModelBuilder();

            builder.Entity<AnEntity>()
                .GenerateValuesUsingSequence("SlimShady", 8);

            ValidateSequenceWithOptions(GetEntityType(builder.Model));
        }

        [Fact]
        public void Can_set_sequence_generation_with_options_on_entity_type_with_non_generic_basic_builder()
        {
            var builder = new BasicModelBuilder();

            builder.Entity(typeof(AnEntity))
                .GenerateValuesUsingSequence("SlimShady", 8);

            ValidateSequenceWithOptions(GetEntityType(builder.Model));
        }

        [Fact]
        public void Can_set_sequence_generation_with_options_on_entity_type_with_non_generic_convention_builder()
        {
            var builder = new ModelBuilder();

            builder.Entity(typeof(AnEntity))
                .GenerateValuesUsingSequence("SlimShady", 8);

            ValidateSequenceWithOptions(GetEntityType(builder.Model));
        }

        [Fact]
        public void Can_set_sequence_generation_with_options_on_model_with_basic_builder()
        {
            var builder = new BasicModelBuilder();
            builder.Entity<AnEntity>();

            builder.GenerateValuesUsingSequence("SlimShady", 8);

            ValidateSequenceWithOptions(builder.Model);
        }

        [Fact]
        public void Can_set_sequence_generation_with_options_on_model_with_convention_builder()
        {
            var builder = new ModelBuilder();
            builder.Entity<AnEntity>();

            builder.GenerateValuesUsingSequence("SlimShady", 8);

            ValidateSequenceWithOptions(builder.Model);
        }

        [Fact]
        public void Can_set_identity_generation_on_property_with_basic_builder()
        {
            var builder = new BasicModelBuilder();

            builder.Entity<AnEntity>()
                .Property(e => e.Id)
                .GenerateValuesUsingIdentity();

            var property = GetProperty(builder.Model, "Id");
            ValidateIdentity(property);
            Assert.Equal(ValueGeneration.OnAdd, property.ValueGeneration);
        }

        [Fact]
        public void Can_set_identity_generation_on_non_key_property_with_basic_builder()
        {
            var builder = new BasicModelBuilder();

            builder.Entity<AnEntity>()
                .Property(e => e.NotAnId)
                .GenerateValuesUsingIdentity();

            var property = GetProperty(builder.Model, "NotAnId");
            ValidateIdentity(property);
            Assert.Equal(ValueGeneration.OnAdd, property.ValueGeneration);
        }

        [Fact]
        public void Can_set_identity_generation_on_property_with_convention_builder()
        {
            var builder = new ModelBuilder();

            builder.Entity<AnEntity>()
                .Property(e => e.Id)
                .GenerateValuesUsingIdentity();

            var property = GetProperty(builder.Model, "Id");
            ValidateIdentity(property);
            Assert.Equal(ValueGeneration.OnAdd, property.ValueGeneration);
        }

        [Fact]
        public void Can_set_identity_generation_on_non_key_property_with_convention_builder()
        {
            var builder = new ModelBuilder();

            builder.Entity<AnEntity>()
                .Property(e => e.NotAnId)
                .GenerateValuesUsingIdentity();

            var property = GetProperty(builder.Model, "NotAnId");
            ValidateIdentity(property);
            Assert.Equal(ValueGeneration.OnAdd, property.ValueGeneration);
        }

        [Fact]
        public void Can_set_identity_generation_on_entity_type_with_basic_builder()
        {
            var builder = new BasicModelBuilder();

            builder.Entity<AnEntity>()
                .GenerateValuesUsingIdentity();

            ValidateIdentity(GetEntityType(builder.Model));
        }

        [Fact]
        public void Can_set_identity_generation_on_entity_type_with_convention_builder()
        {
            var builder = new ModelBuilder();

            builder.Entity<AnEntity>()
                .GenerateValuesUsingIdentity();

            ValidateIdentity(GetEntityType(builder.Model));
        }

        [Fact]
        public void Can_set_identity_generation_on_entity_type_with_non_generic_basic_builder()
        {
            var builder = new BasicModelBuilder();

            builder.Entity(typeof(AnEntity))
                .GenerateValuesUsingIdentity();

            ValidateIdentity(GetEntityType(builder.Model));
        }

        [Fact]
        public void Can_set_identity_generation_on_entity_type_with_non_generic_convention_builder()
        {
            var builder = new ModelBuilder();

            builder.Entity(typeof(AnEntity))
                .GenerateValuesUsingIdentity();

            ValidateIdentity(GetEntityType(builder.Model));
        }

        [Fact]
        public void Can_set_identity_generation_on_model_with_basic_builder()
        {
            var builder = new BasicModelBuilder();
            builder.Entity<AnEntity>();

            builder.GenerateValuesUsingIdentity();

            ValidateIdentity(builder.Model);
        }

        [Fact]
        public void Can_set_identity_generation_on_model_with_convention_builder()
        {
            var builder = new ModelBuilder();
            builder.Entity<AnEntity>();

            builder.GenerateValuesUsingIdentity();

            ValidateIdentity(builder.Model);
        }

        [Fact]
        public void Checks_valid_block_size()
        {
            var builder = new ModelBuilder();

            Assert.StartsWith(
                Strings.SequenceBadBlockSize,
                Assert.Throws<ArgumentOutOfRangeException>(
                    () => builder.Entity<AnEntity>()
                        .Property(e => e.Id)
                        .GenerateValuesUsingSequence("Shady", 0)).Message);

            Assert.StartsWith(
                Strings.SequenceBadBlockSize,
                Assert.Throws<ArgumentOutOfRangeException>(
                    () => builder.Entity<AnEntity>()
                        .GenerateValuesUsingSequence("Shady", 0)).Message);

            Assert.StartsWith(
                Strings.SequenceBadBlockSize,
                Assert.Throws<ArgumentOutOfRangeException>(
                    () => builder.Entity(typeof(AnEntity))
                        .GenerateValuesUsingSequence("Shady", 0)).Message);

            Assert.StartsWith(
                Strings.SequenceBadBlockSize,
                Assert.Throws<ArgumentOutOfRangeException>(
                    () => builder.GenerateValuesUsingSequence("Shady", 0)).Message);
        }

        [Fact]
        public void Checks_valid_property_type_for_sequence()
        {
            var builder = new ModelBuilder();
            
            builder.Entity<AnEntity>()
                .Property(e => e.Long)
                .GenerateValuesUsingSequence();

            builder.Entity<AnEntity>()
                .Property(e => e.Short)
                .GenerateValuesUsingSequence();

            builder.Entity<AnEntity>()
                .Property(e => e.Byte)
                .GenerateValuesUsingSequence();

            Assert.StartsWith(
                Strings.FormatSequenceBadType("String", typeof(AnEntity).FullName, typeof(string).Name),
                Assert.Throws<ArgumentException>(
                    () => builder.Entity<AnEntity>()
                        .Property(e => e.String)
                        .GenerateValuesUsingSequence()).Message);

            builder.Entity<AnEntity>()
                .Property(e => e.Long)
                .GenerateValuesUsingSequence("Shady", 8);

            builder.Entity<AnEntity>()
                .Property(e => e.Short)
                .GenerateValuesUsingSequence("Shady", 8);

            builder.Entity<AnEntity>()
                .Property(e => e.Byte)
                .GenerateValuesUsingSequence("Shady", 8);

            Assert.StartsWith(
                Strings.FormatSequenceBadType("String", typeof(AnEntity).FullName, typeof(string).Name),
                Assert.Throws<ArgumentException>(
                    () => builder.Entity<AnEntity>()
                        .Property(e => e.String)
                        .GenerateValuesUsingSequence("Shady", 8)).Message);
        }

        [Fact]
        public void Checks_valid_property_type_for_identity()
        {
            var builder = new ModelBuilder();

            builder.Entity<AnEntity>()
                .Property(e => e.Long)
                .GenerateValuesUsingIdentity();

            builder.Entity<AnEntity>()
                .Property(e => e.Short)
                .GenerateValuesUsingIdentity();

            Assert.StartsWith(
                Strings.FormatIdentityBadType("Byte", typeof(AnEntity).FullName, typeof(byte).Name),
                Assert.Throws<ArgumentException>(
                    () => builder.Entity<AnEntity>()
                        .Property(e => e.Byte)
                        .GenerateValuesUsingIdentity()).Message);

            Assert.StartsWith(
                Strings.FormatIdentityBadType("String", typeof(AnEntity).FullName, typeof(string).Name),
                Assert.Throws<ArgumentException>(
                    () => builder.Entity<AnEntity>()
                        .Property(e => e.String)
                        .GenerateValuesUsingIdentity()).Message);
        }

        private static void ValidateSequence(MetadataBase property)
        {
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.Annotations.Sequence, property[Entity.Metadata.SqlServerMetadataExtensions.Annotations.ValueGeneration]);
            Assert.Null(property[Entity.Metadata.SqlServerMetadataExtensions.Annotations.SequenceBlockSize]);
            Assert.Null(property[Entity.Metadata.SqlServerMetadataExtensions.Annotations.SequenceName]);
        }

        private static void ValidateSequenceWithOptions(MetadataBase property)
        {
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.Annotations.Sequence, property[Entity.Metadata.SqlServerMetadataExtensions.Annotations.ValueGeneration]);
            Assert.Equal("8", property[Entity.Metadata.SqlServerMetadataExtensions.Annotations.SequenceBlockSize]);
            Assert.Equal("SlimShady", property[Entity.Metadata.SqlServerMetadataExtensions.Annotations.SequenceName]);
        }

        private static void ValidateIdentity(MetadataBase property)
        {
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.Annotations.Identity, property[Entity.Metadata.SqlServerMetadataExtensions.Annotations.ValueGeneration]);
            Assert.Null(property[Entity.Metadata.SqlServerMetadataExtensions.Annotations.SequenceBlockSize]);
            Assert.Null(property[Entity.Metadata.SqlServerMetadataExtensions.Annotations.SequenceName]);
        }

        private static Property GetProperty(Model model, string propertyName)
        {
            return GetEntityType(model).GetProperty(propertyName);
        }

        private static EntityType GetEntityType(Model model)
        {
            return model.GetEntityType(typeof(AnEntity));
        }

        private class AnEntity
        {
            public int Id { get; set; }
            public int NotAnId { get; set; }

            public long Long { get; set; }
            public short Short { get; set; }
            public byte Byte { get; set; }
            public string String { get; set; }
        }
    }
}
