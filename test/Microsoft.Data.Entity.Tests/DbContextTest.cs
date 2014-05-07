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
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Advanced;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;
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
        public void Each_context_gets_new_scoped_context_configuration()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework();
            var serviceProvider = services.BuildServiceProvider();

            DbContextConfiguration configuration;
            using (var context = new DbContext(serviceProvider))
            {
                configuration = context.Configuration;
                Assert.Same(configuration, context.Configuration);
            }

            using (var context = new DbContext(serviceProvider))
            {
                Assert.NotSame(configuration, context.Configuration);
            }
        }

        [Fact]
        public void Each_context_gets_new_scoped_context_configuration_with_implicit_services()
        {
            DbContextConfiguration configuration;
            using (var context = new Mock<DbContext> { CallBase = true }.Object)
            {
                configuration = context.Configuration;
                Assert.Same(configuration, context.Configuration);
            }

            using (var context = new Mock<DbContext> { CallBase = true }.Object)
            {
                Assert.NotSame(configuration, context.Configuration);
            }
        }

        [Fact]
        public void Each_context_gets_new_scoped_context_configuration_with_explicit_config()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework();
            var serviceProvider = services.BuildServiceProvider();

            var entityConfig = new DbContextOptions().BuildConfiguration();

            DbContextConfiguration configuration;
            using (var context = new DbContext(serviceProvider, entityConfig))
            {
                configuration = context.Configuration;
                Assert.Same(configuration, context.Configuration);
            }

            using (var context = new DbContext(serviceProvider, entityConfig))
            {
                Assert.NotSame(configuration, context.Configuration);
            }
        }

        [Fact]
        public void Each_context_gets_new_scoped_context_configuration_with_implicit_services_and_explicit_config()
        {
            var entityConfig = new DbContextOptions().BuildConfiguration();

            DbContextConfiguration configuration;
            using (var context = new DbContext(entityConfig))
            {
                configuration = context.Configuration;
                Assert.Same(configuration, context.Configuration);
            }

            using (var context = new DbContext(entityConfig))
            {
                Assert.NotSame(configuration, context.Configuration);
            }
        }

        [Fact]
        public void SaveChanges_calls_DetectChanges()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework().UseStateManager<FakeStateManager>();
            var serviceProvider = services.BuildServiceProvider();

            var configuration = new DbContextOptions().BuildConfiguration();

            using (var context = new DbContext(serviceProvider, configuration))
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
            var services = new ServiceCollection();
            services.AddEntityFramework().UseStateManager<FakeStateManager>();
            var serviceProvider = services.BuildServiceProvider();

            var configuration = new DbContextOptions().BuildConfiguration();

            using (var context = new DbContext(serviceProvider, configuration))
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

            var configuration = new DbContextOptions().UseModel(model).BuildConfiguration();

            using (var context = new EarlyLearningCenter(configuration))
            {
                Assert.Equal(
                    new[] { "TheGu" },
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
        public void Set_and_non_generic_set_always_return_same_instance_returns_a_new_DbSet_for_the_given_type()
        {
            using (var context = new ContextWithSets())
            {
                var set = context.Set<Product>();
                Assert.NotNull(set);
                Assert.Same(set, context.Set<Product>());
                Assert.Same(set, context.Set(typeof(Product)));
                Assert.Same(set, context.Products);
            }
        }

        [Fact]
        public void SaveChanges_doesnt_call_DataStore_when_nothing_is_dirty()
        {
            var store = new Mock<DataStore>();

            var sourceMock = new Mock<DataStoreSource>();
            sourceMock.Setup(m => m.IsAvailable(It.IsAny<DbContextConfiguration>())).Returns(true);
            sourceMock.Setup(m => m.IsConfigured(It.IsAny<DbContextConfiguration>())).Returns(true);
            sourceMock.Setup(m => m.GetStore(It.IsAny<DbContextConfiguration>())).Returns(store.Object);

            var services = new ServiceCollection();
            services.AddEntityFramework();
            services.AddInstance(sourceMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var configuration = new DbContextOptions().BuildConfiguration();

            using (var context = new EarlyLearningCenter(serviceProvider, configuration))
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
        public void SaveChanges_only_passes_dirty_entries_to_DatStore()
        {
            var passedEntries = new List<StateEntry>();
            var store = new Mock<DataStore>();
            store.Setup(s => s.SaveChangesAsync(It.IsAny<IReadOnlyList<StateEntry>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<StateEntry>, CancellationToken>((e, c) => passedEntries.AddRange(e))
                .Returns(Task.FromResult(3));

            var sourceMock = new Mock<DataStoreSource>();
            sourceMock.Setup(m => m.IsAvailable(It.IsAny<DbContextConfiguration>())).Returns(true);
            sourceMock.Setup(m => m.IsConfigured(It.IsAny<DbContextConfiguration>())).Returns(true);
            sourceMock.Setup(m => m.GetStore(It.IsAny<DbContextConfiguration>())).Returns(store.Object);

            var services = new ServiceCollection();
            services.AddEntityFramework();
            services.AddInstance(sourceMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var configuration = new DbContextOptions().BuildConfiguration();

            using (var context = new EarlyLearningCenter(serviceProvider, configuration))
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
                s => s.SaveChangesAsync(It.IsAny<IReadOnlyList<StateEntry>>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public void Default_services_are_registered_when_parameterless_constructor_used()
        {
            using (var context = new EarlyLearningCenter())
            {
                var configuration = context.Configuration;

                Assert.IsType<ActiveIdentityGenerators>(configuration.Services.ActiveIdentityGenerators);
                Assert.IsType<EntityKeyFactorySource>(configuration.Services.EntityKeyFactorySource);
                Assert.IsType<ClrPropertyGetterSource>(configuration.Services.ClrPropertyGetterSource);
                Assert.IsType<ClrPropertySetterSource>(configuration.Services.ClrPropertySetterSource);
            }
        }

        [Fact]
        public void Default_context_scoped_services_are_registered_when_parameterless_constructor_used()
        {
            using (var context = new EarlyLearningCenter())
            {
                var configuration = context.Configuration;

                Assert.IsType<StateEntryFactory>(configuration.Services.StateEntryFactory);
                Assert.IsType<StateEntryNotifier>(configuration.Services.StateEntryNotifier);
                Assert.IsType<ContextSets>(configuration.Services.ContextSets);
                Assert.IsType<StateManager>(configuration.Services.StateManager);
                Assert.IsType<NavigationFixer>(configuration.Services.EntityStateListeners.Single());
            }
        }

        [Fact]
        public void Can_get_singleton_service_from_scoped_configuration()
        {
            using (var context = new EarlyLearningCenter())
            {
                var configuration = context.Configuration;

                Assert.IsType<StateManager>(configuration.Services.StateManager);
            }
        }

        [Fact]
        public void Can_start_with_custom_services_by_passing_in_base_service_provider()
        {
            var factory = Mock.Of<OriginalValuesFactory>();
            var serviceCollection = new ServiceCollection()
                .AddSingleton<DbSetFinder, DbSetFinder>()
                .AddSingleton<DbSetInitializer, DbSetInitializer>()
                .AddSingleton<ClrPropertyGetterSource, ClrPropertyGetterSource>()
                .AddSingleton<ClrPropertySetterSource, ClrPropertySetterSource>()
                .AddSingleton<ClrCollectionAccessorSource, ClrCollectionAccessorSource>()
                .AddSingleton<EntityMaterializerSource, EntityMaterializerSource>()
                .AddSingleton<MemberMapper, MemberMapper>()
                .AddSingleton<FieldMatcher, FieldMatcher>()
                .AddSingleton<DataStoreSelector, DataStoreSelector>()
                .AddScoped<DbContextConfiguration, DbContextConfiguration>()
                .AddScoped<ContextSets, ContextSets>()
                .AddInstance<OriginalValuesFactory>(factory);

            var provider = serviceCollection.BuildServiceProvider();

            using (var context = new EarlyLearningCenter(provider))
            {
                var configuration = context.Configuration;

                Assert.Same(factory, configuration.Services.OriginalValuesFactory);
            }
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
                var configuration = context.Configuration;

                Assert.Same(factory, configuration.Services.OriginalValuesFactory);
            }
        }

        [Fact]
        public void Can_set_known_singleton_services_using_instance_sugar()
        {
            var identityGenerators = Mock.Of<ActiveIdentityGenerators>();
            var collectionSource = Mock.Of<ClrCollectionAccessorSource>();
            var getterSource = Mock.Of<ClrPropertyGetterSource>();
            var setterSource = Mock.Of<ClrPropertySetterSource>();
            var keyFactorySource = Mock.Of<EntityKeyFactorySource>();
            var materializerSource = Mock.Of<EntityMaterializerSource>();
            var setFinder = Mock.Of<DbSetFinder>();
            var setInitializer = Mock.Of<DbSetInitializer>();
            var generatorFactory = Mock.Of<IdentityGeneratorFactory>();
            var loggerFactory = Mock.Of<ILoggerFactory>();
            var modelSource = Mock.Of<IModelSource>();

            var services = new ServiceCollection();
            services.AddEntityFramework()
                .UseActiveIdentityGenerators(identityGenerators)
                .UseClrCollectionAccessorSource(collectionSource)
                .UseClrPropertyGetterSource(getterSource)
                .UseClrPropertySetterSource(setterSource)
                .UseEntityKeyFactorySource(keyFactorySource)
                .UseEntityMaterializerSource(materializerSource)
                .UseDbSetFinder(setFinder)
                .UseDbSetInitializer(setInitializer)
                .UseIdentityGeneratorFactory(generatorFactory)
                .UseLoggerFactory(loggerFactory)
                .UseModelSource(modelSource);

            var provider = services.BuildServiceProvider();

            using (var context = new EarlyLearningCenter(provider))
            {
                var configuration = context.Configuration;

                Assert.Same(identityGenerators, configuration.Services.ActiveIdentityGenerators);
                Assert.Same(collectionSource, configuration.Services.ServiceProvider.GetService<ClrCollectionAccessorSource>());
                Assert.Same(getterSource, configuration.Services.ClrPropertyGetterSource);
                Assert.Same(setterSource, configuration.Services.ClrPropertySetterSource);
                Assert.Same(keyFactorySource, configuration.Services.EntityKeyFactorySource);
                Assert.Same(materializerSource, configuration.Services.ServiceProvider.GetService<EntityMaterializerSource>());
                Assert.Same(setFinder, configuration.Services.ServiceProvider.GetService<DbSetFinder>());
                Assert.Same(setInitializer, configuration.Services.ServiceProvider.GetService<DbSetInitializer>());
                Assert.Same(generatorFactory, configuration.Services.ServiceProvider.GetService<IdentityGeneratorFactory>());
                Assert.Same(loggerFactory, configuration.Services.ServiceProvider.GetService<ILoggerFactory>());
                Assert.Same(modelSource, configuration.Services.ModelSource);
            }
        }

        [Fact]
        public void Can_set_known_singleton_services_using_type_activation()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework()
                .UseActiveIdentityGenerators<FakeActiveIdentityGenerators>()
                .UseClrCollectionAccessorSource<FakeClrCollectionAccessorSource>()
                .UseClrPropertyGetterSource<FakeClrPropertyGetterSource>()
                .UseClrPropertySetterSource<FakeClrPropertySetterSource>()
                .UseEntityKeyFactorySource<FakeEntityKeyFactorySource>()
                .UseEntityMaterializerSource<FakeEntityMaterializerSource>()
                .UseDbSetFinder<FakeDbSetFinder>()
                .UseDbSetInitializer<FakeDbSetInitializer>()
                .UseEntityStateListener<FakeEntityStateListener>()
                .UseIdentityGeneratorFactory<FakeIdentityGeneratorFactory>()
                .UseContextSets<FakeContextSets>()
                .UseLoggerFactory<FakeLoggerFactory>()
                .UseModelSource<FakeModelSource>();

            var provider = services.BuildServiceProvider();

            using (var context = new EarlyLearningCenter(provider))
            {
                var configuration = context.Configuration;

                Assert.IsType<FakeActiveIdentityGenerators>(configuration.Services.ActiveIdentityGenerators);
                Assert.IsType<FakeClrCollectionAccessorSource>(configuration.Services.ServiceProvider.GetService<ClrCollectionAccessorSource>());
                Assert.IsType<FakeClrPropertyGetterSource>(configuration.Services.ClrPropertyGetterSource);
                Assert.IsType<FakeClrPropertySetterSource>(configuration.Services.ClrPropertySetterSource);
                Assert.IsType<FakeEntityKeyFactorySource>(configuration.Services.EntityKeyFactorySource);
                Assert.IsType<FakeEntityMaterializerSource>(configuration.Services.ServiceProvider.GetService<EntityMaterializerSource>());
                Assert.IsType<FakeDbSetFinder>(configuration.Services.ServiceProvider.GetService<DbSetFinder>());
                Assert.IsType<FakeDbSetInitializer>(configuration.Services.ServiceProvider.GetService<DbSetInitializer>());
                Assert.IsType<FakeIdentityGeneratorFactory>(configuration.Services.ServiceProvider.GetService<IdentityGeneratorFactory>());
                Assert.IsType<FakeLoggerFactory>(configuration.Services.ServiceProvider.GetService<ILoggerFactory>());
                Assert.IsType<FakeModelSource>(configuration.Services.ModelSource);
            }
        }

        [Fact]
        public void Can_set_known_context_scoped_services_using_type_activation()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework()
                .UseStateEntryFactory<FakeStateEntryFactory>()
                .UseStateEntryNotifier<FakeStateEntryNotifier>()
                .UseContextSets<FakeContextSets>()
                .UseStateManager<FakeStateManager>()
                .UseEntityStateListener<FakeNavigationFixer>();

            var provider = services.BuildServiceProvider();

            using (var context = new EarlyLearningCenter(provider))
            {
                var contextConfiguration = context.Configuration;

                Assert.IsType<FakeStateEntryFactory>(contextConfiguration.Services.StateEntryFactory);
                Assert.IsType<FakeStateEntryNotifier>(contextConfiguration.Services.StateEntryNotifier);
                Assert.IsType<FakeContextSets>(contextConfiguration.Services.ContextSets);
                Assert.IsType<FakeStateManager>(contextConfiguration.Services.StateManager);

                Assert.Equal(
                    new[] { typeof(FakeNavigationFixer), typeof(NavigationFixer) },
                    context.Configuration.Services.EntityStateListeners.Select(l => l.GetType()).OrderBy(t => t.Name).ToArray());
            }
        }

        [Fact]
        public void Replaced_services_are_scoped_appropriately()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework()
                .UseActiveIdentityGenerators<FakeActiveIdentityGenerators>()
                .UseClrCollectionAccessorSource<FakeClrCollectionAccessorSource>()
                .UseClrPropertyGetterSource<FakeClrPropertyGetterSource>()
                .UseClrPropertySetterSource<FakeClrPropertySetterSource>()
                .UseEntityKeyFactorySource<FakeEntityKeyFactorySource>()
                .UseEntityMaterializerSource<FakeEntityMaterializerSource>()
                .UseDbSetFinder<FakeDbSetFinder>()
                .UseDbSetInitializer<FakeDbSetInitializer>()
                .UseEntityStateListener<FakeEntityStateListener>()
                .UseIdentityGeneratorFactory<FakeIdentityGeneratorFactory>()
                .UseLoggerFactory<FakeLoggerFactory>()
                .UseModelSource<FakeModelSource>()
                .UseStateEntryFactory<FakeStateEntryFactory>()
                .UseStateEntryNotifier<FakeStateEntryNotifier>()
                .UseContextSets<FakeContextSets>()
                .UseStateManager<FakeStateManager>()
                .UseEntityStateListener<FakeNavigationFixer>();

            var provider = services.BuildServiceProvider();

            StateEntryFactory stateEntryFactory;
            StateEntryNotifier stateEntryNotifier;
            ContextSets contextSets;
            StateManager stateManager;
            IEntityStateListener entityStateListener;

            var context = new EarlyLearningCenter(provider);
            var configuration = context.Configuration;

            var activeIdentityGenerators = configuration.Services.ActiveIdentityGenerators;
            var clrCollectionAccessorSource = configuration.Services.ServiceProvider.GetService<ClrCollectionAccessorSource>();
            var clrPropertyGetterSource = configuration.Services.ClrPropertyGetterSource;
            var clrPropertySetterSource = configuration.Services.ClrPropertySetterSource;
            var entityKeyFactorySource = configuration.Services.EntityKeyFactorySource;
            var entityMaterializerSource = configuration.Services.ServiceProvider.GetService<EntityMaterializerSource>();
            var setFinder = configuration.Services.ServiceProvider.GetService<DbSetFinder>();
            var setInitializer = configuration.Services.ServiceProvider.GetService<DbSetInitializer>();
            var identityGeneratorFactory = configuration.Services.ServiceProvider.GetService<IdentityGeneratorFactory>();
            var loggerFactory = configuration.Services.ServiceProvider.GetService<ILoggerFactory>();
            var modelSource = configuration.Services.ModelSource;

            context.Dispose();

            context = new EarlyLearningCenter(provider);
            configuration = context.Configuration;

            stateEntryFactory = configuration.Services.StateEntryFactory;
            stateEntryNotifier = configuration.Services.StateEntryNotifier;
            contextSets = configuration.Services.ContextSets;
            stateManager = configuration.Services.StateManager;
            entityStateListener = configuration.Services.EntityStateListeners.OfType<FakeNavigationFixer>().Single();

            Assert.Same(stateEntryFactory, configuration.Services.StateEntryFactory);
            Assert.Same(stateEntryNotifier, configuration.Services.StateEntryNotifier);
            Assert.Same(contextSets, configuration.Services.ContextSets);
            Assert.Same(stateManager, configuration.Services.StateManager);
            Assert.Same(entityStateListener, configuration.Services.EntityStateListeners.OfType<FakeNavigationFixer>().Single());

            Assert.Same(activeIdentityGenerators, configuration.Services.ActiveIdentityGenerators);
            Assert.Same(clrCollectionAccessorSource, configuration.Services.ServiceProvider.GetService<ClrCollectionAccessorSource>());
            Assert.Same(clrPropertyGetterSource, configuration.Services.ClrPropertyGetterSource);
            Assert.Same(clrPropertySetterSource, configuration.Services.ClrPropertySetterSource);
            Assert.Same(entityKeyFactorySource, configuration.Services.EntityKeyFactorySource);
            Assert.Same(entityMaterializerSource, configuration.Services.ServiceProvider.GetService<EntityMaterializerSource>());
            Assert.Same(setFinder, configuration.Services.ServiceProvider.GetService<DbSetFinder>());
            Assert.Same(setInitializer, configuration.Services.ServiceProvider.GetService<DbSetInitializer>());
            Assert.Same(identityGeneratorFactory, configuration.Services.ServiceProvider.GetService<IdentityGeneratorFactory>());
            Assert.Same(loggerFactory, configuration.Services.ServiceProvider.GetService<ILoggerFactory>());
            Assert.Same(modelSource, configuration.Services.ModelSource);

            context.Dispose();

            context = new EarlyLearningCenter(provider);
            configuration = context.Configuration;

            Assert.NotSame(stateEntryFactory, configuration.Services.StateEntryFactory);
            Assert.NotSame(stateEntryNotifier, configuration.Services.StateEntryNotifier);
            Assert.NotSame(contextSets, configuration.Services.ContextSets);
            Assert.NotSame(stateManager, configuration.Services.StateManager);
            Assert.NotSame(entityStateListener, configuration.Services.EntityStateListeners.OfType<FakeNavigationFixer>().Single());

            Assert.Same(activeIdentityGenerators, configuration.Services.ActiveIdentityGenerators);
            Assert.Same(clrCollectionAccessorSource, configuration.Services.ServiceProvider.GetService<ClrCollectionAccessorSource>());
            Assert.Same(clrPropertyGetterSource, configuration.Services.ClrPropertyGetterSource);
            Assert.Same(clrPropertySetterSource, configuration.Services.ClrPropertySetterSource);
            Assert.Same(entityKeyFactorySource, configuration.Services.EntityKeyFactorySource);
            Assert.Same(entityMaterializerSource, configuration.Services.ServiceProvider.GetService<EntityMaterializerSource>());
            Assert.Same(setFinder, configuration.Services.ServiceProvider.GetService<DbSetFinder>());
            Assert.Same(setInitializer, configuration.Services.ServiceProvider.GetService<DbSetInitializer>());
            Assert.Same(identityGeneratorFactory, configuration.Services.ServiceProvider.GetService<IdentityGeneratorFactory>());
            Assert.Same(loggerFactory, configuration.Services.ServiceProvider.GetService<ILoggerFactory>());
            Assert.Same(modelSource, configuration.Services.ModelSource);

            context.Dispose();
        }

        [Fact]
        public void Can_get_replaced_singleton_service_from_scoped_configuration()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework().UseEntityMaterializerSource<FakeEntityMaterializerSource>();
            var provider = services.BuildServiceProvider();

            using (var context = new EarlyLearningCenter(provider))
            {
                var contextConfiguration = context.Configuration;

                Assert.IsType<FakeEntityMaterializerSource>(contextConfiguration.Services.ServiceProvider.GetService<EntityMaterializerSource>());
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

            public EarlyLearningCenter(ImmutableDbContextOptions configuration)
                : base(configuration)
            {
            }

            public EarlyLearningCenter(IServiceProvider serviceProvider, ImmutableDbContextOptions configuration)
                : base(serviceProvider, configuration)
            {
            }

            public DbSet<Product> Products { get; set; }
            public DbSet<Category> Categories { get; set; }
            public DbSet<TheGu> Gus { get; set; }
        }

        private class FakeActiveIdentityGenerators : ActiveIdentityGenerators
        {
        }

        private class FakeClrCollectionAccessorSource : ClrCollectionAccessorSource
        {
        }

        private class FakeClrPropertyGetterSource : ClrPropertyGetterSource
        {
        }

        private class FakeClrPropertySetterSource : ClrPropertySetterSource
        {
        }

        private class FakeEntityKeyFactorySource : EntityKeyFactorySource
        {
        }

        private class FakeEntityMaterializerSource : EntityMaterializerSource
        {
        }

        private class FakeDbSetFinder : DbSetFinder
        {
        }

        private class FakeDbSetInitializer : DbSetInitializer
        {
            public override void InitializeSets(DbContext context)
            {
            }
        }

        private class FakeEntityStateListener : IEntityStateListener
        {
            public void StateChanging(StateEntry entry, EntityState newState)
            {
            }

            public void StateChanged(StateEntry entry, EntityState oldState)
            {
            }
        }

        private class FakeIdentityGeneratorFactory : IdentityGeneratorFactory
        {
            public override IIdentityGenerator Create(IProperty property)
            {
                return null;
            }
        }

        private class FakeLoggerFactory : ILoggerFactory
        {
            public ILogger Create(string name)
            {
                return null;
            }
        }

        private class FakeModelSource : IModelSource
        {
            public IModel GetModel(DbContext context)
            {
                return null;
            }
        }

        private class FakeStateEntryFactory : StateEntryFactory
        {
        }

        private class FakeStateEntryNotifier : StateEntryNotifier
        {
        }

        private class FakeContextSets : ContextSets
        {
        }

        private class FakeNavigationFixer : NavigationFixer
        {
        }
    }
}
