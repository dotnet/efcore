// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.ChangeTracking.Internal
{
    public class StateManagerTest
    {
        [Fact]
        public void Can_get_existing_entry_if_entity_is_already_tracked_otherwise_new_entry()
        {
            var stateManager = CreateStateManager(BuildModel());
            var category = new Category { Id = 1, PrincipalId = 777 };

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
            var stateManager = CreateStateManager(model);

            var entry1 = stateManager.GetOrCreateEntry(new Category { Id = 77, PrincipalId = 777 });
            stateManager.StartTracking(entry1);

            var entry2 = stateManager.GetOrCreateEntry(new Category { Id = 77, PrincipalId = 777 });

            Assert.Equal(
                CoreStrings.IdentityConflict("Category"),
                Assert.Throws<InvalidOperationException>(
                    () => stateManager.StartTracking(entry2)).Message);
        }

        [Fact]
        public void StartTracking_is_no_op_if_entity_is_already_tracked()
        {
            var model = BuildModel();
            var categoryType = model.FindEntityType(typeof(Category));
            var stateManager = CreateStateManager(model);

            var category = new Category { Id = 77, PrincipalId = 777 };
            var valueBuffer = new ValueBuffer(new object[] { 77, "Bjork", 777 });

            var entry = stateManager.StartTrackingFromQuery(categoryType, category, valueBuffer, null);

            Assert.Same(entry, stateManager.StartTrackingFromQuery(categoryType, category, valueBuffer, null));
        }

        [Fact]
        public void StartTracking_throws_for_invalid_entity_key()
        {
            var model = BuildModel();
            var stateManager = CreateStateManager(model);

            var entry = stateManager.GetOrCreateEntry(new Dogegory { Id = null });

            Assert.Equal(
                CoreStrings.InvalidKeyValue("Dogegory"),
                Assert.Throws<InvalidOperationException>(
                    () => stateManager.StartTracking(entry)).Message);
        }

        [Fact]
        public void State_manager_switches_out_of_single_query_mode_when_second_query_begins()
        {
            var model = BuildModel();
            var categoryType = model.FindEntityType(typeof(Category));
            var stateManager = CreateStateManager(model);

            Assert.Equal(TrackingQueryMode.Simple, stateManager.GetTrackingQueryMode(categoryType));

            stateManager.BeginTrackingQuery();

            Assert.Equal(TrackingQueryMode.Simple, stateManager.GetTrackingQueryMode(categoryType));

            stateManager.StartTrackingFromQuery(
                categoryType,
                new Category { Id = 77, PrincipalId = 777 },
                new ValueBuffer(new object[] { 77, "Bjork", 777 }), null);

            Assert.Equal(TrackingQueryMode.Simple, stateManager.GetTrackingQueryMode(categoryType));

            stateManager.StartTrackingFromQuery(
                categoryType,
                new Category { Id = 78, PrincipalId = 778 },
                new ValueBuffer(new object[] { 78, "Beck", 778 }), null);

            Assert.Equal(TrackingQueryMode.Simple, stateManager.GetTrackingQueryMode(categoryType));

            stateManager.BeginTrackingQuery();

            Assert.Equal(TrackingQueryMode.Multiple, stateManager.GetTrackingQueryMode(categoryType));

            stateManager.StartTrackingFromQuery(
                categoryType,
                new Category { Id = 79, PrincipalId = 779 },
                new ValueBuffer(new object[] { 79, "Bush", 779 }), null);

            Assert.Equal(TrackingQueryMode.Multiple, stateManager.GetTrackingQueryMode(categoryType));
        }

        [Fact]
        public void State_manager_switches_out_of_single_query_mode_when_entity_has_self_refs()
        {
            var model = BuildModel();
            var widgetType = model.FindEntityType(typeof(Widget));
            var stateManager = CreateStateManager(model);

            Assert.Equal(TrackingQueryMode.Single, stateManager.GetTrackingQueryMode(widgetType));

            stateManager.BeginTrackingQuery();

            Assert.Equal(TrackingQueryMode.Single, stateManager.GetTrackingQueryMode(widgetType));
        }

        [Fact]
        public void State_manager_switches_out_of_single_query_mode_when_entity_included()
        {
            var model = BuildModel();
            var productType = model.FindEntityType(typeof(Product));
            var categoryType = model.FindEntityType(typeof(Category));
            var stateManager = CreateStateManager(model);

            Assert.Equal(TrackingQueryMode.Simple, stateManager.GetTrackingQueryMode(categoryType));

            stateManager.BeginTrackingQuery();

            Assert.Equal(TrackingQueryMode.Simple, stateManager.GetTrackingQueryMode(categoryType));

            Assert.Equal(TrackingQueryMode.Single, stateManager.GetTrackingQueryMode(productType));

            Assert.Equal(TrackingQueryMode.Single, stateManager.GetTrackingQueryMode(categoryType));
        }

        [Fact]
        public void State_manager_switches_out_of_single_query_mode_when_tracked_state_changes_to_Modified()
        {
            var model = BuildModel();
            var categoryType = model.FindEntityType(typeof(Category));
            var stateManager = CreateStateManager(model);

            Assert.Equal(TrackingQueryMode.Simple, stateManager.GetTrackingQueryMode(categoryType));

            stateManager.BeginTrackingQuery();

            Assert.Equal(TrackingQueryMode.Simple, stateManager.GetTrackingQueryMode(categoryType));

            stateManager.StartTrackingFromQuery(
                categoryType,
                new Category { Id = 77, PrincipalId = 777 },
                new ValueBuffer(new object[] { 77, "Bjork", 777 }), null);

            Assert.Equal(TrackingQueryMode.Simple, stateManager.GetTrackingQueryMode(categoryType));

            var entry = stateManager.StartTrackingFromQuery(
                categoryType,
                new Category { Id = 78, PrincipalId = 778 },
                new ValueBuffer(new object[] { 78, "Beck", 778 }), null);

            Assert.Equal(TrackingQueryMode.Simple, stateManager.GetTrackingQueryMode(categoryType));

            entry.SetEntityState(EntityState.Modified);

            Assert.Equal(TrackingQueryMode.Multiple, stateManager.GetTrackingQueryMode(categoryType));
        }

        [Fact]
        public void State_manager_switches_out_of_single_query_mode_when_tracked_state_changes_to_Added()
        {
            var model = BuildModel();
            var categoryType = model.FindEntityType(typeof(Category));
            var stateManager = CreateStateManager(model);

            Assert.Equal(TrackingQueryMode.Simple, stateManager.GetTrackingQueryMode(categoryType));

            stateManager.BeginTrackingQuery();

            Assert.Equal(TrackingQueryMode.Simple, stateManager.GetTrackingQueryMode(categoryType));

            stateManager.StartTrackingFromQuery(
                categoryType,
                new Category { Id = 77, PrincipalId = 777 },
                new ValueBuffer(new object[] { 77, "Bjork", null }), null);

            Assert.Equal(TrackingQueryMode.Simple, stateManager.GetTrackingQueryMode(categoryType));

            var entry = stateManager.StartTrackingFromQuery(
                categoryType,
                new Category { Id = 78, PrincipalId = 778 },
                new ValueBuffer(new object[] { 78, "Beck", 778 }), null);

            Assert.Equal(TrackingQueryMode.Simple, stateManager.GetTrackingQueryMode(categoryType));

            entry.SetEntityState(EntityState.Added);

            Assert.Equal(TrackingQueryMode.Multiple, stateManager.GetTrackingQueryMode(categoryType));
        }

        [Fact]
        public void State_manager_does_not_switch_out_of_single_query_mode_when_getting_existing_entry()
        {
            var model = BuildModel();
            var categoryType = model.FindEntityType(typeof(Category));
            var stateManager = CreateStateManager(model);

            Assert.Equal(TrackingQueryMode.Simple, stateManager.GetTrackingQueryMode(categoryType));

            stateManager.BeginTrackingQuery();

            Assert.Equal(TrackingQueryMode.Simple, stateManager.GetTrackingQueryMode(categoryType));

            var entry = stateManager.StartTrackingFromQuery(
                categoryType,
                new Category { Id = 77, PrincipalId = 777 },
                new ValueBuffer(new object[] { 77, "Bjork", 777 }), null);

            Assert.Equal(TrackingQueryMode.Simple, stateManager.GetTrackingQueryMode(categoryType));

            stateManager.StartTrackingFromQuery(
                categoryType,
                new Category { Id = 78, PrincipalId = 778 },
                new ValueBuffer(new object[] { 78, "Beck", 778 }), null);

            Assert.Equal(TrackingQueryMode.Simple, stateManager.GetTrackingQueryMode(categoryType));

            stateManager.GetOrCreateEntry(entry.Entity);

            Assert.Equal(TrackingQueryMode.Simple, stateManager.GetTrackingQueryMode(categoryType));
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
            var category = new Category { Id = 1, PrincipalId = 777 };

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
            var category = new Category { Id = 1, PrincipalId = 777 };

            var entry = stateManager.GetOrCreateEntry(category);
            stateManager.StartTracking(entry);
            stateManager.StopTracking(entry);

            var entry2 = stateManager.GetOrCreateEntry(category);
            Assert.NotSame(entry, entry2);

            stateManager.StartTracking(entry2);
        }

        [Fact]
        public void StopTracking_releases_reference_to_entry()
        {
            var stateManager = CreateStateManager(BuildModel());
            var category = new Category { Id = 1, PrincipalId = 777 };

            var entry = stateManager.GetOrCreateEntry(category);
            stateManager.StartTracking(entry);
            stateManager.StopTracking(entry);

            var entry2 = stateManager.GetOrCreateEntry(category);
            stateManager.StartTracking(entry2);

            Assert.NotSame(entry, entry2);
            Assert.Equal(EntityState.Detached, entry.EntityState);
        }

        [Fact]
        public void Throws_on_attempt_to_start_tracking_different_entities_with_same_identity()
        {
            var stateManager = CreateStateManager(BuildModel());
            var category1 = new Category { Id = 7, PrincipalId = 777 };
            var category2 = new Category { Id = 7, PrincipalId = 778 };

            var entry1 = stateManager.GetOrCreateEntry(category1);
            var entry2 = stateManager.GetOrCreateEntry(category2);

            stateManager.StartTracking(entry1);

            Assert.Equal(
                CoreStrings.IdentityConflict("Category"),
                Assert.Throws<InvalidOperationException>(() => stateManager.StartTracking(entry2)).Message);
        }

        [Fact]
        public void Throws_on_attempt_to_start_tracking_entity_with_null_key()
        {
            var stateManager = CreateStateManager(BuildModel());
            var entity = new Dogegory();

            var entry = stateManager.GetOrCreateEntry(entity);

            Assert.Equal(
                CoreStrings.InvalidKeyValue("Dogegory"),
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
                CoreStrings.WrongStateManager(nameof(Category)),
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

            stateManager.StartTracking(stateManager.GetOrCreateEntry(new Category { Id = 77, PrincipalId = 777 }));
            stateManager.StartTracking(stateManager.GetOrCreateEntry(new Category { Id = 78, PrincipalId = 778 }));
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
                .AddSingleton(listeners[0].Object)
                .AddSingleton(listeners[1].Object)
                .AddSingleton(listeners[2].Object);

            var contextServices = TestHelpers.Instance.CreateContextServices(services, BuildModel());

            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var entry = stateManager.GetOrCreateEntry(new Category { Id = 77, PrincipalId = 777 });
            entry.SetEntityState(EntityState.Added);

            foreach (var listener in listeners)
            {
                listener.Verify(m => m.StateChanging(entry, It.IsAny<EntityState>()), Times.Once);
                listener.Verify(m => m.StateChanged(entry, It.IsAny<EntityState>(), false), Times.Once);

                listener.Verify(m => m.StateChanging(entry, EntityState.Added), Times.Once);
                listener.Verify(m => m.StateChanged(entry, EntityState.Detached, false), Times.Once);
            }

            entry.SetEntityState(EntityState.Modified);

            foreach (var listener in listeners)
            {
                listener.Verify(m => m.StateChanging(entry, It.IsAny<EntityState>()), Times.Exactly(2));
                listener.Verify(m => m.StateChanged(entry, It.IsAny<EntityState>(), false), Times.Exactly(2));

                listener.Verify(m => m.StateChanging(entry, EntityState.Modified), Times.Once);
                listener.Verify(m => m.StateChanged(entry, EntityState.Detached, false), Times.Once);
            }
        }

        [Fact]
        public void DetectChanges_is_called_for_all_tracked_entities_and_returns_true_if_any_changes_detected()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<IChangeDetector, ChangeDetectorProxy>(),
                BuildModel());

            var stateManager = contextServices.GetRequiredService<IStateManager>();

            var entry1 = stateManager.GetOrCreateEntry(new Category { Id = 77, Name = "Beverages", PrincipalId = 777 });
            var entry2 = stateManager.GetOrCreateEntry(new Category { Id = 78, Name = "Foods", PrincipalId = 778 });
            var entry3 = stateManager.GetOrCreateEntry(new Category { Id = 79, Name = "Stuff", PrincipalId = 779 });

            entry1.SetEntityState(EntityState.Unchanged);
            entry2.SetEntityState(EntityState.Unchanged);
            entry3.SetEntityState(EntityState.Unchanged);

            var changeDetector = (ChangeDetectorProxy)contextServices.GetRequiredService<IChangeDetector>();

            changeDetector.DetectChanges(stateManager);

            Assert.Equal(new[] { 77, 78, 79 }, changeDetector.Entries.Select(e => ((Category)e.Entity).Id).ToArray());

            ((Category)entry2.Entity).Name = "Snacks";

            changeDetector.DetectChanges(stateManager);

            Assert.Equal(new[] { 77, 78, 79, 77, 78, 79 }, changeDetector.Entries.Select(e => ((Category)e.Entity).Id).ToArray());
        }

        internal class ChangeDetectorProxy : ChangeDetector
        {
            public List<InternalEntityEntry> Entries { get; } = new List<InternalEntityEntry>();

            public override void DetectChanges(InternalEntityEntry entry)
            {
                Entries.Add(entry);

                base.DetectChanges(entry);
            }
        }

        [Fact]
        public void AcceptAllChanges_processes_all_tracked_entities()
        {
            var stateManager = CreateStateManager(BuildModel());

            var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
            var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

            var entry1 = stateManager.GetOrCreateEntry(new Category { Id = 77, PrincipalId = 777 });
            var entry2 = stateManager.GetOrCreateEntry(new Category { Id = 78, PrincipalId = 778 });
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
            var categoryEntry4 = stateManager.StartTracking(stateManager.GetOrCreateEntry(new Category { Id = 4, PrincipalId = 0 }));
            var productEntry1 = stateManager.StartTracking(stateManager.GetOrCreateEntry(new Product { Id = Guid.NewGuid(), DependentId = 77 }));
            var productEntry2 = stateManager.StartTracking(stateManager.GetOrCreateEntry(new Product { Id = Guid.NewGuid(), DependentId = 77 }));
            var productEntry3 = stateManager.StartTracking(stateManager.GetOrCreateEntry(new Product { Id = Guid.NewGuid(), DependentId = 78 }));
            var productEntry4 = stateManager.StartTracking(stateManager.GetOrCreateEntry(new Product { Id = Guid.NewGuid(), DependentId = 78 }));
            var productEntry5 = stateManager.StartTracking(stateManager.GetOrCreateEntry(new Product { Id = Guid.NewGuid(), DependentId = null }));

            var fk = model.FindEntityType(typeof(Product)).GetForeignKeys().Single();

            Assert.Equal(
                new[] { productEntry1, productEntry2 },
                stateManager.GetDependents(categoryEntry1, fk).ToArray());

            Assert.Equal(
                new[] { productEntry3, productEntry4 },
                stateManager.GetDependents(categoryEntry2, fk).ToArray());

            Assert.Empty(stateManager.GetDependents(categoryEntry3, fk).ToArray());
            Assert.Empty(stateManager.GetDependents(categoryEntry4, fk).ToArray());
        }

        [Fact] // Issue #743
        public void Throws_when_instance_of_unmapped_derived_type_is_used()
        {
            var model = BuildModel();
            var stateManager = CreateStateManager(model);
            Assert.Equal(CoreStrings.EntityTypeNotFound(typeof(SpecialProduct).Name),
                Assert.Throws<InvalidOperationException>(() => stateManager.GetOrCreateEntry(new SpecialProduct())).Message);
        }

        private static IStateManager CreateStateManager(IModel model)
            => TestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

        public class Widget
        {
            public int Id { get; set; }

            public int? ParentWidgetId { get; set; }
            public Widget ParentWidget { get; set; }

            public List<Widget> ChildWidgets { get; set; }
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

        private class SpecialProduct : Product
        {
        }

        private class Dogegory
        {
            public string Id { get; set; }
        }

        private static IMutableModel BuildModel()
        {
            var builder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            var model = builder.Model;

            builder.Entity<Product>().HasOne<Category>().WithOne()
                .HasForeignKey<Product>(e => e.DependentId)
                .HasPrincipalKey<Category>(e => e.PrincipalId);

            builder.Entity<Widget>()
                .HasOne(e => e.ParentWidget)
                .WithMany(e => e.ChildWidgets)
                .HasForeignKey(e => e.ParentWidgetId);

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
