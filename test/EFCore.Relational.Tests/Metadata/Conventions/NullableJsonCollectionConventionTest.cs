// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class NullableJsonCollectionConventionTest
{
    [Fact]
    public void Nullable_owned_collection_mapped_to_json_is_nullable()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<WithNullableItems>().OwnsMany(p => p.Items, b => b.ToJson());

        var model = modelBuilder.FinalizeModel();

        Assert.True(GetNavigation(model, typeof(WithNullableItems), nameof(WithNullableItems.Items)).IsNullable);
    }

    [Fact]
    public void Non_nullable_owned_collection_mapped_to_json_is_not_nullable()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<WithNonNullableItems>().OwnsMany(p => p.Items, b => b.ToJson());

        var model = modelBuilder.FinalizeModel();

        Assert.False(GetNavigation(model, typeof(WithNonNullableItems), nameof(WithNonNullableItems.Items)).IsNullable);
    }

    [Fact]
    public void Nullable_owned_collection_not_mapped_to_json_is_not_nullable()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<WithNullableItems>().OwnsMany(p => p.Items);

        var model = modelBuilder.FinalizeModel();

        Assert.False(GetNavigation(model, typeof(WithNullableItems), nameof(WithNullableItems.Items)).IsNullable);
    }

    [Fact]
    public void Owned_collections_nested_inside_json_are_nullable_when_annotated_nullable()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<WithReference>().OwnsOne(
            p => p.Reference, b =>
            {
                b.ToJson();
                b.OwnsMany(r => r.NullableItems);
                b.OwnsMany(r => r.Items);
            });

        var model = modelBuilder.FinalizeModel();

        Assert.True(GetNavigation(model, typeof(NestedReference), nameof(NestedReference.NullableItems)).IsNullable);
        Assert.False(GetNavigation(model, typeof(NestedReference), nameof(NestedReference.Items)).IsNullable);
    }

    [Fact]
    public void Explicit_configuration_is_not_overridden_by_the_convention()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<WithNullableItems>().OwnsMany(p => p.Items, b => b.ToJson());
        modelBuilder.Model.FindEntityType(typeof(WithNullableItems))!.FindNavigation(nameof(WithNullableItems.Items))!
            .SetIsNullable(false);

        var model = modelBuilder.FinalizeModel();

        Assert.False(GetNavigation(model, typeof(WithNullableItems), nameof(WithNullableItems.Items)).IsNullable);
    }

    [Fact]
    public void Owned_collection_in_a_nullable_oblivious_context_is_not_nullable()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<WithObliviousItems>().OwnsMany(p => p.Items, b => b.ToJson());

        var model = modelBuilder.FinalizeModel();

        Assert.False(GetNavigation(model, typeof(WithObliviousItems), nameof(WithObliviousItems.Items)).IsNullable);
    }

    [Fact]
    public void Nullable_foreign_key_collection_is_not_nullable()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<WithForeignKeyChildren>().HasMany(p => p.Children).WithOne();

        var model = modelBuilder.FinalizeModel();

        Assert.False(GetNavigation(model, typeof(WithForeignKeyChildren), nameof(WithForeignKeyChildren.Children)).IsNullable);
    }

    [Fact]
    public void Nullable_owned_reference_mapped_to_json_is_not_a_nullable_collection()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<WithReference>().OwnsOne(
            p => p.Reference, b =>
            {
                b.ToJson();
                b.Ignore(r => r.Items);
                b.Ignore(r => r.NullableItems);
            });

        var model = modelBuilder.FinalizeModel();

        Assert.False(GetNavigation(model, typeof(WithReference), nameof(WithReference.Reference)).IsNullable);
    }

    [Fact]
    public void Nullable_owned_collection_declared_on_a_derived_type_is_nullable()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<BaseWithoutItems>();
        modelBuilder.Entity<DerivedWithNullableItems>().OwnsMany(p => p.Items, b => b.ToJson());

        var model = modelBuilder.FinalizeModel();

        Assert.True(GetNavigation(model, typeof(DerivedWithNullableItems), nameof(DerivedWithNullableItems.Items)).IsNullable);
    }

    private static INavigation GetNavigation(IModel model, Type entityType, string name)
        => model.FindEntityType(entityType)!.FindNavigation(name)!;

    private class WithNullableItems
    {
        public int Id { get; set; }
        public List<Item>? Items { get; set; }
    }

    private class WithNonNullableItems
    {
        public int Id { get; set; }
        public List<Item> Items { get; set; } = null!;
    }

    private class WithReference
    {
        public int Id { get; set; }
        public NestedReference? Reference { get; set; }
    }

    private class NestedReference
    {
        public List<Item> Items { get; set; } = null!;
        public List<Item>? NullableItems { get; set; }
    }

    private class WithForeignKeyChildren
    {
        public int Id { get; set; }
        public List<ForeignKeyChild>? Children { get; set; }
    }

    private class ForeignKeyChild
    {
        public int Id { get; set; }
    }

    private class BaseWithoutItems
    {
        public int Id { get; set; }
    }

    private class DerivedWithNullableItems : BaseWithoutItems
    {
        public List<Item>? Items { get; set; }
    }

#nullable disable
    private class WithObliviousItems
    {
        public int Id { get; set; }
        public List<Item> Items { get; set; }
    }
#nullable restore

    private class Item
    {
        public string? Name { get; set; }
    }
}
