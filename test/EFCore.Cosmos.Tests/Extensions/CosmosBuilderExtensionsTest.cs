// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Cosmos;

public class CosmosBuilderExtensionsTest
{
    [ConditionalFact]
    public void Can_get_and_set_collection_name()
    {
        var modelBuilder = CreateConventionModelBuilder();

        var entityType = modelBuilder
            .Entity<Customer>();

        Assert.Equal(nameof(DbContext), entityType.Metadata.GetContainer());

        entityType.ToContainer("Customizer");
        Assert.Equal("Customizer", entityType.Metadata.GetContainer());

        entityType.ToContainer(null);
        Assert.Equal(nameof(DbContext), entityType.Metadata.GetContainer());

        modelBuilder.HasDefaultContainer("Unicorn");
        Assert.Equal("Unicorn", entityType.Metadata.GetContainer());
    }

    [ConditionalFact]
    public void Can_get_and_set_partition_key_name()
    {
        var modelBuilder = CreateConventionModelBuilder();

        var entityTypeBuilder = modelBuilder.Entity<Customer>();
        var entityType = entityTypeBuilder.Metadata;

        ((IConventionEntityType)entityType).Builder.HasPartitionKey("pk");
        Assert.Equal("pk", entityType.GetPartitionKeyPropertyName());
        Assert.Equal(
            ConfigurationSource.Convention,
            ((IConventionEntityType)entityType).GetPartitionKeyPropertyNameConfigurationSource());

        entityTypeBuilder.HasPartitionKey("pk");
        Assert.Equal("pk", entityType.GetPartitionKeyPropertyName());
        Assert.Equal(
            ConfigurationSource.Explicit,
            ((IConventionEntityType)entityType).GetPartitionKeyPropertyNameConfigurationSource());

        Assert.False(((IConventionEntityType)entityType).Builder.CanSetPartitionKey("partition"));

        entityTypeBuilder.HasPartitionKey(null);
        Assert.Null(entityType.GetPartitionKeyPropertyName());
        Assert.Null(((IConventionEntityType)entityType).GetPartitionKeyPropertyNameConfigurationSource());
    }

    [ConditionalFact]
    public void Default_container_name_is_used_if_not_set()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>();

        var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
        modelBuilder.HasDefaultContainer(null);

        Assert.Equal(nameof(Customer), entityType.GetContainer());
        Assert.Null(modelBuilder.Model.GetDefaultContainer());

        modelBuilder.HasDefaultContainer("db0");

        Assert.Equal("db0", entityType.GetContainer());
        Assert.Equal("db0", modelBuilder.Model.GetDefaultContainer());

        modelBuilder
            .Entity<Customer>()
            .ToContainer("db1");

        Assert.Equal("db1", entityType.GetContainer());
    }

    [ConditionalFact]
    public void Default_discriminator_can_be_removed()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Customer>();

        var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

        Assert.Equal("Discriminator", entityType.FindDiscriminatorProperty().Name);
        Assert.Equal(nameof(Customer), entityType.GetDiscriminatorValue());

        modelBuilder.Entity<Customer>().HasNoDiscriminator();

        Assert.Null(entityType.FindDiscriminatorProperty());
        Assert.Null(entityType.GetDiscriminatorValue());

        modelBuilder.Entity<Customer>().HasBaseType<object>();

        Assert.Equal("Discriminator", entityType.FindDiscriminatorProperty().Name);
        Assert.Equal(nameof(Customer), entityType.GetDiscriminatorValue());

        modelBuilder.Entity<Customer>().HasBaseType((string)null);

        Assert.Null(entityType.FindDiscriminatorProperty());
    }

    [ConditionalFact]
    public void Can_set_etag_concurrency_entity()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().UseETagConcurrency();
        var model = modelBuilder.Model;

        var etagProperty = model.FindEntityType(typeof(Customer).FullName).FindProperty("_etag");
        Assert.NotNull(etagProperty);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, etagProperty.ValueGenerated);
        Assert.True(etagProperty.IsConcurrencyToken);
    }

    [ConditionalFact]
    public void Can_set_etag_concurrency_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().Property(x => x.ETag).IsETagConcurrency();
        var model = modelBuilder.Model;

        var etagProperty = model.FindEntityType(typeof(Customer).FullName).FindProperty("ETag");
        Assert.NotNull(etagProperty);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, etagProperty.ValueGenerated);
        Assert.True(etagProperty.IsConcurrencyToken);
        Assert.Equal("_etag", etagProperty.GetJsonPropertyName());
    }

    protected virtual ModelBuilder CreateConventionModelBuilder()
        => CosmosTestHelpers.Instance.CreateConventionBuilder();

    private class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public short SomeShort { get; set; }
        public string ETag { get; set; }
    }
}
