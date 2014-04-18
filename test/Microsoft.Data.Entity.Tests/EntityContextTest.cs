// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.DependencyInjection.Advanced;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntityContextTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            using (var context = new EarlyLearningCenter())
            {
                Assert.Equal(
                    "entity",
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Assert.Throws<ArgumentNullException>(() => context.Add<Random>(null)).ParamName);
                Assert.Equal(
                    "entity",
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Assert.ThrowsAsync<ArgumentNullException>(() => context.AddAsync<Random>(null)).Result.ParamName);
                Assert.Equal(
                    "entity",
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Assert.Throws<ArgumentNullException>(
                        () => context.AddAsync<Random>(null, new CancellationToken()).GetAwaiter().GetResult()).ParamName);
                Assert.Equal(
                    "entity",
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Assert.Throws<ArgumentNullException>(() => context.Update<Random>(null)).ParamName);
                Assert.Equal(
                    "entity",
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Assert.ThrowsAsync<ArgumentNullException>(() => context.UpdateAsync<Random>(null)).Result.ParamName);
                Assert.Equal(
                    "entity",
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Assert.ThrowsAsync<ArgumentNullException>(() => context.UpdateAsync<Random>(null, new CancellationToken())).Result.ParamName);
            }
        }

        [Fact]
        public void Each_context_gets_new_scoped_context_configuration()
        {
            var configuration = new EntityConfigurationBuilder()
                .WithServices(s => s.UseStateManager<FakeStateManager>())
                .BuildConfiguration();

            ContextConfiguration config1;
            using (var context = new EntityContext(configuration))
            {
                config1 = context.Configuration;
                Assert.Same(config1, context.Configuration);
            }

            using (var context = new EntityContext(configuration))
            {
                var config2 = context.Configuration;
                Assert.Same(config2, context.Configuration);

                Assert.NotSame(config1, config2);
            }
        }

        [Fact]
        public void Each_context_gets_new_scoped_StateManager()
        {
            var configuration = new EntityConfigurationBuilder()
                .WithServices(s => s.UseStateManager<FakeStateManager>())
                .BuildConfiguration();

            StateManager stateManager1;
            using (var context = new EntityContext(configuration))
            {
                stateManager1 = context.ChangeTracker.StateManager;
                Assert.Same(stateManager1, context.ChangeTracker.StateManager);
            }

            using (var context = new EntityContext(configuration))
            {
                var stateManager2 = context.ChangeTracker.StateManager;
                Assert.Same(stateManager2, context.ChangeTracker.StateManager);

                Assert.NotSame(stateManager1, stateManager2);
            }
        }

        [Fact]
        public void SaveChanges_calls_DetectChanges()
        {
            var configuration = new EntityConfigurationBuilder()
                .WithServices(s => s.UseStateManager<FakeStateManager>())
                .BuildConfiguration();

            using (var context = new EntityContext(configuration))
            {
                var stateManager = (FakeStateManager)context.Configuration.Services.StateManager;

                Assert.False(stateManager.DetectChangesCalled);

                context.SaveChanges();

                Assert.True(stateManager.DetectChangesCalled);
            }
        }

        [Fact]
        public void SaveChanges_calls_state_manager_SaveChanges()
        {
            var configuration = new EntityConfigurationBuilder()
                .WithServices(s => s.UseStateManager<FakeStateManager>())
                .BuildConfiguration();

            using (var context = new EntityContext(configuration))
            {
                var stateManager = (FakeStateManager)context.Configuration.Services.StateManager;

                var entryMock = new Mock<StateEntry>();
                entryMock.Setup(m => m.EntityState).Returns(EntityState.Modified);
                stateManager.Entries = new[] { entryMock.Object };

                Assert.False(stateManager.SaveChangesCalled);

                context.SaveChanges();

                Assert.True(stateManager.SaveChangesCalled);
            }
        }

        private class FakeStateManager : StateManager
        {
            public IEnumerable<StateEntry> Entries { get; set; }
            public bool DetectChangesCalled { get; set; }
            public bool SaveChangesCalled { get; set; }

            public override bool DetectChanges()
            {
                DetectChangesCalled = true;
                return false;
            }

            public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
            {
                SaveChangesCalled = true;
                return Task.FromResult(1);
            }

            public override IEnumerable<StateEntry> StateEntries
            {
                get { return Entries ?? Enumerable.Empty<StateEntry>(); }
            }
        }

        [Fact]
        public void Can_add_new_entities_to_context()
        {
            TrackEntitiesTest((c, e) => c.Add(e), (c, e) => c.Add(e), EntityState.Added);
        }

        [Fact]
        public void Can_add_new_entities_to_context_async()
        {
            TrackEntitiesTest((c, e) => c.AddAsync(e).Result, (c, e) => c.AddAsync(e).Result, EntityState.Added);

            TrackEntitiesTest(
                (c, e) => c.AddAsync(e, new CancellationToken()).Result,
                (c, e) => c.AddAsync(e, new CancellationToken()).Result,
                EntityState.Added);
        }

        [Fact]
        public void Can_add_existing_entities_to_context_to_be_updated()
        {
            TrackEntitiesTest((c, e) => c.Update(e), (c, e) => c.Update(e), EntityState.Modified);
        }

        [Fact]
        public void Can_add_existing_entities_to_context_to_be_updated_async()
        {
            TrackEntitiesTest((c, e) => c.UpdateAsync(e).Result, (c, e) => c.UpdateAsync(e).Result, EntityState.Modified);

            TrackEntitiesTest(
                (c, e) => c.UpdateAsync(e, new CancellationToken()).Result,
                (c, e) => c.UpdateAsync(e, new CancellationToken()).Result,
                EntityState.Modified);
        }

        [Fact]
        public void Can_add_existing_entities_to_context_to_be_deleted()
        {
            TrackEntitiesTest((c, e) => c.Delete(e), (c, e) => c.Delete(e), EntityState.Deleted);
        }

        private static void TrackEntitiesTest(
            Func<EntityContext, Category, Category> categoryAdder,
            Func<EntityContext, Product, Product> productAdder, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter())
            {
                var category1 = new Category { Id = 1, Name = "Beverages" };
                var category2 = new Category { Id = 2, Name = "Foods" };
                var product1 = new Product { Id = 1, Name = "Marmite", Price = 7.99m };
                var product2 = new Product { Id = 2, Name = "Bovril", Price = 4.99m };

                Assert.Same(category1, categoryAdder(context, category1));
                Assert.Same(category2, categoryAdder(context, category2));
                Assert.Same(product1, productAdder(context, product1));
                Assert.Same(product2, productAdder(context, product2));

                var categoryEntry = context.ChangeTracker.Entry(category1);
                Assert.Same(category1, categoryEntry.Entity);
                Assert.Equal(expectedState, categoryEntry.State);

                categoryEntry = context.ChangeTracker.Entry(category2);
                Assert.Same(category2, categoryEntry.Entity);
                Assert.Equal(expectedState, categoryEntry.State);

                var productEntry = context.ChangeTracker.Entry(product1);
                Assert.Same(product1, productEntry.Entity);
                Assert.Equal(expectedState, productEntry.State);

                productEntry = context.ChangeTracker.Entry(product2);
                Assert.Same(product2, productEntry.Entity);
                Assert.Equal(expectedState, productEntry.State);
            }
        }

        [Fact]
        public void Can_add_new_entities_to_context_with_key_generation()
        {
            TrackEntitiesWithKeyGenerationTest((c, e) => c.Add(e));
        }

        [Fact]
        public void Can_add_new_entities_to_context_with_key_generation_async()
        {
            TrackEntitiesWithKeyGenerationTest((c, e) => c.AddAsync(e).Result);
            TrackEntitiesWithKeyGenerationTest((c, e) => c.AddAsync(e, new CancellationToken()).Result);
        }

        private static void TrackEntitiesWithKeyGenerationTest(Func<EntityContext, TheGu, TheGu> adder)
        {
            using (var context = new EarlyLearningCenter())
            {
                var gu1 = new TheGu { ShirtColor = "Red" };
                var gu2 = new TheGu { ShirtColor = "Still Red" };

                Assert.Same(gu1, adder(context, gu1));
                Assert.Same(gu2, adder(context, gu2));
                Assert.NotEqual(default(Guid), gu1.Id);
                Assert.NotEqual(default(Guid), gu2.Id);
                Assert.NotEqual(gu1.Id, gu2.Id);

                var categoryEntry = context.ChangeTracker.Entry(gu1);
                Assert.Same(gu1, categoryEntry.Entity);
                Assert.Equal(EntityState.Added, categoryEntry.State);

                categoryEntry = context.ChangeTracker.Entry(gu2);
                Assert.Same(gu2, categoryEntry.Entity);
                Assert.Equal(EntityState.Added, categoryEntry.State);
            }
        }

        [Fact]
        public void Context_can_build_model_using_EntitySet_properties()
        {
            using (var context = new EarlyLearningCenter())
            {
                Assert.Equal(
                    new[] { "Category", "Product", "TheGu" },
                    context.Model.EntityTypes.Select(e => e.Name).ToArray());

                var categoryType = context.Model.GetEntityType(typeof(Category));
                Assert.Equal("Id", categoryType.GetKey().Properties.Single().Name);
                Assert.Equal(
                    new[] { "Id", "Name" },
                    categoryType.Properties.Select(p => p.Name).ToArray());

                var productType = context.Model.GetEntityType(typeof(Product));
                Assert.Equal("Id", productType.GetKey().Properties.Single().Name);
                Assert.Equal(
                    new[] { "Id", "Name", "Price" },
                    productType.Properties.Select(p => p.Name).ToArray());

                var guType = context.Model.GetEntityType(typeof(TheGu));
                Assert.Equal("Id", guType.GetKey().Properties.Single().Name);
                Assert.Equal(
                    new[] { "Id", "ShirtColor" },
                    guType.Properties.Select(p => p.Name).ToArray());
            }
        }

        [Fact]
        public void Context_will_use_explicit_model_if_set_in_config()
        {
            var model = new Model();
            model.AddEntityType(new EntityType(typeof(TheGu)));

            var configuration = new EntityConfigurationBuilder().UseModel(model).BuildConfiguration();

            using (var context = new EarlyLearningCenter(configuration))
            {
                Assert.Equal(
                    new[] { "TheGu" },
                    context.Model.EntityTypes.Select(e => e.Name).ToArray());
            }
        }

        [Fact]
        public void Context_initializes_all_EntitySet_properties_with_setters()
        {
            using (var context = new ContextWithSets())
            {
                Assert.NotNull(context.Products);
                Assert.NotNull(context.Categories);
                Assert.NotNull(context.GetGus());
                Assert.Null(context.NoSetter);
            }
        }

        public class ContextWithSets : EntityContext
        {
            private readonly EntitySet<Random> _noSetter = null;

            public EntitySet<Product> Products { get; set; }
            public EntitySet<Category> Categories { get; private set; }
            private EntitySet<TheGu> Gus { get; set; }

            public EntitySet<Random> NoSetter
            {
                get { return _noSetter; }
            }

            public EntitySet<TheGu> GetGus()
            {
                return Gus;
            }
        }

        [Fact]
        public void Set_and_non_generic_set_always_return_same_instance_returns_a_new_EntitySet_for_the_given_type()
        {
            using (var context = new ContextWithSets())
            {
                var entitySet = context.Set<Product>();
                Assert.NotNull(entitySet);
                Assert.Same(entitySet, context.Set<Product>());
                Assert.Same(entitySet, context.Set(typeof(Product)));
                Assert.Same(entitySet, context.Products);
            }
        }

        [Fact]
        public void SaveChanges_doesnt_call_DataStore_when_nothing_is_dirty()
        {
            var store = new Mock<DataStore>();

            var sourceMock = new Mock<DataStoreSource>();
            sourceMock.Setup(m => m.IsAvailable(It.IsAny<ContextConfiguration>())).Returns(true);
            sourceMock.Setup(m => m.IsConfigured(It.IsAny<ContextConfiguration>())).Returns(true);
            sourceMock.Setup(m => m.GetDataStore(It.IsAny<ContextConfiguration>())).Returns(store.Object);

            var configuration = new EntityConfigurationBuilder()
                .WithServices(s => s.ServiceCollection.AddInstance<DataStoreSource>(sourceMock.Object))
                .BuildConfiguration();

            using (var context = new EarlyLearningCenter(configuration))
            {
                context.ChangeTracker.Entry(new Category { Id = 1 }).State = EntityState.Unchanged;
                context.ChangeTracker.Entry(new Category { Id = 2 }).State = EntityState.Unchanged;
                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                context.SaveChanges();
            }

            store.Verify(
                s => s.SaveChangesAsync(It.IsAny<IEnumerable<StateEntry>>(), It.IsAny<IModel>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public void SaveChanges_only_passes_dirty_entries_to_DatStore()
        {
            var passedEntries = new List<StateEntry>();
            var store = new Mock<DataStore>();
            store.Setup(s => s.SaveChangesAsync(It.IsAny<IEnumerable<StateEntry>>(), It.IsAny<IModel>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<StateEntry>, IModel, CancellationToken>((e, m, c) => passedEntries.AddRange(e))
                .Returns(Task.FromResult(3));

            var sourceMock = new Mock<DataStoreSource>();
            sourceMock.Setup(m => m.IsAvailable(It.IsAny<ContextConfiguration>())).Returns(true);
            sourceMock.Setup(m => m.IsConfigured(It.IsAny<ContextConfiguration>())).Returns(true);
            sourceMock.Setup(m => m.GetDataStore(It.IsAny<ContextConfiguration>())).Returns(store.Object);

            var configuration = new EntityConfigurationBuilder()
                .WithServices(s => s.ServiceCollection.AddInstance<DataStoreSource>(sourceMock.Object))
                .BuildConfiguration();

            using (var context = new EarlyLearningCenter(configuration))
            {
                context.ChangeTracker.Entry(new Category { Id = 1 }).State = EntityState.Unchanged;
                context.ChangeTracker.Entry(new Category { Id = 2 }).State = EntityState.Modified;
                context.ChangeTracker.Entry(new Category { Id = 3 }).State = EntityState.Added;
                context.ChangeTracker.Entry(new Category { Id = 4 }).State = EntityState.Deleted;
                Assert.Equal(4, context.ChangeTracker.Entries().Count());

                context.SaveChanges();
            }

            Assert.Equal(3, passedEntries.Count);

            store.Verify(
                s => s.SaveChangesAsync(It.IsAny<IEnumerable<StateEntry>>(), It.IsAny<IModel>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #region Fixture

        public class Category
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
        }

        public class TheGu
        {
            public Guid Id { get; set; }
            public string ShirtColor { get; set; }
        }

        public class EarlyLearningCenter : EntityContext
        {
            public EarlyLearningCenter(EntityConfiguration configuration)
                : base(configuration)
            {
            }

            public EarlyLearningCenter()
            {
            }

            public EntitySet<Product> Products { get; set; }
            public EntitySet<Category> Categories { get; set; }
            public EntitySet<TheGu> Gus { get; set; }
        }

        #endregion
    }
}
