// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.InMemory.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public class InMemoryCrossStoreFixture : CrossStoreFixture<InMemoryTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public InMemoryCrossStoreFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddInMemoryStore()
                .AddRedis()
                .AddSQLite()
                .AddSqlServer()
                .AddAzureTableStorage()
                .ServiceCollection
                .BuildServiceProvider();
        }

        public override InMemoryTestStore CreateTestStore()
        {
            return new InMemoryTestStore();
        }

        public override CrossStoreContext CreateContext(InMemoryTestStore testStore)
        {
            var options = new DbContextOptions()
                .UseInMemoryStore();

            return new CrossStoreContext(_serviceProvider, options);
        }
    }
}
