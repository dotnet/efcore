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

        [ConditionalFact]
        public void Can_access_model()
        {
            var builder = CreateBuilder();

            Assert.NotNull(builder
                .HasValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo));
            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, builder.Metadata.GetValueGenerationStrategy());

            Assert.NotNull(builder
                    .HasValueGenerationStrategy(SqlServerValueGenerationStrategy.IdentityColumn, fromDataAnnotation: true));
            Assert.Equal(
                SqlServerValueGenerationStrategy.IdentityColumn, builder.Metadata.GetValueGenerationStrategy());

            Assert.Null(builder
                .HasValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo));
            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, builder.Metadata.GetValueGenerationStrategy());

            Assert.Equal(
                1, builder.Metadata.GetAnnotations().Count(
                    a => a.Name.StartsWith(SqlServerAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [ConditionalFact]
        public void Can_access_entity_type()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot));

            Assert.NotNull(typeBuilder.IsMemoryOptimized(true));
            Assert.True(typeBuilder.Metadata.IsMemoryOptimized());

            Assert.NotNull(typeBuilder.IsMemoryOptimized(false, fromDataAnnotation: true));
            Assert.False(typeBuilder.Metadata.IsMemoryOptimized());

            Assert.Null(typeBuilder.IsMemoryOptimized(true));
            Assert.False(typeBuilder.Metadata.IsMemoryOptimized());

            Assert.Equal(
                1, typeBuilder.Metadata.GetAnnotations().Count(
                    a => a.Name.StartsWith(SqlServerAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [ConditionalFact]
        public void Can_access_property()
        {
            var propertyBuilder = CreateBuilder()
                .Entity(typeof(Splot))
                .Property(typeof(int), "Id");

            Assert.NotNull(propertyBuilder.HasHiLoSequence("Splew", null));
            Assert.Equal("Splew", propertyBuilder.Metadata.GetHiLoSequenceName());

            Assert.NotNull(propertyBuilder.HasHiLoSequence("Splow", null, fromDataAnnotation: true));
            Assert.Equal("Splow", propertyBuilder.Metadata.GetHiLoSequenceName());

            Assert.Null(propertyBuilder.HasHiLoSequence("Splod", null));
            Assert.Equal("Splow", propertyBuilder.Metadata.GetHiLoSequenceName());

            Assert.Equal(
                1, propertyBuilder.Metadata.GetAnnotations().Count(
                    a => a.Name.StartsWith(SqlServerAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [ConditionalFact]
        public void Throws_setting_sequence_generation_for_invalid_type()
        {
            var propertyBuilder = CreateBuilder()
                .Entity(typeof(Splot))
                .Property(typeof(string), "Name");

            Assert.Equal(
                SqlServerStrings.SequenceBadType("Name", nameof(Splot), "string"),
                Assert.Throws<ArgumentException>(
                    () => propertyBuilder.HasValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo)).Message);

            Assert.Equal(
                SqlServerStrings.SequenceBadType("Name", nameof(Splot), "string"),
                Assert.Throws<ArgumentException>(
                    () => new PropertyBuilder((IMutableProperty)propertyBuilder.Metadata).UseHiLo()).Message);
        }

        [ConditionalFact]
        public void Throws_setting_identity_generation_for_invalid_type_only_with_explicit()
        {
            var propertyBuilder = CreateBuilder()
                .Entity(typeof(Splot))
                .Property(typeof(string), "Name");

            Assert.Equal(
                SqlServerStrings.IdentityBadType("Name", nameof(Splot), "string"),
                Assert.Throws<ArgumentException>(
                    () => propertyBuilder.HasValueGenerationStrategy(SqlServerValueGenerationStrategy.IdentityColumn)).Message);

            Assert.Equal(
                SqlServerStrings.IdentityBadType("Name", nameof(Splot), "string"),
                Assert.Throws<ArgumentException>(
                    () => new PropertyBuilder((IMutableProperty)propertyBuilder.Metadata).UseIdentityColumn()).Message);
        }

        [ConditionalFact]
        public void Can_access_key()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot));
            var idProperty = entityTypeBuilder.Property(typeof(string), "Id").Metadata;
            var keyBuilder = entityTypeBuilder.HasKey(new[] { idProperty });

            Assert.NotNull(keyBuilder.IsClustered(true));
            Assert.True(keyBuilder.Metadata.IsClustered());

            Assert.NotNull(keyBuilder.IsClustered(false, fromDataAnnotation: true));
            Assert.False(keyBuilder.Metadata.IsClustered());

            Assert.Null(keyBuilder.IsClustered(true));
            Assert.False(keyBuilder.Metadata.IsClustered());

            Assert.Equal(
                1, keyBuilder.Metadata.GetAnnotations().Count(
                    a => a.Name.StartsWith(SqlServerAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_access_index(bool obsolete)
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot));
            var idProperty = entityTypeBuilder.Property(typeof(int), "Id").Metadata;
            var indexBuilder = entityTypeBuilder.HasIndex(new[] { idProperty });

            if (obsolete)
            {
#pragma warning disable 618
                Assert.NotNull(indexBuilder.ForSqlServerIsClustered(true));
#pragma warning restore 618
            }
            else
            {
                Assert.NotNull(indexBuilder.IsClustered(true));
            }
            Assert.True(indexBuilder.Metadata.IsClustered());

            if (obsolete)
            {
#pragma warning disable 618
                Assert.NotNull(indexBuilder.ForSqlServerIsClustered(false, fromDataAnnotation: true));
#pragma warning restore 618
            }
            else
            {
                Assert.NotNull(indexBuilder.IsClustered(false, fromDataAnnotation: true));
            }
            Assert.False(indexBuilder.Metadata.IsClustered());


            if (obsolete)
            {
#pragma warning disable 618
                Assert.Null(indexBuilder.ForSqlServerIsClustered(true));
#pragma warning restore 618
            }
            else
            {
                Assert.Null(indexBuilder.IsClustered(true));
            }
            Assert.False(indexBuilder.Metadata.IsClustered());

            Assert.Equal(
                1, indexBuilder.Metadata.GetAnnotations().Count(
                    a => a.Name.StartsWith(SqlServerAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [ConditionalFact]
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
