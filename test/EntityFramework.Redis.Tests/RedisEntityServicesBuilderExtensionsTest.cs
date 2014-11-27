// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.Redis.Tests
{
    public class RedisEntityServicesBuilderExtensionsTest
    {
        [Fact]
        public void AddRedis_does_not_replace_services_already_registered()
        {
            var services = new ServiceCollection()
                .AddSingleton<RedisDataStore, FakeRedisDataStore>();

            services.AddEntityFramework().AddRedis();

            var serviceProvider = services.BuildServiceProvider();

            Assert.IsType<FakeRedisDataStore>(serviceProvider.GetRequiredService<RedisDataStore>());
        }

        private class FakeRedisDataStore : RedisDataStore
        {
        }
    }
}
