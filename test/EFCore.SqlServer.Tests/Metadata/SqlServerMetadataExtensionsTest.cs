// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata;

public class SqlServerMetadataExtensionsTest
{
    [ConditionalFact]
    public void Can_get_and_set_MaxSize()
    {
        var modelBuilder = GetModelBuilder();

        var model = modelBuilder
            .Model;

        Assert.Null(model.GetDatabaseMaxSize());
        Assert.Null(((IConventionModel)model).GetDatabaseMaxSizeConfigurationSource());

        ((IConventionModel)model).SetDatabaseMaxSize("1 GB", fromDataAnnotation: true);

        Assert.Equal("1 GB", model.GetDatabaseMaxSize());
        Assert.Equal(ConfigurationSource.DataAnnotation, ((IConventionModel)model).GetDatabaseMaxSizeConfigurationSource());

        model.SetDatabaseMaxSize("10 GB");

        Assert.Equal("10 GB", model.GetDatabaseMaxSize());
        Assert.Equal(ConfigurationSource.Explicit, ((IConventionModel)model).GetDatabaseMaxSizeConfigurationSource());

        model.SetDatabaseMaxSize(null);

        Assert.Null(model.GetDatabaseMaxSize());
        Assert.Null(((IConventionModel)model).GetDatabaseMaxSizeConfigurationSource());
    }

    [ConditionalFact]
    public void Can_get_and_set_ServiceTier()
    {
        var modelBuilder = GetModelBuilder();

        var model = modelBuilder
            .Model;

        Assert.Null(model.GetServiceTierSql());
        Assert.Null(((IConventionModel)model).GetDatabaseMaxSizeConfigurationSource());

        ((IConventionModel)model).SetServiceTierSql("basic", fromDataAnnotation: true);

        Assert.Equal("basic", model.GetServiceTierSql());
        Assert.Equal(ConfigurationSource.DataAnnotation, ((IConventionModel)model).GetServiceTierSqlConfigurationSource());

        model.SetServiceTierSql("standard");

        Assert.Equal("standard", model.GetServiceTierSql());
        Assert.Equal(ConfigurationSource.Explicit, ((IConventionModel)model).GetServiceTierSqlConfigurationSource());

        model.SetServiceTierSql(null);

        Assert.Null(model.GetServiceTierSql());
        Assert.Null(((IConventionModel)model).GetServiceTierSqlConfigurationSource());
    }

    [ConditionalFact]
    public void Can_get_and_set_PerformanceLevel()
    {
        var modelBuilder = GetModelBuilder();

        var model = modelBuilder
            .Model;

        Assert.Null(model.GetPerformanceLevelSql());
        Assert.Null(((IConventionModel)model).GetPerformanceLevelSqlConfigurationSource());

        ((IConventionModel)model).SetPerformanceLevelSql("S0", fromDataAnnotation: true);

        Assert.Equal("S0", model.GetPerformanceLevelSql());
        Assert.Equal(ConfigurationSource.DataAnnotation, ((IConventionModel)model).GetPerformanceLevelSqlConfigurationSource());

        model.SetPerformanceLevelSql("ELASTIC_POOL (name = elastic_pool)");

        Assert.Equal("ELASTIC_POOL (name = elastic_pool)", model.GetPerformanceLevelSql());
        Assert.Equal(ConfigurationSource.Explicit, ((IConventionModel)model).GetPerformanceLevelSqlConfigurationSource());

        model.SetPerformanceLevelSql(null);

        Assert.Null(model.GetPerformanceLevelSql());
        Assert.Null(((IConventionModel)model).GetPerformanceLevelSqlConfigurationSource());
    }

    [ConditionalFact]
    public void Can_get_and_set_column_name()
    {
        var modelBuilder = GetModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Name)
            .Metadata;

        Assert.Equal("Name", property.GetColumnName());
        Assert.Null(((IConventionProperty)property).GetColumnNameConfigurationSource());

        ((IConventionProperty)property).SetColumnName("Eman", fromDataAnnotation: true);

        Assert.Equal("Eman", property.GetColumnName());
        Assert.Equal(ConfigurationSource.DataAnnotation, ((IConventionProperty)property).GetColumnNameConfigurationSource());

        property.SetColumnName("MyNameIs");

        Assert.Equal("Name", property.Name);
        Assert.Equal("MyNameIs", property.GetColumnName());
        Assert.Equal(ConfigurationSource.Explicit, ((IConventionProperty)property).GetColumnNameConfigurationSource());

        property.SetColumnName(null);

