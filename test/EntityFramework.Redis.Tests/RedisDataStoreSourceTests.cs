// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Redis
{
    public class RedisDataStoreSourceTests
    {
        [Fact]
        public void Available_when_configured()
        {
            var configuration = new DbContextConfiguration();
            configuration.Initialize(
                Mock.Of<IServiceProvider>(),
                Mock.Of<IServiceProvider>(),
                new DbContextOptions(),
                Mock.Of<DbContext>(),
                DbContextConfiguration.ServiceProviderSource.Implicit);

            var dataStoreSource = new RedisDataStoreSource();
            Assert.False(dataStoreSource.IsAvailable(configuration));

            configuration.ContextOptions.AddExtension(new RedisOptionsExtension());

            Assert.True(dataStoreSource.IsAvailable(configuration));
        }

        [Fact]
        public void Named_correctly()
        {
            Assert.Equal(typeof(RedisDataStore).Name, new RedisDataStoreSource().Name);
        }
    }
}
