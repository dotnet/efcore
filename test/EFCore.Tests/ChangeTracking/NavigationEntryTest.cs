// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

public class NavigationEntryTest
{
    [ConditionalFact]
    public void Can_get_back_reference_reference()
    {
        using var context = new FreezerContext();
        var entity = new Chunky();
        context.Add(entity);

        var entityEntry = context.Entry(entity);
        Assert.Same(entityEntry.Entity, entityEntry.Navigation("Garcia").EntityEntry.Entity);
    }

    [ConditionalFact]
    public void Can_get_back_reference_collection()
    {
        using var context = new FreezerContext();
        var entity = new Cherry();
        context.Add(entity);

        var entityEntry = context.Entry(entity);
        Assert.Same(entityEntry.Entity, entityEntry.Navigation("Monkeys").EntityEntry.Entity);
    }

    [ConditionalFact]
    public void Can_get_metadata_reference()
    {
        using var context = new FreezerContext();
        var entity = new Chunky();
        context.Add(entity);

        Assert.Equal("Garcia", context.Entry(entity).Navigation("Garcia").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_metadata_collection()
    {
        using var context = new FreezerContext();
        var entity = new Cherry();
        context.Add(entity);

        Assert.Equal("Monkeys", context.Entry(entity).Navigation("Monkeys").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_and_set_current_value_reference()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky = new Chunky();
        context.AddRange(chunky, cherry);

        var reference = context.Entry(chunky).Navigation("Garcia");

        Assert.Null(reference.CurrentValue);

        reference.CurrentValue = cherry;

        Assert.Same(cherry, chunky.Garcia);
        Assert.Same(chunky, cherry.Monkeys.Single());
        Assert.Equal(cherry.Id, chunky.GarciaId);
        Assert.Same(cherry, reference.CurrentValue);

        reference.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Empty(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Null(reference.CurrentValue);
    }

    [ConditionalFact]
    public void Can_get_and_set_current_value_collection()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky = new Chunky();
        context.AddRange(chunky, cherry);

        var collection = context.Entry(cherry).Navigation("Monkeys");

        Assert.Null(collection.CurrentValue);

        collection.CurrentValue = new List<Chunky> { chunky };

        Assert.Same(cherry, chunky.Garcia);
        Assert.Same(chunky, cherry.Monkeys.Single());
        Assert.Equal(cherry.Id, chunky.GarciaId);
        Assert.Same(chunky, ((ICollection<Chunky>)collection.CurrentValue).Single());

        collection.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Null(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Null(collection.CurrentValue);
    }

    [ConditionalFact]
    public void IsModified_tracks_state_of_FK_property_reference()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky = new Chunky { Garcia = cherry };
        cherry.Monkeys = new List<Chunky> { chunky };
        context.AttachRange(cherry, chunky);

        var reference = context.Entry(chunky).Navigation("Garcia");

        Assert.False(reference.IsModified);

        chunky.GarciaId = null;
        context.ChangeTracker.DetectChanges();

        Assert.True(reference.IsModified);

        context.Entry(chunky).State = EntityState.Unchanged;

        Assert.False(reference.IsModified);
    }

    [ConditionalFact]
    public void IsModified_can_set_fk_to_modified_collection()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky1 = new Chunky { Garcia = cherry };
        var chunky2 = new Chunky { Garcia = cherry };
        cherry.Monkeys = new List<Chunky> { chunky1, chunky2 };
        context.AttachRange(cherry, chunky1, chunky2);

        var collection = context.Entry(cherry).Navigation("Monkeys");

        Assert.False(collection.IsModified);

        collection.IsModified = true;

        Assert.True(collection.IsModified);
        Assert.True(context.Entry(chunky1).Property(e => e.GarciaId).IsModified);
        Assert.True(context.Entry(chunky2).Property(e => e.GarciaId).IsModified);

        collection.IsModified = false;

        Assert.False(collection.IsModified);
        Assert.False(context.Entry(chunky1).Property(e => e.GarciaId).IsModified);
        Assert.False(context.Entry(chunky2).Property(e => e.GarciaId).IsModified);
        Assert.Equal(EntityState.Unchanged, context.Entry(chunky1).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(chunky2).State);
    }

    private class Chunky
    {
        public int Monkey { get; set; }
        public int Id { get; set; }

        public int? GarciaId { get; set; }
        public Cherry Garcia { get; set; }
    }

    private class Cherry
    {
        public int Garcia { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int Id { get; set; }

        public ICollection<Chunky> Monkeys { get; set; }
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
