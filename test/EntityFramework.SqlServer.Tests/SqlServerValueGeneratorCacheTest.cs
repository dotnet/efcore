// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Tests;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerValueGeneratorCacheTest
    {
        [Fact]
        public void Uses_single_generator_per_property()
        {
            var model = CreateModel();
            var property1 = GetProperty1(model);
            var property2 = GetProperty2(model);
            var cache = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<SqlServerValueGeneratorCache>();

            var generator1 = cache.GetOrAdd(property1, p => new TemporaryIntegerValueGenerator<int>());
            Assert.NotNull(generator1);
            Assert.Same(generator1, cache.GetOrAdd(property1, p => new TemporaryIntegerValueGenerator<int>()));

            var generator2 = cache.GetOrAdd(property2, p => new TemporaryIntegerValueGenerator<int>());
            Assert.NotNull(generator2);
            Assert.Same(generator2, cache.GetOrAdd(property2, p => new TemporaryIntegerValueGenerator<int>()));
            Assert.NotSame(generator1, generator2);
        }

        [Fact]
        public void Uses_single_sequence_generator_per_sequence()
        {
            var model = CreateModel();
            var property1 = GetProperty1(model);
            var property2 = GetProperty2(model);
            var property3 = GetProperty3(model);
            var cache = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<SqlServerValueGeneratorCache>();

            var generator1 = cache.GetOrAddSequenceState(property1);
            Assert.NotNull(generator1);
            Assert.Same(generator1, cache.GetOrAddSequenceState(property1));

            var generator2 = cache.GetOrAddSequenceState(property2);
            Assert.NotNull(generator2);
            Assert.Same(generator2, cache.GetOrAddSequenceState(property2));
            Assert.Same(generator1, generator2);

            var generator3 = cache.GetOrAddSequenceState(property3);
            Assert.NotNull(generator3);
            Assert.Same(generator3, cache.GetOrAddSequenceState(property3));
            Assert.NotSame(generator1, generator3);
        }

        [Fact]
        public void Block_size_is_obtained_from_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence())
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal(10, cache.GetBlockSize(property));
        }

        [Fact]
        public void Block_size_is_obtained_from_named_sequence()
        {
            var property = CreateConventionModelBuilder()
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw"))
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal(10, cache.GetBlockSize(property));
        }

        [Fact]
        public void Block_size_is_obtained_from_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServer(b => b.UseSequence())
                .Entity<Robot>()
                .Property(e => e.Id)
                .GenerateValueOnAdd()
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal(10, cache.GetBlockSize(property));
        }

        [Fact]
        public void Block_size_is_obtained_from_named_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw"))
                .Entity<Robot>()
                .Property(e => e.Id)
                .GenerateValueOnAdd()
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal(10, cache.GetBlockSize(property));
        }

        [Fact]
        public void Block_size_is_obtained_from_specified_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServer(b => b.Sequence().IncrementBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence())
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal(11, cache.GetBlockSize(property));
        }

        [Fact]
        public void Block_size_is_obtained_from_specified_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServer(b => b.Sequence("DaneelOlivaw").IncrementBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw"))
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal(11, cache.GetBlockSize(property));
        }

        [Fact]
        public void Block_size_is_obtained_from_model_specified_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServer(b =>
                    {
                        b.UseSequence();
                        b.Sequence().IncrementBy(11);
                    })
                .Entity<Robot>()
                .Property(e => e.Id)
                .GenerateValueOnAdd()
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal(11, cache.GetBlockSize(property));
        }

        [Fact]
        public void Non_positive_block_sizes_are_not_allowed()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServer(b => b.Sequence("DaneelOlivaw").IncrementBy(-1))
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw"))
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal(
                Strings.SequenceBadBlockSize(-1, "DaneelOlivaw"),
                Assert.Throws<NotSupportedException>(() => cache.GetBlockSize(property)).Message);
        }

        [Fact]
        public void Block_size_is_obtained_from_specified_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServer(b =>
                    {
                        b.UseSequence("DaneelOlivaw");
                        b.Sequence("DaneelOlivaw").IncrementBy(11);
                    })
                .Entity<Robot>()
                .Property(e => e.Id)
                .GenerateValueOnAdd()
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal(11, cache.GetBlockSize(property));
        }

        [Fact]
        public void Sequence_name_is_obtained_from_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence())
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("DefaultSequence", cache.GetSequenceName(property));
        }

        [Fact]
        public void Sequence_name_is_obtained_from_named_sequence()
        {
            var property = CreateConventionModelBuilder()
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw"))
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("DaneelOlivaw", cache.GetSequenceName(property));
        }

        [Fact]
        public void Sequence_name_is_obtained_from_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServer(b => b.UseSequence())
                .Entity<Robot>()
                .Property(e => e.Id)
                .GenerateValueOnAdd()
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("DefaultSequence", cache.GetSequenceName(property));
        }

        [Fact]
        public void Sequence_name_is_obtained_from_named_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw"))
                .Entity<Robot>()
                .Property(e => e.Id)
                .GenerateValueOnAdd()
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("DaneelOlivaw", cache.GetSequenceName(property));
        }

        [Fact]
        public void Sequence_name_is_obtained_from_specified_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServer(b => b.Sequence().IncrementBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence())
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("DefaultSequence", cache.GetSequenceName(property));
        }

        [Fact]
        public void Sequence_name_is_obtained_from_specified_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServer(b => b.Sequence("DaneelOlivaw").IncrementBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw"))
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("DaneelOlivaw", cache.GetSequenceName(property));
        }

        [Fact]
        public void Sequence_name_is_obtained_from_model_specified_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServer(b =>
                    {
                        b.UseSequence();
                        b.Sequence().IncrementBy(11);
                    })
                .Entity<Robot>()
                .Property(e => e.Id)
                .GenerateValueOnAdd()
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("DefaultSequence", cache.GetSequenceName(property));
        }

        [Fact]
        public void Sequence_name_is_obtained_from_specified_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServer(b =>
                    {
                        b.UseSequence("DaneelOlivaw");
                        b.Sequence("DaneelOlivaw").IncrementBy(11);
                    })
                .Entity<Robot>()
                .Property(e => e.Id)
                .GenerateValueOnAdd()
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("DaneelOlivaw", cache.GetSequenceName(property));
        }

        [Fact]
        public void Schema_qualified_sequence_name_is_obtained_from_named_sequence()
        {
            var property = CreateConventionModelBuilder()
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw", "R"))
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("R.DaneelOlivaw", cache.GetSequenceName(property));
        }

        [Fact]
        public void Schema_qualified_sequence_name_is_obtained_from_named_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw", "R"))
                .Entity<Robot>()
                .Property(e => e.Id)
                .GenerateValueOnAdd()
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("R.DaneelOlivaw", cache.GetSequenceName(property));
        }

        [Fact]
        public void Schema_qualified_sequence_name_is_obtained_from_specified_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServer(b => b.Sequence("DaneelOlivaw", "R").IncrementBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServer(b => b.UseSequence("DaneelOlivaw", "R"))
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("R.DaneelOlivaw", cache.GetSequenceName(property));
        }

        [Fact]
        public void Schema_qualified_sequence_name_is_obtained_from_specified_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServer(b =>
                    {
                        b.UseSequence("DaneelOlivaw", "R");
                        b.Sequence("DaneelOlivaw", "R").IncrementBy(11);
                    })
                .Entity<Robot>()
                .Property(e => e.Id)
                .GenerateValueOnAdd()
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("R.DaneelOlivaw", cache.GetSequenceName(property));
        }

        [Fact]
        public void Returns_the_default_pool_size()
        {
            var property = CreateProperty();

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal(5, cache.GetPoolSize(property));
        }

        protected virtual ModelBuilder CreateConventionModelBuilder()
        {
            return SqlServerTestHelpers.Instance.CreateConventionBuilder();
        }

        private static Property CreateProperty()
        {
            var entityType = new Model().AddEntityType("MyType");
            var property = entityType.GetOrAddProperty("MyProperty", typeof(string), shadowProperty: true);
            entityType.SqlServer().Table = "MyTable";

            return property;
        }

        private class Robot
        {
            public int Id { get; set; }
        }

        private static IProperty GetProperty1(IModel model) => model.GetEntityType(typeof(Led)).GetProperty("Zeppelin");

        private static IProperty GetProperty2(IModel model) => model.GetEntityType(typeof(Led)).GetProperty("Stairway");

        private static IProperty GetProperty3(IModel model) => model.GetEntityType(typeof(Led)).GetProperty("WholeLotta");

        private static IModel CreateModel()
        {
            var modelBuilder = SqlServerTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.ForRelational().Sequence("Heaven");
            modelBuilder.ForRelational().Sequence("Rosie");

            modelBuilder.Entity<Led>(b =>
                {
                    b.Property(e => e.Zeppelin).ForSqlServer().UseSequence("Heaven");
                    b.Property(e => e.Stairway).ForSqlServer().UseSequence("Heaven");
                    b.Property(e => e.WholeLotta).ForSqlServer().UseSequence("Rosie");
                });

            return modelBuilder.Model;
        }

        private class Led
        {
            public int Zeppelin { get; set; }
            public int Stairway { get; set; }
            public int WholeLotta { get; set; }
        }
    }
}
