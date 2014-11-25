// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests.TestModels;
using Microsoft.Data.Entity.Sqlite.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public class SqLiteCrossStoreFixture : CrossStoreFixture<SqLiteTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public SqLiteCrossStoreFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlite()
                .AddSqlServer()
                .AddAzureTableStorage()
                .AddInMemoryStore()
                .AddRedis()
                .ServiceCollection
                .BuildServiceProvider();
        }

        public override SqLiteTestStore CreateTestStore()
        {
            var testStore = SqLiteTestStore.CreateScratchAsync().Result;
            using (var context = CreateContext(testStore))
            {
                context.Database.EnsureCreated();
            }

            return testStore;
        }

        public override CrossStoreContext CreateContext(SqLiteTestStore testStore)
        {
            var options = new DbContextOptions()
                .UseSqlite(testStore.Connection.ConnectionString);

            return new CrossStoreContext(_serviceProvider, options);
        }
    }
}
