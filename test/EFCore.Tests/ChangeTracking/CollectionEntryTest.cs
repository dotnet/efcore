// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

public class CollectionEntryTest
{
    [Fact]
    public void Can_get_back_reference()
    {
        using var context = new FreezerContext();
        var entity = new Cherry();
        context.Add(entity);

        var entityEntry = context.Entry(entity);
        Assert.Same(entityEntry.Entity, entityEntry.Collection("Monkeys").EntityEntry.Entity);
    }

    [Fact]
    public void Can_get_back_reference_generic()
    {
        using var context = new FreezerContext();
        var entity = new Cherry();
        context.Add(entity);

        var entityEntry = context.Entry(entity);
        Assert.Same(entityEntry.Entity, entityEntry.Collection(e => e.Monkeys).EntityEntry.Entity);
    }

    [Fact]
    public void Can_get_metadata()
    {
        using var context = new FreezerContext();
        var entity = new Cherry();
        context.Add(entity);

        Assert.Equal("Monkeys", context.Entry(entity).Collection("Monkeys").Metadata.Name);
    }

    [Fact]
    public void Can_get_metadata_generic()
    {
        using var context = new FreezerContext();
        var entity = new Cherry();
        context.Add(entity);

        Assert.Equal("Monkeys", context.Entry(entity).Collection(e => e.Monkeys).Metadata.Name);
    }

    [Fact]
    public void Can_get_and_set_current_value()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky = new Chunky();
        context.AddRange(chunky, cherry);

        var collection = context.Entry(cherry).Collection("Monkeys");

        Assert.Null(collection.CurrentValue);

        collection.CurrentValue = new List<Chunky> { chunky };

        Assert.Same(cherry, chunky.Garcia);
        Assert.Same(chunky, cherry.Monkeys.Single());
        Assert.Equal(cherry.Id, chunky.GarciaId);
        Assert.Same(chunky, collection.CurrentValue.Cast<Chunky>().Single());
        Assert.Same(collection.FindEntry(chunky)!.GetInfrastructure(), context.Entry(chunky).GetInfrastructure());

        collection.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Null(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Null(collection.CurrentValue);
        Assert.Null(collection.FindEntry(chunky));
    }

    [Fact]
    public void Can_get_and_set_current_value_generic()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky = new Chunky();
        context.AddRange(chunky, cherry);

        var collection = context.Entry(cherry).Collection(e => e.Monkeys);

        Assert.Null(collection.CurrentValue);

        collection.CurrentValue = new List<Chunky> { chunky };

        Assert.Same(cherry, chunky.Garcia);
        Assert.Same(chunky, cherry.Monkeys.Single());
        Assert.Equal(cherry.Id, chunky.GarciaId);
        Assert.Same(chunky, collection.CurrentValue.Single());
        Assert.Same(collection.FindEntry(chunky)!.GetInfrastructure(), context.Entry(chunky).GetInfrastructure());

        collection.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Null(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Null(collection.CurrentValue);
        Assert.Null(collection.FindEntry(chunky));
    }

    [Fact]
    public void Can_get_and_set_current_value_not_tracked()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky = new Chunky();

        var collection = context.Entry(cherry).Collection("Monkeys");

        Assert.Null(collection.CurrentValue);

        collection.CurrentValue = new List<Chunky> { chunky };

        Assert.Null(chunky.Garcia);
        Assert.Same(chunky, cherry.Monkeys.Single());
        Assert.Null(chunky.GarciaId);
        Assert.Same(chunky, collection.CurrentValue.Cast<Chunky>().Single());

        collection.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Null(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Null(collection.CurrentValue);
    }

    [Fact]
    public void Can_get_and_set_current_value_generic_not_tracked()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky = new Chunky();

        var collection = context.Entry(cherry).Collection(e => e.Monkeys);

        Assert.Null(collection.CurrentValue);

        collection.CurrentValue = new List<Chunky> { chunky };

