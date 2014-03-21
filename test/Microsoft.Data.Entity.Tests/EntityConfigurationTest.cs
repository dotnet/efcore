// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntityConfigurationTest
    {
        [Fact]
        public void Throws_if_required_services_not_configured()
        {
            RequiredServiceTest(c => c.DataStore);
            RequiredServiceTest(c => c.IdentityGeneratorFactory);
            RequiredServiceTest(c => c.StateManagerFactory);
            RequiredServiceTest(c => c.ActiveIdentityGenerators);
            RequiredServiceTest(c => c.ModelSource);
            RequiredServiceTest(c => c.EntitySetInitializer);
            RequiredServiceTest(c => c.EntitySetFinder);
            RequiredServiceTest(c => c.EntityKeyFactorySource);
            RequiredServiceTest(c => c.StateEntryFactory);
            RequiredServiceTest(c => c.ClrCollectionAccessorSource);
            RequiredServiceTest(c => c.ClrPropertyGetterSource);
            RequiredServiceTest(c => c.ClrPropertySetterSource);
            RequiredServiceTest(c => c.EntityMaterializerSource);
            RequiredServiceTest(c => c.LoggerFactory);
        }

        private static void RequiredServiceTest<TService>(Func<EntityConfiguration, TService> test)
        {
            Assert.Equal(
                Strings.FormatMissingConfigurationItem(typeof(TService)),
                Assert.Throws<InvalidOperationException>(
                    () => test(new EntityConfiguration(new ServiceCollection().BuildServiceProvider()))).Message);
        }

        [Fact]
        public void Optional_multi_services_return_empty_list_when_not_registered()
        {
            var entityConfiguration = new EntityConfiguration(new ServiceCollection().BuildServiceProvider());

            Assert.Empty(entityConfiguration.EntityStateListeners);
        }

        [Fact]
        public void Requesting_a_singleton_always_returns_same_instance()
        {
            var configuration = new EntityConfigurationBuilder().BuildConfiguration();

            Assert.Same(configuration.IdentityGeneratorFactory, configuration.IdentityGeneratorFactory);
            Assert.Same(configuration.StateManagerFactory, configuration.StateManagerFactory);
            Assert.Same(configuration.ActiveIdentityGenerators, configuration.ActiveIdentityGenerators);
            Assert.Same(configuration.ModelSource, configuration.ModelSource);
            Assert.Same(configuration.EntitySetInitializer, configuration.EntitySetInitializer);
            Assert.Same(configuration.EntitySetFinder, configuration.EntitySetFinder);
            Assert.Same(configuration.EntityKeyFactorySource, configuration.EntityKeyFactorySource);
            Assert.Same(configuration.StateEntryFactory, configuration.StateEntryFactory);
            Assert.Same(configuration.ClrCollectionAccessorSource, configuration.ClrCollectionAccessorSource);
            Assert.Same(configuration.ClrPropertyGetterSource, configuration.ClrPropertyGetterSource);
            Assert.Same(configuration.ClrPropertySetterSource, configuration.ClrPropertySetterSource);
            Assert.Same(configuration.EntityMaterializerSource, configuration.EntityMaterializerSource);
        }
    }
}
