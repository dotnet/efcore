// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Redis.Tests
{
    public class RedisDataStoreCreatorTests
    {
        [Fact]
        public void Ensure_creation()
        {
            var model = Mock.Of<IModel>();
            var configurationMock = new Mock<DbContextServices>();

            var creator = new RedisDataStoreCreator(configurationMock.Object);
            Assert.False(creator.EnsureCreated(model));
        }

        [Fact]
        public void Ensure_creation_async()
        {
            var model = Mock.Of<IModel>();
            var configurationMock = new Mock<DbContextServices>();

            var creator = new RedisDataStoreCreator(configurationMock.Object);
            Assert.False(creator.EnsureCreatedAsync(model).Result);
        }

        [Fact]
        public void Ensure_deletion()
        {
            var model = Mock.Of<IModel>();
            var configurationMock = new Mock<DbContextServices>();
            var databaseMock = new Mock<RedisDatabase>();
            configurationMock.SetupGet(m => m.Database).Returns(databaseMock.Object);

            var creator = new RedisDataStoreCreator(configurationMock.Object);
            Assert.True(creator.EnsureDeleted(model));
            databaseMock.Verify(m => m.FlushDatabase(), Times.Once);
        }

        [Fact]
        public void Ensure_deletion_async()
        {
            var model = Mock.Of<IModel>();
            var configurationMock = new Mock<DbContextServices>();
            var databaseMock = new Mock<RedisDatabase>();
            configurationMock.SetupGet(m => m.Database).Returns(databaseMock.Object);

            var creator = new RedisDataStoreCreator(configurationMock.Object);
            Assert.True(creator.EnsureDeletedAsync(model).Result);
            databaseMock.Verify(m => m.FlushDatabaseAsync(CancellationToken.None), Times.Once);
        }
    }
}
