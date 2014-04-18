// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Advanced;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntityConfigurationBuilderTest
    {
        [Fact]
        public void Default_services_are_registered_when_parameterless_constructor_used()
        {
            using (var context = new EntityContext(new EntityConfigurationBuilder().BuildConfiguration()))
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
            using (var context = new EntityContext(new EntityConfigurationBuilder().BuildConfiguration()))
            {
                var configuration = context.Configuration;

                Assert.IsType<StateEntryFactory>(configuration.Services.StateEntryFactory);
                Assert.IsType<StateEntryNotifier>(configuration.Services.StateEntryNotifier);
                Assert.IsType<ContextEntitySets>(configuration.Services.ContextEntitySets);
                Assert.IsType<StateManager>(configuration.Services.StateManager);
                Assert.IsType<NavigationFixer>(configuration.Services.EntityStateListeners.Single());
            }
        }

        [Fact]
        public void Can_get_singleton_service_from_scoped_configuration()
        {
            using (var context = new EntityContext(new EntityConfigurationBuilder().BuildConfiguration()))
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
                .AddSingleton<EntitySetFinder, EntitySetFinder>()
                .AddSingleton<EntitySetInitializer, EntitySetInitializer>()
                .AddSingleton<ClrPropertyGetterSource, ClrPropertyGetterSource>()
                .AddSingleton<ClrPropertySetterSource, ClrPropertySetterSource>()
                .AddSingleton<EntitySetSource, EntitySetSource>()
                .AddSingleton<ClrCollectionAccessorSource, ClrCollectionAccessorSource>()
                .AddSingleton<EntityMaterializerSource, EntityMaterializerSource>()
                .AddSingleton<MemberMapper, MemberMapper>()
                .AddSingleton<FieldMatcher, FieldMatcher>()
                .AddSingleton<DataStoreSelector, DataStoreSelector>()
                .AddScoped<ContextConfiguration, ContextConfiguration>()
                .AddScoped<ContextEntitySets, ContextEntitySets>()
                .AddInstance<OriginalValuesFactory>(factory);

            var provider = serviceCollection.BuildServiceProvider();

            using (var context = new EntityContext(new EntityConfigurationBuilder(provider).BuildConfiguration()))
            {
                var configuration = context.Configuration;

                Assert.Same(factory, configuration.Services.OriginalValuesFactory);
            }
        }

        [Fact]
        public void Can_replace_already_registered_service_with_new_service()
        {
            var factory = Mock.Of<OriginalValuesFactory>();
            var serviceCollection = new ServiceCollection()
                .AddEntityFramework()
                .AddInstance<OriginalValuesFactory>(factory);

            var provider = serviceCollection.BuildServiceProvider();

            using (var context = new EntityContext(new EntityConfigurationBuilder(provider).BuildConfiguration()))
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
            var setFinder = Mock.Of<EntitySetFinder>();
            var setInitializer = Mock.Of<EntitySetInitializer>();
            var setSource = Mock.Of<EntitySetSource>();
            var generatorFactory = Mock.Of<IdentityGeneratorFactory>();
            var loggerFactory = Mock.Of<ILoggerFactory>();
            var modelSource = Mock.Of<IModelSource>();

            var configuration = CreateConfiguration(new EntityConfigurationBuilder()
                .WithServices(s => s.UseActiveIdentityGenerators(identityGenerators)
                    .UseClrCollectionAccessorSource(collectionSource)
                    .UseClrPropertyGetterSource(getterSource)
                    .UseClrPropertySetterSource(setterSource)
                    .UseEntityKeyFactorySource(keyFactorySource)
                    .UseEntityMaterializerSource(materializerSource)
                    .UseEntitySetFinder(setFinder)
                    .UseEntitySetInitializer(setInitializer)
                    .UseEntitySetSource(setSource)
                    .UseIdentityGeneratorFactory(generatorFactory)
                    .UseLoggerFactory(loggerFactory)
                    .UseModelSource(modelSource))
                .BuildConfiguration());

            Assert.Same(identityGenerators, configuration.Services.ActiveIdentityGenerators);
            Assert.Same(collectionSource, configuration.Services.ServiceProvider.GetService<ClrCollectionAccessorSource>());
            Assert.Same(getterSource, configuration.Services.ClrPropertyGetterSource);
            Assert.Same(setterSource, configuration.Services.ClrPropertySetterSource);
            Assert.Same(keyFactorySource, configuration.Services.EntityKeyFactorySource);
            Assert.Same(materializerSource, configuration.Services.ServiceProvider.GetService<EntityMaterializerSource>());
            Assert.Same(setFinder, configuration.Services.ServiceProvider.GetService<EntitySetFinder>());
            Assert.Same(setInitializer, configuration.Services.ServiceProvider.GetService<EntitySetInitializer>());
            Assert.Same(setSource, configuration.Services.ServiceProvider.GetService<EntitySetSource>());
            Assert.Same(generatorFactory, configuration.Services.ServiceProvider.GetService<IdentityGeneratorFactory>());
            Assert.Same(loggerFactory, configuration.Services.ServiceProvider.GetService<ILoggerFactory>());
            Assert.Same(modelSource, configuration.Services.ModelSource);
        }

        [Fact]
        public void Can_set_known_singleton_services_using_type_activation()
        {
            var configuration = CreateConfiguration(new EntityConfigurationBuilder()
                .WithServices(s => s.UseActiveIdentityGenerators<FakeActiveIdentityGenerators>()
                    .UseClrCollectionAccessorSource<FakeClrCollectionAccessorSource>()
                    .UseClrPropertyGetterSource<FakeClrPropertyGetterSource>()
                    .UseClrPropertySetterSource<FakeClrPropertySetterSource>()
                    .UseEntityKeyFactorySource<FakeEntityKeyFactorySource>()
                    .UseEntityMaterializerSource<FakeEntityMaterializerSource>()
                    .UseEntitySetFinder<FakeEntitySetFinder>()
                    .UseEntitySetInitializer<FakeEntitySetInitializer>()
                    .UseEntitySetSource<FakeEntitySetSource>()
                    .UseEntityStateListener<FakeEntityStateListener>()
                    .UseIdentityGeneratorFactory<FakeIdentityGeneratorFactory>()
                    .UseLoggerFactory<FakeLoggerFactory>()
                    .UseModelSource<FakeModelSource>())
                .BuildConfiguration());

            Assert.IsType<FakeActiveIdentityGenerators>(configuration.Services.ActiveIdentityGenerators);
            Assert.IsType<FakeClrCollectionAccessorSource>(configuration.Services.ServiceProvider.GetService<ClrCollectionAccessorSource>());
            Assert.IsType<FakeClrPropertyGetterSource>(configuration.Services.ClrPropertyGetterSource);
            Assert.IsType<FakeClrPropertySetterSource>(configuration.Services.ClrPropertySetterSource);
            Assert.IsType<FakeEntityKeyFactorySource>(configuration.Services.EntityKeyFactorySource);
            Assert.IsType<FakeEntityMaterializerSource>(configuration.Services.ServiceProvider.GetService<EntityMaterializerSource>());
            Assert.IsType<FakeEntitySetFinder>(configuration.Services.ServiceProvider.GetService<EntitySetFinder>());
            Assert.IsType<FakeEntitySetInitializer>(configuration.Services.ServiceProvider.GetService<EntitySetInitializer>());
            Assert.IsType<FakeEntitySetSource>(configuration.Services.ServiceProvider.GetService<EntitySetSource>());
            Assert.IsType<FakeIdentityGeneratorFactory>(configuration.Services.ServiceProvider.GetService<IdentityGeneratorFactory>());
            Assert.IsType<FakeLoggerFactory>(configuration.Services.ServiceProvider.GetService<ILoggerFactory>());
            Assert.IsType<FakeModelSource>(configuration.Services.ModelSource);
        }

        [Fact]
        public void Can_set_known_context_scoped_services_using_type_activation()
        {
            var configuration = new EntityConfigurationBuilder()
                .WithServices(s => s.UseStateEntryFactory<FakeStateEntryFactory>()
                    .UseStateEntryNotifier<FakeStateEntryNotifier>()
                    .UseContextEntitySets<FakeContextEntitySets>()
                    .UseStateManager<FakeStateManager>()
                    .UseEntityStateListener<FakeNavigationFixer>())
                .BuildConfiguration();

            using (var context = new EntityContext(configuration))
            {
                var contextConfiguration = context.Configuration;

                Assert.IsType<FakeStateEntryFactory>(contextConfiguration.Services.StateEntryFactory);
                Assert.IsType<FakeStateEntryNotifier>(contextConfiguration.Services.StateEntryNotifier);
                Assert.IsType<FakeContextEntitySets>(contextConfiguration.Services.ContextEntitySets);
                Assert.IsType<FakeStateManager>(contextConfiguration.Services.StateManager);

                Assert.Equal(
                    new[] { typeof(FakeNavigationFixer), typeof(NavigationFixer) },
                    context.Configuration.Services.EntityStateListeners.Select(l => l.GetType()).OrderBy(t => t.Name).ToArray());
            }
        }

        [Fact]
        public void Replaced_services_are_scoped_appropriately()
        {
            var serviceProvider = new ServiceCollection().AddEntityFramework(
                s => s.UseActiveIdentityGenerators<FakeActiveIdentityGenerators>()
                    .UseClrCollectionAccessorSource<FakeClrCollectionAccessorSource>()
                    .UseClrPropertyGetterSource<FakeClrPropertyGetterSource>()
                    .UseClrPropertySetterSource<FakeClrPropertySetterSource>()
                    .UseEntityKeyFactorySource<FakeEntityKeyFactorySource>()
                    .UseEntityMaterializerSource<FakeEntityMaterializerSource>()
                    .UseEntitySetFinder<FakeEntitySetFinder>()
                    .UseEntitySetInitializer<FakeEntitySetInitializer>()
                    .UseEntitySetSource<FakeEntitySetSource>()
                    .UseEntityStateListener<FakeEntityStateListener>()
                    .UseIdentityGeneratorFactory<FakeIdentityGeneratorFactory>()
                    .UseLoggerFactory<FakeLoggerFactory>()
                    .UseModelSource<FakeModelSource>()
                    .UseStateEntryFactory<FakeStateEntryFactory>()
                    .UseStateEntryNotifier<FakeStateEntryNotifier>()
                    .UseContextEntitySets<FakeContextEntitySets>()
                    .UseStateManager<FakeStateManager>()
                    .UseEntityStateListener<FakeNavigationFixer>())
                .BuildServiceProvider();

            var builder = new EntityConfigurationBuilder(serviceProvider);

            var entityConfiguration = builder.BuildConfiguration();
            var configuration = CreateConfiguration(entityConfiguration);

            StateEntryFactory stateEntryFactory;
            StateEntryNotifier stateEntryNotifier;
            ContextEntitySets contextEntitySets;
            StateManager stateManager;
            IEntityStateListener entityStateListener;

            var activeIdentityGenerators = configuration.Services.ActiveIdentityGenerators;
            var clrCollectionAccessorSource = configuration.Services.ServiceProvider.GetService<ClrCollectionAccessorSource>();
            var clrPropertyGetterSource = configuration.Services.ClrPropertyGetterSource;
            var clrPropertySetterSource = configuration.Services.ClrPropertySetterSource;
            var entityKeyFactorySource = configuration.Services.EntityKeyFactorySource;
            var entityMaterializerSource = configuration.Services.ServiceProvider.GetService<EntityMaterializerSource>();
            var entitySetFinder = configuration.Services.ServiceProvider.GetService<EntitySetFinder>();
            var entitySetInitializer = configuration.Services.ServiceProvider.GetService<EntitySetInitializer>();
            var entitySetSource = configuration.Services.ServiceProvider.GetService<EntitySetSource>();
            var identityGeneratorFactory = configuration.Services.ServiceProvider.GetService<IdentityGeneratorFactory>();
            var loggerFactory = configuration.Services.ServiceProvider.GetService<ILoggerFactory>();
            var modelSource = configuration.Services.ModelSource;

            using (var context = new EntityContext(entityConfiguration))
            {
                var contextConfiguration = context.Configuration;

                stateEntryFactory = contextConfiguration.Services.StateEntryFactory;
                stateEntryNotifier = contextConfiguration.Services.StateEntryNotifier;
                contextEntitySets = contextConfiguration.Services.ContextEntitySets;
                stateManager = contextConfiguration.Services.StateManager;
                entityStateListener = contextConfiguration.Services.EntityStateListeners.OfType<FakeNavigationFixer>().Single();

                Assert.Same(stateEntryFactory, contextConfiguration.Services.StateEntryFactory);
                Assert.Same(stateEntryNotifier, contextConfiguration.Services.StateEntryNotifier);
                Assert.Same(contextEntitySets, contextConfiguration.Services.ContextEntitySets);
                Assert.Same(stateManager, contextConfiguration.Services.StateManager);
                Assert.Same(entityStateListener, contextConfiguration.Services.EntityStateListeners.OfType<FakeNavigationFixer>().Single());

                Assert.Same(activeIdentityGenerators, contextConfiguration.Services.ActiveIdentityGenerators);
                Assert.Same(clrCollectionAccessorSource, contextConfiguration.Services.ServiceProvider.GetService<ClrCollectionAccessorSource>());
                Assert.Same(clrPropertyGetterSource, contextConfiguration.Services.ClrPropertyGetterSource);
                Assert.Same(clrPropertySetterSource, contextConfiguration.Services.ClrPropertySetterSource);
                Assert.Same(entityKeyFactorySource, contextConfiguration.Services.EntityKeyFactorySource);
                Assert.Same(entityMaterializerSource, contextConfiguration.Services.ServiceProvider.GetService<EntityMaterializerSource>());
                Assert.Same(entitySetFinder, contextConfiguration.Services.ServiceProvider.GetService<EntitySetFinder>());
                Assert.Same(entitySetInitializer, contextConfiguration.Services.ServiceProvider.GetService<EntitySetInitializer>());
                Assert.Same(entitySetSource, contextConfiguration.Services.ServiceProvider.GetService<EntitySetSource>());
                Assert.Same(identityGeneratorFactory, contextConfiguration.Services.ServiceProvider.GetService<IdentityGeneratorFactory>());
                Assert.Same(loggerFactory, contextConfiguration.Services.ServiceProvider.GetService<ILoggerFactory>());
                Assert.Same(modelSource, contextConfiguration.Services.ModelSource);
            }

            using (var context = new EntityContext(entityConfiguration))
            {
                var contextConfiguration = context.Configuration;

                Assert.NotSame(stateEntryFactory, contextConfiguration.Services.StateEntryFactory);
                Assert.NotSame(stateEntryNotifier, contextConfiguration.Services.StateEntryNotifier);
                Assert.NotSame(contextEntitySets, contextConfiguration.Services.ContextEntitySets);
                Assert.NotSame(stateManager, contextConfiguration.Services.StateManager);
                Assert.NotSame(entityStateListener, contextConfiguration.Services.EntityStateListeners.OfType<FakeNavigationFixer>().Single());

                Assert.Same(activeIdentityGenerators, contextConfiguration.Services.ActiveIdentityGenerators);
                Assert.Same(clrCollectionAccessorSource, contextConfiguration.Services.ServiceProvider.GetService<ClrCollectionAccessorSource>());
                Assert.Same(clrPropertyGetterSource, contextConfiguration.Services.ClrPropertyGetterSource);
                Assert.Same(clrPropertySetterSource, contextConfiguration.Services.ClrPropertySetterSource);
                Assert.Same(entityKeyFactorySource, contextConfiguration.Services.EntityKeyFactorySource);
                Assert.Same(entityMaterializerSource, contextConfiguration.Services.ServiceProvider.GetService<EntityMaterializerSource>());
                Assert.Same(entitySetFinder, contextConfiguration.Services.ServiceProvider.GetService<EntitySetFinder>());
                Assert.Same(entitySetInitializer, contextConfiguration.Services.ServiceProvider.GetService<EntitySetInitializer>());
                Assert.Same(entitySetSource, contextConfiguration.Services.ServiceProvider.GetService<EntitySetSource>());
                Assert.Same(identityGeneratorFactory, contextConfiguration.Services.ServiceProvider.GetService<IdentityGeneratorFactory>());
                Assert.Same(loggerFactory, contextConfiguration.Services.ServiceProvider.GetService<ILoggerFactory>());
                Assert.Same(modelSource, contextConfiguration.Services.ModelSource);
            }
        }

        [Fact]
        public void Can_get_replaced_singleton_service_from_scoped_configuration()
        {
            var configuration = new EntityConfigurationBuilder()
                .WithServices(s => s.UseEntityMaterializerSource<FakeEntityMaterializerSource>()).BuildConfiguration();

            using (var context = new EntityContext(configuration))
            {
                var contextConfiguration = context.Configuration;

                Assert.IsType<FakeEntityMaterializerSource>(contextConfiguration.Services.ServiceProvider.GetService<EntityMaterializerSource>());
            }
        }

        private static ContextConfiguration CreateConfiguration(EntityConfiguration configuration)
        {
            var provider = configuration.Services ?? configuration.ServiceCollection.BuildServiceProvider();
            return new ContextConfiguration()
                .Initialize(provider, configuration, Mock.Of<EntityContext>());
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

        private class FakeEntitySetFinder : EntitySetFinder
        {
        }

        private class FakeEntitySetInitializer : EntitySetInitializer
        {
        }

        private class FakeEntitySetSource : EntitySetSource
        {
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
            public IModel GetModel(EntityContext context)
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

        private class FakeContextEntitySets : ContextEntitySets
        {
            public override void InitializeSets(EntityContext context)
            {
            }
        }

        private class FakeStateManager : StateManager
        {
        }

        private class FakeNavigationFixer : NavigationFixer
        {
        }
    }
}
