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
                    () => new StateManager(null, new Mock<ActiveIdentityGenerators>().Object, Enumerable.Empty<IEntityStateListener>()))
                    .ParamName);
            Assert.Equal(
                "identityGenerators",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(
                    () => new StateManager(new Model(), null, Enumerable.Empty<IEntityStateListener>())).ParamName);
            Assert.Equal(
                "entityStateListeners",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(
                    () => new StateManager(new Model(), new Mock<ActiveIdentityGenerators>().Object, null)).ParamName);

            var stateManager = new StateManager(
                new Model(), new Mock<ActiveIdentityGenerators>().Object, Enumerable.Empty<IEntityStateListener>());

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

        [Fact]
        public void Can_get_existing_entry_if_entity_is_already_tracked_otherwise_null()
        {
            var stateManager =
                new StateManager(BuildModel(), new Mock<ActiveIdentityGenerators>().Object, Enumerable.Empty<IEntityStateListener>());

            var category = new Category();
            var stateEntry = stateManager.GetOrCreateEntry(category);

            stateManager.StartTracking(stateEntry);

            var stateEntry2 = stateManager.GetOrCreateEntry(category);

            Assert.Same(stateEntry, stateEntry2);
            Assert.Equal(EntityState.Unchanged, stateEntry.EntityState);
        }

        [Fact]
        public void Will_get_new_entry_if_another_entity_with_the_same_key_is_already_tracked()
        {
            var stateManager =
                new StateManager(BuildModel(), new Mock<ActiveIdentityGenerators>().Object, Enumerable.Empty<IEntityStateListener>());

            Assert.NotSame(
                stateManager.GetOrCreateEntry(new Category { Id = 77 }),
                stateManager.GetOrCreateEntry(new Category { Id = 77 }));
        }

        [Fact]
        public void Can_get_all_entities()
        {
            var stateManager =
                new StateManager(BuildModel(), new Mock<ActiveIdentityGenerators>().Object, Enumerable.Empty<IEntityStateListener>());

            stateManager.StartTracking(stateManager.GetOrCreateEntry(new Category { Id = 77 }));
            stateManager.StartTracking(stateManager.GetOrCreateEntry(new Category { Id = 78 }));
            stateManager.StartTracking(stateManager.GetOrCreateEntry(new Product { Id = 77 }));
            stateManager.StartTracking(stateManager.GetOrCreateEntry(new Product { Id = 78 }));

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
                new[] { 77, 78 },
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
            Assert.Same(
                model,
                new StateManager(model, new Mock<ActiveIdentityGenerators>().Object, Enumerable.Empty<IEntityStateListener>()).Model);
        }

        [Fact]
        public void Listeners_are_notified_when_entity_states_change()
        {
            var listeners = new[] { new Mock<IEntityStateListener>(), new Mock<IEntityStateListener>(), new Mock<IEntityStateListener>() };

            var stateManager =
                new StateManager(BuildModel(), new Mock<ActiveIdentityGenerators>().Object, listeners.Select(m => m.Object));

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
            public int Id { get; set; }
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

            return model;
        }

        #endregion
    }
}
