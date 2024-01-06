// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata;

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

    [ConditionalTheory]
    [InlineData(SqlServerValueGenerationStrategy.Sequence, SqlServerValueGenerationStrategy.IdentityColumn)]
    [InlineData(SqlServerValueGenerationStrategy.Sequence, SqlServerValueGenerationStrategy.SequenceHiLo)]
    [InlineData(SqlServerValueGenerationStrategy.IdentityColumn, SqlServerValueGenerationStrategy.Sequence)]
    [InlineData(SqlServerValueGenerationStrategy.IdentityColumn, SqlServerValueGenerationStrategy.SequenceHiLo)]
    [InlineData(SqlServerValueGenerationStrategy.SequenceHiLo, SqlServerValueGenerationStrategy.IdentityColumn)]
    [InlineData(SqlServerValueGenerationStrategy.SequenceHiLo, SqlServerValueGenerationStrategy.Sequence)]
    public void Can_change_value_generation_strategy(SqlServerValueGenerationStrategy from, SqlServerValueGenerationStrategy to)
    {
        var builder = CreateBuilder();

        Assert.Null(builder.Metadata.GetValueGenerationStrategy());
        Assert.Null(builder.Metadata.GetValueGenerationStrategyConfigurationSource());
        AssertFacets();

        Assert.NotNull(builder.HasValueGenerationStrategy(from));
        Assert.Equal(from, builder.Metadata.GetValueGenerationStrategy());
        AssertFacets();

        Assert.NotNull(builder.HasValueGenerationStrategy(to));
        Assert.Equal(to, builder.Metadata.GetValueGenerationStrategy());
        AssertFacets();

        void AssertFacets()
        {
            Assert.Equal(1, builder.Metadata.GetIdentitySeed());
            Assert.Equal(1, builder.Metadata.GetIdentityIncrement());
            Assert.Equal("Sequence", builder.Metadata.GetSequenceNameSuffix());
            Assert.Null(builder.Metadata.GetSequenceSchema());
            Assert.Equal("EntityFrameworkHiLoSequence", builder.Metadata.GetHiLoSequenceName());
            Assert.Null(builder.Metadata.GetHiLoSequenceSchema());
        }
    }

    [ConditionalTheory]
    [InlineData(SqlServerValueGenerationStrategy.Sequence)]
    [InlineData(SqlServerValueGenerationStrategy.SequenceHiLo)]
    public void Seed_and_increment_are_reset_when_changing_strategy(SqlServerValueGenerationStrategy to)
    {
        var builder = CreateBuilder();

        Assert.Null(builder.Metadata.GetValueGenerationStrategy());
        Assert.Null(builder.Metadata.GetValueGenerationStrategyConfigurationSource());
        AssertFacets(1, 1);

        builder.HasValueGenerationStrategy(SqlServerValueGenerationStrategy.IdentityColumn);
        Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, builder.Metadata.GetValueGenerationStrategy());
        builder.HasIdentityColumnSeed(77);
        builder.HasIdentityColumnIncrement(7);
        AssertFacets(77, 7);

        Assert.NotNull(builder.HasValueGenerationStrategy(to));
        Assert.Equal(to, builder.Metadata.GetValueGenerationStrategy());
        AssertFacets(1, 1);

        builder.HasValueGenerationStrategy(SqlServerValueGenerationStrategy.IdentityColumn);
        Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, builder.Metadata.GetValueGenerationStrategy());
        AssertFacets(1, 1);

        void AssertFacets(long seed, int increment)
        {
            Assert.Equal(seed, builder.Metadata.GetIdentitySeed());
            Assert.Equal(increment, builder.Metadata.GetIdentityIncrement());
            Assert.Equal("Sequence", builder.Metadata.GetSequenceNameSuffix());
            Assert.Null(builder.Metadata.GetSequenceSchema());
            Assert.Equal("EntityFrameworkHiLoSequence", builder.Metadata.GetHiLoSequenceName());
            Assert.Null(builder.Metadata.GetHiLoSequenceSchema());
        }
    }

    [ConditionalTheory]
    [InlineData(SqlServerValueGenerationStrategy.IdentityColumn)]
    [InlineData(SqlServerValueGenerationStrategy.SequenceHiLo)]
    public void Sequence_and_schema_are_reset_when_changing_strategy(SqlServerValueGenerationStrategy to)
    {
        var builder = CreateBuilder();

        Assert.Null(builder.Metadata.GetValueGenerationStrategy());
        Assert.Null(builder.Metadata.GetValueGenerationStrategyConfigurationSource());
        AssertFacets("Sequence", null);

        builder.HasValueGenerationStrategy(SqlServerValueGenerationStrategy.Sequence);
        Assert.Equal(SqlServerValueGenerationStrategy.Sequence, builder.Metadata.GetValueGenerationStrategy());
        builder.Metadata.SetSequenceNameSuffix("MySequence");
        builder.Metadata.SetSequenceSchema("MySchema");
        AssertFacets("MySequence", "MySchema");

        Assert.NotNull(builder.HasValueGenerationStrategy(to));
        Assert.Equal(to, builder.Metadata.GetValueGenerationStrategy());
        AssertFacets("Sequence", null);

        builder.HasValueGenerationStrategy(SqlServerValueGenerationStrategy.Sequence);
        Assert.Equal(SqlServerValueGenerationStrategy.Sequence, builder.Metadata.GetValueGenerationStrategy());
        AssertFacets("Sequence", null);

        void AssertFacets(string sequenceSuffix, string schema)
        {
            Assert.Equal(1, builder.Metadata.GetIdentitySeed());
            Assert.Equal(1, builder.Metadata.GetIdentityIncrement());
            Assert.Equal(sequenceSuffix, builder.Metadata.GetSequenceNameSuffix());
            Assert.Equal(schema, builder.Metadata.GetSequenceSchema());
            Assert.Equal("EntityFrameworkHiLoSequence", builder.Metadata.GetHiLoSequenceName());
            Assert.Null(builder.Metadata.GetHiLoSequenceSchema());
        }
    }

    [ConditionalTheory]
    [InlineData(SqlServerValueGenerationStrategy.IdentityColumn)]
    [InlineData(SqlServerValueGenerationStrategy.Sequence)]
    public void HiLo_sequence_and_schema_are_reset_when_changing_strategy(SqlServerValueGenerationStrategy to)
    {
        var builder = CreateBuilder();

        Assert.Null(builder.Metadata.GetValueGenerationStrategy());
        Assert.Null(builder.Metadata.GetValueGenerationStrategyConfigurationSource());
        AssertFacets("EntityFrameworkHiLoSequence", null);

        builder.HasValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);
        Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, builder.Metadata.GetValueGenerationStrategy());
        builder.HasHiLoSequence("MyHiLoSequence", "MyHiLoSchema");
        AssertFacets("MyHiLoSequence", "MyHiLoSchema");

        Assert.NotNull(builder.HasValueGenerationStrategy(to));
        Assert.Equal(to, builder.Metadata.GetValueGenerationStrategy());
        AssertFacets("EntityFrameworkHiLoSequence", null);

        builder.HasValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);
        Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, builder.Metadata.GetValueGenerationStrategy());
        AssertFacets("EntityFrameworkHiLoSequence", null);

        void AssertFacets(string sequence, string schema)
        {
            Assert.Equal(1, builder.Metadata.GetIdentitySeed());
            Assert.Equal(1, builder.Metadata.GetIdentityIncrement());
            Assert.Equal("Sequence", builder.Metadata.GetSequenceNameSuffix());
            Assert.Null(builder.Metadata.GetSequenceSchema());
            Assert.Equal(sequence, builder.Metadata.GetHiLoSequenceName());
            Assert.Equal(schema, builder.Metadata.GetHiLoSequenceSchema());
        }
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
    public void Throws_setting_key_sequence_generation_for_invalid_type()
    {
        var propertyBuilder = CreateBuilder()
            .Entity(typeof(Splot))
            .Property(typeof(string), "Name");

        Assert.Equal(
            SqlServerStrings.SequenceBadType("Name", nameof(Splot), "string"),
            Assert.Throws<ArgumentException>(
                () => propertyBuilder.HasValueGenerationStrategy(SqlServerValueGenerationStrategy.Sequence)).Message);

        Assert.Equal(
            SqlServerStrings.SequenceBadType("Name", nameof(Splot), "string"),
            Assert.Throws<ArgumentException>(
                () => new PropertyBuilder((IMutableProperty)propertyBuilder.Metadata).UseSequence()).Message);
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
        Assert.Equal(ConfigurationSource.DataAnnotation, indexBuilder.Metadata.GetIsClusteredConfigurationSource());
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

    private class Splot;
}
