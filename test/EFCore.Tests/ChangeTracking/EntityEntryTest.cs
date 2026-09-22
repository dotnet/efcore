// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
#pragma warning disable CS0414 // Field is assigned but its value is never used

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

public class EntityEntryTest
{
    [ConditionalFact]
    public void Non_store_generated_key_is_always_set()
    {
        using var context = new KeySetContext();
        Assert.True(context.Entry(new NotStoreGenerated()).IsKeySet);
        Assert.True(context.Entry(new NotStoreGenerated { Id = 1 }).IsKeySet);
    }

    [ConditionalFact]
    public void Non_store_generated_composite_key_is_always_set()
    {
        using var context = new KeySetContext();
        Assert.True(context.Entry(new CompositeNotStoreGenerated()).IsKeySet);
        Assert.True(context.Entry(new CompositeNotStoreGenerated { Id1 = 1 }).IsKeySet);
        Assert.True(context.Entry(new CompositeNotStoreGenerated { Id2 = true }).IsKeySet);
        Assert.True(context.Entry(new CompositeNotStoreGenerated { Id1 = 1, Id2 = true }).IsKeySet);
    }

    [ConditionalFact]
    public void Store_generated_key_is_set_only_if_non_default_value()
    {
        using var context = new KeySetContext();
        Assert.False(context.Entry(new StoreGenerated()).IsKeySet);
        Assert.True(context.Entry(new StoreGenerated { Id = 1 }).IsKeySet);
    }

    [ConditionalFact]
    public void Store_generated_key_is_set_only_if_non_sentinel_value()
    {
        using var context = new KeySetContext();
        Assert.False(context.Entry(new StoreGeneratedWithSentinel { Id = 667 }).IsKeySet);
        Assert.True(context.Entry(new StoreGeneratedWithSentinel { Id = 1 }).IsKeySet);
        Assert.True(context.Entry(new StoreGeneratedWithSentinel()).IsKeySet);
    }

    [ConditionalFact]
    public void Composite_store_generated_key_is_set_only_if_non_default_value_in_store_generated_part()
    {
        using var context = new KeySetContext();
        Assert.False(context.Entry(new CompositeStoreGenerated()).IsKeySet);
        Assert.False(context.Entry(new CompositeStoreGenerated { Id1 = 1 }).IsKeySet);
        Assert.True(context.Entry(new CompositeStoreGenerated { Id2 = true }).IsKeySet);
        Assert.True(context.Entry(new CompositeStoreGenerated { Id1 = 1, Id2 = true }).IsKeySet);
    }

    [ConditionalFact]
    public void Composite_store_generated_key_is_set_only_if_non_sentinel_value_in_store_generated_part()
    {
        using var context = new KeySetContext();
        Assert.False(context.Entry(new CompositeStoreGeneratedWithSentinel { Id2 = true }).IsKeySet);
        Assert.False(context.Entry(new CompositeStoreGeneratedWithSentinel { Id1 = 1, Id2 = true }).IsKeySet);
        Assert.True(context.Entry(new CompositeStoreGeneratedWithSentinel { Id2 = false }).IsKeySet);
        Assert.True(context.Entry(new CompositeStoreGeneratedWithSentinel { Id1 = 1, Id2 = false }).IsKeySet);
        Assert.True(context.Entry(new CompositeStoreGeneratedWithSentinel()).IsKeySet);
        Assert.True(context.Entry(new CompositeStoreGeneratedWithSentinel { Id1 = 1 }).IsKeySet);
    }

    [ConditionalFact]
    public void Primary_key_that_is_also_foreign_key_is_set_only_if_non_default_value()
    {
        using var context = new KeySetContext();
        Assert.False(context.Entry(new Dependent()).IsKeySet);
        Assert.True(context.Entry(new Dependent { Id = 1 }).IsKeySet);
    }

    [ConditionalFact]
    public void Primary_key_that_is_also_foreign_key_is_set_only_if_non_sentinel_value()
    {
        using var context = new KeySetContext();
        Assert.False(context.Entry(new DependentWithSentinel { Id = 667 }).IsKeySet);
        Assert.True(context.Entry(new DependentWithSentinel { Id = 1 }).IsKeySet);
        Assert.True(context.Entry(new DependentWithSentinel()).IsKeySet);
    }

    private class StoreGenerated
    {
        public int Id { get; set; }

        public Dependent? Dependent { get; set; }
    }

    private class StoreGeneratedWithSentinel
    {
        public int Id { get; set; }

        public DependentWithSentinel? Dependent { get; set; }
    }

    private class NotStoreGenerated
    {
        public int Id { get; set; }
    }

    private class CompositeStoreGenerated
    {
        public int Id1 { get; set; }
        public bool Id2 { get; set; }
    }

    private class CompositeStoreGeneratedWithSentinel
    {
        public int Id1 { get; set; }
        public bool Id2 { get; set; }
    }

    private class CompositeNotStoreGenerated
    {
        public int Id1 { get; set; }
        public bool Id2 { get; set; }
    }

    private class Dependent
    {
        public int Id { get; set; }

        public StoreGenerated? Principal { get; set; }
    }

    private class DependentWithSentinel
    {
        public int Id { get; set; }

        public StoreGeneratedWithSentinel? Principal { get; set; }
    }

    private class KeySetContext : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(nameof(KeySetContext));

        public DbSet<StoreGenerated> StoreGenerated
            => Set<StoreGenerated>();

        public DbSet<StoreGeneratedWithSentinel> StoreGeneratedWithSentinel
            => Set<StoreGeneratedWithSentinel>();

        public DbSet<NotStoreGenerated> NotStoreGenerated
            => Set<NotStoreGenerated>();

        public DbSet<CompositeStoreGenerated> CompositeStoreGenerated
            => Set<CompositeStoreGenerated>();

        public DbSet<CompositeStoreGeneratedWithSentinel> CompositeStoreGeneratedWithSentinel
            => Set<CompositeStoreGeneratedWithSentinel>();

        public DbSet<CompositeNotStoreGenerated> CompositeNotStoreGenerated
            => Set<CompositeNotStoreGenerated>();

        public DbSet<Dependent> Dependent
            => Set<Dependent>();

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StoreGenerated>()
                .HasOne(e => e.Dependent)
                .WithOne(e => e.Principal)
                .HasForeignKey<Dependent>(e => e.Id);

