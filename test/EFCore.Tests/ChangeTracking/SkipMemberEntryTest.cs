// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

public class SkipMemberEntryTest
{
    [ConditionalFact]
    public void Can_get_back_reference_collection()
    {
        using var context = new FreezerContext();
        var entity = new Cherry();

        var entityEntry = context.Add(entity);
        Assert.Same(entityEntry.Entity, entityEntry.Member("Chunkies").EntityEntry.Entity);
    }

    [ConditionalFact]
    public void Can_get_metadata_collection()
    {
        using var context = new FreezerContext();
        var entity = new Cherry();

        var entityEntry = context.Add(entity);
        Assert.Equal("Chunkies", entityEntry.Member("Chunkies").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_and_set_current_value_collection()
    {
        using var context = new FreezerContext();

        var cherry = new Cherry();
        var chunky = new Chunky();
        context.AddRange(chunky, cherry);

        var collection = context.Entry(cherry).Member("Chunkies");
        var inverseCollection = context.Entry(chunky).Member("Cherries");

        Assert.Null(collection.CurrentValue);
        Assert.Null(inverseCollection.CurrentValue);

        collection.CurrentValue = new List<Chunky> { chunky };

        Assert.Same(cherry, chunky.Cherries.Single());
        Assert.Same(chunky, cherry.Chunkies.Single());
        Assert.Equal(cherry, ((ICollection<Cherry>)inverseCollection.CurrentValue).Single());
        Assert.Same(chunky, ((ICollection<Chunky>)collection.CurrentValue).Single());

        collection.CurrentValue = null;

        Assert.Empty(chunky.Cherries);
        Assert.Null(cherry.Chunkies);
        Assert.Empty((IEnumerable)inverseCollection.CurrentValue);
        Assert.Null(collection.CurrentValue);
    }

    [ConditionalFact]
    public void Can_get_skip_collection_entry_by_name_using_Member()
    {
        using var context = new FreezerContext();
        var entity = context.Add(new Cherry()).Entity;

        var entry = context.Entry(entity).Member("Chunkies");
        Assert.Equal("Chunkies", entry.Metadata.Name);
        Assert.IsType<CollectionEntry>(entry);

        entry = context.Entry((object)entity).Member("Chunkies");
        Assert.Equal("Chunkies", entry.Metadata.Name);
        Assert.IsType<CollectionEntry>(entry);
    }

    [ConditionalFact]
    public void Can_get_skip_collection_entry_by_IPropertyBase_using_Member()
    {
        using var context = new FreezerContext();
        var entity = context.Add(new Cherry()).Entity;
        var propertyBase = (IPropertyBase)context.Entry(entity).Metadata.FindSkipNavigation("Chunkies")!;

        var entry = context.Entry(entity).Member(propertyBase);
        Assert.Same(propertyBase, entry.Metadata);
        Assert.IsType<CollectionEntry>(entry);

        entry = context.Entry((object)entity).Member(propertyBase);
        Assert.Same(propertyBase, entry.Metadata);
        Assert.IsType<CollectionEntry>(entry);
    }

    [ConditionalFact]
    public void Can_get_skip_collection_entry_by_name_using_Collection()
    {
        using var context = new FreezerContext();
        var entity = context.Add(new Cherry()).Entity;

        var entry = context.Entry(entity).Collection("Chunkies");
        Assert.Equal("Chunkies", entry.Metadata.Name);
        Assert.IsType<CollectionEntry>(entry);

        entry = context.Entry((object)entity).Collection("Chunkies");
        Assert.Equal("Chunkies", entry.Metadata.Name);
        Assert.IsType<CollectionEntry>(entry);
    }

    [ConditionalFact]
    public void Can_get_skip_collection_entry_by_INavigationBase_using_Collection()
    {
        using var context = new FreezerContext();
        var entity = context.Add(new Cherry()).Entity;
        var navigationBase = (INavigationBase)context.Entry(entity).Metadata.FindSkipNavigation("Chunkies")!;

        var entry = context.Entry(entity).Collection(navigationBase);
        Assert.Same(navigationBase, entry.Metadata);
        Assert.IsType<CollectionEntry>(entry);

        entry = context.Entry((object)entity).Collection(navigationBase);
        Assert.Same(navigationBase, entry.Metadata);
        Assert.IsType<CollectionEntry>(entry);
    }

    private class Chunky
    {
        public int Id { get; set; }
        public ICollection<Cherry> Cherries { get; set; }
    }

    private class Cherry
    {
        public int Id { get; set; }
        public ICollection<Chunky> Chunkies { get; }
    }

    private class FreezerContext : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(nameof(FreezerContext));

        public DbSet<Chunky> Icecream { get; set; }
    }
}
