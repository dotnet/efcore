// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerDatabaseBuilderTest
    {
        [Fact]
        public void Build_creates_sequence_specified_on_property()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id")
                            .GenerateValuesUsingSequence("S0", 3);
                        b.Property<short>("P")
                            .GenerateValuesUsingSequence("S1", 7);
                        b.Key("Id");
                    });

            var databaseBuilder = new SqlServerDatabaseBuilder();
            var database = databaseBuilder.GetDatabase(modelBuilder.Model);

            Assert.Equal(2, database.Sequences.Count);

            var sequence0 = database.Sequences[0];

            Assert.Equal("S0", sequence0.Name);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceDataType, sequence0.DataType);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceStartWith, sequence0.StartWith);
            Assert.Equal(3, sequence0.IncrementBy);

            var sequence1 = database.Sequences[1];

            Assert.Equal("S1", sequence1.Name);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceDataType, sequence1.DataType);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceStartWith, sequence1.StartWith);
            Assert.Equal(7, sequence1.IncrementBy);
        }

        [Fact]
        public void Build_creates_sequence_with_defaults_specified_on_property()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder.Entity("A",
                b =>
                {
                    b.Property<int>("Id")
                        .GenerateValuesUsingSequence();
                    b.Key("Id");
                    b.ToTable("T", "dbo");
                });

            var databaseBuilder = new SqlServerDatabaseBuilder();
            var database = databaseBuilder.GetDatabase(modelBuilder.Model);

            Assert.Equal(1, database.Sequences.Count);

            var sequence = database.Sequences[0];

            Assert.Equal("dbo_T_Sequence", sequence.Name);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceDataType, sequence.DataType);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceStartWith, sequence.StartWith);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceBlockSize, sequence.IncrementBy);
        }

        [Fact]
        public void Sequence_specified_on_property_is_shared_if_matches_previous_definition()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id")
                            .GenerateValuesUsingSequence("MySequence", 7);
                        b.Property<short>("P")
                            .GenerateValuesUsingSequence("MySequence", 7);
                        b.Key("Id");
                    });

            var databaseBuilder = new SqlServerDatabaseBuilder();
            var database = databaseBuilder.GetDatabase(modelBuilder.Model);

            Assert.Equal(1, database.Sequences.Count);

            var sequence = database.Sequences[0];

            Assert.Equal("MySequence", sequence.Name);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceDataType, sequence.DataType);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceStartWith, sequence.StartWith);
            Assert.Equal(7, sequence.IncrementBy);
        }

        [Fact]
        public void Sequence_with_defaults_specified_on_property_is_shared()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder.Entity("A",
                b =>
                {
                    b.Property<int>("Id")
                        .GenerateValuesUsingSequence();
                    b.Property<short>("P")
                        .GenerateValuesUsingSequence();
                    b.Key("Id");
                    b.ToTable("T", "dbo");
                });

            var databaseBuilder = new SqlServerDatabaseBuilder();
            var database = databaseBuilder.GetDatabase(modelBuilder.Model);

            Assert.Equal(1, database.Sequences.Count);

            var sequence = database.Sequences[0];

            Assert.Equal("dbo_T_Sequence", sequence.Name);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceDataType, sequence.DataType);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceStartWith, sequence.StartWith);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceBlockSize, sequence.IncrementBy);
        }

        [Fact]
        public void Redefining_sequence_throws()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder.Entity("A",
                b =>
                {
                    b.Property<int>("Id")
                        .GenerateValuesUsingSequence("MySequence", 7);
                    b.Property<int>("P")
                        .GenerateValuesUsingSequence("MySequence", 13);
                    b.Key("Id");
                });

            var databaseBuilder = new SqlServerDatabaseBuilder();
            
            Assert.Throws<InvalidOperationException>(() => databaseBuilder.GetDatabase(modelBuilder.Model));
        }

        [Fact]
        public void Build_creates_sequence_defined_on_entity()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id")
                            .GenerateValuesOnAdd();
                        b.Property<short>("P")
                            .GenerateValuesOnAdd();
                        b.Key("Id");
                        b.GenerateValuesUsingSequence("MySequence", 7);
                    });

            var databaseBuilder = new SqlServerDatabaseBuilder();
            var database = databaseBuilder.GetDatabase(modelBuilder.Model);

            Assert.Equal(1, database.Sequences.Count);

            var sequence = database.Sequences[0];

            Assert.Equal("MySequence", sequence.Name);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceDataType, sequence.DataType);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceStartWith, sequence.StartWith);
            Assert.Equal(7, sequence.IncrementBy);
        }

        [Fact]
        public void Build_creates_sequence_defined_on_entity_with_defaults()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder.Entity("A",
                b =>
                {
                    b.Property<int>("Id")
                        .GenerateValuesOnAdd();
                    b.Property<short>("P")
                        .GenerateValuesOnAdd();
                    b.Key("Id");
                    b.ToTable("T", "dbo");
                    b.GenerateValuesUsingSequence();
                })

                .GenerateValuesUsingSequence();

            var databaseBuilder = new SqlServerDatabaseBuilder();
            var database = databaseBuilder.GetDatabase(modelBuilder.Model);

            Assert.Equal(1, database.Sequences.Count);

            var sequence = database.Sequences[0];

            Assert.Equal("dbo_T_Sequence", sequence.Name);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceDataType, sequence.DataType);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceStartWith, sequence.StartWith);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceBlockSize, sequence.IncrementBy);
        }

        [Fact]
        public void Build_creates_sequence_defined_on_model()
        {
            var modelBuilder = new BasicModelBuilder();
            modelBuilder.GenerateValuesUsingSequence("MySequence", 7);

            modelBuilder.Entity("A", b =>
            {
                b.Property<int>("Id")
                    .GenerateValuesOnAdd();
                b.Key("Id");
            });
            modelBuilder.Entity("B", b =>
            {
                b.Property<short>("Id")
                    .GenerateValuesOnAdd();
                b.Key("Id");
            });

            var databaseBuilder = new SqlServerDatabaseBuilder();
            var database = databaseBuilder.GetDatabase(modelBuilder.Model);

            Assert.Equal(1, database.Sequences.Count);

            var sequence = database.Sequences[0];

            Assert.Equal("MySequence", sequence.Name);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceDataType, sequence.DataType);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceStartWith, sequence.StartWith);
            Assert.Equal(7, sequence.IncrementBy);
        }

        [Fact]
        public void Build_creates_sequence_defined_on_model_with_defaults()
        {
            var modelBuilder = new BasicModelBuilder();
            modelBuilder.GenerateValuesUsingSequence();

            modelBuilder.Entity("A", b =>
            {
                b.Property<int>("Id")
                    .GenerateValuesOnAdd();
                b.Key("Id");
                b.ToTable("T0", "dbo");
            });
            modelBuilder.Entity("B", b =>
            {
                b.Property<short>("Id")
                    .GenerateValuesOnAdd();
                b.Key("Id");
                b.ToTable("T1", "dbo");
            });

            var databaseBuilder = new SqlServerDatabaseBuilder();
            var database = databaseBuilder.GetDatabase(modelBuilder.Model);

            Assert.Equal(2, database.Sequences.Count);

            var sequence0 = database.Sequences[0];

            Assert.Equal("dbo_T0_Sequence", sequence0.Name);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceDataType, sequence0.DataType);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceStartWith, sequence0.StartWith);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceBlockSize, sequence0.IncrementBy);

            var sequence1 = database.Sequences[1];

            Assert.Equal("dbo_T1_Sequence", sequence1.Name);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceDataType, sequence1.DataType);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceStartWith, sequence1.StartWith);
            Assert.Equal(Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceBlockSize, sequence1.IncrementBy);
        }
    }
}
