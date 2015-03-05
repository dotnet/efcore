// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using EntityFramework.Redis.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Redis.Extensions;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Redis.FunctionalTests
{
    public class RedisBuiltInDataTypesFixture : BuiltInDataTypesFixtureBase<RedisTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public RedisBuiltInDataTypesFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddRedis()
                .ServiceCollection
                .AddTestModelSource(OnModelCreating)
                .BuildServiceProvider();
        }
        public override DbContext CreateContext(RedisTestStore testStore)
        {
            var options = new DbContextOptions()
                .UseRedis("127.0.0.1", RedisTestConfig.RedisPort);

            return new DbContext(options);
        }

        public override RedisTestStore CreateTestStore()
        {
            return new RedisTestStore();
        }

        public override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

    }
}
