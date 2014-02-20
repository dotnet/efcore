// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Storage;
using Xunit;

namespace Microsoft.Data.Entity
{
    public class EntityConfigurationTest
    {
        [Fact]
        public void Throws_if_no_data_store()
        {
            Assert.Equal(
                Strings.MissingConfigurationItem(typeof(DataStore)),
                Assert.Throws<InvalidOperationException>(() => new EntityConfiguration().DataStore).Message);
        }

        private class FakeDataStore : DataStore
        {
            public override Task<int> SaveChangesAsync(IEnumerable<EntityEntry> entityEntries)
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
            var serviceProvider = new ServiceProvider();
            var dataStore = new FakeDataStore();
            serviceProvider.AddInstance<DataStore>(dataStore);
            var entityConfiguration = new EntityConfiguration(serviceProvider);

            Assert.Same(dataStore, entityConfiguration.DataStore);
        }

        [Fact]
        public void Throws_if_no_identity_generator()
        {
            Assert.Equal(
                Strings.MissingConfigurationItem(typeof(IIdentityGenerator<object>)),
                Assert.Throws<InvalidOperationException>(() => new EntityConfiguration().GetIdentityGenerator<object>()).Message);
        }

        private class FakeIdentityGenerator<T> : IIdentityGenerator<T>
        {
            Task<T> IIdentityGenerator<T>.NextAsync()
            {
                return null;
            }

            Task<object> IIdentityGenerator.NextAsync()
            {
                return null;
            }
        }

        [Fact]
        public void Can_set_identity_generator()
        {
            var identityGenerator = new FakeIdentityGenerator<object>();
            var entityConfiguration = new EntityConfiguration();

            entityConfiguration.SetIdentityGenerator(identityGenerator);

            Assert.Same(identityGenerator, entityConfiguration.GetIdentityGenerator<object>());
        }

        [Fact]
        public void Can_provide_identity_generator_from_service_provider()
        {
            var serviceProvider = new ServiceProvider();
            var entityConfiguration = new EntityConfiguration(serviceProvider);
            var identityGenerator1 = new FakeIdentityGenerator<object>();
            serviceProvider.AddInstance<IIdentityGenerator<object>>(identityGenerator1);
            var identityGenerator2 = new FakeIdentityGenerator<string>();
            entityConfiguration.SetIdentityGenerator(identityGenerator2);

            Assert.Same(identityGenerator1, entityConfiguration.GetIdentityGenerator<object>());
            Assert.Same(identityGenerator2, entityConfiguration.GetIdentityGenerator<string>());
        }
    }
}
