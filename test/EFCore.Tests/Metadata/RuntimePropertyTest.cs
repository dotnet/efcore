// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata;

public class RuntimePropertyTest
{
    [Fact]
    public void GetKeyValueComparer_with_default_factory_does_not_create_type_mapping_key_comparer()
    {
        var typeMapping = new InMemoryTypeMapping(typeof(Guid));
        var property = CreateProperty("Id", typeof(Guid), typeMapping);

        Assert.Null(typeMapping.FindKeyComparer());

        var comparer = property.GetKeyValueComparer(
            static () => ValueComparer.CreateDefault<Guid>(favorStructuralComparisons: true));

        // The supplied generic factory is used instead of the reflection-based default on the type mapping.
        Assert.Null(typeMapping.FindKeyComparer());

        var comparer1 = Assert.IsAssignableFrom<ValueComparer<Guid>>(comparer);
        var value = Guid.NewGuid();
        Assert.True(comparer1.Equals(value, value));
        Assert.False(comparer1.Equals(value, Guid.NewGuid()));
    }

    [Fact]
    public void GetKeyValueComparer_with_default_factory_uses_comparer_from_the_model()
    {
        var keyComparer = new ValueComparer<Guid>(favorStructuralComparisons: false);
        var typeMapping = new InMemoryTypeMapping(typeof(Guid), keyComparer: keyComparer);
        var property = CreateProperty("Id", typeof(Guid), typeMapping);

        var comparer = property.GetKeyValueComparer(
            static () => throw new InvalidOperationException("The default factory should not be called."));

        Assert.Same(keyComparer, comparer);
    }

    [Fact]
    public void GetKeyValueComparer_without_factory_creates_type_mapping_key_comparer()
    {
        var typeMapping = new InMemoryTypeMapping(typeof(Guid));
        var property = CreateProperty("Id", typeof(Guid), typeMapping);

        Assert.Null(typeMapping.FindKeyComparer());

        var comparer = property.GetKeyValueComparer();

        Assert.NotNull(comparer);
        Assert.NotNull(typeMapping.FindKeyComparer());
    }

    private static RuntimeProperty CreateProperty(string name, Type clrType, CoreTypeMapping typeMapping)
    {
        var model = new RuntimeModel(skipDetectChanges: false, modelId: Guid.NewGuid(), entityTypeCount: 1);
        var entityType = model.AddEntityType("E", typeof(object), propertyCount: 1);
        return entityType.AddProperty(name, clrType, typeMapping: typeMapping);
    }
}
