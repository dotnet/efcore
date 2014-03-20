// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Services;
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
            var configuration = new EntityConfigurationBuilder().BuildConfiguration();

            Assert.IsType<DefaultIdentityGeneratorFactory>(configuration.IdentityGeneratorFactory);
            Assert.IsType<ConsoleLoggerFactory>(configuration.LoggerFactory);
            Assert.IsType<ActiveIdentityGenerators>(configuration.ActiveIdentityGenerators);
            Assert.IsType<StateManagerFactory>(configuration.StateManagerFactory);
            Assert.IsType<EntitySetFinder>(configuration.EntitySetFinder);
            Assert.IsType<EntitySetInitializer>(configuration.EntitySetInitializer);
            Assert.IsType<EntityKeyFactorySource>(configuration.EntityKeyFactorySource);
            Assert.IsType<StateEntryFactory>(configuration.StateEntryFactory);
            Assert.IsType<ClrPropertyGetterSource>(configuration.ClrPropertyGetterSource);
            Assert.IsType<ClrPropertySetterSource>(configuration.ClrPropertySetterSource);
            Assert.IsType<EntitySetSource>(configuration.EntitySetSource);
            Assert.IsType<EntityMaterializerSource>(configuration.EntityMaterializerSource);

            Assert.IsType<NavigationFixer>(configuration.EntityStateListeners.Single());
        }

        [Fact]
        public void Can_start_with_custom_services_by_passing_in_service_collection()
        {
            var model = Mock.Of<IModel>();

            var serviceCollection = new ServiceCollection()
                .AddInstance<IModel>(model);

            var configuration = new EntityConfigurationBuilder(serviceCollection).BuildConfiguration();

            Assert.Same(model, configuration.Model);
            Assert.Null(configuration.LoggerFactory);
        }

        [Fact]
        public void Can_replace_already_registered_service_with_new_service()
        {
            var myService = Mock.Of<IModelSource>();

            var configuration = new EntityConfigurationBuilder()
                .UseModelSource(myService)
                .BuildConfiguration();

            Assert.Same(myService, configuration.ModelSource);
        }

        [Fact]
        public void Can_add_to_collection_of_services()
        {
            var myService = Mock.Of<IEntityStateListener>();

            var configuration = new EntityConfigurationBuilder()
                .UseEntityStateListener(myService)
                .BuildConfiguration();

            Assert.Equal(
                new[] { myService.GetType(), typeof(NavigationFixer) },
                configuration.EntityStateListeners.Select(l => l.GetType()).ToArray());
        }

        [Fact]
        public void Can_get_and_use_service_collection_directly()
        {
            var myService = Mock.Of<IModelSource>();

            var builder = new EntityConfigurationBuilder();
            builder.ServiceCollection.AddInstance<IModelSource>(myService);
            var configuration = builder.BuildConfiguration();

            Assert.Same(myService, configuration.ModelSource);
        }

        [Fact]
        public void Can_set_known_services_using_sugar()
        {
            var identityGenerators = Mock.Of<ActiveIdentityGenerators>();
            var getterSource = Mock.Of<ClrPropertyGetterSource>();
            var setterSource = Mock.Of<ClrPropertySetterSource>();
            var dataStore = Mock.Of<DataStore>();
            var keyFactorySource = Mock.Of<EntityKeyFactorySource>();
            var materializerSource = Mock.Of<EntityMaterializerSource>();
            var setFinder = Mock.Of<EntitySetFinder>();
            var setInitializer = Mock.Of<EntitySetInitializer>();
            var setSource = Mock.Of<EntitySetSource>();
            var entityStateListener = Mock.Of<IEntityStateListener>();
            var generatorFactory = Mock.Of<IdentityGeneratorFactory>();
            var loggerFactory = Mock.Of<ILoggerFactory>();
            var model = Mock.Of<IModel>();
            var modelSource = Mock.Of<IModelSource>();
            var entryFactory = Mock.Of<StateEntryFactory>();
            var managerFactory = Mock.Of<StateManagerFactory>();

            var configuration = new EntityConfigurationBuilder()
                .UseActiveIdentityGenerators(identityGenerators)
                .UseClrPropertyGetterSource(getterSource)
                .UseClrPropertySetterSource(setterSource)
                .UseDataStore(dataStore)
                .UseEntityKeyFactorySource(keyFactorySource)
                .UseEntityMaterializerSource(materializerSource)
                .UseEntitySetFinder(setFinder)
                .UseEntitySetInitializer(setInitializer)
                .UseEntitySetSource(setSource)
                .UseEntityStateListener(entityStateListener)
                .UseIdentityGeneratorFactory(generatorFactory)
                .UseLoggerFactory(loggerFactory)
                .UseModel(model)
                .UseModelSource(modelSource)
                .UseStateEntryFactory(entryFactory)
                .UseStateManagerFactory(managerFactory)
                .BuildConfiguration();

            Assert.Same(identityGenerators, configuration.ActiveIdentityGenerators);
            Assert.Same(getterSource, configuration.ClrPropertyGetterSource);
            Assert.Same(setterSource, configuration.ClrPropertySetterSource);
            Assert.Same(dataStore, configuration.DataStore);
            Assert.Same(keyFactorySource, configuration.EntityKeyFactorySource);
            Assert.Same(materializerSource, configuration.EntityMaterializerSource);
            Assert.Same(setFinder, configuration.EntitySetFinder);
            Assert.Same(setInitializer, configuration.EntitySetInitializer);
            Assert.Same(setSource, configuration.EntitySetSource);
            Assert.Same(generatorFactory, configuration.IdentityGeneratorFactory);
            Assert.Same(loggerFactory, configuration.LoggerFactory);
            Assert.Same(model, configuration.Model);
            Assert.Same(modelSource, configuration.ModelSource);
            Assert.Same(entryFactory, configuration.StateEntryFactory);
            Assert.Same(managerFactory, configuration.StateManagerFactory);

            Assert.Contains(entityStateListener, configuration.EntityStateListeners);
        }

        [Fact]
        public void Can_set_known_services_using_type_activation()
        {
            var configuration = new EntityConfigurationBuilder()
                .UseActiveIdentityGenerators<FakeActiveIdentityGenerators>()
                .UseClrPropertyGetterSource<FakeClrPropertyGetterSource>()
                .UseClrPropertySetterSource<FakeClrPropertySetterSource>()
                .UseDataStore<FakeDataStore>()
                .UseEntityKeyFactorySource<FakeEntityKeyFactorySource>()
                .UseEntityMaterializerSource<FakeEntityMaterializerSource>()
                .UseEntitySetFinder<FakeEntitySetFinder>()
                .UseEntitySetInitializer<FakeEntitySetInitializer>()
                .UseEntitySetSource<FakeEntitySetSource>()
                .UseEntityStateListener<FakeEntityStateListener>()
                .UseIdentityGeneratorFactory<FakeIdentityGeneratorFactory>()
                .UseLoggerFactory<FakeLoggerFactory>()
                .UseModel<FakeModel>()
                .UseModelSource<FakeModelSource>()
                .UseStateEntryFactory<FakeStateEntryFactory>()
                .UseStateManagerFactory<FakeStateManagerFactory>()
                .BuildConfiguration();

            Assert.IsType<FakeActiveIdentityGenerators>(configuration.ActiveIdentityGenerators);
            Assert.IsType<FakeClrPropertyGetterSource>(configuration.ClrPropertyGetterSource);
            Assert.IsType<FakeClrPropertySetterSource>(configuration.ClrPropertySetterSource);
            Assert.IsType<FakeDataStore>(configuration.DataStore);
            Assert.IsType<FakeEntityKeyFactorySource>(configuration.EntityKeyFactorySource);
            Assert.IsType<FakeEntityMaterializerSource>(configuration.EntityMaterializerSource);
            Assert.IsType<FakeEntitySetFinder>(configuration.EntitySetFinder);
            Assert.IsType<FakeEntitySetInitializer>(configuration.EntitySetInitializer);
            Assert.IsType<FakeEntitySetSource>(configuration.EntitySetSource);
            Assert.IsType<FakeIdentityGeneratorFactory>(configuration.IdentityGeneratorFactory);
            Assert.IsType<FakeLoggerFactory>(configuration.LoggerFactory);
            Assert.IsType<FakeModel>(configuration.Model);
            Assert.IsType<FakeModelSource>(configuration.ModelSource);
            Assert.IsType<FakeStateEntryFactory>(configuration.StateEntryFactory);
            Assert.IsType<FakeStateManagerFactory>(configuration.StateManagerFactory);

            Assert.Contains(typeof(FakeEntityStateListener), configuration.EntityStateListeners.Select(l => l.GetType()));
        }

        [Fact]
        public void Can_set_IdentityGeneratorFactory_but_fallback_to_service_provider_default()
        {
            var generator1 = new Mock<IIdentityGenerator>().Object;
            var defaultFactoryMock = new Mock<IdentityGeneratorFactory>();
            defaultFactoryMock.Setup(m => m.Create(It.Is<IProperty>(p => p.Name == "Foo"))).Returns(generator1);

            var generator2 = new Mock<IIdentityGenerator>().Object;
            var customFactoryMock = new Mock<IdentityGeneratorFactory>();
            customFactoryMock.Setup(m => m.Create(It.Is<IProperty>(p => p.Name == "Goo"))).Returns(generator2);

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddInstance<IdentityGeneratorFactory>(defaultFactoryMock.Object);

            // This looks silly, but the idea is that I'm getting the default that has been configured
            // so I can then override it. In this test I just created the default myself and I'm adding
            // it myself but this would not normally be the case.
            var defaultFactory = new EntityConfigurationBuilder()
                .UseIdentityGeneratorFactory(defaultFactoryMock.Object)
                .BuildConfiguration()
                .IdentityGeneratorFactory;

            var configuration = new EntityConfigurationBuilder()
                .UseIdentityGeneratorFactory(new OverridingIdentityGeneratorFactory(customFactoryMock.Object, defaultFactory))
                .BuildConfiguration();

            // Should get overridden generator
            Assert.Same(
                generator2,
                configuration.IdentityGeneratorFactory.Create(new Property("Goo", typeof(int), shadowProperty: false)));

            customFactoryMock.Verify(m => m.Create(It.IsAny<IProperty>()), Times.Once);
            defaultFactoryMock.Verify(m => m.Create(It.IsAny<IProperty>()), Times.Never);

            // Should fall back to the service provider
            Assert.Same(
                generator1,
                configuration.IdentityGeneratorFactory.Create(new Property("Foo", typeof(int), shadowProperty: false)));

            customFactoryMock.Verify(m => m.Create(It.IsAny<IProperty>()), Times.Exactly(2));
            defaultFactoryMock.Verify(m => m.Create(It.IsAny<IProperty>()), Times.Once);
        }

        private class FakeActiveIdentityGenerators : ActiveIdentityGenerators
        {
        }

        private class FakeClrPropertyGetterSource : ClrPropertyGetterSource
        {
        }

        private class FakeClrPropertySetterSource : ClrPropertySetterSource
        {
        }

        private class FakeDataStore : DataStore
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

        private class FakeModel : IModel
        {
            public string this[string annotationName]
            {
                get { return null; }
            }

            public IReadOnlyList<IAnnotation> Annotations { get; private set; }
            public string StorageName { get; private set; }

            public IEntityType TryGetEntityType(Type type)
            {
                return null;
            }

            public IEntityType GetEntityType(Type type)
            {
                return null;
            }

            public IReadOnlyList<IEntityType> EntityTypes { get; private set; }

            public IEntityType TryGetEntityType(string name)
            {
                return null;
            }

            public IEntityType GetEntityType(string name)
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

        private class FakeStateManagerFactory : StateManagerFactory
        {
        }
    }
}
