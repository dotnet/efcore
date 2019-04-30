// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class SqlServerInternalMetadataBuilderExtensionsTest
    {
        private IConventionModelBuilder CreateBuilder()
            => new InternalModelBuilder(new Model());

        [Fact]
        public void Can_access_model()
        {
            var builder = CreateBuilder();

            Assert.NotNull(builder
                .ForSqlServerHasValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo));
            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, builder.Metadata.GetSqlServerValueGenerationStrategy());

            Assert.NotNull(builder
                    .ForSqlServerHasValueGenerationStrategy(SqlServerValueGenerationStrategy.IdentityColumn, fromDataAnnotation: true));
            Assert.Equal(
                SqlServerValueGenerationStrategy.IdentityColumn, builder.Metadata.GetSqlServerValueGenerationStrategy());

            Assert.Null(builder
                .ForSqlServerHasValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo));
            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, builder.Metadata.GetSqlServerValueGenerationStrategy());

            Assert.Equal(
                1, builder.Metadata.GetAnnotations().Count(
                    a => a.Name.StartsWith(SqlServerAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_entity_type()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot));

            Assert.NotNull(typeBuilder.ForSqlServerIsMemoryOptimized(true));
            Assert.True(typeBuilder.Metadata.GetSqlServerIsMemoryOptimized());

            Assert.NotNull(typeBuilder.ForSqlServerIsMemoryOptimized(false, fromDataAnnotation: true));
            Assert.False(typeBuilder.Metadata.GetSqlServerIsMemoryOptimized());

            Assert.Null(typeBuilder.ForSqlServerIsMemoryOptimized(true));
            Assert.False(typeBuilder.Metadata.GetSqlServerIsMemoryOptimized());

            Assert.Equal(
                1, typeBuilder.Metadata.GetAnnotations().Count(
                    a => a.Name.StartsWith(SqlServerAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_property()
        {
            var propertyBuilder = CreateBuilder()
                .Entity(typeof(Splot))
                .Property(typeof(int), "Id");

            Assert.NotNull(propertyBuilder.ForSqlServerHasHiLoSequence("Splew", null));
            Assert.Equal("Splew", propertyBuilder.Metadata.GetSqlServerHiLoSequenceName());

            Assert.NotNull(propertyBuilder.ForSqlServerHasHiLoSequence("Splow", null, fromDataAnnotation: true));
            Assert.Equal("Splow", propertyBuilder.Metadata.GetSqlServerHiLoSequenceName());

            Assert.Null(propertyBuilder.ForSqlServerHasHiLoSequence("Splod", null));
            Assert.Equal("Splow", propertyBuilder.Metadata.GetSqlServerHiLoSequenceName());

            Assert.Equal(
                1, propertyBuilder.Metadata.GetAnnotations().Count(
                    a => a.Name.StartsWith(SqlServerAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Throws_setting_sequence_generation_for_invalid_type()
        {
            var propertyBuilder = CreateBuilder()
                .Entity(typeof(Splot))
                .Property(typeof(string), "Name");

            Assert.Equal(
                SqlServerStrings.SequenceBadType("Name", nameof(Splot), "string"),
                Assert.Throws<ArgumentException>(
                    () => propertyBuilder.ForSqlServerHasValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo)).Message);

            Assert.Equal(
                SqlServerStrings.SequenceBadType("Name", nameof(Splot), "string"),
                Assert.Throws<ArgumentException>(
                    () => new PropertyBuilder((IMutableProperty)propertyBuilder.Metadata).ForSqlServerUseSequenceHiLo()).Message);
        }

        [Fact]
        public void Throws_setting_identity_generation_for_invalid_type_only_with_explicit()
        {
            var propertyBuilder = CreateBuilder()
                .Entity(typeof(Splot))
                .Property(typeof(string), "Name");

            Assert.Equal(
                SqlServerStrings.IdentityBadType("Name", nameof(Splot), "string"),
                Assert.Throws<ArgumentException>(
                    () => propertyBuilder.ForSqlServerHasValueGenerationStrategy(SqlServerValueGenerationStrategy.IdentityColumn)).Message);

            Assert.Equal(
                SqlServerStrings.IdentityBadType("Name", nameof(Splot), "string"),
                Assert.Throws<ArgumentException>(
                    () => new PropertyBuilder((IMutableProperty)propertyBuilder.Metadata).ForSqlServerUseIdentityColumn()).Message);
        }

        [Fact]
        public void Can_access_key()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot));
            var idProperty = entityTypeBuilder.Property(typeof(string), "Id").Metadata;
            var keyBuilder = entityTypeBuilder.HasKey(new[] { idProperty });

            Assert.NotNull(keyBuilder.ForSqlServerIsClustered(true));
            Assert.True(keyBuilder.Metadata.GetSqlServerIsClustered());

            Assert.NotNull(keyBuilder.ForSqlServerIsClustered(false, fromDataAnnotation: true));
            Assert.False(keyBuilder.Metadata.GetSqlServerIsClustered());

            Assert.Null(keyBuilder.ForSqlServerIsClustered(true));
            Assert.False(keyBuilder.Metadata.GetSqlServerIsClustered());

            Assert.Equal(
                1, keyBuilder.Metadata.GetAnnotations().Count(
                    a => a.Name.StartsWith(SqlServerAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_index()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot));
            var idProperty = entityTypeBuilder.Property(typeof(int), "Id").Metadata;
            var indexBuilder = entityTypeBuilder.HasIndex(new[] { idProperty });

            Assert.NotNull(indexBuilder.ForSqlServerIsClustered(true));
            Assert.True(indexBuilder.Metadata.GetSqlServerIsClustered());

            Assert.NotNull(indexBuilder.ForSqlServerIsClustered(false, fromDataAnnotation: true));
            Assert.False(indexBuilder.Metadata.GetSqlServerIsClustered());

            Assert.Null(indexBuilder.ForSqlServerIsClustered(true));
            Assert.False(indexBuilder.Metadata.GetSqlServerIsClustered());

            Assert.Equal(
                1, indexBuilder.Metadata.GetAnnotations().Count(
                    a => a.Name.StartsWith(SqlServerAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_relationship()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot));
            var idProperty = entityTypeBuilder.Property(typeof(int), "Id").Metadata;
            var key = entityTypeBuilder.HasKey(new[] { idProperty }).Metadata;
            var relationshipBuilder = entityTypeBuilder.HasRelationship(entityTypeBuilder.Metadata, key);

            Assert.NotNull(relationshipBuilder.HasConstraintName("Splew"));
            Assert.Equal("Splew", relationshipBuilder.Metadata.GetConstraintName());

            Assert.NotNull(relationshipBuilder.HasConstraintName("Splow", fromDataAnnotation: true));
            Assert.Equal("Splow", relationshipBuilder.Metadata.GetConstraintName());

            Assert.Null(relationshipBuilder.HasConstraintName("Splod"));
            Assert.Equal("Splow", relationshipBuilder.Metadata.GetConstraintName());

            Assert.Equal(
                1, relationshipBuilder.Metadata.GetAnnotations().Count(
                    a => a.Name.StartsWith(RelationalAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        private class Splot
        {
        }
    }
}
