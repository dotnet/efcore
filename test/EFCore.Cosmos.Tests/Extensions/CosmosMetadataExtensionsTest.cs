// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Cosmos;

public class CosmosMetadataExtensionsTest
{
    [ConditionalFact]
    public void Can_get_and_set_collection_name()
    {
        var modelBuilder = CreateModelBuilder();

        var entityType = modelBuilder
            .Entity<Customer>().Metadata;

        Assert.Equal(nameof(Customer), entityType.GetContainer());

        ((IConventionEntityType)entityType).SetContainer("Customizer");
        Assert.Equal("Customizer", entityType.GetContainer());
        Assert.Equal(ConfigurationSource.Convention, ((IConventionEntityType)entityType).GetContainerConfigurationSource());

        entityType.SetContainer("Customizer");
        Assert.Equal("Customizer", entityType.GetContainer());
        Assert.Equal(ConfigurationSource.Explicit, ((IConventionEntityType)entityType).GetContainerConfigurationSource());

        entityType.SetContainer(null);
        Assert.Equal(nameof(Customer), entityType.GetContainer());
        Assert.Null(((IConventionEntityType)entityType).GetContainerConfigurationSource());

        ((IConventionModel)modelBuilder.Model).Builder.HasDefaultContainer("Unicorn");
        Assert.Equal("Unicorn", entityType.GetContainer());
    }

    [ConditionalFact]
    public void Can_get_and_set_partition_key_name()
    {
        var modelBuilder = CreateModelBuilder();

        var entityType = modelBuilder
            .Entity<Customer>().Metadata;

        Assert.Empty(entityType.GetPartitionKeyPropertyNames());

        ((IConventionEntityType)entityType).SetPartitionKeyPropertyNames(["pk"]);
        Assert.Equal("pk", entityType.GetPartitionKeyPropertyNames().Single());
        Assert.Equal(
            ConfigurationSource.Convention, ((IConventionEntityType)entityType).GetPartitionKeyPropertyNamesConfigurationSource());

        entityType.SetPartitionKeyPropertyNames(["pk"]);
        Assert.Equal("pk", entityType.GetPartitionKeyPropertyNames().Single());
        Assert.Equal(
            ConfigurationSource.Explicit, ((IConventionEntityType)entityType).GetPartitionKeyPropertyNamesConfigurationSource());

        entityType.SetPartitionKeyPropertyNames(null);
        Assert.Empty(entityType.GetPartitionKeyPropertyNames());
        Assert.Null(((IConventionEntityType)entityType).GetPartitionKeyPropertyNamesConfigurationSource());
    }

    [ConditionalFact]
    public void Can_get_and_set_partition_key_name_obsolete()
    {
        var modelBuilder = CreateModelBuilder();

        var entityType = modelBuilder
            .Entity<Customer>().Metadata;

#pragma warning disable CS0618 // Type or member is obsolete
        Assert.Null(entityType.GetPartitionKeyPropertyName());

        ((IConventionEntityType)entityType).SetPartitionKeyPropertyName("pk");
        Assert.Equal("pk", entityType.GetPartitionKeyPropertyName());
        Assert.Equal(
            ConfigurationSource.Convention, ((IConventionEntityType)entityType).GetPartitionKeyPropertyNameConfigurationSource());

        entityType.SetPartitionKeyPropertyName("pk");
        Assert.Equal("pk", entityType.GetPartitionKeyPropertyName());
        Assert.Equal(
            ConfigurationSource.Explicit, ((IConventionEntityType)entityType).GetPartitionKeyPropertyNameConfigurationSource());

        entityType.SetPartitionKeyPropertyName(null);
        Assert.Null(entityType.GetPartitionKeyPropertyName());
        Assert.Null(((IConventionEntityType)entityType).GetPartitionKeyPropertyNameConfigurationSource());
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [ConditionalFact]
    public void Can_get_and_set_hierarchical_partition_key_name()
    {
        var modelBuilder = CreateModelBuilder();

        var entityType = modelBuilder
            .Entity<Customer>().Metadata;

        Assert.Empty(entityType.GetPartitionKeyPropertyNames());

        ((IConventionEntityType)entityType).SetPartitionKeyPropertyNames(["pk1", "pk2", "pk3"]);
        Assert.Equal(["pk1", "pk2", "pk3"], entityType.GetPartitionKeyPropertyNames());
        Assert.Equal(
            ConfigurationSource.Convention, ((IConventionEntityType)entityType).GetPartitionKeyPropertyNamesConfigurationSource());

        entityType.SetPartitionKeyPropertyNames(["pk1", "pk2", "pk3"]);
        Assert.Equal(["pk1", "pk2", "pk3"], entityType.GetPartitionKeyPropertyNames());
        Assert.Equal(
            ConfigurationSource.Explicit, ((IConventionEntityType)entityType).GetPartitionKeyPropertyNamesConfigurationSource());

        entityType.SetPartitionKeyPropertyNames(null);
        Assert.Empty(entityType.GetPartitionKeyPropertyNames());
        Assert.Null(((IConventionEntityType)entityType).GetPartitionKeyPropertyNamesConfigurationSource());
    }

    [ConditionalFact]
    public void Can_get_and_set_etag_name()
    {
        var modelBuilder = CreateModelBuilder();

        var entityType = modelBuilder
            .Entity<Customer>().Metadata;

        Assert.Null(entityType.GetETagPropertyName());

        ((IConventionEntityType)entityType).SetETagPropertyName("etag");
        Assert.Equal("etag", entityType.GetETagPropertyName());
        Assert.Equal(
            ConfigurationSource.Convention, ((IConventionEntityType)entityType).GetETagPropertyNameConfigurationSource());

        entityType.SetETagPropertyName("etag");
        Assert.Equal("etag", entityType.GetETagPropertyName());
        Assert.Equal(
            ConfigurationSource.Explicit, ((IConventionEntityType)entityType).GetETagPropertyNameConfigurationSource());

        entityType.SetETagPropertyName(null);
        Assert.Null(entityType.GetETagPropertyName());
        Assert.Null(((IConventionEntityType)entityType).GetETagPropertyNameConfigurationSource());
    }

    private static ModelBuilder CreateModelBuilder()
        => new();

    private class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid AlternateId { get; set; }
    }
}
