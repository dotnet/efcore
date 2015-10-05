// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Tests;
using Microsoft.Data.Entity.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerValueGeneratorCacheTest
    {
        [Fact]
        public void Uses_single_generator_per_property()
        {
            var model = CreateModel();
            var entityType = model.GetEntityType(typeof(Led));
            var property1 = GetProperty1(model);
            var property2 = GetProperty2(model);
            var cache = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<ISqlServerValueGeneratorCache>();

            var generator1 = cache.GetOrAdd(property1, entityType, (p, et) => new TemporaryNumberValueGenerator<int>());
            Assert.NotNull(generator1);
            Assert.Same(generator1, cache.GetOrAdd(property1, entityType, (p, et) => new TemporaryNumberValueGenerator<int>()));

            var generator2 = cache.GetOrAdd(property2, entityType, (p, et) => new TemporaryNumberValueGenerator<int>());
            Assert.NotNull(generator2);
            Assert.Same(generator2, cache.GetOrAdd(property2, entityType, (p, et) => new TemporaryNumberValueGenerator<int>()));
            Assert.NotSame(generator1, generator2);
        }

        [Fact]
        public void Uses_single_sequence_generator_per_sequence()
        {
            var model = CreateModel();
            var property1 = GetProperty1(model);
            var property2 = GetProperty2(model);
            var property3 = GetProperty3(model);
            var cache = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<ISqlServerValueGeneratorCache>();

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
                .UseSqlServerSequenceHiLo()
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal(10, cache.GetOrAddSequenceState(property).Sequence.IncrementBy);
        }

        [Fact]
        public void Block_size_is_obtained_from_named_sequence()
        {
            var property = CreateConventionModelBuilder()
                .Entity<Robot>()
                .Property(e => e.Id)
                .UseSqlServerSequenceHiLo("DaneelOlivaw")
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal(10, cache.GetOrAddSequenceState(property).Sequence.IncrementBy);
        }

        [Fact]
        public void Block_size_is_obtained_from_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .UseSqlServerSequenceHiLo()
                .Entity<Robot>()
                .Property(e => e.Id)
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal(10, cache.GetOrAddSequenceState(property).Sequence.IncrementBy);
        }

        [Fact]
        public void Block_size_is_obtained_from_named_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .UseSqlServerSequenceHiLo("DaneelOlivaw")
                .Entity<Robot>()
                .Property(e => e.Id)
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal(10, cache.GetOrAddSequenceState(property).Sequence.IncrementBy);
        }

        [Fact]
        public void Block_size_is_obtained_from_specified_sequence()
        {
            var property = CreateConventionModelBuilder()
                .SqlServerSequence("DaneelOlivaw", b => b.IncrementsBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .UseSqlServerSequenceHiLo("DaneelOlivaw")
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal(11, cache.GetOrAddSequenceState(property).Sequence.IncrementBy);
        }

        [Fact]
        public void Non_positive_block_sizes_are_not_allowed()
        {
            var property = CreateConventionModelBuilder()
                .SqlServerSequence("DaneelOlivaw", b => b.IncrementsBy(-1))
                .Entity<Robot>()
                .Property(e => e.Id)
                .UseSqlServerSequenceHiLo("DaneelOlivaw")
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.StartsWith(
                CoreStrings.HiLoBadBlockSize,
                Assert.Throws<ArgumentOutOfRangeException>(() => cache.GetOrAddSequenceState(property).Sequence.IncrementBy).Message);
        }

        [Fact]
        public void Block_size_is_obtained_from_specified_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .UseSqlServerSequenceHiLo("DaneelOlivaw")
                .SqlServerSequence("DaneelOlivaw", b => b.IncrementsBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal(11, cache.GetOrAddSequenceState(property).Sequence.IncrementBy);
        }

        [Fact]
        public void Sequence_name_is_obtained_from_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .Entity<Robot>()
                .Property(e => e.Id)
                .UseSqlServerSequenceHiLo()
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("EntityFrameworkHiLoSequence", cache.GetOrAddSequenceState(property).Sequence.Name);
        }

        [Fact]
        public void Sequence_name_is_obtained_from_named_sequence()
        {
            var property = CreateConventionModelBuilder()
                .Entity<Robot>()
                .Property(e => e.Id)
                .UseSqlServerSequenceHiLo("DaneelOlivaw")
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("DaneelOlivaw", cache.GetOrAddSequenceState(property).Sequence.Name);
        }

        [Fact]
        public void Sequence_name_is_obtained_from_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .UseSqlServerSequenceHiLo()
                .Entity<Robot>()
                .Property(e => e.Id)
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("EntityFrameworkHiLoSequence", cache.GetOrAddSequenceState(property).Sequence.Name);
        }

        [Fact]
        public void Sequence_name_is_obtained_from_named_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .UseSqlServerSequenceHiLo("DaneelOlivaw")
                .Entity<Robot>()
                .Property(e => e.Id)
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("DaneelOlivaw", cache.GetOrAddSequenceState(property).Sequence.Name);
        }

        [Fact]
        public void Sequence_name_is_obtained_from_specified_sequence()
        {
            var property = CreateConventionModelBuilder()
                .SqlServerSequence("DaneelOlivaw", b => b.IncrementsBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .UseSqlServerSequenceHiLo("DaneelOlivaw")
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("DaneelOlivaw", cache.GetOrAddSequenceState(property).Sequence.Name);
        }

        [Fact]
        public void Sequence_name_is_obtained_from_specified_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .UseSqlServerSequenceHiLo("DaneelOlivaw")
                .SqlServerSequence("DaneelOlivaw", b => b.IncrementsBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("DaneelOlivaw", cache.GetOrAddSequenceState(property).Sequence.Name);
        }

        [Fact]
        public void Schema_qualified_sequence_name_is_obtained_from_named_sequence()
        {
            var property = CreateConventionModelBuilder()
                .Entity<Robot>()
                .Property(e => e.Id)
                .UseSqlServerSequenceHiLo("DaneelOlivaw", "R")
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("DaneelOlivaw", cache.GetOrAddSequenceState(property).Sequence.Name);
            Assert.Equal("R", cache.GetOrAddSequenceState(property).Sequence.Schema);
        }

        [Fact]
        public void Schema_qualified_sequence_name_is_obtained_from_named_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .UseSqlServerSequenceHiLo("DaneelOlivaw", "R")
                .Entity<Robot>()
                .Property(e => e.Id)
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("DaneelOlivaw", cache.GetOrAddSequenceState(property).Sequence.Name);
            Assert.Equal("R", cache.GetOrAddSequenceState(property).Sequence.Schema);
        }

        [Fact]
        public void Schema_qualified_sequence_name_is_obtained_from_specified_sequence()
        {
            var property = CreateConventionModelBuilder()
                .SqlServerSequence("DaneelOlivaw", "R", b => b.IncrementsBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .UseSqlServerSequenceHiLo("DaneelOlivaw", "R")
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("DaneelOlivaw", cache.GetOrAddSequenceState(property).Sequence.Name);
            Assert.Equal("R", cache.GetOrAddSequenceState(property).Sequence.Schema);
        }

        [Fact]
        public void Schema_qualified_sequence_name_is_obtained_from_specified_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .UseSqlServerSequenceHiLo("DaneelOlivaw", "R")
                .SqlServerSequence("DaneelOlivaw", "R", b => b.IncrementsBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .Metadata;

            var cache = new SqlServerValueGeneratorCache();

            Assert.Equal("DaneelOlivaw", cache.GetOrAddSequenceState(property).Sequence.Name);
            Assert.Equal("R", cache.GetOrAddSequenceState(property).Sequence.Schema);
        }

        protected virtual ModelBuilder CreateConventionModelBuilder() => SqlServerTestHelpers.Instance.CreateConventionBuilder();

        private static Property CreateProperty()
        {
            var entityType = new Model().AddEntityType("MyType");
            var property = entityType.AddProperty("MyProperty", typeof(string));
            entityType.SqlServer().TableName = "MyTable";

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

            modelBuilder.Sequence("Heaven");
            modelBuilder.Sequence("Rosie");

            modelBuilder.Entity<Led>(b =>
                {
                    b.Property(e => e.Zeppelin).UseSqlServerSequenceHiLo("Heaven");
                    b.Property(e => e.Stairway).UseSqlServerSequenceHiLo("Heaven");
                    b.Property(e => e.WholeLotta).UseSqlServerSequenceHiLo("Rosie");
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
