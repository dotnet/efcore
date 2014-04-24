// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Storage;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Storage
{
    public class DataStoreSelectorTest
    {
        [Fact]
        public void Selects_single_configured_store()
        {
            var store = Mock.Of<DataStore>();
            var source = CreateSource("DataStore1", configured: true, available: false, store: store);

            var selector = new DataStoreSelector(new[] { source });

            Assert.Same(store, selector.SelectDataStore(new ContextConfiguration()));
        }

        [Fact]
        public void Throws_if_multiple_stores_configured()
        {
            var source1 = CreateSource("DataStore1", configured: true, available: false);
            var source2 = CreateSource("DataStore2", configured: true, available: false);
            var source3 = CreateSource("DataStore3", configured: false, available: true);
            var source4 = CreateSource("DataStore4", configured: true, available: false);

            var selector = new DataStoreSelector(new[] { source1, source2, source3, source4 });

            Assert.Equal(Strings.FormatMultipleDataStoresConfigured("'DataStore1' 'DataStore2' 'DataStore4' "),
                Assert.Throws<InvalidOperationException>(() => selector.SelectDataStore(new ContextConfiguration())).Message);
        }

        [Fact]
        public void Throws_if_no_store_services_have_been_registered_using_external_service_provider()
        {
            var configurationMock = new Mock<ContextConfiguration>();
            configurationMock.Setup(m => m.ProviderSource).Returns(ContextConfiguration.ServiceProviderSource.Explicit);

            var selector = new DataStoreSelector(null);

            Assert.Equal(Strings.FormatNoDataStoreService(),
                Assert.Throws<InvalidOperationException>(() => selector.SelectDataStore(configurationMock.Object)).Message);
        }

        [Fact]
        public void Throws_if_no_store_services_have_been_registered_using_implicit_service_provider()
        {
            var configurationMock = new Mock<ContextConfiguration>();
            configurationMock.Setup(m => m.ProviderSource).Returns(ContextConfiguration.ServiceProviderSource.Implicit);

            var selector = new DataStoreSelector(null);

            Assert.Equal(Strings.FormatNoDataStoreConfigured(),
                Assert.Throws<InvalidOperationException>(() => selector.SelectDataStore(configurationMock.Object)).Message);
        }

        [Fact]
        public void Throws_if_multiple_store_services_are_registered_but_none_are_configured()
        {
            var source1 = CreateSource("DataStore1", configured: false, available: true);
            var source2 = CreateSource("DataStore2", configured: false, available: false);
            var source3 = CreateSource("DataStore3", configured: false, available: false);

            var selector = new DataStoreSelector(new[] { source1, source2, source3 });

            Assert.Equal(Strings.FormatMultipleDataStoresAvailable("'DataStore1' 'DataStore2' 'DataStore3' "),
                Assert.Throws<InvalidOperationException>(() => selector.SelectDataStore(new ContextConfiguration())).Message);
        }

        [Fact]
        public void Throws_if_one_store_service_is_registered_but_not_configured_and_cannot_be_used_without_configuration()
        {
            var source = CreateSource("DataStore1", configured: false, available: false);

            var selector = new DataStoreSelector(new[] { source });

            Assert.Equal(Strings.FormatNoDataStoreConfigured(),
                Assert.Throws<InvalidOperationException>(() => selector.SelectDataStore(new ContextConfiguration())).Message);
        }

        [Fact]
        public void Selects_single_available_store()
        {
            var store = Mock.Of<DataStore>();
            var source = CreateSource("DataStore1", configured: false, available: true, store: store);

            var selector = new DataStoreSelector(new[] { source });

            Assert.Same(store, selector.SelectDataStore(new ContextConfiguration()));
        }

        private static DataStoreSource CreateSource(string name, bool configured, bool available, DataStore store = null)
        {
            var sourceMock = new Mock<DataStoreSource>();
            sourceMock.Setup(m => m.IsConfigured(It.IsAny<ContextConfiguration>())).Returns(configured);
            sourceMock.Setup(m => m.IsAvailable(It.IsAny<ContextConfiguration>())).Returns(available);
            sourceMock.Setup(m => m.GetDataStore(It.IsAny<ContextConfiguration>())).Returns(store);
            sourceMock.Setup(m => m.Name).Returns(name);

            return sourceMock.Object;
        }
    }
}
