// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class DbContextTest
    {
        [Fact]
        public void Each_context_gets_new_scoped_services()
        {
            var serviceProvider = TestHelpers.CreateServiceProvider();

            IServiceProvider contextServices;
            using (var context = new DbContext(serviceProvider))
            {
                contextServices = ((IDbContextServices)context).ScopedServiceProvider;
                Assert.Same(contextServices, ((IDbContextServices)context).ScopedServiceProvider);
            }

            using (var context = new DbContext(serviceProvider))
            {
                Assert.NotSame(contextServices, ((IDbContextServices)context).ScopedServiceProvider);
            }
        }

        [Fact]
        public void Each_context_gets_new_scoped_services_with_implicit_services()
        {
            IServiceProvider contextServices;
            using (var context = new Mock<DbContext> { CallBase = true }.Object)
            {
                contextServices = ((IDbContextServices)context).ScopedServiceProvider;
                Assert.Same(contextServices, ((IDbContextServices)context).ScopedServiceProvider);
            }

            using (var context = new Mock<DbContext> { CallBase = true }.Object)
            {
                Assert.NotSame(contextServices, ((IDbContextServices)context).ScopedServiceProvider);
            }
        }

        [Fact]
        public void Each_context_gets_new_scoped_services_with_explicit_config()
        {
            var serviceProvider = TestHelpers.CreateServiceProvider();

            var options = new DbContextOptions();

            IServiceProvider contextServices;
            using (var context = new DbContext(serviceProvider, options))
            {
                contextServices = ((IDbContextServices)context).ScopedServiceProvider;
                Assert.Same(contextServices, ((IDbContextServices)context).ScopedServiceProvider);
            }

            using (var context = new DbContext(serviceProvider, options))
            {
                Assert.NotSame(contextServices, ((IDbContextServices)context).ScopedServiceProvider);
            }
        }

        [Fact]
        public void Each_context_gets_new_scoped_services_with_implicit_services_and_explicit_config()
        {
            var options = new DbContextOptions();

            IServiceProvider contextServices;
            using (var context = new DbContext(options))
            {
                contextServices = ((IDbContextServices)context).ScopedServiceProvider;
                Assert.Same(contextServices, ((IDbContextServices)context).ScopedServiceProvider);
            }

            using (var context = new DbContext(options))
            {
                Assert.NotSame(contextServices, ((IDbContextServices)context).ScopedServiceProvider);
            }
        }

        [Fact]
        public void SaveChanges_calls_DetectChanges()
        {
            var services = new ServiceCollection()
                .AddScoped<StateManager, FakeStateManager>()
                .AddScoped<ChangeDetector, FakeChangeDetector>();

            var serviceProvider = TestHelpers.CreateServiceProvider(services);

            var options = new DbContextOptions();

            using (var context = new DbContext(serviceProvider, options))
            {
                var changeDetector = (FakeChangeDetector)((IDbContextServices)context).ScopedServiceProvider.GetRequiredService<ChangeDetector>();

                Assert.False(changeDetector.DetectChangesCalled);

                context.SaveChanges();

                Assert.True(changeDetector.DetectChangesCalled);
            }
        }

        [Fact]
        public void SaveChanges_calls_state_manager_SaveChanges()
        {
            var services = new ServiceCollection()
                .AddScoped<StateManager, FakeStateManager>()
                .AddScoped<ChangeDetector, FakeChangeDetector>();

            var serviceProvider = TestHelpers.CreateServiceProvider(services);

            var options = new DbContextOptions();

            using (var context = new DbContext(serviceProvider, options))
            {
                var stateManager = (FakeStateManager)((IDbContextServices)context).ScopedServiceProvider.GetRequiredService<StateManager>();

                var entryMock = new Mock<StateEntry>();
                entryMock.Setup(m => m.EntityState).Returns(EntityState.Modified);
                stateManager.Entries = new[] { entryMock.Object };

                Assert.False(stateManager.SaveChangesCalled);

                context.SaveChanges();

                Assert.True(stateManager.SaveChangesCalled);
            }
        }

        [Fact]
        public async Task SaveChangesAsync_calls_state_manager_SaveChangesAsync()
        {
            var services = new ServiceCollection()
                .AddScoped<StateManager, FakeStateManager>()
                .AddScoped<ChangeDetector, FakeChangeDetector>();

            var serviceProvider = TestHelpers.CreateServiceProvider(services);

            var options = new DbContextOptions();

            using (var context = new DbContext(serviceProvider, options))
            {
                context.Configuration.AutoDetectChangesEnabled = false;

                var stateManager = (FakeStateManager)((IDbContextServices)context).ScopedServiceProvider.GetRequiredService<StateManager>();

                var entryMock = new Mock<StateEntry>();
                entryMock.Setup(m => m.EntityState).Returns(EntityState.Modified);
                stateManager.Entries = new[] { entryMock.Object };

                Assert.False(stateManager.SaveChangesAsyncCalled);

                await context.SaveChangesAsync();

                Assert.True(stateManager.SaveChangesAsyncCalled);
            }
        }

        [Fact]
        public void Entry_methods_check_arguments()
        {
            var services = new ServiceCollection()
                .AddScoped<StateManager, FakeStateManager>();

            var serviceProvider = TestHelpers.CreateServiceProvider(services);

            using (var context = new DbContext(serviceProvider))
            {
                Assert.Equal(
                    "entity",
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Assert.Throws<ArgumentNullException>(() => context.Entry(null)).ParamName);
                Assert.Equal(
                    "entity",
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Assert.Throws<ArgumentNullException>(() => context.Entry<Random>(null)).ParamName);
            }
        }

        [Fact]
        public void Entry_methods_delegate_to_underlying_state_manager()
        {
            var entity = new Random();
            var stateManagerMock = new Mock<StateManager>();
            var stateEntry = new Mock<StateEntry>().Object;
            stateManagerMock.Setup(m => m.GetOrCreateEntry(entity)).Returns(stateEntry);

            var services = new ServiceCollection()
                .AddScoped(_ => stateManagerMock.Object);

            var serviceProvider = TestHelpers.CreateServiceProvider(services);

            using (var context = new DbContext(serviceProvider))
            {
                Assert.Same(stateEntry, context.Entry(entity).StateEntry);
                Assert.Same(stateEntry, context.Entry((object)entity).StateEntry);
            }
        }

        private class FakeStateManager : StateManager
        {
            public IEnumerable<StateEntry> Entries { get; set; }
            public bool SaveChangesCalled { get; set; }
            public bool SaveChangesAsyncCalled { get; set; }

            public override int SaveChanges()
            {
                SaveChangesCalled = true;
                return 1;
            }

            public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
            {
                SaveChangesAsyncCalled = true;
                return Task.FromResult(1);
            }

            public override IEnumerable<StateEntry> StateEntries
            {
                get { return Entries ?? Enumerable.Empty<StateEntry>(); }
            }
        }

        private class FakeChangeDetector : ChangeDetector
        {
            public bool DetectChangesCalled { get; set; }

            public override bool DetectChanges(StateManager stateManager)
            {
                DetectChangesCalled = true;
                return false;
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
        }

        [Fact]
        public void Can_add_existing_entities_to_context_to_be_attached()
        {
            TrackEntitiesTest((c, e) => c.Attach(e), (c, e) => c.Attach(e), EntityState.Unchanged);
        }

        [Fact]
        public void Can_add_existing_entities_to_context_to_be_updated()
        {
            TrackEntitiesTest((c, e) => c.Update(e), (c, e) => c.Update(e), EntityState.Modified);
        }

        [Fact]
        public void Can_add_existing_entities_to_context_to_be_deleted()
        {
            TrackEntitiesTest((c, e) => c.Remove(e), (c, e) => c.Remove(e), EntityState.Deleted);
        }

        private static void TrackEntitiesTest(
            Func<DbContext, Category, EntityEntry<Category>> categoryAdder,
            Func<DbContext, Product, EntityEntry<Product>> productAdder, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category1 = new Category { Id = 1, Name = "Beverages" };
                var category2 = new Category { Id = 2, Name = "Foods" };
                var product1 = new Product { Id = 1, Name = "Marmite", Price = 7.99m };
                var product2 = new Product { Id = 2, Name = "Bovril", Price = 4.99m };

                var categoryEntry1 = categoryAdder(context, category1);
                var categoryEntry2 = categoryAdder(context, category2);
                var productEntry1 = productAdder(context, product1);
                var productEntry2 = productAdder(context, product2);

                Assert.Same(category1, categoryEntry1.Entity);
                Assert.Same(category2, categoryEntry2.Entity);
                Assert.Same(product1, productEntry1.Entity);
                Assert.Same(product2, productEntry2.Entity);

                Assert.Same(category1, categoryEntry1.Entity);
                Assert.Equal(expectedState, categoryEntry2.State);
                Assert.Same(category2, categoryEntry2.Entity);
                Assert.Equal(expectedState, categoryEntry2.State);

                Assert.Same(product1, productEntry1.Entity);
                Assert.Equal(expectedState, productEntry1.State);
                Assert.Same(product2, productEntry2.Entity);
                Assert.Equal(expectedState, productEntry2.State);

                Assert.Same(categoryEntry1.StateEntry, context.Entry(category1).StateEntry);
                Assert.Same(categoryEntry2.StateEntry, context.Entry(category2).StateEntry);
                Assert.Same(productEntry1.StateEntry, context.Entry(product1).StateEntry);
                Assert.Same(productEntry2.StateEntry, context.Entry(product2).StateEntry);
            }
        }

        [Fact]
        public void Can_add_multiple_new_entities_to_context()
        {
            TrackMultipleEntitiesTest((c, e) => c.Add(e[0], e[1]), (c, e) => c.Add(e[0], e[1]), EntityState.Added);
        }

        [Fact]
        public void Can_add_multiple_new_entities_to_context_async()
        {
            TrackMultipleEntitiesTest((c, e) => c.AddAsync(e[0], e[1]).Result, (c, e) => c.AddAsync(e[0], e[1]).Result, EntityState.Added);

            TrackMultipleEntitiesTest(
                (c, e) => c.AddAsync(new[] { e[0], e[1] }, new CancellationToken()).Result,
                (c, e) => c.AddAsync(new[] { e[0], e[1] }, new CancellationToken()).Result, EntityState.Added);
        }

        [Fact]
        public void Can_add_multiple_existing_entities_to_context_to_be_attached()
        {
            TrackMultipleEntitiesTest((c, e) => c.Attach(e[0], e[1]), (c, e) => c.Attach(e[0], e[1]), EntityState.Unchanged);
        }

        [Fact]
        public void Can_add_multiple_existing_entities_to_context_to_be_updated()
        {
            TrackMultipleEntitiesTest((c, e) => c.Update(e[0], e[1]), (c, e) => c.Update(e[0], e[1]), EntityState.Modified);
        }

        [Fact]
        public void Can_add_multiple_existing_entities_to_context_to_be_deleted()
        {
            TrackMultipleEntitiesTest((c, e) => c.Remove(e[0], e[1]), (c, e) => c.Remove(e[0], e[1]), EntityState.Deleted);
        }

        private static void TrackMultipleEntitiesTest(
            Func<DbContext, Category[], IReadOnlyList<EntityEntry<Category>>> categoryAdder,
            Func<DbContext, Product[], IReadOnlyList<EntityEntry<Product>>> productAdder, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category1 = new Category { Id = 1, Name = "Beverages" };
                var category2 = new Category { Id = 2, Name = "Foods" };
                var product1 = new Product { Id = 1, Name = "Marmite", Price = 7.99m };
                var product2 = new Product { Id = 2, Name = "Bovril", Price = 4.99m };

                var categoryEntries = categoryAdder(context, new[] { category1, category2 });
                var productEntries = productAdder(context, new[] { product1, product2 });

                Assert.Equal(2, categoryEntries.Count);
                Assert.Equal(2, productEntries.Count);

                Assert.Same(category1, categoryEntries[0].Entity);
                Assert.Same(category2, categoryEntries[1].Entity);
                Assert.Same(product1, productEntries[0].Entity);
                Assert.Same(product2, productEntries[1].Entity);

                Assert.Same(category1, categoryEntries[0].Entity);
                Assert.Equal(expectedState, categoryEntries[0].State);
                Assert.Same(category2, categoryEntries[1].Entity);
                Assert.Equal(expectedState, categoryEntries[1].State);

                Assert.Same(product1, productEntries[0].Entity);
                Assert.Equal(expectedState, productEntries[0].State);
                Assert.Same(product2, productEntries[1].Entity);
                Assert.Equal(expectedState, productEntries[1].State);

                Assert.Same(categoryEntries[0].StateEntry, context.Entry(category1).StateEntry);
                Assert.Same(categoryEntries[1].StateEntry, context.Entry(category2).StateEntry);
                Assert.Same(productEntries[0].StateEntry, context.Entry(product1).StateEntry);
                Assert.Same(productEntries[1].StateEntry, context.Entry(product2).StateEntry);
            }
        }

        [Fact]
        public void Can_add_no_new_entities_to_context()
        {
            TrackNoEntitiesTest(c => c.Add(new Category[0]), c => c.Add(new Product[0]), EntityState.Added);
        }

        [Fact]
        public void Can_add_no_new_entities_to_context_async()
        {
            TrackNoEntitiesTest(c => c.AddAsync(new Category[0]).Result, c => c.AddAsync(new Product[0]).Result, EntityState.Added);

            TrackNoEntitiesTest(
                c => c.AddAsync(new Category[0], new CancellationToken()).Result,
                c => c.AddAsync(new Product[0], new CancellationToken()).Result, EntityState.Added);
        }

        [Fact]
        public void Can_add_no_existing_entities_to_context_to_be_attached()
        {
            TrackNoEntitiesTest(c => c.Attach(new Category[0]), c => c.Attach(new Product[0]), EntityState.Unchanged);
        }

        [Fact]
        public void Can_add_no_existing_entities_to_context_to_be_updated()
        {
            TrackNoEntitiesTest(c => c.Update(new Category[0]), c => c.Update(new Product[0]), EntityState.Modified);
        }

        [Fact]
        public void Can_add_no_existing_entities_to_context_to_be_deleted()
        {
            TrackNoEntitiesTest(c => c.Remove(new Category[0]), c => c.Remove(new Product[0]), EntityState.Deleted);
        }

        private static void TrackNoEntitiesTest(
            Func<DbContext, IReadOnlyList<EntityEntry<Category>>> categoryAdder,
            Func<DbContext, IReadOnlyList<EntityEntry<Product>>> productAdder, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                Assert.Empty(categoryAdder(context));
                Assert.Empty(productAdder(context));
            }
        }

        [Fact]
        public void Can_add_new_entities_to_context_non_generic()
        {
            TrackEntitiesTestNonGeneric((c, e) => c.Add(e), (c, e) => c.Add(e), EntityState.Added);
        }

        [Fact]
        public void Can_add_new_entities_to_context_async_non_generic()
        {
            TrackEntitiesTestNonGeneric((c, e) => c.AddAsync(e).Result, (c, e) => c.AddAsync(e).Result, EntityState.Added);
        }

        [Fact]
        public void Can_add_existing_entities_to_context_to_be_attached_non_generic()
        {
            TrackEntitiesTestNonGeneric((c, e) => c.Attach(e), (c, e) => c.Attach(e), EntityState.Unchanged);
        }

        [Fact]
        public void Can_add_existing_entities_to_context_to_be_updated_non_generic()
        {
            TrackEntitiesTestNonGeneric((c, e) => c.Update(e), (c, e) => c.Update(e), EntityState.Modified);
        }

        [Fact]
        public void Can_add_existing_entities_to_context_to_be_deleted_non_generic()
        {
            TrackEntitiesTestNonGeneric((c, e) => c.Remove(e), (c, e) => c.Remove(e), EntityState.Deleted);
        }

        private static void TrackEntitiesTestNonGeneric(
            Func<DbContext, object, EntityEntry> categoryAdder,
            Func<DbContext, object, EntityEntry> productAdder, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category1 = new Category { Id = 1, Name = "Beverages" };
                var category2 = new Category { Id = 2, Name = "Foods" };
                var product1 = new Product { Id = 1, Name = "Marmite", Price = 7.99m };
                var product2 = new Product { Id = 2, Name = "Bovril", Price = 4.99m };

                var categoryEntry1 = categoryAdder(context, category1);
                var categoryEntry2 = categoryAdder(context, category2);
                var productEntry1 = productAdder(context, product1);
                var productEntry2 = productAdder(context, product2);

                Assert.Same(category1, categoryEntry1.Entity);
                Assert.Same(category2, categoryEntry2.Entity);
                Assert.Same(product1, productEntry1.Entity);
                Assert.Same(product2, productEntry2.Entity);

                Assert.Same(category1, categoryEntry1.Entity);
                Assert.Equal(expectedState, categoryEntry2.State);
                Assert.Same(category2, categoryEntry2.Entity);
                Assert.Equal(expectedState, categoryEntry2.State);

                Assert.Same(product1, productEntry1.Entity);
                Assert.Equal(expectedState, productEntry1.State);
                Assert.Same(product2, productEntry2.Entity);
                Assert.Equal(expectedState, productEntry2.State);

                Assert.Same(categoryEntry1.StateEntry, context.Entry(category1).StateEntry);
                Assert.Same(categoryEntry2.StateEntry, context.Entry(category2).StateEntry);
                Assert.Same(productEntry1.StateEntry, context.Entry(product1).StateEntry);
                Assert.Same(productEntry2.StateEntry, context.Entry(product2).StateEntry);
            }
        }

        [Fact]
        public void Can_add_multiple_new_entities_to_context_non_generic()
        {
            TrackMultipleEntitiesTestNonGeneric((c, e) => c.Add(e[0], e[1]), (c, e) => c.Add(e[0], e[1]), EntityState.Added);
        }

        [Fact]
        public void Can_add_multiple_new_entities_to_context_async_non_generic()
        {
            TrackMultipleEntitiesTestNonGeneric((c, e) => c.AddAsync(e[0], e[1]).Result, (c, e) => c.AddAsync(e[0], e[1]).Result, EntityState.Added);

            TrackMultipleEntitiesTestNonGeneric(
                (c, e) => c.AddAsync(new[] { e[0], e[1] }, new CancellationToken()).Result,
                (c, e) => c.AddAsync(new[] { e[0], e[1] }, new CancellationToken()).Result, EntityState.Added);
        }

        [Fact]
        public void Can_add_multiple_existing_entities_to_context_to_be_attached_non_generic()
        {
            TrackMultipleEntitiesTestNonGeneric((c, e) => c.Attach(e[0], e[1]), (c, e) => c.Attach(e[0], e[1]), EntityState.Unchanged);
        }

        [Fact]
        public void Can_add_multiple_existing_entities_to_context_to_be_updated_non_generic()
        {
            TrackMultipleEntitiesTestNonGeneric((c, e) => c.Update(e[0], e[1]), (c, e) => c.Update(e[0], e[1]), EntityState.Modified);
        }

        [Fact]
        public void Can_add_multiple_existing_entities_to_context_to_be_deleted_non_generic()
        {
            TrackMultipleEntitiesTestNonGeneric((c, e) => c.Remove(e[0], e[1]), (c, e) => c.Remove(e[0], e[1]), EntityState.Deleted);
        }

        private static void TrackMultipleEntitiesTestNonGeneric(
            Func<DbContext, object[], IReadOnlyList<EntityEntry>> categoryAdder,
            Func<DbContext, object[], IReadOnlyList<EntityEntry>> productAdder, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category1 = new Category { Id = 1, Name = "Beverages" };
                var category2 = new Category { Id = 2, Name = "Foods" };
                var product1 = new Product { Id = 1, Name = "Marmite", Price = 7.99m };
                var product2 = new Product { Id = 2, Name = "Bovril", Price = 4.99m };

                var categoryEntries = categoryAdder(context, new[] { category1, category2 });
                var productEntries = productAdder(context, new[] { product1, product2 });

                Assert.Equal(2, categoryEntries.Count);
                Assert.Equal(2, productEntries.Count);

                Assert.Same(category1, categoryEntries[0].Entity);
                Assert.Same(category2, categoryEntries[1].Entity);
                Assert.Same(product1, productEntries[0].Entity);
                Assert.Same(product2, productEntries[1].Entity);

                Assert.Same(category1, categoryEntries[0].Entity);
                Assert.Equal(expectedState, categoryEntries[0].State);
                Assert.Same(category2, categoryEntries[1].Entity);
                Assert.Equal(expectedState, categoryEntries[1].State);

                Assert.Same(product1, productEntries[0].Entity);
                Assert.Equal(expectedState, productEntries[0].State);
                Assert.Same(product2, productEntries[1].Entity);
                Assert.Equal(expectedState, productEntries[1].State);

                Assert.Same(categoryEntries[0].StateEntry, context.Entry(category1).StateEntry);
                Assert.Same(categoryEntries[1].StateEntry, context.Entry(category2).StateEntry);
                Assert.Same(productEntries[0].StateEntry, context.Entry(product1).StateEntry);
                Assert.Same(productEntries[1].StateEntry, context.Entry(product2).StateEntry);
            }
        }

        [Fact]
        public void Can_add_no_new_entities_to_context_non_generic()
        {
            TrackNoEntitiesTestNonGeneric(c => c.Add(), c => c.Add(), EntityState.Added);
        }

        [Fact]
        public void Can_add_no_new_entities_to_context_async_non_generic()
        {
            TrackNoEntitiesTestNonGeneric(c => c.AddAsync().Result, c => c.AddAsync().Result, EntityState.Added);

            TrackNoEntitiesTestNonGeneric(
                c => c.AddAsync(new object[0], new CancellationToken()).Result,
                c => c.AddAsync(new object[0], new CancellationToken()).Result, EntityState.Added);
        }

        [Fact]
        public void Can_add_no_existing_entities_to_context_to_be_attached_non_generic()
        {
            TrackNoEntitiesTestNonGeneric(c => c.Attach(), c => c.Attach(), EntityState.Unchanged);
        }

        [Fact]
        public void Can_add_no_existing_entities_to_context_to_be_updated_non_generic()
        {
            TrackNoEntitiesTestNonGeneric(c => c.Update(), c => c.Update(), EntityState.Modified);
        }

        [Fact]
        public void Can_add_no_existing_entities_to_context_to_be_deleted_non_generic()
        {
            TrackNoEntitiesTestNonGeneric(c => c.Remove(), c => c.Remove(), EntityState.Deleted);
        }

        private static void TrackNoEntitiesTestNonGeneric(
            Func<DbContext, IReadOnlyList<EntityEntry>> categoryAdder,
            Func<DbContext, IReadOnlyList<EntityEntry>> productAdder, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                Assert.Empty(categoryAdder(context));
                Assert.Empty(productAdder(context));
            }
        }

        [Fact]
        public void Can_add_new_entities_to_context_with_key_generation()
        {
            TrackEntitiesWithKeyGenerationTest((c, e) => c.Add(e).Entity);
        }

        [Fact]
        public void Can_add_new_entities_to_context_with_key_generation_async()
        {
            TrackEntitiesWithKeyGenerationTest((c, e) => c.AddAsync(e).Result.Entity);
        }

        private static void TrackEntitiesWithKeyGenerationTest(Func<DbContext, TheGu, TheGu> adder)
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var gu1 = new TheGu { ShirtColor = "Red" };
                var gu2 = new TheGu { ShirtColor = "Still Red" };

                Assert.Same(gu1, adder(context, gu1));
                Assert.Same(gu2, adder(context, gu2));
                Assert.NotEqual(default(Guid), gu1.Id);
                Assert.NotEqual(default(Guid), gu2.Id);
                Assert.NotEqual(gu1.Id, gu2.Id);

                var categoryEntry = context.Entry(gu1);
                Assert.Same(gu1, categoryEntry.Entity);
                Assert.Equal(EntityState.Added, categoryEntry.State);

                categoryEntry = context.Entry(gu2);
                Assert.Same(gu2, categoryEntry.Entity);
                Assert.Equal(EntityState.Added, categoryEntry.State);
            }
        }

        [Fact]
        public void Can_use_Add_to_change_entity_state()
        {
            ChangeStateWithMethod((c, e) => c.Add(e), EntityState.Unknown, EntityState.Added);
            ChangeStateWithMethod((c, e) => c.Add(e), EntityState.Unchanged, EntityState.Added);
            ChangeStateWithMethod((c, e) => c.Add(e), EntityState.Deleted, EntityState.Added);
            ChangeStateWithMethod((c, e) => c.Add(e), EntityState.Modified, EntityState.Added);
            ChangeStateWithMethod((c, e) => c.Add(e), EntityState.Added, EntityState.Added);
        }

        [Fact]
        public void Can_use_Attach_to_change_entity_state()
        {
            ChangeStateWithMethod((c, e) => c.Attach(e), EntityState.Unknown, EntityState.Unchanged);
            ChangeStateWithMethod((c, e) => c.Attach(e), EntityState.Unchanged, EntityState.Unchanged);
            ChangeStateWithMethod((c, e) => c.Attach(e), EntityState.Deleted, EntityState.Unchanged);
            ChangeStateWithMethod((c, e) => c.Attach(e), EntityState.Modified, EntityState.Unchanged);
            ChangeStateWithMethod((c, e) => c.Attach(e), EntityState.Added, EntityState.Unchanged);
        }

        [Fact]
        public void Can_use_Update_to_change_entity_state()
        {
            ChangeStateWithMethod((c, e) => c.Update(e), EntityState.Unknown, EntityState.Modified);
            ChangeStateWithMethod((c, e) => c.Update(e), EntityState.Unchanged, EntityState.Modified);
            ChangeStateWithMethod((c, e) => c.Update(e), EntityState.Deleted, EntityState.Modified);
            ChangeStateWithMethod((c, e) => c.Update(e), EntityState.Modified, EntityState.Modified);
            ChangeStateWithMethod((c, e) => c.Update(e), EntityState.Added, EntityState.Modified);
        }

        [Fact]
        public void Can_use_Remove_to_change_entity_state()
        {
            ChangeStateWithMethod((c, e) => c.Remove(e), EntityState.Unknown, EntityState.Deleted);
            ChangeStateWithMethod((c, e) => c.Remove(e), EntityState.Unchanged, EntityState.Deleted);
            ChangeStateWithMethod((c, e) => c.Remove(e), EntityState.Deleted, EntityState.Deleted);
            ChangeStateWithMethod((c, e) => c.Remove(e), EntityState.Modified, EntityState.Deleted);
            ChangeStateWithMethod((c, e) => c.Remove(e), EntityState.Added, EntityState.Unknown);
        }

        private void ChangeStateWithMethod(Action<DbContext, object> action, EntityState initialState, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var entity = new Category { Name = "Beverages" };
                var entry = context.Entry(entity);

                entry.SetState(initialState);

                action(context, entity);

                Assert.Equal(expectedState, entry.State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_principal_first_fully_fixed_up()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product> { product };

                context.Attach(category);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unknown, context.Entry(product).State);

                context.Attach(product);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);

                // Dependent is Unchanged here because the FK change happened before it was attached
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_dependent_first_fully_fixed_up()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product> { product };

                context.Attach(product);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unknown, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);

                context.Attach(category);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_principal_first_collection_not_fixed_up()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product>();

                context.Attach(category);

                Assert.Equal(7, product.CategoryId);
                Assert.Empty(category.Products);
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unknown, context.Entry(product).State);

                context.Attach(product);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_dependent_first_collection_not_fixed_up()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product>();

                context.Attach(product);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unknown, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);

                context.Attach(category);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_principal_first_reference_not_fixed_up()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite" };
                category.Products = new List<Product> { product };

                context.Attach(category);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unknown, context.Entry(product).State);

                context.Attach(product);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);

                // Dependent is Unchanged here because the FK change happened before it was attached
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_dependent_first_reference_not_fixed_up()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite" };
                category.Products = new List<Product> { product };

                context.Attach(product);

                Assert.Equal(7, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Null(product.Category);
                Assert.Equal(EntityState.Unknown, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

                context.Attach(category);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_principal_first_fully_fixed_up()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product> { product };

                context.Entry(category).SetState(EntityState.Unchanged);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unknown, context.Entry(product).State);

                context.Entry(product).SetState(EntityState.Unchanged);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);

                // Dependent is Unchanged here because the FK change happened before it was attached
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_dependent_first_fully_fixed_up()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product> { product };

                context.Entry(product).SetState(EntityState.Unchanged);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unknown, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);

                context.Entry(category).SetState(EntityState.Unchanged);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_principal_first_collection_not_fixed_up()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product>();

                context.Entry(category).SetState(EntityState.Unchanged);

                Assert.Equal(7, product.CategoryId);
                Assert.Empty(category.Products);
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unknown, context.Entry(product).State);

                context.Entry(product).SetState(EntityState.Unchanged);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_dependent_first_collection_not_fixed_up()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product>();

                context.Entry(product).SetState(EntityState.Unchanged);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unknown, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);

                context.Entry(category).SetState(EntityState.Unchanged);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_principal_first_reference_not_fixed_up()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite" };
                category.Products = new List<Product> { product };

                context.Entry(category).SetState(EntityState.Unchanged);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unknown, context.Entry(product).State);

                context.Entry(product).SetState(EntityState.Unchanged);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);

                // Dependent is Unchanged here because the FK change happened before it was attached
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_dependent_first_reference_not_fixed_up()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite" };
                category.Products = new List<Product> { product };

                context.Entry(product).SetState(EntityState.Unchanged);

                Assert.Equal(7, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Null(product.Category);
                Assert.Equal(EntityState.Unknown, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

                context.Entry(category).SetState(EntityState.Unchanged);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_principal_first_fully_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product> { product };

                context.Attach(category);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unknown, context.Entry(product).State);

                context.Attach(product);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);

                // Dependent is Unchanged here because the FK change happened before it was attached
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_dependent_first_fully_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product> { product };

                context.Attach(product);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unknown, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);

                context.Attach(category);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_principal_first_collection_not_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product>();

                context.Attach(category);

                Assert.Equal(7, product.CategoryId);
                Assert.Empty(category.Products);
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unknown, context.Entry(product).State);

                context.Attach(product);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_dependent_first_collection_not_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product>();

                context.Attach(product);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unknown, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);

                context.Attach(category);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_principal_first_reference_not_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite" };
                category.Products = new List<Product> { product };

                context.Attach(category);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unknown, context.Entry(product).State);

                context.Attach(product);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);

                // Dependent is Unchanged here because the FK change happened before it was attached
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_dependent_first_reference_not_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite" };
                category.Products = new List<Product> { product };

                context.Attach(product);

                Assert.Equal(7, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category7, product.Category);
                Assert.Same(product, category7.Products.Single());
                Assert.Equal(EntityState.Unknown, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

                context.Attach(category);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_principal_first_fully_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product> { product };

                context.Entry(category).SetState(EntityState.Unchanged);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unknown, context.Entry(product).State);

                context.Entry(product).SetState(EntityState.Unchanged);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);

                // Dependent is Unchanged here because the FK change happened before it was attached
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_dependent_first_fully_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product> { product };

                context.Entry(product).SetState(EntityState.Unchanged);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unknown, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);

                context.Entry(category).SetState(EntityState.Unchanged);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_principal_first_collection_not_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product>();

                context.Entry(category).SetState(EntityState.Unchanged);

                Assert.Equal(7, product.CategoryId);
                Assert.Empty(category.Products);
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unknown, context.Entry(product).State);

                context.Entry(product).SetState(EntityState.Unchanged);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_dependent_first_collection_not_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product>();

                context.Entry(product).SetState(EntityState.Unchanged);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unknown, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);

                context.Entry(category).SetState(EntityState.Unchanged);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_principal_first_reference_not_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite" };
                category.Products = new List<Product> { product };

                context.Entry(category).SetState(EntityState.Unchanged);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unknown, context.Entry(product).State);

                context.Entry(product).SetState(EntityState.Unchanged);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);

                // Dependent is Unchanged here because the FK change happened before it was attached
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_dependent_first_reference_not_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite" };
                category.Products = new List<Product> { product };

                context.Entry(product).SetState(EntityState.Unchanged);

                Assert.Equal(7, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category7, product.Category);
                Assert.Same(product, category7.Products.Single());
                Assert.Equal(EntityState.Unknown, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

                context.Entry(category).SetState(EntityState.Unchanged);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(product).State);
            }
        }

        [Fact]
        public void Context_can_build_model_using_DbSet_properties()
        {
            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider()))
            {
                Assert.Equal(
                    new[] { typeof(Category).FullName, typeof(Product).FullName, typeof(TheGu).FullName },
                    context.Model.EntityTypes.Select(e => e.Name).ToArray());

                var categoryType = context.Model.GetEntityType(typeof(Category));
                Assert.Equal("Id", categoryType.GetPrimaryKey().Properties.Single().Name);
                Assert.Equal(
                    new[] { "Id", "Name" },
                    categoryType.Properties.Select(p => p.Name).ToArray());

                var productType = context.Model.GetEntityType(typeof(Product));
                Assert.Equal("Id", productType.GetPrimaryKey().Properties.Single().Name);
                Assert.Equal(
                    new[] { "CategoryId", "Id", "Name", "Price" },
                    productType.Properties.Select(p => p.Name).ToArray());

                var guType = context.Model.GetEntityType(typeof(TheGu));
                Assert.Equal("Id", guType.GetPrimaryKey().Properties.Single().Name);
                Assert.Equal(
                    new[] { "Id", "ShirtColor" },
                    guType.Properties.Select(p => p.Name).ToArray());
            }
        }

        [Fact]
        public void Context_will_use_explicit_model_if_set_in_config()
        {
            var model = new Model();
            model.AddEntityType(typeof(TheGu));

            var options = new DbContextOptions().UseModel(model);

            using (var context = new EarlyLearningCenter(TestHelpers.CreateServiceProvider(), options))
            {
                Assert.Equal(
                    new[] { typeof(TheGu).FullName },
                    context.Model.EntityTypes.Select(e => e.Name).ToArray());
            }
        }

        [Fact]
        public void Context_initializes_all_DbSet_properties_with_setters()
        {
            using (var context = new ContextWithSets())
            {
                Assert.NotNull(context.Products);
                Assert.NotNull(context.Categories);
                Assert.NotNull(context.GetGus());
                Assert.Null(context.NoSetter);
            }
        }

        private class ContextWithSets : DbContext
        {
            public DbSet<Product> Products { get; set; }
            public DbSet<Category> Categories { get; private set; }
            private DbSet<TheGu> Gus { get; set; }

            public DbSet<Random> NoSetter { get; } = null;

            public DbSet<TheGu> GetGus()
            {
                return Gus;
            }
        }

        [Fact]
        public void SaveChanges_doesnt_call_DataStore_when_nothing_is_dirty()
        {
            var store = new Mock<DataStore>();

            var servicesMock = new Mock<DataStoreServices>();
            servicesMock.Setup(m => m.Store).Returns(store.Object);
            servicesMock.Setup(m => m.ModelBuilderFactory).Returns(new ModelBuilderFactory());

            var sourceMock = new Mock<DataStoreSource>();
            sourceMock.Setup(m => m.IsAvailable).Returns(true);
            sourceMock.Setup(m => m.IsConfigured).Returns(true);
            sourceMock.Setup(m => m.StoreServices).Returns(servicesMock.Object);
            sourceMock.Setup(m => m.ContextOptions).Returns(new DbContextOptions());

            var services = new ServiceCollection();
            services.AddEntityFramework();
            services.AddInstance(sourceMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var options = new DbContextOptions();

            using (var context = new EarlyLearningCenter(serviceProvider, options))
            {
                context.Entry(new Category { Id = 1 }).SetState(EntityState.Unchanged);
                context.Entry(new Category { Id = 2 }).SetState(EntityState.Unchanged);
                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                context.SaveChanges();
            }

            store.Verify(
                s => s.SaveChangesAsync(It.IsAny<IReadOnlyList<StateEntry>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public void SaveChanges_only_passes_dirty_entries_to_DataStore()
        {
            var passedEntries = new List<StateEntry>();
            var store = new Mock<DataStore>();
            store.Setup(s => s.SaveChanges(It.IsAny<IReadOnlyList<StateEntry>>()))
                .Callback<IEnumerable<StateEntry>>(passedEntries.AddRange)
                .Returns(3);

            var servicesMock = new Mock<DataStoreServices>();
            servicesMock.Setup(m => m.Store).Returns(store.Object);
            servicesMock.Setup(m => m.ValueGeneratorCache).Returns(Mock.Of<ValueGeneratorCache>);
            servicesMock.Setup(m => m.ModelBuilderFactory).Returns(new ModelBuilderFactory());

            var sourceMock = new Mock<DataStoreSource>();
            sourceMock.Setup(m => m.IsAvailable).Returns(true);
            sourceMock.Setup(m => m.IsConfigured).Returns(true);
            sourceMock.Setup(m => m.StoreServices).Returns(servicesMock.Object);
            sourceMock.Setup(m => m.ContextOptions).Returns(new DbContextOptions());

            var services = new ServiceCollection();
            services.AddEntityFramework();
            services.AddInstance(sourceMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var options = new DbContextOptions();

            using (var context = new EarlyLearningCenter(serviceProvider, options))
            {
                context.Entry(new Category { Id = 1 }).SetState(EntityState.Unchanged);
                context.Entry(new Category { Id = 2 }).SetState(EntityState.Modified);
                context.Entry(new Category { Id = 3 }).SetState(EntityState.Added);
                context.Entry(new Category { Id = 4 }).SetState(EntityState.Deleted);
                Assert.Equal(4, context.ChangeTracker.Entries().Count());

                context.SaveChanges();
            }

            Assert.Equal(3, passedEntries.Count);

            store.Verify(
                s => s.SaveChanges(It.IsAny<IReadOnlyList<StateEntry>>()),
                Times.Once);
        }

        [Fact]
        public async Task SaveChangesAsync_only_passes_dirty_entries_to_DataStore()
        {
            var passedEntries = new List<StateEntry>();
            var store = new Mock<DataStore>();
            store.Setup(s => s.SaveChangesAsync(It.IsAny<IReadOnlyList<StateEntry>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<StateEntry>, CancellationToken>((e, c) => passedEntries.AddRange(e))
                .Returns(Task.FromResult(3));

            var servicesMock = new Mock<DataStoreServices>();
            servicesMock.Setup(m => m.Store).Returns(store.Object);
            servicesMock.Setup(m => m.ValueGeneratorCache).Returns(Mock.Of<ValueGeneratorCache>);
            servicesMock.Setup(m => m.ModelBuilderFactory).Returns(new ModelBuilderFactory());

            var sourceMock = new Mock<DataStoreSource>();
            sourceMock.Setup(m => m.IsAvailable).Returns(true);
            sourceMock.Setup(m => m.IsConfigured).Returns(true);
            sourceMock.Setup(m => m.StoreServices).Returns(servicesMock.Object);
            sourceMock.Setup(m => m.ContextOptions).Returns(new DbContextOptions());

            var services = new ServiceCollection();
            services.AddEntityFramework();
            services.AddInstance(sourceMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var options = new DbContextOptions();

            using (var context = new EarlyLearningCenter(serviceProvider, options))
            {
                context.Entry(new Category { Id = 1 }).SetState(EntityState.Unchanged);
                context.Entry(new Category { Id = 2 }).SetState(EntityState.Modified);
                context.Entry(new Category { Id = 3 }).SetState(EntityState.Added);
                context.Entry(new Category { Id = 4 }).SetState(EntityState.Deleted);
                Assert.Equal(4, context.ChangeTracker.Entries().Count());

                await context.SaveChangesAsync();
            }

            Assert.Equal(3, passedEntries.Count);

            store.Verify(
                s => s.SaveChangesAsync(It.IsAny<IReadOnlyList<StateEntry>>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public void Default_services_are_registered_when_parameterless_constructor_used()
        {
            using (var context = new EarlyLearningCenter())
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;

                Assert.IsType<EntityKeyFactorySource>(contextServices.GetRequiredService<EntityKeyFactorySource>());
            }
        }

        [Fact]
        public void Default_context_scoped_services_are_registered_when_parameterless_constructor_used()
        {
            using (var context = new EarlyLearningCenter())
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;

                Assert.IsType<StateEntryFactory>(contextServices.GetRequiredService<StateEntryFactory>());
            }
        }

        [Fact]
        public void Can_get_singleton_service_from_scoped_configuration()
        {
            using (var context = new EarlyLearningCenter())
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;

                Assert.IsType<StateManager>(contextServices.GetRequiredService<StateManager>());
            }
        }

        [Fact]
        public void Can_start_with_custom_services_by_passing_in_base_service_provider()
        {
            var factory = Mock.Of<OriginalValuesFactory>();
            var serviceCollection = new ServiceCollection()
                .AddSingleton<DbSetFinder>()
                .AddSingleton<DbSetSource>()
                .AddSingleton<ClrPropertyGetterSource>()
                .AddSingleton<ClrPropertySetterSource>()
                .AddSingleton<ClrCollectionAccessorSource>()
                .AddSingleton<EntityMaterializerSource>()
                .AddSingleton<MemberMapper>()
                .AddSingleton<FieldMatcher>()
                .AddSingleton<DataStoreSelector>()
                .AddScoped<DbSetInitializer>()
                .AddScoped<DbContextServices>()
                .AddInstance(factory);

            var provider = serviceCollection.BuildServiceProvider();

            using (var context = new EarlyLearningCenter(provider))
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;

                Assert.Same(factory, contextServices.GetRequiredService<OriginalValuesFactory>());
            }
        }

        [Fact]
        public void Required_low_level_services_are_added_if_needed()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddEntityFramework();

            var provider = serviceCollection.BuildServiceProvider();

            Assert.IsType<LoggerFactory>(provider.GetRequiredService<ILoggerFactory>());
            Assert.IsType<TypeActivator>(provider.GetRequiredService<ITypeActivator>());
            Assert.IsType<OptionsManager<DbContextOptions>>(provider.GetRequiredService<IOptions<DbContextOptions>>());
        }

        [Fact]
        public void Required_low_level_services_are_not_added_if_already_present()
        {
            var serviceCollection = new ServiceCollection();

            var loggerFactory = new FakeLoggerFactory();
            var typeActivator = new TypeActivator();

            serviceCollection
                .AddInstance<ILoggerFactory>(loggerFactory)
                .AddInstance<ITypeActivator>(typeActivator)
                .AddOptions()
                .AddEntityFramework();

            var provider = serviceCollection.BuildServiceProvider();

            Assert.Same(loggerFactory, provider.GetRequiredService<ILoggerFactory>());
            Assert.Same(typeActivator, provider.GetRequiredService<ITypeActivator>());
            Assert.IsType<OptionsManager<DbContextOptions>>(provider.GetRequiredService<IOptions<DbContextOptions>>());
        }

        [Fact]
        public void Low_level_services_can_be_replaced_after_being_added()
        {
            var serviceCollection = new ServiceCollection();

            var loggerFactory = new FakeLoggerFactory();
            var typeActivator = new TypeActivator();

            serviceCollection
                .AddEntityFramework();

            serviceCollection
                .AddInstance<ILoggerFactory>(loggerFactory)
                .AddInstance<ITypeActivator>(typeActivator)
                .AddOptions();

            var provider = serviceCollection.BuildServiceProvider();

            Assert.Same(loggerFactory, provider.GetRequiredService<ILoggerFactory>());
            Assert.Same(typeActivator, provider.GetRequiredService<ITypeActivator>());
            Assert.IsType<OptionsManager<DbContextOptions>>(provider.GetRequiredService<IOptions<DbContextOptions>>());
        }

        [Fact]
        public void Can_replace_already_registered_service_with_new_service()
        {
            var factory = Mock.Of<OriginalValuesFactory>();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddEntityFramework();
            serviceCollection.AddInstance(factory);

            var provider = serviceCollection.BuildServiceProvider();

            using (var context = new EarlyLearningCenter(provider))
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;

                Assert.Same(factory, contextServices.GetRequiredService<OriginalValuesFactory>());
            }
        }

        [Fact]
        public void Can_set_known_singleton_services_using_instance_sugar()
        {
            var modelSource = Mock.Of<IModelSource>();

            var services = new ServiceCollection()
                .AddInstance(modelSource);

            var provider = TestHelpers.CreateServiceProvider(services);

            using (var context = new EarlyLearningCenter(provider))
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;

                Assert.Same(modelSource, contextServices.GetRequiredService<IModelSource>());
            }
        }

        [Fact]
        public void Can_set_known_singleton_services_using_type_activation()
        {
            var services = new ServiceCollection()
                .AddSingleton<IModelSource, FakeModelSource>();

            var provider = TestHelpers.CreateServiceProvider(services);

            using (var context = new EarlyLearningCenter(provider))
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;

                Assert.IsType<FakeModelSource>(contextServices.GetRequiredService<IModelSource>());
            }
        }

        [Fact]
        public void Can_set_known_context_scoped_services_using_type_activation()
        {
            var services = new ServiceCollection()
                .AddScoped<StateManager, FakeStateManager>();

            var provider = TestHelpers.CreateServiceProvider(services);

            using (var context = new EarlyLearningCenter(provider))
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;

                Assert.IsType<FakeStateManager>(contextServices.GetRequiredService<StateManager>());
            }
        }

        [Fact]
        public void Replaced_services_are_scoped_appropriately()
        {
            var services = new ServiceCollection();
            services
                .AddEntityFramework().ServiceCollection
                .AddSingleton<IModelSource, FakeModelSource>()
                .AddScoped<StateManager, FakeStateManager>();

            var provider = services.BuildServiceProvider();

            var context = new EarlyLearningCenter(provider);
            var contextServices = ((IDbContextServices)context).ScopedServiceProvider;

            var modelSource = contextServices.GetRequiredService<IModelSource>();

            context.Dispose();

            context = new EarlyLearningCenter(provider);
            contextServices = ((IDbContextServices)context).ScopedServiceProvider;

            var stateManager = contextServices.GetRequiredService<StateManager>();

            Assert.Same(stateManager, contextServices.GetRequiredService<StateManager>());

            Assert.Same(modelSource, contextServices.GetRequiredService<IModelSource>());

            context.Dispose();

            context = new EarlyLearningCenter(provider);
            contextServices = ((IDbContextServices)context).ScopedServiceProvider;

            Assert.NotSame(stateManager, contextServices.GetRequiredService<StateManager>());

            Assert.Same(modelSource, contextServices.GetRequiredService<IModelSource>());

            context.Dispose();
        }

        [Fact]
        public void Can_get_replaced_singleton_service_from_scoped_configuration()
        {
            var provider = new ServiceCollection()
                .AddEntityFramework().ServiceCollection
                .AddSingleton<EntityMaterializerSource, FakeEntityMaterializerSource>()
                .BuildServiceProvider();

            using (var context = new EarlyLearningCenter(provider))
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;

                Assert.IsType<FakeEntityMaterializerSource>(contextServices.GetRequiredService<EntityMaterializerSource>());
            }
        }

        private class Category
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public List<Product> Products { get; set; }
        }

        private class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }

            public int CategoryId { get; set; }
            public Category Category { get; set; }
        }

        private class TheGu
        {
            public Guid Id { get; set; }
            public string ShirtColor { get; set; }
        }

        private class EarlyLearningCenter : DbContext
        {
            public EarlyLearningCenter()
            {
            }

            public EarlyLearningCenter(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            public EarlyLearningCenter(IServiceProvider serviceProvider, DbContextOptions options)
                : base(serviceProvider, options)
            {
            }

            public DbSet<Product> Products { get; set; }
            public DbSet<Category> Categories { get; set; }
            public DbSet<TheGu> Gus { get; set; }

            protected internal override void OnConfiguring(DbContextOptions options)
            {
                options.UseInMemoryStore(persist: false);
            }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Category>()
                    .OneToMany(e => e.Products, e => e.Category);
            }
        }

        private class FakeEntityMaterializerSource : EntityMaterializerSource
        {
        }

        private class FakeLoggerFactory : ILoggerFactory
        {
            public ILogger Create(string name)
            {
                return null;
            }

            public void AddProvider(ILoggerProvider provider)
            {
            }
        }

        private class FakeModelSource : IModelSource
        {
            public IModel GetModel(DbContext context, IModelBuilderFactory modelBuilder = null)
            {
                return null;
            }
        }

        [Fact]
        public void Context_with_defaults_can_be_used_as_service()
        {
            var services = new ServiceCollection();

            services
                .AddSingleton<FakeService>()
                .AddOptions()
                .AddEntityFramework()
                .AddDbContext<ContextWithDefaults>();

            var serviceProvider = services.BuildServiceProvider();

            using (var context = serviceProvider.GetRequiredService<ContextWithDefaults>())
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;

                Assert.NotNull(serviceProvider.GetRequiredService<FakeService>());
                Assert.NotSame(serviceProvider, contextServices);
                Assert.Equal(0, contextServices.GetRequiredService<DbContextService<IDbContextOptions>>().Service.Extensions.Count);
            }
        }

        [Fact]
        public void Context_with_defaults_and_options_action_can_be_used_as_service()
        {
            var services = new ServiceCollection();
            var contextOptionsExtension = new FakeDbContextOptionsExtension();

            services
                .AddSingleton<FakeService>()
                .AddEntityFramework()
                .AddDbContext<ContextWithDefaults>(options => ((IDbContextOptions)options).AddExtension(contextOptionsExtension));

            var serviceProvider = services.BuildServiceProvider();

            using (var context = serviceProvider.GetRequiredService<ContextWithDefaults>())
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;
                var options = contextServices.GetRequiredService<DbContextService<IDbContextOptions>>().Service;

                Assert.NotNull(contextServices.GetRequiredService<FakeService>());
                Assert.Equal(1, options.Extensions.Count);
                Assert.Same(contextOptionsExtension, options.Extensions[0]);
            }
        }

        [Fact]
        public void Context_with_service_provider_and_options_action_can_be_used_as_service()
        {
            var services = new ServiceCollection();
            var contextOptionsExtension = new FakeDbContextOptionsExtension();

            services
                .AddSingleton<FakeService>()
                .AddEntityFramework()
                .AddDbContext<ContextWithServiceProvider>(options => ((IDbContextOptions)options).AddExtension(contextOptionsExtension));

            var serviceProvider = services.BuildServiceProvider();

            using (var context = serviceProvider.GetRequiredService<ContextWithServiceProvider>())
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;
                var options = contextServices.GetRequiredService<DbContextService<IDbContextOptions>>().Service;

                Assert.NotNull(contextServices.GetRequiredService<FakeService>());
                Assert.Equal(1, options.Extensions.Count);
                Assert.Same(contextOptionsExtension, options.Extensions[0]);
            }
        }

        [Fact]
        public void Context_with_options_and_options_action_can_be_used_as_service()
        {
            var services = new ServiceCollection();
            var contextOptionsExtension = new FakeDbContextOptionsExtension();

            services
                .AddSingleton<FakeService>()
                .AddEntityFramework()
                .AddDbContext<ContextWithOptions>(options => ((IDbContextOptions)options).AddExtension(contextOptionsExtension));

            var serviceProvider = services.BuildServiceProvider();

            using (var context = serviceProvider.GetRequiredService<ContextWithOptions>())
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;
                var options = contextServices.GetRequiredService<DbContextService<IDbContextOptions>>().Service;

                Assert.NotNull(contextServices.GetRequiredService<FakeService>());
                Assert.Equal(1, options.Extensions.Count);
                Assert.Same(contextOptionsExtension, options.Extensions[0]);
            }
        }

        [Fact]
        public void Context_activation_reads_options_from_configuration_keyed_using_context_type_name()
        {
            Context_activation_reads_options_from_configuration<ContextWithDefaults>(t => t.Name);
            Context_activation_reads_options_from_configuration<ContextWithServiceProvider>(t => t.Name);
            Context_activation_reads_options_from_configuration<ContextWithOptions>(t => t.Name);
        }

        [Fact]
        public void Context_activation_reads_options_from_configuration_keyed_using_context_type_full_name()
        {
            Context_activation_reads_options_from_configuration<ContextWithDefaults>(t => t.FullName);
            Context_activation_reads_options_from_configuration<ContextWithServiceProvider>(t => t.FullName);
            Context_activation_reads_options_from_configuration<ContextWithOptions>(t => t.FullName);
        }

        private static void Context_activation_reads_options_from_configuration<ContextT>(Func<Type, string> contextKeyFunc)
            where ContextT : DbContext
        {
            var configSource = new MemoryConfigurationSource();
            configSource.Add(string.Concat("EntityFramework:", contextKeyFunc(typeof(ContextT)), ":ConnectionString"), "MyConnectionString");

            var config = new Configuration();
            config.Add(configSource);

            var services = new ServiceCollection();
            var contextOptionsExtension = new FakeDbContextOptionsExtension();

            services
                .AddEntityFramework(config)
                .AddDbContext<ContextT>(options => ((IDbContextOptions)options).AddExtension(contextOptionsExtension));

            var serviceProvider = services.BuildServiceProvider();

            using (var context = serviceProvider.GetRequiredService<ContextT>())
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;
                var contextOptions = (DbContextOptions<ContextT>)contextServices.GetRequiredService<DbContextService<IDbContextOptions>>().Service;

                Assert.NotNull(contextOptions);
                var rawOptions = ((IDbContextOptions)contextOptions).RawOptions;
                Assert.Equal(1, rawOptions.Count);
                Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
                Assert.Equal(1, ((IDbContextOptions)contextOptions).Extensions.Count);
                Assert.Same(contextOptionsExtension, ((IDbContextOptions)contextOptions).Extensions[0]);
            }
        }

        [Fact]
        public void Context_activation_reads_options_from_configuration_with_key_redirection()
        {
            var configSource = new MemoryConfigurationSource();
            configSource.Add("Data:DefaultConnection:ConnectionString", "MyConnectionString");
            configSource.Add("EntityFramework:ContextWithDefaults:ConnectionStringKey", "Data:DefaultConnection:ConnectionString");

            var config = new Configuration();
            config.Add(configSource);

            var services = new ServiceCollection();

            services
                .AddEntityFramework(config)
                .AddDbContext<ContextWithDefaults>();

            var serviceProvider = services.BuildServiceProvider();

            using (var context = serviceProvider.GetRequiredService<ContextWithDefaults>())
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;
                var contextOptions = (DbContextOptions<ContextWithDefaults>)contextServices.GetRequiredService<DbContextService<IDbContextOptions>>().Service;

                Assert.NotNull(contextOptions);
                var rawOptions = ((IDbContextOptions)contextOptions).RawOptions;
                Assert.Equal(1, rawOptions.Count);
                Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
            }
        }

        [Fact]
        public void Context_activation_reads_options_from_configuration_case_insensitively()
        {
            var configSource = new MemoryConfigurationSource();
            configSource.Add("entityFramework:contextWithDefaults:connectionString", "MyConnectionString");

            var config = new Configuration();
            config.Add(configSource);

            var services = new ServiceCollection();

            services
                .AddEntityFramework(config)
                .AddDbContext<ContextWithDefaults>();

            var serviceProvider = services.BuildServiceProvider();

            using (var context = serviceProvider.GetRequiredService<ContextWithDefaults>())
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;
                var contextOptions = (DbContextOptions<ContextWithDefaults>)contextServices.GetRequiredService<DbContextService<IDbContextOptions>>().Service;

                Assert.NotNull(contextOptions);
                var rawOptions = ((IDbContextOptions)contextOptions).RawOptions;
                Assert.Equal(1, rawOptions.Count);
                Assert.Equal("MyConnectionString", rawOptions["ConnectionString"]);
            }
        }

        private class FakeService
        {
        }

        private class FakeDbContextOptionsExtension : DbContextOptionsExtension
        {
            protected internal override void ApplyServices(EntityServicesBuilder builder)
            {
            }
        }

        private class ContextWithDefaults : DbContext
        {
            public DbSet<Product> Products { get; set; }
        }

        private class ContextWithServiceProvider : DbContext
        {
            public ContextWithServiceProvider(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            public DbSet<Product> Products { get; set; }
        }

        private class ContextWithOptions : DbContext
        {
            public ContextWithOptions(DbContextOptions<ContextWithOptions> contextOptions)
                : base(contextOptions)
            {
            }

            public DbSet<Product> Products { get; set; }
        }

        [Fact]
        public void Model_cannot_be_used_in_OnModelCreating()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddInMemoryStore()
                .AddDbContext<UseModelInOnModelCreatingContext>()
                .ServiceCollection
                .BuildServiceProvider();

            using (var context = serviceProvider.GetRequiredService<UseModelInOnModelCreatingContext>())
            {
                Assert.Equal(
                    Strings.RecursiveOnModelCreating,
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        private class UseModelInOnModelCreatingContext : DbContext
        {
            public UseModelInOnModelCreatingContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            public DbSet<Product> Products { get; set; }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                var _ = Model;
            }
        }

        [Fact]
        public void Context_cannot_be_used_in_OnModelCreating()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddInMemoryStore()
                .AddDbContext<UseInOnModelCreatingContext>()
                .ServiceCollection
                .BuildServiceProvider();

            using (var context = serviceProvider.GetRequiredService<UseInOnModelCreatingContext>())
            {
                Assert.Equal(
                    Strings.RecursiveOnModelCreating,
                    Assert.Throws<InvalidOperationException>(() => context.Products.ToList()).Message);
            }
        }

        private class UseInOnModelCreatingContext : DbContext
        {
            public UseInOnModelCreatingContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            public DbSet<Product> Products { get; set; }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                Products.ToList();
            }
        }

        [Fact]
        public void Context_cannot_be_used_in_OnConfiguring()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddInMemoryStore()
                .AddDbContext<UseInOnConfiguringContext>()
                .ServiceCollection
                .BuildServiceProvider();

            using (var context = serviceProvider.GetRequiredService<UseInOnConfiguringContext>())
            {
                Assert.Equal(
                    Strings.RecursiveOnConfiguring,
                    Assert.Throws<InvalidOperationException>(() => context.Products.ToList()).Message);
            }
        }

        private class UseInOnConfiguringContext : DbContext
        {
            public UseInOnConfiguringContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            public DbSet<Product> Products { get; set; }

            protected internal override void OnConfiguring(DbContextOptions options)
            {
                Products.ToList();

                base.OnConfiguring(options);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SaveChanges_calls_DetectChanges_by_default(bool async)
        {
            var provider = TestHelpers.CreateServiceProvider();

            using (var context = new ButTheHedgehogContext(provider))
            {
                Assert.True(context.Configuration.AutoDetectChangesEnabled);

                var product = context.Attach(new Product { Id = 1, Name = "Little Hedgehogs" }).Entity;

                product.Name = "Cracked Cookies";

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }
            }

            using (var context = new ButTheHedgehogContext(provider))
            {
                Assert.Equal("Cracked Cookies", context.Products.Single().Name);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Auto_DetectChanges_for_SaveChanges_can_be_switched_off(bool async)
        {
            var provider = TestHelpers.CreateServiceProvider();

            using (var context = new ButTheHedgehogContext(provider))
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                Assert.False(context.Configuration.AutoDetectChangesEnabled);

                var product = context.Attach(new Product { Id = 1, Name = "Little Hedgehogs" }).Entity;

                product.Name = "Cracked Cookies";

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }
            }

            using (var context = new ButTheHedgehogContext(provider))
            {
                Assert.Empty(context.Products);
            }
        }

        private class ButTheHedgehogContext : DbContext
        {
            public ButTheHedgehogContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            public DbSet<Product> Products { get; set; }

            protected internal override void OnConfiguring(DbContextOptions options)
            {
                options.UseInMemoryStore(persist: true);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Entry_calls_DetectChanges_by_default(bool useGenericOverload)
        {
            using (var context = new ButTheHedgehogContext(TestHelpers.CreateServiceProvider()))
            {
                var entry = context.Attach(new Product { Id = 1, Name = "Little Hedgehogs" });

                entry.Entity.Name = "Cracked Cookies";

                Assert.Equal(EntityState.Unchanged, entry.State);

                if (useGenericOverload)
                {
                    context.Entry(entry.Entity);
                }
                else
                {
                    context.Entry((object)entry.Entity);
                }

                Assert.Equal(EntityState.Modified, entry.State);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Auto_DetectChanges_for_Entry_can_be_switched_off(bool useGenericOverload)
        {
            using (var context = new ButTheHedgehogContext(TestHelpers.CreateServiceProvider()))
            {
                context.Configuration.AutoDetectChangesEnabled = false;

                var entry = context.Attach(new Product { Id = 1, Name = "Little Hedgehogs" });

                entry.Entity.Name = "Cracked Cookies";

                Assert.Equal(EntityState.Unchanged, entry.State);

                if (useGenericOverload)
                {
                    context.Entry(entry.Entity);
                }
                else
                {
                    context.Entry((object)entry.Entity);
                }

                Assert.Equal(EntityState.Unchanged, entry.State);
            }
        }
    }
}
