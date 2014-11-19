// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Redis.Tests
{
    public class RedisDataStoreSourceTests
    {
        [Fact]
        public void Available_when_configured()
        {
            var config = new DbContextConfiguration();
            var options = new DbContextOptions();
            config.Initialize(
                Mock.Of<IServiceProvider>(),
                options,
                Mock.Of<DbContext>(),
                DbContextConfiguration.ServiceProviderSource.Implicit);

            var source = new RedisDataStoreSource(config, new LazyRef<IDbContextOptions>(options));

            Assert.False(source.IsAvailable);

            config.ContextOptions.AddExtension(new RedisOptionsExtension());

            Assert.True(source.IsAvailable);
        }

        [Fact]
        public void Named_correctly()
        {
            Assert.Equal(
                typeof(RedisDataStore).Name,
                new RedisDataStoreSource(Mock.Of<DbContextConfiguration>(), new LazyRef<IDbContextOptions>(() => null)).Name);
        }
    }
}
