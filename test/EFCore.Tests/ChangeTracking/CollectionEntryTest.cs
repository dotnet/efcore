// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

public class CollectionEntryTest
{
    [ConditionalFact]
    public void Can_get_back_reference()
    {
        using var context = new FreezerContext();
        var entity = new Cherry();
        context.Add(entity);

        var entityEntry = context.Entry(entity);
        Assert.Same(entityEntry.Entity, entityEntry.Collection("Monkeys").EntityEntry.Entity);
    }

    [ConditionalFact]
    public void Can_get_back_reference_generic()
    {
        using var context = new FreezerContext();
        var entity = new Cherry();
        context.Add(entity);

        var entityEntry = context.Entry(entity);
        Assert.Same(entityEntry.Entity, entityEntry.Collection(e => e.Monkeys).EntityEntry.Entity);
    }

    [ConditionalFact]
    public void Can_get_metadata()
    {
        using var context = new FreezerContext();
        var entity = new Cherry();
        context.Add(entity);

        Assert.Equal("Monkeys", context.Entry(entity).Collection("Monkeys").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_metadata_generic()
    {
        using var context = new FreezerContext();
        var entity = new Cherry();
        context.Add(entity);

        Assert.Equal("Monkeys", context.Entry(entity).Collection(e => e.Monkeys).Metadata.Name);
    }

    [ConditionalFact]
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
        Assert.Same(collection.FindEntry(chunky).GetInfrastructure(), context.Entry(chunky).GetInfrastructure());

        collection.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Null(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Null(collection.CurrentValue);
        Assert.Null(collection.FindEntry(chunky));
    }

    [ConditionalFact]
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
        Assert.Same(collection.FindEntry(chunky).GetInfrastructure(), context.Entry(chunky).GetInfrastructure());

        collection.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Null(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Null(collection.CurrentValue);
        Assert.Null(collection.FindEntry(chunky));
    }

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalTheory]
    [InlineData(EntityState.Detached, EntityState.Added)]
    [InlineData(EntityState.Added, EntityState.Added)]
    [InlineData(EntityState.Modified, EntityState.Added)]
    [InlineData(EntityState.Deleted, EntityState.Added)]
    [InlineData(EntityState.Unchanged, EntityState.Added)]
    [InlineData(EntityState.Detached, EntityState.Deleted)]
    [InlineData(EntityState.Added, EntityState.Deleted)]
    [InlineData(EntityState.Modified, EntityState.Deleted)]
    [InlineData(EntityState.Deleted, EntityState.Deleted)]
    [InlineData(EntityState.Unchanged, EntityState.Deleted)]
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

    [ConditionalTheory]
    [InlineData(EntityState.Detached, EntityState.Unchanged)]
    [InlineData(EntityState.Added, EntityState.Unchanged)]
    [InlineData(EntityState.Modified, EntityState.Unchanged)]
    [InlineData(EntityState.Deleted, EntityState.Unchanged)]
    [InlineData(EntityState.Unchanged, EntityState.Unchanged)]
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

    [ConditionalTheory]
    [InlineData(EntityState.Detached, EntityState.Modified)]
    [InlineData(EntityState.Added, EntityState.Modified)]
    [InlineData(EntityState.Modified, EntityState.Modified)]
    [InlineData(EntityState.Deleted, EntityState.Modified)]
    [InlineData(EntityState.Unchanged, EntityState.Modified)]
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

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chunky>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<Cherry>().Property(e => e.Id).ValueGeneratedNever();
        }
    }
}