        Assert.Null(chunky.Garcia);
        Assert.Same(chunky, cherry.Monkeys.Single());
        Assert.Null(chunky.GarciaId);
        Assert.Same(chunky, collection.CurrentValue.Single());

        collection.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Null(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Null(collection.CurrentValue);
    }

    [Fact]
    public void Can_get_and_set_current_value_start_tracking()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky = new Chunky();
        context.Add(cherry);

        var collection = context.Entry(cherry).Collection("Monkeys");

        Assert.Null(collection.CurrentValue);

        collection.CurrentValue = new List<Chunky> { chunky };

        Assert.Same(cherry, chunky.Garcia);
        Assert.Same(chunky, cherry.Monkeys.Single());
        Assert.Equal(cherry.Id, chunky.GarciaId);
        Assert.Same(chunky, collection.CurrentValue.Cast<Chunky>().Single());

        Assert.Equal(EntityState.Added, context.Entry(cherry).State);
        Assert.Equal(EntityState.Added, context.Entry(chunky).State);

        collection.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Null(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Null(collection.CurrentValue);

        Assert.Equal(EntityState.Added, context.Entry(cherry).State);
        Assert.Equal(EntityState.Added, context.Entry(chunky).State);
    }

    [Fact]
    public void Can_get_and_set_current_value_start_tracking_generic()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky = new Chunky();
        context.Add(cherry);

        var collection = context.Entry(cherry).Collection(e => e.Monkeys);

        Assert.Null(collection.CurrentValue);

        collection.CurrentValue = new List<Chunky> { chunky };

        Assert.Same(cherry, chunky.Garcia);
        Assert.Same(chunky, cherry.Monkeys.Single());
        Assert.Equal(cherry.Id, chunky.GarciaId);
        Assert.Same(chunky, collection.CurrentValue.Single());

        Assert.Equal(EntityState.Added, context.Entry(cherry).State);
        Assert.Equal(EntityState.Added, context.Entry(chunky).State);

        collection.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Null(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Null(collection.CurrentValue);

        Assert.Equal(EntityState.Added, context.Entry(cherry).State);
        Assert.Equal(EntityState.Added, context.Entry(chunky).State);
    }

    [Fact]
    public void Can_get_and_set_current_value_attached()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky = new Chunky();
        context.AttachRange(chunky, cherry);

        var collection = context.Entry(cherry).Collection("Monkeys");

        Assert.Null(collection.CurrentValue);

        collection.CurrentValue = new List<Chunky> { chunky };

        Assert.Same(cherry, chunky.Garcia);
        Assert.Same(chunky, cherry.Monkeys.Single());
        Assert.Equal(cherry.Id, chunky.GarciaId);
        Assert.Same(chunky, collection.CurrentValue.Cast<Chunky>().Single());

        Assert.Equal(EntityState.Unchanged, context.Entry(cherry).State);
        Assert.Equal(EntityState.Modified, context.Entry(chunky).State);
        Assert.True(context.Entry(chunky).Property(e => e.GarciaId).IsModified);

        collection.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Null(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Null(collection.CurrentValue);

        Assert.Equal(EntityState.Unchanged, context.Entry(cherry).State);
        Assert.Equal(EntityState.Modified, context.Entry(chunky).State);
        Assert.True(context.Entry(chunky).Property(e => e.GarciaId).IsModified);
    }

    [Fact]
    public void Can_get_and_set_current_value_generic_attached()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky = new Chunky();
        context.AttachRange(chunky, cherry);

        var collection = context.Entry(cherry).Collection(e => e.Monkeys);

        Assert.Null(collection.CurrentValue);

        collection.CurrentValue = new List<Chunky> { chunky };

        Assert.Same(cherry, chunky.Garcia);
        Assert.Same(chunky, cherry.Monkeys.Single());
        Assert.Equal(cherry.Id, chunky.GarciaId);
        Assert.Same(chunky, collection.CurrentValue.Single());

        Assert.Equal(EntityState.Unchanged, context.Entry(cherry).State);
        Assert.Equal(EntityState.Modified, context.Entry(chunky).State);
        Assert.True(context.Entry(chunky).Property(e => e.GarciaId).IsModified);

