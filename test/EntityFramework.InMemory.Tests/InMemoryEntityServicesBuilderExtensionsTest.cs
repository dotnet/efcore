// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryEntityServicesBuilderExtensionsTest
    {
        [Fact]
        public void Can_get_default_services()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework().AddInMemoryStore();

            // In memory dingletones
            Assert.True(services.Any(sd => sd.ServiceType == typeof(InMemoryValueGeneratorCache)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(InMemoryValueGeneratorSelector)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(SimpleValueGeneratorFactory<InMemoryValueGenerator>)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(InMemoryDatabase)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(InMemoryModelSource)));

            // In memory scoped
            Assert.True(services.Any(sd => sd.ServiceType == typeof(DataStoreSource)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(InMemoryDataStoreServices)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(InMemoryDatabaseFacade)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(InMemoryDataStore)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(InMemoryConnection)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(InMemoryDataStoreCreator)));
        }

        [Fact]
        public void Services_wire_up_correctly()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();
            using (var context = InMemoryTestHelpers.Instance.CreateContext(serviceProvider))
            {
                var scopedProvider = ((IAccessor<IServiceProvider>)context).Service;

                var inMemoryValueGeneratorCache = serviceProvider.GetRequiredService<InMemoryValueGeneratorCache>();
                var inMemoryValueGeneratorSelector = serviceProvider.GetRequiredService<InMemoryValueGeneratorSelector>();
                var simpleValueGeneratorFactory = serviceProvider.GetRequiredService<SimpleValueGeneratorFactory<InMemoryValueGenerator>>();
                var inMemoryDatabase = serviceProvider.GetRequiredService<InMemoryDatabase>();
                var inMemoryModelSource = serviceProvider.GetRequiredService<InMemoryModelSource>();

                var dataStoreSource = scopedProvider.GetRequiredService<DataStoreSource>() as InMemoryDataStoreSource;
                var inMemoryDataStoreServices = scopedProvider.GetRequiredService<InMemoryDataStoreServices>();
                var inMemoryDatabaseFacade = scopedProvider.GetRequiredService<InMemoryDatabaseFacade>();
                var inMemoryDataStore = scopedProvider.GetRequiredService<InMemoryDataStore>();
                var inMemoryConnection = scopedProvider.GetRequiredService<InMemoryConnection>();
                var inMemoryDataStoreCreator = scopedProvider.GetRequiredService<InMemoryDataStoreCreator>();

                Assert.NotNull(inMemoryValueGeneratorCache);
                Assert.NotNull(inMemoryValueGeneratorSelector);
                Assert.NotNull(simpleValueGeneratorFactory);
                Assert.NotNull(inMemoryDatabase);
                Assert.NotNull(inMemoryModelSource);

                Assert.NotNull(dataStoreSource);
                Assert.NotNull(inMemoryDataStoreServices);
                Assert.NotNull(inMemoryDatabaseFacade);
                Assert.NotNull(inMemoryDataStore);
                Assert.NotNull(inMemoryConnection);
                Assert.NotNull(inMemoryDataStoreCreator);

                // Dingletons
                Assert.Same(inMemoryValueGeneratorCache, serviceProvider.GetRequiredService<InMemoryValueGeneratorCache>());
                Assert.Same(inMemoryValueGeneratorSelector, serviceProvider.GetRequiredService<InMemoryValueGeneratorSelector>());
                Assert.Same(simpleValueGeneratorFactory, serviceProvider.GetRequiredService<SimpleValueGeneratorFactory<InMemoryValueGenerator>>());
                Assert.Same(inMemoryDatabase, serviceProvider.GetRequiredService<InMemoryDatabase>());
                Assert.Same(inMemoryModelSource, serviceProvider.GetRequiredService<InMemoryModelSource>());

                // Scoped
                Assert.Same(dataStoreSource, scopedProvider.GetRequiredService<DataStoreSource>());
                Assert.Same(inMemoryDataStoreServices, scopedProvider.GetRequiredService<InMemoryDataStoreServices>());
                Assert.Same(inMemoryDatabaseFacade, scopedProvider.GetRequiredService<InMemoryDatabaseFacade>());
                Assert.Same(inMemoryDataStore, scopedProvider.GetRequiredService<InMemoryDataStore>());
                Assert.Same(inMemoryConnection, scopedProvider.GetRequiredService<InMemoryConnection>());
                Assert.Same(inMemoryDataStoreCreator, scopedProvider.GetRequiredService<InMemoryDataStoreCreator>());

                using (var secondContext = InMemoryTestHelpers.Instance.CreateContext(serviceProvider))
                {
                    scopedProvider = ((IAccessor<IServiceProvider>)secondContext).Service;

                    Assert.NotSame(dataStoreSource, scopedProvider.GetRequiredService<DataStoreSource>());
                    Assert.NotSame(inMemoryDataStoreServices, scopedProvider.GetRequiredService<InMemoryDataStoreServices>());
                    Assert.NotSame(inMemoryDatabaseFacade, scopedProvider.GetRequiredService<InMemoryDatabaseFacade>());
                    Assert.NotSame(inMemoryDataStore, scopedProvider.GetRequiredService<InMemoryDataStore>());
                    Assert.NotSame(inMemoryConnection, scopedProvider.GetRequiredService<InMemoryConnection>());
                    Assert.NotSame(inMemoryDataStoreCreator, scopedProvider.GetRequiredService<InMemoryDataStoreCreator>());
                }
            }
        }

        [Fact]
        public void AddInMemoryStore_does_not_replace_services_already_registered()
        {
            var services = new ServiceCollection()
                .AddSingleton<InMemoryDataStore, FakeInMemoryDataStore>();

            services.AddEntityFramework().AddInMemoryStore();

            var serviceProvider = services.BuildServiceProvider();

            Assert.IsType<FakeInMemoryDataStore>(serviceProvider.GetRequiredService<InMemoryDataStore>());
        }

        private class FakeInMemoryDataStore : InMemoryDataStore
        {
        }
    }
}
