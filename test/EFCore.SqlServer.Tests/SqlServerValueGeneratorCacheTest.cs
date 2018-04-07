// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerValueGeneratorCacheTest
    {
        [Fact]
        public void Uses_single_generator_per_property()
        {
            var model = CreateModel();
            var entityType = model.FindEntityType(typeof(Led));
            var property1 = GetProperty1(model);
            var property2 = GetProperty2(model);
            var cache = SqlServerTestHelpers.Instance.CreateContextServices(model).GetRequiredService<ISqlServerValueGeneratorCache>();

            var generator1 = cache.GetOrAdd(property1, entityType, (p, et) => new TemporaryIntValueGenerator());
            Assert.NotNull(generator1);
            Assert.Same(generator1, cache.GetOrAdd(property1, entityType, (p, et) => new TemporaryIntValueGenerator()));

            var generator2 = cache.GetOrAdd(property2, entityType, (p, et) => new TemporaryIntValueGenerator());
            Assert.NotNull(generator2);
            Assert.Same(generator2, cache.GetOrAdd(property2, entityType, (p, et) => new TemporaryIntValueGenerator()));
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
                .ForSqlServerUseSequenceHiLo()
                .Metadata;

            var cache = new SqlServerValueGeneratorCache(new ValueGeneratorCacheDependencies());

            Assert.Equal(10, cache.GetOrAddSequenceState(property).Sequence.IncrementBy);
        }

        [Fact]
        public void Block_size_is_obtained_from_named_sequence()
        {
            var property = CreateConventionModelBuilder()
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServerUseSequenceHiLo("DaneelOlivaw")
                .Metadata;

            var cache = new SqlServerValueGeneratorCache(new ValueGeneratorCacheDependencies());

            Assert.Equal(10, cache.GetOrAddSequenceState(property).Sequence.IncrementBy);
        }

        [Fact]
        public void Block_size_is_obtained_from_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServerUseSequenceHiLo()
                .Entity<Robot>()
                .Property(e => e.Id)
                .Metadata;

            var cache = new SqlServerValueGeneratorCache(new ValueGeneratorCacheDependencies());

            Assert.Equal(10, cache.GetOrAddSequenceState(property).Sequence.IncrementBy);
        }

        [Fact]
        public void Block_size_is_obtained_from_named_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServerUseSequenceHiLo("DaneelOlivaw")
                .Entity<Robot>()
                .Property(e => e.Id)
                .Metadata;

            var cache = new SqlServerValueGeneratorCache(new ValueGeneratorCacheDependencies());

            Assert.Equal(10, cache.GetOrAddSequenceState(property).Sequence.IncrementBy);
        }

        [Fact]
        public void Block_size_is_obtained_from_specified_sequence()
        {
            var property = CreateConventionModelBuilder()
                .HasSequence("DaneelOlivaw", b => b.IncrementsBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServerUseSequenceHiLo("DaneelOlivaw")
                .Metadata;

            var cache = new SqlServerValueGeneratorCache(new ValueGeneratorCacheDependencies());

            Assert.Equal(11, cache.GetOrAddSequenceState(property).Sequence.IncrementBy);
        }

        [Fact]
        public void Non_positive_block_sizes_are_not_allowed()
        {
            var property = CreateConventionModelBuilder()
                .HasSequence("DaneelOlivaw", b => b.IncrementsBy(-1))
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServerUseSequenceHiLo("DaneelOlivaw")
                .Metadata;

            var cache = new SqlServerValueGeneratorCache(new ValueGeneratorCacheDependencies());

            Assert.StartsWith(
                CoreStrings.HiLoBadBlockSize,
                Assert.Throws<ArgumentOutOfRangeException>(() => cache.GetOrAddSequenceState(property).Sequence.IncrementBy).Message);
        }

        [Fact]
        public void Block_size_is_obtained_from_specified_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServerUseSequenceHiLo("DaneelOlivaw")
                .HasSequence("DaneelOlivaw", b => b.IncrementsBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .Metadata;

            var cache = new SqlServerValueGeneratorCache(new ValueGeneratorCacheDependencies());

            Assert.Equal(11, cache.GetOrAddSequenceState(property).Sequence.IncrementBy);
        }

        [Fact]
        public void Sequence_name_is_obtained_from_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServerUseSequenceHiLo()
                .Metadata;

            var cache = new SqlServerValueGeneratorCache(new ValueGeneratorCacheDependencies());

            Assert.Equal("EntityFrameworkHiLoSequence", cache.GetOrAddSequenceState(property).Sequence.Name);
        }

        [Fact]
        public void Sequence_name_is_obtained_from_named_sequence()
        {
            var property = CreateConventionModelBuilder()
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServerUseSequenceHiLo("DaneelOlivaw")
                .Metadata;

            var cache = new SqlServerValueGeneratorCache(new ValueGeneratorCacheDependencies());

            Assert.Equal("DaneelOlivaw", cache.GetOrAddSequenceState(property).Sequence.Name);
        }

        [Fact]
        public void Sequence_name_is_obtained_from_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServerUseSequenceHiLo()
                .Entity<Robot>()
                .Property(e => e.Id)
                .Metadata;

            var cache = new SqlServerValueGeneratorCache(new ValueGeneratorCacheDependencies());

            Assert.Equal("EntityFrameworkHiLoSequence", cache.GetOrAddSequenceState(property).Sequence.Name);
        }

        [Fact]
        public void Sequence_name_is_obtained_from_named_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServerUseSequenceHiLo("DaneelOlivaw")
                .Entity<Robot>()
                .Property(e => e.Id)
                .Metadata;

            var cache = new SqlServerValueGeneratorCache(new ValueGeneratorCacheDependencies());

            Assert.Equal("DaneelOlivaw", cache.GetOrAddSequenceState(property).Sequence.Name);
        }

        [Fact]
        public void Sequence_name_is_obtained_from_specified_sequence()
        {
            var property = CreateConventionModelBuilder()
                .HasSequence("DaneelOlivaw", b => b.IncrementsBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServerUseSequenceHiLo("DaneelOlivaw")
                .Metadata;

            var cache = new SqlServerValueGeneratorCache(new ValueGeneratorCacheDependencies());

            Assert.Equal("DaneelOlivaw", cache.GetOrAddSequenceState(property).Sequence.Name);
        }

        [Fact]
        public void Sequence_name_is_obtained_from_specified_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServerUseSequenceHiLo("DaneelOlivaw")
                .HasSequence("DaneelOlivaw", b => b.IncrementsBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .Metadata;

            var cache = new SqlServerValueGeneratorCache(new ValueGeneratorCacheDependencies());

            Assert.Equal("DaneelOlivaw", cache.GetOrAddSequenceState(property).Sequence.Name);
        }

        [Fact]
        public void Schema_qualified_sequence_name_is_obtained_from_named_sequence()
        {
            var property = CreateConventionModelBuilder()
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServerUseSequenceHiLo("DaneelOlivaw", "R")
                .Metadata;

            var cache = new SqlServerValueGeneratorCache(new ValueGeneratorCacheDependencies());

            Assert.Equal("DaneelOlivaw", cache.GetOrAddSequenceState(property).Sequence.Name);
            Assert.Equal("R", cache.GetOrAddSequenceState(property).Sequence.Schema);
        }

        [Fact]
        public void Schema_qualified_sequence_name_is_obtained_from_named_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServerUseSequenceHiLo("DaneelOlivaw", "R")
                .Entity<Robot>()
                .Property(e => e.Id)
                .Metadata;

            var cache = new SqlServerValueGeneratorCache(new ValueGeneratorCacheDependencies());

            Assert.Equal("DaneelOlivaw", cache.GetOrAddSequenceState(property).Sequence.Name);
            Assert.Equal("R", cache.GetOrAddSequenceState(property).Sequence.Schema);
        }

        [Fact]
        public void Schema_qualified_sequence_name_is_obtained_from_specified_sequence()
        {
            var property = CreateConventionModelBuilder()
                .HasSequence("DaneelOlivaw", "R", b => b.IncrementsBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .ForSqlServerUseSequenceHiLo("DaneelOlivaw", "R")
                .Metadata;

            var cache = new SqlServerValueGeneratorCache(new ValueGeneratorCacheDependencies());

            Assert.Equal("DaneelOlivaw", cache.GetOrAddSequenceState(property).Sequence.Name);
            Assert.Equal("R", cache.GetOrAddSequenceState(property).Sequence.Schema);
        }

        [Fact]
        public void Schema_qualified_sequence_name_is_obtained_from_specified_model_default_sequence()
        {
            var property = CreateConventionModelBuilder()
                .ForSqlServerUseSequenceHiLo("DaneelOlivaw", "R")
                .HasSequence("DaneelOlivaw", "R", b => b.IncrementsBy(11))
                .Entity<Robot>()
                .Property(e => e.Id)
                .Metadata;

            var cache = new SqlServerValueGeneratorCache(new ValueGeneratorCacheDependencies());

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

        private static IProperty GetProperty1(IModel model) => model.FindEntityType(typeof(Led)).FindProperty("Zeppelin");

        private static IProperty GetProperty2(IModel model) => model.FindEntityType(typeof(Led)).FindProperty("Stairway");

        private static IProperty GetProperty3(IModel model) => model.FindEntityType(typeof(Led)).FindProperty("WholeLotta");

        private static IModel CreateModel()
        {
            var modelBuilder = SqlServerTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.HasSequence("Heaven");
            modelBuilder.HasSequence("Rosie");

            modelBuilder.Entity<Led>(
                b =>
                    {
                        b.Property(e => e.Zeppelin).ForSqlServerUseSequenceHiLo("Heaven");
                        b.Property(e => e.Stairway).ForSqlServerUseSequenceHiLo("Heaven");
                        b.Property(e => e.WholeLotta).ForSqlServerUseSequenceHiLo("Rosie");
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
