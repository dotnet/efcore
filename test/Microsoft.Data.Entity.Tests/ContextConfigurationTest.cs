// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class ContextConfigurationTest : EntityConfigurationTest
    {
        [Fact]
        public void Throws_if_required_scoped_services_not_configured()
        {
            RequiredServiceTest(c => c.StateManager);
            RequiredServiceTest(c => c.ContextEntitySets);
            RequiredServiceTest(c => c.StateEntryNotifier);
            RequiredServiceTest(c => c.StateEntryFactory);
        }

        [Fact]
        public void Optional_multi_services_return_empty_list_when_not_registered()
        {
            Assert.Empty(CreateEmptyContextConfiguration().EntityStateListeners);
        }

        [Fact]
        public void Requesting_a_scoped_service_always_returns_same_instance_in_scope()
        {
            var configuration = CreateDefaultContextConfiguration();

            Assert.Same(configuration.StateManager, configuration.StateManager);
            Assert.Same(configuration.ContextEntitySets, configuration.ContextEntitySets);
            Assert.Same(configuration.StateEntryNotifier, configuration.StateEntryNotifier);
            Assert.Same(configuration.StateEntryFactory, configuration.StateEntryFactory);
        }

        [Fact]
        public void Requesting_a_scoped_service_always_returns_a_different_instance_in_a_different_scope()
        {
            var configuration = new EntityConfigurationBuilder().BuildConfiguration();

            var scopedProvider1 = configuration
                .ServiceProvider.GetService<IServiceScopeFactory>()
                .CreateScope().ServiceProvider;

            var contextConfiguration1 = scopedProvider1
                .GetService<ContextConfiguration>()
                .Initialize(scopedProvider1, Mock.Of<EntityContext>());

            var scopedProvider2 = configuration
                .ServiceProvider.GetService<IServiceScopeFactory>()
                .CreateScope().ServiceProvider;

            var contextConfiguration2 = scopedProvider2
                .GetService<ContextConfiguration>()
                .Initialize(scopedProvider2, Mock.Of<EntityContext>());

            Assert.NotSame(contextConfiguration1.StateManager, contextConfiguration2.StateManager);
            Assert.NotSame(contextConfiguration1.ContextEntitySets, contextConfiguration2.ContextEntitySets);
            Assert.NotSame(contextConfiguration1.StateEntryNotifier, contextConfiguration2.StateEntryNotifier);
            Assert.NotSame(contextConfiguration1.StateEntryFactory, contextConfiguration2.StateEntryFactory);
        }

        private void RequiredServiceTest<TService>(Func<ContextConfiguration, TService> test)
        {
            Assert.Equal(
                Strings.FormatMissingConfigurationItem(typeof(TService)),
                Assert.Throws<InvalidOperationException>(() => test(CreateEmptyContextConfiguration())).Message);
        }

        private ContextConfiguration CreateEmptyContextConfiguration()
        {
            var configuration = new EntityConfiguration()
                .Initialize(new ServiceCollection().BuildServiceProvider());

            var scopedProvider = configuration
                .ServiceProvider.GetService<IServiceScopeFactory>()
                .CreateScope().ServiceProvider;

            return new ContextConfiguration()
                .Initialize(scopedProvider, Mock.Of<EntityContext>());
        }

        protected override EntityConfiguration CreateEmptyConfiguration()
        {
            return CreateEmptyContextConfiguration();
        }

        private ContextConfiguration CreateDefaultContextConfiguration()
        {
            var configuration = new EntityConfigurationBuilder().BuildConfiguration();

            var scopedProvider = configuration
                .ServiceProvider.GetService<IServiceScopeFactory>()
                .CreateScope().ServiceProvider;

            return scopedProvider
                .GetService<ContextConfiguration>()
                .Initialize(scopedProvider, Mock.Of<EntityContext>());
        }

        protected override EntityConfiguration CreateDefaultConfiguration()
        {
            return CreateDefaultContextConfiguration();
        }
    }
}