            modelBuilder.Entity<StoreGeneratedWithSentinel>(
                b =>
                {
                    b.Property(e => e.Id).HasSentinel(667);
                    b.HasOne(e => e.Dependent)
                        .WithOne(e => e.Principal)
                        .HasForeignKey<DependentWithSentinel>(e => e.Id);
                });

            modelBuilder.Entity<DependentWithSentinel>().Property(e => e.Id).HasSentinel(667);

            modelBuilder.Entity<NotStoreGenerated>().Property(e => e.Id).ValueGeneratedNever();

            modelBuilder.Entity<CompositeNotStoreGenerated>().HasKey(
                e => new { e.Id1, e.Id2 });

            modelBuilder.Entity<CompositeStoreGenerated>(
                b =>
                {
                    b.HasKey(e => new { e.Id1, e.Id2 });
                    b.Property(e => e.Id2).ValueGeneratedOnAdd();
                });

            modelBuilder.Entity<CompositeStoreGeneratedWithSentinel>(
                b =>
                {
                    b.HasKey(e => new { e.Id1, e.Id2 });
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasSentinel(true);
                });
        }
    }

    [ConditionalFact]
    public void Detached_entities_are_not_returned_from_the_change_tracker()
    {
        using var context = new FreezerContext();
        var entity = CreateChunky(808);
        context.Attach(entity);

        Assert.Single(context.ChangeTracker.Entries());

        context.Entry(entity).State = EntityState.Detached;

        Assert.Empty(context.ChangeTracker.Entries());

        context.ChangeTracker.DetectChanges();

        Assert.Empty(context.ChangeTracker.Entries());

        context.Entry(entity);

        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalFact]
    public void Can_obtain_entity_instance()
    {
        using var context = new FreezerContext();
        var entity = CreateChunky();
        context.Add(entity);

        Assert.Same(entity, context.Entry(entity).Entity);
        Assert.Same(entity, context.Entry((object)entity).Entity);
    }

    [ConditionalFact]
    public void Can_obtain_context()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Same(context, context.Entry(entity).Context);
        Assert.Same(context, context.Entry((object)entity).Context);
    }

    [ConditionalFact]
    public void Can_obtain_underlying_state_entry()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;
        var entry = context.GetService<IStateManager>().GetOrCreateEntry(entity);

        Assert.Same(entry, context.Entry(entity).GetInfrastructure());
        Assert.Same(entry, context.Entry((object)entity).GetInfrastructure());
    }

    [ConditionalFact]
    public void Can_get_metadata()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;
        var entityType = context.Model.FindEntityType(typeof(Chunky));

        Assert.Same(entityType, context.Entry(entity).Metadata);
        Assert.Same(entityType, context.Entry((object)entity).Metadata);
    }

    [ConditionalFact]
    public void Can_get_and_change_state()
    {
        using var context = new FreezerContext();
        var entity = CreateChunky();
        var entry = context.Add(entity).GetInfrastructure();

        context.Entry(entity).State = EntityState.Modified;
        Assert.Equal(EntityState.Modified, entry.EntityState);
        Assert.Equal(EntityState.Modified, context.Entry(entity).State);

        context.Entry((object)entity).State = EntityState.Unchanged;
        Assert.Equal(EntityState.Unchanged, entry.EntityState);
        Assert.Equal(EntityState.Unchanged, context.Entry((object)entity).State);
    }

    [ConditionalFact]
    public void Cannot_set_invalid_state()
    {
        using var context = new FreezerContext();
        var entity = CreateChunky();

        Assert.Equal(
            CoreStrings.InvalidEnumValue("-1", "value", typeof(EntityState).FullName),
            Assert.Throws<ArgumentException>(() => context.Entry(entity).State = (EntityState)(-1)).Message);

        Assert.Equal(
            CoreStrings.InvalidEnumValue("5", "value", typeof(EntityState).FullName),
            Assert.Throws<ArgumentException>(() => context.Entry(entity).State = (EntityState)(5)).Message);
    }

    [ConditionalFact]
    public void Can_use_entry_to_change_state_to_Added()
    {
        ChangeStateOnEntry(EntityState.Detached, EntityState.Added);
        ChangeStateOnEntry(EntityState.Unchanged, EntityState.Added);
        ChangeStateOnEntry(EntityState.Deleted, EntityState.Added);
        ChangeStateOnEntry(EntityState.Modified, EntityState.Added);
        ChangeStateOnEntry(EntityState.Added, EntityState.Added);
    }

    [ConditionalFact]
    public void Can_use_entry_to_change_state_to_Unchanged()
    {
        ChangeStateOnEntry(EntityState.Detached, EntityState.Unchanged);
        ChangeStateOnEntry(EntityState.Unchanged, EntityState.Unchanged);
        ChangeStateOnEntry(EntityState.Deleted, EntityState.Unchanged);
        ChangeStateOnEntry(EntityState.Modified, EntityState.Unchanged);
        ChangeStateOnEntry(EntityState.Added, EntityState.Unchanged);
    }

    [ConditionalFact]
    public void Can_use_entry_to_change_state_to_Modified()
    {
        ChangeStateOnEntry(EntityState.Detached, EntityState.Modified);
        ChangeStateOnEntry(EntityState.Unchanged, EntityState.Modified);
        ChangeStateOnEntry(EntityState.Deleted, EntityState.Modified);
        ChangeStateOnEntry(EntityState.Modified, EntityState.Modified);
        ChangeStateOnEntry(EntityState.Added, EntityState.Modified);
    }

    [ConditionalFact]
    public void Can_use_entry_to_change_state_to_Deleted()
    {
        ChangeStateOnEntry(EntityState.Detached, EntityState.Deleted);
        ChangeStateOnEntry(EntityState.Unchanged, EntityState.Deleted);
        ChangeStateOnEntry(EntityState.Deleted, EntityState.Deleted);
        ChangeStateOnEntry(EntityState.Modified, EntityState.Deleted);
        ChangeStateOnEntry(EntityState.Added, EntityState.Deleted);
    }

    [ConditionalFact]
    public void Can_use_entry_to_change_state_to_Unknown()
    {
        ChangeStateOnEntry(EntityState.Detached, EntityState.Detached);
        ChangeStateOnEntry(EntityState.Unchanged, EntityState.Detached);
        ChangeStateOnEntry(EntityState.Deleted, EntityState.Detached);
        ChangeStateOnEntry(EntityState.Modified, EntityState.Detached);
        ChangeStateOnEntry(EntityState.Added, EntityState.Detached);
    }

    private void ChangeStateOnEntry(EntityState initialState, EntityState expectedState)
    {
        using var context = new FreezerContext();
        var entry = context.Add(CreateChunky());

        entry.State = initialState;
        entry.State = expectedState;

        Assert.Equal(expectedState, entry.State);
    }

    [ConditionalFact]
    public void Can_get_property_entry_by_name()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Equal("Monkey", context.Entry(entity).Property("Monkey").Metadata.Name);
        Assert.Equal("Monkey", context.Entry((object)entity).Property("Monkey").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_generic_property_entry_by_name()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Equal("Monkey", context.Entry(entity).Property<int>("Monkey").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_property_entry_by_IProperty()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;
        var property = context.Entry(entity).Metadata.FindProperty("Monkey")!;

        Assert.Same(property, context.Entry(entity).Property(property).Metadata);
        Assert.Same(property, context.Entry((object)entity).Property(property).Metadata);
    }

    [ConditionalFact]
    public void Can_get_generic_property_entry_by_IProperty()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;
        var property = context.Entry(entity).Metadata.FindProperty("Monkey")!;

        Assert.Same(property, context.Entry(entity).Property<int>(property).Metadata);
    }

    [ConditionalFact]
    public void Throws_when_wrong_generic_type_is_used_while_getting_property_entry_by_name()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Equal(
            CoreStrings.WrongGenericPropertyType("Monkey", entity.GetType().ShortDisplayName(), "int", "string"),
            Assert.Throws<ArgumentException>(() => context.Entry(entity).Property<string>("Monkey")).Message);
    }

    [ConditionalFact]
    public void Can_get_generic_property_entry_by_lambda()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Equal("Monkey", context.Entry(entity).Property(e => e.Monkey).Metadata.Name);
    }

    [ConditionalFact]
    public void Throws_when_wrong_property_name_is_used_while_getting_property_entry_by_name()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Equal(
            CoreStrings.PropertyNotFound("Chimp", entity.GetType().Name),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Property("Chimp").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.PropertyNotFound("Chimp", entity.GetType().Name),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Property("Chimp").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.PropertyNotFound("Chimp", entity.GetType().Name),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Property<int>("Chimp").Metadata.Name).Message);
    }

    [ConditionalFact]
    public void Can_get_reference_entry_by_name()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Equal("Garcia", context.Entry(entity).Reference("Garcia").Metadata.Name);
        Assert.Equal("Garcia", context.Entry((object)entity).Reference("Garcia").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_generic_reference_entry_by_name()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Equal("Garcia", context.Entry(entity).Reference<Cherry>("Garcia").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_generic_reference_entry_by_lambda()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Equal("Garcia", context.Entry(entity).Reference(e => e.Garcia).Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_reference_entry_by_INavigationBase()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;
        var navigationBase = (INavigationBase)context.Entry(entity).Metadata.FindNavigation("Garcia")!;

        Assert.Same(navigationBase, context.Entry(entity).Reference(navigationBase).Metadata);
        Assert.Same(navigationBase, context.Entry((object)entity).Reference(navigationBase).Metadata);
    }

    [ConditionalFact]
    public void Can_get_generic_reference_entry_by_INavigationBase()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;
        var navigationBase = (INavigationBase)context.Entry(entity).Metadata.FindNavigation("Garcia")!;

        Assert.Same(navigationBase, context.Entry(entity).Reference<Cherry>(navigationBase).Metadata);
    }

    [ConditionalFact]
    public void Throws_when_wrong_reference_name_is_used_while_getting_property_entry_by_name()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Equal(
            CoreStrings.PropertyNotFound("Chimp", entity.GetType().Name),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference("Chimp").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.PropertyNotFound("Chimp", entity.GetType().Name),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Reference("Chimp").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.PropertyNotFound("Chimp", entity.GetType().Name),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference<Cherry>("Chimp").Metadata.Name).Message);
    }

    [ConditionalFact]
    public void Throws_when_accessing_property_as_reference()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Monkey", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference("Monkey").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Monkey", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Reference("Monkey").Metadata.Name)
                .Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Monkey", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference<Random>("Monkey").Metadata.Name)
                .Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Nonkey", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference(e => e.Nonkey).Metadata.Name).Message);
    }

    [ConditionalFact]
    public void Throws_when_accessing_collection_as_reference()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;

        Assert.Equal(
            CoreStrings.ReferenceIsCollection(
                "Monkeys", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference("Monkeys")).Message);

        Assert.Equal(
            CoreStrings.ReferenceIsCollection(
                "Monkeys", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection)),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Reference("Monkeys")).Message);

        Assert.Equal(
            CoreStrings.ReferenceIsCollection(
                "Monkeys", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference<Random>("Monkeys")).Message);

        Assert.Equal(
            CoreStrings.ReferenceIsCollection(
                "Monkeys", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference(e => e.Monkeys)).Message);

        var navigationBase = context.Entry(entity).Navigation("Monkeys").Metadata;

        Assert.Equal(
            CoreStrings.ReferenceIsCollection(
                "Monkeys", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference(navigationBase)).Message);

        Assert.Equal(
            CoreStrings.ReferenceIsCollection(
                "Monkeys", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection)),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Reference(navigationBase)).Message);

        Assert.Equal(
            CoreStrings.ReferenceIsCollection(
                "Monkeys", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference<Random>(navigationBase)).Message);
    }

    [ConditionalFact]
    public void Can_get_collection_entry_by_name()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;

        Assert.Equal("Monkeys", context.Entry(entity).Collection("Monkeys").Metadata.Name);
        Assert.Equal("Monkeys", context.Entry((object)entity).Collection("Monkeys").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_generic_collection_entry_by_name()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;

        Assert.Equal("Monkeys", context.Entry(entity).Collection<Chunky>("Monkeys").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_generic_collection_entry_by_lambda()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;

        Assert.Equal("Monkeys", context.Entry(entity).Collection(e => e.Monkeys!).Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_collection_entry_by_INavigationBase()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;
        var navigationBase = (INavigationBase)context.Entry(entity).Metadata.FindNavigation("Monkeys")!;

        Assert.Same(navigationBase, context.Entry(entity).Collection(navigationBase).Metadata);
        Assert.Same(navigationBase, context.Entry((object)entity).Collection(navigationBase).Metadata);
    }

    [ConditionalFact]
    public void Can_get_generic_collection_entry_by_INavigationBase()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;
        var navigationBase = (INavigationBase)context.Entry(entity).Metadata.FindNavigation("Monkeys")!;

        Assert.Same(navigationBase, context.Entry(entity).Collection<Chunky>(navigationBase).Metadata);
    }

    [ConditionalFact]
    public void Throws_when_wrong_collection_name_is_used_while_getting_property_entry_by_name()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;

        Assert.Equal(
            CoreStrings.PropertyNotFound("Chimp", entity.GetType().Name),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Collection("Chimp").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.PropertyNotFound("Chimp", entity.GetType().Name),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Collection("Chimp").Metadata.Name)
                .Message);
        Assert.Equal(
            CoreStrings.PropertyNotFound("Chimp", entity.GetType().Name),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Collection<Cherry>("Chimp").Metadata.Name)
                .Message);
    }

    [ConditionalFact]
    public void Throws_when_accessing_property_as_collection()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;

        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Garcia", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Collection("Garcia").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Garcia", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Collection("Garcia").Metadata.Name)
                .Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Garcia", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Collection<Random>("Garcia").Metadata.Name)
                .Message);
    }

    [ConditionalFact]
    public void Throws_when_accessing_reference_as_collection()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Equal(
            CoreStrings.CollectionIsReference(
                "Garcia", entity.GetType().Name,
                nameof(EntityEntry.Collection), nameof(EntityEntry.Reference)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Collection("Garcia")).Message);

        Assert.Equal(
            CoreStrings.CollectionIsReference(
                "Garcia", entity.GetType().Name,
                nameof(EntityEntry.Collection), nameof(EntityEntry.Reference)),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Collection("Garcia")).Message);

        Assert.Equal(
            CoreStrings.CollectionIsReference(
                "Garcia", entity.GetType().Name,
                nameof(EntityEntry.Collection), nameof(EntityEntry.Reference)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Collection<Cherry>("Garcia")).Message);

        var navigationBase = context.Entry(entity).Navigation("Garcia").Metadata;

        Assert.Equal(
            CoreStrings.CollectionIsReference(
                "Garcia", entity.GetType().Name,
                nameof(EntityEntry.Collection), nameof(EntityEntry.Reference)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Collection(navigationBase)).Message);

        Assert.Equal(
            CoreStrings.CollectionIsReference(
                "Garcia", entity.GetType().Name,
                nameof(EntityEntry.Collection), nameof(EntityEntry.Reference)),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Collection(navigationBase)).Message);

        Assert.Equal(
            CoreStrings.CollectionIsReference(
                "Garcia", entity.GetType().Name,
                nameof(EntityEntry.Collection), nameof(EntityEntry.Reference)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Collection<Cherry>(navigationBase)).Message);
    }

    [ConditionalFact]
    public void Can_get_property_entry_by_name_using_Member()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        var entry = context.Entry(entity).Member("Monkey");
        Assert.Equal("Monkey", entry.Metadata.Name);
        Assert.IsType<PropertyEntry>(entry);

        entry = context.Entry((object)entity).Member("Monkey");
        Assert.Equal("Monkey", entry.Metadata.Name);
        Assert.IsType<PropertyEntry>(entry);
    }

    [ConditionalFact]
    public void Throws_when_wrong_property_name_is_used_while_getting_property_entry_by_name_using_Member()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Equal(
            CoreStrings.PropertyNotFound("Chimp", entity.GetType().Name),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Member("Chimp").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.PropertyNotFound("Chimp", entity.GetType().Name),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Member("Chimp").Metadata.Name).Message);
    }

    [ConditionalFact]
    public void Can_get_reference_entry_by_name_using_Member()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        var entry = context.Entry(entity).Member("Garcia");
        Assert.Equal("Garcia", entry.Metadata.Name);
        Assert.IsType<ReferenceEntry>(entry);

        entry = context.Entry((object)entity).Member("Garcia");
        Assert.Equal("Garcia", entry.Metadata.Name);
        Assert.IsType<ReferenceEntry>(entry);
    }

    [ConditionalFact]
    public void Can_get_collection_entry_by_name_using_Member()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;

        var entry = context.Entry(entity).Member("Monkeys");
        Assert.Equal("Monkeys", entry.Metadata.Name);
        Assert.IsType<CollectionEntry>(entry);

        entry = context.Entry((object)entity).Member("Monkeys");
        Assert.Equal("Monkeys", entry.Metadata.Name);
        Assert.IsType<CollectionEntry>(entry);
    }

    [ConditionalFact]
    public void Can_get_property_entry_by_IPropertyBase_using_Member()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;
        var propertyBase = (IPropertyBase)context.Entry(entity).Metadata.FindProperty("Monkey")!;

        var entry = context.Entry(entity).Member(propertyBase);
        Assert.Same(propertyBase, entry.Metadata);
        Assert.IsType<PropertyEntry>(entry);

        entry = context.Entry((object)entity).Member(propertyBase);
        Assert.Same(propertyBase, entry.Metadata);
        Assert.IsType<PropertyEntry>(entry);
    }

    [ConditionalFact]
    public void Can_get_reference_entry_by_IPropertyBase_using_Member()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;
        var propertyBase = (IPropertyBase)context.Entry(entity).Metadata.FindNavigation("Garcia")!;

        var entry = context.Entry(entity).Member(propertyBase);
        Assert.Same(propertyBase, entry.Metadata);
        Assert.IsType<ReferenceEntry>(entry);

        entry = context.Entry((object)entity).Member(propertyBase);
        Assert.Same(propertyBase, entry.Metadata);
        Assert.IsType<ReferenceEntry>(entry);
    }

    [ConditionalFact]
    public void Can_get_collection_entry_by_IPropertyBase_using_Member()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;
        var propertyBase = (IPropertyBase)context.Entry(entity).Metadata.FindNavigation("Monkeys")!;

        var entry = context.Entry(entity).Member(propertyBase);
        Assert.Same(propertyBase, entry.Metadata);
        Assert.IsType<CollectionEntry>(entry);

        entry = context.Entry((object)entity).Member(propertyBase);
        Assert.Same(propertyBase, entry.Metadata);
        Assert.IsType<CollectionEntry>(entry);
    }

    [ConditionalFact]
    public void Throws_when_wrong_property_name_is_used_while_getting_property_entry_by_name_using_Navigation()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Equal(
            CoreStrings.PropertyNotFound("Chimp", entity.GetType().Name),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Navigation("Chimp").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.PropertyNotFound("Chimp", entity.GetType().Name),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Navigation("Chimp").Metadata.Name)
                .Message);
    }

    [ConditionalFact]
    public void Can_get_reference_entry_by_name_using_Navigation()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        var entry = context.Entry(entity).Navigation("Garcia");
        Assert.Equal("Garcia", entry.Metadata.Name);
        Assert.IsType<ReferenceEntry>(entry);

        entry = context.Entry((object)entity).Navigation("Garcia");
        Assert.Equal("Garcia", entry.Metadata.Name);
        Assert.IsType<ReferenceEntry>(entry);
    }

    [ConditionalFact]
    public void Can_get_collection_entry_by_name_using_Navigation()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;

        var entry = context.Entry(entity).Navigation("Monkeys");
        Assert.Equal("Monkeys", entry.Metadata.Name);
        Assert.IsType<CollectionEntry>(entry);

        entry = context.Entry((object)entity).Navigation("Monkeys");
        Assert.Equal("Monkeys", entry.Metadata.Name);
        Assert.IsType<CollectionEntry>(entry);
    }

    [ConditionalFact]
    public void Can_get_reference_entry_by_INavigationBase_using_Navigation()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;
        var navigationBase = (INavigationBase)context.Entry(entity).Metadata.FindNavigation("Garcia")!;

        var entry = context.Entry(entity).Navigation(navigationBase);
        Assert.Same(navigationBase, entry.Metadata);
        Assert.IsType<ReferenceEntry>(entry);

        entry = context.Entry((object)entity).Navigation(navigationBase);
        Assert.Same(navigationBase, entry.Metadata);
        Assert.IsType<ReferenceEntry>(entry);
    }

    [ConditionalFact]
    public void Can_get_collection_entry_by_INavigationBase_using_Navigation()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;
        var navigationBase = (INavigationBase)context.Entry(entity).Metadata.FindNavigation("Monkeys")!;

        var entry = context.Entry(entity).Navigation(navigationBase);
        Assert.Same(navigationBase, entry.Metadata);
        Assert.IsType<CollectionEntry>(entry);

        entry = context.Entry((object)entity).Navigation(navigationBase);
        Assert.Same(navigationBase, entry.Metadata);
        Assert.IsType<CollectionEntry>(entry);
    }

    [ConditionalFact]
    public void Throws_when_accessing_property_as_navigation()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Monkey", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Navigation("Monkey").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Monkey", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Navigation("Monkey").Metadata.Name)
                .Message);
    }

    [ConditionalFact]
    public void Can_get_all_modified_properties()
    {
        using var context = new FreezerContext();
        var entity = context.Attach(CreateChunky()).Entity;

        var modified = context.Entry(entity).Properties
            .Where(e => e.IsModified).Select(e => e.Metadata.Name).ToList();

        Assert.Empty(modified);

        entity.Nonkey = "Blue";
        entity.GarciaId = 77;

        context.ChangeTracker.DetectChanges();

        modified = context.Entry(entity).Properties
            .Where(e => e.IsModified).Select(e => e.Metadata.Name).ToList();

        Assert.Equal(
            ["GarciaId", "Nonkey"], modified);
    }

    [ConditionalFact]
    public void Can_get_all_member_entries()
    {
        using var context = new FreezerContext();
        Assert.Equal(
            [
                "Id",
                "GarciaId",
                "Monkey",
                "Nonkey",
                "Culture",
                "Milk",
                "Garcia"
            ],
            context.Attach(CreateChunky()).Members.Select(e => e.Metadata.Name).ToList());

        Assert.Equal(
            [
                "Id",
                "Garcia",
                "Culture",
                "Milk",
                "Baked",
                "Monkeys"
            ],
            context.Attach(CreateCherry()).Members.Select(e => e.Metadata.Name).ToList());

        Assert.Equal(
            [
                "Id",
                "Baked",
                "GarciaId",
                "Garcia"
            ],
            context.Attach(new Half()).Members.Select(e => e.Metadata.Name).ToList());
    }

    [ConditionalFact]
    public void Can_get_all_property_entries()
    {
        using var context = new FreezerContext();
        Assert.Equal(
            [
                "Id",
                "GarciaId",
                "Monkey",
                "Nonkey"
            ],
            context.Attach(CreateChunky()).Properties.Select(e => e.Metadata.Name).ToList());

        Assert.Equal(
            ["Id", "Garcia"],
            context.Attach(CreateCherry()).Properties.Select(e => e.Metadata.Name).ToList());

        Assert.Equal(
            [
                "Id",
                "Baked",
                "GarciaId"
            ],
            context.Attach(new Half()).Properties.Select(e => e.Metadata.Name).ToList());
    }

    [ConditionalFact]
    public void Can_get_all_navigation_entries()
    {
        using var context = new FreezerContext();
        Assert.Equal(
            ["Garcia"],
            context.Attach(CreateChunky()).Navigations.Select(e => e.Metadata.Name).ToList());

        Assert.Equal(
            ["Baked", "Monkeys"],
            context.Attach(CreateCherry()).Navigations.Select(e => e.Metadata.Name).ToList());

        Assert.Equal(
            ["Garcia"],
            context.Attach(new Half()).Navigations.Select(e => e.Metadata.Name).ToList());
    }

    [ConditionalFact]
    public void Can_get_all_reference_entries()
    {
        using var context = new FreezerContext();
        Assert.Equal(
            ["Garcia"],
            context.Attach(CreateChunky()).References.Select(e => e.Metadata.Name).ToList());

        Assert.Equal(
            ["Baked"],
            context.Attach(CreateCherry()).References.Select(e => e.Metadata.Name).ToList());

        Assert.Equal(
            ["Garcia"],
            context.Attach(new Half()).References.Select(e => e.Metadata.Name).ToList());
    }

    [ConditionalFact]
    public void Can_get_all_collection_entries()
    {
        using var context = new FreezerContext();
        Assert.Empty(context.Attach(CreateChunky()).Collections.Select(e => e.Metadata.Name).ToList());

        Assert.Equal(
            ["Monkeys"],
            context.Attach(CreateCherry()).Collections.Select(e => e.Metadata.Name).ToList());

        Assert.Empty(context.Attach(new Half()).Collections.Select(e => e.Metadata.Name).ToList());
    }

    [ConditionalFact]
    public void Can_get_complex_property_entry_by_name()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Equal("Culture", context.Entry(entity).ComplexProperty("Culture").Metadata.Name);
        Assert.Equal("Milk", context.Entry(entity).ComplexProperty("Milk").Metadata.Name);
        Assert.Equal("Culture", context.Entry((object)entity).ComplexProperty("Culture").Metadata.Name);
        Assert.Equal("Milk", context.Entry((object)entity).ComplexProperty("Milk").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_generic_complex_property_entry_by_name()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Equal("Culture", context.Entry(entity).ComplexProperty<Culture>("Culture").Metadata.Name);
        Assert.Equal("Milk", context.Entry(entity).ComplexProperty<Milk>("Milk").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_property_complex_entry_by_IComplexProperty()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;
        var cultureProperty = context.Entry(entity).Metadata.FindComplexProperty("Culture")!;
        var milkProperty = context.Entry(entity).Metadata.FindComplexProperty("Milk")!;

        Assert.Same(cultureProperty, context.Entry(entity).ComplexProperty(cultureProperty).Metadata);
        Assert.Same(milkProperty, context.Entry((object)entity).ComplexProperty(milkProperty).Metadata);
    }

    [ConditionalFact]
    public void Can_get_generic_complex_property_entry_by_IComplexProperty()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;
        var cultureProperty = context.Entry(entity).Metadata.FindComplexProperty("Culture")!;
        var milkProperty = context.Entry(entity).Metadata.FindComplexProperty("Milk")!;

        Assert.Same(cultureProperty, context.Entry(entity).ComplexProperty<Culture>(cultureProperty).Metadata);
        Assert.Same(milkProperty, context.Entry(entity).ComplexProperty<Milk>(milkProperty).Metadata);
    }

    [ConditionalFact]
    public void Throws_when_wrong_generic_type_is_used_while_getting_complex_property_entry_by_name()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Equal(
            CoreStrings.WrongGenericPropertyType("Culture", entity.GetType().ShortDisplayName(), "Culture", "string"),
            Assert.Throws<ArgumentException>(() => context.Entry(entity).ComplexProperty<string>("Culture")).Message);

        Assert.Equal(
            CoreStrings.WrongGenericPropertyType("Milk", entity.GetType().ShortDisplayName(), "Milk", "string"),
            Assert.Throws<ArgumentException>(() => context.Entry(entity).ComplexProperty<string>("Milk")).Message);
    }

    [ConditionalFact]
    public void Can_get_generic_complex_property_entry_by_lambda()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Equal("Culture", context.Entry(entity).ComplexProperty(e => e.Culture).Metadata.Name);
        Assert.Equal("Milk", context.Entry(entity).ComplexProperty(e => e.Milk).Metadata.Name);
    }

    [ConditionalFact]
    public void Throws_when_wrong_complex_property_name_is_used_while_getting_property_entry_by_name()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Equal(
            CoreStrings.ComplexPropertyNotFound(entity.GetType().Name, "Chimp"),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).ComplexProperty("Chimp").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.ComplexPropertyNotFound(entity.GetType().Name, "Chimp"),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).ComplexProperty("Chimp").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.ComplexPropertyNotFound(entity.GetType().Name, "Chimp"),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).ComplexProperty<int>("Chimp").Metadata.Name).Message);
    }

    [ConditionalFact]
    public void Throws_when_accessing_complex_property_as_reference()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Culture", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference("Culture").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Culture", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Reference("Culture").Metadata.Name)
                .Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Culture", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference<Random>("Culture").Metadata.Name)
                .Message);

        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Milk", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference("Milk").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Milk", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Reference("Milk").Metadata.Name)
                .Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Milk", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference<Random>("Milk").Metadata.Name)
                .Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Milk", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference(e => e.Milk).Metadata.Name).Message);
    }

    [ConditionalFact]
    public void Throws_when_accessing_complex_property_as_collection()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;

        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Culture", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Collection("Culture").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Culture", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Collection("Culture").Metadata.Name)
                .Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Culture", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Collection<Random>("Culture").Metadata.Name)
                .Message);

        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Milk", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Collection("Milk").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Milk", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Collection("Milk").Metadata.Name)
                .Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Milk", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Collection<Random>("Milk").Metadata.Name)
                .Message);
    }

    [ConditionalFact]
    public void Can_get_complex_property_entry_by_name_using_Member()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        var entry = context.Entry(entity).Member("Culture");
        Assert.Equal("Culture", entry.Metadata.Name);
        Assert.IsType<ComplexPropertyEntry>(entry);

        entry = context.Entry((object)entity).Member("Culture");
        Assert.Equal("Culture", entry.Metadata.Name);
        Assert.IsType<ComplexPropertyEntry>(entry);

        entry = context.Entry(entity).Member("Milk");
        Assert.Equal("Milk", entry.Metadata.Name);
        Assert.IsType<ComplexPropertyEntry>(entry);

        entry = context.Entry((object)entity).Member("Milk");
        Assert.Equal("Milk", entry.Metadata.Name);
        Assert.IsType<ComplexPropertyEntry>(entry);
    }

    [ConditionalFact]
    public void Can_get_complex_property_entry_by_IPropertyBase_using_Member()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;
        var cultureBase = (IPropertyBase)context.Entry(entity).Metadata.FindComplexProperty("Culture")!;
        var milkBase = (IPropertyBase)context.Entry(entity).Metadata.FindComplexProperty("Milk")!;

        var entry = context.Entry(entity).Member(cultureBase);
        Assert.Same(cultureBase, entry.Metadata);
        Assert.IsType<ComplexPropertyEntry>(entry);

        entry = context.Entry((object)entity).Member(cultureBase);
        Assert.Same(cultureBase, entry.Metadata);
        Assert.IsType<ComplexPropertyEntry>(entry);

        entry = context.Entry(entity).Member(milkBase);
        Assert.Same(milkBase, entry.Metadata);
        Assert.IsType<ComplexPropertyEntry>(entry);

        entry = context.Entry((object)entity).Member(milkBase);
        Assert.Same(milkBase, entry.Metadata);
        Assert.IsType<ComplexPropertyEntry>(entry);
    }

    [ConditionalFact]
    public void Throws_when_accessing_complex_property_as_navigation()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateChunky()).Entity;

        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Culture", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Navigation("Culture").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Culture", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Navigation("Culture").Metadata.Name)
                .Message);
    }

    [ConditionalFact]
    public void Can_get_all_complex_property_entries()
    {
        using var context = new FreezerContext();
        Assert.Equal(
            ["Culture", "Milk"],
            context.Attach(CreateChunky()).ComplexProperties.Select(e => e.Metadata.Name).ToList());

        Assert.Equal(
            ["Culture", "Milk"],
            context.Attach(CreateCherry()).ComplexProperties.Select(e => e.Metadata.Name).ToList());

        Assert.Empty(context.Attach(new Half()).ComplexProperties);
    }

    [ConditionalFact]
    public void Can_get_complex_property_entry_by_name_using_fields()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;

        Assert.Equal("Culture", context.Entry(entity).ComplexProperty("Culture").Metadata.Name);
        Assert.Equal("Milk", context.Entry(entity).ComplexProperty("Milk").Metadata.Name);
        Assert.Equal("Culture", context.Entry((object)entity).ComplexProperty("Culture").Metadata.Name);
        Assert.Equal("Milk", context.Entry((object)entity).ComplexProperty("Milk").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_generic_complex_property_entry_by_name_using_fields()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;

        Assert.Equal("Culture", context.Entry(entity).ComplexProperty<FieldCulture>("Culture").Metadata.Name);
        Assert.Equal("Milk", context.Entry(entity).ComplexProperty<FieldMilk>("Milk").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_property_complex_entry_by_IComplexProperty_using_fields()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;
        var cultureProperty = context.Entry(entity).Metadata.FindComplexProperty("Culture")!;
        var milkProperty = context.Entry(entity).Metadata.FindComplexProperty("Milk")!;

        Assert.Same(cultureProperty, context.Entry(entity).ComplexProperty(cultureProperty).Metadata);
        Assert.Same(milkProperty, context.Entry((object)entity).ComplexProperty(milkProperty).Metadata);
    }

    [ConditionalFact]
    public void Can_get_generic_complex_property_entry_by_IComplexProperty_using_fields()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;
        var cultureProperty = context.Entry(entity).Metadata.FindComplexProperty("Culture")!;
        var milkProperty = context.Entry(entity).Metadata.FindComplexProperty("Milk")!;

        Assert.Same(cultureProperty, context.Entry(entity).ComplexProperty<FieldCulture>(cultureProperty).Metadata);
        Assert.Same(milkProperty, context.Entry(entity).ComplexProperty<FieldMilk>(milkProperty).Metadata);
    }

    [ConditionalFact]
    public void Throws_when_wrong_generic_type_is_used_while_getting_complex_property_entry_by_name_using_fields()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;

        Assert.Equal(
            CoreStrings.WrongGenericPropertyType("Culture", entity.GetType().ShortDisplayName(), "FieldCulture", "string"),
            Assert.Throws<ArgumentException>(() => context.Entry(entity).ComplexProperty<string>("Culture")).Message);

        Assert.Equal(
            CoreStrings.WrongGenericPropertyType("Milk", entity.GetType().ShortDisplayName(), "FieldMilk", "string"),
            Assert.Throws<ArgumentException>(() => context.Entry(entity).ComplexProperty<string>("Milk")).Message);
    }

    [ConditionalFact]
    public void Can_get_generic_complex_property_entry_by_lambda_using_fields()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;

        Assert.Equal("Culture", context.Entry(entity).ComplexProperty(e => e.Culture).Metadata.Name);
        Assert.Equal("Milk", context.Entry(entity).ComplexProperty(e => e.Milk).Metadata.Name);
    }

    [ConditionalFact]
    public void Throws_when_wrong_complex_property_name_is_used_while_getting_property_entry_by_name_using_fields()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;

        Assert.Equal(
            CoreStrings.ComplexPropertyNotFound(entity.GetType().Name, "Chimp"),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).ComplexProperty("Chimp").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.ComplexPropertyNotFound(entity.GetType().Name, "Chimp"),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).ComplexProperty("Chimp").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.ComplexPropertyNotFound(entity.GetType().Name, "Chimp"),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).ComplexProperty<int>("Chimp").Metadata.Name).Message);
    }

    [ConditionalFact]
    public void Throws_when_accessing_complex_property_as_reference_using_fields()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;

        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Culture", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference("Culture").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Culture", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Reference("Culture").Metadata.Name)
                .Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Culture", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference<Random>("Culture").Metadata.Name)
                .Message);

        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Milk", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference("Milk").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Milk", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Reference("Milk").Metadata.Name)
                .Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Milk", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference<Random>("Milk").Metadata.Name)
                .Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Milk", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference(e => e.Milk).Metadata.Name).Message);
    }

    [ConditionalFact]
    public void Can_get_complex_property_entry_by_name_using_Member_using_fields()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;

        var entry = context.Entry(entity).Member("Culture");
        Assert.Equal("Culture", entry.Metadata.Name);
        Assert.IsType<ComplexPropertyEntry>(entry);

        entry = context.Entry((object)entity).Member("Culture");
        Assert.Equal("Culture", entry.Metadata.Name);
        Assert.IsType<ComplexPropertyEntry>(entry);

        entry = context.Entry(entity).Member("Milk");
        Assert.Equal("Milk", entry.Metadata.Name);
        Assert.IsType<ComplexPropertyEntry>(entry);

        entry = context.Entry((object)entity).Member("Milk");
        Assert.Equal("Milk", entry.Metadata.Name);
        Assert.IsType<ComplexPropertyEntry>(entry);
    }

    [ConditionalFact]
    public void Can_get_complex_property_entry_by_IPropertyBase_using_Member_using_fields()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;
        var cultureBase = (IPropertyBase)context.Entry(entity).Metadata.FindComplexProperty("Culture")!;
        var milkBase = (IPropertyBase)context.Entry(entity).Metadata.FindComplexProperty("Milk")!;

        var entry = context.Entry(entity).Member(cultureBase);
        Assert.Same(cultureBase, entry.Metadata);
        Assert.IsType<ComplexPropertyEntry>(entry);

        entry = context.Entry((object)entity).Member(cultureBase);
        Assert.Same(cultureBase, entry.Metadata);
        Assert.IsType<ComplexPropertyEntry>(entry);

        entry = context.Entry(entity).Member(milkBase);
        Assert.Same(milkBase, entry.Metadata);
        Assert.IsType<ComplexPropertyEntry>(entry);

        entry = context.Entry((object)entity).Member(milkBase);
        Assert.Same(milkBase, entry.Metadata);
        Assert.IsType<ComplexPropertyEntry>(entry);
    }

    [ConditionalFact]
    public void Throws_when_accessing_complex_property_as_navigation_using_fields()
    {
        using var context = new FreezerContext();
        var entity = context.Add(CreateCherry()).Entity;

        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Culture", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Navigation("Culture").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.NavigationIsProperty(
                "Culture", entity.GetType().Name,
                nameof(EntityEntry.Reference), nameof(EntityEntry.Collection), nameof(EntityEntry.Property)),
            Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Navigation("Culture").Metadata.Name)
                .Message);
    }

    [ConditionalFact]
    public void Can_get_all_complex_property_entries_using_fields()
    {
        using var context = new FreezerContext();
        Assert.Equal(
            ["Culture", "Milk"],
            context.Attach(CreateCherry(76)).ComplexProperties.Select(e => e.Metadata.Name).ToList());

        Assert.Equal(
            ["Culture", "Milk"],
            context.Attach(CreateCherry(77)).ComplexProperties.Select(e => e.Metadata.Name).ToList());

        Assert.Empty(context.Attach(new Half()).ComplexProperties);
    }

    private class Chunky
    {
        public int Monkey { get; set; }
        public string? Nonkey { get; set; }
        public int Id { get; set; }

        public int GarciaId { get; set; }
        public Cherry? Garcia { get; set; }

        public Culture Culture { get; set; }
        public Milk Milk { get; set; } = null!;
    }

    private static Chunky CreateChunky(int id = 0)
        => new()
        {
            Id = id,
            Culture = new Culture
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            Milk = new Milk
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            }
        };

    private static Cherry CreateCherry(int id = 0)
        => new()
        {
            Id = id,
            Culture = new FieldCulture
            {
                License = new FieldLicense
                {
                    Charge = 1.0m,
                    Tag = new FieldTag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new FieldTog { Text = "To1" }
                },
                Manufacturer = new FieldManufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new FieldTag { Text = "Ta2" },
                    Tog = new FieldTog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            Milk = new FieldMilk
            {
                License = new FieldLicense
                {
                    Charge = 1.0m,
                    Tag = new FieldTag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new FieldTog { Text = "To1" }
                },
                Manufacturer = new FieldManufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new FieldTag { Text = "Ta2" },
                    Tog = new FieldTog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            }
        };

    private class Cherry
    {
        public int Garcia { get; set; }
        public int Id { get; set; }

        public ICollection<Chunky>? Monkeys { get; set; }

        public Half? Baked { get; set; }

        public FieldCulture Culture;
        public FieldMilk Milk = null!;
    }

    private class Half
    {
        public int Baked { get; set; }
        public int Id { get; set; }

        public int? GarciaId { get; set; }
        public Cherry? Garcia { get; set; }
    }

    private struct Culture
    {
        public string Species { get; set; }
        public string? Subspecies { get; set; }
        public int Rating { get; set; }
        public bool? Validation { get; set; }
        public Manufacturer Manufacturer { get; set; }
        public License License { get; set; }
    }

    private class Milk
    {
        public string Species { get; set; } = null!;
        public string? Subspecies { get; set; }
        public int Rating { get; set; }
        public bool? Validation { get; set; }
        public Manufacturer Manufacturer { get; set; } = null!;
        public License License { get; set; }
    }

    private class Manufacturer
    {
        public string? Name { get; set; }
        public int Rating { get; set; }
        public Tag Tag { get; set; } = null!;
        public Tog Tog { get; set; }
    }

    private struct License
    {
        public string Title { get; set; }
        public decimal Charge { get; set; }
        public Tag Tag { get; set; }
        public Tog Tog { get; set; }
    }

    private class Tag
    {
        public string? Text { get; set; }
    }

    private struct Tog
    {
        public string? Text { get; set; }
    }

    private struct FieldCulture
    {
        public string Species;
        public string? Subspecies;
        public int Rating;
        public bool? Validation;
        public FieldManufacturer Manufacturer;
        public FieldLicense License;
    }

    private class FieldMilk
    {
        public string Species = null!;
        public string? Subspecies;
        public int Rating;
        public bool? Validation;
        public FieldManufacturer Manufacturer = null!;
        public FieldLicense License;
    }

    private class FieldManufacturer
    {
        public string? Name;
        public int Rating;
        public FieldTag Tag = null!;
        public FieldTog Tog;
    }

    private struct FieldLicense
    {
        public string Title;
        public decimal Charge;
        public FieldTag Tag;
        public FieldTog Tog;
    }

    private class FieldTag
    {
        public string? Text;
    }

    private struct FieldTog
    {
        public string? Text;
    }

    private class FreezerContext : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(nameof(FreezerContext));

        public DbSet<Chunky> Icecream
            => Set<Chunky>();

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chunky>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();

                    b.ComplexProperty(
                        e => e.Culture, b =>
                        {
                            b.ComplexProperty(
                                e => e.License, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                            b.ComplexProperty(
                                e => e.Manufacturer, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                        });

                    b.ComplexProperty(
                        e => e.Milk, b =>
                        {
                            b.ComplexProperty(
                                e => e.License, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                            b.ComplexProperty(
                                e => e.Manufacturer, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                        });
                });

            modelBuilder.Entity<Cherry>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();

                    b.ComplexProperty(
                        e => e.Culture, b =>
                        {
                            b.ComplexProperty(
                                e => e.License, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                            b.ComplexProperty(
                                e => e.Manufacturer, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                        });

                    b.ComplexProperty(
                        e => e.Milk, b =>
                        {
                            b.ComplexProperty(
                                e => e.License, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                            b.ComplexProperty(
                                e => e.Manufacturer, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                        });
                });
        }
    }
}
