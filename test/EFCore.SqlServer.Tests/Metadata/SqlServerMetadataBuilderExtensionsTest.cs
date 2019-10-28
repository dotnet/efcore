// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class SqlServerMetadataBuilderExtensionsTest
    {
        private IConventionModelBuilder CreateBuilder()
            => new InternalModelBuilder(new Model());

        [ConditionalFact]
        public void Can_access_model_value_generation_strategy()
        {
            var builder = CreateBuilder();

            Assert.Null(builder.Metadata.GetValueGenerationStrategy());
            Assert.Null(builder.Metadata.GetValueGenerationStrategyConfigurationSource());

            Assert.NotNull(
                builder.HasValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo));
            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, builder.Metadata.GetValueGenerationStrategy());

            Assert.NotNull(
                builder.HasValueGenerationStrategy(SqlServerValueGenerationStrategy.IdentityColumn, fromDataAnnotation: true));
            Assert.Equal(
                SqlServerValueGenerationStrategy.IdentityColumn, builder.Metadata.GetValueGenerationStrategy());
            Assert.Equal(ConfigurationSource.DataAnnotation, builder.Metadata.GetValueGenerationStrategyConfigurationSource());
            Assert.NotNull(builder.HasValueGenerationStrategy(SqlServerValueGenerationStrategy.IdentityColumn));

            Assert.Null(
                builder.HasValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo));
            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, builder.Metadata.GetValueGenerationStrategy());

            Assert.NotNull(builder.HasValueGenerationStrategy(null, fromDataAnnotation: true));
            Assert.Null(builder.Metadata.GetValueGenerationStrategy());
            Assert.Null(builder.Metadata.GetValueGenerationStrategyConfigurationSource());
        }

        [ConditionalFact]
        public void Can_access_model_max_size()
        {
            var builder = CreateBuilder();

            Assert.Null(builder.Metadata.GetDatabaseMaxSize());
            Assert.Null(builder.Metadata.GetDatabaseMaxSizeConfigurationSource());

            Assert.NotNull(builder.HasDatabaseMaxSize("50 GB"));
            Assert.Equal("50 GB", builder.Metadata.GetDatabaseMaxSize());

            Assert.NotNull(
                builder.HasDatabaseMaxSize("100 GB", fromDataAnnotation: true));
            Assert.Equal("100 GB", builder.Metadata.GetDatabaseMaxSize());
            Assert.Equal(ConfigurationSource.DataAnnotation, builder.Metadata.GetDatabaseMaxSizeConfigurationSource());
            Assert.NotNull(builder.HasDatabaseMaxSize("100 GB"));

            Assert.Null(builder.HasDatabaseMaxSize("500 GB"));
            Assert.Equal("100 GB", builder.Metadata.GetDatabaseMaxSize());

            Assert.NotNull(builder.HasDatabaseMaxSize(null, fromDataAnnotation: true));
            Assert.Null(builder.Metadata.GetDatabaseMaxSize());
            Assert.Null(builder.Metadata.GetDatabaseMaxSizeConfigurationSource());
        }

        [ConditionalFact]
        public void Can_access_model_service_tier()
        {
            var builder = CreateBuilder();

            Assert.Null(builder.Metadata.GetServiceTierSql());
            Assert.Null(builder.Metadata.GetServiceTierSqlConfigurationSource());

            Assert.NotNull(builder.HasServiceTierSql("premium"));
            Assert.Equal("premium", builder.Metadata.GetServiceTierSql());

            Assert.NotNull(
                builder.HasServiceTierSql("basic", fromDataAnnotation: true));
            Assert.Equal("basic", builder.Metadata.GetServiceTierSql());
            Assert.Equal(ConfigurationSource.DataAnnotation, builder.Metadata.GetServiceTierSqlConfigurationSource());
            Assert.NotNull(builder.HasServiceTierSql("basic"));

            Assert.Null(builder.HasServiceTierSql("premium"));
            Assert.Equal("basic", builder.Metadata.GetServiceTierSql());

            Assert.NotNull(builder.HasServiceTierSql(null, fromDataAnnotation: true));
            Assert.Null(builder.Metadata.GetServiceTierSql());
            Assert.Null(builder.Metadata.GetServiceTierSqlConfigurationSource());
        }

        [ConditionalFact]
        public void Can_access_model_performance_level()
        {
            var builder = CreateBuilder();

            Assert.Null(builder.Metadata.GetPerformanceLevelSql());
            Assert.Null(builder.Metadata.GetPerformanceLevelSqlConfigurationSource());

            Assert.NotNull(builder.HasPerformanceLevelSql("P1"));
            Assert.Equal("P1", builder.Metadata.GetPerformanceLevelSql());

            Assert.NotNull(
                builder.HasPerformanceLevelSql("P4", fromDataAnnotation: true));
            Assert.Equal("P4", builder.Metadata.GetPerformanceLevelSql());
            Assert.Equal(ConfigurationSource.DataAnnotation, builder.Metadata.GetPerformanceLevelSqlConfigurationSource());
            Assert.NotNull(builder.HasPerformanceLevelSql("P4"));

            Assert.Null(builder.HasPerformanceLevelSql("P1"));
            Assert.Equal("P4", builder.Metadata.GetPerformanceLevelSql());

            Assert.NotNull(builder.HasPerformanceLevelSql(null, fromDataAnnotation: true));
            Assert.Null(builder.Metadata.GetPerformanceLevelSql());
            Assert.Null(builder.Metadata.GetPerformanceLevelSqlConfigurationSource());
        }

        [ConditionalFact]
        public void Can_access_entity_type()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot));

            Assert.Null(typeBuilder.Metadata.GetIsMemoryOptimizedConfigurationSource());

            Assert.NotNull(typeBuilder.IsMemoryOptimized(true));
            Assert.True(typeBuilder.Metadata.IsMemoryOptimized());
            Assert.Equal(ConfigurationSource.Convention, typeBuilder.Metadata.GetIsMemoryOptimizedConfigurationSource());

            Assert.NotNull(typeBuilder.IsMemoryOptimized(false, fromDataAnnotation: true));
            Assert.False(typeBuilder.Metadata.IsMemoryOptimized());
            Assert.Equal(ConfigurationSource.DataAnnotation, typeBuilder.Metadata.GetIsMemoryOptimizedConfigurationSource());

            Assert.Null(typeBuilder.IsMemoryOptimized(true));
            Assert.False(typeBuilder.Metadata.IsMemoryOptimized());
            Assert.NotNull(typeBuilder.IsMemoryOptimized(false));

            Assert.NotNull(typeBuilder.IsMemoryOptimized(null, fromDataAnnotation: true));
            Assert.False(typeBuilder.Metadata.IsMemoryOptimized());
            Assert.Null(typeBuilder.Metadata.GetIsMemoryOptimizedConfigurationSource());
        }

        [ConditionalFact]
        public void Can_access_property()
        {
            var propertyBuilder = CreateBuilder()
                .Entity(typeof(Splot))
                .Property(typeof(int), "Id");

            Assert.Null(propertyBuilder.Metadata.GetHiLoSequenceNameConfigurationSource());

            Assert.NotNull(propertyBuilder.HasHiLoSequence("Splew", null));
            Assert.Equal("Splew", propertyBuilder.Metadata.GetHiLoSequenceName());
            Assert.Equal(ConfigurationSource.Convention, propertyBuilder.Metadata.GetHiLoSequenceNameConfigurationSource());

            Assert.NotNull(propertyBuilder.HasHiLoSequence("Splow", null, fromDataAnnotation: true));
            Assert.Equal("Splow", propertyBuilder.Metadata.GetHiLoSequenceName());
            Assert.Equal(ConfigurationSource.DataAnnotation, propertyBuilder.Metadata.GetHiLoSequenceNameConfigurationSource());
            Assert.NotNull(propertyBuilder.HasHiLoSequence("Splow", null));

            Assert.Null(propertyBuilder.HasHiLoSequence("Splod", null));
            Assert.Equal("Splow", propertyBuilder.Metadata.GetHiLoSequenceName());

            Assert.Null(propertyBuilder.HasHiLoSequence(null, null, fromDataAnnotation: true));
            Assert.Null(propertyBuilder.Metadata.GetHiLoSequenceNameConfigurationSource());
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

            Assert.Null(keyBuilder.Metadata.GetIsClusteredConfigurationSource());

            Assert.NotNull(keyBuilder.IsClustered(true));
            Assert.True(keyBuilder.Metadata.IsClustered());
            Assert.Equal(ConfigurationSource.Convention, keyBuilder.Metadata.GetIsClusteredConfigurationSource());

            Assert.NotNull(keyBuilder.IsClustered(false, fromDataAnnotation: true));
            Assert.False(keyBuilder.Metadata.IsClustered());
            Assert.Equal(ConfigurationSource.DataAnnotation, keyBuilder.Metadata.GetIsClusteredConfigurationSource());
            Assert.NotNull(keyBuilder.IsClustered(false));

            Assert.Null(keyBuilder.IsClustered(true));
            Assert.False(keyBuilder.Metadata.IsClustered());

            Assert.NotNull(keyBuilder.IsClustered(null, fromDataAnnotation: true));
            Assert.Null(keyBuilder.Metadata.GetIsClusteredConfigurationSource());
        }

        [ConditionalFact]
        public void Can_access_index()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot));
            var idProperty = entityTypeBuilder.Property(typeof(int), "Id").Metadata;
            var indexBuilder = entityTypeBuilder.HasIndex(new[] { idProperty });

            Assert.Null(indexBuilder.Metadata.GetIsClusteredConfigurationSource());

            Assert.NotNull(indexBuilder.IsClustered(true));
            Assert.True(indexBuilder.Metadata.IsClustered());
            Assert.Equal(ConfigurationSource.Convention, indexBuilder.Metadata.GetIsClusteredConfigurationSource());

            Assert.NotNull(indexBuilder.IsClustered(false, fromDataAnnotation: true));
            Assert.False(indexBuilder.Metadata.IsClustered());
            Assert.Equal(ConfigurationSource.DataAnnotation, indexBuilder.Metadata.GetIsClusteredConfigurationSource());
            Assert.NotNull(indexBuilder.IsClustered(false));

            Assert.Null(indexBuilder.IsClustered(true));
            Assert.False(indexBuilder.Metadata.IsClustered());

            Assert.NotNull(indexBuilder.IsClustered(null, fromDataAnnotation: true));
            Assert.Null(indexBuilder.Metadata.GetIsClusteredConfigurationSource());
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
        }

        private class Splot
        {
        }
    }
}
