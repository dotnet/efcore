// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class StateManagerTest
    {
        [Fact]
        public void Can_get_existing_entry_if_entity_is_already_tracked_otherwise_new_entry()
        {
            var stateManager = CreateStateManager(BuildModel());
            var category = new Category { Id = 1 };

            var entry = stateManager.GetOrCreateEntry(category);

            stateManager.StartTracking(entry);

            var entry2 = stateManager.GetOrCreateEntry(category);

            Assert.Same(entry, entry2);
            Assert.Equal(EntityState.Detached, entry.EntityState);
        }

        [Fact]
        public void StartTracking_throws_if_different_instance_with_same_identity_is_already_tracked()
        {
            var model = BuildModel();
            var categoryType = model.GetEntityType(typeof(Category));
            var stateManager = CreateStateManager(model);

            var entityKey = new SimpleEntityKey<int>(categoryType.GetPrimaryKey(), 77);
            var valueBuffer = new ValueBuffer(new object[] { 77, "Bjork", null });

            stateManager.StartTracking(categoryType, entityKey, new Category { Id = 77 }, valueBuffer);

            Assert.Equal(
                CoreStrings.IdentityConflict("Category"),
                Assert.Throws<InvalidOperationException>(
                    () => stateManager.StartTracking(categoryType, entityKey, new Category { Id = 77 }, valueBuffer)).Message);
        }

        [Fact]
        public void StartTracking_is_no_op_if_entity_is_already_tracked()
        {
            var model = BuildModel();
            var categoryType = model.GetEntityType(typeof(Category));
            var stateManager = CreateStateManager(model);

            var category = new Category { Id = 77 };
            var entityKey = new SimpleEntityKey<int>(categoryType.GetPrimaryKey(), 77);
            var valueBuffer = new ValueBuffer(new object[] { 77, "Bjork", null });

            var entry = stateManager.StartTracking(categoryType, entityKey, category, valueBuffer);

            Assert.Same(entry, stateManager.StartTracking(categoryType, entityKey, category, valueBuffer));
        }

        [Fact]
        public void StartTracking_throws_for_invalid_entity_key()
        {
            var model = BuildModel();
            var categoryType = model.GetEntityType(typeof(Category));
            var stateManager = CreateStateManager(model);

            var valueBuffer = new ValueBuffer(new object[] { 0, "Bjork", null });

            Assert.Equal(
                CoreStrings.InvalidPrimaryKey("Category"),
                Assert.Throws<InvalidOperationException>(
                    () => stateManager.StartTracking(categoryType, EntityKey.InvalidEntityKey, new Category { Id = 0 }, valueBuffer)).Message);
        }

        [Fact]
        public void Can_explicitly_create_new_entry_for_shadow_state_entity()
        {
            var model = BuildModel();
            var stateManager = CreateStateManager(model);
            var entityType = model.GetEntityType("Location");
            var entry = stateManager.CreateNewEntry(entityType);
            entry[entityType.GetPrimaryKey().Properties.Single()] = 42;

            Assert.Equal(EntityState.Detached, entry.EntityState);
            Assert.Null(entry.Entity);
            Assert.Equal(0, stateManager.Entries.Count());

            stateManager.StartTracking(entry);

            Assert.Equal(1, stateManager.Entries.Count());
        }

        [Fact]
        public void Can_explicitly_create_new_entry_for_normal_state_entity()
        {
            var model = BuildModel();
            var stateManager = CreateStateManager(model);

            var entry = stateManager.CreateNewEntry(model.GetEntityType(typeof(Category)));

            Assert.Equal(EntityState.Detached, entry.EntityState);
            Assert.IsType<Category>(entry.Entity);
            Assert.Equal(0, stateManager.Entries.Count());

            ((Category)entry.Entity).Id = 1;
            stateManager.StartTracking(entry);

            Assert.Equal(1, stateManager.Entries.Count());
        }

        [Fact]
        public void Can_get_existing_entry_even_if_state_not_yet_set()
        {
            var stateManager = CreateStateManager(BuildModel());
            var category = new Category { Id = 1 };

            var entry = stateManager.GetOrCreateEntry(category);
            var entry2 = stateManager.GetOrCreateEntry(category);

            Assert.Same(entry, entry2);
            Assert.Equal(EntityState.Detached, entry.EntityState);
        }

        [Fact]
        public void Can_stop_tracking_and_then_start_tracking_again()
        {
            var stateManager = CreateStateManager(BuildModel());
            var category = new Category { Id = 1 };

            var entry = stateManager.GetOrCreateEntry(category);
            stateManager.StartTracking(entry);
            stateManager.StopTracking(entry);
            stateManager.StartTracking(entry);

            var entry2 = stateManager.GetOrCreateEntry(category);
            Assert.Same(entry, entry2);
        }

        [Fact]
        public void Can_stop_tracking_and_then_start_tracking_using_a_new_state_entry()
        {
            var stateManager = CreateStateManager(BuildModel());
            var category = new Category { Id = 1 };

            var entry = stateManager.GetOrCreateEntry(category);
            stateManager.StartTracking(entry);
            stateManager.StopTracking(entry);

            var entry2 = stateManager.GetOrCreateEntry(category);
            Assert.Same(entry, entry2);

            stateManager.StartTracking(entry2);
        }

        [Fact]
        public void StopTracking_keeps_track_of_detached_entity_using_weak_reference()
        {
            var stateManager = CreateStateManager(BuildModel());
            var category = new Category { Id = 1 };

            var entry = stateManager.GetOrCreateEntry(category);
            stateManager.StartTracking(entry);
            stateManager.StopTracking(entry);

            var entry2 = stateManager.GetOrCreateEntry(category);
            stateManager.StartTracking(entry2);

            Assert.Same(entry, entry2);
            Assert.Equal(EntityState.Detached, entry.EntityState);
        }

        [Fact]
        public void Throws_on_attempt_to_start_tracking_different_entities_with_same_identity()
        {
            var stateManager = CreateStateManager(BuildModel());
            var category1 = new Category { Id = 7 };
            var category2 = new Category { Id = 7 };

            var entry1 = stateManager.GetOrCreateEntry(category1);
            var entry2 = stateManager.GetOrCreateEntry(category2);

            stateManager.StartTracking(entry1);

            Assert.Equal(
                CoreStrings.IdentityConflict(typeof(Category).FullName),
                Assert.Throws<InvalidOperationException>(() => stateManager.StartTracking(entry2)).Message);
        }

        [Fact]
        public void Throws_on_attempt_to_start_tracking_entity_with_null_key()
        {
            var stateManager = CreateStateManager(BuildModel());
            var entity = new Dogegory();

            var entry = stateManager.GetOrCreateEntry(entity);

            Assert.Equal(
                CoreStrings.InvalidPrimaryKey(typeof(Dogegory).FullName),
                Assert.Throws<InvalidOperationException>(() => stateManager.StartTracking(entry)).Message);
        }

        [Fact]
        public void Throws_on_attempt_to_start_tracking_entity_with_sentinel_key()
        {
            var stateManager = CreateStateManager(BuildModel());
            var entity = new Category();

            var entry = stateManager.GetOrCreateEntry(entity);

            Assert.Equal(
                CoreStrings.InvalidPrimaryKey(typeof(Category).FullName),
                Assert.Throws<InvalidOperationException>(() => stateManager.StartTracking(entry)).Message);
        }

        [Fact]
        public void Throws_on_attempt_to_start_tracking_entity_with_non_default_sentinel_key()
        {
            var model = BuildModel();
            model.GetEntityType(typeof(Category)).GetProperty("Id").SentinelValue = 7;

            var stateManager = CreateStateManager(model);
            var entity = new Category { Id = 7 };

            var entry = stateManager.GetOrCreateEntry(entity);

            Assert.Equal(
                CoreStrings.InvalidPrimaryKey(typeof(Category).FullName),
                Assert.Throws<InvalidOperationException>(() => stateManager.StartTracking(entry)).Message);
        }

        [Fact]
        public void Throws_on_attempt_to_start_tracking_with_wrong_manager()
        {
            var model = BuildModel();
            var stateManager = CreateStateManager(model);
            var stateManager2 = CreateStateManager(model);

            var entry = stateManager.GetOrCreateEntry(new Category());

            Assert.Equal(
                CoreStrings.WrongStateManager(typeof(Category).FullName),
                Assert.Throws<InvalidOperationException>(() => stateManager2.StartTracking(entry)).Message);
        }

        [Fact]
        public void Will_get_new_entry_if_another_entity_with_the_same_key_is_already_tracked()
        {
            var stateManager = CreateStateManager(BuildModel());

            Assert.NotSame(
                stateManager.GetOrCreateEntry(new Category { Id = 77 }),
                stateManager.GetOrCreateEntry(new Category { Id = 77 }));
        }

        [Fact]
        public void Can_get_all_entities()
        {
            var stateManager = CreateStateManager(BuildModel());

            var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
            var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

            stateManager.StartTracking(stateManager.GetOrCreateEntry(new Category { Id = 77 }));
            stateManager.StartTracking(stateManager.GetOrCreateEntry(new Category { Id = 78 }));
            stateManager.StartTracking(stateManager.GetOrCreateEntry(new Product { Id = productId1 }));
            stateManager.StartTracking(stateManager.GetOrCreateEntry(new Product { Id = productId2 }));

            Assert.Equal(4, stateManager.Entries.Count());

            Assert.Equal(
                new[] { 77, 78 },
                stateManager.Entries
                    .Select(e => e.Entity)
                    .OfType<Category>()
                    .Select(e => e.Id)
                    .OrderBy(k => k)
                    .ToArray());

            Assert.Equal(
                new[] { productId2, productId1 },
                stateManager.Entries
                    .Select(e => e.Entity)
                    .OfType<Product>()
                    .Select(e => e.Id)
                    .OrderBy(k => k)
                    .ToArray());
        }

        [Fact]
        public void Listeners_are_notified_when_entity_states_change()
        {
            var listeners = new[]
                {
                    new Mock<IEntityStateListener>(),
                    new Mock<IEntityStateListener>(),
                    new Mock<IEntityStateListener>()
                };

            var services = new ServiceCollection()
                .AddInstance(listeners[0].Object)
                .AddInstance(listeners[1].Object)
                .AddInstance(listeners[2].Object);

            var contextServices = TestHelpers.Instance.CreateContextServices(services, BuildModel());

            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var entry = stateManager.GetOrCreateEntry(new Category { Id = 77 });
            entry.SetEntityState(EntityState.Added);

            foreach (var listener in listeners)
            {
                listener.Verify(m => m.StateChanging(entry, It.IsAny<EntityState>()), Times.Once);
                listener.Verify(m => m.StateChanged(entry, It.IsAny<EntityState>()), Times.Once);

                listener.Verify(m => m.StateChanging(entry, EntityState.Added), Times.Once);
                listener.Verify(m => m.StateChanged(entry, EntityState.Detached), Times.Once);
            }

            entry.SetEntityState(EntityState.Modified);

            foreach (var listener in listeners)
            {
                listener.Verify(m => m.StateChanging(entry, It.IsAny<EntityState>()), Times.Exactly(2));
                listener.Verify(m => m.StateChanged(entry, It.IsAny<EntityState>()), Times.Exactly(2));

                listener.Verify(m => m.StateChanging(entry, EntityState.Modified), Times.Once);
                listener.Verify(m => m.StateChanged(entry, EntityState.Detached), Times.Once);
            }
        }

        [Fact]
        public void DetectChanges_is_called_for_all_tracked_entities_and_returns_true_if_any_changes_detected()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<IChangeDetector, ChangeDetectorProxy>(),
                BuildModel());

            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var entry1 = stateManager.GetOrCreateEntry(new Category { Id = 77, Name = "Beverages" });
            var entry2 = stateManager.GetOrCreateEntry(new Category { Id = 78, Name = "Foods" });
            var entry3 = stateManager.GetOrCreateEntry(new Category { Id = 79, Name = "Stuff" });

            stateManager.StartTracking(entry1);
            stateManager.StartTracking(entry2);
            stateManager.StartTracking(entry3);

            var changeDetector = (ChangeDetectorProxy)contextServices.GetRequiredService<IChangeDetector>();

            changeDetector.DetectChanges(stateManager);

            Assert.Equal(new[] { 77, 78, 79 }, changeDetector.Entries.Select(e => ((Category)e.Entity).Id).ToArray());

            ((Category)entry2.Entity).Name = "Snacks";

            changeDetector.DetectChanges(stateManager);

            Assert.Equal(new[] { 77, 78, 79, 77, 78, 79 }, changeDetector.Entries.Select(e => ((Category)e.Entity).Id).ToArray());
        }

        internal class ChangeDetectorProxy : ChangeDetector
        {
            public ChangeDetectorProxy(IEntityGraphAttacher attacher)
                : base(attacher)
            {
            }

            public List<InternalEntityEntry> Entries { get; } = new List<InternalEntityEntry>();

            public override void DetectChanges(InternalEntityEntry entry)
            {
                Entries.Add(entry);

                base.DetectChanges(entry);
            }
        }

        [Fact]
        public void SaveChanges_processes_all_tracked_entities()
        {
            var stateManager = CreateStateManager(BuildModel());

            var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
            var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

            var entry1 = stateManager.GetOrCreateEntry(new Category { Id = 77 });
            var entry2 = stateManager.GetOrCreateEntry(new Category { Id = 78 });
            var entry3 = stateManager.GetOrCreateEntry(new Product { Id = productId1 });
            var entry4 = stateManager.GetOrCreateEntry(new Product { Id = productId2 });

            entry1.SetEntityState(EntityState.Added);
            entry2.SetEntityState(EntityState.Modified);
            entry3.SetEntityState(EntityState.Unchanged);
            entry4.SetEntityState(EntityState.Deleted);

            var processedEntities = stateManager.SaveChanges(acceptAllChangesOnSuccess: true);

            Assert.Equal(3, processedEntities);
            Assert.Equal(3, stateManager.Entries.Count());
            Assert.Contains(entry1, stateManager.Entries);
            Assert.Contains(entry2, stateManager.Entries);
            Assert.Contains(entry3, stateManager.Entries);

            Assert.Equal(EntityState.Unchanged, entry1.EntityState);
            Assert.Equal(EntityState.Unchanged, entry2.EntityState);
            Assert.Equal(EntityState.Unchanged, entry3.EntityState);
        }

        [Fact]
        public void SaveChanges_false_processes_all_tracked_entities_without_calling_AcceptAllChanges()
        {
            var stateManager = CreateStateManager(BuildModel());

            var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
            var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

            var entry1 = stateManager.GetOrCreateEntry(new Category { Id = 77 });
            var entry2 = stateManager.GetOrCreateEntry(new Category { Id = 78 });
            var entry3 = stateManager.GetOrCreateEntry(new Product { Id = productId1 });
            var entry4 = stateManager.GetOrCreateEntry(new Product { Id = productId2 });

            entry1.SetEntityState(EntityState.Added);
            entry2.SetEntityState(EntityState.Modified);
            entry3.SetEntityState(EntityState.Unchanged);
            entry4.SetEntityState(EntityState.Deleted);

            var processedEntities = stateManager.SaveChanges(acceptAllChangesOnSuccess: false);

            Assert.Equal(3, processedEntities);
            Assert.Equal(4, stateManager.Entries.Count());
            Assert.Contains(entry1, stateManager.Entries);
            Assert.Contains(entry2, stateManager.Entries);
            Assert.Contains(entry3, stateManager.Entries);
            Assert.Contains(entry4, stateManager.Entries);

            Assert.Equal(EntityState.Added, entry1.EntityState);
            Assert.Equal(EntityState.Modified, entry2.EntityState);
            Assert.Equal(EntityState.Unchanged, entry3.EntityState);
            Assert.Equal(EntityState.Deleted, entry4.EntityState);
        }

        [Fact]
        public async void SaveChangesAsync_processes_all_tracked_entities()
        {
            var stateManager = CreateStateManager(BuildModel());

            var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
            var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

            var entry1 = stateManager.GetOrCreateEntry(new Category { Id = 77 });
            var entry2 = stateManager.GetOrCreateEntry(new Category { Id = 78 });
            var entry3 = stateManager.GetOrCreateEntry(new Product { Id = productId1 });
            var entry4 = stateManager.GetOrCreateEntry(new Product { Id = productId2 });

            entry1.SetEntityState(EntityState.Added);
            entry2.SetEntityState(EntityState.Modified);
            entry3.SetEntityState(EntityState.Unchanged);
            entry4.SetEntityState(EntityState.Deleted);

            var processedEntities = await stateManager.SaveChangesAsync(acceptAllChangesOnSuccess: true);

            Assert.Equal(3, processedEntities);
            Assert.Equal(3, stateManager.Entries.Count());
            Assert.Contains(entry1, stateManager.Entries);
            Assert.Contains(entry2, stateManager.Entries);
            Assert.Contains(entry3, stateManager.Entries);

            Assert.Equal(EntityState.Unchanged, entry1.EntityState);
            Assert.Equal(EntityState.Unchanged, entry2.EntityState);
            Assert.Equal(EntityState.Unchanged, entry3.EntityState);
        }

        [Fact]
        public async void SaveChangesAsync_false_processes_all_tracked_entities_without_calling_AcceptAllChanges()
        {
            var stateManager = CreateStateManager(BuildModel());

            var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
            var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

            var entry1 = stateManager.GetOrCreateEntry(new Category { Id = 77 });
            var entry2 = stateManager.GetOrCreateEntry(new Category { Id = 78 });
            var entry3 = stateManager.GetOrCreateEntry(new Product { Id = productId1 });
            var entry4 = stateManager.GetOrCreateEntry(new Product { Id = productId2 });

            entry1.SetEntityState(EntityState.Added);
            entry2.SetEntityState(EntityState.Modified);
            entry3.SetEntityState(EntityState.Unchanged);
            entry4.SetEntityState(EntityState.Deleted);

            var processedEntities = await stateManager.SaveChangesAsync(acceptAllChangesOnSuccess: false);

            Assert.Equal(3, processedEntities);
            Assert.Equal(4, stateManager.Entries.Count());
            Assert.Contains(entry1, stateManager.Entries);
            Assert.Contains(entry2, stateManager.Entries);
            Assert.Contains(entry3, stateManager.Entries);
            Assert.Contains(entry4, stateManager.Entries);

            Assert.Equal(EntityState.Added, entry1.EntityState);
            Assert.Equal(EntityState.Modified, entry2.EntityState);
            Assert.Equal(EntityState.Unchanged, entry3.EntityState);
            Assert.Equal(EntityState.Deleted, entry4.EntityState);
        }

        [Fact]
        public void AcceptAllChanges_processes_all_tracked_entities()
        {
            var stateManager = CreateStateManager(BuildModel());

            var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
            var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

            var entry1 = stateManager.GetOrCreateEntry(new Category { Id = 77 });
            var entry2 = stateManager.GetOrCreateEntry(new Category { Id = 78 });
            var entry3 = stateManager.GetOrCreateEntry(new Product { Id = productId1 });
            var entry4 = stateManager.GetOrCreateEntry(new Product { Id = productId2 });

            entry1.SetEntityState(EntityState.Added);
            entry2.SetEntityState(EntityState.Modified);
            entry3.SetEntityState(EntityState.Unchanged);
            entry4.SetEntityState(EntityState.Deleted);

            stateManager.AcceptAllChanges();

            Assert.Equal(3, stateManager.Entries.Count());
            Assert.Contains(entry1, stateManager.Entries);
            Assert.Contains(entry2, stateManager.Entries);
            Assert.Contains(entry3, stateManager.Entries);

            Assert.Equal(EntityState.Unchanged, entry1.EntityState);
            Assert.Equal(EntityState.Unchanged, entry2.EntityState);
            Assert.Equal(EntityState.Unchanged, entry3.EntityState);
            Assert.Equal(EntityState.Detached, entry4.EntityState);
        }

        [Fact]
        public void Can_get_all_dependent_entries()
        {
            var model = BuildModel();
            var stateManager = CreateStateManager(model);

            var categoryEntry1 = stateManager.StartTracking(stateManager.GetOrCreateEntry(new Category { Id = 1, PrincipalId = 77 }));
            var categoryEntry2 = stateManager.StartTracking(stateManager.GetOrCreateEntry(new Category { Id = 2, PrincipalId = 78 }));
            var categoryEntry3 = stateManager.StartTracking(stateManager.GetOrCreateEntry(new Category { Id = 3, PrincipalId = 79 }));
            var categoryEntry4 = stateManager.StartTracking(stateManager.GetOrCreateEntry(new Category { Id = 4, PrincipalId = null }));
            var productEntry1 = stateManager.StartTracking(stateManager.GetOrCreateEntry(new Product { Id = Guid.NewGuid(), DependentId = 77 }));
            var productEntry2 = stateManager.StartTracking(stateManager.GetOrCreateEntry(new Product { Id = Guid.NewGuid(), DependentId = 77 }));
            var productEntry3 = stateManager.StartTracking(stateManager.GetOrCreateEntry(new Product { Id = Guid.NewGuid(), DependentId = 78 }));
            var productEntry4 = stateManager.StartTracking(stateManager.GetOrCreateEntry(new Product { Id = Guid.NewGuid(), DependentId = 78 }));
            var productEntry5 = stateManager.StartTracking(stateManager.GetOrCreateEntry(new Product { Id = Guid.NewGuid(), DependentId = null }));

            var fk = model.GetEntityType(typeof(Product)).GetForeignKeys().Single();

            Assert.Equal(
                new[] { productEntry1, productEntry2 },
                stateManager.GetDependents(categoryEntry1, fk).ToArray());

            Assert.Equal(
                new[] { productEntry3, productEntry4 },
                stateManager.GetDependents(categoryEntry2, fk).ToArray());

            Assert.Empty(stateManager.GetDependents(categoryEntry3, fk).ToArray());
            Assert.Empty(stateManager.GetDependents(categoryEntry4, fk).ToArray());
        }

        private static IStateManager CreateStateManager(IModel model)
        {
            return TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();
        }

        private class Category
        {
            public int Id { get; set; }
            public int? PrincipalId { get; set; }
            public string Name { get; set; }
        }

        private class Product
        {
            public Guid Id { get; set; }
            public int? DependentId { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
        }

        private class Dogegory
        {
            public string Id { get; set; }
        }

        private static Model BuildModel()
        {
            var builder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            var model = builder.Model;

            builder.Entity<Product>().HasOne<Category>().WithOne()
                .HasForeignKey<Product>(e => e.DependentId)
                .HasPrincipalKey<Category>(e => e.PrincipalId);
            builder.Entity<Category>();
            builder.Entity<Dogegory>();
            builder.Entity("Location", eb =>
                {
                    eb.Property<int>("Id");
                    eb.Property<string>("Planet");
                    eb.HasKey("Id");
                });

            return model;
        }
    }
}
