// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable CollectionNeverUpdated.Local
namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

public class CosmosClientWrapperTest
{
    [Fact]
    public void GetJsonPropertyPathFromRoot_returns_simple_path_for_scalar_property()
    {
        var entityType = BuildModel().FindEntityType(typeof(Root))!;
        var property = entityType.FindProperty(nameof(Root.Name))!;

        Assert.Equal("/Name", CosmosClientWrapper.GetJsonPropertyPathFromRoot(property));
    }

    [Fact]
    public void GetJsonPropertyPathFromRoot_quote_escapes_segments_with_special_characters()
    {
        var entityType = BuildModel(eb => eb.Property(e => e.Name).ToJsonProperty("my-name"))
            .FindEntityType(typeof(Root))!;
        var property = entityType.FindProperty(nameof(Root.Name))!;

        Assert.Equal("/\"my-name\"", CosmosClientWrapper.GetJsonPropertyPathFromRoot(property));
    }

    [Fact]
    public void GetJsonPropertyPathFromRoot_returns_simple_path_for_complex_property_itself()
    {
        var entityType = BuildModel().FindEntityType(typeof(Root))!;
        var owner = entityType.FindComplexProperty(nameof(Root.Owner))!;

        Assert.Equal("/Owner", CosmosClientWrapper.GetJsonPropertyPathFromRoot(owner));
    }

    [Fact]
    public void GetJsonPropertyPathFromRoot_walks_complex_type_chain()
    {
        var entityType = BuildModel().FindEntityType(typeof(Root))!;
        var nameProperty = entityType.FindComplexProperty(nameof(Root.Owner))!.ComplexType.FindProperty(nameof(Sub.Name))!;

        Assert.Equal("/Owner/Name", CosmosClientWrapper.GetJsonPropertyPathFromRoot(nameProperty));
    }

    [Fact]
    public void GetJsonPropertyPathFromRoot_returns_collection_path_for_complex_collection_property_itself()
    {
        var entityType = BuildModel().FindEntityType(typeof(Root))!;
        var items = entityType.FindComplexProperty(nameof(Root.Items))!;

        Assert.Equal("/Items/[]", CosmosClientWrapper.GetJsonPropertyPathFromRoot(items));
    }

    [Fact]
    public void GetJsonPropertyPathFromRoot_walks_complex_collection_chain()
    {
        var entityType = BuildModel().FindEntityType(typeof(Root))!;
        var nameProperty = entityType.FindComplexProperty(nameof(Root.Items))!.ComplexType.FindProperty(nameof(Sub.Name))!;

        Assert.Equal("/Items/[]/Name", CosmosClientWrapper.GetJsonPropertyPathFromRoot(nameProperty));
    }

    [Fact]
    public void GetJsonPropertyPathFromRoot_escapes_quote_and_backslash_in_complex_property_chain()
    {
        var entityType = BuildModel(
                eb =>
                {
                    eb.ComplexProperty(e => e.Owner).Metadata.SetJsonPropertyName("with\"and\\backslash");
                    eb.ComplexProperty(e => e.Owner).Property(s => s.Name).ToJsonProperty("plain");
                })
            .FindEntityType(typeof(Root))!;
        var nameProperty = entityType.FindComplexProperty(nameof(Root.Owner))!.ComplexType.FindProperty(nameof(Sub.Name))!;

        Assert.Equal("/\"with\\\"and\\\\backslash\"/plain", CosmosClientWrapper.GetJsonPropertyPathFromRoot(nameProperty));
    }

    [Fact]
    public void GetJsonPropertyPathFromRoot_escapes_special_chars_in_complex_collection_segment()
    {
        var entityType = BuildModel(
                eb => eb.ComplexCollection(e => e.Items).Metadata.SetJsonPropertyName("items-list"))
            .FindEntityType(typeof(Root))!;
        var nameProperty = entityType.FindComplexProperty(nameof(Root.Items))!.ComplexType.FindProperty(nameof(Sub.Name))!;

        Assert.Equal("/\"items-list\"/[]/Name", CosmosClientWrapper.GetJsonPropertyPathFromRoot(nameProperty));
    }

    private static IReadOnlyModel BuildModel(Action<EntityTypeBuilder<Root>>? configure = null)
    {
        var modelBuilder = CosmosTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<Root>(eb =>
        {
            eb.HasPartitionKey(e => e.PartitionKey);
            eb.ComplexProperty(e => e.Owner);
            eb.ComplexCollection(e => e.Items);
            configure?.Invoke(eb);
        });
        return modelBuilder.FinalizeModel();
    }

    private class Root
    {
        public string Id { get; set; } = null!;
        public string PartitionKey { get; set; } = null!;
        public string Name { get; set; } = null!;
        public Sub Owner { get; set; } = null!;
        public List<Sub> Items { get; set; } = null!;
    }

    private class Sub
    {
        public string Name { get; set; } = null!;
    }
}
