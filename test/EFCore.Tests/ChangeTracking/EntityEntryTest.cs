// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    public class EntityEntryTest
    {
        [ConditionalFact]
        public void Non_store_generated_key_is_always_set()
        {
            using var context = new KeySetContext();
            Assert.True(context.Entry(new NotStoreGenerated()).IsKeySet);
            Assert.True(
                context.Entry(
                    new NotStoreGenerated { Id = 1 }).IsKeySet);
        }

        [ConditionalFact]
        public void Non_store_generated_composite_key_is_always_set()
        {
            using var context = new KeySetContext();
            Assert.True(context.Entry(new CompositeNotStoreGenerated()).IsKeySet);
            Assert.True(
                context.Entry(
                    new CompositeNotStoreGenerated { Id1 = 1 }).IsKeySet);
            Assert.True(
                context.Entry(
                    new CompositeNotStoreGenerated { Id2 = true }).IsKeySet);
            Assert.True(
                context.Entry(
                    new CompositeNotStoreGenerated { Id1 = 1, Id2 = true }).IsKeySet);
        }

        [ConditionalFact]
        public void Store_generated_key_is_set_only_if_non_default_value()
        {
            using var context = new KeySetContext();
            Assert.False(context.Entry(new StoreGenerated()).IsKeySet);
            Assert.True(
                context.Entry(
                    new StoreGenerated { Id = 1 }).IsKeySet);
        }

        [ConditionalFact]
        public void Composite_store_generated_key_is_set_only_if_non_default_value_in_store_generated_part()
        {
            using var context = new KeySetContext();
            Assert.False(context.Entry(new CompositeStoreGenerated()).IsKeySet);
            Assert.False(
                context.Entry(
                    new CompositeStoreGenerated { Id1 = 1 }).IsKeySet);
            Assert.True(
                context.Entry(
                    new CompositeStoreGenerated { Id2 = true }).IsKeySet);
            Assert.True(
                context.Entry(
                    new CompositeStoreGenerated { Id1 = 1, Id2 = true }).IsKeySet);
        }

        [ConditionalFact]
        public void Primary_key_that_is_also_foreign_key_is_set_only_if_non_default_value()
        {
            using var context = new KeySetContext();
            Assert.False(context.Entry(new Dependent()).IsKeySet);
            Assert.True(
                context.Entry(
                    new Dependent { Id = 1 }).IsKeySet);
        }

        private class StoreGenerated
        {
            public int Id { get; set; }

            public Dependent Dependent { get; set; }
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

        private class CompositeNotStoreGenerated
        {
            public int Id1 { get; set; }
            public bool Id2 { get; set; }
        }

        private class Dependent
        {
            public int Id { get; set; }

            public StoreGenerated Principal { get; set; }
        }

        private class KeySetContext : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase(nameof(KeySetContext));

            public DbSet<StoreGenerated> StoreGenerated { get; set; }
            public DbSet<NotStoreGenerated> NotStoreGenerated { get; set; }
            public DbSet<CompositeStoreGenerated> CompositeStoreGenerated { get; set; }
            public DbSet<CompositeNotStoreGenerated> CompositeNotStoreGenerated { get; set; }
            public DbSet<Dependent> Dependent { get; set; }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<StoreGenerated>()
                    .HasOne(e => e.Dependent)
                    .WithOne(e => e.Principal)
                    .HasForeignKey<Dependent>(e => e.Id);

                modelBuilder.Entity<NotStoreGenerated>().Property(e => e.Id).ValueGeneratedNever();

                modelBuilder.Entity<CompositeNotStoreGenerated>().HasKey(
                    e => new { e.Id1, e.Id2 });

                modelBuilder.Entity<CompositeStoreGenerated>(
                    b =>
                    {
                        b.HasKey(
                            e => new { e.Id1, e.Id2 });
                        b.Property(e => e.Id2).ValueGeneratedOnAdd();
                    });
            }
        }

        [ConditionalFact]
        public void Detached_entities_are_not_returned_from_the_change_tracker()
        {
            using var context = new FreezerContext();
            var entity = new Chunky { Id = 808 };
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
            var entity = new Chunky();
            context.Add(entity);

            Assert.Same(entity, context.Entry(entity).Entity);
            Assert.Same(entity, context.Entry((object)entity).Entity);
        }

        [ConditionalFact]
        public void Can_obtain_context()
        {
            using var context = new FreezerContext();
            var entity = context.Add(new Chunky()).Entity;

            Assert.Same(context, context.Entry(entity).Context);
            Assert.Same(context, context.Entry((object)entity).Context);
        }

        [ConditionalFact]
        public void Can_obtain_underlying_state_entry()
        {
            using var context = new FreezerContext();
            var entity = context.Add(new Chunky()).Entity;
            var entry = context.GetService<IStateManager>().GetOrCreateEntry(entity);

            Assert.Same(entry, context.Entry(entity).GetInfrastructure());
            Assert.Same(entry, context.Entry((object)entity).GetInfrastructure());
        }

        [ConditionalFact]
        public void Can_get_metadata()
        {
            using var context = new FreezerContext();
            var entity = context.Add(new Chunky()).Entity;
            var entityType = context.Model.FindEntityType(typeof(Chunky));

            Assert.Same(entityType, context.Entry(entity).Metadata);
            Assert.Same(entityType, context.Entry((object)entity).Metadata);
        }

        [ConditionalFact]
        public void Can_get_and_change_state()
        {
            using var context = new FreezerContext();
            var entity = new Chunky();
            var entry = context.Add(entity).GetInfrastructure();

            context.Entry(entity).State = EntityState.Modified;
            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.Equal(EntityState.Modified, context.Entry(entity).State);

            context.Entry((object)entity).State = EntityState.Unchanged;
            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal(EntityState.Unchanged, context.Entry((object)entity).State);
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
            var entry = context.Add(new Chunky());

            entry.State = initialState;
            entry.State = expectedState;

            Assert.Equal(expectedState, entry.State);
        }

        [ConditionalFact]
        public void Can_get_property_entry_by_name()
        {
            using var context = new FreezerContext();
            var entity = context.Add(new Chunky()).Entity;

            Assert.Equal("Monkey", context.Entry(entity).Property("Monkey").Metadata.Name);
            Assert.Equal("Monkey", context.Entry((object)entity).Property("Monkey").Metadata.Name);
        }

        [ConditionalFact]
        public void Can_get_generic_property_entry_by_name()
        {
            using var context = new FreezerContext();
            var entity = context.Add(new Chunky()).Entity;

            Assert.Equal("Monkey", context.Entry(entity).Property<int>("Monkey").Metadata.Name);
        }

        [ConditionalFact]
        public void Throws_when_wrong_generic_type_is_used_while_getting_property_entry_by_name()
        {
            using var context = new FreezerContext();
            var entity = context.Add(new Chunky()).Entity;

            Assert.Equal(
                CoreStrings.WrongGenericPropertyType("Monkey", entity.GetType().ShortDisplayName(), "int", "string"),
                Assert.Throws<ArgumentException>(() => context.Entry(entity).Property<string>("Monkey")).Message);
        }

        [ConditionalFact]
        public void Can_get_generic_property_entry_by_lambda()
        {
            using var context = new FreezerContext();
            var entity = context.Add(new Chunky()).Entity;

            Assert.Equal("Monkey", context.Entry(entity).Property(e => e.Monkey).Metadata.Name);
        }

        [ConditionalFact]
        public void Throws_when_wrong_property_name_is_used_while_getting_property_entry_by_name()
        {
            using var context = new FreezerContext();
            var entity = context.Add(new Chunky()).Entity;

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
        public void Throws_when_accessing_navigation_as_property()
        {
            using var context = new FreezerContext();
            var entity = context.Add(new Chunky()).Entity;

            Assert.Equal(
                CoreStrings.PropertyIsNavigation(
                    "Garcia", entity.GetType().Name,
                    nameof(EntityEntry.Property), nameof(EntityEntry.Reference), nameof(EntityEntry.Collection)),
                Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Property("Garcia").Metadata.Name).Message);
            Assert.Equal(
                CoreStrings.PropertyIsNavigation(
                    "Garcia", entity.GetType().Name,
                    nameof(EntityEntry.Property), nameof(EntityEntry.Reference), nameof(EntityEntry.Collection)),
                Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Property("Garcia").Metadata.Name).Message);
            Assert.Equal(
                CoreStrings.PropertyIsNavigation(
                    "Garcia", entity.GetType().Name,
                    nameof(EntityEntry.Property), nameof(EntityEntry.Reference), nameof(EntityEntry.Collection)),
                Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Property<Cherry>("Garcia").Metadata.Name).Message);
            Assert.Equal(
                CoreStrings.PropertyIsNavigation(
                    "Garcia", entity.GetType().Name,
                    nameof(EntityEntry.Property), nameof(EntityEntry.Reference), nameof(EntityEntry.Collection)),
                Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Property(e => e.Garcia).Metadata.Name).Message);
        }

        [ConditionalFact]
        public void Can_get_reference_entry_by_name()
        {
            using var context = new FreezerContext();
            var entity = context.Add(new Chunky()).Entity;

            Assert.Equal("Garcia", context.Entry(entity).Reference("Garcia").Metadata.Name);
            Assert.Equal("Garcia", context.Entry((object)entity).Reference("Garcia").Metadata.Name);
        }

        [ConditionalFact]
        public void Can_get_generic_reference_entry_by_name()
        {
            using var context = new FreezerContext();
            var entity = context.Add(new Chunky()).Entity;

            Assert.Equal("Garcia", context.Entry(entity).Reference<Cherry>("Garcia").Metadata.Name);
        }

        [ConditionalFact]
        public void Can_get_generic_reference_entry_by_lambda()
        {
            using var context = new FreezerContext();
            var entity = context.Add(new Chunky()).Entity;

            Assert.Equal("Garcia", context.Entry(entity).Reference(e => e.Garcia).Metadata.Name);
        }

        [ConditionalFact]
        public void Throws_when_wrong_reference_name_is_used_while_getting_property_entry_by_name()
        {
            using var context = new FreezerContext();
            var entity = context.Add(new Chunky()).Entity;

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
            var entity = context.Add(new Chunky()).Entity;

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
            var entity = context.Add(new Cherry()).Entity;

            Assert.Equal(
                CoreStrings.ReferenceIsCollection(
                    "Monkeys", entity.GetType().Name,
                    nameof(EntityEntry.Reference), nameof(EntityEntry.Collection)),
                Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference("Monkeys").Metadata.Name).Message);
            Assert.Equal(
                CoreStrings.ReferenceIsCollection(
                    "Monkeys", entity.GetType().Name,
                    nameof(EntityEntry.Reference), nameof(EntityEntry.Collection)),
                Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Reference("Monkeys").Metadata.Name)
                    .Message);
            Assert.Equal(
                CoreStrings.ReferenceIsCollection(
                    "Monkeys", entity.GetType().Name,
                    nameof(EntityEntry.Reference), nameof(EntityEntry.Collection)),
                Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference<Random>("Monkeys").Metadata.Name)
                    .Message);
            Assert.Equal(
                CoreStrings.ReferenceIsCollection(
                    "Monkeys", entity.GetType().Name,
                    nameof(EntityEntry.Reference), nameof(EntityEntry.Collection)),
                Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Reference(e => e.Monkeys).Metadata.Name).Message);
        }

        [ConditionalFact]
        public void Can_get_collection_entry_by_name()
        {
            using var context = new FreezerContext();
            var entity = context.Add(new Cherry()).Entity;

            Assert.Equal("Monkeys", context.Entry(entity).Collection("Monkeys").Metadata.Name);
            Assert.Equal("Monkeys", context.Entry((object)entity).Collection("Monkeys").Metadata.Name);
        }

        [ConditionalFact]
        public void Can_get_generic_collection_entry_by_name()
        {
            using var context = new FreezerContext();
            var entity = context.Add(new Cherry()).Entity;

            Assert.Equal("Monkeys", context.Entry(entity).Collection<Chunky>("Monkeys").Metadata.Name);
        }

        [ConditionalFact]
        public void Can_get_generic_collection_entry_by_lambda()
        {
            using var context = new FreezerContext();
            var entity = context.Add(new Cherry()).Entity;

            Assert.Equal("Monkeys", context.Entry(entity).Collection(e => e.Monkeys).Metadata.Name);
        }

        [ConditionalFact]
        public void Throws_when_wrong_collection_name_is_used_while_getting_property_entry_by_name()
        {
            using var context = new FreezerContext();
            var entity = context.Add(new Cherry()).Entity;

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
            var entity = context.Add(new Cherry()).Entity;

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
            var entity = context.Add(new Chunky()).Entity;

            Assert.Equal(
                CoreStrings.CollectionIsReference(
                    "Garcia", entity.GetType().Name,
                    nameof(EntityEntry.Collection), nameof(EntityEntry.Reference)),
                Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Collection("Garcia").Metadata.Name).Message);
            Assert.Equal(
                CoreStrings.CollectionIsReference(
                    "Garcia", entity.GetType().Name,
                    nameof(EntityEntry.Collection), nameof(EntityEntry.Reference)),
                Assert.Throws<InvalidOperationException>(() => context.Entry((object)entity).Collection("Garcia").Metadata.Name)
                    .Message);
            Assert.Equal(
                CoreStrings.CollectionIsReference(
                    "Garcia", entity.GetType().Name,
                    nameof(EntityEntry.Collection), nameof(EntityEntry.Reference)),
                Assert.Throws<InvalidOperationException>(() => context.Entry(entity).Collection<Cherry>("Garcia").Metadata.Name)
                    .Message);
        }

        [ConditionalFact]
        public void Can_get_property_entry_by_name_using_Member()
        {
            using var context = new FreezerContext();
            var entity = context.Add(new Chunky()).Entity;

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
            var entity = context.Add(new Chunky()).Entity;

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
            var entity = context.Add(new Chunky()).Entity;

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
            var entity = context.Add(new Cherry()).Entity;

            var entry = context.Entry(entity).Member("Monkeys");
            Assert.Equal("Monkeys", entry.Metadata.Name);
            Assert.IsType<CollectionEntry>(entry);

            entry = context.Entry((object)entity).Member("Monkeys");
            Assert.Equal("Monkeys", entry.Metadata.Name);
            Assert.IsType<CollectionEntry>(entry);
        }

        [ConditionalFact]
        public void Throws_when_wrong_property_name_is_used_while_getting_property_entry_by_name_using_Navigation()
        {
            using var context = new FreezerContext();
            var entity = context.Add(new Chunky()).Entity;

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
            var entity = context.Add(new Chunky()).Entity;

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
            var entity = context.Add(new Cherry()).Entity;

            var entry = context.Entry(entity).Navigation("Monkeys");
            Assert.Equal("Monkeys", entry.Metadata.Name);
            Assert.IsType<CollectionEntry>(entry);

            entry = context.Entry((object)entity).Navigation("Monkeys");
            Assert.Equal("Monkeys", entry.Metadata.Name);
            Assert.IsType<CollectionEntry>(entry);
        }

        [ConditionalFact]
        public void Throws_when_accessing_property_as_navigation()
        {
            using var context = new FreezerContext();
            var entity = context.Add(new Chunky()).Entity;

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
            var entity = context.Attach(new Chunky()).Entity;

            var modified = context.Entry(entity).Properties
                .Where(e => e.IsModified).Select(e => e.Metadata.Name).ToList();

            Assert.Empty(modified);

            entity.Nonkey = "Blue";
            entity.GarciaId = 77;

            context.ChangeTracker.DetectChanges();

            modified = context.Entry(entity).Properties
                .Where(e => e.IsModified).Select(e => e.Metadata.Name).ToList();

            Assert.Equal(
                new List<string> { "GarciaId", "Nonkey" }, modified);
        }

        [ConditionalFact]
        public void Can_get_all_member_entries()
        {
            using var context = new FreezerContext();
            Assert.Equal(
                new List<string>
                {
                    "Id",
                    "GarciaId",
                    "Monkey",
                    "Nonkey",
                    "Garcia"
                },
                context.Attach(new Chunky()).Members.Select(e => e.Metadata.Name).ToList());

            Assert.Equal(
                new List<string>
                {
                    "Id",
                    "Garcia",
                    "Baked",
                    "Monkeys"
                },
                context.Attach(new Cherry()).Members.Select(e => e.Metadata.Name).ToList());

            Assert.Equal(
                new List<string>
                {
                    "Id",
                    "Baked",
                    "GarciaId",
                    "Garcia"
                },
                context.Attach(new Half()).Members.Select(e => e.Metadata.Name).ToList());
        }

        [ConditionalFact]
        public void Can_get_all_property_entries()
        {
            using var context = new FreezerContext();
            Assert.Equal(
                new List<string>
                {
                    "Id",
                    "GarciaId",
                    "Monkey",
                    "Nonkey"
                },
                context.Attach(new Chunky()).Properties.Select(e => e.Metadata.Name).ToList());

            Assert.Equal(
                new List<string> { "Id", "Garcia" },
                context.Attach(new Cherry()).Properties.Select(e => e.Metadata.Name).ToList());

            Assert.Equal(
                new List<string>
                {
                    "Id",
                    "Baked",
                    "GarciaId"
                },
                context.Attach(new Half()).Properties.Select(e => e.Metadata.Name).ToList());
        }

        [ConditionalFact]
        public void Can_get_all_navigation_entries()
        {
            using var context = new FreezerContext();
            Assert.Equal(
                new List<string> { "Garcia" },
                context.Attach(new Chunky()).Navigations.Select(e => e.Metadata.Name).ToList());

            Assert.Equal(
                new List<string> { "Baked", "Monkeys" },
                context.Attach(new Cherry()).Navigations.Select(e => e.Metadata.Name).ToList());

            Assert.Equal(
                new List<string> { "Garcia" },
                context.Attach(new Half()).Navigations.Select(e => e.Metadata.Name).ToList());
        }

        [ConditionalFact]
        public void Can_get_all_reference_entries()
        {
            using var context = new FreezerContext();
            Assert.Equal(
                new List<string> { "Garcia" },
                context.Attach(new Chunky()).References.Select(e => e.Metadata.Name).ToList());

            Assert.Equal(
                new List<string> { "Baked" },
                context.Attach(new Cherry()).References.Select(e => e.Metadata.Name).ToList());

            Assert.Equal(
                new List<string> { "Garcia" },
                context.Attach(new Half()).References.Select(e => e.Metadata.Name).ToList());
        }

        [ConditionalFact]
        public void Can_get_all_collection_entries()
        {
            using var context = new FreezerContext();
            Assert.Empty(context.Attach(new Chunky()).Collections.Select(e => e.Metadata.Name).ToList());

            Assert.Equal(
                new List<string> { "Monkeys" },
                context.Attach(new Cherry()).Collections.Select(e => e.Metadata.Name).ToList());

            Assert.Empty(context.Attach(new Half()).Collections.Select(e => e.Metadata.Name).ToList());
        }

        private class Chunky
        {
            public int Monkey { get; set; }
            public string Nonkey { get; set; }
            public int Id { get; set; }

            public int GarciaId { get; set; }
            public Cherry Garcia { get; set; }
        }

        private class Cherry
        {
            public int Garcia { get; set; }
            public int Id { get; set; }

            public ICollection<Chunky> Monkeys { get; set; }

            public Half Baked { get; set; }
        }

        private class Half
        {
            public int Baked { get; set; }
            public int Id { get; set; }

            public int? GarciaId { get; set; }
            public Cherry Garcia { get; set; }
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
}
