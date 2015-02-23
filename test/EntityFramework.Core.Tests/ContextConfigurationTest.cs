// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class ContextConfigurationTest
    {
        [Fact]
        public void Requesting_a_singleton_always_returns_same_instance()
        {
            var provider = TestHelpers.Instance.CreateServiceProvider();
            var contextServices1 = TestHelpers.Instance.CreateContextServices(provider);
            var contextServices2 = TestHelpers.Instance.CreateContextServices(provider);

            Assert.Same(contextServices1.GetRequiredService<MemberMapper>(), contextServices2.GetRequiredService<MemberMapper>());
        }

        [Fact]
        public void Requesting_a_scoped_service_always_returns_same_instance_in_scope()
        {
            var provider = TestHelpers.Instance.CreateServiceProvider();
            var contextServices = TestHelpers.Instance.CreateContextServices(provider);

            Assert.Same(contextServices.GetRequiredService<StateManager>(), contextServices.GetRequiredService<StateManager>());
        }

        [Fact]
        public void Requesting_a_scoped_service_always_returns_a_different_instance_in_a_different_scope()
        {
            var provider = TestHelpers.Instance.CreateServiceProvider();
            var contextServices1 = TestHelpers.Instance.CreateContextServices(provider);
            var contextServices2 = TestHelpers.Instance.CreateContextServices(provider);

            Assert.NotSame(contextServices1.GetRequiredService<StateManager>(), contextServices2.GetRequiredService<StateManager>());
        }

        [Fact]
        public void Scoped_data_store_services_can_be_obtained_from_configuration()
        {
            var serviceProvider = TestHelpers.Instance.CreateServiceProvider();

            DataStore store;
            DataStoreCreator creator;
            DataStoreConnection connection;

            using (var context = new DbContext(serviceProvider))
            {
                var contextServices = ((IAccessor<IServiceProvider>)context).Service;

                store = contextServices.GetRequiredService<DataStore>();
                creator = contextServices.GetRequiredService<DataStoreCreator>();
                connection = contextServices.GetRequiredService<DataStoreConnection>();

                Assert.Same(store, contextServices.GetRequiredService<DataStore>());
                Assert.Same(creator, contextServices.GetRequiredService<DataStoreCreator>());
                Assert.Same(connection, contextServices.GetRequiredService<DataStoreConnection>());
            }

            using (var context = new DbContext(serviceProvider))
            {
                var contextServices = ((IAccessor<IServiceProvider>)context).Service;

                Assert.NotSame(store, contextServices.GetRequiredService<DataStore>());
                Assert.NotSame(creator, contextServices.GetRequiredService<DataStoreCreator>());
                Assert.NotSame(connection, contextServices.GetRequiredService<DataStoreConnection>());
            }
        }

        [Fact]
        public void Scoped_data_store_services_can_be_obtained_from_configuration_with_implicit_service_provider()
        {
            DataStore store;
            DataStoreCreator creator;
            DataStoreConnection connection;

            using (var context = new GiddyupContext())
            {
                var contextServices = ((IAccessor<IServiceProvider>)context).Service;

                store = contextServices.GetRequiredService<DataStore>();
                creator = contextServices.GetRequiredService<DataStoreCreator>();
                connection = contextServices.GetRequiredService<DataStoreConnection>();

                Assert.Same(store, contextServices.GetRequiredService<DataStore>());
                Assert.Same(creator, contextServices.GetRequiredService<DataStoreCreator>());
                Assert.Same(connection, contextServices.GetRequiredService<DataStoreConnection>());
            }

            using (var context = new GiddyupContext())
            {
                var contextServices = ((IAccessor<IServiceProvider>)context).Service;

                Assert.NotSame(store, contextServices.GetRequiredService<DataStore>());
                Assert.NotSame(creator, contextServices.GetRequiredService<DataStoreCreator>());
                Assert.NotSame(connection, contextServices.GetRequiredService<DataStoreConnection>());
            }
        }

        private class GiddyupContext : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptions options)
            {
                options.UseInMemoryStore();
            }
        }
    }
}
