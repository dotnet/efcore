// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.AzureTableStorage.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public class AtsCrossStoreFixture : CrossStoreFixture<AtsTestStore>, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;

        public AtsCrossStoreFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddAzureTableStorage()
                .AddInMemoryStore()
                .AddRedis()
                .AddSqlite()
                .AddSqlServer()
                .ServiceCollection
                .BuildServiceProvider();
        }

        public override AtsTestStore CreateTestStore()
        {
            var store = new AtsTestStore(CrossStoreContext.AtsTableSuffix);
            using (var context = CreateContext(store))
            {
                context.Database.EnsureCreated();
            }

            store.CleanupAction = () =>
                {
                    using (var context = CreateContext(store))
                    {
                        CrossStoreContext.RemoveAllEntities(context);
                        context.SaveChanges();
                    }
                };

            return store;
        }

        public override CrossStoreContext CreateContext(AtsTestStore testStore)
        {
            var options = new DbContextOptions()
                .UseAzureTableStorage(testStore.ConnectionString);

            return new CrossStoreContext(_serviceProvider, options);
        }

        public void Dispose()
        {
            var testStore = CreateTestStore();
            using (var context = CreateContext(testStore))
            {
                context.Database.EnsureDeleted();
            }
        }
    }
}
