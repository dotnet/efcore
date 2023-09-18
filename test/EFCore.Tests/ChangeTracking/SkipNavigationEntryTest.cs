// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

public class SkipNavigationEntryTest
{
    [ConditionalFact]
    public void Can_get_back_reference_collection()
    {
        using var context = new FreezerContext();

        var entity = new Cherry();
        context.Add(entity);

        var entityEntry = context.Entry(entity);
        Assert.Same(entityEntry.Entity, entityEntry.Navigation("Chunkies").EntityEntry.Entity);
    }

    [ConditionalFact]
    public void Can_get_metadata_collection()
    {
        using var context = new FreezerContext();

        var entity = new Cherry();
        context.Add(entity);

        Assert.Equal("Chunkies", context.Entry(entity).Navigation("Chunkies").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_and_set_current_value_collection()
    {
        using var context = new FreezerContext();

        var cherry = new Cherry();
        var chunky = new Chunky();
        context.AddRange(chunky, cherry);

        var collection = context.Entry(cherry).Navigation("Chunkies");
        var inverseCollection = context.Entry(chunky).Navigation("Cherries");

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
