// Copyright (c) .NET Foundation. All rights reserved.
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
            var source = new TestDataStoreSource1(configured: true, services: services);

            var selector = new DataStoreSelector(Mock.Of<IServiceProvider>(), Mock.Of<IDbContextOptions>(), new[] { source });

            Assert.Same(services, selector.SelectDataStore(ServiceProviderSource.Explicit));
        }

        [Fact]
        public void Selects_single_configured_store_with_duplicates()
        {
            var services = Mock.Of<IDataStoreServices>();
            var source1 = new TestDataStoreSource1(configured: true, services: services);
            var source2 = new TestDataStoreSource1(configured: true, services: services);

            var selector = new DataStoreSelector(
                Mock.Of<IServiceProvider>(),
                Mock.Of<IDbContextOptions>(),
                new IDataStoreSource[] { source1, source2 });

            Assert.Same(services, selector.SelectDataStore(ServiceProviderSource.Explicit));
        }

        [Fact]
        public void Throws_if_multiple_stores_configured()
        {
            var source1 = new TestDataStoreSource1(configured: true);
            var source2 = new TestDataStoreSource2(configured: true);
            var source3 = new TestDataStoreSource3(configured: false);
            var source4 = new TestDataStoreSource4(configured: true);

            var selector = new DataStoreSelector(
                Mock.Of<IServiceProvider>(),
                Mock.Of<IDbContextOptions>(),
                new IDataStoreSource[] { source1, source2, source3, source4 });

            Assert.Equal(Strings.MultipleDataStoresConfigured("'DataStore1' 'DataStore2' 'DataStore4' "),
                Assert.Throws<InvalidOperationException>(
                    () => selector.SelectDataStore(ServiceProviderSource.Explicit)).Message);
        }

        [Fact]
        public void Throws_if_no_store_services_have_been_registered_using_external_service_provider()
        {
            var selector = new DataStoreSelector(
                Mock.Of<IServiceProvider>(),
                Mock.Of<IDbContextOptions>(),
                null);

            Assert.Equal(Strings.NoDataStoreService,
                Assert.Throws<InvalidOperationException>(
                    () => selector.SelectDataStore(ServiceProviderSource.Explicit)).Message);
        }

        [Fact]
        public void Throws_if_no_store_services_have_been_registered_using_implicit_service_provider()
        {
            var selector = new DataStoreSelector(
                Mock.Of<IServiceProvider>(),
                Mock.Of<IDbContextOptions>(),
                null);

            Assert.Equal(Strings.NoDataStoreConfigured,
                Assert.Throws<InvalidOperationException>(
                    () => selector.SelectDataStore(ServiceProviderSource.Implicit)).Message);
        }

        [Fact]
        public void Throws_if_multiple_store_services_are_registered_but_none_are_configured()
        {
            var source1 = new TestDataStoreSource1(configured: false);
            var source2 = new TestDataStoreSource2(configured: false);
            var source3 = new TestDataStoreSource3(configured: false);

            var selector = new DataStoreSelector(
                Mock.Of<IServiceProvider>(),
                Mock.Of<IDbContextOptions>(),
                new IDataStoreSource[] { source1, source2, source3 });

            Assert.Equal(Strings.MultipleDataStoresAvailable("'DataStore1' 'DataStore2' 'DataStore3' "),
                Assert.Throws<InvalidOperationException>(
                    () => selector.SelectDataStore(ServiceProviderSource.Explicit)).Message);
        }

        [Fact]
        public void Throws_if_one_store_service_is_registered_but_not_configured_and_cannot_be_used_without_configuration()
        {
            var source = new TestDataStoreSource1(configured: false);

            var selector = new DataStoreSelector(
                Mock.Of<IServiceProvider>(),
                Mock.Of<IDbContextOptions>(),
                new[] { source });

            Assert.Equal(Strings.NoDataStoreConfigured,
                Assert.Throws<InvalidOperationException>(
                    () => selector.SelectDataStore(ServiceProviderSource.Explicit)).Message);
        }

        private abstract class TestDataStoreSource : IDataStoreSource
        {
            private readonly bool _isConfigured;
            private readonly IDataStoreServices _services;

            public TestDataStoreSource(bool configured, IDataStoreServices services)
            {
                _isConfigured = configured;
                _services = services;
            }

            public bool IsConfigured(IDbContextOptions options) => _isConfigured;
            public IDataStoreServices GetStoreServices(IServiceProvider serviceProvider) => _services;
            public virtual string Name { get { throw new NotImplementedException(); } }
            public void AutoConfigure(DbContextOptionsBuilder optionsBuilder) { }
        }

        private class TestDataStoreSource1 : TestDataStoreSource
        {
            public TestDataStoreSource1(bool configured, IDataStoreServices services = null)
                : base(configured, services)
            {
            }

            public override string Name => "DataStore1";
        }

        private class TestDataStoreSource2 : TestDataStoreSource
        {
            public TestDataStoreSource2(bool configured, IDataStoreServices services = null)
                : base(configured, services)
            {
            }

            public override string Name => "DataStore2";
        }

        private class TestDataStoreSource3 : TestDataStoreSource
        {
            public TestDataStoreSource3(bool configured, IDataStoreServices services = null)
                : base(configured, services)
            {
            }

            public override string Name => "DataStore3";
        }

        private class TestDataStoreSource4 : TestDataStoreSource
        {
            public TestDataStoreSource4(bool configured, IDataStoreServices services = null)
                : base(configured, services)
            {
            }

            public override string Name => "DataStore4";
        }
    }
}
