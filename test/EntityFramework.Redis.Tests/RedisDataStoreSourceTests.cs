// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Redis.Tests
{
    public class RedisDataStoreSourceTests
    {
        [Fact]
        public void Available_when_configured()
        {
            IDbContextOptions options = new DbContextOptions();
            options.AddOrUpdateExtension<RedisOptionsExtension>(e => { });

            var configurationMock = new Mock<DbContextServices>();
            configurationMock.Setup(m => m.ContextOptions).Returns(options);

            Assert.True(new RedisDataStoreSource(configurationMock.Object, new DbContextService<IDbContextOptions>(options)).IsAvailable);
        }

        [Fact]
        public void Named_correctly()
        {
            Assert.Equal(
                typeof(RedisDataStore).Name,
                new RedisDataStoreSource(Mock.Of<DbContextServices>(), new DbContextService<IDbContextOptions>(() => null)).Name);
        }
    }
}
