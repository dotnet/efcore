// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
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
            var services = Mock.Of<IDataStoreServices>();
            var source = CreateSource("DataStore1", configured: true, available: false, services: services);

            var selector = new DataStoreSelector(new[] { source });

            Assert.Same(services, selector.SelectDataStore(DbContextServices.ServiceProviderSource.Explicit));
        }

        [Fact]
        public void Throws_if_multiple_stores_configured()
        {
            var source1 = CreateSource("DataStore1", configured: true, available: false);
            var source2 = CreateSource("DataStore2", configured: true, available: false);
            var source3 = CreateSource("DataStore3", configured: false, available: true);
            var source4 = CreateSource("DataStore4", configured: true, available: false);

            var selector = new DataStoreSelector(new[] { source1, source2, source3, source4 });

            Assert.Equal(Strings.MultipleDataStoresConfigured("'DataStore1' 'DataStore2' 'DataStore4' "),
                Assert.Throws<InvalidOperationException>(
                    () => selector.SelectDataStore(DbContextServices.ServiceProviderSource.Explicit)).Message);
        }

        [Fact]
        public void Throws_if_no_store_services_have_been_registered_using_external_service_provider()
        {
            var selector = new DataStoreSelector(null);

            Assert.Equal(Strings.NoDataStoreService,
                Assert.Throws<InvalidOperationException>(
                    () => selector.SelectDataStore(DbContextServices.ServiceProviderSource.Explicit)).Message);
        }

        [Fact]
        public void Throws_if_no_store_services_have_been_registered_using_implicit_service_provider()
        {
            var selector = new DataStoreSelector(null);

            Assert.Equal(Strings.NoDataStoreConfigured,
                Assert.Throws<InvalidOperationException>(
                    () => selector.SelectDataStore(DbContextServices.ServiceProviderSource.Implicit)).Message);
        }

        [Fact]
        public void Throws_if_multiple_store_services_are_registered_but_none_are_configured()
        {
            var source1 = CreateSource("DataStore1", configured: false, available: true);
            var source2 = CreateSource("DataStore2", configured: false, available: false);
            var source3 = CreateSource("DataStore3", configured: false, available: false);

            var selector = new DataStoreSelector(new[] { source1, source2, source3 });

            Assert.Equal(Strings.MultipleDataStoresAvailable("'DataStore1' 'DataStore2' 'DataStore3' "),
                Assert.Throws<InvalidOperationException>(
                    () => selector.SelectDataStore(DbContextServices.ServiceProviderSource.Explicit)).Message);
        }

        [Fact]
        public void Throws_if_one_store_service_is_registered_but_not_configured_and_cannot_be_used_without_configuration()
        {
            var source = CreateSource("DataStore1", configured: false, available: false);

            var selector = new DataStoreSelector(new[] { source });

            Assert.Equal(Strings.NoDataStoreConfigured,
                Assert.Throws<InvalidOperationException>(
                    () => selector.SelectDataStore(DbContextServices.ServiceProviderSource.Explicit)).Message);
        }

        [Fact]
        public void Selects_single_available_store()
        {
            var services = Mock.Of<IDataStoreServices>();
            var source = CreateSource("DataStore1", configured: false, available: true, services: services);

            var selector = new DataStoreSelector(new[] { source });

            Assert.Same(services, selector.SelectDataStore(DbContextServices.ServiceProviderSource.Explicit));
        }

        private static IDataStoreSource CreateSource(string name, bool configured, bool available, IDataStoreServices services = null)
        {
            var sourceMock = new Mock<IDataStoreSource>();
            sourceMock.Setup(m => m.IsConfigured).Returns(configured);
            sourceMock.Setup(m => m.IsAvailable).Returns(available);
            sourceMock.Setup(m => m.StoreServices).Returns(services);
            sourceMock.Setup(m => m.Name).Returns(name);
            sourceMock.Setup(m => m.ContextOptions).Returns(new DbContextOptions());

            return sourceMock.Object;
        }
    }
}
