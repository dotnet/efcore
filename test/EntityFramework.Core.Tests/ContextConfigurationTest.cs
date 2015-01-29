// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking;
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
            var provider = TestHelpers.CreateServiceProvider();
            var contextServices1 = TestHelpers.CreateContextServices(provider);
            var contextServices2 = TestHelpers.CreateContextServices(provider);

            Assert.Same(contextServices1.GetRequiredService<MemberMapper>(), contextServices2.GetRequiredService<MemberMapper>());
        }

        [Fact]
        public void Requesting_a_scoped_service_always_returns_same_instance_in_scope()
        {
            var provider = TestHelpers.CreateServiceProvider();
            var contextServices = TestHelpers.CreateContextServices(provider);

            Assert.Same(contextServices.GetRequiredService<StateManager>(), contextServices.GetRequiredService<StateManager>());
        }

        [Fact]
        public void Requesting_a_scoped_service_always_returns_a_different_instance_in_a_different_scope()
        {
            var provider = TestHelpers.CreateServiceProvider();
            var contextServices1 = TestHelpers.CreateContextServices(provider);
            var contextServices2 = TestHelpers.CreateContextServices(provider);

            Assert.NotSame(contextServices1.GetRequiredService<StateManager>(), contextServices2.GetRequiredService<StateManager>());
        }

        [Fact]
        public void Scoped_data_store_services_can_be_obtained_from_configuration()
        {
            var serviceProvider = TestHelpers.CreateServiceProvider();

            DataStore store;
            DataStoreCreator creator;
            DataStoreConnection connection;

            using (var context = new DbContext(serviceProvider))
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;

                store = contextServices.GetRequiredService<DbContextService<DataStore>>().Service;
                creator = contextServices.GetRequiredService<DbContextService<DataStoreCreator>>().Service;
                connection = contextServices.GetRequiredService<DbContextService<DataStoreConnection>>().Service;

                Assert.Same(store, contextServices.GetRequiredService<DbContextService<DataStore>>().Service);
                Assert.Same(creator, contextServices.GetRequiredService<DbContextService<DataStoreCreator>>().Service);
                Assert.Same(connection, contextServices.GetRequiredService<DbContextService<DataStoreConnection>>().Service);
            }

            using (var context = new DbContext(serviceProvider))
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;

                Assert.NotSame(store, contextServices.GetRequiredService<DbContextService<DataStore>>().Service);
                Assert.NotSame(creator, contextServices.GetRequiredService<DbContextService<DataStoreCreator>>().Service);
                Assert.NotSame(connection, contextServices.GetRequiredService<DbContextService<DataStoreConnection>>().Service);
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
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;

                store = contextServices.GetRequiredService<DbContextService<DataStore>>().Service;
                creator = contextServices.GetRequiredService<DbContextService<DataStoreCreator>>().Service;
                connection = contextServices.GetRequiredService<DbContextService<DataStoreConnection>>().Service;

                Assert.Same(store, contextServices.GetRequiredService<DbContextService<DataStore>>().Service);
                Assert.Same(creator, contextServices.GetRequiredService<DbContextService<DataStoreCreator>>().Service);
                Assert.Same(connection, contextServices.GetRequiredService<DbContextService<DataStoreConnection>>().Service);
            }

            using (var context = new GiddyupContext())
            {
                var contextServices = ((IDbContextServices)context).ScopedServiceProvider;

                Assert.NotSame(store, contextServices.GetRequiredService<DbContextService<DataStore>>().Service);
                Assert.NotSame(creator, contextServices.GetRequiredService<DbContextService<DataStoreCreator>>().Service);
                Assert.NotSame(connection, contextServices.GetRequiredService<DbContextService<DataStoreConnection>>().Service);
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
