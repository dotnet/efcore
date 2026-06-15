// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata;

public class CosmosEntityTypeExtensionsTest
{
    [Fact]
    public virtual void Except_throws_when_automatic_indexing_disabled()
    {
        var modelBuilder = CreateConventionModelBuilder();
        Assert.Equal(
            CosmosStrings.AutomaticIndexingExceptionWhileDisabled,
            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Entity<Customer>(b => b.HasAutomaticIndexing(enabled: false).Except("/X/?"))).Message);
    }

    [Fact]
    public virtual void Disabling_after_excepting_disables_exceptions()
    {
        // Final emission for HasAutomaticIndexing(true).Except(...).HasAutomaticIndexing(false): /* is not emitted,
        // exceptions are retained in the model (no longer relevant since automatic indexing is off) and not emitted.
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>(b =>
        {
            b.HasAutomaticIndexing().Except("/Notes/?");
            b.HasAutomaticIndexing(false);
        });

        var entityType = modelBuilder.Model.FindEntityType(typeof(Customer))!;
        Assert.False(entityType.GetAutomaticIndexingEnabled());
        Assert.Null(entityType.GetAutomaticIndexingExceptions());
    }

    [Fact]
    public virtual void Except_does_not_dedupe()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>(b =>
        {
            b.HasAutomaticIndexing()
                .Except("/Notes/?")
                .Except("/Notes/?");
        });

        var entityType = modelBuilder.Model.FindEntityType(typeof(Customer))!;
        Assert.Equal(["/Notes/?", "/Notes/?"], entityType.GetAutomaticIndexingExceptions()!);
    }

    [Fact]
    public virtual void Get_automatic_indexing_reads_to_root_for_derived_type()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>(b => b.HasAutomaticIndexing(false));
        modelBuilder.Entity<SpecialCustomer>(b => b.HasBaseType<Customer>());

        var derived = modelBuilder.Model.FindEntityType(typeof(SpecialCustomer))!;
        Assert.False(derived.GetAutomaticIndexingEnabled());
    }

    [Fact]
    public virtual void Convention_api_HasAutomaticIndexing_sets_annotation()
    {
        var modelBuilder = CreateConventionModelBuilder().GetInfrastructure();
        var entity = modelBuilder.Entity(typeof(Customer))!;

        Assert.NotNull(entity.HasAutomaticIndexing(true));
        Assert.True(entity.Metadata.GetAutomaticIndexingEnabled());

        Assert.NotNull(entity.HasAutomaticIndexing(false));
        Assert.False(entity.Metadata.GetAutomaticIndexingEnabled());

        Assert.NotNull(entity.HasAutomaticIndexing(null));
        Assert.Null(entity.Metadata.GetAutomaticIndexingEnabled());
    }

    [Fact]
    public virtual void Convention_api_CanSet_succeeds()
    {
        var modelBuilder = CreateConventionModelBuilder().GetInfrastructure();
        var entity = modelBuilder.Entity(typeof(Customer))!;

        Assert.True(entity.CanSetAutomaticIndexing(true));
        Assert.True(entity.CanSetAutomaticIndexingExceptions(["/X/?"]));
    }

    [Fact]
    public virtual void Convention_api_sets_automatic_indexing_and_reads_configuration_source()
    {
        var modelBuilder = CreateConventionModelBuilder().GetInfrastructure();
        var entityType = modelBuilder.Entity(typeof(Customer))!.Metadata;

        Assert.Null(entityType.GetAutomaticIndexingEnabledConfigurationSource());
        Assert.Null(entityType.GetAutomaticIndexingExceptionsConfigurationSource());

        Assert.Equal(true, entityType.SetAutomaticIndexingEnabled(true));
        Assert.Equal(ConfigurationSource.Convention, entityType.GetAutomaticIndexingEnabledConfigurationSource());

        Assert.Equal(["/X/?"], entityType.SetAutomaticIndexingExceptions(["/X/?"]));
        Assert.Equal(["/X/?"], entityType.GetAutomaticIndexingExceptions());
        Assert.Equal(ConfigurationSource.Convention, entityType.GetAutomaticIndexingExceptionsConfigurationSource());

        // Setting from a data annotation overrides the configuration source.
        Assert.Equal(true, entityType.SetAutomaticIndexingEnabled(true, fromDataAnnotation: true));
        Assert.Equal(ConfigurationSource.DataAnnotation, entityType.GetAutomaticIndexingEnabledConfigurationSource());

        // Removing the settings clears the configuration source.
        Assert.Null(entityType.SetAutomaticIndexingExceptions(null, fromDataAnnotation: true));
        Assert.Null(entityType.GetAutomaticIndexingExceptionsConfigurationSource());
    }

    [Fact]
    public virtual void Convention_api_sets_full_text_index_on_convention_index()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().HasIndex(e => e.Name);

        var index = (IConventionIndex)modelBuilder.Model.FindEntityType(typeof(Customer))!.GetIndexes().Single();

        Assert.Null(index.GetIsFullTextIndexConfigurationSource());

        Assert.Equal(true, index.SetIsFullTextIndex(true));
        Assert.True(index.IsFullTextIndex());
        Assert.Equal(ConfigurationSource.Convention, index.GetIsFullTextIndexConfigurationSource());

        Assert.Null(index.SetIsFullTextIndex(null, fromDataAnnotation: true));
        Assert.Null(index.IsFullTextIndex());
        Assert.Null(index.GetIsFullTextIndexConfigurationSource());
    }

    [Fact]
    public virtual void Convention_api_CanSetEnableFullTextSearch_for_complex_collection_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<EntityWithStringsInComplexCollection>(b =>
            b.ComplexCollection(e => e.Items, cb => cb.Property(d => d.Description)));

        var entityType = modelBuilder.Model.FindEntityType(typeof(EntityWithStringsInComplexCollection))!;
        var complexType = entityType.FindComplexProperty(nameof(EntityWithStringsInComplexCollection.Items))!.ComplexType;
        var property = (IConventionProperty)complexType.FindProperty(nameof(DescriptionItem.Description))!;

        Assert.True(property.Builder.CanSetEnableFullTextSearch(language: null, enabled: true));
        Assert.NotNull(property.Builder.EnableFullTextSearch(language: null, enabled: true));
    }

    [Fact]
    public virtual void Convention_api_CanSetIsVectorProperty_for_complex_collection_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<EntityWithVectorsInComplexCollection>(b =>
            b.ComplexCollection(e => e.Items, cb => cb.Property(d => d.Embedding)));

        var entityType = modelBuilder.Model.FindEntityType(typeof(EntityWithVectorsInComplexCollection))!;
        var complexType = entityType.FindComplexProperty(nameof(EntityWithVectorsInComplexCollection.Items))!.ComplexType;
        var property = (IConventionProperty)complexType.FindProperty(nameof(EmbeddingDetails.Embedding))!;

        Assert.True(property.Builder.CanSetIsVectorProperty(DistanceFunction.Cosine, dimensions: 8));
        Assert.NotNull(property.Builder.IsVectorProperty(DistanceFunction.Cosine, dimensions: 8));
    }

    private static ModelBuilder CreateConventionModelBuilder()
        => CosmosTestHelpers.Instance.CreateConventionBuilder();

    private class Customer
    {
        public int Id { get; set; }
        public string PartitionId { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    private class SpecialCustomer : Customer;

    private class EntityWithVectorsInComplexCollection
    {
        public string Id { get; set; } = null!;
        public List<EmbeddingDetails> Items { get; set; } = null!;
    }

    private class EmbeddingDetails
    {
        public ReadOnlyMemory<float> Embedding { get; set; }
    }

    private class EntityWithStringsInComplexCollection
    {
        public string Id { get; set; } = null!;
        public List<DescriptionItem> Items { get; set; } = null!;
    }

    private class DescriptionItem
    {
        public string Description { get; set; } = null!;
    }
}
