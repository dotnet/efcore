// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity
{
    public class EntityConfigurationTest
    {
        [Fact]
        public void ThrowsIfNoDataStore()
        {
            Assert.Equal(
                Strings.MissingConfigurationItem("DataStore"),
                Assert.Throws<InvalidOperationException>(() => new EntityConfiguration().DataStore).Message);
        }

        private class FakeDataStore : DataStore
        {
        }

        [Fact]
        public void CanSetDataStore()
        {
            var dataStore = new FakeDataStore();
            var entityConfiguration = new EntityConfiguration { DataStore = dataStore };

            Assert.Same(dataStore, entityConfiguration.DataStore);
        }

        [Fact]
        public void CanProvideDataStoreFromServiceProvider()
        {
            var serviceProvider = new ServiceProvider();
            var dataStore = new FakeDataStore();
            serviceProvider.AddInstance<DataStore>(dataStore);
            var entityConfiguration = new EntityConfiguration(serviceProvider);

            Assert.Same(dataStore, entityConfiguration.DataStore);
        }
    }
}
