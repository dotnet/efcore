// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

public class ReferenceEntryTest
{
    [ConditionalFact]
    public void Can_get_back_reference()
    {
        using var context = new FreezerContext();
        var entity = new Chunky();
        context.Add(entity);

        var entityEntry = context.Entry(entity);
        Assert.Same(entityEntry.Entity, entityEntry.Reference("Garcia").EntityEntry.Entity);
    }

    [ConditionalFact]
    public void Can_get_back_reference_generic()
    {
        using var context = new FreezerContext();
        var entity = new Chunky();
        context.Add(entity);

        var entityEntry = context.Entry(entity);
        Assert.Same(entityEntry.Entity, entityEntry.Reference(e => e.Garcia).EntityEntry.Entity);
    }

    [ConditionalFact]
    public void Can_get_metadata()
    {
        using var context = new FreezerContext();
        var entity = new Chunky();
        context.Add(entity);

        Assert.Equal("Garcia", context.Entry(entity).Reference("Garcia").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_metadata_generic()
    {
        using var context = new FreezerContext();
        var entity = new Chunky();
        context.Add(entity);

        Assert.Equal("Garcia", context.Entry(entity).Reference(e => e.Garcia).Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_and_set_current_value()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky = new Chunky();
        context.AddRange(chunky, cherry);

        var reference = context.Entry(chunky).Reference("Garcia");

        Assert.Null(reference.CurrentValue);

        reference.CurrentValue = cherry;

        Assert.Same(cherry, chunky.Garcia);
        Assert.Same(chunky, cherry.Monkeys.Single());
        Assert.Equal(cherry.Id, chunky.GarciaId);
        Assert.Same(cherry, reference.CurrentValue);
        Assert.Same(reference.TargetEntry.GetInfrastructure(), context.Entry(cherry).GetInfrastructure());

        reference.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Empty(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Null(reference.CurrentValue);
        Assert.Null(reference.TargetEntry);
    }

    [ConditionalFact]
    public void Can_get_and_set_current_value_generic()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky = new Chunky();
        context.AddRange(chunky, cherry);

        var reference = context.Entry(chunky).Reference(e => e.Garcia);

        Assert.Null(reference.CurrentValue);

        reference.CurrentValue = cherry;

        Assert.Same(cherry, chunky.Garcia);
        Assert.Same(chunky, cherry.Monkeys.Single());
        Assert.Equal(cherry.Id, chunky.GarciaId);
        Assert.Same(cherry, reference.CurrentValue);
        Assert.Same(reference.TargetEntry.GetInfrastructure(), context.Entry(cherry).GetInfrastructure());

        reference.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Empty(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Null(reference.CurrentValue);
        Assert.Null(reference.TargetEntry);
    }

    [ConditionalFact]
    public void Can_get_and_set_current_value_not_tracked()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky = new Chunky();

        var reference = context.Entry(chunky).Reference("Garcia");

        Assert.Null(reference.CurrentValue);

        reference.CurrentValue = cherry;

        Assert.Same(cherry, chunky.Garcia);
        Assert.Null(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Same(cherry, reference.CurrentValue);

        reference.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Null(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Null(reference.CurrentValue);
    }

    [ConditionalFact]
    public void Can_get_and_set_current_value_not_tracked_generic()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky = new Chunky();

        var reference = context.Entry(chunky).Reference(e => e.Garcia);

        Assert.Null(reference.CurrentValue);

        reference.CurrentValue = cherry;

        Assert.Same(cherry, chunky.Garcia);
        Assert.Null(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Same(cherry, reference.CurrentValue);

        reference.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Null(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Null(reference.CurrentValue);
    }

    [ConditionalFact]
    public void Can_get_and_set_current_value_start_tracking()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky = new Chunky();
        context.Add(chunky);

        var reference = context.Entry(chunky).Reference("Garcia");

        Assert.Null(reference.CurrentValue);

        reference.CurrentValue = cherry;

        Assert.Same(cherry, chunky.Garcia);
        Assert.Same(chunky, cherry.Monkeys.Single());
        Assert.Equal(cherry.Id, chunky.GarciaId);
        Assert.Same(cherry, reference.CurrentValue);

        Assert.Same(reference.TargetEntry.GetInfrastructure(), context.Entry(cherry).GetInfrastructure());
        Assert.Equal(EntityState.Added, context.Entry(cherry).State);
        Assert.Equal(EntityState.Added, context.Entry(chunky).State);

        reference.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Empty(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Null(reference.CurrentValue);

        Assert.Null(reference.TargetEntry);
        Assert.Equal(EntityState.Added, context.Entry(cherry).State);
        Assert.Equal(EntityState.Added, context.Entry(chunky).State);
    }

    [ConditionalFact]
    public void Can_get_and_set_current_value_start_tracking_generic()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky = new Chunky();
        context.Add(chunky);

        var reference = context.Entry(chunky).Reference(e => e.Garcia);

        Assert.Null(reference.CurrentValue);

        reference.CurrentValue = cherry;

        Assert.Same(cherry, chunky.Garcia);
        Assert.Same(chunky, cherry.Monkeys.Single());
        Assert.Equal(cherry.Id, chunky.GarciaId);
        Assert.Same(cherry, reference.CurrentValue);

        Assert.Same(reference.TargetEntry.GetInfrastructure(), context.Entry(cherry).GetInfrastructure());
        Assert.Equal(EntityState.Added, context.Entry(cherry).State);
        Assert.Equal(EntityState.Added, context.Entry(chunky).State);

        reference.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Empty(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Null(reference.CurrentValue);

        Assert.Null(reference.TargetEntry);
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

        var reference = context.Entry(chunky).Reference("Garcia");

        Assert.Null(reference.CurrentValue);

        reference.CurrentValue = cherry;

        Assert.Same(cherry, chunky.Garcia);
        Assert.Same(chunky, cherry.Monkeys.Single());
        Assert.Equal(cherry.Id, chunky.GarciaId);
        Assert.Same(cherry, reference.CurrentValue);

        Assert.Equal(EntityState.Unchanged, context.Entry(cherry).State);
        Assert.Equal(EntityState.Modified, context.Entry(chunky).State);
        Assert.True(context.Entry(chunky).Property(e => e.GarciaId).IsModified);

        reference.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Empty(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Null(reference.CurrentValue);

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

        var reference = context.Entry(chunky).Reference(e => e.Garcia);

        Assert.Null(reference.CurrentValue);

        reference.CurrentValue = cherry;

        Assert.Same(cherry, chunky.Garcia);
        Assert.Same(chunky, cherry.Monkeys.Single());
        Assert.Equal(cherry.Id, chunky.GarciaId);
        Assert.Same(cherry, reference.CurrentValue);

        Assert.Equal(EntityState.Unchanged, context.Entry(cherry).State);
        Assert.Equal(EntityState.Modified, context.Entry(chunky).State);
        Assert.True(context.Entry(chunky).Property(e => e.GarciaId).IsModified);

        reference.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Empty(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Null(reference.CurrentValue);

        Assert.Equal(EntityState.Unchanged, context.Entry(cherry).State);
        Assert.Equal(EntityState.Modified, context.Entry(chunky).State);
        Assert.True(context.Entry(chunky).Property(e => e.GarciaId).IsModified);
    }

    [ConditionalFact]
    public void IsModified_tracks_state_of_FK_property()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky = new Chunky { Garcia = cherry };
        cherry.Monkeys = new List<Chunky> { chunky };
        context.AttachRange(cherry, chunky);

        var reference = context.Entry(chunky).Reference(e => e.Garcia);

        Assert.False(reference.IsModified);

        chunky.GarciaId = null;
        context.ChangeTracker.DetectChanges();

        Assert.True(reference.IsModified);

        context.Entry(chunky).State = EntityState.Unchanged;

        Assert.False(reference.IsModified);
    }

    [ConditionalFact]
    public void IsModified_can_set_fk_to_modified()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky = new Chunky { Garcia = cherry };
        cherry.Monkeys = new List<Chunky> { chunky };
        context.AttachRange(cherry, chunky);

        var entityEntry = context.Entry(chunky);
        var reference = entityEntry.Reference(e => e.Garcia);

        Assert.False(reference.IsModified);

        reference.IsModified = true;

        Assert.True(reference.IsModified);
        Assert.True(entityEntry.Property(e => e.GarciaId).IsModified);

        reference.IsModified = false;

        Assert.False(reference.IsModified);
        Assert.False(entityEntry.Property(e => e.GarciaId).IsModified);
        Assert.Equal(EntityState.Unchanged, entityEntry.State);
    }

    [ConditionalFact]
    public void IsModified_can_reject_changes_to_an_fk()
    {
        using var context = new FreezerContext();
        var cherry = new Cherry();
        var chunky = new Chunky { Garcia = cherry };
        cherry.Monkeys = new List<Chunky> { chunky };
        context.AttachRange(cherry, chunky);

        var entityEntry = context.Entry(chunky);
        var reference = entityEntry.Reference(e => e.Garcia);
        var originalValue = entityEntry.Property(e => e.GarciaId).CurrentValue;

        Assert.False(reference.IsModified);

        entityEntry.Property(e => e.GarciaId).CurrentValue = 77;

        Assert.True(reference.IsModified);
        Assert.True(entityEntry.Property(e => e.GarciaId).IsModified);
        Assert.Equal(77, entityEntry.Property(e => e.GarciaId).CurrentValue);
        Assert.Equal(originalValue, entityEntry.Property(e => e.GarciaId).OriginalValue);

        reference.IsModified = false;

        Assert.False(reference.IsModified);
        Assert.False(entityEntry.Property(e => e.GarciaId).IsModified);
        Assert.Equal(originalValue, entityEntry.Property(e => e.GarciaId).CurrentValue);
        Assert.Equal(originalValue, entityEntry.Property(e => e.GarciaId).OriginalValue);
        Assert.Equal(EntityState.Unchanged, entityEntry.State);
    }

    [ConditionalFact]
    public void IsModified_tracks_state_of_FK_property_principal()
    {
        using var context = new FreezerContext();
        var half = new Half();
        var chunky = new Chunky { Baked = half };
        half.Monkey = chunky;
        context.AttachRange(chunky, half);

        var reference = context.Entry(chunky).Reference(e => e.Baked);

        Assert.False(reference.IsModified);

        context.Entry(half).State = EntityState.Modified;

        Assert.True(reference.IsModified);

        context.Entry(half).State = EntityState.Unchanged;

        Assert.False(reference.IsModified);
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
    public void IsModified_can_set_fk_to_modified_principal_with_Added_or_Deleted_dependent(
        EntityState principalState,
        EntityState dependentState)
    {
        using var context = new FreezerContext();
        var half = new Half();
        var chunky = new Chunky { Id = 1, Baked = half };
        half.Monkey = chunky;

        context.Entry(chunky).State = principalState;
        context.Entry(half).State = dependentState;

        var reference = context.Entry(chunky).Reference(e => e.Baked);

        Assert.True(reference.IsModified);

        reference.IsModified = true;

        Assert.True(reference.IsModified);
        Assert.False(context.Entry(half).Property(e => e.MonkeyId).IsModified);

        reference.IsModified = false;

        Assert.True(reference.IsModified);
        Assert.False(context.Entry(half).Property(e => e.MonkeyId).IsModified);
        Assert.Equal(dependentState, context.Entry(half).State);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Detached, EntityState.Unchanged)]
    [InlineData(EntityState.Added, EntityState.Unchanged)]
    [InlineData(EntityState.Modified, EntityState.Unchanged)]
    [InlineData(EntityState.Deleted, EntityState.Unchanged)]
    [InlineData(EntityState.Unchanged, EntityState.Unchanged)]
    public void IsModified_can_set_fk_to_modified_principal_with_Unchanged_dependent(
        EntityState principalState,
        EntityState dependentState)
    {
        using var context = new FreezerContext();
        var half = new Half();
        var chunky = new Chunky { Id = 1, Baked = half };
        half.Monkey = chunky;

        context.Entry(chunky).State = principalState;
        context.Entry(half).State = dependentState;

        var reference = context.Entry(chunky).Reference(e => e.Baked);

        Assert.False(reference.IsModified);

        reference.IsModified = true;

        Assert.True(reference.IsModified);
        Assert.True(context.Entry(half).Property(e => e.MonkeyId).IsModified);

        reference.IsModified = false;

        Assert.False(reference.IsModified);
        Assert.False(context.Entry(half).Property(e => e.MonkeyId).IsModified);
        Assert.Equal(dependentState, context.Entry(half).State);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Detached, EntityState.Modified)]
    [InlineData(EntityState.Added, EntityState.Modified)]
    [InlineData(EntityState.Modified, EntityState.Modified)]
    [InlineData(EntityState.Deleted, EntityState.Modified)]
    [InlineData(EntityState.Unchanged, EntityState.Modified)]
    public void IsModified_can_set_fk_to_modified_principal_with_Modified_dependent(
        EntityState principalState,
        EntityState dependentState)
    {
        using var context = new FreezerContext();
        var half = new Half { Id = 7 };
        var chunky = new Chunky { Id = 1, Baked = half };
        half.Monkey = chunky;

        context.Attach(half);

        context.Entry(chunky).State = principalState;
        context.Entry(half).State = dependentState;

        var reference = context.Entry(chunky).Reference(e => e.Baked);

        Assert.True(reference.IsModified);

        reference.IsModified = false;

        Assert.False(reference.IsModified);
        Assert.False(context.Entry(half).Property(e => e.MonkeyId).IsModified);

        reference.IsModified = true;

        Assert.True(reference.IsModified);
        Assert.True(context.Entry(half).Property(e => e.MonkeyId).IsModified);
        Assert.Equal(dependentState, context.Entry(half).State);
    }

    private class Chunky
    {
        public int Monkey { get; set; }
        public int Id { get; set; }

        public int? GarciaId { get; set; }
        public Cherry Garcia { get; set; }

        public Half Baked { get; set; }
    }

    private class Half
    {
        public int Baked { get; set; }
        public int Id { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int? MonkeyId { get; set; }
        public Chunky Monkey { get; set; }
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
