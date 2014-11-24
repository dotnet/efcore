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
using Microsoft.Data.Entity.Utilities;
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
        public void Each_context_gets_new_scoped_services()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework();
            var serviceProvider = services.BuildServiceProvider();

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
            var services = new ServiceCollection();
            services.AddEntityFramework();
            var serviceProvider = services.BuildServiceProvider();

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
            var services = new ServiceCollection();
            services.AddEntityFramework()
                .ServiceCollection
                .AddScoped<StateManager, FakeStateManager>()
                .AddScoped<ChangeDetector, FakeChangeDetector>();

            var serviceProvider = services.BuildServiceProvider();

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
            var services = new ServiceCollection();
            services.AddEntityFramework()
                .ServiceCollection
                .AddScoped<StateManager, FakeStateManager>()
                .AddScoped<ChangeDetector, FakeChangeDetector>();

            var serviceProvider = services.BuildServiceProvider();

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
            var services = new ServiceCollection();
            services.AddEntityFramework()
                .ServiceCollection
                .AddScoped<StateManager, FakeStateManager>()
                .AddScoped<ChangeDetector, FakeChangeDetector>();

            var serviceProvider = services.BuildServiceProvider();

            var options = new DbContextOptions();

            using (var context = new DbContext(serviceProvider, options))
            {
                var stateManager = (FakeStateManager)((IDbContextServices)context).ScopedServiceProvider.GetRequiredService<StateManager>();

                var entryMock = new Mock<StateEntry>();
                entryMock.Setup(m => m.EntityState).Returns(EntityState.Modified);
                stateManager.Entries = new[] { entryMock.Object };

                Assert.False(stateManager.SaveChangesAsyncCalled);

                await context.SaveChangesAsync();

                Assert.True(stateManager.SaveChangesAsyncCalled);
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
            Func<DbContext, Category, Category> categoryAdder,
            Func<DbContext, Product, Product> productAdder, EntityState expectedState)
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

        private static void TrackEntitiesWithKeyGenerationTest(Func<DbContext, TheGu, TheGu> adder)
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
        public void Context_can_build_model_using_DbSet_properties()
        {
            using (var context = new EarlyLearningCenter())
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
                    new[] { "Id", "Name", "Price" },
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

            using (var context = new EarlyLearningCenter(options))
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
            private readonly DbSet<Random> _noSetter = null;

            public DbSet<Product> Products { get; set; }
            public DbSet<Category> Categories { get; private set; }
            private DbSet<TheGu> Gus { get; set; }

            public DbSet<Random> NoSetter
            {
                get { return _noSetter; }
            }

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
                context.ChangeTracker.Entry(new Category { Id = 1 }).State = EntityState.Unchanged;
                context.ChangeTracker.Entry(new Category { Id = 2 }).State = EntityState.Unchanged;
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
                context.ChangeTracker.Entry(new Category { Id = 1 }).State = EntityState.Unchanged;
                context.ChangeTracker.Entry(new Category { Id = 2 }).State = EntityState.Modified;
                context.ChangeTracker.Entry(new Category { Id = 3 }).State = EntityState.Added;
                context.ChangeTracker.Entry(new Category { Id = 4 }).State = EntityState.Deleted;
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
                context.ChangeTracker.Entry(new Category { Id = 1 }).State = EntityState.Unchanged;
                context.ChangeTracker.Entry(new Category { Id = 2 }).State = EntityState.Modified;
                context.ChangeTracker.Entry(new Category { Id = 3 }).State = EntityState.Added;
                context.ChangeTracker.Entry(new Category { Id = 4 }).State = EntityState.Deleted;
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
                .AddScoped<DbContextConfiguration>()
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

            var services = new ServiceCollection();
            services
                .AddEntityFramework().ServiceCollection
                .AddInstance(modelSource);

            var provider = services.BuildServiceProvider();

            using (var context = new EarlyLearningCenter(provider))
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;

                Assert.Same(modelSource, contextServices.GetRequiredService<IModelSource>());
            }
        }

        [Fact]
        public void Can_set_known_singleton_services_using_type_activation()
        {
            var services = new ServiceCollection();
            services
                .AddEntityFramework().ServiceCollection
                .AddSingleton<IModelSource, FakeModelSource>();

            var provider = services.BuildServiceProvider();

            using (var context = new EarlyLearningCenter(provider))
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;

                Assert.IsType<FakeModelSource>(contextServices.GetRequiredService<IModelSource>());
            }
        }

        [Fact]
        public void Can_set_known_context_scoped_services_using_type_activation()
        {
            var services = new ServiceCollection();
            services
                .AddEntityFramework().ServiceCollection
                .AddScoped<StateManager, FakeStateManager>();

            var provider = services.BuildServiceProvider();

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
        }

        private class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
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

            public EarlyLearningCenter(DbContextOptions options)
                : base(options)
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
                Assert.Equal(0, contextServices.GetRequiredService<LazyRef<IDbContextOptions>>().Value.Extensions.Count);
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
                var options = contextServices.GetRequiredService<LazyRef<IDbContextOptions>>().Value;

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
                var options = contextServices.GetRequiredService<LazyRef<IDbContextOptions>>().Value;

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
                var options = contextServices.GetRequiredService<LazyRef<IDbContextOptions>>().Value;

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
                var contextOptions = (DbContextOptions<ContextT>)contextServices.GetRequiredService<LazyRef<IDbContextOptions>>().Value;

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
                var contextOptions = (DbContextOptions<ContextWithDefaults>)contextServices.GetRequiredService<LazyRef<IDbContextOptions>>().Value;

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
    }
}
