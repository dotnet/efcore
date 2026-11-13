// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable EF1001 // Internal EF Core API usage.
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

public class NullableCollectionNavigationTest
{
    [Fact]
    public void IsNullable_is_false_by_default_and_preserved_in_runtime_model()
    {
        var runtimeNavigation = GetRuntimeNavigation(nullable: null);

        Assert.False(runtimeNavigation.IsNullable);
    }

    [Fact]
    public void IsNullable_is_preserved_in_runtime_model()
    {
        var runtimeNavigation = GetRuntimeNavigation(nullable: true);

        Assert.True(runtimeNavigation.IsNullable);
        Assert.IsType<RuntimeNavigation>(runtimeNavigation);
    }

    [Theory, InlineData(true, 1), InlineData(false, 0)]
    public void NullableCollectionCount_counts_nullable_collection_navigations_including_inherited_ones(bool nullable, int expectedCount)
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<Parent>().HasMany(p => p.Children).WithOne();
        modelBuilder.Entity<DerivedParent>();

        modelBuilder.Model.FindEntityType(typeof(Parent))!.FindNavigation(nameof(Parent.Children))!.SetIsNullable(nullable);

        var model = modelBuilder.FinalizeModel();
        var runtimeModel = InMemoryTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IModel>();

        Assert.Equal(expectedCount, ((IRuntimeEntityType)runtimeModel.FindEntityType(typeof(Parent))!).NullableCollectionCount);
        Assert.Equal(expectedCount, ((IRuntimeEntityType)runtimeModel.FindEntityType(typeof(DerivedParent))!).NullableCollectionCount);
        Assert.Equal(0, ((IRuntimeEntityType)runtimeModel.FindEntityType(typeof(Child))!).NullableCollectionCount);
    }

    private static INavigation GetRuntimeNavigation(bool? nullable)
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<Parent>().HasMany(p => p.Children).WithOne();

        var navigation = modelBuilder.Model.FindEntityType(typeof(Parent))!.FindNavigation(nameof(Parent.Children))!;
        if (nullable.HasValue)
        {
            navigation.SetIsNullable(nullable);
        }

        var model = modelBuilder.FinalizeModel();
        var runtimeModel = InMemoryTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IModel>();

        return runtimeModel.FindEntityType(typeof(Parent))!.FindNavigation(nameof(Parent.Children))!;
    }

    private class Parent
    {
        public int Id { get; set; }
        public List<Child>? Children { get; set; }
    }

    private class DerivedParent : Parent
    {
    }

    private class Child
    {
        public int Id { get; set; }
    }
}
