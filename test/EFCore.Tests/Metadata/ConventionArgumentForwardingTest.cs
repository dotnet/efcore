// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

public class ConventionArgumentForwardingTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Can_reapply_explicit_discriminator_completeness(bool complete)
    {
        var entityType = new ModelBuilder(new ConventionSet()).Entity("Entity").Metadata;
        var builder = new DiscriminatorBuilder(entityType);
        builder.IsComplete(complete);

        var conventionBuilder = (IConventionDiscriminatorBuilder)builder;
        Assert.True(conventionBuilder.CanSetIsComplete(complete));
        Assert.False(conventionBuilder.CanSetIsComplete(!complete));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Can_override_discriminator_completeness_from_data_annotation(bool complete)
    {
        var entityType = new ModelBuilder(new ConventionSet()).Entity("Entity").Metadata;
        ((IConventionEntityType)entityType).SetDiscriminatorMappingComplete(!complete, fromDataAnnotation: true);
        var builder = (IConventionDiscriminatorBuilder)new DiscriminatorBuilder(entityType);

        Assert.False(builder.CanSetIsComplete(complete));
        Assert.True(builder.CanSetIsComplete(complete, fromDataAnnotation: true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Single_property_foreign_key_preserves_configuration_source(bool fromDataAnnotation)
    {
        var model = (IConventionModel)new Model();
        var principal = model.AddEntityType("Principal")!;
        var key = principal.SetPrimaryKey(principal.AddProperty("Id", typeof(int))!)!;
        var dependent = model.AddEntityType("Dependent")!;
        var property = dependent.AddProperty("PrincipalId", typeof(int))!;

        var foreignKey = dependent.AddForeignKey(property, key, principal, fromDataAnnotation)!;

        var expected = fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention;
        Assert.Equal(expected, foreignKey.GetConfigurationSource());
        Assert.Equal(expected, foreignKey.GetPropertiesConfigurationSource());
        Assert.Equal(expected, foreignKey.GetPrincipalKeyConfigurationSource());
        Assert.Equal(expected, foreignKey.GetPrincipalEndConfigurationSource());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Complex_indexer_property_preserves_collection_and_configuration_source(bool collection, bool fromDataAnnotation)
    {
        var entityType = (IConventionTypeBase)new ModelBuilder(new ConventionSet())
            .SharedTypeEntity<Dictionary<string, object>>("Bag").Metadata;
        var propertyType = collection ? typeof(List<Detail>) : typeof(Detail);

        var property = entityType.AddComplexIndexerProperty(
            "Details", propertyType, typeof(Detail), "DetailType", collection, fromDataAnnotation)!;

        Assert.Equal(collection, property.IsCollection);
        Assert.Equal(
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention,
            property.GetConfigurationSource());
        Assert.True(property.IsIndexerProperty());
        Assert.Equal("Details", property.Name);
    }

    [Fact]
    public void Nested_complex_collection_preserves_property_and_type_names()
    {
        var builder = new ModelBuilder(new ConventionSet()).Entity<Owner>().ComplexCollection(e => e.Items);
        IMutableComplexProperty? nestedProperty = null;

        var returned = builder.ComplexCollection(
            typeof(List<Detail>), nameof(Item.Details), "NestedDetails", nested => nestedProperty = nested.Metadata);

        Assert.Same(builder, returned);
        Assert.NotNull(nestedProperty);
        Assert.Equal(nameof(Item.Details), nestedProperty.Name);
        Assert.Equal("NestedDetails", nestedProperty.ComplexType.Name);
        Assert.True(nestedProperty.IsCollection);
        Assert.False(nestedProperty.IsShadowProperty());
    }

    private class Owner
    {
        public List<Item> Items { get; set; } = [];
    }

    private class Item
    {
        public List<Detail> Details { get; set; } = [];
    }

    private class Detail
    {
        public int Value { get; set; }
    }
}
