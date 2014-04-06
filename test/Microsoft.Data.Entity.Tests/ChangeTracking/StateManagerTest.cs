// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class StateManagerTest
    {
        [Fact]
        public void Can_get_existing_entry_if_entity_is_already_tracked_otherwise_new_entry()
        {
            var stateManager = CreateStateManager(BuildModel());
            var category = new Category();

            var stateEntry = stateManager.GetOrCreateEntry(category);

            stateManager.StartTracking(stateEntry);

            var stateEntry2 = stateManager.GetOrCreateEntry(category);

            Assert.Same(stateEntry, stateEntry2);
            Assert.Equal(EntityState.Unknown, stateEntry.EntityState);
        }

        [Fact]
        public void Can_get_existing_entry_if_entity_in_value_buffer_is_already_tracked_otherwise_new_entry()
        {
            var model = BuildModel();
            var categoryType = model.GetEntityType(typeof(Category));
            var stateManager = CreateStateManager(model);

            var stateEntry = stateManager.GetOrMaterializeEntry(categoryType, new object[] { 77, "Bjork" });

            Assert.Equal(EntityState.Unchanged, stateEntry.EntityState);
            Assert.Same(stateEntry, stateManager.GetOrMaterializeEntry(categoryType, new object[] { 77, "Bjork" }));

            stateEntry.SetEntityStateAsync(EntityState.Modified, CancellationToken.None).Wait();

            Assert.Same(stateEntry, stateManager.GetOrMaterializeEntry(categoryType, new object[] { 77, "Bjork" }));
            Assert.Equal(EntityState.Modified, stateEntry.EntityState);

            Assert.NotSame(stateEntry, stateManager.GetOrMaterializeEntry(categoryType, new object[] { 78, "Bjork" }));
        }

        [Fact]
        public void Can_explicitly_create_new_entry_for_shadow_state_entity()
        {
            var model = BuildModel();
            var stateManager = CreateStateManager(model);
            var entityType = model.GetEntityType("Location");
            var stateEntry = stateManager.CreateNewEntry(entityType);
            stateEntry.SetPropertyValue(entityType.GetKey().Properties.Single(), 42);

            Assert.Equal(EntityState.Unknown, stateEntry.EntityState);
            Assert.Null(stateEntry.Entity);
            Assert.Equal(0, stateManager.StateEntries.Count());

            stateManager.StartTracking(stateEntry);

            Assert.Equal(1, stateManager.StateEntries.Count());
        }

        [Fact]
        public void Can_explicitly_create_new_entry_for_normal_state_entity()
        {
            var model = BuildModel();
            var stateManager = CreateStateManager(model);

            var stateEntry = stateManager.CreateNewEntry(model.GetEntityType(typeof(Category)));

            Assert.Equal(EntityState.Unknown, stateEntry.EntityState);
            Assert.IsType<Category>(stateEntry.Entity);
            Assert.Equal(0, stateManager.StateEntries.Count());

            stateManager.StartTracking(stateEntry);

            Assert.Equal(1, stateManager.StateEntries.Count());
        }

        [Fact]
        public void Can_get_existing_entry_even_if_state_not_yet_set()
        {
            var stateManager = CreateStateManager(BuildModel());
            var category = new Category();

            var stateEntry = stateManager.GetOrCreateEntry(category);
            var stateEntry2 = stateManager.GetOrCreateEntry(category);

            Assert.Same(stateEntry, stateEntry2);
            Assert.Equal(EntityState.Unknown, stateEntry.EntityState);
        }

        [Fact]
        public void Can_stop_tracking_and_then_start_tracking_again()
        {
            var stateManager = CreateStateManager(BuildModel());
            var category = new Category();

            var stateEntry = stateManager.GetOrCreateEntry(category);
            stateManager.StartTracking(stateEntry);
            stateManager.StopTracking(stateEntry);
            stateManager.StartTracking(stateEntry);

            var stateEntry2 = stateManager.GetOrCreateEntry(category);
            Assert.Same(stateEntry, stateEntry2);
        }

        [Fact]
        public void Can_stop_tracking_and_then_start_tracking_using_a_new_state_entry()
        {
            var stateManager = CreateStateManager(BuildModel());
            var category = new Category();

            var stateEntry = stateManager.GetOrCreateEntry(category);
            stateManager.StartTracking(stateEntry);
            stateManager.StopTracking(stateEntry);

            var stateEntry2 = stateManager.GetOrCreateEntry(category);
            Assert.NotSame(stateEntry, stateEntry2);

            stateManager.StartTracking(stateEntry2);
        }

        [Fact]
        public void Throws_on_attempt_to_start_tracking_same_entity_with_two_entries()
        {
            var stateManager = CreateStateManager(BuildModel());
            var category = new Category();

            var stateEntry = stateManager.GetOrCreateEntry(category);
            stateManager.StartTracking(stateEntry);
            stateManager.StopTracking(stateEntry);

            var stateEntry2 = stateManager.GetOrCreateEntry(category);
            stateManager.StartTracking(stateEntry2);

            Assert.Equal(
                Strings.FormatMultipleStateEntries("Category"),
                Assert.Throws<InvalidOperationException>(() => stateManager.StartTracking(stateEntry)).Message);
        }

        [Fact]
        public void Throws_on_attempt_to_start_tracking_with_wrong_manager()
        {
            var model = BuildModel();
            var stateManager = CreateStateManager(model);
            var stateManager2 = CreateStateManager(model);

            var stateEntry = stateManager.GetOrCreateEntry(new Category());

            Assert.Equal(
                Strings.FormatWrongStateManager("Category"),
                Assert.Throws<InvalidOperationException>(() => stateManager2.StartTracking(stateEntry)).Message);
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

            Assert.Equal(4, stateManager.StateEntries.Count());

            Assert.Equal(
                new[] { 77, 78 },
                stateManager.StateEntries
                    .Select(e => e.Entity)
                    .OfType<Category>()
                    .Select(e => e.Id)
                    .OrderBy(k => k)
                    .ToArray());

            Assert.Equal(
                new[] { productId2, productId1 },
                stateManager.StateEntries
                    .Select(e => e.Entity)
                    .OfType<Product>()
                    .Select(e => e.Id)
                    .OrderBy(k => k)
                    .ToArray());
        }

        [Fact]
        public void Can_get_model()
        {
            var model = BuildModel();
            var stateManager = CreateStateManager(model);

            Assert.Same(model, stateManager.Model);
        }

        [Fact]
        public void Listeners_are_notified_when_entity_states_change()
        {
            var listeners = new[] { new Mock<IEntityStateListener>(), new Mock<IEntityStateListener>(), new Mock<IEntityStateListener>() };

            var configMock = new Mock<ContextConfiguration> { CallBase = true };
            var stateManager = CreateStateManager(BuildModel(), configMock);

            configMock.Setup(m => m.EntityStateListeners).Returns(listeners.Select(m => m.Object));
            configMock.Setup(m => m.StateEntryNotifier).Returns(new StateEntryNotifier(listeners.Select(m => m.Object)));

            var entry = stateManager.GetOrCreateEntry(new Category { Id = 77 });
            entry.SetEntityStateAsync(EntityState.Added, CancellationToken.None).Wait();

            foreach (var listener in listeners)
            {
                listener.Verify(m => m.StateChanging(entry, It.IsAny<EntityState>()), Times.Once);
                listener.Verify(m => m.StateChanged(entry, It.IsAny<EntityState>()), Times.Once);

                listener.Verify(m => m.StateChanging(entry, EntityState.Added), Times.Once);
                listener.Verify(m => m.StateChanged(entry, EntityState.Unknown), Times.Once);
            }

            entry.SetEntityStateAsync(EntityState.Modified, CancellationToken.None).Wait();

            foreach (var listener in listeners)
            {
                listener.Verify(m => m.StateChanging(entry, It.IsAny<EntityState>()), Times.Exactly(2));
                listener.Verify(m => m.StateChanged(entry, It.IsAny<EntityState>()), Times.Exactly(2));

                listener.Verify(m => m.StateChanging(entry, EntityState.Modified), Times.Once);
                listener.Verify(m => m.StateChanged(entry, EntityState.Unknown), Times.Once);
            }
        }

        [Fact]
        public void DetectChanges_is_called_for_all_tracked_entities_and_returns_true_if_any_changes_detected()
        {
            var model = BuildModel();
            var configMock = new Mock<ContextConfiguration> { CallBase = true };
            var stateManager = CreateStateManager(model, configMock);

            var entryMock1 = CreateEntryMock(model, configMock, changes: false, key: 1);
            var entryMock2 = CreateEntryMock(model, configMock, changes: false, key: 2);
            var entryMock3 = CreateEntryMock(model, configMock, changes: false, key: 3);

            stateManager.StartTracking(entryMock1.Object);
            stateManager.StartTracking(entryMock2.Object);
            stateManager.StartTracking(entryMock3.Object);

            Assert.False(stateManager.DetectChanges());

            entryMock1.Verify(m => m.DetectChanges());
            entryMock2.Verify(m => m.DetectChanges());
            entryMock3.Verify(m => m.DetectChanges());

            var entryMock4 = CreateEntryMock(model, configMock, changes: true, key: 4);
            var entryMock5 = CreateEntryMock(model, configMock, changes: false, key: 5);

            stateManager.StartTracking(entryMock4.Object);
            stateManager.StartTracking(entryMock5.Object);

            Assert.True(stateManager.DetectChanges());

            entryMock1.Verify(m => m.DetectChanges());
            entryMock2.Verify(m => m.DetectChanges());
            entryMock3.Verify(m => m.DetectChanges());
            entryMock4.Verify(m => m.DetectChanges());
            entryMock5.Verify(m => m.DetectChanges());
        }

        private static Mock<StateEntry> CreateEntryMock(IModel model, Mock<ContextConfiguration> configMock, bool changes, int key)
        {
            var entryMock = new Mock<StateEntry>();
            entryMock.Setup(m => m.Configuration).Returns(configMock.Object);
            entryMock.Setup(m => m.EntityType).Returns(model.GetEntityType("Location"));
            entryMock.Setup(m => m.GetPropertyValue(It.IsAny<IProperty>())).Returns(key);
            entryMock.Setup(m => m.DetectChanges()).Returns(changes);

            return entryMock;
        }

        private static StateManager CreateStateManager(IModel model, Mock<ContextConfiguration> configMock = null)
        {
            configMock = configMock ?? new Mock<ContextConfiguration> { CallBase = true };
            configMock.Object.Initialize(new EntityConfigurationBuilder().BuildConfiguration().ServiceProvider);

            configMock.Setup(m => m.Model).Returns(model);
            configMock.Setup(m => m.StateEntryNotifier).Returns(Mock.Of<StateEntryNotifier>());

            var stateManager = new StateManager(
                configMock.Object,
                new StateEntryFactory(configMock.Object, new EntityMaterializerSource(new MemberMapper(new FieldMatcher()))),
                new EntityKeyFactorySource(),
                new StateEntrySubscriber());

            configMock.Setup(m => m.StateManager).Returns(stateManager);

            return stateManager;
        }

        #region Fixture

        private class Category
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class Product
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
        }

        private static IModel BuildModel()
        {
            var model = new Model();
            var builder = new ModelBuilder(model);

            builder.Entity<Product>();
            builder.Entity<Category>();

            new SimpleTemporaryConvention().Apply(model);

            var locationType = new EntityType("Location");
            var idProperty = locationType.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false);
            locationType.AddProperty("Planet", typeof(string), shadowProperty: true, concurrencyToken: false);
            locationType.SetKey(idProperty);
            model.AddEntityType(locationType);

            return model;
        }

        #endregion
    }
}
