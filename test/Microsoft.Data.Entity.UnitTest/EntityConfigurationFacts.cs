// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.Data.Entity
{
    using System;
    using Microsoft.AspNet.DependencyInjection;
    using Microsoft.Data.Entity.Resources;
    using Xunit;

    public class EntityConfigurationFacts
    {
        [Fact]
        public void Throws_if_no_data_store()
        {
            Assert.Equal(
                Strings.MissingConfigurationItem("DataStore"),
                Assert.Throws<InvalidOperationException>(() => new EntityConfiguration().DataStore).Message);
        }

        private class FakeDataStore : DataStore
        {
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
    }
}