        Assert.Equal("Name", property.GetColumnName());
        Assert.Null(((IConventionProperty)property).GetColumnNameConfigurationSource());
    }

    [ConditionalFact]
    public void Can_get_and_set_key_name()
    {
        var modelBuilder = GetModelBuilder();

        var key = modelBuilder
            .Entity<Customer>()
            .HasKey(e => e.Id)
            .Metadata;

        Assert.Equal("PK_Customer", key.GetName());

        key.SetName("PrimaryKey");

        Assert.Equal("PrimaryKey", key.GetName());

        key.SetName("PrimarySchool");

        Assert.Equal("PrimarySchool", key.GetName());

        key.SetName(null);

        Assert.Equal("PK_Customer", key.GetName());
    }

    [ConditionalFact]
    public void Can_get_and_set_index_clustering()
    {
        var modelBuilder = GetModelBuilder();

        var index = modelBuilder
            .Entity<Customer>()
            .HasIndex(e => e.Id)
            .Metadata;

        Assert.Null(index.IsClustered());

        index.SetIsClustered(true);

        Assert.True(index.IsClustered().Value);

        index.SetIsClustered(null);

        Assert.Null(index.IsClustered());
    }

    [ConditionalFact]
    public void Can_get_and_set_key_clustering()
    {
        var modelBuilder = GetModelBuilder();

        var key = modelBuilder
            .Entity<Customer>()
            .HasKey(e => e.Id)
            .Metadata;

        Assert.Null(key.IsClustered());

        key.SetIsClustered(true);

        Assert.True(key.IsClustered().Value);

        key.SetIsClustered(null);

        Assert.Null(key.IsClustered());
    }

    [ConditionalFact]
    public void Can_get_and_set_value_generation_on_model()
    {
        var modelBuilder = GetModelBuilder();
        var model = modelBuilder.Model;

        Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, model.GetValueGenerationStrategy());

        model.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);

        Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, model.GetValueGenerationStrategy());

        model.SetValueGenerationStrategy(null);

        Assert.Null(model.GetValueGenerationStrategy());
    }

    [ConditionalFact]
    public void Can_get_and_set_default_sequence_name_on_model()
    {
        var modelBuilder = GetModelBuilder();
        var model = modelBuilder.Model;

        model.SetHiLoSequenceName("Tasty.Snook");

        Assert.Equal("Tasty.Snook", model.GetHiLoSequenceName());

        model.SetHiLoSequenceName(null);

        Assert.Equal(SqlServerModelExtensions.DefaultHiLoSequenceName, model.GetHiLoSequenceName());
    }

    [ConditionalFact]
    public void Can_get_and_set_default_sequence_schema_on_model()
    {
        var modelBuilder = GetModelBuilder();
        var model = modelBuilder.Model;

        Assert.Null(model.GetHiLoSequenceSchema());

        model.SetHiLoSequenceSchema("Tasty.Snook");

        Assert.Equal("Tasty.Snook", model.GetHiLoSequenceSchema());

        model.SetHiLoSequenceSchema(null);

        Assert.Null(model.GetHiLoSequenceSchema());
    }

    [ConditionalFact]
    public void Can_get_and_set_value_generation_on_property()
    {
        var modelBuilder = GetModelBuilder();
        modelBuilder.Model.SetValueGenerationStrategy(null);

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Id)
            .Metadata;

        Assert.Equal(SqlServerValueGenerationStrategy.None, property.GetValueGenerationStrategy());
        Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);

        property.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);

        Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, property.GetValueGenerationStrategy());
        Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);

        property.SetValueGenerationStrategy(null);

        Assert.Equal(SqlServerValueGenerationStrategy.None, property.GetValueGenerationStrategy());
        Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
    }

    [ConditionalFact]
    public void Can_get_and_set_value_generation_on_nullable_property()
    {
        var modelBuilder = GetModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.NullableInt).ValueGeneratedOnAdd()
            .Metadata;

        Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, property.GetValueGenerationStrategy());

        property.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);

        Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, property.GetValueGenerationStrategy());

        property.SetValueGenerationStrategy(null);

        Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, property.GetValueGenerationStrategy());
    }

    [ConditionalFact]
    public void Throws_setting_sequence_generation_for_invalid_type()
    {
        var modelBuilder = GetModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Name)
            .Metadata;

        Assert.Equal(
            SqlServerStrings.SequenceBadType("Name", nameof(Customer), "string"),
            Assert.Throws<ArgumentException>(
                () => property.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo)).Message);
    }

    [ConditionalFact]
    public void Throws_setting_identity_generation_for_invalid_type()
    {
        var modelBuilder = GetModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Name)
            .Metadata;

        Assert.Equal(
            SqlServerStrings.IdentityBadType("Name", nameof(Customer), "string"),
            Assert.Throws<ArgumentException>(
                () => property.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.IdentityColumn)).Message);
    }

    [ConditionalFact]
    public void Can_get_and_set_sequence_name_on_property()
    {
        var modelBuilder = GetModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Id)
            .Metadata;

        Assert.Null(property.GetHiLoSequenceName());
        Assert.Null(property.GetHiLoSequenceName());

        property.SetHiLoSequenceName("Snook");

        Assert.Equal("Snook", property.GetHiLoSequenceName());

        property.SetHiLoSequenceName(null);

        Assert.Null(property.GetHiLoSequenceName());
    }

    [ConditionalFact]
    public void Can_get_and_set_sequence_schema_on_property()
    {
        var modelBuilder = GetModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Id)
            .Metadata;

        Assert.Null(property.GetHiLoSequenceSchema());

        property.SetHiLoSequenceSchema("Tasty");

        Assert.Equal("Tasty", property.GetHiLoSequenceSchema());

        property.SetHiLoSequenceSchema(null);

        Assert.Null(property.GetHiLoSequenceSchema());
    }

    [ConditionalFact]
    public void TryGetSequence_returns_sequence_property_is_marked_for_sequence_generation()
    {
        var modelBuilder = GetModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .Metadata;

        modelBuilder.Model.AddSequence("DaneelOlivaw");
        property.SetHiLoSequenceName("DaneelOlivaw");
        property.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);

        Assert.Equal("DaneelOlivaw", property.FindHiLoSequence().Name);
    }

    [ConditionalFact]
    public void TryGetSequence_returns_sequence_property_is_marked_for_default_generation_and_model_is_marked_for_sequence_generation()
    {
        var modelBuilder = GetModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .Metadata;

        modelBuilder.Model.AddSequence("DaneelOlivaw");
        modelBuilder.Model.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);
        property.SetHiLoSequenceName("DaneelOlivaw");

        Assert.Equal("DaneelOlivaw", property.FindHiLoSequence().Name);
    }

    [ConditionalFact]
    public void TryGetSequence_returns_sequence_property_is_marked_for_sequence_generation_and_model_has_name()
    {
        var modelBuilder = GetModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .Metadata;

        modelBuilder.Model.AddSequence("DaneelOlivaw");
        modelBuilder.Model.SetHiLoSequenceName("DaneelOlivaw");
        property.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);

        Assert.Equal("DaneelOlivaw", property.FindHiLoSequence().Name);
    }

    [ConditionalFact]
    public void
        TryGetSequence_returns_sequence_property_is_marked_for_default_generation_and_model_is_marked_for_sequence_generation_and_model_has_name()
    {
        var modelBuilder = GetModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .Metadata;

        modelBuilder.Model.AddSequence("DaneelOlivaw");
        modelBuilder.Model.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);
        modelBuilder.Model.SetHiLoSequenceName("DaneelOlivaw");

        Assert.Equal("DaneelOlivaw", property.FindHiLoSequence().Name);
    }

    [ConditionalFact]
    public void TryGetSequence_with_schema_returns_sequence_property_is_marked_for_sequence_generation()
    {
        var modelBuilder = GetModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .Metadata;

        modelBuilder.Model.AddSequence("DaneelOlivaw", "R");
        property.SetHiLoSequenceName("DaneelOlivaw");
        property.SetHiLoSequenceSchema("R");
        property.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);

        Assert.Equal("DaneelOlivaw", property.FindHiLoSequence().Name);
        Assert.Equal("R", property.FindHiLoSequence().Schema);
    }

    [ConditionalFact]
    public void TryGetSequence_with_schema_returns_sequence_model_is_marked_for_sequence_generation()
    {
        var modelBuilder = GetModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .Metadata;

        modelBuilder.Model.AddSequence("DaneelOlivaw", "R");
        modelBuilder.Model.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);
        property.SetHiLoSequenceName("DaneelOlivaw");
        property.SetHiLoSequenceSchema("R");

        Assert.Equal("DaneelOlivaw", property.FindHiLoSequence().Name);
        Assert.Equal("R", property.FindHiLoSequence().Schema);
    }

    [ConditionalFact]
    public void TryGetSequence_with_schema_returns_sequence_property_is_marked_for_sequence_generation_and_model_has_name()
    {
        var modelBuilder = GetModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .Metadata;

        modelBuilder.Model.AddSequence("DaneelOlivaw", "R");
        modelBuilder.Model.SetHiLoSequenceName("DaneelOlivaw");
        modelBuilder.Model.SetHiLoSequenceSchema("R");
        property.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);

        Assert.Equal("DaneelOlivaw", property.FindHiLoSequence().Name);
        Assert.Equal("R", property.FindHiLoSequence().Schema);
    }

    [ConditionalFact]
    public void TryGetSequence_with_schema_returns_sequence_model_is_marked_for_sequence_generation_and_model_has_name()
    {
        var modelBuilder = GetModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .Metadata;

        modelBuilder.Model.AddSequence("DaneelOlivaw", "R");
        modelBuilder.Model.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);
        modelBuilder.Model.SetHiLoSequenceName("DaneelOlivaw");
        modelBuilder.Model.SetHiLoSequenceSchema("R");

        Assert.Equal("DaneelOlivaw", property.FindHiLoSequence().Name);
        Assert.Equal("R", property.FindHiLoSequence().Schema);
    }

    private static ModelBuilder GetModelBuilder()
        => SqlServerTestHelpers.Instance.CreateConventionBuilder();

    // ReSharper disable once ClassNeverInstantiated.Local
    private class Customer
    {
        public int Id { get; set; }
        public int? NullableInt { get; set; }
        public string Name { get; set; }
        public byte Byte { get; set; }
        public byte? NullableByte { get; set; }
        public byte[] ByteArray { get; set; }
    }

    private class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
    }
}
