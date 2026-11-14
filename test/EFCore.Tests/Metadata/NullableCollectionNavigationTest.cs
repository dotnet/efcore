// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable EF1001 // Internal EF Core API usage.
using System.Reflection;
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

        // The in-memory provider rejects nullable collection navigations, but the model metadata is what is tested here.
        var model = modelBuilder.FinalizeModel(designTime: false, skipValidation: true);
        var runtimeModel = InMemoryTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IModel>();

        Assert.Equal(expectedCount, ((IRuntimeEntityType)runtimeModel.FindEntityType(typeof(Parent))!).NullableCollectionCount);
        Assert.Equal(expectedCount, ((IRuntimeEntityType)runtimeModel.FindEntityType(typeof(DerivedParent))!).NullableCollectionCount);
        Assert.Equal(0, ((IRuntimeEntityType)runtimeModel.FindEntityType(typeof(Child))!).NullableCollectionCount);
    }

    [Fact]
    public void AddNavigation_without_nullable_dispatches_to_overridden_original_virtual()
    {
        var (entityType, foreignKey) = CreateRuntimeForeignKey();

        var navigation = entityType.AddNavigation(nameof(Parent.Children), foreignKey, onDependent: false, typeof(List<Child>));

        Assert.Equal([nameof(Parent.Children)], entityType.OriginalAddNavigationCalls);
        Assert.False(((IReadOnlyNavigation)navigation).IsNullable);
    }

    [Fact]
    public void AddNavigation_with_nullable_false_dispatches_to_overridden_original_virtual()
    {
        var (entityType, foreignKey) = CreateRuntimeForeignKey();

        var navigation = entityType.AddNavigation(
            nameof(Parent.Children), foreignKey, onDependent: false, typeof(List<Child>), nullable: false);

        Assert.Equal([nameof(Parent.Children)], entityType.OriginalAddNavigationCalls);
        Assert.False(((IReadOnlyNavigation)navigation).IsNullable);
    }

    [Fact]
    public void AddNavigation_with_nullable_true_creates_a_nullable_navigation()
    {
        var (entityType, foreignKey) = CreateRuntimeForeignKey();

        var navigation = entityType.AddNavigation(
            nameof(Parent.Children), foreignKey, onDependent: false, typeof(List<Child>), nullable: true);

        Assert.True(((IReadOnlyNavigation)navigation).IsNullable);
    }

    private static (OriginalVirtualOverridingEntityType, RuntimeForeignKey) CreateRuntimeForeignKey()
    {
        var model = new RuntimeModel(skipDetectChanges: false, modelId: Guid.Empty, entityTypeCount: 2);

        var parentType = new OriginalVirtualOverridingEntityType(model, typeof(Parent));
        var childType = new OriginalVirtualOverridingEntityType(model, typeof(Child));

        var parentId = parentType.AddProperty(nameof(Parent.Id), typeof(int));
        var parentKey = parentType.AddKey([parentId]);
        parentType.SetPrimaryKey(parentKey);

        var childId = childType.AddProperty(nameof(Child.Id), typeof(int));
        childType.SetPrimaryKey(childType.AddKey([childId]));

        var foreignKeyProperty = childType.AddProperty("ParentId", typeof(int));
        var foreignKey = childType.AddForeignKey([foreignKeyProperty], parentKey, parentType);

        return (parentType, foreignKey);
    }

    private sealed class OriginalVirtualOverridingEntityType(RuntimeModel model, Type type)
        : RuntimeEntityType(
            type.Name, type, sharedClrType: false, model, baseType: null, ChangeTrackingStrategy.Snapshot,
            indexerPropertyInfo: null, propertyBag: false, discriminatorProperty: null, discriminatorValue: null,
            derivedTypesCount: 0, propertyCount: 2, complexPropertyCount: 0, foreignKeyCount: 1, navigationCount: 1,
            skipNavigationCount: 0, servicePropertyCount: 0, unnamedIndexCount: 0, namedIndexCount: 0, keyCount: 1,
            triggerCount: 0)
    {
        public List<string> OriginalAddNavigationCalls { get; } = [];

        public override RuntimeNavigation AddNavigation(
            string name,
            RuntimeForeignKey foreignKey,
            bool onDependent,
            Type clrType,
            PropertyInfo? propertyInfo = null,
            FieldInfo? fieldInfo = null,
            PropertyAccessMode propertyAccessMode = Internal.Model.DefaultPropertyAccessMode,
            bool eagerLoaded = false,
            bool lazyLoadingEnabled = true)
        {
            OriginalAddNavigationCalls.Add(name);
            return base.AddNavigation(
                name, foreignKey, onDependent, clrType, propertyInfo, fieldInfo, propertyAccessMode, eagerLoaded, lazyLoadingEnabled);
        }
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

        // The in-memory provider rejects nullable collection navigations, but the model metadata is what is tested here.
        var model = modelBuilder.FinalizeModel(designTime: false, skipValidation: true);
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