        collection.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Null(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Null(collection.CurrentValue);

        Assert.Equal(EntityState.Unchanged, context.Entry(cherry).State);
        Assert.Equal(EntityState.Modified, context.Entry(chunky).State);
        Assert.True(context.Entry(chunky).Property(e => e.GarciaId).IsModified);
    }

    [Fact]
    public void IsModified_tracks_state_of_FK_property_principal()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky1 = new Chunky { Id = 1, Garcia = cherry };
        var chunky2 = new Chunky { Id = 2, Garcia = cherry };
        cherry.Monkeys = new List<Chunky> { chunky1, chunky2 };
        context.AttachRange(cherry, chunky1, chunky2);

        var collection = context.Entry(cherry).Collection(e => e.Monkeys);

        Assert.False(collection.IsModified);

        context.Entry(chunky1).State = EntityState.Modified;

        Assert.True(collection.IsModified);

        context.Entry(chunky1).State = EntityState.Unchanged;

        Assert.False(collection.IsModified);
    }

    [Theory, InlineData(EntityState.Detached, EntityState.Added), InlineData(EntityState.Added, EntityState.Added),
     InlineData(EntityState.Modified, EntityState.Added), InlineData(EntityState.Deleted, EntityState.Added),
     InlineData(EntityState.Unchanged, EntityState.Added), InlineData(EntityState.Detached, EntityState.Deleted),
     InlineData(EntityState.Added, EntityState.Deleted), InlineData(EntityState.Modified, EntityState.Deleted),
     InlineData(EntityState.Deleted, EntityState.Deleted), InlineData(EntityState.Unchanged, EntityState.Deleted)]
    public void IsModified_can_set_fk_to_modified_principal_with_Added_or_Deleted_dependents(
        EntityState principalState,
        EntityState dependentState)
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky1 = new Chunky { Id = 1, Garcia = cherry };
        var chunky2 = new Chunky { Id = 2, Garcia = cherry };

        cherry.Monkeys = new List<Chunky> { chunky1, chunky2 };

        context.Entry(cherry).State = principalState;
        context.Entry(chunky1).State = dependentState;
        context.Entry(chunky2).State = dependentState;

        var collection = context.Entry(cherry).Collection(e => e.Monkeys);

        Assert.True(collection.IsModified);

        collection.IsModified = false;

        Assert.True(collection.IsModified);
        Assert.False(context.Entry(chunky1).Property(e => e.GarciaId).IsModified);
        Assert.False(context.Entry(chunky2).Property(e => e.GarciaId).IsModified);

        collection.IsModified = true;

        Assert.True(collection.IsModified);
        Assert.False(context.Entry(chunky1).Property(e => e.GarciaId).IsModified);
        Assert.False(context.Entry(chunky2).Property(e => e.GarciaId).IsModified);
        Assert.Equal(dependentState, context.Entry(chunky1).State);
        Assert.Equal(dependentState, context.Entry(chunky2).State);

        if (dependentState == EntityState.Deleted)
        {
            context.Entry(chunky1).State = EntityState.Detached;
            context.Entry(chunky2).State = EntityState.Detached;
        }
        else
        {
            context.Entry(chunky1).State = EntityState.Unchanged;
            context.Entry(chunky2).State = EntityState.Unchanged;
        }

        Assert.False(collection.IsModified);
        Assert.False(context.Entry(chunky1).Property(e => e.GarciaId).IsModified);
        Assert.False(context.Entry(chunky2).Property(e => e.GarciaId).IsModified);
    }

    [Theory, InlineData(EntityState.Detached, EntityState.Unchanged), InlineData(EntityState.Added, EntityState.Unchanged),
     InlineData(EntityState.Modified, EntityState.Unchanged), InlineData(EntityState.Deleted, EntityState.Unchanged),
     InlineData(EntityState.Unchanged, EntityState.Unchanged)]
    public void IsModified_can_set_fk_to_modified_principal_with_Unchanged_dependents(
        EntityState principalState,
        EntityState dependentState)
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky1 = new Chunky { Id = 1, Garcia = cherry };
        var chunky2 = new Chunky { Id = 2, Garcia = cherry };
        cherry.Monkeys = new List<Chunky> { chunky1, chunky2 };

        context.Entry(cherry).State = principalState;
        context.Entry(chunky1).State = dependentState;
        context.Entry(chunky2).State = dependentState;

        var collection = context.Entry(cherry).Collection(e => e.Monkeys);

        Assert.False(collection.IsModified);

        collection.IsModified = true;

        Assert.True(collection.IsModified);
        Assert.True(context.Entry(chunky1).Property(e => e.GarciaId).IsModified);
        Assert.True(context.Entry(chunky2).Property(e => e.GarciaId).IsModified);

        collection.IsModified = false;

        Assert.False(collection.IsModified);
        Assert.False(context.Entry(chunky1).Property(e => e.GarciaId).IsModified);
        Assert.False(context.Entry(chunky2).Property(e => e.GarciaId).IsModified);
        Assert.Equal(dependentState, context.Entry(chunky1).State);
        Assert.Equal(dependentState, context.Entry(chunky2).State);
    }

    [Theory, InlineData(EntityState.Detached, EntityState.Modified), InlineData(EntityState.Added, EntityState.Modified),
     InlineData(EntityState.Modified, EntityState.Modified), InlineData(EntityState.Deleted, EntityState.Modified),
     InlineData(EntityState.Unchanged, EntityState.Modified)]
    public void IsModified_can_set_fk_to_modified_principal_with_Modified_dependents(
        EntityState principalState,
        EntityState dependentState)
    {
        using var context = new FreezerContext();
        var cherry = new Cherry { Id = 1 };
        var chunky1 = new Chunky { Id = 1, Garcia = cherry };
        var chunky2 = new Chunky { Id = 2, Garcia = cherry };
        cherry.Monkeys = new List<Chunky> { chunky1, chunky2 };

        context.Attach(chunky1);
        context.Attach(chunky2);

        context.Entry(cherry).State = principalState;
        context.Entry(chunky1).State = dependentState;
        context.Entry(chunky2).State = dependentState;

        var collection = context.Entry(cherry).Collection(e => e.Monkeys);

        Assert.True(collection.IsModified);

        collection.IsModified = false;

        Assert.False(collection.IsModified);
        Assert.False(context.Entry(chunky1).Property(e => e.GarciaId).IsModified);
        Assert.False(context.Entry(chunky2).Property(e => e.GarciaId).IsModified);

        collection.IsModified = true;

        Assert.True(collection.IsModified);
        Assert.True(context.Entry(chunky1).Property(e => e.GarciaId).IsModified);
        Assert.True(context.Entry(chunky2).Property(e => e.GarciaId).IsModified);
        Assert.Equal(dependentState, context.Entry(chunky1).State);
        Assert.Equal(dependentState, context.Entry(chunky2).State);
    }

    [Fact]
    public void IsModified_reflects_a_nullness_change_of_a_nullable_collection_navigation()
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = [] };
        context.Attach(parent);
        Assert.False(context.Entry(parent).Collection(p => p.Children!).IsModified);

        parent.Children = null;
        context.ChangeTracker.DetectChanges();

        Assert.True(context.Entry(parent).Collection(p => p.Children!).IsModified);
    }

    [Fact]
    public void IsModified_can_be_set_to_flag_and_unflag_a_nullable_collection_navigation()
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = [] };
        context.Attach(parent);

        context.Entry(parent).Collection(p => p.Children!).IsModified = true;

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(NullableCollectionTestHelpers.IsChildrenModified(context, parent));

        context.Entry(parent).Collection(p => p.Children!).IsModified = false;

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(NullableCollectionTestHelpers.IsChildrenModified(context, parent));
        Assert.False(context.Entry(parent).Collection(p => p.Children!).IsModified);
    }

    [Fact]
    public void IsModified_of_a_collection_navigation_that_is_not_nullable_ignores_nullness()
    {
        using var context = new NotNullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = [] };
        context.Attach(parent);

        context.Entry(parent).Collection(p => p.Children!).IsModified = true;

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(context.Entry(parent).Collection(p => p.Children!).IsModified);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Setting_IsModified_to_false_reverts_a_nullness_change_of_a_nullable_collection_navigation(bool startNull)
    {
        using var context = new NullableCollectionNavigationContext();
        var original = startNull ? null : new List<NullableCollectionChild>();
        var parent = new NullableCollectionParent { Id = 1, Children = original };
        context.Attach(parent);
        parent.Children = startNull ? [] : null;
        context.ChangeTracker.DetectChanges();
        Assert.True(NullableCollectionTestHelpers.IsChildrenModified(context, parent));

        context.Entry(parent).Collection(p => p.Children!).IsModified = false;
        context.ChangeTracker.DetectChanges();

        if (startNull)
        {
            Assert.Null(parent.Children);
        }
        else
        {
            Assert.Same(original, parent.Children);
        }

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(NullableCollectionTestHelpers.IsChildrenModified(context, parent));
    }

    [Fact]
    public void Setting_IsModified_to_false_does_not_revert_a_replacement_that_is_not_a_nullness_change()
    {
        using var context = new NullableCollectionNavigationContext();
        var original = new List<NullableCollectionChild> { new() { Id = 2 } };
        var parent = new NullableCollectionParent { Id = 1, Children = original };
        context.Attach(parent);

        var replacement = new List<NullableCollectionChild>();
        parent.Children = replacement;
        context.ChangeTracker.DetectChanges();
        Assert.False(NullableCollectionTestHelpers.IsChildrenModified(context, parent));

        context.Entry(parent).Collection(p => p.Children!).IsModified = false;

        Assert.Same(replacement, parent.Children);
        Assert.False(NullableCollectionTestHelpers.IsChildrenModified(context, parent));
    }

    [Fact]
    public void Setting_IsModified_to_false_does_not_revert_a_null_nullable_collection_navigation_that_was_null()
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = null };
        context.Attach(parent);

        context.Entry(parent).Collection(p => p.Children!).IsModified = false;

        Assert.Null(parent.Children);
        Assert.False(NullableCollectionTestHelpers.IsChildrenModified(context, parent));
    }

    [Fact]
    public void Querying_a_null_nullable_collection_navigation_does_not_replace_it_with_an_empty_collection()
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = null };
        context.Attach(parent);

        _ = context.Entry(parent).Collection(p => p.Children!).Query();

        AssertStillNull(context, parent);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Loading_or_querying_a_null_collection_navigation_that_is_not_nullable_still_creates_the_collection(bool load)
    {
        using var context = new RegularCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = null };
        context.Attach(parent);

        if (load)
        {
            context.Entry(parent).Collection(p => p.Children!).Load();
        }
        else
        {
            _ = context.Entry(parent).Collection(p => p.Children!).Query();
        }

        Assert.NotNull(parent.Children);
    }

    private class RegularCollectionNavigationContext : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<NullableCollectionParent>().HasMany(p => p.Children).WithOne();
    }

    private static void AssertStillNull(DbContext context, NullableCollectionParent parent)
    {
        Assert.Null(parent.Children);
        context.ChangeTracker.DetectChanges();
        Assert.Null(parent.Children);
        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(NullableCollectionTestHelpers.IsChildrenModified(context, parent));
    }

    private class Chunky
    {
        public int Monkey { get; set; }
        public int Id { get; set; }

        public int? GarciaId { get; set; }
        public Cherry Garcia { get; set; } = null!;
    }

    private class Cherry
    {
        public int Garcia { get; set; }
        public int Id { get; set; }

        public ICollection<Chunky> Monkeys { get; set; } = null!;
    }

    private class FreezerContext : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(nameof(FreezerContext));

        public DbSet<Chunky> Icecream { get; set; } = null!;

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chunky>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<Cherry>().Property(e => e.Id).ValueGeneratedNever();
        }
    }
}
