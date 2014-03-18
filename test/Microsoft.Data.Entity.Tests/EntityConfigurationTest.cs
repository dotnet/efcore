// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntityConfigurationTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            var configuration = new EntityConfiguration();

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentNullException>(() => configuration.ActiveIdentityGenerators = null).ParamName);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentNullException>(() => configuration.DataStore = null).ParamName);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentNullException>(() => configuration.IdentityGeneratorFactory = null).ParamName);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentNullException>(() => configuration.StateManagerFactory = null).ParamName);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentNullException>(() => configuration.ModelSource = null).ParamName);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentNullException>(() => configuration.Model = null).ParamName);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentNullException>(() => configuration.EntitySetInitializer = null).ParamName);
        }

        [Fact]
        public void Throws_if_no_data_store()
        {
            Assert.Equal(
                Strings.FormatMissingConfigurationItem(typeof(DataStore)),
                Assert.Throws<InvalidOperationException>(() => new EntityConfiguration().DataStore).Message);
        }

        private class FakeDataStore : DataStore
        {
            public override Task<int> SaveChangesAsync(
                IEnumerable<StateEntry> stateEntries, IModel model, CancellationToken cancellationToken)
            {
                return Task.FromResult(0);
            }
        }

        [Fact]
        public void Can_set_data_store()
        {
            var dataStore = new FakeDataStore();
            var entityConfiguration = new EntityConfiguration { DataStore = dataStore };

            Assert.Same(dataStore, entityConfiguration.DataStore);
        }

        [Fact]
        public void Can_provide_data_store_from_service_provider()
        {
            var collection = new ServiceCollection();
            var dataStore = new FakeDataStore();
            collection.AddInstance<DataStore>(dataStore);
            var entityConfiguration = new EntityConfiguration(collection.BuildServiceProvider());

            Assert.Same(dataStore, entityConfiguration.DataStore);
        }

        [Fact]
        public void Throws_if_no_IdentityGeneratorFactory_registered()
        {
            Assert.Equal(
                Strings.FormatMissingConfigurationItem(typeof(IdentityGeneratorFactory)),
                Assert.Throws<InvalidOperationException>(
                    () => new EntityConfiguration(new ServiceCollection().BuildServiceProvider()).IdentityGeneratorFactory).Message);
        }

        [Fact]
        public void Can_provide_IdentityGeneratorFactory_from_service_provider()
        {
            var serviceCollection = new ServiceCollection();
            var factory = new Mock<IdentityGeneratorFactory>().Object;
            serviceCollection.AddInstance<IdentityGeneratorFactory>(factory);
            var configuration = new EntityConfiguration(serviceCollection.BuildServiceProvider());

            Assert.Same(factory, configuration.IdentityGeneratorFactory);
        }

        [Fact]
        public void Can_set_IdentityGeneratorFactory()
        {
            var configuration = new EntityConfiguration();

            var factory = new Mock<IdentityGeneratorFactory>().Object;
            configuration.IdentityGeneratorFactory = factory;

            Assert.Same(factory, configuration.IdentityGeneratorFactory);
        }

        [Fact]
        public void Can_set_IdentityGeneratorFactory_to_override_service_provider_default()
        {
            var serviceCollection = new ServiceCollection();
            var configuration = new EntityConfiguration(serviceCollection.BuildServiceProvider());
            serviceCollection.AddInstance<IdentityGeneratorFactory>(new Mock<IdentityGeneratorFactory>().Object);

            var factory = new Mock<IdentityGeneratorFactory>().Object;
            configuration.IdentityGeneratorFactory = factory;

            Assert.Same(factory, configuration.IdentityGeneratorFactory);
        }

        [Fact]
        public void Can_set_IdentityGeneratorFactory_but_fallback_to_service_provider_default()
        {
            var serviceCollection = new ServiceCollection();

            var generator1 = new Mock<IIdentityGenerator>().Object;
            var defaultFactoryMock = new Mock<IdentityGeneratorFactory>();
            defaultFactoryMock.Setup(m => m.Create(It.Is<IProperty>(p => p.Name == "Foo"))).Returns(generator1);
            serviceCollection.AddInstance<IdentityGeneratorFactory>(defaultFactoryMock.Object);

            var generator2 = new Mock<IIdentityGenerator>().Object;
            var customFactoryMock = new Mock<IdentityGeneratorFactory>();
            customFactoryMock.Setup(m => m.Create(It.Is<IProperty>(p => p.Name == "Goo"))).Returns(generator2);

            var configuration = new EntityConfiguration(serviceCollection.BuildServiceProvider());

            configuration.IdentityGeneratorFactory = new OverridingIdentityGeneratorFactory(
                customFactoryMock.Object, configuration.IdentityGeneratorFactory);

            // Should get overridden generator
            Assert.Same(generator2, configuration.IdentityGeneratorFactory.Create(new Property("Goo", typeof(int), hasClrProperty: true)));
            customFactoryMock.Verify(m => m.Create(It.IsAny<IProperty>()), Times.Once);
            defaultFactoryMock.Verify(m => m.Create(It.IsAny<IProperty>()), Times.Never);

            // Should fall back to the service provider
            Assert.Same(generator1, configuration.IdentityGeneratorFactory.Create(new Property("Foo", typeof(int), hasClrProperty: true)));
            customFactoryMock.Verify(m => m.Create(It.IsAny<IProperty>()), Times.Exactly(2));
            defaultFactoryMock.Verify(m => m.Create(It.IsAny<IProperty>()), Times.Once);
        }

        [Fact]
        public void Throws_if_no_StateManagerFactory_registered()
        {
            Assert.Equal(
                Strings.FormatMissingConfigurationItem(typeof(StateManagerFactory)),
                Assert.Throws<InvalidOperationException>(
                    () => new EntityConfiguration(new ServiceCollection().BuildServiceProvider()).StateManagerFactory).Message);
        }

        [Fact]
        public void Can_provide_StateManagerFactory_from_service_provider()
        {
            var serviceCollection = new ServiceCollection();
            var factory = new Mock<StateManagerFactory>().Object;
            serviceCollection.AddInstance<StateManagerFactory>(factory);
            var configuration = new EntityConfiguration(serviceCollection.BuildServiceProvider());

            Assert.Same(factory, configuration.StateManagerFactory);
        }

        [Fact]
        public void Can_set_StateManagerFactory()
        {
            var configuration = new EntityConfiguration();

            var factory = new Mock<StateManagerFactory>().Object;
            configuration.StateManagerFactory = factory;

            Assert.Same(factory, configuration.StateManagerFactory);
        }

        [Fact]
        public void Throws_if_no_ActiveIdentityGenerators_registered()
        {
            Assert.Equal(
                Strings.FormatMissingConfigurationItem(typeof(ActiveIdentityGenerators)),
                Assert.Throws<InvalidOperationException>(
                    () => new EntityConfiguration(new ServiceCollection().BuildServiceProvider()).ActiveIdentityGenerators).Message);
        }

        [Fact]
        public void Can_provide_ActiveIdentityGenerators_from_service_provider()
        {
            var serviceCollection = new ServiceCollection();
            var factory = new Mock<ActiveIdentityGenerators>().Object;
            serviceCollection.AddInstance<ActiveIdentityGenerators>(factory);
            var configuration = new EntityConfiguration(serviceCollection.BuildServiceProvider());

            Assert.Same(factory, configuration.ActiveIdentityGenerators);
        }

        [Fact]
        public void Can_set_ActiveIdentityGenerators()
        {
            var configuration = new EntityConfiguration();

            var factory = new Mock<ActiveIdentityGenerators>().Object;
            configuration.ActiveIdentityGenerators = factory;

            Assert.Same(factory, configuration.ActiveIdentityGenerators);
        }

        [Fact]
        public void Model_returns_null_if_no_ModelSource_registered()
        {
            Assert.Null(new EntityConfiguration(new ServiceCollection().BuildServiceProvider()).Model);
        }

        [Fact]
        public void Can_provide_Model_from_service_provider()
        {
            var serviceCollection = new ServiceCollection();
            var model = new Mock<IModel>().Object;
            serviceCollection.AddInstance<IModel>(model);
            var configuration = new EntityConfiguration(serviceCollection.BuildServiceProvider());

            Assert.Same(model, configuration.Model);
        }

        [Fact]
        public void Can_set_Model()
        {
            var configuration = new EntityConfiguration();

            var model = new Mock<IModel>().Object;
            configuration.Model = model;

            Assert.Same(model, configuration.Model);
        }

        [Fact]
        public void Throws_if_no_ModelSource_registered()
        {
            Assert.Equal(
                Strings.FormatMissingConfigurationItem(typeof(IModelSource)),
                Assert.Throws<InvalidOperationException>(
                    () => new EntityConfiguration(new ServiceCollection().BuildServiceProvider()).ModelSource).Message);
        }

        [Fact]
        public void Can_provide_ModelSource_from_service_provider()
        {
            var serviceCollection = new ServiceCollection();
            var factory = new Mock<IModelSource>().Object;
            serviceCollection.AddInstance<IModelSource>(factory);
            var configuration = new EntityConfiguration(serviceCollection.BuildServiceProvider());

            Assert.Same(factory, configuration.ModelSource);
        }

        [Fact]
        public void Can_set_ModelSource()
        {
            var configuration = new EntityConfiguration();

            var factory = new Mock<IModelSource>().Object;
            configuration.ModelSource = factory;

            Assert.Same(factory, configuration.ModelSource);
        }

        [Fact]
        public void Throws_if_no_EntitySetInitializer_registered()
        {
            Assert.Equal(
                Strings.FormatMissingConfigurationItem(typeof(EntitySetInitializer)),
                Assert.Throws<InvalidOperationException>(
                    () => new EntityConfiguration(new ServiceCollection().BuildServiceProvider()).EntitySetInitializer).Message);
        }

        [Fact]
        public void Can_provide_EntitySetInitializer_from_service_provider()
        {
            var serviceCollection = new ServiceCollection();
            var service = new Mock<EntitySetInitializer>().Object;
            serviceCollection.AddInstance<EntitySetInitializer>(service);
            var configuration = new EntityConfiguration(serviceCollection.BuildServiceProvider());

            Assert.Same(service, configuration.EntitySetInitializer);
        }

        [Fact]
        public void Can_set_EntitySetInitializer()
        {
            var configuration = new EntityConfiguration();

            var service = new Mock<EntitySetInitializer>().Object;
            configuration.EntitySetInitializer = service;

            Assert.Same(service, configuration.EntitySetInitializer);
        }
    }
}
