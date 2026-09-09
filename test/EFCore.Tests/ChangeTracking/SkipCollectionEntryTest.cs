// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

public class SkipCollectionEntryTest
{
    [ConditionalFact]
    public void Can_get_all_member_entries()
    {
        using var context = new FreezerContext();

        Assert.Equal(
            ["Id", "Cherries"],
            context.Attach(new Chunky()).Members.Select(e => e.Metadata.Name).ToList());
    }

    [ConditionalFact]
    public void Can_get_all_navigation_entries()
    {
        using var context = new FreezerContext();

        Assert.Equal(
            ["Cherries"],
            context.Attach(new Chunky()).Navigations.Select(e => e.Metadata.Name).ToList());
    }

    [ConditionalFact]
    public void Can_get_all_collection_entries()
    {
        using var context = new FreezerContext();

        Assert.Equal(
            ["Cherries"],
            context.Attach(new Chunky()).Collections.Select(e => e.Metadata.Name).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Can_get_back_reference(bool useExplicitPk)
    {
        using var context = useExplicitPk ? new ExplicitFreezerContext() : new FreezerContext();

        var entity = new Cherry();
        context.Add(entity);

        var entityEntry = context.Entry(entity);
        Assert.Same(entityEntry.Entity, entityEntry.Collection("Chunkies").EntityEntry.Entity);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Can_get_back_reference_generic(bool useExplicitPk)
    {
        using var context = useExplicitPk ? new ExplicitFreezerContext() : new FreezerContext();

        var entity = new Cherry();
        context.Add(entity);

        var entityEntry = context.Entry(entity);
        Assert.Same(entityEntry.Entity, entityEntry.Collection(e => e.Chunkies).EntityEntry.Entity);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Can_get_metadata(bool useExplicitPk)
    {
        using var context = useExplicitPk ? new ExplicitFreezerContext() : new FreezerContext();

        var entity = new Cherry();
        context.Add(entity);

        Assert.Equal("Chunkies", context.Entry(entity).Collection("Chunkies").Metadata.Name);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Can_get_metadata_generic(bool useExplicitPk)
    {
        using var context = useExplicitPk ? new ExplicitFreezerContext() : new FreezerContext();

        var entity = new Cherry();
        context.Add(entity);

        Assert.Equal("Chunkies", context.Entry(entity).Collection(e => e.Chunkies).Metadata.Name);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Can_get_and_set_current_value(bool useExplicitPk)
    {
        using var context = useExplicitPk ? new ExplicitFreezerContext() : new FreezerContext();

        var cherry = new Cherry();
        var chunky = new Chunky();
        context.AddRange(chunky, cherry);

        var collection = context.Entry(cherry).Collection("Chunkies");
        var inverseCollection = context.Entry(chunky).Collection("Cherries");

        Assert.Null(collection.CurrentValue);

        collection.CurrentValue = new List<Chunky> { chunky };

        Assert.Same(chunky, cherry.Chunkies.Single());
        Assert.Same(cherry, chunky.Cherries.Single());
        Assert.Same(chunky, collection.CurrentValue.Cast<Chunky>().Single());
        Assert.Same(cherry, inverseCollection.CurrentValue.Cast<Cherry>().Single());
        Assert.Same(collection.FindEntry(chunky).GetInfrastructure(), context.Entry(chunky).GetInfrastructure());

        collection.CurrentValue = null;

        Assert.Empty(chunky.Cherries);
        Assert.Null(cherry.Chunkies);
        Assert.Null(collection.CurrentValue);
        Assert.Empty(inverseCollection.CurrentValue);
        Assert.Null(collection.FindEntry(chunky));
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Can_get_and_set_current_value_generic(bool useExplicitPk)
    {
        using var context = useExplicitPk ? new ExplicitFreezerContext() : new FreezerContext();

        var cherry = new Cherry();
        var chunky = new Chunky();
        context.AddRange(chunky, cherry);

        var collection = context.Entry(cherry).Collection(e => e.Chunkies);
        var inverseCollection = context.Entry(chunky).Collection(e => e.Cherries);

        Assert.Null(collection.CurrentValue);

        collection.CurrentValue = new List<Chunky> { chunky };

        Assert.Same(chunky, cherry.Chunkies.Single());
        Assert.Same(cherry, chunky.Cherries.Single());
        Assert.Same(chunky, collection.CurrentValue.Single());
        Assert.Same(cherry, inverseCollection.CurrentValue.Single());
        Assert.Same(collection.FindEntry(chunky).GetInfrastructure(), context.Entry(chunky).GetInfrastructure());

        collection.CurrentValue = null;

        Assert.Empty(chunky.Cherries);
        Assert.Null(cherry.Chunkies);
        Assert.Null(collection.CurrentValue);
        Assert.Empty(inverseCollection.CurrentValue);
        Assert.Null(collection.FindEntry(chunky));
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Can_get_and_set_current_value_not_tracked(bool useExplicitPk)
    {
        using var context = useExplicitPk ? new ExplicitFreezerContext() : new FreezerContext();

        var cherry = new Cherry();
        var chunky = new Chunky();

        var collection = context.Entry(cherry).Collection("Chunkies");
        var inverseCollection = context.Entry(chunky).Collection("Cherries");

        Assert.Null(collection.CurrentValue);

        collection.CurrentValue = new List<Chunky> { chunky };

        Assert.Same(chunky, cherry.Chunkies.Single());
        Assert.Null(chunky.Cherries);
        Assert.Same(chunky, collection.CurrentValue.Cast<Chunky>().Single());
        Assert.Null(inverseCollection.CurrentValue);

        collection.CurrentValue = null;

        Assert.Null(chunky.Cherries);
        Assert.Null(cherry.Chunkies);
        Assert.Null(collection.CurrentValue);
        Assert.Null(inverseCollection.CurrentValue);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Can_get_and_set_current_value_generic_not_tracked(bool useExplicitPk)
    {
        using var context = useExplicitPk ? new ExplicitFreezerContext() : new FreezerContext();

        var cherry = new Cherry();
        var chunky = new Chunky();

        var collection = context.Entry(cherry).Collection(e => e.Chunkies);
        var inverseCollection = context.Entry(chunky).Collection(e => e.Cherries);

        Assert.Null(collection.CurrentValue);

        collection.CurrentValue = new List<Chunky> { chunky };

        Assert.Same(chunky, cherry.Chunkies.Single());
        Assert.Null(chunky.Cherries);
        Assert.Same(chunky, collection.CurrentValue.Single());
        Assert.Null(inverseCollection.CurrentValue);

        collection.CurrentValue = null;

        Assert.Null(chunky.Cherries);
        Assert.Null(cherry.Chunkies);
        Assert.Null(collection.CurrentValue);
        Assert.Null(inverseCollection.CurrentValue);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Can_get_and_set_current_value_start_tracking(bool useExplicitPk)
    {
        using var context = useExplicitPk ? new ExplicitFreezerContext() : new FreezerContext();

        var cherry = new Cherry();
        var chunky = new Chunky();
        context.Add(cherry);

        var collection = context.Entry(cherry).Collection("Chunkies");
        var inverseCollection = context.Entry(chunky).Collection("Cherries");

        Assert.Null(collection.CurrentValue);

        collection.CurrentValue = new List<Chunky> { chunky };

        Assert.Same(chunky, cherry.Chunkies.Single());
        Assert.Same(cherry, chunky.Cherries.Single());
        Assert.Same(chunky, collection.CurrentValue.Cast<Chunky>().Single());
        Assert.Same(cherry, inverseCollection.CurrentValue.Cast<Cherry>().Single());

        Assert.Equal(EntityState.Added, context.Entry(cherry).State);
        Assert.Equal(EntityState.Added, context.Entry(chunky).State);

        collection.CurrentValue = null;

        Assert.Empty(chunky.Cherries);
        Assert.Null(cherry.Chunkies);
        Assert.Null(collection.CurrentValue);
        Assert.Empty(inverseCollection.CurrentValue);

        Assert.Equal(EntityState.Added, context.Entry(cherry).State);
        Assert.Equal(EntityState.Added, context.Entry(chunky).State);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Can_get_and_set_current_value_start_tracking_generic(bool useExplicitPk)
    {
        using var context = useExplicitPk ? new ExplicitFreezerContext() : new FreezerContext();

        var cherry = new Cherry();
        var chunky = new Chunky();
        context.Add(cherry);

        var collection = context.Entry(cherry).Collection(e => e.Chunkies);
        var inverseCollection = context.Entry(chunky).Collection(e => e.Cherries);

        Assert.Null(collection.CurrentValue);

        collection.CurrentValue = new List<Chunky> { chunky };

        Assert.Same(chunky, cherry.Chunkies.Single());
        Assert.Same(cherry, chunky.Cherries.Single());
        Assert.Same(chunky, collection.CurrentValue.Single());
        Assert.Same(cherry, inverseCollection.CurrentValue.Single());

        Assert.Equal(EntityState.Added, context.Entry(cherry).State);
        Assert.Equal(EntityState.Added, context.Entry(chunky).State);

        collection.CurrentValue = null;

        Assert.Empty(chunky.Cherries);
        Assert.Null(cherry.Chunkies);
        Assert.Null(collection.CurrentValue);
        Assert.Empty(inverseCollection.CurrentValue);

        Assert.Equal(EntityState.Added, context.Entry(cherry).State);
        Assert.Equal(EntityState.Added, context.Entry(chunky).State);
    }

    [ConditionalTheory]
    [InlineData(false, CascadeTiming.Immediate)]
    [InlineData(true, CascadeTiming.Immediate)]
    [InlineData(false, CascadeTiming.OnSaveChanges)]
    [InlineData(true, CascadeTiming.OnSaveChanges)]
    [InlineData(false, CascadeTiming.Never)]
    [InlineData(true, CascadeTiming.Never)]
    public void IsModified_tracks_detects_deletion_of_related_entity(bool useExplicitPk, CascadeTiming cascadeTiming)
    {
        using var context = useExplicitPk ? new ExplicitFreezerContext() : new FreezerContext();

        context.ChangeTracker.CascadeDeleteTiming = cascadeTiming;

        var cherry1 = new Cherry { Id = 1 };
        var cherry2 = new Cherry { Id = 2 };
        var chunky1 = new Chunky { Id = 1 };
        var chunky2 = new Chunky { Id = 2 };

        AttachGraph(context, cherry1, cherry2, chunky1, chunky2);

        var relatedToCherry1 = context.Entry(cherry1).Collection(e => e.Chunkies);
        var relatedToCherry2 = context.Entry(cherry2).Collection(e => e.Chunkies);
        var relatedToChunky1 = context.Entry(chunky1).Collection(e => e.Cherries);
        var relatedToChunky2 = context.Entry(chunky2).Collection(e => e.Cherries);

        Assert.False(relatedToCherry1.IsModified);
        Assert.False(relatedToCherry2.IsModified);
        Assert.False(relatedToChunky1.IsModified);
        Assert.False(relatedToChunky2.IsModified);

        context.Entry(chunky1).State = EntityState.Deleted;

        Assert.True(relatedToCherry1.IsModified);
        Assert.False(relatedToCherry2.IsModified);
        Assert.True(relatedToChunky1.IsModified);
        Assert.False(relatedToChunky2.IsModified);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void IsModified_tracks_adding_new_related_entity(bool useExplicitPk)
    {
        using var context = useExplicitPk ? new ExplicitFreezerContext() : new FreezerContext();

        var cherry1 = new Cherry { Id = 1 };
        var cherry2 = new Cherry { Id = 2 };
        var chunky1 = new Chunky { Id = 1 };
        var chunky2 = new Chunky { Id = 2 };

        AttachGraph(context, cherry1, cherry2, chunky1, chunky2);

        var relatedToCherry1 = context.Entry(cherry1).Collection(e => e.Chunkies);
        var relatedToCherry2 = context.Entry(cherry2).Collection(e => e.Chunkies);
        var relatedToChunky1 = context.Entry(chunky1).Collection(e => e.Cherries);
        var relatedToChunky2 = context.Entry(chunky2).Collection(e => e.Cherries);

        var chunky3 = new Chunky { Id = 3 };
        cherry1.Chunkies.Add(chunky3);
        context.ChangeTracker.DetectChanges();

        Assert.True(relatedToCherry1.IsModified);
        Assert.False(relatedToCherry2.IsModified);
        Assert.False(relatedToChunky1.IsModified);
        Assert.False(relatedToChunky2.IsModified);

        context.Entry(chunky3).State = EntityState.Detached;

        Assert.False(relatedToCherry1.IsModified);
        Assert.False(relatedToCherry2.IsModified);
        Assert.False(relatedToChunky1.IsModified);
        Assert.False(relatedToChunky2.IsModified);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void IsModified_tracks_removing_items_from_the_join_table(bool useExplicitPk)
    {
        using var context = useExplicitPk ? new ExplicitFreezerContext() : new FreezerContext();

        var cherry1 = new Cherry { Id = 1 };
        var cherry2 = new Cherry { Id = 2 };
        var chunky1 = new Chunky { Id = 1 };
        var chunky2 = new Chunky { Id = 2 };

        AttachGraph(context, cherry1, cherry2, chunky1, chunky2);

        var relatedToCherry1 = context.Entry(cherry1).Collection(e => e.Chunkies);
        var relatedToCherry2 = context.Entry(cherry2).Collection(e => e.Chunkies);
        var relatedToChunky1 = context.Entry(chunky1).Collection(e => e.Cherries);
        var relatedToChunky2 = context.Entry(chunky2).Collection(e => e.Cherries);

        cherry1.Chunkies.Remove(chunky2);
        context.ChangeTracker.DetectChanges();

        Assert.True(relatedToCherry1.IsModified);
        Assert.False(relatedToCherry2.IsModified);
        Assert.False(relatedToChunky1.IsModified);
        Assert.True(relatedToChunky2.IsModified);

        cherry1.Chunkies.Add(chunky2);
        context.ChangeTracker.DetectChanges();

        Assert.True(relatedToCherry1.IsModified);
        Assert.False(relatedToCherry2.IsModified);
        Assert.False(relatedToChunky1.IsModified);
        Assert.True(relatedToChunky2.IsModified);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void IsModified_tracks_adding_items_to_the_join_table(bool useExplicitPk)
    {
        using var context = useExplicitPk ? new ExplicitFreezerContext() : new FreezerContext();

        var cherry1 = new Cherry { Id = 1 };
        var cherry2 = new Cherry { Id = 2 };
        var chunky1 = new Chunky { Id = 1 };
        var chunky2 = new Chunky { Id = 2 };

        AttachGraph(context, cherry1, cherry2, chunky1, chunky2);

        var relatedToCherry1 = context.Entry(cherry1).Collection(e => e.Chunkies);
        var relatedToCherry2 = context.Entry(cherry2).Collection(e => e.Chunkies);
        var relatedToChunky1 = context.Entry(chunky1).Collection(e => e.Cherries);
        var relatedToChunky2 = context.Entry(chunky2).Collection(e => e.Cherries);

        cherry2.Chunkies.Add(chunky1);
        context.ChangeTracker.DetectChanges();

        Assert.False(relatedToCherry1.IsModified);
        Assert.True(relatedToCherry2.IsModified);
        Assert.True(relatedToChunky1.IsModified);
        Assert.False(relatedToChunky2.IsModified);
    }

    [ConditionalFact]
    public void IsModified_tracks_mutation_of_join_fks()
    {
        using var context = new ExplicitFreezerContext();

        var cherry1 = new Cherry { Id = 1 };
        var cherry2 = new Cherry { Id = 2 };
        var chunky1 = new Chunky { Id = 1 };
        var chunky2 = new Chunky { Id = 2 };

        AttachGraph(context, cherry1, cherry2, chunky1, chunky2);

        var relatedToCherry1 = context.Entry(cherry1).Collection(e => e.Chunkies);
        var relatedToCherry2 = context.Entry(cherry2).Collection(e => e.Chunkies);
        var relatedToChunky1 = context.Entry(chunky1).Collection(e => e.Cherries);
        var relatedToChunky2 = context.Entry(chunky2).Collection(e => e.Cherries);

        var joinEntity = context.ChangeTracker.Entries<Dictionary<string, object>>()
            .Single(e => e.Property<int>("CherryId").CurrentValue == 1 && e.Property<int>("ChunkyId").CurrentValue == 2)
            .Entity;

        joinEntity["CherryId"] = 2;
        context.ChangeTracker.DetectChanges();

        Assert.False(relatedToCherry1.IsModified);
        Assert.True(relatedToCherry2.IsModified);
        Assert.False(relatedToChunky1.IsModified);
        Assert.True(relatedToChunky2.IsModified);

        joinEntity["CherryId"] = 1;
        context.ChangeTracker.DetectChanges();

        Assert.True(relatedToCherry1.IsModified);
        Assert.False(relatedToCherry2.IsModified);
        Assert.False(relatedToChunky1.IsModified);
        Assert.True(relatedToChunky2.IsModified);
    }

    [ConditionalFact]
    public void Setting_IsModified_true_marks_all_join_table_FK_modified()
    {
        using var context = new ExplicitFreezerContext();

        var cherry1 = new Cherry { Id = 1 };
        var cherry2 = new Cherry { Id = 2 };
        var chunky1 = new Chunky { Id = 1 };
        var chunky2 = new Chunky { Id = 2 };

        AttachGraph(context, cherry1, cherry2, chunky1, chunky2);

        var relatedToCherry1 = context.Entry(cherry1).Collection(e => e.Chunkies);
        var relatedToCherry2 = context.Entry(cherry2).Collection(e => e.Chunkies);
        var relatedToChunky1 = context.Entry(chunky1).Collection(e => e.Cherries);
        var relatedToChunky2 = context.Entry(chunky2).Collection(e => e.Cherries);

        Assert.False(relatedToCherry1.IsModified);
        Assert.False(relatedToCherry2.IsModified);
        Assert.False(relatedToChunky1.IsModified);
        Assert.False(relatedToChunky2.IsModified);

        foreach (var joinEntry in context.ChangeTracker.Entries<Dictionary<string, object>>())
        {
            Assert.Equal(EntityState.Unchanged, joinEntry.State);
        }

        relatedToCherry1.IsModified = true;

        Assert.True(relatedToCherry1.IsModified);
        Assert.False(relatedToCherry2.IsModified);
        Assert.True(relatedToChunky1.IsModified);
        Assert.True(relatedToChunky2.IsModified);

        foreach (var joinEntry in context.ChangeTracker.Entries<Dictionary<string, object>>())
        {
            Assert.Equal(EntityState.Modified, joinEntry.State);
        }
    }

    [ConditionalFact]
    public void Setting_IsModified_false_reverts_changes_to_join_table_FKs()
    {
        using var context = new ExplicitFreezerContext();

        var cherry1 = new Cherry { Id = 1 };
        var cherry2 = new Cherry { Id = 2 };
        var chunky1 = new Chunky { Id = 1 };
        var chunky2 = new Chunky { Id = 2 };

        AttachGraph(context, cherry1, cherry2, chunky1, chunky2);

        var relatedToCherry1 = context.Entry(cherry1).Collection(e => e.Chunkies);
        var relatedToCherry2 = context.Entry(cherry2).Collection(e => e.Chunkies);
        var relatedToChunky1 = context.Entry(chunky1).Collection(e => e.Cherries);
        var relatedToChunky2 = context.Entry(chunky2).Collection(e => e.Cherries);

        var joinEntity = context.ChangeTracker.Entries<Dictionary<string, object>>()
            .Single(e => e.Property<int>("CherryId").CurrentValue == 1 && e.Property<int>("ChunkyId").CurrentValue == 2)
            .Entity;

        joinEntity["CherryId"] = 2;
        context.ChangeTracker.DetectChanges();

        Assert.False(relatedToCherry1.IsModified);
        Assert.True(relatedToCherry2.IsModified);
        Assert.False(relatedToChunky1.IsModified);
        Assert.True(relatedToChunky2.IsModified);

        relatedToCherry2.IsModified = false;

        Assert.False(relatedToCherry1.IsModified);
        Assert.False(relatedToCherry2.IsModified);
        Assert.False(relatedToChunky1.IsModified);
        Assert.False(relatedToChunky2.IsModified);

        foreach (var joinEntry in context.ChangeTracker.Entries<Dictionary<string, object>>())
        {
            Assert.Equal(EntityState.Unchanged, joinEntry.State);
        }
    }

    private static void AttachGraph(FreezerContext context, Cherry cherry1, Cherry cherry2, Chunky chunky1, Chunky chunky2)
    {
        cherry1.Chunkies = new List<Chunky> { chunky1, chunky2 };
        cherry2.Chunkies = new List<Chunky>();

        if (context is ExplicitFreezerContext)
        {
            context.AddRange(cherry1, cherry2, chunky1, chunky2); // So that PKs get generated values
            context.ChangeTracker.Entries().ToList().ForEach(e => e.State = EntityState.Unchanged);
        }
        else
        {
            context.AttachRange(cherry1, cherry2, chunky1, chunky2);
        }
    }

    private class Chunky
    {
        public int Id { get; set; }
        public ICollection<Cherry> Cherries { get; set; }
    }

    private class Cherry
    {
        public int Id { get; set; }
        public ICollection<Chunky> Chunkies { get; set; }
    }

    private class FreezerContext : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(nameof(FreezerContext));

        public DbSet<Chunky> Icecream { get; set; }
    }

    private class ExplicitFreezerContext : FreezerContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(nameof(ExplicitFreezerContext));

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder
                .Entity<Cherry>().HasMany(e => e.Chunkies).WithMany(e => e.Cherries)
                .UsingEntity<Dictionary<string, object>>(
                    "CherryChunky",
                    b => b.HasOne<Chunky>().WithMany().HasForeignKey("ChunkyId"),
                    b => b.HasOne<Cherry>().WithMany().HasForeignKey("CherryId"))
                .IndexerProperty<int>("Id");
    }
}
