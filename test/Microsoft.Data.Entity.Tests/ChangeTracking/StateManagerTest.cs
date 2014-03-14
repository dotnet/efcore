// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class StateManagerTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                "model",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(
                    () => new StateManager(
                        null, Mock.Of<ActiveIdentityGenerators>(), Enumerable.Empty<IEntityStateListener>(),
                        Mock.Of<EntityKeyFactorySource>(), Mock.Of<StateEntryFactory>(),
                        Mock.Of<ClrPropertyGetterSource>(), Mock.Of<ClrPropertySetterSource>())).ParamName);

            Assert.Equal(
                "identityGenerators",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(
                    () => new StateManager(Mock.Of<RuntimeModel>(), null, Enumerable.Empty<IEntityStateListener>(),
                        Mock.Of<EntityKeyFactorySource>(), Mock.Of<StateEntryFactory>(),
                        Mock.Of<ClrPropertyGetterSource>(), Mock.Of<ClrPropertySetterSource>())).ParamName);

            Assert.Equal(
                "entityStateListeners",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(
                    () => new StateManager(Mock.Of<RuntimeModel>(), Mock.Of<ActiveIdentityGenerators>(), null,
                        Mock.Of<EntityKeyFactorySource>(), Mock.Of<StateEntryFactory>(),
                        Mock.Of<ClrPropertyGetterSource>(), Mock.Of<ClrPropertySetterSource>())).ParamName);

            Assert.Equal(
                "entityKeyFactorySource",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(
                    () => new StateManager(Mock.Of<RuntimeModel>(), Mock.Of<ActiveIdentityGenerators>(),
                        Enumerable.Empty<IEntityStateListener>(), null, Mock.Of<StateEntryFactory>(),
                        Mock.Of<ClrPropertyGetterSource>(), Mock.Of<ClrPropertySetterSource>())).ParamName);

            Assert.Equal(
                "stateEntryFactory",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(
                    () => new StateManager(Mock.Of<RuntimeModel>(), Mock.Of<ActiveIdentityGenerators>(),
                        Enumerable.Empty<IEntityStateListener>(), Mock.Of<EntityKeyFactorySource>(), null,
                        Mock.Of<ClrPropertyGetterSource>(), Mock.Of<ClrPropertySetterSource>())).ParamName);

            var stateManager = CreateStateManager(Mock.Of<RuntimeModel>());

            Assert.Equal(
                "entity",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => stateManager.GetOrCreateEntry(null)).ParamName);

            Assert.Equal(
                "entry",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => stateManager.StartTracking(null)).ParamName);

            Assert.Equal(
                "entry",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => stateManager.StopTracking(null)).ParamName);

            Assert.Equal(
                "property",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => stateManager.GetIdentityGenerator(null)).ParamName);
        }

        private static StateManager CreateStateManager(RuntimeModel model)
        {
            return new StateManager(
                model,
                Mock.Of<ActiveIdentityGenerators>(),
                Enumerable.Empty<IEntityStateListener>(),
                new EntityKeyFactorySource(),
                new StateEntryFactory(),
                new ClrPropertyGetterSource(), 
                new ClrPropertySetterSource());
        }

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

            var stateManager = new StateManager(
                BuildModel(),
                Mock.Of<ActiveIdentityGenerators>(),
                listeners.Select(m => m.Object),
                new EntityKeyFactorySource(),
                new StateEntryFactory(),
                new ClrPropertyGetterSource(), 
                new ClrPropertySetterSource());

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

        private static RuntimeModel BuildModel()
        {
            var model = new Model();
            var builder = new ModelBuilder(model);

            builder.Entity<Product>();
            builder.Entity<Category>();

            new SimpleTemporaryConvention().Apply(model);

            var locationType = new EntityType("Location");
            var idProperty = new Property("Id", typeof(int), hasClrProperty: false);
            locationType.AddProperty(idProperty);
            locationType.AddProperty(new Property("Planet", typeof(string), hasClrProperty: false));
            locationType.SetKey(new Key(new []{ idProperty }));
            model.AddEntityType(locationType);

            return new RuntimeModel(model, new EntityKeyFactorySource());
        }

        #endregion
    }
}
