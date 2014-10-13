// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Services;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerSequenceValueGeneratorFactoryTest
    {
        [Fact]
        public void Block_size_is_obtained_from_default_sequence()
        {
            var property = new ModelBuilder()
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence())
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal(10, factory.GetBlockSize(property));
        }

        [Fact]
        public void Block_size_is_obtained_from_named_sequence()
        {
            var property = new ModelBuilder()
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw"))
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal(10, factory.GetBlockSize(property));
        }

        [Fact]
        public void Block_size_is_obtained_from_model_default_sequence()
        {
            var property = new ModelBuilder()
                .ForSqlServer(b => b.UseSequence())
                .Entity<Robot>()
                .Property(e => e.Id)
                .GenerateValuesOnAdd()
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal(10, factory.GetBlockSize(property));
        }

        [Fact]
        public void Block_size_is_obtained_from_named_model_default_sequence()
        {
            var property = new ModelBuilder()
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw"))
                .Entity<Robot>()
                .Property(e => e.Id)
                .GenerateValuesOnAdd()
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal(10, factory.GetBlockSize(property));
        }

        [Fact]
        public void Block_size_is_obtained_from_specified_default_sequence()
        {
            var property = new ModelBuilder()
                .ForSqlServer(b => b.Sequence().IncrementBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence())
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal(11, factory.GetBlockSize(property));
        }

        [Fact]
        public void Block_size_is_obtained_from_specified_sequence()
        {
            var property = new ModelBuilder()
                .ForSqlServer(b => b.Sequence("DaneelOlivaw").IncrementBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw"))
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal(11, factory.GetBlockSize(property));
        }

        [Fact]
        public void Block_size_is_obtained_from_model_specified_model_default_sequence()
        {
            var property = new ModelBuilder()
                .ForSqlServer(b =>
                    {
                        b.UseSequence();
                        b.Sequence().IncrementBy(11);
                    })
                .Entity<Robot>()
                .Property(e => e.Id)
                .GenerateValuesOnAdd()
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal(11, factory.GetBlockSize(property));
        }

        [Fact]
        public void Non_positive_block_sizes_are_not_allowed()
        {
            var property = new ModelBuilder()
                .ForSqlServer(b => b.Sequence("DaneelOlivaw").IncrementBy(-1))
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw"))
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal(
                Strings.FormatSequenceBadBlockSize(-1, "DaneelOlivaw"),
                Assert.Throws<NotSupportedException>(() => factory.GetBlockSize(property)).Message);
        }

        [Fact]
        public void Block_size_is_obtained_from_specified_model_default_sequence()
        {
            var property = new ModelBuilder()
                .ForSqlServer(b =>
                    {
                        b.UseSequence("DaneelOlivaw");
                        b.Sequence("DaneelOlivaw").IncrementBy(11);
                    })
                .Entity<Robot>()
                .Property(e => e.Id)
                .GenerateValuesOnAdd()
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal(11, factory.GetBlockSize(property));
        }

        [Fact]
        public void Sequence_name_is_obtained_from_default_sequence()
        {
            var property = new ModelBuilder()
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence())
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal("EntityFrameworkDefaultSequence", factory.GetSequenceName(property));
        }

        [Fact]
        public void Sequence_name_is_obtained_from_named_sequence()
        {
            var property = new ModelBuilder()
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw"))
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal("DaneelOlivaw", factory.GetSequenceName(property));
        }

        [Fact]
        public void Sequence_name_is_obtained_from_model_default_sequence()
        {
            var property = new ModelBuilder()
                .ForSqlServer(b => b.UseSequence())
                .Entity<Robot>()
                .Property(e => e.Id)
                .GenerateValuesOnAdd()
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal("EntityFrameworkDefaultSequence", factory.GetSequenceName(property));
        }

        [Fact]
        public void Sequence_name_is_obtained_from_named_model_default_sequence()
        {
            var property = new ModelBuilder()
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw"))
                .Entity<Robot>()
                .Property(e => e.Id)
                .GenerateValuesOnAdd()
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal("DaneelOlivaw", factory.GetSequenceName(property));
        }

        [Fact]
        public void Sequence_name_is_obtained_from_specified_default_sequence()
        {
            var property = new ModelBuilder()
                .ForSqlServer(b => b.Sequence().IncrementBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence())
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal("EntityFrameworkDefaultSequence", factory.GetSequenceName(property));
        }

        [Fact]
        public void Sequence_name_is_obtained_from_specified_sequence()
        {
            var property = new ModelBuilder()
                .ForSqlServer(b => b.Sequence("DaneelOlivaw").IncrementBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw"))
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal("DaneelOlivaw", factory.GetSequenceName(property));
        }

        [Fact]
        public void Sequence_name_is_obtained_from_model_specified_model_default_sequence()
        {
            var property = new ModelBuilder()
                .ForSqlServer(b =>
                    {
                        b.UseSequence();
                        b.Sequence().IncrementBy(11);
                    })
                .Entity<Robot>()
                .Property(e => e.Id)
                .GenerateValuesOnAdd()
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal("EntityFrameworkDefaultSequence", factory.GetSequenceName(property));
        }

        [Fact]
        public void Sequence_name_is_obtained_from_specified_model_default_sequence()
        {
            var property = new ModelBuilder()
                .ForSqlServer(b =>
                    {
                        b.UseSequence("DaneelOlivaw");
                        b.Sequence("DaneelOlivaw").IncrementBy(11);
                    })
                .Entity<Robot>()
                .Property(e => e.Id)
                .GenerateValuesOnAdd()
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal("DaneelOlivaw", factory.GetSequenceName(property));
        }

        [Fact]
        public void Schema_qualified_sequence_name_is_obtained_from_named_sequence()
        {
            var property = new ModelBuilder()
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw", "R"))
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal("R.DaneelOlivaw", factory.GetSequenceName(property));
        }

        [Fact]
        public void Schema_qualified_sequence_name_is_obtained_from_named_model_default_sequence()
        {
            var property = new ModelBuilder()
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw", "R"))
                .Entity<Robot>()
                .Property(e => e.Id)
                .GenerateValuesOnAdd()
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal("R.DaneelOlivaw", factory.GetSequenceName(property));
        }

        [Fact]
        public void Schema_qualified_sequence_name_is_obtained_from_specified_sequence()
        {
            var property = new ModelBuilder()
                .ForSqlServer(b => b.Sequence("DaneelOlivaw", "R").IncrementBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw", "R"))
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal("R.DaneelOlivaw", factory.GetSequenceName(property));
        }

        [Fact]
        public void Schema_qualified_sequence_name_is_obtained_from_specified_model_default_sequence()
        {
            var property = new ModelBuilder()
                .ForSqlServer(b =>
                    {
                        b.UseSequence("DaneelOlivaw", "R");
                        b.Sequence("DaneelOlivaw", "R").IncrementBy(11);
                    })
                .Entity<Robot>()
                .Property(e => e.Id)
                .GenerateValuesOnAdd()
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal("R.DaneelOlivaw", factory.GetSequenceName(property));
        }

        [Fact]
        public void Creates_the_appropriate_value_generator()
        {
            var property = new ModelBuilder()
                .ForSqlServer(b => b.Sequence("DaneelOlivaw", "R").IncrementBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw", "R"))
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            var generator = (SqlServerSequenceValueGenerator)factory.Create(property);

            Assert.Equal("R.DaneelOlivaw", generator.SequenceName);
            Assert.Equal(11, generator.BlockSize);
        }

        [Fact]
        public void Returns_the_default_pool_size()
        {
            var property = CreateProperty();

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal(5, factory.GetPoolSize(property));
        }

        [Fact]
        public void Sequence_name_is_the_cache_key()
        {
            var property = new ModelBuilder()
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw", "R"))
                .Metadata;

            var factory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            Assert.Equal("R.DaneelOlivaw", factory.GetCacheKey(property));
        }

        private static Property CreateProperty()
        {
            var entityType = new Model().AddEntityType("MyType");
            var property = entityType.GetOrAddProperty("MyProperty", typeof(string), shadowProperty: true);
            entityType.SetTableName("MyTable");

            return property;
        }

        private class Robot
        {
            public int Id { get; set; }
        }
    }
}
