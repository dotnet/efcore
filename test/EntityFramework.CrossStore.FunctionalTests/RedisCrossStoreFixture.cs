// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Redis.Extensions;
using Microsoft.Data.Entity.Redis.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public class RedisCrossStoreFixture : CrossStoreFixture<RedisTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public RedisCrossStoreFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddRedis()
                .AddSqlite()
                .AddSqlServer()
                .AddAzureTableStorage()
                .AddInMemoryStore()
                .ServiceCollection
                .BuildServiceProvider();
        }

        public override RedisTestStore CreateTestStore()
        {
            var store = new RedisTestStore();
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

        public override CrossStoreContext CreateContext(RedisTestStore testStore)
        {
            var options = new DbContextOptions()
                .UseRedis("127.0.0.1", RedisTestConfig.RedisPort);

            return new CrossStoreContext(_serviceProvider, options);
        }
    }
}
