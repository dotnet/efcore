// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
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

            Assert.Same(contextServices1.GetRequiredService<IMemberMapper>(), contextServices2.GetRequiredService<IMemberMapper>());
        }

        [Fact]
        public void Requesting_a_scoped_service_always_returns_same_instance_in_scope()
        {
            var provider = TestHelpers.Instance.CreateServiceProvider();
            var contextServices = TestHelpers.Instance.CreateContextServices(provider);

            Assert.Same(contextServices.GetRequiredService<IStateManager>(), contextServices.GetRequiredService<IStateManager>());
        }

        [Fact]
        public void Requesting_a_scoped_service_always_returns_a_different_instance_in_a_different_scope()
        {
            var provider = TestHelpers.Instance.CreateServiceProvider();
            var contextServices1 = TestHelpers.Instance.CreateContextServices(provider);
            var contextServices2 = TestHelpers.Instance.CreateContextServices(provider);

            Assert.NotSame(contextServices1.GetRequiredService<IStateManager>(), contextServices2.GetRequiredService<IStateManager>());
        }

        [Fact]
        public void Scoped_data_store_services_can_be_obtained_from_configuration()
        {
            var serviceProvider = TestHelpers.Instance.CreateServiceProvider();

            IDataStore store;
            IDataStoreCreator creator;
            IDataStoreConnection connection;

            using (var context = new DbContext(serviceProvider))
            {
                var contextServices = ((IAccessor<IServiceProvider>)context).Service;

                store = contextServices.GetRequiredService<IDataStore>();
                creator = contextServices.GetRequiredService<IDataStoreCreator>();
                connection = contextServices.GetRequiredService<IDataStoreConnection>();

                Assert.Same(store, contextServices.GetRequiredService<IDataStore>());
                Assert.Same(creator, contextServices.GetRequiredService<IDataStoreCreator>());
                Assert.Same(connection, contextServices.GetRequiredService<IDataStoreConnection>());
            }

            using (var context = new DbContext(serviceProvider))
            {
                var contextServices = ((IAccessor<IServiceProvider>)context).Service;

                Assert.NotSame(store, contextServices.GetRequiredService<IDataStore>());
                Assert.NotSame(creator, contextServices.GetRequiredService<IDataStoreCreator>());
                Assert.NotSame(connection, contextServices.GetRequiredService<IDataStoreConnection>());
            }
        }

        [Fact]
        public void Scoped_data_store_services_can_be_obtained_from_configuration_with_implicit_service_provider()
        {
            IDataStore store;
            IDataStoreCreator creator;
            IDataStoreConnection connection;

            using (var context = new GiddyupContext())
            {
                var contextServices = ((IAccessor<IServiceProvider>)context).Service;

                store = contextServices.GetRequiredService<IDataStore>();
                creator = contextServices.GetRequiredService<IDataStoreCreator>();
                connection = contextServices.GetRequiredService<IDataStoreConnection>();

                Assert.Same(store, contextServices.GetRequiredService<IDataStore>());
                Assert.Same(creator, contextServices.GetRequiredService<IDataStoreCreator>());
                Assert.Same(connection, contextServices.GetRequiredService<IDataStoreConnection>());
            }

            using (var context = new GiddyupContext())
            {
                var contextServices = ((IAccessor<IServiceProvider>)context).Service;

                Assert.NotSame(store, contextServices.GetRequiredService<IDataStore>());
                Assert.NotSame(creator, contextServices.GetRequiredService<IDataStoreCreator>());
                Assert.NotSame(connection, contextServices.GetRequiredService<IDataStoreConnection>());
            }
        }

        private class GiddyupContext : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder options)
            {
                options.UseInMemoryStore();
            }
        }
    }
}
